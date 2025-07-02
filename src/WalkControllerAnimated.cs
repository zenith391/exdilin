using System;
using System.Collections.Generic;
using Blocks;
using Gestures;
using UnityEngine;

public class WalkControllerAnimated
{
	public enum VicinityMode
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

	private bool requestingTranslate;

	private Vector3 aggregatedTranslateRequest;

	private bool requestingDPadControl;

	private Vector2 dPadControlRequestDir;

	private float dPadControlRequestSpeed;

	private bool requestingTurn;

	private float turnRequestSpeed;

	private bool requestingTurnToTag;

	private string turnToTagRequestStr;

	private bool requestingTurnToTap;

	private bool requestingGoToTag;

	private Block gotoTagBlock;

	private Vector3 gotoTagTarget;

	private string gotoTagRequestStr;

	private float gotoTagRequestSpeed;

	private bool requestingGoToTap;

	private float gotoTapRequestSpeed;

	private bool requestingTurnAlongCamera;

	private bool wasRequestingTapAction;

	private Chunk chunk;

	private Rigidbody rigidBodyBelow;

	private Vector3 rigidBodyBelowVelocity;

	private HandInfo[] handInfos;

	private HashSet<Collider> ignoreColliders;

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

	public Vector3 currentMoveDirection;

	public Vector3 currentFaceDirection;

	private Vector3 wantedVel;

	public float defaultMaxSpeed;

	public float defaultAvoidDistance;

	private Dictionary<string, float> avoidDistances;

	private Dictionary<string, float> avoidApplications;

	public float avoidMaxSpeed;

	public float defaultDPadMaxSpeed;

	public float torqueMultiplier;

	public float addedTorqueMult;

	private float highSpeedFraction;

	private float tapActivatedTime;

	private bool onGround;

	private bool nearGround;

	public Vector3 groundNormal;

	private float onGroundHeight;

	private Vector3 onGroundPoint;

	public float groundFriction;

	private const float defaultGroundFriction = 0.4f;

	public float groundBounciness;

	public Vector3 slideVelocity;

	public bool onMovingObject;

	public Block onGroundBlock;

	private string dPadMoveKey;

	public VicinityMode vicinityMode;

	public VicinityMode previousVicinityMode;

	private Vector3 gotoOffset;

	private Vector3 avoidVector;

	private float gotoMaxSpeed;

	public BlockWalkable legs;

	public BlockAnimatedCharacter character;

	public List<WalkControllerAnimated> chunkControllers;

	private List<WalkControllerAnimated> sameUpControllers;

	private List<WalkControllerAnimated> conflictingUpControllers;

	private HashSet<Block> chunkBlocks;

	private bool translating;

	private bool beingPulled;

	private bool wasPulled;

	private float prevAngleError;

	private Vector3 iUpAngleError;

	private float prevVelError;

	private float prevForwardAngleError;

	private float iForwardAngleSum;

	private float prevHeightError;

	private Vector3 currentVelFiltered;

	private Vector3 targetVelocity;

	public Vector3 totalForce;

	private Vector3 totalCmTorque;

	public float speedFraction;

	private CapsuleCollider capsule;

	private const float SPEED_FRACTION_THRESHOLD = 0.001f;

	private const float VEL_FILTER_ALPHA = 0.9f;

	private const float TRANSLATE_CONTROL_K = 10f;

	private const float TRANSLATE_CONTROL_D = 10f;

	private const float ANGLE_CONTROL_K = 2f;

	private const float ANGLE_CONTROL_D = 8f;

	private const float FORWARD_ANGLE_CONTROL_K = 2f;

	private const float FORWARD_ANGLE_CONTROL_D = 0.1f;

	private const float FORWARD_ANGLE_CONTROL_I = 5f;

	public Rigidbody legsRb;

	private Transform legsTransform;

	private const float BODY_HEIGHT_ADJUST = 4f;

	private const float standingAttackSpeedChange = 6f;

	private int[] handCounters;

	private Vector3[] handTargets;

	public float SetAddedTorqueMultiplier
	{
		set
		{
			addedTorqueMult = value * legs.GetBlockMetaData().massM;
		}
	}

	public WalkControllerAnimated(BlockWalkable target)
	{
		rigidBodyBelowVelocity = Vector3.zero;
		handInfos = new HandInfo[2]
		{
			new HandInfo(),
			new HandInfo()
		};
		jumpUp = Vector3.up;
		sideJumpVector = Vector3.up;
		heightTestOffset = 0.5f;
		downRayOffset = Vector3.zero;
		currentMoveDirection = Vector3.forward;
		currentFaceDirection = Vector3.forward;
		wantedVel = Vector3.zero;
		defaultMaxSpeed = 5f;
		defaultAvoidDistance = 3f;
		avoidDistances = new Dictionary<string, float>();
		avoidApplications = new Dictionary<string, float>();
		avoidMaxSpeed = 5f;
		defaultDPadMaxSpeed = 5f;
		torqueMultiplier = 1f;
		addedTorqueMult = 1f;
		tapActivatedTime = -1f;
		groundNormal = Vector3.zero;
		dPadMoveKey = "L";
		gotoOffset = Vector3.zero;
		avoidVector = Vector3.zero;
		gotoMaxSpeed = 8f;
		chunkControllers = new List<WalkControllerAnimated>();
		sameUpControllers = new List<WalkControllerAnimated>();
		conflictingUpControllers = new List<WalkControllerAnimated>();
		chunkBlocks = new HashSet<Block>();
		currentVelFiltered = Vector3.zero;
		targetVelocity = Vector3.zero;
		totalForce = Vector3.zero;
		totalCmTorque = Vector3.zero;
		speedFraction = 1f;
		handCounters = new int[2];
		handTargets = new Vector3[2];
		legs = target;
		character = target as BlockAnimatedCharacter;
		legsRb = legs.GetRigidBody();
		legsTransform = legs.goT;
		currentMoveDirection = legs.goT.forward;
		currentFaceDirection = currentMoveDirection;
		if (legsRb != null)
		{
			legsRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			legsRb.maxDepenetrationVelocity = 10f;
			legsRb.solverIterations = 8;
			legsRb.solverVelocityIterations = 4;
		}
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
		return wantedVel.sqrMagnitude;
	}

