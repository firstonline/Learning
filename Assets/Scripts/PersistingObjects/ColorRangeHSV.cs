using UnityEngine;

[System.Serializable]
public class ColorRangeHSV
{
	[FloatRangeSlider(0f, 1f)]
	public FloatRange Hue, Saturation, Value;

	public Color RandomInRange
	{
		get
		{
			return Random.ColorHSV(Hue.min, Hue.max, Saturation.min, Saturation.max, Value.min, Value.max, 1f, 1f);
		}
	}

}
