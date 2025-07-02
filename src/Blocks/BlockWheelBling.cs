using System;
using System.Collections.Generic;

namespace Blocks;

public class BlockWheelBling : BlockAbstractWheel
{
	public BlockWheelBling(List<List<Tile>> tiles)
		: base(tiles, "Golden Wheel Axle", string.Empty)
	{
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockWheelBling>("GoldenWheel.Drive", (Block b) => ((BlockAbstractWheel)b).IsDrivingSensor, (Block b) => ((BlockAbstractWheel)b).Drive, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockWheelBling>("GoldenWheel.Turn", (Block b) => ((BlockAbstractWheel)b).IsTurning, (Block b) => ((BlockAbstractWheel)b).Turn, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockWheelBling>("GoldenWheel.TurnTowardsTag", null, (Block b) => ((BlockAbstractWheel)b).TurnTowardsTag, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockWheelBling>("GoldenWheel.DriveTowardsTag", null, (Block b) => ((BlockAbstractWheel)b).DriveTowardsTag, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockWheelBling>("GoldenWheel.DriveTowardsTagRaw", null, (Block b) => ((BlockAbstractWheel)b).DriveTowardsTagRaw, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockWheelBling>("GoldenWheel.IsWheelTowardsTag", (Block b) => ((BlockAbstractWheel)b).IsWheelTowardsTag, null, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockWheelBling>("GoldenWheel.IsDPadAlongWheel", (Block b) => ((BlockAbstractWheel)b).IsDPadAlongWheel, null, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockWheelBling>("GoldenWheel.TurnAlongDPad", null, (Block b) => ((BlockAbstractWheel)b).TurnAlongDPad, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockWheelBling>("GoldenWheel.DriveAlongDPad", null, (Block b) => ((BlockAbstractWheel)b).DriveAlongDPad, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockWheelBling>("GoldenWheel.DriveAlongDPadRaw", null, (Block b) => ((BlockAbstractWheel)b).DriveAlongDPadRaw, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockWheelBling>("GoldenWheel.SetAsSpareTire", null, (Block b) => ((BlockAbstractWheel)b).SetAsSpareTire);
	}
}
