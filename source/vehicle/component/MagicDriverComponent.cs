using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTrainER.source.vehicle.component
{
    internal class MagicDriverComponent : DriverComponent
    {
        protected override void OnTick(double delta)
        {
            base.OnTick(delta);

            double throttle = Vehicle.GetProperty("controls:throttle");
            Vehicle.ChangeProperty("speed", throttle * delta);
        }
    }
}
