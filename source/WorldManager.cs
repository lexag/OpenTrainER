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
	public static List<TrackNode> trackNodes = new List<TrackNode>();

	public static Node worldRoot = null;
	static WorldLoader worldLoader = new WorldLoader();


	public static void Setup()
	{
		worldRoot.SetProcess(false);
		worldRoot.AddChild(worldLoader);
		worldLoader.Load();
		
		// Awaits NetworkingDone call from worldLoader.
	}

	public static void NetworkingDone()
	{
		WorldRenderer.RenderNodeset(trackNodes);
		WorldRenderer.LoadTracksideScene("SE", "Unv-Djo");

		VehicleManager.Startup();

		worldRoot.SetProcess(true);
	}

	public static void Tick(double delta)
	{
		VehicleManager.Tick(delta);
		WorldRenderer.RenderTick();
		if (WorldRenderer.flagRequestingWorldLoad)
		{
			WorldRenderer.flagRequestingWorldLoad = false;
			worldLoader.Load();
		}
	}
}

