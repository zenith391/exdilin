using UnityEngine;

public class FpsCounter
{
	public float updateInterval = 0.5f;

	private float accum;

	private int frames;

	private float timeleft = -100f;

	private string format = string.Empty;

	private float fps = 30f;

	public static Rect GetRect()
	{
		float num = 200f;
		float num2 = 40f;
		return new Rect((float)(Screen.width / 2) - num / 2f, NormalizedScreen.scale * 2f, num * NormalizedScreen.scale, num2 * NormalizedScreen.scale);
	}

	public void ShowGUI()
	{
		if (timeleft == -100f)
		{
			timeleft = updateInterval;
		}
		timeleft -= Time.deltaTime;
		accum += Time.timeScale / Time.deltaTime;
		frames++;
		if ((double)timeleft <= 0.0)
		{
			fps = accum / (float)frames;
			format = $"{fps:F2} FPS";
			timeleft = updateInterval;
			accum = 0f;
			frames = 0;
		}
		if (Blocksworld.skin.customStyles.Length > 4)
		{
			GUIStyle style = Blocksworld.skin.customStyles[4];
			if (fps <= 10f)
			{
				GUI.color = Color.red;
			}
			else if (fps <= 20f)
			{
				GUI.color = Color.yellow;
			}
			else
			{
				GUI.color = Color.white;
			}
			GUI.Label(GetRect(), format, style);
		}
		else
		{
			BWLog.Info("no custom styles?");
		}
	}
}
