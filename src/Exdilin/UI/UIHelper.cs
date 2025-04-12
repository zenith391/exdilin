using System;
using UnityEngine;
using UnityEngine.UI;

namespace Exdilin.UI {

	public static class UIHelper {

		public class TextFormat {
			public Color color;
			public Font font;
			public int fontSize;
			public FontStyle fontStyle = FontStyle.Normal;

			public static TextFormat Default = new TextFormat(Color.white, null, 24, FontStyle.Normal);

			public TextFormat(Color color, Font font, int fontSize, FontStyle fontStyle) {
				this.color = color;
				this.font = font;
				this.fontSize = fontSize;
				this.fontStyle = fontStyle;
			}
		}

		public static string DebugTransform(Transform t, int indentation = 0) {
			string debug = "";
			for (int i = 0; i < indentation; i++) {
				debug += " ";
			}
			debug += "- " + t.name + " (" + t.position.x + ", " + t.position.y + ", " + t.position.z + ")\n";
			foreach (Transform child in t) {
				debug += "|" + DebugTransform(child, indentation + 2);
			}
			return debug;
		}

		public static GameObject CreateLabelGameObject(MonoBehaviour parent, float x, float y, string text, TextFormat format = null) {
			if (format == null) format = TextFormat.Default;
			GameObject go = new GameObject("Label");

			RectTransform rectTransform = go.AddComponent<RectTransform>();

			Text textComponent = go.AddComponent<Text>();
			textComponent.text = text;
			textComponent.color = Color.white;

			textComponent.color = format.color;
			if (format.font != null)
				textComponent.font = format.font;
			if (format.fontSize != 0)
				textComponent.fontSize = format.fontSize;
			textComponent.fontStyle = format.fontStyle;

			rectTransform.SetParent(parent.transform, false);
			rectTransform.position = new Vector2(x, y);
			Debug.Log("transform:\n" + DebugTransform(parent.transform));
			return go;
		}

		public static Font GetBlocksworldFont() { // TODO
			return null;
		}

	}

}
