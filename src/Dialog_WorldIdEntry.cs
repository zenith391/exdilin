public class Dialog_WorldIdEntry : Dialog_TextInput
{
	public UIWorldInfo worldInfoCell;

	public override void DidEditText(string text)
	{
		if (int.TryParse(text, out var result) && result > 0)
		{
			WorldInfo.Get(result, SetWorldInfo, ClearWorldInfo);
		}
		base.DidEditText(text);
	}

	public void SetWorldInfo(WorldInfo worldInfo)
	{
		worldInfoCell.LoadWorldInfo(worldInfo);
	}

	public void ClearWorldInfo()
	{
		worldInfoCell.ClearWorldInfo();
	}
}
