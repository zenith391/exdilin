using System;

[Serializable]
public class SfxDefinition
{
	public string name;

	public string label;

	public float shortestPlayInterval;

	public bool useLengthForPlayInterval;

	public float durationalTime;

	public bool isVox;

	public bool duckMusicVolume;

	public float durationalVolume = 1f;
}
