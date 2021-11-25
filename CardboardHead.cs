using System;
using UnityEngine;

// Token: 0x02000018 RID: 24
[AddComponentMenu("Cardboard/CardboardHead")]
public class CardboardHead : MonoBehaviour
{
	// Token: 0x1700002F RID: 47
	// (get) Token: 0x060000ED RID: 237 RVA: 0x0000643A File Offset: 0x0000483A
	public Ray Gaze
	{
		get
		{
			this.UpdateHead();
			return new Ray(base.transform.position, base.transform.forward);
		}
	}

	// Token: 0x14000006 RID: 6
	// (add) Token: 0x060000EE RID: 238 RVA: 0x00006460 File Offset: 0x00004860
	// (remove) Token: 0x060000EF RID: 239 RVA: 0x00006498 File Offset: 0x00004898
	public event CardboardHead.HeadUpdatedDelegate OnHeadUpdated;

	// Token: 0x060000F0 RID: 240 RVA: 0x000064CE File Offset: 0x000048CE
	private void Awake()
	{
		Cardboard.Create();
	}

	// Token: 0x060000F1 RID: 241 RVA: 0x000064D5 File Offset: 0x000048D5
	private void Update()
	{
		this.updated = false;
		if (this.updateEarly)
		{
			this.UpdateHead();
		}
	}

	// Token: 0x060000F2 RID: 242 RVA: 0x000064EF File Offset: 0x000048EF
	private void LateUpdate()
	{
		this.UpdateHead();
	}

	// Token: 0x060000F3 RID: 243 RVA: 0x000064F8 File Offset: 0x000048F8
	private void UpdateHead()
	{
		if (this.updated)
		{
			return;
		}
		this.updated = true;
		Cardboard.SDK.UpdateState();
		if (this.trackRotation)
		{
			Quaternion orientation = Cardboard.SDK.HeadPose.Orientation;
			if (this.target == null)
			{
				base.transform.localRotation = orientation;
			}
			else
			{
				base.transform.rotation = this.target.rotation * orientation;
			}
		}
		if (this.trackPosition)
		{
			Vector3 position = Cardboard.SDK.HeadPose.Position;
			if (this.target == null)
			{
				base.transform.localPosition = position;
			}
			else
			{
				base.transform.position = this.target.position + this.target.rotation * position;
			}
		}
		if (this.OnHeadUpdated != null)
		{
			this.OnHeadUpdated(base.gameObject);
		}
	}

	// Token: 0x04000120 RID: 288
	public bool trackRotation = true;

	// Token: 0x04000121 RID: 289
	public bool trackPosition = true;

	// Token: 0x04000122 RID: 290
	public Transform target;

	// Token: 0x04000123 RID: 291
	public bool updateEarly;

	// Token: 0x04000125 RID: 293
	private bool updated;

	// Token: 0x02000019 RID: 25
	// (Invoke) Token: 0x060000F5 RID: 245
	public delegate void HeadUpdatedDelegate(GameObject head);
}
