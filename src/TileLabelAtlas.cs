using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// Token: 0x020002D1 RID: 721
public class TileLabelAtlas
{
	// Token: 0x060020F5 RID: 8437 RVA: 0x000F0F04 File Offset: 0x000EF304
	public TileLabelAtlas(Font _font, bool hd)
	{
		this.font = _font;
		if (hd)
		{
			this.atlasSize *= 2;
			this.labelWidth *= 2;
			this.labelHeight *= 2;
		}
		this.fontTexture = (this.font.material.mainTexture as Texture2D);
		this.fontTexture.wrapMode = TextureWrapMode.Repeat;
		this.baseMaterial = Resources.Load<Material>("GUI/Tile Label Enabled");
		this.baseMaterialDisabled = Resources.Load<Material>("GUI/Tile Label Disabled");
	}

	// Token: 0x1700015A RID: 346
	// (get) Token: 0x060020F6 RID: 8438 RVA: 0x000F103C File Offset: 0x000EF43C
	private int xCount
	{
		get
		{
			return this.atlasSize / this.labelWidth;
		}
	}

	// Token: 0x1700015B RID: 347
	// (get) Token: 0x060020F7 RID: 8439 RVA: 0x000F104B File Offset: 0x000EF44B
	private int yCount
	{
		get
		{
			return this.atlasSize / this.labelHeight;
		}
	}

	// Token: 0x060020F8 RID: 8440 RVA: 0x000F105C File Offset: 0x000EF45C
	public void Clear()
	{
		for (int i = 0; i < this.atlases.Count; i++)
		{
			UnityEngine.Object.Destroy(this.atlasMaterials[i]);
			UnityEngine.Object.Destroy(this.atlasMaterialsDisabled[i]);
			UnityEngine.Object.Destroy(this.atlases[i]);
		}
		this.atlases.Clear();
		this.atlasMaterials.Clear();
		this.atlasMaterialsDisabled.Clear();
		this.currentAtlas = 0;
		this.currentIndex = 0;
		this.rectFromString.Clear();
		this.atlasIndexFromString.Clear();
	}

	// Token: 0x060020F9 RID: 8441 RVA: 0x000F10FD File Offset: 0x000EF4FD
	public bool Contains(string label)
	{
		return !string.IsNullOrEmpty(label) && this.rectFromString.ContainsKey(label) && this.atlasIndexFromString.ContainsKey(label);
	}

	// Token: 0x060020FA RID: 8442 RVA: 0x000F112C File Offset: 0x000EF52C
	public Texture2D GetTexture(string label)
	{
		int num = -1;
		this.atlasIndexFromString.TryGetValue(label, out num);
		if (num >= 0 && num < this.atlases.Count)
		{
			return this.atlases[num];
		}
		return null;
	}

	// Token: 0x060020FB RID: 8443 RVA: 0x000F1170 File Offset: 0x000EF570
	public Material GetMaterial(string label)
	{
		int num = -1;
		this.atlasIndexFromString.TryGetValue(label, out num);
		if (num >= 0 && num < this.atlasMaterials.Count)
		{
			return this.atlasMaterials[num];
		}
		return null;
	}

	// Token: 0x060020FC RID: 8444 RVA: 0x000F11B4 File Offset: 0x000EF5B4
	public Material GetMaterialDisabled(string label)
	{
		int num = -1;
		this.atlasIndexFromString.TryGetValue(label, out num);
		if (num >= 0 && num < this.atlasMaterialsDisabled.Count)
		{
			return this.atlasMaterialsDisabled[num];
		}
		return null;
	}

	// Token: 0x060020FD RID: 8445 RVA: 0x000F11F8 File Offset: 0x000EF5F8
	public Rect GetLabelRect(string label)
	{
		Rect result = new Rect(0f, 0f, 0f, 0f);
		this.rectFromString.TryGetValue(label, out result);
		return result;
	}

