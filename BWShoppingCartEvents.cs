using System;

// Token: 0x020003B3 RID: 947
public static class BWShoppingCartEvents
{
	// Token: 0x14000016 RID: 22
	// (add) Token: 0x06002927 RID: 10535 RVA: 0x0012E23C File Offset: 0x0012C63C
	// (remove) Token: 0x06002928 RID: 10536 RVA: 0x0012E270 File Offset: 0x0012C670
	private static event ShoppingCartEventHandler shoppingCartDidLoad;

	// Token: 0x14000017 RID: 23
	// (add) Token: 0x06002929 RID: 10537 RVA: 0x0012E2A4 File Offset: 0x0012C6A4
	// (remove) Token: 0x0600292A RID: 10538 RVA: 0x0012E2D8 File Offset: 0x0012C6D8
	private static event ShoppingCartEventHandler shoppingCartDidClear;

	// Token: 0x14000018 RID: 24
	// (add) Token: 0x0600292B RID: 10539 RVA: 0x0012E30C File Offset: 0x0012C70C
	// (remove) Token: 0x0600292C RID: 10540 RVA: 0x0012E340 File Offset: 0x0012C740
	private static event ShoppingCartItemEventHandler shoppingCartDidAddItem;

	// Token: 0x14000019 RID: 25
	// (add) Token: 0x0600292D RID: 10541 RVA: 0x0012E374 File Offset: 0x0012C774
	// (remove) Token: 0x0600292E RID: 10542 RVA: 0x0012E3A8 File Offset: 0x0012C7A8
	private static event ShoppingCartItemEventHandler shoppingCartDidRemoveItem;

	// Token: 0x1400001A RID: 26
	// (add) Token: 0x0600292F RID: 10543 RVA: 0x0012E3DC File Offset: 0x0012C7DC
	// (remove) Token: 0x06002930 RID: 10544 RVA: 0x0012E410 File Offset: 0x0012C810
	private static event ShoppingCartItemEventHandler shoppingCartDidUpdateItem;

	// Token: 0x06002931 RID: 10545 RVA: 0x0012E444 File Offset: 0x0012C844
	public static void AddListeners(ShoppingCartEventHandler cartListener, ShoppingCartItemEventHandler cartItemListener)
	{
		BWShoppingCartEvents.AddShoppingCartDidLoadListener(cartListener);
		BWShoppingCartEvents.AddShoppingCartDidClearListener(cartListener);
		BWShoppingCartEvents.AddShoppingCartDidAddItemListener(cartItemListener);
		BWShoppingCartEvents.AddShoppingCartDidRemoveItemListener(cartItemListener);
		BWShoppingCartEvents.AddShoppingCartDidUpdateItemListener(cartItemListener);
	}

	// Token: 0x06002932 RID: 10546 RVA: 0x0012E464 File Offset: 0x0012C864
	public static void RemoveListeners(ShoppingCartEventHandler cartListener, ShoppingCartItemEventHandler cartItemListener)
	{
		BWShoppingCartEvents.RemoveShoppingCartDidLoadListener(cartListener);
		BWShoppingCartEvents.RemoveShoppingCartDidClearListener(cartListener);
		BWShoppingCartEvents.RemoveShoppingCartDidAddItemListener(cartItemListener);
		BWShoppingCartEvents.RemoveShoppingCartDidRemoveItemListener(cartItemListener);
		BWShoppingCartEvents.RemoveShoppingCartDidUpdateItemListener(cartItemListener);
	}

	// Token: 0x06002933 RID: 10547 RVA: 0x0012E484 File Offset: 0x0012C884
	public static void AddShoppingCartDidLoadListener(ShoppingCartEventHandler listener)
	{
		BWShoppingCartEvents.shoppingCartDidLoad -= listener;
		BWShoppingCartEvents.shoppingCartDidLoad += listener;
	}

	// Token: 0x06002934 RID: 10548 RVA: 0x0012E492 File Offset: 0x0012C892
	public static void RemoveShoppingCartDidLoadListener(ShoppingCartEventHandler listener)
	{
		BWShoppingCartEvents.shoppingCartDidLoad -= listener;
	}

	// Token: 0x06002935 RID: 10549 RVA: 0x0012E49A File Offset: 0x0012C89A
	public static void OnShoppingCartLoad(BWShoppingCart cart)
	{
		if (BWShoppingCartEvents.shoppingCartDidLoad != null)
		{
			BWShoppingCartEvents.shoppingCartDidLoad(cart);
		}
	}

