using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockAbstractAntiGravity : BlockAbstractHover
{
	public static Predicate predicateTiltMover;

	public static Predicate predicateAlignToTilt;

	protected float extraModelGravityMultiplier;

	private float extraChunkGravityMultiplier;

	protected float alignInFieldChunkApplications;

	protected float alignToTiltChunkApplications;

	private Quaternion tiltAttitudeCorrect = Quaternion.identity;

	private Quaternion tiltAlignBaseAttitude;

	private Quaternion tiltAlignBaseRotation;

	private Vector3 tiltAlignTorque;

	private Vector3 tiltAlignTorqueDelta;

	private bool applyTiltAlign;

	private bool isTrackingTiltAlign;

	private PDControllerVector3 tiltAlignHeadingController;

	private PDControllerVector3 tiltAlignUpController;

	private PDControllerVector3 tiltAlignAngVelController;

	protected Vector3 positionInFieldChunkApplications = Vector3.zero;

	private Vector3 prevPositionInFieldChunkApplications = Vector3.zero;

	protected float chunkPositionErrorControlFactor = 1f;

	private Vector3 positionInFieldChunkOffsets = Vector3.zero;

	private Vector3 positionInFieldChunkOffsetTargets = Vector3.zero;

	private Vector3 positionInFieldChunkOffsetIncrements = Vector3.one;

	private float turnTowardsTagChunkApplications;

	private string turnTowardsTag = string.Empty;

	protected Vector3 turnAlongDirection = Vector3.zero;

	protected float turnAlongMaxAngVel = -1f;

	protected Vector3 targetAngVel = Vector3.zero;

	protected float targetAngVelApplications;

	private Vector3 targetVelocity = Vector3.zero;

	private float targetVelocityApplications;

	private Vector3 extraTorqueChunk = Vector3.zero;

	private Vector3[] positionInFieldPositions = new Vector3[3]
	{
		Vector3.zero,
		Vector3.zero,
		Vector3.zero
	};

	private bool[] positionInFieldHover = new bool[3];

	private Vector3 hoverInFieldDistances = Vector3.zero;

	private Vector3 hoverInFieldDistanceOffsets = Util.nullVector3;

	private Quaternion alignRotation = Quaternion.identity;

	private float inertiaBank = 1f;

	private float inertiaTurn = 1f;

	private float bankAngle;

	protected Vector2 currentDpad = Vector2.zero;

	private int treatAsVehicleStatus = -1;

	private AntigravityMetaData metaData;

	private List<BlockAbstractAntiGravity> modelAntigravityBlocks = new List<BlockAbstractAntiGravity>();

	private static RaycastHit tempHit;

	private static HashSet<Predicate> inertiaPredicates;

	private const float BANK_ANGLE_FACTOR = 80f;

	private const float BANK_ANGLE_LIMIT = 50f;

	private const float ALIGN_PER_INERTIA_FACTOR = 0.15f;

	private const float MINIMUM_TOLERANCE = 0.01f;

	private const float TOTAL_INERTIA_MIN_VALUE = 1f;

	private const float TOTAL_INERTIA_MAX_VALUE = 10000f;

	public Vector3 EARTH_GRAVITY => -9.82f * Vector3.up;

	public BlockAbstractAntiGravity(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	public new static void Register()
	{
		predicateTiltMover = PredicateRegistry.Add<BlockAbstractAntiGravity>("AntiGravity.TiltMover", null, (Block b) => b.TiltMoverControl, new Type[2]
		{
			typeof(float),
			typeof(int)
		});
		predicateAlignToTilt = PredicateRegistry.Add<BlockAbstractAntiGravity>("AntiGravity.AlignToTilt", null, (Block b) => ((BlockAbstractAntiGravity)b).AlignToTilt, new Type[1] { typeof(int) });
		PredicateRegistry.Add<BlockAbstractAntiGravity>("AntiGravity.ConstrainTranslation", (Block b) => b.IsConstrainTranslation, (Block b) => b.ConstrainTranslation, new Type[1] { typeof(int) });
		PredicateRegistry.Add<BlockAbstractAntiGravity>("AntiGravity.FreeTranslation", (Block b) => b.IsFreeTranslation, (Block b) => b.FreeTranslation, new Type[1] { typeof(int) });
		PredicateRegistry.Add<BlockAbstractAntiGravity>("AntiGravity.ConstrainRotation", (Block b) => b.IsConstrainRotation, (Block b) => b.ConstrainRotation, new Type[1] { typeof(int) });
		PredicateRegistry.Add<BlockAbstractAntiGravity>("AntiGravity.FreeRotation", (Block b) => b.IsFreeRotation, (Block b) => b.FreeRotation, new Type[1] { typeof(int) });
	}

	public override void OnCreate()
	{
		base.OnCreate();
		metaData = go.GetComponent<AntigravityMetaData>();
		if (metaData != null)
		{
			rotation = Quaternion.Euler(metaData.orientation);
		}
		else
		{
			BWLog.Info("Could not find antigravity meta data component in " + BlockType());
		}
	}

	public override void Play()
	{
		base.Play();
		if (chunk != null)
		{
			chunk.UpdateCenterOfMass();
		}
		treatAsVehicleStatus = -1;
		tiltAlignHeadingController = new PDControllerVector3(50f, 4f);
		tiltAlignUpController = new PDControllerVector3(50f, 4f);
		tiltAlignAngVelController = new PDControllerVector3(4f, 0f);
		hoverInFieldDistanceOffsets = Util.nullVector3;
		modelAntigravityBlocks.Clear();
		UpdateConnectedCache();
		List<Block> list = Block.connectedCache[this];
		for (int i = 0; i < list.Count; i++)
		{
			Block block = list[i];
			if (block is BlockAbstractAntiGravity)
			{
				modelAntigravityBlocks.Add((BlockAbstractAntiGravity)block);
			}
		}
		modelAntigravityBlocks.Sort(new BlockNameComparer<BlockAbstractAntiGravity>());
	}

	private void ApplyChunkVelocityForce(Vector3 targetVelocity)
	{
		if (Mathf.Abs(targetVelocityApplications) > 0.001f && chunkRigidBody != null && !chunkRigidBody.isKinematic)
		{
			Vector3 velocity = chunkRigidBody.velocity;
			Vector3 normalized = targetVelocity.normalized;
			UpdateConnectedCache();
			Vector3 modelAcceleration = BlockAccelerations.GetModelAcceleration(Block.connectedCache[this]);
			targetVelocity += modelAcceleration;
			Vector3 vector = targetVelocity - velocity;
			Vector3 vector2 = Vector3.Dot(normalized, vector) * normalized;
			Vector3 vector3 = vector - vector2;
			Vector3 vector4 = vector2 + 0.2f * vector3;
			float mass = chunkRigidBody.mass;
			float num = 25f * targetVelocityApplications;
			if (vector4.magnitude > 4f)
			{
				vector4 = vector4.normalized * 4f;
			}
			Vector3 force = vector4 * mass * num;
			chunkRigidBody.AddForce(force);
			Vector3 vec = targetVelocity * targetVelocityApplications;
			vec = Util.ProjectOntoPlane(vec, Vector3.up);
			Blocksworld.blocksworldCamera.AddForceDirectionHint(chunk, vec);
			float num2 = Mathf.Abs(force.magnitude / mass);
			sfxLoopStrength += num2 / 20f;
		}
	}

	private void ApplyChunkAngularVelocityTorque(Vector3 targetAngVel)
	{
		if (Mathf.Abs(targetAngVelApplications) > 0.001f && chunkRigidBody != null && !chunkRigidBody.isKinematic)
		{
			Vector3 angularVelocity = chunkRigidBody.angularVelocity;
			Vector3 vector = targetAngVel - angularVelocity;
			float mass = chunkRigidBody.mass;
			Vector3 vector2 = vector * mass;
			chunkRigidBody.AddTorque(vector2);
			float magnitude = (vector2 / mass).magnitude;
			sfxLoopStrength += magnitude / 100f;
		}
	}

	private void ApplyChunkExtraTorque(Vector3 extraTorque)
	{
		if (extraTorque.sqrMagnitude > 0.001f && chunkRigidBody != null && !chunkRigidBody.isKinematic)
		{
			float mass = chunkRigidBody.mass;
			Vector3 torque = extraTorque * mass;
			chunkRigidBody.AddTorque(torque);
			if (isTrackingTiltAlign)
			{
				Vector3 normalized = torque.normalized;
				float angle = torque.magnitude * Time.fixedDeltaTime;
				tiltAlignBaseRotation *= Quaternion.AngleAxis(angle, normalized);
			}
			float magnitude = extraTorque.magnitude;
			sfxLoopStrength += magnitude / 100f;
		}
	}

	protected void ApplyChunkAlignTorque(Vector3 field, Vector3 localUp, float applications, float maxAngVelMag = -1f)
	{
		float num = Mathf.Abs(applications);
		if (num > 0.01f && chunkRigidBody != null)
		{
			Vector3 vector = goT.TransformDirection(localUp);
			float a = Vector3.Angle(vector, -field);
			Vector3 vector2 = Vector3.Cross(field, vector);
			vector2 = ((!(vector2.sqrMagnitude > 0.001f)) ? goT.forward : vector2.normalized);
			float mass = chunkRigidBody.mass;
			float num2 = num;
			float num3 = 0.5f * num2;
			vector2 *= num3 * Mathf.Min(a, 90f) * mass;
			Vector3 angularVelocity = chunkRigidBody.angularVelocity;
			Vector3 normalized = field.normalized;
			Vector3 vector3 = Util.ProjectOntoPlane(angularVelocity, -normalized);
			float magnitude = vector3.magnitude;
			if (maxAngVelMag > 0f && magnitude > maxAngVelMag)
			{
				float num4 = magnitude - maxAngVelMag;
				vector2 *= 1f / (1f + 5f * num4);
			}
			float num5 = num2;
			float num6 = magnitude * num5 * mass;
			vector2 += (0f - num6) * vector3;
			chunkRigidBody.AddTorque(vector2);
		}
	}

	protected void ApplyTiltAlignChunkTorque()
	{
		if (applyTiltAlign && !(chunkRigidBody == null) && TiltManager.Instance.IsMonitoring())
		{
			Quaternion quaternion = Quaternion.Inverse(tiltAlignBaseAttitude) * TiltManager.Instance.GetCurrentAttitude();
			quaternion *= tiltAttitudeCorrect;
			Quaternion quaternion2 = tiltAlignBaseRotation * quaternion;
			if (false)
			{
				quaternion2 = Quaternion.Slerp(chunkRigidBody.rotation, quaternion2, 8f * Time.fixedDeltaTime);
				chunkRigidBody.MoveRotation(quaternion2);
				return;
			}
			Vector3 currentError = -chunkRigidBody.angularVelocity;
			Vector3 torque = tiltAlignAngVelController.Update(currentError, Time.fixedDeltaTime);
			Vector3 rhs = quaternion2 * Vector3.forward;
			Vector3 currentError2 = Vector3.Cross(chunkRigidBody.transform.forward, rhs);
			Vector3 torque2 = tiltAlignHeadingController.Update(currentError2, Time.fixedDeltaTime);
			Vector3 rhs2 = quaternion2 * Vector3.up;
			Vector3 currentError3 = Vector3.Cross(chunkRigidBody.transform.up, rhs2);
			Vector3 torque3 = tiltAlignUpController.Update(currentError3, Time.fixedDeltaTime);
			chunkRigidBody.AddTorque(torque, ForceMode.Acceleration);
			chunkRigidBody.AddTorque(torque2, ForceMode.Acceleration);
			chunkRigidBody.AddTorque(torque3, ForceMode.Acceleration);
		}
	}

	private bool GetClosestGroundHit(Vector3 pos, ref RaycastHit hit, float maxDist = 100f)
	{
		RaycastHit[] array = Physics.RaycastAll(pos, Vector3.down, maxDist);
		Util.SmartSort(array, pos);
		RaycastHit[] array2 = array;
		foreach (RaycastHit raycastHit in array2)
		{
			Block block = BWSceneManager.FindBlock(raycastHit.collider.gameObject);
			if (block != null)
			{
				if (block.isTerrain)
				{
					return true;
				}
				if (block.modelBlock != modelBlock)
				{
					return true;
				}
			}
		}
		return false;
	}

	private float GetDistanceToGround(Vector3 pos)
	{
		if (GetClosestGroundHit(pos, ref tempHit))
		{
			return tempHit.distance;
		}
		return 200f;
	}

	private void ApplyChunkPositionForces()
	{
		for (int i = 0; i < 3; i++)
		{
			float num = positionInFieldChunkApplications[i];
			float num2 = prevPositionInFieldChunkApplications[i];
			if (num > 0.01f && chunkRigidBody != null)
			{
				bool flag = positionInFieldHover[i];
				Vector3 worldCenterOfMass = chunkRigidBody.worldCenterOfMass;
				float num3 = hoverInFieldDistanceOffsets[i];
				Vector3 zero = Vector3.zero;
				zero[i] = 1f;
				if (num2 < 0.01f)
				{
					if (flag && Util.IsNullVector3Component(num3, i))
					{
						num3 = Mathf.Min(50f, GetDistanceToGround(worldCenterOfMass)) + 2.5f * (1f + extraModelGravityMultiplier);
						hoverInFieldDistanceOffsets[i] = num3;
					}
					else
					{
						positionInFieldPositions[i] = worldCenterOfMass;
					}
					positionInFieldChunkOffsets[i] = 0f;
				}
				float num4 = positionInFieldChunkOffsets[i];
				float num5 = positionInFieldChunkOffsetTargets[i];
				float num6 = num5 - num4;
				float num7 = Mathf.Abs(num6);
				float num8 = positionInFieldChunkOffsetIncrements[i];
				if (num7 < num8)
				{
					ref Vector3 reference = ref positionInFieldChunkOffsets;
					int index;
					positionInFieldChunkOffsets[index = i] = reference[index] + num6;
				}
				else
				{
					ref Vector3 reference2 = ref positionInFieldChunkOffsets;
					int index2;
					positionInFieldChunkOffsets[index2 = i] = reference2[index2] + Mathf.Sign(num6) * num8;
				}
				float num9 = positionInFieldChunkOffsets[i];
				Vector3 vector = zero * num9;
				Vector3 vector2;
				if (!flag)
				{
					vector2 = positionInFieldPositions[i] + vector;
				}
				else
				{
					float distanceToGround = GetDistanceToGround(worldCenterOfMass);
					if (distanceToGround > 100f)
					{
						vector2 = positionInFieldPositions[i] + vector;
					}
					else
					{
						vector2 = worldCenterOfMass + zero * (hoverInFieldDistances[i] + num3 + num9 - distanceToGround);
						positionInFieldPositions[i] = worldCenterOfMass - vector;
					}
				}
				Vector3 rhs = vector2 - worldCenterOfMass;
				rhs = zero * Vector3.Dot(zero, rhs);
				float magnitude = rhs.magnitude;
				if ((double)magnitude > 0.1)
				{
					float num10 = Vector3.Dot(rhs, zero);
					float num11 = num * chunkPositionErrorControlFactor;
					float num12 = num * 4f;
					float num13 = Mathf.Clamp(num11 * num10, 0f - num12, num12);
					float num14 = Vector3.Dot(chunkRigidBody.velocity, zero);
					float num15 = num13 - num14;
					float num16 = 4f * num;
					float num17 = ((!flag) ? chunkRigidBody.mass : totalMassModel);
					float num18 = 25f * num17 * num;
					float num19 = Mathf.Clamp(num17 * (num16 * num15), 0f - num18, num18);
					chunkRigidBody.AddForce(zero * num19);
					float num20 = Mathf.Abs(num19 / num17);
					sfxLoopStrength += num20 / 20f;
				}
			}
			prevPositionInFieldChunkApplications[i] = num;
		}
	}

	private void ApplyModelGravityForce()
	{
		if (modelAntigravityBlocks[0] != this)
		{
			return;
		}
		float num = 0f;
		float num2 = 0f;
		for (int i = 0; i < modelAntigravityBlocks.Count; i++)
		{
			BlockAbstractAntiGravity blockAbstractAntiGravity = modelAntigravityBlocks[i];
			float a = blockAbstractAntiGravity.extraModelGravityMultiplier;
			num = Mathf.Min(a, num);
			num2 = Mathf.Max(a, num2);
		}
		float num3 = 0f;
		float num4 = Mathf.Abs(num);
		if (num4 > num2)
		{
			num3 = num;
		}
		else if (num4 < num2)
		{
			num3 = num2;
		}
		if (!(Mathf.Abs(num3) > 0.01f))
		{
			return;
		}
		foreach (Rigidbody allRigidbody in allRigidbodies)
		{
			if (!(allRigidbody == null))
			{
				AddGravityForce(allRigidbody, num3, allRigidbody.mass);
				if (varyingMassBlocksModel.Count > 0 && varyingMassBlocksModel.TryGetValue(allRigidbody, out var value))
				{
					float varyingMassOffset = GetVaryingMassOffset(value);
					AddGravityForce(allRigidbody, num3, varyingMassOffset);
				}
			}
		}
	}

	private void ApplyChunkGravityForce()
	{
		if (Mathf.Abs(extraChunkGravityMultiplier) > 0.01f && chunkRigidBody != null)
		{
			AddGravityForce(chunkRigidBody, extraChunkGravityMultiplier, chunkRigidBody.mass);
			if (varyingMassBlocksChunk.Count > 0)
			{
				AddGravityForce(chunkRigidBody, extraChunkGravityMultiplier, GetVaryingMassOffset(varyingMassBlocksChunk));
			}
		}
	}

	private void ApplyChunkTurnTowardsTag()
	{
		float num = Mathf.Abs(turnTowardsTagChunkApplications);
		if (!(num > 0f) || turnTowardsTag.Length <= 0)
		{
			return;
		}
		Vector3 position = goT.position;
		if (chunkBlocksSet == null)
		{
			if (chunk.go != null)
			{
				chunkBlocksSet = new HashSet<Block>(chunk.blocks);
			}
			else
			{
				chunkBlocksSet = new HashSet<Block>();
			}
		}
		if (TagManager.TryGetClosestBlockWithTag(turnTowardsTag, position, out var block, chunkBlocksSet))
		{
			Vector3 vector = block.goT.position - position;
			if (vector.sqrMagnitude > 0.0001f)
			{
				ApplyChunkAlignTorque(Mathf.Sign(turnTowardsTagChunkApplications) * vector.normalized, -(rotation * Vector3.forward), Mathf.Abs(turnTowardsTagChunkApplications));
			}
		}
	}

	private void ApplyChunkTurnAlongDirection()
	{
		if (turnAlongDirection.sqrMagnitude > 0.1f)
		{
			float magnitude = turnAlongDirection.magnitude;
			ApplyChunkAlignTorque(turnAlongDirection.normalized, -(rotation * Vector3.forward), magnitude, turnAlongMaxAngVel);
		}
	}

	public override void ResetFrame()
	{
		base.ResetFrame();
		ResetActions();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		ApplyModelGravityForce();
		if (chunk != null && chunk.mobileCharacter != null)
		{
			chunk.mobileCharacter.isHovering = !didFix && targetVelocity.magnitude > 0f;
		}
		if (!didFix && !broken)
		{
			ApplyChunkGravityForce();
			Vector3 eARTH_GRAVITY = EARTH_GRAVITY;
			eARTH_GRAVITY = alignRotation * eARTH_GRAVITY;
			if (eARTH_GRAVITY.sqrMagnitude < 0.01f)
			{
				eARTH_GRAVITY = -Vector3.up;
			}
			ApplyTiltAlignChunkTorque();
			ApplyChunkAlignTorque(eARTH_GRAVITY, rotation * Vector3.up, alignInFieldChunkApplications);
			ApplyChunkTurnTowardsTag();
			ApplyChunkTurnAlongDirection();
			ApplyChunkPositionForces();
			ApplyChunkAngularVelocityTorque(targetAngVel);
			ApplyChunkExtraTorque(extraTorqueChunk);
			ApplyChunkVelocityForce(targetVelocity);
			currentDpad = Vector2.Lerp(Vector2.zero, currentDpad, 0.95f);
		}
		isTrackingTiltAlign = applyTiltAlign;
		applyTiltAlign = false;
		UpdateSFXs();
	}

	public override void Stop(bool resetBlock = true)
	{
		base.Stop(resetBlock);
		chunkBlocksSet = null;
		currentDpad.Set(0f, 0f);
	}

	private void ResetActions()
	{
		extraModelGravityMultiplier = 0f;
		extraChunkGravityMultiplier = 0f;
		alignInFieldChunkApplications = 0f;
		positionInFieldChunkApplications.Set(0f, 0f, 0f);
		positionInFieldHover[0] = false;
		positionInFieldHover[1] = false;
		positionInFieldHover[2] = false;
		turnTowardsTagChunkApplications = 0f;
		turnAlongDirection.Set(0f, 0f, 0f);
		turnAlongMaxAngVel = -1f;
		targetVelocityApplications = 0f;
		targetAngVelApplications = 0f;
		targetAngVel.Set(0f, 0f, 0f);
		extraTorqueChunk.Set(0f, 0f, 0f);
		targetVelocity.Set(0f, 0f, 0f);
		alignRotation.Set(0f, 0f, 0f, 1f);
	}

	public static HashSet<Predicate> GetInertiaPredicates()
	{
		if (inertiaPredicates == null)
		{
			inertiaPredicates = new HashSet<Predicate>
			{
				BlockFlightYoke.predicateBankTurn,
				BlockAntiGravity.predicateAntigravityBankTurn,
				BlockAntiGravityColumn.predicateAntigravityColumnBankTurn,
				BlockFlightYoke.predicateFlightSim,
				BlockFlightYoke.predicateTiltFlightSim
			};
		}
		return inertiaPredicates;
	}

	public override void Play2()
	{
		base.Play2();
		ResetActions();
		prevPositionInFieldChunkApplications = Vector3.zero;
		positionInFieldChunkOffsets = Vector3.zero;
		positionInFieldChunkOffsetTargets = Vector3.zero;
		positionInFieldChunkOffsetIncrements = Vector3.one * Blocksworld.fixedDeltaTime;
		if (ContainsTileWithAnyPredicateInPlayMode2(GetInertiaPredicates()))
		{
			Transform transform = goT;
			inertiaBank = GetInertia(transform.TransformDirection(rotation * Vector3.forward));
			inertiaTurn = GetInertia(transform.TransformDirection(rotation * Vector3.up));
		}
		else
		{
			inertiaBank = 1f;
			inertiaTurn = 1f;
		}
	}

	public TileResultCode TurnTowardsTagChunk(ScriptRowExecutionInfo eInfo, object[] args)
	{
		turnTowardsTag = ((args.Length == 0) ? string.Empty : ((string)args[0]));
		float num = ((args.Length <= 1) ? 1f : ((float)args[1]));
		turnTowardsTagChunkApplications += eInfo.floatArg * num;
		return TileResultCode.True;
	}

	public TileResultCode AlignInGravityFieldChunk(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float num = ((args.Length == 0) ? 1f : ((float)args[0]));
		alignInFieldChunkApplications += eInfo.floatArg * num;
		return TileResultCode.True;
	}

	public TileResultCode AlignTerrainChunk(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float num = ((args.Length == 0) ? 1f : ((float)args[0]));
		alignInFieldChunkApplications += eInfo.floatArg * num;
		Vector3 position = goT.position;
		if (GetClosestGroundHit(position, ref tempHit, 50f))
		{
			float num2 = 3f / Mathf.Max(tempHit.distance, 3f);
			alignRotation = Quaternion.FromToRotation(Vector3.up, num2 * tempHit.normal + (1f - num2) * Vector3.up);
		}
		return TileResultCode.True;
	}

	public TileResultCode AlignToTilt(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (chunkRigidBody == null || !TiltManager.Instance.IsMonitoring())
		{
			return TileResultCode.True;
		}
		if (!isTrackingTiltAlign)
		{
			bool flag = Util.GetIntArg(args, 0, 0) > 0;
			tiltAlignBaseAttitude = TiltManager.Instance.GetCurrentAttitude();
			if (flag)
			{
				Vector3 gravityVector = TiltManager.Instance.GetGravityVector();
				tiltAlignBaseAttitude *= Quaternion.FromToRotation(gravityVector, -Vector3.forward);
			}
			tiltAttitudeCorrect = Quaternion.FromToRotation(Vector3.forward, Vector3.up);
			tiltAttitudeCorrect *= Quaternion.Inverse(goT.rotation);
			tiltAttitudeCorrect *= Quaternion.Inverse(rotation);
			tiltAlignBaseAttitude *= tiltAttitudeCorrect;
			tiltAlignBaseRotation = chunkRigidBody.rotation;
			tiltAlignTorque = (tiltAlignTorqueDelta = Vector3.zero);
			tiltAlignHeadingController.Reset();
			tiltAlignUpController.Reset();
			tiltAlignAngVelController.Reset();
		}
		Blocksworld.UI.Controls.UpdateTiltPrompt();
		isTrackingTiltAlign = true;
		applyTiltAlign = true;
		return TileResultCode.True;
	}

	private float GetGravityInfluence(object[] args, float arg1)
	{
		return arg1 * ((args.Length == 0) ? 0f : ((float)args[0]));
	}

	public TileResultCode IncreaseModelGravityInfluence(ScriptRowExecutionInfo eInfo, object[] args)
	{
		extraModelGravityMultiplier += GetGravityInfluence(args, eInfo.floatArg);
		return TileResultCode.True;
	}

	public TileResultCode IncreaseChunkGravityInfluence(ScriptRowExecutionInfo eInfo, object[] args)
	{
		extraChunkGravityMultiplier += GetGravityInfluence(args, eInfo.floatArg);
		return TileResultCode.True;
	}

	public TileResultCode IncreaseChunkGlobalAngularVelocity(ScriptRowExecutionInfo eInfo, object[] args)
	{
		Vector3 vector = ((args.Length == 0) ? Vector3.zero : ((Vector3)args[0]));
		float num = ((args.Length <= 1) ? 1f : ((float)args[1]));
		vector *= eInfo.floatArg * num;
		targetAngVel += vector;
		targetAngVelApplications += eInfo.floatArg;
		return TileResultCode.True;
	}

	public TileResultCode IncreaseLocalAngularVelocityChunk(ScriptRowExecutionInfo eInfo, object[] args)
	{
		Vector3 direction = ((args.Length == 0) ? Vector3.zero : ((Vector3)args[0]));
		float num = ((args.Length <= 1) ? 2f : ((float)args[1]));
		direction *= eInfo.floatArg * num;
		Vector3 vector = goT.TransformDirection(direction);
		targetAngVel += vector;
		targetAngVelApplications += eInfo.floatArg;
		return TileResultCode.True;
	}

	public TileResultCode IncreaseLocalTorqueChunk(ScriptRowExecutionInfo eInfo, object[] args)
	{
		Vector3 localTorqueInc = ((args.Length == 0) ? Vector3.zero : ((Vector3)args[0]));
		float num = ((args.Length <= 1) ? 2f : ((float)args[1]));
		currentDpad = Blocksworld.UI.Controls.GetNormalizedDPadOffset("L");
		localTorqueInc *= eInfo.floatArg * num;
		IncreaseLocalTorque(localTorqueInc);
		return TileResultCode.True;
	}

	private void IncreaseLocalTorque(Vector3 localTorqueInc)
	{
		Vector3 vector = goT.TransformDirection(localTorqueInc);
		extraTorqueChunk += vector;
	}

	public TileResultCode IncreaseLocalVelocityChunk(ScriptRowExecutionInfo eInfo, object[] args)
	{
		Vector3 direction = ((args.Length == 0) ? Vector3.zero : ((Vector3)args[0]));
		float num = ((args.Length <= 1) ? 4f : ((float)args[1]));
		direction *= eInfo.floatArg * num;
		Vector3 vector = goT.TransformDirection(direction);
		targetVelocity += vector;
		targetVelocityApplications += eInfo.floatArg;
		return TileResultCode.True;
	}

	public TileResultCode DPadIncreaseTorqueChunk(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string key = ((args.Length == 0) ? "L" : ((string)args[0]));
		float num = ((args.Length <= 1) ? 3f : ((float)args[1]));
		Blocksworld.UI.Controls.EnableDPad(key, MoverDirectionMask.ALL);
		currentDpad = Blocksworld.UI.Controls.GetNormalizedDPadOffset(key);
		Vector3 worldDPadOffset = Blocksworld.UI.Controls.GetWorldDPadOffset(key);
		Vector3 vector = num * eInfo.floatArg * Vector3.Cross(Vector3.up, worldDPadOffset);
		extraTorqueChunk += vector;
		return TileResultCode.True;
	}

	public TileResultCode DPadIncreaseVelocityChunk(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string key = ((args.Length == 0) ? "L" : ((string)args[0]));
		float num = ((args.Length <= 1) ? 4f : ((float)args[1]));
		Blocksworld.UI.Controls.EnableDPad(key, MoverDirectionMask.ALL);
		currentDpad = Blocksworld.UI.Controls.GetNormalizedDPadOffset(key);
		Vector3 worldDPadOffset = Blocksworld.UI.Controls.GetWorldDPadOffset(key);
		Vector3 vector = num * worldDPadOffset * eInfo.floatArg;
		targetVelocity += vector;
		targetVelocityApplications += worldDPadOffset.magnitude;
		return TileResultCode.True;
	}

	public TileResultCode AlignAlongDPadChunk(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string key = ((args.Length == 0) ? "L" : ((string)args[0]));
		float floatArg = Util.GetFloatArg(args, 1, -1f);
		Blocksworld.UI.Controls.EnableDPad(key, MoverDirectionMask.ALL);
		currentDpad = Blocksworld.UI.Controls.GetNormalizedDPadOffset(key);
		Vector3 worldDPadOffset = Blocksworld.UI.Controls.GetWorldDPadOffset(key);
		turnAlongDirection += worldDPadOffset;
		if (floatArg > 0f)
		{
			if (turnAlongMaxAngVel <= 0f)
			{
				turnAlongMaxAngVel = floatArg;
			}
			else
			{
				turnAlongMaxAngVel += floatArg;
			}
			float num = Mathf.Max(0f, (turnAlongMaxAngVel - 5f) / 5f);
			if (num > 0f)
			{
				turnAlongDirection += worldDPadOffset * num;
			}
		}
		return TileResultCode.True;
	}

	protected override void HandleTiltMover(float xTilt, float yTilt, float zTilt)
	{
		Vector3 cameraUp = Blocksworld.cameraUp;
		Vector3 cameraForward = Blocksworld.cameraForward;
		Vector3 cameraRight = Blocksworld.cameraRight;
		Vector3 forward = goT.forward;
		Vector3 forward2 = goT.forward;
		Quaternion quaternion = Quaternion.AngleAxis(xTilt * 90f, Vector3.up);
		Quaternion quaternion2 = Quaternion.AngleAxis(yTilt * 90f, Blocksworld.cameraRight);
		Vector3 vector = quaternion * (quaternion2 * Blocksworld.cameraForward);
		turnAlongDirection += vector;
		bankAngle = (0f - zTilt) * 30f;
		alignInFieldChunkApplications = 1f;
	}

	public TileResultCode BankTurnChunk(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string key = ((args.Length == 0) ? "L" : ((string)args[0]));
		float floatArg = Util.GetFloatArg(args, 1, -1f);
		Blocksworld.UI.Controls.EnableDPad(key, MoverDirectionMask.ALL);
		currentDpad = Blocksworld.UI.Controls.GetNormalizedDPadOffset(key);
		Vector3 worldDPadOffset = Blocksworld.UI.Controls.GetWorldDPadOffset(key);
		Transform transform = goT;
		Vector3 vec = transform.TransformDirection(rotation * Vector3.forward);
		Vector3 normalized = Util.ProjectOntoPlane(vec, Vector3.up).normalized;
		Vector3 normalized2 = Util.ProjectOntoPlane(Vector3.RotateTowards(normalized, worldDPadOffset, 1f, float.MaxValue), Vector3.up).normalized;
		turnAlongDirection += normalized2 * inertiaTurn;
		if (floatArg > 0f)
		{
			if (turnAlongMaxAngVel <= 0f)
			{
				turnAlongMaxAngVel = floatArg;
			}
			else
			{
				turnAlongMaxAngVel += floatArg;
			}
			float num = Mathf.Max(0f, (turnAlongMaxAngVel - 5f) / 5f);
			if (num > 0f)
			{
				turnAlongDirection += normalized2 * num * inertiaTurn;
			}
		}
		float num2 = 0.5f * (1f - Vector3.Dot(worldDPadOffset, normalized));
		num2 *= Mathf.Sign(Vector3.Cross(worldDPadOffset, normalized).y);
		if (chunkRigidBody != null && worldDPadOffset.sqrMagnitude > 0f)
		{
			bankAngle = num2 * 80f;
			bankAngle = Mathf.Clamp(bankAngle, -50f, 50f);
		}
		else
		{
			bankAngle = 0f;
		}
		alignRotation = Quaternion.AngleAxis(bankAngle, normalized);
		alignInFieldChunkApplications += inertiaBank * 0.15f;
		return TileResultCode.True;
	}

	public TileResultCode FlightSimChunk(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string key = ((args.Length == 0) ? "L" : ((string)args[0]));
		float num = ((args.Length <= 1) ? 2f : ((float)args[1]));
		Blocksworld.UI.Controls.EnableDPad(key, MoverDirectionMask.ALL);
		currentDpad = Blocksworld.UI.Controls.GetNormalizedDPadOffset(key);
		if (currentDpad.magnitude > 0.01f)
		{
			float num2 = eInfo.floatArg * num;
			float num3 = currentDpad.x * num2;
			float num4 = currentDpad.y * num2;
			Vector3 localTorqueInc = new Vector3(0f - num4, num3, num3);
			IncreaseLocalTorque(localTorqueInc);
		}
		return TileResultCode.True;
	}

	public TileResultCode TiltFlightSimChunk(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (chunkRigidBody == null || !TiltManager.Instance.IsMonitoring())
		{
			return TileResultCode.True;
		}
		float floatArg = Util.GetFloatArg(args, 1, 2f);
		Vector3 relativeGravityVector = TiltManager.Instance.GetRelativeGravityVector();
		float num = floatArg * TiltManager.Instance.GetTiltTwist();
		float num2 = -2f * floatArg * relativeGravityVector.y;
		float num3 = 2f * floatArg * relativeGravityVector.x + num;
		tiltAlignBaseRotation *= Quaternion.AngleAxis(num3 * Time.fixedDeltaTime, Vector3.up);
		return AlignToTilt(eInfo, args);
	}

	private float GetInertia(Vector3 axis)
	{
		float num = 0f;
		List<Block> list = Block.connectedCache[this];
		for (int i = 0; i < list.Count; i++)
		{
			Block block = list[i];
			num += block.GetMomentOfInertia(block.goT.position, axis, includeMass: false);
		}
		return Mathf.Clamp(num, 1f, 10000f);
	}

	public TileResultCode IncreasePositionInGravityFieldChunk(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int num = ((args.Length == 0) ? 1 : ((int)args[0]));
		float num2 = ((args.Length <= 1) ? 1f : ((float)args[1]));
		float num3 = ((args.Length <= 2) ? 1f : ((float)args[2]));
		float num4 = ((args.Length <= 3) ? 1f : ((float)args[3]));
		ref Vector3 reference = ref positionInFieldChunkApplications;
		int index;
		positionInFieldChunkApplications[index = num] = reference[index] + num2;
		if (eInfo.timer < 0.001f)
		{
			reference = ref positionInFieldChunkOffsetTargets;
			int index2;
			positionInFieldChunkOffsetTargets[index2 = num] = reference[index2] + num3 * num4;
			positionInFieldChunkOffsetIncrements[num] = Mathf.Max(Mathf.Abs(num3), positionInFieldChunkOffsetIncrements[num]) * Blocksworld.fixedDeltaTime;
		}
		if (eInfo.timer >= num4)
		{
			positionInFieldChunkOffsetIncrements[num] = Blocksworld.fixedDeltaTime;
			return TileResultCode.True;
		}
		return TileResultCode.Delayed;
	}

	private void PositionInGravityFieldChunk(object[] args, float arg1, int index)
	{
		float num = ((args.Length == 0) ? 1f : ((float)args[0]));
		ref Vector3 reference = ref positionInFieldChunkApplications;
		positionInFieldChunkApplications[index] = reference[index] + arg1 * num;
		positionInFieldHover[index] = false;
	}

	public TileResultCode PositionInGravityFieldXChunk(ScriptRowExecutionInfo eInfo, object[] args)
	{
		PositionInGravityFieldChunk(args, eInfo.floatArg, 0);
		return TileResultCode.True;
	}

	public TileResultCode PositionInGravityFieldYChunk(ScriptRowExecutionInfo eInfo, object[] args)
	{
		PositionInGravityFieldChunk(args, eInfo.floatArg, 1);
		return TileResultCode.True;
	}

	public TileResultCode HoverInGravityFieldChunk(ScriptRowExecutionInfo eInfo, object[] args)
	{
		positionInFieldHover[1] = true;
		ref Vector3 reference = ref positionInFieldChunkApplications;
		positionInFieldChunkApplications[1] = reference[1] + 1.25f;
		hoverInFieldDistances[1] = Util.GetFloatArg(args, 1, 1f) * eInfo.floatArg;
		return TileResultCode.True;
	}

	public TileResultCode PositionInGravityFieldZChunk(ScriptRowExecutionInfo eInfo, object[] args)
	{
		PositionInGravityFieldChunk(args, eInfo.floatArg, 2);
		return TileResultCode.True;
	}

	public override bool TreatAsVehicleLikeBlock()
	{
		return TreatAsVehicleLikeBlockWithStatus(ref treatAsVehicleStatus);
	}
}
