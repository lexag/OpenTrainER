using System;

namespace OpenTrainER.source.vehicle.component
{
    internal class DriverComponent : VehicleComponent
    {
        double cross_sectional_area = 0;
        double drag_coefficient = 0;
        double rolling_resistance_coefficient = 0;
        double mass = 10000;

        protected override void OnInit()
        {
            Vehicle.InitProperty("speed");
            Vehicle.InitProperty("gradient");
        }


        protected override void OnTick(double delta)
        {
            const double airDensity = 1.293;
            const double gravity = 9.81;

            double speed = Vehicle.GetProperty("speed");
            double gradient = Vehicle.GetProperty("gradient");
            
            // Air resistance
            double dragForce = 0.5 * airDensity * speed * speed * drag_coefficient * cross_sectional_area;

            // Rolling resistance
            double rollingResistance = rolling_resistance_coefficient * mass * gravity * Math.Cos(Math.Atan(gradient));

            // Gradient gravity
            double gradientGravity = mass * gravity * Math.Sin(Math.Atan(gradient));

            double totalForce = dragForce * rollingResistance * gradientGravity;

            Vehicle.ChangeProperty("speed", -totalForce / mass * delta);
        }
    }
}
