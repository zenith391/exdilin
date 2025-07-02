using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockSteeringWheel : Block
{
	private enum DriveStates
	{
		IDLE,
		FORWARD,
		REVERSE,
		FORWARD_BRAKING,
		REVERSE_BRAKING
	}

	public static Predicate predicateSteeringWheelMoveAlongMover;

	public static Predicate predicateSteeringWheelMoveLocalAlongMover;

	public static Predicate predicateSteeringWheelMoveLocalAlongTilt;

	public static Predicate predicateSteeringWheelMoverSteer;

	public static Predicate predicateSteeringWheelTiltSteer;

	public static Predicate predicateSteeringWheelTiltDrive;

	public static Predicate predicateSetVehicleType;

	public static Predicate predicateDrive;

	public static Predicate predicateTurn;

	public static Predicate predicateSetupTurn;

	public static Predicate predicateSetupDrive;

	public static Predicate predicateDriveControl;

	private SteeringWheelMetaData metaData;

	private const float DEFAULT_BRAKE_FORCE = 0.5f;

	private const float DEFAULT_DRIVE_FORCE = 17.6f;

	private const float DEFAULT_TURN_ANGLE = 45f;

	private string vehicleType = "Regular";

	private bool hasSetControlType;

	private bool hadSetControlType;

	private string controlType = string.Empty;

	private float brakeEffectiveness = 1f;

	private float storeForce = 17.6f;

	private float storeTurn = 45f;

	private Vector3 origLocalCM;

	private Vector3 origLocalVehicleCenter;

	private bool canChangeDriveState = true;

	private bool beingDriven;

	private bool turnOnSpeedometer;

	private bool speedometerOn;

	private DriveStates driveState;

	public static VehicleDefinitions vehicleDefinitions;

	private static Dictionary<string, VehicleDefinition> vehicleDefsDict;

	private HashSet<Block> chunkBlocks = new HashSet<Block>();

	private List<BlockAbstractWheel> allWheels = new List<BlockAbstractWheel>();

	private List<Block> wheelBlocks = new List<Block>();

	private List<BlockTankTreadsWheel> allMainTankWheels = new List<BlockTankTreadsWheel>();

	private List<Block> jumpBlocks = new List<Block>();

	private List<BlockAbstractWheel> allControlledWheels = new List<BlockAbstractWheel>();

	private List<BlockTankTreadsWheel> allControlledTankWheels = new List<BlockTankTreadsWheel>();

	private List<BlockAbstractWheel> backWheels = new List<BlockAbstractWheel>();

	private List<BlockAbstractWheel> frontWheels = new List<BlockAbstractWheel>();

	private List<BlockAbstractWheel> turnWheels = new List<BlockAbstractWheel>();

	private List<BlockAbstractWheel> inverseTurnWheels = new List<BlockAbstractWheel>();

	private List<BlockAbstractWheel> driveWheels = new List<BlockAbstractWheel>();

	private List<BlockAbstractWheel> inverseDriveWheels = new List<BlockAbstractWheel>();

	private List<BlockAbstractWheel> frontDriveWheels = new List<BlockAbstractWheel>();

	private List<BlockAbstractWheel> backDriveWheels = new List<BlockAbstractWheel>();

	private List<BlockAbstractWheel> frontInverseDriveWheels = new List<BlockAbstractWheel>();

	private List<BlockAbstractWheel> backInverseDriveWheels = new List<BlockAbstractWheel>();

	private List<BlockAbstractWheel> leftTankTurn_Wheels = new List<BlockAbstractWheel>();

	private List<BlockAbstractWheel> rightTankTurn_Wheels = new List<BlockAbstractWheel>();

	private List<BlockAbstractWheel> inverseLeftTankTurn_Wheels = new List<BlockAbstractWheel>();

	private List<BlockAbstractWheel> inverseRightTankTurn_Wheels = new List<BlockAbstractWheel>();

	private List<BlockTankTreadsWheel> drive_TankWheels = new List<BlockTankTreadsWheel>();

	private List<BlockTankTreadsWheel> inverseDrive_TankWheels = new List<BlockTankTreadsWheel>();

	private List<BlockTankTreadsWheel> leftTurn_TankWheels = new List<BlockTankTreadsWheel>();

	private List<BlockTankTreadsWheel> rightTurn_TankWheels = new List<BlockTankTreadsWheel>();

	private List<BlockTankTreadsWheel> inverseLeftTurn_TankWheels = new List<BlockTankTreadsWheel>();

	private List<BlockTankTreadsWheel> inverseRightTurn_TankWheels = new List<BlockTankTreadsWheel>();

	private List<List<Tile>> steeringWheelTiles = new List<List<Tile>>();

	private Vector3 alignUp = Vector3.up;

	private float onGroundFraction;

	private float maxSpeedInc = 0.3f;

	private float averageTurnWheelRadius = 1f;

	private float flipping;

	private int treatAsVehicleStatus = -1;

	private Vector3 moveAlongDirection = Vector3.zero;

	private float moveAlongMaxAngle = 45f;

	private float moveAlongMaxForce;

	private VehicleTurnMode turnMode;

	private VehicleDriveMode driveMode;

	private float helpForceTurnAngle;

	private float turnAngle;

	private float visualAngleTarget;

	private float visualAngle;

	private Vector3 localSteeringAxle;

	private float jumpHeight;

	private int jumpCounter;

	private const int MAX_JUMP_FRAMES = 5;

	private const int JUMP_RELOAD_FRAMES = 40;

	private List<Transform> meshesToTurn;

	private Bounds wheelsBounds;

	private Vector3 vehicleCenter;

	private bool engineSoundOn;

	private float engineSoundRPM;

	private bool setEngineSoundRPM;

	private float engineSoundWheelSpinSpeed;

	private EngineSoundDefinition engineSoundDefinition;

	private UISpeedometer speedometer;

	public static float awdBalanceFront = 0.4f;

	public static float awdBalanceRear = 0.6f;

	private const float steeringWheelBackwardSpeedMod = 0.75f;

	public BlockSteeringWheel(List<List<Tile>> tiles)
		: base(tiles)
	{
		localSteeringAxle = new Vector3(0f, 0f, -1f).normalized;
		meshesToTurn = new List<Transform>();
		wheelsBounds = default(Bounds);
		meshesToTurn.Add(goT.Find("DrivingWheel SW"));
		meshesToTurn.Add(goT.Find("CarHorn SW"));
		meshesToTurn.Add(goT.Find("SteeringWheelConnect SW"));
		engineSoundDefinition = Blocksworld.engineSoundDefinitions.GetEngineSoundDefinition("Default");
		loopName = engineSoundDefinition.loopSFXName;
	}

	public override void OnCreate()
	{
		metaData = go.GetComponent<SteeringWheelMetaData>();
	}

	public static void AddVehicleDefintionToggleChain()
	{
		LoadVehicleDefinitions();
		List<GAF> list = new List<GAF>();
		VehicleDefinition[] definitions = vehicleDefinitions.definitions;
		foreach (VehicleDefinition vehicleDefinition in definitions)
		{
			list.Add(new GAF(predicateSetVehicleType, vehicleDefinition.name));
		}
		TileToggleChain.AddChain(list);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (broken || vanished || isTreasure)
		{
			return;
		}
		UpdateEngineSound();
		speedometerOn = turnOnSpeedometer;
		turnOnSpeedometer = false;
		if (hadSetControlType)
		{
			if (moveAlongMaxForce > 0f)
			{
				if (moveAlongDirection.sqrMagnitude > 1f)
				{
					moveAlongDirection.Normalize();
				}
				MoveAlongDirection(moveAlongDirection, moveAlongMaxAngle, moveAlongMaxForce);
				moveAlongDirection.Set(0f, 0f, 0f);
				moveAlongMaxAngle = 45f;
				moveAlongMaxForce = 0f;
			}
			int num = 0;
			bool flag = jumpCounter < 5 && jumpHeight > 0.01f;
			float jumpYOffset = 0f;
			if (flag)
			{
				float jumpForcePerFrame = Util.GetJumpForcePerFrame(jumpHeight, chunk.rb.mass, 5);
				float num2 = jumpForcePerFrame / chunk.rb.mass;
				jumpYOffset = num2 * Blocksworld.fixedDeltaTime * 0.5f;
			}
			jumpCounter++;
			jumpHeight = 0f;
			for (int i = 0; i < allControlledWheels.Count; i++)
			{
				BlockAbstractWheel blockAbstractWheel = allControlledWheels[i];
				if (blockAbstractWheel.externalControlBlock == this)
				{
					blockAbstractWheel.FixedUpdateDriveAndTurn(jumpYOffset);
				}
				if (blockAbstractWheel.onGround > 0.17f)
				{
					num++;
				}
			}
			for (int j = 0; j < allControlledTankWheels.Count; j++)
			{
				BlockTankTreadsWheel blockTankTreadsWheel = allControlledTankWheels[j];
				if (blockTankTreadsWheel.externalControlBlock == this)
				{
					blockTankTreadsWheel.FixedUpdateDriveAndTurn();
				}
				if (blockTankTreadsWheel.onGround > 0.17f)
				{
					num++;
				}
			}
			int num3 = allControlledWheels.Count + allControlledTankWheels.Count;
			Rigidbody rigidbody = null;
			if (num3 >= 3)
			{
				onGroundFraction = Mathf.Lerp(onGroundFraction, (float)num / (float)num3, 0.2f);
				if (chunk.go != null)
				{
					rigidbody = chunk.rb;
					if (rigidbody != null && !rigidbody.isKinematic)
					{
						float num4 = 1f / Mathf.Max(1f, 4f * rigidbody.angularVelocity.magnitude);
					}
				}
				Vector3 up = goT.up;
				if (onGroundFraction > 0.75f)
				{
					alignUp = (0.95f * alignUp + 0.05f * up).normalized;
				}
				else if (onGroundFraction > 0.25f || flipping > 0f)
				{
					if (rigidbody != null)
					{
						float num5 = Mathf.Clamp(Vector3.Angle(alignUp, up), -45f, 45f);
						if (rigidbody != null && !rigidbody.isKinematic)
						{
							float num6 = 1f / Mathf.Max(1f, 4f * rigidbody.angularVelocity.magnitude);
							Vector3 vector = 5f * (0f - num5) * num6 * rigidbody.mass * Vector3.Cross(alignUp, up).normalized;
							if (flipping > 0f)
							{
								vector *= flipping;
							}
							alignUp = (0.99f * alignUp + 0.01f * Vector3.up).normalized;
						}
					}
					flipping = 0f;
				}
				else if (onGroundFraction < 0.05f && num == 0)
				{
					alignUp = (0.98f * alignUp + 0.02f * Vector3.up).normalized;
				}
			}
			float num7 = ((!(vehicleType == "Utility")) ? 1f : (-1f));
			visualAngleTarget = turnAngle * num7;
			visualAngle = 0.1f * visualAngleTarget + 0.9f * visualAngle;
			turnAngle = 0f;
			helpForceTurnAngle = 0f;
			if (beingDriven)
			{
				beingDriven = false;
			}
			else
			{
				canChangeDriveState = true;
				if (rigidbody != null && Mathf.Abs(rigidbody.velocity.sqrMagnitude) < 3f)
				{
					driveState = DriveStates.IDLE;
				}
			}
		}
		if (hadSetControlType != hasSetControlType)
		{
			BlockSteeringWheel blockSteeringWheel = ((!hasSetControlType) ? null : this);
			foreach (BlockAbstractWheel allControlledWheel in allControlledWheels)
			{
				allControlledWheel.AssignExternalControl(blockSteeringWheel, allControlledWheels);
				allControlledWheel.maxSpeedInc = ((!hasSetControlType) ? 99999f : maxSpeedInc);
			}
			foreach (BlockTankTreadsWheel allControlledTankWheel in allControlledTankWheels)
			{
				allControlledTankWheel.externalControlBlock = blockSteeringWheel;
			}
			ResetHandling();
			if (hasSetControlType)
			{
				ImproveHandling();
			}
			hadSetControlType = hasSetControlType;
		}
		hasSetControlType = false;
		float b = 0f;
		if (!setEngineSoundRPM)
		{
			engineSoundRPM = Mathf.Lerp(engineSoundRPM, 0f, engineSoundDefinition.RPMDecaySpeed * Time.fixedDeltaTime);
			foreach (BlockAbstractWheel allControlledWheel2 in allControlledWheels)
			{
				float magnitude = allControlledWheel2.chunk.rb.angularVelocity.magnitude;
				b = Mathf.Max(magnitude, b);
			}
		}
		setEngineSoundRPM = false;
		engineSoundWheelSpinSpeed = Mathf.Lerp(engineSoundWheelSpinSpeed, b, Time.fixedDeltaTime);
	}

	private void UpdateEngineSound()
	{
		if (!engineSoundOn || Blocksworld.CurrentState != State.Play)
		{
			PlayLoopSound(play: false, GetLoopClip());
			return;
		}
		float a = engineSoundDefinition.baseVolume + engineSoundRPM * engineSoundDefinition.RPMVolumeMod;
		float a2 = engineSoundDefinition.basePitch + engineSoundRPM * engineSoundDefinition.RPMPitchMod;
		float num = Mathf.Clamp(engineSoundWheelSpinSpeed - engineSoundDefinition.wheelSpinThreshold, 0f, engineSoundDefinition.wheelSpinMax);
		float b = engineSoundDefinition.baseVolume + num * engineSoundDefinition.wheelSpinVolumeMod;
		float b2 = engineSoundDefinition.basePitch + num * engineSoundDefinition.wheelSpinPitchMod;
		a = Mathf.Max(a, b);
		a2 = Mathf.Max(a2, b2);
		PlayLoopSound(play: true, GetLoopClip(), a, null, a2);
	}

	public override void Update()
	{
		base.Update();
		if (Blocksworld.CurrentState != State.Play)
		{
			return;
		}
		if (broken || vanished || isTreasure)
		{
			if (speedometer != null)
			{
				speedometer.Hide();
			}
			return;
		}
		for (int i = 0; i < meshesToTurn.Count; i++)
		{
			Transform transform = meshesToTurn[i];
			transform.localRotation = Quaternion.AngleAxis(0f - visualAngle, localSteeringAxle);
		}
		if (speedometerOn && chunk.rb != null)
		{
			if (speedometer == null)
			{
				speedometer = Blocksworld.UI.Overlay.CreateSpeedometer();
			}
			speedometer.SetSpeed(Mathf.Max(chunk.rb.velocity.magnitude - 0.5f, 0f));
			speedometer.Show();
		}
		else if (speedometer != null)
		{
			speedometer.Hide();
		}
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.SetTurnMode", null, (Block b) => ((BlockSteeringWheel)b).SetTurnMode, new Type[1] { typeof(int) }, new string[1] { "Turn Mode" });
		PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.SetDriveMode", null, (Block b) => ((BlockSteeringWheel)b).SetDriveMode, new Type[1] { typeof(int) }, new string[1] { "Drive Mode" });
		predicateSteeringWheelMoverSteer = PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.MoverSteer", null, (Block b) => ((BlockSteeringWheel)b).MoverSteer, new Type[2]
		{
			typeof(string),
			typeof(float)
		}, new string[2] { "Stick Name", "Max Angle" });
		predicateSteeringWheelTiltSteer = PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.TiltSteer", null, (Block b) => ((BlockSteeringWheel)b).TiltSteer, new Type[1] { typeof(float) }, new string[1] { "Max Angle" });
		predicateSteeringWheelTiltDrive = PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.TiltDrive", null, (Block b) => ((BlockSteeringWheel)b).TiltDrive, new Type[1] { typeof(float) }, new string[1] { "Speed" });
		PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.Flip", null, (Block b) => ((BlockSteeringWheel)b).Flip, new Type[1] { typeof(float) }, new string[1] { "Strength" });
		predicateSteeringWheelMoveLocalAlongMover = PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.MoveLocalAlongMover", null, (Block b) => ((BlockSteeringWheel)b).MoveLocalAlongMover, new Type[2]
		{
			typeof(string),
			typeof(float)
		}, new string[2] { "Stick Name", "Max Force" });
		predicateSteeringWheelMoveAlongMover = PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.MoveAlongMover", null, (Block b) => ((BlockSteeringWheel)b).MoveAlongMover, new Type[3]
		{
			typeof(string),
			typeof(float),
			typeof(float)
		}, new string[3] { "Stick Name", "Max Angle", "Max Force" });
		predicateSteeringWheelMoveLocalAlongTilt = PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.MoveLocalAlongTilt", null, (Block b) => ((BlockSteeringWheel)b).MoveLocalAlongTilt, new Type[1] { typeof(float) }, new string[1] { "Max Force" });
		PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.DriveThroughTag", null, (Block b) => ((BlockSteeringWheel)b).DriveThroughTag, new Type[3]
		{
			typeof(string),
			typeof(float),
			typeof(float)
		}, new string[3] { "Tag Name", "Max Angle", "Max Force" });
		PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.AvoidTag", null, (Block b) => ((BlockSteeringWheel)b).AvoidTag, new Type[5]
		{
			typeof(string),
			typeof(float),
			typeof(float),
			typeof(float),
			typeof(int)
		}, new string[4] { "Tag Name", "Distance", "Max Angle", "Max Force" });
		PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.JumpHeight", null, (Block b) => ((BlockSteeringWheel)b).Jump, new Type[1] { typeof(float) }, new string[1] { "Height" });
		PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.TurnAlongMover", null, (Block b) => ((BlockSteeringWheel)b).TurnAlongMover, new Type[3]
		{
			typeof(string),
			typeof(float),
			typeof(int)
		}, new string[2] { "Stick Name", "Max Angle" });
		PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.DriveAlongMover", null, (Block b) => ((BlockSteeringWheel)b).DriveAlongMover, new Type[3]
		{
			typeof(string),
			typeof(float),
			typeof(int)
		}, new string[2] { "Stick Name", "Max Force" });
		predicateTurn = PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.Turn", null, (Block b) => ((BlockSteeringWheel)b).Turn, new Type[1] { typeof(float) }, new string[1] { "Angle" });
		predicateDrive = PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.Drive", null, (Block b) => ((BlockSteeringWheel)b).Drive, new Type[1] { typeof(float) }, new string[1] { "Force" });
		PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.BrakeEffectiveness", (Block b) => ((BlockSteeringWheel)b).IsBrakeEffectiveness, (Block b) => ((BlockSteeringWheel)b).SetBrakeEffectiveness, new Type[1] { typeof(float) }, new string[1] { "Effectiveness" });
		PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.Brake", (Block b) => ((BlockSteeringWheel)b).IsBraking);
		PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.Driving", (Block b) => ((BlockSteeringWheel)b).IsDriving);
		PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.Idling", (Block b) => ((BlockSteeringWheel)b).IsIdling);
		PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.Reversing", (Block b) => ((BlockSteeringWheel)b).IsReversing);
		PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.Turning", (Block b) => ((BlockSteeringWheel)b).IsTurning, null, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.Turning", (Block b) => ((BlockSteeringWheel)b).IsTurning, null, new Type[1] { typeof(float) });
		predicateSetupTurn = PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.SetupTurn", null, (Block b) => ((BlockSteeringWheel)b).SetupTurn, new Type[1] { typeof(float) }, new string[1] { "Angle" });
		predicateSetupDrive = PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.SetupDriver", null, (Block b) => ((BlockSteeringWheel)b).SetupDriver, new Type[1] { typeof(float) }, new string[1] { "Force" });
		predicateDriveControl = PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.DriveControlType", null, (Block b) => ((BlockSteeringWheel)b).SetDriveControl, new Type[1] { typeof(string) }, new string[5] { "Mover", "Buttons", "Combined", "Tilt", "TiltCombined" });
		predicateSetVehicleType = PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.SetVehicleType", null, (Block b) => ((BlockSteeringWheel)b).SetVehicleType, new Type[1] { typeof(string) }, new string[1] { "Vehicle type" });
		PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.SetBallastFraction", null, (Block b) => ((BlockSteeringWheel)b).SetBallastFraction, new Type[1] { typeof(float) }, new string[1] { "Fraction" });
		PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.ShowSpeedometer", null, (Block b) => ((BlockSteeringWheel)b).ShowSpeedometer);
		PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.SuspensionDamper", null, (Block b) => ((BlockSteeringWheel)b).SetSuspensionDamper, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.SuspensionHeight", null, (Block b) => ((BlockSteeringWheel)b).SetSuspensionHeight, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.SuspensionLength", null, (Block b) => ((BlockSteeringWheel)b).SetSuspensionLength, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockSteeringWheel>("SteeringWheel.SuspensionSpring", null, (Block b) => ((BlockSteeringWheel)b).SetSuspensionSpring, new Type[1] { typeof(float) });
		List<GAF> list = new List<GAF>();
		list.Add(new GAF(predicateDriveControl, "Combined"));
		Block.AddSimpleDefaultTiles(list, "Steering Wheel");
	}

	private void TurnAlongDirection(Vector3 direction, float maxAngle)
	{
		Transform transform = goT;
		float value = Util.AngleBetween(Util.ProjectOntoPlane(transform.forward, Vector3.up), direction, transform.up);
		float num = Mathf.Clamp(value, 0f - maxAngle, maxAngle);
		TurnStandard(0f - num);
	}

	private void DriveAlongDirection(Vector3 direction, float maxForce)
	{
		Transform transform = goT;
		float num = Vector3.Dot(transform.forward, direction);
		num = ((!(num >= 0f)) ? Mathf.Min(-0.25f, num) : Mathf.Max(0.25f, num));
		float force = maxForce * num;
		Drive(force);
	}

	private void MoveAlongDirection(Vector3 direction, float maxAngle, float maxForce)
	{
		Transform transform = goT;
		Vector3 forward = transform.forward;
		float num = 0.75f;
		float num2 = Vector3.Dot(forward, direction.normalized);
		if (num2 > 0f - num)
		{
			num2 = (num2 + num) / (1f + num);
		}
		else if (num2 < 0f - num)
		{
			num2 = (num2 + num) / (1f - num);
		}
		float magnitude = direction.magnitude;
		float num3 = magnitude * num2;
		float num4 = magnitude * Mathf.Sign(num2);
		Vector3 zero = Vector3.zero;
		Rigidbody rb = chunk.rb;
		if (rb != null)
		{
			zero = rb.velocity;
			float num5 = Vector3.Dot(rb.angularVelocity, transform.up);
		}
		float num6 = maxForce * num3;
		Drive(0f - num6);
	}

	private void UpdateDriveState(float force)
	{
		if (canChangeDriveState || controlType == "Mover")
		{
			switch (driveState)
			{
			case DriveStates.IDLE:
			case DriveStates.FORWARD_BRAKING:
			case DriveStates.REVERSE_BRAKING:
				driveState = ((!(force >= 0f)) ? DriveStates.FORWARD : ((!(force <= 0f)) ? DriveStates.REVERSE : DriveStates.IDLE));
				break;
			case DriveStates.FORWARD:
				if (force > 0f)
				{
					driveState = DriveStates.FORWARD_BRAKING;
				}
				break;
			case DriveStates.REVERSE:
				if (force < 0f)
				{
					driveState = DriveStates.REVERSE_BRAKING;
				}
				break;
			}
		}
		else if (force < 0f)
		{
			if (driveState == DriveStates.FORWARD_BRAKING)
			{
				driveState = DriveStates.FORWARD;
			}
			else if (driveState == DriveStates.REVERSE)
			{
				driveState = DriveStates.REVERSE_BRAKING;
			}
		}
		else if (force > 0f)
		{
			if (driveState == DriveStates.REVERSE_BRAKING)
			{
				driveState = DriveStates.REVERSE;
			}
			else if (driveState == DriveStates.FORWARD)
			{
				driveState = DriveStates.FORWARD_BRAKING;
			}
		}
		canChangeDriveState = false;
	}

	private void Drive(float force)
	{
		beingDriven = true;
		UpdateDriveState(force);
		if (driveState == DriveStates.FORWARD_BRAKING || driveState == DriveStates.REVERSE_BRAKING)
		{
			float f = brakeEffectiveness * 0.5f;
			BrakeAll(driveWheels, f);
			BrakeAll(inverseDriveWheels, f);
		}
		else
		{
			engineSoundRPM = Mathf.Lerp(engineSoundRPM, engineSoundDefinition.maximumRPM, engineSoundDefinition.RPMIncreaseSpeed * Time.fixedDeltaTime);
			setEngineSoundRPM = true;
			float num = 2f / Mathf.Max(1f, frontDriveWheels.Count + frontInverseDriveWheels.Count);
			float num2 = 2f / Mathf.Max(1f, backDriveWheels.Count + backInverseDriveWheels.Count);
			switch (driveMode)
			{
			case VehicleDriveMode.BACK:
				DriveAll(backDriveWheels, force * num2);
				DriveAll(backInverseDriveWheels, (0f - force) * num2);
				break;
			case VehicleDriveMode.FRONT:
				DriveAll(frontDriveWheels, force * num);
				DriveAll(frontInverseDriveWheels, (0f - force) * num);
				break;
			case VehicleDriveMode.ALL:
				DriveAll(frontDriveWheels, force * awdBalanceFront * num);
				DriveAll(frontInverseDriveWheels, (0f - force) * awdBalanceFront * num);
				DriveAll(backDriveWheels, force * awdBalanceRear * num2);
				DriveAll(backInverseDriveWheels, (0f - force) * awdBalanceRear * num2);
				break;
			}
		}
		DriveAll(drive_TankWheels, force);
		DriveAll(inverseDrive_TankWheels, (0f - force) * 0.25f);
	}

	private void SetDefinitionForAll(WheelDefinition def, HashSet<BlockAbstractWheel> done, params List<BlockAbstractWheel>[] wheelLists)
	{
		foreach (List<BlockAbstractWheel> list in wheelLists)
		{
			for (int j = 0; j < list.Count; j++)
			{
				BlockAbstractWheel blockAbstractWheel = list[j];
				if (!done.Contains(blockAbstractWheel))
				{
					blockAbstractWheel.SetWheelDefinitionData(def);
					done.Add(blockAbstractWheel);
				}
			}
		}
	}

	public static void LoadVehicleDefinitions()
	{
		if (!(vehicleDefinitions != null))
		{
			vehicleDefinitions = Resources.Load<VehicleDefinitions>("VehicleDefinitions");
			vehicleDefsDict = new Dictionary<string, VehicleDefinition>();
			VehicleDefinition[] definitions = vehicleDefinitions.definitions;
			foreach (VehicleDefinition vehicleDefinition in definitions)
			{
				vehicleDefsDict[vehicleDefinition.name] = vehicleDefinition;
			}
		}
	}

	public static VehicleDefinition FindVehicleDefinition(string name)
	{
		LoadVehicleDefinitions();
		if (vehicleDefsDict.TryGetValue(name, out var value))
		{
			return value;
		}
		return vehicleDefinitions.definitions[0];
	}

	private void UpdateVehicleTypeValues()
	{
		VehicleDefinition vehicleDefinition = FindVehicleDefinition(vehicleType);
		turnMode = vehicleDefinition.turnMode;
		driveMode = vehicleDefinition.driveMode;
		WheelDefinition wheelDefinition = BlockAbstractWheel.FindWheelDefinition(vehicleDefinition.wheelDefinitionName);
		WheelDefinition def = wheelDefinition;
		WheelDefinition def2 = wheelDefinition;
		if (!string.IsNullOrEmpty(vehicleDefinition.backWheelDefinitionName))
		{
			def = BlockAbstractWheel.FindWheelDefinition(vehicleDefinition.backWheelDefinitionName);
		}
		if (!string.IsNullOrEmpty(vehicleDefinition.backWheelDefinitionName))
		{
			def2 = BlockAbstractWheel.FindWheelDefinition(vehicleDefinition.frontWheelDefinitionName);
		}
		HashSet<BlockAbstractWheel> done = new HashSet<BlockAbstractWheel>();
		SetDefinitionForAll(def2, done, frontWheels);
		SetDefinitionForAll(def, done, backWheels);
		SetBallastFraction(vehicleDefinition.ballastFraction);
	}

	public TileResultCode SetVehicleType(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string text = vehicleType;
		vehicleType = Util.GetStringArg(args, 0, string.Empty);
		if (vehicleType != text)
		{
			UpdateVehicleTypeValues();
		}
		return TileResultCode.True;
	}

	public TileResultCode SetDriveControl(ScriptRowExecutionInfo eInfo, object[] args)
	{
		controlType = Util.GetStringArg(args, 0, "Mover");
		hasSetControlType = true;
		return TileResultCode.True;
	}

	public TileResultCode SetBallastFraction(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float floatArg = Util.GetFloatArg(args, 0, 0.5f);
		SetBallastFraction(floatArg);
		return TileResultCode.True;
	}

	public TileResultCode SetSuspensionHeight(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float externalSuspensionHeight = Util.GetFloatArg(args, 0, 0.25f) * eInfo.floatArg;
		for (int i = 0; i < allWheels.Count; i++)
		{
			allWheels[i].SetExternalSuspensionHeight(externalSuspensionHeight);
		}
		return TileResultCode.True;
	}

	public TileResultCode SetSuspensionLength(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float floatArg = Util.GetFloatArg(args, 0, 0.5f);
		for (int i = 0; i < allWheels.Count; i++)
		{
			allWheels[i].SetExternalSuspensionLength(floatArg);
		}
		return TileResultCode.True;
	}

	public TileResultCode SetSuspensionDamper(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float floatArg = Util.GetFloatArg(args, 0, 2f);
		for (int i = 0; i < allWheels.Count; i++)
		{
			allWheels[i].SetExternalSuspensionDamper(floatArg);
		}
		return TileResultCode.True;
	}

	public TileResultCode SetSuspensionSpring(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float floatArg = Util.GetFloatArg(args, 0, 5f);
		for (int i = 0; i < allWheels.Count; i++)
		{
			allWheels[i].SetExternalSuspensionSpring(floatArg);
		}
		return TileResultCode.True;
	}

	public TileResultCode SetTurnMode(ScriptRowExecutionInfo eInfo, object[] args)
	{
		turnMode = (VehicleTurnMode)Util.GetIntArg(args, 0, 0);
		return TileResultCode.True;
	}

	public TileResultCode SetDriveMode(ScriptRowExecutionInfo eInfo, object[] args)
	{
		driveMode = (VehicleDriveMode)Util.GetIntArg(args, 0, 0);
		return TileResultCode.True;
	}

	public TileResultCode MoveAlongMover(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, "L");
		Blocksworld.UI.Controls.EnableDPad(stringArg, MoverDirectionMask.ALL);
		Vector3 worldDPadOffset = Blocksworld.UI.Controls.GetWorldDPadOffset(stringArg);
		if (worldDPadOffset.sqrMagnitude > 0.01f)
		{
			float floatArg = Util.GetFloatArg(args, 1, 45f);
			float floatArg2 = Util.GetFloatArg(args, 2, 20f);
			moveAlongDirection += worldDPadOffset;
			moveAlongMaxAngle = Mathf.Max(moveAlongMaxAngle, floatArg);
			moveAlongMaxForce = Mathf.Max(moveAlongMaxForce, floatArg2);
		}
		return TileResultCode.True;
	}

	public TileResultCode Flip(ScriptRowExecutionInfo eInfo, object[] args)
	{
		flipping += Util.GetFloatArg(args, 0, 1f);
		return TileResultCode.True;
	}

	public TileResultCode MoverSteer(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, "L");
		Blocksworld.UI.Controls.EnableDPad(stringArg, MoverDirectionMask.LEFT | MoverDirectionMask.RIGHT);
		Vector2 normalizedDPadOffset = Blocksworld.UI.Controls.GetNormalizedDPadOffset(stringArg);
		if (normalizedDPadOffset.x * normalizedDPadOffset.x > 0.01f)
		{
			float floatArg = Util.GetFloatArg(args, 1, 45f);
			float angle = (0f - normalizedDPadOffset.x) * floatArg;
			Turn(angle);
		}
		return TileResultCode.True;
	}

	public TileResultCode TiltSteer(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (!TiltManager.Instance.IsMonitoring())
		{
			return TileResultCode.Delayed;
		}
		float tiltTwist = TiltManager.Instance.GetTiltTwist();
		float x = TiltManager.Instance.GetRelativeGravityVector().x;
		float f = ((Mathf.Abs(tiltTwist) <= Mathf.Abs(x)) ? (0f - x) : (0f - tiltTwist));
		float value = Mathf.Asin(f) * 57.29578f;
		float floatArg = Util.GetFloatArg(args, 1, 45f);
		value = Mathf.Clamp(value, 0f - floatArg, floatArg);
		Blocksworld.UI.Controls.UpdateTiltPrompt();
		Turn(value);
		return TileResultCode.True;
	}

	public TileResultCode TiltDrive(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (!TiltManager.Instance.IsMonitoring())
		{
			return TileResultCode.Delayed;
		}
		float num = 0f - TiltManager.Instance.GetRelativeGravityVector().y;
		if (num < 0f)
		{
			num *= 0.75f;
		}
		float num2 = 3f * Util.GetFloatArg(args, 0, 1f);
		Drive(num2 * num);
		return TileResultCode.True;
	}

	public TileResultCode MoveLocalAlongMover(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, "L");
		if (go == null)
		{
			return TileResultCode.True;
		}
		Blocksworld.UI.Controls.EnableDPad(stringArg, MoverDirectionMask.ALL);
		Vector2 normalizedDPadOffset = Blocksworld.UI.Controls.GetNormalizedDPadOffset(stringArg);
		if (normalizedDPadOffset.y < 0f)
		{
			normalizedDPadOffset *= 0.75f;
		}
		Vector3 vector = goT.forward * normalizedDPadOffset.y;
		if (vector.sqrMagnitude > 0.01f)
		{
			float b = storeForce;
			moveAlongDirection += vector;
			moveAlongMaxAngle = 45f;
			moveAlongMaxForce = Mathf.Max(moveAlongMaxForce, b);
		}
		return TileResultCode.True;
	}

	public TileResultCode MoveLocalAlongTilt(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (go == null)
		{
			return TileResultCode.True;
		}
		if (!TiltManager.Instance.IsMonitoring())
		{
			return TileResultCode.Delayed;
		}
		float num = TiltManager.Instance.GetRelativeGravityVector().y;
		if (num < 0f)
		{
			num *= 0.75f;
		}
		Vector3 vector = goT.forward * num;
		if (vector.sqrMagnitude > 0.01f)
		{
			float b = storeForce;
			moveAlongDirection += vector;
			moveAlongMaxAngle = 45f;
			moveAlongMaxForce = Mathf.Max(moveAlongMaxForce, b);
		}
		return TileResultCode.True;
	}

	public TileResultCode DriveThroughTag(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		Vector3 position = goT.position;
		if (TagManager.TryGetClosestBlockWithTag(stringArg, position, out var block, chunkBlocks))
		{
			Vector3 vector = block.goT.position - position;
			if (vector.sqrMagnitude > 0.01f)
			{
				float floatArg = Util.GetFloatArg(args, 1, 45f);
				float floatArg2 = Util.GetFloatArg(args, 2, 20f);
				moveAlongDirection += vector.normalized;
				moveAlongMaxAngle = Mathf.Max(moveAlongMaxAngle, floatArg);
				moveAlongMaxForce = Mathf.Max(moveAlongMaxForce, floatArg2);
			}
		}
		return TileResultCode.True;
	}

	public TileResultCode AvoidTag(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		Vector3 position = goT.position;
		if (TagManager.TryGetClosestBlockWithTag(stringArg, position, out var block, chunkBlocks))
		{
			Vector3 vector = ((!(block.size.sqrMagnitude > 4f)) ? block.goT.position : block.go.GetComponent<Collider>().ClosestPointOnBounds(position));
			Vector3 vector2 = vector - position;
			float magnitude = vector2.magnitude;
			float floatArg = Util.GetFloatArg(args, 1, 10f);
			if (magnitude > 0.01f && magnitude < floatArg)
			{
				float floatArg2 = Util.GetFloatArg(args, 2, 45f);
				float floatArg3 = Util.GetFloatArg(args, 3, 10f);
				Vector3 vector3 = vector2 / magnitude;
				float num = 2f * Mathf.Clamp(1f - magnitude / floatArg, 0f, 1f);
				moveAlongDirection -= num * vector3;
				moveAlongMaxAngle = Mathf.Max(moveAlongMaxAngle, floatArg2);
				moveAlongMaxForce = Mathf.Max(moveAlongMaxForce, floatArg3);
			}
		}
		return TileResultCode.True;
	}

	public TileResultCode TurnAlongMover(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, "L");
		Blocksworld.UI.Controls.EnableDPad(stringArg, MoverDirectionMask.ALL);
		Vector3 worldDPadOffset = Blocksworld.UI.Controls.GetWorldDPadOffset(stringArg);
		if (worldDPadOffset.x * worldDPadOffset.x > 0.01f)
		{
			float floatArg = Util.GetFloatArg(args, 1, 20f);
			TurnAlongDirection(worldDPadOffset, floatArg);
		}
		return TileResultCode.True;
	}

	public TileResultCode DriveAlongMover(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, "L");
		Blocksworld.UI.Controls.EnableDPad(stringArg, MoverDirectionMask.ALL);
		Vector3 worldDPadOffset = Blocksworld.UI.Controls.GetWorldDPadOffset(stringArg);
		if (worldDPadOffset.y * worldDPadOffset.y > 0.01f)
		{
			float floatArg = Util.GetFloatArg(args, 1, 20f);
			DriveAlongDirection(worldDPadOffset, 0f - floatArg);
		}
		return TileResultCode.True;
	}

	public TileResultCode Drive(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float num = eInfo.floatArg * Util.GetFloatArg(args, 0, 20f);
		float force = Mathf.Sign(0f - num) * storeForce;
		Drive(force);
		return TileResultCode.True;
	}

	public TileResultCode IsBrakeEffectiveness(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float num = Util.GetFloatArg(args, 0, 1f) * 0.01f;
		if (num == brakeEffectiveness)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode SetBrakeEffectiveness(ScriptRowExecutionInfo eInfo, object[] args)
	{
		brakeEffectiveness = Util.GetFloatArg(args, 0, 1f) * eInfo.floatArg * 0.01f;
		return TileResultCode.True;
	}

	public TileResultCode IsBraking(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (driveState == DriveStates.FORWARD_BRAKING || driveState == DriveStates.REVERSE_BRAKING)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsDriving(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (driveState == DriveStates.FORWARD || driveState == DriveStates.FORWARD_BRAKING)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsIdling(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (driveState == DriveStates.IDLE)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsReversing(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (driveState == DriveStates.REVERSE || driveState == DriveStates.REVERSE_BRAKING)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsTurning(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float floatArg = Util.GetFloatArg(args, 0, 0f);
		bool flag = false;
		if (floatArg < 0f)
		{
			flag = visualAngle > 0f - floatArg;
		}
		else if (floatArg > 0f)
		{
			flag = visualAngle < 0f - floatArg;
		}
		if (flag)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode ShowSpeedometer(ScriptRowExecutionInfo eInfo, object[] args)
	{
		turnOnSpeedometer = true;
		return TileResultCode.True;
	}

	private void TurnWheel(BlockAbstractWheel w, float angle)
	{
		if (!w.isSpareTire)
		{
			w.Turn(angle);
		}
	}

	private void TurnStandard(float angle)
	{
		for (int i = 0; i < turnWheels.Count; i++)
		{
			BlockAbstractWheel w = turnWheels[i];
			TurnWheel(w, 0f - angle);
		}
		turnAngle += angle;
	}

	private void TurnAll(float angle)
	{
		for (int i = 0; i < turnWheels.Count; i++)
		{
			BlockAbstractWheel w = turnWheels[i];
			TurnWheel(w, 0f - angle);
		}
		for (int j = 0; j < inverseTurnWheels.Count; j++)
		{
			BlockAbstractWheel w2 = inverseTurnWheels[j];
			TurnWheel(w2, angle);
		}
		turnAngle += angle;
	}

	private void TurnBack(float angle)
	{
		for (int i = 0; i < inverseTurnWheels.Count; i++)
		{
			BlockAbstractWheel w = inverseTurnWheels[i];
			TurnWheel(w, 0f - angle);
		}
		turnAngle += angle;
	}

	private void TankTurnAll(float f)
	{
		DriveAll(leftTankTurn_Wheels, f, drivenToTurn: true);
		DriveAll(rightTankTurn_Wheels, 0f - f, drivenToTurn: true);
		DriveAll(inverseLeftTankTurn_Wheels, f, drivenToTurn: true);
		DriveAll(inverseRightTankTurn_Wheels, 0f - f, drivenToTurn: true);
		turnAngle += f * 3f;
	}

	private void Turn_TankWheels(float f)
	{
		DriveAll(leftTurn_TankWheels, f);
		DriveAll(rightTurn_TankWheels, 0f - f);
		DriveAll(inverseLeftTurn_TankWheels, f);
		DriveAll(inverseRightTurn_TankWheels, 0f - f);
	}

	private void Turn(float angle)
	{
		float num = angle * 0.5f * storeTurn / 45f;
		float num2 = 1f;
		if (chunk.rb != null)
		{
			num2 = 1f / Mathf.Clamp(chunk.rb.velocity.magnitude / 50f, 1.5f, 3f);
		}
		angle = Mathf.Clamp(angle, (0f - storeTurn) * num2, storeTurn * num2);
		switch (turnMode)
		{
		case VehicleTurnMode.FRONT:
			TurnStandard(angle);
			break;
		case VehicleTurnMode.BACK:
			TurnBack(0f - angle);
			break;
		case VehicleTurnMode.FRONT_AND_BACK:
			TurnAll(angle);
			break;
		case VehicleTurnMode.TANK:
			TankTurnAll(num);
			break;
		}
		Turn_TankWheels(num * 0.25f);
		helpForceTurnAngle += angle;
	}

	public TileResultCode Turn(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float num = Mathf.Min(1f, 2f * eInfo.floatArg) * Util.GetFloatArg(args, 0, 45f);
		float angle = 0f - num;
		Turn(angle);
		return TileResultCode.True;
	}

	public TileResultCode SetupTurn(ScriptRowExecutionInfo eInfo, object[] args)
	{
		storeTurn = Mathf.Min(1f, 2f * eInfo.floatArg) * Util.GetFloatArg(args, 0, 45f);
		return TileResultCode.True;
	}

	public TileResultCode SetupDriver(ScriptRowExecutionInfo eInfo, object[] args)
	{
		storeForce = eInfo.floatArg * Util.GetFloatArg(args, 0, 20f);
		storeForce = 10f + 1.5f * storeForce;
		return TileResultCode.True;
	}

	private void BrakeAll(List<BlockAbstractWheel> wheels, float f)
	{
		for (int i = 0; i < wheels.Count; i++)
		{
			BlockAbstractWheel blockAbstractWheel = wheels[i];
			if (!blockAbstractWheel.isSpareTire)
			{
				blockAbstractWheel.Brake(f);
			}
		}
	}

	private void DriveAll(List<BlockAbstractWheel> wheels, float f, bool drivenToTurn = false)
	{
		for (int i = 0; i < wheels.Count; i++)
		{
			BlockAbstractWheel blockAbstractWheel = wheels[i];
			if (!blockAbstractWheel.isSpareTire)
			{
				blockAbstractWheel.Drive(f, drivenToTurn);
			}
		}
	}

	private void DriveAll(List<BlockTankTreadsWheel> wheels, float f)
	{
		for (int i = 0; i < wheels.Count; i++)
		{
			BlockTankTreadsWheel blockTankTreadsWheel = wheels[i];
			blockTankTreadsWheel.Drive(f);
		}
	}

	private void SpinAll(List<BlockAbstractWheel> wheels)
	{
		for (int i = 0; i < wheels.Count; i++)
		{
			BlockAbstractWheel blockAbstractWheel = wheels[i];
			if (!blockAbstractWheel.isSpareTire)
			{
				blockAbstractWheel.KeepSpinning();
			}
		}
	}

	public TileResultCode Jump(ScriptRowExecutionInfo eInfo, object[] args)
	{
		jumpHeight += Util.GetFloatArg(args, 0, 0f) * eInfo.floatArg;
		if (jumpCounter - 5 > 40)
		{
			jumpCounter = 0;
		}
		return TileResultCode.True;
	}

	private void ClearFunctionalWheels()
	{
		allControlledWheels.Clear();
		allControlledTankWheels.Clear();
		turnWheels.Clear();
		inverseTurnWheels.Clear();
		driveWheels.Clear();
		inverseDriveWheels.Clear();
		leftTankTurn_Wheels.Clear();
		rightTankTurn_Wheels.Clear();
		inverseLeftTankTurn_Wheels.Clear();
		inverseRightTankTurn_Wheels.Clear();
		frontDriveWheels.Clear();
		backDriveWheels.Clear();
		frontInverseDriveWheels.Clear();
		backInverseDriveWheels.Clear();
		drive_TankWheels.Clear();
		inverseDrive_TankWheels.Clear();
		leftTurn_TankWheels.Clear();
		rightTurn_TankWheels.Clear();
		inverseLeftTurn_TankWheels.Clear();
		inverseRightTurn_TankWheels.Clear();
		frontWheels.Clear();
		backWheels.Clear();
	}

	private void GatherWheelFunctions()
	{
		ClearFunctionalWheels();
		Transform transform = goT;
		Vector3 right = transform.right;
		foreach (BlockTankTreadsWheel allMainTankWheel in allMainTankWheels)
		{
			Transform transform2 = allMainTankWheel.goT;
			Vector3 position = transform2.position;
			Vector3 right2 = transform2.right;
			Vector3 v = position - vehicleCenter;
			Vector3 vector = transform.worldToLocalMatrix.MultiplyVector(v);
			float num = Vector3.Dot(right2, right);
			if (!(Mathf.Abs(num) > 0.1f))
			{
				continue;
			}
			if (num < 0f)
			{
				drive_TankWheels.Add(allMainTankWheel);
				if (vector.x < -0.1f)
				{
					leftTurn_TankWheels.Add(allMainTankWheel);
				}
				else if (vector.x > 0.1f)
				{
					rightTurn_TankWheels.Add(allMainTankWheel);
				}
			}
			else
			{
				inverseDrive_TankWheels.Add(allMainTankWheel);
				if (vector.x > 0.1f)
				{
					inverseLeftTurn_TankWheels.Add(allMainTankWheel);
				}
				else if (vector.x < -0.1f)
				{
					inverseRightTurn_TankWheels.Add(allMainTankWheel);
				}
			}
			allControlledTankWheels.Add(allMainTankWheel);
		}
		averageTurnWheelRadius = 0f;
		foreach (BlockAbstractWheel allWheel in allWheels)
		{
			Transform transform3 = allWheel.goT;
			Vector3 position2 = transform3.position;
			Vector3 right3 = transform3.right;
			Vector3 v2 = position2 - vehicleCenter;
			Vector3 vector2 = transform.worldToLocalMatrix.MultiplyVector(v2);
			bool flag = false;
			if (vector2.z < -0.4f)
			{
				inverseTurnWheels.Add(allWheel);
				flag = true;
			}
			else if (vector2.z > 0.4f)
			{
				turnWheels.Add(allWheel);
				flag = true;
				averageTurnWheelRadius += allWheel.GetRadius();
			}
			bool flag2 = vector2.z >= 0f;
			float num2 = Vector3.Dot(right3, right);
			if (Mathf.Abs(num2) > 0.1f)
			{
				if (num2 < 0f)
				{
					driveWheels.Add(allWheel);
					if (flag2)
					{
						frontDriveWheels.Add(allWheel);
					}
					if (!flag2)
					{
						backDriveWheels.Add(allWheel);
					}
					flag = true;
					if (vector2.x < -0.1f)
					{
						leftTankTurn_Wheels.Add(allWheel);
					}
					else if (vector2.x > 0.1f)
					{
						rightTankTurn_Wheels.Add(allWheel);
					}
				}
				else
				{
					inverseDriveWheels.Add(allWheel);
					if (flag2)
					{
						frontInverseDriveWheels.Add(allWheel);
					}
					if (!flag2)
					{
						backInverseDriveWheels.Add(allWheel);
					}
					flag = true;
					if (vector2.x > 0.1f)
					{
						inverseLeftTankTurn_Wheels.Add(allWheel);
					}
					else if (vector2.x < -0.1f)
					{
						inverseRightTankTurn_Wheels.Add(allWheel);
					}
				}
			}
			if (flag)
			{
				allControlledWheels.Add(allWheel);
			}
			if (flag2)
			{
				frontWheels.Add(allWheel);
			}
			else
			{
				backWheels.Add(allWheel);
			}
		}
		if (turnWheels.Count > 0)
		{
			averageTurnWheelRadius /= turnWheels.Count;
		}
		else
		{
			averageTurnWheelRadius = 1f;
		}
	}

	private void GetEngineSoundFromWheels()
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		foreach (BlockAbstractWheel allWheel in allWheels)
		{
			string text = allWheel.GetBlockMetaData().preferredEngineSound;
			if (string.IsNullOrEmpty(text))
			{
				text = "Default";
			}
			int num = ((!driveWheels.Contains(allWheel)) ? 1 : 2);
			if (!dictionary.ContainsKey(text))
			{
				dictionary[text] = 0;
			}
			dictionary[text] += num;
		}
		int num2 = -1;
		List<string> list = new List<string> { "Default" };
		foreach (KeyValuePair<string, int> item in dictionary)
		{
			if (item.Value > num2)
			{
				num2 = item.Value;
				list.Insert(0, item.Key);
			}
		}
		foreach (string item2 in list)
		{
			EngineSoundDefinition engineSoundDefinition = Blocksworld.engineSoundDefinitions.GetEngineSoundDefinition(item2);
			if (engineSoundDefinition != null)
			{
				this.engineSoundDefinition = engineSoundDefinition;
				loopName = this.engineSoundDefinition.loopSFXName;
				break;
			}
		}
	}

	private static List<List<Tile>> GetDriveControlTiles()
	{
		List<List<Tile>> list = new List<List<Tile>>();
		list.Add(new List<Tile>
		{
			new Tile(Block.predicateSendCustomSignalModel, "BSW_Mover"),
			Block.ThenTile(),
			new Tile(predicateSteeringWheelMoveLocalAlongMover, "L", 90f),
			new Tile(predicateSteeringWheelMoverSteer, "L", 90f)
		});
		list.Add(new List<Tile>
		{
			new Tile(Block.predicateSendCustomSignalModel, "BSW_Buttons"),
			new Tile(Block.predicateButton, "Up"),
			Block.ThenTile(),
			new Tile(predicateDrive, 1f)
		});
		list.Add(new List<Tile>
		{
			new Tile(Block.predicateSendCustomSignalModel, "BSW_Buttons"),
			new Tile(Block.predicateButton, "Down"),
			Block.ThenTile(),
			new Tile(predicateDrive, -0.75f)
		});
		list.Add(new List<Tile>
		{
			new Tile(Block.predicateSendCustomSignalModel, "BSW_Buttons"),
			new Tile(Block.predicateButton, "Left"),
			Block.ThenTile(),
			new Tile(predicateTurn, -90f)
		});
		list.Add(new List<Tile>
		{
			new Tile(Block.predicateSendCustomSignalModel, "BSW_Buttons"),
			new Tile(Block.predicateButton, "Right"),
			Block.ThenTile(),
			new Tile(predicateTurn, 90f)
		});
		list.Add(new List<Tile>
		{
			new Tile(Block.predicateSendCustomSignalModel, "BSW_Combined"),
			new Tile(Block.predicateButton, "Up"),
			Block.ThenTile(),
			new Tile(predicateDrive, 1f)
		});
		list.Add(new List<Tile>
		{
			new Tile(Block.predicateSendCustomSignalModel, "BSW_Combined"),
			new Tile(Block.predicateButton, "Down"),
			Block.ThenTile(),
			new Tile(predicateDrive, -0.75f)
		});
		list.Add(new List<Tile>
		{
			new Tile(Block.predicateSendCustomSignalModel, "BSW_Combined"),
			Block.ThenTile(),
			new Tile(predicateSteeringWheelMoverSteer, "L", 90f)
		});
		list.Add(new List<Tile>
		{
			new Tile(Block.predicateSendCustomSignalModel, "BSW_Tilt"),
			Block.ThenTile(),
			new Tile(predicateSteeringWheelMoveLocalAlongTilt, 90f),
			new Tile(predicateSteeringWheelTiltSteer, 90f)
		});
		list.Add(new List<Tile>
		{
			new Tile(Block.predicateSendCustomSignalModel, "BSW_TiltCombined"),
			Block.ThenTile(),
			new Tile(predicateSteeringWheelTiltSteer, 90f)
		});
		list.Add(new List<Tile>
		{
			new Tile(Block.predicateSendCustomSignalModel, "BSW_TiltCombined"),
			new Tile(Block.predicateButton, "Up"),
			Block.ThenTile(),
			new Tile(predicateDrive, 1f)
		});
		list.Add(new List<Tile>
		{
			new Tile(Block.predicateSendCustomSignalModel, "BSW_TiltCombined"),
			new Tile(Block.predicateButton, "Down"),
			Block.ThenTile(),
			new Tile(predicateDrive, -0.75f)
		});
		list.Add(Block.EmptyTileRow());
		return list;
	}

	private void AddDriveControlEquivalent(List<Tile> row, Tile t)
	{
		row.Add(t);
		if (predicateDriveControl == t.gaf.Predicate)
		{
			string text = "BSW_" + Util.GetStringArg(t.gaf.Args, 0, "Mover");
			row.Add(new Tile(new GAF(Block.predicateSendCustomSignalModel, text)));
		}
	}

	private void SetupDriveControlTiles()
	{
		steeringWheelTiles = new List<List<Tile>>();
		for (int i = 0; i < tiles.Count; i++)
		{
			List<Tile> list = new List<Tile>();
			for (int j = 0; j < tiles[i].Count; j++)
			{
				AddDriveControlEquivalent(list, tiles[i][j]);
			}
			steeringWheelTiles.Add(list);
		}
		steeringWheelTiles.AddRange(GetDriveControlTiles());
		CreateFlattenTiles();
	}

	public override void Play()
	{
		base.Play();
		engineSoundOn = true;
		engineSoundRPM = 0f;
		loopClip = null;
		SetupDriveControlTiles();
		hasSetControlType = false;
		hadSetControlType = false;
		vehicleType = "Regular";
		controlType = string.Empty;
		storeForce = 17.6f;
		storeTurn = 45f;
		brakeEffectiveness = 1f;
		onGroundFraction = 0f;
		alignUp = goT.up;
		turnMode = VehicleTurnMode.FRONT;
		driveMode = VehicleDriveMode.ALL;
		jumpCounter = 45;
		jumpHeight = 0f;
		treatAsVehicleStatus = -1;
		allWheels.Clear();
		allMainTankWheels.Clear();
		jumpBlocks.Clear();
		chunkBlocks.Clear();
		chunkBlocks.UnionWith(chunk.blocks);
		wheelBlocks = new List<Block>();
		List<Block> list = Block.connectedCache[this];
		foreach (Block item2 in list)
		{
			BlockAbstractWheel blockAbstractWheel = item2 as BlockAbstractWheel;
			BlockTankTreadsWheel blockTankTreadsWheel = item2 as BlockTankTreadsWheel;
			if (blockAbstractWheel == null && (blockTankTreadsWheel == null || !blockTankTreadsWheel.IsMainBlockInGroup()))
			{
				continue;
			}
			if (blockAbstractWheel != null)
			{
				allWheels.Add(blockAbstractWheel);
				jumpBlocks.Add(blockAbstractWheel);
				wheelBlocks.Add(blockAbstractWheel);
			}
			if (blockTankTreadsWheel != null)
			{
				allMainTankWheels.Add(blockTankTreadsWheel);
				Block[] blocks = blockTankTreadsWheel.group.GetBlocks();
				foreach (Block item in blocks)
				{
					wheelBlocks.Add(item);
					jumpBlocks.Add(item);
				}
			}
		}
		jumpBlocks.Add(this);
		wheelsBounds = Util.ComputeBounds(wheelBlocks);
		vehicleCenter = wheelsBounds.center;
		GatherWheelFunctions();
		GetEngineSoundFromWheels();
		UpdateVehicleTypeValues();
	}

	public override List<List<Tile>> GetRuntimeTiles()
	{
		if (steeringWheelTiles.Count > 0)
		{
			return steeringWheelTiles;
		}
		return base.GetRuntimeTiles();
	}

	public override void RemovedPlayBlock(Block b)
	{
		base.RemovedPlayBlock(b);
		bool flag = false;
		if (b is BlockAbstractWheel item)
		{
			flag = true;
			allWheels.Remove(item);
		}
		if (b is BlockTankTreadsWheel item2)
		{
			flag = true;
			allMainTankWheels.Remove(item2);
		}
		if (flag)
		{
			GatherWheelFunctions();
		}
	}

	private void SetBallastFraction(float fraction, bool force = false)
	{
		if (chunk.rb != null)
		{
			Vector3 vector = (1f - fraction) * origLocalCM + fraction * origLocalVehicleCenter;
			if (force || (vector - chunk.rb.centerOfMass).sqrMagnitude > 0.0001f)
			{
				chunk.rb.centerOfMass = vector;
			}
		}
	}

	private void ResetHandling()
	{
		if (chunk.rb != null)
		{
			chunk.rb.ResetCenterOfMass();
			chunk.rb.ResetInertiaTensor();
		}
		chunk.UpdateCenterOfMass();
		wheelsBounds = Util.ComputeBounds(wheelBlocks);
		vehicleCenter = wheelsBounds.center;
		if (chunk.rb != null)
		{
			origLocalCM = chunk.rb.centerOfMass;
		}
		origLocalVehicleCenter = chunk.go.transform.InverseTransformPoint(vehicleCenter);
	}

	private void ImproveHandling()
	{
		if (allControlledWheels.Count + allControlledTankWheels.Count == 0)
		{
			return;
		}
		Rigidbody rb = chunk.rb;
		if (rb != null)
		{
			Vector3 inertiaTensor = rb.inertiaTensor;
			Vector3 v = 5f * goT.forward * Vector3.Dot(inertiaTensor, goT.forward);
			v = Util.Abs(v);
			try
			{
				rb.inertiaTensor = inertiaTensor + v;
			}
			catch
			{
				BWLog.Info("Unable to set inertia tensor, possibly due to the use of rigidbody constraints in the world.");
			}
		}
		origLocalCM = rb.centerOfMass;
		origLocalVehicleCenter = chunk.go.transform.InverseTransformPoint(vehicleCenter);
		SetBallastFraction(metaData.ballastFraction, force: true);
	}

	public override void Stop(bool resetBlock)
	{
		steeringWheelTiles.Clear();
		base.Stop(resetBlock);
		engineSoundOn = false;
		UpdateEngineSound();
		foreach (BlockAbstractWheel allWheel in allWheels)
		{
			allWheel.maxSpeedInc = 99999f;
		}
		allWheels.Clear();
		allMainTankWheels.Clear();
		jumpBlocks.Clear();
		ClearFunctionalWheels();
		chunkBlocks.Clear();
		foreach (Transform item in meshesToTurn)
		{
			item.localRotation = Quaternion.identity;
		}
		if (speedometer != null)
		{
			Blocksworld.UI.Overlay.RemoveSpeedometer(speedometer);
		}
		speedometer = null;
	}

	public override bool TreatAsVehicleLikeBlock()
	{
		return TreatAsVehicleLikeBlockWithStatus(ref treatAsVehicleStatus);
	}
}
