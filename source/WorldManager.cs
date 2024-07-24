using Godot;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;

public enum WorldManagerMode
{
	World,
	Menu
}

public struct TrackFileStruct
{
	public Dictionary<string, TrackPoint> points;
	public float[] origin;
}

public struct RoutesFileStruct
{
	public Dictionary<string, Route> routes;
}


public struct TrackPoint
{
	public double xoffset;
	public double yoffset;
	public Dictionary<string, TrackPointLink> linked_nodes;
	public float[] tangent;
}

public struct TrackPointLink
{
	public float[] direction;
	public float distance;
	public Dictionary<string, string> tags;
}


public struct Route
{
	public string[] points;
}


public static class WorldManager
{
	public static WorldRoot worldRoot = null;

	public static TrackFileStruct track;

	public static void Setup()
	{
		LoadLineAndRoute("roslagsbanan_27", "OstP2-KarP1");
		//VehicleManager.vehicleWorldCoordinate = RouteManager.trackNodesInRoute.Last().Value.WorldCoordinate;
		//WorldRenderer.RenderListOfTrackNodes(RouteManager.trackNodesInRoute.Values.ToList());
		//WorldRenderer.RenderListOfStations(RouteManager.stationsInRoute);

		Vehicle.vehicleNode = (Node3D)worldRoot.FindChild("vehicle");
		Vehicle.Init("test_vehicle");

		//worldRoot.SetProcess(true);
	}

	public static void Tick(double delta)
	{
		Vehicle.Tick(delta);
		//WorldRenderer.RenderTick();
	}

	public static void LoadLineAndRoute(string lineName, string routeName)
	{
		track = JSONLoader.LoadFile<TrackFileStruct>("lines/"+lineName, "track.json");
		worldRoot.RenderTrack(track.points);

		RoutesFileStruct routes = JSONLoader.LoadFile<RoutesFileStruct>("lines/"+lineName, "routes.json");
		Vehicle.currentRoute = routes.routes[routeName];
	}

}

