using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelElementBlockItemIcon : UIPanelElement
{
	public RawImage backgroundImage;

	public RawImage mainIconImage;

	private TileIconHandle iconLoader;

	private Texture2D iconTexture;

	private float baseSize;

	public override void Init(UIPanelContents parentPanel)
	{
		base.Init(parentPanel);
		if (iconLoader == null)
		{
			iconLoader = TileIconManager.CreateTileIconHandle();
		}
		if (iconTexture == null)
		{
			iconTexture = new Texture2D(1, 1, TextureFormat.RGBA32, mipmap: false);
		}
		backgroundImage.enabled = false;
		mainIconImage.enabled = false;
		baseSize = mainIconImage.rectTransform.sizeDelta.x;
	}

	public override void Clear()
	{
		base.Clear();
		if (iconLoader != null)
		{
			iconLoader.CancelLoad();
		}
	}

	public void OnDestroy()
	{
		if (iconLoader != null)
		{
			iconLoader.CancelLoad();
		}
		if (iconTexture != null)
		{
			Object.Destroy(iconTexture);
		}
	}

	public override void Fill(Dictionary<string, string> data)
	{
		base.Fill(data);
		string value = string.Empty;
		if (data.TryGetValue("blockItemIdentifier", out value))
		{
			BlockItem blockItem = BlockItem.FindByInternalIdentifier(value);
			TileIconInfo tileInfo = TileIconManager.Instance.GetTileInfo(blockItem);
			if (tileInfo.clearBackground)
			{
				backgroundImage.enabled = false;
			}
			else
			{
				Color[] colors = Blocksworld.GetColors(tileInfo.backgroundColorName);
				backgroundImage.enabled = true;
				backgroundImage.color = colors[0];
			}
			iconLoader.SetFilePath(tileInfo.filePath);
			TileIconManager.Instance.RequestIconLoad(tileInfo.filePath, iconLoader);
		}
	}

	private void Update()
	{
		if (iconLoader != null && iconLoader.loadState == TileIconLoadState.Loaded)
		{
			iconLoader.ApplyTexture(iconTexture);
			ApplyTexture();
		}
	}

	private void ApplyTexture()
	{
		mainIconImage.texture = iconTexture;
		mainIconImage.enabled = true;
		float x = (float)iconTexture.width / 134f;
		float y = (float)iconTexture.height / 134f;
		mainIconImage.rectTransform.localScale = new Vector3(x, y, 1f);
		mainIconImage.enabled = true;
	}
}
