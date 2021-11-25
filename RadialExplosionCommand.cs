using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x02000141 RID: 321
public class RadialExplosionCommand : ExplosionCommand, ILightChanger
{
	// Token: 0x0600145E RID: 5214 RVA: 0x0008F240 File Offset: 0x0008D640
	public RadialExplosionCommand(float innerForce, Vector3 position, Vector3 velocity, float innerRadius, float detachRadius, float maxRadius, HashSet<Block> blocksToExclude, string blockTag = "") : base(position, velocity, maxRadius, Mathf.Max(0.15f, maxRadius * 0.02f), blocksToExclude, blockTag)
	{
		this.innerForce = innerForce;
		this.innerRadius = innerRadius;
		this.detachRadius = detachRadius;
		this.subExplosions.Add(new RadialExplosionCommand.SubExplosionData
		{
			position = position
		});
		float num = detachRadius / 15f;
		int num2 = 0;
		while (num > 0f)
		{
			if (num >= 1f || UnityEngine.Random.value < num)
			{
				Vector3 position2 = position + UnityEngine.Random.onUnitSphere * detachRadius * UnityEngine.Random.Range(0.4f, 0.6f);
				position2.y = Mathf.Abs(position2.y);
				this.subExplosions.Add(new RadialExplosionCommand.SubExplosionData
				{
					position = position2,
					startCounter = UnityEngine.Random.Range(15, 17) * (num2 + 1)
				});
			}
			num -= 1f;
			num2++;
		}
	}

	// Token: 0x0600145F RID: 5215 RVA: 0x0008F386 File Offset: 0x0008D786
	public Color GetDynamicalLightTint()
	{
		return this.blastFraction * Color.white + (1f - this.blastFraction) * this.blastColor;
	}

	// Token: 0x06001460 RID: 5216 RVA: 0x0008F3B4 File Offset: 0x0008D7B4
	public float GetFogMultiplier()
	{
		return this.blastFraction + (1f - this.blastFraction) * 0.2f;
	}

	// Token: 0x06001461 RID: 5217 RVA: 0x0008F3CF File Offset: 0x0008D7CF
	public Color GetFogColorOverride()
	{
		return this.blastFraction * Color.white + (1f - this.blastFraction) * this.blastColor;
	}

	// Token: 0x06001462 RID: 5218 RVA: 0x0008F3FD File Offset: 0x0008D7FD
	public float GetLightIntensityMultiplier()
	{
		return this.blastFraction + (1f - this.blastFraction) * 3f;
	}

	// Token: 0x06001463 RID: 5219 RVA: 0x0008F418 File Offset: 0x0008D818
	public override bool DetachBlock(Block block)
	{
		float num = 0f;
		Vector3 a;
		if (block.size.sqrMagnitude > 4f)
		{
			a = block.go.GetComponent<Collider>().ClosestPointOnBounds(this.position);
		}
		else
		{
			a = block.goT.position;
			num = block.size.magnitude * 0.5f;
		}
		return (a - this.position).magnitude - num < this.detachRadius;
	}

	// Token: 0x06001464 RID: 5220 RVA: 0x0008F498 File Offset: 0x0008D898
	public override Vector3 GetForce(Vector3 position, float time)
	{
		Vector3 vector = position - this.position;
		float magnitude = vector.magnitude;
		Vector3 a = Vector3.zero;
		if (magnitude < this.innerRadius)
		{
			a = vector.normalized * this.innerForce;
		}
		else
		{
			if (magnitude >= this.maxRadius)
			{
				return Vector3.zero;
			}
			float d = 1f - (magnitude - this.innerRadius) / (this.maxRadius - this.innerRadius);
			a = vector.normalized * this.innerForce * d;
		}
		return a / (1f + time);
	}

