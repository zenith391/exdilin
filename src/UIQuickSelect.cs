using UnityEngine;
using UnityEngine.UI;

public class UIQuickSelect : MonoBehaviour
{
	public UITile colorIcon;

	public UITile textureIcon;

	public UITile modelIcon;

	public UITile scriptIcon;

	public Sprite checkOnSprite;

	public Sprite checkOffSprite;

	private int paintAvailable;

	private int textureAvailable;

	private int modelAvailable;

	private int scriptAvailable;

	public bool showModelScarcityBadge;

	public bool showScriptScarcityBadge;

	private float _defaultHeight;

	private Clipboard _clipboard;

	private Image _colorCornerOverlay;

	private Image _textureCornerOverlay;

	public static string scriptButtonIconName = "Buttons/Script_Gear";

	public static string modelButtonIconName = "Buttons/Copied_Model";

	public void Init()
	{
		_defaultHeight = ((RectTransform)base.transform).sizeDelta.y;
		foreach (object item in colorIcon.transform)
		{
			Transform transform = (Transform)item;
			if (transform.name == "CornerOverlay")
			{
				_colorCornerOverlay = transform.GetComponent<Image>();
			}
		}
		foreach (object item2 in textureIcon.transform)
		{
			Transform transform2 = (Transform)item2;
			if (transform2.name == "CornerOverlay")
			{
				_textureCornerOverlay = transform2.GetComponent<Image>();
			}
		}
	}

	public void ConnectToClipboard(Clipboard clipboard)
	{
		_clipboard = clipboard;
		UpdateIcons();
		_clipboard.OnSetPaintColor += UpdateColorIcon;
		_clipboard.OnSetTexture += UpdateTextureIcon;
		_clipboard.OnSetModel += UpdateModelIcon;
		_clipboard.OnSetScript += UpdateScriptIcon;
		SetAutoPaint(clipboard.autoPaintMode);
		SetAutoTexture(clipboard.autoTextureMode);
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
		UpdateIcons();
	}

