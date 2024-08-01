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

    public static Route currentRoute;
	public static int routePointIndex = 1;

	public static Node3D vehicleNode;

	static Vector3 vehiclePosition = new();
	static Vector3 targetPosition = new();
	static Vector3 travelDirection = new();
	static Vector3 forwardDirection = new();

	static Dictionary<string, double> properties = new();
	static List<VehicleComponent> components = new();

	public static void Init(string vehicleFilePath)
	{
		VehicleFileStruct vehicleFileStruct = JSONLoader.LoadFile<VehicleFileStruct>("vehicles/" + vehicleFilePath, "vehicle.json");
		GD.Print(JSONLoader.Reparse<KeyboardInputComponent>(vehicleFileStruct.components["KeyboardInputComponent"]).mappings);

		// Load all components
		foreach (var component in vehicleFileStruct.components)
		{
			VehicleComponent newComponent = null;
			try
			{
				newComponent = (VehicleComponent)Activator.CreateInstance(Type.GetType("OpenTrainER.source.vehicle.component." + component.Key));
				JSONLoader.Repopulate(newComponent, component.Value);
			}
			catch (ArgumentNullException argumentNullException)
			{
				GD.Print($"component {component.Key} is invalid");
			}

			if (newComponent != null)
			{
				components.Add(newComponent);
				newComponent.Init();
				GD.Print($"added component {component.Key}");
			}

		}

		vehiclePosition = GetRoutePointPosition(0);
		targetPosition = GetRoutePointPosition(1);
		travelDirection = vehiclePosition.DirectionTo(targetPosition);
	}


	public static void Tick(double delta)
	{
		Vector3 deltaTravel = forwardDirection.Normalized() * (float)delta * (float)GetProperty("speed");
		vehiclePosition += deltaTravel;
		Transform3D vehicleTransform = WorldManager.renderer.MoveVehicle(vehiclePosition, travelDirection, currentRoute.points[routePointIndex - 1], currentRoute.points[routePointIndex]);
		vehiclePosition = vehicleTransform.Origin;
		forwardDirection = -vehicleTransform.Basis.Z;

		Vector3 flat = new Vector3(1, 0, 1);
		//GD.Print($"{(vehiclePosition * flat).DistanceTo(targetPosition * flat)}   {speed * delta * 2}   {currentRoute.points[routePointIndex]}   {targetPosition}  {vehiclePosition}");
		if ((vehiclePosition * flat).DistanceTo(targetPosition * flat) < (float)GetProperty("speed") * delta*2f)
		{
			routePointIndex++;
			targetPosition = GetRoutePointPosition(routePointIndex);
			travelDirection = vehiclePosition.DirectionTo(targetPosition);
			// crashes on track runout probably?
		}

		foreach (var component in components)
		{
			component.Tick(delta);
		}

		// Debug
		foreach (var property in properties)
		{
			GD.Print($"{property.Key}: {property.Value}");
		}
	}

	private static Vector3 GetRoutePointPosition(int idx) {
		TrackPoint point = WorldManager.track.points[currentRoute.points[idx]];
		return new Vector3((float)point.xoffset, 0, (float)point.yoffset);
    }


	public static double GetProperty(string name)
	{
		if (properties.ContainsKey(name)) return properties[name];
		return 0;
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

