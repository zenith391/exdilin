using System;
using System.Collections.Generic;
using Blocks;
using Gestures;
using UnityEngine;

public class WalkController
{
	private enum WalkControllerMode
	{
		Idle,
		GotoTap,
		GotoTag,
		DPad,
		Translate
	}

	private enum TurnMode
	{
		None,
		Turn,
		TurnTowardsTag,
		TurnTowardsTap,
		TurnAlongCamera
	}

	private enum VicinityMode
	{
		None,
		AvoidTag
	}

	private enum JumpMode
	{
		Ready,
		AddingForce,
		WaitingForReady
	}

	private Chunk chunk;

	private Rigidbody rigidBodyBelow;

	private Vector3 rigidBodyBelowVelocity;

	private HandInfo[] handInfos;

	private HashSet<Collider> ignoreColliders;

	private Dictionary<WalkControllerMode, float> applicationCounts;

	private JumpMode jumpMode;

	private Vector3 jumpUp;

	private bool jumpPressed;

	private int jumpCountThisFrame;

	private float jumpTimer;

	private float jumpEnergyLeft;

	private float totalJumpEnergy;

	private Vector3 sideJumpVector;

	public float heightTestOffset;

	public bool climbOn;

	public Vector3 downRayOffset;

	public bool climbing;

	public Vector3 currentDirection;

	private Vector3 wantedVel;

	public float defaultMaxSpeed;

	public float defaultAvoidDistance;

	private Dictionary<string, float> avoidDistances;

	private Dictionary<string, float> avoidApplications;

	public float avoidMaxSpeed;

	public float defaultDPadMaxSpeed;

	public float speedIncPerApplication;

	public float wackiness;

	public float defaultWackiness;

	public float torqueMultiplier;

	public float addedTorqueMult;

	private float highSpeedFraction;

	private float tapActivatedTime;

	private bool onGround;

	private Vector3 onGroundNormal;

	private bool strafe;

	public Block onGroundBlock;

	private string dPadMoveKey;

	private WalkControllerMode mode;

	private WalkControllerMode previousMode;

	private TurnMode turnMode;

	private TurnMode previousTurnMode;

	private VicinityMode vicinityMode;

	private Vector3 gotoOffset;

	private Vector3 avoidVector;

	private float gotoMaxSpeed;

	private bool slowDownAtTarget;

	public BlockAbstractLegs legs;

	public List<WalkController> chunkControllers;

	private List<WalkController> sameUpControllers;

	private List<WalkController> conflictingUpControllers;

	private HashSet<Block> chunkBlocks;

	private float prevAngleError;

	private Vector3 iUpAngleError;

	private float prevVelError;

	private float prevForwardAngleError;

	private float iForwardAngleSum;

	private float prevHeightError;

	private Vector3 currentVelFiltered;

	private Vector3 targetVelocity;

	private Vector3 totalForce;

	private Vector3 totalCmTorque;

	public float speedFraction;

	private Vector3 hoverResult;

	private const float SPEED_FRACTION_THRESHOLD = 0.001f;

	private const float VEL_FILTER_ALPHA = 0.9f;

	private const float TRANSLATE_CONTROL_K = 10f;

	private const float TRANSLATE_CONTROL_D = 10f;

	private const float ANGLE_CONTROL_K = 2f;

	private const float ANGLE_CONTROL_D = 8f;

	private const float FORWARD_ANGLE_CONTROL_K = 2f;

	private const float FORWARD_ANGLE_CONTROL_D = 0.1f;

	private const float FORWARD_ANGLE_CONTROL_I = 5f;

	private Rigidbody legsRb;

	private Transform legsTransform;

	private const float BODY_HEIGHT_ADJUST = 4f;

	private int[] handCounters;

	private Vector3[] handTargets;

	public float SetAddedTorqueMultiplier
	{
		set
		{
			addedTorqueMult = value * legs.modelMass;
		}
	}

	public WalkController(BlockAbstractLegs legs)
	{
		rigidBodyBelowVelocity = Vector3.zero;
		handInfos = new HandInfo[2]
		{
			new HandInfo(),
			new HandInfo()
		};
		applicationCounts = new Dictionary<WalkControllerMode, float>();
		jumpUp = Vector3.up;
		sideJumpVector = Vector3.up;
		heightTestOffset = 0.5f;
		downRayOffset = Vector3.zero;
		currentDirection = Vector3.forward;
		wantedVel = Vector3.zero;
		defaultMaxSpeed = 5f;
		defaultAvoidDistance = 3f;
		avoidDistances = new Dictionary<string, float>();
		avoidApplications = new Dictionary<string, float>();
		avoidMaxSpeed = 5f;
		defaultDPadMaxSpeed = 5f;
		speedIncPerApplication = 2.5f;
		wackiness = 1f;
		defaultWackiness = 1f;
		torqueMultiplier = 1f;
		addedTorqueMult = 1f;
		tapActivatedTime = -1f;
		dPadMoveKey = "L";
		gotoOffset = Vector3.zero;
		avoidVector = Vector3.zero;
		gotoMaxSpeed = 8f;
		slowDownAtTarget = true;
		chunkControllers = new List<WalkController>();
		sameUpControllers = new List<WalkController>();
		conflictingUpControllers = new List<WalkController>();
		chunkBlocks = new HashSet<Block>();
		currentVelFiltered = Vector3.zero;
		targetVelocity = Vector3.zero;
		totalForce = Vector3.zero;
		totalCmTorque = Vector3.zero;
		speedFraction = 1f;
		hoverResult = Vector3.zero;
		handCounters = new int[2];
		handTargets = new Vector3[2];
		this.legs = legs;
		legsTransform = legs.goT;
		legsRb = legsTransform.GetComponent<Rigidbody>();
		if (legsRb != null)
		{
			legsRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			legsRb.maxDepenetrationVelocity = 10f;
			legsRb.solverIterations = 8;
			legsRb.solverVelocityIterations = 4;
		}
		currentDirection = legs.goT.forward;
	}

	public Vector3 GetRigidBodyBelowVelocity()
	{
		if (rigidBodyBelow == null)
		{
			return Vector3.zero;
		}
		return rigidBodyBelowVelocity;
	}

