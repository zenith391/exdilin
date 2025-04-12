using System;
using UnityEngine;

// Token: 0x0200020D RID: 525
public class HudMeshButton : MonoBehaviour
{
	// Token: 0x06001A3C RID: 6716 RVA: 0x000C1830 File Offset: 0x000BFC30
	public static HudMeshButton Create(Rect rect, string text, HudMeshStyle style, Color color)
	{
		HudMeshStyle hudMeshStyle = style.Clone();
		hudMeshStyle.textColor = color;
		return HudMeshButton.Create(rect, text, hudMeshStyle);
	}

	// Token: 0x06001A3D RID: 6717 RVA: 0x000C1854 File Offset: 0x000BFC54
	public static HudMeshButton Create(Rect rect, Texture texture, HudMeshStyle style)
	{
		HudMeshStyle hudMeshStyle = style.Clone();
		hudMeshStyle.backgroundTexture = (texture as Texture2D);
		return HudMeshButton.Create(rect, string.Empty, hudMeshStyle);
	}

	// Token: 0x06001A3E RID: 6718 RVA: 0x000C1880 File Offset: 0x000BFC80
	public static HudMeshButton Create(Rect rect, string text, HudMeshStyle style)
	{
		HudMeshLabel hudMeshLabel = HudMeshLabel.Create(rect, text, style, 0f);
		HudMeshButton hudMeshButton = hudMeshLabel.gameObject.AddComponent<HudMeshButton>();
		hudMeshButton.label = hudMeshLabel;
		return hudMeshButton;
	}

	// Token: 0x06001A3F RID: 6719 RVA: 0x000C18AF File Offset: 0x000BFCAF
	public void Refresh(Rect rect, string text, Texture texture)
	{
		this.label.Refresh(rect, text, texture);
	}

	// Token: 0x06001A40 RID: 6720 RVA: 0x000C18BF File Offset: 0x000BFCBF
	public void Refresh(Rect rect, string text)
	{
		this.label.Refresh(rect, text, 0f);
	}

	// Token: 0x06001A41 RID: 6721 RVA: 0x000C18D4 File Offset: 0x000BFCD4
	public bool IsPressed()
	{
		bool flag = false;
		Vector2 point = Vector2.zero;
		if (BW.Options.useTouch())
		{
			for (int i = 0; i < Input.touchCount; i++)
			{
				Touch touch = Input.GetTouch(i);
				if (touch.phase == TouchPhase.Ended)
				{
					flag = true;
					point = touch.position;
					break;
				}
			}
		}
		if (!flag && BW.Options.useMouse())
		{
			flag = Input.GetMouseButtonUp(0);
			point = Input.mousePosition;
		}
		if (flag)
		{
			point = new Vector2(point.x, (float)Screen.height - point.y);
			return this.label.screenRect.Contains(point);
		}
		return false;
	}

	// Token: 0x040015CF RID: 5583
	private HudMeshLabel label;
}
