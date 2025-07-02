using UnityEngine;

public class BlowingLeavesEffectSettings : MonoBehaviour
{
	public float smallEmitPerFrame = 0.2f;

	public float smallSizeMultiplier = 0.25f;

	public float mediumEmitPerFrame = 0.2f;

	public float mediumSizeMultiplier = 0.5f;

	public float lifetimeRandomFrom = 4f;

	public float lifetimeRandomTo = 6f;

	public float sizeRandomFrom = 0.9f;

	public float sizeRandomTo = 1.1f;

	public float speedMultiplierPerIntensity = 0.4f;

	public float speedMultiplierPerIntensityBias = 0.6f;

	public float speedRandomFrom = 0.4f;

	public float speedRandomTo = 1f;
}