	public float GetWantedSpeedSqr()
	{
		float result = wantedVel.sqrMagnitude;
		if (mode == WalkControllerMode.Idle)
		{
			result = 0f;
		}
		return result;
	}

	private float IncApplications(WalkControllerMode mode, float analog)
	{
		float num = ((!applicationCounts.TryGetValue(mode, out num)) ? analog : (num + analog));
		applicationCounts[mode] = num;
		return num;
	}

	private float GetApplications(WalkControllerMode mode)
	{
		if (applicationCounts.TryGetValue(mode, out var value))
		{
			return value;
		}
		return 0f;
	}

	public void SetDefaultWackiness(float w)
	{
		defaultWackiness = w;
	}

	private bool InFirstPerson()
	{
		return legs == Blocksworld.blocksworldCamera.firstPersonCharacter;
	}

	private void AddJumpForce(float force, bool swim = false)
	{
		totalJumpEnergy += force / (float)(1 + jumpCountThisFrame * ((!swim) ? 1 : 4));
		jumpEnergyLeft = totalJumpEnergy;
		jumpTimer = 0.5f;
		jumpCountThisFrame++;
		legs.jumpCountdown = legs.startJumpCountdown;
		for (int i = 0; i < chunkControllers.Count; i++)
		{
			WalkController walkController = chunkControllers[i];
			walkController.legs.jumpCountdown = walkController.legs.startJumpCountdown;
		}
	}

	public void Jump(float force)
	{
		if (jumpMode == JumpMode.Ready && !climbing)
		{
			float num = Mathf.Clamp(legs.modelMass, 1f, 5f);
			if (onGround && legs.upright)
			{
				AddJumpForce(force * 100f * num);
				jumpUp = Vector3.up;
			}
			else if (BlockWater.BlockWithinWater(legs))
			{
				AddJumpForce(force * 100f * num, swim: true);
				jumpUp = legs.goT.up;
			}
		}
		jumpPressed = true;
	}

	public float GetWackiness()
	{
		if (!IsActive() || !onGround || jumpMode != JumpMode.Ready)
		{
			return 1f;
		}
		return wackiness;
	}

	public void SetChunk()
	{
		chunk = legs.chunk;
		chunkBlocks = new HashSet<Block>();
		foreach (Block block in chunk.blocks)
		{
			chunkBlocks.Add(block);
			if (block is BlockAbstractLegs && block != legs)
			{
				BlockAbstractLegs blockAbstractLegs = (BlockAbstractLegs)block;
				if (blockAbstractLegs.walkController != null)
				{
					chunkControllers.Add(blockAbstractLegs.walkController);
				}
			}
		}
		foreach (WalkController chunkController in chunkControllers)
		{
			float num = Vector3.Dot(chunkController.legs.goT.up, legs.goT.up);
			if (num > 0.99f)
			{
				sameUpControllers.Add(chunkController);
			}
			else if (num < -0.99f)
			{
				conflictingUpControllers.Add(chunkController);
			}
		}
	}

	public bool IsActive()
	{
		if (mode == WalkControllerMode.Idle && turnMode == TurnMode.None)
		{
			return vicinityMode != VicinityMode.None;
		}
		return true;
	}

	public Vector3 GetTargetVelocity()
	{
		if (mode != WalkControllerMode.Idle)
		{
			return targetVelocity;
		}
		return Vector3.zero;
	}

	public Vector3 GetTotalForce()
	{
		if (mode != WalkControllerMode.Idle)
		{
			return totalForce;
		}
		return Vector3.zero;
	}

	public Vector3 GetTotalCmTorque()
	{
		if (mode != WalkControllerMode.Idle)
		{
			return totalCmTorque;
		}
		return Vector3.zero;
	}

	public void Translate(Vector3 dir, float maxSpeed)
	{
		strafe = Mathf.Abs(dir.x) > 0.5f && Mathf.Abs(dir.z) < 0.1f;
		if (InFirstPerson())
		{
			Vector3 firstPersonDeadZone = Blocksworld.blocksworldCamera.firstPersonDeadZone;
			if (Mathf.Abs(dir.x) < firstPersonDeadZone.x)
			{
				dir.x = 0f;
			}
			else
			{
				bool flag = dir.x < 0f;
				dir.x = (Mathf.Abs(dir.x) - firstPersonDeadZone.x) / (1f - firstPersonDeadZone.x);
				if (flag)
				{
					dir.x = 0f - dir.x;
				}
			}
			if (Mathf.Abs(dir.y) < firstPersonDeadZone.y)
			{
				dir.y = 0f;
			}
			else
			{
				bool flag2 = dir.y < 0f;
				dir.y = (Mathf.Abs(dir.y) - firstPersonDeadZone.y) / (1f - firstPersonDeadZone.y);
				if (flag2)
				{
					dir.y = 0f - dir.y;
				}
			}
			BlocksworldCamera blocksworldCamera = Blocksworld.blocksworldCamera;
			blocksworldCamera.firstPersonDpad.x = blocksworldCamera.firstPersonDpad.x + maxSpeed * dir.x;
			BlocksworldCamera blocksworldCamera2 = Blocksworld.blocksworldCamera;
			blocksworldCamera2.firstPersonDpad.y = blocksworldCamera2.firstPersonDpad.y + maxSpeed * dir.z;
		}
		mode = WalkControllerMode.Translate;
		gotoMaxSpeed = Mathf.Max(maxSpeed, gotoMaxSpeed);
		Vector3 vector = Vector3.Cross(currentDirection, Vector3.up);
		Vector3 vector2 = currentDirection * dir.z + vector * dir.x;
		gotoOffset += maxSpeed * 5f * vector2;
		gotoOffset.y = 0f;
		slowDownAtTarget = true;
	}

