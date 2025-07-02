using System;
using UnityEngine;

public class WorldIdTileParameter : StringTileParameter
{
	public WorldIdTileParameter(int parameterIndex)
		: base(parameterIndex, multiline: false, acceptAnyInTutorial: false, string.Empty)
	{
	}

	public override GameObject SetupUI(Tile tile)
	{
		base.SetupUI(tile);
		startValue = (string)tile.gaf.Args[base.parameterIndex];
		if (string.IsNullOrEmpty(startValue))
		{
			startValue = WorldSession.worldIdClipboard;
		}
		currentValue = startValue;
		forceQuit = false;
		done = false;
		Action completion = delegate
		{
			if (string.IsNullOrEmpty(currentValue))
			{
				base.objectValue = string.Empty;
			}
			else if (multiline)
			{
				base.objectValue = currentValue.Trim();
			}
			else
			{
				int num = currentValue.IndexOfAny(new char[2] { '\r', '\n' });
				string text = ((num != -1) ? currentValue.Substring(0, num).Trim() : currentValue.Trim());
				base.objectValue = ((text.Length > 20) ? text.Substring(0, 20) : text);
			}
			done = true;
			startValue = null;
			currentValue = null;
		};
		Action<string> textInputAction = delegate(string text)
		{
			currentValue = text;
		};
		Blocksworld.UI.Dialog.ShowWorldIdParameterEditorDialog(completion, textInputAction, startValue);
		return null;
	}
}
