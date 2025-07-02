using UnityEngine;

public class UnlockInputCommand : Command
{
	private float unlockTime;

	public void SetUnlockTime(float unlockTime)
	{
		this.unlockTime = Mathf.Max(unlockTime, this.unlockTime);
	}

	public override void Execute()
	{
		if (Blocksworld.lockInput && Time.fixedTime > unlockTime)
		{
			Blocksworld.lockInput = false;
		}
	}

	public override void Removed()
	{
		Blocksworld.lockInput = false;
	}
}
