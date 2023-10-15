import Feature from 'ol/Feature.js';
import Map from 'ol/Map.js';
import View from 'ol/View.js';
import {Circle, Polygon} from 'ol/geom.js';
import {OSM, Vector as VectorSource} from 'ol/source.js';
import {Style, Stroke, Fill} from 'ol/style.js';
import {Tile as TileLayer, Vector as VectorLayer} from 'ol/layer.js';


function latLongToWebMercator(lat, lon) {
  var x = lon * 20037508.34 / 180;
  var y = Math.log(Math.tan((90 + lat) * Math.PI / 360)) / (Math.PI / 180);
  y = y * 20037508.34 / 180;
  return [x, y];
}

var lat = 59.730365, long = 30.070735 ,style, map, circleFeature, vectorLayer; // 59.730365, 30.070735

function createFeaturesFromCoordinates(coordinates) {
  // Create an empty array to hold the new features
  var newFeatures = [];

  // Loop through each pair of coordinates
  for (var i = 0; i < coordinates.length; i++) {
      var lat = coordinates[i][0];
      var lon = coordinates[i][1];

      // Create a new feature with a Point geometry from the coordinates
      var feature = new Feature({
          geometry: new Circle(latLongToWebMercator(lat, lon), 30)
      });

      feature.setStyle( style );

      // Add the new feature to the array
      newFeatures.push(feature);
  }

  return newFeatures;
}

window.addEventListener('message', function(event) {
  console.log('Received message:', event.data);
  
  // Get the source of the VectorLayer
  var source = vectorLayer.getSource();

  // Clear all existing features
  //source.clear();
  
  const {view, features, color} = event.data;
  map.getView().setCenter(latLongToWebMercator(view[1],view[0]));

  
  addPolygonToMap(features, color);
  /*
  const mappedFeatures = createFeaturesFromCoordinates(features);
  for (var i = 0; i < mappedFeatures.length; i++) {
    source.addFeature(mappedFeatures[i]);
  }*/

  
}, false);

function addPolygonToMap(coordinates, color) {
  // Convert the coordinates from lat/lon to the map's projection
  var transformedCoordinates = coordinates.map(function(coord) {
      return latLongToWebMercator(coord[1], coord[0]);
  });

  // Create a polygon geometry from the transformed coordinates
  var polygon = new Polygon([transformedCoordinates]);

  // Create a feature from the polygon
  var feature = new Feature(polygon);

  // Create a style for the feature with the specified color
  var style = new Style({
      fill: new Fill({
          color: color
      }),
      stroke: new Stroke({
          color: 'rgba(0,0,255,1)',
          width: 2
      })
  });

  // Set the feature's style
  feature.setStyle(style);

  // Create a vector source and add the feature to it
  var source = new VectorSource({
      features: [feature]
  });

  // Create a vector layer from the source
  var layer = new VectorLayer({
      source: source
  });

  // Add the layer to the map
  map.addLayer(layer);
}




circleFeature = new Feature({
  geometry: new Circle(latLongToWebMercator(lat, long), 30),
});

style = new Style({
  renderer(coordinates, state) {
    const [[x, y], [x1, y1]] = coordinates;
    const ctx = state.context;
    const dx = x1 - x;
    const dy = y1 - y;
    const radius = Math.sqrt(dx * dx + dy * dy);

    const innerRadius = 0;
    const outerRadius = radius * 1.4;

    const gradient = ctx.createRadialGradient(
      x,
      y,
      innerRadius,
      x,
      y,
      outerRadius
    );
    gradient.addColorStop(0, 'rgba(0,255,0,0)');
    gradient.addColorStop(0.6, 'rgba(0,255,0,0.2)');
    gradient.addColorStop(1, 'rgba(0,255,0,0.8)');
    ctx.beginPath();
    ctx.arc(x, y, radius, 0, 2 * Math.PI, true);
    ctx.fillStyle = gradient;
    ctx.fill();

    ctx.arc(x, y, radius, 0, 2 * Math.PI, true);
    ctx.strokeStyle = 'rgba(0,255,0,1)';
    ctx.stroke();
  },
})
circleFeature.setStyle( style );

vectorLayer = new VectorLayer({
  source: new VectorSource({
    //features: [circleFeature],
  }),
})

map = new Map({
  layers: [
    new TileLayer({
      source: new OSM(),
      visible: true,
    }),
    vectorLayer,
  ],
  target: 'map',
  view: new View({
    center: latLongToWebMercator(lat, long),
    zoom: 15,
  }),
});
