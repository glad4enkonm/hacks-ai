from collections import defaultdict
from pathlib import Path
from typing import Dict

from osmnx._errors import InsufficientResponseError
import pandas as pd
from districts import districts_spb
import osmnx as ox
import geopandas as gpd
import matplotlib.pyplot as plt

from utils import read_yaml, to_pkl, load_pkl, project_root

ox.settings.use_cache = True

pd.set_option('display.max_columns', 25)
pd.set_option('display.max_rows', 10000)
pd.set_option('display.width', 1000)

print(*districts_spb, sep='\n')

city = "Санкт-Петербург, Russia"


# def get_boundaries_of_a_city(city):
#     city_gdf = ox.geocode_to_gdf(city)
#     print(city_gdf['geometry'].iloc[0])


def get_district_poly(district):
    name = district + ' район, ' + city
    print(name)
    # graph = ox.graph_from_place(name, network_type="all")

    boundary = ox.geocode_to_gdf(name)[["geometry", "name"]]

    return boundary['geometry'].iloc[0]


def get_amenities(polygon, tags: Dict):
    try:
        amenities = ox.features_from_polygon(polygon, tags=tags)
        amenities.reset_index(inplace=True)
        print(amenities.columns)
        return amenities[['osmid', 'name', 'geometry', 'amenity']]
    except InsufficientResponseError:
        pass


def dump_to_pkl():
    """
    dump data into data dir
    """

    # TODO: take into account difference between nodes and relations

    dfs = {}
    print(read_yaml('tags.yaml'))

    tags = {'amenity': ['school', 'university', 'college', 'music_school', 'pub']}

    for district in districts_spb[:3]:  # ! <- only 3
        poly = get_district_poly(district)
        print(poly)
        df = get_amenities(poly, tags)
        print(df)
        dfs[district] = df

        to_pkl(df, district)
    # get_boundaries_of_a_city(city)


def load_from_pkls():
    dfs = {}
    for path in Path(project_root, 'data').glob('*.pkl'):
        dfs[path.stem] = load_pkl(path.stem)
    return dfs


if __name__ == '__main__':
    dump_to_pkl()  # first

    # dfs_ = load_from_pkls() # second
    # print(dfs_)
