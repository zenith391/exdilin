using System;
using UnityEngine;

// Token: 0x0200015E RID: 350
public class UserWorldTileParameter : EditableTileParameter
{
	// Token: 0x06001531 RID: 5425 RVA: 0x00094148 File Offset: 0x00092548
	public UserWorldTileParameter(int parameterIndex) : base(parameterIndex, false, 1)
	{
	}

	// Token: 0x06001532 RID: 5426 RVA: 0x00094154 File Offset: 0x00092554
	public override GameObject SetupUI(Tile tile)
	{
		base.SetupUI(tile);
		this.currentValue = (string)tile.gaf.Args[base.parameterIndex];
		this.done = false;
		Action<string> completion = delegate(string s)
		{
			this.currentValue = s;
			if (string.IsNullOrEmpty(this.currentValue))
			{
				base.objectValue = string.Empty;
			}
			else
			{
				base.objectValue = this.currentValue;
			}
			this.tile.gaf = new GAF(this.tile.gaf.Predicate, new object[]
			{
				this.currentValue
			});
			this.tile.Show(true);
			this.tile.tileObject.Setup(this.tile.gaf, true);
			this.done = true;
			this.currentValue = null;
		};
		Blocksworld.UI.Dialog.ShowCurrentUserWorldList(completion, this.currentValue);
		return null;
	}

	// Token: 0x06001533 RID: 5427 RVA: 0x000941B1 File Offset: 0x000925B1
	public override bool HasUIQuit()
	{
		return this.done;
	}

	// Token: 0x0400109F RID: 4255
	private string startValue;

	// Token: 0x040010A0 RID: 4256
	private string currentValue;

	// Token: 0x040010A1 RID: 4257
	private bool done;
}
