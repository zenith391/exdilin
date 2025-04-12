using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020000C1 RID: 193
	public class BlockRadarUI : BlockAbstractUI
	{
		// Token: 0x06000ED5 RID: 3797 RVA: 0x00063C74 File Offset: 0x00062074
		public BlockRadarUI(List<List<Tile>> tiles, int index) : base(tiles, index)
		{
			this.sweepMesh = this.goT.Find("UINRadarSweep_RdUpR");
		}

		// Token: 0x06000ED6 RID: 3798 RVA: 0x00063CE4 File Offset: 0x000620E4
		public new static void Register()
		{
			PredicateRegistry.Add<BlockRadarUI>("RadarUI.SetMaxDistance", null, (Block b) => new PredicateActionDelegate(((BlockRadarUI)b).SetMaxDistance), new Type[]
			{
				typeof(float)
			}, null, null);
			BlockRadarUI.predicateRadarTrackTag = PredicateRegistry.Add<BlockRadarUI>("RadarUI.TrackTag", (Block b) => new PredicateSensorDelegate(((BlockRadarUI)b).TagWithinRange), (Block b) => new PredicateActionDelegate(((BlockRadarUI)b).TrackTag), new Type[]
			{
				typeof(string),
				typeof(int),
				typeof(string)
			}, null, null);
			PredicateRegistry.Add<BlockRadarUI>("RadarUI.SetText", null, (Block b) => new PredicateActionDelegate(((BlockRadarUI)b).SetText), new Type[]
			{
				typeof(string)
			}, null, null);
			BlockRadarUI.predicateRadarCenterOnTag = PredicateRegistry.Add<BlockRadarUI>("RadarUI.CenterOnTag", null, (Block b) => new PredicateActionDelegate(((BlockRadarUI)b).CenterOnTag), new Type[]
			{
				typeof(string)
			}, null, null);
			PredicateRegistry.Add<BlockRadarUI>("RadarUI.ShowUI", null, (Block b) => new PredicateActionDelegate(((BlockAbstractUI)b).ShowUI), new Type[]
			{
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<BlockRadarUI>("RadarUI.SeeForever", null, (Block b) => new PredicateActionDelegate(((BlockRadarUI)b).SeeForever), new Type[]
			{
				typeof(int)
			}, null, null);
			Block.AddSimpleDefaultTiles(new List<GAF>
			{
				new GAF("RadarUI.SetMaxDistance", new object[]
				{
					50f
				}),
				new GAF("RadarUI.SeeForever", new object[0])
			}, new string[]
			{
				"UI Radar I"
			});
			bool flag = true;
			if (flag)
			{
				Blocksworld.UI.SetRadarUIActive(false);
			}
			BlockRadarUI.SetNameCompares();
		}

		// Token: 0x06000ED7 RID: 3799 RVA: 0x00063F0E File Offset: 0x0006230E
		private void ActivateRadar(bool enabled)
		{
			if (this.radarActive != enabled)
			{
				this.radarActive = enabled;
				Blocksworld.UI.SetRadarUIActive(enabled);
				Blocksworld.UI.SetRadarUICenterTagActive(this.centerTag);
			}
		}

		// Token: 0x06000ED8 RID: 3800 RVA: 0x00063F3E File Offset: 0x0006233E
		protected override void SetUIVisible(bool v)
		{
			base.SetUIVisible(v);
			if (this.dirty)
			{
				this.ActivateRadar(base.uiVisible);
			}
		}

		// Token: 0x06000ED9 RID: 3801 RVA: 0x00063F60 File Offset: 0x00062360
		private static void SetNameCompares()
		{
			string[] tagNames = Tile.tagNames;
			string[] shortTagNames = Tile.shortTagNames;
			for (int i = 0; i < Mathf.Min(tagNames.Length, shortTagNames.Length); i++)
			{
				if (!BlockRadarUI.tagNameCompares.ContainsKey(shortTagNames[i]))
				{
					BlockRadarUI.tagNameCompares.Add(shortTagNames[i], tagNames[i]);
				}
			}
		}

		// Token: 0x06000EDA RID: 3802 RVA: 0x00063FB8 File Offset: 0x000623B8
		public override void Play()
		{
			base.Play();
			this.style = HudMeshOnGUI.dataSource.GetStyle("Counter");
			this.outlineStyle = HudMeshOnGUI.dataSource.GetStyle("Outline");
			this.radarRect = Blocksworld.UI.GetRadarUIRect();
			this.textRect = BlockRadarUI.GetRadarTextRect(this.radarRect);
			this.radarRect.x = this.radarRect.x / NormalizedScreen.scale;
			this.radarRect.y = this.radarRect.y / NormalizedScreen.scale;
			this.radarRect.width = this.radarRect.width / NormalizedScreen.scale;
			this.radarRect.height = this.radarRect.height / NormalizedScreen.scale;
			this.followedTags.Clear();
			this.guiFollowedTags.Clear();
			this.centerTag = string.Empty;
			this.maxDistance = 50f;
		}

		// Token: 0x06000EDB RID: 3803 RVA: 0x0006409E File Offset: 0x0006249E
		public override void Stop(bool resetBlock = true)
		{
			base.Stop(resetBlock);
			this.maxDistance = 50f;
			this.ActivateRadar(false);
			this.ClearTiles();
		}

		// Token: 0x06000EDC RID: 3804 RVA: 0x000640C0 File Offset: 0x000624C0
		private void ClearTiles()
		{
			foreach (KeyValuePair<string, List<Tile>> keyValuePair in BlockRadarUI.tagTiles)
			{
				foreach (Tile tile in keyValuePair.Value)
				{
					tile.DestroyPhysical();
				}
			}
			BlockRadarUI.tagTiles.Clear();
			BlockRadarUI.tagColors.Clear();
		}

		// Token: 0x06000EDD RID: 3805 RVA: 0x00064174 File Offset: 0x00062574
		public TileResultCode SeeForever(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.alwaysSee = (args.Length <= 0 || (int)args[0] != 1);
			return TileResultCode.True;
		}

		// Token: 0x06000EDE RID: 3806 RVA: 0x0006419A File Offset: 0x0006259A
		public override TileResultCode SetText(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.text = ((args.Length <= 0) ? string.Empty : base.ParseText((string)args[0]));
			return TileResultCode.True;
		}

		// Token: 0x06000EDF RID: 3807 RVA: 0x000641C4 File Offset: 0x000625C4
		public TileResultCode SetMaxDistance(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.maxDistance = ((args.Length <= 0) ? 50f : ((float)args[0]));
			return TileResultCode.True;
		}

		// Token: 0x06000EE0 RID: 3808 RVA: 0x000641E8 File Offset: 0x000625E8
		public TileResultCode TrackTag(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string text = (args.Length <= 0) ? string.Empty : ((string)args[0]);
			string colorName = (args.Length <= 2) ? "White" : ((string)args[2]);
			this.SetColor(text, colorName);
			if (text.Length > 0)
			{
				this.followedTags.Add(text);
				this.guiFollowedTags.Add(text);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000EE1 RID: 3809 RVA: 0x0006425C File Offset: 0x0006265C
		public TileResultCode CenterOnTag(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string a = (args.Length <= 0) ? string.Empty : ((string)args[0]);
			if (!string.Equals(a, this.centerTag))
			{
				this.centerTag = a;
				Blocksworld.UI.SetRadarUICenterTagActive(this.centerTag);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000EE2 RID: 3810 RVA: 0x000642B0 File Offset: 0x000626B0
		public TileResultCode TagWithinRange(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string tagName = (args.Length <= 0) ? string.Empty : ((string)args[0]);
			Vector3 position = this.goT.position;
			Block block;
			if (TagManager.TryGetClosestBlockWithTag(tagName, position, out block, null) && (block.goT.position - position).sqrMagnitude < this.maxDistance * this.maxDistance)
			{
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x06000EE3 RID: 3811 RVA: 0x00064322 File Offset: 0x00062722
		public override void ResetFrame()
		{
			this.alwaysSee = false;
		}

		// Token: 0x06000EE4 RID: 3812 RVA: 0x0006432C File Offset: 0x0006272C
		private void TrackAllTags()
		{
			Color[] array = new Color[]
			{
				Color.white,
				Color.white
			};
			for (int i = 0; i < Tile.shortTagNames.Length; i++)
			{
				if (this.centerTag != Tile.shortTagNames[i])
				{
					this.guiFollowedTags.Add(Tile.shortTagNames[i]);
					Blocksworld.colorDefinitions.TryGetValue(BlockRadarUI.defaultTagColorNames[Tile.shortTagNames[i]], out array);
					BlockRadarUI.tagColors[Tile.shortTagNames[i]] = array[0];
				}
			}
		}

		// Token: 0x06000EE5 RID: 3813 RVA: 0x000643E0 File Offset: 0x000627E0
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (Blocksworld.CurrentState == State.Play)
			{
				this.ActivateRadar(base.uiVisible);
				if (base.uiVisible)
				{
					bool flag = false;
					foreach (string item in this.followedTags)
					{
						if (!this.prevFollowedTags.Contains(item))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						foreach (string item2 in this.prevFollowedTags)
						{
							if (!this.followedTags.Contains(item2))
							{
								flag = true;
								break;
							}
						}
					}
					if (flag || this.guiFollowedTags.Count == 0)
					{
						this.guiFollowedTags.Clear();
						if (this.followedTags.Count == 0)
						{
							this.TrackAllTags();
						}
						else
						{
							foreach (string item3 in this.followedTags)
							{
								this.guiFollowedTags.Add(item3);
							}
						}
					}
					this.prevFollowedTags.Clear();
					foreach (string item4 in this.followedTags)
					{
						this.prevFollowedTags.Add(item4);
					}
					this.followedTags.Clear();
					this.sweepAngle += 2f;
					if (this.sweepAngle > 360f)
					{
						this.sweepAngle -= 360f;
					}
				}
			}
		}

		// Token: 0x06000EE6 RID: 3814 RVA: 0x00064608 File Offset: 0x00062A08
		private static Rect GetRadarTextRect(Rect radarDisplayRect)
		{
			float width = radarDisplayRect.width;
			float height = 65f * NormalizedScreen.scale;
			float x = radarDisplayRect.x;
			float y = 32f * NormalizedScreen.scale;
			Rect result = new Rect(x, y, width, height);
			return result;
		}

		// Token: 0x06000EE7 RID: 3815 RVA: 0x0006464C File Offset: 0x00062A4C
		private Tile GetTile(string tag, Dictionary<string, int> ourIndices)
		{
			int num = 0;
			if (ourIndices.ContainsKey(tag))
			{
				num = ourIndices[tag];
			}
			string str;
			BlockRadarUI.tagNameCompares.TryGetValue(tag, out str);
			string iconName = "Radar/Radar_Tag_" + str;
			Tile tile;
			if (!BlockRadarUI.tagTiles.ContainsKey(tag))
			{
				tile = new Tile(Blocksworld.tilePool.GetTileObjectForIcon(iconName, true));
				BlockRadarUI.tagTiles.Add(tag, new List<Tile>());
				BlockRadarUI.tagTiles[tag].Add(tile);
			}
			else if (BlockRadarUI.tagTiles[tag].Count < num)
			{
				tile = new Tile(Blocksworld.tilePool.GetTileObjectForIcon(iconName, true));
				BlockRadarUI.tagTiles[tag].Add(tile);
			}
			else
			{
				tile = BlockRadarUI.tagTiles[tag][num - 1];
			}
			return tile;
		}

		// Token: 0x06000EE8 RID: 3816 RVA: 0x00064724 File Offset: 0x00062B24
		private void SetColor(string tagName, string colorName)
		{
			Color[] array = new Color[]
			{
				Color.white
			};
			Blocksworld.colorDefinitions.TryGetValue(colorName, out array);
			BlockRadarUI.tagColors[tagName] = array[0];
		}

		// Token: 0x06000EE9 RID: 3817 RVA: 0x00064770 File Offset: 0x00062B70
		private Color GetColor(string tag)
		{
			Color white = Color.white;
			BlockRadarUI.tagColors.TryGetValue(tag, out white);
			return white;
		}

		// Token: 0x06000EEA RID: 3818 RVA: 0x00064794 File Offset: 0x00062B94
		private void GetSortedBlockList(Vector3 ourCenter)
		{
			this.sortedBlockList.Clear();
			List<Block> list = new List<Block>();
			foreach (string posName in this.guiFollowedTags)
			{
				List<Block> blocksWithTag = TagManager.GetBlocksWithTag(posName);
				for (int i = 0; i < blocksWithTag.Count; i++)
				{
					Block block = blocksWithTag[i];
					if (!block.isTreasure || (!TreasureHandler.IsHiddenTreasureModel(block) && TreasureHandler.GetTreasureModelScale(block) >= 0.15f))
					{
						if (!list.Contains(block))
						{
							list.Add(block);
						}
					}
				}
			}
			IEnumerable<Block> source = list;
			source = from taggedBlock in source
			orderby (taggedBlock.goT.position - ourCenter).sqrMagnitude
			select taggedBlock;
			list = source.ToList<Block>();
			if (list.Count > 25)
			{
				list.RemoveRange(25, list.Count - 25);
			}
			foreach (Block block2 in list)
			{
				float y = block2.goT.position.y;
				this.minY = Mathf.Min(y, this.minY);
				this.maxY = Mathf.Max(y, this.maxY);
			}
			foreach (string text in this.guiFollowedTags)
			{
				List<Block> blocksWithTag2 = TagManager.GetBlocksWithTag(text);
				List<Block> list2 = new List<Block>();
				for (int j = 0; j < blocksWithTag2.Count; j++)
				{
					Block item = blocksWithTag2[j];
					if (list.Contains(item))
					{
						list2.Add(item);
					}
				}
				this.sortedBlockList.Add(text, list2);
			}
		}

		// Token: 0x06000EEB RID: 3819 RVA: 0x000649D8 File Offset: 0x00062DD8
		public override void OnHudMesh()
		{
			base.OnHudMesh();
			if (base.uiVisible && Blocksworld.CurrentState == State.Play && !string.IsNullOrEmpty(this.text))
			{
				Rect rect = new Rect(this.textRect.x + 2f, this.textRect.y + 2f, this.textRect.width, this.textRect.height);
				HudMeshOnGUI.Label(ref this.textOutlineLabel, rect, this.text, this.outlineStyle, 0f);
				HudMeshOnGUI.Label(ref this.textLabel, this.textRect, this.text, this.textColor, this.style);
			}
		}

		// Token: 0x06000EEC RID: 3820 RVA: 0x00064A90 File Offset: 0x00062E90
		private Vector3 GetRadarCenter()
		{
			if (Blocksworld.blocksworldCamera.firstPersonBlock != null)
			{
				return Blocksworld.blocksworldCamera.firstPersonBlock.go.transform.position;
			}
			if (string.IsNullOrEmpty(this.centerTag))
			{
				return Blocksworld.blocksworldCamera.GetTargetPosition();
			}
			Block block;
			if (TagManager.TryGetClosestBlockWithTag(this.centerTag, this.goT.position, out block, null))
			{
				return block.goT.position;
			}
			return this.goT.position;
		}

		// Token: 0x06000EED RID: 3821 RVA: 0x00064B18 File Offset: 0x00062F18
		public override void Update()
		{
			base.Update();
			HashSet<Tile> hashSet = new HashSet<Tile>();
			foreach (KeyValuePair<string, List<Tile>> keyValuePair in BlockRadarUI.tagTiles)
			{
				for (int i = 0; i < keyValuePair.Value.Count; i++)
				{
					if (keyValuePair.Value[i].IsShowing())
					{
						hashSet.Add(keyValuePair.Value[i]);
					}
				}
			}
			if (base.uiVisible && Blocksworld.CurrentState == State.Play)
			{
				Vector3 cameraRight = Blocksworld.cameraRight;
				Vector3 vec = -Blocksworld.cameraForward;
				Vector3 radarCenter = this.GetRadarCenter();
				Vector2 b = new Vector2(radarCenter.x, radarCenter.z);
				Vector2 center = this.radarRect.center;
				float num = 30f;
				float num2 = this.radarRect.width * 0.45f;
				Vector3 normalized = Util.ProjectOntoPlane(cameraRight, Vector3.up).normalized;
				Vector3 normalized2 = Util.ProjectOntoPlane(vec, Vector3.up).normalized;
				Vector2 lhs = new Vector2(normalized.x, normalized.z);
				Vector2 lhs2 = new Vector2(normalized2.x, normalized2.z);
				Dictionary<string, int> dictionary = new Dictionary<string, int>();
				this.GetSortedBlockList(radarCenter);
				foreach (KeyValuePair<string, List<Block>> keyValuePair2 in this.sortedBlockList)
				{
					string key = keyValuePair2.Key;
					foreach (Block block in keyValuePair2.Value)
					{
						if (block.go.activeInHierarchy)
						{
							Vector3 position = block.goT.position;
							Vector2 a = new Vector2(position.x, position.z);
							Vector2 rhs = a - b;
							float num3 = this.maxDistance * this.maxDistance;
							float num4 = num3 * 0.8f * 0.8f;
							bool flag = rhs.sqrMagnitude < num3;
							bool flag2 = rhs.sqrMagnitude > num4;
							if (flag || this.alwaysSee)
							{
								int value = 1;
								if (!dictionary.ContainsKey(key))
								{
									dictionary[key] = value;
								}
								else
								{
									Dictionary<string, int> dictionary2;
									string key2;
									(dictionary2 = dictionary)[key2 = key] = dictionary2[key2] + 1;
								}
								Tile tile = this.GetTile(key, dictionary);
								hashSet.Remove(tile);
								Vector2 a2 = new Vector2(Vector2.Dot(lhs, rhs), -Vector2.Dot(lhs2, rhs));
								Vector2 vector = a2 / this.maxDistance;
								Vector2 a3 = center + new Vector2(vector.x * num2, vector.y * num2);
								Rect rect = new Rect(a3.x, a3.y, num, num);
								float num5 = TreasureHandler.GetTreasureModelScale(block);
								if (!flag)
								{
									Vector2 a4 = a3 - center;
									Vector2 a5 = a4 / a4.magnitude;
									rect.position = center + a5 * num2;
									num5 *= 0.5f;
								}
								else if (flag2)
								{
									float t = (num3 - rhs.sqrMagnitude) / (num3 - num4);
									num5 = Mathf.Lerp(0.5f, 1f, t);
								}
								float num6 = position.y - radarCenter.y;
								float z = 0f;
								Color color = this.GetColor(key);
								tile.Show(true);
								if (tile.tileObject != null)
								{
									tile.tileObject.SetScale(Vector3.one * num5);
									tile.SetTileForegroundColor(color);
									rect.position -= Vector2.one * num5 * num + Vector2.one * 2f;
									tile.SmoothMoveTo(new Vector3(rect.position.x, rect.position.y, z));
								}
							}
						}
					}
				}
				this.sweepMesh.localRotation = Quaternion.AngleAxis(this.sweepAngle, Vector3.up);
			}
			else
			{
				this.ActivateRadar(false);
			}
			this.minY = 0f;
			this.maxY = 0f;
			foreach (Tile tile2 in hashSet)
			{
				tile2.Show(false);
			}
		}

		// Token: 0x04000B79 RID: 2937
		private const float defaultMaxDistance = 50f;

		// Token: 0x04000B7A RID: 2938
		protected float maxDistance = 50f;

		// Token: 0x04000B7B RID: 2939
		protected HashSet<string> prevFollowedTags = new HashSet<string>();

		// Token: 0x04000B7C RID: 2940
		protected HashSet<string> followedTags = new HashSet<string>();

		// Token: 0x04000B7D RID: 2941
		protected string centerTag;

		// Token: 0x04000B7E RID: 2942
		protected HashSet<string> guiFollowedTags = new HashSet<string>();

		// Token: 0x04000B7F RID: 2943
		private static Dictionary<string, Color> tagColors = new Dictionary<string, Color>();

		// Token: 0x04000B80 RID: 2944
		private static Dictionary<string, string> tagNameCompares = new Dictionary<string, string>();

		// Token: 0x04000B81 RID: 2945
		private static Dictionary<string, List<Tile>> tagTiles = new Dictionary<string, List<Tile>>();

		// Token: 0x04000B82 RID: 2946
		public static Predicate predicateRadarTrackTag;

		// Token: 0x04000B83 RID: 2947
		public static Predicate predicateRadarCenterOnTag;

		// Token: 0x04000B84 RID: 2948
		private bool radarActive;

		// Token: 0x04000B85 RID: 2949
		private bool alwaysSee;

		// Token: 0x04000B86 RID: 2950
		private Vector2 backgroundPosition;

		// Token: 0x04000B87 RID: 2951
		private float minY;

		// Token: 0x04000B88 RID: 2952
		private float maxY;

		// Token: 0x04000B89 RID: 2953
		private Rect radarRect;

		// Token: 0x04000B8A RID: 2954
		private Rect textRect;

		// Token: 0x04000B8B RID: 2955
		private HudMeshStyle style;

		// Token: 0x04000B8C RID: 2956
		private HudMeshStyle outlineStyle;

		// Token: 0x04000B8D RID: 2957
		protected HudMeshLabel textLabel;

		// Token: 0x04000B8E RID: 2958
		protected HudMeshLabel textOutlineLabel;

		// Token: 0x04000B8F RID: 2959
		protected Color textColor = Color.white;

		// Token: 0x04000B90 RID: 2960
		private Transform sweepMesh;

		// Token: 0x04000B91 RID: 2961
		private float sweepAngle;

		// Token: 0x04000B92 RID: 2962
		private static Dictionary<string, string> defaultTagColorNames = new Dictionary<string, string>
		{
			{
				"0",
				"Orange"
			},
			{
				"1",
				"Yellow"
			},
			{
				"2",
				"Yellow Green"
			},
			{
				"3",
				"Green"
			},
			{
				"4",
				"Teal"
			},
			{
				"5",
				"Blue"
			},
			{
				"6",
				"Magenta"
			},
			{
				"7",
				"Purple"
			},
			{
				"8",
				"Pink"
			},
			{
				"A",
				"White"
			},
			{
				"B",
				"Deep Red"
			}
		};

		// Token: 0x04000B93 RID: 2963
		private const int MAX_BLIPS = 25;

		// Token: 0x04000B94 RID: 2964
		private Dictionary<string, List<Block>> sortedBlockList = new Dictionary<string, List<Block>>();

		// Token: 0x04000B95 RID: 2965
		private const float HEIGHT_DISTANCE_CHECK = 5f;

		// Token: 0x04000B96 RID: 2966
		private const float HEIGHT_COLOR_MULTIPLIER = 0.15f;
	}
}
