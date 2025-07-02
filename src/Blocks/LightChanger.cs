using UnityEngine;

namespace Blocks;

public class LightChanger
{
	public virtual Color GetLightTint(Vector3 pos)
	{
		return Color.white;
	}
}