	public void AvoidTag(string tagName, float avoidDistance, float maxSpeed, float analog)
	{
		vicinityMode = VicinityMode.AvoidTag;
		float num;
		if (avoidDistances.TryGetValue(tagName, out var value))
		{
			avoidDistances[tagName] = value + analog * avoidDistance;
			num = avoidApplications[tagName] + analog;
		}
		else
		{
			avoidDistances[tagName] = analog * avoidDistance;
			num = analog;
		}
		avoidApplications[tagName] = num;
		avoidMaxSpeed = Mathf.Max(analog * maxSpeed + speedIncPerApplication * Mathf.Max(0f, num - 1f), avoidMaxSpeed);
	}

	public void Turn(float speed)
	{
		turnMode = TurnMode.Turn;
		if (InFirstPerson())
		{
			bool flag = speed < 0f;
			float num = Mathf.Abs(speed) / 4f;
			if (num < 0.3f)
			{
				num = 0f;
			}
			else
			{
				if (num > 1f)
				{
					num = 1f;
				}
				num = (num - 0.3f) / 0.7f;
				num = Mathf.Pow(num, 9f);
				if (flag)
				{
					num *= -1f;
				}
			}
			num *= 0.001f;
			Vector3 firstPersonDeadZone = Blocksworld.blocksworldCamera.firstPersonDeadZone;
			if (Mathf.Abs(num) < firstPersonDeadZone.x)
			{
				num = 0f;
			}
			else
			{
				bool flag2 = num < 0f;
				num = (Mathf.Abs(num) - firstPersonDeadZone.x) / (1f - firstPersonDeadZone.x);
				if (flag2)
				{
					num = 0f - num;
				}
			}
			Blocksworld.blocksworldCamera.firstPersonRotation += num;
		}
		currentDirection = Quaternion.AngleAxis(speed, Vector3.up) * currentDirection;
		currentDirection.y = 0f;
		if (currentDirection.sqrMagnitude > 0.01f)
		{
			currentDirection.Normalize();
		}
	}

	public void TurnTowardsTag(string tagName)
	{
		turnMode = TurnMode.TurnTowardsTag;
		Vector3 target = default(Vector3);
		if (TryGetClosestBlockWithTag(tagName, out target))
		{
			Vector3 vector = target - legs.goT.position;
			vector.y = 0f;
			if (vector.sqrMagnitude > 0.01f)
			{
				currentDirection = vector.normalized;
			}
		}
	}

	public void DPadControl(string key, float maxSpeed, float wackiness)
	{
		if (InFirstPerson())
		{
			Vector2 vector = ((!Blocksworld.UI.Controls.IsDPadActive(key)) ? Vector2.zero : Blocksworld.UI.Controls.GetNormalizedDPadOffset(key));
			Vector3 firstPersonDeadZone = Blocksworld.blocksworldCamera.firstPersonDeadZone;
			if (Mathf.Abs(vector.x) < firstPersonDeadZone.x)
			{
				vector.x = 0f;
			}
			else
			{
				bool flag = vector.x < 0f;
				vector.x = (Mathf.Abs(vector.x) - firstPersonDeadZone.x) / (1f - firstPersonDeadZone.x);
				if (flag)
				{
					vector.x = 0f - vector.x;
				}
			}
			if (Mathf.Abs(vector.y) < firstPersonDeadZone.y)
			{
				vector.y = 0f;
			}
			else
			{
				bool flag2 = vector.y < 0f;
				vector.y = (Mathf.Abs(vector.y) - firstPersonDeadZone.y) / (1f - firstPersonDeadZone.y);
				if (flag2)
				{
					vector.y = 0f - vector.y;
				}
			}
			Blocksworld.blocksworldCamera.firstPersonRotation += Mathf.Pow(vector.x, 3f) / 2f;
			BlocksworldCamera blocksworldCamera = Blocksworld.blocksworldCamera;
			blocksworldCamera.firstPersonDpad.y = blocksworldCamera.firstPersonDpad.y + maxSpeed * Mathf.Pow(vector.y, 3f);
		}
		mode = WalkControllerMode.DPad;
		dPadMoveKey = key;
		float num = IncApplications(mode, 1f);
		gotoMaxSpeed = Mathf.Max(maxSpeed + speedIncPerApplication * Mathf.Max(0f, num - 1f), gotoMaxSpeed);
		slowDownAtTarget = true;
		this.wackiness = wackiness;
	}

	public void TiltMover(Vector2 tiltVector)
	{
		float magnitude = tiltVector.magnitude;
		tiltVector = tiltVector.normalized;
		if (InFirstPerson())
		{
			Vector3 firstPersonDeadZone = Blocksworld.blocksworldCamera.firstPersonDeadZone;
			if (Mathf.Abs(tiltVector.x) < firstPersonDeadZone.x)
			{
				tiltVector.x = 0f;
			}
			else
			{
				bool flag = tiltVector.x < 0f;
				tiltVector.x = (Mathf.Abs(tiltVector.x) - firstPersonDeadZone.x) / (1f - firstPersonDeadZone.x);
				if (flag)
				{
					tiltVector.x = 0f - tiltVector.x;
				}
			}
			if (Mathf.Abs(tiltVector.y) < firstPersonDeadZone.y)
			{
				tiltVector.y = 0f;
			}
			else
			{
				bool flag2 = tiltVector.y < 0f;
				tiltVector.y = (Mathf.Abs(tiltVector.y) - firstPersonDeadZone.y) / (1f - firstPersonDeadZone.y);
				if (flag2)
				{
					tiltVector.y = 0f - tiltVector.y;
				}
			}
			Blocksworld.blocksworldCamera.firstPersonRotation += Mathf.Pow(tiltVector.x, 3f) / 2f;
			BlocksworldCamera blocksworldCamera = Blocksworld.blocksworldCamera;
			blocksworldCamera.firstPersonDpad.y = blocksworldCamera.firstPersonDpad.y + magnitude * Mathf.Pow(tiltVector.y, 3f);
		}
		mode = WalkControllerMode.DPad;
		dPadMoveKey = "L";
		float num = IncApplications(mode, 1f);
		gotoMaxSpeed = Mathf.Max(magnitude + speedIncPerApplication * Mathf.Max(0f, num - 1f), gotoMaxSpeed);
		slowDownAtTarget = true;
		wackiness = wackiness;
	}

