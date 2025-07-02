using UnityEngine;

public class WinLoseManager
{
	public static bool winning;

	public static bool losing;

	public static bool ending;

	public static UISpeechBubble messageWindow;

	public static bool playedStinger;

	public static bool buttonPressed;

	private static ParticleSystem winParticleSystem;

	public static ParticleSystem GetParticleSystem()
	{
		if (winParticleSystem == null)
		{
			GameObject gameObject = Object.Instantiate(Blocksworld.starsReward.gameObject);
			winParticleSystem = gameObject.GetComponent<ParticleSystem>();
		}
		return winParticleSystem;
	}

	public static bool IsFinished()
	{
		if (!winning && !losing)
		{
			return ending;
		}
		return true;
	}

	public static void Reset()
	{
		winning = false;
		losing = false;
		ending = false;
		messageWindow = null;
		playedStinger = false;
		buttonPressed = false;
	}
}
