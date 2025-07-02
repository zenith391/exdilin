using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

namespace Blocks;

public class CharacterStateHandler : StateHandlerBase
{
	protected class InternalStateInfo
	{
		public List<CharacterState> allowedTransitions = new List<CharacterState>();

		public CharacterBaseState stateFunctions;

		public int transitionAnim = -1;

		public float transitionBlend = 0.05f;

		public float animationRate = 1f;

		public HashSet<string> tags = new HashSet<string>();
	}

	public CharacterRole currentRole;

	public CharacterState currentState;

	protected InternalStateInfo currentStateInfo;

	public CharacterBaseState currentFunctions;

	protected int transitionAnim = -1;

	public float stateBlend = 0.05f;

	public UpperBodyStateHandler upperBody;

	public float stateTime;

	public float timeInAnim;

	public int sideAnim = -1;

	public int lastSideAnim = -1;

	public int lastHoverAnim = -1;

	public Vector3 lastForward;

	public Vector3 lastRight;

	public Vector3 lastUp;

	protected bool isPulling;

	public bool isTransitioning;

	protected BlockAbstractAntiGravityWing targetCape;

	protected BlockAbstractJetpack targetJetpack;

	protected Quaternion capeFlightRotation = Quaternion.identity;

	protected Quaternion capeHoverRotation = Quaternion.identity;

	public GameObject targetRig;

	public Rigidbody rb;

	private Vector3 lastPos = Vector3.zero;

	public Vector3 offset = Vector3.zero;

	protected float currentVelMag;

	public float turnPower;

	public Vector3 desiredGoto = Vector3.zero;

	protected Vector3 rootOffset = Vector3.zero;

	public Vector3 currentVelocity = Vector3.zero;

	public float speedForceModifier = 1f;

	public float desiredJumpForce = 1f;

	public float maxDownSpeedDuringJump;

	public float dodgeSpeed;

	public float standingAttackMaxSpeed;

	public float standingAttackMinSpeed;

	public Vector3 standingAttackForward;

	public float desiredAnimSpeed = 1f;

	protected float actualAnimSpeed = 1f;

	protected Queue<CharacterState> desiredStateQueue = new Queue<CharacterState>();

	public List<Block> attachments = new List<Block>();

	public bool desiresMove;

	private bool wasDesiringMove;

	public Vector3 requestedMoveVelocity;

	protected string playingAnim = string.Empty;

	public float blendStart;

	public int animationHash;

	public bool firstFrame;

	protected bool startPlay;

	public Quaternion startRotation;

	public float getUpAnim;

	public CharacterJumpState.JumpState currentJumpState;

	public bool hasDoubleJumped;

	public bool allowDouble;

	public int currentVelocityRange;

	public float currentSpeed;

	public CharacterWalkState.WalkDirection currentDirection = CharacterWalkState.WalkDirection.NumDirections;

	public bool deliberateWalk;

	public bool walkStrafe;

	public bool preventLook;

	public bool requestingPlayAnim;

	public string playAnimCurrent;

	public bool playAnimFinished;

	private Quaternion sideAnimRotation = Quaternion.identity;

	private CharacterState queuedHitReactState;

	private Dictionary<int, object> savedAnimatorParameterValues;

	protected static Dictionary<CharacterState, Dictionary<CharacterRole, InternalStateInfo>> stateMap = new Dictionary<CharacterState, Dictionary<CharacterRole, InternalStateInfo>>();

	public CharacterStateHandler()
	{
		combatController = new CombatController();
		combatController.AttachToStateHandler(this);
		upperBody = new UpperBodyStateHandler(this);
		upperBody.combatController = combatController;
	}

	public static void ClearStateMap()
	{
		stateMap.Clear();
	}

