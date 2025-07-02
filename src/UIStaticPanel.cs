using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIPanelContents))]
public class UIStaticPanel : UISceneElement, PanelContentsDelegate
{
	public int index;

	private bool contentLoaded;

	private UIPanelContents panelContents;

	private string id;

	private void Awake()
	{
		Init();
	}

	public override void Init()
	{
		panelContents = GetComponent<UIPanelContents>();
		panelContents.Init();
		panelContents.panelContentsDelegate = this;
		contentLoaded = false;
	}

	public void OnDisable()
	{
		if (dataSource != null)
		{
			dataSource.RemoveListener(OnDataLoaded);
		}
	}

	protected override void LoadContentFromDataSource()
	{
		dataSource.AddListener(OnDataLoaded);
		if (forceReloadData)
		{
			dataSource.ClearData();
		}
		dataSource.LoadIfNeeded();
	}

	public override void UnloadContent()
	{
		dataSource.RemoveListener(OnDataLoaded);
		Clear();
	}

	private void Clear()
	{
		panelContents.Clear();
	}

	public override void UnloadEditorExampleContent()
	{
		panelContents.Clear();
	}

	private void OnDataLoaded(List<string> modifiedKeys)
	{
		panelContents.Clear();
		contentLoaded = false;
		Layout();
	}

	private void Layout()
	{
		if (!contentLoaded)
		{
			List<string> keys = dataSource.Keys;
			if (keys.Count > index)
			{
				id = keys[index];
				panelContents.SetupPanel(dataSource, imageManager, id);
			}
			contentLoaded = true;
		}
	}

	public void OnLayoutComplete(UIPanelContents panelContents)
	{
	}

	public void OnCloseButtonPressed(UIPanelContents panelContents)
	{
	}
}
