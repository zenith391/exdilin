using System.Collections.Generic;
using UnityEngine;

public class HudMeshText
{
	private static Dictionary<string, GameObject> fontBank;

	private static GameObject defaultFontObject;

	public static GameObject hudTextParent;

	private const int poolSize = 10;

	public static void Init()
	{
		string[] array = ((!Blocksworld.hd) ? new string[3] { "Raleway-Regular 24 SD", "BlocksworldOutline-Regular 24 SD", "Raleway-Regular 17 SD" } : new string[3] { "Raleway-Regular 24 HD", "BlocksworldOutline-Regular 24 HD", "Raleway-Regular 17 HD" });
		fontBank = new Dictionary<string, GameObject>();
		hudTextParent = new GameObject("HudText");
		for (int i = 0; i < array.Length; i++)
		{
			GameObject gameObject = Resources.Load("TextMesh/" + array[i]) as GameObject;
			if (!(gameObject != null))
			{
				continue;
			}
			GameObject gameObject2 = Object.Instantiate(gameObject);
			if (gameObject2 != null)
			{
				TextMesh component = gameObject2.GetComponent<TextMesh>();
				fontBank.Add(component.font.name, gameObject2);
				if (defaultFontObject == null)
				{
					defaultFontObject = gameObject2;
				}
				gameObject2.transform.parent = hudTextParent.transform;
				gameObject2.SetActive(value: false);
			}
		}
	}

	public static PooledText Create(string inputString, HudMeshStyle style)
	{
		if (fontBank == null)
		{
			Init();
		}
		if (style == null || style.font == null)
		{
			style = HudMeshOnGUI.dataSource.GetStyle("label");
		}
		if (!fontBank.TryGetValue(style.font.name, out var value))
		{
			value = Object.Instantiate(defaultFontObject);
			value.GetComponent<TextMesh>().font = style.font;
			value.GetComponent<Renderer>().material = style.font.material;
			fontBank.Add(style.font.name, value);
		}
		PooledObject pooledObject = ObjectPool.Get(value, Vector3.zero, Quaternion.identity);
		TextMesh component = pooledObject.GameObject.GetComponent<TextMesh>();
		Renderer component2 = pooledObject.GameObject.GetComponent<Renderer>();
		component2.material.color = style.textColor;
		component.text = inputString;
		component.fontSize = style.fontSize;
		component.fontStyle = style.fontStyle;
		component.characterSize = 10f / NormalizedScreen.scale;
		switch (style.alignment)
		{
		case TextAnchor.UpperLeft:
		case TextAnchor.MiddleLeft:
		case TextAnchor.LowerLeft:
			component.alignment = TextAlignment.Left;
			break;
		case TextAnchor.UpperCenter:
		case TextAnchor.MiddleCenter:
		case TextAnchor.LowerCenter:
			component.alignment = TextAlignment.Center;
			break;
		case TextAnchor.UpperRight:
		case TextAnchor.MiddleRight:
		case TextAnchor.LowerRight:
			component.alignment = TextAlignment.Right;
			break;
		default:
			component.alignment = TextAlignment.Center;
			break;
		}
		component.anchor = style.alignment;
		component.text = inputString;
		component2.enabled = true;
		return new PooledText(value, pooledObject);
	}
}
