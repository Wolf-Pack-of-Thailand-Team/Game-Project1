using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Planetary {

public static class VoronoiNoise {
	public enum DistanceFunction {
		EUCLIDIAN, MANHATTAN, CHEBYSHEV
	}
	public enum CombineFunction {
		d0, d1, d2
	}

	public static float Noise3D(Vector3 input, float seed, 
								DistanceFunction distanceFunction = DistanceFunction.EUCLIDIAN, 
								CombineFunction combineFunction = CombineFunction.d0) {
		uint lastRandom, numberFeaturePoints;
		Vector3 randomDiff = Vector3.zero;
		Vector3 featurePoint = Vector3.zero;
		int cubeX, cubeY, cubeZ;
		
		float[] distanceArray = new float[3];
		for (int i = 0; i < distanceArray.Length; i++)
			distanceArray[i] = Mathf.Infinity;

		int evalCubeX = Mathf.FloorToInt(input.x);
		int evalCubeY = Mathf.FloorToInt(input.y);
		int evalCubeZ = Mathf.FloorToInt(input.z);

		for (int i = -1; i < 2; ++i) {
			for (int j = -1; j < 2; ++j) {
				for (int k = -1; k < 2; ++k) {
					cubeX = evalCubeX + i;
					cubeY = evalCubeY + j;
					cubeZ = evalCubeZ + k;

					lastRandom = lcgRandom(hash((uint)(cubeX + seed), (uint)(cubeY), (uint)(cubeZ)));
				
					numberFeaturePoints = 1;

					for (uint l = 0; l < numberFeaturePoints; ++l) {
						lastRandom = lcgRandom(lastRandom);
						randomDiff.x = (float)lastRandom / 0x100000000;

						lastRandom = lcgRandom(lastRandom);
						randomDiff.y = (float)lastRandom / 0x100000000;

						lastRandom = lcgRandom(lastRandom);
						randomDiff.z = (float)lastRandom / 0x100000000;

						featurePoint.x = randomDiff.x + (float)cubeX;
						featurePoint.y = randomDiff.y + (float)cubeY;
 						featurePoint.z = randomDiff.z + (float)cubeZ;

						switch(distanceFunction) {
						case DistanceFunction.EUCLIDIAN:
							insert(distanceArray, EuclidianDistanceFunc3(input, featurePoint));
							break;
						case DistanceFunction.MANHATTAN:
							insert(distanceArray, ManhattanDistanceFunc3(input, featurePoint));
							break;
						case DistanceFunction.CHEBYSHEV:
							insert(distanceArray, ChebyshevDistanceFunc3(input, featurePoint));
							break;
						}
					}
				}
			}
		}
		
		float combine = 0f;
		switch(combineFunction) {
		case CombineFunction.d0:
			combine = CombineFunc_D0(distanceArray);
			break;
		case CombineFunction.d1:
			combine = CombineFunc_D1_D0(distanceArray);
			break;
		case CombineFunction.d2:
			combine = CombineFunc_D2_D0(distanceArray);
			break;
		}
		return Mathf.Clamp(combine * 2f - 1f, -1f, 1f);
	}

	private static float EuclidianDistanceFunc3(Vector3 p1, Vector3 p2) {
		return (p1.x - p2.x) * (p1.x - p2.x) + (p1.y - p2.y) * (p1.y - p2.y) + (p1.z - p2.z) * (p1.z - p2.z);
	}
	
	private static float ManhattanDistanceFunc3(Vector3 p1, Vector3 p2) {
		return Mathf.Abs(p1.x - p2.x) + Mathf.Abs(p1.y - p2.y) + Mathf.Abs(p1.z - p2.z);
	}

	private static float ChebyshevDistanceFunc3(Vector3 p1, Vector3 p2) {
		Vector3 diff = p1 - p2;
		return Mathf.Max(Mathf.Max(Mathf.Abs(diff.x), Mathf.Abs(diff.y)), Mathf.Abs(diff.z));
	}
	
	private static float CombineFunc_D0(float[] arr) { 
		float value = 0f;
		for(int i = 0; i < arr.Length; i++)
			value += arr[i];
		
		return arr[0];
	}

	private static float CombineFunc_D1_D0(float[] arr) { 
		return arr[1]-arr[0]; 
	}

	private static float CombineFunc_D2_D0(float[] arr) { 
		return arr[2]-arr[0]; 
	}

	private static void insert(float[] arr, float value) {
		float temp;
		for (int i = arr.Length - 1; i >= 0; i--)
		{
			if (value > arr[i]) break;
			temp = arr[i];
			arr[i] = value;
			if (i + 1 < arr.Length) arr[i + 1] = temp;
		}
	}

	private static uint lcgRandom(uint lastValue) {
		return (uint)((1103515245u * lastValue + 12345u) % 0x100000000u);
	}

	private const uint OFFSET_BASIS = 2166136261;
	private const uint FNV_PRIME = 16777619;

	private static uint hash(uint i, uint j, uint k) {
		return (uint)((((((OFFSET_BASIS ^ (uint)i) * FNV_PRIME) ^ (uint)j) * FNV_PRIME) ^ (uint)k) * FNV_PRIME);
	}
	
	private static uint hash(uint i, uint j) {
		return (uint)((((OFFSET_BASIS ^ (uint)i) * FNV_PRIME) ^ (uint)j) * FNV_PRIME);
	}
}

}