	public bool IsFPC()
	{
		if (legs == Blocksworld.blocksworldCamera.firstPersonBlock && !legs.unmoving)
		{
			return !legs.IsFixed();
		}
		return false;
	}

	private void AddJumpForce(float force, bool swim)
	{
		totalJumpEnergy += force / (float)(1 + jumpCountThisFrame * ((!swim) ? 1 : 4));
		jumpEnergyLeft = totalJumpEnergy;
		jumpTimer = 0.5f;
		jumpCountThisFrame++;
		legs.jumpCountdown = legs.startJumpCountdown;
		for (int i = 0; i < chunkControllers.Count; i++)
		{
			WalkControllerAnimated walkControllerAnimated = chunkControllers[i];
			walkControllerAnimated.legs.jumpCountdown = walkControllerAnimated.legs.startJumpCountdown;
		}
	}

	public void Jump(float force)
	{
		jumpPressed = true;
		float massM = legs.GetBlockMetaData().massM;
		if (jumpMode == JumpMode.Ready && !climbing)
		{
			if (onGround && legs.upright)
			{
				AddJumpForce(force * 50f * massM, swim: false);
				jumpUp = Vector3.up;
			}
			else if (BlockWater.BlockWithinWater(legs))
			{
				AddJumpForce(force * 50f * massM, swim: true);
				jumpUp = legs.goT.up;
			}
		}
	}

	public void SetChunk()
	{
		chunk = legs.chunk;
		chunkBlocks = new HashSet<Block>();
		foreach (Block block in chunk.blocks)
		{
			chunkBlocks.Add(block);
			if (block is BlockWalkable && block != legs)
			{
				BlockWalkable blockWalkable = (BlockWalkable)block;
				if (blockWalkable.walkController != null)
				{
					chunkControllers.Add(blockWalkable.walkController);
				}
			}
		}
		foreach (WalkControllerAnimated chunkController in chunkControllers)
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
		if (!requestingTurn && !requestingTranslate && !requestingTurnToTag && !requestingDPadControl && !requestingTurnAlongCamera)
		{
			return vicinityMode != VicinityMode.None;
		}
		return true;
	}

	public bool GotDeliberateMovement()
	{
		return translating;
	}

	public void StartPull()
	{
		beingPulled = true;
	}

	public void StopPull()
	{
		beingPulled = false;
		wasPulled = true;
	}

	public void CancelPull()
	{
		beingPulled = false;
		wasPulled = false;
	}

	public bool WasPulled()
	{
		return wasPulled;
	}

	public void Translate(Vector3 dir, float maxSpeed)
	{
		requestingTranslate |= maxSpeed > 0f;
		aggregatedTranslateRequest += maxSpeed * dir.normalized;
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
		avoidMaxSpeed = Mathf.Max(analog * maxSpeed + 2.5f * Mathf.Max(0f, num - 1f), avoidMaxSpeed);
	}

	public void Turn(float speed)
	{
		if (Mathf.Abs(speed) > Mathf.Epsilon)
		{
			requestingTurn = true;
			turnRequestSpeed += speed;
		}
	}

	public void TurnTowardsTag(string tagName)
	{
		requestingTurnToTag = true;
		turnToTagRequestStr = tagName;
	}

	public void DPadControl(string key, float maxSpeed)
	{
		if (key == "L" && maxSpeed > 0f)
		{
			Vector2 vector = ((!Blocksworld.UI.Controls.IsDPadActive(key)) ? Vector2.zero : Blocksworld.UI.Controls.GetNormalizedDPadOffset(key));
			requestingDPadControl = vector.sqrMagnitude > Mathf.Epsilon;
			if (requestingDPadControl)
			{
				dPadControlRequestDir = vector.normalized;
				dPadControlRequestSpeed = maxSpeed * Mathf.Min(1f, vector.magnitude);
			}
		}
	}

	public void TiltMoverControl(Vector2 tiltVector)
	{
		requestingDPadControl = true;
		dPadControlRequestDir = tiltVector.normalized;
		dPadControlRequestSpeed = tiltVector.magnitude;
	}

