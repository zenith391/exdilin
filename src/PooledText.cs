using System;
using UnityEngine;

// Token: 0x0200019A RID: 410
public class PooledText
{
	// Token: 0x060016EA RID: 5866 RVA: 0x000A4897 File Offset: 0x000A2C97
	public PooledText(GameObject master, PooledObject instance)
	{
		this.masterObject = master;
		this.pooledObject = instance;
		instance.GameObject.SetActive(true);
	}

	// Token: 0x060016EB RID: 5867 RVA: 0x000A48BC File Offset: 0x000A2CBC
	public void Return()
	{
		this.pooledObject.GameObject.transform.parent = HudMeshText.hudTextParent.transform;
		this.pooledObject.GameObject.transform.localScale = Vector3.one;
		this.pooledObject.GameObject.GetComponent<Renderer>().enabled = false;
		this.pooledObject.GameObject.SetActive(false);
		TextMesh component = this.pooledObject.GameObject.GetComponent<TextMesh>();
		component.text = string.Empty;
		ObjectPool.Return(this.masterObject, this.pooledObject.GameObject);
	}

	// Token: 0x040011E7 RID: 4583
	private GameObject masterObject;

	// Token: 0x040011E8 RID: 4584
	public PooledObject pooledObject;
}
