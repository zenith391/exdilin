using System;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x02000451 RID: 1105
public class UIPopupLinkToIOS : UIPopup
{
	// Token: 0x06002F02 RID: 12034 RVA: 0x0014D59C File Offset: 0x0014B99C
	private void OnEnable()
	{
		this.cancelButton.onClick.AddListener(new UnityAction(base.Hide));
		this.submitButton.onClick.AddListener(new UnityAction(this.Submit));
		this.inputField.onEndEdit.AddListener(new UnityAction<string>(this.OnEditID));
		this.inputField.onValueChanged.AddListener(new UnityAction<string>(this.OnEditID));
		this.enteredUserID = string.Empty;
		this.SetLinkState(UIPopupLinkToIOS.LinkState.Unlinked);
	}

	// Token: 0x06002F03 RID: 12035 RVA: 0x0014D62B File Offset: 0x0014BA2B
	private void OnDisable()
	{
		this.cancelButton.onClick.RemoveAllListeners();
		this.submitButton.onClick.RemoveAllListeners();
		this.inputField.onEndEdit.RemoveAllListeners();
	}

	// Token: 0x06002F04 RID: 12036 RVA: 0x0014D660 File Offset: 0x0014BA60
	private void Submit()
	{
		if (this.linkState == UIPopupLinkToIOS.LinkState.BidirectionalFalse)
		{
			this.SetLinkState(UIPopupLinkToIOS.LinkState.Unlinked);
		}
		else if (this.linkState == UIPopupLinkToIOS.LinkState.Unlinked)
		{
			BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("POST", "/api/v1/steam_current_user/link_to_ios_user");
			Debug.Log("linking to " + this.enteredUserID);
			bwapirequestBase.AddParam("ios_user_id", this.enteredUserID);
			bwapirequestBase.onSuccess = delegate(JObject respJson)
			{
				bool flag = false;
				this.iosUsername = "(no username)";
				flag = BWJsonHelpers.PropertyIfExists(flag, "link_bidirectional", respJson);
				this.iosUsername = BWJsonHelpers.PropertyIfExists(this.iosUsername, "ios_username", respJson);
				if (flag)
				{
					this.SetLinkState(UIPopupLinkToIOS.LinkState.BidirectionalTrue);
				}
				else
				{
					this.SetLinkState(UIPopupLinkToIOS.LinkState.BidirectionalFalse);
				}
			};
			bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
			{
				this.SetLinkState(UIPopupLinkToIOS.LinkState.FailedToFindIOS);
			};
			bwapirequestBase.SendOwnerCoroutine(this);
			this.SetLinkState(UIPopupLinkToIOS.LinkState.Busy);
		}
		else if (this.linkState == UIPopupLinkToIOS.LinkState.BidirectionalTrue)
		{
			BWAPIRequestBase bwapirequestBase2 = BW.API.CreateRequest("POST", "/api/v1/steam_current_user/account_link");
			bwapirequestBase2.onSuccess = delegate(JObject respJson)
			{
				this.SetLinkState(UIPopupLinkToIOS.LinkState.Linked);
			};
			bwapirequestBase2.onFailure = delegate(BWAPIRequestError error)
			{
				this.SetLinkState(UIPopupLinkToIOS.LinkState.BidirectionalTrue);
			};
			bwapirequestBase2.SendOwnerCoroutine(this);
			this.SetLinkState(UIPopupLinkToIOS.LinkState.Busy);
		}
		else if (this.linkState == UIPopupLinkToIOS.LinkState.FailedToFindIOS)
		{
			this.SetLinkState(UIPopupLinkToIOS.LinkState.Unlinked);
		}
		else if (this.linkState == UIPopupLinkToIOS.LinkState.Linked)
		{
			Application.Quit();
		}
	}

	// Token: 0x06002F05 RID: 12037 RVA: 0x0014D783 File Offset: 0x0014BB83
	private void OnEditID(string ID)
	{
		this.enteredUserID = ID;
		this.SetLinkState(this.linkState);
	}

