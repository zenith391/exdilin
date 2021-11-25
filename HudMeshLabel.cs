using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// Token: 0x02000197 RID: 407
public class HudMeshLabel : MonoBehaviour
{
	// Token: 0x17000069 RID: 105
	// (get) Token: 0x060016C8 RID: 5832 RVA: 0x000A2F6E File Offset: 0x000A136E
	public Rect screenRect
	{
		get
		{
			return this.actualScreenRect;
		}
	}

	// Token: 0x060016C9 RID: 5833 RVA: 0x000A2F78 File Offset: 0x000A1378
	public static HudMeshLabel Create(Rect rect, string text, HudMeshStyle style, Color color)
	{
		HudMeshStyle hudMeshStyle = style.Clone();
		hudMeshStyle.textColor = color;
		return HudMeshLabel.Create(rect, text, hudMeshStyle, 0f);
	}

	// Token: 0x060016CA RID: 5834 RVA: 0x000A2FA4 File Offset: 0x000A13A4
	public static HudMeshLabel Create(Rect rect, Texture texture, HudMeshStyle style)
	{
		HudMeshStyle hudMeshStyle = style.Clone();
		hudMeshStyle.backgroundTexture = (texture as Texture2D);
		return HudMeshLabel.Create(rect, string.Empty, hudMeshStyle, 0f);
	}

	// Token: 0x060016CB RID: 5835 RVA: 0x000A2FD8 File Offset: 0x000A13D8
	public static HudMeshLabel Create(Rect rect, string text, HudMeshStyle style, float extraContentHeight = 0f)
	{
		if (text == null)
		{
			text = string.Empty;
		}
		Texture2D backgroundTexture = style.backgroundTexture;
		bool flag = style.border.left != 0 || style.border.right != 0 || style.border.top != 0 || style.border.bottom != 0;
		Rect r = new Rect(0f, (float)Screen.height, rect.width, rect.height);
		RectOffset border = new RectOffset((int)((float)style.border.left * NormalizedScreen.pixelScale), (int)((float)style.border.right * NormalizedScreen.pixelScale), (int)((float)style.border.top * NormalizedScreen.pixelScale), (int)((float)style.border.bottom * NormalizedScreen.pixelScale));
		GameObject gameObject;
		if (backgroundTexture == null)
		{
			gameObject = new GameObject("Label");
		}
		else if (flag)
		{
			gameObject = HudMeshUtils.CreateNineSidedMeshObject("Label", backgroundTexture, border, Vector2.one);
			HudMeshUtils.UpdateVertPositionsNineSided(HudMeshUtils.GetMesh(gameObject), r, border, 0f, true);
		}
		else
		{
			gameObject = HudMeshUtils.CreateMeshObject("Label", backgroundTexture);
			HudMeshUtils.UpdateVertPositions(HudMeshUtils.GetMesh(gameObject), r, 0f, true);
		}
		HudMeshLabel hudMeshLabel = gameObject.AddComponent<HudMeshLabel>();
		hudMeshLabel.nineSided = flag;
		hudMeshLabel.style = style;
		if (!string.IsNullOrEmpty(text) && style != null && style.font != null)
		{
			hudMeshLabel.textObject = HudMeshText.Create(text, style);
			hudMeshLabel.textMesh = hudMeshLabel.textObject.pooledObject.GameObject.GetComponent<TextMesh>();
		}
		else
		{
			hudMeshLabel.textMesh = null;
		}
		hudMeshLabel.Refresh(rect, text, extraContentHeight);
		return hudMeshLabel;
	}

