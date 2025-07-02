using UnityEngine;

internal static class RectExtensions
{
	public static bool Intersects(this Rect r1, Rect r2)
	{
		if (r2.xMin <= r1.xMax && r2.xMax >= r1.xMin && r2.yMin <= r1.yMax)
		{
			return r2.yMax >= r1.yMin;
		}
		return false;
	}
}
