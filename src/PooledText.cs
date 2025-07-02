using UnityEngine;

public class PooledText
{
	private GameObject masterObject;

	public PooledObject pooledObject;

	public PooledText(GameObject master, PooledObject instance)
	{
		masterObject = master;
		pooledObject = instance;
		instance.GameObject.SetActive(value: true);
	}

	public void Return()
	{
		pooledObject.GameObject.transform.parent = HudMeshText.hudTextParent.transform;
		pooledObject.GameObject.transform.localScale = Vector3.one;
		pooledObject.GameObject.GetComponent<Renderer>().enabled = false;
		pooledObject.GameObject.SetActive(value: false);
		TextMesh component = pooledObject.GameObject.GetComponent<TextMesh>();
		component.text = string.Empty;
		ObjectPool.Return(masterObject, pooledObject.GameObject);
	}
}
