using System;
using UnityEngine;

// Token: 0x020003D9 RID: 985
[ExecuteInEditMode]
public class PanelContentsPreview : MonoBehaviour
{
	// Token: 0x06002BBB RID: 11195 RVA: 0x0013B899 File Offset: 0x00139C99
	public void CreatePreview(GameObject source, string id, UIDataSource dataSource, SharedUIResources sharedResources)
	{
		this._id = id;
		this._dataSource = dataSource;
		this._sourceObject = source;
		this._sharedResources = sharedResources;
		this.RefreshContents();
		Debug.Log("Creating preview");
	}

	// Token: 0x06002BBC RID: 11196 RVA: 0x0013B8C8 File Offset: 0x00139CC8
	public void Update()
	{
		this.RefreshContents();
	}

	// Token: 0x06002BBD RID: 11197 RVA: 0x0013B8D0 File Offset: 0x00139CD0
	private void RefreshContents()
	{
		if (this._targetObject != null)
		{
			UnityEngine.Object.DestroyImmediate(this._targetObject);
		}
		Debug.Log("Refreshing preview");
		if (this._sourceObject != null && this._dataSource != null && this._sharedResources != null)
		{
			this._sharedResources.imageManager.ClearListeners();
			this._targetObject = UnityEngine.Object.Instantiate<GameObject>(this._sourceObject);
			RectTransform rectTransform = (RectTransform)this._targetObject.transform;
			rectTransform.SetParent((RectTransform)base.transform, false);
			UIPanelContents componentInChildren = this._targetObject.GetComponentInChildren<UIPanelContents>();
			componentInChildren.Init();
			componentInChildren.SetupPanel(this._dataSource, this._sharedResources.imageManager, this._id);
		}
	}

	// Token: 0x040024F5 RID: 9461
	private GameObject _sourceObject;

	// Token: 0x040024F6 RID: 9462
	private SharedUIResources _sharedResources;

	// Token: 0x040024F7 RID: 9463
	private UIDataSource _dataSource;

	// Token: 0x040024F8 RID: 9464
	private string _id;

	// Token: 0x040024F9 RID: 9465
	private GameObject _targetObject;
}