	public static void LoadStateMap(string jsonMap, CharacterRole defaultRole = CharacterRole.Male)
	{
		JObject jObject = JSONDecoder.Decode(jsonMap);
		Dictionary<string, JObject> objectValue = jObject.ObjectValue;
		foreach (JObject item in objectValue["states"].ArrayValue)
		{
			InternalStateInfo internalStateInfo = new InternalStateInfo();
			CharacterState characterState = (CharacterState)Enum.Parse(typeof(CharacterState), item["name"].StringValue);
			CharacterRole key = defaultRole;
			if (item.ContainsKey("role"))
			{
				key = (CharacterRole)Enum.Parse(typeof(CharacterRole), item["role"].StringValue);
			}
			List<string> list = ((!item.ContainsKey("animations")) ? null : new List<string>(item["animations"].StringValue.Split(',')));
			if (item.ContainsKey("tags"))
			{
				internalStateInfo.tags = new HashSet<string>(item["tags"].StringValue.ToLower().Split(','));
			}
			string text = "Anim";
			bool isLeftHanded = false;
			bool isRightHanded = false;
			bool isLeftFooted = false;
			bool isRightFooted = false;
			if (item.ContainsKey("feet"))
			{
				List<string> list2 = new List<string>(item["feet"].StringValue.Split(','));
				foreach (string item2 in list2)
				{
					if (item2.ToLower() == "left")
					{
						isLeftFooted = true;
					}
					if (item2.ToLower() == "right")
					{
						isRightFooted = true;
					}
				}
			}
			else if (item.ContainsKey("hands"))
			{
				List<string> list3 = new List<string>(item["hands"].StringValue.Split(','));
				foreach (string item3 in list3)
				{
					if (item3.ToLower() == "left")
					{
						isLeftHanded = true;
					}
					if (item3.ToLower() == "right")
					{
						isRightHanded = true;
					}
				}
			}
			else
			{
				isRightHanded = true;
			}
			if (item.ContainsKey("type"))
			{
				text = item["type"].StringValue;
			}
			if (characterState == CharacterState.Idle)
			{
				CharacterIdleState characterIdleState = new CharacterIdleState();
				characterIdleState.animations = list;
				characterIdleState.leftIdle = ((!item.ContainsKey("leftSide")) ? string.Empty : item["leftSide"].StringValue);
				characterIdleState.rightIdle = ((!item.ContainsKey("rightSide")) ? string.Empty : item["rightSide"].StringValue);
				characterIdleState.frontIdle = ((!item.ContainsKey("frontSide")) ? string.Empty : item["frontSide"].StringValue);
				characterIdleState.backIdle = ((!item.ContainsKey("backSide")) ? string.Empty : item["backSide"].StringValue);
				characterIdleState.topIdle = ((!item.ContainsKey("topSide")) ? string.Empty : item["topSide"].StringValue);
				characterIdleState.sideOffsets = new List<Vector3>();
				if (item.ContainsKey("sideOffsets"))
				{
					List<JObject> arrayValue = item["sideOffsets"].ArrayValue;
					for (int i = 0; i < arrayValue.Count; i++)
					{
						List<string> list4 = new List<string>(arrayValue[i].StringValue.Split(','));
						characterIdleState.sideOffsets.Add(new Vector3(float.Parse(list4[0]), float.Parse(list4[1]), float.Parse(list4[2])));
					}
				}
				while (characterIdleState.sideOffsets.Count < 5)
				{
					characterIdleState.sideOffsets.Add(Vector3.zero);
				}
				internalStateInfo.stateFunctions = characterIdleState;
			}
			else
			{
				switch (text)
				{
				case "Melee_Attack":
				{
					MeleeAttackState meleeAttackState = new MeleeAttackState();
					CharacterAttackState characterAttackState = new CharacterAttackState(meleeAttackState);
					meleeAttackState.animationState = list[0];
					if (list.Count > 1)
					{
						meleeAttackState.recoilAnimation = list[1];
					}
					meleeAttackState.isLeftHanded = isLeftHanded;
					meleeAttackState.isRightHanded = isRightHanded;
					meleeAttackState.isLeftFooted = isLeftFooted;
					meleeAttackState.isRightFooted = isRightFooted;
					characterAttackState.maxForwardSpeed = ((!item.ContainsKey("max_forward_speed")) ? 0f : item["max_forward_speed"].FloatValue);
					characterAttackState.minForwardSpeed = ((!item.ContainsKey("min_forward_speed")) ? 0f : item["min_forward_speed"].FloatValue);
					characterAttackState.moveEndNormalizedTime = ((!item.ContainsKey("move_end_normalized")) ? 0f : item["move_end_normalized"].FloatValue);
					meleeAttackState.damageStartNormalizedTime = ((!item.ContainsKey("damage_start_normalized")) ? 0f : item["damage_start_normalized"].FloatValue);
					meleeAttackState.damageEndNormalizedTime = ((!item.ContainsKey("damage_end_normalized")) ? 1f : item["damage_end_normalized"].FloatValue);
					meleeAttackState.interruptNormalizedTime = ((!item.ContainsKey("interrupt_normalized")) ? 0.8f : item["interrupt_normalized"].FloatValue);
					internalStateInfo.stateFunctions = characterAttackState;
					break;
				}
				case "Dodge":
					internalStateInfo.stateFunctions = new CharacterDodgeState
					{
						animation = list[0],
						speed = ((!item.ContainsKey("speed")) ? 5f : item["speed"].FloatValue),
						dodgeStartNormalizedTime = ((!item.ContainsKey("move_start_normalized")) ? 0f : item["move_start_normalized"].FloatValue),
						dodgeEndNormalizedTime = ((!item.ContainsKey("move_end_normalized")) ? 1f : item["move_end_normalized"].FloatValue)
					};
					break;
				case "GetUp":
				{
					CharacterGetUpState characterGetUpState = new CharacterGetUpState();
					characterGetUpState.timing = new List<float>();
					characterGetUpState.animations = list;
					if (item.ContainsKey("time"))
					{
						List<JObject> arrayValue4 = item["time"].ArrayValue;
						foreach (JObject item4 in arrayValue4)
						{
							characterGetUpState.timing.Add(item4.FloatValue);
						}
					}
					while (characterGetUpState.timing.Count < characterGetUpState.animations.Count)
					{
						characterGetUpState.timing.Add(0.5f);
					}
					internalStateInfo.stateFunctions = characterGetUpState;
					break;
				}
				case "Chain":
					internalStateInfo.stateFunctions = new CharacterChainAnimState
					{
						animationHash = Animator.StringToHash((list.Count <= 0) ? string.Empty : list[0])
					};
					break;
				case "Walk":
				{
					CharacterWalkState characterWalkState = new CharacterWalkState();
					if (list != null && list.Count > 0)
					{
						characterWalkState.AddDirection(item);
					}
					else if (item.ContainsKey("directions"))
					{
						List<JObject> arrayValue2 = item["directions"].ArrayValue;
						foreach (JObject item5 in arrayValue2)
						{
							characterWalkState.AddDirection(item5);
						}
					}
					if (item.ContainsKey("baseRunVel"))
					{
						characterWalkState.baseRunVel = item["baseRunVel"].FloatValue;
					}
					internalStateInfo.stateFunctions = characterWalkState;
					break;
				}
				case "Jump":
				{
					CharacterJumpState characterJumpState = new CharacterJumpState();
					characterJumpState.animations = list;
					if (item.ContainsKey("blends"))
					{
						characterJumpState.blendSpeeds = new List<float>();
						List<JObject> arrayValue3 = item["blends"].ArrayValue;
						for (int j = 0; j < arrayValue3.Count; j++)
						{
							characterJumpState.blendSpeeds.Add(arrayValue3[j].FloatValue);
						}
					}
					internalStateInfo.stateFunctions = characterJumpState;
					break;
				}
				case "Hover":
					internalStateInfo.stateFunctions = new CharacterHoverState
					{
						animations = list
					};
					break;
				case "PlayAnim":
				{
					CharacterPlayAnimState stateFunctions = new CharacterPlayAnimState();
					internalStateInfo.stateFunctions = stateFunctions;
					break;
				}
				default:
					internalStateInfo.stateFunctions = new CharacterSingleAnimState
					{
						animationHash = Animator.StringToHash((list.Count <= 0) ? string.Empty : list[0]),
						animations = list
					};
					break;
				}
			}
			if (internalStateInfo.stateFunctions != null && item.ContainsKey("desired"))
			{
				internalStateInfo.stateFunctions.desiredState = (CharacterState)Enum.Parse(typeof(CharacterState), item["desired"].StringValue);
			}
			else
			{
				internalStateInfo.stateFunctions.desiredState = CharacterState.Idle;
			}
			if (item.ContainsKey("transition"))
			{
				internalStateInfo.transitionAnim = Animator.StringToHash(item["transition"].StringValue);
			}
			else
			{
				internalStateInfo.transitionAnim = -1;
			}
			if (item.ContainsKey("blend"))
			{
				internalStateInfo.transitionBlend = item["blend"].FloatValue;
			}
			else
			{
				internalStateInfo.transitionBlend = 0.025f;
			}
			if (item.ContainsKey("offsets"))
			{
				List<JObject> arrayValue5 = item["offsets"].ArrayValue;
				if (arrayValue5.Count > 0)
				{
					internalStateInfo.stateFunctions.rootOffsets = new List<Vector3>();
				}
				for (int k = 0; k < arrayValue5.Count; k++)
				{
					List<string> list5 = new List<string>(arrayValue5[k].StringValue.Split(','));
					internalStateInfo.stateFunctions.rootOffsets.Add(new Vector3(float.Parse(list5[0]), float.Parse(list5[1]), float.Parse(list5[2])));
				}
			}
			else if (item.ContainsKey("offset"))
			{
				internalStateInfo.stateFunctions.rootOffsets = new List<Vector3>();
				List<string> list6 = new List<string>(item["offset"].StringValue.Split(','));
				internalStateInfo.stateFunctions.rootOffsets.Add(new Vector3(float.Parse(list6[0]), float.Parse(list6[1]), float.Parse(list6[2])));
			}
			if (item.ContainsKey("animationRate"))
			{
				float floatValue = item["animationRate"].FloatValue;
				if (floatValue > 0f)
				{
					internalStateInfo.animationRate = floatValue;
				}
			}
			if (characterState != CharacterState.Flail)
			{
				internalStateInfo.allowedTransitions.Add(CharacterState.Flail);
			}
			internalStateInfo.allowedTransitions.Add(internalStateInfo.stateFunctions.desiredState);
			if (item.ContainsKey("allowed"))
			{
				List<string> list7 = new List<string>(((!item.ContainsKey("allowed")) ? string.Empty : item["allowed"].StringValue).Split(','));
				foreach (string item6 in list7)
				{
					if (item6 == "UprightDefault")
					{
						internalStateInfo.allowedTransitions.Add(CharacterState.Idle);
						internalStateInfo.allowedTransitions.Add(CharacterState.Balance);
						internalStateInfo.allowedTransitions.Add(CharacterState.CrawlEnter);
						internalStateInfo.allowedTransitions.Add(CharacterState.Walk);
						internalStateInfo.allowedTransitions.Add(CharacterState.Jump);
						internalStateInfo.allowedTransitions.Add(CharacterState.SwimIn);
						internalStateInfo.allowedTransitions.Add(CharacterState.SitDown);
						internalStateInfo.allowedTransitions.Add(CharacterState.Collapse);
						internalStateInfo.allowedTransitions.Add(CharacterState.ImpactRight);
						internalStateInfo.allowedTransitions.Add(CharacterState.ImpactLeft);
						internalStateInfo.allowedTransitions.Add(CharacterState.SoftHitTop);
						internalStateInfo.allowedTransitions.Add(CharacterState.SoftHitBack);
						internalStateInfo.allowedTransitions.Add(CharacterState.SoftHitFront);
						internalStateInfo.allowedTransitions.Add(CharacterState.SoftHitRight);
						internalStateInfo.allowedTransitions.Add(CharacterState.SoftHitLeft);
						internalStateInfo.allowedTransitions.Add(CharacterState.GetUp);
						internalStateInfo.allowedTransitions.Add(CharacterState.Hover);
						internalStateInfo.allowedTransitions.Add(CharacterState.SwordLungeRight);
						internalStateInfo.allowedTransitions.Add(CharacterState.SwordLungeLeft);
						internalStateInfo.allowedTransitions.Add(CharacterState.SwordJumpAttack);
						internalStateInfo.allowedTransitions.Add(CharacterState.KickFrontLeft);
						internalStateInfo.allowedTransitions.Add(CharacterState.KickFrontRight);
						internalStateInfo.allowedTransitions.Add(CharacterState.DodgeLeft);
						internalStateInfo.allowedTransitions.Add(CharacterState.DodgeRight);
					}
					else if (item6 == "SwimDefault")
					{
						internalStateInfo.allowedTransitions.Add(CharacterState.Idle);
						internalStateInfo.allowedTransitions.Add(CharacterState.SwimIdle);
						internalStateInfo.allowedTransitions.Add(CharacterState.Swim);
						internalStateInfo.allowedTransitions.Add(CharacterState.Hover);
						internalStateInfo.allowedTransitions.Add(CharacterState.SwimOut);
					}
					else
					{
						internalStateInfo.allowedTransitions.Add((CharacterState)Enum.Parse(typeof(CharacterState), item6));
					}
				}
			}
			if (!stateMap.ContainsKey(characterState))
			{
				stateMap[characterState] = new Dictionary<CharacterRole, InternalStateInfo>();
			}
			stateMap[characterState][key] = internalStateInfo;
		}
	}

