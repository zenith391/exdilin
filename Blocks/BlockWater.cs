using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020000ED RID: 237
	public class BlockWater : BlockAbstractWater
	{
		// Token: 0x06001191 RID: 4497 RVA: 0x00077ACC File Offset: 0x00075ECC
		public BlockWater(List<List<Tile>> tiles) : base(tiles)
		{
			Transform transform = this.goT.Find("Reflective Water");
			if (transform != null)
			{
				this.reflectiveWater = transform.gameObject;
			}
		}

		// Token: 0x06001192 RID: 4498 RVA: 0x00077B47 File Offset: 0x00075F47
		public new static void Register()
		{
		}

		// Token: 0x06001193 RID: 4499 RVA: 0x00077B4C File Offset: 0x00075F4C
		public static bool BlockWithinWater(Block block, bool checkDensity = false)
		{
			int instanceId = block.GetInstanceId();
			if (BlockWater.blocksWithinWater != null && BlockWater.blocksWithinWater.Contains(block))
			{
				return !checkDensity || BlockWater.mainWater.waterDensity != 0f || !(block is BlockAnimatedCharacter);
			}
			for (int i = 0; i < BlockAbstractWater.waterCubes.Count; i++)
			{
				BlockWaterCube blockWaterCube = BlockAbstractWater.waterCubes[i];
				if (blockWaterCube.blocksWithinWater != null && blockWaterCube.blocksWithinWater.Contains(instanceId))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001194 RID: 4500 RVA: 0x00077BEC File Offset: 0x00075FEC
		public static bool BlockWithinTaggedWater(Block block, string tag)
		{
			int instanceId = block.GetInstanceId();
			for (int i = 0; i < BlockAbstractWater.waterCubes.Count; i++)
			{
				BlockWaterCube blockWaterCube = BlockAbstractWater.waterCubes[i];
				HashSet<int> hashSet;
				if (blockWaterCube.blocksWithinTaggedWater.TryGetValue(tag, out hashSet) && hashSet.Contains(instanceId))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001195 RID: 4501 RVA: 0x00077C4C File Offset: 0x0007604C
		public override void OnCreate()
		{
			this.TextureTo(base.GetTexture(0), base.GetTextureNormal(), true, 0, false);
			this.UpdateSolidness();
			if (this.go.GetComponent<Collider>() != null)
			{
				this.go.GetComponent<Collider>().isTrigger = false;
			}
			this.UpdateRenderState();
		}

		// Token: 0x06001196 RID: 4502 RVA: 0x00077CA3 File Offset: 0x000760A3
		public override void OnReconstructed()
		{
			base.OnReconstructed();
			this.UpdateSolidness();
			this.TextureTo(Scarcity.GetNormalizedTexture(base.GetTexture(0)), Vector3.up, true, 0, true);
		}

		// Token: 0x06001197 RID: 4503 RVA: 0x00077CCC File Offset: 0x000760CC
		private void UpdateSolidness()
		{
			string texture = base.GetTexture(0);
			this.isSolid = this.IsSolidTexture(texture);
			this.isReflective = this.IsReflectiveTexture(texture);
			if (this.go.GetComponent<Collider>() != null)
			{
				this.go.GetComponent<Collider>().isTrigger = !this.isSolid;
			}
			int layer = (!this.isSolid) ? 13 : 4;
			this.go.layer = layer;
			if (this.goT.parent != null)
			{
				this.goT.parent.gameObject.layer = layer;
			}
			if (this.go.GetComponent<Collider>() is BoxCollider)
			{
				BoxCollider boxCollider = (BoxCollider)this.go.GetComponent<Collider>();
				Vector3 scale = base.GetScale();
				boxCollider.size = new Vector3(scale.x * 10f, scale.y, scale.z * 10f);
			}
			this.hasSlowWaves = (texture == "Texture Water Stream" || Blocksworld.renderingWater);
		}

		// Token: 0x06001198 RID: 4504 RVA: 0x00077DF0 File Offset: 0x000761F0
		private void UpdateRenderState()
		{
			if (this.go.activeSelf && this.reflectiveWater != null)
			{
				this.go.GetComponent<MeshRenderer>().enabled = !Blocksworld.renderingWater;
				this.reflectiveWater.SetActive(Blocksworld.renderingWater);
			}
		}

		// Token: 0x06001199 RID: 4505 RVA: 0x00077E48 File Offset: 0x00076248
		public override void Play()
		{
			base.Play();
			if (BlockWater.mainWater != null)
			{
				BWLog.Warning("Possible error - multiple main water objects - this can cause problems with the blocks within list");
			}
			BlockWater.mainWater = this;
			if (BlockWater.blocksWithinWater == null)
			{
				BlockWater.blocksWithinWater = new HashSet<Block>();
			}
			this.buoyancyMultiplier = 1f;
			this.splashInfos.Clear();
			this.playingSplashInfos.Clear();
			BlockWater.blocksWithinWater.Clear();
			this.blockInfos = null;
			this.blockInfoList = null;
			this.waterBounds = this.GetWaterBounds();
			this.IgnoreRaycasts(false);
			this.UpdateSolidness();
			base.FindSplashAudioSources();
			BlockWater.maxWaterLevel = Mathf.Max(-1E+07f, this.waterBounds.max.y);
			this.origPosY = this.goT.position.y;
			CollisionManager.AddIgnoreTriggerGO(this.go);
			this.UpdateRenderState();
		}

		// Token: 0x0600119A RID: 4506 RVA: 0x00077F2D File Offset: 0x0007632D
		public override void Play2()
		{
		}

		// Token: 0x0600119B RID: 4507 RVA: 0x00077F30 File Offset: 0x00076330
		public override void Stop(bool resetBlock = true)
		{
			BlockWater.maxWaterLevel = -1000000f;
			BlockWater.mainWater = null;
			BlockWater.blocksWithinWater = null;
			this.IgnoreRaycasts(false);
			this.go.GetComponent<Collider>().enabled = true;
			this.UpdateSolidness();
			this.blockInfos = null;
			this.blockInfoList = null;
			this.fogColorOverride = Color.white;
			this.fogMultiplier = 1f;
			base.Stop(resetBlock);
			this.UpdateRenderState();
		}

		// Token: 0x0600119C RID: 4508 RVA: 0x00077FA2 File Offset: 0x000763A2
		public override void ResetFrame()
		{
			this.streamVelocity.Set(0f, 0f, 0f);
		}

		// Token: 0x0600119D RID: 4509 RVA: 0x00077FC0 File Offset: 0x000763C0
		public override Bounds GetWaterBounds()
		{
			if (this.go == null || this.go.GetComponent<Collider>() == null)
			{
				BWLog.Warning("GetWaterBounds called on block with no collider");
				return default(Bounds);
			}
			Bounds bounds = this.go.GetComponent<Collider>().bounds;
			if (this.isInfiniteOcean)
			{
				float num = 9999999f;
				bounds.Expand(num);
				bounds.center -= new Vector3(0f, num * 0.5f, 0f);
			}
			return bounds;
		}

		// Token: 0x0600119E RID: 4510 RVA: 0x0007805C File Offset: 0x0007645C
		public override void Update()
		{
			base.Update();
			if (Blocksworld.CurrentState != State.Play)
			{
				this.waterBounds = this.GetWaterBounds();
			}
			bool flag = this.waterBounds.Contains(Blocksworld.cameraPosition);
			if (flag != this.cameraWasWithinWater)
			{
				base.UpdateUnderwaterLightColors(flag);
				if (this.isLava)
				{
					Blocksworld.worldSky.go.GetComponent<Renderer>().enabled = (!flag && !Blocksworld.renderingSkybox);
					Blocksworld.mainCamera.backgroundColor = new Color(1f, 0.5f, 0f);
					Debug.Log("Water set camera background color to " + Blocksworld.mainCamera.backgroundColor);
				}
			}
			bool flag2 = flag && this.isLava;
			this.fogMultiplier = ((!flag2) ? 1f : 0.15f);
			this.fogColorOverride = ((!flag2) ? Color.white : new Color(1f, 0.5f, 0f));
			base.UpdateSounds(this.bubbleSoundWithin && flag);
			this.cameraWasWithinWater = flag;
			BlockAbstractWater.cameraWithinOcean = flag;
		}

		// Token: 0x0600119F RID: 4511 RVA: 0x00078190 File Offset: 0x00076590
		public bool IsSolidTexture(string texture)
		{
			texture = Scarcity.GetNormalizedTexture(texture);
			if (texture != null)
			{
				if (texture == "Water" || texture == "Texture Water Stream" || texture == "Texture Lava")
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x060011A0 RID: 4512 RVA: 0x000781E4 File Offset: 0x000765E4
		public bool IsReflectiveTexture(string texture)
		{
			texture = Scarcity.GetNormalizedTexture(texture);
			if (texture != null)
			{
				if (texture == "Water" || texture == "Texture Water Stream" || texture == "Ice Material")
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060011A1 RID: 4513 RVA: 0x00078238 File Offset: 0x00076638
		public static bool IsWaterTexture(string texture)
		{
			if (texture != null)
			{
				if (BlockWater.f__switch_map4 == null)
				{
					BlockWater.f__switch_map4 = new Dictionary<string, int>(19)
					{
						{
							"Plain",
							0
						},
						{
							"Water",
							0
						},
						{
							"Ice Material",
							0
						},
						{
							"Ice Material_Terrain",
							0
						},
						{
							"Grass",
							0
						},
						{
							"Rubber Material",
							0
						},
						{
							"Rubber Material_Terrain",
							0
						},
						{
							"Rock",
							0
						},
						{
							"Sand",
							0
						},
						{
							"Texture Crater",
							0
						},
						{
							"Texture Crater_Terrain",
							0
						},
						{
							"Yellow Brick Road",
							0
						},
						{
							"Grass 2",
							0
						},
						{
							"Terrain Grid",
							0
						},
						{
							"Road Blank",
							0
						},
						{
							"Texture Water Stream",
							0
						},
						{
							"Texture Lava",
							0
						},
						{
							"Dust",
							0
						},
						{
							"Terrain Planet",
							0
						}
					};
				}
				int num;
				if (BlockWater.f__switch_map4.TryGetValue(texture, out num))
				{
					if (num == 0)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060011A2 RID: 4514 RVA: 0x00078368 File Offset: 0x00076768
		public static bool IsBubblingWithin(string texture)
		{
			if (texture != null)
			{
				if (texture == "Plain" || texture == "Water" || texture == "Texture Water Stream" || texture == "Texture Lava" || texture == "Ice Material" || texture == "Ice Material_Terrain")
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060011A3 RID: 4515 RVA: 0x000783E4 File Offset: 0x000767E4
		public void SnapPosition()
		{
			if (this.doSnap)
			{
				Vector3 position = this.goT.position;
				Vector3 position2 = new Vector3(Mathf.Round(position.x / this.snapper) * this.snapper, position.y, Mathf.Round(position.z / this.snapper) * this.snapper);
				this.goT.position = position2;
			}
		}

		// Token: 0x060011A4 RID: 4516 RVA: 0x00078458 File Offset: 0x00076858
		private void UpdateReflectiveWaterProperties()
		{
			if (this.reflectiveWater != null)
			{
				Material material = this.reflectiveWater.GetComponent<Renderer>().material;
				Color[] array;
				if (Blocksworld.colorDefinitions.TryGetValue(base.currentPaint, out array))
				{
					material.SetColor("_RefrColor", array[0]);
				}
				Vector4 zero = Vector4.zero;
				if (!this.isSolid)
				{
					zero = new Vector4(9f, 4.5f, -8f, -3.5f);
				}
				material.SetVector("WaveSpeed", zero);
			}
		}

		// Token: 0x060011A5 RID: 4517 RVA: 0x000784F0 File Offset: 0x000768F0
		public override TileResultCode PaintTo(string paint, bool permanent, int meshIndex = 0)
		{
			if (meshIndex != 0)
			{
				return TileResultCode.True;
			}
			TileResultCode result = base.PaintTo(paint, permanent, meshIndex);
			if (this.isSolid)
			{
				Material sharedMaterial = this.go.GetComponent<Renderer>().sharedMaterial;
				Color terrainColor = BlockTerrain.GetTerrainColor(paint);
				if (terrainColor != Color.white)
				{
					sharedMaterial.SetColor("_Color", terrainColor);
				}
			}
			this.UpdateReflectiveWaterProperties();
			return result;
		}

		// Token: 0x060011A6 RID: 4518 RVA: 0x00078558 File Offset: 0x00076958
		public override TileResultCode TextureTo(string texture, Vector3 normal, bool permanent, int meshIndex = 0, bool force = false)
		{
			if (meshIndex != 0)
			{
				return TileResultCode.True;
			}
			texture = Scarcity.GetNormalizedTexture(texture);
			this.bubbleSoundWithin = BlockWater.IsBubblingWithin(texture);
			bool flag = BlockWater.IsWaterTexture(texture);
			if (texture == "Plain")
			{
				texture = "Water";
			}
			else if (flag && texture != "Water")
			{
				string normalizedTexture = Scarcity.GetNormalizedTexture(texture);
				if (normalizedTexture != null)
				{
					if (!(normalizedTexture == "Ice Material"))
					{
						if (normalizedTexture == "Rubber Material" || normalizedTexture == "Texture Crater")
						{
							texture += "_Terrain";
						}
					}
					else
					{
						texture = Scarcity.GetNormalizedTexture(texture) + "_Water";
					}
				}
			}
			TileResultCode result = base.TextureTo(texture, normal, permanent, meshIndex, flag || force);
			bool hasSlowWaves = this.hasSlowWaves;
			this.UpdateSolidness();
			this.isLava = (texture == "Texture Lava");
			this.doSnap = (this.isSolid || this.isLava);
			if (this.reflectiveWater != null)
			{
				if (hasSlowWaves != this.hasSlowWaves)
				{
					if (this.hasSlowWaves)
					{
						this.reflectiveWater.GetComponent<Renderer>().material = (Material)Resources.Load("Materials/BWWaterPro");
					}
					else
					{
						this.reflectiveWater.GetComponent<Renderer>().material = (Material)Resources.Load("Materials/BWWaterProStagnant");
					}
				}
				this.UpdateReflectiveWaterProperties();
			}
			if (Blocksworld.CurrentState != State.Play)
			{
				this.go.GetComponent<Collider>().isTrigger = false;
			}
			if (this.bubbleSoundWithin)
			{
				base.GetBubbleClip();
			}
			return result;
		}

		// Token: 0x060011A7 RID: 4519 RVA: 0x00078714 File Offset: 0x00076B14
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (!this.isSolid)
			{
				this.UpdateWaterForces();
			}
		}

		// Token: 0x060011A8 RID: 4520 RVA: 0x00078730 File Offset: 0x00076B30
		protected override void SetWaterLevelOffset(float newOffset)
		{
			base.SetWaterLevelOffset(newOffset);
			Vector3 position = this.goT.position;
			this.goT.position = new Vector3(position.x, this.origPosY + this.waterLevelOffset, position.z);
			this.waterBounds = this.GetWaterBounds();
			BlockWater.maxWaterLevel = this.waterBounds.max.y;
		}

		// Token: 0x060011A9 RID: 4521 RVA: 0x000787A0 File Offset: 0x00076BA0
		private void CreateBlockLists()
		{
			if (this.blockInfos == null)
			{
				BlockAbstractWater.GatherAllBlocksWithWaterSensors();
				this.blockInfos = new Dictionary<int, BlockWaterInfo>();
				this.blockInfoList = new List<BlockWaterInfo>();
				foreach (Block block in BWSceneManager.AllBlocks())
				{
					if (!(block is BlockAbstractWater) && !(block is BlockVolume) && !(block is BlockPosition) && !(block is BlockTerrain) && !(block is BlockSky) && !(block is BlockBillboard))
					{
						BlockWaterInfo blockWaterInfo = new BlockWaterInfo(block);
						this.blockInfos[block.GetInstanceId()] = blockWaterInfo;
						blockWaterInfo.interval = 1;
						blockWaterInfo.isSimulating = true;
						this.blockInfoList.Add(blockWaterInfo);
					}
				}
				this.counter = 0;
			}
		}

		// Token: 0x060011AA RID: 4522 RVA: 0x0007889C File Offset: 0x00076C9C
		private void UpdateWaterForces()
		{
			this.CreateBlockLists();
			float magnitude = Physics.gravity.magnitude;
			bool sfxEnabled = Sound.sfxEnabled;
			float num = Blocksworld.fixedDeltaTime / 0.02f;
			this.counter++;
			Vector3 up = this.goT.up;
			Bounds bounds = default(Bounds);
			for (int i = this.blockInfoList.Count - 1; i >= 0; i--)
			{
				BlockWaterInfo blockWaterInfo = this.blockInfoList[i];
				int interval = blockWaterInfo.interval;
				if ((this.counter + blockWaterInfo.counterOffset) % interval == 0)
				{
					Block block = blockWaterInfo.block;
					GameObject go = block.go;
					int instanceID = go.GetInstanceID();
					Transform goT = block.goT;
					Vector3 vector = goT.position;
					bool flag = block is BlockAnimatedCharacter;
					if (flag)
					{
						vector += Vector3.up;
					}
					float maxExtent = blockWaterInfo.maxExtent;
					float num2 = vector.y - maxExtent - BlockWater.maxWaterLevel;
					if (num2 > 0f)
					{
						BlockWater.blocksWithinWater.Remove(block);
						WaterSplashInfo waterSplashInfo;
						if (this.splashInfos.TryGetValue(instanceID, out waterSplashInfo))
						{
							waterSplashInfo.LeaveWater();
							this.splashInfos.Remove(instanceID);
						}
						int interval2 = Mathf.Min(Mathf.RoundToInt(5f + num2 * 0.2f), 25);
						blockWaterInfo.interval = interval2;
					}
					else
					{
						bool flag2 = vector.y + maxExtent < BlockWater.maxWaterLevel;
						if (flag2)
						{
							bounds.center = vector;
							bounds.size = blockWaterInfo.scale;
						}
						else
						{
							Collider component = go.GetComponent<Collider>();
							if (component != null)
							{
								bounds = component.bounds;
							}
							else
							{
								bounds = block.GetBounds();
							}
						}
						if (flag)
						{
							bounds.center += Vector3.up;
						}
						bounds.center -= Vector3.up * base.WaterLevelOffset(vector);
						blockWaterInfo.checkCount++;
						if (flag2 || this.waterBounds.Intersects(bounds))
						{
							Rigidbody rb = block.chunk.rb;
							if (rb == null)
							{
								if (!blockWaterInfo.hasWaterSensor)
								{
									this.blockInfoList.RemoveAt(i);
									blockWaterInfo.isSimulating = false;
								}
								else
								{
									blockWaterInfo.interval = 5;
								}
								BlockWater.blocksWithinWater.Add(block);
							}
							else if (rb.isKinematic)
							{
								if (!blockWaterInfo.hasWaterSensor)
								{
									this.blockInfoList.RemoveAt(i);
									blockWaterInfo.isSimulating = false;
								}
								else
								{
									blockWaterInfo.interval = 5;
								}
								BlockWater.blocksWithinWater.Add(block);
							}
							else
							{
								blockWaterInfo.interval = 1;
								Vector3 velocity = rb.velocity;
								Vector3 worldCenterOfMass = rb.worldCenterOfMass;
								float num3 = blockWaterInfo.mass;
								Vector3 scale = blockWaterInfo.scale;
								float num4 = scale.x * scale.y * scale.z;
								float num5 = num3 / num4;
								if ((double)num5 < 0.2501)
								{
									num4 *= 0.5f;
								}
								WaterSplashInfo waterSplashInfo2;
								bool flag3 = this.splashInfos.TryGetValue(instanceID, out waterSplashInfo2);
								if (!BlockWater.blocksWithinWater.Contains(block) || !flag3)
								{
									waterSplashInfo2 = new WaterSplashInfo(block);
									this.splashInfos[instanceID] = waterSplashInfo2;
									waterSplashInfo2.EnterWater();
								}
								else
								{
									waterSplashInfo2 = this.splashInfos[instanceID];
								}
								waterSplashInfo2.Update();
								BlockWater.blocksWithinWater.Add(block);
								Bounds bounds2 = bounds;
								float num6;
								if (flag2)
								{
									num6 = num4;
								}
								else
								{
									Vector3 size = bounds.size;
									float num7 = Mathf.Max(0.5f, size.x * size.y * size.z);
									float num8 = num4 / num7;
									bounds2.SetMinMax(Vector3.Max(bounds.min, this.waterBounds.min), Vector3.Min(bounds.max, this.waterBounds.max));
									Vector3 size2 = bounds2.size;
									float num9 = size2.x * size2.y * size2.z;
									num6 = num9 * num8;
								}
								Vector3 a = up * (num6 * magnitude * this.waterDensity);
								Vector3 center = bounds2.center;
								Vector3 rhs = center - worldCenterOfMass;
								Vector3 a2 = velocity + Vector3.Cross(rb.angularVelocity, rhs);
								Vector3 vector2 = a2 - this.streamVelocity;
								Vector3 b = -vector2 * (num6 * this.waterFriction);
								float a3 = (!flag) ? 450f : 0f;
								b = b.normalized * Mathf.Min(a3, b.magnitude);
								Vector3 a4 = a + b + block.GetWaterForce(num6 / num4, vector2, this);
								if (waterSplashInfo2.counter < 10 && sfxEnabled)
								{
									waterSplashInfo2.forceSum += a4.magnitude;
									if (waterSplashInfo2.forceSum > 40f)
									{
										waterSplashInfo2.counter = 1000;
										foreach (GameObject gameObject in BlockAbstractWater.splashAudioSources)
										{
											AudioSource component2 = gameObject.GetComponent<AudioSource>();
											if (!component2.isPlaying)
											{
												gameObject.transform.position = bounds2.max + Vector3.up;
												float volume = Mathf.Clamp((waterSplashInfo2.forceSum - 30f) * 0.01f, 0f, 1f);
												component2.volume = volume;
												component2.Play();
												break;
											}
										}
									}
								}
								rb.AddForceAtPosition(a4 * (block.buoyancyMultiplier * this.buoyancyMultiplier), center, ForceMode.Force);
								float num10 = num6 * -0.6f;
								Vector3 a5 = num10 * this.waterFriction * rb.angularVelocity;
								float a6 = (!flag) ? 100f : 0f;
								a5 = a5.normalized * Mathf.Min(a6, a5.magnitude);
								rb.AddTorque(a5 * block.buoyancyMultiplier, ForceMode.Force);
								if (num * b.magnitude * 0.01f > UnityEngine.Random.value)
								{
									Vector3 size3 = bounds2.size;
									Vector3 position = center - size3 * 0.5f + new Vector3(UnityEngine.Random.value * size3.x, UnityEngine.Random.value * size3.y, UnityEngine.Random.value * size3.y);
									float lifetime = 0.5f + UnityEngine.Random.value * 0.5f;
									this.bubblePs.Emit(position, a2 * 0.1f, 0.1f + UnityEngine.Random.value * 0.15f, lifetime, BlockWater.bubbleColor);
								}
							}
						}
						else
						{
							BlockWater.blocksWithinWater.Remove(block);
							WaterSplashInfo waterSplashInfo3;
							if (this.splashInfos.TryGetValue(instanceID, out waterSplashInfo3))
							{
								waterSplashInfo3.LeaveWater();
								this.splashInfos.Remove(instanceID);
							}
							blockWaterInfo.interval = Mathf.RoundToInt(Mathf.Max(5f + num2 * 0.2f, 2f));
							if (!blockWaterInfo.hasWaterSensor && blockWaterInfo.checkCount == 1)
							{
								Transform parent = goT.parent;
								if (parent != null)
								{
									Rigidbody component3 = parent.GetComponent<Rigidbody>();
									if (component3 == null)
									{
										this.blockInfoList.RemoveAt(i);
										blockWaterInfo.isSimulating = false;
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x060011AB RID: 4523 RVA: 0x000790B8 File Offset: 0x000774B8
		public override void RemovedPlayBlock(Block b)
		{
			this.CreateBlockLists();
			BlockWaterInfo blockWaterInfo;
			if (this.blockInfos.TryGetValue(b.GetInstanceId(), out blockWaterInfo))
			{
				this.blockInfoList.Remove(blockWaterInfo);
				blockWaterInfo.isSimulating = false;
			}
		}

		// Token: 0x060011AC RID: 4524 RVA: 0x000790F8 File Offset: 0x000774F8
		public void AddBlockToSimulation(Block b)
		{
			this.CreateBlockLists();
			int instanceId = b.GetInstanceId();
			BlockWaterInfo blockWaterInfo;
			if (this.blockInfos.TryGetValue(instanceId, out blockWaterInfo))
			{
				if (!blockWaterInfo.isSimulating)
				{
					this.blockInfoList.Add(blockWaterInfo);
				}
				BlockWater.blocksWithinWater.Remove(b);
				this.splashInfos.Remove(instanceId);
				blockWaterInfo.isSimulating = true;
				blockWaterInfo.interval = 1;
			}
		}

		// Token: 0x060011AD RID: 4525 RVA: 0x00079164 File Offset: 0x00077564
		public bool SimulatesBlock(Block b)
		{
			BlockWaterInfo blockWaterInfo;
			return this.blockInfos == null || !this.blockInfos.TryGetValue(b.GetInstanceId(), out blockWaterInfo) || blockWaterInfo.isSimulating;
		}

		// Token: 0x060011AE RID: 4526 RVA: 0x0007919C File Offset: 0x0007759C
		public override bool HasDynamicalLight()
		{
			return true;
		}

		// Token: 0x060011AF RID: 4527 RVA: 0x0007919F File Offset: 0x0007759F
		public override Color GetFogColorOverride()
		{
			return this.fogColorOverride;
		}

		// Token: 0x060011B0 RID: 4528 RVA: 0x000791A7 File Offset: 0x000775A7
		public override float GetFogMultiplier()
		{
			return this.fogMultiplier;
		}

        private static Dictionary<string, int> f__switch_map4;

        // Token: 0x04000DE0 RID: 3552
        private Dictionary<int, BlockWaterInfo> blockInfos;

		// Token: 0x04000DE1 RID: 3553
		private List<BlockWaterInfo> blockInfoList;

		// Token: 0x04000DE2 RID: 3554
		private int counter;

		// Token: 0x04000DE3 RID: 3555
		private float mass;

		// Token: 0x04000DE4 RID: 3556
		protected static BlockWater mainWater = null;

		// Token: 0x04000DE5 RID: 3557
		public static HashSet<Block> blocksWithinWater;

		// Token: 0x04000DE6 RID: 3558
		private static float maxWaterLevel = -1000000f;

		// Token: 0x04000DE7 RID: 3559
		private float origPosY;

		// Token: 0x04000DE8 RID: 3560
		private bool isInfiniteOcean = true;

		// Token: 0x04000DE9 RID: 3561
		private Bounds waterBounds = default(Bounds);

		// Token: 0x04000DEA RID: 3562
		public bool isReflective = true;

		// Token: 0x04000DEB RID: 3563
		private GameObject reflectiveWater;

		// Token: 0x04000DEC RID: 3564
		private float fogMultiplier = 1f;

		// Token: 0x04000DED RID: 3565
		private Color fogColorOverride = Color.white;

		// Token: 0x04000DEE RID: 3566
		private static Color32 bubbleColor = Color.white;

		// Token: 0x04000DEF RID: 3567
		private bool doSnap;

		// Token: 0x04000DF0 RID: 3568
		public float snapper = 80f;
	}
}
