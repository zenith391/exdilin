using UnityEngine;
using UnityEngine.UI;

public class UIReportModelPanel : MonoBehaviour
{
	public Button reportButton;

	public Button noActionButton;

	public Toggle disclosureToggle;

	public Toggle hateSpeechToggle;

	public Toggle sexToggle;

	public Toggle drugsToggle;

	public Toggle violenceToggle;

	private string modelID;

	private string reportReason;

	public void Setup(string modelID)
	{
		this.modelID = modelID;
		reportReason = string.Empty;
		disclosureToggle.onValueChanged.AddListener(ReasonToggleHandler);
		hateSpeechToggle.onValueChanged.AddListener(ReasonToggleHandler);
		sexToggle.onValueChanged.AddListener(ReasonToggleHandler);
		drugsToggle.onValueChanged.AddListener(ReasonToggleHandler);
		violenceToggle.onValueChanged.AddListener(ReasonToggleHandler);
		reportButton.onClick.AddListener(ReportModel);
	}

	public void ReasonToggleHandler(bool status)
	{
		if (disclosureToggle.isOn)
		{
			reportReason = "Personal information";
		}
		else if (hateSpeechToggle.isOn)
		{
			reportReason = "Hate Speech or Harassment";
		}
		else if (sexToggle.isOn)
		{
			reportReason = "Nudity";
		}
		else if (drugsToggle.isOn)
		{
			reportReason = "Drugs or Alcohol";
		}
		else if (violenceToggle.isOn)
		{
			reportReason = "Violence";
		}
		if (string.IsNullOrEmpty(reportReason))
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

	private void ReportModel()
	{
		BWUserDataManager.Instance.ReportModel(modelID);
		string path = $"/api/v1/u2u_models/{modelID}/abuse";
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("POST", path);
		bWAPIRequestBase.AddParam("description", reportReason);
		bWAPIRequestBase.onSuccess = delegate
		{
			BWStandalone.Overlays.SetUIBusy(busy: false);
			BWStandalone.Overlays.ShowMessage(BWMenuTextEnum.ModelReportSucessMessage);
		};
		bWAPIRequestBase.onFailure = delegate
		{
			BWStandalone.Overlays.SetUIBusy(busy: false);
		};
		BWStandalone.Overlays.SetUIBusy(busy: true);
		bWAPIRequestBase.SendOwnerCoroutine(this);
	}
}
