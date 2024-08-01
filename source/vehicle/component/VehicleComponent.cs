using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTrainER.source.vehicle.component
{
	internal class VehicleComponent
	{
		public void Init()
		{
			OnInit();
		}

		protected virtual void OnInit() { }

		public void Tick(double delta)
		{
			OnTick(delta);
		}

		protected virtual void OnTick(double delta) { }
	}
}
