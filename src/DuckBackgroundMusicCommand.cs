using UnityEngine;

public class DuckBackgroundMusicCommand : Command
{
	private float currentVolumeMultiplier = 1f;

	private float targetVolumeMultiplier = 1f;

	private float unDuckTime = -1f;

	private readonly float alpha = 0.2f;

	public void DuckMusicVolume(float duration)
	{
		unDuckTime = Mathf.Max(Time.fixedTime + duration, unDuckTime);
		targetVolumeMultiplier = 0f;
	}

	public override void Execute()
	{
		if (unDuckTime < 0f)
		{
			done = true;
			return;
		}
		float fixedTime = Time.fixedTime;
		if (fixedTime > unDuckTime)
		{
			targetVolumeMultiplier = 1f;
		}
		currentVolumeMultiplier = (1f - alpha) * currentVolumeMultiplier + alpha * targetVolumeMultiplier;
		if (Mathf.Abs(currentVolumeMultiplier - targetVolumeMultiplier) < 0.01f)
		{
			currentVolumeMultiplier = targetVolumeMultiplier;
			if (fixedTime > unDuckTime)
			{
				done = true;
			}
		}
		SetMultiplier(currentVolumeMultiplier);
	}

	public override void Removed()
	{
		SetMultiplier(1f);
	}

	private void SetMultiplier(float m)
	{
		Blocksworld.SetBackgroundMusicVolumeMultiplier(m);
	}
}