	public bool TryGetClosestBlockWithTag(string tagName, out Vector3 target, bool allowChunk = false)
	{
		List<Block> blocksWithTag = TagManager.GetBlocksWithTag(tagName);
		target = default(Vector3);
		if (blocksWithTag.Count > 0)
		{
			float num = 1E+09f;
			Vector3 position = legs.goT.position;
			bool result = false;
			for (int i = 0; i < blocksWithTag.Count; i++)
			{
				Block block = blocksWithTag[i];
				if (allowChunk || !chunkBlocks.Contains(block))
				{
					Vector3 position2 = block.goT.position;
					float sqrMagnitude = (position - position2).sqrMagnitude;
					if (sqrMagnitude < num)
					{
						num = sqrMagnitude;
						target = position2;
						result = true;
					}
				}
			}
			return result;
		}
		return false;
	}

	public void GotoTag(string tagName, float maxSpeed, float wackiness, float analog, bool slowDown = true)
	{
		mode = WalkControllerMode.GotoTag;
		float num = IncApplications(mode, analog);
		gotoMaxSpeed = Mathf.Max(analog * maxSpeed + speedIncPerApplication * Mathf.Max(0f, num - 1f), gotoMaxSpeed);
		this.wackiness = wackiness;
		slowDownAtTarget = slowDown;
		Vector3 target = default(Vector3);
		if (TryGetClosestBlockWithTag(tagName, out target))
		{
			gotoOffset += target - legs.goT.position;
			gotoOffset.y = 0f;
			if (gotoOffset.sqrMagnitude > 1E-05f)
			{
				currentDirection = gotoOffset.normalized;
			}
		}
	}

	public void GotoTap(float maxSpeed, float wackiness, bool slowDown = true)
	{
		mode = WalkControllerMode.GotoTap;
		float num = IncApplications(mode, 1f);
		gotoMaxSpeed = Mathf.Max(maxSpeed + speedIncPerApplication * Mathf.Max(0f, num - 1f), gotoMaxSpeed);
		this.wackiness = wackiness;
		slowDownAtTarget = slowDown;
	}

	public void TurnTowardsTap()
	{
		turnMode = TurnMode.TurnTowardsTap;
	}

	public void TurnAlongCamera()
	{
		turnMode = TurnMode.TurnAlongCamera;
		Vector3 vector = Util.ProjectOntoPlane(Blocksworld.cameraForward, Vector3.up);
		if (vector.sqrMagnitude > 0.001f)
		{
			currentDirection = vector.normalized;
		}
	}

	public void AddIgnoreCollider(Collider c)
	{
		if (ignoreColliders == null)
		{
			GatherIgnoreColliders();
		}
		ignoreColliders.Add(c);
	}

	private void GatherIgnoreColliders()
	{
		ignoreColliders = new HashSet<Collider>();
		for (int i = 0; i < 2 * legs.legPairCount; i++)
		{
			ignoreColliders.Add(legs.feet[i].collider);
		}
		if (!(legs.body != null))
		{
			return;
		}
		Transform[] componentsInChildren = legs.body.GetComponentsInChildren<Transform>();
		foreach (Transform transform in componentsInChildren)
		{
			Collider component = transform.gameObject.GetComponent<Collider>();
			if (component != null)
			{
				ignoreColliders.Add(component);
			}
		}
		foreach (WalkController chunkController in chunkControllers)
		{
			BlockAbstractLegs blockAbstractLegs = chunkController.legs;
			for (int k = 0; k < 2 * blockAbstractLegs.legPairCount; k++)
			{
				ignoreColliders.Add(chunkController.legs.feet[k].collider);
			}
		}
	}

	private Vector3 GetHoverForceAndUpdateOnGround(float mass)
	{
		hoverResult.Set(0f, 0f, 0f);
		float maxDistance = legs.onGroundHeight + 1f;
		if (ignoreColliders == null)
		{
			GatherIgnoreColliders();
		}
		Vector3 up = legsTransform.up;
		Vector3 vector = legsTransform.position + up * heightTestOffset;
		if (climbOn && wantedVel.sqrMagnitude > 0.001f)
		{
			downRayOffset = wantedVel.normalized * 0.65f;
			vector += downRayOffset;
		}
		else
		{
			downRayOffset = Vector3.zero;
		}
		RaycastHit[] array = Physics.RaycastAll(vector, -up, maxDistance, BlockAbstractLegs.raycastMask);
		RaycastHit hit = default(RaycastHit);
		float num = float.MaxValue;
		bool flag = false;
		if (array.Length != 0)
		{
			RaycastHit[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				RaycastHit raycastHit = array2[i];
				Collider collider = raycastHit.collider;
				if (!(collider == null) && !collider.isTrigger && !ignoreColliders.Contains(collider))
				{
					float sqrMagnitude = (raycastHit.point - vector).sqrMagnitude;
					if (sqrMagnitude < num)
					{
						num = sqrMagnitude;
						hit = raycastHit;
						flag = true;
					}
				}
			}
		}
		climbing = false;
		rigidBodyBelow = null;
		rigidBodyBelowVelocity = Vector3.zero;
		onGroundBlock = null;
		if (!flag)
		{
			prevHeightError = 0f;
			onGround = false;
			return hoverResult;
		}
		GameObject gameObject = hit.collider.gameObject;
		Transform parent = gameObject.transform.parent;
		if (parent != null)
		{
			AddRbBelowVelocity(hit, parent.gameObject.GetComponent<Rigidbody>());
		}
		else
		{
			AddRbBelowVelocity(hit, gameObject.GetComponent<Rigidbody>(), checkSurfaceBlocks: true);
		}
		float magnitude = (vector - hit.point).magnitude;
		float num2 = Mathf.Clamp(Vector3.Dot(hit.normal, Vector3.up), 0f, 1f);
		onGround = num2 > 0.2f && magnitude < legs.onGroundHeight + 0.25f + heightTestOffset;
		onGroundNormal = hit.normal;
		bool flag2 = magnitude < legs.onGroundHeight + 0.6f;
		onGroundBlock = ((!flag2) ? null : BWSceneManager.FindBlock(gameObject));
		float num3 = legs.onGroundHeight + 0.5f;
		if (magnitude < num3)
		{
			float modelMass = legs.modelMass;
			float num4 = legs.onGroundHeight + 0.5f - magnitude;
			float num5 = num4 - prevHeightError;
			prevHeightError = num4;
			float num6 = 30f * modelMass;
			float num7 = 50f * modelMass;
			float num8 = 100f;
			float num9 = Mathf.Min(num2 * num2 * 1.2f, 1f);
			float num10 = num9 * Mathf.Clamp((num6 * num4 + num7 * num5) * 0.5f * mass, 0f - num8, num8);
			hoverResult += up * num10;
			if (climbOn && magnitude < 0.9f && num2 > 0.2f)
			{
				climbing = true;
			}
		}
		else
		{
			prevHeightError = 0f;
		}
		return hoverResult;
	}

