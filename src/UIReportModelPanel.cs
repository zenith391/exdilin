using System;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x02000455 RID: 1109
public class UIReportModelPanel : MonoBehaviour
{
	// Token: 0x06002F19 RID: 12057 RVA: 0x0014DE1C File Offset: 0x0014C21C
	public void Setup(string modelID)
	{
		this.modelID = modelID;
		this.reportReason = string.Empty;
		this.disclosureToggle.onValueChanged.AddListener(new UnityAction<bool>(this.ReasonToggleHandler));
		this.hateSpeechToggle.onValueChanged.AddListener(new UnityAction<bool>(this.ReasonToggleHandler));
		this.sexToggle.onValueChanged.AddListener(new UnityAction<bool>(this.ReasonToggleHandler));
		this.drugsToggle.onValueChanged.AddListener(new UnityAction<bool>(this.ReasonToggleHandler));
		this.violenceToggle.onValueChanged.AddListener(new UnityAction<bool>(this.ReasonToggleHandler));
		this.reportButton.onClick.AddListener(new UnityAction(this.ReportModel));
	}

	// Token: 0x06002F1A RID: 12058 RVA: 0x0014DEE4 File Offset: 0x0014C2E4
	public void ReasonToggleHandler(bool status)
	{
		if (this.disclosureToggle.isOn)
		{
			this.reportReason = "Personal information";
		}
		else if (this.hateSpeechToggle.isOn)
		{
			this.reportReason = "Hate Speech or Harassment";
		}
		else if (this.sexToggle.isOn)
		{
			this.reportReason = "Nudity";
		}
		else if (this.drugsToggle.isOn)
		{
			this.reportReason = "Drugs or Alcohol";
		}
		else if (this.violenceToggle.isOn)
		{
			this.reportReason = "Violence";
		}
		if (string.IsNullOrEmpty(this.reportReason))
		{
			this.reportButton.gameObject.SetActive(false);
			this.noActionButton.gameObject.SetActive(true);
		}
		else
		{
			this.reportButton.gameObject.SetActive(true);
			this.noActionButton.gameObject.SetActive(false);
		}
	}

	// Token: 0x06002F1B RID: 12059 RVA: 0x0014DFE8 File Offset: 0x0014C3E8
	private void ReportModel()
	{
		BWUserDataManager.Instance.ReportModel(this.modelID);
		string path = string.Format("/api/v1/u2u_models/{0}/abuse", this.modelID);
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("POST", path);
		bwapirequestBase.AddParam("description", this.reportReason);
		bwapirequestBase.onSuccess = delegate(JObject resp)
		{
			BWStandalone.Overlays.SetUIBusy(false);
			BWStandalone.Overlays.ShowMessage(BWMenuTextEnum.ModelReportSucessMessage);
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWStandalone.Overlays.SetUIBusy(false);
		};
		BWStandalone.Overlays.SetUIBusy(true);
		bwapirequestBase.SendOwnerCoroutine(this);
	}

	// Token: 0x04002781 RID: 10113
	public Button reportButton;

	// Token: 0x04002782 RID: 10114
	public Button noActionButton;

	// Token: 0x04002783 RID: 10115
	public Toggle disclosureToggle;

	// Token: 0x04002784 RID: 10116
	public Toggle hateSpeechToggle;

	// Token: 0x04002785 RID: 10117
	public Toggle sexToggle;

	// Token: 0x04002786 RID: 10118
	public Toggle drugsToggle;

	// Token: 0x04002787 RID: 10119
	public Toggle violenceToggle;

	// Token: 0x04002788 RID: 10120
	private string modelID;

	// Token: 0x04002789 RID: 10121
	private string reportReason;
}
