using UnityEngine;

public static class NormalizedInput
{
	public static Vector3 mousePosition => Input.mousePosition / NormalizedScreen.scale;
}
