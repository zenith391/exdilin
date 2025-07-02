using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class LoopSfxCommand : Command
{
	private class BlockLoopInfo
	{
		public string sfxName;

		public float volume;

		public float targetVolume;

		public float lastPlayTime = -1f;

		public float pitch = 1f;

		public float targetPitch = 1f;

		public GameObject go;

		public AudioSource source;

		public Block block;

		public AudioLowPassFilter lpFilter;

		private int counter;

		private int zeroVolumeCounter;

		private void UpdateWithinWater()
		{
			counter++;
			if (counter % 7 != 0)
			{
				return;
			}
			if (BlockAbstractWater.CameraWithinAnyWater() || BlockWater.BlockWithinWater(block))
			{
				if (lpFilter == null)
				{
					lpFilter = go.AddComponent<AudioLowPassFilter>();
				}
				if (!lpFilter.enabled)
				{
					lpFilter.enabled = true;
				}
				lpFilter.cutoffFrequency = 600f;
			}
			else if (lpFilter != null)
			{
				if (lpFilter.enabled)
				{
					lpFilter.enabled = false;
				}
				lpFilter.cutoffFrequency = 20000f;
			}
		}

		public void Create(Block b)
		{
			GameObject gameObject = b.go;
			if (gameObject != null)
			{
				go = new GameObject(gameObject.name + " Loop " + sfxName);
				go.transform.parent = gameObject.transform;
				go.transform.localPosition = Vector3.zero;
				source = go.AddComponent<AudioSource>();
				source.volume = volume;
				source.loop = true;
				source.pitch = pitch;
				source.clip = Sound.GetSfx(sfxName);
				source.Play();
				Sound.SetWorldAudioSourceParams(source);
				UpdateWithinWater();
			}
		}

		public void Destroy()
		{
			if (go != null)
			{
				Object.Destroy(go);
				go = null;
			}
		}

		public void Update()
		{
			if (!(go != null))
			{
				return;
			}
			UpdateWithinWater();
			float f = targetPitch - pitch;
			if (Mathf.Abs(f) > 0.01f)
			{
				source.pitch = targetPitch;
				pitch = targetPitch;
			}
			float fixedTime = Time.fixedTime;
			bool flag = false;
			if (lastPlayTime < fixedTime - Blocksworld.fixedDeltaTime)
			{
				volume = Mathf.Max(volume - 0.01f, 0f);
				if (volume <= 0f)
				{
					if (zeroVolumeCounter > 15)
					{
						source.Stop();
						Destroy();
					}
					else
					{
						flag = true;
						zeroVolumeCounter++;
					}
				}
				else
				{
					flag = true;
				}
			}
			else
			{
				zeroVolumeCounter = 0;
				volume += 0.02f;
				if (volume > targetVolume)
				{
					volume = targetVolume;
				}
				flag = true;
			}
			if (flag)
			{
				source.volume = volume;
			}
		}
	}

	private static Dictionary<Block, Dictionary<string, BlockLoopInfo>> blockLoopInfos = new Dictionary<Block, Dictionary<string, BlockLoopInfo>>();

	private static List<BlockLoopInfo> loopInfos = new List<BlockLoopInfo>();

	public void BlockPlaysLoop(Block b, string sfxName, float targetVolume = 1f, float pitch = 1f)
	{
		if (blockLoopInfos.TryGetValue(b, out var value))
		{
			if (value.TryGetValue(sfxName, out var value2))
			{
				value2.targetVolume = targetVolume;
				value2.lastPlayTime = Time.fixedTime;
				value2.targetPitch = pitch;
				return;
			}
			value2 = new BlockLoopInfo
			{
				sfxName = sfxName,
				volume = 0f,
				targetVolume = targetVolume,
				lastPlayTime = Time.fixedTime,
				targetPitch = pitch,
				pitch = pitch,
				block = b
			};
			loopInfos.Add(value2);
			value2.Create(b);
			value[sfxName] = value2;
		}
		else
		{
			value = new Dictionary<string, BlockLoopInfo>();
			blockLoopInfos[b] = value;
			BlockLoopInfo blockLoopInfo = (value[sfxName] = new BlockLoopInfo
			{
				sfxName = sfxName,
				volume = 0f,
				targetVolume = targetVolume,
				lastPlayTime = Time.fixedTime,
				targetPitch = pitch,
				pitch = pitch,
				block = b
			});
			blockLoopInfo.Create(b);
			blockLoopInfos[b] = value;
			loopInfos.Add(blockLoopInfo);
		}
	}

	public override void Execute()
	{
		for (int num = loopInfos.Count - 1; num >= 0; num--)
		{
			BlockLoopInfo blockLoopInfo = loopInfos[num];
			blockLoopInfo.Update();
			if (blockLoopInfo.go == null)
			{
				loopInfos.RemoveAt(num);
				blockLoopInfos[blockLoopInfo.block].Remove(blockLoopInfo.sfxName);
				if (blockLoopInfos[blockLoopInfo.block].Count == 0)
				{
					blockLoopInfos.Remove(blockLoopInfo.block);
				}
			}
		}
		done = loopInfos.Count == 0;
	}

	public override void Removed()
	{
		foreach (BlockLoopInfo loopInfo in loopInfos)
		{
			loopInfo.Destroy();
		}
		loopInfos.Clear();
		blockLoopInfos.Clear();
	}
}
