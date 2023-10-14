import json
import logging
import logging.config
import pathlib


def load_config():
    with open(pathlib.Path(pathlib.Path(__file__).parents[0], 'logging.json'), 'rt') as file:
        return json.load(file)


# logger initialization
logging.config.dictConfig(load_config())


def get_logger(name):
    return logging.getLogger(name)
