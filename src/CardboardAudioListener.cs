using UnityEngine;

[RequireComponent(typeof(AudioListener))]
[AddComponentMenu("Cardboard/Audio/CardboardAudioListener")]
public class CardboardAudioListener : MonoBehaviour
{
	public float globalGainDb;

	public float worldScale = 1f;

	[SerializeField]
	private CardboardAudio.Quality quality = CardboardAudio.Quality.Medium;

	private void Awake()
	{
		CardboardAudio.Initialize(this, quality);
	}

	private void OnEnable()
	{
		CardboardAudio.UpdateAudioListener(globalGainDb, worldScale);
	}

	private void OnDestroy()
	{
		CardboardAudio.Shutdown(this);
	}

	private void Update()
	{
		CardboardAudio.UpdateAudioListener(globalGainDb, worldScale);
	}

	private void OnAudioFilterRead(float[] data, int channels)
	{
		CardboardAudio.ProcessAudioListener(data, data.Length);
	}
}
