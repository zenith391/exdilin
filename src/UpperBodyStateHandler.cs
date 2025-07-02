using System;
using System.Collections.Generic;
using Blocks;
using SimpleJSON;
using UnityEngine;

public class UpperBodyStateHandler : StateHandlerBase
{
	protected class UpperBodyStateInfo
	{
		public UpperBodyBaseState stateFunctions;

		public int transitionAnim = -1;

		public float transitionBlend = 0.05f;

		public float layerWeight = 1f;

		public UpperBodyStateType stateType;
	}

	private CharacterStateHandler _characterStateHandler;

	private UpperBodyState _currentState;

	private UpperBodyBaseState _currentFunctions;

	private UpperBodyStateInfo _currentStateInfo;

	private CharacterRole _currentRole;

	private const int _upperBodyAnimatorLayer = 1;

	private const int _fullBodyAnimatorLayer = 2;

	private const int _rightHandAnimatorLayer = 3;

	private const int _leftHandAnimatorLayer = 4;

	private float _layerWeight;

	private bool _playingOnFullBody;

	private float _fullBodyBlend;

	private const float _fullBodyBlendSpeed = 0.1f;

	public float stateTime;

	public float stateBlend = 0.05f;

	public float blendStart;

	private int _animationHash;

	private float _layerWeightBlendTo;

	private float _layerWeightBlendFrom;

	private float _layerWeightBlendTime;

	private bool _blendingLayerWeight;

	protected static Dictionary<UpperBodyState, Dictionary<CharacterRole, UpperBodyStateInfo>> stateMap = new Dictionary<UpperBodyState, Dictionary<CharacterRole, UpperBodyStateInfo>>();

	private const float propDamp = 0.55f;

	private Block LeftHandAttachment => _characterStateHandler.combatController.GetLeftHandAttachment();

	private Block RightHandAttachment => _characterStateHandler.combatController.GetRightHandAttachment();

	public UpperBodyStateHandler(CharacterStateHandler characterStateHandler)
	{
		_characterStateHandler = characterStateHandler;
	}

