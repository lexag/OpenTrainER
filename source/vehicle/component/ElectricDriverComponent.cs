﻿//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;


using System;
using System.Data;

namespace OpenTrainER.source.vehicle.component
{
    internal class ElectricDriverComponent : DriverComponent
    {
        public double power;
        public double starting_force;

        protected override void OnInit()
        {
            base.OnInit();
            Vehicle.InitProperty("motor_current");
        }

        protected override void OnTick(double delta)
        {
            base.OnTick(delta);

            double speed = Math.Abs(Vehicle.GetProperty("speed"));
            double throttle = Math.Max(Vehicle.GetProperty("controls:throttle"), 0);
            double force = Math.Min(starting_force, power/Math.Max(speed, 0.0000001));
            force *= throttle;

            Vehicle.ChangeProperty("traction_force", force);

            if (Vehicle.GetProperty("status:wheelslip") > 0.5)
            {
                Vehicle.SetProperty("motor_current", 0);
            }
            else
            {
                Vehicle.SetProperty("motor_current", speed * force / Vehicle.GetProperty("line_voltage"));
            }
        }
    }
}
