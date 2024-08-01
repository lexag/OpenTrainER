using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTrainER.source.vehicle.component
{
    internal class PneumaticBrakeComponent : VehicleComponent
    {
        public double pipe_fill_constant;
        public double pipe_clear_constant;
        public double pipe_max_pressure;
        public double pipe_min_acting_pressure;
        public double cylinder_fill_constant;
        public double cylinder_clear_constant;
        public double cylinder_max_pressure;
        public double max_brake_force;
        public double feed_line_max_pressure;
        public double feed_line_fill_speed;
        public double feed_line_reservoir_ratio;

        protected override void OnInit()
        {
            base.OnInit();
            Vehicle.InitProperty("brake_pipe_pressure", pipe_max_pressure);
            Vehicle.InitProperty("brake_cylinder_pressure", 0);
            Vehicle.InitProperty("brake_feed_pressure", feed_line_max_pressure);
            Vehicle.InitProperty("signal:emergency_brake", 0);
        }

        protected override void OnTick(double delta)
        {
            base.OnTick(delta);


            double pipePressure = Vehicle.GetProperty("brake_pipe_pressure");
            double cylinderPressure = Vehicle.GetProperty("brake_cylinder_pressure");
            double feedPressure = Vehicle.GetProperty("brake_feed_pressure");

            // Brake Cylinder
            // If pipe is full => empty cylinder
            if (pipePressure >= pipe_max_pressure * 0.99)
            {
                cylinderPressure -= cylinderPressure * cylinder_clear_constant * delta;
            }
            // If pipe is not full => fill cylinder
            else
            {
                // feed as normal
                double deltaPressure = (cylinder_max_pressure - cylinderPressure) * cylinder_fill_constant * delta;
                // feed speed dependent on pipe pressure drop
                deltaPressure *= (1 - (Math.Max(pipePressure, pipe_min_acting_pressure) - pipe_min_acting_pressure) / (pipe_max_pressure - pipe_min_acting_pressure));

                if (feed_line_max_pressure > 1)
                {
                    // feed from reservoir/feed line
                    // if feed line low, no brakes!
                    deltaPressure *= (feedPressure - cylinderPressure) / feed_line_max_pressure;
                }
                cylinderPressure += deltaPressure;
            }


            // Brake Pipe
            // Driver control and emergency brake
            double wantedPressure = Mathf.Lerp(pipe_max_pressure, pipe_min_acting_pressure, Math.Max(Vehicle.GetProperty("controls:automatic_air_brake"), 0));
            if (Vehicle.GetProperty("signal:emergency_brake") > 0.5)
            {
                wantedPressure = 0;
            }

            // Fill pipe
            if (wantedPressure > pipePressure)
            {
                // If feed line exists and is high, feed from feed line and drain it by reservoir size ratio
                if (feedPressure > pipePressure)
                {
                    pipePressure += 1 * pipe_fill_constant * delta * (feedPressure - pipePressure);
                    feedPressure -= 1 / feed_line_reservoir_ratio * pipe_fill_constant * delta * (feedPressure - pipePressure);
                }
                // else feed direct from compressor
                else
                {
                    pipePressure += pipe_fill_constant * delta * pipe_max_pressure;
                }
            }
            // Drain pipe
            else
            {
                pipePressure += (wantedPressure - pipePressure) * pipe_clear_constant * delta;
            }

            // Recharge feed line pressure
            if (feedPressure < feed_line_max_pressure)
            {
                feedPressure += feed_line_fill_speed * delta;
            }

            Vehicle.SetProperty("brake_pipe_pressure", pipePressure);
            Vehicle.SetProperty("brake_cylinder_pressure", cylinderPressure);
            Vehicle.SetProperty("brake_feed_pressure", feedPressure);

            double brakingForce = max_brake_force * (cylinderPressure / cylinder_max_pressure);

            Vehicle.ChangeProperty("braking_force", brakingForce);
        }
    }
}
