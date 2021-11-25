using System;
using System.Collections;
using System.Collections.Generic;
using Blocks;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020002F9 RID: 761
public class UIControls : MonoBehaviour
{
	// Token: 0x06002254 RID: 8788 RVA: 0x000FFDB4 File Offset: 0x000FE1B4
	public void Init()
	{
		this._inputControls = base.GetComponentsInChildren<UIInputControl>(true);
		this.inputControlLookup = new Dictionary<UIInputControl.ControlType, UIInputControl>();
		this._visibiltyGroups = new Dictionary<int, List<UIInputControl>>();
		for (int i = 0; i < this._inputControls.Length; i++)
		{
			UIInputControl uiinputControl = this._inputControls[i];
			this.inputControlLookup[uiinputControl.controlType] = uiinputControl;
			uiinputControl.Init();
			uiinputControl.SetPointerControlEnabled(this.mouseAndFingerControlEnabled);
			int visibilityGroup = uiinputControl.visibilityGroup;
			if (visibilityGroup > 0)
			{
				List<UIInputControl> list = null;
				if (!this._visibiltyGroups.TryGetValue(visibilityGroup, out list))
				{
					list = new List<UIInputControl>();
					this._visibiltyGroups[visibilityGroup] = list;
				}
				list.Add(uiinputControl);
			}
		}
		this.controlVariantSpriteLookup = new Dictionary<UIInputControl.ControlVariant, UIControls.SpriteVariantInfo>();
		for (int j = 0; j < this.inputControlVariants.Count; j++)
		{
			this.controlVariantSpriteLookup[this.inputControlVariants[j].variant] = this.inputControlVariants[j];
		}
		this.activeControlVariants = new Dictionary<UIInputControl.ControlType, UIInputControl.ControlVariant>();
		this.inputVisibilityHandler = new InputVisibilityHandler();
		this.inputVisibilityHandler.Init();
		if (this.leftUniversalMover != null)
		{
			this.leftUniversalMover.Init(1);
		}
		if (this.rightUniversalMover != null)
		{
			this.rightUniversalMover.Init(2);
		}
		this._allMovers = new List<UIMover>();
		if (this.leftMover != null)
		{
			this.leftMover.SetToInputAxis1();
			this._allMovers.Add(this.leftMover);
		}
		if (this.rightMover != null)
		{
			this.rightMover.SetToInputAxis2();
			this._allMovers.Add(this.rightMover);
		}
		for (int k = 0; k < this._allMovers.Count; k++)
		{
			this._allMovers[k].Init();
			this._allMovers[k].SetPointerControlEnabled(this.mouseAndFingerControlEnabled);
			this._allMovers[k].Hide();
		}
		this.ResetDPad();
		this._externalTriggers = new BitArray(UIInputControl.controlTypeCount);
		this._canvasScaler = base.GetComponent<CanvasScaler>();
	}

	// Token: 0x06002255 RID: 8789 RVA: 0x00100000 File Offset: 0x000FE400
	public void GetUIObjects(List<GameObject> objectList)
	{
		foreach (UIInputControl uiinputControl in this.inputControlLookup.Values)
		{
			objectList.Add(uiinputControl.gameObject);
		}
		for (int i = 0; i < this._allMovers.Count; i++)
		{
			UIMover uimover = this._allMovers[i];
			objectList.Add(uimover.gameObject);
		}
	}

	// Token: 0x06002256 RID: 8790 RVA: 0x0010009C File Offset: 0x000FE49C
	public void Hide()
	{
		this.inputVisibilityHandler.Reset();
		for (int i = 0; i < this._allMovers.Count; i++)
		{
			UIMover uimover = this._allMovers[i];
			uimover.Hide();
			uimover.SetActive(false);
		}
		foreach (UIInputControl uiinputControl in this.inputControlLookup.Values)
		{
			uiinputControl.Hide();
		}
		this._externalTriggers.SetAll(false);
	}

	// Token: 0x06002257 RID: 8791 RVA: 0x0010014C File Offset: 0x000FE54C
	public void ResetAllControls()
	{
		this.ResetDPad();
		this.ResetInputControls();
		this.CancelTiltPrompt();
	}

