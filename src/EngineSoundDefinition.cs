using System;

[Serializable]
public class EngineSoundDefinition
{
	public string name = "Default";

	public string loopSFXName = "Wheel Engine Loop Default";

	public float baseVolume = 0.6f;

	public float basePitch = 0.7f;

	public float RPMIncreaseSpeed = 0.5f;

	public float RPMDecaySpeed = 0.9f;

	public float RPMVolumeMod = 0.2f;

	public float RPMPitchMod = 0.01f;

	public float maximumRPM = 200f;

	public float wheelSpinThreshold = 0.1f;

	public float wheelSpinMax = 20f;

	public float wheelSpinVolumeMod;

	public float wheelSpinPitchMod;
}