	// Token: 0x06002936 RID: 10550 RVA: 0x0012E4B1 File Offset: 0x0012C8B1
	public static void AddShoppingCartDidClearListener(ShoppingCartEventHandler listener)
	{
		BWShoppingCartEvents.shoppingCartDidClear -= listener;
		BWShoppingCartEvents.shoppingCartDidClear += listener;
	}

	// Token: 0x06002937 RID: 10551 RVA: 0x0012E4BF File Offset: 0x0012C8BF
	public static void RemoveShoppingCartDidClearListener(ShoppingCartEventHandler listener)
	{
		BWShoppingCartEvents.shoppingCartDidClear -= listener;
	}

	// Token: 0x06002938 RID: 10552 RVA: 0x0012E4C7 File Offset: 0x0012C8C7
	public static void OnShoppingCartClear(BWShoppingCart cart)
	{
		if (BWShoppingCartEvents.shoppingCartDidClear != null)
		{
			BWShoppingCartEvents.shoppingCartDidClear(cart);
		}
	}

	// Token: 0x06002939 RID: 10553 RVA: 0x0012E4DE File Offset: 0x0012C8DE
	public static void AddShoppingCartDidAddItemListener(ShoppingCartItemEventHandler listener)
	{
		BWShoppingCartEvents.shoppingCartDidAddItem -= listener;
		BWShoppingCartEvents.shoppingCartDidAddItem += listener;
	}

	// Token: 0x0600293A RID: 10554 RVA: 0x0012E4EC File Offset: 0x0012C8EC
	public static void RemoveShoppingCartDidAddItemListener(ShoppingCartItemEventHandler listener)
	{
		BWShoppingCartEvents.shoppingCartDidAddItem -= listener;
	}

	// Token: 0x0600293B RID: 10555 RVA: 0x0012E4F4 File Offset: 0x0012C8F4
	public static void OnShoppingCartAddItem(BWShoppingCart cart, int itemIndex)
	{
		if (BWShoppingCartEvents.shoppingCartDidAddItem != null)
		{
			BWShoppingCartEvents.shoppingCartDidAddItem(cart, itemIndex);
		}
	}

	// Token: 0x0600293C RID: 10556 RVA: 0x0012E50C File Offset: 0x0012C90C
	public static void AddShoppingCartDidRemoveItemListener(ShoppingCartItemEventHandler listener)
	{
		BWShoppingCartEvents.shoppingCartDidRemoveItem -= listener;
		BWShoppingCartEvents.shoppingCartDidRemoveItem += listener;
	}

	// Token: 0x0600293D RID: 10557 RVA: 0x0012E51A File Offset: 0x0012C91A
	public static void RemoveShoppingCartDidRemoveItemListener(ShoppingCartItemEventHandler listener)
	{
		BWShoppingCartEvents.shoppingCartDidRemoveItem -= listener;
	}

	// Token: 0x0600293E RID: 10558 RVA: 0x0012E522 File Offset: 0x0012C922
	public static void OnShoppingCartRemoveItem(BWShoppingCart cart, int itemIndex)
	{
		if (BWShoppingCartEvents.shoppingCartDidRemoveItem != null)
		{
			BWShoppingCartEvents.shoppingCartDidRemoveItem(cart, itemIndex);
		}
	}

	// Token: 0x0600293F RID: 10559 RVA: 0x0012E53A File Offset: 0x0012C93A
	public static void AddShoppingCartDidUpdateItemListener(ShoppingCartItemEventHandler listener)
	{
		BWShoppingCartEvents.shoppingCartDidUpdateItem -= listener;
		BWShoppingCartEvents.shoppingCartDidUpdateItem += listener;
	}

	// Token: 0x06002940 RID: 10560 RVA: 0x0012E548 File Offset: 0x0012C948
	public static void RemoveShoppingCartDidUpdateItemListener(ShoppingCartItemEventHandler listener)
	{
		BWShoppingCartEvents.shoppingCartDidUpdateItem -= listener;
	}

	// Token: 0x06002941 RID: 10561 RVA: 0x0012E550 File Offset: 0x0012C950
	public static void OnShoppingCartUpdateItem(BWShoppingCart cart, int itemIndex)
	{
		if (BWShoppingCartEvents.shoppingCartDidUpdateItem != null)
		{
			BWShoppingCartEvents.shoppingCartDidUpdateItem(cart, itemIndex);
		}
	}
}
