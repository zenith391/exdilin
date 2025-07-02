using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class RadialExplosionCommand : ExplosionCommand, ILightChanger
{
	private class SubExplosionData
	{
		public Vector3 position;

		public int startCounter;

		public bool EmitSmoke(int counter)
		{
			int num = counter - startCounter;
			if (num < 10)
			{
				return num >= 0;
			}
			return false;
		}

		public bool EmitFire(int counter)
		{
			int num = counter - startCounter;
			if (num < 5)
			{
				return num >= 0;
			}
			return false;
		}

		public bool EmitLines(int counter)
		{
			int num = counter - startCounter;
			if (num < 5)
			{
				return num >= 0;
			}
			return false;
		}
	}

	protected float innerForce = 5f;

	protected float innerRadius = 10f;

	protected float detachRadius = 10f;

	private static GameObject explosionVisualGoPrefab;

	private GameObject explosionVisualGo;

	private static ParticleSystem fireParticles;

	private static ParticleSystem smokeParticles;

	private static ParticleSystem lineParticles;

	private static ExplosionMetaData explosionMeta;

	private GameObject outer;

	private bool addedLightChanger;

	private Color blastColor = new Color(1f, 0.9f, 0.3f);

	private Color _cameraBackroundRestoreColor;

	private bool _setCameraBackgroundColor;

	private float blastFraction;

	private List<SubExplosionData> subExplosions = new List<SubExplosionData>();

	public RadialExplosionCommand(float innerForce, Vector3 position, Vector3 velocity, float innerRadius, float detachRadius, float maxRadius, HashSet<Block> blocksToExclude, string blockTag = "")
		: base(position, velocity, maxRadius, Mathf.Max(0.15f, maxRadius * 0.02f), blocksToExclude, blockTag)
	{
		this.innerForce = innerForce;
		this.innerRadius = innerRadius;
		this.detachRadius = detachRadius;
		subExplosions.Add(new SubExplosionData
		{
			position = position
		});
		float num = detachRadius / 15f;
		int num2 = 0;
		while (num > 0f)
		{
			if (num >= 1f || Random.value < num)
			{
				Vector3 vector = position + Random.onUnitSphere * detachRadius * Random.Range(0.4f, 0.6f);
				vector.y = Mathf.Abs(vector.y);
				subExplosions.Add(new SubExplosionData
				{
					position = vector,
					startCounter = Random.Range(15, 17) * (num2 + 1)
				});
			}
			num -= 1f;
			num2++;
		}
	}

	public Color GetDynamicalLightTint()
	{
		return blastFraction * Color.white + (1f - blastFraction) * blastColor;
	}

	public float GetFogMultiplier()
	{
		return blastFraction + (1f - blastFraction) * 0.2f;
	}

	public Color GetFogColorOverride()
	{
		return blastFraction * Color.white + (1f - blastFraction) * blastColor;
	}

	public float GetLightIntensityMultiplier()
	{
		return blastFraction + (1f - blastFraction) * 3f;
	}

	public override bool DetachBlock(Block block)
	{
		float num = 0f;
		Vector3 vector;
		if (block.size.sqrMagnitude > 4f)
		{
			vector = block.go.GetComponent<Collider>().ClosestPointOnBounds(position);
		}
		else
		{
			vector = block.goT.position;
			num = block.size.magnitude * 0.5f;
		}
		return (vector - position).magnitude - num < detachRadius;
	}

	public override Vector3 GetForce(Vector3 position, float time)
	{
		Vector3 vector = position - base.position;
		float magnitude = vector.magnitude;
		Vector3 zero = Vector3.zero;
		if (magnitude < innerRadius)
		{
			zero = vector.normalized * innerForce;
		}
		else
		{
			if (magnitude >= maxRadius)
			{
				return Vector3.zero;
			}
			float num = 1f - (magnitude - innerRadius) / (maxRadius - innerRadius);
			zero = vector.normalized * innerForce * num;
		}
		return zero / (1f + time);
	}

	public override void Execute()
	{
		base.Execute();
		Vector3 vector = Blocksworld.cameraTransform.position;
		float magnitude = (vector - position).magnitude;
		float num = maxRadius * 4f;
		blastFraction = Mathf.Min(1f, (float)visualCounter * Blocksworld.fixedDeltaTime / visualEffectDuration);
		if (!addedLightChanger && magnitude < num * blastFraction)
		{
			Blocksworld.dynamicalLightChangers.Add(this);
			Blocksworld.worldSky.go.GetComponent<Renderer>().enabled = false;
			addedLightChanger = true;
		}
		if (addedLightChanger)
		{
			Blocksworld.UpdateDynamicalLights();
			Color dynamicalLightTint = GetDynamicalLightTint();
			_cameraBackroundRestoreColor = Blocksworld.mainCamera.backgroundColor;
			Blocksworld.mainCamera.backgroundColor = dynamicalLightTint;
			_setCameraBackgroundColor = true;
		}
		if (explosionVisualGoPrefab == null)
		{
			explosionVisualGoPrefab = Resources.Load("VFX/Radial Explosion") as GameObject;
		}
		if (fireParticles == null)
		{
			GameObject gameObject = Object.Instantiate(Resources.Load("VFX/Explosion Particles"), position, Quaternion.identity) as GameObject;
			explosionMeta = gameObject.GetComponent<ExplosionMetaData>();
			Transform transform = gameObject.transform;
			fireParticles = transform.Find("Fire").gameObject.GetComponent<ParticleSystem>();
			smokeParticles = transform.Find("Smoke").gameObject.GetComponent<ParticleSystem>();
			lineParticles = transform.Find("Lines").gameObject.GetComponent<ParticleSystem>();
			fireParticles.Stop();
			smokeParticles.Stop();
			lineParticles.Stop();
		}
		if (explosionVisualGo == null)
		{
			explosionVisualGo = Object.Instantiate(explosionVisualGoPrefab);
			Transform transform2 = explosionVisualGo.transform;
			outer = transform2.Find("Outer Sphere").gameObject;
		}
		Transform transform3 = explosionVisualGo.transform;
		transform3.position = position;
		outer.transform.localScale = Vector3.one * blastFraction * num;
		outer.GetComponent<Renderer>().material.SetFloat("_Alpha", 1f - 0.8f * blastFraction);
		int num2 = 0;
		for (int i = 0; i < subExplosions.Count; i++)
		{
			SubExplosionData subExplosionData = subExplosions[i];
			if (subExplosionData.EmitSmoke(visualCounter))
			{
				float num3 = 1f / (1f + 0.5f * (float)num2);
				float num4 = maxRadius * num3;
				int num5 = Mathf.RoundToInt(explosionMeta.smokeEmissionRateMultiplier * Mathf.Clamp(num4 / (2.5f * (float)(visualCounter - subExplosionData.startCounter)), 1f, 10f));
				float num6 = Mathf.Clamp(num4 / 1.5f, 2f, 15f);
				float num7 = Mathf.Clamp(num4 / 2f, 4f, 25f);
				Vector3 vector2 = subExplosionData.position;
				if (num2 > 0 && subExplosionData.startCounter == visualCounter)
				{
					Sound.PlayPositionedOneShot("Local Explode", vector2, 5f, Mathf.Max(120f, detachRadius * 30f * num3));
				}
				for (int j = 0; j < num5; j++)
				{
					float num8 = Random.Range(0.5f, 1f);
					Vector3 vector3 = vector2 + Random.onUnitSphere * (explosionMeta.smokeEmitRadiusBias + explosionMeta.smokeEmitRadiusPerSize * num4);
					Vector3 vector4 = Random.onUnitSphere * Random.Range(2f, 3f) * num6;
					vector4.y = Mathf.Abs(vector4.y);
					ParticleSystem.Particle particle = new ParticleSystem.Particle
					{
						position = vector3,
						velocity = vector4,
						size = Random.Range(0.5f, 1f) * num7,
						remainingLifetime = num8,
						startLifetime = num8,
						color = Color.white,
						rotation = Random.Range(-180f, 180f),
						randomSeed = (uint)Random.Range(17, 9999999)
					};
					smokeParticles.Emit(particle);
				}
				if (subExplosionData.EmitFire(visualCounter))
				{
					int num9 = 2;
					num7 = Mathf.Clamp(num4 / (float)num9, 1.5f, 20f);
					for (int k = 0; k < num9; k++)
					{
						float num10 = Random.Range(0.35f, 0.65f);
						ParticleSystem.Particle particle2 = new ParticleSystem.Particle
						{
							position = vector2,
							size = (float)(k + 1) * num7,
							remainingLifetime = num10,
							startLifetime = num10,
							color = Color.white,
							rotation = Random.Range(-180f, 180f),
							randomSeed = (uint)Random.Range(17, 9999999)
						};
						fireParticles.Emit(particle2);
					}
				}
				if (subExplosionData.EmitLines(visualCounter))
				{
					int num11 = Mathf.RoundToInt(explosionMeta.smokeEmissionRateMultiplier * Mathf.Clamp(num4 / (2.5f * (float)(visualCounter - subExplosionData.startCounter)), 1f, 10f));
					num7 = Mathf.Clamp(num4 / 2f, 3f, 6f);
					for (int l = 0; l < num11; l++)
					{
						float num12 = Random.Range(0.5f, 0.65f);
						Vector3 vector5 = Random.onUnitSphere * Random.Range(4f, 6f) * num6;
						vector5.y = Mathf.Abs(vector5.y);
						ParticleSystem.Particle particle3 = new ParticleSystem.Particle
						{
							position = vector2,
							velocity = vector5,
							size = Random.Range(0.5f, 1f) * num7,
							remainingLifetime = num12,
							startLifetime = num12,
							color = Color.white,
							rotation = Random.Range(-180f, 180f),
							randomSeed = (uint)Random.Range(17, 9999999)
						};
						lineParticles.Emit(particle3);
					}
				}
			}
			num2++;
		}
		float num13 = num * 4f * blastFraction;
		if (magnitude < num13)
		{
			float strength = 1f + 6f * (1f - magnitude / num13);
			BlockMaster.CameraShake(strength);
		}
		if (VisualEffectDone())
		{
			Cleanup();
		}
	}

	private void Cleanup()
	{
		if (explosionVisualGo != null)
		{
			Object.Destroy(explosionVisualGo, Blocksworld.fixedDeltaTime);
			explosionVisualGo = null;
			if (addedLightChanger)
			{
				Blocksworld.dynamicalLightChangers.Remove(this);
				Blocksworld.worldSky.go.GetComponent<Renderer>().enabled = !Blocksworld.renderingSkybox;
				Blocksworld.UpdateDynamicalLights();
			}
			if (_setCameraBackgroundColor)
			{
				Blocksworld.mainCamera.backgroundColor = _cameraBackroundRestoreColor;
			}
		}
	}

	public override void Removed()
	{
		base.Removed();
		Cleanup();
	}
}
