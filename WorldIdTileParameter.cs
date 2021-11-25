using System;
using UnityEngine;

// Token: 0x02000160 RID: 352
public class WorldIdTileParameter : StringTileParameter
{
	// Token: 0x0600153C RID: 5436 RVA: 0x000944B4 File Offset: 0x000928B4
	public WorldIdTileParameter(int parameterIndex) : base(parameterIndex, false, false, string.Empty)
	{
	}

	// Token: 0x0600153D RID: 5437 RVA: 0x000944C4 File Offset: 0x000928C4
	public override GameObject SetupUI(Tile tile)
	{
		base.SetupUI(tile);
		this.startValue = (string)tile.gaf.Args[base.parameterIndex];
		if (string.IsNullOrEmpty(this.startValue))
		{
			this.startValue = WorldSession.worldIdClipboard;
		}
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
		Blocksworld.UI.Dialog.ShowWorldIdParameterEditorDialog(completion, textInputAction, this.startValue);
		return null;
	}
}
