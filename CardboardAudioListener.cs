using System;
using UnityEngine;

// Token: 0x0200000D RID: 13
[RequireComponent(typeof(AudioListener))]
[AddComponentMenu("Cardboard/Audio/CardboardAudioListener")]
public class CardboardAudioListener : MonoBehaviour
{
	// Token: 0x06000053 RID: 83 RVA: 0x00004389 File Offset: 0x00002789
	private void Awake()
	{
		CardboardAudio.Initialize(this, this.quality);
	}

	// Token: 0x06000054 RID: 84 RVA: 0x00004397 File Offset: 0x00002797
	private void OnEnable()
	{
		CardboardAudio.UpdateAudioListener(this.globalGainDb, this.worldScale);
	}

	// Token: 0x06000055 RID: 85 RVA: 0x000043AA File Offset: 0x000027AA
	private void OnDestroy()
	{
		CardboardAudio.Shutdown(this);
	}

	// Token: 0x06000056 RID: 86 RVA: 0x000043B2 File Offset: 0x000027B2
	private void Update()
	{
		CardboardAudio.UpdateAudioListener(this.globalGainDb, this.worldScale);
	}

	// Token: 0x06000057 RID: 87 RVA: 0x000043C5 File Offset: 0x000027C5
	private void OnAudioFilterRead(float[] data, int channels)
	{
		CardboardAudio.ProcessAudioListener(data, data.Length);
	}

	// Token: 0x040000AF RID: 175
	public float globalGainDb;

	// Token: 0x040000B0 RID: 176
	public float worldScale = 1f;

	// Token: 0x040000B1 RID: 177
	[SerializeField]
	private CardboardAudio.Quality quality = CardboardAudio.Quality.Medium;
}
