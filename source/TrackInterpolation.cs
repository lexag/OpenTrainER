using Godot;
using System;

internal static class TrackInterpolation
{
    public static Vector3 GetPositionAlongTrack(TrackNode a, TrackNode b, float t)
    {
        float aTangentStrength = 5;
        float bTangentStrength = 5;
        return CubicBezier(
            a.physicalNode.Position,
            a.physicalNode.Position + a.tangentVector * aTangentStrength,
            b.physicalNode.Position - b.tangentVector * bTangentStrength,
            b.physicalNode.Position,
            t);
    }

    public static Vector3 GetForwardVectorAlongTrack(TrackNode a, TrackNode b, float t)
    {
        float e = 0.001f;
        if (t+e >= 1)
        {
            return GetForwardVectorAlongTrack(a, b, t - 5*e);
        }
        return (GetPositionAlongTrack(a, b, t + e) - GetPositionAlongTrack(a, b, t - e)).Normalized();
    }

    public static double GetLengthOfSegment(TrackNode a, TrackNode b) 
    {
        double sum = 0;
        float e = 0.05f;
        for(float i = 0; i < 1;i+=e)
        {
            sum += GetPositionAlongTrack(a, b, i).DistanceTo(GetPositionAlongTrack(a, b, i + e));
        }
        return sum;
    }


    static private Vector3 CubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        Vector3 q0 = p0.Lerp(p1, t);
        Vector3 q1 = p1.Lerp(p2, t);
        Vector3 q2 = p2.Lerp(p3, t);

        Vector3 r0 = q0.Lerp(q1, t);
        Vector3 r1 = q1.Lerp(q2, t);

        Vector3 s = r0.Lerp(r1, t);
        return s;
    }    
}