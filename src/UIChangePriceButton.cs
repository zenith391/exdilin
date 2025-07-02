using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIChangePriceButton : Button, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler
{
	public bool decrease;

	public float clickWindow = 2f;

	private bool clickDisabled;

	public event UIChangePriceButtonHandler onPriceChange;

	public void AddListener(UIChangePriceButtonHandler handler)
	{
		onPriceChange -= handler;
		onPriceChange += handler;
	}

	public void ClearListeners()
	{
		this.onPriceChange = null;
	}

	private new void OnEnable()
	{
		base.onClick.RemoveAllListeners();
		base.onClick.AddListener(Clicked);
	}

	private new void OnDisable()
	{
		base.onClick.RemoveAllListeners();
	}

	private void Clicked()
	{
		if (!clickDisabled)
		{
			ChangeValue(1);
		}
	}

	public new void OnPointerDown(PointerEventData eventData)
	{
		StartCoroutine(PriceChangeCoroutine());
	}

	public new void OnPointerUp(PointerEventData eventData)
	{
		StopAllCoroutines();
		clickDisabled = false;
	}

	private IEnumerator PriceChangeCoroutine()
	{
		int holdCounter = 0;
		float interval = 0.1f;
		while (true)
		{
			yield return new WaitForSeconds(interval);
			holdCounter++;
			if (holdCounter > 0)
			{
				if (holdCounter > 20)
				{
					ChangeValue(100);
				}
				else if (holdCounter > 16)
				{
					ChangeValue(8);
				}
				else if (holdCounter > 12)
				{
					ChangeValue(4);
				}
				else if (holdCounter > 8)
				{
					ChangeValue(2);
				}
				else if (holdCounter > 4)
				{
					clickDisabled = true;
					ChangeValue(1);
				}
			}
		}
	}

	private void ChangeValue(int amount)
	{
		if (this.onPriceChange != null)
		{
			if (decrease)
			{
				amount = -1 * amount;
			}
			this.onPriceChange(amount);
		}
	}
}
