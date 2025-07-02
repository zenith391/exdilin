using UnityEngine;

namespace TMPro.Examples;

public class TMP_TextEventCheck : MonoBehaviour
{
	public TMP_TextEventHandler TextEventHandler;

	private void OnEnable()
	{
		if (TextEventHandler != null)
		{
			TextEventHandler.onCharacterSelection.AddListener(OnCharacterSelection);
			TextEventHandler.onWordSelection.AddListener(OnWordSelection);
			TextEventHandler.onLineSelection.AddListener(OnLineSelection);
			TextEventHandler.onLinkSelection.AddListener(OnLinkSelection);
		}
	}

	private void OnDisable()
	{
		if (TextEventHandler != null)
		{
			TextEventHandler.onCharacterSelection.RemoveListener(OnCharacterSelection);
			TextEventHandler.onWordSelection.RemoveListener(OnWordSelection);
			TextEventHandler.onLineSelection.RemoveListener(OnLineSelection);
			TextEventHandler.onLinkSelection.RemoveListener(OnLinkSelection);
		}
	}

	private void OnCharacterSelection(char c, int index)
	{
		Debug.Log("Character [" + c + "] at Index: " + index + " has been selected.");
	}

	private void OnWordSelection(string word, int firstCharacterIndex, int length)
	{
		Debug.Log("Word [" + word + "] with first character index of " + firstCharacterIndex + " and length of " + length + " has been selected.");
	}

	private void OnLineSelection(string lineText, int firstCharacterIndex, int length)
	{
		Debug.Log("Line [" + lineText + "] with first character index of " + firstCharacterIndex + " and length of " + length + " has been selected.");
	}

	private void OnLinkSelection(string linkID, string linkText, int linkIndex)
	{
		Debug.Log("Link Index: " + linkIndex + " with ID [" + linkID + "] and Text \"" + linkText + "\" has been selected.");
	}
}
