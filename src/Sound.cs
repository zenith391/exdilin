using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x0200028F RID: 655
public class Sound
{
	// Token: 0x06001E95 RID: 7829 RVA: 0x000DB10C File Offset: 0x000D950C
	public static bool GetRandomSfx(ref string sfxName)
	{
		Sound.RandomSfxInfo randomSfxInfo;
		if (Sound.randomSfxs.TryGetValue(sfxName, out randomSfxInfo))
		{
			sfxName = randomSfxInfo.Sample();
			return true;
		}
		return false;
	}

	// Token: 0x06001E96 RID: 7830 RVA: 0x000DB138 File Offset: 0x000D9538
	public static float GetDurationalSfxVolume(string sfxName)
	{
		float result;
		if (Sound.durationalSfxVolumes.TryGetValue(sfxName, out result))
		{
			return result;
		}
		return 1f;
	}

	// Token: 0x06001E97 RID: 7831 RVA: 0x000DB160 File Offset: 0x000D9560
	public static void PlayPositionedOneShot(string sfxName, Vector3 position, float volume = 1f, float minDistance = 5f, float maxDistance = 150f, AudioRolloffMode mode = AudioRolloffMode.Logarithmic)
	{
		if (!Sound.sfxEnabled)
		{
			return;
		}
		AudioClip audioClip = Sound.GetSfx(sfxName);
		if (audioClip != null)
		{
			GameObject gameObject = new GameObject(sfxName);
			gameObject.transform.position = position;
			AudioSource audioSource = gameObject.AddComponent<AudioSource>();
			audioSource.volume = volume * Sound.sfxVolume;
			Sound.SetWorldAudioSourceParams(audioSource, minDistance, maxDistance, mode);
			Sound.PlaySound(sfxName, audioSource, true, 1f, 1f, false);
			UnityEngine.Object.Destroy(gameObject, audioClip.length + 0.1f);
		}
	}

	// Token: 0x06001E98 RID: 7832 RVA: 0x000DB1E2 File Offset: 0x000D95E2
	public static void SetWorldAudioSourceParams(AudioSource aus, float minDistance = 5f, float maxDistance = 150f, AudioRolloffMode mode = AudioRolloffMode.Logarithmic)
	{
		aus.minDistance = minDistance;
		aus.maxDistance = maxDistance;
		aus.rolloffMode = mode;
		aus.spatialBlend = 1f;
	}

	// Token: 0x06001E99 RID: 7833 RVA: 0x000DB204 File Offset: 0x000D9604
	public static HashSet<Block> GetDurationalSoundSources(string sfxName)
	{
		HashSet<Block> result;
		if (Sound.durationalSoundSources.TryGetValue(sfxName, out result))
		{
			return result;
		}
		return null;
	}

	// Token: 0x06001E9A RID: 7834 RVA: 0x000DB228 File Offset: 0x000D9628
	public static void AddDurationalSoundSource(string sfxName, Block block)
	{
		HashSet<Block> hashSet;
		if (Sound.durationalSoundSources.TryGetValue(sfxName, out hashSet))
		{
			hashSet.Add(block);
		}
		else
		{
			hashSet = new HashSet<Block>();
			hashSet.Add(block);
			Sound.durationalSoundSources[sfxName] = hashSet;
		}
		Sound.durationalSoundBlockIDs.Add(block.GetInstanceId());
	}

	// Token: 0x06001E9B RID: 7835 RVA: 0x000DB280 File Offset: 0x000D9680
	public static void RemoveAllSfxs()
	{
		foreach (string name in new List<string>(Sound.sfx.Keys))
		{
			Sound.RemoveSfx(name);
		}
	}

	// Token: 0x06001E9C RID: 7836 RVA: 0x000DB2E4 File Offset: 0x000D96E4
	public static void RemoveSfx(string name)
	{
		if (Sound.sfx.ContainsKey(name))
		{
			AudioClip audioClip = Sound.sfx[name];
			if (audioClip != null)
			{
				Resources.UnloadAsset(audioClip);
				Sound.sfx.Remove(name);
				Sound.sfxsTestedLoaded.Remove(name);
			}
		}
	}

	// Token: 0x06001E9D RID: 7837 RVA: 0x000DB337 File Offset: 0x000D9737
	public static void AddSfxTestedLoaded(string name)
	{
		Sound.sfxsTestedLoaded.Add(name);
		Sound.sfxsTestedLoaded.Add("SFX " + name);
	}

