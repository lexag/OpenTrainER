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

	static float speed = 80/3.6f;
	static double distanceAlongTrackNode = 0;

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
		if (currentTrackNode == null) 
		{
			SnapToTrackNode();
		}
		if (vehicleCursor.Position.DistanceTo(targetTrackNode.physicalNode.Position) < speed*delta)
		{
			TrackNodeStepover();
		}
		vehicleWorldCoordinate = vehicleWorldCoordinate.Moved_M(deltaTravel.Y, deltaTravel.X);
		WorldRenderer.MoveWorld(deltaTravel);

		travelDirection = (targetTrackNode.physicalNode.Position - vehicleCursor.Position).Normalized();
		camera.LookAt(camera.Position + travelDirection*100);
    }


	static void TrackNodeStepover()
	{
		double lastSegmentLength = currentTrackNode.NeighbourDistances[targetTrackNode];
		currentTrackNode = targetTrackNode;
		foreach (var entry in targetTrackNode.NeighbourDistances)
		{
			if (entry.Key != currentTrackNode)
			{
				targetTrackNode = entry.Key; break;
			}
		}
		distanceAlongTrackNode = 0;
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
		currentTrackNode = recordHolder;
		targetTrackNode = currentTrackNode.NeighbourDistances.FirstOrDefault().Key;
		WorldRenderer.MoveWorld(currentTrackNode.physicalNode.Position);

    }
}

