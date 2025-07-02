using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIPopup : MonoBehaviour, IMenuInputHandler
{
	public UIPanelContents popupPanel;

	public Text mainText;

	public bool hideOnSceneLoad = true;

	public bool closeOnBackgroundClick;

	public Selectable defaultSelectable;

	private string itemID;

	private string dataType;

	private string dataSubtype;

	private IPopupDelegate popupDelegate;

	public virtual void LoadData(UIDataSource dataSource, ImageManager imageManager, string itemID)
	{
		dataType = dataSource.dataType;
		dataSubtype = dataSource.dataSubtype;
		this.itemID = itemID;
		if (popupPanel != null)
		{
			popupPanel.SetupPanel(dataSource, imageManager, itemID);
		}
	}

	public void Show(IPopupDelegate popupDelegate)
	{
		this.popupDelegate = popupDelegate;
		if (hideOnSceneLoad)
		{
			SceneManager.activeSceneChanged += OnSceneChange;
		}
		MenuInputHandler.RequestControl(this);
	}

	public void Hide()
	{
		popupDelegate.ClosePopup();
		SceneManager.activeSceneChanged -= OnSceneChange;
		MenuInputHandler.Release(this);
	}

	public void OnButtonPressed(string message)
	{
		if (MainUIController.active)
		{
			MainUIController.Instance.HandleMessage(message, itemID, dataType, dataSubtype);
		}
	}

	private void OnDestroy()
	{
		MenuInputHandler.Release(this);
	}

	private void OnSceneChange(Scene fromScene, Scene toScene)
	{
		Hide();
	}

	public virtual void HandleMenuInputEvents()
	{
		if (MappedInput.InputDown(MappableInput.MENU_CANCEL))
		{
			Hide();
		}
	}
}
