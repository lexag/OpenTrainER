using System;


namespace OpenTrainER.source.vehicle.component
{
    internal class DriverComponent : VehicleComponent
    {
        public double cross_sectional_area;
        public double drag_coefficient;
        public double rolling_resistance_coefficient;
        public double mass;
        public double equivalent_mass;

        protected override void OnInit()
        {
            Vehicle.InitProperty("speed");
            Vehicle.InitProperty("gradient");
            Vehicle.InitProperty("traction_force");
            Vehicle.InitProperty("status:wheelslip");
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
            double rollingResistanceForce = rolling_resistance_coefficient * mass * gravity * Math.Cos(Math.Atan(gradient)) * Math.Sign(speed);

            // Gradient gravity
            double gradientGravityForce = mass * gravity * Math.Sin(Math.Atan(gradient));

            // Traction
            double tractionForce = Vehicle.GetProperty("traction_force");
            double maxTractionForce = Environment.wheelRailFrictionCoefficient * mass * gravity;

            if (tractionForce > maxTractionForce) 
            {
                tractionForce = 0;
                Vehicle.SetProperty("status:wheelslip", 1);
            }
            else
            {
                Vehicle.SetProperty("status:wheelslip", 0);
            }

            double slowingSpeedChange = (-dragForce - rollingResistanceForce - gradientGravityForce) / equivalent_mass * delta;
            double acceleratingSpeedChange = tractionForce / mass * delta;

            Vehicle.ChangeProperty("speed", acceleratingSpeedChange + slowingSpeedChange);
        }
    }
}