	public float GetHeight()
	{
		return _defaultHeight / NormalizedScreen.scale;
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public bool Hit(Vector3 v)
	{
		if (base.gameObject.activeInHierarchy)
		{
			return Util.RectTransformContains(base.transform, v);
		}
		return false;
	}

	public bool HitPaint(Vector3 v, bool ignoreAvailable = false)
	{
		if (base.gameObject.activeInHierarchy && (paintAvailable != 0 || ignoreAvailable))
		{
			return Util.RectTransformContains(colorIcon.transform, v);
		}
		return false;
	}

	public bool HitTexture(Vector3 v, bool ignoreAvailable = false)
	{
		if (base.gameObject.activeInHierarchy && (textureAvailable != 0 || ignoreAvailable))
		{
			return Util.RectTransformContains(textureIcon.transform, v);
		}
		return false;
	}

	public bool HitModel(Vector3 v, bool ignoreAvailable = false)
	{
		if (base.gameObject.activeInHierarchy && (modelAvailable != 0 || ignoreAvailable))
		{
			return Util.RectTransformContains(modelIcon.transform, v);
		}
		return false;
	}

	public bool HitScript(Vector3 v, bool ignoreAvailable = false)
	{
		if (base.gameObject.activeInHierarchy && (scriptAvailable != 0 || ignoreAvailable))
		{
			return Util.RectTransformContains(scriptIcon.transform, v);
		}
		return false;
	}

	public Vector2 PaintRectCenter()
	{
		return Util.CenterOfRectTransform(colorIcon.transform);
	}

	public Vector2 TextureRectCenter()
	{
		return Util.CenterOfRectTransform(textureIcon.transform);
	}

	public Vector2 ModelRectCenter()
	{
		return Util.CenterOfRectTransform(modelIcon.transform);
	}

	public Vector2 ScriptRectCenter()
	{
		return Util.CenterOfRectTransform(scriptIcon.transform);
	}

	public void UpdateIcons()
	{
		UpdateColorIcon();
		UpdateTextureIcon();
		UpdateModelIcon();
		UpdateScriptIcon();
	}

	public void HideScarcity()
	{
		HidePaintScarcity();
		HideTextureScarcity();
		HideModelScarcity();
		HideScriptScarcity();
	}

	public void ShowPaintScarcity()
	{
		if (paintAvailable >= 0)
		{
			colorIcon.ShowQuantity(paintAvailable);
		}
	}

	public void HidePaintScarcity()
	{
		colorIcon.HideQuantity();
	}

	public void ShowTextureScarcity()
	{
		if (textureAvailable >= 0)
		{
			textureIcon.ShowQuantity(textureAvailable);
		}
	}

	public void HideTextureScarcity()
	{
		textureIcon.HideQuantity();
	}

	public void ShowModelScarcity()
	{
		if (modelAvailable >= 0)
		{
			modelIcon.ShowQuantity(modelAvailable);
		}
	}

	public void HideModelScarcity()
	{
		modelIcon.HideQuantity();
	}

	public void ShowScriptScarcity()
	{
		if (scriptAvailable >= 0)
		{
			scriptIcon.ShowQuantity(scriptAvailable);
		}
	}

	public void HideScriptScarcity()
	{
		scriptIcon.HideQuantity();
	}

	public void UpdateColorIcon()
	{
		if (_clipboard != null)
		{
			GAF lastPaintedColorGAF = _clipboard.GetLastPaintedColorGAF();
			int inventoryCount = Scarcity.GetInventoryCount(lastPaintedColorGAF, Tutorial.state == TutorialState.None && Scarcity.inventory != null);
			paintAvailable = inventoryCount;
			bool flag = paintAvailable != 0 && (Tutorial.state == TutorialState.None || Tutorial.usingQuickSelectColorIcon);
			colorIcon.SetGAF(lastPaintedColorGAF);
			colorIcon.SetEnabled(flag);
			HidePaintScarcity();
		}
	}

	public void UpdateTextureIcon()
	{
		if (_clipboard != null)
		{
			GAF lastTextureGAF = _clipboard.GetLastTextureGAF();
			textureAvailable = Scarcity.GetInventoryCount(lastTextureGAF, Tutorial.state == TutorialState.None && Scarcity.inventory != null);
			bool flag = textureAvailable != 0 && (Tutorial.state == TutorialState.None || Tutorial.usingQuickSelectTextureIcon);
			textureIcon.SetGAF(lastTextureGAF);
			textureIcon.SetEnabled(flag);
			HideTextureScarcity();
		}
	}

	public void UpdateTextureIconScarcity()
	{
		if (_clipboard != null)
		{
			GAF lastTextureGAF = _clipboard.GetLastTextureGAF();
			textureAvailable = Scarcity.GetInventoryCount(lastTextureGAF, Tutorial.state == TutorialState.None && Scarcity.inventory != null);
			bool flag = textureAvailable != 0 && (Tutorial.state == TutorialState.None || Tutorial.usingQuickSelectTextureIcon);
			textureIcon.SetEnabled(flag);
			ShowTextureScarcity();
		}
	}

	public void UpdateModelIcon()
	{
		if (_clipboard == null)
		{
			return;
		}
		if (_clipboard.modelCopyPasteBuffer == null || _clipboard.modelCopyPasteBuffer.Count == 0)
		{
			modelAvailable = 0;
		}
		else
		{
			modelAvailable = _clipboard.AvailableCopyPasteBufferCount();
		}
		bool flag = modelAvailable != 0 && Tutorial.state == TutorialState.None;
		if (_clipboard.modelCopyPasteBuffer == null)
		{
			AssignCopiedModelIcon(null);
		}
		else
		{
			AssignCopiedModelIcon(_clipboard.copiedModelIcon);
			if (_clipboard.copiedModelIcon == null || !_clipboard.copiedModelIconUpToDate)
			{
				_clipboard.GenerateModelIcon(delegate(Texture2D tex)
				{
					AssignCopiedModelIcon(tex);
				});
			}
		}
		modelIcon.SetEnabled(flag);
		HideModelScarcity();
	}

	public void UpdateScriptIcon()
	{
		if (_clipboard != null)
		{
			if (_clipboard.scriptCopyPasteBuffer == null || _clipboard.scriptCopyPasteBuffer.Count == 0)
			{
				scriptAvailable = 0;
			}
			else
			{
				scriptAvailable = _clipboard.AvailableScriptCount(_clipboard.scriptCopyPasteBuffer);
			}
			if (scriptIcon.IconName() != scriptButtonIconName)
			{
				scriptIcon.SetIcon(scriptButtonIconName);
			}
			bool flag = scriptAvailable != 0 && Tutorial.state == TutorialState.None;
			scriptIcon.SetEnabled(flag);
			HideScriptScarcity();
		}
	}

	public void SetAutoPaint(bool autoPaint)
	{
		_colorCornerOverlay.sprite = ((!autoPaint) ? checkOffSprite : checkOnSprite);
		_colorCornerOverlay.enabled = true;
	}

	public void SetAutoTexture(bool autoTexture)
	{
		_textureCornerOverlay.sprite = ((!autoTexture) ? checkOffSprite : checkOnSprite);
		_textureCornerOverlay.enabled = true;
	}

	private void AssignCopiedModelIcon(Texture2D tex)
	{
		if (tex == null)
		{
			modelIcon.SetIcon(modelButtonIconName);
		}
		else
		{
			modelIcon.SetTexture(tex);
		}
	}

	public Tile CreatePaintTile()
	{
		GAF lastPaintedColorGAF = _clipboard.GetLastPaintedColorGAF();
		return new Tile(lastPaintedColorGAF);
	}

	public Tile CreateTextureTile()
	{
		GAF lastTextureGAF = _clipboard.GetLastTextureGAF();
		return new Tile(lastTextureGAF);
	}

	public Tile CreateScriptTile()
	{
		TileObject tileObjectForIcon = Blocksworld.tilePool.GetTileObjectForIcon(scriptButtonIconName, enabled: true);
		tileObjectForIcon.Show();
		return new Tile(tileObjectForIcon);
	}

	public Tile CreateModelTile()
	{
		TileObject tileObjectForIcon = Blocksworld.tilePool.GetTileObjectForIcon(modelButtonIconName, enabled: true);
		tileObjectForIcon.Show();
		return new Tile(tileObjectForIcon);
	}

	public void SetModelIconScale(float s)
	{
	}

	public void SetScriptIconScale(float s)
	{
	}
}
