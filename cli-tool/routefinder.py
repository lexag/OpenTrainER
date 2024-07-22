import json
import argparse
from pprint import pprint
import numpy as np

parser = argparse.ArgumentParser(
					prog='OpenTrainER CLI Tool - routefinder',
					description='Finds a path between two points in track.json and adds it to route.json')

parser.add_argument('track_json_path')
parser.add_argument('route_json_path')
parser.add_argument('start_node')
parser.add_argument('end_node')
parser.add_argument('name')
args = parser.parse_args()

track_points = json.load(open(args.track_json_path, "r"))["points"]

# -- UTIL --

def getDistanceSquared(a, b):
	if a == None or b == None:
		return 1_000_000_000
	xdif = float(track_points[a]["xoffset"]) - float(track_points[b]["xoffset"])
	ydif = float(track_points[a]["yoffset"]) - float(track_points[b]["yoffset"])
	return xdif ** 2 + ydif ** 2

def dotProduct(pa, pb, qa, qb):
	if None in [pa, pb, qa, qb]:
		return 0
	px = float(track_points[pa]["xoffset"]) - float(track_points[pb]["xoffset"])
	py = float(track_points[pa]["yoffset"]) - float(track_points[pb]["yoffset"])
	qx = float(track_points[qa]["xoffset"]) - float(track_points[qb]["xoffset"])
	qy = float(track_points[qa]["yoffset"]) - float(track_points[qb]["yoffset"])

	return px*qx + py*qy

# -- CODE START --



path = []
visited_nodes = []

recursion_limit = 900
def recursive_step(node, parent = None, depth = 0):
	visited_nodes.append(node)
	if depth > recursion_limit:
		print(f"recursion_limit {recursion_limit} reached at {node}")
		return False
	if node == args.end_node:
		path.append(node)
		return True
	for neighbour, data in sorted(track_points[node]["linked_nodes"].items(), key=lambda elem: 0 if "maxspeed" not in elem[1]["tags"] else int(elem[1]["tags"]["maxspeed"]), reverse=True):
		dot = dotProduct(parent, node, node, neighbour)
		if neighbour == parent or neighbour in visited_nodes or dot < 0:
			continue
		if recursive_step(neighbour, node, depth + 1):
			path.append(node)
			return True

recursive_step(args.start_node)
path.reverse()

total_length = 0
for i, j in zip(path, path[1:]):
	total_length += float(track_points[i]["linked_nodes"][j]["distance"])

new_route = {
	"points": path,
	"length": total_length
}
route_data = json.load(open(args.route_json_path, "r"))
route_data["routes"][args.name] = new_route
with open(args.route_json_path, "w") as f:
    f.write(json.dumps(route_data))

print(f"Added route {args.name} (with length {total_length} m and {len(path)} points) to {args.route_json_path}")
