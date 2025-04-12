using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020000C0 RID: 192
	public class BlockQuadped : BlockAbstractLegs
	{
        private static int legPairCount_ = 2;
        private static float[] legPairOffsets_ = new float[]
        {
                0.35f,
                -0.35f
        };
        private static int[][] legPairIndices_ = new int[][]
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
        };
        // Token: 0x06000EC4 RID: 3780 RVA: 0x00063704 File Offset: 0x00061B04
        public BlockQuadped(List<List<Tile>> tiles, Dictionary<string, string> partNames = null, float ankleYSeparator = 0f, float footWidth = 0.25f) :
            base(tiles, partNames, legPairCount_, legPairOffsets_, legPairIndices_, ankleYSeparator, false, 1f, footWidth)
		{
			this.maxStepHeight = 0.85f;
			this.maxStepLength = 1f;
			this.stepLengthMultiplier = 0.7f;
			this.stepTimeMultiplier = 0.5f;
			this.footOffsetY = -0.35f;
		}

		// Token: 0x06000EC5 RID: 3781 RVA: 0x000637A8 File Offset: 0x00061BA8
		public new static void Register()
		{
			PredicateRegistry.Add<BlockQuadped>("Quadped.Turn", (Block b) => new PredicateSensorDelegate(((BlockAbstractLegs)b).IsTurning), (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).Turn), new Type[]
			{
				typeof(float)
			}, null, null);
			BlockQuadped.predicateQuadpedJump = PredicateRegistry.Add<BlockQuadped>("Quadped.Jump", (Block b) => new PredicateSensorDelegate(((BlockAbstractLegs)b).IsJumping), (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).Jump), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockQuadped>("Quadped.GotoTag", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).GotoTag), new Type[]
			{
				typeof(string),
				typeof(float),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockQuadped>("Quadped.ChaseTag", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).ChaseTag), new Type[]
			{
				typeof(string),
				typeof(float),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockQuadped>("Quadped.GotoTap", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).GotoTap), new Type[]
			{
				typeof(float),
				typeof(float)
			}, null, null);
			BlockQuadped.predicateQuadpedMover = PredicateRegistry.Add<BlockQuadped>("Quadped.AnalogStickControl", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).DPadControl), new Type[]
			{
				typeof(string),
				typeof(float),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockQuadped>("Quadped.WackyMode", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).WackyMode), null, null, null);
			PredicateRegistry.Add<BlockQuadped>("Quadped.Translate", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).Translate), new Type[]
			{
				typeof(Vector3),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockQuadped>("Quadped.TurnTowardsTag", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).TurnTowardsTag), new Type[]
			{
				typeof(string)
			}, null, null);
			PredicateRegistry.Add<BlockQuadped>("Quadped.TurnTowardsTap", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).TurnTowardsTap), null, null, null);
			PredicateRegistry.Add<BlockQuadped>("Quadped.TurnAlongCam", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).TurnAlongCam), null, null, null);
			PredicateRegistry.Add<BlockQuadped>("Quadped.AvoidTag", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).AvoidTag), new Type[]
			{
				typeof(string),
				typeof(float),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockQuadped>("Quadped.Idle", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).Idle), null, null, null);
		}

		// Token: 0x04000B68 RID: 2920
		public static Predicate predicateQuadpedMover;

		// Token: 0x04000B69 RID: 2921
		public static Predicate predicateQuadpedJump;
	}
}