	// Token: 0x06002258 RID: 8792 RVA: 0x00100160 File Offset: 0x000FE560
	public void UpdateAll(bool showControls)
	{
		bool flag = false;
		if (this.leftUniversalMover != null)
		{
			this.leftUniversalMover.UpdateMover();
			this._hasAnyControlBeenPressed |= this.leftUniversalMover.IsMoving();
			flag |= this.leftUniversalMover.IsActive();
		}
		for (int i = 0; i < this._allMovers.Count; i++)
		{
			UIMover uimover = this._allMovers[i];
			uimover.UpdateControl();
			this._hasAnyControlBeenPressed |= uimover.IsMoving();
		}
		for (int num = 0; num != this._inputControls.Length; num++)
		{
			this._inputControls[num].UpdateKeyboardInput();
			this._hasAnyControlBeenPressed |= this.IsControlPressed(this._inputControls[num].controlType);
			flag |= this._inputControls[num].GetEnabled();
		}
		if (this.leftMover != null && !this._leftMoverInUse)
		{
			this.leftMover.SetActive(false);
		}
		if (this.rightMover != null && !this._rightMoverInUse)
		{
			this.rightMover.SetActive(false);
		}
		flag |= (this._leftMoverInUse || this._rightMoverInUse);
		this._leftMoverInUse = (this._rightMoverInUse = false);
		if (Time.time > this.lastTiltPromptShowRequestTime + 0.5f)
		{
			this.tiltPromptAnimator.SetBool("Show", false);
		}
		if (this.hideActiveControlsAfterTimeout)
		{
			if (showControls || (!this._hasAnyControlBeenPressed && flag))
			{
				this.ResetPrompts();
			}
			if (this._showActiveControls)
			{
				this.UpdateActiveControlsAlphaFade();
			}
		}
	}

	// Token: 0x06002259 RID: 8793 RVA: 0x00100322 File Offset: 0x000FE722
	private void ResetPrompts()
	{
		this.ResetTiltPrompt();
		this.SetupActiveControlsTimer();
		this._hasAnyControlBeenPressed = false;
	}

	// Token: 0x0600225A RID: 8794 RVA: 0x00100337 File Offset: 0x000FE737
	public void OnPlay()
	{
		this.ResetPrompts();
	}

	// Token: 0x0600225B RID: 8795 RVA: 0x0010033F File Offset: 0x000FE73F
	public void SetupActiveControlsTimer()
	{
		if (this.hideActiveControlsAfterTimeout)
		{
			this._showActiveControls = true;
			this._hideActiveControlsTimer = Time.time + this.hideActiveControlsTimeout + this.hideActiveControlsFadeoutTimer;
		}
	}

	// Token: 0x0600225C RID: 8796 RVA: 0x0010036C File Offset: 0x000FE76C
	private void UpdateActiveControlsAlphaFade()
	{
		float num = 0f;
		if (this._showActiveControls)
		{
			float time = Time.time;
			if (time >= this._hideActiveControlsTimer)
			{
				num = 0f;
			}
			else if (time > this._hideActiveControlsTimer - this.hideActiveControlsFadeoutTimer)
			{
				num = (this._hideActiveControlsTimer - time) / this.hideActiveControlsFadeoutTimer;
			}
			else
			{
				num = 1f;
			}
			for (int i = 0; i < this._allMovers.Count; i++)
			{
				this._allMovers[i].Show();
				RectTransform moverTransform = this._allMovers[i].moverTransform;
				CanvasGroup canvasGroup = moverTransform.GetComponent<CanvasGroup>();
				if (canvasGroup == null)
				{
					canvasGroup = moverTransform.gameObject.AddComponent<CanvasGroup>();
				}
				canvasGroup.alpha = num;
			}
			for (int j = 0; j < this._inputControls.Length; j++)
			{
				CanvasGroup component = this._inputControls[j].GetComponent<CanvasGroup>();
				if (component != null)
				{
					component.alpha = num * ((!this._inputControls[j].GetEnabled()) ? 0.25f : 1f);
				}
			}
			if (this.leftUniversalMover != null)
			{
				this.leftUniversalMover.UpdateAlphaFade(num);
			}
		}
		this._showActiveControls = (num > 0f);
	}

