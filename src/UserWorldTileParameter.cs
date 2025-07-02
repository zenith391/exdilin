using System;
using UnityEngine;

public class UserWorldTileParameter : EditableTileParameter
{
	private string startValue;

	private string currentValue;

	private bool done;

	public UserWorldTileParameter(int parameterIndex)
		: base(parameterIndex, useDoubleWidth: false)
	{
	}

	public override GameObject SetupUI(Tile tile)
	{
		base.SetupUI(tile);
		currentValue = (string)tile.gaf.Args[base.parameterIndex];
		done = false;
		Action<string> completion = delegate(string s)
		{
			currentValue = s;
			if (string.IsNullOrEmpty(currentValue))
			{
				base.objectValue = string.Empty;
			}
			else
			{
				base.objectValue = currentValue;
			}
			base.tile.gaf = new GAF(base.tile.gaf.Predicate, currentValue);
			base.tile.Show(show: true);
			base.tile.tileObject.Setup(base.tile.gaf, enabled: true);
			done = true;
			currentValue = null;
		};
		Blocksworld.UI.Dialog.ShowCurrentUserWorldList(completion, currentValue);
		return null;
	}

	public override bool HasUIQuit()
	{
		return done;
	}
}
