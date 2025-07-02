public static class BWShoppingCartEvents
{
	private static event ShoppingCartEventHandler shoppingCartDidLoad;

	private static event ShoppingCartEventHandler shoppingCartDidClear;

	private static event ShoppingCartItemEventHandler shoppingCartDidAddItem;

	private static event ShoppingCartItemEventHandler shoppingCartDidRemoveItem;

	private static event ShoppingCartItemEventHandler shoppingCartDidUpdateItem;

	public static void AddListeners(ShoppingCartEventHandler cartListener, ShoppingCartItemEventHandler cartItemListener)
	{
		AddShoppingCartDidLoadListener(cartListener);
		AddShoppingCartDidClearListener(cartListener);
		AddShoppingCartDidAddItemListener(cartItemListener);
		AddShoppingCartDidRemoveItemListener(cartItemListener);
		AddShoppingCartDidUpdateItemListener(cartItemListener);
	}

	public static void RemoveListeners(ShoppingCartEventHandler cartListener, ShoppingCartItemEventHandler cartItemListener)
	{
		RemoveShoppingCartDidLoadListener(cartListener);
		RemoveShoppingCartDidClearListener(cartListener);
		RemoveShoppingCartDidAddItemListener(cartItemListener);
		RemoveShoppingCartDidRemoveItemListener(cartItemListener);
		RemoveShoppingCartDidUpdateItemListener(cartItemListener);
	}

	public static void AddShoppingCartDidLoadListener(ShoppingCartEventHandler listener)
	{
		shoppingCartDidLoad -= listener;
		shoppingCartDidLoad += listener;
	}

	public static void RemoveShoppingCartDidLoadListener(ShoppingCartEventHandler listener)
	{
		shoppingCartDidLoad -= listener;
	}

	public static void OnShoppingCartLoad(BWShoppingCart cart)
	{
		if (BWShoppingCartEvents.shoppingCartDidLoad != null)
		{
			BWShoppingCartEvents.shoppingCartDidLoad(cart);
		}
	}

	public static void AddShoppingCartDidClearListener(ShoppingCartEventHandler listener)
	{
		shoppingCartDidClear -= listener;
		shoppingCartDidClear += listener;
	}

	public static void RemoveShoppingCartDidClearListener(ShoppingCartEventHandler listener)
	{
		shoppingCartDidClear -= listener;
	}

	public static void OnShoppingCartClear(BWShoppingCart cart)
	{
		if (BWShoppingCartEvents.shoppingCartDidClear != null)
		{
			BWShoppingCartEvents.shoppingCartDidClear(cart);
		}
	}

	public static void AddShoppingCartDidAddItemListener(ShoppingCartItemEventHandler listener)
	{
		shoppingCartDidAddItem -= listener;
		shoppingCartDidAddItem += listener;
	}

	public static void RemoveShoppingCartDidAddItemListener(ShoppingCartItemEventHandler listener)
	{
		shoppingCartDidAddItem -= listener;
	}

	public static void OnShoppingCartAddItem(BWShoppingCart cart, int itemIndex)
	{
		if (BWShoppingCartEvents.shoppingCartDidAddItem != null)
		{
			BWShoppingCartEvents.shoppingCartDidAddItem(cart, itemIndex);
		}
	}

	public static void AddShoppingCartDidRemoveItemListener(ShoppingCartItemEventHandler listener)
	{
		shoppingCartDidRemoveItem -= listener;
		shoppingCartDidRemoveItem += listener;
	}

	public static void RemoveShoppingCartDidRemoveItemListener(ShoppingCartItemEventHandler listener)
	{
		shoppingCartDidRemoveItem -= listener;
	}

	public static void OnShoppingCartRemoveItem(BWShoppingCart cart, int itemIndex)
	{
		if (BWShoppingCartEvents.shoppingCartDidRemoveItem != null)
		{
			BWShoppingCartEvents.shoppingCartDidRemoveItem(cart, itemIndex);
		}
	}

	public static void AddShoppingCartDidUpdateItemListener(ShoppingCartItemEventHandler listener)
	{
		shoppingCartDidUpdateItem -= listener;
		shoppingCartDidUpdateItem += listener;
	}

	public static void RemoveShoppingCartDidUpdateItemListener(ShoppingCartItemEventHandler listener)
	{
		shoppingCartDidUpdateItem -= listener;
	}

	public static void OnShoppingCartUpdateItem(BWShoppingCart cart, int itemIndex)
	{
		if (BWShoppingCartEvents.shoppingCartDidUpdateItem != null)
		{
			BWShoppingCartEvents.shoppingCartDidUpdateItem(cart, itemIndex);
		}
	}
}
