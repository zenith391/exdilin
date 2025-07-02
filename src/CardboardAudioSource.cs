using UnityEngine;

[AddComponentMenu("Cardboard/Audio/CardboardAudioSource")]
public class CardboardAudioSource : MonoBehaviour
{
	public float directivityAlpha;

	public float directivitySharpness = 1f;

	public float gainDb;

	public bool occlusionEnabled;

	public bool playOnAwake = true;

	public AudioRolloffMode rolloffMode;

	[SerializeField]
	private AudioClip sourceClip;

	[SerializeField]
	private bool sourceLoop;

	[SerializeField]
	private bool sourceMute;

	[SerializeField]
	[Range(-3f, 3f)]
	private float sourcePitch = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float sourceVolume = 1f;

	[SerializeField]
	private float sourceMaxDistance = 500f;

	[SerializeField]
	private float sourceMinDistance;

	[SerializeField]
	private bool hrtfEnabled = true;

	private int id = -1;

	private float currentOcclusion;

	private float targetOcclusion;

	private float nextOcclusionUpdate;

	private AudioSource audioSource;

	private bool isPaused;

	public AudioClip clip
	{
		get
		{
			return clip;
		}
		set
		{
			sourceClip = value;
			if (audioSource != null)
			{
				audioSource.clip = sourceClip;
			}
		}
	}

	public bool isPlaying
	{
		get
		{
			if (audioSource != null)
			{
				return audioSource.isPlaying;
			}
			return false;
		}
	}

	public bool loop
	{
		get
		{
			return sourceLoop;
		}
		set
		{
			sourceLoop = value;
			if (audioSource != null)
			{
				audioSource.loop = sourceLoop;
			}
		}
	}

	public bool mute
	{
		get
		{
			return sourceMute;
		}
		set
		{
			sourceMute = value;
			if (audioSource != null)
			{
				audioSource.mute = sourceMute;
			}
		}
	}

	public float pitch
	{
		get
		{
			return sourcePitch;
		}
		set
		{
			sourcePitch = value;
			if (audioSource != null)
			{
				audioSource.pitch = sourcePitch;
			}
		}
	}

	public float volume
	{
		get
		{
			return sourceVolume;
		}
		set
		{
			sourceVolume = value;
			if (audioSource != null)
			{
				audioSource.volume = sourceVolume;
			}
		}
	}

	public float maxDistance
	{
		get
		{
			return sourceMaxDistance;
		}
		set
		{
			sourceMaxDistance = Mathf.Clamp(sourceMaxDistance, sourceMinDistance + 0.01f, 1000000f);
		}
	}

	public float minDistance
	{
		get
		{
			return sourceMinDistance;
		}
		set
		{
			sourceMinDistance = Mathf.Clamp(value, 0f, 990099f);
		}
	}

	private void Awake()
	{
		audioSource = base.gameObject.AddComponent<AudioSource>();
		audioSource.hideFlags = HideFlags.HideInInspector;
		audioSource.playOnAwake = false;
		audioSource.bypassReverbZones = true;
		audioSource.spatialBlend = 0f;
		OnValidate();
	}

	private void OnEnable()
	{
		audioSource.enabled = true;
		if (playOnAwake && !isPlaying)
		{
			Play();
		}
	}

	private void Start()
	{
		if (playOnAwake && !isPlaying)
		{
			Play();
		}
	}

	private void OnDisable()
	{
		Stop();
		audioSource.enabled = false;
	}

	private void OnDestroy()
	{
		Object.Destroy(audioSource);
	}

	private void Update()
	{
		if (!occlusionEnabled)
		{
			targetOcclusion = 0f;
		}
		else if (Time.time >= nextOcclusionUpdate)
		{
			nextOcclusionUpdate = Time.time + 0.2f;
			targetOcclusion = CardboardAudio.ComputeOcclusion(base.transform);
		}
		currentOcclusion = Mathf.Lerp(currentOcclusion, targetOcclusion, 6f * Time.deltaTime);
		if (!isPlaying && !isPaused)
		{
			Stop();
		}
		else
		{
			CardboardAudio.UpdateAudioSource(id, base.transform, gainDb, rolloffMode, sourceMinDistance, sourceMaxDistance, directivityAlpha, directivitySharpness, currentOcclusion);
		}
	}

