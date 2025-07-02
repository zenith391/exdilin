using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class TileLabelAtlas
{
	private Dictionary<string, Rect> rectFromString = new Dictionary<string, Rect>();

	private Dictionary<string, int> atlasIndexFromString = new Dictionary<string, int>();

	private List<Texture2D> atlases = new List<Texture2D>();

	private List<Material> atlasMaterials = new List<Material>();

	private List<Material> atlasMaterialsDisabled = new List<Material>();

	private List<bool> dirtyAtlases = new List<bool>();

	private Material baseMaterial;

	private Material baseMaterialDisabled;

	private Texture2D fontTexture;

	private Font font;

	private int currentAtlas;

	private int currentIndex;

	private int labelWidth = 62;

	private int labelHeight = 14;

	private int atlasSize = 256;

	private Vector2 dropShadowOffset = new Vector2(1f, 0.5f);

	private Color dropShadowColor = new Color(0f, 0f, 0f, 1f);

	private float dropShadowStrength = 0.27f;

	private Color textColor = Color.white;

	private int xCount => atlasSize / labelWidth;

	private int yCount => atlasSize / labelHeight;

	public TileLabelAtlas(Font _font, bool hd)
	{
		font = _font;
		if (hd)
		{
			atlasSize *= 2;
			labelWidth *= 2;
			labelHeight *= 2;
		}
		fontTexture = font.material.mainTexture as Texture2D;
		fontTexture.wrapMode = TextureWrapMode.Repeat;
		baseMaterial = Resources.Load<Material>("GUI/Tile Label Enabled");
		baseMaterialDisabled = Resources.Load<Material>("GUI/Tile Label Disabled");
	}

	public void Clear()
	{
		for (int i = 0; i < atlases.Count; i++)
		{
			Object.Destroy(atlasMaterials[i]);
			Object.Destroy(atlasMaterialsDisabled[i]);
			Object.Destroy(atlases[i]);
		}
		atlases.Clear();
		atlasMaterials.Clear();
		atlasMaterialsDisabled.Clear();
		currentAtlas = 0;
		currentIndex = 0;
		rectFromString.Clear();
		atlasIndexFromString.Clear();
	}

	public bool Contains(string label)
	{
		if (!string.IsNullOrEmpty(label) && rectFromString.ContainsKey(label))
		{
			return atlasIndexFromString.ContainsKey(label);
		}
		return false;
	}

	public Texture2D GetTexture(string label)
	{
		int value = -1;
		atlasIndexFromString.TryGetValue(label, out value);
		if (value >= 0 && value < atlases.Count)
		{
			return atlases[value];
		}
		return null;
	}

	public Material GetMaterial(string label)
	{
		int value = -1;
		atlasIndexFromString.TryGetValue(label, out value);
		if (value >= 0 && value < atlasMaterials.Count)
		{
			return atlasMaterials[value];
		}
		return null;
	}

	public Material GetMaterialDisabled(string label)
	{
		int value = -1;
		atlasIndexFromString.TryGetValue(label, out value);
		if (value >= 0 && value < atlasMaterialsDisabled.Count)
		{
			return atlasMaterialsDisabled[value];
		}
		return null;
	}

	public Rect GetLabelRect(string label)
	{
		Rect value = new Rect(0f, 0f, 0f, 0f);
		rectFromString.TryGetValue(label, out value);
		return value;
	}

	public Vector2[] GetLabelUVs(string label)
	{
		Rect labelRect = GetLabelRect(label);
		return RectToUVs(labelRect);
	}

	public void AddNewLabel(string label)
	{
		Add(label);
		ApplyChanges();
	}

	public void AddNewLabels(List<string> labels)
	{
		foreach (string label in labels)
		{
			Add(label);
		}
		ApplyChanges();
	}

	private void AddAtlasMaterials(Texture atlasTexture)
	{
		Material material = new Material(baseMaterial);
		Material material2 = new Material(baseMaterialDisabled);
		material.mainTexture = atlasTexture;
		material2.mainTexture = atlasTexture;
		atlasMaterials.Add(material);
		atlasMaterialsDisabled.Add(material2);
	}

	private void ApplyChanges()
	{
		for (int i = 0; i < atlases.Count; i++)
		{
			if (dirtyAtlases.Count <= i)
			{
				dirtyAtlases.Add(item: true);
			}
			if (dirtyAtlases[i])
			{
				atlases[i].Apply();
				dirtyAtlases[i] = false;
			}
		}
	}

	private void Add(string label)
	{
		if (Contains(label) || string.IsNullOrEmpty(label))
		{
			return;
		}
		Color color = new Color(1f, 1f, 1f, 0f);
		int num = xCount * yCount;
		float num2 = 1f / (float)xCount;
		float num3 = 1f / (float)yCount;
		if (currentIndex == num)
		{
			currentAtlas++;
			currentIndex = 0;
		}
		int num4 = currentAtlas;
		int num5 = currentIndex / xCount;
		int num6 = currentIndex % xCount;
		Rect rect = new Rect((float)num6 * num2, (float)num5 * num3, num2, num3);
		Rect rect2 = new Rect(Mathf.Round(rect.x * (float)atlasSize), Mathf.Round(rect.y * (float)atlasSize), Mathf.Round(rect.width * (float)atlasSize), Mathf.Round(rect.height * (float)atlasSize));
		rect = new Rect(rect2.x / (float)atlasSize, rect2.y / (float)atlasSize, rect2.width / (float)atlasSize, rect2.height / (float)atlasSize);
		currentIndex++;
		rectFromString[label] = rect;
		atlasIndexFromString[label] = num4;
		if (num4 >= atlases.Count || atlases.Count == 0)
		{
			Texture2D texture2D = new Texture2D(atlasSize, atlasSize, TextureFormat.RGBA32, mipmap: false);
			texture2D.wrapMode = TextureWrapMode.Clamp;
			texture2D.filterMode = FilterMode.Point;
			for (int i = 0; i < atlasSize; i++)
			{
				for (int j = 0; j < atlasSize; j++)
				{
					texture2D.SetPixel(j, i, color);
				}
			}
			atlases.Add(texture2D);
			AddAtlasMaterials(texture2D);
			dirtyAtlases.Add(item: true);
		}
		List<CharacterInfo> list = new List<CharacterInfo>();
		int num7 = 0;
		int fontSize = font.fontSize;
		List<int> list2 = new List<int>();
		for (int k = 0; k < label.Length; k++)
		{
			if (font.GetCharacterInfo(label[k], out var info))
			{
				list.Add(info);
				num7 += info.advance;
			}
			else
			{
				BWLog.Info("Invalid character in label: " + label[k]);
				list2.Add(k);
			}
		}
		StringBuilder stringBuilder = new StringBuilder(64);
		for (int l = 0; l < label.Length; l++)
		{
			if (!list2.Contains(l))
			{
				stringBuilder.Append(label[l]);
			}
		}
		label = stringBuilder.ToString();
		num7 = Mathf.Min(labelWidth, num7);
		int num8 = 4;
		Texture2D texture2D2 = new Texture2D(num7 + num8, fontSize + num8, TextureFormat.RGBA32, mipmap: false);
		texture2D2.filterMode = FilterMode.Point;
		for (int m = 0; m < texture2D2.height; m++)
		{
			for (int n = 0; n < texture2D2.width; n++)
			{
				texture2D2.SetPixel(n, m, color);
			}
		}
		int num9 = 0;
		for (int num10 = 0; num10 < label.Length; num10++)
		{
			CharacterInfo characterInfo = list[num10];
			Rect uv = characterInfo.uv;
			Rect vert = characterInfo.vert;
			float num11 = uv.width * (float)fontTexture.width;
			float num12 = (0f - uv.height) * (float)fontTexture.height;
			float num13 = uv.x * (float)fontTexture.width;
			float num14 = (uv.y + uv.height) * (float)fontTexture.height;
			int value = num9 + Mathf.RoundToInt(vert.x);
			int value2 = -Mathf.RoundToInt(vert.y);
			int num15 = Mathf.RoundToInt(vert.width);
			int num16 = -Mathf.RoundToInt(vert.height);
			float num17 = vert.x - (float)Mathf.RoundToInt(vert.x);
			float num18 = vert.y - (float)Mathf.RoundToInt(vert.y);
			value = Mathf.Clamp(value, 0, texture2D2.width);
			value2 = Mathf.Clamp(value2, 0, texture2D2.height);
			for (int num19 = 0; num19 < num15; num19++)
			{
				for (int num20 = 0; num20 < num16; num20++)
				{
					float f;
					float f2;
					if (characterInfo.flipped)
					{
						float a = num13 + num11 - 1f;
						float b = num13 - 1f;
						float a2 = num14 + num12 - 1f;
						float b2 = num14 - 1f;
						f = Mathf.Lerp(a, b, (float)num20 / (float)num16) + num18;
						f2 = Mathf.Lerp(a2, b2, (float)num19 / (float)num15) - num17;
					}
					else
					{
						float a3 = num13;
						float b3 = num13 + num11;
						float a4 = num14;
						float b4 = num14 + num12;
						f = Mathf.Lerp(a3, b3, (float)num19 / (float)num15) + num17;
						f2 = Mathf.Lerp(a4, b4, (float)num20 / (float)num16) - num18;
					}
					Color pixel = fontTexture.GetPixel(Mathf.RoundToInt(f), Mathf.RoundToInt(f2));
					texture2D2.SetPixel(num8 / 2 + value + num19, value2 + num20 + num8 / 2, pixel);
				}
			}
			num9 += Mathf.FloorToInt(characterInfo.advance);
			if (num9 >= texture2D2.width)
			{
				BWLog.Warning("Label string too long. " + label);
				break;
			}
		}
		int num21 = (Mathf.RoundToInt(num2 * (float)atlasSize) - num9 - num8) / 2;
		int num22 = (Mathf.RoundToInt(num3 * (float)atlasSize) - texture2D2.height + num8) / 2;
		Rect rect3 = new Rect(rect.x * (float)atlasSize + (float)num21, rect.y * (float)atlasSize + (float)num22, texture2D2.width, texture2D2.height);
		Rect rect4 = new Rect(rect3.x + dropShadowOffset.x, rect3.y + dropShadowOffset.y, texture2D2.width, texture2D2.height);
		Texture2D texture2D3 = atlases[num4];
		int num24;
		int num23 = (num24 = 0);
		int num25 = (int)rect4.y;
		while (num25 < (int)(rect4.y + rect4.height))
		{
			int num26 = (int)rect4.x;
			while (num26 < (int)(rect4.x + rect4.width))
			{
				float num27 = texture2D2.GetPixel(num24, (int)rect4.height - num23).a * dropShadowStrength;
				Color color2 = new Color(dropShadowColor.r, dropShadowColor.g, dropShadowColor.b, num27);
				if (num27 > 0f)
				{
					texture2D3.SetPixel(num26, num25, color2);
				}
				num26++;
				num24++;
			}
			num24 = 0;
			num25++;
			num23++;
		}
		num23 = (num24 = 0);
		int num28 = (int)rect3.y;
		while (num28 < (int)(rect3.y + rect3.height))
		{
			int num29 = (int)rect3.x;
			while (num29 < (int)(rect3.x + rect3.width))
			{
				Color pixel2 = texture2D3.GetPixel(num29, num28);
				Color pixel3 = texture2D2.GetPixel(num24, (int)rect3.height - num23);
				Color color3 = pixel2;
				bool flag = false;
				float num30 = pixel2.a - pixel3.a;
				if (num30 > 0f)
				{
					Color color4 = Color.Lerp(textColor, dropShadowColor, num30 / dropShadowStrength);
					color3 = new Color(color4.r, color4.g, color4.b, pixel2.a);
					flag = true;
				}
				else if (pixel3.a > 0f)
				{
					color3 = new Color(textColor.r, textColor.g, textColor.b, pixel3.a);
					flag = true;
				}
				if (flag)
				{
					texture2D3.SetPixel(num29, num28, color3);
				}
				num29++;
				num24++;
			}
			num24 = 0;
			num28++;
			num23++;
		}
		dirtyAtlases[num4] = true;
		if (Application.isEditor)
		{
			Object.DestroyImmediate(texture2D2);
		}
		else
		{
			Object.Destroy(texture2D2);
		}
	}

	public static Vector2[] RectToUVs(Rect rect)
	{
		return new Vector2[4]
		{
			new Vector2(rect.xMin, rect.yMin),
			new Vector2(rect.xMax, rect.yMin),
			new Vector2(rect.xMin, rect.yMax),
			new Vector2(rect.xMax, rect.yMax)
		};
	}
}
