using System;
using System.Collections;
using System.Collections.Generic;
using Blocks;
using UnityEngine;
using UnityEngine.UI;

public class UIControls : MonoBehaviour
{
	[Serializable]
	public struct SpriteVariantInfo
	{
		public UIInputControl.ControlVariant variant;

		public Sprite sprite;

		public Sprite spritePressed;

		public Sprite keySprite;
	}

	public List<SpriteVariantInfo> inputControlVariants;

	public UIMoverUniversal leftUniversalMover;

	public UIMoverUniversal rightUniversalMover;

	public UIMover leftMover;

	public UIMover rightMover;

	public bool moversOverlapControls = true;

	public bool mouseAndFingerControlEnabled = true;

	public bool useKeyImages;

	public bool hideInactiveControls = true;

	public bool hideActiveControlsAfterTimeout;

	public float hideActiveControlsTimeout = 5f;

	public float hideActiveControlsFadeoutTimer = 1f;

	public Animator tiltPromptAnimator;

	private bool _hasAnyControlBeenPressed;

	private float _leftMoverSafeX;

	private float _leftMoverSafeY;

	private float _rightMoverSafeX;

	private float _rightMoverSafeY;

	private float _moverShiftY = 50f;

	private float _moverShiftX = 65f;

	private bool _leftMoverInUse;

	private bool _rightMoverInUse;

	private bool _showActiveControls;

	private float _hideActiveControlsTimer;

	private int _lastVisibleControlMask;

	private UIInputControl[] _inputControls;

	private Dictionary<UIInputControl.ControlType, UIInputControl> inputControlLookup;

	private Dictionary<UIInputControl.ControlVariant, SpriteVariantInfo> controlVariantSpriteLookup;

	private Dictionary<UIInputControl.ControlType, UIInputControl.ControlVariant> activeControlVariants;

	private InputVisibilityHandler inputVisibilityHandler;

	private List<UIMover> _allMovers = new List<UIMover>();

	private CanvasScaler _canvasScaler;

	private BitArray _externalTriggers;

	private Dictionary<int, List<UIInputControl>> _visibiltyGroups;

	private bool tiltPromptCancelled;

	private float lastTiltPromptShowRequestTime;

	public void Init()
	{
		_inputControls = GetComponentsInChildren<UIInputControl>(includeInactive: true);
		inputControlLookup = new Dictionary<UIInputControl.ControlType, UIInputControl>();
		_visibiltyGroups = new Dictionary<int, List<UIInputControl>>();
		for (int i = 0; i < _inputControls.Length; i++)
		{
			UIInputControl uIInputControl = _inputControls[i];
			inputControlLookup[uIInputControl.controlType] = uIInputControl;
			uIInputControl.Init();
			uIInputControl.SetPointerControlEnabled(mouseAndFingerControlEnabled);
			int visibilityGroup = uIInputControl.visibilityGroup;
			if (visibilityGroup > 0)
			{
				List<UIInputControl> value = null;
				if (!_visibiltyGroups.TryGetValue(visibilityGroup, out value))
				{
					value = new List<UIInputControl>();
					_visibiltyGroups[visibilityGroup] = value;
				}
				value.Add(uIInputControl);
			}
		}
		controlVariantSpriteLookup = new Dictionary<UIInputControl.ControlVariant, SpriteVariantInfo>();
		for (int j = 0; j < inputControlVariants.Count; j++)
		{
			controlVariantSpriteLookup[inputControlVariants[j].variant] = inputControlVariants[j];
		}
		activeControlVariants = new Dictionary<UIInputControl.ControlType, UIInputControl.ControlVariant>();
		inputVisibilityHandler = new InputVisibilityHandler();
		inputVisibilityHandler.Init();
		if (leftUniversalMover != null)
		{
			leftUniversalMover.Init(1);
		}
		if (rightUniversalMover != null)
		{
			rightUniversalMover.Init(2);
		}
		_allMovers = new List<UIMover>();
		if (leftMover != null)
		{
			leftMover.SetToInputAxis1();
			_allMovers.Add(leftMover);
		}
		if (rightMover != null)
		{
			rightMover.SetToInputAxis2();
			_allMovers.Add(rightMover);
		}
		for (int k = 0; k < _allMovers.Count; k++)
		{
			_allMovers[k].Init();
			_allMovers[k].SetPointerControlEnabled(mouseAndFingerControlEnabled);
			_allMovers[k].Hide();
		}
		ResetDPad();
		_externalTriggers = new BitArray(UIInputControl.controlTypeCount);
		_canvasScaler = GetComponent<CanvasScaler>();
	}

