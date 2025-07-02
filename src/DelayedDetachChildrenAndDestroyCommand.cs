using UnityEngine;

public class DelayedDetachChildrenAndDestroyCommand : DelayedCommand
{
	private GameObject go;

	public DelayedDetachChildrenAndDestroyCommand(GameObject go)
		: base(3)
	{
		this.go = go;
	}

	private void DetachDestroy()
	{
		if (go != null)
		{
			go.transform.DetachChildren();
			Object.Destroy(go);
			go = null;
		}
	}

	protected override void DelayedExecute()
	{
		base.DelayedExecute();
		DetachDestroy();
	}

	public override void Removed()
	{
		base.Removed();
		if (go != null)
		{
			DetachDestroy();
		}
	}
}
