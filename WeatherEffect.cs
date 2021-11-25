using System;
using Blocks;
using UnityEngine;

// Token: 0x02000357 RID: 855
public class WeatherEffect
{
	// Token: 0x17000176 RID: 374
	// (get) Token: 0x06002623 RID: 9763 RVA: 0x00117302 File Offset: 0x00115702
	// (set) Token: 0x06002624 RID: 9764 RVA: 0x0011730A File Offset: 0x0011570A
	public float IntensityMultiplier
	{
		get
		{
			return this.intensityMultiplier;
		}
		set
		{
			this.intensityMultiplier = value;
		}
	}

	// Token: 0x06002625 RID: 9765 RVA: 0x00117313 File Offset: 0x00115713
	protected virtual float GetTargetVolume()
	{
		return Mathf.Clamp(this.volumeIntensityFactor * this.intensityMultiplier, 0f, 1f);
	}

	// Token: 0x06002626 RID: 9766 RVA: 0x00117334 File Offset: 0x00115734
	public virtual void Update()
	{
		if (WeatherEffect.weatherAudioSource != null)
		{
			WeatherEffect.weatherSfxObject.transform.position = Blocksworld.cameraPosition;
			float volume = WeatherEffect.weatherAudioSource.volume;
			float value = this.GetTargetVolume() - volume;
			float num = volume + Mathf.Clamp(value, -0.05f, 0.05f);
			if (Mathf.Abs(num - volume) > 0.01f)
			{
				WeatherEffect.weatherAudioSource.volume = num;
			}
			if (Blocksworld.updateCounter % 7 == 0)
			{
				WeatherEffect.lpFilter.enabled = BlockAbstractWater.CameraWithinAnyWater();
			}
		}
	}

	// Token: 0x06002627 RID: 9767 RVA: 0x001173C4 File Offset: 0x001157C4
	public virtual void Start()
	{
		if (WeatherEffect.weatherAudioSource == null)
		{
			WeatherEffect.weatherSfxObject = new GameObject("Weather SFX");
			WeatherEffect.weatherAudioSource = WeatherEffect.weatherSfxObject.AddComponent<AudioSource>();
			WeatherEffect.lpFilter = WeatherEffect.weatherSfxObject.AddComponent<AudioLowPassFilter>();
			WeatherEffect.lpFilter.enabled = false;
			WeatherEffect.lpFilter.cutoffFrequency = 600f;
			WeatherEffect.weatherAudioSource.maxDistance = 100000f;
			WeatherEffect.weatherAudioSource.minDistance = 1000f;
			WeatherEffect.weatherAudioSource.rolloffMode = AudioRolloffMode.Linear;
			WeatherEffect.weatherAudioSource.dopplerLevel = 0f;
			WeatherEffect.weatherAudioSource.playOnAwake = false;
			WeatherEffect.weatherAudioSource.loop = true;
			WeatherEffect.weatherAudioSource.volume = 0f;
		}
		if (string.IsNullOrEmpty(this.loopSfx))
		{
			WeatherEffect.weatherAudioSource.Stop();
		}
		else if (Sound.sfxEnabled)
		{
			if (this.loopSfx != WeatherEffect.currentLoopSfx)
			{
				AudioClip sfx = Sound.GetSfx(this.loopSfx);
				WeatherEffect.weatherAudioSource.clip = sfx;
				WeatherEffect.currentLoopSfx = this.loopSfx;
			}
			WeatherEffect.weatherAudioSource.Play();
		}
	}

	// Token: 0x06002628 RID: 9768 RVA: 0x001174EF File Offset: 0x001158EF
	public virtual void Stop()
	{
		this.paused = false;
	}

	// Token: 0x06002629 RID: 9769 RVA: 0x001174F8 File Offset: 0x001158F8
	public virtual float GetFogMultiplier()
	{
		return 1f;
	}

	// Token: 0x0600262A RID: 9770 RVA: 0x001174FF File Offset: 0x001158FF
	public virtual void FogChanged()
	{
	}

