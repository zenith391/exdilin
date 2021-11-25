using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000054 RID: 84
	public abstract class BlockAbstractJetpack : BlockAbstractAntiGravity
	{
		// Token: 0x060006ED RID: 1773 RVA: 0x0002EFE8 File Offset: 0x0002D3E8
		public BlockAbstractJetpack(List<List<Tile>> tiles) : base(tiles)
		{
			this.playLoop = false;
			this.informAboutVaryingGravity = false;
			this.transform = this.goT;
			this.jetpackMeta = this.go.GetComponent<JetpackMetaData>();
			if (this.jetpackMeta != null)
			{
				this.localOffsets = this.jetpackMeta.effectOffsets;
				this.particleName = this.jetpackMeta.particleName;
				this.setParticleColor = this.jetpackMeta.inheritColor;
			}
		}

		// Token: 0x060006EE RID: 1774 RVA: 0x0002F0EC File Offset: 0x0002D4EC
		public override void Play()
		{
			this.chunkRB = this.chunk.rb;
			base.Play();
			if (this.setParticleColor)
			{
				this.UpdateParticleColorPaint(this.GetPaint(this.colorMeshIndex), this.colorMeshIndex);
				this.UpdateParticleColorTexture(base.GetTexture(this.colorMeshIndex), this.colorMeshIndex);
			}
		}

		// Token: 0x060006EF RID: 1775 RVA: 0x0002F14C File Offset: 0x0002D54C
		public static Color GetParticleColor(string paint, Renderer renderer)
		{
			Color color = renderer.sharedMaterial.color * 1.3f;
			if (Blocksworld.IsLuminousPaint(paint))
			{
				color += color;
			}
			return color;
		}

		// Token: 0x060006F0 RID: 1776 RVA: 0x0002F184 File Offset: 0x0002D584
		private void UpdateParticleColorPaint(string paint, int meshIndex)
		{
			if (this.setParticleColor && meshIndex == this.colorMeshIndex && this.subMeshGameObjects.Count > this.colorMeshIndex)
			{
				Renderer renderer = (meshIndex != 0) ? this.subMeshGameObjects[meshIndex - 1].GetComponent<Renderer>() : this.go.GetComponent<Renderer>();
				this.particleColor = BlockAbstractJetpack.GetParticleColor(paint, renderer);
			}
		}

		// Token: 0x060006F1 RID: 1777 RVA: 0x0002F1F5 File Offset: 0x0002D5F5
		private void UpdateParticleColorTexture(string texture, int meshIndex)
		{
			if (this.setParticleColor && meshIndex == this.colorMeshIndex && this.subMeshGameObjects.Count > this.colorMeshIndex)
			{
				this.emitParticle = (texture != "Glass");
			}
		}

		// Token: 0x060006F2 RID: 1778 RVA: 0x0002F238 File Offset: 0x0002D638
		public override TileResultCode TextureTo(string texture, Vector3 normal, bool permanent, int meshIndex = 0, bool force = false)
		{
			TileResultCode result = base.TextureTo(texture, normal, permanent, meshIndex, force);
			this.UpdateParticleColorTexture(texture, meshIndex);
			return result;
		}

		// Token: 0x060006F3 RID: 1779 RVA: 0x0002F260 File Offset: 0x0002D660
		public override TileResultCode PaintTo(string paint, bool permanent, int meshIndex = 0)
		{
			TileResultCode result = base.PaintTo(paint, permanent, meshIndex);
			this.UpdateParticleColorPaint(paint, meshIndex);
			return result;
		}

		// Token: 0x060006F4 RID: 1780 RVA: 0x0002F280 File Offset: 0x0002D680
		public override void Stop(bool resetBlock)
		{
			base.Stop(resetBlock);
			if (this.particles != null)
			{
				if (this.particles.isPlaying)
				{
					this.particles.Stop();
				}
				this.particles.Clear();
				UnityEngine.Object.Destroy(this.particles.gameObject);
			}
		}

		// Token: 0x060006F5 RID: 1781 RVA: 0x0002F2DB File Offset: 0x0002D6DB
		public override void Pause()
		{
			base.Pause();
			if (this.particles != null && this.particles.isPlaying)
			{
				this.particles.Pause();
			}
		}

		// Token: 0x060006F6 RID: 1782 RVA: 0x0002F30F File Offset: 0x0002D70F
		public override void ResetFrame()
		{
			base.ResetFrame();
			this.enabled = false;
		}

		// Token: 0x060006F7 RID: 1783 RVA: 0x0002F31E File Offset: 0x0002D71E
		public override void Resume()
		{
			base.Resume();
		}

		// Token: 0x060006F8 RID: 1784 RVA: 0x0002F326 File Offset: 0x0002D726
		public TileResultCode EmitSmoke(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.enabled = true;
			return TileResultCode.True;
		}

		// Token: 0x060006F9 RID: 1785 RVA: 0x0002F330 File Offset: 0x0002D730
		private ParticleSystem SpawnParticle()
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("VFX/" + this.particleName)) as GameObject;
			if (gameObject != null)
			{
				ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
				if (component != null)
				{
					this.particleSpeed = component.startSpeed;
					this.particleLifetime = component.startLifetime;
					return component;
				}
			}
			return null;
		}

		// Token: 0x060006FA RID: 1786 RVA: 0x0002F397 File Offset: 0x0002D797
		public override void Update()
		{
			base.Update();
			if (this.particles != null)
			{
			}
		}

		// Token: 0x060006FB RID: 1787 RVA: 0x0002F3B0 File Offset: 0x0002D7B0
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (this.paused)
			{
				return;
			}
			if (this.particles == null)
			{
				this.particles = this.SpawnParticle();
				return;
			}
			if (this.transform == null || !this.enabled || !this.emitParticle)
			{
				this.particles.Stop();
				return;
			}
			int particleCount = this.particles.particleCount;
			if (particleCount >= 100)
			{
				return;
			}
			float num = 1f - (float)particleCount / 100f;
			float num2 = num;
			Vector3 b = (!(this.chunkRB != null)) ? Vector3.zero : this.chunk.rb.velocity;
			while (num2 > 0f)
			{
				num2 -= 1f;
				Vector3 cameraPosition = Blocksworld.cameraPosition;
				Vector3 vector = this.transform.position;
				float magnitude = (vector - cameraPosition).magnitude;
				if (magnitude <= Blocksworld.fogEnd)
				{
					if (magnitude <= 15f || GeometryUtility.TestPlanesAABB(Blocksworld.frustumPlanes, new Bounds(vector, Vector3.one)))
					{
						for (int i = 0; i < this.localOffsets.Length; i++)
						{
							Vector3 direction = this.localOffsets[i];
							Vector3 b2 = this.transform.TransformDirection(direction);
							vector = this.transform.position + b2;
							float num3 = UnityEngine.Random.Range(this.particleLifetime - 0.05f, this.particleLifetime + 0.05f);
							float size = UnityEngine.Random.Range(1f, 1.25f);
							float y = UnityEngine.Random.Range(this.particleSpeed + 0.25f, this.particleSpeed - 0.25f);
							Vector3 velocity = this.transform.TransformDirection(new Vector3(this.GetRandomXZ(), y, this.GetRandomXZ())) + b;
							ParticleSystem.Particle particle = new ParticleSystem.Particle
							{
								remainingLifetime = num3,
								color = this.particleColor,
								position = vector,
								velocity = velocity,
								rotation = UnityEngine.Random.Range(0f, 360f),
								startLifetime = num3,
								size = size,
								randomSeed = (uint)UnityEngine.Random.Range(12, 21314)
							};
							this.particles.Emit(particle);
						}
					}
				}
			}
		}

		// Token: 0x060006FC RID: 1788 RVA: 0x0002F639 File Offset: 0x0002DA39
		private float GetRandomXZ()
		{
			return UnityEngine.Random.Range(0.1f, -0.1f);
		}

		// Token: 0x04000517 RID: 1303
		private ParticleSystem particles;

		// Token: 0x04000518 RID: 1304
		private Vector3[] localOffsets = new Vector3[]
		{
			new Vector3(-0.325f, -0.4f, -0.8f),
			new Vector3(0.325f, -0.4f, -0.8f)
		};

		// Token: 0x04000519 RID: 1305
		private float particleSpeed = -3.5f;

		// Token: 0x0400051A RID: 1306
		private float particleLifetime = 1f;

		// Token: 0x0400051B RID: 1307
		private const int MAX_PARTICLES = 100;

		// Token: 0x0400051C RID: 1308
		private bool enabled;

		// Token: 0x0400051D RID: 1309
		private bool setParticleColor;

		// Token: 0x0400051E RID: 1310
		private bool paused;

		// Token: 0x0400051F RID: 1311
		private bool emitParticle = true;

		// Token: 0x04000520 RID: 1312
		private int colorMeshIndex;

		// Token: 0x04000521 RID: 1313
		private Color particleColor = Color.white;

		// Token: 0x04000522 RID: 1314
		private Transform transform;

		// Token: 0x04000523 RID: 1315
		private Rigidbody chunkRB;

		// Token: 0x04000524 RID: 1316
		private JetpackMetaData jetpackMeta;

		// Token: 0x04000525 RID: 1317
		private string particleName = "Jetpack Smoke";
	}
}
