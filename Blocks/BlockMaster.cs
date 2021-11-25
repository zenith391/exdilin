using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020000A7 RID: 167
	public class BlockMaster : BlockAbstractUI
	{
		// Token: 0x06000CEF RID: 3311 RVA: 0x0005A3CC File Offset: 0x000587CC
		public BlockMaster(List<List<Tile>> tiles) : base(tiles, 0)
		{
		}

		// Token: 0x06000CF0 RID: 3312 RVA: 0x0005A424 File Offset: 0x00058824
		public override void Play()
		{
			base.Play();
			this.informedBlocksAboutVaryingGravity = false;
			this.ClearTerrainBlocks();
			this.earthQuakeStrength = 0f;
			this.cameraShakeStrength = 0f;
			this.prolongedEarthQuakeStrength = 0f;
			this.prolongedCameraShakeStrength = 0f;
			this.cameraShakeStartTime = -10f;
			this.earthQuakeStartTime = -10f;
			this.toEnvEffect = null;
			this.fromEnvEffect = null;
			this.fromEnvEffectIntensity = 1f;
			BlockMaster.masterRBConstraints = RigidbodyConstraints.None;
			BlockMaster.masterRBConstraintsUpdated = false;
		}

		// Token: 0x06000CF1 RID: 3313 RVA: 0x0005A4AB File Offset: 0x000588AB
		public override void Stop(bool resetBlock = true)
		{
			BlockMaster.masterRBConstraints = RigidbodyConstraints.None;
			base.Stop(resetBlock);
			this.ClearTerrainBlocks();
			this.earthquakeBlocks.Clear();
		}

		// Token: 0x06000CF2 RID: 3314 RVA: 0x0005A4CC File Offset: 0x000588CC
		public static GAF ReplaceGaf(GAF gaf)
		{
			Predicate predicate = gaf.Predicate;
			object[] args = gaf.Args;
			if (predicate == Block.predicatePaintTo)
			{
				return new GAF(BlockMaster.predicatePaintSkyTo, new object[]
				{
					args[0],
					0,
					5f
				});
			}
			if (predicate == Block.predicateTextureTo)
			{
				string texture = (string)args[0];
				if (BlockSky.IsSkyTexture(texture))
				{
					string text = BlockSky.SkyTextureToEnvEffect(texture);
					return new GAF(BlockMaster.predicateSetEnvEffect, new object[]
					{
						text,
						1f,
						10f
					});
				}
			}
			return gaf;
		}

		// Token: 0x06000CF3 RID: 3315 RVA: 0x0005A575 File Offset: 0x00058975
		private void ClearTerrainBlocks()
		{
			if (this.terrainBlocks != null)
			{
				this.terrainBlocks.Clear();
				this.terrainBlocks = null;
			}
		}

		// Token: 0x06000CF4 RID: 3316 RVA: 0x0005A594 File Offset: 0x00058994
		public new static void Register()
		{
			BlockMaster.predicatePaintSkyTo = PredicateRegistry.Add<BlockMaster>("Master.PaintSkyTo", (Block b) => new PredicateSensorDelegate(b.IsSkyPaintedTo), (Block b) => new PredicateActionDelegate(b.PaintSkyToAction), new Type[]
			{
				typeof(string),
				typeof(int),
				typeof(float)
			}, new string[]
			{
				"Paint",
				"Mesh index",
				"Duration"
			}, null);
			PredicateRegistry.Add<BlockMaster>("Master.RotateSkyTo", (Block b) => new PredicateSensorDelegate(((BlockMaster)b).IsSkyRotatedTo), (Block b) => new PredicateActionDelegate(((BlockMaster)b).SkyRotateToAction), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Angle"
			}, null);
			PredicateRegistry.Add<BlockMaster>("Master.SetSunIntensity", (Block b) => new PredicateSensorDelegate(((BlockMaster)b).IsSunIntensity), (Block b) => new PredicateActionDelegate(((BlockMaster)b).SetSunIntensity), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Intensity"
			}, null);
			BlockMaster.predicateSkyBoxTo = PredicateRegistry.Add<BlockMaster>("Master.SkyBoxTo", (Block b) => new PredicateSensorDelegate(((BlockMaster)b).IsSkyBox), (Block b) => new PredicateActionDelegate(((BlockMaster)b).SetSkyBox), new Type[]
			{
				typeof(int)
			}, null, null);
			BlockMaster.predicateSetEnvEffect = PredicateRegistry.Add<BlockMaster>("Master.SetEnvEffect", (Block b) => new PredicateSensorDelegate(((BlockMaster)b).IsEnvEffect), (Block b) => new PredicateActionDelegate(((BlockMaster)b).SetEnvEffect), new Type[]
			{
				typeof(string),
				typeof(float),
				typeof(float)
			}, new string[]
			{
				"Effect name",
				"Intensity",
				"Fade Time"
			}, null);
			PredicateRegistry.Add<BlockMaster>("Master.SetGravity", null, (Block b) => new PredicateActionDelegate(((BlockMaster)b).SetGravity), new Type[]
			{
				typeof(Vector3)
			}, new string[]
			{
				"Gravity field"
			}, null);
			PredicateRegistry.Add<BlockMaster>("Master.SetWorldGravity", null, (Block b) => new PredicateActionDelegate(((BlockMaster)b).SetWorldGravity), new Type[]
			{
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<BlockMaster>("Master.SetDragMult", null, (Block b) => new PredicateActionDelegate(((BlockMaster)b).SetDragMultiplier), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Multiplier"
			}, null);
			PredicateRegistry.Add<BlockMaster>("Master.SetAngularDragMult", null, (Block b) => new PredicateActionDelegate(((BlockMaster)b).SetAngularDragMultiplier), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Multiplier"
			}, null);
			PredicateRegistry.Add<BlockMaster>("Master.SetAtmosphere", null, (Block b) => new PredicateActionDelegate(((BlockMaster)b).SetAtmosphere), new Type[]
			{
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<BlockMaster>("Master.ShowUI", null, (Block b) => new PredicateActionDelegate(((BlockAbstractUI)b).ShowUI), new Type[]
			{
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<BlockMaster>("Master.ShowPhysical", null, (Block b) => new PredicateActionDelegate(((BlockAbstractUI)b).ShowPhysical), new Type[]
			{
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<BlockMaster>("Master.SetBackgroundMusic", null, (Block b) => new PredicateActionDelegate(b.SetBackgroundMusic), new Type[]
			{
				typeof(string)
			}, new string[]
			{
				"Music name"
			}, null);
			PredicateRegistry.Add<BlockMaster>("Master.PaintTerrainTo", null, (Block b) => new PredicateActionDelegate(((BlockMaster)b).PaintTerrainTo), new Type[]
			{
				typeof(string),
				typeof(string)
			}, new string[]
			{
				"Terrain block",
				"Paint"
			}, null);
			PredicateRegistry.Add<BlockMaster>("Master.TextureTerrainTo", null, (Block b) => new PredicateActionDelegate(((BlockMaster)b).TextureTerrainTo), new Type[]
			{
				typeof(string),
				typeof(string)
			}, new string[]
			{
				"Terrain block",
				"Texture"
			}, null);
			PredicateRegistry.Add<BlockMaster>("Master.SetWaterBuoyancyMultiplier", null, (Block b) => new PredicateActionDelegate(((BlockMaster)b).SetWaterBuoyancyMultiplier), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Multiplier"
			}, null);
			PredicateRegistry.Add<BlockMaster>("Master.PullLock", null, (Block b) => new PredicateActionDelegate(((BlockMaster)b).PullLockGlobal), null, null, null);
			PredicateRegistry.Add<BlockMaster>("Master.DisableAutoFollow", null, (Block b) => new PredicateActionDelegate(((BlockMaster)b).DisableAutoFollow), null, null, null);
			PredicateRegistry.Add<BlockMaster>("Master.Earthquake", null, (Block b) => new PredicateActionDelegate(((BlockMaster)b).Earthquake), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMaster>("Master.CameraShake", null, (Block b) => new PredicateActionDelegate(((BlockMaster)b).CameraShake), new Type[]
			{
				typeof(float)
			}, null, null);
			BlockMaster.predicateIncreaseWaterLevel = PredicateRegistry.Add<BlockMaster>("Master.IncreaseWaterLevel", null, (Block b) => new PredicateActionDelegate(((BlockMaster)b).IncreaseWaterLevel), new Type[]
			{
				typeof(float)
			}, null, null);
			BlockMaster.predicateStepIncreaseWaterLevel = PredicateRegistry.Add<BlockMaster>("Master.StepIncreaseWaterLevel", null, (Block b) => new PredicateActionDelegate(((BlockMaster)b).StepIncreaseWaterLevel), new Type[]
			{
				typeof(float),
				typeof(float)
			}, new string[]
			{
				"Units",
				"Duration"
			}, null);
			BlockMaster.predicateSetLiquidProperties = PredicateRegistry.Add<BlockMaster>("Master.SetLiquidProperties", null, (Block b) => new PredicateActionDelegate(((BlockMaster)b).SetLiquidProperties), new Type[]
			{
				typeof(string)
			}, null, null);
			BlockMaster.predicateSetMaxPositiveWaterLevelOffset = PredicateRegistry.Add<BlockMaster>("Master.SetMaxPositiveWaterLevelOffset", (Block b) => new PredicateSensorDelegate(((BlockMaster)b).AtMaxPositiveWaterLevelOffset), (Block b) => new PredicateActionDelegate(((BlockMaster)b).SetMaxPositiveWaterLevelOffset), new Type[]
			{
				typeof(float)
			}, null, null);
			BlockMaster.predicateSetMaxNegativeWaterLevelOffset = PredicateRegistry.Add<BlockMaster>("Master.SetMaxNegativeWaterLevelOffset", (Block b) => new PredicateSensorDelegate(((BlockMaster)b).AtMaxNegativeWaterLevelOffset), (Block b) => new PredicateActionDelegate(((BlockMaster)b).SetMaxNegativeWaterLevelOffset), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMaster>("Master.SetEnvEffectAngle", null, (Block b) => new PredicateActionDelegate(((BlockMaster)b).SetEnvEffectAngle), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMaster>("Master.FogColorTo", (Block b) => new PredicateSensorDelegate(((BlockMaster)b).IsFogColor), (Block b) => new PredicateActionDelegate(((BlockMaster)b).SetFogColor), new Type[]
			{
				typeof(int),
				typeof(string)
			}, null, null);
			PredicateRegistry.Add<BlockMaster>("Master.FogDensityTo", (Block b) => new PredicateSensorDelegate(((BlockMaster)b).IsFogDensity), (Block b) => new PredicateActionDelegate(((BlockMaster)b).SetFogDensity), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMaster>("Master.FogStartTo", (Block b) => new PredicateSensorDelegate(((BlockMaster)b).IsFogStart), (Block b) => new PredicateActionDelegate(((BlockMaster)b).SetFogStart), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMaster>("Master.TiltGravity", null, (Block b) => new PredicateActionDelegate(((BlockMaster)b).TiltGravity), new Type[]
			{
				typeof(float),
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<BlockMaster>("Master.ConstrainTranslation", (Block b) => new PredicateSensorDelegate(((BlockMaster)b).IsConstrainTranslationMaster), (Block b) => new PredicateActionDelegate(((BlockMaster)b).ConstrainTranslationMaster), new Type[]
			{
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<BlockMaster>("Master.FreeTranslation", (Block b) => new PredicateSensorDelegate(((BlockMaster)b).IsFreeTranslationMaster), (Block b) => new PredicateActionDelegate(((BlockMaster)b).FreeTranslationMaster), new Type[]
			{
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<BlockMaster>("Master.ConstrainRotation", (Block b) => new PredicateSensorDelegate(((BlockMaster)b).IsConstrainRotationMaster), (Block b) => new PredicateActionDelegate(((BlockMaster)b).ConstrainRotationMaster), new Type[]
			{
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<BlockMaster>("Master.FreeRotation", (Block b) => new PredicateSensorDelegate(((BlockMaster)b).IsFreeRotationMaster), (Block b) => new PredicateActionDelegate(((BlockMaster)b).FreeRotationMaster), new Type[]
			{
				typeof(int)
			}, null, null);
			BlockMaster.SetupGravityAndAtmosphereDefinitions();
		}

		// Token: 0x06000CF5 RID: 3317 RVA: 0x0005B0B0 File Offset: 0x000594B0
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (BlockMaster.masterRBConstraintsUpdated)
			{
				for (int i = 0; i < Blocksworld.chunks.Count; i++)
				{
					Blocksworld.chunks[i].UpdateRBConstraints();
				}
				BlockMaster.masterRBConstraintsUpdated = false;
			}
			float magnitude = Physics.gravity.magnitude;
			if (this.tiltGravity && TiltManager.Instance.IsMonitoring())
			{
				Vector3 vector = (!this.tiltGravityFlat) ? TiltManager.Instance.GetRelativeGravityVector() : TiltManager.Instance.GetGravityVector();
				bool flag = (Blocksworld.cameraForward - Vector3.down).sqrMagnitude < 0.1f;
				Vector3 vec = (!flag) ? Blocksworld.cameraForward : Blocksworld.cameraUp;
				Vector3 a = Util.ProjectOntoPlane(vec, Vector3.up);
				Vector3 a2 = Util.ProjectOntoPlane(Blocksworld.cameraRight, Vector3.up);
				a.Normalize();
				a2.Normalize();
				Vector3 normalized = (vector.x * a2 + vector.y * a + vector.z * Vector3.up).normalized;
				Physics.gravity = normalized * magnitude;
			}
			else
			{
				Physics.gravity = magnitude * Vector3.down;
			}
			this.tiltGravity = false;
			if (Sound.sfxEnabled && (this.earthQuakeStrength > 0f || this.prolongedEarthQuakeStrength > 0f))
			{
				float num = Mathf.Max(this.earthQuakeStrength, this.prolongedEarthQuakeStrength);
				this.PlaySound("Earthquake Loop", "Block", "Loop", Mathf.Min(num * 0.5f, 1f), 1f, false, 0f);
			}
			float fixedTime = Time.fixedTime;
			if (this.earthQuakeStrength > 0f || this.prolongedEarthQuakeStrength > 0f)
			{
				foreach (Block key in CollisionManager.blocksOnGround)
				{
					this.earthquakeBlocks[key] = 0;
				}
				CollisionManager.blocksOnGround.Clear();
				float d = Mathf.Max(this.earthQuakeStrength, this.prolongedEarthQuakeStrength);
				foreach (Block block in new List<Block>(this.earthquakeBlocks.Keys))
				{
					int num2 = this.earthquakeBlocks[block];
					if (num2 < 4)
					{
						num2++;
						this.earthquakeBlocks[block] = num2;
						GameObject go = block.go;
						if (go != null)
						{
							Chunk chunk = block.chunk;
							Rigidbody rb = chunk.rb;
							if (rb != null && !rb.isKinematic)
							{
								Vector3 insideUnitSphere = UnityEngine.Random.insideUnitSphere;
								insideUnitSphere.y = Mathf.Abs(insideUnitSphere.y);
								Vector3 force = insideUnitSphere * d * rb.mass;
								Vector3 position = go.transform.position;
								rb.AddForceAtPosition(force, position, ForceMode.Impulse);
							}
						}
					}
					else
					{
						this.earthquakeBlocks.Remove(block);
					}
				}
				if (this.earthQuakeStrength > 0f)
				{
					this.prolongedEarthQuakeStrength = this.earthQuakeStrength;
				}
				else if (fixedTime > this.earthQuakeStartTime + 1f)
				{
					this.prolongedEarthQuakeStrength = 0f;
				}
				this.earthQuakeStrength = 0f;
			}
			if (this.cameraShakeStrength > 0f || this.prolongedCameraShakeStrength > 0f)
			{
				float strength = Mathf.Max(this.cameraShakeStrength, this.prolongedCameraShakeStrength);
				BlockMaster.CameraShake(strength);
				if (this.cameraShakeStrength > 0f)
				{
					this.prolongedCameraShakeStrength = this.cameraShakeStrength;
				}
				else if (fixedTime > this.cameraShakeStartTime + 1f)
				{
					this.prolongedCameraShakeStrength = 0f;
				}
				this.cameraShakeStrength = 0f;
			}
		}

		// Token: 0x06000CF6 RID: 3318 RVA: 0x0005B510 File Offset: 0x00059910
		public static void CameraShake(float strength)
		{
			float d = strength * 2f * (Mathf.PerlinNoise(Time.time * 20f, 0f) - 0.5f);
			float angle = 360f * (Mathf.PerlinNoise(Time.time * 2f + 232.2f, 0f) - 0.5f);
			Vector3 offset = d * (Quaternion.AngleAxis(angle, Blocksworld.cameraForward) * Blocksworld.cameraUp);
			Blocksworld.blocksworldCamera.AddImmediateOffset(offset);
		}

		// Token: 0x06000CF7 RID: 3319 RVA: 0x0005B590 File Offset: 0x00059990
		public TileResultCode TiltGravity(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.tiltGravity = true;
			this.tiltGravityStrength = 0.25f * Util.GetFloatArg(args, 0, 4f);
			this.tiltGravityFlat = (Util.GetIntArg(args, 1, 0) > 0);
			Blocksworld.UI.Controls.UpdateTiltPrompt();
			return TileResultCode.True;
		}

		// Token: 0x06000CF8 RID: 3320 RVA: 0x0005B5E0 File Offset: 0x000599E0
		public TileResultCode SetEnvEffectAngle(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float floatArg = Util.GetFloatArg(args, 0, 0f);
			Blocksworld.weather.SetEffectAngle(floatArg);
			return TileResultCode.True;
		}

		// Token: 0x06000CF9 RID: 3321 RVA: 0x0005B608 File Offset: 0x00059A08
		public TileResultCode SetLiquidProperties(ScriptRowExecutionInfo eInfo, object[] args)
		{
			BlockWater worldOceanBlock = Blocksworld.worldOceanBlock;
			if (worldOceanBlock != null)
			{
				worldOceanBlock.SetLiquidProperties(Util.GetStringArg(args, 0, "Water"));
			}
			return TileResultCode.True;
		}

		// Token: 0x06000CFA RID: 3322 RVA: 0x0005B634 File Offset: 0x00059A34
		public TileResultCode AtMaxPositiveWaterLevelOffset(ScriptRowExecutionInfo eInfo, object[] args)
		{
			BlockWater worldOceanBlock = Blocksworld.worldOceanBlock;
			if (worldOceanBlock != null)
			{
				return (!worldOceanBlock.AtMaxPositiveWaterLevelOffset()) ? TileResultCode.False : TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x06000CFB RID: 3323 RVA: 0x0005B664 File Offset: 0x00059A64
		public TileResultCode AtMaxNegativeWaterLevelOffset(ScriptRowExecutionInfo eInfo, object[] args)
		{
			BlockWater worldOceanBlock = Blocksworld.worldOceanBlock;
			if (worldOceanBlock != null)
			{
				return (!worldOceanBlock.AtMaxNegativeWaterLevelOffset()) ? TileResultCode.False : TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x06000CFC RID: 3324 RVA: 0x0005B694 File Offset: 0x00059A94
		public TileResultCode SetMaxPositiveWaterLevelOffset(ScriptRowExecutionInfo eInfo, object[] args)
		{
			BlockWater worldOceanBlock = Blocksworld.worldOceanBlock;
			if (worldOceanBlock != null)
			{
				worldOceanBlock.SetMaxPositiveWaterLevelOffset(eInfo.floatArg * Util.GetFloatArg(args, 0, 10f));
			}
			return TileResultCode.True;
		}

		// Token: 0x06000CFD RID: 3325 RVA: 0x0005B6C8 File Offset: 0x00059AC8
		public TileResultCode SetMaxNegativeWaterLevelOffset(ScriptRowExecutionInfo eInfo, object[] args)
		{
			BlockWater worldOceanBlock = Blocksworld.worldOceanBlock;
			if (worldOceanBlock != null)
			{
				worldOceanBlock.SetMaxNegativeWaterLevelOffset(eInfo.floatArg * Util.GetFloatArg(args, 0, 10f));
			}
			return TileResultCode.True;
		}

		// Token: 0x06000CFE RID: 3326 RVA: 0x0005B6FC File Offset: 0x00059AFC
		public TileResultCode IncreaseWaterLevel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			BlockWater worldOceanBlock = Blocksworld.worldOceanBlock;
			if (worldOceanBlock != null)
			{
				float num = eInfo.floatArg * Util.GetFloatArg(args, 0, 0f);
				worldOceanBlock.IncreaseWaterLevel(num * Blocksworld.fixedDeltaTime);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000CFF RID: 3327 RVA: 0x0005B738 File Offset: 0x00059B38
		public TileResultCode StepIncreaseWaterLevel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			BlockWater worldOceanBlock = Blocksworld.worldOceanBlock;
			if (worldOceanBlock != null)
			{
				float floatArg = Util.GetFloatArg(args, 0, 1f);
				float num = Util.GetFloatArg(args, 1, 1f);
				if (num < 0.01f)
				{
					num = 1f;
				}
				float num2 = eInfo.floatArg * floatArg / num;
				worldOceanBlock.IncreaseWaterLevel(num2 * Blocksworld.fixedDeltaTime);
				return (eInfo.timer < num) ? TileResultCode.Delayed : TileResultCode.True;
			}
			return TileResultCode.True;
		}

		// Token: 0x06000D00 RID: 3328 RVA: 0x0005B7AC File Offset: 0x00059BAC
		public TileResultCode Earthquake(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float num = eInfo.floatArg * Util.GetFloatArg(args, 0, 1f);
			if (this.prolongedEarthQuakeStrength == 0f)
			{
				this.earthQuakeStartTime = Time.fixedTime;
			}
			if (this.prolongedCameraShakeStrength == 0f)
			{
				this.cameraShakeStartTime = Time.fixedTime;
			}
			this.earthQuakeStrength += num;
			this.cameraShakeStrength += 0.5f * num;
			return TileResultCode.True;
		}

		// Token: 0x06000D01 RID: 3329 RVA: 0x0005B828 File Offset: 0x00059C28
		public TileResultCode CameraShake(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float num = eInfo.floatArg * Util.GetFloatArg(args, 0, 1f);
			if (this.prolongedCameraShakeStrength == 0f)
			{
				this.cameraShakeStartTime = Time.fixedTime;
			}
			this.cameraShakeStrength += num;
			return TileResultCode.True;
		}

		// Token: 0x06000D02 RID: 3330 RVA: 0x0005B873 File Offset: 0x00059C73
		public TileResultCode PullLockGlobal(ScriptRowExecutionInfo eInfo, object[] args)
		{
			Blocksworld.dynamicLockPull = true;
			Blocksworld.AddResetStateCommand(new UnlockDynamicLockPullCommand());
			return TileResultCode.True;
		}

		// Token: 0x06000D03 RID: 3331 RVA: 0x0005B886 File Offset: 0x00059C86
		public TileResultCode DisableAutoFollow(ScriptRowExecutionInfo eInfo, object[] args)
		{
			Blocksworld.blocksworldCamera.EnableAutoFollow(false);
			Blocksworld.AddResetStateCommand(new DelegateCommand(delegate(DelegateCommand c)
			{
				Blocksworld.blocksworldCamera.EnableAutoFollow(true);
			}));
			return TileResultCode.True;
		}

		// Token: 0x06000D04 RID: 3332 RVA: 0x0005B8BB File Offset: 0x00059CBB
		public TileResultCode SetWaterBuoyancyMultiplier(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (Blocksworld.worldOceanBlock != null)
			{
				Blocksworld.worldOceanBlock.buoyancyMultiplier = eInfo.floatArg * Util.GetFloatArg(args, 0, 1f);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000D05 RID: 3333 RVA: 0x0005B8E8 File Offset: 0x00059CE8
		private Block GetTerrainBlock(string name)
		{
			if (this.terrainBlocks == null)
			{
				this.terrainBlocks = new Dictionary<string, Block>();
				foreach (Block block in BWSceneManager.AllBlocks())
				{
					if (block.isTerrain)
					{
						this.terrainBlocks[block.BlockType()] = block;
					}
				}
			}
			Block result;
			if (this.terrainBlocks.TryGetValue(name, out result))
			{
				return result;
			}
			BWLog.Info("Could not find terrain block '" + name + "'");
			BWLog.Info("Choose from one of the following:");
			foreach (string s in this.terrainBlocks.Keys)
			{
				BWLog.Info(s);
			}
			this.terrainBlocks[name] = null;
			return null;
		}

		// Token: 0x06000D06 RID: 3334 RVA: 0x0005BA04 File Offset: 0x00059E04
		public TileResultCode PaintTerrainTo(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string name = (string)args[0];
			string paint = (string)args[1];
			Block terrainBlock = this.GetTerrainBlock(name);
			if (terrainBlock != null)
			{
				return terrainBlock.PaintTo(paint, false, 0);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000D07 RID: 3335 RVA: 0x0005BA3C File Offset: 0x00059E3C
		public TileResultCode TextureTerrainTo(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string name = (string)args[0];
			string texture = (string)args[1];
			Block terrainBlock = this.GetTerrainBlock(name);
			if (terrainBlock != null)
			{
				return terrainBlock.TextureTo(texture, Vector3.up, false, 0, false);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000D08 RID: 3336 RVA: 0x0005BA7C File Offset: 0x00059E7C
		public TileResultCode IsEnvEffect(ScriptRowExecutionInfo eInfo, object[] args)
		{
			BlockSky worldSky = Blocksworld.worldSky;
			if (worldSky != null)
			{
				return (!(worldSky.GetPaint(0) == (string)args[0])) ? TileResultCode.False : TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x06000D09 RID: 3337 RVA: 0x0005BAB8 File Offset: 0x00059EB8
		public TileResultCode SetEnvEffect(ScriptRowExecutionInfo eInfo, object[] args)
		{
			BlockSky worldSky = Blocksworld.worldSky;
			if (worldSky == null)
			{
				return TileResultCode.True;
			}
			string stringArg = Util.GetStringArg(args, 0, "Clear");
			float num = eInfo.floatArg * Util.GetFloatArg(args, 1, 1f);
			float num2 = Mathf.Max(0.1f, Util.GetFloatArg(args, 2, 1f));
			if (eInfo.timer == 0f)
			{
				if (this.toEnvEffect != null)
				{
					return TileResultCode.True;
				}
				this.fromEnvEffect = BlockSky.GetEnvEffectName(Blocksworld.weather);
				this.toEnvEffect = stringArg;
				if (this.fromEnvEffect == "Clear" && this.toEnvEffect != "Clear")
				{
					worldSky.UpdateWeather(BlockSky.GetWeatherEffectByName(stringArg), false);
				}
				this.fromEnvEffectIntensity = Blocksworld.weather.IntensityMultiplier;
			}
			if (this.fromEnvEffect == this.toEnvEffect)
			{
				if (eInfo.timer >= num2)
				{
					Blocksworld.weather.IntensityMultiplier = num;
					this.toEnvEffect = null;
					this.fromEnvEffect = null;
					return TileResultCode.True;
				}
				float num3 = eInfo.timer / num2;
				Blocksworld.weather.IntensityMultiplier = num3 * num + (1f - num3) * this.fromEnvEffectIntensity;
				return TileResultCode.Delayed;
			}
			else
			{
				if (eInfo.timer >= num2)
				{
					if (this.toEnvEffect == "Clear")
					{
						worldSky.UpdateWeather(BlockSky.GetWeatherEffectByName(this.toEnvEffect), false);
					}
					Blocksworld.weather.IntensityMultiplier = num;
					this.toEnvEffect = null;
					this.fromEnvEffect = null;
					return TileResultCode.True;
				}
				if (this.fromEnvEffect == "Clear")
				{
					Blocksworld.weather.IntensityMultiplier = num * (eInfo.timer / num2);
				}
				else if (this.toEnvEffect == "Clear")
				{
					Blocksworld.weather.IntensityMultiplier = this.fromEnvEffectIntensity * (1f - eInfo.timer / num2);
				}
				else if (eInfo.timer < num2 * 0.5f)
				{
					Blocksworld.weather.IntensityMultiplier = this.fromEnvEffectIntensity * (1f - eInfo.timer / (num2 * 0.5f));
				}
				else
				{
					if (Blocksworld.weather != BlockSky.GetWeatherEffectByName(stringArg))
					{
						worldSky.UpdateWeather(BlockSky.GetWeatherEffectByName(stringArg), false);
					}
					Blocksworld.weather.IntensityMultiplier = num * (eInfo.timer - num2 * 0.5f) / (num2 * 0.5f);
				}
				return TileResultCode.Delayed;
			}
		}

		// Token: 0x06000D0A RID: 3338 RVA: 0x0005BD20 File Offset: 0x0005A120
		public TileResultCode SetGravity(ScriptRowExecutionInfo eInfo, object[] args)
		{
			Vector3 field = eInfo.floatArg * 9.82f * Util.GetVector3Arg(args, 0, -Vector3.up);
			this.ApplyGravity(field);
			return TileResultCode.True;
		}

		// Token: 0x06000D0B RID: 3339 RVA: 0x0005BD58 File Offset: 0x0005A158
		public TileResultCode SetWorldGravity(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 0, 0);
			if (intArg < BlockMaster.gravDefs.definitions.Length)
			{
				this.ApplyGravity(BlockMaster.gravDefs.definitions[intArg].amount);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000D0C RID: 3340 RVA: 0x0005BD98 File Offset: 0x0005A198
		public TileResultCode SetAtmosphere(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 0, 0);
			if (intArg < BlockMaster.atmosDefs.definitions.Length)
			{
				this.ApplyDrag(BlockMaster.atmosDefs.definitions[intArg].drag);
				this.ApplyAngularDrag(BlockMaster.atmosDefs.definitions[intArg].angularDrag);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000D0D RID: 3341 RVA: 0x0005BDEF File Offset: 0x0005A1EF
		public static void SetupGravityAndAtmosphereDefinitions()
		{
			BlockMaster.gravDefs = Resources.Load<GravityDefinitions>("GravityDefinitions");
			BlockMaster.atmosDefs = Resources.Load<AtmosphereDefinitions>("AtmosphereDefinitions");
		}

		// Token: 0x06000D0E RID: 3342 RVA: 0x0005BE10 File Offset: 0x0005A210
		private void ApplyGravity(Vector3 field)
		{
			Physics.gravity = field;
			if (!this.informedBlocksAboutVaryingGravity)
			{
				foreach (Block block in BWSceneManager.AllBlocks())
				{
					block.SetVaryingGravity(true);
				}
				this.informedBlocksAboutVaryingGravity = true;
			}
		}

		// Token: 0x06000D0F RID: 3343 RVA: 0x0005BE84 File Offset: 0x0005A284
		public TileResultCode SetDragMultiplier(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float dragMult = eInfo.floatArg * Util.GetFloatArg(args, 0, 1f);
			this.ApplyDrag(dragMult);
			return TileResultCode.True;
		}

		// Token: 0x06000D10 RID: 3344 RVA: 0x0005BEAD File Offset: 0x0005A2AD
		private void ApplyDrag(float dragMult)
		{
			Blocksworld.dragMultiplier = dragMult;
			Blocksworld.UpdateDrag();
		}

		// Token: 0x06000D11 RID: 3345 RVA: 0x0005BEBC File Offset: 0x0005A2BC
		public TileResultCode SetAngularDragMultiplier(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float angDragMult = eInfo.floatArg * Util.GetFloatArg(args, 0, 1f);
			this.ApplyAngularDrag(angDragMult);
			return TileResultCode.True;
		}

		// Token: 0x06000D12 RID: 3346 RVA: 0x0005BEE5 File Offset: 0x0005A2E5
		private void ApplyAngularDrag(float angDragMult)
		{
			Blocksworld.angularDragMultiplier = angDragMult;
			Blocksworld.UpdateAngularDrag();
		}

		// Token: 0x06000D13 RID: 3347 RVA: 0x0005BEF4 File Offset: 0x0005A2F4
		public TileResultCode IsFogColor(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArg = Util.GetStringArg(args, 0, "White");
			return base.boolToTileResult(this._fogPaint == stringArg);
		}

		// Token: 0x06000D14 RID: 3348 RVA: 0x0005BF20 File Offset: 0x0005A320
		public TileResultCode SetFogColor(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this._fogPaint = Util.GetStringArg(args, 1, "White");
			WorldEnvironmentManager.OverrideFogPaintTemporarily(this._fogPaint);
			Blocksworld.worldSky.SetFogColor(this._fogPaint);
			return TileResultCode.True;
		}

		// Token: 0x06000D15 RID: 3349 RVA: 0x0005BF50 File Offset: 0x0005A350
		public TileResultCode IsFogDensity(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float floatArg = Util.GetFloatArg(args, 0, 10f);
			return base.boolToTileResult(Util.FloatsClose(floatArg, this._fogDensity));
		}

		// Token: 0x06000D16 RID: 3350 RVA: 0x0005BF7C File Offset: 0x0005A37C
		public TileResultCode SetFogDensity(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this._fogDensity = Util.GetFloatArg(args, 0, 10f);
			WorldEnvironmentManager.OverrideFogDensityTemporarily(this._fogDensity);
			this.UpdateFog();
			return TileResultCode.True;
		}

		// Token: 0x06000D17 RID: 3351 RVA: 0x0005BFA4 File Offset: 0x0005A3A4
		public TileResultCode IsFogStart(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float f = Mathf.Max(0.1f, Util.GetFloatArg(args, 0, 100f));
			return base.boolToTileResult(Util.FloatsClose(f, this._fogStart));
		}

		// Token: 0x06000D18 RID: 3352 RVA: 0x0005BFDA File Offset: 0x0005A3DA
		public TileResultCode SetFogStart(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this._fogStart = Mathf.Max(0.1f, Util.GetFloatArg(args, 0, 100f));
			WorldEnvironmentManager.OverrideFogStartTemporarily(this._fogStart);
			this.UpdateFog();
			return TileResultCode.True;
		}

		// Token: 0x06000D19 RID: 3353 RVA: 0x0005C00C File Offset: 0x0005A40C
		private void UpdateFog()
		{
			float end = WorldEnvironmentManager.ComputeFogEnd(this._fogStart, this._fogDensity);
			Blocksworld.bw.SetFog(this._fogStart, end);
		}

		// Token: 0x06000D1A RID: 3354 RVA: 0x0005C03C File Offset: 0x0005A43C
		internal static RigidbodyConstraints GetGlobalRigidbodyConstraints()
		{
			return BlockMaster.masterRBConstraints;
		}

		// Token: 0x06000D1B RID: 3355 RVA: 0x0005C044 File Offset: 0x0005A444
		public TileResultCode IsConstrainTranslationMaster(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int rbconstraintsArgAsInt = base.GetRBConstraintsArgAsInt(args, 0, true);
			return base.tileResultFromRBConstraintInclusion((int)BlockMaster.masterRBConstraints, rbconstraintsArgAsInt);
		}

		// Token: 0x06000D1C RID: 3356 RVA: 0x0005C068 File Offset: 0x0005A468
		public TileResultCode ConstrainTranslationMaster(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int num = base.GetRBConstraintsArgAsInt(args, 0, true);
			num = (int)(BlockMaster.masterRBConstraints | (RigidbodyConstraints)num);
			BlockMaster.masterRBConstraintsUpdated |= (num != (int)BlockMaster.masterRBConstraints);
			BlockMaster.masterRBConstraints = (RigidbodyConstraints)num;
			return TileResultCode.True;
		}

		// Token: 0x06000D1D RID: 3357 RVA: 0x0005C0A4 File Offset: 0x0005A4A4
		public TileResultCode IsFreeTranslationMaster(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int rbconstraintsArgAsInt = base.GetRBConstraintsArgAsInt(args, 0, true);
			return base.tileResultFromRBConstraintExclusion((int)BlockMaster.masterRBConstraints, rbconstraintsArgAsInt);
		}

		// Token: 0x06000D1E RID: 3358 RVA: 0x0005C0C8 File Offset: 0x0005A4C8
		public TileResultCode FreeTranslationMaster(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int num = base.GetRBConstraintsArgAsInt(args, 0, true);
			num = (int)(BlockMaster.masterRBConstraints & (RigidbodyConstraints)(~(RigidbodyConstraints)num));
			BlockMaster.masterRBConstraintsUpdated |= (num != (int)BlockMaster.masterRBConstraints);
			BlockMaster.masterRBConstraints = (RigidbodyConstraints)num;
			return TileResultCode.True;
		}

		// Token: 0x06000D1F RID: 3359 RVA: 0x0005C108 File Offset: 0x0005A508
		public TileResultCode IsConstrainRotationMaster(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int rbconstraintsArgAsInt = base.GetRBConstraintsArgAsInt(args, 0, false);
			return base.tileResultFromRBConstraintInclusion((int)BlockMaster.masterRBConstraints, rbconstraintsArgAsInt);
		}

		// Token: 0x06000D20 RID: 3360 RVA: 0x0005C12C File Offset: 0x0005A52C
		public TileResultCode ConstrainRotationMaster(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int num = base.GetRBConstraintsArgAsInt(args, 0, false);
			num = (int)(BlockMaster.masterRBConstraints | (RigidbodyConstraints)num);
			BlockMaster.masterRBConstraintsUpdated |= (num != (int)BlockMaster.masterRBConstraints);
			BlockMaster.masterRBConstraints = (RigidbodyConstraints)num;
			return TileResultCode.True;
		}

		// Token: 0x06000D21 RID: 3361 RVA: 0x0005C168 File Offset: 0x0005A568
		public TileResultCode IsFreeRotationMaster(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int rbconstraintsArgAsInt = base.GetRBConstraintsArgAsInt(args, 0, false);
			return base.tileResultFromRBConstraintExclusion((int)BlockMaster.masterRBConstraints, rbconstraintsArgAsInt);
		}

		// Token: 0x06000D22 RID: 3362 RVA: 0x0005C18C File Offset: 0x0005A58C
		public TileResultCode FreeRotationMaster(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int num = base.GetRBConstraintsArgAsInt(args, 0, false);
			num = (int)(BlockMaster.masterRBConstraints & (RigidbodyConstraints)(~(RigidbodyConstraints)num));
			BlockMaster.masterRBConstraintsUpdated |= (num != (int)BlockMaster.masterRBConstraints);
			BlockMaster.masterRBConstraints = (RigidbodyConstraints)num;
			return TileResultCode.True;
		}

		// Token: 0x06000D23 RID: 3363 RVA: 0x0005C1CC File Offset: 0x0005A5CC
		public TileResultCode IsSkyRotatedTo(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float floatArg = Util.GetFloatArg(args, 0, 0f);
			return base.boolToTileResult(Util.FloatsClose(floatArg, Blocksworld.worldSky.GetSkyBoxRotation()));
		}

		// Token: 0x06000D24 RID: 3364 RVA: 0x0005C1FC File Offset: 0x0005A5FC
		public TileResultCode SkyRotateToAction(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float angle = Util.GetFloatArg(args, 0, 0f) * eInfo.floatArg;
			Blocksworld.worldSky.SetSkyBoxRotation(angle, false);
			return TileResultCode.True;
		}

		// Token: 0x06000D25 RID: 3365 RVA: 0x0005C22C File Offset: 0x0005A62C
		public TileResultCode IsSunIntensity(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float f = Util.GetFloatArg(args, 0, 1f) * 0.01f;
			return base.boolToTileResult(Util.FloatsClose(f, Blocksworld.worldSky.GetSunIntensity()));
		}

		// Token: 0x06000D26 RID: 3366 RVA: 0x0005C264 File Offset: 0x0005A664
		public TileResultCode SetSunIntensity(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float sunIntensity = Util.GetFloatArg(args, 0, 1f) * eInfo.floatArg * 0.01f;
			Blocksworld.worldSky.SetSunIntensity(sunIntensity);
			return TileResultCode.True;
		}

		// Token: 0x06000D27 RID: 3367 RVA: 0x0005C297 File Offset: 0x0005A697
		public TileResultCode IsSkyBox(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return Blocksworld.worldSky.IsSkyBox(eInfo, args);
		}

		// Token: 0x06000D28 RID: 3368 RVA: 0x0005C2A5 File Offset: 0x0005A6A5
		public TileResultCode SetSkyBox(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return Blocksworld.worldSky.SetSkyBox(eInfo, args);
		}

		// Token: 0x04000A38 RID: 2616
		private bool informedBlocksAboutVaryingGravity;

		// Token: 0x04000A39 RID: 2617
		private Dictionary<string, Block> terrainBlocks;

		// Token: 0x04000A3A RID: 2618
		public static Predicate predicatePaintSkyTo;

		// Token: 0x04000A3B RID: 2619
		public static Predicate predicateSkyBoxTo;

		// Token: 0x04000A3C RID: 2620
		public static Predicate predicateSetEnvEffect;

		// Token: 0x04000A3D RID: 2621
		public static Predicate predicateSetLiquidProperties;

		// Token: 0x04000A3E RID: 2622
		public static Predicate predicateIncreaseWaterLevel;

		// Token: 0x04000A3F RID: 2623
		public static Predicate predicateStepIncreaseWaterLevel;

		// Token: 0x04000A40 RID: 2624
		public static Predicate predicateSetMaxPositiveWaterLevelOffset;

		// Token: 0x04000A41 RID: 2625
		public static Predicate predicateSetMaxNegativeWaterLevelOffset;

		// Token: 0x04000A42 RID: 2626
		private float earthQuakeStrength;

		// Token: 0x04000A43 RID: 2627
		private float cameraShakeStrength;

		// Token: 0x04000A44 RID: 2628
		private float prolongedEarthQuakeStrength;

		// Token: 0x04000A45 RID: 2629
		private float prolongedCameraShakeStrength;

		// Token: 0x04000A46 RID: 2630
		private float earthQuakeStartTime;

		// Token: 0x04000A47 RID: 2631
		private float cameraShakeStartTime;

		// Token: 0x04000A48 RID: 2632
		private string toEnvEffect;

		// Token: 0x04000A49 RID: 2633
		private string fromEnvEffect;

		// Token: 0x04000A4A RID: 2634
		private float fromEnvEffectIntensity = 1f;

		// Token: 0x04000A4B RID: 2635
		private string _fogPaint = "White";

		// Token: 0x04000A4C RID: 2636
		private float _fogDensity = 10f;

		// Token: 0x04000A4D RID: 2637
		private float _fogStart = 100f;

		// Token: 0x04000A4E RID: 2638
		private bool tiltGravity;

		// Token: 0x04000A4F RID: 2639
		private bool trackingTilt;

		// Token: 0x04000A50 RID: 2640
		private bool tiltGravityFlat;

		// Token: 0x04000A51 RID: 2641
		private float tiltGravityStrength = 1f;

		// Token: 0x04000A52 RID: 2642
		internal static RigidbodyConstraints masterRBConstraints;

		// Token: 0x04000A53 RID: 2643
		private static bool masterRBConstraintsUpdated;

		// Token: 0x04000A54 RID: 2644
		private Dictionary<Block, int> earthquakeBlocks = new Dictionary<Block, int>();

		// Token: 0x04000A55 RID: 2645
		private const float DEFAULT_GRAVITY = 9.82f;

		// Token: 0x04000A56 RID: 2646
		public static GravityDefinitions gravDefs;

		// Token: 0x04000A57 RID: 2647
		public static AtmosphereDefinitions atmosDefs;
	}
}
