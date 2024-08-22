using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace OpenTrainER.source
{
	static internal class Util
	{
		public static Vector3 ToVector(float[] array)
		{
			return new Vector3(array[0], array[1], array[2]);
		}

		public static Vector3 ToVector(double[] array)
		{
			return new Vector3((float)array[0], (float)array[1], (float)array[2]);
		}
	}
}
