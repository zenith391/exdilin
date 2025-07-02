using UnityEngine;

public class HudMeshButton : MonoBehaviour
{
	private HudMeshLabel label;

	public static HudMeshButton Create(Rect rect, string text, HudMeshStyle style, Color color)
	{
		HudMeshStyle hudMeshStyle = style.Clone();
		hudMeshStyle.textColor = color;
		return Create(rect, text, hudMeshStyle);
	}

	public static HudMeshButton Create(Rect rect, Texture texture, HudMeshStyle style)
	{
		HudMeshStyle hudMeshStyle = style.Clone();
		hudMeshStyle.backgroundTexture = texture as Texture2D;
		return Create(rect, string.Empty, hudMeshStyle);
	}

	public static HudMeshButton Create(Rect rect, string text, HudMeshStyle style)
	{
		HudMeshLabel hudMeshLabel = HudMeshLabel.Create(rect, text, style);
		HudMeshButton hudMeshButton = hudMeshLabel.gameObject.AddComponent<HudMeshButton>();
		hudMeshButton.label = hudMeshLabel;
		return hudMeshButton;
	}

	public void Refresh(Rect rect, string text, Texture texture)
	{
		label.Refresh(rect, text, texture);
	}

	public void Refresh(Rect rect, string text)
	{
		label.Refresh(rect, text);
	}

	public bool IsPressed()
	{
		bool flag = false;
		Vector2 vector = Vector2.zero;
		if (BW.Options.useTouch())
		{
			for (int i = 0; i < Input.touchCount; i++)
			{
				Touch touch = Input.GetTouch(i);
				if (touch.phase == TouchPhase.Ended)
				{
					flag = true;
					vector = touch.position;
					break;
				}
			}
		}
		if (!flag && BW.Options.useMouse())
		{
			flag = Input.GetMouseButtonUp(0);
			vector = Input.mousePosition;
		}
		if (flag)
		{
			vector = new Vector2(vector.x, (float)Screen.height - vector.y);
			return label.screenRect.Contains(vector);
		}
		return false;
	}
}
