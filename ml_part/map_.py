from IPython.core.display_functions import display
from ipyleaflet import Map, Marker, Polygon
from ipywidgets import HTML


m = Map(center=(59.9311, 30.3609), zoom=15)

html_layer = HTML()
html_layer.value = "<h1>Hello, World!</h1>"

# Add the HTML layer to the map
m.add(html_layer)


m.save('map.html')
