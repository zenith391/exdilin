using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020000A3 RID: 163
	public class BlockLegs : BlockAbstractLegs
	{
		// Token: 0x06000CA9 RID: 3241 RVA: 0x00058D48 File Offset: 0x00057148
		public BlockLegs(List<List<Tile>> tiles, Dictionary<string, string> partNames, float ankleYSeparator = 0f, float torqueMult = 1f, float footWidth = 0.25f) : base(tiles, partNames, 1, null, null, ankleYSeparator, false, torqueMult, footWidth)
		{
			this.maxStepHeight = 0.85f;
			this.maxStepLength = 1f;
			this.footOffsetY = -0.25f;
			this.resetFeetPositionsOnCreate = true;
		}

		// Token: 0x06000CAA RID: 3242 RVA: 0x00058D9C File Offset: 0x0005719C
		public new static void Register()
		{
			PredicateRegistry.Add<BlockLegs>("Legs.Walk", (Block b) => new PredicateSensorDelegate(((BlockAbstractLegs)b).IsWalkingSensor), (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).Walk), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockLegs>("Legs.Turn", (Block b) => new PredicateSensorDelegate(((BlockAbstractLegs)b).IsTurning), (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).Turn), new Type[]
			{
				typeof(float)
			}, null, null);
			BlockLegs.predicateLegsJump = PredicateRegistry.Add<BlockLegs>("Legs.Jump", (Block b) => new PredicateSensorDelegate(((BlockAbstractLegs)b).IsJumping), (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).Jump), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockLegs>("Legs.GotoTag", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).GotoTag), new Type[]
			{
				typeof(string),
				typeof(float),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockLegs>("Legs.ChaseTag", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).ChaseTag), new Type[]
			{
				typeof(string),
				typeof(float),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockLegs>("Legs.GotoTap", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).GotoTap), new Type[]
			{
				typeof(float),
				typeof(float)
			}, null, null);
			BlockLegs.predicateLegsMover = PredicateRegistry.Add<BlockLegs>("Legs.AnalogStickControl", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).DPadControl), new Type[]
			{
				typeof(string),
				typeof(float),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockLegs>("Legs.WackyMode", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).WackyMode), null, null, null);
			PredicateRegistry.Add<BlockLegs>("Legs.Translate", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).Translate), new Type[]
			{
				typeof(Vector3),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockLegs>("Legs.TurnTowardsTag", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).TurnTowardsTag), new Type[]
			{
				typeof(string)
			}, null, null);
			PredicateRegistry.Add<BlockLegs>("Legs.TurnTowardsTap", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).TurnTowardsTap), null, null, null);
			PredicateRegistry.Add<BlockLegs>("Legs.TurnAlongCam", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).TurnAlongCam), null, null, null);
			PredicateRegistry.Add<BlockLegs>("Legs.AvoidTag", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).AvoidTag), new Type[]
			{
				typeof(string),
				typeof(float),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockLegs>("Legs.Idle", null, (Block b) => new PredicateActionDelegate(((BlockAbstractLegs)b).Idle), null, null, null);
		}

		// Token: 0x040009FD RID: 2557
		public static Predicate predicateLegsJump;

		// Token: 0x040009FE RID: 2558
		public static Predicate predicateLegsMover;
	}
}
