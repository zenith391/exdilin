using UnityEngine;

public class SandStormEffect : WeatherEffect
{
	private GameObject sandGo;

	private ParticleSystem dustPs;

	private ParticleSystem rubblePs;

	private Vector3 prevCamPos = Vector3.zero;

	private Vector3 camVelocity = Vector3.zero;

	private float dustToEmit;

	private float rubbleToEmit;

	private float avgSpawnDiffY;

	private SandStormEffectSettings settings;

	public SandStormEffect()
	{
		loopSfx = "Sandstorm Loop";
	}

	public override void Pause()
	{
		base.Pause();
		if (dustPs != null)
		{
			dustPs.Pause();
		}
		if (rubblePs != null)
		{
			rubblePs.Pause();
		}
	}

	public override void Resume()
	{
		base.Resume();
		if (dustPs != null)
		{
			dustPs.Play();
		}
		if (rubblePs != null)
		{
			rubblePs.Play();
		}
	}

	protected override float GetTargetVolume()
	{
		float num = 1f / (1f + 0.5f * avgSpawnDiffY);
		return num * base.GetTargetVolume();
	}

	public override void SetEffectAngle(float angle)
	{
		base.SetEffectAngle(angle);
		sandGo.transform.rotation = Quaternion.Euler(0f, angle, 0f);
	}

	private void CreateParticleSystem()
	{
		if (sandGo == null)
		{
			sandGo = Object.Instantiate(Resources.Load("Env Effect/Sand Storm Particle Systems")) as GameObject;
		}
		if (sandGo != null)
		{
			settings = sandGo.GetComponent<SandStormEffectSettings>();
			dustPs = sandGo.transform.Find("Sand Storm Dust Particle System").gameObject.GetComponent<ParticleSystem>();
			rubblePs = sandGo.transform.Find("Sand Storm Rubble Particle System").gameObject.GetComponent<ParticleSystem>();
		}
		Stop();
	}

	public override void FogChanged()
	{
		if (sandGo != null)
		{
			Color fogColor = Blocksworld.fogColor;
			Material material = dustPs.GetComponent<Renderer>().material;
			Material material2 = rubblePs.GetComponent<Renderer>().material;
			material.SetColor("_FogColor", fogColor);
			material2.SetColor("_FogColor", fogColor);
			float value = Blocksworld.fogStart * Blocksworld.fogMultiplier;
			float value2 = Blocksworld.fogEnd * Blocksworld.fogMultiplier;
			material.SetFloat("_FogStart", value);
			material.SetFloat("_FogEnd", value2);
			material2.SetFloat("_FogStart", value);
			material2.SetFloat("_FogEnd", value2);
		}
	}

