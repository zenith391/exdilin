using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Exdilin.UI;

// Token: 0x0200043D RID: 1085
public class MenuBar : MonoBehaviour
{
	private GameObject templateMenuBarButton;
	public MenuBarButton currentCustomMenuBarButton;

	// Token: 0x06002E7F RID: 11903 RVA: 0x0014B2C3 File Offset: 0x001496C3
	public void Init(UIDataManager dataManager, ImageManager imageManager)
	{
		this.animator = base.GetComponent<Animator>();
		this.canvasGroup = base.GetComponent<CanvasGroup>();
		this.dataManager = dataManager;
		this.imageManager = imageManager;
	}

	public void SetTemplateMenuBarButton(GameObject go) {
		templateMenuBarButton = go;
	}

	public GameObject GetTemplateMenuBarButton()
    {
        UISceneElement sceneElement = base.GetComponentInChildren<UISceneElement>();
        return sceneElement.gameObject;
    }

	// Token: 0x06002E80 RID: 11904 RVA: 0x0014B2EC File Offset: 0x001496EC
	public void OnSceneLoad()
    {
        UISceneElement[] componentsInChildren = base.GetComponentsInChildren<UISceneElement>();
		foreach (UISceneElement uisceneElement in componentsInChildren)
		{
			uisceneElement.Init();
			uisceneElement.LoadContent(this.dataManager, this.imageManager);
		}
		this.PutCoinsDiplayInOverlay(false);
	}

	public void DeselectCustomButton() {
		if (currentCustomMenuBarButton != null) {
			currentCustomMenuBarButton.OnClean(MainUIController.Instance.loadedSceneController.GetComponent<Canvas>());
			currentCustomMenuBarButton = null;
		}
	}

	void OnGUI() {
		if (MainUIController.active) {
			UIMenuBarButton[] buttons = GetComponentsInChildren<UIMenuBarButton>();
			if (buttons.Length < 3) return;
			UIMenuBarButton last = buttons[buttons.Length - 3];
			if (last == null) return;
			GameObject lastGo = last.gameObject;
			RectTransform lastTransform = lastGo.GetComponent<RectTransform>();
			//Rect rect = new Rect(position.x, 0, 90, 24);
			Rect rect = lastTransform.rect;
			rect.x = lastTransform.position.x;
			rect.y = 0;
			Dictionary<string, MenuBarButton> dict = UIRegistry.GetMenuBarButtons();
			Canvas canvas = MainUIController.Instance.loadedSceneController.GetComponent<Canvas>();
			foreach (string key in dict.Keys) {
				MenuBarButton button = dict[key];
				rect.x += rect.width;
				if (GUI.Button(rect, button.Title)) {
					foreach (UIMenuBarButton uiButton in buttons) {
						uiButton.Deselect();
					}
					DeselectCustomButton();
					GameObject obj = canvas.gameObject;
					obj.transform.DetachChildren();
					foreach (Transform child in obj.transform) {
						child.parent = null;
					}
					button.OnInit(canvas);
					currentCustomMenuBarButton = button;
				}
			}
			if (currentCustomMenuBarButton != null) {
				if (currentCustomMenuBarButton.usesIMGUI()) {
					currentCustomMenuBarButton.OnRender();
				}
			}
		}
	}

	// Token: 0x06002E81 RID: 11905 RVA: 0x0014B339 File Offset: 0x00149739
	public void SetInteractable(bool interactable)
	{
		if (interactable)
		{
			this.animator.SetTrigger("Enable");
		}
		else
		{
			this.animator.SetTrigger("Disable");
		}
	}

	// Token: 0x06002E82 RID: 11906 RVA: 0x0014B368 File Offset: 0x00149768
	public void SelectMenuButton(MenuBarButtonEnum buttonType)
	{
		foreach (UIMenuBarButton uimenuBarButton in this.menuButtons)
		{
			if (uimenuBarButton.menuBarButton == buttonType)
			{
				uimenuBarButton.Select();
				break;
			}
		}
	}

	// Token: 0x06002E83 RID: 11907 RVA: 0x0014B3D4 File Offset: 0x001497D4
	public void DeselectMenuButtons()
	{
		foreach (UIMenuBarButton uimenuBarButton in base.GetComponentsInChildren<UIMenuBarButton>())
		{
			uimenuBarButton.Deselect();
		}
		DeselectCustomButton();
	}

	// Token: 0x06002E84 RID: 11908 RVA: 0x0014B406 File Offset: 0x00149806
	public void PutCoinsDiplayInOverlay(bool inOverlay)
	{
		this.coinsDisplayForwardObj.SetActive(inOverlay);
	}

	// Token: 0x06002E85 RID: 11909 RVA: 0x0014B414 File Offset: 0x00149814
	public void ButtonTapped_Back()
	{
		if (MainUIController.active)
		{
			MainUIController.Instance.NavigateBack();
		}
	}

	// Token: 0x06002E86 RID: 11910 RVA: 0x0014B42A File Offset: 0x0014982A
	public void ButtonTapped_Forward()
	{
		if (MainUIController.active)
		{
			MainUIController.Instance.NavigateForward();
		}
	}

	// Token: 0x06002E87 RID: 11911 RVA: 0x0014B440 File Offset: 0x00149840
	public void DoSearch(string searchStr)
	{
		if (MainUIController.active)
		{
			MainUIController.Instance.Search(searchStr);
		}
	}

	// Token: 0x06002E88 RID: 11912 RVA: 0x0014B457 File Offset: 0x00149857
	public void Show(bool show)
	{
		this.canvasGroup.alpha = ((!show) ? 0f : 1f);
	}

	// Token: 0x06002E89 RID: 11913 RVA: 0x0014B479 File Offset: 0x00149879
	public void ShowBackButton()
	{
		this.backButton.interactable = true;
	}

	// Token: 0x06002E8A RID: 11914 RVA: 0x0014B487 File Offset: 0x00149887
	public void HideBackButton()
	{
		this.backButton.interactable = false;
	}

	// Token: 0x06002E8B RID: 11915 RVA: 0x0014B495 File Offset: 0x00149895
	public void ShowForwardButton()
	{
		this.forwardButton.interactable = true;
	}

	// Token: 0x06002E8C RID: 11916 RVA: 0x0014B4A3 File Offset: 0x001498A3
	public void HideForwardButton()
	{
		this.forwardButton.interactable = false;
	}

	// Token: 0x04002701 RID: 9985
	public Button backButton;

	// Token: 0x04002702 RID: 9986
	public Button forwardButton;

	// Token: 0x04002703 RID: 9987
	public RectTransform menuButtonRoot;

	// Token: 0x04002704 RID: 9988
	public List<UIMenuBarButton> menuButtons;

	// Token: 0x04002705 RID: 9989
	public Button settingsMenuButton;

	// Token: 0x04002706 RID: 9990
	public GameObject coinsDisplayForwardObj;

	// Token: 0x04002707 RID: 9991
	public RectTransform coinsAnimTarget;

	// Token: 0x04002708 RID: 9992
	private Animator animator;

	// Token: 0x04002709 RID: 9993
	private CanvasGroup canvasGroup;

	// Token: 0x0400270A RID: 9994
	private UIDataManager dataManager;

	// Token: 0x0400270B RID: 9995
	private ImageManager imageManager;

	// Token: 0x0400270C RID: 9996
	private Canvas coinsDisplaySortOverride;
}
