import json
from pathlib import Path
import pandas as pd
import yaml

project_root = Path(Path(__file__).parents[0])


def read_yaml(file):
    with open(Path(project_root, file)) as f:
        return yaml.load(f, Loader=yaml.SafeLoader)


def to_pkl(df, name):
    df.to_pickle(Path(project_root, 'data', f"{name}.pkl"))


def load_pkl(name):
    return pd.read_pickle(Path(project_root, 'data', f"{name}.pkl"))


def to_json(name, data):
    with open(Path(project_root, 'data', f"{name}.json"), 'w') as f:
        json.dump(data, f)


def load_json(name):
    with open(Path(project_root, 'data', f"{name}.json"), 'r') as f:
        return json.load(f)
