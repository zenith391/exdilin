using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x020000D1 RID: 209
	public class BlockSpokedWheel : BlockAbstractWheel
	{
		// Token: 0x06000F9B RID: 3995 RVA: 0x00068FEB File Offset: 0x000673EB
		public BlockSpokedWheel(List<List<Tile>> tiles) : base(tiles, "Spoked Wheel Axle", string.Empty)
		{
		}

		// Token: 0x06000F9C RID: 3996 RVA: 0x00069000 File Offset: 0x00067400
		public new static void Register()
		{
			BlockSpokedWheel.predicateSpokedWheelDrive = PredicateRegistry.Add<BlockSpokedWheel>("SpokedWheel.Drive", (Block b) => new PredicateSensorDelegate(((BlockAbstractWheel)b).IsDrivingSensor), (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).Drive), new Type[]
			{
				typeof(float)
			}, null, null);
			BlockSpokedWheel.predicateSpokedWheelTurn = PredicateRegistry.Add<BlockSpokedWheel>("SpokedWheel.Turn", (Block b) => new PredicateSensorDelegate(((BlockAbstractWheel)b).IsTurning), (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).Turn), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockSpokedWheel>("SpokedWheel.TurnTowardsTag", null, (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).TurnTowardsTag), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockSpokedWheel>("SpokedWheel.DriveTowardsTag", null, (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).DriveTowardsTag), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockSpokedWheel>("SpokedWheel.DriveTowardsTagRaw", null, (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).DriveTowardsTagRaw), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockSpokedWheel>("SpokedWheel.IsWheelTowardsTag", (Block b) => new PredicateSensorDelegate(((BlockAbstractWheel)b).IsWheelTowardsTag), null, new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockSpokedWheel>("SpokedWheel.IsDPadAlongWheel", (Block b) => new PredicateSensorDelegate(((BlockAbstractWheel)b).IsDPadAlongWheel), null, new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockSpokedWheel>("SpokedWheel.TurnAlongDPad", null, (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).TurnAlongDPad), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockSpokedWheel>("SpokedWheel.DriveAlongDPad", null, (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).DriveAlongDPad), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockSpokedWheel>("SpokedWheel.DriveAlongDPadRaw", null, (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).DriveAlongDPadRaw), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockSpokedWheel>("SpokedWheel.SetAsSpareTire", null, (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).SetAsSpareTire), null, null, null);
		}

		// Token: 0x04000C25 RID: 3109
		public static Predicate predicateSpokedWheelDrive;

		// Token: 0x04000C26 RID: 3110
		public static Predicate predicateSpokedWheelTurn;
	}
}
