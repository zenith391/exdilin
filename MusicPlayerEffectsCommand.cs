using System;
using Blocks;
using UnityEngine;

// Token: 0x02000140 RID: 320
public class MusicPlayerEffectsCommand : Command
{
	// Token: 0x06001459 RID: 5209 RVA: 0x0008EEE0 File Offset: 0x0008D2E0
	public override void Execute()
	{
		if (!this.highpassOnHeight && !this.lowpassInWater && !this.distortionOn && this.distortionFilter == null)
		{
			this.done = true;
		}
		GameObject gameObject = Blocksworld.musicPlayer.gameObject;
		if (this.highpassOnHeight != this.prevHighpassOnHeight)
		{
			if (this.highpassOnHeight)
			{
				this.highpassFilter = gameObject.AddComponent<AudioHighPassFilter>();
			}
			else
			{
				this.DestroyHighpass();
			}
		}
		if (this.lowpassInWater != this.prevLowpassInWater)
		{
			if (this.lowpassInWater)
			{
				this.lowpassFilter = gameObject.AddComponent<AudioLowPassFilter>();
			}
			else
			{
				this.DestroyLowpass();
			}
		}
		if (this.distortionOn != this.prevDistortionOn && this.distortionOn && this.distortionFilter == null)
		{
			this.distortionFilter = gameObject.AddComponent<AudioDistortionFilter>();
		}
		Vector3 position = Blocksworld.cameraTransform.position;
		if (this.lowpassInWater && this.lowpassFilter != null)
		{
			if (this.forceInWaterLowpass || BlockAbstractWater.CameraWithinAnyWater())
			{
				if (!this.lowpassFilter.enabled)
				{
					this.lowpassFilter.enabled = true;
				}
				this.lowpassFilter.cutoffFrequency = this.lowpassCutoff;
			}
			else if (this.lowpassFilter.enabled)
			{
				this.lowpassFilter.enabled = false;
			}
		}
		if (this.highpassOnHeight && this.highpassFilter != null)
		{
			float num = (Mathf.Max(position.y, this.minHighpassY) - this.highpassStartHeight) / this.highpassMaxHeight;
			this.highpassFilter.cutoffFrequency = Mathf.Clamp(num * this.highpassMaxCutoff, 1f, this.highpassMaxCutoff);
		}
		if (!this.distortionOn && this.distortionFilter != null)
		{
			this.distortionLevel -= 0.05f;
			this.distortionFilter.distortionLevel = this.distortionLevel;
			if (this.distortionLevel < 0.01f)
			{
				this.DestroyDistortion();
			}
		}
		if (this.distortionOn && this.distortionFilter != null)
		{
			this.distortionFilter.distortionLevel = this.distortionLevel;
		}
		this.prevHighpassOnHeight = this.highpassOnHeight;
		this.prevLowpassInWater = this.lowpassInWater;
		this.prevDistortionOn = this.distortionOn;
		this.lowpassInWater = false;
		this.highpassOnHeight = false;
		this.forceInWaterLowpass = false;
		this.distortionOn = false;
	}

	// Token: 0x0600145A RID: 5210 RVA: 0x0008F17C File Offset: 0x0008D57C
	private void DestroyHighpass()
	{
		if (this.highpassFilter != null)
		{
			UnityEngine.Object.Destroy(this.highpassFilter);
			this.highpassFilter = null;
		}
	}

	// Token: 0x0600145B RID: 5211 RVA: 0x0008F1A1 File Offset: 0x0008D5A1
	private void DestroyLowpass()
	{
		if (this.lowpassFilter != null)
		{
			UnityEngine.Object.Destroy(this.lowpassFilter);
			this.lowpassFilter = null;
		}
	}

	// Token: 0x0600145C RID: 5212 RVA: 0x0008F1C6 File Offset: 0x0008D5C6
	private void DestroyDistortion()
	{
		if (this.distortionFilter != null)
		{
			UnityEngine.Object.Destroy(this.distortionFilter);
			this.distortionFilter = null;
		}
	}

	// Token: 0x0600145D RID: 5213 RVA: 0x0008F1EC File Offset: 0x0008D5EC
	public override void Removed()
	{
		this.DestroyHighpass();
		this.DestroyLowpass();
		this.DestroyDistortion();
		this.lowpassInWater = false;
		this.prevLowpassInWater = false;
		this.highpassOnHeight = false;
		this.prevHighpassOnHeight = false;
		this.distortionOn = false;
		this.prevDistortionOn = false;
		this.minHighpassY = -999999f;
	}

	// Token: 0x04001013 RID: 4115
	public float highpassStartHeight = 50f;

	// Token: 0x04001014 RID: 4116
	public float highpassMaxHeight = 300f;

	// Token: 0x04001015 RID: 4117
	public float highpassMaxCutoff = 5000f;

	// Token: 0x04001016 RID: 4118
	public bool highpassOnHeight;

	// Token: 0x04001017 RID: 4119
	public float minHighpassY = -999999f;

	// Token: 0x04001018 RID: 4120
	private bool prevHighpassOnHeight;

	// Token: 0x04001019 RID: 4121
	public bool forceInWaterLowpass;

	// Token: 0x0400101A RID: 4122
	public float lowpassCutoff = 600f;

	// Token: 0x0400101B RID: 4123
	public bool lowpassInWater;

	// Token: 0x0400101C RID: 4124
	private bool prevLowpassInWater;

	// Token: 0x0400101D RID: 4125
	public float distortionLevel;

	// Token: 0x0400101E RID: 4126
	public bool distortionOn;

	// Token: 0x0400101F RID: 4127
	public bool prevDistortionOn;

	// Token: 0x04001020 RID: 4128
	private AudioHighPassFilter highpassFilter;

	// Token: 0x04001021 RID: 4129
	private AudioLowPassFilter lowpassFilter;

	// Token: 0x04001022 RID: 4130
	private AudioDistortionFilter distortionFilter;
}
