
using System.Collections.Generic;

internal class TrackNode
{
    LatLon worldCoordinate;
    List<TrackNode> neighbours = new List<TrackNode>();

    public LatLon WorldCoordinate {  get { return worldCoordinate; } set { worldCoordinate = value; } }



    public TrackNode(LatLon worldCoordinate)
    {
        this.worldCoordinate = worldCoordinate;
    }

    public TrackNode() { }

    public void AddNeighbour(TrackNode neighbour)
    {
        if (neighbours.Contains(neighbour))
        {
            return;
        }
        neighbours.Add(neighbour);
        neighbour.AddNeighbour(this);
    }
}