	public override bool IsPlayingAnimation(string anim)
	{
		return targetController.GetCurrentAnimatorStateInfo(0).IsName(anim);
	}

	public override int PlayAnimation(string animation, bool interrupt = false)
	{
		PlayAnimation(Animator.StringToHash(animation), interrupt);
		playingAnim = animation;
		return animationHash;
	}

	public override int GetAnimatorLayer()
	{
		return 0;
	}

	public override float TimeInCurrentState()
	{
		return stateTime;
	}

	public float StateNormalizedTime()
	{
		return targetController.GetCurrentAnimatorStateInfo(0).normalizedTime;
	}

	protected InternalStateInfo GetStateInfo(CharacterState state = CharacterState.None, CharacterRole role = CharacterRole.None)
	{
		if (state == CharacterState.None)
		{
			state = currentState;
		}
		if (role == CharacterRole.None)
		{
			role = currentRole;
		}
		if (!stateMap.ContainsKey(state))
		{
			return null;
		}
		Dictionary<CharacterRole, InternalStateInfo> dictionary = stateMap[state];
		if (dictionary.ContainsKey(role))
		{
			return dictionary[role];
		}
		if (role == CharacterRole.MiniFemale && dictionary.ContainsKey(CharacterRole.Female))
		{
			return dictionary[CharacterRole.Female];
		}
		if (dictionary.ContainsKey(CharacterRole.Male))
		{
			return dictionary[CharacterRole.Male];
		}
		return null;
	}

	public void SetTarget(BlockAnimatedCharacter targetBlock, GameObject targetGO = null)
	{
		targetObject = targetBlock;
		if (null == targetGO)
		{
			targetGO = targetObject.go;
		}
		targetRig = targetGO;
		if (null != targetRig)
		{
			targetController = targetRig.GetComponent<Animator>();
		}
		if (!(null == targetController))
		{
			upperBody.targetController = targetController;
			upperBody.targetObject = targetObject;
			targetRig.transform.parent = targetObject.go.transform;
			targetRig.transform.localRotation = Quaternion.identity;
		}
	}

	public void Play()
	{
		offset = Vector3.zero;
		currentVelMag = 0f;
		turnPower = 0f;
		desiredGoto = Vector3.zero;
		rootOffset = Vector3.zero;
		currentVelocity = Vector3.zero;
		speedForceModifier = 1f;
		desiredJumpForce = 1f;
		desiredAnimSpeed = 1f;
		actualAnimSpeed = 1f;
		desiresMove = false;
		wasDesiringMove = false;
		playingAnim = string.Empty;
		blendStart = 0f;
		animationHash = -1;
		firstFrame = false;
		startPlay = true;
		hasDoubleJumped = false;
		currentVelocityRange = 0;
		currentSpeed = 0f;
		currentDirection = CharacterWalkState.WalkDirection.NumDirections;
		deliberateWalk = false;
		walkStrafe = false;
		currentState = CharacterState.None;
		currentFunctions = null;
		isPulling = false;
		rb = targetObject.body.GetComponent<Rigidbody>();
		targetCape = null;
		targetJetpack = null;
		desiredStateQueue.Clear();
		currentState = CharacterState.None;
		currentFunctions = null;
		ForceSitStill();
		AddCharacterAttachments();
		combatController.Play();
		upperBody.Play();
		SetSpeedForce();
		if (targetObject.unmoving)
		{
			bool flag = targetObject.attachmentsPreventLookAnim || targetObject.isConnectedToUnsupportedActionBlock;
			EnterState((!flag) ? CharacterState.Sitting : CharacterState.SittingStill);
			return;
		}
		if (targetJetpack != null)
		{
			EnterState(CharacterState.Hover);
			return;
		}
		EnterState(CharacterState.Idle);
		if (IsOnSide())
		{
			InterruptState(CharacterState.GetUp);
		}
	}

	public void Appearing()
	{
		if (targetObject.unmoving)
		{
			ForceSit();
		}
	}

	public void IgnoreRaycasts(bool ignore)
	{
		Layer layer = (ignore ? Layer.MeshEmitters : Layer.Default);
		for (int i = 0; i < attachments.Count; i++)
		{
			attachments[i].go.SetLayer(layer, recursive: true);
		}
		combatController.IgnoreRaycasts(ignore, layer);
	}

	public void AddCharacterAttachments()
	{
		combatController.rightHandAttachmentParent = targetObject.GetHandAttach(0).transform;
		combatController.leftHandAttachmentParent = targetObject.GetHandAttach(1).transform;
		combatController.rightFootAttachmentParent = targetObject.GetRightFootBoneTransform();
		combatController.leftFootAttachmentParent = targetObject.GetLeftFootBoneTransform();
		if (targetObject.unmoving)
		{
			for (int i = 0; i < targetObject.attachedHeadBlocks.Count; i++)
			{
				Block block = targetObject.attachedHeadBlocks[i];
				if (CharacterEditor.IsGear(block))
				{
					attachments.Add(block);
					block.goT.SetParent(targetObject.GetHeadAttach().transform);
				}
			}
			return;
		}
		Block attachedRightBlock = targetObject.attachedRightBlock;
		Block attachedLeftBlock = targetObject.attachedLeftBlock;
		if (attachedRightBlock != null)
		{
			foreach (Block attachedRightHandBlock in targetObject.attachedRightHandBlocks)
			{
				attachedRightHandBlock.goT.SetParent(attachedRightBlock.goT);
			}
			bool applyOffset = attachedRightBlock.BlockType() != "SC2 Mechanical Hand" && attachedRightBlock.BlockType() != "BBG Claws Right";
			combatController.AddRightHandAttachment(attachedRightBlock, applyOffset);
		}
		else
		{
			combatController.RemoveRightHandAttachment();
		}
		if (attachedLeftBlock != null)
		{
			foreach (Block attachedLeftHandBlock in targetObject.attachedLeftHandBlocks)
			{
				attachedLeftHandBlock.goT.SetParent(attachedLeftBlock.goT);
			}
			bool applyOffset2 = attachedLeftBlock.BlockType() != "SC2 Mechanical Hand" && attachedLeftBlock.BlockType() != "BBG Claws Right";
			combatController.AddLeftHandAttachment(attachedLeftBlock, applyOffset2);
		}
		else
		{
			combatController.RemoveLeftHandAttachment();
		}
		for (int j = 0; j < targetObject.attachedBackBlocks.Count; j++)
		{
			Block block2 = targetObject.attachedBackBlocks[j];
			attachments.Add(block2);
			block2.goT.SetParent(targetObject.GetBodyAttach().transform);
			if (targetCape == null)
			{
				targetCape = block2 as BlockAbstractAntiGravityWing;
				if (targetCape != null)
				{
					SetRole(CharacterRole.Caped);
					capeFlightRotation = targetCape.rotation;
					targetCape.rotation = Quaternion.Euler(0f, targetCape.rotation.y, targetCape.rotation.z);
					capeHoverRotation = targetCape.rotation;
				}
			}
			if (targetJetpack == null)
			{
				targetJetpack = block2 as BlockAbstractJetpack;
			}
			if (block2.GetBlockMetaData() != null)
			{
				block2.goT.localPosition += block2.GetBlockMetaData().attachOffset;
			}
		}
		for (int k = 0; k < targetObject.attachedHeadBlocks.Count; k++)
		{
			Block block3 = targetObject.attachedHeadBlocks[k];
			attachments.Add(block3);
			block3.goT.SetParent(targetObject.GetHeadAttach().transform);
		}
	}