	public void GetUIObjects(List<GameObject> objectList)
	{
		foreach (UIInputControl value in inputControlLookup.Values)
		{
			objectList.Add(value.gameObject);
		}
		for (int i = 0; i < _allMovers.Count; i++)
		{
			UIMover uIMover = _allMovers[i];
			objectList.Add(uIMover.gameObject);
		}
	}

	public void Hide()
	{
		inputVisibilityHandler.Reset();
		for (int i = 0; i < _allMovers.Count; i++)
		{
			UIMover uIMover = _allMovers[i];
			uIMover.Hide();
			uIMover.SetActive(active: false);
		}
		foreach (UIInputControl value in inputControlLookup.Values)
		{
			value.Hide();
		}
		_externalTriggers.SetAll(value: false);
	}

	public void ResetAllControls()
	{
		ResetDPad();
		ResetInputControls();
		CancelTiltPrompt();
	}

	public void UpdateAll(bool showControls)
	{
		bool flag = false;
		if (leftUniversalMover != null)
		{
			leftUniversalMover.UpdateMover();
			_hasAnyControlBeenPressed |= leftUniversalMover.IsMoving();
			flag |= leftUniversalMover.IsActive();
		}
		for (int i = 0; i < _allMovers.Count; i++)
		{
			UIMover uIMover = _allMovers[i];
			uIMover.UpdateControl();
			_hasAnyControlBeenPressed |= uIMover.IsMoving();
		}
		for (int j = 0; j != _inputControls.Length; j++)
		{
			_inputControls[j].UpdateKeyboardInput();
			_hasAnyControlBeenPressed |= IsControlPressed(_inputControls[j].controlType);
			flag |= _inputControls[j].GetEnabled();
		}
		if (leftMover != null && !_leftMoverInUse)
		{
			leftMover.SetActive(active: false);
		}
		if (rightMover != null && !_rightMoverInUse)
		{
			rightMover.SetActive(active: false);
		}
		flag |= _leftMoverInUse || _rightMoverInUse;
		_leftMoverInUse = (_rightMoverInUse = false);
		if (Time.time > lastTiltPromptShowRequestTime + 0.5f)
		{
			tiltPromptAnimator.SetBool("Show", value: false);
		}
		if (hideActiveControlsAfterTimeout)
		{
			if (showControls || (!_hasAnyControlBeenPressed && flag))
			{
				ResetPrompts();
			}
			if (_showActiveControls)
			{
				UpdateActiveControlsAlphaFade();
			}
		}
	}

	private void ResetPrompts()
	{
		ResetTiltPrompt();
		SetupActiveControlsTimer();
		_hasAnyControlBeenPressed = false;
	}

	public void OnPlay()
	{
		ResetPrompts();
	}

	public void SetupActiveControlsTimer()
	{
		if (hideActiveControlsAfterTimeout)
		{
			_showActiveControls = true;
			_hideActiveControlsTimer = Time.time + hideActiveControlsTimeout + hideActiveControlsFadeoutTimer;
		}
	}

	private void UpdateActiveControlsAlphaFade()
	{
		float num = 0f;
		if (_showActiveControls)
		{
			float time = Time.time;
			num = ((time >= _hideActiveControlsTimer) ? 0f : ((!(time > _hideActiveControlsTimer - hideActiveControlsFadeoutTimer)) ? 1f : ((_hideActiveControlsTimer - time) / hideActiveControlsFadeoutTimer)));
			for (int i = 0; i < _allMovers.Count; i++)
			{
				_allMovers[i].Show();
				RectTransform moverTransform = _allMovers[i].moverTransform;
				CanvasGroup canvasGroup = moverTransform.GetComponent<CanvasGroup>();
				if (canvasGroup == null)
				{
					canvasGroup = moverTransform.gameObject.AddComponent<CanvasGroup>();
				}
				canvasGroup.alpha = num;
			}
			for (int j = 0; j < _inputControls.Length; j++)
			{
				CanvasGroup component = _inputControls[j].GetComponent<CanvasGroup>();
				if (component != null)
				{
					component.alpha = num * ((!_inputControls[j].GetEnabled()) ? 0.25f : 1f);
				}
			}
			if (leftUniversalMover != null)
			{
				leftUniversalMover.UpdateAlphaFade(num);
			}
		}
		_showActiveControls = num > 0f;
	}

