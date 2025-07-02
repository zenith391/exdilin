using System.Collections;
using System.Text;
using Exdilin;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
	public delegate void FadeCompletionHandler();

	public const float DefaultVolume = 0.4f;

	private const float FadeTime = 0.2f;

	private const float MinPlayFrequency = 0.75f;

	private AudioSource _source;

	private string _current;

	private bool _isEnabled = true;

	private float _volumeMultiplier = 1f;

	private float _currentVolume = 1f;

	private bool _isLoading;

	private string _playWhenEnabled;

	private string _loadingName;

	private float _loadingVolume;

	private bool _playScheduled;

	private float _playNextVolume;

	private string _playNextName;

	private float _lastPlayTime;

	private float _nextPlayTime;

	private WWW _audioLoader;

	private AudioClip _loadingClip;

	private bool _enableStreaming = true;

	private float _musicVolume = 1f;

	public static MusicPlayer Create()
	{
		GameObject gameObject = new GameObject("MusicPlayer");
		AudioSource audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.priority = 0;
		MusicPlayer musicPlayer = gameObject.AddComponent<MusicPlayer>();
		musicPlayer.RefreshVolumeFromSettings();
		return musicPlayer;
	}

	public string GetCurrentMusicName()
	{
		return _current;
	}

	public void SetMusic(string name, float volume = 0.4f)
	{
		if (string.IsNullOrEmpty(name))
		{
			Stop();
		}
		else if (name == _current && _source.clip != null)
		{
			_playScheduled = false;
			_isLoading = false;
			if (!_source.isPlaying)
			{
				Resume();
			}
		}
		else if (!_isEnabled)
		{
			_playWhenEnabled = name;
		}
		else
		{
			LoadAndPlay(name, volume);
		}
	}

	private void SetVolume(float v)
	{
		_currentVolume = v;
		_source.volume = _currentVolume * _volumeMultiplier * _musicVolume;
	}

	public void SetVolumeMultiplier(float m)
	{
		_volumeMultiplier = m;
		SetVolume(_currentVolume);
	}

	public void SetEnabled(bool enabled)
	{
		_isEnabled = enabled;
		if (enabled)
		{
			if (_isLoading || _playScheduled)
			{
				return;
			}
			if (_source.clip != null)
			{
				if (!_source.isPlaying)
				{
					Play();
				}
			}
			else if (!string.IsNullOrEmpty(_current))
			{
				LoadAndPlay(_current, 0.4f);
			}
			else if (!string.IsNullOrEmpty(_playWhenEnabled))
			{
				LoadAndPlay(_playWhenEnabled, 0.4f);
			}
		}
		else
		{
			Unload();
		}
	}

	public void Pause()
	{
		FadeOut(stop: false, null);
	}

	public void Resume()
	{
		if (_isEnabled && _source.clip != null && !_source.isPlaying)
		{
			Play();
		}
	}

	public void Stop()
	{
		if (_source.isPlaying)
		{
			FadeOut(stop: true, null);
		}
	}

	public void Unload()
	{
		_isLoading = false;
		_playScheduled = false;
		ClearLoader();
		if (!(_source == null))
		{
			if (_source.isPlaying)
			{
				_source.Stop();
			}
			if (_source.clip != null)
			{
				_source.clip = null;
			}
			_current = null;
		}
	}

	public void StopWithCompletion(FadeCompletionHandler completion)
	{
		FadeOut(stop: true, completion);
	}

	private string FileUrlForName(string name)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(BWFilesystem.FileProtocolPrefixStr);
		if (Application.isEditor)
		{
			stringBuilder.Append(Application.dataPath);
			stringBuilder.Append("/StreamingAssets/Music/");
		}
		else
		{
			stringBuilder.Append(Application.streamingAssetsPath);
			stringBuilder.Append("/Music/");
		}
		stringBuilder.Append(name);
		stringBuilder.Append(".ogg");
		return stringBuilder.ToString();
	}

	private string WebUrlForName(string name)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(Application.streamingAssetsPath);
		stringBuilder.Append("/Music/");
		stringBuilder.Append(name);
		stringBuilder.Append(".ogg");
		return stringBuilder.ToString();
	}

	private void LoadAndPlay(string name, float volume)
	{
		if ((!_playScheduled || !(_playNextName == name)) && (!_isLoading || !(_loadingName == name)))
		{
			_isLoading = false;
			ClearLoader();
			_playNextName = name;
			_playNextVolume = volume;
			_nextPlayTime = Time.time + 0.75f;
			_playScheduled = true;
		}
	}

	private void Play(float vol = 0.4f)
	{
		if (_isEnabled)
		{
			StartCoroutine(FadeIn(vol));
			_source.Play();
		}
	}

	private void FadeOut(bool stop, FadeCompletionHandler completion)
	{
		StartCoroutine(FadeOutWithCompletion(stop, completion));
	}

	private IEnumerator FadeIn(float volume)
	{
		SetVolume(0f);
		int steps = (int)(0.2f / Time.smoothDeltaTime);
		float dV = volume / (float)steps;
		for (int i = 0; i < steps; i++)
		{
			SetVolume((float)i * dV);
			yield return null;
		}
		SetVolume(volume);
	}

	private IEnumerator FadeOutWithCompletion(bool stop, FadeCompletionHandler completionHandler)
	{
		float startVolume = _currentVolume;
		if (startVolume <= 0f || !_source.isPlaying)
		{
			_source.Stop();
			completionHandler?.Invoke();
			yield break;
		}
		int steps = (int)(0.2f / Time.smoothDeltaTime);
		float dV = startVolume / (float)steps;
		for (int i = 0; i < steps; i++)
		{
			SetVolume(startVolume - (float)i * dV);
			yield return null;
		}
		SetVolume(0f);
		if (stop)
		{
			_source.Stop();
			_source.clip = null;
		}
		else
		{
			_source.Pause();
		}
		completionHandler?.Invoke();
	}

	private void Awake()
	{
		_source = GetComponent<AudioSource>();
		_source.loop = false;
		SetVolume(0.4f);
	}

	private void Update()
	{
		if (_isLoading)
		{
			ContinueLoad();
		}
		else
		{
			if (!_isEnabled)
			{
				return;
			}
			if (_playScheduled && Time.time > _nextPlayTime)
			{
				if (_playNextName == _current)
				{
					SetVolume(_playNextVolume);
				}
				else
				{
					_loadingName = _playNextName;
					_loadingVolume = _playNextVolume;
					StartLoad(_playNextName);
				}
			}
			if (_enableStreaming && _source.clip != null && !_source.isPlaying)
			{
				_source.time = 0f;
				_source.Play();
			}
		}
	}

	private void StartLoad(string name)
	{
		if (_source.clip != null)
		{
			Unload();
		}
		if (_audioLoader != null)
		{
			_audioLoader.Dispose();
			_audioLoader = null;
		}
		if (AssetsManager.HasObject("Music/" + name))
		{
			AudioClip audioClip = AssetsManager.GetAudioClip("Music/" + name);
			_loadingClip = audioClip;
			_isLoading = true;
		}
		else
		{
			StartWWW(name);
			_isLoading = true;
		}
		_lastPlayTime = Time.time;
		_playScheduled = false;
	}

	private void ContinueLoad()
	{
		_lastPlayTime = Time.time;
		if (!_isEnabled)
		{
			_isLoading = false;
			ClearLoader();
		}
		else if (IsLoadComplete())
		{
			_current = _loadingName;
			AssignLoadedClip();
			Play(_loadingVolume);
			_isLoading = false;
			ClearLoader();
		}
	}

	private void StartWWW(string name)
	{
		string url = FileUrlForName(name);
		_audioLoader = new WWW(url);
	}

	private bool IsLoadComplete()
	{
		if (_audioLoader == null)
		{
			return true;
		}
		if (!_audioLoader.isDone)
		{
			return false;
		}
		if (_loadingClip == null)
		{
			if (_enableStreaming)
			{
				_loadingClip = _audioLoader.GetAudioClip(threeD: false, stream: true);
			}
			else
			{
				_loadingClip = _audioLoader.GetAudioClipCompressed();
			}
			return false;
		}
		return AudioDataLoadState.Loaded == _loadingClip.loadState;
	}

	private void AssignLoadedClip()
	{
		_source.clip = _loadingClip;
	}

	private void ClearLoader()
	{
		_audioLoader = null;
		_loadingClip = null;
		Resources.UnloadUnusedAssets();
	}

	internal void BWApplicationPause(bool pauseStatus)
	{
		if (_isEnabled && !(_source.clip == null) && !pauseStatus)
		{
			_source.Stop();
			_source.time = 0f;
			_source.Play();
		}
	}

	private IEnumerator RunTests()
	{
		yield return new WaitForSeconds(1f);
		SetEnabled(enabled: true);
		yield return new WaitForSeconds(1f);
		SetMusic("MusicBackground");
		yield return new WaitForSeconds(4f);
		SetEnabled(enabled: false);
		yield return new WaitForSeconds(1f);
		SetMusic("MusicFantasy");
		yield return new WaitForSeconds(1f);
		SetEnabled(enabled: true);
		yield return new WaitForSeconds(4f);
		Stop();
	}

	public void RefreshVolumeFromSettings()
	{
		_musicVolume = PlayerPrefs.GetFloat(UISceneSettings.inGameMusicVolumePrefLabel, 1f);
		SetVolume(_currentVolume);
	}
}