	public void Stop()
	{
		combatController.Stop();
		upperBody.Stop();
		Unfreeze();
		desiredStateQueue.Clear();
		currentState = CharacterState.None;
		currentFunctions = null;
		desiredAnimSpeed = 1f;
		actualAnimSpeed = 1f;
		blendStart = 0f;
		offset = Vector3.zero;
		desiredGoto = Vector3.zero;
		isPulling = false;
		ForceSit();
	}

	public void ForceSit()
	{
		rootOffset = Vector3.zero;
		targetRig.transform.localPosition = rootOffset - 1.2f * Vector3.up;
		ForceAnimState(CharacterState.EditSitting);
	}

	public void ForceSitStill()
	{
		rootOffset = Vector3.zero;
		targetRig.transform.localPosition = rootOffset - 1.2f * Vector3.up;
		ForceAnimState(CharacterState.EditSittingStill);
	}

	public void ForceEditPose()
	{
		targetRig.transform.localPosition = rootOffset + new Vector3(0f, -0.65f, -0.2f);
		ForceAnimState(CharacterState.EditModePose);
	}

	private void ForceAnimState(CharacterState state)
	{
		stateBlend = 0f;
		desiredAnimSpeed = 1f;
		actualAnimSpeed = 1f;
		blendStart = -1f;
		currentState = state;
		currentStateInfo = GetStateInfo();
		currentFunctions = currentStateInfo.stateFunctions;
		rootOffset = currentStateInfo.stateFunctions.GetOffset(this);
		CharacterSingleAnimState characterSingleAnimState = currentStateInfo.stateFunctions as CharacterSingleAnimState;
		characterSingleAnimState.Enter(this, interrupt: true);
		targetController.Play(animationHash, -1, 0f);
		targetController.Update(0.04f);
	}

	protected void DetachCharacterAttachments()
	{
		combatController.UnparentHandAttachments();
		for (int i = 0; i < attachments.Count; i++)
		{
			Collider component = attachments[i].go.GetComponent<Collider>();
			if (component != null)
			{
				component.enabled = true;
			}
			attachments[i].goT.SetParent(null);
		}
		IgnoreRaycasts(ignore: false);
		attachments.Clear();
	}

	public CharacterState GetState()
	{
		return currentState;
	}

	private bool CanChangeTo(CharacterState desiredState, CharacterState fromState = CharacterState.None)
	{
		if (fromState == CharacterState.None)
		{
			fromState = currentState;
		}
		if (fromState == CharacterState.None)
		{
			return true;
		}
		if (desiredState == CharacterState.PlayAnim && (fromState == CharacterState.Idle || fromState == CharacterState.PlayAnim) && currentVelMag <= 0.05f && rootOffset.sqrMagnitude <= 0.05f)
		{
			return sideAnim == -1;
		}
		InternalStateInfo internalStateInfo = ((fromState != currentState) ? GetStateInfo(fromState) : currentStateInfo);
		if (internalStateInfo == null)
		{
			BWLog.Info("Unable to get state info for " + fromState);
			return false;
		}
		if (fromState == CharacterState.Idle && sideAnim != -1)
		{
			return desiredState == CharacterState.GetUp;
		}
		return internalStateInfo.allowedTransitions.Contains(desiredState);
	}

	private bool CurrentStateHasTag(string tag)
	{
		if (currentStateInfo != null)
		{
			return currentStateInfo.tags.Contains(tag.ToLower());
		}
		return false;
	}

	public bool IsChangingState()
	{
		CharacterState characterState = currentState;
		if (characterState != CharacterState.CrawlEnter)
		{
			return characterState == CharacterState.CrawlExit;
		}
		return true;
	}

	public bool IsSwimming()
	{
		if (currentState != CharacterState.SwimIn && currentState != CharacterState.SwimIdle && currentState != CharacterState.Swim)
		{
			return currentState == CharacterState.SwimOut;
		}
		return true;
	}

	public bool IsSitting()
	{
		if (currentState != CharacterState.SitDown)
		{
			return currentState == CharacterState.Sitting;
		}
		return true;
	}

	public bool IsImmobile()
	{
		return CurrentStateHasTag("Immobile");
	}

	public bool InAttack()
	{
		if (!InStandingAttack())
		{
			return upperBody.InAttackState();
		}
		return true;
	}

	public bool InStandingAttack()
	{
		if (currentState != CharacterState.SwordLungeRight && currentState != CharacterState.SwordLungeLeft && currentState != CharacterState.SwordJumpAttack && currentState != CharacterState.KickFrontLeft)
		{
			return currentState == CharacterState.KickFrontRight;
		}
		return true;
	}

	public bool IsCrawling()
	{
		if (currentState != CharacterState.CrawlIdle)
		{
			return currentState == CharacterState.Crawl;
		}
		return true;
	}

	public bool IsDodging()
	{
		if (currentState != CharacterState.DodgeLeft)
		{
			return currentState == CharacterState.DodgeRight;
		}
		return true;
	}

	public bool IsWalking()
	{
		return currentState == CharacterState.Walk;
	}

	public bool IsProne()
	{
		return currentState == CharacterState.Prone;
	}

	public void StartJump(float jumpForce)
	{
		desiredJumpForce = jumpForce;
		InterruptState(CharacterState.Jump);
	}

	public void StartFalling()
	{
		desiredJumpForce = -1f;
		InterruptState(CharacterState.Jump);
	}

	public bool CanStartNewStandingAttack()
	{
		if (targetObject.IsOnGround())
		{
			return CanStartNewAttack();
		}
		return false;
	}

	public bool CanStartNewAttack()
	{
		if (sideAnim != -1 || IsImmobile() || IsImpactState())
		{
			return false;
		}
		if (!upperBody.CanStartNewAttack())
		{
			return false;
		}
		if (!InStandingAttack())
		{
			return !IsDodging();
		}
		MeleeAttackState attackState = ((CharacterAttackState)currentFunctions).attackState;
		if (attackState == null || stateTime < 0.15f)
		{
			return false;
		}
		float normalizedTime = targetController.GetCurrentAnimatorStateInfo(GetAnimatorLayer()).normalizedTime;
		if (IsPlayingAnimation(attackState.recoilAnimation))
		{
			return normalizedTime >= 1f;
		}
		return normalizedTime > attackState.interruptNormalizedTime;
	}

	public bool CanDoubleJump()
	{
		if (currentState != CharacterState.Jump || stateTime < 0.25f)
		{
			return false;
		}
		CharacterJumpState characterJumpState = (CharacterJumpState)currentFunctions;
		return allowDouble = characterJumpState.CanAddJumpForce(this) && !hasDoubleJumped;
	}

