using Godot;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

partial class WorldManager : Node3D
{
    public List<TrackNode> trackNodes = new List<TrackNode>();

    public override void _Ready()
    {
        base._Ready();
        SetProcess(false);

        WorldLoader worldLoader = new WorldLoader();
        worldLoader.worldManager = this;
        AddChild(worldLoader);
        worldLoader.Load();
        
        // Awaits NetworkingDone call from worldLoader.
    }

    public void NetworkingDone()
    {
        SetProcess(true);

        WorldRenderer.worldRoot = this;
        WorldRenderer.InstanceTrack(trackNodes);

        VehicleManager.worldManager = this;
        VehicleManager.Startup();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        VehicleManager.Tick(delta);
    }
}