	public override void Update()
	{
		base.Update();
		if (!(sandGo != null) || paused)
		{
			return;
		}
		Vector3 cameraPosition = Blocksworld.cameraPosition;
		Vector3 vector = Blocksworld.blocksworldCamera.GetTargetPosition() - cameraPosition;
		float magnitude = vector.magnitude;
		if (!(magnitude > 0.01f))
		{
			return;
		}
		Vector3 vector2 = cameraPosition - prevCamPos;
		float smoothDeltaTime = Time.smoothDeltaTime;
		if (smoothDeltaTime > 0.001f)
		{
			camVelocity = 0.9f * camVelocity + 0.1f * (vector2 / smoothDeltaTime);
			camVelocity.y = 0f;
		}
		prevCamPos = cameraPosition;
		dustToEmit += (Random.Range(settings.dustEmitRandomFrom, settings.dustEmitRandomTo) + settings.dustEmitBias) * intensityMultiplier;
		rubbleToEmit += (Random.Range(settings.rubbleEmitRandomFrom, settings.rubbleEmitRandomTo) + settings.rubbleEmitBias) * intensityMultiplier;
		Vector3 vector3 = vector / magnitude;
		float num = Mathf.Min(magnitude * 0.5f, 30f);
		Vector3 vector4 = camVelocity * 0.5f;
		if (vector4.magnitude > 15f)
		{
			vector4 = vector4.normalized * 15f;
		}
		Vector3 centerPos = cameraPosition + vector3 * num + vector4;
		int num2 = 0;
		for (int i = 0; (float)i < rubbleToEmit; i++)
		{
			num2++;
			float num3 = Mathf.Min(1.2f, intensityMultiplier * settings.rubbleSpeedFractionPerIntensity + settings.rubbleSpeedFractionBias) * Random.Range(settings.rubbleSpeedRandomFrom, settings.rubbleSpeedRandomTo);
			Vector3 vector5 = CalculateEmitReferencePoint(centerPos, num3, 1f);
			Vector3 spawnPoint = vector5 + sandGo.transform.right * Random.Range(-30f, 30f) + Vector3.up * Random.Range(0f, 3f);
			float lifetime = Random.Range(settings.rubbleLifetimeRandomFrom, settings.rubbleLifetimeRandomTo);
			float size = Random.Range(settings.rubbleSizeRandomFrom, settings.rubbleSizeRandomTo);
			float y = spawnPoint.y;
			GetGoodSpawnPoint(size, num3, ref spawnPoint, ref lifetime);
			float magnitude2 = (spawnPoint - cameraPosition).magnitude;
			if (magnitude2 < 200f && lifetime > 0.4f)
			{
				float num4 = spawnPoint.y - y;
				float num5 = 0f;
				if (num4 <= 5f)
				{
					num5 = num3 * Random.Range(0.1f, 0.3f);
				}
				float value = Random.value;
				Color color = value * settings.rubbleStartColorFrom + (1f - value) * settings.rubbleStartColorTo;
				Vector3 velocity = sandGo.transform.forward * num3 + Vector3.up * num5;
				ParticleSystem.Particle particle = new ParticleSystem.Particle
				{
					position = spawnPoint,
					velocity = velocity,
					size = size,
					startLifetime = lifetime,
					remainingLifetime = lifetime,
					color = new Color(color.r, color.g, color.b, Random.Range(settings.rubbleAlphaRandomFrom, settings.rubbleAlphaRandomTo)),
					randomSeed = (uint)Random.Range(1, 9999999)
				};
				rubblePs.Emit(particle);
			}
		}
		rubbleToEmit -= num2;
		int num6 = 0;
		for (int j = 0; (float)j < dustToEmit; j++)
		{
			num6++;
			float num7 = Mathf.Min(1.2f, intensityMultiplier * settings.dustSpeedFractionPerIntensity + settings.dustSpeedFractionBias) * Random.Range(settings.dustSpeedRandomFrom, settings.dustSpeedRandomTo);
			Vector3 vector6 = CalculateEmitReferencePoint(centerPos, num7);
			Vector3 spawnPoint2 = vector6 + sandGo.transform.right * Random.Range(-30f, 30f) + Vector3.up * Random.Range(0f, 3f);
			float lifetime2 = Random.Range(settings.dustLifetimeRandomFrom, settings.dustLifetimeRandomTo);
			float size2 = Random.Range(settings.dustSizeRandomFrom, settings.dustSizeRandomTo);
			float y2 = spawnPoint2.y;
			GetGoodSpawnPoint(size2, num7, ref spawnPoint2, ref lifetime2);
			avgSpawnDiffY = 0.9f * avgSpawnDiffY + 0.1f * Mathf.Abs(spawnPoint2.y - y2);
			float magnitude3 = (spawnPoint2 - cameraPosition).magnitude;
			if (magnitude3 < 200f && lifetime2 > 0.4f)
			{
				ParticleSystem.Particle particle2 = new ParticleSystem.Particle
				{
					position = spawnPoint2,
					velocity = sandGo.transform.forward * num7,
					rotation = Random.Range(0f, 360f),
					size = size2,
					remainingLifetime = lifetime2,
					startLifetime = lifetime2,
					randomSeed = (uint)Random.Range(1, 9999999)
				};
				float value2 = Random.value;
				Color color2 = value2 * settings.dustStartColorFrom + (1f - value2) * settings.dustStartColorTo;
				particle2.color = new Color(color2.r, color2.g, color2.b, Random.Range(settings.dustAlphaRandomFrom, settings.dustAlphaRandomTo));
				dustPs.Emit(particle2);
			}
		}
		dustToEmit -= num6;
	}

	private Vector3 CalculateEmitReferencePoint(Vector3 centerPos, float speed, float offsetMultiplier = 0.5f)
	{
		Vector3 result = centerPos - sandGo.transform.forward * speed * offsetMultiplier;
		GameObject worldOcean = Blocksworld.worldOcean;
		if (worldOcean != null && worldOcean.GetComponent<Collider>() != null)
		{
			Bounds bounds = worldOcean.GetComponent<Collider>().bounds;
			result.y = Mathf.Clamp(result.y, bounds.max.y + 5f, bounds.max.y + 10000f);
		}
		return result;
	}

	private void GetGoodSpawnPoint(float size, float speed, ref Vector3 spawnPoint, ref float lifetime)
	{
		if (Physics.Raycast(spawnPoint + Vector3.up * 30f, -Vector3.up, out var hitInfo, 45f))
		{
			Vector3 point = hitInfo.point;
			point.y = Mathf.Max(point.y + size * 0.5f, spawnPoint.y);
			spawnPoint = point;
			if (Physics.Raycast(spawnPoint, sandGo.transform.forward, out hitInfo, 45f))
			{
				lifetime = hitInfo.distance / speed;
			}
		}
	}

	public override void Start()
	{
		base.Start();
		CreateParticleSystem();
	}

	public override void Stop()
	{
		if (dustPs != null)
		{
			dustPs.Stop();
			dustPs.Clear();
		}
		if (rubblePs != null)
		{
			rubblePs.Stop();
			rubblePs.Clear();
		}
	}

	public override void Reset()
	{
		base.Reset();
		if (sandGo != null)
		{
			Object.Destroy(sandGo);
		}
		sandGo = null;
		dustPs = null;
		rubblePs = null;
	}

	public override float GetFogMultiplier()
	{
		return 1f - Mathf.Min(0.85f, 0.7f * intensityMultiplier);
	}
}
