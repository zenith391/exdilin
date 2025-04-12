using System;
using Blocks;
using UnityEngine;

// Token: 0x0200015C RID: 348
public class StringTileParameter : EditableTileParameter
{
	// Token: 0x06001516 RID: 5398 RVA: 0x00093573 File Offset: 0x00091973
	public StringTileParameter(int parameterIndex, bool multiline = true, bool acceptAnyInTutorial = true, string acceptAnyHint = "Enter some text!") : base(parameterIndex, false, 1)
	{
		this.multiline = multiline;
		this.acceptAnyInTutorial = acceptAnyInTutorial;
		this.acceptAnyHint = acceptAnyHint;
	}

	// Token: 0x06001517 RID: 5399 RVA: 0x000935A8 File Offset: 0x000919A8
	public override GameObject SetupUI(Tile tile)
	{
		base.SetupUI(tile);
		this.startValue = (string)tile.gaf.Args[base.parameterIndex];
		this.currentValue = this.startValue;
		this.forceQuit = false;
		this.done = false;
		Action completion = delegate()
		{
			if (string.IsNullOrEmpty(this.currentValue))
			{
				base.objectValue = string.Empty;
			}
			else if (this.multiline)
			{
				base.objectValue = this.currentValue.Trim();
			}
			else
			{
				int num = this.currentValue.IndexOfAny(new char[]
				{
					'\r',
					'\n'
				});
				string text = (num != -1) ? this.currentValue.Substring(0, num).Trim() : this.currentValue.Trim();
				base.objectValue = ((text.Length > 20) ? text.Substring(0, 20) : text);
			}
			this.done = true;
			this.startValue = null;
			this.currentValue = null;
		};
		Action<string> textInputAction = delegate(string text)
		{
			this.currentValue = text;
		};
		Blocksworld.UI.Dialog.ShowStringParameterEditorDialog(completion, textInputAction, this.startValue);
		return null;
	}

	// Token: 0x06001518 RID: 5400 RVA: 0x00093628 File Offset: 0x00091A28
	public override void CleanupUI()
	{
		this.done = false;
		if (Tutorial.state == TutorialState.SetParameter && this.goalTile != null && this.acceptAnyInTutorial)
		{
			Tutorial.AddOkParameterTile(this.goalTile);
		}
		this.goalTile = null;
		this.forceQuit = false;
		Tutorial.Step();
	}

	// Token: 0x06001519 RID: 5401 RVA: 0x0009367C File Offset: 0x00091A7C
	public override bool UIUpdate()
	{
		return false;
	}

	// Token: 0x0600151A RID: 5402 RVA: 0x0009367F File Offset: 0x00091A7F
	public override bool HasUIQuit()
	{
		return this.done;
	}

	// Token: 0x0600151B RID: 5403 RVA: 0x00093687 File Offset: 0x00091A87
	public override string ValueAsString()
	{
		return string.Empty;
	}

	// Token: 0x0600151C RID: 5404 RVA: 0x00093690 File Offset: 0x00091A90
	public override void HelpSetParameterValueInTutorial(Block block, Tile thisTile, Tile goalTile)
	{
		this.goalTile = goalTile;
		Tile selectedTile = Blocksworld.bw.tileParameterEditor.selectedTile;
		if (selectedTile != thisTile || !selectedTile.IsShowing())
		{
			Tutorial.HelpToggleTile(block, thisTile);
			return;
		}
		Tutorial.state = TutorialState.SetParameter;
	}

	// Token: 0x0600151D RID: 5405 RVA: 0x000936D5 File Offset: 0x00091AD5
	public void ForceQuit()
	{
		this.forceQuit = true;
	}

	// Token: 0x0600151E RID: 5406 RVA: 0x000936E0 File Offset: 0x00091AE0
	public override void OnHudMesh()
	{
		if (Tutorial.state == TutorialState.SetParameter)
		{
			string text = string.Empty;
			if (this.acceptAnyInTutorial)
			{
				text = this.acceptAnyHint;
			}
			else if (this.goalTile != null)
			{
				text = "Enter: '" + this.goalTile.gaf.Args[base.parameterIndex] + "'";
			}
			if (text != string.Empty)
			{
				Rect rect = new Rect(0f, 0f, (float)Screen.width, 100f);
				HudMeshStyle style = HudMeshOnGUI.dataSource.GetStyle("Counter");
				HudMeshOnGUI.Label(ref this.hintLabel, rect, text, style, 0f);
			}
		}
	}

	// Token: 0x0400108C RID: 4236
	protected TouchScreenKeyboard keyboard;

	// Token: 0x0400108D RID: 4237
	protected string startValue;

	// Token: 0x0400108E RID: 4238
	protected string currentValue;

	// Token: 0x0400108F RID: 4239
	protected bool done;

	// Token: 0x04001090 RID: 4240
	protected bool multiline;

	// Token: 0x04001091 RID: 4241
	protected bool acceptAnyInTutorial = true;

	// Token: 0x04001092 RID: 4242
	protected string acceptAnyHint = string.Empty;

	// Token: 0x04001093 RID: 4243
	protected Tile goalTile;

	// Token: 0x04001094 RID: 4244
	protected State startState;

	// Token: 0x04001095 RID: 4245
	private HudMeshLabel hintLabel;

	// Token: 0x04001096 RID: 4246
	protected bool forceQuit;
}
