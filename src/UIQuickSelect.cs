using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000317 RID: 791
public class UIQuickSelect : MonoBehaviour
{
	// Token: 0x0600239F RID: 9119 RVA: 0x00106B04 File Offset: 0x00104F04
	public void Init()
	{
		this._defaultHeight = ((RectTransform)base.transform).sizeDelta.y;
		IEnumerator enumerator = this.colorIcon.transform.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Transform transform = (Transform)obj;
				if (transform.name == "CornerOverlay")
				{
					this._colorCornerOverlay = transform.GetComponent<Image>();
				}
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = (enumerator as IDisposable)) != null)
			{
				disposable.Dispose();
			}
		}
		IEnumerator enumerator2 = this.textureIcon.transform.GetEnumerator();
		try
		{
			while (enumerator2.MoveNext())
			{
				object obj2 = enumerator2.Current;
				Transform transform2 = (Transform)obj2;
				if (transform2.name == "CornerOverlay")
				{
					this._textureCornerOverlay = transform2.GetComponent<Image>();
				}
			}
		}
		finally
		{
			IDisposable disposable2;
			if ((disposable2 = (enumerator2 as IDisposable)) != null)
			{
				disposable2.Dispose();
			}
		}
	}

	// Token: 0x060023A0 RID: 9120 RVA: 0x00106C24 File Offset: 0x00105024
	public void ConnectToClipboard(Clipboard clipboard)
	{
		this._clipboard = clipboard;
		this.UpdateIcons();
		this._clipboard.OnSetPaintColor += this.UpdateColorIcon;
		this._clipboard.OnSetTexture += this.UpdateTextureIcon;
		this._clipboard.OnSetModel += this.UpdateModelIcon;
		this._clipboard.OnSetScript += this.UpdateScriptIcon;
		this.SetAutoPaint(clipboard.autoPaintMode);
		this.SetAutoTexture(clipboard.autoTextureMode);
	}

	// Token: 0x060023A1 RID: 9121 RVA: 0x00106CB2 File Offset: 0x001050B2
	public void Show()
	{
		base.gameObject.SetActive(true);
		this.UpdateIcons();
	}

	// Token: 0x060023A2 RID: 9122 RVA: 0x00106CC6 File Offset: 0x001050C6
	public float GetHeight()
	{
		return this._defaultHeight / NormalizedScreen.scale;
	}

	// Token: 0x060023A3 RID: 9123 RVA: 0x00106CD4 File Offset: 0x001050D4
	public void Hide()
	{
		base.gameObject.SetActive(false);
	}

	// Token: 0x060023A4 RID: 9124 RVA: 0x00106CE2 File Offset: 0x001050E2
	public bool Hit(Vector3 v)
	{
		return base.gameObject.activeInHierarchy && Util.RectTransformContains(base.transform, v);
	}

	// Token: 0x060023A5 RID: 9125 RVA: 0x00106D03 File Offset: 0x00105103
	public bool HitPaint(Vector3 v, bool ignoreAvailable = false)
	{
		return base.gameObject.activeInHierarchy && (this.paintAvailable != 0 || ignoreAvailable) && Util.RectTransformContains(this.colorIcon.transform, v);
	}

	// Token: 0x060023A6 RID: 9126 RVA: 0x00106D3A File Offset: 0x0010513A
	public bool HitTexture(Vector3 v, bool ignoreAvailable = false)
	{
		return base.gameObject.activeInHierarchy && (this.textureAvailable != 0 || ignoreAvailable) && Util.RectTransformContains(this.textureIcon.transform, v);
	}

	// Token: 0x060023A7 RID: 9127 RVA: 0x00106D71 File Offset: 0x00105171
	public bool HitModel(Vector3 v, bool ignoreAvailable = false)
	{
		return base.gameObject.activeInHierarchy && (this.modelAvailable != 0 || ignoreAvailable) && Util.RectTransformContains(this.modelIcon.transform, v);
	}

	// Token: 0x060023A8 RID: 9128 RVA: 0x00106DA8 File Offset: 0x001051A8
	public bool HitScript(Vector3 v, bool ignoreAvailable = false)
	{
		return base.gameObject.activeInHierarchy && (this.scriptAvailable != 0 || ignoreAvailable) && Util.RectTransformContains(this.scriptIcon.transform, v);
	}

	// Token: 0x060023A9 RID: 9129 RVA: 0x00106DDF File Offset: 0x001051DF
	public Vector2 PaintRectCenter()
	{
		return Util.CenterOfRectTransform(this.colorIcon.transform);
	}

	// Token: 0x060023AA RID: 9130 RVA: 0x00106DF6 File Offset: 0x001051F6
	public Vector2 TextureRectCenter()
	{
		return Util.CenterOfRectTransform(this.textureIcon.transform);
	}

	// Token: 0x060023AB RID: 9131 RVA: 0x00106E0D File Offset: 0x0010520D
	public Vector2 ModelRectCenter()
	{
		return Util.CenterOfRectTransform(this.modelIcon.transform);
	}

	// Token: 0x060023AC RID: 9132 RVA: 0x00106E24 File Offset: 0x00105224
	public Vector2 ScriptRectCenter()
	{
		return Util.CenterOfRectTransform(this.scriptIcon.transform);
	}

	// Token: 0x060023AD RID: 9133 RVA: 0x00106E3B File Offset: 0x0010523B
	public void UpdateIcons()
	{
		this.UpdateColorIcon();
		this.UpdateTextureIcon();
		this.UpdateModelIcon();
		this.UpdateScriptIcon();
	}

	// Token: 0x060023AE RID: 9134 RVA: 0x00106E55 File Offset: 0x00105255
	public void HideScarcity()
	{
		this.HidePaintScarcity();
		this.HideTextureScarcity();
		this.HideModelScarcity();
		this.HideScriptScarcity();
	}

	// Token: 0x060023AF RID: 9135 RVA: 0x00106E6F File Offset: 0x0010526F
	public void ShowPaintScarcity()
	{
		if (this.paintAvailable >= 0)
		{
			this.colorIcon.ShowQuantity(this.paintAvailable);
		}
	}

	// Token: 0x060023B0 RID: 9136 RVA: 0x00106E8E File Offset: 0x0010528E
	public void HidePaintScarcity()
	{
		this.colorIcon.HideQuantity();
	}

	// Token: 0x060023B1 RID: 9137 RVA: 0x00106E9B File Offset: 0x0010529B
	public void ShowTextureScarcity()
	{
		if (this.textureAvailable >= 0)
		{
			this.textureIcon.ShowQuantity(this.textureAvailable);
		}
	}

	// Token: 0x060023B2 RID: 9138 RVA: 0x00106EBA File Offset: 0x001052BA
	public void HideTextureScarcity()
	{
		this.textureIcon.HideQuantity();
	}

	// Token: 0x060023B3 RID: 9139 RVA: 0x00106EC7 File Offset: 0x001052C7
	public void ShowModelScarcity()
	{
		if (this.modelAvailable >= 0)
		{
			this.modelIcon.ShowQuantity(this.modelAvailable);
		}
	}

	// Token: 0x060023B4 RID: 9140 RVA: 0x00106EE6 File Offset: 0x001052E6
	public void HideModelScarcity()
	{
		this.modelIcon.HideQuantity();
	}

	// Token: 0x060023B5 RID: 9141 RVA: 0x00106EF3 File Offset: 0x001052F3
	public void ShowScriptScarcity()
	{
		if (this.scriptAvailable >= 0)
		{
			this.scriptIcon.ShowQuantity(this.scriptAvailable);
		}
	}

	// Token: 0x060023B6 RID: 9142 RVA: 0x00106F12 File Offset: 0x00105312
	public void HideScriptScarcity()
	{
		this.scriptIcon.HideQuantity();
	}

	// Token: 0x060023B7 RID: 9143 RVA: 0x00106F20 File Offset: 0x00105320
	public void UpdateColorIcon()
	{
		if (this._clipboard == null)
		{
			return;
		}
		GAF lastPaintedColorGAF = this._clipboard.GetLastPaintedColorGAF();
		int inventoryCount = Scarcity.GetInventoryCount(lastPaintedColorGAF, Tutorial.state == TutorialState.None && Scarcity.inventory != null);
		this.paintAvailable = inventoryCount;
		bool enabled = this.paintAvailable != 0 && (Tutorial.state == TutorialState.None || Tutorial.usingQuickSelectColorIcon);
		this.colorIcon.SetGAF(lastPaintedColorGAF);
		this.colorIcon.SetEnabled(enabled);
		this.HidePaintScarcity();
	}

	// Token: 0x060023B8 RID: 9144 RVA: 0x00106FAC File Offset: 0x001053AC
	public void UpdateTextureIcon()
	{
		if (this._clipboard == null)
		{
			return;
		}
		GAF lastTextureGAF = this._clipboard.GetLastTextureGAF();
		this.textureAvailable = Scarcity.GetInventoryCount(lastTextureGAF, Tutorial.state == TutorialState.None && Scarcity.inventory != null);
		bool enabled = this.textureAvailable != 0 && (Tutorial.state == TutorialState.None || Tutorial.usingQuickSelectTextureIcon);
		this.textureIcon.SetGAF(lastTextureGAF);
		this.textureIcon.SetEnabled(enabled);
		this.HideTextureScarcity();
	}

	// Token: 0x060023B9 RID: 9145 RVA: 0x00107034 File Offset: 0x00105434
	public void UpdateTextureIconScarcity()
	{
		if (this._clipboard == null)
		{
			return;
		}
		GAF lastTextureGAF = this._clipboard.GetLastTextureGAF();
		this.textureAvailable = Scarcity.GetInventoryCount(lastTextureGAF, Tutorial.state == TutorialState.None && Scarcity.inventory != null);
		bool enabled = this.textureAvailable != 0 && (Tutorial.state == TutorialState.None || Tutorial.usingQuickSelectTextureIcon);
		this.textureIcon.SetEnabled(enabled);
		this.ShowTextureScarcity();
	}

	// Token: 0x060023BA RID: 9146 RVA: 0x001070B0 File Offset: 0x001054B0
	public void UpdateModelIcon()
	{
		if (this._clipboard == null)
		{
			return;
		}
		if (this._clipboard.modelCopyPasteBuffer == null || this._clipboard.modelCopyPasteBuffer.Count == 0)
		{
			this.modelAvailable = 0;
		}
		else
		{
			this.modelAvailable = this._clipboard.AvailableCopyPasteBufferCount();
		}
		bool enabled = this.modelAvailable != 0 && Tutorial.state == TutorialState.None;
		if (this._clipboard.modelCopyPasteBuffer == null)
		{
			this.AssignCopiedModelIcon(null);
		}
		else
		{
			this.AssignCopiedModelIcon(this._clipboard.copiedModelIcon);
			if (this._clipboard.copiedModelIcon == null || !this._clipboard.copiedModelIconUpToDate)
			{
				this._clipboard.GenerateModelIcon(delegate(Texture2D tex)
				{
					this.AssignCopiedModelIcon(tex);
				});
			}
		}
		this.modelIcon.SetEnabled(enabled);
		this.HideModelScarcity();
	}

	// Token: 0x060023BB RID: 9147 RVA: 0x001071A0 File Offset: 0x001055A0
	public void UpdateScriptIcon()
	{
		if (this._clipboard == null)
		{
			return;
		}
		if (this._clipboard.scriptCopyPasteBuffer == null || this._clipboard.scriptCopyPasteBuffer.Count == 0)
		{
			this.scriptAvailable = 0;
		}
		else
		{
			this.scriptAvailable = this._clipboard.AvailableScriptCount(this._clipboard.scriptCopyPasteBuffer, null, null);
		}
		if (this.scriptIcon.IconName() != UIQuickSelect.scriptButtonIconName)
		{
			this.scriptIcon.SetIcon(UIQuickSelect.scriptButtonIconName);
		}
		bool enabled = this.scriptAvailable != 0 && Tutorial.state == TutorialState.None;
		this.scriptIcon.SetEnabled(enabled);
		this.HideScriptScarcity();
	}

	// Token: 0x060023BC RID: 9148 RVA: 0x0010725B File Offset: 0x0010565B
	public void SetAutoPaint(bool autoPaint)
	{
		this._colorCornerOverlay.sprite = ((!autoPaint) ? this.checkOffSprite : this.checkOnSprite);
		this._colorCornerOverlay.enabled = true;
	}

	// Token: 0x060023BD RID: 9149 RVA: 0x0010728B File Offset: 0x0010568B
	public void SetAutoTexture(bool autoTexture)
	{
		this._textureCornerOverlay.sprite = ((!autoTexture) ? this.checkOffSprite : this.checkOnSprite);
		this._textureCornerOverlay.enabled = true;
	}

	// Token: 0x060023BE RID: 9150 RVA: 0x001072BB File Offset: 0x001056BB
	private void AssignCopiedModelIcon(Texture2D tex)
	{
		if (tex == null)
		{
			this.modelIcon.SetIcon(UIQuickSelect.modelButtonIconName);
		}
		else
		{
			this.modelIcon.SetTexture(tex);
		}
	}

	// Token: 0x060023BF RID: 9151 RVA: 0x001072EC File Offset: 0x001056EC
	public Tile CreatePaintTile()
	{
		GAF lastPaintedColorGAF = this._clipboard.GetLastPaintedColorGAF();
		return new Tile(lastPaintedColorGAF);
	}

	// Token: 0x060023C0 RID: 9152 RVA: 0x00107310 File Offset: 0x00105710
	public Tile CreateTextureTile()
	{
		GAF lastTextureGAF = this._clipboard.GetLastTextureGAF();
		return new Tile(lastTextureGAF);
	}

	// Token: 0x060023C1 RID: 9153 RVA: 0x00107334 File Offset: 0x00105734
	public Tile CreateScriptTile()
	{
		TileObject tileObjectForIcon = Blocksworld.tilePool.GetTileObjectForIcon(UIQuickSelect.scriptButtonIconName, true);
		tileObjectForIcon.Show();
		return new Tile(tileObjectForIcon);
	}

	// Token: 0x060023C2 RID: 9154 RVA: 0x00107360 File Offset: 0x00105760
	public Tile CreateModelTile()
	{
		TileObject tileObjectForIcon = Blocksworld.tilePool.GetTileObjectForIcon(UIQuickSelect.modelButtonIconName, true);
		tileObjectForIcon.Show();
		return new Tile(tileObjectForIcon);
	}

	// Token: 0x060023C3 RID: 9155 RVA: 0x0010738A File Offset: 0x0010578A
	public void SetModelIconScale(float s)
	{
	}

	// Token: 0x060023C4 RID: 9156 RVA: 0x0010738C File Offset: 0x0010578C
	public void SetScriptIconScale(float s)
	{
	}

	// Token: 0x04001ED9 RID: 7897
	public UITile colorIcon;

	// Token: 0x04001EDA RID: 7898
	public UITile textureIcon;

	// Token: 0x04001EDB RID: 7899
	public UITile modelIcon;

	// Token: 0x04001EDC RID: 7900
	public UITile scriptIcon;

	// Token: 0x04001EDD RID: 7901
	public Sprite checkOnSprite;

	// Token: 0x04001EDE RID: 7902
	public Sprite checkOffSprite;

	// Token: 0x04001EDF RID: 7903
	private int paintAvailable;

	// Token: 0x04001EE0 RID: 7904
	private int textureAvailable;

	// Token: 0x04001EE1 RID: 7905
	private int modelAvailable;

	// Token: 0x04001EE2 RID: 7906
	private int scriptAvailable;

	// Token: 0x04001EE3 RID: 7907
	public bool showModelScarcityBadge;

	// Token: 0x04001EE4 RID: 7908
	public bool showScriptScarcityBadge;

	// Token: 0x04001EE5 RID: 7909
	private float _defaultHeight;

	// Token: 0x04001EE6 RID: 7910
	private Clipboard _clipboard;

	// Token: 0x04001EE7 RID: 7911
	private Image _colorCornerOverlay;

	// Token: 0x04001EE8 RID: 7912
	private Image _textureCornerOverlay;

	// Token: 0x04001EE9 RID: 7913
	public static string scriptButtonIconName = "Buttons/Script_Gear";

	// Token: 0x04001EEA RID: 7914
	public static string modelButtonIconName = "Buttons/Copied_Model";
}