	public static void LoadStateMap(string jsonMap, CharacterRole defaultRole = CharacterRole.Male)
	{
		JObject jObject = JSONDecoder.Decode(jsonMap);
		Dictionary<string, JObject> objectValue = jObject.ObjectValue;
		foreach (JObject item in objectValue["states"].ArrayValue)
		{
			UpperBodyStateInfo upperBodyStateInfo = new UpperBodyStateInfo();
			UpperBodyState upperBodyState = (UpperBodyState)Enum.Parse(typeof(UpperBodyState), item["name"].StringValue);
			CharacterRole key = defaultRole;
			if (item.ContainsKey("role"))
			{
				key = (CharacterRole)Enum.Parse(typeof(CharacterRole), item["role"].StringValue);
			}
			List<string> list = ((!item.ContainsKey("animations")) ? null : new List<string>(item["animations"].StringValue.Split(',')));
			bool isLeftHanded = false;
			bool isRightHanded = false;
			if (item.ContainsKey("hands"))
			{
				List<string> list2 = new List<string>(item["hands"].StringValue.Split(','));
				foreach (string item2 in list2)
				{
					if (item2.ToLower() == "left")
					{
						isLeftHanded = true;
					}
					if (item2.ToLower() == "right")
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
				upperBodyStateInfo.stateType = (UpperBodyStateType)Enum.Parse(typeof(UpperBodyStateType), item["type"].StringValue);
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
				meleeAttackState.damageStartNormalizedTime = ((!item.ContainsKey("damage_start_normalized")) ? 0f : item["damage_start_normalized"].FloatValue);
				meleeAttackState.damageEndNormalizedTime = ((!item.ContainsKey("damage_end_normalized")) ? 1f : item["damage_end_normalized"].FloatValue);
				meleeAttackState.interruptNormalizedTime = ((!item.ContainsKey("interrupt_normalized")) ? 0.8f : item["interrupt_normalized"].FloatValue);
				upperBodyStateInfo.layerWeight = 1f;
				upperBodyStateInfo.stateFunctions = stateFunctions;
			}
			else if (upperBodyStateInfo.stateType == UpperBodyStateType.Ranged_Attack)
			{
				UpperBodyFireWeaponState upperBodyFireWeaponState = new UpperBodyFireWeaponState();
				upperBodyFireWeaponState.animationState = list[0];
				upperBodyFireWeaponState.fireNormalizedTime = ((!item.ContainsKey("damage_start_normalized")) ? 0f : item["damage_start_normalized"].FloatValue);
				upperBodyFireWeaponState.interruptNormalizedTime = ((!item.ContainsKey("interrupt_normalized")) ? 0.8f : item["interrupt_normalized"].FloatValue);
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
			if (item.ContainsKey("blend"))
			{
				upperBodyStateInfo.transitionBlend = item["blend"].FloatValue;
			}
			else
			{
				upperBodyStateInfo.transitionBlend = 0.025f;
			}
			if (upperBodyStateInfo.stateFunctions != null)
			{
				if (!stateMap.ContainsKey(upperBodyState))
				{
					stateMap[upperBodyState] = new Dictionary<CharacterRole, UpperBodyStateInfo>();
				}
				stateMap[upperBodyState][key] = upperBodyStateInfo;
			}
		}
	}

	public static void ClearStateMap()
	{
		stateMap.Clear();
	}

	public override int PlayAnimation(string animation, bool interrupt = false)
	{
		PlayAnimation(Animator.StringToHash(animation), interrupt);
		return _animationHash;
	}

	public int PlayAnimation(int hash, bool interrupt = false)
	{
		_animationHash = hash;
		if (null == targetController)
		{
			return 0;
		}
		float transitionDuration = stateBlend;
		blendStart = stateTime;
		if (stateBlend > 0f && !interrupt)
		{
			targetController.CrossFade(_animationHash, transitionDuration, 1, 0f);
			targetController.CrossFade(_animationHash, transitionDuration, 2, 0f);
		}
		else
		{
			targetController.Play(_animationHash, 1, 0f);
			targetController.Play(_animationHash, 2, 0f);
		}
		return _animationHash;
	}

	public override bool IsPlayingAnimation(string animName)
	{
		return targetController.GetCurrentAnimatorStateInfo(GetAnimatorLayer()).IsName(animName);
	}

	public override int GetAnimatorLayer()
	{
		if (_playingOnFullBody)
		{
			return 2;
		}
		return 1;
	}

	public override float TimeInCurrentState()
	{
		return stateTime;
	}

	public void SetRole(CharacterRole role)
	{
		_currentRole = role;
		if (_currentState == UpperBodyState.None)
		{
			_currentFunctions = null;
			return;
		}
		_currentStateInfo = GetStateInfo();
		if (_currentStateInfo == null)
		{
			BWLog.Error(string.Concat("Unable to find state info for ", _currentState, " in role ", _currentRole));
		}
		else
		{
			_currentFunctions = _currentStateInfo.stateFunctions;
		}
	}

	public void Play()
	{
		_playingOnFullBody = true;
		EnterState(UpperBodyState.BaseLayer, interrupt: true);
	}

	public void Stop()
	{
		EnterState(UpperBodyState.BaseLayer, interrupt: true);
		PlayOnFullBody(fullBody: false);
		_fullBodyBlend = 0f;
		SetLayerWeight(0f);
		targetController.SetLayerWeight(3, 0f);
		targetController.SetLayerWeight(4, 0f);
	}

	public void Attack(UpperBodyState attackState)
	{
		if (CanStartNewAttack())
		{
			EnterState(attackState);
		}
	}

	public bool CanStartNewAttack()
	{
		if (_characterStateHandler.sideAnim != -1 || _characterStateHandler.IsImmobile() || _characterStateHandler.IsImpactState())
		{
			return false;
		}
		if (_currentStateInfo.stateType == UpperBodyStateType.None || _currentStateInfo.stateType == UpperBodyStateType.Anim)
		{
			return true;
		}
		if (_currentStateInfo.stateType == UpperBodyStateType.Melee_Attack)
		{
			MeleeAttackState attackState = ((UpperBodyAttackState)_currentFunctions).attackState;
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
		if (_currentStateInfo.stateType != UpperBodyStateType.Ranged_Attack)
		{
			return true;
		}
		UpperBodyFireWeaponState upperBodyFireWeaponState = (UpperBodyFireWeaponState)_currentFunctions;
		if (upperBodyFireWeaponState == null || targetController.IsInTransition(GetAnimatorLayer()))
		{
			return false;
		}
		float normalizedTime2 = targetController.GetCurrentAnimatorStateInfo(GetAnimatorLayer()).normalizedTime;
		return normalizedTime2 > upperBodyFireWeaponState.interruptNormalizedTime;
	}

	public void Shield(UpperBodyState shieldState)
	{
		if (_characterStateHandler.sideAnim == -1 && !_characterStateHandler.IsImmobile() && (_currentStateInfo.stateType == UpperBodyStateType.None || _currentStateInfo.stateType == UpperBodyStateType.Anim))
		{
			EnterState(shieldState);
		}
		if (_currentStateInfo.stateType == UpperBodyStateType.Block)
		{
			_characterStateHandler.combatController.SetIsShielding();
		}
	}

	public UpperBodyState GetState()
	{
		return _currentState;
	}

	public bool InAttackState()
	{
		if (_currentStateInfo.stateType != UpperBodyStateType.Melee_Attack)
		{
			return _currentStateInfo.stateType == UpperBodyStateType.Ranged_Attack;
		}
		return true;
	}

	public void PlayOnFullBody(bool fullBody)
	{
		_playingOnFullBody = fullBody;
	}

	protected UpperBodyStateInfo GetStateInfo(UpperBodyState state = UpperBodyState.None, CharacterRole role = CharacterRole.None)
	{
		if (state == UpperBodyState.None)
		{
			state = _currentState;
		}
		if (role == CharacterRole.None)
		{
			role = _currentRole;
		}
		if (!stateMap.ContainsKey(state))
		{
			return null;
		}
		Dictionary<CharacterRole, UpperBodyStateInfo> dictionary = stateMap[state];
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

	public string GetDebugDescription()
	{
		return string.Concat("State: ", _currentState, " Class: ", _currentFunctions.GetType());
	}

	public void InterruptState(UpperBodyState desiredState)
	{
		EnterState(desiredState, interrupt: true);
	}

	private void EnterState(UpperBodyState desiredState, bool interrupt = false)
	{
		if (UpperBodyOverridden() && desiredState != UpperBodyState.BaseLayer)
		{
			return;
		}
		if (_currentFunctions != null)
		{
			ExitState();
		}
		stateTime = 0f;
		blendStart = 0f;
		_currentState = desiredState;
		_currentStateInfo = GetStateInfo();
		if (_currentStateInfo == null)
		{
			BWLog.Error(string.Concat("Unable to find state info for ", _currentState, " in role ", _currentRole));
			return;
		}
		_currentFunctions = _currentStateInfo.stateFunctions;
		stateBlend = _currentStateInfo.transitionBlend;
		if (_currentFunctions == null)
		{
			BWLog.Warning("Entering state " + _currentState.ToString() + ", which has no class to support it");
			return;
		}
		_currentFunctions.Enter(this, interrupt);
		_layerWeightBlendTime = _currentStateInfo.transitionBlend;
		if (_layerWeight == _currentStateInfo.layerWeight)
		{
			_blendingLayerWeight = false;
		}
		else if (interrupt)
		{
			SetLayerWeight(_currentStateInfo.layerWeight);
			_blendingLayerWeight = false;
		}
		else
		{
			_layerWeightBlendFrom = _layerWeight;
			_layerWeightBlendTo = _currentStateInfo.layerWeight;
			_blendingLayerWeight = true;
		}
	}

	private void ExitState()
	{
		if (_currentFunctions != null)
		{
			_currentFunctions.Exit(this);
		}
	}

	private void SetLayerWeight(float weight)
	{
		_layerWeight = weight;
		float weight2 = weight * (1f - _fullBodyBlend);
		float weight3 = weight * _fullBodyBlend;
		targetController.SetLayerWeight(1, weight2);
		targetController.SetLayerWeight(2, weight3);
	}

	public bool IsPlayingAnimationFromList(List<string> animNames)
	{
		if (animNames == null)
		{
			return false;
		}
		int shortNameHash = targetController.GetCurrentAnimatorStateInfo(GetAnimatorLayer()).shortNameHash;
		foreach (string animName in animNames)
		{
			if (Animator.StringToHash(animName) == shortNameHash)
			{
				return true;
			}
		}
		return false;
	}

	private void DampLeftHand(bool damp)
	{
		float weight = ((LeftHandAttachment == null || LeftHandAttachment.IsRuntimeInvisible() || !damp) ? 0f : 0.55f);
		SetHandLayerWeight(4, weight);
	}

	private void DampRightHand(bool damp)
	{
		float weight = ((RightHandAttachment == null || RightHandAttachment.IsRuntimeInvisible() || !damp) ? 0f : 0.55f);
		SetHandLayerWeight(3, weight);
	}

	private void SetHandLayerWeight(int layer, float weight)
	{
		float layerWeight = targetController.GetLayerWeight(layer);
		float f = weight - layerWeight;
		if (!(Mathf.Abs(f) < Mathf.Epsilon))
		{
			float t = 12f * Time.fixedDeltaTime;
			float weight2 = Mathf.Lerp(layerWeight, weight, t);
			targetController.SetLayerWeight(layer, weight2);
		}
	}

	private bool UpperBodyOverridden()
	{
		if (!_characterStateHandler.IsImmobile())
		{
			return _characterStateHandler.InStandingAttack();
		}
		return true;
	}

	public void Update()
	{
		if (_playingOnFullBody && _fullBodyBlend < 1f)
		{
			_fullBodyBlend = Mathf.Min(1f, _fullBodyBlend + 0.1f);
		}
		if (!_playingOnFullBody && _fullBodyBlend > 0f)
		{
			_fullBodyBlend = Mathf.Max(0f, _fullBodyBlend - 0.1f);
		}
		float layerWeight = _layerWeight;
		if (_blendingLayerWeight)
		{
			if (stateTime >= _layerWeightBlendTime)
			{
				layerWeight = _layerWeightBlendTo;
				_blendingLayerWeight = false;
			}
			else
			{
				float t = stateTime / _layerWeightBlendTime;
				layerWeight = Mathf.Lerp(_layerWeightBlendFrom, _layerWeightBlendTo, t);
			}
		}
		SetLayerWeight(layerWeight);
		if (_currentState != UpperBodyState.BaseLayer)
		{
			DampRightHand(damp: false);
			DampLeftHand(damp: false);
		}
		else
		{
			bool damp = !UpperBodyOverridden();
			DampLeftHand(damp);
			DampRightHand(damp);
		}
		if (_currentStateInfo.stateType != UpperBodyStateType.None)
		{
			bool flag = !_currentFunctions.Update(this);
			if (flag | UpperBodyOverridden())
			{
				EnterState(UpperBodyState.BaseLayer);
			}
		}
		stateTime += Time.fixedDeltaTime;
	}
}
