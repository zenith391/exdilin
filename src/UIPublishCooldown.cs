using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIPublishCooldown : MonoBehaviour
{
	private enum CooldownType
	{
		World,
		Model
	}

	public GameObject timerObj;

	public Text timerText;

	public GameObject canPublishObj;

	public Button timerButton;

	public Button canPublishButton;

	public Text priceToSkipText;

	public string worldID;

	public bool autoRefresh = true;

	[SerializeField]
	private CooldownType cooldownType;

	private void OnEnable()
	{
		if (timerButton != null)
		{
			timerButton.onClick.RemoveAllListeners();
			timerButton.onClick.AddListener(OnTimerClick);
		}
		if (canPublishButton != null)
		{
			canPublishButton.onClick.RemoveAllListeners();
			canPublishButton.onClick.AddListener(OnCanPublishClick);
		}
		if (autoRefresh)
		{
			StartCoroutine(RunTimerCoroutine());
		}
	}

	private void OnDisable()
	{
		StopAllCoroutines();
		if (timerButton != null)
		{
			timerButton.onClick.RemoveAllListeners();
		}
		if (canPublishButton != null)
		{
			canPublishButton.onClick.RemoveAllListeners();
		}
	}

	private void OnTimerClick()
	{
		GetCooldownRemaining(out var hours, out var minutes, out var seconds, out var _);
		string empty = string.Empty;
		empty = ((hours > 1) ? (hours + " hours, " + minutes + " minutes") : ((hours == 1) ? (hours + " hour, " + minutes + " minutes") : ((minutes > 1) ? (minutes + " minutes, " + seconds + " seconds") : ((minutes != 1) ? (seconds + " seconds") : (minutes + " minute, " + seconds + " seconds")))));
		string messageStr = "Please wait another " + empty + " before publishing again!";
		BWStandalone.Overlays.ShowMessage(messageStr);
	}

	private void GetCooldownRemaining(out int hours, out int minutes, out int seconds, out int priceToSkip)
	{
		if (cooldownType == CooldownType.World)
		{
			BWWorldPublishCooldown.CooldownRemaining(out hours, out minutes, out seconds, out priceToSkip);
		}
		else if (cooldownType == CooldownType.Model)
		{
			BWModelPublishCooldown.CooldownRemaining(out hours, out minutes, out seconds, out priceToSkip);
		}
		else
		{
			hours = (minutes = (seconds = (priceToSkip = 0)));
		}
	}

	private void OnCanPublishClick()
	{
		if (cooldownType == CooldownType.World)
		{
			BWStandalone.Overlays.ShowMessage("You can select one of your worlds and publish it now!");
		}
		else if (cooldownType == CooldownType.Model)
		{
			BWStandalone.Overlays.ShowMessage("You can select one of your models and publish it now!");
		}
	}

	public void StartAutoRefresh()
	{
		autoRefresh = true;
		if (base.isActiveAndEnabled)
		{
			StartCoroutine(RunTimerCoroutine());
		}
	}

	public void StopAutoRefresh()
	{
		StopAllCoroutines();
		autoRefresh = false;
	}

	private IEnumerator RunTimerCoroutine()
	{
		while (true)
		{
			Refresh();
			yield return new WaitForSeconds(0.5f);
		}
	}

	public void Refresh()
	{
		bool flag = false;
		if (cooldownType == CooldownType.World)
		{
			flag = ((!string.IsNullOrEmpty(worldID)) ? BWWorldPublishCooldown.CanPublish(worldID) : BWWorldPublishCooldown.CanPublish());
		}
		else if (cooldownType == CooldownType.Model)
		{
			flag = BWModelPublishCooldown.CanPublish();
		}
		canPublishObj.SetActive(flag);
		timerObj.SetActive(!flag);
		if (!flag)
		{
			GetCooldownRemaining(out var hours, out var minutes, out var seconds, out var priceToSkip);
			if (timerText != null)
			{
				string empty = string.Empty;
				empty = ((hours > 1) ? (hours + " hrs " + minutes + " min") : ((hours == 1) ? (hours + " hr " + minutes + " min") : ((minutes < 1) ? (seconds + " sec") : (minutes + " min " + seconds + " sec"))));
				timerText.text = empty;
			}
			if (priceToSkipText != null)
			{
				priceToSkipText.text = priceToSkip.ToString();
			}
		}
	}
}
