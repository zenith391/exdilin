using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockAbstractMotor : Block
{
	private enum MotorState
	{
		Hold,
		Step,
		Turn,
		Spin,
		TargetAngle
	}

	private const float anglesPerSecond = 90f;

	private float stepSize = 22.5f;

	private MotorState state;

	private int treatAsVehicleStatus = -1;

	private float motorTargetVelocity;

	private bool hasPositiveAngleLimit;

	private float positiveAngleLimit = 45f;

	private bool hasNegativeAngleLimit;

	private float negativeAngleLimit = -45f;

	private float cumulativeTargetAngle;

	private float stepTargetAngle;

	private float targetAngle;

	private float currentAngle;

	private float lastAngle;

	private const float turnAcceleration = 180f;

	private float turnDecceleration = 90f;

	private float turnSpeed;

	private float cumulativeTurn;

	private float turnLoopPitch = 1f;

	private float turnLoopVol;

	private int stepSoundCounter;

	private int sfxLoopUpdateCounter;

	private const int SFX_LOOP_UPDATE_INTERVAL = 5;

	private GameObject rotor;

	private GameObject fakeRotor;

	private GameObject axle;

	private ConfigurableJoint joint;

	private JointMotor m;

	private Vector3 pausedVelocityAxle;

	private Vector3 pausedAngularVelocityAxle;

	private float chunkMi = 1f;

	private float rotorChunkMi = 1f;

	private bool chunkKinematic;

	private bool rotorChunkKinematic;

	private Chunk rotorChunk;

	public BlockAbstractMotor(List<List<Tile>> tiles, string axleName)
		: base(tiles)
	{
		axle = goT.Find(axleName).gameObject;
		loopName = "Motor Turn";
		sfxLoopUpdateCounter = Random.Range(0, 5);
	}

	public override void Play2()
	{
		base.Play2();
		CreateFakeRigidbodyBetweenJoints();
	}

	public override void Play()
	{
		base.Play();
		hasNegativeAngleLimit = false;
		hasPositiveAngleLimit = false;
		treatAsVehicleStatus = -1;
		motorTargetVelocity = 0f;
		List<Block> list = ConnectionsOfType(2, directed: true);
		if (list.Count > 0)
		{
			rotorChunk = list[0].chunk;
			rotor = rotorChunk.go;
		}
		else
		{
			fakeRotor = new GameObject(go.name + " Fake Rotor");
			fakeRotor.transform.position = goT.position;
			Rigidbody rigidbody = fakeRotor.AddComponent<Rigidbody>();
			if (Blocksworld.interpolateRigidBodies)
			{
				rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
			}
			rigidbody.mass = 0.0001f;
			axle.transform.parent = fakeRotor.transform;
			rotor = fakeRotor;
		}
		CreateJoint();
		state = MotorState.Hold;
		cumulativeTargetAngle = 0f;
		targetAngle = 0f;
		stepTargetAngle = 0f;
		currentAngle = 0f;
		lastAngle = 0f;
		turnSpeed = 0f;
		CalculateMassDistributions();
	}

	private void CreateJoint()
	{
		if (chunk.go != rotor)
		{
			joint = chunk.go.AddComponent<ConfigurableJoint>();
			joint.anchor = goT.localPosition;
			joint.axis = goT.up;
			joint.xMotion = ConfigurableJointMotion.Locked;
			joint.yMotion = ConfigurableJointMotion.Locked;
			joint.zMotion = ConfigurableJointMotion.Locked;
			joint.angularXMotion = ConfigurableJointMotion.Free;
			joint.angularYMotion = ConfigurableJointMotion.Locked;
			joint.angularZMotion = ConfigurableJointMotion.Locked;
			joint.connectedBody = rotor.GetComponent<Rigidbody>();
		}
	}

	private void DestroyJoint()
	{
		if (joint != null)
		{
			Object.Destroy(joint);
			joint = null;
			DestroyFakeRigidbodies();
		}
	}

	public override void Stop(bool resetBlock = true)
	{
		DestroyJoint();
		if (fakeRotor != null)
		{
			if (axle.GetComponent<Collider>() != null)
			{
				Object.Destroy(axle.GetComponent<Collider>());
			}
			DestroyFakeRotor();
		}
		axle.transform.localScale = Vector3.one;
		axle.transform.localRotation = Quaternion.identity;
		axle.GetComponent<Renderer>().enabled = true;
		PlayLoopSound(play: false, GetLoopClip());
		base.Stop(resetBlock);
	}

	private void DestroyFakeRotor()
	{
		if (fakeRotor != null)
		{
			Util.UnparentTransformSafely(axle.transform);
			axle.transform.position = goT.position;
			axle.transform.rotation = goT.rotation;
			axle.transform.parent = goT;
			Object.Destroy(fakeRotor);
			fakeRotor = null;
		}
	}

	public override void Pause()
	{
		if (fakeRotor != null)
		{
			pausedVelocityAxle = fakeRotor.GetComponent<Rigidbody>().velocity;
			pausedAngularVelocityAxle = fakeRotor.GetComponent<Rigidbody>().angularVelocity;
			fakeRotor.GetComponent<Rigidbody>().isKinematic = true;
		}
		PlayLoopSound(play: false, GetLoopClip());
	}

	public override void Resume()
	{
		if (fakeRotor != null)
		{
			fakeRotor.GetComponent<Rigidbody>().isKinematic = false;
			fakeRotor.GetComponent<Rigidbody>().velocity = pausedVelocityAxle;
			fakeRotor.GetComponent<Rigidbody>().angularVelocity = pausedAngularVelocityAxle;
		}
	}

	public override void Break(Vector3 chunkPos, Vector3 chunkVel, Vector3 chunkAngVel)
	{
		base.Break(chunkPos, chunkVel, chunkAngVel);
		if (fakeRotor != null)
		{
			axle.AddComponent<BoxCollider>();
			Block.AddExplosiveForce(fakeRotor.GetComponent<Rigidbody>(), fakeRotor.transform.position, chunkPos, chunkVel, chunkAngVel);
		}
		DestroyJoint();
	}

	public TileResultCode SetPositiveAngleLimit(ScriptRowExecutionInfo eInfo, object[] args)
	{
		positiveAngleLimit = Util.GetFloatArg(args, 0, float.MaxValue);
		hasPositiveAngleLimit = true;
		return TileResultCode.True;
	}

	public TileResultCode SetNegativeAngleLimit(ScriptRowExecutionInfo eInfo, object[] args)
	{
		negativeAngleLimit = Util.GetFloatArg(args, 0, float.MinValue);
		hasNegativeAngleLimit = true;
		return TileResultCode.True;
	}

	public TileResultCode Turn(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float num = (float)args[0] * eInfo.floatArg;
		cumulativeTurn += num;
		state = MotorState.Turn;
		return TileResultCode.True;
	}

	private void ExecuteTurn()
	{
		if (Mathf.Abs(cumulativeTurn) > 0.0001f)
		{
			float num = Mathf.Max(Mathf.Abs(motorTargetVelocity), Mathf.Abs(cumulativeTurn) * stepSize / 0.25f);
			turnSpeed = Mathf.Clamp(turnSpeed + cumulativeTurn * 180f * Blocksworld.fixedDeltaTime, 0f - num, num);
			float num2 = Mathf.Abs(motorTargetVelocity);
			turnDecceleration += num2;
		}
		cumulativeTurn = 0f;
	}

	public TileResultCode Return(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float num = eInfo.floatArg * Util.GetFloatArg(args, 0, 1f);
		float a = num * stepSize / 0.25f;
		float num2 = num;
		if (targetAngle > 0f)
		{
			num2 *= -1f;
		}
		float num3 = Mathf.Min(a, Mathf.Max(Mathf.Abs(targetAngle * 10f), 1f));
		turnSpeed = Mathf.Clamp(turnSpeed + num2 * 180f * Blocksworld.fixedDeltaTime, 0f - num3, num3);
		turnDecceleration += Mathf.Abs(motorTargetVelocity);
		state = MotorState.Turn;
		return TileResultCode.True;
	}

	public TileResultCode FreeSpin(ScriptRowExecutionInfo eInfo, object[] args)
	{
		state = MotorState.Spin;
		return TileResultCode.True;
	}

	public TileResultCode Step(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float num = (float)args[0] * eInfo.floatArg;
		if (Blocksworld.CurrentState == State.Play && eInfo.timer == Blocksworld.fixedDeltaTime && !broken && !vanished)
		{
			stepSoundCounter = 3;
		}
		float num2 = 0.25f;
		if (eInfo.timer == 0f)
		{
			float num3 = stepSize * num;
			stepTargetAngle += num3;
			cumulativeTargetAngle += num3;
		}
		state = MotorState.Step;
		float num4 = Blocksworld.fixedDeltaTime / num2 * stepSize * num;
		targetAngle += num4;
		if (eInfo.timer >= num2)
		{
			state = MotorState.Hold;
			targetAngle = stepTargetAngle;
			return TileResultCode.True;
		}
		return TileResultCode.Delayed;
	}

	public TileResultCode TargetAngle(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float num = (float)args[0] * eInfo.floatArg;
		float num2 = 180f * Blocksworld.fixedDeltaTime;
		if (num < targetAngle)
		{
			targetAngle = Mathf.Max(num, targetAngle - num2);
		}
		else if (num > targetAngle)
		{
			targetAngle = Mathf.Min(num, targetAngle + num2);
		}
		state = MotorState.TargetAngle;
		return TileResultCode.True;
	}

	private void UpdateMotorLoopSound()
	{
		if (!Sound.sfxEnabled || vanished || isTreasure)
		{
			PlayLoopSound(play: false, GetLoopClip(), 0f);
			return;
		}
		float num = 0f;
		if (state != MotorState.Spin && state != MotorState.Hold && joint != null && rotor != null)
		{
			Rigidbody rb = chunk.rb;
			if (rb != null)
			{
				Vector3 up = goT.up;
				float num2 = Vector3.Dot(rotor.GetComponent<Rigidbody>().angularVelocity, up);
				float num3 = Vector3.Dot(rb.angularVelocity, up);
				num = Mathf.Abs(num2 - num3);
				float num4 = num / 1.5f;
				float num5 = 0.05f;
				if (turnLoopPitch > num4)
				{
					num5 *= -2f;
				}
				turnLoopPitch = Mathf.Clamp(turnLoopPitch + num5, 0.5f, 3f);
			}
		}
		float num6 = 0.03f;
		if (stepSoundCounter > 0 && !broken && !broken)
		{
			num6 = 0.2f;
			stepSoundCounter--;
		}
		if (num < 0.2f || broken || vanished)
		{
			num6 *= -1f;
		}
		turnLoopVol = Mathf.Clamp(turnLoopVol + num6, 0f, 0.5f);
		if (sfxLoopUpdateCounter % 5 == 0)
		{
			float num7 = 1f;
			if (turnLoopVol > 0.01f)
			{
				float num8 = 0.18f;
				num7 = num8 * 2f * (Mathf.PerlinNoise(0.972f * Time.time, 5f) - 0.5f) + 1f;
			}
			PlayLoopSound(turnLoopVol > 0.01f, GetLoopClip(), turnLoopVol, null, num7 * turnLoopPitch);
			UpdateWithinWaterLPFilter();
		}
		sfxLoopUpdateCounter++;
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		ExecuteTurn();
		UpdateMotorLoopSound();
		if (isTreasure || joint == null || rotor == null)
		{
			return;
		}
		if (joint.connectedBody == null)
		{
			DestroyJoint();
			return;
		}
		lastAngle = currentAngle;
		currentAngle = Util.AngleBetween(rotor.transform.rotation * goT.localRotation * Vector3.forward, goT.forward, goT.up);
		if (fakeRotor == null)
		{
			axle.transform.localRotation = Quaternion.Euler(0f, 0f - currentAngle, 0f);
		}
		if (turnSpeed != 0f)
		{
			float num = Blocksworld.fixedDeltaTime * turnSpeed;
			targetAngle += num;
			cumulativeTargetAngle += num;
		}
		float num2 = targetAngle;
		if (hasPositiveAngleLimit)
		{
			targetAngle = Mathf.Min(positiveAngleLimit, targetAngle);
		}
		if (hasNegativeAngleLimit)
		{
			targetAngle = Mathf.Max(negativeAngleLimit, targetAngle);
		}
		bool flag = Mathf.Abs(targetAngle - num2) > 0.01f;
		if (targetAngle > 180f)
		{
			targetAngle -= 360f;
		}
		else if (targetAngle < -180f)
		{
			targetAngle += 360f;
		}
		float num3 = targetAngle - currentAngle;
		if (num3 > 0f)
		{
			float num4 = num3 - 360f;
			if (Mathf.Abs(num4) < Mathf.Abs(num3))
			{
				num3 = num4;
			}
		}
		else
		{
			float num5 = num3 + 360f;
			if (Mathf.Abs(num5) < Mathf.Abs(num3))
			{
				num3 = num5;
			}
		}
		float num6 = Mathf.Abs(num3);
		if (state == MotorState.Turn && num6 > 120f)
		{
			targetAngle = currentAngle;
			turnSpeed = 0f;
		}
		if (state == MotorState.Spin && !flag)
		{
			targetAngle = currentAngle;
			cumulativeTargetAngle = currentAngle;
			SetMotorVelocity(0f, 0f);
		}
		else
		{
			if (state == MotorState.Spin)
			{
				targetAngle = 0.98f * targetAngle;
			}
			float num7 = Mathf.Max(chunkMi, rotorChunkMi);
			Rigidbody rb = chunk.rb;
			if (rb != null && !chunkKinematic)
			{
				num7 = Mathf.Min(num7, chunkMi);
			}
			Rigidbody component = rotor.GetComponent<Rigidbody>();
			if (component != null && !rotorChunkKinematic)
			{
				num7 = Mathf.Min(num7, rotorChunkMi);
			}
			float v = Mathf.Clamp(num3, -25f, 25f);
			float force = Mathf.Max(num7 * (100f + Mathf.Abs(turnSpeed * 0.2f)), 20f) * 100f;
			SetMotorVelocity(v, force);
		}
		if (state != MotorState.Turn)
		{
			turnDecceleration = Mathf.Max(90f, Mathf.Abs(3f * turnSpeed));
		}
		if (turnSpeed > 0f)
		{
			turnSpeed = Mathf.Max(0f, turnSpeed - turnDecceleration * Blocksworld.fixedDeltaTime);
		}
		else if (turnSpeed < 0f)
		{
			turnSpeed = Mathf.Min(0f, turnSpeed + turnDecceleration * Blocksworld.fixedDeltaTime);
		}
		if (state != MotorState.Step)
		{
			stepTargetAngle = targetAngle;
		}
		turnDecceleration = 0f;
		state = MotorState.Hold;
	}

	private void SetMotorVelocity(float v, float force = 1000f)
	{
		motorTargetVelocity = v;
		JointDrive angularXDrive = new JointDrive
		{
			maximumForce = force,
			positionDamper = force * 0.0035f
		};
		if (fakeRotor != null)
		{
			angularXDrive.positionDamper *= 0.0035f;
		}
		joint.targetAngularVelocity = new Vector3(v, 0f, 0f);
		joint.angularXDrive = angularXDrive;
		Rigidbody rb = chunk.rb;
		if (rb.IsSleeping())
		{
			rb.WakeUp();
		}
		Rigidbody connectedBody = joint.connectedBody;
		if (connectedBody.IsSleeping())
		{
			connectedBody.WakeUp();
		}
	}

	public TileResultCode IsStepping(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (!((float)args[0] >= 0f))
		{
			return IsSteppingLeft();
		}
		return IsSteppingRight();
	}

	public TileResultCode IsSteppingLeft()
	{
		if (state == MotorState.Step)
		{
			return IsTurning();
		}
		return TileResultCode.False;
	}

	public TileResultCode IsSteppingRight()
	{
		if (state == MotorState.Step)
		{
			return IsReversing();
		}
		return TileResultCode.False;
	}

	public TileResultCode IsTurningSensor(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (!((float)args[0] >= 0f))
		{
			return IsTurning();
		}
		return IsReversing();
	}

	public TileResultCode IsTurning()
	{
		if (Mathf.Abs(currentAngle - lastAngle) < 180f)
		{
			if (!(lastAngle <= currentAngle))
			{
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}
		if (!(lastAngle >= 0f) && !(currentAngle <= 0f))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsReversing()
	{
		if (Mathf.Abs(currentAngle - lastAngle) < 180f)
		{
			if (!(lastAngle >= currentAngle))
			{
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}
		if (!(lastAngle <= 0f) && !(currentAngle >= 0f))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsFreeSpinning(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (state == MotorState.Spin)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public void CalculateMassDistributions()
	{
		Chunk ignoreChunk = chunk;
		HashSet<Chunk> hashSet = new HashSet<Chunk>();
		chunkMi = CalculateMassDistribution(ignoreChunk, goT.up, rotorChunk);
		chunkKinematic = ConnectedToKinematicChunk(ignoreChunk, rotorChunk);
		rotorChunkMi = chunkMi;
		if (rotorChunk != null)
		{
			rotorChunkMi = CalculateMassDistribution(rotorChunk, goT.up, ignoreChunk);
			rotorChunkKinematic = ConnectedToKinematicChunk(rotorChunk, ignoreChunk);
		}
	}

	public override void ChunkInModelFrozen()
	{
		base.ChunkInModelFrozen();
		chunkKinematic = ConnectedToKinematicChunk(chunk, rotorChunk);
		if (rotorChunk != null)
		{
			rotorChunkKinematic = ConnectedToKinematicChunk(rotorChunk, chunk);
		}
	}

	public override void ChunkInModelUnfrozen()
	{
		base.ChunkInModelUnfrozen();
		chunkKinematic = ConnectedToKinematicChunk(chunk, rotorChunk);
		if (rotorChunk != null)
		{
			rotorChunkKinematic = ConnectedToKinematicChunk(rotorChunk, chunk);
		}
	}

	protected override void Appearing(float scale)
	{
		base.Appearing(scale);
		if (fakeRotor != null)
		{
			axle.GetComponent<Renderer>().enabled = true;
			axle.transform.localScale = Vector3.one * scale;
		}
	}

	protected override void Vanishing(float scale)
	{
		base.Vanishing(scale);
		if (fakeRotor != null)
		{
			axle.transform.localScale = Vector3.one * scale;
		}
	}

	public override void Vanished()
	{
		base.Vanished();
		if (fakeRotor != null)
		{
			axle.GetComponent<Renderer>().enabled = false;
		}
	}

	public override void Appeared()
	{
		base.Appeared();
		if (fakeRotor != null)
		{
			axle.GetComponent<Renderer>().enabled = true;
			axle.transform.localScale = Vector3.one;
		}
	}

	public override void BecameTreasure()
	{
		base.BecameTreasure();
		DestroyFakeRotor();
	}

	public override bool TreatAsVehicleLikeBlock()
	{
		return TreatAsVehicleLikeBlockWithStatus(ref treatAsVehicleStatus);
	}

	public override void ChunksAndJointsModified(Dictionary<Joint, Joint> oldToNew, Dictionary<Chunk, Chunk> oldToNewChunks, Dictionary<Chunk, Chunk> newToOldChunks)
	{
		if (!broken && !isTreasure && !(joint == null))
		{
			if (oldToNew.TryGetValue(joint, out var value))
			{
				joint = (ConfigurableJoint)value;
			}
			rotor = joint.connectedBody.gameObject;
			List<Block> list = ConnectionsOfType(2, directed: true);
			if (list.Count > 0)
			{
				rotorChunk = list[0].chunk;
			}
		}
	}

	public override void Deactivate()
	{
		base.Deactivate();
		if (fakeRotor != null)
		{
			fakeRotor.SetActive(value: false);
		}
	}

	public override bool IsRuntimeInvisible()
	{
		return false;
	}
}