	private void AddRbBelowVelocity(RaycastHit hit, Rigidbody rb, bool checkSurfaceBlocks = false)
	{
		rigidBodyBelow = rb;
		if (!(rigidBodyBelow != null) || rigidBodyBelow.isKinematic)
		{
			return;
		}
		rigidBodyBelowVelocity = rigidBodyBelow.velocity;
		Vector3 rhs = hit.point - rigidBodyBelow.worldCenterOfMass;
		rigidBodyBelowVelocity += Vector3.Cross(rigidBodyBelow.angularVelocity, rhs);
		if (checkSurfaceBlocks)
		{
			Block block = BWSceneManager.FindBlock(rb.gameObject, checkChildGos: true);
			if (block is BlockTankTreadsWheel { treadsInfo: not null } blockTankTreadsWheel && blockTankTreadsWheel.treadsInfo.gameObjectToTreadLink.TryGetValue(rb.gameObject, out var value))
			{
				rigidBodyBelowVelocity += value.GetTreadVelocity();
			}
		}
	}

	private void DPadFixedUpdate(string key)
	{
		Vector2 vector = ((!Blocksworld.UI.Controls.IsDPadActive(key)) ? Vector2.zero : Blocksworld.UI.Controls.GetNormalizedDPadOffset(key));
		float maxSpeed = gotoMaxSpeed * Mathf.Min(1f, vector.magnitude);
		Vector3 cameraUp = Blocksworld.cameraUp;
		Vector3 cameraRight = Blocksworld.cameraRight;
		Vector3 cameraForward = Blocksworld.cameraForward;
		float num = vector.y;
		if (cameraForward.y > 0f)
		{
			num *= -1f;
		}
		Vector3 vector2 = Util.ProjectOntoPlane(cameraUp, Vector3.up).normalized * num;
		Vector3 vector3 = Util.ProjectOntoPlane(cameraRight, Vector3.up).normalized * vector.x;
		Vector3 vector4 = vector2 + vector3;
		slowDownAtTarget = false;
		gotoOffset += vector4 * 100f;
		if (turnMode == TurnMode.None && vector4.sqrMagnitude > 0.001f)
		{
			currentDirection = vector4.normalized;
		}
		GotoFixedUpdate(maxSpeed);
	}