	// Token: 0x060016CC RID: 5836 RVA: 0x000A31A0 File Offset: 0x000A15A0
	private void PositionText(Rect rect)
	{
		if (this.textObject == null)
		{
			return;
		}
		Transform transform = this.textObject.pooledObject.GameObject.transform;
		transform.parent = base.transform;
		transform.localScale = Vector3.one * NormalizedScreen.pixelScale;
		float num = (float)this.style.padding.left * NormalizedScreen.pixelScale;
		float num2 = (float)this.style.padding.right * NormalizedScreen.pixelScale;
		float num3 = (float)this.style.padding.top * NormalizedScreen.pixelScale;
		float num4 = (float)this.style.padding.bottom * NormalizedScreen.pixelScale;
		Rect rect2 = new Rect(num / NormalizedScreen.scale, -num3 / NormalizedScreen.scale, (rect.width - num - num2) / NormalizedScreen.scale, (num3 + num4 - rect.height) / NormalizedScreen.scale);
		float num5 = 1.1f;
		switch (this.style.alignment)
		{
		case TextAnchor.UpperLeft:
			transform.localPosition = new Vector3(rect2.xMin, rect2.yMin, -num5);
			break;
		case TextAnchor.UpperCenter:
			transform.localPosition = new Vector3(rect2.center.x, rect2.yMin, -num5);
			break;
		case TextAnchor.UpperRight:
			transform.localPosition = new Vector3(rect2.xMax, rect2.yMin, -num5);
			break;
		case TextAnchor.MiddleLeft:
			transform.localPosition = new Vector3(rect2.xMin, rect2.center.y, -num5);
			break;
		case TextAnchor.MiddleCenter:
			transform.localPosition = new Vector3(rect2.center.x, rect2.center.y, -num5);
			break;
		case TextAnchor.MiddleRight:
			transform.localPosition = new Vector3(rect2.xMax, rect2.center.y, -num5);
			break;
		case TextAnchor.LowerLeft:
			transform.localPosition = new Vector3(rect2.xMin, rect2.yMax, -num5);
			break;
		case TextAnchor.LowerCenter:
			transform.localPosition = new Vector3(rect2.center.x, rect2.yMax, -num5);
			break;
		case TextAnchor.LowerRight:
			transform.localPosition = new Vector3(rect2.xMax, rect2.yMax, -num5);
			break;
		default:
			transform.localPosition = new Vector3(rect2.xMin, rect2.yMin, -num5);
			break;
		}
	}

	// Token: 0x060016CD RID: 5837 RVA: 0x000A3450 File Offset: 0x000A1850
	public void Refresh(Rect rect, string text, Texture texture)
	{
		Texture2D texture2D = texture as Texture2D;
		if (texture2D != base.GetComponent<Renderer>().sharedMaterial.mainTexture)
		{
			base.GetComponent<Renderer>().sharedMaterial.mainTexture = texture2D;
		}
		this.Refresh(rect, text, 0f);
	}

