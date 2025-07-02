using UnityEngine;
using UnityEngine.UI;

public class UIShoppingCartContentIndicator : MonoBehaviour
{
	public Text countText;

	public Animator animator;

	private void OnEnable()
	{
		if (countText != null)
		{
			countText.text = BWStandalone.ShoppingCart.itemCount.ToString();
		}
		BWShoppingCartEvents.AddListeners(UpdateCount, UpdateCountForItem);
	}

	private void OnDisable()
	{
		BWShoppingCartEvents.RemoveListeners(UpdateCount, UpdateCountForItem);
	}

	private void UpdateCountForItem(BWShoppingCart cart, int index)
	{
		UpdateCount(cart);
	}

	private void UpdateCount(BWShoppingCart cart)
	{
		if (countText != null)
		{
			countText.text = cart.itemCount.ToString();
			countText.enabled = cart.itemCount > 0;
		}
		if (animator != null)
		{
			animator.SetTrigger("AddItem");
		}
	}

	public void OpenShoppingCart()
	{
		MainUIController.Instance.LoadUIScene("ShoppingCart");
	}

	public void ClearShoppingCart()
	{
		BWStandalone.ShoppingCart.ClearContents();
	}

	public void PurchaseShoppingCartContents()
	{
		if (BWStandalone.ShoppingCart.contents.Count != 0)
		{
			BWStandalone.Overlays.ShowConfirmationDialog("Purchase items?", delegate
			{
				BWStandalone.ShoppingCart.BuyContents();
			});
		}
	}
}
