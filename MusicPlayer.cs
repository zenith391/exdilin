using System;
using System.Collections;
using System.Text;
using UnityEngine;

// Token: 0x02000215 RID: 533
using Exdilin;
public class MusicPlayer : MonoBehaviour
{
	// Token: 0x06001A5E RID: 6750 RVA: 0x000C2110 File Offset: 0x000C0510
	public static MusicPlayer Create()
	{
		GameObject gameObject = new GameObject("MusicPlayer");
		AudioSource audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.priority = 0;
		MusicPlayer musicPlayer = gameObject.AddComponent<MusicPlayer>();
		musicPlayer.RefreshVolumeFromSettings();
		return musicPlayer;
	}

	// Token: 0x06001A5F RID: 6751 RVA: 0x000C2144 File Offset: 0x000C0544
	public string GetCurrentMusicName()
	{
		return this._current;
	}

	// Token: 0x06001A60 RID: 6752 RVA: 0x000C214C File Offset: 0x000C054C
	public void SetMusic(string name, float volume = 0.4f)
	{
		if (string.IsNullOrEmpty(name))
		{
			this.Stop();
			return;
		}
		if (name == this._current && this._source.clip != null)
		{
			this._playScheduled = false;
			this._isLoading = false;
			if (!this._source.isPlaying)
			{
				this.Resume();
			}
			return;
		}
		if (!this._isEnabled)
		{
			this._playWhenEnabled = name;
			return;
		}
		this.LoadAndPlay(name, volume);
	}

	// Token: 0x06001A61 RID: 6753 RVA: 0x000C21D2 File Offset: 0x000C05D2
	private void SetVolume(float v)
	{
		this._currentVolume = v;
		this._source.volume = this._currentVolume * this._volumeMultiplier * this._musicVolume;
	}

	// Token: 0x06001A62 RID: 6754 RVA: 0x000C21FA File Offset: 0x000C05FA
	public void SetVolumeMultiplier(float m)
	{
		this._volumeMultiplier = m;
		this.SetVolume(this._currentVolume);
	}

	// Token: 0x06001A63 RID: 6755 RVA: 0x000C2210 File Offset: 0x000C0610
	public void SetEnabled(bool enabled)
	{
		this._isEnabled = enabled;
		if (enabled)
		{
			if (this._isLoading || this._playScheduled)
			{
				return;
			}
			if (this._source.clip != null)
			{
				if (!this._source.isPlaying)
				{
					this.Play(0.4f);
				}
			}
			else if (!string.IsNullOrEmpty(this._current))
			{
				this.LoadAndPlay(this._current, 0.4f);
			}
			else if (!string.IsNullOrEmpty(this._playWhenEnabled))
			{
				this.LoadAndPlay(this._playWhenEnabled, 0.4f);
			}
		}
		else
		{
			this.Unload();
		}
	}

	// Token: 0x06001A64 RID: 6756 RVA: 0x000C22C9 File Offset: 0x000C06C9
	public void Pause()
	{
		this.FadeOut(false, null);
	}

	// Token: 0x06001A65 RID: 6757 RVA: 0x000C22D3 File Offset: 0x000C06D3
	public void Resume()
	{
		if (this._isEnabled && this._source.clip != null && !this._source.isPlaying)
		{
			this.Play(0.4f);
		}
	}

	// Token: 0x06001A66 RID: 6758 RVA: 0x000C2311 File Offset: 0x000C0711
	public void Stop()
	{
		if (this._source.isPlaying)
		{
			this.FadeOut(true, null);
		}
	}

	// Token: 0x06001A67 RID: 6759 RVA: 0x000C232C File Offset: 0x000C072C
	public void Unload()
	{
		this._isLoading = false;
		this._playScheduled = false;
		this.ClearLoader();
		if (this._source == null)
		{
			return;
		}
		if (this._source.isPlaying)
		{
			this._source.Stop();
		}
		if (this._source.clip != null)
		{
			this._source.clip = null;
		}
		this._current = null;
	}

	// Token: 0x06001A68 RID: 6760 RVA: 0x000C23A3 File Offset: 0x000C07A3
	public void StopWithCompletion(MusicPlayer.FadeCompletionHandler completion)
	{
		this.FadeOut(true, completion);
	}

	// Token: 0x06001A69 RID: 6761 RVA: 0x000C23B0 File Offset: 0x000C07B0
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

