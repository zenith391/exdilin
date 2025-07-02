using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class Sound
{
	public class RandomSfxInfo
	{
		public List<string> sfxNames = new List<string>();

		public List<float> cumulativeProbs = new List<float>();

		public RandomSfxInfo(SfxVariations variations)
		{
			float num = 0f;
			for (int i = 0; i < variations.variations.Length; i++)
			{
				num += variations.variations[i].likelihood;
			}
			float num2 = 0f;
			for (int j = 0; j < variations.variations.Length; j++)
			{
				SfxVariation sfxVariation = variations.variations[j];
				num2 += sfxVariation.likelihood / num;
				sfxNames.Add(sfxVariation.name);
				cumulativeProbs.Add(num2);
			}
		}

		public string Sample()
		{
			float value = Random.value;
			for (int i = 0; i < cumulativeProbs.Count; i++)
			{
				float num = cumulativeProbs[i];
				if (value < num)
				{
					return sfxNames[i];
				}
			}
			return sfxNames[sfxNames.Count - 1];
		}
	}

	private static Dictionary<string, AudioClip> sfx;

	private static Dictionary<string, RandomSfxInfo> randomSfxs;

	private static Dictionary<string, float> durationalSfxVolumes;

	public static HashSet<string> sfxsTestedLoaded;

	public static Dictionary<string, HashSet<Block>> durationalSoundSources;

	public static Dictionary<string, string> createSfxNames;

	public static Dictionary<string, string> destroySfxNames;

	public static AudioSource oneShotAudioSource;

	public static bool sfxEnabled;

	private static float sfxVolume;

	public const float ROLL_OFF_MIN_DISTANCE = 5f;

	public const float ROLL_OFF_MAX_DISTANCE = 150f;

	public const AudioRolloffMode ROLL_OFF_MODE = AudioRolloffMode.Logarithmic;

	public static HashSet<string> existingSfxs;

	public static HashSet<int> mutedBlocks;

	public static HashSet<int> durationalSoundBlockIDs;

	private static List<string> collisionSounds;

	private static Dictionary<string, float> lastPlayTimes;

	private static Dictionary<string, float> minReplayTimes;

	public static HashSet<string> duckMusicSfxs;

	private static HashSet<string> initializedSounds;

	private static Dictionary<string, SfxDefinition> sfxDefinitions;

	private static SfxDefinitions sfxDefs;

	private static DuckBackgroundMusicCommand duckBgMusicCommand;

	private static Dictionary<string, float> durationalSfxTimes;

	public static bool GetRandomSfx(ref string sfxName)
	{
		if (randomSfxs.TryGetValue(sfxName, out var value))
		{
			sfxName = value.Sample();
			return true;
		}
		return false;
	}

	public static float GetDurationalSfxVolume(string sfxName)
	{
		if (durationalSfxVolumes.TryGetValue(sfxName, out var value))
		{
			return value;
		}
		return 1f;
	}

	public static void PlayPositionedOneShot(string sfxName, Vector3 position, float volume = 1f, float minDistance = 5f, float maxDistance = 150f, AudioRolloffMode mode = AudioRolloffMode.Logarithmic)
	{
		if (sfxEnabled)
		{
			AudioClip audioClip = GetSfx(sfxName);
			if (audioClip != null)
			{
				GameObject gameObject = new GameObject(sfxName);
				gameObject.transform.position = position;
				AudioSource audioSource = gameObject.AddComponent<AudioSource>();
				audioSource.volume = volume * sfxVolume;
				SetWorldAudioSourceParams(audioSource, minDistance, maxDistance, mode);
				PlaySound(sfxName, audioSource, oneShot: true);
				Object.Destroy(gameObject, audioClip.length + 0.1f);
			}
		}
	}

	public static void SetWorldAudioSourceParams(AudioSource aus, float minDistance = 5f, float maxDistance = 150f, AudioRolloffMode mode = AudioRolloffMode.Logarithmic)
	{
		aus.minDistance = minDistance;
		aus.maxDistance = maxDistance;
		aus.rolloffMode = mode;
		aus.spatialBlend = 1f;
	}

	public static HashSet<Block> GetDurationalSoundSources(string sfxName)
	{
		if (durationalSoundSources.TryGetValue(sfxName, out var value))
		{
			return value;
		}
		return null;
	}

	public static void AddDurationalSoundSource(string sfxName, Block block)
	{
		if (durationalSoundSources.TryGetValue(sfxName, out var value))
		{
			value.Add(block);
		}
		else
		{
			value = new HashSet<Block>();
			value.Add(block);
			durationalSoundSources[sfxName] = value;
		}
		durationalSoundBlockIDs.Add(block.GetInstanceId());
	}

	public static void RemoveAllSfxs()
	{
		foreach (string item in new List<string>(sfx.Keys))
		{
			RemoveSfx(item);
		}
	}

	public static void RemoveSfx(string name)
	{
		if (sfx.ContainsKey(name))
		{
			AudioClip audioClip = sfx[name];
			if (audioClip != null)
			{
				Resources.UnloadAsset(audioClip);
				sfx.Remove(name);
				sfxsTestedLoaded.Remove(name);
			}
		}
	}

	public static void AddSfxTestedLoaded(string name)
	{
		sfxsTestedLoaded.Add(name);
		sfxsTestedLoaded.Add("SFX " + name);
	}

	public static void AddSfx(string name, AudioClip clip)
	{
		AddSfxTestedLoaded(name);
		sfx[name] = clip;
		sfx["SFX " + name] = sfx[name];
	}

	public static AudioClip GetSfx(string name)
	{
		if (sfx.ContainsKey(name))
		{
			return sfx[name];
		}
		if (name.StartsWith("SFX "))
		{
			name = name.Substring(4);
			if (sfx.ContainsKey(name))
			{
				AddSfx(name, sfx[name]);
				return sfx[name];
			}
		}
		if (sfxsTestedLoaded.Contains(name))
		{
			BWLog.Info("Could not find sfx '" + name + "'");
			return null;
		}
		AudioClip audioClip = Resources.Load("SFX/" + name) as AudioClip;
		if (audioClip != null)
		{
			AddSfx(name, audioClip);
		}
		else
		{
			BWLog.Info("Failed to load sfx '" + name + "'");
			AddSfxTestedLoaded(name);
		}
		return audioClip;
	}

	public static void CollisionSFX(Block b1, Block b2, GameObject go1, GameObject go2, Collision collision)
	{
		Vector3 relativeVelocity = collision.relativeVelocity;
		float magnitude = relativeVelocity.magnitude;
		if (magnitude > 4f)
		{
			bool flag = true;
			if (b1 is BlockLegs)
			{
				BlockLegs blockLegs = (BlockLegs)b1;
				blockLegs.Collided(relativeVelocity);
			}
			else if (b2 is BlockLegs)
			{
				BlockLegs blockLegs2 = (BlockLegs)b2;
				blockLegs2.Collided(relativeVelocity);
			}
			else if (b1 is INoCollisionSound || b2 is INoCollisionSound)
			{
				flag = false;
			}
			if (flag && b1 != null)
			{
				float value = (magnitude - 3f) / 20f;
				int index = Random.Range(0, collisionSounds.Count);
				b1.PlayPositionedSound(collisionSounds[index], Mathf.Clamp(value, 0f, 1f), 0.8f + 0.4f * Random.value);
			}
		}
	}

	public static void PlaySoundMovedSelectedBlock()
	{
		Block selectedBlock = Blocksworld.selectedBlock;
		Bunch selectedBunch = Blocksworld.selectedBunch;
		if (selectedBlock != null)
		{
			bool enabled = selectedBlock.go.GetComponent<Collider>().enabled;
			selectedBlock.EnableCollider(value: true);
			ConnectednessGraph.Update(selectedBlock);
			int count = selectedBlock.connections.Count;
			selectedBlock.EnableCollider(enabled);
			if (count > 0)
			{
				PlaySound("Connect", GetOrCreateOneShotAudioSource(), oneShot: true, 0.5f);
			}
			else
			{
				PlaySound("Move", GetOrCreateOneShotAudioSource(), oneShot: true);
			}
		}
		else if (selectedBunch != null)
		{
			PlaySound("Move", GetOrCreateOneShotAudioSource(), oneShot: true);
		}
	}

	public static AudioSource GetOrCreateOneShotAudioSource()
	{
		if (oneShotAudioSource == null)
		{
			AudioSource audioSource = Blocksworld.bw.gameObject.AddComponent<AudioSource>();
			audioSource.dopplerLevel = 0f;
			audioSource.bypassEffects = true;
			audioSource.spatialBlend = 0f;
			audioSource.spread = 0f;
			audioSource.spatialBlend = 0f;
			oneShotAudioSource = audioSource;
			RefreshVolumeFromSettings();
		}
		return oneShotAudioSource;
	}

	public static void PlayCreateSound(GAF gaf, bool script = false, Block block = null)
	{
		string name = gaf.Predicate.Name;
		string text = string.Empty;
		bool oneShot = true;
		AudioSource orCreateOneShotAudioSource = GetOrCreateOneShotAudioSource();
		switch (name)
		{
		case "Block.TextureTo":
		{
			string text2 = (string)gaf.Args[0];
			text = ((!script) ? "Texture" : string.Empty);
			break;
		}
		case "Block.PaintTo":
			text = string.Empty;
			break;
		case "Block.Create":
			text = GetCreateOrDestroySound(gaf, block, "Create Block ", "Create");
			break;
		case "Laser.Beam":
			text = "Script Tile " + name;
			orCreateOneShotAudioSource = GetOrCreateOneShotAudioSource();
			break;
		}
		if (script && text == string.Empty)
		{
			text = "Script Tile Generic";
		}
		if (text != string.Empty)
		{
			PlaySound(text, orCreateOneShotAudioSource, oneShot);
		}
	}

	private static string GetCreateOrDestroySound(GAF gaf, Block block, string prefix, string defaultSound)
	{
		string result = defaultSound;
		string text = (string)gaf.Args[0];
		if (block is BlockCharacter || block is BlockAnimatedCharacter)
		{
			result = (text.EndsWith("Skeleton") ? (prefix + "Legs") : ((!text.EndsWith("Headless")) ? (prefix + "Character") : (prefix + "Headless")));
		}
		else if (block is BlockAbstractLegs)
		{
			result = prefix + "Legs";
		}
		else if (block is BlockAbstractStabilizer)
		{
			result = prefix + "Stabilizer";
		}
		else if (block is BlockAbstractWheel)
		{
			result = prefix + "Wheel";
		}
		else if (block is BlockAbstractLaser)
		{
			result = prefix + "Laser";
		}
		else if (block is BlockAbstractRocket)
		{
			result = prefix + "Rocket";
		}
		else if (block is BlockAbstractMotor)
		{
			result = prefix + "Motor";
		}
		else if (!(block is BlockAbstractAntiGravityWing) && block is BlockAbstractAntiGravity)
		{
			result = prefix + "Antigravity";
		}
		return result;
	}

	public static void PlayDestroySound(GAF gaf, Block block)
	{
		string name = gaf.Predicate.Name;
		string text = string.Empty;
		if (name != null && name == "Block.Create")
		{
			text = GetCreateOrDestroySound(gaf, block, "Destroy Block ", "Destroy");
		}
		if (text != string.Empty)
		{
			PlaySound(text, GetOrCreateOneShotAudioSource(), oneShot: true);
		}
	}

	private static bool CanPlayThisSoundNow(string name)
	{
		if (minReplayTimes == null)
		{
			minReplayTimes = new Dictionary<string, float>();
			float value = 0.5f;
			float value2 = 0.25f;
			float value3 = 0.05f;
			minReplayTimes.Add("Move", value3);
			minReplayTimes.Add("Create", value2);
			minReplayTimes.Add("Create Block Laser", value2);
			minReplayTimes.Add("Create Block Rocket", value2);
			minReplayTimes.Add("Create Block Stabilizer", value2);
			minReplayTimes.Add("Create Block Legs", value2);
			minReplayTimes.Add("Create Block Character", value2);
			minReplayTimes.Add("Create Block Motor", value2);
			minReplayTimes.Add("Destroy", value2);
			minReplayTimes.Add("Destroy Block Laser", value2);
			minReplayTimes.Add("Texture", value2);
			minReplayTimes.Add("Paint", value2);
			minReplayTimes.Add("Button Generic", value2);
			minReplayTimes.Add("Button Play", value2);
			minReplayTimes.Add("Button Stop", value2);
			minReplayTimes.Add("Button Rewind", value2);
			minReplayTimes.Add("Menu", value);
			minReplayTimes.Add("Error", value2);
			minReplayTimes.Add("Connect", value2);
			minReplayTimes.Add("Whee 1", value3);
			minReplayTimes.Add("Whee 2", value3);
			minReplayTimes.Add("Ouch 1", value3);
			minReplayTimes.Add("Ouch 2", value3);
			minReplayTimes.Add("Oh 1", value3);
			minReplayTimes.Add("Oh 2", value3);
			minReplayTimes.Add("Reward Spinning", GetSfx("Reward Spinning").length);
			minReplayTimes.Add("Tile Start Drag", 1f);
			sfxDefs = Blocksworld.sfxDefinitions;
			if (sfxDefs != null)
			{
				SfxDefinition[] definitions = sfxDefs.definitions;
				foreach (SfxDefinition sfxDefinition in definitions)
				{
					sfxDefinitions[sfxDefinition.name] = sfxDefinition;
					string text = "SFX " + sfxDefinition.name;
					if (!sfxDefinition.name.StartsWith("SFX "))
					{
						sfxDefinitions[text] = sfxDefinition;
						if (sfxDefinition.duckMusicVolume)
						{
							duckMusicSfxs.Add(text);
						}
					}
					if (sfxDefinition.duckMusicVolume)
					{
						duckMusicSfxs.Add(sfxDefinition.name);
					}
					if (sfxDefinition.durationalVolume > 0.001f)
					{
						durationalSfxVolumes[sfxDefinition.name] = sfxDefinition.durationalVolume;
						durationalSfxVolumes[text] = sfxDefinition.durationalVolume;
					}
				}
				SfxVariations[] randomVariations = sfxDefs.randomVariations;
				foreach (SfxVariations sfxVariations in randomVariations)
				{
					RandomSfxInfo value4 = new RandomSfxInfo(sfxVariations);
					randomSfxs[sfxVariations.name] = value4;
					randomSfxs["SFX " + sfxVariations.name] = value4;
				}
			}
		}
		if (!initializedSounds.Contains(name))
		{
			if (sfxDefinitions.ContainsKey(name))
			{
				SfxDefinition sfxDefinition2 = sfxDefinitions[name];
				minReplayTimes[sfxDefinition2.name] = sfxDefinition2.shortestPlayInterval;
				AudioClip audioClip = GetSfx(sfxDefinition2.name);
				if (sfxDefinition2.useLengthForPlayInterval && audioClip != null)
				{
					minReplayTimes[sfxDefinition2.name] = audioClip.length;
				}
			}
			initializedSounds.Add(name);
		}
		if (!lastPlayTimes.ContainsKey(name))
		{
			lastPlayTimes.Add(name, Time.time);
			return true;
		}
		if (!minReplayTimes.ContainsKey(name))
		{
			return true;
		}
		float num = lastPlayTimes[name];
		float num2 = minReplayTimes[name];
		if (Time.time - num > num2)
		{
			lastPlayTimes[name] = Time.time;
			return true;
		}
		return false;
	}

	public static void PlayOneShotSound(string name, float volume = 1f)
	{
		PlaySound(name, GetOrCreateOneShotAudioSource(), oneShot: true, volume);
	}

	public static void PlaySound(string name, AudioSource audio = null, bool oneShot = false, float volume = 1f, float pitch = 1f, bool force = false)
	{
		if (!sfxEnabled)
		{
			return;
		}
		if (audio == null)
		{
			audio = Blocksworld.bw.GetComponent<AudioSource>();
			RefreshVolumeFromSettings();
		}
		if (!CanPlayThisSoundNow(name) && !force)
		{
			return;
		}
		AudioClip audioClip = GetSfx(name);
		if (audioClip != null)
		{
			if (oneShot)
			{
				if (!audio.enabled)
				{
					audio.enabled = true;
				}
				audio.PlayOneShot(audioClip, volume * sfxVolume);
				DuckMusic(name);
			}
			else if (!audio.isPlaying)
			{
				audio.clip = audioClip;
				audio.volume = volume * sfxVolume;
				audio.pitch = pitch;
				audio.Play();
			}
			else if (audio.isPlaying)
			{
				audio.volume = volume * sfxVolume;
				audio.pitch = pitch;
			}
		}
		else
		{
			BWLog.Info("Could not find sound '" + name + "'");
		}
	}

	private static void DuckMusic(string sfxName)
	{
		if (Blocksworld.CurrentState == State.Play && duckMusicSfxs.Contains(sfxName))
		{
			float durationalSfxTime = GetDurationalSfxTime(sfxName);
			duckBgMusicCommand.DuckMusicVolume(durationalSfxTime);
			Blocksworld.AddFixedUpdateUniqueCommand(duckBgMusicCommand);
		}
	}

	public static float GetDurationalSfxTime(string sfxName)
	{
		if (durationalSfxTimes == null)
		{
			durationalSfxTimes = new Dictionary<string, float>();
			SfxDefinitions sfxDefinitions = Blocksworld.sfxDefinitions;
			if (sfxDefinitions != null)
			{
				SfxDefinition[] definitions = sfxDefinitions.definitions;
				foreach (SfxDefinition sfxDefinition in definitions)
				{
					durationalSfxTimes[sfxDefinition.name] = sfxDefinition.durationalTime;
				}
			}
		}
		float value = 0.5f;
		AudioClip audioClip = GetSfx(sfxName);
		if (durationalSfxTimes.TryGetValue(sfxName, out value))
		{
			if (value <= 0f)
			{
				value = ((!(audioClip != null)) ? 0.5f : audioClip.length);
			}
		}
		else if (sfx.ContainsKey(sfxName))
		{
			value = audioClip.length;
		}
		return value;
	}

	public static bool SfxIsVox(string sfxName)
	{
		if (sfxDefinitions.TryGetValue(sfxName, out var value))
		{
			return value.isVox;
		}
		return false;
	}

	public static bool BlockIsMuted(Block b)
	{
		return mutedBlocks.Contains(b.go.GetInstanceID());
	}

	public static bool BlockIsMuted(int blockInstanceID)
	{
		return mutedBlocks.Contains(blockInstanceID);
	}

	public static void MuteBlock(Block b)
	{
		mutedBlocks.Add(b.go.GetInstanceID());
	}

	public static void UnmuteBlock(Block b)
	{
		mutedBlocks.Remove(b.go.GetInstanceID());
	}

	public static void UnmuteAllBlocks()
	{
		mutedBlocks.Clear();
	}

	public static void Play()
	{
		mutedBlocks.Clear();
		durationalSoundSources.Clear();
	}

	public static void ResetState()
	{
		if (durationalSoundSources.Count > 0)
		{
			bool flag = true;
			foreach (KeyValuePair<string, HashSet<Block>> durationalSoundSource in durationalSoundSources)
			{
				HashSet<Block> value = durationalSoundSource.Value;
				if (value.Count > 0)
				{
					flag = false;
				}
				value.Clear();
			}
			if (flag)
			{
				durationalSoundSources.Clear();
			}
		}
		durationalSoundBlockIDs.Clear();
	}

	public static void RefreshVolumeFromSettings()
	{
		sfxVolume = PlayerPrefs.GetFloat(UISceneSettings.inGameSFXVolumePrefLabel, 1f);
	}

	static Sound()
	{
		sfx = new Dictionary<string, AudioClip>();
		randomSfxs = new Dictionary<string, RandomSfxInfo>();
		durationalSfxVolumes = new Dictionary<string, float>();
		sfxsTestedLoaded = new HashSet<string>();
		durationalSoundSources = new Dictionary<string, HashSet<Block>>();
		createSfxNames = new Dictionary<string, string>();
		destroySfxNames = new Dictionary<string, string>();
		oneShotAudioSource = null;
		sfxEnabled = true;
		sfxVolume = 1f;
		existingSfxs = new HashSet<string>();
		mutedBlocks = new HashSet<int>();
		durationalSoundBlockIDs = new HashSet<int>();
		collisionSounds = new List<string> { "Collision Medium 1", "Collision Medium 2", "Collision Medium 3", "Collision Medium 4" };
		lastPlayTimes = new Dictionary<string, float>();
		minReplayTimes = null;
		duckMusicSfxs = new HashSet<string>();
		initializedSounds = new HashSet<string>();
		sfxDefinitions = new Dictionary<string, SfxDefinition>();
		duckBgMusicCommand = new DuckBackgroundMusicCommand();
		durationalSfxTimes = null;
	}
}