	public void HasDoubleJumped()
	{
		if (currentState == CharacterState.Jump)
		{
			CharacterJumpState characterJumpState = (CharacterJumpState)currentFunctions;
			characterJumpState.DoubleJump(this);
		}
	}

	public bool CanJump()
	{
		if (CanChangeTo(CharacterState.Jump) && targetObject != null)
		{
			return !targetObject.isHovering;
		}
		return false;
	}

	public bool IsJumping()
	{
		return currentState == CharacterState.Jump;
	}

	public bool DoubleJump(float power)
	{
		if (currentState != CharacterState.Jump)
		{
			return false;
		}
		CharacterJumpState characterJumpState = (CharacterJumpState)currentFunctions;
		if (hasDoubleJumped || !allowDouble)
		{
			return false;
		}
		characterJumpState.DoubleJump(this);
		return true;
	}

	public bool IsHover()
	{
		return currentState == CharacterState.Hover;
	}

	public bool IsImpactState()
	{
		if (currentState != CharacterState.SoftHitLeft && currentState != CharacterState.SoftHitRight && currentState != CharacterState.SoftHitBack && currentState != CharacterState.SoftHitFront && currentState != CharacterState.SoftHitTop && currentState != CharacterState.ImpactLeft)
		{
			return currentState == CharacterState.ImpactRight;
		}
		return true;
	}

	public void SetSideAnimRotation(Quaternion rot)
	{
		sideAnimRotation = rot;
	}

	public Quaternion GetSideAnimRotation()
	{
		return sideAnimRotation;
	}

	public bool IsOnSide()
	{
		return sideAnim != -1;
	}

	public bool IsGetUpState()
	{
		return currentState == CharacterState.GetUp;
	}

	public bool ApplyDiveForce(float force)
	{
		if (!IsSwimming())
		{
			return false;
		}
		targetObject.walkController.legsRb.AddForceAtPosition(-80f * force * Vector3.up, targetObject.walkController.legsRb.worldCenterOfMass);
		return true;
	}

	public void SetSpeedForce(float force = 1f)
	{
		speedForceModifier = force;
	}

	public float GetSpeedForceModifier()
	{
		if (currentState == CharacterState.GetUp || currentState == CharacterState.Balance)
		{
			return 0.1f;
		}
		if (currentState == CharacterState.SoftHitLeft || currentState == CharacterState.SoftHitRight || currentState == CharacterState.SoftHitFront || currentState == CharacterState.SoftHitBack)
		{
			return 0.1f;
		}
		if (currentState == CharacterState.Walk)
		{
			CharacterWalkState characterWalkState = (CharacterWalkState)currentFunctions;
			if (deliberateWalk && desiredGoto.magnitude < 0.001f)
			{
				return 0.5f * speedForceModifier;
			}
			if (walkStrafe || desiredGoto.z < 0f)
			{
				return 1f * speedForceModifier;
			}
			return speedForceModifier * Mathf.Max(0.05f, Vector3.Dot(desiredGoto.normalized, Vector3.forward));
		}
		if (currentState == CharacterState.Jump)
		{
			_ = currentJumpState;
			_ = 4;
			return 1f;
		}
		return speedForceModifier;
	}

	public bool QueueState(CharacterState desiredState)
	{
		if (currentState == desiredState)
		{
			return true;
		}
		if (desiredStateQueue.Contains(desiredState))
		{
			return true;
		}
		if (!CanChangeTo(desiredState))
		{
			BWLog.Info(string.Concat("Unable to shift from ", currentState, " to ", desiredState));
			return false;
		}
		desiredStateQueue.Enqueue(desiredState);
		if (desiredStateQueue.Count > 3)
		{
			BWLog.Info("Somehow queued " + desiredStateQueue.Count + " items for " + targetObject.go.name);
		}
		return true;
	}

	public void ClearQueue()
	{
		desiredStateQueue.Clear();
	}

	public bool InterruptQueue(CharacterState desiredState)
	{
		if (!CanChangeTo(desiredState))
		{
			return false;
		}
		ClearQueue();
		desiredStateQueue.Enqueue(desiredState);
		return true;
	}

	public bool InterruptState(CharacterState desiredState, bool noBlend = true)
	{
		if (!CanChangeTo(desiredState))
		{
			return false;
		}
		ClearQueue();
		EnterState(desiredState, noBlend);
		return true;
	}

	public float CurrentAnimationNormalizedTime()
	{
		return targetController.GetCurrentAnimatorStateInfo(0).normalizedTime;
	}

	public void PlayAnim(string anim)
	{
		if (currentState == CharacterState.PlayAnim)
		{
			playAnimCurrent = anim;
			animationHash = PlayAnimation(playAnimCurrent);
			stateTime = 0f;
			playAnimFinished = false;
		}
		else
		{
			playAnimCurrent = anim;
			InterruptState(CharacterState.PlayAnim);
		}
	}

	public void ClearAnimation()
	{
		playAnimCurrent = null;
		animationHash = -1;
	}

	public void DebugPlayAnim(string anim)
	{
		if (currentState != CharacterState.PlayAnim)
		{
			List<string> list = new List<string>(anim.Split('\n'));
			playAnimCurrent = list[0];
			InterruptState(CharacterState.PlayAnim);
		}
	}

	private void SetAnimatorParametersForRole(CharacterRole role)
	{
		string[] array = new string[4] { "DefaultRole", "SkeletonRole", "SneakyRole", "PirateRole" };
		string empty = string.Empty;
		empty = role switch
		{
			CharacterRole.Skeleton => "SkeletonRole", 
			CharacterRole.Pirate => "PirateRole", 
			CharacterRole.Sneaky => "SneakyRole", 
			_ => "DefaultRole", 
		};
		string[] array2 = array;
		foreach (string text in array2)
		{
			targetController.SetFloat(text, (!(text == empty)) ? 0f : 1f);
		}
	}

	public void SetAnimatorParametersForBodyParts(HashSet<string> bodyPartStrs)
	{
		string[] array = new string[3] { "LegOverride_Default", "LegOverride_PegLegRight", "LegOverride_PegLegLeft" };
		string text = "LegOverride_Default";
		if (bodyPartStrs.Contains("Limb Peg Leg Left"))
		{
			text = "LegOverride_PegLegLeft";
		}
		if (bodyPartStrs.Contains("Limb Peg Leg Right"))
		{
			text = "LegOverride_PegLegRight";
		}
		string[] array2 = array;
		foreach (string text2 in array2)
		{
			targetController.SetFloat(text2, (!(text2 == text)) ? 0f : 1f);
		}
	}

	public void SetRole(CharacterRole role)
	{
		if (role == currentRole)
		{
			return;
		}
		SetAnimatorParametersForRole(role);
		currentRole = role;
		upperBody.SetRole(role);
		if (currentState == CharacterState.None)
		{
			currentFunctions = null;
			return;
		}
		currentStateInfo = GetStateInfo();
		if (currentStateInfo == null)
		{
			BWLog.Error(string.Concat("Unable to find state info for ", currentState, " in role ", currentRole));
		}
		else
		{
			currentFunctions = currentStateInfo.stateFunctions;
		}
	}

	public void StandingAttack(CharacterState attackState)
	{
		if (CanStartNewStandingAttack())
		{
			EnterState(attackState);
		}
	}

	public void Attack(UpperBodyState attackState)
	{
		upperBody.Attack(attackState);
	}

	public void Shield(UpperBodyState shieldState)
	{
		upperBody.Shield(shieldState);
	}

