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
args = parser.parse_args()


# -- CODE START --

img = np.zeros(shape=(args.size,args.size,3))

track_points = json.load((Path(args.line_dir) / "track.json").open())["points"]
for point in track_points:
	pos = (
		float(track_points[point]["xoffset"]),
		float(track_points[point]["yoffset"]))
	pos = (
		int(pos[0] / args.zoom + args.size/2), 
		args.size - int(pos[1] / args.zoom + args.size/2))
	print(pos)
	cv.circle(img, pos, radius=0, color=(255, 255, 255), thickness=-1)


cv.imshow(f"Map: {args.line_dir}", img) 
cv.waitKey(0) 
cv.destroyAllWindows() 

