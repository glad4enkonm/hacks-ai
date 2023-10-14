import json
from collections import defaultdict
from typing import Dict
import osmnx as ox
import pandas as pd
from osmnx._errors import InsufficientResponseError

from districts import districts_spb
from utils import read_yaml, to_pkl, to_json, load_json


def get_district_poly(district, city):
    name = district + ' район, ' + city
    boundary = ox.geocode_to_gdf(name)[["geometry", "name"]]
    return boundary['geometry'].iloc[0]


def load_tags_inside_polygon(polygon, tags: Dict):
    try:
        amenities = ox.features_from_polygon(polygon, tags=tags)
        amenities.reset_index(inplace=True)
        print(amenities.columns)
        amenities = amenities[['osmid', 'name', 'geometry', 'amenity', 'leisure']]

        df = amenities[['amenity', 'leisure']].bfill(axis=1)
        df = df[['amenity']]
        df.columns = ['type_']

        df_result = pd.concat([amenities, df], axis=1)
        df_result.drop(['amenity', 'leisure'], axis='columns', inplace=True)
        return df_result
    except InsufficientResponseError:
        pass


def save_to_geo_json(district, poly):
    geojson_dict = {"type": "Feature",
                    "properties": {},
                    "geometry": json.loads(json.dumps(poly.__geo_interface__))}
    to_json(district, geojson_dict)


def load_district_json(district):
    try:
        return load_json(district)
    except FileNotFoundError:
        return {}


def download_data(city):
    """
    dump data into data dir
    """

    # TODO: take into account difference between nodes and relations

    dfs = {}

    """
    TODO: Make function
    Make format like this:
    tags = {'amenity': ['school', 'university', 'college', 'music_school', 'pub', 'bar', 'food_court']}
    """
    tags = defaultdict(list)
    for d in read_yaml('tags.yaml').values():
        for k, v in d.items():
            tags[k].extend(v)
    tags = dict(tags)

    for district in districts_spb:
        district_poly = get_district_poly(district, city)

        df = load_tags_inside_polygon(district_poly, tags)
        dfs[district] = df

        save_to_geo_json(district, district_poly)
        to_pkl(df, district)


if __name__ == '__main__':
    download_data(city="Санкт-Петербург, Russia")
