import json
import argparse
from pprint import pprint
import types
import math
import numpy as np
import re

parser = argparse.ArgumentParser(
					prog='OpenTrainER CLI Tool - osm2signals',
					description='Converts osm file to signals.json')

parser.add_argument('osm_path')
parser.add_argument('track_json_path')
parser.add_argument('output')
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
	
	point_object["signal"] = {}
	
	
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



# -- CODE START --

layouts = {
	"speed_limit": "None",
	"main": "Stack3",
	"combined": "Stack5",
	"distant": "Pre3",
	"shunting": "Shunt4",
}

n = 10_000

signals = {}
track_points = json.load(open(args.track_json_path, "r"))["points"]

with open(args.osm_path, 'r') as f:
	osm_data = json.load(f)
	for element in osm_data["elements"]:
		if (element["type"] == "node"):
			node = element
			if NodeHasKeyValue(node, "railway", "signal") and NodeHasKeyRegex(node, "railway:signal:\w+:form"):
				point = track_points[str(node["id"])]
				osm_type = None
				for k in node["tags"]:
					if ma := re.search("railway:signal:(\w+):form", k):
						osm_type = ma
						break
				osm_type = osm_type.group(1)

				has_ref = "ref" in node["tags"]
				if not has_ref: n+=1

				direction = (np.array(point["tangent"]) * (1 if node["tags"]["railway:signal:direction"] == "forward" else -1)).tolist()
				signals[str(node["id"])] = {
					"id": n if not has_ref else node["tags"]["ref"],
					"layout": layouts[osm_type],
					"speed": -1 if "railway:signal:speed_limit" not in node["tags"] else int(node["tags"]["railway:signal:speed_limit"]),
					"atc": "railway:atc" in node["tags"] and node["tags"]["railway:atc"] == "yes",
					"direction": direction,
					"offset": (np.array([direction[1], -direction[0]]) * (1 if node["tags"]["railway:signal:position"] == "right" else -1)).tolist()
				}


with open(args.output, "w") as f:
	json.dump({"signals": signals}, f, indent=2)

print(f"Wrote {len(signals)} signals to {args.output}")