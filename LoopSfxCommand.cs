using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x0200013E RID: 318
public class LoopSfxCommand : Command
{
	// Token: 0x0600144F RID: 5199 RVA: 0x0008E8F8 File Offset: 0x0008CCF8
	public void BlockPlaysLoop(Block b, string sfxName, float targetVolume = 1f, float pitch = 1f)
	{
		Dictionary<string, LoopSfxCommand.BlockLoopInfo> dictionary;
		if (LoopSfxCommand.blockLoopInfos.TryGetValue(b, out dictionary))
		{
			LoopSfxCommand.BlockLoopInfo blockLoopInfo;
			if (dictionary.TryGetValue(sfxName, out blockLoopInfo))
			{
				blockLoopInfo.targetVolume = targetVolume;
				blockLoopInfo.lastPlayTime = Time.fixedTime;
				blockLoopInfo.targetPitch = pitch;
			}
			else
			{
				blockLoopInfo = new LoopSfxCommand.BlockLoopInfo
				{
					sfxName = sfxName,
					volume = 0f,
					targetVolume = targetVolume,
					lastPlayTime = Time.fixedTime,
					targetPitch = pitch,
					pitch = pitch,
					block = b
				};
				LoopSfxCommand.loopInfos.Add(blockLoopInfo);
				blockLoopInfo.Create(b);
				dictionary[sfxName] = blockLoopInfo;
			}
		}
		else
		{
			dictionary = new Dictionary<string, LoopSfxCommand.BlockLoopInfo>();
			LoopSfxCommand.blockLoopInfos[b] = dictionary;
			LoopSfxCommand.BlockLoopInfo blockLoopInfo = new LoopSfxCommand.BlockLoopInfo
			{
				sfxName = sfxName,
				volume = 0f,
				targetVolume = targetVolume,
				lastPlayTime = Time.fixedTime,
				targetPitch = pitch,
				pitch = pitch,
				block = b
			};
			dictionary[sfxName] = blockLoopInfo;
			blockLoopInfo.Create(b);
			LoopSfxCommand.blockLoopInfos[b] = dictionary;
			LoopSfxCommand.loopInfos.Add(blockLoopInfo);
		}
	}

	// Token: 0x06001450 RID: 5200 RVA: 0x0008EA24 File Offset: 0x0008CE24
	public override void Execute()
	{
		for (int i = LoopSfxCommand.loopInfos.Count - 1; i >= 0; i--)
		{
			LoopSfxCommand.BlockLoopInfo blockLoopInfo = LoopSfxCommand.loopInfos[i];
			blockLoopInfo.Update();
			if (blockLoopInfo.go == null)
			{
				LoopSfxCommand.loopInfos.RemoveAt(i);
				LoopSfxCommand.blockLoopInfos[blockLoopInfo.block].Remove(blockLoopInfo.sfxName);
				if (LoopSfxCommand.blockLoopInfos[blockLoopInfo.block].Count == 0)
				{
					LoopSfxCommand.blockLoopInfos.Remove(blockLoopInfo.block);
				}
			}
		}
		this.done = (LoopSfxCommand.loopInfos.Count == 0);
	}

	// Token: 0x06001451 RID: 5201 RVA: 0x0008EAD8 File Offset: 0x0008CED8
	public override void Removed()
	{
		foreach (LoopSfxCommand.BlockLoopInfo blockLoopInfo in LoopSfxCommand.loopInfos)
		{
			blockLoopInfo.Destroy();
		}
		LoopSfxCommand.loopInfos.Clear();
		LoopSfxCommand.blockLoopInfos.Clear();
	}

	// Token: 0x04001005 RID: 4101
	private static Dictionary<Block, Dictionary<string, LoopSfxCommand.BlockLoopInfo>> blockLoopInfos = new Dictionary<Block, Dictionary<string, LoopSfxCommand.BlockLoopInfo>>();

	// Token: 0x04001006 RID: 4102
	private static List<LoopSfxCommand.BlockLoopInfo> loopInfos = new List<LoopSfxCommand.BlockLoopInfo>();

