using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000068 RID: 104
	public class BlockAbstractRocket : Block
	{
		// Token: 0x06000872 RID: 2162 RVA: 0x0003ACC0 File Offset: 0x000390C0
		public BlockAbstractRocket(List<List<Tile>> tiles, string smokePrefab = "Blocks/Rocket Flame", string flamePrefab = "") : base(tiles)
		{
			this.flame = (UnityEngine.Object.Instantiate(Resources.Load(smokePrefab)) as GameObject);
			if (!string.IsNullOrEmpty(flamePrefab))
			{
				this.fireFlame = (UnityEngine.Object.Instantiate(Resources.Load(flamePrefab)) as GameObject);
				this.flameParticles = this.fireFlame.GetComponent<ParticleSystem>();
				this.flameParticles.enableEmission = false;
				this.fireFlame.SetActive(false);
			}
			this.particles = this.flame.GetComponent<ParticleSystem>();
			this.particles.enableEmission = false;
			this.flame.SetActive(false);
			this.loopName = "Rocket Burst Loop";
			this.sfxLoopUpdateCounter = UnityEngine.Random.Range(0, 5);
		}

		// Token: 0x06000873 RID: 2163 RVA: 0x0003ADBA File Offset: 0x000391BA
		public override void Destroy()
		{
			UnityEngine.Object.Destroy(this.flame);
			if (this.fireFlame != null)
			{
				UnityEngine.Object.Destroy(this.fireFlame);
			}
			base.Destroy();
		}

		// Token: 0x06000874 RID: 2164 RVA: 0x0003ADEC File Offset: 0x000391EC
		public override void Play()
		{
			base.Play();
			this.treatAsVehicleStatus = -1;
			this.flame.SetActive(true);
			if (this.fireFlame != null)
			{
				this.fireFlame.SetActive(true);
			}
			this.smokeForce = 0f;
			this.fireForce = 0f;
			this.rocketMeta = this.go.GetComponent<RocketMetaData>();
			if (this.rocketMeta != null)
			{
				this.exitOffset = this.rocketMeta.exitOffset;
				this.exitOffset.Scale(base.Scale());
				this.loopName = this.rocketMeta.loopSfx;
				this.smokeSizeMultiplier = this.rocketMeta.smokeSizeMultiplier;
				this.smokeSpeedMultiplier = this.rocketMeta.smokeSpeedMultiplier;
				this.particlesPerSecond = this.rocketMeta.particlesPerSecond;
			}
			else
			{
				BWLog.Info("Could not find rocket meta data component");
			}
		}

		// Token: 0x06000875 RID: 2165 RVA: 0x0003AEDC File Offset: 0x000392DC
		public override void Play2()
		{
			this.mass = Bunch.GetModelMass(this);
			this.mass = Mathf.Min(10f, this.mass);
			Vector3 lhs = base.Scale();
			if (lhs != Vector3.one)
			{
				this.mass *= 1f + 0.1f * lhs.x * lhs.y * lhs.z;
			}
			if (this.setSmokeColor)
			{
				this.UpdateSmokeColorPaint(this.GetPaint(this.smokeColorMeshIndex), this.smokeColorMeshIndex);
				this.UpdateSmokeColorTexture(base.GetTexture(this.smokeColorMeshIndex), this.smokeColorMeshIndex);
			}
		}

		// Token: 0x06000876 RID: 2166 RVA: 0x0003AF90 File Offset: 0x00039390
		public override void Stop(bool resetBlock = true)
		{
			this.flame.SetActive(false);
			if (this.fireFlame != null)
			{
				this.fireFlame.SetActive(false);
			}
			base.Stop(resetBlock);
			this.playBurst = false;
			this.PlayLoopSound(false, base.GetLoopClip(), 1f, null, 1f);
		}

		// Token: 0x06000877 RID: 2167 RVA: 0x0003AFEC File Offset: 0x000393EC
		public override void Pause()
		{
			this.particles.Pause();
			this.playBurst = false;
			this.PlayLoopSound(false, base.GetLoopClip(), 1f, null, 1f);
		}

		// Token: 0x06000878 RID: 2168 RVA: 0x0003B018 File Offset: 0x00039418
		public override void Resume()
		{
			this.particles.Play();
		}

		// Token: 0x06000879 RID: 2169 RVA: 0x0003B025 File Offset: 0x00039425
		public override void ResetFrame()
		{
			this.isFiring = false;
			this.isSmoking = false;
			this.isFlaming = false;
		}

		// Token: 0x0600087A RID: 2170 RVA: 0x0003B03C File Offset: 0x0003943C
		public TileResultCode IsFiring(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!this.isFiring) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x0600087B RID: 2171 RVA: 0x0003B050 File Offset: 0x00039450
		public TileResultCode IsSmoking(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!this.isSmoking) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x0600087C RID: 2172 RVA: 0x0003B064 File Offset: 0x00039464
		public TileResultCode IsFlaming(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!this.isFlaming) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x0600087D RID: 2173 RVA: 0x0003B078 File Offset: 0x00039478
		public TileResultCode FireRocket(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float num = eInfo.floatArg * Util.GetFloatArg(args, 0, 1f);
			this.smokeForce += num;
			num *= 12f;
			this.fireForce += num;
			this.isFiring = true;
			this.playBurst = true;
			return TileResultCode.True;
		}

		// Token: 0x0600087E RID: 2174 RVA: 0x0003B0CC File Offset: 0x000394CC
		public TileResultCode Smoke(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.smokeForce += eInfo.floatArg;
			this.isSmoking = true;
			return TileResultCode.True;
		}

		// Token: 0x0600087F RID: 2175 RVA: 0x0003B0E9 File Offset: 0x000394E9
		public TileResultCode Flame(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.smokeForce += eInfo.floatArg;
			this.isFlaming = true;
			return TileResultCode.True;
		}

		// Token: 0x06000880 RID: 2176 RVA: 0x0003B108 File Offset: 0x00039508
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			Transform goT = this.goT;
			Vector3 position = goT.position + goT.TransformDirection(this.exitOffset);
			this.playBurst = this.isFiring;
			float num = Mathf.Min(this.size.x, this.size.z) * this.smokeSizeMultiplier;
			float num2 = Mathf.Sqrt(num);
			if (Sound.sfxEnabled && !this.vanished && this.go.activeInHierarchy)
			{
				float num3 = (!this.playBurst) ? -0.04f : 0.04f;
				this.playBurstLevel = Mathf.Clamp(this.playBurstLevel + num3, 0f, Mathf.Clamp(0.5f * this.smokeForce, 0.1f, 1f));
				float num4 = (0.98f + Mathf.Min(0.1f, 0.02f * this.smokeForce)) / (0.5f + 0.5f * num2);
				num4 = Mathf.Clamp(num4, 0.25f, 1.25f);
				if (this.sfxLoopUpdateCounter % 5 == 0)
				{
					this.PlayLoopSound(this.playBurst || this.playBurstLevel > 0.01f, base.GetLoopClip(), this.playBurstLevel, null, num4);
					base.UpdateWithinWaterLPFilter(null);
				}
				this.sfxLoopUpdateCounter++;
			}
			else
			{
				this.PlayLoopSound(false, base.GetLoopClip(), 1f, null, 1f);
			}
			if (this.fireForce > 0f)
			{
				Transform parent = goT.parent;
				if (parent != null && parent.gameObject != null)
				{
					Rigidbody component = parent.gameObject.GetComponent<Rigidbody>();
					if (component != null && !component.isKinematic)
					{
						Vector3 a = this.fireForce * goT.up;
						Vector3 force = this.mass * a;
						component.AddForceAtPosition(force, goT.position);
						Vector3 vector = 2f * a;
						float magnitude = vector.magnitude;
						if (magnitude > 0.1f)
						{
							vector = vector / magnitude * Mathf.Clamp(magnitude, -60f, 60f);
							vector = Util.ProjectOntoPlane(vector, Vector3.up);
							Blocksworld.blocksworldCamera.AddForceDirectionHint(this.chunk, vector);
							BlockAccelerations.BlockAccelerates(this, a);
						}
					}
				}
				this.fireForce = 0f;
			}
			if (this.smokeForce > 0f && this.emitSmoke && !this.vanished)
			{
				this.smokeForce = Mathf.Min(2.5f, this.smokeForce);
				float num5 = this.smokeForce * this.particlesPerSecond;
				float num6 = num5 * Blocksworld.fixedDeltaTime;
				int num7 = 0;
				Vector3 a2 = Vector3.zero;
				if (this.chunk.go != null)
				{
					Rigidbody rb = this.chunk.rb;
					if (rb != null && !rb.isKinematic)
					{
						a2 = rb.velocity;
					}
				}
				float d = this.smokeForce * this.smokeSpeedMultiplier;
				while (num7 < 6 && num6 > 0f && (num6 >= 1f || UnityEngine.Random.value < num6))
				{
					float size = num * (0.8f * UnityEngine.Random.value + 0.5f);
					Vector3 velocity = (a2 - goT.up * num * d * (9f + 2f * UnityEngine.Random.value)) * UnityEngine.Random.Range(0.75f, 1f);
					float num8 = 0.5f;
					ParticleSystem.Particle particle = new ParticleSystem.Particle
					{
						position = position,
						velocity = velocity,
						size = size,
						remainingLifetime = num8,
						startLifetime = num8,
						rotation = (float)UnityEngine.Random.Range(-180, 180),
						color = this.smokeColor,
						randomSeed = (uint)UnityEngine.Random.Range(17, 9999999)
					};
					if (this.isFlaming && this.flameParticles != null)
					{
						this.flameParticles.Emit(particle);
					}
					else
					{
						this.particles.Emit(particle);
					}
					num6 -= 1f;
					num7++;
				}
			}
			this.smokeForce = 0f;
		}

		// Token: 0x06000881 RID: 2177 RVA: 0x0003B5A9 File Offset: 0x000399A9
		private void UpdateSmokeColorTexture(string texture, int meshIndex)
		{
			if (this.setSmokeColor && meshIndex == this.smokeColorMeshIndex && this.subMeshGameObjects.Count > this.smokeColorMeshIndex)
			{
				this.emitSmoke = (texture != "Glass");
			}
		}

		// Token: 0x06000882 RID: 2178 RVA: 0x0003B5EC File Offset: 0x000399EC
		public static Color GetSmokeColor(string paint, Renderer renderer)
		{
			Color color = Color.white;
			if (paint != null)
			{
				if (paint == "White")
				{
					goto IL_37;
				}
			}
			color = renderer.sharedMaterial.color;
			IL_37:
			if (Blocksworld.IsLuminousPaint(paint))
			{
				color += color;
			}
			return color;
		}

		// Token: 0x06000883 RID: 2179 RVA: 0x0003B644 File Offset: 0x00039A44
		private void UpdateSmokeColorPaint(string paint, int meshIndex)
		{
			if (this.setSmokeColor && meshIndex == this.smokeColorMeshIndex && this.subMeshGameObjects.Count > this.smokeColorMeshIndex)
			{
				Renderer renderer = (meshIndex != 0) ? this.subMeshGameObjects[meshIndex - 1].GetComponent<Renderer>() : this.go.GetComponent<Renderer>();
				this.smokeColor = BlockAbstractRocket.GetSmokeColor(paint, renderer);
			}
		}

		// Token: 0x06000884 RID: 2180 RVA: 0x0003B6B8 File Offset: 0x00039AB8
		public override TileResultCode TextureTo(string texture, Vector3 normal, bool permanent, int meshIndex = 0, bool force = false)
		{
			TileResultCode result = base.TextureTo(texture, normal, permanent, meshIndex, force);
			this.UpdateSmokeColorTexture(texture, meshIndex);
			return result;
		}

		// Token: 0x06000885 RID: 2181 RVA: 0x0003B6E0 File Offset: 0x00039AE0
		public override TileResultCode PaintTo(string paint, bool permanent, int meshIndex = 0)
		{
			TileResultCode result = base.PaintTo(paint, permanent, meshIndex);
			this.UpdateSmokeColorPaint(paint, meshIndex);
			return result;
		}

		// Token: 0x06000886 RID: 2182 RVA: 0x0003B700 File Offset: 0x00039B00
		public override bool TreatAsVehicleLikeBlock()
		{
			return base.TreatAsVehicleLikeBlockWithStatus(ref this.treatAsVehicleStatus);
		}

		// Token: 0x04000675 RID: 1653
		private GameObject flame;

		// Token: 0x04000676 RID: 1654
		private GameObject fireFlame;

		// Token: 0x04000677 RID: 1655
		private ParticleSystem particles;

		// Token: 0x04000678 RID: 1656
		private ParticleSystem flameParticles;

		// Token: 0x04000679 RID: 1657
		private float mass;

		// Token: 0x0400067A RID: 1658
		private bool isFiring;

		// Token: 0x0400067B RID: 1659
		private bool isSmoking;

		// Token: 0x0400067C RID: 1660
		private bool isFlaming;

		// Token: 0x0400067D RID: 1661
		private bool playBurst;

		// Token: 0x0400067E RID: 1662
		private float playBurstLevel;

		// Token: 0x0400067F RID: 1663
		protected float smokeForce;

		// Token: 0x04000680 RID: 1664
		private float fireForce;

		// Token: 0x04000681 RID: 1665
		private int treatAsVehicleStatus = -1;

		// Token: 0x04000682 RID: 1666
		private int sfxLoopUpdateCounter;

		// Token: 0x04000683 RID: 1667
		private const int SFX_LOOP_UPDATE_INTERVAL = 5;

		// Token: 0x04000684 RID: 1668
		private RocketMetaData rocketMeta;

		// Token: 0x04000685 RID: 1669
		protected Color smokeColor = Color.white;

		// Token: 0x04000686 RID: 1670
		protected bool setSmokeColor;

		// Token: 0x04000687 RID: 1671
		protected int smokeColorMeshIndex;

		// Token: 0x04000688 RID: 1672
		protected bool emitSmoke = true;

		// Token: 0x04000689 RID: 1673
		private Vector3 exitOffset = Vector3.zero;

		// Token: 0x0400068A RID: 1674
		private float smokeSizeMultiplier = 1f;

		// Token: 0x0400068B RID: 1675
		private float smokeSpeedMultiplier = 1f;

		// Token: 0x0400068C RID: 1676
		private float particlesPerSecond = 50f;
	}
}
