using Blocks;
using UnityEngine;

public class RainEffect : WeatherEffect
{
	private GameObject rainGo;

	private ParticleSystem rainPs;

	private ParticleSystem ripplePs;

	private ParticleSystem splashPs;

	private bool needsAngle;

	private float newYAngle;

	private float indoorFraction;

	private float intensityFraction = 1f;

	private const float indoorAlpha = 0.05f;

	private const float intensityAlpha = 0.1f;

	private float toEmit;

	private float avgSplashDistance = 1f;

	private RainEffectSettings settings;

	public RainEffect()
	{
		loopSfx = "Rain Loop";
	}

	protected override float GetTargetVolume()
	{
		float num = 1f / (1f + 0.02f * avgSplashDistance + 2f * indoorFraction);
		return num * base.GetTargetVolume();
	}

	private void CreateParticleSystem()
	{
		if (rainPs == null)
		{
			if (rainGo == null)
			{
				rainGo = Object.Instantiate(Resources.Load("Env Effect/Rain Particle Systems")) as GameObject;
			}
			settings = rainGo.GetComponent<RainEffectSettings>();
			rainPs = rainGo.transform.Find("Rain Particle System").gameObject.GetComponent<ParticleSystem>();
			GameObject gameObject = rainGo.transform.Find("Ripple Particle System").gameObject;
			ripplePs = gameObject.GetComponent<ParticleSystem>();
			GameObject gameObject2 = rainGo.transform.Find("Splash Particle System").gameObject;
			splashPs = gameObject2.GetComponent<ParticleSystem>();
		}
		ResetRain();
		Stop();
	}

	public override void Pause()
	{
		base.Pause();
		if (rainPs != null)
		{
			rainPs.Pause();
		}
		if (ripplePs != null)
		{
			ripplePs.Pause();
		}
		if (splashPs != null)
		{
			splashPs.Pause();
		}
	}

	public override void Resume()
	{
		base.Resume();
		if (rainPs != null)
		{
			rainPs.Play();
		}
		if (ripplePs != null)
		{
			ripplePs.Play();
		}
		if (splashPs != null)
		{
			splashPs.Play();
		}
	}