	public void QueueHitReact(Vector3 hitDirection)
	{
		if (!targetObject.broken)
		{
			Vector3 rhs = Vector3.Cross(hitDirection.normalized, targetObject.goT.up);
			float num = Vector3.Dot(targetObject.goT.forward, hitDirection.normalized);
			float num2 = Vector3.Dot(targetObject.goT.right, rhs);
			if (Mathf.Abs(num) > 0.8f * Mathf.Abs(num2))
			{
				queuedHitReactState = ((num >= 0f) ? CharacterState.SoftHitBack : CharacterState.SoftHitFront);
			}
			else
			{
				queuedHitReactState = ((num2 >= 0f) ? CharacterState.SoftHitRight : CharacterState.SoftHitLeft);
			}
		}
	}

	public void PlayQueuedHitReact()
	{
		if (queuedHitReactState != CharacterState.None)
		{
			InterruptState(queuedHitReactState);
			queuedHitReactState = CharacterState.None;
		}
	}

	private void EnterState(CharacterState desiredState, bool interrupt = false)
	{
		CharacterState characterState = currentState;
		bool flag = IsSwimming();
		bool flag2 = currentState == CharacterState.PlayAnim;
		if (currentFunctions != null)
		{
			ExitState();
		}
		desiredAnimSpeed = 1f;
		transitionAnim = -1;
		stateTime = 0f;
		blendStart = 0f;
		sideAnim = -1;
		isTransitioning = true;
		currentState = desiredState;
		currentStateInfo = GetStateInfo();
		upperBody.PlayOnFullBody(currentState == CharacterState.Idle);
		if (currentStateInfo == null)
		{
			BWLog.Error(string.Concat("Unable to find state info for ", currentState, " in role ", currentRole));
			return;
		}
		currentFunctions = currentStateInfo.stateFunctions;
		stateBlend = currentStateInfo.transitionBlend;
		if (characterState == CharacterState.Balance)
		{
			stateBlend = ((desiredState != CharacterState.Idle || targetObject.goT.up.y < 0.85f) ? 0.08f : 1f);
			interrupt = false;
		}
		if (desiredState == CharacterState.Walk && characterState == CharacterState.Jump)
		{
			interrupt = true;
		}
		if (characterState == CharacterState.Walk && desiredState == CharacterState.Jump)
		{
			interrupt = false;
		}
		if (flag && (desiredState == CharacterState.Idle || desiredState == CharacterState.Walk))
		{
			interrupt = false;
		}
		if (desiredState == CharacterState.Idle && characterState == CharacterState.Hover)
		{
			interrupt = true;
		}
		if (desiredState == CharacterState.SwimIdle)
		{
			stateBlend = ((characterState != CharacterState.Swim) ? 0.025f : 0.1f);
		}
		if (flag2 && desiredState == CharacterState.Idle)
		{
			stateBlend = 0.2f;
		}
		if (desiredState == CharacterState.Collapse)
		{
			Collider component = targetObject.go.GetComponent<Collider>();
			if (component != null)
			{
				component.enabled = false;
			}
			targetObject.Freeze(informModelBlocks: true);
		}
		SetSpeedForce();
		if (targetObject.unmoving && currentState == CharacterState.Sitting)
		{
			PlayAnimation("SitIdle", interrupt);
		}
		queuedHitReactState = CharacterState.None;
		if (currentState == CharacterState.Unmoving || currentState == CharacterState.Treasure)
		{
			PlayAnimation("SitIdle", interrupt);
			return;
		}
		if (currentFunctions == null)
		{
			BWLog.Warning("Entering state " + currentState.ToString() + ", which has no class to support it");
			return;
		}
		int num = currentStateInfo.transitionAnim;
		if (num != -1 && !interrupt)
		{
			transitionAnim = num;
			PlayAnimation(num);
		}
		else
		{
			currentFunctions.Enter(this, interrupt);
		}
		if (!targetObject.playCallouts && IsImpactState())
		{
			targetObject.PlayOuchSound(1f, always: true);
		}
	}

	private void ExitState()
	{
		if (currentFunctions == null)
		{
			return;
		}
		currentFunctions.Exit(this);
		if (currentState == CharacterState.Recover)
		{
			Collider component = targetObject.go.GetComponent<Collider>();
			if (component != null)
			{
				component.enabled = true;
			}
			targetObject.Unfreeze();
		}
		CharacterState desiredState = currentFunctions.desiredState;
		currentFunctions = null;
		if (desiredStateQueue.Count > 0)
		{
			currentState = desiredStateQueue.Dequeue();
		}
		else if (desiredState != CharacterState.None)
		{
			currentState = desiredState;
		}
		else
		{
			currentState = CharacterState.None;
		}
	}

	public void StartPull()
	{
		isPulling = true;
		targetObject.walkController.StartPull();
	}

	public void StopPull()
	{
		isPulling = false;
		targetObject.walkController.StopPull();
		if (targetObject.IsOnGround() && currentState == CharacterState.Balance)
		{
			stateTime = 0f;
		}
	}

	public bool IsPulling()
	{
		return isPulling;
	}

	public bool WasPulling()
	{
		return targetObject.walkController.WasPulled();
	}

	public void Freeze()
	{
		targetController.speed = 0f;
	}

	public void Unfreeze()
	{
		targetController.speed = 1f;
	}