	// Token: 0x060016CE RID: 5838 RVA: 0x000A34A0 File Offset: 0x000A18A0
	public void Refresh(Rect rect, string text, float extraContentHeight = 0f)
	{
		if (this.framestamp == Time.frameCount)
		{
			return;
		}
		if (this.style == null)
		{
			return;
		}
		this.framestamp = Time.frameCount;
		bool flag = Mathf.Abs(rect.width - this.lastRect.width) > Mathf.Epsilon || Mathf.Abs(rect.height - this.lastRect.height) > Mathf.Epsilon;
		this.lastRect = rect;
		bool flag2 = false;
		if (this.textObject == null && !string.IsNullOrEmpty(text))
		{
			this.textObject = HudMeshText.Create(text, this.style);
			this.textMesh = this.textObject.pooledObject.GameObject.GetComponent<TextMesh>();
			flag2 = true;
		}
		if (!this.lastText.Equals(text) && this.textMesh != null)
		{
			this.lastText = text;
			flag2 = true;
		}
		if (flag2 && !this.style.wordWrap)
		{
			this.textMesh.text = text;
		}
		else if (flag2 && !string.IsNullOrEmpty(text) && this.style.font != null)
		{
			float maxLineLength = rect.width - (float)this.style.padding.horizontal;
			this.style.font.RequestCharactersInTexture(text);
			Dictionary<char, float> characterLengths = new Dictionary<char, float>();
			Func<string, bool> func = delegate(string line)
			{
				float num8 = 0f;
				for (int num9 = 0; num9 < line.Length; num9++)
				{
					float num10;
					if (characterLengths.TryGetValue(line[num9], out num10))
					{
						num8 += num10;
						if (num8 > maxLineLength)
						{
							return true;
						}
					}
				}
				return false;
			};
			float num = 0f;
			float num2 = 0f;
			CharacterInfo characterInfo;
			if (this.style.font.GetCharacterInfo(' ', out characterInfo))
			{
				num2 = (float)characterInfo.advance;
			}
			for (int i = 0; i < text.Length; i++)
			{
				float num3;
				if (characterLengths.TryGetValue(text[i], out num3))
				{
					num += num3;
				}
				else if (this.style.font.GetCharacterInfo(text[i], out characterInfo))
				{
					num3 = (float)characterInfo.advance;
					num += num3;
					characterLengths.Add(text[i], num3);
				}
				else
				{
					characterLengths.Add(text[i], num2);
				}
			}
			if (num > maxLineLength)
			{
				StringBuilder stringBuilder = new StringBuilder();
				StringBuilder stringBuilder2 = new StringBuilder();
				string[] array = text.Split(new string[]
				{
					Environment.NewLine
				}, StringSplitOptions.None);
				for (int j = 0; j < array.Length; j++)
				{
					stringBuilder2.Length = 0;
					float num4 = 0f;
					if (j > 0)
					{
						stringBuilder.AppendLine();
					}
					string[] array2 = array[j].Split(new char[]
					{
						' '
					});
					for (int k = 0; k < array2.Length; k++)
					{
						if (func(array2[k]))
						{
							float num5 = 0f;
							for (int l = 0; l < array2[k].Length; l++)
							{
								stringBuilder2.Append(array2[k][l]);
								num5 += characterLengths[array2[k][l]];
								if (num5 > maxLineLength)
								{
									stringBuilder2.Length--;
									stringBuilder.Append(stringBuilder2.ToString());
									stringBuilder.AppendLine();
									stringBuilder2.Length = 0;
									num5 = 0f;
								}
							}
						}
						else
						{
							stringBuilder2.Append(array2[k]);
							for (int m = 0; m < array2[k].Length; m++)
							{
								num4 += characterLengths[array2[k][m]];
							}
							if (stringBuilder2.Length > 0 && num4 > maxLineLength)
							{
								stringBuilder2.Length -= array2[k].Length;
								stringBuilder.Append(stringBuilder2.ToString());
								stringBuilder.AppendLine();
								stringBuilder2.Length = 0;
								num4 = 0f;
								stringBuilder2.Append(array2[k]);
								for (int n = 0; n < array2[k].Length; n++)
								{
									num4 += characterLengths[array2[k][n]];
								}
							}
							if (k < array2.Length - 1)
							{
								stringBuilder2.Append(' ');
								num4 += num2;
							}
						}
					}
					if (stringBuilder2.Length > 0)
					{
						stringBuilder.Append(stringBuilder2.ToString());
					}
				}
				this.textMesh.text = stringBuilder.ToString();
			}
			else
			{
				this.textMesh.text = text;
			}
		}
		if (flag2 && (this.style.stretchWidth || this.style.stretchHeight))
		{
			bool flag3 = false;
			float num6 = rect.width;
			float num7 = rect.height;
			if (this.style.stretchWidth && this.textMesh.GetComponent<Renderer>().bounds.size.x * NormalizedScreen.scale > rect.width - (float)this.style.padding.horizontal)
			{
				num6 = this.textMesh.GetComponent<Renderer>().bounds.size.x * NormalizedScreen.scale + (float)this.style.padding.horizontal;
				flag3 = true;
			}
			if (this.style.stretchHeight && this.textMesh.GetComponent<Renderer>().bounds.size.y * NormalizedScreen.scale + extraContentHeight > rect.height - (float)this.style.padding.vertical)
			{
				num7 = this.textMesh.GetComponent<Renderer>().bounds.size.y * NormalizedScreen.scale + extraContentHeight + (float)this.style.padding.vertical;
				flag3 = true;
			}
			if (flag3)
			{
				switch (this.style.alignment)
				{
				case TextAnchor.UpperLeft:
					rect = new Rect(rect.x, rect.y, num6, num7);
					break;
				case TextAnchor.UpperCenter:
					rect = new Rect(rect.center.x - num6 / 2f, rect.y, num6, num7);
					break;
				case TextAnchor.UpperRight:
					rect = new Rect(rect.xMax - num6, rect.y, num6, num7);
					break;
				case TextAnchor.MiddleLeft:
					rect = new Rect(rect.x, rect.center.y - num7 / 2f, num6, num7);
					break;
				case TextAnchor.MiddleCenter:
					rect = new Rect(rect.center.x - num6 / 2f, rect.center.y - num7 / 2f, num6, num7);
					break;
				case TextAnchor.MiddleRight:
					rect = new Rect(rect.xMax - num6, rect.center.y - num7 / 2f, num6, num7);
					break;
				case TextAnchor.LowerLeft:
					rect = new Rect(rect.x, rect.yMax - num7, num6, num7);
					break;
				case TextAnchor.LowerCenter:
					rect = new Rect(rect.center.x - num6 / 2f, rect.yMax - num7, num6, num7);
					break;
				case TextAnchor.LowerRight:
					rect = new Rect(rect.xMax - num6, rect.yMax - num7, num6, num7);
					break;
				default:
					rect = new Rect(rect.x, rect.y, num6, num7);
					break;
				}
				flag = true;
			}
		}
		if (flag)
		{
			base.transform.position = Vector3.zero;
			Mesh mesh = HudMeshUtils.GetMesh(base.gameObject);
			if (mesh != null)
			{
				Rect r = new Rect(0f, (float)Screen.height, rect.width, rect.height);
				if (this.nineSided)
				{
					HudMeshUtils.UpdateVertPositionsNineSided(mesh, r, this.style.border, 0f, true);
				}
				else
				{
					HudMeshUtils.UpdateVertPositions(mesh, r, 0f, true);
				}
			}
			flag2 = true;
			this.actualScreenRect = rect;
		}
		else
		{
			this.actualScreenRect = new Rect(rect.x, rect.y, this.actualScreenRect.width, this.actualScreenRect.height);
		}
		base.transform.position = HudMeshUtils.ToGUICameraSpace(new Vector3(rect.x, rect.y, HudMeshOnGUI.GetDepth(HudMeshLabel.count)), true);
		if (flag2)
		{
			this.PositionText(rect);
		}
		HudMeshLabel.count++;
	}

