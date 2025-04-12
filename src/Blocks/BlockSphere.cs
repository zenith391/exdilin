using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020000D0 RID: 208
	public class BlockSphere : Block
	{
		// Token: 0x06000F85 RID: 3973 RVA: 0x00068419 File Offset: 0x00066819
		public BlockSphere(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000F86 RID: 3974 RVA: 0x0006844C File Offset: 0x0006684C
		public new static void Register()
		{
			BlockSphere.predicateSphereMover = PredicateRegistry.Add<BlockSphere>("Sphere.AnalogStickControl", null, (Block b) => new PredicateActionDelegate(((BlockSphere)b).AnalogStickControl), new Type[]
			{
				typeof(string),
				typeof(float)
			}, new string[]
			{
				"Stick name",
				"Amount"
			}, null);
			BlockSphere.predicateSphereTiltMover = PredicateRegistry.Add<BlockSphere>("Sphere.TiltMover", null, (Block b) => new PredicateActionDelegate(((BlockSphere)b).TiltMoverControl), new Type[]
			{
				typeof(float),
				typeof(int)
			}, new string[]
			{
				"Speed",
				"WorldUp"
			}, null);
			BlockSphere.predicateSphereChase = PredicateRegistry.Add<BlockSphere>("Sphere.MoveThroughTag", null, (Block b) => new PredicateActionDelegate(((BlockSphere)b).MoveThroughTag), new Type[]
			{
				typeof(string),
				typeof(float)
			}, new string[]
			{
				"Tag name",
				"Speed"
			}, null);
			BlockSphere.predicateSphereGoto = PredicateRegistry.Add<BlockSphere>("Sphere.MoveToTag", null, (Block b) => new PredicateActionDelegate(((BlockSphere)b).MoveToTag), new Type[]
			{
				typeof(string),
				typeof(float)
			}, new string[]
			{
				"Tag name",
				"Speed"
			}, null);
			BlockSphere.predicateSphereAvoid = PredicateRegistry.Add<BlockSphere>("Sphere.AvoidTag", null, (Block b) => new PredicateActionDelegate(((BlockSphere)b).AvoidTag), new Type[]
			{
				typeof(string),
				typeof(float),
				typeof(float)
			}, new string[]
			{
				"Tag name",
				"Max Speed",
				"Distance"
			}, null);
			BlockSphere.predicateSphereStay = PredicateRegistry.Add<BlockSphere>("Sphere.Stay", null, (Block b) => new PredicateActionDelegate(((BlockSphere)b).Stay), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Force"
			}, null);
			BlockSphere.predicateSphereJump = PredicateRegistry.Add<BlockSphere>("Sphere.Jump", null, (Block b) => new PredicateActionDelegate(((BlockSphere)b).Jump), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Height"
			}, null);
			Block.AddSimpleDefaultTiles(new GAF(BlockSphere.predicateSphereMover, new object[]
			{
				"L",
				5f
			}), new string[]
			{
				"Sphere"
			});
			Block.AddSimpleDefaultTiles(new GAF(BlockSphere.predicateSphereMover, new object[]
			{
				"L",
				5f
			}), new GAF("Block.PlayVfxDurational", new object[]
			{
				"Sparkle"
			}), new string[]
			{
				"Geodesic Ball"
			});
		}

		// Token: 0x06000F87 RID: 3975 RVA: 0x00068788 File Offset: 0x00066B88
		public TileResultCode AnalogStickControl(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArg = Util.GetStringArg(args, 0, "L");
			float d = Util.GetFloatArg(args, 1, 0f) * eInfo.floatArg;
			Blocksworld.UI.Controls.EnableDPad(stringArg, MoverDirectionMask.ALL);
			Vector3 worldDPadOffset = Blocksworld.UI.Controls.GetWorldDPadOffset(stringArg);
			if (worldDPadOffset.sqrMagnitude > 0.001f)
			{
				this.targetVelocity += d * worldDPadOffset;
				Vector3 normalized = Vector3.Cross(Vector3.up, worldDPadOffset).normalized;
				this.torqueSum += 0.5f * normalized * d * worldDPadOffset.magnitude;
			}
			this.ModifyRb();
			return TileResultCode.True;
		}

		// Token: 0x06000F88 RID: 3976 RVA: 0x0006884C File Offset: 0x00066C4C
		protected override void HandleTiltMover(float xTilt, float yTilt, float zTilt)
		{
			Vector3 vector = Blocksworld.cameraUp - Vector3.Dot(Blocksworld.cameraUp, Vector3.up) * Vector3.up;
			Vector3 cameraRight = Blocksworld.cameraRight;
			Vector3 vector2 = 2f * cameraRight * xTilt + 2f * vector.normalized * yTilt;
			if (vector2.sqrMagnitude > 0.001f)
			{
				this.targetVelocity += vector2;
				Vector3 normalized = Vector3.Cross(Vector3.up, vector2).normalized;
				this.torqueSum += 0.5f * normalized * vector2.magnitude;
			}
			this.ModifyRb();
		}

		// Token: 0x06000F89 RID: 3977 RVA: 0x00068918 File Offset: 0x00066D18
		private void MoveTowardsTag(string tag, float speed, float slowdownDist = 0f)
		{
			Vector3 position = this.goT.position;
			Block block;
			if (TagManager.TryGetClosestBlockWithTag(tag, position, out block, null))
			{
				Vector3 a;
				if (block.size.sqrMagnitude > 4f)
				{
					a = block.go.GetComponent<Collider>().ClosestPointOnBounds(position);
				}
				else
				{
					a = block.goT.position;
				}
				Vector3 vector = a - position;
				vector.y = 0f;
				float num = vector.magnitude - this.size.x * 0.5f;
				if (num > 0.1f)
				{
					Vector3 normalized = vector.normalized;
					float num2 = slowdownDist * 0.2f;
					if (num < num2)
					{
						speed = 0f;
					}
					else if (num < slowdownDist)
					{
						speed *= num / slowdownDist;
					}
					this.targetVelocity += speed * normalized;
					Vector3 normalized2 = Vector3.Cross(Vector3.up, normalized).normalized;
					this.torqueSum += 0.5f * normalized2 * speed;
				}
				this.ModifyRb();
			}
		}

		// Token: 0x06000F8A RID: 3978 RVA: 0x00068A44 File Offset: 0x00066E44
		public TileResultCode AvoidTag(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			Vector3 position = this.goT.position;
			Block block;
			if (TagManager.TryGetClosestBlockWithTag(stringArg, position, out block, null))
			{
				float num = Util.GetFloatArg(args, 1, 5f) * eInfo.floatArg;
				float num2 = Util.GetFloatArg(args, 2, 10f) * eInfo.floatArg;
				Vector3 a;
				if (block.size.sqrMagnitude > 4f)
				{
					a = block.go.GetComponent<Collider>().ClosestPointOnBounds(position);
				}
				else
				{
					a = block.goT.position;
				}
				Vector3 vector = a - position;
				vector.y = 0f;
				float num3 = vector.magnitude - this.size.x * 0.5f;
				if (num3 > 0.001f && num3 <= num2)
				{
					Vector3 normalized = vector.normalized;
					float num4 = Mathf.Clamp(1f - num3 / num2, 0.1f, 1f);
					this.targetVelocity -= num * num4 * normalized;
					Vector3 normalized2 = Vector3.Cross(Vector3.up, normalized).normalized;
					this.torqueSum -= 0.5f * normalized2 * num * num4;
				}
				this.ModifyRb();
			}
			return TileResultCode.True;
		}

		// Token: 0x06000F8B RID: 3979 RVA: 0x00068BAC File Offset: 0x00066FAC
		public TileResultCode MoveToTag(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			float num = Util.GetFloatArg(args, 1, 0f) * eInfo.floatArg;
			this.MoveTowardsTag(stringArg, num, num);
			return TileResultCode.True;
		}

		// Token: 0x06000F8C RID: 3980 RVA: 0x00068BE4 File Offset: 0x00066FE4
		public TileResultCode MoveThroughTag(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			float speed = Util.GetFloatArg(args, 1, 0f) * eInfo.floatArg;
			this.MoveTowardsTag(stringArg, speed, 0f);
			return TileResultCode.True;
		}

		// Token: 0x06000F8D RID: 3981 RVA: 0x00068C20 File Offset: 0x00067020
		public TileResultCode Stay(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float num = Util.GetFloatArg(args, 0, 0f) * eInfo.floatArg;
			this.stayForce += num;
			this.ModifyRb();
			return TileResultCode.True;
		}

		// Token: 0x06000F8E RID: 3982 RVA: 0x00068C56 File Offset: 0x00067056
		public TileResultCode Jump(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.jumpHeight += Util.GetFloatArg(args, 0, 0f) * eInfo.floatArg;
			this.ModifyRb();
			return TileResultCode.True;
		}

		// Token: 0x06000F8F RID: 3983 RVA: 0x00068C80 File Offset: 0x00067080
		public override void Play()
		{
			base.Play();
			this.treatAsVehicleStatus = -1;
			this.torqueSum = Vector3.zero;
			this.targetVelocity = Vector3.zero;
			this.onGround = 0f;
			this.stayForce = 0f;
			this.jumpCounter = 25;
			this.jumpHeight = 0f;
			this.hasModifiedRb = false;
			this.sphereMass = this.GetChunkRigidbody().mass;
		}

		// Token: 0x06000F90 RID: 3984 RVA: 0x00068CF4 File Offset: 0x000670F4
		private void ModifyRb()
		{
			if (!this.hasModifiedRb)
			{
				Rigidbody chunkRigidbody = this.GetChunkRigidbody();
				if (chunkRigidbody != null)
				{
					chunkRigidbody.maxAngularVelocity = 30f;
					chunkRigidbody.angularDrag = 0.01f;
				}
				this.hasModifiedRb = true;
			}
		}

		// Token: 0x06000F91 RID: 3985 RVA: 0x00068D3C File Offset: 0x0006713C
		private Rigidbody GetChunkRigidbody()
		{
			return this.chunk.rb;
		}

		// Token: 0x06000F92 RID: 3986 RVA: 0x00068D4C File Offset: 0x0006714C
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (this.didFix || this.broken)
			{
				return;
			}
			if (this.hasModifiedRb)
			{
				Transform parent = this.goT.parent;
				if (parent != null && CollisionManager.bumping.Contains(parent.gameObject))
				{
					this.onGround = 0.2f;
				}
				else
				{
					this.onGround -= Blocksworld.fixedDeltaTime;
				}
				Rigidbody chunkRigidbody = this.GetChunkRigidbody();
				if (chunkRigidbody != null)
				{
					if (chunkRigidbody.IsSleeping())
					{
						chunkRigidbody.WakeUp();
					}
					Vector3 velocity = chunkRigidbody.velocity;
					if (this.stayForce > 0f)
					{
						this.targetVelocity -= this.stayForce * velocity;
					}
					Vector3 a = this.targetVelocity - velocity;
					Vector3 a2 = Vector3.Cross(Vector3.up, a.normalized) * this.torqueSum.magnitude;
					chunkRigidbody.AddTorque(a2 * chunkRigidbody.mass * Mathf.Min(a.magnitude, 2f));
					if (this.onGround > 0f)
					{
						Vector3 force = a * chunkRigidbody.mass;
						chunkRigidbody.AddForce(force);
					}
					if (this.jumpHeight > 0.01f)
					{
						Vector3 force2 = Vector3.up * Util.GetJumpForcePerFrame(this.jumpHeight, this.sphereMass, 5);
						if (this.jumpCounter < 5)
						{
							chunkRigidbody.AddForce(force2, ForceMode.Force);
						}
						else if (this.jumpCounter - 5 > 20 && this.onGround > 0f)
						{
							this.jumpCounter = 0;
							chunkRigidbody.AddForce(force2, ForceMode.Force);
						}
					}
					this.jumpCounter++;
				}
				this.torqueSum = Vector3.zero;
				this.targetVelocity = Vector3.zero;
				this.stayForce = 0f;
				this.jumpHeight = 0f;
			}
		}

		// Token: 0x06000F93 RID: 3987 RVA: 0x00068F58 File Offset: 0x00067358
		public override bool TreatAsVehicleLikeBlock()
		{
			return base.TreatAsVehicleLikeBlockWithStatus(ref this.treatAsVehicleStatus);
		}

		// Token: 0x04000C0C RID: 3084
		public static Predicate predicateSphereMover;

		// Token: 0x04000C0D RID: 3085
		public static Predicate predicateSphereTiltMover;

		// Token: 0x04000C0E RID: 3086
		public static Predicate predicateSphereGoto;

		// Token: 0x04000C0F RID: 3087
		public static Predicate predicateSphereChase;

		// Token: 0x04000C10 RID: 3088
		public static Predicate predicateSphereStay;

		// Token: 0x04000C11 RID: 3089
		public static Predicate predicateSphereAvoid;

		// Token: 0x04000C12 RID: 3090
		public static Predicate predicateSphereJump;

		// Token: 0x04000C13 RID: 3091
		private Vector3 torqueSum = Vector3.zero;

		// Token: 0x04000C14 RID: 3092
		private Vector3 targetVelocity = Vector3.zero;

		// Token: 0x04000C15 RID: 3093
		private int treatAsVehicleStatus = -1;

		// Token: 0x04000C16 RID: 3094
		private float stayForce;

		// Token: 0x04000C17 RID: 3095
		private bool hasModifiedRb;

		// Token: 0x04000C18 RID: 3096
		private float onGround;

		// Token: 0x04000C19 RID: 3097
		private float sphereMass = 1f;

		// Token: 0x04000C1A RID: 3098
		private float jumpHeight;

		// Token: 0x04000C1B RID: 3099
		private int jumpCounter;

		// Token: 0x04000C1C RID: 3100
		private const int MAX_JUMP_FRAMES = 5;

		// Token: 0x04000C1D RID: 3101
		private const int JUMP_RELOAD_FRAMES = 20;
	}
}
