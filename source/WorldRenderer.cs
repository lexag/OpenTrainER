
//using Godot;
//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.IO;

//static internal class WorldRenderer
//{

//	// below is probably all obsolete (maybe?)
//	internal static List<WorldObject> worldObjects = new List<WorldObject>();

//	static List<TrackPoint> nodesQueuedForInstancing = new List<TrackPoint>();

//	public static void RenderListOfTrackNodes(List<TrackPoint> trackNodes)
//	{
//		foreach (var node in trackNodes)
//		{
//			if (!node.isInstanced)
//			{
//				InstanceTrackNode(node);
//				QueueTrackNodeForSegmentInstancing(node);
//			}
//		}
//		foreach (var node in trackNodes)
//		{
//			node.RecalculateTangent();
//		}
//	}

//	public static void QueueTrackNodeForSegmentInstancing(TrackPoint trackNode)
//	{
//		nodesQueuedForInstancing.Add(trackNode);
//	}


//	public static void RenderTick()
//	{
//		if (nodesQueuedForInstancing.Count > 0)
//		{
//			TrackPoint trackNode = nodesQueuedForInstancing[0];
//			nodesQueuedForInstancing.RemoveAt(0);

//			foreach (var neighbour in trackNode.NeighbourDistances)
//			{
//				InstanceTrackSegment(trackNode, neighbour.Key);
//			}
//		}
//	}

//	static void InstanceTrackNode(TrackPoint trackNode)
//	{
//		Vector3 localPosition = trackNode.WorldCoordinate.ToLocal_M(VehicleManager.vehicleWorldCoordinate);

//		Node3D point = new Node3D();
//		trackNode.physicalNode = point;
//		WorldManager.worldRoot.AddChild(point);
//		trackNode.LocalCoordinate = localPosition;
//		point.Position = localPosition;

//		Sprite3D sprite = new Sprite3D();
//		point.AddChild(sprite);
//		sprite.Texture = (Texture2D)GD.Load("res://icon.svg");
//		//sprite.GlobalScale(trackNode.tangentVector);
//		sprite.PixelSize = 0.02f;
//		sprite.RotateX(Mathf.Pi / 2);

//		worldObjects.Add(trackNode);
//		trackNode.isInstanced = true;
//	}

//	static void InstanceTrackSegment(TrackPoint a, TrackPoint b)
//	{
//		float distance = (float)TrackInterpolation.GetLengthOfSegment(a, b);
//		float targetSegmentLength = 2.0f;
//		int numSegments = Math.Max((int)(distance / targetSegmentLength) + 1, 2);

//		float[] keysAlongSegment = TrackInterpolation.SampleEquidistantKeys(a, b, numSegments);

//		Vector3 cursor = new Vector3();
//		for (int i = 0; i < numSegments - 1;i++)
//		{
//			float t = keysAlongSegment[i];
//			cursor = TrackInterpolation.GetPositionFromKey(a, b, t);
//			Vector3 nextCursor = TrackInterpolation.GetPositionFromKey(a, b, keysAlongSegment[i + 1]);

//			SpawnTrackMesh(a, cursor, nextCursor);
//		}
//		SpawnTrackMesh(a, cursor, b.LocalCoordinate);
//	}

//	static void SpawnTrackMesh(TrackPoint ownerNode, Vector3 position, Vector3 target)
//	{
//		Vector3 forwardVector = position.DirectionTo(target);
//		Vector3 sideVector = Vector3.Up.Cross(forwardVector).Normalized();
//		Vector3 railSideOffset = sideVector * 1.435f / 2;

//		position += railSideOffset;
//		target += railSideOffset;

//		float actualSegmentLength = position.DistanceTo(target);

//		MeshInstance3D meshInstance = new MeshInstance3D();
//		ownerNode.physicalNode.AddChild(meshInstance);
//		meshInstance.Mesh = (Mesh)GD.Load("res://assets/track/TrackCube.obj");
//		meshInstance.GlobalPosition = position;
//		meshInstance.Scale = new Vector3(actualSegmentLength + 0.05f, 0.1f, 0.05f);
//		meshInstance.LookAt(target);
//		meshInstance.RotateY(Mathf.Pi / 2);
//	}

//	public static void MoveWorld(Vector2 deltaTravel)
//	{
//		for (int i = 0; i < worldObjects.Count; i++)
//		{
//			worldObjects[i].LocalCoordinate -= new Vector3(deltaTravel.X, 0, deltaTravel.Y);
//		}
//	}

//	public static void MoveWorld(Vector3 deltaTravel)
//	{
//		MoveWorld(new Vector2(deltaTravel.X, deltaTravel.Z));
//	}


//	public static void RenderListOfStations(Dictionary<string, LatLon> stationsInRoute)
//	{
//		foreach (var station in stationsInRoute)
//		{
//			LoadTracksideScene("SE", station.Key, station.Value);
//		}
//	}

//	public static void LoadTracksideScene(string region, string identifier, LatLon worldCoordinate)
//	{
//		GltfDocument gltfLoadDocument = new GltfDocument();
//		GltfState gltfLoadState = new GltfState();
//		try
//		{
//			Error error = gltfLoadDocument.AppendFromFile($"res://assets/route/{region}/{identifier}.glb", gltfLoadState);
//			if (error.Equals(Error.Ok))
//			{
//				Node3D gltfSceneRootNode = (Node3D)gltfLoadDocument.GenerateScene(gltfLoadState);

//				WorldManager.worldRoot.AddChild(gltfSceneRootNode);

//				WorldObject wo = new WorldObject();
//				wo.physicalNode = gltfSceneRootNode;
//				wo.LocalCoordinate = worldCoordinate.ToLocal_M(VehicleManager.vehicleWorldCoordinate);
//				gltfSceneRootNode.Scale = new Vector3(1, 1, 1);
//				gltfSceneRootNode.RotateY(-Mathf.Pi / 2);
//				//gltfSceneRootNode.RotateX(Mathf.Pi / 2);
//				//gltfSceneRootNode.RotateX(Mathf.Pi);
//				//gltfSceneRootNode.RotateZ(Mathf.Pi);
//				worldObjects.Add(wo);
//			}
//			else
//			{
//				GD.PrintErr($"Couldn't load glTF scene (error code: {error}).");
//			}
//		}
//		catch (Exception ex)
//		{
//			GD.PrintErr(ex.ToString());
//		}
//	}


//}

