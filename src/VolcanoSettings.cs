using UnityEngine;

public class VolcanoSettings : MonoBehaviour
{
	public Vector3 relativeOffset = new Vector3(-0.05f, 0.3f, -0.12f);

	public float smokePerFrame = 0.07f;

	public float smokeLifetimeRandomFrom = 0.4f;

	public float smokeLifetimeRandomTo = 0.5f;

	public float smokeAngularVelocityRandomFrom = -200f;

	public float smokeAngularVelocityRandomTo = 200f;

	public float smokeRotationRandomFrom;

	public float smokeRotationRandomTo = 360f;

	public float smokeSizeRandomFrom = 0.4f;

	public float smokeSizeRandomTo = 0.5f;

	public float smokeSizeBias = 0.2f;

	public float firePerFrame = 0.012f;

	public float fireSizeRandomFrom = 0.04f;

	public float fireSizeRandomTo = 0.07f;

	public float fireLifetimeRandomFrom = 0.5f;

	public float fireLifetimeRandomTo = 0.75f;
}
