using System;
using UnityEngine;

public class Tween
{
	private float duration;

	private float start;

	private float end;

	private float from;

	private float to;

	public void Start(float duration, float from = 0f, float to = 1f)
	{
		this.duration = duration;
		start = Time.time;
		end = start + duration;
		this.from = from;
		this.to = to;
	}

	public void Finish()
	{
		end = Time.time;
	}

	public float Value()
	{
		if (Time.time >= end)
		{
			return to;
		}
		return from + (to - from) * 0.5f * (0f - Mathf.Cos((Time.time - start) / duration * (float)Math.PI) + 1f);
	}

	public float TimePassed()
	{
		return Time.time - start;
	}

	public bool IsFinished()
	{
		return Time.time >= end;
	}
}
