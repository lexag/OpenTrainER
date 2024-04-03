using Godot;
using System;

public static class TrackInterpolation
{
	public static Vector3 GetPositionFromKey(TrackNode a, TrackNode b, float t)
	{
		float tangentStrength = a.LocalCoordinate.DistanceTo(b.LocalCoordinate) / 3;
		Vector3 pA = a.LocalCoordinate;
		Vector3 pB = b.LocalCoordinate;
		Vector3 cA = a.tangentVector * tangentStrength * TangentDirectionFactor(a, b);
		Vector3 cB = b.tangentVector * tangentStrength * TangentDirectionFactor(a, b);

		return CubicBezier(
			pA,
			pA + cA,
			pB - cB,
			pB,
			t);
	}



	private static int TangentDirectionFactor(TrackNode a, TrackNode b)
	{
		Vector3 pA = a.LocalCoordinate;
		Vector3 pB = b.LocalCoordinate;
		Vector3 cA = a.tangentVector;

		return (cA.AngleTo(pB - pA) >= Math.PI / 2) ? -1 : 1;
	}

	public static Vector3 GetForwardVectorFromKey(TrackNode a, TrackNode b, float t)
	{
		float e = 0.01f;
		if (t + e >= 1)
		{
			return a.tangentVector * TangentDirectionFactor(a, b);
		}
		if (t - e <= 0)
		{
			return b.tangentVector * TangentDirectionFactor(a, b);
		}
		return (GetPositionFromKey(a, b, t + e) - GetPositionFromKey(a, b, t - e)).Normalized();
	}

	public static double GetLengthOfSegment(TrackNode a, TrackNode b)
	{
		double sum = 0;
		float e = 0.05f;
		for (float i = 0; i < 1; i += e)
		{
			sum += GetPositionFromKey(a, b, i).DistanceTo(GetPositionFromKey(a, b, i + e));
		}
		return sum;
	}

	public static float GetClosestKey(TrackNode a, TrackNode b, Vector3 position, float precision = 0.01f)
	{
		double distanceRecord = double.MaxValue;
		float recordHolder = 0;
		for (float i = 0; i < 1; i += precision)
		{
			double d = GetPositionFromKey(a, b, i).DistanceTo(position);
			if (d < distanceRecord)
			{
				distanceRecord = d;
				recordHolder = i;
			}
		}
		return recordHolder;
	}


	public static Vector3[] SampleEquidistantPoints(TrackNode a, TrackNode b, int numPoints)
	{
		Vector3[] list = new Vector3[numPoints];
		float[] keys = SampleEquidistantKeys(a, b, numPoints);

		for (int i = 0; i < numPoints; i++)
		{
			list[i] = GetPositionFromKey(a, b, keys[i]);
		}
		return list;
	}

	public static float[] SampleEquidistantKeys(TrackNode a, TrackNode b, int numPoints)
	{
		if (numPoints < 2)
		{
			throw new ArgumentException("numPoints must be greater or equal to 2.");
		}
		if (numPoints == 2)
		{
			return new float[2] { 0f, 1f };
		}

		if (numPoints < 10)
		{
			return ResampleFloatArray(SampleEquidistantKeys(a, b, 100), numPoints);
		}

		// (https://gamedev.stackexchange.com/a/5427)
		Vector3 origin = a.LocalCoordinate;
		float clen = 0;
		float[] arcLengths = new float[numPoints+1];
		for (var i = 1; i < numPoints+1; i += 1)
		{
			Vector3 p = GetPositionFromKey(a, b, i * 1f / numPoints);
			Vector3 delta = origin - p;
			clen += delta.Length();
			arcLengths[i] = clen;
			origin = p;
		}

		int n = 0;
		float[] list = new float[numPoints];
		float totalLength = arcLengths[numPoints - 1];

        for (float targetLength = 0; targetLength <= totalLength && n < numPoints; targetLength += totalLength / numPoints)
		{
			float low = 0, high = numPoints;
			int index = 0;
			while (low < high)
			{
				index = (int)(low + ((high - low) / 2));
				if (arcLengths[index] < targetLength)
				{
					low = index + 1;

				}
				else
				{
					high = index;
				}
			}
			if (arcLengths[index] > targetLength)
			{
				index--;
			}

			var lengthBefore = arcLengths[index];
			if (lengthBefore == targetLength)
			{
				
				list[n] = (float)index / numPoints;

			}
			else
			{
				list[n] = (float)(index + (float)(targetLength - lengthBefore) / (arcLengths[index + 1] - lengthBefore)) / numPoints;
			}
			n++;
		}

		return list;

        //float[] list = new float[numPoints];

        //float distance = (float)GetLengthOfSegment(a, b);
        //float actualSegmentLength = distance / (numPoints - 1);

        //list[0] = 0;
        //float t = 0;
        //for (int i = 1; i < numPoints - 1; i++)
        //{
        //	Vector3 cursor = GetPositionFromKey(a, b, t);
        //	Vector3 direction = GetForwardVectorFromKey(a, b, t).Normalized();
        //	cursor += direction * actualSegmentLength;
        //	t = GetClosestKey(a, b, cursor, 0.1f/numPoints);

        //	list[i] = t;
        //}
        //list[numPoints - 1] = 1;
        //return list;
    }



	static private Vector3 CubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
	{
		Vector3 q0 = p0.Lerp(p1, t);
		Vector3 q1 = p1.Lerp(p2, t);
		Vector3 q2 = p2.Lerp(p3, t);

		Vector3 r0 = q0.Lerp(q1, t);
		Vector3 r1 = q1.Lerp(q2, t);

		Vector3 s = r0.Lerp(r1, t);
		return s;
	}

	// https://stackoverflow.com/a/53584079
	static private float[] ResampleFloatArray(float[] source, int newSize)
	{
		int m = source.Length; //source length
		float[] destination = new float[newSize];
		destination[0] = source[0];
		destination[newSize - 1] = source[m - 1];

		for (int i = 1; i < newSize - 1; i++)
		{
			float jd = ((float)i * (float)(m - 1) / (float)(newSize - 1));
			int j = (int)jd;
			destination[i] = source[j] + (source[j + 1] - source[j]) * (jd - (float)j);
		}
		return destination;
	}
}
