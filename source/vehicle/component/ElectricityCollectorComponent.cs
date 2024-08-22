using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTrainER.source.vehicle.component
{
	internal class ElectricityCollectorComponent : VehicleComponent
	{
		string trackPointId;
		string prevTrackPointId;

		double lineVoltage;
		double lineFrequency;

		protected override void OnInit()
		{
			Vehicle.InitProperty("line_voltage");
			Vehicle.InitProperty("line_frequency");
		}

		protected override void OnTick(double delta)
		{

			if (trackPointId != Vehicle.currentRoute.points[Vehicle.routePointIndex]) 
			{
				prevTrackPointId = Vehicle.currentRoute.points[Vehicle.routePointIndex - 1];
				trackPointId = Vehicle.currentRoute.points[Vehicle.routePointIndex];
				lineVoltage = (double?)WorldManager.track.points[trackPointId].linked_nodes[prevTrackPointId].electricity["voltage"] ?? lineVoltage;
				lineFrequency = (double?)WorldManager.track.points[trackPointId].linked_nodes[prevTrackPointId].electricity["frequency"] ?? lineVoltage;

				Vehicle.SetProperty("line_voltage", lineVoltage);
				Vehicle.SetProperty("line_frequency", lineFrequency);

			}
		}
	}
}
