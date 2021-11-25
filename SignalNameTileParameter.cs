using System;
using UnityEngine;

// Token: 0x0200015B RID: 347
internal class SignalNameTileParameter : StringTileParameter
{
	// Token: 0x0600150F RID: 5391 RVA: 0x0009386C File Offset: 0x00091C6C
	public SignalNameTileParameter(int parameterIndex) : base(parameterIndex, false, false, "Enter a signal name!")
	{
	}

	// Token: 0x06001510 RID: 5392 RVA: 0x0009387C File Offset: 0x00091C7C
	public static void ResetDefaultIndex()
	{
		SignalNameTileParameter.highestDefaultIndex = 0;
	}

	// Token: 0x06001511 RID: 5393 RVA: 0x00093884 File Offset: 0x00091C84
	public override GameObject SetupUI(Tile tile)
	{
		this.tile = tile;
		this.forceQuit = false;
		this.startState = Blocksworld.CurrentState;
		int num = SignalNameTileParameter.highestDefaultIndex + 1;
		this.startValue = (string)tile.gaf.Args[base.parameterIndex];
		if (this.startValue == "Signal")
		{
			this.startValue = "Signal " + num;
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
				SignalNameTileParameter.highestDefaultIndex++;
			}
			else if (text.Length > "Signal".Length + 1 && text.StartsWith("Signal "))
			{
				int b = 0;
				if (int.TryParse(text.Substring("Signal".Length + 1), out b))
				{
					SignalNameTileParameter.highestDefaultIndex = Mathf.Max(SignalNameTileParameter.highestDefaultIndex, b);
				}
			}
			TileIconManager.Instance.labelAtlas.AddNewLabel(text);
			Blocksworld.scriptPanel.ClearOverlays();
			Blocksworld.scriptPanel.UpdateAllOverlays();
			Blocksworld.bw.tileParameterEditor.StopEditing();
			this.tile.gaf = new GAF(this.tile.gaf.Predicate, new object[]
			{
				text,
				1f
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
		Blocksworld.UI.Dialog.ShowCustomNameEditor(completion, textInputAction, "signal");
		return null;
	}

	// Token: 0x06001512 RID: 5394 RVA: 0x00093937 File Offset: 0x00091D37
	public override bool UIUpdate()
	{
		return false;
	}

	// Token: 0x0400108B RID: 4235
	private static int highestDefaultIndex;
}
