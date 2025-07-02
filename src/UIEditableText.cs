using UnityEngine;
using UnityEngine.UI;

public class UIEditableText : MonoBehaviour
{
	public Text[] texts;

	private string[] placeHolders;

	private string[] staticParts;

	private string currentText;

	private string placeholderText;

	public float PreferredWidth => texts[0].preferredWidth;

	public void Init()
	{
		if (texts == null || texts.Length == 0)
		{
			Debug.Log("editiable text not setup right ", base.gameObject);
			return;
		}
		placeholderText = texts[0].text;
		for (int i = 0; i < texts.Length; i++)
		{
			texts[i].raycastTarget = false;
		}
	}

	public void Set(string str)
	{
		currentText = str;
		if (texts == null)
		{
			Debug.Log("editiable text not setup right ", base.gameObject);
			return;
		}
		for (int i = 0; i < texts.Length; i++)
		{
			texts[i].text = str;
		}
	}

	public string Get()
	{
		return currentText;
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public void ReplacePlaceholder(string str, string placeholderStr = "placeholder")
	{
		placeholderStr = "{" + placeholderStr + "}";
		string str2 = placeholderText.Replace(placeholderStr, str);
		Set(str2);
	}
}
