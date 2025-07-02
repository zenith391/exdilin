using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class UIPanelContentsNetworkImage : UIPanelElement
{
	public string imageUrlKey;

	public Texture2D missingImagePlaceholder;

	private string imageUrl;

	private RawImage image;

	public bool adjustAspectRatioFitter = true;

	private bool clear;

	public override void Init(UIPanelContents parentPanel)
	{
		image = GetComponent<RawImage>();
	}

	public override void Fill(Dictionary<string, string> data)
	{
		imageUrl = string.Empty;
		if (data.TryGetValue(imageUrlKey, out imageUrl) && !string.IsNullOrEmpty(imageUrl))
		{
			imageManager.AddListener(imageUrl, ImageLoaded);
			imageManager.LoadImage(imageUrl, base.gameObject.GetInstanceID());
			clear = false;
		}
		else
		{
			BWLog.Info("No data for key " + imageUrlKey);
			image.texture = missingImagePlaceholder;
			image.enabled = true;
		}
	}

	public override void Fill(string dataValue)
	{
		imageUrl = dataValue;
		if (!string.IsNullOrEmpty(imageUrl))
		{
			imageManager.AddListener(imageUrl, ImageLoaded);
			imageManager.LoadImage(imageUrl, base.gameObject.GetInstanceID());
			clear = false;
		}
	}

	public override void Clear()
	{
		if (image != null)
		{
			image.texture = null;
			image.enabled = false;
		}
		clear = true;
		if (!string.IsNullOrEmpty(imageUrl))
		{
			imageManager.ReleaseReference(imageUrl, base.gameObject.GetInstanceID());
			imageManager.RemoveListener(imageUrl, ImageLoaded);
		}
		imageUrl = string.Empty;
	}

	public void OnDestroy()
	{
		Clear();
	}

	private void ImageLoaded(string path, Texture2D imageTex, ImageLoader.LoadState loadState)
	{
		if (loadState == ImageLoader.LoadState.Loaded)
		{
			image.texture = imageTex;
			image.enabled = true;
			FixAspectRatio();
		}
		else
		{
			image.texture = missingImagePlaceholder;
			image.enabled = true;
			FixAspectRatio();
		}
	}

	private void FixAspectRatio()
	{
		if (adjustAspectRatioFitter && !(image.texture == null))
		{
			AspectRatioFitter component = GetComponent<AspectRatioFitter>();
			if (!(component == null))
			{
				component.aspectRatio = (float)image.texture.width / (float)image.texture.height;
			}
		}
	}
}
