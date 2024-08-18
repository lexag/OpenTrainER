import json
import argparse
from pprint import pprint
import types
import math
import numpy as np

parser = argparse.ArgumentParser(
					prog='OpenTrainER CLI Tool - osm2json',
					description='Converts osm json files to OpenTrainER track.json files')

parser.add_argument('filename')
parser.add_argument('output')
parser.add_argument('origin_lat', type=float)
parser.add_argument('origin_lon', type=float)
args = parser.parse_args()

# -- UTIL ---

def normalize(v):
	norm = np.linalg.norm(v)
	if norm == 0: 
		return v
	return v / norm



# -- CODE START --

num_points = 0
num_links = 0
total_distance = 0

region_data = {}
region_data["points"] = {}
region_data["origin"] = [float(args.origin_lat), float(args.origin_lon)]

with open(args.filename, 'r') as f:
	osm_data = json.load(f)
	for element in osm_data["elements"]:
		for i, node in enumerate(element["nodes"]):
			lat = element["geometry"][i]["lat"]
			lon = element["geometry"][i]["lon"]
			
			# place point in space
			point_object = {}
			point_object["xoffset"] = -(lon - args.origin_lon)/((1 / ((2 * math.pi / 360) * 6378.137)) / 1000 / math.cos(lat * (math.pi / 180)))
			point_object["yoffset"] = (lat - args.origin_lat)/((1 / ((2 * math.pi / 360) * 6378.137)) / 1000)
			region_data["points"][node] = (point_object)
			point_object["linked_nodes"] = {}
			num_points += 1
	
	# find neighbours
	for element in osm_data["elements"]:
		for i, node in enumerate(element["nodes"]):
			point_object = region_data["points"][node]
			# consider previous point and next point in current way 
			for flag, idx_offset in [(i < len(element["nodes"])-1, 1), (i > 0, -1)]:
				if flag:
					neighbour_idx = element["nodes"][i+idx_offset]
					pos_diff = np.array(
								[region_data["points"][neighbour_idx]["xoffset"]-point_object["xoffset"], 
									region_data["points"][neighbour_idx]["yoffset"]-point_object["yoffset"]])
					point_object["linked_nodes"][neighbour_idx] = {
						"direction": normalize(pos_diff).tolist(),
						"distance": np.linalg.norm(pos_diff),
						"tags": {x: element["tags"][x] for x in element["tags"]}
						}
					total_distance += np.linalg.norm(pos_diff)
					num_links += 1


# calculate tangent for each node
for point_id in region_data["points"]:
	point = region_data["points"][point_id]
	## Consider each neighbour direction
	## Flip to between 0 -> pi (eg. 30deg and 210 deg are the same tangent direction)
	## Average all neigbour directions => tangent direction
	dir_sum = np.array([0.0, 0.0])
	for neighbour in point["linked_nodes"]:
		direction = point["linked_nodes"][neighbour]["direction"].copy()
		if direction[1] < 0:
			direction *= np.array([-1, -1])
		dir_sum += direction
	tangent = normalize(dir_sum)
	point["tangent"] = [tangent[0], tangent[1]]


	
with open(args.output, "w") as f:
	f.write(json.dumps(region_data, indent=4))
print(f"Wrote {num_points} points, {num_links} links with length {round(total_distance, 1)} m to {args.output}")