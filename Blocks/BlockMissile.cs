using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020000AA RID: 170
	public class BlockMissile : Block, IMissileLauncher
	{
		// Token: 0x06000D69 RID: 3433 RVA: 0x0005C89C File Offset: 0x0005AC9C
		public BlockMissile(List<List<Tile>> tiles) : base(tiles)
		{
			if (BlockMissile.smokeGo == null)
			{
				BlockMissile.smokeGo = (UnityEngine.Object.Instantiate(Resources.Load("Blocks/Missile Exhaust")) as GameObject);
				BlockMissile.particles = BlockMissile.smokeGo.GetComponent<ParticleSystem>();
				BlockMissile.particles.enableEmission = false;
			}
			this.sfxLoopUpdateCounter = UnityEngine.Random.Range(0, 5);
			this.loopName = "Rocket Burst 3 Loop";
			MissileMetaData component = this.go.GetComponent<MissileMetaData>();
			if (component != null)
			{
				this.smokeExitOffset = component.exitOffset;
				this.loopName = component.loopSfx;
			}
			else
			{
				BWLog.Info("Could not find missile meta data component on " + base.BlockType());
			}
		}

		// Token: 0x06000D6A RID: 3434 RVA: 0x0005CA05 File Offset: 0x0005AE05
		public override void Pause()
		{
			BlockMissile.particles.Pause();
			this.PlayLoopSound(false, base.GetLoopClip(), 1f, null, 1f);
		}

		// Token: 0x06000D6B RID: 3435 RVA: 0x0005CA29 File Offset: 0x0005AE29
		public override void Resume()
		{
			if (BlockMissile.particles.isPaused)
			{
				BlockMissile.particles.Play();
			}
		}

		// Token: 0x06000D6C RID: 3436 RVA: 0x0005CA44 File Offset: 0x0005AE44
		public static void WorldLoaded()
		{
			BlockMissile.nextMissileLabel = 0;
		}

		// Token: 0x06000D6D RID: 3437 RVA: 0x0005CA4C File Offset: 0x0005AE4C
		public new static void Register()
		{
			BlockMissile.predicateMissileLaunch = PredicateRegistry.Add<BlockMissile>("Missile.Launch", (Block b) => new PredicateSensorDelegate(((BlockMissile)b).MissileIsLaunched), (Block b) => new PredicateActionDelegate(((BlockMissile)b).Launch), new Type[]
			{
				typeof(float)
			}, null, null);
			BlockMissile.predicateMissileType = PredicateRegistry.Add<BlockMissile>("Missile.SetMissileType", null, (Block b) => new PredicateActionDelegate(((BlockMissile)b).SetMissileType), new Type[]
			{
				typeof(int),
				typeof(float)
			}, new string[]
			{
				"Type",
				"Yield"
			}, null);
			PredicateRegistry.Add<BlockMissile>("Missile.TargetTag", null, (Block b) => new PredicateActionDelegate(((BlockMissile)b).TargetTag), new Type[]
			{
				typeof(string),
				typeof(float)
			}, new string[]
			{
				"Tag name",
				"Lock delay"
			}, null);
			BlockMissile.predicateMissileLabel = PredicateRegistry.Add<BlockMissile>("Missile.Label", null, (Block b) => new PredicateActionDelegate(((BlockMissile)b).Label), new Type[]
			{
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<BlockMissile>("Missile.Ballistic", null, (Block b) => new PredicateActionDelegate(((BlockMissile)b).Ballistic), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMissile>("Missile.InitLifetime", (Block b) => new PredicateSensorDelegate(((BlockMissile)b).MissileIsExpired), (Block b) => new PredicateActionDelegate(((BlockMissile)b).InitLifetime), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMissile>("Missile.InFlightTimeGreaterThan", (Block b) => new PredicateSensorDelegate(((BlockMissile)b).InFlightTimeGreaterThan), null, new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMissile>("Missile.InFlightTimeLessThan", (Block b) => new PredicateSensorDelegate(((BlockMissile)b).InFlightTimeLessThan), null, new Type[]
			{
				typeof(float)
			}, null, null);
			Predicate predicate = PredicateRegistry.Add<BlockMissile>("Missile.SetBurstTime", null, (Block b) => new PredicateActionDelegate(((BlockMissile)b).SetBurstTime), new Type[]
			{
				typeof(float)
			}, null, null);
			predicate.updatesIconOnArgumentChange = true;
			PredicateRegistry.Add<BlockMissile>("Missile.Reload", (Block b) => new PredicateSensorDelegate(((BlockMissile)b).MissileIsLoaded), (Block b) => new PredicateActionDelegate(((BlockMissile)b).Reload), null, null, null);
			Dictionary<string, int> dictionary = new Dictionary<string, int>
			{
				{
					"Missile A",
					0
				},
				{
					"Missile B",
					0
				},
				{
					"Missile C",
					0
				}
			};
			foreach (KeyValuePair<string, int> keyValuePair in dictionary)
			{
				List<List<Tile>> value = new List<List<Tile>>
				{
					new List<Tile>
					{
						Block.ThenTile(),
						BlockMissile.MissileTypeTile(keyValuePair.Value),
						BlockMissile.MissileLabelTile(1)
					},
					new List<Tile>
					{
						Block.ButtonTile("R"),
						Block.ThenTile(),
						BlockMissile.MissileLaunchTile()
					},
					Block.EmptyTileRow()
				};
				Block.defaultExtraTiles[keyValuePair.Key] = value;
				Block.defaultExtraTilesProcessors[keyValuePair.Key] = delegate(List<List<Tile>> tiles)
				{
					foreach (List<Tile> list in tiles)
					{
						foreach (Tile tile in list)
						{
							GAF gaf = tile.gaf;
							if (gaf.Predicate == BlockMissile.predicateMissileLabel)
							{
								gaf.Args[0] = BlockMissile.nextMissileLabel + 1;
								BlockMissile.nextMissileLabel = (BlockMissile.nextMissileLabel + 1) % 6;
							}
						}
					}
				};
			}
		}

		// Token: 0x06000D6E RID: 3438 RVA: 0x0005CE9C File Offset: 0x0005B29C
		public static Tile MissileTypeTile(int type)
		{
			return new Tile(BlockMissile.predicateMissileType, new object[]
			{
				type,
				3f
			});
		}

		// Token: 0x06000D6F RID: 3439 RVA: 0x0005CEC4 File Offset: 0x0005B2C4
		public static Tile MissileLabelTile(int label)
		{
			return new Tile(BlockMissile.predicateMissileLabel, new object[]
			{
				label
			});
		}

		// Token: 0x06000D70 RID: 3440 RVA: 0x0005CEDF File Offset: 0x0005B2DF
		public static Tile MissileLaunchTile()
		{
			return new Tile(BlockMissile.predicateMissileLaunch, new object[]
			{
				1f
			});
		}

		// Token: 0x06000D71 RID: 3441 RVA: 0x0005CF00 File Offset: 0x0005B300
		public override void Play()
		{
			base.Play();
			this.playChunk = null;
			if (BlockMissile.particles.isPaused)
			{
				BlockMissile.particles.Play();
			}
			if (BlockMissile.globalMissileControllers == null)
			{
				BlockMissile.globalMissileControllers = new List<BlockMissileControl>();
				foreach (Block block in BWSceneManager.AllBlocks())
				{
					BlockMissileControl blockMissileControl = block as BlockMissileControl;
					if (blockMissileControl != null)
					{
						BlockMissile.globalMissileControllers.Add(blockMissileControl);
					}
				}
			}
			foreach (BlockMissileControl blockMissileControl2 in BlockMissile.globalMissileControllers)
			{
				blockMissileControl2.RegisterMissileLauncher(this);
			}
			base.UpdateConnectedCache();
			List<Block> list = Block.connectedCache[this];
			foreach (Block block2 in list)
			{
				BlockModelMissileControl blockModelMissileControl = block2 as BlockModelMissileControl;
				if (blockModelMissileControl != null)
				{
					blockModelMissileControl.RegisterMissileLauncher(this);
				}
			}
			this.endWaitCounter = 0;
			this.initLifetime = 10f;
			this.burstTime = 10f;
			this.burstTimeSet = false;
			this.globalBurstTime = 10f;
			this.globalBurstTimeSet = false;
			this.gravityFraction = 1f;
			Vector3 vector = base.Scale();
			this.smokeOffset = vector.y * 0.5f;
			this.smokeSize = Mathf.Min(vector.x, vector.z);
			this.missileState = BlockMissile.MissileState.LOADED;
			this.ResetFrame();
			this.labels.Clear();
			this.labelSetCounters = new int[7];
			if (this.setSmokeColor)
			{
				this.UpdateSmokeColorPaint(this.GetPaint(this.smokeColorMeshIndex), this.smokeColorMeshIndex);
				this.UpdateSmokeColorTexture(base.GetTexture(this.smokeColorMeshIndex), this.smokeColorMeshIndex);
			}
			this.reloadConnections = new List<Block>();
			for (int i = 0; i < this.connections.Count; i++)
			{
				Block block3 = this.connections[i];
				if (!(block3 is BlockMissile))
				{
					this.reloadConnections.Add(block3);
				}
			}
		}

		// Token: 0x06000D72 RID: 3442 RVA: 0x0005D188 File Offset: 0x0005B588
		public override bool CanTriggerBlockListSensor()
		{
			return this.missileState == BlockMissile.MissileState.LOADED;
		}

		// Token: 0x06000D73 RID: 3443 RVA: 0x0005D194 File Offset: 0x0005B594
		public override void Stop(bool resetBlock = true)
		{
			BlockMissile.globalMissileControllers = null;
			this.DestroyLaunchedMissile();
			Util.UnparentTransformSafely(this.goT);
			this.go.SetActive(true);
			if (this.goShadow != null)
			{
				this.goShadow.SetActive(true);
			}
			this.missileState = BlockMissile.MissileState.LOADED;
			this.ResetFrame();
			this.labelSetCounters = new int[7];
			this.labels.Clear();
			this.reloadConnections.Clear();
			BlockMissile.particles.Clear();
			base.Stop(resetBlock);
		}

		// Token: 0x06000D74 RID: 3444 RVA: 0x0005D221 File Offset: 0x0005B621
		public override void Destroy()
		{
			this.DestroyLaunchedMissile();
			base.Destroy();
		}

		// Token: 0x06000D75 RID: 3445 RVA: 0x0005D230 File Offset: 0x0005B630
		public override TileResultCode ExplodeLocal(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.broken)
			{
				return TileResultCode.True;
			}
			if (this.missileState == BlockMissile.MissileState.LAUNCHED && this.launchedMissile != null)
			{
				float floatArg = Util.GetFloatArg(args, 0, 3f);
				this.ExplodeMissile(floatArg, false);
			}
			else if (this.missileState == BlockMissile.MissileState.LOADED)
			{
				base.ExplodeLocal(eInfo, args);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000D76 RID: 3446 RVA: 0x0005D290 File Offset: 0x0005B690
		private void DestroyLaunchedMissile()
		{
			if (this.launchedMissile != null)
			{
				this.launchedMissile.Destroy();
				this.launchedMissile = null;
			}
		}

		// Token: 0x06000D77 RID: 3447 RVA: 0x0005D2AF File Offset: 0x0005B6AF
		private void DeactivateLaunchedMissile()
		{
			if (this.launchedMissile != null)
			{
				this.launchedMissile.Deactivate();
			}
		}

		// Token: 0x06000D78 RID: 3448 RVA: 0x0005D2C7 File Offset: 0x0005B6C7
		public override bool BreakByDetachExplosion()
		{
			return this.missileState == BlockMissile.MissileState.LOADED;
		}

		// Token: 0x06000D79 RID: 3449 RVA: 0x0005D2D2 File Offset: 0x0005B6D2
		private bool StepEndWaitCounter()
		{
			if (this.endWaitCounter > 40)
			{
				this.DestroyLaunchedMissile();
				this.endWaitCounter = 0;
				return true;
			}
			if (this.endWaitCounter == 0)
			{
				this.DeactivateLaunchedMissile();
			}
			this.endWaitCounter++;
			return false;
		}

		// Token: 0x06000D7A RID: 3450 RVA: 0x0005D310 File Offset: 0x0005B710
		public override TileResultCode Explode(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.missileState == BlockMissile.MissileState.LAUNCHED && this.launchedMissile != null)
			{
				this.launchedMissile.Break();
				return TileResultCode.True;
			}
			return base.Explode(eInfo, args);
		}

		// Token: 0x06000D7B RID: 3451 RVA: 0x0005D33E File Offset: 0x0005B73E
		public void RemoveMissile()
		{
			if (this.launchedMissile != null)
			{
				this.launchedMissile.SetExpired();
			}
			else
			{
				BWSceneManager.RemovePlayBlock(this);
			}
		}

		// Token: 0x06000D7C RID: 3452 RVA: 0x0005D364 File Offset: 0x0005B764
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (this.launchedMissile != null)
			{
				this.launchedMissile.FixedUpdate();
				if (this.missileType != -1 && this.missileState == BlockMissile.MissileState.LAUNCHED && !this.launchedMissile.IsBroken() && CollisionManager.IsImpactingBlock(this))
				{
					float explodeRadius = (!this.innateExplodeRadiusSet) ? this.size.magnitude : this.innateExplodeRadius;
					this.ExplodeMissile(explodeRadius, false);
				}
				if (this.launchedMissile.HasExploded())
				{
					this.StepEndWaitCounter();
					this.missileState = BlockMissile.MissileState.EXPLODED;
				}
				else if (this.launchedMissile.HasExpired())
				{
					this.StepEndWaitCounter();
					this.missileState = BlockMissile.MissileState.EXPIRED;
				}
			}
			if (Sound.sfxEnabled && !this.vanished && this.go.activeInHierarchy)
			{
				float num = 1f;
				float f = Mathf.Min(this.size.x, this.size.z);
				float num2 = Mathf.Sqrt(f);
				bool flag = this.missileState == BlockMissile.MissileState.LAUNCHED && this.launchedMissile.IsBursting();
				float num3 = (!flag) ? -0.04f : 0.04f;
				this.playBurstLevel = Mathf.Clamp(this.playBurstLevel + num3, 0f, Mathf.Clamp(0.5f * num, 0.1f, 1f));
				float num4 = (0.98f + Mathf.Min(0.1f, 0.02f * num)) / (0.5f + 0.5f * num2);
				num4 = Mathf.Clamp(num4, 0.25f, 1.25f);
				if (this.sfxLoopUpdateCounter % 5 == 0)
				{
					this.PlayLoopSound(flag || this.playBurstLevel > 0.01f, base.GetLoopClip(), this.playBurstLevel, null, num4);
					base.UpdateWithinWaterLPFilter(null);
				}
				this.sfxLoopUpdateCounter++;
			}
			else
			{
				this.PlayLoopSound(false, base.GetLoopClip(), 1f, null, 1f);
			}
		}

		// Token: 0x06000D7D RID: 3453 RVA: 0x0005D581 File Offset: 0x0005B981
		public bool MissileGone()
		{
			return this.missileState == BlockMissile.MissileState.EXPIRED || this.missileState == BlockMissile.MissileState.EXPLODED;
		}

		// Token: 0x06000D7E RID: 3454 RVA: 0x0005D59B File Offset: 0x0005B99B
		public bool IsLoaded()
		{
			return this.missileState == BlockMissile.MissileState.LOADED;
		}

		// Token: 0x06000D7F RID: 3455 RVA: 0x0005D5A6 File Offset: 0x0005B9A6
		public bool CanLaunch()
		{
			return this.missileState == BlockMissile.MissileState.LOADED && !this.isTreasure;
		}

		// Token: 0x06000D80 RID: 3456 RVA: 0x0005D5BF File Offset: 0x0005B9BF
		private void UpdateSmokeColorTexture(string texture, int meshIndex)
		{
			if (this.setSmokeColor && meshIndex == this.smokeColorMeshIndex && this.subMeshGameObjects.Count > this.smokeColorMeshIndex)
			{
				this.emitSmoke = (texture != "Glass");
			}
		}

		// Token: 0x06000D81 RID: 3457 RVA: 0x0005D600 File Offset: 0x0005BA00
		private void UpdateSmokeColorPaint(string paint, int meshIndex)
		{
			if (this.setSmokeColor && meshIndex == this.smokeColorMeshIndex && this.subMeshGameObjects.Count > this.smokeColorMeshIndex)
			{
				Renderer renderer = (meshIndex != 0) ? this.subMeshGameObjects[meshIndex - 1].GetComponent<Renderer>() : this.go.GetComponent<Renderer>();
				this.smokeColor = BlockAbstractRocket.GetSmokeColor(paint, renderer);
			}
		}

		// Token: 0x06000D82 RID: 3458 RVA: 0x0005D671 File Offset: 0x0005BA71
		private void SetInnateExplodeRadius(float r)
		{
			this.innateExplodeRadius = r;
			this.innateExplodeRadiusSet = true;
		}

		// Token: 0x06000D83 RID: 3459 RVA: 0x0005D684 File Offset: 0x0005BA84
		public override void Exploded()
		{
			base.Exploded();
			if (this.missileState == BlockMissile.MissileState.LAUNCHED || this.missileState == BlockMissile.MissileState.LOADED)
			{
				float explodeRadius = (!this.innateExplodeRadiusSet) ? this.size.magnitude : this.innateExplodeRadius;
				this.ExplodeMissile(explodeRadius, true);
			}
		}

		// Token: 0x06000D84 RID: 3460 RVA: 0x0005D6D8 File Offset: 0x0005BAD8
		public override bool TreatAsVehicleLikeBlock()
		{
			return false;
		}

		// Token: 0x06000D85 RID: 3461 RVA: 0x0005D6DC File Offset: 0x0005BADC
		public void LaunchMissile(float burstMultiplier)
		{
			Chunk chunk = this.chunk;
			base.PlayPositionedSound("missile_launcher_fire_single", 1f, 1f);
			Quaternion oldLocalRotation = this.goT.rotation;
			Vector3 oldLocalPosition = this.goT.position;
			if (this.reloadConnections.Count > 0)
			{
				oldLocalRotation = Quaternion.Inverse(this.reloadConnections[0].goT.rotation) * this.goT.rotation;
				oldLocalPosition = this.reloadConnections[0].goT.InverseTransformPoint(this.goT.position);
			}
			chunk.RemoveBlock(this);
			Blocksworld.blocksworldCamera.ChunkDirty(chunk);
			this.playChunk = chunk;
			Vector3 velocity = Vector3.zero;
			Vector3 angularVelocity = Vector3.zero;
			bool oldRbWasKinematic = false;
			Rigidbody rb = chunk.rb;
			if (rb != null)
			{
				if (!rb.isKinematic)
				{
					angularVelocity = rb.angularVelocity;
					Vector3 lhs = this.goT.position - rb.worldCenterOfMass;
					velocity = rb.velocity + Vector3.Cross(lhs, rb.angularVelocity);
				}
				oldRbWasKinematic = rb.isKinematic;
				if (chunk.blocks.Count == 0)
				{
					rb.isKinematic = (base.ConnectionsOfType(2, false).Count == 0);
				}
			}
			Chunk chunk2 = new Chunk(new List<Block>
			{
				this
			}, true);
			Blocksworld.chunks.Add(chunk2);
			Rigidbody rb2 = chunk2.rb;
			rb2.velocity = velocity;
			rb2.angularVelocity = angularVelocity;
			Blocksworld.blocksworldCamera.SetSingleton(this, true);
			MissileBody missileBody = new MissileBody
			{
				block = this,
				chunk = chunk2,
				oldLocalRotation = oldLocalRotation,
				oldLocalPosition = oldLocalPosition,
				oldRbWasKinematic = oldRbWasKinematic,
				lifetime = this.initLifetime,
				burstMultiplier = burstMultiplier
			};
			this.launchedMissile = missileBody;
			this.missileState = BlockMissile.MissileState.LAUNCHED;
		}

		// Token: 0x06000D86 RID: 3462 RVA: 0x0005D8D8 File Offset: 0x0005BCD8
		public bool CanReload()
		{
			bool flag = this.launchedMissile == null && (this.missileState == BlockMissile.MissileState.EXPLODED || this.missileState == BlockMissile.MissileState.EXPIRED);
			if (flag)
			{
				foreach (Block block in this.reloadConnections)
				{
					if (block.broken)
					{
						flag = false;
						break;
					}
				}
			}
			return flag;
		}

		// Token: 0x06000D87 RID: 3463 RVA: 0x0005D970 File Offset: 0x0005BD70
		public void Reload()
		{
			bool active = true;
			this.go.SetActive(active);
			if (this.goShadow != null)
			{
				this.goShadow.SetActive(active);
			}
			this.didFix = false;
			this.missileState = BlockMissile.MissileState.LOADED;
			this.Appeared();
		}

		// Token: 0x06000D88 RID: 3464 RVA: 0x0005D9BC File Offset: 0x0005BDBC
		public bool HasLabel(int label)
		{
			return this.labels.Contains(label);
		}

		// Token: 0x06000D89 RID: 3465 RVA: 0x0005D9CA File Offset: 0x0005BDCA
		public IMissile GetLaunchedMissile()
		{
			return this.launchedMissile;
		}

		// Token: 0x06000D8A RID: 3466 RVA: 0x0005D9D2 File Offset: 0x0005BDD2
		public void AddControllerTargetTag(string t, float lockDelay)
		{
			this.controllerTargetTag = t;
			this.controllerLockDelay = lockDelay;
		}

		// Token: 0x06000D8B RID: 3467 RVA: 0x0005D9E2 File Offset: 0x0005BDE2
		public void SetGlobalBurstTime(float bt)
		{
			this.globalBurstTimeSet = true;
			this.globalBurstTime = bt;
		}

		// Token: 0x06000D8C RID: 3468 RVA: 0x0005D9F4 File Offset: 0x0005BDF4
		public override void ResetFrame()
		{
			this.localLockDelay = 0.5f;
			this.controllerLockDelay = 0.5f;
			this.localTargetTag = null;
			this.controllerTargetTag = null;
			this.globalBurstTimeSet = false;
			this.innateExplodeRadiusSet = false;
			this.burstTimeSet = false;
			this.missileType = -1;
			for (int i = 0; i < this.labelSetCounters.Length; i++)
			{
				int num = this.labelSetCounters[i];
				num--;
				if (num == 0)
				{
					this.labels.Remove(i);
				}
				else
				{
					this.labelSetCounters[i] = num;
				}
			}
		}

		// Token: 0x06000D8D RID: 3469 RVA: 0x0005DA88 File Offset: 0x0005BE88
		public void AddLocalExplosion(Vector3 pos, Vector3 vel, float radius)
		{
			RadialExplosionCommand c = new RadialExplosionCommand(5f, pos, vel, radius * 0.5f, radius, radius * 2f, new HashSet<Block>
			{
				this
			}, string.Empty);
			Blocksworld.AddFixedUpdateCommand(c);
			Sound.PlayPositionedOneShot("missile_detonation", pos, 5f, Mathf.Max(120f, radius * 30f), 150f, AudioRolloffMode.Logarithmic);
		}

		// Token: 0x06000D8E RID: 3470 RVA: 0x0005DAF4 File Offset: 0x0005BEF4
		public void EmitSmoke(Vector3 pos, Vector3 velocity)
		{
			if (this.emitSmoke)
			{
				Transform goT = this.goT;
				Vector3 b = goT.TransformDirection(this.smokeExitOffset);
				ParticleSystem.Particle particle = new ParticleSystem.Particle
				{
					position = pos + b,
					velocity = velocity,
					size = this.smokeSize,
					remainingLifetime = 1f,
					rotation = (float)UnityEngine.Random.Range(-180, 180),
					startLifetime = 1f,
					color = this.smokeColor,
					randomSeed = (uint)UnityEngine.Random.Range(17, 9999999)
				};
				BlockMissile.particles.Emit(particle);
			}
		}

		// Token: 0x06000D8F RID: 3471 RVA: 0x0005DBAC File Offset: 0x0005BFAC
		private void ExplodeMissile(float explodeRadius, bool force = false)
		{
			if (this.launchedMissile != null && this.launchedMissile.CanExplode())
			{
				this.launchedMissile.Explode(explodeRadius);
				this.missileState = BlockMissile.MissileState.EXPLODED;
			}
			if (this.missileState == BlockMissile.MissileState.LOADED && force)
			{
				this.go.SetActive(false);
				if (this.goShadow != null)
				{
					this.goShadow.SetActive(false);
				}
				Vector3 vel = Vector3.zero;
				Rigidbody rb = this.chunk.rb;
				if (rb != null && !rb.isKinematic)
				{
					vel = rb.velocity;
				}
				this.AddLocalExplosion(this.goT.position, vel, explodeRadius);
				this.missileState = BlockMissile.MissileState.EXPLODED;
			}
		}

		// Token: 0x06000D90 RID: 3472 RVA: 0x0005DC6B File Offset: 0x0005C06B
		public TileResultCode MissileIsExploded(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (this.missileState != BlockMissile.MissileState.EXPLODED) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000D91 RID: 3473 RVA: 0x0005DC80 File Offset: 0x0005C080
		public TileResultCode Reload(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.CanReload())
			{
				this.Reload();
			}
			return TileResultCode.True;
		}

		// Token: 0x06000D92 RID: 3474 RVA: 0x0005DC94 File Offset: 0x0005C094
		public TileResultCode InFlightTimeGreaterThan(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float floatArg = Util.GetFloatArg(args, 0, 1f);
			return (this.launchedMissile == null || this.missileState != BlockMissile.MissileState.LAUNCHED || this.launchedMissile.GetInFlightTime() <= floatArg) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000D93 RID: 3475 RVA: 0x0005DCE0 File Offset: 0x0005C0E0
		public TileResultCode InFlightTimeLessThan(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float floatArg = Util.GetFloatArg(args, 0, 1f);
			return (this.launchedMissile == null || this.missileState != BlockMissile.MissileState.LAUNCHED || this.launchedMissile.GetInFlightTime() >= floatArg) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000D94 RID: 3476 RVA: 0x0005DD29 File Offset: 0x0005C129
		public TileResultCode InitLifetime(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.initLifetime = Util.GetFloatArg(args, 0, 10f);
			if (this.launchedMissile != null && this.missileState == BlockMissile.MissileState.LAUNCHED)
			{
				this.launchedMissile.SetLifetime(this.initLifetime);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000D95 RID: 3477 RVA: 0x0005DD66 File Offset: 0x0005C166
		public TileResultCode SetBurstTime(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.burstTimeSet = true;
			this.burstTime = Util.GetFloatArg(args, 0, 10f);
			return TileResultCode.True;
		}

		// Token: 0x06000D96 RID: 3478 RVA: 0x0005DD82 File Offset: 0x0005C182
		public TileResultCode Ballistic(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.gravityFraction = Util.GetFloatArg(args, 0, 1f);
			return TileResultCode.True;
		}

		// Token: 0x06000D97 RID: 3479 RVA: 0x0005DD98 File Offset: 0x0005C198
		public TileResultCode Launch(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.CanLaunch())
			{
				float floatArg = Util.GetFloatArg(args, 0, 1f);
				this.LaunchMissile(floatArg);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000D98 RID: 3480 RVA: 0x0005DDC5 File Offset: 0x0005C1C5
		public TileResultCode MissileIsLaunched(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (this.missileState != BlockMissile.MissileState.LAUNCHED) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000D99 RID: 3481 RVA: 0x0005DDDA File Offset: 0x0005C1DA
		public TileResultCode MissileIsExpired(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (this.missileState != BlockMissile.MissileState.EXPIRED) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000D9A RID: 3482 RVA: 0x0005DDEF File Offset: 0x0005C1EF
		public TileResultCode MissileIsExploaded(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (this.missileState != BlockMissile.MissileState.EXPLODED) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000D9B RID: 3483 RVA: 0x0005DE04 File Offset: 0x0005C204
		public TileResultCode MissileIsLoaded(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (this.missileState != BlockMissile.MissileState.LOADED) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000D9C RID: 3484 RVA: 0x0005DE18 File Offset: 0x0005C218
		public TileResultCode SetMissileType(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.missileType = Util.GetIntArg(args, 0, -1);
			this.innateExplodeRadius = Util.GetFloatArg(args, 1, 3f);
			this.innateExplodeRadiusSet = true;
			int num = this.missileType;
			if (num != 0)
			{
				if (num != 1)
				{
					if (num == 2)
					{
						this.burstTimeSet = true;
						this.burstTime = 0f;
						this.gravityFraction = 1f;
					}
				}
				else
				{
					this.burstTimeSet = true;
					this.burstTime = 1f;
					this.gravityFraction = 1f;
				}
			}
			else
			{
				this.gravityFraction = 0f;
			}
			return TileResultCode.True;
		}

		// Token: 0x06000D9D RID: 3485 RVA: 0x0005DEC1 File Offset: 0x0005C2C1
		public static string GetMissileTypeIconName(int type)
		{
			if (type == 0)
			{
				return "Missile Type";
			}
			if (type == 1)
			{
				return "Ballistic Type";
			}
			if (type != 2)
			{
				return "Unknown Type";
			}
			return "Bomb Type";
		}

		// Token: 0x06000D9E RID: 3486 RVA: 0x0005DEF3 File Offset: 0x0005C2F3
		public TileResultCode TargetTag(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.localTargetTag = Util.GetStringArg(args, 0, string.Empty);
			this.localLockDelay = Util.GetFloatArg(args, 1, 0.5f);
			return TileResultCode.True;
		}

		// Token: 0x06000D9F RID: 3487 RVA: 0x0005DF1C File Offset: 0x0005C31C
		public TileResultCode Label(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 0, 0);
			this.labels.Add(intArg);
			this.labelSetCounters[intArg] = 2;
			return TileResultCode.True;
		}

		// Token: 0x06000DA0 RID: 3488 RVA: 0x0005DF4C File Offset: 0x0005C34C
		public override TileResultCode TextureTo(string texture, Vector3 normal, bool permanent, int meshIndex = 0, bool force = false)
		{
			TileResultCode result = base.TextureTo(texture, normal, permanent, meshIndex, force);
			this.UpdateSmokeColorTexture(texture, meshIndex);
			return result;
		}

		// Token: 0x06000DA1 RID: 3489 RVA: 0x0005DF74 File Offset: 0x0005C374
		public override TileResultCode PaintTo(string paint, bool permanent, int meshIndex = 0)
		{
			TileResultCode result = base.PaintTo(paint, permanent, meshIndex);
			this.UpdateSmokeColorPaint(paint, meshIndex);
			return result;
		}

		// Token: 0x06000DA2 RID: 3490 RVA: 0x0005DF94 File Offset: 0x0005C394
		protected override bool IgnoreChangesToDefaultForPredicate(Predicate predicate)
		{
			return predicate == BlockMissile.predicateMissileLabel;
		}

		// Token: 0x04000A91 RID: 2705
		public static Predicate predicateMissileLabel;

		// Token: 0x04000A92 RID: 2706
		public static Predicate predicateMissileType;

		// Token: 0x04000A93 RID: 2707
		public static Predicate predicateMissileLaunch;

		// Token: 0x04000A94 RID: 2708
		private int sfxLoopUpdateCounter;

		// Token: 0x04000A95 RID: 2709
		private const int SFX_LOOP_UPDATE_INTERVAL = 5;

		// Token: 0x04000A96 RID: 2710
		private float playBurstLevel;

		// Token: 0x04000A97 RID: 2711
		public static int nextMissileLabel;

		// Token: 0x04000A98 RID: 2712
		public const int MISSILE_LABEL_COUNT = 6;

		// Token: 0x04000A99 RID: 2713
		private BlockMissile.MissileState missileState;

		// Token: 0x04000A9A RID: 2714
		private int endWaitCounter;

		// Token: 0x04000A9B RID: 2715
		private static List<BlockMissileControl> globalMissileControllers;

		// Token: 0x04000A9C RID: 2716
		public HashSet<int> labels = new HashSet<int>();

		// Token: 0x04000A9D RID: 2717
		public int[] labelSetCounters = new int[7];

		// Token: 0x04000A9E RID: 2718
		public string localTargetTag;

		// Token: 0x04000A9F RID: 2719
		public string controllerTargetTag;

		// Token: 0x04000AA0 RID: 2720
		public float localLockDelay = 0.5f;

		// Token: 0x04000AA1 RID: 2721
		public float controllerLockDelay = 0.5f;

		// Token: 0x04000AA2 RID: 2722
		private IMissile launchedMissile;

		// Token: 0x04000AA3 RID: 2723
		internal Chunk playChunk;

		// Token: 0x04000AA4 RID: 2724
		public float gravityFraction = 1f;

		// Token: 0x04000AA5 RID: 2725
		private float initLifetime = 10f;

		// Token: 0x04000AA6 RID: 2726
		public float burstTime = 10f;

		// Token: 0x04000AA7 RID: 2727
		public bool burstTimeSet;

		// Token: 0x04000AA8 RID: 2728
		public float globalBurstTime = 10f;

		// Token: 0x04000AA9 RID: 2729
		public bool globalBurstTimeSet;

		// Token: 0x04000AAA RID: 2730
		public float innateExplodeRadius = 3f;

		// Token: 0x04000AAB RID: 2731
		public bool innateExplodeRadiusSet;

		// Token: 0x04000AAC RID: 2732
		public int missileType = -1;

		// Token: 0x04000AAD RID: 2733
		public float smokeOffset = 1f;

		// Token: 0x04000AAE RID: 2734
		public float smokeSize = 1f;

		// Token: 0x04000AAF RID: 2735
		private static GameObject smokeGo;

		// Token: 0x04000AB0 RID: 2736
		private static ParticleSystem particles;

		// Token: 0x04000AB1 RID: 2737
		private Color smokeColor = Color.white;

		// Token: 0x04000AB2 RID: 2738
		private bool setSmokeColor = true;

		// Token: 0x04000AB3 RID: 2739
		protected int smokeColorMeshIndex;

		// Token: 0x04000AB4 RID: 2740
		private bool emitSmoke = true;

		// Token: 0x04000AB5 RID: 2741
		internal List<Block> reloadConnections = new List<Block>();

		// Token: 0x04000AB6 RID: 2742
		public Vector3 smokeExitOffset = Vector3.zero;

		// Token: 0x020000AB RID: 171
		private enum MissileState
		{
			// Token: 0x04000AC6 RID: 2758
			LOADED,
			// Token: 0x04000AC7 RID: 2759
			LAUNCHED,
			// Token: 0x04000AC8 RID: 2760
			EXPLODED,
			// Token: 0x04000AC9 RID: 2761
			EXPIRED
		}
	}
}
