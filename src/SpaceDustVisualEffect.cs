using UnityEngine;

public class SpaceDustVisualEffect : EmissionVisualEffect
{
	private static GameObject dust;

	private static ParticleSystem particles;

	private const int MAX_PARTICLES = 200;

	private Transform transform;

	private Vector3 size;

	private Vector3 localOffset = Vector3.zero;

	private bool enabled = true;

	public SpaceDustVisualEffect(string name)
		: base(name)
	{
	}

	public override void Stop()
	{
		if (particles != null)
		{
			particles.Clear();
			if (particles.isPaused)
			{
				particles.Play();
			}
		}
	}

	public override void Pause()
	{
		base.Pause();
		if (particles != null)
		{
			particles.Pause();
		}
	}

	public override void Resume()
	{
		base.Resume();
		if (particles != null)
		{
			particles.Play();
		}
	}

	public override void Begin()
	{
		base.Begin();
		if (dust == null)
		{
			dust = Object.Instantiate(Resources.Load("Env Effect/Space Dust Particle System")) as GameObject;
			particles = dust.GetComponent<ParticleSystem>();
		}
		paused = false;
		size = block.GetEffectSize();
		transform = block.goT;
		localOffset = block.GetEffectLocalOffset();
		timeLength = 100f;
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (paused)
		{
			return;
		}
		if (HasEnded())
		{
			if (particles.particleCount == 0)
			{
				Destroy();
			}
		}
		else
		{
			if (transform == null || !enabled)
			{
				return;
			}
			int particleCount = particles.particleCount;
			if (particleCount >= 200)
			{
				return;
			}
			float num = 1f - (float)particleCount / 200f;
			float num2 = num * Mathf.Min((size.x * size.y + size.x * size.z + size.y * size.z) * 0.1f, 5f);
			Vector3 vector = transform.TransformDirection(localOffset);
			while (num2 > 0f && (!(num2 < 1f) || !(Random.value > num2)))
			{
				num2 -= 1f;
				Vector3 vector2 = size;
				Vector3 cameraPosition = Blocksworld.cameraPosition;
				Vector3 vector3 = transform.position + vector;
				float num3 = (vector3 - cameraPosition).magnitude - vector2.magnitude;
				if (num3 <= Blocksworld.fogEnd)
				{
					Vector3 vector4 = vector3 + 20f * Random.Range(0.6f, 1f) * vector2.magnitude * Random.onUnitSphere;
					if (!(vector4 == Vector3.zero) && (num3 <= 15f || GeometryUtility.TestPlanesAABB(Blocksworld.frustumPlanes, new Bounds(vector4, Vector3.one))))
					{
						float num4 = Random.Range(0.2f, 0.4f);
						float y = Random.Range(-0.001f, 0.001f);
						Vector3 velocity = new Vector3(0f, y, 0f);
						float num5 = num4;
						float lifetime = 10f;
						Color white = Color.white;
						white.a = 0.6f;
						particles.Emit(vector4, velocity, num5, lifetime, white);
					}
				}
			}
		}
	}
}
