using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIPanelElementPopupSpawner : UIPanelElement
{
	public UIPopup prefab;

	private Button button;

	private UIPopup popup;

	private Dictionary<string, string> dataForPopup;

	private void OnEnable()
	{
		button = GetComponent<Button>();
		button.onClick.AddListener(Spawn);
	}

	private void OnDisable()
	{
		button.onClick.RemoveListener(Spawn);
	}

	private void Spawn()
	{
		popup = BWStandalone.Overlays.ShowPopup(prefab);
		if (parentPanel != null)
		{
			popup.LoadData(parentPanel.dataSource, imageManager, parentPanel.itemId);
		}
	}
}
