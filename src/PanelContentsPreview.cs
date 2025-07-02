using UnityEngine;

[ExecuteInEditMode]
public class PanelContentsPreview : MonoBehaviour
{
	private GameObject _sourceObject;

	private SharedUIResources _sharedResources;

	private UIDataSource _dataSource;

	private string _id;

	private GameObject _targetObject;

	public void CreatePreview(GameObject source, string id, UIDataSource dataSource, SharedUIResources sharedResources)
	{
		_id = id;
		_dataSource = dataSource;
		_sourceObject = source;
		_sharedResources = sharedResources;
		RefreshContents();
		Debug.Log("Creating preview");
	}

	public void Update()
	{
		RefreshContents();
	}

	private void RefreshContents()
	{
		if (_targetObject != null)
		{
			Object.DestroyImmediate(_targetObject);
		}
		Debug.Log("Refreshing preview");
		if (_sourceObject != null && _dataSource != null && _sharedResources != null)
		{
			_sharedResources.imageManager.ClearListeners();
			_targetObject = Object.Instantiate(_sourceObject);
			RectTransform rectTransform = (RectTransform)_targetObject.transform;
			rectTransform.SetParent((RectTransform)base.transform, worldPositionStays: false);
			UIPanelContents componentInChildren = _targetObject.GetComponentInChildren<UIPanelContents>();
			componentInChildren.Init();
			componentInChildren.SetupPanel(_dataSource, _sharedResources.imageManager, _id);
		}
	}
}
