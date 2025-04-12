using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020000EB RID: 235
	public class BlockVolumeForce : BlockPosition
	{
		// Token: 0x0600114C RID: 4428 RVA: 0x00077650 File Offset: 0x00075A50
		public BlockVolumeForce(List<List<Tile>> tiles) : base(tiles, false)
		{
		}

		// Token: 0x0600114D RID: 4429 RVA: 0x00077698 File Offset: 0x00075A98
		public new static void Register()
		{
			PredicateRegistry.Add<BlockVolumeForce>("VolumeForce.SetWindPower", null, (Block b) => new PredicateActionDelegate(((BlockVolumeForce)b).SetWindPower), new Type[]
			{
				typeof(float)
			}, null, null);
			List<List<Tile>> value = new List<List<Tile>>
			{
				new List<Tile>
				{
					Block.ThenTile(),
					new Tile(new GAF("Block.Fixed", new object[0])),
					new Tile(new GAF("VolumeForce.SetWindPower", new object[]
					{
						10f
					}))
				},
				new List<Tile>
				{
					Block.ThenTile(),
					new Tile(new GAF("Block.PlayVfxDurational", new object[]
					{
						"WindLines"
					}))
				},
				Block.EmptyTileRow()
			};
			Block.defaultExtraTiles["Volume Block Force"] = value;
		}

		// Token: 0x0600114E RID: 4430 RVA: 0x0007779C File Offset: 0x00075B9C
		public override float GetEffectPower()
		{
			return this.windPower;
		}

		// Token: 0x0600114F RID: 4431 RVA: 0x000777A4 File Offset: 0x00075BA4
		public TileResultCode SetWindPower(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float num = (args.Length <= 0) ? 0f : ((float)args[0]);
			this.forceTime = 0f;
			this.windPower = num;
			return TileResultCode.True;
		}

		// Token: 0x06001150 RID: 4432 RVA: 0x000777E0 File Offset: 0x00075BE0
		public override void Play()
		{
			base.Play();
			this.go.layer = 13;
			Collider component = this.go.GetComponent<Collider>();
			component.isTrigger = true;
			this.collisionName = component.name;
			this.colliderSize = component.bounds.size;
		}

		// Token: 0x06001151 RID: 4433 RVA: 0x00077833 File Offset: 0x00075C33
		public override Vector3 GetEffectSize()
		{
			return this.colliderSize;
		}

		// Token: 0x06001152 RID: 4434 RVA: 0x0007783B File Offset: 0x00075C3B
		public override void Stop(bool resetBlock = true)
		{
			base.Stop(resetBlock);
			this.go.GetComponent<Collider>().isTrigger = false;
		}

		// Token: 0x06001153 RID: 4435 RVA: 0x00077858 File Offset: 0x00075C58
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			this.forceTime += Time.deltaTime;
			if (this.forceTime > 0.25f)
			{
				this.windPower = 0f;
			}
			this.windBounds = this.go.GetComponent<Collider>().bounds;
			HashSet<GameObject> hashSet;
			if (CollisionManager.triggering.TryGetValue(this.collisionName, out hashSet))
			{
				foreach (GameObject gameObject in hashSet)
				{
					if (!(gameObject == this.go))
					{
						Transform transform = gameObject.transform;
						Rigidbody component = transform.GetComponent<Rigidbody>();
						if (!(component == null))
						{
							bool isKinematic = component.isKinematic;
							if (!isKinematic)
							{
								IEnumerator enumerator2 = transform.GetEnumerator();
								try
								{
									while (enumerator2.MoveNext())
									{
										object obj = enumerator2.Current;
										Transform transform2 = (Transform)obj;
										GameObject gameObject2 = transform2.gameObject;
										Block block = BWSceneManager.FindBlock(gameObject2, false);
										if (block != null)
										{
											Bounds bounds = gameObject2.GetComponent<Collider>().bounds;
											if (bounds.Intersects(this.windBounds))
											{
												float mass = block.GetMass();
												Vector3 vector = block.Scale();
												float num = vector.x * vector.y * vector.z;
												float num2 = mass / num;
												if ((double)num2 < 0.2501)
												{
													num *= 0.5f;
												}
												Bounds bounds2 = bounds;
												bounds2.SetMinMax(Vector3.Max(bounds.min, this.windBounds.min), Vector3.Min(bounds.max, this.windBounds.max));
												Vector3 force = this.go.transform.forward * this.windPower;
												component.AddForceAtPosition(force, bounds2.center, ForceMode.Force);
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
			}
		}

		// Token: 0x06001154 RID: 4436 RVA: 0x00077AB4 File Offset: 0x00075EB4
		public override bool ColliderIsTriggerInPlayMode()
		{
			return true;
		}

		// Token: 0x04000D8D RID: 3469
		private string collisionName = string.Empty;

		// Token: 0x04000D8E RID: 3470
		private float forceTime;

		// Token: 0x04000D8F RID: 3471
		public float windPower = 10f;

		// Token: 0x04000D90 RID: 3472
		protected const int MAX_SPLASH_SOUNDS = 5;

		// Token: 0x04000D91 RID: 3473
		protected const float FORCE_DELAY = 0.25f;

		// Token: 0x04000D92 RID: 3474
		protected Vector3 colliderSize = Vector3.one;

		// Token: 0x04000D93 RID: 3475
		private Bounds windBounds = default(Bounds);
	}
}
