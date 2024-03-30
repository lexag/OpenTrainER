﻿using Godot;
using System;
using System.Numerics;

internal struct LatLon
{
    const double earthRadius = 6378.137;
    
    double lat;
    double lon;

    public double Latitude
    {
        get { return lat; }
        set { lat = value; }
    }

    public double Longitude
    {
        get { return lon; }
        set { lon = value; }
    }



    public LatLon(double lat, double lon)
    {
        this.lat = lat;
        this.lon = lon;
    }

    private Tuple<double, double> DegreesPerMeter(LatLon coordinate) { return new Tuple<double, double>((1 / ((2 * Math.PI / 360) * earthRadius)) / 1000, (1 / ((2 * Math.PI / 360) * earthRadius)) / 1000 / Math.Cos(coordinate.lat * (Math.PI / 180))); }


    public static LatLon operator +(LatLon left, LatLon right)
    {
        return new LatLon(left.Latitude+right.Latitude, left.Longitude+right.Longitude);
    }
    public static LatLon operator -(LatLon left, LatLon right)
    {
        return new LatLon(left.Latitude - right.Latitude, left.Longitude - right.Longitude);
    }


    public LatLon MovedMeters(double offsetLat, double offsetLon)
    {
        (double mLat, double mLon) = DegreesPerMeter(this);
        var newLatitude = this.lat + (offsetLat * mLat);
        var newLongitude = this.lon + (offsetLon * mLon);

        return new LatLon(newLatitude, newLongitude);
    }

    public Godot.Vector2 ToLocal(LatLon origin)
    {
        (double mLat, double mLon) = DegreesPerMeter(origin);
        float x =  (float)((this.lon - origin.lon)/mLon);
        float y =  (float)((this.lat - origin.lat)/mLat);
        return new Godot.Vector2(x, y);
    }
}
