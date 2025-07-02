using System;
using UnityEngine;
using UnityEngine.UI;

public class UIWorldInfo : MonoBehaviour
{
	public Text titleText;

	public Text authorUsernameText;

	public Text titleLabel;

	public Text authorLabel;

	public Text notLoadedText;

	public RawImage worldThumbnailView;

	public bool showAuthor;

	private WorldInfo _worldInfo;

	private bool _linkedToWorldInfoScreenshot;

	public void Init()
	{
		authorUsernameText.enabled = showAuthor;
		authorLabel.enabled = showAuthor;
	}

	public void LoadWorldInfo(WorldInfo worldInfo)
	{
		_worldInfo = worldInfo;
		if (string.IsNullOrEmpty(worldInfo.title))
		{
			titleText.supportRichText = true;
			titleText.text = "Untitled  <color=#ffffff99><i>(id: " + worldInfo.id + ")</i></color>";
		}
		else
		{
			titleText.supportRichText = false;
			titleText.text = worldInfo.title;
		}
		authorUsernameText.text = worldInfo.authorUserName;
		titleText.enabled = true;
		authorUsernameText.enabled = showAuthor;
		titleLabel.enabled = true;
		authorLabel.enabled = showAuthor;
		notLoadedText.enabled = false;
		worldThumbnailView.texture = null;
		if (!string.IsNullOrEmpty(worldInfo.thumbnailUrl))
		{
			worldThumbnailView.texture = _worldInfo.GetWorldThumbnail();
			worldInfo.ThumbnailLoaded += WorldInfoLoadedScreenshot;
			worldInfo.LoadThumbnail();
			_linkedToWorldInfoScreenshot = true;
		}
	}

	public void ClearWorldInfo()
	{
		if (_linkedToWorldInfoScreenshot)
		{
			worldThumbnailView.texture = null;
			_worldInfo.ThumbnailLoaded -= WorldInfoLoadedScreenshot;
			_linkedToWorldInfoScreenshot = false;
		}
		_worldInfo = null;
		titleText.enabled = false;
		authorUsernameText.enabled = false;
		titleLabel.enabled = false;
		authorLabel.enabled = false;
		notLoadedText.enabled = true;
		notLoadedText.text = "World not found";
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	private void WorldInfoLoadedScreenshot(object sender, EventArgs e)
	{
		if (sender == _worldInfo)
		{
			worldThumbnailView.texture = _worldInfo.GetWorldThumbnail();
		}
	}
}
