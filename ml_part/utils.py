import json
from pathlib import Path
import pandas as pd
import yaml

project_root = Path(Path(__file__).parents[0])

data_path = Path(project_root, 'data')

def read_yaml(file):
    with open(Path(project_root, file)) as f:
        return yaml.load(f, Loader=yaml.SafeLoader)


def to_pkl(df, name):
    df.to_pickle(Path(data_path, f"{name}.pkl"))


def load_pkl(name):
    return pd.read_pickle(Path(data_path, f"{name}.pkl"))


def to_json(name, data):
    with open(Path(data_path,  f"{name}.json"), 'w') as f:
        json.dump(data, f)


def load_json(name, path=None):
    path = Path(data_path, f"{name}.json") if path is None else Path(path, f'{name}.json')
    with open(path, 'r') as f:
        return json.load(f)
