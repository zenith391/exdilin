using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class UIPanelElement : MonoBehaviour
{
	protected ImageManager imageManager;

	protected UIPanelContents parentPanel;

	public virtual void Init(UIPanelContents parentPanel)
	{
		this.parentPanel = parentPanel;
	}

	public virtual void Clear()
	{
	}

	public virtual void Fill(Dictionary<string, string> data)
	{
	}

	public virtual void Fill(string dataValue)
	{
	}

	public void SetImageManager(ImageManager imageManager)
	{
		this.imageManager = imageManager;
	}

	protected string ReplacePlaceholderMenuTextWithData(BWMenuTextEnum placeholder, Dictionary<string, string> data)
	{
		string textString = MenuTextDefinitions.GetTextString(placeholder);
		return ReplacePlaceholderTextWithData(textString, data);
	}

	protected string ReplacePlaceholderMenuTextWithData(BWMenuTextEnum placeholder, Dictionary<string, string> data, BWMenuTextEnum noData)
	{
		string textString = MenuTextDefinitions.GetTextString(placeholder);
		string textString2 = MenuTextDefinitions.GetTextString(noData);
		return ReplacePlaceholderTextWithData(textString, data, textString2);
	}

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
		foreach (string item in hashSet)
		{
			if (data.TryGetValue(item, out var value))
			{
				if (!string.IsNullOrEmpty(value))
				{
					string oldValue = "{" + item + "}";
					text = text.Replace(oldValue, value);
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
		if (flag2)
		{
			return defaultStr;
		}
		return text;
	}
}
