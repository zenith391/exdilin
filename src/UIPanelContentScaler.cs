using UnityEngine;

[ExecuteInEditMode]
public class UIPanelContentScaler : MonoBehaviour
{
	public bool useWidth;

	public Vector2 defaultParentSize;

	private RectTransform parentTransform;

	private Vector3 scale = Vector3.one;

	private void Update()
	{
		parentTransform = (RectTransform)base.transform.parent;
		if (parentTransform != null && defaultParentSize.sqrMagnitude > Mathf.Epsilon)
		{
			RectTransform rectTransform = (RectTransform)base.transform;
			if (useWidth)
			{
				scale.y = (scale.x = parentTransform.rect.width / defaultParentSize.x);
			}
			else
			{
				scale.y = (scale.x = parentTransform.rect.height / defaultParentSize.y);
			}
			rectTransform.localScale = scale;
		}
	}
}
