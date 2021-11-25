using System;

// Token: 0x0200032A RID: 810
public class WorldUILayoutParameters
{
	// Token: 0x06002496 RID: 9366 RVA: 0x0010B27C File Offset: 0x0010967C
	public WorldUILayoutParameters()
	{
	}

	// Token: 0x06002497 RID: 9367 RVA: 0x0010B2CC File Offset: 0x001096CC
	public WorldUILayoutParameters(TILE_BUTTON button)
	{
		this.mainButtons = new TILE_BUTTON[]
		{
			button
		};
	}

	// Token: 0x06002498 RID: 9368 RVA: 0x0010B32C File Offset: 0x0010972C
	public WorldUILayoutParameters(TILE_BUTTON button1, TILE_BUTTON button2)
	{
		this.mainButtons = new TILE_BUTTON[]
		{
			button1,
			button2
		};
	}

	// Token: 0x06002499 RID: 9369 RVA: 0x0010B390 File Offset: 0x00109790
	public WorldUILayoutParameters(TILE_BUTTON button1, TILE_BUTTON button2, TILE_BUTTON button3)
	{
		this.mainButtons = new TILE_BUTTON[]
		{
			button1,
			button2,
			button3
		};
	}

	// Token: 0x0600249A RID: 9370 RVA: 0x0010B3F8 File Offset: 0x001097F8
	public WorldUILayoutParameters(TILE_BUTTON button1, TILE_BUTTON button2, TILE_BUTTON button3, TILE_BUTTON button4)
	{
		this.mainButtons = new TILE_BUTTON[]
		{
			button1,
			button2,
			button3,
			button4
		};
	}

	// Token: 0x0600249B RID: 9371 RVA: 0x0010B464 File Offset: 0x00109864
	public WorldUILayoutParameters(TILE_BUTTON[] buttons)
	{
		this.mainButtons = buttons;
	}

	// Token: 0x0600249C RID: 9372 RVA: 0x0010B4BC File Offset: 0x001098BC
	public WorldUILayoutParameters(WorldUILayoutParameters original)
	{
		this.mainButtons = (TILE_BUTTON[])original.mainButtons.Clone();
		this.includeUndoRedo = original.includeUndoRedo;
		this.includeLikeUnlike = original.includeLikeUnlike;
		this.includeTitleBar = original.includeTitleBar;
		this.includeVRCameraToggle = original.includeVRCameraToggle;
		this.includePurchasedBanner = original.includePurchasedBanner;
		this.includeBuildModelButton = original.includeBuildModelButton;
		this.includeBuyModelButton = original.includeBuyModelButton;
		this.includeTapedeck = original.includeTapedeck;
		this.includeRecord = original.includeRecord;
		this.buyModelPrice = original.buyModelPrice;
		this.titleBarText = original.titleBarText;
		this.titleBarSubtitle = original.titleBarSubtitle;
		this.titleBarHasCoinBalance = original.titleBarHasCoinBalance;
		this.titleBarCoinBalance = original.titleBarCoinBalance;
	}

	// Token: 0x04001F7E RID: 8062
	public TILE_BUTTON[] mainButtons = new TILE_BUTTON[]
	{
		TILE_BUTTON.HIDDEN,
		TILE_BUTTON.HIDDEN,
		TILE_BUTTON.HIDDEN
	};

	// Token: 0x04001F7F RID: 8063
	public bool includeUndoRedo;

	// Token: 0x04001F80 RID: 8064
	public bool includeLikeUnlike;

	// Token: 0x04001F81 RID: 8065
	public bool includeTitleBar;

	// Token: 0x04001F82 RID: 8066
	public bool includeVRCameraToggle;

	// Token: 0x04001F83 RID: 8067
	public bool includePurchasedBanner;

	// Token: 0x04001F84 RID: 8068
	public bool includeBuildModelButton;

	// Token: 0x04001F85 RID: 8069
	public bool includeBuyModelButton;

	// Token: 0x04001F86 RID: 8070
	public bool includeTapedeck = true;

	// Token: 0x04001F87 RID: 8071
	public bool includeRecord = true;

	// Token: 0x04001F88 RID: 8072
	public int buyModelPrice;

	// Token: 0x04001F89 RID: 8073
	public string titleBarText = string.Empty;

	// Token: 0x04001F8A RID: 8074
	public string titleBarSubtitle = string.Empty;

	// Token: 0x04001F8B RID: 8075
	public bool titleBarHasCoinBalance;

	// Token: 0x04001F8C RID: 8076
	public int titleBarCoinBalance;
}