	// Token: 0x0600225D RID: 8797 RVA: 0x001004D0 File Offset: 0x000FE8D0
	private bool IsControlPressed(UIInputControl.ControlType control)
	{
		bool flag = this._externalTriggers[(int)control];
		return flag | (this.inputVisibilityHandler.IsVisible(control) && this.inputControlLookup.ContainsKey(control) && this.inputControlLookup[control].IsPressed());
	}

	// Token: 0x0600225E RID: 8798 RVA: 0x00100524 File Offset: 0x000FE924
	public bool IsControlPressed(string controlTypeStr)
	{
		bool flag = false;
		UIInputControl.ControlType controlType;
		if (UIInputControl.controlTypeFromString.TryGetValue(controlTypeStr, out controlType))
		{
			flag = this._externalTriggers[(int)controlType];
			flag |= (this.inputVisibilityHandler.IsVisible(controlType) && this.inputControlLookup.ContainsKey(controlType) && this.inputControlLookup[controlType].IsPressed());
		}
		return flag;
	}

	// Token: 0x0600225F RID: 8799 RVA: 0x0010058C File Offset: 0x000FE98C
	public void HandleInputControlVisibility(State gameState)
	{
		if (gameState != State.Play && gameState != State.Paused)
		{
			for (int i = 0; i < this._inputControls.Length; i++)
			{
				this._inputControls[i].Hide();
			}
			return;
		}
		if (gameState == State.Play)
		{
			this.inputVisibilityHandler.FixedUpdate();
		}
		this._leftMoverSafeX = (this._leftMoverSafeY = 0f);
		this._rightMoverSafeX = (this._rightMoverSafeY = 0f);
		int num = 0;
		int num2 = 0;
		HashSet<int> hashSet = new HashSet<int>();
		foreach (KeyValuePair<UIInputControl.ControlType, UIInputControl> keyValuePair in this.inputControlLookup)
		{
			UIInputControl.ControlType key = keyValuePair.Key;
			UIInputControl value = keyValuePair.Value;
			bool flag = this.inputVisibilityHandler.IsVisible(key);
			if (this.moversOverlapControls && flag)
			{
				if (key == UIInputControl.ControlType.L)
				{
					this._leftMoverSafeX = this._moverShiftX * NormalizedScreen.scale;
				}
				else if (key == UIInputControl.ControlType.Left || key == UIInputControl.ControlType.Right)
				{
					this._leftMoverSafeY = this._moverShiftY * NormalizedScreen.scale;
				}
				if (key == UIInputControl.ControlType.R)
				{
					this._rightMoverSafeX = -this._moverShiftX * NormalizedScreen.scale;
				}
				else if (key == UIInputControl.ControlType.Up || key == UIInputControl.ControlType.Down)
				{
					this._rightMoverSafeY = this._moverShiftY * NormalizedScreen.scale;
				}
			}
			value.Show();
			value.SetEnabled(flag);
			if (this.hideInactiveControls)
			{
				if (flag && value.visibilityGroup > 0)
				{
					hashSet.Add(value.visibilityGroup);
				}
				if (this.leftUniversalMover != null && this.leftUniversalMover.visibilityGroup > 0)
				{
					hashSet.Add(this.leftUniversalMover.visibilityGroup);
				}
			}
			if (flag)
			{
				num2 |= 1 << num;
			}
			num++;
		}
		if ((num2 | this._lastVisibleControlMask) != this._lastVisibleControlMask)
		{
			this.SetupActiveControlsTimer();
		}
		this._lastVisibleControlMask = num2;
		foreach (int num3 in hashSet)
		{
			foreach (UIInputControl uiinputControl in this._visibiltyGroups[num3])
			{
				if (!uiinputControl.IsVisible())
				{
					uiinputControl.Show();
					uiinputControl.SetEnabled(false);
				}
			}
			if (this.leftUniversalMover != null && this.leftUniversalMover.visibilityGroup == num3)
			{
				this.leftUniversalMover.Show();
			}
		}
		if (this.moversOverlapControls)
		{
			if (this.leftMover != null)
			{
				this.leftMover.AdjustBasePositionOffset(this._leftMoverSafeX, this._leftMoverSafeY);
			}
			if (this.rightMover != null)
			{
				this.rightMover.AdjustBasePositionOffset(this._rightMoverSafeX, this._rightMoverSafeY);
			}
		}
	}

