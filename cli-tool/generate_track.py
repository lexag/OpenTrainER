import json
import argparse
from pprint import pprint
import types
import math
import numpy as np
import re

parser = argparse.ArgumentParser(
					prog='OpenTrainER CLI Tool - generate track',
					description='Converts osm and height data to OpenTrainER track.json files')

parser.add_argument('osm_path')
parser.add_argument('height_path')
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
	point_object["position2d"] = [-(lon - args.origin_lon)/((1 / ((2 * math.pi / 360) * 6378.137)) / 1000 / math.cos(lat * (math.pi / 180))), 
								(lat - args.origin_lat)/((1 / ((2 * math.pi / 360) * 6378.137)) / 1000)]
	
	# point_object["signal"] = {}
	# if NodeHasKeyValue(node, "railway", "signal") and NodeHasKeyValueRegex(node, "railway:signal:\w+:form", "light"):
	# 	m = None
	# 	for k in node["tags"]:
	# 		if ma := re.search("railway:signal:(\w+):form", k):
	# 			m = ma
	# 			break
	# 	point_object["signal"] = {
	# 		"id": "0" if "ref" not in node["tags"] else node["tags"]["ref"],
	# 		"osm_type": m.group(1),
	# 		"speed": -1 if "railway:signal:speed_limit" not in node["tags"] else int(node["tags"]["railway:signal:speed_limit"]),
	# 		"atc": "railway:atc" in node["tags"] and node["tags"]["railway:atc"] == "yes",
	# 		"osm_position": node["tags"]["railway:signal:position"],
	# 		"osm_direction": 1 if node["tags"]["railway:signal:direction"] == "forward" else -1,
	# 	}
	
	# point_object["sign"] = {}
	# if NodeHasKeyValue(node, "railway", "signal") and NodeHasKeyRegex(node, "railway:signal:speed_limit"):
	# 	point_object["sign"] = {
	# 		"type": "speed_sign",
	# 		"speed": node["tags"]["railway:signal:speed_limit"],
	# 		"atc": "railway:atc" in node["tags"] and node["tags"]["railway:atc"] == "yes",
	# 		"osm_position": node["tags"]["railway:signal:position"],
	# 		"osm_direction": 1 if node["tags"]["railway:signal:direction"] == "forward" else -1,
	# 	}
	
	return point_object


def getDistance(points, a, b):
	if a == None or b == None:
		return 1_000_000_000
	apos = np.array(points[a]["position2d"])
	bpos = np.array(points[b]["position2d"])
	return np.linalg.norm(apos-bpos)


# -- CODE START --

num_points = 0
num_links = 0
total_distance = 0

region_data = {}
region_data["points"] = {}
region_data["origin"] = [float(args.origin_lat), float(args.origin_lon)]


with open(args.osm_path, 'r') as f:
	print("placing points")
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
								[region_data["points"][neighbour_idx]["position2d"][0]-point_object["position2d"][0], 
									region_data["points"][neighbour_idx]["position2d"][1]-point_object["position2d"][1]])
					point_object["linked_nodes"][neighbour_idx] = {
						"osm_way_direction": idx_offset,
						"direction": normalize(pos_diff).tolist(),
						"distance": np.linalg.norm(pos_diff),
						"gauge": int(element["tags"]["gauge"]) or 1435,
						"voltage": 0 if "voltage" not in element["tags"] else float(element["tags"]["voltage"]),
						"frequency": 0 if "frequency" not in element["tags"] else float(element["tags"]["frequency"]),
						"maxspeed": 40 if "maxspeed" not in element["tags"] else float(element["tags"]["maxspeed"]),
						}
					total_distance += np.linalg.norm(pos_diff)
					num_links += 1

# prune nodes with no linked nodes (i.e. station markers etc, nodes not in the track)
region_data["points"] = {id: point for id, point in region_data["points"].items() if len(point["linked_nodes"]) > 0}