	// Token: 0x06001E9E RID: 7838 RVA: 0x000DB35B File Offset: 0x000D975B
	public static void AddSfx(string name, AudioClip clip)
	{
		Sound.AddSfxTestedLoaded(name);
		Sound.sfx[name] = clip;
		Sound.sfx["SFX " + name] = Sound.sfx[name];
	}

	// Token: 0x06001E9F RID: 7839 RVA: 0x000DB390 File Offset: 0x000D9790
	public static AudioClip GetSfx(string name)
	{
		if (Sound.sfx.ContainsKey(name))
		{
			return Sound.sfx[name];
		}
		if (name.StartsWith("SFX "))
		{
			name = name.Substring(4);
			if (Sound.sfx.ContainsKey(name))
			{
				Sound.AddSfx(name, Sound.sfx[name]);
				return Sound.sfx[name];
			}
		}
		if (Sound.sfxsTestedLoaded.Contains(name))
		{
			BWLog.Info("Could not find sfx '" + name + "'");
			return null;
		}
		AudioClip audioClip = Resources.Load("SFX/" + name) as AudioClip;
		if (audioClip != null)
		{
			Sound.AddSfx(name, audioClip);
		}
		else
		{
			BWLog.Info("Failed to load sfx '" + name + "'");
			Sound.AddSfxTestedLoaded(name);
		}
		return audioClip;
	}

