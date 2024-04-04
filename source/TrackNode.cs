
using Godot;
using System;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;
using System.Numerics;
using Vector3 = Godot.Vector3;
using System.Linq;

public class TrackNode : WorldObject
{
    Dictionary<TrackNode, double> neighbourDistances = new Dictionary<TrackNode, double>();
    public Vector3 tangentVector;

    public Dictionary<TrackNode, double> NeighbourDistances { get { return neighbourDistances; } }

    public TrackNode() : base() { }
    public TrackNode(LatLon worldCoordinate) : base(worldCoordinate) { }


    public void AddNeighbour(TrackNode neighbour)
    {
        if (neighbourDistances.ContainsKey(neighbour))
        {
            return;
        }
        neighbourDistances.Add(neighbour, worldCoordinate.DistanceTo_M(neighbour.worldCoordinate));
        neighbour.AddNeighbour(this);
    }

    public Vector3 RecalculateTangent()
    {
        if (neighbourDistances.Count == 1)
        {
            tangentVector = (neighbourDistances.Keys.Single().localCoordinate - this.localCoordinate).Normalized();
            GD.Print(tangentVector);
            return tangentVector;
        }


        // Find two furthest neighbours with at least 90 deg separation
        double distanceRecord = 100000;
        Tuple<TrackNode, TrackNode> recordHolder = null;
        foreach (TrackNode a in neighbourDistances.Keys)
        {
            foreach (TrackNode b in neighbourDistances.Keys)
            {
                double distance = a.localCoordinate.DistanceTo(b.localCoordinate);
                if (a != b
                    && (a.localCoordinate - this.localCoordinate).AngleTo(this.localCoordinate - b.localCoordinate) < Mathf.Pi / 2
                    && distance < distanceRecord)
                {
                    distanceRecord = distance;
                    recordHolder = new Tuple<TrackNode, TrackNode>(a, b);
                }
            }
        }
        Vector3 nextPosition = recordHolder.Item1.localCoordinate;
        Vector3 thisPosition = this.localCoordinate;
        Vector3 prevPosition = recordHolder.Item2.localCoordinate;

        // Find circle through 3 point (https://math.stackexchange.com/a/3503338)
        Vector3 w = (prevPosition - nextPosition) / (thisPosition - nextPosition);

        tangentVector = (nextPosition - prevPosition).Normalized();
        return tangentVector;


        Vector3 bisectorA = prevPosition.DirectionTo(thisPosition).Cross(Vector3.Up);
        Vector3 p1 = (prevPosition + thisPosition) / 2;
        Vector3 p2 = p1 + bisectorA * 100;
        Vector3 bisectorB = thisPosition.DirectionTo(nextPosition).Cross(Vector3.Up);
        Vector3 p3 = (thisPosition + nextPosition) / 2;
        Vector3 p4 = p2 + bisectorB * 100;

        p1.Y = 0;
        p2.Y = 0;
        p3.Y = 0;
        p4.Y = 0;

        Vector3 center = new Vector3();

        center.X = (p1.X * p2.Z - p1.Z * p2.X) * (p3.X - p4.X) - (p3.X * p4.Z - p3.Z * p4.X) * (p1.X - p2.X);
        center.Z = (p1.X * p2.Z - p1.Z * p2.X) * (p3.Z - p4.Z) - (p3.X * p4.Z - p3.Z * p4.X) * (p1.Z - p2.Z);

        center /= (p1.X - p2.X) * (p3.Z - p4.Z) - (p3.X - p4.X) * (p1.Z - p2.Z);

        tangentVector = (center - thisPosition).Cross(Vector3.Down).Normalized();
        // CONTINUE HERE
        return tangentVector;
        
        if (Math.Abs(w.Z) <= 0.0000001)
        {
            tangentVector = (nextPosition - prevPosition).Normalized();

        }

        center = (thisPosition - nextPosition) * (w - new Vector3(Mathf.Pow(w.Length(), 2), 0, 0)) / (-2 * w.Z) + nextPosition;
        double radius = (nextPosition - center).Length();

        // Find tangent of circle at point
        if (tangentVector.AngleTo(thisPosition - nextPosition) > Mathf.Pi / 2)
        {
            tangentVector *= -1;
        }
        tangentVector = tangentVector.Normalized();
        return tangentVector;
    }
}
