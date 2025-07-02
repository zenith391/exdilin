using System.Collections.Generic;
using Gestures;
using UnityEngine;

public class DragGestureCommand : GestureCommand
{
	protected Vector2 targetPos;

	protected Vector2 startPos;

	protected float fraction;

	protected float endDelay;

	public const float DEFAULT_SPEED = 0.3f;

	public float speed = 0.3f;

	private float endTime;

	public DragGestureCommand()
	{
	}

	public DragGestureCommand(Vector2 startPos, Vector2 targetPos, float endDelay = 0f)
	{
		this.startPos = startPos;
		this.targetPos = targetPos;
		this.endDelay = endDelay;
	}

	public override void Execute(HashSet<Gestures.Touch> touches)
	{
		Gestures.Touch touch;
		if (fraction == 0f)
		{
			touch = new Gestures.Touch(startPos);
			touch.Phase = TouchPhase.Began;
			touch.Position = startPos;
			fraction += speed;
		}
		else if (fraction < 1f)
		{
			fraction += speed;
			if (fraction >= 1f)
			{
				endTime = Time.time + endDelay;
				fraction = 1f;
			}
			touch = new Gestures.Touch(fraction * targetPos + (1f - fraction) * startPos);
			touch.Phase = TouchPhase.Moved;
		}
		else
		{
			float time = Time.time;
			fraction = 1f;
			touch = new Gestures.Touch(targetPos);
			if (time < endTime)
			{
				touch.Phase = TouchPhase.Moved;
			}
			else
			{
				touch.Phase = TouchPhase.Ended;
				done = true;
			}
		}
		Blocksworld.mouseBlock = Blocksworld.BlockAtMouse(touch.Position, out var _);
		touch.Command = this;
		touches.Add(touch);
	}
}