	public void Pause()
	{
		if (audioSource != null)
		{
			isPaused = true;
			audioSource.Pause();
		}
	}

	public void Play()
	{
		if (audioSource != null && InitializeSource())
		{
			audioSource.Play();
			isPaused = false;
		}
	}

	public void PlayDelayed(float delay)
	{
		if (audioSource != null && InitializeSource())
		{
			audioSource.PlayDelayed(delay);
			isPaused = false;
		}
	}

	public void PlayOneShot(AudioClip clip)
	{
		PlayOneShot(clip, 1f);
	}

	public void PlayOneShot(AudioClip clip, float volume)
	{
		if (audioSource != null && InitializeSource())
		{
			audioSource.PlayOneShot(clip, volume);
			isPaused = false;
		}
	}

	public void Stop()
	{
		if (audioSource != null)
		{
			audioSource.Stop();
			ShutdownSource();
			isPaused = false;
		}
	}

	public void UnPause()
	{
		if (audioSource != null)
		{
			audioSource.UnPause();
			isPaused = false;
		}
	}

	private bool InitializeSource()
	{
		if (id < 0)
		{
			id = CardboardAudio.CreateAudioSource(hrtfEnabled);
			if (id >= 0)
			{
				CardboardAudio.UpdateAudioSource(id, base.transform, gainDb, rolloffMode, sourceMinDistance, sourceMaxDistance, directivityAlpha, directivitySharpness, currentOcclusion);
				audioSource.spatialize = true;
				audioSource.SetSpatializerFloat(0, id);
			}
		}
		return id >= 0;
	}

	private void ShutdownSource()
	{
		if (id >= 0)
		{
			audioSource.SetSpatializerFloat(0, -1f);
			audioSource.spatialize = false;
			CardboardAudio.DestroyAudioSource(id);
			id = -1;
		}
	}

	private void OnValidate()
	{
		clip = sourceClip;
		loop = sourceLoop;
		mute = sourceMute;
		pitch = sourcePitch;
		volume = sourceVolume;
		minDistance = sourceMinDistance;
		maxDistance = sourceMaxDistance;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color(0.75f, 0.75f, 1f, 0.5f);
		DrawDirectivityGizmo(180);
	}

	private void DrawDirectivityGizmo(int resolution)
	{
		Vector2[] array = CardboardAudio.Generate2dPolarPattern(directivityAlpha, directivitySharpness, resolution);
		int num = resolution + 1;
		Vector3[] array2 = new Vector3[num];
		array2[0] = Vector3.zero;
		for (int i = 0; i < array.Length; i++)
		{
			array2[i + 1] = new Vector3(array[i].x, 0f, array[i].y);
		}
		int[] array3 = new int[6 * num];
		for (int j = 0; j < num - 1; j++)
		{
			int num2 = 6 * j;
			if (j < num - 2)
			{
				array3[num2] = 0;
				array3[num2 + 1] = j + 1;
				array3[num2 + 2] = j + 2;
			}
			else
			{
				array3[num2] = 0;
				array3[num2 + 1] = num - 1;
				array3[num2 + 2] = 1;
			}
			array3[num2 + 3] = array3[num2];
			array3[num2 + 4] = array3[num2 + 2];
			array3[num2 + 5] = array3[num2 + 1];
		}
		Mesh mesh = new Mesh();
		mesh.hideFlags = HideFlags.DontSaveInEditor;
		mesh.vertices = array2;
		mesh.triangles = array3;
		mesh.RecalculateNormals();
		Vector3 scale = 2f * Mathf.Max(base.transform.lossyScale.x, base.transform.lossyScale.z) * Vector3.one;
		Gizmos.DrawMesh(mesh, base.transform.position, base.transform.rotation, scale);
	}
}
