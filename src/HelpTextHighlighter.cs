using UnityEngine;

public class HelpTextHighlighter
{
	protected GameObject go;

	public virtual void Update()
	{
	}

	public virtual bool IsVisible()
	{
		return go != null;
	}

	public virtual void Show()
	{
		if (go == null)
		{
			go = new GameObject("Highlight");
		}
	}

	public virtual void Destroy()
	{
		if (go != null)
		{
			Object.Destroy(go);
			go = null;
		}
	}
}
