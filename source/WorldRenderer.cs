
using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;

static internal class WorldRenderer
{
    internal static List<Node3D> worldObjects = new List<Node3D>();

    public static Node3D worldRoot = null;

    public static void InstanceTrack(List<TrackNode> trackNodes)
    {
        foreach (var node in trackNodes)
        {
            InstanceTrackNode(node);
        }
        foreach (var node in trackNodes)
        {
            foreach (var neighbour in node.NeighbourDistances)
            {
                InstanceTrackSegment(node, neighbour.Key);
            }
        }
    }


    static void InstanceTrackNode(TrackNode trackNode)
    {
        Vector3 localPosition = trackNode.WorldCoordinate.ToLocal_M(VehicleManager.vehicleWorldCoordinate);

        Node3D point = new Node3D();
        trackNode.physicalNode = point;
        worldRoot.AddChild(point);
        point.Position = localPosition;

        Sprite3D sprite = new Sprite3D();
        point.AddChild(sprite);
        sprite.Texture = (Texture2D)GD.Load("res://icon.svg");
        sprite.PixelSize = 0.02f;
        sprite.RotateX(Mathf.Pi / 2);
        worldObjects.Add(point);
    }

    static void InstanceTrackSegment(TrackNode a, TrackNode b)
    {
        float distance = (float)TrackInterpolation.GetLengthOfSegment(a, b);
        float targetSegmentLength = 2.0f;
        int numSegments = (int)(distance / targetSegmentLength);
        Vector3 direction = b.WorldCoordinate.ToLocal_M(a.WorldCoordinate).Normalized();

        for (int i = 0; i < numSegments;i++)
        {
            Vector3 railSideOffset = direction.Rotated(Vector3.Down, Mathf.Pi / 2) * 1.435f / 2;


            Vector3 cursor = TrackInterpolation.GetPositionAlongTrack(a, b, (float)i/numSegments) - a.physicalNode.Position;
            Vector3 directionCursor = TrackInterpolation.GetPositionAlongTrack(a, b, (float)(i+1) / numSegments) + railSideOffset;
            float segmentLength = cursor.DistanceTo(directionCursor - railSideOffset - a.physicalNode.Position);

            MeshInstance3D meshInstance = new MeshInstance3D();
            meshInstance.Mesh = (Mesh)GD.Load("res://SandboxMeshes/TrackCube.obj");
            Vector3 meshPos = cursor + railSideOffset;
            meshInstance.Position = meshPos;
            meshInstance.Scale = new Vector3(segmentLength, 0.1f, 0.1f);
            
            a.physicalNode.AddChild(meshInstance);
            meshInstance.LookAt(directionCursor);
            meshInstance.RotateY(Mathf.Pi / 2);
        }
    }

    public static void MoveWorld(Vector2 deltaTravel)
    {
        for (int i = 0; i < worldObjects.Count; i++)
        {
            worldObjects[i].Position -= new Vector3(deltaTravel.X, 0, deltaTravel.Y);
        }
    }

    public static void MoveWorld(Vector3 deltaTravel)
    {
        MoveWorld(new Vector2(deltaTravel.X, deltaTravel.Z));
    }
}

