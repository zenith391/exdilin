using System;
using System.Collections.Generic;
using Blocks;
using SimpleJSON;
using UnityEngine;

// Token: 0x020002B0 RID: 688
public class UpperBodyStateHandler : StateHandlerBase
{
	// Token: 0x06001FB1 RID: 8113 RVA: 0x000E41B1 File Offset: 0x000E25B1
	public UpperBodyStateHandler(CharacterStateHandler characterStateHandler)
	{
		this._characterStateHandler = characterStateHandler;
	}

	// Token: 0x06001FB2 RID: 8114 RVA: 0x000E41CC File Offset: 0x000E25CC
	public static void LoadStateMap(string jsonMap, CharacterRole defaultRole = CharacterRole.Male)
	{
		JObject jobject = JSONDecoder.Decode(jsonMap);
		Dictionary<string, JObject> objectValue = jobject.ObjectValue;
		foreach (JObject jobject2 in objectValue["states"].ArrayValue)
		{
			UpperBodyStateHandler.UpperBodyStateInfo upperBodyStateInfo = new UpperBodyStateHandler.UpperBodyStateInfo();
			UpperBodyState upperBodyState = (UpperBodyState)Enum.Parse(typeof(UpperBodyState), jobject2["name"].StringValue);
			CharacterRole key = defaultRole;
			if (jobject2.ContainsKey("role"))
			{
				key = (CharacterRole)Enum.Parse(typeof(CharacterRole), jobject2["role"].StringValue);
			}
			List<string> list = (!jobject2.ContainsKey("animations")) ? null : new List<string>(jobject2["animations"].StringValue.Split(new char[]
			{
				','
			}));
			bool isLeftHanded = false;
			bool isRightHanded = false;
			if (jobject2.ContainsKey("hands"))
			{
				List<string> list2 = new List<string>(jobject2["hands"].StringValue.Split(new char[]
				{
					','
				}));
				foreach (string text in list2)
				{
					if (text.ToLower() == "left")
					{
						isLeftHanded = true;
					}
					if (text.ToLower() == "right")
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
				upperBodyStateInfo.stateType = (UpperBodyStateType)Enum.Parse(typeof(UpperBodyStateType), jobject2["type"].StringValue);
			}
			else
			{
				upperBodyStateInfo.stateType = UpperBodyStateType.None;
			}
			if (upperBodyState == UpperBodyState.BaseLayer)
			{
				UpperBodyIdleState upperBodyIdleState = new UpperBodyIdleState();
				upperBodyIdleState.animationState = list[0];
				upperBodyStateInfo.layerWeight = 0f;
				upperBodyStateInfo.stateFunctions = upperBodyIdleState;
			}
			else if (upperBodyStateInfo.stateType == UpperBodyStateType.Melee_Attack)
			{
				MeleeAttackState meleeAttackState = new MeleeAttackState();
				UpperBodyAttackState stateFunctions = new UpperBodyAttackState(meleeAttackState);
				meleeAttackState.animationState = list[0];
				if (list.Count > 1)
				{
					meleeAttackState.recoilAnimation = list[1];
				}
				meleeAttackState.isLeftHanded = isLeftHanded;
				meleeAttackState.isRightHanded = isRightHanded;
				meleeAttackState.damageStartNormalizedTime = ((!jobject2.ContainsKey("damage_start_normalized")) ? 0f : jobject2["damage_start_normalized"].FloatValue);
				meleeAttackState.damageEndNormalizedTime = ((!jobject2.ContainsKey("damage_end_normalized")) ? 1f : jobject2["damage_end_normalized"].FloatValue);
				meleeAttackState.interruptNormalizedTime = ((!jobject2.ContainsKey("interrupt_normalized")) ? 0.8f : jobject2["interrupt_normalized"].FloatValue);
				upperBodyStateInfo.layerWeight = 1f;
				upperBodyStateInfo.stateFunctions = stateFunctions;
			}
			else if (upperBodyStateInfo.stateType == UpperBodyStateType.Ranged_Attack)
			{
				UpperBodyFireWeaponState upperBodyFireWeaponState = new UpperBodyFireWeaponState();
				upperBodyFireWeaponState.animationState = list[0];
				upperBodyFireWeaponState.fireNormalizedTime = ((!jobject2.ContainsKey("damage_start_normalized")) ? 0f : jobject2["damage_start_normalized"].FloatValue);
				upperBodyFireWeaponState.interruptNormalizedTime = ((!jobject2.ContainsKey("interrupt_normalized")) ? 0.8f : jobject2["interrupt_normalized"].FloatValue);
				upperBodyFireWeaponState.isLeftHanded = isLeftHanded;
				upperBodyStateInfo.layerWeight = 1f;
				upperBodyStateInfo.stateFunctions = upperBodyFireWeaponState;
			}
			else if (upperBodyStateInfo.stateType == UpperBodyStateType.Block)
			{
				UpperBodyBlockState upperBodyBlockState = new UpperBodyBlockState();
				if (list.Count < 4)
				{
					BWLog.Error("Expecting 4 animations for block state, in, loop, out, react");
				}
				upperBodyBlockState.animations = list;
				upperBodyBlockState.animationIn = list[0];
				upperBodyBlockState.animationLoop = list[1];
				upperBodyBlockState.animationOut = list[2];
				upperBodyBlockState.animationHitReact = list[3];
				upperBodyBlockState.isLeftHanded = isLeftHanded;
				upperBodyStateInfo.layerWeight = 1f;
				upperBodyStateInfo.stateFunctions = upperBodyBlockState;
			}
			if (jobject2.ContainsKey("blend"))
			{
				upperBodyStateInfo.transitionBlend = jobject2["blend"].FloatValue;
			}
			else
			{
				upperBodyStateInfo.transitionBlend = 0.025f;
			}
			if (upperBodyStateInfo.stateFunctions != null)
			{
				if (!UpperBodyStateHandler.stateMap.ContainsKey(upperBodyState))
				{
					UpperBodyStateHandler.stateMap[upperBodyState] = new Dictionary<CharacterRole, UpperBodyStateHandler.UpperBodyStateInfo>();
				}
				UpperBodyStateHandler.stateMap[upperBodyState][key] = upperBodyStateInfo;
			}
		}
	}

	// Token: 0x06001FB3 RID: 8115 RVA: 0x000E46EC File Offset: 0x000E2AEC
	public static void ClearStateMap()
	{
		UpperBodyStateHandler.stateMap.Clear();
	}

	// Token: 0x06001FB4 RID: 8116 RVA: 0x000E46F8 File Offset: 0x000E2AF8
	public override int PlayAnimation(string animation, bool interrupt = false)
	{
		this.PlayAnimation(Animator.StringToHash(animation), interrupt);
		return this._animationHash;
	}

	// Token: 0x06001FB5 RID: 8117 RVA: 0x000E4710 File Offset: 0x000E2B10
	public int PlayAnimation(int hash, bool interrupt = false)
	{
		this._animationHash = hash;
		if (null == this.targetController)
		{
			return 0;
		}
		float transitionDuration = this.stateBlend;
		this.blendStart = this.stateTime;
		if (this.stateBlend > 0f && !interrupt)
		{
			this.targetController.CrossFade(this._animationHash, transitionDuration, 1, 0f);
			this.targetController.CrossFade(this._animationHash, transitionDuration, 2, 0f);
		}
		else
		{
			this.targetController.Play(this._animationHash, 1, 0f);
			this.targetController.Play(this._animationHash, 2, 0f);
		}
		return this._animationHash;
	}

	// Token: 0x06001FB6 RID: 8118 RVA: 0x000E47CC File Offset: 0x000E2BCC
	public override bool IsPlayingAnimation(string animName)
	{
		return this.targetController.GetCurrentAnimatorStateInfo(this.GetAnimatorLayer()).IsName(animName);
	}

	// Token: 0x06001FB7 RID: 8119 RVA: 0x000E47F3 File Offset: 0x000E2BF3
	public override int GetAnimatorLayer()
	{
		return (!this._playingOnFullBody) ? 1 : 2;
	}

	// Token: 0x06001FB8 RID: 8120 RVA: 0x000E4807 File Offset: 0x000E2C07
	public override float TimeInCurrentState()
	{
		return this.stateTime;
	}

	// Token: 0x06001FB9 RID: 8121 RVA: 0x000E4810 File Offset: 0x000E2C10
	public void SetRole(CharacterRole role)
	{
		this._currentRole = role;
		if (this._currentState == UpperBodyState.None)
		{
			this._currentFunctions = null;
			return;
		}
		this._currentStateInfo = this.GetStateInfo(UpperBodyState.None, CharacterRole.None);
		if (this._currentStateInfo == null)
		{
			BWLog.Error(string.Concat(new object[]
			{
				"Unable to find state info for ",
				this._currentState,
				" in role ",
				this._currentRole
			}));
			return;
		}
		this._currentFunctions = this._currentStateInfo.stateFunctions;
	}

	// Token: 0x06001FBA RID: 8122 RVA: 0x000E48A2 File Offset: 0x000E2CA2
	public void Play()
	{
		this._playingOnFullBody = true;
		this.EnterState(UpperBodyState.BaseLayer, true);
	}

	// Token: 0x06001FBB RID: 8123 RVA: 0x000E48B4 File Offset: 0x000E2CB4
	public void Stop()
	{
		this.EnterState(UpperBodyState.BaseLayer, true);
		this.PlayOnFullBody(false);
		this._fullBodyBlend = 0f;
		this.SetLayerWeight(0f);
		this.targetController.SetLayerWeight(3, 0f);
		this.targetController.SetLayerWeight(4, 0f);
	}

	// Token: 0x06001FBC RID: 8124 RVA: 0x000E4908 File Offset: 0x000E2D08
	public void Attack(UpperBodyState attackState)
	{
		if (this.CanStartNewAttack())
		{
			this.EnterState(attackState, false);
		}
	}

	// Token: 0x06001FBD RID: 8125 RVA: 0x000E4920 File Offset: 0x000E2D20
	public bool CanStartNewAttack()
	{
		if (this._characterStateHandler.sideAnim != -1 || this._characterStateHandler.IsImmobile() || this._characterStateHandler.IsImpactState())
		{
			return false;
		}
		if (this._currentStateInfo.stateType == UpperBodyStateType.None || this._currentStateInfo.stateType == UpperBodyStateType.Anim)
		{
			return true;
		}
		if (this._currentStateInfo.stateType == UpperBodyStateType.Melee_Attack)
		{
			MeleeAttackState attackState = ((UpperBodyAttackState)this._currentFunctions).attackState;
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
		else
		{
			if (this._currentStateInfo.stateType != UpperBodyStateType.Ranged_Attack)
			{
				return true;
			}
			UpperBodyFireWeaponState upperBodyFireWeaponState = (UpperBodyFireWeaponState)this._currentFunctions;
			if (upperBodyFireWeaponState == null || this.targetController.IsInTransition(this.GetAnimatorLayer()))
			{
				return false;
			}
			float normalizedTime2 = this.targetController.GetCurrentAnimatorStateInfo(this.GetAnimatorLayer()).normalizedTime;
			return normalizedTime2 > upperBodyFireWeaponState.interruptNormalizedTime;
		}
	}

	// Token: 0x06001FBE RID: 8126 RVA: 0x000E4A64 File Offset: 0x000E2E64
	public void Shield(UpperBodyState shieldState)
	{
		if (this._characterStateHandler.sideAnim == -1 && !this._characterStateHandler.IsImmobile() && (this._currentStateInfo.stateType == UpperBodyStateType.None || this._currentStateInfo.stateType == UpperBodyStateType.Anim))
		{
			this.EnterState(shieldState, false);
		}
		if (this._currentStateInfo.stateType == UpperBodyStateType.Block)
		{
			this._characterStateHandler.combatController.SetIsShielding();
		}
	}

	// Token: 0x06001FBF RID: 8127 RVA: 0x000E4ADC File Offset: 0x000E2EDC
	public UpperBodyState GetState()
	{
		return this._currentState;
	}

	// Token: 0x06001FC0 RID: 8128 RVA: 0x000E4AE4 File Offset: 0x000E2EE4
	public bool InAttackState()
	{
		return this._currentStateInfo.stateType == UpperBodyStateType.Melee_Attack || this._currentStateInfo.stateType == UpperBodyStateType.Ranged_Attack;
	}

	// Token: 0x06001FC1 RID: 8129 RVA: 0x000E4B08 File Offset: 0x000E2F08
	public void PlayOnFullBody(bool fullBody)
	{
		this._playingOnFullBody = fullBody;
	}

	// Token: 0x06001FC2 RID: 8130 RVA: 0x000E4B14 File Offset: 0x000E2F14
	protected UpperBodyStateHandler.UpperBodyStateInfo GetStateInfo(UpperBodyState state = UpperBodyState.None, CharacterRole role = CharacterRole.None)
	{
		if (state == UpperBodyState.None)
		{
			state = this._currentState;
		}
		if (role == CharacterRole.None)
		{
			role = this._currentRole;
		}
		if (!UpperBodyStateHandler.stateMap.ContainsKey(state))
		{
			return null;
		}
		Dictionary<CharacterRole, UpperBodyStateHandler.UpperBodyStateInfo> dictionary = UpperBodyStateHandler.stateMap[state];
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

	// Token: 0x06001FC3 RID: 8131 RVA: 0x000E4BA4 File Offset: 0x000E2FA4
	public string GetDebugDescription()
	{
		return string.Concat(new object[]
		{
			"State: ",
			this._currentState,
			" Class: ",
			this._currentFunctions.GetType()
		});
	}

	// Token: 0x06001FC4 RID: 8132 RVA: 0x000E4BEA File Offset: 0x000E2FEA
	public void InterruptState(UpperBodyState desiredState)
	{
		this.EnterState(desiredState, true);
	}

	// Token: 0x06001FC5 RID: 8133 RVA: 0x000E4BF4 File Offset: 0x000E2FF4
	private void EnterState(UpperBodyState desiredState, bool interrupt = false)
	{
		if (this.UpperBodyOverridden() && desiredState != UpperBodyState.BaseLayer)
		{
			return;
		}
		if (this._currentFunctions != null)
		{
			this.ExitState();
		}
		this.stateTime = 0f;
		this.blendStart = 0f;
		this._currentState = desiredState;
		this._currentStateInfo = this.GetStateInfo(UpperBodyState.None, CharacterRole.None);
		if (this._currentStateInfo == null)
		{
			BWLog.Error(string.Concat(new object[]
			{
				"Unable to find state info for ",
				this._currentState,
				" in role ",
				this._currentRole
			}));
			return;
		}
		this._currentFunctions = this._currentStateInfo.stateFunctions;
		this.stateBlend = this._currentStateInfo.transitionBlend;
		if (this._currentFunctions == null)
		{
			BWLog.Warning("Entering state " + this._currentState + ", which has no class to support it");
			return;
		}
		this._currentFunctions.Enter(this, interrupt);
		this._layerWeightBlendTime = this._currentStateInfo.transitionBlend;
		if (this._layerWeight == this._currentStateInfo.layerWeight)
		{
			this._blendingLayerWeight = false;
		}
		else if (interrupt)
		{
			this.SetLayerWeight(this._currentStateInfo.layerWeight);
			this._blendingLayerWeight = false;
		}
		else
		{
			this._layerWeightBlendFrom = this._layerWeight;
			this._layerWeightBlendTo = this._currentStateInfo.layerWeight;
			this._blendingLayerWeight = true;
		}
	}

	// Token: 0x06001FC6 RID: 8134 RVA: 0x000E4D6D File Offset: 0x000E316D
	private void ExitState()
	{
		if (this._currentFunctions == null)
		{
			return;
		}
		this._currentFunctions.Exit(this);
	}

	// Token: 0x06001FC7 RID: 8135 RVA: 0x000E4D88 File Offset: 0x000E3188
	private void SetLayerWeight(float weight)
	{
		this._layerWeight = weight;
		float weight2 = weight * (1f - this._fullBodyBlend);
		float weight3 = weight * this._fullBodyBlend;
		this.targetController.SetLayerWeight(1, weight2);
		this.targetController.SetLayerWeight(2, weight3);
	}

	// Token: 0x06001FC8 RID: 8136 RVA: 0x000E4DD0 File Offset: 0x000E31D0
	public bool IsPlayingAnimationFromList(List<string> animNames)
	{
		if (animNames == null)
		{
			return false;
		}
		int shortNameHash = this.targetController.GetCurrentAnimatorStateInfo(this.GetAnimatorLayer()).shortNameHash;
		foreach (string name in animNames)
		{
			if (Animator.StringToHash(name) == shortNameHash)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x17000152 RID: 338
	// (get) Token: 0x06001FC9 RID: 8137 RVA: 0x000E4E5C File Offset: 0x000E325C
	private Block LeftHandAttachment
	{
		get
		{
			return this._characterStateHandler.combatController.GetLeftHandAttachment();
		}
	}

	// Token: 0x17000153 RID: 339
	// (get) Token: 0x06001FCA RID: 8138 RVA: 0x000E4E6E File Offset: 0x000E326E
	private Block RightHandAttachment
	{
		get
		{
			return this._characterStateHandler.combatController.GetRightHandAttachment();
		}
	}

	// Token: 0x06001FCB RID: 8139 RVA: 0x000E4E80 File Offset: 0x000E3280
	private void DampLeftHand(bool damp)
	{
		bool flag = this.LeftHandAttachment != null && !this.LeftHandAttachment.IsRuntimeInvisible();
		float weight = (!flag || !damp) ? 0f : 0.55f;
		this.SetHandLayerWeight(4, weight);
	}

	// Token: 0x06001FCC RID: 8140 RVA: 0x000E4ED0 File Offset: 0x000E32D0
	private void DampRightHand(bool damp)
	{
		bool flag = this.RightHandAttachment != null && !this.RightHandAttachment.IsRuntimeInvisible();
		float weight = (!flag || !damp) ? 0f : 0.55f;
		this.SetHandLayerWeight(3, weight);
	}

	// Token: 0x06001FCD RID: 8141 RVA: 0x000E4F20 File Offset: 0x000E3320
	private void SetHandLayerWeight(int layer, float weight)
	{
		float layerWeight = this.targetController.GetLayerWeight(layer);
		float f = weight - layerWeight;
		if (Mathf.Abs(f) < Mathf.Epsilon)
		{
			return;
		}
		float t = 12f * Time.fixedDeltaTime;
		float weight2 = Mathf.Lerp(layerWeight, weight, t);
		this.targetController.SetLayerWeight(layer, weight2);
	}

	// Token: 0x06001FCE RID: 8142 RVA: 0x000E4F71 File Offset: 0x000E3371
	private bool UpperBodyOverridden()
	{
		return this._characterStateHandler.IsImmobile() || this._characterStateHandler.InStandingAttack();
	}

	// Token: 0x06001FCF RID: 8143 RVA: 0x000E4F94 File Offset: 0x000E3394
	public void Update()
	{
		if (this._playingOnFullBody && this._fullBodyBlend < 1f)
		{
			this._fullBodyBlend = Mathf.Min(1f, this._fullBodyBlend + 0.1f);
		}
		if (!this._playingOnFullBody && this._fullBodyBlend > 0f)
		{
			this._fullBodyBlend = Mathf.Max(0f, this._fullBodyBlend - 0.1f);
		}
		float layerWeight = this._layerWeight;
		if (this._blendingLayerWeight)
		{
			if (this.stateTime >= this._layerWeightBlendTime)
			{
				layerWeight = this._layerWeightBlendTo;
				this._blendingLayerWeight = false;
			}
			else
			{
				float t = this.stateTime / this._layerWeightBlendTime;
				layerWeight = Mathf.Lerp(this._layerWeightBlendFrom, this._layerWeightBlendTo, t);
			}
		}
		this.SetLayerWeight(layerWeight);
		UpperBodyState currentState = this._currentState;
		if (currentState != UpperBodyState.BaseLayer)
		{
			this.DampRightHand(false);
			this.DampLeftHand(false);
		}
		else
		{
			bool damp = !this.UpperBodyOverridden();
			this.DampLeftHand(damp);
			this.DampRightHand(damp);
		}
		if (this._currentStateInfo.stateType != UpperBodyStateType.None)
		{
			bool flag = !this._currentFunctions.Update(this);
			flag |= this.UpperBodyOverridden();
			if (flag)
			{
				this.EnterState(UpperBodyState.BaseLayer, false);
			}
		}
		this.stateTime += Time.fixedDeltaTime;
	}

	// Token: 0x040019E2 RID: 6626
	private CharacterStateHandler _characterStateHandler;

	// Token: 0x040019E3 RID: 6627
	private UpperBodyState _currentState;

	// Token: 0x040019E4 RID: 6628
	private UpperBodyBaseState _currentFunctions;

	// Token: 0x040019E5 RID: 6629
	private UpperBodyStateHandler.UpperBodyStateInfo _currentStateInfo;

	// Token: 0x040019E6 RID: 6630
	private CharacterRole _currentRole;

	// Token: 0x040019E7 RID: 6631
	private const int _upperBodyAnimatorLayer = 1;

	// Token: 0x040019E8 RID: 6632
	private const int _fullBodyAnimatorLayer = 2;

	// Token: 0x040019E9 RID: 6633
	private const int _rightHandAnimatorLayer = 3;

	// Token: 0x040019EA RID: 6634
	private const int _leftHandAnimatorLayer = 4;

	// Token: 0x040019EB RID: 6635
	private float _layerWeight;

	// Token: 0x040019EC RID: 6636
	private bool _playingOnFullBody;

	// Token: 0x040019ED RID: 6637
	private float _fullBodyBlend;

	// Token: 0x040019EE RID: 6638
	private const float _fullBodyBlendSpeed = 0.1f;

	// Token: 0x040019EF RID: 6639
	public float stateTime;

	// Token: 0x040019F0 RID: 6640
	public float stateBlend = 0.05f;

	// Token: 0x040019F1 RID: 6641
	public float blendStart;

	// Token: 0x040019F2 RID: 6642
	private int _animationHash;

	// Token: 0x040019F3 RID: 6643
	private float _layerWeightBlendTo;

	// Token: 0x040019F4 RID: 6644
	private float _layerWeightBlendFrom;

	// Token: 0x040019F5 RID: 6645
	private float _layerWeightBlendTime;

	// Token: 0x040019F6 RID: 6646
	private bool _blendingLayerWeight;

	// Token: 0x040019F7 RID: 6647
	protected static Dictionary<UpperBodyState, Dictionary<CharacterRole, UpperBodyStateHandler.UpperBodyStateInfo>> stateMap = new Dictionary<UpperBodyState, Dictionary<CharacterRole, UpperBodyStateHandler.UpperBodyStateInfo>>();

	// Token: 0x040019F8 RID: 6648
	private const float propDamp = 0.55f;

	// Token: 0x020002B1 RID: 689
	protected class UpperBodyStateInfo
	{
		// Token: 0x040019F9 RID: 6649
		public UpperBodyBaseState stateFunctions;

		// Token: 0x040019FA RID: 6650
		public int transitionAnim = -1;

		// Token: 0x040019FB RID: 6651
		public float transitionBlend = 0.05f;

		// Token: 0x040019FC RID: 6652
		public float layerWeight = 1f;

		// Token: 0x040019FD RID: 6653
		public UpperBodyStateType stateType;
	}
}