	// Token: 0x060020FE RID: 8446 RVA: 0x000F1230 File Offset: 0x000EF630
	public Vector2[] GetLabelUVs(string label)
	{
		Rect labelRect = this.GetLabelRect(label);
		return TileLabelAtlas.RectToUVs(labelRect);
	}

	// Token: 0x060020FF RID: 8447 RVA: 0x000F124B File Offset: 0x000EF64B
	public void AddNewLabel(string label)
	{
		this.Add(label);
		this.ApplyChanges();
	}

	// Token: 0x06002100 RID: 8448 RVA: 0x000F125C File Offset: 0x000EF65C
	public void AddNewLabels(List<string> labels)
	{
		foreach (string label in labels)
		{
			this.Add(label);
		}
		this.ApplyChanges();
	}

	// Token: 0x06002101 RID: 8449 RVA: 0x000F12BC File Offset: 0x000EF6BC
	private void AddAtlasMaterials(Texture atlasTexture)
	{
		Material material = new Material(this.baseMaterial);
		Material material2 = new Material(this.baseMaterialDisabled);
		material.mainTexture = atlasTexture;
		material2.mainTexture = atlasTexture;
		this.atlasMaterials.Add(material);
		this.atlasMaterialsDisabled.Add(material2);
	}

	// Token: 0x06002102 RID: 8450 RVA: 0x000F1308 File Offset: 0x000EF708
	private void ApplyChanges()
	{
		for (int i = 0; i < this.atlases.Count; i++)
		{
			if (this.dirtyAtlases.Count <= i)
			{
				this.dirtyAtlases.Add(true);
			}
			if (this.dirtyAtlases[i])
			{
				this.atlases[i].Apply();
				this.dirtyAtlases[i] = false;
			}
		}
	}

