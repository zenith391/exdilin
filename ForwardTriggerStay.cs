using System;
using UnityEngine;

// Token: 0x02000209 RID: 521
public class ForwardTriggerStay : MonoBehaviour
{
	// Token: 0x06001A37 RID: 6711 RVA: 0x000C17FF File Offset: 0x000BFBFF
	private void OnTriggerStay(Collider collider)
	{
		TreasureHandler.ForwardTriggerStay(base.gameObject, collider);
	}
}
