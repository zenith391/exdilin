using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000326 RID: 806
public class UIWorldInfo : MonoBehaviour
{
	// Token: 0x06002483 RID: 9347 RVA: 0x0010AEC5 File Offset: 0x001092C5
	public void Init()
	{
		this.authorUsernameText.enabled = this.showAuthor;
		this.authorLabel.enabled = this.showAuthor;
	}

	// Token: 0x06002484 RID: 9348 RVA: 0x0010AEEC File Offset: 0x001092EC
	public void LoadWorldInfo(WorldInfo worldInfo)
	{
		this._worldInfo = worldInfo;
		if (string.IsNullOrEmpty(worldInfo.title))
		{
			this.titleText.supportRichText = true;
			this.titleText.text = "Untitled  <color=#ffffff99><i>(id: " + worldInfo.id + ")</i></color>";
		}
		else
		{
			this.titleText.supportRichText = false;
			this.titleText.text = worldInfo.title;
		}
		this.authorUsernameText.text = worldInfo.authorUserName;
		this.titleText.enabled = true;
		this.authorUsernameText.enabled = this.showAuthor;
		this.titleLabel.enabled = true;
		this.authorLabel.enabled = this.showAuthor;
		this.notLoadedText.enabled = false;
		this.worldThumbnailView.texture = null;
		if (!string.IsNullOrEmpty(worldInfo.thumbnailUrl))
		{
			this.worldThumbnailView.texture = this._worldInfo.GetWorldThumbnail();
			worldInfo.ThumbnailLoaded += this.WorldInfoLoadedScreenshot;
			worldInfo.LoadThumbnail();
			this._linkedToWorldInfoScreenshot = true;
		}
	}

	// Token: 0x06002485 RID: 9349 RVA: 0x0010B008 File Offset: 0x00109408
	public void ClearWorldInfo()
	{
		if (this._linkedToWorldInfoScreenshot)
		{
			this.worldThumbnailView.texture = null;
			this._worldInfo.ThumbnailLoaded -= this.WorldInfoLoadedScreenshot;
			this._linkedToWorldInfoScreenshot = false;
		}
		this._worldInfo = null;
		this.titleText.enabled = false;
		this.authorUsernameText.enabled = false;
		this.titleLabel.enabled = false;
		this.authorLabel.enabled = false;
		this.notLoadedText.enabled = true;
		this.notLoadedText.text = "World not found";
	}

	// Token: 0x06002486 RID: 9350 RVA: 0x0010B09D File Offset: 0x0010949D
	public void Show()
	{
		base.gameObject.SetActive(true);
	}

	// Token: 0x06002487 RID: 9351 RVA: 0x0010B0AB File Offset: 0x001094AB
	public void Hide()
	{
		base.gameObject.SetActive(false);
	}

	// Token: 0x06002488 RID: 9352 RVA: 0x0010B0B9 File Offset: 0x001094B9
	private void WorldInfoLoadedScreenshot(object sender, EventArgs e)
	{
		if (sender != this._worldInfo)
		{
			return;
		}
		this.worldThumbnailView.texture = this._worldInfo.GetWorldThumbnail();
	}

	// Token: 0x04001F62 RID: 8034
	public Text titleText;

	// Token: 0x04001F63 RID: 8035
	public Text authorUsernameText;

	// Token: 0x04001F64 RID: 8036
	public Text titleLabel;

	// Token: 0x04001F65 RID: 8037
	public Text authorLabel;

	// Token: 0x04001F66 RID: 8038
	public Text notLoadedText;

	// Token: 0x04001F67 RID: 8039
	public RawImage worldThumbnailView;

	// Token: 0x04001F68 RID: 8040
	public bool showAuthor;

	// Token: 0x04001F69 RID: 8041
	private WorldInfo _worldInfo;

	// Token: 0x04001F6A RID: 8042
	private bool _linkedToWorldInfoScreenshot;
}
