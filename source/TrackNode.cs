
using Godot;
using System;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;
using System.Numerics;
using Vector3 = Godot.Vector3;

internal class TrackNode
{
    LatLon worldCoordinate;
    Dictionary<TrackNode, double> neighbourDistances = new Dictionary<TrackNode, double>();
    public Node3D physicalNode;

    public LatLon WorldCoordinate { get { return worldCoordinate; } set { worldCoordinate = value; } }
    public Dictionary<TrackNode, double> NeighbourDistances { get { return neighbourDistances; } }

    public Vector3 tangentVector;

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

    public Vector3 recalculateTangent()
    {
        // Find two furthest neighbours with at least 90 deg separation
        double distanceRecord = 100000;
        Tuple<TrackNode, TrackNode> recordHolder = null;
        foreach (TrackNode a in neighbourDistances.Keys)
        {
            foreach (TrackNode b in neighbourDistances.Keys)
            {
                double distance = a.physicalNode.Position.DistanceTo(b.physicalNode.Position);
                if (a != b 
                    && (a.physicalNode.Position - this.physicalNode.Position).AngleTo(this.physicalNode.Position - b.physicalNode.Position) < Mathf.Pi/2
                    && distance < distanceRecord)
                {
                    distanceRecord = distance;
                    recordHolder = new Tuple<TrackNode, TrackNode>(a, b);
                }
            }
        }
        Vector3 nextPosition = recordHolder.Item1.physicalNode.Position;
        Vector3 thisPosition = this.physicalNode.Position;
        Vector3 prevPosition = recordHolder.Item2.physicalNode.Position;

        // Find circle through 3 point (https://math.stackexchange.com/a/3503338)
        Vector3 w = (prevPosition - nextPosition) / (thisPosition - nextPosition);

        if (Math.Abs(w.Z) <= 0.0000001)
        {
            tangentVector = (nextPosition - prevPosition).Normalized();

        }

        Vector3 center = (thisPosition - nextPosition) * (w - new Vector3(Mathf.Pow(w.Length(), 2), 0, 0)) / (-2 * w.Z) + nextPosition;
        double radius = (nextPosition - center).Length();

        tangentVector = (center - thisPosition).Rotated(Vector3.Down, Mathf.Pi / 2);
        // Find tangent of circle at point
        if (tangentVector.AngleTo(thisPosition - nextPosition) > Mathf.Pi / 2)
        {
            tangentVector *= -1;
        }       
        tangentVector = tangentVector.Normalized();
        return tangentVector;
    }
}
