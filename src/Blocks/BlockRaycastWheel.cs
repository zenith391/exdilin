using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockRaycastWheel : Block
{
	private int treatAsVehicleStatus = -1;

	private Collider origCollider;

	private WheelCollider wheelCollider;

	private float chunkMass;

	private GameObject wco;

	private float filteredAngle;

	private float speedTarget;

	private float angleTarget;

	public GameObject axle;

	public BlockRaycastWheel(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockRaycastWheel>("RaycastWheel.Drive", null, (Block b) => ((BlockRaycastWheel)b).Drive, new Type[1] { typeof(float) }, new string[1] { "Force" });
		PredicateRegistry.Add<BlockRaycastWheel>("RaycastWheel.Turn", null, (Block b) => ((BlockRaycastWheel)b).Turn, new Type[1] { typeof(float) }, new string[1] { "Angle" });
		Block.AddSimpleDefaultTiles(new GAF("RaycastWheel.Drive", 20f), "Raycast Wheel");
	}

	public override void Play()
	{
		base.Play();
		treatAsVehicleStatus = -1;
		speedTarget = 0f;
		angleTarget = 0f;
		chunkMass = chunk.rb.mass;
		float mass = GetMass();
		origCollider = go.GetComponent<Collider>();
		origCollider.enabled = false;
		wco = new GameObject(go.name + " Wheel Collider GO");
		wco.transform.position = goT.position;
		wco.transform.rotation = goT.rotation;
		Rigidbody rigidbody = wco.AddComponent<Rigidbody>();
		rigidbody.mass = 1f;
		wheelCollider = wco.AddComponent<WheelCollider>();
		FixedJoint fixedJoint = wco.AddComponent<FixedJoint>();
		fixedJoint.connectedBody = chunk.rb;
		WheelFrictionCurve sidewaysFriction = wheelCollider.sidewaysFriction;
		sidewaysFriction.stiffness = chunkMass / 20f;
		wheelCollider.sidewaysFriction = sidewaysFriction;
		WheelFrictionCurve forwardFriction = wheelCollider.forwardFriction;
		forwardFriction.stiffness = chunkMass / 5f;
		wheelCollider.forwardFriction = forwardFriction;
		wheelCollider.enabled = true;
		wheelCollider.suspensionSpring = new JointSpring
		{
			spring = 20f * chunkMass,
			damper = 5f * chunkMass
		};
		wheelCollider.mass = mass;
		float radius = GetRadius();
		wheelCollider.radius = radius;
		CapsuleCollider capsuleCollider = wco.AddComponent<CapsuleCollider>();
		capsuleCollider.direction = 2;
		capsuleCollider.radius = Mathf.Min(radius, 0.3f);
		capsuleCollider.height = capsuleCollider.radius * 0.5f;
		Vector3 center = Vector3.down * 0.3f;
		capsuleCollider.center = center;
		capsuleCollider.material = new PhysicMaterial
		{
			dynamicFriction = 0f,
			staticFriction = 0f
		};
		BWSceneManager.AddBlockMap(wco, this);
	}

	public override void Stop(bool resetBlock = true)
	{
		if (wco != null)
		{
			BWSceneManager.RemoveBlockMap(wco);
			UnityEngine.Object.Destroy(wco);
			wco = null;
		}
		base.Stop(resetBlock);
	}

	public float GetRadius()
	{
		Vector3 scale = GetScale();
		return 0.25f * (scale.y + scale.z);
	}

	public override void ResetFrame()
	{
		speedTarget = 0f;
		angleTarget = 0f;
	}

	public void Drive(float f)
	{
		speedTarget += f;
	}

	public TileResultCode Drive(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float f = (float)args[0] * eInfo.floatArg;
		Drive(f);
		return TileResultCode.True;
	}

	public void Turn(float angleOffset)
	{
		angleTarget += angleOffset;
	}

	public TileResultCode Turn(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float num = (float)args[0] * Mathf.Min(1f, eInfo.floatArg);
		angleTarget += num;
		return TileResultCode.True;
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!isTreasure && !broken && !vanished)
		{
			filteredAngle += (angleTarget - filteredAngle) / 10f;
			wheelCollider.motorTorque = 0.1f * speedTarget * chunkMass;
			wheelCollider.steerAngle = filteredAngle;
			wheelCollider.GetWorldPose(out var pos, out var quat);
			goT.position = pos;
			goT.rotation = quat;
		}
	}

	public override bool TreatAsVehicleLikeBlock()
	{
		return TreatAsVehicleLikeBlockWithStatus(ref treatAsVehicleStatus);
	}
}
