using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

public class UIPopupLinkToIOS : UIPopup
{
	private enum LinkState
	{
		Unlinked,
		Busy,
		BidirectionalTrue,
		BidirectionalFalse,
		FailedToFindIOS,
		Linked
	}

	public Button cancelButton;

	public Button submitButton;

	public Text bidirectionalLinkText;

	public Text submitButtonText;

	public InputField inputField;

	public GameObject stateUnlinked;

	public GameObject stateLinked;

	public GameObject stateBusy;

	public GameObject stateBidirectionalTrue;

	public GameObject stateBidirectionalFalse;

	public GameObject statedFailedToFindIOS;

	private string iosUsername;

	private LinkState linkState;

	private string enteredUserID;

	private void OnEnable()
	{
		cancelButton.onClick.AddListener(base.Hide);
		submitButton.onClick.AddListener(Submit);
		inputField.onEndEdit.AddListener(OnEditID);
		inputField.onValueChanged.AddListener(OnEditID);
		enteredUserID = string.Empty;
		SetLinkState(LinkState.Unlinked);
	}

	private void OnDisable()
	{
		cancelButton.onClick.RemoveAllListeners();
		submitButton.onClick.RemoveAllListeners();
		inputField.onEndEdit.RemoveAllListeners();
	}

	private void Submit()
	{
		if (linkState == LinkState.BidirectionalFalse)
		{
			SetLinkState(LinkState.Unlinked);
		}
		else if (linkState == LinkState.Unlinked)
		{
			BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("POST", "/api/v1/steam_current_user/link_to_ios_user");
			Debug.Log("linking to " + enteredUserID);
			bWAPIRequestBase.AddParam("ios_user_id", enteredUserID);
			bWAPIRequestBase.onSuccess = delegate(JObject respJson)
			{
				bool property = false;
				iosUsername = "(no username)";
				property = BWJsonHelpers.PropertyIfExists(property, "link_bidirectional", respJson);
				iosUsername = BWJsonHelpers.PropertyIfExists(iosUsername, "ios_username", respJson);
				if (property)
				{
					SetLinkState(LinkState.BidirectionalTrue);
				}
				else
				{
					SetLinkState(LinkState.BidirectionalFalse);
				}
			};
			bWAPIRequestBase.onFailure = delegate
			{
				SetLinkState(LinkState.FailedToFindIOS);
			};
			bWAPIRequestBase.SendOwnerCoroutine(this);
			SetLinkState(LinkState.Busy);
		}
		else if (linkState == LinkState.BidirectionalTrue)
		{
			BWAPIRequestBase bWAPIRequestBase2 = BW.API.CreateRequest("POST", "/api/v1/steam_current_user/account_link");
			bWAPIRequestBase2.onSuccess = delegate
			{
				SetLinkState(LinkState.Linked);
			};
			bWAPIRequestBase2.onFailure = delegate
			{
				SetLinkState(LinkState.BidirectionalTrue);
			};
			bWAPIRequestBase2.SendOwnerCoroutine(this);
			SetLinkState(LinkState.Busy);
		}
		else if (linkState == LinkState.FailedToFindIOS)
		{
			SetLinkState(LinkState.Unlinked);
		}
		else if (linkState == LinkState.Linked)
		{
			Application.Quit();
		}
	}

	private void OnEditID(string ID)
	{
		enteredUserID = ID;
		SetLinkState(linkState);
	}

	private void SetLinkState(LinkState state)
	{
		linkState = state;
		submitButtonText.text = "Submit";
		bool flag = false;
		cancelButton.gameObject.SetActive(value: true);
		if (linkState == LinkState.Unlinked)
		{
			flag = !string.IsNullOrEmpty(enteredUserID);
			stateUnlinked.SetActive(value: true);
			stateLinked.SetActive(value: false);
			stateBusy.SetActive(value: false);
			stateBidirectionalTrue.SetActive(value: false);
			stateBidirectionalFalse.SetActive(value: false);
			statedFailedToFindIOS.SetActive(value: false);
		}
		else if (linkState == LinkState.Linked)
		{
			closeOnBackgroundClick = false;
			submitButtonText.text = "Quit";
			flag = true;
			cancelButton.gameObject.SetActive(value: false);
			stateUnlinked.SetActive(value: false);
			stateLinked.SetActive(value: true);
			stateBusy.SetActive(value: false);
			stateBidirectionalTrue.SetActive(value: false);
			stateBidirectionalFalse.SetActive(value: false);
			statedFailedToFindIOS.SetActive(value: false);
		}
		else if (linkState == LinkState.Busy)
		{
			flag = false;
			stateUnlinked.SetActive(value: false);
			stateLinked.SetActive(value: false);
			stateBusy.SetActive(value: true);
			stateBidirectionalTrue.SetActive(value: false);
			stateBidirectionalFalse.SetActive(value: false);
			statedFailedToFindIOS.SetActive(value: false);
		}
		else if (linkState == LinkState.BidirectionalTrue)
		{
			submitButtonText.text = "Link To This Account";
			string text = "We found your iOS account with the following username:\n\n";
			text = text + iosUsername + "\n\n";
			text += "Would you like to proceed?";
			text += "\n\n\n";
			text += "Please note this action cannot be undone!";
			bidirectionalLinkText.text = text;
			flag = true;
			stateUnlinked.SetActive(value: false);
			stateLinked.SetActive(value: false);
			stateBusy.SetActive(value: false);
			stateBidirectionalTrue.SetActive(value: true);
			stateBidirectionalFalse.SetActive(value: false);
			statedFailedToFindIOS.SetActive(value: false);
		}
		else if (linkState == LinkState.BidirectionalFalse)
		{
			submitButtonText.text = "Try Again";
			flag = true;
			stateUnlinked.SetActive(value: false);
			stateLinked.SetActive(value: false);
			stateBusy.SetActive(value: false);
			stateBidirectionalTrue.SetActive(value: false);
			stateBidirectionalFalse.SetActive(value: true);
			statedFailedToFindIOS.SetActive(value: false);
		}
		else if (linkState == LinkState.FailedToFindIOS)
		{
			submitButtonText.text = "Try Again";
			flag = true;
			stateUnlinked.SetActive(value: false);
			stateLinked.SetActive(value: false);
			stateBusy.SetActive(value: false);
			stateBidirectionalTrue.SetActive(value: false);
			stateBidirectionalFalse.SetActive(value: false);
			statedFailedToFindIOS.SetActive(value: true);
		}
		CanvasGroup component = submitButton.gameObject.GetComponent<CanvasGroup>();
		if (component != null)
		{
			component.alpha = ((!flag) ? 0.5f : 1f);
			component.interactable = flag;
			component.blocksRaycasts = flag;
		}
	}
}
