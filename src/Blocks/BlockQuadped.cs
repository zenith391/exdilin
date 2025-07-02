using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockQuadped : BlockAbstractLegs
{
	private static int legPairCount_ = 2;

	private static float[] legPairOffsets_ = new float[2] { 0.35f, -0.35f };

	private static int[][] legPairIndices_ = new int[2][]
	{
		new int[2] { 0, 1 },
		new int[2] { 3, 2 }
	};

	public static Predicate predicateQuadpedMover;

	public static Predicate predicateQuadpedJump;

	public BlockQuadped(List<List<Tile>> tiles, Dictionary<string, string> partNames = null, float ankleYSeparator = 0f, float footWidth = 0.25f)
		: base(tiles, partNames, legPairCount_, legPairOffsets_, legPairIndices_, ankleYSeparator, oneAnkleMeshPerFoot: false, 1f, footWidth)
	{
		maxStepHeight = 0.85f;
		maxStepLength = 1f;
		stepLengthMultiplier = 0.7f;
		stepTimeMultiplier = 0.5f;
		footOffsetY = -0.35f;
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockQuadped>("Quadped.Turn", (Block b) => ((BlockAbstractLegs)b).IsTurning, (Block b) => ((BlockAbstractLegs)b).Turn, new Type[1] { typeof(float) });
		predicateQuadpedJump = PredicateRegistry.Add<BlockQuadped>("Quadped.Jump", (Block b) => ((BlockAbstractLegs)b).IsJumping, (Block b) => ((BlockAbstractLegs)b).Jump, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockQuadped>("Quadped.GotoTag", null, (Block b) => ((BlockAbstractLegs)b).GotoTag, new Type[3]
		{
			typeof(string),
			typeof(float),
			typeof(float)
		});
		PredicateRegistry.Add<BlockQuadped>("Quadped.ChaseTag", null, (Block b) => ((BlockAbstractLegs)b).ChaseTag, new Type[3]
		{
			typeof(string),
			typeof(float),
			typeof(float)
		});
		PredicateRegistry.Add<BlockQuadped>("Quadped.GotoTap", null, (Block b) => ((BlockAbstractLegs)b).GotoTap, new Type[2]
		{
			typeof(float),
			typeof(float)
		});
		predicateQuadpedMover = PredicateRegistry.Add<BlockQuadped>("Quadped.AnalogStickControl", null, (Block b) => ((BlockAbstractLegs)b).DPadControl, new Type[3]
		{
			typeof(string),
			typeof(float),
			typeof(float)
		});
		PredicateRegistry.Add<BlockQuadped>("Quadped.WackyMode", null, (Block b) => ((BlockAbstractLegs)b).WackyMode);
		PredicateRegistry.Add<BlockQuadped>("Quadped.Translate", null, (Block b) => ((BlockAbstractLegs)b).Translate, new Type[2]
		{
			typeof(Vector3),
			typeof(float)
		});
		PredicateRegistry.Add<BlockQuadped>("Quadped.TurnTowardsTag", null, (Block b) => ((BlockAbstractLegs)b).TurnTowardsTag, new Type[1] { typeof(string) });
		PredicateRegistry.Add<BlockQuadped>("Quadped.TurnTowardsTap", null, (Block b) => ((BlockAbstractLegs)b).TurnTowardsTap);
		PredicateRegistry.Add<BlockQuadped>("Quadped.TurnAlongCam", null, (Block b) => ((BlockAbstractLegs)b).TurnAlongCam);
		PredicateRegistry.Add<BlockQuadped>("Quadped.AvoidTag", null, (Block b) => ((BlockAbstractLegs)b).AvoidTag, new Type[3]
		{
			typeof(string),
			typeof(float),
			typeof(float)
		});
		PredicateRegistry.Add<BlockQuadped>("Quadped.Idle", null, (Block b) => ((BlockAbstractLegs)b).Idle);
	}
}
