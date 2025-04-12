using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020002A0 RID: 672
	public class CharacterStateHandler : StateHandlerBase
	{
		// Token: 0x06001EEB RID: 7915 RVA: 0x000DD820 File Offset: 0x000DBC20
		public CharacterStateHandler()
		{
			this.combatController = new CombatController();
			this.combatController.AttachToStateHandler(this);
			this.upperBody = new UpperBodyStateHandler(this);
			this.upperBody.combatController = this.combatController;
		}

		// Token: 0x06001EEC RID: 7916 RVA: 0x000DD93A File Offset: 0x000DBD3A
		public static void ClearStateMap()
		{
			CharacterStateHandler.stateMap.Clear();
		}

		// Token: 0x06001EED RID: 7917 RVA: 0x000DD948 File Offset: 0x000DBD48
		public static void LoadStateMap(string jsonMap, CharacterRole defaultRole = CharacterRole.Male)
		{
			JObject jobject = JSONDecoder.Decode(jsonMap);
			Dictionary<string, JObject> objectValue = jobject.ObjectValue;
			foreach (JObject jobject2 in objectValue["states"].ArrayValue)
			{
				CharacterStateHandler.InternalStateInfo internalStateInfo = new CharacterStateHandler.InternalStateInfo();
				CharacterState characterState = (CharacterState)Enum.Parse(typeof(CharacterState), jobject2["name"].StringValue);
				CharacterRole key = defaultRole;
				if (jobject2.ContainsKey("role"))
				{
					key = (CharacterRole)Enum.Parse(typeof(CharacterRole), jobject2["role"].StringValue);
				}
				List<string> list = (!jobject2.ContainsKey("animations")) ? null : new List<string>(jobject2["animations"].StringValue.Split(new char[]
				{
					','
				}));
				if (jobject2.ContainsKey("tags"))
				{
					internalStateInfo.tags = new HashSet<string>(jobject2["tags"].StringValue.ToLower().Split(new char[]
					{
						','
					}));
				}
				string a = "Anim";
				bool isLeftHanded = false;
				bool isRightHanded = false;
				bool isLeftFooted = false;
				bool isRightFooted = false;
				if (jobject2.ContainsKey("feet"))
				{
					List<string> list2 = new List<string>(jobject2["feet"].StringValue.Split(new char[]
					{
						','
					}));
					foreach (string text in list2)
					{
						if (text.ToLower() == "left")
						{
							isLeftFooted = true;
						}
						if (text.ToLower() == "right")
						{
							isRightFooted = true;
						}
					}
				}
				else if (jobject2.ContainsKey("hands"))
				{
					List<string> list3 = new List<string>(jobject2["hands"].StringValue.Split(new char[]
					{
						','
					}));
					foreach (string text2 in list3)
					{
						if (text2.ToLower() == "left")
						{
							isLeftHanded = true;
						}
						if (text2.ToLower() == "right")
						{
							isRightHanded = true;
						}
					}
				}
				else
				{
					isRightHanded = true;
				}
				if (jobject2.ContainsKey("type"))
				{
					a = jobject2["type"].StringValue;
				}
				if (characterState == CharacterState.Idle)
				{
					CharacterIdleState characterIdleState = new CharacterIdleState();
					characterIdleState.animations = list;
					characterIdleState.leftIdle = ((!jobject2.ContainsKey("leftSide")) ? string.Empty : jobject2["leftSide"].StringValue);
					characterIdleState.rightIdle = ((!jobject2.ContainsKey("rightSide")) ? string.Empty : jobject2["rightSide"].StringValue);
					characterIdleState.frontIdle = ((!jobject2.ContainsKey("frontSide")) ? string.Empty : jobject2["frontSide"].StringValue);
					characterIdleState.backIdle = ((!jobject2.ContainsKey("backSide")) ? string.Empty : jobject2["backSide"].StringValue);
					characterIdleState.topIdle = ((!jobject2.ContainsKey("topSide")) ? string.Empty : jobject2["topSide"].StringValue);
					characterIdleState.sideOffsets = new List<Vector3>();
					if (jobject2.ContainsKey("sideOffsets"))
					{
						List<JObject> arrayValue = jobject2["sideOffsets"].ArrayValue;
						for (int i = 0; i < arrayValue.Count; i++)
						{
							List<string> list4 = new List<string>(arrayValue[i].StringValue.Split(new char[]
							{
								','
							}));
							characterIdleState.sideOffsets.Add(new Vector3(float.Parse(list4[0]), float.Parse(list4[1]), float.Parse(list4[2])));
						}
					}
					while (characterIdleState.sideOffsets.Count < 5)
					{
						characterIdleState.sideOffsets.Add(Vector3.zero);
					}
					internalStateInfo.stateFunctions = characterIdleState;
				}
				else if (a == "Melee_Attack")
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
					characterAttackState.maxForwardSpeed = ((!jobject2.ContainsKey("max_forward_speed")) ? 0f : jobject2["max_forward_speed"].FloatValue);
					characterAttackState.minForwardSpeed = ((!jobject2.ContainsKey("min_forward_speed")) ? 0f : jobject2["min_forward_speed"].FloatValue);
					characterAttackState.moveEndNormalizedTime = ((!jobject2.ContainsKey("move_end_normalized")) ? 0f : jobject2["move_end_normalized"].FloatValue);
					meleeAttackState.damageStartNormalizedTime = ((!jobject2.ContainsKey("damage_start_normalized")) ? 0f : jobject2["damage_start_normalized"].FloatValue);
					meleeAttackState.damageEndNormalizedTime = ((!jobject2.ContainsKey("damage_end_normalized")) ? 1f : jobject2["damage_end_normalized"].FloatValue);
					meleeAttackState.interruptNormalizedTime = ((!jobject2.ContainsKey("interrupt_normalized")) ? 0.8f : jobject2["interrupt_normalized"].FloatValue);
					internalStateInfo.stateFunctions = characterAttackState;
				}
				else if (a == "Dodge")
				{
					internalStateInfo.stateFunctions = new CharacterDodgeState
					{
						animation = list[0],
						speed = ((!jobject2.ContainsKey("speed")) ? 5f : jobject2["speed"].FloatValue),
						dodgeStartNormalizedTime = ((!jobject2.ContainsKey("move_start_normalized")) ? 0f : jobject2["move_start_normalized"].FloatValue),
						dodgeEndNormalizedTime = ((!jobject2.ContainsKey("move_end_normalized")) ? 1f : jobject2["move_end_normalized"].FloatValue)
					};
				}
				else if (a == "GetUp")
				{
					CharacterGetUpState characterGetUpState = new CharacterGetUpState();
					characterGetUpState.timing = new List<float>();
					characterGetUpState.animations = list;
					if (jobject2.ContainsKey("time"))
					{
						List<JObject> arrayValue2 = jobject2["time"].ArrayValue;
						foreach (JObject jobject3 in arrayValue2)
						{
							characterGetUpState.timing.Add(jobject3.FloatValue);
						}
					}
					while (characterGetUpState.timing.Count < characterGetUpState.animations.Count)
					{
						characterGetUpState.timing.Add(0.5f);
					}
					internalStateInfo.stateFunctions = characterGetUpState;
				}
				else if (a == "Chain")
				{
					internalStateInfo.stateFunctions = new CharacterChainAnimState
					{
						animationHash = Animator.StringToHash((list.Count <= 0) ? string.Empty : list[0])
					};
				}
				else if (a == "Walk")
				{
					CharacterWalkState characterWalkState = new CharacterWalkState();
					if (list != null && list.Count > 0)
					{
						characterWalkState.AddDirection(jobject2);
					}
					else if (jobject2.ContainsKey("directions"))
					{
						List<JObject> arrayValue3 = jobject2["directions"].ArrayValue;
						foreach (JObject state in arrayValue3)
						{
							characterWalkState.AddDirection(state);
						}
					}
					if (jobject2.ContainsKey("baseRunVel"))
					{
						characterWalkState.baseRunVel = jobject2["baseRunVel"].FloatValue;
					}
					internalStateInfo.stateFunctions = characterWalkState;
				}
				else if (a == "Jump")
				{
					CharacterJumpState characterJumpState = new CharacterJumpState();
					characterJumpState.animations = list;
					if (jobject2.ContainsKey("blends"))
					{
						characterJumpState.blendSpeeds = new List<float>();
						List<JObject> arrayValue4 = jobject2["blends"].ArrayValue;
						for (int j = 0; j < arrayValue4.Count; j++)
						{
							characterJumpState.blendSpeeds.Add(arrayValue4[j].FloatValue);
						}
					}
					internalStateInfo.stateFunctions = characterJumpState;
				}
				else if (a == "Hover")
				{
					internalStateInfo.stateFunctions = new CharacterHoverState
					{
						animations = list
					};
				}
				else if (a == "PlayAnim")
				{
					CharacterPlayAnimState stateFunctions = new CharacterPlayAnimState();
					internalStateInfo.stateFunctions = stateFunctions;
				}
				else
				{
					internalStateInfo.stateFunctions = new CharacterSingleAnimState
					{
						animationHash = Animator.StringToHash((list.Count <= 0) ? string.Empty : list[0]),
						animations = list
					};
				}
				if (internalStateInfo.stateFunctions != null && jobject2.ContainsKey("desired"))
				{
					internalStateInfo.stateFunctions.desiredState = (CharacterState)Enum.Parse(typeof(CharacterState), jobject2["desired"].StringValue);
				}
				else
				{
					internalStateInfo.stateFunctions.desiredState = CharacterState.Idle;
				}
				if (jobject2.ContainsKey("transition"))
				{
					internalStateInfo.transitionAnim = Animator.StringToHash(jobject2["transition"].StringValue);
				}
				else
				{
					internalStateInfo.transitionAnim = -1;
				}
				if (jobject2.ContainsKey("blend"))
				{
					internalStateInfo.transitionBlend = jobject2["blend"].FloatValue;
				}
				else
				{
					internalStateInfo.transitionBlend = 0.025f;
				}
				if (jobject2.ContainsKey("offsets"))
				{
					List<JObject> arrayValue5 = jobject2["offsets"].ArrayValue;
					if (arrayValue5.Count > 0)
					{
						internalStateInfo.stateFunctions.rootOffsets = new List<Vector3>();
					}
					for (int k = 0; k < arrayValue5.Count; k++)
					{
						List<string> list5 = new List<string>(arrayValue5[k].StringValue.Split(new char[]
						{
							','
						}));
						internalStateInfo.stateFunctions.rootOffsets.Add(new Vector3(float.Parse(list5[0]), float.Parse(list5[1]), float.Parse(list5[2])));
					}
				}
				else if (jobject2.ContainsKey("offset"))
				{
					internalStateInfo.stateFunctions.rootOffsets = new List<Vector3>();
					List<string> list6 = new List<string>(jobject2["offset"].StringValue.Split(new char[]
					{
						','
					}));
					internalStateInfo.stateFunctions.rootOffsets.Add(new Vector3(float.Parse(list6[0]), float.Parse(list6[1]), float.Parse(list6[2])));
				}
				if (jobject2.ContainsKey("animationRate"))
				{
					float floatValue = jobject2["animationRate"].FloatValue;
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
				if (jobject2.ContainsKey("allowed"))
				{
					List<string> list7 = new List<string>(((!jobject2.ContainsKey("allowed")) ? string.Empty : jobject2["allowed"].StringValue).Split(new char[]
					{
						','
					}));
					foreach (string text3 in list7)
					{
						if (text3 == "UprightDefault")
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
						else if (text3 == "SwimDefault")
						{
							internalStateInfo.allowedTransitions.Add(CharacterState.Idle);
							internalStateInfo.allowedTransitions.Add(CharacterState.SwimIdle);
							internalStateInfo.allowedTransitions.Add(CharacterState.Swim);
							internalStateInfo.allowedTransitions.Add(CharacterState.Hover);
							internalStateInfo.allowedTransitions.Add(CharacterState.SwimOut);
						}
						else
						{
							internalStateInfo.allowedTransitions.Add((CharacterState)Enum.Parse(typeof(CharacterState), text3));
						}
					}
				}
				if (!CharacterStateHandler.stateMap.ContainsKey(characterState))
				{
					CharacterStateHandler.stateMap[characterState] = new Dictionary<CharacterRole, CharacterStateHandler.InternalStateInfo>();
				}
				CharacterStateHandler.stateMap[characterState][key] = internalStateInfo;
			}
		}

		// Token: 0x06001EEE RID: 7918 RVA: 0x000DE934 File Offset: 0x000DCD34
		public override bool IsPlayingAnimation(string anim)
		{
			return this.targetController.GetCurrentAnimatorStateInfo(0).IsName(anim);
		}

		// Token: 0x06001EEF RID: 7919 RVA: 0x000DE956 File Offset: 0x000DCD56
		public override int PlayAnimation(string animation, bool interrupt = false)
		{
			this.PlayAnimation(Animator.StringToHash(animation), interrupt);
			this.playingAnim = animation;
			return this.animationHash;
		}

		// Token: 0x06001EF0 RID: 7920 RVA: 0x000DE973 File Offset: 0x000DCD73
		public override int GetAnimatorLayer()
		{
			return 0;
		}

		// Token: 0x06001EF1 RID: 7921 RVA: 0x000DE976 File Offset: 0x000DCD76
		public override float TimeInCurrentState()
		{
			return this.stateTime;
		}

		// Token: 0x06001EF2 RID: 7922 RVA: 0x000DE980 File Offset: 0x000DCD80
		public float StateNormalizedTime()
		{
			return this.targetController.GetCurrentAnimatorStateInfo(0).normalizedTime;
		}

		// Token: 0x06001EF3 RID: 7923 RVA: 0x000DE9A4 File Offset: 0x000DCDA4
		protected CharacterStateHandler.InternalStateInfo GetStateInfo(CharacterState state = CharacterState.None, CharacterRole role = CharacterRole.None)
		{
			if (state == CharacterState.None)
			{
				state = this.currentState;
			}
			if (role == CharacterRole.None)
			{
				role = this.currentRole;
			}
			if (!CharacterStateHandler.stateMap.ContainsKey(state))
			{
				return null;
			}
			Dictionary<CharacterRole, CharacterStateHandler.InternalStateInfo> dictionary = CharacterStateHandler.stateMap[state];
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

		// Token: 0x06001EF4 RID: 7924 RVA: 0x000DEA34 File Offset: 0x000DCE34
		public void SetTarget(BlockAnimatedCharacter targetBlock, GameObject targetGO = null)
		{
			if (targetBlock != null)
			{
			}
			this.targetObject = targetBlock;
			if (null == targetGO)
			{
				targetGO = this.targetObject.go;
			}
			this.targetRig = targetGO;
			if (null != this.targetRig)
			{
				this.targetController = this.targetRig.GetComponent<Animator>();
			}
			if (null == this.targetController)
			{
				return;
			}
			this.upperBody.targetController = this.targetController;
			this.upperBody.targetObject = this.targetObject;
			this.targetRig.transform.parent = this.targetObject.go.transform;
			this.targetRig.transform.localRotation = Quaternion.identity;
		}

		// Token: 0x06001EF5 RID: 7925 RVA: 0x000DEAFC File Offset: 0x000DCEFC
		public void Play()
		{
			this.offset = Vector3.zero;
			this.currentVelMag = 0f;
			this.turnPower = 0f;
			this.desiredGoto = Vector3.zero;
			this.rootOffset = Vector3.zero;
			this.currentVelocity = Vector3.zero;
			this.speedForceModifier = 1f;
			this.desiredJumpForce = 1f;
			this.desiredAnimSpeed = 1f;
			this.actualAnimSpeed = 1f;
			this.desiresMove = false;
			this.wasDesiringMove = false;
			this.playingAnim = string.Empty;
			this.blendStart = 0f;
			this.animationHash = -1;
			this.firstFrame = false;
			this.startPlay = true;
			this.hasDoubleJumped = false;
			this.currentVelocityRange = 0;
			this.currentSpeed = 0f;
			this.currentDirection = CharacterWalkState.WalkDirection.NumDirections;
			this.deliberateWalk = false;
			this.walkStrafe = false;
			this.currentState = CharacterState.None;
			this.currentFunctions = null;
			this.isPulling = false;
			this.rb = this.targetObject.body.GetComponent<Rigidbody>();
			this.targetCape = null;
			this.targetJetpack = null;
			this.desiredStateQueue.Clear();
			this.currentState = CharacterState.None;
			this.currentFunctions = null;
			this.ForceSitStill();
			this.AddCharacterAttachments();
			this.combatController.Play();
			this.upperBody.Play();
			this.SetSpeedForce(1f);
			if (this.targetObject.unmoving)
			{
				bool flag = this.targetObject.attachmentsPreventLookAnim || this.targetObject.isConnectedToUnsupportedActionBlock;
				this.EnterState((!flag) ? CharacterState.Sitting : CharacterState.SittingStill, false);
			}
			else if (this.targetJetpack != null)
			{
				this.EnterState(CharacterState.Hover, false);
			}
			else
			{
				this.EnterState(CharacterState.Idle, false);
				if (this.IsOnSide())
				{
					this.InterruptState(CharacterState.GetUp, true);
				}
			}
		}

		// Token: 0x06001EF6 RID: 7926 RVA: 0x000DECDD File Offset: 0x000DD0DD
		public void Appearing()
		{
			if (this.targetObject.unmoving)
			{
				this.ForceSit();
			}
		}

		// Token: 0x06001EF7 RID: 7927 RVA: 0x000DECF8 File Offset: 0x000DD0F8
		public void IgnoreRaycasts(bool ignore)
		{
			Layer layer = (!ignore) ? Layer.Default : Layer.MeshEmitters;
			for (int i = 0; i < this.attachments.Count; i++)
			{
				this.attachments[i].go.SetLayer(layer, true);
			}
			this.combatController.IgnoreRaycasts(ignore, layer);
		}

		// Token: 0x06001EF8 RID: 7928 RVA: 0x000DED58 File Offset: 0x000DD158
		public void AddCharacterAttachments()
		{
			this.combatController.rightHandAttachmentParent = this.targetObject.GetHandAttach(0).transform;
			this.combatController.leftHandAttachmentParent = this.targetObject.GetHandAttach(1).transform;
			this.combatController.rightFootAttachmentParent = this.targetObject.GetRightFootBoneTransform();
			this.combatController.leftFootAttachmentParent = this.targetObject.GetLeftFootBoneTransform();
			if (this.targetObject.unmoving)
			{
				for (int i = 0; i < this.targetObject.attachedHeadBlocks.Count; i++)
				{
					Block block = this.targetObject.attachedHeadBlocks[i];
					if (CharacterEditor.IsGear(block))
					{
						this.attachments.Add(block);
						block.goT.SetParent(this.targetObject.GetHeadAttach().transform);
					}
				}
				return;
			}
			Block attachedRightBlock = this.targetObject.attachedRightBlock;
			Block attachedLeftBlock = this.targetObject.attachedLeftBlock;
			if (attachedRightBlock != null)
			{
				foreach (Block block2 in this.targetObject.attachedRightHandBlocks)
				{
					block2.goT.SetParent(attachedRightBlock.goT);
				}
				bool applyOffset = attachedRightBlock.BlockType() != "SC2 Mechanical Hand" && attachedRightBlock.BlockType() != "BBG Claws Right";
				this.combatController.AddRightHandAttachment(attachedRightBlock, applyOffset);
			}
			else
			{
				this.combatController.RemoveRightHandAttachment();
			}
			if (attachedLeftBlock != null)
			{
				foreach (Block block3 in this.targetObject.attachedLeftHandBlocks)
				{
					block3.goT.SetParent(attachedLeftBlock.goT);
				}
				bool applyOffset2 = attachedLeftBlock.BlockType() != "SC2 Mechanical Hand" && attachedLeftBlock.BlockType() != "BBG Claws Right";
				this.combatController.AddLeftHandAttachment(attachedLeftBlock, applyOffset2);
			}
			else
			{
				this.combatController.RemoveLeftHandAttachment();
			}
			for (int j = 0; j < this.targetObject.attachedBackBlocks.Count; j++)
			{
				Block block4 = this.targetObject.attachedBackBlocks[j];
				this.attachments.Add(block4);
				block4.goT.SetParent(this.targetObject.GetBodyAttach().transform);
				if (this.targetCape == null)
				{
					this.targetCape = (block4 as BlockAbstractAntiGravityWing);
					if (this.targetCape != null)
					{
						this.SetRole(CharacterRole.Caped);
						this.capeFlightRotation = this.targetCape.rotation;
						this.targetCape.rotation = Quaternion.Euler(0f, this.targetCape.rotation.y, this.targetCape.rotation.z);
						this.capeHoverRotation = this.targetCape.rotation;
					}
				}
				if (this.targetJetpack == null)
				{
					this.targetJetpack = (block4 as BlockAbstractJetpack);
				}
				if (block4.GetBlockMetaData() != null)
				{
					block4.goT.localPosition += block4.GetBlockMetaData().attachOffset;
				}
			}
			for (int k = 0; k < this.targetObject.attachedHeadBlocks.Count; k++)
			{
				Block block5 = this.targetObject.attachedHeadBlocks[k];
				this.attachments.Add(block5);
				block5.goT.SetParent(this.targetObject.GetHeadAttach().transform);
			}
		}

		// Token: 0x06001EF9 RID: 7929 RVA: 0x000DF144 File Offset: 0x000DD544
		public void Stop()
		{
			this.combatController.Stop();
			this.upperBody.Stop();
			this.Unfreeze();
			this.desiredStateQueue.Clear();
			this.currentState = CharacterState.None;
			this.currentFunctions = null;
			this.desiredAnimSpeed = 1f;
			this.actualAnimSpeed = 1f;
			this.blendStart = 0f;
			this.offset = Vector3.zero;
			this.desiredGoto = Vector3.zero;
			this.isPulling = false;
			this.ForceSit();
		}

		// Token: 0x06001EFA RID: 7930 RVA: 0x000DF1CA File Offset: 0x000DD5CA
		public void ForceSit()
		{
			this.rootOffset = Vector3.zero;
			this.targetRig.transform.localPosition = this.rootOffset - 1.2f * Vector3.up;
			this.ForceAnimState(CharacterState.EditSitting);
		}

		// Token: 0x06001EFB RID: 7931 RVA: 0x000DF209 File Offset: 0x000DD609
		public void ForceSitStill()
		{
			this.rootOffset = Vector3.zero;
			this.targetRig.transform.localPosition = this.rootOffset - 1.2f * Vector3.up;
			this.ForceAnimState(CharacterState.EditSittingStill);
		}

		// Token: 0x06001EFC RID: 7932 RVA: 0x000DF248 File Offset: 0x000DD648
		public void ForceEditPose()
		{
			this.targetRig.transform.localPosition = this.rootOffset + new Vector3(0f, -0.65f, -0.2f);
			this.ForceAnimState(CharacterState.EditModePose);
		}

		// Token: 0x06001EFD RID: 7933 RVA: 0x000DF284 File Offset: 0x000DD684
		private void ForceAnimState(CharacterState state)
		{
			this.stateBlend = 0f;
			this.desiredAnimSpeed = 1f;
			this.actualAnimSpeed = 1f;
			this.blendStart = -1f;
			this.currentState = state;
			this.currentStateInfo = this.GetStateInfo(CharacterState.None, CharacterRole.None);
			this.currentFunctions = this.currentStateInfo.stateFunctions;
			this.rootOffset = this.currentStateInfo.stateFunctions.GetOffset(this);
			CharacterSingleAnimState characterSingleAnimState = this.currentStateInfo.stateFunctions as CharacterSingleAnimState;
			characterSingleAnimState.Enter(this, true);
			this.targetController.Play(this.animationHash, -1, 0f);
			this.targetController.Update(0.04f);
		}

		// Token: 0x06001EFE RID: 7934 RVA: 0x000DF33C File Offset: 0x000DD73C
		protected void DetachCharacterAttachments()
		{
			this.combatController.UnparentHandAttachments();
			for (int i = 0; i < this.attachments.Count; i++)
			{
				Collider component = this.attachments[i].go.GetComponent<Collider>();
				if (component != null)
				{
					component.enabled = true;
				}
				this.attachments[i].goT.SetParent(null);
			}
			this.IgnoreRaycasts(false);
			this.attachments.Clear();
		}

		// Token: 0x06001EFF RID: 7935 RVA: 0x000DF3C3 File Offset: 0x000DD7C3
		public CharacterState GetState()
		{
			return this.currentState;
		}

		// Token: 0x06001F00 RID: 7936 RVA: 0x000DF3CC File Offset: 0x000DD7CC
		private bool CanChangeTo(CharacterState desiredState, CharacterState fromState = CharacterState.None)
		{
			if (fromState == CharacterState.None)
			{
				fromState = this.currentState;
			}
			if (fromState == CharacterState.None)
			{
				return true;
			}
			if (desiredState == CharacterState.PlayAnim && (fromState == CharacterState.Idle || fromState == CharacterState.PlayAnim) && this.currentVelMag <= 0.05f && this.rootOffset.sqrMagnitude <= 0.05f)
			{
				return this.sideAnim == -1;
			}
			CharacterStateHandler.InternalStateInfo internalStateInfo = (fromState != this.currentState) ? this.GetStateInfo(fromState, CharacterRole.None) : this.currentStateInfo;
			if (internalStateInfo == null)
			{
				BWLog.Info("Unable to get state info for " + fromState);
				return false;
			}
			if (fromState == CharacterState.Idle && this.sideAnim != -1)
			{
				return desiredState == CharacterState.GetUp;
			}
			return internalStateInfo.allowedTransitions.Contains(desiredState);
		}

		// Token: 0x06001F01 RID: 7937 RVA: 0x000DF498 File Offset: 0x000DD898
		private bool CurrentStateHasTag(string tag)
		{
			return this.currentStateInfo != null && this.currentStateInfo.tags.Contains(tag.ToLower());
		}

		// Token: 0x06001F02 RID: 7938 RVA: 0x000DF4C0 File Offset: 0x000DD8C0
		public bool IsChangingState()
		{
			CharacterState characterState = this.currentState;
			return characterState == CharacterState.CrawlEnter || characterState == CharacterState.CrawlExit;
		}

		// Token: 0x06001F03 RID: 7939 RVA: 0x000DF4EB File Offset: 0x000DD8EB
		public bool IsSwimming()
		{
			return this.currentState == CharacterState.SwimIn || this.currentState == CharacterState.SwimIdle || this.currentState == CharacterState.Swim || this.currentState == CharacterState.SwimOut;
		}

		// Token: 0x06001F04 RID: 7940 RVA: 0x000DF521 File Offset: 0x000DD921
		public bool IsSitting()
		{
			return this.currentState == CharacterState.SitDown || this.currentState == CharacterState.Sitting;
		}

		// Token: 0x06001F05 RID: 7941 RVA: 0x000DF53D File Offset: 0x000DD93D
		public bool IsImmobile()
		{
			return this.CurrentStateHasTag("Immobile");
		}

		// Token: 0x06001F06 RID: 7942 RVA: 0x000DF54A File Offset: 0x000DD94A
		public bool InAttack()
		{
			return this.InStandingAttack() || this.upperBody.InAttackState();
		}

		// Token: 0x06001F07 RID: 7943 RVA: 0x000DF568 File Offset: 0x000DD968
		public bool InStandingAttack()
		{
			return this.currentState == CharacterState.SwordLungeRight || this.currentState == CharacterState.SwordLungeLeft || this.currentState == CharacterState.SwordJumpAttack || this.currentState == CharacterState.KickFrontLeft || this.currentState == CharacterState.KickFrontRight;
		}

		// Token: 0x06001F08 RID: 7944 RVA: 0x000DF5B6 File Offset: 0x000DD9B6
		public bool IsCrawling()
		{
			return this.currentState == CharacterState.CrawlIdle || this.currentState == CharacterState.Crawl;
		}

		// Token: 0x06001F09 RID: 7945 RVA: 0x000DF5D0 File Offset: 0x000DD9D0
		public bool IsDodging()
		{
			return this.currentState == CharacterState.DodgeLeft || this.currentState == CharacterState.DodgeRight;
		}

		// Token: 0x06001F0A RID: 7946 RVA: 0x000DF5EC File Offset: 0x000DD9EC
		public bool IsWalking()
		{
			return this.currentState == CharacterState.Walk;
		}

		// Token: 0x06001F0B RID: 7947 RVA: 0x000DF5F7 File Offset: 0x000DD9F7
		public bool IsProne()
		{
			return this.currentState == CharacterState.Prone;
		}

		// Token: 0x06001F0C RID: 7948 RVA: 0x000DF603 File Offset: 0x000DDA03
		public void StartJump(float jumpForce)
		{
			this.desiredJumpForce = jumpForce;
			this.InterruptState(CharacterState.Jump, true);
		}

		// Token: 0x06001F0D RID: 7949 RVA: 0x000DF616 File Offset: 0x000DDA16
		public void StartFalling()
		{
			this.desiredJumpForce = -1f;
			this.InterruptState(CharacterState.Jump, true);
		}

		// Token: 0x06001F0E RID: 7950 RVA: 0x000DF62D File Offset: 0x000DDA2D
		public bool CanStartNewStandingAttack()
		{
			return this.targetObject.IsOnGround() && this.CanStartNewAttack();
		}

		// Token: 0x06001F0F RID: 7951 RVA: 0x000DF648 File Offset: 0x000DDA48
		public bool CanStartNewAttack()
		{
			if (this.sideAnim != -1 || this.IsImmobile() || this.IsImpactState())
			{
				return false;
			}
			if (!this.upperBody.CanStartNewAttack())
			{
				return false;
			}
			if (!this.InStandingAttack())
			{
				return !this.IsDodging();
			}
			MeleeAttackState attackState = ((CharacterAttackState)this.currentFunctions).attackState;
			if (attackState == null || this.stateTime < 0.15f)
			{
				return false;
			}
			float normalizedTime = this.targetController.GetCurrentAnimatorStateInfo(this.GetAnimatorLayer()).normalizedTime;
			if (this.IsPlayingAnimation(attackState.recoilAnimation))
			{
				return normalizedTime >= 1f;
			}
			return normalizedTime > attackState.interruptNormalizedTime;
		}

		// Token: 0x06001F10 RID: 7952 RVA: 0x000DF710 File Offset: 0x000DDB10
		public bool CanDoubleJump()
		{
			if (this.currentState != CharacterState.Jump || this.stateTime < 0.25f)
			{
				return false;
			}
			CharacterJumpState characterJumpState = (CharacterJumpState)this.currentFunctions;
			bool result = characterJumpState.CanAddJumpForce(this) && !this.hasDoubleJumped;
			this.allowDouble = result;
			return result;
		}

		// Token: 0x06001F11 RID: 7953 RVA: 0x000DF76C File Offset: 0x000DDB6C
		public void HasDoubleJumped()
		{
			if (this.currentState != CharacterState.Jump)
			{
				return;
			}
			CharacterJumpState characterJumpState = (CharacterJumpState)this.currentFunctions;
			characterJumpState.DoubleJump(this);
		}

		// Token: 0x06001F12 RID: 7954 RVA: 0x000DF79A File Offset: 0x000DDB9A
		public bool CanJump()
		{
			return this.CanChangeTo(CharacterState.Jump, CharacterState.None) && this.targetObject != null && !this.targetObject.isHovering;
		}

		// Token: 0x06001F13 RID: 7955 RVA: 0x000DF7C6 File Offset: 0x000DDBC6
		public bool IsJumping()
		{
			return this.currentState == CharacterState.Jump;
		}

		// Token: 0x06001F14 RID: 7956 RVA: 0x000DF7D4 File Offset: 0x000DDBD4
		public bool DoubleJump(float power)
		{
			if (this.currentState != CharacterState.Jump)
			{
				return false;
			}
			CharacterJumpState characterJumpState = (CharacterJumpState)this.currentFunctions;
			if (this.hasDoubleJumped || !this.allowDouble)
			{
				return false;
			}
			characterJumpState.DoubleJump(this);
			return true;
		}

		// Token: 0x06001F15 RID: 7957 RVA: 0x000DF81C File Offset: 0x000DDC1C
		public bool IsHover()
		{
			return this.currentState == CharacterState.Hover;
		}

		// Token: 0x06001F16 RID: 7958 RVA: 0x000DF828 File Offset: 0x000DDC28
		public bool IsImpactState()
		{
			return this.currentState == CharacterState.SoftHitLeft || this.currentState == CharacterState.SoftHitRight || this.currentState == CharacterState.SoftHitBack || this.currentState == CharacterState.SoftHitFront || this.currentState == CharacterState.SoftHitTop || this.currentState == CharacterState.ImpactLeft || this.currentState == CharacterState.ImpactRight;
		}

		// Token: 0x06001F17 RID: 7959 RVA: 0x000DF890 File Offset: 0x000DDC90
		public void SetSideAnimRotation(Quaternion rot)
		{
			this.sideAnimRotation = rot;
		}

		// Token: 0x06001F18 RID: 7960 RVA: 0x000DF899 File Offset: 0x000DDC99
		public Quaternion GetSideAnimRotation()
		{
			return this.sideAnimRotation;
		}

		// Token: 0x06001F19 RID: 7961 RVA: 0x000DF8A1 File Offset: 0x000DDCA1
		public bool IsOnSide()
		{
			return this.sideAnim != -1;
		}

		// Token: 0x06001F1A RID: 7962 RVA: 0x000DF8AF File Offset: 0x000DDCAF
		public bool IsGetUpState()
		{
			return this.currentState == CharacterState.GetUp;
		}

		// Token: 0x06001F1B RID: 7963 RVA: 0x000DF8BC File Offset: 0x000DDCBC
		public bool ApplyDiveForce(float force)
		{
			if (!this.IsSwimming())
			{
				return false;
			}
			this.targetObject.walkController.legsRb.AddForceAtPosition(-80f * force * Vector3.up, this.targetObject.walkController.legsRb.worldCenterOfMass);
			return true;
		}

		// Token: 0x06001F1C RID: 7964 RVA: 0x000DF912 File Offset: 0x000DDD12
		public void SetSpeedForce(float force = 1f)
		{
			this.speedForceModifier = force;
		}

		// Token: 0x06001F1D RID: 7965 RVA: 0x000DF91C File Offset: 0x000DDD1C
		public float GetSpeedForceModifier()
		{
			if (this.currentState == CharacterState.GetUp || this.currentState == CharacterState.Balance)
			{
				return 0.1f;
			}
			if (this.currentState == CharacterState.SoftHitLeft || this.currentState == CharacterState.SoftHitRight || this.currentState == CharacterState.SoftHitFront || this.currentState == CharacterState.SoftHitBack)
			{
				return 0.1f;
			}
			if (this.currentState == CharacterState.Walk)
			{
				CharacterWalkState characterWalkState = (CharacterWalkState)this.currentFunctions;
				if (this.deliberateWalk && this.desiredGoto.magnitude < 0.001f)
				{
					return 0.5f * this.speedForceModifier;
				}
				if (this.walkStrafe || this.desiredGoto.z < 0f)
				{
					return 1f * this.speedForceModifier;
				}
				return this.speedForceModifier * Mathf.Max(0.05f, Vector3.Dot(this.desiredGoto.normalized, Vector3.forward));
			}
			else
			{
				if (this.currentState == CharacterState.Jump)
				{
					if (this.currentJumpState == CharacterJumpState.JumpState.Land)
					{
					}
					return 1f;
				}
				return this.speedForceModifier;
			}
		}

		// Token: 0x06001F1E RID: 7966 RVA: 0x000DFA40 File Offset: 0x000DDE40
		public bool QueueState(CharacterState desiredState)
		{
			if (this.currentState == desiredState)
			{
				return true;
			}
			if (this.desiredStateQueue.Contains(desiredState))
			{
				return true;
			}
			if (!this.CanChangeTo(desiredState, CharacterState.None))
			{
				BWLog.Info(string.Concat(new object[]
				{
					"Unable to shift from ",
					this.currentState,
					" to ",
					desiredState
				}));
				return false;
			}
			this.desiredStateQueue.Enqueue(desiredState);
			if (this.desiredStateQueue.Count > 3)
			{
				BWLog.Info(string.Concat(new object[]
				{
					"Somehow queued ",
					this.desiredStateQueue.Count,
					" items for ",
					this.targetObject.go.name
				}));
			}
			return true;
		}

		// Token: 0x06001F1F RID: 7967 RVA: 0x000DFB18 File Offset: 0x000DDF18
		public void ClearQueue()
		{
			this.desiredStateQueue.Clear();
		}

		// Token: 0x06001F20 RID: 7968 RVA: 0x000DFB25 File Offset: 0x000DDF25
		public bool InterruptQueue(CharacterState desiredState)
		{
			if (!this.CanChangeTo(desiredState, CharacterState.None))
			{
				return false;
			}
			this.ClearQueue();
			this.desiredStateQueue.Enqueue(desiredState);
			return true;
		}

		// Token: 0x06001F21 RID: 7969 RVA: 0x000DFB49 File Offset: 0x000DDF49
		public bool InterruptState(CharacterState desiredState, bool noBlend = true)
		{
			if (!this.CanChangeTo(desiredState, CharacterState.None))
			{
				return false;
			}
			this.ClearQueue();
			this.EnterState(desiredState, noBlend);
			return true;
		}

		// Token: 0x06001F22 RID: 7970 RVA: 0x000DFB6C File Offset: 0x000DDF6C
		public float CurrentAnimationNormalizedTime()
		{
			return this.targetController.GetCurrentAnimatorStateInfo(0).normalizedTime;
		}

		// Token: 0x06001F23 RID: 7971 RVA: 0x000DFB90 File Offset: 0x000DDF90
		public void PlayAnim(string anim)
		{
			if (this.currentState == CharacterState.PlayAnim)
			{
				this.playAnimCurrent = anim;
				this.animationHash = this.PlayAnimation(this.playAnimCurrent, false);
				this.stateTime = 0f;
				this.playAnimFinished = false;
			}
			else
			{
				this.playAnimCurrent = anim;
				this.InterruptState(CharacterState.PlayAnim, true);
			}
		}

		// Token: 0x06001F24 RID: 7972 RVA: 0x000DFBEC File Offset: 0x000DDFEC
		public void ClearAnimation()
		{
			this.playAnimCurrent = null;
			this.animationHash = -1;
		}

		// Token: 0x06001F25 RID: 7973 RVA: 0x000DFBFC File Offset: 0x000DDFFC
		public void DebugPlayAnim(string anim)
		{
			if (this.currentState == CharacterState.PlayAnim)
			{
				return;
			}
			List<string> list = new List<string>(anim.Split(new char[]
			{
				'\n'
			}));
			this.playAnimCurrent = list[0];
			this.InterruptState(CharacterState.PlayAnim, true);
		}

		// Token: 0x06001F26 RID: 7974 RVA: 0x000DFC48 File Offset: 0x000DE048
		private void SetAnimatorParametersForRole(CharacterRole role)
		{
			string[] array = new string[]
			{
				"DefaultRole",
				"SkeletonRole",
				"SneakyRole",
				"PirateRole"
			};
			string b = string.Empty;
			if (role != CharacterRole.Sneaky)
			{
				if (role != CharacterRole.Pirate)
				{
					if (role != CharacterRole.Skeleton)
					{
						b = "DefaultRole";
					}
					else
					{
						b = "SkeletonRole";
					}
				}
				else
				{
					b = "PirateRole";
				}
			}
			else
			{
				b = "SneakyRole";
			}
			foreach (string text in array)
			{
				this.targetController.SetFloat(text, (!(text == b)) ? 0f : 1f);
			}
		}

		// Token: 0x06001F27 RID: 7975 RVA: 0x000DFD10 File Offset: 0x000DE110
		public void SetAnimatorParametersForBodyParts(HashSet<string> bodyPartStrs)
		{
			string[] array = new string[]
			{
				"LegOverride_Default",
				"LegOverride_PegLegRight",
				"LegOverride_PegLegLeft"
			};
			string b = "LegOverride_Default";
			if (bodyPartStrs.Contains("Limb Peg Leg Left"))
			{
				b = "LegOverride_PegLegLeft";
			}
			if (bodyPartStrs.Contains("Limb Peg Leg Right"))
			{
				b = "LegOverride_PegLegRight";
			}
			foreach (string text in array)
			{
				this.targetController.SetFloat(text, (!(text == b)) ? 0f : 1f);
			}
		}

		// Token: 0x06001F28 RID: 7976 RVA: 0x000DFDB4 File Offset: 0x000DE1B4
		public void SetRole(CharacterRole role)
		{
			if (role == this.currentRole)
			{
				return;
			}
			this.SetAnimatorParametersForRole(role);
			this.currentRole = role;
			this.upperBody.SetRole(role);
			if (this.currentState == CharacterState.None)
			{
				this.currentFunctions = null;
				return;
			}
			this.currentStateInfo = this.GetStateInfo(CharacterState.None, CharacterRole.None);
			if (this.currentStateInfo == null)
			{
				BWLog.Error(string.Concat(new object[]
				{
					"Unable to find state info for ",
					this.currentState,
					" in role ",
					this.currentRole
				}));
				return;
			}
			this.currentFunctions = this.currentStateInfo.stateFunctions;
		}

		// Token: 0x06001F29 RID: 7977 RVA: 0x000DFE63 File Offset: 0x000DE263
		public void StandingAttack(CharacterState attackState)
		{
			if (this.CanStartNewStandingAttack())
			{
				this.EnterState(attackState, false);
			}
		}

		// Token: 0x06001F2A RID: 7978 RVA: 0x000DFE78 File Offset: 0x000DE278
		public void Attack(UpperBodyState attackState)
		{
			this.upperBody.Attack(attackState);
		}

		// Token: 0x06001F2B RID: 7979 RVA: 0x000DFE86 File Offset: 0x000DE286
		public void Shield(UpperBodyState shieldState)
		{
			this.upperBody.Shield(shieldState);
		}

		// Token: 0x06001F2C RID: 7980 RVA: 0x000DFE94 File Offset: 0x000DE294
		public void QueueHitReact(Vector3 hitDirection)
		{
			if (!this.targetObject.broken)
			{
				Vector3 rhs = Vector3.Cross(hitDirection.normalized, this.targetObject.goT.up);
				float num = Vector3.Dot(this.targetObject.goT.forward, hitDirection.normalized);
				float num2 = Vector3.Dot(this.targetObject.goT.right, rhs);
				bool flag = Mathf.Abs(num) > 0.8f * Mathf.Abs(num2);
				if (flag)
				{
					this.queuedHitReactState = ((num >= 0f) ? CharacterState.SoftHitBack : CharacterState.SoftHitFront);
				}
				else
				{
					this.queuedHitReactState = ((num2 >= 0f) ? CharacterState.SoftHitRight : CharacterState.SoftHitLeft);
				}
			}
		}

		// Token: 0x06001F2D RID: 7981 RVA: 0x000DFF56 File Offset: 0x000DE356
		public void PlayQueuedHitReact()
		{
			if (this.queuedHitReactState != CharacterState.None)
			{
				this.InterruptState(this.queuedHitReactState, true);
				this.queuedHitReactState = CharacterState.None;
			}
		}

		// Token: 0x06001F2E RID: 7982 RVA: 0x000DFF78 File Offset: 0x000DE378
		private void EnterState(CharacterState desiredState, bool interrupt = false)
		{
			CharacterState characterState = this.currentState;
			bool flag = this.IsSwimming();
			bool flag2 = this.currentState == CharacterState.PlayAnim;
			if (this.currentFunctions != null)
			{
				this.ExitState();
			}
			this.desiredAnimSpeed = 1f;
			this.transitionAnim = -1;
			this.stateTime = 0f;
			this.blendStart = 0f;
			this.sideAnim = -1;
			this.isTransitioning = true;
			this.currentState = desiredState;
			this.currentStateInfo = this.GetStateInfo(CharacterState.None, CharacterRole.None);
			this.upperBody.PlayOnFullBody(this.currentState == CharacterState.Idle);
			if (this.currentStateInfo == null)
			{
				BWLog.Error(string.Concat(new object[]
				{
					"Unable to find state info for ",
					this.currentState,
					" in role ",
					this.currentRole
				}));
				return;
			}
			this.currentFunctions = this.currentStateInfo.stateFunctions;
			this.stateBlend = this.currentStateInfo.transitionBlend;
			if (characterState == CharacterState.Balance)
			{
				this.stateBlend = ((desiredState != CharacterState.Idle || this.targetObject.goT.up.y < 0.85f) ? 0.08f : 1f);
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
				this.stateBlend = ((characterState != CharacterState.Swim) ? 0.025f : 0.1f);
			}
			if (flag2 && desiredState == CharacterState.Idle)
			{
				this.stateBlend = 0.2f;
			}
			if (desiredState == CharacterState.Collapse)
			{
				Collider component = this.targetObject.go.GetComponent<Collider>();
				if (component != null)
				{
					component.enabled = false;
				}
				this.targetObject.Freeze(true);
			}
			this.SetSpeedForce(1f);
			if (this.targetObject.unmoving && this.currentState == CharacterState.Sitting)
			{
				this.PlayAnimation("SitIdle", interrupt);
			}
			this.queuedHitReactState = CharacterState.None;
			if (this.currentState == CharacterState.Unmoving || this.currentState == CharacterState.Treasure)
			{
				this.PlayAnimation("SitIdle", interrupt);
				return;
			}
			if (this.currentFunctions == null)
			{
				BWLog.Warning("Entering state " + this.currentState + ", which has no class to support it");
				return;
			}
			int num = this.currentStateInfo.transitionAnim;
			if (num != -1 && !interrupt)
			{
				this.transitionAnim = num;
				this.PlayAnimation(num, false);
			}
			else
			{
				this.currentFunctions.Enter(this, interrupt);
			}
			if (!this.targetObject.playCallouts && this.IsImpactState())
			{
				this.targetObject.PlayOuchSound(1f, true);
			}
		}

		// Token: 0x06001F2F RID: 7983 RVA: 0x000E0280 File Offset: 0x000DE680
		private void ExitState()
		{
			if (this.currentFunctions == null)
			{
				return;
			}
			this.currentFunctions.Exit(this);
			if (this.currentState == CharacterState.Recover)
			{
				Collider component = this.targetObject.go.GetComponent<Collider>();
				if (component != null)
				{
					component.enabled = true;
				}
				this.targetObject.Unfreeze();
			}
			CharacterState desiredState = this.currentFunctions.desiredState;
			this.currentFunctions = null;
			if (this.desiredStateQueue.Count > 0)
			{
				this.currentState = this.desiredStateQueue.Dequeue();
			}
			else if (desiredState != CharacterState.None)
			{
				this.currentState = desiredState;
			}
			else
			{
				this.currentState = CharacterState.None;
			}
		}

		// Token: 0x06001F30 RID: 7984 RVA: 0x000E0334 File Offset: 0x000DE734
		public void StartPull()
		{
			this.isPulling = true;
			this.targetObject.walkController.StartPull();
		}

		// Token: 0x06001F31 RID: 7985 RVA: 0x000E034D File Offset: 0x000DE74D
		public void StopPull()
		{
			this.isPulling = false;
			this.targetObject.walkController.StopPull();
			if (this.targetObject.IsOnGround() && this.currentState == CharacterState.Balance)
			{
				this.stateTime = 0f;
			}
		}

		// Token: 0x06001F32 RID: 7986 RVA: 0x000E038D File Offset: 0x000DE78D
		public bool IsPulling()
		{
			return this.isPulling;
		}

		// Token: 0x06001F33 RID: 7987 RVA: 0x000E0395 File Offset: 0x000DE795
		public bool WasPulling()
		{
			return this.targetObject.walkController.WasPulled();
		}

		// Token: 0x06001F34 RID: 7988 RVA: 0x000E03A7 File Offset: 0x000DE7A7
		public void Freeze()
		{
			this.targetController.speed = 0f;
		}

		// Token: 0x06001F35 RID: 7989 RVA: 0x000E03B9 File Offset: 0x000DE7B9
		public void Unfreeze()
		{
			this.targetController.speed = 1f;
		}

		// Token: 0x06001F36 RID: 7990 RVA: 0x000E03CC File Offset: 0x000DE7CC
		public void Update()
		{
			if (this.currentStateInfo == null || this.targetObject == null || this.targetController == null)
			{
				return;
			}
			this.stateTime += Time.fixedDeltaTime;
			if (Blocksworld.CurrentState != State.Play)
			{
				List<Block> list = ConnectednessGraph.ConnectedComponent(this.targetObject, 1, null, true);
				bool flag = Blocksworld.selectedBlock == this.targetObject;
				if (Blocksworld.selectedBunch != null)
				{
					flag |= Blocksworld.selectedBunch.ContainsBlock(this.targetObject);
				}
				bool flag2 = this.targetObject.attachedHeadBlocks != null && this.targetObject.attachedHeadBlocks.Count > 0;
				bool flag3 = !this.preventLook && !flag2 && list.Count <= 1 && this.targetObject.goT.up.y > 0.9f && this.targetObject.IsOnGround() && !flag;
				if (this.currentState == CharacterState.EditSitting && !flag3)
				{
					this.InterruptState(CharacterState.EditSittingStill, true);
				}
				if (this.currentState == CharacterState.EditSittingStill && flag3)
				{
					this.EnterState(CharacterState.EditSitting, false);
				}
				if (this.currentFunctions != null)
				{
					this.currentFunctions.Update(this);
				}
				return;
			}
			if (this.rb == null || this.targetController.speed == 0f)
			{
				return;
			}
			bool flag4 = this.targetObject.IsOnGround();
			if (this.startPlay && this.stateTime >= 0.05f)
			{
				this.startPlay = false;
				if (flag4 && this.sideAnim == -1)
				{
					this.rb.AddForce(1000f * this.targetObject.goT.up);
				}
			}
			this.isTransitioning = false;
			this.desiredGoto.y = 0f;
			this.isTransitioning = this.targetController.IsInTransition(0);
			bool flag5 = this.IsImpactState();
			this.rootOffset = Vector3.Lerp(this.rootOffset, this.currentFunctions.GetOffset(this), 0.2f);
			if (this.targetObject != null && !this.targetObject.unmoving)
			{
				this.targetRig.transform.localPosition = this.rootOffset - 0.3f * this.targetObject.goT.up;
			}
			this.actualAnimSpeed = Mathf.Lerp(this.actualAnimSpeed, this.currentStateInfo.animationRate * this.desiredAnimSpeed, 0.1f);
			this.targetController.SetFloat("BaseLayerAnimationSpeed", this.actualAnimSpeed);
			this.currentVelocity = this.targetObject.goT.InverseTransformDirection(this.rb.velocity);
			this.currentVelocity.y = 0f;
			float magnitude = this.currentVelocity.magnitude;
			if (magnitude > 0.05f && this.targetObject.IsOnGround() && this.currentState != CharacterState.Balance && this.isPulling && this.targetObject.goT.up.y > 0.9f && !this.isTransitioning)
			{
				this.InterruptState(CharacterState.Balance, false);
			}
			this.wasDesiringMove = this.desiresMove;
			this.desiresMove = (this.requestedMoveVelocity.sqrMagnitude > Mathf.Epsilon || Mathf.Abs(this.turnPower) > 0f || (this.targetObject.walkController.previousVicinityMode == WalkControllerAnimated.VicinityMode.AvoidTag && magnitude > 0.085f));
			if (!flag5 && !this.desiresMove && !this.wasDesiringMove && magnitude > 10f * Mathf.Max(0.1f, this.currentVelMag))
			{
				if (Mathf.Abs(this.currentVelocity.x) > Mathf.Abs(this.currentVelocity.z))
				{
					float num = 6f;
					if (this.currentVelocity.x > 0f)
					{
						if (this.currentVelocity.x > num)
						{
							this.InterruptState(CharacterState.ImpactLeft, true);
						}
						else
						{
							this.InterruptState(CharacterState.SoftHitLeft, true);
						}
					}
					else if (this.currentVelocity.x < -num)
					{
						this.InterruptState(CharacterState.ImpactRight, true);
					}
					else
					{
						this.InterruptState(CharacterState.SoftHitRight, true);
					}
				}
				else if (this.currentVelocity.z > 0f)
				{
					this.InterruptState(CharacterState.SoftHitBack, true);
				}
				else
				{
					this.InterruptState(CharacterState.SoftHitFront, true);
				}
			}
			this.currentVelMag = magnitude;
			if (this.targetCape != null || this.targetJetpack != null)
			{
				if (!flag4 && this.targetObject.isHovering && this.currentState != CharacterState.Hover)
				{
					this.InterruptState(CharacterState.Hover, true);
				}
				else if (flag4 && this.currentState == CharacterState.Hover)
				{
					this.InterruptState(CharacterState.Idle, true);
				}
				if (this.targetCape != null)
				{
					if (this.desiresMove)
					{
						this.targetCape.rotation = Quaternion.Slerp(this.targetCape.rotation, this.capeFlightRotation, 0.1f);
					}
					else
					{
						this.targetCape.rotation = Quaternion.Slerp(this.targetCape.rotation, this.capeHoverRotation, 0.1f);
					}
				}
			}
			if (this.currentFunctions == null)
			{
				this.turnPower = 0f;
				return;
			}
			this.offset = -this.targetObject.go.transform.InverseTransformPoint(this.lastPos);
			this.lastPos = this.targetObject.go.transform.position;
			bool flag6 = BlockWater.BlockWithinWater(this.targetObject, true);
			if (this.currentState != CharacterState.Flail && this.isPulling && !this.targetObject.unmoving && !flag4 && !this.isTransitioning && this.currentState != CharacterState.Jump && !flag6 && this.currentState != CharacterState.Hover)
			{
				this.InterruptState(CharacterState.Flail, false);
			}
			if (this.transitionAnim != -1)
			{
				if (this.transitionAnim != this.targetController.GetCurrentAnimatorStateInfo(0).shortNameHash)
				{
					this.transitionAnim = -1;
					this.currentFunctions.Enter(this, true);
				}
				this.turnPower = 0f;
				return;
			}
			if (!flag5)
			{
				bool flag7 = this.IsSwimming();
				if (flag7 && this.targetObject.goT.up.y < 0.99f)
				{
					if (this.targetObject.goT.up.y < -0.97f)
					{
						this.rb.AddForceAtPosition(5f * Vector3.forward, this.targetObject.goT.position + 0.5f * this.targetObject.goT.up);
					}
					this.rb.AddForceAtPosition(45f * Vector3.up, this.targetObject.goT.position + 0.3f * this.targetObject.goT.up);
					this.rb.AddForceAtPosition(-45f * Vector3.up, this.targetObject.goT.position - 0.3f * this.targetObject.goT.up);
				}
				if (flag6 && !flag7)
				{
					bool flag8 = true;
					if (this.IsJumping())
					{
						CharacterJumpState characterJumpState = (CharacterJumpState)this.currentFunctions;
						flag8 = !characterJumpState.HeadingUp(this);
					}
					if (flag8)
					{
						this.InterruptState(CharacterState.SwimIn, false);
					}
				}
				else if (flag7 && !flag6 && this.stateTime > 0.75f)
				{
					this.InterruptState(CharacterState.SwimOut, false);
				}
			}
			if (this.currentState == CharacterState.PlayAnim && (this.currentVelMag > 0.05f || this.rootOffset.sqrMagnitude > 0.05f))
			{
				this.InterruptState(CharacterState.Idle, true);
			}
			switch (this.currentState)
			{
			case CharacterState.Idle:
				if (this.desiresMove)
				{
					if (this.IsOnSide())
					{
						this.InterruptState(CharacterState.GetUp, true);
					}
					else
					{
						this.InterruptState(CharacterState.Walk, this.isTransitioning);
					}
				}
				else if (this.rb.velocity.y < -0.1f && !flag4 && this.targetCape == null && this.targetJetpack == null)
				{
					this.StartFalling();
				}
				break;
			case CharacterState.Walk:
			{
				float num2 = this.targetObject.NearGround(0.3f);
				if ((num2 > 0.2f || num2 < 0f) && this.targetCape == null && !this.IsHover() && this.rb.velocity.y < -5f)
				{
					this.StartFalling();
				}
				break;
			}
			case CharacterState.Balance:
				if (!this.isPulling && !this.isTransitioning && this.stateTime > 0.15f)
				{
					if (flag4)
					{
						this.InterruptState(CharacterState.Idle, true);
					}
					else
					{
						this.InterruptState(CharacterState.Jump, true);
					}
				}
				break;
			case CharacterState.CrawlIdle:
				if (Mathf.Abs(this.offset.z) > 0.01f)
				{
					this.EnterState(CharacterState.Crawl, false);
				}
				break;
			case CharacterState.Flail:
				if (!this.isPulling)
				{
					if (this.targetObject.NearGround(0.2f) > 0f)
					{
						this.InterruptState(CharacterState.Idle, true);
					}
					else if (flag6)
					{
						this.InterruptState(CharacterState.SwimIdle, true);
					}
				}
				break;
			case CharacterState.SwimIdle:
				if (Mathf.Abs(this.offset.z) > 0.04f)
				{
					this.EnterState(CharacterState.Swim, false);
				}
				break;
			case CharacterState.Swim:
				if (Mathf.Abs(this.offset.z) < 0.04f)
				{
					this.EnterState(CharacterState.SwimIdle, false);
				}
				break;
			}
			if (!this.currentFunctions.Update(this))
			{
				if (this.desiredStateQueue.Count > 0)
				{
					CharacterState desiredState = this.desiredStateQueue.Dequeue();
					this.EnterState(desiredState, false);
				}
				else if (this.currentState == CharacterState.Jump && this.desiredGoto.magnitude > 0.1f && this.targetObject.goT.up.y > 0.85f)
				{
					this.InterruptState(CharacterState.Walk, true);
				}
				else
				{
					this.EnterState(this.currentFunctions.desiredState, false);
				}
			}
			this.turnPower = 0f;
			this.upperBody.Update();
			this.combatController.Update();
			if (this.requestingPlayAnim)
			{
				this.requestingPlayAnim = false;
			}
		}

		// Token: 0x06001F37 RID: 7991 RVA: 0x000E0FA0 File Offset: 0x000DF3A0
		public int PlayAnimation(int hash, bool interrupt = false)
		{
			this.animationHash = hash;
			this.playingAnim = "Hash " + this.animationHash;
			if (null == this.targetController)
			{
				BWLog.Warning(string.Concat(new object[]
				{
					"No anim controller for block ",
					(this.targetObject == null) ? "null" : this.targetObject.go.name,
					" attempting to play ",
					this.animationHash
				}));
				return 0;
			}
			float transitionDuration = this.stateBlend;
			if (this.targetController.IsInTransition(0) && !this.IsImpactState() && Blocksworld.CurrentState == State.Play)
			{
				transitionDuration = Mathf.Max(this.stateBlend, this.stateTime - this.blendStart + 0.0333333351f);
			}
			this.blendStart = this.stateTime;
			if (this.stateBlend > 0f && !interrupt)
			{
				this.isTransitioning = true;
				this.targetController.CrossFade(this.animationHash, transitionDuration, -1, 0f);
			}
			else if (this.targetController.GetCurrentAnimatorStateInfo(0).normalizedTime > 1f)
			{
				this.targetController.Play(this.animationHash, 0, 0f);
			}
			else
			{
				this.targetController.Play(this.animationHash);
			}
			return this.animationHash;
		}

		// Token: 0x06001F38 RID: 7992 RVA: 0x000E111C File Offset: 0x000DF51C
		public int ShiftAnimation(string animation)
		{
			this.ShiftAnimation(Animator.StringToHash(animation));
			this.playingAnim = animation;
			return this.animationHash;
		}

		// Token: 0x06001F39 RID: 7993 RVA: 0x000E1138 File Offset: 0x000DF538
		public int ShiftAnimation(int hash)
		{
			this.animationHash = hash;
			this.playingAnim = "Hash " + hash;
			if (null == this.targetController)
			{
				BWLog.Warning(string.Concat(new object[]
				{
					"No anim controller for block ",
					(this.targetObject == null) ? "null" : this.targetObject.go.name,
					" attempting to play ",
					this.animationHash
				}));
				return 0;
			}
			float transitionDuration = this.stateBlend;
			if (this.targetController.IsInTransition(0) && !this.IsImpactState() && Blocksworld.CurrentState == State.Play)
			{
				transitionDuration = Mathf.Max(this.stateBlend, this.stateTime - this.blendStart + 0.0333333351f);
			}
			this.blendStart = this.stateTime;
			AnimatorStateInfo currentAnimatorStateInfo = this.targetController.GetCurrentAnimatorStateInfo(0);
			this.targetController.CrossFade(this.animationHash, transitionDuration, -1, currentAnimatorStateInfo.normalizedTime % 1f);
			return this.animationHash;
		}

		// Token: 0x06001F3A RID: 7994 RVA: 0x000E1257 File Offset: 0x000DF657
		public int StartAnimationAt(string animation, float percent)
		{
			this.StartAnimationAt(Animator.StringToHash(animation), percent);
			this.playingAnim = animation;
			return this.animationHash;
		}

		// Token: 0x06001F3B RID: 7995 RVA: 0x000E1274 File Offset: 0x000DF674
		public int StartAnimationAt(int hash, float percent)
		{
			this.animationHash = hash;
			this.playingAnim = "Hash " + hash;
			if (null == this.targetController)
			{
				BWLog.Warning(string.Concat(new object[]
				{
					"No anim controller for block ",
					(this.targetObject == null) ? "null" : this.targetObject.go.name,
					" attempting to play ",
					this.animationHash
				}));
				return 0;
			}
			float transitionDuration = this.stateBlend;
			if (this.targetController.IsInTransition(0) && !this.IsImpactState() && Blocksworld.CurrentState == State.Play)
			{
				transitionDuration = Mathf.Max(this.stateBlend, this.stateTime - this.blendStart + 0.0333333351f);
			}
			this.blendStart = this.stateTime;
			AnimatorStateInfo currentAnimatorStateInfo = this.targetController.GetCurrentAnimatorStateInfo(0);
			this.targetController.CrossFade(this.animationHash, transitionDuration, -1, percent * currentAnimatorStateInfo.normalizedTime);
			return this.animationHash;
		}

		// Token: 0x06001F3C RID: 7996 RVA: 0x000E138F File Offset: 0x000DF78F
		public void OnScreenDebug()
		{
		}

		// Token: 0x06001F3D RID: 7997 RVA: 0x000E1394 File Offset: 0x000DF794
		public bool HitByHandAttachment(Block block)
		{
			Block rightHandAttachment = this.combatController.GetRightHandAttachment();
			Block leftHandAttachment = this.combatController.GetLeftHandAttachment();
			return this.combatController.IsHitByBlockThisFrame(block, rightHandAttachment) || this.combatController.IsHitByBlockThisFrame(block, leftHandAttachment);
		}

		// Token: 0x06001F3E RID: 7998 RVA: 0x000E13DB File Offset: 0x000DF7DB
		public bool HitByFoot(Block block)
		{
			return this.combatController.IsHitByFootThisFrame(block);
		}

		// Token: 0x06001F3F RID: 7999 RVA: 0x000E13EC File Offset: 0x000DF7EC
		public bool HitModelByFoot(Block hitBlock)
		{
			HashSet<Block> hashSet = this.combatController.BlocksHitThisFrameByFeet();
			foreach (Block block in hashSet)
			{
				if (hitBlock.modelBlock == block)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001F40 RID: 8000 RVA: 0x000E1460 File Offset: 0x000DF860
		public bool HitByTaggedHandAttachment(Block block, string tag)
		{
			Block rightHandAttachment = this.combatController.GetRightHandAttachment();
			bool flag = rightHandAttachment != null && TagManager.GetBlockTags(rightHandAttachment).Contains(tag);
			if (flag && this.combatController.IsHitByBlockThisFrame(block, rightHandAttachment))
			{
				return true;
			}
			Block leftHandAttachment = this.combatController.GetLeftHandAttachment();
			bool flag2 = leftHandAttachment != null && TagManager.GetBlockTags(leftHandAttachment).Contains(tag);
			return flag2 && this.combatController.IsHitByBlockThisFrame(block, leftHandAttachment);
		}

		// Token: 0x06001F41 RID: 8001 RVA: 0x000E14E8 File Offset: 0x000DF8E8
		public bool HitModelByHandAttachment(Block modelBlock)
		{
			Block rightHandAttachment = this.combatController.GetRightHandAttachment();
			HashSet<Block> hashSet = (rightHandAttachment != null) ? this.combatController.BlocksHitThisFrameByBlock(rightHandAttachment) : null;
			if (hashSet != null)
			{
				foreach (Block block in hashSet)
				{
					if (block.modelBlock == modelBlock)
					{
						return true;
					}
				}
			}
			Block leftHandAttachment = this.combatController.GetLeftHandAttachment();
			HashSet<Block> hashSet2 = (leftHandAttachment != null) ? this.combatController.BlocksHitThisFrameByBlock(leftHandAttachment) : null;
			if (hashSet2 != null)
			{
				foreach (Block block2 in hashSet2)
				{
					if (block2.modelBlock == modelBlock)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06001F42 RID: 8002 RVA: 0x000E1604 File Offset: 0x000DFA04
		public bool HitModelByTaggedHandAttachment(Block modelBlock, string tag)
		{
			Block rightHandAttachment = this.combatController.GetRightHandAttachment();
			if (rightHandAttachment != null)
			{
				List<string> blockTags = TagManager.GetBlockTags(rightHandAttachment);
				if (blockTags.Contains(tag))
				{
					HashSet<Block> hashSet = this.combatController.BlocksHitThisFrameByBlock(rightHandAttachment);
					if (hashSet != null)
					{
						foreach (Block block in hashSet)
						{
							if (block.modelBlock == modelBlock)
							{
								return true;
							}
						}
					}
				}
			}
			Block leftHandAttachment = this.combatController.GetLeftHandAttachment();
			if (leftHandAttachment != null)
			{
				List<string> blockTags2 = TagManager.GetBlockTags(leftHandAttachment);
				if (blockTags2.Contains(tag))
				{
					HashSet<Block> hashSet2 = this.combatController.BlocksHitThisFrameByBlock(leftHandAttachment);
					if (hashSet2 != null)
					{
						foreach (Block block2 in hashSet2)
						{
							if (block2.modelBlock == modelBlock)
							{
								return true;
							}
						}
					}
				}
			}
			return false;
		}

		// Token: 0x06001F43 RID: 8003 RVA: 0x000E173C File Offset: 0x000DFB3C
		public bool FiredAsWeapon(Block block)
		{
			return (block == this.combatController.GetRightHandAttachment() && this.combatController.RightAttachmentFired()) || (block == this.combatController.GetLeftHandAttachment() && this.combatController.LeftAttachmentFired());
		}

		// Token: 0x06001F44 RID: 8004 RVA: 0x000E1790 File Offset: 0x000DFB90
		public void ClearAttackFlags()
		{
			this.combatController.ClearAttackFlags();
		}

		// Token: 0x06001F45 RID: 8005 RVA: 0x000E17A0 File Offset: 0x000DFBA0
		public void SaveAnimatorState()
		{
			AnimatorControllerParameter[] parameters = this.targetController.parameters;
			this.savedAnimatorParameterValues = new Dictionary<int, object>();
			for (int i = 0; i < parameters.Length; i++)
			{
				object obj = null;
				AnimatorControllerParameter animatorControllerParameter = parameters[i];
				AnimatorControllerParameterType type = animatorControllerParameter.type;
				if (type != AnimatorControllerParameterType.Bool)
				{
					if (type != AnimatorControllerParameterType.Int)
					{
						if (type == AnimatorControllerParameterType.Float)
						{
							obj = this.targetController.GetFloat(animatorControllerParameter.nameHash);
						}
					}
					else
					{
						obj = this.targetController.GetInteger(animatorControllerParameter.nameHash);
					}
				}
				else
				{
					obj = this.targetController.GetBool(animatorControllerParameter.nameHash);
				}
				if (obj != null)
				{
					this.savedAnimatorParameterValues.Add(animatorControllerParameter.nameHash, obj);
				}
			}
		}

		// Token: 0x06001F46 RID: 8006 RVA: 0x000E1870 File Offset: 0x000DFC70
		public void RestoreAnimatorState()
		{
			if (this.savedAnimatorParameterValues == null)
			{
				return;
			}
			foreach (AnimatorControllerParameter animatorControllerParameter in this.targetController.parameters)
			{
				object obj;
				if (this.savedAnimatorParameterValues.TryGetValue(animatorControllerParameter.nameHash, out obj))
				{
					AnimatorControllerParameterType type = animatorControllerParameter.type;
					if (type != AnimatorControllerParameterType.Bool)
					{
						if (type != AnimatorControllerParameterType.Int)
						{
							if (type == AnimatorControllerParameterType.Float)
							{
								this.targetController.SetFloat(animatorControllerParameter.nameHash, (float)obj);
							}
						}
						else
						{
							this.targetController.SetInteger(animatorControllerParameter.nameHash, (int)obj);
						}
					}
					else
					{
						this.targetController.SetBool(animatorControllerParameter.nameHash, (bool)obj);
					}
				}
			}
			this.savedAnimatorParameterValues = null;
		}

		// Token: 0x06001F47 RID: 8007 RVA: 0x000E1948 File Offset: 0x000DFD48
		public void Bounce(float bounciness)
		{
			if (this.currentState == CharacterState.Jump)
			{
				this.currentJumpState = CharacterJumpState.JumpState.Up;
				Vector3 bounceVector = this.targetObject.walkController.GetBounceVector(bounciness);
				this.targetObject.walkController.Bounce(bounceVector);
				CharacterJumpState characterJumpState = (CharacterJumpState)this.currentFunctions;
				characterJumpState.Bounce(this);
			}
			this.StartJump(bounciness * 10f);
		}

		// Token: 0x04001931 RID: 6449
		public CharacterRole currentRole;

		// Token: 0x04001932 RID: 6450
		public CharacterState currentState;

		// Token: 0x04001933 RID: 6451
		protected CharacterStateHandler.InternalStateInfo currentStateInfo;

		// Token: 0x04001934 RID: 6452
		public CharacterBaseState currentFunctions;

		// Token: 0x04001935 RID: 6453
		protected int transitionAnim = -1;

		// Token: 0x04001936 RID: 6454
		public float stateBlend = 0.05f;

		// Token: 0x04001937 RID: 6455
		public UpperBodyStateHandler upperBody;

		// Token: 0x04001938 RID: 6456
		public float stateTime;

		// Token: 0x04001939 RID: 6457
		public float timeInAnim;

		// Token: 0x0400193A RID: 6458
		public int sideAnim = -1;

		// Token: 0x0400193B RID: 6459
		public int lastSideAnim = -1;

		// Token: 0x0400193C RID: 6460
		public int lastHoverAnim = -1;

		// Token: 0x0400193D RID: 6461
		public Vector3 lastForward;

		// Token: 0x0400193E RID: 6462
		public Vector3 lastRight;

		// Token: 0x0400193F RID: 6463
		public Vector3 lastUp;

		// Token: 0x04001940 RID: 6464
		protected bool isPulling;

		// Token: 0x04001941 RID: 6465
		public bool isTransitioning;

		// Token: 0x04001942 RID: 6466
		protected BlockAbstractAntiGravityWing targetCape;

		// Token: 0x04001943 RID: 6467
		protected BlockAbstractJetpack targetJetpack;

		// Token: 0x04001944 RID: 6468
		protected Quaternion capeFlightRotation = Quaternion.identity;

		// Token: 0x04001945 RID: 6469
		protected Quaternion capeHoverRotation = Quaternion.identity;

		// Token: 0x04001946 RID: 6470
		public GameObject targetRig;

		// Token: 0x04001947 RID: 6471
		public Rigidbody rb;

		// Token: 0x04001948 RID: 6472
		private Vector3 lastPos = Vector3.zero;

		// Token: 0x04001949 RID: 6473
		public Vector3 offset = Vector3.zero;

		// Token: 0x0400194A RID: 6474
		protected float currentVelMag;

		// Token: 0x0400194B RID: 6475
		public float turnPower;

		// Token: 0x0400194C RID: 6476
		public Vector3 desiredGoto = Vector3.zero;

		// Token: 0x0400194D RID: 6477
		protected Vector3 rootOffset = Vector3.zero;

		// Token: 0x0400194E RID: 6478
		public Vector3 currentVelocity = Vector3.zero;

		// Token: 0x0400194F RID: 6479
		public float speedForceModifier = 1f;

		// Token: 0x04001950 RID: 6480
		public float desiredJumpForce = 1f;

		// Token: 0x04001951 RID: 6481
		public float maxDownSpeedDuringJump;

		// Token: 0x04001952 RID: 6482
		public float dodgeSpeed;

		// Token: 0x04001953 RID: 6483
		public float standingAttackMaxSpeed;

		// Token: 0x04001954 RID: 6484
		public float standingAttackMinSpeed;

		// Token: 0x04001955 RID: 6485
		public Vector3 standingAttackForward;

		// Token: 0x04001956 RID: 6486
		public float desiredAnimSpeed = 1f;

		// Token: 0x04001957 RID: 6487
		protected float actualAnimSpeed = 1f;

		// Token: 0x04001958 RID: 6488
		protected Queue<CharacterState> desiredStateQueue = new Queue<CharacterState>();

		// Token: 0x04001959 RID: 6489
		public List<Block> attachments = new List<Block>();

		// Token: 0x0400195A RID: 6490
		public bool desiresMove;

		// Token: 0x0400195B RID: 6491
		private bool wasDesiringMove;

		// Token: 0x0400195C RID: 6492
		public Vector3 requestedMoveVelocity;

		// Token: 0x0400195D RID: 6493
		protected string playingAnim = string.Empty;

		// Token: 0x0400195E RID: 6494
		public float blendStart;

		// Token: 0x0400195F RID: 6495
		public int animationHash;

		// Token: 0x04001960 RID: 6496
		public bool firstFrame;

		// Token: 0x04001961 RID: 6497
		protected bool startPlay;

		// Token: 0x04001962 RID: 6498
		public Quaternion startRotation;

		// Token: 0x04001963 RID: 6499
		public float getUpAnim;

		// Token: 0x04001964 RID: 6500
		public CharacterJumpState.JumpState currentJumpState;

		// Token: 0x04001965 RID: 6501
		public bool hasDoubleJumped;

		// Token: 0x04001966 RID: 6502
		public bool allowDouble;

		// Token: 0x04001967 RID: 6503
		public int currentVelocityRange;

		// Token: 0x04001968 RID: 6504
		public float currentSpeed;

		// Token: 0x04001969 RID: 6505
		public CharacterWalkState.WalkDirection currentDirection = CharacterWalkState.WalkDirection.NumDirections;

		// Token: 0x0400196A RID: 6506
		public bool deliberateWalk;

		// Token: 0x0400196B RID: 6507
		public bool walkStrafe;

		// Token: 0x0400196C RID: 6508
		public bool preventLook;

		// Token: 0x0400196D RID: 6509
		public bool requestingPlayAnim;

		// Token: 0x0400196E RID: 6510
		public string playAnimCurrent;

		// Token: 0x0400196F RID: 6511
		public bool playAnimFinished;

		// Token: 0x04001970 RID: 6512
		private Quaternion sideAnimRotation = Quaternion.identity;

		// Token: 0x04001971 RID: 6513
		private CharacterState queuedHitReactState;

		// Token: 0x04001972 RID: 6514
		private Dictionary<int, object> savedAnimatorParameterValues;

		// Token: 0x04001973 RID: 6515
		protected static Dictionary<CharacterState, Dictionary<CharacterRole, CharacterStateHandler.InternalStateInfo>> stateMap = new Dictionary<CharacterState, Dictionary<CharacterRole, CharacterStateHandler.InternalStateInfo>>();

		// Token: 0x020002A1 RID: 673
		protected class InternalStateInfo
		{
			// Token: 0x04001974 RID: 6516
			public List<CharacterState> allowedTransitions = new List<CharacterState>();

			// Token: 0x04001975 RID: 6517
			public CharacterBaseState stateFunctions;

			// Token: 0x04001976 RID: 6518
			public int transitionAnim = -1;

			// Token: 0x04001977 RID: 6519
			public float transitionBlend = 0.05f;

			// Token: 0x04001978 RID: 6520
			public float animationRate = 1f;

			// Token: 0x04001979 RID: 6521
			public HashSet<string> tags = new HashSet<string>();
		}
	}
}
