
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
        Vector2 localPosition = trackNode.WorldCoordinate.ToLocal_M(VehicleManager.vehicleWorldCoordinate);

        Node3D point = new Node3D();
        trackNode.physicalNode = point;
        worldRoot.AddChild(point);
        point.Position = new Vector3(localPosition.X, 0, localPosition.Y);

        Sprite3D sprite = new Sprite3D();
        point.AddChild(sprite);
        sprite.Texture = (Texture2D)GD.Load("res://icon.svg");
        sprite.PixelSize = 0.02f;
        sprite.RotateX(Mathf.Pi / 2);
        worldObjects.Add(point);
    }

    static void InstanceTrackSegment(TrackNode a, TrackNode b)
    {
        float distance = (float)a.WorldCoordinate.DistanceTo_M(b.WorldCoordinate);
        float targetSegmentLength = 100.0f;
        int numSegments = (int)(distance / targetSegmentLength + 1);
        float actualSegmentLength = distance / numSegments;
        Vector2 cursor = new Vector2();
        Vector2 direction = b.WorldCoordinate.ToLocal_M(a.WorldCoordinate).Normalized();

        for (int i = 0; i < numSegments;i++)
        {
            MeshInstance3D meshInstance = new MeshInstance3D();
            meshInstance.Mesh = (Mesh)GD.Load("res://SandboxMeshes/TrackCube.obj");
            Vector2 meshPos = cursor + direction.Rotated(Mathf.Pi / 2) * 1.435f / 2;
            Vector3 meshPos3 = new Vector3(meshPos.X, 0, meshPos.Y);
            meshInstance.Position = meshPos3;
            meshInstance.Scale = new Vector3(actualSegmentLength, 0.1f, 0.1f);
            a.physicalNode.AddChild(meshInstance);
            meshInstance.LookAt(a.physicalNode.Position + meshPos3 + new Vector3(direction.X, 0, direction.Y)*100);
            meshInstance.RotateY(Mathf.Pi/2);


            cursor += direction * actualSegmentLength;
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

