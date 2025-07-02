using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockJetEngine : BlockAbstractRocket
{
	private GameObject blades;

	private float bladesAngleVelocity;

	private float bladesAngle;

	private float initBladesAngle;

	private bool canRotate;

	public BlockJetEngine(List<List<Tile>> tiles)
		: base(tiles, "Blocks/Jet Exhaust", "Blocks/Rocket Flame")
	{
		setSmokeColor = true;
		smokeColorMeshIndex = 0;
		blades = GetSubMeshGameObject(1);
		initBladesAngle = blades.transform.localRotation.eulerAngles.y;
		bladesAngle = initBladesAngle;
		bladesAngleVelocity = 0f;
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockJetEngine>("JetEngine.Fire", (Block b) => ((BlockAbstractRocket)b).IsFiring, (Block b) => ((BlockAbstractRocket)b).FireRocket, new Type[1] { typeof(float) }, new string[1] { "Force" });
		PredicateRegistry.Add<BlockJetEngine>("JetEngine.Smoke", (Block b) => ((BlockAbstractRocket)b).IsFlaming, (Block b) => ((BlockAbstractRocket)b).Flame);
		PredicateRegistry.Add<BlockJetEngine>("JetEngine.Flame", (Block b) => ((BlockAbstractRocket)b).IsSmoking, (Block b) => ((BlockAbstractRocket)b).Smoke);
		Block.AddSimpleDefaultTiles(new GAF("JetEngine.Fire", 2f), "Jet Engine");
	}

	private void ResetBladesAngle()
	{
		SetBladesAngle(initBladesAngle);
		bladesAngleVelocity = 0f;
	}

	public override void Play()
	{
		base.Play();
		Vector3 vector = Scale();
		ResetBladesAngle();
		canRotate = Mathf.Abs(vector.x - vector.z) < 0.01f;
	}

	public override void Stop(bool resetBlock = true)
	{
		base.Stop(resetBlock);
		ResetBladesAngle();
	}

	private void SetBladesAngle(float a)
	{
		blades.transform.localRotation = Quaternion.Euler(0f, a, 0f);
	}

	public override void FixedUpdate()
	{
		if (canRotate && !broken)
		{
			float num = 0f;
			Rigidbody rb = chunk.rb;
			if (rb != null && !rb.isKinematic)
			{
				Vector3 up = goT.up;
				num = Mathf.Max(0f, Vector3.Dot(rb.velocity, up));
			}
			bladesAngleVelocity = 0.8f * bladesAngleVelocity + 0.2f * Mathf.Max(200f, 200f + smokeForce * 1000f + num * 20f) * Blocksworld.fixedDeltaTime;
			bladesAngle += bladesAngleVelocity;
			if (bladesAngle > 360f)
			{
				bladesAngle -= 360f;
			}
			SetBladesAngle(bladesAngle);
		}
		base.FixedUpdate();
	}

	public override bool HasPreferredLookTowardAngleLocalVector()
	{
		return true;
	}

	public override Vector3 GetPreferredLookTowardAngleLocalVector()
	{
		return Vector3.down;
	}
}
