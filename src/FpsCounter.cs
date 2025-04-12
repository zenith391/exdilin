using System;
using UnityEngine;

// Token: 0x02000163 RID: 355
public class FpsCounter
{
	// Token: 0x06001546 RID: 5446 RVA: 0x00094970 File Offset: 0x00092D70
	public static Rect GetRect()
	{
		float num = 200f;
		float num2 = 40f;
		Rect result = new Rect((float)(Screen.width / 2) - num / 2f, NormalizedScreen.scale * 2f, num * NormalizedScreen.scale, num2 * NormalizedScreen.scale);
		return result;
	}

	// Token: 0x06001547 RID: 5447 RVA: 0x000949BC File Offset: 0x00092DBC
	public void ShowGUI()
	{
		if (this.timeleft == -100f)
		{
			this.timeleft = this.updateInterval;
		}
		this.timeleft -= Time.deltaTime;
		this.accum += Time.timeScale / Time.deltaTime;
		this.frames++;
		if ((double)this.timeleft <= 0.0)
		{
			this.fps = this.accum / (float)this.frames;
			this.format = string.Format("{0:F2} FPS", this.fps);
			this.timeleft = this.updateInterval;
			this.accum = 0f;
			this.frames = 0;
		}
		if (Blocksworld.skin.customStyles.Length > 4)
		{
			GUIStyle style = Blocksworld.skin.customStyles[4];
			if (this.fps <= 10f)
			{
				GUI.color = Color.red;
			}
			else if (this.fps <= 20f)
			{
				GUI.color = Color.yellow;
			}
			else
			{
				GUI.color = Color.white;
			}
			GUI.Label(FpsCounter.GetRect(), this.format, style);
		}
		else
		{
			BWLog.Info("no custom styles?");
		}
	}

	// Token: 0x040010A3 RID: 4259
	public float updateInterval = 0.5f;

	// Token: 0x040010A4 RID: 4260
	private float accum;

	// Token: 0x040010A5 RID: 4261
	private int frames;

	// Token: 0x040010A6 RID: 4262
	private float timeleft = -100f;

	// Token: 0x040010A7 RID: 4263
	private string format = string.Empty;

	// Token: 0x040010A8 RID: 4264
	private float fps = 30f;
}
