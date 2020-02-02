using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtil {

	public static float Remap(float val, float from1, float to1, float from2, float to2)
	{
		return (val - from1) / (to1 - from1) * (to2 - from2) + from2;
	}

	public static float RemapFrom01(float val, float from2, float to2)
	{
		return from2 + val * (to2 - from2);
	}

	public static float RemapTo01(float val, float from1, float to1)
	{
		return (val - from1) / (to1 - from1);
	}

	public static float RemapClamped(float val, float from1, float to1, float from2, float to2)
	{
		return Mathf.Clamp(Remap(val, from1, to1, from2, to2), from2, to2);
	}

	public static float LerpUnclamped(float from, float to, float tUnclamped)
	{
		return from + (to - from) * tUnclamped;
	}

	//distribution power must be > 0. 1 is uniform, lower weights towards max, higher weights towards min. if random value override is provided, it should be in range [0,1]
	public static float RandomRangeWeighted(float min, float max, float distributionPower, float randomValueOverride = -1)
	{
		float randVal = randomValueOverride > 0f ? randomValueOverride : Random.value;
		return min + (max - min) * Mathf.Pow(randVal, distributionPower);
	}

	public static int RandomWeightedIdx(List<int> weights)
	{
		int cumulativeWeight = 0;
		foreach (int weight in weights)
		{
			cumulativeWeight += weight;
		}

		int rand = Random.Range(0, cumulativeWeight);
		int weightSoFar = 0;
		for (int i=0; i<weights.Count; i++)
		{
			int weight = weights[i];
			weightSoFar += weight;
			if (rand < weightSoFar)
			{
				return i;
			}
		}

		throw new UnityException("RandomWeightedIdx function broke, this should be unreachable.");
	}
}
