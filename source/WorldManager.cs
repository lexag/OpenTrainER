using Godot;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

public partial class WorldManager : Node3D
{
	public List<TrackNode> trackNodes = new List<TrackNode>();

	WorldLoader worldLoader = new WorldLoader();

	public override void _Ready()
	{
		base._Ready();
		SetProcess(false);

		worldLoader.worldManager = this;
		AddChild(worldLoader);
		worldLoader.Load();
		
		// Awaits NetworkingDone call from worldLoader.
	}

	public void NetworkingDone()
	{
		WorldRenderer.worldRoot = this;
		WorldRenderer.InstanceTrack(trackNodes);

		VehicleManager.worldManager = this;
		VehicleManager.Startup();

		SetProcess(true);
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		VehicleManager.Tick(delta);
		if (WorldRenderer.flagRequestingWorldLoad)
		{
			WorldRenderer.flagRequestingWorldLoad = false;
			worldLoader.Load();
		}
	}
}