	// Token: 0x06001EA0 RID: 7840 RVA: 0x000DB47C File Offset: 0x000D987C
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
				int index = UnityEngine.Random.Range(0, Sound.collisionSounds.Count);
				b1.PlayPositionedSound(Sound.collisionSounds[index], Mathf.Clamp(value, 0f, 1f), 0.8f + 0.4f * UnityEngine.Random.value);
			}
		}
	}

	// Token: 0x06001EA1 RID: 7841 RVA: 0x000DB560 File Offset: 0x000D9960
	public static void PlaySoundMovedSelectedBlock()
	{
		Block selectedBlock = Blocksworld.selectedBlock;
		Bunch selectedBunch = Blocksworld.selectedBunch;
		if (selectedBlock != null)
		{
			bool enabled = selectedBlock.go.GetComponent<Collider>().enabled;
			selectedBlock.EnableCollider(true);
			ConnectednessGraph.Update(selectedBlock);
			int count = selectedBlock.connections.Count;
			selectedBlock.EnableCollider(enabled);
			if (count > 0)
			{
				Sound.PlaySound("Connect", Sound.GetOrCreateOneShotAudioSource(), true, 0.5f, 1f, false);
			}
			else
			{
				Sound.PlaySound("Move", Sound.GetOrCreateOneShotAudioSource(), true, 1f, 1f, false);
			}
		}
		else if (selectedBunch != null)
		{
			Sound.PlaySound("Move", Sound.GetOrCreateOneShotAudioSource(), true, 1f, 1f, false);
		}
	}

	// Token: 0x06001EA2 RID: 7842 RVA: 0x000DB618 File Offset: 0x000D9A18
	public static AudioSource GetOrCreateOneShotAudioSource()
	{
		if (Sound.oneShotAudioSource == null)
		{
			AudioSource audioSource = Blocksworld.bw.gameObject.AddComponent<AudioSource>();
			audioSource.dopplerLevel = 0f;
			audioSource.bypassEffects = true;
			audioSource.spatialBlend = 0f;
			audioSource.spread = 0f;
			audioSource.spatialBlend = 0f;
			Sound.oneShotAudioSource = audioSource;
			Sound.RefreshVolumeFromSettings();
		}
		return Sound.oneShotAudioSource;
	}

	// Token: 0x06001EA3 RID: 7843 RVA: 0x000DB688 File Offset: 0x000D9A88
	public static void PlayCreateSound(GAF gaf, bool script = false, Block block = null)
	{
		string name = gaf.Predicate.Name;
		string text = string.Empty;
		bool oneShot = true;
		AudioSource orCreateOneShotAudioSource = Sound.GetOrCreateOneShotAudioSource();
		if (name != null)
		{
			if (!(name == "Laser.Beam"))
			{
				if (!(name == "Block.Create"))
				{
					if (!(name == "Block.PaintTo"))
					{
						if (name == "Block.TextureTo")
						{
							string text2 = (string)gaf.Args[0];
							if (text2 != null)
							{
							}
							text = ((!script) ? "Texture" : string.Empty);
						}
					}
					else
					{
						text = string.Empty;
					}
				}
				else
				{
					text = Sound.GetCreateOrDestroySound(gaf, block, "Create Block ", "Create");
				}
			}
			else
			{
				text = "Script Tile " + name;
				orCreateOneShotAudioSource = Sound.GetOrCreateOneShotAudioSource();
			}
		}
		if (script && text == string.Empty)
		{
			text = "Script Tile Generic";
		}
		if (text != string.Empty)
		{
			Sound.PlaySound(text, orCreateOneShotAudioSource, oneShot, 1f, 1f, false);
		}
	}

	// Token: 0x06001EA4 RID: 7844 RVA: 0x000DB7B0 File Offset: 0x000D9BB0
	private static string GetCreateOrDestroySound(GAF gaf, Block block, string prefix, string defaultSound)
	{
		string result = defaultSound;
		string text = (string)gaf.Args[0];
		if (block is BlockCharacter || block is BlockAnimatedCharacter)
		{
			if (text.EndsWith("Skeleton"))
			{
				result = prefix + "Legs";
			}
			else if (text.EndsWith("Headless"))
			{
				result = prefix + "Headless";
			}
			else
			{
				result = prefix + "Character";
			}
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
		else if (!(block is BlockAbstractAntiGravityWing))
		{
			if (block is BlockAbstractAntiGravity)
			{
				result = prefix + "Antigravity";
			}
		}
		return result;
	}

	// Token: 0x06001EA5 RID: 7845 RVA: 0x000DB908 File Offset: 0x000D9D08
	public static void PlayDestroySound(GAF gaf, Block block)
	{
		string name = gaf.Predicate.Name;
		string text = string.Empty;
		if (name != null)
		{
			if (name == "Block.Create")
			{
				text = Sound.GetCreateOrDestroySound(gaf, block, "Destroy Block ", "Destroy");
			}
		}
		if (text != string.Empty)
		{
			Sound.PlaySound(text, Sound.GetOrCreateOneShotAudioSource(), true, 1f, 1f, false);
		}
	}

	// Token: 0x06001EA6 RID: 7846 RVA: 0x000DB980 File Offset: 0x000D9D80
	private static bool CanPlayThisSoundNow(string name)
	{
		if (Sound.minReplayTimes == null)
		{
			Sound.minReplayTimes = new Dictionary<string, float>();
			float value = 0.5f;
			float value2 = 0.25f;
			float value3 = 0.05f;
			Sound.minReplayTimes.Add("Move", value3);
			Sound.minReplayTimes.Add("Create", value2);
			Sound.minReplayTimes.Add("Create Block Laser", value2);
			Sound.minReplayTimes.Add("Create Block Rocket", value2);
			Sound.minReplayTimes.Add("Create Block Stabilizer", value2);
			Sound.minReplayTimes.Add("Create Block Legs", value2);
			Sound.minReplayTimes.Add("Create Block Character", value2);
			Sound.minReplayTimes.Add("Create Block Motor", value2);
			Sound.minReplayTimes.Add("Destroy", value2);
			Sound.minReplayTimes.Add("Destroy Block Laser", value2);
			Sound.minReplayTimes.Add("Texture", value2);
			Sound.minReplayTimes.Add("Paint", value2);
			Sound.minReplayTimes.Add("Button Generic", value2);
			Sound.minReplayTimes.Add("Button Play", value2);
			Sound.minReplayTimes.Add("Button Stop", value2);
			Sound.minReplayTimes.Add("Button Rewind", value2);
			Sound.minReplayTimes.Add("Menu", value);
			Sound.minReplayTimes.Add("Error", value2);
			Sound.minReplayTimes.Add("Connect", value2);
			Sound.minReplayTimes.Add("Whee 1", value3);
			Sound.minReplayTimes.Add("Whee 2", value3);
			Sound.minReplayTimes.Add("Ouch 1", value3);
			Sound.minReplayTimes.Add("Ouch 2", value3);
			Sound.minReplayTimes.Add("Oh 1", value3);
			Sound.minReplayTimes.Add("Oh 2", value3);
			Sound.minReplayTimes.Add("Reward Spinning", Sound.GetSfx("Reward Spinning").length);
			Sound.minReplayTimes.Add("Object Pull", 1f);
			Sound.minReplayTimes.Add("Tile Start Drag", 1f);
			Sound.sfxDefs = Blocksworld.sfxDefinitions;
			if (Sound.sfxDefs != null)
			{
				foreach (SfxDefinition sfxDefinition in Sound.sfxDefs.definitions)
				{
					Sound.sfxDefinitions[sfxDefinition.name] = sfxDefinition;
					string text = "SFX " + sfxDefinition.name;
					if (!sfxDefinition.name.StartsWith("SFX "))
					{
						Sound.sfxDefinitions[text] = sfxDefinition;
						if (sfxDefinition.duckMusicVolume)
						{
							Sound.duckMusicSfxs.Add(text);
						}
					}
					if (sfxDefinition.duckMusicVolume)
					{
						Sound.duckMusicSfxs.Add(sfxDefinition.name);
					}
					if (sfxDefinition.durationalVolume > 0.001f)
					{
						Sound.durationalSfxVolumes[sfxDefinition.name] = sfxDefinition.durationalVolume;
						Sound.durationalSfxVolumes[text] = sfxDefinition.durationalVolume;
					}
				}
				foreach (SfxVariations sfxVariations in Sound.sfxDefs.randomVariations)
				{
					Sound.RandomSfxInfo value4 = new Sound.RandomSfxInfo(sfxVariations);
					Sound.randomSfxs[sfxVariations.name] = value4;
					Sound.randomSfxs["SFX " + sfxVariations.name] = value4;
				}
			}
		}
		if (!Sound.initializedSounds.Contains(name))
		{
			if (Sound.sfxDefinitions.ContainsKey(name))
			{
				SfxDefinition sfxDefinition2 = Sound.sfxDefinitions[name];
				Sound.minReplayTimes[sfxDefinition2.name] = sfxDefinition2.shortestPlayInterval;
				AudioClip audioClip = Sound.GetSfx(sfxDefinition2.name);
				if (sfxDefinition2.useLengthForPlayInterval && audioClip != null)
				{
					Sound.minReplayTimes[sfxDefinition2.name] = audioClip.length;
				}
			}
			Sound.initializedSounds.Add(name);
		}
		if (!Sound.lastPlayTimes.ContainsKey(name))
		{
			Sound.lastPlayTimes.Add(name, Time.time);
			return true;
		}
		if (!Sound.minReplayTimes.ContainsKey(name))
		{
			return true;
		}
		float num = Sound.lastPlayTimes[name];
		float num2 = Sound.minReplayTimes[name];
		float num3 = Time.time - num;
		if (num3 > num2)
		{
			Sound.lastPlayTimes[name] = Time.time;
			return true;
		}
		return false;
	}

	// Token: 0x06001EA7 RID: 7847 RVA: 0x000DBDE8 File Offset: 0x000DA1E8
	public static void PlayOneShotSound(string name, float volume = 1f)
	{
		Sound.PlaySound(name, Sound.GetOrCreateOneShotAudioSource(), true, volume, 1f, false);
	}

	// Token: 0x06001EA8 RID: 7848 RVA: 0x000DBE00 File Offset: 0x000DA200
	public static void PlaySound(string name, AudioSource audio = null, bool oneShot = false, float volume = 1f, float pitch = 1f, bool force = false)
	{
		if (!Sound.sfxEnabled)
		{
			return;
		}
		if (audio == null)
		{
			audio = Blocksworld.bw.GetComponent<AudioSource>();
			Sound.RefreshVolumeFromSettings();
		}
		if (!Sound.CanPlayThisSoundNow(name) && !force)
		{
			return;
		}
		AudioClip audioClip = Sound.GetSfx(name);
		if (audioClip != null)
		{
			if (oneShot)
			{
				if (!audio.enabled)
				{
					audio.enabled = true;
				}
				audio.PlayOneShot(audioClip, volume * Sound.sfxVolume);
				Sound.DuckMusic(name);
			}
			else if (!audio.isPlaying)
			{
				audio.clip = audioClip;
				audio.volume = volume * Sound.sfxVolume;
				audio.pitch = pitch;
				audio.Play();
			}
			else if (audio.isPlaying)
			{
				audio.volume = volume * Sound.sfxVolume;
				audio.pitch = pitch;
			}
		}
		else
		{
			BWLog.Info("Could not find sound '" + name + "'");
		}
	}

	// Token: 0x06001EA9 RID: 7849 RVA: 0x000DBEF8 File Offset: 0x000DA2F8
	private static void DuckMusic(string sfxName)
	{
		if (Blocksworld.CurrentState == State.Play && Sound.duckMusicSfxs.Contains(sfxName))
		{
			float durationalSfxTime = Sound.GetDurationalSfxTime(sfxName);
			Sound.duckBgMusicCommand.DuckMusicVolume(durationalSfxTime);
			Blocksworld.AddFixedUpdateUniqueCommand(Sound.duckBgMusicCommand, true);
		}
	}

	// Token: 0x06001EAA RID: 7850 RVA: 0x000DBF40 File Offset: 0x000DA340
	public static float GetDurationalSfxTime(string sfxName)
	{
		if (Sound.durationalSfxTimes == null)
		{
			Sound.durationalSfxTimes = new Dictionary<string, float>();
			SfxDefinitions sfxDefinitions = Blocksworld.sfxDefinitions;
			if (sfxDefinitions != null)
			{
				foreach (SfxDefinition sfxDefinition in sfxDefinitions.definitions)
				{
					Sound.durationalSfxTimes[sfxDefinition.name] = sfxDefinition.durationalTime;
				}
			}
		}
		float num = 0.5f;
		AudioClip audioClip = Sound.GetSfx(sfxName);
		if (Sound.durationalSfxTimes.TryGetValue(sfxName, out num))
		{
			if (num <= 0f)
			{
				if (audioClip != null)
				{
					num = audioClip.length;
				}
				else
				{
					num = 0.5f;
				}
			}
		}
		else if (Sound.sfx.ContainsKey(sfxName))
		{
			num = audioClip.length;
		}
		return num;
	}

	// Token: 0x06001EAB RID: 7851 RVA: 0x000DC018 File Offset: 0x000DA418
	public static bool SfxIsVox(string sfxName)
	{
		SfxDefinition sfxDefinition;
		return Sound.sfxDefinitions.TryGetValue(sfxName, out sfxDefinition) && sfxDefinition.isVox;
	}

	// Token: 0x06001EAC RID: 7852 RVA: 0x000DC03F File Offset: 0x000DA43F
	public static bool BlockIsMuted(Block b)
	{
		return Sound.mutedBlocks.Contains(b.go.GetInstanceID());
	}

	// Token: 0x06001EAD RID: 7853 RVA: 0x000DC056 File Offset: 0x000DA456
	public static bool BlockIsMuted(int blockInstanceID)
	{
		return Sound.mutedBlocks.Contains(blockInstanceID);
	}

	// Token: 0x06001EAE RID: 7854 RVA: 0x000DC063 File Offset: 0x000DA463
	public static void MuteBlock(Block b)
	{
		Sound.mutedBlocks.Add(b.go.GetInstanceID());
	}

	// Token: 0x06001EAF RID: 7855 RVA: 0x000DC07B File Offset: 0x000DA47B
	public static void UnmuteBlock(Block b)
	{
		Sound.mutedBlocks.Remove(b.go.GetInstanceID());
	}

	// Token: 0x06001EB0 RID: 7856 RVA: 0x000DC093 File Offset: 0x000DA493
	public static void UnmuteAllBlocks()
	{
		Sound.mutedBlocks.Clear();
	}

	// Token: 0x06001EB1 RID: 7857 RVA: 0x000DC09F File Offset: 0x000DA49F
	public static void Play()
	{
		Sound.mutedBlocks.Clear();
		Sound.durationalSoundSources.Clear();
	}

	// Token: 0x06001EB2 RID: 7858 RVA: 0x000DC0B8 File Offset: 0x000DA4B8
	public static void ResetState()
	{
		if (Sound.durationalSoundSources.Count > 0)
		{
			bool flag = true;
			foreach (KeyValuePair<string, HashSet<Block>> keyValuePair in Sound.durationalSoundSources)
			{
				HashSet<Block> value = keyValuePair.Value;
				if (value.Count > 0)
				{
					flag = false;
				}
				value.Clear();
			}
			if (flag)
			{
				Sound.durationalSoundSources.Clear();
			}
		}
		Sound.durationalSoundBlockIDs.Clear();
	}

	// Token: 0x06001EB3 RID: 7859 RVA: 0x000DC154 File Offset: 0x000DA554
	public static void RefreshVolumeFromSettings()
	{
		Sound.sfxVolume = PlayerPrefs.GetFloat(UISceneSettings.inGameSFXVolumePrefLabel, 1f);
	}

	// Token: 0x040018A3 RID: 6307
	private static Dictionary<string, AudioClip> sfx = new Dictionary<string, AudioClip>();

	// Token: 0x040018A4 RID: 6308
	private static Dictionary<string, Sound.RandomSfxInfo> randomSfxs = new Dictionary<string, Sound.RandomSfxInfo>();

	// Token: 0x040018A5 RID: 6309
	private static Dictionary<string, float> durationalSfxVolumes = new Dictionary<string, float>();

	// Token: 0x040018A6 RID: 6310
	public static HashSet<string> sfxsTestedLoaded = new HashSet<string>();

	// Token: 0x040018A7 RID: 6311
	public static Dictionary<string, HashSet<Block>> durationalSoundSources = new Dictionary<string, HashSet<Block>>();

	// Token: 0x040018A8 RID: 6312
	public static Dictionary<string, string> createSfxNames = new Dictionary<string, string>();

	// Token: 0x040018A9 RID: 6313
	public static Dictionary<string, string> destroySfxNames = new Dictionary<string, string>();

	// Token: 0x040018AA RID: 6314
	public static AudioSource oneShotAudioSource = null;

	// Token: 0x040018AB RID: 6315
	public static bool sfxEnabled = true;

	// Token: 0x040018AC RID: 6316
	private static float sfxVolume = 1f;

	// Token: 0x040018AD RID: 6317
	public const float ROLL_OFF_MIN_DISTANCE = 5f;

	// Token: 0x040018AE RID: 6318
	public const float ROLL_OFF_MAX_DISTANCE = 150f;

	// Token: 0x040018AF RID: 6319
	public const AudioRolloffMode ROLL_OFF_MODE = AudioRolloffMode.Logarithmic;

	// Token: 0x040018B0 RID: 6320
	public static HashSet<string> existingSfxs = new HashSet<string>();

	// Token: 0x040018B1 RID: 6321
	public static HashSet<int> mutedBlocks = new HashSet<int>();

	// Token: 0x040018B2 RID: 6322
	public static HashSet<int> durationalSoundBlockIDs = new HashSet<int>();

	// Token: 0x040018B3 RID: 6323
	private static List<string> collisionSounds = new List<string>
	{
		"Collision Medium 1",
		"Collision Medium 2",
		"Collision Medium 3",
		"Collision Medium 4"
	};

	// Token: 0x040018B4 RID: 6324
	private static Dictionary<string, float> lastPlayTimes = new Dictionary<string, float>();

	// Token: 0x040018B5 RID: 6325
	private static Dictionary<string, float> minReplayTimes = null;

	// Token: 0x040018B6 RID: 6326
	public static HashSet<string> duckMusicSfxs = new HashSet<string>();

	// Token: 0x040018B7 RID: 6327
	private static HashSet<string> initializedSounds = new HashSet<string>();

	// Token: 0x040018B8 RID: 6328
	private static Dictionary<string, SfxDefinition> sfxDefinitions = new Dictionary<string, SfxDefinition>();

	// Token: 0x040018B9 RID: 6329
	private static SfxDefinitions sfxDefs;

	// Token: 0x040018BA RID: 6330
	private static DuckBackgroundMusicCommand duckBgMusicCommand = new DuckBackgroundMusicCommand();

	// Token: 0x040018BB RID: 6331
	private static Dictionary<string, float> durationalSfxTimes = null;

	// Token: 0x02000290 RID: 656
	public class RandomSfxInfo
	{
		// Token: 0x06001EB5 RID: 7861 RVA: 0x000DC26C File Offset: 0x000DA66C
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
				this.sfxNames.Add(sfxVariation.name);
				this.cumulativeProbs.Add(num2);
			}
		}

		// Token: 0x06001EB6 RID: 7862 RVA: 0x000DC318 File Offset: 0x000DA718
		public string Sample()
		{
			float value = UnityEngine.Random.value;
			for (int i = 0; i < this.cumulativeProbs.Count; i++)
			{
				float num = this.cumulativeProbs[i];
				if (value < num)
				{
					return this.sfxNames[i];
				}
			}
			return this.sfxNames[this.sfxNames.Count - 1];
		}

		// Token: 0x040018BC RID: 6332
		public List<string> sfxNames = new List<string>();

		// Token: 0x040018BD RID: 6333
		public List<float> cumulativeProbs = new List<float>();
	}
}
