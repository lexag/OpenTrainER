//using Godot;
//using System;
//using System.Xml.Schema;

//public class WorldObject
//{
//    protected LatLon worldCoordinate;
//    protected Vector3 localCoordinate;

//    public Node3D physicalNode;

//    public LatLon WorldCoordinate
//    {
//        get
//        {
//            return worldCoordinate;
//        }
//        set
//        {
//            worldCoordinate = value;
//            localCoordinate = worldCoordinate.ToLocal_M(VehicleManager.vehicleWorldCoordinate);
//        }
//    }

//    public Vector3 LocalCoordinate
//    {
//        get
//        {
//            return localCoordinate;
//        }
//        set
//        {
//            localCoordinate = value;
//            physicalNode.GlobalPosition = localCoordinate;
//        }
//    }


//    public WorldObject() { }
//    public WorldObject(LatLon worldCoordinate)
//    {
//        this.worldCoordinate = worldCoordinate;
//        this.localCoordinate = worldCoordinate.ToLocal_M(VehicleManager.vehicleWorldCoordinate);
//    }

//    public WorldObject(Vector3 localCoordinate)
//    {
//        throw new NotImplementedException();
//        //this.localCoordinate = localCoordinate;
//        //this.worldCoordinate = localCoordinate to global;
//    }
//}

