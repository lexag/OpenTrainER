using Godot;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

public enum WorldManagerMode
{
	World,
	Menu
}

public static class WorldManager
{
	public static Node worldRoot = null;

	public static void Setup()
	{
		VehicleManager.vehicleWorldCoordinate = RouteManager.trackNodesInRoute.Last().Value.WorldCoordinate;
		WorldRenderer.RenderListOfTrackNodes(RouteManager.trackNodesInRoute.Values.ToList());
		WorldRenderer.RenderListOfStations(RouteManager.stationsInRoute);

		VehicleManager.Startup();

		worldRoot.SetProcess(true);
	}

	public static void Tick(double delta)
	{
		VehicleManager.Tick(delta);
		WorldRenderer.RenderTick();
	}
}

