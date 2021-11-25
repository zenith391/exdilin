using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Gestures;
using UnityEngine;
using Exdilin;
using System.Reflection;

namespace Blocks
{
	// Token: 0x0200004B RID: 75
	public class Block : ITBox, ILightChanger
	{
		// Token: 0x06000263 RID: 611 RVA: 0x0000DD6C File Offset: 0x0000C16C
		public Block(List<List<Tile>> tiles)
		{
			this.tiles = tiles;
			this.isTerrain = (this is BlockSky || this is BlockTerrain || this is BlockAbstractWater || this is BlockBillboard);
			string blockType = this.BlockType();
			this.go = Blocksworld.InstantiateBlockGo(blockType);
			this.goT = this.go.transform;
			MeshFilter component = this.go.GetComponent<MeshFilter>();
			SkinnedMeshRenderer component2 = this.go.GetComponent<SkinnedMeshRenderer>();
			if (component != null || component2 != null)
			{
				if (component2 != null)
				{
					component2.sharedMesh = UnityEngine.Object.Instantiate<Mesh>(component2.sharedMesh);
					this.mesh = component2.sharedMesh;
				}
				else if (component != null)
				{
					this.mesh = component.mesh;
				}
			}
			if (this.goT.childCount > 0)
			{
				this.childMeshes = new Dictionary<string, Mesh>();
				List<GameObject> list = new List<GameObject>();
				IEnumerator enumerator = this.goT.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						object obj = enumerator.Current;
						Transform transform = (Transform)obj;
						list.Add(transform.gameObject);
					}
				}
				finally
				{
					IDisposable disposable;
					if ((disposable = (enumerator as IDisposable)) != null)
					{
						disposable.Dispose();
					}
				}
				list.Sort(new GameObjectNameComparer());
				foreach (GameObject gameObject in list)
				{
					MeshFilter component3 = gameObject.GetComponent<MeshFilter>();
					SkinnedMeshRenderer component4 = gameObject.GetComponent<SkinnedMeshRenderer>();
					if (this.childMeshes.ContainsKey(gameObject.name))
					{
						BWLog.Error("Block contains duplicate child object names: " + gameObject.name);
					}
					if (component3 != null)
					{
						this.childMeshes[gameObject.name] = component3.mesh;
					}
					else if (component4 != null)
					{
						component4.sharedMesh = UnityEngine.Object.Instantiate<Mesh>(component4.sharedMesh);
						this.childMeshes[gameObject.name] = component4.sharedMesh;
					}
				}
			}
			this.renderer = this.go.GetComponent<Renderer>();
			if (this.renderer == null)
			{
				this.renderer = this.go.GetComponent<SkinnedMeshRenderer>();
			}
			this.goLayerAssignment = ((!this.isTerrain) ? Layer.Default : Layer.Terrain);
			this.go.layer = (int)this.goLayerAssignment;
			if (!Blocksworld.renderingShadows && this.HasShadow())
			{
				this.InstantiateShadow(Block.prefabShadow);
			}
			if (this is BlockSky)
			{
				Blocksworld.worldSky = (BlockSky)this;
			}
			if (this is BlockWater)
			{
				Blocksworld.worldOcean = this.go;
				Blocksworld.worldOceanBlock = (BlockWater)this;
			}
			this.go.name = Block.nextBlockId++.ToString();
			this.AddMissingScaleTo();
			this.FindSubMeshes();
			this.size = this.BlockSize();
			this.shadowSize = this.size;
			this.CalculateMaxExtent();
			string text = (string)tiles[0][1].gaf.Args[0];
			string paint = "Yellow";
			string texture = "Plain";
			switch (text)
			{
			case "Wheel":
			case "Spoked Wheel":
				paint = "Black";
				break;
			case "Legs":
				paint = "White";
				break;
			case "Motor":
			case "Torsion Spring":
			case "Torsion Spring Slab":
				paint = "Black";
				break;
			case "Laser":
			case "Stabilizer":
				paint = "Grey";
				break;
			}
			for (int i = 0; i < this.subMeshes.Count; i++)
			{
				int meshIndex = i + 1;
				this.SetSubmeshInitPaintToTile(meshIndex, paint, true);
				this.SetSubmeshInitTextureToTile(meshIndex, texture, Vector3.up, true);
			}
			this.UpdateRuntimeInvisible();
			this.Reset(false);
		}

		// Token: 0x06000264 RID: 612 RVA: 0x0000E3D8 File Offset: 0x0000C7D8
		public List<Block> ConnectionsOfType(int connectionType, bool directed = false)
		{
			List<Block> list = new List<Block>();
			for (int i = 0; i < this.connections.Count; i++)
			{
				if ((Mathf.Abs(this.connectionTypes[i]) & connectionType) != 0 && (!directed || this.connectionTypes[i] >= 0))
				{
					list.Add(this.connections[i]);
				}
			}
			return list;
		}

		// Token: 0x17000041 RID: 65
		// (get) Token: 0x06000265 RID: 613 RVA: 0x0000E44A File Offset: 0x0000C84A
		// (set) Token: 0x06000266 RID: 614 RVA: 0x0000E452 File Offset: 0x0000C852
		public string currentPaint { get; private set; }

        // Token: 0x06000267 RID: 615 RVA: 0x0000E45C File Offset: 0x0000C85C
        public static void Register()
        {
            Block.predicateMoveTo = PredicateRegistry.Add<Block>("Block.MoveTo", null, (Block b) => new PredicateActionDelegate(b.MoveToAction), new Type[]
            {
                typeof(Vector3)
            }, null, null);
            Block.predicateRotateTo = PredicateRegistry.Add<Block>("Block.RotateTo", null, (Block b) => new PredicateActionDelegate(b.RotateToAction), new Type[]
            {
                typeof(Vector3)
            }, null, null);
            Block.predicateScaleTo = PredicateRegistry.Add<Block>("Block.ScaleTo", null, (Block b) => new PredicateActionDelegate(b.ScaleToAction), new Type[]
            {
                typeof(Vector3)
            }, null, null);
            Block.predicateTextureTo = PredicateRegistry.Add<Block>("Block.TextureTo", (Block b) => new PredicateSensorDelegate(b.IsTexturedTo), (Block b) => new PredicateActionDelegate(b.TextureToAction), new Type[]
            {
                typeof(string),
                typeof(Vector3),
                typeof(int)
            }, new string[]
            {
                "Texture name",
                "Normal",
                "Submesh index"
            }, null);
            Block.predicatePaintTo = PredicateRegistry.Add<Block>("Block.PaintTo", (Block b) => new PredicateSensorDelegate(b.IsPaintedTo), (Block b) => new PredicateActionDelegate(b.PaintToAction), new Type[]
            {
                typeof(string),
                typeof(int)
            }, new string[]
            {
                "Paint name",
                "Submesh index"
            }, null);
            Block.predicateGroup = PredicateRegistry.Add<Block>("Block.Group", null, (Block b) => new PredicateActionDelegate(b.IgnoreAction), new Type[]
            {
                typeof(int),
                typeof(string),
                typeof(int)
            }, new string[]
            {
                "Group ID",
                "Group type",
                "Main block"
            }, null);
            Block.predicateSetFog = PredicateRegistry.Add<Block>("Block.SetFog", null, (Block b) => new PredicateActionDelegate(b.SetFogAction), new Type[]
            {
                typeof(string),
                typeof(string),
                typeof(string)
            }, null, null);
            PredicateRegistry.Add<Block>("Block.PaintSkyTo", (Block b) => new PredicateSensorDelegate(b.IsSkyPaintedTo), (Block b) => new PredicateActionDelegate(b.PaintSkyToAction), new Type[]
            {
                typeof(string),
                typeof(int)
            }, null, null);
            PredicateRegistry.Add<Block>("Error", (Block b) => new PredicateSensorDelegate(b.IgnoreSensor), (Block b) => new PredicateActionDelegate(b.IgnoreAction), new Type[]
            {
                typeof(string)
            }, new string[]
            {
                "Message"
            }, null);
            Block.predicateGameOver = PredicateRegistry.Add<Block>("Meta.GameOver", null, (Block b) => new PredicateActionDelegate(b.GameOver), new Type[]
            {
                typeof(string),
                typeof(float),
                typeof(string)
            }, new string[]
            {
                "Message",
                "Duration",
                "SFX"
            }, null);
            Block.predicateGameWin = PredicateRegistry.Add<Block>("Meta.GameWin", null, (Block b) => new PredicateActionDelegate(b.GameWin), new Type[]
            {
                typeof(string),
                typeof(float),
                typeof(string)
            }, new string[]
            {
                "Message",
                "Duration",
                "SFX"
            }, null);
            Block.predicateGameLose = PredicateRegistry.Add<Block>("Meta.GameLose", null, (Block b) => new PredicateActionDelegate(b.GameLose), new Type[]
            {
                typeof(string),
                typeof(float),
                typeof(string)
            }, new string[]
            {
                "Message",
                "Duration",
                "SFX"
            }, null);
            Block.noTilesAfterPredicates.Add(Block.predicateGameOver);
            Block.noTilesAfterPredicates.Add(Block.predicateGameWin);
            Block.noTilesAfterPredicates.Add(Block.predicateGameLose);
            Block.predicateTag = PredicateRegistry.Add<Block>("Position.Position", null, (Block b) => new PredicateActionDelegate(b.RegisterTag), new Type[]
            {
                typeof(string)
            }, null, null);
            Block.predicateWithinTaggedBlock = PredicateRegistry.Add<Block>("Position.IsWithin", (Block b) => new PredicateSensorDelegate(b.WithinTaggedBlock), null, new Type[]
            {
                typeof(string)
            }, null, null);
            Block.predicateCustomTag = PredicateRegistry.Add<Block>("Block.CustomTag", null, (Block b) => new PredicateActionDelegate(b.RegisterTag), new Type[]
            {
                typeof(string)
            }, null, null);
            PredicateRegistry.Add<Block>("Variables.GlobalBooleanVariableEquals", delegate (Block b)
            {
                if (Block.f__mg_cache0 == null)
                {
                    Block.f__mg_cache0 = new PredicateSensorDelegate(VariableManager.GlobalBooleanVariableValueEquals);
                }
                return Block.f__mg_cache0;
            }, delegate(Block b)
			{
				if (Block.f__mg_cache1 == null)
				{
					Block.f__mg_cache1 = new PredicateActionDelegate(VariableManager.SetGlobalBooleanVariableValue);
				}
				return Block.f__mg_cache1;
			}, new Type[]
			{
				typeof(string),
				typeof(int)
			}, new string[]
			{
				"Variable name",
				"Value"
			}, null);
			PredicateRegistry.Add<Block>("Variables.GlobalIntegerVariableEquals", delegate(Block b)
			{
				if (Block.f__mg_cache2 == null)
				{
					Block.f__mg_cache2 = new PredicateSensorDelegate(VariableManager.GlobalIntegerVariableValueEquals);
				}
				return Block.f__mg_cache2;
			}, delegate(Block b)
			{
				if (Block.f__mg_cache3 == null)
				{
					Block.f__mg_cache3 = new PredicateActionDelegate(VariableManager.SetGlobalIntegerVariableValue);
				}
				return Block.f__mg_cache3;
			}, new Type[]
			{
				typeof(string),
				typeof(int)
			}, new string[]
			{
				"Variable name",
				"Value"
			}, null);
			PredicateRegistry.Add<Block>("Variables.RandomizeGlobalIntegerVariable", null, delegate(Block b)
			{
				if (Block.f__mg_cache4 == null)
				{
					Block.f__mg_cache4 = new PredicateActionDelegate(VariableManager.RandomizeGlobalIntegerVariable);
				}
				return Block.f__mg_cache4;
			}, new Type[]
			{
				typeof(string),
				typeof(int),
				typeof(int)
			}, new string[]
			{
				"Variable name",
				"Min (inclusive)",
				"Max (exclusive)"
			}, null);
			PredicateRegistry.Add<Block>("Variables.IncrementIntegerVariable", null, delegate(Block b)
			{
				if (Block.f__mg_cache5 == null)
				{
					Block.f__mg_cache5 = new PredicateActionDelegate(VariableManager.IncrementIntegerVariable);
				}
				return Block.f__mg_cache5;
			}, new Type[]
			{
				typeof(string),
				typeof(int),
				typeof(int)
			}, new string[]
			{
				"Variable name",
				"Increment"
			}, null);
			PredicateRegistry.Add<Block>("Block.InCameraFrustum", (Block b) => new PredicateSensorDelegate(b.InCameraFrustum), null, null, null, null);
			PredicateRegistry.Add<Block>("GaugeUI.Equals", (Block b) => new PredicateSensorDelegate(b.GaugeUIValueEquals), (Block b) => new PredicateActionDelegate(b.GaugeUISetValue), new Type[]
			{
				typeof(int),
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<Block>("GaugeUI.Fraction", (Block b) => new PredicateSensorDelegate(b.GaugeUIGetFraction), null, new Type[]
			{
				typeof(float),
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<Block>("GaugeUI.Increment", null, (Block b) => new PredicateActionDelegate(b.GaugeUIIncrementValue), new Type[]
			{
				typeof(int),
				typeof(int)
			}, null, null);
			Block.predicateObjectCounterIncrement = PredicateRegistry.Add<Block>("ObjectCounterUI.Increment", null, (Block b) => new PredicateActionDelegate(b.IncrementObjectCounterUI), new Type[]
			{
				typeof(int),
				typeof(int)
			}, new string[]
			{
				"Increment",
				"Counter Index"
			}, null);
			Block.predicateObjectCounterDecrement = PredicateRegistry.Add<Block>("ObjectCounterUI.Decrement", null, (Block b) => new PredicateActionDelegate(b.DecrementObjectCounterUI), new Type[]
			{
				typeof(int),
				typeof(int)
			}, new string[]
			{
				"Decrement",
				"Counter Index"
			}, null);
			Block.predicateObjectCounterEquals = PredicateRegistry.Add<Block>("ObjectCounterUI.Equals", (Block b) => new PredicateSensorDelegate(b.ObjectCounterUIValueEquals), (Block b) => new PredicateActionDelegate(b.ObjectCounterUISetValue), new Type[]
			{
				typeof(int),
				typeof(int)
			}, new string[]
			{
				"Value",
				"Counter Index"
			}, null);
			Block.predicateObjectCounterEqualsMax = PredicateRegistry.Add<Block>("ObjectCounterUI.EqualsMax", (Block b) => new PredicateSensorDelegate(b.ObjectCounterUIValueEqualsMaxValue), (Block b) => new PredicateActionDelegate(b.ObjectCounterUISetValueToMax), new Type[]
			{
				typeof(int)
			}, new string[]
			{
				"Counter Index"
			}, null);
			PredicateRegistry.Add<Block>("GaugeUI.EqualsMax", (Block b) => new PredicateSensorDelegate(b.GaugeUIValueEqualsMaxValue), (Block b) => new PredicateActionDelegate(b.GaugeUISetValueToMax), new Type[]
			{
				typeof(int)
			}, new string[]
			{
				"Gauge Index"
			}, null);
			PredicateRegistry.Add<Block>("TimerUI.EqualsMax", (Block b) => new PredicateSensorDelegate(b.TimerUIValueEqualsMaxValue), (Block b) => new PredicateActionDelegate(b.TimerUISetValueToMax), new Type[]
			{
				typeof(int)
			}, new string[]
			{
				"Timer Index"
			}, null);
			PredicateRegistry.Add<Block>("CounterUI.EqualsMin", (Block b) => new PredicateSensorDelegate(b.CounterUIValueEqualsMinValue), (Block b) => new PredicateActionDelegate(b.CounterUISetValueToMin), new Type[]
			{
				typeof(int)
			}, new string[]
			{
				"Counter Index"
			}, null);
			PredicateRegistry.Add<Block>("CounterUI.EqualsMax", (Block b) => new PredicateSensorDelegate(b.CounterUIValueEqualsMaxValue), (Block b) => new PredicateActionDelegate(b.CounterUISetValueToMax), new Type[]
			{
				typeof(int)
			}, new string[]
			{
				"Counter Index"
			}, null);
			Block.predicateObjectCounterValueCondition = PredicateRegistry.Add<Block>("ObjectCounterUI.ValueCondition", (Block b) => new PredicateSensorDelegate(b.ObjectCounterUIValueCondition), null, new Type[]
			{
				typeof(int),
				typeof(int),
				typeof(int)
			}, new string[]
			{
				"Value",
				"0: lt, 1: gt, 2: !=",
				"Counter index"
			}, null);
			Block.predicateCounterEquals = PredicateRegistry.Add<Block>("CounterUI.Equals", (Block b) => new PredicateSensorDelegate(b.CounterUIValueEquals), (Block b) => new PredicateActionDelegate(b.SetCounterUIValue), new Type[]
			{
				typeof(int),
				typeof(int)
			}, new string[]
			{
				"Value",
				"Counter Index"
			}, null);
			PredicateRegistry.Add<Block>("CounterUI.Increment", null, (Block b) => new PredicateActionDelegate(b.IncrementCounterUIValue), new Type[]
			{
				typeof(int),
				typeof(int)
			}, new string[]
			{
				"Value",
				"Counter Index"
			}, null);
			PredicateRegistry.Add<Block>("CounterUI.AnimateScore", null, (Block b) => new PredicateActionDelegate(b.CounterUIAnimateScore), new Type[]
			{
				typeof(int),
				typeof(int)
			}, new string[]
			{
				"Animation type",
				"Counter Index"
			}, null);
			PredicateRegistry.Add<Block>("CounterUI.ScoreMultiplier", null, (Block b) => new PredicateActionDelegate(b.CounterUIScoreMultiplier), new Type[]
			{
				typeof(int),
				typeof(int),
				typeof(int)
			}, new string[]
			{
				"Multiplier",
				"Positive only",
				"Counter Index"
			}, null);
			PredicateRegistry.Add<Block>("CounterUI.GlobalScoreMultiplier", null, (Block b) => new PredicateActionDelegate(b.CounterUIGlobalScoreMultiplier), new Type[]
			{
				typeof(int),
				typeof(int),
				typeof(int)
			}, new string[]
			{
				"Multiplier",
				"Positive only",
				"Counter Index"
			}, null);
			PredicateRegistry.Add<Block>("CounterUI.Randomize", null, (Block b) => new PredicateActionDelegate(b.RandomizeCounterUI), new Type[]
			{
				typeof(int),
				typeof(int),
				typeof(int)
			}, new string[]
			{
				"Min",
				"Max",
				"Counter Index"
			}, null);
			Block.predicateCounterValueCondition = PredicateRegistry.Add<Block>("CounterUI.ValueCondition", (Block b) => new PredicateSensorDelegate(b.CounterUIValueCondition), null, new Type[]
			{
				typeof(int),
				typeof(int),
				typeof(int)
			}, new string[]
			{
				"Value",
				"0: lt, 1: gt, 2: !=",
				"Counter index"
			}, null);
			Block.predicateGaugeValueCondition = PredicateRegistry.Add<Block>("GaugeUI.ValueCondition", (Block b) => new PredicateSensorDelegate(b.GaugeUIValueCondition), null, new Type[]
			{
				typeof(int),
				typeof(int),
				typeof(int)
			}, new string[]
			{
				"Value",
				"0: lt, 1: gt, 2: !=",
				"Gauge index"
			}, null);
			PredicateRegistry.Add<Block>("TimerUI.Start", null, (Block b) => new PredicateActionDelegate(b.TimerUIStartTimer), new Type[]
			{
				typeof(int),
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<Block>("TimerUI.Pause", null, (Block b) => new PredicateActionDelegate(b.TimerUIPauseTimer), new Type[]
			{
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<Block>("TimerUI.PauseUI", null, (Block b) => new PredicateActionDelegate(b.TimerUIPauseUI), new Type[]
			{
				typeof(float),
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<Block>("TimerUI.Wait", null, (Block b) => new PredicateActionDelegate(b.TimerUIWaitTimer), new Type[]
			{
				typeof(float),
				typeof(int)
			}, null, null);
			Block.predicateTimerValueCondition = PredicateRegistry.Add<Block>("TimerUI.ValueCondition", (Block b) => new PredicateSensorDelegate(b.TimerUIValueCondition), null, new Type[]
			{
				typeof(float),
				typeof(int),
				typeof(int)
			}, new string[]
			{
				"Value",
				"0: lt, 1: gt, 2: !=",
				"Timer index"
			}, null);
			Block.predicateTimerEquals = PredicateRegistry.Add<Block>("TimerUI.Equals", (Block b) => new PredicateSensorDelegate(b.TimerUITimeEquals), (Block b) => new PredicateActionDelegate(b.TimerUISetTime), new Type[]
			{
				typeof(float),
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<Block>("TimerUI.Increment", null, (Block b) => new PredicateActionDelegate(b.TimerUIIncrementTime), new Type[]
			{
				typeof(float),
				typeof(int)
			}, null, null);
			Block.predicateNegate = PredicateRegistry.Add<Block>("Meta.Negate", (Block b) => new PredicateSensorDelegate(b.IgnoreSensor), null, null, null, null);
			Block.predicateNegateMod = PredicateRegistry.Add<Block>("Meta.NegateModifier", (Block b) => new PredicateSensorDelegate(b.IgnoreSensor), null, null, null, null);
			Block.predicateCreate = PredicateRegistry.Add<Block>("Block.Create", null, null, new Type[]
			{
				typeof(string)
			}, null, null);
			Block.predicateCreateModel = PredicateRegistry.Add<Block>("Model.Create", null, null, new Type[]
			{
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<Block>("Button", null, null, new Type[]
			{
				typeof(string)
			}, null, null);
			PredicateRegistry.Add<Block>("Control", null, null, new Type[]
			{
				typeof(string)
			}, null, null);
			PredicateRegistry.Add<Block>("Radar", null, null, new Type[]
			{
				typeof(string)
			}, null, null);
			PredicateRegistry.Add<Block>("Key", null, null, new Type[]
			{
				typeof(string)
			}, null, null);
			PredicateRegistry.Add<Block>("Hand", null, null, new Type[]
			{
				typeof(string)
			}, null, null);
			PredicateRegistry.Add<Block>("TileOverlay", null, null, new Type[]
			{
				typeof(string)
			}, null, null);
			Block.predicateUI = PredicateRegistry.Add<Block>("UI", null, null, new Type[]
			{
				typeof(string)
			}, null, null);
			Block.predicateUIOpaque = PredicateRegistry.Add<Block>("UI Opaque", null, null, null, null, null);
			Block.predicateThen = PredicateRegistry.Add<Block>("Meta.Then", null, null, null, null, null);
			Block.predicateWait = PredicateRegistry.Add<Block>("Meta.Wait", null, (Block b) => new PredicateActionDelegate(b.Wait), null, null, null);
			Block.predicateWaitTime = PredicateRegistry.Add<Block>("Meta.WaitTime", null, (Block b) => new PredicateActionDelegate(b.WaitTime), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Seconds"
			}, null);
			PredicateRegistry.Add<Block>("Meta.RandomWaitTime", (Block b) => new PredicateSensorDelegate(b.RandomWaitTimeSensor), (Block b) => new PredicateActionDelegate(b.RandomWaitTime), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Seconds"
			}, null);
			PredicateRegistry.Add<Block>("Meta.StopScriptsModelForTime", null, (Block b) => new PredicateActionDelegate(b.StopScriptsModelForTime), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<Block>("Meta.StopScriptsModel", null, (Block b) => new PredicateActionDelegate(b.StopScriptsModel), null, null, null);
			PredicateRegistry.Add<Block>("Meta.StopScriptsBlock", null, (Block b) => new PredicateActionDelegate(b.StopScriptsBlock), null, null, null);
			Block.predicateStop = PredicateRegistry.Add<Block>("Meta.Stop", null, null, null, null, null);
			PredicateRegistry.Add<Block>("Meta.LockInput", null, (Block b) => new PredicateActionDelegate(b.LockInput), null, null, null);
			Block.predicateHideTileRow = PredicateRegistry.Add<Block>("Meta.HideTileRow", null, (Block b) => new PredicateActionDelegate(b.IgnoreAction), null, null, null);
			Block.predicateHideNextTile = PredicateRegistry.Add<Block>("Meta.HideNextTile", (Block b) => new PredicateSensorDelegate(b.IgnoreSensor), (Block b) => new PredicateActionDelegate(b.IgnoreAction), null, null, null);
			PredicateRegistry.Add<Block>("Meta.TileRowHint", null, (Block b) => new PredicateActionDelegate(b.IgnoreAction), new Type[]
			{
				typeof(string)
			}, new string[]
			{
				"Hint text"
			}, null);
			PredicateRegistry.Add<Block>("Meta.TestSensor", (Block b) => new PredicateSensorDelegate(b.TestSensor), null, new Type[]
			{
				typeof(string),
				typeof(string)
			}, new string[]
			{
				"Sensor Name",
				"Data"
			}, null);
			PredicateRegistry.Add<Block>("Meta.TestAction", null, (Block b) => new PredicateActionDelegate(b.TestAction), new Type[]
			{
				typeof(string),
				typeof(string)
			}, new string[]
			{
				"Action Name",
				"Data"
			}, null);
			PredicateRegistry.Add<Block>("Meta.TestObjective", (Block b) => new PredicateSensorDelegate(b.IsTestObjectiveDone), (Block b) => new PredicateActionDelegate(b.SetTestObjectiveDone), new Type[]
			{
				typeof(string)
			}, new string[]
			{
				"Objective name"
			}, null);
			Block.predicateSectionIndex = PredicateRegistry.Add<Block>("Section.SectionIndex", null, (Block b) => new PredicateActionDelegate(b.IgnoreAction), new Type[]
			{
				typeof(int)
			}, new string[]
			{
				"Section Index"
			}, null);
			Block.predicateIsTreasure = PredicateRegistry.Add<Block>("Block.IsTreasure", null, (Block b) => new PredicateActionDelegate(b.SetAsTreasureModel), new Type[]
			{
				typeof(int)
			}, new string[]
			{
				"Counter Index"
			}, null);
			Block.predicateIsTreasureForTag = PredicateRegistry.Add<Block>("Block.IsTreasureForTag", null, (Block b) => new PredicateActionDelegate(b.SetAsTreasureModelTag), new Type[]
			{
				typeof(string),
				typeof(int)
			}, new string[]
			{
				"Tag name",
				"Counter Index"
			}, null);
			Block.predicateIsPickup = PredicateRegistry.Add<Block>("Block.IsPickup", null, (Block b) => new PredicateActionDelegate(b.SetAsPickupModel), null, null, null);
			Block.predicateIsPickupForTag = PredicateRegistry.Add<Block>("Block.IsPickupForTag", null, (Block b) => new PredicateActionDelegate(b.SetAsPickupModelTag), new Type[]
			{
				typeof(string)
			}, new string[]
			{
				"Tag name"
			}, null);
			PredicateRegistry.Add<Block>("Block.RespawnPickup", null, (Block b) => new PredicateActionDelegate(b.RespawnPickup), null, null, null);
			PredicateRegistry.Add<Block>("Block.OnCollect", (Block b) => new PredicateSensorDelegate(b.OnCollect), (Block b) => new PredicateActionDelegate(b.ForceCollectPickup), null, null, null);
			PredicateRegistry.Add<Block>("Block.OnCollectByTag", (Block b) => new PredicateSensorDelegate(b.OnCollectByTag), (Block b) => new PredicateActionDelegate(b.ForceCollectPickup), new Type[]
			{
				typeof(string)
			}, new string[]
			{
				"Tag name"
			}, null);
			PredicateRegistry.Add<Block>("Block.SetAsTreasureBlockIcon", null, (Block b) => new PredicateActionDelegate(b.SetAsTreasureBlockIcon), new Type[]
			{
				typeof(int)
			}, new string[]
			{
				"Counter Index"
			}, null);
			PredicateRegistry.Add<Block>("Block.SetAsTreasureTextureIcon", null, (Block b) => new PredicateActionDelegate(b.SetAsTreasureTextureIcon), new Type[]
			{
				typeof(int)
			}, new string[]
			{
				"Counter Index"
			}, null);
			PredicateRegistry.Add<Block>("Block.SetAsCounterUIBlockIcon", null, (Block b) => new PredicateActionDelegate(b.SetAsCounterUIBlockIcon), new Type[]
			{
				typeof(int)
			}, new string[]
			{
				"Counter Index"
			}, null);
			PredicateRegistry.Add<Block>("Block.SetAsCounterUITextureIcon", null, (Block b) => new PredicateActionDelegate(b.SetAsCounterUITextureIcon), new Type[]
			{
				typeof(int)
			}, new string[]
			{
				"Counter Index"
			}, null);
			PredicateRegistry.Add<Block>("Block.SetAsTimerUIBlockIcon", null, (Block b) => new PredicateActionDelegate(b.SetAsTimerUIBlockIcon), new Type[]
			{
				typeof(int)
			}, new string[]
			{
				"Timer Index"
			}, null);
			PredicateRegistry.Add<Block>("Block.SetAsTimerUITextureIcon", null, (Block b) => new PredicateActionDelegate(b.SetAsTimerUITextureIcon), new Type[]
			{
				typeof(int)
			}, new string[]
			{
				"Timer Index"
			}, null);
			PredicateRegistry.Add<Block>("Block.SetAsGaugeUIBlockIcon", null, (Block b) => new PredicateActionDelegate(b.SetAsGaugeUIBlockIcon), new Type[]
			{
				typeof(int)
			}, new string[]
			{
				"Gauge Index"
			}, null);
			PredicateRegistry.Add<Block>("Block.SetAsGaugeUITextureIcon", null, (Block b) => new PredicateActionDelegate(b.SetAsGaugeUITextureIcon), new Type[]
			{
				typeof(int)
			}, new string[]
			{
				"Gauge Index"
			}, null);
			Block.predicateWithinWater = PredicateRegistry.Add<Block>("Water.IsWithin", (Block b) => new PredicateSensorDelegate(b.IsWithinWater), null, null, null, null);
			Block.predicateModelWithinWater = PredicateRegistry.Add<Block>("Water.IsWithinModel", (Block b) => new PredicateSensorDelegate(b.IsAnyBlockInModelWithinWater), null, null, null, null);
			PredicateRegistry.Add<Block>("Water.IsWithinChunk", (Block b) => new PredicateSensorDelegate(b.IsAnyBlockInChunkWithinWater), null, null, null, null);
			PredicateRegistry.Add<Block>("Block.IsSpeedLess", (Block b) => new PredicateSensorDelegate(b.IsSlowerThan), null, new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<Block>("Block.IsSpeedMore", (Block b) => new PredicateSensorDelegate(b.IsFasterThan), null, new Type[]
			{
				typeof(float)
			}, null, null);
			Block.predicateWithinTaggedWater = PredicateRegistry.Add<Block>("Block.WithinTaggedWater", (Block b) => new PredicateSensorDelegate(b.WithinTaggedWater), null, new Type[]
			{
				typeof(string)
			}, new string[]
			{
				"Tag"
			}, null);
			PredicateRegistry.Add<Block>("Block.WithinTaggedWaterChunk", (Block b) => new PredicateSensorDelegate(b.WithinTaggedWaterChunk), null, new Type[]
			{
				typeof(string)
			}, new string[]
			{
				"Tag"
			}, null);
			Block.predicateModelWithinTaggedWater = PredicateRegistry.Add<Block>("Block.WithinTaggedWaterModel", (Block b) => new PredicateSensorDelegate(b.WithinTaggedWaterModel), null, new Type[]
			{
				typeof(string)
			}, new string[]
			{
				"Tag"
			}, null);
			PredicateRegistry.Add<Block>("Block.BumpTarget", null, null, null, null, null);
			Block.predicateLocked = PredicateRegistry.Add<Block>("Block.Locked", null, (Block b) => new PredicateActionDelegate(b.IgnoreAction), null, null, null);
			Block.predicateUnlocked = PredicateRegistry.Add<Block>("Block.Unlocked", null, (Block b) => new PredicateActionDelegate(b.IgnoreAction), null, null, null);
			PredicateRegistry.Add<Block>("Block.Win", null, (Block b) => new PredicateActionDelegate(b.Win), null, null, null);
			PredicateRegistry.Add<Block>("Block.IncreaseCounter", null, (Block b) => new PredicateActionDelegate(b.IncreaseCounter), new Type[]
			{
				typeof(string)
			}, new string[]
			{
				"Counter name"
			}, null);
			PredicateRegistry.Add<Block>("Block.DecreaseCounter", null, (Block b) => new PredicateActionDelegate(b.DecreaseCounter), new Type[]
			{
				typeof(string)
			}, new string[]
			{
				"Counter name"
			}, null);
			PredicateRegistry.Add<Block>("Block.CounterEquals", (Block b) => new PredicateSensorDelegate(b.CounterEquals), (Block b) => new PredicateActionDelegate(b.SetCounter), new Type[]
			{
				typeof(string),
				typeof(string)
			}, new string[]
			{
				"Counter name",
				"Value"
			}, null);
			PredicateRegistry.Add<Block>("Block.Target", null, (Block b) => new PredicateActionDelegate(b.Target), null, null, null);
			PredicateRegistry.Add<Block>("Block.Inventory", null, (Block b) => new PredicateActionDelegate(b.IgnoreAction), null, null, null);
			PredicateRegistry.Add<Block>("Block.VisualizeReward", null, (Block b) => new PredicateActionDelegate(b.VisualizeReward), new Type[]
			{
				typeof(string),
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<Block>("Block.PlaySound", null, (Block b) => new PredicateActionDelegate(b.PlaySound), new Type[]
			{
				typeof(string),
				typeof(string),
				typeof(string),
				typeof(float),
				typeof(float)
			}, null, null);
			Block.predicatePlaySoundDurational = PredicateRegistry.Add<Block>("Block.PlaySoundDurational", (Block b) => new PredicateSensorDelegate(b.SoundSensor), (Block b) => new PredicateActionDelegate(b.PlaySoundDurational), new Type[]
			{
				typeof(string),
				typeof(string),
				typeof(string),
				typeof(float),
				typeof(float)
			}, new string[]
			{
				"SFX name",
				"Location"
			}, null);
			PredicateRegistry.Add<Block>("Block.Mute", null, (Block b) => new PredicateActionDelegate(b.Mute), null, null, null);
			PredicateRegistry.Add<Block>("Block.MuteModel", null, (Block b) => new PredicateActionDelegate(b.MuteModel), null, null, null);
			PredicateRegistry.Add<Block>("Block.PlayVfxDurational", null, (Block b) => new PredicateActionDelegate(b.PlayVfxDurational), new Type[]
			{
				typeof(string),
				typeof(float),
				typeof(int),
				typeof(string)
			}, null, null);
			PredicateRegistry.Add<Block>("Block.TeleportToTag", null, (Block b) => new PredicateActionDelegate(b.TeleportToTag), new Type[]
			{
				typeof(string),
				typeof(int),
				typeof(int),
				typeof(int),
				typeof(int)
			}, new string[]
			{
				"Tag name",
				"Reset angle",
				"Reset velocity",
				"Reset ang vel",
				"Find free"
			}, null);
			PredicateRegistry.Add<Block>("Block.TagProximityCheck", (Block b) => new PredicateSensorDelegate(b.TagProximityCheck), null, new Type[]
			{
				typeof(string),
				typeof(float),
				typeof(int)
			}, new string[]
			{
				"Tag name",
				"Distance",
				"Trigger below"
			}, null);
			PredicateRegistry.Add<Block>("Block.TagVisibilityCheck", (Block b) => new PredicateSensorDelegate(b.TagVisibilityCheck), null, new Type[]
			{
				typeof(string),
				typeof(int)
			}, new string[]
			{
				"Tag name",
				"Trigger invisible"
			}, null);
			PredicateRegistry.Add<Block>("Block.TagVisibilityCheckModel", (Block b) => new PredicateSensorDelegate(b.TagVisibilityCheckModel), null, new Type[]
			{
				typeof(string),
				typeof(int)
			}, new string[]
			{
				"Tag name",
				"Trigger invisible"
			}, null);
			PredicateRegistry.Add<Block>("Block.TagFrustumCheck", (Block b) => new PredicateSensorDelegate(b.TagFrustumCheck), null, new Type[]
			{
				typeof(string),
				typeof(int),
				typeof(float),
				typeof(float),
				typeof(Vector3)
			}, null, null);
			Block.predicateAnimate = PredicateRegistry.Add<Block>("Block.Animate", null, (Block b) => new PredicateActionDelegate(b.Animate), new Type[]
			{
				typeof(int),
				typeof(string)
			}, null, null);
			Block.predicateTutorialHelpTextAction = PredicateRegistry.Add<Block>("Block.TutorialHelpTextAction", (Block b) => new PredicateSensorDelegate(b.IgnoreSensor), (Block b) => new PredicateActionDelegate(b.IgnoreAction), new Type[]
			{
				typeof(string),
				typeof(string),
				typeof(Vector3),
				typeof(float),
				typeof(string),
				typeof(string),
				typeof(string),
				typeof(string),
				typeof(string),
				typeof(float)
			}, new string[]
			{
				typeof(TutorialActionContext).Name,
				"Text",
				"Position",
				"Width",
				"Pose Name",
				"Buttons",
				"SFX",
				"Highlights",
				"Tiles",
				"Lifetime"
			}, null);
			Block.predicateTutorialCreateBlockHint = PredicateRegistry.Add<Block>("Block.TutorialCreateBlockHint", null, (Block b) => new PredicateActionDelegate(b.TutorialCreateBlockHint), new Type[]
			{
				typeof(Vector3),
				typeof(Vector3),
				typeof(Vector3),
				typeof(float),
				typeof(int),
				typeof(int),
				typeof(int),
				typeof(Vector3),
				typeof(int),
				typeof(int),
				typeof(int)
			}, new string[]
			{
				"World target position",
				"World target normal",
				"Camera euler angles",
				"Camera distance",
				"Rotate before scale",
				"Paint before pose",
				"Texture before pose",
				"Auto angles filter",
				"Use two-finger scale",
				"Use two-finger move",
				"Add default tiles"
			}, null);
			Block.predicateTutorialRemoveBlockHint = PredicateRegistry.Add<Block>("Block.TutorialRemoveBlockHint", null, (Block b) => new PredicateActionDelegate(b.IgnoreAction), new Type[]
			{
				typeof(int),
				typeof(Vector3),
				typeof(Vector3)
			}, new string[]
			{
				"Block index",
				"Camera position",
				"Camera lookat position"
			}, null);
			Block.predicateTutorialAutoAddBlock = PredicateRegistry.Add<Block>("Block.TutorialAutoAddBlock", null, (Block b) => new PredicateActionDelegate(b.IgnoreAction), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Wait time"
			}, null);
			Block.predicateTutorialPaintExistingBlock = PredicateRegistry.Add<Block>("Block.TutorialPaintExistingBlock", null, (Block b) => new PredicateActionDelegate(b.TutorialPaintExistingBlock), new Type[]
			{
				typeof(int),
				typeof(string),
				typeof(Vector3),
				typeof(Vector3)
			}, new string[]
			{
				"Block index",
				"Paint name",
				"Camera position",
				"Camera lookat position"
			}, null);
			Block.predicateTutorialTextureExistingBlock = PredicateRegistry.Add<Block>("Block.TutorialTextureExistingBlock", null, (Block b) => new PredicateActionDelegate(b.TutorialTextureExistingBlock), new Type[]
			{
				typeof(int),
				typeof(string),
				typeof(Vector3),
				typeof(Vector3)
			}, new string[]
			{
				"Block index",
				"Texture name",
				"Camera position",
				"Camera lookat position"
			}, null);
			Block.predicateTutorialRotateExistingBlock = PredicateRegistry.Add<Block>("Block.TutorialRotateExistingBlock", null, (Block b) => new PredicateActionDelegate(b.TutorialRotateExistingBlock), new Type[]
			{
				typeof(int),
				typeof(Vector3),
				typeof(Vector3),
				typeof(Vector3)
			}, new string[]
			{
				"Block index",
				"Angles",
				"Camera position",
				"Camera lookat position"
			}, null);
			Block.predicateTutorialOperationPose = PredicateRegistry.Add<Block>("Block.TutorialOperationPose", null, (Block b) => new PredicateActionDelegate(b.IgnoreAction), new Type[]
			{
				typeof(string),
				typeof(int),
				typeof(string)
			}, new string[]
			{
				"Tutorial State",
				"Mesh index",
				"Pose Name"
			}, null);
			Block.predicateTutorialMoveBlock = PredicateRegistry.Add<Block>("Block.TutorialMoveBlock", null, (Block b) => new PredicateActionDelegate(b.IgnoreAction), new Type[]
			{
				typeof(Vector3)
			}, new string[]
			{
				"Position"
			}, null);
			Block.predicateTutorialMoveModel = PredicateRegistry.Add<Block>("Block.TutorialMoveModel", null, (Block b) => new PredicateActionDelegate(b.IgnoreAction), new Type[]
			{
				typeof(Vector3)
			}, new string[]
			{
				"Position"
			}, null);
			Block.predicateTutorialHideInBuildMode = PredicateRegistry.Add<Block>("Block.TutorialHideInBuildMode", null, (Block b) => new PredicateActionDelegate(b.IgnoreAction), null, null, null);
			Block.predicateTiltLeftRight = PredicateRegistry.Add<Block>("Block.DeviceTilt", (Block b) => new PredicateSensorDelegate(b.IsTiltedLeftRight), null, new Type[]
			{
				typeof(float),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<Block>("Block.TiltCamera", null, (Block b) => new PredicateActionDelegate(b.TiltCamera), new Type[]
			{
				typeof(float)
			}, null, null);
			Block.predicateTiltFrontBack = PredicateRegistry.Add<Block>("Block.DeviceTiltFrontBack", (Block b) => new PredicateSensorDelegate(b.IsTiltedFrontBack), null, new Type[]
			{
				typeof(float),
				typeof(float)
			}, null, null);
			Block.predicateButton = PredicateRegistry.Add<Block>("Block.ButtonInput", (Block b) => new PredicateSensorDelegate(b.IsReceivingButtonInput), null, new Type[]
			{
				typeof(string)
			}, null, null);
			Block.predicateSendSignal = PredicateRegistry.Add<Block>("Block.SendSignal", (Block b) => new PredicateSensorDelegate(b.IsSendingSignal), (Block b) => new PredicateActionDelegate(b.SendSignal), new Type[]
			{
				typeof(int),
				typeof(float)
			}, new string[]
			{
				"Signal index",
				"Signal strength"
			}, null);
			Block.predicateSendSignalModel = PredicateRegistry.Add<Block>("Block.SendSignalModel", (Block b) => new PredicateSensorDelegate(b.IsSendingSignalModel), (Block b) => new PredicateActionDelegate(b.SendSignalModel), new Type[]
			{
				typeof(int),
				typeof(float)
			}, new string[]
			{
				"Signal index",
				"Signal strength"
			}, null);
			Block.predicateSendCustomSignal = PredicateRegistry.Add<Block>("Block.SendCustomSignal", (Block b) => new PredicateSensorDelegate(b.IsSendingCustomSignal), (Block b) => new PredicateActionDelegate(b.SendCustomSignal), new Type[]
			{
				typeof(string),
				typeof(float)
			}, new string[]
			{
				"Signal name",
				"Signal strength"
			}, null);
			Block.predicateSendSignal.canHaveOverlay = true;
			Block.predicateSendCustomSignalModel = PredicateRegistry.Add<Block>("Block.SendCustomSignalModel", (Block b) => new PredicateSensorDelegate(b.IsSendingCustomSignalModel), (Block b) => new PredicateActionDelegate(b.SendCustomSignalModel), new Type[]
			{
				typeof(string),
				typeof(float)
			}, new string[]
			{
				"Signal name",
				"Signal strength"
			}, null);
			Block.predicateSendCustomSignalModel.canHaveOverlay = true;
			PredicateRegistry.Add<Block>("Block.GameStart", (Block b) => new PredicateSensorDelegate(b.IsGameStart), null, null, null, null);
			Block.predicateDPadHorizontal = PredicateRegistry.Add<Block>("Block.DPadHorizontal", (Block b) => new PredicateSensorDelegate(b.IsDPadHorizontal), null, new Type[]
			{
				typeof(float),
				typeof(string)
			}, new string[]
			{
				"Sign",
				"DPad Name"
			}, null);
			Block.predicateDPadVertical = PredicateRegistry.Add<Block>("Block.DPadVertical", (Block b) => new PredicateSensorDelegate(b.IsDPadVertical), null, new Type[]
			{
				typeof(float),
				typeof(string)
			}, new string[]
			{
				"Sign",
				"DPad Name"
			}, null);
			Block.predicateDPadMoved = PredicateRegistry.Add<Block>("Block.DPadMoved", (Block b) => new PredicateSensorDelegate(b.IsDPadMoved), null, new Type[]
			{
				typeof(string)
			}, new string[]
			{
				"DPad Name"
			}, null);
			Block.predicateTapBlock = PredicateRegistry.Add<Block>("Block.Tap", (Block b) => new PredicateSensorDelegate(b.IsTappingBlock), null, null, null, null);
			Block.predicateTapModel = PredicateRegistry.Add<Block>("Block.TapModel", (Block b) => new PredicateSensorDelegate(b.IsTappingAnyBlockInModel), null, null, null, null);
			Block.predicateTapChunk = PredicateRegistry.Add<Block>("Block.TapChunk", (Block b) => new PredicateSensorDelegate(b.IsTappingAnyBlockInChunk), null, null, null, null);
			Block.predicateTapHoldBlock = PredicateRegistry.Add<Block>("Block.TapHold", (Block b) => new PredicateSensorDelegate(b.IsTapHoldingBlock), null, null, null, null);
			Block.predicateTapHoldModel = PredicateRegistry.Add<Block>("Block.TapHoldModel", (Block b) => new PredicateSensorDelegate(b.IsTapHoldingAnyBlockInModel), null, null, null, null);
			PredicateRegistry.Add<Block>("Block.GlueOnContact", (Block b) => new PredicateSensorDelegate(b.IsGlueOnContact), (Block b) => new PredicateActionDelegate(b.GlueOnContact), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Size Factor"
			}, null);
			PredicateRegistry.Add<Block>("Block.GlueOnContactChunk", null, (Block b) => new PredicateActionDelegate(b.GlueOnContactChunk), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Size Factor"
			}, null);
			PredicateRegistry.Add<Block>("Block.AllowGlueOnContact", (Block b) => new PredicateSensorDelegate(b.IsAllowGlueOnContact), (Block b) => new PredicateActionDelegate(b.AllowGlueOnContact), new Type[]
			{
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<Block>("Block.ReleaseGlueOnContact", null, (Block b) => new PredicateActionDelegate(b.ReleaseGlueOnContact), null, null, null);
			Block.predicateBump = PredicateRegistry.Add<Block>("Block.Bump", (Block b) => new PredicateSensorDelegate(b.IsBumping), null, new Type[]
			{
				typeof(string)
			}, null, null);
			Block.predicateBumpModel = PredicateRegistry.Add<Block>("Block.BumpModel", (Block b) => new PredicateSensorDelegate(b.IsBumpingAnyBlockInModel), null, new Type[]
			{
				typeof(string)
			}, null, null);
			Block.predicateImpact = PredicateRegistry.Add<Block>("Block.OnImpact", (Block b) => new PredicateSensorDelegate(b.OnImpact), null, null, null, null);
			Block.predicateImpactModel = PredicateRegistry.Add<Block>("Block.OnImpactModel", (Block b) => new PredicateSensorDelegate(b.OnImpactModel), null, null, null, null);
			Block.predicateParticleImpact = PredicateRegistry.Add<Block>("Block.OnParticleImpact", (Block b) => new PredicateSensorDelegate(b.OnParticleImpact), null, new Type[]
			{
				typeof(int)
			}, null, null);
			Block.predicateParticleImpactModel = PredicateRegistry.Add<Block>("Block.OnParticleImpactModel", (Block b) => new PredicateSensorDelegate(b.OnParticleImpactModel), null, new Type[]
			{
				typeof(int)
			}, null, null);
			Block.predicateBumpChunk = PredicateRegistry.Add<Block>("Block.BumpChunk", (Block b) => new PredicateSensorDelegate(b.IsBumpingAnyBlockInChunk), null, new Type[]
			{
				typeof(string)
			}, null, null);
			Block.predicateTaggedBump = PredicateRegistry.Add<Block>("Block.TaggedBump", (Block b) => new PredicateSensorDelegate(b.IsBumpingTag), null, new Type[]
			{
				typeof(string)
			}, new string[]
			{
				"Tag name"
			}, null);
			Block.predicateTaggedBumpModel = PredicateRegistry.Add<Block>("Block.TaggedBumpModel", (Block b) => new PredicateSensorDelegate(b.IsBumpingAnyTaggedBlockInModel), null, new Type[]
			{
				typeof(string)
			}, new string[]
			{
				"Tag name"
			}, null);
			Block.predicateTaggedBumpChunk = PredicateRegistry.Add<Block>("Block.TaggedBumpChunk", (Block b) => new PredicateSensorDelegate(b.IsBumpingAnyTaggedBlockInChunk), null, new Type[]
			{
				typeof(string)
			}, new string[]
			{
				"Tag name"
			}, null);
			Block.predicatePulledByMagnet = PredicateRegistry.Add<Block>("Block.PulledByMagnet", (Block b) => new PredicateSensorDelegate(b.IsPulledByMagnet), null, null, null, null);
			Block.predicatePulledByMagnetModel = PredicateRegistry.Add<Block>("Block.PulledByMagnetModel", (Block b) => new PredicateSensorDelegate(b.IsPulledByMagnetModel), null, null, null, null);
			Block.predicatePushedByMagnet = PredicateRegistry.Add<Block>("Block.PushedByMagnet", (Block b) => new PredicateSensorDelegate(b.IsPushedByMagnet), null, null, null, null);
			Block.predicatePushedByMagnetModel = PredicateRegistry.Add<Block>("Block.PushedByMagnetModel", (Block b) => new PredicateSensorDelegate(b.IsPushedByMagnetModel), null, null, null, null);
			PredicateRegistry.Add<Block>("Block.HitByBlocksterHandAttachment", (Block b) => new PredicateSensorDelegate(b.IsHitByBlocksterHandAttachment), null, null, null, null);
			PredicateRegistry.Add<Block>("Block.HitByTaggedBlocksterHandAttachment", (Block b) => new PredicateSensorDelegate(b.IsHitByTaggedBlocksterHandAttachment), null, new Type[]
			{
				typeof(string)
			}, null, null);
			PredicateRegistry.Add<Block>("Block.ModelHitByBlocksterHandAttachment", (Block b) => new PredicateSensorDelegate(b.IsHitByBlocksterHandAttachmentModel), null, null, null, null);
			PredicateRegistry.Add<Block>("Block.ModelHitByTaggedBlocksterHandAttachment", (Block b) => new PredicateSensorDelegate(b.IsHitByTaggedBlocksterHandAttachmentModel), null, new Type[]
			{
				typeof(string)
			}, null, null);
			PredicateRegistry.Add<Block>("Block.HitByProjectile", (Block b) => new PredicateSensorDelegate(b.IsHitByProjectile), null, null, null, null);
			PredicateRegistry.Add<Block>("Block.TaggedHitByProjectile", (Block b) => new PredicateSensorDelegate(b.IsHitByTaggedProjectile), null, new Type[]
			{
				typeof(string)
			}, null, null);
			PredicateRegistry.Add<Block>("Block.HitByTaggedProjectileModel", (Block b) => new PredicateSensorDelegate(b.IsHitByTaggedProjectileModel), null, new Type[]
			{
				typeof(string)
			}, null, null);
			PredicateRegistry.Add<Block>("Block.HitByTaggedProjectileChunk", (Block b) => new PredicateSensorDelegate(b.IsHitByTaggedProjectileChunk), null, new Type[]
			{
				typeof(string)
			}, null, null);
			PredicateRegistry.Add<Block>("Block.HitByProjectileModel", (Block b) => new PredicateSensorDelegate(b.IsHitByProjectileModel), null, null, null, null);
			PredicateRegistry.Add<Block>("Block.HitByProjectileChunk", (Block b) => new PredicateSensorDelegate(b.IsHitByProjectileChunk), null, null, null, null);
			PredicateRegistry.Add<Block>("Laser.HitByBeam", (Block b) => new PredicateSensorDelegate(b.IsHitByPulseOrBeam), null, null, null, null);
			PredicateRegistry.Add<Block>("Laser.HitByPulse", (Block b) => new PredicateSensorDelegate(b.IsHitByPulseOrBeam), null, null, null, null);
			PredicateRegistry.Add<Block>("Laser.TaggedHitByBeam", (Block b) => new PredicateSensorDelegate(b.IsHitByTaggedPulseOrBeam), null, new Type[]
			{
				typeof(string)
			}, null, null);
			PredicateRegistry.Add<Block>("Block.HitByLaserModel", (Block b) => new PredicateSensorDelegate(b.IsHitByLaserModel), null, null, null, null);
			PredicateRegistry.Add<Block>("Block.HitByLaserChunk", (Block b) => new PredicateSensorDelegate(b.IsHitByLaserChunk), null, null, null, null);
			PredicateRegistry.Add<Block>("Block.HitByTaggedLaserModel", (Block b) => new PredicateSensorDelegate(b.IsHitByTaggedLaserModel), null, new Type[]
			{
				typeof(string)
			}, null, null);
			PredicateRegistry.Add<Block>("Block.HitByTaggedLaserChunk", (Block b) => new PredicateSensorDelegate(b.IsHitByTaggedLaserChunk), null, new Type[]
			{
				typeof(string)
			}, null, null);
			PredicateRegistry.Add<Block>("Block.HitByArrow", (Block b) => new PredicateSensorDelegate(b.IsHitByArrow), null, null, null, null);
			PredicateRegistry.Add<Block>("Block.HitByTaggedArrow", (Block b) => new PredicateSensorDelegate(b.IsHitByTaggedArrow), null, new Type[]
			{
				typeof(string)
			}, null, null);
			PredicateRegistry.Add<Block>("Block.ModelHitByArrow", (Block b) => new PredicateSensorDelegate(b.IsHitByArrowModel), null, null, null, null);
			PredicateRegistry.Add<Block>("Block.ModelHitByTaggedArrow", (Block b) => new PredicateSensorDelegate(b.IsHitByTaggedArrowModel), null, new Type[]
			{
				typeof(string)
			}, null, null);
			PredicateRegistry.Add<Block>("Block.Teleported", (Block b) => new PredicateSensorDelegate(b.Teleported), null, null, null, null);
			PredicateRegistry.Add<Block>("Block.Trigger", (Block b) => new PredicateSensorDelegate(b.IsTriggering), null, null, null, null);
			PredicateRegistry.Add<Block>("Block.TriggerModel", (Block b) => new PredicateSensorDelegate(b.IsAnyBlockInModelTriggering), null, null, null, null);
			PredicateRegistry.Add<Block>("Block.TriggerChunk", (Block b) => new PredicateSensorDelegate(b.IsAnyBlockInChunkTriggering), null, null, null, null);
			PredicateRegistry.Add<Block>("Block.SetTrigger", null, (Block b) => new PredicateActionDelegate(b.SetTrigger), null, null, null);
			PredicateRegistry.Add<Block>("Block.SetTriggerModel", null, (Block b) => new PredicateActionDelegate(b.SetEveryBlockInModelAsTrigger), null, null, null);
			PredicateRegistry.Add<Block>("Block.SetTriggerChunk", null, (Block b) => new PredicateActionDelegate(b.SetEveryBlockInChunkAsTrigger), null, null, null);
			Block.predicateFirstFrame = PredicateRegistry.Add<Block>("Block.FirstFrame", (Block b) => new PredicateSensorDelegate(b.IsFirstFrame), null, null, null, null);
			Block.predicateFreeze = PredicateRegistry.Add<Block>("Block.Fixed", (Block b) => new PredicateSensorDelegate(b.IsFrozen), (Block b) => new PredicateActionDelegate(b.Freeze), null, null, null);
			Block.predicateUnfreeze = PredicateRegistry.Add<Block>("Block.Unfreeze", (Block b) => new PredicateSensorDelegate(b.IsNotFrozen), (Block b) => new PredicateActionDelegate(b.Unfreeze), null, null, null);
			PredicateRegistry.Add<Block>("Block.Phantom", (Block b) => new PredicateSensorDelegate(b.IsPhantom), (Block b) => new PredicateActionDelegate(b.SetPhantom), null, null, null);
			PredicateRegistry.Add<Block>("Block.Unphantom", (Block b) => new PredicateSensorDelegate(b.IsNotPhantom), (Block b) => new PredicateActionDelegate(b.SetUnphantom), null, null, null);
			PredicateRegistry.Add<Block>("Block.PhantomModel", (Block b) => new PredicateSensorDelegate(b.IsPhantomModel), (Block b) => new PredicateActionDelegate(b.SetPhantomModel), null, null, null);
			PredicateRegistry.Add<Block>("Block.UnphantomModel", (Block b) => new PredicateSensorDelegate(b.IsNotPhantomModel), (Block b) => new PredicateActionDelegate(b.SetUnphantomModel), null, null, null);

			/// ADDED BY EXDILIN ///

			PredicateRegistry.Add<Block>("Block.Clone", null, (Block b) => new PredicateActionDelegate(delegate (ScriptRowExecutionInfo eInfo, object[] args)
			{
				Block block = new Block(b.tiles);
				BWSceneManager.AddTempBlock(block);
				Vector3 vec = b.goT.position;
				vec.x += 2;
				block.goT.position = vec;
				return TileResultCode.True;
			}), null, null, null);
			BlockItemEntry entry = new BlockItemEntry();
			entry.argumentPatterns = new string[0];
			entry.buildPaneTab = "Actions & Scripting";
			entry.item = new BlockItem(9876, "clone", "Clone", "Clone", "Block.Clone", new object[0], "Yellow/Danger", "Yellow", RarityLevelEnum.common);
			Exdilin.BlockItemsRegistry.AddBlockItem(entry);

			/// ORIGINAL CODE ///
			PredicateRegistry.Add<Block>("Block.Explode", (Block b) => new PredicateSensorDelegate(b.IsExploded), (Block b) => new PredicateActionDelegate(b.Explode), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Strength Multiplier"
			}, null);
			PredicateRegistry.Add<Block>("Block.ExplodeLocal", (Block b) => new PredicateSensorDelegate(b.HitByExplosion), (Block b) => new PredicateActionDelegate(b.ExplodeLocal), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Radius"
			}, null);
			PredicateRegistry.Add<Block>("Block.SmashLocal", null, (Block b) => new PredicateActionDelegate(b.SmashLocal), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Radius"
			}, null);
			PredicateRegistry.Add<Block>("Block.Detach", (Block b) => new PredicateSensorDelegate(b.IsDetached), (Block b) => new PredicateActionDelegate(b.Detach), null, null, null);
			PredicateRegistry.Add<Block>("Block.ExplodeTag", (Block b) => new PredicateSensorDelegate(b.IsExploded), (Block b) => new PredicateActionDelegate(b.ExplodeTag), new Type[]
			{
				typeof(string),
				typeof(float)
			}, new string[]
			{
				"Tag",
				"Radius"
			}, null);
			PredicateRegistry.Add<Block>("Block.Invincible", (Block b) => new PredicateSensorDelegate(b.IsInvincible), (Block b) => new PredicateActionDelegate(b.SetInvincible), null, null, null);
			PredicateRegistry.Add<Block>("Block.InvincibleModel", (Block b) => new PredicateSensorDelegate(b.IsInvincibleModel), (Block b) => new PredicateActionDelegate(b.SetInvincibleModel), null, null, null);
			PredicateRegistry.Add<Block>("Prop.Attacking", (Block b) => new PredicateSensorDelegate(b.IsWeaponAttacking), null, null, null, null);
			PredicateRegistry.Add<Block>("Prop.Blocking", (Block b) => new PredicateSensorDelegate(b.IsShieldBlocking), null, null, null, null);
			PredicateRegistry.Add<Block>("Block.CameraFollow", null, (Block b) => new PredicateActionDelegate(b.CameraFollow), new Type[]
			{
				typeof(float),
				typeof(float),
				typeof(float),
				typeof(Vector3)
			}, null, null);
			PredicateRegistry.Add<Block>("Block.CameraFollow2D", null, (Block b) => new PredicateActionDelegate(b.CameraFollow2D), new Type[]
			{
				typeof(float),
				typeof(float)
			}, null, null);
			Block.predicateCamFollow = PredicateRegistry.Add<Block>("Block.CameraFollowLookToward", null, (Block b) => new PredicateActionDelegate(b.CameraFollowLookToward), new Type[]
			{
				typeof(float),
				typeof(float),
				typeof(float)
			}, new string[]
			{
				"Vel. resp.",
				"Smoothness",
				"Angle"
			}, null);
			PredicateRegistry.Add<Block>("Block.CameraFollowLookTowardTag", null, (Block b) => new PredicateActionDelegate(b.CameraFollowLookTowardTag), new Type[]
			{
				typeof(string),
				typeof(float),
				typeof(float)
			}, new string[]
			{
				"Tag Name",
				"Vel. resp.",
				"Smoothness"
			}, null);
			PredicateRegistry.Add<Block>("Block.CameraFollowThirdPersonPlatform", null, (Block b) => new PredicateActionDelegate(b.CameraFollowThirdPersonPlatform), null, null, null);
			PredicateRegistry.Add<Block>("Block.CameraMoveTo", null, (Block b) => new PredicateActionDelegate(b.CameraMoveTo), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<Block>("Block.CameraLookAt", null, (Block b) => new PredicateActionDelegate(b.CameraLookAt), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<Block>("Block.SetTargetCameraAngle", null, (Block b) => new PredicateActionDelegate(b.SetTargetCameraAngle), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<Block>("Block.SetVerticalDistanceOffsetFactor", null, (Block b) => new PredicateActionDelegate(b.SetVerticalDistanceOffsetFactor), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<Block>("Block.SetTargetFollowDistanceMultiplier", null, (Block b) => new PredicateActionDelegate(b.SetTargetFollowDistanceMultiplier), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<Block>("Block.CameraToNamedPose", null, (Block b) => new PredicateActionDelegate(b.CameraToNamedPose), new Type[]
			{
				typeof(string),
				typeof(float),
				typeof(float),
				typeof(float),
				typeof(int)
			}, new string[]
			{
				"Pose Name",
				"Move smoothness",
				"Aim smoothness",
				"Direction dist",
				"Move only"
			}, null);
			PredicateRegistry.Add<Block>("Block.CameraSpeedFoV", null, (Block b) => new PredicateActionDelegate(b.CameraSpeedFoV), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockCharacter>("Block.StartFirstPersonCamera", (Block b) => new PredicateSensorDelegate(b.IsFirstPersonBlock), (Block b) => new PredicateActionDelegate(b.FirstPersonCamera), new Type[]
			{
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<Block>("Block.HUDReticle", null, (Block b) => new PredicateActionDelegate(b.SetHudReticle), new Type[]
			{
				typeof(int)
			}, null, null);
			Block.predicateSpeak = PredicateRegistry.Add<Block>("Block.Speak", null, (Block b) => new PredicateActionDelegate(b.Speak), new Type[]
			{
				typeof(string)
			}, new string[]
			{
				"Text"
			}, null);
			PredicateRegistry.Add<Block>("Block.SpeakNonDurational", null, (Block b) => new PredicateActionDelegate(b.SpeakNonDurational), new Type[]
			{
				typeof(string),
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<Block>("Block.ShowTextWindow", null, (Block b) => new PredicateActionDelegate(b.ShowTextWindow), new Type[]
			{
				typeof(string),
				typeof(Vector3),
				typeof(float),
				typeof(string)
			}, new string[]
			{
				"Text",
				"Position",
				"Width",
				"Buttons"
			}, null);
			PredicateRegistry.Add<Block>("Block.BreakOff", (Block b) => new PredicateSensorDelegate(b.IsBrokenOff), (Block b) => new PredicateActionDelegate(b.BreakOff), null, null, null);
			PredicateRegistry.Add<Block>("Block.Vanish", (Block b) => new PredicateSensorDelegate(b.IsVanished), (Block b) => new PredicateActionDelegate(b.Vanish), null, null, null);
			PredicateRegistry.Add<Block>("Block.Appear", null, (Block b) => new PredicateActionDelegate(b.Appear), null, null, null);
			Block.predicateVanishBlock = PredicateRegistry.Add<Block>("Block.VanishBlock", (Block b) => new PredicateSensorDelegate(b.IsVanished), (Block b) => new PredicateActionDelegate(b.VanishBlock), new Type[]
			{
				typeof(int)
			}, new string[]
			{
				"Animate"
			}, null);
			PredicateRegistry.Add<Block>("Block.AppearBlock", (Block b) => new PredicateSensorDelegate(b.IsAppeared), (Block b) => new PredicateActionDelegate(b.AppearBlock), new Type[]
			{
				typeof(int)
			}, new string[]
			{
				"Animate"
			}, null);
			Block.predicateVanishModel = PredicateRegistry.Add<Block>("Block.VanishModel", (Block b) => new PredicateSensorDelegate(b.IsVanished), (Block b) => new PredicateActionDelegate(b.VanishModel), new Type[]
			{
				typeof(int)
			}, new string[]
			{
				"Animate"
			}, null);
			PredicateRegistry.Add<Block>("Block.AppearModel", (Block b) => new PredicateSensorDelegate(b.IsAppeared), (Block b) => new PredicateActionDelegate(b.AppearModel), new Type[]
			{
				typeof(int)
			}, new string[]
			{
				"Animate"
			}, null);
			PredicateRegistry.Add<Block>("Block.VanishModelForever", null, (Block b) => new PredicateActionDelegate(b.VanishModelForever), new Type[]
			{
				typeof(int),
				typeof(float)
			}, new string[]
			{
				"Animate",
				"Per block delay"
			}, null);
			Block.predicateLevitate = PredicateRegistry.Add<Block>("Block.LevitateAnimation", null, (Block b) => new PredicateActionDelegate(b.LevitateAnimation), null, null, null);
			PredicateRegistry.Add<Block>("Block.SetBackgroundMusic", null, (Block b) => new PredicateActionDelegate(b.SetBackgroundMusic), new Type[]
			{
				typeof(string)
			}, new string[]
			{
				"Music name"
			}, null);
			PredicateRegistry.Add<Block>("Meta.TileBackground", null, null, null, null, null);
			PredicateRegistry.Add<Block>("Meta.TileBackgroundWithLabel", null, null, null, null, null);
			PredicateRegistry.Add<Block>("Meta.ButtonBackground", null, null, null, null, null);
			PredicateRegistry.Add<Block>("Block.PullLockBlock", (Block b) => new PredicateSensorDelegate(b.IsPullLockedSensor), (Block b) => new PredicateActionDelegate(b.PullLockBlock), null, null, null);
			PredicateRegistry.Add<Block>("Block.PullLockChunk", (Block b) => new PredicateSensorDelegate(b.IsPullLockedSensor), (Block b) => new PredicateActionDelegate(b.PullLockChunk), null, null, null);
			PredicateRegistry.Add<Block>("Block.PullLockModel", (Block b) => new PredicateSensorDelegate(b.IsPullLockedSensor), (Block b) => new PredicateActionDelegate(b.PullLockModel), null, null, null);
			Block.predicateSetSpawnPoint = PredicateRegistry.Add<Block>("Block.SetSpawnpoint", null, (Block b) => new PredicateActionDelegate(b.SetSpawnpoint), new Type[]
			{
				typeof(int)
			}, new string[]
			{
				"Player Index"
			}, null);
			Block.predicateSetActiveCheckpoint = PredicateRegistry.Add<Block>("Block.SetActiveCheckpoint", null, (Block b) => new PredicateActionDelegate(b.SetActiveCheckpoint), new Type[]
			{
				typeof(int)
			}, new string[]
			{
				"Player Index"
			}, null);
			Block.predicateSpawn = PredicateRegistry.Add<Block>("Block.Spawn", null, (Block b) => new PredicateActionDelegate(b.Spawn), new Type[]
			{
				typeof(int),
				typeof(float)
			}, new string[]
			{
				"Player Index",
				"Duration"
			}, null);
			PredicateRegistry.Add<Block>("Block.SetBuoyancy", null, (Block b) => new PredicateActionDelegate(b.SetBuoyancy), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<Block>("Block.SetMass", null, (Block b) => new PredicateActionDelegate(b.SetMass), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<Block>("Block.SetFriction", null, (Block b) => new PredicateActionDelegate(b.SetFriction), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<Block>("Block.SetBounce", null, (Block b) => new PredicateActionDelegate(b.SetBounce), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<Block>("AnalogOp.Ceil", (Block b) => new PredicateSensorDelegate(b.AnalogCeil), null, null, null, null);
			PredicateRegistry.Add<Block>("AnalogOp.Floor", (Block b) => new PredicateSensorDelegate(b.AnalogFloor), null, null, null, null);
			PredicateRegistry.Add<Block>("AnalogOp.Round", (Block b) => new PredicateSensorDelegate(b.AnalogRound), null, null, null, null);
			PredicateRegistry.Add<Block>("AnalogOp.Min", (Block b) => new PredicateSensorDelegate(b.AnalogMin), null, new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Value"
			}, null);
			PredicateRegistry.Add<Block>("AnalogOp.Max", (Block b) => new PredicateSensorDelegate(b.AnalogMax), null, new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Value"
			}, null);
			PredicateRegistry.Add<Block>("AnalogOp.Clamp", (Block b) => new PredicateSensorDelegate(b.AnalogClamp), null, new Type[]
			{
				typeof(float),
				typeof(float)
			}, new string[]
			{
				"Lower Value",
				"Upper Value"
			}, null);
			Block.defaultExtraTiles["DNO Gas Canister"] = new List<List<Tile>>
			{
				new List<Tile>
				{
					new Tile(new GAF("Laser.HitByBeam", new object[0])),
					Block.ThenTile(),
					new Tile(new GAF("Block.ExplodeLocal", new object[]
					{
						8f
					}))
				},
				Block.EmptyTileRow()
			};
			foreach (Mod mod in ModLoader.mods) {
				mod.Register(RegisterType.PREDICATES);
			}

			foreach (PredicateEntry predicate in PredicateEntryRegistry.GetPredicateEntries().Values) {
				PredicateRegistry.AddTyped(predicate.blockType, predicate.id, predicate.sensorConstructor,
					predicate.actionConstructor, predicate.argTypes, predicate.argNames, null);
			}
			Block.SetupVariablePredicates();
			Block.SetupVRPredicates();
		}

		// Token: 0x06000268 RID: 616 RVA: 0x000131D8 File Offset: 0x000115D8
		public static void SetupVariablePredicates()
		{
			Block.predicateVariableCustomInt = PredicateRegistry.Add<Block>("Variable.CustomInt", null, (Block b) => new PredicateActionDelegate(b.VariableDeclare), new Type[]
			{
				typeof(string),
				typeof(int)
			}, new string[]
			{
				"Variable name",
				"Int value"
			}, null);
			Predicate predicate = PredicateRegistry.Add<Block>("Variable.CustomIntAdd", null, (Block b) => new PredicateActionDelegate(b.VariableAdd), new Type[]
			{
				typeof(string),
				typeof(int)
			}, new string[]
			{
				"Variable name",
				"Int value"
			}, null);
			Predicate predicate2 = PredicateRegistry.Add<Block>("Variable.CustomIntSubtract", null, (Block b) => new PredicateActionDelegate(b.VariableSubtract), new Type[]
			{
				typeof(string),
				typeof(int)
			}, new string[]
			{
				"Variable name",
				"Int value"
			}, null);
			Predicate predicate3 = PredicateRegistry.Add<Block>("Variable.CustomIntMultiply", null, (Block b) => new PredicateActionDelegate(b.VariableMultiply), new Type[]
			{
				typeof(string),
				typeof(int)
			}, new string[]
			{
				"Block variable name",
				"Int value"
			}, null);
			Predicate predicate4 = PredicateRegistry.Add<Block>("Variable.CustomIntDivide", null, (Block b) => new PredicateActionDelegate(b.VariableDivide), new Type[]
			{
				typeof(string),
				typeof(int)
			}, new string[]
			{
				"Block variable name",
				"Int value"
			}, null);
			Predicate predicate5 = PredicateRegistry.Add<Block>("Variable.CustomIntModulus", null, (Block b) => new PredicateActionDelegate(b.VariableModulus), new Type[]
			{
				typeof(string),
				typeof(int)
			}, new string[]
			{
				"Block variable name",
				"Int value"
			}, null);
			Predicate predicate6 = PredicateRegistry.Add<Block>("Variable.CustomIntEquals", (Block b) => new PredicateSensorDelegate(b.VariableEquals), (Block b) => new PredicateActionDelegate(b.VariableAssign), new Type[]
			{
				typeof(string),
				typeof(int)
			}, new string[]
			{
				"Variable name",
				"Int value"
			}, null);
			Predicate predicate7 = PredicateRegistry.Add<Block>("Variable.CustomIntNotEquals", (Block b) => new PredicateSensorDelegate(b.VariableNotEquals), null, new Type[]
			{
				typeof(string),
				typeof(int)
			}, new string[]
			{
				"Variable name",
				"Int value"
			}, null);
			Predicate predicate8 = PredicateRegistry.Add<Block>("Variable.CustomIntLessThan", (Block b) => new PredicateSensorDelegate(b.VariableLessThan), null, new Type[]
			{
				typeof(string),
				typeof(int)
			}, new string[]
			{
				"Variable name",
				"Int value"
			}, null);
			Predicate predicate9 = PredicateRegistry.Add<Block>("Variable.CustomIntMoreThan", (Block b) => new PredicateSensorDelegate(b.VariableMoreThan), null, new Type[]
			{
				typeof(string),
				typeof(int)
			}, new string[]
			{
				"Variable name",
				"Int value"
			}, null);
			Block.predicateBlockVariableInt = PredicateRegistry.Add<Block>("BlockVariable.Int", null, (Block b) => new PredicateActionDelegate(b.BlockVariableDeclare), new Type[]
			{
				typeof(string),
				typeof(int)
			}, new string[]
			{
				"Block variable name",
				"Int value"
			}, null);
			Predicate predicate10 = PredicateRegistry.Add<Block>("BlockVariable.IntAdd", null, (Block b) => new PredicateActionDelegate(b.BlockVariableAdd), new Type[]
			{
				typeof(string),
				typeof(int)
			}, new string[]
			{
				"Block variable name",
				"Int value"
			}, null);
			Predicate predicate11 = PredicateRegistry.Add<Block>("BlockVariable.IntSubtract", null, (Block b) => new PredicateActionDelegate(b.BlockVariableSubtract), new Type[]
			{
				typeof(string),
				typeof(int)
			}, new string[]
			{
				"Block variable name",
				"Int value"
			}, null);
			Predicate predicate12 = PredicateRegistry.Add<Block>("BlockVariable.IntMultiply", null, (Block b) => new PredicateActionDelegate(b.BlockVariableMultiply), new Type[]
			{
				typeof(string),
				typeof(int)
			}, new string[]
			{
				"Block variable name",
				"Int value"
			}, null);
			Predicate predicate13 = PredicateRegistry.Add<Block>("BlockVariable.IntDivide", null, (Block b) => new PredicateActionDelegate(b.BlockVariableDivide), new Type[]
			{
				typeof(string),
				typeof(int)
			}, new string[]
			{
				"Block variable name",
				"Int value"
			}, null);
			Predicate predicate14 = PredicateRegistry.Add<Block>("BlockVariable.IntModulus", null, (Block b) => new PredicateActionDelegate(b.BlockVariableModulus), new Type[]
			{
				typeof(string),
				typeof(int)
			}, new string[]
			{
				"Block variable name",
				"Int value"
			}, null);
			Predicate predicate15 = PredicateRegistry.Add<Block>("BlockVariable.IntEquals", (Block b) => new PredicateSensorDelegate(b.BlockVariableEquals), (Block b) => new PredicateActionDelegate(b.BlockVariableAssign), new Type[]
			{
				typeof(string),
				typeof(int)
			}, new string[]
			{
				"Block variable name",
				"Int value"
			}, null);
			Predicate predicate16 = PredicateRegistry.Add<Block>("BlockVariable.IntNotEquals", (Block b) => new PredicateSensorDelegate(b.BlockVariableNotEquals), null, new Type[]
			{
				typeof(string),
				typeof(int)
			}, new string[]
			{
				"Block variable name",
				"Int value"
			}, null);
			Predicate predicate17 = PredicateRegistry.Add<Block>("BlockVariable.IntLessThan", (Block b) => new PredicateSensorDelegate(b.BlockVariableLessThan), null, new Type[]
			{
				typeof(string),
				typeof(int)
			}, new string[]
			{
				"Block variable name",
				"Int value"
			}, null);
			Predicate predicate18 = PredicateRegistry.Add<Block>("BlockVariable.IntMoreThan", (Block b) => new PredicateSensorDelegate(b.BlockVariableMoreThan), null, new Type[]
			{
				typeof(string),
				typeof(int)
			}, new string[]
			{
				"Block variable name",
				"Int value"
			}, null);
			Predicate predicate19 = PredicateRegistry.Add<Block>("BlockVariable.IntRandom", null, (Block b) => new PredicateActionDelegate(b.BlockVariableAssignRandom), new Type[]
			{
				typeof(string),
				typeof(int)
			}, new string[]
			{
				"Block variable name",
				"Int value"
			}, null);
			Predicate predicate20 = PredicateRegistry.Add<Block>("BlockVariable.IntSpeed", null, (Block b) => new PredicateActionDelegate(b.BlockVariableAssignSpeed), new Type[]
			{
				typeof(string),
				typeof(int)
			}, new string[]
			{
				"Block variable name"
			}, null);
			Predicate predicate21 = PredicateRegistry.Add<Block>("BlockVariable.IntAltitude", null, (Block b) => new PredicateActionDelegate(b.BlockVariableAssignAltitude), new Type[]
			{
				typeof(string),
				typeof(int)
			}, new string[]
			{
				"Block variable name"
			}, null);
			Block.predicateBlockVariableIntLoadGlobal = PredicateRegistry.Add<Block>("BlockVariable.IntLoadGlobal", null, (Block b) => new PredicateActionDelegate(b.BlockVariableLoadGlobal), new Type[]
			{
				typeof(string),
				typeof(string)
			}, new string[]
			{
				"Block variable name",
				"Global variable name"
			}, null);
			Block.predicateBlockVariableIntStoreGlobal = PredicateRegistry.Add<Block>("BlockVariable.IntStoreGlobal", null, (Block b) => new PredicateActionDelegate(b.BlockVariableStoreGlobal), new Type[]
			{
				typeof(string),
				typeof(string)
			}, new string[]
			{
				"Block variable name",
				"Global variable name"
			}, null);
			Predicate predicate22 = PredicateRegistry.Add<Block>("BlockVariable.IntAddBV", null, (Block b) => new PredicateActionDelegate(b.BlockVariableAddBV), new Type[]
			{
				typeof(string),
				typeof(string)
			}, new string[]
			{
				"Block variable name",
				"Block variable name"
			}, null);
			Predicate predicate23 = PredicateRegistry.Add<Block>("BlockVariable.IntSubtractBV", null, (Block b) => new PredicateActionDelegate(b.BlockVariableSubtractBV), new Type[]
			{
				typeof(string),
				typeof(string)
			}, new string[]
			{
				"Block variable name",
				"Block variable name"
			}, null);
			Predicate predicate24 = PredicateRegistry.Add<Block>("BlockVariable.IntMultiplyBV", null, (Block b) => new PredicateActionDelegate(b.BlockVariableMultiplyBV), new Type[]
			{
				typeof(string),
				typeof(string)
			}, new string[]
			{
				"Block variable name",
				"Block variable name"
			}, null);
			Predicate predicate25 = PredicateRegistry.Add<Block>("BlockVariable.IntDivideBV", null, (Block b) => new PredicateActionDelegate(b.BlockVariableDivideBV), new Type[]
			{
				typeof(string),
				typeof(string)
			}, new string[]
			{
				"Block variable name",
				"Block variable name"
			}, null);
			Predicate predicate26 = PredicateRegistry.Add<Block>("BlockVariable.IntModulusBV", null, (Block b) => new PredicateActionDelegate(b.BlockVariableModulusBV), new Type[]
			{
				typeof(string),
				typeof(string)
			}, new string[]
			{
				"Block variable name",
				"Block variable name"
			}, null);
			Predicate predicate27 = PredicateRegistry.Add<Block>("BlockVariable.IntEqualsBV", (Block b) => new PredicateSensorDelegate(b.BlockVariableEqualsBV), (Block b) => new PredicateActionDelegate(b.BlockVariableAssignBV), new Type[]
			{
				typeof(string),
				typeof(string)
			}, new string[]
			{
				"Block variable name",
				"Block variable name"
			}, null);
			Predicate predicate28 = PredicateRegistry.Add<Block>("BlockVariable.IntNotEqualsBV", (Block b) => new PredicateSensorDelegate(b.BlockVariableNotEqualsBV), null, new Type[]
			{
				typeof(string),
				typeof(string)
			}, new string[]
			{
				"Block variable name",
				"Block variable name"
			}, null);
			Predicate predicate29 = PredicateRegistry.Add<Block>("BlockVariable.IntLessThanBV", (Block b) => new PredicateSensorDelegate(b.BlockVariableLessThanBV), null, new Type[]
			{
				typeof(string),
				typeof(string)
			}, new string[]
			{
				"Block variable name",
				"Block variable name"
			}, null);
			Predicate predicate30 = PredicateRegistry.Add<Block>("BlockVariable.IntMoreThanBV", (Block b) => new PredicateSensorDelegate(b.BlockVariableMoreThanBV), null, new Type[]
			{
				typeof(string),
				typeof(string)
			}, new string[]
			{
				"Block variable name",
				"Block variable name"
			}, null);
			Block.customVariablePredicates = new HashSet<Predicate>();
			Block.variablePredicateParamDefaults = new Dictionary<Predicate, int>();
			Block.customVariablePredicates.Add(Block.predicateVariableCustomInt);
			Block.customVariablePredicates.Add(Block.predicateBlockVariableInt);
			Block.globalVariableOperations = new HashSet<Predicate>();
			Block.globalVariableOperations.Add(predicate);
			Block.globalVariableOperations.Add(predicate2);
			Block.globalVariableOperations.Add(predicate3);
			Block.globalVariableOperations.Add(predicate4);
			Block.globalVariableOperations.Add(predicate5);
			Block.globalVariableOperations.Add(predicate6);
			Block.globalVariableOperations.Add(predicate7);
			Block.globalVariableOperations.Add(predicate8);
			Block.globalVariableOperations.Add(predicate9);
			Block.variablePredicateParamDefaults[predicate] = 1;
			Block.variablePredicateParamDefaults[predicate2] = 1;
			Block.variablePredicateParamDefaults[predicate3] = 2;
			Block.variablePredicateParamDefaults[predicate4] = 2;
			Block.variablePredicateParamDefaults[predicate5] = 2;
			Block.variablePredicateParamDefaults[predicate6] = 0;
			Block.variablePredicateParamDefaults[predicate7] = 0;
			Block.variablePredicateParamDefaults[predicate8] = 0;
			Block.variablePredicateParamDefaults[predicate9] = 0;
			Block.blockVariableOperations = new HashSet<Predicate>();
			Block.blockVariableOperations.Add(predicate10);
			Block.blockVariableOperations.Add(predicate11);
			Block.blockVariableOperations.Add(predicate12);
			Block.blockVariableOperations.Add(predicate13);
			Block.blockVariableOperations.Add(predicate14);
			Block.blockVariableOperations.Add(predicate15);
			Block.blockVariableOperations.Add(predicate16);
			Block.blockVariableOperations.Add(predicate17);
			Block.blockVariableOperations.Add(predicate18);
			Block.blockVariableOperations.Add(predicate19);
			Block.blockVariableOperations.Add(predicate20);
			Block.blockVariableOperations.Add(predicate21);
			Block.variablePredicateParamDefaults[predicate10] = 1;
			Block.variablePredicateParamDefaults[predicate11] = 1;
			Block.variablePredicateParamDefaults[predicate12] = 2;
			Block.variablePredicateParamDefaults[predicate13] = 2;
			Block.variablePredicateParamDefaults[predicate14] = 2;
			Block.variablePredicateParamDefaults[predicate15] = 0;
			Block.variablePredicateParamDefaults[predicate16] = 0;
			Block.variablePredicateParamDefaults[predicate17] = 0;
			Block.variablePredicateParamDefaults[predicate18] = 0;
			Block.variablePredicateParamDefaults[predicate19] = 10;
			Block.variablePredicateParamDefaults[predicate20] = 0;
			Block.variablePredicateParamDefaults[predicate21] = 0;
			Block.blockVariableOperationsOnGlobals = new HashSet<Predicate>();
			Block.blockVariableOperationsOnGlobals.Add(Block.predicateBlockVariableIntLoadGlobal);
			Block.blockVariableOperationsOnGlobals.Add(Block.predicateBlockVariableIntStoreGlobal);
			Block.blockVariableOperationsOnOtherBlockVars = new HashSet<Predicate>();
			Block.blockVariableOperationsOnOtherBlockVars.Add(predicate22);
			Block.blockVariableOperationsOnOtherBlockVars.Add(predicate23);
			Block.blockVariableOperationsOnOtherBlockVars.Add(predicate24);
			Block.blockVariableOperationsOnOtherBlockVars.Add(predicate25);
			Block.blockVariableOperationsOnOtherBlockVars.Add(predicate26);
			Block.blockVariableOperationsOnOtherBlockVars.Add(predicate27);
			Block.blockVariableOperationsOnOtherBlockVars.Add(predicate28);
			Block.blockVariableOperationsOnOtherBlockVars.Add(predicate29);
			Block.blockVariableOperationsOnOtherBlockVars.Add(predicate30);
			Block.variablePredicateLabels = new Dictionary<Predicate, string>();
			Block.variablePredicateLabels[predicate22] = " + ";
			Block.variablePredicateLabels[predicate23] = " - ";
			Block.variablePredicateLabels[predicate24] = " * ";
			Block.variablePredicateLabels[predicate25] = " / ";
			Block.variablePredicateLabels[predicate26] = " % ";
			Block.variablePredicateLabels[predicate27] = " = ";
			Block.variablePredicateLabels[predicate28] = " != ";
			Block.variablePredicateLabels[predicate29] = " < ";
			Block.variablePredicateLabels[predicate30] = " > ";
			Block.customVariablePredicates.UnionWith(Block.globalVariableOperations);
			Block.customVariablePredicates.UnionWith(Block.blockVariableOperations);
			Block.customVariablePredicates.UnionWith(Block.blockVariableOperationsOnGlobals);
			Block.customVariablePredicates.UnionWith(Block.blockVariableOperationsOnOtherBlockVars);
		}

		// Token: 0x06000269 RID: 617 RVA: 0x00014300 File Offset: 0x00012700
		public static string GetLabelForPredicate(Predicate p, string lhs, string rhs)
		{
			if (p == Block.predicateBlockVariableIntLoadGlobal)
			{
				return lhs + " <- " + rhs;
			}
			if (p == Block.predicateBlockVariableIntStoreGlobal)
			{
				return lhs + " -> " + rhs;
			}
			if (Block.variablePredicateLabels.ContainsKey(p))
			{
				return lhs + Block.variablePredicateLabels[p] + rhs;
			}
			return lhs;
		}

		// Token: 0x0600026A RID: 618 RVA: 0x00014364 File Offset: 0x00012764
		public static void SetupVRPredicates()
		{
			PredicateRegistry.Add<Block>("Block.VRCameraMode", (Block b) => new PredicateSensorDelegate(b.IsVRCameraMode), null, null, null, null);
			PredicateRegistry.Add<Block>("Block.IsVRCameraFocus", (Block b) => new PredicateSensorDelegate(b.IsVRFocus), null, null, null, null);
			PredicateRegistry.Add<Block>("Block.IsVRCameraFocusModel", (Block b) => new PredicateSensorDelegate(b.IsVRFocusModel), null, null, null, null);
			PredicateRegistry.Add<Block>("Block.IsVRCameraLookAt", (Block b) => new PredicateSensorDelegate(b.IsVRLookAt), null, null, null, null);
			PredicateRegistry.Add<Block>("Block.IsVRCameraLookAtModel", (Block b) => new PredicateSensorDelegate(b.IsVRLookAtModel), null, null, null, null);
		}

		// Token: 0x0600026B RID: 619 RVA: 0x0001444D File Offset: 0x0001284D
		public TileResultCode IsVRCameraMode(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!Blocksworld.IsVRCameraMode()) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x0600026C RID: 620 RVA: 0x00014460 File Offset: 0x00012860
		public TileResultCode IsVRFocus(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!Blocksworld.IsBlockVRCameraFocus(this)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x0600026D RID: 621 RVA: 0x00014474 File Offset: 0x00012874
		public TileResultCode IsVRFocusModel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			bool flag = false;
			this.UpdateConnectedCache();
			List<Block> list = Block.connectedCache[this];
			int num = 0;
			while (num < list.Count && !flag)
			{
				flag |= Blocksworld.IsBlockVRCameraFocus(list[num]);
				num++;
			}
			return (!flag) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x0600026E RID: 622 RVA: 0x000144CF File Offset: 0x000128CF
		public TileResultCode IsVRLookAt(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!Blocksworld.IsBlockVRCameraLookAt(this)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x0600026F RID: 623 RVA: 0x000144E4 File Offset: 0x000128E4
		public TileResultCode IsVRLookAtModel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			bool flag = false;
			this.UpdateConnectedCache();
			List<Block> list = Block.connectedCache[this];
			int num = 0;
			while (num < list.Count && !flag)
			{
				flag |= Blocksworld.IsBlockVRCameraLookAt(list[num]);
				num++;
			}
			return (!flag) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000270 RID: 624 RVA: 0x00014540 File Offset: 0x00012940
		internal int GetRigidbodyConstraintsMask()
		{
			int globalRigidbodyConstraints = (int)BlockMaster.GetGlobalRigidbodyConstraints();
			return (globalRigidbodyConstraints & ~this.rbConstraintsOff) | this.rbConstraintsOn;
		}

		// Token: 0x06000271 RID: 625 RVA: 0x00014568 File Offset: 0x00012968
		protected int GetRBConstraintsArgAsInt(object[] args, int index, bool translation)
		{
			int intArg = Util.GetIntArg(args, index, 0);
			RigidbodyConstraints result = RigidbodyConstraints.None;
			if (translation)
			{
				switch (intArg)
				{
				case 0:
					result = RigidbodyConstraints.FreezePosition;
					break;
				case 1:
					result = RigidbodyConstraints.FreezePositionX;
					break;
				case 2:
					result = RigidbodyConstraints.FreezePositionY;
					break;
				case 3:
					result = RigidbodyConstraints.FreezePositionZ;
					break;
				}
			}
			else
			{
				switch (intArg)
				{
				case 0:
					result = RigidbodyConstraints.FreezeRotation;
					break;
				case 1:
					result = RigidbodyConstraints.FreezeRotationX;
					break;
				case 2:
					result = RigidbodyConstraints.FreezeRotationY;
					break;
				case 3:
					result = RigidbodyConstraints.FreezeRotationZ;
					break;
				}
			}
			return (int)result;
		}

		// Token: 0x06000272 RID: 626 RVA: 0x000145FF File Offset: 0x000129FF
		protected TileResultCode boolToTileResult(bool result)
		{
			return (!result) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000273 RID: 627 RVA: 0x00014610 File Offset: 0x00012A10
		protected TileResultCode tileResultFromRBConstraintExclusion(int existingConstraints, int rc)
		{
			bool result = (existingConstraints & rc) == 0;
			return this.boolToTileResult(result);
		}

		// Token: 0x06000274 RID: 628 RVA: 0x0001462C File Offset: 0x00012A2C
		protected TileResultCode tileResultFromRBConstraintInclusion(int existingConstraints, int rc)
		{
			bool result = (existingConstraints & rc) == rc;
			return this.boolToTileResult(result);
		}

		// Token: 0x06000275 RID: 629 RVA: 0x00014648 File Offset: 0x00012A48
		public TileResultCode IsConstrainTranslation(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int rbconstraintsArgAsInt = this.GetRBConstraintsArgAsInt(args, 0, true);
			return this.tileResultFromRBConstraintInclusion(this.rbConstraintsOn, rbconstraintsArgAsInt);
		}

		// Token: 0x06000276 RID: 630 RVA: 0x0001466C File Offset: 0x00012A6C
		public TileResultCode ConstrainTranslation(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int num = this.GetRBConstraintsArgAsInt(args, 0, true);
			num = (this.rbConstraintsOn | num);
			this.rbUpdatedConstraints |= (num != this.rbConstraintsOn);
			this.rbConstraintsOn = num;
			this.rbConstraintsOff &= ~num;
			return TileResultCode.True;
		}

		// Token: 0x06000277 RID: 631 RVA: 0x000146BC File Offset: 0x00012ABC
		public TileResultCode IsFreeTranslation(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int rbconstraintsArgAsInt = this.GetRBConstraintsArgAsInt(args, 0, true);
			return this.tileResultFromRBConstraintInclusion(this.rbConstraintsOff, rbconstraintsArgAsInt);
		}

		// Token: 0x06000278 RID: 632 RVA: 0x000146E0 File Offset: 0x00012AE0
		public TileResultCode FreeTranslation(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int num = this.GetRBConstraintsArgAsInt(args, 0, true);
			num = (this.rbConstraintsOff | num);
			this.rbUpdatedConstraints |= (num != this.rbConstraintsOff);
			this.rbConstraintsOff = num;
			this.rbConstraintsOn &= ~num;
			return TileResultCode.True;
		}

		// Token: 0x06000279 RID: 633 RVA: 0x00014730 File Offset: 0x00012B30
		public TileResultCode IsConstrainRotation(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int rbconstraintsArgAsInt = this.GetRBConstraintsArgAsInt(args, 0, false);
			return this.tileResultFromRBConstraintInclusion(this.rbConstraintsOn, rbconstraintsArgAsInt);
		}

		// Token: 0x0600027A RID: 634 RVA: 0x00014754 File Offset: 0x00012B54
		public TileResultCode ConstrainRotation(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int num = this.GetRBConstraintsArgAsInt(args, 0, false);
			num = (this.rbConstraintsOn | num);
			this.rbUpdatedConstraints |= (num != this.rbConstraintsOn);
			this.rbConstraintsOn = num;
			this.rbConstraintsOff &= ~num;
			return TileResultCode.True;
		}

		// Token: 0x0600027B RID: 635 RVA: 0x000147A4 File Offset: 0x00012BA4
		public TileResultCode IsFreeRotation(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int rbconstraintsArgAsInt = this.GetRBConstraintsArgAsInt(args, 0, false);
			return this.tileResultFromRBConstraintInclusion(this.rbConstraintsOff, rbconstraintsArgAsInt);
		}

		// Token: 0x0600027C RID: 636 RVA: 0x000147C8 File Offset: 0x00012BC8
		public TileResultCode FreeRotation(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int num = this.GetRBConstraintsArgAsInt(args, 0, false);
			num = (this.rbConstraintsOff | num);
			this.rbUpdatedConstraints |= (num != this.rbConstraintsOff);
			this.rbConstraintsOff = num;
			this.rbConstraintsOn &= ~num;
			return TileResultCode.True;
		}

		// Token: 0x0600027D RID: 637 RVA: 0x00014818 File Offset: 0x00012C18
		public static void Init()
		{
			if (Block.leaderboardData == null)
			{
				Block.leaderboardData = Blocksworld.leaderboardData;
			}
			if (!Blocksworld.renderingShadows)
			{
				if (Block.prefabShadow == null)
				{
					Block.prefabShadow = (UnityEngine.Object.Instantiate(Resources.Load("Blocks/Shadow2")) as GameObject);
					Block.prefabShadow.SetActive(false);
				}
				if (Block.shadowParent == null)
				{
					GameObject gameObject = new GameObject("Block Shadows");
					Block.shadowParent = gameObject.transform;
					Block.prefabShadow.transform.parent = Block.shadowParent;
				}
			}
		}

		// Token: 0x0600027E RID: 638 RVA: 0x000148B8 File Offset: 0x00012CB8
		protected void InstantiateShadow(GameObject prefab)
		{
			this.goShadow = UnityEngine.Object.Instantiate<GameObject>(prefab);
			this.goShadow.SetActive(true);
			this.goShadowT = this.goShadow.transform;
			this.goShadowT.parent = Block.shadowParent;
			this.hasShadow = true;
			Mesh mesh = this.meshShadow;
			this.meshShadow = this.goShadow.GetComponent<MeshFilter>().mesh;
			if (mesh != null && mesh != this.meshShadow)
			{
				UnityEngine.Object.Destroy(mesh);
			}
			this.colorsShadow = new Color[4];
			this.UpdateShadowColors("Black", null, null);
		}

		// Token: 0x0600027F RID: 639 RVA: 0x0001495E File Offset: 0x00012D5E
		private bool HasShadow()
		{
			return !this.isTerrain && !(this is BlockAbstractMovingPlatform);
		}

		// Token: 0x06000280 RID: 640 RVA: 0x0001497C File Offset: 0x00012D7C
		public virtual void Destroy()
		{
			UnityEngine.Object.Destroy(this.mesh);
			if (this.childMeshes != null)
			{
				foreach (Mesh obj in this.childMeshes.Values)
				{
					UnityEngine.Object.Destroy(obj);
				}
			}
			this.childMeshes = null;
			UnityEngine.Object.Destroy(this.go);
			this.go = null;
			this.DestroyShadow();
			BWSceneManager.BlockDestroyed(this);
		}

		// Token: 0x06000281 RID: 641 RVA: 0x00014A18 File Offset: 0x00012E18
		public void DestroyShadow()
		{
			if (this.goShadow != null)
			{
				UnityEngine.Object.Destroy(this.goShadow);
			}
			if (this.meshShadow != null)
			{
				UnityEngine.Object.Destroy(this.meshShadow);
			}
			this.goShadow = null;
			this.goShadowT = null;
			this.hasShadow = false;
		}

		// Token: 0x06000282 RID: 642 RVA: 0x00014A72 File Offset: 0x00012E72
		public virtual void BeforePlay()
		{
			this.hadRigidBody = false;
		}

		// Token: 0x06000283 RID: 643 RVA: 0x00014A7C File Offset: 0x00012E7C
		public int AddOrRemoveEmptyScriptLine()
		{
			int num = 0;
			if (this.tiles[this.tiles.Count - 1].Count > 1)
			{
				Tile item = new Tile(new GAF("Meta.Then", new object[0]));
				this.tiles.Add(new List<Tile>
				{
					item
				});
				num++;
			}
			for (int i = this.tiles.Count - 2; i >= 0; i--)
			{
				if (this.tiles[i].Count == 1)
				{
					this.tiles[i][0].Show(false);
					this.tiles.RemoveAt(i);
					num--;
				}
			}
			return num;
		}

		// Token: 0x06000284 RID: 644 RVA: 0x00014B40 File Offset: 0x00012F40
		public void RemoveTile(Tile tile)
		{
			foreach (List<Tile> list in this.tiles)
			{
				if (list.Remove(tile))
				{
					break;
				}
			}
		}

		// Token: 0x06000285 RID: 645 RVA: 0x00014BA8 File Offset: 0x00012FA8
		public void PrintTiles()
		{
			string text = string.Empty;
			foreach (List<Tile> list in this.tiles)
			{
				foreach (Tile arg in list)
				{
					text = text + arg + ", ";
				}
				text += "\n";
			}
			BWLog.Info(text);
		}

		// Token: 0x06000286 RID: 646 RVA: 0x00014C64 File Offset: 0x00013064
		public void AddLockedTileRow()
		{
			this.tiles.Insert(1, Block.LockedTileRow());
		}

		// Token: 0x06000287 RID: 647 RVA: 0x00014C78 File Offset: 0x00013078
		public bool IsDefaultPaint(GAF gaf)
		{
			string paint = (string)gaf.Args[0];
			int meshIndex = (gaf.Args.Length <= 1) ? 0 : ((int)gaf.Args[1]);
			return Scarcity.FreePaint(this.BlockType(), meshIndex, paint);
		}

		// Token: 0x06000288 RID: 648 RVA: 0x00014CC4 File Offset: 0x000130C4
		public string GetDefaultPaint(int meshIndex = 0)
		{
			string blockType = this.BlockType();
			return Scarcity.DefaultPaint(blockType, meshIndex);
		}

		// Token: 0x06000289 RID: 649 RVA: 0x00014CE0 File Offset: 0x000130E0
		public bool IsDefaultTexture(GAF gaf)
		{
			string texture = (string)gaf.Args[0];
			int meshIndex = (gaf.Args.Length <= 2) ? 0 : ((int)gaf.Args[2]);
			return this.IsDefaultTexture(texture, meshIndex);
		}

		// Token: 0x0600028A RID: 650 RVA: 0x00014D25 File Offset: 0x00013125
		public bool IsDefaultTexture(string texture, int meshIndex)
		{
			return Scarcity.GetNormalizedTexture(texture) == Scarcity.GetNormalizedTexture(this.GetDefaultTexture(meshIndex));
		}

		// Token: 0x0600028B RID: 651 RVA: 0x00014D40 File Offset: 0x00013140
		public string GetDefaultTexture(int meshIndex = 0)
		{
			string blockType = this.BlockType();
			return Scarcity.DefaultTexture(blockType, meshIndex);
		}

		// Token: 0x0600028C RID: 652 RVA: 0x00014D5C File Offset: 0x0001315C
		public bool[] GetCanBeTextured()
		{
			string key = this.BlockType();
			if (Block.blockTypeCanBeTextured == null)
			{
				Block.blockTypeCanBeTextured = new Dictionary<string, bool[]>();
			}
			if (!Block.blockTypeCanBeTextured.ContainsKey(key))
			{
				List<bool> list = new List<bool>();
				foreach (BlockMeshMetaData blockMeshMetaData in this.GetBlockMeshMetaDatas())
				{
					list.Add(blockMeshMetaData.canBeTextured);
				}
				bool[] array = list.ToArray();
				Block.blockTypeCanBeTextured[key] = array;
				return array;
			}
			return Block.blockTypeCanBeTextured[key];
		}

		// Token: 0x0600028D RID: 653 RVA: 0x00014DF0 File Offset: 0x000131F0
		public float GetBuoyancyMultiplier()
		{
			BlockMetaData blockMetaData = this.GetBlockMetaData();
			if (blockMetaData != null)
			{
				return blockMetaData.buoyancyMultiplier;
			}
			return 1f;
		}

		// Token: 0x0600028E RID: 654 RVA: 0x00014E1C File Offset: 0x0001321C
		public bool[] GetCanBeMaterialTextured()
		{
			string key = this.BlockType();
			if (!Block.blockTypeCanBeMaterialTextured.ContainsKey(key))
			{
				List<bool> list = new List<bool>();
				foreach (BlockMeshMetaData blockMeshMetaData in this.GetBlockMeshMetaDatas())
				{
					list.Add(blockMeshMetaData.canBeMaterialTextured);
				}
				bool[] array = list.ToArray();
				Block.blockTypeCanBeMaterialTextured[key] = array;
				return array;
			}
			return Block.blockTypeCanBeMaterialTextured[key];
		}

		// Token: 0x0600028F RID: 655 RVA: 0x00014E9C File Offset: 0x0001329C
		public static bool IsDefaultSfx(string blockType, string sfxName)
		{
			HashSet<string> hashSet;
			return Block.defaultSfxs.TryGetValue(blockType, out hashSet) && hashSet.Contains(sfxName);
		}

		// Token: 0x06000290 RID: 656 RVA: 0x00014EC4 File Offset: 0x000132C4
		private static void CreateDefaultDictionaries()
		{
			if (Block.defaultTextureNormals == null)
			{
				Block.defaultTextureNormals = new Dictionary<string, Vector3[]>();
				Block.defaultTextureNormals.Add("Sky UV", new Vector3[]
				{
					new Vector3(1f, 0f, -1f)
				});
				Block.defaultTextureNormals.Add("Terrain Cube", new Vector3[]
				{
					Vector3.up
				});
				Block.defaultTextureNormals.Add("Terrain Wedge", new Vector3[]
				{
					Vector3.up
				});
				Block.defaultTextureNormals.Add("Emitter", new Vector3[]
				{
					Vector3.right
				});
				foreach (string key in Block.treasureBlocks)
				{
					Block.defaultSfxs[key] = new HashSet<string>
					{
						Block.treasurePickupSfx
					};
				}
			}
		}

		// Token: 0x06000291 RID: 657 RVA: 0x00014FF4 File Offset: 0x000133F4
		private static void AddSameGridCellPair(string s1, string s2)
		{
			HashSet<string> hashSet;
			if (Block.sameGridCellPairs.ContainsKey(s1))
			{
				hashSet = Block.sameGridCellPairs[s1];
			}
			else
			{
				hashSet = new HashSet<string>();
				Block.sameGridCellPairs.Add(s1, hashSet);
			}
			hashSet.Add(s2);
		}

		// Token: 0x06000292 RID: 658 RVA: 0x0001503E File Offset: 0x0001343E
		private static void AddSameGridCellPairBidirectional(string s1, string s2)
		{
			Block.AddSameGridCellPair(s1, s2);
			Block.AddSameGridCellPair(s2, s1);
		}

		// Token: 0x06000293 RID: 659 RVA: 0x00015050 File Offset: 0x00013450
		public static void UpdateOccupySameGridCell(Block b)
		{
			string text = b.BlockType();
			if (!Block.sameGridCellPairs.ContainsKey(text))
			{
				BlockMetaData blockMetaData = b.GetBlockMetaData();
				if (blockMetaData != null)
				{
					string[] canOccupySameGrid = blockMetaData.canOccupySameGrid;
					foreach (string s in canOccupySameGrid)
					{
						Block.AddSameGridCellPairBidirectional(text, s);
					}
				}
			}
		}

		// Token: 0x06000294 RID: 660 RVA: 0x000150B8 File Offset: 0x000134B8
		public static bool WantsToOccupySameGridCell(Block b1, Block b2)
		{
			Block.UpdateOccupySameGridCell(b1);
			Block.UpdateOccupySameGridCell(b2);
			string key = b1.BlockType();
			string item = b2.BlockType();
			if (b1 is BlockCharacter || b1 is BlockAnimatedCharacter)
			{
				key = "Character";
			}
			if (b2 is BlockCharacter || b2 is BlockAnimatedCharacter)
			{
				item = "Character";
			}
			return Block.sameGridCellPairs.ContainsKey(key) && Block.sameGridCellPairs[key].Contains(item);
		}

		// Token: 0x06000295 RID: 661 RVA: 0x0001513C File Offset: 0x0001353C
		private static void AddSensorActions(List<List<Tile>> tiles, GAF sensorGaf, params GAF[] actionGafs)
		{
			if (sensorGaf != null)
			{
				tiles[tiles.Count - 1].Insert(0, new Tile(sensorGaf));
			}
			foreach (GAF gaf in actionGafs)
			{
				tiles[tiles.Count - 1].Add(new Tile(gaf));
			}
			tiles.Add(new List<Tile>());
			tiles[tiles.Count - 1].Add(new Tile(new GAF("Meta.Then", new object[0])));
		}

		// Token: 0x06000296 RID: 662 RVA: 0x000151D0 File Offset: 0x000135D0
		private static void AddAntigravityDefaultActions(bool defaultTiles, string prefix, List<List<Tile>> tiles)
		{
			Block.AddDefaultSensorActions(defaultTiles, tiles, null, new GAF[]
			{
				new GAF(prefix + ".IncreaseModelGravityInfluence", new object[]
				{
					-1f
				})
			});
			Block.AddDefaultSensorActions(defaultTiles, tiles, null, new GAF[]
			{
				new GAF(prefix + ".AlignInGravityFieldChunk", new object[]
				{
					1f
				})
			});
		}

		// Token: 0x06000297 RID: 663 RVA: 0x00015248 File Offset: 0x00013648
		private static void AddDefaultSensorActions(bool def, List<List<Tile>> tiles, GAF sensorGaf, params GAF[] actionGafs)
		{
			if (def)
			{
				if (sensorGaf != null)
				{
					tiles[tiles.Count - 1].Insert(0, new Tile(sensorGaf));
				}
				foreach (GAF gaf in actionGafs)
				{
					tiles[tiles.Count - 1].Add(new Tile(gaf));
				}
				tiles.Add(new List<Tile>());
				tiles[tiles.Count - 1].Add(new Tile(new GAF("Meta.Then", new object[0])));
			}
		}

		// Token: 0x06000298 RID: 664 RVA: 0x000152E1 File Offset: 0x000136E1
		private static void AddFirstRowActions(List<List<Tile>> tiles, params GAF[] actionGafs)
		{
			Block.AddSensorActions(tiles, null, actionGafs);
		}

		// Token: 0x06000299 RID: 665 RVA: 0x000152EC File Offset: 0x000136EC
		private static void AddDefaultGAF(bool defaultTiles, List<List<Tile>> tiles, string predName, params object[] args)
		{
			if (defaultTiles)
			{
				tiles[1].Add(new Tile(new GAF(predName, args)));
				tiles.Add(new List<Tile>());
				tiles[2].Add(new Tile(new GAF("Meta.Then", new object[0])));
			}
		}

		// Token: 0x0600029A RID: 666 RVA: 0x00015344 File Offset: 0x00013744
		public virtual bool HasDefaultScript(List<List<Tile>> tilesToUse = null)
		{
			List<List<Tile>> defaultExtraTilesCopy = Block.GetDefaultExtraTilesCopy(this.BlockType(), false);
			if (tilesToUse == null)
			{
				tilesToUse = this.tiles;
			}
			if (defaultExtraTilesCopy == null)
			{
				if (tilesToUse.Count != 2 || tilesToUse[1].Count != 1)
				{
					return false;
				}
			}
			else
			{
				for (int i = 0; i < Mathf.Max(defaultExtraTilesCopy.Count, tilesToUse.Count - 1); i++)
				{
					if (i >= defaultExtraTilesCopy.Count)
					{
						return false;
					}
					int num = i + 1;
					if (num >= tilesToUse.Count)
					{
						return false;
					}
					List<Tile> list = defaultExtraTilesCopy[i];
					List<Tile> list2 = tilesToUse[num];
					if (list.Count != list2.Count)
					{
						return false;
					}
					for (int j = 0; j < list.Count; j++)
					{
						GAF gaf = list[j].gaf;
						GAF gaf2 = list2[j].gaf;
						if (gaf.Predicate != gaf2.Predicate)
						{
							return false;
						}
						if (!this.IgnoreChangesToDefaultForPredicate(gaf2.Predicate))
						{
							object[] args = gaf.Args;
							object[] args2 = gaf2.Args;
							if (args.Length != args2.Length)
							{
								return false;
							}
							for (int k = 0; k < args2.Length; k++)
							{
								object obj = args2[k];
								object obj2 = args[k];
								if (obj is float)
								{
									if (Mathf.Abs((float)obj - (float)obj2) > 1E-06f)
									{
										return false;
									}
								}
								else if (obj is int)
								{
									if ((int)obj != (int)obj2)
									{
										return false;
									}
								}
								else if (args2[k] != args[k])
								{
									return false;
								}
							}
						}
					}
				}
			}
			return true;
		}

		// Token: 0x0600029B RID: 667 RVA: 0x00015517 File Offset: 0x00013917
		protected virtual bool IgnoreChangesToDefaultForPredicate(Predicate predicate)
		{
			return false;
		}

		// Token: 0x0600029C RID: 668 RVA: 0x0001551C File Offset: 0x0001391C
		public bool HasDefaultTiles()
		{
			if (this.CanScale().magnitude > 0.01f)
			{
				Vector3 defaultScale = this.GetDefaultScale();
				Vector3 scale = this.GetScale();
				if ((defaultScale - scale).magnitude > 0.01f)
				{
					return false;
				}
			}
			Dictionary<GAF, int> dictionary = new Dictionary<GAF, int>();
			Scarcity.CalculateBlockGafUsage(this, dictionary, false, false);
			foreach (GAF gaf in dictionary.Keys)
			{
				Predicate predicate = gaf.Predicate;
				if (predicate == BlockAnimatedCharacter.predicateReplaceLimb)
				{
					return false;
				}
				if (predicate == Block.predicateTextureTo || predicate == Block.predicatePaintTo || predicate == Block.predicatePlaySoundDurational)
				{
					return false;
				}
			}
			return this.HasDefaultScript(null);
		}

		// Token: 0x0600029D RID: 669 RVA: 0x0001561C File Offset: 0x00013A1C
		protected static Tile FirstFrameTile()
		{
			return new Tile(new GAF("Block.FirstFrame", new object[0]));
		}

		// Token: 0x0600029E RID: 670 RVA: 0x00015633 File Offset: 0x00013A33
		public static Tile ThenTile()
		{
			return new Tile(new GAF("Meta.Then", new object[0]));
		}

		// Token: 0x0600029F RID: 671 RVA: 0x0001564A File Offset: 0x00013A4A
		public static Tile StopTile()
		{
			return new Tile(new GAF(Block.predicateStop, new object[0]));
		}

		// Token: 0x060002A0 RID: 672 RVA: 0x00015661 File Offset: 0x00013A61
		public static Tile WaitTile(float time)
		{
			return new Tile(Block.predicateWaitTime, new object[]
			{
				time
			});
		}

		// Token: 0x060002A1 RID: 673 RVA: 0x0001567C File Offset: 0x00013A7C
		public static Tile ButtonTile(string name)
		{
			return new Tile(new GAF(Block.predicateButton, new object[]
			{
				name
			}));
		}

		// Token: 0x060002A2 RID: 674 RVA: 0x00015698 File Offset: 0x00013A98
		public static List<Tile> EmptyTileRow()
		{
			return new List<Tile>
			{
				Block.ThenTile()
			};
		}

		// Token: 0x060002A3 RID: 675 RVA: 0x000156B7 File Offset: 0x00013AB7
		public static Tile LockedTile()
		{
			return new Tile(new GAF(Block.predicateLocked, new object[0]));
		}

		// Token: 0x060002A4 RID: 676 RVA: 0x000156D0 File Offset: 0x00013AD0
		public static List<Tile> LockedTileRow()
		{
			return new List<Tile>
			{
				Block.ThenTile(),
				Block.LockedTile()
			};
		}

		// Token: 0x060002A5 RID: 677 RVA: 0x000156FC File Offset: 0x00013AFC
		public static void WriteDefaultExtraTiles(bool defaultTiles, List<List<Tile>> tiles, string blockType)
		{
			if (defaultTiles)
			{
				List<List<Tile>> defaultExtraTilesCopy = Block.GetDefaultExtraTilesCopy(blockType, true);
				if (defaultExtraTilesCopy != null)
				{
					tiles.RemoveRange(1, tiles.Count - 1);
					tiles.AddRange(defaultExtraTilesCopy);
				}
			}
		}

		// Token: 0x060002A6 RID: 678 RVA: 0x00015734 File Offset: 0x00013B34
		public static List<List<Tile>> GetDefaultExtraTilesCopy(string blockType, bool processTiles = false)
		{
			List<List<Tile>> list;
			if (Block.defaultExtraTiles.TryGetValue(blockType, out list))
			{
				List<List<Tile>> list2 = Util.CopyTiles(list);
				Action<List<List<Tile>>> action;
				if (processTiles && Block.defaultExtraTilesProcessors.TryGetValue(blockType, out action))
				{
					action(list2);
				}
				return list2;
			}
			return null;
		}

		// Token: 0x060002A7 RID: 679 RVA: 0x0001577C File Offset: 0x00013B7C
		protected static void AddSimpleDefaultTiles(GAF gaf, params string[] blockTypes)
		{
			List<List<Tile>> value = new List<List<Tile>>
			{
				new List<Tile>
				{
					Block.ThenTile(),
					new Tile(gaf)
				},
				Block.EmptyTileRow()
			};
			foreach (string key in blockTypes)
			{
				Block.defaultExtraTiles[key] = value;
			}
		}

		// Token: 0x060002A8 RID: 680 RVA: 0x000157F0 File Offset: 0x00013BF0
		protected static void AddSimpleDefaultTiles(GAF gafFirstRow, GAF gafSecondRow, params string[] blockTypes)
		{
			List<List<Tile>> value = new List<List<Tile>>
			{
				new List<Tile>
				{
					Block.ThenTile(),
					new Tile(gafFirstRow)
				},
				new List<Tile>
				{
					Block.ThenTile(),
					new Tile(gafSecondRow)
				},
				Block.EmptyTileRow()
			};
			foreach (string key in blockTypes)
			{
				Block.defaultExtraTiles[key] = value;
			}
		}

		// Token: 0x060002A9 RID: 681 RVA: 0x00015888 File Offset: 0x00013C88
		protected static void AddSimpleDefaultTiles(List<GAF> gafs, params string[] blockTypes)
		{
			List<Tile> list = new List<Tile>
			{
				Block.ThenTile()
			};
			foreach (GAF gaf in gafs)
			{
				list.Add(new Tile(gaf));
			}
			List<List<Tile>> value = new List<List<Tile>>
			{
				list,
				Block.EmptyTileRow()
			};
			foreach (string key in blockTypes)
			{
				Block.defaultExtraTiles[key] = value;
			}
		}

		// Token: 0x060002AA RID: 682 RVA: 0x00015948 File Offset: 0x00013D48
		public static Block NewBlock(List<List<Tile>> tiles, bool defaultColors = false, bool defaultTiles = false)
		{
			Block.CreateDefaultDictionaries();
			List<Tile> list = tiles[0];
			int index = 0;
			if (list[index].gaf.Predicate != Block.predicateStop)
			{
				list.Insert(index, Block.StopTile());
			}
			int index2 = 5;
			if (list[index2].gaf.Predicate != Block.predicateTextureTo)
			{
				list.Insert(index2, new Tile(new GAF(Block.predicateTextureTo, new object[]
				{
					"Plain",
					Vector3.zero
				})));
			}
			List<Tile> list2 = tiles[tiles.Count - 1];
			if (list2.Count != 1 || list2[0].gaf.Predicate != Block.predicateThen)
			{
				tiles.Add(Block.EmptyTileRow());
			}
			string text = (string)tiles[0][1].gaf.Args[0];
			if (text == "Character Avatar")
			{
				bool flag = WorldSession.current.BlockIsAvailable("Block Anim Character Male") || WorldSession.current.BlockIsAvailable("Block Anim Character Female") || WorldSession.current.BlockIsAvailable("Block Anim Character Skeleton");
				if (flag)
				{
					text = "Anim Character Avatar";
					tiles[0][1].gaf = new GAF(Block.predicateCreate, new object[]
					{
						text
					});
					ProfileBlocksterUtils.ConvertToAnimated(tiles);
				}
			}
			if (text == "Anim Character Avatar" && !WorldSession.current.BlockIsAvailable("Block Anim Character Male") && !WorldSession.current.BlockIsAvailable("Block Anim Character Female") && !WorldSession.current.BlockIsAvailable("Block Anim Character Skeleton"))
			{
				text = "Character Avatar";
				tiles[0][1].gaf = new GAF(Block.predicateCreate, new object[]
				{
					text
				});
				ProfileBlocksterUtils.ConvertToNonAnimated(tiles);
			}
			if (!Blocksworld.prefabs.ContainsKey(text))
			{
				if (Blocksworld.existingBlockNames.Contains(text))
				{
					Blocksworld.LoadBlock(text);
				}
				else
				{
					UnityEngine.Object x = Resources.Load("Blocks/Prefab " + text);
					if (x != null)
					{
						Blocksworld.existingBlockNames.Add(text);
						//OnScreenLog.AddLogItem("Please update BlockList.txt (Window -> BW Commands -> Generate Block List) and push to repo. '" + text + "' ", 60f, true);
                        // TODO: properly disable logs
						Blocksworld.LoadBlock(text);
					}
					else
					{
						if (Blocksworld.prefabs.ContainsKey("Cube"))
						{
							BWLog.Warning("Missing prefab '" + text + "', using 'Cube' instead");
							text = "Cube";
						}
						else if (Blocksworld.prefabs.Count > 0)
						{
							Dictionary<string, GameObject>.KeyCollection.Enumerator enumerator = Blocksworld.prefabs.Keys.GetEnumerator();
							enumerator.MoveNext();
							BWLog.Warning(string.Concat(new string[]
							{
								"Missing prefab '",
								text,
								"', using '",
								enumerator.Current,
								"' instead"
							}));
							text = enumerator.Current;
						}
						else
						{
							BWLog.Error("No fallback prefab found for missing resource of type " + text);
						}
						tiles[0][1].gaf.Args[0] = text;
					}
				}
			}
			if (!Blocksworld.goPrefabs.ContainsKey(text))
			{
				Blocksworld.LoadBlockFromPrefab(text);
			}
			Block block;
			switch (text)
			{
			case "Jukebox":
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				block = new BlockJukebox(tiles);
				goto IL_22AA;
			case "Mountain 1":
			case "Mountain 2":
			case "Hill 1":
			case "Hill 2":
			case "Cliff 1":
			case "Cliff 2":
			case "Cliff Curve 1":
			case "Arch Rock":
			case "Cave":
			case "Rock Ramp":
			case "Forrest":
			case "DNO Nest":
			case "SR2 Planet Terrain":
				block = new BlockTerrain(tiles);
				((BlockTerrain)block).doubleTapToSelect = true;
				BWSceneManager.AddTerrainBlock((BlockTerrain)block);
				goto IL_22AA;
			case "Cloud 1":
			case "Cloud 2":
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				block = new BlockCloud(tiles);
				((BlockTerrain)block).doubleTapToSelect = true;
				BWSceneManager.AddTerrainBlock((BlockTerrain)block);
				goto IL_22AA;
			case "Volcano":
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				block = new BlockVolcano(tiles);
				((BlockTerrain)block).doubleTapToSelect = true;
				BWSceneManager.AddTerrainBlock((BlockTerrain)block);
				goto IL_22AA;
			case "Water Cube":
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				block = new BlockWaterCube(tiles);
				BlockAbstractWater.waterCubes.Add((BlockWaterCube)block);
				goto IL_22AA;
			case "Master":
				block = new BlockMaster(tiles);
				goto IL_22AA;
			case "Highscore I":
				block = new BlockHighscoreList(tiles, 0);
				goto IL_22AA;
			case "UI Counter I":
				if (defaultTiles)
				{
					Block.AddSensorActions(tiles, new GAF("Block.FirstFrame", new object[0]), new GAF[]
					{
						new GAF("CounterUI.Equals", new object[]
						{
							0,
							0
						})
					});
				}
				block = new BlockCounterUI(tiles, 0);
				goto IL_22AA;
			case "UI Counter II":
				if (defaultTiles)
				{
					Block.AddSensorActions(tiles, new GAF("Block.FirstFrame", new object[0]), new GAF[]
					{
						new GAF("CounterUI.Equals", new object[]
						{
							0,
							1
						})
					});
				}
				block = new BlockCounterUI(tiles, 1);
				goto IL_22AA;
			case "UI Object Counter I":
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				block = new BlockObjectCounterUI(tiles, 0);
				goto IL_22AA;
			case "UI Gauge I":
				if (defaultTiles)
				{
					Block.AddSensorActions(tiles, new GAF("Block.FirstFrame", new object[0]), new GAF[]
					{
						new GAF("GaugeUI.Equals", new object[]
						{
							0,
							0
						})
					});
				}
				block = new BlockGaugeUI(tiles, 0);
				goto IL_22AA;
			case "UI Gauge II":
				if (defaultTiles)
				{
					Block.AddSensorActions(tiles, new GAF("Block.FirstFrame", new object[0]), new GAF[]
					{
						new GAF("GaugeUI.Equals", new object[]
						{
							0,
							1
						})
					});
				}
				block = new BlockGaugeUI(tiles, 1);
				goto IL_22AA;
			case "UI Radar I":
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				block = new BlockRadarUI(tiles, 0);
				goto IL_22AA;
			case "UI Timer I":
				if (defaultTiles)
				{
					Block.AddSensorActions(tiles, new GAF("Block.FirstFrame", new object[0]), new GAF[]
					{
						new GAF("TimerUI.Equals", new object[]
						{
							0f
						})
					});
				}
				block = new BlockTimerUI(tiles, 0);
				goto IL_22AA;
			case "UI Timer II":
				if (defaultTiles)
				{
					Block.AddSensorActions(tiles, new GAF("Block.FirstFrame", new object[0]), new GAF[]
					{
						new GAF("TimerUI.Equals", new object[]
						{
							0f
						})
					});
				}
				block = new BlockTimerUI(tiles, 1);
				goto IL_22AA;
			case "Orrery":
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				block = new BlockWorldJumper(tiles);
				goto IL_22AA;
			case "Rocket":
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				block = new BlockRocket(tiles);
				goto IL_22AA;
			case "Rocket Square":
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				block = new BlockRocketSquare(tiles);
				goto IL_22AA;
			case "Rocket Octagonal":
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				block = new BlockRocketOctagonal(tiles);
				goto IL_22AA;
			case "Tank Treads Wheel":
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				block = new BlockTankTreadsWheel(tiles);
				goto IL_22AA;
			case "Missile A":
			case "Missile B":
			case "Missile C":
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				block = new BlockMissile(tiles);
				goto IL_22AA;
			case "Missile Control":
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				block = new BlockMissileControl(tiles);
				goto IL_22AA;
			case "Missile Control Model":
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				block = new BlockModelMissileControl(tiles);
				goto IL_22AA;
			case "Jet Engine":
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				block = new BlockJetEngine(tiles);
				goto IL_22AA;
			case "Raycast Wheel":
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				block = new BlockRaycastWheel(tiles);
				goto IL_22AA;
			case "Wheel":
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				block = new BlockWheel(tiles, "Wheel Axle");
				goto IL_22AA;
			case "RAR Moon Rover Wheel":
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				block = new BlockWheel(tiles, "RAR Moon Rover Wheel Axle");
				goto IL_22AA;
			case "Wheel Generic1":
			case "Wheel Generic2":
			case "Wheel Semi1":
			case "Wheel Semi2":
			case "Wheel Monster1":
			case "Wheel Monster2":
			case "Wheel Monster3":
			case "Wheel 6 Spoke":
			case "Wheel Pinwheel":
			case "Wheel BasketWeave":
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				block = new BlockTwoSidedWheel(tiles);
				goto IL_22AA;
			case "Bulky Wheel":
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				block = new BlockBulkyWheel(tiles, "Bulky");
				goto IL_22AA;
			case "Spoked Wheel":
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				block = new BlockSpokedWheel(tiles);
				goto IL_22AA;
			case "Golden Wheel":
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				block = new BlockWheelBling(tiles);
				if (defaultColors)
				{
					block.RotateTo(Quaternion.Euler(0f, 270f, 0f));
				}
				goto IL_22AA;
			case "Billboard Celestial Emissive":
			case "Billboard Celestial Emissive Far":
			case "Billboard Terrain":
			case "Billboard Jupiter":
			case "Billboard Nebula":
			case "Billboard Celestial Emissive No Reflect":
			case "Billboard Celestial Emissive Far No Reflect":
			case "Billboard Terrain No Reflect":
			case "Billboard Jupiter No Reflect":
			case "Billboard Nebula No Reflect":
				block = new BlockBillboard(tiles);
				goto IL_22AA;
			case "Sphere":
			case "Soccer Ball":
			case "Geodesic Ball":
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				block = new BlockSphere(tiles);
				goto IL_22AA;
			case "Motor":
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				block = new BlockMotor(tiles);
				goto IL_22AA;
			case "Motor Cube":
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				block = new BlockMotorCube(tiles);
				goto IL_22AA;
			case "Motor Slab":
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				block = new BlockMotorSlab(tiles);
				goto IL_22AA;
			case "Motor Slab 2":
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				block = new BlockMotorSlab2(tiles);
				goto IL_22AA;
			case "Piston":
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				block = new BlockPiston(tiles);
				goto IL_22AA;
			case "Magnet":
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				block = new BlockMagnet(tiles);
				goto IL_22AA;
			case "Torsion Spring":
				block = new BlockTorsionSpring(tiles, "Torsion Spring Axle");
				goto IL_22AA;
			case "Torsion Spring Slab":
				block = new BlockTorsionSpringSlab(tiles);
				goto IL_22AA;
			case "Torsion Spring Cube":
				block = new BlockTorsionSpringCube(tiles);
				goto IL_22AA;
			case "Motor Spindle":
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				block = new BlockMotorSpindle(tiles);
				goto IL_22AA;
			case "Sky":
			case "Sky UV":
			case "Sky Medieval":
			case "Sky Starter Island":
			case "Sky Winter":
			case "Sky Space":
			case "Sky Outerspace":
			case "Sky Space Asteroid":
			case "Sky Oz":
			case "Sky City":
			case "Sky GI Joe":
			case "Sky Desert Space":
				block = new BlockSky(text, tiles);
				goto IL_22AA;
			case "PIR Pistol":
				block = new BlockPIRPistol(tiles);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "RAR Cross Bow":
				block = new BlockCrossbow(tiles);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "Laser":
				block = new BlockLaser(tiles);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				if (defaultColors)
				{
					block.RotateTo(Quaternion.Euler(90f, 0f, 0f));
				}
				goto IL_22AA;
			case "Laser Cannon":
				block = new BlockLaserCannon(tiles);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "Laser Blaster":
				block = new BlockLaserBlaster(tiles);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "Laser Octagonal":
				block = new BlockOctagonalLaser(tiles);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "Jazz Gun":
				block = new BlockJazzGun(tiles);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "Bumblebee Gun":
				block = new BlockBumblebeeGun(tiles);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "Megatron Gun":
				block = new BlockMegatronGun(tiles);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "Optimus Gun":
				block = new BlockOptimusGun(tiles);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "Soundwave Gun":
				block = new BlockSoundwaveGun(tiles);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "Starscream Gun":
				block = new BlockStarscreamGun(tiles);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "Laser Minigun":
				block = new BlockLaserMiniGun(tiles);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "Laser Rifle":
				block = new BlockLaserRifle(tiles);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "Hand Cannon":
				block = new BlockHandCannon(tiles);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "Laser Pistol":
				block = new BlockLaserPistol(tiles);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "Laser Pistol2":
			case "BBG Ray Gun":
			case "FUT Space Gun":
				block = new BlockLaserPistol2(tiles);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "GIJ MiniGun":
				block = new BlockMiniGun(tiles);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "Emitter":
				block = new BlockEmitter(tiles);
				if (defaultColors)
				{
					block.RotateTo(Quaternion.Euler(180f, 0f, 0f));
				}
				goto IL_22AA;
			case "Stabilizer":
				block = new BlockStabilizer(tiles);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "Stabilizer Square":
				block = new BlockSquareStabilizer(tiles);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "Water":
			case "Ice Water":
			case "Desert Water":
			case "Water Endless Expanse":
			{
				block = new BlockWater(tiles);
				Vector3 vector = Vector3.one * 2500f;
				if (Blocksworld.worldSky != null && Blocksworld.worldSky.go != null && !Blocksworld.renderingSkybox)
				{
					vector = Blocksworld.worldSky.go.GetComponent<Collider>().bounds.size;
				}
				Vector3 vector2 = (Vector3)tiles[0][6].gaf.Args[0];
				block.ScaleTo(new Vector3(vector.x, vector2.y, vector.z), true, false);
				BlockWater blockWater = (BlockWater)block;
				blockWater.snapper = vector.x / 10f;
				goto IL_22AA;
			}
			case "Moving Platform Force":
				if (defaultTiles)
				{
					Block.AddFirstRowActions(tiles, new GAF[]
					{
						new GAF("MovingPlatformForce.MoveTo", new object[]
						{
							1,
							2f
						}),
						new GAF("MovingPlatformForce.MoveTo", new object[]
						{
							0,
							2f
						})
					});
				}
				block = new BlockMovingPlatformForce(tiles);
				goto IL_22AA;
			case "Moving Platform":
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				block = new BlockMovingPlatform(tiles);
				goto IL_22AA;
			case "Rotating Platform":
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				block = new BlockRotatingPlatform(tiles);
				goto IL_22AA;
			case "Antigravity Pump":
				block = new BlockAntiGravity(tiles);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "Antigravity Cube":
				block = new BlockAntiGravity(tiles);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "Antigravity Column":
				block = new BlockAntiGravityColumn(tiles);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "Bat Wing":
				block = new BlockBatWing(tiles);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "Fairy Wings":
				block = new BlockFairyWings(tiles);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "Cape":
			case "BBG Cape":
				block = new BlockCape(tiles);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "SPY Jet Pack":
			case "RAR Jet Pack":
			case "FUT Space EVA":
				block = new BlockJetpack(tiles);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "Bat Wing Backpack":
				block = new BlockBatWingBackpack(tiles);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "Bird Wing":
				block = new BlockBirdWing(tiles);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "Wiser Wing":
				block = new BlockWiserWing(tiles);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "MLP Wings":
				block = new BlockMLPWings(tiles);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "Flight Yoke":
				block = new BlockFlightYoke(tiles);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "Steering Wheel":
				block = new BlockSteeringWheel(tiles);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "Drive Assist":
				if (defaultTiles)
				{
					tiles[1].Add(new Tile(new GAF("DriveAssist.Assist", new object[]
					{
						1f
					})));
					tiles[1].Add(new Tile(new GAF("DriveAssist.AlignInGravityFieldChunk", new object[]
					{
						1f
					})));
					tiles.Add(new List<Tile>());
					tiles[2].Add(new Tile(new GAF("Meta.Then", new object[0])));
				}
				block = new BlockDriveAssist(tiles);
				goto IL_22AA;
			case "Hover":
				block = new BlockHover(tiles);
				if (defaultColors)
				{
					block.PaintTo("White", true, 0);
				}
				goto IL_22AA;
			case "GravityGun":
				block = new BlockGravityGun(tiles);
				if (defaultColors)
				{
					block.PaintTo("Blue", true, 0);
				}
				goto IL_22AA;
			case "Position":
			case "Position No Glue":
			case "Position Camera Hint":
				block = new BlockPosition(tiles, false);
				if (defaultTiles)
				{
					tiles[1].Add(new Tile(new GAF("Block.Fixed", new object[0])));
					tiles.Add(new List<Tile>());
					tiles[2].Add(new Tile(new GAF("Meta.Then", new object[0])));
				}
				goto IL_22AA;
			case "Teleport Volume Block":
				block = new BlockTeleportVolumeBlock(tiles);
				goto IL_22AA;
			case "Volume Block":
			case "Volume Block No Glue":
			case "Volume Block Slab":
			case "Volume Block Slab No Glue":
				block = new BlockVolumeBlock(tiles);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "Volume Block Force":
				block = new BlockVolumeForce(tiles);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "Volume":
				block = new BlockVolume(tiles);
				goto IL_22AA;
			case "Legs":
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary["Legs Mesh"] = string.Empty;
				block = new BlockLegs(tiles, dictionary, 0.125f, 1f, 0.25f);
				goto IL_22AA;
			}
			case "Raptor Legs":
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary["Legs Mesh"] = string.Empty;
				block = new BlockLegs(tiles, dictionary, 0.125f, 0.25f, 0.25f);
				goto IL_22AA;
			}
			case "Legs Small":
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary["Foot Right Front"] = "Legs Small Foot Right Front";
				dictionary["Foot Left Front"] = "Legs Small Foot Left Front";
				dictionary["Foot Right Back"] = "Legs Small Foot Right Back";
				dictionary["Foot Left Back"] = "Legs Small Foot Left Back";
				dictionary["Legs Mesh"] = "Legs Small Ankle Mesh";
				block = new BlockQuadped(tiles, dictionary, 0.5f, 0.25f);
				goto IL_22AA;
			}
			case "MLP Body":
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary["Foot Right Front"] = "MLP Legs Foot Right Front";
				dictionary["Foot Left Front"] = "MLP Legs Foot Left Front";
				dictionary["Foot Right Back"] = "MLP Legs Foot Right Back";
				dictionary["Foot Left Back"] = "MLP Legs Foot Left Back";
				dictionary["Legs Mesh Right Front"] = "MLP Legs Ankle Mesh Right Front";
				dictionary["Legs Mesh Left Front"] = "MLP Legs Ankle Mesh Left Front";
				dictionary["Legs Mesh Right Back"] = "MLP Legs Ankle Mesh Right Back";
				dictionary["Legs Mesh Left Back"] = "MLP Legs Ankle Mesh Left Back";
				block = new BlockMLPLegs(tiles, dictionary, 0.875f);
				goto IL_22AA;
			}
			case "Character Profile":
			case "Character Headless Profile":
			case "Character Skeleton Profile":
			case "Character":
			case "Character Male":
			case "Character Skeleton":
			case "Character Headless":
			{
				BlockCharacter.StripNonCompatibleTiles(tiles);
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary["Legs Mesh"] = string.Empty;
				block = new BlockCharacter(tiles, dictionary, false, -1.35f);
				goto IL_22AA;
			}
			case "Character Female Profile":
			case "Character Female":
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary["Legs Mesh"] = string.Empty;
				BlockCharacter.StripNonCompatibleTiles(tiles);
				block = new BlockCharacter(tiles, dictionary, false, -1.35f);
				if (defaultColors)
				{
					block.RotateTo(Quaternion.Euler(0f, 90f, 0f));
				}
				goto IL_22AA;
			}
			case "Character Female Dress":
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary["Body"] = "Character Dress Body";
				dictionary["Legs Mesh"] = "Character Skirt";
				block = new BlockCharacter(tiles, dictionary, true, -1.35f);
				if (defaultColors)
				{
					block.RotateTo(Quaternion.Euler(0f, 90f, 0f));
				}
				goto IL_22AA;
			}
			case "Character Mini":
			case "Character Mini Female":
			case "Character Mini Profile":
			case "Character Mini Female Profile":
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary["Legs Mesh"] = string.Empty;
				BlockCharacter.StripNonCompatibleTiles(tiles);
				block = new BlockCharacter(tiles, dictionary, false, -1f);
				if (text.Contains("Female") && defaultColors)
				{
					block.RotateTo(Quaternion.Euler(0f, 90f, 0f));
				}
				goto IL_22AA;
			}
			case "Character Avatar":
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary["Legs Mesh"] = string.Empty;
				BlockCharacter.StripNonCompatibleTiles(tiles);
				block = new BlockCharacter(tiles, dictionary, false, -1f);
				((BlockCharacter)block).isAvatar = true;
				BWSceneManager.RegisterRuntimeBlockSubstitution(block, new UserAvatarSubstitution(block as BlockCharacter));
				goto IL_22AA;
			}
			case "Anim Character Avatar":
				BlockAnimatedCharacter.StripNonCompatibleTiles(tiles);
				block = new BlockAnimatedCharacter(tiles, 2.5f, CharacterType.Avatar);
				BWSceneManager.RegisterRuntimeBlockSubstitution(block, new UserAvatarSubstitution(block as BlockAnimatedCharacter));
				goto IL_22AA;
			case "Anim Character Male Profile":
			case "Anim Character":
			case "Anim Character Male":
				BlockAnimatedCharacter.StripNonCompatibleTiles(tiles);
				block = new BlockAnimatedCharacter(tiles, 2.5f, CharacterType.Male);
				goto IL_22AA;
			case "Anim Character Skeleton":
			case "Anim Character Skeleton Profile":
				BlockAnimatedCharacter.StripNonCompatibleTiles(tiles);
				block = new BlockAnimatedCharacter(tiles, 2.5f, CharacterType.Skeleton);
				goto IL_22AA;
			case "Anim Character Female Profile":
			case "Anim Character Female":
				BlockAnimatedCharacter.StripNonCompatibleTiles(tiles);
				block = new BlockAnimatedCharacter(tiles, 2.5f, CharacterType.Female);
				if (defaultColors)
				{
					block.RotateTo(Quaternion.Euler(0f, 90f, 0f));
				}
				goto IL_22AA;
			case "Anim Character Female Dress":
				block = new BlockAnimatedCharacter(tiles, 2.5f, CharacterType.Dress);
				if (defaultColors)
				{
					block.RotateTo(Quaternion.Euler(0f, 90f, 0f));
				}
				goto IL_22AA;
			case "Anim Character Mini":
				BlockAnimatedCharacter.StripNonCompatibleTiles(tiles);
				block = new BlockAnimatedCharacter(tiles, 1.75f, CharacterType.MiniMale);
				if (text.Contains("Female") && defaultColors)
				{
					block.RotateTo(Quaternion.Euler(0f, 90f, 0f));
				}
				goto IL_22AA;
			case "Anim Character Mini Female":
				BlockAnimatedCharacter.StripNonCompatibleTiles(tiles);
				block = new BlockAnimatedCharacter(tiles, 1.75f, CharacterType.MiniFemale);
				if (text.Contains("Female") && defaultColors)
				{
					block.RotateTo(Quaternion.Euler(0f, 90f, 0f));
				}
				goto IL_22AA;
			case "Tree Poplar":
				block = new Block(tiles);
				if (defaultColors)
				{
					block.ScaleTo(new Vector3(1f, 3f, 1f), true, false);
				}
				goto IL_22AA;
			case "Slice Inverse":
				block = new BlockSliceInverse(tiles);
				goto IL_22AA;
			case "Water Emitter Block":
				block = new BlockEmitterWater(tiles, false);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "Water Volume Block":
				block = new BlockEmitterWater(tiles, true);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "Fire Emitter Block":
				block = new BlockEmitterFire(tiles, false);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "Fire Volume Block":
				block = new BlockEmitterFire(tiles, true);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "Gas Emitter Block":
				block = new BlockEmitterGas(tiles, false);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "Gas Volume Block":
				block = new BlockEmitterGas(tiles, true);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "Campfire":
				block = new BlockEmitterCampfire(tiles);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
			case "Mannequin":
			case "SAM Statue":
			case "Statue":
				block = new BlockStatue(tiles);
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				goto IL_22AA;
            default:
                    foreach (string key in BlockItemsRegistry.GetBlockEntries().Keys)
                    {
                        if (key == text)
                        {
                            BlockEntry entry = BlockItemsRegistry.GetBlockEntries()[key];
                            if (entry.blockType != null)
                            {
                                Type blockType = entry.blockType;
                                ConstructorInfo info = blockType.GetConstructor(new Type[] { typeof(List<List<Tile>>) });
                                block = info.Invoke(new object[] { tiles }) as Block;
                                if (entry.hasDefaultTiles)
                                {
                                    Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
                                }
                                goto IL_22AA;
                            }
                        }
                    }
                    break;
			}
			if (Blocksworld.compoundColliders.ContainsKey(text))
			{
				block = new BlockCompoundCollider(tiles);
			}
			else if (text.StartsWith("Terrain "))
			{
				block = new BlockTerrain(tiles);
				BWSceneManager.AddTerrainBlock((BlockTerrain)block);
			}
			else if (Block.treasureBlocks.Contains(text))
			{
				if (!Block.defaultExtraTiles.ContainsKey(text))
				{
					Block.defaultExtraTiles[text] = new List<List<Tile>>
					{
						new List<Tile>
						{
							Block.ThenTile(),
							new Tile(new GAF("Block.IsTreasure", new object[]
							{
								0
							}))
						},
						new List<Tile>
						{
							new Tile(new GAF("Block.OnCollect", new object[0])),
							Block.ThenTile(),
							new Tile(new GAF("Block.PlaySoundDurational", new object[]
							{
								Block.treasurePickupSfx,
								"Camera"
							}))
						},
						Block.EmptyTileRow()
					};
				}
				Block.WriteDefaultExtraTiles(defaultTiles, tiles, text);
				if (text == "Idol")
				{
					block = new BlockIdol(tiles);
				}
				else
				{
					block = new Block(tiles);
				}
			}
			else
			{
				block = new Block(tiles);
				Block.RegisterDefaultTilesWithMetaData(block, text);
				if (defaultTiles)
				{
					List<List<Tile>> defaultExtraTilesCopy = Block.GetDefaultExtraTilesCopy(text, false);
					if (defaultExtraTilesCopy != null)
					{
						tiles.RemoveRange(1, tiles.Count - 1);
						tiles.AddRange(defaultExtraTilesCopy);
					}
				}
				if (defaultColors)
				{
					block.PaintTo("Yellow", true);
				}
			}
			IL_22AA:
			if (block != null)
			{
				if (defaultColors)
				{
					Vector3 defaultOrientation = block.GetDefaultOrientation();
					if (defaultOrientation.magnitude > 0.001f)
					{
						block.RotateTo(Quaternion.Euler(defaultOrientation));
					}
					Vector3 defaultScale = block.GetDefaultScale();
					if ((defaultScale - Vector3.one).magnitude > 0.001f)
					{
						block.ScaleTo(defaultScale, true, false);
					}
					string[] array = Scarcity.DefaultPaints(text);
					for (int i = 0; i < array.Length; i++)
					{
						string paint = array[i].Split(new char[]
						{
							','
						})[0];
						block.PaintTo(paint, true, i);
					}
					string[] array2 = Scarcity.DefaultTextures(text);
					Vector3[] array3 = block.GetDefaultTextureNormals();
					if (Block.defaultTextureNormals.ContainsKey(text))
					{
						array3 = Block.defaultTextureNormals[text];
					}
					for (int j = 0; j < array2.Length; j++)
					{
						Vector3 normal = Vector3.up;
						if (array3 != null && j < array3.Length)
						{
							normal = array3[j];
						}
						block.TextureTo(array2[j], normal, true, j, true);
					}
				}
				if (block.canBeTextured == null)
				{
					bool[] array4 = block.GetCanBeTextured();
					if (array4 != null)
					{
						block.canBeTextured = array4;
					}
				}
				if (block.canBeMaterialTextured == null)
				{
					bool[] array5 = block.GetCanBeMaterialTextured();
					if (array5 != null)
					{
						block.canBeMaterialTextured = array5;
					}
				}
				block.buoyancyMultiplier = block.GetBuoyancyMultiplier();
				block.OnCreate();
				return block;
			}
			BWLog.Info("Returning null from NewBlock()");
			return block;
		}

		// Token: 0x060002AB RID: 683 RVA: 0x00017DAC File Offset: 0x000161AC
		private static void RegisterDefaultTilesWithMetaData(Block b, string blockType)
		{
			if (Block.defaultExtraTiles.ContainsKey(blockType))
			{
				return;
			}
			BlockMetaData blockMetaData = b.GetBlockMetaData();
			List<List<Tile>> list = new List<List<Tile>>
			{
				new List<Tile>
				{
					new Tile(new GAF("Meta.Then", new object[0]))
				}
			};
			List<GAF> list2 = new List<GAF>();
			if (blockMetaData != null && blockMetaData.defaultGAFs != null)
			{
				foreach (GAFInfo gafinfo in blockMetaData.defaultGAFs)
				{
					list2.Add(new GAF(gafinfo.Predicate, gafinfo.Args));
				}
			}
			if (blockMetaData != null && blockMetaData.handUse == HandAttachmentType.Shield)
			{
				list[0].Insert(0, new Tile(new GAF("Prop.Blocking", new object[0])));
				object[] args = new object[0];
				list2.Add(new GAF("Block.Invincible", args));
			}
			foreach (GAF gaf in list2)
			{
				list[0].Add(new Tile(gaf));
			}
			if (list2.Count > 0)
			{
				list.Add(new List<Tile>());
				list[1].Add(new Tile(new GAF("Meta.Then", new object[0])));
			}
			Block.defaultExtraTiles[blockType] = list;
		}

		// Token: 0x060002AC RID: 684 RVA: 0x00017F54 File Offset: 0x00016354
		public virtual void OnCreate()
		{
		}

		// Token: 0x060002AD RID: 685 RVA: 0x00017F56 File Offset: 0x00016356
		public virtual void OnReconstructed()
		{
		}

		// Token: 0x060002AE RID: 686 RVA: 0x00017F58 File Offset: 0x00016358
		public BlockMeshMetaData[] GetBlockMeshMetaDatas()
		{
			BlockMetaData blockMetaData = this.GetBlockMetaData();
			if (blockMetaData != null)
			{
				return blockMetaData.meshDatas;
			}
			return new BlockMeshMetaData[0];
		}

		// Token: 0x060002AF RID: 687 RVA: 0x00017F88 File Offset: 0x00016388
		public Vector3 GetDefaultScale()
		{
			string key = this.BlockType();
			if (!Block.defaultScales.ContainsKey(key))
			{
				BlockMetaData blockMetaData = this.GetBlockMetaData();
				if (blockMetaData != null)
				{
					Vector3 vector = blockMetaData.defaultScale;
					if (Util.MinComponent(vector) < 0.1f)
					{
						vector = Vector3.one;
					}
					Block.defaultScales[key] = vector;
				}
				else
				{
					Block.defaultScales[key] = Vector3.one;
				}
			}
			return Block.defaultScales[key];
		}

		// Token: 0x060002B0 RID: 688 RVA: 0x00018008 File Offset: 0x00016408
		public Vector3 GetDefaultOrientation()
		{
			string key = this.BlockType();
			if (!Block.defaultOrientations.ContainsKey(key))
			{
				BlockMetaData blockMetaData = this.GetBlockMetaData();
				if (blockMetaData != null)
				{
					Block.defaultOrientations[key] = blockMetaData.defaultOrientation;
				}
				else
				{
					Block.defaultOrientations[key] = Vector3.zero;
				}
			}
			return Block.defaultOrientations[key];
		}

		// Token: 0x060002B1 RID: 689 RVA: 0x00018070 File Offset: 0x00016470
		public Vector3[] GetDefaultTextureNormals()
		{
			string key = this.BlockType();
			if (!Block.defaultTextureNormals.ContainsKey(key))
			{
				BlockMetaData blockMetaData = this.GetBlockMetaData();
				if (blockMetaData != null)
				{
					List<Vector3> list = new List<Vector3>();
					foreach (BlockMeshMetaData blockMeshMetaData in blockMetaData.meshDatas)
					{
						list.Add(blockMeshMetaData.defaultTextureNormal);
					}
					Block.defaultTextureNormals[key] = list.ToArray();
				}
				else
				{
					Block.defaultTextureNormals[key] = null;
				}
			}
			return Block.defaultTextureNormals[key];
		}

		// Token: 0x060002B2 RID: 690 RVA: 0x0001810D File Offset: 0x0001650D
		public BlockMetaData GetBlockMetaData()
		{
			if (this.meta == null && this.go != null)
			{
				this.meta = this.go.GetComponent<BlockMetaData>();
			}
			return this.meta;
		}

		// Token: 0x060002B3 RID: 691 RVA: 0x00018148 File Offset: 0x00016548
		public bool IsLocked()
		{
			return this.tiles[1].Count > 1 && this.tiles[1][1].IsLocked();
		}

		// Token: 0x060002B4 RID: 692 RVA: 0x0001817C File Offset: 0x0001657C
		public string BlockType()
		{
			return (string)this.tiles[0][1].gaf.Args[0];
		}

		// Token: 0x060002B5 RID: 693 RVA: 0x000181AE File Offset: 0x000165AE
		public int BlockItemId()
		{
			return this.tiles[0][1].gaf.BlockItemId;
		}

		// Token: 0x060002B6 RID: 694 RVA: 0x000181CC File Offset: 0x000165CC
		public GAF BlockCreateGAF()
		{
			return this.tiles[0][1].gaf;
		}

		// Token: 0x060002B7 RID: 695 RVA: 0x000181E8 File Offset: 0x000165E8
		public Tile FindTile(GAF gaf)
		{
			foreach (List<Tile> list in this.tiles)
			{
				foreach (Tile tile in list)
				{
					if (tile.gaf.Equals(gaf))
					{
						return tile;
					}
				}
			}
			return null;
		}

		// Token: 0x060002B8 RID: 696 RVA: 0x0001829C File Offset: 0x0001669C
		public Vector3 BlockSize()
		{
			Vector3 vector = Vector3.one;
			string key = this.BlockType();
			if (!Block.blockSizes.ContainsKey(key))
			{
				BlockMetaData blockMetaData = this.GetBlockMetaData();
				if (blockMetaData != null)
				{
					vector = blockMetaData.blockSize;
					if (Util.MinComponent(vector) < 0.001f)
					{
						vector = Vector3.one;
					}
				}
				Block.blockSizes[key] = vector;
			}
			return Block.blockSizes[key];
		}

		// Token: 0x060002B9 RID: 697 RVA: 0x00018310 File Offset: 0x00016710
		public Vector3 Scale()
		{
			if (this.tiles[0].Count > 6)
			{
				return (Vector3)this.tiles[0][6].gaf.Args[0];
			}
			return new Vector3(1f, 1f, 1f);
		}

		// Token: 0x060002BA RID: 698 RVA: 0x0001836E File Offset: 0x0001676E
		public Vector3 GetScale()
		{
			return this.size;
		}

		// Token: 0x060002BB RID: 699 RVA: 0x00018378 File Offset: 0x00016778
		public float GetMass()
		{
			if (this.blockMassOverride >= 0f)
			{
				return this.blockMassOverride;
			}
			if (this.IsRuntimeInvisible())
			{
				return 0f;
			}
			BlockMetaData blockMetaData = this.GetBlockMetaData();
			if (!this.broken && this.chunk != null && null != blockMetaData && blockMetaData.isBlocksterMassless && this.chunk.HasCharacter())
			{
				return 0f;
			}
			if (this.blockMass > 0f)
			{
				return this.blockMass;
			}
			Vector3 vector = this.Scale();
			if (Block.blockMassConstants == null)
			{
				Block.blockMassConstants = new Dictionary<string, float[]>();
			}
			string text = this.BlockType();
			float num;
			float num2;
			if (text != null)
			{
				if (text == "Terrain Wedge")
				{
					num = 0f;
					num2 = 0.5f;
					goto IL_119;
				}
				if (text == "Hemisphere")
				{
					num = 0f;
					num2 = 0.2617995f;
					goto IL_119;
				}
			}
			num = 0f;
			num2 = 1f;
			IL_119:
			float num3 = 0.75f + 0.25f * vector.x * vector.y * vector.z;
			string key = this.BlockType();
			if (!Block.blockMassConstants.ContainsKey(key))
			{
				BlockMetaData blockMetaData2 = this.GetBlockMetaData();
				if (blockMetaData2 != null)
				{
					num2 = blockMetaData2.massK;
					num = blockMetaData2.massM;
					if (num <= 0f && num2 <= 0f)
					{
						num2 = 1f;
						num = 0f;
					}
				}
				Block.blockMassConstants.Add(key, new float[]
				{
					num2,
					num
				});
			}
			float[] array = Block.blockMassConstants[key];
			float result = array[0] * num3 + array[1];
			this.blockMass = result;
			return result;
		}

		// Token: 0x060002BC RID: 700 RVA: 0x00018564 File Offset: 0x00016964
		public virtual void Reset(bool forceRescale = false)
		{
			this.meshScale = Vector3.one;
			if (forceRescale)
			{
				this.meshScale = Vector3.zero;
			}
			this.meshScaleTexture = string.Empty;
			this.skipUpdateSATVolumes = true;
			if (this.tiles.Count > 0)
			{
				Block.resetExecutionInfo.timer = 0f;
				Block.resetExecutionInfo.floatArg = 1f;
				for (int i = 2; i < this.tiles[0].Count; i++)
				{
					this.tiles[0][i].Execute(this, Block.resetExecutionInfo);
				}
			}
			else
			{
				BWLog.Info("Block reset() without necessary tiles");
			}
			this.skipUpdateSATVolumes = false;
			this.UpdateSATVolumes();
		}

		// Token: 0x060002BD RID: 701 RVA: 0x0001862A File Offset: 0x00016A2A
		public virtual void ResetFrame()
		{
		}

		// Token: 0x060002BE RID: 702 RVA: 0x0001862C File Offset: 0x00016A2C
		public void CheckContainsPlayModeTiles()
		{
			this.containsPlayModeTiles = false;
			if (Block.notPlayModePredicates == null)
			{
				Block.notPlayModePredicates = new HashSet<Predicate>
				{
					Block.predicateLocked,
					Block.predicateThen,
					Block.predicateUnlocked,
					Block.predicateTutorialCreateBlockHint,
					Block.predicateTutorialRemoveBlockHint,
					Block.predicateTutorialOperationPose,
					Block.predicateTutorialPaintExistingBlock,
					Block.predicateTutorialTextureExistingBlock,
					Block.predicateTutorialRotateExistingBlock,
					Block.predicateTutorialMoveBlock,
					Block.predicateTutorialMoveModel
				};
			}
			this.containsPlayModeTiles = !BWSceneManager.PlayBlockPredicates(this).IsSubsetOf(Block.notPlayModePredicates);
		}

		// Token: 0x060002BF RID: 703 RVA: 0x000186F4 File Offset: 0x00016AF4
		public virtual void Play()
		{
			this.broken = false;
			this.didFix = false;
			this.vanished = false;
			this.isTreasure = false;
			this.gluedOnContact = false;
			this.allowGlueOnContact = true;
			this.lastTeleportedFrameCount = -1;
			this.blockMassOverride = -1f;
			this.storedMassOverride = -1f;
			this.overridingMass = false;
			this.rbConstraintsOn = 0;
			this.rbConstraintsOff = 0;
			this.rbUpdatedConstraints = false;
			if (!this.go.activeSelf && this.activateForPlay)
			{
				this.go.SetActive(true);
			}
			Collider component = this.go.GetComponent<Collider>();
			component.contactOffset = 0.01f;
			this.colliderName = component.name;
			Transform transform = this.go.transform;
			Transform parent = transform.parent;
			this.playPosition = transform.position;
			this.oldPos = Util.nullVector3;
			this.playRotation = transform.rotation;
			if (parent != null)
			{
				this.parentPlayPosition = parent.position;
				this.parentPlayRotation = parent.rotation;
			}
			else
			{
				this.parentPlayPosition = this.playPosition;
				this.parentPlayRotation = this.playRotation;
			}
			this.animationStep = 0;
			this.lastShadowHitDistance = -2f;
			if (this.audioSource != null)
			{
				this.audioSource.Stop();
				this.audioSource.volume = 1f;
			}
			this.UpdatePhysicMaterialsForTextureAssignment();
			this.UpdateBlockPropertiesForAllTextureAssignments(false);
			if (this.subMeshGameObjects.Count > 0)
			{
				this.SetSubMeshVisibility(true);
			}
			this.meshScaleTexture = string.Empty;
			this.isRuntimePhantom = false;
			this.goLayerAssignment = ((!this.isTerrain) ? Layer.Default : Layer.Terrain);
			this.UpdateRuntimeInvisible();
			this.shadowUpdateCounter = UnityEngine.Random.Range(0, 1000);
			if (this.goShadow != null && this.CanMergeShadow())
			{
				float sqrMagnitude = this.Scale().sqrMagnitude;
				float num = 8f;
				if (sqrMagnitude < num * num)
				{
					for (int i = 0; i < this.connections.Count; i++)
					{
						Block block = this.connections[i];
						if (block.CanMergeShadow())
						{
							if (Mathf.Abs(this.connectionTypes[i]) == 1)
							{
								float sqrMagnitude2 = block.shadowSize.sqrMagnitude;
								if (sqrMagnitude2 < 16f && (block.goShadow == null || sqrMagnitude2 < sqrMagnitude))
								{
									this.shadowSize += Vector3.one * Util.MinComponent(block.shadowSize);
									block.DestroyShadow();
									if (this.shadowSize.sqrMagnitude > num * num)
									{
										break;
									}
								}
							}
						}
					}
				}
			}
			for (int j = 0; j < this.subMeshPaints.Count + 1; j++)
			{
				this.RegisterPaintChanged(j, this.GetPaint(j), null);
				this.RegisterTextureChanged(j, this.GetTexture(j), null);
			}
		}

		// Token: 0x060002C0 RID: 704 RVA: 0x00018A20 File Offset: 0x00016E20
		protected virtual void UpdateRuntimeInvisible()
		{
			bool flag = false;
			for (int i = 0; i < ((this.childMeshes != null) ? (this.childMeshes.Count + 1) : 1); i++)
			{
				string texture = this.GetTexture(i);
				if (texture != "Invisible")
				{
					flag = true;
					break;
				}
			}
			bool flag2 = this.isRuntimeInvisible;
			this.isRuntimeInvisible = !flag;
			this.go.layer = (int)((!this.isTransparent) ? ((!this.isRuntimePhantom) ? this.goLayerAssignment : Layer.Phantom) : Layer.TransparentFX);
			if (flag2 != this.isRuntimeInvisible)
			{
				this.UpdateNeighboringConnections();
			}
		}

		// Token: 0x060002C1 RID: 705 RVA: 0x00018AD4 File Offset: 0x00016ED4
		public virtual bool IsRuntimeInvisible()
		{
			return this.vanished || this.isRuntimeInvisible;
		}

		// Token: 0x060002C2 RID: 706 RVA: 0x00018AE9 File Offset: 0x00016EE9
		public bool IsProfileCharacter()
		{
			return ProfileBlocksterUtils.IsProfileBlockType(this.BlockType());
		}

		// Token: 0x060002C3 RID: 707 RVA: 0x00018AF6 File Offset: 0x00016EF6
		public virtual bool CanMergeShadow()
		{
			return true;
		}

		// Token: 0x060002C4 RID: 708 RVA: 0x00018AFC File Offset: 0x00016EFC
		public void MakeFixedAndSpawnpointBeforeFirstFrame()
		{
			if (this.frozenInTerrainStatus == -1)
			{
				this.UpdateFrozenInTerrainStatus();
			}
			if (this.frozenInTerrainStatus == 1 || this.IsFixed())
			{
				this.Freeze(null, null);
			}
			Predicate predicate = Block.predicateSetSpawnPoint;
			Predicate predicate2 = Block.predicateFirstFrame;
			for (int i = 0; i < this.tiles.Count; i++)
			{
				List<Tile> list = this.tiles[i];
				Predicate predicate3 = (list.Count <= 0) ? null : list[0].gaf.Predicate;
				Predicate predicate4 = (list.Count <= 1) ? null : list[1].gaf.Predicate;
				if (predicate3 == Block.predicateThen || (predicate3 == predicate2 && predicate4 == Block.predicateThen))
				{
					for (int j = 0; j < list.Count; j++)
					{
						GAF gaf = list[j].gaf;
						if (gaf.Predicate == predicate)
						{
							int intArg = Util.GetIntArg(gaf.Args, 0, 0);
							CheckpointSystem.SetSpawnPoint(this, intArg);
						}
					}
				}
			}
		}

		// Token: 0x060002C5 RID: 709 RVA: 0x00018C2C File Offset: 0x0001702C
		public void HandleHideOnPlay()
		{
			Predicate predicate = Block.predicateFirstFrame;
			for (int i = 0; i < this.tiles.Count; i++)
			{
				List<Tile> list = this.tiles[i];
				Predicate predicate2 = (list.Count <= 0) ? null : list[0].gaf.Predicate;
				Predicate predicate3 = (list.Count <= 1) ? null : list[1].gaf.Predicate;
				if (predicate2 == predicate && predicate3 == Block.predicateThen)
				{
					for (int j = 0; j < list.Count; j++)
					{
						GAF gaf = list[j].gaf;
						if (gaf.Predicate == Block.predicateVanishModel)
						{
							this.VanishModel(0f, false, false, 0f);
						}
						else if (gaf.Predicate == Block.predicateVanishBlock)
						{
							this.VanishBlock(false, 0f);
						}
					}
				}
			}
		}

		// Token: 0x060002C6 RID: 710 RVA: 0x00018D38 File Offset: 0x00017138
		private void UpdatePhysicMaterialsForTextureAssignment()
		{
			Collider component = this.go.GetComponent<Collider>();
			if (component != null)
			{
				PhysicMaterial physicMaterialTexture = MaterialTexture.GetPhysicMaterialTexture(this.GetTexture(0));
				if (physicMaterialTexture != null)
				{
					component.sharedMaterial = physicMaterialTexture;
				}
				else
				{
					component.sharedMaterial = this.GetDefaultPhysicMaterialForPrefabCollider(0);
				}
			}
			for (int i = 0; i < this.subMeshGameObjects.Count; i++)
			{
				component = this.subMeshGameObjects[i].GetComponent<Collider>();
				if (component != null)
				{
					PhysicMaterial physicMaterialTexture2 = MaterialTexture.GetPhysicMaterialTexture(this.GetTexture(i + 1));
					if (physicMaterialTexture2 != null)
					{
						component.sharedMaterial = physicMaterialTexture2;
					}
					else
					{
						component.sharedMaterial = this.GetDefaultPhysicMaterialForPrefabCollider(i + 1);
					}
				}
			}
		}

		// Token: 0x060002C7 RID: 711 RVA: 0x00018DFF File Offset: 0x000171FF
		protected virtual int GetPrimaryMeshIndex()
		{
			return 0;
		}

		// Token: 0x060002C8 RID: 712 RVA: 0x00018E04 File Offset: 0x00017204
		protected void UpdateBlockPropertiesForAllTextureAssignments(bool forceEnabled)
		{
			for (int i = 0; i <= this.subMeshGameObjects.Count; i++)
			{
				this.UpdateBlockPropertiesForTextureAssignment(i, forceEnabled);
			}
		}

		// Token: 0x060002C9 RID: 713 RVA: 0x00018E38 File Offset: 0x00017238
		protected virtual void UpdateBlockPropertiesForTextureAssignment(int meshIndex, bool forceEnabled)
		{
			int primaryMeshIndex = this.GetPrimaryMeshIndex();
			if (meshIndex == primaryMeshIndex)
			{
				bool flag = this.isTransparent;
				bool flag2 = this.isRuntimeInvisible;
				string texture = this.GetTexture(primaryMeshIndex);
				this.isTransparent = Materials.TextureIsTransparent(texture);
				if (texture == "Metal")
				{
					this.buoyancyMultiplier = 0.2f;
				}
				else
				{
					this.buoyancyMultiplier = this.GetBuoyancyMultiplier();
				}
				this.isRuntimeInvisible = (texture == "Invisible");
				bool enabled = forceEnabled || !this.isRuntimeInvisible;
				if (this.goShadow != null)
				{
					this.goShadow.GetComponent<Renderer>().enabled = enabled;
				}
				Renderer component = this.go.GetComponent<Renderer>();
				if (component != null)
				{
					component.enabled = enabled;
				}
				if (flag != this.isTransparent || flag2 != this.isRuntimeInvisible)
				{
					this.UpdateNeighboringConnections();
				}
			}
			int num = meshIndex - 1;
			if (num >= 0 && num < this.subMeshGameObjects.Count)
			{
				string texture2 = this.GetTexture(num + 1);
				bool enabled2 = forceEnabled || texture2 != "Invisible";
				this.subMeshGameObjects[num].GetComponent<Renderer>().enabled = enabled2;
			}
		}

		// Token: 0x060002CA RID: 714 RVA: 0x00018F85 File Offset: 0x00017385
		public virtual void Play2()
		{
		}

		// Token: 0x060002CB RID: 715 RVA: 0x00018F88 File Offset: 0x00017388
		public virtual void Stop(bool resetBlock = true)
		{
			if (Block.massAlteredBlocks.Contains(this))
			{
				Block.massAlteredBlocks.Remove(this);
			}
			this.RestoreMeshColliderInfo();
			if (!Blocksworld.renderingShadows && this.HasShadow() && this.goShadow == null)
			{
				this.InstantiateShadow(Block.prefabShadow);
			}
			this.shadowSize = this.size;
			if (this.goT.root != null && this.goT.parent != null)
			{
				GameObject gameObject = this.goT.parent.gameObject;
				Util.UnparentTransformSafely(this.goT);
				if (this.goT.root == this.goT.parent)
				{
					UnityEngine.Object.Destroy(gameObject);
				}
			}
			this.meshScaleTexture = string.Empty;
			this.modelBlock = null;
			if (this.go.GetComponent<Renderer>() != null && !this.go.GetComponent<Renderer>().enabled)
			{
				this.go.GetComponent<Renderer>().enabled = true;
			}
			if (this.go.GetComponent<Collider>() != null)
			{
				this.go.GetComponent<Collider>().enabled = true;
				this.go.GetComponent<Collider>().isTrigger = false;
			}
			if (this.goShadow != null)
			{
				this.goShadow.GetComponent<Renderer>().enabled = true;
			}
			this.go.layer = (int)this.goLayerAssignment;
			this.goT.localScale = Vector3.one;
			this.go.SetActive(this.activateForPlay);
			RewardVisualization.rewardAnimationRunning = false;
			this.didFix = false;
			this.vanished = false;
			this.isTreasure = false;
			this.broken = false;
			this.gluedOnContact = false;
			this.allowGlueOnContact = true;
			this.lastTeleportedFrameCount = -1;
			if (resetBlock)
			{
				this.Reset(false);
			}
			this.lastShadowHitDistance = -2f;
			this.shadowUpdateInterval = 1;
			if (this.audioSource != null)
			{
				this.audioSource.Stop();
				this.audioSource.volume = 0f;
			}
			if (this.subMeshGameObjects.Count > 0)
			{
				this.SetSubMeshVisibility(false);
			}
			this.UpdateBlockPropertiesForAllTextureAssignments(true);
			this.chunk = null;
		}

		// Token: 0x060002CC RID: 716 RVA: 0x000191E4 File Offset: 0x000175E4
		public virtual void Pause()
		{
		}

		// Token: 0x060002CD RID: 717 RVA: 0x000191E6 File Offset: 0x000175E6
		public virtual void Resume()
		{
		}

		// Token: 0x060002CE RID: 718 RVA: 0x000191E8 File Offset: 0x000175E8
		public virtual void Break(Vector3 chunkPos, Vector3 chunkVel, Vector3 chunkAngVel)
		{
			this.broken = true;
		}

		// Token: 0x060002CF RID: 719 RVA: 0x000191F1 File Offset: 0x000175F1
		public virtual void OnHudMesh()
		{
		}

		// Token: 0x060002D0 RID: 720 RVA: 0x000191F3 File Offset: 0x000175F3
		public virtual void Update()
		{
			if (!Blocksworld.renderingShadows && this.hasShadow && !this.vanished)
			{
				this.UpdateShadow();
			}
		}

		// Token: 0x060002D1 RID: 721 RVA: 0x0001921C File Offset: 0x0001761C
		private float TotalBlockAlpha()
		{
			float num = (!Materials.TextureIsTransparent(this.GetTexture(0))) ? 1f : 0.3f;
			if (this.subMeshTextures.Count > 0)
			{
				for (int i = 0; i < this.subMeshTextures.Count; i++)
				{
					num += ((!Materials.TextureIsTransparent(this.subMeshTextures[i])) ? 1f : 0.3f);
				}
				num /= (float)(this.subMeshTextures.Count + 1);
			}
			return num;
		}

		// Token: 0x060002D2 RID: 722 RVA: 0x000192B4 File Offset: 0x000176B4
		protected void SetShadowAlpha(float alpha)
		{
			alpha *= this.shadowStrengthMultiplier * this.TotalBlockAlpha();
			if (alpha != this.oldShadowAlpha)
			{
				this.colorsShadow[0].a = alpha;
				this.colorsShadow[1].a = alpha;
				this.colorsShadow[2].a = alpha;
				this.colorsShadow[3].a = alpha;
				this.meshShadow.colors = this.colorsShadow;
				this.oldShadowAlpha = alpha;
			}
		}

		// Token: 0x060002D3 RID: 723 RVA: 0x00019340 File Offset: 0x00017740
		private bool UpdateShadowDefined(Vector3 goPos, float maxDist, ref bool shadowDefined)
		{
			Vector3 rhs = goPos - Blocksworld.cameraPosition;
			float sqrMagnitude = rhs.sqrMagnitude;
			float num = maxDist * maxDist;
			if (sqrMagnitude > num)
			{
				this.SetShadowAlpha(0f);
				this.lastShadowHitDistance = -3f;
				this.shadowUpdateInterval = Mathf.Min(10, 5 + Mathf.RoundToInt(0.1f * Mathf.Sqrt(sqrMagnitude - num)));
				return false;
			}
			if (this.lastShadowHitDistance == -3f)
			{
				this.lastShadowHitDistance = -2f;
				this.shadowUpdateInterval = 1;
				shadowDefined = false;
			}
			Block.shadowBounds.center = goPos;
			Block.shadowBounds.size = this.shadowSize;
			if (!GeometryUtility.TestPlanesAABB(Blocksworld.frustumPlanes, Block.shadowBounds))
			{
				this.SetShadowAlpha(0f);
				this.lastShadowHitDistance = -4f;
				this.shadowUpdateInterval = 5;
				if (Vector3.Dot(Blocksworld.cameraForward, rhs) < 0f)
				{
					this.shadowUpdateInterval = 8;
				}
				return false;
			}
			if (this.lastShadowHitDistance == -4f)
			{
				this.lastShadowHitDistance = -2f;
				this.shadowUpdateInterval = 1;
				shadowDefined = false;
			}
			return true;
		}

		// Token: 0x060002D4 RID: 724 RVA: 0x0001945C File Offset: 0x0001785C
		private void PlaceShadow(bool shadowDefined, bool moved, Vector3 goPos, float maxDist, Quaternion goRot)
		{
			RaycastHit raycastHit = default(RaycastHit);
			Vector3 b = 0.45f * Vector3.up;
			bool flag = shadowDefined && this.lastShadowHitDistance >= 0f && this.lastShadowHitDistance < this.shadowMaxDistance;
			bool flag2 = flag && !moved;
			bool flag3 = flag2;
			Vector3 vector = default(Vector3);
			Vector3 vector2 = default(Vector3);
			float num = 0f;
			if (flag2)
			{
				num = this.lastShadowHitDistance;
				vector = this.lastShadowHitPoint;
				vector2 = this.oldShadowHitNormal;
			}
			else if (Physics.Raycast(goPos + b, Vector3.down, out raycastHit, this.shadowMaxDistance, 16))
			{
				vector = raycastHit.point;
				vector2 = raycastHit.normal;
				num = raycastHit.distance;
				flag3 = true;
			}
			if (flag3)
			{
				this.lastShadowHitDistance = num;
				this.lastShadowHitPoint = vector;
				Vector3 cameraPosition = Blocksworld.cameraPosition;
				RaycastHit raycastHit2;
				if (Physics.Raycast(cameraPosition, this.lastShadowHitPoint - cameraPosition, out raycastHit2, maxDist, 16))
				{
					Vector3 point = raycastHit2.point;
					if ((vector - point).sqrMagnitude > 1f && (cameraPosition - vector).sqrMagnitude > (cameraPosition - point).sqrMagnitude)
					{
						this.SetShadowAlpha(0f);
						this.shadowUpdateInterval = 5;
						return;
					}
				}
				float shadowAlpha = Mathf.Clamp(0.3f - num * (0.3f / this.shadowMaxDistance), 0f, 1f);
				this.SetShadowAlpha(shadowAlpha);
				Vector3 vector3 = vector;
				this.goShadowT.position = vector3;
				float num2 = (moved && Blocksworld.CurrentState == State.Play) ? 0.95f : 0f;
				Vector3 vector4 = num2 * this.oldShadowHitNormal + (1f - num2) * vector2;
				this.oldShadowHitNormal = vector4;
				if (this.shadowSize != Vector3.one)
				{
					Vector3 b2 = Util.Abs(goRot * this.shadowSize);
					Vector3 up = this.goT.up;
					Vector3 forward = this.goT.forward;
					Vector3 right = this.goT.right;
					if (Mathf.Abs(up.y) > Mathf.Abs(forward.y) && Mathf.Abs(up.y) > Mathf.Abs(right.y))
					{
						this.goShadowT.localScale = new Vector3(this.shadowSize.x + 1f, 1f, this.shadowSize.z + 1f);
						b2 = Util.ProjectOntoPlane(forward, vector2);
						this.goShadowT.LookAt(vector3 + b2, vector4);
					}
					else if (Mathf.Abs(forward.y) > Mathf.Abs(right.y))
					{
						this.goShadowT.localScale = new Vector3(this.shadowSize.x + 1f, 1f, this.shadowSize.y + 1f);
						b2 = Util.ProjectOntoPlane(up, vector2);
						this.goShadowT.LookAt(vector3 + b2, vector4);
					}
					else
					{
						this.goShadowT.localScale = new Vector3(this.shadowSize.y + 1f, 1f, this.shadowSize.z + 1f);
						b2 = Util.ProjectOntoPlane(forward, vector2);
						this.goShadowT.LookAt(vector3 + b2, vector4);
					}
				}
				else
				{
					this.goShadowT.rotation = Quaternion.FromToRotation(Vector3.up, vector4);
				}
			}
			else
			{
				if (this.lastShadowHitDistance == -1f)
				{
					return;
				}
				this.lastShadowHitDistance = -1f;
				this.SetShadowAlpha(0f);
				this.shadowUpdateInterval = 4;
			}
		}

		// Token: 0x060002D5 RID: 725 RVA: 0x00019868 File Offset: 0x00017C68
		private bool TryPruneShadow(Vector3 goPos, Quaternion goRot, ref bool shadowDefined, ref bool moved, ref float maxDist)
		{
			this.shadowUpdateInterval = 3;
			moved = ((goPos - this.oldPos).sqrMagnitude > 1.00000011E-06f);
			if (moved)
			{
				this.oldPos = goPos;
			}
			if (!moved && this.lastShadowHitDistance == -1f)
			{
				this.shadowUpdateInterval = 5;
				this.SetShadowAlpha(0f);
				return true;
			}
			Vector4 vector = new Vector4(goRot.x - this.oldRotation.x, goRot.y - this.oldRotation.y, goRot.z - this.oldRotation.z, goRot.w - this.oldRotation.w);
			bool flag = vector.sqrMagnitude > 0.0001f;
			if (flag)
			{
				this.oldRotation = goRot;
			}
			shadowDefined = (this.lastShadowHitDistance != -2f);
			if (!moved && !flag && !Blocksworld.cameraMoved && shadowDefined)
			{
				return true;
			}
			maxDist = Mathf.Clamp(150f * Util.MaxComponent(this.shadowSize), Mathf.Min(150f, Blocksworld.fogEnd - 1f), Blocksworld.fogEnd);
			return shadowDefined && !this.UpdateShadowDefined(goPos, maxDist, ref shadowDefined);
		}

		// Token: 0x060002D6 RID: 726 RVA: 0x000199C4 File Offset: 0x00017DC4
		protected virtual void UpdateShadow()
		{
			if (this.didFix && this.lastShadowHitDistance == -1f)
			{
				return;
			}
			this.shadowUpdateCounter++;
			Vector3 position = this.goT.position;
			if (this.lastShadowHitDistance > 0f)
			{
				float num = position.y - this.lastShadowHitDistance + 0.45f;
				float f = num - this.lastShadowHitPoint.y;
				if ((double)Mathf.Abs(f) > 0.02)
				{
					this.shadowUpdateInterval = 1;
				}
				this.goShadowT.position = new Vector3(position.x, num, position.z);
			}
			if (this.shadowUpdateCounter % this.shadowUpdateInterval != 0)
			{
				return;
			}
			bool shadowDefined = true;
			bool moved = true;
			float maxDist = 0f;
			Quaternion rotation = this.goT.rotation;
			if (this.TryPruneShadow(position, rotation, ref shadowDefined, ref moved, ref maxDist))
			{
				return;
			}
			this.PlaceShadow(shadowDefined, moved, position, maxDist, rotation);
		}

		// Token: 0x060002D7 RID: 727 RVA: 0x00019AC4 File Offset: 0x00017EC4
		public virtual void FixedUpdate()
		{
			if (this.chunk.rb != null && this.rbUpdatedConstraints)
			{
				this.chunk.rb.constraints = (RigidbodyConstraints)this.GetRigidbodyConstraintsMask();
				this.rbUpdatedConstraints = false;
			}
		}

		// Token: 0x060002D8 RID: 728 RVA: 0x00019B04 File Offset: 0x00017F04
		public void CreateFlattenTiles()
		{
			int count = this.GetRuntimeTiles().Count;
			if (this.executionInfos.Length != count)
			{
				Array.Resize<ScriptRowExecutionInfo>(ref this.executionInfos, count);
			}
			for (int i = 0; i < count; i++)
			{
				ScriptRowExecutionInfo scriptRowExecutionInfo = new ScriptRowExecutionInfo(i, this);
				this.executionInfos[i] = scriptRowExecutionInfo;
			}
		}

		// Token: 0x060002D9 RID: 729 RVA: 0x00019B5A File Offset: 0x00017F5A
		private void RunCondition(ScriptRowExecutionInfo info)
		{
			if (info.beforeThen)
			{
				info.RunRow();
			}
		}

		// Token: 0x060002DA RID: 730 RVA: 0x00019B6D File Offset: 0x00017F6D
		private void RunAction(ScriptRowExecutionInfo info)
		{
			if (!info.beforeThen)
			{
				info.RunRow();
			}
		}

		// Token: 0x060002DB RID: 731 RVA: 0x00019B80 File Offset: 0x00017F80
		public void RunConditions()
		{
			for (int i = 1; i < this.executionInfos.Length; i++)
			{
				ScriptRowExecutionInfo scriptRowExecutionInfo = this.executionInfos[i];
				if (scriptRowExecutionInfo.predicateTiles.Length > 1)
				{
					this.RunCondition(this.executionInfos[i]);
				}
			}
		}

		// Token: 0x060002DC RID: 732 RVA: 0x00019BCC File Offset: 0x00017FCC
		public void RunActions()
		{
			for (int i = 1; i < this.executionInfos.Length; i++)
			{
				ScriptRowExecutionInfo scriptRowExecutionInfo = this.executionInfos[i];
				if (scriptRowExecutionInfo.predicateTiles.Length > 1)
				{
					this.RunAction(scriptRowExecutionInfo);
				}
			}
		}

		// Token: 0x060002DD RID: 733 RVA: 0x00019C10 File Offset: 0x00018010
		public void RunFirstFrameActions()
		{
			for (int i = 1; i < this.executionInfos.Length; i++)
			{
				List<Tile> list = this.GetRuntimeTiles()[i];
				if (list.Count > 1)
				{
					ScriptRowExecutionInfo scriptRowExecutionInfo = this.executionInfos[i];
					bool flag = scriptRowExecutionInfo.predicateTiles[0] == Block.predicateThen;
					flag |= (scriptRowExecutionInfo.predicateTiles[0] == Block.predicateFirstFrame);
					if (!flag)
					{
						int num = 0;
						while (scriptRowExecutionInfo.predicateTiles[num] != Block.predicateThen)
						{
							flag |= (scriptRowExecutionInfo.predicateTiles[num] == Block.predicateFirstFrame);
							num++;
						}
					}
					if (flag)
					{
						this.RunCondition(scriptRowExecutionInfo);
						this.RunAction(scriptRowExecutionInfo);
					}
				}
			}
		}

		// Token: 0x060002DE RID: 734 RVA: 0x00019CC8 File Offset: 0x000180C8
		public virtual void IgnoreRaycasts(bool value)
		{
			this.go.layer = (int)((!value) ? this.goLayerAssignment : Layer.IgnoreRaycast);
		}

		// Token: 0x060002DF RID: 735 RVA: 0x00019CE7 File Offset: 0x000180E7
		public Vector3 GetPosition()
		{
			return (Vector3)this.tiles[0][2].gaf.Args[0];
		}

		// Token: 0x060002E0 RID: 736 RVA: 0x00019D0C File Offset: 0x0001810C
		public virtual Quaternion GetRotation()
		{
			return Quaternion.Euler((Vector3)this.tiles[0][3].gaf.Args[0]);
		}

		// Token: 0x060002E1 RID: 737 RVA: 0x00019D36 File Offset: 0x00018136
		public virtual void EnableCollider(bool value)
		{
			if (this.go != null)
			{
				this.go.GetComponent<Collider>().enabled = value;
			}
		}

		// Token: 0x060002E2 RID: 738 RVA: 0x00019D5A File Offset: 0x0001815A
		public bool IsColliderEnabled()
		{
			return this.go != null && this.go.GetComponent<Collider>().enabled;
		}

		// Token: 0x060002E3 RID: 739 RVA: 0x00019D80 File Offset: 0x00018180
		public HashSet<string> GetNoShapeCollideClasses()
		{
			BlockMetaData blockMetaData = this.GetBlockMetaData();
			if (blockMetaData != null)
			{
				string[] noShapeCollideClasses = blockMetaData.noShapeCollideClasses;
				if (noShapeCollideClasses.Length > 0)
				{
					return new HashSet<string>(noShapeCollideClasses);
				}
			}
			return Block.emptySet;
		}

		// Token: 0x060002E4 RID: 740 RVA: 0x00019DBC File Offset: 0x000181BC
		public HashSet<string> GetShapeCategories()
		{
			BlockMetaData blockMetaData = this.GetBlockMetaData();
			if (blockMetaData != null)
			{
				string[] shapeCategories = blockMetaData.shapeCategories;
				if (shapeCategories.Length > 0)
				{
					return new HashSet<string>(shapeCategories);
				}
			}
			return Block.emptySet;
		}

		// Token: 0x060002E5 RID: 741 RVA: 0x00019DF8 File Offset: 0x000181F8
		public bool IsAnimatedCharacterAttachment(Block characterBlock)
		{
			BlockMetaData blockMetaData = this.GetBlockMetaData();
			return !(blockMetaData == null) && (blockMetaData.isBlocksterMassless || (!base.GetType().IsSubclassOf(typeof(Block)) && characterBlock.goT.InverseTransformPoint(this.goT.position).y < 0f));
		}

		// Token: 0x060002E6 RID: 742 RVA: 0x00019E6C File Offset: 0x0001826C
		public bool IsColliderHit(Collider other)
		{
			return this.go.GetComponent<Collider>() == other;
		}

		// Token: 0x060002E7 RID: 743 RVA: 0x00019E80 File Offset: 0x00018280
		public virtual bool IsColliding(float terrainOffset = 0f, HashSet<Block> exclude = null)
		{
			Bounds bounds = this.GetBounds();
			return (!this.isTerrain && !this.GetNoShapeCollideClasses().Contains("Terrain") && Util.PointWithinTerrain(bounds.center - new Vector3(0f, bounds.size.y * terrainOffset, 0f), false)) || CollisionTest.Collision(this, exclude);
		}

		// Token: 0x060002E8 RID: 744 RVA: 0x00019EFC File Offset: 0x000182FC
		public bool ContainsBlock(Block block)
		{
			return this == block;
		}

		// Token: 0x060002E9 RID: 745 RVA: 0x00019F04 File Offset: 0x00018304
		public virtual void UpdateSATVolumes()
		{
			if (this.skipUpdateSATVolumes)
			{
				return;
			}
			string key = this.BlockType();
			this.CreateCollisionMeshes(ref this.glueMeshes, Blocksworld.glues[key]);
			this.CreateCollisionMeshes(ref this.shapeMeshes, Blocksworld.shapes[key]);
			this.CreateCollisionMeshes(ref this.jointMeshes, Blocksworld.joints[key]);
		}

		// Token: 0x060002EA RID: 746 RVA: 0x00019F69 File Offset: 0x00018369
		protected virtual void TranslateSATVolumes(Vector3 offset)
		{
			if (this.skipUpdateSATVolumes)
			{
				return;
			}
			CollisionVolumes.TranslateMeshes(this.glueMeshes, offset);
			CollisionVolumes.TranslateMeshes(this.shapeMeshes, offset);
			CollisionVolumes.TranslateMeshes(this.jointMeshes, offset);
		}

		// Token: 0x060002EB RID: 747 RVA: 0x00019F9C File Offset: 0x0001839C
		private void CreateCollisionMeshes(ref CollisionMesh[] meshes, GameObject prefab)
		{
			if (prefab == null)
			{
				meshes = new CollisionMesh[0];
				return;
			}
			Vector3 scale = this.CollisionVolumesScale(meshes);
			if (prefab.name == "Joint Motor Cube")
			{
				scale = new Vector3(1f, scale.y, 1f);
			}
			CollisionVolumes.FromPrefab(prefab, this.goT, scale, ref meshes);
		}

		// Token: 0x060002EC RID: 748 RVA: 0x0001A002 File Offset: 0x00018402
		protected virtual Vector3 CollisionVolumesScale(CollisionMesh[] meshes)
		{
			return this.Scale();
		}

		// Token: 0x060002ED RID: 749 RVA: 0x0001A00A File Offset: 0x0001840A
		public TileResultCode MoveToAction(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!this.MoveTo((Vector3)args[0])) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x060002EE RID: 750 RVA: 0x0001A026 File Offset: 0x00018426
		public virtual void TBoxStartRotate()
		{
		}

		// Token: 0x060002EF RID: 751 RVA: 0x0001A028 File Offset: 0x00018428
		public virtual void TBoxStopRotate()
		{
		}

		// Token: 0x060002F0 RID: 752 RVA: 0x0001A02A File Offset: 0x0001842A
		public virtual void TBoxStopScale()
		{
		}

		// Token: 0x060002F1 RID: 753 RVA: 0x0001A02C File Offset: 0x0001842C
		public virtual bool TBoxMoveTo(Vector3 pos, bool forced = false)
		{
			return this.MoveTo(pos);
		}

		// Token: 0x060002F2 RID: 754 RVA: 0x0001A035 File Offset: 0x00018435
		public virtual void UpdateFrozenInTerrainStatus()
		{
			this.frozenInTerrainStatus = ((!this.IsFixedInTerrain()) ? 0 : 1);
		}

		// Token: 0x060002F3 RID: 755 RVA: 0x0001A050 File Offset: 0x00018450
		public virtual bool MoveTo(Vector3 pos)
		{
			if (Util.IsNullVector3(pos))
			{
				return true;
			}
			Vector3 vector = pos - this.goT.position;
			this.goT.position = pos;
			object[] args = this.tiles[0][2].gaf.Args;
			Vector3 b = (Vector3)args[0];
			args[0] = pos;
			if (vector != Vector3.zero)
			{
				this.TranslateSATVolumes(vector);
			}
			if ((pos - b).sqrMagnitude > 0.0001f)
			{
				this.frozenInTerrainStatus = -1;
			}
			this.lastShadowHitDistance = -2f;
			this.shadowUpdateInterval = 1;
			return true;
		}

		// Token: 0x060002F4 RID: 756 RVA: 0x0001A101 File Offset: 0x00018501
		public TileResultCode RotateToAction(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!this.RotateTo(Quaternion.Euler((Vector3)args[0]))) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x060002F5 RID: 757 RVA: 0x0001A122 File Offset: 0x00018522
		public virtual bool TBoxRotateTo(Quaternion rot)
		{
			return this.RotateTo(rot);
		}

		// Token: 0x060002F6 RID: 758 RVA: 0x0001A12C File Offset: 0x0001852C
		public virtual bool RotateTo(Quaternion rot)
		{
			bool flag = rot != this.goT.rotation;
			this.goT.rotation = rot;
			Vector3 vector = rot.eulerAngles;
			vector = Util.Round(vector * 100f) / 100f;
			object[] args = this.tiles[0][3].gaf.Args;
			Vector3 b = (Vector3)args[0];
			args[0] = vector;
			if (flag)
			{
				this.UpdateSATVolumes();
			}
			if ((vector - b).sqrMagnitude > 0.0001f)
			{
				this.frozenInTerrainStatus = -1;
			}
			this.lastShadowHitDistance = -2f;
			this.shadowUpdateInterval = 1;
			return true;
		}

		// Token: 0x060002F7 RID: 759 RVA: 0x0001A1EC File Offset: 0x000185EC
		public Vector3 CanScale()
		{
			Vector3 value = Vector3.zero;
			string text = this.BlockType();
			if (Block.canScales == null)
			{
				Block.canScales = new Dictionary<string, Vector3>();
			}
			if (text != null)
			{
				if (text == "Terrain Cube" || text == "Terrain Wedge" || text == "smoothCone" || text == "Crenelation")
				{
					return Vector3.one;
				}
			}
			if (!Block.canScales.ContainsKey(text))
			{
				BlockMetaData blockMetaData = this.GetBlockMetaData();
				if (blockMetaData != null)
				{
					value = blockMetaData.canScale;
				}
				Block.canScales.Add(text, value);
			}
			return Block.canScales[text];
		}

		// Token: 0x060002F8 RID: 760 RVA: 0x0001A2AC File Offset: 0x000186AC
		public bool IsScaled()
		{
			return this.isScaled;
		}

		// Token: 0x060002F9 RID: 761 RVA: 0x0001A2B4 File Offset: 0x000186B4
		public TileResultCode ScaleToAction(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!this.ScaleTo((Vector3)args[0], true, false)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x060002FA RID: 762 RVA: 0x0001A2D2 File Offset: 0x000186D2
		public virtual bool TBoxScaleTo(Vector3 scale, bool recalculateCollider = true, bool forceRescale = false)
		{
			return this.ScaleTo(scale, recalculateCollider, forceRescale);
		}

		// Token: 0x060002FB RID: 763 RVA: 0x0001A2E0 File Offset: 0x000186E0
		public Vector3 LimitScale(ref Vector3 s)
		{
			BlockMetaData blockMetaData = this.GetBlockMetaData();
			Vector3 zero = Vector3.zero;
			if (blockMetaData != null)
			{
				Vector3 scaleLimit = blockMetaData.scaleLimit;
				for (int i = 0; i < 3; i++)
				{
					if (scaleLimit[i] > 0.99f)
					{
						zero[i] = s[i] - scaleLimit[i];
						if (zero[i] > 0f)
						{
							s[i] = scaleLimit[i];
						}
					}
				}
			}
			return zero;
		}

		// Token: 0x060002FC RID: 764 RVA: 0x0001A36C File Offset: 0x0001876C
		public virtual bool ScaleTo(Vector3 scale, bool recalculateCollider = true, bool forceRescale = false)
		{
			scale.x = Mathf.Abs(scale.x);
			scale.y = Mathf.Abs(scale.y);
			scale.z = Mathf.Abs(scale.z);
			this.LimitScale(ref scale);
			bool flag = scale != this.meshScale;
			this.isScaled = (scale != Vector3.one);
			string text = this.BlockType();
			this.blockMass = -1f;
			if (scale != this.colliderScale)
			{
				if (!Blocksworld.colliders.ContainsKey(text))
				{
					BWLog.Info("Could not find collider for " + text);
				}
				if (this.CanScaleMesh(0))
				{
					Collider component = this.go.GetComponent<Collider>();
					if (component != null)
					{
						BoxCollider boxCollider = Blocksworld.colliders[text] as BoxCollider;
						if (boxCollider != null)
						{
							(component as BoxCollider).center = new Vector3(boxCollider.center.x * scale.x, boxCollider.center.y * scale.y, boxCollider.center.z * scale.z);
							(component as BoxCollider).size = new Vector3(boxCollider.size.x * scale.x, boxCollider.size.y * scale.y, boxCollider.size.z * scale.z);
							(component as BoxCollider).size -= Vector3.one * 0.01125f;
						}
						SphereCollider sphereCollider = Blocksworld.colliders[text] as SphereCollider;
						if (sphereCollider != null)
						{
							(component as SphereCollider).center = new Vector3(sphereCollider.center.x * scale.x, sphereCollider.center.y * scale.y, sphereCollider.center.z * scale.z);
							(component as SphereCollider).radius = sphereCollider.radius * scale.x;
						}
						MeshCollider meshCollider = Blocksworld.colliders[text] as MeshCollider;
						if (meshCollider != null)
						{
							MeshCollider meshCollider2 = component as MeshCollider;
							if (meshCollider2 != null)
							{
								if (meshCollider.sharedMesh == null)
								{
									BWLog.Info("Block " + this.go.name + " prefab is missing a mesh on its mesh collider");
								}
								else
								{
									meshCollider2.sharedMesh = null;
									meshCollider2.sharedMesh = this.UseColliderMesh(meshCollider.sharedMesh, scale);
								}
							}
						}
						CapsuleCollider x = Blocksworld.colliders[text] as CapsuleCollider;
						if (x != null)
						{
							CapsuleCollider capsuleCollider = component as CapsuleCollider;
							capsuleCollider.height = scale.x;
							capsuleCollider.radius = 0.5f * scale.z;
						}
					}
					else
					{
						BWLog.Info("Could not find a collider on " + this.go.name + " when scaling mesh");
					}
				}
				this.colliderScale = scale;
			}
			if (flag || forceRescale)
			{
				string scaleType = string.Empty;
				BlockMetaData blockMetaData = this.GetBlockMetaData();
				if (blockMetaData != null)
				{
					scaleType = blockMetaData.scaleType;
				}
				if (this.CanScaleMesh(0))
				{
					this.ScaleMesh(scale, this.GetTexture(0), text, this.mesh, scaleType);
				}
				if (this.childMeshes != null)
				{
					int num = 1;
					foreach (KeyValuePair<string, Mesh> keyValuePair in this.childMeshes)
					{
						if (this.CanScaleMesh(num))
						{
							this.ScaleMesh(scale, this.GetTexture(num), keyValuePair.Key, keyValuePair.Value, scaleType);
						}
						num++;
					}
				}
				Vector3 a = Vector3.zero;
				if (this.tiles[0].Count > 6)
				{
					object[] args = this.tiles[0][6].gaf.Args;
					a = (Vector3)args[0];
					args[0] = scale;
				}
				else
				{
					this.tiles[0].Add(new Tile(new GAF("Block.ScaleTo", new object[]
					{
						scale
					})));
				}
				if ((a - scale).sqrMagnitude > 0.0001f)
				{
					this.frozenInTerrainStatus = -1;
				}
			}
			if (flag)
			{
				this.UpdateSATVolumes();
			}
			if (!Blocksworld.renderingShadows)
			{
				this.UpdateShadowMaxDistance(this.GetPaint(0));
			}
			this.meshScale = scale;
			this.size = this.BlockSize();
			this.size.Scale(scale);
			this.CalculateMaxExtent();
			this.shadowSize = this.size;
			return true;
		}

		// Token: 0x060002FD RID: 765 RVA: 0x0001A894 File Offset: 0x00018C94
		public float CalculateMaxExtent()
		{
			return Mathf.Max(new float[]
			{
				this.size.x,
				this.size.y,
				this.size.z
			}) * 1.73205078f;
		}

		// Token: 0x060002FE RID: 766 RVA: 0x0001A8D4 File Offset: 0x00018CD4
		private void ScaleMeshUsingColors(Vector3[] vertices, Vector3[] verticesPrefab, Vector3 scale, Color[] colors)
		{
			for (int i = 0; i < vertices.Length; i++)
			{
				float x = (colors[i].r != 0f) ? (verticesPrefab[i].x * scale.x) : (verticesPrefab[i].x + Mathf.Sign(verticesPrefab[i].x) * 0.5f * (scale.x - 1f));
				float y = (colors[i].g != 0f) ? (verticesPrefab[i].y * scale.y) : (verticesPrefab[i].y + Mathf.Sign(verticesPrefab[i].y) * 0.5f * (scale.y - 1f));
				float z = (colors[i].b != 0f) ? (verticesPrefab[i].z * scale.z) : (verticesPrefab[i].z + Mathf.Sign(verticesPrefab[i].z) * 0.5f * (scale.z - 1f));
				vertices[i] = new Vector3(x, y, z);
			}
		}

		// Token: 0x060002FF RID: 767 RVA: 0x0001AA34 File Offset: 0x00018E34
		private void ScaleMeshMotor(Vector3[] vertices, Vector3[] verticesPrefab, Vector3 scale)
		{
			for (int i = 0; i < vertices.Length; i++)
			{
				vertices[i] = new Vector3(verticesPrefab[i].x + Mathf.Sign(verticesPrefab[i].x) * 0.5f * (scale.x - 1f), verticesPrefab[i].y + Mathf.Sign(verticesPrefab[i].y) * 0.5f * (scale.y - 1f), verticesPrefab[i].z + Mathf.Sign(verticesPrefab[i].z) * 0.5f * (scale.z - 1f));
			}
		}

		// Token: 0x06000300 RID: 768 RVA: 0x0001AAFC File Offset: 0x00018EFC
		private void ScaleMeshDefault(Vector3[] vertices, Vector3[] verticesPrefab, Vector3 scale)
		{
			for (int i = 0; i < vertices.Length; i++)
			{
				vertices[i] = new Vector3(verticesPrefab[i].x + Mathf.Sign(verticesPrefab[i].x) * 0.5f * (scale.x - 1f), verticesPrefab[i].y + Mathf.Sign(verticesPrefab[i].y) * 0.5f * (scale.y - 1f), verticesPrefab[i].z + Mathf.Sign(verticesPrefab[i].z) * 0.5f * (scale.z - 1f));
			}
		}

		// Token: 0x06000301 RID: 769 RVA: 0x0001ABC4 File Offset: 0x00018FC4
		private void ScaleMeshDefault(Vector3[] vertices, Vector3[] verticesPrefab, Vector3 scale, Vector2[] uvs, Mapping mapping, Vector3 uvWrapScale, bool textureFourSidesIgnoreRightLeft)
		{
			this.ScaleMeshDefault(vertices, verticesPrefab, scale);
			if (uvs != null && vertices.Length == uvs.Length)
			{
				int num = 0;
				if (mapping == Mapping.TwoSidesWrapTo1x1)
				{
					switch (Materials.FindSide(this.GetTextureNormal()))
					{
					case Side.Front:
					case Side.Back:
						num = 1;
						break;
					case Side.Right:
					case Side.Left:
						num = 3;
						break;
					case Side.Top:
					case Side.Bottom:
						num = 2;
						break;
					}
				}
				for (int i = 0; i < vertices.Length; i++)
				{
					if (uvs[i].x < 0.333333343f)
					{
						uvs[i].x = Mathf.Repeat(uvs[i].x * 6f, 1f);
						uvs[i].x = uvs[i].x + Mathf.Sign(uvs[i].x - 0.5f) * 0.5f * (uvWrapScale.x - 1f) + ((Mathf.Round(uvWrapScale.x) % 2f != 0f) ? 0f : 0.5f);
						uvs[i].y = uvs[i].y + Mathf.Sign(uvs[i].y - 0.5f) * 0.5f * (uvWrapScale.y - 1f) + ((Mathf.Round(uvWrapScale.y) % 2f != 0f) ? 0f : 0.5f);
						if (num != 0 && num != 1)
						{
							uvs[i] = Vector2.zero;
						}
					}
					else if (uvs[i].x >= 0.6666667f)
					{
						if (mapping == Mapping.FourSidesTo1x1 && !textureFourSidesIgnoreRightLeft)
						{
							uvs[i].x = 0f;
							uvs[i].y = 0f;
						}
						else
						{
							uvs[i].x = Mathf.Repeat(uvs[i].x * 6f, 1f);
							uvs[i].x = uvs[i].x + Mathf.Sign(uvs[i].x - 0.5f) * 0.5f * (uvWrapScale.x - 1f) + ((Mathf.Round(uvWrapScale.x) % 2f != 0f) ? 0f : 0.5f);
							uvs[i].y = uvs[i].y + Mathf.Sign(uvs[i].y - 0.5f) * 0.5f * (uvWrapScale.z - 1f) + ((Mathf.Round(uvWrapScale.z) % 2f != 0f) ? 0f : 0.5f);
						}
						if (num != 0 && num != 2)
						{
							uvs[i] = Vector2.zero;
						}
					}
					else if (mapping == Mapping.FourSidesTo1x1 && textureFourSidesIgnoreRightLeft)
					{
						uvs[i].x = 0f;
						uvs[i].y = 0f;
					}
					else
					{
						uvs[i].x = Mathf.Repeat(uvs[i].x * 6f, 1f);
						uvs[i].x = uvs[i].x + Mathf.Sign(uvs[i].x - 0.5f) * 0.5f * (uvWrapScale.z - 1f) + ((Mathf.Round(uvWrapScale.z) % 2f != 0f) ? 0f : 0.5f);
						uvs[i].y = uvs[i].y + Mathf.Sign(uvs[i].y - 0.5f) * 0.5f * (uvWrapScale.y - 1f) + ((Mathf.Round(uvWrapScale.y) % 2f != 0f) ? 0f : 0.5f);
						if (num != 0 && num != 3)
						{
							uvs[i] = Vector2.zero;
						}
					}
				}
			}
		}

		// Token: 0x06000302 RID: 770 RVA: 0x0001B084 File Offset: 0x00019484
		private void ScaleMeshUniform(Vector3[] vertices, Vector3[] verticesPrefab, Vector3 scale)
		{
			for (int i = 0; i < vertices.Length; i++)
			{
				vertices[i] = new Vector3(verticesPrefab[i].x * scale.x, verticesPrefab[i].y * scale.y, verticesPrefab[i].z * scale.z);
			}
		}

		// Token: 0x06000303 RID: 771 RVA: 0x0001B0F2 File Offset: 0x000194F2
		private void ScaleMeshUniform(Vector3[] vertices, Vector3[] verticesPrefab, Vector3 scale, Vector2[] uvs, Mapping mapping, Vector3 uvWrapScale, bool textureFourSidesIgnoreRightLeft)
		{
			this.ScaleMeshUniform(vertices, verticesPrefab, scale);
			if (uvs != null)
			{
				this.ScaleUVsUniform(uvs, mapping, uvWrapScale, textureFourSidesIgnoreRightLeft);
			}
		}

		// Token: 0x06000304 RID: 772 RVA: 0x0001B114 File Offset: 0x00019514
		public virtual void ScaleMesh(Vector3 scale, string texture, string type, Mesh mesh, string scaleType = "")
		{
			if (this is BlockAnimatedCharacter)
			{
				return;
			}
			if (this is BlockCharacter)
			{
				return;
			}
			Mesh mesh2;
			if (!Blocksworld.meshes.TryGetValue(type, out mesh2))
			{
				BWLog.Info("Count not find mesh for type " + type);
				return;
			}
			Vector3[] vertices = mesh2.vertices;
			Vector3[] vertices2 = mesh.vertices;
			if (vertices2.Length != vertices.Length)
			{
				BWLog.Info(string.Concat(new object[]
				{
					"vertices.Length != verticesPrefab.Length in ScaleMesh() ",
					this.BlockType(),
					" ",
					vertices2.Length,
					" ",
					vertices.Length,
					" for ",
					mesh.name,
					" vs. ",
					mesh2.name
				}));
			}
			bool flag = this is BlockAbstractWheel || this is BlockRaycastWheel;
			Vector2[] array = null;
			Mapping mapping = Materials.GetMapping(texture);
			Vector3 vector = scale;
			if (!this.isTerrain && !this.fakeTerrain && (mapping == Mapping.AllSidesTo1x1 || mapping == Mapping.FourSidesTo1x1 || mapping == Mapping.TwoSidesWrapTo1x1) && !flag && Materials.uvWraps.ContainsKey(type))
			{
				array = Materials.CopyUVs(Materials.uvWraps[type]);
				vector = Block.GetWrapScale(texture, scale);
			}
			Vector3 vector2 = new Vector3(vector.x / scale.x, vector.y / scale.y, vector.z / scale.z);
			bool flag2 = Materials.FourSidesIgnoreRightLeft(texture);
			float num = Mathf.Min(scale.y, scale.z);
			switch (type)
			{
			case "Tree Poplar":
			case "Tree Spruce Stem":
			case "Tree Linden Stem":
			case "Tree Poplar Stem":
			case "Water":
			case "Ice Water":
			case "Water Endless Expanse":
			case "Desert Water":
			case "Crenelation":
				this.ScaleMeshUniform(vertices2, vertices, scale, array, mapping, vector, flag2);
				goto IL_1119;
			case "Slice":
			case "Slice Inverse":
				this.ScaleMeshUsingColors(vertices2, vertices, scale, mesh2.colors);
				if (array != null)
				{
					this.ScaleUVsUniform(array, mapping, vector, flag2);
				}
				goto IL_1119;
			case "octagonalCone":
			case "octagonalCylinder":
			case "smoothCone":
			case "Pyramid":
			case "Cylinder":
			case "Wheel":
			case "Raycast Wheel":
			case "Bulky Wheel":
			case "Spoked Wheel":
			case "Golden Wheel":
			case "Golden Wheel Rim":
			case "Golden Wheel Side":
			case "Sphere":
			case "Hemisphere":
			case "Rocket":
			case "Bulky Wheel Outer":
			case "Bulky Wheel Inner":
			case "RAR Moon Rover Wheel":
			case "RAR Moon Rover Wheel Rim":
			{
				bool flag3 = type == "Sphere";
				bool flag4 = type == "Hemisphere";
				bool flag5 = type == "Wheel" || type == "Raycast Wheel";
				bool flag6 = type == "Bulky Wheel";
				bool flag7 = type == "Cylinder";
				bool flag8 = type == "Rocket";
				bool flag9 = flag || type == "Bulky Wheel Outer" || type == "Bulky Wheel Inner";
				float num3 = (!flag9) ? Mathf.Min(scale.x, scale.z) : Mathf.Min(scale.y, scale.z);
				float num4 = num3;
				if (flag9)
				{
					num4 = Mathf.Clamp(num3, 1f, scale.x);
				}
				float num5 = num3;
				if (flag8 || type == "smoothCone" || type == "octagonalCone" || type == "Pyramid")
				{
					num5 = Mathf.Clamp(num3, 1f, scale.y);
				}
				if (flag7 || type == "octagonalCylinder")
				{
					num5 = Mathf.Clamp(num3, 1f, 5f * scale.y);
				}
				if (flag4)
				{
					num4 = scale.x * 0.5f;
					num5 = scale.y * 0.5f;
					num3 = scale.z * 0.5f;
				}
				for (int i = 0; i < vertices2.Length; i++)
				{
					if (flag3 || flag4)
					{
						vertices2[i] = new Vector3(vertices[i].x * scale.x, vertices[i].y * scale.y, vertices[i].z * scale.z);
					}
					else
					{
						float x = vertices[i].x * num4 + Mathf.Sign(vertices[i].x) * 0.5f * (scale.x - num4);
						float y = vertices[i].y * num5 + Mathf.Sign(vertices[i].y) * 0.5f * (scale.y - num5);
						float z = vertices[i].z * num3 + Mathf.Sign(vertices[i].z) * 0.5f * (scale.z - num3);
						if (flag7 && (i == 96 || i == 165))
						{
							z = (x = 0f);
						}
						if (flag8 && (i == 225 || i == 246))
						{
							z = (x = 0f);
						}
						vertices2[i] = new Vector3(x, y, z);
					}
					if (array != null)
					{
						if (array[i].x < 0.333333343f)
						{
							array[i].x = Mathf.Repeat(array[i].x * 6f, 1f);
							if (flag4)
							{
								float num6 = Mathf.Max(Mathf.Round(0.7f * Mathf.Max(scale.x, scale.z) / scale.y), 1f);
								array[i].x = array[i].x * scale.x * vector2.x;
								array[i].y = array[i].y * scale.y * 0.5f * num6 * vector2.y;
							}
							else
							{
								array[i].x = vector2.x * (array[i].x * num4 + Mathf.Sign(array[i].x - 0.5f) * 0.5f * (scale.x - num4) + ((Mathf.Round(scale.x) % 2f != 0f) ? 0f : 0.5f));
								array[i].y = array[i].y + Mathf.Sign(array[i].y - 0.5f) * 0.5f * (vector.y - 1f) + ((Mathf.Round(vector.y) % 2f != 0f) ? 0f : 0.5f);
							}
						}
						else if (array[i].x >= 0.6666667f)
						{
							if (mapping == Mapping.FourSidesTo1x1 && !flag2)
							{
								array[i].x = 0f;
								array[i].y = 0f;
							}
							else
							{
								array[i].x = Mathf.Repeat(array[i].x * 6f, 1f);
								if (flag4)
								{
									array[i].x = array[i].x * scale.x * vector2.x;
									array[i].y = array[i].y * scale.z * vector2.z;
								}
								else
								{
									array[i].x = vector2.x * (array[i].x * num4 + Mathf.Sign(array[i].x - 0.5f) * 0.5f * (scale.x - num4) + ((Mathf.Round(scale.x) % 2f != 0f) ? 0f : 0.5f));
									array[i].y = vector2.z * (array[i].y * num3 + Mathf.Sign(array[i].y - 0.5f) * 0.5f * (scale.z - num3) + ((Mathf.Round(scale.z) % 2f != 0f) ? 0f : 0.5f));
								}
								if ((flag7 && (i == 60 || i == 81)) || (flag8 && (i == 120 || i == 141)))
								{
									array[i].x = vector2.x * (0.5f * num4 + ((Mathf.Round(scale.x) % 2f != 0f) ? 0f : 0.5f));
									array[i].y = vector2.z * (0.5f * num3 + ((Mathf.Round(scale.z) % 2f != 0f) ? 0f : 0.5f));
								}
							}
						}
						else if (mapping == Mapping.FourSidesTo1x1 && flag2)
						{
							array[i].x = 0f;
							array[i].y = 0f;
						}
						else
						{
							array[i].x = Mathf.Repeat(array[i].x * 6f, 1f);
							if (flag4)
							{
								float num7 = Mathf.Max(Mathf.Round(0.7f * Mathf.Max(scale.x, scale.z) / scale.y), 1f);
								array[i].x = array[i].x * scale.z * vector2.z;
								array[i].y = array[i].y * scale.y * 0.5f * num7 * vector2.y;
							}
							else
							{
								array[i].x = vector2.z * (array[i].x * num3 + Mathf.Sign(array[i].x - 0.5f) * 0.5f * (scale.z - num3) + ((Mathf.Round(scale.z) % 2f != 0f) ? 0f : 0.5f));
								array[i].y = array[i].y + Mathf.Sign(array[i].y - 0.5f) * 0.5f * (vector.y - 1f) + ((Mathf.Round(vector.y) % 2f != 0f) ? 0f : 0.5f);
							}
							if ((flag5 || flag6) && (i == 100 || i == 161))
							{
								array[i].x = vector2.z * 0.5f * num3 + ((Mathf.Round(scale.z) % 2f != 0f) ? 0f : 0.5f);
								array[i].y = vector2.y * 0.5f * num5 + ((Mathf.Round(scale.y) % 2f != 0f) ? 0f : 0.5f);
							}
						}
					}
				}
				goto IL_1119;
			}
			case "Raycast Wheel Axle":
			case "Wheel Axle":
			case "Spoked Wheel Axle":
			case "Golden Wheel Axle":
			case "6 Spoke Wheel X N":
			case "6 Spoke Wheel X P":
			case "Semi1 Wheel X N":
			case "Semi1 Wheel X P":
			case "Semi2 Wheel X N":
			case "Semi2 Wheel X P":
			case "Pinwheel Wheel X N":
			case "Pinwheel Wheel X P":
			case "Monster1 Wheel X N":
			case "Monster1 Wheel X P":
			case "Monster2 Wheel X N":
			case "Monster2 Wheel X P":
			case "Monster3 Wheel X N":
			case "Monster3 Wheel X P":
				for (int j = 0; j < vertices2.Length; j++)
				{
					vertices2[j] = new Vector3(vertices[j].x * scale.x, vertices[j].y * num, vertices[j].z * num);
				}
				goto IL_1119;
			case "Treads X P":
			case "Treads X N":
			case "Bulky Wheel X N":
			case "Bulky Wheel X P":
				for (int k = 0; k < vertices2.Length; k++)
				{
					vertices2[k] = new Vector3(vertices[k].x * num, vertices[k].y * scale.x, vertices[k].z * num);
				}
				goto IL_1119;
			case "Motor Cube Axle":
				for (int l = 0; l < vertices2.Length; l++)
				{
					vertices2[l] = new Vector3(vertices[l].x, vertices[l].y * scale.y, vertices[l].z);
				}
				goto IL_1119;
			}
			if (scaleType != null)
			{
				if (scaleType == "Uniform")
				{
					this.ScaleMeshUniform(vertices2, vertices, scale, array, mapping, vector, flag2);
					goto IL_1114;
				}
				if (scaleType == "UniformUV")
				{
					this.fakeTerrain = true;
					this.ScaleMeshUniform(vertices2, vertices, scale, null, mapping, vector, flag2);
					goto IL_1114;
				}
				if (scaleType == "Colors")
				{
					this.ScaleMeshUsingColors(vertices2, vertices, scale, mesh2.colors);
					if (array != null)
					{
						this.ScaleUVsUniform(array, mapping, vector, flag2);
					}
					goto IL_1114;
				}
			}
			this.ScaleMeshDefault(vertices2, vertices, scale, array, mapping, vector, flag2);
			IL_1114:
			IL_1119:
			mesh.vertices = vertices2;
			if (array != null)
			{
				mesh.uv = array;
			}
			mesh.RecalculateBounds();
		}

		// Token: 0x06000305 RID: 773 RVA: 0x0001C25C File Offset: 0x0001A65C
		protected void ResizeMeshForBlock(Mesh mesh)
		{
			Vector3[] vertices = mesh.vertices;
			Vector3[] vertices2 = new Vector3[mesh.vertices.Length];
			string a = string.Empty;
			BlockMetaData blockMetaData = this.GetBlockMetaData();
			if (blockMetaData != null)
			{
				a = blockMetaData.scaleType;
			}
			if (a == "Uniform")
			{
				this.ScaleMeshUniform(vertices2, vertices, this.Scale());
			}
			else
			{
				this.ScaleMeshDefault(vertices2, vertices, this.Scale());
			}
			mesh.vertices = vertices2;
			mesh.RecalculateBounds();
		}

		// Token: 0x06000306 RID: 774 RVA: 0x0001C2DC File Offset: 0x0001A6DC
		private static float GetWrapScaleComponent(float s, float p)
		{
			float result = 1f;
			if (s > 1.01f)
			{
				if (s <= p)
				{
					result = 1f;
				}
				else
				{
					result = (float)Mathf.RoundToInt(s / p);
				}
			}
			return result;
		}

		// Token: 0x06000307 RID: 775 RVA: 0x0001C318 File Offset: 0x0001A718
		public static Vector3 GetWrapScale(string texture, Vector3 scale)
		{
			Vector3 result = scale;
			Vector3 vector;
			if (Materials.wrapTexturePrefSizes.TryGetValue(texture, out vector))
			{
				result.x = Block.GetWrapScaleComponent(scale.x, vector.x);
				result.y = Block.GetWrapScaleComponent(scale.y, vector.y);
				result.z = Block.GetWrapScaleComponent(scale.z, vector.z);
			}
			return result;
		}

		// Token: 0x06000308 RID: 776 RVA: 0x0001C388 File Offset: 0x0001A788
		private void ScaleUVsUniform(Vector2[] uvs, Mapping mapping, Vector3 scale, bool textureFourSidesIgnoreRightLeft)
		{
			for (int i = 0; i < uvs.Length; i++)
			{
				if (uvs[i].x < 0.333333343f)
				{
					uvs[i].x = Mathf.Repeat(uvs[i].x * 6f, 1f);
					uvs[i].x = uvs[i].x * scale.x;
					uvs[i].y = uvs[i].y * scale.y;
				}
				else if (uvs[i].x >= 0.6666667f)
				{
					if (mapping == Mapping.FourSidesTo1x1 && !textureFourSidesIgnoreRightLeft)
					{
						uvs[i].x = 0f;
						uvs[i].y = 0f;
					}
					else
					{
						uvs[i].x = Mathf.Repeat(uvs[i].x * 6f, 1f);
						uvs[i].x = uvs[i].x * scale.x;
						uvs[i].y = uvs[i].y * scale.z;
					}
				}
				else if (mapping == Mapping.FourSidesTo1x1 && textureFourSidesIgnoreRightLeft)
				{
					uvs[i].x = 0f;
					uvs[i].y = 0f;
				}
				else
				{
					uvs[i].x = Mathf.Repeat(uvs[i].x * 6f, 1f);
					uvs[i].x = uvs[i].x * scale.z;
					uvs[i].y = uvs[i].y * scale.y;
				}
			}
		}

		// Token: 0x06000309 RID: 777 RVA: 0x0001C584 File Offset: 0x0001A984
		private Mesh UseColliderMesh(Mesh meshPrefab, Vector3 scale)
		{
			Vector3[] vertices = meshPrefab.vertices;
			int[] triangles = meshPrefab.triangles;
			Mesh mesh = new Mesh();
			string text = this.BlockType();
			bool flag;
			if (text != null)
			{
				if (Block.f__switch_map3 == null)
				{
					Block.f__switch_map3 = new Dictionary<string, int>(12)
					{
						{
							"Cylinder",
							0
						},
						{
							"octagonalCylinder",
							0
						},
						{
							"Rocket",
							0
						},
						{
							"smoothCone",
							0
						},
						{
							"octagonalCone",
							0
						},
						{
							"Wheel",
							0
						},
						{
							"Raycast Wheel",
							0
						},
						{
							"Bulky Wheel",
							0
						},
						{
							"Golden Wheel",
							0
						},
						{
							"Pyramid",
							0
						},
						{
							"Spoked Wheel",
							0
						},
						{
							"RAR Moon Rover Wheel",
							0
						}
					};
				}
				int num;
				if (Block.f__switch_map3.TryGetValue(text, out num))
				{
					if (num == 0)
					{
						flag = true;
						goto IL_108;
					}
				}
			}
			flag = false;
			IL_108:
			bool flag2 = this is BlockAbstractWheel;
			if (flag || flag2)
			{
				float num2 = (!flag2) ? Mathf.Min(scale.x, scale.z) : Mathf.Min(scale.y, scale.z);
				float num3 = num2;
				if (flag2)
				{
					num3 = Mathf.Clamp(num2, 1f, scale.x);
				}
				float num4 = num2;
				if (text == "Rocket")
				{
					num4 = Mathf.Clamp(num2, 1f, scale.y);
				}
				for (int i = 0; i < vertices.Length; i++)
				{
					float x = vertices[i].x * num3 + Mathf.Sign(vertices[i].x) * 0.5f * (scale.x - num3);
					float y = vertices[i].y * num4 + Mathf.Sign(vertices[i].y) * 0.5f * (scale.y - num4);
					float z = vertices[i].z * num2 + Mathf.Sign(vertices[i].z) * 0.5f * (scale.z - num2);
					vertices[i] = new Vector3(x, y, z);
				}
			}
			else
			{
				for (int j = 0; j < vertices.Length; j++)
				{
					vertices[j] = new Vector3(vertices[j].x * scale.x, vertices[j].y * scale.y, vertices[j].z * scale.z);
				}
			}
			mesh.vertices = vertices;
			mesh.triangles = triangles;
			return mesh;
		}

		// Token: 0x0600030A RID: 778 RVA: 0x0001C880 File Offset: 0x0001AC80
		public virtual string GetPaint(int meshIndex = 0)
		{
			if (meshIndex > 0)
			{
				return this.GetSubMeshPaint(meshIndex);
			}
			return (!string.IsNullOrEmpty(this.currentPaint)) ? this.currentPaint : ((string)this.tiles[0][4].gaf.Args[0]);
		}

		// Token: 0x0600030B RID: 779 RVA: 0x0001C8DC File Offset: 0x0001ACDC
		public string GetTexture(int meshIndex = 0)
		{
			if (this.BlockType().Contains("Volume Block") && Blocksworld.CurrentState != State.Play)
			{
				return "Volume";
			}
			if (meshIndex > 0)
			{
				return this.GetSubMeshTexture(meshIndex);
			}
			if (this.renderer != null)
			{
				Material sharedMaterial = this.renderer.sharedMaterial;
				string result;
				if (sharedMaterial != null && Materials.materialCacheTexture.TryGetValue(sharedMaterial, out result))
				{
					return result;
				}
			}
			return (string)this.tiles[0][5].gaf.Args[0];
		}

		// Token: 0x0600030C RID: 780 RVA: 0x0001C97E File Offset: 0x0001AD7E
		public string GetBuildModeTexture(int meshIndex)
		{
			if (meshIndex == 0)
			{
				return (string)this.tiles[0][5].gaf.Args[0];
			}
			return this.subMeshTextures[meshIndex - 1];
		}

		// Token: 0x0600030D RID: 781 RVA: 0x0001C9B8 File Offset: 0x0001ADB8
		public string GetBuildModePaint(int meshIndex)
		{
			if (meshIndex == 0)
			{
				return (string)this.tiles[0][4].gaf.Args[0];
			}
			return this.subMeshPaints[meshIndex - 1];
		}

		// Token: 0x0600030E RID: 782 RVA: 0x0001C9F2 File Offset: 0x0001ADF2
		public Vector3 GetTextureNormal()
		{
			return (Vector3)this.tiles[0][5].gaf.Args[1];
		}

		// Token: 0x0600030F RID: 783 RVA: 0x0001CA17 File Offset: 0x0001AE17
		public void SetTextureNormal(Vector3 angles)
		{
			this.tiles[0][5].gaf.Args[1] = angles;
		}

		// Token: 0x06000310 RID: 784 RVA: 0x0001CA3D File Offset: 0x0001AE3D
		public virtual TileResultCode IsPaintedTo(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!(this.GetPaint(0) == (string)args[0])) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000311 RID: 785 RVA: 0x0001CA60 File Offset: 0x0001AE60
		public virtual TileResultCode PaintToAction(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 1, 0);
			return this.PaintTo((string)args[0], false, intArg);
		}

		// Token: 0x06000312 RID: 786 RVA: 0x0001CA88 File Offset: 0x0001AE88
		public TileResultCode IsSkyPaintedTo(ScriptRowExecutionInfo eInfo, object[] args)
		{
			BlockSky worldSky = Blocksworld.worldSky;
			if (worldSky != null)
			{
				return (!(worldSky.GetPaint(0) == (string)args[0])) ? TileResultCode.False : TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x06000313 RID: 787 RVA: 0x0001CAC4 File Offset: 0x0001AEC4
		public TileResultCode PaintSkyToAction(ScriptRowExecutionInfo eInfo, object[] args)
		{
			BlockSky worldSky = Blocksworld.worldSky;
			if (worldSky == null)
			{
				return TileResultCode.True;
			}
			string stringArg = Util.GetStringArg(args, 0, "Blue");
			int intArg = Util.GetIntArg(args, 1, 0);
			float floatArg = Util.GetFloatArg(args, 2, 0f);
			if (floatArg > 0f)
			{
				return worldSky.TransitionPaintTo(stringArg, intArg, floatArg, eInfo.timer);
			}
			return worldSky.PaintTo(stringArg, false, intArg);
		}

		// Token: 0x06000314 RID: 788 RVA: 0x0001CB28 File Offset: 0x0001AF28
		private void UpdateShadowMaxDistance(string paint)
		{
			Vector3 v = this.shadowSize;
			bool flag = Blocksworld.IsLuminousPaint(paint);
			BlockMetaData blockMetaData = this.GetBlockMetaData();
			if (flag)
			{
				this.shadowMaxDistance = 1f + Mathf.Abs(Util.MinComponent(v));
				if (blockMetaData != null)
				{
					this.shadowStrengthMultiplier = blockMetaData.lightStrengthMultiplier;
				}
			}
			else
			{
				this.shadowMaxDistance = 4f + Mathf.Abs(Util.MinComponent(v));
				if (blockMetaData != null)
				{
					this.shadowStrengthMultiplier = blockMetaData.shadowStrengthMultiplier;
				}
			}
		}

		// Token: 0x06000315 RID: 789 RVA: 0x0001CBB4 File Offset: 0x0001AFB4
		protected void UpdateShadowColors(string paint, string texture = null, string oldTexture = null)
		{
			if (this.meshShadow != null && this.go != null && this.go.GetComponent<Renderer>() != null && this.go.GetComponent<Renderer>().sharedMaterial != null)
			{
				Color a = Color.black;
				if (Blocksworld.IsLuminousPaint(paint))
				{
					a = this.go.GetComponent<Renderer>().sharedMaterial.GetColor("_Emission");
				}
				if (texture != null && Blocksworld.IsLuminousTexture(texture))
				{
					a += this.go.GetComponent<Renderer>().sharedMaterial.GetColor("_Color");
				}
				for (int i = 0; i < this.colorsShadow.Length; i++)
				{
					this.colorsShadow[i].r = a.r;
					this.colorsShadow[i].g = a.g;
					this.colorsShadow[i].b = a.b;
				}
				this.meshShadow.colors = this.colorsShadow;
				this.oldShadowAlpha = -1f;
				this.UpdateShadowMaxDistance(paint);
				bool flag = !this.vanished && (this.VisibleInPlayMode() || Blocksworld.CurrentState != State.Play);
				if (this.goShadow != null)
				{
					this.goShadow.GetComponent<Renderer>().enabled = (flag && texture != null);
				}
			}
		}

		// Token: 0x06000316 RID: 790 RVA: 0x0001CD50 File Offset: 0x0001B150
		protected virtual void SetSubMeshVisibility(bool t)
		{
			for (int i = 0; i < this.subMeshGameObjects.Count; i++)
			{
				string texture = this.GetTexture(i + 1);
				if (texture == "Invisible")
				{
					this.subMeshGameObjects[i].GetComponent<Renderer>().enabled = !t;
				}
			}
		}

		// Token: 0x06000317 RID: 791 RVA: 0x0001CDB0 File Offset: 0x0001B1B0
		internal void MakeInvisibleVisible()
		{
			for (int i = 0; i < ((this.childMeshes != null) ? (this.childMeshes.Count + 1) : 1); i++)
			{
				string texture = this.GetTexture(i);
				if (texture == "Invisible")
				{
					Vector3 normal = (i != 0) ? this.GetSubMeshTextureNormal(i) : this.GetTextureNormal();
					this.TextureTo("Glass", normal, true, i, true);
				}
			}
		}

		// Token: 0x06000318 RID: 792 RVA: 0x0001CE2D File Offset: 0x0001B22D
		protected virtual void RegisterPaintChanged(int meshIndex, string paint, string oldPaint)
		{
			TextureAndPaintBlockRegistry.BlockPaintChanged(this, paint, oldPaint);
		}

		// Token: 0x06000319 RID: 793 RVA: 0x0001CE37 File Offset: 0x0001B237
		protected virtual void RegisterTextureChanged(int meshIndex, string texture, string oldTexture)
		{
			TextureAndPaintBlockRegistry.BlockTextureChanged(this, texture, oldTexture);
		}

		// Token: 0x0600031A RID: 794 RVA: 0x0001CE44 File Offset: 0x0001B244
		public TileResultCode PaintToAllMeshes(string paint, bool permanent)
		{
			TileResultCode result = this.PaintTo(paint, permanent, 0);
			for (int i = 0; i < this.subMeshPaints.Count; i++)
			{
				this.PaintToSubMesh(paint, permanent, i + 1);
			}
			return result;
		}

		// Token: 0x0600031B RID: 795 RVA: 0x0001CE84 File Offset: 0x0001B284
		public virtual TileResultCode PaintTo(string paint, bool permanent, int meshIndex = 0)
		{
			if (meshIndex > 0)
			{
				this.PaintToSubMesh(paint, permanent, meshIndex);
				return TileResultCode.True;
			}
			string paint2 = this.GetPaint(meshIndex);
			this.currentPaint = paint;
			if (!string.IsNullOrEmpty(this.meshScaleTexture) && paint2 == paint)
			{
				return TileResultCode.True;
			}
			if (Blocksworld.CurrentState == State.Play)
			{
				this.RegisterPaintChanged(0, paint, paint2);
			}
			string texture = this.GetTexture(0);
			Vector3 textureNormal = this.GetTextureNormal();
			Vector3 scale = this.Scale();
			if (this.mesh != null)
			{
				Materials.SetMaterial(this.go, this.mesh, this.BlockType(), paint, texture, textureNormal, scale, this.meshScaleTexture);
			}
			this.skipUpdateSATVolumes = true;
			this.meshScaleTexture = texture;
			this.skipUpdateSATVolumes = false;
			if (permanent)
			{
				if (paint != null)
				{
					Tile tile = this.tiles[0][4];
					if (tile.IsShowing())
					{
						tile.Show(false);
						tile.gaf.Args[0] = paint;
						tile.Show(true);
					}
					else
					{
						tile.gaf.Args[0] = paint;
					}
				}
				else
				{
					BWLog.Warning("PaintTo() trying to set paint to null");
				}
			}
			if (!Blocksworld.renderingShadows)
			{
				this.UpdateShadowColors(paint, texture, (Blocksworld.CurrentState != State.Build) ? texture : string.Empty);
			}
			return TileResultCode.True;
		}

		// Token: 0x0600031C RID: 796 RVA: 0x0001CFD8 File Offset: 0x0001B3D8
		public TileResultCode SetFogAction(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float start = float.Parse((string)args[0]);
			float end = float.Parse((string)args[1]);
			Blocksworld.bw.SetFog(start, end);
			if (args.Length > 2)
			{
				string stringArg = Util.GetStringArg(args, 2, "White");
				Blocksworld.worldSky.SetFogColor(stringArg);
			}
			return TileResultCode.True;
		}

		// Token: 0x0600031D RID: 797 RVA: 0x0001D030 File Offset: 0x0001B430
		protected bool IsCharacterFaceTexture(string texture)
		{
			return texture.Contains("Face") || texture == "Robot" || texture == "Texture Jack O Lantern";
		}

		// Token: 0x0600031E RID: 798 RVA: 0x0001D070 File Offset: 0x0001B470
		protected bool IsCharacterFaceWrapAroundTexture(string texture)
		{
			bool result = false;
			if (this.IsCharacterFaceTexture(texture))
			{
				Mapping mapping = Materials.GetMapping(texture);
				result = (mapping == Mapping.AllSidesTo4x1);
			}
			return result;
		}

		// Token: 0x0600031F RID: 799 RVA: 0x0001D09A File Offset: 0x0001B49A
		public virtual TileResultCode IsTexturedTo(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!(this.GetTexture(0) == (string)args[0])) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000320 RID: 800 RVA: 0x0001D0BC File Offset: 0x0001B4BC
		public virtual TileResultCode TextureToAction(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string newTexture = (string)args[0];
			Vector3 newNormal = (Vector3)args[1];
			int meshIndex = 0;
			if (args.Length > 2)
			{
				meshIndex = (int)args[2];
			}
			return this.TextureToAction(newTexture, newNormal, meshIndex);
		}

		// Token: 0x06000321 RID: 801 RVA: 0x0001D0F8 File Offset: 0x0001B4F8
		public virtual TileResultCode TextureToAction(string newTexture, Vector3 newNormal, int meshIndex)
		{
			string texture = this.GetTexture(meshIndex);
			if (newTexture != texture || this is BlockSky)
			{
				if (meshIndex == 0 && Blocksworld.CurrentState == State.Play)
				{
					this.RegisterTextureChanged(meshIndex, newTexture, texture);
					if (Materials.IsMaterialShaderTexture(newTexture))
					{
						for (int i = meshIndex + 1; i <= this.subMeshGameObjects.Count; i++)
						{
							this.TextureTo(newTexture, newNormal, false, i, false);
						}
					}
				}
				this.TextureTo(newTexture, newNormal, false, meshIndex, false);
				newTexture = this.GetTexture(meshIndex);
				if (Materials.HasMapping(newTexture))
				{
					if (Materials.GetMapping(texture) != Materials.GetMapping(newTexture))
					{
						this.skipUpdateSATVolumes = true;
						this.skipUpdateSATVolumes = false;
					}
				}
				else
				{
					BWLog.Info("Could not find texture: " + newTexture);
				}
			}
			this.ScaleTo(this.Scale(), false, true);
			return TileResultCode.True;
		}

		// Token: 0x06000322 RID: 802 RVA: 0x0001D1D8 File Offset: 0x0001B5D8
		private string SetMaterialTexture(string texture, int meshIndex)
		{
			if (this is BlockSky)
			{
				return texture;
			}
			if (texture == "Metal")
			{
				this.buoyancyMultiplier = 0.2f;
			}
			else
			{
				this.buoyancyMultiplier = this.GetBuoyancyMultiplier();
			}
			GameObject gameObject = this.go;
			int num = meshIndex - 1;
			if (num >= 0 && num < this.subMeshGameObjects.Count)
			{
				gameObject = this.subMeshGameObjects[num];
			}
			MaterialTextureDefinition physicMaterialDefinition = MaterialTexture.GetPhysicMaterialDefinition(texture);
			PhysicMaterial physicMaterialTexture = MaterialTexture.GetPhysicMaterialTexture(texture);
			Collider component = gameObject.GetComponent<Collider>();
			if (component != null)
			{
				if (physicMaterialDefinition != null && physicMaterialTexture != null)
				{
					component.sharedMaterial = physicMaterialTexture;
				}
				else
				{
					component.sharedMaterial = this.GetDefaultPhysicMaterialForPrefabCollider(0);
				}
			}
			if (physicMaterialDefinition != null)
			{
				if (this is BlockTerrain || this.fakeTerrain)
				{
					texture += physicMaterialDefinition.terrainTextureSuffix;
				}
				else if (!(this is BlockWater))
				{
					texture += physicMaterialDefinition.blockTextureSuffix;
				}
			}
			return texture;
		}

		// Token: 0x06000323 RID: 803 RVA: 0x0001D2E8 File Offset: 0x0001B6E8
		private PhysicMaterial GetDefaultPhysicMaterialForPrefabCollider(int meshIndex = 0)
		{
			GameObject gameObject = Blocksworld.prefabs[this.BlockType()];
			if (meshIndex == 0)
			{
				Collider component = gameObject.GetComponent<Collider>();
				if (component != null)
				{
					return component.sharedMaterial;
				}
			}
			else if (gameObject.transform.childCount > meshIndex)
			{
				Transform child = gameObject.transform.GetChild(meshIndex);
				if (child != null)
				{
					Collider component2 = child.GetComponent<Collider>();
					if (component2 != null)
					{
						return component2.sharedMaterial;
					}
				}
			}
			return null;
		}

		// Token: 0x06000324 RID: 804 RVA: 0x0001D370 File Offset: 0x0001B770
		private string ApplyTextureChangeRules(string texture, int meshIndex, string blockType)
		{
			if (Materials.textureApplicationRules.ContainsKey(texture))
			{
				List<TextureApplicationChangeRule> list = Materials.textureApplicationRules[texture];
				for (int i = 0; i < list.Count; i++)
				{
					TextureApplicationChangeRule textureApplicationChangeRule = list[i];
					bool flag = (textureApplicationChangeRule.meshIndex == meshIndex || textureApplicationChangeRule.meshIndex == -1) && textureApplicationChangeRule.blockType == blockType;
					if (textureApplicationChangeRule.negateCondition)
					{
						flag = !flag;
					}
					if (flag)
					{
						return textureApplicationChangeRule.texture;
					}
				}
			}
			return texture;
		}

		// Token: 0x06000325 RID: 805 RVA: 0x0001D400 File Offset: 0x0001B800
		private Vector3 ApplyNormalChanges(string blockType, Vector3 normal, int meshIndex)
		{
			if (meshIndex == 0 && blockType != null)
			{
				if (!(blockType == "Wedge") && !(blockType == "Slice") && !(blockType == "Slice Inverse"))
				{
					if (blockType == "Slice Corner")
					{
						if (normal.x > 0.99f || normal.z < -0.99f || normal.y < -0.99f)
						{
							return normal;
						}
						return Vector3.forward;
					}
				}
				else
				{
					if (normal.z > 0.01f)
					{
						return new Vector3(0f, 1f, 1f);
					}
					return normal;
				}
			}
			return normal;
		}

		// Token: 0x06000326 RID: 806 RVA: 0x0001D4C4 File Offset: 0x0001B8C4
		public virtual TileResultCode TextureTo(string texture, Vector3 normal, bool permanent, int meshIndex = 0, bool force = false)
		{
			this.FindSubMeshes();
			if (!force && texture != this.GetDefaultTexture(meshIndex))
			{
				if (MaterialTexture.IsPhysicMaterialTexture(texture))
				{
					if (meshIndex > 0 && !this.isTerrain)
					{
						return TileResultCode.False;
					}
					if (!this.isTerrain && !MaterialTexture.CanMaterialTextureNonTerrain(texture) && !Materials.IsNormalTerrainTexture(texture) && !this.fakeTerrain)
					{
						return TileResultCode.False;
					}
					if (this.canBeMaterialTextured != null && meshIndex < this.canBeMaterialTextured.Length && !this.canBeMaterialTextured[meshIndex])
					{
						return TileResultCode.False;
					}
				}
				else if (this.canBeTextured != null && meshIndex < this.canBeTextured.Length && !this.canBeTextured[meshIndex] && Materials.TextureRequiresUVs(texture))
				{
					return TileResultCode.False;
				}
				BlockMetaData blockMetaData = this.GetBlockMetaData();
				if (blockMetaData != null && blockMetaData.meshDatas.Length > meshIndex)
				{
					bool flag = Materials.IsMaterialShaderTexture(texture);
					if (flag)
					{
						if (!blockMetaData.meshDatas[meshIndex].canBeMaterialTextured)
						{
							return TileResultCode.False;
						}
					}
					else if (!blockMetaData.meshDatas[meshIndex].canBeTextured)
					{
						return TileResultCode.False;
					}
				}
			}
			string text = this.BlockType();
			bool flag2 = normal == Vector3.zero;
			if (permanent && !flag2)
			{
				Vector3 vector;
				if (TextureNormalRestriction.CanTextureBlockWithNormal(this, texture, text, normal, meshIndex, out vector))
				{
					normal = vector;
				}
				else if (Materials.TextureRequiresUVs(texture))
				{
					return TileResultCode.False;
				}
			}
			texture = this.SetMaterialTexture(texture, meshIndex);
			texture = this.ApplyTextureChangeRules(texture, meshIndex, text);
			normal = this.ApplyNormalChanges(text, normal, meshIndex);
			string text2 = (!permanent) ? this.GetTexture(meshIndex) : string.Empty;
			bool flag3 = text2 != texture;
			if (meshIndex > 0)
			{
				this.TextureToSubMesh(texture, normal, permanent, meshIndex);
				if (Blocksworld.CurrentState == State.Play && flag3)
				{
					this.UpdateBlockPropertiesForTextureAssignment(meshIndex, false);
				}
				return TileResultCode.True;
			}
			if (flag2)
			{
				normal = (Vector3)this.tiles[0][5].gaf.Args[1];
			}
			string paint = this.GetPaint(0);
			if (this.mesh != null)
			{
				Materials.SetMaterial(this.go, this.mesh, text, paint, texture, normal, Vector3.one, text2);
			}
			if (!Blocksworld.renderingShadows)
			{
				this.UpdateShadowColors(paint, texture, text2);
			}
			bool flag4 = this.isTransparent;
			this.isTransparent = Materials.TextureIsTransparent(texture);
			if (this.hasShadow && !this.vanished && flag3)
			{
				this.lastShadowHitDistance = -2f;
			}
			if (permanent)
			{
				if (texture != null)
				{
					Tile tile = this.tiles[0][5];
					normal = new Vector3(Mathf.Round(normal.x), Mathf.Round(normal.y), Mathf.Round(normal.z));
					if (tile.IsShowing())
					{
						tile.Show(false);
						tile.gaf.Args[0] = texture;
						tile.gaf.Args[1] = normal;
						tile.Show(true);
					}
					else
					{
						tile.gaf.Args[0] = texture;
						tile.gaf.Args[1] = normal;
					}
				}
				else
				{
					BWLog.Warning("TextureTo() trying to set texture to null");
				}
			}
			if (this.isTerrain && !(this is BlockSky))
			{
				this.PaintTo(paint, permanent, 0);
			}
			if (Blocksworld.CurrentState == State.Play && flag3)
			{
				this.UpdateBlockPropertiesForTextureAssignment(meshIndex, false);
			}
			this.UpdateRuntimeInvisible();
			if (flag4 != this.isTransparent)
			{
				this.UpdateNeighboringConnections();
			}
			return TileResultCode.True;
		}

		// Token: 0x06000327 RID: 807 RVA: 0x0001D88C File Offset: 0x0001BC8C
		public virtual TileResultCode TextureToSubMesh(string texture, Vector3 normal, bool permanent, int meshIndex = 0)
		{
			if (meshIndex > 0)
			{
				this.FindSubMeshes();
				int num = meshIndex - 1;
				if (num < this.subMeshGameObjects.Count)
				{
					GameObject gameObject = this.subMeshGameObjects[num];
					if (gameObject != null)
					{
						Mesh x = null;
						if (this.childMeshes != null && this.childMeshes.ContainsKey(gameObject.name))
						{
							x = this.childMeshes[gameObject.name];
						}
						else
						{
							MeshFilter component = gameObject.GetComponent<MeshFilter>();
							if (component != null)
							{
								x = component.sharedMesh;
							}
						}
						if (x != null)
						{
							string oldTexture = (!permanent) ? this.subMeshTextures[num] : string.Empty;
							Materials.SetMaterial(gameObject, x, gameObject.name, this.GetPaint(meshIndex), texture, normal, Vector3.one, oldTexture);
							if (permanent)
							{
								this.SetSubmeshInitTextureToTile(meshIndex, texture, normal, false);
							}
							this.subMeshTextures[num] = texture;
							this.subMeshTextureNormals[num] = normal;
						}
						else
						{
							BWLog.Info("TextureToSubMesh: Unable to find submesh with index " + num);
						}
					}
				}
			}
			this.UpdateRuntimeInvisible();
			return TileResultCode.True;
		}

		// Token: 0x06000328 RID: 808 RVA: 0x0001D9BE File Offset: 0x0001BDBE
		public TileResultCode IsFirstFrame(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!Blocksworld.isFirstFrame) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000329 RID: 809 RVA: 0x0001D9D1 File Offset: 0x0001BDD1
		public TileResultCode IsPullLockedSensor(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!PullObjectGesture.IsPullLocked(this)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x0600032A RID: 810 RVA: 0x0001D9E5 File Offset: 0x0001BDE5
		public virtual TileResultCode PullLockBlock(ScriptRowExecutionInfo eInfo, object[] args)
		{
			PullObjectGesture.PullLock(this);
			return TileResultCode.True;
		}

		// Token: 0x0600032B RID: 811 RVA: 0x0001D9F0 File Offset: 0x0001BDF0
		public TileResultCode PullLockChunk(ScriptRowExecutionInfo eInfo, object[] args)
		{
			for (int i = 0; i < this.chunk.blocks.Count; i++)
			{
				PullObjectGesture.PullLock(this.chunk.blocks[i]);
			}
			return TileResultCode.True;
		}

		// Token: 0x0600032C RID: 812 RVA: 0x0001DA35 File Offset: 0x0001BE35
		public static void ClearConnectedCache()
		{
			Block.connectedCache.Clear();
			Block.connectedChunks.Clear();
		}

		// Token: 0x0600032D RID: 813 RVA: 0x0001DA4C File Offset: 0x0001BE4C
		public bool UpdateConnectedCache()
		{
			if (!Block.connectedCache.ContainsKey(this))
			{
				List<Block> list = ConnectednessGraph.ConnectedComponent(this, 3, null, true);
				HashSet<Chunk> hashSet = new HashSet<Chunk>();
				HashSet<Block> hashSet2 = new HashSet<Block>(list);
				foreach (Block block in new List<Block>(list))
				{
					if (!block.broken || (!(block.go == null) && block.go.activeSelf))
					{
						hashSet.Add(block.chunk);
						BlockGroup groupOfType = block.GetGroupOfType("tank-treads");
						if (groupOfType != null && block.IsMainBlockInGroup("tank-treads"))
						{
							foreach (Block item in groupOfType.GetBlocks())
							{
								if (!hashSet2.Contains(item))
								{
									list.Add(item);
									hashSet2.Add(item);
								}
							}
						}
						BlockGroup groupOfType2 = block.GetGroupOfType("teleport-volume");
						if (groupOfType2 != null && block.IsMainBlockInGroup("teleport-volume"))
						{
							foreach (Block item2 in groupOfType2.GetBlocks())
							{
								if (!hashSet2.Contains(item2))
								{
									list.Add(item2);
									hashSet2.Add(item2);
								}
							}
						}
					}
				}
				for (int k = 0; k < list.Count; k++)
				{
					Block block2 = list[k];
					block2.modelBlock = list[0];
					Block.connectedCache[block2] = list;
					Block.connectedChunks[block2] = hashSet;
				}
				return true;
			}
			return false;
		}

		// Token: 0x0600032E RID: 814 RVA: 0x0001DC3C File Offset: 0x0001C03C
		public TileResultCode SetSpawnpoint(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 0, 0);
			CheckpointSystem.SetSpawnPoint(this, intArg);
			return TileResultCode.True;
		}

		// Token: 0x0600032F RID: 815 RVA: 0x0001DC5C File Offset: 0x0001C05C
		public TileResultCode SetActiveCheckpoint(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 0, 0);
			CheckpointSystem.SetActiveCheckPoint(this, intArg, true);
			return TileResultCode.True;
		}

		// Token: 0x06000330 RID: 816 RVA: 0x0001DC7C File Offset: 0x0001C07C
		public TileResultCode Spawn(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 0, 0);
			float floatArg = Util.GetFloatArg(args, 1, 1f);
			if (eInfo.timer == 0f)
			{
				CheckpointSystem.Spawn(this, intArg);
			}
			if (eInfo.timer >= floatArg)
			{
				return TileResultCode.True;
			}
			return TileResultCode.Delayed;
		}

		// Token: 0x06000331 RID: 817 RVA: 0x0001DCC8 File Offset: 0x0001C0C8
		public TileResultCode PullLockModel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.UpdateConnectedCache();
			List<Block> list = Block.connectedCache[this];
			foreach (Block b in list)
			{
				PullObjectGesture.PullLock(b);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000332 RID: 818 RVA: 0x0001DD34 File Offset: 0x0001C134
		public TileResultCode IsFrozen(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!this.didFix && !this.isTerrain) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000333 RID: 819 RVA: 0x0001DD53 File Offset: 0x0001C153
		public TileResultCode IsNotFrozen(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (this.didFix || this.isTerrain) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000334 RID: 820 RVA: 0x0001DD72 File Offset: 0x0001C172
		public TileResultCode IsPhantom(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!this.isRuntimePhantom) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000335 RID: 821 RVA: 0x0001DD86 File Offset: 0x0001C186
		public TileResultCode IsNotPhantom(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!this.isRuntimePhantom) ? TileResultCode.True : TileResultCode.False;
		}

		// Token: 0x06000336 RID: 822 RVA: 0x0001DD9C File Offset: 0x0001C19C
		public TileResultCode SetPhantom(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.isRuntimePhantom = true;
			this.goLayerAssignment = Layer.Phantom;
			this.go.layer = 10;
			IEnumerator enumerator = this.go.transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform = (Transform)obj;
					transform.gameObject.layer = 10;
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
			if (this is BlockAnimatedCharacter)
			{
				BlockAnimatedCharacter blockAnimatedCharacter = this as BlockAnimatedCharacter;
				blockAnimatedCharacter.SetLayer(Layer.Phantom);
			}
			else if (this is BlockCharacter)
			{
				BlockCharacter blockCharacter = this as BlockCharacter;
				blockCharacter.SetLayer(Layer.Phantom);
			}
			else if (this is BlockProceduralCollider)
			{
				BlockProceduralCollider blockProceduralCollider = this as BlockProceduralCollider;
				blockProceduralCollider.SetLayer(Layer.Phantom);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000337 RID: 823 RVA: 0x0001DE88 File Offset: 0x0001C288
		public TileResultCode SetUnphantom(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.isRuntimePhantom = false;
			this.goLayerAssignment = ((!this.isTerrain) ? Layer.Default : Layer.Terrain);
			this.go.layer = (int)this.goLayerAssignment;
			IEnumerator enumerator = this.go.transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform = (Transform)obj;
					transform.gameObject.layer = (int)this.goLayerAssignment;
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
			if (this is BlockAnimatedCharacter)
			{
				BlockAnimatedCharacter blockAnimatedCharacter = this as BlockAnimatedCharacter;
				blockAnimatedCharacter.SetLayer(this.goLayerAssignment);
			}
			else if (this is BlockCharacter)
			{
				BlockCharacter blockCharacter = this as BlockCharacter;
				blockCharacter.SetLayer(this.goLayerAssignment);
			}
			else if (this is BlockProceduralCollider)
			{
				BlockProceduralCollider blockProceduralCollider = this as BlockProceduralCollider;
				blockProceduralCollider.SetLayer(this.goLayerAssignment);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000338 RID: 824 RVA: 0x0001DF98 File Offset: 0x0001C398
		public TileResultCode IsPhantomModel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.UpdateConnectedCache();
			List<Block> list = Block.connectedCache[this];
			if (list.Count > 0)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (!list[i].isRuntimePhantom)
					{
						return TileResultCode.False;
					}
				}
				return TileResultCode.True;
			}
			return this.IsPhantom(eInfo, args);
		}

		// Token: 0x06000339 RID: 825 RVA: 0x0001DFF8 File Offset: 0x0001C3F8
		public TileResultCode IsNotPhantomModel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.UpdateConnectedCache();
			List<Block> list = Block.connectedCache[this];
			if (list.Count > 0)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (!list[i].isRuntimePhantom)
					{
						return TileResultCode.True;
					}
				}
				return TileResultCode.False;
			}
			return this.IsNotPhantom(eInfo, args);
		}

		// Token: 0x0600033A RID: 826 RVA: 0x0001E058 File Offset: 0x0001C458
		public TileResultCode SetPhantomModel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.isRuntimePhantom = true;
			this.UpdateConnectedCache();
			List<Block> list = Block.connectedCache[this];
			for (int i = 0; i < list.Count; i++)
			{
				list[i].isRuntimePhantom = true;
				list[i].goLayerAssignment = Layer.Phantom;
				list[i].go.layer = 10;
				IEnumerator enumerator = list[i].go.transform.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						object obj = enumerator.Current;
						Transform transform = (Transform)obj;
						transform.gameObject.layer = 10;
					}
				}
				finally
				{
					IDisposable disposable;
					if ((disposable = (enumerator as IDisposable)) != null)
					{
						disposable.Dispose();
					}
				}
				if (list[i] is BlockAnimatedCharacter)
				{
					BlockAnimatedCharacter blockAnimatedCharacter = list[i] as BlockAnimatedCharacter;
					blockAnimatedCharacter.SetLayer(Layer.Phantom);
				}
				else if (list[i] is BlockCharacter)
				{
					BlockCharacter blockCharacter = list[i] as BlockCharacter;
					blockCharacter.SetLayer(Layer.Phantom);
				}
				else if (list[i] is BlockProceduralCollider)
				{
					BlockProceduralCollider blockProceduralCollider = list[i] as BlockProceduralCollider;
					blockProceduralCollider.SetLayer(Layer.Phantom);
				}
			}
			return TileResultCode.True;
		}

		// Token: 0x0600033B RID: 827 RVA: 0x0001E1B4 File Offset: 0x0001C5B4
		public TileResultCode SetUnphantomModel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.isRuntimePhantom = false;
			this.UpdateConnectedCache();
			List<Block> list = Block.connectedCache[this];
			for (int i = 0; i < list.Count; i++)
			{
				list[i].isRuntimePhantom = false;
				list[i].goLayerAssignment = ((!list[i].isTerrain) ? Layer.Default : Layer.Terrain);
				list[i].go.layer = (int)list[i].goLayerAssignment;
				IEnumerator enumerator = list[i].go.transform.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						object obj = enumerator.Current;
						Transform transform = (Transform)obj;
						transform.gameObject.layer = (int)list[i].goLayerAssignment;
					}
				}
				finally
				{
					IDisposable disposable;
					if ((disposable = (enumerator as IDisposable)) != null)
					{
						disposable.Dispose();
					}
				}
				if (list[i] is BlockAnimatedCharacter)
				{
					BlockAnimatedCharacter blockAnimatedCharacter = list[i] as BlockAnimatedCharacter;
					blockAnimatedCharacter.SetLayer(list[i].goLayerAssignment);
				}
				else if (list[i] is BlockCharacter)
				{
					BlockCharacter blockCharacter = list[i] as BlockCharacter;
					blockCharacter.SetLayer(list[i].goLayerAssignment);
				}
				else if (list[i] is BlockProceduralCollider)
				{
					BlockProceduralCollider blockProceduralCollider = list[i] as BlockProceduralCollider;
					blockProceduralCollider.SetLayer(list[i].goLayerAssignment);
				}
			}
			return TileResultCode.True;
		}

		// Token: 0x0600033C RID: 828 RVA: 0x0001E358 File Offset: 0x0001C758
		public bool IsFixed()
		{
			if (this.didFix || this.isTerrain)
			{
				return true;
			}
			for (int i = 0; i < this.tiles.Count; i++)
			{
				List<Tile> list = this.tiles[i];
				if (list.Count > 1 && list[0].gaf.Predicate == Block.predicateThen && list[1].gaf.Predicate == Block.predicateFreeze)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600033D RID: 829 RVA: 0x0001E3EC File Offset: 0x0001C7EC
		public bool IsFixedInTerrain()
		{
			BlockMetaData blockMetaData = this.GetBlockMetaData();
			if (blockMetaData != null && !blockMetaData.freezeInTerrain)
			{
				return false;
			}
			string a = this.BlockType();
			if (a == "Position" || a == "Volume Block" || a == "Teleport Volume Block")
			{
				return false;
			}
			Bounds bounds = this.GetBounds();
			float num = 0.1f;
			float num2 = bounds.extents.x - num;
			float num3 = bounds.extents.y - num;
			float num4 = bounds.extents.z - num;
			Vector3 point = bounds.center + new Vector3(num2, -num3, num4);
			Vector3 point2 = bounds.center + new Vector3(num2, -num3, -num4);
			Vector3 point3 = bounds.center + new Vector3(-num2, -num3, -num4);
			Vector3 point4 = bounds.center + new Vector3(-num2, -num3, num4);
			return Util.PointWithinTerrain(point, false) || Util.PointWithinTerrain(point2, false) || Util.PointWithinTerrain(point3, false) || Util.PointWithinTerrain(point4, false);
		}

		// Token: 0x0600033E RID: 830 RVA: 0x0001E540 File Offset: 0x0001C940
		private static HashSet<Predicate> GetKeepRBPreds()
		{
			if (Block.keepRBPreds == null)
			{
				Block.keepRBPreds = new HashSet<Predicate>
				{
					Block.predicateImpact,
					Block.predicateImpactModel,
					Block.predicateParticleImpact,
					Block.predicateParticleImpactModel,
					Block.predicateBump,
					Block.predicateBumpChunk,
					Block.predicateBumpModel,
					Block.predicateTaggedBump,
					Block.predicateTaggedBumpChunk,
					Block.predicateTaggedBumpModel,
					Block.predicateWithinWater,
					Block.predicateWithinTaggedWater,
					Block.predicateModelWithinWater,
					Block.predicateModelWithinTaggedWater,
					Block.predicateUnfreeze
				};
			}
			return Block.keepRBPreds;
		}

		// Token: 0x0600033F RID: 831 RVA: 0x0001E61C File Offset: 0x0001CA1C
		public static bool IsKeepRBChunkBlocks(List<Block> blocks)
		{
			if (blocks.Count == 0)
			{
				return false;
			}
			Block block = blocks[0];
			if (block.isTerrain)
			{
				return false;
			}
			List<Block> list = Block.connectedCache[block];
			foreach (Block block2 in list)
			{
				if (block2.connectionTypes.Contains(2) || block2.connectionTypes.Contains(-2))
				{
					return true;
				}
			}
			foreach (Block block3 in blocks)
			{
				if (block3 is BlockWalkable || block3 is BlockCharacter || block3 is BlockAbstractLegs || block3 is BlockAbstractWheel || block3 is BlockAbstractMotor || block3 is BlockAbstractTorsionSpring || block3 is BlockTankTreadsWheel || block3 is BlockMissile || block3 is BlockAbstractPlatform || block3 is BlockPiston)
				{
					return true;
				}
			}
			HashSet<Predicate> manyPreds = Block.GetKeepRBPreds();
			foreach (Block block4 in list)
			{
				if (block4.ContainsTileWithAnyPredicateInPlayMode2(manyPreds))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000340 RID: 832 RVA: 0x0001E7E8 File Offset: 0x0001CBE8
		public virtual void ReassignedToChunk(Chunk c)
		{
		}

		// Token: 0x06000341 RID: 833 RVA: 0x0001E7EC File Offset: 0x0001CBEC
		public virtual TileResultCode Freeze(bool informModelBlocks)
		{
			if (this.isTreasure)
			{
				TreasureHandler.BlockFrozen(this);
				return TileResultCode.True;
			}
			if (!this.didFix && this.goT.parent != null)
			{
				this.didFix = true;
				if (this.chunk.go == null)
				{
					return TileResultCode.True;
				}
				bool flag = Block.IsKeepRBChunkBlocks(this.chunk.blocks);
				Rigidbody rb = this.chunk.rb;
				if (rb != null)
				{
					if (flag)
					{
						rb.isKinematic = true;
					}
					else
					{
						this.hadRigidBody = true;
						this.chunk.RemoveRigidbody();
					}
				}
				if (informModelBlocks)
				{
					this.UpdateConnectedCache();
					List<Block> list = Block.connectedCache[this];
					list.ForEach(delegate(Block b)
					{
						b.ChunkInModelFrozen();
					});
				}
			}
			return TileResultCode.True;
		}

		// Token: 0x06000342 RID: 834 RVA: 0x0001E8D6 File Offset: 0x0001CCD6
		public virtual TileResultCode Freeze(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return this.Freeze(true);
		}

		// Token: 0x06000343 RID: 835 RVA: 0x0001E8DF File Offset: 0x0001CCDF
		public virtual TileResultCode Unfreeze(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.Unfreeze();
			return TileResultCode.True;
		}

		// Token: 0x06000344 RID: 836 RVA: 0x0001E8E8 File Offset: 0x0001CCE8
		public virtual void Unfreeze()
		{
			if (this.isTreasure)
			{
				TreasureHandler.BlockUnfrozen(this);
				return;
			}
			if (this.didFix)
			{
				this.didFix = false;
				bool flag = false;
				List<Block> list = Block.connectedCache[this];
				for (int i = 0; i < list.Count; i++)
				{
					Block block = list[i];
					flag |= block.didFix;
				}
				if (!flag)
				{
					Rigidbody rigidbody = this.chunk.rb;
					if (null == rigidbody && this.hadRigidBody)
					{
						this.hadRigidBody = false;
						rigidbody = this.chunk.AddRigidbody();
					}
					if (rigidbody != null && !this.vanished)
					{
						rigidbody.isKinematic = false;
						rigidbody.WakeUp();
					}
					list.ForEach(delegate(Block b)
					{
						b.ChunkInModelUnfrozen();
					});
				}
			}
		}

		// Token: 0x06000345 RID: 837 RVA: 0x0001E9D8 File Offset: 0x0001CDD8
		public TileResultCode IsBrokenOff(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!(this.goT.parent.gameObject.name == "Broken")) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000346 RID: 838 RVA: 0x0001EA08 File Offset: 0x0001CE08
		public TileResultCode BreakOff(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.goT.parent.gameObject.name == "Broken")
			{
				return TileResultCode.False;
			}
			GameObject gameObject = new GameObject("Broken");
			gameObject.transform.position = this.goT.position;
			Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
			if (Blocksworld.interpolateRigidBodies)
			{
				rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
			}
			rigidbody.mass = this.GetMass();
			gameObject.GetComponent<Rigidbody>().velocity = this.goT.parent.GetComponent<Rigidbody>().velocity;
			gameObject.GetComponent<Rigidbody>().angularVelocity = this.goT.parent.GetComponent<Rigidbody>().angularVelocity;
			Transform parent = this.goT.parent;
			IEnumerator enumerator = parent.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform = (Transform)obj;
					transform.GetComponent<Collider>().enabled = false;
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
			this.goT.parent = gameObject.transform;
			IEnumerator enumerator2 = parent.GetEnumerator();
			try
			{
				while (enumerator2.MoveNext())
				{
					object obj2 = enumerator2.Current;
					Transform transform2 = (Transform)obj2;
					transform2.GetComponent<Collider>().enabled = true;
				}
			}
			finally
			{
				IDisposable disposable2;
				if ((disposable2 = (enumerator2 as IDisposable)) != null)
				{
					disposable2.Dispose();
				}
			}
			this.go.GetComponent<Collider>().enabled = true;
			return TileResultCode.True;
		}

		// Token: 0x06000347 RID: 839 RVA: 0x0001EBAC File Offset: 0x0001CFAC
		public TileResultCode IsExploded(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!this.broken) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000348 RID: 840 RVA: 0x0001EBC0 File Offset: 0x0001CFC0
		public void CreatePositionedAudioSourceIfNecessary(string sfxNameWhenCreate = "Create", GameObject theGo = null)
		{
			if (this.audioSource == null || !this.audioSource.enabled)
			{
				if (theGo == null)
				{
					theGo = this.go;
				}
				if (this.audioSource == null)
				{
					this.audioSource = this.CreateAudioSource(sfxNameWhenCreate, theGo);
				}
			}
			this.audioSource.enabled = true;
		}

		// Token: 0x06000349 RID: 841 RVA: 0x0001EC30 File Offset: 0x0001D030
		public void CreateLoopingPositionedAudioSourceIfNecessary(string sfxNameWhenCreate = "Create", GameObject theGo = null)
		{
			if (this.loopingAudioSource == null)
			{
				if (theGo == null)
				{
					theGo = this.go;
				}
				if (this.loopingAudioSource == null)
				{
					this.loopingAudioSource = this.CreateAudioSource(sfxNameWhenCreate, theGo);
				}
			}
			this.loopingAudioSource.enabled = true;
		}

		// Token: 0x0600034A RID: 842 RVA: 0x0001EC90 File Offset: 0x0001D090
		private AudioSource CreateAudioSource(string sfxNameWhenCreate = "Create", GameObject theGo = null)
		{
			AudioSource audioSource = theGo.AddComponent<AudioSource>();
			Sound.SetWorldAudioSourceParams(audioSource, 5f, 150f, AudioRolloffMode.Logarithmic);
			audioSource.playOnAwake = false;
			audioSource.clip = Sound.GetSfx(sfxNameWhenCreate);
			return audioSource;
		}

		// Token: 0x0600034B RID: 843 RVA: 0x0001ECCC File Offset: 0x0001D0CC
		public void PlayPositionedSound(string sfxName, float volume = 1f, float pitch = 1f)
		{
			if (Block.f__mg_cache6 == null)
			{
				Block.f__mg_cache6 = new Action<Block, string, float, float>(Block.PlayPositionedSoundNow);
			}
			DelegateCommand<Block, string, float, float> c = new DelegateCommand<Block, string, float, float>(this, sfxName, volume, pitch, Block.f__mg_cache6);
			Blocksworld.AddFixedUpdateCommand(c);
		}

		// Token: 0x0600034C RID: 844 RVA: 0x0001ED08 File Offset: 0x0001D108
		public void PlayPositionedSoundAfterDelay(int delay, string sfxName, float volume = 1f, float pitch = 1f)
		{
			if (Block.f__mg_cache7 == null)
			{
				Block.f__mg_cache7 = new Action<Block, string, float, float>(Block.PlayPositionedSoundNow);
			}
			DelayedDelegateCommand<Block, string, float, float> c = new DelayedDelegateCommand<Block, string, float, float>(this, sfxName, volume, pitch, Block.f__mg_cache7, delay);
			Blocksworld.AddFixedUpdateCommand(c);
		}

		// Token: 0x0600034D RID: 845 RVA: 0x0001ED44 File Offset: 0x0001D144
		private static void PlayPositionedSoundNow(Block block, string sfxName, float volume = 1f, float pitch = 1f)
		{
			if (block.go == null)
			{
				return;
			}
			block.CreatePositionedAudioSourceIfNecessary(sfxName, null);
			if (block.audioSource != null && block.go.activeSelf && !block.vanished)
			{
				if (Sound.BlockIsMuted(block))
				{
					return;
				}
				Sound.PlaySound(sfxName, block.audioSource, true, volume, pitch, false);
			}
		}

		// Token: 0x0600034E RID: 846 RVA: 0x0001EDB4 File Offset: 0x0001D1B4
		public Vector3 GetChunkVelocity()
		{
			Vector3 result = Vector3.zero;
			Rigidbody rb = this.chunk.rb;
			if (rb != null && !rb.isKinematic)
			{
				result = rb.velocity;
			}
			return result;
		}

		// Token: 0x0600034F RID: 847 RVA: 0x0001EDF2 File Offset: 0x0001D1F2
		public virtual TileResultCode HitByExplosion(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!AbstractDetachCommand.HitByExplosion(this)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000350 RID: 848 RVA: 0x0001EE08 File Offset: 0x0001D208
		public virtual TileResultCode ExplodeLocal(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (eInfo.timer == 0f && !this.vanished && !this.broken)
			{
				float radius = Util.GetFloatArg(args, 0, 3f);
				RadialExplosionCommand c3 = new RadialExplosionCommand(5f, this.goT.position, this.GetChunkVelocity(), radius * 0.5f, radius, radius * 2f, null, string.Empty);
				Blocksworld.AddFixedUpdateCommand(c3);
				Sound.PlayPositionedOneShot("Local Explode", this.goT.position, 5f, Mathf.Max(120f, radius * 30f), 150f, AudioRolloffMode.Logarithmic);
				Block b = this;
				DelegateCommand c2 = new DelegateCommand(delegate(DelegateCommand c)
				{
					if (!Invincibility.IsInvincible(b))
					{
						BWSceneManager.RemovePlayBlock(b);
						this.ExplodeOffConnectedBlocks(radius, false);
						b.broken = true;
					}
				});
				Blocksworld.AddFixedUpdateCommand(c2);
				if (!Invincibility.IsInvincible(this))
				{
					this.broken = true;
					WorldSession.platformDelegate.TrackAchievementIncrease("fireworks_display", 1);
					return TileResultCode.True;
				}
			}
			return (eInfo.timer >= 0.5f) ? TileResultCode.True : TileResultCode.Delayed;
		}

		// Token: 0x06000351 RID: 849 RVA: 0x0001EF30 File Offset: 0x0001D330
		public virtual TileResultCode ExplodeTag(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (eInfo.timer == 0f && !this.vanished && !this.broken)
			{
				string stringArg = Util.GetStringArg(args, 0, string.Empty);
				float radius = Util.GetFloatArg(args, 1, 3f);
				RadialExplosionCommand c3 = new RadialExplosionCommand(5f, this.goT.position, this.GetChunkVelocity(), radius * 0.5f, radius, radius * 2f, null, stringArg);
				Blocksworld.AddFixedUpdateCommand(c3);
				Sound.PlayPositionedOneShot("Local Explode", this.goT.position, 5f, Mathf.Max(120f, radius * 30f), 150f, AudioRolloffMode.Logarithmic);
				Block b = this;
				DelegateCommand c2 = new DelegateCommand(delegate(DelegateCommand c)
				{
					if (!Invincibility.IsInvincible(b))
					{
						BWSceneManager.RemovePlayBlock(b);
						this.ExplodeOffConnectedBlocks(radius, false);
						b.broken = true;
					}
				});
				Blocksworld.AddFixedUpdateCommand(c2);
				if (!Invincibility.IsInvincible(this))
				{
					this.broken = true;
					WorldSession.platformDelegate.TrackAchievementIncrease("fireworks_display", 1);
					return TileResultCode.True;
				}
			}
			return (eInfo.timer >= 0.5f) ? TileResultCode.True : TileResultCode.Delayed;
		}

		// Token: 0x06000352 RID: 850 RVA: 0x0001F064 File Offset: 0x0001D464
		private void AddInvincibilityStep()
		{
			if (Block.stepInvincibilityCommand == null)
			{
				Block.stepInvincibilityCommand = new DelegateMultistepCommand(delegate()
				{
					Invincibility.Step();
				}, 3);
			}
			else
			{
				Block.stepInvincibilityCommand.SetSteps(3);
			}
			Blocksworld.AddFixedUpdateUniqueCommand(Block.stepInvincibilityCommand, true);
		}

		// Token: 0x06000353 RID: 851 RVA: 0x0001F0BE File Offset: 0x0001D4BE
		public virtual TileResultCode IsInvincible(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!Invincibility.IsInvincible(this)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000354 RID: 852 RVA: 0x0001F0D2 File Offset: 0x0001D4D2
		public virtual TileResultCode IsInvincibleModel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return this.IsInvincible(eInfo, args);
		}

		// Token: 0x06000355 RID: 853 RVA: 0x0001F0DC File Offset: 0x0001D4DC
		public virtual TileResultCode IsShieldBlocking(ScriptRowExecutionInfo eInfo, object[] args)
		{
			BlockAnimatedCharacter blockAnimatedCharacter = BlockAnimatedCharacter.FindBlockingPropHolder(this);
			return (blockAnimatedCharacter == null) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000356 RID: 854 RVA: 0x0001F100 File Offset: 0x0001D500
		public virtual TileResultCode IsWeaponAttacking(ScriptRowExecutionInfo eInfo, object[] args)
		{
			BlockAnimatedCharacter blockAnimatedCharacter = BlockAnimatedCharacter.FindAttackingPropHolder(this);
			return (blockAnimatedCharacter == null) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000357 RID: 855 RVA: 0x0001F121 File Offset: 0x0001D521
		public virtual TileResultCode SetInvincible(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.AddInvincibilityStep();
			Invincibility.SetBlockInvincible(this);
			return TileResultCode.True;
		}

		// Token: 0x06000358 RID: 856 RVA: 0x0001F130 File Offset: 0x0001D530
		public virtual TileResultCode SetInvincibleModel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.AddInvincibilityStep();
			Invincibility.SetModelInvincible(this);
			return TileResultCode.True;
		}

		// Token: 0x06000359 RID: 857 RVA: 0x0001F13F File Offset: 0x0001D53F
		public virtual TileResultCode IsDetached(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return this.boolToTileResult(!Block.connectedCache.ContainsKey(this) || Block.connectedCache[this].Count == 0);
		}

		// Token: 0x0600035A RID: 858 RVA: 0x0001F170 File Offset: 0x0001D570
		public virtual TileResultCode Detach(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.broken)
			{
				return TileResultCode.True;
			}
			if (Block.vanishingOrAppearingBlocks.Contains(this))
			{
				return TileResultCode.Delayed;
			}
			if (this.isTreasure)
			{
				if (TreasureHandler.IsPickingUpOrRespawning(this))
				{
					return TileResultCode.Delayed;
				}
				bool flag = TreasureHandler.IsHiddenTreasureModel(this);
				TreasureHandler.RemoveTreasureModel(this);
			}
			RadialSmashCommand c = new RadialSmashCommand(this, this.GetChunkVelocity(), 0f, null, false);
			Blocksworld.AddFixedUpdateCommand(c);
			this.broken = true;
			return TileResultCode.True;
		}

		// Token: 0x0600035B RID: 859 RVA: 0x0001F1E8 File Offset: 0x0001D5E8
		public virtual TileResultCode SmashLocal(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.broken)
			{
				return TileResultCode.True;
			}
			float floatArg = Util.GetFloatArg(args, 0, 3f);
			RadialSmashCommand c = new RadialSmashCommand(this, this.GetChunkVelocity(), floatArg, null, true);
			Blocksworld.AddFixedUpdateCommand(c);
			this.PlayPositionedSound("Explode", 1f, 1f);
			this.broken = true;
			return TileResultCode.True;
		}

		// Token: 0x0600035C RID: 860 RVA: 0x0001F244 File Offset: 0x0001D644
		public virtual TileResultCode Explode(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.broken)
			{
				return TileResultCode.True;
			}
			if (Block.vanishingOrAppearingBlocks.Contains(this))
			{
				return TileResultCode.Delayed;
			}
			bool isHiddenTreasure = false;
			if (this.isTreasure)
			{
				if (TreasureHandler.IsPickingUpOrRespawning(this))
				{
					return TileResultCode.Delayed;
				}
				isHiddenTreasure = TreasureHandler.IsHiddenTreasureModel(this);
				TreasureHandler.RemoveTreasureModel(this);
			}
			this.PlayPositionedSound("Explode", 1f, 1f);
			float floatArg = Util.GetFloatArg(args, 0, 1f);
			this.ExplodeOffConnectedBlocks(floatArg, isHiddenTreasure);
			return TileResultCode.True;
		}

		// Token: 0x0600035D RID: 861 RVA: 0x0001F2C4 File Offset: 0x0001D6C4
		public void ExplodeOffConnectedBlocks(float forceMultiplier = 1f, bool isHiddenTreasure = false)
		{
			List<Block> list;
			if (Block.connectedCache.ContainsKey(this))
			{
				list = Block.connectedCache[this];
			}
			else
			{
				list = ConnectednessGraph.ConnectedComponent(this, 3, null, true);
			}
			List<Block> nonVanishedBlocks = new List<Block>();
			list.ForEach(delegate(Block b)
			{
				if (b.vanished || isHiddenTreasure)
				{
					BWSceneManager.RemovePlayBlock(b);
					b.broken = true;
					b.go.SetActive(false);
				}
				else if (!b.broken)
				{
					nonVanishedBlocks.Add(b);
				}
			});
			if (nonVanishedBlocks.Count == 0)
			{
				return;
			}
			List<Vector3> list2 = new List<Vector3>();
			List<Vector3> list3 = new List<Vector3>();
			List<Vector3> list4 = new List<Vector3>();
			HashSet<Block> hashSet = new HashSet<Block>();
			List<Chunk> list5 = new List<Chunk>();
			foreach (Block block in nonVanishedBlocks)
			{
				Chunk chunk = block.chunk;
				if (chunk.go == null)
				{
					list2.Add(block.goT.position);
					list3.Add(Vector3.zero);
					list4.Add(Vector3.zero);
				}
				else
				{
					if (!list5.Contains(chunk))
					{
						list5.Add(chunk);
					}
					list2.Add(chunk.go.transform.position);
					if (chunk.rb != null)
					{
						list3.Add(chunk.rb.velocity);
						list4.Add(chunk.rb.angularVelocity);
					}
					else
					{
						list3.Add(Vector3.zero);
						list4.Add(Vector3.zero);
					}
					if (!(block is BlockPosition))
					{
						Collider component = block.go.GetComponent<Collider>();
						if (component != null)
						{
							component.isTrigger = false;
						}
					}
				}
			}
			for (int i = 0; i < list5.Count; i++)
			{
				Chunk chunk2 = list5[i];
				Rigidbody rb = chunk2.rb;
				if (rb == null || rb.isKinematic)
				{
					hashSet.UnionWith(chunk2.blocks);
				}
				Blocksworld.blocksworldCamera.Unfollow(chunk2);
				Blocksworld.chunks.Remove(chunk2);
				chunk2.Destroy(true);
			}
			List<Chunk> list6 = new List<Chunk>();
			for (int j = 0; j < nonVanishedBlocks.Count; j++)
			{
				Block block2 = nonVanishedBlocks[j];
				Block.connectedChunks.Remove(block2);
				block2.Break(list2[j], list3[j], list4[j]);
				Chunk chunk3 = new Chunk(new List<Block>
				{
					block2
				}, true);
				if (block2 is BlockPosition)
				{
					chunk3.rb.velocity = list3[j];
					chunk3.rb.angularVelocity = list4[j];
				}
				else if (Invincibility.IsInvincible(chunk3.blocks[0]))
				{
					chunk3.rb.isKinematic = true;
				}
				else
				{
					Block.AddExplosiveForce(chunk3.rb, chunk3.go.transform.position, list2[j], list3[j], list4[j], forceMultiplier);
				}
				Blocksworld.chunks.Add(chunk3);
				list6.Add(chunk3);
			}
			for (int k = 0; k < nonVanishedBlocks.Count; k++)
			{
				Block key = nonVanishedBlocks[k];
				Block.connectedChunks[key] = new HashSet<Chunk>(list6);
			}
			if (Blocksworld.worldOceanBlock != null)
			{
				foreach (Block b2 in hashSet)
				{
					Blocksworld.worldOceanBlock.AddBlockToSimulation(b2);
				}
			}
			Blocksworld.blocksworldCamera.UpdateChunkSpeeds();
		}

		// Token: 0x0600035E RID: 862 RVA: 0x0001F700 File Offset: 0x0001DB00
		public static void AddExplosiveForce(Rigidbody rb, Vector3 localPos, Vector3 chunkPos, Vector3 chunkVel, Vector3 chunkAngVel, float forceMultiplier = 1f)
		{
			rb.velocity = chunkVel;
			rb.angularVelocity = chunkAngVel;
			if (forceMultiplier > 0f)
			{
				float d = 5f * forceMultiplier;
				rb.AddForce((localPos - chunkPos).normalized * d + UnityEngine.Random.insideUnitSphere * d, ForceMode.Impulse);
			}
		}

		// Token: 0x0600035F RID: 863 RVA: 0x0001F760 File Offset: 0x0001DB60
		public TileResultCode IncreaseCounter(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string text = (string)args[0];
			Dictionary<string, int> counters;
			string key;
			(counters = Blocksworld.counters)[key = text] = counters[key] + 1;
			Blocksworld.countersActivated[text] = true;
			return TileResultCode.True;
		}

		// Token: 0x06000360 RID: 864 RVA: 0x0001F79C File Offset: 0x0001DB9C
		public TileResultCode DecreaseCounter(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string text = (string)args[0];
			Dictionary<string, int> counters;
			string key;
			(counters = Blocksworld.counters)[key = text] = counters[key] - 1;
			Blocksworld.countersActivated[text] = true;
			return TileResultCode.True;
		}

		// Token: 0x06000361 RID: 865 RVA: 0x0001F7D8 File Offset: 0x0001DBD8
		public TileResultCode CounterEquals(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string key = (string)args[0];
			Blocksworld.countersActivated[key] = true;
			int num = int.Parse((string)args[1]);
			Blocksworld.counterTargetsActivated[key] = true;
			Blocksworld.counterTargets[key] = num;
			if (num == Blocksworld.counters[key])
			{
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x06000362 RID: 866 RVA: 0x0001F834 File Offset: 0x0001DC34
		public TileResultCode SetCounter(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string key = (string)args[0];
			Blocksworld.countersActivated[key] = true;
			int value = int.Parse((string)args[1]);
			Blocksworld.counters[key] = value;
			return TileResultCode.True;
		}

		// Token: 0x06000363 RID: 867 RVA: 0x0001F874 File Offset: 0x0001DC74
		private void StepObjectAnimation()
		{
			if (this.animationFixedTime < Time.fixedTime)
			{
				this.animationStep++;
				this.animationFixedTime = Time.fixedTime;
				Transform transform = this.goT;
				Vector3 a = this.playPosition;
				Quaternion lhs = this.playRotation;
				Rigidbody rb = this.chunk.rb;
				if (rb != null)
				{
					rb.isKinematic = true;
				}
				transform = this.chunk.go.transform;
				a = this.parentPlayPosition;
				lhs = this.parentPlayRotation;
				transform.position = a + this.objectAnimationPositionOffset;
				transform.rotation = lhs * this.objectAnimationRotationOffset;
				this.objectAnimationPositionOffset = Vector3.zero;
				this.objectAnimationRotationOffset = Quaternion.identity;
				this.objectAnimationScaleOffset = Vector3.one;
			}
		}

		// Token: 0x06000364 RID: 868 RVA: 0x0001F944 File Offset: 0x0001DD44
		public TileResultCode LevitateAnimation(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float num = Blocksworld.fixedDeltaTime * (float)this.animationStep;
			this.objectAnimationPositionOffset += new Vector3(0f, 0.5f * Mathf.Sin(2f * num + this.playPosition.x * 10f + this.playPosition.z * 25f), 0f);
			Vector3 axis = new Vector3(0.2f, 0.9f, 0f);
			this.objectAnimationRotationOffset *= Quaternion.AngleAxis(this.playPosition.x * 10.3242f + this.playPosition.z * 1.456f + this.playPosition.y + 25f * num, axis);
			this.StepObjectAnimation();
			return TileResultCode.True;
		}

		// Token: 0x06000365 RID: 869 RVA: 0x0001FA20 File Offset: 0x0001DE20
		public TileResultCode SineScaleAnimation(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float num = Blocksworld.fixedDeltaTime * (float)this.animationStep;
			Vector3 vector = (Vector3)args[0];
			Vector3 vector2 = (Vector3)args[1];
			Vector3 vector3 = (Vector3)args[1];
			float num2 = 6.28318548f;
			this.objectAnimationScaleOffset = new Vector3(this.objectAnimationScaleOffset.x * vector.x * Mathf.Sin(num2 * (vector2.x * num + vector3.x)), this.objectAnimationScaleOffset.y * vector.y * Mathf.Sin(num2 * (vector2.y * num + vector3.y)), this.objectAnimationScaleOffset.z * vector.z * Mathf.Sin(num2 * (vector2.z * num + vector3.z)));
			this.objectAnimationPositionOffset += new Vector3(0f, 0.5f * Mathf.Sin(2f * num + this.playPosition.x * 10f + this.playPosition.z * 25f), 0f);
			Vector3 axis = new Vector3(0.2f, 0.9f, 0f);
			this.objectAnimationRotationOffset *= Quaternion.AngleAxis(this.playPosition.x * 10.3242f + this.playPosition.z * 1.456f + this.playPosition.y + 25f * num, axis);
			this.StepObjectAnimation();
			return TileResultCode.True;
		}

		// Token: 0x06000366 RID: 870 RVA: 0x0001FBB1 File Offset: 0x0001DFB1
		public AudioSource GetOrCreateLoopingAudioSource(string sfxName, float volume = 1f, float pitch = 1f)
		{
			return null;
		}

		// Token: 0x06000367 RID: 871 RVA: 0x0001FBB4 File Offset: 0x0001DFB4
		public AudioSource GetOrCreateLoopingPositionedAudioSource(string sfxName = "Create")
		{
			this.CreateLoopingPositionedAudioSourceIfNecessary(sfxName, null);
			return this.loopingAudioSource;
		}

		// Token: 0x06000368 RID: 872 RVA: 0x0001FBC4 File Offset: 0x0001DFC4
		public TileResultCode Animate(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return TileResultCode.True;
		}

		// Token: 0x06000369 RID: 873 RVA: 0x0001FBC8 File Offset: 0x0001DFC8
		public TileResultCode PlaySound(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string sfxName = (args.Length <= 0) ? "Create" : ((string)args[0]);
			string location = (args.Length <= 1) ? "Camera" : ((string)args[1]);
			string soundType = (args.Length <= 2) ? "OneShot" : ((string)args[2]);
			float volume = (args.Length <= 3) ? 1f : ((float)args[3]);
			float pitch = (args.Length <= 4) ? 1f : ((float)args[4]);
			return this.PlaySound(sfxName, location, soundType, volume, pitch, false, eInfo.timer);
		}

		// Token: 0x0600036A RID: 874 RVA: 0x0001FC78 File Offset: 0x0001E078
		public TileResultCode SoundSensor(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string text = (args.Length <= 0) ? string.Empty : ((string)args[0]);
			float num = 0f;
			if (text.Length > 0)
			{
				HashSet<Block> durationalSoundSources = Sound.GetDurationalSoundSources(text);
				if (durationalSoundSources != null)
				{
					Vector3 position = this.goT.position;
					foreach (Block block in durationalSoundSources)
					{
						if (block.go != null)
						{
							Vector3 position2 = block.goT.position;
							float magnitude = (position2 - position).magnitude;
							if (magnitude < 150f)
							{
								float a = 1f - magnitude / 150f;
								num = Mathf.Max(a, num);
							}
						}
					}
					eInfo.floatArg = Mathf.Min(eInfo.floatArg, num);
					if (num > 0f)
					{
						return TileResultCode.True;
					}
				}
			}
			return TileResultCode.False;
		}

		// Token: 0x0600036B RID: 875 RVA: 0x0001FD8C File Offset: 0x0001E18C
		public virtual TileResultCode Mute(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.Mute();
			Blocksworld.AddFixedUpdateUniqueCommand(Block.unmuteCommand, true);
			return TileResultCode.True;
		}

		// Token: 0x0600036C RID: 876 RVA: 0x0001FDA0 File Offset: 0x0001E1A0
		public TileResultCode MuteModel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.UpdateConnectedCache();
			List<Block> list = Block.connectedCache[this];
			for (int i = 0; i < list.Count; i++)
			{
				list[i].Mute();
			}
			Blocksworld.AddFixedUpdateUniqueCommand(Block.unmuteCommand, true);
			return TileResultCode.True;
		}

		// Token: 0x0600036D RID: 877 RVA: 0x0001FDEF File Offset: 0x0001E1EF
		public virtual void Mute()
		{
			Sound.MuteBlock(this);
		}

		// Token: 0x0600036E RID: 878 RVA: 0x0001FDF8 File Offset: 0x0001E1F8
		public TileResultCode PlaySoundDurational(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string text = (args.Length <= 0) ? "Create" : ((string)args[0]);
			string location = (args.Length <= 1) ? "Camera" : ((string)args[1]);
			string soundType = (args.Length <= 2) ? "OneShot" : ((string)args[2]);
			bool flag = eInfo.timer == 0f;
			ScriptRowData scriptRowData = ScriptRowData.GetScriptRowData(eInfo);
			string text2;
			if (flag)
			{
				if (Sound.GetRandomSfx(ref text))
				{
					scriptRowData.SetString("Rnd SFX", text);
				}
			}
			else if (scriptRowData.TryGetString("Rnd SFX", out text2))
			{
				text = text2;
			}
			float durationalSfxVolume = Sound.GetDurationalSfxVolume(text);
			return this.PlaySound(text, location, soundType, durationalSfxVolume, 1f, true, eInfo.timer);
		}

		// Token: 0x0600036F RID: 879 RVA: 0x0001FEC8 File Offset: 0x0001E2C8
		public virtual TileResultCode PlaySound(string sfxName, string location, string soundType, float volume, float pitch, bool durational = false, float timer = 0f)
		{
			bool flag = timer <= 0.001f;
			bool flag2 = soundType == "Loop";
			if ((flag || !durational) && !Sound.BlockIsMuted(this) && location != null)
			{
				if (!(location == "Camera"))
				{
					if (location == "Block")
					{
						this.UpdateWithinWaterLPFilter(null);
						if (soundType == "OneShot")
						{
							this.PlayPositionedSound(sfxName, volume, pitch);
						}
					}
				}
				else if (soundType == "OneShot")
				{
					Sound.PlaySound(sfxName, Sound.GetOrCreateOneShotAudioSource(), true, 1f, 1f, false);
				}
				else if (flag2)
				{
					Sound.PlaySound(sfxName, this.GetOrCreateLoopingAudioSource(sfxName, 1f, 1f), false, volume, pitch, false);
				}
			}
			if (Sound.sfxEnabled && flag2)
			{
				Block.loopSfxCommand.BlockPlaysLoop(this, sfxName, volume, pitch);
				Blocksworld.AddFixedUpdateUniqueCommand(Block.loopSfxCommand, false);
			}
			if (durational)
			{
				return this.UpdateDurationalSoundSource(sfxName, timer);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000370 RID: 880 RVA: 0x0001FFEC File Offset: 0x0001E3EC
		protected TileResultCode UpdateDurationalSoundSource(string sfxName, float timer)
		{
			float durationalSfxTime = Sound.GetDurationalSfxTime(sfxName);
			bool flag = timer >= durationalSfxTime;
			if (!Sound.BlockIsMuted(this))
			{
				Sound.AddDurationalSoundSource(sfxName, this);
			}
			return (!flag) ? TileResultCode.Delayed : TileResultCode.True;
		}

		// Token: 0x06000371 RID: 881 RVA: 0x00020028 File Offset: 0x0001E428
		public virtual TileResultCode PlayVfxDurational(string vfxName, float lengthMult, float timer, string colorName)
		{
			if (timer == 0f)
			{
				VisualEffect visualEffect = VisualEffect.CreateEffect(this, vfxName, lengthMult, colorName);
				if (visualEffect == null)
				{
					return TileResultCode.True;
				}
				visualEffect.Begin();
				if (vfxName == "WindLines")
				{
					return TileResultCode.True;
				}
				return TileResultCode.Delayed;
			}
			else
			{
				float num = lengthMult * VisualEffect.GetEffectLength(vfxName);
				if (timer < num)
				{
					return TileResultCode.Delayed;
				}
				return TileResultCode.True;
			}
		}

		// Token: 0x06000372 RID: 882 RVA: 0x00020088 File Offset: 0x0001E488
		public TileResultCode PlayVfxDurational(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArg = Util.GetStringArg(args, 0, "Sparkle");
			float floatArg = Util.GetFloatArg(args, 1, 1f);
			int num = (args.Length <= 2) ? 0 : ((int)args[2]);
			string colorName = (args.Length <= 3) ? "White" : ((string)args[3]);
			return this.PlayVfxDurational(stringArg, floatArg, eInfo.timer, colorName);
		}

		// Token: 0x06000373 RID: 883 RVA: 0x000200F2 File Offset: 0x0001E4F2
		public TileResultCode OnCollect(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!TreasureHandler.IsPickedUpThisFrame(this, null)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000374 RID: 884 RVA: 0x00020107 File Offset: 0x0001E507
		public TileResultCode OnCollectByTag(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!TreasureHandler.IsPickedUpThisFrame(this, (string)args[0])) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000375 RID: 885 RVA: 0x00020123 File Offset: 0x0001E523
		public TileResultCode RespawnPickup(ScriptRowExecutionInfo eInfo, object[] args)
		{
			TreasureHandler.Respawn(this);
			return TileResultCode.True;
		}

		// Token: 0x06000376 RID: 886 RVA: 0x0002012C File Offset: 0x0001E52C
		public TileResultCode ForceCollectPickup(ScriptRowExecutionInfo eInfo, object[] args)
		{
			TreasureHandler.ForceCollect(this, Util.GetStringArg(args, 0, null));
			return TileResultCode.True;
		}

		// Token: 0x06000377 RID: 887 RVA: 0x00020140 File Offset: 0x0001E540
		public TileResultCode SetAsTreasureBlockIcon(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 0, 0);
			TreasureHandler.SetAsTreasureBlockIcon(this, intArg);
			return TileResultCode.True;
		}

		// Token: 0x06000378 RID: 888 RVA: 0x00020160 File Offset: 0x0001E560
		public TileResultCode SetAsTreasureTextureIcon(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 0, 0);
			TreasureHandler.SetAsTreasureTextureIcon(this, intArg);
			return TileResultCode.True;
		}

		// Token: 0x06000379 RID: 889 RVA: 0x00020180 File Offset: 0x0001E580
		public TileResultCode SetAsCounterUIBlockIcon(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 0, 0);
			BlockCounterUI blockCounterUI;
			if (BlockCounterUI.allCounterBlocks.TryGetValue(intArg, out blockCounterUI))
			{
				blockCounterUI.SetCustomIconGAF(this.GetIconGaf());
			}
			return TileResultCode.True;
		}

		// Token: 0x0600037A RID: 890 RVA: 0x000201B8 File Offset: 0x0001E5B8
		public TileResultCode SetAsCounterUITextureIcon(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 0, 0);
			BlockCounterUI blockCounterUI;
			if (BlockCounterUI.allCounterBlocks.TryGetValue(intArg, out blockCounterUI))
			{
				blockCounterUI.SetCustomIconGAF(new GAF(Block.predicateTextureTo, new object[]
				{
					this.GetTexture(0),
					Vector3.zero
				}));
			}
			return TileResultCode.True;
		}

		// Token: 0x0600037B RID: 891 RVA: 0x00020210 File Offset: 0x0001E610
		public TileResultCode SetAsTimerUIBlockIcon(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 0, 0);
			BlockTimerUI blockTimerUI;
			if (BlockTimerUI.allTimerBlocks.TryGetValue(intArg, out blockTimerUI))
			{
				blockTimerUI.SetCustomIconGAF(this.GetIconGaf());
			}
			return TileResultCode.True;
		}

		// Token: 0x0600037C RID: 892 RVA: 0x00020248 File Offset: 0x0001E648
		public TileResultCode SetAsTimerUITextureIcon(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 0, 0);
			BlockTimerUI blockTimerUI;
			if (BlockTimerUI.allTimerBlocks.TryGetValue(intArg, out blockTimerUI))
			{
				blockTimerUI.SetCustomIconGAF(new GAF(Block.predicateTextureTo, new object[]
				{
					this.GetTexture(0),
					Vector3.zero
				}));
			}
			return TileResultCode.True;
		}

		// Token: 0x0600037D RID: 893 RVA: 0x000202A0 File Offset: 0x0001E6A0
		public TileResultCode SetAsGaugeUIBlockIcon(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 0, 0);
			BlockGaugeUI blockGaugeUI;
			if (BlockGaugeUI.allGaugeBlocks.TryGetValue(intArg, out blockGaugeUI))
			{
				blockGaugeUI.SetCustomIconGAF(this.GetIconGaf());
			}
			return TileResultCode.True;
		}

		// Token: 0x0600037E RID: 894 RVA: 0x000202D8 File Offset: 0x0001E6D8
		public TileResultCode SetAsGaugeUITextureIcon(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 0, 0);
			BlockGaugeUI blockGaugeUI;
			if (BlockGaugeUI.allGaugeBlocks.TryGetValue(intArg, out blockGaugeUI))
			{
				blockGaugeUI.SetCustomIconGAF(new GAF(Block.predicateTextureTo, new object[]
				{
					this.GetTexture(0),
					Vector3.zero
				}));
			}
			return TileResultCode.True;
		}

		// Token: 0x0600037F RID: 895 RVA: 0x00020330 File Offset: 0x0001E730
		public TileResultCode SetAsTreasureModel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (!this.broken)
			{
				int intArg = Util.GetIntArg(args, 0, 0);
				TreasureHandler.AddTreasureModel(this, null, true, intArg);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000380 RID: 896 RVA: 0x0002035C File Offset: 0x0001E75C
		public TileResultCode SetAsTreasureModelTag(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (!this.broken)
			{
				int intArg = Util.GetIntArg(args, 1, 0);
				TreasureHandler.AddTreasureModel(this, (string)args[0], true, intArg);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000381 RID: 897 RVA: 0x0002038E File Offset: 0x0001E78E
		public TileResultCode SetAsPickupModel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (!this.broken)
			{
				TreasureHandler.AddTreasureModel(this, null, false, 0);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000382 RID: 898 RVA: 0x000203A5 File Offset: 0x0001E7A5
		public TileResultCode SetAsPickupModelTag(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (!this.broken)
			{
				TreasureHandler.AddTreasureModel(this, (string)args[0], false, 0);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000383 RID: 899 RVA: 0x000203C3 File Offset: 0x0001E7C3
		public TileResultCode VisualizeReward(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return RewardVisualization.VisualizeReward(eInfo, args);
		}

		// Token: 0x06000384 RID: 900 RVA: 0x000203CC File Offset: 0x0001E7CC
		public virtual TileResultCode AnalogCeil(ScriptRowExecutionInfo eInfo, object[] args)
		{
			eInfo.floatArg = Mathf.Ceil(eInfo.floatArg);
			return (eInfo.floatArg <= 0f) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000385 RID: 901 RVA: 0x000203F6 File Offset: 0x0001E7F6
		public virtual TileResultCode AnalogFloor(ScriptRowExecutionInfo eInfo, object[] args)
		{
			eInfo.floatArg = Mathf.Floor(eInfo.floatArg);
			return (eInfo.floatArg <= 0f) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000386 RID: 902 RVA: 0x00020420 File Offset: 0x0001E820
		public virtual TileResultCode AnalogRound(ScriptRowExecutionInfo eInfo, object[] args)
		{
			eInfo.floatArg = Mathf.Round(eInfo.floatArg);
			return (eInfo.floatArg <= 0f) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000387 RID: 903 RVA: 0x0002044A File Offset: 0x0001E84A
		public virtual TileResultCode AnalogMin(ScriptRowExecutionInfo eInfo, object[] args)
		{
			eInfo.floatArg = Mathf.Min(eInfo.floatArg, Util.GetFloatArg(args, 0, 1f));
			return (eInfo.floatArg <= 0f) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000388 RID: 904 RVA: 0x00020480 File Offset: 0x0001E880
		public virtual TileResultCode AnalogMax(ScriptRowExecutionInfo eInfo, object[] args)
		{
			eInfo.floatArg = Mathf.Max(eInfo.floatArg, Util.GetFloatArg(args, 0, 1f));
			return (eInfo.floatArg <= 0f) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000389 RID: 905 RVA: 0x000204B8 File Offset: 0x0001E8B8
		public virtual TileResultCode AnalogClamp(ScriptRowExecutionInfo eInfo, object[] args)
		{
			eInfo.floatArg = Mathf.Clamp(eInfo.floatArg, Util.GetFloatArg(args, 0, 0f), Util.GetFloatArg(args, 1, 1f));
			return (eInfo.floatArg <= 0f) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x0600038A RID: 906 RVA: 0x00020508 File Offset: 0x0001E908
		private AnimationCurve GetVanishAnimCurve(float animTime)
		{
			if (Block.vanishAnimCurve == null)
			{
				float time = animTime * 0.2f;
				float value = 1.5f;
				Block.vanishAnimCurve = new AnimationCurve(new Keyframe[]
				{
					new Keyframe(0f, 1f, 0f, 0f),
					new Keyframe(time, value, 0f, 0f),
					new Keyframe(animTime, 0.02f, 0f, 0f)
				});
			}
			return Block.vanishAnimCurve;
		}

		// Token: 0x0600038B RID: 907 RVA: 0x000205A5 File Offset: 0x0001E9A5
		public virtual TileResultCode IsVanished(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!this.vanished) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x0600038C RID: 908 RVA: 0x000205B9 File Offset: 0x0001E9B9
		public virtual TileResultCode IsAppeared(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return this.vanished ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x0600038D RID: 909 RVA: 0x000205D0 File Offset: 0x0001E9D0
		public virtual TileResultCode Appear(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float num = 0.375f;
			if (eInfo.timer <= 0.25f)
			{
				if (this.go.GetComponent<Renderer>() != null && !this.go.GetComponent<Renderer>().enabled)
				{
					this.go.GetComponent<Renderer>().enabled = true;
				}
				if (this.go.GetComponent<Collider>() != null)
				{
					this.go.GetComponent<Collider>().enabled = true;
					this.go.GetComponent<Collider>().isTrigger = false;
				}
				if (this.goShadow != null)
				{
					this.goShadow.GetComponent<Renderer>().enabled = true;
				}
				Transform parent = this.goT.parent;
				if (parent != null && parent.GetComponent<Rigidbody>() != null)
				{
					parent.GetComponent<Rigidbody>().isKinematic = false;
					parent.GetComponent<Rigidbody>().WakeUp();
				}
				this.go.SetActive(true);
			}
			if (eInfo.timer >= num)
			{
				this.goT.localScale = Vector3.one;
				this.vanished = false;
				return TileResultCode.True;
			}
			if (this.CanAnimateScale())
			{
				this.GetVanishAnimCurve(num);
				float num2 = Mathf.Clamp(Block.vanishAnimCurve.Evaluate(num - eInfo.timer), 0.001f, 1f);
				this.goT.localScale = new Vector3(num2, num2, num2);
			}
			return TileResultCode.Delayed;
		}

		// Token: 0x0600038E RID: 910 RVA: 0x00020744 File Offset: 0x0001EB44
		public virtual TileResultCode Vanish(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.go.GetComponent<Collider>().enabled = false;
			if (this.goT.parent != null && this.goT.parent.GetComponent<Rigidbody>() != null)
			{
				this.goT.parent.GetComponent<Rigidbody>().isKinematic = true;
			}
			float num = 0.375f;
			if (!this.vanished)
			{
				this.vanished = true;
				CollisionManager.WakeUpObjectsRestingOn(this);
			}
			if (eInfo.timer >= num)
			{
				this.go.GetComponent<Renderer>().enabled = false;
				if (this.goShadow != null)
				{
					this.goShadow.GetComponent<Renderer>().enabled = false;
				}
				this.go.SetActive(false);
				return TileResultCode.True;
			}
			this.GetVanishAnimCurve(num);
			float num2 = Block.vanishAnimCurve.Evaluate(eInfo.timer);
			this.goT.localScale = new Vector3(num2, num2, num2);
			return TileResultCode.Delayed;
		}

		// Token: 0x0600038F RID: 911 RVA: 0x00020844 File Offset: 0x0001EC44
		private void SetKinematicIfSingletonOrLegs(bool k)
		{
			this.UpdateConnectedCache();
			List<Block> list = Block.connectedCache[this];
			bool flag = TreasureHandler.IsPartOfTreasureModel(this);
			BlockWalkable blockWalkable = this as BlockWalkable;
			if (!this.didFix && (list.Count == 1 || (blockWalkable != null && !blockWalkable.unmoving)) && this.chunk.go != null)
			{
				Rigidbody rb = this.chunk.rb;
				if (rb != null && !flag && this.chunk.blocks.Count > 0 && !this.chunk.IsFrozen())
				{
					rb.isKinematic = k;
					if (!k)
					{
						rb.WakeUp();
					}
				}
			}
			if (!k && !flag)
			{
				foreach (Block block in list)
				{
					BlockWalkable blockWalkable2 = block as BlockWalkable;
					if (block != this && blockWalkable2 != null && !blockWalkable2.unmoving && block.vanished)
					{
						return;
					}
				}
				HashSet<Chunk> hashSet = Block.connectedChunks[this];
				foreach (Chunk chunk in hashSet)
				{
					if (chunk.blocks.Count > 0)
					{
						bool flag2 = false;
						int num = 0;
						while (num < chunk.blocks.Count && !flag2)
						{
							flag2 |= chunk.blocks[num].didFix;
							num++;
						}
						if (!flag2 && chunk.go != null)
						{
							Rigidbody rb2 = chunk.rb;
							if (rb2 != null && rb2.isKinematic)
							{
								rb2.isKinematic = false;
								rb2.WakeUp();
							}
						}
					}
				}
			}
		}

		// Token: 0x06000390 RID: 912 RVA: 0x00020A7C File Offset: 0x0001EE7C
		public virtual TileResultCode AppearBlock(bool animate, float timer)
		{
			float num = (!animate) ? 0f : 0.375f;
			if (timer == 0f)
			{
				if (Block.vanishingOrAppearingBlocks.Contains(this))
				{
					return TileResultCode.True;
				}
				if (!this.vanished)
				{
					Block.vanishingOrAppearingBlocks.Remove(this);
					return TileResultCode.True;
				}
				Block.vanishingOrAppearingBlocks.Add(this);
				if (this.go.GetComponent<Collider>() != null)
				{
					this.go.GetComponent<Collider>().enabled = true;
					if (!TreasureHandler.IsPartOfTreasureModel(this) && !this.ColliderIsTriggerInPlayMode())
					{
						this.go.GetComponent<Collider>().isTrigger = false;
					}
				}
				if (this.goShadow != null)
				{
					this.goShadow.GetComponent<Renderer>().enabled = true;
				}
				this.go.SetActive(true);
				BlockAnimatedCharacter blockAnimatedCharacter = this as BlockAnimatedCharacter;
				if (blockAnimatedCharacter != null)
				{
					blockAnimatedCharacter.stateHandler.Appearing();
				}
			}
			if (timer >= num)
			{
				this.goT.localScale = Vector3.one;
				this.vanished = false;
				this.SetKinematicIfSingletonOrLegs(false);
				this.Appeared();
				if (Block.f__mg_cache8 == null)
				{
					Block.f__mg_cache8 = new Action<Block>(Block.BlockModelBlockAppeared);
				}
				this.DoWithNonVanishingOrAppearingModelBlocks(Block.f__mg_cache8);
				Block.vanishingOrAppearingBlocks.Remove(this);
				if (!this.isTreasure)
				{
					this.RestoreMeshColliderInfo();
				}
				return TileResultCode.True;
			}
			this.GetVanishAnimCurve(num);
			float num2 = Mathf.Clamp(Block.vanishAnimCurve.Evaluate(num - timer), 0.001f, 1f);
			this.goT.localScale = new Vector3(num2, num2, num2);
			this.Appearing(num2);
			List<Block> list = Block.connectedCache[this];
			for (int i = 0; i < list.Count; i++)
			{
				Block block = list[i];
				if (!Block.vanishingOrAppearingBlocks.Contains(block))
				{
					block.ModelBlockAppearing(num2);
				}
			}
			return TileResultCode.Delayed;
		}

		// Token: 0x06000391 RID: 913 RVA: 0x00020C6C File Offset: 0x0001F06C
		private static void BlockModelBlockAppeared(Block b)
		{
			b.ModelBlockAppeared();
		}

		// Token: 0x06000392 RID: 914 RVA: 0x00020C74 File Offset: 0x0001F074
		public virtual TileResultCode AppearBlock(ScriptRowExecutionInfo eInfo, object[] args)
		{
			bool animate = this.CanAnimateScale() && Util.GetIntBooleanArg(args, 0, false);
			return this.AppearBlock(animate, eInfo.timer);
		}

		// Token: 0x06000393 RID: 915 RVA: 0x00020CA5 File Offset: 0x0001F0A5
		protected virtual void RestoreMeshColliderInfo()
		{
			if (this.meshColliderInfo != null)
			{
				this.meshColliderInfo.Restore();
				this.meshColliderInfo = null;
			}
		}

		// Token: 0x06000394 RID: 916 RVA: 0x00020CC4 File Offset: 0x0001F0C4
		protected void DoWithNonVanishingOrAppearingModelBlocks(Action<Block> action)
		{
			this.UpdateConnectedCache();
			List<Block> list = Block.connectedCache[this];
			for (int i = 0; i < list.Count; i++)
			{
				Block block = list[i];
				if (!Block.vanishingOrAppearingBlocks.Contains(block))
				{
					action(block);
				}
			}
		}

		// Token: 0x06000395 RID: 917 RVA: 0x00020D1A File Offset: 0x0001F11A
		protected virtual bool CanAnimateScale()
		{
			return true;
		}

		// Token: 0x06000396 RID: 918 RVA: 0x00020D20 File Offset: 0x0001F120
		public virtual TileResultCode VanishBlock(bool animate, float timer)
		{
			if (timer == 0f)
			{
				if (Block.vanishingOrAppearingBlocks.Contains(this))
				{
					return TileResultCode.True;
				}
				if (this.vanished)
				{
					Block.vanishingOrAppearingBlocks.Remove(this);
					return TileResultCode.True;
				}
				Block.vanishingOrAppearingBlocks.Add(this);
				Collider component = this.go.GetComponent<Collider>();
				if (component != null)
				{
					component.enabled = false;
					if (component is MeshCollider)
					{
						MeshCollider meshCollider = (MeshCollider)component;
						this.ReplaceMeshCollider(meshCollider);
						component.isTrigger = meshCollider.convex;
					}
					else
					{
						component.isTrigger = true;
					}
				}
				this.SetKinematicIfSingletonOrLegs(true);
			}
			float num = (!animate) ? 0f : 0.375f;
			if (timer >= num)
			{
				if (!this.vanished)
				{
					this.vanished = true;
					CollisionManager.WakeUpObjectsRestingOn(this);
				}
				if (this.goShadow != null)
				{
					this.goShadow.GetComponent<Renderer>().enabled = false;
				}
				this.Vanished();
				this.DoWithNonVanishingOrAppearingModelBlocks(delegate(Block b)
				{
					b.ModelBlockVanished();
				});
				Block.vanishingOrAppearingBlocks.Remove(this);
				return TileResultCode.True;
			}
			if (this.CanAnimateScale())
			{
				this.GetVanishAnimCurve(num);
				float scale = Block.vanishAnimCurve.Evaluate(timer);
				this.Vanishing(scale);
				this.DoWithNonVanishingOrAppearingModelBlocks(delegate(Block b)
				{
					b.ModelBlockVanishing(scale);
				});
			}
			return TileResultCode.Delayed;
		}

		// Token: 0x06000397 RID: 919 RVA: 0x00020EA0 File Offset: 0x0001F2A0
		public virtual TileResultCode VanishBlock(ScriptRowExecutionInfo eInfo, object[] args)
		{
			bool animate = this.CanAnimateScale() && Util.GetIntBooleanArg(args, 0, false);
			return this.VanishBlock(animate, eInfo.timer);
		}

		// Token: 0x06000398 RID: 920 RVA: 0x00020ED1 File Offset: 0x0001F2D1
		protected virtual void ModelBlockAppearing(float scale)
		{
		}

		// Token: 0x06000399 RID: 921 RVA: 0x00020ED3 File Offset: 0x0001F2D3
		protected virtual void ModelBlockVanishing(float scale)
		{
		}

		// Token: 0x0600039A RID: 922 RVA: 0x00020ED5 File Offset: 0x0001F2D5
		protected virtual void ModelBlockAppeared()
		{
		}

		// Token: 0x0600039B RID: 923 RVA: 0x00020ED7 File Offset: 0x0001F2D7
		protected virtual void ModelBlockVanished()
		{
		}

		// Token: 0x0600039C RID: 924 RVA: 0x00020ED9 File Offset: 0x0001F2D9
		protected virtual void Appearing(float scale)
		{
			if (this.go != null && this.CanAnimateScale())
			{
				this.goT.localScale = Vector3.one * scale;
				this.AdjustScaleForHierarchy();
			}
		}

		// Token: 0x0600039D RID: 925 RVA: 0x00020F14 File Offset: 0x0001F314
		public virtual void Appeared()
		{
			if (this.go != null && this.CanAnimateScale())
			{
				this.goT.localScale = Vector3.one;
				this.AdjustScaleForHierarchy();
			}
			Rigidbody rb = this.chunk.rb;
			if (rb != null)
			{
				BlockWater worldOceanBlock = Blocksworld.worldOceanBlock;
				if (worldOceanBlock != null && !worldOceanBlock.SimulatesBlock(this))
				{
					worldOceanBlock.AddBlockToSimulation(this);
				}
			}
		}

		// Token: 0x0600039E RID: 926 RVA: 0x00020F8A File Offset: 0x0001F38A
		protected virtual void Vanishing(float scale)
		{
			if (this.go != null && this.CanAnimateScale())
			{
				this.goT.localScale = Vector3.one * scale;
				this.AdjustScaleForHierarchy();
			}
		}

		// Token: 0x0600039F RID: 927 RVA: 0x00020FC4 File Offset: 0x0001F3C4
		public virtual void Vanished()
		{
			this.go.SetActive(false);
		}

		// Token: 0x060003A0 RID: 928 RVA: 0x00020FD4 File Offset: 0x0001F3D4
		private void EnableAndShowBlocks(List<Block> blocks)
		{
			bool flag = blocks.Count > 0 && TreasureHandler.IsPartOfTreasureModel(blocks[0]);
			foreach (Block block in blocks)
			{
				GameObject gameObject = block.go;
				if (gameObject.GetComponent<Collider>() != null)
				{
					gameObject.GetComponent<Collider>().enabled = true;
					if (!block.ColliderIsTriggerInPlayMode() && !flag && !block.ColliderIsTriggerInPlayMode())
					{
						gameObject.GetComponent<Collider>().isTrigger = false;
					}
				}
				if (block.goShadow != null && block.VisibleInPlayMode())
				{
					block.goShadow.GetComponent<Renderer>().enabled = true;
				}
				gameObject.SetActive(true);
				if (!flag && block.vanished && !block.isTerrain)
				{
					CollisionManager.SetGhostBlockMode(block, CollisionManager.GhostBlockMode.Propagate);
				}
			}
		}

		// Token: 0x060003A1 RID: 929 RVA: 0x000210E4 File Offset: 0x0001F4E4
		private void WakeUpNonFrozenBlocksAppear(List<Block> connected, HashSet<Chunk> chunks)
		{
			foreach (Chunk chunk in chunks)
			{
				bool flag = chunk.blocks.Count > 0 && chunk.IsFrozen();
				if (!(chunk.go == null))
				{
					Rigidbody[] componentsInChildren = chunk.go.GetComponentsInChildren<Rigidbody>(true);
					if (!flag)
					{
						foreach (Rigidbody rigidbody in componentsInChildren)
						{
							rigidbody.isKinematic = false;
							rigidbody.WakeUp();
						}
					}
				}
			}
			foreach (Block block in connected)
			{
				block.vanished = false;
				block.Appeared();
			}
		}

		// Token: 0x060003A2 RID: 930 RVA: 0x000211FC File Offset: 0x0001F5FC
		public virtual TileResultCode AppearModel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			bool animate = Util.GetIntBooleanArg(args, 0, true);
			this.UpdateConnectedCache();
			List<Block> list = Block.connectedCache[this];
			HashSet<Chunk> hashSet = Block.connectedChunks[this];
			float num = (!animate) ? 0f : 0.375f;
			if (eInfo.timer == 0f)
			{
				if (Block.vanishingOrAppearingBlocks.Overlaps(list))
				{
					return TileResultCode.True;
				}
				Block.vanishingOrAppearingBlocks.UnionWith(list);
				this.EnableAndShowBlocks(list);
			}
			if (eInfo.timer >= num)
			{
				this.WakeUpNonFrozenBlocksAppear(list, hashSet);
				CollisionManager.GhostBlockMode modelGhostMode = (!list.Exists(delegate(Block b)
				{
					CollisionManager.GhostBlockInfo ghostBlockInfo = CollisionManager.GetGhostBlockInfo(b);
					return ghostBlockInfo != null && ghostBlockInfo.didPropagate;
				})) ? CollisionManager.GhostBlockMode.NotGhost : CollisionManager.GhostBlockMode.NoPropagate;
				BlockWater ocean = Blocksworld.worldOceanBlock;
				list.ForEach(delegate(Block b)
				{
					CollisionManager.GhostBlockInfo ghostBlockInfo = CollisionManager.GetGhostBlockInfo(b);
					if (ghostBlockInfo != null)
					{
						CollisionManager.SetGhostBlockMode(b, modelGhostMode);
					}
					if (!b.isTreasure)
					{
						b.RestoreMeshColliderInfo();
					}
					if (!animate)
					{
						b.Appearing(1f);
					}
					if (ocean != null && !b.isTreasure)
					{
						ocean.AddBlockToSimulation(b);
					}
				});
				Block.vanishingOrAppearingBlocks.ExceptWith(list);
				return TileResultCode.True;
			}
			this.GetVanishAnimCurve(num);
			float scale = Mathf.Clamp(Block.vanishAnimCurve.Evaluate(num - eInfo.timer), 0.001f, 5f);
			foreach (Chunk chunk in hashSet)
			{
				for (int i = 0; i < chunk.blocks.Count; i++)
				{
					Block block = chunk.blocks[i];
					if (block.vanished)
					{
						block.Appearing(scale);
					}
				}
			}
			return TileResultCode.Delayed;
		}

		// Token: 0x060003A3 RID: 931 RVA: 0x000213C8 File Offset: 0x0001F7C8
		public virtual TileResultCode VanishModel(float timer, bool animate = true, bool forever = false, float delayPerBlock = 0f)
		{
			this.UpdateConnectedCache();
			List<Block> list = Block.connectedCache[this];
			HashSet<Chunk> hashSet = Block.connectedChunks[this];
			float num = 0.375f;
			this.GetVanishAnimCurve(num);
			bool flag = timer == 0f;
			if (flag)
			{
				bool flag2 = true;
				for (int i = 0; i < list.Count; i++)
				{
					Block block = list[i];
					if (!block.vanished || (forever && !BWSceneManager.playBlocksRemoved.Contains(block)))
					{
						flag2 = false;
						break;
					}
				}
				if (flag2)
				{
					return TileResultCode.True;
				}
				if (Block.vanishingOrAppearingBlocks.Overlaps(list))
				{
					return TileResultCode.True;
				}
				Block.vanishingOrAppearingBlocks.UnionWith(list);
				for (int j = 0; j < list.Count; j++)
				{
					Block block2 = list[j];
					Collider component = block2.go.GetComponent<Collider>();
					if (component != null)
					{
						component.enabled = false;
						if (component is MeshCollider)
						{
							MeshCollider meshCollider = (MeshCollider)component;
							block2.ReplaceMeshCollider(meshCollider);
							component.isTrigger = meshCollider.convex;
						}
						else
						{
							component.isTrigger = true;
						}
					}
				}
				if (!TreasureHandler.IsPartOfTreasureModel(this))
				{
					foreach (Chunk chunk in hashSet)
					{
						if (chunk != null && chunk.go != null)
						{
							Rigidbody rb = chunk.rb;
							if (rb != null)
							{
								rb.isKinematic = true;
							}
						}
					}
				}
				if (!animate && delayPerBlock == 0f)
				{
					for (int k = 0; k < list.Count; k++)
					{
						Block block3 = list[k];
						if (!this.vanished)
						{
							block3.Vanishing(Block.vanishAnimCurve.Evaluate(num));
						}
						block3.SetVanished(forever);
					}
					Block.vanishingOrAppearingBlocks.ExceptWith(list);
					return TileResultCode.True;
				}
			}
			if (delayPerBlock == 0f && (timer >= num || !animate))
			{
				for (int l = 0; l < list.Count; l++)
				{
					Block block4 = list[l];
					block4.SetVanished(forever);
				}
				Block.vanishingOrAppearingBlocks.ExceptWith(list);
				return TileResultCode.True;
			}
			float scale = Block.vanishAnimCurve.Evaluate(timer);
			bool flag3 = delayPerBlock > 0f;
			int num2 = 0;
			foreach (Chunk chunk2 in hashSet)
			{
				if (chunk2 != null)
				{
					for (int m = 0; m < chunk2.blocks.Count; m++)
					{
						Block block5 = chunk2.blocks[m];
						if (delayPerBlock > 0f)
						{
							float num3 = timer - delayPerBlock * (float)num2;
							if (!animate)
							{
								num3 += num;
							}
							if (num3 > 0f)
							{
								if (animate)
								{
									float scale2 = Block.vanishAnimCurve.Evaluate(num3);
									if (!this.vanished)
									{
										block5.Vanishing(scale2);
									}
								}
								if (num3 >= num)
								{
									block5.SetVanished(forever);
								}
								else
								{
									flag3 = false;
								}
							}
							else
							{
								flag3 = false;
							}
						}
						else if (!this.vanished)
						{
							block5.Vanishing(scale);
						}
						num2++;
					}
				}
			}
			if (flag3)
			{
				Block.vanishingOrAppearingBlocks.ExceptWith(list);
				if (forever)
				{
					for (int n = 0; n < list.Count; n++)
					{
						Block block6 = list[n];
						if (!BWSceneManager.playBlocksRemoved.Contains(block6))
						{
							block6.SetVanished(forever);
						}
					}
				}
				return TileResultCode.True;
			}
			return TileResultCode.Delayed;
		}

		// Token: 0x060003A4 RID: 932 RVA: 0x000217D0 File Offset: 0x0001FBD0
		private void SetVanished(bool forever = false)
		{
			this.Vanished();
			if (!this.vanished)
			{
				this.vanished = true;
				CollisionManager.WakeUpObjectsRestingOn(this);
			}
			if (this.goShadow != null)
			{
				this.goShadow.GetComponent<Renderer>().enabled = false;
			}
			if (forever && !TreasureHandler.IsPartOfTreasureModel(this))
			{
				BWSceneManager.RemovePlayBlock(this);
			}
		}

		// Token: 0x060003A5 RID: 933 RVA: 0x00021834 File Offset: 0x0001FC34
		public virtual TileResultCode VanishModel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			bool intBooleanArg = Util.GetIntBooleanArg(args, 0, true);
			if (this.broken)
			{
				if (!this.vanished)
				{
					Blocksworld.AddFixedUpdateCommand(new VanishModelForeverCommand(this, intBooleanArg, 0f));
				}
				return TileResultCode.True;
			}
			return this.VanishModel(eInfo.timer, intBooleanArg, false, 0f);
		}

		// Token: 0x060003A6 RID: 934 RVA: 0x00021888 File Offset: 0x0001FC88
		public virtual TileResultCode VanishModelForever(ScriptRowExecutionInfo eInfo, object[] args)
		{
			bool intBooleanArg = Util.GetIntBooleanArg(args, 0, false);
			float floatArg = Util.GetFloatArg(args, 1, 0f);
			if (!this.vanished)
			{
				Blocksworld.AddFixedUpdateCommand(new VanishModelForeverCommand(this, intBooleanArg, floatArg));
			}
			return TileResultCode.True;
		}

		// Token: 0x060003A7 RID: 935 RVA: 0x000218C4 File Offset: 0x0001FCC4
		public Vector3 GetHierarchyScale()
		{
			Vector3 vector = Vector3.one;
			Transform parent = this.goT.parent;
			while (parent != null)
			{
				vector = Vector3.Scale(vector, parent.localScale);
				parent = parent.parent;
			}
			return vector;
		}

		// Token: 0x060003A8 RID: 936 RVA: 0x0002190C File Offset: 0x0001FD0C
		private void AdjustScaleForHierarchy()
		{
			Vector3 hierarchyScale = this.GetHierarchyScale();
			if (hierarchyScale.x == 0f || hierarchyScale.y == 0f || hierarchyScale.z == 0f)
			{
				return;
			}
			Vector3 a = new Vector3(1f / hierarchyScale.x, 1f / hierarchyScale.y, 1f / hierarchyScale.z);
			Vector3 localScale = this.goT.localScale;
			this.goT.localScale = Vector3.Scale(a, localScale);
		}

		// Token: 0x060003A9 RID: 937 RVA: 0x000219A0 File Offset: 0x0001FDA0
		public TileResultCode Target(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.goT.parent.name = "Target";
			return TileResultCode.True;
		}

		// Token: 0x060003AA RID: 938 RVA: 0x000219B8 File Offset: 0x0001FDB8
		public TileResultCode TutorialCreateBlockHint(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return TileResultCode.True;
		}

		// Token: 0x060003AB RID: 939 RVA: 0x000219BB File Offset: 0x0001FDBB
		public TileResultCode TutorialPaintExistingBlock(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return TileResultCode.True;
		}

		// Token: 0x060003AC RID: 940 RVA: 0x000219BE File Offset: 0x0001FDBE
		public TileResultCode TutorialTextureExistingBlock(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return TileResultCode.True;
		}

		// Token: 0x060003AD RID: 941 RVA: 0x000219C1 File Offset: 0x0001FDC1
		public TileResultCode TutorialRotateExistingBlock(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return TileResultCode.True;
		}

		// Token: 0x060003AE RID: 942 RVA: 0x000219C4 File Offset: 0x0001FDC4
		private TileResultCode Speak(string text, float timer, int direction, bool durational = true)
		{
			if (WinLoseManager.IsFinished())
			{
				return TileResultCode.True;
			}
			if (timer < 0.4f)
			{
				return TileResultCode.Delayed;
			}
			UISpeechBubble uispeechBubble = Blocksworld.UI.SpeechBubble.OnBlockSpeak(this, text, direction);
			if (this is BlockCharacter || this is BlockAnimatedCharacter)
			{
				Blocksworld.worldSessionHadBlocksterSpeaker = true;
			}
			if (!durational || uispeechBubble.ButtonPressed())
			{
				return TileResultCode.True;
			}
			float num = (float)((!uispeechBubble.HasButtons()) ? 4 : 1000000);
			if (timer >= num)
			{
				return TileResultCode.True;
			}
			return TileResultCode.Delayed;
		}

		// Token: 0x060003AF RID: 943 RVA: 0x00021A50 File Offset: 0x0001FE50
		public TileResultCode Speak(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string text = (string)args[0];
			return this.Speak(text, eInfo.timer, 0, true);
		}

		// Token: 0x060003B0 RID: 944 RVA: 0x00021A78 File Offset: 0x0001FE78
		public TileResultCode SpeakNonDurational(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string text = (string)args[0];
			int direction = (args.Length <= 1) ? 0 : ((int)args[1]);
			return this.Speak(text, eInfo.timer, direction, false);
		}

		// Token: 0x060003B1 RID: 945 RVA: 0x00021AB8 File Offset: 0x0001FEB8
		public TileResultCode ShowTextWindow(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string text = (string)args[0];
			Vector3 v = (Vector3)args[1];
			float width = (float)args[2];
			string stringArg = Util.GetStringArg(args, 3, string.Empty);
			Blocksworld.UI.ShowTextWindow(this, text, v, width, stringArg);
			return TileResultCode.True;
		}

		// Token: 0x060003B2 RID: 946 RVA: 0x00021B04 File Offset: 0x0001FF04
		public TileResultCode GameOver(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (WinLoseManager.winning || WinLoseManager.losing || (WinLoseManager.playedStinger && eInfo.timer == 0f))
			{
				return TileResultCode.Delayed;
			}
			string stringArgSafe = Util.GetStringArgSafe(args, 2, "SFX win_level_stinger_01");
			UISpeechBubble uispeechBubble = Blocksworld.UI.GetBlockTextWindow(this);
			if (uispeechBubble == WinLoseManager.messageWindow && !Blocksworld.winIsWaiting)
			{
				string text = (string)args[0];
				uispeechBubble = this.ShowCentralizedTextWindow(text, "Restart;Exit|GameDone", 400f, 100f);
				WinLoseManager.ending = true;
				WinLoseManager.messageWindow = uispeechBubble;
			}
			ParticleSystem particleSystem = WinLoseManager.GetParticleSystem();
			Transform transform = Blocksworld.rewardCamera.transform;
			float d = 5f;
			if (eInfo.timer == 0f)
			{
				Blocksworld.musicPlayer.Stop();
				particleSystem.transform.position = transform.position + transform.forward * d;
				WinLoseManager.playedStinger = true;
				Sound.PlayOneShotSound(stringArgSafe, 1f);
			}
			if (eInfo.timer < 0.3f)
			{
				int num = Mathf.RoundToInt(100f / (30f * eInfo.timer + 1f));
				for (int i = 0; i < num; i++)
				{
					Vector3 velocity = UnityEngine.Random.onUnitSphere * (UnityEngine.Random.value * 2f + 1f);
					Vector3 position = transform.up * (UnityEngine.Random.value - 0.5f) * d * 3f + transform.right * 2f * (UnityEngine.Random.value - 0.5f) * d * 2f;
					float num2 = UnityEngine.Random.value * 0.5f + 0.25f;
					float lifetime = 1.7f + UnityEngine.Random.value * 0.5f;
					Color c = (UnityEngine.Random.value <= 0.5f) ? Color.yellow : Color.white;
					particleSystem.Emit(position, velocity, num2, lifetime, c);
				}
			}
			if (uispeechBubble != null && uispeechBubble.ButtonPressed())
			{
				WinLoseManager.buttonPressed = true;
			}
			return TileResultCode.Delayed;
		}

		// Token: 0x060003B3 RID: 947 RVA: 0x00021D48 File Offset: 0x00020148
		public TileResultCode GameWin(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (WinLoseManager.losing || WinLoseManager.ending || (WinLoseManager.playedStinger && eInfo.timer == 0f))
			{
				return TileResultCode.Delayed;
			}
			Blocksworld.HandleWin();
			float floatArg = Util.GetFloatArg(args, 1, 0f);
			bool flag = floatArg == 0f && WorldSession.canShowLeaderboard();
			float num = (floatArg >= 4f) ? floatArg : 0f;
			string stringArgSafe = Util.GetStringArgSafe(args, 2, "SFX win_level_stinger_01");
			bool flag2 = num > 0f;
			UISpeechBubble uispeechBubble = Blocksworld.UI.GetBlockTextWindow(this);
			if (uispeechBubble == WinLoseManager.messageWindow && !Blocksworld.winIsWaiting)
			{
				string str = (!flag) ? "Exit" : "Next";
				string buttons = "Restart;" + str + "|GameDone";
				string text = (string)args[0];
				if (flag2)
				{
					string[] array = ((string)args[0]).Split(new char[]
					{
						';'
					});
					if (array.Length > 1 && !string.IsNullOrEmpty(array[1]))
					{
						buttons = array[1] + "|Exit";
						text = array[0];
					}
					else
					{
						buttons = "Exit";
					}
				}
				if (flag && Block.leaderboardData != null && !Block.leaderboardData.temporarilyDisableTimer)
				{
					Block.leaderboardData.FinishLeaderboard(text);
				}
				else
				{
					uispeechBubble = this.ShowCentralizedTextWindow(text, buttons, 400f, 100f);
				}
				WinLoseManager.winning = true;
				WinLoseManager.messageWindow = uispeechBubble;
			}
			ParticleSystem particleSystem = WinLoseManager.GetParticleSystem();
			Transform transform = Blocksworld.rewardCamera.transform;
			float d = 5f;
			if (eInfo.timer == 0f)
			{
				Blocksworld.musicPlayer.Stop();
				particleSystem.transform.position = transform.position + transform.forward * d;
				WinLoseManager.playedStinger = true;
				Sound.PlayOneShotSound(stringArgSafe, 1f);
			}
			if (eInfo.timer < 0.3f)
			{
				int num2 = Mathf.RoundToInt(100f / (30f * eInfo.timer + 1f));
				for (int i = 0; i < num2; i++)
				{
					Vector3 velocity = UnityEngine.Random.onUnitSphere * (UnityEngine.Random.value * 2f + 1f);
					Vector3 position = transform.up * (UnityEngine.Random.value - 0.5f) * d * 3f + transform.right * 2f * (UnityEngine.Random.value - 0.5f) * d * 2f;
					float num3 = UnityEngine.Random.value * 0.5f + 0.25f;
					float lifetime = 1.7f + UnityEngine.Random.value * 0.5f;
					Color c = (UnityEngine.Random.value <= 0.5f) ? Color.yellow : Color.white;
					particleSystem.Emit(position, velocity, num3, lifetime, c);
				}
			}
			if (uispeechBubble != null && uispeechBubble.ButtonPressed())
			{
				WinLoseManager.buttonPressed = true;
			}
			if (flag2)
			{
				return (!WinLoseManager.buttonPressed) ? TileResultCode.Delayed : this.Win();
			}
			return TileResultCode.Delayed;
		}

		// Token: 0x060003B4 RID: 948 RVA: 0x000220B4 File Offset: 0x000204B4
		public TileResultCode GameLose(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (WinLoseManager.winning || WinLoseManager.ending || (WinLoseManager.playedStinger && eInfo.timer == 0f))
			{
				return TileResultCode.Delayed;
			}
			Blocksworld.HandleLose();
			if (eInfo.timer == 0f)
			{
				Blocksworld.musicPlayer.Stop();
				string stringArgSafe = Util.GetStringArgSafe(args, 2, "SFX lose_level_stinger_01");
				Sound.PlayOneShotSound(stringArgSafe, 1f);
				WinLoseManager.playedStinger = true;
			}
			UISpeechBubble uispeechBubble = Blocksworld.UI.GetBlockTextWindow(this);
			if (uispeechBubble == WinLoseManager.messageWindow)
			{
				if (WorldSession.canShowLeaderboard() && Block.leaderboardData != null && !Block.leaderboardData.temporarilyDisableTimer)
				{
					Block.leaderboardData.LoseCondition();
				}
				uispeechBubble = this.ShowCentralizedTextWindow((string)args[0], "Restart", 400f, 100f);
				WinLoseManager.losing = true;
				WinLoseManager.messageWindow = uispeechBubble;
			}
			return TileResultCode.Delayed;
		}

		// Token: 0x060003B5 RID: 949 RVA: 0x000221A8 File Offset: 0x000205A8
		public virtual TileResultCode RegisterTag(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string tagName = (string)args[0];
			TagManager.RegisterBlockTag(this, tagName);
			return TileResultCode.True;
		}

		// Token: 0x060003B6 RID: 950 RVA: 0x000221C8 File Offset: 0x000205C8
		private UISpeechBubble ShowCentralizedTextWindow(string text, string buttons, float width, float y)
		{
			float x = (float)NormalizedScreen.width * 0.5f - width * 0.5f;
			Vector3 v = new Vector3(x, y, 3f);
			return Blocksworld.UI.ShowTextWindow(this, text, v, width, buttons);
		}

		// Token: 0x060003B7 RID: 951 RVA: 0x0002220D File Offset: 0x0002060D
		public TileResultCode IgnoreAction(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return TileResultCode.True;
		}

		// Token: 0x060003B8 RID: 952 RVA: 0x00022210 File Offset: 0x00020610
		public TileResultCode IgnoreSensor(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return TileResultCode.True;
		}

		// Token: 0x060003B9 RID: 953 RVA: 0x00022213 File Offset: 0x00020613
		public TileResultCode TestAction(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return TileResultCode.True;
		}

		// Token: 0x060003BA RID: 954 RVA: 0x00022218 File Offset: 0x00020618
		public TileResultCode TestSensor(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return TileResultCode.False;
		}

		// Token: 0x060003BB RID: 955 RVA: 0x00022228 File Offset: 0x00020628
		public TileResultCode IsTestObjectiveDone(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return TileResultCode.False;
		}

		// Token: 0x060003BC RID: 956 RVA: 0x00022238 File Offset: 0x00020638
		public TileResultCode SetTestObjectiveDone(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return TileResultCode.True;
		}

		// Token: 0x060003BD RID: 957 RVA: 0x0002223B File Offset: 0x0002063B
		private TileResultCode Win()
		{
			if (Blocksworld.winIsWaiting)
			{
				return TileResultCode.Delayed;
			}
			if (Blocksworld.hasWon)
			{
				Blocksworld.hasWon = false;
				WorldSession.Quit();
				return TileResultCode.True;
			}
			WorldSession.current.OnWinGame();
			Blocksworld.winIsWaiting = true;
			Blocksworld.hasWon = false;
			return TileResultCode.Delayed;
		}

		// Token: 0x060003BE RID: 958 RVA: 0x00022277 File Offset: 0x00020677
		public TileResultCode Win(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return this.Win();
		}

		// Token: 0x060003BF RID: 959 RVA: 0x0002227F File Offset: 0x0002067F
		internal static void ResetTiltOrientation()
		{
			TiltManager.Instance.ResetOrientation();
		}

		// Token: 0x060003C0 RID: 960 RVA: 0x0002228C File Offset: 0x0002068C
		private static float GetTilt(bool xDirection, float maxAngle)
		{
			float num;
			if (xDirection)
			{
				num = TiltManager.Instance.GetTiltTwist();
			}
			else
			{
				num = TiltManager.Instance.GetRelativeGravityVector().y;
			}
			return Mathf.Clamp(num * 90f / Mathf.Max(10f, maxAngle), -1f, 1f);
		}

		// Token: 0x060003C1 RID: 961 RVA: 0x000222E4 File Offset: 0x000206E4
		public TileResultCode TiltMoverControl(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (!TiltManager.Instance.IsMonitoring())
			{
				return TileResultCode.Delayed;
			}
			float floatArg = Util.GetFloatArg(args, 0, 1f);
			bool flag = Util.GetIntArg(args, 1, 0) > 0;
			Vector3 vector = floatArg * TiltManager.Instance.GetGravityVector();
			Vector3 vector2 = floatArg * TiltManager.Instance.GetRelativeGravityVector();
			float zTilt = floatArg * TiltManager.Instance.GetTiltTwist();
			Blocksworld.UI.Controls.UpdateTiltPrompt();
			if (flag)
			{
				this.HandleTiltMover(vector.x, vector.y, zTilt);
			}
			else
			{
				this.HandleTiltMover(vector2.x, vector2.y, zTilt);
			}
			return TileResultCode.True;
		}

		// Token: 0x060003C2 RID: 962 RVA: 0x00022392 File Offset: 0x00020792
		protected virtual void HandleTiltMover(float xTilt, float yTilt, float zTilt)
		{
		}

		// Token: 0x060003C3 RID: 963 RVA: 0x00022394 File Offset: 0x00020794
		public TileResultCode IsTiltedFrontBack(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float f = (float)args[0];
			float floatArg = Util.GetFloatArg(args, 1, 30f);
			float tilt = Block.GetTilt(false, floatArg);
			Blocksworld.UI.Controls.UpdateTiltPrompt();
			bool flag = Mathf.Abs(tilt) > 0.01f && Mathf.Sign(f) == Mathf.Sign(tilt);
			if (flag)
			{
				eInfo.floatArg = Mathf.Min(eInfo.floatArg, Mathf.Abs(tilt));
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x060003C4 RID: 964 RVA: 0x00022410 File Offset: 0x00020810
		public TileResultCode TiltCamera(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (!TiltManager.Instance.IsMonitoring())
			{
				return TileResultCode.Delayed;
			}
			Quaternion currentAttitude = TiltManager.Instance.GetCurrentAttitude();
			Blocksworld.blocksworldCamera.SetScreenTiltRotation(currentAttitude);
			Blocksworld.UI.Controls.UpdateTiltPrompt();
			return TileResultCode.True;
		}

		// Token: 0x060003C5 RID: 965 RVA: 0x00022454 File Offset: 0x00020854
		public TileResultCode IsTiltedLeftRight(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float f = (float)args[0];
			float floatArg = Util.GetFloatArg(args, 1, 30f);
			float tilt = Block.GetTilt(true, floatArg);
			Blocksworld.UI.Controls.UpdateTiltPrompt();
			bool flag = Mathf.Abs(tilt) > 0.01f && Mathf.Sign(f) == Mathf.Sign(tilt);
			if (flag)
			{
				eInfo.floatArg = Mathf.Min(eInfo.floatArg, Mathf.Abs(tilt));
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x060003C6 RID: 966 RVA: 0x000224D0 File Offset: 0x000208D0
		public TileResultCode IsDPadHorizontal(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float num = (float)args[0];
			string key = (args.Length <= 1) ? "L" : ((string)args[1]);
			Blocksworld.UI.Controls.EnableDPad(key, (num <= 0f) ? MoverDirectionMask.LEFT : MoverDirectionMask.RIGHT);
			if (Blocksworld.UI.Controls.IsDPadActive(key))
			{
				Vector2 normalizedDPadOffset = Blocksworld.UI.Controls.GetNormalizedDPadOffset(key);
				if (Mathf.Sign(normalizedDPadOffset.x) == num && Mathf.Abs(normalizedDPadOffset.x) > 0.001f)
				{
					eInfo.floatArg = Mathf.Min(num * normalizedDPadOffset.x, eInfo.floatArg);
					this.wasHorizPositive = (num > 0f);
					this.wasHorizNegative = (num < 0f);
					return TileResultCode.True;
				}
			}
			eInfo.floatArg = 0f;
			if (this.wasHorizPositive && num > 0f)
			{
				this.wasHorizPositive = false;
				return TileResultCode.True;
			}
			if (this.wasHorizNegative && num < 0f)
			{
				this.wasHorizNegative = false;
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x060003C7 RID: 967 RVA: 0x000225F4 File Offset: 0x000209F4
		public TileResultCode IsDPadVertical(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float num = (float)args[0];
			string key = (args.Length <= 1) ? "L" : ((string)args[1]);
			Blocksworld.UI.Controls.EnableDPad(key, (num <= 0f) ? MoverDirectionMask.DOWN : MoverDirectionMask.UP);
			if (Blocksworld.UI.Controls.IsDPadActive(key))
			{
				Vector2 normalizedDPadOffset = Blocksworld.UI.Controls.GetNormalizedDPadOffset(key);
				if (Mathf.Sign(normalizedDPadOffset.y) == num && Mathf.Abs(normalizedDPadOffset.y) > 0.001f)
				{
					eInfo.floatArg = Mathf.Min(eInfo.floatArg, num * normalizedDPadOffset.y);
					this.wasVertPositive = (num > 0f);
					this.wasVertNegative = (num < 0f);
					return TileResultCode.True;
				}
			}
			eInfo.floatArg = 0f;
			if (this.wasVertPositive && num > 0f)
			{
				this.wasVertPositive = false;
				return TileResultCode.True;
			}
			if (this.wasVertNegative && num < 0f)
			{
				this.wasVertNegative = false;
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x060003C8 RID: 968 RVA: 0x00022718 File Offset: 0x00020B18
		public TileResultCode IsDPadMoved(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string key = (args.Length <= 0) ? "L" : ((string)args[0]);
			Blocksworld.UI.Controls.EnableDPad(key, MoverDirectionMask.ALL);
			if (Blocksworld.UI.Controls.IsDPadActive(key))
			{
				float magnitude = Blocksworld.UI.Controls.GetNormalizedDPadOffset(key).magnitude;
				if (magnitude > 0.001f)
				{
					eInfo.floatArg = Mathf.Min(eInfo.floatArg, magnitude);
					return TileResultCode.True;
				}
			}
			eInfo.floatArg = 0f;
			return TileResultCode.False;
		}

		// Token: 0x060003C9 RID: 969 RVA: 0x000227AC File Offset: 0x00020BAC
		public TileResultCode IsReceivingButtonInput(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string text = (string)args[0];
			Blocksworld.UI.Controls.AddControlFromBlock(text, this);
			if (Blocksworld.UI.Controls.IsControlPressed(text))
			{
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x060003CA RID: 970 RVA: 0x000227EC File Offset: 0x00020BEC
		public TileResultCode IsSendingSignal(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int num = (int)args[0];
			float floatArg = Util.GetFloatArg(args, 1, 1f);
			bool flag = Blocksworld.sending[num];
			eInfo.floatArg = floatArg * Mathf.Min(eInfo.floatArg, Blocksworld.sendingValues[num]);
			return (!flag) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x060003CB RID: 971 RVA: 0x00022840 File Offset: 0x00020C40
		public TileResultCode SendSignal(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int num = (int)args[0];
			float floatArg = Util.GetFloatArg(args, 1, 1f);
			Blocksworld.sending[num] = true;
			Blocksworld.sendingValues[num] = Mathf.Max(eInfo.floatArg * floatArg, Blocksworld.sendingValues[num]);
			return TileResultCode.True;
		}

		// Token: 0x060003CC RID: 972 RVA: 0x00022888 File Offset: 0x00020C88
		public TileResultCode IsSendingSignalModel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int index = (int)args[0];
			float floatArg = Util.GetFloatArg(args, 1, 1f);
			float b;
			bool flag = ModelSignals.IsSendingSignal(this, index, out b);
			eInfo.floatArg = floatArg * Mathf.Min(eInfo.floatArg, b);
			return (!flag) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x060003CD RID: 973 RVA: 0x000228D8 File Offset: 0x00020CD8
		public TileResultCode SendSignalModel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int index = (int)args[0];
			float floatArg = Util.GetFloatArg(args, 1, 1f);
			ModelSignals.SendSignal(this, index, floatArg);
			return TileResultCode.True;
		}

		// Token: 0x060003CE RID: 974 RVA: 0x00022904 File Offset: 0x00020D04
		public TileResultCode IsSendingCustomSignal(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			float floatArg = Util.GetFloatArg(args, 1, 1f);
			float b;
			if (Blocksworld.sendingCustom.TryGetValue(stringArg, out b))
			{
				eInfo.floatArg = floatArg * Mathf.Min(eInfo.floatArg, b);
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x060003CF RID: 975 RVA: 0x00022954 File Offset: 0x00020D54
		public TileResultCode SendCustomSignal(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			float floatArg = Util.GetFloatArg(args, 1, 1f);
			float b;
			if (!Blocksworld.sendingCustom.TryGetValue(stringArg, out b))
			{
				b = 1f;
			}
			Blocksworld.sendingCustom[stringArg] = Mathf.Max(eInfo.floatArg * floatArg, b);
			return TileResultCode.True;
		}

		// Token: 0x060003D0 RID: 976 RVA: 0x000229B0 File Offset: 0x00020DB0
		public TileResultCode IsSendingCustomSignalModel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			float floatArg = Util.GetFloatArg(args, 1, 1f);
			bool flag = ModelSignals.IsSendingCustomSignal(this, eInfo, stringArg, floatArg);
			return (!flag) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x060003D1 RID: 977 RVA: 0x000229F0 File Offset: 0x00020DF0
		public TileResultCode SendCustomSignalModel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			float floatArg = Util.GetFloatArg(args, 1, 1f);
			ModelSignals.SendCustomSignal(this, eInfo, stringArg, floatArg);
			return TileResultCode.True;
		}

		// Token: 0x060003D2 RID: 978 RVA: 0x00022A24 File Offset: 0x00020E24
		public TileResultCode VariableDeclare(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			if (!Blocksworld.customIntVariables.ContainsKey(stringArg))
			{
				int intArg = Util.GetIntArg(args, 1, 0);
				Blocksworld.customIntVariables[stringArg] = intArg;
			}
			return TileResultCode.True;
		}

		// Token: 0x060003D3 RID: 979 RVA: 0x00022A64 File Offset: 0x00020E64
		public TileResultCode VariableAssign(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			if (Blocksworld.customIntVariables.ContainsKey(stringArg))
			{
				int intArg = Util.GetIntArg(args, 1, 0);
				float num = Mathf.Sign((float)intArg);
				int value = (int)(num * Mathf.Floor(eInfo.floatArg * ((float)Mathf.Abs(intArg) + 0.49f)));
				Blocksworld.customIntVariables[stringArg] = value;
			}
			return TileResultCode.True;
		}

		// Token: 0x060003D4 RID: 980 RVA: 0x00022ACC File Offset: 0x00020ECC
		public TileResultCode VariableAdd(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			if (Blocksworld.customIntVariables.ContainsKey(stringArg))
			{
				int intArg = Util.GetIntArg(args, 1, 1);
				Dictionary<string, int> customIntVariables;
				string key;
				(customIntVariables = Blocksworld.customIntVariables)[key = stringArg] = customIntVariables[key] + intArg;
			}
			return TileResultCode.True;
		}

		// Token: 0x060003D5 RID: 981 RVA: 0x00022B18 File Offset: 0x00020F18
		public TileResultCode VariableSubtract(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			if (Blocksworld.customIntVariables.ContainsKey(stringArg))
			{
				int intArg = Util.GetIntArg(args, 1, 1);
				Dictionary<string, int> customIntVariables;
				string key;
				(customIntVariables = Blocksworld.customIntVariables)[key = stringArg] = customIntVariables[key] - intArg;
			}
			return TileResultCode.True;
		}

		// Token: 0x060003D6 RID: 982 RVA: 0x00022B64 File Offset: 0x00020F64
		public TileResultCode VariableMultiply(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			if (Blocksworld.customIntVariables.ContainsKey(stringArg))
			{
				int intArg = Util.GetIntArg(args, 1, 1);
				Dictionary<string, int> customIntVariables;
				string key;
				(customIntVariables = Blocksworld.customIntVariables)[key = stringArg] = customIntVariables[key] * intArg;
			}
			return TileResultCode.True;
		}

		// Token: 0x060003D7 RID: 983 RVA: 0x00022BB0 File Offset: 0x00020FB0
		public TileResultCode VariableDivide(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			if (Blocksworld.customIntVariables.ContainsKey(stringArg))
			{
				int num = Util.GetIntArg(args, 1, 1);
				if (num == 0)
				{
					num = 1;
				}
				Blocksworld.customIntVariables[stringArg] = Blocksworld.customIntVariables[stringArg] / num;
			}
			return TileResultCode.True;
		}

		// Token: 0x060003D8 RID: 984 RVA: 0x00022C04 File Offset: 0x00021004
		public TileResultCode VariableModulus(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			if (Blocksworld.customIntVariables.ContainsKey(stringArg))
			{
				int num = Util.GetIntArg(args, 1, 1);
				if (num == 0)
				{
					num = 1;
				}
				Blocksworld.customIntVariables[stringArg] = Blocksworld.customIntVariables[stringArg] % num;
			}
			return TileResultCode.True;
		}

		// Token: 0x060003D9 RID: 985 RVA: 0x00022C58 File Offset: 0x00021058
		public TileResultCode VariableEquals(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			bool flag = false;
			if (Blocksworld.customIntVariables.ContainsKey(stringArg))
			{
				int intArg = Util.GetIntArg(args, 1, 0);
				flag = (Blocksworld.customIntVariables[stringArg] == intArg);
			}
			return (!flag) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x060003DA RID: 986 RVA: 0x00022CAC File Offset: 0x000210AC
		public TileResultCode VariableNotEquals(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			bool flag = false;
			if (Blocksworld.customIntVariables.ContainsKey(stringArg))
			{
				int intArg = Util.GetIntArg(args, 1, 0);
				flag = (Blocksworld.customIntVariables[stringArg] != intArg);
			}
			return (!flag) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x060003DB RID: 987 RVA: 0x00022D00 File Offset: 0x00021100
		public TileResultCode VariableLessThan(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			bool flag = false;
			if (Blocksworld.customIntVariables.ContainsKey(stringArg))
			{
				int intArg = Util.GetIntArg(args, 1, 0);
				flag = (Blocksworld.customIntVariables[stringArg] < intArg);
			}
			return (!flag) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x060003DC RID: 988 RVA: 0x00022D54 File Offset: 0x00021154
		public TileResultCode VariableMoreThan(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			bool flag = false;
			if (Blocksworld.customIntVariables.ContainsKey(stringArg))
			{
				int intArg = Util.GetIntArg(args, 1, 0);
				flag = (Blocksworld.customIntVariables[stringArg] > intArg);
			}
			return (!flag) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x060003DD RID: 989 RVA: 0x00022DA8 File Offset: 0x000211A8
		public TileResultCode BlockVariableDeclare(ScriptRowExecutionInfo eInfo, object[] args)
		{
			Dictionary<string, int> dictionary;
			if (Blocksworld.blockIntVariables.ContainsKey(this))
			{
				dictionary = Blocksworld.blockIntVariables[this];
			}
			else
			{
				Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
				Blocksworld.blockIntVariables[this] = dictionary2;
				dictionary = dictionary2;
			}
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			if (!dictionary.ContainsKey(stringArg))
			{
				int intArg = Util.GetIntArg(args, 1, 0);
				dictionary[stringArg] = intArg;
			}
			return TileResultCode.True;
		}

		// Token: 0x060003DE RID: 990 RVA: 0x00022E18 File Offset: 0x00021218
		public TileResultCode BlockVariableAssign(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (Blocksworld.blockIntVariables.ContainsKey(this))
			{
				Dictionary<string, int> dictionary = Blocksworld.blockIntVariables[this];
				string stringArg = Util.GetStringArg(args, 0, string.Empty);
				if (dictionary.ContainsKey(stringArg))
				{
					int intArg = Util.GetIntArg(args, 1, 0);
					float num = Mathf.Sign((float)intArg);
					int value = (int)(num * Mathf.Floor(eInfo.floatArg * ((float)Mathf.Abs(intArg) + 0.49f)));
					dictionary[stringArg] = value;
				}
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x060003DF RID: 991 RVA: 0x00022E9C File Offset: 0x0002129C
		public TileResultCode BlockVariableAssignRandom(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (Blocksworld.blockIntVariables.ContainsKey(this))
			{
				Dictionary<string, int> dictionary = Blocksworld.blockIntVariables[this];
				string stringArg = Util.GetStringArg(args, 0, string.Empty);
				int intArg = Util.GetIntArg(args, 1, 0);
				if (dictionary.ContainsKey(stringArg))
				{
					dictionary[stringArg] = UnityEngine.Random.Range(0, intArg);
				}
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x060003E0 RID: 992 RVA: 0x00022F00 File Offset: 0x00021300
		public TileResultCode BlockVariableAssignSpeed(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (Blocksworld.blockIntVariables.ContainsKey(this))
			{
				Dictionary<string, int> dictionary = Blocksworld.blockIntVariables[this];
				string stringArg = Util.GetStringArg(args, 0, string.Empty);
				if (dictionary.ContainsKey(stringArg))
				{
					int value = 0;
					if (this.chunk != null && this.chunk.rb != null)
					{
						value = (int)this.chunk.rb.velocity.magnitude;
					}
					dictionary[stringArg] = value;
				}
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x060003E1 RID: 993 RVA: 0x00022F94 File Offset: 0x00021394
		public TileResultCode BlockVariableAssignAltitude(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (Blocksworld.blockIntVariables.ContainsKey(this))
			{
				Dictionary<string, int> dictionary = Blocksworld.blockIntVariables[this];
				string stringArg = Util.GetStringArg(args, 0, string.Empty);
				if (dictionary.ContainsKey(stringArg))
				{
					int value = (int)this.goT.position.y;
					dictionary[stringArg] = value;
				}
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x060003E2 RID: 994 RVA: 0x00023000 File Offset: 0x00021400
		public TileResultCode BlockVariableAdd(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (Blocksworld.blockIntVariables.ContainsKey(this))
			{
				Dictionary<string, int> dictionary = Blocksworld.blockIntVariables[this];
				string stringArg = Util.GetStringArg(args, 0, string.Empty);
				int intArg = Util.GetIntArg(args, 1, 1);
				if (dictionary.ContainsKey(stringArg))
				{
					Dictionary<string, int> dictionary2;
					string key;
					(dictionary2 = dictionary)[key = stringArg] = dictionary2[key] + intArg;
				}
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x060003E3 RID: 995 RVA: 0x0002306C File Offset: 0x0002146C
		public TileResultCode BlockVariableSubtract(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (Blocksworld.blockIntVariables.ContainsKey(this))
			{
				Dictionary<string, int> dictionary = Blocksworld.blockIntVariables[this];
				string stringArg = Util.GetStringArg(args, 0, string.Empty);
				int intArg = Util.GetIntArg(args, 1, 1);
				if (dictionary.ContainsKey(stringArg))
				{
					Dictionary<string, int> dictionary2;
					string key;
					(dictionary2 = dictionary)[key = stringArg] = dictionary2[key] - intArg;
				}
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x060003E4 RID: 996 RVA: 0x000230D8 File Offset: 0x000214D8
		public TileResultCode BlockVariableMultiply(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (Blocksworld.blockIntVariables.ContainsKey(this))
			{
				Dictionary<string, int> dictionary = Blocksworld.blockIntVariables[this];
				string stringArg = Util.GetStringArg(args, 0, string.Empty);
				int intArg = Util.GetIntArg(args, 1, 1);
				if (dictionary.ContainsKey(stringArg))
				{
					Dictionary<string, int> dictionary2;
					string key;
					(dictionary2 = dictionary)[key = stringArg] = dictionary2[key] * intArg;
				}
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x060003E5 RID: 997 RVA: 0x00023144 File Offset: 0x00021544
		public TileResultCode BlockVariableDivide(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (Blocksworld.blockIntVariables.ContainsKey(this))
			{
				Dictionary<string, int> dictionary = Blocksworld.blockIntVariables[this];
				string stringArg = Util.GetStringArg(args, 0, string.Empty);
				if (dictionary.ContainsKey(stringArg))
				{
					int num = Util.GetIntArg(args, 1, 1);
					if (num == 0)
					{
						num = 1;
					}
					dictionary[stringArg] /= num;
				}
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x060003E6 RID: 998 RVA: 0x000231B4 File Offset: 0x000215B4
		public TileResultCode BlockVariableModulus(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (Blocksworld.blockIntVariables.ContainsKey(this))
			{
				Dictionary<string, int> dictionary = Blocksworld.blockIntVariables[this];
				string stringArg = Util.GetStringArg(args, 0, string.Empty);
				if (dictionary.ContainsKey(stringArg))
				{
					int num = Util.GetIntArg(args, 1, 1);
					if (num == 0)
					{
						num = 1;
					}
					dictionary[stringArg] %= num;
				}
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x060003E7 RID: 999 RVA: 0x00023224 File Offset: 0x00021624
		public TileResultCode BlockVariableEquals(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (Blocksworld.blockIntVariables.ContainsKey(this))
			{
				Dictionary<string, int> dictionary = Blocksworld.blockIntVariables[this];
				string stringArg = Util.GetStringArg(args, 0, string.Empty);
				bool flag = false;
				if (dictionary.ContainsKey(stringArg))
				{
					int intArg = Util.GetIntArg(args, 1, 0);
					flag = (dictionary[stringArg] == intArg);
				}
				return (!flag) ? TileResultCode.False : TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x060003E8 RID: 1000 RVA: 0x00023294 File Offset: 0x00021694
		public TileResultCode BlockVariableNotEquals(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (Blocksworld.blockIntVariables.ContainsKey(this))
			{
				Dictionary<string, int> dictionary = Blocksworld.blockIntVariables[this];
				string stringArg = Util.GetStringArg(args, 0, string.Empty);
				bool flag = false;
				if (dictionary.ContainsKey(stringArg))
				{
					int intArg = Util.GetIntArg(args, 1, 0);
					flag = (dictionary[stringArg] != intArg);
				}
				return (!flag) ? TileResultCode.False : TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x060003E9 RID: 1001 RVA: 0x00023308 File Offset: 0x00021708
		public TileResultCode BlockVariableLessThan(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (Blocksworld.blockIntVariables.ContainsKey(this))
			{
				Dictionary<string, int> dictionary = Blocksworld.blockIntVariables[this];
				string stringArg = Util.GetStringArg(args, 0, string.Empty);
				bool flag = false;
				if (dictionary.ContainsKey(stringArg))
				{
					int intArg = Util.GetIntArg(args, 1, 0);
					flag = (dictionary[stringArg] < intArg);
				}
				return (!flag) ? TileResultCode.False : TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x060003EA RID: 1002 RVA: 0x00023378 File Offset: 0x00021778
		public TileResultCode BlockVariableMoreThan(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (Blocksworld.blockIntVariables.ContainsKey(this))
			{
				Dictionary<string, int> dictionary = Blocksworld.blockIntVariables[this];
				string stringArg = Util.GetStringArg(args, 0, string.Empty);
				bool flag = false;
				if (dictionary.ContainsKey(stringArg))
				{
					int intArg = Util.GetIntArg(args, 1, 0);
					flag = (dictionary[stringArg] > intArg);
				}
				return (!flag) ? TileResultCode.False : TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x060003EB RID: 1003 RVA: 0x000233E8 File Offset: 0x000217E8
		public TileResultCode BlockVariableLoadGlobal(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (Blocksworld.blockIntVariables.ContainsKey(this))
			{
				Dictionary<string, int> dictionary = Blocksworld.blockIntVariables[this];
				string stringArg = Util.GetStringArg(args, 0, string.Empty);
				string stringArg2 = Util.GetStringArg(args, 1, string.Empty);
				bool flag = false;
				if (dictionary.ContainsKey(stringArg) && Blocksworld.customIntVariables.ContainsKey(stringArg2))
				{
					dictionary[stringArg] = Blocksworld.customIntVariables[stringArg2];
					flag = true;
				}
				return (!flag) ? TileResultCode.False : TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x060003EC RID: 1004 RVA: 0x00023474 File Offset: 0x00021874
		public TileResultCode BlockVariableStoreGlobal(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (Blocksworld.blockIntVariables.ContainsKey(this))
			{
				Dictionary<string, int> dictionary = Blocksworld.blockIntVariables[this];
				string stringArg = Util.GetStringArg(args, 0, string.Empty);
				string stringArg2 = Util.GetStringArg(args, 1, string.Empty);
				bool flag = false;
				if (dictionary.ContainsKey(stringArg) && Blocksworld.customIntVariables.ContainsKey(stringArg2))
				{
					Blocksworld.customIntVariables[stringArg2] = dictionary[stringArg];
					flag = true;
				}
				return (!flag) ? TileResultCode.False : TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x060003ED RID: 1005 RVA: 0x00023500 File Offset: 0x00021900
		public TileResultCode BlockVariableAssignBV(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (Blocksworld.blockIntVariables.ContainsKey(this))
			{
				Dictionary<string, int> dictionary = Blocksworld.blockIntVariables[this];
				string stringArg = Util.GetStringArg(args, 0, string.Empty);
				string stringArg2 = Util.GetStringArg(args, 1, string.Empty);
				if (dictionary.ContainsKey(stringArg) && dictionary.ContainsKey(stringArg2))
				{
					int num = dictionary[stringArg2];
					float num2 = Mathf.Sign((float)num);
					int value = (int)(num2 * Mathf.Floor(eInfo.floatArg * ((float)Mathf.Abs(num) + 0.49f)));
					dictionary[stringArg] = value;
				}
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x060003EE RID: 1006 RVA: 0x000235A0 File Offset: 0x000219A0
		public TileResultCode BlockVariableAddBV(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (Blocksworld.blockIntVariables.ContainsKey(this))
			{
				Dictionary<string, int> dictionary = Blocksworld.blockIntVariables[this];
				string stringArg = Util.GetStringArg(args, 0, string.Empty);
				string stringArg2 = Util.GetStringArg(args, 1, string.Empty);
				if (dictionary.ContainsKey(stringArg) && dictionary.ContainsKey(stringArg2))
				{
					Dictionary<string, int> dictionary2;
					string key;
					(dictionary2 = dictionary)[key = stringArg] = dictionary2[key] + dictionary[stringArg2];
				}
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x060003EF RID: 1007 RVA: 0x00023624 File Offset: 0x00021A24
		public TileResultCode BlockVariableSubtractBV(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (Blocksworld.blockIntVariables.ContainsKey(this))
			{
				Dictionary<string, int> dictionary = Blocksworld.blockIntVariables[this];
				string stringArg = Util.GetStringArg(args, 0, string.Empty);
				string stringArg2 = Util.GetStringArg(args, 1, string.Empty);
				if (dictionary.ContainsKey(stringArg) && dictionary.ContainsKey(stringArg2))
				{
					Dictionary<string, int> dictionary2;
					string key;
					(dictionary2 = dictionary)[key = stringArg] = dictionary2[key] - dictionary[stringArg2];
				}
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x060003F0 RID: 1008 RVA: 0x000236A8 File Offset: 0x00021AA8
		public TileResultCode BlockVariableMultiplyBV(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (Blocksworld.blockIntVariables.ContainsKey(this))
			{
				Dictionary<string, int> dictionary = Blocksworld.blockIntVariables[this];
				string stringArg = Util.GetStringArg(args, 0, string.Empty);
				string stringArg2 = Util.GetStringArg(args, 1, string.Empty);
				if (dictionary.ContainsKey(stringArg) && dictionary.ContainsKey(stringArg2))
				{
					Dictionary<string, int> dictionary2;
					string key;
					(dictionary2 = dictionary)[key = stringArg] = dictionary2[key] * dictionary[stringArg2];
				}
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x060003F1 RID: 1009 RVA: 0x0002372C File Offset: 0x00021B2C
		public TileResultCode BlockVariableDivideBV(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (Blocksworld.blockIntVariables.ContainsKey(this))
			{
				Dictionary<string, int> dictionary = Blocksworld.blockIntVariables[this];
				string stringArg = Util.GetStringArg(args, 0, string.Empty);
				string stringArg2 = Util.GetStringArg(args, 1, string.Empty);
				if (dictionary.ContainsKey(stringArg) && dictionary.ContainsKey(stringArg2))
				{
					int num = dictionary[stringArg2];
					if (num == 0)
					{
						num = 1;
					}
					dictionary[stringArg] /= num;
				}
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x060003F2 RID: 1010 RVA: 0x000237B4 File Offset: 0x00021BB4
		public TileResultCode BlockVariableModulusBV(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (Blocksworld.blockIntVariables.ContainsKey(this))
			{
				Dictionary<string, int> dictionary = Blocksworld.blockIntVariables[this];
				string stringArg = Util.GetStringArg(args, 0, string.Empty);
				string stringArg2 = Util.GetStringArg(args, 1, string.Empty);
				if (dictionary.ContainsKey(stringArg) && dictionary.ContainsKey(stringArg2))
				{
					int num = dictionary[stringArg2];
					if (num == 0)
					{
						num = 1;
					}
					dictionary[stringArg] %= num;
				}
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x060003F3 RID: 1011 RVA: 0x0002383C File Offset: 0x00021C3C
		public TileResultCode BlockVariableEqualsBV(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (Blocksworld.blockIntVariables.ContainsKey(this))
			{
				Dictionary<string, int> dictionary = Blocksworld.blockIntVariables[this];
				bool flag = false;
				string stringArg = Util.GetStringArg(args, 0, string.Empty);
				string stringArg2 = Util.GetStringArg(args, 1, string.Empty);
				if (dictionary.ContainsKey(stringArg) && dictionary.ContainsKey(stringArg2))
				{
					flag = (dictionary[stringArg] == dictionary[stringArg2]);
				}
				return (!flag) ? TileResultCode.False : TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x060003F4 RID: 1012 RVA: 0x000238C0 File Offset: 0x00021CC0
		public TileResultCode BlockVariableNotEqualsBV(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (Blocksworld.blockIntVariables.ContainsKey(this))
			{
				Dictionary<string, int> dictionary = Blocksworld.blockIntVariables[this];
				bool flag = false;
				string stringArg = Util.GetStringArg(args, 0, string.Empty);
				string stringArg2 = Util.GetStringArg(args, 1, string.Empty);
				if (dictionary.ContainsKey(stringArg) && dictionary.ContainsKey(stringArg2))
				{
					flag = (dictionary[stringArg] != dictionary[stringArg2]);
				}
				return (!flag) ? TileResultCode.False : TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x060003F5 RID: 1013 RVA: 0x00023948 File Offset: 0x00021D48
		public TileResultCode BlockVariableLessThanBV(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (Blocksworld.blockIntVariables.ContainsKey(this))
			{
				Dictionary<string, int> dictionary = Blocksworld.blockIntVariables[this];
				bool flag = false;
				string stringArg = Util.GetStringArg(args, 0, string.Empty);
				string stringArg2 = Util.GetStringArg(args, 1, string.Empty);
				if (dictionary.ContainsKey(stringArg) && dictionary.ContainsKey(stringArg2))
				{
					flag = (dictionary[stringArg] < dictionary[stringArg2]);
				}
				return (!flag) ? TileResultCode.False : TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x060003F6 RID: 1014 RVA: 0x000239CC File Offset: 0x00021DCC
		public TileResultCode BlockVariableMoreThanBV(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (Blocksworld.blockIntVariables.ContainsKey(this))
			{
				Dictionary<string, int> dictionary = Blocksworld.blockIntVariables[this];
				bool flag = false;
				string stringArg = Util.GetStringArg(args, 0, string.Empty);
				string stringArg2 = Util.GetStringArg(args, 1, string.Empty);
				if (dictionary.ContainsKey(stringArg) && dictionary.ContainsKey(stringArg2))
				{
					flag = (dictionary[stringArg] > dictionary[stringArg2]);
				}
				return (!flag) ? TileResultCode.False : TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x060003F7 RID: 1015 RVA: 0x00023A50 File Offset: 0x00021E50
		private bool AnyWithinWater(List<Block> blocks)
		{
			for (int i = 0; i < blocks.Count; i++)
			{
				Block block = blocks[i];
				if (block.CanTriggerBlockListSensor() && BlockWater.BlockWithinWater(block, false))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060003F8 RID: 1016 RVA: 0x00023A98 File Offset: 0x00021E98
		private bool AnyWithinTaggedWater(List<Block> blocks, string tag)
		{
			for (int i = 0; i < blocks.Count; i++)
			{
				Block block = blocks[i];
				if (block.CanTriggerBlockListSensor() && BlockWater.BlockWithinTaggedWater(block, tag))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060003F9 RID: 1017 RVA: 0x00023ADE File Offset: 0x00021EDE
		public virtual TileResultCode IsWithinWater(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!BlockWater.BlockWithinWater(this, false)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x060003FA RID: 1018 RVA: 0x00023AF3 File Offset: 0x00021EF3
		public TileResultCode IsAnyBlockInModelWithinWater(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.UpdateConnectedCache();
			return (!this.AnyWithinWater(Block.connectedCache[this])) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x060003FB RID: 1019 RVA: 0x00023B19 File Offset: 0x00021F19
		public TileResultCode IsAnyBlockInChunkWithinWater(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!this.AnyWithinWater(this.chunk.blocks)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x060003FC RID: 1020 RVA: 0x00023B38 File Offset: 0x00021F38
		public virtual TileResultCode WithinTaggedWater(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			return (!BlockWater.BlockWithinTaggedWater(this, stringArg)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x060003FD RID: 1021 RVA: 0x00023B68 File Offset: 0x00021F68
		public TileResultCode WithinTaggedWaterModel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			this.UpdateConnectedCache();
			return (!this.AnyWithinTaggedWater(Block.connectedCache[this], stringArg)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x060003FE RID: 1022 RVA: 0x00023BA8 File Offset: 0x00021FA8
		public TileResultCode WithinTaggedWaterChunk(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			return (!this.AnyWithinTaggedWater(this.chunk.blocks, stringArg)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x060003FF RID: 1023 RVA: 0x00023BE0 File Offset: 0x00021FE0
		public TileResultCode WithinTaggedBlock(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			List<Block> blocksWithTag = TagManager.GetBlocksWithTag(stringArg);
			Collider component = this.go.GetComponent<Collider>();
			if (component == null)
			{
				return TileResultCode.False;
			}
			Bounds bounds = component.bounds;
			int count = blocksWithTag.Count;
			for (int i = 0; i < count; i++)
			{
				Block block = blocksWithTag[i];
				Collider component2 = block.go.GetComponent<Collider>();
				if (!(component2 == null))
				{
					if (component2.bounds.Intersects(bounds))
					{
						return TileResultCode.True;
					}
				}
			}
			return TileResultCode.False;
		}

		// Token: 0x06000400 RID: 1024 RVA: 0x00023C84 File Offset: 0x00022084
		public TileResultCode IsTappingBlock(ScriptRowExecutionInfo eInfo, object[] args)
		{
			bool flag = this.IsTapHoldingBlock(eInfo, args) == TileResultCode.True && Block.goTouchStarted;
			return (!flag) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000401 RID: 1025 RVA: 0x00023CB8 File Offset: 0x000220B8
		public virtual TileResultCode IsTapHoldingBlock(ScriptRowExecutionInfo eInfo, object[] args)
		{
			bool flag = Block.goTouched == this.go;
			Blocksworld.worldSessionHadBlockTap = (Blocksworld.worldSessionHadBlockTap || flag);
			return (!flag) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000402 RID: 1026 RVA: 0x00023CF0 File Offset: 0x000220F0
		private bool IsTappingAny(List<Block> blocks)
		{
			for (int i = 0; i < blocks.Count; i++)
			{
				Block block = blocks[i];
				if (Block.goTouched == block.go)
				{
					Blocksworld.worldSessionHadBlockTap = true;
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000403 RID: 1027 RVA: 0x00023D3C File Offset: 0x0002213C
		public TileResultCode IsTappingAnyBlockInModel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			bool flag = this.IsTapHoldingAnyBlockInModel(eInfo, args) == TileResultCode.True && Block.goTouchStarted;
			return (!flag) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000404 RID: 1028 RVA: 0x00023D6D File Offset: 0x0002216D
		public TileResultCode IsTapHoldingAnyBlockInModel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.UpdateConnectedCache();
			return (!this.IsTappingAny(Block.connectedCache[this])) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000405 RID: 1029 RVA: 0x00023D93 File Offset: 0x00022193
		public TileResultCode IsTappingAnyBlockInChunk(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!this.IsTappingAny(this.chunk.blocks)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000406 RID: 1030 RVA: 0x00023DB4 File Offset: 0x000221B4
		public virtual TileResultCode IsFasterThan(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float num = Util.GetFloatArg(args, 0, 0f) + 0.5f;
			bool flag = false;
			if (this.chunk != null && this.chunk.rb != null)
			{
				flag = (this.chunk.rb.velocity.sqrMagnitude > num * num);
			}
			return (!flag) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000407 RID: 1031 RVA: 0x00023E24 File Offset: 0x00022224
		public virtual TileResultCode IsSlowerThan(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float num = Mathf.Max(Util.GetFloatArg(args, 0, 1f) - 0.5f, 0f);
			bool flag = false;
			if (this.chunk != null && this.chunk.rb != null)
			{
				flag = (this.chunk.rb.velocity.sqrMagnitude < num * num);
			}
			return (!flag) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000408 RID: 1032 RVA: 0x00023E9C File Offset: 0x0002229C
		private bool GlueOnContactBlock(Block b, float sizeFactor = 1.5f)
		{
			Block block;
			if (CollisionManager.bumpedBy.TryGetValue(b, out block) && !block.isTerrain && block.modelBlock != this.modelBlock && !block.didFix)
			{
				Chunk chunk = block.chunk;
				if (chunk.rb != null && !chunk.rb.isKinematic)
				{
					Chunk chunk2 = b.chunk;
					BlockData blockData = BlockData.GetBlockData(this);
					float num;
					if (!blockData.TryGetFloat("CGOCS", out num))
					{
						num = chunk2.CalculateVolumeWithSizes(float.MaxValue);
						blockData.SetFloat("CGOCS", num);
					}
					if (chunk2 != chunk)
					{
						float num2 = chunk.CalculateVolumeWithSizes(float.MaxValue);
						if (num2 < num * sizeFactor)
						{
							for (int i = 0; i < chunk.blocks.Count; i++)
							{
								Block block2 = chunk.blocks[i];
								if (block2.allowGlueOnContact)
								{
									chunk.blocks.Remove(block2);
									chunk2.AddBlock(block2);
									block2.gluedOnContact = true;
								}
							}
							if (chunk.blocks.Count == 0)
							{
								Blocksworld.blocksworldCamera.Unfollow(chunk);
								Blocksworld.chunks.Remove(chunk);
								chunk.Destroy(false);
							}
							else
							{
								chunk.UpdateCenterOfMass(true);
							}
							chunk2.UpdateCenterOfMass(true);
							blockData.SetFloat("CGOCS", chunk2.CalculateVolumeWithSizes(float.MaxValue));
							return true;
						}
					}
				}
			}
			return false;
		}

		// Token: 0x06000409 RID: 1033 RVA: 0x00024016 File Offset: 0x00022416
		public TileResultCode IsGlueOnContact(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!this.gluedOnContact) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x0600040A RID: 1034 RVA: 0x0002402C File Offset: 0x0002242C
		public TileResultCode GlueOnContact(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.broken || this.vanished)
			{
				return TileResultCode.True;
			}
			float floatArg = Util.GetFloatArg(args, 0, 1.5f);
			this.GlueOnContactBlock(this, floatArg);
			return TileResultCode.True;
		}

		// Token: 0x0600040B RID: 1035 RVA: 0x00024068 File Offset: 0x00022468
		public TileResultCode GlueOnContactChunk(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.broken || this.vanished)
			{
				return TileResultCode.True;
			}
			if (this.chunk.rb != null)
			{
				float floatArg = Util.GetFloatArg(args, 0, 1.5f);
				for (int i = 0; i < this.chunk.blocks.Count; i++)
				{
					if (this.GlueOnContactBlock(this.chunk.blocks[i], floatArg))
					{
						break;
					}
				}
			}
			return TileResultCode.True;
		}

		// Token: 0x0600040C RID: 1036 RVA: 0x000240F4 File Offset: 0x000224F4
		public TileResultCode ReleaseGlueOnContact(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.broken || this.vanished)
			{
				return TileResultCode.True;
			}
			ReleaseGlueOnContactCommand c = new ReleaseGlueOnContactCommand(this);
			Blocksworld.AddFixedUpdateCommand(c);
			return TileResultCode.True;
		}

		// Token: 0x0600040D RID: 1037 RVA: 0x00024128 File Offset: 0x00022528
		public TileResultCode IsAllowGlueOnContact(ScriptRowExecutionInfo eInfo, object[] args)
		{
			bool intBooleanArg = Util.GetIntBooleanArg(args, 0, true);
			bool flag = (!intBooleanArg) ? (!this.allowGlueOnContact) : this.allowGlueOnContact;
			return (!flag) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x0600040E RID: 1038 RVA: 0x00024166 File Offset: 0x00022566
		public TileResultCode AllowGlueOnContact(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.allowGlueOnContact = Util.GetIntBooleanArg(args, 0, true);
			return TileResultCode.True;
		}

		// Token: 0x0600040F RID: 1039 RVA: 0x00024178 File Offset: 0x00022578
		public virtual void TeleportTo(Vector3 targetPos, bool resetAngle = false, bool resetVel = false, bool resetAngVel = false)
		{
			Vector3 position = this.goT.position;
			if ((double)(targetPos - position).sqrMagnitude > 0.1)
			{
				this.UpdateConnectedCache();
				Chunk chunk = this.chunk;
				if (chunk.go == null)
				{
					return;
				}
				Vector3 position2 = chunk.go.transform.position;
				HashSet<Chunk> hashSet = Block.connectedChunks[this];
				foreach (Chunk chunk2 in hashSet)
				{
					GameObject gameObject = chunk2.go;
					Transform transform = gameObject.transform;
					Vector3 b = transform.position - position2;
					if (resetAngle)
					{
						transform.rotation = this.parentPlayRotation;
					}
					if (resetVel || resetAngVel)
					{
						Rigidbody component = gameObject.GetComponent<Rigidbody>();
						bool isKinematic = component.isKinematic;
						if (component != null)
						{
							if (resetVel && !isKinematic)
							{
								component.velocity = Vector3.zero;
							}
							if (resetAngVel && !isKinematic)
							{
								component.angularVelocity = Vector3.zero;
							}
						}
					}
					transform.position = targetPos + b;
					BlockWater worldOceanBlock = Blocksworld.worldOceanBlock;
					for (int i = 0; i < chunk2.blocks.Count; i++)
					{
						Block block = chunk2.blocks[i];
						block.Teleported(resetAngle, resetVel, resetAngVel);
						if (worldOceanBlock != null && !worldOceanBlock.SimulatesBlock(block))
						{
							worldOceanBlock.AddBlockToSimulation(block);
						}
						Blocksworld.blocksworldCamera.HandleTeleport(block);
					}
				}
			}
		}

		// Token: 0x06000410 RID: 1040 RVA: 0x00024350 File Offset: 0x00022750
		public Vector3 GetSafeTeleportToPosition(Vector3 targetPos)
		{
			Vector3 position = this.goT.position;
			Vector3 b = targetPos - position;
			this.UpdateConnectedCache();
			if (Block.connectedCache == null)
			{
				Debug.Log("connectedCache is null!");
			}
			HashSet<Block> hashSet = new HashSet<Block>(Block.connectedCache[this]);
			Vector3 result = targetPos;
			if (hashSet.Count > 0)
			{
				Bounds bounds = default(Bounds);
				bool flag = true;
				foreach (Block block in hashSet)
				{
					Collider component = block.go.GetComponent<Collider>();
					if (flag)
					{
						flag = false;
						if (component != null)
						{
							bounds = component.bounds;
						}
						else
						{
							bounds = this.GetBounds();
						}
					}
					else if (component != null)
					{
						bounds.Encapsulate(component.bounds);
					}
					else
					{
						bounds.Encapsulate(this.GetBounds());
					}
				}
				bounds.center += b;
				result = targetPos + this.GetSafeNonOverlapOffset(bounds, hashSet);
			}
			return result;
		}

		// Token: 0x06000411 RID: 1041 RVA: 0x0002448C File Offset: 0x0002288C
		private Vector3 GetSafeNonOverlapOffset(Bounds bounds, HashSet<Block> safeBlocks)
		{
			Vector3 vector = bounds.size * 1.1f;
			int[] array = new int[]
			{
				0,
				1,
				-1
			};
			int num = int.MaxValue;
			Vector3 result = Vector3.zero;
			for (int i = 0; i < array.Length; i++)
			{
				float y = (float)array[i] * vector[1];
				for (int j = 0; j < array.Length; j++)
				{
					float z = (float)array[j] * vector[2];
					for (int k = 0; k < array.Length; k++)
					{
						float x = (float)array[k] * vector[0];
						Vector3 vector2 = new Vector3(x, y, z);
						Bounds testBounds = new Bounds(bounds.center + vector2, bounds.size);
						int num2 = this.ApproximateOverlappingCollidersCount(testBounds, safeBlocks, num);
						if (num2 == 0)
						{
							return vector2;
						}
						if (num2 < num)
						{
							num = num2;
							result = vector2;
						}
					}
				}
			}
			return result;
		}

		// Token: 0x06000412 RID: 1042 RVA: 0x0002458C File Offset: 0x0002298C
		private int ApproximateOverlappingCollidersCount(Bounds testBounds, HashSet<Block> safeBlocks, int bestCount)
		{
			Collider[] array = Physics.OverlapSphere(testBounds.center, Util.MaxComponent(testBounds.extents));
			int num = 0;
			foreach (Collider collider in array)
			{
				if (!collider.isTrigger && collider.bounds.Intersects(testBounds))
				{
					Block block = BWSceneManager.FindBlock(collider.gameObject, true);
					if (!safeBlocks.Contains(block))
					{
						if (block.isTerrain)
						{
							if (Util.PointWithinTerrain(testBounds.center, false) || Util.PointWithinTerrain(testBounds.center + Vector3.up * testBounds.extents.y * 0.7f, false) || Util.PointWithinTerrain(testBounds.center - Vector3.up * testBounds.extents.y * 0.7f, false))
							{
								num++;
							}
						}
						else
						{
							num++;
						}
					}
				}
				if (num >= bestCount)
				{
					break;
				}
			}
			return num;
		}

		// Token: 0x06000413 RID: 1043 RVA: 0x000246B5 File Offset: 0x00022AB5
		public virtual void Teleported(bool resetAngle = false, bool resetVel = false, bool resetAngVel = false)
		{
			this.lastTeleportedFrameCount = Blocksworld.playFixedUpdateCounter + 1;
		}

		// Token: 0x06000414 RID: 1044 RVA: 0x000246C4 File Offset: 0x00022AC4
		public TileResultCode Teleported(ScriptRowExecutionInfo eInfo, object[] args)
		{
			bool result = this.lastTeleportedFrameCount == Blocksworld.playFixedUpdateCounter;
			return this.boolToTileResult(result);
		}

		// Token: 0x06000415 RID: 1045 RVA: 0x000246E8 File Offset: 0x00022AE8
		public TileResultCode TeleportToTag(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string tagName = (args.Length <= 0) ? string.Empty : ((string)args[0]);
			Vector3 position = this.goT.position;
			Block block;
			if (TagManager.TryGetClosestBlockWithTag(tagName, position, out block, null))
			{
				bool intBooleanArg = Util.GetIntBooleanArg(args, 1, true);
				bool intBooleanArg2 = Util.GetIntBooleanArg(args, 2, true);
				bool intBooleanArg3 = Util.GetIntBooleanArg(args, 3, true);
				Vector3 targetPos = block.goT.position;
				if (Util.GetIntBooleanArg(args, 4, true))
				{
					targetPos = this.GetSafeTeleportToPosition(targetPos);
				}
				this.TeleportTo(targetPos, intBooleanArg, intBooleanArg2, intBooleanArg3);
				return TileResultCode.True;
			}
			return TileResultCode.True;
		}

		// Token: 0x06000416 RID: 1046 RVA: 0x00024780 File Offset: 0x00022B80
		public TileResultCode TagProximityCheck(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string posName = (args.Length <= 0) ? string.Empty : ((string)args[0]);
			List<Block> blocksWithTag = TagManager.GetBlocksWithTag(posName);
			if (blocksWithTag.Count == 0)
			{
				return TileResultCode.False;
			}
			float num = (args.Length <= 1) ? 15f : ((float)args[1]);
			bool flag = args.Length <= 2 || (int)args[2] != 0;
			Vector3 position = this.goT.position;
			if (flag)
			{
				for (int i = 0; i < blocksWithTag.Count; i++)
				{
					Block block = blocksWithTag[i];
					Vector3 position2 = block.goT.position;
					Collider component = block.go.GetComponent<Collider>();
					Vector3 a;
					if (component != null)
					{
						a = component.ClosestPointOnBounds(position);
					}
					else
					{
						a = position2;
					}
					float sqrMagnitude = (a - position).sqrMagnitude;
					if (sqrMagnitude < num * num)
					{
						return TileResultCode.True;
					}
				}
				return TileResultCode.False;
			}
			for (int j = 0; j < blocksWithTag.Count; j++)
			{
				Block block2 = blocksWithTag[j];
				Vector3 position3 = block2.goT.position;
				Collider component2 = block2.go.GetComponent<Collider>();
				Vector3 a2;
				if (component2 != null)
				{
					a2 = component2.ClosestPointOnBounds(position);
				}
				else
				{
					a2 = position3;
				}
				float sqrMagnitude2 = (a2 - position).sqrMagnitude;
				if (sqrMagnitude2 < num * num)
				{
					return TileResultCode.False;
				}
			}
			return TileResultCode.True;
		}

		// Token: 0x06000417 RID: 1047 RVA: 0x00024914 File Offset: 0x00022D14
		public TileResultCode TagVisibilityCheck(string tagName, bool triggerWhenInvisible, bool ignoreModel = false)
		{
			List<Block> blocksWithTag = TagManager.GetBlocksWithTag(tagName);
			Vector3 position = this.goT.position;
			int layermask = 539;
			for (int i = 0; i < blocksWithTag.Count; i++)
			{
				Block block = blocksWithTag[i];
				GameObject gameObject = block.go;
				if (gameObject.activeSelf)
				{
					Vector3 position2 = gameObject.transform.position;
					Collider component = gameObject.GetComponent<Collider>();
					Vector3 direction = position2 - position;
					float num = direction.magnitude;
					RaycastHit[] array = Physics.RaycastAll(position, direction, num + 0.1f, layermask);
					bool flag = array.Length == 0;
					float num2 = Util.MinComponent(block.size) * 0.9f;
					foreach (RaycastHit raycastHit in array)
					{
						Collider collider = raycastHit.collider;
						if (collider == component)
						{
							flag = true;
							num = raycastHit.distance;
						}
						else
						{
							Block block2 = BWSceneManager.FindBlock(collider.gameObject, false);
							if (block2 != null && block2 != this && (!ignoreModel || (block2.modelBlock != block.modelBlock && block2.modelBlock != this.modelBlock)) && !Materials.TextureIsTransparent(block2.GetTexture(0)) && raycastHit.distance + num2 < num)
							{
								flag = false;
								break;
							}
						}
					}
					if (!triggerWhenInvisible && flag)
					{
						return TileResultCode.True;
					}
					if (triggerWhenInvisible && flag)
					{
						return TileResultCode.False;
					}
				}
			}
			return (!triggerWhenInvisible) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000418 RID: 1048 RVA: 0x00024AC0 File Offset: 0x00022EC0
		public TileResultCode TagVisibilityCheck(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string tagName = (args.Length <= 0) ? string.Empty : ((string)args[0]);
			bool triggerWhenInvisible = args.Length > 1 && (int)args[1] != 0;
			return this.TagVisibilityCheck(tagName, triggerWhenInvisible, false);
		}

		// Token: 0x06000419 RID: 1049 RVA: 0x00024B10 File Offset: 0x00022F10
		public TileResultCode TagVisibilityCheckModel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string tagName = (args.Length <= 0) ? string.Empty : ((string)args[0]);
			bool triggerWhenInvisible = args.Length > 1 && (int)args[1] != 0;
			return this.TagVisibilityCheck(tagName, triggerWhenInvisible, true);
		}

		// Token: 0x0600041A RID: 1050 RVA: 0x00024B60 File Offset: 0x00022F60
		public TileResultCode TagFrustumCheck(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string posName = (args.Length <= 0) ? string.Empty : ((string)args[0]);
			bool flag = args.Length > 1 && (int)args[1] != 0;
			float num = (args.Length <= 2) ? 90f : ((float)args[2]);
			float num2 = (args.Length <= 3) ? 90f : ((float)args[3]);
			Vector3 vector = (args.Length <= 4) ? Vector3.forward : ((Vector3)args[4]);
			List<Block> blocksWithTag = TagManager.GetBlocksWithTag(posName);
			Transform transform = this.goT;
			Vector3 position = transform.position;
			Vector3 forward = transform.forward;
			Vector3 up = transform.up;
			Vector3 right = transform.right;
			Vector3 normalized = (right * vector.x + up * vector.y + forward * vector.z).normalized;
			for (int i = 0; i < blocksWithTag.Count; i++)
			{
				Block block = blocksWithTag[i];
				Vector3 position2 = block.goT.position;
				Vector3 a = position2 - position;
				float magnitude = a.magnitude;
				if (magnitude >= 0.01f)
				{
					Vector3 vec = a / magnitude;
					Vector3 to = Util.ProjectOntoPlane(vec, up);
					Vector3 to2 = Util.ProjectOntoPlane(vec, right);
					bool flag2 = Vector3.Angle(normalized, to) < num * 0.5f && Vector3.Angle(normalized, to2) < num2 * 0.5f;
					if ((flag2 && !flag) || (!flag2 && flag))
					{
						return TileResultCode.True;
					}
				}
			}
			return TileResultCode.False;
		}

		// Token: 0x0600041B RID: 1051 RVA: 0x00024D34 File Offset: 0x00023134
		public TileResultCode CounterUIValueCondition(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 2, 0);
			BlockCounterUI blockCounterUI;
			if (BlockCounterUI.allCounterBlocks.TryGetValue(intArg, out blockCounterUI))
			{
				int intArg2 = Util.GetIntArg(args, 0, 1);
				int intArg3 = Util.GetIntArg(args, 1, 0);
				return blockCounterUI.ValueCondition(intArg2, intArg3);
			}
			return TileResultCode.False;
		}

		// Token: 0x0600041C RID: 1052 RVA: 0x00024D78 File Offset: 0x00023178
		public TileResultCode GaugeUIValueCondition(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 2, 0);
			BlockGaugeUI blockGaugeUI;
			if (BlockGaugeUI.allGaugeBlocks.TryGetValue(intArg, out blockGaugeUI))
			{
				int intArg2 = Util.GetIntArg(args, 0, 1);
				int intArg3 = Util.GetIntArg(args, 1, 0);
				return blockGaugeUI.ValueCondition(intArg2, intArg3);
			}
			return TileResultCode.False;
		}

		// Token: 0x0600041D RID: 1053 RVA: 0x00024DBC File Offset: 0x000231BC
		public TileResultCode TimerUIValueCondition(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 2, 0);
			BlockTimerUI blockTimerUI;
			if (BlockTimerUI.allTimerBlocks.TryGetValue(intArg, out blockTimerUI))
			{
				float floatArg = Util.GetFloatArg(args, 0, 1f);
				int intArg2 = Util.GetIntArg(args, 1, 0);
				return blockTimerUI.ValueCondition(floatArg, intArg2);
			}
			return TileResultCode.False;
		}

		// Token: 0x0600041E RID: 1054 RVA: 0x00024E04 File Offset: 0x00023204
		public TileResultCode TimerUITimeEquals(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 1, 0);
			BlockTimerUI blockTimerUI;
			if (BlockTimerUI.allTimerBlocks.TryGetValue(intArg, out blockTimerUI))
			{
				float time = (args.Length <= 0) ? 0f : ((float)args[0]);
				return blockTimerUI.TimeEquals(time);
			}
			return TileResultCode.False;
		}

		// Token: 0x0600041F RID: 1055 RVA: 0x00024E54 File Offset: 0x00023254
		public TileResultCode TimerUIStartTimer(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 1, 0);
			BlockTimerUI blockTimerUI;
			if (BlockTimerUI.allTimerBlocks.TryGetValue(intArg, out blockTimerUI))
			{
				int dir = (args.Length <= 0) ? 1 : ((int)args[0]);
				blockTimerUI.StartTimer(dir);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000420 RID: 1056 RVA: 0x00024E9C File Offset: 0x0002329C
		public TileResultCode TimerUISetTime(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 1, 0);
			BlockTimerUI blockTimerUI;
			if (BlockTimerUI.allTimerBlocks.TryGetValue(intArg, out blockTimerUI))
			{
				float time = (args.Length <= 0) ? 0f : ((float)args[0]);
				blockTimerUI.SetTime(time);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000421 RID: 1057 RVA: 0x00024EE8 File Offset: 0x000232E8
		public TileResultCode TimerUIIncrementTime(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 1, 0);
			BlockTimerUI blockTimerUI;
			if (BlockTimerUI.allTimerBlocks.TryGetValue(intArg, out blockTimerUI))
			{
				float time = (args.Length <= 0) ? 0f : ((float)args[0]);
				blockTimerUI.IncrementTime(time);
			}
			return TileResultCode.False;
		}

		// Token: 0x06000422 RID: 1058 RVA: 0x00024F34 File Offset: 0x00023334
		public TileResultCode TimerUIPauseTimer(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 0, 0);
			BlockTimerUI blockTimerUI;
			if (BlockTimerUI.allTimerBlocks.TryGetValue(intArg, out blockTimerUI))
			{
				bool flag = blockTimerUI.IsRunning();
				if (flag)
				{
					blockTimerUI.PauseOneFrame();
				}
			}
			return TileResultCode.True;
		}

		// Token: 0x06000423 RID: 1059 RVA: 0x00024F70 File Offset: 0x00023370
		public TileResultCode TimerUIPauseUI(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float floatArg = Util.GetFloatArg(args, 0, 0.5f);
			int intArg = Util.GetIntArg(args, 1, 0);
			bool flag = eInfo.timer >= floatArg;
			BlockTimerUI blockTimerUI;
			if (BlockTimerUI.allTimerBlocks.TryGetValue(intArg, out blockTimerUI))
			{
				blockTimerUI.PauseUI(!flag);
			}
			return (!flag) ? TileResultCode.Delayed : TileResultCode.True;
		}

		// Token: 0x06000424 RID: 1060 RVA: 0x00024FCC File Offset: 0x000233CC
		public TileResultCode TimerUIWaitTimer(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float floatArg = Util.GetFloatArg(args, 0, 1f);
			int intArg = Util.GetIntArg(args, 1, 0);
			bool flag = eInfo.timer >= floatArg;
			BlockTimerUI blockTimerUI;
			if (!BlockTimerUI.allTimerBlocks.TryGetValue(intArg, out blockTimerUI))
			{
				return TileResultCode.True;
			}
			if (eInfo.timer == 0f && !blockTimerUI.CanBePaused())
			{
				return TileResultCode.True;
			}
			blockTimerUI.PauseOneFrame();
			return (!flag) ? TileResultCode.Delayed : TileResultCode.True;
		}

		// Token: 0x06000425 RID: 1061 RVA: 0x00025048 File Offset: 0x00023448
		public TileResultCode ObjectCounterUIValueCondition(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 0, 1);
			int intArg2 = Util.GetIntArg(args, 1, 0);
			int intArg3 = Util.GetIntArg(args, 2, 0);
			BlockObjectCounterUI objectCounter = TreasureHandler.GetObjectCounter(intArg3);
			return (objectCounter == null) ? TileResultCode.False : objectCounter.ValueCondition(intArg, intArg2);
		}

		// Token: 0x06000426 RID: 1062 RVA: 0x0002508C File Offset: 0x0002348C
		public TileResultCode ObjectCounterUIValueEqualsMaxValue(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 0, 0);
			BlockObjectCounterUI objectCounter = TreasureHandler.GetObjectCounter(intArg);
			return (objectCounter == null || !objectCounter.ValueEqualsMaxValue()) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000427 RID: 1063 RVA: 0x000250C4 File Offset: 0x000234C4
		public TileResultCode ObjectCounterUISetValueToMax(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 0, 0);
			BlockObjectCounterUI objectCounter = TreasureHandler.GetObjectCounter(intArg);
			if (objectCounter != null)
			{
				objectCounter.SetValueToMax(eInfo.floatArg);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000428 RID: 1064 RVA: 0x000250F4 File Offset: 0x000234F4
		public TileResultCode GaugeUIValueEqualsMaxValue(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 0, 0);
			BlockGaugeUI blockGaugeUI;
			if (BlockGaugeUI.allGaugeBlocks.TryGetValue(intArg, out blockGaugeUI))
			{
				return (!blockGaugeUI.ValueEqualsMaxValue()) ? TileResultCode.False : TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x06000429 RID: 1065 RVA: 0x00025130 File Offset: 0x00023530
		public TileResultCode GaugeUISetValueToMax(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 0, 0);
			BlockGaugeUI blockGaugeUI;
			if (BlockGaugeUI.allGaugeBlocks.TryGetValue(intArg, out blockGaugeUI))
			{
				blockGaugeUI.SetValueToMax(eInfo.floatArg);
			}
			return TileResultCode.True;
		}

		// Token: 0x0600042A RID: 1066 RVA: 0x00025168 File Offset: 0x00023568
		public TileResultCode TimerUIValueEqualsMaxValue(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 0, 0);
			BlockTimerUI blockTimerUI;
			if (BlockTimerUI.allTimerBlocks.TryGetValue(intArg, out blockTimerUI))
			{
				return (!blockTimerUI.ValueEqualsMaxValue()) ? TileResultCode.False : TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x0600042B RID: 1067 RVA: 0x000251A4 File Offset: 0x000235A4
		public TileResultCode TimerUISetValueToMax(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 0, 0);
			BlockTimerUI blockTimerUI;
			if (BlockTimerUI.allTimerBlocks.TryGetValue(intArg, out blockTimerUI))
			{
				blockTimerUI.SetValueToMax(eInfo.floatArg);
			}
			return TileResultCode.True;
		}

		// Token: 0x0600042C RID: 1068 RVA: 0x000251DC File Offset: 0x000235DC
		public TileResultCode CounterUIValueEqualsMaxValue(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 0, 0);
			BlockCounterUI blockCounterUI;
			if (BlockCounterUI.allCounterBlocks.TryGetValue(intArg, out blockCounterUI))
			{
				return (!blockCounterUI.ValueEqualsMaxValue()) ? TileResultCode.False : TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x0600042D RID: 1069 RVA: 0x00025218 File Offset: 0x00023618
		public TileResultCode CounterUISetValueToMax(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 0, 0);
			BlockCounterUI blockCounterUI;
			if (BlockCounterUI.allCounterBlocks.TryGetValue(intArg, out blockCounterUI))
			{
				blockCounterUI.SetValueToMax(eInfo.floatArg);
			}
			return TileResultCode.True;
		}

		// Token: 0x0600042E RID: 1070 RVA: 0x00025250 File Offset: 0x00023650
		public TileResultCode CounterUIValueEqualsMinValue(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 0, 0);
			BlockCounterUI blockCounterUI;
			if (BlockCounterUI.allCounterBlocks.TryGetValue(intArg, out blockCounterUI))
			{
				return (!blockCounterUI.ValueEqualsMinValue()) ? TileResultCode.False : TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x0600042F RID: 1071 RVA: 0x0002528C File Offset: 0x0002368C
		public TileResultCode CounterUISetValueToMin(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 0, 0);
			BlockCounterUI blockCounterUI;
			if (BlockCounterUI.allCounterBlocks.TryGetValue(intArg, out blockCounterUI))
			{
				blockCounterUI.SetValueToMin(eInfo.floatArg);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000430 RID: 1072 RVA: 0x000252C4 File Offset: 0x000236C4
		public TileResultCode GaugeUIValueEquals(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 1, 0);
			BlockGaugeUI blockGaugeUI;
			if (BlockGaugeUI.allGaugeBlocks.TryGetValue(intArg, out blockGaugeUI))
			{
				int intArg2 = Util.GetIntArg(args, 0, 1);
				return (!blockGaugeUI.ValueEquals(intArg2)) ? TileResultCode.False : TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x06000431 RID: 1073 RVA: 0x0002530C File Offset: 0x0002370C
		public TileResultCode GaugeUISetValue(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 1, 0);
			BlockGaugeUI blockGaugeUI;
			if (BlockGaugeUI.allGaugeBlocks.TryGetValue(intArg, out blockGaugeUI))
			{
				int intArg2 = Util.GetIntArg(args, 0, 1);
				blockGaugeUI.SetValue(intArg2, eInfo.floatArg);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000432 RID: 1074 RVA: 0x0002534C File Offset: 0x0002374C
		public TileResultCode GaugeUIGetFraction(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 1, 0);
			BlockGaugeUI blockGaugeUI;
			if (BlockGaugeUI.allGaugeBlocks.TryGetValue(intArg, out blockGaugeUI))
			{
				float num = Mathf.Max(Util.GetFloatArg(args, 0, 1f), 0.001f);
				float b = Mathf.Clamp(blockGaugeUI.GetFraction() / num, 0f, 1f);
				eInfo.floatArg = Mathf.Min(eInfo.floatArg, b);
				return (eInfo.floatArg <= 0f) ? TileResultCode.False : TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x06000433 RID: 1075 RVA: 0x000253D0 File Offset: 0x000237D0
		public TileResultCode ObjectCounterUIValueEquals(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 0, 1);
			int intArg2 = Util.GetIntArg(args, 1, 0);
			BlockObjectCounterUI objectCounter = TreasureHandler.GetObjectCounter(intArg2);
			return (objectCounter == null || !objectCounter.ValueEquals(intArg)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000434 RID: 1076 RVA: 0x00025410 File Offset: 0x00023810
		public TileResultCode ObjectCounterUISetValue(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 0, 1);
			int intArg2 = Util.GetIntArg(args, 1, 0);
			BlockObjectCounterUI objectCounter = TreasureHandler.GetObjectCounter(intArg2);
			if (objectCounter != null)
			{
				objectCounter.SetValue(intArg, eInfo.floatArg);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000435 RID: 1077 RVA: 0x0002544C File Offset: 0x0002384C
		public TileResultCode CounterUIValueEquals(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 0, 1);
			int intArg2 = Util.GetIntArg(args, 1, 0);
			BlockCounterUI blockCounterUI;
			if (BlockCounterUI.allCounterBlocks.TryGetValue(intArg2, out blockCounterUI))
			{
				return (!blockCounterUI.ValueEquals(intArg)) ? TileResultCode.False : TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x06000436 RID: 1078 RVA: 0x00025494 File Offset: 0x00023894
		public TileResultCode SetCounterUIValue(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 0, 1);
			int intArg2 = Util.GetIntArg(args, 1, 0);
			BlockCounterUI blockCounterUI;
			if (BlockCounterUI.allCounterBlocks.TryGetValue(intArg2, out blockCounterUI))
			{
				blockCounterUI.SetValue(intArg);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000437 RID: 1079 RVA: 0x000254CD File Offset: 0x000238CD
		public TileResultCode InCameraFrustum(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!GeometryUtility.TestPlanesAABB(Blocksworld.frustumPlanes, this.go.GetComponent<Collider>().bounds)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000438 RID: 1080 RVA: 0x000254F8 File Offset: 0x000238F8
		public TileResultCode GaugeUIIncrementValue(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 0, 1);
			int intArg2 = Util.GetIntArg(args, 1, 0);
			this.IncrementGaugeUI(intArg, intArg2);
			return TileResultCode.True;
		}

		// Token: 0x06000439 RID: 1081 RVA: 0x00025520 File Offset: 0x00023920
		private void IncrementGaugeUI(int inc, int counterIndex)
		{
			BlockGaugeUI blockGaugeUI;
			if (BlockGaugeUI.allGaugeBlocks.TryGetValue(counterIndex, out blockGaugeUI))
			{
				blockGaugeUI.IncrementValue(inc);
			}
		}

		// Token: 0x0600043A RID: 1082 RVA: 0x00025548 File Offset: 0x00023948
		public TileResultCode IncrementObjectCounterUI(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 0, 1);
			int intArg2 = Util.GetIntArg(args, 1, 0);
			return this.IncrementObjectCounterUI(intArg, intArg2);
		}

		// Token: 0x0600043B RID: 1083 RVA: 0x00025570 File Offset: 0x00023970
		public TileResultCode DecrementObjectCounterUI(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 0, 1);
			int intArg2 = Util.GetIntArg(args, 1, 0);
			return this.IncrementObjectCounterUI(-intArg, intArg2);
		}

		// Token: 0x0600043C RID: 1084 RVA: 0x00025598 File Offset: 0x00023998
		private TileResultCode IncrementObjectCounterUI(int inc, int counterIndex)
		{
			BlockObjectCounterUI objectCounter = TreasureHandler.GetObjectCounter(counterIndex);
			if (objectCounter != null)
			{
				objectCounter.IncrementValue(inc);
			}
			return TileResultCode.True;
		}

		// Token: 0x0600043D RID: 1085 RVA: 0x000255BC File Offset: 0x000239BC
		public TileResultCode RandomizeCounterUI(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 2, 0);
			BlockCounterUI blockCounterUI;
			if (BlockCounterUI.allCounterBlocks.TryGetValue(intArg, out blockCounterUI))
			{
				int intArg2 = Util.GetIntArg(args, 0, 0);
				int intArg3 = Util.GetIntArg(args, 1, 10);
				blockCounterUI.Randomize(intArg2, intArg3);
			}
			return TileResultCode.True;
		}

		// Token: 0x0600043E RID: 1086 RVA: 0x00025600 File Offset: 0x00023A00
		public TileResultCode IncrementCounterUIValue(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 0, 1);
			int intArg2 = Util.GetIntArg(args, 1, 0);
			this.IncrementCounterUI(intArg, intArg2);
			return TileResultCode.True;
		}

		// Token: 0x0600043F RID: 1087 RVA: 0x00025628 File Offset: 0x00023A28
		private void IncrementCounterUI(int inc, int counterIndex)
		{
			BlockCounterUI blockCounterUI;
			if (BlockCounterUI.allCounterBlocks.TryGetValue(counterIndex, out blockCounterUI))
			{
				blockCounterUI.BlockIncrementValue(this, inc);
			}
		}

		// Token: 0x06000440 RID: 1088 RVA: 0x00025650 File Offset: 0x00023A50
		public TileResultCode CounterUIAnimateScore(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 1, 0);
			BlockCounterUI blockCounterUI;
			if (BlockCounterUI.allCounterBlocks.TryGetValue(intArg, out blockCounterUI))
			{
				int intArg2 = Util.GetIntArg(args, 0, 0);
				blockCounterUI.SetScoreAnimatedBlock(this, intArg2);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000441 RID: 1089 RVA: 0x0002568C File Offset: 0x00023A8C
		public TileResultCode CounterUIScoreMultiplier(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 2, 0);
			BlockCounterUI blockCounterUI;
			if (BlockCounterUI.allCounterBlocks.TryGetValue(intArg, out blockCounterUI))
			{
				int intArg2 = Util.GetIntArg(args, 0, 0);
				bool intBooleanArg = Util.GetIntBooleanArg(args, 1, true);
				blockCounterUI.SetScoreMultiplierBlock(this, intArg2, intBooleanArg);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000442 RID: 1090 RVA: 0x000256D0 File Offset: 0x00023AD0
		public TileResultCode CounterUIGlobalScoreMultiplier(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 2, 0);
			BlockCounterUI blockCounterUI;
			if (BlockCounterUI.allCounterBlocks.TryGetValue(intArg, out blockCounterUI))
			{
				int intArg2 = Util.GetIntArg(args, 0, 0);
				bool intBooleanArg = Util.GetIntBooleanArg(args, 1, true);
				blockCounterUI.SetGlobalScoreMultiplier(intArg2, intBooleanArg);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000443 RID: 1091 RVA: 0x00025713 File Offset: 0x00023B13
		public virtual TileResultCode OnImpact(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!CollisionManager.IsImpactingBlock(this)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000444 RID: 1092 RVA: 0x00025727 File Offset: 0x00023B27
		public TileResultCode OnImpactModel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.UpdateConnectedCache();
			return (!CollisionManager.IsImpactingAny(Block.connectedCache[this], this)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000445 RID: 1093 RVA: 0x00025750 File Offset: 0x00023B50
		public virtual TileResultCode OnParticleImpact(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int particleType = (int)args[0];
			return (!CollisionManager.IsParticleImpactingBlock(this, particleType)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000446 RID: 1094 RVA: 0x0002577C File Offset: 0x00023B7C
		public TileResultCode OnParticleImpactModel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int particleType = (int)args[0];
			this.UpdateConnectedCache();
			return (!CollisionManager.IsParticleCollidingAny(Block.connectedCache[this], this, particleType)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000447 RID: 1095 RVA: 0x000257B8 File Offset: 0x00023BB8
		public virtual TileResultCode IsBumping(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string target = (string)args[0];
			return (!CollisionManager.IsBumpingBlock(this, target)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000448 RID: 1096 RVA: 0x000257E4 File Offset: 0x00023BE4
		public TileResultCode IsBumpingAnyBlockInModel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string target = (string)args[0];
			this.UpdateConnectedCache();
			return (!CollisionManager.IsBumpingAny(Block.connectedCache[this], target, this)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000449 RID: 1097 RVA: 0x00025820 File Offset: 0x00023C20
		public TileResultCode IsBumpingAnyBlockInChunk(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string target = (string)args[0];
			return (!CollisionManager.IsBumpingAny(this.chunk.blocks, target, this)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x0600044A RID: 1098 RVA: 0x00025854 File Offset: 0x00023C54
		public virtual TileResultCode IsBumpingTag(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string tag = (string)args[0];
			return (!CollisionManager.BlockIsBumpingTag(this, tag)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x0600044B RID: 1099 RVA: 0x00025880 File Offset: 0x00023C80
		private bool IsBumpingTagAny(List<Block> blocks, string tag)
		{
			for (int i = 0; i < blocks.Count; i++)
			{
				if (CollisionManager.BlockIsBumpingTag(blocks[i], tag))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600044C RID: 1100 RVA: 0x000258BC File Offset: 0x00023CBC
		public TileResultCode IsBumpingAnyTaggedBlockInModel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.UpdateConnectedCache();
			string tag = (string)args[0];
			return (!this.IsBumpingTagAny(Block.connectedCache[this], tag)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x0600044D RID: 1101 RVA: 0x000258F8 File Offset: 0x00023CF8
		public TileResultCode IsBumpingAnyTaggedBlockInChunk(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.UpdateConnectedCache();
			string tag = (string)args[0];
			return (!this.IsBumpingTagAny(this.chunk.blocks, tag)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x0600044E RID: 1102 RVA: 0x00025934 File Offset: 0x00023D34
		public TileResultCode IsHitByTaggedLaserModel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (!BlockAbstractLaser.anyLaserBeamOrPulseAvailable)
			{
				return TileResultCode.False;
			}
			string tagName = (string)args[0];
			return (!BlockAbstractLaser.IsHitByTaggedLaserModel(this.modelBlock, tagName)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x0600044F RID: 1103 RVA: 0x00025970 File Offset: 0x00023D70
		public TileResultCode IsHitByTaggedProjectileModel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (!BlockAbstractLaser.anyProjectileAvailable)
			{
				return TileResultCode.False;
			}
			string tagName = (string)args[0];
			return (!BlockAbstractLaser.IsHitByTaggedProjectileModel(this.modelBlock, tagName)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000450 RID: 1104 RVA: 0x000259AC File Offset: 0x00023DAC
		public TileResultCode IsHitByTaggedLaserChunk(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string tagName = (string)args[0];
			List<Block> blocks = this.chunk.blocks;
			return (!BlockAbstractLaser.IsHitByTaggedLaser(blocks, tagName)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000451 RID: 1105 RVA: 0x000259E4 File Offset: 0x00023DE4
		public TileResultCode IsHitByTaggedProjectileChunk(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string tagName = (string)args[0];
			List<Block> blocks = this.chunk.blocks;
			return (!BlockAbstractLaser.IsHitByTaggedProjectile(blocks, tagName)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000452 RID: 1106 RVA: 0x00025A19 File Offset: 0x00023E19
		public TileResultCode IsHitByLaserModel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (!BlockAbstractLaser.anyLaserBeamOrPulseAvailable)
			{
				return TileResultCode.False;
			}
			return (!BlockAbstractLaser.IsHitByLaserModel(this.modelBlock)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000453 RID: 1107 RVA: 0x00025A3E File Offset: 0x00023E3E
		public TileResultCode IsHitByLaserChunk(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (!BlockAbstractLaser.anyLaserBeamOrPulseAvailable)
			{
				return TileResultCode.False;
			}
			return (!BlockAbstractLaser.IsHitByLaser(this.chunk.blocks)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000454 RID: 1108 RVA: 0x00025A68 File Offset: 0x00023E68
		public TileResultCode IsHitByProjectileModel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (!BlockAbstractLaser.anyProjectileAvailable)
			{
				return TileResultCode.False;
			}
			return (!BlockAbstractLaser.IsHitByProjectileModel(this.modelBlock)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000455 RID: 1109 RVA: 0x00025A8D File Offset: 0x00023E8D
		public TileResultCode IsHitByProjectileChunk(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (!BlockAbstractLaser.anyProjectileAvailable)
			{
				return TileResultCode.False;
			}
			return (!BlockAbstractLaser.IsHitByProjectile(this.chunk.blocks)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000456 RID: 1110 RVA: 0x00025AB7 File Offset: 0x00023EB7
		public virtual TileResultCode IsHitByProjectile(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!BlockAbstractLaser.IsHitByProjectile(this)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000457 RID: 1111 RVA: 0x00025ACB File Offset: 0x00023ECB
		public virtual TileResultCode IsHitByPulseOrBeam(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!BlockAbstractLaser.IsHitByPulseOrBeam(this)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000458 RID: 1112 RVA: 0x00025ADF File Offset: 0x00023EDF
		public virtual TileResultCode IsHitByTaggedProjectile(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!BlockAbstractLaser.IsHitByTaggedProjectile(this, Util.GetStringArg(args, 0, string.Empty))) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000459 RID: 1113 RVA: 0x00025AFF File Offset: 0x00023EFF
		public virtual TileResultCode IsHitByTaggedPulseOrBeam(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!BlockAbstractLaser.IsHitByTaggedPulseOrBeam(this, Util.GetStringArg(args, 0, string.Empty))) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x0600045A RID: 1114 RVA: 0x00025B1F File Offset: 0x00023F1F
		public virtual TileResultCode IsHitByArrow(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!BlockAbstractBow.IsHitByArrow(this)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x0600045B RID: 1115 RVA: 0x00025B34 File Offset: 0x00023F34
		public virtual TileResultCode IsHitByTaggedArrow(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string tag = (string)args[0];
			return (!BlockAbstractBow.IsHitByTaggedArrow(this, tag)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x0600045C RID: 1116 RVA: 0x00025B5D File Offset: 0x00023F5D
		public TileResultCode IsHitByArrowModel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!BlockAbstractBow.IsHitByArrowModel(this.modelBlock)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x0600045D RID: 1117 RVA: 0x00025B76 File Offset: 0x00023F76
		public TileResultCode IsHitByArrowChunk(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!BlockAbstractBow.IsHitByArrow(this.chunk.blocks)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x0600045E RID: 1118 RVA: 0x00025B94 File Offset: 0x00023F94
		public TileResultCode IsHitByTaggedArrowModel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string tag = (string)args[0];
			return (!BlockAbstractBow.IsHitByTaggedArrowModel(this.modelBlock, tag)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x0600045F RID: 1119 RVA: 0x00025BC4 File Offset: 0x00023FC4
		public TileResultCode IsHitByTaggedArrowChunk(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string tag = (string)args[0];
			return (!BlockAbstractBow.IsHitByTaggedArrow(this.chunk.blocks, tag)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000460 RID: 1120 RVA: 0x00025BF7 File Offset: 0x00023FF7
		public TileResultCode IsPulledByMagnet(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!BlockAbstractMagnet.PulledByMagnet(this)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000461 RID: 1121 RVA: 0x00025C0B File Offset: 0x0002400B
		public TileResultCode IsPulledByMagnetModel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!BlockAbstractMagnet.PulledByMagnetModel(this.modelBlock)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000462 RID: 1122 RVA: 0x00025C24 File Offset: 0x00024024
		public TileResultCode IsPushedByMagnet(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!BlockAbstractMagnet.PushedByMagnet(this)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000463 RID: 1123 RVA: 0x00025C38 File Offset: 0x00024038
		public TileResultCode IsPushedByMagnetModel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!BlockAbstractMagnet.PushedByMagnetModel(this.modelBlock)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000464 RID: 1124 RVA: 0x00025C54 File Offset: 0x00024054
		private bool IsTriggeringAny(List<Block> blocks)
		{
			for (int i = 0; i < blocks.Count; i++)
			{
				Block b = blocks[i];
				if (CollisionManager.IsTriggeringBlock(b))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000465 RID: 1125 RVA: 0x00025C8E File Offset: 0x0002408E
		public TileResultCode IsTriggering(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!CollisionManager.IsTriggeringBlock(this)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000466 RID: 1126 RVA: 0x00025CA4 File Offset: 0x000240A4
		public TileResultCode IsAnyBlockInModelTriggering(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.UpdateConnectedCache();
			List<Block> blocks = Block.connectedCache[this];
			return (!this.IsTriggeringAny(blocks)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000467 RID: 1127 RVA: 0x00025CD7 File Offset: 0x000240D7
		public TileResultCode IsAnyBlockInChunkTriggering(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!this.IsTriggeringAny(this.chunk.blocks)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000468 RID: 1128 RVA: 0x00025CF6 File Offset: 0x000240F6
		public virtual TileResultCode SetTrigger(ScriptRowExecutionInfo eInfo, object[] args)
		{
			Blocksworld.AddFixedUpdateUniqueCommand(Block.updateBlockTriggerCommand, false);
			Block.updateBlockTriggerCommand.BlockIsTrigger(this);
			return TileResultCode.True;
		}

		// Token: 0x06000469 RID: 1129 RVA: 0x00025D0F File Offset: 0x0002410F
		public TileResultCode SetEveryBlockInModelAsTrigger(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.UpdateConnectedCache();
			this.SetAllTrigger(Block.connectedCache[this]);
			return TileResultCode.True;
		}

		// Token: 0x0600046A RID: 1130 RVA: 0x00025D2A File Offset: 0x0002412A
		public TileResultCode SetEveryBlockInChunkAsTrigger(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.SetAllTrigger(this.chunk.blocks);
			return TileResultCode.True;
		}

		// Token: 0x0600046B RID: 1131 RVA: 0x00025D40 File Offset: 0x00024140
		private void SetAllTrigger(List<Block> blocks)
		{
			Blocksworld.AddFixedUpdateUniqueCommand(Block.updateBlockTriggerCommand, false);
			for (int i = 0; i < blocks.Count; i++)
			{
				Block b = blocks[i];
				Block.updateBlockTriggerCommand.BlockIsTrigger(b);
			}
		}

		// Token: 0x0600046C RID: 1132 RVA: 0x00025D82 File Offset: 0x00024182
		public TileResultCode CameraFollow(ScriptRowExecutionInfo eInfo, object[] args)
		{
			Blocksworld.blocksworldCamera.CameraFollow(this, args);
			return TileResultCode.True;
		}

		// Token: 0x0600046D RID: 1133 RVA: 0x00025D91 File Offset: 0x00024191
		public TileResultCode CameraFollow2D(ScriptRowExecutionInfo eInfo, object[] args)
		{
			Blocksworld.blocksworldCamera.CameraFollow2D(this, args);
			return TileResultCode.True;
		}

		// Token: 0x0600046E RID: 1134 RVA: 0x00025DA0 File Offset: 0x000241A0
		public TileResultCode CameraFollowLookToward(ScriptRowExecutionInfo eInfo, object[] args)
		{
			Blocksworld.blocksworldCamera.CameraFollowLookToward(this, args);
			return TileResultCode.True;
		}

		// Token: 0x0600046F RID: 1135 RVA: 0x00025DAF File Offset: 0x000241AF
		public TileResultCode CameraFollowLookTowardTag(ScriptRowExecutionInfo eInfo, object[] args)
		{
			Blocksworld.blocksworldCamera.CameraFollowLookTowardTag(this, args);
			return TileResultCode.True;
		}

		// Token: 0x06000470 RID: 1136 RVA: 0x00025DBE File Offset: 0x000241BE
		public TileResultCode CameraFollowThirdPersonPlatform(ScriptRowExecutionInfo eInfo, object[] args)
		{
			Blocksworld.blocksworldCamera.CameraFollowThirdPersonPlatform(this, args);
			return TileResultCode.True;
		}

		// Token: 0x06000471 RID: 1137 RVA: 0x00025DD0 File Offset: 0x000241D0
		public TileResultCode CameraMoveTo(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float alpha = 0.95f;
			if (args.Length > 0)
			{
				alpha = (float)args[0];
			}
			Blocksworld.blocksworldCamera.CameraMoveTo(this, alpha);
			return TileResultCode.True;
		}

		// Token: 0x06000472 RID: 1138 RVA: 0x00025E04 File Offset: 0x00024204
		public TileResultCode CameraToNamedPose(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			float floatArg = Util.GetFloatArg(args, 1, 0.985f);
			float floatArg2 = Util.GetFloatArg(args, 2, 0.985f);
			float floatArg3 = Util.GetFloatArg(args, 3, 15f);
			bool intBooleanArg = Util.GetIntBooleanArg(args, 4, false);
			Blocksworld.blocksworldCamera.CameraToNamedPose(stringArg, floatArg, floatArg2, floatArg3, intBooleanArg);
			return TileResultCode.True;
		}

		// Token: 0x06000473 RID: 1139 RVA: 0x00025E60 File Offset: 0x00024260
		public TileResultCode SetTargetCameraAngle(ScriptRowExecutionInfo eInfo, object[] args)
		{
			Blocksworld.blocksworldCamera.SetTargetCameraAngle(Util.GetFloatArg(args, 0, 70f));
			return TileResultCode.True;
		}

		// Token: 0x06000474 RID: 1140 RVA: 0x00025E79 File Offset: 0x00024279
		public TileResultCode SetVerticalDistanceOffsetFactor(ScriptRowExecutionInfo eInfo, object[] args)
		{
			Blocksworld.blocksworldCamera.SetVerticalDistanceOffsetFactor(Util.GetFloatArg(args, 0, 1f));
			return TileResultCode.True;
		}

		// Token: 0x06000475 RID: 1141 RVA: 0x00025E92 File Offset: 0x00024292
		public TileResultCode SetTargetFollowDistanceMultiplier(ScriptRowExecutionInfo eInfo, object[] args)
		{
			Blocksworld.blocksworldCamera.SetTargetFollowDistanceMultiplier(Util.GetFloatArg(args, 0, 1f));
			return TileResultCode.True;
		}

		// Token: 0x06000476 RID: 1142 RVA: 0x00025EAC File Offset: 0x000242AC
		public TileResultCode CameraSpeedFoV(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float num = eInfo.floatArg * Util.GetFloatArg(args, 0, 50f);
			Blocksworld.blocksworldCamera.desiredSpeedFoV = num / 100f;
			return TileResultCode.True;
		}

		// Token: 0x06000477 RID: 1143 RVA: 0x00025EE0 File Offset: 0x000242E0
		public TileResultCode CameraLookAt(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float alpha = 0.95f;
			if (args.Length > 0)
			{
				alpha = (float)args[0];
			}
			Blocksworld.blocksworldCamera.CameraLookAt(this, alpha);
			return TileResultCode.True;
		}

		// Token: 0x06000478 RID: 1144 RVA: 0x00025F12 File Offset: 0x00024312
		public TileResultCode IsGameStart(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!Blocksworld.gameStart) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000479 RID: 1145 RVA: 0x00025F25 File Offset: 0x00024325
		public TileResultCode Wait(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (eInfo.timer >= 0.25f)
			{
				return TileResultCode.True;
			}
			return TileResultCode.Delayed;
		}

		// Token: 0x0600047A RID: 1146 RVA: 0x00025F3C File Offset: 0x0002433C
		public TileResultCode RandomWaitTime(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float max = (float)args[0];
			ScriptRowData scriptRowData = ScriptRowData.GetScriptRowData(eInfo);
			float num;
			if (eInfo.timer == 0f)
			{
				num = UnityEngine.Random.Range(0f, max);
				scriptRowData.SetFloat("RandomWaitTime", num);
			}
			else
			{
				num = scriptRowData.GetFloat("RandomWaitTime");
			}
			if (eInfo.timer >= num)
			{
				return TileResultCode.True;
			}
			return TileResultCode.Delayed;
		}

		// Token: 0x0600047B RID: 1147 RVA: 0x00025FA4 File Offset: 0x000243A4
		public TileResultCode RandomWaitTimeSensor(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float max = (float)args[0];
			ScriptRowData scriptRowData = ScriptRowData.GetScriptRowData(eInfo);
			float fixedTime = Time.fixedTime;
			float num;
			if (!scriptRowData.TryGetFloat("RandomWaitTimeSensor", out num))
			{
				num = fixedTime + UnityEngine.Random.Range(0f, max);
				scriptRowData.SetFloat("RandomWaitTimeSensor", num);
			}
			if (fixedTime >= num)
			{
				scriptRowData.RemoveFloat("RandomWaitTimeSensor");
				return TileResultCode.True;
			}
			return TileResultCode.Delayed;
		}

		// Token: 0x0600047C RID: 1148 RVA: 0x00026008 File Offset: 0x00024408
		public TileResultCode WaitTime(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float num = (float)args[0];
			if (eInfo.timer >= num)
			{
				return TileResultCode.True;
			}
			return TileResultCode.Delayed;
		}

		// Token: 0x0600047D RID: 1149 RVA: 0x00026030 File Offset: 0x00024430
		public TileResultCode StopScriptsModel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.UpdateConnectedCache();
			List<Block> list = Block.connectedCache[this];
			for (int i = 0; i < list.Count; i++)
			{
				Block b = list[i];
				Block.stopScriptsCommand.StopBlockScripts(b);
			}
			Blocksworld.AddFixedUpdateUniqueCommand(Block.stopScriptsCommand, true);
			return TileResultCode.Delayed;
		}

		// Token: 0x0600047E RID: 1150 RVA: 0x00026088 File Offset: 0x00024488
		public TileResultCode StopScriptsModelForTime(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float floatArg = Util.GetFloatArg(args, 0, 1f);
			this.UpdateConnectedCache();
			List<Block> list = Block.connectedCache[this];
			for (int i = 0; i < list.Count; i++)
			{
				Block b = list[i];
				Block.stopScriptsCommand.StopBlockScripts(b);
			}
			Block.stopScriptsCommand.StartBlockScripts(list, floatArg);
			Blocksworld.AddFixedUpdateUniqueCommand(Block.stopScriptsCommand, true);
			return TileResultCode.True;
		}

		// Token: 0x0600047F RID: 1151 RVA: 0x000260F7 File Offset: 0x000244F7
		public virtual TileResultCode StopScriptsBlock(ScriptRowExecutionInfo eInfo, object[] args)
		{
			Block.stopScriptsCommand.StopBlockScripts(this);
			Blocksworld.AddFixedUpdateUniqueCommand(Block.stopScriptsCommand, true);
			return TileResultCode.Delayed;
		}

		// Token: 0x06000480 RID: 1152 RVA: 0x00026110 File Offset: 0x00024510
		public TileResultCode LockInput(ScriptRowExecutionInfo eInfo, object[] args)
		{
			Blocksworld.lockInput = true;
			Block.unlockInputCommand.SetUnlockTime(Time.fixedTime + 0.1f);
			Blocksworld.AddFixedUpdateUniqueCommand(Block.unlockInputCommand, true);
			return TileResultCode.True;
		}

		// Token: 0x06000481 RID: 1153 RVA: 0x0002613C File Offset: 0x0002453C
		public TileResultCode SetBuoyancy(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float floatArg = Util.GetFloatArg(args, 0, 1f);
			this.buoyancyMultiplier = ((this.buoyancyMultiplier == floatArg) ? this.buoyancyMultiplier : floatArg);
			return TileResultCode.True;
		}

		// Token: 0x06000482 RID: 1154 RVA: 0x00026178 File Offset: 0x00024578
		public TileResultCode SetMass(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float floatArg = Util.GetFloatArg(args, 0, 1f);
			if (this.storedMassOverride != floatArg)
			{
				this.blockMassOverride = floatArg;
				this.storedMassOverride = floatArg;
				if (this.chunk != null)
				{
					this.chunk.UpdateCenterOfMass(false);
				}
			}
			this.overridingMass = true;
			if (this.chunk != null && !Block.massAlteredBlocks.Contains(this))
			{
				Block.massAlteredBlocks.Add(this);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000483 RID: 1155 RVA: 0x000261F4 File Offset: 0x000245F4
		public TileResultCode SetFriction(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float floatArg = Util.GetFloatArg(args, 0, 0.5f);
			Collider component = this.go.GetComponent<Collider>();
			PhysicMaterial material = component.material;
			material.staticFriction = Mathf.Clamp01(floatArg);
			material.dynamicFriction = Mathf.Clamp01(floatArg);
			if (floatArg > 1f)
			{
				material.frictionCombine = PhysicMaterialCombine.Maximum;
			}
			else if (floatArg < 0f)
			{
				material.frictionCombine = PhysicMaterialCombine.Minimum;
			}
			else
			{
				material.frictionCombine = PhysicMaterialCombine.Multiply;
			}
			component.material = material;
			return TileResultCode.True;
		}

		// Token: 0x06000484 RID: 1156 RVA: 0x00026278 File Offset: 0x00024678
		public TileResultCode SetBounce(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float floatArg = Util.GetFloatArg(args, 0, 0.5f);
			Collider component = this.go.GetComponent<Collider>();
			PhysicMaterial material = component.material;
			material.bounciness = floatArg;
			material.bounceCombine = PhysicMaterialCombine.Maximum;
			component.material = material;
			return TileResultCode.True;
		}

		// Token: 0x06000485 RID: 1157 RVA: 0x000262BC File Offset: 0x000246BC
		public static void UpdateOverridenMasses()
		{
			foreach (Block block in Block.massAlteredBlocks)
			{
				if (block.overridingMass)
				{
					block.overridingMass = false;
				}
				else if (block.storedMassOverride >= 0f && block.storedMassOverride != block.blockMassOverride)
				{
					block.blockMassOverride = -1f;
					block.chunk.UpdateCenterOfMass(true);
				}
			}
		}

		// Token: 0x06000486 RID: 1158 RVA: 0x00026360 File Offset: 0x00024760
		public virtual Color GetLightTint()
		{
			return Color.white;
		}

		// Token: 0x06000487 RID: 1159 RVA: 0x00026367 File Offset: 0x00024767
		public virtual Color GetEmissiveLightTint()
		{
			return Block.transparent;
		}

		// Token: 0x06000488 RID: 1160 RVA: 0x00026370 File Offset: 0x00024770
		private void AddMissingScaleTo()
		{
			List<Tile> list = this.tiles[0];
			if (6 >= list.Count)
			{
				list.Add(new Tile(new GAF("Block.ScaleTo", new object[]
				{
					new Vector3(1f, 1f, 1f)
				})));
			}
		}

		// Token: 0x06000489 RID: 1161 RVA: 0x000263D4 File Offset: 0x000247D4
		public void SetSubmeshInitPaintToTile(int meshIndex, string paint, bool ignoreIfExists = false)
		{
			if (paint == null)
			{
				BWLog.Warning("SetSubmeshInitPaintToTile() trying to set paint to null");
				return;
			}
			if (meshIndex > 0)
			{
				this.AddMissingScaleTo();
				List<Tile> list = this.tiles[0];
				int num = 0;
				foreach (Tile tile in list)
				{
					string name = tile.gaf.Predicate.Name;
					if (name == "Block.PaintTo" && tile.gaf.Args.Length > 1 && (int)tile.gaf.Args[1] == meshIndex)
					{
						if (ignoreIfExists)
						{
							return;
						}
						break;
					}
					else
					{
						num++;
					}
				}
				if (num < list.Count)
				{
					Tile tile2 = list[num];
					tile2.gaf.Args[0] = paint;
				}
				else
				{
					list.Add(new Tile(new GAF("Block.PaintTo", new object[]
					{
						paint,
						meshIndex
					})));
				}
			}
		}

		// Token: 0x0600048A RID: 1162 RVA: 0x00026504 File Offset: 0x00024904
		public void SetSimpleInitTile(GAF gaf)
		{
			List<Tile> list = this.tiles[0];
			int num = 0;
			foreach (Tile tile in list)
			{
				string name = tile.gaf.Predicate.Name;
				if (name == gaf.Predicate.Name)
				{
					break;
				}
				num++;
			}
			if (num < list.Count)
			{
				Tile tile2 = list[num];
				tile2.gaf = gaf;
			}
			else
			{
				list.Add(new Tile(gaf));
			}
		}

		// Token: 0x0600048B RID: 1163 RVA: 0x000265C4 File Offset: 0x000249C4
		public GAF GetSimpleInitGAF(string predName)
		{
			List<Tile> list = this.tiles[0];
			foreach (Tile tile in list)
			{
				if (predName == tile.gaf.Predicate.Name)
				{
					return tile.gaf;
				}
			}
			return null;
		}

		// Token: 0x0600048C RID: 1164 RVA: 0x0002664C File Offset: 0x00024A4C
		public Vector3 GetTextureToNormal(int meshIndex)
		{
			List<Tile> list = this.tiles[0];
			if (meshIndex == 0)
			{
				return (Vector3)list[5].gaf.Args[1];
			}
			foreach (Tile tile in list)
			{
				string name = tile.gaf.Predicate.Name;
				if (name == "Block.TextureTo" && tile.gaf.Args.Length > 2 && (int)tile.gaf.Args[2] == meshIndex)
				{
					return (Vector3)tile.gaf.Args[1];
				}
			}
			return Vector3.zero;
		}

		// Token: 0x0600048D RID: 1165 RVA: 0x00026738 File Offset: 0x00024B38
		public void SetSubmeshInitTextureToTile(int meshIndex, string texture, Vector3 normal, bool ignoreIfExists = false)
		{
			if (meshIndex > 0)
			{
				this.AddMissingScaleTo();
				List<Tile> list = this.tiles[0];
				int num = 0;
				foreach (Tile tile in list)
				{
					string name = tile.gaf.Predicate.Name;
					if (name == "Block.TextureTo" && tile.gaf.Args.Length > 2 && (int)tile.gaf.Args[2] == meshIndex)
					{
						if (ignoreIfExists)
						{
							return;
						}
						break;
					}
					else
					{
						num++;
					}
				}
				if (num < list.Count)
				{
					Tile tile2 = list[num];
					tile2.gaf.Args[0] = texture;
					tile2.gaf.Args[1] = normal;
				}
				else
				{
					list.Add(new Tile(new GAF("Block.TextureTo", new object[]
					{
						texture,
						normal,
						meshIndex
					})));
				}
			}
		}

		// Token: 0x0600048E RID: 1166 RVA: 0x00026878 File Offset: 0x00024C78
		protected virtual void FindSubMeshes()
		{
			if (this.subMeshes == null)
			{
				this.subMeshes = new List<CollisionMesh>();
				this.subMeshGameObjects = new List<GameObject>();
				this.subMeshPaints = new List<string>();
				this.subMeshTextures = new List<string>();
				this.subMeshTextureNormals = new List<Vector3>();
				List<GameObject> list = new List<GameObject>();
				IEnumerator enumerator = this.goT.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						object obj = enumerator.Current;
						Transform transform = (Transform)obj;
						GameObject gameObject = transform.gameObject;
						MeshFilter component = gameObject.GetComponent<MeshFilter>();
						SkinnedMeshRenderer component2 = gameObject.GetComponent<SkinnedMeshRenderer>();
						if (component != null || component2 != null)
						{
							list.Add(gameObject);
						}
					}
				}
				finally
				{
					IDisposable disposable;
					if ((disposable = (enumerator as IDisposable)) != null)
					{
						disposable.Dispose();
					}
				}
				if (this.canBeTextured == null)
				{
					this.canBeTextured = new bool[list.Count + 1];
				}
				if (this.canBeMaterialTextured == null)
				{
					this.canBeMaterialTextured = new bool[list.Count + 1];
				}
				list.Sort(new GameObjectNameComparer());
				int i;
				for (i = 0; i < list.Count; i++)
				{
					GameObject item = list[i];
					this.subMeshes.Add(null);
					this.subMeshGameObjects.Add(item);
					this.subMeshPaints.Add("Yellow");
					this.subMeshTextures.Add("Plain");
					this.subMeshTextureNormals.Add(Vector3.up);
					if (i < this.canBeTextured.Length)
					{
						this.canBeTextured[i] = true;
					}
					if (i < this.canBeMaterialTextured.Length)
					{
						this.canBeMaterialTextured[i] = true;
					}
				}
				this.canBeTextured[i] = true;
				this.canBeMaterialTextured[i] = true;
			}
		}

		// Token: 0x0600048F RID: 1167 RVA: 0x00026A54 File Offset: 0x00024E54
		public virtual CollisionMesh GetSubMesh(int meshIndex)
		{
			this.FindSubMeshes();
			int num = meshIndex - 1;
			if (num < this.subMeshGameObjects.Count)
			{
				this.subMeshes[num] = CollisionVolumes.GenerateCollisionMesh(this.subMeshGameObjects[num]);
				return this.subMeshes[num];
			}
			return null;
		}

		// Token: 0x06000490 RID: 1168 RVA: 0x00026AA8 File Offset: 0x00024EA8
		public virtual GameObject GetSubMeshGameObject(int meshIndex)
		{
			this.FindSubMeshes();
			int num = meshIndex - 1;
			if (num < this.subMeshGameObjects.Count)
			{
				return this.subMeshGameObjects[num];
			}
			return null;
		}

		// Token: 0x06000491 RID: 1169 RVA: 0x00026AE0 File Offset: 0x00024EE0
		public virtual string GetSubMeshPaint(int meshIndex)
		{
			this.FindSubMeshes();
			int num = meshIndex - 1;
			if (num < this.subMeshPaints.Count)
			{
				return this.subMeshPaints[num];
			}
			return "Black";
		}

		// Token: 0x06000492 RID: 1170 RVA: 0x00026B1C File Offset: 0x00024F1C
		public virtual string GetSubMeshTexture(int meshIndex)
		{
			this.FindSubMeshes();
			int num = meshIndex - 1;
			if (num < this.subMeshTextures.Count)
			{
				return this.subMeshTextures[num];
			}
			return "Plain";
		}

		// Token: 0x06000493 RID: 1171 RVA: 0x00026B58 File Offset: 0x00024F58
		public virtual Vector3 GetSubMeshTextureNormal(int meshIndex)
		{
			this.FindSubMeshes();
			int num = meshIndex - 1;
			if (num < this.subMeshTextureNormals.Count)
			{
				return this.subMeshTextureNormals[num];
			}
			return Vector3.up;
		}

		// Token: 0x06000494 RID: 1172 RVA: 0x00026B94 File Offset: 0x00024F94
		public int GetSubMeshIndex(GameObject obj)
		{
			int result = -1;
			if (obj == this.go)
			{
				result = 0;
			}
			else
			{
				for (int i = 0; i < this.subMeshGameObjects.Count; i++)
				{
					if (obj == this.subMeshGameObjects[i])
					{
						result = i + 1;
						break;
					}
				}
			}
			return result;
		}

		// Token: 0x06000495 RID: 1173 RVA: 0x00026BF8 File Offset: 0x00024FF8
		public virtual bool ContainsPaintableSubmeshes()
		{
			this.FindSubMeshes();
			return this.subMeshes.Count > 0;
		}

		// Token: 0x06000496 RID: 1174 RVA: 0x00026C10 File Offset: 0x00025010
		public virtual int GetMeshIndexForRay(Ray ray, bool refresh, out Vector3 point, out Vector3 normal)
		{
			this.FindSubMeshes();
			point = Vector3.zero;
			normal = Vector3.up;
			if (this.subMeshGameObjects.Count == 0)
			{
				return 0;
			}
			float num = 1E+10f;
			if (refresh || this.mainMesh == null)
			{
				this.mainMesh = CollisionVolumes.GenerateCollisionMesh(this.go);
			}
			if (this.mainMesh != null)
			{
				Vector3 a = default(Vector3);
				if (CollisionTest.RayMeshTestClosest(ray.origin, ray.direction, this.mainMesh, out a, out normal))
				{
					num = (a - ray.origin).magnitude;
				}
			}
			int result = 0;
			int num2 = 0;
			foreach (GameObject gameObject in this.subMeshGameObjects)
			{
				if (gameObject.activeInHierarchy && gameObject.GetComponent<Renderer>().enabled)
				{
					if (refresh || this.subMeshes[num2] == null)
					{
						this.subMeshes[num2] = CollisionVolumes.GenerateCollisionMesh(gameObject);
					}
					Vector3 vector = default(Vector3);
					Vector3 vector2 = default(Vector3);
					bool flag = CollisionTest.RayMeshTestClosest(ray.origin, ray.direction, this.subMeshes[num2], out vector, out vector2);
					if (flag)
					{
						float magnitude = (vector - ray.origin).magnitude;
						if (magnitude < num)
						{
							num = magnitude;
							result = num2 + 1;
							point = vector;
							normal = vector2;
						}
					}
				}
				num2++;
			}
			return result;
		}

		// Token: 0x06000497 RID: 1175 RVA: 0x00026DD8 File Offset: 0x000251D8
		public virtual TileResultCode PaintToSubMesh(string paint, bool permanent, int meshIndex = 0)
		{
			if (meshIndex > 0)
			{
				this.FindSubMeshes();
				int num = meshIndex - 1;
				if (num < this.subMeshes.Count)
				{
					GameObject gameObject = this.subMeshGameObjects[num];
					if (gameObject != null && this.childMeshes != null && this.childMeshes.ContainsKey(gameObject.name))
					{
						Materials.SetMaterial(gameObject, this.childMeshes[gameObject.name], gameObject.name, paint, this.GetTexture(meshIndex), this.GetSubMeshTextureNormal(meshIndex), Vector3.one, string.Empty);
						if (permanent)
						{
							this.SetSubmeshInitPaintToTile(meshIndex, paint, false);
						}
						this.subMeshPaints[num] = paint;
					}
				}
			}
			return TileResultCode.True;
		}

		// Token: 0x06000498 RID: 1176 RVA: 0x00026E94 File Offset: 0x00025294
		public Bounds GetBounds()
		{
			return Util.ComputeBounds(new List<Block>(1)
			{
				this
			});
		}

		// Token: 0x06000499 RID: 1177 RVA: 0x00026EB8 File Offset: 0x000252B8
		public Vector3[] GetScaleConstraints()
		{
			string key = this.BlockType();
			if (!Block.blockScaleConstraints.ContainsKey(key))
			{
				BlockMetaData blockMetaData = this.GetBlockMetaData();
				Vector3[] array;
				if (blockMetaData != null)
				{
					array = blockMetaData.scaleConstraints;
					if (array.Length == 0)
					{
						array = new Vector3[]
						{
							Vector3.right,
							Vector3.up,
							Vector3.forward
						};
					}
				}
				else
				{
					array = new Vector3[]
					{
						Vector3.right,
						Vector3.up,
						Vector3.forward
					};
				}
				Block.blockScaleConstraints.Add(key, array);
			}
			return Block.blockScaleConstraints[key];
		}

		// Token: 0x0600049A RID: 1178 RVA: 0x00026F8E File Offset: 0x0002538E
		public override string ToString()
		{
			return (!(this.go == null)) ? this.go.name : "Destroyed block";
		}

		// Token: 0x0600049B RID: 1179 RVA: 0x00026FB8 File Offset: 0x000253B8
		public void UpdateWithinWaterLPFilter(GameObject theGo = null)
		{
			if (!Sound.sfxEnabled)
			{
				return;
			}
			AudioLowPassFilter audioLowPassFilter = this.lpFilter;
			if (theGo == null)
			{
				theGo = this.go;
			}
			else
			{
				audioLowPassFilter = theGo.GetComponent<AudioLowPassFilter>();
			}
			this.CreatePositionedAudioSourceIfNecessary("Create", theGo);
			if (BlockAbstractWater.CameraWithinAnyWater() || BlockWater.BlockWithinWater(this, false))
			{
				if (audioLowPassFilter == null)
				{
					audioLowPassFilter = theGo.GetComponent<AudioLowPassFilter>();
					if (audioLowPassFilter == null)
					{
						audioLowPassFilter = theGo.AddComponent<AudioLowPassFilter>();
					}
					if (theGo == this.go)
					{
						this.lpFilter = audioLowPassFilter;
					}
					audioLowPassFilter.cutoffFrequency = 600f;
				}
				if (!audioLowPassFilter.enabled)
				{
					audioLowPassFilter.enabled = true;
				}
				audioLowPassFilter.cutoffFrequency = 600f;
			}
			else if (audioLowPassFilter != null)
			{
				if (audioLowPassFilter.enabled)
				{
					audioLowPassFilter.enabled = false;
				}
				audioLowPassFilter.cutoffFrequency = 20000f;
			}
		}

		// Token: 0x0600049C RID: 1180 RVA: 0x000270B0 File Offset: 0x000254B0
		protected virtual void PlayLoopSound(bool play, AudioClip clip = null, float volume = 1f, AudioSource source = null, float pitch = 1f)
		{
			if (this.go == null)
			{
				return;
			}
			if (Sound.BlockIsMuted(this) || this.vanished)
			{
				play = false;
			}
			if (source == null)
			{
				if (this.loopingAudioSource == null)
				{
					this.loopingAudioSource = this.GetOrCreateLoopingPositionedAudioSource("Create");
					Sound.SetWorldAudioSourceParams(this.loopingAudioSource, 5f, 150f, AudioRolloffMode.Logarithmic);
				}
				source = this.loopingAudioSource;
			}
			if (Blocksworld.CurrentState != State.Play || !Sound.sfxEnabled)
			{
				if (source.isPlaying)
				{
					source.Stop();
				}
				return;
			}
			if (source != null)
			{
				if (clip != null && source.clip != clip)
				{
					source.clip = clip;
				}
				if (play && !source.isPlaying && source.gameObject.activeSelf)
				{
					source.enabled = true;
					source.loop = true;
					source.volume = volume;
					source.pitch = pitch;
					source.Play();
				}
				else if (!play && source.isPlaying)
				{
					source.Stop();
				}
				else if (play && source.isPlaying)
				{
					source.enabled = true;
					source.volume = volume;
					source.pitch = pitch;
				}
			}
			else
			{
				BWLog.Info("Could not find audio source in block " + this.BlockType());
			}
		}

		// Token: 0x0600049D RID: 1181 RVA: 0x00027244 File Offset: 0x00025644
		public TileResultCode SetBackgroundMusic(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string text = (string)args[0];
			if (text != Blocksworld.currentBackgroundMusic)
			{
				Blocksworld.currentBackgroundMusic = text;
			}
			Blocksworld.SetBackgroundMusic(text);
			return TileResultCode.True;
		}

		// Token: 0x0600049E RID: 1182 RVA: 0x00027278 File Offset: 0x00025678
		public float GetMomentOfInertia(Vector3 pos, Vector3 axis, bool includeMass = true)
		{
			Vector3 vector = this.goT.rotation * this.GetScale();
			Vector3 position = this.goT.position;
			float num = (!includeMass) ? 1f : this.GetMass();
			Vector3 vector2 = new Vector3(Mathf.Abs(axis.x), Mathf.Abs(axis.y), Mathf.Abs(axis.z));
			Vector3 lhs = Vector3.one - vector2;
			Vector3 rhs = new Vector3(vector.x * vector.x, vector.y * vector.y, vector.z * vector.z);
			float num2 = num / 12f * Vector3.Dot(lhs, rhs);
			float magnitude = Vector3.Cross(vector2, position - pos).magnitude;
			return num2 + num * magnitude * magnitude;
		}

		// Token: 0x0600049F RID: 1183 RVA: 0x00027360 File Offset: 0x00025760
		private void AddAnimationSupport(string blockType, int objectIndex, string animationType)
		{
			if (Block.animationSupports == null)
			{
				Block.animationSupports = new Dictionary<string, List<HashSet<string>>>();
			}
			List<HashSet<string>> list;
			if (Block.animationSupports.ContainsKey(blockType))
			{
				list = Block.animationSupports[blockType];
			}
			else
			{
				list = new List<HashSet<string>>();
				Block.animationSupports[blockType] = list;
			}
			objectIndex = Mathf.Clamp(objectIndex, 0, 1000);
			while (objectIndex >= list.Count)
			{
				list.Add(null);
			}
			HashSet<string> hashSet = list[objectIndex];
			if (hashSet == null)
			{
				hashSet = new HashSet<string>();
				list[objectIndex] = hashSet;
			}
			hashSet.Add(animationType);
		}

		// Token: 0x060004A0 RID: 1184 RVA: 0x00027400 File Offset: 0x00025800
		private bool SupportsAnimation(GAF gaf)
		{
			if (Block.animationSupports == null)
			{
				Block.animationSupports = new Dictionary<string, List<HashSet<string>>>();
			}
			string key = this.BlockType();
			if (Block.animationSupports.ContainsKey(key))
			{
				List<HashSet<string>> list = Block.animationSupports[key];
				int num = (int)gaf.Args[0];
				if (num < list.Count)
				{
					string item = (string)gaf.Args[1];
					HashSet<string> hashSet = list[num];
					return hashSet != null && hashSet.Contains(item);
				}
			}
			return false;
		}

		// Token: 0x060004A1 RID: 1185 RVA: 0x00027489 File Offset: 0x00025889
		public bool SupportsGaf(GAF gaf)
		{
			return gaf.Predicate != Block.predicateAnimate || this.SupportsAnimation(gaf);
		}

		// Token: 0x060004A2 RID: 1186 RVA: 0x000274A4 File Offset: 0x000258A4
		private ConfigurableJoint FindJointBetween(Rigidbody rb, Rigidbody rb2)
		{
			foreach (ConfigurableJoint configurableJoint in rb.gameObject.GetComponents<ConfigurableJoint>())
			{
				if (configurableJoint.connectedBody == rb2)
				{
					return configurableJoint;
				}
			}
			return null;
		}

		// Token: 0x060004A3 RID: 1187 RVA: 0x000274EC File Offset: 0x000258EC
		protected void DestroyFakeRigidbodies()
		{
			if (this.fakeRigidbodyGos != null)
			{
				foreach (GameObject obj in this.fakeRigidbodyGos)
				{
					UnityEngine.Object.Destroy(obj);
				}
				this.fakeRigidbodyGos.Clear();
				this.fakeRigidbodyGos = null;
			}
		}

		// Token: 0x060004A4 RID: 1188 RVA: 0x00027564 File Offset: 0x00025964
		private Rigidbody CreateFakeRigidbody(Block b1)
		{
			Rigidbody rb = this.chunk.rb;
			Rigidbody rb2 = b1.chunk.rb;
			GameObject gameObject = new GameObject(this.go.name + " Fake Ridigbody");
			gameObject.transform.position = (b1.goT.position + this.goT.position) * 0.5f;
			Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
			if (Blocksworld.interpolateRigidBodies)
			{
				rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
			}
			rigidbody.angularDrag = 0f;
			rigidbody.drag = 0f;
			rigidbody.mass = Mathf.Max(0.5f, 0.1f * (rb2.mass + rb.mass));
			if (this.fakeRigidbodyGos == null)
			{
				this.fakeRigidbodyGos = new List<GameObject>();
			}
			this.fakeRigidbodyGos.Add(gameObject);
			return rigidbody;
		}

		// Token: 0x060004A5 RID: 1189 RVA: 0x00027648 File Offset: 0x00025A48
		public void CreateFakeRigidbodyBetweenJoints()
		{
			List<Block> list = this.ConnectionsOfType(2, true);
			foreach (Block block in list)
			{
				List<Block> list2 = block.ConnectionsOfType(2, true);
				foreach (Block block2 in list2)
				{
					if (block2 == this)
					{
						Rigidbody rb = this.chunk.rb;
						Rigidbody rb2 = block.chunk.rb;
						if (rb != null && rb2 != null)
						{
							if (block is BlockAbstractWheel)
							{
								BlockAbstractWheel blockAbstractWheel = (BlockAbstractWheel)block;
								ConfigurableJoint configurableJoint = null;
								GameObject gameObject = this.goT.parent.gameObject;
								foreach (ConfigurableJoint configurableJoint2 in blockAbstractWheel.turnJoints)
								{
									if (configurableJoint2.gameObject == gameObject)
									{
										configurableJoint = configurableJoint2;
										break;
									}
								}
								if (configurableJoint != null)
								{
									Rigidbody rigidbody = this.CreateFakeRigidbody(block);
									blockAbstractWheel.DestroyJoint(configurableJoint);
									configurableJoint = blockAbstractWheel.CreateJoints(rigidbody.gameObject);
									ConfigurableJoint configurableJoint3 = this.FindJointBetween(rb, rb2);
									if (this is BlockAbstractTorsionSpring)
									{
										BlockAbstractTorsionSpring blockAbstractTorsionSpring = (BlockAbstractTorsionSpring)this;
										configurableJoint3 = blockAbstractTorsionSpring.joint;
									}
									if (configurableJoint3 != null)
									{
										configurableJoint3.connectedBody = rigidbody;
										if (configurableJoint3 == configurableJoint)
										{
											BWLog.Info("same joints!!!");
										}
									}
								}
							}
							else
							{
								ConfigurableJoint configurableJoint4 = this.FindJointBetween(rb, rb2);
								ConfigurableJoint configurableJoint5 = this.FindJointBetween(rb2, rb);
								if (configurableJoint4 != null && configurableJoint5 != null)
								{
									Rigidbody rigidbody2 = this.CreateFakeRigidbody(block);
									configurableJoint4.connectedBody = rigidbody2;
									configurableJoint5.connectedBody = rigidbody2;
									if (block is BlockAbstractTorsionSpring)
									{
										BlockAbstractTorsionSpring blockAbstractTorsionSpring2 = (BlockAbstractTorsionSpring)block;
										blockAbstractTorsionSpring2.jointToJointConnection = rigidbody2;
									}
									if (block2 is BlockAbstractTorsionSpring)
									{
										BlockAbstractTorsionSpring blockAbstractTorsionSpring3 = (BlockAbstractTorsionSpring)block2;
										blockAbstractTorsionSpring3.jointToJointConnection = rigidbody2;
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x060004A6 RID: 1190 RVA: 0x000278E8 File Offset: 0x00025CE8
		public int GetInstanceId()
		{
			if (this.go != null)
			{
				return this.go.GetInstanceID();
			}
			return -1;
		}

		// Token: 0x060004A7 RID: 1191 RVA: 0x00027908 File Offset: 0x00025D08
		public bool ContainsTileWithAnyPredicate(HashSet<Predicate> preds)
		{
			for (int i = 0; i < this.tiles.Count; i++)
			{
				List<Tile> list = this.tiles[i];
				for (int j = 0; j < list.Count; j++)
				{
					Predicate predicate = list[j].gaf.Predicate;
					if (preds.Contains(predicate))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060004A8 RID: 1192 RVA: 0x00027976 File Offset: 0x00025D76
		public bool ContainsTileWithAnyPredicateInPlayMode(HashSet<Predicate> fewPreds)
		{
			return fewPreds.Overlaps(BWSceneManager.PlayBlockPredicates(this));
		}

		// Token: 0x060004A9 RID: 1193 RVA: 0x00027984 File Offset: 0x00025D84
		public bool ContainsTileWithAnyPredicateInPlayMode2(HashSet<Predicate> manyPreds)
		{
			if (!BWSceneManager.ContainsPlayBlockPredicate(this))
			{
				BWLog.Error("not in play block predicates " + this.ToString());
				return false;
			}
			return BWSceneManager.PlayBlockPredicates(this).Overlaps(manyPreds);
		}

		// Token: 0x060004AA RID: 1194 RVA: 0x000279B4 File Offset: 0x00025DB4
		public bool ContainsTileWithPredicateInPlayMode(Predicate pred)
		{
			return BWSceneManager.PlayBlockPredicates(this).Contains(pred);
		}

		// Token: 0x060004AB RID: 1195 RVA: 0x000279C4 File Offset: 0x00025DC4
		public bool ContainsTileWithPredicate(Predicate pred)
		{
			for (int i = 0; i < this.tiles.Count; i++)
			{
				List<Tile> list = this.tiles[i];
				for (int j = 0; j < list.Count; j++)
				{
					Predicate predicate = list[j].gaf.Predicate;
					if (pred == predicate)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060004AC RID: 1196 RVA: 0x00027A30 File Offset: 0x00025E30
		public bool ContainsTagBump()
		{
			if (Block.taggedBumpPreds == null)
			{
				Block.taggedBumpPreds = new HashSet<Predicate>();
				Block.taggedBumpPreds.Add(Block.predicateTaggedBump);
				Block.taggedBumpPreds.Add(Block.predicateTaggedBumpModel);
				Block.taggedBumpPreds.Add(Block.predicateTaggedBumpChunk);
			}
			return this.ContainsTileWithAnyPredicateInPlayMode(Block.taggedBumpPreds);
		}

		// Token: 0x060004AD RID: 1197 RVA: 0x00027A8C File Offset: 0x00025E8C
		protected AudioClip GetLoopClip()
		{
			if (this.loopClip == null)
			{
				this.loopClip = Sound.GetSfx(this.loopName);
			}
			return this.loopClip;
		}

		// Token: 0x060004AE RID: 1198 RVA: 0x00027AB6 File Offset: 0x00025EB6
		public virtual bool HasDynamicalLight()
		{
			return false;
		}

		// Token: 0x060004AF RID: 1199 RVA: 0x00027AB9 File Offset: 0x00025EB9
		public virtual Color GetDynamicalLightTint()
		{
			return Color.white;
		}

		// Token: 0x060004B0 RID: 1200 RVA: 0x00027AC0 File Offset: 0x00025EC0
		public float CalculateMassDistribution(Chunk chunk, Vector3 dir, Chunk ignoreChunk = null)
		{
			Vector3 position = this.goT.position;
			float result = 0f;
			HashSet<Chunk> hashSet = new HashSet<Chunk>();
			if (ignoreChunk != null)
			{
				hashSet.Add(ignoreChunk);
			}
			this.AddMassDistributionInfo(chunk, position, dir, hashSet, ref result);
			return result;
		}

		// Token: 0x060004B1 RID: 1201 RVA: 0x00027B00 File Offset: 0x00025F00
		private void AddMassDistributionInfo(Chunk chunk, Vector3 pos, Vector3 dir, HashSet<Chunk> visited, ref float mi)
		{
			visited.Add(chunk);
			HashSet<Chunk> hashSet = new HashSet<Chunk>();
			for (int i = 0; i < chunk.blocks.Count; i++)
			{
				Block block = chunk.blocks[i];
				if (block != null && block.go != null)
				{
					float momentOfInertia = block.GetMomentOfInertia(pos, dir, true);
					mi += momentOfInertia;
					foreach (Block block2 in block.ConnectionsOfType(2, true))
					{
						Chunk item = block2.chunk;
						if (!visited.Contains(item))
						{
							this.AddMassDistributionInfo(item, pos, dir, visited, ref mi);
						}
					}
				}
			}
		}

		// Token: 0x060004B2 RID: 1202 RVA: 0x00027BE4 File Offset: 0x00025FE4
		public bool ConnectedToKinematicChunk(Chunk chunk, Chunk ignoreChunk)
		{
			if (chunk.rb != null && chunk.rb.isKinematic)
			{
				return true;
			}
			HashSet<Chunk> hashSet = new HashSet<Chunk>();
			if (ignoreChunk != null)
			{
				hashSet.Add(ignoreChunk);
			}
			bool result = false;
			this.ConnectedToKinematicChunkRecursive(chunk, hashSet, ref result);
			return result;
		}

		// Token: 0x060004B3 RID: 1203 RVA: 0x00027C38 File Offset: 0x00026038
		private void ConnectedToKinematicChunkRecursive(Chunk chunk, HashSet<Chunk> visited, ref bool result)
		{
			visited.Add(chunk);
			for (int i = 0; i < chunk.blocks.Count; i++)
			{
				Block block = chunk.blocks[i];
				if (block != null && block.go != null)
				{
					foreach (Block block2 in block.ConnectionsOfType(2, true))
					{
						Chunk chunk2 = block2.chunk;
						if (!(chunk2.rb == null))
						{
							if (!visited.Contains(chunk2))
							{
								if (chunk2.rb.isKinematic)
								{
									result = true;
									return;
								}
								this.ConnectedToKinematicChunkRecursive(chunk2, visited, ref result);
							}
						}
					}
				}
			}
		}

		// Token: 0x060004B4 RID: 1204 RVA: 0x00027D2C File Offset: 0x0002612C
		public virtual void SetVaryingGravity(bool vg)
		{
		}

		// Token: 0x060004B5 RID: 1205 RVA: 0x00027D2E File Offset: 0x0002612E
		public virtual bool CanChangeMass()
		{
			return false;
		}

		// Token: 0x060004B6 RID: 1206 RVA: 0x00027D31 File Offset: 0x00026131
		public virtual float GetCurrentMassChange()
		{
			return 0f;
		}

		// Token: 0x060004B7 RID: 1207 RVA: 0x00027D38 File Offset: 0x00026138
		public virtual bool VisibleInPlayMode()
		{
			return true;
		}

		// Token: 0x060004B8 RID: 1208 RVA: 0x00027D3B File Offset: 0x0002613B
		public virtual bool ColliderIsTriggerInPlayMode()
		{
			return false;
		}

		// Token: 0x060004B9 RID: 1209 RVA: 0x00027D40 File Offset: 0x00026140
		public bool SelectableTerrain()
		{
			BlockMetaData blockMetaData = this.GetBlockMetaData();
			return blockMetaData != null && blockMetaData.selectableTerrain;
		}

		// Token: 0x060004BA RID: 1210 RVA: 0x00027D68 File Offset: 0x00026168
		public bool DisableBuildModeScale()
		{
			BlockMetaData blockMetaData = this.GetBlockMetaData();
			return blockMetaData != null && blockMetaData.disableBuildModeScale;
		}

		// Token: 0x060004BB RID: 1211 RVA: 0x00027D90 File Offset: 0x00026190
		public bool DisableBuildModeMove()
		{
			BlockMetaData blockMetaData = this.GetBlockMetaData();
			return blockMetaData != null && blockMetaData.disableBuildModeMove;
		}

		// Token: 0x060004BC RID: 1212 RVA: 0x00027DB8 File Offset: 0x000261B8
		public Vector3 AllowedBuildModeRotations()
		{
			BlockMetaData blockMetaData = this.GetBlockMetaData();
			if (blockMetaData != null)
			{
				return blockMetaData.allowedBuildModeRotations;
			}
			return Vector3.one;
		}

		// Token: 0x060004BD RID: 1213 RVA: 0x00027DE4 File Offset: 0x000261E4
		public virtual bool DoubleTapToSelect()
		{
			return false;
		}

		// Token: 0x060004BE RID: 1214 RVA: 0x00027DE8 File Offset: 0x000261E8
		private void UpdateIndexedTiles(Func<int, int> indexConverter)
		{
			HashSet<Predicate> hashSet = new HashSet<Predicate>();
			hashSet.Add(PredicateRegistry.ByName("Block.TutorialPaintExistingBlock", true));
			hashSet.Add(PredicateRegistry.ByName("Block.TutorialTextureExistingBlock", true));
			hashSet.Add(PredicateRegistry.ByName("Block.TutorialRotateExistingBlock", true));
			hashSet.Add(Block.predicateTutorialRemoveBlockHint);
			foreach (List<Tile> list in this.tiles)
			{
				foreach (Tile tile in list)
				{
					if (hashSet.Contains(tile.gaf.Predicate))
					{
						int arg = (int)tile.gaf.Args[0];
						int num = indexConverter(arg);
						tile.gaf.Args[0] = num;
					}
				}
			}
		}

		// Token: 0x060004BF RID: 1215 RVA: 0x00027F10 File Offset: 0x00026310
		public void BlockRemoved(int index)
		{
			this.UpdateIndexedTiles(delegate(int oldIndex)
			{
				if (oldIndex >= index)
				{
					return oldIndex - 1;
				}
				return oldIndex;
			});
		}

		// Token: 0x060004C0 RID: 1216 RVA: 0x00027F3C File Offset: 0x0002633C
		public void IndicesSwitched(int index1, int index2)
		{
			this.UpdateIndexedTiles(delegate(int index)
			{
				if (index == index1)
				{
					return index2;
				}
				if (index == index2)
				{
					return index1;
				}
				return index;
			});
		}

		// Token: 0x060004C1 RID: 1217 RVA: 0x00027F6F File Offset: 0x0002636F
		public virtual void RemovedPlayBlock(Block b)
		{
		}

		// Token: 0x060004C2 RID: 1218 RVA: 0x00027F71 File Offset: 0x00026371
		protected virtual bool CanScaleMesh(int meshIndex)
		{
			return true;
		}

		// Token: 0x060004C3 RID: 1219 RVA: 0x00027F74 File Offset: 0x00026374
		public virtual Vector3 GetCenter()
		{
			return this.goT.position;
		}

		// Token: 0x060004C4 RID: 1220 RVA: 0x00027F81 File Offset: 0x00026381
		public virtual Vector3 GetPlayModeCenter()
		{
			return this.goT.position;
		}

		// Token: 0x060004C5 RID: 1221 RVA: 0x00027F8E File Offset: 0x0002638E
		public virtual void BecameTreasure()
		{
		}

		// Token: 0x060004C6 RID: 1222 RVA: 0x00027F90 File Offset: 0x00026390
		public virtual List<Collider> GetColliders()
		{
			List<Collider> list = new List<Collider>(1);
			Collider component = this.go.GetComponent<Collider>();
			if (component != null)
			{
				list.Add(component);
			}
			return list;
		}

		// Token: 0x060004C7 RID: 1223 RVA: 0x00027FC4 File Offset: 0x000263C4
		protected void SetChildrenLocalScale(float scale)
		{
			IEnumerator enumerator = this.goT.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform = (Transform)obj;
					transform.localScale = Vector3.one * scale;
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
		}

		// Token: 0x060004C8 RID: 1224 RVA: 0x00028034 File Offset: 0x00026434
		public virtual void ChunkInModelFrozen()
		{
		}

		// Token: 0x060004C9 RID: 1225 RVA: 0x00028036 File Offset: 0x00026436
		public virtual void ChunkInModelUnfrozen()
		{
		}

		// Token: 0x060004CA RID: 1226 RVA: 0x00028038 File Offset: 0x00026438
		public static Type GetBlockTypeFromName(string name)
		{
			Type result;
			if (Block.blockNameTypeMap.TryGetValue(name, out result))
			{
				return result;
			}
			return typeof(Block);
		}

		// Token: 0x060004CB RID: 1227 RVA: 0x00028064 File Offset: 0x00026464
		public void ReplaceMeshCollider(MeshCollider mc)
		{
			if (this.meshColliderInfo == null)
			{
				this.meshColliderInfo = new MeshColliderInfo
				{
					mesh = mc.sharedMesh,
					convex = mc.convex,
					material = mc.material,
					block = this
				};
				UnityEngine.Object.Destroy(mc);
				BoxCollider boxCollider = this.go.AddComponent<BoxCollider>();
				boxCollider.isTrigger = true;
				boxCollider.size = this.size;
			}
		}

		// Token: 0x060004CC RID: 1228 RVA: 0x000280D9 File Offset: 0x000264D9
		public virtual float GetFogMultiplier()
		{
			return 1f;
		}

		// Token: 0x060004CD RID: 1229 RVA: 0x000280E0 File Offset: 0x000264E0
		public virtual Color GetFogColorOverride()
		{
			return Color.white;
		}

		// Token: 0x060004CE RID: 1230 RVA: 0x000280E7 File Offset: 0x000264E7
		public virtual float GetLightIntensityMultiplier()
		{
			return 1f;
		}

		// Token: 0x060004CF RID: 1231 RVA: 0x000280EE File Offset: 0x000264EE
		public bool IsUnlocked()
		{
			return this.ContainsTileWithPredicate(Block.predicateUnlocked);
		}

		// Token: 0x060004D0 RID: 1232 RVA: 0x000280FB File Offset: 0x000264FB
		public virtual bool TreatAsVehicleLikeBlock()
		{
			return false;
		}

		// Token: 0x060004D1 RID: 1233 RVA: 0x00028100 File Offset: 0x00026500
		protected bool TreatAsVehicleLikeBlockWithStatus(ref int treatAsVehicleStatus)
		{
			bool flag;
			if (treatAsVehicleStatus == -1)
			{
				flag = (!this.didFix && this.ContainsTileWithAnyPredicateInPlayMode2(Block.GetInputPredicates()));
				treatAsVehicleStatus = ((!flag) ? 0 : 1);
			}
			else
			{
				flag = (treatAsVehicleStatus == 1);
			}
			return flag;
		}

		// Token: 0x060004D2 RID: 1234 RVA: 0x0002814C File Offset: 0x0002654C
		public static HashSet<Predicate> GetPossibleModelConflictingInputPredicates()
		{
			if (Block.possibleModelConflictingInputPredicates == null)
			{
				Block.possibleModelConflictingInputPredicates = new HashSet<Predicate>
				{
					Block.predicateButton,
					Block.predicateTiltLeftRight,
					Block.predicateTiltFrontBack,
					Block.predicateDPadMoved,
					Block.predicateDPadVertical,
					Block.predicateDPadHorizontal,
					BlockCharacter.predicateCharacterMover,
					BlockCharacter.predicateCharacterJump,
					BlockMLPLegs.predicateMLPLegsMover,
					BlockMLPLegs.predicateMLPLegsJump,
					BlockLegs.predicateLegsMover,
					BlockLegs.predicateLegsJump,
					BlockQuadped.predicateQuadpedMover,
					BlockSphere.predicateSphereMover,
					BlockFlightYoke.predicateAlignAlongDPad,
					BlockAntiGravity.predicateAntigravityAlignAlongMover,
					BlockTankTreadsWheel.predicateTankTreadsAnalogStickControl,
					BlockTankTreadsWheel.predicateTankTreadsDriveAlongAnalogStick,
					BlockTankTreadsWheel.predicateTankTreadsTurnAlongAnalogStick,
					BlockSteeringWheel.predicateSteeringWheelMoveAlongMover,
					BlockSteeringWheel.predicateSteeringWheelMoverSteer
				};
			}
			return Block.possibleModelConflictingInputPredicates;
		}

		// Token: 0x060004D3 RID: 1235 RVA: 0x00028270 File Offset: 0x00026670
		public static HashSet<Predicate> GetInputPredicates()
		{
			if (Block.inputPredicates == null)
			{
				Block.inputPredicates = new HashSet<Predicate>
				{
					Block.predicateButton,
					Block.predicateTiltLeftRight,
					Block.predicateDPadMoved,
					Block.predicateDPadVertical,
					Block.predicateDPadHorizontal,
					Block.predicateTapBlock,
					Block.predicateTapChunk,
					Block.predicateTapModel,
					Block.predicateTapHoldBlock,
					Block.predicateTapHoldModel,
					Block.predicateSendSignal,
					Block.predicateSendCustomSignal,
					BlockCharacter.predicateCharacterMover,
					BlockCharacter.predicateCharacterJump,
					BlockMLPLegs.predicateMLPLegsMover,
					BlockMLPLegs.predicateMLPLegsJump,
					BlockLegs.predicateLegsMover,
					BlockLegs.predicateLegsJump,
					BlockQuadped.predicateQuadpedMover,
					BlockQuadped.predicateQuadpedJump,
					BlockSphere.predicateSphereMover,
					BlockSphere.predicateSphereTiltMover,
					BlockSphere.predicateSphereGoto,
					BlockSphere.predicateSphereChase,
					BlockFlightYoke.predicateAlignAlongDPad,
					BlockFlightYoke.predicateIncreaseLocalTorque,
					BlockFlightYoke.predicateIncreaseLocalVel,
					BlockAntiGravity.predicateAntigravityAlignAlongMover,
					BlockAntiGravity.predicateAntigravityIncreaseTorqueChunk,
					BlockAntiGravity.predicateAntigravityIncreaseVelocityChunk,
					BlockTankTreadsWheel.predicateTankTreadsAnalogStickControl,
					BlockTankTreadsWheel.predicateTankTreadsDriveAlongAnalogStick,
					BlockTankTreadsWheel.predicateTankTreadsTurnAlongAnalogStick,
					BlockSteeringWheel.predicateSteeringWheelMoveAlongMover,
					BlockSteeringWheel.predicateSteeringWheelMoverSteer
				};
			}
			return Block.inputPredicates;
		}

		// Token: 0x060004D4 RID: 1236 RVA: 0x0002843C File Offset: 0x0002683C
		public virtual float GetEffectPower()
		{
			return this.effectPower;
		}

		// Token: 0x060004D5 RID: 1237 RVA: 0x00028444 File Offset: 0x00026844
		public virtual Vector3 GetEffectSize()
		{
			return this.size;
		}

		// Token: 0x060004D6 RID: 1238 RVA: 0x0002844C File Offset: 0x0002684C
		public virtual Vector3 GetEffectLocalOffset()
		{
			return Vector3.zero;
		}

		// Token: 0x060004D7 RID: 1239 RVA: 0x00028453 File Offset: 0x00026853
		public virtual bool CanScaleUpwards()
		{
			return true;
		}

		// Token: 0x060004D8 RID: 1240 RVA: 0x00028456 File Offset: 0x00026856
		public virtual void BunchMoved()
		{
		}

		// Token: 0x060004D9 RID: 1241 RVA: 0x00028458 File Offset: 0x00026858
		public virtual void BunchRotated()
		{
		}

		// Token: 0x060004DA RID: 1242 RVA: 0x0002845A File Offset: 0x0002685A
		public virtual void RemoveBlockMaps()
		{
			BWSceneManager.RemoveBlockInstanceIDs(this.go);
		}

		// Token: 0x060004DB RID: 1243 RVA: 0x00028467 File Offset: 0x00026867
		public virtual GAF GetIconGaf()
		{
			return new GAF(Block.predicateCreate, new object[]
			{
				this.BlockType()
			});
		}

		// Token: 0x060004DC RID: 1244 RVA: 0x00028482 File Offset: 0x00026882
		public virtual Vector3 GetWaterForce(float fractionWithin, Vector3 relativeVelocity, BlockAbstractWater water)
		{
			return Vector3.zero;
		}

		// Token: 0x060004DD RID: 1245 RVA: 0x00028489 File Offset: 0x00026889
		public virtual void RestoredMeshCollider()
		{
		}

		// Token: 0x060004DE RID: 1246 RVA: 0x0002848B File Offset: 0x0002688B
		public virtual Bounds GetShapeCollisionBounds()
		{
			return this.go.GetComponent<Collider>().bounds;
		}

		// Token: 0x060004DF RID: 1247 RVA: 0x0002849D File Offset: 0x0002689D
		public virtual bool ShapeMeshCanCollideWith(Block b)
		{
			return true;
		}

		// Token: 0x060004E0 RID: 1248 RVA: 0x000284A0 File Offset: 0x000268A0
		public virtual void Exploded()
		{
		}

		// Token: 0x060004E1 RID: 1249 RVA: 0x000284A2 File Offset: 0x000268A2
		public virtual bool CanTriggerBlockListSensor()
		{
			return true;
		}

		// Token: 0x060004E2 RID: 1250 RVA: 0x000284A5 File Offset: 0x000268A5
		public virtual bool BreakByDetachExplosion()
		{
			return true;
		}

		// Token: 0x060004E3 RID: 1251 RVA: 0x000284A8 File Offset: 0x000268A8
		public virtual void TBoxSnap()
		{
		}

		// Token: 0x060004E4 RID: 1252 RVA: 0x000284AC File Offset: 0x000268AC
		private void UpdateNeighboringConnections()
		{
			for (int i = 0; i < this.connections.Count; i++)
			{
				Block block = this.connections[i];
				block.ConnectionsChanged();
			}
		}

		// Token: 0x060004E5 RID: 1253 RVA: 0x000284E8 File Offset: 0x000268E8
		public virtual void ConnectionsChanged()
		{
		}

		// Token: 0x060004E6 RID: 1254 RVA: 0x000284EC File Offset: 0x000268EC
		public virtual List<GameObject> GetIgnoreRaycastGOs()
		{
			return new List<GameObject>
			{
				this.go
			};
		}

		// Token: 0x060004E7 RID: 1255 RVA: 0x0002850C File Offset: 0x0002690C
		public virtual bool IgnoreTextureToIndexInTutorial(int meshIndex)
		{
			return false;
		}

		// Token: 0x060004E8 RID: 1256 RVA: 0x0002850F File Offset: 0x0002690F
		public virtual bool IgnorePaintToIndexInTutorial(int meshIndex)
		{
			return false;
		}

		// Token: 0x060004E9 RID: 1257 RVA: 0x00028512 File Offset: 0x00026912
		public virtual List<List<Tile>> GetRuntimeTiles()
		{
			return this.tiles;
		}

		// Token: 0x060004EA RID: 1258 RVA: 0x0002851C File Offset: 0x0002691C
		public HashSet<Predicate> GetPlayPredicates()
		{
			if (this.tiles.Count == 2 && this.tiles[1].Count == 1)
			{
				return Block.emptyPredicateSet;
			}
			HashSet<Predicate> hashSet = new HashSet<Predicate>();
			for (int i = 1; i < this.tiles.Count; i++)
			{
				List<Tile> list = this.tiles[i];
				for (int j = 0; j < list.Count; j++)
				{
					Predicate predicate = list[j].gaf.Predicate;
					if (predicate != Block.predicateThen)
					{
						hashSet.Add(predicate);
					}
				}
			}
			return hashSet;
		}

		// Token: 0x060004EB RID: 1259 RVA: 0x000285C8 File Offset: 0x000269C8
		public BlockGroup GetGroupOfType(string groupType)
		{
			if (this.groups != null)
			{
				foreach (BlockGroup blockGroup in this.groups)
				{
					if (blockGroup.GetGroupType() == groupType)
					{
						return blockGroup;
					}
				}
			}
			return null;
		}

		// Token: 0x060004EC RID: 1260 RVA: 0x00028644 File Offset: 0x00026A44
		public void RemoveGroup(string groupType)
		{
			BlockGroup groupOfType = this.GetGroupOfType(groupType);
			if (groupOfType != null)
			{
				this.groups.Remove(groupOfType);
			}
		}

		// Token: 0x060004ED RID: 1261 RVA: 0x0002866C File Offset: 0x00026A6C
		public bool IsMainBlockInGroup(string groupType)
		{
			BlockGroup groupOfType = this.GetGroupOfType(groupType);
			return groupOfType != null && groupOfType.GetBlocks()[0] == this;
		}

		// Token: 0x060004EE RID: 1262 RVA: 0x00028698 File Offset: 0x00026A98
		public Block GetMainBlockInGroup(string groupType)
		{
			BlockGroup groupOfType = this.GetGroupOfType(groupType);
			return (groupOfType != null) ? groupOfType.GetBlocks()[0] : this;
		}

		// Token: 0x060004EF RID: 1263 RVA: 0x000286C1 File Offset: 0x00026AC1
		public bool HasGroup(string groupType)
		{
			return this.GetGroupOfType(groupType) != null;
		}

		// Token: 0x060004F0 RID: 1264 RVA: 0x000286D0 File Offset: 0x00026AD0
		public bool HasAnyGroup()
		{
			return this.groups != null && this.groups.Count > 0;
		}

		// Token: 0x060004F1 RID: 1265 RVA: 0x000286EE File Offset: 0x00026AEE
		public virtual void SetBlockGroup(BlockGroup group)
		{
			if (this.groups == null)
			{
				this.groups = new HashSet<BlockGroup>();
			}
			this.groups.Add(group);
		}

		// Token: 0x060004F2 RID: 1266 RVA: 0x00028713 File Offset: 0x00026B13
		public virtual void OnBlockGroupReconstructed()
		{
		}

		// Token: 0x060004F3 RID: 1267 RVA: 0x00028718 File Offset: 0x00026B18
		public bool ContainsGroupTile()
		{
			List<Tile> list = this.tiles[0];
			for (int i = 6; i < list.Count; i++)
			{
				if (list[i].gaf.Predicate == Block.predicateGroup)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060004F4 RID: 1268 RVA: 0x00028767 File Offset: 0x00026B67
		public virtual bool HasPreferredChunkRotation()
		{
			return false;
		}

		// Token: 0x060004F5 RID: 1269 RVA: 0x0002876A File Offset: 0x00026B6A
		public virtual Quaternion GetPreferredChunkRotation()
		{
			return Quaternion.identity;
		}

		// Token: 0x060004F6 RID: 1270 RVA: 0x00028771 File Offset: 0x00026B71
		public virtual void ChunksAndJointsModified(Dictionary<Joint, Joint> oldToNew, Dictionary<Chunk, Chunk> oldToNewChunks, Dictionary<Chunk, Chunk> newToOldChunks)
		{
		}

		// Token: 0x060004F7 RID: 1271 RVA: 0x00028773 File Offset: 0x00026B73
		public virtual void Deactivate()
		{
			if (this.go != null)
			{
				this.go.SetActive(false);
			}
			if (this.goShadow != null)
			{
				this.goShadow.SetActive(false);
			}
		}

		// Token: 0x060004F8 RID: 1272 RVA: 0x000287AF File Offset: 0x00026BAF
		public virtual void Activate()
		{
			if (this.go != null)
			{
				this.go.SetActive(true);
			}
			if (this.goShadow != null)
			{
				this.goShadow.SetActive(true);
			}
		}

		// Token: 0x060004F9 RID: 1273 RVA: 0x000287EB File Offset: 0x00026BEB
		public virtual bool HasPreferredLookTowardAngleLocalVector()
		{
			return false;
		}

		// Token: 0x060004FA RID: 1274 RVA: 0x000287EE File Offset: 0x00026BEE
		public virtual Vector3 GetPreferredLookTowardAngleLocalVector()
		{
			return Vector3.back;
		}

		// Token: 0x060004FB RID: 1275 RVA: 0x000287F5 File Offset: 0x00026BF5
		public TileResultCode IsFirstPersonBlock(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (Blocksworld.blocksworldCamera.firstPersonBlock != this) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x060004FC RID: 1276 RVA: 0x0002880E File Offset: 0x00026C0E
		public TileResultCode FirstPersonCamera(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.broken || this.vanished)
			{
				return TileResultCode.True;
			}
			Blocksworld.blocksworldCamera.FirstPersonFollow(this, Util.GetIntArg(args, 0, 0));
			return TileResultCode.True;
		}

		// Token: 0x060004FD RID: 1277 RVA: 0x0002883C File Offset: 0x00026C3C
		public TileResultCode SetHudReticle(ScriptRowExecutionInfo eInfo, object[] args)
		{
			Blocksworld.blocksworldCamera.SetHudReticle(Util.GetIntArg(args, 0, 1));
			return TileResultCode.True;
		}

		// Token: 0x060004FE RID: 1278 RVA: 0x00028854 File Offset: 0x00026C54
		public virtual void SetFPCGearVisible(bool visible)
		{
			for (int i = 0; i < this.connections.Count; i++)
			{
				Block block = this.connections[i];
				int num = this.connectionTypes[i];
				if (num == 1)
				{
					BlockMetaData blockMetaData = block.GetBlockMetaData();
					if (null != blockMetaData && blockMetaData.hideInFirstPersonCamera)
					{
						block.go.SetActive(visible);
						if (blockMetaData.firstPersonCameraReplacement != null)
						{
							if (!visible)
							{
								Blocksworld.SetupCameraOverride(blockMetaData.firstPersonCameraReplacement, block);
							}
							else
							{
								Blocksworld.ResetCameraOverride();
							}
						}
					}
				}
			}
		}

		// Token: 0x060004FF RID: 1279 RVA: 0x000288F8 File Offset: 0x00026CF8
		public bool HasMover()
		{
			HashSet<Predicate> analogStickPredicates = Blocksworld.GetAnalogStickPredicates();
			HashSet<Predicate> tiltMoverPredicates = Blocksworld.GetTiltMoverPredicates();
			bool flag = false;
			int num = 0;
			while (num < this.tiles.Count && !flag)
			{
				bool flag2 = false;
				List<Tile> list = this.tiles[num];
				int num2 = 0;
				while (num2 < list.Count && !flag)
				{
					Predicate predicate = list[num2].gaf.Predicate;
					if (predicate == Block.predicateThen)
					{
						flag2 = true;
					}
					else if (flag2)
					{
						flag |= analogStickPredicates.Contains(predicate);
						flag |= tiltMoverPredicates.Contains(predicate);
					}
					num2++;
				}
				num++;
			}
			return flag;
		}

		// Token: 0x06000500 RID: 1280 RVA: 0x000289B0 File Offset: 0x00026DB0
		public bool HasAnyInputButton()
		{
			List<List<Tile>> runtimeTiles = this.GetRuntimeTiles();
			for (int i = 0; i < runtimeTiles.Count; i++)
			{
				List<Tile> list = runtimeTiles[i];
				for (int j = 0; j < list.Count; j++)
				{
					Predicate predicate = list[j].gaf.Predicate;
					if (predicate != Block.predicateThen)
					{
						if (predicate == Block.predicateButton)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		// Token: 0x06000501 RID: 1281 RVA: 0x00028A2D File Offset: 0x00026E2D
		public virtual void StartPull()
		{
			if (this.chunk != null)
			{
				this.chunk.StartPull();
			}
		}

		// Token: 0x06000502 RID: 1282 RVA: 0x00028A45 File Offset: 0x00026E45
		public virtual void StopPull()
		{
			if (this.chunk != null)
			{
				this.chunk.StopPull();
			}
		}

		// Token: 0x06000503 RID: 1283 RVA: 0x00028A5D File Offset: 0x00026E5D
		public virtual void PlaceInCharacterHand(BlockAnimatedCharacter character)
		{
		}

		// Token: 0x06000504 RID: 1284 RVA: 0x00028A5F File Offset: 0x00026E5F
		public virtual void RemoveFromCharacterHand()
		{
		}

		// Token: 0x06000505 RID: 1285 RVA: 0x00028A61 File Offset: 0x00026E61
		public virtual bool CanRepelAttack(Vector3 attackPosition, Vector3 attackDirection)
		{
			return Invincibility.IsInvincible(this);
		}

		// Token: 0x06000506 RID: 1286 RVA: 0x00028A6C File Offset: 0x00026E6C
		public virtual void OnAttacked(Vector3 attackPosition, Vector3 attackDirection)
		{
			if (this.chunk != null && this.chunk.rb != null)
			{
				this.chunk.rb.AddForceAtPosition(1000f * (attackDirection - attackDirection.y * Vector3.up), attackPosition);
			}
		}

		// Token: 0x06000507 RID: 1287 RVA: 0x00028ACC File Offset: 0x00026ECC
		public TileResultCode IsHitByBlocksterHandAttachment(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (BlockAnimatedCharacter.HitByHandAttachment(this))
			{
				return TileResultCode.True;
			}
			if (BlockAnimatedCharacter.HitByFoot(this))
			{
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x06000508 RID: 1288 RVA: 0x00028AEC File Offset: 0x00026EEC
		public TileResultCode IsHitByTaggedBlocksterHandAttachment(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string tag = (string)args[0];
			if (BlockAnimatedCharacter.HitByTaggedHandAttachment(this, tag))
			{
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x06000509 RID: 1289 RVA: 0x00028B11 File Offset: 0x00026F11
		public TileResultCode IsHitByBlocksterHandAttachmentModel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (BlockAnimatedCharacter.HitModelByHandAttachment(this))
			{
				return TileResultCode.True;
			}
			if (BlockAnimatedCharacter.HitModelByFoot(this))
			{
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x0600050A RID: 1290 RVA: 0x00028B30 File Offset: 0x00026F30
		public TileResultCode IsHitByTaggedBlocksterHandAttachmentModel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string tag = (string)args[0];
			if (BlockAnimatedCharacter.HitModelByTaggedHandAttachment(this, tag))
			{
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x0600050B RID: 1291 RVA: 0x00028B55 File Offset: 0x00026F55
		public TileResultCode IsFiredAsWeapon(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (BlockAnimatedCharacter.FiredAsWeapon(this))
			{
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x04000234 RID: 564
		public static bool goTouchStarted = false;

		// Token: 0x04000235 RID: 565
		public static GameObject goTouched = null;

		// Token: 0x04000236 RID: 566
		public static GameObject prefabShadow = null;

		// Token: 0x04000237 RID: 567
		public static Transform shadowParent = null;

		// Token: 0x04000238 RID: 568
		private BlockMetaData meta;

		// Token: 0x04000239 RID: 569
		public HashSet<BlockGroup> groups;

		// Token: 0x0400023A RID: 570
		public Chunk chunk;

		// Token: 0x0400023B RID: 571
		public List<Block> connections = new List<Block>();

		// Token: 0x0400023C RID: 572
		public List<int> connectionTypes = new List<int>();

		// Token: 0x0400023D RID: 573
		public Block modelBlock;

		// Token: 0x0400023F RID: 575
		private float blockMass = -1f;

		// Token: 0x04000240 RID: 576
		private float blockMassOverride = -1f;

		// Token: 0x04000241 RID: 577
		private float storedMassOverride = -1f;

		// Token: 0x04000242 RID: 578
		private bool overridingMass;

		// Token: 0x04000243 RID: 579
		private static List<Block> massAlteredBlocks = new List<Block>();

		// Token: 0x04000244 RID: 580
		private static int nextBlockId = 0;

		// Token: 0x04000245 RID: 581
		public GameObject go;

		// Token: 0x04000246 RID: 582
		public Transform goT;

		// Token: 0x04000247 RID: 583
		public GameObject goShadow;

		// Token: 0x04000248 RID: 584
		public Transform goShadowT;

		// Token: 0x04000249 RID: 585
		private bool hasShadow;

		// Token: 0x0400024A RID: 586
		public string colliderName = string.Empty;

		// Token: 0x0400024B RID: 587
		protected Layer goLayerAssignment;

		// Token: 0x0400024C RID: 588
		public List<List<Tile>> tiles = new List<List<Tile>>();

		// Token: 0x0400024D RID: 589
		public Mesh mesh;

		// Token: 0x0400024E RID: 590
		public Dictionary<string, Mesh> childMeshes;

		// Token: 0x0400024F RID: 591
		public Mesh meshShadow;

		// Token: 0x04000250 RID: 592
		protected Color[] colorsShadow;

		// Token: 0x04000251 RID: 593
		public Renderer renderer;

		// Token: 0x04000252 RID: 594
		private bool isScaled;

		// Token: 0x04000253 RID: 595
		private Vector3 colliderScale = Vector3.one;

		// Token: 0x04000254 RID: 596
		private Vector3 meshScale = Vector3.one;

		// Token: 0x04000255 RID: 597
		private string meshScaleTexture = string.Empty;

		// Token: 0x04000256 RID: 598
		public float effectPower;

		// Token: 0x04000257 RID: 599
		public Vector3 size = Vector3.one;

		// Token: 0x04000258 RID: 600
		public Vector3 shadowSize = Vector3.one;

		// Token: 0x04000259 RID: 601
		public bool broken;

		// Token: 0x0400025A RID: 602
		public bool vanished;

		// Token: 0x0400025B RID: 603
		public bool didFix;

		// Token: 0x0400025C RID: 604
		public bool isTreasure;

		// Token: 0x0400025D RID: 605
		public bool gluedOnContact;

		// Token: 0x0400025E RID: 606
		public bool allowGlueOnContact = true;

		// Token: 0x0400025F RID: 607
		private bool hadRigidBody;

		// Token: 0x04000260 RID: 608
		public bool activateForPlay = true;

		// Token: 0x04000261 RID: 609
		public int lastTeleportedFrameCount = -1;

		// Token: 0x04000262 RID: 610
		public bool tagBumpEnabled;

		// Token: 0x04000263 RID: 611
		public bool isTransparent;

		// Token: 0x04000264 RID: 612
		public bool isRuntimeInvisible;

		// Token: 0x04000265 RID: 613
		public bool isRuntimePhantom;

		// Token: 0x04000266 RID: 614
		public int frozenInTerrainStatus = -1;

		// Token: 0x04000267 RID: 615
		public CollisionMesh[] shapeMeshes;

		// Token: 0x04000268 RID: 616
		public CollisionMesh[] glueMeshes;

		// Token: 0x04000269 RID: 617
		public CollisionMesh[] jointMeshes;

		// Token: 0x0400026A RID: 618
		public const float JOINT_ANGULAR_DRIVE_FORCE_FACTOR = 100f;

		// Token: 0x0400026B RID: 619
		public const float JOINT_ANGULAR_SPRING_DAMPING_FACTOR = 0.0035f;

		// Token: 0x0400026C RID: 620
		public const float JOINT_ANGULAR_SPRING_FACTOR = 0.5f;

		// Token: 0x0400026D RID: 621
		public const float cycleTime = 0.25f;

		// Token: 0x0400026E RID: 622
		public ScriptRowExecutionInfo[] executionInfos = new ScriptRowExecutionInfo[1];

		// Token: 0x0400026F RID: 623
		public bool skipUpdateSATVolumes;

		// Token: 0x04000270 RID: 624
		public bool isTerrain;

		// Token: 0x04000271 RID: 625
		public bool fakeTerrain;

		// Token: 0x04000272 RID: 626
		public bool containsPlayModeTiles;

		// Token: 0x04000273 RID: 627
		public float lastShadowHitDistance = -2f;

		// Token: 0x04000274 RID: 628
		private Vector3 lastShadowHitPoint;

		// Token: 0x04000275 RID: 629
		protected Vector3 oldShadowHitNormal = Vector3.up;

		// Token: 0x04000276 RID: 630
		private float shadowMaxDistance = 5f;

		// Token: 0x04000277 RID: 631
		private float shadowStrengthMultiplier = 1f;

		// Token: 0x04000278 RID: 632
		private float oldShadowAlpha = -1f;

		// Token: 0x04000279 RID: 633
		public Vector3 oldPos = Util.nullVector3;

		// Token: 0x0400027A RID: 634
		private Quaternion oldRotation = Quaternion.Euler(new Vector3(12.3f, 34.8f, 34.2f));

		// Token: 0x0400027B RID: 635
		public float buoyancyMultiplier = 1f;

		// Token: 0x0400027C RID: 636
		protected const float METAL_BUOYANCY_MULTIPLIER = 0.2f;

		// Token: 0x0400027D RID: 637
		public static AnimationCurve vanishAnimCurve = null;

		// Token: 0x0400027E RID: 638
		private int animationStep;

		// Token: 0x0400027F RID: 639
		private float animationFixedTime;

		// Token: 0x04000280 RID: 640
		private Vector3 playPosition = default(Vector3);

		// Token: 0x04000281 RID: 641
		private Vector3 parentPlayPosition = default(Vector3);

		// Token: 0x04000282 RID: 642
		public Quaternion playRotation = default(Quaternion);

		// Token: 0x04000283 RID: 643
		private Quaternion parentPlayRotation = default(Quaternion);

		// Token: 0x04000284 RID: 644
		private Vector3 objectAnimationPositionOffset = default(Vector3);

		// Token: 0x04000285 RID: 645
		private Quaternion objectAnimationRotationOffset = default(Quaternion);

		// Token: 0x04000286 RID: 646
		private Vector3 objectAnimationScaleOffset = default(Vector3);

		// Token: 0x04000287 RID: 647
		internal int rbConstraintsOn;

		// Token: 0x04000288 RID: 648
		internal int rbConstraintsOff;

		// Token: 0x04000289 RID: 649
		internal bool rbUpdatedConstraints;

		// Token: 0x0400028A RID: 650
		protected List<CollisionMesh> subMeshes;

		// Token: 0x0400028B RID: 651
		protected List<string> subMeshPaints;

		// Token: 0x0400028C RID: 652
		protected List<string> subMeshTextures;

		// Token: 0x0400028D RID: 653
		protected List<Vector3> subMeshTextureNormals;

		// Token: 0x0400028E RID: 654
		public List<GameObject> subMeshGameObjects;

		// Token: 0x0400028F RID: 655
		private CollisionMesh mainMesh;

		// Token: 0x04000290 RID: 656
		protected bool[] canBeTextured;

		// Token: 0x04000291 RID: 657
		protected bool[] canBeMaterialTextured;

		// Token: 0x04000292 RID: 658
		protected AudioSource audioSource;

		// Token: 0x04000293 RID: 659
		protected AudioSource loopingAudioSource;

		// Token: 0x04000294 RID: 660
		protected AudioLowPassFilter lpFilter;

		// Token: 0x04000295 RID: 661
		public static LeaderboardData leaderboardData = null;

		// Token: 0x04000296 RID: 662
		public static Predicate predicateThen;

		// Token: 0x04000297 RID: 663
		public static Predicate predicateStop;

		// Token: 0x04000298 RID: 664
		public static Predicate predicateSectionIndex;

		// Token: 0x04000299 RID: 665
		public static Predicate predicateHideTileRow;

		// Token: 0x0400029A RID: 666
		public static Predicate predicateHideNextTile;

		// Token: 0x0400029B RID: 667
		public static Predicate predicateLocked;

		// Token: 0x0400029C RID: 668
		public static Predicate predicateUnlocked;

		// Token: 0x0400029D RID: 669
		public static Predicate predicateCreate;

		// Token: 0x0400029E RID: 670
		public static Predicate predicateCreateModel;

		// Token: 0x0400029F RID: 671
		public static Predicate predicateFreeze;

		// Token: 0x040002A0 RID: 672
		public static Predicate predicateUnfreeze;

		// Token: 0x040002A1 RID: 673
		public static Predicate predicatePaintTo;

		// Token: 0x040002A2 RID: 674
		public static Predicate predicateTextureTo;

		// Token: 0x040002A3 RID: 675
		public static Predicate predicateMoveTo;

		// Token: 0x040002A4 RID: 676
		public static Predicate predicateRotateTo;

		// Token: 0x040002A5 RID: 677
		public static Predicate predicateScaleTo;

		// Token: 0x040002A6 RID: 678
		public static Predicate predicatePlaySoundDurational;

		// Token: 0x040002A7 RID: 679
		public static Predicate predicateSetFog;

		// Token: 0x040002A8 RID: 680
		public static Predicate predicateVanishBlock;

		// Token: 0x040002A9 RID: 681
		public static Predicate predicateVanishModel;

		// Token: 0x040002AA RID: 682
		public static Predicate predicateGroup;

		// Token: 0x040002AB RID: 683
		public static Predicate predicateUI;

		// Token: 0x040002AC RID: 684
		public static Predicate predicateUIOpaque;

		// Token: 0x040002AD RID: 685
		public static Predicate predicateTutorialCreateBlockHint;

		// Token: 0x040002AE RID: 686
		public static Predicate predicateTutorialAutoAddBlock;

		// Token: 0x040002AF RID: 687
		public static Predicate predicateTutorialRemoveBlockHint;

		// Token: 0x040002B0 RID: 688
		public static Predicate predicateTutorialRotateExistingBlock;

		// Token: 0x040002B1 RID: 689
		public static Predicate predicateTutorialPaintExistingBlock;

		// Token: 0x040002B2 RID: 690
		public static Predicate predicateTutorialTextureExistingBlock;

		// Token: 0x040002B3 RID: 691
		public static Predicate predicateTutorialOperationPose;

		// Token: 0x040002B4 RID: 692
		public static Predicate predicateTutorialMoveBlock;

		// Token: 0x040002B5 RID: 693
		public static Predicate predicateTutorialMoveModel;

		// Token: 0x040002B6 RID: 694
		public static Predicate predicateTutorialHideInBuildMode;

		// Token: 0x040002B7 RID: 695
		public static Predicate predicateImpact;

		// Token: 0x040002B8 RID: 696
		public static Predicate predicateImpactModel;

		// Token: 0x040002B9 RID: 697
		public static Predicate predicateParticleImpact;

		// Token: 0x040002BA RID: 698
		public static Predicate predicateParticleImpactModel;

		// Token: 0x040002BB RID: 699
		public static Predicate predicateBump;

		// Token: 0x040002BC RID: 700
		public static Predicate predicateBumpChunk;

		// Token: 0x040002BD RID: 701
		public static Predicate predicateBumpModel;

		// Token: 0x040002BE RID: 702
		public static Predicate predicateTaggedBump;

		// Token: 0x040002BF RID: 703
		public static Predicate predicateTaggedBumpModel;

		// Token: 0x040002C0 RID: 704
		public static Predicate predicateTaggedBumpChunk;

		// Token: 0x040002C1 RID: 705
		public static Predicate predicateNegate;

		// Token: 0x040002C2 RID: 706
		public static Predicate predicateNegateMod;

		// Token: 0x040002C3 RID: 707
		public static Predicate predicateAnimate;

		// Token: 0x040002C4 RID: 708
		public static Predicate predicatePulledByMagnet;

		// Token: 0x040002C5 RID: 709
		public static Predicate predicatePulledByMagnetModel;

		// Token: 0x040002C6 RID: 710
		public static Predicate predicatePushedByMagnet;

		// Token: 0x040002C7 RID: 711
		public static Predicate predicatePushedByMagnetModel;

		// Token: 0x040002C8 RID: 712
		public static Predicate predicateGameOver;

		// Token: 0x040002C9 RID: 713
		public static Predicate predicateGameWin;

		// Token: 0x040002CA RID: 714
		public static Predicate predicateGameLose;

		// Token: 0x040002CB RID: 715
		public static Predicate predicateTag;

		// Token: 0x040002CC RID: 716
		public static Predicate predicateWithinTaggedBlock;

		// Token: 0x040002CD RID: 717
		public static Predicate predicateCustomTag;

		// Token: 0x040002CE RID: 718
		public static Predicate predicateObjectCounterIncrement;

		// Token: 0x040002CF RID: 719
		public static Predicate predicateObjectCounterDecrement;

		// Token: 0x040002D0 RID: 720
		public static Predicate predicateObjectCounterEquals;

		// Token: 0x040002D1 RID: 721
		public static Predicate predicateObjectCounterEqualsMax;

		// Token: 0x040002D2 RID: 722
		public static Predicate predicateObjectCounterValueCondition;

		// Token: 0x040002D3 RID: 723
		public static Predicate predicateCounterEquals;

		// Token: 0x040002D4 RID: 724
		public static Predicate predicateCounterValueCondition;

		// Token: 0x040002D5 RID: 725
		public static Predicate predicateGaugeValueCondition;

		// Token: 0x040002D6 RID: 726
		public static Predicate predicateTimerEquals;

		// Token: 0x040002D7 RID: 727
		public static Predicate predicateTimerValueCondition;

		// Token: 0x040002D8 RID: 728
		public static Predicate predicateIsTreasure;

		// Token: 0x040002D9 RID: 729
		public static Predicate predicateIsTreasureForTag;

		// Token: 0x040002DA RID: 730
		public static Predicate predicateIsPickup;

		// Token: 0x040002DB RID: 731
		public static Predicate predicateIsPickupForTag;

		// Token: 0x040002DC RID: 732
		public static Predicate predicateWithinWater;

		// Token: 0x040002DD RID: 733
		public static Predicate predicateModelWithinWater;

		// Token: 0x040002DE RID: 734
		public static Predicate predicateWithinTaggedWater;

		// Token: 0x040002DF RID: 735
		public static Predicate predicateModelWithinTaggedWater;

		// Token: 0x040002E0 RID: 736
		public static Predicate predicateSetSpawnPoint;

		// Token: 0x040002E1 RID: 737
		public static Predicate predicateSetActiveCheckpoint;

		// Token: 0x040002E2 RID: 738
		public static Predicate predicateSpawn;

		// Token: 0x040002E3 RID: 739
		public static Predicate predicateWait;

		// Token: 0x040002E4 RID: 740
		public static Predicate predicateWaitTime;

		// Token: 0x040002E5 RID: 741
		public static Predicate predicateFirstFrame;

		// Token: 0x040002E6 RID: 742
		public static Predicate predicateLevitate;

		// Token: 0x040002E7 RID: 743
		public static Predicate predicateSpeak;

		// Token: 0x040002E8 RID: 744
		public static Predicate predicateTiltLeftRight;

		// Token: 0x040002E9 RID: 745
		public static Predicate predicateTiltFrontBack;

		// Token: 0x040002EA RID: 746
		public static Predicate predicateButton;

		// Token: 0x040002EB RID: 747
		public static Predicate predicateDPadHorizontal;

		// Token: 0x040002EC RID: 748
		public static Predicate predicateDPadVertical;

		// Token: 0x040002ED RID: 749
		public static Predicate predicateDPadMoved;

		// Token: 0x040002EE RID: 750
		public static Predicate predicateSendSignal;

		// Token: 0x040002EF RID: 751
		public static Predicate predicateSendCustomSignal;

		// Token: 0x040002F0 RID: 752
		public static Predicate predicateSendSignalModel;

		// Token: 0x040002F1 RID: 753
		public static Predicate predicateSendCustomSignalModel;

		// Token: 0x040002F2 RID: 754
		public static Predicate predicateVariableCustomInt;

		// Token: 0x040002F3 RID: 755
		public static Predicate predicateBlockVariableInt;

		// Token: 0x040002F4 RID: 756
		public static Predicate predicateBlockVariableIntLoadGlobal;

		// Token: 0x040002F5 RID: 757
		public static Predicate predicateBlockVariableIntStoreGlobal;

		// Token: 0x040002F6 RID: 758
		public static HashSet<Predicate> customVariablePredicates;

		// Token: 0x040002F7 RID: 759
		public static HashSet<Predicate> globalVariableOperations;

		// Token: 0x040002F8 RID: 760
		public static HashSet<Predicate> blockVariableOperations;

		// Token: 0x040002F9 RID: 761
		public static HashSet<Predicate> blockVariableOperationsOnGlobals;

		// Token: 0x040002FA RID: 762
		public static HashSet<Predicate> blockVariableOperationsOnOtherBlockVars;

		// Token: 0x040002FB RID: 763
		public static Dictionary<Predicate, int> variablePredicateParamDefaults;

		// Token: 0x040002FC RID: 764
		public static Dictionary<Predicate, string> variablePredicateLabels;

		// Token: 0x040002FD RID: 765
		public static Predicate predicateTapBlock;

		// Token: 0x040002FE RID: 766
		public static Predicate predicateTapChunk;

		// Token: 0x040002FF RID: 767
		public static Predicate predicateTapModel;

		// Token: 0x04000300 RID: 768
		public static Predicate predicateTapHoldBlock;

		// Token: 0x04000301 RID: 769
		public static Predicate predicateTapHoldModel;

		// Token: 0x04000302 RID: 770
		public static Predicate predicateCamFollow;

		// Token: 0x04000303 RID: 771
		public static Predicate predicateTutorialHelpTextAction;

		// Token: 0x04000304 RID: 772
		public static HashSet<Predicate> noTilesAfterPredicates = new HashSet<Predicate>();

		// Token: 0x04000305 RID: 773
		public static Dictionary<string, Vector3[]> defaultTextureNormals = null;

		// Token: 0x04000306 RID: 774
		public static Dictionary<string, bool[]> blockTypeCanBeTextured = null;

		// Token: 0x04000307 RID: 775
		public static Dictionary<string, bool[]> blockTypeCanBeMaterialTextured = new Dictionary<string, bool[]>();

		// Token: 0x04000308 RID: 776
		public static Dictionary<string, Vector3> defaultScales = new Dictionary<string, Vector3>();

		// Token: 0x04000309 RID: 777
		public static Dictionary<string, Vector3> defaultOrientations = new Dictionary<string, Vector3>();

		// Token: 0x0400030A RID: 778
		public static Dictionary<string, HashSet<string>> defaultSfxs = new Dictionary<string, HashSet<string>>();

		// Token: 0x0400030B RID: 779
		public static Dictionary<string, HashSet<string>> sameGridCellPairs = new Dictionary<string, HashSet<string>>();

		// Token: 0x0400030C RID: 780
		public static readonly HashSet<string> treasureBlocks = new HashSet<string>
		{
			"Coin",
			"Popsicle",
			"Feather",
			"Heart",
			"Key",
			"Energy Canister",
			"Amulet",
			"Idol",
			"Crystal Shard"
		};

		// Token: 0x0400030D RID: 781
		public static readonly string treasurePickupSfx = "SFX pick_up_sound_1";

		// Token: 0x0400030E RID: 782
		protected static Dictionary<string, List<List<Tile>>> defaultExtraTiles = new Dictionary<string, List<List<Tile>>>();

		// Token: 0x0400030F RID: 783
		protected static Dictionary<string, Action<List<List<Tile>>>> defaultExtraTilesProcessors = new Dictionary<string, Action<List<List<Tile>>>>();

		// Token: 0x04000310 RID: 784
		private static Dictionary<string, Vector3> blockSizes = new Dictionary<string, Vector3>();

		// Token: 0x04000311 RID: 785
		private static Dictionary<string, float[]> blockMassConstants;

		// Token: 0x04000312 RID: 786
		private static ScriptRowExecutionInfo resetExecutionInfo = new ScriptRowExecutionInfo();

		// Token: 0x04000313 RID: 787
		private static HashSet<Predicate> notPlayModePredicates;

		// Token: 0x04000314 RID: 788
		private int shadowUpdateInterval = 1;

		// Token: 0x04000315 RID: 789
		private int shadowUpdateCounter;

		// Token: 0x04000316 RID: 790
		private static Bounds shadowBounds = default(Bounds);

		// Token: 0x04000317 RID: 791
		private const float CAMERA_MOVE_THRESHOLD_SQR = 1.00000011E-06f;

		// Token: 0x04000318 RID: 792
		private const float CAMERA_ROT_THRESHOLD_SQR = 0.0001f;

		// Token: 0x04000319 RID: 793
		private static HashSet<string> emptySet = new HashSet<string>();

		// Token: 0x0400031A RID: 794
		public static Dictionary<string, Vector3> canScales;

		// Token: 0x0400031B RID: 795
		protected static HashSet<string> skinPaints = new HashSet<string>
		{
			"Light Magenta",
			"Magenta",
			"Light Pink",
			"Pink",
			"Dark Pink",
			"Light Red",
			"Deep Red",
			"Light Orange",
			"Earth Orange",
			"Light Yellow",
			"Dark Yellow",
			"Light Ginger",
			"Gingerbread Brown",
			"Dark Ginger",
			"Tan",
			"Beige",
			"Brown"
		};

		// Token: 0x0400031C RID: 796
		public static Dictionary<Block, List<Block>> connectedCache = new Dictionary<Block, List<Block>>();

		// Token: 0x0400031D RID: 797
		public static Dictionary<Block, HashSet<Chunk>> connectedChunks = new Dictionary<Block, HashSet<Chunk>>();

		// Token: 0x0400031E RID: 798
		private static HashSet<Predicate> keepRBPreds = null;

		// Token: 0x0400031F RID: 799
		private static DelegateMultistepCommand stepInvincibilityCommand;

		// Token: 0x04000320 RID: 800
		protected static UnmuteAllBlocksCommand unmuteCommand = new UnmuteAllBlocksCommand();

		// Token: 0x04000321 RID: 801
		private static LoopSfxCommand loopSfxCommand = new LoopSfxCommand();

		// Token: 0x04000322 RID: 802
		public static HashSet<Block> vanishingOrAppearingBlocks = new HashSet<Block>();

		// Token: 0x04000323 RID: 803
		private const float SPEECH_BUBBLE_DELAY = 0.4f;

		// Token: 0x04000324 RID: 804
		private bool wasHorizNegative;

		// Token: 0x04000325 RID: 805
		private bool wasHorizPositive;

		// Token: 0x04000326 RID: 806
		private bool wasVertNegative;

		// Token: 0x04000327 RID: 807
		private bool wasVertPositive;

		// Token: 0x04000328 RID: 808
		private const string CHUNK_GLUE_ON_CONTACT_SIZE = "CGOCS";

		// Token: 0x04000329 RID: 809
		private static UpdateBlockIsTriggerCommand updateBlockTriggerCommand = new UpdateBlockIsTriggerCommand();

		// Token: 0x0400032A RID: 810
		private const string RANDOM_WAIT_TIME = "RandomWaitTime";

		// Token: 0x0400032B RID: 811
		private const string RANDOM_WAIT_TIME_SENSOR = "RandomWaitTimeSensor";

		// Token: 0x0400032C RID: 812
		private static StopScriptsCommand stopScriptsCommand = new StopScriptsCommand();

		// Token: 0x0400032D RID: 813
		private static UnlockInputCommand unlockInputCommand = new UnlockInputCommand();

		// Token: 0x0400032E RID: 814
		public static Color transparent = new Color(0f, 0f, 0f, 0f);

		// Token: 0x0400032F RID: 815
		private static Dictionary<string, Vector3[]> blockScaleConstraints = new Dictionary<string, Vector3[]>();

		// Token: 0x04000330 RID: 816
		public static Dictionary<string, List<HashSet<string>>> animationSupports;

		// Token: 0x04000331 RID: 817
		private List<GameObject> fakeRigidbodyGos;

		// Token: 0x04000332 RID: 818
		private static HashSet<Predicate> taggedBumpPreds;

		// Token: 0x04000333 RID: 819
		protected AudioClip loopClip;

		// Token: 0x04000334 RID: 820
		protected string loopName = string.Empty;

		// Token: 0x04000335 RID: 821
		public static Dictionary<string, Type> blockNameTypeMap = new Dictionary<string, Type>
		{
			{
				"Master",
				typeof(BlockMaster)
			},
			{
				"Orrery",
				typeof(BlockWorldJumper)
			},
			{
				"Highscore I",
				typeof(BlockHighscoreList)
			},
			{
				"UI Counter I",
				typeof(BlockCounterUI)
			},
			{
				"UI Counter II",
				typeof(BlockCounterUI)
			},
			{
				"UI Object Counter I",
				typeof(BlockObjectCounterUI)
			},
			{
				"UI Gauge I",
				typeof(BlockGaugeUI)
			},
			{
				"UI Gauge II",
				typeof(BlockGaugeUI)
			},
			{
				"UI Radar I",
				typeof(BlockRadarUI)
			},
			{
				"UI Timer I",
				typeof(BlockTimerUI)
			},
			{
				"UI Timer II",
				typeof(BlockTimerUI)
			},
			{
				"Rocket",
				typeof(BlockRocket)
			},
			{
				"Rocket Square",
				typeof(BlockRocketSquare)
			},
			{
				"Rocket Octagonal",
				typeof(BlockRocketOctagonal)
			},
			{
				"Missile A",
				typeof(BlockMissile)
			},
			{
				"Missile B",
				typeof(BlockMissile)
			},
			{
				"Missile C",
				typeof(BlockMissile)
			},
			{
				"Missile Control",
				typeof(BlockMissileControl)
			},
			{
				"Missile Control Model",
				typeof(BlockModelMissileControl)
			},
			{
				"Jet Engine",
				typeof(BlockJetEngine)
			},
			{
				"Wheel",
				typeof(BlockWheel)
			},
			{
				"RAR Moon Rover Wheel",
				typeof(BlockWheel)
			},
			{
				"Raycast Wheel",
				typeof(BlockRaycastWheel)
			},
			{
				"Wheel Generic1",
				typeof(BlockTwoSidedWheel)
			},
			{
				"Wheel Generic2",
				typeof(BlockTwoSidedWheel)
			},
			{
				"Wheel Semi1",
				typeof(BlockTwoSidedWheel)
			},
			{
				"Wheel Semi2",
				typeof(BlockTwoSidedWheel)
			},
			{
				"Wheel Monster1",
				typeof(BlockTwoSidedWheel)
			},
			{
				"Wheel Monster2",
				typeof(BlockTwoSidedWheel)
			},
			{
				"Wheel Monster3",
				typeof(BlockTwoSidedWheel)
			},
			{
				"Wheel 6 Spoke",
				typeof(BlockTwoSidedWheel)
			},
			{
				"Wheel Pinwheel",
				typeof(BlockTwoSidedWheel)
			},
			{
				"Wheel BasketWeave",
				typeof(BlockTwoSidedWheel)
			},
			{
				"Bulky Wheel",
				typeof(BlockBulkyWheel)
			},
			{
				"Spoked Wheel",
				typeof(BlockSpokedWheel)
			},
			{
				"Golden Wheel",
				typeof(BlockWheelBling)
			},
			{
				"Sphere",
				typeof(BlockSphere)
			},
			{
				"Geodesic Ball",
				typeof(BlockSphere)
			},
			{
				"Soccer Ball",
				typeof(BlockSphere)
			},
			{
				"Motor",
				typeof(BlockMotor)
			},
			{
				"Motor Cube",
				typeof(BlockMotorCube)
			},
			{
				"Motor Slab",
				typeof(BlockMotorSlab)
			},
			{
				"Motor Slab 2",
				typeof(BlockMotorSlab2)
			},
			{
				"Motor Spindle",
				typeof(BlockMotorSpindle)
			},
			{
				"Piston",
				typeof(BlockPiston)
			},
			{
				"Magnet",
				typeof(BlockMagnet)
			},
			{
				"Torsion Spring",
				typeof(BlockTorsionSpring)
			},
			{
				"Torsion Spring Slab",
				typeof(BlockTorsionSpring)
			},
			{
				"Torsion Spring Cube",
				typeof(BlockTorsionSpringCube)
			},
			{
				"Laser",
				typeof(BlockLaser)
			},
			{
				"Laser Cannon",
				typeof(BlockLaserCannon)
			},
			{
				"Laser Blaster",
				typeof(BlockLaserBlaster)
			},
			{
				"Laser Octagonal",
				typeof(BlockOctagonalLaser)
			},
			{
				"Jazz Gun",
				typeof(BlockJazzGun)
			},
			{
				"Bumblebee Gun",
				typeof(BlockBumblebeeGun)
			},
			{
				"Megatron Gun",
				typeof(BlockMegatronGun)
			},
			{
				"Optimus Gun",
				typeof(BlockOptimusGun)
			},
			{
				"Soundwave Gun",
				typeof(BlockSoundwaveGun)
			},
			{
				"Starscream Gun",
				typeof(BlockStarscreamGun)
			},
			{
				"Laser Minigun",
				typeof(BlockLaserMiniGun)
			},
			{
				"GIJ Minigun",
				typeof(BlockMiniGun)
			},
			{
				"Laser Rifle",
				typeof(BlockLaserRifle)
			},
			{
				"Hand Cannon",
				typeof(BlockHandCannon)
			},
			{
				"Laser Pistol",
				typeof(BlockLaserPistol)
			},
			{
				"Laser Pistol2",
				typeof(BlockLaserPistol2)
			},
			{
				"BBG Ray Gun",
				typeof(BlockLaserPistol2)
			},
			{
				"FUT Space Gun",
				typeof(BlockLaserPistol2)
			},
			{
				"Emitter",
				typeof(BlockEmitter)
			},
			{
				"Stabilizer",
				typeof(BlockStabilizer)
			},
			{
				"Stabilizer Square",
				typeof(BlockSquareStabilizer)
			},
			{
				"Moving Platform",
				typeof(BlockMovingPlatform)
			},
			{
				"Rotating Platform",
				typeof(BlockRotatingPlatform)
			},
			{
				"Antigravity Pump",
				typeof(BlockAntiGravity)
			},
			{
				"Antigravity Cube",
				typeof(BlockAntiGravity)
			},
			{
				"Antigravity Column",
				typeof(BlockAntiGravityColumn)
			},
			{
				"Steering Wheel",
				typeof(BlockSteeringWheel)
			},
			{
				"Flight Yoke",
				typeof(BlockFlightYoke)
			},
			{
				"Bat Wing",
				typeof(BlockBatWing)
			},
			{
				"Fairy Wings",
				typeof(BlockFairyWings)
			},
			{
				"Cape",
				typeof(BlockCape)
			},
			{
				"BBG Cape",
				typeof(BlockCape)
			},
			{
				"SPY Jet Pack",
				typeof(BlockJetpack)
			},
			{
				"RAR Jet Pack",
				typeof(BlockJetpack)
			},
			{
				"Bat Wing Backpack",
				typeof(BlockBatWingBackpack)
			},
			{
				"Bird Wing",
				typeof(BlockBirdWing)
			},
			{
				"Wiser Wing",
				typeof(BlockWiserWing)
			},
			{
				"MLP Wings",
				typeof(BlockMLPWings)
			},
			{
				"Drive Assist",
				typeof(BlockDriveAssist)
			},
			{
				"Legs",
				typeof(BlockLegs)
			},
			{
				"Raptor Legs",
				typeof(BlockLegs)
			},
			{
				"Legs Small",
				typeof(BlockQuadped)
			},
			{
				"MLP Body",
				typeof(BlockMLPLegs)
			},
			{
				"Character",
				typeof(BlockCharacter)
			},
			{
				"Character Male",
				typeof(BlockCharacter)
			},
			{
				"Character Avatar",
				typeof(BlockCharacter)
			},
			{
				"Character Profile",
				typeof(BlockCharacter)
			},
			{
				"Character Female Profile",
				typeof(BlockCharacter)
			},
			{
				"Anim Character Male Profile",
				typeof(BlockAnimatedCharacter)
			},
			{
				"Anim Character Female Profile",
				typeof(BlockAnimatedCharacter)
			},
			{
				"Anim Character Skeleton Profile",
				typeof(BlockAnimatedCharacter)
			},
			{
				"Character Skeleton",
				typeof(BlockCharacter)
			},
			{
				"Character Headless",
				typeof(BlockCharacter)
			},
			{
				"Character Female",
				typeof(BlockCharacter)
			},
			{
				"Character Female Dress",
				typeof(BlockCharacter)
			},
			{
				"Character Mini",
				typeof(BlockCharacter)
			},
			{
				"Character Mini Female",
				typeof(BlockCharacter)
			},
			{
				"Jukebox",
				typeof(BlockJukebox)
			},
			{
				"Cloud 1",
				typeof(BlockCloud)
			},
			{
				"Cloud 2",
				typeof(BlockCloud)
			},
			{
				"Volcano",
				typeof(BlockVolcano)
			},
			{
				"Water Cube",
				typeof(BlockWaterCube)
			},
			{
				"Teleport Volume Block",
				typeof(BlockTeleportVolumeBlock)
			},
			{
				"Volume Block",
				typeof(BlockVolumeBlock)
			},
			{
				"Volume Block No Glue",
				typeof(BlockVolumeBlock)
			},
			{
				"Volume Block Slab",
				typeof(BlockVolumeBlock)
			},
			{
				"Volume Block Slab No Glue",
				typeof(BlockVolumeBlock)
			},
			{
				"Water Emitter Block",
				typeof(BlockEmitterWater)
			},
			{
				"Fire Emitter Block",
				typeof(BlockEmitterFire)
			},
			{
				"Gas Emitter Block",
				typeof(BlockEmitterGas)
			},
			{
				"Campfire",
				typeof(BlockEmitterCampfire)
			}
		};

		// Token: 0x04000336 RID: 822
		protected MeshColliderInfo meshColliderInfo;

		// Token: 0x04000337 RID: 823
		private static HashSet<Predicate> possibleModelConflictingInputPredicates = null;

		// Token: 0x04000338 RID: 824
		private static HashSet<Predicate> inputPredicates = null;

		// Token: 0x04000339 RID: 825
		private static HashSet<Predicate> emptyPredicateSet = new HashSet<Predicate>();

        [CompilerGenerated]
        private static Dictionary<string, int> f__switch_map3;

        // Token: 0x0400033A RID: 826
        [CompilerGenerated]
		private static PredicateSensorDelegate f__mg_cache0;

		// Token: 0x0400033B RID: 827
		[CompilerGenerated]
		private static PredicateActionDelegate f__mg_cache1;

		// Token: 0x0400033C RID: 828
		[CompilerGenerated]
		private static PredicateSensorDelegate f__mg_cache2;

		// Token: 0x0400033D RID: 829
		[CompilerGenerated]
		private static PredicateActionDelegate f__mg_cache3;

		// Token: 0x0400033E RID: 830
		[CompilerGenerated]
		private static PredicateActionDelegate f__mg_cache4;

		// Token: 0x0400033F RID: 831
		[CompilerGenerated]
		private static PredicateActionDelegate f__mg_cache5;

		// Token: 0x04000492 RID: 1170
		[CompilerGenerated]
		private static Action<Block, string, float, float> f__mg_cache6;

		// Token: 0x04000493 RID: 1171
		[CompilerGenerated]
		private static Action<Block, string, float, float> f__mg_cache7;

		// Token: 0x04000495 RID: 1173
		[CompilerGenerated]
		private static Action<Block> f__mg_cache8;
	}
}
