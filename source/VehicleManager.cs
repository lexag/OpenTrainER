using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



public static class VehicleManager
{
	public static LatLon vehicleWorldCoordinate = new LatLon(59.38791, 18.04446);

	public static WorldManager worldManager;
	
	static TrackNode currentTrackNode;
	static TrackNode targetTrackNode;
	static Vector3 travelDirection = new Vector3();
	static Vector3 cameraLookTarget = new Vector3();

	static float speed = 1f;
	static double distanceAlongTrackSegment = 0;
	static double fractionAlongTrackSegment = 0;
	static double currentSegmentLength;

	static Camera3D camera;
	static Node3D vehicleCursor;

	public static void Startup()
	{
		camera = new Camera3D();
		worldManager.AddChild(camera);
		camera.Current = true;
		camera.Position = new Vector3(0, 4, 0);
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
		distanceAlongTrackSegment += deltaTravel.Length();
		fractionAlongTrackSegment += deltaTravel.Length() / currentSegmentLength;

		Vector3 actualPositionDelta = TrackInterpolation.GetPositionFromKey(currentTrackNode, targetTrackNode, (float)fractionAlongTrackSegment) - vehicleCursor.GlobalPosition;

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

		travelDirection = TrackInterpolation.GetForwardVectorFromKey(currentTrackNode, targetTrackNode, (float)fractionAlongTrackSegment);// + (targetTrackNode.physicalNode.Position - vehicleCursor.Position).Normalized();
		cameraLookTarget += (camera.Position + travelDirection * 100) * 0.01f;
		cameraLookTarget /= 1.01f;
		
		// Debug
		cameraLookTarget = camera.Position + Vector3.Down * 1000 + Vector3.Forward;

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
		distanceAlongTrackSegment = 0;
		fractionAlongTrackSegment = 0;
		currentSegmentLength = TrackInterpolation.GetLengthOfSegment(currentTrackNode, targetTrackNode);
	}

	static void SnapToTrackNode()
	{
		double distanceRecord = double.MaxValue;
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

