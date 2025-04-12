using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000199 RID: 409
public class HudMeshText
{
	// Token: 0x060016E8 RID: 5864 RVA: 0x000A45E8 File Offset: 0x000A29E8
	public static void Init()
	{
		string[] array;
		if (Blocksworld.hd)
		{
			array = new string[]
			{
				"Raleway-Regular 24 HD",
				"BlocksworldOutline-Regular 24 HD",
				"Raleway-Regular 17 HD"
			};
		}
		else
		{
			array = new string[]
			{
				"Raleway-Regular 24 SD",
				"BlocksworldOutline-Regular 24 SD",
				"Raleway-Regular 17 SD"
			};
		}
		HudMeshText.fontBank = new Dictionary<string, GameObject>();
		HudMeshText.hudTextParent = new GameObject("HudText");
		for (int i = 0; i < array.Length; i++)
		{
			GameObject gameObject = Resources.Load("TextMesh/" + array[i]) as GameObject;
			if (gameObject != null)
			{
				GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject);
				if (gameObject2 != null)
				{
					TextMesh component = gameObject2.GetComponent<TextMesh>();
					HudMeshText.fontBank.Add(component.font.name, gameObject2);
					if (HudMeshText.defaultFontObject == null)
					{
						HudMeshText.defaultFontObject = gameObject2;
					}
					gameObject2.transform.parent = HudMeshText.hudTextParent.transform;
					gameObject2.SetActive(false);
				}
			}
		}
	}

	// Token: 0x060016E9 RID: 5865 RVA: 0x000A46F8 File Offset: 0x000A2AF8
	public static PooledText Create(string inputString, HudMeshStyle style)
	{
		if (HudMeshText.fontBank == null)
		{
			HudMeshText.Init();
		}
		if (style == null || style.font == null)
		{
			style = HudMeshOnGUI.dataSource.GetStyle("label");
		}
		GameObject gameObject;
		if (!HudMeshText.fontBank.TryGetValue(style.font.name, out gameObject))
		{
			gameObject = UnityEngine.Object.Instantiate<GameObject>(HudMeshText.defaultFontObject);
			gameObject.GetComponent<TextMesh>().font = style.font;
			gameObject.GetComponent<Renderer>().material = style.font.material;
			HudMeshText.fontBank.Add(style.font.name, gameObject);
		}
		PooledObject pooledObject = ObjectPool.Get(gameObject, Vector3.zero, Quaternion.identity);
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
		return new PooledText(gameObject, pooledObject);
	}

	// Token: 0x040011E3 RID: 4579
	private static Dictionary<string, GameObject> fontBank;

	// Token: 0x040011E4 RID: 4580
	private static GameObject defaultFontObject;

	// Token: 0x040011E5 RID: 4581
	public static GameObject hudTextParent;

	// Token: 0x040011E6 RID: 4582
	private const int poolSize = 10;
}
