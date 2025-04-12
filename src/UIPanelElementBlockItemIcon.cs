using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000414 RID: 1044
public class UIPanelElementBlockItemIcon : UIPanelElement
{
	// Token: 0x06002D6A RID: 11626 RVA: 0x00143E40 File Offset: 0x00142240
	public override void Init(UIPanelContents parentPanel)
	{
		base.Init(parentPanel);
		if (this.iconLoader == null)
		{
			this.iconLoader = TileIconManager.CreateTileIconHandle();
		}
		if (this.iconTexture == null)
		{
			this.iconTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
		}
		this.backgroundImage.enabled = false;
		this.mainIconImage.enabled = false;
		this.baseSize = this.mainIconImage.rectTransform.sizeDelta.x;
	}

	// Token: 0x06002D6B RID: 11627 RVA: 0x00143EC0 File Offset: 0x001422C0
	public override void Clear()
	{
		base.Clear();
		if (this.iconLoader != null)
		{
			this.iconLoader.CancelLoad();
		}
	}

	// Token: 0x06002D6C RID: 11628 RVA: 0x00143EDE File Offset: 0x001422DE
	public void OnDestroy()
	{
		if (this.iconLoader != null)
		{
			this.iconLoader.CancelLoad();
		}
		if (this.iconTexture != null)
		{
			UnityEngine.Object.Destroy(this.iconTexture);
		}
	}

	// Token: 0x06002D6D RID: 11629 RVA: 0x00143F14 File Offset: 0x00142314
	public override void Fill(Dictionary<string, string> data)
	{
		base.Fill(data);
		string empty = string.Empty;
		if (data.TryGetValue("blockItemIdentifier", out empty))
		{
			BlockItem blockItem = BlockItem.FindByInternalIdentifier(empty);
			TileIconInfo tileInfo = TileIconManager.Instance.GetTileInfo(blockItem);
			if (tileInfo.clearBackground)
			{
				this.backgroundImage.enabled = false;
			}
			else
			{
				Color[] colors = Blocksworld.GetColors(tileInfo.backgroundColorName);
				this.backgroundImage.enabled = true;
				this.backgroundImage.color = colors[0];
			}
			this.iconLoader.SetFilePath(tileInfo.filePath);
			TileIconManager.Instance.RequestIconLoad(tileInfo.filePath, this.iconLoader);
		}
	}

	// Token: 0x06002D6E RID: 11630 RVA: 0x00143FC6 File Offset: 0x001423C6
	private void Update()
	{
		if (this.iconLoader != null && this.iconLoader.loadState == TileIconLoadState.Loaded)
		{
			this.iconLoader.ApplyTexture(this.iconTexture);
			this.ApplyTexture();
		}
	}

	// Token: 0x06002D6F RID: 11631 RVA: 0x00143FFC File Offset: 0x001423FC
	private void ApplyTexture()
	{
		this.mainIconImage.texture = this.iconTexture;
		this.mainIconImage.enabled = true;
		float x = (float)this.iconTexture.width / 134f;
		float y = (float)this.iconTexture.height / 134f;
		this.mainIconImage.rectTransform.localScale = new Vector3(x, y, 1f);
		this.mainIconImage.enabled = true;
	}

	// Token: 0x040025ED RID: 9709
	public RawImage backgroundImage;

	// Token: 0x040025EE RID: 9710
	public RawImage mainIconImage;

	// Token: 0x040025EF RID: 9711
	private TileIconHandle iconLoader;

	// Token: 0x040025F0 RID: 9712
	private Texture2D iconTexture;

	// Token: 0x040025F1 RID: 9713
	private float baseSize;
}
