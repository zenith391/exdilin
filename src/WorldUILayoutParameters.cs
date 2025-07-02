public class WorldUILayoutParameters
{
	public TILE_BUTTON[] mainButtons = new TILE_BUTTON[3]
	{
		TILE_BUTTON.HIDDEN,
		TILE_BUTTON.HIDDEN,
		TILE_BUTTON.HIDDEN
	};

	public bool includeUndoRedo;

	public bool includeLikeUnlike;

	public bool includeTitleBar;

	public bool includeVRCameraToggle;

	public bool includePurchasedBanner;

	public bool includeBuildModelButton;

	public bool includeBuyModelButton;

	public bool includeTapedeck = true;

	public bool includeRecord = true;

	public int buyModelPrice;

	public string titleBarText = string.Empty;

	public string titleBarSubtitle = string.Empty;

	public bool titleBarHasCoinBalance;

	public int titleBarCoinBalance;

	public WorldUILayoutParameters()
	{
	}

	public WorldUILayoutParameters(TILE_BUTTON button)
	{
		mainButtons = new TILE_BUTTON[1] { button };
	}

	public WorldUILayoutParameters(TILE_BUTTON button1, TILE_BUTTON button2)
	{
		mainButtons = new TILE_BUTTON[2] { button1, button2 };
	}

	public WorldUILayoutParameters(TILE_BUTTON button1, TILE_BUTTON button2, TILE_BUTTON button3)
	{
		mainButtons = new TILE_BUTTON[3] { button1, button2, button3 };
	}

	public WorldUILayoutParameters(TILE_BUTTON button1, TILE_BUTTON button2, TILE_BUTTON button3, TILE_BUTTON button4)
	{
		mainButtons = new TILE_BUTTON[4] { button1, button2, button3, button4 };
	}

	public WorldUILayoutParameters(TILE_BUTTON[] buttons)
	{
		mainButtons = buttons;
	}

	public WorldUILayoutParameters(WorldUILayoutParameters original)
	{
		mainButtons = (TILE_BUTTON[])original.mainButtons.Clone();
		includeUndoRedo = original.includeUndoRedo;
		includeLikeUnlike = original.includeLikeUnlike;
		includeTitleBar = original.includeTitleBar;
		includeVRCameraToggle = original.includeVRCameraToggle;
		includePurchasedBanner = original.includePurchasedBanner;
		includeBuildModelButton = original.includeBuildModelButton;
		includeBuyModelButton = original.includeBuyModelButton;
		includeTapedeck = original.includeTapedeck;
		includeRecord = original.includeRecord;
		buyModelPrice = original.buyModelPrice;
		titleBarText = original.titleBarText;
		titleBarSubtitle = original.titleBarSubtitle;
		titleBarHasCoinBalance = original.titleBarHasCoinBalance;
		titleBarCoinBalance = original.titleBarCoinBalance;
	}
}
