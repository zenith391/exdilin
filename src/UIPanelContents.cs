using System.Collections.Generic;
using UnityEngine;

public class UIPanelContents : MonoBehaviour
{
	public PanelContentsDelegate panelContentsDelegate;

	[HideInInspector]
	public string itemId;

	public UIDataSource dataSource;

	public bool autoLoadDataSourceFromScene;

	public bool requestFullData;

	public bool loadFromDataSourceInfo;

	public List<string> debugLoadedData;

	private List<UIPanelElement> _panelElements;

	private Animator _animator;

	private bool _showing;

	private bool _initComplete;

	private bool _layoutComplete;

	public void Init()
	{
		if (_initComplete)
		{
			return;
		}
		UIPanelElement[] componentsInChildren = GetComponentsInChildren<UIPanelElement>();
		_panelElements = new List<UIPanelElement>();
		UIPanelElement[] array = componentsInChildren;
		foreach (UIPanelElement uIPanelElement in array)
		{
			UIPanelContents componentInParent = uIPanelElement.GetComponentInParent<UIPanelContents>();
			if (componentInParent == this)
			{
				_panelElements.Add(uIPanelElement);
				uIPanelElement.Init(this);
				uIPanelElement.Clear();
			}
		}
		_animator = GetComponent<Animator>();
		_initComplete = true;
	}

	public void SetupPanel(UIDataSource dataSource, ImageManager imageManager)
	{
		this.dataSource = dataSource;
		if (_panelElements == null)
		{
			Init();
		}
		foreach (UIPanelElement panelElement in _panelElements)
		{
			panelElement.SetImageManager(imageManager);
			panelElement.Clear();
		}
		_layoutComplete = false;
		dataSource.AddListener(OnDataSourceReloaded);
		Layout();
	}

	public void SetupPanel(UIDataSource dataSource, ImageManager imageManager, string itemId)
	{
		if (this.dataSource == dataSource && this.itemId == itemId)
		{
			return;
		}
		this.dataSource = dataSource;
		if (_panelElements == null)
		{
			Init();
		}
		foreach (UIPanelElement panelElement in _panelElements)
		{
			panelElement.SetImageManager(imageManager);
			panelElement.Clear();
		}
		this.itemId = itemId;
		_layoutComplete = false;
		if (requestFullData)
		{
			dataSource.RequestFullData(itemId);
		}
		dataSource.AddListener(OnDataSourceReloaded);
		Layout();
	}

	public void OnDataSourceReloaded(List<string> modifedKeys)
	{
		if (loadFromDataSourceInfo || modifedKeys.Contains(itemId))
		{
			_layoutComplete = false;
			Layout();
		}
	}

	public void Clear()
	{
		if (_panelElements == null)
		{
			return;
		}
		foreach (UIPanelElement panelElement in _panelElements)
		{
			if (!(panelElement == null))
			{
				panelElement.Clear();
			}
		}
		itemId = string.Empty;
		_layoutComplete = false;
	}

	public void ButtonPressed_Close()
	{
		if (panelContentsDelegate != null)
		{
			panelContentsDelegate.OnCloseButtonPressed(this);
		}
	}

	public void ButtonPressed_Play()
	{
		string playButtonMessage = dataSource.GetPlayButtonMessage();
		if (!string.IsNullOrEmpty(playButtonMessage) && MainUIController.active)
		{
			MainUIController.Instance.HandleMessage(playButtonMessage, itemId, dataSource.dataType, dataSource.dataSubtype);
		}
	}

	public void ButtonPressed(string message)
	{
		if (MainUIController.active)
		{
			MainUIController.Instance.HandleMessage(message, itemId, dataSource.dataType, dataSource.dataSubtype);
		}
	}

	public void ElementEditedText(string dataKey, string newValueStr)
	{
		dataSource.OverwriteData(itemId, dataKey, newValueStr);
	}

	public void ElementEditedBool(string dataKey, bool newValue)
	{
		dataSource.OverwriteData(itemId, dataKey, (!newValue) ? "false" : "true");
	}

	public void ElementEditedFloat(string dataKey, float newValue)
	{
		dataSource.OverwriteData(itemId, dataKey, newValue.ToString());
	}

	private void Layout()
	{
		if (_layoutComplete || !dataSource.IsDataLoaded())
		{
			return;
		}
		if (loadFromDataSourceInfo)
		{
			Dictionary<string, string> info = dataSource.Info;
			foreach (UIPanelElement panelElement in _panelElements)
			{
				panelElement.Fill(info);
			}
			_layoutComplete = true;
			debugLoadedData = new List<string>();
			foreach (KeyValuePair<string, string> item in dataSource.Info)
			{
				debugLoadedData.Add(item.Key + ": " + item.Value);
			}
			if (panelContentsDelegate != null)
			{
				panelContentsDelegate.OnLayoutComplete(this);
			}
			return;
		}
		Dictionary<string, Dictionary<string, string>> data = dataSource.Data;
		if (!data.ContainsKey(itemId))
		{
			BWLog.Info("no data with id " + itemId);
			return;
		}
		Dictionary<string, string> data2 = data[itemId];
		foreach (UIPanelElement panelElement2 in _panelElements)
		{
			panelElement2.Fill(data2);
		}
		UISceneElement[] componentsInChildren = GetComponentsInChildren<UISceneElement>();
		UISceneElement[] array = componentsInChildren;
		foreach (UISceneElement uISceneElement in array)
		{
			UIPanelContents componentInParent = uISceneElement.GetComponentInParent<UIPanelContents>();
			if (componentInParent == this && uISceneElement.getIDFromParentPanel)
			{
				uISceneElement.dataSubtype = itemId;
				if (MainUIController.active)
				{
					uISceneElement.LoadContent(MainUIController.Instance.dataManager, MainUIController.Instance.imageManager);
				}
			}
		}
		_layoutComplete = true;
		if (panelContentsDelegate != null)
		{
			panelContentsDelegate.OnLayoutComplete(this);
		}
	}

	public void Show()
	{
		if (_animator != null && _animator.runtimeAnimatorController != null)
		{
			_animator.SetBool("Visible", value: true);
		}
	}

	public void Hide()
	{
		if (_animator != null && _animator.runtimeAnimatorController != null)
		{
			_animator.SetBool("Visible", value: false);
		}
	}

	public void OnDestroy()
	{
		if (dataSource != null)
		{
			dataSource.RemoveListener(OnDataSourceReloaded);
		}
	}
}
