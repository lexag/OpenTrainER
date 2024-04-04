
using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;

static internal class WorldRenderer
{
	internal static List<WorldObject> worldObjects = new List<WorldObject>();

	public static Node3D worldRoot = null;

	public static bool flagRequestingWorldLoad = false;
	static WorldObject loadSphere = new WorldObject();

	public static void InstanceTrack(List<TrackNode> trackNodes)
	{
		loadSphere = new WorldObject();
		worldObjects.Add(loadSphere);

		Area3D loadSphereArea = new Area3D();
		worldRoot.AddChild(loadSphereArea);
		loadSphere.physicalNode = loadSphereArea;

		CollisionShape3D loadSphereShape = new CollisionShape3D();
		loadSphereShape.Shape = new SphereShape3D();
		((SphereShape3D)loadSphereShape.Shape).Radius = 1000;
		loadSphereArea.AddChild(loadSphereShape);

		loadSphereArea.AreaExited += ExitedLoadSphere;

		foreach (var node in trackNodes)
		{
			if (!node.isInstanced)
			{
				InstanceTrackNode(node);
			}
		}
		foreach (var node in trackNodes)
		{
			node.RecalculateTangent();
		}
		foreach (var node in trackNodes)
		{
			foreach (var neighbour in node.NeighbourDistances)
			{
				InstanceTrackSegment(node, neighbour.Key);
			}
		}
	}

	static void ExitedLoadSphere(Area3D area)
	{
		if (area.GetParent().Name == "cursor")
		{
			flagRequestingWorldLoad = true;
			loadSphere.physicalNode.QueueFree();
			worldObjects.Remove(loadSphere);
			
		}
	}


	static void InstanceTrackNode(TrackNode trackNode)
	{
		Vector3 localPosition = trackNode.WorldCoordinate.ToLocal_M(VehicleManager.vehicleWorldCoordinate);

		Node3D point = new Node3D();
		trackNode.physicalNode = point;
		worldRoot.AddChild(point);
		trackNode.LocalCoordinate = localPosition;
		point.Position = localPosition;

		Sprite3D sprite = new Sprite3D();
		point.AddChild(sprite);
		sprite.Texture = (Texture2D)GD.Load("res://icon.svg");
		//sprite.GlobalScale(trackNode.tangentVector);
		sprite.PixelSize = 0.02f;
		sprite.RotateX(Mathf.Pi / 2);

		worldObjects.Add(trackNode);
		trackNode.isInstanced = true;
	}

	static void InstanceTrackSegment(TrackNode a, TrackNode b)
	{
		float distance = (float)TrackInterpolation.GetLengthOfSegment(a, b);
		float targetSegmentLength = 2.0f;
		int numSegments = Math.Max((int)(distance / targetSegmentLength) + 1, 2);

		float[] keysAlongSegment = TrackInterpolation.SampleEquidistantKeys(a, b, numSegments);

		Vector3 cursor = new Vector3();
		for (int i = 0; i < numSegments - 1;i++)
		{
			float t = keysAlongSegment[i];
			cursor = TrackInterpolation.GetPositionFromKey(a, b, t);
			Vector3 nextCursor = TrackInterpolation.GetPositionFromKey(a, b, keysAlongSegment[i + 1]);

			SpawnTrackMesh(a, cursor, nextCursor);
		}
		SpawnTrackMesh(a, cursor, b.LocalCoordinate);
	}

	static void SpawnTrackMesh(TrackNode ownerNode, Vector3 position, Vector3 target)
	{
		Vector3 forwardVector = position.DirectionTo(target);
		Vector3 sideVector = Vector3.Up.Cross(forwardVector).Normalized();
		Vector3 railSideOffset = sideVector * 1.435f / 2;

		position += railSideOffset;
		target += railSideOffset;

		float actualSegmentLength = position.DistanceTo(target);

		MeshInstance3D meshInstance = new MeshInstance3D();
		ownerNode.physicalNode.AddChild(meshInstance);
		meshInstance.Mesh = (Mesh)GD.Load("res://assets/track/TrackCube.obj");
		meshInstance.GlobalPosition = position;
		meshInstance.Scale = new Vector3(actualSegmentLength + 0.05f, 0.1f, 0.05f);
		meshInstance.LookAt(target);
		meshInstance.RotateY(Mathf.Pi / 2);
	}

	public static void MoveWorld(Vector2 deltaTravel)
	{
		for (int i = 0; i < worldObjects.Count; i++)
		{
			worldObjects[i].LocalCoordinate -= new Vector3(deltaTravel.X, 0, deltaTravel.Y);
		}
	}

	public static void MoveWorld(Vector3 deltaTravel)
	{
		MoveWorld(new Vector2(deltaTravel.X, deltaTravel.Z));
	}
}

