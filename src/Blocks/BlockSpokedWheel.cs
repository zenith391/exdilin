using System;
using System.Collections.Generic;

namespace Blocks;

public class BlockSpokedWheel : BlockAbstractWheel
{
	public static Predicate predicateSpokedWheelDrive;

	public static Predicate predicateSpokedWheelTurn;

	public BlockSpokedWheel(List<List<Tile>> tiles)
		: base(tiles, "Spoked Wheel Axle", string.Empty)
	{
	}

	public new static void Register()
	{
		predicateSpokedWheelDrive = PredicateRegistry.Add<BlockSpokedWheel>("SpokedWheel.Drive", (Block b) => ((BlockAbstractWheel)b).IsDrivingSensor, (Block b) => ((BlockAbstractWheel)b).Drive, new Type[1] { typeof(float) });
		predicateSpokedWheelTurn = PredicateRegistry.Add<BlockSpokedWheel>("SpokedWheel.Turn", (Block b) => ((BlockAbstractWheel)b).IsTurning, (Block b) => ((BlockAbstractWheel)b).Turn, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockSpokedWheel>("SpokedWheel.TurnTowardsTag", null, (Block b) => ((BlockAbstractWheel)b).TurnTowardsTag, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockSpokedWheel>("SpokedWheel.DriveTowardsTag", null, (Block b) => ((BlockAbstractWheel)b).DriveTowardsTag, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockSpokedWheel>("SpokedWheel.DriveTowardsTagRaw", null, (Block b) => ((BlockAbstractWheel)b).DriveTowardsTagRaw, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockSpokedWheel>("SpokedWheel.IsWheelTowardsTag", (Block b) => ((BlockAbstractWheel)b).IsWheelTowardsTag, null, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockSpokedWheel>("SpokedWheel.IsDPadAlongWheel", (Block b) => ((BlockAbstractWheel)b).IsDPadAlongWheel, null, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockSpokedWheel>("SpokedWheel.TurnAlongDPad", null, (Block b) => ((BlockAbstractWheel)b).TurnAlongDPad, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockSpokedWheel>("SpokedWheel.DriveAlongDPad", null, (Block b) => ((BlockAbstractWheel)b).DriveAlongDPad, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockSpokedWheel>("SpokedWheel.DriveAlongDPadRaw", null, (Block b) => ((BlockAbstractWheel)b).DriveAlongDPadRaw, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockSpokedWheel>("SpokedWheel.SetAsSpareTire", null, (Block b) => ((BlockAbstractWheel)b).SetAsSpareTire);
	}
}
