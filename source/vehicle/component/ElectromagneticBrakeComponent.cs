using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTrainER.source.vehicle.component
{
    internal class ElectromagneticBrakeComponent : VehicleComponent
    {
        public double max_efficiency_speed;
        public double max_brake_force;

        protected override void OnInit()
        {
            base.OnInit();
        }

        protected override void OnTick(double delta)
        {
            base.OnTick(delta);

            double brakingForce = max_brake_force * Mathf.Min(Vehicle.GetProperty("speed") / max_efficiency_speed, 1) * Vehicle.GetProperty("controls:electromagnetic_brake");

            Vehicle.ChangeProperty("braking_force", brakingForce);
        }
    }
}