	private void GotoFixedUpdate(float maxSpeed)
	{
		if (legsRb == null)
		{
			legsRb = legs.GetRigidBody();
			legsTransform = legs.goT;
			if (legsRb == null)
			{
				return;
			}
		}
		if (gotoMaxSpeed > 0.001f)
		{
			speedFraction = maxSpeed / gotoMaxSpeed;
		}
		else
		{
			speedFraction = 0f;
		}
		Vector3 worldCenterOfMass = legsRb.worldCenterOfMass;
		bool flag = InFirstPerson() && !legs.unmoving && !legs.IsFixed();
		if (flag && (mode == WalkControllerMode.DPad || (mode == WalkControllerMode.Translate && !strafe)))
		{
			Vector3 forward = legsTransform.forward;
			Vector3 right = legsTransform.right;
			Vector2 firstPersonDpad = Blocksworld.blocksworldCamera.firstPersonDpad;
			float num = Blocksworld.blocksworldCamera.firstPersonTurnScale * Blocksworld.blocksworldCamera.firstPersonRotation;
			if (Blocksworld.blocksworldCamera.firstPersonMode == 2)
			{
				firstPersonDpad.x = 1.5f * num;
				num = -0.0005f * Blocksworld.blocksworldCamera.firstPersonLookOffset.x;
			}
			BlockCharacter firstPersonCharacter = Blocksworld.blocksworldCamera.firstPersonCharacter;
			if (firstPersonCharacter != null && !firstPersonCharacter.GetThinksOnGround() && Mathf.Abs(num) > 0.05f && Mathf.Abs(firstPersonDpad.y) < 0.1f)
			{
				firstPersonDpad.y = 0.1f;
			}
			currentDirection = 0.2f * num * right + currentDirection;
			currentDirection.y = 0f;
			currentDirection.Normalize();
			gotoOffset = 100f * (firstPersonDpad.x * right + firstPersonDpad.y * forward);
		}
		Vector3 vector = worldCenterOfMass + gotoOffset;
		Vector3 zero = Vector3.zero;
		totalForce = Vector3.zero;
		totalCmTorque = Vector3.zero;
		Vector3 vector2 = vector - worldCenterOfMass;
		vector2.y = 0f;
		float magnitude = vector2.magnitude;
		Vector3 normalized = vector2.normalized;
		Vector3 velocity = legsRb.velocity;
		velocity.y = 0f;
		currentVelFiltered = 0.9f * currentVelFiltered + 0.100000024f * velocity;
		float num2 = maxSpeed;
		if (slowDownAtTarget)
		{
			if (magnitude < 5f)
			{
				num2 = magnitude / 5f;
			}
			else if (magnitude < 10f)
			{
				num2 = maxSpeed * (magnitude / 10f);
			}
			speedFraction = num2 / gotoMaxSpeed;
		}
		Vector3 b = normalized * num2;
		float sqrMagnitude = avoidVector.sqrMagnitude;
		bool flag2 = sqrMagnitude > 0.01f;
		if (flag2)
		{
			if (sqrMagnitude > 1f)
			{
				avoidVector.Normalize();
			}
			b += avoidVector * avoidMaxSpeed;
			float num3 = Mathf.Max(avoidMaxSpeed, gotoMaxSpeed);
			if (b.sqrMagnitude > num3 * num3)
			{
				b = b.normalized * num3;
			}
		}
		if (onGround && onGroundBlock != null)
		{
			Collider component = onGroundBlock.go.GetComponent<Collider>();
			PhysicMaterial material = component.material;
			float dynamicFriction = material.dynamicFriction;
			if (dynamicFriction < 0.4f && legsRb.velocity.magnitude > 0.1f)
			{
				float t = Mathf.Pow(dynamicFriction / 0.4f, 3f);
				b = Vector3.Lerp(wantedVel, b, t);
			}
		}
		wantedVel = b;
		float num4 = 0f;
		if (gotoMaxSpeed > 0.01f)
		{
			num4 = currentVelFiltered.magnitude / gotoMaxSpeed;
		}
		float num5 = 1f - wackiness;
		highSpeedFraction = num4 * Mathf.Clamp((gotoMaxSpeed - 7f) * 0.05f, 0f, 1f);
		if (highSpeedFraction > 0.01f)
		{
			num5 = Mathf.Min(1f, num5 + highSpeedFraction);
		}
		Vector3 vector3 = num5 * GetHoverForceAndUpdateOnGround(legsRb.mass) * 4f;
		zero += vector3;
		if (rigidBodyBelow != null)
		{
			rigidBodyBelowVelocity.y = 0f;
			float sqrMagnitude2 = rigidBodyBelowVelocity.sqrMagnitude;
			if (sqrMagnitude2 > 0.01f)
			{
				Vector3 vector4 = new Vector3(rigidBodyBelowVelocity.x, 0f, rigidBodyBelowVelocity.z);
				wantedVel += vector4;
			}
		}
		if (!onGround)
		{
			prevHeightError = 0f;
		}
		Vector3 vector5 = wantedVel - currentVelFiltered;
		vector5.y = 0f;
		float magnitude2 = vector5.magnitude;
		Vector3 normalized2 = vector5.normalized;
		float num6 = magnitude2 - prevVelError;
		prevVelError = magnitude2;
		Vector3 up = legsTransform.up;
		float num7 = Vector3.Angle(Vector3.up, up);
		float num8 = Mathf.Max(1f - Mathf.Abs(num7) / 135f, 0f);
		float num9 = num8;
		if (!onGround)
		{
			num8 *= 0.5f;
			num9 = 0.6f;
		}
		float num10 = 1f + highSpeedFraction;
		bool flag3 = flag2 || gotoOffset.sqrMagnitude > 0.0001f || (magnitude2 > 1f && onGround);
		float num11 = 50f;
		if (flag3 && (onGround || magnitude2 > 0f))
		{
			Vector3 vector6 = normalized2;
			float num12 = num10 * num8 * Mathf.Clamp(magnitude2 * 10f + num6 * 10f, 0f - num11, num11);
			vector6 *= num12;
			zero += vector6;
		}
		if (onGround && onGroundBlock != null)
		{
			Collider componentInChildren = onGroundBlock.go.GetComponentInChildren<Collider>();
			float num13 = ((!(componentInChildren != null) || !(componentInChildren.sharedMaterial != null)) ? 0f : componentInChildren.sharedMaterial.bounciness);
			float num14 = 4f - 3f * num13;
			Vector3 velocity2 = legsRb.velocity;
			float num15 = 0f - Vector3.Dot(onGroundNormal, velocity2);
			if (num13 > 0f && num15 > num14)
			{
				float num16 = velocity2.magnitude * num13;
				Vector3 vector7 = Vector3.Reflect(velocity2.normalized, onGroundNormal);
				legsRb.AddForce(1.5f * num16 * vector7 - velocity2, ForceMode.VelocityChange);
				onGround = false;
				return;
			}
		}
		if (onGround || flag3)
		{
			Vector3 vector8 = Vector3.Cross(up, Vector3.up);
			float num17 = Mathf.Clamp(num7 - prevAngleError, -5f, 5f);
			prevAngleError = num7;
			float num18 = 70f * num10;
			float value = num10 * (0.1f + 0.9f * num9) * (num7 * 2f + num17 * 8f + 40f);
			value = Mathf.Clamp(value, 0f - num18, num18);
			totalCmTorque += vector8 * value;
			Vector3 angularVelocity = legsRb.angularVelocity;
			float magnitude3 = angularVelocity.magnitude;
			float num19 = Mathf.Clamp(magnitude3, -10f, 10f);
			totalCmTorque += (0f - num9) * num19 * angularVelocity;
			float num20 = ((!slowDownAtTarget) ? 1f : 3f);
			if (strafe)
			{
				currentDirection = legsTransform.forward;
			}
			else if (((double)magnitude2 > 0.02 && magnitude > num20) || turnMode != TurnMode.None || flag)
			{
				Vector3 forward2 = legsTransform.forward;
				if ((double)Mathf.Abs(Vector3.Dot(forward2, Vector3.up)) < 0.25)
				{
					forward2.y = 0f;
					Vector3 vector9 = ((turnMode == TurnMode.None && !flag) ? normalized : currentDirection);
					float num21 = Util.AngleBetween(forward2, vector9, Vector3.up);
					float num22 = Mathf.Clamp(num21 - prevForwardAngleError, -5f, 5f) / Blocksworld.fixedDeltaTime;
					prevForwardAngleError = num21;
					if (Mathf.Abs(num21) < 25f)
					{
						iForwardAngleSum = Mathf.Clamp(iForwardAngleSum + Blocksworld.fixedDeltaTime * num21, -1000f, 1000f);
					}
					else
					{
						iForwardAngleSum = 0f;
					}
					vector8 = Vector3.Cross(forward2, vector9);
					float num23 = Mathf.Clamp(num21 * 2f + num22 * 0.1f + iForwardAngleSum * 5f, 0f - num18, num18);
					if (flag)
					{
						num23 *= Blocksworld.blocksworldCamera.firstPersonTorque;
					}
					bool ignoreRotation = legs.ignoreRotation;
					num23 = ((!ignoreRotation) ? num23 : 0f);
					totalCmTorque += num9 * up * num23 * (1f + highSpeedFraction);
					Vector3 position = legsTransform.position;
					Vector3 vector10 = position - worldCenterOfMass;
					vector10.y = 0f;
					float magnitude4 = vector10.magnitude;
					if (magnitude4 > 0.25f && !ignoreRotation)
					{
						float num24 = 0.5f * num11;
						float num25 = num9 * (1f + highSpeedFraction) * Mathf.Clamp(-0.1f * num23 * magnitude4, 0f - num24, num24);
						Vector3 vector11 = num25 * Vector3.Cross(vector10 / magnitude4, Vector3.up);
						totalForce += vector11;
						legsRb.AddForceAtPosition(vector11, position);
					}
				}
			}
		}
		else
		{
			prevAngleError = 0f;
		}
		legsRb.AddForceAtPosition(zero, worldCenterOfMass);
		legsRb.AddTorque(totalCmTorque * torqueMultiplier * addedTorqueMult);
		totalForce += zero;
		applicationCounts.Clear();
	}

