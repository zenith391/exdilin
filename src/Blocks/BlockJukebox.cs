using System;
using System.Collections.Generic;

namespace Blocks;

public class BlockJukebox : Block
{
	private float startDistortionLevel = -1f;

	private string newMusicName = string.Empty;

	private float newMusicVolume;

	public static MusicPlayerEffectsCommand musicPlayerEffectsCommand = new MusicPlayerEffectsCommand();

	public BlockJukebox(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	public override void Play()
	{
		startDistortionLevel = -1f;
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockJukebox>("Jukebox.SetMusic", (Block b) => ((BlockJukebox)b).MusicSetTo, (Block b) => ((BlockJukebox)b).SetMusic, new Type[2]
		{
			typeof(string),
			typeof(float)
		}, new string[2] { "Name", "Volume" });
		PredicateRegistry.Add<BlockJukebox>("Jukebox.SetMusicHighpassWithHeightSettings", null, (Block b) => ((BlockJukebox)b).SetMusicHighpassWithHeightSettings, new Type[3]
		{
			typeof(float),
			typeof(float),
			typeof(float)
		}, new string[3] { "Max height", "Max cutoff", "Start height" });
		PredicateRegistry.Add<BlockJukebox>("Jukebox.SetMusicLowpassInWaterSettings", null, (Block b) => ((BlockJukebox)b).SetMusicLowpassInWaterSettings, new Type[1] { typeof(float) }, new string[1] { "Cutoff freq" });
		PredicateRegistry.Add<BlockJukebox>("Jukebox.MusicPhysicalEffects", null, (Block b) => ((BlockJukebox)b).MusicPhysicalEffects, new Type[2]
		{
			typeof(int),
			typeof(int)
		}, new string[2] { "Lowpass in water", "Highpass on height" });
		PredicateRegistry.Add<BlockJukebox>("Jukebox.MusicSlideToDistortionLevel", null, (Block b) => ((BlockJukebox)b).MusicSlideToDistortionLevel, new Type[2]
		{
			typeof(float),
			typeof(float)
		}, new string[2] { "Level", "Slide duration" });
		List<List<Tile>> list = new List<List<Tile>>();
		list.Add(new List<Tile>
		{
			Block.FirstFrameTile(),
			Block.ThenTile(),
			new Tile(new GAF("Jukebox.SetMusic", "MusicBackgroundAction", 0.4f))
		});
		list.Add(Block.EmptyTileRow());
		List<List<Tile>> value = list;
		Block.defaultExtraTiles["Jukebox"] = value;
	}

	public TileResultCode MusicSlideToDistortionLevel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float num = eInfo.floatArg * Util.GetFloatArg(args, 0, 0.8f);
		float floatArg = Util.GetFloatArg(args, 1, 5f);
		musicPlayerEffectsCommand.distortionOn = true;
		if (eInfo.timer == 0f)
		{
			startDistortionLevel = musicPlayerEffectsCommand.distortionLevel;
		}
		if (floatArg > 0f)
		{
			float num2 = eInfo.timer / floatArg;
			musicPlayerEffectsCommand.distortionLevel = num2 * num + (1f - num2) * startDistortionLevel;
		}
		if (eInfo.timer >= floatArg)
		{
			musicPlayerEffectsCommand.distortionLevel = num;
			return TileResultCode.True;
		}
		return TileResultCode.Delayed;
	}

	public TileResultCode MusicPhysicalEffects(ScriptRowExecutionInfo eInfo, object[] args)
	{
		musicPlayerEffectsCommand.lowpassInWater = Util.GetIntBooleanArg(args, 0, defaultValue: true);
		musicPlayerEffectsCommand.highpassOnHeight = Util.GetIntBooleanArg(args, 1, defaultValue: true);
		Blocksworld.AddFixedUpdateUniqueCommand(musicPlayerEffectsCommand);
		return TileResultCode.True;
	}

	public TileResultCode SetMusicLowpassInWaterSettings(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float floatArg = Util.GetFloatArg(args, 0, 600f);
		musicPlayerEffectsCommand.lowpassCutoff = floatArg;
		Blocksworld.AddFixedUpdateUniqueCommand(musicPlayerEffectsCommand);
		return TileResultCode.True;
	}

	public TileResultCode SetMusicHighpassWithHeightSettings(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float floatArg = Util.GetFloatArg(args, 0, 300f);
		float floatArg2 = Util.GetFloatArg(args, 1, 5000f);
		float floatArg3 = Util.GetFloatArg(args, 2, 50f);
		musicPlayerEffectsCommand.highpassMaxCutoff = floatArg2;
		musicPlayerEffectsCommand.highpassStartHeight = floatArg3;
		musicPlayerEffectsCommand.highpassMaxHeight = floatArg;
		Blocksworld.AddFixedUpdateUniqueCommand(musicPlayerEffectsCommand);
		return TileResultCode.True;
	}

	public TileResultCode SetMusic(ScriptRowExecutionInfo eInfo, object[] args)
	{
		newMusicName = Util.GetStringArg(args, 0, "MusicBackground");
		newMusicVolume = Util.GetFloatArg(args, 1, 0.4f);
		return TileResultCode.True;
	}

	public TileResultCode MusicSetTo(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		string currentMusicName = Blocksworld.musicPlayer.GetCurrentMusicName();
		if (!string.IsNullOrEmpty(currentMusicName) && stringArg == currentMusicName)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (musicPlayerEffectsCommand.lowpassInWater && BlockWater.BlockWithinWater(this))
		{
			musicPlayerEffectsCommand.forceInWaterLowpass = true;
		}
		if (musicPlayerEffectsCommand.highpassOnHeight)
		{
			musicPlayerEffectsCommand.minHighpassY = goT.position.y;
		}
		if (!string.IsNullOrEmpty(newMusicName))
		{
			Blocksworld.musicPlayer.SetMusic(newMusicName, newMusicVolume);
			newMusicName = string.Empty;
		}
	}
}
