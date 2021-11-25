using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x02000457 RID: 1111
public class UIReportWorldPanel : MonoBehaviour
{
	// Token: 0x06002F1F RID: 12063 RVA: 0x0014E0C0 File Offset: 0x0014C4C0
	public void Setup(string worldID)
	{
		this.worldID = worldID;
		this.reportReason = WorldReportReasonEnum.WorldModerationApproval;
		this.disclosureToggle.onValueChanged.AddListener(new UnityAction<bool>(this.ReasonToggleHandler));
		this.hateSpeechToggle.onValueChanged.AddListener(new UnityAction<bool>(this.ReasonToggleHandler));
		this.sexToggle.onValueChanged.AddListener(new UnityAction<bool>(this.ReasonToggleHandler));
		this.drugsToggle.onValueChanged.AddListener(new UnityAction<bool>(this.ReasonToggleHandler));
		this.violenceToggle.onValueChanged.AddListener(new UnityAction<bool>(this.ReasonToggleHandler));
		this.reportButton.onClick.AddListener(new UnityAction(this.ReportWorld));
	}

	// Token: 0x06002F20 RID: 12064 RVA: 0x0014E184 File Offset: 0x0014C584
	public void ReasonToggleHandler(bool status)
	{
		int num = 0;
		if (this.disclosureToggle.isOn)
		{
			num |= 1;
		}
		if (this.hateSpeechToggle.isOn)
		{
			num |= 2;
		}
		if (this.sexToggle.isOn)
		{
			num |= 4;
		}
		if (this.drugsToggle.isOn)
		{
			num |= 8;
		}
		if (this.violenceToggle.isOn)
		{
			num |= 16;
		}
		this.reportReason = (WorldReportReasonEnum)num;
		if (this.reportReason == WorldReportReasonEnum.WorldModerationApproval)
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

	// Token: 0x06002F21 RID: 12065 RVA: 0x0014E254 File Offset: 0x0014C654
	private void ReportWorld()
	{
		BWUserDataManager.Instance.ReportWorld(this.worldID);
		string path = string.Format("/api/v1/worlds/{0}/report", this.worldID);
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("POST", path);
		BWAPIRequestBase bwapirequestBase2 = bwapirequestBase;
		string key = "reason";
		int num = (int)this.reportReason;
		bwapirequestBase2.AddParam(key, num.ToString());
		bwapirequestBase.onSuccess = delegate(JObject resp)
		{
			BWStandalone.Overlays.SetUIBusy(false);
			BWStandalone.Overlays.ShowMessage(BWMenuTextEnum.WorldReportSuccessMessage);
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWStandalone.Overlays.SetUIBusy(false);
		};
		BWStandalone.Overlays.SetUIBusy(true);
		bwapirequestBase.SendOwnerCoroutine(this);
	}

	// Token: 0x04002793 RID: 10131
	public Button reportButton;

	// Token: 0x04002794 RID: 10132
	public Button noActionButton;

	// Token: 0x04002795 RID: 10133
	public Toggle disclosureToggle;

	// Token: 0x04002796 RID: 10134
	public Toggle hateSpeechToggle;

	// Token: 0x04002797 RID: 10135
	public Toggle sexToggle;

	// Token: 0x04002798 RID: 10136
	public Toggle drugsToggle;

	// Token: 0x04002799 RID: 10137
	public Toggle violenceToggle;

	// Token: 0x0400279A RID: 10138
	private HashSet<Toggle> allToggles;

	// Token: 0x0400279B RID: 10139
	private string worldID;

	// Token: 0x0400279C RID: 10140
	private WorldReportReasonEnum reportReason;

	// Token: 0x0400279D RID: 10141
	private bool ignoreToggles;
}
