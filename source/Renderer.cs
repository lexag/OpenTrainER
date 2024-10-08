using Godot;
using Godot.NativeInterop;
using OpenTrainER.source.debug;
using OpenTrainER.source.renderer;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;

public partial class Renderer : Node
{
	Dictionary<UnorderedPair<string>, Path3D> pointIdPathMap = new();
	List<Node3D> trackHeightMarkers = new();

    [Export]
	public bool idle = false;

	public override void _EnterTree()
	{
		base._EnterTree();
	}

	public override void _Ready()
	{
		base._Ready();
		WorldManager.renderer = this;
		if (!idle)
		{
			WorldManager.Setup();
		}

        vehicleNode = (Node3D)FindChild("vehicle");
    }

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (!idle)
		{
			WorldManager.Tick(delta);
		}
	}

	public void RenderTrack(Dictionary<string, TrackPoint> trackPoints, bool enableHeight = true)
	{
		//int n = 0;
		List<UnorderedPair<string>> instancedSegments = new();
		foreach (var trackPoint in trackPoints)
		{
			string pointId = trackPoint.Key;
			TrackPoint point = trackPoint.Value;
			foreach (var neighbour in point.linked_nodes)
			{
				if (instancedSegments.Contains(new UnorderedPair<string>(pointId, neighbour.Key)))
				{
					continue;
				}
				Path3D path = InstanceTrackSegment(point, trackPoints[neighbour.Key], enableHeight: enableHeight);
				pointIdPathMap[new UnorderedPair<string>(pointId, neighbour.Key)] = path;

				instancedSegments.Add(new UnorderedPair<string>(pointId, neighbour.Key));
			}
			//n++;
			//if (n > 500) { break;}
		}
	}

    public void RenderScene(Node3D scene, float[] offset)
    {
        AddChild(scene);
		//scene.Visible = false;
		scene.Rotation = new Vector3(0, Mathf.Pi, 0);
		scene.Position += new Vector3(offset[0], offset[1], offset[2]);
		foreach (var trackHeightMarker in scene.FindChildren("track-height*", recursive: true))
		{
			trackHeightMarkers.Add((Node3D)trackHeightMarker);
		}
    }

    Node3D InstancePoint(TrackPoint point, bool b_sprite = false)
	{
		Node3D node = new Node3D();
		AddChild(node);
		node.Position = new Vector3((float)point.position[0], 0, (float)point.position[1]);

		if (b_sprite)
		{
			Sprite3D sprite = new Sprite3D();
			node.AddChild(sprite);
			sprite.Texture = (Texture2D)GD.Load("res://icon.svg");
			//sprite.GlobalScale(trackNode.tangentVector);
			sprite.PixelSize = 0.02f;
			sprite.RotateX(Mathf.Pi / 2);
		}

		return node;
	}


	private float EvaluateWeightedAveragePointHeight(Vector3 pos)
	{
        List<Node3D> sortedList = trackHeightMarkers.OrderBy(o => o.Position.DistanceSquaredTo(new Vector3(pos.X, o.Position.Y, pos.Z))).Reverse().ToList();
		Node3D closest = sortedList[0];
		Node3D secondClosest = sortedList[1];
		Vector3 flat = new Vector3(1, 0, 1);
		float a = closest.Position.Y;
		float b = secondClosest.Position.Y;
        float da = (closest.Position * flat).DistanceTo(pos * flat);
        float db = (secondClosest.Position * flat).DistanceTo(pos * flat);
		float D = da + db;
		return a * (1-da/D) + b * (1-db/D);
    }


	Path3D InstanceTrackSegment(TrackPoint a, TrackPoint b, bool enableHeight = true, float gauge = 1.435f, float thickness = 0.2f)
	{
		Vector3 aPos = new Vector3((float)a.position[0], 0, (float)a.position[1]);
		Vector3 bPos = new Vector3((float)b.position[0], 0, (float)b.position[1]);

		if(enableHeight)
		{
            aPos.Y = EvaluateWeightedAveragePointHeight(aPos);
            bPos.Y = EvaluateWeightedAveragePointHeight(bPos);
        }

  //      Vector3 aTangent = new Vector3(a.tangent[0], aPos.DirectionTo(bPos).Y, a.tangent[1]);
		//Vector3 bTangent = new Vector3(b.tangent[0], bPos.DirectionTo(aPos).Y, b.tangent[1]);
        Vector3 aTangent = new Vector3(a.tangent[0], 0, a.tangent[1]);
        Vector3 bTangent = new Vector3(b.tangent[0], 0, b.tangent[1]);
        Vector3 abDisplacement = bPos - aPos;

		
		aTangent *= Mathf.Sign(aTangent.Dot(abDisplacement));
		bTangent *= Mathf.Sign(bTangent.Dot(-abDisplacement));


		aTangent *= abDisplacement.Length() / 3f;
		bTangent *= abDisplacement.Length() / 3f;
        
		// Debug
        //Debug3D.DrawVector(aTangent, aPos, colorStart: Colors.Red, colorEnd: Colors.Blue);
        //Debug3D.DrawVector(bTangent, bPos, colorStart: Colors.Red, colorEnd: Colors.Green);

		Vector2[] polygonPoints = { new(-gauge, thickness), new(gauge, thickness), new(gauge, 0), new(-gauge, 0) };
		polygonPoints = Geometry2D.ConvexHull(polygonPoints);

		Path3D path = new Path3D();
		AddChild(path);
		path.Position = aPos;

		path.Curve = new Curve3D();
		path.Curve.AddPoint(Vector3.Zero, Vector3.Zero, aTangent);
		path.Curve.AddPoint(abDisplacement, bTangent, Vector3.Zero);

		CsgPolygon3D csgPolygon3D = new();
		path.AddChild(csgPolygon3D);
		csgPolygon3D.Mode = CsgPolygon3D.ModeEnum.Path;
		csgPolygon3D.PathNode = path.GetPath();
		csgPolygon3D.PathLocal = true;
		csgPolygon3D.Polygon = polygonPoints;


        //GD.Print($"a: ({a.xoffset}; {a.yoffset}), b: ({b.xoffset}; {b.yoffset}), aTangent: {aTangent}, bTangent: {bTangent}, abDisplacement: {abDisplacement}, pathPos: {path.Position} == {path.GlobalPosition}");
        return path;
	}

    
	
	Node3D vehicleNode;

	public Transform3D MoveVehicle(Vector3 position, Vector3 forwardVector, string pointA, string pointB)
	{
		Path3D path = pointIdPathMap[new UnorderedPair<string>(pointA, pointB)];
		float closestOffset = path.Curve.GetClosestOffset(position - path.Position);
		Transform3D transform = path.Curve.SampleBakedWithRotation(closestOffset);
		if (transform.Basis.Z.Dot(forwardVector) > 0)
		{
			transform = transform.RotatedLocal(transform.Basis.Y, Mathf.Pi);
		}
		transform.Origin += path.Position;
		Vector3 deltaMove = transform.Origin - position;
		vehicleNode.GlobalTransform = transform;
		//GD.Print($"delta: {deltaMove}, A: {pointA}, B: {pointB}, closestOffset: {closestOffset}   {WorldManager.track.points[pointB].xoffset}, {WorldManager.track.points[pointB].yoffset}   {transform.Origin}");
		return transform;
	}


	public void RenderSignals(Dictionary<string, SignalFeature> signals)
	{
		foreach (var kv in signals)
		{
			SignalRenderer signalRenderer = new SignalRenderer(kv.Key, kv.Value);
			AddChild(signalRenderer);
		}
	}
}

