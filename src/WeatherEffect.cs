using Blocks;
using UnityEngine;

public class WeatherEffect
{
	public static SnowEffect snow = new SnowEffect();

	public static AshEffect ash = new AshEffect();

	public static RainEffect rain = new RainEffect();

	public static SandStormEffect sandStorm = new SandStormEffect();

	public static BlowingLeavesEffect greenLeaves = new BlowingLeavesEffect(green: true);

	public static BlowingLeavesEffect autumnLeaves = new BlowingLeavesEffect(green: false);

	public static MeteorShowerEffect meteors = new MeteorShowerEffect();

	public static MeteorShowerEffect meteors2 = new MeteorShowerEffect();

	public static SpaceDustEffect spaceDust = new SpaceDustEffect();

	public static ClearWeatherEffect clear = new ClearWeatherEffect();

	protected float intensityMultiplier = 1f;

	protected float effectAngle;

	protected static AudioSource weatherAudioSource;

	protected static GameObject weatherSfxObject;

	protected static string currentLoopSfx = string.Empty;

	protected static AudioLowPassFilter lpFilter;

	protected float volumeIntensityFactor = 0.5f;

	protected string loopSfx = string.Empty;

	protected bool paused;

	public float IntensityMultiplier
	{
		get
		{
			return intensityMultiplier;
		}
		set
		{
			intensityMultiplier = value;
		}
	}

	protected virtual float GetTargetVolume()
	{
		return Mathf.Clamp(volumeIntensityFactor * intensityMultiplier, 0f, 1f);
	}

	public virtual void Update()
	{
		if (weatherAudioSource != null)
		{
			weatherSfxObject.transform.position = Blocksworld.cameraPosition;
			float volume = weatherAudioSource.volume;
			float value = GetTargetVolume() - volume;
			float num = volume + Mathf.Clamp(value, -0.05f, 0.05f);
			if (Mathf.Abs(num - volume) > 0.01f)
			{
				weatherAudioSource.volume = num;
			}
			if (Blocksworld.updateCounter % 7 == 0)
			{
				lpFilter.enabled = BlockAbstractWater.CameraWithinAnyWater();
			}
		}
	}

	public virtual void Start()
	{
		if (weatherAudioSource == null)
		{
			weatherSfxObject = new GameObject("Weather SFX");
			weatherAudioSource = weatherSfxObject.AddComponent<AudioSource>();
			lpFilter = weatherSfxObject.AddComponent<AudioLowPassFilter>();
			lpFilter.enabled = false;
			lpFilter.cutoffFrequency = 600f;
			weatherAudioSource.maxDistance = 100000f;
			weatherAudioSource.minDistance = 1000f;
			weatherAudioSource.rolloffMode = AudioRolloffMode.Linear;
			weatherAudioSource.dopplerLevel = 0f;
			weatherAudioSource.playOnAwake = false;
			weatherAudioSource.loop = true;
			weatherAudioSource.volume = 0f;
		}
		if (string.IsNullOrEmpty(loopSfx))
		{
			weatherAudioSource.Stop();
		}
		else if (Sound.sfxEnabled)
		{
			if (loopSfx != currentLoopSfx)
			{
				AudioClip sfx = Sound.GetSfx(loopSfx);
				weatherAudioSource.clip = sfx;
				currentLoopSfx = loopSfx;
			}
			weatherAudioSource.Play();
		}
	}

	public virtual void Stop()
	{
		paused = false;
	}

	public virtual float GetFogMultiplier()
	{
		return 1f;
	}

	public virtual void FogChanged()
	{
	}

	public virtual void SetEffectAngle(float angle)
	{
		effectAngle = angle;
	}

	public virtual void Pause()
	{
		paused = true;
	}

	public virtual void Resume()
	{
		paused = false;
	}

	public virtual void Reset()
	{
		Stop();
	}

	public static void ResetAll()
	{
		if (weatherSfxObject != null)
		{
			Object.Destroy(weatherSfxObject);
		}
		weatherSfxObject = null;
		weatherAudioSource = null;
		if (snow != null)
		{
			snow.Reset();
		}
		if (ash != null)
		{
			ash.Reset();
		}
		if (rain != null)
		{
			rain.Reset();
		}
		if (sandStorm != null)
		{
			sandStorm.Reset();
		}
		if (greenLeaves != null)
		{
			greenLeaves.Reset();
		}
		if (autumnLeaves != null)
		{
			autumnLeaves.Reset();
		}
		if (meteors != null)
		{
			meteors.Reset();
		}
		if (meteors2 != null)
		{
			meteors2.Reset();
		}
		if (spaceDust != null)
		{
			spaceDust.Reset();
		}
	}
}
