using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockLegs : BlockAbstractLegs
{
	public static Predicate predicateLegsJump;

	public static Predicate predicateLegsMover;

	public BlockLegs(List<List<Tile>> tiles, Dictionary<string, string> partNames, float ankleYSeparator = 0f, float torqueMult = 1f, float footWidth = 0.25f)
		: base(tiles, partNames, 1, null, null, ankleYSeparator, oneAnkleMeshPerFoot: false, torqueMult, footWidth)
	{
		maxStepHeight = 0.85f;
		maxStepLength = 1f;
		footOffsetY = -0.25f;
		resetFeetPositionsOnCreate = true;
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockLegs>("Legs.Walk", (Block b) => ((BlockAbstractLegs)b).IsWalkingSensor, (Block b) => ((BlockAbstractLegs)b).Walk, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockLegs>("Legs.Turn", (Block b) => ((BlockAbstractLegs)b).IsTurning, (Block b) => ((BlockAbstractLegs)b).Turn, new Type[1] { typeof(float) });
		predicateLegsJump = PredicateRegistry.Add<BlockLegs>("Legs.Jump", (Block b) => ((BlockAbstractLegs)b).IsJumping, (Block b) => ((BlockAbstractLegs)b).Jump, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockLegs>("Legs.GotoTag", null, (Block b) => ((BlockAbstractLegs)b).GotoTag, new Type[3]
		{
			typeof(string),
			typeof(float),
			typeof(float)
		});
		PredicateRegistry.Add<BlockLegs>("Legs.ChaseTag", null, (Block b) => ((BlockAbstractLegs)b).ChaseTag, new Type[3]
		{
			typeof(string),
			typeof(float),
			typeof(float)
		});
		PredicateRegistry.Add<BlockLegs>("Legs.GotoTap", null, (Block b) => ((BlockAbstractLegs)b).GotoTap, new Type[2]
		{
			typeof(float),
			typeof(float)
		});
		predicateLegsMover = PredicateRegistry.Add<BlockLegs>("Legs.AnalogStickControl", null, (Block b) => ((BlockAbstractLegs)b).DPadControl, new Type[3]
		{
			typeof(string),
			typeof(float),
			typeof(float)
		});
		PredicateRegistry.Add<BlockLegs>("Legs.WackyMode", null, (Block b) => ((BlockAbstractLegs)b).WackyMode);
		PredicateRegistry.Add<BlockLegs>("Legs.Translate", null, (Block b) => ((BlockAbstractLegs)b).Translate, new Type[2]
		{
			typeof(Vector3),
			typeof(float)
		});
		PredicateRegistry.Add<BlockLegs>("Legs.TurnTowardsTag", null, (Block b) => ((BlockAbstractLegs)b).TurnTowardsTag, new Type[1] { typeof(string) });
		PredicateRegistry.Add<BlockLegs>("Legs.TurnTowardsTap", null, (Block b) => ((BlockAbstractLegs)b).TurnTowardsTap);
		PredicateRegistry.Add<BlockLegs>("Legs.TurnAlongCam", null, (Block b) => ((BlockAbstractLegs)b).TurnAlongCam);
		PredicateRegistry.Add<BlockLegs>("Legs.AvoidTag", null, (Block b) => ((BlockAbstractLegs)b).AvoidTag, new Type[3]
		{
			typeof(string),
			typeof(float),
			typeof(float)
		});
		PredicateRegistry.Add<BlockLegs>("Legs.Idle", null, (Block b) => ((BlockAbstractLegs)b).Idle);
	}
}
