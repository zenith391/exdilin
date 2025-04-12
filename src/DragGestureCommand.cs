using System;
using System.Collections.Generic;
using Gestures;
using UnityEngine;

// Token: 0x02000139 RID: 313
public class DragGestureCommand : GestureCommand
{
	// Token: 0x06001440 RID: 5184 RVA: 0x0008E519 File Offset: 0x0008C919
	public DragGestureCommand()
	{
	}

	// Token: 0x06001441 RID: 5185 RVA: 0x0008E52C File Offset: 0x0008C92C
	public DragGestureCommand(Vector2 startPos, Vector2 targetPos, float endDelay = 0f)
	{
		this.startPos = startPos;
		this.targetPos = targetPos;
		this.endDelay = endDelay;
	}

	// Token: 0x06001442 RID: 5186 RVA: 0x0008E554 File Offset: 0x0008C954
	public override void Execute(HashSet<Gestures.Touch> touches)
	{
		Gestures.Touch touch;
		if (this.fraction == 0f)
		{
			touch = new Gestures.Touch(this.startPos);
			touch.Phase = TouchPhase.Began;
			touch.Position = this.startPos;
			this.fraction += this.speed;
		}
		else if (this.fraction < 1f)
		{
			this.fraction += this.speed;
			if (this.fraction >= 1f)
			{
				this.endTime = Time.time + this.endDelay;
				this.fraction = 1f;
			}
			touch = new Gestures.Touch(this.fraction * this.targetPos + (1f - this.fraction) * this.startPos);
			touch.Phase = TouchPhase.Moved;
		}
		else
		{
			float time = Time.time;
			this.fraction = 1f;
			touch = new Gestures.Touch(this.targetPos);
			if (time < this.endTime)
			{
				touch.Phase = TouchPhase.Moved;
			}
			else
			{
				touch.Phase = TouchPhase.Ended;
				this.done = true;
			}
		}
		int num;
		Blocksworld.mouseBlock = Blocksworld.BlockAtMouse(touch.Position, out num);
		touch.Command = this;
		touches.Add(touch);
	}

	// Token: 0x04000FF8 RID: 4088
	protected Vector2 targetPos;

	// Token: 0x04000FF9 RID: 4089
	protected Vector2 startPos;

	// Token: 0x04000FFA RID: 4090
	protected float fraction;

	// Token: 0x04000FFB RID: 4091
	protected float endDelay;

	// Token: 0x04000FFC RID: 4092
	public const float DEFAULT_SPEED = 0.3f;

	// Token: 0x04000FFD RID: 4093
	public float speed = 0.3f;

	// Token: 0x04000FFE RID: 4094
	private float endTime;
}