	// Token: 0x06001465 RID: 5221 RVA: 0x0008F544 File Offset: 0x0008D944
	public override void Execute()
	{
		base.Execute();
		Vector3 position = Blocksworld.cameraTransform.position;
		float magnitude = (position - this.position).magnitude;
		float num = this.maxRadius * 4f;
		this.blastFraction = Mathf.Min(1f, (float)this.visualCounter * Blocksworld.fixedDeltaTime / this.visualEffectDuration);
		if (!this.addedLightChanger && magnitude < num * this.blastFraction)
		{
			Blocksworld.dynamicalLightChangers.Add(this);
			Blocksworld.worldSky.go.GetComponent<Renderer>().enabled = false;
			this.addedLightChanger = true;
		}
		if (this.addedLightChanger)
		{
			Blocksworld.UpdateDynamicalLights(true, false);
			Color dynamicalLightTint = this.GetDynamicalLightTint();
			this._cameraBackroundRestoreColor = Blocksworld.mainCamera.backgroundColor;
			Blocksworld.mainCamera.backgroundColor = dynamicalLightTint;
			this._setCameraBackgroundColor = true;
		}
		if (RadialExplosionCommand.explosionVisualGoPrefab == null)
		{
			RadialExplosionCommand.explosionVisualGoPrefab = (Resources.Load("VFX/Radial Explosion") as GameObject);
		}
		if (RadialExplosionCommand.fireParticles == null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("VFX/Explosion Particles"), this.position, Quaternion.identity) as GameObject;
			RadialExplosionCommand.explosionMeta = gameObject.GetComponent<ExplosionMetaData>();
			Transform transform = gameObject.transform;
			RadialExplosionCommand.fireParticles = transform.Find("Fire").gameObject.GetComponent<ParticleSystem>();
			RadialExplosionCommand.smokeParticles = transform.Find("Smoke").gameObject.GetComponent<ParticleSystem>();
			RadialExplosionCommand.lineParticles = transform.Find("Lines").gameObject.GetComponent<ParticleSystem>();
			RadialExplosionCommand.fireParticles.Stop();
			RadialExplosionCommand.smokeParticles.Stop();
			RadialExplosionCommand.lineParticles.Stop();
		}
		if (this.explosionVisualGo == null)
		{
			this.explosionVisualGo = UnityEngine.Object.Instantiate<GameObject>(RadialExplosionCommand.explosionVisualGoPrefab);
			Transform transform2 = this.explosionVisualGo.transform;
			this.outer = transform2.Find("Outer Sphere").gameObject;
		}
		Transform transform3 = this.explosionVisualGo.transform;
		transform3.position = this.position;
		this.outer.transform.localScale = Vector3.one * this.blastFraction * num;
		this.outer.GetComponent<Renderer>().material.SetFloat("_Alpha", 1f - 0.8f * this.blastFraction);
		int num2 = 0;
		for (int i = 0; i < this.subExplosions.Count; i++)
		{
			RadialExplosionCommand.SubExplosionData subExplosionData = this.subExplosions[i];
			if (subExplosionData.EmitSmoke(this.visualCounter))
			{
				float num3 = 1f / (1f + 0.5f * (float)num2);
				float num4 = this.maxRadius * num3;
				int num5 = Mathf.RoundToInt(RadialExplosionCommand.explosionMeta.smokeEmissionRateMultiplier * Mathf.Clamp(num4 / (2.5f * (float)(this.visualCounter - subExplosionData.startCounter)), 1f, 10f));
				float d = Mathf.Clamp(num4 / 1.5f, 2f, 15f);
				float num6 = Mathf.Clamp(num4 / 2f, 4f, 25f);
				Vector3 position2 = subExplosionData.position;
				if (num2 > 0 && subExplosionData.startCounter == this.visualCounter)
				{
					Sound.PlayPositionedOneShot("Local Explode", position2, 5f, Mathf.Max(120f, this.detachRadius * 30f * num3), 150f, AudioRolloffMode.Logarithmic);
				}
				for (int j = 0; j < num5; j++)
				{
					float num7 = UnityEngine.Random.Range(0.5f, 1f);
					Vector3 position3 = position2 + UnityEngine.Random.onUnitSphere * (RadialExplosionCommand.explosionMeta.smokeEmitRadiusBias + RadialExplosionCommand.explosionMeta.smokeEmitRadiusPerSize * num4);
					Vector3 velocity = UnityEngine.Random.onUnitSphere * UnityEngine.Random.Range(2f, 3f) * d;
					velocity.y = Mathf.Abs(velocity.y);
					ParticleSystem.Particle particle = new ParticleSystem.Particle
					{
						position = position3,
						velocity = velocity,
						size = UnityEngine.Random.Range(0.5f, 1f) * num6,
						remainingLifetime = num7,
						startLifetime = num7,
						color = Color.white,
						rotation = UnityEngine.Random.Range(-180f, 180f),
						randomSeed = (uint)UnityEngine.Random.Range(17, 9999999)
					};
					RadialExplosionCommand.smokeParticles.Emit(particle);
				}
				if (subExplosionData.EmitFire(this.visualCounter))
				{
					int num8 = 2;
					num6 = Mathf.Clamp(num4 / (float)num8, 1.5f, 20f);
					for (int k = 0; k < num8; k++)
					{
						float num9 = UnityEngine.Random.Range(0.35f, 0.65f);
						ParticleSystem.Particle particle2 = new ParticleSystem.Particle
						{
							position = position2,
							size = (float)(k + 1) * num6,
							remainingLifetime = num9,
							startLifetime = num9,
							color = Color.white,
							rotation = UnityEngine.Random.Range(-180f, 180f),
							randomSeed = (uint)UnityEngine.Random.Range(17, 9999999)
						};
						RadialExplosionCommand.fireParticles.Emit(particle2);
					}
				}
				if (subExplosionData.EmitLines(this.visualCounter))
				{
					int num10 = Mathf.RoundToInt(RadialExplosionCommand.explosionMeta.smokeEmissionRateMultiplier * Mathf.Clamp(num4 / (2.5f * (float)(this.visualCounter - subExplosionData.startCounter)), 1f, 10f));
					num6 = Mathf.Clamp(num4 / 2f, 3f, 6f);
					for (int l = 0; l < num10; l++)
					{
						float num11 = UnityEngine.Random.Range(0.5f, 0.65f);
						Vector3 velocity2 = UnityEngine.Random.onUnitSphere * UnityEngine.Random.Range(4f, 6f) * d;
						velocity2.y = Mathf.Abs(velocity2.y);
						ParticleSystem.Particle particle3 = new ParticleSystem.Particle
						{
							position = position2,
							velocity = velocity2,
							size = UnityEngine.Random.Range(0.5f, 1f) * num6,
							remainingLifetime = num11,
							startLifetime = num11,
							color = Color.white,
							rotation = UnityEngine.Random.Range(-180f, 180f),
							randomSeed = (uint)UnityEngine.Random.Range(17, 9999999)
						};
						RadialExplosionCommand.lineParticles.Emit(particle3);
					}
				}
			}
			num2++;
		}
		float num12 = num * 4f * this.blastFraction;
		if (magnitude < num12)
		{
			float strength = 1f + 6f * (1f - magnitude / num12);
			BlockMaster.CameraShake(strength);
		}
		if (this.VisualEffectDone())
		{
			this.Cleanup();
		}
	}

	// Token: 0x06001466 RID: 5222 RVA: 0x0008FC58 File Offset: 0x0008E058
	private void Cleanup()
	{
		if (this.explosionVisualGo != null)
		{
			UnityEngine.Object.Destroy(this.explosionVisualGo, Blocksworld.fixedDeltaTime);
			this.explosionVisualGo = null;
			if (this.addedLightChanger)
			{
				Blocksworld.dynamicalLightChangers.Remove(this);
				Blocksworld.worldSky.go.GetComponent<Renderer>().enabled = !Blocksworld.renderingSkybox;
				Blocksworld.UpdateDynamicalLights(true, false);
			}
			if (this._setCameraBackgroundColor)
			{
				Blocksworld.mainCamera.backgroundColor = this._cameraBackroundRestoreColor;
			}
		}
	}

	// Token: 0x06001467 RID: 5223 RVA: 0x0008FCE2 File Offset: 0x0008E0E2
	public override void Removed()
	{
		base.Removed();
		this.Cleanup();
	}

	// Token: 0x04001023 RID: 4131
	protected float innerForce = 5f;

	// Token: 0x04001024 RID: 4132
	protected float innerRadius = 10f;

	// Token: 0x04001025 RID: 4133
	protected float detachRadius = 10f;

	// Token: 0x04001026 RID: 4134
	private static GameObject explosionVisualGoPrefab;

	// Token: 0x04001027 RID: 4135
	private GameObject explosionVisualGo;

	// Token: 0x04001028 RID: 4136
	private static ParticleSystem fireParticles;

	// Token: 0x04001029 RID: 4137
	private static ParticleSystem smokeParticles;

	// Token: 0x0400102A RID: 4138
	private static ParticleSystem lineParticles;

	// Token: 0x0400102B RID: 4139
	private static ExplosionMetaData explosionMeta;

	// Token: 0x0400102C RID: 4140
	private GameObject outer;

	// Token: 0x0400102D RID: 4141
	private bool addedLightChanger;

	// Token: 0x0400102E RID: 4142
	private Color blastColor = new Color(1f, 0.9f, 0.3f);

	// Token: 0x0400102F RID: 4143
	private Color _cameraBackroundRestoreColor;

	// Token: 0x04001030 RID: 4144
	private bool _setCameraBackgroundColor;

	// Token: 0x04001031 RID: 4145
	private float blastFraction;

	// Token: 0x04001032 RID: 4146
	private List<RadialExplosionCommand.SubExplosionData> subExplosions = new List<RadialExplosionCommand.SubExplosionData>();

	// Token: 0x02000142 RID: 322
	private class SubExplosionData
	{
		// Token: 0x06001469 RID: 5225 RVA: 0x0008FCF8 File Offset: 0x0008E0F8
		public bool EmitSmoke(int counter)
		{
			int num = counter - this.startCounter;
			return num < 10 && num >= 0;
		}

		// Token: 0x0600146A RID: 5226 RVA: 0x0008FD20 File Offset: 0x0008E120
		public bool EmitFire(int counter)
		{
			int num = counter - this.startCounter;
			return num < 5 && num >= 0;
		}

		// Token: 0x0600146B RID: 5227 RVA: 0x0008FD48 File Offset: 0x0008E148
		public bool EmitLines(int counter)
		{
			int num = counter - this.startCounter;
			return num < 5 && num >= 0;
		}

		// Token: 0x04001033 RID: 4147
		public Vector3 position;

		// Token: 0x04001034 RID: 4148
		public int startCounter;
	}
}
