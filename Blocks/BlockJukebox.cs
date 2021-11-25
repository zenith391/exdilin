using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x0200009A RID: 154
	public class BlockJukebox : Block
	{
		// Token: 0x06000C5D RID: 3165 RVA: 0x0005795E File Offset: 0x00055D5E
		public BlockJukebox(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000C5E RID: 3166 RVA: 0x0005797D File Offset: 0x00055D7D
		public override void Play()
		{
			this.startDistortionLevel = -1f;
		}

		// Token: 0x06000C5F RID: 3167 RVA: 0x0005798C File Offset: 0x00055D8C
		public new static void Register()
		{
			PredicateRegistry.Add<BlockJukebox>("Jukebox.SetMusic", (Block b) => new PredicateSensorDelegate(((BlockJukebox)b).MusicSetTo), (Block b) => new PredicateActionDelegate(((BlockJukebox)b).SetMusic), new Type[]
			{
				typeof(string),
				typeof(float)
			}, new string[]
			{
				"Name",
				"Volume"
			}, null);
			PredicateRegistry.Add<BlockJukebox>("Jukebox.SetMusicHighpassWithHeightSettings", null, (Block b) => new PredicateActionDelegate(((BlockJukebox)b).SetMusicHighpassWithHeightSettings), new Type[]
			{
				typeof(float),
				typeof(float),
				typeof(float)
			}, new string[]
			{
				"Max height",
				"Max cutoff",
				"Start height"
			}, null);
			PredicateRegistry.Add<BlockJukebox>("Jukebox.SetMusicLowpassInWaterSettings", null, (Block b) => new PredicateActionDelegate(((BlockJukebox)b).SetMusicLowpassInWaterSettings), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Cutoff freq"
			}, null);
			PredicateRegistry.Add<BlockJukebox>("Jukebox.MusicPhysicalEffects", null, (Block b) => new PredicateActionDelegate(((BlockJukebox)b).MusicPhysicalEffects), new Type[]
			{
				typeof(int),
				typeof(int)
			}, new string[]
			{
				"Lowpass in water",
				"Highpass on height"
			}, null);
			PredicateRegistry.Add<BlockJukebox>("Jukebox.MusicSlideToDistortionLevel", null, (Block b) => new PredicateActionDelegate(((BlockJukebox)b).MusicSlideToDistortionLevel), new Type[]
			{
				typeof(float),
				typeof(float)
			}, new string[]
			{
				"Level",
				"Slide duration"
			}, null);
			List<List<Tile>> value = new List<List<Tile>>
			{
				new List<Tile>
				{
					Block.FirstFrameTile(),
					Block.ThenTile(),
					new Tile(new GAF("Jukebox.SetMusic", new object[]
					{
						"MusicBackgroundAction",
						0.4f
					}))
				},
				Block.EmptyTileRow()
			};
			Block.defaultExtraTiles["Jukebox"] = value;
		}

		// Token: 0x06000C60 RID: 3168 RVA: 0x00057C0C File Offset: 0x0005600C
		public TileResultCode MusicSlideToDistortionLevel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float num = eInfo.floatArg * Util.GetFloatArg(args, 0, 0.8f);
			float floatArg = Util.GetFloatArg(args, 1, 5f);
			BlockJukebox.musicPlayerEffectsCommand.distortionOn = true;
			if (eInfo.timer == 0f)
			{
				this.startDistortionLevel = BlockJukebox.musicPlayerEffectsCommand.distortionLevel;
			}
			if (floatArg > 0f)
			{
				float num2 = eInfo.timer / floatArg;
				BlockJukebox.musicPlayerEffectsCommand.distortionLevel = num2 * num + (1f - num2) * this.startDistortionLevel;
			}
			if (eInfo.timer >= floatArg)
			{
				BlockJukebox.musicPlayerEffectsCommand.distortionLevel = num;
				return TileResultCode.True;
			}
			return TileResultCode.Delayed;
		}

		// Token: 0x06000C61 RID: 3169 RVA: 0x00057CAF File Offset: 0x000560AF
		public TileResultCode MusicPhysicalEffects(ScriptRowExecutionInfo eInfo, object[] args)
		{
			BlockJukebox.musicPlayerEffectsCommand.lowpassInWater = Util.GetIntBooleanArg(args, 0, true);
			BlockJukebox.musicPlayerEffectsCommand.highpassOnHeight = Util.GetIntBooleanArg(args, 1, true);
			Blocksworld.AddFixedUpdateUniqueCommand(BlockJukebox.musicPlayerEffectsCommand, true);
			return TileResultCode.True;
		}

		// Token: 0x06000C62 RID: 3170 RVA: 0x00057CE4 File Offset: 0x000560E4
		public TileResultCode SetMusicLowpassInWaterSettings(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float floatArg = Util.GetFloatArg(args, 0, 600f);
			BlockJukebox.musicPlayerEffectsCommand.lowpassCutoff = floatArg;
			Blocksworld.AddFixedUpdateUniqueCommand(BlockJukebox.musicPlayerEffectsCommand, true);
			return TileResultCode.True;
		}

		// Token: 0x06000C63 RID: 3171 RVA: 0x00057D18 File Offset: 0x00056118
		public TileResultCode SetMusicHighpassWithHeightSettings(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float floatArg = Util.GetFloatArg(args, 0, 300f);
			float floatArg2 = Util.GetFloatArg(args, 1, 5000f);
			float floatArg3 = Util.GetFloatArg(args, 2, 50f);
			BlockJukebox.musicPlayerEffectsCommand.highpassMaxCutoff = floatArg2;
			BlockJukebox.musicPlayerEffectsCommand.highpassStartHeight = floatArg3;
			BlockJukebox.musicPlayerEffectsCommand.highpassMaxHeight = floatArg;
			Blocksworld.AddFixedUpdateUniqueCommand(BlockJukebox.musicPlayerEffectsCommand, true);
			return TileResultCode.True;
		}

		// Token: 0x06000C64 RID: 3172 RVA: 0x00057D79 File Offset: 0x00056179
		public TileResultCode SetMusic(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.newMusicName = Util.GetStringArg(args, 0, "MusicBackground");
			this.newMusicVolume = Util.GetFloatArg(args, 1, 0.4f);
			return TileResultCode.True;
		}

		// Token: 0x06000C65 RID: 3173 RVA: 0x00057DA0 File Offset: 0x000561A0
		public TileResultCode MusicSetTo(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			string currentMusicName = Blocksworld.musicPlayer.GetCurrentMusicName();
			return (string.IsNullOrEmpty(currentMusicName) || !(stringArg == currentMusicName)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000C66 RID: 3174 RVA: 0x00057DE4 File Offset: 0x000561E4
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (BlockJukebox.musicPlayerEffectsCommand.lowpassInWater && BlockWater.BlockWithinWater(this, false))
			{
				BlockJukebox.musicPlayerEffectsCommand.forceInWaterLowpass = true;
			}
			if (BlockJukebox.musicPlayerEffectsCommand.highpassOnHeight)
			{
				BlockJukebox.musicPlayerEffectsCommand.minHighpassY = this.goT.position.y;
			}
			if (!string.IsNullOrEmpty(this.newMusicName))
			{
				Blocksworld.musicPlayer.SetMusic(this.newMusicName, this.newMusicVolume);
				this.newMusicName = string.Empty;
			}
		}

		// Token: 0x040009CA RID: 2506
		private float startDistortionLevel = -1f;

		// Token: 0x040009CB RID: 2507
		private string newMusicName = string.Empty;

		// Token: 0x040009CC RID: 2508
		private float newMusicVolume;

		// Token: 0x040009CD RID: 2509
		public static MusicPlayerEffectsCommand musicPlayerEffectsCommand = new MusicPlayerEffectsCommand();
	}
}