	// Token: 0x06002F06 RID: 12038 RVA: 0x0014D798 File Offset: 0x0014BB98
	private void SetLinkState(UIPopupLinkToIOS.LinkState state)
	{
		this.linkState = state;
		this.submitButtonText.text = "Submit";
		bool flag = false;
		this.cancelButton.gameObject.SetActive(true);
		if (this.linkState == UIPopupLinkToIOS.LinkState.Unlinked)
		{
			flag = !string.IsNullOrEmpty(this.enteredUserID);
			this.stateUnlinked.SetActive(true);
			this.stateLinked.SetActive(false);
			this.stateBusy.SetActive(false);
			this.stateBidirectionalTrue.SetActive(false);
			this.stateBidirectionalFalse.SetActive(false);
			this.statedFailedToFindIOS.SetActive(false);
		}
		else if (this.linkState == UIPopupLinkToIOS.LinkState.Linked)
		{
			this.closeOnBackgroundClick = false;
			this.submitButtonText.text = "Quit";
			flag = true;
			this.cancelButton.gameObject.SetActive(false);
			this.stateUnlinked.SetActive(false);
			this.stateLinked.SetActive(true);
			this.stateBusy.SetActive(false);
			this.stateBidirectionalTrue.SetActive(false);
			this.stateBidirectionalFalse.SetActive(false);
			this.statedFailedToFindIOS.SetActive(false);
		}
		else if (this.linkState == UIPopupLinkToIOS.LinkState.Busy)
		{
			flag = false;
			this.stateUnlinked.SetActive(false);
			this.stateLinked.SetActive(false);
			this.stateBusy.SetActive(true);
			this.stateBidirectionalTrue.SetActive(false);
			this.stateBidirectionalFalse.SetActive(false);
			this.statedFailedToFindIOS.SetActive(false);
		}
		else if (this.linkState == UIPopupLinkToIOS.LinkState.BidirectionalTrue)
		{
			this.submitButtonText.text = "Link To This Account";
			string text = "We found your iOS account with the following username:\n\n";
			text = text + this.iosUsername + "\n\n";
			text += "Would you like to proceed?";
			text += "\n\n\n";
			text += "Please note this action cannot be undone!";
			this.bidirectionalLinkText.text = text;
			flag = true;
			this.stateUnlinked.SetActive(false);
			this.stateLinked.SetActive(false);
			this.stateBusy.SetActive(false);
			this.stateBidirectionalTrue.SetActive(true);
			this.stateBidirectionalFalse.SetActive(false);
			this.statedFailedToFindIOS.SetActive(false);
		}
		else if (this.linkState == UIPopupLinkToIOS.LinkState.BidirectionalFalse)
		{
			this.submitButtonText.text = "Try Again";
			flag = true;
			this.stateUnlinked.SetActive(false);
			this.stateLinked.SetActive(false);
			this.stateBusy.SetActive(false);
			this.stateBidirectionalTrue.SetActive(false);
			this.stateBidirectionalFalse.SetActive(true);
			this.statedFailedToFindIOS.SetActive(false);
		}
		else if (this.linkState == UIPopupLinkToIOS.LinkState.FailedToFindIOS)
		{
			this.submitButtonText.text = "Try Again";
			flag = true;
			this.stateUnlinked.SetActive(false);
			this.stateLinked.SetActive(false);
			this.stateBusy.SetActive(false);
			this.stateBidirectionalTrue.SetActive(false);
			this.stateBidirectionalFalse.SetActive(false);
			this.statedFailedToFindIOS.SetActive(true);
		}
		CanvasGroup component = this.submitButton.gameObject.GetComponent<CanvasGroup>();
		if (component != null)
		{
			component.alpha = ((!flag) ? 0.5f : 1f);
			component.interactable = flag;
			component.blocksRaycasts = flag;
		}
	}

	// Token: 0x04002761 RID: 10081
	public Button cancelButton;

	// Token: 0x04002762 RID: 10082
	public Button submitButton;

	// Token: 0x04002763 RID: 10083
	public Text bidirectionalLinkText;

	// Token: 0x04002764 RID: 10084
	public Text submitButtonText;

	// Token: 0x04002765 RID: 10085
	public InputField inputField;

	// Token: 0x04002766 RID: 10086
	public GameObject stateUnlinked;

	// Token: 0x04002767 RID: 10087
	public GameObject stateLinked;

	// Token: 0x04002768 RID: 10088
	public GameObject stateBusy;

	// Token: 0x04002769 RID: 10089
	public GameObject stateBidirectionalTrue;

	// Token: 0x0400276A RID: 10090
	public GameObject stateBidirectionalFalse;

	// Token: 0x0400276B RID: 10091
	public GameObject statedFailedToFindIOS;

	// Token: 0x0400276C RID: 10092
	private string iosUsername;

	// Token: 0x0400276D RID: 10093
	private UIPopupLinkToIOS.LinkState linkState;

	// Token: 0x0400276E RID: 10094
	private string enteredUserID;

	// Token: 0x02000452 RID: 1106
	private enum LinkState
	{
		// Token: 0x04002770 RID: 10096
		Unlinked,
		// Token: 0x04002771 RID: 10097
		Busy,
		// Token: 0x04002772 RID: 10098
		BidirectionalTrue,
		// Token: 0x04002773 RID: 10099
		BidirectionalFalse,
		// Token: 0x04002774 RID: 10100
		FailedToFindIOS,
		// Token: 0x04002775 RID: 10101
		Linked
	}
}
