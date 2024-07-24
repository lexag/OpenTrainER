import json
import argparse
from pprint import pprint
import numpy
from stl import mesh

parser = argparse.ArgumentParser(
					prog='OpenTrainER CLI Tool - osm2json',
					description='Converts OpenTrainER track.json files to stl file for modeling reference')

parser.add_argument('track_json_path')
parser.add_argument('output')
args = parser.parse_args()

track_points = json.load(open(args.track_json_path, "r"))["points"]

# -- CODE START --


# Write the mesh to file "cube.stl"
cube.save('cube.stl')

VERTICE_COUNT = len(track_points)
data = numpy.zeros(VERTICE_COUNT, dtype=mesh.Mesh.dtype)
your_mesh = mesh.Mesh(data, remove_empty_areas=False)

your_mesh.save(args.output)