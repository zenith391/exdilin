using System;

// Token: 0x020002F3 RID: 755
public class Dialog_WorldIdEntry : Dialog_TextInput
{
	// Token: 0x06002233 RID: 8755 RVA: 0x000FF474 File Offset: 0x000FD874
	public override void DidEditText(string text)
	{
		int num;
		if (int.TryParse(text, out num) && num > 0)
		{
			WorldInfo.Get(num, new Action<WorldInfo>(this.SetWorldInfo), new Action(this.ClearWorldInfo));
		}
		base.DidEditText(text);
	}

	// Token: 0x06002234 RID: 8756 RVA: 0x000FF4BA File Offset: 0x000FD8BA
	public void SetWorldInfo(WorldInfo worldInfo)
	{
		this.worldInfoCell.LoadWorldInfo(worldInfo);
	}

	// Token: 0x06002235 RID: 8757 RVA: 0x000FF4C8 File Offset: 0x000FD8C8
	public void ClearWorldInfo()
	{
		this.worldInfoCell.ClearWorldInfo();
	}

	// Token: 0x04001D31 RID: 7473
	public UIWorldInfo worldInfoCell;
}
