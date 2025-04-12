using System;
using UnityEngine;

// Token: 0x020002CA RID: 714
public class TileCustom
{
	// Token: 0x060020B4 RID: 8372 RVA: 0x000EFDC7 File Offset: 0x000EE1C7
	public TileCustom(GAF gaf, bool showBackground, Color[] backgroundColors)
	{
		this.showBackground = showBackground;
		this.backgroundColors = backgroundColors;
		this.tileObject = Blocksworld.tilePool.GetTileObject(gaf, true, false);
	}

	// Token: 0x060020B5 RID: 8373 RVA: 0x000EFDF7 File Offset: 0x000EE1F7
	public TileCustom(TileObject tileObject)
	{
		this.showBackground = false;
		this.tileObject = tileObject;
	}

	// Token: 0x060020B6 RID: 8374 RVA: 0x000EFE14 File Offset: 0x000EE214
	public void SetGaf(GAF gaf)
	{
		if (this.tileObject.IsSetupForGAF(gaf) && this.tileObject.IsEnabled())
		{
			return;
		}
		this.tileObject.Setup(gaf, true);
	}

	// Token: 0x060020B7 RID: 8375 RVA: 0x000EFE45 File Offset: 0x000EE245
	public void SetIcon(string iconName)
	{
		if (this.tileObject.IconName() == iconName && this.tileObject.IsEnabled())
		{
			return;
		}
		this.tileObject.SetupForIcon(iconName, this.tileObject.IsEnabled());
	}

	// Token: 0x060020B8 RID: 8376 RVA: 0x000EFE85 File Offset: 0x000EE285
	public void Show(bool show)
	{
		if (show)
		{
			this.tileObject.Show();
		}
		else
		{
			this.tileObject.Hide();
		}
	}

	// Token: 0x060020B9 RID: 8377 RVA: 0x000EFEA8 File Offset: 0x000EE2A8
	public bool IsShown()
	{
		return this.tileObject.IsActive();
	}

	// Token: 0x060020BA RID: 8378 RVA: 0x000EFEB5 File Offset: 0x000EE2B5
	public void Destroy()
	{
		this.tileObject.ReturnToPool();
	}

	// Token: 0x060020BB RID: 8379 RVA: 0x000EFEC2 File Offset: 0x000EE2C2
	public void Enable(bool enable)
	{
		if (enable)
		{
			this.tileObject.Enable();
		}
		else
		{
			this.tileObject.Disable();
		}
	}

	// Token: 0x060020BC RID: 8380 RVA: 0x000EFEE5 File Offset: 0x000EE2E5
	public void MoveTo(float x, float y, float z)
	{
		this.position = new Vector3(Mathf.Floor(x), Mathf.Floor(y), z);
		this.tileObject.MoveTo(this.position);
	}

	// Token: 0x04001BCD RID: 7117
	private bool showBackground = true;

	// Token: 0x04001BCE RID: 7118
	private Color[] backgroundColors;

	// Token: 0x04001BCF RID: 7119
	private TileObject tileObject;

	// Token: 0x04001BD0 RID: 7120
	public Vector3 position;
}