	// Token: 0x06002260 RID: 8800 RVA: 0x00100918 File Offset: 0x000FED18
	public void AddControlFromBlock(string controlStr, Block block)
	{
		UIInputControl.ControlType controlType;
		if (!UIInputControl.controlTypeFromString.TryGetValue(controlStr, out controlType))
		{
			return;
		}
		UIInputControl uiinputControl;
		if (!this.inputControlLookup.TryGetValue(controlType, out uiinputControl))
		{
			return;
		}
		this.inputVisibilityHandler.AddBlock(block);
		this.inputVisibilityHandler.ControlUsedAsSensor(controlType);
		UIInputControl.ControlVariant controlVariant = UIInputControl.ControlVariant.Default;
		UIInputControl.controlVariantFromString.TryGetValue(controlStr, out controlVariant);
		if (this.useKeyImages)
		{
			if (controlVariant == UIInputControl.ControlVariant.Default)
			{
				uiinputControl.HideKeySprite();
			}
			else
			{
				uiinputControl.AssignKeySprite(this.controlVariantSpriteLookup[controlVariant].keySprite);
			}
		}
		else if (controlVariant == UIInputControl.ControlVariant.Default)
		{
			uiinputControl.ResetDefaultSprites();
		}
		else
		{
			uiinputControl.OverrideSprite(this.controlVariantSpriteLookup[controlVariant].sprite);
			uiinputControl.OverridePressedSprite(this.controlVariantSpriteLookup[controlVariant].spritePressed);
		}
	}

	// Token: 0x06002261 RID: 8801 RVA: 0x001009F8 File Offset: 0x000FEDF8
	private void ResetInputControls()
	{
		for (int i = 0; i < this._inputControls.Length; i++)
		{
			this._inputControls[i].ResetInputControl();
		}
	}

	// Token: 0x06002262 RID: 8802 RVA: 0x00100A2B File Offset: 0x000FEE2B
	public void ScriptBlockRemoved(Block block)
	{
		this.inputVisibilityHandler.RemoveBlock(block);
	}

	// Token: 0x06002263 RID: 8803 RVA: 0x00100A3C File Offset: 0x000FEE3C
	public Transform GetTransformForControl(UIInputControl.ControlType controlType)
	{
		UIInputControl uiinputControl;
		if (!this.inputControlLookup.TryGetValue(controlType, out uiinputControl))
		{
			return null;
		}
		return (RectTransform)uiinputControl.transform;
	}

	// Token: 0x06002264 RID: 8804 RVA: 0x00100A69 File Offset: 0x000FEE69
	public Transform GetTransformForLeftMover()
	{
		if (this.leftUniversalMover != null)
		{
			return this.leftUniversalMover.moverTransform;
		}
		if (this.leftMover != null)
		{
			return this.leftMover.moverTransform;
		}
		return null;
	}

	// Token: 0x06002265 RID: 8805 RVA: 0x00100AA8 File Offset: 0x000FEEA8
	public void SetControlVariantsFromBlocks(List<Block> blocks)
	{
		HashSet<string> inputNames = this.GetInputNames(blocks);
		foreach (string key in inputNames)
		{
			UIInputControl.ControlType control;
			UIInputControl.ControlVariant controlVariant;
			if (UIInputControl.controlTypeFromString.TryGetValue(key, out control) && UIInputControl.controlVariantFromString.TryGetValue(key, out controlVariant) && controlVariant != UIInputControl.ControlVariant.Default)
			{
				this.MapControlToVariant(control, controlVariant);
			}
		}
	}

