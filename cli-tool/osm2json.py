import json
import argparse
from pprint import pprint
import types
import math
import numpy as np
import re

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

def NodeHasKeyValue(node, key, val):
	return key in node["tags"] and node["tags"][key] == val

def NodeHasKeyValueRegex(node, key, val):
	for k in node["tags"]:
		if re.search(key, k) and re.search(val, node["tags"][k]):
			return True
	return False

def NodeHasKeyRegex(node, key):
	for k in node["tags"]:
		r = re.search(key, k)
		if r:
			return True
	return False

def CreateCrudePointObject(id, lat, lon):
	return CreatePointObject({
		"type": "node",
		"id": id,
		"lat": lat,
		"lon": lon,
		"tags": {}
	})

def CreatePointObject(node):
	lat = node["lat"]
	lon = node["lon"]
	
	point_object = {}
	point_object["linked_nodes"] = {}
	point_object["position"] = [-(lon - args.origin_lon)/((1 / ((2 * math.pi / 360) * 6378.137)) / 1000 / math.cos(lat * (math.pi / 180))), 
								(lat - args.origin_lat)/((1 / ((2 * math.pi / 360) * 6378.137)) / 1000)]
	
	point_feature = {}
	if NodeHasKeyValue(node, "railway", "signal") and NodeHasKeyValueRegex(node, "railway:signal:\w+:form", "light"):
		point_feature = {
			"feature_type": "signal",
			"id": "0" if "ref" not in node["tags"] else node["tags"]["ref"],
			"atc": "railway:atc" in node["tags"] and node["tags"]["railway:atc"] == "yes",
			"osm_position": node["tags"]["railway:signal:position"],
			"osm_direction": 1 if node["tags"]["railway:signal:direction"] == "forward" else -1,
		}
	elif NodeHasKeyValue(node, "railway", "signal") and NodeHasKeyRegex(node, "railway:signal:speed_limit"):
		point_feature = {
			"feature_type": "speed_sign",
			"speed": node["tags"]["railway:signal:speed_limit"],
			"atc": "railway:atc" in node["tags"] and node["tags"]["railway:atc"] == "yes",
			"osm_position": node["tags"]["railway:signal:position"],
			"osm_direction": 1 if node["tags"]["railway:signal:direction"] == "forward" else -1,
		}
	point_object["feature"] = point_feature
	
	return point_object



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
		if (element["type"] == "node"):
			obj = CreatePointObject(element)
			region_data["points"][str(element["id"])] = obj
			num_points += 1
		
		elif (element["type"] == "way"):
			for i, node in enumerate(element["nodes"]):
				if str(node) not in region_data["points"]:
					region_data["points"][str(node)] = CreateCrudePointObject(node, element["geometry"][i]["lat"], element["geometry"][i]["lon"])
					num_points += 1


	# find neighbours
	for element in osm_data["elements"]:
		if (element["type"] != "way") : continue
		# consider previous point and next point in current way
		for i, node in enumerate(element["nodes"]):
			node = str(node)
			for flag, idx_offset in [(i < len(element["nodes"])-1, 1), (i > 0, -1)]:
				point_object = region_data["points"][node]
				if flag:
					neighbour_idx = str(element["nodes"][i+idx_offset])
					pos_diff = np.array(
								[region_data["points"][neighbour_idx]["position"][0]-point_object["position"][0], 
									region_data["points"][neighbour_idx]["position"][1]-point_object["position"][1]])
					point_object["linked_nodes"][neighbour_idx] = {
						"osm_way_direction": idx_offset,
						"direction": normalize(pos_diff).tolist(),
						"distance": np.linalg.norm(pos_diff),
						"gauge": int(element["tags"]["gauge"]) or 1435,
						# "tags": {x: element["tags"][x] for x in element["tags"]}
						}
					total_distance += np.linalg.norm(pos_diff)
					num_links += 1


for point_id in region_data["points"]:
	point = region_data["points"][point_id]
	# calculate tangent
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

	# calculate average direction of only 'forward' osm ways
	dirsum = np.array([0.0, 0.0])
	for neighbour in point["linked_nodes"]:
		if point["linked_nodes"][neighbour]["osm_way_direction"] == 1:
			dirsum += np.array(point["linked_nodes"][neighbour]["direction"]) * float(point["linked_nodes"][neighbour]["distance"])
	point["osm_forward_vector"] = normalize(dirsum).tolist()

	# calculate feature position
	if point["feature"] != {}:
		ftr = point["feature"]
		if ftr["feature_type"] in ["signal", "speed_sign", "sign"]:
			ftr["direction"] = np.array(point["osm_forward_vector"]) * ftr["osm_direction"]
			ftr["direction"] = ftr["direction"].tolist()
			distance_from_track = 3
			ftr["position"] = np.array(point["position"]) + np.array([ftr["direction"][1], -ftr["direction"][0]] * 1 if ftr["osm_position"] == "left" else -1) * distance_from_track
			ftr["position"] = ftr["position"].tolist()

# prune nodes with no linked nodes (i.e. station markers etc, nodes not in the track)
region_data["points"] = {id: point for id, point in region_data["points"].items() if len(point["linked_nodes"]) > 0}


with open(args.output, "w") as f:
	json.dump(region_data, f, indent=2)
print(f"Wrote {num_points} points, {num_links} links with length {round(total_distance, 1)} m to {args.output}")