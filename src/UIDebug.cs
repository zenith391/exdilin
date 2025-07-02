using UnityEngine;
using UnityEngine.UI;

public class UIDebug : MonoBehaviour
{
	public Text textField;

	public void SetText(string text)
	{
		base.gameObject.SetActive(!string.IsNullOrEmpty(text));
		textField.text = text;
	}
}
