using Godot;
using OpenTrainER.source.vehicle.component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


struct VehicleFileStruct
{
	public string name;
	public Dictionary<string, object> components;
}


public static class Vehicle
{
	static readonly Dictionary<string, Type> stringComponentMap = new Dictionary<string, Type>() 
	{
        { "KeyboardInputComponent", typeof(KeyboardInputComponent)},
	};

    public static Route currentRoute;
	static int routePointIndex = 1;

	public static Node3D vehicleNode;

	static Vector3 vehiclePosition = new();
	static Vector3 targetPosition = new();
	static Vector3 travelDirection = new();
	static Vector3 forwardDirection = new();
	static float speed = 10f;

	static Dictionary<string, double> properties = new();
	static List<VehicleComponent> components = new();

	public static void Init(string vehicleFilePath)
	{
		VehicleFileStruct vehicleFileStruct = JSONLoader.LoadFile<VehicleFileStruct>("vehicles", vehicleFilePath + ".json");
		GD.Print(JSONLoader.Reparse<KeyboardInputComponent>(vehicleFileStruct.components["KeyboardInputComponent"]).mappings);

		// Load all components
		foreach (var component in vehicleFileStruct.components)
		{
			VehicleComponent newComponent = component.Key switch
			{
				"KeyboardInputComponent" => JSONLoader.Reparse<KeyboardInputComponent>(component.Value),
				_ => null
			};
			if (newComponent != null)
			{
				components.Add(newComponent);
			}
		}

		vehiclePosition = GetRoutePointPosition(0);
		targetPosition = GetRoutePointPosition(1);
		travelDirection = vehiclePosition.DirectionTo(targetPosition);
	}


	public static void Tick(double delta)
	{
		Vector3 deltaTravel = forwardDirection.Normalized() * speed * (float)delta;
		vehiclePosition += deltaTravel;
		Transform3D vehicleTransform = WorldManager.renderer.MoveVehicle(vehiclePosition, travelDirection, currentRoute.points[routePointIndex - 1], currentRoute.points[routePointIndex]);
		vehiclePosition = vehicleTransform.Origin;
		forwardDirection = -vehicleTransform.Basis.Z;

		Vector3 flat = new Vector3(1, 0, 1);
		//GD.Print($"{(vehiclePosition * flat).DistanceTo(targetPosition * flat)}   {speed * delta * 2}   {currentRoute.points[routePointIndex]}   {targetPosition}  {vehiclePosition}");
		if ((vehiclePosition * flat).DistanceTo(targetPosition * flat) < speed * delta*2f)
		{
			routePointIndex++;
			targetPosition = GetRoutePointPosition(routePointIndex);
			travelDirection = vehiclePosition.DirectionTo(targetPosition);
			// crashes on track runout probably?
		}
	}

	private static Vector3 GetRoutePointPosition(int idx) {
		TrackPoint point = WorldManager.track.points[currentRoute.points[idx]];
		return new Vector3((float)point.xoffset, 0, (float)point.yoffset);
    }


	public static double GetProperty(string name)
	{
		return properties[name];
	}

	/// <summary>
	/// Used by VehicleComponents
	/// Tries to set property and returns whether property exists.
	/// </summary>
	public static bool SetProperty(string name, double value)
	{
		if (!properties.ContainsKey(name))
		{
			return false;
		}
		properties[name] = value;
		return true;
	}


    /// <summary>
    /// Used by VehicleComponents
    /// Tries to change property and returns whether property exists.
    /// </summary>
    public static bool ChangeProperty(string name, double value)
    {
        if (!properties.ContainsKey(name))
        {
            return false;
        }
        properties[name] += value;
        return true;
    }


    /// <summary>
    /// Used by VehicleComponents
    /// Tries to init property (with optional value) and returns whether property was inited properly.
    /// </summary>
	public static bool InitProperty(string name, double value = 0.0) 
	{
        if (properties.ContainsKey(name))
        {
            return false;
        }
        properties[name] = value;
        return true;
    }

}

