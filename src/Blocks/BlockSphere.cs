using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockSphere : Block
{
	public static Predicate predicateSphereMover;

	public static Predicate predicateSphereTiltMover;

	public static Predicate predicateSphereGoto;

	public static Predicate predicateSphereChase;

	public static Predicate predicateSphereStay;

	public static Predicate predicateSphereAvoid;

	public static Predicate predicateSphereJump;

	private Vector3 torqueSum = Vector3.zero;

	private Vector3 targetVelocity = Vector3.zero;

	private int treatAsVehicleStatus = -1;

	private float stayForce;

	private bool hasModifiedRb;

	private float onGround;

	private float sphereMass = 1f;

	private float jumpHeight;

	private int jumpCounter;

	private const int MAX_JUMP_FRAMES = 5;

	private const int JUMP_RELOAD_FRAMES = 20;

	public BlockSphere(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	public new static void Register()
	{
		predicateSphereMover = PredicateRegistry.Add<BlockSphere>("Sphere.AnalogStickControl", null, (Block b) => ((BlockSphere)b).AnalogStickControl, new Type[2]
		{
			typeof(string),
			typeof(float)
		}, new string[2] { "Stick name", "Amount" });
		predicateSphereTiltMover = PredicateRegistry.Add<BlockSphere>("Sphere.TiltMover", null, (Block b) => ((BlockSphere)b).TiltMoverControl, new Type[2]
		{
			typeof(float),
			typeof(int)
		}, new string[2] { "Speed", "WorldUp" });
		predicateSphereChase = PredicateRegistry.Add<BlockSphere>("Sphere.MoveThroughTag", null, (Block b) => ((BlockSphere)b).MoveThroughTag, new Type[2]
		{
			typeof(string),
			typeof(float)
		}, new string[2] { "Tag name", "Speed" });
		predicateSphereGoto = PredicateRegistry.Add<BlockSphere>("Sphere.MoveToTag", null, (Block b) => ((BlockSphere)b).MoveToTag, new Type[2]
		{
			typeof(string),
			typeof(float)
		}, new string[2] { "Tag name", "Speed" });
		predicateSphereAvoid = PredicateRegistry.Add<BlockSphere>("Sphere.AvoidTag", null, (Block b) => ((BlockSphere)b).AvoidTag, new Type[3]
		{
			typeof(string),
			typeof(float),
			typeof(float)
		}, new string[3] { "Tag name", "Max Speed", "Distance" });
		predicateSphereStay = PredicateRegistry.Add<BlockSphere>("Sphere.Stay", null, (Block b) => ((BlockSphere)b).Stay, new Type[1] { typeof(float) }, new string[1] { "Force" });
		predicateSphereJump = PredicateRegistry.Add<BlockSphere>("Sphere.Jump", null, (Block b) => ((BlockSphere)b).Jump, new Type[1] { typeof(float) }, new string[1] { "Height" });
		Block.AddSimpleDefaultTiles(new GAF(predicateSphereMover, "L", 5f), "Sphere");
		Block.AddSimpleDefaultTiles(new GAF(predicateSphereMover, "L", 5f), new GAF("Block.PlayVfxDurational", "Sparkle"), "Geodesic Ball");
	}

	public TileResultCode AnalogStickControl(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, "L");
		float num = Util.GetFloatArg(args, 1, 0f) * eInfo.floatArg;
		Blocksworld.UI.Controls.EnableDPad(stringArg, MoverDirectionMask.ALL);
		Vector3 worldDPadOffset = Blocksworld.UI.Controls.GetWorldDPadOffset(stringArg);
		if (worldDPadOffset.sqrMagnitude > 0.001f)
		{
			targetVelocity += num * worldDPadOffset;
			Vector3 normalized = Vector3.Cross(Vector3.up, worldDPadOffset).normalized;
			torqueSum += 0.5f * normalized * num * worldDPadOffset.magnitude;
		}
		ModifyRb();
		return TileResultCode.True;
	}

	protected override void HandleTiltMover(float xTilt, float yTilt, float zTilt)
	{
		Vector3 vector = Blocksworld.cameraUp - Vector3.Dot(Blocksworld.cameraUp, Vector3.up) * Vector3.up;
		Vector3 cameraRight = Blocksworld.cameraRight;
		Vector3 vector2 = 2f * cameraRight * xTilt + 2f * vector.normalized * yTilt;
		if (vector2.sqrMagnitude > 0.001f)
		{
			targetVelocity += vector2;
			Vector3 normalized = Vector3.Cross(Vector3.up, vector2).normalized;
			torqueSum += 0.5f * normalized * vector2.magnitude;
		}
		ModifyRb();
	}

	private void MoveTowardsTag(string tag, float speed, float slowdownDist = 0f)
	{
		Vector3 position = goT.position;
		if (!TagManager.TryGetClosestBlockWithTag(tag, position, out var block))
		{
			return;
		}
		Vector3 vector = ((!(block.size.sqrMagnitude > 4f)) ? block.goT.position : block.go.GetComponent<Collider>().ClosestPointOnBounds(position));
		Vector3 vector2 = vector - position;
		vector2.y = 0f;
		float num = vector2.magnitude - size.x * 0.5f;
		if (num > 0.1f)
		{
			Vector3 normalized = vector2.normalized;
			float num2 = slowdownDist * 0.2f;
			if (num < num2)
			{
				speed = 0f;
			}
			else if (num < slowdownDist)
			{
				speed *= num / slowdownDist;
			}
			targetVelocity += speed * normalized;
			Vector3 normalized2 = Vector3.Cross(Vector3.up, normalized).normalized;
			torqueSum += 0.5f * normalized2 * speed;
		}
		ModifyRb();
	}

	public TileResultCode AvoidTag(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		Vector3 position = goT.position;
		if (TagManager.TryGetClosestBlockWithTag(stringArg, position, out var block))
		{
			float num = Util.GetFloatArg(args, 1, 5f) * eInfo.floatArg;
			float num2 = Util.GetFloatArg(args, 2, 10f) * eInfo.floatArg;
			Vector3 vector = ((!(block.size.sqrMagnitude > 4f)) ? block.goT.position : block.go.GetComponent<Collider>().ClosestPointOnBounds(position));
			Vector3 vector2 = vector - position;
			vector2.y = 0f;
			float num3 = vector2.magnitude - size.x * 0.5f;
			if (num3 > 0.001f && num3 <= num2)
			{
				Vector3 normalized = vector2.normalized;
				float num4 = Mathf.Clamp(1f - num3 / num2, 0.1f, 1f);
				targetVelocity -= num * num4 * normalized;
				Vector3 normalized2 = Vector3.Cross(Vector3.up, normalized).normalized;
				torqueSum -= 0.5f * normalized2 * num * num4;
			}
			ModifyRb();
		}
		return TileResultCode.True;
	}

	public TileResultCode MoveToTag(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		float num = Util.GetFloatArg(args, 1, 0f) * eInfo.floatArg;
		MoveTowardsTag(stringArg, num, num);
		return TileResultCode.True;
	}

	public TileResultCode MoveThroughTag(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		float speed = Util.GetFloatArg(args, 1, 0f) * eInfo.floatArg;
		MoveTowardsTag(stringArg, speed);
		return TileResultCode.True;
	}

	public TileResultCode Stay(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float num = Util.GetFloatArg(args, 0, 0f) * eInfo.floatArg;
		stayForce += num;
		ModifyRb();
		return TileResultCode.True;
	}

	public TileResultCode Jump(ScriptRowExecutionInfo eInfo, object[] args)
	{
		jumpHeight += Util.GetFloatArg(args, 0, 0f) * eInfo.floatArg;
		ModifyRb();
		return TileResultCode.True;
	}

	public override void Play()
	{
		base.Play();
		treatAsVehicleStatus = -1;
		torqueSum = Vector3.zero;
		targetVelocity = Vector3.zero;
		onGround = 0f;
		stayForce = 0f;
		jumpCounter = 25;
		jumpHeight = 0f;
		hasModifiedRb = false;
		sphereMass = GetChunkRigidbody().mass;
	}

	private void ModifyRb()
	{
		if (!hasModifiedRb)
		{
			Rigidbody chunkRigidbody = GetChunkRigidbody();
			if (chunkRigidbody != null)
			{
				chunkRigidbody.maxAngularVelocity = 30f;
				chunkRigidbody.angularDrag = 0.01f;
			}
			hasModifiedRb = true;
		}
	}

	private Rigidbody GetChunkRigidbody()
	{
		return chunk.rb;
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (didFix || broken || !hasModifiedRb)
		{
			return;
		}
		Transform parent = goT.parent;
		if (parent != null && CollisionManager.bumping.Contains(parent.gameObject))
		{
			onGround = 0.2f;
		}
		else
		{
			onGround -= Blocksworld.fixedDeltaTime;
		}
		Rigidbody chunkRigidbody = GetChunkRigidbody();
		if (chunkRigidbody != null)
		{
			if (chunkRigidbody.IsSleeping())
			{
				chunkRigidbody.WakeUp();
			}
			Vector3 velocity = chunkRigidbody.velocity;
			if (stayForce > 0f)
			{
				targetVelocity -= stayForce * velocity;
			}
			Vector3 vector = targetVelocity - velocity;
			Vector3 vector2 = Vector3.Cross(Vector3.up, vector.normalized) * torqueSum.magnitude;
			chunkRigidbody.AddTorque(vector2 * chunkRigidbody.mass * Mathf.Min(vector.magnitude, 2f));
			if (onGround > 0f)
			{
				Vector3 force = vector * chunkRigidbody.mass;
				chunkRigidbody.AddForce(force);
			}
			if (jumpHeight > 0.01f)
			{
				Vector3 force2 = Vector3.up * Util.GetJumpForcePerFrame(jumpHeight, sphereMass, 5);
				if (jumpCounter < 5)
				{
					chunkRigidbody.AddForce(force2, ForceMode.Force);
				}
				else if (jumpCounter - 5 > 20 && onGround > 0f)
				{
					jumpCounter = 0;
					chunkRigidbody.AddForce(force2, ForceMode.Force);
				}
			}
			jumpCounter++;
		}
		torqueSum = Vector3.zero;
		targetVelocity = Vector3.zero;
		stayForce = 0f;
		jumpHeight = 0f;
	}

	public override bool TreatAsVehicleLikeBlock()
	{
		return TreatAsVehicleLikeBlockWithStatus(ref treatAsVehicleStatus);
	}
}
