using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class UIPanelElementImage : UIPanelElement
{
	public enum ResizeMode
	{
		None,
		Fill,
		Fit
	}

	public string dataKey = "imagePath";

	public ResizeMode resizeMode;

	private RawImage image;

	private RectTransform imageTransform;

	private Vector2 baseSize;

	public override void Init(UIPanelContents parentPanel)
	{
		base.Init(parentPanel);
		image = GetComponent<RawImage>();
		imageTransform = (RectTransform)image.transform;
		baseSize = imageTransform.sizeDelta;
	}

	public override void Clear()
	{
		base.Clear();
		image.texture = null;
		image.enabled = false;
	}

	public override void Fill(Dictionary<string, string> data)
	{
		if (data.TryGetValue(dataKey, out var value) && !string.IsNullOrEmpty(value))
		{
			image.texture = Resources.Load<Texture2D>(value);
			if (!(image.texture != null))
			{
				return;
			}
			image.enabled = true;
			int width = image.texture.width;
			int height = image.texture.height;
			bool flag = width > height;
			float num = (float)width / (float)height;
			if (resizeMode == ResizeMode.Fill)
			{
				if (flag)
				{
					imageTransform.sizeDelta = new Vector2(num * baseSize.y, baseSize.y);
				}
				else
				{
					imageTransform.sizeDelta = new Vector2(baseSize.x, baseSize.y / num);
				}
			}
			else if (resizeMode == ResizeMode.Fit)
			{
				if (flag)
				{
					imageTransform.sizeDelta = new Vector2(baseSize.x, baseSize.y / num);
				}
				else
				{
					imageTransform.sizeDelta = new Vector2(num * baseSize.y, baseSize.y);
				}
			}
		}
		else
		{
			BWLog.Info("No data for key " + dataKey);
			image.texture = null;
			image.enabled = false;
		}
	}
}
