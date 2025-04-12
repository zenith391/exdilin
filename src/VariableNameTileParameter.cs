using System;
using UnityEngine;

// Token: 0x0200015F RID: 351
internal class VariableNameTileParameter : StringTileParameter
{
	// Token: 0x06001535 RID: 5429 RVA: 0x00094261 File Offset: 0x00092661
	public VariableNameTileParameter(int parameterIndex) : base(parameterIndex, false, false, "Enter a variable name!")
	{
	}

	// Token: 0x06001536 RID: 5430 RVA: 0x00094271 File Offset: 0x00092671
	public static void ResetDefaultIndex()
	{
		VariableNameTileParameter.highestDefaultIndex = 0;
	}

	// Token: 0x06001537 RID: 5431 RVA: 0x0009427C File Offset: 0x0009267C
	public override GameObject SetupUI(Tile tile)
	{
		this.tile = tile;
		this.forceQuit = false;
		this.startState = Blocksworld.CurrentState;
		int num = VariableNameTileParameter.highestDefaultIndex + 1;
		this.startValue = (string)tile.gaf.Args[base.parameterIndex];
		if (this.startValue == "Int")
		{
			this.startValue = "Int " + num;
		}
		this.currentValue = this.startValue;
		Action completion = delegate()
		{
			string text = this.currentValue.Trim();
			if (string.IsNullOrEmpty(text) || text == "*")
			{
				base.objectValue = this.startValue;
			}
			else
			{
				base.objectValue = text;
			}
			text = (string)base.objectValue;
			if (text == this.startValue)
			{
				VariableNameTileParameter.highestDefaultIndex++;
			}
			else if (text.Length > "Int".Length + 1 && text.StartsWith("Int "))
			{
				int b = 0;
				if (int.TryParse(text.Substring("Int".Length + 1), out b))
				{
					VariableNameTileParameter.highestDefaultIndex = Mathf.Max(VariableNameTileParameter.highestDefaultIndex, b);
				}
			}
			TileIconManager.Instance.labelAtlas.AddNewLabel(text);
			Blocksworld.scriptPanel.ClearOverlays();
			Blocksworld.scriptPanel.UpdateAllOverlays();
			Blocksworld.bw.tileParameterEditor.StopEditing();
			this.tile.gaf = new GAF(this.tile.gaf.Predicate, new object[]
			{
				text,
				0
			});
			this.tile.Show(true);
			this.tile.tileObject.Setup(this.tile.gaf, true);
			Blocksworld.buildPanel.Layout();
			this.startValue = null;
			this.currentValue = null;
		};
		Action<string> textInputAction = delegate(string name)
		{
			this.currentValue = name;
		};
		Blocksworld.UI.Dialog.ShowCustomNameEditor(completion, textInputAction, "variable");
		return null;
	}

	// Token: 0x06001538 RID: 5432 RVA: 0x0009432F File Offset: 0x0009272F
	public override bool UIUpdate()
	{
		return false;
	}

	// Token: 0x040010A2 RID: 4258
	private static int highestDefaultIndex;
}
