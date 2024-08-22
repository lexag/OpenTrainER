import json
import argparse
import numpy as np
from pathlib import Path
import cv2 as cv

parser = argparse.ArgumentParser(
					prog='OpenTrainER CLI Tool - routefinder',
					description='Draws a route based on line and route info')

parser.add_argument('line_dir')
parser.add_argument('-s', '--size', type=int, default=512)
parser.add_argument('-z', '--zoom', type=float, default=10.0)
parser.add_argument('-c', '--center', type=str)
args = parser.parse_args()


# -- CODE START --

img = np.zeros(shape=(args.size,args.size,3))

track_points = json.load((Path(args.line_dir) / "track.json").open())["points"]
offsetx, offsety = (0, 0)
if args.center:
	offsetx = track_points[args.center]["position"][0]
	offsety = track_points[args.center]["position"][2]
for point in track_points:
	pos = (
		float(track_points[point]["position"][0] - offsetx),
		float(track_points[point]["position"][2] - offsety))
	pos = (
		args.size - int(pos[0] / args.zoom + args.size/2),
		args.size - int(pos[1] / args.zoom + args.size/2))
	print(pos)
	c = track_points[point]["position"][1] / 20
	cv.circle(img, pos, radius=2, color=(c, c, c), thickness=-1)


cv.imshow(f"Map: {args.line_dir}", img) 
cv.waitKey(0) 
cv.destroyAllWindows() 

