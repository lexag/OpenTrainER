using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



public static class VehicleManager
{
	public static LatLon vehicleWorldCoordinate = new LatLon(59.34732744174741, 18.069374291605303);

	
	static TrackNode currentTrackNode;
	static TrackNode targetTrackNode;
	static Vector3 travelDirection = new Vector3();
	static Vector3 cameraLookTarget = new Vector3();

	static float speed = 30f;
	static double distanceAlongTrackSegment = 0;
	static double fractionAlongTrackSegment = 0;
	static double currentSegmentLength;

	static Camera3D camera;
	static Node3D vehicleCursor;

	public static void Startup()
	{
		camera = new Camera3D();
		WorldManager.worldRoot.AddChild(camera);
		camera.Current = true;
		camera.Position = new Vector3(0, 3, 0);
		camera.Fov = 60;
		//camera.RotateX(Mathf.Pi / 2);
		//camera.LookAt(new Vector3(0.001f, 0, 0));

		vehicleCursor = new Node3D();
		WorldManager.worldRoot.AddChild(vehicleCursor);

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
		vehicleWorldCoordinate = vehicleWorldCoordinate.Moved_M(deltaTravel.X, deltaTravel.Z);
		WorldRenderer.MoveWorld(actualPositionDelta);

		travelDirection = TrackInterpolation.GetForwardVectorFromKey(currentTrackNode, targetTrackNode, (float)fractionAlongTrackSegment);// + (targetTrackNode.physicalNode.Position - vehicleCursor.Position).Normalized();
		double angleDiff = travelDirection.AngleTo(camera.Position.DirectionTo(cameraLookTarget));

        if (angleDiff < Math.PI / 20)
		{
			cameraLookTarget += (camera.Position + travelDirection * 100) * 0.01f;
			cameraLookTarget /= 1.01f;
		}
		else if (angleDiff > Math.PI / 4)
		{
			cameraLookTarget = camera.Position + travelDirection * 100;
        }

		// Debug
		//cameraLookTarget = camera.Position + Vector3.Down * 1000 + Vector3.Forward;

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
		TrackNode recordHolder = RouteManager.trackNodesInRoute.Values.ToArray()[0];
		foreach (TrackNode trackNode in RouteManager.trackNodesInRoute.Values)
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
		currentTrackNode = recordHolder;
		TrackNodeStepover();
	}
}

