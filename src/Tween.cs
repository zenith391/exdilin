using System;
using UnityEngine;

// Token: 0x020002E5 RID: 741
public class Tween
{
	// Token: 0x060021E6 RID: 8678 RVA: 0x000FE359 File Offset: 0x000FC759
	public void Start(float duration, float from = 0f, float to = 1f)
	{
		this.duration = duration;
		this.start = Time.time;
		this.end = this.start + duration;
		this.from = from;
		this.to = to;
	}

	// Token: 0x060021E7 RID: 8679 RVA: 0x000FE389 File Offset: 0x000FC789
	public void Finish()
	{
		this.end = Time.time;
	}

	// Token: 0x060021E8 RID: 8680 RVA: 0x000FE398 File Offset: 0x000FC798
	public float Value()
	{
		if (Time.time >= this.end)
		{
			return this.to;
		}
		return this.from + (this.to - this.from) * 0.5f * (-Mathf.Cos((Time.time - this.start) / this.duration * 3.14159274f) + 1f);
	}

	// Token: 0x060021E9 RID: 8681 RVA: 0x000FE3FC File Offset: 0x000FC7FC
	public float TimePassed()
	{
		return Time.time - this.start;
	}

	// Token: 0x060021EA RID: 8682 RVA: 0x000FE40A File Offset: 0x000FC80A
	public bool IsFinished()
	{
		return Time.time >= this.end;
	}

	// Token: 0x04001CF0 RID: 7408
	private float duration;

	// Token: 0x04001CF1 RID: 7409
	private float start;

	// Token: 0x04001CF2 RID: 7410
	private float end;

	// Token: 0x04001CF3 RID: 7411
	private float from;

	// Token: 0x04001CF4 RID: 7412
	private float to;
}