	// Token: 0x060016CF RID: 5839 RVA: 0x000A3DFC File Offset: 0x000A21FC
	public void Refresh(Rect rect, string text, Vector2 scale)
	{
		this.Refresh(rect, text, 0f);
		float x = rect.x - rect.width * (scale.x - 1f) / 2f;
		float y = rect.y - rect.height * (scale.y - 1f) / 2f;
		base.transform.position = HudMeshUtils.ToGUICameraSpace(new Vector3(x, y, HudMeshOnGUI.GetDepth(HudMeshLabel.count)), true);
		base.transform.localScale = new Vector3(scale.x, scale.y, 1f);
	}

	// Token: 0x060016D0 RID: 5840 RVA: 0x000A3EA3 File Offset: 0x000A22A3
	public void RefreshText(string text)
	{
		if (this.textMesh == null)
		{
			return;
		}
		this.textMesh.text = text;
	}

	// Token: 0x060016D1 RID: 5841 RVA: 0x000A3EC4 File Offset: 0x000A22C4
	public float GetTextWidth()
	{
		if (this.textMesh == null)
		{
			return 0f;
		}
		return this.textMesh.GetComponent<Renderer>().bounds.size.x;
	}

	// Token: 0x060016D2 RID: 5842 RVA: 0x000A3F08 File Offset: 0x000A2308
	private void LateUpdate()
	{
		HudMeshLabel.count = 0;
		if (Time.frameCount > this.framestamp)
		{
			if (this.textObject != null)
			{
				this.textObject.pooledObject.GameObject.GetComponent<Renderer>().enabled = false;
				this.textObject.Return();
			}
			MeshFilter component = base.gameObject.GetComponent<MeshFilter>();
			if (component != null)
			{
				component.mesh = null;
				component.sharedMesh = null;
				UnityEngine.Object.Destroy(component);
			}
			if (base.gameObject.GetComponent<Renderer>() != null && base.gameObject.GetComponent<Renderer>().material != null)
			{
				UnityEngine.Object.Destroy(base.gameObject.GetComponent<Renderer>().material);
				base.gameObject.GetComponent<Renderer>().material = null;
			}
			IEnumerator enumerator = base.gameObject.transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform = (Transform)obj;
					if (transform.gameObject.GetComponent<Renderer>() != null && transform.gameObject.GetComponent<Renderer>().material != null)
					{
						UnityEngine.Object.Destroy(transform.gameObject.GetComponent<Renderer>().material);
						transform.GetComponent<Renderer>().material = null;
					}
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
			this.style = null;
			UnityEngine.Object.Destroy(base.gameObject);
			UnityEngine.Object.Destroy(this);
			this.framestamp = -1;
		}
	}

	// Token: 0x040011D2 RID: 4562
	private int framestamp = -1;

	// Token: 0x040011D3 RID: 4563
	private static int count;

	// Token: 0x040011D4 RID: 4564
	private Vector2 offset;

	// Token: 0x040011D5 RID: 4565
	private string lastText = string.Empty;

	// Token: 0x040011D6 RID: 4566
	private PooledText textObject;

	// Token: 0x040011D7 RID: 4567
	private TextMesh textMesh;

	// Token: 0x040011D8 RID: 4568
	private HudMeshStyle style;

	// Token: 0x040011D9 RID: 4569
	private bool nineSided;

	// Token: 0x040011DA RID: 4570
	protected Rect lastRect;

	// Token: 0x040011DB RID: 4571
	private const float depthStep = 0.001f;

	// Token: 0x040011DC RID: 4572
	private const float maxDepthStep = 0.1f;

	// Token: 0x040011DD RID: 4573
	private Rect actualScreenRect;
}
