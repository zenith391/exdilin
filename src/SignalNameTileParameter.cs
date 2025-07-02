using System;
using UnityEngine;

internal class SignalNameTileParameter : StringTileParameter
{
	private static int highestDefaultIndex;

	public SignalNameTileParameter(int parameterIndex)
		: base(parameterIndex, multiline: false, acceptAnyInTutorial: false, "Enter a signal name!")
	{
	}

	public static void ResetDefaultIndex()
	{
		highestDefaultIndex = 0;
	}

	public override GameObject SetupUI(Tile tile)
	{
		base.tile = tile;
		forceQuit = false;
		startState = Blocksworld.CurrentState;
		int num = highestDefaultIndex + 1;
		startValue = (string)tile.gaf.Args[base.parameterIndex];
		if (startValue == "Signal")
		{
			startValue = "Signal " + num;
		}
		currentValue = startValue;
		Action completion = delegate
		{
			string text = currentValue.Trim();
			if (string.IsNullOrEmpty(text) || text == "*")
			{
				base.objectValue = startValue;
			}
			else
			{
				base.objectValue = text;
			}
			text = (string)base.objectValue;
			if (text == startValue)
			{
				highestDefaultIndex++;
			}
			else if (text.Length > "Signal".Length + 1 && text.StartsWith("Signal "))
			{
				int result = 0;
				if (int.TryParse(text.Substring("Signal".Length + 1), out result))
				{
					highestDefaultIndex = Mathf.Max(highestDefaultIndex, result);
				}
			}
			TileIconManager.Instance.labelAtlas.AddNewLabel(text);
			Blocksworld.scriptPanel.ClearOverlays();
			Blocksworld.scriptPanel.UpdateAllOverlays();
			Blocksworld.bw.tileParameterEditor.StopEditing();
			base.tile.gaf = new GAF(base.tile.gaf.Predicate, text, 1f);
			base.tile.Show(show: true);
			base.tile.tileObject.Setup(base.tile.gaf, enabled: true);
			Blocksworld.buildPanel.Layout();
			startValue = null;
			currentValue = null;
		};
		Action<string> textInputAction = delegate(string name)
		{
			currentValue = name;
		};
		Blocksworld.UI.Dialog.ShowCustomNameEditor(completion, textInputAction, "signal");
		return null;
	}

	public override bool UIUpdate()
	{
		return false;
	}
}
