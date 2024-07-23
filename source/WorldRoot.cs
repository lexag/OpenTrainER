using Godot;
using Godot.NativeInterop;
using System.Collections.Generic;

internal partial class WorldRoot : Node
{
	[Export]
	public bool idle = false;

	public override void _EnterTree()
	{
		base._EnterTree();
	}

	public override void _Ready()
	{
		base._Ready();
		WorldManager.worldRoot = this;
		if (!idle)
		{
			WorldManager.Setup();
		}
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (!idle)
		{
			WorldManager.Tick(delta);
		}
	}

    public void RenderTrack(Dictionary<ulong, TrackPoint> trackPoints)
    {
        foreach (var trackPoint in trackPoints)
        {
            ulong pointId = trackPoint.Key;
            TrackPoint point = trackPoint.Value;
			foreach (var neighbour in point.linked_nodes)
			{

			}
        }
    }
	
	public Node3D InstancePoint(TrackPoint point, bool b_sprite = false)
	{
        Node3D node = new Node3D();
        AddChild(node);
        node.Position = new Vector3((float)point.xoffset, 0, (float)point.yoffset);

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

	public void InstanceHalfTrackSegment(TrackPoint a, TrackPoint b)
	{
        Vector3 aTangent = new Vector3(a.tangent[0], 0, a.tangent[1]);
        Vector3 bTangent = new Vector3(b.tangent[0], 0, b.tangent[1]);
		Vector3 abDisplacement = new Vector3((float)(b.xoffset - a.xoffset), 0, (float)(b.yoffset - a.yoffset));

		Path3D path = new Path3D();
		AddChild(path);
		path.Position = new Vector3((float)a.xoffset, 0, (float)a.yoffset);

		path.Curve = new Curve3D();
        path.Curve.AddPoint(Vector3.Zero, Vector3.Zero, aTangent);
        path.Curve.AddPoint(abDisplacement, bTangent, Vector3.Zero);
		
		CsgPolygon3D csgPolygon3D = new();
		path.AddChild(csgPolygon3D);
		csgPolygon3D.Mode = CsgPolygon3D.ModeEnum.Path;
		csgPolygon3D.PathNode = path.GetPath();
		Vector2[] polygonPoints = { new(0, 1), new(1, 1), new(1, 0), new(0, 0) };
		csgPolygon3D.Polygon = polygonPoints;
	}
}

