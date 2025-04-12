using System;
using UnityEngine;

// Token: 0x02000010 RID: 16
[AddComponentMenu("Cardboard/Audio/CardboardAudioSource")]
public class CardboardAudioSource : MonoBehaviour
{
	// Token: 0x17000005 RID: 5
	// (get) Token: 0x06000063 RID: 99 RVA: 0x000045F8 File Offset: 0x000029F8
	// (set) Token: 0x06000064 RID: 100 RVA: 0x00004600 File Offset: 0x00002A00
	public AudioClip clip
	{
		get
		{
			return this.clip;
		}
		set
		{
			this.sourceClip = value;
			if (this.audioSource != null)
			{
				this.audioSource.clip = this.sourceClip;
			}
		}
	}

	// Token: 0x17000006 RID: 6
	// (get) Token: 0x06000065 RID: 101 RVA: 0x0000462B File Offset: 0x00002A2B
	public bool isPlaying
	{
		get
		{
			return this.audioSource != null && this.audioSource.isPlaying;
		}
	}

	// Token: 0x17000007 RID: 7
	// (get) Token: 0x06000066 RID: 102 RVA: 0x0000464B File Offset: 0x00002A4B
	// (set) Token: 0x06000067 RID: 103 RVA: 0x00004653 File Offset: 0x00002A53
	public bool loop
	{
		get
		{
			return this.sourceLoop;
		}
		set
		{
			this.sourceLoop = value;
			if (this.audioSource != null)
			{
				this.audioSource.loop = this.sourceLoop;
			}
		}
	}

	// Token: 0x17000008 RID: 8
	// (get) Token: 0x06000068 RID: 104 RVA: 0x0000467E File Offset: 0x00002A7E
	// (set) Token: 0x06000069 RID: 105 RVA: 0x00004686 File Offset: 0x00002A86
	public bool mute
	{
		get
		{
			return this.sourceMute;
		}
		set
		{
			this.sourceMute = value;
			if (this.audioSource != null)
			{
				this.audioSource.mute = this.sourceMute;
			}
		}
	}

	// Token: 0x17000009 RID: 9
	// (get) Token: 0x0600006A RID: 106 RVA: 0x000046B1 File Offset: 0x00002AB1
	// (set) Token: 0x0600006B RID: 107 RVA: 0x000046B9 File Offset: 0x00002AB9
	public float pitch
	{
		get
		{
			return this.sourcePitch;
		}
		set
		{
			this.sourcePitch = value;
			if (this.audioSource != null)
			{
				this.audioSource.pitch = this.sourcePitch;
			}
		}
	}

	// Token: 0x1700000A RID: 10
	// (get) Token: 0x0600006C RID: 108 RVA: 0x000046E4 File Offset: 0x00002AE4
	// (set) Token: 0x0600006D RID: 109 RVA: 0x000046EC File Offset: 0x00002AEC
	public float volume
	{
		get
		{
			return this.sourceVolume;
		}
		set
		{
			this.sourceVolume = value;
			if (this.audioSource != null)
			{
				this.audioSource.volume = this.sourceVolume;
			}
		}
	}

	// Token: 0x1700000B RID: 11
	// (get) Token: 0x0600006E RID: 110 RVA: 0x00004717 File Offset: 0x00002B17
	// (set) Token: 0x0600006F RID: 111 RVA: 0x0000471F File Offset: 0x00002B1F
	public float maxDistance
	{
		get
		{
			return this.sourceMaxDistance;
		}
		set
		{
			this.sourceMaxDistance = Mathf.Clamp(this.sourceMaxDistance, this.sourceMinDistance + 0.01f, 1000000f);
		}
	}

	// Token: 0x1700000C RID: 12
	// (get) Token: 0x06000070 RID: 112 RVA: 0x00004743 File Offset: 0x00002B43
	// (set) Token: 0x06000071 RID: 113 RVA: 0x0000474B File Offset: 0x00002B4B
	public float minDistance
	{
		get
		{
			return this.sourceMinDistance;
		}
		set
		{
			this.sourceMinDistance = Mathf.Clamp(value, 0f, 990099f);
		}
	}

