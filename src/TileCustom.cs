using UnityEngine;

public class TileCustom
{
	private bool showBackground = true;

	private Color[] backgroundColors;

	private TileObject tileObject;

	public Vector3 position;

	public TileCustom(GAF gaf, bool showBackground, Color[] backgroundColors)
	{
		this.showBackground = showBackground;
		this.backgroundColors = backgroundColors;
		tileObject = Blocksworld.tilePool.GetTileObject(gaf, enabled: true);
	}

	public TileCustom(TileObject tileObject)
	{
		showBackground = false;
		this.tileObject = tileObject;
	}

	public void SetGaf(GAF gaf)
	{
		if (!tileObject.IsSetupForGAF(gaf) || !tileObject.IsEnabled())
		{
			tileObject.Setup(gaf, enabled: true);
		}
	}

	public void SetIcon(string iconName)
	{
		if (!(tileObject.IconName() == iconName) || !tileObject.IsEnabled())
		{
			tileObject.SetupForIcon(iconName, tileObject.IsEnabled());
		}
	}

	public void Show(bool show)
	{
		if (show)
		{
			tileObject.Show();
		}
		else
		{
			tileObject.Hide();
		}
	}

	public bool IsShown()
	{
		return tileObject.IsActive();
	}

	public void Destroy()
	{
		tileObject.ReturnToPool();
	}

	public void Enable(bool enable)
	{
		if (enable)
		{
			tileObject.Enable();
		}
		else
		{
			tileObject.Disable();
		}
	}

	public void MoveTo(float x, float y, float z)
	{
		position = new Vector3(Mathf.Floor(x), Mathf.Floor(y), z);
		tileObject.MoveTo(position);
	}
}
