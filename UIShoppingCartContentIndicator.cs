using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200045B RID: 1115
public class UIShoppingCartContentIndicator : MonoBehaviour
{
	// Token: 0x06002F34 RID: 12084 RVA: 0x0014EAB8 File Offset: 0x0014CEB8
	private void OnEnable()
	{
		if (this.countText != null)
		{
			this.countText.text = BWStandalone.ShoppingCart.itemCount.ToString();
		}
		BWShoppingCartEvents.AddListeners(new ShoppingCartEventHandler(this.UpdateCount), new ShoppingCartItemEventHandler(this.UpdateCountForItem));
	}

	// Token: 0x06002F35 RID: 12085 RVA: 0x0014EB16 File Offset: 0x0014CF16
	private void OnDisable()
	{
		BWShoppingCartEvents.RemoveListeners(new ShoppingCartEventHandler(this.UpdateCount), new ShoppingCartItemEventHandler(this.UpdateCountForItem));
	}

	// Token: 0x06002F36 RID: 12086 RVA: 0x0014EB35 File Offset: 0x0014CF35
	private void UpdateCountForItem(BWShoppingCart cart, int index)
	{
		this.UpdateCount(cart);
	}

	// Token: 0x06002F37 RID: 12087 RVA: 0x0014EB40 File Offset: 0x0014CF40
	private void UpdateCount(BWShoppingCart cart)
	{
		if (this.countText != null)
		{
			this.countText.text = cart.itemCount.ToString();
			this.countText.enabled = (cart.itemCount > 0);
		}
		if (this.animator != null)
		{
			this.animator.SetTrigger("AddItem");
		}
	}

	// Token: 0x06002F38 RID: 12088 RVA: 0x0014EBB2 File Offset: 0x0014CFB2
	public void OpenShoppingCart()
	{
		MainUIController.Instance.LoadUIScene("ShoppingCart");
	}

	// Token: 0x06002F39 RID: 12089 RVA: 0x0014EBC3 File Offset: 0x0014CFC3
	public void ClearShoppingCart()
	{
		BWStandalone.ShoppingCart.ClearContents();
	}

	// Token: 0x06002F3A RID: 12090 RVA: 0x0014EBD0 File Offset: 0x0014CFD0
	public void PurchaseShoppingCartContents()
	{
		if (BWStandalone.ShoppingCart.contents.Count == 0)
		{
			return;
		}
		BWStandalone.Overlays.ShowConfirmationDialog("Purchase items?", delegate()
		{
			BWStandalone.ShoppingCart.BuyContents();
		});
	}

	// Token: 0x040027A1 RID: 10145
	public Text countText;

	// Token: 0x040027A2 RID: 10146
	public Animator animator;
}
