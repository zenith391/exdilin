using System;
using UnityEngine;

// Token: 0x020001F8 RID: 504
public class BlocksworldComponentData : MonoBehaviour
{
	// Token: 0x06001A0D RID: 6669 RVA: 0x000C100C File Offset: 0x000BF40C
	public void Awake()
	{
		Blocksworld.Bootstrap(base.gameObject);
	}

	// Token: 0x04001578 RID: 5496
	public Camera guiCamera;

	// Token: 0x04001579 RID: 5497
	public Camera rewardCamera;

	// Token: 0x0400157A RID: 5498
	public Texture buttonPlus;

	// Token: 0x0400157B RID: 5499
	public Texture buttonMinus;

	// Token: 0x0400157C RID: 5500
	public GameObject prefabArrow;

	// Token: 0x0400157D RID: 5501
	public ParticleSystem stars;

	// Token: 0x0400157E RID: 5502
	public ParticleSystem starsReward;

	// Token: 0x0400157F RID: 5503
	public Vector3 firstPersonDeadZone = new Vector3(5f, 5f, 0f);

	// Token: 0x04001580 RID: 5504
	public float firstPersonTurnPower = 1f;

	// Token: 0x04001581 RID: 5505
	public float firstPersonTorque = 0.5f;

	// Token: 0x04001582 RID: 5506
	public float aimAdjustMin = 10f;

	// Token: 0x04001583 RID: 5507
	public float aimAdjustMax = 50f;

	// Token: 0x04001584 RID: 5508
	public float maxSpeedFoV = 35f;

	// Token: 0x04001585 RID: 5509
	public Texture[] hudTextures;

	// Token: 0x04001586 RID: 5510
	public bool FPCLookXFlip;

	// Token: 0x04001587 RID: 5511
	public bool FPCLookYFlip;
}
