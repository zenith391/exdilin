using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockAbstractTorsionSpring : Block
{
	private enum TorsionSpringMode
	{
		FreeSpin,
		Spring
	}

	private TorsionSpringMode mode = TorsionSpringMode.Spring;

	private int treatAsVehicleStatus = -1;

	private const float angleEpsilon = 0.05f;

	private const float anglesPerSecond = 90f;

	private float targetAngle;

	private float currentAngle;

	private float lastRealAngle;

	private float springStiffness = 1f;

	private float springLimit;

	private float angleOffset;

	private int halfRevolutions;

	private float turnLoopPitch = 1f;

	private float turnLoopVol;

	private float stretchSoundTime;

	private Chunk rotorChunk;

	private float chunkMi = 1f;

	private float rotorChunkMi = 1f;

	private bool chunkKinematic;

	private bool rotorChunkKinematic;

	private bool released;

	private bool charging;

	private bool _limitedRange;

	private float _limitedRangeAngle = 0.05f;

	private float _limitedRangeRealAngle;

	private GameObject rotor;

	private Transform rotorT;

	private GameObject fakeRotor;

	private GameObject axle;

	public ConfigurableJoint joint;

	private Vector3 pausedVelocityAxle;

	private Vector3 pausedAngularVelocityAxle;

	private AudioSource axleAudioSource;

	private int sfxLoopUpdateCounter;

	private const int SFX_LOOP_UPDATE_INTERVAL = 5;

	private bool locked;

	private bool wasLocked;

	private float lockedAngleOffset;

	private float rotorAngleVecMultiplier = 1f;

	private int rotorAngleVecType;

	public Rigidbody jointToJointConnection;

	private Vector3 jointOffset = Vector3.zero;

	public BlockAbstractTorsionSpring(List<List<Tile>> tiles, string axleName, float angleLimit)
		: base(tiles)
	{
		axle = goT.Find(axleName).gameObject;
		loopName = "Torsion Spring Loop";
		sfxLoopUpdateCounter = Random.Range(0, 5);
		springLimit = angleLimit;
	}

	private float GetAngleIncPerSecond(object[] args)
	{
		if (args.Length != 0)
		{
			return (float)args[0];
		}
		return 15f;
	}

	private void Charge(object[] args, float arg1 = 1f)
	{
		if (joint == null)
		{
			return;
		}
		float num = ((springLimit <= 0f) ? 90f : springLimit);
		float num2 = arg1 * Blocksworld.fixedDeltaTime * GetAngleIncPerSecond(args);
		angleOffset = Mathf.Clamp(angleOffset + num2, 0f - num, num);
		charging = Mathf.Abs(angleOffset) < 89f;
		mode = TorsionSpringMode.Spring;
		if (_limitedRange)
		{
			float limitedRangeRealAngle = _limitedRangeRealAngle;
			if (charging)
			{
				_limitedRangeRealAngle = Mathf.Clamp(angleOffset, 0f - num, num);
			}
			else
			{
				_limitedRangeRealAngle = Mathf.Clamp(num2 - GetRealAngle(), 0f - num, num);
			}
			if (limitedRangeRealAngle != _limitedRangeRealAngle)
			{
				SetSpringLimits(_limitedRangeRealAngle - _limitedRangeAngle, _limitedRangeRealAngle + _limitedRangeAngle);
			}
		}
	}

	public TileResultCode SetSpringStiffness(ScriptRowExecutionInfo eInfo, object[] args)
	{
		springStiffness = Util.GetFloatArg(args, 0, 1f);
		return TileResultCode.True;
	}

	public TileResultCode SetRigidity(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (joint == null)
		{
			return TileResultCode.True;
		}
		float num = ((args.Length == 0) ? 1f : ((float)args[0]));
		float num2 = 90f;
		if (num > 0.9f)
		{
			num2 = 0.05f;
		}
		else if (num > 0.8f)
		{
			num2 = 0.25f;
		}
		else if (num > 0.7f)
		{
			num2 = 0.7f;
		}
		else if (num > 0.6f)
		{
			num2 = 1.4f;
		}
		else if (num > 0.5f)
		{
			num2 = 2.5f;
		}
		else if (num > 0.4f)
		{
			num2 = 4.75f;
		}
		else if (num > 0.3f)
		{
			num2 = 7f;
		}
		else if (num > 0.2f)
		{
			num2 = 10f;
		}
		else if (num > 0.1f)
		{
			num2 = 14f;
		}
		else if (num > 0f)
		{
			num2 = 20f;
		}
		else if (springLimit > 0f)
		{
			num2 = springLimit;
		}
		if (_limitedRange || num > 0f)
		{
			bool flag = false;
			if (!_limitedRange)
			{
				flag = true;
				_limitedRange = true;
				_limitedRangeAngle = num2;
				_limitedRangeRealAngle = 0f - GetRealAngle();
			}
			else if (_limitedRangeAngle != num2)
			{
				flag = true;
				_limitedRangeAngle = num2;
			}
			if (flag)
			{
				SetSpringLimits(_limitedRangeRealAngle - _limitedRangeAngle, _limitedRangeRealAngle + _limitedRangeAngle);
			}
		}
		else if (springLimit > 0f)
		{
			SetSpringLimits(0f - springLimit, springLimit);
		}
		else
		{
			SetSpringLimits(-90f, 90f);
		}
		if (springLimit <= 0f)
		{
			if (_limitedRange)
			{
				joint.angularXMotion = ConfigurableJointMotion.Limited;
			}
			else
			{
				joint.angularXMotion = ConfigurableJointMotion.Free;
			}
		}
		return TileResultCode.True;
	}

	public TileResultCode Charge(ScriptRowExecutionInfo eInfo, object[] args)
	{
		Charge(args, eInfo.floatArg);
		return TileResultCode.True;
	}

	public TileResultCode StepCharge(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (eInfo.timer >= 0.25f)
		{
			return TileResultCode.True;
		}
		Charge(args, eInfo.floatArg);
		return TileResultCode.Delayed;
	}

	public TileResultCode FreeSpin(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (joint == null)
		{
			return TileResultCode.True;
		}
		mode = TorsionSpringMode.FreeSpin;
		if (_limitedRange)
		{
			if (springLimit > 0f)
			{
				SetSpringLimits(0f - springLimit, springLimit);
			}
			else
			{
				SetSpringLimits(-90f, 90f);
			}
		}
		_limitedRange = false;
		_limitedRangeAngle = 0.05f;
		_limitedRangeRealAngle = 0f;
		return TileResultCode.True;
	}

	public TileResultCode Release(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (joint == null)
		{
			return TileResultCode.True;
		}
		released = Mathf.Abs(angleOffset) > 0f;
		angleOffset = 0f;
		mode = TorsionSpringMode.Spring;
		if (_limitedRange)
		{
			SetSpringLimits(0f - _limitedRangeAngle, _limitedRangeAngle);
			_limitedRangeRealAngle = 0f;
		}
		return TileResultCode.True;
	}

	public TileResultCode ChargeGreaterThan(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float num = ((args.Length == 0) ? 45f : ((float)args[0]));
		if (((args.Length > 1 && !(bool)args[1]) ? angleOffset : Mathf.Abs(angleOffset)) >= num)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public override void Play2()
	{
		base.Play2();
		CreateFakeRigidbodyBetweenJoints();
	}

	public override void Play()
	{
		base.Play();
		treatAsVehicleStatus = -1;
		List<Block> list = ConnectionsOfType(2, directed: true);
		fakeRotor = null;
		if (jointToJointConnection != null)
		{
			rotor = jointToJointConnection.gameObject;
		}
		else if (list.Count > 0)
		{
			Chunk chunk = (rotorChunk = list[0].chunk);
			rotor = rotorChunk.go;
			ConfigurableJoint[] components = base.chunk.go.GetComponents<ConfigurableJoint>();
			if (components.Length != 0)
			{
				jointToJointConnection = chunk.rb;
				jointOffset = goT.localEulerAngles;
			}
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
			rigidbody.mass = 1f;
			axle.transform.parent = fakeRotor.transform;
			rotor = fakeRotor;
		}
		rotorT = rotor.transform;
		axleAudioSource = axle.GetComponent<AudioSource>();
		if (axleAudioSource == null)
		{
			axleAudioSource = axle.AddComponent<AudioSource>();
			axleAudioSource.playOnAwake = false;
			axleAudioSource.clip = Sound.GetSfx("Torsion Spring Stretch");
			Sound.SetWorldAudioSourceParams(axleAudioSource);
		}
		CreateJoint();
		targetAngle = 0f;
		currentAngle = 0f;
		lastRealAngle = 0f;
		_limitedRange = false;
		_limitedRangeAngle = 0.05f;
		CalculateMassDistributions();
		angleOffset = 0f;
		lockedAngleOffset = 0f;
		locked = false;
		wasLocked = false;
		halfRevolutions = 0;
		CalculateRotorAngleVecType();
		mode = TorsionSpringMode.Spring;
	}

	private void SetSpringLimits(float lowLimit, float highLimit)
	{
		SoftJointLimit softJointLimit = new SoftJointLimit
		{
			bounciness = 0f,
			limit = lowLimit
		};
		joint.lowAngularXLimit = softJointLimit;
		softJointLimit.bounciness = 0f;
		softJointLimit.limit = highLimit;
		joint.highAngularXLimit = softJointLimit;
	}

	private void CreateJoint()
	{
		if (!(chunk.go == rotor))
		{
			joint = chunk.go.AddComponent<ConfigurableJoint>();
			joint.anchor = goT.localPosition;
			joint.axis = goT.right;
			joint.xMotion = ConfigurableJointMotion.Locked;
			joint.yMotion = ConfigurableJointMotion.Locked;
			joint.zMotion = ConfigurableJointMotion.Locked;
			if (springLimit > 0f)
			{
				joint.angularXMotion = ConfigurableJointMotion.Limited;
			}
			else
			{
				joint.angularXMotion = ConfigurableJointMotion.Free;
			}
			joint.angularYMotion = ConfigurableJointMotion.Locked;
			joint.angularZMotion = ConfigurableJointMotion.Locked;
			joint.connectedBody = rotor.GetComponent<Rigidbody>();
			if (springLimit > 0f)
			{
				SetSpringLimits(0f - springLimit, springLimit);
			}
		}
	}

	private void CalculateMassDistributions()
	{
		Chunk ignoreChunk = chunk;
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
		jointToJointConnection = null;
		axle.transform.localRotation = default(Quaternion);
		axle.transform.localScale = Vector3.one;
		axle.GetComponent<Renderer>().enabled = true;
		PlayLoopSound(play: false, GetLoopClip());
		base.Stop(resetBlock);
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

	public override void Update()
	{
		base.Update();
		if (Blocksworld.CurrentState == State.Play && fakeRotor == null && rotor != null && !vanished)
		{
			if (jointToJointConnection != null)
			{
				Quaternion rotation = ((!(jointOffset == Vector3.zero)) ? (jointToJointConnection.rotation * Quaternion.Euler(jointOffset)) : Quaternion.LookRotation(jointToJointConnection.transform.position - goT.position, axle.transform.up));
				axle.transform.rotation = rotation;
			}
			else
			{
				float realAngle = GetRealAngle();
				axle.transform.localRotation = Quaternion.Euler(new Vector3(0f - realAngle, 0f, 0f));
			}
		}
	}

	private float GetRealAngle()
	{
		Vector3 forward = goT.forward;
		Vector3 planeNormal = Vector3.Cross(forward, goT.up);
		Vector3 rotorAngleVec = GetRotorAngleVec();
		rotorAngleVec = Util.ProjectOntoPlane(rotorAngleVec, planeNormal).normalized;
		return Util.AngleBetween(rotorAngleVec, forward, goT.right);
	}

	private void CheckAndSet(Vector3 v2, int type)
	{
		Vector3 forward = goT.forward;
		float f = Vector3.Dot(forward, v2);
		float num = Mathf.Sign(f);
		if (Mathf.Abs(f) > 0.5f)
		{
			rotorAngleVecType = type;
			rotorAngleVecMultiplier = num;
		}
	}

	private void CalculateRotorAngleVecType()
	{
		CheckAndSet(rotorT.forward, 0);
		CheckAndSet(rotorT.up, 1);
		CheckAndSet(rotorT.right, 2);
	}

	public Vector3 GetRotorAngleVec()
	{
		if (rotorT == null)
		{
			BWLog.Info("rotorT null!");
			return rotorAngleVecMultiplier * Vector3.forward;
		}
		return rotorAngleVecType switch
		{
			0 => rotorAngleVecMultiplier * rotorT.forward, 
			1 => rotorAngleVecMultiplier * rotorT.up, 
			2 => rotorAngleVecMultiplier * rotorT.right, 
			_ => rotorAngleVecMultiplier * rotorT.forward, 
		};
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		UpdateTorsionSpringLoopSound();
		if (isTreasure || joint == null)
		{
			return;
		}
		if (joint.connectedBody == null)
		{
			DestroyJoint();
			return;
		}
		if (rotor == null)
		{
			SetMotorVelocity(0f, 10f);
			return;
		}
		float realAngle = GetRealAngle();
		float f = realAngle - lastRealAngle;
		if (Mathf.Abs(f) > 90f)
		{
			if (lastRealAngle < 0f)
			{
				halfRevolutions--;
			}
			else
			{
				halfRevolutions++;
			}
		}
		lastRealAngle = realAngle;
		currentAngle = realAngle + (float)halfRevolutions * 360f;
		if (locked && !wasLocked)
		{
			lockedAngleOffset = angleOffset;
		}
		if (!locked)
		{
			currentAngle += angleOffset;
			lockedAngleOffset = 0f;
		}
		else
		{
			currentAngle += lockedAngleOffset;
		}
		float f2 = targetAngle - currentAngle;
		if (released && Sound.sfxEnabled && !vanished)
		{
			axleAudioSource.pitch = 0.8f + Random.value * 0.4f;
			axleAudioSource.PlayOneShot(Sound.GetSfx("Torsion Spring Stretch"), 0.5f);
		}
		float num = Mathf.Max(chunkMi, rotorChunkMi);
		Rigidbody rb = chunk.rb;
		if (rb != null && !chunkKinematic)
		{
			num = Mathf.Min(num, chunkMi);
		}
		Rigidbody component = rotor.GetComponent<Rigidbody>();
		if (component != null && !rotorChunkKinematic)
		{
			num = Mathf.Min(num, rotorChunkMi);
		}
		float num2 = 0.07f * Mathf.Sqrt(3f * num + 0.3f);
		float maxForce = 0f;
		float v = 0f;
		switch (mode)
		{
		case TorsionSpringMode.Spring:
			maxForce = num2 * springStiffness * Mathf.Min(1200f, Mathf.Abs(f2));
			v = Mathf.Sign(f2) * 50f;
			break;
		case TorsionSpringMode.FreeSpin:
			if (Mathf.Abs(f2) >= 90f)
			{
				maxForce = num2 * springStiffness * (Mathf.Abs(f2) - 90f);
				v = Mathf.Sign(f2) * 50f;
			}
			break;
		}
		SetMotorVelocity(v, maxForce);
		wasLocked = locked;
		locked = false;
		released = false;
		charging = false;
	}

	private void SetMotorVelocity(float v, float maxForce = 1000f)
	{
		JointDrive angularXDrive = default(JointDrive);
		float num = (angularXDrive.maximumForce = maxForce * 100f * 0.5f);
		angularXDrive.positionDamper = ((mode != TorsionSpringMode.FreeSpin) ? num : 0f);
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

	private void UpdateTorsionSpringLoopSound()
	{
		float num = 0.03f;
		if (!charging || broken || vanished || isTreasure)
		{
			num *= -1f;
		}
		turnLoopVol = Mathf.Clamp(turnLoopVol + num, 0f, 0.5f);
		if (sfxLoopUpdateCounter % 5 == 0)
		{
			PlayLoopSound(turnLoopVol > 0.01f, GetLoopClip(), turnLoopVol, null, turnLoopPitch);
			UpdateWithinWaterLPFilter();
			UpdateWithinWaterLPFilter(axle);
		}
		sfxLoopUpdateCounter++;
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
			rotorT = rotor.transform;
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
}
