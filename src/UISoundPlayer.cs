using System.Collections.Generic;
using UnityEngine;

public class UISoundPlayer : MonoBehaviour
{
	private static UISoundPlayer instance;

	private static int sourceCount = 4;

	private AudioSource[] audioSources;

	private Dictionary<string, AudioClip> clipLibrary;

	private int sourceIndex;

	public static UISoundPlayer Instance
	{
		get
		{
			if (instance == null)
			{
				instance = CreateInstance();
			}
			return instance;
		}
	}

	private static UISoundPlayer CreateInstance()
	{
		GameObject gameObject = new GameObject("UISoundPlayer");
		Object.DontDestroyOnLoad(gameObject);
		UISoundPlayer uISoundPlayer = gameObject.AddComponent<UISoundPlayer>();
		uISoundPlayer.audioSources = new AudioSource[sourceCount];
		for (int i = 0; i < sourceCount; i++)
		{
			uISoundPlayer.audioSources[i] = gameObject.AddComponent<AudioSource>();
		}
		uISoundPlayer.RefreshVolumeFromSettings();
		return uISoundPlayer;
	}

	public void PlayClip(string clipName, float volume = 1f)
	{
		AudioClip clip = GetClip(clipName);
		if (clip != null)
		{
			audioSources[sourceIndex].PlayOneShot(clip, volume);
			sourceIndex = (sourceIndex + 1) % sourceCount;
		}
	}

	private AudioClip GetClip(string clipName)
	{
		if (clipLibrary == null)
		{
			clipLibrary = new Dictionary<string, AudioClip>();
		}
		AudioClip value = null;
		if (!clipLibrary.TryGetValue(clipName, out value))
		{
			value = Resources.Load<AudioClip>("UISounds/" + clipName);
			clipLibrary.Add(clipName, value);
		}
		return value;
	}

	public void RefreshVolumeFromSettings()
	{
		float volume = PlayerPrefs.GetFloat(UISceneSettings.uiSFXVolumePrefLabel, 1f);
		for (int i = 0; i < sourceCount; i++)
		{
			audioSources[i].volume = volume;
		}
	}
}