	// Token: 0x0200013F RID: 319
	private class BlockLoopInfo
	{
		// Token: 0x06001454 RID: 5204 RVA: 0x0008EB88 File Offset: 0x0008CF88
		private void UpdateWithinWater()
		{
			this.counter++;
			if (this.counter % 7 == 0)
			{
				if (BlockAbstractWater.CameraWithinAnyWater() || BlockWater.BlockWithinWater(this.block, false))
				{
					if (this.lpFilter == null)
					{
						this.lpFilter = this.go.AddComponent<AudioLowPassFilter>();
					}
					if (!this.lpFilter.enabled)
					{
						this.lpFilter.enabled = true;
					}
					this.lpFilter.cutoffFrequency = 600f;
				}
				else if (this.lpFilter != null)
				{
					if (this.lpFilter.enabled)
					{
						this.lpFilter.enabled = false;
					}
					this.lpFilter.cutoffFrequency = 20000f;
				}
			}
		}

		// Token: 0x06001455 RID: 5205 RVA: 0x0008EC5C File Offset: 0x0008D05C
		public void Create(Block b)
		{
			GameObject gameObject = b.go;
			if (gameObject != null)
			{
				this.go = new GameObject(gameObject.name + " Loop " + this.sfxName);
				this.go.transform.parent = gameObject.transform;
				this.go.transform.localPosition = Vector3.zero;
				this.source = this.go.AddComponent<AudioSource>();
				this.source.volume = this.volume;
				this.source.loop = true;
				this.source.pitch = this.pitch;
				this.source.clip = Sound.GetSfx(this.sfxName);
				this.source.Play();
				Sound.SetWorldAudioSourceParams(this.source, 5f, 150f, AudioRolloffMode.Logarithmic);
				this.UpdateWithinWater();
			}
		}

		// Token: 0x06001456 RID: 5206 RVA: 0x0008ED44 File Offset: 0x0008D144
		public void Destroy()
		{
			if (this.go != null)
			{
				UnityEngine.Object.Destroy(this.go);
				this.go = null;
			}
		}

		// Token: 0x06001457 RID: 5207 RVA: 0x0008ED6C File Offset: 0x0008D16C
		public void Update()
		{
			if (this.go != null)
			{
				this.UpdateWithinWater();
				float f = this.targetPitch - this.pitch;
				if (Mathf.Abs(f) > 0.01f)
				{
					this.source.pitch = this.targetPitch;
					this.pitch = this.targetPitch;
				}
				float fixedTime = Time.fixedTime;
				bool flag = false;
				if (this.lastPlayTime < fixedTime - Blocksworld.fixedDeltaTime)
				{
					this.volume = Mathf.Max(this.volume - 0.01f, 0f);
					if (this.volume <= 0f)
					{
						if (this.zeroVolumeCounter > 15)
						{
							this.source.Stop();
							this.Destroy();
						}
						else
						{
							flag = true;
							this.zeroVolumeCounter++;
						}
					}
					else
					{
						flag = true;
					}
				}
				else
				{
					this.zeroVolumeCounter = 0;
					this.volume += 0.02f;
					if (this.volume > this.targetVolume)
					{
						this.volume = this.targetVolume;
					}
					flag = true;
				}
				if (flag)
				{
					this.source.volume = this.volume;
				}
			}
		}

		// Token: 0x04001007 RID: 4103
		public string sfxName;

		// Token: 0x04001008 RID: 4104
		public float volume;

		// Token: 0x04001009 RID: 4105
		public float targetVolume;

		// Token: 0x0400100A RID: 4106
		public float lastPlayTime = -1f;

		// Token: 0x0400100B RID: 4107
		public float pitch = 1f;

		// Token: 0x0400100C RID: 4108
		public float targetPitch = 1f;

		// Token: 0x0400100D RID: 4109
		public GameObject go;

		// Token: 0x0400100E RID: 4110
		public AudioSource source;

		// Token: 0x0400100F RID: 4111
		public Block block;

		// Token: 0x04001010 RID: 4112
		public AudioLowPassFilter lpFilter;

		// Token: 0x04001011 RID: 4113
		private int counter;

		// Token: 0x04001012 RID: 4114
		private int zeroVolumeCounter;
	}
}
