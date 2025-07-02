using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockMLPLegs : BlockAbstractLegs
{
	public static Predicate predicateMLPLegsMover;

	public static Predicate predicateMLPLegsJump;

	public BlockMLPLegs(List<List<Tile>> tiles, Dictionary<string, string> partNames = null, float ankleYSeparator = 0f)
		: base(tiles, partNames, 2, new float[2] { 0.4f, -1f }, new int[2][]
		{
			new int[2] { 0, 1 },
			new int[2] { 3, 2 }
		}, ankleYSeparator, oneAnkleMeshPerFoot: true)
	{
		footOffsetY = 0f;
		onGroundHeight = 0.8f;
		stepSpeedTrigger = 1f;
		maxStepLength = 0.8f;
		moveCM = true;
		moveCMOffsetFeetCenter = 1f;
		resetFeetPositionsOnCreate = true;
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockMLPLegs>("MLPLegs.Turn", (Block b) => ((BlockAbstractLegs)b).IsTurning, (Block b) => ((BlockAbstractLegs)b).Turn, new Type[1] { typeof(float) });
		predicateMLPLegsJump = PredicateRegistry.Add<BlockMLPLegs>("MLPLegs.Jump", (Block b) => ((BlockAbstractLegs)b).IsJumping, (Block b) => ((BlockAbstractLegs)b).Jump, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockMLPLegs>("MLPLegs.GotoTag", null, (Block b) => ((BlockAbstractLegs)b).GotoTag, new Type[3]
		{
			typeof(string),
			typeof(float),
			typeof(float)
		});
		PredicateRegistry.Add<BlockMLPLegs>("MLPLegs.ChaseTag", null, (Block b) => ((BlockAbstractLegs)b).ChaseTag, new Type[3]
		{
			typeof(string),
			typeof(float),
			typeof(float)
		});
		PredicateRegistry.Add<BlockMLPLegs>("MLPLegs.GotoTap", null, (Block b) => ((BlockAbstractLegs)b).GotoTap, new Type[2]
		{
			typeof(float),
			typeof(float)
		});
		predicateMLPLegsMover = PredicateRegistry.Add<BlockMLPLegs>("MLPLegs.AnalogStickControl", null, (Block b) => ((BlockAbstractLegs)b).DPadControl, new Type[3]
		{
			typeof(string),
			typeof(float),
			typeof(float)
		});
		PredicateRegistry.Add<BlockMLPLegs>("MLPLegs.Translate", null, (Block b) => ((BlockAbstractLegs)b).Translate, new Type[2]
		{
			typeof(Vector3),
			typeof(float)
		});
		PredicateRegistry.Add<BlockMLPLegs>("MLPLegs.WackyMode", null, (Block b) => ((BlockAbstractLegs)b).WackyMode);
		PredicateRegistry.Add<BlockMLPLegs>("MLPLegs.TurnTowardsTag", null, (Block b) => ((BlockAbstractLegs)b).TurnTowardsTag, new Type[1] { typeof(string) });
		PredicateRegistry.Add<BlockMLPLegs>("MLPLegs.TurnTowardsTap", null, (Block b) => ((BlockAbstractLegs)b).TurnTowardsTap);
		PredicateRegistry.Add<BlockMLPLegs>("MLPLegs.TurnAlongCam", null, (Block b) => ((BlockAbstractLegs)b).TurnAlongCam);
		PredicateRegistry.Add<BlockMLPLegs>("MLPLegs.AvoidTag", null, (Block b) => ((BlockAbstractLegs)b).AvoidTag, new Type[3]
		{
			typeof(string),
			typeof(float),
			typeof(float)
		});
		PredicateRegistry.Add<BlockMLPLegs>("MLPLegs.Idle", null, (Block b) => ((BlockAbstractLegs)b).Idle);
	}

	public override void Play()
	{
		base.Play();
		BoxCollider boxCollider = (BoxCollider)go.GetComponent<Collider>();
		boxCollider.size = new Vector3(1f, 1f, 1.5f);
		boxCollider.center = new Vector3(0f, 0.5f, -0.25f);
		HideFeet();
	}

	protected override void PauseAnkles()
	{
	}

	private void HideFeet()
	{
		for (int i = 0; i < feet.Length; i++)
		{
			FootInfo footInfo = feet[i];
			footInfo.go.GetComponent<Renderer>().enabled = false;
		}
	}

	public override void Stop(bool resetBlock = true)
	{
		base.Stop(resetBlock);
		BoxCollider boxCollider = (BoxCollider)go.GetComponent<Collider>();
		boxCollider.size = new Vector3(1f, 2f, 1.5f);
		boxCollider.center = new Vector3(0f, 0f, -0.25f);
		HideFeet();
	}

	public override void Break(Vector3 chunkPos, Vector3 chunkVel, Vector3 chunkAngVel)
	{
		ResetFeetPositions();
		base.Break(chunkPos, chunkVel, chunkAngVel);
		BoxCollider boxCollider = (BoxCollider)go.GetComponent<Collider>();
		boxCollider.size = new Vector3(1f, 2f, 1.5f);
		boxCollider.center = new Vector3(0f, 0f, -0.25f);
	}

	public override void FindFeet()
	{
		base.FindFeet();
		for (int i = 0; i < feet.Length; i++)
		{
			FootInfo footInfo = feet[i];
			Transform transform = footInfo.go.transform;
			footInfo.origLocalPosition = transform.localPosition;
			footInfo.origLocalRotation = transform.localRotation;
		}
		HideFeet();
	}

	protected override void ResetFeetPositions()
	{
		bool flag = Blocksworld.CurrentState == State.Play;
		Matrix4x4 localToWorldMatrix = goT.localToWorldMatrix;
		for (int i = 0; i < feet.Length; i++)
		{
			FootInfo footInfo = feet[i];
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
			PositionAnkle(i);
		}
		for (int j = 0; j < feet.Length; j++)
		{
			FootInfo footInfo2 = feet[j];
			footInfo2.position = footInfo2.go.transform.position;
		}
	}
}
