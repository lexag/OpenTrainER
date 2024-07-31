//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;


//namespace OpenTrainER.source.vehicle.component
//{
//    internal class ElectricDriverComponent : DriverComponent
//    {
//        Dictionary<int, double> power_curve;

//        protected override void OnInit()
//        {
//            base.OnInit();
//        }

//        protected override void OnTick(double delta)
//        {
//            base.OnTick(delta);

//            int over;
//            double overValue;
//            int under;
//            double underValue;

//            foreach (var powerPoint in power_curve)
//            {
//                if (powerPoint.Key >= Vehicle.GetProperty("speed"))
//                {
//                    over = powerPoint.Key;
//                    underValue = powerPoint.Value;
//                    break:
//   }
//                fpUnder = fp;
//            }

//            Vehicle.SetProperty("traction_force", power_curve);
//        }
//    }
//}