# calculate heights
with open(args.height_path, "r") as f:
	print("generating heights...")
	heights = json.load(f)
	num_failed = 0
	for i, point_id in enumerate(region_data["points"]):
		if i % 100 == 0:
			print(f"progress: {i}/{num_points}")
		current_point = region_data["points"][point_id]
		closest_points = []
		
		## breadth first node search
		visited = []
		point_stack = [point_id]
		for i in range(100000):
			try:
				current_point_id = point_stack.pop(0)
			except:
				print(f"point {point_id} ran out at {current_point_id}")
				break

			if current_point_id in visited:
				continue
			for neighbour_id in region_data["points"][current_point_id]["linked_nodes"]:
				point_stack.append(neighbour_id)
			
			for height_entry in heights:
				if current_point_id == height_entry["id"]:
					closest_points.append((current_point_id, height_entry["height"]))

			
			visited.append(current_point_id)
			if len(closest_points) == 2:
				break
		
		if len(closest_points) == 0:
			height = 0
			print(f"point {point_id} found no height reference and failed")
			num_failed += 1
		elif len(closest_points) == 1:
			height = closest_points[0][1]
		else:
			pa_distance = getDistance(region_data["points"], point_id, closest_points[0][0])
			pb_distance = getDistance(region_data["points"], point_id, closest_points[1][0])
			if pa_distance < 0.0001:
				height = closest_points[0][1]
			elif pb_distance < 0.0001:
				height = closest_points[1][1]
			else:
				total = 1/pa_distance + 1/pb_distance
				a_contr = closest_points[0][1] / pa_distance
				b_contr = closest_points[1][1] / pb_distance
				height = (a_contr + b_contr) / total
			print(f"{point_id} --- {closest_points} --- {height}")
		current_point["position"] = [current_point["position2d"][0], height, current_point["position2d"][1]]
	print(f"{num_failed} points failed height interpolation")


for point_id in region_data["points"]:
	point = region_data["points"][point_id]
	#calculate distance/direction/gradient in 3d
	for neighbour_id in point["linked_nodes"]:
		link = point["linked_nodes"][neighbour_id]
		n = region_data["points"][neighbour_id]
		pos_diff = np.array(n["position"]) - np.array(point["position"])
		link["direction"] = normalize(pos_diff).tolist()
		link["distance"] = np.linalg.norm(pos_diff)
		link["gradient"] = link["direction"][1] / link["distance"]

for point_id in region_data["points"]:
	point = region_data["points"][point_id]
	# calculate average direction of only 'forward' osm ways
	dirsum = np.array([0.0, 0.0, 0.0])
	for neighbour in point["linked_nodes"]:
		if point["linked_nodes"][neighbour]["osm_way_direction"] == 1:
			dirsum += np.array(point["linked_nodes"][neighbour]["direction"]) * float(point["linked_nodes"][neighbour]["distance"])
	osm_fwd_vector = normalize(dirsum)	
 
	# calculate tangent
	dir_sum = np.array([0.0, 0.0, 0.0])
	if len(point["linked_nodes"]) == 1:
		tangent = np.array(next(iter(point["linked_nodes"].values()))["direction"])
	else:
		record = 2 * math.pi
		for neighbour_one in point["linked_nodes"]:
			v1 = np.array(point["linked_nodes"][neighbour_one]["direction"].copy())
			for neighbour_two in point["linked_nodes"]:
				if neighbour_one == neighbour_two : continue
				v2 = np.array(point["linked_nodes"][neighbour_one]["direction"].copy())
				for i in [-1, 1]:
					angle = np.arccos(np.clip(np.dot(normalize(v1), normalize(i*v2)), -1.0, 1.0))
					if abs(angle) < record:
						record = abs(angle)
						tangent = normalize(v1 + i*v2)
	tangent *= math.copysign(1, np.dot(tangent, osm_fwd_vector))
	point["tangent"] = tangent.tolist()



with open(args.output, "w") as f:
	json.dump(region_data, f, indent=2)





print(f"Wrote {num_points} points, {num_links} links with length {round(total_distance, 1)} m to {args.output}")