	// Token: 0x06001A6A RID: 6762 RVA: 0x000C2428 File Offset: 0x000C0828
	private string WebUrlForName(string name)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(Application.streamingAssetsPath);
		stringBuilder.Append("/Music/");
		stringBuilder.Append(name);
		stringBuilder.Append(".ogg");
		return stringBuilder.ToString();
	}

	// Token: 0x06001A6B RID: 6763 RVA: 0x000C2470 File Offset: 0x000C0870
	private void LoadAndPlay(string name, float volume)
	{
		if (this._playScheduled && this._playNextName == name)
		{
			return;
		}
		if (this._isLoading && this._loadingName == name)
		{
			return;
		}
		this._isLoading = false;
		this.ClearLoader();
		this._playNextName = name;
		this._playNextVolume = volume;
		this._nextPlayTime = Time.time + 0.75f;
		this._playScheduled = true;
	}

	// Token: 0x06001A6C RID: 6764 RVA: 0x000C24EA File Offset: 0x000C08EA
	private void Play(float vol = 0.4f)
	{
		if (this._isEnabled)
		{
			base.StartCoroutine(this.FadeIn(vol));
			this._source.Play();
		}
	}

	// Token: 0x06001A6D RID: 6765 RVA: 0x000C2510 File Offset: 0x000C0910
	private void FadeOut(bool stop, MusicPlayer.FadeCompletionHandler completion)
	{
		base.StartCoroutine(this.FadeOutWithCompletion(stop, completion));
	}

	// Token: 0x06001A6E RID: 6766 RVA: 0x000C2524 File Offset: 0x000C0924
	private IEnumerator FadeIn(float volume)
	{
		this.SetVolume(0f);
		int steps = (int)(0.2f / Time.smoothDeltaTime);
		float dV = volume / (float)steps;
		for (int i = 0; i < steps; i++)
		{
			this.SetVolume((float)i * dV);
			yield return null;
		}
		this.SetVolume(volume);
		yield break;
	}

	// Token: 0x06001A6F RID: 6767 RVA: 0x000C2548 File Offset: 0x000C0948
	private IEnumerator FadeOutWithCompletion(bool stop, MusicPlayer.FadeCompletionHandler completionHandler)
	{
		float startVolume = this._currentVolume;
		if (startVolume <= 0f || !this._source.isPlaying)
		{
			this._source.Stop();
			if (completionHandler != null)
			{
				completionHandler();
			}
			yield break;
		}
		int steps = (int)(0.2f / Time.smoothDeltaTime);
		float dV = startVolume / (float)steps;
		for (int i = 0; i < steps; i++)
		{
			this.SetVolume(startVolume - (float)i * dV);
			yield return null;
		}
		this.SetVolume(0f);
		if (stop)
		{
			this._source.Stop();
			this._source.clip = null;
		}
		else
		{
			this._source.Pause();
		}
		if (completionHandler != null)
		{
			completionHandler();
		}
		yield break;
	}

	// Token: 0x06001A70 RID: 6768 RVA: 0x000C2571 File Offset: 0x000C0971
	private void Awake()
	{
		this._source = base.GetComponent<AudioSource>();
		this._source.loop = false;
		this.SetVolume(0.4f);
	}

	// Token: 0x06001A71 RID: 6769 RVA: 0x000C2598 File Offset: 0x000C0998
	private void Update()
	{
		if (this._isLoading)
		{
			this.ContinueLoad();
			return;
		}
		if (!this._isEnabled)
		{
			return;
		}
		if (this._playScheduled && Time.time > this._nextPlayTime)
		{
			if (this._playNextName == this._current)
			{
				this.SetVolume(this._playNextVolume);
			}
			else
			{
				this._loadingName = this._playNextName;
				this._loadingVolume = this._playNextVolume;
				this.StartLoad(this._playNextName);
			}
		}
		if (this._enableStreaming && this._source.clip != null && !this._source.isPlaying)
		{
			this._source.time = 0f;
			this._source.Play();
		}
	}

	// Token: 0x06001A72 RID: 6770 RVA: 0x000C2678 File Offset: 0x000C0A78
	private void StartLoad(string name)
	{
		if (this._source.clip != null)
		{
			this.Unload();
		}
		if (this._audioLoader != null)
		{
			this._audioLoader.Dispose();
			this._audioLoader = null;
		}
		if (AssetsManager.HasObject("Music/" + name)) {
			AudioClip clip = AssetsManager.GetAudioClip("Music/" + name);
			_loadingClip = clip;
			this._isLoading = true;
		} else {
			this.StartWWW(name);
			this._isLoading = true;
		}
		this._lastPlayTime = Time.time;
		this._playScheduled = false;
	}

	// Token: 0x06001A73 RID: 6771 RVA: 0x000C26E0 File Offset: 0x000C0AE0
	private void ContinueLoad()
	{
		this._lastPlayTime = Time.time;
		if (!this._isEnabled)
		{
			this._isLoading = false;
			this.ClearLoader();
		}
		else if (this.IsLoadComplete())
		{
			this._current = this._loadingName;
			this.AssignLoadedClip();
			this.Play(this._loadingVolume);
			this._isLoading = false;
			this.ClearLoader();
		}
	}

	// Token: 0x06001A74 RID: 6772 RVA: 0x000C274C File Offset: 0x000C0B4C
	private void StartWWW(string name)
	{
		string url = this.FileUrlForName(name);
		this._audioLoader = new WWW(url);
	}

	// Token: 0x06001A75 RID: 6773 RVA: 0x000C2770 File Offset: 0x000C0B70
	private bool IsLoadComplete()
	{
		if (_audioLoader == null) {
			return true;
		}
		if (!this._audioLoader.isDone)
		{
			return false;
		}
		if (this._loadingClip == null)
		{
			if (this._enableStreaming)
			{
				this._loadingClip = this._audioLoader.GetAudioClip(false, true);
			}
			else
			{
				this._loadingClip = this._audioLoader.GetAudioClipCompressed();
			}
			return false;
		}
		return AudioDataLoadState.Loaded == this._loadingClip.loadState;
	}

	// Token: 0x06001A76 RID: 6774 RVA: 0x000C27E4 File Offset: 0x000C0BE4
	private void AssignLoadedClip()
	{
		this._source.clip = this._loadingClip;
	}

	// Token: 0x06001A77 RID: 6775 RVA: 0x000C27F7 File Offset: 0x000C0BF7
	private void ClearLoader()
	{
		this._audioLoader = null;
		this._loadingClip = null;
		Resources.UnloadUnusedAssets();
	}

	// Token: 0x06001A78 RID: 6776 RVA: 0x000C2810 File Offset: 0x000C0C10
	internal void BWApplicationPause(bool pauseStatus)
	{
		if (!this._isEnabled || this._source.clip == null)
		{
			return;
		}
		if (!pauseStatus)
		{
			this._source.Stop();
			this._source.time = 0f;
			this._source.Play();
		}
	}

	// Token: 0x06001A79 RID: 6777 RVA: 0x000C286C File Offset: 0x000C0C6C
	private IEnumerator RunTests()
	{
		yield return new WaitForSeconds(1f);
		this.SetEnabled(true);
		yield return new WaitForSeconds(1f);
		this.SetMusic("MusicBackground", 0.4f);
		yield return new WaitForSeconds(4f);
		this.SetEnabled(false);
		yield return new WaitForSeconds(1f);
		this.SetMusic("MusicFantasy", 0.4f);
		yield return new WaitForSeconds(1f);
		this.SetEnabled(true);
		yield return new WaitForSeconds(4f);
		this.Stop();
		yield break;
	}

	// Token: 0x06001A7A RID: 6778 RVA: 0x000C2887 File Offset: 0x000C0C87
	public void RefreshVolumeFromSettings()
	{
		this._musicVolume = PlayerPrefs.GetFloat(UISceneSettings.inGameMusicVolumePrefLabel, 1f);
		this.SetVolume(this._currentVolume);
	}

	// Token: 0x040015FB RID: 5627
	public const float DefaultVolume = 0.4f;

	// Token: 0x040015FC RID: 5628
	private const float FadeTime = 0.2f;

	// Token: 0x040015FD RID: 5629
	private const float MinPlayFrequency = 0.75f;

	// Token: 0x040015FE RID: 5630
	private AudioSource _source;

	// Token: 0x040015FF RID: 5631
	private string _current;

	// Token: 0x04001600 RID: 5632
	private bool _isEnabled = true;

	// Token: 0x04001601 RID: 5633
	private float _volumeMultiplier = 1f;

	// Token: 0x04001602 RID: 5634
	private float _currentVolume = 1f;

	// Token: 0x04001603 RID: 5635
	private bool _isLoading;

	// Token: 0x04001604 RID: 5636
	private string _playWhenEnabled;

	// Token: 0x04001605 RID: 5637
	private string _loadingName;

	// Token: 0x04001606 RID: 5638
	private float _loadingVolume;

	// Token: 0x04001607 RID: 5639
	private bool _playScheduled;

	// Token: 0x04001608 RID: 5640
	private float _playNextVolume;

	// Token: 0x04001609 RID: 5641
	private string _playNextName;

	// Token: 0x0400160A RID: 5642
	private float _lastPlayTime;

	// Token: 0x0400160B RID: 5643
	private float _nextPlayTime;

	// Token: 0x0400160C RID: 5644
	private WWW _audioLoader;

	// Token: 0x0400160D RID: 5645
	private AudioClip _loadingClip;

	// Token: 0x0400160E RID: 5646
	private bool _enableStreaming = true;

	// Token: 0x0400160F RID: 5647
	private float _musicVolume = 1f;

	// Token: 0x02000216 RID: 534
	// (Invoke) Token: 0x06001A7C RID: 6780
	public delegate void FadeCompletionHandler();
}