	private bool IsControlPressed(UIInputControl.ControlType control)
	{
		bool flag = _externalTriggers[(int)control];
		return flag | (inputVisibilityHandler.IsVisible(control) && inputControlLookup.ContainsKey(control) && inputControlLookup[control].IsPressed());
	}

	public bool IsControlPressed(string controlTypeStr)
	{
		bool result = false;
		if (UIInputControl.controlTypeFromString.TryGetValue(controlTypeStr, out var value))
		{
			result = _externalTriggers[(int)value];
			result |= inputVisibilityHandler.IsVisible(value) && inputControlLookup.ContainsKey(value) && inputControlLookup[value].IsPressed();
		}
		return result;
	}

	public void HandleInputControlVisibility(State gameState)
	{
		if (gameState != State.Play && gameState != State.Paused)
		{
			for (int i = 0; i < _inputControls.Length; i++)
			{
				_inputControls[i].Hide();
			}
			return;
		}
		if (gameState == State.Play)
		{
			inputVisibilityHandler.FixedUpdate();
		}
		_leftMoverSafeX = (_leftMoverSafeY = 0f);
		_rightMoverSafeX = (_rightMoverSafeY = 0f);
		int num = 0;
		int num2 = 0;
		HashSet<int> hashSet = new HashSet<int>();
		foreach (KeyValuePair<UIInputControl.ControlType, UIInputControl> item in inputControlLookup)
		{
			UIInputControl.ControlType key = item.Key;
			UIInputControl value = item.Value;
			bool flag = inputVisibilityHandler.IsVisible(key);
			if (moversOverlapControls && flag)
			{
				switch (key)
				{
				case UIInputControl.ControlType.L:
					_leftMoverSafeX = _moverShiftX * NormalizedScreen.scale;
					break;
				case UIInputControl.ControlType.Left:
				case UIInputControl.ControlType.Right:
					_leftMoverSafeY = _moverShiftY * NormalizedScreen.scale;
					break;
				}
				switch (key)
				{
				case UIInputControl.ControlType.R:
					_rightMoverSafeX = (0f - _moverShiftX) * NormalizedScreen.scale;
					break;
				case UIInputControl.ControlType.Up:
				case UIInputControl.ControlType.Down:
					_rightMoverSafeY = _moverShiftY * NormalizedScreen.scale;
					break;
				}
			}
			value.Show();
			value.SetEnabled(flag);
			if (hideInactiveControls)
			{
				if (flag && value.visibilityGroup > 0)
				{
					hashSet.Add(value.visibilityGroup);
				}
				if (leftUniversalMover != null && leftUniversalMover.visibilityGroup > 0)
				{
					hashSet.Add(leftUniversalMover.visibilityGroup);
				}
			}
			if (flag)
			{
				num2 |= 1 << num;
			}
			num++;
		}
		if ((num2 | _lastVisibleControlMask) != _lastVisibleControlMask)
		{
			SetupActiveControlsTimer();
		}
		_lastVisibleControlMask = num2;
		foreach (int item2 in hashSet)
		{
			foreach (UIInputControl item3 in _visibiltyGroups[item2])
			{
				if (!item3.IsVisible())
				{
					item3.Show();
					item3.SetEnabled(enabled: false);
				}
			}
			if (leftUniversalMover != null && leftUniversalMover.visibilityGroup == item2)
			{
				leftUniversalMover.Show();
			}
		}
		if (moversOverlapControls)
		{
			if (leftMover != null)
			{
				leftMover.AdjustBasePositionOffset(_leftMoverSafeX, _leftMoverSafeY);
			}
			if (rightMover != null)
			{
				rightMover.AdjustBasePositionOffset(_rightMoverSafeX, _rightMoverSafeY);
			}
		}
	}

