using UnityEngine;

public class LaserMetaData : MonoBehaviour
{
	public Vector3 exitOffset;

	public Vector3 projectileExitOffset;

	public string loopSfx = "Laser Beam Loop";

	public string pulseSfx = "Laser Pulse";

	public string projectileSfx = "Gun Fire";

	public float beamSizeMultiplier = 1f;

	public float pulseSizeMultiplier = 1f;

	public float projectileSizeMultiplier = 1f;

	public float pulseFrequencyMultiplier = 1f;

	public float pulseLengthMultiplier = 1f;

	public bool fixLaserColor;

	public Color laserColor;

	public float projectileRecoilForce = 1f;

	public float projectileHitForce = 2f;

	public float projectileMaxRecoilPerMass = 0.3f;
}
