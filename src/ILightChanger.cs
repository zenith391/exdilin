using UnityEngine;

public interface ILightChanger
{
	Color GetDynamicalLightTint();

	float GetFogMultiplier();

	Color GetFogColorOverride();

	float GetLightIntensityMultiplier();
}
