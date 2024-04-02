
using Godot;
using System.Collections.Generic;

internal class TrackNode
{
    LatLon worldCoordinate;
    Dictionary<TrackNode, double> neighbourDistances = new Dictionary<TrackNode, double>();
    public Node3D physicalNode;

    public LatLon WorldCoordinate {  get { return worldCoordinate; } set { worldCoordinate = value; } }
    public Dictionary<TrackNode, double> NeighbourDistances { get { return neighbourDistances; } }


    public TrackNode(LatLon worldCoordinate)
    {
        this.worldCoordinate = worldCoordinate;
    }

    public TrackNode() { }

    public void AddNeighbour(TrackNode neighbour)
    {
        if (neighbourDistances.ContainsKey(neighbour))
        {
            return;
        }
        neighbourDistances.Add(neighbour, worldCoordinate.DistanceTo_M(neighbour.worldCoordinate));
        neighbour.AddNeighbour(this);
    }
}