	public void Update()
	{
		if (currentStateInfo == null || targetObject == null || targetController == null)
		{
			return;
		}
		stateTime += Time.fixedDeltaTime;
		if (Blocksworld.CurrentState != State.Play)
		{
			List<Block> list = ConnectednessGraph.ConnectedComponent(targetObject, 1);
			bool flag = Blocksworld.selectedBlock == targetObject;
			if (Blocksworld.selectedBunch != null)
			{
				flag |= Blocksworld.selectedBunch.ContainsBlock(targetObject);
			}
			bool flag2 = targetObject.attachedHeadBlocks != null && targetObject.attachedHeadBlocks.Count > 0;
			bool flag3 = !preventLook && !flag2 && list.Count <= 1 && targetObject.goT.up.y > 0.9f && targetObject.IsOnGround() && !flag;
			if (currentState == CharacterState.EditSitting && !flag3)
			{
				InterruptState(CharacterState.EditSittingStill);
			}
			if (currentState == CharacterState.EditSittingStill && flag3)
			{
				EnterState(CharacterState.EditSitting);
			}
			if (currentFunctions != null)
			{
				currentFunctions.Update(this);
			}
		}
		else
		{
			if (rb == null || targetController.speed == 0f)
			{
				return;
			}
			bool flag4 = targetObject.IsOnGround();
			if (startPlay && stateTime >= 0.05f)
			{
				startPlay = false;
				if (flag4 && sideAnim == -1)
				{
					rb.AddForce(1000f * targetObject.goT.up);
				}
			}
			isTransitioning = false;
			desiredGoto.y = 0f;
			isTransitioning = targetController.IsInTransition(0);
			bool flag5 = IsImpactState();
			rootOffset = Vector3.Lerp(rootOffset, currentFunctions.GetOffset(this), 0.2f);
			if (targetObject != null && !targetObject.unmoving)
			{
				targetRig.transform.localPosition = rootOffset - 0.3f * targetObject.goT.up;
			}
			actualAnimSpeed = Mathf.Lerp(actualAnimSpeed, currentStateInfo.animationRate * desiredAnimSpeed, 0.1f);
			targetController.SetFloat("BaseLayerAnimationSpeed", actualAnimSpeed);
			currentVelocity = targetObject.goT.InverseTransformDirection(rb.velocity);
			currentVelocity.y = 0f;
			float magnitude = currentVelocity.magnitude;
			if (magnitude > 0.05f && targetObject.IsOnGround() && currentState != CharacterState.Balance && isPulling && targetObject.goT.up.y > 0.9f && !isTransitioning)
			{
				InterruptState(CharacterState.Balance, noBlend: false);
			}
			wasDesiringMove = desiresMove;
			desiresMove = requestedMoveVelocity.sqrMagnitude > Mathf.Epsilon || Mathf.Abs(turnPower) > 0f || (targetObject.walkController.previousVicinityMode == WalkControllerAnimated.VicinityMode.AvoidTag && magnitude > 0.085f);
			if (!flag5 && !desiresMove && !wasDesiringMove && magnitude > 10f * Mathf.Max(0.1f, currentVelMag))
			{
				if (Mathf.Abs(currentVelocity.x) > Mathf.Abs(currentVelocity.z))
				{
					float num = 6f;
					if (currentVelocity.x > 0f)
					{
						if (currentVelocity.x > num)
						{
							InterruptState(CharacterState.ImpactLeft);
						}
						else
						{
							InterruptState(CharacterState.SoftHitLeft);
						}
					}
					else if (currentVelocity.x < 0f - num)
					{
						InterruptState(CharacterState.ImpactRight);
					}
					else
					{
						InterruptState(CharacterState.SoftHitRight);
					}
				}
				else if (currentVelocity.z > 0f)
				{
					InterruptState(CharacterState.SoftHitBack);
				}
				else
				{
					InterruptState(CharacterState.SoftHitFront);
				}
			}
			currentVelMag = magnitude;
			if (targetCape != null || targetJetpack != null)
			{
				if (!flag4 && targetObject.isHovering && currentState != CharacterState.Hover)
				{
					InterruptState(CharacterState.Hover);
				}
				else if (flag4 && currentState == CharacterState.Hover)
				{
					InterruptState(CharacterState.Idle);
				}
				if (targetCape != null)
				{
					if (desiresMove)
					{
						targetCape.rotation = Quaternion.Slerp(targetCape.rotation, capeFlightRotation, 0.1f);
					}
					else
					{
						targetCape.rotation = Quaternion.Slerp(targetCape.rotation, capeHoverRotation, 0.1f);
					}
				}
			}
			if (currentFunctions == null)
			{
				turnPower = 0f;
				return;
			}
			offset = -targetObject.go.transform.InverseTransformPoint(lastPos);
			lastPos = targetObject.go.transform.position;
			bool flag6 = BlockWater.BlockWithinWater(targetObject, checkDensity: true);
			if (currentState != CharacterState.Flail && isPulling && !targetObject.unmoving && !flag4 && !isTransitioning && currentState != CharacterState.Jump && !flag6 && currentState != CharacterState.Hover)
			{
				InterruptState(CharacterState.Flail, noBlend: false);
			}
			if (transitionAnim != -1)
			{
				if (transitionAnim != targetController.GetCurrentAnimatorStateInfo(0).shortNameHash)
				{
					transitionAnim = -1;
					currentFunctions.Enter(this, interrupt: true);
				}
				turnPower = 0f;
				return;
			}
			if (!flag5)
			{
				bool flag7 = IsSwimming();
				if (flag7 && targetObject.goT.up.y < 0.99f)
				{
					if (targetObject.goT.up.y < -0.97f)
					{
						rb.AddForceAtPosition(5f * Vector3.forward, targetObject.goT.position + 0.5f * targetObject.goT.up);
					}
					rb.AddForceAtPosition(45f * Vector3.up, targetObject.goT.position + 0.3f * targetObject.goT.up);
					rb.AddForceAtPosition(-45f * Vector3.up, targetObject.goT.position - 0.3f * targetObject.goT.up);
				}
				if (flag6 && !flag7)
				{
					bool flag8 = true;
					if (IsJumping())
					{
						CharacterJumpState characterJumpState = (CharacterJumpState)currentFunctions;
						flag8 = !characterJumpState.HeadingUp(this);
					}
					if (flag8)
					{
						InterruptState(CharacterState.SwimIn, noBlend: false);
					}
				}
				else if (flag7 && !flag6 && stateTime > 0.75f)
				{
					InterruptState(CharacterState.SwimOut, noBlend: false);
				}
			}
			if (currentState == CharacterState.PlayAnim && (currentVelMag > 0.05f || rootOffset.sqrMagnitude > 0.05f))
			{
				InterruptState(CharacterState.Idle);
			}
			switch (currentState)
			{
			case CharacterState.Idle:
				if (desiresMove)
				{
					if (IsOnSide())
					{
						InterruptState(CharacterState.GetUp);
					}
					else
					{
						InterruptState(CharacterState.Walk, isTransitioning);
					}
				}
				else if (rb.velocity.y < -0.1f && !flag4 && targetCape == null && targetJetpack == null)
				{
					StartFalling();
				}
				break;
			case CharacterState.Walk:
			{
				float num2 = targetObject.NearGround(0.3f);
				if ((num2 > 0.2f || num2 < 0f) && targetCape == null && !IsHover() && rb.velocity.y < -5f)
				{
					StartFalling();
				}
				break;
			}
			case CharacterState.Balance:
				if (!isPulling && !isTransitioning && stateTime > 0.15f)
				{
					if (flag4)
					{
						InterruptState(CharacterState.Idle);
					}
					else
					{
						InterruptState(CharacterState.Jump);
					}
				}
				break;
			case CharacterState.CrawlIdle:
				if (Mathf.Abs(offset.z) > 0.01f)
				{
					EnterState(CharacterState.Crawl);
				}
				break;
			case CharacterState.Flail:
				if (!isPulling)
				{
					if (targetObject.NearGround(0.2f) > 0f)
					{
						InterruptState(CharacterState.Idle);
					}
					else if (flag6)
					{
						InterruptState(CharacterState.SwimIdle);
					}
				}
				break;
			case CharacterState.SwimIdle:
				if (Mathf.Abs(offset.z) > 0.04f)
				{
					EnterState(CharacterState.Swim);
				}
				break;
			case CharacterState.Swim:
				if (Mathf.Abs(offset.z) < 0.04f)
				{
					EnterState(CharacterState.SwimIdle);
				}
				break;
			}
			if (!currentFunctions.Update(this))
			{
				if (desiredStateQueue.Count > 0)
				{
					CharacterState desiredState = desiredStateQueue.Dequeue();
					EnterState(desiredState);
				}
				else if (currentState == CharacterState.Jump && desiredGoto.magnitude > 0.1f && targetObject.goT.up.y > 0.85f)
				{
					InterruptState(CharacterState.Walk);
				}
				else
				{
					EnterState(currentFunctions.desiredState);
				}
			}
			turnPower = 0f;
			upperBody.Update();
			combatController.Update();
			if (requestingPlayAnim)
			{
				requestingPlayAnim = false;
			}
		}
	}

	public int PlayAnimation(int hash, bool interrupt = false)
	{
		animationHash = hash;
		playingAnim = "Hash " + animationHash;
		if (null == targetController)
		{
			BWLog.Warning("No anim controller for block " + ((targetObject == null) ? "null" : targetObject.go.name) + " attempting to play " + animationHash);
			return 0;
		}
		float transitionDuration = stateBlend;
		if (targetController.IsInTransition(0) && !IsImpactState() && Blocksworld.CurrentState == State.Play)
		{
			transitionDuration = Mathf.Max(stateBlend, stateTime - blendStart + 1f / 30f);
		}
		blendStart = stateTime;
		if (stateBlend > 0f && !interrupt)
		{
			isTransitioning = true;
			targetController.CrossFade(animationHash, transitionDuration, -1, 0f);
		}
		else if (targetController.GetCurrentAnimatorStateInfo(0).normalizedTime > 1f)
		{
			targetController.Play(animationHash, 0, 0f);
		}
		else
		{
			targetController.Play(animationHash);
		}
		return animationHash;
	}

	public int ShiftAnimation(string animation)
	{
		ShiftAnimation(Animator.StringToHash(animation));
		playingAnim = animation;
		return animationHash;
	}

