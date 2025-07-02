using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Blocks;
using SimpleJSON;

public class WorldMetrics
{
	private static HashSet<Predicate> objectCounterConditionPredicates;

	private static HashSet<Predicate> counterConditionPredicates;

	private static HashSet<Predicate> timerConditionPredicates;

	private static HashSet<Predicate> jumpPredicates;

	private static HashSet<Predicate> leggedMoverPredicates;

	private static HashSet<Predicate> tapPredicates;

	public static string GetMetaData()
	{
		StringBuilder stringBuilder = new StringBuilder();
		List<Block> list = BWSceneManager.AllBlocks();
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		bool flag6 = false;
		bool flag7 = false;
		bool flag8 = false;
		bool flag9 = false;
		bool flag10 = false;
		bool flag11 = false;
		bool flag12 = false;
		bool flag13 = false;
		bool flag14 = false;
		bool flag15 = false;
		bool value = false;
		bool flag16 = false;
		bool flag17 = false;
		bool flag18 = false;
		bool flag19 = false;
		bool flag20 = false;
		bool flag21 = false;
		bool flag22 = false;
		bool flag23 = false;
		bool flag24 = false;
		bool flag25 = false;
		bool flag26 = false;
		bool flag27 = false;
		bool flag28 = false;
		bool flag29 = false;
		bool flag30 = false;
		bool flag31 = false;
		bool value2 = false;
		HashSet<string> hashSet = new HashSet<string>();
		HashSet<string> hashSet2 = new HashSet<string>();
		HashSet<string> hashSet3 = new HashSet<string>();
		HashSet<string> hashSet4 = new HashSet<string>();
		HashSet<string> hashSet5 = new HashSet<string>();
		HashSet<string> hashSet6 = new HashSet<string>();
		HashSet<string> hashSet7 = new HashSet<string>();
		HashSet<Predicate> taggedPredicates = Blocksworld.GetTaggedPredicates();
		HashSet<Predicate> analogStickPredicates = Blocksworld.GetAnalogStickPredicates();
		for (int i = 0; i < list.Count; i++)
		{
			Block block = list[i];
			flag = flag || block is BlockObjectCounterUI;
			flag2 = flag2 || block is BlockCounterUI;
			flag3 = flag3 || block is BlockTimerUI;
			flag5 = flag5 || block is BlockRadarUI;
			flag4 = flag4 || block is BlockGaugeUI;
			bool flag32 = false;
			bool flag33 = false;
			for (int j = 0; j < block.tiles.Count; j++)
			{
				List<Tile> list2 = block.tiles[j];
				bool flag34 = true;
				bool flag35 = false;
				bool flag36 = false;
				bool flag37 = false;
				for (int k = 0; k < list2.Count; k++)
				{
					Tile tile = list2[k];
					GAF gaf = tile.gaf;
					object[] args = gaf.Args;
					Predicate predicate = gaf.Predicate;
					if (predicate == Block.predicateThen)
					{
						flag34 = false;
					}
					if (GetLeggedMoverPredicates().Contains(predicate))
					{
						flag32 = true;
						value = true;
						flag16 = flag16 || block is BlockCharacter;
						flag17 = flag17 || block is BlockLegs;
						flag18 = flag18 || block is BlockMLPLegs;
						flag19 = flag19 || block is BlockQuadped;
					}
					if (GetJumpPredicates().Contains(predicate))
					{
						flag33 = true;
					}
					flag20 = flag20 || (flag33 && flag32);
					flag21 = flag21 || (flag20 && block is BlockCharacter);
					flag22 = flag22 || (flag20 && block is BlockLegs);
					flag23 = flag23 || (flag20 && block is BlockMLPLegs);
					flag24 = flag24 || (flag20 && block is BlockQuadped);
					flag6 = flag6 || predicate == Block.predicateGameWin;
					flag7 = flag7 || predicate == Block.predicateGameLose;
					flag8 = flag8 || predicate == Block.predicateIsTreasure || predicate == Block.predicateIsTreasureForTag;
					flag9 = flag9 || predicate == Block.predicateIsPickup || predicate == Block.predicateIsPickupForTag;
					if (analogStickPredicates.Contains(predicate))
					{
						for (int l = 0; l < args.Length; l++)
						{
							if (args[l] is string)
							{
								hashSet7.Add((string)args[l]);
								break;
							}
						}
					}
					flag31 = flag31 || predicate == Block.predicateButton;
					flag28 = flag28 || analogStickPredicates.Contains(predicate);
					flag29 = flag29 || predicate == Block.predicateTiltLeftRight;
					flag31 = flag31 || predicate == Block.predicateButton;
					flag30 = flag30 || GetTapPredicates().Contains(predicate);
					if (predicate == Block.predicateSpeak)
					{
						value2 = true;
						string text = (string)tile.gaf.Args[0];
						flag25 = flag25 || text.Contains("|Stop]");
						flag26 = flag26 || text.Contains("|Back]") || text.Contains("|Done]");
						flag27 = flag27 || text.Contains("|Restart]");
					}
					if (predicate == Block.predicateTag || predicate == Block.predicateCustomTag)
					{
						hashSet.Add((string)args[0]);
					}
					if (taggedPredicates.Contains(predicate))
					{
						hashSet2.Add((string)args[0]);
					}
					if (predicate == Block.predicateSendSignal)
					{
						if (flag34)
						{
							hashSet3.Add(args[0].ToString());
						}
						else
						{
							hashSet4.Add(args[0].ToString());
						}
					}
					if (predicate == Block.predicateSendCustomSignal)
					{
						if (flag34)
						{
							hashSet5.Add(args[0].ToString());
						}
						else
						{
							hashSet6.Add(args[0].ToString());
						}
					}
					if (flag34)
					{
						flag35 = flag35 || GetObjectCounterConditionPredicates().Contains(predicate);
						flag36 = flag36 || GetCounterConditionPredicates().Contains(predicate);
						flag37 = flag37 || GetTimerConditionPredicates().Contains(predicate);
						continue;
					}
					if (flag35)
					{
						flag10 = flag10 || predicate == Block.predicateGameWin;
						flag11 = flag11 || predicate == Block.predicateGameLose;
					}
					if (flag36)
					{
						flag12 = flag12 || predicate == Block.predicateGameWin;
						flag13 = flag13 || predicate == Block.predicateGameLose;
					}
					if (flag37)
					{
						flag14 = flag14 || predicate == Block.predicateGameWin;
						flag15 = flag15 || predicate == Block.predicateGameLose;
					}
				}
			}
		}
		StringWriter writer = new StringWriter(stringBuilder, CultureInfo.InvariantCulture);
		JSONStreamEncoder jSONStreamEncoder = new JSONStreamEncoder(writer);
		jSONStreamEncoder.BeginObject();
		jSONStreamEncoder.InsertNewline();
		Dictionary<string, bool> dictionary = new Dictionary<string, bool>
		{
			{ "hasObjectCounter", flag },
			{ "hasCounter", flag2 },
			{ "hasRadar", flag5 },
			{ "hasGauge", flag4 },
			{ "hasWinTile", flag6 },
			{ "hasLoseTile", flag7 },
			{ "hasLoot", flag8 },
			{ "hasPickup", flag9 },
			{ "winHasObjectCounterCondition", flag10 },
			{ "loseHasObjectCounterCondition", flag11 },
			{ "winHasCounterCondition", flag12 },
			{ "loseHasCounterCondition", flag13 },
			{ "winHasTimerCondition", flag14 },
			{ "loseHasTimerCondition", flag15 },
			{ "hasMoverOnLeggedBlock", value },
			{ "hasMoverOnCharacter", flag16 },
			{ "hasMoverOnLegs", flag17 },
			{ "hasMoverOnMLPLegs", flag18 },
			{ "hasMoverOnQuadped", flag19 },
			{ "hasMoverAndJumpOnCharacter", flag21 },
			{ "hasMoverAndJumpOnLegs", flag22 },
			{ "hasMoverAndJumpOnMLPLegs", flag23 },
			{ "hasMoverAndJumpOnQuadped", flag24 },
			{ "hasStopActionInSpeak", flag25 },
			{ "hasBackActionInSpeak", flag26 },
			{ "hasRestartActionInSpeak", flag27 },
			{ "usingAnalogStick", flag28 },
			{ "usingTilt", flag29 },
			{ "usingTap", flag30 },
			{ "usingButtons", flag31 },
			{ "usingSpeak", value2 }
		};
		Dictionary<string, List<string>> dictionary2 = new Dictionary<string, List<string>>
		{
			{
				"usedTags",
				new List<string>(hashSet)
			},
			{
				"tileTags",
				new List<string>(hashSet2)
			},
			{
				"sendingSignals",
				new List<string>(hashSet4)
			},
			{
				"receivingSignals",
				new List<string>(hashSet3)
			},
			{
				"sendingCustomSignals",
				new List<string>(hashSet6)
			},
			{
				"receivingCustomSignals",
				new List<string>(hashSet5)
			},
			{
				"usedAnalogSticks",
				new List<string>(hashSet7)
			}
		};
		foreach (KeyValuePair<string, bool> item in dictionary)
		{
			jSONStreamEncoder.WriteKey(item.Key);
			jSONStreamEncoder.WriteBool(item.Value);
			jSONStreamEncoder.InsertNewline();
		}
		foreach (KeyValuePair<string, List<string>> item2 in dictionary2)
		{
			jSONStreamEncoder.WriteKey(item2.Key);
			jSONStreamEncoder.BeginArray();
			item2.Value.Sort();
			foreach (string item3 in item2.Value)
			{
				jSONStreamEncoder.WriteString(item3);
			}
			jSONStreamEncoder.EndArray();
			jSONStreamEncoder.InsertNewline();
		}
		jSONStreamEncoder.EndObject();
		jSONStreamEncoder.InsertNewline();
		return stringBuilder.ToString();
	}

