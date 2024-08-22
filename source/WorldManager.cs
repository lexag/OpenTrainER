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

public struct SignalsFileStruct
{
	public Dictionary<string, SignalFeature> signals;
}

public struct SignalingFileStruct
{
	public Dictionary<string, Dictionary<string, SignalLightStruct>> layouts; //Mapping layout names to group of lights, with each light id mapped to SignalLightStruct
}


public struct TrackPoint
{
	public double[] position;
	public Dictionary<string, TrackPointLink> linked_nodes;
	public object feature;
	public float[] tangent;
}

public struct TrackPointLink
{
	public float[] direction;
	public float distance;
	public int gauge;
	public int osm_way_direction;
	public Dictionary<string, float> electricity;
}


public struct Route
{
	public string[] points;
}

public static class WorldManager
{
	public static Renderer renderer = null;

	public static string lineName;

	public static TrackFileStruct track;
    public static Dictionary<string, SignalFeature> signals;
    public static SignalingFileStruct signalingData;

    public static void Setup()
	{
		LoadLineAndRoute("roslagsbanan_27", "OstP2-KarP1");
		//VehicleManager.vehicleWorldCoordinate = RouteManager.trackNodesInRoute.Last().Value.WorldCoordinate;
		//WorldRenderer.RenderListOfTrackNodes(RouteManager.trackNodesInRoute.Values.ToList());
		//WorldRenderer.RenderListOfStations(RouteManager.stationsInRoute);

		Vehicle.Init("test_vehicle");

		//worldRoot.SetProcess(true);
	}

	public static void Tick(double delta)
	{
		Vehicle.Tick(delta);
		//WorldRenderer.RenderTick();
	}

	public static void LoadLineAndRoute(string _lineName, string routeName)
	{
		lineName = _lineName;
		RoutesFileStruct routes = JSONLoader.LoadFile<RoutesFileStruct>("lines/" + _lineName, "routes.json");
		Vehicle.currentRoute = routes.routes[routeName];

		Dictionary<string, float[]> sceneData = JSONLoader.LoadFile<Dictionary<string, float[]>>("lines/" + _lineName, "scene.json");
		foreach (var sceneInfo in sceneData)
		{
			Node3D scene = GLTFLoader.LoadFromPath("lines/" + _lineName, "scene/" + sceneInfo.Key, userFile: true);
			renderer.RenderScene(scene, sceneInfo.Value);
		}

		track = JSONLoader.LoadFile<TrackFileStruct>("lines/" + _lineName, "track.json");
		signals = JSONLoader.LoadFile<SignalsFileStruct>("lines/" + _lineName, "signals.json").signals;
        signalingData = JSONLoader.LoadFile<SignalingFileStruct>("lines/" + _lineName, "signaling.json");

        renderer.RenderTrack(track.points);
		renderer.RenderSignals(signals);
	}

    
}