	public void AddControlFromBlock(string controlStr, Block block)
	{
		if (!UIInputControl.controlTypeFromString.TryGetValue(controlStr, out var value) || !inputControlLookup.TryGetValue(value, out var value2))
		{
			return;
		}
		inputVisibilityHandler.AddBlock(block);
		inputVisibilityHandler.ControlUsedAsSensor(value);
		UIInputControl.ControlVariant value3 = UIInputControl.ControlVariant.Default;
		UIInputControl.controlVariantFromString.TryGetValue(controlStr, out value3);
		if (useKeyImages)
		{
			if (value3 == UIInputControl.ControlVariant.Default)
			{
				value2.HideKeySprite();
			}
			else
			{
				value2.AssignKeySprite(controlVariantSpriteLookup[value3].keySprite);
			}
		}
		else if (value3 == UIInputControl.ControlVariant.Default)
		{
			value2.ResetDefaultSprites();
		}
		else
		{
			value2.OverrideSprite(controlVariantSpriteLookup[value3].sprite);
			value2.OverridePressedSprite(controlVariantSpriteLookup[value3].spritePressed);
		}
	}

	private void ResetInputControls()
	{
		for (int i = 0; i < _inputControls.Length; i++)
		{
			_inputControls[i].ResetInputControl();
		}
	}

	public void ScriptBlockRemoved(Block block)
	{
		inputVisibilityHandler.RemoveBlock(block);
	}

	public Transform GetTransformForControl(UIInputControl.ControlType controlType)
	{
		if (!inputControlLookup.TryGetValue(controlType, out var value))
		{
			return null;
		}
		return (RectTransform)value.transform;
	}

	public Transform GetTransformForLeftMover()
	{
		if (leftUniversalMover != null)
		{
			return leftUniversalMover.moverTransform;
		}
		if (leftMover != null)
		{
			return leftMover.moverTransform;
		}
		return null;
	}

	public void SetControlVariantsFromBlocks(List<Block> blocks)
	{
		HashSet<string> inputNames = GetInputNames(blocks);
		foreach (string item in inputNames)
		{
			if (UIInputControl.controlTypeFromString.TryGetValue(item, out var value) && UIInputControl.controlVariantFromString.TryGetValue(item, out var value2) && value2 != UIInputControl.ControlVariant.Default)
			{
				MapControlToVariant(value, value2);
			}
		}
	}

	private HashSet<string> GetInputNames(List<Block> blocks)
	{
		HashSet<string> hashSet = new HashSet<string>();
		for (int i = 0; i < blocks.Count; i++)
		{
			Block block = blocks[i];
			for (int j = 0; j < block.tiles.Count; j++)
			{
				List<Tile> list = block.tiles[j];
				for (int k = 0; k < list.Count; k++)
				{
					Tile tile = list[k];
					if (tile.gaf.Predicate == Block.predicateButton)
					{
						hashSet.Add((string)tile.gaf.Args[0]);
					}
				}
			}
		}
		return hashSet;
	}

	public void MapControlToVariant(UIInputControl.ControlType control, UIInputControl.ControlVariant variant)
	{
		activeControlVariants[control] = variant;
	}

	public UIInputControl.ControlVariant GetControlVariant(UIInputControl.ControlType control)
	{
		UIInputControl.ControlVariant value = UIInputControl.ControlVariant.Default;
		activeControlVariants.TryGetValue(control, out value);
		return value;
	}

	public void ResetContolVariants()
	{
		activeControlVariants.Clear();
	}

	private void ResetDPad()
	{
		for (int i = 0; i < _allMovers.Count; i++)
		{
			UIMover uIMover = _allMovers[i];
			uIMover.SetActive(active: false);
		}
		if (leftUniversalMover != null)
		{
			leftUniversalMover.Hide();
		}
		_leftMoverInUse = (_rightMoverInUse = false);
	}

	public void EnableDPad(string key, MoverDirectionMask directionMask)
	{
		if (leftUniversalMover != null)
		{
			leftUniversalMover.Show();
			leftUniversalMover.UpdateDirectionMask(directionMask);
			return;
		}
		UIMover moverForKey = GetMoverForKey(key);
		if (moverForKey == leftMover)
		{
			_leftMoverInUse = true;
		}
		else if ((bool)(moverForKey = rightMover))
		{
			_rightMoverInUse = true;
		}
		moverForKey.SetActive(active: true);
		moverForKey.SetDirectionMask(directionMask);
		moverForKey.Show();
	}