	public override void Update()
	{
		base.Update();
		if (!(rainGo != null) || paused)
		{
			return;
		}
		if (needsAngle && rainPs.startSpeed < 20f)
		{
			rainPs.startSpeed += Time.deltaTime * 10f;
		}
		float y = rainGo.transform.eulerAngles.y;
		if (Mathf.Abs(newYAngle - y) > 3f)
		{
			float num = Time.deltaTime * 100f;
			if (y > newYAngle)
			{
				num = 0f - num;
			}
			rainGo.transform.rotation = Quaternion.Euler(0f, y + num, 0f);
		}
		Vector3 cameraPosition = Blocksworld.cameraPosition;
		Vector3 vector = Blocksworld.blocksworldCamera.GetTargetPosition() - cameraPosition;
		float magnitude = vector.magnitude;
		bool flag = true;
		Bounds bounds = default(Bounds);
		if (magnitude > 0.01f)
		{
			Vector3 vector2 = vector / magnitude;
			float num2 = Mathf.Min(magnitude * 0.5f, 30f);
			Vector3 position = cameraPosition + vector2 * num2;
			GameObject worldOcean = Blocksworld.worldOcean;
			if (worldOcean != null && worldOcean.GetComponent<Collider>() != null)
			{
				bounds = worldOcean.GetComponent<Collider>().bounds;
				position.y = Mathf.Max(position.y, bounds.max.y + 10f);
			}
			else if (ripplePs.isPlaying)
			{
				flag = false;
			}
			Vector3 vector3 = Util.ProjectOntoPlane(Blocksworld.cameraForward, Vector3.up);
			if (vector3.sqrMagnitude > 0.001f)
			{
				vector3 = vector3.normalized;
				position += vector3 * indoorFraction * 70f;
			}
			rainGo.transform.position = position;
		}
		rainPs.emissionRate = intensityMultiplier * settings.rainEmissionRate * intensityFraction;
		Vector3 vector4 = cameraPosition + Blocksworld.cameraForward * 15f + Vector3.up * 20f;
		if (flag)
		{
			vector4.y = Mathf.Max(vector4.y, bounds.max.y + 1f);
		}
		bool flag2 = false;
		if (vector4.y - cameraPosition.y < 40f)
		{
			toEmit += settings.ripplesPerFrame * intensityMultiplier;
			int num3 = 0;
			for (int i = 0; (float)i < toEmit; i++)
			{
				num3++;
				Vector3 origin = vector4 + Vector3.right * Random.Range(-15f, 15f) + Vector3.forward * Random.Range(-15f, 15f);
				if (Physics.Raycast(origin, Vector3.down, out var hitInfo, 40f))
				{
					flag2 = true;
					Vector3 point = hitInfo.point;
					bool flag3 = Vector3.Dot(hitInfo.normal, Vector3.up) > 0.98f;
					Vector3 vector5 = point + Vector3.up * 0.05f;
					Vector3 vector6 = settings.splashSpeed * Vector3.Reflect(Vector3.down, hitInfo.normal);
					Vector3 vector7 = Vector3.zero;
					Rigidbody rigidbody = hitInfo.rigidbody;
					if (rigidbody != null)
					{
						vector7 = rigidbody.velocity;
						Vector3 lhs = point - rigidbody.worldCenterOfMass;
						vector7 += Vector3.Cross(lhs, rigidbody.angularVelocity);
					}
					vector6 = new Vector3(vector6.x * Random.Range(0.7f, 1.3f), vector6.y * Random.Range(0.7f, 1.3f), vector6.z * Random.Range(0.7f, 1.3f));
					float num4 = Random.Range(settings.splashSizeRandomFrom, settings.splashSizeRandomTo);
					float num5 = Random.Range(settings.splashLifetimeRandomFrom, settings.splashLifetimeRandomTo);
					Color white = Color.white;
					Block block = BWSceneManager.FindBlock(hitInfo.collider.gameObject);
					if (block is BlockAbstractWater)
					{
						num5 *= 0.7f;
						num4 *= 0.7f;
						white.a = 0.7f;
					}
					ParticleSystem.Particle particle = new ParticleSystem.Particle
					{
						position = vector5,
						velocity = vector6,
						size = num4,
						startLifetime = num5,
						remainingLifetime = num5,
						color = white,
						randomSeed = (uint)Random.Range(1, 9999999)
					};
					splashPs.Emit(particle);
					if (flag3 && vector7.sqrMagnitude < 2f)
					{
						ripplePs.Emit(vector5, Vector3.zero, Random.Range(settings.rippleSizeRandomFrom, settings.rippleSizeRandomTo), Random.Range(settings.rippleSizeRandomFrom, settings.rippleSizeRandomTo), new Color(1f, 1f, 1f, Random.Range(0.8f, 1f)));
					}
					float magnitude2 = (cameraPosition - vector5).magnitude;
					avgSplashDistance = 0.99f * avgSplashDistance + 0.01f * magnitude2;
					bool flag4 = Mathf.Abs(cameraPosition.x - point.x) < 5f && Mathf.Abs(cameraPosition.z - point.z) < 5f;
					if (point.y > cameraPosition.y)
					{
						intensityFraction = 0.9f * intensityFraction;
						if (flag4)
						{
							indoorFraction = 0.95f * indoorFraction + 0.05f;
						}
					}
					else
					{
						intensityFraction = 0.9f * intensityFraction + 0.1f;
						if (flag4)
						{
							indoorFraction = 0.95f * indoorFraction;
						}
					}
				}
				else
				{
					intensityFraction = 0.9f * intensityFraction + 0.1f;
					indoorFraction = 0.95f * indoorFraction;
				}
			}
			toEmit -= num3;
		}
		if (!flag2)
		{
			avgSplashDistance = 0.99f * avgSplashDistance + 2f;
		}
	}

	public override void SetEffectAngle(float angle)
	{
		base.SetEffectAngle(angle);
		needsAngle = true;
		if (angle < 0f)
		{
			angle += 360f;
		}
		if (angle == 360f)
		{
			angle = 0f;
		}
		newYAngle = angle;
	}

	public override void Start()
	{
		base.Start();
		CreateParticleSystem();
		rainPs.Play();
	}

	public override void Stop()
	{
		if (rainPs != null)
		{
			rainPs.Stop();
			ripplePs.Stop();
			splashPs.Stop();
			rainPs.Clear();
			ripplePs.Clear();
			splashPs.Clear();
			rainPs.startSpeed = 0f;
		}
		needsAngle = false;
	}

	private void ResetRain()
	{
		newYAngle = 0f;
		needsAngle = false;
		if (rainPs != null)
		{
			rainPs.startSpeed = 0f;
		}
		if (rainGo != null)
		{
			rainGo.transform.rotation = Quaternion.identity;
		}
	}

	public override void Reset()
	{
		base.Reset();
		if (rainGo != null)
		{
			Object.Destroy(rainGo);
		}
		rainGo = null;
		rainPs = null;
		ripplePs = null;
		splashPs = null;
	}

	public override float GetFogMultiplier()
	{
		return 1f - Mathf.Min(0.3f, intensityMultiplier * 0.5f);
	}
}
