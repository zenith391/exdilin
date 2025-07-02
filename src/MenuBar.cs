using System;
using System.Collections.Generic;
using Exdilin.UI;
using UnityEngine;
using UnityEngine.UI;

public class MenuBar : MonoBehaviour
{
	private GameObject templateMenuBarButton;

	public MenuBarButton currentCustomMenuBarButton;

	public Button backButton;

	public Button forwardButton;

	public RectTransform menuButtonRoot;

	public List<UIMenuBarButton> menuButtons;

	public Button settingsMenuButton;

	public GameObject coinsDisplayForwardObj;

	public RectTransform coinsAnimTarget;

	private Animator animator;

	private CanvasGroup canvasGroup;

	private UIDataManager dataManager;

	private ImageManager imageManager;

	private Canvas coinsDisplaySortOverride;

	public void Init(UIDataManager dataManager, ImageManager imageManager)
	{
		animator = GetComponent<Animator>();
		canvasGroup = GetComponent<CanvasGroup>();
		this.dataManager = dataManager;
		this.imageManager = imageManager;
	}

	public void SetTemplateMenuBarButton(GameObject go)
	{
		templateMenuBarButton = go;
	}

	public GameObject GetTemplateMenuBarButton()
	{
		UISceneElement componentInChildren = GetComponentInChildren<UISceneElement>();
		return componentInChildren.gameObject;
	}

	public void OnSceneLoad()
	{
		UISceneElement[] componentsInChildren = GetComponentsInChildren<UISceneElement>();
		UISceneElement[] array = componentsInChildren;
		foreach (UISceneElement uISceneElement in array)
		{
			uISceneElement.Init();
			uISceneElement.LoadContent(dataManager, imageManager);
		}
		PutCoinsDiplayInOverlay(inOverlay: false);
	}

	public void DeselectCustomButton()
	{
		if (currentCustomMenuBarButton != null)
		{
			currentCustomMenuBarButton.OnClean(MainUIController.Instance.loadedSceneController.GetComponent<Canvas>());
			currentCustomMenuBarButton = null;
		}
	}

	private void OnGUI()
	{
		if (!MainUIController.active)
		{
			return;
		}
		UIMenuBarButton[] componentsInChildren = GetComponentsInChildren<UIMenuBarButton>();
		if (componentsInChildren.Length < 3)
		{
			return;
		}
		UIMenuBarButton uIMenuBarButton = componentsInChildren[componentsInChildren.Length - 3];
		if (uIMenuBarButton == null)
		{
			return;
		}
		GameObject gameObject = uIMenuBarButton.gameObject;
		if (gameObject == null)
		{
			return;
		}
		RectTransform component = gameObject.GetComponent<RectTransform>();
		if (component == null)
		{
			return;
		}
		Rect rect = component.rect;
		rect.x = component.position.x;
		rect.y = 0f;
		Dictionary<string, MenuBarButton> menuBarButtons = UIRegistry.GetMenuBarButtons();
		if (menuBarButtons == null || MainUIController.Instance == null || MainUIController.Instance.loadedSceneController == null)
		{
			return;
		}
		Canvas component2 = MainUIController.Instance.loadedSceneController.GetComponent<Canvas>();
		if (component2 == null)
		{
			return;
		}
		foreach (string key in menuBarButtons.Keys)
		{
			if (string.IsNullOrEmpty(key))
			{
				continue;
			}
			MenuBarButton menuBarButton = menuBarButtons[key];
			if (menuBarButton == null)
			{
				continue;
			}
			rect.x += rect.width;
			string text = menuBarButton.Title ?? "Unnamed Button";
			if (!GUI.Button(rect, text))
			{
				continue;
			}
			UIMenuBarButton[] array = componentsInChildren;
			foreach (UIMenuBarButton uIMenuBarButton2 in array)
			{
				if (uIMenuBarButton2 != null)
				{
					uIMenuBarButton2.Deselect();
				}
			}
			DeselectCustomButton();
			GameObject gameObject2 = component2.gameObject;
			if (gameObject2 != null && gameObject2.transform != null)
			{
				gameObject2.transform.DetachChildren();
				Transform[] array2 = new Transform[gameObject2.transform.childCount];
				for (int j = 0; j < gameObject2.transform.childCount; j++)
				{
					array2[j] = gameObject2.transform.GetChild(j);
				}
				Transform[] array3 = array2;
				foreach (Transform transform in array3)
				{
					if (transform != null)
					{
						transform.parent = null;
					}
				}
			}
			try
			{
				menuBarButton.OnInit(component2);
			}
			catch (Exception ex)
			{
				Debug.LogError("Error calling OnInit on MenuBarButton: " + ex.Message);
			}
			currentCustomMenuBarButton = menuBarButton;
		}
		if (currentCustomMenuBarButton == null || !currentCustomMenuBarButton.usesIMGUI())
		{
			return;
		}
		try
		{
			currentCustomMenuBarButton.OnRender();
		}
		catch (Exception ex2)
		{
			Debug.LogError("Error calling OnRender on currentCustomMenuBarButton: " + ex2.Message);
		}
	}

	public void SetInteractable(bool interactable)
	{
		if (interactable)
		{
			animator.SetTrigger("Enable");
		}
		else
		{
			animator.SetTrigger("Disable");
		}
	}

	public void SelectMenuButton(MenuBarButtonEnum buttonType)
	{
		foreach (UIMenuBarButton menuButton in menuButtons)
		{
			if (menuButton.menuBarButton == buttonType)
			{
				menuButton.Select();
				break;
			}
		}
	}

	public void DeselectMenuButtons()
	{
		UIMenuBarButton[] componentsInChildren = GetComponentsInChildren<UIMenuBarButton>();
		foreach (UIMenuBarButton uIMenuBarButton in componentsInChildren)
		{
			uIMenuBarButton.Deselect();
		}
		DeselectCustomButton();
	}

	public void PutCoinsDiplayInOverlay(bool inOverlay)
	{
		coinsDisplayForwardObj.SetActive(inOverlay);
	}

	public void ButtonTapped_Back()
	{
		if (MainUIController.active)
		{
			MainUIController.Instance.NavigateBack();
		}
	}

	public void ButtonTapped_Forward()
	{
		if (MainUIController.active)
		{
			MainUIController.Instance.NavigateForward();
		}
	}

	public void DoSearch(string searchStr)
	{
		if (MainUIController.active)
		{
			MainUIController.Instance.Search(searchStr);
		}
	}

	public void Show(bool show)
	{
		canvasGroup.alpha = ((!show) ? 0f : 1f);
	}

	public void ShowBackButton()
	{
		backButton.interactable = true;
	}

	public void HideBackButton()
	{
		backButton.interactable = false;
	}

	public void ShowForwardButton()
	{
		forwardButton.interactable = true;
	}

	public void HideForwardButton()
	{
		forwardButton.interactable = false;
	}
}