	// Token: 0x0600262B RID: 9771 RVA: 0x00117501 File Offset: 0x00115901
	public virtual void SetEffectAngle(float angle)
	{
		this.effectAngle = angle;
	}

	// Token: 0x0600262C RID: 9772 RVA: 0x0011750A File Offset: 0x0011590A
	public virtual void Pause()
	{
		this.paused = true;
	}

	// Token: 0x0600262D RID: 9773 RVA: 0x00117513 File Offset: 0x00115913
	public virtual void Resume()
	{
		this.paused = false;
	}

	// Token: 0x0600262E RID: 9774 RVA: 0x0011751C File Offset: 0x0011591C
	public virtual void Reset()
	{
		this.Stop();
	}

	// Token: 0x0600262F RID: 9775 RVA: 0x00117524 File Offset: 0x00115924
	public static void ResetAll()
	{
		if (WeatherEffect.weatherSfxObject != null)
		{
			UnityEngine.Object.Destroy(WeatherEffect.weatherSfxObject);
		}
		WeatherEffect.weatherSfxObject = null;
		WeatherEffect.weatherAudioSource = null;
		if (WeatherEffect.snow != null)
		{
			WeatherEffect.snow.Reset();
		}
		if (WeatherEffect.ash != null)
		{
			WeatherEffect.ash.Reset();
		}
		if (WeatherEffect.rain != null)
		{
			WeatherEffect.rain.Reset();
		}
		if (WeatherEffect.sandStorm != null)
		{
			WeatherEffect.sandStorm.Reset();
		}
		if (WeatherEffect.greenLeaves != null)
		{
			WeatherEffect.greenLeaves.Reset();
		}
		if (WeatherEffect.autumnLeaves != null)
		{
			WeatherEffect.autumnLeaves.Reset();
		}
		if (WeatherEffect.meteors != null)
		{
			WeatherEffect.meteors.Reset();
		}
		if (WeatherEffect.meteors2 != null)
		{
			WeatherEffect.meteors2.Reset();
		}
		if (WeatherEffect.spaceDust != null)
		{
			WeatherEffect.spaceDust.Reset();
		}
	}

	// Token: 0x04002124 RID: 8484
	public static SnowEffect snow = new SnowEffect();

	// Token: 0x04002125 RID: 8485
	public static AshEffect ash = new AshEffect();

	// Token: 0x04002126 RID: 8486
	public static RainEffect rain = new RainEffect();

	// Token: 0x04002127 RID: 8487
	public static SandStormEffect sandStorm = new SandStormEffect();

	// Token: 0x04002128 RID: 8488
	public static BlowingLeavesEffect greenLeaves = new BlowingLeavesEffect(true);

	// Token: 0x04002129 RID: 8489
	public static BlowingLeavesEffect autumnLeaves = new BlowingLeavesEffect(false);

	// Token: 0x0400212A RID: 8490
	public static MeteorShowerEffect meteors = new MeteorShowerEffect();

	// Token: 0x0400212B RID: 8491
	public static MeteorShowerEffect meteors2 = new MeteorShowerEffect();

	// Token: 0x0400212C RID: 8492
	public static SpaceDustEffect spaceDust = new SpaceDustEffect();

	// Token: 0x0400212D RID: 8493
	public static ClearWeatherEffect clear = new ClearWeatherEffect();

	// Token: 0x0400212E RID: 8494
	protected float intensityMultiplier = 1f;

	// Token: 0x0400212F RID: 8495
	protected float effectAngle;

	// Token: 0x04002130 RID: 8496
	protected static AudioSource weatherAudioSource;

	// Token: 0x04002131 RID: 8497
	protected static GameObject weatherSfxObject;

	// Token: 0x04002132 RID: 8498
	protected static string currentLoopSfx = string.Empty;

	// Token: 0x04002133 RID: 8499
	protected static AudioLowPassFilter lpFilter;

	// Token: 0x04002134 RID: 8500
	protected float volumeIntensityFactor = 0.5f;

	// Token: 0x04002135 RID: 8501
	protected string loopSfx = string.Empty;

	// Token: 0x04002136 RID: 8502
	protected bool paused;
}
