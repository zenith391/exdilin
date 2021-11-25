using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// Token: 0x02000412 RID: 1042
public class UIPanelElement : MonoBehaviour
{
	// Token: 0x06002D5E RID: 11614 RVA: 0x001439D0 File Offset: 0x00141DD0
	public virtual void Init(UIPanelContents parentPanel)
	{
		this.parentPanel = parentPanel;
	}

	// Token: 0x06002D5F RID: 11615 RVA: 0x001439D9 File Offset: 0x00141DD9
	public virtual void Clear()
	{
	}

	// Token: 0x06002D60 RID: 11616 RVA: 0x001439DB File Offset: 0x00141DDB
	public virtual void Fill(Dictionary<string, string> data)
	{
	}

	// Token: 0x06002D61 RID: 11617 RVA: 0x001439DD File Offset: 0x00141DDD
	public virtual void Fill(string dataValue)
	{
	}

	// Token: 0x06002D62 RID: 11618 RVA: 0x001439DF File Offset: 0x00141DDF
	public void SetImageManager(ImageManager imageManager)
	{
		this.imageManager = imageManager;
	}

	// Token: 0x06002D63 RID: 11619 RVA: 0x001439E8 File Offset: 0x00141DE8
	protected string ReplacePlaceholderMenuTextWithData(BWMenuTextEnum placeholder, Dictionary<string, string> data)
	{
		string textString = MenuTextDefinitions.GetTextString(placeholder);
		return this.ReplacePlaceholderTextWithData(textString, data, null);
	}

	// Token: 0x06002D64 RID: 11620 RVA: 0x00143A08 File Offset: 0x00141E08
	protected string ReplacePlaceholderMenuTextWithData(BWMenuTextEnum placeholder, Dictionary<string, string> data, BWMenuTextEnum noData)
	{
		string textString = MenuTextDefinitions.GetTextString(placeholder);
		string textString2 = MenuTextDefinitions.GetTextString(noData);
		return this.ReplacePlaceholderTextWithData(textString, data, textString2);
	}

	// Token: 0x06002D65 RID: 11621 RVA: 0x00143A2C File Offset: 0x00141E2C
	protected string ReplacePlaceholderTextWithData(string placeholderStr, Dictionary<string, string> data, string defaultStr = null)
	{
		if (defaultStr == null)
		{
			defaultStr = placeholderStr;
		}
		HashSet<string> hashSet = new HashSet<string>();
		StringBuilder stringBuilder = new StringBuilder(64);
		bool flag = false;
		foreach (char c in placeholderStr)
		{
			if (flag)
			{
				if (c == '}')
				{
					hashSet.Add(stringBuilder.ToString());
					stringBuilder.Length = 0;
				}
				else
				{
					stringBuilder.Append(c);
				}
			}
			else if (c == '{')
			{
				flag = true;
			}
		}
		string text = placeholderStr;
		bool flag2 = false;
		foreach (string text2 in hashSet)
		{
			string text3;
			if (data.TryGetValue(text2, out text3))
			{
				if (!string.IsNullOrEmpty(text3))
				{
					string oldValue = '{' + text2 + '}';
					text = text.Replace(oldValue, text3);
				}
				else
				{
					flag2 = true;
				}
			}
			else
			{
				flag2 = true;
			}
		}
		return (!flag2) ? text : defaultStr;
	}

	// Token: 0x040025E9 RID: 9705
	protected ImageManager imageManager;

	// Token: 0x040025EA RID: 9706
	protected UIPanelContents parentPanel;
}
