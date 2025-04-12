using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x020000F1 RID: 241
	public class BlockWheelBling : BlockAbstractWheel
	{
		// Token: 0x060011CE RID: 4558 RVA: 0x0007A2E0 File Offset: 0x000786E0
		public BlockWheelBling(List<List<Tile>> tiles) : base(tiles, "Golden Wheel Axle", string.Empty)
		{
		}

		// Token: 0x060011CF RID: 4559 RVA: 0x0007A2F4 File Offset: 0x000786F4
		public new static void Register()
		{
			PredicateRegistry.Add<BlockWheelBling>("GoldenWheel.Drive", (Block b) => new PredicateSensorDelegate(((BlockAbstractWheel)b).IsDrivingSensor), (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).Drive), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockWheelBling>("GoldenWheel.Turn", (Block b) => new PredicateSensorDelegate(((BlockAbstractWheel)b).IsTurning), (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).Turn), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockWheelBling>("GoldenWheel.TurnTowardsTag", null, (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).TurnTowardsTag), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockWheelBling>("GoldenWheel.DriveTowardsTag", null, (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).DriveTowardsTag), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockWheelBling>("GoldenWheel.DriveTowardsTagRaw", null, (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).DriveTowardsTagRaw), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockWheelBling>("GoldenWheel.IsWheelTowardsTag", (Block b) => new PredicateSensorDelegate(((BlockAbstractWheel)b).IsWheelTowardsTag), null, new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockWheelBling>("GoldenWheel.IsDPadAlongWheel", (Block b) => new PredicateSensorDelegate(((BlockAbstractWheel)b).IsDPadAlongWheel), null, new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockWheelBling>("GoldenWheel.TurnAlongDPad", null, (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).TurnAlongDPad), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockWheelBling>("GoldenWheel.DriveAlongDPad", null, (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).DriveAlongDPad), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockWheelBling>("GoldenWheel.DriveAlongDPadRaw", null, (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).DriveAlongDPadRaw), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockWheelBling>("GoldenWheel.SetAsSpareTire", null, (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).SetAsSpareTire), null, null, null);
		}
	}
}