	// Token: 0x06002266 RID: 8806 RVA: 0x00100B34 File Offset: 0x000FEF34
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

	// Token: 0x06002267 RID: 8807 RVA: 0x00100BE8 File Offset: 0x000FEFE8
	public void MapControlToVariant(UIInputControl.ControlType control, UIInputControl.ControlVariant variant)
	{
		this.activeControlVariants[control] = variant;
	}

	// Token: 0x06002268 RID: 8808 RVA: 0x00100BF8 File Offset: 0x000FEFF8
	public UIInputControl.ControlVariant GetControlVariant(UIInputControl.ControlType control)
	{
		UIInputControl.ControlVariant result = UIInputControl.ControlVariant.Default;
		this.activeControlVariants.TryGetValue(control, out result);
		return result;
	}

	// Token: 0x06002269 RID: 8809 RVA: 0x00100C17 File Offset: 0x000FF017
	public void ResetContolVariants()
	{
		this.activeControlVariants.Clear();
	}

	// Token: 0x0600226A RID: 8810 RVA: 0x00100C24 File Offset: 0x000FF024
	private void ResetDPad()
	{
		for (int i = 0; i < this._allMovers.Count; i++)
		{
			UIMover uimover = this._allMovers[i];
			uimover.SetActive(false);
		}
		if (this.leftUniversalMover != null)
		{
			this.leftUniversalMover.Hide();
		}
		this._leftMoverInUse = (this._rightMoverInUse = false);
	}

	// Token: 0x0600226B RID: 8811 RVA: 0x00100C90 File Offset: 0x000FF090
	public void EnableDPad(string key, MoverDirectionMask directionMask)
	{
		if (this.leftUniversalMover != null)
		{
			this.leftUniversalMover.Show();
			this.leftUniversalMover.UpdateDirectionMask(directionMask);
		}
		else
		{
			UIMover moverForKey = this.GetMoverForKey(key);
			if (moverForKey == this.leftMover)
			{
				this._leftMoverInUse = true;
			}
			else if (moverForKey = this.rightMover)
			{
				this._rightMoverInUse = true;
			}
			moverForKey.SetActive(true);
			moverForKey.SetDirectionMask(directionMask);
			moverForKey.Show();
		}
	}

	// Token: 0x0600226C RID: 8812 RVA: 0x00100D1C File Offset: 0x000FF11C
	public bool IsDPadActive(string key)
	{
		if (this.leftUniversalMover != null)
		{
			return this.leftUniversalMover.gameObject.activeSelf;
		}
		UIMover moverForKey = this.GetMoverForKey(key);
		return moverForKey.isActiveAndEnabled;
	}

	// Token: 0x0600226D RID: 8813 RVA: 0x00100D5C File Offset: 0x000FF15C
	public Vector2 GetNormalizedDPadOffset(string key)
	{
		if (this.leftUniversalMover != null)
		{
			return this.leftUniversalMover.GetNormalizedOffset();
		}
		UIMover moverForKey = this.GetMoverForKey(key);
		return moverForKey.GetNormalizedOffset();
	}

	// Token: 0x0600226E RID: 8814 RVA: 0x00100D94 File Offset: 0x000FF194
	public Vector3 GetWorldDPadOffset(string key)
	{
		if (this.leftUniversalMover != null)
		{
			return this.leftUniversalMover.GetWorldOffset();
		}
		UIMover moverForKey = this.GetMoverForKey(key);
		return moverForKey.GetWorldOffset();
	}

	// Token: 0x0600226F RID: 8815 RVA: 0x00100DCC File Offset: 0x000FF1CC
	private UIMover GetMoverForKey(string key)
	{
		if (this.rightMover == null)
		{
			return this.leftMover;
		}
		if (key == "R")
		{
			return this.rightMover;
		}
		return this.leftMover;
	}

	// Token: 0x06002270 RID: 8816 RVA: 0x00100E03 File Offset: 0x000FF203
	public void ResetTiltPrompt()
	{
		this.tiltPromptAnimator.SetBool("Show", false);
		this.tiltPromptCancelled = false;
	}

