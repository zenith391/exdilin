using UnityEngine;
using UnityEngine.UI;

namespace Exdilin.UI;

public static class UIHelper
{
	public class TextFormat
	{
		public Color color;

		public Font font;

		public int fontSize;

		public FontStyle fontStyle;

		public static TextFormat Default = new TextFormat(Color.white, null, 24, FontStyle.Normal);

		public TextFormat(Color color, Font font, int fontSize, FontStyle fontStyle)
		{
			this.color = color;
			this.font = font;
			this.fontSize = fontSize;
			this.fontStyle = fontStyle;
		}
	}

	public static string DebugTransform(Transform t, int indentation = 0)
	{
		string text = "";
		for (int i = 0; i < indentation; i++)
		{
			text += " ";
		}
		text = text + "- " + t.name + " (" + t.position.x + ", " + t.position.y + ", " + t.position.z + ")\n";
		foreach (Transform item in t)
		{
			text = text + "|" + DebugTransform(item, indentation + 2);
		}
		return text;
	}

	public static GameObject CreateLabelGameObject(MonoBehaviour parent, float x, float y, string text, TextFormat format = null)
	{
		if (format == null)
		{
			format = TextFormat.Default;
		}
		GameObject gameObject = new GameObject("Label");
		RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
		Text text2 = gameObject.AddComponent<Text>();
		text2.text = text;
		text2.color = Color.white;
		text2.color = format.color;
		if (format.font != null)
		{
			text2.font = format.font;
		}
		if (format.fontSize != 0)
		{
			text2.fontSize = format.fontSize;
		}
		text2.fontStyle = format.fontStyle;
		rectTransform.SetParent(parent.transform, worldPositionStays: false);
		rectTransform.position = new Vector2(x, y);
		Debug.Log("transform:\n" + DebugTransform(parent.transform));
		return gameObject;
	}

	public static Font GetBlocksworldFont()
	{
		return null;
	}
}