	public int ShiftAnimation(int hash)
	{
		animationHash = hash;
		playingAnim = "Hash " + hash;
		if (null == targetController)
		{
			BWLog.Warning("No anim controller for block " + ((targetObject == null) ? "null" : targetObject.go.name) + " attempting to play " + animationHash);
			return 0;
		}
		float transitionDuration = stateBlend;
		if (targetController.IsInTransition(0) && !IsImpactState() && Blocksworld.CurrentState == State.Play)
		{
			transitionDuration = Mathf.Max(stateBlend, stateTime - blendStart + 1f / 30f);
		}
		blendStart = stateTime;
		AnimatorStateInfo currentAnimatorStateInfo = targetController.GetCurrentAnimatorStateInfo(0);
		targetController.CrossFade(animationHash, transitionDuration, -1, currentAnimatorStateInfo.normalizedTime % 1f);
		return animationHash;
	}

	public int StartAnimationAt(string animation, float percent)
	{
		StartAnimationAt(Animator.StringToHash(animation), percent);
		playingAnim = animation;
		return animationHash;
	}

	public int StartAnimationAt(int hash, float percent)
	{
		animationHash = hash;
		playingAnim = "Hash " + hash;
		if (null == targetController)
		{
			BWLog.Warning("No anim controller for block " + ((targetObject == null) ? "null" : targetObject.go.name) + " attempting to play " + animationHash);
			return 0;
		}
		float transitionDuration = stateBlend;
		if (targetController.IsInTransition(0) && !IsImpactState() && Blocksworld.CurrentState == State.Play)
		{
			transitionDuration = Mathf.Max(stateBlend, stateTime - blendStart + 1f / 30f);
		}
		blendStart = stateTime;
		AnimatorStateInfo currentAnimatorStateInfo = targetController.GetCurrentAnimatorStateInfo(0);
		targetController.CrossFade(animationHash, transitionDuration, -1, percent * currentAnimatorStateInfo.normalizedTime);
		return animationHash;
	}

	public void OnScreenDebug()
	{
	}

	public bool HitByHandAttachment(Block block)
	{
		Block rightHandAttachment = combatController.GetRightHandAttachment();
		Block leftHandAttachment = combatController.GetLeftHandAttachment();
		if (!combatController.IsHitByBlockThisFrame(block, rightHandAttachment))
		{
			return combatController.IsHitByBlockThisFrame(block, leftHandAttachment);
		}
		return true;
	}

	public bool HitByFoot(Block block)
	{
		return combatController.IsHitByFootThisFrame(block);
	}

	public bool HitModelByFoot(Block hitBlock)
	{
		HashSet<Block> hashSet = combatController.BlocksHitThisFrameByFeet();
		foreach (Block item in hashSet)
		{
			if (hitBlock.modelBlock == item)
			{
				return true;
			}
		}
		return false;
	}

	public bool HitByTaggedHandAttachment(Block block, string tag)
	{
		Block rightHandAttachment = combatController.GetRightHandAttachment();
		if (rightHandAttachment != null && TagManager.GetBlockTags(rightHandAttachment).Contains(tag) && combatController.IsHitByBlockThisFrame(block, rightHandAttachment))
		{
			return true;
		}
		Block leftHandAttachment = combatController.GetLeftHandAttachment();
		if (leftHandAttachment != null && TagManager.GetBlockTags(leftHandAttachment).Contains(tag))
		{
			return combatController.IsHitByBlockThisFrame(block, leftHandAttachment);
		}
		return false;
	}

	public bool HitModelByHandAttachment(Block modelBlock)
	{
		Block rightHandAttachment = combatController.GetRightHandAttachment();
		HashSet<Block> hashSet = ((rightHandAttachment != null) ? combatController.BlocksHitThisFrameByBlock(rightHandAttachment) : null);
		if (hashSet != null)
		{
			foreach (Block item in hashSet)
			{
				if (item.modelBlock == modelBlock)
				{
					return true;
				}
			}
		}
		Block leftHandAttachment = combatController.GetLeftHandAttachment();
		HashSet<Block> hashSet2 = ((leftHandAttachment != null) ? combatController.BlocksHitThisFrameByBlock(leftHandAttachment) : null);
		if (hashSet2 != null)
		{
			foreach (Block item2 in hashSet2)
			{
				if (item2.modelBlock == modelBlock)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HitModelByTaggedHandAttachment(Block modelBlock, string tag)
	{
		Block rightHandAttachment = combatController.GetRightHandAttachment();
		if (rightHandAttachment != null)
		{
			List<string> blockTags = TagManager.GetBlockTags(rightHandAttachment);
			if (blockTags.Contains(tag))
			{
				HashSet<Block> hashSet = combatController.BlocksHitThisFrameByBlock(rightHandAttachment);
				if (hashSet != null)
				{
					foreach (Block item in hashSet)
					{
						if (item.modelBlock == modelBlock)
						{
							return true;
						}
					}
				}
			}
		}
		Block leftHandAttachment = combatController.GetLeftHandAttachment();
		if (leftHandAttachment != null)
		{
			List<string> blockTags2 = TagManager.GetBlockTags(leftHandAttachment);
			if (blockTags2.Contains(tag))
			{
				HashSet<Block> hashSet2 = combatController.BlocksHitThisFrameByBlock(leftHandAttachment);
				if (hashSet2 != null)
				{
					foreach (Block item2 in hashSet2)
					{
						if (item2.modelBlock == modelBlock)
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	public bool FiredAsWeapon(Block block)
	{
		if (block != combatController.GetRightHandAttachment() || !combatController.RightAttachmentFired())
		{
			if (block == combatController.GetLeftHandAttachment())
			{
				return combatController.LeftAttachmentFired();
			}
			return false;
		}
		return true;
	}

	public void ClearAttackFlags()
	{
		combatController.ClearAttackFlags();
	}

	public void SaveAnimatorState()
	{
		AnimatorControllerParameter[] parameters = targetController.parameters;
		savedAnimatorParameterValues = new Dictionary<int, object>();
		for (int i = 0; i < parameters.Length; i++)
		{
			object obj = null;
			AnimatorControllerParameter animatorControllerParameter = parameters[i];
			switch (animatorControllerParameter.type)
			{
			case AnimatorControllerParameterType.Float:
				obj = targetController.GetFloat(animatorControllerParameter.nameHash);
				break;
			case AnimatorControllerParameterType.Int:
				obj = targetController.GetInteger(animatorControllerParameter.nameHash);
				break;
			case AnimatorControllerParameterType.Bool:
				obj = targetController.GetBool(animatorControllerParameter.nameHash);
				break;
			}
			if (obj != null)
			{
				savedAnimatorParameterValues.Add(animatorControllerParameter.nameHash, obj);
			}
		}
	}

	public void RestoreAnimatorState()
	{
		if (savedAnimatorParameterValues == null)
		{
			return;
		}
		AnimatorControllerParameter[] parameters = targetController.parameters;
		foreach (AnimatorControllerParameter animatorControllerParameter in parameters)
		{
			if (savedAnimatorParameterValues.TryGetValue(animatorControllerParameter.nameHash, out var value))
			{
				switch (animatorControllerParameter.type)
				{
				case AnimatorControllerParameterType.Float:
					targetController.SetFloat(animatorControllerParameter.nameHash, (float)value);
					break;
				case AnimatorControllerParameterType.Int:
					targetController.SetInteger(animatorControllerParameter.nameHash, (int)value);
					break;
				case AnimatorControllerParameterType.Bool:
					targetController.SetBool(animatorControllerParameter.nameHash, (bool)value);
					break;
				}
			}
		}
		savedAnimatorParameterValues = null;
	}

	public void Bounce(float bounciness)
	{
		if (currentState == CharacterState.Jump)
		{
			currentJumpState = CharacterJumpState.JumpState.Up;
			Vector3 bounceVector = targetObject.walkController.GetBounceVector(bounciness);
			targetObject.walkController.Bounce(bounceVector);
			CharacterJumpState characterJumpState = (CharacterJumpState)currentFunctions;
			characterJumpState.Bounce(this);
		}
		StartJump(bounciness * 10f);
	}
}
