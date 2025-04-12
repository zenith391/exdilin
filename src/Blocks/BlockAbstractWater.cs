using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000070 RID: 112
	public class BlockAbstractWater : Block
	{
		// Token: 0x060008F8 RID: 2296 RVA: 0x0003EC9C File Offset: 0x0003D09C
		public BlockAbstractWater(List<List<Tile>> tiles) : base(tiles)
		{
			this.bubblePsObject = (UnityEngine.Object.Instantiate(Resources.Load("Blocks/BlockWater Particle System")) as GameObject);
			this.bubblePs = this.bubblePsObject.GetComponent<ParticleSystem>();
			this.bubblePs.enableEmission = false;
			this.bubbleSfxObject = new GameObject(this.go.name + " bubbles");
			this.bubbleSfxAudio = this.bubbleSfxObject.AddComponent<AudioSource>();
			this.bubbleSfxAudio.dopplerLevel = 0f;
			this.bubbleSfxAudio.playOnAwake = false;
			this.bubbleSfxAudio.volume = 0f;
			this.bubbleSfxAudio.loop = true;
		}

		// Token: 0x060008F9 RID: 2297 RVA: 0x0003EDC0 File Offset: 0x0003D1C0
		public override void Play()
		{
			base.Play();
			this.maxPositiveWaterLevelOffset = 3f;
			this.maxNegativeWaterLevelOffset = 3f;
			this.maxPositiveWaterLevelOffsetSet = false;
			this.maxNegativeWaterLevelOffsetSet = false;
			this.waterLevelOffset = 0f;
			this.SetLiquidProperties((!this.isLava) ? "Water" : "Lava");
			this.GetBubbleClip();
			this.withinClip = this.bubbleSfxAudio.clip;
			this.aboveClip = null;
			this.playLoopAboveWater = false;
			string texture = base.GetTexture(0);
			if (texture == "Texture Water Stream")
			{
				this.aboveClip = Sound.GetSfx("Ocean Loop");
				this.playLoopAboveWater = true;
			}
		}

		// Token: 0x060008FA RID: 2298 RVA: 0x0003EE78 File Offset: 0x0003D278
		protected static void GatherAllBlocksWithWaterSensors()
		{
			if (BlockAbstractWater.blocksWithWaterSensors == null)
			{
				HashSet<Predicate> fewPreds = new HashSet<Predicate>
				{
					Block.predicateWithinWater,
					Block.predicateModelWithinWater,
					Block.predicateWithinTaggedWater,
					Block.predicateModelWithinTaggedWater
				};
				HashSet<Predicate> fewPreds2 = new HashSet<Predicate>
				{
					Block.predicateModelWithinWater,
					Block.predicateModelWithinTaggedWater
				};
				BlockAbstractWater.blocksWithWaterSensors = new HashSet<Block>();
				foreach (Block block in BWSceneManager.AllBlocks())
				{
					if (block.ContainsTileWithAnyPredicateInPlayMode(fewPreds))
					{
						BlockAbstractWater.blocksWithWaterSensors.Add(block);
						if (block.ContainsTileWithAnyPredicateInPlayMode(fewPreds2))
						{
							block.UpdateConnectedCache();
							List<Block> other = Block.connectedCache[block];
							BlockAbstractWater.blocksWithWaterSensors.UnionWith(other);
						}
					}
				}
			}
		}

		// Token: 0x060008FB RID: 2299 RVA: 0x0003EF80 File Offset: 0x0003D380
		public override void Stop(bool resetBlock = true)
		{
			base.Stop(resetBlock);
			this.waterLevelOffset = 0f;
			this.IncreaseWaterLevel(0f);
			BlockAbstractWater.blocksWithWaterSensors = null;
		}

		// Token: 0x060008FC RID: 2300 RVA: 0x0003EFA8 File Offset: 0x0003D3A8
		public static bool CameraWithinAnyWater()
		{
			bool flag = BlockAbstractWater.cameraWithinOcean;
			if (!flag)
			{
				for (int i = 0; i < BlockAbstractWater.waterCubes.Count; i++)
				{
					BlockWaterCube blockWaterCube = BlockAbstractWater.waterCubes[i];
					if (blockWaterCube.cameraWasWithinWater)
					{
						return true;
					}
				}
			}
			return flag;
		}

		// Token: 0x060008FD RID: 2301 RVA: 0x0003EFF8 File Offset: 0x0003D3F8
		protected void GetBubbleClip()
		{
			string name = "Bubbles Loop";
			string texture = base.GetTexture(0);
			if (texture == "Texture Lava")
			{
				name = "Lava Bubbles Loop";
			}
			this.bubbleSfxAudio.clip = Sound.GetSfx(name);
		}

		// Token: 0x060008FE RID: 2302 RVA: 0x0003F03A File Offset: 0x0003D43A
		public override void Destroy()
		{
			UnityEngine.Object.Destroy(this.bubblePsObject);
			UnityEngine.Object.Destroy(this.bubbleSfxObject);
			base.Destroy();
		}

		// Token: 0x060008FF RID: 2303 RVA: 0x0003F058 File Offset: 0x0003D458
		public override void OnCreate()
		{
			base.OnCreate();
			this.GetBubbleClip();
		}

		// Token: 0x06000900 RID: 2304 RVA: 0x0003F066 File Offset: 0x0003D466
		public virtual Bounds GetWaterBounds()
		{
			return this.go.GetComponent<Collider>().bounds;
		}

		// Token: 0x06000901 RID: 2305 RVA: 0x0003F078 File Offset: 0x0003D478
		public TileResultCode SetLiquidProperties(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.SetLiquidProperties(Util.GetStringArg(args, 0, "Water"));
			return TileResultCode.True;
		}

		// Token: 0x06000902 RID: 2306 RVA: 0x0003F090 File Offset: 0x0003D490
		public void SetLiquidProperties(string type)
		{
			float density = 0.7f;
			float friction = 1f;
			if (type != null)
			{
				if (!(type == "Lava"))
				{
					if (!(type == "Jello"))
					{
						if (!(type == "Intangible"))
						{
							if (!(type == "Mud"))
							{
								if (type == "Quicksand")
								{
									density = 0.25f;
									friction = 20f;
								}
							}
							else
							{
								density = 1f;
								friction = 6f;
							}
						}
						else
						{
							density = 0f;
							friction = 0f;
						}
					}
					else
					{
						density = 20f;
						friction = 3f;
					}
				}
				else
				{
					density = 0.9f;
					friction = 3f;
				}
			}
			this.SetDensity(density);
			this.SetFriction(friction);
		}

		// Token: 0x06000903 RID: 2307 RVA: 0x0003F167 File Offset: 0x0003D567
		public TileResultCode SetMaxPositiveWaterLevelOffset(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.SetMaxPositiveWaterLevelOffset(eInfo.floatArg * Util.GetFloatArg(args, 0, 10f));
			return TileResultCode.True;
		}

		// Token: 0x06000904 RID: 2308 RVA: 0x0003F183 File Offset: 0x0003D583
		public TileResultCode SetMaxNegativeWaterLevelOffset(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.SetMaxNegativeWaterLevelOffset(eInfo.floatArg * Util.GetFloatArg(args, 0, 10f));
			return TileResultCode.True;
		}

		// Token: 0x06000905 RID: 2309 RVA: 0x0003F19F File Offset: 0x0003D59F
		public TileResultCode AtMaxPositiveWaterLevelOffset(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!this.AtMaxPositiveWaterLevelOffset()) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000906 RID: 2310 RVA: 0x0003F1B3 File Offset: 0x0003D5B3
		public TileResultCode AtMaxNegativeWaterLevelOffset(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!this.AtMaxNegativeWaterLevelOffset()) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000907 RID: 2311 RVA: 0x0003F1C7 File Offset: 0x0003D5C7
		public bool AtMaxPositiveWaterLevelOffset()
		{
			return this.maxPositiveWaterLevelOffsetSet && Mathf.Abs(this.waterLevelOffset - this.maxPositiveWaterLevelOffset) < 0.05f;
		}

		// Token: 0x06000908 RID: 2312 RVA: 0x0003F1EF File Offset: 0x0003D5EF
		public bool AtMaxNegativeWaterLevelOffset()
		{
			return this.maxNegativeWaterLevelOffsetSet && Mathf.Abs(this.waterLevelOffset + this.maxNegativeWaterLevelOffset) < 0.05f;
		}

		// Token: 0x06000909 RID: 2313 RVA: 0x0003F217 File Offset: 0x0003D617
		public void SetMaxPositiveWaterLevelOffset(float offset)
		{
			this.maxPositiveWaterLevelOffsetSet = true;
			this.maxPositiveWaterLevelOffset = offset;
			if (this.waterLevelOffset > this.maxPositiveWaterLevelOffset)
			{
				this.SetWaterLevelOffset(this.maxPositiveWaterLevelOffset);
			}
		}

		// Token: 0x0600090A RID: 2314 RVA: 0x0003F244 File Offset: 0x0003D644
		public void SetMaxNegativeWaterLevelOffset(float offset)
		{
			this.maxNegativeWaterLevelOffsetSet = true;
			this.maxNegativeWaterLevelOffset = offset;
			if (this.waterLevelOffset < -this.maxNegativeWaterLevelOffset)
			{
				this.SetWaterLevelOffset(-this.maxNegativeWaterLevelOffset);
			}
		}

		// Token: 0x0600090B RID: 2315 RVA: 0x0003F274 File Offset: 0x0003D674
		public void IncreaseWaterLevel(float inc)
		{
			if (!this.isSolid)
			{
				float num = this.waterLevelOffset + inc;
				this.SetWaterLevelOffset(num);
			}
		}

		// Token: 0x0600090C RID: 2316 RVA: 0x0003F29C File Offset: 0x0003D69C
		protected void UpdateSounds(bool within)
		{
			if (Sound.sfxEnabled && Blocksworld.CurrentState == State.Play)
			{
				float num = 0.1f;
				if (!within && !this.playLoopAboveWater)
				{
					num *= -1f;
				}
				float max = 1f;
				if (!within && this.playLoopAboveWater && this.aboveClip != null && this.bubbleSfxAudio.clip != this.aboveClip)
				{
					this.bubbleSfxAudio.clip = this.aboveClip;
				}
				else if (within && this.withinClip != null && this.bubbleSfxAudio.clip != this.withinClip)
				{
					this.bubbleSfxAudio.clip = this.withinClip;
				}
				if (!within && this.playLoopAboveWater)
				{
					float num2 = Mathf.Abs(Blocksworld.cameraPosition.y - this.GetWaterBounds().max.y);
					max = 0.1f / (0.1f + num2 * 0.02f);
				}
				float num3 = this.bubbleSfxAudio.volume;
				num3 = Mathf.Clamp(num3 + num, 0f, max);
				this.bubbleSfxAudio.volume = num3;
				if (num3 > 0.01f && !this.bubbleSfxAudio.isPlaying && num > 0f)
				{
					this.bubbleSfxAudio.Play();
				}
				else if (num3 < 0.01f && num < 0f && this.bubbleSfxAudio.isPlaying)
				{
					this.bubbleSfxAudio.Stop();
				}
			}
			else
			{
				this.bubbleSfxAudio.volume = 0f;
				if (this.bubbleSfxAudio.isPlaying)
				{
					this.bubbleSfxAudio.Stop();
				}
			}
		}

		// Token: 0x0600090D RID: 2317 RVA: 0x0003F48A File Offset: 0x0003D88A
		public override void Pause()
		{
			this.bubblePs.Pause();
		}

		// Token: 0x0600090E RID: 2318 RVA: 0x0003F497 File Offset: 0x0003D897
		public override void Resume()
		{
			this.bubblePs.Play();
		}

		// Token: 0x0600090F RID: 2319 RVA: 0x0003F4A4 File Offset: 0x0003D8A4
		protected void FindSplashAudioSources()
		{
			this.playingSplashInfos.Clear();
			for (int i = BlockAbstractWater.splashAudioSources.Count; i < 3; i++)
			{
				GameObject gameObject = new GameObject();
				AudioSource audioSource = gameObject.AddComponent<AudioSource>();
				audioSource.playOnAwake = false;
				Sound.SetWorldAudioSourceParams(audioSource, 5f, 150f, AudioRolloffMode.Logarithmic);
				BlockAbstractWater.splashAudioSources.Add(gameObject);
			}
			for (int j = 0; j < 3; j++)
			{
				AudioSource component = BlockAbstractWater.splashAudioSources[j].GetComponent<AudioSource>();
				int num = UnityEngine.Random.Range(1, 4);
				component.clip = Sound.GetSfx("Water Splash Medium " + num);
			}
		}

		// Token: 0x06000910 RID: 2320 RVA: 0x0003F554 File Offset: 0x0003D954
		protected void UpdateUnderwaterLightColors(bool w)
		{
			if (Blocksworld.renderingWater)
			{
				return;
			}
			if (w)
			{
				Color color = this.go.GetComponent<Renderer>().sharedMaterial.GetColor("_Color");
				this.lightColor = color;
				if (Blocksworld.IsLuminousPaint(this.GetPaint(0)))
				{
					this.emissiveLightColor = new Color(color.r, color.g, color.b, 0f);
					this.lightColor = Color.white;
				}
				else if (base.GetTexture(0) == "Texture Lava")
				{
					this.lightColor = Color.white;
				}
				else
				{
					this.emissiveLightColor = Block.transparent;
				}
			}
			else
			{
				this.lightColor = Color.white;
				this.emissiveLightColor = Block.transparent;
			}
			Blocksworld.UpdateLightColor(true);
		}

		// Token: 0x06000911 RID: 2321 RVA: 0x0003F62C File Offset: 0x0003DA2C
		public override Color GetEmissiveLightTint()
		{
			return this.emissiveLightColor;
		}

		// Token: 0x06000912 RID: 2322 RVA: 0x0003F634 File Offset: 0x0003DA34
		public override Color GetLightTint()
		{
			return this.lightColor;
		}

		// Token: 0x06000913 RID: 2323 RVA: 0x0003F63C File Offset: 0x0003DA3C
		public void SetDensity(float dens)
		{
			this.waterDensity = Mathf.Clamp(dens, 0f, 100f);
		}

		// Token: 0x06000914 RID: 2324 RVA: 0x0003F654 File Offset: 0x0003DA54
		public void SetFriction(float frict)
		{
			this.waterFriction = Mathf.Clamp(frict, 0f, 100f);
		}

		// Token: 0x06000915 RID: 2325 RVA: 0x0003F66C File Offset: 0x0003DA6C
		public TileResultCode SetDensity(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float density = eInfo.floatArg * Util.GetFloatArg(args, 0, 0.7f);
			this.SetDensity(density);
			return TileResultCode.True;
		}

		// Token: 0x06000916 RID: 2326 RVA: 0x0003F698 File Offset: 0x0003DA98
		public new TileResultCode SetFriction(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float friction = eInfo.floatArg * Util.GetFloatArg(args, 0, 1f);
			this.SetFriction(friction);
			return TileResultCode.True;
		}

		// Token: 0x06000917 RID: 2327 RVA: 0x0003F6C1 File Offset: 0x0003DAC1
		public float WaterLevelOffset(Vector3 pos)
		{
			if (this.hasSlowWaves)
			{
				return 0.25f + 0.2f * Mathf.Sin(0.1f * pos.x + Time.time);
			}
			return 0f;
		}

		// Token: 0x06000918 RID: 2328 RVA: 0x0003F6F8 File Offset: 0x0003DAF8
		protected virtual void SetWaterLevelOffset(float newOffset)
		{
			this.waterLevelOffset = this.ClampWaterLevelOffset(newOffset);
		}

		// Token: 0x06000919 RID: 2329 RVA: 0x0003F708 File Offset: 0x0003DB08
		protected float ClampWaterLevelOffset(float offset)
		{
			if (offset > 0f && this.maxPositiveWaterLevelOffsetSet)
			{
				offset = Mathf.Min(offset, this.maxPositiveWaterLevelOffset);
			}
			if (offset < 0f && this.maxNegativeWaterLevelOffsetSet)
			{
				offset = Mathf.Max(offset, -this.maxNegativeWaterLevelOffset);
			}
			return offset;
		}

		// Token: 0x0600091A RID: 2330 RVA: 0x0003F75F File Offset: 0x0003DB5F
		public override TileResultCode Freeze(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return TileResultCode.True;
		}

		// Token: 0x0600091B RID: 2331 RVA: 0x0003F762 File Offset: 0x0003DB62
		public override TileResultCode Explode(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return TileResultCode.True;
		}

		// Token: 0x04000702 RID: 1794
		protected bool bubbleSoundWithin = true;

		// Token: 0x04000703 RID: 1795
		protected bool playLoopAboveWater;

		// Token: 0x04000704 RID: 1796
		protected GameObject bubblePsObject;

		// Token: 0x04000705 RID: 1797
		public ParticleSystem bubblePs;

		// Token: 0x04000706 RID: 1798
		protected GameObject bubbleSfxObject;

		// Token: 0x04000707 RID: 1799
		protected AudioSource bubbleSfxAudio;

		// Token: 0x04000708 RID: 1800
		public static HashSet<Block> blocksWithWaterSensors;

		// Token: 0x04000709 RID: 1801
		protected Dictionary<int, WaterSplashInfo> splashInfos = new Dictionary<int, WaterSplashInfo>();

		// Token: 0x0400070A RID: 1802
		protected List<WaterSplashInfo> playingSplashInfos = new List<WaterSplashInfo>();

		// Token: 0x0400070B RID: 1803
		protected static List<GameObject> splashAudioSources = new List<GameObject>();

		// Token: 0x0400070C RID: 1804
		protected const float MAX_WATER_DENSITY = 100f;

		// Token: 0x0400070D RID: 1805
		protected const float MAX_WATER_FRICTION = 100f;

		// Token: 0x0400070E RID: 1806
		public const float DEFAULT_WATER_DENSITY = 0.7f;

		// Token: 0x0400070F RID: 1807
		protected float waterDensity = 0.7f;

		// Token: 0x04000710 RID: 1808
		protected const float LAVA_DENSITY = 0.9f;

		// Token: 0x04000711 RID: 1809
		protected const float LAVA_FRICTION = 3f;

		// Token: 0x04000712 RID: 1810
		public const float DEFAULT_WATER_FRICTION = 1f;

		// Token: 0x04000713 RID: 1811
		protected float waterFriction = 1f;

		// Token: 0x04000714 RID: 1812
		protected Vector3 streamVelocity = default(Vector3);

		// Token: 0x04000715 RID: 1813
		protected static bool cameraWithinOcean = false;

		// Token: 0x04000716 RID: 1814
		public static List<BlockWaterCube> waterCubes = new List<BlockWaterCube>();

		// Token: 0x04000717 RID: 1815
		protected bool hasSlowWaves;

		// Token: 0x04000718 RID: 1816
		public bool isSolid;

		// Token: 0x04000719 RID: 1817
		protected bool isLava;

		// Token: 0x0400071A RID: 1818
		protected bool maxPositiveWaterLevelOffsetSet;

		// Token: 0x0400071B RID: 1819
		protected bool maxNegativeWaterLevelOffsetSet;

		// Token: 0x0400071C RID: 1820
		protected float maxPositiveWaterLevelOffset = 3f;

		// Token: 0x0400071D RID: 1821
		protected float maxNegativeWaterLevelOffset = 3f;

		// Token: 0x0400071E RID: 1822
		protected float waterLevelOffset;

		// Token: 0x0400071F RID: 1823
		protected AudioClip withinClip;

		// Token: 0x04000720 RID: 1824
		protected AudioClip aboveClip;

		// Token: 0x04000721 RID: 1825
		private const string LAVA = "Lava";

		// Token: 0x04000722 RID: 1826
		private const string JELLO = "Jello";

		// Token: 0x04000723 RID: 1827
		private const string MUD = "Mud";

		// Token: 0x04000724 RID: 1828
		private const string WATER = "Water";

		// Token: 0x04000725 RID: 1829
		private const string INTANGIBLE = "Intangible";

		// Token: 0x04000726 RID: 1830
		private const string QUICKSAND = "Quicksand";

		// Token: 0x04000727 RID: 1831
		public bool cameraWasWithinWater;

		// Token: 0x04000728 RID: 1832
		protected Color lightColor = Color.white;

		// Token: 0x04000729 RID: 1833
		protected Color emissiveLightColor = Block.transparent;
	}
}
