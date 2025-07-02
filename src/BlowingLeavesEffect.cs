using UnityEngine;

public class BlowingLeavesEffect : WeatherEffect
{
	private static ParticleSystem smallLeavesPs;

	private static ParticleSystem mediumLeavesPs;

	private Vector3 prevCamPos = Vector3.zero;

	private Vector3 camVelocity = Vector3.zero;

	private float smallToEmit;

	private float mediumToEmit;

	private static BlowingLeavesEffectSettings settings;

	private Vector3 velDirectionVec = Vector3.forward;

	private GameObject leavesGo;

	private bool green;

	private float avgLeafSpeed;

	public BlowingLeavesEffect(bool green)
	{
		this.green = green;
		loopSfx = "Wind Loop";
	}

	public override void Pause()
	{
		base.Pause();
		if (smallLeavesPs != null)
		{
			smallLeavesPs.Pause();
		}
		if (mediumLeavesPs != null)
		{
			mediumLeavesPs.Pause();
		}
	}

	public override void Resume()
	{
		base.Resume();
		if (smallLeavesPs != null)
		{
			smallLeavesPs.Play();
		}
		if (mediumLeavesPs != null)
		{
			mediumLeavesPs.Play();
		}
	}

	protected override float GetTargetVolume()
	{
		float num = Mathf.Clamp(avgLeafSpeed * 0.1f, 0f, 1f);
		return num * base.GetTargetVolume();
	}

	public override void SetEffectAngle(float angle)
	{
		base.SetEffectAngle(angle);
		velDirectionVec = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
	}

	private void CreateParticleSystem()
	{
		if (leavesGo == null)
		{
			leavesGo = Object.Instantiate(Resources.Load("Env Effect/Blowing Leaves Particle System")) as GameObject;
		}
		if (leavesGo != null)
		{
			settings = leavesGo.GetComponent<BlowingLeavesEffectSettings>();
			if (smallLeavesPs == null)
			{
				smallLeavesPs = leavesGo.transform.Find("Small Leaves").gameObject.GetComponent<ParticleSystem>();
			}
			if (mediumLeavesPs == null)
			{
				mediumLeavesPs = leavesGo.transform.Find("Medium Leaves").gameObject.GetComponent<ParticleSystem>();
			}
		}
		Stop();
	}

	public override void Update()
	{
		base.Update();
		if (!(smallLeavesPs != null) || paused)
		{
			return;
		}
		Vector3 cameraPosition = Blocksworld.cameraPosition;
		Vector3 vector = Blocksworld.blocksworldCamera.GetTargetPosition() - cameraPosition;
		float magnitude = vector.magnitude;
		if (magnitude > 0.01f)
		{
			Vector3 vector2 = cameraPosition - prevCamPos;
			float smoothDeltaTime = Time.smoothDeltaTime;
			if (smoothDeltaTime > 0.001f)
			{
				camVelocity = 0.9f * camVelocity + 0.1f * (vector2 / smoothDeltaTime);
				camVelocity.y = 0f;
			}
			prevCamPos = cameraPosition;
			smallToEmit += settings.smallEmitPerFrame * intensityMultiplier;
			mediumToEmit += settings.mediumEmitPerFrame * intensityMultiplier;
			Vector3 vector3 = vector / magnitude;
			float num = Mathf.Min(magnitude * 0.5f, 30f);
			Vector3 vector4 = camVelocity * 0.5f;
			if (vector4.magnitude > 15f)
			{
				vector4 = vector4.normalized * 15f;
			}
			Vector3 centerPos = cameraPosition + vector3 * num + vector4;
			int num2 = 0;
			for (int i = 0; (float)i < mediumToEmit; i++)
			{
				num2++;
				EmitLeaf(cameraPosition, centerPos, mediumLeavesPs, settings.mediumSizeMultiplier, vector4);
			}
			mediumToEmit -= num2;
			int num3 = 0;
			for (int j = 0; (float)j < smallToEmit; j++)
			{
				num3++;
				EmitLeaf(cameraPosition, centerPos, smallLeavesPs, settings.smallSizeMultiplier, vector4);
			}
			smallToEmit -= num3;
		}
	}

