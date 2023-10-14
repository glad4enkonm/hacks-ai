from pathlib import Path
from flask import Flask, render_template
from ipyleaflet import Map, Marker
from flask import Flask
from ipywidgets.embed import embed_minimal_html

from download_data import load_district_json
from download_data import load_district_json
from utils import load_json, project_root
from ipywidgets import HTML

app = Flask(__name__)

cities_data = load_json('cities_data', Path(project_root))


@app.route("/")
def index():
    m = Map(center=(59.9311, 30.3609), zoom=15)

    # marker = Marker(location=[37.7749, -122.4194])
    # m.add(marker)
    embed_minimal_html('templates/index.html', views=[m], title='Map')
    return render_template('index.html')


@app.route("/<district>")
def get_district_coords(district):
    if district in cities_data['spb']['districts']:
        return load_district_json(district)
    return {}

#app.run(port=5001)