	public bool TryGetClosestBlockWithTag(string tagName, out Block targetBlock, bool allowChunk = false)
	{
		List<Block> blocksWithTag = TagManager.GetBlocksWithTag(tagName);
		targetBlock = null;
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
						targetBlock = block;
						result = true;
					}
				}
			}
			return result;
		}
		return false;
	}

	public bool AtBlock(Block block, out Vector3 gotoPoint, float distCheck = 1f)
	{
		Collider component = block.go.GetComponent<Collider>();
		Vector3 position = legs.goT.position;
		Bounds bounds = component.bounds;
		gotoPoint = ((!bounds.Contains(position)) ? bounds.ClosestPoint(position) : block.GetPosition());
		if ((gotoPoint - position).magnitude < distCheck)
		{
			return true;
		}
		Ray ray = new Ray(position, (gotoPoint - position).normalized);
		RaycastHit hitInfo;
		return component.Raycast(ray, out hitInfo, distCheck);
	}

	public bool GotoTag(string tagName, float maxSpeed)
	{
		gotoTagBlock = null;
		requestingGoToTag = TryGetClosestBlockWithTag(tagName, out gotoTagBlock);
		if (requestingGoToTag)
		{
			gotoTagRequestStr = tagName;
			gotoTagRequestSpeed = maxSpeed;
			bool flag = AtBlock(gotoTagBlock, out gotoTagTarget, 1.5f);
			if (flag)
			{
				requestingGoToTag = false;
			}
			return !flag;
		}
		return false;
	}

	public void ChaseTag(string tagName, float maxSpeed)
	{
		gotoTagBlock = null;
		requestingGoToTag = TryGetClosestBlockWithTag(tagName, out gotoTagBlock);
		if (requestingGoToTag)
		{
			gotoTagTarget = gotoTagBlock.GetCenter();
			Vector3 vector = gotoTagTarget - legs.goT.position;
			vector.y = 0f;
			if (vector.sqrMagnitude < 0.25f)
			{
				requestingGoToTag = false;
				return;
			}
			gotoTagRequestStr = tagName;
			gotoTagRequestSpeed = maxSpeed;
		}
	}

	public void GotoTap(float maxSpeed)
	{
		requestingGoToTap = true;
		gotoTapRequestSpeed = maxSpeed;
	}

	public void TurnTowardsTap()
	{
		requestingTurnToTap = true;
	}

	public void TurnAlongCamera()
	{
		requestingTurnAlongCamera = true;
	}

	public void SetCapsuleCollider(CapsuleCollider c)
	{
		capsule = c;
	}

	public void AddIgnoreCollider(Collider c)
	{
		if (ignoreColliders == null)
		{
			GatherIgnoreColliders();
		}
		if (c != null)
		{
			ignoreColliders.Add(c);
		}
	}

	private void GatherIgnoreColliders()
	{
		ignoreColliders = new HashSet<Collider>();
		if (legs?.feet != null && legs.feet.Length != 0)
		{
			int num = Mathf.Min(legs.feet.Length, 2 * legs.legPairCount);
			for (int i = 0; i < num; i++)
			{
				Collider collider = legs.feet[i].collider;
				if (collider != null)
				{
					ignoreColliders.Add(collider);
				}
			}
		}
		if (!(legs?.body != null))
		{
			return;
		}
		Transform[] componentsInChildren = legs.body.GetComponentsInChildren<Transform>();
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			Collider component = componentsInChildren[j].gameObject.GetComponent<Collider>();
			if (component != null)
			{
				ignoreColliders.Add(component);
			}
		}
		if (chunkControllers == null)
		{
			return;
		}
		foreach (WalkControllerAnimated chunkController in chunkControllers)
		{
			if (chunkController?.legs?.feet == null)
			{
				continue;
			}
			int num2 = Mathf.Min(chunkController.legs.feet.Length, 2 * chunkController.legs.legPairCount);
			for (int k = 0; k < num2; k++)
			{
				Collider collider2 = chunkController.legs.feet[k].collider;
				if (collider2 != null)
				{
					ignoreColliders.Add(collider2);
				}
			}
		}
	}

	private bool DoGroundRaycast(Vector3 fromPosition, Vector3 direction, float maxDistance, out RaycastHit hit)
	{
		RaycastHit[] array = Physics.RaycastAll(fromPosition, direction, maxDistance, BlockWalkable.raycastMask);
		if (ignoreColliders == null)
		{
			GatherIgnoreColliders();
		}
		hit = default(RaycastHit);
		float num = float.MaxValue;
		bool result = false;
		RaycastHit[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			RaycastHit raycastHit = array2[i];
			Collider collider = raycastHit.collider;
			if (collider != null && collider.gameObject != null && !collider.isTrigger && !ignoreColliders.Contains(collider))
			{
				float sqrMagnitude = (raycastHit.point - fromPosition).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					hit = raycastHit;
					result = true;
				}
			}
		}
		return result;
	}

	private bool DoGroundRaycastWithNormalCheck(Vector3 fromPosition, Vector3 direction, float maxDistance, ref float closestDist, ref Vector3 closestPoint, ref Vector3 closestPointNormal, ref GameObject closestPointGameObject)
	{
		if (!DoGroundRaycast(fromPosition, direction, maxDistance, out var hit))
		{
			return false;
		}
		Vector3 vector = fromPosition - hit.point;
		if (Mathf.Clamp(Vector3.Dot(hit.normal, Vector3.up), 0f, 1f) > 0.2f)
		{
			float magnitude = vector.magnitude;
			if (magnitude < closestDist)
			{
				closestDist = magnitude;
				closestPoint = hit.point;
				closestPointNormal = hit.normal;
				closestPointGameObject = hit.collider.gameObject;
			}
			return true;
		}
		return false;
	}

	private bool DoOnGroundCheck()
	{
		if (legsTransform == null)
		{
			Debug.LogError("legsTransform is null. Make sure it's properly initialized.");
			return false;
		}
		if (capsule == null)
		{
			if (legs == null)
			{
				Debug.LogError("legs reference is null. Make sure it's properly initialized.");
				return false;
			}
			if (legs.go != null)
			{
				capsule = legs.go.GetComponent<CapsuleCollider>();
			}
			if (capsule == null)
			{
				Debug.LogError("CapsuleCollider is missing on the legs object's GameObject. Make sure to assign one via SetCapsuleCollider or attach one to the GameObject.");
				return false;
			}
		}
		float num = 1f;
		float num2 = num * 0.45f;
		float num3 = num * 0.3f;
		float num4 = num * 2f + num3;
		Vector3 vector = legsTransform.position + legsTransform.up * capsule.center.y;
		vector -= legsTransform.up * (capsule.height / 2f - capsule.radius);
		vector += Vector3.down * (capsule.radius - num3);
		Vector3 fromPosition = vector;
		float closestDist = num4;
		Vector3 closestPoint = Vector3.zero;
		Vector3 closestPointNormal = Vector3.up;
		GameObject closestPointGameObject = null;
		Vector3 direction = -Vector3.up;
		bool num5 = DoGroundRaycastWithNormalCheck(fromPosition, direction, num4, ref closestDist, ref closestPoint, ref closestPointNormal, ref closestPointGameObject);
		fromPosition = vector + legsTransform.forward * num2;
		bool num6 = num5 | DoGroundRaycastWithNormalCheck(fromPosition, direction, num4, ref closestDist, ref closestPoint, ref closestPointNormal, ref closestPointGameObject);
		fromPosition = vector - legsTransform.forward * num2;
		bool num7 = num6 | DoGroundRaycastWithNormalCheck(fromPosition, direction, num4, ref closestDist, ref closestPoint, ref closestPointNormal, ref closestPointGameObject);
		fromPosition = vector + legsTransform.right * num2;
		bool num8 = num7 | DoGroundRaycastWithNormalCheck(fromPosition, direction, num4, ref closestDist, ref closestPoint, ref closestPointNormal, ref closestPointGameObject);
		fromPosition = vector - legsTransform.right * num2;
		if (!(num8 | DoGroundRaycastWithNormalCheck(fromPosition, direction, num4, ref closestDist, ref closestPoint, ref closestPointNormal, ref closestPointGameObject)))
		{
			prevHeightError = 0f;
			onGround = false;
			nearGround = false;
			groundNormal = Vector3.zero;
			return false;
		}
		if (closestPointGameObject == null)
		{
			Debug.LogError("Ground raycast hit a null gameObject.");
			return false;
		}
		Transform parent = closestPointGameObject.transform.parent;
		if (parent != null)
		{
			AddRbBelowVelocity(closestPoint, parent.gameObject.GetComponent<Rigidbody>());
		}
		else
		{
			AddRbBelowVelocity(closestPoint, closestPointGameObject.GetComponent<Rigidbody>(), checkSurfaceBlocks: true);
		}
		onGroundPoint = closestPoint;
		groundNormal = closestPointNormal;
		onGroundHeight = Mathf.Max(Mathf.Epsilon, closestDist - num3);
		nearGround = true;
		onGround = onGroundHeight < 0.2f * num;
		onGroundBlock = ((onGroundHeight < num * (legs.onGroundHeight + 0.6f)) ? BWSceneManager.FindBlock(closestPointGameObject) : null);
		if (onGroundBlock != null)
		{
			Collider component = onGroundBlock.go.GetComponent<Collider>();
			if (component != null && component.material != null)
			{
				groundFriction = component.material.dynamicFriction;
				groundBounciness = component.material.bounciness;
			}
			else
			{
				groundFriction = 0.4f;
				groundBounciness = 0f;
			}
		}
		else
		{
			groundFriction = 0.4f;
			groundBounciness = 0f;
		}
		return onGround;
	}

	private void AddRbBelowVelocity(Vector3 hitPoint, Rigidbody rb, bool checkSurfaceBlocks = false)
	{
		rigidBodyBelow = rb;
		if (!(rigidBodyBelow != null) || rigidBodyBelow.isKinematic)
		{
			return;
		}
		rigidBodyBelowVelocity = rigidBodyBelow.velocity;
		Vector3 rhs = hitPoint - rigidBodyBelow.worldCenterOfMass;
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

	private void GotoFixedUpdate(float maxSpeed)
	{
		Vector3 worldCenterOfMass = legsRb.worldCenterOfMass;
		if (character == null)
		{
			return;
		}
		if (gotoMaxSpeed > 0.001f)
		{
			speedFraction = maxSpeed / gotoMaxSpeed;
		}
		else
		{
			speedFraction = 0f;
		}
		if (character.stateHandler.IsSwimming())
		{
			speedFraction *= 4f;
		}
		if (IsFPC() && (requestingTranslate || requestingDPadControl))
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
			BlockAnimatedCharacter firstPersonAnimatedCharacter = Blocksworld.blocksworldCamera.firstPersonAnimatedCharacter;
			if (firstPersonAnimatedCharacter != null && !firstPersonAnimatedCharacter.GetThinksOnGround() && Mathf.Abs(num) > 0.05f && Mathf.Abs(firstPersonDpad.y) < 0.1f)
			{
				firstPersonDpad.y = 0.1f;
			}
			currentMoveDirection = 0.2f * num * right + currentMoveDirection;
			currentMoveDirection.y = 0f;
			currentMoveDirection.Normalize();
			if (!character.stateHandler.walkStrafe)
			{
				currentFaceDirection = currentMoveDirection;
			}
			gotoOffset = 100f * (firstPersonDpad.x * right + firstPersonDpad.y * forward);
		}
		if (gotoOffset.z < 0f && Mathf.Abs(gotoOffset.x) <= 0.0001f)
		{
			gotoOffset.x = -0.05f * gotoOffset.z;
		}
		Vector3 vector = worldCenterOfMass + gotoOffset;
		character.stateHandler.desiredGoto = legsTransform.InverseTransformPoint(vector);
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
		float num2 = maxSpeed * character.stateHandler.GetSpeedForceModifier();
		Vector3 b = normalized * num2;
		float sqrMagnitude = avoidVector.sqrMagnitude;
		bool flag = sqrMagnitude > 0.01f;
		if (flag)
		{
			if (sqrMagnitude > 1f)
			{
				avoidVector.Normalize();
			}
			b += avoidVector * avoidMaxSpeed;
			float num3 = Mathf.Max(avoidMaxSpeed, gotoMaxSpeed);
			if (b.sqrMagnitude > num3 * num3)
			{
				b = wantedVel.normalized * num3;
			}
		}
		float num4 = 0f;
		if (gotoMaxSpeed > 0.01f)
		{
			num4 = currentVelFiltered.magnitude / gotoMaxSpeed;
		}
		float num5 = 0.6f;
		highSpeedFraction = num4 * Mathf.Clamp((gotoMaxSpeed - 7f) * 0.05f, 0f, 1f);
		if (highSpeedFraction > 0.01f)
		{
			num5 = Mathf.Min(1f, num5 + highSpeedFraction);
		}
		DoOnGroundCheck();
		if (onGround || wantedVel.sqrMagnitude < Mathf.Epsilon)
		{
			if (groundFriction < 0.4f)
			{
				float t = Mathf.Pow(groundFriction / 0.4f, 3f);
				b = Vector3.Lerp(wantedVel, b, t);
			}
			wantedVel = b;
		}
		else
		{
			wantedVel = Vector3.Lerp(wantedVel, b, 0.06f);
			wantedVel.y = b.y;
		}
		if (!onGround)
		{
			prevHeightError = 0f;
		}
		Vector3 vector3 = wantedVel - currentVelFiltered;
		vector3.y = 0f;
		float magnitude2 = vector3.magnitude;
		Vector3 normalized2 = vector3.normalized;
		float num6 = magnitude2 - prevVelError;
		prevVelError = magnitude2;
		Vector3 up = legsTransform.up;
		float num7 = Vector3.Angle(Vector3.up, up);
		float num8 = 1f;
		if (!onGround)
		{
			num8 = 0.6f;
			if (character != null && character.stateHandler.IsSwimming())
			{
				num8 *= 0.2f;
			}
		}
		float num9 = 1f + highSpeedFraction;
		translating = flag || gotoOffset.sqrMagnitude > 0.0001f || (magnitude2 > 1f && onGround);
		if (rigidBodyBelow != null)
		{
			rigidBodyBelowVelocity.y = 0f;
			float sqrMagnitude2 = rigidBodyBelowVelocity.sqrMagnitude;
			if (sqrMagnitude2 > 0.01f)
			{
				Vector3 vector4 = new Vector3(rigidBodyBelowVelocity.x, 0f, rigidBodyBelowVelocity.z);
				float num10 = Mathf.Clamp01((rigidBodyBelow.mass - 1f) / 5f);
				if (onGroundBlock != null && onGroundBlock is BlockAbstractPlatform)
				{
					num10 = 1f;
				}
				zero += num10 * vector4;
				onMovingObject = true;
			}
			else
			{
				onMovingObject = false;
			}
		}
		if (onGround)
		{
			Vector3 lhs = Vector3.Cross(Vector3.up, wantedVel);
			Vector3 vector5 = Vector3.Cross(lhs, groundNormal);
			Debug.DrawRay(legsRb.worldCenterOfMass, wantedVel, Color.red);
			Debug.DrawRay(legsRb.worldCenterOfMass, vector5, Color.blue);
			float num11 = Mathf.Clamp(Vector3.Dot(groundNormal, Vector3.up), 0f, 1f);
			float num12 = ((num11 >= 0.6f) ? Mathf.Min(num11 * num11 * 1.2f, 1f) : 0f);
			wantedVel = vector5 * num12;
		}
		zero += wantedVel;
		if (onGround && groundFriction < 0.4f)
		{
			float num13 = 1f - Mathf.Clamp(Vector3.Dot(groundNormal, Vector3.up), 0f, 1f);
			if (num13 < Mathf.Epsilon)
			{
				slideVelocity.y = 0f;
				float num14 = 1f - 0.25f * groundFriction / 0.4f;
				slideVelocity *= num14;
			}
			else
			{
				slideVelocity += 10f * num13 * Vector3.Cross(Vector3.Cross(Vector3.up, groundNormal), groundNormal).normalized * (0.4f - groundFriction) * Physics.gravity.magnitude * Time.fixedDeltaTime;
			}
			if (slideVelocity.magnitude >= 10f)
			{
				slideVelocity = slideVelocity.normalized * 10f;
			}
			zero += slideVelocity;
		}
		else
		{
			slideVelocity = Vector3.zero;
		}
		if (onGround || translating)
		{
			Vector3 vector6 = Vector3.Cross(up, Vector3.up);
			float num15 = Mathf.Clamp(num7 - prevAngleError, -5f, 5f);
			prevAngleError = 0f;
			float num16 = 700f * num9;
			float value = num9 * (0.1f + 0.9f * num8) * (num7 * 2f + num15 * 8f + 40f);
			value = Mathf.Clamp(value, 0f - num16, num16);
			Vector3 vector7 = ((character != null) ? (vector6 * value * 10f) : (vector6 * value));
			totalCmTorque += vector7;
			Vector3 angularVelocity = legsRb.angularVelocity;
			float num17 = ((character == null) ? 10f : 100f);
			float value2 = angularVelocity.magnitude * num17;
			float num18 = ((character == null) ? Mathf.Clamp(value2, 0f - num17, num17) : 10f);
			totalCmTorque += (0f - num8) * num18 * angularVelocity;
			float num19 = 3f;
			bool flag2;
			if (character.stateHandler.walkStrafe)
			{
				float num20 = Vector3.Angle(currentFaceDirection, legsTransform.forward);
				flag2 = num20 > 0.1f;
			}
			else
			{
				flag2 = ((double)magnitude2 > 0.02 && magnitude > num19) || requestingTurn || requestingTurnToTag || requestingTurnToTap || requestingDPadControl || requestingTurnAlongCamera || IsFPC();
			}
			if (flag2)
			{
				Vector3 forward2 = legsTransform.forward;
				if (Mathf.Abs(Vector3.Dot(forward2, Vector3.up)) < 0.25f)
				{
					forward2.y = 0f;
					float num21 = Util.AngleBetween(forward2, currentFaceDirection, Vector3.up);
					float num22 = Mathf.Clamp(num21 - prevForwardAngleError, -50f, 50f) / Blocksworld.fixedDeltaTime;
					prevForwardAngleError = 0f;
					if (Mathf.Abs(num21) < 5f)
					{
						iForwardAngleSum = Mathf.Clamp(iForwardAngleSum + Blocksworld.fixedDeltaTime * num21, -1000f, 1000f);
					}
					else
					{
						iForwardAngleSum = 0f;
					}
					vector6 = Vector3.Cross(forward2, currentFaceDirection);
					float num23 = Mathf.Clamp(num21 * 2f + num22 * 0.1f + iForwardAngleSum * 5f, 0f - num16, num16);
					if (IsFPC())
					{
						num23 *= Blocksworld.blocksworldCamera.firstPersonTorque;
					}
					num23 = ((!legs.ignoreRotation) ? num23 : 0f);
					totalCmTorque += num8 * up * num23 * (1f + highSpeedFraction);
				}
			}
		}
		else
		{
			prevAngleError = 0f;
		}
		character.stateHandler.requestedMoveVelocity = zero;
		Vector3 force = zero - legsRb.velocity;
		float num24 = 0f;
		if (character.stateHandler.IsWalking())
		{
			num24 = GetStepForce(zero, legsRb.velocity);
		}
		if (jumpPressed)
		{
			force.y = 0f;
		}
		else if (!onGround)
		{
			if (num24 < Mathf.Epsilon && legsRb.velocity.y > 2f && onGroundHeight > 0.1f)
			{
				force.y = -0.25f * (legsRb.velocity.y - 2f);
			}
			else
			{
				force.y = 0f;
			}
		}
		if (num24 > 0f && num24 > force.y)
		{
			force.y = num24;
		}
		legsRb.AddForce(force, ForceMode.VelocityChange);
		legsRb.AddTorque(totalCmTorque * torqueMultiplier * addedTorqueMult);
		totalForce = force;
	}

	public Vector3 GetDownSlopeDirection()
	{
		if (!onGround)
		{
			return Vector3.zero;
		}
		float num = 1f - Mathf.Clamp(Vector3.Dot(groundNormal, Vector3.up), 0f, 1f);
		if (num < Mathf.Epsilon)
		{
			return Vector3.zero;
		}
		Vector3 normalized = Vector3.Cross(Vector3.up, groundNormal).normalized;
		return Vector3.Cross(normalized, groundNormal).normalized;
	}

	private float GetStepForce(Vector3 desiredVelocity, Vector3 currentVelocity)
	{
		if (!nearGround)
		{
			return 0f;
		}
		float maxDistance = Mathf.Clamp(desiredVelocity.magnitude * 0.2f, 0.8f, 3f);
		Vector3 stepFwd = Vector3.Cross(legsTransform.right.normalized, Vector3.up);
		Vector3 vector = onGroundPoint + Vector3.up * 0.1f;
		Vector3 fromPosition = vector + Vector3.up * 0.95f / 2f;
		Vector3 fromPosition2 = vector + Vector3.up * 0.95f;
		bool flag = DoGroundRaycast(vector, stepFwd, maxDistance, out var hit);
		bool flag2 = DoGroundRaycast(fromPosition, stepFwd, maxDistance, out var hit2);
		bool flag3 = DoGroundRaycast(fromPosition2, stepFwd, maxDistance, out var hit3);
		Func<Vector3, bool> func = (Vector3 normal) => Vector3.Dot(normal, -stepFwd) > 0.35f;
		flag &= func(hit.normal);
		flag2 &= func(hit2.normal);
		flag3 &= func(hit3.normal);
		flag &= !(hit.collider is MeshCollider);
		flag2 &= !(hit2.collider is MeshCollider);
		if (!flag || flag3)
		{
			return 0f;
		}
		float num = ((!flag2) ? 1f : 4f);
		Vector3 vector2 = desiredVelocity - Vector3.up * desiredVelocity.y;
		Vector3 vector3 = desiredVelocity - currentVelocity;
		Vector3 vector4 = vector3 - Vector3.up * vector3.y;
		float num2 = Mathf.Max(0f, (vector2.magnitude - 0.15f) * 8f);
		float num3 = Mathf.Max(0f, (vector4.magnitude - 0.15f) * 12f);
		float num4 = Mathf.Clamp(num * (num2 + num3), 0f, 8f);
		float num5 = Mathf.Clamp01(2f - onGroundHeight);
		num4 *= num5 * num5;
		return num4 / 20f;
	}

	private void IdleFixedUpdate()
	{
		DoOnGroundCheck();
		if (beingPulled || wasPulled)
		{
			return;
		}
		float num = Vector3.Angle(legsTransform.up, Vector3.up);
		if (num < 10f)
		{
			legsRb.freezeRotation = true;
			if (character != null && rigidBodyBelow == null)
			{
				legsRb.AddForce(-legsRb.velocity, ForceMode.VelocityChange);
			}
		}
	}

	private void StandingAttackFixedUpdate()
	{
		if (!beingPulled && !wasPulled)
		{
			legsRb.freezeRotation = true;
			CharacterStateHandler stateHandler = (legs as BlockAnimatedCharacter).stateHandler;
			float magnitude = legsRb.velocity.magnitude;
			float num = magnitude;
			if (magnitude > stateHandler.standingAttackMaxSpeed)
			{
				num = Mathf.Lerp(magnitude, stateHandler.standingAttackMaxSpeed, 6f * Time.fixedDeltaTime);
			}
			else if (magnitude < stateHandler.standingAttackMinSpeed)
			{
				num = Mathf.Lerp(magnitude, stateHandler.standingAttackMinSpeed, 6f * Time.fixedDeltaTime);
			}
			legsRb.AddForce(num * stateHandler.standingAttackForward - legsRb.velocity, ForceMode.VelocityChange);
		}
	}

	public void AddJumpForce(float energy)
	{
		float num = 7f * energy * Blocksworld.fixedDeltaTime;
		Vector3 force = num * jumpUp + 0.2f * num * sideJumpVector;
		Rigidbody component = legs.body.GetComponent<Rigidbody>();
		Vector3 worldCenterOfMass = component.worldCenterOfMass;
		Vector3 position = 0.7f * worldCenterOfMass + 0.3f * legs.goT.position;
		component.AddForceAtPosition(force, position);
		jumpEnergyLeft -= num;
	}

	public Vector3 GetBounceVector(float bounciness)
	{
		jumpEnergyLeft = totalJumpEnergy;
		jumpCountThisFrame = 0;
		Rigidbody component = legs.body.GetComponent<Rigidbody>();
		Vector3 velocity = component.velocity;
		Vector3 vector = groundNormal;
		float num = velocity.magnitude * bounciness;
		Vector3 vector2 = Vector3.Reflect(velocity.normalized, groundNormal);
		return num * vector2;
	}

	public void Bounce(Vector3 bounceVector)
	{
		Rigidbody component = legs.body.GetComponent<Rigidbody>();
		component.AddForce(bounceVector - component.velocity, ForceMode.VelocityChange);
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
				if (character != null)
				{
					character.stateHandler.StartJump(totalJumpEnergy);
					jumpMode = JumpMode.WaitingForReady;
					totalJumpEnergy = 0f;
					jumpEnergyLeft = 0f;
				}
				else
				{
					AddJumpForce(totalJumpEnergy);
					jumpMode = JumpMode.AddingForce;
				}
				jumpCountThisFrame = 0;
				if (gotoOffset.sqrMagnitude > 1E-05f)
				{
					sideJumpVector = gotoOffset.normalized * speedFraction;
				}
				else
				{
					sideJumpVector = Vector3.zero;
				}
			}
			break;
		case JumpMode.AddingForce:
			AddJumpForce(totalJumpEnergy);
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
		if (legsRb == null)
		{
			legsRb = legs.GetRigidBody();
			legsTransform = legs.goT;
			if (legsRb == null)
			{
				return;
			}
		}
		character.stateHandler.requestedMoveVelocity = Vector3.zero;
		legsRb.freezeRotation = false;
		switch (vicinityMode)
		{
		case VicinityMode.None:
			avoidMaxSpeed = defaultMaxSpeed;
			break;
		case VicinityMode.AvoidTag:
			AvoidTagFixedUpdate();
			break;
		}
		bool flag = false;
		if (character.stateHandler.IsOnSide() || character.stateHandler.IsGetUpState())
		{
			SideFixedUpdate();
		}
		else if (character.stateHandler.IsImmobile())
		{
			IdleFixedUpdate();
		}
		else if (character.stateHandler.InStandingAttack())
		{
			StandingAttackFixedUpdate();
		}
		else if (character.isHovering)
		{
			DoOnGroundCheck();
		}
		else if (!beingPulled && !wasPulled && !character.isHovering)
		{
			flag = true;
			MoveFixedUpdate();
		}
		if (wasPulled)
		{
			if (DoGroundRaycast(legsTransform.position + 0.5f * Vector3.up, -Vector3.up, 3f, out var _))
			{
				onGround = true;
				wasPulled = false;
			}
			else
			{
				onGround = false;
			}
		}
		JumpFixedUpdate();
		if (!requestingTranslate && !requestingDPadControl && !requestingTurn && !requestingTurnToTag && !requestingDPadControl && !requestingTurnToTap && !requestingTurnAlongCamera)
		{
			prevAngleError = 0f;
		}
		previousVicinityMode = vicinityMode;
		vicinityMode = VicinityMode.None;
		gotoOffset = Vector3.zero;
		avoidVector = Vector3.zero;
		currentFaceDirection = legs.goT.forward;
		ResetMovementRequests();
		if (!flag)
		{
			wantedVel = Vector3.zero;
			prevVelError = 0f;
			character.stateHandler.desiredGoto = Vector3.zero;
		}
		gotoMaxSpeed = 0f;
		if (IsFPC())
		{
			Blocksworld.blocksworldCamera.firstPersonDpad = Vector2.zero;
			Blocksworld.blocksworldCamera.firstPersonRotation = 0f;
		}
	}

	private void MoveFixedUpdate()
	{
		bool isActive = Blocksworld.orbitDuringControlGesture.IsActive;
		float num = 0f;
		gotoMaxSpeed = defaultMaxSpeed;
		Vector3 vector = Vector3.zero;
		Vector3 vector2 = currentFaceDirection;
		bool flag = false;
		if (requestingTranslate || requestingDPadControl)
		{
			tapActivatedTime = -1f;
		}
		else if ((requestingGoToTap || requestingTurnToTap) && !wasRequestingTapAction)
		{
			tapActivatedTime = Time.time;
		}
		bool flag2 = TapControlGesture.HasWorldTapPos() && tapActivatedTime >= 0f && TapControlGesture.GetWorldTapTime() >= tapActivatedTime;
		if (requestingTranslate)
		{
			num = aggregatedTranslateRequest.magnitude;
			Vector3 normalized = aggregatedTranslateRequest.normalized;
			if (IsFPC())
			{
				Vector3 firstPersonDeadZone = Blocksworld.blocksworldCamera.firstPersonDeadZone;
				if (Mathf.Abs(normalized.x) < firstPersonDeadZone.x)
				{
					normalized.x = 0f;
				}
				else
				{
					normalized.x = Mathf.Sign(normalized.x) * (Mathf.Abs(normalized.x) - firstPersonDeadZone.x) / (1f - firstPersonDeadZone.x);
				}
				if (Mathf.Abs(normalized.y) < firstPersonDeadZone.y)
				{
					normalized.y = 0f;
				}
				else
				{
					normalized.y = Mathf.Sign(normalized.y) * (Mathf.Abs(normalized.y) - firstPersonDeadZone.y) / (1f - firstPersonDeadZone.y);
				}
				BlocksworldCamera blocksworldCamera = Blocksworld.blocksworldCamera;
				blocksworldCamera.firstPersonDpad.x = blocksworldCamera.firstPersonDpad.x - num * normalized.x;
				BlocksworldCamera blocksworldCamera2 = Blocksworld.blocksworldCamera;
				blocksworldCamera2.firstPersonDpad.y = blocksworldCamera2.firstPersonDpad.y + num * normalized.z;
			}
			Vector3 vector3 = Vector3.Cross(currentFaceDirection, Vector3.up);
			Vector3 vector4 = currentFaceDirection * normalized.z + vector3 * normalized.x;
			vector += aggregatedTranslateRequest.magnitude * 5f * vector4;
		}
		if (requestingGoToTag)
		{
			Vector3 vector5 = gotoTagTarget - legs.goT.position;
			vector5.y = 0f;
			vector += vector5.normalized * gotoTagRequestSpeed;
			num = SlowdownAtTarget(gotoTagRequestSpeed, vector5.magnitude);
		}
		if (requestingGoToTap && flag2)
		{
			Vector3 vector6 = TapControlGesture.GetWorldTapPos() - legs.goT.position;
			vector6.y = 0f;
			if (vector6.sqrMagnitude > 0.25f)
			{
				vector += vector6;
				num = SlowdownAtTarget(gotoTapRequestSpeed, vector6.magnitude);
			}
		}
		if (requestingTurnToTag && !flag)
		{
			Block targetBlock = null;
			if (TryGetClosestBlockWithTag(turnToTagRequestStr, out targetBlock))
			{
				Vector3 vector7 = targetBlock.goT.position - legs.goT.position;
				vector7.y = 0f;
				if (vector7.sqrMagnitude > 0.01f)
				{
					vector2 = vector7.normalized;
					flag = true;
				}
			}
		}
		if (requestingTurnAlongCamera && !flag)
		{
			Vector3 vector8 = Util.ProjectOntoPlane(Blocksworld.cameraForward, Vector3.up);
			if (vector8.sqrMagnitude > 0.001f)
			{
				vector2 = vector8.normalized;
				flag = true;
			}
		}
		if (requestingDPadControl)
		{
			if (IsFPC())
			{
				float angle = dPadControlRequestSpeed * (dPadControlRequestDir.x * 0.45f);
				Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);
				vector2 = quaternion * currentFaceDirection;
				vector2.y = 0f;
				if (vector2.sqrMagnitude > 0.01f)
				{
					vector2.Normalize();
				}
				flag = true;
				Vector3 firstPersonDeadZone2 = Blocksworld.blocksworldCamera.firstPersonDeadZone;
				if (Mathf.Abs(dPadControlRequestDir.y) < firstPersonDeadZone2.y)
				{
					dPadControlRequestDir.y = 0f;
				}
				else
				{
					dPadControlRequestDir.y = Mathf.Sign(dPadControlRequestDir.y) * (Mathf.Abs(dPadControlRequestDir.y) - firstPersonDeadZone2.y) / (1f - firstPersonDeadZone2.y);
				}
				BlocksworldCamera blocksworldCamera3 = Blocksworld.blocksworldCamera;
				blocksworldCamera3.firstPersonDpad.y = blocksworldCamera3.firstPersonDpad.y + dPadControlRequestSpeed * Mathf.Pow(dPadControlRequestDir.y, 3f);
			}
			Vector3 cameraUp = Blocksworld.cameraUp;
			Vector3 cameraRight = Blocksworld.cameraRight;
			Vector3 cameraForward = Blocksworld.cameraForward;
			float num2 = dPadControlRequestDir.y;
			if (cameraForward.y > 0f)
			{
				num2 *= -1f;
			}
			Vector3 vector9 = Util.ProjectOntoPlane(cameraUp, Vector3.up).normalized * num2;
			Vector3 vector10 = Util.ProjectOntoPlane(cameraRight, Vector3.up).normalized * dPadControlRequestDir.x;
			Vector3 vector11 = vector9 + vector10;
			if (!requestingTranslate)
			{
				vector += 100f * vector11;
			}
			num = dPadControlRequestSpeed;
			if (!flag)
			{
				if (isActive)
				{
					vector2 = Util.ProjectOntoPlane(cameraUp, Vector3.up).normalized;
				}
				else
				{
					vector2 = vector11.normalized;
					flag = true;
				}
			}
		}
		if (requestingTurn && !flag)
		{
			Quaternion quaternion2 = Quaternion.AngleAxis(turnRequestSpeed, Vector3.up);
			vector2 = quaternion2 * currentFaceDirection;
			vector2.y = 0f;
			if (vector2.sqrMagnitude > 0.01f)
			{
				vector2.Normalize();
			}
			flag = true;
			if (vector.sqrMagnitude > Mathf.Epsilon)
			{
				vector = quaternion2 * vector;
			}
			character.stateHandler.turnPower = turnRequestSpeed;
		}
		if ((requestingTurnToTap || requestingGoToTap) && flag2 && !flag)
		{
			Vector3 vector12 = TapControlGesture.GetWorldTapPos() - legs.goT.position;
			vector12.y = 0f;
			if (vector12.sqrMagnitude > 0.01f)
			{
				vector2 = vector12.normalized;
				flag = true;
			}
		}
		if (requestingGoToTag && !flag)
		{
			Block targetBlock2 = null;
			if (TryGetClosestBlockWithTag(gotoTagRequestStr, out targetBlock2))
			{
				Vector3 vector13 = targetBlock2.goT.position - legs.goT.position;
				vector13.y = 0f;
				if (vector13.sqrMagnitude > 0.01f)
				{
					vector2 = vector13.normalized;
					flag = true;
				}
			}
		}
		if (flag && ((requestingTurnToTap && flag2) || requestingTurnToTag || requestingTurnAlongCamera) && !IsFPC())
		{
			Vector3 vector14 = legs.goT.InverseTransformDirection(vector2.normalized);
			float x = vector14.x;
			float num3 = ((!requestingTurnAlongCamera) ? 0.45f : 0.05f);
			float num4 = ((!requestingTurnAlongCamera) ? 1f : 5f);
			x *= num4;
			if (Mathf.Abs(x) < num3)
			{
				x = 0f;
			}
			if (vector14.z < 0f && x == 0f)
			{
				x = -1f;
			}
			character.stateHandler.turnPower = x;
		}
		gotoOffset = (currentMoveDirection = vector);
		currentFaceDirection = vector2;
		float num5 = Vector3.Angle(currentMoveDirection, currentFaceDirection);
		bool walkStrafe = ((!requestingDPadControl && !requestingTurnAlongCamera) || !IsFPC()) && vector.sqrMagnitude > 0.05f && num5 > 10f;
		character.stateHandler.walkStrafe = walkStrafe;
		gotoMaxSpeed = Mathf.Max(gotoMaxSpeed, num);
		GotoFixedUpdate(num);
	}

	private float SlowdownAtTarget(float speed, float distanceToTarget)
	{
		if (distanceToTarget > 2f)
		{
			return speed;
		}
		return Mathf.Lerp(Mathf.Min(speed, 0.15f), speed, distanceToTarget / 2f);
	}

	private void ResetMovementRequests()
	{
		wasRequestingTapAction = (requestingGoToTap || requestingTurnToTap) && tapActivatedTime > 0f;
		requestingTranslate = false;
		requestingDPadControl = false;
		requestingTurn = false;
		requestingTurnToTag = false;
		requestingTurnToTap = false;
		requestingGoToTag = false;
		requestingGoToTap = false;
		requestingTurnAlongCamera = false;
		gotoTagBlock = null;
		gotoTagTarget = Vector3.zero;
		aggregatedTranslateRequest = Vector3.zero;
		turnRequestSpeed = 0f;
	}

	private void SideFixedUpdate()
	{
		DoOnGroundCheck();
		if (!beingPulled && !wasPulled)
		{
			legsRb.freezeRotation = true;
			character.goT.rotation = character.stateHandler.GetSideAnimRotation();
		}
	}

	public float GetAndResetHighSpeedFraction()
	{
		float result = highSpeedFraction;
		highSpeedFraction = 1f;
		return result;
	}

	public bool IsOnGround()
	{
		return onGround;
	}

	public float OnGroundHeight()
	{
		return onGroundHeight;
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
