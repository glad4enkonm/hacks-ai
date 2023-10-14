from itertools import chain
from pathlib import Path
from typing import Dict

import osmnx as ox
import pandas as pd
from geopy.distance import geodesic

from utils import read_yaml, load_pkl, project_root

ox.settings.use_cache = True

pd.options.mode.chained_assignment = None

pd.set_option('display.max_columns', 25)
pd.set_option('display.max_rows', 10000)
pd.set_option('display.width', 1000)


def load_from_pkls():
    dfs = {}
    for path in Path(project_root, 'data').glob('*.pkl'):
        dfs[path.stem] = load_pkl(path.stem)
    return dfs


def get_tags(t_type):
    """
    build structure
    tags = {'amenity': ['school', 'university', '...'], 'leisure': ['park', ...]}
    """
    r = read_yaml('tags.yaml')
    if t_type in {'positive', 'negative', 'education'}:
        return r[t_type]


def get_tags_test():
    print('Positive: ', get_tags('positive'))
    print('Negative: ', get_tags('negative'))
    print('Education: ', get_tags('education'))


def get_df_by(df, tags: Dict):
    tags = set(chain(*tags.values()))  # flatten
    return df[df['type_'].isin(tags)]


def calc_first_condition():
    """
    Для районов: сумма положительных точек в муниципалитете (районе
    города) превышает отрицательные на 50%
    weight = 0.6
    """

    dfs_ = load_from_pkls()  # second

    # TODO: working on nan values

    lst = []

    for distr, df in dfs_.items():
        df: pd.DataFrame
        df_education = get_df_by(df, get_tags('education'))
        df_positive = get_df_by(df, get_tags('positive'))
        df_negative = get_df_by(df, get_tags('negative'))

        lst.append([distr, len(df_education), len(df_positive), len(df_negative)])

    result_df = pd.DataFrame(lst, columns=['Район', 'Образовательные', 'Положительные', 'Отрицательные'])
    result_df.set_index('Район', inplace=True)
    result_df['Cond(pos > 1.5 * neg)'] = result_df['Положительные'] > 1.5 * result_df['Отрицательные']
    print(result_df)

    return result_df


def calc_second_condition():
    """
       Для районов: число точек продажи алкоголя или табака или фастфуда
        удаленность от образовательных учреждений, у которых менее 100м в
        районе больше или равно 1
    """
    dfs_ = load_from_pkls()  # second

    lst = []
    for distr, df in dfs_.items():
        df: pd.DataFrame
        df_education = get_df_by(df, get_tags('education'))
        df_positive = get_df_by(df, get_tags('positive'))
        df_negative = get_df_by(df, get_tags('negative'))

        # print(df_education)

        df_negative['center'] = df_negative['geometry'].apply(lambda a: a.centroid)

        points_negative = df_negative['center'].tolist()

        df_education['center'] = df_education['geometry'].apply(lambda a: a.centroid)

        for k in df_education[['name', 'center']].itertuples():
            education_point = k.center
            radius_meters = 150

            points_inside_circle = []
            for point in points_negative:
                distance = geodesic((education_point.y, education_point.x), (point.y, point.x)).meters
                if distance <= radius_meters:
                    points_inside_circle.append(point)
            if len(points_inside_circle) > 0:
                lst.append((distr, k.name, len(points_inside_circle)))
    result_df = pd.DataFrame(lst, columns=['Район', 'Образовательное уч.', 'Neg points'])
    result_df.set_index('Район', inplace=True)
    print(result_df)
    return result_df


if __name__ == '__main__':
    calc_first_condition()
    calc_second_condition()
