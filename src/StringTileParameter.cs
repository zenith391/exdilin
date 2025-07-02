using System;
using Blocks;
using UnityEngine;

public class StringTileParameter : EditableTileParameter
{
	protected TouchScreenKeyboard keyboard;

	protected string startValue;

	protected string currentValue;

	protected bool done;

	protected bool multiline;

	protected bool acceptAnyInTutorial = true;

	protected string acceptAnyHint = string.Empty;

	protected Tile goalTile;

	protected State startState;

	private HudMeshLabel hintLabel;

	protected bool forceQuit;

	public StringTileParameter(int parameterIndex, bool multiline = true, bool acceptAnyInTutorial = true, string acceptAnyHint = "Enter some text!")
		: base(parameterIndex, useDoubleWidth: false)
	{
		this.multiline = multiline;
		this.acceptAnyInTutorial = acceptAnyInTutorial;
		this.acceptAnyHint = acceptAnyHint;
	}

	public override GameObject SetupUI(Tile tile)
	{
		base.SetupUI(tile);
		startValue = (string)tile.gaf.Args[base.parameterIndex];
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
		Blocksworld.UI.Dialog.ShowStringParameterEditorDialog(completion, textInputAction, startValue);
		return null;
	}

	public override void CleanupUI()
	{
		done = false;
		if (Tutorial.state == TutorialState.SetParameter && goalTile != null && acceptAnyInTutorial)
		{
			Tutorial.AddOkParameterTile(goalTile);
		}
		goalTile = null;
		forceQuit = false;
		Tutorial.Step();
	}

	public override bool UIUpdate()
	{
		return false;
	}

	public override bool HasUIQuit()
	{
		return done;
	}

	public override string ValueAsString()
	{
		return string.Empty;
	}

	public override void HelpSetParameterValueInTutorial(Block block, Tile thisTile, Tile goalTile)
	{
		this.goalTile = goalTile;
		Tile selectedTile = Blocksworld.bw.tileParameterEditor.selectedTile;
		if (selectedTile != thisTile || !selectedTile.IsShowing())
		{
			Tutorial.HelpToggleTile(block, thisTile);
		}
		else
		{
			Tutorial.state = TutorialState.SetParameter;
		}
	}

	public void ForceQuit()
	{
		forceQuit = true;
	}

	public override void OnHudMesh()
	{
		if (Tutorial.state == TutorialState.SetParameter)
		{
			string text = string.Empty;
			if (acceptAnyInTutorial)
			{
				text = acceptAnyHint;
			}
			else if (goalTile != null)
			{
				text = "Enter: '" + goalTile.gaf.Args[base.parameterIndex]?.ToString() + "'";
			}
			if (text != string.Empty)
			{
				Rect rect = new Rect(0f, 0f, Screen.width, 100f);
				HudMeshStyle hudMeshStyle = HudMeshOnGUI.dataSource.GetStyle("Counter");
				HudMeshOnGUI.Label(ref hintLabel, rect, text, hudMeshStyle);
			}
		}
	}
}
