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
	apos = np.array(track_points[a]["position"])
	bpos = np.array(track_points[b]["position"])
	# xdif = float(track_points[a]["xoffset"]) - float(track_points[b]["xoffset"])
	# ydif = float(track_points[a]["yoffset"]) - float(track_points[b]["yoffset"])
	return np.linalg.norm(apos-bpos)

def dotProduct(pa, pb, qa, qb):
	if None in [pa, pb, qa, qb]:
		return 0
	papos = np.array(track_points[pa]["position"])
	pbpos = np.array(track_points[pb]["position"])
	qapos = np.array(track_points[qa]["position"])
	qbpos = np.array(track_points[qb]["position"])

	return np.dot(papos - pbpos, qapos - qbpos)

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
	for neighbour, data in sorted(track_points[node]["linked_nodes"].items(), key=lambda elem: int(elem[1]["maxspeed"]), reverse=True):
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