	private void GotoTapFixedUpdate()
	{
		if (TapControlGesture.HasWorldTapPos() && tapActivatedTime >= 0f && TapControlGesture.GetWorldTapTime() >= tapActivatedTime)
		{
			gotoOffset += TapControlGesture.GetWorldTapPos() - legs.goT.position;
			gotoOffset.y = 0f;
			if (turnMode == TurnMode.None && gotoOffset.sqrMagnitude > 1E-05f)
			{
				currentDirection = gotoOffset.normalized;
			}
		}
		GotoFixedUpdate(gotoMaxSpeed);
	}

	private void TurnTowardsTapFixedUpdate()
	{
		if (TapControlGesture.HasWorldTapPos() && tapActivatedTime >= 0f && TapControlGesture.GetWorldTapTime() >= tapActivatedTime)
		{
			Vector3 vector = TapControlGesture.GetWorldTapPos() - legs.goT.position;
			vector.y = 0f;
			if (vector.sqrMagnitude > 0.01f)
			{
				currentDirection = vector.normalized;
			}
		}
	}

	private void AddJumpForce()
	{
		float num = 7f * totalJumpEnergy * Blocksworld.fixedDeltaTime;
		Vector3 force = num * jumpUp + 0.2f * num * sideJumpVector;
		Rigidbody component = legs.body.GetComponent<Rigidbody>();
		Vector3 worldCenterOfMass = component.worldCenterOfMass;
		Vector3 position = 0.7f * worldCenterOfMass + 0.3f * legs.goT.position;
		component.AddForceAtPosition(force, position);
		jumpEnergyLeft -= num;
	}

	private void JumpFixedUpdate()
	{
		switch (jumpMode)
		{
		case JumpMode.WaitingForReady:
			jumpTimer -= Blocksworld.fixedDeltaTime;
			if (jumpTimer <= 0f)
			{
				jumpMode = JumpMode.Ready;
			}
			break;
		case JumpMode.Ready:
			if (jumpEnergyLeft > 0f)
			{
				AddJumpForce();
				if (gotoOffset.sqrMagnitude > 1E-05f)
				{
					sideJumpVector = gotoOffset.normalized * speedFraction;
				}
				else
				{
					sideJumpVector = Vector3.zero;
				}
				jumpMode = JumpMode.AddingForce;
				jumpCountThisFrame = 0;
			}
			break;
		case JumpMode.AddingForce:
			AddJumpForce();
			if (jumpEnergyLeft <= 0f || !jumpPressed)
			{
				jumpEnergyLeft = 0f;
				totalJumpEnergy = 0f;
				jumpMode = JumpMode.WaitingForReady;
			}
			break;
		}
		jumpPressed = false;
	}

	private void AvoidTagFixedUpdate()
	{
		foreach (KeyValuePair<string, float> avoidDistance in avoidDistances)
		{
			string key = avoidDistance.Key;
			float num = avoidDistances[key];
			List<Block> blocksWithTag = TagManager.GetBlocksWithTag(key);
			Vector3 position = legs.goT.position;
			for (int i = 0; i < blocksWithTag.Count; i++)
			{
				Block block = blocksWithTag[i];
				if (!chunkBlocks.Contains(block))
				{
					Collider component = block.go.GetComponent<Collider>();
					Vector3 vector = ((!(component != null)) ? block.goT.position : component.ClosestPointOnBounds(position));
					Vector3 vector2 = position - vector;
					vector2.y = 0f;
					float magnitude = vector2.magnitude;
					if (magnitude > 0.01f && magnitude < num)
					{
						Vector3 normalized = vector2.normalized;
						float num2 = 1f - magnitude / num;
						avoidVector += normalized * num2;
					}
				}
			}
		}
		avoidDistances.Clear();
		avoidApplications.Clear();
	}

