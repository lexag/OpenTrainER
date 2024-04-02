using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



internal static class VehicleManager
{
	public static LatLon vehicleWorldCoordinate = new LatLon(59.50576, 18.07801);

	public static WorldManager worldManager;
	
	static TrackNode currentTrackNode;
	static TrackNode targetTrackNode;
	static Vector3 travelDirection = new Vector3();
	static Vector3 cameraLookTarget = new Vector3();

	static float speed = 50f;
	static double distanceAlongTrackNode = 0;
	static double fractionAlongTrackNode = 0;
	static double currentSegmentLength;

	static Camera3D camera;
	static Node3D vehicleCursor;

	public static void Startup()
	{
		camera = new Camera3D();
		worldManager.AddChild(camera);
		camera.Current = true;
		camera.Position = new Vector3(0, 3, 0);
		camera.Fov = 120;
		//camera.RotateX(Mathf.Pi / 2);
		//camera.LookAt(new Vector3(0.001f, 0, 0));

		vehicleCursor = new Node3D();
		worldManager.AddChild(vehicleCursor);

		SnapToTrackNode();
	}


	public static void Tick(double delta)
	{
		Vector3 deltaTravel = travelDirection * speed * (float)delta;
		distanceAlongTrackNode += deltaTravel.Length();
		fractionAlongTrackNode += deltaTravel.Length() / currentSegmentLength;

		Vector3 actualPositionDelta = TrackInterpolation.GetPositionAlongTrack(currentTrackNode, targetTrackNode, (float)fractionAlongTrackNode) - vehicleCursor.Position;

		if (currentTrackNode == null) 
		{
			SnapToTrackNode();
		}
		else if (vehicleCursor.Position.DistanceTo(targetTrackNode.physicalNode.Position) < 2* speed * delta)
		{
			TrackNodeStepover();
		}
		vehicleWorldCoordinate = vehicleWorldCoordinate.Moved_M(deltaTravel.Y, deltaTravel.X);
		WorldRenderer.MoveWorld(actualPositionDelta);

		travelDirection = TrackInterpolation.GetForwardVectorAlongTrack(currentTrackNode, targetTrackNode, (float)fractionAlongTrackNode);// + (targetTrackNode.physicalNode.Position - vehicleCursor.Position).Normalized();
		cameraLookTarget += (camera.Position + travelDirection * 100) * 0.01f;
		cameraLookTarget /= 1.01f;

        camera.LookAt(cameraLookTarget);
	}


	static void TrackNodeStepover()
	{
		currentTrackNode = targetTrackNode;
		double deviationRecord = 3;
		TrackNode recordHolder = null;
		foreach (var entry in targetTrackNode.NeighbourDistances)
		{
			double deviation = travelDirection.AngleTo(entry.Key.physicalNode.Position - targetTrackNode.physicalNode.Position);
			if (entry.Key != currentTrackNode && deviation < deviationRecord)
			{
				recordHolder = entry.Key;
				deviationRecord = deviation;
			}
		}
		targetTrackNode = recordHolder;
		distanceAlongTrackNode = 0;
		fractionAlongTrackNode = 0;
		currentSegmentLength = TrackInterpolation.GetLengthOfSegment(currentTrackNode, targetTrackNode);
	}

	static void SnapToTrackNode()
	{
		double distanceRecord = 10000;
		TrackNode recordHolder = worldManager.trackNodes[0];
		foreach (TrackNode trackNode in worldManager.trackNodes)
		{
			double d = trackNode.physicalNode.Position.Length();
			if (d < distanceRecord)
			{
				distanceRecord = d;
				recordHolder = trackNode;
			}
		}
		WorldRenderer.MoveWorld(recordHolder.physicalNode.Position);
		targetTrackNode = recordHolder;
		currentTrackNode = targetTrackNode;
		TrackNodeStepover();
	}
}

