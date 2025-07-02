using Blocks;
using UnityEngine;

public class MusicPlayerEffectsCommand : Command
{
	public float highpassStartHeight = 50f;

	public float highpassMaxHeight = 300f;

	public float highpassMaxCutoff = 5000f;

	public bool highpassOnHeight;

	public float minHighpassY = -999999f;

	private bool prevHighpassOnHeight;

	public bool forceInWaterLowpass;

	public float lowpassCutoff = 600f;

	public bool lowpassInWater;

	private bool prevLowpassInWater;

	public float distortionLevel;

	public bool distortionOn;

	public bool prevDistortionOn;

	private AudioHighPassFilter highpassFilter;

	private AudioLowPassFilter lowpassFilter;

	private AudioDistortionFilter distortionFilter;

	public override void Execute()
	{
		if (!highpassOnHeight && !lowpassInWater && !distortionOn && distortionFilter == null)
		{
			done = true;
		}
		GameObject gameObject = Blocksworld.musicPlayer.gameObject;
		if (highpassOnHeight != prevHighpassOnHeight)
		{
			if (highpassOnHeight)
			{
				highpassFilter = gameObject.AddComponent<AudioHighPassFilter>();
			}
			else
			{
				DestroyHighpass();
			}
		}
		if (lowpassInWater != prevLowpassInWater)
		{
			if (lowpassInWater)
			{
				lowpassFilter = gameObject.AddComponent<AudioLowPassFilter>();
			}
			else
			{
				DestroyLowpass();
			}
		}
		if (distortionOn != prevDistortionOn && distortionOn && distortionFilter == null)
		{
			distortionFilter = gameObject.AddComponent<AudioDistortionFilter>();
		}
		Vector3 position = Blocksworld.cameraTransform.position;
		if (lowpassInWater && lowpassFilter != null)
		{
			if (forceInWaterLowpass || BlockAbstractWater.CameraWithinAnyWater())
			{
				if (!lowpassFilter.enabled)
				{
					lowpassFilter.enabled = true;
				}
				lowpassFilter.cutoffFrequency = lowpassCutoff;
			}
			else if (lowpassFilter.enabled)
			{
				lowpassFilter.enabled = false;
			}
		}
		if (highpassOnHeight && highpassFilter != null)
		{
			float num = (Mathf.Max(position.y, minHighpassY) - highpassStartHeight) / highpassMaxHeight;
			highpassFilter.cutoffFrequency = Mathf.Clamp(num * highpassMaxCutoff, 1f, highpassMaxCutoff);
		}
		if (!distortionOn && distortionFilter != null)
		{
			distortionLevel -= 0.05f;
			distortionFilter.distortionLevel = distortionLevel;
			if (distortionLevel < 0.01f)
			{
				DestroyDistortion();
			}
		}
		if (distortionOn && distortionFilter != null)
		{
			distortionFilter.distortionLevel = distortionLevel;
		}
		prevHighpassOnHeight = highpassOnHeight;
		prevLowpassInWater = lowpassInWater;
		prevDistortionOn = distortionOn;
		lowpassInWater = false;
		highpassOnHeight = false;
		forceInWaterLowpass = false;
		distortionOn = false;
	}

	private void DestroyHighpass()
	{
		if (highpassFilter != null)
		{
			Object.Destroy(highpassFilter);
			highpassFilter = null;
		}
	}

	private void DestroyLowpass()
	{
		if (lowpassFilter != null)
		{
			Object.Destroy(lowpassFilter);
			lowpassFilter = null;
		}
	}

	private void DestroyDistortion()
	{
		if (distortionFilter != null)
		{
			Object.Destroy(distortionFilter);
			distortionFilter = null;
		}
	}

	public override void Removed()
	{
		DestroyHighpass();
		DestroyLowpass();
		DestroyDistortion();
		lowpassInWater = false;
		prevLowpassInWater = false;
		highpassOnHeight = false;
		prevHighpassOnHeight = false;
		distortionOn = false;
		prevDistortionOn = false;
		minHighpassY = -999999f;
	}
}