	public void FixedUpdate()
	{
		switch (vicinityMode)
		{
		case VicinityMode.None:
			avoidMaxSpeed = defaultMaxSpeed;
			break;
		case VicinityMode.AvoidTag:
			AvoidTagFixedUpdate();
			break;
		}
		if (InFirstPerson())
		{
			GotoFixedUpdate(gotoMaxSpeed);
		}
		else
		{
			TurnMode turnMode = this.turnMode;
			if (turnMode == TurnMode.TurnTowardsTap)
			{
				TurnTowardsTapFixedUpdate();
				if (previousTurnMode != TurnMode.TurnTowardsTap)
				{
					tapActivatedTime = Time.time;
				}
			}
			switch (mode)
			{
			case WalkControllerMode.Idle:
				if (this.turnMode != TurnMode.None || vicinityMode != VicinityMode.None)
				{
					GotoFixedUpdate(gotoMaxSpeed);
				}
				gotoMaxSpeed = defaultMaxSpeed;
				speedFraction = 1f;
				prevVelError = 0f;
				wantedVel = Vector3.zero;
				break;
			case WalkControllerMode.GotoTap:
				GotoTapFixedUpdate();
				if (previousMode != WalkControllerMode.GotoTap)
				{
					tapActivatedTime = Time.time;
				}
				break;
			case WalkControllerMode.GotoTag:
				GotoFixedUpdate(gotoMaxSpeed);
				break;
			case WalkControllerMode.DPad:
				DPadFixedUpdate(dPadMoveKey);
				break;
			case WalkControllerMode.Translate:
				GotoFixedUpdate(gotoMaxSpeed);
				break;
			}
		}
		JumpFixedUpdate();
		if (mode == WalkControllerMode.Idle && this.turnMode == TurnMode.None)
		{
			tapActivatedTime = -1f;
			prevAngleError = 0f;
		}
		previousMode = mode;
		previousTurnMode = this.turnMode;
		mode = WalkControllerMode.Idle;
		this.turnMode = TurnMode.None;
		vicinityMode = VicinityMode.None;
		gotoOffset = Vector3.zero;
		avoidVector = Vector3.zero;
		strafe = false;
		gotoMaxSpeed = 0f;
		if (InFirstPerson())
		{
			Blocksworld.blocksworldCamera.firstPersonDpad = Vector2.zero;
			Blocksworld.blocksworldCamera.firstPersonRotation = 0f;
		}
	}

	public float GetAndResetHighSpeedFraction()
	{
		float result = highSpeedFraction;
		highSpeedFraction = 1f;
		return result;
	}

	private void Climp(int handIndex, Vector3 shoulderPoint, Vector3 climbPoint, Vector3 signedRight, ref float alpha)
	{
		Vector3 normalized = (shoulderPoint - climbPoint).normalized;
		handTargets[handIndex] = climbPoint + 0.3f * normalized + signedRight * 0.3f;
		alpha = 0.5f;
	}

	public void SetHandPosition(int handIndex, GameObject hand, float handWidth, float handHeight)
	{
		Transform transform = hand.transform;
		Vector3 position = transform.position;
		Transform goT = legs.goT;
		Vector3 up = goT.up;
		Vector3 forward = goT.forward;
		Vector3 right = goT.right;
		int num = handIndex * 2 - 1;
		Vector3 vector = right * (0f - (float)num);
		Vector3 position2 = goT.position;
		Vector3 vector2 = position2 + up * handHeight + vector * handWidth + forward * -0.1f;
		int num2 = 4;
		float num3 = 0.2f;
		HandInfo handInfo = handInfos[handIndex];
		if (handCounters[handIndex] % num2 == 0)
		{
			Rigidbody rigidBody = legs.GetRigidBody();
			float num4 = 0.65f;
			Vector3 vector3 = vector2 + (-up * 2.5f + vector).normalized * num4;
			Vector3 vector4 = Util.ProjectOntoPlane(rigidBody.velocity - GetRigidBodyBelowVelocity(), Vector3.up);
			float magnitude = vector4.magnitude;
			Vector3 normalized = vector4.normalized;
			float num5 = Mathf.Min(1f, magnitude * 0.06f);
			handInfo.lastParentUpdatePos = position2;
			if (onGround)
			{
				handInfo.pendlumTimer += num5;
				if (handInfo.pendlumTimer > (float)Math.PI * 2f)
				{
					handInfo.pendlumTimer -= (float)Math.PI * 2f;
				}
				handInfo.pendlumAmp = Mathf.Min(0.7f, magnitude * 0.15f);
			}
			else
			{
				for (int i = 0; i < num2; i++)
				{
					handInfo.pendlumTimer *= 0.98f;
					handInfo.pendlumAmp *= 0.98f;
				}
			}
			Vector3 vector5 = handInfo.pendlumAmp * forward * num * Mathf.Sin(handInfo.pendlumTimer * (float)num2);
			vector3 += vector5;
			Vector3 rhs = vector4;
			float num6 = 5f;
			if (magnitude > num6)
			{
				rhs = normalized * num6;
			}
			Vector3 vector6 = 0.1f * (forward * Vector3.Dot(forward, rhs));
			Vector3 vector7 = 0.1f * (up * magnitude * Mathf.Clamp(Vector3.Dot(up, normalized), -0.5f, 1f));
			if (onGround)
			{
				vector6 *= 0.2f;
				vector7 *= 0.2f;
			}
			Vector3 vector8 = vector6 + vector7;
			vector3 += vector8;
			Vector3 rhs2 = up - Vector3.up;
			rhs2 = Vector3.Dot(forward, rhs2) * forward + Mathf.Max(0f, Vector3.Dot(vector, rhs2)) * vector + Vector3.Dot(up, rhs2) * up;
			vector3 += rhs2;
			Vector3 vector9 = vector3 - vector2;
			float magnitude2 = vector9.magnitude;
			Vector3 normalized2 = vector9.normalized;
			float num7 = magnitude2 - num4;
			float num8 = 1f;
			float num9 = 0.6f;
			if (magnitude2 > num8)
			{
				vector3 = vector2 + normalized2 * num8;
			}
			else if (magnitude2 < num9)
			{
				vector3 = vector2 + normalized2 * num9;
			}
			else
			{
				vector3 += -0.1f * num7 * normalized2;
			}
			handTargets[handIndex] = vector3;
		}
		else
		{
			Vector3 vector10 = position2 - handInfo.lastParentUpdatePos;
			handTargets[handIndex] += vector10;
			handInfo.lastParentUpdatePos = position2;
		}
		handCounters[handIndex]++;
		transform.position = num3 * handTargets[handIndex] + (1f - num3) * position;
		transform.LookAt(vector2, vector);
		transform.Rotate(0f, 0f, 90f);
	}

	public bool IsOnGround()
	{
		return onGround;
	}

	public void ClearIgnoreColliders()
	{
		if (ignoreColliders != null)
		{
			ignoreColliders.Clear();
			ignoreColliders = null;
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void __PatchPhysicsSettings()
	{
		Time.fixedDeltaTime = 0.01f;
		Physics.defaultSolverIterations = 10;
		Physics.defaultSolverVelocityIterations = 10;
	}
}
