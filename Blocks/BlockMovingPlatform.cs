using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020000B6 RID: 182
	public class BlockMovingPlatform : BlockAbstractMovingPlatform, ITreasureAnimationDriver
	{
		// Token: 0x06000E1D RID: 3613 RVA: 0x0005FA2F File Offset: 0x0005DE2F
		public BlockMovingPlatform(List<List<Tile>> tiles) : base(tiles)
		{
			this.controlsVelocity = true;
		}

		// Token: 0x06000E1E RID: 3614 RVA: 0x0005FA40 File Offset: 0x0005DE40
		public new static void Register()
		{
			PredicateRegistry.Add<BlockMovingPlatform>("MovingPlatform.MoveTo", null, (Block b) => new PredicateActionDelegate(((BlockMovingPlatform)b).MoveTo), new Type[]
			{
				typeof(int),
				typeof(float)
			}, new string[]
			{
				"0: A, 1: B",
				"Speed"
			}, null);
			PredicateRegistry.Add<BlockMovingPlatform>("MovingPlatform.MoveTowards", null, (Block b) => new PredicateActionDelegate(((BlockMovingPlatform)b).MoveTowards), new Type[]
			{
				typeof(int),
				typeof(float)
			}, new string[]
			{
				"0: A, 1: B",
				"Speed"
			}, null);
			PredicateRegistry.Add<BlockMovingPlatform>("MovingPlatform.StepTowards", null, (Block b) => new PredicateActionDelegate(((BlockMovingPlatform)b).StepTowards), new Type[]
			{
				typeof(int),
				typeof(float),
				typeof(float)
			}, new string[]
			{
				"0: A, 1: B",
				"Units",
				"Speed"
			}, null);
			PredicateRegistry.Add<BlockMovingPlatform>("MovingPlatform.AtPosition", (Block b) => new PredicateSensorDelegate(((BlockMovingPlatform)b).AtPosition), null, new Type[]
			{
				typeof(int)
			}, new string[]
			{
				"0: A, 1: B"
			}, null);
			PredicateRegistry.Add<BlockMovingPlatform>("MovingPlatform.FreeSlide", null, (Block b) => new PredicateActionDelegate(((BlockMovingPlatform)b).FreeSlide), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Mass multiplier"
			}, null);
			Block.AddSimpleDefaultTiles(new List<GAF>
			{
				new GAF("MovingPlatform.MoveTo", new object[]
				{
					1,
					2f
				}),
				new GAF("MovingPlatform.MoveTo", new object[]
				{
					0,
					2f
				})
			}, new string[]
			{
				"Moving Platform"
			});
		}

		// Token: 0x06000E1F RID: 3615 RVA: 0x0005FC8A File Offset: 0x0005E08A
		public TileResultCode FreeSlide(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.enabled && !this.broken)
			{
				this.slideFree = true;
				this.massMultiplier = Util.GetFloatArg(args, 0, 1f);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000E20 RID: 3616 RVA: 0x0005FCBC File Offset: 0x0005E0BC
		protected int GetSteps(float positionInc)
		{
			return Mathf.RoundToInt(positionInc * 100000f);
		}

		// Token: 0x06000E21 RID: 3617 RVA: 0x0005FCCC File Offset: 0x0005E0CC
		public Vector3 GetPositionOffset()
		{
			Vector3 vector = this.positions[1] - this.positions[0];
			if (vector.sqrMagnitude > 0.1f)
			{
				Vector3 normalized = vector.normalized;
				return normalized * this.positionOffset;
			}
			return Vector3.zero;
		}

		// Token: 0x06000E22 RID: 3618 RVA: 0x0005FD2C File Offset: 0x0005E12C
		public TileResultCode StepTowards(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.enabled && !this.broken)
			{
				int intArg = Util.GetIntArg(args, 0, 0);
				float floatArg = Util.GetFloatArg(args, 1, 2f);
				float num = Mathf.Max(0.001f, Util.GetFloatArg(args, 2, 2f) * eInfo.floatArg);
				float num2 = floatArg / num;
				float magnitude = (this.positions[1] - this.positions[0]).magnitude;
				float num3 = 0f;
				if (num2 > 0f && eInfo.timer + Blocksworld.fixedDeltaTime > num2)
				{
					num3 = eInfo.timer + Blocksworld.fixedDeltaTime - num2;
				}
				float num4 = num * Mathf.Max(Blocksworld.fixedDeltaTime - num3, 0f);
				if (intArg == 0)
				{
					num4 *= -1f;
				}
				this.targetSteps = Mathf.Clamp(this.targetSteps + this.GetSteps(num4), 0, this.GetSteps(magnitude));
				this.positionOffset = (float)this.targetSteps / 100000f;
				this.slideFree = false;
				return (eInfo.timer + Blocksworld.fixedDeltaTime < num2) ? TileResultCode.Delayed : TileResultCode.True;
			}
			return TileResultCode.True;
		}

		// Token: 0x06000E23 RID: 3619 RVA: 0x0005FE6C File Offset: 0x0005E26C
		public TileResultCode MoveTo(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.enabled && !this.broken)
			{
				int intArg = Util.GetIntArg(args, 0, 0);
				float num = Mathf.Max(0.001f, Util.GetFloatArg(args, 1, 5f) * eInfo.floatArg);
				float magnitude = (this.positions[1] - this.positions[0]).magnitude;
				float num2 = (intArg != 0) ? (magnitude - this.positionOffset) : this.positionOffset;
				float num3 = num2 / num;
				float num4 = num * Blocksworld.fixedDeltaTime;
				if (intArg == 0)
				{
					num4 *= -1f;
				}
				this.positionOffset = Mathf.Clamp(this.positionOffset + num4, 0f, magnitude);
				this.targetSteps = this.GetSteps(this.positionOffset);
				this.slideFree = false;
				return (num3 <= 0f) ? TileResultCode.True : TileResultCode.Delayed;
			}
			return TileResultCode.True;
		}

		// Token: 0x06000E24 RID: 3620 RVA: 0x0005FF68 File Offset: 0x0005E368
		public TileResultCode AtPosition(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.enabled && !this.broken)
			{
				int intArg = Util.GetIntArg(args, 0, 0);
				float magnitude = (this.positions[1] - this.positions[0]).magnitude;
				int num = (intArg != 0) ? this.GetSteps(magnitude) : 0;
				this.slideFree = false;
				int value = num - this.targetSteps;
				return ((float)Mathf.Abs(value) >= 10000f) ? TileResultCode.False : TileResultCode.True;
			}
			return TileResultCode.True;
		}

		// Token: 0x06000E25 RID: 3621 RVA: 0x00060004 File Offset: 0x0005E404
		public TileResultCode MoveTowards(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.enabled && !this.broken)
			{
				int intArg = Util.GetIntArg(args, 0, 0);
				float num = Mathf.Max(0.001f, Util.GetFloatArg(args, 1, 5f) * eInfo.floatArg);
				float magnitude = (this.positions[1] - this.positions[0]).magnitude;
				float num2 = num * Blocksworld.fixedDeltaTime;
				if (intArg == 0)
				{
					num2 *= -1f;
				}
				this.positionOffset = Mathf.Clamp(this.positionOffset + num2, 0f, magnitude);
				this.targetSteps = this.GetSteps(this.positionOffset);
				this.maxSpeed = Mathf.Max(this.maxSpeed, Mathf.Abs(num));
				this.slideFree = false;
			}
			return TileResultCode.True;
		}

		// Token: 0x06000E26 RID: 3622 RVA: 0x000600E4 File Offset: 0x0005E4E4
		private void ControlPlatform()
		{
			Vector3 vector = this.positions[1] - this.positions[0];
			Vector3 normalized = vector.normalized;
			Vector3 vector2 = this.goT.position - this.origPosition;
			Vector3 vector3 = vector2 + this.positions[0];
			Vector3 a = vector3;
			bool flag = true;
			if (this.slideFree)
			{
				float num = Vector3.Dot(normalized, vector2);
				if (num < 0f)
				{
					a = this.positions[0];
				}
				else if (num > vector.magnitude)
				{
					a = this.positions[1];
				}
				else
				{
					flag = false;
				}
				this.positionOffset = Mathf.Clamp(num, 0f, vector.magnitude);
				this.targetSteps = this.GetSteps(this.positionOffset);
			}
			else
			{
				a = this.positions[0] + normalized * this.positionOffset;
			}
			if (flag)
			{
				Vector3 vector4 = a - vector3;
				float num2 = Mathf.Abs(this.positionOffset - this.lastPositionOffset);
				if (this.chunkRigidBody != null && !this.chunkRigidBody.isKinematic)
				{
					float num3 = Mathf.Max(num2 * 10f, 0.5f);
					if (vector4.sqrMagnitude > num3 * num3)
					{
						if (CollisionManager.bumpedObject.Overlaps(this.chunk.blocks))
						{
							Vector3 velocity = vector4;
							if (velocity.sqrMagnitude > 1f)
							{
								velocity.Normalize();
							}
							this.chunkRigidBody.velocity = velocity;
						}
						else
						{
							this.chunkRigidBody.velocity = vector4 * 10f;
						}
					}
					else
					{
						this.chunkRigidBody.velocity = vector4 * 10f;
					}
				}
			}
			this.lastTargetSteps = this.targetSteps;
			this.lastPositionOffset = this.positionOffset;
		}

		// Token: 0x06000E27 RID: 3623 RVA: 0x00060309 File Offset: 0x0005E709
		public override void Play2()
		{
			base.Play2();
			if (this.enabled)
			{
				this.hasFrozenPosition = false;
				this.UpdateFreezePosition(true);
				this.origPosition = this.goT.position;
			}
		}

		// Token: 0x06000E28 RID: 3624 RVA: 0x0006033C File Offset: 0x0005E73C
		private void UpdateFreezePosition(bool freezePosition)
		{
			if (freezePosition != this.hasFrozenPosition)
			{
				RigidbodyConstraints rigidbodyConstraints = RigidbodyConstraints.FreezeRotation;
				if (freezePosition)
				{
					Vector3 vector = this.positions[1] - this.positions[0];
					if (Mathf.Abs(vector.x) < 0.01f)
					{
						rigidbodyConstraints |= RigidbodyConstraints.FreezePositionX;
					}
					if (Mathf.Abs(vector.y) < 0.01f)
					{
						rigidbodyConstraints |= RigidbodyConstraints.FreezePositionY;
					}
					if (Mathf.Abs(vector.z) < 0.01f)
					{
						rigidbodyConstraints |= RigidbodyConstraints.FreezePositionZ;
					}
				}
				this.chunkRigidBody.constraints = rigidbodyConstraints;
				this.hasFrozenPosition = freezePosition;
			}
		}

		// Token: 0x06000E29 RID: 3625 RVA: 0x000603E8 File Offset: 0x0005E7E8
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (this.enabled && !this.broken)
			{
				if (this.didSlideFree != this.slideFree)
				{
					this.UpdateFreezePosition(this.slideFree);
				}
				this.ControlPlatform();
				this.didSlideFree = this.slideFree;
				this.slideFree = false;
			}
		}

		// Token: 0x06000E2A RID: 3626 RVA: 0x00060447 File Offset: 0x0005E847
		public Vector3 GetTreasurePositionOffset(TreasureHandler.TreasureState state)
		{
			return this.GetPositionOffset();
		}

		// Token: 0x06000E2B RID: 3627 RVA: 0x0006044F File Offset: 0x0005E84F
		public Quaternion GetTreasureRotation(TreasureHandler.TreasureState state)
		{
			return Quaternion.identity;
		}

		// Token: 0x06000E2C RID: 3628 RVA: 0x00060456 File Offset: 0x0005E856
		public bool TreasureAnimationActivated()
		{
			return this.enabled;
		}

		// Token: 0x06000E2D RID: 3629 RVA: 0x0006045E File Offset: 0x0005E85E
		protected override void Vanishing(float scale)
		{
			base.SetChildrenLocalScale(scale);
			this.UpdateFreezePosition(this.didSlideFree);
			this.SetChunkPosition(scale);
		}

		// Token: 0x06000E2E RID: 3630 RVA: 0x0006047A File Offset: 0x0005E87A
		protected override void Appearing(float scale)
		{
			base.SetChildrenLocalScale(scale);
			this.UpdateFreezePosition(this.didSlideFree);
			this.SetChunkPosition(scale);
		}

		// Token: 0x06000E2F RID: 3631 RVA: 0x00060498 File Offset: 0x0005E898
		protected void SetChunkPosition(float scale = 1f)
		{
			if (this.enabled && !this.didFix && !this.isTreasure && !this.broken)
			{
				Vector3 vector = this.positions[1] - this.positions[0];
				if (vector.sqrMagnitude < 0.01f)
				{
					return;
				}
				Vector3 a = this.goT.position - this.origPosition;
				Vector3 b = a + this.positions[0];
				Vector3 normalized = vector.normalized;
				Vector3 a2 = this.positions[0] + normalized * this.positionOffset;
				Vector3 b2 = a2 - b;
				Transform transform = this.chunk.go.transform;
				Vector3 position = transform.position + b2;
				transform.position = position;
			}
		}

		// Token: 0x06000E30 RID: 3632 RVA: 0x0006059B File Offset: 0x0005E99B
		public override void Appeared()
		{
			base.Appeared();
			base.SetChildrenLocalScale(1f);
			this.SetChunkPosition(1f);
			this.UpdateFreezePosition(true);
		}

		// Token: 0x06000E31 RID: 3633 RVA: 0x000605C0 File Offset: 0x0005E9C0
		protected override void ModelBlockAppearing(float scale)
		{
			this.SetChunkPosition(scale);
		}

		// Token: 0x06000E32 RID: 3634 RVA: 0x000605C9 File Offset: 0x0005E9C9
		public override void Stop(bool resetBlock = true)
		{
			base.Stop(resetBlock);
			base.SetChildrenLocalScale(1f);
		}

		// Token: 0x06000E33 RID: 3635 RVA: 0x000605DD File Offset: 0x0005E9DD
		public override void ChunkInModelFrozen()
		{
			base.ChunkInModelFrozen();
			this.enabled = false;
		}

		// Token: 0x06000E34 RID: 3636 RVA: 0x000605EC File Offset: 0x0005E9EC
		public override void ChunkInModelUnfrozen()
		{
			base.ChunkInModelUnfrozen();
			foreach (Block block in Block.connectedCache[this])
			{
				if (block.didFix)
				{
					return;
				}
			}
			this.enabled = true;
		}

		// Token: 0x04000B0C RID: 2828
		private Vector3 origPosition;

		// Token: 0x04000B0D RID: 2829
		private const float STEPS_PER_UNIT = 100000f;

		// Token: 0x04000B0E RID: 2830
		private bool hasFrozenPosition;
	}
}