	// Token: 0x06002103 RID: 8451 RVA: 0x000F1380 File Offset: 0x000EF780
	private void Add(string label)
	{
		if (this.Contains(label))
		{
			return;
		}
		if (string.IsNullOrEmpty(label))
		{
			return;
		}
		Color color = new Color(1f, 1f, 1f, 0f);
		int num = this.xCount * this.yCount;
		float num2 = 1f / (float)this.xCount;
		float num3 = 1f / (float)this.yCount;
		if (this.currentIndex == num)
		{
			this.currentAtlas++;
			this.currentIndex = 0;
		}
		int num4 = this.currentAtlas;
		int num5 = this.currentIndex / this.xCount;
		int num6 = this.currentIndex % this.xCount;
		Rect value = new Rect((float)num6 * num2, (float)num5 * num3, num2, num3);
		Rect rect = new Rect(Mathf.Round(value.x * (float)this.atlasSize), Mathf.Round(value.y * (float)this.atlasSize), Mathf.Round(value.width * (float)this.atlasSize), Mathf.Round(value.height * (float)this.atlasSize));
		value = new Rect(rect.x / (float)this.atlasSize, rect.y / (float)this.atlasSize, rect.width / (float)this.atlasSize, rect.height / (float)this.atlasSize);
		this.currentIndex++;
		this.rectFromString[label] = value;
		this.atlasIndexFromString[label] = num4;
		if (num4 >= this.atlases.Count || this.atlases.Count == 0)
		{
			Texture2D texture2D = new Texture2D(this.atlasSize, this.atlasSize, TextureFormat.RGBA32, false);
			texture2D.wrapMode = TextureWrapMode.Clamp;
			texture2D.filterMode = FilterMode.Point;
			for (int i = 0; i < this.atlasSize; i++)
			{
				for (int j = 0; j < this.atlasSize; j++)
				{
					texture2D.SetPixel(j, i, color);
				}
			}
			this.atlases.Add(texture2D);
			this.AddAtlasMaterials(texture2D);
			this.dirtyAtlases.Add(true);
		}
		List<CharacterInfo> list = new List<CharacterInfo>();
		int num7 = 0;
		int fontSize = this.font.fontSize;
		List<int> list2 = new List<int>();
		for (int k = 0; k < label.Length; k++)
		{
			CharacterInfo item;
			if (this.font.GetCharacterInfo(label[k], out item))
			{
				list.Add(item);
				num7 += item.advance;
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
		num7 = Mathf.Min(this.labelWidth, num7);
		int num8 = 4;
		Texture2D texture2D2 = new Texture2D(num7 + num8, fontSize + num8, TextureFormat.RGBA32, false);
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
			float num11 = uv.width * (float)this.fontTexture.width;
			float num12 = -uv.height * (float)this.fontTexture.height;
			float num13 = uv.x * (float)this.fontTexture.width;
			float num14 = (uv.y + uv.height) * (float)this.fontTexture.height;
			int num15 = num9 + Mathf.RoundToInt(vert.x);
			int num16 = -Mathf.RoundToInt(vert.y);
			int num17 = Mathf.RoundToInt(vert.width);
			int num18 = -Mathf.RoundToInt(vert.height);
			float num19 = vert.x - (float)Mathf.RoundToInt(vert.x);
			float num20 = vert.y - (float)Mathf.RoundToInt(vert.y);
			num15 = Mathf.Clamp(num15, 0, texture2D2.width);
			num16 = Mathf.Clamp(num16, 0, texture2D2.height);
			for (int num21 = 0; num21 < num17; num21++)
			{
				for (int num22 = 0; num22 < num18; num22++)
				{
					float f;
					float f2;
					if (characterInfo.flipped)
					{
						float a = num13 + num11 - 1f;
						float b = num13 - 1f;
						float a2 = num14 + num12 - 1f;
						float b2 = num14 - 1f;
						f = Mathf.Lerp(a, b, (float)num22 / (float)num18) + num20;
						f2 = Mathf.Lerp(a2, b2, (float)num21 / (float)num17) - num19;
					}
					else
					{
						float a = num13;
						float b = num13 + num11;
						float a2 = num14;
						float b2 = num14 + num12;
						f = Mathf.Lerp(a, b, (float)num21 / (float)num17) + num19;
						f2 = Mathf.Lerp(a2, b2, (float)num22 / (float)num18) - num20;
					}
					Color pixel = this.fontTexture.GetPixel(Mathf.RoundToInt(f), Mathf.RoundToInt(f2));
					texture2D2.SetPixel(num8 / 2 + num15 + num21, num16 + num22 + num8 / 2, pixel);
				}
			}
			num9 += Mathf.FloorToInt((float)characterInfo.advance);
			if (num9 >= texture2D2.width)
			{
				BWLog.Warning("Label string too long. " + label);
				break;
			}
		}
		int num23 = (Mathf.RoundToInt(num2 * (float)this.atlasSize) - num9 - num8) / 2;
		int num24 = (Mathf.RoundToInt(num3 * (float)this.atlasSize) - texture2D2.height + num8) / 2;
		Rect rect2 = new Rect(value.x * (float)this.atlasSize + (float)num23, value.y * (float)this.atlasSize + (float)num24, (float)texture2D2.width, (float)texture2D2.height);
		Rect rect3 = new Rect(rect2.x + this.dropShadowOffset.x, rect2.y + this.dropShadowOffset.y, (float)texture2D2.width, (float)texture2D2.height);
		Texture2D texture2D3 = this.atlases[num4];
		int num26;
		int num25 = num26 = 0;
		int num27 = (int)rect3.y;
		while (num27 < (int)(rect3.y + rect3.height))
		{
			int num28 = (int)rect3.x;
			while (num28 < (int)(rect3.x + rect3.width))
			{
				float num29 = texture2D2.GetPixel(num26, (int)rect3.height - num25).a * this.dropShadowStrength;
				Color color2 = new Color(this.dropShadowColor.r, this.dropShadowColor.g, this.dropShadowColor.b, num29);
				if (num29 > 0f)
				{
					texture2D3.SetPixel(num28, num27, color2);
				}
				num28++;
				num26++;
			}
			num26 = 0;
			num27++;
			num25++;
		}
		num25 = (num26 = 0);
		int num30 = (int)rect2.y;
		while (num30 < (int)(rect2.y + rect2.height))
		{
			int num31 = (int)rect2.x;
			while (num31 < (int)(rect2.x + rect2.width))
			{
				Color pixel2 = texture2D3.GetPixel(num31, num30);
				Color pixel3 = texture2D2.GetPixel(num26, (int)rect2.height - num25);
				Color color3 = pixel2;
				bool flag = false;
				float num32 = pixel2.a - pixel3.a;
				if (num32 > 0f)
				{
					Color color4 = Color.Lerp(this.textColor, this.dropShadowColor, num32 / this.dropShadowStrength);
					color3 = new Color(color4.r, color4.g, color4.b, pixel2.a);
					flag = true;
				}
				else if (pixel3.a > 0f)
				{
					color3 = new Color(this.textColor.r, this.textColor.g, this.textColor.b, pixel3.a);
					flag = true;
				}
				if (flag)
				{
					texture2D3.SetPixel(num31, num30, color3);
				}
				num31++;
				num26++;
			}
			num26 = 0;
			num30++;
			num25++;
		}
		this.dirtyAtlases[num4] = true;
		if (Application.isEditor)
		{
			UnityEngine.Object.DestroyImmediate(texture2D2);
		}
		else
		{
			UnityEngine.Object.Destroy(texture2D2);
		}
	}

	// Token: 0x06002104 RID: 8452 RVA: 0x000F1C84 File Offset: 0x000F0084
	public static Vector2[] RectToUVs(Rect rect)
	{
		return new Vector2[]
		{
			new Vector2(rect.xMin, rect.yMin),
			new Vector2(rect.xMax, rect.yMin),
			new Vector2(rect.xMin, rect.yMax),
			new Vector2(rect.xMax, rect.yMax)
		};
	}

	// Token: 0x04001BF2 RID: 7154
	private Dictionary<string, Rect> rectFromString = new Dictionary<string, Rect>();

	// Token: 0x04001BF3 RID: 7155
	private Dictionary<string, int> atlasIndexFromString = new Dictionary<string, int>();

	// Token: 0x04001BF4 RID: 7156
	private List<Texture2D> atlases = new List<Texture2D>();

	// Token: 0x04001BF5 RID: 7157
	private List<Material> atlasMaterials = new List<Material>();

	// Token: 0x04001BF6 RID: 7158
	private List<Material> atlasMaterialsDisabled = new List<Material>();

	// Token: 0x04001BF7 RID: 7159
	private List<bool> dirtyAtlases = new List<bool>();

	// Token: 0x04001BF8 RID: 7160
	private Material baseMaterial;

	// Token: 0x04001BF9 RID: 7161
	private Material baseMaterialDisabled;

	// Token: 0x04001BFA RID: 7162
	private Texture2D fontTexture;

	// Token: 0x04001BFB RID: 7163
	private Font font;

	// Token: 0x04001BFC RID: 7164
	private int currentAtlas;

	// Token: 0x04001BFD RID: 7165
	private int currentIndex;

	// Token: 0x04001BFE RID: 7166
	private int labelWidth = 62;

	// Token: 0x04001BFF RID: 7167
	private int labelHeight = 14;

	// Token: 0x04001C00 RID: 7168
	private int atlasSize = 256;

	// Token: 0x04001C01 RID: 7169
	private Vector2 dropShadowOffset = new Vector2(1f, 0.5f);

	// Token: 0x04001C02 RID: 7170
	private Color dropShadowColor = new Color(0f, 0f, 0f, 1f);

	// Token: 0x04001C03 RID: 7171
	private float dropShadowStrength = 0.27f;

	// Token: 0x04001C04 RID: 7172
	private Color textColor = Color.white;
}
