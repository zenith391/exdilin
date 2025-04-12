using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Blocks;
using SimpleJSON;

// Token: 0x02000363 RID: 867
public class WorldMetrics
{
	// Token: 0x0600267B RID: 9851 RVA: 0x0011B5DC File Offset: 0x001199DC
	public static string GetMetaData()
	{
		StringBuilder stringBuilder = new StringBuilder();
		List<Block> list = BWSceneManager.AllBlocks();
		bool hasObjectCounter = false;
		bool hasCounter = false;
		bool hasTimer = false;
		bool hasGauge = false;
		bool hasRadar = false;
		bool hasWinTile = false;
		bool hasLoseTile = false;
		bool hasLoot = false;
		bool hasPickup = false;
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
		bool hasStopActionInSpeak = false;
		bool hasBackActionInSpeak = false;
		bool hasRestartActionInSpeak = false;
		bool usingAnalogStick = false;
		bool usingTilt = false;
		bool usingTap = false;
		bool usingButtons = false;
		bool usingSpeak = false;
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
            hasObjectCounter = (hasObjectCounter || block is BlockObjectCounterUI);
            hasCounter = (hasCounter || block is BlockCounterUI);
            hasTimer = (hasTimer || block is BlockTimerUI);
            hasRadar = (hasRadar || block is BlockRadarUI);
            hasGauge = (hasGauge || block is BlockGaugeUI);
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
					if (WorldMetrics.GetLeggedMoverPredicates().Contains(predicate))
					{
						flag32 = true;
						value = true;
						flag16 = (flag16 || block is BlockCharacter);
						flag17 = (flag17 || block is BlockLegs);
						flag18 = (flag18 || block is BlockMLPLegs);
						flag19 = (flag19 || block is BlockQuadped);
					}
					if (WorldMetrics.GetJumpPredicates().Contains(predicate))
					{
						flag33 = true;
					}
					flag20 = (flag20 || (flag33 && flag32));
					flag21 = (flag21 || (flag20 && block is BlockCharacter));
					flag22 = (flag22 || (flag20 && block is BlockLegs));
					flag23 = (flag23 || (flag20 && block is BlockMLPLegs));
					flag24 = (flag24 || (flag20 && block is BlockQuadped));
                    hasWinTile = (hasWinTile || predicate == Block.predicateGameWin);
                    hasLoseTile = (hasLoseTile || predicate == Block.predicateGameLose);
                    hasLoot = (hasLoot || predicate == Block.predicateIsTreasure || predicate == Block.predicateIsTreasureForTag);
                    hasPickup = (hasPickup || predicate == Block.predicateIsPickup || predicate == Block.predicateIsPickupForTag);
					bool flag38 = analogStickPredicates.Contains(predicate);
					if (flag38)
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
                    usingButtons = (usingButtons || predicate == Block.predicateButton);
                    usingAnalogStick = (usingAnalogStick || analogStickPredicates.Contains(predicate));
                    usingTilt = (usingTilt || predicate == Block.predicateTiltLeftRight);
                    usingButtons = (usingButtons || predicate == Block.predicateButton); // duplicate from OG source code, to keep or delete?
                    usingTap = (usingTap || WorldMetrics.GetTapPredicates().Contains(predicate));
					if (predicate == Block.predicateSpeak)
					{
                        usingSpeak = true;
						string text = (string)tile.gaf.Args[0];
                        hasStopActionInSpeak = (hasStopActionInSpeak || text.Contains("|Stop]"));
                        hasBackActionInSpeak = (hasBackActionInSpeak || text.Contains("|Back]") || text.Contains("|Done]"));
                        hasRestartActionInSpeak = (hasRestartActionInSpeak || text.Contains("|Restart]"));
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
						flag35 = (flag35 || WorldMetrics.GetObjectCounterConditionPredicates().Contains(predicate));
						flag36 = (flag36 || WorldMetrics.GetCounterConditionPredicates().Contains(predicate));
						flag37 = (flag37 || WorldMetrics.GetTimerConditionPredicates().Contains(predicate));
					}
					else
					{
						if (flag35)
						{
							flag10 = (flag10 || predicate == Block.predicateGameWin);
							flag11 = (flag11 || predicate == Block.predicateGameLose);
						}
						if (flag36)
						{
							flag12 = (flag12 || predicate == Block.predicateGameWin);
							flag13 = (flag13 || predicate == Block.predicateGameLose);
						}
						if (flag37)
						{
							flag14 = (flag14 || predicate == Block.predicateGameWin);
							flag15 = (flag15 || predicate == Block.predicateGameLose);
						}
					}
				}
			}
		}
		StringWriter writer = new StringWriter(stringBuilder, CultureInfo.InvariantCulture);
		JSONStreamEncoder jsonstreamEncoder = new JSONStreamEncoder(writer, 20);
		jsonstreamEncoder.BeginObject();
		jsonstreamEncoder.InsertNewline();
		Dictionary<string, bool> dictionary = new Dictionary<string, bool>
		{
			{
				"hasObjectCounter",
                hasObjectCounter
            },
			{
				"hasCounter",
                hasCounter
            },
			{
				"hasRadar",
                hasRadar
            },
			{
				"hasGauge",
                hasGauge
            },
			{
				"hasWinTile",
                hasWinTile
            },
			{
				"hasLoseTile",
                hasLoseTile
            },
			{
				"hasLoot",
                hasLoot
            },
			{
				"hasPickup",
                hasPickup
            },
			{
				"winHasObjectCounterCondition",
				flag10
			},
			{
				"loseHasObjectCounterCondition",
				flag11
			},
			{
				"winHasCounterCondition",
				flag12
			},
			{
				"loseHasCounterCondition",
				flag13
			},
			{
				"winHasTimerCondition",
				flag14
			},
			{
				"loseHasTimerCondition",
				flag15
			},
			{
				"hasMoverOnLeggedBlock",
				value
			},
			{
				"hasMoverOnCharacter",
				flag16
			},
			{
				"hasMoverOnLegs",
				flag17
			},
			{
				"hasMoverOnMLPLegs",
				flag18
			},
			{
				"hasMoverOnQuadped",
				flag19
			},
			{
				"hasMoverAndJumpOnCharacter",
				flag21
			},
			{
				"hasMoverAndJumpOnLegs",
				flag22
			},
			{
				"hasMoverAndJumpOnMLPLegs",
				flag23
			},
			{
				"hasMoverAndJumpOnQuadped",
				flag24
			},
			{
				"hasStopActionInSpeak",
                hasStopActionInSpeak
            },
			{
				"hasBackActionInSpeak",
                hasBackActionInSpeak
            },
			{
				"hasRestartActionInSpeak",
                hasRestartActionInSpeak
            },
			{
				"usingAnalogStick",
                usingAnalogStick
            },
			{
				"usingTilt",
                usingTilt
            },
			{
				"usingTap",
                usingTap
            },
			{
				"usingButtons",
                usingButtons
            },
			{
				"usingSpeak",
                usingSpeak
            }
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
		foreach (KeyValuePair<string, bool> keyValuePair in dictionary)
		{
			jsonstreamEncoder.WriteKey(keyValuePair.Key);
			jsonstreamEncoder.WriteBool(keyValuePair.Value);
			jsonstreamEncoder.InsertNewline();
		}
		foreach (KeyValuePair<string, List<string>> keyValuePair2 in dictionary2)
		{
			jsonstreamEncoder.WriteKey(keyValuePair2.Key);
			jsonstreamEncoder.BeginArray();
			keyValuePair2.Value.Sort();
			foreach (string str in keyValuePair2.Value)
			{
				jsonstreamEncoder.WriteString(str);
			}
			jsonstreamEncoder.EndArray();
			jsonstreamEncoder.InsertNewline();
		}
		jsonstreamEncoder.EndObject();
		jsonstreamEncoder.InsertNewline();
		return stringBuilder.ToString();
	}

	// Token: 0x0600267C RID: 9852 RVA: 0x0011BFCC File Offset: 0x0011A3CC
	private static HashSet<Predicate> GetObjectCounterConditionPredicates()
	{
		if (WorldMetrics.objectCounterConditionPredicates == null)
		{
			WorldMetrics.objectCounterConditionPredicates = new HashSet<Predicate>
			{
				Block.predicateObjectCounterEquals,
				Block.predicateObjectCounterEqualsMax,
				Block.predicateObjectCounterValueCondition
			};
		}
		return WorldMetrics.objectCounterConditionPredicates;
	}

	// Token: 0x0600267D RID: 9853 RVA: 0x0011C018 File Offset: 0x0011A418
	private static HashSet<Predicate> GetCounterConditionPredicates()
	{
		if (WorldMetrics.counterConditionPredicates == null)
		{
			WorldMetrics.counterConditionPredicates = new HashSet<Predicate>
			{
				Block.predicateCounterEquals,
				Block.predicateCounterValueCondition
			};
		}
		return WorldMetrics.counterConditionPredicates;
	}

	// Token: 0x0600267E RID: 9854 RVA: 0x0011C058 File Offset: 0x0011A458
	private static HashSet<Predicate> GetTimerConditionPredicates()
	{
		if (WorldMetrics.timerConditionPredicates == null)
		{
			WorldMetrics.timerConditionPredicates = new HashSet<Predicate>
			{
				Block.predicateTimerEquals
			};
		}
		return WorldMetrics.timerConditionPredicates;
	}

	// Token: 0x0600267F RID: 9855 RVA: 0x0011C08C File Offset: 0x0011A48C
	private static HashSet<Predicate> GetJumpPredicates()
	{
		if (WorldMetrics.jumpPredicates == null)
		{
			WorldMetrics.jumpPredicates = new HashSet<Predicate>
			{
				BlockLegs.predicateLegsJump,
				BlockCharacter.predicateCharacterJump,
				BlockMLPLegs.predicateMLPLegsJump,
				BlockQuadped.predicateQuadpedJump
			};
		}
		return WorldMetrics.jumpPredicates;
	}

	// Token: 0x06002680 RID: 9856 RVA: 0x0011C0E4 File Offset: 0x0011A4E4
	private static HashSet<Predicate> GetLeggedMoverPredicates()
	{
		if (WorldMetrics.leggedMoverPredicates == null)
		{
			WorldMetrics.leggedMoverPredicates = new HashSet<Predicate>
			{
				BlockLegs.predicateLegsMover,
				BlockCharacter.predicateCharacterMover,
				BlockMLPLegs.predicateMLPLegsMover,
				BlockQuadped.predicateQuadpedMover
			};
		}
		return WorldMetrics.leggedMoverPredicates;
	}

	// Token: 0x06002681 RID: 9857 RVA: 0x0011C13C File Offset: 0x0011A53C
	private static HashSet<Predicate> GetTapPredicates()
	{
		if (WorldMetrics.tapPredicates == null)
		{
			WorldMetrics.tapPredicates = new HashSet<Predicate>
			{
				Block.predicateTapBlock,
				Block.predicateTapChunk,
				Block.predicateTapModel,
				Block.predicateTapHoldBlock,
				Block.predicateTapHoldModel
			};
		}
		return WorldMetrics.tapPredicates;
	}

	// Token: 0x040021AD RID: 8621
	private static HashSet<Predicate> objectCounterConditionPredicates;

	// Token: 0x040021AE RID: 8622
	private static HashSet<Predicate> counterConditionPredicates;

	// Token: 0x040021AF RID: 8623
	private static HashSet<Predicate> timerConditionPredicates;

	// Token: 0x040021B0 RID: 8624
	private static HashSet<Predicate> jumpPredicates;

	// Token: 0x040021B1 RID: 8625
	private static HashSet<Predicate> leggedMoverPredicates;

	// Token: 0x040021B2 RID: 8626
	private static HashSet<Predicate> tapPredicates;
}
