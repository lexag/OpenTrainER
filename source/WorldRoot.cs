using Godot;
using Godot.NativeInterop;
using System.Collections.Generic;

public partial class WorldRoot : Node
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

    public void RenderTrack(Dictionary<string, TrackPoint> trackPoints)
    {
        List<string> instancedSegments = new List<string>();
        foreach (var trackPoint in trackPoints)
        {
            string pointId = trackPoint.Key;
            TrackPoint point = trackPoint.Value;
            foreach (var neighbour in point.linked_nodes)
            {
                if (instancedSegments.Contains(neighbour.Key + '-' + pointId) || instancedSegments.Contains(pointId + '-' + neighbour.Key))
                {
                    continue;
                }
                InstanceTrackSegment(point, trackPoints[neighbour.Key]);

                instancedSegments.Add(neighbour.Key + '-' + pointId);
                instancedSegments.Add(pointId + '-' + neighbour.Key);
            }
        }
    }

    Node3D InstancePoint(TrackPoint point, bool b_sprite = false)
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

    Path3D InstanceTrackSegment(TrackPoint a, TrackPoint b, float gauge = 1.435f, float thickness = 0.2f)
    {
        Vector3 aTangent = new Vector3(a.tangent[0], 0, a.tangent[1]);
        Vector3 bTangent = new Vector3(b.tangent[0], 0, b.tangent[1]);
        Vector3 abDisplacement = new Vector3((float)(b.xoffset - a.xoffset), 0, (float)(b.yoffset - a.yoffset));

        aTangent *= Mathf.Sign(aTangent.Dot(abDisplacement));
        bTangent *= Mathf.Sign(bTangent.Dot(-abDisplacement));

        aTangent *= abDisplacement.Length() / 2;
        bTangent *= abDisplacement.Length() / 2;

        //Vector2[] polygonPoints = { new(-gauge, thickness), new(gauge, thickness), new(gauge, 0), new(-gauge, 0) };
        //polygonPoints = Geometry2D.ConvexHull(polygonPoints);

        Path3D path = new Path3D();
        AddChild(path);
        path.Position = new Vector3((float)a.xoffset, 0, (float)a.yoffset);

        path.Curve = new Curve3D();
        path.Curve.AddPoint(Vector3.Zero, -aTangent, aTangent);
        path.Curve.AddPoint(abDisplacement, bTangent, -bTangent);

        CsgPolygon3D csgPolygon3D = new();
        path.AddChild(csgPolygon3D);
        csgPolygon3D.Mode = CsgPolygon3D.ModeEnum.Path;
        csgPolygon3D.PathNode = path.GetPath();
        csgPolygon3D.PathLocal = true;
        //csgPolygon3D.Polygon = polygonPoints;

        GD.Print($"a: ({a.xoffset}; {a.yoffset}), b: ({b.xoffset}; {b.yoffset}), aTangent: {aTangent}, bTangent: {bTangent}, abDisplacement: {abDisplacement}, pathPos: {path.Position} == {path.GlobalPosition}");
        return path;
    }
}

