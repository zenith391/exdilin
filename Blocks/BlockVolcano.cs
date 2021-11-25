using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020000E8 RID: 232
	public class BlockVolcano : BlockTerrain
	{
		// Token: 0x0600112A RID: 4394 RVA: 0x00076680 File Offset: 0x00074A80
		public BlockVolcano(List<List<Tile>> tiles) : base(tiles)
		{
			this.particleGo = (UnityEngine.Object.Instantiate(Resources.Load("Env Effect/Volcano Particle System")) as GameObject);
			this.smokePs = this.particleGo.transform.Find("Smoke").gameObject.GetComponent<ParticleSystem>();
			this.firePs = this.particleGo.transform.Find("Fire").gameObject.GetComponent<ParticleSystem>();
			this.origPsRotation = this.particleGo.transform.rotation;
			this.smokePs.enableEmission = false;
			this.firePs.enableEmission = false;
			this.SetFog(Blocksworld.fogStart, Blocksworld.fogEnd);
			this.UpdateFogColor(Blocksworld.fogColor);
			this.settings = this.go.GetComponent<VolcanoSettings>();
			this.eruptIntensity = 0f;
			this.cumulativeEruptIntensity = 0f;
			this.UpdateParticlePosition();
			this.loopName = "Rocket Burst 2 Loop";
		}

		// Token: 0x0600112B RID: 4395 RVA: 0x00076790 File Offset: 0x00074B90
		public new static void Register()
		{
			PredicateRegistry.Add<BlockVolcano>("Volcano.Erupt", null, (Block b) => new PredicateActionDelegate(((BlockVolcano)b).Erupt), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Intensity"
			}, null);
			Block.AddSimpleDefaultTiles(new GAF("Volcano.Erupt", new object[]
			{
				2f
			}), new string[]
			{
				"Volcano"
			});
		}

		// Token: 0x0600112C RID: 4396 RVA: 0x00076818 File Offset: 0x00074C18
		public override void Play()
		{
			base.Play();
			this.eruptPitch = 1f;
			this.eruptIntensity = 0f;
			this.cumulativeEruptIntensity = 0f;
			this.updateLoopCounter = UnityEngine.Random.Range(1, 10);
			this.pitchRnd = UnityEngine.Random.Range(0f, 1000f);
			BlockVolcano.particleCounts.Clear();
			this.eruptVolume = 0f;
			this.targetEruptVolume = 0f;
		}

		// Token: 0x0600112D RID: 4397 RVA: 0x00076890 File Offset: 0x00074C90
		public override void Stop(bool resetBlock = true)
		{
			base.Stop(resetBlock);
			this.eruptIntensity = 0f;
			this.cumulativeEruptIntensity = 0f;
			this.smokePs.Clear();
			this.firePs.Clear();
			BlockVolcano.particleCounts.Clear();
			this.PlayLoopSound(false, base.GetLoopClip(), 1f, null, 1f);
		}

		// Token: 0x0600112E RID: 4398 RVA: 0x000768F2 File Offset: 0x00074CF2
		public TileResultCode Erupt(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.cumulativeEruptIntensity += eInfo.floatArg * Util.GetFloatArg(args, 0, 1f);
			return TileResultCode.True;
		}

		// Token: 0x0600112F RID: 4399 RVA: 0x00076915 File Offset: 0x00074D15
		public override void SetFog(float start, float end)
		{
			base.SetFog(start, end);
			this.smokePs.GetComponent<Renderer>().material.SetFloat("_FogStart", start);
			this.smokePs.GetComponent<Renderer>().material.SetFloat("_FogEnd", end);
		}

		// Token: 0x06001130 RID: 4400 RVA: 0x00076958 File Offset: 0x00074D58
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			this.eruptIntensity = this.cumulativeEruptIntensity;
			this.updateLoopCounter++;
			bool flag = this.eruptIntensity > 0.01f && Sound.sfxEnabled && !this.vanished;
			if (this.updateLoopCounter % 5 == 0)
			{
				if (flag)
				{
					this.targetEruptVolume = Mathf.Min(this.eruptIntensity * 0.5f, this.sizeMultiplier * 0.05f);
					float num = 0.1f;
					this.eruptVolume = Mathf.Clamp(this.eruptVolume + Mathf.Clamp(this.targetEruptVolume - this.eruptVolume, -num, num), 0f, 1f);
					float num2 = Mathf.Max(3f - this.sizeMultiplier * 0.3f, 0.5f);
					this.eruptPitch = num2 + 0.1f * SimplexNoise.Noise(0.25f * Time.time, this.pitchRnd);
				}
				else
				{
					this.targetEruptVolume = 0f;
				}
			}
			if (flag)
			{
				this.PlaySound("Rocket Burst 2 Loop", "Block", "Loop", this.eruptVolume, this.eruptPitch, false, 0f);
			}
			else
			{
				this.eruptVolume = 0f;
			}
			this.cumulativeEruptIntensity = 0f;
		}

		// Token: 0x06001131 RID: 4401 RVA: 0x00076AB3 File Offset: 0x00074EB3
		public override void UpdateFogColor(Color newFogColor)
		{
			base.UpdateFogColor(newFogColor);
			this.smokePs.GetComponent<Renderer>().material.SetColor("_FogColor", newFogColor);
		}

		// Token: 0x06001132 RID: 4402 RVA: 0x00076AD8 File Offset: 0x00074ED8
		private void UpdateParticlePosition()
		{
			if (this.particleGo != null)
			{
				Vector3 vector = base.Scale();
				Vector3 relativeOffset = this.settings.relativeOffset;
				Vector3 direction = new Vector3(vector.x * relativeOffset.x, vector.y * relativeOffset.y, vector.z * relativeOffset.z);
				Vector3 b = this.goT.TransformDirection(direction);
				Vector3 position = base.GetPosition();
				this.particleGo.transform.position = position + b;
				this.particleGo.transform.rotation = this.goT.rotation * this.origPsRotation;
				this.sizeMultiplier = Mathf.Abs((vector.x + vector.y + vector.z) / 3f);
			}
		}

		// Token: 0x06001133 RID: 4403 RVA: 0x00076BB8 File Offset: 0x00074FB8
		public override void Pause()
		{
			base.Pause();
			this.smokePs.Pause();
			this.firePs.Pause();
			this.PlayLoopSound(false, base.GetLoopClip(), 1f, null, 1f);
		}

		// Token: 0x06001134 RID: 4404 RVA: 0x00076BEE File Offset: 0x00074FEE
		public override void Resume()
		{
			base.Resume();
			this.smokePs.Play();
			this.firePs.Play();
		}

		// Token: 0x06001135 RID: 4405 RVA: 0x00076C0C File Offset: 0x0007500C
		public override void Update()
		{
			base.Update();
			if (Blocksworld.CurrentState != State.Play || this.vanished)
			{
				return;
			}
			Transform goT = this.goT;
			Vector3 position = goT.position;
			Vector3 cameraPosition = Blocksworld.cameraPosition;
			float num = Blocksworld.fogEnd * 1.2f + Util.MaxComponent(this.size);
			Vector3 a = position - cameraPosition;
			float magnitude = a.magnitude;
			Vector3 rhs = a / magnitude;
			BlockVolcano.particleCounts[this] = (float)(this.smokePs.particleCount + this.firePs.particleCount);
			float num2 = 0f;
			foreach (KeyValuePair<BlockVolcano, float> keyValuePair in BlockVolcano.particleCounts)
			{
				num2 += keyValuePair.Value;
			}
			float num3 = Mathf.Clamp(1f - num2 / 200f, 0f, 1f);
			if (magnitude < num)
			{
				if (Vector3.Dot(Blocksworld.cameraForward, rhs) <= 0f && magnitude > Util.MaxComponent(this.size) * 4f)
				{
					return;
				}
				this.smokeToEmit += num3 * this.eruptIntensity * this.settings.smokePerFrame;
				Vector3 position2 = this.particleGo.transform.position;
				int num4 = 0;
				int num5 = 0;
				while ((float)num5 < this.smokeToEmit)
				{
					float size = this.sizeMultiplier * UnityEngine.Random.Range(this.settings.smokeSizeRandomFrom, this.settings.smokeSizeRandomTo) + this.settings.smokeSizeBias;
					float d = this.sizeMultiplier * (UnityEngine.Random.Range(0.06f, 0.11f) * this.eruptIntensity + 0.02f);
					float num6 = Mathf.Clamp(this.sizeMultiplier * UnityEngine.Random.Range(this.settings.smokeLifetimeRandomFrom, this.settings.smokeLifetimeRandomTo), 1f, 3f);
					Vector3 vector = goT.TransformDirection(Vector3.up) * d;
					float num7 = 5f * this.eruptIntensity;
					vector = Quaternion.Euler(UnityEngine.Random.Range(-num7, num7), UnityEngine.Random.Range(-num7, num7), UnityEngine.Random.Range(-num7, num7)) * vector;
					ParticleSystem.Particle particle = default(ParticleSystem.Particle);
					particle.position = position2;
					particle.startLifetime = num6;
					particle.remainingLifetime = num6;
					particle.velocity = vector;
					particle.angularVelocity = UnityEngine.Random.Range(this.settings.smokeAngularVelocityRandomFrom, this.settings.smokeAngularVelocityRandomTo);
					particle.size = size;
					particle.color = Color.white;
					particle.rotation = UnityEngine.Random.Range(this.settings.smokeRotationRandomFrom, this.settings.smokeRotationRandomTo);
					particle.randomSeed = (uint)UnityEngine.Random.Range(1, 999999);
					this.smokePs.Emit(particle);
					num4++;
					num5++;
				}
				this.smokeToEmit -= (float)num4;
				this.fireToEmit += num3 * this.eruptIntensity * this.settings.firePerFrame;
				int num8 = 0;
				int num9 = 0;
				while ((float)num9 < this.fireToEmit)
				{
					this.firePs.startSize = this.sizeMultiplier * UnityEngine.Random.Range(this.settings.fireSizeRandomFrom, this.settings.fireSizeRandomTo);
					this.firePs.startSpeed = this.sizeMultiplier * (UnityEngine.Random.Range(0f, 0.3f) * this.eruptIntensity + 0.5f) + 2f;
					this.firePs.startLifetime = Mathf.Clamp(this.sizeMultiplier * UnityEngine.Random.Range(this.settings.fireLifetimeRandomFrom, this.settings.fireLifetimeRandomTo), 1.5f, 3f);
					this.firePs.Emit(1);
					num8++;
					num9++;
				}
				this.fireToEmit -= (float)num8;
			}
		}

		// Token: 0x06001136 RID: 4406 RVA: 0x00077044 File Offset: 0x00075444
		public override bool MoveTo(Vector3 pos)
		{
			bool result = base.MoveTo(pos);
			this.UpdateParticlePosition();
			return result;
		}

		// Token: 0x06001137 RID: 4407 RVA: 0x00077060 File Offset: 0x00075460
		public override bool RotateTo(Quaternion rot)
		{
			bool result = base.RotateTo(rot);
			this.UpdateParticlePosition();
			return result;
		}

		// Token: 0x06001138 RID: 4408 RVA: 0x0007707C File Offset: 0x0007547C
		public override bool ScaleTo(Vector3 scale, bool recalculateCollider = true, bool forceRescale = false)
		{
			bool result = base.ScaleTo(scale, recalculateCollider, forceRescale);
			this.UpdateParticlePosition();
			return result;
		}

		// Token: 0x06001139 RID: 4409 RVA: 0x0007709A File Offset: 0x0007549A
		public override void Destroy()
		{
			base.Destroy();
			if (this.particleGo != null)
			{
				UnityEngine.Object.Destroy(this.particleGo);
				this.particleGo = null;
			}
		}

		// Token: 0x04000D77 RID: 3447
		private GameObject particleGo;

		// Token: 0x04000D78 RID: 3448
		private ParticleSystem smokePs;

		// Token: 0x04000D79 RID: 3449
		private ParticleSystem firePs;

		// Token: 0x04000D7A RID: 3450
		private float eruptIntensity;

		// Token: 0x04000D7B RID: 3451
		private float cumulativeEruptIntensity;

		// Token: 0x04000D7C RID: 3452
		private float pitchRnd;

		// Token: 0x04000D7D RID: 3453
		private float smokeToEmit;

		// Token: 0x04000D7E RID: 3454
		private float fireToEmit;

		// Token: 0x04000D7F RID: 3455
		private float sizeMultiplier = 1f;

		// Token: 0x04000D80 RID: 3456
		private VolcanoSettings settings;

		// Token: 0x04000D81 RID: 3457
		private Quaternion origPsRotation;

		// Token: 0x04000D82 RID: 3458
		private static Dictionary<BlockVolcano, float> particleCounts = new Dictionary<BlockVolcano, float>();

		// Token: 0x04000D83 RID: 3459
		private const int MAX_PARTICLES = 200;

		// Token: 0x04000D84 RID: 3460
		private int updateLoopCounter;

		// Token: 0x04000D85 RID: 3461
		private float eruptPitch = 1f;

		// Token: 0x04000D86 RID: 3462
		private float eruptVolume;

		// Token: 0x04000D87 RID: 3463
		private float targetEruptVolume;
	}
}
