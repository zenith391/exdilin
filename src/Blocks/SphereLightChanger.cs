using UnityEngine;

namespace Blocks;

public class SphereLightChanger : LightChanger
{
	public Color color = Color.white;

	public Vector3 position;

	public float radius = 50f;

	public float innerRadius = 20f;

	public override Color GetLightTint(Vector3 pos)
	{
		Color result = Color.white;
		float magnitude = (pos - position).magnitude;
		if (magnitude < radius)
		{
			float num = 1f / (radius - innerRadius);
			float num2 = Mathf.Clamp(num * (magnitude - innerRadius), 0f, 1f);
			result = Color.white * num2 + color * (1f - num2);
		}
		return result;
	}
}
