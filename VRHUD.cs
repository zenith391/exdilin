using System;
using UnityEngine;

// Token: 0x02000333 RID: 819
public class VRHUD : MonoBehaviour
{
	// Token: 0x06002525 RID: 9509 RVA: 0x0010F150 File Offset: 0x0010D550
	private void OnPreRender()
	{
		Blocksworld.blocksworldCamera.SetReticleCameraEyePosition(base.transform.localPosition.x);
	}
}
