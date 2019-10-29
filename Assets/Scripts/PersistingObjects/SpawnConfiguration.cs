using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnConfiguration 
{
	public enum SpawnMovementDirection
	{
		Forward,
		Upward,
		OutWard,
		Random,
	}
	
	
	public SpawnMovementDirection spawnMovementDirection;
	public FloatRange spawnSpeed;
	public FloatRange angularSpeed;
	public FloatRange scale;
	public ColorRangeHSV color;
	public bool UniformColor;
}
