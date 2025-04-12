using System;
using UnityEngine;

// Token: 0x020001E8 RID: 488
public class ArrowPrefabData : MonoBehaviour
{
	// Token: 0x060018D0 RID: 6352 RVA: 0x000AF5A4 File Offset: 0x000AD9A4
	public void Awake()
	{
		Type type = Type.GetType("Arrow");
		if (type == null)
		{
			type = Type.GetType("Arrow,Blocksworld");
		}
		type.GetMethod("Bootstrap").Invoke(null, new GameObject[]
		{
			base.gameObject
		});
	}

	// Token: 0x040013DF RID: 5087
	public GameObject prefabBody;

	// Token: 0x040013E0 RID: 5088
	public GameObject prefabHead;
}
