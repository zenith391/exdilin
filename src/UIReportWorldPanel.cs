using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIReportWorldPanel : MonoBehaviour
{
	public Button reportButton;

	public Button noActionButton;

	public Toggle disclosureToggle;

	public Toggle hateSpeechToggle;

	public Toggle sexToggle;

	public Toggle drugsToggle;

	public Toggle violenceToggle;

	private HashSet<Toggle> allToggles;

	private string worldID;

	private WorldReportReasonEnum reportReason;

	private bool ignoreToggles;

	public void Setup(string worldID)
	{
		this.worldID = worldID;
		reportReason = WorldReportReasonEnum.WorldModerationApproval;
		disclosureToggle.onValueChanged.AddListener(ReasonToggleHandler);
		hateSpeechToggle.onValueChanged.AddListener(ReasonToggleHandler);
		sexToggle.onValueChanged.AddListener(ReasonToggleHandler);
		drugsToggle.onValueChanged.AddListener(ReasonToggleHandler);
		violenceToggle.onValueChanged.AddListener(ReasonToggleHandler);
		reportButton.onClick.AddListener(ReportWorld);
	}

	public void ReasonToggleHandler(bool status)
	{
		int num = 0;
		if (disclosureToggle.isOn)
		{
			num |= 1;
		}
		if (hateSpeechToggle.isOn)
		{
			num |= 2;
		}
		if (sexToggle.isOn)
		{
			num |= 4;
		}
		if (drugsToggle.isOn)
		{
			num |= 8;
		}
		if (violenceToggle.isOn)
		{
			num |= 0x10;
		}
		reportReason = (WorldReportReasonEnum)num;
		if (reportReason == WorldReportReasonEnum.WorldModerationApproval)
		{
			reportButton.gameObject.SetActive(value: false);
			noActionButton.gameObject.SetActive(value: true);
		}
		else
		{
			reportButton.gameObject.SetActive(value: true);
			noActionButton.gameObject.SetActive(value: false);
		}
	}

	private void ReportWorld()
	{
		BWUserDataManager.Instance.ReportWorld(worldID);
		string path = $"/api/v1/worlds/{worldID}/report";
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("POST", path);
		BWAPIRequestBase bWAPIRequestBase2 = bWAPIRequestBase;
		string key = "reason";
		int num = (int)reportReason;
		bWAPIRequestBase2.AddParam(key, num.ToString());
		bWAPIRequestBase.onSuccess = delegate
		{
			BWStandalone.Overlays.SetUIBusy(busy: false);
			BWStandalone.Overlays.ShowMessage(BWMenuTextEnum.WorldReportSuccessMessage);
		};
		bWAPIRequestBase.onFailure = delegate
		{
			BWStandalone.Overlays.SetUIBusy(busy: false);
		};
		BWStandalone.Overlays.SetUIBusy(busy: true);
		bWAPIRequestBase.SendOwnerCoroutine(this);
	}
}
