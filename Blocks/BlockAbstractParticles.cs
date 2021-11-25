using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000065 RID: 101
	public class BlockAbstractParticles : Block
	{
		// Token: 0x06000838 RID: 2104 RVA: 0x00039F64 File Offset: 0x00038364
		public BlockAbstractParticles(List<List<Tile>> tiles, string particlePathName = "Particles/Water Stream", bool shouldHide = false, int meshIndex = 0, string audioName = "815 Water Jet") : base(tiles)
		{
			this.meshIndexToColor = meshIndex;
			this.emitterGO = (UnityEngine.Object.Instantiate(Resources.Load(particlePathName)) as GameObject);
			this.particles = this.emitterGO.GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < this.particles.Length; i++)
			{
				this.particles[i].playOnAwake = false;
				ParticleParameters particleParameters = new ParticleParameters();
				particleParameters.storeSize = this.particles[i].startSize;
				particleParameters.storeSpeed = this.particles[i].startSpeed;
				particleParameters.storeLife = this.particles[i].startLifetime;
				particleParameters.storeEmission = this.particles[i].emissionRate;
				particleParameters.storeEmitTimes = this.particles[i].emissionRate / 60f;
				particleParameters.storeColor = this.particles[i].startColor;
				particleParameters.storeOriginalColor = this.particles[i].startColor;
				particleParameters.storeMaxParts = this.particles[i].maxParticles;
				BlockAbstractParticles.particleParameters.Add(this.particles[i], particleParameters);
				this.particleEmitTimes.Add(0f);
			}
			if (this.go.GetComponent<Renderer>() != null)
			{
				string paint = this.GetPaint(this.meshIndexToColor);
				this.particleTint = BlockAbstractParticles.GetParticleColor(paint);
			}
			this.hideEmitter = shouldHide;
			this.loopName = audioName;
			this.UpdateParticleColor();
			this.emitterGO.SetActive(false);
		}

		// Token: 0x06000839 RID: 2105 RVA: 0x0003A118 File Offset: 0x00038518
		public override void Destroy()
		{
			UnityEngine.Object.Destroy(this.emitterGO);
			base.Destroy();
		}

		// Token: 0x0600083A RID: 2106 RVA: 0x0003A12C File Offset: 0x0003852C
		public override void Play()
		{
			base.Play();
			for (int i = 0; i < this.connections.Count; i++)
			{
				this.connections[i].go.SetLayer(Layer.MeshEmitters, true);
			}
			this.go.SetLayer(Layer.MeshEmitters, true);
			this.storeRender = this.go.GetComponent<Renderer>();
			if (this.storeRender != null)
			{
				Bounds bounds = this.storeRender.bounds;
				Mesh mesh = this.go.GetComponent<MeshFilter>().mesh;
				if (mesh != null)
				{
					bounds = mesh.bounds;
				}
				this.baseSize = new Vector3(bounds.size.x * 0.5f, bounds.size.y * 0.5f, bounds.size.z * 0.5f);
			}
			this.isFiring = true;
			this.shouldEmit = true;
			this.ShowHideEmitter(false);
			this.emitterGO.SetActive(true);
			this.UpdateEmitTimes();
		}

		// Token: 0x0600083B RID: 2107 RVA: 0x0003A248 File Offset: 0x00038648
		public override void Stop(bool resetBlock = true)
		{
			for (int i = 0; i < this.connections.Count; i++)
			{
				this.connections[i].go.SetLayer(Layer.Default, true);
			}
			this.go.SetLayer(Layer.Default, true);
			base.Stop(resetBlock);
			for (int j = 0; j < this.particles.Length; j++)
			{
				this.particles[j].Stop();
				this.particles[j].Clear();
			}
			this.PlayLoopSound(false, base.GetLoopClip(), 1f, null, 1f);
			this.ShowHideEmitter(true);
			this.emitterGO.SetActive(false);
		}

		// Token: 0x0600083C RID: 2108 RVA: 0x0003A2FB File Offset: 0x000386FB
		public bool HidingEmitter()
		{
			return this.hideEmitter;
		}

		// Token: 0x0600083D RID: 2109 RVA: 0x0003A304 File Offset: 0x00038704
		private void ShowHideEmitter(bool hide)
		{
			if (!this.hideEmitter)
			{
				return;
			}
			Renderer[] componentsInChildren = this.go.GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer in componentsInChildren)
			{
				renderer.enabled = hide;
			}
			List<Collider> colliders = this.GetColliders();
			foreach (Collider collider in colliders)
			{
				collider.enabled = hide;
			}
		}

		// Token: 0x0600083E RID: 2110 RVA: 0x0003A3A0 File Offset: 0x000387A0
		public override void Pause()
		{
			for (int i = 0; i < this.particles.Length; i++)
			{
				this.particles[i].Pause();
			}
			this.PlayLoopSound(false, base.GetLoopClip(), 1f, null, 1f);
		}

		// Token: 0x0600083F RID: 2111 RVA: 0x0003A3EC File Offset: 0x000387EC
		public override void Resume()
		{
			for (int i = 0; i < this.particles.Length; i++)
			{
				this.particles[i].Play();
			}
		}

		// Token: 0x06000840 RID: 2112 RVA: 0x0003A420 File Offset: 0x00038820
		public override void ResetFrame()
		{
			this.isFiring = false;
			this.particleAngleModifier = 0f;
			if (this.gotSpread)
			{
				this.gotSpread = false;
			}
			else if (this.particleSpreadModifier != 0f)
			{
				this.particleSpreadModifier = 0f;
				this.UpdateEmitTimes();
			}
		}

		// Token: 0x06000841 RID: 2113 RVA: 0x0003A478 File Offset: 0x00038878
		public TileResultCode IsFiring(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (!BlockWater.BlockWithinWater(this, false))
			{
				float num = (args.Length <= 0) ? 1f : ((float)args[0]);
				float num2 = eInfo.floatArg * num;
				if (num2 != this.particleSpeedModifier)
				{
					this.particleSpeedModifier = num2;
					this.UpdateEmitTimes();
				}
				this.isFiring = true;
			}
			return TileResultCode.True;
		}

		// Token: 0x06000842 RID: 2114 RVA: 0x0003A4D8 File Offset: 0x000388D8
		public TileResultCode ParticleSpread(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float num = (args.Length <= 0) ? 0f : ((float)args[0]);
			float num2 = eInfo.floatArg * num;
			this.gotSpread = true;
			if (num2 != this.particleSpreadModifier)
			{
				this.particleSpreadModifier = num2;
				this.UpdateEmitTimes();
			}
			return TileResultCode.True;
		}

		// Token: 0x06000843 RID: 2115 RVA: 0x0003A52C File Offset: 0x0003892C
		public TileResultCode ParticleAngle(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float num = (args.Length <= 0) ? 1f : ((float)args[0]);
			float num2 = eInfo.floatArg * num;
			if (num2 != this.particleAngleModifier)
			{
				this.particleAngleModifier = num2;
			}
			return TileResultCode.True;
		}

		// Token: 0x06000844 RID: 2116 RVA: 0x0003A574 File Offset: 0x00038974
		public override TileResultCode PaintTo(string paint, bool permanent, int meshIndex = 0)
		{
			TileResultCode result = base.PaintTo(paint, permanent, meshIndex);
			paint = paint.Replace("Luminous ", string.Empty);
			this.particleTint = BlockAbstractParticles.GetParticleColor(paint);
			if (meshIndex == this.meshIndexToColor && this.particles != null)
			{
				this.UpdateParticleColor();
			}
			if (this.hideEmitter && Blocksworld.CurrentState == State.Play)
			{
				this.ShowHideEmitter(false);
			}
			return result;
		}

		// Token: 0x06000845 RID: 2117 RVA: 0x0003A5E4 File Offset: 0x000389E4
		public override TileResultCode ExplodeLocal(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.shouldEmit = false;
			return base.ExplodeLocal(eInfo, args);
		}

		// Token: 0x06000846 RID: 2118 RVA: 0x0003A604 File Offset: 0x00038A04
		public override TileResultCode Explode(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.shouldEmit = false;
			return base.Explode(eInfo, args);
		}

		// Token: 0x06000847 RID: 2119 RVA: 0x0003A624 File Offset: 0x00038A24
		public override TileResultCode TextureTo(string texture, Vector3 normal, bool permanent, int meshIndex = 0, bool force = false)
		{
			if (this.hideEmitter)
			{
				if (Blocksworld.CurrentState == State.Play)
				{
					ShaderType shader = ShaderType.Normal;
					if (!Materials.shaders.TryGetValue(texture, out shader))
					{
						ResourceLoader.LoadTexture(texture, "Textures");
					}
					string paint = this.GetPaint(0);
					Material material = Materials.GetMaterial(paint, texture, shader);
					if (material != null)
					{
						this.go.GetComponent<Renderer>().sharedMaterial = material;
					}
					return TileResultCode.True;
				}
				texture = "Volume";
			}
			return base.TextureTo(texture, normal, permanent, meshIndex, force);
		}

		// Token: 0x06000848 RID: 2120 RVA: 0x0003A6AF File Offset: 0x00038AAF
		public override TileResultCode AppearModel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.shouldEmit = true;
			if (this.hideEmitter)
			{
				return TileResultCode.True;
			}
			return base.AppearModel(eInfo, args);
		}

		// Token: 0x06000849 RID: 2121 RVA: 0x0003A6CD File Offset: 0x00038ACD
		public override TileResultCode VanishModel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.shouldEmit = false;
			if (this.hideEmitter)
			{
				return TileResultCode.True;
			}
			return base.VanishModel(eInfo, args);
		}

		// Token: 0x0600084A RID: 2122 RVA: 0x0003A6EC File Offset: 0x00038AEC
		private void UpdateEmitTimes()
		{
			for (int i = 0; i < this.particles.Length; i++)
			{
				if (BlockAbstractParticles.particleParameters.ContainsKey(this.particles[i]))
				{
					ParticleParameters particleParameters = BlockAbstractParticles.particleParameters[this.particles[i]];
					float num = Mathf.Clamp(this.particleSpeedModifier, 1f, 100f);
					float num2 = Mathf.Clamp(this.particleSpreadModifier * 10f, 1f, 10f);
					float storeEmitTimes = 1f / (particleParameters.storeEmission * num * num2);
					particleParameters.storeEmitTimes = storeEmitTimes;
				}
			}
		}

		// Token: 0x0600084B RID: 2123 RVA: 0x0003A788 File Offset: 0x00038B88
		private void UpdateParticleColor()
		{
			for (int i = 0; i < this.particles.Length; i++)
			{
				float b = 0.5f;
				if (BlockAbstractParticles.particleParameters.ContainsKey(this.particles[i]))
				{
					Color storeOriginalColor = BlockAbstractParticles.particleParameters[this.particles[i]].storeOriginalColor;
					Color color = this.particleTint + storeOriginalColor * b;
					if (storeOriginalColor != Color.white)
					{
						BlockAbstractParticles.particleParameters[this.particles[i]].storeColor = color;
					}
					if (this.particles[i].name.Contains("SubEmitter"))
					{
						this.particles[i].startColor = color;
					}
				}
			}
		}

		// Token: 0x0600084C RID: 2124 RVA: 0x0003A848 File Offset: 0x00038C48
		public static Color GetParticleColor(string paint)
		{
			Color white = Color.white;
			return Blocksworld.getColor(paint);
		}

		// Token: 0x0600084D RID: 2125 RVA: 0x0003A864 File Offset: 0x00038C64
		private void UpdateParticlePosition()
		{
			if (this.emitterGO != null)
			{
				this.emitterGO.transform.position = this.goT.position;
				this.emitterGO.transform.rotation = this.goT.rotation;
				this.emitterGO.transform.rotation *= Quaternion.Euler(-this.particleAngleModifier, 0f, 0f);
			}
		}

		// Token: 0x0600084E RID: 2126 RVA: 0x0003A8EC File Offset: 0x00038CEC
		private void EmitParticle(ParticleSystem ourSystem)
		{
			ParticleParameters particleParameters = BlockAbstractParticles.particleParameters[ourSystem];
			Transform transform = ourSystem.transform;
			Vector3 b = new Vector3(UnityEngine.Random.Range(-this.particleSpreadModifier, this.particleSpreadModifier), UnityEngine.Random.Range(-this.particleSpreadModifier, this.particleSpreadModifier), 0f);
			Vector3 velocity = transform.TransformDirection(Vector3.forward + b).normalized * particleParameters.storeSpeed * this.particleSpeedModifier;
			float num = Mathf.Clamp(particleParameters.storeLife * (this.particleSpeedModifier * 0.75f), 1f, 5f);
			float num2 = Mathf.Clamp(this.particleSpreadModifier * 100f / 20f, 1f, 1.5f);
			float size = particleParameters.storeSize * this.particleSpeedModifier * num2;
			Vector3 position = this.goT.position + this.goT.TransformDirection(new Vector3(UnityEngine.Random.Range(-this.baseSize.x, this.baseSize.x), UnityEngine.Random.Range(-this.baseSize.y, this.baseSize.y), 0.1f));
			ParticleSystem.Particle particle = new ParticleSystem.Particle
			{
				position = position,
				velocity = velocity,
				size = size,
				remainingLifetime = num,
				startLifetime = num,
				color = particleParameters.storeColor,
				randomSeed = (uint)UnityEngine.Random.Range(17, 9999999)
			};
			ourSystem.Emit(particle);
		}

		// Token: 0x0600084F RID: 2127 RVA: 0x0003AA8C File Offset: 0x00038E8C
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (this.particles.Length < 1)
			{
				return;
			}
			this.UpdateParticlePosition();
			bool flag = TreasureHandler.IsPartOfPickedUpTreasureModel(this);
			if (this.isFiring && this.shouldEmit && !flag)
			{
				if (Sound.sfxEnabled)
				{
					this.PlayLoopSound(true, base.GetLoopClip(), 1f, null, 1f);
				}
				for (int i = 0; i < Mathf.Min(this.particles.Length, this.particleEmitTimes.Count); i++)
				{
					List<float> list;
					int index;
					(list = this.particleEmitTimes)[index = i] = list[index] + Time.deltaTime;
					if (BlockAbstractParticles.particleParameters.ContainsKey(this.particles[i]))
					{
						float storeEmitTimes = BlockAbstractParticles.particleParameters[this.particles[i]].storeEmitTimes;
						if (this.particleEmitTimes[i] > storeEmitTimes)
						{
							this.particles[i].maxParticles = (int)((float)BlockAbstractParticles.particleParameters[this.particles[i]].storeMaxParts * Mathf.Max(new float[]
							{
								this.particleSpeedModifier * this.particleSpeedModifier,
								1f,
								1.5f
							}));
							this.EmitParticle(this.particles[i]);
							this.particleEmitTimes[i] = 0f;
						}
					}
				}
			}
			else
			{
				this.PlayLoopSound(false, base.GetLoopClip(), 1f, null, 1f);
			}
		}

		// Token: 0x0400064C RID: 1612
		private GameObject emitterGO;

		// Token: 0x0400064D RID: 1613
		private bool isFiring;

		// Token: 0x0400064E RID: 1614
		private bool gotSpread;

		// Token: 0x0400064F RID: 1615
		private int meshIndexToColor;

		// Token: 0x04000650 RID: 1616
		private float particleSpeedModifier = 1f;

		// Token: 0x04000651 RID: 1617
		private float particleAngleModifier;

		// Token: 0x04000652 RID: 1618
		private float particleSpreadModifier;

		// Token: 0x04000653 RID: 1619
		private Vector3 baseSize = Vector3.zero;

		// Token: 0x04000654 RID: 1620
		private Renderer storeRender;

		// Token: 0x04000655 RID: 1621
		private bool hideEmitter;

		// Token: 0x04000656 RID: 1622
		private bool shouldEmit = true;

		// Token: 0x04000657 RID: 1623
		private ParticleSystem[] particles;

		// Token: 0x04000658 RID: 1624
		private Color particleTint = Color.white;

		// Token: 0x04000659 RID: 1625
		private List<float> particleEmitTimes = new List<float>();

		// Token: 0x0400065A RID: 1626
		private static Dictionary<ParticleSystem, ParticleParameters> particleParameters = new Dictionary<ParticleSystem, ParticleParameters>();
	}
}