	// Token: 0x06002271 RID: 8817 RVA: 0x00100E20 File Offset: 0x000FF220
	public void UpdateTiltPrompt()
	{
		if (this.tiltPromptCancelled || !TiltManager.Instance.IsMonitoring())
		{
			return;
		}
		Vector3 normalized = TiltManager.Instance.GetRelativeGravityVector().normalized;
		bool flag = Mathf.Abs(normalized.x) + Mathf.Abs(normalized.y) > 0.35f;
		if (flag)
		{
			this.tiltPromptAnimator.SetBool("Show", false);
			this.tiltPromptCancelled = true;
		}
		else
		{
			this.lastTiltPromptShowRequestTime = Time.time;
			this.tiltPromptAnimator.SetBool("Show", true);
		}
	}

	// Token: 0x06002272 RID: 8818 RVA: 0x00100EBB File Offset: 0x000FF2BB
	private void CancelTiltPrompt()
	{
		this.tiltPromptAnimator.SetBool("Show", false);
		this.tiltPromptCancelled = true;
	}

	// Token: 0x06002273 RID: 8819 RVA: 0x00100ED8 File Offset: 0x000FF2D8
	public bool DPadOwnsTouch(int touchId)
	{
		for (int i = 0; i < this._allMovers.Count; i++)
		{
			if (this._allMovers[i].OwnsTouch(touchId))
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06002274 RID: 8820 RVA: 0x00100F1C File Offset: 0x000FF31C
	public bool ControlOwnsTouch(int touchId)
	{
		foreach (UIInputControl uiinputControl in this.inputControlLookup.Values)
		{
			if (uiinputControl.OwnsTouch(touchId))
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06002275 RID: 8821 RVA: 0x00100F8C File Offset: 0x000FF38C
	public bool AnyControlActive()
	{
		bool flag = false;
		if (this.leftUniversalMover != null)
		{
			flag |= this.leftUniversalMover.IsMoving();
		}
		if (this.rightUniversalMover != null)
		{
			flag |= this.rightUniversalMover.IsMoving();
		}
		for (int i = 0; i < this._allMovers.Count; i++)
		{
			flag |= this._allMovers[i].IsMoving();
		}
		if (!flag)
		{
			foreach (UIInputControl uiinputControl in this.inputControlLookup.Values)
			{
				flag |= uiinputControl.IsPressed();
			}
		}
		return flag;
	}

	// Token: 0x06002276 RID: 8822 RVA: 0x00101068 File Offset: 0x000FF468
	public void GetProtectedRects(List<Rect> rects)
	{
		foreach (UIInputControl uiinputControl in this.inputControlLookup.Values)
		{
			if (uiinputControl.gameObject.activeSelf)
			{
				RectTransform rt = (RectTransform)uiinputControl.transform;
				rects.Add(Util.GetWorldRectForRectTransform(rt));
			}
		}
		if (this.leftMover != null && this.leftMover.gameObject.activeSelf)
		{
			rects.Add(Util.GetWorldRectForRectTransform(this.leftMover.moverTransform));
		}
		if (this.rightMover != null && this.rightMover.gameObject.activeSelf)
		{
			rects.Add(Util.GetWorldRectForRectTransform(this.rightMover.moverTransform));
		}
	}

	// Token: 0x06002277 RID: 8823 RVA: 0x00101164 File Offset: 0x000FF564
	public void Layout()
	{
		this._canvasScaler.scaleFactor = NormalizedScreen.pixelScale;
		Vector2 touchZoneScale = new Vector2((float)Screen.width / NormalizedScreen.referenceResolution.x, (float)Screen.height / NormalizedScreen.referenceResolution.y);
		for (int i = 0; i < this._allMovers.Count; i++)
		{
			this._allMovers[i].SetTouchZoneScale(touchZoneScale);
		}
	}

	// Token: 0x06002278 RID: 8824 RVA: 0x001011E0 File Offset: 0x000FF5E0
	public void TriggerDefaultAction(bool triggerState)
	{
		int index = 4;
		this._externalTriggers[index] = triggerState;
	}

	// Token: 0x04001D59 RID: 7513
	public List<UIControls.SpriteVariantInfo> inputControlVariants;

	// Token: 0x04001D5A RID: 7514
	public UIMoverUniversal leftUniversalMover;

	// Token: 0x04001D5B RID: 7515
	public UIMoverUniversal rightUniversalMover;

	// Token: 0x04001D5C RID: 7516
	public UIMover leftMover;

	// Token: 0x04001D5D RID: 7517
	public UIMover rightMover;

	// Token: 0x04001D5E RID: 7518
	public bool moversOverlapControls = true;

	// Token: 0x04001D5F RID: 7519
	public bool mouseAndFingerControlEnabled = true;

	// Token: 0x04001D60 RID: 7520
	public bool useKeyImages;

	// Token: 0x04001D61 RID: 7521
	public bool hideInactiveControls = true;

	// Token: 0x04001D62 RID: 7522
	public bool hideActiveControlsAfterTimeout;

	// Token: 0x04001D63 RID: 7523
	public float hideActiveControlsTimeout = 5f;

	// Token: 0x04001D64 RID: 7524
	public float hideActiveControlsFadeoutTimer = 1f;

	// Token: 0x04001D65 RID: 7525
	public Animator tiltPromptAnimator;

	// Token: 0x04001D66 RID: 7526
	private bool _hasAnyControlBeenPressed;

	// Token: 0x04001D67 RID: 7527
	private float _leftMoverSafeX;

	// Token: 0x04001D68 RID: 7528
	private float _leftMoverSafeY;

	// Token: 0x04001D69 RID: 7529
	private float _rightMoverSafeX;

	// Token: 0x04001D6A RID: 7530
	private float _rightMoverSafeY;

	// Token: 0x04001D6B RID: 7531
	private float _moverShiftY = 50f;

	// Token: 0x04001D6C RID: 7532
	private float _moverShiftX = 65f;

	// Token: 0x04001D6D RID: 7533
	private bool _leftMoverInUse;

	// Token: 0x04001D6E RID: 7534
	private bool _rightMoverInUse;

	// Token: 0x04001D6F RID: 7535
	private bool _showActiveControls;

	// Token: 0x04001D70 RID: 7536
	private float _hideActiveControlsTimer;

	// Token: 0x04001D71 RID: 7537
	private int _lastVisibleControlMask;

	// Token: 0x04001D72 RID: 7538
	private UIInputControl[] _inputControls;

	// Token: 0x04001D73 RID: 7539
	private Dictionary<UIInputControl.ControlType, UIInputControl> inputControlLookup;

	// Token: 0x04001D74 RID: 7540
	private Dictionary<UIInputControl.ControlVariant, UIControls.SpriteVariantInfo> controlVariantSpriteLookup;

	// Token: 0x04001D75 RID: 7541
	private Dictionary<UIInputControl.ControlType, UIInputControl.ControlVariant> activeControlVariants;

	// Token: 0x04001D76 RID: 7542
	private InputVisibilityHandler inputVisibilityHandler;

	// Token: 0x04001D77 RID: 7543
	private List<UIMover> _allMovers = new List<UIMover>();

	// Token: 0x04001D78 RID: 7544
	private CanvasScaler _canvasScaler;

	// Token: 0x04001D79 RID: 7545
	private BitArray _externalTriggers;

	// Token: 0x04001D7A RID: 7546
	private Dictionary<int, List<UIInputControl>> _visibiltyGroups;

	// Token: 0x04001D7B RID: 7547
	private bool tiltPromptCancelled;

	// Token: 0x04001D7C RID: 7548
	private float lastTiltPromptShowRequestTime;

	// Token: 0x020002FA RID: 762
	[Serializable]
	public struct SpriteVariantInfo
	{
		// Token: 0x04001D7D RID: 7549
		public UIInputControl.ControlVariant variant;

		// Token: 0x04001D7E RID: 7550
		public Sprite sprite;

		// Token: 0x04001D7F RID: 7551
		public Sprite spritePressed;

		// Token: 0x04001D80 RID: 7552
		public Sprite keySprite;
	}
}
