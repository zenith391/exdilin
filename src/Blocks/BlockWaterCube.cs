using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020000EF RID: 239
	public class BlockWaterCube : BlockAbstractWater
	{
		// Token: 0x060011B3 RID: 4531 RVA: 0x00079234 File Offset: 0x00077634
		public BlockWaterCube(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x060011B4 RID: 4532 RVA: 0x000792C8 File Offset: 0x000776C8
		public new static void Register()
		{
			PredicateRegistry.Add<BlockWaterCube>("WaterBlock.IncreaseWaterLevel", null, (Block b) => new PredicateActionDelegate(((BlockWaterCube)b).IncreaseWaterLevelOffset), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockWaterCube>("WaterBlock.StepIncreaseWaterLevel", null, (Block b) => new PredicateActionDelegate(((BlockWaterCube)b).StepIncreaseWaterLevel), new Type[]
			{
				typeof(float),
				typeof(float)
			}, new string[]
			{
				"Units",
				"Duration"
			}, null);
			PredicateRegistry.Add<BlockWaterCube>("WaterBlock.SetLiquidProperties", null, (Block b) => new PredicateActionDelegate(((BlockAbstractWater)b).SetLiquidProperties), new Type[]
			{
				typeof(string)
			}, null, null);
			PredicateRegistry.Add<BlockWaterCube>("WaterBlock.SetMaxPositiveWaterLevelOffset", (Block b) => new PredicateSensorDelegate(((BlockAbstractWater)b).AtMaxPositiveWaterLevelOffset), (Block b) => new PredicateActionDelegate(((BlockWaterCube)b).SetMaxPositiveWaterLevelOffset), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockWaterCube>("WaterBlock.SetMaxNegativeWaterLevelOffset", (Block b) => new PredicateSensorDelegate(((BlockAbstractWater)b).AtMaxNegativeWaterLevelOffset), (Block b) => new PredicateActionDelegate(((BlockWaterCube)b).SetMaxNegativeWaterLevelOffset), new Type[]
			{
				typeof(float)
			}, null, null);
			List<List<Tile>> value = new List<List<Tile>>
			{
				new List<Tile>
				{
					Block.ThenTile(),
					new Tile(new GAF("WaterBlock.SetLiquidProperties", new object[]
					{
						"Water"
					}))
				},
				Block.EmptyTileRow()
			};
			Block.defaultExtraTiles["Water Cube"] = value;
		}

		// Token: 0x060011B5 RID: 4533 RVA: 0x000794C4 File Offset: 0x000778C4
		public override void Play()
		{
			base.Play();
			this.go.layer = 13;
			this.go.GetComponent<Collider>().isTrigger = true;
			this.blocksWithinWater.Clear();
			this.FindWaterSurfaceMeshVertices();
			this.origScale = base.Scale();
			this.origPos = base.GetPosition();
			this.origWaterLevel = Mathf.Abs(this.goT.TransformDirection(this.origScale).y);
			this.colliderSize = this.size;
			this.colliderLocalOffset = Vector3.zero;
		}

		// Token: 0x060011B6 RID: 4534 RVA: 0x00079559 File Offset: 0x00077959
		public override TileResultCode TextureTo(string texture, Vector3 normal, bool permanent, int meshIndex = 0, bool force = false)
		{
			if (texture != "Water_Block")
			{
				return TileResultCode.False;
			}
			return base.TextureTo(texture, normal, permanent, meshIndex, force);
		}

		// Token: 0x060011B7 RID: 4535 RVA: 0x0007957C File Offset: 0x0007797C
		public TileResultCode IncreaseWaterLevelOffset(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float incPerSecond = eInfo.floatArg * Util.GetFloatArg(args, 0, 0f);
			this.IncreaseWaterLevelOffset(incPerSecond);
			return TileResultCode.True;
		}

		// Token: 0x060011B8 RID: 4536 RVA: 0x000795A8 File Offset: 0x000779A8
		public TileResultCode StepIncreaseWaterLevel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float num = eInfo.floatArg * Util.GetFloatArg(args, 1, 1f);
			if (num < 0.01f)
			{
				num = 1f;
			}
			float incPerSecond = eInfo.floatArg * Util.GetFloatArg(args, 0, 0f) / num;
			this.IncreaseWaterLevelOffset(incPerSecond);
			return (eInfo.timer < num) ? TileResultCode.Delayed : TileResultCode.True;
		}

		// Token: 0x060011B9 RID: 4537 RVA: 0x0007960C File Offset: 0x00077A0C
		protected void IncreaseWaterLevelOffset(float incPerSecond)
		{
			float num = this.origWaterLevel - 0.0449999981f;
			float waterLevelOffset = Mathf.Max(this.waterLevelOffset + incPerSecond * Blocksworld.fixedDeltaTime, -num);
			BlockWaterCube.wakeUpWaterSensorBlocks = true;
			this.SetWaterLevelOffset(waterLevelOffset);
		}

		// Token: 0x060011BA RID: 4538 RVA: 0x0007964C File Offset: 0x00077A4C
		protected override void SetWaterLevelOffset(float amount)
		{
			float num = this.origWaterLevel + this.waterLevelOffset;
			base.SetWaterLevelOffset(amount);
			float num2 = this.origWaterLevel + this.waterLevelOffset;
			if (num2 < 0.05f && num > 0.05f)
			{
				this.go.GetComponent<Renderer>().enabled = false;
			}
			else if (num < 0.05f && num2 > 0.05f)
			{
				this.go.GetComponent<Renderer>().enabled = true;
			}
			if (this.waterMesh == null)
			{
				return;
			}
			Vector3[] vertices = this.waterMesh.vertices;
			Vector3 vector = this.goT.InverseTransformDirection(Vector3.up) * this.waterLevelOffset;
			for (int i = 0; i < this.waterSurfaceMeshVertices.Count; i++)
			{
				int num3 = this.waterSurfaceMeshVertices[i];
				Vector3 vector2 = this.origWaterSurfaceMeshVertices[i];
				vector2 += vector;
				vertices[num3] = vector2;
			}
			this.waterMesh.vertices = vertices;
			BoxCollider component = this.go.GetComponent<BoxCollider>();
			component.size = this.origScale + Util.Abs(vector) * Mathf.Sign(amount);
			component.center = vector * 0.5f;
			this.colliderLocalOffset = Vector3.up * this.waterLevelOffset * 0.5f;
			this.colliderSize = component.size;
		}

		// Token: 0x060011BB RID: 4539 RVA: 0x000797DB File Offset: 0x00077BDB
		public override Vector3 GetEffectLocalOffset()
		{
			return this.colliderLocalOffset;
		}

		// Token: 0x060011BC RID: 4540 RVA: 0x000797E3 File Offset: 0x00077BE3
		public override Vector3 GetEffectSize()
		{
			return this.colliderSize;
		}

		// Token: 0x060011BD RID: 4541 RVA: 0x000797EC File Offset: 0x00077BEC
		private void FindWaterSurfaceMeshVertices()
		{
			this.origWaterSurfaceMeshVertices.Clear();
			this.waterSurfaceMeshVertices.Clear();
			MeshFilter component = this.go.GetComponent<MeshFilter>();
			this.waterMesh = component.sharedMesh;
			Transform goT = this.goT;
			Vector3 position = goT.position;
			Vector3[] vertices = this.waterMesh.vertices;
			for (int i = 0; i < vertices.Length; i++)
			{
				Vector3 vector = vertices[i];
				Vector3 a = goT.TransformPoint(vector);
				if ((a - position).y > 0f)
				{
					this.waterSurfaceMeshVertices.Add(i);
					this.origWaterSurfaceMeshVertices.Add(vector);
				}
			}
		}

		// Token: 0x060011BE RID: 4542 RVA: 0x000798A7 File Offset: 0x00077CA7
		public override void Stop(bool resetBlock = true)
		{
			this.SetWaterLevelOffset(0f);
			base.Stop(resetBlock);
			this.go.layer = 4;
			this.go.GetComponent<Collider>().isTrigger = false;
			this.blocksWithinWater.Clear();
		}

		// Token: 0x060011BF RID: 4543 RVA: 0x000798E4 File Offset: 0x00077CE4
		public override void Update()
		{
			base.Update();
			Vector3 cameraPosition = Blocksworld.cameraPosition;
			bool flag = this.go.GetComponent<Collider>().bounds.Contains(cameraPosition);
			base.UpdateSounds(flag);
			if (flag != this.cameraWasWithinWater)
			{
				base.UpdateUnderwaterLightColors(flag);
			}
			this.cameraWasWithinWater = flag;
		}

		// Token: 0x060011C0 RID: 4544 RVA: 0x00079938 File Offset: 0x00077D38
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			HashSet<int> hashSet = new HashSet<int>(this.blocksWithinWater);
			this.blocksWithinWater.Clear();
			this.blocksWithinTaggedWater.Clear();
			this.waterBounds = this.go.GetComponent<Collider>().bounds;
			HashSet<GameObject> hashSet2;
			if (CollisionManager.triggering.TryGetValue(this.colliderName, out hashSet2))
			{
				float magnitude = Physics.gravity.magnitude;
				float num = Blocksworld.fixedDeltaTime / 0.02f;
				foreach (GameObject gameObject in hashSet2)
				{
					if (!(gameObject == null))
					{
						Transform transform = gameObject.transform;
						Rigidbody component = transform.GetComponent<Rigidbody>();
						if (!(component == null))
						{
							bool isKinematic = component.isKinematic;
							IEnumerator enumerator2 = transform.GetEnumerator();
							try
							{
								while (enumerator2.MoveNext())
								{
									object obj = enumerator2.Current;
									Transform transform2 = (Transform)obj;
									GameObject gameObject2 = transform2.gameObject;
									Block block = BWSceneManager.FindBlock(gameObject2, false);
									if (block == null)
									{
										BWLog.Info("Could not find block " + gameObject2.name);
									}
									else
									{
										Bounds bounds = gameObject2.GetComponent<Collider>().bounds;
										bounds.extents = ((!(bounds.extents == Vector3.zero)) ? bounds.extents : Vector3.one);
										Vector3 position = transform2.position;
										bool flag = block is BlockAnimatedCharacter;
										if (flag)
										{
											bounds.center += Vector3.up;
										}
										if (bounds.Intersects(this.waterBounds))
										{
											Vector3 velocity = component.velocity;
											Vector3 worldCenterOfMass = component.worldCenterOfMass;
											float mass = block.GetMass();
											Vector3 vector = block.Scale();
											float num2 = vector.x * vector.y * vector.z;
											float num3 = mass / num2;
											if ((double)num3 < 0.2501)
											{
												num2 *= 0.5f;
											}
											Bounds bounds2 = bounds;
											Vector3 size = bounds.size;
											float num4 = Mathf.Max(0.5f, size.x * size.y * size.z);
											float num5 = num2 / num4;
											bounds2.SetMinMax(Vector3.Max(bounds.min, this.waterBounds.min), Vector3.Min(bounds.max, this.waterBounds.max));
											Vector3 size2 = bounds2.size;
											float num6 = size2.x * size2.y * size2.z;
											float num7 = num6 * num5;
											Vector3 a = Vector3.up * num7 * magnitude * this.waterDensity;
											Vector3 rhs = bounds2.center - worldCenterOfMass;
											Vector3 a2 = velocity + Vector3.Cross(component.angularVelocity, rhs);
											Vector3 vector2 = a2 - this.streamVelocity;
											Vector3 b = -vector2 * num7 * this.waterFriction;
											Vector3 a3 = a + b + block.GetWaterForce(num7 / num2, vector2, this);
											int instanceId = block.GetInstanceId();
											WaterSplashInfo waterSplashInfo;
											if (!this.splashInfos.ContainsKey(instanceId))
											{
												waterSplashInfo = new WaterSplashInfo(block);
												this.splashInfos.Add(instanceId, waterSplashInfo);
												waterSplashInfo.EnterWater();
											}
											else
											{
												waterSplashInfo = this.splashInfos[instanceId];
											}
											waterSplashInfo.Update();
											waterSplashInfo.forceSum += a3.magnitude;
											this.blocksWithinWater.Add(instanceId);
											if (waterSplashInfo.counter < 10 && waterSplashInfo.forceSum > 40f)
											{
												waterSplashInfo.counter = 1000;
												if (Sound.sfxEnabled)
												{
													this.PlaySplashSound(bounds2.center);
												}
											}
											hashSet.Remove(instanceId);
											if (!isKinematic)
											{
												Vector3 vector3 = a3 * block.buoyancyMultiplier * this.buoyancyMultiplier;
												if (block is BlockAnimatedCharacter)
												{
													vector3 /= 4f;
												}
												component.AddForceAtPosition(vector3, bounds2.center, ForceMode.Force);
												float d = num7 * -0.6f;
												Vector3 a4 = d * component.angularVelocity * this.waterFriction;
												component.AddTorque(a4 * block.buoyancyMultiplier, ForceMode.Force);
												if (num * b.magnitude * 0.01f > UnityEngine.Random.value)
												{
													Vector3 size3 = bounds2.size;
													Vector3 position2 = bounds2.center - size3 * 0.5f + new Vector3(UnityEngine.Random.value * size3.x, UnityEngine.Random.value * size3.y, UnityEngine.Random.value * size3.y);
													float lifetime = 0.5f + UnityEngine.Random.value * 0.5f;
													this.bubblePs.Emit(position2, a2 * 0.1f, 0.1f + UnityEngine.Random.value * 0.15f, lifetime, Color.white);
												}
											}
										}
									}
								}
							}
							finally
							{
								IDisposable disposable;
								if ((disposable = (enumerator2 as IDisposable)) != null)
								{
									disposable.Dispose();
								}
							}
						}
					}
				}
			}
			if (this.blocksWithinWater.Count > 0)
			{
				List<string> blockTags = TagManager.GetBlockTags(this);
				if (blockTags.Count > 0)
				{
					for (int i = 0; i < blockTags.Count; i++)
					{
						string key = blockTags[i];
						this.blocksWithinTaggedWater[key] = this.blocksWithinWater;
					}
				}
			}
			foreach (int key2 in hashSet)
			{
				this.splashInfos.Remove(key2);
			}
			if (BlockWaterCube.wakeUpWaterSensorBlocks)
			{
				HashSet<Chunk> hashSet3 = new HashSet<Chunk>();
				BlockAbstractWater.GatherAllBlocksWithWaterSensors();
				foreach (Block block2 in BlockAbstractWater.blocksWithWaterSensors)
				{
					Chunk chunk = block2.chunk;
					if (!hashSet3.Contains(chunk))
					{
						GameObject go = chunk.go;
						Rigidbody component2 = go.GetComponent<Rigidbody>();
						if (component2 != null)
						{
							component2.WakeUp();
							if (component2.IsSleeping() && component2.isKinematic)
							{
								Transform transform3 = go.transform;
								Vector3 position3 = transform3.position;
								transform3.position = position3;
							}
						}
						hashSet3.Add(chunk);
					}
				}
				BlockWaterCube.wakeUpWaterSensorBlocks = false;
			}
		}

		// Token: 0x060011C1 RID: 4545 RVA: 0x0007A08C File Offset: 0x0007848C
		private void PlaySplashSound(Vector3 pos)
		{
			GameObject gameObject = null;
			AudioSource audioSource = null;
			if (this.audioSourceObjects.Count < 5)
			{
				gameObject = new GameObject(this.go.name + " Splash " + (this.audioSourceObjects.Count + 1));
				audioSource = gameObject.AddComponent<AudioSource>();
				Sound.SetWorldAudioSourceParams(audioSource, 5f, 150f, AudioRolloffMode.Logarithmic);
				this.audioSourceObjects.Add(gameObject);
			}
			if (gameObject == null)
			{
				for (int i = 0; i < this.audioSourceObjects.Count; i++)
				{
					GameObject gameObject2 = this.audioSourceObjects[i];
					AudioSource component = gameObject2.GetComponent<AudioSource>();
					if (!component.isPlaying)
					{
						audioSource = component;
						gameObject = gameObject2;
						break;
					}
				}
			}
			if (gameObject != null)
			{
				int num = UnityEngine.Random.Range(1, 4);
				string name = "Water Splash Medium " + num;
				gameObject.transform.position = pos;
				Sound.PlaySound(name, audioSource, false, 1f, 1f, false);
			}
		}

		// Token: 0x060011C2 RID: 4546 RVA: 0x0007A19C File Offset: 0x0007859C
		public override void Destroy()
		{
			base.Destroy();
			for (int i = 0; i < this.audioSourceObjects.Count; i++)
			{
				UnityEngine.Object.Destroy(this.audioSourceObjects[i]);
			}
			this.audioSourceObjects.Clear();
		}

		// Token: 0x060011C3 RID: 4547 RVA: 0x0007A1E8 File Offset: 0x000785E8
		public override TileResultCode PaintToAction(ScriptRowExecutionInfo eInfo, object[] args)
		{
			bool flag = false;
			int intArg = Util.GetIntArg(args, 1, 0);
			if (intArg == 0)
			{
				string stringArg = Util.GetStringArg(args, 0, string.Empty);
				string paint = this.GetPaint(intArg);
				if (paint != stringArg)
				{
					flag = true;
				}
			}
			TileResultCode result = base.PaintToAction(eInfo, args);
			if (flag)
			{
				base.UpdateUnderwaterLightColors(this.cameraWasWithinWater);
			}
			return result;
		}

		// Token: 0x060011C4 RID: 4548 RVA: 0x0007A247 File Offset: 0x00078647
		public override bool ColliderIsTriggerInPlayMode()
		{
			return true;
		}

		// Token: 0x04000DFB RID: 3579
		public HashSet<int> blocksWithinWater = new HashSet<int>();

		// Token: 0x04000DFC RID: 3580
		public Dictionary<string, HashSet<int>> blocksWithinTaggedWater = new Dictionary<string, HashSet<int>>();

		// Token: 0x04000DFD RID: 3581
		protected Mesh waterMesh;

		// Token: 0x04000DFE RID: 3582
		protected List<int> waterSurfaceMeshVertices = new List<int>();

		// Token: 0x04000DFF RID: 3583
		protected List<Vector3> origWaterSurfaceMeshVertices = new List<Vector3>();

		// Token: 0x04000E00 RID: 3584
		protected Vector3 origScale = Vector3.one;

		// Token: 0x04000E01 RID: 3585
		protected Vector3 origPos = Vector3.zero;

		// Token: 0x04000E02 RID: 3586
		protected float origWaterLevel = 1f;

		// Token: 0x04000E03 RID: 3587
		protected const int MAX_SPLASH_SOUNDS = 5;

		// Token: 0x04000E04 RID: 3588
		protected Vector3 colliderSize = Vector3.one;

		// Token: 0x04000E05 RID: 3589
		protected Vector3 colliderLocalOffset = Vector3.zero;

		// Token: 0x04000E06 RID: 3590
		private static bool wakeUpWaterSensorBlocks;

		// Token: 0x04000E07 RID: 3591
		private Bounds waterBounds = default(Bounds);

		// Token: 0x04000E08 RID: 3592
		private const float OFF_THRESHOLD = 0.05f;

		// Token: 0x04000E09 RID: 3593
		private List<GameObject> audioSourceObjects = new List<GameObject>();
	}
}
