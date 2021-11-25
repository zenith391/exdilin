using System;
using UnityEngine;

// Token: 0x0200033C RID: 828
public class LetterboxVisualEffect : VisualEffect
{
	// Token: 0x06002548 RID: 9544 RVA: 0x00110059 File Offset: 0x0010E459
	public LetterboxVisualEffect(string name, bool moveIn) : base(name)
	{
		this.moveIn = moveIn;
	}

	// Token: 0x06002549 RID: 9545 RVA: 0x00110070 File Offset: 0x0010E470
	public override void Begin()
	{
		base.Begin();
		GameObject gameObject = Blocksworld.mainCamera.gameObject;
		LetterboxingEffect x = gameObject.GetComponent<LetterboxingEffect>();
		if (x == null)
		{
			x = gameObject.AddComponent<LetterboxingEffect>();
		}
		this.theEffect = x;
	}

	// Token: 0x0600254A RID: 9546 RVA: 0x001100AF File Offset: 0x0010E4AF
	public override void End()
	{
		base.End();
		if (!this.moveIn)
		{
			this.Destroy();
		}
	}

	// Token: 0x0600254B RID: 9547 RVA: 0x001100C8 File Offset: 0x0010E4C8
	public override void Destroy()
	{
		base.Destroy();
		UnityEngine.Object.Destroy(this.theEffect);
		this.theEffect = null;
	}

	// Token: 0x0600254C RID: 9548 RVA: 0x001100E4 File Offset: 0x0010E4E4
	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (this.theEffect != null && !base.HasEnded())
		{
			float num = this.currentTime / this.timeLength;
			if (!this.moveIn)
			{
				num = 1f - num;
			}
			float fraction = num * 0.05f;
			this.theEffect.SetFraction(fraction);
		}
	}

	// Token: 0x04001FE2 RID: 8162
	private bool moveIn = true;

	// Token: 0x04001FE3 RID: 8163
	private LetterboxingEffect theEffect;
}
