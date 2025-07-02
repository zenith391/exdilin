using UnityEngine;
using UnityEngine.UI;

public class UIPopupPendingPayouts : UIPopup
{
	public Button collectButton;

	public RectTransform coinAnimSource;

	private void OnEnable()
	{
		collectButton.onClick.AddListener(Collect);
		BWPendingPayouts.AddPendingPayoutsCollectedListener(OnCollectComplete);
	}

	private void OnDisable()
	{
		collectButton.onClick.RemoveListener(Collect);
		BWPendingPayouts.RemovePendingPayoutsCollectedListener(OnCollectComplete);
	}

	private void Collect()
	{
		BWPendingPayouts.Collect();
		collectButton.interactable = false;
	}

	private void OnCollectComplete(bool success)
	{
		if (success)
		{
			BWStandalone.Overlays.DoCoinAwardAnimation(coinAnimSource.position, base.Hide);
		}
		else
		{
			Hide();
		}
	}
}