	public bool IsDPadActive(string key)
	{
		if (leftUniversalMover != null)
		{
			return leftUniversalMover.gameObject.activeSelf;
		}
		UIMover moverForKey = GetMoverForKey(key);
		return moverForKey.isActiveAndEnabled;
	}

	public Vector2 GetNormalizedDPadOffset(string key)
	{
		if (leftUniversalMover != null)
		{
			return leftUniversalMover.GetNormalizedOffset();
		}
		UIMover moverForKey = GetMoverForKey(key);
		return moverForKey.GetNormalizedOffset();
	}

	public Vector3 GetWorldDPadOffset(string key)
	{
		if (leftUniversalMover != null)
		{
			return leftUniversalMover.GetWorldOffset();
		}
		UIMover moverForKey = GetMoverForKey(key);
		return moverForKey.GetWorldOffset();
	}

	private UIMover GetMoverForKey(string key)
	{
		if (rightMover == null)
		{
			return leftMover;
		}
		if (key == "R")
		{
			return rightMover;
		}
		return leftMover;
	}

	public void ResetTiltPrompt()
	{
		tiltPromptAnimator.SetBool("Show", value: false);
		tiltPromptCancelled = false;
	}

	public void UpdateTiltPrompt()
	{
		if (!tiltPromptCancelled && TiltManager.Instance.IsMonitoring())
		{
			Vector3 normalized = TiltManager.Instance.GetRelativeGravityVector().normalized;
			if (Mathf.Abs(normalized.x) + Mathf.Abs(normalized.y) > 0.35f)
			{
				tiltPromptAnimator.SetBool("Show", value: false);
				tiltPromptCancelled = true;
			}
			else
			{
				lastTiltPromptShowRequestTime = Time.time;
				tiltPromptAnimator.SetBool("Show", value: true);
			}
		}
	}

	private void CancelTiltPrompt()
	{
		tiltPromptAnimator.SetBool("Show", value: false);
		tiltPromptCancelled = true;
	}

	public bool DPadOwnsTouch(int touchId)
	{
		for (int i = 0; i < _allMovers.Count; i++)
		{
			if (_allMovers[i].OwnsTouch(touchId))
			{
				return true;
			}
		}
		return false;
	}

	public bool ControlOwnsTouch(int touchId)
	{
		foreach (UIInputControl value in inputControlLookup.Values)
		{
			if (value.OwnsTouch(touchId))
			{
				return true;
			}
		}
		return false;
	}

	public bool AnyControlActive()
	{
		bool flag = false;
		if (leftUniversalMover != null)
		{
			flag |= leftUniversalMover.IsMoving();
		}
		if (rightUniversalMover != null)
		{
			flag |= rightUniversalMover.IsMoving();
		}
		for (int i = 0; i < _allMovers.Count; i++)
		{
			flag |= _allMovers[i].IsMoving();
		}
		if (!flag)
		{
			foreach (UIInputControl value in inputControlLookup.Values)
			{
				flag |= value.IsPressed();
			}
		}
		return flag;
	}

	public void GetProtectedRects(List<Rect> rects)
	{
		foreach (UIInputControl value in inputControlLookup.Values)
		{
			if (value.gameObject.activeSelf)
			{
				RectTransform rt = (RectTransform)value.transform;
				rects.Add(Util.GetWorldRectForRectTransform(rt));
			}
		}
		if (leftMover != null && leftMover.gameObject.activeSelf)
		{
			rects.Add(Util.GetWorldRectForRectTransform(leftMover.moverTransform));
		}
		if (rightMover != null && rightMover.gameObject.activeSelf)
		{
			rects.Add(Util.GetWorldRectForRectTransform(rightMover.moverTransform));
		}
	}

	public void Layout()
	{
		_canvasScaler.scaleFactor = NormalizedScreen.pixelScale;
		Vector2 touchZoneScale = new Vector2((float)Screen.width / NormalizedScreen.referenceResolution.x, (float)Screen.height / NormalizedScreen.referenceResolution.y);
		for (int i = 0; i < _allMovers.Count; i++)
		{
			_allMovers[i].SetTouchZoneScale(touchZoneScale);
		}
	}

	public void TriggerDefaultAction(bool triggerState)
	{
		int index = 4;
		_externalTriggers[index] = triggerState;
	}
}
