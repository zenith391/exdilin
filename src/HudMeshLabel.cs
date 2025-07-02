using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class HudMeshLabel : MonoBehaviour
{
	private int framestamp = -1;

	private static int count;

	private Vector2 offset;

	private string lastText = string.Empty;

	private PooledText textObject;

	private TextMesh textMesh;

	private HudMeshStyle style;

	private bool nineSided;

	protected Rect lastRect;

	private const float depthStep = 0.001f;

	private const float maxDepthStep = 0.1f;

	private Rect actualScreenRect;

	public Rect screenRect => actualScreenRect;

	public static HudMeshLabel Create(Rect rect, string text, HudMeshStyle style, Color color)
	{
		HudMeshStyle hudMeshStyle = style.Clone();
		hudMeshStyle.textColor = color;
		return Create(rect, text, hudMeshStyle);
	}

	public static HudMeshLabel Create(Rect rect, Texture texture, HudMeshStyle style)
	{
		HudMeshStyle hudMeshStyle = style.Clone();
		hudMeshStyle.backgroundTexture = texture as Texture2D;
		return Create(rect, string.Empty, hudMeshStyle);
	}

	public static HudMeshLabel Create(Rect rect, string text, HudMeshStyle style, float extraContentHeight = 0f)
	{
		if (text == null)
		{
			text = string.Empty;
		}
		Texture2D backgroundTexture = style.backgroundTexture;
		bool flag = style.border.left != 0 || style.border.right != 0 || style.border.top != 0 || style.border.bottom != 0;
		Rect r = new Rect(0f, Screen.height, rect.width, rect.height);
		RectOffset border = new RectOffset((int)((float)style.border.left * NormalizedScreen.pixelScale), (int)((float)style.border.right * NormalizedScreen.pixelScale), (int)((float)style.border.top * NormalizedScreen.pixelScale), (int)((float)style.border.bottom * NormalizedScreen.pixelScale));
		GameObject gameObject;
		if (backgroundTexture == null)
		{
			gameObject = new GameObject("Label");
		}
		else if (flag)
		{
			gameObject = HudMeshUtils.CreateNineSidedMeshObject("Label", backgroundTexture, border, Vector2.one);
			HudMeshUtils.UpdateVertPositionsNineSided(HudMeshUtils.GetMesh(gameObject), r, border, 0f);
		}
		else
		{
			gameObject = HudMeshUtils.CreateMeshObject("Label", backgroundTexture);
			HudMeshUtils.UpdateVertPositions(HudMeshUtils.GetMesh(gameObject), r, 0f);
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

	private void PositionText(Rect rect)
	{
		if (textObject != null)
		{
			Transform transform = textObject.pooledObject.GameObject.transform;
			transform.parent = base.transform;
			transform.localScale = Vector3.one * NormalizedScreen.pixelScale;
			float num = (float)style.padding.left * NormalizedScreen.pixelScale;
			float num2 = (float)style.padding.right * NormalizedScreen.pixelScale;
			float num3 = (float)style.padding.top * NormalizedScreen.pixelScale;
			float num4 = (float)style.padding.bottom * NormalizedScreen.pixelScale;
			Rect rect2 = new Rect(num / NormalizedScreen.scale, (0f - num3) / NormalizedScreen.scale, (rect.width - num - num2) / NormalizedScreen.scale, (num3 + num4 - rect.height) / NormalizedScreen.scale);
			float num5 = 1.1f;
			switch (style.alignment)
			{
			case TextAnchor.UpperLeft:
				transform.localPosition = new Vector3(rect2.xMin, rect2.yMin, 0f - num5);
				break;
			case TextAnchor.UpperCenter:
				transform.localPosition = new Vector3(rect2.center.x, rect2.yMin, 0f - num5);
				break;
			case TextAnchor.UpperRight:
				transform.localPosition = new Vector3(rect2.xMax, rect2.yMin, 0f - num5);
				break;
			case TextAnchor.MiddleLeft:
				transform.localPosition = new Vector3(rect2.xMin, rect2.center.y, 0f - num5);
				break;
			case TextAnchor.MiddleCenter:
				transform.localPosition = new Vector3(rect2.center.x, rect2.center.y, 0f - num5);
				break;
			case TextAnchor.MiddleRight:
				transform.localPosition = new Vector3(rect2.xMax, rect2.center.y, 0f - num5);
				break;
			case TextAnchor.LowerLeft:
				transform.localPosition = new Vector3(rect2.xMin, rect2.yMax, 0f - num5);
				break;
			case TextAnchor.LowerCenter:
				transform.localPosition = new Vector3(rect2.center.x, rect2.yMax, 0f - num5);
				break;
			case TextAnchor.LowerRight:
				transform.localPosition = new Vector3(rect2.xMax, rect2.yMax, 0f - num5);
				break;
			default:
				transform.localPosition = new Vector3(rect2.xMin, rect2.yMin, 0f - num5);
				break;
			}
		}
	}

	public void Refresh(Rect rect, string text, Texture texture)
	{
		Texture2D texture2D = texture as Texture2D;
		if (texture2D != GetComponent<Renderer>().sharedMaterial.mainTexture)
		{
			GetComponent<Renderer>().sharedMaterial.mainTexture = texture2D;
		}
		Refresh(rect, text);
	}

	public void Refresh(Rect rect, string text, float extraContentHeight = 0f)
	{
		if (framestamp == Time.frameCount || style == null)
		{
			return;
		}
		framestamp = Time.frameCount;
		bool flag = Mathf.Abs(rect.width - lastRect.width) > Mathf.Epsilon || Mathf.Abs(rect.height - lastRect.height) > Mathf.Epsilon;
		lastRect = rect;
		bool flag2 = false;
		if (textObject == null && !string.IsNullOrEmpty(text))
		{
			textObject = HudMeshText.Create(text, style);
			textMesh = textObject.pooledObject.GameObject.GetComponent<TextMesh>();
			flag2 = true;
		}
		if (!lastText.Equals(text) && textMesh != null)
		{
			lastText = text;
			flag2 = true;
		}
		if (flag2 && !style.wordWrap)
		{
			textMesh.text = text;
		}
		else if (flag2 && !string.IsNullOrEmpty(text) && style.font != null)
		{
			float maxLineLength = rect.width - (float)style.padding.horizontal;
			style.font.RequestCharactersInTexture(text);
			Dictionary<char, float> characterLengths = new Dictionary<char, float>();
			Func<string, bool> func = delegate(string line)
			{
				float num13 = 0f;
				for (int i = 0; i < line.Length; i++)
				{
					if (characterLengths.TryGetValue(line[i], out var value2))
					{
						num13 += value2;
						if (num13 > maxLineLength)
						{
							return true;
						}
					}
				}
				return false;
			};
			float num = 0f;
			float num2 = 0f;
			if (style.font.GetCharacterInfo(' ', out var info))
			{
				num2 = info.advance;
			}
			for (int num3 = 0; num3 < text.Length; num3++)
			{
				if (characterLengths.TryGetValue(text[num3], out var value))
				{
					num += value;
				}
				else if (style.font.GetCharacterInfo(text[num3], out info))
				{
					value = info.advance;
					num += value;
					characterLengths.Add(text[num3], value);
				}
				else
				{
					characterLengths.Add(text[num3], num2);
				}
			}
			if (num > maxLineLength)
			{
				StringBuilder stringBuilder = new StringBuilder();
				StringBuilder stringBuilder2 = new StringBuilder();
				string[] array = text.Split(new string[1] { Environment.NewLine }, StringSplitOptions.None);
				for (int num4 = 0; num4 < array.Length; num4++)
				{
					stringBuilder2.Length = 0;
					float num5 = 0f;
					if (num4 > 0)
					{
						stringBuilder.AppendLine();
					}
					string[] array2 = array[num4].Split(' ');
					for (int num6 = 0; num6 < array2.Length; num6++)
					{
						if (func(array2[num6]))
						{
							float num7 = 0f;
							for (int num8 = 0; num8 < array2[num6].Length; num8++)
							{
								stringBuilder2.Append(array2[num6][num8]);
								num7 += characterLengths[array2[num6][num8]];
								if (num7 > maxLineLength)
								{
									stringBuilder2.Length--;
									stringBuilder.Append(stringBuilder2.ToString());
									stringBuilder.AppendLine();
									stringBuilder2.Length = 0;
									num7 = 0f;
								}
							}
							continue;
						}
						stringBuilder2.Append(array2[num6]);
						for (int num9 = 0; num9 < array2[num6].Length; num9++)
						{
							num5 += characterLengths[array2[num6][num9]];
						}
						if (stringBuilder2.Length > 0 && num5 > maxLineLength)
						{
							stringBuilder2.Length -= array2[num6].Length;
							stringBuilder.Append(stringBuilder2.ToString());
							stringBuilder.AppendLine();
							stringBuilder2.Length = 0;
							num5 = 0f;
							stringBuilder2.Append(array2[num6]);
							for (int num10 = 0; num10 < array2[num6].Length; num10++)
							{
								num5 += characterLengths[array2[num6][num10]];
							}
						}
						if (num6 < array2.Length - 1)
						{
							stringBuilder2.Append(' ');
							num5 += num2;
						}
					}
					if (stringBuilder2.Length > 0)
					{
						stringBuilder.Append(stringBuilder2.ToString());
					}
				}
				textMesh.text = stringBuilder.ToString();
			}
			else
			{
				textMesh.text = text;
			}
		}
		if (flag2 && (style.stretchWidth || style.stretchHeight))
		{
			bool flag3 = false;
			float num11 = rect.width;
			float num12 = rect.height;
			if (style.stretchWidth && textMesh.GetComponent<Renderer>().bounds.size.x * NormalizedScreen.scale > rect.width - (float)style.padding.horizontal)
			{
				num11 = textMesh.GetComponent<Renderer>().bounds.size.x * NormalizedScreen.scale + (float)style.padding.horizontal;
				flag3 = true;
			}
			if (style.stretchHeight && textMesh.GetComponent<Renderer>().bounds.size.y * NormalizedScreen.scale + extraContentHeight > rect.height - (float)style.padding.vertical)
			{
				num12 = textMesh.GetComponent<Renderer>().bounds.size.y * NormalizedScreen.scale + extraContentHeight + (float)style.padding.vertical;
				flag3 = true;
			}
			if (flag3)
			{
				rect = style.alignment switch
				{
					TextAnchor.UpperLeft => new Rect(rect.x, rect.y, num11, num12), 
					TextAnchor.UpperCenter => new Rect(rect.center.x - num11 / 2f, rect.y, num11, num12), 
					TextAnchor.UpperRight => new Rect(rect.xMax - num11, rect.y, num11, num12), 
					TextAnchor.MiddleLeft => new Rect(rect.x, rect.center.y - num12 / 2f, num11, num12), 
					TextAnchor.MiddleCenter => new Rect(rect.center.x - num11 / 2f, rect.center.y - num12 / 2f, num11, num12), 
					TextAnchor.MiddleRight => new Rect(rect.xMax - num11, rect.center.y - num12 / 2f, num11, num12), 
					TextAnchor.LowerLeft => new Rect(rect.x, rect.yMax - num12, num11, num12), 
					TextAnchor.LowerCenter => new Rect(rect.center.x - num11 / 2f, rect.yMax - num12, num11, num12), 
					TextAnchor.LowerRight => new Rect(rect.xMax - num11, rect.yMax - num12, num11, num12), 
					_ => new Rect(rect.x, rect.y, num11, num12), 
				};
				flag = true;
			}
		}
		if (flag)
		{
			base.transform.position = Vector3.zero;
			Mesh mesh = HudMeshUtils.GetMesh(base.gameObject);
			if (mesh != null)
			{
				Rect r = new Rect(0f, Screen.height, rect.width, rect.height);
				if (nineSided)
				{
					HudMeshUtils.UpdateVertPositionsNineSided(mesh, r, style.border, 0f);
				}
				else
				{
					HudMeshUtils.UpdateVertPositions(mesh, r, 0f);
				}
			}
			flag2 = true;
			actualScreenRect = rect;
		}
		else
		{
			actualScreenRect = new Rect(rect.x, rect.y, actualScreenRect.width, actualScreenRect.height);
		}
		base.transform.position = HudMeshUtils.ToGUICameraSpace(new Vector3(rect.x, rect.y, HudMeshOnGUI.GetDepth(count)), invertY: true);
		if (flag2)
		{
			PositionText(rect);
		}
		count++;
	}

	public void Refresh(Rect rect, string text, Vector2 scale)
	{
		Refresh(rect, text);
		float x = rect.x - rect.width * (scale.x - 1f) / 2f;
		float y = rect.y - rect.height * (scale.y - 1f) / 2f;
		base.transform.position = HudMeshUtils.ToGUICameraSpace(new Vector3(x, y, HudMeshOnGUI.GetDepth(count)), invertY: true);
		base.transform.localScale = new Vector3(scale.x, scale.y, 1f);
	}

	public void RefreshText(string text)
	{
		if (!(textMesh == null))
		{
			textMesh.text = text;
		}
	}

	public float GetTextWidth()
	{
		if (textMesh == null)
		{
			return 0f;
		}
		return textMesh.GetComponent<Renderer>().bounds.size.x;
	}

	private void LateUpdate()
	{
		count = 0;
		if (Time.frameCount <= framestamp)
		{
			return;
		}
		if (textObject != null)
		{
			textObject.pooledObject.GameObject.GetComponent<Renderer>().enabled = false;
			textObject.Return();
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
		foreach (object item in base.gameObject.transform)
		{
			Transform transform = (Transform)item;
			if (transform.gameObject.GetComponent<Renderer>() != null && transform.gameObject.GetComponent<Renderer>().material != null)
			{
				UnityEngine.Object.Destroy(transform.gameObject.GetComponent<Renderer>().material);
				transform.GetComponent<Renderer>().material = null;
			}
		}
		style = null;
		UnityEngine.Object.Destroy(base.gameObject);
		UnityEngine.Object.Destroy(this);
		framestamp = -1;
	}
}
