using UnityEngine;

public class AngleXYVisualizer : AngleVisualizer
{
	public AngleXYVisualizer(float sign = 1f)
		: base(sign)
	{
	}

	protected override Quaternion GetArrowRotation(float angle, Transform blockT)
	{
		return blockT.rotation * Quaternion.Euler(90f, sign * angle, 0f);
	}
}
