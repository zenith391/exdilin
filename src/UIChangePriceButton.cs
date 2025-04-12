using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Token: 0x0200043A RID: 1082
public class UIChangePriceButton : Button, IPointerDownHandler, IPointerUpHandler, IEventSystemHandler
{
	// Token: 0x14000021 RID: 33
	// (add) Token: 0x06002E57 RID: 11863 RVA: 0x0014A298 File Offset: 0x00148698
	// (remove) Token: 0x06002E58 RID: 11864 RVA: 0x0014A2D0 File Offset: 0x001486D0
	public event UIChangePriceButtonHandler onPriceChange;

	// Token: 0x06002E59 RID: 11865 RVA: 0x0014A306 File Offset: 0x00148706
	public void AddListener(UIChangePriceButtonHandler handler)
	{
		this.onPriceChange -= handler;
		this.onPriceChange += handler;
	}

	// Token: 0x06002E5A RID: 11866 RVA: 0x0014A316 File Offset: 0x00148716
	public void ClearListeners()
	{
		this.onPriceChange = null;
	}

	// Token: 0x06002E5B RID: 11867 RVA: 0x0014A31F File Offset: 0x0014871F
	private new void OnEnable()
	{
		base.onClick.RemoveAllListeners();
		base.onClick.AddListener(new UnityAction(this.Clicked));
	}

	// Token: 0x06002E5C RID: 11868 RVA: 0x0014A343 File Offset: 0x00148743
	private new void OnDisable()
	{
		base.onClick.RemoveAllListeners();
	}

	// Token: 0x06002E5D RID: 11869 RVA: 0x0014A350 File Offset: 0x00148750
	private void Clicked()
	{
		if (this.clickDisabled)
		{
			return;
		}
		this.ChangeValue(1);
	}

	// Token: 0x06002E5E RID: 11870 RVA: 0x0014A365 File Offset: 0x00148765
	public new void OnPointerDown(PointerEventData eventData)
	{
		base.StartCoroutine(this.PriceChangeCoroutine());
	}

	// Token: 0x06002E5F RID: 11871 RVA: 0x0014A374 File Offset: 0x00148774
	public new void OnPointerUp(PointerEventData eventData)
	{
		base.StopAllCoroutines();
		this.clickDisabled = false;
	}

	// Token: 0x06002E60 RID: 11872 RVA: 0x0014A384 File Offset: 0x00148784
	private IEnumerator PriceChangeCoroutine()
	{
		int holdCounter = 0;
		float interval = 0.1f;
		for (;;)
		{
			yield return new WaitForSeconds(interval);
			holdCounter++;
			if (holdCounter > 0)
			{
				if (holdCounter > 20)
				{
					this.ChangeValue(100);
				}
				else if (holdCounter > 16)
				{
					this.ChangeValue(8);
				}
				else if (holdCounter > 12)
				{
					this.ChangeValue(4);
				}
				else if (holdCounter > 8)
				{
					this.ChangeValue(2);
				}
				else if (holdCounter > 4)
				{
					this.clickDisabled = true;
					this.ChangeValue(1);
				}
			}
		}
		yield break;
	}

	// Token: 0x06002E61 RID: 11873 RVA: 0x0014A39F File Offset: 0x0014879F
	private void ChangeValue(int amount)
	{
		if (this.onPriceChange != null)
		{
			if (this.decrease)
			{
				amount = -1 * amount;
			}
			this.onPriceChange(amount);
		}
	}

	// Token: 0x040026E4 RID: 9956
	public bool decrease;

	// Token: 0x040026E5 RID: 9957
	public float clickWindow = 2f;

	// Token: 0x040026E7 RID: 9959
	private bool clickDisabled;
}
