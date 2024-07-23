using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



public static class VehicleManager
{
	public static Vector3 vehiclePosition = new();

    public static Route currentRoute;

	static int routePointIndex = 1;
	static Vector3 targetPosition = new();
	static Vector3 travelDirection = new();
	//static Vector3 cameraLookTarget = new Vector3();

	static float speed = 60f;
	//static double distanceAlongTrackSegment = 0;
	//static double fractionAlongTrackSegment = 0;
	//static double currentSegmentLength;

	//static Camera3D camera;
	public static Node3D vehicleNode;

	public static void Startup()
	{
		vehiclePosition = GetRoutePointPosition(0);
		targetPosition = GetRoutePointPosition(1);
	}


	public static void Tick(double delta)
	{
		Vector3 deltaTravel = travelDirection * speed * (float)delta;
		vehiclePosition += deltaTravel;
		vehicleNode.Position = vehiclePosition;
		vehicleNode.LookAt(targetPosition);
		travelDirection = vehiclePosition.DirectionTo(targetPosition);
		if ((vehiclePosition - targetPosition).Length() < speed * delta*2)
		{
			routePointIndex++;
			targetPosition = GetRoutePointPosition(routePointIndex);
			// crashes on track runout probably?
		}
	}

	private static Vector3 GetRoutePointPosition(int idx) {
		TrackPoint point = WorldManager.track.points[currentRoute.points[routePointIndex]];
		return new Vector3((float)point.xoffset, 0, (float)point.yoffset);
    }

	//static void TrackNodeStepover()
	//{
	//	currentTrackNode = targetTrackNode;
	//	double deviationRecord = 3;
	//	TrackPoint recordHolder = null;
	//	foreach (var entry in targetTrackNode.NeighbourDistances)
	//	{
	//		double deviation = travelDirection.AngleTo(entry.Key.physicalNode.Position - targetTrackNode.physicalNode.Position);
	//		if (entry.Key != currentTrackNode && deviation < deviationRecord)
	//		{
	//			recordHolder = entry.Key;
	//			deviationRecord = deviation;
	//		}
	//	}
	//	targetTrackNode = recordHolder;
	//	distanceAlongTrackSegment = 0;
	//	fractionAlongTrackSegment = 0;
	//	currentSegmentLength = TrackInterpolation.GetLengthOfSegment(currentTrackNode, targetTrackNode);
	//}

	//static void SnapToTrackNode()
	//{
	//	double distanceRecord = double.MaxValue;
	//	TrackPoint recordHolder = RouteManager.trackNodesInRoute.Values.ToArray()[0];
	//	foreach (TrackPoint trackNode in RouteManager.trackNodesInRoute.Values)
	//	{
	//		double d = trackNode.physicalNode.Position.Length();
	//		if (d < distanceRecord)
	//		{
	//			distanceRecord = d;
	//			recordHolder = trackNode;
	//		}
	//	}
	//	WorldRenderer.MoveWorld(recordHolder.physicalNode.Position);
	//	targetTrackNode = recordHolder;
	//	currentTrackNode = recordHolder;
	//	TrackNodeStepover();
	//}
}