	// Token: 0x06000072 RID: 114 RVA: 0x00004764 File Offset: 0x00002B64
	private void Awake()
	{
		this.audioSource = base.gameObject.AddComponent<AudioSource>();
		this.audioSource.hideFlags = HideFlags.HideInInspector;
		this.audioSource.playOnAwake = false;
		this.audioSource.bypassReverbZones = true;
		this.audioSource.spatialBlend = 0f;
		this.OnValidate();
	}

	// Token: 0x06000073 RID: 115 RVA: 0x000047BC File Offset: 0x00002BBC
	private void OnEnable()
	{
		this.audioSource.enabled = true;
		if (this.playOnAwake && !this.isPlaying)
		{
			this.Play();
		}
	}

	// Token: 0x06000074 RID: 116 RVA: 0x000047E6 File Offset: 0x00002BE6
	private void Start()
	{
		if (this.playOnAwake && !this.isPlaying)
		{
			this.Play();
		}
	}

	// Token: 0x06000075 RID: 117 RVA: 0x00004804 File Offset: 0x00002C04
	private void OnDisable()
	{
		this.Stop();
		this.audioSource.enabled = false;
	}

	// Token: 0x06000076 RID: 118 RVA: 0x00004818 File Offset: 0x00002C18
	private void OnDestroy()
	{
		UnityEngine.Object.Destroy(this.audioSource);
	}

	// Token: 0x06000077 RID: 119 RVA: 0x00004828 File Offset: 0x00002C28
	private void Update()
	{
		if (!this.occlusionEnabled)
		{
			this.targetOcclusion = 0f;
		}
		else if (Time.time >= this.nextOcclusionUpdate)
		{
			this.nextOcclusionUpdate = Time.time + 0.2f;
			this.targetOcclusion = CardboardAudio.ComputeOcclusion(base.transform);
		}
		this.currentOcclusion = Mathf.Lerp(this.currentOcclusion, this.targetOcclusion, 6f * Time.deltaTime);
		if (!this.isPlaying && !this.isPaused)
		{
			this.Stop();
		}
		else
		{
			CardboardAudio.UpdateAudioSource(this.id, base.transform, this.gainDb, this.rolloffMode, this.sourceMinDistance, this.sourceMaxDistance, this.directivityAlpha, this.directivitySharpness, this.currentOcclusion);
		}
	}

	// Token: 0x06000078 RID: 120 RVA: 0x00004900 File Offset: 0x00002D00
	public void Pause()
	{
		if (this.audioSource != null)
		{
			this.isPaused = true;
			this.audioSource.Pause();
		}
	}

	// Token: 0x06000079 RID: 121 RVA: 0x00004925 File Offset: 0x00002D25
	public void Play()
	{
		if (this.audioSource != null && this.InitializeSource())
		{
			this.audioSource.Play();
			this.isPaused = false;
		}
	}

	// Token: 0x0600007A RID: 122 RVA: 0x00004955 File Offset: 0x00002D55
	public void PlayDelayed(float delay)
	{
		if (this.audioSource != null && this.InitializeSource())
		{
			this.audioSource.PlayDelayed(delay);
			this.isPaused = false;
		}
	}

	// Token: 0x0600007B RID: 123 RVA: 0x00004986 File Offset: 0x00002D86
	public void PlayOneShot(AudioClip clip)
	{
		this.PlayOneShot(clip, 1f);
	}

	// Token: 0x0600007C RID: 124 RVA: 0x00004994 File Offset: 0x00002D94
	public void PlayOneShot(AudioClip clip, float volume)
	{
		if (this.audioSource != null && this.InitializeSource())
		{
			this.audioSource.PlayOneShot(clip, volume);
			this.isPaused = false;
		}
	}

	// Token: 0x0600007D RID: 125 RVA: 0x000049C6 File Offset: 0x00002DC6
	public void Stop()
	{
		if (this.audioSource != null)
		{
			this.audioSource.Stop();
			this.ShutdownSource();
			this.isPaused = false;
		}
	}

