using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x0200042D RID: 1069
public class UIPublishCooldown : MonoBehaviour
{
	// Token: 0x06002E1F RID: 11807 RVA: 0x00148ED8 File Offset: 0x001472D8
	private void OnEnable()
	{
		if (this.timerButton != null)
		{
			this.timerButton.onClick.RemoveAllListeners();
			this.timerButton.onClick.AddListener(new UnityAction(this.OnTimerClick));
		}
		if (this.canPublishButton != null)
		{
			this.canPublishButton.onClick.RemoveAllListeners();
			this.canPublishButton.onClick.AddListener(new UnityAction(this.OnCanPublishClick));
		}
		if (this.autoRefresh)
		{
			base.StartCoroutine(this.RunTimerCoroutine());
		}
	}

	// Token: 0x06002E20 RID: 11808 RVA: 0x00148F78 File Offset: 0x00147378
	private void OnDisable()
	{
		base.StopAllCoroutines();
		if (this.timerButton != null)
		{
			this.timerButton.onClick.RemoveAllListeners();
		}
		if (this.canPublishButton != null)
		{
			this.canPublishButton.onClick.RemoveAllListeners();
		}
	}

	// Token: 0x06002E21 RID: 11809 RVA: 0x00148FD0 File Offset: 0x001473D0
	private void OnTimerClick()
	{
		int num;
		int num2;
		int num3;
		int num4;
		this.GetCooldownRemaining(out num, out num2, out num3, out num4);
		string str = string.Empty;
		if (num > 1)
		{
			str = num.ToString() + " hours, " + num2.ToString() + " minutes";
		}
		else if (num == 1)
		{
			str = num.ToString() + " hour, " + num2.ToString() + " minutes";
		}
		else if (num2 > 1)
		{
			str = num2.ToString() + " minutes, " + num3.ToString() + " seconds";
		}
		else if (num2 == 1)
		{
			str = num2.ToString() + " minute, " + num3.ToString() + " seconds";
		}
		else
		{
			str = num3.ToString() + " seconds";
		}
		string messageStr = "Please wait another " + str + " before publishing again!";
		BWStandalone.Overlays.ShowMessage(messageStr);
	}

	// Token: 0x06002E22 RID: 11810 RVA: 0x00149108 File Offset: 0x00147508
	private void GetCooldownRemaining(out int hours, out int minutes, out int seconds, out int priceToSkip)
	{
		if (this.cooldownType == UIPublishCooldown.CooldownType.World)
		{
			BWWorldPublishCooldown.CooldownRemaining(out hours, out minutes, out seconds, out priceToSkip);
		}
		else if (this.cooldownType == UIPublishCooldown.CooldownType.Model)
		{
			BWModelPublishCooldown.CooldownRemaining(out hours, out minutes, out seconds, out priceToSkip);
		}
		else
		{
			hours = (minutes = (seconds = (priceToSkip = 0)));
		}
	}

	// Token: 0x06002E23 RID: 11811 RVA: 0x0014915D File Offset: 0x0014755D
	private void OnCanPublishClick()
	{
		if (this.cooldownType == UIPublishCooldown.CooldownType.World)
		{
			BWStandalone.Overlays.ShowMessage("You can select one of your worlds and publish it now!");
		}
		else if (this.cooldownType == UIPublishCooldown.CooldownType.Model)
		{
			BWStandalone.Overlays.ShowMessage("You can select one of your models and publish it now!");
		}
	}

	// Token: 0x06002E24 RID: 11812 RVA: 0x00149199 File Offset: 0x00147599
	public void StartAutoRefresh()
	{
		this.autoRefresh = true;
		if (base.isActiveAndEnabled)
		{
			base.StartCoroutine(this.RunTimerCoroutine());
		}
	}

	// Token: 0x06002E25 RID: 11813 RVA: 0x001491BA File Offset: 0x001475BA
	public void StopAutoRefresh()
	{
		base.StopAllCoroutines();
		this.autoRefresh = false;
	}

	// Token: 0x06002E26 RID: 11814 RVA: 0x001491CC File Offset: 0x001475CC
	private IEnumerator RunTimerCoroutine()
	{
		for (;;)
		{
			this.Refresh();
			yield return new WaitForSeconds(0.5f);
		}
		yield break;
	}

	// Token: 0x06002E27 RID: 11815 RVA: 0x001491E8 File Offset: 0x001475E8
	public void Refresh()
	{
		bool flag = false;
		if (this.cooldownType == UIPublishCooldown.CooldownType.World)
		{
			flag = ((!string.IsNullOrEmpty(this.worldID)) ? BWWorldPublishCooldown.CanPublish(this.worldID) : BWWorldPublishCooldown.CanPublish());
		}
		else if (this.cooldownType == UIPublishCooldown.CooldownType.Model)
		{
			flag = BWModelPublishCooldown.CanPublish();
		}
		this.canPublishObj.SetActive(flag);
		this.timerObj.SetActive(!flag);
		if (!flag)
		{
			int num;
			int num2;
			int num3;
			int num4;
			this.GetCooldownRemaining(out num, out num2, out num3, out num4);
			if (this.timerText != null)
			{
				string text = string.Empty;
				if (num > 1)
				{
					text = num.ToString() + " hrs " + num2.ToString() + " min";
				}
				else if (num == 1)
				{
					text = num.ToString() + " hr " + num2.ToString() + " min";
				}
				else if (num2 >= 1)
				{
					text = num2.ToString() + " min " + num3.ToString() + " sec";
				}
				else
				{
					text = num3.ToString() + " sec";
				}
				this.timerText.text = text;
			}
			if (this.priceToSkipText != null)
			{
				this.priceToSkipText.text = num4.ToString();
			}
		}
	}

	// Token: 0x04002690 RID: 9872
	public GameObject timerObj;

	// Token: 0x04002691 RID: 9873
	public Text timerText;

	// Token: 0x04002692 RID: 9874
	public GameObject canPublishObj;

	// Token: 0x04002693 RID: 9875
	public Button timerButton;

	// Token: 0x04002694 RID: 9876
	public Button canPublishButton;

	// Token: 0x04002695 RID: 9877
	public Text priceToSkipText;

	// Token: 0x04002696 RID: 9878
	public string worldID;

	// Token: 0x04002697 RID: 9879
	public bool autoRefresh = true;

	// Token: 0x04002698 RID: 9880
	[SerializeField]
	private UIPublishCooldown.CooldownType cooldownType;

	// Token: 0x0200042E RID: 1070
	private enum CooldownType
	{
		// Token: 0x0400269A RID: 9882
		World,
		// Token: 0x0400269B RID: 9883
		Model
	}
}
