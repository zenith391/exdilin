using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020000A4 RID: 164
	public class BlockMLPLegs : BlockAbstractLegs
	{
		// Token: 0x06000CBC RID: 3260 RVA: 0x000592E8 File Offset: 0x000576E8
		public BlockMLPLegs(List<List<Tile>> tiles, Dictionary<string, string> partNames = null, float ankleYSeparator = 0f) : base(tiles, partNames, 2, new float[]
		{
			0.4f,
			-1f
		}, new int[][]
		{
			new int[]
			{
				0,
				1
			},
			new int[]
			{
				3,
				2
			}
		}, ankleYSeparator, true, 1f, 0.25f)
		{
			this.footOffsetY = 0f;
			this.onGroundHeight = 0.8f;
			this.stepSpeedTrigger = 1f;
			this.maxStepLength = 0.8f;
			this.moveCM = true;
			this.moveCMOffsetFeetCenter = 1f;
			this.resetFeetPositionsOnCreate = true;
		}

		// Token: 0x06000CBD RID: 3261 RVA: 0x0005938C File Offset: 0x0005778C
		public new static void Register()
		{
			PredicateRegistry.Add<BlockMLPLegs>("MLPLegs.Turn", (Block b) => new PredicateSensorDelegate(((BlockAbstractLegs)b).IsTurning), (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).Turn), new Type[]
			{
				typeof(float)
			}, null, null);
			BlockMLPLegs.predicateMLPLegsJump = PredicateRegistry.Add<BlockMLPLegs>("MLPLegs.Jump", (Block b) => new PredicateSensorDelegate(((BlockAbstractLegs)b).IsJumping), (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).Jump), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMLPLegs>("MLPLegs.GotoTag", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).GotoTag), new Type[]
			{
				typeof(string),
				typeof(float),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMLPLegs>("MLPLegs.ChaseTag", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).ChaseTag), new Type[]
			{
				typeof(string),
				typeof(float),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMLPLegs>("MLPLegs.GotoTap", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).GotoTap), new Type[]
			{
				typeof(float),
				typeof(float)
			}, null, null);
			BlockMLPLegs.predicateMLPLegsMover = PredicateRegistry.Add<BlockMLPLegs>("MLPLegs.AnalogStickControl", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).DPadControl), new Type[]
			{
				typeof(string),
				typeof(float),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMLPLegs>("MLPLegs.Translate", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).Translate), new Type[]
			{
				typeof(Vector3),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMLPLegs>("MLPLegs.WackyMode", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).WackyMode), null, null, null);
			PredicateRegistry.Add<BlockMLPLegs>("MLPLegs.TurnTowardsTag", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).TurnTowardsTag), new Type[]
			{
				typeof(string)
			}, null, null);
			PredicateRegistry.Add<BlockMLPLegs>("MLPLegs.TurnTowardsTap", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).TurnTowardsTap), null, null, null);
			PredicateRegistry.Add<BlockMLPLegs>("MLPLegs.TurnAlongCam", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).TurnAlongCam), null, null, null);
			PredicateRegistry.Add<BlockMLPLegs>("MLPLegs.AvoidTag", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).AvoidTag), new Type[]
			{
				typeof(string),
				typeof(float),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMLPLegs>("MLPLegs.Idle", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).Idle), null, null, null);
		}

		// Token: 0x06000CBE RID: 3262 RVA: 0x0005973C File Offset: 0x00057B3C
		public override void Play()
		{
			base.Play();
			BoxCollider boxCollider = (BoxCollider)this.go.GetComponent<Collider>();
			boxCollider.size = new Vector3(1f, 1f, 1.5f);
			boxCollider.center = new Vector3(0f, 0.5f, -0.25f);
			this.HideFeet();
		}

		// Token: 0x06000CBF RID: 3263 RVA: 0x0005979A File Offset: 0x00057B9A
		protected override void PauseAnkles()
		{
		}

		// Token: 0x06000CC0 RID: 3264 RVA: 0x0005979C File Offset: 0x00057B9C
		private void HideFeet()
		{
			for (int i = 0; i < this.feet.Length; i++)
			{
				FootInfo footInfo = this.feet[i];
				footInfo.go.GetComponent<Renderer>().enabled = false;
			}
		}

		// Token: 0x06000CC1 RID: 3265 RVA: 0x000597DC File Offset: 0x00057BDC
		public override void Stop(bool resetBlock = true)
		{
			base.Stop(resetBlock);
			BoxCollider boxCollider = (BoxCollider)this.go.GetComponent<Collider>();
			boxCollider.size = new Vector3(1f, 2f, 1.5f);
			boxCollider.center = new Vector3(0f, 0f, -0.25f);
			this.HideFeet();
		}

		// Token: 0x06000CC2 RID: 3266 RVA: 0x0005983C File Offset: 0x00057C3C
		public override void Break(Vector3 chunkPos, Vector3 chunkVel, Vector3 chunkAngVel)
		{
			this.ResetFeetPositions();
			base.Break(chunkPos, chunkVel, chunkAngVel);
			BoxCollider boxCollider = (BoxCollider)this.go.GetComponent<Collider>();
			boxCollider.size = new Vector3(1f, 2f, 1.5f);
			boxCollider.center = new Vector3(0f, 0f, -0.25f);
		}

		// Token: 0x06000CC3 RID: 3267 RVA: 0x000598A0 File Offset: 0x00057CA0
		public override void FindFeet()
		{
			base.FindFeet();
			for (int i = 0; i < this.feet.Length; i++)
			{
				FootInfo footInfo = this.feet[i];
				Transform transform = footInfo.go.transform;
				footInfo.origLocalPosition = transform.localPosition;
				footInfo.origLocalRotation = transform.localRotation;
			}
			this.HideFeet();
		}

		// Token: 0x06000CC4 RID: 3268 RVA: 0x00059900 File Offset: 0x00057D00
		protected override void ResetFeetPositions()
		{
			bool flag = Blocksworld.CurrentState == State.Play;
			Matrix4x4 localToWorldMatrix = this.goT.localToWorldMatrix;
			for (int i = 0; i < this.feet.Length; i++)
			{
				FootInfo footInfo = this.feet[i];
				Transform transform = footInfo.go.transform;
				if (flag)
				{
					transform.localPosition = Vector3.zero;
					transform.position = localToWorldMatrix.MultiplyPoint(footInfo.origLocalPosition);
				}
				else
				{
					transform.localPosition = footInfo.origLocalPosition;
					transform.localRotation = footInfo.origLocalRotation;
				}
				this.PositionAnkle(i);
			}
			for (int j = 0; j < this.feet.Length; j++)
			{
				FootInfo footInfo2 = this.feet[j];
				footInfo2.position = footInfo2.go.transform.position;
			}
		}

		// Token: 0x04000A10 RID: 2576
		public static Predicate predicateMLPLegsMover;

		// Token: 0x04000A11 RID: 2577
		public static Predicate predicateMLPLegsJump;
	}
}