	private static HashSet<Predicate> GetObjectCounterConditionPredicates()
	{
		if (objectCounterConditionPredicates == null)
		{
			objectCounterConditionPredicates = new HashSet<Predicate>
			{
				Block.predicateObjectCounterEquals,
				Block.predicateObjectCounterEqualsMax,
				Block.predicateObjectCounterValueCondition
			};
		}
		return objectCounterConditionPredicates;
	}

	private static HashSet<Predicate> GetCounterConditionPredicates()
	{
		if (counterConditionPredicates == null)
		{
			counterConditionPredicates = new HashSet<Predicate>
			{
				Block.predicateCounterEquals,
				Block.predicateCounterValueCondition
			};
		}
		return counterConditionPredicates;
	}

	private static HashSet<Predicate> GetTimerConditionPredicates()
	{
		if (timerConditionPredicates == null)
		{
			timerConditionPredicates = new HashSet<Predicate> { Block.predicateTimerEquals };
		}
		return timerConditionPredicates;
	}

	private static HashSet<Predicate> GetJumpPredicates()
	{
		if (jumpPredicates == null)
		{
			jumpPredicates = new HashSet<Predicate>
			{
				BlockLegs.predicateLegsJump,
				BlockCharacter.predicateCharacterJump,
				BlockMLPLegs.predicateMLPLegsJump,
				BlockQuadped.predicateQuadpedJump
			};
		}
		return jumpPredicates;
	}

	private static HashSet<Predicate> GetLeggedMoverPredicates()
	{
		if (leggedMoverPredicates == null)
		{
			leggedMoverPredicates = new HashSet<Predicate>
			{
				BlockLegs.predicateLegsMover,
				BlockCharacter.predicateCharacterMover,
				BlockMLPLegs.predicateMLPLegsMover,
				BlockQuadped.predicateQuadpedMover
			};
		}
		return leggedMoverPredicates;
	}

	private static HashSet<Predicate> GetTapPredicates()
	{
		if (tapPredicates == null)
		{
			tapPredicates = new HashSet<Predicate>
			{
				Block.predicateTapBlock,
				Block.predicateTapChunk,
				Block.predicateTapModel,
				Block.predicateTapHoldBlock,
				Block.predicateTapHoldModel
			};
		}
		return tapPredicates;
	}
}
