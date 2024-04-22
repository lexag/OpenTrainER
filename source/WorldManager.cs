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
		WorldRenderer.RenderListOfTrackNodes(RouteManager.trackNodesInRoute.Values.ToList());
		//WorldRenderer.LoadTracksideScene("SE", "Unv-Djo");

		VehicleManager.Startup();

		worldRoot.SetProcess(true);
	}

	public static void Tick(double delta)
	{
		VehicleManager.Tick(delta);
		WorldRenderer.RenderTick();
	}
}