	// Token: 0x0600007E RID: 126 RVA: 0x000049F1 File Offset: 0x00002DF1
	public void UnPause()
	{
		if (this.audioSource != null)
		{
			this.audioSource.UnPause();
			this.isPaused = false;
		}
	}

	// Token: 0x0600007F RID: 127 RVA: 0x00004A18 File Offset: 0x00002E18
	private bool InitializeSource()
	{
		if (this.id < 0)
		{
			this.id = CardboardAudio.CreateAudioSource(this.hrtfEnabled);
			if (this.id >= 0)
			{
				CardboardAudio.UpdateAudioSource(this.id, base.transform, this.gainDb, this.rolloffMode, this.sourceMinDistance, this.sourceMaxDistance, this.directivityAlpha, this.directivitySharpness, this.currentOcclusion);
				this.audioSource.spatialize = true;
				this.audioSource.SetSpatializerFloat(0, (float)this.id);
			}
		}
		return this.id >= 0;
	}

	// Token: 0x06000080 RID: 128 RVA: 0x00004AB5 File Offset: 0x00002EB5
	private void ShutdownSource()
	{
		if (this.id >= 0)
		{
			this.audioSource.SetSpatializerFloat(0, -1f);
			this.audioSource.spatialize = false;
			CardboardAudio.DestroyAudioSource(this.id);
			this.id = -1;
		}
	}

	// Token: 0x06000081 RID: 129 RVA: 0x00004AF4 File Offset: 0x00002EF4
	private void OnValidate()
	{
		this.clip = this.sourceClip;
		this.loop = this.sourceLoop;
		this.mute = this.sourceMute;
		this.pitch = this.sourcePitch;
		this.volume = this.sourceVolume;
		this.minDistance = this.sourceMinDistance;
		this.maxDistance = this.sourceMaxDistance;
	}

	// Token: 0x06000082 RID: 130 RVA: 0x00004B55 File Offset: 0x00002F55
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color(0.75f, 0.75f, 1f, 0.5f);
		this.DrawDirectivityGizmo(180);
	}

	// Token: 0x06000083 RID: 131 RVA: 0x00004B80 File Offset: 0x00002F80
	private void DrawDirectivityGizmo(int resolution)
	{
		Vector2[] array = CardboardAudio.Generate2dPolarPattern(this.directivityAlpha, this.directivitySharpness, resolution);
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

	// Token: 0x040000D6 RID: 214
	public float directivityAlpha;

	// Token: 0x040000D7 RID: 215
	public float directivitySharpness = 1f;

	// Token: 0x040000D8 RID: 216
	public float gainDb;

	// Token: 0x040000D9 RID: 217
	public bool occlusionEnabled;

	// Token: 0x040000DA RID: 218
	public bool playOnAwake = true;

	// Token: 0x040000DB RID: 219
	public AudioRolloffMode rolloffMode;

	// Token: 0x040000DC RID: 220
	[SerializeField]
	private AudioClip sourceClip;

	// Token: 0x040000DD RID: 221
	[SerializeField]
	private bool sourceLoop;

	// Token: 0x040000DE RID: 222
	[SerializeField]
	private bool sourceMute;

	// Token: 0x040000DF RID: 223
	[SerializeField]
	[Range(-3f, 3f)]
	private float sourcePitch = 1f;

	// Token: 0x040000E0 RID: 224
	[SerializeField]
	[Range(0f, 1f)]
	private float sourceVolume = 1f;

	// Token: 0x040000E1 RID: 225
	[SerializeField]
	private float sourceMaxDistance = 500f;

	// Token: 0x040000E2 RID: 226
	[SerializeField]
	private float sourceMinDistance;

	// Token: 0x040000E3 RID: 227
	[SerializeField]
	private bool hrtfEnabled = true;

	// Token: 0x040000E4 RID: 228
	private int id = -1;

	// Token: 0x040000E5 RID: 229
	private float currentOcclusion;

	// Token: 0x040000E6 RID: 230
	private float targetOcclusion;

	// Token: 0x040000E7 RID: 231
	private float nextOcclusionUpdate;

	// Token: 0x040000E8 RID: 232
	private AudioSource audioSource;

	// Token: 0x040000E9 RID: 233
	private bool isPaused;
}