	private void EmitLeaf(Vector3 camPos, Vector3 centerPos, ParticleSystem ps, float sizeMultiplier, Vector3 camVelOffset)
	{
		float num = Random.Range(settings.lifetimeRandomFrom, settings.lifetimeRandomTo);
		float size = Random.Range(settings.sizeRandomFrom, settings.sizeRandomTo) * sizeMultiplier;
		Vector3 vector = centerPos + new Vector3(Random.Range(-30, 30), centerPos.y + 30f, Random.Range(-30, 30));
		float num2 = centerPos.y - 5f;
		Vector3 vector2 = vector;
		if (Physics.Raycast(vector, Vector3.down, out var hitInfo, 40f))
		{
			Vector3 point = hitInfo.point;
			num2 = point.y;
			vector2.y = point.y + Random.Range(0.1f, 10f);
		}
		else
		{
			vector2.y = centerPos.y + Random.Range(-5f, 10f);
		}
		float num3 = CalculateAlphaMultiplier(camPos, vector2 - camVelOffset);
		if (!(num3 > 0.01f))
		{
			return;
		}
		Vector3 vector3 = velDirectionVec + 0.4f * Random.onUnitSphere;
		vector3.y *= 0.5f;
		if (vector3.sqrMagnitude > 0.0001f)
		{
			vector3.Normalize();
		}
		float a = (intensityMultiplier * settings.speedMultiplierPerIntensity + settings.speedMultiplierPerIntensityBias) * Random.Range(settings.speedRandomFrom, settings.speedRandomTo) * (vector2.y - num2 + 0.5f);
		a = Mathf.Min(a, 5f);
		avgLeafSpeed = 0.99f * avgLeafSpeed + 0.01f * a;
		if (Mathf.Abs(num2 - vector2.y) < 2f)
		{
			Vector3 vector4 = vector2 + vector3 * a * num;
			Vector3 origin = vector4 + Vector3.up * 5f;
			if (Physics.Raycast(origin, Vector3.down, out var hitInfo2, 6f))
			{
				vector3 = (hitInfo2.point - vector2).normalized;
			}
		}
		ParticleSystem.Particle particle = new ParticleSystem.Particle
		{
			position = vector2,
			velocity = vector3 * a,
			size = size,
			startLifetime = num,
			remainingLifetime = num
		};
		Color color = ((!green) ? new Color(Random.Range(0.6f, 1f), Random.Range(0.2f, 0.8f), 0f, num3 * Random.Range(0.95f, 1f)) : new Color(Random.Range(0f, 0.1f), Random.Range(0.5f, 1f), 0f, num3 * Random.Range(0.95f, 1f)));
		particle.color = color;
		particle.angularVelocity = Random.Range((0f - a) * 800f, a * 800f);
		particle.rotation = Random.Range(0f, 360f);
		ps.Emit(particle);
	}

	private float CalculateAlphaMultiplier(Vector3 camPos, Vector3 spawnPoint)
	{
		float result = 1f;
		float num = 40f;
		float magnitude = (camPos - spawnPoint).magnitude;
		if (magnitude > num)
		{
			result = Mathf.Max(0f, 1f - (magnitude - num) / 20f);
		}
		return result;
	}

	public override void Start()
	{
		base.Start();
		CreateParticleSystem();
	}

	public override void Stop()
	{
		if (smallLeavesPs != null)
		{
			smallLeavesPs.Stop();
			smallLeavesPs.Clear();
		}
		if (mediumLeavesPs != null)
		{
			mediumLeavesPs.Stop();
			mediumLeavesPs.Clear();
		}
	}

	public override void Reset()
	{
		base.Reset();
		if (leavesGo != null)
		{
			Object.Destroy(leavesGo);
		}
		leavesGo = null;
		smallLeavesPs = null;
		mediumLeavesPs = null;
	}
}
