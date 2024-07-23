using Godot;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public enum WorldManagerMode
{
	World,
	Menu
}

public struct TrackPoint
{
	public double xoffset;
	public double yoffset;
	public Dictionary<ulong, TrackPoint> linked_nodes;
	public float[] tangent;
}

public struct TrackPointLink
{
	public Vector2 direction;
	public float distance;
	public Dictionary<string, string> tags;
}

public static class WorldManager
{
	public static Node worldRoot = null;

	public static void Setup()
	{
		LoadLine("roslagsbanan_27");
		//VehicleManager.vehicleWorldCoordinate = RouteManager.trackNodesInRoute.Last().Value.WorldCoordinate;
		//WorldRenderer.RenderListOfTrackNodes(RouteManager.trackNodesInRoute.Values.ToList());
		//WorldRenderer.RenderListOfStations(RouteManager.stationsInRoute);

		//VehicleManager.Startup();

		//worldRoot.SetProcess(true);
	}

	public static void Tick(double delta)
	{
		//VehicleManager.Tick(delta);
		//WorldRenderer.RenderTick();
	}

	public static void LoadLine(string lineName)
	{
		var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
		var lineFolderPath = Path.Combine(path, "OpenTrainER", "lines", lineName);
		using (StreamReader r = new StreamReader(Path.Combine(lineFolderPath, "track.json")))
		{
			string json = r.ReadToEnd();
			Dictionary<ulong, TrackPoint> track = JsonConvert.DeserializeObject<Dictionary<ulong, TrackPoint>>(json);
			//WorldRenderer.RenderTrack(track);
		}
	}
}

