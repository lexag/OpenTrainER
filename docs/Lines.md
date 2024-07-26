Lines are the top level geographical representation of the world in OpenTrainER, and contains all geographical data for a certain section of railway, such as Plymouth-Exeter or Stockholm-Uppsala. 

## The line directory
Each line is defined by a directory placed under `Documents/OpenTrainER/lines`. The directory structure is as follows:
```
lines/
├── <line_name>/
│   ├── routes.json
│   ├── scene.json
│   ├── scene/
│   │   ├── <area_1>
│   │   └── <area_2>
│   └── track.json
└── <line_name_2>/
    └── ...
```

### Routes
`routes.json` contain information about all created routes in the line. A route is a path with a start point, where the simulation session starts, and an end point, where the simulation session ends. 

```json
routes.json
{
  "routes": {
    "<route_name>": {
      "points": [
        "1344331359", 
        "1344331353", 
        "1506060474", 
        "9949058331",
        ...
      ]
    },
    "<route_name_2>": {
      ...
    }
  }
}
```

### Scene
The `scene` directory contains .glb-files which contain the 3d scene. The line is split into multiple areas, each one with its own scene file. Areas are arbitrarily divided for convenience when working with the scene files, and can be omitted completely on a short enough line. However, a good target is approximately one area per station along the line. For long high-speed lines, intermediate stations may be needed, and for short suburban/tram lines, one area may cover 4-5 stations. 

`scene.json` contains offset values for each area, such that all areas may be centered in the editor. If your 3d editor supports precise offsets on export, you may not need this option. 


```json
scene.json
{
  "area_name_1": [0.0, -5.0, 10000.0],
  "area_name_2": ...
}
```


### Track
`track.json` contains data about the railway tracks themselves. It is generated from OpenStreetMap data points and does generally not need manual edits.

```json
example track.json
{
	"origin": [
        59.44221,
        18.06251
    ],
    "points": {
        "254628239": {
            "xoffset": -744.9084868222618,
            "yoffset": 7835.990463970706,
            "tangent": [
                0.014529880727445998,
                0.9998944357111136
            ],
            "linked_nodes": {
                "2072178796": {
                    "direction": [
                        -0.014529880727445998,
                        -0.9998944357111136
                    ],
                    "distance": 30.738556301934413,
                    "tags": {
                        "axle_load": "16",
                        "electrified": "contact_line",
                        ...
                    }
                },
                "254628238": {
                    "direction": [
                        0.019406218973844758,
                        0.9998116816006597
                    ],
                    "distance": 20.876335922201896,
                    "tags": {
                        "axle_load": "16",
                        "bridge": "yes",
                        ...
                    }
                }
            }
        },
        "2072178796": {
            ...
	    },
	    ...
    }
}
```