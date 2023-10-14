from flask import Flask
from download_data import load_district_json
from districts import districts_spb

app = Flask(__name__)

@app.route("/<district>")
def get_district_coords(district):
    if district in districts_spb:
        return load_district_json(district)
    return {}

app.run()