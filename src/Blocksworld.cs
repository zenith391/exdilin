using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using Blocks;
using Gestures;
using SimpleJSON;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using Exdilin;

// Token: 0x020001F7 RID: 503
public class Blocksworld : MonoBehaviour
{
	// Token: 0x17000076 RID: 118
	// (get) Token: 0x060018DD RID: 6365 RVA: 0x000AF9DE File Offset: 0x000ADDDE
	public static Camera mainCamera
	{
		get
		{
			return Blocksworld._mainCamera;
		}
	}

	// Token: 0x17000077 RID: 119
	// (get) Token: 0x060018DE RID: 6366 RVA: 0x000AF9E5 File Offset: 0x000ADDE5
	public static UIMain UI
	{
		get
		{
			return Blocksworld.bw._ui;
		}
	}

	// Token: 0x17000078 RID: 120
	// (get) Token: 0x060018DF RID: 6367 RVA: 0x000AF9F1 File Offset: 0x000ADDF1
	public static State CurrentState
	{
		get
		{
			return Blocksworld._currentState;
		}
	}

	// Token: 0x17000079 RID: 121
	// (get) Token: 0x060018E0 RID: 6368 RVA: 0x000AF9F8 File Offset: 0x000ADDF8
	private static State LastState
	{
		get
		{
			return Blocksworld._lastState;
		}
	}

	// Token: 0x060018E1 RID: 6369 RVA: 0x000AF9FF File Offset: 0x000ADDFF
	public static void SetBlocksworldState(State nextState)
	{
		Blocksworld._lastState = Blocksworld._currentState;
		Blocksworld._currentState = nextState;
		Blocksworld._stateTime = 0f;
		Blocksworld.musicPlayer.SetEnabled(Blocksworld.IsMusicEnabledForState());
	}

	// Token: 0x060018E2 RID: 6370 RVA: 0x000AFA2A File Offset: 0x000ADE2A
	public static float TimeInCurrentState()
	{
		return Blocksworld._stateTime;
	}

	// Token: 0x060018E3 RID: 6371 RVA: 0x000AFA31 File Offset: 0x000ADE31
	public static bool IsMusicEnabledForState()
	{
		return Blocksworld.CurrentState != State.Background;
	}

	// Token: 0x060018E4 RID: 6372 RVA: 0x000AFA40 File Offset: 0x000ADE40
	public static void Bootstrap(GameObject go)
	{
		Component component = go.GetComponent("BlocksworldComponentData");
		Blocksworld.guiCamera = (Camera)component.GetType().GetField("guiCamera").GetValue(component);
		Blocksworld.rewardCamera = (Camera)component.GetType().GetField("rewardCamera").GetValue(component);
		Blocksworld.buttonPlus = (Texture)component.GetType().GetField("buttonPlus").GetValue(component);
		Blocksworld.buttonMinus = (Texture)component.GetType().GetField("buttonMinus").GetValue(component);
		Blocksworld.prefabArrow = (GameObject)component.GetType().GetField("prefabArrow").GetValue(component);
		Blocksworld.stars = (ParticleSystem)component.GetType().GetField("stars").GetValue(component);
		Blocksworld.starsReward = (ParticleSystem)component.GetType().GetField("starsReward").GetValue(component);
		Blocksworld blocksworld = go.AddComponent<Blocksworld>();
		blocksworld.autoLoad = (BWStandalone.Instance == null);
	}

	// Token: 0x060018E5 RID: 6373 RVA: 0x000AFB54 File Offset: 0x000ADF54
	public static string DefaultProfileWorldAssetPath()
	{
		return "ProfileWorlds/profile_world_source_anim_male";
	}

	// Token: 0x060018E6 RID: 6374 RVA: 0x000AFB68 File Offset: 0x000ADF68
	private void Awake()
	{
		BWLog.Info("Awaking Blocksworld..");
		Blocksworld.bw = this;
		Blocksworld.cameraTransform = GameObject.Find("Camera Holder").transform;
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("Main Camera")) as GameObject;
		Blocksworld._mainCamera = gameObject.GetComponent<Camera>();
		this._defaultBackgroundColor = Blocksworld._mainCamera.backgroundColor;
		Blocksworld.mainCamera.transform.parent = Blocksworld.cameraTransform;
		Blocksworld.mainCamera.transform.position = Blocksworld.cameraTransform.position;
		Blocksworld.mainCamera.transform.rotation = Blocksworld.cameraTransform.rotation;
		Blocksworld.blocksworldDataContainer = GameObject.Find("BlocksworldData");
		Blocksworld.sfxDefinitions = Blocksworld.blocksworldDataContainer.GetComponent<SfxDefinitions>();
		Blocksworld.engineSoundDefinitions = Blocksworld.blocksworldDataContainer.GetComponent<EngineSoundDefinitions>();
		Blocksworld.componentData = Blocksworld.blocksworldDataContainer.GetComponent<BlocksworldComponentData>();
		Blocksworld.leaderboardData = Blocksworld.blocksworldDataContainer.GetComponent<LeaderboardData>();
		Shader.globalMaximumLOD = 1000;
		Blocksworld.renderingShadows = (QualitySettings.shadows != ShadowQuality.Disable);
		Blocksworld.renderingWater = true;
		Blocksworld.renderingSkybox = true;
		Blocksworld._mainCamera.clearFlags = CameraClearFlags.Skybox;
		AmplifyOcclusionEffect component = gameObject.GetComponent<AmplifyOcclusionEffect>();
		if (component != null)
		{
			component.enabled = Blocksworld.renderingShadows;
		}
		BWFog component2 = gameObject.GetComponent<BWFog>();
		if (component2 != null)
		{
			component2.enabled = Blocksworld.renderingShadows;
		}
		BWLog.Info("Done awaking Blocksworld!");
	}

	// Token: 0x060018E7 RID: 6375 RVA: 0x000AFCC0 File Offset: 0x000AE0C0
	private IEnumerator Start()
	{
        yield return null;
		TiltManager.Instance.Init();
		PerformaceTest speedTest = new PerformaceTest("Load Main Scene");
		speedTest.Start();
		Blocksworld.screenScale = NormalizedScreen.scale;
		Blocksworld.hd = (Blocksworld.screenScale > 1f);
		BWLog.Info("Creating UI..");
		this._ui = UIMain.CreateUI();
		Blocksworld.UI.gameObject.SetActive(false);
		Blocksworld.SetupCameraHierarchy();
		yield return null;
		Blocksworld.fixedDeltaTime = Time.fixedDeltaTime;
		this.tileParameterEditor = base.gameObject.AddComponent<TileParameterEditor>();
		this.tileParameterEditor.enabled = false;
		SymbolCompat.Init();
		BWLog.Info("Initializing settings..");
		Blocksworld.useCompactGafWriteRenamings = Options.UseCompactGafWriteRenamings;
		Blocksworld.screenWidth = NormalizedScreen.width;
		Blocksworld.screenHeight = NormalizedScreen.height;
		Blocksworld.skin = Blocksworld.LoadSkin(Blocksworld.hd);
		Blocksworld.guiCamera.transform.position = new Vector3((float)(Blocksworld.screenWidth / 2), (float)(Blocksworld.screenHeight / 2), -1f);
		Blocksworld.guiCamera.orthographicSize = (float)(Blocksworld.screenHeight / 2);
		Blocksworld.guiCamera.transparencySortMode = TransparencySortMode.Orthographic;
		speedTest.StartSubTest("ReadScarcityInfo");
		Scarcity.ReadScarcityInfo();
		speedTest.StopSubTest();
		BWLog.Info("Initializing music..");
		Blocksworld.musicPlayer = MusicPlayer.Create();
		Blocksworld.UpdateEditorMusicPlayerEnabled();
		ResourceLoader.UpdateTextureInfos();
		Blocksworld.SetupColorDefinitions(Blocksworld.colorDefinitions, Blocksworld.ambientLightGradientDefinitions, Blocksworld.skyBoxTintDefinitions);
		Blocksworld.SetupRarityBorders(Blocksworld.rarityBorderMaterialsEnabled, Blocksworld.rarityBorderMaterialsDisabled);
		this.SetupTextureMetaDatas();
		speedTest.StartSubTest("RegisterBlocks");
		BWLog.Info("Registering blocks..");
		Blocksworld.RegisterBlocks();
        foreach (Mod mod in ModLoader.mods)
        {
            Mod.ExecutionMod = mod;
            mod.Register(RegisterType.BLOCKS);
            Mod.ExecutionMod = null;
        }
        speedTest.StopSubTest();
		BlockItem.LoadBlockItemsFromResources();
		PanelSlots.LoadPanelSlotsFromResources();
		CollisionTest.ReadNoShapeCollides();
		PredicateRegistry.UpdateEquivalentPredicates();
		Blocksworld.modelCollection = new ModelCollection();
		speedTest.StartSubTest("Block.Init");
		Block.Init();
		speedTest.StopSubTest();
		yield return null;
		BWLog.Info("Loading blocks..");
		speedTest.StartSubTest("LoadBlocksFromResources");
		ResourceLoader.LoadBlocksFromResources("Blocks");
		speedTest.StopSubTest();
		speedTest.StartSubTest("LoadSFXNames");
		ResourceLoader.LoadSFXNames();
		speedTest.StopSubTest();
		BWLog.Info("Initializing materials..");
		speedTest.StartSubTest("Materials.Init");
		Materials.Init();
		speedTest.StopSubTest();
		ActionDebug.Init();
		Blocksworld.globalGafs = GAF.GetAllGlobalGAFs();
		yield return null;
		BWLog.Info("Loading tile data..");
		speedTest.StartSubTest("LoadTileData");
        foreach (Mod mod in ModLoader.mods)
        {
            Mod.ExecutionMod = mod;
            mod.Register(RegisterType.BLOCK_ITEMS);
            Mod.ExecutionMod = null;
        }
        TileIconManager.Init();
		speedTest.StopSubTest();
		TileToggleChain.LoadTileToggleChains();
		speedTest.StartSubTest("UpdateTileParameterSettings");
		Tile.CreateAntigravityArgumentConverters();
        foreach (Mod mod in ModLoader.mods)
        {
            mod.Register(RegisterType.TILE_PARAMETERS);
        }
        Tile.UpdateTileParameterSettings();
		speedTest.StopSubTest();
		BWLog.Info("Creating tile pools..");
		Blocksworld.tilePool = new TilePool("Blocksworld", 256, TilePool.TileImageSource.Resources);
		Blocksworld.modelTilePool = new TilePool("ModelTiles", 256, TilePool.TileImageSource.StandaloneImageManger);
		yield return null;
		EntityTagsRegistry.Read();
		Blocksworld.tileButtonClearScript = new Tile(Blocksworld.tilePool.GetTileObjectForIcon("Buttons/Clear_Script", true));
		Blocksworld.tileButtonCopyScript = new Tile(Blocksworld.tilePool.GetTileObjectForIcon("Buttons/Copy_Script", true));
		Blocksworld.tileButtonPasteScript = new Tile(Blocksworld.tilePool.GetTileObjectForIcon("Buttons/Paste_Script", true));
		speedTest.StartSubTest("create build panel");
		int num = 3;
		if (BW.isUnityEditor)
		{
			num = Options.PanelColumnCount;
			if (num == 0)
			{
				num = 3;
			}
			else
			{
				num = Mathf.Clamp(num, 2, 20);
			}
		}
		BWLog.Info("Creating build panel..");
		Blocksworld.buildPanel = new BuildPanel("Build Panel", num);
		Blocksworld.buildPanel.UpdatePosition();
		Blocksworld.buildPanel.depth = 20f;
		speedTest.StopSubTest();
		BWLog.Info("Creating script panel..");
		Blocksworld.scriptPanel = new ScriptPanel("Script Panel");
		Blocksworld.scriptPanel.depth = 10f;
		Blocksworld.scriptPanel.Show(false);
		Blocksworld.SetupClipboard();
		CharacterStateHandler.ClearStateMap();
		BWLog.Info("Loading character data..");
		TextAsset stateMap = Resources.Load<TextAsset>("StateHandler/Character");
		if (null != stateMap)
		{
			CharacterStateHandler.LoadStateMap(stateMap.text, CharacterRole.Male);
		}
		TextAsset upperBodyMap = Resources.Load<TextAsset>("StateHandler/UpperBody");
		if (null != upperBodyMap)
		{
			UpperBodyStateHandler.LoadStateMap(upperBodyMap.text, CharacterRole.Male);
		}
		for (CharacterRole characterRole = CharacterRole.Male; characterRole < CharacterRole.None; characterRole++)
		{
			stateMap = Resources.Load<TextAsset>("StateHandler/Character" + characterRole.ToString());
			if (null != stateMap)
			{
				CharacterStateHandler.LoadStateMap(stateMap.text, characterRole);
			}
			upperBodyMap = Resources.Load<TextAsset>("StateHandler/UpperBody" + characterRole.ToString());
			if (null != upperBodyMap)
			{
				UpperBodyStateHandler.LoadStateMap(upperBodyMap.text, characterRole);
			}
		}
		yield return null;
		BWLog.Info("Creating GUI..");
		WorldUILayout.Init();
		Blocksworld.blocksworldCamera.Init();
		this.InitializeLights();
		Tutorial.Init();
		TBox.Init();
		Blocksworld.buildPanel.PositionReset(false);
		speedTest.StartSubTest("HudMesh.Init");
		HudMeshOnGUI.Init();
		speedTest.StopSubTest();
		Blocksworld.rewardStarburst = (UnityEngine.Object.Instantiate(Resources.Load("Blocks/Block Starburst System")) as GameObject);
		Blocksworld.rewardStarburst.SetLayer(Layer.Rewards, false);
		Blocksworld.uiGesture = new UIGesture();
		Blocksworld.blockDupeGesture = new BlockDuplicateGesture(new BlockDuplicateBeginDelegate(this.BlockDuplicateBegan), new BlockDuplicateGestureDelegate(this.BlockDuplicated));
		Blocksworld.blockTapGesture = new BlockTapGesture(new BlockTapBeginDelegate(this.BlockTapBegan), new BlockTapGestureDelegate(this.BlockTapped));
		Blocksworld.buttonTapGesture = new ButtonTapGesture(Blocksworld.buildPanel);
		Blocksworld.tileDragGesture = new TileDragGesture(Blocksworld.buildPanel, Blocksworld.scriptPanel);
		Blocksworld.createTileDragGesture = new CreateTileDragGesture(Blocksworld.buildPanel);
		Blocksworld.replaceBodyPartGesture = new ReplaceBodyPartTileDragGesture(Blocksworld.buildPanel);
		Blocksworld.characterEditGearGesture = new CharacterEditGearGesture(Blocksworld.buildPanel);
		PanelScrollGesture panelScroll = new PanelScrollGesture(Blocksworld.buildPanel, Blocksworld.scriptPanel);
		PanelMoveGesture panelMove = new PanelMoveGesture(Blocksworld.scriptPanel);
		this.pullObjectLineRenderer = Blocksworld.mainCamera.GetComponent<LineRenderer>();
		this.pullObject = new PullObjectGesture(this.pullObjectLineRenderer);
		this.autoPlayGesture = new SecretCommandGesture();
		this.tapControl = new TapControlGesture();
		Blocksworld.tBoxGesture = new TBoxGesture();
		Blocksworld.cWidgetGesture = new CWidgetGesture();
		this.parameterEditGesture = new ParameterEditGesture();
		TileTapGesture clearScriptGesture = new TileTapGesture(Blocksworld.tileButtonClearScript, new TileTapGestureDelegate(this.ButtonClearScriptTapped), false, false);
		clearScriptGesture.SetExtendedHit(-10f, -10f, -10f, -10f);
		TileTapGesture copyScriptGesture = new TileTapGesture(Blocksworld.tileButtonCopyScript, new TileTapGestureDelegate(this.ButtonCopyScriptTapped), false, false);
		copyScriptGesture.SetExtendedHit(-10f, -10f, -10f, -10f);
		TileTapGesture pasteScriptGesture = new TileTapGesture(Blocksworld.tileButtonPasteScript, new TileTapGestureDelegate(this.ButtonPasteScriptTapped), false, false);
		pasteScriptGesture.SetExtendedHit(-10f, -10f, -10f, -10f);
		Tile characterEditTile = new Tile(TBox.tileCharacterEditIcon);
		TileTapGesture characterEditGesture = new TileTapGesture(characterEditTile, new TileTapGestureDelegate(this.CharacterEditTapped), false, false);
		Tile characterEditExitTile = new Tile(TBox.tileCharacterEditExitIcon);
		TileTapGesture characterEditExitGesture = new TileTapGesture(characterEditExitTile, new TileTapGestureDelegate(this.CharacterEditExitTapped), false, false);
		List<BaseGesture> panelLayerList = new List<BaseGesture>
		{
			this.autoPlayGesture,
			this.parameterEditGesture,
			panelScroll,
			clearScriptGesture,
			copyScriptGesture,
			pasteScriptGesture,
			panelMove,
			Blocksworld.replaceBodyPartGesture,
			Blocksworld.characterEditGearGesture,
			Blocksworld.createTileDragGesture,
			Blocksworld.tileDragGesture,
			Blocksworld.buttonTapGesture
		};
		BaseGesture[] panelLayer = panelLayerList.ToArray();
		Blocksworld.orbitDuringControlGesture = new OrbitDuringControlCameraGesture();
		BWLog.Info("Creating mapped input..");
		MappedInput.Init();
		BWLog.Info("Loading key maps..");
		TextAsset buildInput = Resources.Load<TextAsset>("KeyMaps/default_keymap_build");
		if (null != buildInput)
		{
			MappedInput.ClearInputMap(MappableInputMode.Build);
			if (!MappedInput.AddInputMap(buildInput.text, MappableInputMode.Build))
			{
				BWLog.Error("Unable to add default build keymap!");
			}
		}
		TextAsset runtimeInput = Resources.Load<TextAsset>("KeyMaps/default_keymap_play");
		if (null != runtimeInput)
		{
			MappedInput.ClearInputMap(MappableInputMode.Play);
			if (!MappedInput.AddInputMap(runtimeInput.text, MappableInputMode.Play))
			{
				BWLog.Error("Unable to add default play keymap!");
			}
		}
		TextAsset menuInput = Resources.Load<TextAsset>("KeyMaps/default_keymap_menu");
		if (null != menuInput)
		{
			MappedInput.ClearInputMap(MappableInputMode.Menu);
			if (!MappedInput.AddInputMap(menuInput.text, MappableInputMode.Menu))
			{
				BWLog.Error("Unable to add default menu keymap!");
			}
		}
		MappedInput.SetMode(MappableInputMode.Menu);
		BaseGesture[] buttonLayer = new BaseGesture[]
		{
			Blocksworld.orbitDuringControlGesture
		};
		BaseGesture[] worldLayer = new BaseGesture[]
		{
			Blocksworld.blockDupeGesture,
			Blocksworld.blockTapGesture,
			this.tapControl,
			this.pullObject,
			Blocksworld.tBoxGesture,
			Blocksworld.cWidgetGesture,
			characterEditGesture,
			characterEditExitGesture
		};
		Blocksworld.recognizer.AddGesture(Blocksworld.uiGesture);
		foreach (BaseGesture[] array2 in new BaseGesture[][]
		{
			panelLayer,
			buttonLayer,
			worldLayer
		})
		{
			foreach (BaseGesture gesture in array2)
			{
				Blocksworld.recognizer.AddGesture(gesture);
			}
		}
		Blocksworld.recognizer.CancelsAll(Blocksworld.uiGesture, panelLayer);
		Blocksworld.recognizer.CancelsAll(Blocksworld.uiGesture, worldLayer);
		Blocksworld.recognizer.AnyCancelsAll(panelLayer, buttonLayer);
		Blocksworld.recognizer.AnyCancelsAll(panelLayer, worldLayer);
		Blocksworld.recognizer.AnyCancelsAll(buttonLayer, worldLayer);
		Blocksworld.recognizer.CancelsAll(this.parameterEditGesture, new BaseGesture[]
		{
			panelScroll,
			panelMove,
			Blocksworld.createTileDragGesture,
			Blocksworld.replaceBodyPartGesture,
			Blocksworld.characterEditGearGesture,
			Blocksworld.tileDragGesture
		});
		Blocksworld.recognizer.Cancels(Blocksworld.tBoxGesture, Blocksworld.tileDragGesture);
		Blocksworld.recognizer.Cancels(Blocksworld.replaceBodyPartGesture, Blocksworld.createTileDragGesture);
		Blocksworld.recognizer.Cancels(Blocksworld.replaceBodyPartGesture, Blocksworld.tileDragGesture);
		Blocksworld.recognizer.Cancels(Blocksworld.characterEditGearGesture, Blocksworld.createTileDragGesture);
		Blocksworld.recognizer.Cancels(Blocksworld.characterEditGearGesture, Blocksworld.tileDragGesture);
		Blocksworld.recognizer.Cancels(Blocksworld.createTileDragGesture, Blocksworld.tileDragGesture);
		Blocksworld.recognizer.Cancels(Blocksworld.replaceBodyPartGesture, Blocksworld.characterEditGearGesture);
		Blocksworld.recognizer.Cancels(Blocksworld.buttonTapGesture, panelScroll);
		Blocksworld.recognizer.Cancels(Blocksworld.tileDragGesture, panelMove);
		Blocksworld.recognizer.Cancels(Blocksworld.tBoxGesture, Blocksworld.blockTapGesture);
		Blocksworld.recognizer.Cancels(characterEditGesture, Blocksworld.blockTapGesture);
		Blocksworld.recognizer.Cancels(characterEditExitGesture, Blocksworld.blockTapGesture);
		this.buildOnlyGestures = new BaseGesture[]
		{
			Blocksworld.createTileDragGesture,
			Blocksworld.tileDragGesture,
			Blocksworld.replaceBodyPartGesture,
			Blocksworld.characterEditGearGesture,
			Blocksworld.blockDupeGesture,
			Blocksworld.blockTapGesture,
			this.autoPlayGesture
		};
		for (int k = 1; k <= Blocksworld.numCounters; k++)
		{
			string key = k + string.Empty;
			Blocksworld.countersActivated.Add(key, false);
			Blocksworld.counters.Add(key, 0);
			Blocksworld.counterTargets.Add(key, 1);
			Blocksworld.counterTargetsActivated.Add(key, false);
		}
		Blocksworld.starOutlineTexture = (Resources.Load("Particles/Counter Star Disabled") as Texture);
		Blocksworld.starTexture = (Resources.Load("Particles/Counter Star Enabled") as Texture);
		History.activated = true;
		Blocksworld.started = true;
		BlockGroups.Init();
		if (BW.isIPad)
		{
			IOSInterface.BlocksworldSceneLoaded();
		}
		BWLog.Info("Finished loading scene.");
		Blocksworld.isLoadingScene = false;
		Blocksworld.loadComplete = true;
		speedTest.Stop();
        speedTest.DebugLogTestResults();
        foreach (Mod mod in ModLoader.mods)
        {
            Mod.ExecutionMod = mod;
            mod.Register(RegisterType.SETTINGS);
            Mod.ExecutionMod = null;
        }
		foreach (Mod mod in ModLoader.mods) {
			Mod.ExecutionMod = mod;
			mod.Register(RegisterType.TEXTURES);
			Mod.ExecutionMod = null;
		}
		BWLog.Info("Initing mods..");
		foreach (Mod mod in ModLoader.mods) {
            Mod.ExecutionMod = mod;
            mod.Init();
            Mod.ExecutionMod = null;
        }
		StartCoroutine(ModLoader.ShowErrors());
        foreach (BlockItemEntry entry in BlockItemsRegistry.GetItemEntries())
        {
            BWUser.currentUser.blocksInventory.Add(entry.item.Id, entry.count, entry.infinite ? 1 : 0);
        }

		/*foreach (GameObject gameObject in Resources.LoadAll("Blocks", typeof(GameObject))) {
			if (gameObject.name.StartsWith("Prefab ")) {
				string item = gameObject.name.Substring("Prefab ".Length);
				BWLog.Info("Exporting " + item);
				MeshUtils.ExportGameObject("Blocks/" + item + ".obj", gameObject);
			}
		}*/
		BWLog.Info("Finished loading.");
		yield break;
	}

	// Token: 0x060018E8 RID: 6376 RVA: 0x000AFCDC File Offset: 0x000AE0DC
	private void InitializeLights()
	{
		Blocksworld.directionalLight = GameObject.Find("Directional light");
		GameObject gameObject = GameObject.Find("Lighting Rig");
		if (gameObject != null)
		{
			if (Blocksworld.renderingShadows)
			{
				Blocksworld.lightingRig = gameObject.transform;
				Light[] componentsInChildren = Blocksworld.lightingRig.GetComponentsInChildren<Light>(true);
				if (componentsInChildren.Length > 0)
				{
					Blocksworld.overheadLight = componentsInChildren[0];
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						componentsInChildren[i].gameObject.SetActive(true);
					}
					Blocksworld.directionalLight.SetActive(false);
				}
				else
				{
					gameObject.SetActive(false);
				}
			}
			else
			{
				gameObject.SetActive(false);
			}
		}
		else if (Blocksworld.renderingShadows)
		{
			BWLog.Warning("No lighting rig found for dynamic lights!");
		}
	}

	// Token: 0x060018E9 RID: 6377 RVA: 0x000AFDA0 File Offset: 0x000AE1A0
	public static bool HasWinCondition()
	{
		Predicate predicateGameWin = Block.predicateGameWin;
		List<Block> list = BWSceneManager.AllBlocks();
		for (int i = 0; i < list.Count; i++)
		{
			List<List<Tile>> tiles = list[i].tiles;
			for (int j = 0; j < tiles.Count; j++)
			{
				List<Tile> list2 = tiles[j];
				for (int k = 0; k < list2.Count; k++)
				{
					Predicate predicate = list2[k].gaf.Predicate;
					if (predicate == predicateGameWin)
					{
						object[] args = list2[k].gaf.Args;
						if (Util.GetFloatArg(args, 1, 0f) == 0f)
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	public static List<Dependency> GetRequiredMods() {
		List<Dependency> requiredMods = new List<Dependency>();
		List<Block> list = BWSceneManager.AllBlocks();

		// TODO: also count modded music, script and SFX predicates
		for (int i = 0; i < list.Count; i++) {
			string blockType = list[i].BlockType();
			BlockEntry entry = BlockItemsRegistry.GetBlockEntry(blockType);
			if (entry != null) {
				if (entry.originator != null) {
					Dependency dep = new Dependency(entry.originator.Id, entry.originator.Version);
					if (!requiredMods.Contains(dep)) {
						requiredMods.Add(dep);
					}
				} else {
					BWLog.Warning("Unknown mod originator for block " + blockType);
				}
			}
			List<List<Tile>> tiles = list[i].tiles;
			for (int j = 0; j < tiles.Count; j++) {
				List<Tile> list2 = tiles[j];
				for (int k = 0; k < list2.Count; k++) {
					Predicate predicate = list2[k].gaf.Predicate;
					if (predicate == Block.predicateTextureTo) {
						object[] args = list2[k].gaf.Args;
						string texture = Util.GetStringArg(args, 0, "Plain");
						Mapping mapping = Materials.GetMapping(texture);
						ShaderType shader = Materials.shaders[texture];
						Mod owner = AssetsManager.GetOwner("Textures/" + shader + "/" + mapping + "/" + texture);
						if (owner != null) {
							Dependency dep = new Dependency(owner.Id, owner.Version);
							if (!requiredMods.Contains(dep)) {
								requiredMods.Add(dep);
							}
						}
					}
				}
			}
		}
		return requiredMods;
	}

	// Token: 0x060018EA RID: 6378 RVA: 0x000AFE6C File Offset: 0x000AE26C
	public static void SetupCameraOverride(Transform newCameraPrefab, Block sourceBlock)
	{
		if (Blocksworld._mainCameraOverrideBackup != null)
		{
			BWLog.Error("Only one camera override can be active at a time!");
			return;
		}
		Blocksworld.TakedownVRCamera();
		Blocksworld._mainCamera.gameObject.SetActive(false);
		Blocksworld._mainCameraOverrideBackup = Blocksworld._mainCamera;
		Transform transform = UnityEngine.Object.Instantiate<Transform>(newCameraPrefab).transform;
		Blocksworld._mainCamera = transform.GetComponent<Camera>();
		Blocksworld.mainCamera.transform.parent = Blocksworld.cameraTransform;
		Blocksworld.mainCamera.transform.position = Blocksworld.cameraTransform.position;
		Blocksworld.mainCamera.transform.rotation = Blocksworld.cameraTransform.rotation;
		NightVisionColoring component = transform.GetComponent<NightVisionColoring>();
		if (component)
		{
			component.SetSourceBlock(sourceBlock);
		}
		Blocksworld.SetupVRCamera();
	}

	// Token: 0x060018EB RID: 6379 RVA: 0x000AFF30 File Offset: 0x000AE330
	public static void ResetCameraOverride()
	{
		if (Blocksworld._mainCameraOverrideBackup != null)
		{
			Blocksworld.TakedownVRCamera();
			UnityEngine.Object.Destroy(Blocksworld._mainCamera.gameObject);
			Blocksworld._mainCamera = Blocksworld._mainCameraOverrideBackup;
			Blocksworld._mainCamera.gameObject.SetActive(true);
			Blocksworld._mainCameraOverrideBackup = null;
			Blocksworld.SetupVRCamera();
		}
	}

	// Token: 0x060018EC RID: 6380 RVA: 0x000AFF86 File Offset: 0x000AE386
	public static void CleanupAndQuitToMenu()
	{
		Blocksworld.Cleanup(true);
		Blocksworld.UI.gameObject.SetActive(false);
	}

	// Token: 0x060018ED RID: 6381 RVA: 0x000AFFA0 File Offset: 0x000AE3A0
	public static void Cleanup(bool quitToMenu)
	{
		if (CharacterEditor.Instance.InEditMode())
		{
			CharacterEditor.Instance.Exit();
		}
		try
		{
			Blocksworld.CleanupScene();
			if (quitToMenu)
			{
				if (Blocksworld.modelCollection != null)
				{
					Blocksworld.modelCollection.Clear();
				}
				Blocksworld.buildPanel.ClearAllTiles();
			}
			Blocksworld.bw.StopAllCoroutines();
			ScreenshotUtils.OnStopAllCoroutines();
			Blocksworld.prefabs.Clear();
			Blocksworld.compoundColliders.Clear();
			Blocksworld.shapes.Clear();
			Blocksworld.glues.Clear();
			Blocksworld.joints.Clear();
			Blocksworld.leaderboardData.ClearWorldSession();
		}
		catch (Exception ex)
		{
			BWLog.Error("Exception in Blocksworld.CleanupAndQuitToMenu '" + ex.Message + "'");
		}
		finally
		{
			if (quitToMenu)
			{
				Blocksworld.worldSessionHadVR = false;
				Blocksworld.worldSessionHadBlocksterMover = false;
				Blocksworld.worldSessionHadBlocksterSpeaker = false;
				Blocksworld.worldSessionHadBlockTap = false;
				Blocksworld.worldSessionCoinsCollected = 0;
				Blocksworld.worldSessionHadHypderjumpUse = false;
				WorldSession.jumpRestoreConfig = null;
				WorldSession.current = null;
				Blocksworld.bw.StartCoroutine(Blocksworld.CoroutineWorldDidQuit());
			}
		}
	}

	// Token: 0x060018EE RID: 6382 RVA: 0x000B00C4 File Offset: 0x000AE4C4
	private static IEnumerator CoroutineWorldDidQuit()
	{
		yield return null;
		WorldSession.platformDelegate.WorldDidQuit();
		yield break;
	}

	// Token: 0x060018EF RID: 6383 RVA: 0x000B00D8 File Offset: 0x000AE4D8
	public void OpenEditorMenu()
	{
		Blocksworld.SetBlocksworldState(State.Menu);
		Blocksworld.canSaveInMenu = (Tutorial.state == TutorialState.None && !Blocksworld.f3PressedInCurrentWorld);
	}

	// Token: 0x060018F0 RID: 6384 RVA: 0x000B00FA File Offset: 0x000AE4FA
	public void ToggleEditorMenu()
	{
		if (Blocksworld.CurrentState == State.Build)
		{
			this.OpenEditorMenu();
		}
		else
		{
			Sound.PlayOneShotSound("Button Generic", 1f);
			Blocksworld.SetBlocksworldState(State.Build);
		}
	}

	// Token: 0x060018F1 RID: 6385 RVA: 0x000B0128 File Offset: 0x000AE528
	public static void CleanupScene()
	{
		BWSceneManager.Cleanup();
		if (WorldSession.isPuzzleBuildSession() || WorldSession.isPuzzlePlaySession())
		{
			Tutorial.Stop(false);
		}
		TileIconManager.Instance.CancelAllFileLoads();
		Blocksworld.winIsWaiting = false;
		Blocksworld.hasWon = false;
		Tutorial.ResetState();
		Blocksworld.bw.Stop(true, false);
		BW.Analytics.SendAnalyticsEvent("world-exit");
		Blocksworld.inBackground = true;
		if (BlockAbstractWater.waterCubes != null)
		{
			BlockAbstractWater.waterCubes.Clear();
		}
		BlockGroups.Clear();
		Blocksworld.worldSky = null;
		Blocksworld.worldOcean = null;
		Blocksworld.worldOceanBlock = null;
		WeatherEffect.ResetAll();
		Blocksworld.weather = WeatherEffect.clear;
		Blocksworld.bw.Reset();
		TBox.tileCharacterEditIcon.Hide();
		BlockProceduralCollider.ClearReuseableMeshes();
		Physics.gravity = -Blocksworld.defaultGravityStrength * Vector3.up;
		TileIconManager.Instance.labelAtlas.Clear();
		Materials.materialCache.Clear();
		Materials.materialCachePaint.Clear();
		Materials.materialCacheTexture.Clear();
		Blocksworld.RemoveUnusedBlockPrefabs();
		Blocksworld.RemoveUnusedTextures();
		RewardVisualization.Cancel();
		Blocksworld.rewardVisualizationGafs = null;
		Blocksworld.rewardVisualizationIndex = 0;
		Blocksworld.rewardExecutionInfo.timer = 0f;
		Blocksworld.UI.Tapedeck.Reset();
		Blocksworld.UI.SidePanel.Hide();
		WorldUILayout.HideAll();
		Blocksworld.musicPlayer.SetEnabled(false);
		if (Sound.oneShotAudioSource != null)
		{
			Sound.oneShotAudioSource.Stop();
			Sound.oneShotAudioSource.clip = null;
		}
		BlockAnimatedCharacter.stateControllers.Clear();
		Blocksworld.SetBlocksworldState(State.Background);
	}

	// Token: 0x060018F2 RID: 6386 RVA: 0x000B02B4 File Offset: 0x000AE6B4
	public static void EnableGameCameras(bool enableCams)
	{
		Blocksworld.mainCamera.gameObject.SetActive(enableCams);
		Blocksworld.mainCamera.transform.parent = Blocksworld.bw.cameraParentTransform;
		Blocksworld.mainCamera.transform.position = Blocksworld.cameraTransform.position;
		Blocksworld.mainCamera.transform.rotation = Blocksworld.cameraTransform.rotation;
		Blocksworld.guiCamera.gameObject.SetActive(enableCams);
	}

	// Token: 0x060018F3 RID: 6387 RVA: 0x000B032C File Offset: 0x000AE72C
	public static void RegisterBlocks()
	{
		Block.Register();
		BlockRocket.Register();
		BlockRocketSquare.Register();
		BlockRocketOctagonal.Register();
		BlockMissile.Register();
		BlockAbstractMissileControl.Register();
		BlockJetEngine.Register();
		BlockLegs.Register();
		BlockMotor.Register();
		BlockMotorSlab.Register();
		BlockMotorCube.Register();
		BlockMotorSlab2.Register();
		BlockMotorSpindle.Register();
		BlockPiston.Register();
		BlockMagnet.Register();
		BlockTorsionSpring.Register();
		BlockTorsionSpringCube.Register();
		BlockTorsionSpringSlab.Register();
		BlockBulkyWheel.Register();
		BlockSpokedWheel.Register();
		BlockWheelBling.Register();
		BlockTankTreadsWheel.Register();
		BlockRaycastWheel.Register();
		BlockAbstractWheel.Register();
		BlockLaser.Register();
		BlockLaserCannon.Register();
		BlockOctagonalLaser.Register();
		BlockLaserBlaster.Register();
		BlockJazzGun.Register();
		BlockOptimusGun.Register();
		BlockBumblebeeGun.Register();
		BlockMegatronGun.Register();
		BlockSoundwaveGun.Register();
		BlockStarscreamGun.Register();
		BlockLaserMiniGun.Register();
		BlockLaserRifle.Register();
		BlockHandCannon.Register();
		BlockLaserPistol.Register();
		BlockLaserPistol2.Register();
		BlockMiniGun.Register();
		BlockEmitter.Register();
		BlockStabilizer.Register();
		BlockSquareStabilizer.Register();
		BlockWater.Register();
		BlockGravityGun.Register();
		BlockAntiGravity.Register();
		BlockAntiGravityColumn.Register();
		BlockFlightYoke.Register();
		BlockBatWing.Register();
		BlockCape.Register();
		BlockJetpack.Register();
		BlockAbstractAntiGravity.Register();
		BlockBatWingBackpack.Register();
		BlockWiserWing.Register();
		BlockBirdWing.Register();
		BlockFairyWings.Register();
		BlockMLPWings.Register();
		BlockMovingPlatformForce.Register();
		BlockMovingPlatform.Register();
		BlockRotatingPlatform.Register();
		BlockRotatingPlatformForce.Register();
		BlockHover.Register();
		BlockVolume.Register();
		BlockVolumeBlock.Register();
		BlockVolumeForce.Register();
		BlockTeleportVolumeBlock.Register();
		BlockCharacter.Register();
		BlockQuadped.Register();
		BlockMLPLegs.Register();
		BlockAnimatedCharacter.Register();
		BlockSky.Register();
		BlockTimerUI.Register();
		BlockCounterUI.Register();
		BlockRadarUI.Register();
		BlockGaugeUI.Register();
		BlockObjectCounterUI.Register();
		BlockMaster.Register();
		BlockWorldJumper.Register();
		BlockHighscoreList.Register();
		BlockDriveAssist.Register();
		BlockJukebox.Register();
		BlockWaterCube.Register();
		BlockCloud.Register();
		BlockVolcano.Register();
		BlockSphere.Register();
		BlockSteeringWheel.Register();
		BlockEmitterWater.Register();
		BlockEmitterFire.Register();
		BlockEmitterGas.Register();
		BlockEmitterCampfire.Register();
		BlockBow.Register();
		BlockCrossbow.Register();
		BlockPIRPistol.Register();
	}

	// Token: 0x060018F4 RID: 6388 RVA: 0x000B050A File Offset: 0x000AE90A
	public static bool IsStarted()
	{
		return Blocksworld.started;
	}

	// Token: 0x060018F5 RID: 6389 RVA: 0x000B0511 File Offset: 0x000AE911
	public static void SetupClipboard()
	{
		if (Blocksworld.clipboard == null)
		{
			Blocksworld.clipboard = new Clipboard();
			Blocksworld.clipboard.Load();
		}
		else
		{
			Blocksworld.UI.QuickSelect.UpdateIcons();
		}
	}

	// Token: 0x060018F6 RID: 6390 RVA: 0x000B0545 File Offset: 0x000AE945
	public void InsertTile(Block block, int x, int y, Tile tile)
	{
		block.tiles[y].Insert(x, tile);
		if (block == Blocksworld.selectedBlock)
		{
			Blocksworld.scriptPanel.UpdateGestureRecognizer(Blocksworld.recognizer);
		}
	}

	// Token: 0x060018F7 RID: 6391 RVA: 0x000B0578 File Offset: 0x000AE978
	public static Block GetSelectedScriptBlock()
	{
		if (Blocksworld.selectedBlock != null)
		{
			BlockGrouped blockGrouped = Blocksworld.selectedBlock as BlockGrouped;
			if (blockGrouped == null || blockGrouped.GroupHasIndividualSripting())
			{
				return Blocksworld.selectedBlock;
			}
			return blockGrouped.GetMainBlockInGroup();
		}
		else
		{
			if (Blocksworld.selectedBunch != null && Blocksworld.SelectedBunchIsGroup())
			{
				return (Blocksworld.selectedBunch.blocks[0] as BlockGrouped).GetMainBlockInGroup();
			}
			return null;
		}
	}

	// Token: 0x060018F8 RID: 6392 RVA: 0x000B05E8 File Offset: 0x000AE9E8
	public static bool IsUnlocked(Tile tile)
	{
		return true;
	}

	// Token: 0x060018F9 RID: 6393 RVA: 0x000B05EC File Offset: 0x000AE9EC
	public static void UpdateCenterOfMasses()
	{
		foreach (Chunk chunk in Blocksworld.chunks)
		{
			chunk.UpdateCenterOfMass(true);
		}
		foreach (Chunk chunk2 in Blocksworld.chunks)
		{
			foreach (Block block in chunk2.blocks)
			{
				BlockAbstractMotor blockAbstractMotor = block as BlockAbstractMotor;
				if (blockAbstractMotor != null)
				{
					blockAbstractMotor.CalculateMassDistributions();
				}
			}
		}
	}

	// Token: 0x060018FA RID: 6394 RVA: 0x000B06E8 File Offset: 0x000AEAE8
	private static void UnloadIfNotNull(GameObject g)
	{
		if (g != null)
		{
			Resources.UnloadAsset(g);
		}
	}

	// Token: 0x060018FB RID: 6395 RVA: 0x000B06FC File Offset: 0x000AEAFC
	public static void UnloadBlock(string resource, HashSet<string> usedMeshes)
	{
		if (Blocksworld.meshes.ContainsKey(resource) && Blocksworld.meshes[resource] != null)
		{
			if (!usedMeshes.Contains(resource))
			{
				Mesh obj = Blocksworld.meshes[resource];
				UnityEngine.Object.Destroy(obj);
			}
			Blocksworld.meshes.Remove(resource);
			IEnumerator enumerator = Blocksworld.goPrefabs[resource].transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj2 = enumerator.Current;
					Transform transform = (Transform)obj2;
					string name = transform.gameObject.name;
					Blocksworld.meshes.Remove(name);
					if (!usedMeshes.Contains(name) && Blocksworld.meshes.ContainsKey(name))
					{
						Mesh obj3 = Blocksworld.meshes[name];
						UnityEngine.Object.Destroy(obj3);
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
		}
		Blocksworld.colliders.Remove(resource);
		if (Blocksworld.goPrefabs.ContainsKey(resource))
		{
			if (Blocksworld.glues.ContainsKey(resource))
			{
				CollisionVolumes.Remove(Blocksworld.glues[resource]);
			}
			if (Blocksworld.shapes.ContainsKey(resource))
			{
				CollisionVolumes.Remove(Blocksworld.shapes[resource]);
			}
			if (Blocksworld.joints.ContainsKey(resource))
			{
				CollisionVolumes.Remove(Blocksworld.joints[resource]);
			}
			if (Blocksworld.compoundColliders.ContainsKey(resource))
			{
				CollisionVolumes.Remove(Blocksworld.compoundColliders[resource]);
			}
			UnityEngine.Object.Destroy(Blocksworld.goPrefabs[resource]);
			Blocksworld.goPrefabs.Remove(resource);
		}
	}

	// Token: 0x060018FC RID: 6396 RVA: 0x000B08B8 File Offset: 0x000AECB8
	public static GameObject InstantiateBlockGo(string blockType)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Blocksworld.prefabs[blockType]);
		gameObject.SetActive(true);
		foreach (MeshRenderer meshRenderer in gameObject.GetComponentsInChildren<MeshRenderer>())
		{
			if (Blocksworld.renderingShadows && !blockType.Contains("Volume Block"))
			{
				meshRenderer.receiveShadows = true;
				meshRenderer.shadowCastingMode = ShadowCastingMode.On;
			}
			else
			{
				meshRenderer.receiveShadows = false;
				meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
			}
		}
		return gameObject;
	}

	// Token: 0x060018FD RID: 6397 RVA: 0x000B0934 File Offset: 0x000AED34
	public static void LoadBlockFromPrefab(string resource)
	{
		Blocksworld.goPrefabs[resource] = UnityEngine.Object.Instantiate<GameObject>(Blocksworld.prefabs[resource]);
		Blocksworld.goPrefabs[resource].SetActive(false);
		Blocksworld.colliders[resource] = Blocksworld.goPrefabs[resource].GetComponent<Collider>();
		bool flag = false;
		MeshFilter component = Blocksworld.goPrefabs[resource].GetComponent<MeshFilter>();
		if (component != null)
		{
			Blocksworld.meshes[resource] = component.mesh;
			flag = true;
		}
		IEnumerator enumerator = Blocksworld.goPrefabs[resource].transform.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Transform transform = (Transform)obj;
				component = transform.GetComponent<MeshFilter>();
				if (component != null)
				{
					Blocksworld.meshes[transform.gameObject.name] = component.mesh;
					flag = true;
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
		if (!flag)
		{
			BWLog.Error("No mesh found for: " + resource);
		}
	}

	// Token: 0x060018FE RID: 6398 RVA: 0x000B0A60 File Offset: 0x000AEE60
	public static void LoadBlock(string resource)
	{
        string model = resource;
        BlockEntry modBlock = null;
        foreach (string key in BlockItemsRegistry.GetBlockEntries().Keys)
        {
            if (key == resource)
            {
                modBlock = BlockItemsRegistry.GetBlockEntries()[key];
                model = modBlock.modelName;
            }
        }
        Blocksworld.prefabs[resource] = (Resources.Load("Blocks/Prefab " + model) as GameObject);
		Blocksworld.shapes[resource] = (Resources.Load("Blocks/Shape " + model) as GameObject);
		Blocksworld.glues[resource] = (Resources.Load("Blocks/Glue " + model) as GameObject);
		Blocksworld.joints[resource] = (Resources.Load("Blocks/Joint " + model) as GameObject);
		GameObject gameObject = Resources.Load("Blocks/Collider " + model) as GameObject;
		if (gameObject != null)
		{
			Blocksworld.compoundColliders[resource] = gameObject;
		}
        if (modBlock != null)
        {
            if (modBlock.metaData != null)
            {
                BlockMetaData org = Blocksworld.prefabs[resource].GetComponent<BlockMetaData>();
                BlockMetaData mod = modBlock.metaData;
                if (mod.meshDatas != null) org.meshDatas = mod.meshDatas;
				//org.massK = mod.massK;
				//org.massM = mod.massM;
				//org.hideInFirstPersonCamera = mod.hideInFirstPersonCamera;
				//org.gearType = mod.gearType;
				//org.blockSize = mod.blockSize;
				//org.canScale = mod.canScale;
				//org.scaleConstraints = mod.scaleConstraints;
				//org.disableBuildModeMove = mod.disableBuildModeMove;
				//org.disableBuildModeScale = mod.disableBuildModeScale;
				//org.isBlocksterMassless = mod.isBlocksterMassless;
				org.scaleType = mod.scaleType;
                // TODO: add others
            }
        }
    }

	// Token: 0x060018FF RID: 6399 RVA: 0x000B0B1B File Offset: 0x000AEF1B
	[Conditional("DEBUG")]
	public static void Display(string s)
	{
		Blocksworld.displayString = Blocksworld.displayString + s + "\n";
	}

	// Token: 0x06001900 RID: 6400 RVA: 0x000B0B32 File Offset: 0x000AEF32
	[Conditional("DEBUG")]
	private void DisplayOnGUI()
	{
		GUI.Label(new Rect(10f * NormalizedScreen.scale, 10f * NormalizedScreen.scale, (float)Screen.width * NormalizedScreen.scale, (float)Screen.height * NormalizedScreen.scale), Blocksworld.displayString);
	}

	// Token: 0x06001901 RID: 6401 RVA: 0x000B0B74 File Offset: 0x000AEF74
	private void OnHudMesh()
	{
		this.ExecuteOnHudMesh(Blocksworld.fixedUpdateCommands);
		if (Blocksworld.CurrentState == State.Play)
		{
			BWSceneManager.OnHudMesh();
		}
		Scarcity.PaintScarcityBadges();
		if (Blocksworld.timerStart >= 0f && Blocksworld.CurrentState != State.FrameCapture)
		{
			HudMeshOnGUI.Label(ref this.labelTimer, new Rect(350f * NormalizedScreen.scale, (float)Blocksworld.defaultPanelPadding * NormalizedScreen.scale, (float)(NormalizedScreen.width - 600) * NormalizedScreen.scale, 120f * NormalizedScreen.scale), (((Blocksworld.timerStop <= 0f) ? Time.time : Blocksworld.timerStop) - Blocksworld.timerStart).ToString("0.0"), HudMeshOnGUI.dataSource.GetStyle("label"), 0f);
		}
		HudMeshStyle defaultStyle = HudMeshOnGUI.dataSource.defaultStyle;
		int num = 0;
		for (int i = 1; i <= 2; i++)
		{
			string key = Blocksworld.counterNames[i];
			if (Blocksworld.countersActivated[key] && Blocksworld.CurrentState == State.Play)
			{
				bool flag = Blocksworld.counterTargetsActivated[key];
				int num2 = Blocksworld.counterTargets[key];
				int num3 = Blocksworld.counters[key];
				if (flag)
				{
					float num4 = NormalizedScreen.scale * 70f;
					float height = NormalizedScreen.scale * 70f;
					float num5 = (float)num2 * num4;
					for (int j = 0; j < num2; j++)
					{
						Rect rect = new Rect((float)(Screen.width / 2) - num5 / 2f + num4 * (float)j, NormalizedScreen.scale * 20f, num4, height);
						if (j < num3)
						{
							HudMeshOnGUI.Label(this.labelCounters, num++, rect, Blocksworld.starTexture, null);
						}
						else
						{
							HudMeshOnGUI.Label(this.labelCounters, num++, rect, Blocksworld.starOutlineTexture, null);
						}
					}
				}
				else
				{
					string text = (!flag) ? (string.Empty + num3) : (num3 + "/" + num2);
					float num6 = NormalizedScreen.scale * 100f;
					float height2 = NormalizedScreen.scale * 100f;
					Rect rect2 = new Rect((float)(Screen.width / 2) - num6 / 2f + (float)(i - 1) * num6, NormalizedScreen.scale * 40f, num6, height2);
					HudMeshOnGUI.Label(this.labelCounters, num++, rect2, text, defaultStyle);
				}
			}
		}
		Tutorial.OnHudMesh();
    }

	// Token: 0x06001902 RID: 6402 RVA: 0x000B0E00 File Offset: 0x000AF200
	private void ExecuteOnHudMesh(List<Command> commands)
	{
		for (int i = 0; i < commands.Count; i++)
		{
			commands[i].OnHudMesh();
		}
	}

	// Token: 0x06001903 RID: 6403 RVA: 0x000B0E30 File Offset: 0x000AF230
	private void ShowPlayerProfilePictureHint()
	{
		GUI.Label(new Rect(0f, (float)(Screen.height - 100), (float)Screen.width, 100f), "Tap the camera button to take a profile picture of your Blockster");
	}

	// Token: 0x06001904 RID: 6404 RVA: 0x000B0E5A File Offset: 0x000AF25A
	private void OnApplicationQuit()
	{
		if (BW.Options.saveOnApplicationQuit())
		{
			this.FastSave();
		}
	}

	// Token: 0x06001905 RID: 6405 RVA: 0x000B0E74 File Offset: 0x000AF274
	public void OnApplicationPause(bool pauseStatus)
	{
		if (!Blocksworld.IsStarted())
		{
			return;
		}
		if (pauseStatus && Blocksworld.CurrentState == State.Paused)
		{
			return;
		}
		if (pauseStatus && WorldSession.current != null && (Blocksworld.CurrentState == State.Play || Blocksworld.CurrentState == State.Build || Blocksworld.CurrentState == State.Paused) && !WinLoseManager.winning && !WinLoseManager.ending && !Blocksworld.UI.Tapedeck.RecordButtonActive())
		{
			this.ShowOptionsScreen();
		}
		if (pauseStatus)
		{
			Blocksworld.SetVRMode(false);
		}
		else if (Blocksworld.bw != null && !Blocksworld.UI.IsOptionsScreenVisible())
		{
			WorldSession.UnpauseCurrentSession();
		}
		if (Blocksworld.musicPlayer != null)
		{
			Blocksworld.musicPlayer.BWApplicationPause(pauseStatus);
		}
		Blocksworld.UI.Tapedeck.BWApplicationPause(pauseStatus);
	}

	// Token: 0x06001906 RID: 6406 RVA: 0x000B0F5E File Offset: 0x000AF35E
	public static bool GUIButton(Rect rect, string str, bool enabled = true)
	{
		GUI.enabled = enabled;
		return GUI.RepeatButton(rect, str) && Input.GetMouseButtonUp(0) && (Blocksworld.consumeEvent = true);
	}

	// Token: 0x06001907 RID: 6407 RVA: 0x000B0F87 File Offset: 0x000AF387
	public static bool GUIButton(Rect rect, string str, GUIStyle style, bool enabled = true)
	{
		return GUI.RepeatButton(rect, str, style) && Input.GetMouseButtonUp(0) && (Blocksworld.consumeEvent = true);
	}

	// Token: 0x06001908 RID: 6408 RVA: 0x000B0FAB File Offset: 0x000AF3AB
	public static bool GUIButton(Rect rect, Texture tex)
	{
		return GUI.RepeatButton(rect, tex) && Input.GetMouseButtonUp(0) && (Blocksworld.consumeEvent = true);
	}

	// Token: 0x06001909 RID: 6409 RVA: 0x000B0FCE File Offset: 0x000AF3CE
	public static void MainMenuButtonsDidShow()
	{
		BWLog.Info("Sending custom 'MainMenu' signal to main menu world.");
		Blocksworld.sendingCustom["MainMenu"] = 1f;
	}

	// Token: 0x0600190A RID: 6410 RVA: 0x000B0FEE File Offset: 0x000AF3EE
	private static void SetupVRCamera()
	{
		if (Blocksworld.vrEnabled)
		{
		}
	}

	// Token: 0x0600190B RID: 6411 RVA: 0x000B0FFA File Offset: 0x000AF3FA
	private static void TakedownVRCamera()
	{
		if (Blocksworld.vrEnabled)
		{
		}
	}

	// Token: 0x0600190C RID: 6412 RVA: 0x000B1006 File Offset: 0x000AF406
	public static Ray CameraScreenPointToRay(Vector3 screenPos)
	{
		if (Blocksworld.vrEnabled)
		{
			return Blocksworld._mainCamera.ScreenPointToRay(screenPos);
		}
		return Blocksworld._mainCamera.ScreenPointToRay(screenPos);
	}

	// Token: 0x0600190D RID: 6413 RVA: 0x000B102C File Offset: 0x000AF42C
	public static void SetupCameraHierarchy()
	{
		GameObject gameObject = new GameObject("Camera Tilt Transform");
		Blocksworld.bw.cameraTiltTransform = gameObject.transform;
		Blocksworld.bw.cameraTiltTransform.parent = Blocksworld.cameraTransform;
		Blocksworld.bw.cameraTiltTransform.localPosition = Vector3.zero;
		Blocksworld.bw.cameraTiltTransform.localRotation = Quaternion.identity;
		Blocksworld.vrCameraAdjust = new GameObject("VR Camera Straightener");
		Blocksworld.vrCameraAdjust.transform.parent = Blocksworld.bw.cameraTiltTransform;
		Blocksworld.vrCameraAdjust.transform.localPosition = Vector3.zero;
		Blocksworld.vrCameraAdjust.transform.localRotation = Quaternion.identity;
		Blocksworld.bw.cameraParentTransform = Blocksworld.vrCameraAdjust.transform;
		Blocksworld.mainCamera.transform.parent = Blocksworld.bw.cameraParentTransform;
		Blocksworld.mainCamera.gameObject.SetActive(false);
	}

	// Token: 0x0600190E RID: 6414 RVA: 0x000B111D File Offset: 0x000AF51D
	public static void ResetVRSensor()
	{
		Blocksworld.vrCameraAdjust.transform.localRotation = Quaternion.identity;
		if (!Blocksworld.vrEnabled)
		{
			return;
		}
	}

	// Token: 0x0600190F RID: 6415 RVA: 0x000B1140 File Offset: 0x000AF540
	public void SetVRModeFromJS(int trueFalse)
	{
		BWLog.Info("SetVRModeFromJS(" + trueFalse + ")");
		bool vrmode = 1 == trueFalse;
		Blocksworld.SetVRMode(vrmode);
	}

	// Token: 0x06001910 RID: 6416 RVA: 0x000B1174 File Offset: 0x000AF574
	public static void SetVRMode(bool enabled)
	{
		if (enabled == Blocksworld.vrEnabled)
		{
			return;
		}
		Blocksworld.vrEnabled = enabled;
		Blocksworld.worldSessionHadVR |= Blocksworld.vrEnabled;
		BWLog.Info(((!Blocksworld.vrEnabled) ? "Disabling" : "Enabling") + " VR Mode " + Blocksworld.currentVRType);
		if (Blocksworld.vrEnabled)
		{
			BWLog.Error("VR type " + Blocksworld.currentVRType + " is not enabled in this build.");
		}
		Blocksworld.UI.SpeechBubble.SetWorldSpaceMode(Blocksworld.vrEnabled);
		if (Blocksworld.vrEnabled)
		{
			Vector3 vector = new Vector3(Blocksworld.cameraTransform.forward.x, 0f, Blocksworld.cameraTransform.forward.z);
			if (vector.sqrMagnitude < 0.1f)
			{
				vector = new Vector3(Blocksworld.cameraTransform.up.x, 0f, Blocksworld.cameraTransform.up.z);
			}
			Blocksworld.vrCameraAdjust.transform.rotation = Quaternion.LookRotation(vector.normalized, Vector3.up);
		}
		else
		{
			Blocksworld.vrCameraAdjust.transform.localRotation = Quaternion.identity;
		}
		Blocksworld.blocksworldCamera.SetReticleParent((!Blocksworld.vrEnabled) ? null : Blocksworld.mainCamera.transform);
	}

	// Token: 0x06001911 RID: 6417 RVA: 0x000B12EF File Offset: 0x000AF6EF
	public static bool IsVRCameraMode()
	{
		return Blocksworld.vrEnabled;
	}

	// Token: 0x06001912 RID: 6418 RVA: 0x000B12F6 File Offset: 0x000AF6F6
	public static bool IsBlockVRCameraFocus(Block b)
	{
		return false;
	}

	// Token: 0x06001913 RID: 6419 RVA: 0x000B12F9 File Offset: 0x000AF6F9
	public static bool IsBlockVRCameraLookAt(Block b)
	{
		return false;
	}

	// Token: 0x06001914 RID: 6420 RVA: 0x000B12FC File Offset: 0x000AF6FC
	public void CancelTileDragGestures()
	{
		if (Blocksworld.tileDragGesture.IsActive)
		{
			Blocksworld.tileDragGesture.Cancel();
		}
		if (Blocksworld.createTileDragGesture.IsActive)
		{
			Blocksworld.createTileDragGesture.Cancel();
		}
		if (Blocksworld.replaceBodyPartGesture.IsActive)
		{
			Blocksworld.replaceBodyPartGesture.Cancel();
		}
		if (Blocksworld.characterEditGearGesture.IsActive)
		{
			Blocksworld.characterEditGearGesture.Cancel();
		}
	}

	// Token: 0x06001915 RID: 6421 RVA: 0x000B1370 File Offset: 0x000AF770
	public void Play()
	{
		if (CharacterEditor.Instance.InEditMode())
		{
			CharacterEditor.Instance.Exit();
		}
		if (!Blocksworld.resettingPlay)
		{
			this.lastBuildModeSelectedBlock = Blocksworld.selectedBlock;
			this.lastBuildModeSelectedBunch = Blocksworld.selectedBunch;
			Blocksworld._buildModeFogColor = RenderSettings.fogColor;
			Blocksworld._buildModeFogStart = RenderSettings.fogStartDistance;
			Blocksworld._buildModeFogEnd = RenderSettings.fogEndDistance;
		}
		EventSystem.current.SetSelectedGameObject(null);
		Blocksworld.Select(null, false, true);
		Blocksworld.scriptPanel.Show(false);
		this.CancelTileDragGestures();
		WorldSession.current.OnPlay();
		MappedInput.SetMode(MappableInputMode.Play);
		Block.ClearConnectedCache();
		Block.ResetTiltOrientation();
		Blocksworld.playFixedUpdateCounter = 0;
		BWSceneManager.ResetPlayBlocksAndPredicates();
		Block.vanishingOrAppearingBlocks.Clear();
		TextureAndPaintBlockRegistry.Clear();
		BlockAccelerations.Play();
		CheckpointSystem.Clear();
		VariableManager.Clear();
		WinLoseManager.Reset();
		Invincibility.Clear();
		Blocksworld.UpdateEditorMusicPlayerEnabled();
		ModelSignals.Clear();
		TiltManager.Instance.StartMonitoring();
		Physics.gravity = -Blocksworld.defaultGravityStrength * Vector3.up;
		Blocksworld.angularDragMultiplier = 1f;
		Blocksworld.dragMultiplier = 1f;
		Blocksworld.hasWon = false;
		Blocksworld.winIsWaiting = false;
		Blocksworld.waitForSetPurchase = false;
		Blocksworld.worldSessionHadBlockTap = false;
		Blocksworld.isFirstFrame = true;
		Blocksworld.dynamicLockPull = false;
		Blocksworld.interpolateRigidBodies = BW.Options.interpolateRigidbodies();
		if (Tutorial.state != TutorialState.None)
		{
		}
		Blocksworld.resettingPlay = false;
		Blocksworld.UpdateCameraPosesMap();
		Blocksworld.currentBackgroundMusic = string.Empty;
		Tutorial.StartPlay();
		BWSceneManager.InitPlay();
		List<List<Block>> list = ConnectednessGraph.FindChunkSubgraphs();
		foreach (List<Block> blocks in list)
		{
			Blocksworld.chunks.Add(new Chunk(blocks, false));
		}
		for (int i = 0; i < Blocksworld.chunks.Count; i++)
		{
			Blocksworld.chunks[i].blocks[0].UpdateConnectedCache();
		}
		BWSceneManager.BeforePlay();
		BlockAbstractLegs.InitPlay();
		BlockWalkable.InitPlay();
		BlockAbstractLegs.InitPlay();
		TreasureHandler.Clear();
		BWSceneManager.ClearScriptBlocks();
		OnScreenLog.Clear();
		BWSceneManager.ExecutePlay();
		BWSceneManager.MakeFixedAndSpawnpointBeforeFirstFrame();
		BWSceneManager.ExecutePlay2();
		this.ResetState();
		TagManager.ClearRegisteredBlocks(true);
		BWSceneManager.ExecutePlayLegs();
		Blocksworld.blocksworldCamera.Store();
		Blocksworld.blocksworldCamera.Play();
		Blocksworld.SetBlocksworldState(State.Play);
		TBox.Show(false);
		Blocksworld.gameStart = true;
		Blocksworld.gameTime = 0f;
		Blocksworld.UI.SidePanel.Hide();
		Blocksworld.UI.Controls.OnPlay();
		Blocksworld.orbitDuringControlGesture.IsEnabled = true;
		if (this.pullObject != null)
		{
			this.pullObject.HideLine();
		}
		Sound.Play();
		CollisionManager.Play();
		Blocksworld.UpdateCenterOfMasses();
		this.RemoveAllCommands();
		this.storeFogDistances = this.GetDefaultFog();
		Blocksworld.leaderboardData.SetupLeaderboard();
		Blocksworld.AddFixedUpdateCommand(new DelegateCommand(delegate(DelegateCommand a)
		{
			for (int j = 0; j < Blocksworld.chunks.Count; j++)
			{
				Rigidbody rb = Blocksworld.chunks[j].rb;
				if (rb != null && rb.IsSleeping() && rb.isKinematic)
				{
					rb.isKinematic = false;
					rb.isKinematic = true;
				}
			}
		}));
	}

	// Token: 0x06001916 RID: 6422 RVA: 0x000B1674 File Offset: 0x000AFA74
	public static void UpdateCameraPosesMap()
	{
		Blocksworld.cameraPosesMap.Clear();
		for (int i = 0; i < Blocksworld.cameraPoses.Count; i++)
		{
			NamedPose namedPose = Blocksworld.cameraPoses[i];
			if (!string.IsNullOrEmpty(namedPose.name))
			{
				Blocksworld.cameraPosesMap[namedPose.name] = namedPose;
			}
		}
	}

	// Token: 0x06001917 RID: 6423 RVA: 0x000B16D4 File Offset: 0x000AFAD4
	public static void GoToCameraFrameFor(Vector3 pos, Vector3 scale)
	{
		float d = 7f + 1.3f * Util.MaxComponent(scale);
		Vector3 position = Blocksworld.cameraTransform.position;
		Vector3 normalized = (pos - position).normalized;
		Vector3 vector = pos - normalized * d;
		Blocksworld.blocksworldCamera.SetCameraPosition(vector);
		Blocksworld.blocksworldCamera.SetTargetPosition(pos);
		Blocksworld.cameraTransform.LookAt(pos);
	}

	// Token: 0x06001918 RID: 6424 RVA: 0x000B1744 File Offset: 0x000AFB44
	public static HashSet<Predicate> GetAnalogStickPredicates()
	{
		if (Blocksworld.analogStickPredicates == null)
		{
			Blocksworld.analogStickPredicates = new HashSet<Predicate>
			{
				BlockQuadped.predicateQuadpedMover,
				BlockSphere.predicateSphereMover,
				BlockMLPLegs.predicateMLPLegsMover,
				BlockLegs.predicateLegsMover,
				BlockCharacter.predicateCharacterMover,
				BlockAnimatedCharacter.predicateCharacterMover,
				BlockAntiGravity.predicateAntigravityAlignAlongMover,
				BlockFlightYoke.predicateAlignAlongDPad,
				BlockFlightYoke.predicateBankTurn,
				BlockFlightYoke.predicateFlightSim,
				BlockTankTreadsWheel.predicateTankTreadsAnalogStickControl,
				Block.predicateDPadMoved,
				Block.predicateDPadHorizontal,
				Block.predicateDPadVertical,
				BlockSteeringWheel.predicateSteeringWheelMoveAlongMover,
				BlockSteeringWheel.predicateSteeringWheelMoveLocalAlongMover,
				BlockSteeringWheel.predicateSteeringWheelMoverSteer
			};
		}
		return Blocksworld.analogStickPredicates;
	}

	// Token: 0x06001919 RID: 6425 RVA: 0x000B1838 File Offset: 0x000AFC38
	public static HashSet<Predicate> GetTiltMoverPredicates()
	{
		if (Blocksworld.tiltMoverPredicates == null)
		{
			Blocksworld.tiltMoverPredicates = new HashSet<Predicate>
			{
				BlockCharacter.predicateCharacterTiltMover,
				BlockAnimatedCharacter.predicateChracterTiltMover,
				BlockAbstractAntiGravity.predicateTiltMover,
				BlockSphere.predicateSphereTiltMover,
				BlockFlightYoke.predicateTiltFlightSim
			};
		}
		return Blocksworld.tiltMoverPredicates;
	}

	// Token: 0x0600191A RID: 6426 RVA: 0x000B189C File Offset: 0x000AFC9C
	public static bool DPadTilesInWorld(string key)
	{
		HashSet<Predicate> hashSet = Blocksworld.GetAnalogStickPredicates();
		List<Block> list = BWSceneManager.AllBlocks();
		for (int i = 0; i < list.Count; i++)
		{
			Block block = list[i];
			for (int j = 1; j < block.tiles.Count; j++)
			{
				List<Tile> list2 = block.tiles[j];
				for (int k = 0; k < list2.Count; k++)
				{
					Tile tile = list2[k];
					GAF gaf = tile.gaf;
					Predicate predicate = gaf.Predicate;
					if (hashSet.Contains(predicate))
					{
						object[] args = gaf.Args;
						int num = 0;
						if (predicate == Block.predicateDPadMoved || predicate == Block.predicateDPadVertical || predicate == Block.predicateDPadHorizontal)
						{
							num = 1;
						}
						string a = (args.Length <= num) ? "L" : ((string)args[num]);
						return a == key;
					}
				}
			}
		}
		return false;
	}

	// Token: 0x0600191B RID: 6427 RVA: 0x000B19A8 File Offset: 0x000AFDA8
	public void Stop(bool force = false, bool resetBlocks = true)
	{
		if (WorldSession.current != null)
		{
			WorldSession.current.OnStop();
		}
		if (EventSystem.current != null)
		{
			EventSystem.current.SetSelectedGameObject(null);
		}
		if (force || Blocksworld.CurrentState == State.Play || (Blocksworld.LastState == State.Play && Blocksworld.CurrentState == State.Menu))
		{
			BWSceneManager.StopPlay();
			TiltManager.Instance.StopMonitoring();
			MappedInput.SetMode(MappableInputMode.Build);
			Blocksworld.customIntVariables.Clear();
			Blocksworld.blockIntVariables.Clear();
			BlockData.Clear();
			ScriptRowData.Clear();
			ModelSignals.Clear();
			TBox.ResetTargetSnapping();
			TextureAndPaintBlockRegistry.Clear();
			Blocksworld.UpdateEditorMusicPlayerEnabled();
			Blocksworld.UpdateTiles();
			Blocksworld.currentBackgroundMusic = string.Empty;
			Blocksworld.lockInput = false;
			Blocksworld.SetBlocksworldState(State.Build);
			BlockAccelerations.Stop();
			BWSceneManager.playBlocksRemoved.Clear();
			Blocksworld.orbitDuringControlGesture.IsEnabled = false;
			for (int i = 1; i <= Blocksworld.numCounters; i++)
			{
				string key = i + string.Empty;
				Blocksworld.countersActivated[key] = false;
				Blocksworld.counters[key] = 0;
			}
			Blocksworld.resetting = true;
			BWSceneManager.ExecuteStop(resetBlocks);
			Blocksworld.resetting = false;
			Blocksworld.dynamicLockPull = false;
			this.ClearChunks();
			Blocksworld.blocksworldCamera.Restore();
			Blocksworld.rewardCamera.enabled = false;
			if (!this.forcePlayMode)
			{
				Blocksworld.UI.SidePanel.Show();
				Blocksworld.buildPanel.Layout();
			}
			Blocksworld.blocksworldCamera.Stop();
			Blocksworld.blocksworldCamera.Follow(Blocksworld.selectedBunch);
			Blocksworld.ResetSkyAndFogSettings();
			CollisionManager.Stop();
			Tutorial.StopPlay();
			if (this.pullObject != null)
			{
				this.pullObject.HideLine();
			}
			Blocksworld.UI.Controls.ResetAllControls();
			Blocksworld.UI.Controls.Hide();
			if (!Blocksworld.resettingPlay)
			{
			}
			TagManager.ClearRegisteredBlocks(true);
			VisualEffect.StopVfxs();
			TreasureHandler.Stop();
			Invincibility.Clear();
			this.RemoveAllCommands();
			Block.ClearConnectedCache();
			Blocksworld.leaderboardData.Reset();
			if (BW.isUnityEditor)
			{
				Tile.UpdateTileParameterSettings();
			}
			Resources.UnloadUnusedAssets();
			GC.Collect();
			if (!Blocksworld.resettingPlay)
			{
				if (this.lastBuildModeSelectedBunch != null)
				{
					Blocksworld.SelectBunch(this.lastBuildModeSelectedBunch.blocks, false, true);
				}
				else if (this.lastBuildModeSelectedBlock != null)
				{
					Blocksworld.Select(this.lastBuildModeSelectedBlock, false, true);
				}
				this.lastBuildModeSelectedBunch = null;
				this.lastBuildModeSelectedBlock = null;
			}
		}
	}

	// Token: 0x0600191C RID: 6428 RVA: 0x000B1C1C File Offset: 0x000B001C
	private void ClearChunks()
	{
		foreach (Chunk chunk in Blocksworld.chunks)
		{
			chunk.Destroy(false);
		}
		Blocksworld.chunks.Clear();
	}

	// Token: 0x0600191D RID: 6429 RVA: 0x000B1C84 File Offset: 0x000B0084
	private void RemoveAllCommands()
	{
		Command.RemoveCommands(Blocksworld.fixedUpdateCommands);
		Command.RemoveCommands(Blocksworld.updateCommands);
		Command.RemoveCommands(Blocksworld.resetStateCommands);
	}

	// Token: 0x0600191E RID: 6430 RVA: 0x000B1CA4 File Offset: 0x000B00A4
	public void Restart()
	{
		if (Blocksworld.resettingPlay)
		{
			return;
		}
		Blocksworld.buildPanel.ignoreShow = true;
		Blocksworld.scriptPanel.ignoreShow = true;
		if (WorldSession.jumpRestoreConfig != null)
		{
			WorldSession.RestoreJumpConfig();
		}
		else
		{
			Blocksworld.resettingPlay = true;
			this.Stop(false, true);
			base.StartCoroutine(this.DoRestart());
		}
	}

	// Token: 0x0600191F RID: 6431 RVA: 0x000B1D04 File Offset: 0x000B0104
	public IEnumerator DoRestart()
	{
		WorldSession.current.OnRestart();
		yield return 2;
		this.Play();
		yield break;
	}

	// Token: 0x06001920 RID: 6432 RVA: 0x000B1D20 File Offset: 0x000B0120
	public void Save()
	{
		if (!Blocksworld.worldSaveEnabled || Blocksworld.f3PressedInCurrentWorld)
		{
			return;
		}
		if (Blocksworld.CurrentState == State.Play || (Blocksworld.CurrentState != State.Build && Blocksworld.LastState == State.Play))
		{
			BWLog.Info("Skipping save since in play mode");
			return;
		}
		WorldSession.Save();
	}

	// Token: 0x06001921 RID: 6433 RVA: 0x000B1D72 File Offset: 0x000B0172
	public void FastSave()
	{
		if (!Blocksworld.worldSaveEnabled || Blocksworld.f3PressedInCurrentWorld)
		{
			return;
		}
		if (Blocksworld.CurrentState == State.Play)
		{
			BWLog.Info("Skipping fast save since in play mode");
			return;
		}
		WorldSession.FastSave();
	}

	// Token: 0x06001922 RID: 6434 RVA: 0x000B1DA4 File Offset: 0x000B01A4
	private void WriteTilesAsJSON(JSONStreamEncoder encoder, List<List<Tile>> tileRows, bool lockBlocks, bool compact)
	{
		encoder.BeginArray();
		bool flag = lockBlocks && (tileRows[1].Count < 2 || !tileRows[1][1].IsLocked());
		bool flag2 = compact && tileRows.Count > 0 && tileRows[0].Count > 0 && tileRows[0][0].gaf.Predicate == Block.predicateStop;
		for (int i = 0; i < tileRows.Count; i++)
		{
			List<Tile> list = tileRows[i];
			if (!flag2 || i != tileRows.Count - 1 || list.Count != 1 || list[0].gaf.Predicate != Block.predicateThen)
			{
				encoder.BeginArray();
				foreach (Tile tile in list)
				{
					if (flag2 && i == 0)
					{
						GAF gaf = tile.gaf;
						if ((gaf.Predicate == Block.predicateTextureTo && (string)gaf.Args[0] == "Plain") || gaf.Predicate == Block.predicateStop)
						{
							continue;
						}
					}
					tile.ToJSON(encoder, compact);
				}
				encoder.EndArray();
			}
			if (flag)
			{
				encoder.BeginArray();
				Tile tile2 = new Tile(new GAF("Meta.Then", new object[0]));
				Tile tile3 = new Tile(new GAF("Block.Locked", new object[0]));
				tile2.ToJSON(encoder, compact);
				tile3.ToJSON(encoder, compact);
				encoder.EndArray();
				flag = false;
			}
		}
		encoder.EndArray();
	}

	// Token: 0x06001923 RID: 6435 RVA: 0x000B1FA4 File Offset: 0x000B03A4
	private void WriteBlockAsJSON(JSONStreamEncoder encoder, List<List<Tile>> blockTiles, bool lockBlocks, bool compact)
	{
		encoder.InsertNewline();
		encoder.BeginObject();
		encoder.WriteKey((!Blocksworld.useCompactGafWriteRenamings) ? "tile-rows" : "r");
		this.WriteTilesAsJSON(encoder, blockTiles, lockBlocks, compact);
		encoder.EndObject();
	}

	// Token: 0x06001924 RID: 6436 RVA: 0x000B1FE4 File Offset: 0x000B03E4
	private void WriteBlocksAsJSON(JSONStreamEncoder encoder, bool compact)
	{
		encoder.BeginArray();
		bool flag = Tutorial.state != TutorialState.None && Tutorial.state != TutorialState.Puzzle;
		if (flag && Tutorial.safeState != null)
		{
			foreach (List<List<Tile>> blockTiles in Tutorial.safeState.placedBlockTiles)
			{
				this.WriteBlockAsJSON(encoder, blockTiles, flag, compact);
			}
			foreach (List<List<Tile>> blockTiles2 in Tutorial.safeState.notPlacedBlockTiles)
			{
				this.WriteBlockAsJSON(encoder, blockTiles2, false, compact);
			}
		}
		else
		{
			List<Block> list = BWSceneManager.AllBlocks();
			Block block = (list.Count <= 0) ? null : list[list.Count - 1];
			for (int i = 0; i < list.Count; i++)
			{
				Block block2 = list[i];
				if (!flag || !Tutorial.IsLastBlockIncomplete() || Tutorial.state == TutorialState.BuildingCompleted || Tutorial.numGivenBlocks == list.Count || block2 != block)
				{
					this.WriteBlockAsJSON(encoder, block2.tiles, flag, compact);
				}
			}
			if (Tutorial.blocks != null && flag && Tutorial.step >= 0)
			{
				for (int j = Tutorial.step; j < Tutorial.blocks.Count; j++)
				{
					this.WriteBlockAsJSON(encoder, Tutorial.blocks[j].tiles, false, compact);
				}
			}
		}
		encoder.InsertNewline();
		encoder.EndArray();
	}

	// Token: 0x06001925 RID: 6437 RVA: 0x000B21D4 File Offset: 0x000B05D4
	private void WriteCameraAsJSON(JSONStreamEncoder encoder)
	{
		encoder.BeginObject();
		encoder.InsertNewline();
		encoder.WriteKey("rotation");
		Blocksworld.cameraTransform.rotation.ToJSON(encoder);
		encoder.InsertNewline();
		encoder.WriteKey("position");
		Blocksworld.cameraTransform.position.ToJSON(encoder, false, false, false);
		encoder.InsertNewline();
		encoder.WriteKey("playDistance");
		encoder.WriteNumber(Blocksworld.blocksworldCamera.manualCameraDistance);
		encoder.InsertNewline();
		encoder.WriteKey("playAngle");
		encoder.WriteNumber(Blocksworld.blocksworldCamera.manualCameraAngle);
		encoder.InsertNewline();
		encoder.EndObject();
	}

	// Token: 0x06001926 RID: 6438 RVA: 0x000B227C File Offset: 0x000B067C
	public string Serialize(bool saveCamera = true)
	{
		StringBuilder stringBuilder = new StringBuilder(32768);
		StringWriter writer = new StringWriter(stringBuilder, CultureInfo.InvariantCulture);
		JSONStreamEncoder jsonstreamEncoder = new JSONStreamEncoder(writer, 20);
		jsonstreamEncoder.BeginObject();
		jsonstreamEncoder.InsertNewline();
		if (Blocksworld.currentWorldId != null)
		{
			jsonstreamEncoder.WriteKey("id");
			jsonstreamEncoder.WriteString(Blocksworld.currentWorldId);
			jsonstreamEncoder.InsertNewline();
		}
		jsonstreamEncoder.WriteKey("blocks");
		this.WriteBlocksAsJSON(jsonstreamEncoder, Blocksworld.useCompactGafWriteRenamings);
		jsonstreamEncoder.InsertNewline();
		jsonstreamEncoder.WriteKey("connections");
		jsonstreamEncoder.BeginArray();
		List<Block> list = BWSceneManager.AllBlocks();
		for (int i = 0; i < list.Count; i++)
		{
			Block block = list[i];
			jsonstreamEncoder.InsertNewline();
			jsonstreamEncoder.BeginArray();
			foreach (Block item in block.connections)
			{
				jsonstreamEncoder.WriteNumber((long)list.IndexOf(item));
			}
			jsonstreamEncoder.EndArray();
		}
		jsonstreamEncoder.InsertNewline();
		jsonstreamEncoder.EndArray();
		jsonstreamEncoder.InsertNewline();
		jsonstreamEncoder.WriteKey("connectionTypes");
		jsonstreamEncoder.BeginArray();
		for (int j = 0; j < list.Count; j++)
		{
			Block block2 = list[j];
			jsonstreamEncoder.InsertNewline();
			jsonstreamEncoder.BeginArray();
			foreach (int num in block2.connectionTypes)
			{
				jsonstreamEncoder.WriteNumber((long)num);
			}
			jsonstreamEncoder.EndArray();
		}
		jsonstreamEncoder.InsertNewline();
		jsonstreamEncoder.EndArray();
		jsonstreamEncoder.InsertNewline();
		jsonstreamEncoder.WriteKey("frozen-in-terrain");
		jsonstreamEncoder.BeginArray();
		for (int k = 0; k < list.Count; k++)
		{
			Block block3 = list[k];
			if (block3.frozenInTerrainStatus == -1)
			{
				block3.UpdateFrozenInTerrainStatus();
			}
			jsonstreamEncoder.WriteNumber((long)block3.frozenInTerrainStatus);
		}
		jsonstreamEncoder.EndArray();
		jsonstreamEncoder.InsertNewline();
		if (saveCamera)
		{
			jsonstreamEncoder.WriteKey("camera");
			this.WriteCameraAsJSON(jsonstreamEncoder);
			jsonstreamEncoder.InsertNewline();
		}
		else if (this.lastLoadedCameraObj != null)
		{
			jsonstreamEncoder.WriteKey("camera");
			jsonstreamEncoder.WriteJObject(this.lastLoadedCameraObj);
			jsonstreamEncoder.InsertNewline();
		}
		this.WritePuzzleInventoryAndUsage(jsonstreamEncoder, Scarcity.puzzleInventory, "puzzle-gafs", "puzzle-gaf-usage");
		this.WritePuzzleInventoryAndUsage(jsonstreamEncoder, Scarcity.puzzleInventoryAfterStepByStep, "puzzle-gafs-after-step-by-step", "puzzle-gaf-usage-after-step-by-step");
		if (Blocksworld.editorSelectionLocked.Count > 0)
		{
			jsonstreamEncoder.WriteKey("selection-locked");
			jsonstreamEncoder.BeginArray();
			for (int l = 0; l < list.Count; l++)
			{
				Block item2 = list[l];
				if (Blocksworld.editorSelectionLocked.Contains(item2))
				{
					jsonstreamEncoder.WriteNumber((long)l);
				}
			}
			jsonstreamEncoder.EndArray();
			jsonstreamEncoder.InsertNewline();
		}
		if (Blocksworld.cameraPoses.Count > 0)
		{
			jsonstreamEncoder.WriteKey("camera-poses");
			jsonstreamEncoder.BeginArray();
			jsonstreamEncoder.InsertNewline();
			for (int m = 0; m < Blocksworld.cameraPoses.Count; m++)
			{
				NamedPose namedPose = Blocksworld.cameraPoses[m];
				jsonstreamEncoder.BeginObject();
				jsonstreamEncoder.WriteKey("name");
				jsonstreamEncoder.WriteString(namedPose.name);
				jsonstreamEncoder.WriteKey("position");
				namedPose.position.ToJSON(jsonstreamEncoder, false, false, false);
				jsonstreamEncoder.WriteKey("direction");
				namedPose.direction.ToJSON(jsonstreamEncoder, false, false, false);
				jsonstreamEncoder.EndObject();
				jsonstreamEncoder.InsertNewline();
			}
			jsonstreamEncoder.EndArray();
			jsonstreamEncoder.InsertNewline();
		}
		if (Blocksworld.signalNames.Length > 0)
		{
			bool flag = false;
			foreach (string text in Blocksworld.signalNames)
			{
				if (text != null && text.Length > 0)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				jsonstreamEncoder.WriteKey("signal-names");
				jsonstreamEncoder.BeginArray();
				jsonstreamEncoder.InsertNewline();
				foreach (string text2 in Blocksworld.signalNames)
				{
					string text3 = text2;
					if (text3 == null)
					{
						text3 = string.Empty;
					}
					jsonstreamEncoder.WriteString(text3);
				}
				jsonstreamEncoder.EndArray();
				jsonstreamEncoder.InsertNewline();
			}
		}
		if (Blocksworld.blockNames.Count > 0)
		{
			bool flag2 = false;
			for (int num3 = 0; num3 < list.Count; num3++)
			{
				Block key = list[num3];
				if (Blocksworld.blockNames.ContainsKey(key) && !string.IsNullOrEmpty(Blocksworld.blockNames[key]))
				{
					flag2 = true;
					break;
				}
			}
			if (flag2)
			{
				jsonstreamEncoder.WriteKey("block-names");
				jsonstreamEncoder.BeginArray();
				jsonstreamEncoder.InsertNewline();
				for (int num4 = 0; num4 < list.Count; num4++)
				{
					Block key2 = list[num4];
					string str = string.Empty;
					if (Blocksworld.blockNames.ContainsKey(key2))
					{
						str = Blocksworld.blockNames[key2];
					}
					jsonstreamEncoder.WriteString(str);
				}
				jsonstreamEncoder.EndArray();
				jsonstreamEncoder.InsertNewline();
			}
		}
		if (Blocksworld.launchIntoPlayMode)
		{
			jsonstreamEncoder.WriteKey("launch-into-play-mode");
			jsonstreamEncoder.WriteBool(Blocksworld.launchIntoPlayMode);
			jsonstreamEncoder.InsertNewline();
		}
		if (Blocksworld.staticLockPull)
		{
			jsonstreamEncoder.WriteKey("lock-pull");
			jsonstreamEncoder.WriteBool(Blocksworld.staticLockPull);
			jsonstreamEncoder.InsertNewline();
		}
		if (WorldSession.isProfileBuildSession())
		{
			jsonstreamEncoder.WriteKey("player-profile-world");
			jsonstreamEncoder.WriteBool(WorldSession.isProfileBuildSession());
			jsonstreamEncoder.InsertNewline();
		}
		if (Tutorial.numGivenBlocks != list.Count && Tutorial.safeState == null && Tutorial.state != TutorialState.None && Tutorial.state != TutorialState.Puzzle && Tutorial.state != TutorialState.BuildingCompleted && Tutorial.IsLastBlockIncomplete())
		{
			jsonstreamEncoder.WriteKey("last-tutorial-block-tile-rows");
			jsonstreamEncoder.InsertNewline();
			this.WriteTilesAsJSON(jsonstreamEncoder, list[list.Count - 1].tiles, false, Blocksworld.useCompactGafWriteRenamings);
			jsonstreamEncoder.InsertNewline();
		}
		if (Tutorial.manualPaintOrTexture.Count > 0)
		{
			jsonstreamEncoder.WriteKey("manual-paint-or-texture");
			jsonstreamEncoder.InsertNewline();
			this.WriteTilesAsJSON(jsonstreamEncoder, new List<List<Tile>>
			{
				Tutorial.manualPaintOrTexture
			}, false, Blocksworld.useCompactGafWriteRenamings);
			jsonstreamEncoder.InsertNewline();
		}
		if (Tutorial.completedManualPaintOrTexture.Count > 0)
		{
			jsonstreamEncoder.WriteKey("completed-manual-paint-or-texture");
			jsonstreamEncoder.InsertNewline();
			this.WriteTilesAsJSON(jsonstreamEncoder, new List<List<Tile>>
			{
				Tutorial.completedManualPaintOrTexture
			}, false, Blocksworld.useCompactGafWriteRenamings);
			jsonstreamEncoder.InsertNewline();
		}
		if (Tutorial.state != TutorialState.None)
		{
			jsonstreamEncoder.WriteKey("saved-tutorial-step");
			jsonstreamEncoder.WriteNumber((long)(Tutorial.step + Tutorial.savedStep));
			jsonstreamEncoder.InsertNewline();
		}
		jsonstreamEncoder.EndObject();
		return stringBuilder.ToString();
	}

	// Token: 0x06001927 RID: 6439 RVA: 0x000B29C4 File Offset: 0x000B0DC4
	private void WritePuzzleInventoryAndUsage(JSONStreamEncoder encoder, Dictionary<GAF, int> puzzleInventory, string inventoryKey, string usageKey)
	{
		if (puzzleInventory != null)
		{
			encoder.WriteKey(inventoryKey);
			GAF.WriteGAFCountDictionary(encoder, puzzleInventory);
			encoder.InsertNewline();
			if (Tutorial.state == TutorialState.Puzzle)
			{
				encoder.WriteKey(usageKey);
				GAF.WriteGAFCountDictionary(encoder, Scarcity.worldGAFUsage);
				encoder.InsertNewline();
			}
		}
	}

	// Token: 0x06001928 RID: 6440 RVA: 0x000B2A10 File Offset: 0x000B0E10
	public List<List<Tile>> LoadJSONTiles(JObject tileRows)
	{
		List<List<Tile>> list = new List<List<Tile>>();
		foreach (JObject jobject in tileRows.ArrayValue)
		{
			List<Tile> list2 = new List<Tile>();
			list.Add(list2);
			foreach (JObject obj in jobject.ArrayValue)
			{
				Tile tile = Tile.FromJSON(obj);
				if (tile == null)
				{
					return null;
				}
				list2.Add(tile);
			}
		}
		return list;
	}

	// Token: 0x06001929 RID: 6441 RVA: 0x000B2AE4 File Offset: 0x000B0EE4
	private void ReadSignalNames(JObject obj)
	{
		if (obj.ObjectValue.ContainsKey("signal-names"))
		{
			List<JObject> arrayValue = obj["signal-names"].ArrayValue;
			int num = 0;
			foreach (JObject jobject in arrayValue)
			{
				string stringValue = jobject.StringValue;
				if (string.IsNullOrEmpty(stringValue))
				{
					Blocksworld.signalNames[num] = null;
				}
				else
				{
					Blocksworld.signalNames[num] = stringValue;
				}
				num++;
			}
		}
	}

	// Token: 0x0600192A RID: 6442 RVA: 0x000B2B8C File Offset: 0x000B0F8C
	private void ReadBlockNames(JObject obj)
	{
		if (obj.ObjectValue.ContainsKey("block-names"))
		{
			List<JObject> arrayValue = obj["block-names"].ArrayValue;
			int num = 0;
			foreach (JObject jobject in arrayValue)
			{
				string stringValue = jobject.StringValue;
				if (!string.IsNullOrEmpty(stringValue))
				{
					List<Block> list = BWSceneManager.AllBlocks();
					if (num < list.Count)
					{
						Blocksworld.blockNames[list[num]] = stringValue;
					}
				}
				num++;
			}
		}
	}

	// Token: 0x0600192B RID: 6443 RVA: 0x000B2C44 File Offset: 0x000B1044
	private void ReadCameraPoses(JObject obj)
	{
		if (obj.ObjectValue.ContainsKey("camera-poses"))
		{
			List<JObject> arrayValue = obj["camera-poses"].ArrayValue;
			foreach (JObject jobject in arrayValue)
			{
				Dictionary<string, JObject> objectValue = jobject.ObjectValue;
				if (!objectValue.ContainsKey("name") || !objectValue.ContainsKey("position") || !objectValue.ContainsKey("direction"))
				{
					BWLog.Info("Invalid camera pose. Missing a key in object: 'name', 'position' or 'direction'");
				}
				else
				{
					string stringValue = objectValue["name"].StringValue;
					Vector3 position = objectValue["position"].Vector3Value();
					Vector3 direction = objectValue["direction"].Vector3Value();
					NamedPose namedPose = new NamedPose(stringValue, position, direction);
					Blocksworld.cameraPoses.Add(namedPose);
					Blocksworld.cameraPosesMap[namedPose.name] = namedPose;
				}
			}
		}
	}

	// Token: 0x0600192C RID: 6444 RVA: 0x000B2D64 File Offset: 0x000B1164
	public static JObject GetTileRows(JObject obj)
	{
		Dictionary<string, JObject> objectValue = obj.ObjectValue;
		JObject result;
		if (objectValue.TryGetValue("r", out result))
		{
			return result;
		}
		if (objectValue.TryGetValue("tile-rows", out result))
		{
			return result;
		}
		return null;
	}

	// Token: 0x0600192D RID: 6445 RVA: 0x000B2DA4 File Offset: 0x000B11A4
	private List<List<List<Tile>>> ParseWorldJSON(JObject obj)
	{
		List<List<List<Tile>>> list = new List<List<List<Tile>>>();
		foreach (JObject obj2 in obj["blocks"].ArrayValue)
		{
			List<List<Tile>> list2 = this.LoadJSONTiles(Blocksworld.GetTileRows(obj2));
			if (list2 == null)
			{
				return null;
			}
			list.Add(list2);
		}
		return list;
	}

	// Token: 0x0600192E RID: 6446 RVA: 0x000B2E30 File Offset: 0x000B1230
	public static void UnlockTutorialGAFs(string source)
	{
		JObject jobject = JSONDecoder.Decode(source);
		if (jobject == null)
		{
			return;
		}
		List<List<List<Tile>>> list = new List<List<List<Tile>>>();
		foreach (JObject obj in jobject["blocks"].ArrayValue)
		{
			JObject tileRows = Blocksworld.GetTileRows(obj);
			List<List<Tile>> list2 = new List<List<Tile>>();
			foreach (JObject jobject2 in tileRows.ArrayValue)
			{
				List<Tile> list3 = new List<Tile>();
				list2.Add(list3);
				foreach (JObject obj2 in jobject2.ArrayValue)
				{
					Tile tile = Tile.FromJSON(obj2);
					if (tile != null)
					{
						list3.Add(tile);
					}
				}
			}
			if (list2 == null)
			{
				return;
			}
			list.Add(list2);
		}
		Blocksworld.SetupTutorialGAFs(list);
	}

	// Token: 0x0600192F RID: 6447 RVA: 0x000B2F88 File Offset: 0x000B1388
	public static string GetWorldGAFUsageAsBlocksInventory(string worldSource)
	{
		JObject jobject = JSONDecoder.Decode(worldSource);
		if (jobject == null)
		{
			return null;
		}
		List<List<List<Tile>>> list = new List<List<List<Tile>>>();
		foreach (JObject obj in jobject["blocks"].ArrayValue)
		{
			JObject tileRows = Blocksworld.GetTileRows(obj);
			List<List<Tile>> list2 = new List<List<Tile>>();
			foreach (JObject jobject2 in tileRows.ArrayValue)
			{
				List<Tile> list3 = new List<Tile>();
				list2.Add(list3);
				foreach (JObject obj2 in jobject2.ArrayValue)
				{
					Tile tile = Tile.FromJSON(obj2);
					if (tile != null)
					{
						if (!tile.IsLocked())
						{
							list3.Add(tile);
						}
					}
				}
			}
			if (list2 == null)
			{
				return null;
			}
			list.Add(list2);
		}
		Dictionary<GAF, int> dictionary = new Dictionary<GAF, int>();
		Scarcity.GetInventoryUse(list, WorldType.ForComplexityCalculation, dictionary, false);
		HashSet<Predicate> infinitePredicates = new HashSet<Predicate>
		{
			Block.predicatePlaySoundDurational
		};
		return Scarcity.GetBlockIDInventoryString(dictionary, infinitePredicates);
	}

	// Token: 0x06001930 RID: 6448 RVA: 0x000B3128 File Offset: 0x000B1528
	public static void SetupTutorialGAFs(List<List<List<Tile>>> tilesLists)
	{
		Dictionary<GAF, int> dictionary = new Dictionary<GAF, int>();
		Scarcity.GetInventoryUse(tilesLists, WorldType.StepByStepTutorial, dictionary, false);
		HashSet<GAF> hashSet = new HashSet<GAF>();
		if (Blocksworld.unlockedGAFs == null)
		{
			Blocksworld.unlockedGAFs = new List<GAF>();
		}
		for (int i = 0; i < Blocksworld.unlockedGAFs.Count; i++)
		{
			hashSet.Add(Blocksworld.unlockedGAFs[i]);
		}
		foreach (GAF gaf in dictionary.Keys)
		{
			BlockItem blockItem = BlockItem.FindByGafPredicateNameAndArguments(gaf.Predicate.Name, gaf.Args);
			if (blockItem != null)
			{
				if (!hashSet.Contains(gaf))
				{
					hashSet.Add(gaf);
					Blocksworld.unlockedGAFs.Add(gaf);
					Predicate predicate = gaf.Predicate;
					if (predicate == Block.predicateTextureTo)
					{
						string textureName = (string)gaf.Args[0];
						Blocksworld.AddToPublicProvidedTextures(textureName);
					}
					else if (predicate == Block.predicatePaintTo)
					{
						Blocksworld.unlockedPaints.Add((string)gaf.Args[0]);
					}
				}
			}
			else
			{
				List<BlockItem> list = BlockItem.FindByGafPredicateName(gaf.Predicate.Name);
				foreach (BlockItem blockItem2 in list)
				{
					GAF item = new GAF(blockItem2.GafPredicateName, blockItem2.GafDefaultArgs);
					if (!hashSet.Contains(item))
					{
						hashSet.Add(item);
						Blocksworld.unlockedGAFs.Add(item);
					}
				}
			}
		}
		List<GAF> implicitGAFs = Blocksworld.GetImplicitGAFs(Blocksworld.unlockedGAFs);
		Blocksworld.unlockedGAFs.AddRange(implicitGAFs);
	}

	// Token: 0x06001931 RID: 6449 RVA: 0x000B3330 File Offset: 0x000B1730
	private void HideBlockWithDelayedShow(Block block)
	{
		if (block.go != null)
		{
			block.go.GetComponent<Renderer>().enabled = false;
			if (block.goShadow != null)
			{
				block.goShadow.GetComponent<Renderer>().enabled = false;
			}
			Blocksworld.AddUpdateCommand(new DelayedDelegateCommand(delegate()
			{
				if (Blocksworld.CurrentState == State.Build && !Tutorial.InTutorialOrPuzzle() && block.go != null)
				{
					block.go.GetComponent<Renderer>().enabled = true;
					if (block.goShadow != null)
					{
						block.goShadow.GetComponent<Renderer>().enabled = true;
					}
				}
			}, 10));
		}
	}

	// Token: 0x06001932 RID: 6450 RVA: 0x000B33BC File Offset: 0x000B17BC
	private bool LoadFromJSON(JObject obj, int version)
	{
		List<List<List<Tile>>> list = this.ParseWorldJSON(obj);
		if (list == null)
		{
			WorldSession.NotifyFileTooNew();
			return false;
		}
		foreach (List<List<Tile>> tiles in list)
		{
			Block block = Block.NewBlock(tiles, false, false);
			if (block != null)
			{
				BWSceneManager.AddBlock(block);
				if (!block.VisibleInPlayMode())
				{
					this.HideBlockWithDelayedShow(block);
				}
			}
		}
		List<Block> list2 = BWSceneManager.AllBlocks();
		if (list2.Count == 0)
		{
			return false;
		}
		int i = 0;
		foreach (JObject jobject in obj["connections"].ArrayValue)
		{
			foreach (JObject obj2 in jobject.ArrayValue)
			{
				list2[i].connections.Add(list2[(int)obj2]);
			}
			i++;
		}
		i = 0;
		foreach (JObject jobject2 in obj["connectionTypes"].ArrayValue)
		{
			foreach (JObject obj3 in jobject2.ArrayValue)
			{
				list2[i].connectionTypes.Add((int)obj3);
			}
			i++;
		}
		List<Block> list3 = new List<Block>();
		for (i = 0; i < list2.Count; i++)
		{
			Block block2 = list2[i];
			int num = block2.connections.IndexOf(block2);
			if (num != -1)
			{
				block2.connections.RemoveAt(num);
				block2.connectionTypes.RemoveAt(num);
			}
			if (block2.connections.Count != block2.connectionTypes.Count)
			{
				BWLog.Info("Bad connection data for block " + block2.BlockType());
				ConnectednessGraph.RemoveSafe(block2);
				list3.Add(block2);
			}
		}
		if (list3.Count > 0)
		{
			ConnectednessGraph.Update(list3);
		}
		Dictionary<string, JObject> objectValue = obj.ObjectValue;
		if (objectValue.ContainsKey("id"))
		{
			Blocksworld.currentWorldId = obj["id"].StringValue;
		}
		else
		{
			Blocksworld.currentWorldId = null;
		}
		if (objectValue.ContainsKey("frozen-in-terrain"))
		{
			List<JObject> arrayValue = objectValue["frozen-in-terrain"].ArrayValue;
			for (int j = 0; j < arrayValue.Count; j++)
			{
				if (j < list2.Count)
				{
					list2[j].frozenInTerrainStatus = arrayValue[j].IntValue;
				}
			}
		}
		if (objectValue.ContainsKey("camera"))
		{
			this.lastLoadedCameraObj = obj["camera"];
			Blocksworld.blocksworldCamera.PlaceCamera(this.lastLoadedCameraObj["rotation"].QuaternionValue().eulerAngles, this.lastLoadedCameraObj["position"].Vector3Value());
			if (this.lastLoadedCameraObj.ObjectValue.ContainsKey("playAngle"))
			{
				Blocksworld.blocksworldCamera.manualCameraAngle = this.lastLoadedCameraObj["playAngle"].FloatValue;
			}
			if (this.lastLoadedCameraObj.ObjectValue.ContainsKey("playDistance"))
			{
				Blocksworld.blocksworldCamera.manualCameraDistance = this.lastLoadedCameraObj["playDistance"].FloatValue;
			}
			Blocksworld.blocksworldCamera.CameraStateLoaded();
		}
		if (objectValue.ContainsKey("selection-locked"))
		{
			List<JObject> arrayValue2 = obj["selection-locked"].ArrayValue;
			foreach (JObject obj4 in arrayValue2)
			{
				int num2 = (int)obj4;
				if (num2 < list2.Count)
				{
					Blocksworld.editorSelectionLocked.Add(list2[num2]);
				}
			}
		}
		this.ReadSignalNames(obj);
		this.ReadBlockNames(obj);
		this.ReadCameraPoses(obj);
		BlockGroups.GatherBlockGroups(list2);
		Blocksworld.bw.StatePlayUpdate();
		list2.ForEach(delegate(Block b)
		{
			b.oldPos = Util.nullVector3;
			b.lastShadowHitDistance = -2f;
		});
		Resources.UnloadUnusedAssets();
		GC.Collect();
		if (version < 11)
		{
			for (i = 0; i < list2.Count; i++)
			{
				Block block3 = list2[i];
				foreach (List<Tile> list4 in block3.tiles)
				{
					foreach (Tile tile in list4)
					{
						if (tile.gaf.Predicate.Name == "Block.ButtonInput")
						{
							object[] array = Util.CopyArray<object>(tile.gaf.Args);
							string text = (string)tile.gaf.Args[0];
							switch (text)
							{
							case "L1":
							case "L2":
								array[0] = "L";
								break;
							case "L3":
								array[0] = "Left";
								break;
							case "L4":
								array[0] = "Right";
								break;
							case "R1":
								array[0] = "Up";
								break;
							case "R2":
								array[0] = "Down";
								break;
							case "R3":
							case "R4":
								array[0] = "R";
								break;
							}
							tile.gaf = new GAF(tile.gaf.Predicate, array);
						}
						if (tile.gaf.Predicate.Name == "Block.DeviceTilt")
						{
							tile.gaf = new GAF("Block.ButtonInput", new object[]
							{
								((float)tile.gaf.Args[0] >= 0f) ? "R" : "L"
							});
						}
					}
				}
			}
			for (i = 0; i < list2.Count; i++)
			{
				Block block4 = list2[i];
				string paint = block4.GetPaint(0);
				if (paint != null)
				{
					if (!(paint == "Celeste"))
					{
						if (paint == "Grass")
						{
							block4.PaintTo("Green", true, 0);
						}
					}
					else
					{
						block4.PaintTo("Blue", true, 0);
					}
				}
			}
		}
		if (obj.ObjectValue.ContainsKey("puzzle-gafs"))
		{
			Scarcity.puzzleInventory = Scarcity.ParseScarcityData(obj["puzzle-gafs"]);
			if (obj.ObjectValue.ContainsKey("puzzle-gaf-usage"))
			{
				Scarcity.puzzleGAFUsage = Scarcity.ParseScarcityData(obj["puzzle-gaf-usage"]);
			}
			else
			{
				Scarcity.puzzleGAFUsage = new Dictionary<GAF, int>();
			}
		}
		if (obj.ObjectValue.ContainsKey("puzzle-gafs-after-step-by-step"))
		{
			Scarcity.puzzleInventoryAfterStepByStep = Scarcity.ParseScarcityData(obj["puzzle-gafs-after-step-by-step"]);
			if (obj.ObjectValue.ContainsKey("puzzle-gaf-usage-after-step-by-step"))
			{
				Scarcity.puzzleGAFUsageAfterStepByStep = Scarcity.ParseScarcityData(obj["puzzle-gaf-usage-after-step-by-step"]);
			}
			else
			{
				Scarcity.puzzleGAFUsageAfterStepByStep = new Dictionary<GAF, int>();
			}
		}
		if (obj.ObjectValue.ContainsKey("launch-into-play-mode"))
		{
			Blocksworld.launchIntoPlayMode = (bool)obj["launch-into-play-mode"];
		}
		if (obj.ObjectValue.ContainsKey("lock-pull"))
		{
			Blocksworld.staticLockPull = (bool)obj["lock-pull"];
		}
		Tutorial.savedProgressTiles = null;
		if (obj.ObjectValue.ContainsKey("last-tutorial-block-tile-rows"))
		{
			Tutorial.savedProgressTiles = this.LoadJSONTiles(obj["last-tutorial-block-tile-rows"]);
		}
		Tutorial.manualPaintOrTexture.Clear();
		if (obj.ObjectValue.ContainsKey("manual-paint-or-texture"))
		{
			Tutorial.manualPaintOrTexture = this.LoadJSONTiles(obj["manual-paint-or-texture"])[0];
		}
		Tutorial.completedManualPaintOrTexture.Clear();
		if (obj.ObjectValue.ContainsKey("completed-manual-paint-or-texture"))
		{
			Tutorial.completedManualPaintOrTexture = this.LoadJSONTiles(obj["completed-manual-paint-or-texture"])[0];
		}
		return true;
	}

	// Token: 0x06001933 RID: 6451 RVA: 0x000B3E38 File Offset: 0x000B2238
	public void ClearWorldState()
	{
		if (Blocksworld.CurrentState == State.Play)
		{
			this.Stop(true, false);
		}
		Blocksworld.worldSky = null;
		Blocksworld.worldOcean = null;
		Blocksworld.worldOceanBlock = null;
		Blocksworld.rewardVisualizationGafs = null;
		Blocksworld.fpsCounter = null;
		Blocksworld.dynamicalLightChangers.Clear();
		if (Blocksworld.f3PressedInCurrentWorld)
		{
			Tutorial.Stop(false);
		}
		Blocksworld.f3PressedInCurrentWorld = false;
		Blocksworld.signalNames = new string[26];
		Blocksworld.blockNames = new Dictionary<Block, string>();
		BWSceneManager.ClearTerrainBlocks();
		BlockAbstractWater.waterCubes.Clear();
		BlockGroups.Clear();
		Blocksworld.editorSelectionLocked.Clear();
		Blocksworld.recentSelectionUnlockedBlock = null;
		Blocksworld.keyLReleased = true;
		BlockMissile.WorldLoaded();
		if (Options.Cowlorded)
		{
			Blocksworld.fpsCounter = new FpsCounter();
		}
		Blocksworld.weather.Stop();
		Blocksworld.weather = WeatherEffect.clear;
		SignalNameTileParameter.ResetDefaultIndex();
		this.Reset();
		Blocksworld.blocksworldCamera.Reset();
		Scarcity.puzzleInventory = null;
		Scarcity.puzzleInventoryAfterStepByStep = null;
		Blocksworld.launchIntoPlayMode = false;
		Blocksworld.lockPull = false;
		Blocksworld.cameraPoses.Clear();
		Blocksworld.cameraPosesMap.Clear();
		Blocksworld.staticLockPull = false;
		Scarcity.worldGAFUsage = new Dictionary<GAF, int>();
	}

	// Token: 0x06001934 RID: 6452 RVA: 0x000B3F54 File Offset: 0x000B2354
	public bool LoadFromString(string s)
	{
		this.Reset();
		Blocksworld.blocksworldCamera.Reset();
		Scarcity.puzzleInventory = null;
		Scarcity.puzzleInventoryAfterStepByStep = null;
		Blocksworld.launchIntoPlayMode = false;
		Blocksworld.lockPull = false;
		Blocksworld.cameraPoses.Clear();
		Blocksworld.cameraPosesMap.Clear();
		Blocksworld.staticLockPull = false;
		bool result = true;
		try
		{
			PerformaceTest performaceTest = new PerformaceTest("Blocksworld.LoadFromString");
			performaceTest.Start();
			performaceTest.StartSubTest("JSONDecoder.Decode");
			JObject obj = JSONDecoder.Decode(s);
			performaceTest.StopSubTest();
			int version = 25;
			performaceTest.StartSubTest("LoadFromJSON");
			result = this.LoadFromJSON(obj, version);
			performaceTest.StopSubTest();
			performaceTest.Stop();
			performaceTest.DebugLogTestResults();
		}
		catch (Exception ex)
		{
			BWLog.Warning("Unable to decode JSON for '" + s + "'");
			BWLog.Warning(string.Concat(new string[]
			{
				ex.GetType().Name,
				" exception message: ",
				ex.Message,
				" stacktrace: ",
				ex.StackTrace
			}));
			result = false;
		}
		return result;
	}

	// Token: 0x06001935 RID: 6453 RVA: 0x000B4070 File Offset: 0x000B2470
	public void ProcessLoadedWorld()
	{
		Blocksworld.UI.gameObject.SetActive(true);
		Blocksworld.buildPanel.ResetState();
		Blocksworld.buildPanel.UpdateGestureRecognizer(Blocksworld.recognizer);
		Blocksworld.UI.Controls.ResetContolVariants();
		Blocksworld.UI.Controls.SetControlVariantsFromBlocks(BWSceneManager.AllBlocks());
		Blocksworld.UI.Controls.ResetTiltPrompt();
		Blocksworld.UpdateLightColor(true);
		Tutorial.UpdateCheatButton();
		History.Initialize();
		Blocksworld.RemoveUnusedAssets();
		Blocksworld.inBackground = false;
	}

	// Token: 0x06001936 RID: 6454 RVA: 0x000B40F4 File Offset: 0x000B24F4
	private void SetDynamicalLightBlocks()
	{
		List<Block> list = BWSceneManager.AllBlocks();
		for (int i = 0; i < list.Count; i++)
		{
			Block block = list[i];
			if (block.HasDynamicalLight() && !Blocksworld.dynamicalLightChangers.Contains(block))
			{
				Blocksworld.dynamicalLightChangers.Add(block);
			}
		}
	}

	// Token: 0x06001937 RID: 6455 RVA: 0x000B414C File Offset: 0x000B254C
	private void Reset()
	{
		Blocksworld.Deselect(false, true);
		TBox.Show(false);
		List<Block> list = BWSceneManager.AllBlocks();
		for (int i = 0; i < list.Count; i++)
		{
			Block block = list[i];
			block.Destroy();
			if (block is BlockTerrain)
			{
				BWSceneManager.RemoveTerrainBlock((BlockTerrain)block);
			}
		}
		BWSceneManager.ClearBlocks();
		this.ClearChunks();
		Blocksworld.mainCamera.backgroundColor = this._defaultBackgroundColor;
		Blocksworld.locked.Clear();
		foreach (Block block2 in Tutorial.blocks)
		{
			block2.Destroy();
		}
		Tutorial.blocks.Clear();
		Blocksworld.buildPanel.PositionReset(false);
		this.copyModelAnimationCommand = new CopyModelAnimationCommand();
		this.saveModelAnimationCommand = new SaveModelAnimationCommand();
	}

	// Token: 0x06001938 RID: 6456 RVA: 0x000B4248 File Offset: 0x000B2648
	public static void GenerateTerrain(bool flat = false)
	{
		int num = 5;
		int num2 = 5;
		int num3 = 27;
		int num4 = num3;
		float num5 = Mathf.Floor((float)(num / 2));
		float num6 = Mathf.Floor((float)(num2 / 2));
		Blocksworld.CreateTerrainBlock("Sky", Vector3.zero, Vector3.one, "Celeste", "Clouds");
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				float num7 = -Mathf.Floor((float)(num4 / 2));
				if (flat)
				{
					if (j == 0 || j == num - 1 || i == 0 || i == num2 - 1)
					{
						num7 += 2f + Mathf.Round((float)UnityEngine.Random.Range(0, 5));
					}
				}
				else if ((float)j != num5 || (float)i != num6)
				{
					num7 += Mathf.Round((float)UnityEngine.Random.Range(0, 10));
				}
				Vector3 a = new Vector3(((float)j - num5) * (float)num3, num7, ((float)i - num6) * (float)num3);
				Blocksworld.CreateTerrainBlock("Terrain Cube", a + Mathf.Floor((float)(num3 / 2)) * Vector3.up, new Vector3((float)num3, 1f, (float)num3), "Green", "Checkered");
				Blocksworld.CreateTerrainBlock("Terrain Cube", a - Vector3.up, new Vector3((float)num3, (float)num4, (float)num3), "Beige", "Checkered");
			}
		}
		ConnectednessGraph.Update(BWSceneManager.AllBlocks());
	}

	// Token: 0x06001939 RID: 6457 RVA: 0x000B43C4 File Offset: 0x000B27C4
	private static void CreateTerrainBlock(string type, Vector3 pos, Vector3 scale, string color, string texture)
	{
		List<List<Tile>> list = new List<List<Tile>>();
		list.Add(new List<Tile>());
		list[0].Add(new Tile(new GAF("Meta.Stop", new object[0])));
		list[0].Add(new Tile(new GAF("Block.Create", new object[]
		{
			type
		})));
		list[0].Add(new Tile(new GAF("Block.MoveTo", new object[]
		{
			pos
		})));
		list[0].Add(new Tile(new GAF("Block.RotateTo", new object[]
		{
			Vector3.zero
		})));
		list[0].Add(new Tile(new GAF("Block.PaintTo", new object[]
		{
			color
		})));
		list[0].Add(new Tile(new GAF("Block.TextureTo", new object[]
		{
			texture,
			Vector3.zero
		})));
		list[0].Add(new Tile(new GAF("Block.ScaleTo", new object[]
		{
			scale
		})));
		list.Add(new List<Tile>());
		list[1].Add(new Tile(new GAF("Meta.Then", new object[0])));
		list[1].Add(new Tile(new GAF("Block.Locked", new object[0])));
		list.Add(new List<Tile>());
		list[2].Add(new Tile(new GAF("Meta.Then", new object[0])));
		list[2].Add(new Tile(new GAF("Block.Fixed", new object[0])));
		list.Add(new List<Tile>());
		list[3].Add(new Tile(new GAF("Meta.Then", new object[0])));
		Block block;
		if (type == "Sky")
		{
			block = new BlockSky(type, list);
		}
		else
		{
			block = new Block(list);
		}
		block.Reset(false);
		BWSceneManager.AddBlock(block);
	}

	// Token: 0x0600193A RID: 6458 RVA: 0x000B45FC File Offset: 0x000B29FC
	private bool Close(Vector3 v1, Vector3 v2)
	{
		return (v2 - v1).sqrMagnitude < 0.1f;
	}

	// Token: 0x0600193B RID: 6459 RVA: 0x000B4620 File Offset: 0x000B2A20
	public static List<List<Tile>> CloneBlockTiles(List<List<Tile>> tiles, Dictionary<Tile, Tile> origToCopy = null, bool excludeFirstRow = false, bool ignoreLockedGroupTiles = false)
	{
		List<List<Tile>> list = new List<List<Tile>>();
		int num = (!excludeFirstRow) ? 0 : 1;
		for (int i = num; i < tiles.Count; i++)
		{
			List<Tile> list2 = tiles[i];
			List<Tile> list3 = new List<Tile>();
			foreach (Tile tile in list2)
			{
				if (!ignoreLockedGroupTiles || tile.gaf.Predicate != Block.predicateGroup || !(BlockGroups.GroupType(tile) == "locked-model"))
				{
					Tile tile2 = tile.Clone();
					list3.Add(tile2);
					if (origToCopy != null)
					{
						origToCopy[tile] = tile2;
					}
				}
			}
			list.Add(list3);
		}
		return list;
	}

	// Token: 0x0600193C RID: 6460 RVA: 0x000B4710 File Offset: 0x000B2B10
	public static List<List<Tile>> CloneBlockTiles(Block block, bool excludeFirstRow = false, bool ignoreLockedGroupTiles = false)
	{
		return Blocksworld.CloneBlockTiles(block.tiles, null, excludeFirstRow, ignoreLockedGroupTiles);
	}

	// Token: 0x0600193D RID: 6461 RVA: 0x000B4720 File Offset: 0x000B2B20
	public static Block CloneBlock(Block block)
	{
		Block block2 = Block.NewBlock(Blocksworld.CloneBlockTiles(block, false, false), false, false);
		BWSceneManager.AddBlock(block2);
		return block2;
	}

	// Token: 0x0600193E RID: 6462 RVA: 0x000B4744 File Offset: 0x000B2B44
	private void UpdateCopyPaste()
	{
		if (Blocksworld.CurrentState == State.Play)
		{
			return;
		}
		if (MappedInput.InputDown(MappableInput.COPY) && (Blocksworld.selectedBlock != null || Blocksworld.selectedBunch != null))
		{
			this.ButtonCopyTapped();
		}
		if (MappedInput.InputDown(MappableInput.CUT) && (Blocksworld.selectedBlock != null || Blocksworld.selectedBunch != null))
		{
			this.ButtonCopyTapped();
			List<Block> list = new List<Block>();
			if (Blocksworld.selectedBlock != null)
			{
				list.Add(Blocksworld.selectedBlock);
			}
			else if (Blocksworld.selectedBunch != null)
			{
				list.AddRange(Blocksworld.selectedBunch.blocks);
			}
			Blocksworld.DeselectBlock(false, true);
			foreach (Block block in list)
			{
				if (block == Blocksworld.worldOceanBlock)
				{
					Blocksworld.worldOceanBlock = null;
					Blocksworld.worldOcean = null;
				}
				Blocksworld.DestroyBlock(block);
			}
		}
		if (this.keyboardPasteInProgress)
		{
			if (MappedInput.InputPressed(MappableInput.PASTE) && !Input.GetMouseButtonDown(0))
			{
				TBox.ContinueMove(NormalizedInput.mousePosition, false);
			}
			else
			{
				this.keyboardPasteInProgress = false;
				TBox.StopMove();
			}
		}
		else if (MappedInput.InputDown(MappableInput.PASTE) && Blocksworld.clipboard.modelCopyPasteBuffer.Count > 0 && Blocksworld.mouseBlock != null)
		{
			Dictionary<GAF, int> missing = new Dictionary<GAF, int>();
			List<GAF> uniquesInModel = new List<GAF>();
			if (Blocksworld.clipboard.AvailablityCountForBlockList(Blocksworld.clipboard.modelCopyPasteBuffer, missing, uniquesInModel) == 0)
			{
				Blocksworld.UI.Dialog.ShowPasteFailInfo(missing, uniquesInModel, "Not enough blocks!");
			}
			else
			{
				this.keyboardPasteInProgress = true;
				Vector3 vector = Util.Round(Blocksworld.mouseBlockHitPosition);
				List<Block> list2 = this.PasteBlocks(Blocksworld.clipboard.modelCopyPasteBuffer, vector);
				foreach (Block block2 in list2)
				{
					block2.Update();
				}
				Vector2 touchPos = Blocksworld.mainCamera.WorldToScreenPoint(vector);
				Blocksworld.selectedBlock = null;
				Blocksworld.selectedBunch = null;
				Blocksworld.SelectBunch(list2, true, true);
				TBox.StartMove(touchPos, TBox.MoveMode.Raycast);
				Bounds bounds = Util.ComputeBoundsWithSize(list2, false);
				Vector3 a = Blocksworld.mouseBlockNormal;
				float num = Mathf.Abs(a.x);
				float num2 = Mathf.Abs(a.y);
				float num3 = Mathf.Abs(a.z);
				float d;
				if (num2 >= num && num2 >= num3)
				{
					a = Vector3.up * Mathf.Sign(a.y);
					d = bounds.size.y;
				}
				else if (num > num3)
				{
					a = Vector3.right * Mathf.Sign(a.x);
					d = bounds.size.x;
				}
				else
				{
					a = Vector3.forward * Mathf.Sign(a.z);
					d = bounds.size.z;
				}
				vector = Util.Round(Blocksworld.mouseBlockHitPosition + d * a);
				Vector2 touchPos2 = Blocksworld.mainCamera.WorldToScreenPoint(vector);
				bool flag = TBox.ContinueMove(touchPos2, true);
				TBox.PaintRed(!flag);
				TBox.Update();
			}
		}
	}

	// Token: 0x0600193F RID: 6463 RVA: 0x000B4AD4 File Offset: 0x000B2ED4
	private void UpdateWASDEQMouseCameraMovement()
	{
		Transform transform = Blocksworld.cameraTransform;
		Vector3 position = transform.position;
		float d = 15f;
		if (Input.GetMouseButton(1))
		{
			Vector3 vector = Vector3.zero;
			Vector3 b = transform.forward * d;
			float d2 = 1f;
			if (MappedInput.InputPressed(MappableInput.SPEED_MULTIPLIER))
			{
				d2 = 4f;
			}
			else if (MappedInput.InputPressed(MappableInput.SLOW_MULTIPLIER))
			{
				d2 = 0.05f;
			}
			if (MappedInput.InputPressed(MappableInput.CAM_FORWARD))
			{
				vector += transform.forward;
			}
			if (MappedInput.InputPressed(MappableInput.CAM_BACK))
			{
				vector -= transform.forward;
			}
			if (MappedInput.InputPressed(MappableInput.CAM_LEFT))
			{
				vector -= transform.right;
			}
			if (MappedInput.InputPressed(MappableInput.CAM_RIGHT))
			{
				vector += transform.right;
			}
			if (MappedInput.InputPressed(MappableInput.CAM_UP))
			{
				vector += transform.up;
			}
			if (MappedInput.InputPressed(MappableInput.CAM_DOWN))
			{
				vector -= transform.up;
			}
			if (vector.sqrMagnitude > 0.001f)
			{
				vector = vector.normalized * d2;
				float num = Options.WASDMovementSpeedup;
				if (num < 0.0001f)
				{
					num = 1f;
				}
				vector *= num;
			}
			if (this.mousePositionDelta.sqrMagnitude > 0.01f)
			{
			}
			Quaternion rotation = Quaternion.AngleAxis(0.25f * this.mousePositionDelta.x, transform.up);
			Quaternion rotation2 = Quaternion.AngleAxis(-0.25f * this.mousePositionDelta.y, transform.right);
			Vector3 normalized = (this.wasdeqMouseCamLookAtTarget - this.wasdeqMouseCamPosTarget).normalized;
			Vector3 vector2 = rotation * normalized;
			vector2 = rotation2 * vector2;
			b = vector2 * d;
			bool flag = true;
			if (flag)
			{
				this.wasdeqMouseCamPosTarget += vector;
				this.wasdeqMouseCamLookAtTarget = this.wasdeqMouseCamPosTarget + b;
			}
			float wasdsmoothness = Options.WASDSmoothness;
			Vector3 vector3 = wasdsmoothness * position + (1f - wasdsmoothness) * this.wasdeqMouseCamPosTarget;
			Vector3 vector4 = wasdsmoothness * (position + Blocksworld.cameraTransform.forward * d) + (1f - wasdsmoothness) * this.wasdeqMouseCamLookAtTarget;
			Blocksworld.blocksworldCamera.SetCameraPosition(vector3);
			Blocksworld.cameraTransform.LookAt(vector4);
			Blocksworld.blocksworldCamera.SetTargetPosition(vector4);
			Blocksworld.blocksworldCamera.SetFilteredPositionAndTarget();
		}
		else
		{
			this.wasdeqMouseCamPosTarget = position;
			this.wasdeqMouseCamLookAtTarget = position + Blocksworld.cameraTransform.forward * d;
		}
	}

	// Token: 0x06001940 RID: 6464 RVA: 0x000B4D9C File Offset: 0x000B319C
	private void UpdateWASDEQOrbitMouseCameraMovement()
	{
		UnityEngine.Debug.Log("Right click cam orbit");
		float num = 1f;
		if (MappedInput.InputPressed(MappableInput.SPEED_MULTIPLIER))
		{
			num = 4f;
		}
		else if (MappedInput.InputPressed(MappableInput.SLOW_MULTIPLIER))
		{
			num = 0.05f;
		}
		Vector3 vector = (Blocksworld.selectedBlock == null) ? Blocksworld.selectedBunch.GetPosition() : Blocksworld.selectedBlock.GetPosition();
		Vector3 v = (Blocksworld.selectedBlock == null) ? Blocksworld.selectedBunch.GetScale() : Blocksworld.selectedBlock.Scale();
		Vector3 position = Blocksworld.cameraTransform.position;
		float num2 = Vector3.Distance(vector, position);
		if (MappedInput.InputPressed(MappableInput.CAM_FORWARD))
		{
			num2 -= num;
			num2 = Mathf.Max(num2, Util.MinComponent(v));
		}
		if (MappedInput.InputPressed(MappableInput.CAM_BACK))
		{
			num2 += num;
		}
		Vector3 normalized = (vector - position).normalized;
		Vector3 vector2 = vector - normalized * num2;
		Blocksworld.blocksworldCamera.SetCameraPosition(vector2);
		Blocksworld.blocksworldCamera.SetTargetPosition(vector);
		Blocksworld.cameraTransform.LookAt(vector);
		if (MappedInput.InputPressed(MappableInput.CAM_LEFT))
		{
			Blocksworld.blocksworldCamera.HardOrbit(-Vector2.right * 3f * num);
		}
		if (MappedInput.InputPressed(MappableInput.CAM_RIGHT))
		{
			Blocksworld.blocksworldCamera.HardOrbit(Vector2.right * 3f * num);
		}
		if (MappedInput.InputPressed(MappableInput.CAM_UP))
		{
			Blocksworld.blocksworldCamera.HardOrbit(Vector2.up * 3f * num);
		}
		if (MappedInput.InputPressed(MappableInput.CAM_DOWN))
		{
			Blocksworld.blocksworldCamera.HardOrbit(-Vector2.up * 3f * num);
		}
	}

	// Token: 0x06001941 RID: 6465 RVA: 0x000B4F6C File Offset: 0x000B336C
	private void UpdateAxisLockManipulation()
	{
		if (Blocksworld.CurrentState == State.Build && Options.AxisLockMoveAndScaleEnabled)
		{
			if ((Blocksworld.selectedBlock != null || Blocksworld.selectedBunch != null) && (this.controlPressesInShortTime == 0 || Time.time < this.lastControlPressTime + 0.5f))
			{
				if (MappedInput.InputDown(MappableInput.AXIS_LOCK))
				{
					this.lastControlPressTime = Time.time;
					this.controlPressesInShortTime++;
				}
			}
			else if (!MappedInput.InputPressed(MappableInput.AXIS_LOCK))
			{
				this.controlPressesInShortTime = 0;
			}
			int num = (this.controlPressesInShortTime - 1) % 3;
			if (num == 0 || this.controlPressesInShortTime == 0)
			{
				Blocksworld.constrainedManipulationAxis = Vector3.up;
			}
			else if (num == 1)
			{
				Blocksworld.constrainedManipulationAxis = Vector3.forward;
			}
			else if (num == 2)
			{
				Blocksworld.constrainedManipulationAxis = Vector3.right;
			}
			if (Time.time < this.lastControlPressTime + 1f)
			{
				Vector3 a = Vector3.zero;
				bool flag = false;
				if (Blocksworld.selectedBlock != null)
				{
					a = Blocksworld.selectedBlock.goT.position;
					flag = true;
				}
				else if (Blocksworld.selectedBunch != null)
				{
					a = Util.ComputeCenter(Blocksworld.selectedBunch.blocks, false);
					flag = true;
				}
				if (flag)
				{
					UnityEngine.Debug.DrawLine(a - Blocksworld.constrainedManipulationAxis * 100f, a + Blocksworld.constrainedManipulationAxis * 100f, Color.white);
				}
			}
		}
		else
		{
			Blocksworld.constrainedManipulationAxis = Vector3.up;
		}
	}

	// Token: 0x06001942 RID: 6466 RVA: 0x000B50F8 File Offset: 0x000B34F8
	private void UpdateQuickScroll()
	{
		if (Blocksworld.CurrentState == State.Build && Options.QuickKeyScroll && Blocksworld.buildPanel.IsShowing())
		{
			if (this.quickScrollKeys.Count > 0 && Time.time > this.lastQuickScrollPressTime + 0.2f)
			{
				int num = 0;
				int num2 = 1;
				for (int i = this.quickScrollKeys.Count - 1; i >= 0; i--)
				{
					num += this.quickScrollKeys[i] * num2;
					num2 *= 10;
				}
				Tile firstTileInSection = Blocksworld.buildPanel.GetFirstTileInSection(num);
				if (firstTileInSection != null)
				{
					Blocksworld.buildPanel.ScrollToVisible(firstTileInSection, true, true, true);
				}
				this.quickScrollKeys.Clear();
			}
			int num3 = -1;
			if (MappedInput.InputDown(MappableInput.TILES_1))
			{
				num3 = 1;
			}
			if (MappedInput.InputDown(MappableInput.TILES_2))
			{
				num3 = 2;
			}
			if (MappedInput.InputDown(MappableInput.TILES_3))
			{
				num3 = 3;
			}
			if (MappedInput.InputDown(MappableInput.TILES_4))
			{
				num3 = 4;
			}
			if (MappedInput.InputDown(MappableInput.TILES_5))
			{
				num3 = 5;
			}
			if (MappedInput.InputDown(MappableInput.TILES_6))
			{
				num3 = 6;
			}
			if (MappedInput.InputDown(MappableInput.TILES_7))
			{
				num3 = 7;
			}
			if (MappedInput.InputDown(MappableInput.TILES_8))
			{
				num3 = 8;
			}
			if (MappedInput.InputDown(MappableInput.TILES_9))
			{
				num3 = 9;
			}
			if (MappedInput.InputDown(MappableInput.TILES_0))
			{
				num3 = 0;
			}
			if (num3 > -1)
			{
				this.quickScrollKeys.Add(num3);
				this.lastQuickScrollPressTime = Time.time;
			}
		}
		else
		{
			this.quickScrollKeys.Clear();
			this.lastQuickScrollPressTime = -1f;
		}
	}

	// Token: 0x06001943 RID: 6467 RVA: 0x000B5287 File Offset: 0x000B3687
	private void UpdateTimeChanges()
	{
		if (Blocksworld.CurrentState != State.Play)
		{
			return;
		}
	}

	// Token: 0x06001944 RID: 6468 RVA: 0x000B5298 File Offset: 0x000B3698
	private void UpdateGlueJointShapeDisplays()
	{
		if (Options.GlueVolumeDisplay)
		{
			foreach (Block block in BWSceneManager.AllBlocks())
			{
				CollisionTest.DrawCollisionMeshes(block.glueMeshes, Color.red);
			}
		}
		if (Options.JointMeshDisplay)
		{
			foreach (Block block2 in BWSceneManager.AllBlocks())
			{
				CollisionTest.DrawCollisionMeshes(block2.jointMeshes, Color.black);
			}
		}
		if (Options.ShapeVolumeDisplay)
		{
			foreach (Block block3 in BWSceneManager.AllBlocks())
			{
				CollisionTest.DrawCollisionMeshes(block3.shapeMeshes, Color.blue);
			}
		}
	}

	// Token: 0x06001945 RID: 6469 RVA: 0x000B53C4 File Offset: 0x000B37C4
	private void UpdateTutorialTiles()
	{
		if (Blocksworld.selectedBlock == null || Blocksworld.CurrentState != State.Build)
		{
			return;
		}
		Transform transform = Blocksworld.cameraTransform;
		RaycastHit raycastHit;
		if (MappedInput.InputDown(MappableInput.APPLY_TUTORIAL_TILE) && Physics.Raycast(Blocksworld.mainCamera.ScreenPointToRay(Blocksworld.mouse * NormalizedScreen.scale), out raycastHit))
		{
			string text = "Block.TutorialPaintExistingBlock";
			string text2 = "Black";
			bool flag = true;
			if (MappedInput.InputPressed(MappableInput.APPLY_TUTORIAL_ALL))
			{
				flag = false;
				text = "Block.TutorialTextureExistingBlock";
				text2 = "Plain";
			}
			Block block = BWSceneManager.FindBlock(raycastHit.collider.gameObject, true);
			int num = 0;
			if (block != null)
			{
				if (flag)
				{
					text2 = block.GetPaint(0);
				}
				else
				{
					text2 = block.GetTexture(0);
				}
				num = BWSceneManager.IndexOfBlock(block);
				if (num == -1)
				{
					OnScreenLog.AddLogItem("Failed to find block index", 5f, true);
					num = 0;
				}
			}
			else
			{
				OnScreenLog.AddLogItem("Failed to find the target block. Using index 0", 5f, true);
			}
			List<Tile> list = new List<Tile>();
			list.Add(new Tile(new GAF("Meta.Then", new object[0])));
			list.Add(new Tile(new GAF(text, new object[]
			{
				num,
				text2,
				transform.position,
				raycastHit.point
			})));
			List<List<Tile>> tiles = Blocksworld.selectedBlock.tiles;
			tiles.Insert(tiles.Count - 1, list);
			Blocksworld.ShowSelectedBlockPanel();
			OnScreenLog.AddLogItem("Added " + text + " tile", 5f, true);
			History.AddStateIfNecessary();
		}
		bool flag2 = MappedInput.InputDown(MappableInput.TUTORIAL_ROTATE_BLOCK);
		RaycastHit raycastHit2;
		if (flag2 && Physics.Raycast(Blocksworld.mainCamera.ScreenPointToRay(Blocksworld.mouse * NormalizedScreen.scale), out raycastHit2))
		{
			string text3 = "Block.TutorialRotateExistingBlock";
			Block block2 = BWSceneManager.FindBlock(raycastHit2.collider.gameObject, true);
			int num2 = 0;
			Vector3 zero = Vector3.zero;
			if (block2 != null)
			{
				num2 = BWSceneManager.IndexOfBlock(block2);
				if (num2 == -1)
				{
					OnScreenLog.AddLogItem("Failed to find block index", 5f, true);
					num2 = 0;
				}
			}
			else
			{
				OnScreenLog.AddLogItem("Failed to find the target block. Using index 0 and rotation 0, 0, 0", 5f, true);
			}
			List<Tile> list2 = new List<Tile>();
			list2.Add(new Tile(new GAF("Meta.Then", new object[0])));
			list2.Add(new Tile(new GAF(text3, new object[]
			{
				num2,
				zero,
				transform.position,
				raycastHit2.point
			})));
			List<List<Tile>> tiles2 = Blocksworld.selectedBlock.tiles;
			tiles2.Insert(tiles2.Count - 1, list2);
			Blocksworld.ShowSelectedBlockPanel();
			OnScreenLog.AddLogItem("Added " + text3 + " tile", 5f, true);
			History.AddStateIfNecessary();
		}
		bool flag3 = MappedInput.InputDown(MappableInput.TUTORIAL_ADD_HINT);
		bool flag4 = MappedInput.InputDown(MappableInput.TUTORIAL_REMOVE_HINT);
		RaycastHit raycastHit3;
		if ((flag3 || flag4) && Physics.Raycast(Blocksworld.mainCamera.ScreenPointToRay(Blocksworld.mouse * NormalizedScreen.scale), out raycastHit3))
		{
			Vector3 vector = Util.Round2(raycastHit3.point);
			Vector3 vector2 = Util.Round(raycastHit3.normal);
			Vector3 eulerAngles = transform.eulerAngles;
			float magnitude = (vector - Blocksworld.cameraTransform.position).magnitude;
			List<Tile> list3 = new List<Tile>();
			list3.Add(new Tile(new GAF("Meta.Then", new object[0])));
			if (flag3)
			{
				list3.Add(new Tile(new GAF("Block.TutorialCreateBlockHint", new object[]
				{
					vector,
					vector2,
					eulerAngles,
					magnitude,
					0,
					0,
					0,
					Vector3.zero
				})));
				BWLog.Info(string.Concat(new object[]
				{
					"Added hint pos ",
					vector,
					", normal ",
					vector2,
					", cam ",
					eulerAngles,
					", dist ",
					magnitude,
					" to block ",
					raycastHit3.collider.gameObject.name
				}));
			}
			else
			{
				Block block3 = BWSceneManager.FindBlock(raycastHit3.collider.gameObject, true);
				if (block3 != null)
				{
					int num3 = BWSceneManager.IndexOfBlock(block3);
					if (num3 == -1)
					{
						OnScreenLog.AddLogItem("Failed to find block index", 5f, true);
						num3 = 0;
					}
					list3.Add(new Tile(new GAF("Block.TutorialRemoveBlockHint", new object[]
					{
						num3,
						transform.position,
						raycastHit3.point
					})));
					BWLog.Info(string.Concat(new object[]
					{
						"Added remove block hint tile for the block ",
						block3.go.name,
						" with index ",
						num3
					}));
				}
				else
				{
					OnScreenLog.AddLogItem("Failed to find the target block. Using index 0 and rotation 0, 0, 0", 5f, true);
				}
			}
			Block selectedScriptBlock = Blocksworld.GetSelectedScriptBlock();
			selectedScriptBlock.tiles.Insert(1, list3);
			Blocksworld.ShowSelectedBlockPanel();
			History.AddStateIfNecessary();
		}
	}

	// Token: 0x06001946 RID: 6470 RVA: 0x000B593C File Offset: 0x000B3D3C
	private void ToggleVolumeBlockVisibility()
	{
		foreach (Block block in BWSceneManager.AllBlocks())
		{
			if (block is BlockPosition)
			{
				BlockPosition blockPosition = (BlockPosition)block;
				if (blockPosition.IsHiddenAndTrigger())
				{
					blockPosition.ShowAndRemoveIsTrigger();
					blockPosition.go.GetComponent<Collider>().enabled = true;
				}
				else
				{
					blockPosition.HideAndMakeTrigger();
					blockPosition.go.GetComponent<Collider>().enabled = false;
				}
			}
		}
	}

	// Token: 0x06001947 RID: 6471 RVA: 0x000B59E0 File Offset: 0x000B3DE0
	private void ToggleSkyAndFogVisibility()
	{
		if (Blocksworld.worldSky != null)
		{
			Renderer component = Blocksworld.worldSky.go.GetComponent<Renderer>();
			if (component.enabled)
			{
				this.SetFog(2500f, 3000f);
				component.enabled = false;
			}
			else
			{
				component.enabled = true;
				this.storeFogDistances = this.GetDefaultFog();
				this.SetFog(this.storeFogDistances.x, this.storeFogDistances.y);
			}
		}
	}

	// Token: 0x06001948 RID: 6472 RVA: 0x000B5A60 File Offset: 0x000B3E60
	private Vector2 GetDefaultFog()
	{
		Vector2 result = new Vector2(40f, 100f);
		if (Blocksworld.worldSky != null)
		{
			GAF simpleInitGAF = Blocksworld.worldSky.GetSimpleInitGAF("Block.SetFog");
			if (simpleInitGAF != null)
			{
				object[] args = simpleInitGAF.Args;
				float x = float.Parse((string)args[0]);
				float y = float.Parse((string)args[1]);
				result = new Vector2(x, y);
			}
		}
		return result;
	}

	// Token: 0x06001949 RID: 6473 RVA: 0x000B5ACD File Offset: 0x000B3ECD
	private void UnityStandaloneUpdate()
	{
		if (Blocksworld.CurrentState == State.Build)
		{
			this.StandaloneBuildModeUpdate();
		}
		else if (Blocksworld.CurrentState == State.FrameCapture)
		{
			this.StandaloneFrameCaptureUpdate();
		}
		else if (Blocksworld.CurrentState == State.Play)
		{
			this.StandalonePlayModeUpdate();
		}
	}

	// Token: 0x0600194A RID: 6474 RVA: 0x000B5B0C File Offset: 0x000B3F0C
	private void StandaloneBuildModeUpdate()
	{
		if (MappedInput.InputDown(MappableInput.SAVE) && !Input.GetMouseButton(1))
		{
			WorldSession.Save();
		}
		this.UpdateCopyPaste();
		if (MappedInput.InputDown(MappableInput.CYCLE_BLOCK_PREV))
		{
			this.CycleSelectedScriptBlock(true);
		}
		if (MappedInput.InputDown(MappableInput.CYCLE_BLOCK_NEXT))
		{
			this.CycleSelectedScriptBlock(false);
		}
		if (MappedInput.InputDown(MappableInput.DELETE_BLOCK))
		{
			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
			{
				Blocksworld.RemoveScriptsFromSelection();
			}
			else
			{
				List<Block> list = new List<Block>();
				if (Blocksworld.selectedBlock != null)
				{
					list.Add(Blocksworld.selectedBlock);
				}
				else if (Blocksworld.selectedBunch != null)
				{
					list.AddRange(Blocksworld.selectedBunch.blocks);
				}
				Blocksworld.DeselectBlock(false, true);
				foreach (Block block in list)
				{
					if (block == Blocksworld.worldOceanBlock)
					{
						Blocksworld.worldOceanBlock = null;
						Blocksworld.worldOcean = null;
					}
					Blocksworld.DestroyBlock(block);
				}
			}
			Sound.PlayOneShotSound("Destroy", 1f);
			Scarcity.UpdateInventory(true, null);
			History.AddStateIfNecessary();
		}
		if (MappedInput.InputDown(MappableInput.UNDO))
		{
			if (Input.GetKey(KeyCode.LeftShift))
			{
				this.ButtonRedoTapped();
			}
			else
			{
				this.ButtonUndoTapped();
			}
		}
		if (MappedInput.InputDown(MappableInput.REDO))
		{
			this.ButtonRedoTapped();
		}
		bool flag = false;
		if (MappedInput.InputDown(MappableInput.TOGGLE_ORBIT_CAM))
		{
			this.cameraSelectionOrbitMode = !this.cameraSelectionOrbitMode;
			flag = this.cameraSelectionOrbitMode;
			Sound.PlayOneShotSound("Button Generic", 1f);
		}
		if ((MappedInput.InputDown(MappableInput.FOCUS_CAMERA) || flag) && (Blocksworld.selectedBlock != null || Blocksworld.selectedBunch != null))
		{
			Vector3 pos = Vector3.zero;
			Vector3 scale = Vector3.one;
			if (Blocksworld.selectedBunch != null)
			{
				pos = Blocksworld.selectedBunch.GetPosition();
				scale = Blocksworld.selectedBunch.GetScale();
			}
			else
			{
				pos = Blocksworld.selectedBlock.GetPosition();
				scale = Blocksworld.selectedBlock.Scale();
			}
			Blocksworld.GoToCameraFrameFor(pos, scale);
		}
		bool flag2 = MappedInput.InputDown(MappableInput.YANK_COLOR_AND_TEXTURE);
		bool flag3 = MappedInput.InputDown(MappableInput.APPLY_COLOR);
		bool flag4 = MappedInput.InputDown(MappableInput.APPLY_TEXTURE);
		if ((flag2 || flag3 || flag4) && Blocksworld.mouseBlock != null)
		{
			Ray ray = Blocksworld.mainCamera.ScreenPointToRay(this.mousePositionLast * NormalizedScreen.scale);
			Vector3 vector = default(Vector3);
			int meshIndexForRay = Blocksworld.mouseBlock.GetMeshIndexForRay(ray, true, out vector, out vector);
			if (flag2)
			{
				Blocksworld.clipboard.SetPaintColor(Blocksworld.mouseBlock.GetPaint(meshIndexForRay), true);
				Blocksworld.clipboard.SetTexture(Blocksworld.mouseBlock.GetTexture(meshIndexForRay), true);
			}
			else if (flag3)
			{
				Blocksworld.mouseBlock.PaintTo((string)Blocksworld.clipboard.GetLastPaintedColorGAF().Args[0], true, meshIndexForRay);
				History.AddStateIfNecessary();
			}
			else if (flag4)
			{
				Blocksworld.mouseBlock.TextureTo((string)Blocksworld.clipboard.GetLastTextureGAF().Args[0], Blocksworld.mouseBlockNormal, true, meshIndexForRay, false);
				Blocksworld.mouseBlock.ScaleTo(Blocksworld.mouseBlockLast.Scale(), true, true);
				History.AddStateIfNecessary();
			}
		}
		if (MappedInput.InputDown(MappableInput.DESELECT_BLOCK))
		{
			Blocksworld.DeselectBlock(false, true);
		}
		Vector3 v = Input.mousePosition / NormalizedScreen.scale;
		bool flag5 = v.x >= 0f && v.x <= (float)NormalizedScreen.width && v.y >= 0f && v.y <= (float)NormalizedScreen.height;
		bool flag6 = Blocksworld.CurrentState == State.Build && Blocksworld.buildPanel.Hit(v);
		float num = -50f * Input.GetAxis("Mouse ScrollWheel");
		num = Mathf.Clamp(num, -100f, 100f);
		float num2 = Mathf.Abs(num);
		if (flag6 && num2 > 0.1f)
		{
			Blocksworld.buildPanel.Move(new Vector3(0f, num, 0f));
		}
		if (!flag6 && flag5)
		{
			float num3 = Mathf.Abs(num);
			if (num3 > 0.1f && !Blocksworld.lockInput)
			{
				Blocksworld.blocksworldCamera.ZoomBy(-num * 10f);
			}
			if (this.cameraSelectionOrbitMode && Input.GetMouseButton(1) && (Blocksworld.selectedBlock != null || Blocksworld.selectedBunch != null))
			{
				this.UpdateWASDEQOrbitMouseCameraMovement();
			}
			else
			{
				this.UpdateWASDEQMouseCameraMovement();
			}
		}
	}

	// Token: 0x0600194B RID: 6475 RVA: 0x000B5FD0 File Offset: 0x000B43D0
	private void StandaloneFrameCaptureUpdate()
	{
		Vector3 vector = Input.mousePosition / NormalizedScreen.scale;
		bool flag = vector.x >= 0f && vector.x <= (float)NormalizedScreen.width && vector.y >= 0f && vector.y <= (float)NormalizedScreen.height;
		float num = -50f * Input.GetAxis("Mouse ScrollWheel");
		num = Mathf.Clamp(num, -100f, 100f);
		float num2 = Mathf.Abs(num);
		if (flag)
		{
			float num3 = Mathf.Abs(num);
			if (num3 > 0.1f && !Blocksworld.lockInput)
			{
				Blocksworld.blocksworldCamera.ZoomBy(-num * 10f);
			}
			this.UpdateWASDEQMouseCameraMovement();
		}
	}

	// Token: 0x0600194C RID: 6476 RVA: 0x000B609C File Offset: 0x000B449C
	private void StandalonePlayModeUpdate()
	{
		if (Input.GetMouseButton(1))
		{
			Blocksworld.blocksworldCamera.OrbitBy(-this.mousePositionDelta);
		}
		float num = -50f * Input.GetAxis("Mouse ScrollWheel");
		num = Mathf.Clamp(num, -100f, 100f);
		float num2 = Mathf.Abs(num);
		if (num2 > 0.1f && !Blocksworld.lockInput)
		{
			Blocksworld.blocksworldCamera.ZoomBy(-num * 10f);
		}
	}

	// Token: 0x0600194D RID: 6477 RVA: 0x000B6120 File Offset: 0x000B4520
	private void PrintMaterials()
	{
		Material[] array = UnityEngine.Object.FindObjectsOfType<Material>();
		BWLog.Info("Found " + array.Length + " materials:");
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		for (int i = 0; i < array.Length; i++)
		{
			if (dictionary.ContainsKey(array[i].name))
			{
				dictionary[array[i].name] = dictionary[array[i].name] + 1;
			}
			else
			{
				dictionary[array[i].name] = 1;
			}
			stringBuilder.Append(array[i].name + "\n");
		}
		foreach (KeyValuePair<string, int> keyValuePair in dictionary)
		{
			int num = keyValuePair.Value;
			int num2;
			if (this.materialUsage.TryGetValue(keyValuePair.Key, out num2))
			{
				num -= num2;
			}
			if (num > 0)
			{
				stringBuilder2.Append(string.Concat(new object[]
				{
					num,
					" new materials with name: ",
					keyValuePair.Key,
					"\n"
				}));
			}
			this.materialUsage[keyValuePair.Key] = keyValuePair.Value;
		}
		BWLog.Info(stringBuilder.ToString());
		BWLog.Info(stringBuilder2.ToString());
		BWLog.Info("Materials Cache size: " + Materials.materialCache.Count);
		BWLog.Info("Material paints cache size: " + Materials.materialCachePaint.Count);
		BWLog.Info("Material texture cache size: " + Materials.materialCacheTexture.Count);
	}

	// Token: 0x0600194E RID: 6478 RVA: 0x000B6314 File Offset: 0x000B4714
	private void Update()
	{
        if (!Blocksworld.IsStarted())
		{
			return;
		}
		if (Blocksworld.inBackground)
		{
			TileIconManager.Instance.Update();
			return;
		}
		TileIconManager.Instance.Update();
		if (WorldSession.current != null)
		{
			if (MappedInput.InputDown(MappableInput.PLAY))
			{
				if (Blocksworld.CurrentState == State.Play)
				{
					this.ButtonPauseTapped();
				}
				else
				{
					this.ButtonPlayTapped();
				}
			}
			if (MappedInput.InputDown(MappableInput.STOP) && Blocksworld.CurrentState == State.Play && WorldSession.isNormalBuildAndPlaySession())
			{
				this.ButtonStopTapped();
			}
			if (MappedInput.InputDown(MappableInput.RESTART_PLAY) && Blocksworld.CurrentState == State.Play)
			{
				this.ButtonRestartTapped();
			}
		}
		MappedInput.Update();
		Blocksworld.displayString = string.Empty;
		Blocksworld.cameraForward = Blocksworld.cameraTransform.forward;
		Blocksworld.cameraUp = Blocksworld.cameraTransform.up;
		Blocksworld.cameraRight = Blocksworld.cameraTransform.right;
		Blocksworld.cameraPosition = Blocksworld.cameraTransform.position;
		Blocksworld.cameraMoved = ((double)(Blocksworld.cameraPosition - Blocksworld.prevCamPos).sqrMagnitude > 9.9999997764825828E-05);
		if (Blocksworld.cameraMoved)
		{
			Blocksworld.frustumPlanes = GeometryUtility.CalculateFrustumPlanes(Blocksworld.mainCamera);
			Blocksworld.prevCamPos = Blocksworld.cameraPosition;
		}
		Sound.sfxEnabled = true;
		if (MappedInput.InputDown(MappableInput.SCREENSHOT))
		{
			this.ButtonCaptureTapped();
		}
		OnScreenLog.Update();
		if (Blocksworld.CurrentState != State.WaitForOption && Blocksworld.CurrentState != State.EditTile)
		{
			this.UnityStandaloneUpdate();
		}
		if (Blocksworld.consumeEvent)
		{
			Blocksworld.consumeEvent = false;
			return;
		}
		Blocksworld.deltaTime = Time.realtimeSinceStartup - Blocksworld.lastRealtimeSinceStartup;
		Blocksworld.lastRealtimeSinceStartup = Time.realtimeSinceStartup;
		Blocksworld.updateCounter++;
		Blocksworld.gameTime = (float)Blocksworld.updateCounter * Time.deltaTime;
		Blocksworld._stateTime += Time.deltaTime;
		Blocksworld.mouse = Input.mousePosition / Blocksworld.screenScale;
		Blocksworld.numTouches = 0;
		if (Input.touchCount == 0)
		{
			Blocksworld.touches[0] = Input.mousePosition / Blocksworld.screenScale;
			if (Input.GetMouseButton(0) && !Blocksworld.UI.IsBlocking(Blocksworld.touches[0]))
			{
				Blocksworld.numTouches++;
			}
		}
		else
		{
			for (int i = 0; i < Input.touchCount; i++)
			{
				UnityEngine.Touch touch = Input.GetTouch(i);
				Blocksworld.touches[Blocksworld.numTouches] = touch.position / Blocksworld.screenScale;
				if (!Blocksworld.UI.IsBlocking(Blocksworld.touches[Blocksworld.numTouches]))
				{
					Blocksworld.numTouches++;
				}
			}
		}
		if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
		{
			this.mousePositionLast = Input.mousePosition;
		}
		this.mousePositionDelta = Input.mousePosition - this.mousePositionLast;
		this.mousePositionLast = Input.mousePosition;
		if (Blocksworld.CurrentState != State.Play)
		{
			Blocksworld.mouseBlockLast = Blocksworld.mouseBlock;
			Blocksworld.mouseBlockNormalLast = Blocksworld.mouseBlockNormal;
			Blocksworld.mouseBlock = Blocksworld.BlockAtMouse(NormalizedInput.mousePosition, out Blocksworld.mouseBlockIndex);
		}
		if (Blocksworld.CurrentState != State.Menu && !Blocksworld.lockInput)
		{
			for (int j = 0; j < this.buildOnlyGestures.Length; j++)
			{
				this.buildOnlyGestures[j].IsEnabled = (Blocksworld.CurrentState == State.Build && !this.forcePlayMode);
			}
			bool isEnabled = Blocksworld.CurrentState == State.Play;
			this.pullObject.IsEnabled = isEnabled;
			this.tapControl.IsEnabled = isEnabled;
			Blocksworld.recognizer.Update();
		}
		if (Blocksworld.inBackground)
		{
			return;
		}
		Blocksworld.buildPanel.Update();
		Blocksworld.scriptPanel.Update();
		Blocksworld.blocksworldCamera.Update();
		if (Blocksworld.CurrentState != State.Play)
		{
			TBox.Update();
		}
		this.StatePlayUpdate();
		if (Input.GetMouseButtonDown(0))
		{
			Blocksworld.mousePositionFirst = NormalizedInput.mousePosition;
		}
		if (Blocksworld.CurrentState == State.FrameCapture && Blocksworld.timerStart >= 0f)
		{
			Blocksworld.timerStart += Time.deltaTime;
		}
		if (Blocksworld.CurrentState == State.Build && !Blocksworld.f3PressedInCurrentWorld)
		{
			WorldSession.FastSaveAutoUpdate();
		}
		Tutorial.Update();
		Util.Update();
		Blocksworld.UpdateDynamicalLights(true, false);
		if (Blocksworld.stopASAP)
		{
			this.Stop(false, true);
			Blocksworld.stopASAP = false;
		}
		Command.ExecuteCommands(Blocksworld.updateCommands);
		this.OnHudMesh();
		if (WorldSession.current != null)
		{
			WorldSession.current.UpdateLoop();
		}
	}

	// Token: 0x0600194F RID: 6479 RVA: 0x000B67B4 File Offset: 0x000B4BB4
	public static void SelectionLock(Block b)
	{
		Blocksworld.editorSelectionLocked.Add(b);
	}

	// Token: 0x06001950 RID: 6480 RVA: 0x000B67C2 File Offset: 0x000B4BC2
	public static bool IsSelectionLocked(Block b)
	{
		return Blocksworld.editorSelectionLocked.Contains(b);
	}

	// Token: 0x06001951 RID: 6481 RVA: 0x000B67D0 File Offset: 0x000B4BD0
	private static void UpdateModelUnlockInput()
	{
		if (Blocksworld.CurrentState != State.Build)
		{
			return;
		}
		if (MappedInput.InputDown(MappableInput.MODEL_UNLOCK) && Blocksworld.selectedBunch != null)
		{
			bool flag = Blocksworld.selectedBunch.blocks.Exists((Block b) => b.HasGroup("locked-model"));
			if (flag)
			{
				Blocksworld.selectedBunch.blocks.ForEach(delegate(Block b)
				{
					BlockGroup groupOfType = b.GetGroupOfType("locked-model");
					if (groupOfType is LockedModelBlockGroup)
					{
						b.RemoveGroup("locked-model");
						BlockGroups.RemoveGroup(groupOfType);
					}
				});
				Blocksworld.SelectBunch(Blocksworld.selectedBunch.blocks, false, true);
			}
			else
			{
				BlockGroups.AddGroup(Blocksworld.selectedBunch.blocks, "locked-model");
				Blocksworld.Select(Blocksworld.selectedBunch.blocks[0], false, true);
			}
		}
	}

	// Token: 0x06001952 RID: 6482 RVA: 0x000B68A0 File Offset: 0x000B4CA0
	private static void UpdateSelectionLockInput()
	{
		if (Blocksworld.CurrentState != State.Build)
		{
			return;
		}
		if (!MappedInput.InputPressed(MappableInput.SELECTION_LOCK))
		{
			Blocksworld.keyLReleased = true;
			Blocksworld.recentSelectionUnlockedBlock = null;
		}
		if (MappedInput.InputDown(MappableInput.SELECTION_LOCK))
		{
			Blocksworld.keyLReleased = false;
			if (Blocksworld.selectedBunch != null)
			{
				OnScreenLog.AddLogItem("Selection-locked an entire model", 5f, false);
				foreach (Block b in Blocksworld.selectedBunch.blocks)
				{
					Blocksworld.SelectionLock(b);
				}
				Blocksworld.Select(null, false, true);
			}
			else if (Blocksworld.selectedBlock != null)
			{
				OnScreenLog.AddLogItem("Selection-locked a block of type '" + Blocksworld.selectedBlock.BlockType() + "'", 5f, false);
				Blocksworld.SelectionLock(Blocksworld.selectedBlock);
				Blocksworld.Select(null, false, true);
			}
		}
	}

	// Token: 0x06001953 RID: 6483 RVA: 0x000B699C File Offset: 0x000B4D9C
	public static void UpdateDynamicalLights(bool updateFog = true, bool forceUpdate = false)
	{
		Color color = Blocksworld.lightColor;
		Vector4 b = Blocksworld.dynamicLightColor;
		float num = Blocksworld.fogMultiplier;
		float num2 = Blocksworld.weather.GetFogMultiplier();
		Color color2 = Color.white;
		float num3 = 1f;
		for (int i = 0; i < Blocksworld.dynamicalLightChangers.Count; i++)
		{
			ILightChanger lightChanger = Blocksworld.dynamicalLightChangers[i];
			Color dynamicalLightTint = lightChanger.GetDynamicalLightTint();
			color *= dynamicalLightTint;
			num2 *= lightChanger.GetFogMultiplier();
			color2 *= lightChanger.GetFogColorOverride();
			num3 *= lightChanger.GetLightIntensityMultiplier();
		}
		Vector4 b2 = Blocksworld.fogColor;
		Vector4 a = (!(color2 == Color.white)) ? color2 : Blocksworld.fogColor;
		Vector4 a2 = color;
		if (Mathf.Abs(num3 - Blocksworld.dynamicLightIntensityMultiplier) > 0.01f || (a2 - b).sqrMagnitude > 0.001f || (a - b2).sqrMagnitude > 0.001f || Mathf.Abs(num - num2) > 0.01f || forceUpdate)
		{
			Blocksworld.dynamicLightColor = color;
			Light component = Blocksworld.directionalLight.GetComponent<Light>();
			component.color = color;
			component.intensity = num3;
			Blocksworld.dynamicLightIntensityMultiplier = num3;
			if (updateFog && Blocksworld.worldSky != null)
			{
				if (color2 != Color.white)
				{
					Blocksworld.UpdateFogColor(color2 * color);
				}
				else
				{
					Blocksworld.UpdateFogColor(BlockSky.GetFogColor());
				}
				if (Mathf.Abs(num2 - num) > 0.001f)
				{
					Blocksworld.bw.SetFogMultiplier(num2);
				}
			}
		}
	}

	// Token: 0x06001954 RID: 6484 RVA: 0x000B6B60 File Offset: 0x000B4F60
	private void FixedUpdate()
	{
		if (!Blocksworld.IsStarted())
		{
			return;
		}
		if (Blocksworld.inBackground)
		{
			return;
		}
		Blocksworld.fixedDeltaTime = Time.fixedDeltaTime;
		if (Blocksworld.CurrentState == State.Play)
		{
			this.StatePlayFixedUpdate();
			bool showControls = MappedInput.InputDown(MappableInput.SHOW_CONTROLS) || Blocksworld.TimeInCurrentState() < 1f;
			Blocksworld.UI.Controls.UpdateAll(showControls);
		}
		foreach (KeyValuePair<BlockAnimatedCharacter, CharacterStateHandler> keyValuePair in BlockAnimatedCharacter.stateControllers)
		{
			keyValuePair.Value.Update();
		}
		if (this.pullObject != null && this.pullObject.IsEnabled)
		{
			this.pullObject.FixedUpdate();
		}
		if (Blocksworld.rewardVisualizationGafs != null && !Blocksworld.waitForSetPurchase)
		{
			if (Blocksworld.rewardVisualizationIndex >= Blocksworld.rewardVisualizationGafs.Count)
			{
				Blocksworld.rewardVisualizationGafs = null;
				Blocksworld.winIsWaiting = false;
				Blocksworld.hasWon = true;
				WorldSession.current.OnRewardVisualizationComplete();
			}
			else
			{
				Blocksworld.rewardExecutionInfo.timer += Blocksworld.fixedDeltaTime;
				GAF gaf = Blocksworld.rewardVisualizationGafs[Blocksworld.rewardVisualizationIndex];
				List<Block> list = BWSceneManager.AllBlocks();
				if (gaf == null || list.Count == 0 || gaf.RunAction(list[0], Blocksworld.rewardExecutionInfo) == TileResultCode.True)
				{
					Blocksworld.rewardVisualizationIndex++;
					Blocksworld.rewardExecutionInfo.timer = 0f;
				}
			}
		}
		else if (Blocksworld.CurrentState != State.WaitForOption && Blocksworld.winIsWaiting && !Blocksworld.waitForSetPurchase)
		{
			Blocksworld.hasWon = true;
			Blocksworld.winIsWaiting = false;
		}
		Blocksworld.blocksworldCamera.FixedUpdate();
		Scarcity.StepInventoryScales();
		Blocksworld.UI.UpdateSpeechBubbles();
		Blocksworld.UI.UpdateTextWindows();
		Blocksworld.UI.Controls.HandleInputControlVisibility(Blocksworld.CurrentState);
		Command.ExecuteCommands(Blocksworld.fixedUpdateCommands);
	}

	// Token: 0x06001955 RID: 6485 RVA: 0x000B6D70 File Offset: 0x000B5170
	public void StatePlayUpdate()
	{
		if (Blocksworld.CurrentState == State.Play)
		{
			BWSceneManager.ExecutePlayBlocksUpdate();
		}
		else
		{
			BWSceneManager.ExecuteAllBlocksUpdate();
		}
		Blocksworld.weather.Update();
		Blocksworld.leaderboardData.UpdateGUI();
		VisualEffect.UpdateVfxs();
	}

	// Token: 0x06001956 RID: 6486 RVA: 0x000B6DA8 File Offset: 0x000B51A8
	private void StatePlayFixedUpdate()
	{
		BWSceneManager.ModifyPlayAndScriptBlocks();
		if (Blocksworld.isFirstFrame)
		{
			BWSceneManager.RunFirstFrameActions();
			Blocksworld.isFirstFrame = false;
		}
		BWSceneManager.RunConditions();
		this.ResetState();
		BWSceneManager.RunActions();
		BWSceneManager.RunFixedUpdate();
		VisualEffect.FixedUpdateVfxs();
		TreasureHandler.FixedUpdate();
		CollisionManager.FixedUpdate();
		Block.UpdateOverridenMasses();
		BlockAnimatedCharacter.PlayQueuedHitReacts();
		Blocksworld.blocksworldCamera.FinalUpdateFirstPersonFollow();
		Block.goTouchStarted = false;
		for (int i = 0; i < Blocksworld.chunks.Count; i++)
		{
			Blocksworld.chunks[i].ApplyCenterOfMassChanges();
		}
		Blocksworld.playFixedUpdateCounter++;
	}

	// Token: 0x06001957 RID: 6487 RVA: 0x000B6E44 File Offset: 0x000B5244
	private void ResetState()
	{
		Blocksworld.gameStart = false;
		Blocksworld.blocksworldCamera.Reset();
		CollisionManager.ResetState(false);
		if (!Input.GetMouseButton(0))
		{
			Block.goTouchStarted = false;
			Block.goTouched = null;
		}
		for (int i = 0; i < 26; i++)
		{
			Blocksworld.sending[i] = false;
			Blocksworld.sendingValues[i] = 0f;
		}
		Blocksworld.sendingCustom.Clear();
		BWSceneManager.ResetFrame();
		BlockAbstractBow.ClearHits();
		BlockAbstractLaser.ClearHits();
		BlockGravityGun.ClearHits();
		BlockAnimatedCharacter.ClearAttackFlags();
		TagManager.ClearRegisteredBlocks(false);
		BlockAccelerations.ResetFrame();
		TreasureHandler.ResetState();
		Sound.ResetState();
		Command.ExecuteCommands(Blocksworld.resetStateCommands);
	}

	// Token: 0x06001958 RID: 6488 RVA: 0x000B6EE8 File Offset: 0x000B52E8
	public void CycleSelectedScriptBlock(bool back)
	{
		List<Block> list = new List<Block>();
		BWSceneManager.ResetPlayPredicates();
		List<Block> list2 = BWSceneManager.AllBlocks();
		for (int i = 0; i < list2.Count; i++)
		{
			Block block = list2[i];
			block.CheckContainsPlayModeTiles();
			if (block.containsPlayModeTiles)
			{
				list.Add(block);
			}
		}
		if (list.Count == 0)
		{
			return;
		}
		if (Blocksworld.selectedBlock == null || !list.Contains(Blocksworld.selectedBlock))
		{
			Blocksworld.SelectBlock(list[0], false, true);
			Blocksworld.blocksworldCamera.Focus();
			return;
		}
		int num = list.IndexOf(Blocksworld.selectedBlock);
		if (back)
		{
			num--;
			if (num < 0)
			{
				num = list.Count + num;
			}
		}
		else
		{
			num++;
			if (num >= list.Count)
			{
				num -= list.Count;
			}
		}
		Blocksworld.SelectBlock(list[num], false, true);
		Blocksworld.blocksworldCamera.Focus();
	}

	// Token: 0x06001959 RID: 6489 RVA: 0x000B6FE4 File Offset: 0x000B53E4
	public Block AddNewBlock(Tile type, bool addToBlocks = true, List<Tile> initTiles = null, bool defaultColors = true)
	{
		List<List<Tile>> list = new List<List<Tile>>();
		list.Add(new List<Tile>());
		list[0].Add(new Tile(new GAF("Meta.Stop", new object[0])));
		list[0].Add(type);
		list[0].Add(new Tile(new GAF("Block.MoveTo", new object[]
		{
			Util.nullVector3
		})));
		list[0].Add(new Tile(new GAF("Block.RotateTo", new object[]
		{
			Vector3.zero
		})));
		list[0].Add(new Tile(new GAF("Block.PaintTo", new object[]
		{
			"Grey"
		})));
		if (((string)type.gaf.Args[0]).StartsWith("Decal "))
		{
			list[0].Add(new Tile(new GAF("Block.TextureTo", new object[]
			{
				((string)type.gaf.Args[0]).Substring("Decal ".Length),
				Vector3.zero
			})));
		}
		else
		{
			list[0].Add(new Tile(new GAF("Block.TextureTo", new object[]
			{
				"Plain",
				Vector3.zero
			})));
		}
		list[0].Add(new Tile(new GAF("Block.ScaleTo", new object[]
		{
			Vector3.one
		})));
		if (initTiles != null)
		{
			list[0].AddRange(initTiles);
		}
		list.Add(new List<Tile>());
		list[1].Add(new Tile(new GAF("Meta.Then", new object[0])));
		bool defaultTiles = Tutorial.state == TutorialState.None || Tutorial.includeDefaultBlockTiles;
		Block block = Block.NewBlock(list, defaultColors, defaultTiles);
		block.RotateTo(Quaternion.Euler(0f, 180f + 90f * Mathf.Floor(Blocksworld.cameraTransform.eulerAngles.y / 90f), 0f) * block.goT.rotation);
		if (addToBlocks)
		{
			BWSceneManager.AddBlock(block);
		}
		Blocksworld.scriptPanel.Show(false);
		return block;
	}

	// Token: 0x0600195A RID: 6490 RVA: 0x000B7254 File Offset: 0x000B5654
	private Vector3 WorldPlanePos(Vector3 normal, Vector3 point, Vector3 screenPos)
	{
		Plane plane = new Plane(normal, point);
		Ray ray = Blocksworld.mainCamera.ScreenPointToRay(screenPos);
		float distance;
		plane.Raycast(ray, out distance);
		return ray.GetPoint(distance);
	}

	// Token: 0x0600195B RID: 6491 RVA: 0x000B728C File Offset: 0x000B568C
	public static void DisableRenderer(GameObject parent)
	{
		if (parent.GetComponent<Renderer>() != null)
		{
			parent.GetComponent<Renderer>().enabled = false;
		}
		IEnumerator enumerator = parent.transform.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Transform transform = (Transform)obj;
				transform.GetComponent<Renderer>().enabled = false;
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

	// Token: 0x0600195C RID: 6492 RVA: 0x000B7314 File Offset: 0x000B5714
	public static void DestroyBunch(Bunch bunch)
	{
		if (bunch != null)
		{
			foreach (Block block in new List<Block>(bunch.blocks))
			{
				Blocksworld.DestroyBlock(block);
			}
		}
	}

	// Token: 0x0600195D RID: 6493 RVA: 0x000B737C File Offset: 0x000B577C
	public static void DestroyBlock(Block block)
	{
		if (Blocksworld.selectedBunch != null)
		{
			Blocksworld.selectedBunch.Remove(block);
			if (Blocksworld.selectedBunch.blocks.Count == 0)
			{
				Blocksworld.Deselect(false, true);
			}
		}
		BWSceneManager.RemoveBlock(block);
		block.Destroy();
		BlockGrouped blockGrouped = block as BlockGrouped;
		if (blockGrouped != null && blockGrouped.group != null)
		{
			Block[] blocks = blockGrouped.group.GetBlocks();
			foreach (Block block2 in blocks)
			{
				if (block2.go != null && block2 != block)
				{
					Blocksworld.DestroyBlock(block2);
				}
			}
		}
		Block.ClearConnectedCache();
	}

	// Token: 0x0600195E RID: 6494 RVA: 0x000B742C File Offset: 0x000B582C
	public static Block BlockAtMouse(Vector3 mouse, out int meshIndex)
	{
		meshIndex = -1;
		if (!Blocksworld.UI.SidePanel.Hit(mouse) && !Blocksworld.scriptPanel.Hit(mouse))
		{
			Ray ray = Blocksworld.mainCamera.ScreenPointToRay(mouse * NormalizedScreen.scale);
			RaycastHit[] array = Physics.RaycastAll(ray);
			if (array.Length > 0)
			{
				float num = float.MaxValue;
				Block result = null;
				for (int i = 0; i < 2; i++)
				{
					foreach (RaycastHit raycastHit in array)
					{
						float num2 = raycastHit.distance;
						if (num2 + (float)i <= num)
						{
							Block block = BWSceneManager.FindBlock(raycastHit.collider.gameObject, i == 1);
							if (block != null)
							{
								if (block is BlockVolumeBlock)
								{
									num2 += 0.01f;
									if (num2 + (float)i > num)
									{
										goto IL_16B;
									}
								}
								if (!CharacterEditor.Instance.InEditMode() || CharacterEditor.Instance.IsCharacterBlock(block) || CharacterEditor.Instance.IsCharacterAttachment(block))
								{
									if (block != null && (!Tutorial.FilterOutMouseBlock() || Tutorial.BlockCanBeMouseBlock(block)))
									{
										num = num2;
										Blocksworld.mouseBlockNormal = Quaternion.Inverse(raycastHit.collider.gameObject.transform.rotation) * raycastHit.normal;
										Blocksworld.mouseBlockHitPosition = raycastHit.point;
										result = block;
									}
								}
							}
						}
						IL_16B:;
					}
				}
				return result;
			}
		}
		return null;
	}

	// Token: 0x0600195F RID: 6495 RVA: 0x000B75C8 File Offset: 0x000B59C8
	private static Block LonelyBlock(Bunch bunch)
	{
		if (Blocksworld.selectedBunch.blocks.Count == 1)
		{
			return Blocksworld.selectedBunch.blocks[0];
		}
		Block block = null;
		foreach (Block block2 in Blocksworld.selectedBunch.blocks)
		{
			if (block != null)
			{
				return null;
			}
			block = block2;
		}
		return block;
	}

	// Token: 0x06001960 RID: 6496 RVA: 0x000B7660 File Offset: 0x000B5A60
	public static void Select(Block block, bool silent = false, bool updateTiles = true)
	{
		if (CharacterEditor.Instance.InEditMode())
		{
			return;
		}
		bool flag = Blocksworld.editorSelectionLocked.Contains(block);
		if (BW.isUnityEditor)
		{
			if (flag)
			{
				bool flag2 = MappedInput.InputPressed(MappableInput.SELECTION_LOCK);
				if (flag2)
				{
					Blocksworld.editorSelectionLocked.Remove(block);
					Blocksworld.recentSelectionUnlockedBlock = block;
					OnScreenLog.AddLogItem("Selection-unlocked a block of type '" + block.BlockType() + "'", 5f, false);
					if (!silent)
					{
						Sound.PlayOneShotSound("Move", 1f);
					}
				}
				else
				{
					OnScreenLog.AddLogItem("Trying to select a selection-locked block. Hold the 'L' key and click on the block to unlock it.", 5f, false);
				}
				return;
			}
			if (!Blocksworld.keyLReleased && Blocksworld.recentSelectionUnlockedBlock != null && Blocksworld.recentSelectionUnlockedBlock == block)
			{
				OnScreenLog.AddLogItem("Selection-unlocked the entire model", 5f, false);
				List<Block> list = ConnectednessGraph.ConnectedComponent(block, 3, null, true);
				foreach (Block item in list)
				{
					Blocksworld.editorSelectionLocked.Remove(item);
				}
				if (!silent)
				{
					Sound.PlayOneShotSound("Move", 1f);
				}
				Blocksworld.recentSelectionUnlockedBlock = null;
				Blocksworld.keyLReleased = true;
				return;
			}
		}
		if (flag)
		{
			return;
		}
		if (block != null && block.go == null)
		{
			BWLog.Info("Tried to select an already destroyed block");
			return;
		}
		List<Block> list2 = null;
		bool flag3 = block != null && block.HasGroup("locked-model");
		BlockGrouped blockGrouped = block as BlockGrouped;
		if (flag3)
		{
			list2 = block.GetGroupOfType("locked-model").GetBlockList();
		}
		else if (blockGrouped != null)
		{
			list2 = blockGrouped.group.GetBlockList();
		}
		else if (block is BlockCharacter && !block.IsProfileCharacter())
		{
			list2 = Blocksworld.GetBlocksterPlusGearList((BlockCharacter)block);
		}
		else if (block != null)
		{
			BlockCharacter blockCharacter = block.connections.Find((Block b) => b is BlockCharacter && !b.IsProfileCharacter()) as BlockCharacter;
			if (blockCharacter != null && Blocksworld.IsBlocksterGear(block))
			{
				list2 = Blocksworld.GetBlocksterPlusGearList(blockCharacter);
			}
		}
		if (block != null)
		{
			if (list2 != null && list2.Count > 1)
			{
				if (Blocksworld.selectedBunch != null)
				{
					bool flag4 = Blocksworld.selectedBunch.blocks.Contains(block);
					HashSet<Block> bunchSet = Blocksworld.GetBunchSet(list2);
					if (flag4 && bunchSet.Count > Blocksworld.selectedBunch.blocks.Count)
					{
						Blocksworld.DeselectBunch();
						Blocksworld.SelectBunch(new List<Block>(bunchSet), silent, updateTiles);
					}
					else
					{
						Blocksworld.DeselectBunch();
						if (flag3)
						{
							Blocksworld.SelectBunch(list2, silent, updateTiles);
						}
						else
						{
							Blocksworld.SelectBlock(block, silent, updateTiles);
						}
					}
				}
				else if (Blocksworld.selectedBlock != null)
				{
					if (block == Blocksworld.selectedBlock && (Tutorial.state == TutorialState.None || Tutorial.state == TutorialState.SelectBunch))
					{
						Blocksworld.DeselectBlock(silent, false);
						Blocksworld.SelectBunch(list2, silent, updateTiles);
					}
					else
					{
						Blocksworld.DeselectBlock(silent, false);
						if (flag3)
						{
							Blocksworld.SelectBunch(list2, silent, updateTiles);
						}
						else
						{
							Blocksworld.SelectBlock(block, silent, updateTiles);
						}
					}
				}
				else if (flag3)
				{
					Blocksworld.SelectBunch(list2, silent, updateTiles);
				}
				else
				{
					Blocksworld.SelectBlock(block, silent, updateTiles);
				}
			}
			else if (block != Blocksworld.selectedBlock)
			{
				if (Blocksworld.selectedBunch != null)
				{
					Blocksworld.DeselectBunch();
				}
				if (Blocksworld.selectedBlock != null)
				{
					Blocksworld.DeselectBlock(silent, false);
				}
				Blocksworld.SelectBlock(block, silent, updateTiles);
			}
			else if (block == Blocksworld.selectedBlock && Blocksworld.selectedBunch == null && (Tutorial.state == TutorialState.None || Tutorial.state == TutorialState.SelectBunch))
			{
				Blocksworld.DeselectBlock(silent, false);
				Blocksworld.SelectBunch(block, silent, updateTiles);
			}
			else if (block == Blocksworld.selectedBlock && Blocksworld.selectedBunch != null)
			{
				Blocksworld.DeselectBunch();
				Blocksworld.SelectBlock(block, silent, updateTiles);
			}
		}
		else
		{
			Blocksworld.DeselectBunch();
			Blocksworld.DeselectBlock(silent, updateTiles);
		}
	}

	// Token: 0x06001961 RID: 6497 RVA: 0x000B7A80 File Offset: 0x000B5E80
	private static HashSet<Block> GetBunchSet(List<Block> list)
	{
		HashSet<Block> hashSet = new HashSet<Block>();
		foreach (Block block in list)
		{
			hashSet.UnionWith(Blocksworld.GetBunchBlocks(block));
		}
		return hashSet;
	}

	// Token: 0x06001962 RID: 6498 RVA: 0x000B7AE4 File Offset: 0x000B5EE4
	public static bool IsBlocksterGear(Block cb)
	{
		int tabIndexForGaf = PanelSlots.GetTabIndexForGaf(new GAF(Block.predicateCreate, new object[]
		{
			cb.BlockType()
		}));
		return tabIndexForGaf == 6;
	}

	// Token: 0x06001963 RID: 6499 RVA: 0x000B7B14 File Offset: 0x000B5F14
	public static bool IsBlocksterOverlappingHeadGear(Block cb)
	{
		bool result = false;
		BlockMetaData blockMetaData = cb.GetBlockMetaData();
		if (blockMetaData != null)
		{
			for (int i = 0; i < blockMetaData.canOccupySameGrid.Length; i++)
			{
				if (blockMetaData.canOccupySameGrid[i] == "Character")
				{
					result = true;
					break;
				}
			}
			for (int j = 0; j < blockMetaData.shapeCategories.Length; j++)
			{
				if (blockMetaData.shapeCategories[j] == "Chest Gear" || blockMetaData.shapeCategories[j] == "Necklace" || blockMetaData.shapeCategories[j] == "Torso" || blockMetaData.shapeCategories[j] == "Belt" || blockMetaData.shapeCategories[j] == "Beard" || blockMetaData.shapeCategories[j] == "Ponytail")
				{
					result = false;
					break;
				}
			}
		}
		return result;
	}

	// Token: 0x06001964 RID: 6500 RVA: 0x000B7C18 File Offset: 0x000B6018
	public static List<Block> GetBlocksterPlusGearList(BlockCharacter b)
	{
		List<Block> list = new List<Block>();
		list.Add(b);
		foreach (Block block in b.connections)
		{
			if (Blocksworld.IsBlocksterGear(block))
			{
				list.Add(block);
			}
		}
		return list;
	}

	// Token: 0x06001965 RID: 6501 RVA: 0x000B7C90 File Offset: 0x000B6090
	public static bool SelectedBunchIsGroup()
	{
		if (Blocksworld.selectedBunch != null)
		{
			List<Block> list = new List<Block>();
			int num = -1;
			foreach (Block block in Blocksworld.selectedBunch.blocks)
			{
				BlockGrouped blockGrouped = block as BlockGrouped;
				if (blockGrouped == null)
				{
					return false;
				}
				int groupId = blockGrouped.group.groupId;
				if (num == -1)
				{
					num = groupId;
				}
				else if (groupId != num)
				{
					return false;
				}
				list.Add(block);
			}
			return list.Count == Blocksworld.selectedBunch.blocks.Count;
		}
		return false;
	}

	// Token: 0x06001966 RID: 6502 RVA: 0x000B7D68 File Offset: 0x000B6168
	public static List<Block> GetBunchBlocks(Block block)
	{
		List<Block> list = ConnectednessGraph.ConnectedComponent(block, 3, null, true);
		List<Block> list2 = new List<Block>();
		foreach (Block block2 in list)
		{
			if (!Blocksworld.IsSelectionLocked(block2))
			{
				list2.Add(block2);
			}
		}
		HashSet<Block> hashSet = new HashSet<Block>(list2);
		foreach (Block block3 in new List<Block>(list2))
		{
			BlockGrouped blockGrouped = block3 as BlockGrouped;
			if (blockGrouped != null)
			{
				Block[] blocks = blockGrouped.group.GetBlocks();
				foreach (Block block4 in blocks)
				{
					List<Block> list3 = ConnectednessGraph.ConnectedComponent(block4, 3, null, true);
					foreach (Block item in list3)
					{
						if (!hashSet.Contains(item))
						{
							hashSet.Add(item);
							list2.Add(item);
						}
					}
				}
			}
		}
		return list2;
	}

	// Token: 0x06001967 RID: 6503 RVA: 0x000B7EDC File Offset: 0x000B62DC
	public static void SelectBunch(Block block, bool silent = false, bool updateTiles = true)
	{
		List<Block> bunchBlocks = Blocksworld.GetBunchBlocks(block);
		if (bunchBlocks.Count <= 1)
		{
			if (!Blocksworld.IsSelectionLocked(block))
			{
				Blocksworld.SelectBlock(block, silent, true);
			}
			return;
		}
		Blocksworld.selectedBunch = new Bunch();
		for (int i = 0; i < bunchBlocks.Count; i++)
		{
			Block b = bunchBlocks[i];
			if (!Blocksworld.IsSelectionLocked(b))
			{
				Blocksworld.selectedBunch.Add(b);
			}
		}
		Blocksworld.SortBlocksAsInWorld(Blocksworld.selectedBunch.blocks);
		TBox.Attach(Blocksworld.selectedBunch, silent);
		TBox.Show(true);
		Blocksworld.blocksworldCamera.Follow(Blocksworld.selectedBunch);
		if (updateTiles)
		{
			Blocksworld.UpdateTiles();
			Scarcity.UpdateInventory(true, null);
		}
	}

	// Token: 0x06001968 RID: 6504 RVA: 0x000B7F90 File Offset: 0x000B6390
	private static void UpdateMouseBlock(Block block)
	{
		Blocksworld.mouseBlockLast = Blocksworld.mouseBlock;
		Blocksworld.mouseBlockNormalLast = Blocksworld.mouseBlockNormal;
		Blocksworld.mouseBlock = block;
	}

	// Token: 0x06001969 RID: 6505 RVA: 0x000B7FAC File Offset: 0x000B63AC
	public static void SelectBunch(List<Block> blocks, bool silent = false, bool updateTiles = true)
	{
		if (blocks.Count > 0)
		{
			if (blocks.Count == 1)
			{
				if (!Blocksworld.IsSelectionLocked(blocks[0]))
				{
					Blocksworld.SelectBlock(blocks[0], silent, true);
					Blocksworld.UpdateMouseBlock(blocks[0]);
				}
				return;
			}
			Blocksworld.selectedBunch = new Bunch();
			for (int i = 0; i < blocks.Count; i++)
			{
				Block b = blocks[i];
				if (!Blocksworld.IsSelectionLocked(b))
				{
					Blocksworld.selectedBunch.Add(b);
				}
			}
			Blocksworld.SortBlocksAsInWorld(Blocksworld.selectedBunch.blocks);
			TBox.Attach(Blocksworld.selectedBunch, silent);
			TBox.Show(true);
			Blocksworld.blocksworldCamera.Follow(Blocksworld.selectedBunch);
			if (Blocksworld.SelectedBunchIsGroup())
			{
				Blocksworld.ShowSelectedBlockPanel();
			}
			if (updateTiles)
			{
				Blocksworld.UpdateTiles();
				Scarcity.UpdateInventory(true, null);
			}
		}
	}

	// Token: 0x0600196A RID: 6506 RVA: 0x000B808C File Offset: 0x000B648C
	public static void SortBlocksAsInWorld(List<Block> toSort)
	{
		Dictionary<Block, int> blockIndices = new Dictionary<Block, int>();
		List<Block> list = BWSceneManager.AllBlocks();
		for (int i = 0; i < list.Count; i++)
		{
			blockIndices[list[i]] = i;
		}
		toSort.Sort(delegate(Block b1, Block b2)
		{
			int num = blockIndices[b1];
			int num2 = blockIndices[b2];
			return num - num2;
		});
	}

	// Token: 0x0600196B RID: 6507 RVA: 0x000B80EC File Offset: 0x000B64EC
	public static void DeselectBunch()
	{
		Blocksworld.blocksworldCamera.Unfollow();
		if (Blocksworld.selectedBunch != null && Blocksworld.scriptPanel.IsShowing())
		{
			Blocksworld.scriptPanel.Show(false);
		}
		Blocksworld.selectedBunch = null;
	}

	// Token: 0x0600196C RID: 6508 RVA: 0x000B8124 File Offset: 0x000B6524
	public static void SelectBlock(Block block, bool silent = false, bool updateTiles = true)
	{
		if (block != null && block.go == null)
		{
			BWLog.Info("Tried to select an already destroyed block");
			return;
		}
		Blocksworld.selectedBlock = block;
		TBox.Attach(Blocksworld.selectedBlock, silent);
		TBox.Show(true);
		Blocksworld.blocksworldCamera.Follow(block);
		if (updateTiles)
		{
			Blocksworld.UpdateTiles();
			Scarcity.UpdateInventory(true, null);
		}
		if (Blocksworld.locked.Contains(block))
		{
			Blocksworld.scriptPanel.Show(false);
		}
		else
		{
			Blocksworld.ShowSelectedBlockPanel();
		}
	}

	// Token: 0x0600196D RID: 6509 RVA: 0x000B81AC File Offset: 0x000B65AC
	public static void ScrollToFirstBlockSpecificTile(Block block)
	{
		bool flag = Tutorial.state == TutorialState.None || Tutorial.mode == TutorialMode.Puzzle;
		flag &= !Options.DisableAutoScrollToScriptTile;
		bool flag2 = Blocksworld.buildPanel.GetTabBar().SelectedTab == TabBarTabId.Actions;
		flag = (flag && flag2);
		if (flag)
		{
			BlockMetaData blockMetaData = block.GetBlockMetaData();
			if (blockMetaData != null && blockMetaData.scrollToScriptTileOnSelect)
			{
				HashSet<Predicate> hashSet = new HashSet<Predicate>(PredicateRegistry.ForBlock(block, false));
				if (hashSet.Count > 0)
				{
					Tile tile = Blocksworld.buildPanel.FindFirstTileWithPredicate(hashSet);
					if (tile != null)
					{
						Blocksworld.buildPanel.ScrollToVisible(tile, true, false, false);
					}
				}
			}
		}
	}

	// Token: 0x0600196E RID: 6510 RVA: 0x000B8254 File Offset: 0x000B6654
	public static void ShowSelectedBlockPanel()
	{
		Blocksworld.scriptPanel.ClearTiles();
		Block selectedScriptBlock = Blocksworld.GetSelectedScriptBlock();
		if (Blocksworld.selectedBlock != null && Blocksworld.selectedBlock.HasGroup("locked-model"))
		{
			Blocksworld.scriptPanel.Show(false);
			return;
		}
		Blocksworld.scriptPanel.SetTilesFromBlock(selectedScriptBlock);
		Blocksworld.scriptPanel.Show(true);
		Blocksworld.scriptPanel.UpdateGestureRecognizer(Blocksworld.recognizer);
		Blocksworld.scriptPanel.Position();
		Blocksworld.scriptPanel.Layout();
	}

	// Token: 0x0600196F RID: 6511 RVA: 0x000B82D4 File Offset: 0x000B66D4
	public static void DeselectBlock(bool silent = false, bool updateTiles = true)
	{
		TBox.Detach(silent);
		TBox.Show(false);
		Blocksworld.blocksworldCamera.Unfollow();
		if (Blocksworld.selectedBlock != null)
		{
			Blocksworld.scriptPanel.Show(false);
			Blocksworld.selectedBlock = null;
			if (updateTiles)
			{
				Blocksworld.UpdateTiles();
			}
		}
	}

	// Token: 0x06001970 RID: 6512 RVA: 0x000B8312 File Offset: 0x000B6712
	public static void Deselect(bool silent = false, bool updateTiles = true)
	{
		Blocksworld.Select(null, silent, updateTiles);
		Blocksworld.Select(null, silent, updateTiles);
	}

	// Token: 0x06001971 RID: 6513 RVA: 0x000B8324 File Offset: 0x000B6724
	public static HashSet<Predicate> GetTaggedPredicates()
	{
		if (Blocksworld.taggedPredicates == null)
		{
			Blocksworld.taggedPredicates = new HashSet<Predicate>(new Predicate[]
			{
				PredicateRegistry.ByName("Block.TagVisibilityCheck", true),
				PredicateRegistry.ByName("Position.IsWithin", true),
				PredicateRegistry.ByName("Sphere.MoveToTag", true),
				PredicateRegistry.ByName("Sphere.MoveThroughTag", true),
				PredicateRegistry.ByName("Sphere.AvoidTag", true),
				PredicateRegistry.ByName("Legs.GotoTag", true),
				PredicateRegistry.ByName("Quadped.GotoTag", true),
				PredicateRegistry.ByName("MLPLegs.GotoTag", true),
				PredicateRegistry.ByName("Character.GotoTag", true),
				PredicateRegistry.ByName("AnimCharacter.GotoTag", true),
				PredicateRegistry.ByName("Legs.ChaseTag", true),
				PredicateRegistry.ByName("Quadped.ChaseTag", true),
				PredicateRegistry.ByName("MLPLegs.ChaseTag", true),
				PredicateRegistry.ByName("Character.ChaseTag", true),
				PredicateRegistry.ByName("AnimCharacter.ChaseTag", true),
				PredicateRegistry.ByName("Legs.TurnTowardsTag", true),
				PredicateRegistry.ByName("Quadped.TurnTowardsTag", true),
				PredicateRegistry.ByName("MLPLegs.TurnTowardsTag", true),
				PredicateRegistry.ByName("Character.TurnTowardsTag", true),
				PredicateRegistry.ByName("AnimCharacter.TurnTowardsTag", true),
				PredicateRegistry.ByName("Legs.AvoidTag", true),
				PredicateRegistry.ByName("Quadped.AvoidTag", true),
				PredicateRegistry.ByName("MLPLegs.AvoidTag", true),
				PredicateRegistry.ByName("Character.AvoidTag", true),
				PredicateRegistry.ByName("AnimCharacter.AvoidTag", true),
				PredicateRegistry.ByName("Block.TaggedBump", true),
				PredicateRegistry.ByName("Block.TaggedBumpModel", true),
				PredicateRegistry.ByName("Block.TaggedBumpChunk", true),
				PredicateRegistry.ByName("Block.TeleportToTag", true),
				PredicateRegistry.ByName("Wheel.TurnTowardsTag", true),
				PredicateRegistry.ByName("Wheel.DriveTowardsTag", true),
				PredicateRegistry.ByName("Wheel.DriveTowardsTagRaw", true),
				PredicateRegistry.ByName("Wheel.IsWheelTowardsTag", true),
				PredicateRegistry.ByName("BulkyWheel.TurnTowardsTag", true),
				PredicateRegistry.ByName("BulkyWheel.DriveTowardsTag", true),
				PredicateRegistry.ByName("BulkyWheel.DriveTowardsTagRaw", true),
				PredicateRegistry.ByName("BulkyWheel.IsWheelTowardsTag", true),
				PredicateRegistry.ByName("GoldenWheel.TurnTowardsTag", true),
				PredicateRegistry.ByName("GoldenWheel.DriveTowardsTag", true),
				PredicateRegistry.ByName("GoldenWheel.DriveTowardsTagRaw", true),
				PredicateRegistry.ByName("GoldenWheel.IsWheelTowardsTag", true),
				PredicateRegistry.ByName("SpokedWheel.TurnTowardsTag", true),
				PredicateRegistry.ByName("SpokedWheel.DriveTowardsTag", true),
				PredicateRegistry.ByName("SpokedWheel.DriveTowardsTagRaw", true),
				PredicateRegistry.ByName("SpokedWheel.IsWheelTowardsTag", true),
				PredicateRegistry.ByName("RadarUI.TrackTag", true),
				PredicateRegistry.ByName("AntiGravity.TurnTowardsTagChunk", true),
				PredicateRegistry.ByName("AntiGravityColumn.TurnTowardsTagChunk", true),
				PredicateRegistry.ByName("WiserWing.TurnTowardsTagChunk", true),
				PredicateRegistry.ByName("BirdWing.TurnTowardsTagChunk", true),
				PredicateRegistry.ByName("BatWing.TurnTowardsTagChunk", true),
				PredicateRegistry.ByName("BatWingBackpack.TurnTowardsTagChunk", true),
				PredicateRegistry.ByName("FairyWings.TurnTowardsTagChunk", true),
				PredicateRegistry.ByName("MLPWings.TurnTowardsTagChunk", true),
				PredicateRegistry.ByName("Cape.TurnTowardsTagChunk", true),
				PredicateRegistry.ByName("Jetpack.TurnTowardsTagChunk", true),
				PredicateRegistry.ByName("FlightYoke.TurnTowardsTagChunk", true),
				PredicateRegistry.ByName("Block.IsTreasureForTag", true),
				PredicateRegistry.ByName("Block.IsPickupForTag", true),
				PredicateRegistry.ByName("Block.OnCollectByTag", true),
				PredicateRegistry.ByName("Missile.TargetTag", true),
				PredicateRegistry.ByName("MissileControl.TargetTag", true),
				PredicateRegistry.ByName("Block.TagProximityCheck", true),
				PredicateRegistry.ByName("SteeringWheel.DriveThroughTag", true),
				PredicateRegistry.ByName("SteeringWheel.AvoidTag", true),
				PredicateRegistry.ByName("Block.CameraFollowLookTowardTag", true),
				PredicateRegistry.ByName("Magnet.InfluenceTag", true),
				PredicateRegistry.ByName("Block.ExplodeTag", true),
				PredicateRegistry.ByName("TeleportVolume.TeleportTag", true)
			});
		}
		return Blocksworld.taggedPredicates;
	}

	// Token: 0x06001972 RID: 6514 RVA: 0x000B8754 File Offset: 0x000B6B54
	private static void AddUniqueBlocks(string s1, string s2)
	{
		HashSet<string> hashSet;
		if (!Blocksworld.uniqueBlockMap.TryGetValue(s1, out hashSet))
		{
			hashSet = new HashSet<string>();
			Blocksworld.uniqueBlockMap.Add(s1, hashSet);
		}
		hashSet.Add(s2);
	}

	// Token: 0x06001973 RID: 6515 RVA: 0x000B8790 File Offset: 0x000B6B90
	private static void AddUniqueBlockNames(params string[] blockNames)
	{
		for (int i = 0; i < blockNames.Length; i++)
		{
			string text = blockNames[i];
			Blocksworld.AddUniqueBlocks(text, text);
			for (int j = i + 1; j < blockNames.Length; j++)
			{
				string text2 = blockNames[j];
				Blocksworld.AddUniqueBlocks(text, text2);
				Blocksworld.AddUniqueBlocks(text2, text);
			}
		}
	}

	// Token: 0x06001974 RID: 6516 RVA: 0x000B87E4 File Offset: 0x000B6BE4
	public static Dictionary<string, HashSet<string>> GetUniqueBlockMap()
	{
		if (Blocksworld.uniqueBlockMap == null)
		{
			Blocksworld.uniqueBlockMap = new Dictionary<string, HashSet<string>>();
			Blocksworld.AddUniqueBlockNames(new string[]
			{
				"UI Counter I"
			});
			Blocksworld.AddUniqueBlockNames(new string[]
			{
				"UI Counter II"
			});
			Blocksworld.AddUniqueBlockNames(new string[]
			{
				"UI Timer I"
			});
			Blocksworld.AddUniqueBlockNames(new string[]
			{
				"UI Timer II"
			});
			Blocksworld.AddUniqueBlockNames(new string[]
			{
				"UI Gauge I"
			});
			Blocksworld.AddUniqueBlockNames(new string[]
			{
				"UI Gauge II"
			});
			Blocksworld.AddUniqueBlockNames(new string[]
			{
				"UI Object Counter I"
			});
			Blocksworld.AddUniqueBlockNames(new string[]
			{
				"UI Radar I"
			});
			Blocksworld.AddUniqueBlockNames(new string[]
			{
				"Master"
			});
			Blocksworld.AddUniqueBlockNames(new string[]
			{
				"Highscore I"
			});
			Blocksworld.AddUniqueBlockNames(new string[]
			{
				"Jukebox"
			});
			Blocksworld.AddUniqueBlockNames(new string[]
			{
				"Missile Control"
			});
		}
		return Blocksworld.uniqueBlockMap;
	}

	// Token: 0x06001975 RID: 6517 RVA: 0x000B88F0 File Offset: 0x000B6CF0
	private static bool UpdateDisabledBlockTypes(string blockType, HashSet<string> disabledTypes)
	{
		if (disabledTypes.Contains(blockType))
		{
			return false;
		}
		Dictionary<string, HashSet<string>> dictionary = Blocksworld.GetUniqueBlockMap();
		HashSet<string> hashSet;
		if (dictionary.TryGetValue(blockType, out hashSet))
		{
			foreach (string item in hashSet)
			{
				disabledTypes.Add(item);
			}
			return false;
		}
		return true;
	}

	// Token: 0x06001976 RID: 6518 RVA: 0x000B8970 File Offset: 0x000B6D70
	private static HashSet<GAF> GetUniqueGafs()
	{
		if (Blocksworld.uniqueGafs == null)
		{
			Blocksworld.uniqueGafs = new HashSet<GAF>();
			for (int i = 0; i < 8; i++)
			{
				Blocksworld.uniqueGafs.Add(new GAF("Block.SetSpawnpoint", new object[]
				{
					i
				}));
			}
		}
		return Blocksworld.uniqueGafs;
	}

	// Token: 0x06001977 RID: 6519 RVA: 0x000B89CC File Offset: 0x000B6DCC
	private static void UpdateUsedUniqueGafs(List<List<Tile>> tiles, HashSet<GAF> usedUniqueGafs)
	{
		HashSet<GAF> hashSet = Blocksworld.GetUniqueGafs();
		foreach (List<Tile> list in tiles)
		{
			foreach (Tile tile in list)
			{
				if (hashSet.Contains(tile.gaf))
				{
					usedUniqueGafs.Add(tile.gaf);
				}
			}
		}
	}

	// Token: 0x06001978 RID: 6520 RVA: 0x000B8A80 File Offset: 0x000B6E80
	private static PredicateSet GetMagnetPredicates()
	{
		if (Blocksworld.magnetPredicates == null)
		{
			Blocksworld.magnetPredicates = new PredicateSet(new HashSet<Predicate>
			{
				Block.predicatePulledByMagnet,
				Block.predicatePulledByMagnetModel,
				Block.predicatePushedByMagnet,
				Block.predicatePushedByMagnetModel
			});
		}
		return Blocksworld.magnetPredicates;
	}

	// Token: 0x06001979 RID: 6521 RVA: 0x000B8AE0 File Offset: 0x000B6EE0
	private static PredicateSet GetTaggedHandAttachmentPreds()
	{
		if (Blocksworld.taggedHandAttachmentPreds == null)
		{
			Blocksworld.taggedHandAttachmentPreds = new PredicateSet();
			Blocksworld.taggedHandAttachmentPreds.Add(PredicateRegistry.ByName("Block.HitByTaggedBlocksterHandAttachment", true));
			Blocksworld.taggedHandAttachmentPreds.Add(PredicateRegistry.ByName("Block.ModelHitByTaggedBlocksterHandAttachment", true));
		}
		return Blocksworld.taggedHandAttachmentPreds;
	}

	// Token: 0x0600197A RID: 6522 RVA: 0x000B8B30 File Offset: 0x000B6F30
	private static PredicateSet GetTaggedArrowPreds()
	{
		if (Blocksworld.taggedArrowPreds == null)
		{
			Blocksworld.taggedArrowPreds = new PredicateSet();
			Blocksworld.taggedArrowPreds.Add(PredicateRegistry.ByName("Block.HitByTaggedArrow", true));
			Blocksworld.taggedArrowPreds.Add(PredicateRegistry.ByName("Block.ModelHitByTaggedArrow", true));
		}
		return Blocksworld.taggedArrowPreds;
	}

	// Token: 0x0600197B RID: 6523 RVA: 0x000B8B80 File Offset: 0x000B6F80
	private static PredicateSet GetTaggedLaserPreds()
	{
		if (Blocksworld.taggedLaserPreds == null)
		{
			Blocksworld.taggedLaserPreds = new PredicateSet();
			Blocksworld.taggedLaserPreds.Add(PredicateRegistry.ByName("Laser.TaggedHitByBeam", true));
			Blocksworld.taggedLaserPreds.Add(PredicateRegistry.ByName("Block.HitByTaggedLaserModel", true));
			Blocksworld.taggedLaserPreds.Add(PredicateRegistry.ByName("Block.HitByTaggedLaserChunk", true));
		}
		return Blocksworld.taggedLaserPreds;
	}

	// Token: 0x0600197C RID: 6524 RVA: 0x000B8BE8 File Offset: 0x000B6FE8
	private static PredicateSet GetTaggedProjectilePreds()
	{
		if (Blocksworld.taggedProjectilePreds == null)
		{
			Blocksworld.taggedProjectilePreds = new PredicateSet();
			Blocksworld.taggedProjectilePreds.Add(PredicateRegistry.ByName("Block.TaggedHitByProjectile", true));
			Blocksworld.taggedProjectilePreds.Add(PredicateRegistry.ByName("Block.HitByTaggedProjectileModel", true));
			Blocksworld.taggedProjectilePreds.Add(PredicateRegistry.ByName("Block.HitByTaggedProjectileChunk", true));
		}
		return Blocksworld.taggedProjectilePreds;
	}

	// Token: 0x0600197D RID: 6525 RVA: 0x000B8C50 File Offset: 0x000B7050
	private static PredicateSet GetTaggedWaterPreds()
	{
		if (Blocksworld.taggedWaterPreds == null)
		{
			Blocksworld.taggedWaterPreds = new PredicateSet();
			Blocksworld.taggedWaterPreds.Add(PredicateRegistry.ByName("Block.WithinTaggedWater", true));
			Blocksworld.taggedWaterPreds.Add(PredicateRegistry.ByName("Block.WithinTaggedWaterChunk", true));
			Blocksworld.taggedWaterPreds.Add(PredicateRegistry.ByName("Block.WithinTaggedWaterModel", true));
		}
		return Blocksworld.taggedWaterPreds;
	}

	// Token: 0x0600197E RID: 6526 RVA: 0x000B8CB5 File Offset: 0x000B70B5
	public static void SetupBuildPanel()
	{
		if (Blocksworld.unlockedGAFs == null)
		{
			BWLog.Error("Inventory not loaded!");
			return;
		}
		Blocksworld.buildPanel.CreateInventoryTiles(new HashSet<GAF>(Blocksworld.unlockedGAFs));
	}

	// Token: 0x0600197F RID: 6527 RVA: 0x000B8CE0 File Offset: 0x000B70E0
	public static void UpdateTilesForTab(TabBarTabId tab, List<Tile> tilesInTab)
	{
		if (tilesInTab == null)
		{
			return;
		}
		HashSet<GAF> hiddenGafs = Tutorial.GetHiddenGafs();
		HashSet<GAF> neverHideGafs = Tutorial.GetNeverHideGafs();
		switch (tab)
		{
		case TabBarTabId.ActionBlocks:
			break;
		case TabBarTabId.Actions:
			Blocksworld.UpdateActionTiles(tilesInTab, hiddenGafs, neverHideGafs);
			goto IL_FC;
		case TabBarTabId.Sounds:
			Blocksworld.UpdateSFXTiles(tilesInTab, hiddenGafs, neverHideGafs);
			goto IL_FC;
		default:
			if (tab != TabBarTabId.Blocks)
			{
				foreach (Tile tile in tilesInTab)
				{
					if (tile.gaf.Predicate != Block.predicateThen)
					{
						if (tile.gaf.Predicate != BlockAnimatedCharacter.predicateReplaceLimb || (CharacterEditor.Instance.InEditMode() && CharacterEditor.Instance.CharacterBlock().characterType != CharacterType.Avatar))
						{
							tile.Show(true);
							tile.Enable(Blocksworld.TileEnabled(tile));
						}
					}
				}
				goto IL_FC;
			}
			break;
		}
		Blocksworld.UpdateCreateBlockTiles(tilesInTab, hiddenGafs, neverHideGafs);
		IL_FC:
		for (int i = 0; i < tilesInTab.Count; i++)
		{
			Tile tile2 = tilesInTab[i];
			if (!tile2.visibleInPanel)
			{
				tile2.Destroy();
			}
		}
	}

	// Token: 0x06001980 RID: 6528 RVA: 0x000B8E34 File Offset: 0x000B7234
	public static void UpdateCreateBlockTiles(List<Tile> tiles, HashSet<GAF> hiddenGafs, HashSet<GAF> neverHideGafs)
	{
		HashSet<string> hashSet = new HashSet<string>();
		List<Block> list = BWSceneManager.AllBlocks();
		for (int i = 0; i < list.Count; i++)
		{
			Block block = list[i];
			string blockType = block.BlockType();
			Blocksworld.UpdateDisabledBlockTypes(blockType, hashSet);
		}
		foreach (Tile tile in tiles)
		{
			GAF gaf = tile.gaf;
			if (!neverHideGafs.Contains(gaf))
			{
				if (hiddenGafs.Contains(gaf))
				{
					tile.Show(false);
				}
				else if (gaf.Predicate == Block.predicateThen)
				{
					tile.Show(false);
				}
				else
				{
					tile.Show(true);
					bool enabled = Blocksworld.TileEnabled(tile);
					if (gaf.Predicate == Block.predicateCreate && gaf.Args.Length > 0 && hashSet.Contains((string)gaf.Args[0]))
					{
						enabled = false;
					}
					tile.Enable(enabled);
				}
			}
		}
	}

	// Token: 0x06001981 RID: 6529 RVA: 0x000B8F68 File Offset: 0x000B7368
	public static void UpdateSFXTiles(List<Tile> sfxTiles, HashSet<GAF> hiddenGafs, HashSet<GAF> neverHideGafs)
	{
		Block selectedScriptBlock = Blocksworld.GetSelectedScriptBlock();
		bool flag = selectedScriptBlock != null;
		foreach (Tile tile in sfxTiles)
		{
			if (flag)
			{
				bool show = neverHideGafs.Contains(tile.gaf) || !hiddenGafs.Contains(tile.gaf);
				tile.Show(show);
				tile.Enable(true);
			}
			else
			{
				tile.Show(false);
			}
		}
	}

	// Token: 0x06001982 RID: 6530 RVA: 0x000B900C File Offset: 0x000B740C
	public static void UpdateActionTiles(List<Tile> actionTiles, HashSet<GAF> hiddenGafs, HashSet<GAF> neverHideGafs)
	{
		if (actionTiles == null)
		{
			return;
		}
		Block selectedScriptBlock = Blocksworld.GetSelectedScriptBlock();
		Blocksworld.ComputeNumSendAndTagTilesInUse();
		PredicateSet predicateSet = new PredicateSet();
		PredicateSet predicateSet2 = Blocksworld.GetMagnetPredicates();
		HashSet<GAF> usedUniqueGafs = new HashSet<GAF>();
		bool flag = false;
		HashSet<int> hashSet = new HashSet<int>();
		HashSet<int> hashSet2 = new HashSet<int>();
		HashSet<int> hashSet3 = new HashSet<int>();
		HashSet<int> hashSet4 = new HashSet<int>();
		HashSet<Predicate> hashSet5 = Blocksworld.GetTaggedPredicates();
		Predicate predicateTag = Block.predicateTag;
		PredicateSet predicateSet3 = Blocksworld.GetTaggedArrowPreds();
		PredicateSet predicateSet4 = Blocksworld.GetTaggedHandAttachmentPreds();
		PredicateSet predicateSet5 = Blocksworld.GetTaggedLaserPreds();
		PredicateSet predicateSet6 = Blocksworld.GetTaggedProjectilePreds();
		PredicateSet predicateSet7 = Blocksworld.GetTaggedWaterPreds();
		List<Block> list = BWSceneManager.AllBlocks();
		for (int i = 0; i < list.Count; i++)
		{
			Block block = list[i];
			string text = block.BlockType();
			Blocksworld.UpdateUsedUniqueGafs(block.tiles, usedUniqueGafs);
			BlockObjectCounterUI blockObjectCounterUI = block as BlockObjectCounterUI;
			if (blockObjectCounterUI != null)
			{
				hashSet.Add(blockObjectCounterUI.index);
			}
			BlockCounterUI blockCounterUI = block as BlockCounterUI;
			if (blockCounterUI != null)
			{
				hashSet2.Add(blockCounterUI.index);
			}
			BlockTimerUI blockTimerUI = block as BlockTimerUI;
			if (blockTimerUI != null)
			{
				hashSet3.Add(blockTimerUI.index);
			}
			BlockGaugeUI blockGaugeUI = block as BlockGaugeUI;
			if (blockGaugeUI != null)
			{
				hashSet4.Add(blockGaugeUI.index);
			}
			if (block is BlockMagnet)
			{
				flag = true;
			}
		}
		for (int j = 0; j < 2; j++)
		{
			if (!hashSet.Contains(j))
			{
				hiddenGafs.Add(new GAF("Block.SetAsTreasureBlockIcon", new object[]
				{
					j
				}));
				hiddenGafs.Add(new GAF("Block.SetAsTreasureTextureIcon", new object[]
				{
					j
				}));
				hiddenGafs.Add(new GAF("ObjectCounterUI.Increment", new object[]
				{
					1,
					j
				}));
				hiddenGafs.Add(new GAF("ObjectCounterUI.Decrement", new object[]
				{
					1,
					j
				}));
				hiddenGafs.Add(new GAF("ObjectCounterUI.Equals", new object[]
				{
					0,
					j
				}));
				hiddenGafs.Add(new GAF("ObjectCounterUI.EqualsMax", new object[]
				{
					j
				}));
				hiddenGafs.Add(new GAF("ObjectCounterUI.ValueCondition", new object[]
				{
					5,
					0,
					j
				}));
				hiddenGafs.Add(new GAF("ObjectCounterUI.ValueCondition", new object[]
				{
					5,
					1,
					j
				}));
				hiddenGafs.Add(new GAF("ObjectCounterUI.ValueCondition", new object[]
				{
					5,
					2,
					j
				}));
				if (j > 0)
				{
					hiddenGafs.Add(new GAF("Block.IsTreasure", new object[]
					{
						j
					}));
					for (int k = 0; k < 9; k++)
					{
						hiddenGafs.Add(new GAF("Block.IsTreasureForTag", new object[]
						{
							k.ToString(),
							j
						}));
					}
				}
			}
		}
		for (int l = 0; l < 2; l++)
		{
			if (!hashSet2.Contains(l))
			{
				hiddenGafs.Add(new GAF("Block.SetAsCounterUIBlockIcon", new object[]
				{
					l
				}));
				hiddenGafs.Add(new GAF("Block.SetAsCounterUITextureIcon", new object[]
				{
					l
				}));
				hiddenGafs.Add(new GAF("CounterUI.Increment", new object[]
				{
					1,
					l
				}));
				hiddenGafs.Add(new GAF("CounterUI.Increment", new object[]
				{
					-1,
					l
				}));
				hiddenGafs.Add(new GAF("CounterUI.Equals", new object[]
				{
					0,
					l
				}));
				hiddenGafs.Add(new GAF("CounterUI.EqualsMin", new object[]
				{
					l
				}));
				hiddenGafs.Add(new GAF("CounterUI.EqualsMax", new object[]
				{
					l
				}));
				hiddenGafs.Add(new GAF("CounterUI.AnimateScore", new object[]
				{
					0,
					l
				}));
				hiddenGafs.Add(new GAF("CounterUI.AnimateScore", new object[]
				{
					1,
					l
				}));
				hiddenGafs.Add(new GAF("CounterUI.ScoreMultiplier", new object[]
				{
					2,
					1,
					l
				}));
				hiddenGafs.Add(new GAF("CounterUI.GlobalScoreMultiplier", new object[]
				{
					2,
					1,
					l
				}));
				hiddenGafs.Add(new GAF("CounterUI.ValueCondition", new object[]
				{
					5,
					0,
					l
				}));
				hiddenGafs.Add(new GAF("CounterUI.ValueCondition", new object[]
				{
					5,
					1,
					l
				}));
				hiddenGafs.Add(new GAF("CounterUI.ValueCondition", new object[]
				{
					5,
					2,
					l
				}));
				hiddenGafs.Add(new GAF("CounterUI.Randomize", new object[]
				{
					0,
					10,
					l
				}));
				if (l == 0)
				{
					hiddenGafs.Add(new GAF("CounterUI.Equals", new object[]
					{
						0
					}));
				}
			}
		}
		for (int m = 0; m < 2; m++)
		{
			if (!hashSet4.Contains(m))
			{
				hiddenGafs.Add(new GAF("Block.SetAsGaugeUIBlockIcon", new object[]
				{
					m
				}));
				hiddenGafs.Add(new GAF("Block.SetAsGaugeUITextureIcon", new object[]
				{
					m
				}));
				hiddenGafs.Add(new GAF("GaugeUI.Increment", new object[]
				{
					1,
					m
				}));
				hiddenGafs.Add(new GAF("GaugeUI.Increment", new object[]
				{
					-1,
					m
				}));
				hiddenGafs.Add(new GAF("GaugeUI.Equals", new object[]
				{
					0,
					m
				}));
				hiddenGafs.Add(new GAF("GaugeUI.EqualsMax", new object[]
				{
					m
				}));
				hiddenGafs.Add(new GAF("GaugeUI.Fraction", new object[]
				{
					1f,
					m
				}));
				hiddenGafs.Add(new GAF("GaugeUI.ValueCondition", new object[]
				{
					5,
					0,
					m
				}));
				hiddenGafs.Add(new GAF("GaugeUI.ValueCondition", new object[]
				{
					5,
					1,
					m
				}));
				hiddenGafs.Add(new GAF("GaugeUI.ValueCondition", new object[]
				{
					5,
					2,
					m
				}));
			}
		}
		for (int n = 0; n < 2; n++)
		{
			if (!hashSet3.Contains(n))
			{
				hiddenGafs.Add(new GAF("Block.SetAsTimerUIBlockIcon", new object[]
				{
					n
				}));
				hiddenGafs.Add(new GAF("Block.SetAsTimerUITextureIcon", new object[]
				{
					n
				}));
				hiddenGafs.Add(new GAF("TimerUI.Equals", new object[]
				{
					0f,
					n
				}));
				hiddenGafs.Add(new GAF("TimerUI.Equals", new object[]
				{
					0f,
					n
				}));
				hiddenGafs.Add(new GAF("TimerUI.EqualsMax", new object[]
				{
					n
				}));
				hiddenGafs.Add(new GAF("TimerUI.Increment", new object[]
				{
					1f,
					n
				}));
				hiddenGafs.Add(new GAF("TimerUI.Increment", new object[]
				{
					-1f,
					n
				}));
				hiddenGafs.Add(new GAF("TimerUI.Start", new object[]
				{
					1,
					n
				}));
				hiddenGafs.Add(new GAF("TimerUI.Start", new object[]
				{
					-1,
					n
				}));
				hiddenGafs.Add(new GAF("TimerUI.Pause", new object[]
				{
					n
				}));
				hiddenGafs.Add(new GAF("TimerUI.Wait", new object[]
				{
					1f,
					n
				}));
				hiddenGafs.Add(new GAF("TimerUI.PauseUI", new object[]
				{
					1f,
					n
				}));
				hiddenGafs.Add(new GAF("TimerUI.ValueCondition", new object[]
				{
					5f,
					0,
					n
				}));
				hiddenGafs.Add(new GAF("TimerUI.ValueCondition", new object[]
				{
					5f,
					1,
					n
				}));
				hiddenGafs.Add(new GAF("TimerUI.ValueCondition", new object[]
				{
					5f,
					2,
					n
				}));
			}
		}
		if (Blocksworld.worldOcean == null)
		{
			predicateSet.Add(BlockMaster.predicateSetLiquidProperties);
			predicateSet.Add(BlockMaster.predicateIncreaseWaterLevel);
			predicateSet.Add(BlockMaster.predicateStepIncreaseWaterLevel);
			predicateSet.Add(BlockMaster.predicateSetMaxPositiveWaterLevelOffset);
			predicateSet.Add(BlockMaster.predicateSetMaxNegativeWaterLevelOffset);
		}
		predicateSet.UnionWith(Block.globalVariableOperations);
		predicateSet.UnionWith(Block.blockVariableOperations);
		predicateSet.UnionWith(Block.blockVariableOperationsOnGlobals);
		predicateSet.UnionWith(Block.blockVariableOperationsOnOtherBlockVars);
		HashSet<GAF> hashSet6 = new HashSet<GAF>();
		foreach (int id in TileToggleChain.GetAllNonPrimitiveBlockIds())
		{
			BlockItem blockItem = BlockItem.FindByID(id);
			GAF item = new GAF(blockItem);
			if (hiddenGafs.Contains(item))
			{
				hashSet6.Add(item);
			}
			hiddenGafs.Add(item);
		}
		foreach (Tile tile in actionTiles)
		{
			GAF gaf = tile.gaf;
			if (!neverHideGafs.Contains(gaf))
			{
				if (predicateSet.Contains(gaf.Predicate))
				{
					tile.Show(false);
					continue;
				}
				if (hiddenGafs.Contains(gaf))
				{
					tile.Show(false);
					continue;
				}
				if (!Blocksworld.publicProvidedGafs.Contains(gaf) && (selectedScriptBlock == null || !gaf.Predicate.CompatibleWith(selectedScriptBlock)))
				{
					tile.Show(false);
					continue;
				}
				if (selectedScriptBlock != null)
				{
					if ((gaf.Predicate == Block.predicateSendSignal || gaf.Predicate == Block.predicateSendSignalModel) && (int)gaf.Args[0] > Blocksworld.numSendTilesInUse)
					{
						tile.Show(false);
						continue;
					}
					if (gaf.Predicate == Block.predicateSendCustomSignal || gaf.Predicate == Block.predicateSendCustomSignalModel || gaf.Predicate == Block.predicateVariableCustomInt || gaf.Predicate == Block.predicateBlockVariableInt)
					{
						tile.Show(false);
						continue;
					}
					bool flag2 = gaf.Predicate == predicateTag;
					if (flag2 || hashSet5.Contains(gaf.Predicate))
					{
						int num = (!flag2) ? (Blocksworld.numTagTilesInUse - 1) : Blocksworld.numTagTilesInUse;
						string text2 = (string)gaf.Args[0];
						int num2;
						if (int.TryParse(text2, out num2))
						{
							if (num2 > num)
							{
								tile.Show(false);
								continue;
							}
						}
						else if (!flag2 && !Blocksworld.everPresentTagsInUse.Contains(text2))
						{
							tile.Show(false);
							continue;
						}
					}
					if (predicateSet3.Contains(gaf.Predicate) && gaf.Args.Length > 0 && !Blocksworld.arrowTags.Contains((string)gaf.Args[0]))
					{
						tile.Show(false);
						continue;
					}
					if (predicateSet4.Contains(gaf.Predicate) && gaf.Args.Length > 0 && !Blocksworld.handAttachmentTags.Contains((string)gaf.Args[0]))
					{
						tile.Show(false);
						continue;
					}
					if (predicateSet5.Contains(gaf.Predicate) && gaf.Args.Length > 0 && !Blocksworld.laserTags.Contains((string)gaf.Args[0]))
					{
						tile.Show(false);
						continue;
					}
					if (predicateSet6.Contains(gaf.Predicate) && gaf.Args.Length > 0 && !Blocksworld.projectileTags.Contains((string)gaf.Args[0]))
					{
						tile.Show(false);
						continue;
					}
					if (predicateSet7.Contains(gaf.Predicate) && gaf.Args.Length > 0 && !Blocksworld.waterTags.Contains((string)gaf.Args[0]))
					{
						tile.Show(false);
						continue;
					}
					if (!flag && predicateSet2.Contains(gaf.Predicate))
					{
						tile.Show(false);
						continue;
					}
				}
				if (gaf.Predicate == Block.predicateThen)
				{
					tile.Show(false);
					continue;
				}
				if (selectedScriptBlock != null && !selectedScriptBlock.SupportsGaf(gaf))
				{
					tile.Show(false);
					continue;
				}
				if (gaf.Predicate == BlockMaster.predicatePaintSkyTo && !Blocksworld.unlockedPaints.Contains((string)gaf.Args[0]))
				{
					tile.Show(false);
					continue;
				}
			}
			tile.Show(true);
			tile.Enable(Blocksworld.TileEnabled(tile));
		}
	}

	// Token: 0x06001983 RID: 6531 RVA: 0x000BA004 File Offset: 0x000B8404
	public static void UpdateTiles()
	{
		Blocksworld.buildPanel.Layout();
		Blocksworld.SetupClipboard();
	}

	// Token: 0x06001984 RID: 6532 RVA: 0x000BA018 File Offset: 0x000B8418
	public static string NextAvailableBlockVariableName(Block b)
	{
		int num = 0;
		if (Blocksworld.blockIntVariables.ContainsKey(b))
		{
			num = Blocksworld.blockIntVariables[b].Count;
		}
		return "b" + num;
	}

	// Token: 0x06001985 RID: 6533 RVA: 0x000BA058 File Offset: 0x000B8458
	private static void ComputeNumSendAndTagTilesInUse()
	{
		Blocksworld.numSendTilesInUse = 0;
		Blocksworld.numTagTilesInUse = 0;
		Blocksworld.everPresentTagsInUse.Clear();
		Blocksworld.arrowTags.Clear();
		Blocksworld.handAttachmentTags.Clear();
		Blocksworld.laserTags.Clear();
		Blocksworld.projectileTags.Clear();
		Blocksworld.waterTags.Clear();
		Blocksworld.customSignals.Clear();
		Blocksworld.customSignals.Add("*");
		Blocksworld.customIntVariables.Clear();
		Blocksworld.blockIntVariables.Clear();
		Predicate predicateTag = Block.predicateTag;
		Predicate predicate = PredicateRegistry.ByName("Block.HitByArrow", true);
		Predicate predicate2 = PredicateRegistry.ByName("Laser.HitByBeam", true);
		List<Block> list = BWSceneManager.AllBlocks();
		for (int i = 0; i < list.Count; i++)
		{
			Block block = list[i];
			for (int j = 0; j < block.tiles.Count; j++)
			{
				List<Tile> list2 = block.tiles[j];
				for (int k = 0; k < list2.Count; k++)
				{
					Tile tile = list2[k];
					Predicate predicate3 = tile.gaf.Predicate;
					if (predicate3 == Block.predicateSendSignal || predicate3 == Block.predicateSendSignalModel)
					{
						int a = (int)tile.gaf.Args[0] + 1;
						Blocksworld.numSendTilesInUse = Mathf.Max(a, Blocksworld.numSendTilesInUse);
					}
					else if (predicate3 == predicateTag)
					{
						string text = (string)tile.gaf.Args[0];
						int num;
						if (int.TryParse(text, out num))
						{
							Blocksworld.numTagTilesInUse = Mathf.Max(num + 1, Blocksworld.numTagTilesInUse);
						}
						else
						{
							Blocksworld.everPresentTagsInUse.Add(text);
						}
						BlockAbstractLaser blockAbstractLaser = block as BlockAbstractLaser;
						if (blockAbstractLaser != null)
						{
							if (blockAbstractLaser.CanFireLaser())
							{
								Blocksworld.laserTags.Add((string)tile.gaf.Args[0]);
							}
							if (blockAbstractLaser.CanFireProjectiles())
							{
								Blocksworld.projectileTags.Add((string)tile.gaf.Args[0]);
							}
						}
						else if (block is BlockAbstractBow)
						{
							Blocksworld.arrowTags.Add((string)tile.gaf.Args[0]);
						}
						else if (block is BlockWaterCube)
						{
							Blocksworld.waterTags.Add((string)tile.gaf.Args[0]);
						}
						else if (BlockAnimatedCharacter.FindPropHolder(block) != null)
						{
							Blocksworld.handAttachmentTags.Add((string)tile.gaf.Args[0]);
						}
					}
					else if (predicate3 == predicate2 && tile.gaf.Args.Length > 0)
					{
						Blocksworld.laserTags.Add((string)tile.gaf.Args[0]);
					}
					else if (predicate3 == predicate && tile.gaf.Args.Length > 0)
					{
						Blocksworld.arrowTags.Add((string)tile.gaf.Args[0]);
					}
					else if (predicate3 == Block.predicateSendCustomSignal || predicate3 == Block.predicateSendCustomSignalModel)
					{
						string text2 = (string)tile.gaf.Args[0];
						if (text2 != "*")
						{
							Blocksworld.customSignals.Add(text2);
						}
					}
					else if (predicate3 == Block.predicateVariableCustomInt)
					{
						string text3 = (string)tile.gaf.Args[0];
						if (text3 != "*" && !Blocksworld.customIntVariables.ContainsKey(text3))
						{
							Blocksworld.customIntVariables.Add(text3, (int)tile.gaf.Args[1]);
						}
					}
					else if (predicate3 == Block.predicateBlockVariableInt)
					{
						string text4 = (string)tile.gaf.Args[0];
						if (text4 != "*")
						{
							if (!Blocksworld.blockIntVariables.ContainsKey(block))
							{
								Blocksworld.blockIntVariables[block] = new Dictionary<string, int>();
								Blocksworld.blockIntVariables[block].Add(text4, (int)tile.gaf.Args[1]);
							}
							else if (!Blocksworld.blockIntVariables[block].ContainsKey(text4))
							{
								Blocksworld.blockIntVariables[block].Add(text4, (int)tile.gaf.Args[1]);
							}
						}
					}
				}
			}
		}
	}

	// Token: 0x06001986 RID: 6534 RVA: 0x000BA508 File Offset: 0x000B8908
	private void WhiteBackground()
	{
		if (this.whiteBackground == 0)
		{
			if (WorldSession.current != null && WorldSession.current.worldTitle == "Logo")
			{
				Blocksworld.blocksworldCamera.SetCameraPosition(new Vector3(-21.5f, 26f, 34f));
				Blocksworld.cameraTransform.eulerAngles = new Vector3(45f, 180f, 0f);
				GameObject gameObject = GameObject.Find("Chunk 9");
				gameObject.transform.position = new Vector3(-26f, 2f, -1.25f);
				gameObject.transform.rotation = Quaternion.Euler(0f, 40f, 0f);
				GameObject gameObject2 = GameObject.Find("Chunk 10");
				gameObject2.transform.Translate(0.5f, 0f, 1f);
				gameObject2.transform.Rotate(0f, 12.5f, 0f);
				BlockLegs blockLegs = BWSceneManager.FindBlock(GameObject.Find("CreateLegs1x1x1 91"), false) as BlockLegs;
				blockLegs.feet[0].go.transform.Translate(0.5f, 0f, 1f);
				blockLegs.feet[0].go.transform.Rotate(0f, 22.5f, 0f);
				blockLegs.feet[1].go.transform.Translate(0.5f, 0f, 1.1f);
				blockLegs.feet[1].go.transform.Rotate(0f, 12.5f, 0f);
				Blocksworld.directionalLight.transform.rotation = Quaternion.Euler(65f, 180f, 0f);
				this.StatePlayUpdate();
			}
			if (WorldSession.current != null && WorldSession.current.worldTitle == "Logo Compact")
			{
				Blocksworld.blocksworldCamera.SetCameraPosition(new Vector3(-32f, 17f, 14f));
				Blocksworld.cameraTransform.eulerAngles = new Vector3(45f, 180f, 0f);
				GameObject gameObject3 = GameObject.Find("Chunk 6");
				gameObject3.transform.position = new Vector3(-24f, 2f, -1.25f);
				gameObject3.transform.rotation = Quaternion.Euler(0f, 30f, 0f);
				GameObject gameObject4 = GameObject.Find("Chunk 3");
				gameObject4.transform.Translate(-1.25f, 0f, 2.5f);
				gameObject4.transform.Rotate(0f, -12.5f, 0f);
				BlockLegs blockLegs2 = BWSceneManager.FindBlock(GameObject.Find("CreateLegs1x1x1 38"), false) as BlockLegs;
				blockLegs2.feet[0].go.transform.Translate(-1.25f, 0f, 2.6f);
				blockLegs2.feet[0].go.transform.Rotate(0f, -12.5f, 0f);
				blockLegs2.feet[1].go.transform.Translate(-1.25f, 0f, 2.5f);
				blockLegs2.feet[1].go.transform.Rotate(0f, -22.5f, 0f);
				Blocksworld.directionalLight.transform.rotation = Quaternion.Euler(65f, 180f, 0f);
				this.StatePlayUpdate();
			}
			Blocksworld.mainCamera.backgroundColor = new Color(0.23046875f, 0.55859375f, 0.83203125f, 0f);
			foreach (KeyValuePair<string, Material> keyValuePair in Materials.materialCache)
			{
				keyValuePair.Value.SetFloat("_FogStart", 2000f);
				keyValuePair.Value.SetFloat("_FogEnd", 2000f);
			}
			List<Block> list = BWSceneManager.AllBlocks();
			for (int i = 0; i < list.Count; i++)
			{
				Block block = list[i];
				if (block.goShadow != null)
				{
					block.goShadow.GetComponent<Renderer>().enabled = false;
				}
				if (block.isTerrain)
				{
					block.go.GetComponent<Renderer>().enabled = false;
				}
			}
			this.whiteBackground = 1;
		}
		else
		{
			Color backgroundColor = new Color(0.380392164f, 0.7411765f, 1f);
			Blocksworld.mainCamera.backgroundColor = backgroundColor;
			foreach (KeyValuePair<string, Material> keyValuePair2 in Materials.materialCache)
			{
				keyValuePair2.Value.SetFloat("_FogStart", Blocksworld.fogStart * Blocksworld.fogMultiplier);
				keyValuePair2.Value.SetFloat("_FogEnd", Blocksworld.fogEnd * Blocksworld.fogMultiplier);
			}
			List<Block> list2 = BWSceneManager.AllBlocks();
			for (int j = 0; j < list2.Count; j++)
			{
				Block block2 = list2[j];
				bool enabled = true;
				if (block2 is BlockPosition && Blocksworld.CurrentState == State.Play)
				{
					enabled = false;
				}
				block2.go.GetComponent<Renderer>().enabled = enabled;
				if (block2.goShadow != null)
				{
					block2.goShadow.GetComponent<Renderer>().material.SetFloat("_FogStart", Blocksworld.fogStart * Blocksworld.fogMultiplier);
					block2.goShadow.GetComponent<Renderer>().material.SetFloat("_FogEnd", Blocksworld.fogEnd * Blocksworld.fogMultiplier);
				}
			}
			if (!Blocksworld.renderingShadows)
			{
				GameObject gameObject5 = Resources.Load("Blocks/Shadow") as GameObject;
				gameObject5.GetComponent<Renderer>().sharedMaterial.SetFloat("_FogStart", Blocksworld.fogStart * Blocksworld.fogMultiplier);
				gameObject5.GetComponent<Renderer>().sharedMaterial.SetFloat("_FogEnd", Blocksworld.fogEnd * Blocksworld.fogMultiplier);
			}
			this.whiteBackground = 0;
		}
	}

	// Token: 0x06001987 RID: 6535 RVA: 0x000BAB78 File Offset: 0x000B8F78
	public void SetFogMultiplier(float m)
	{
		Blocksworld.fogMultiplier = m;
		this.SetFog(Blocksworld.fogStart, Blocksworld.fogEnd);
	}

	// Token: 0x06001988 RID: 6536 RVA: 0x000BAB90 File Offset: 0x000B8F90
	public void SetFog(float start, float end)
	{
		Blocksworld.fogStart = start;
		Blocksworld.fogEnd = end;
		if (!Blocksworld.renderingShadows)
		{
			start *= Blocksworld.fogMultiplier;
			end *= Blocksworld.fogMultiplier;
			foreach (KeyValuePair<string, Material> keyValuePair in Materials.materialCache)
			{
				if (keyValuePair.Value != null)
				{
					keyValuePair.Value.SetFloat("_FogStart", start);
					keyValuePair.Value.SetFloat("_FogEnd", end);
				}
				else
				{
					BWLog.Info(keyValuePair.Key + " material was destroyed");
				}
			}
			List<Block> list = BWSceneManager.AllBlocks();
			for (int i = 0; i < list.Count; i++)
			{
				Block block = list[i];
				GameObject goShadow = block.goShadow;
				if (goShadow != null)
				{
					Material sharedMaterial = goShadow.GetComponent<Renderer>().sharedMaterial;
					sharedMaterial.SetFloat("_FogStart", start);
					sharedMaterial.SetFloat("_FogEnd", end);
				}
			}
			List<BlockTerrain> list2 = BWSceneManager.AllTerrainBlocks();
			for (int j = 0; j < list2.Count; j++)
			{
				BlockTerrain blockTerrain = list2[j];
				if (blockTerrain.go != null)
				{
					blockTerrain.SetFog(start, end);
				}
			}
			Material material = Block.prefabShadow.GetComponent<Renderer>().material;
			material.SetFloat("_FogStart", start);
			material.SetFloat("_FogEnd", end);
		}
		Blocksworld.weather.FogChanged();
	}

	// Token: 0x06001989 RID: 6537 RVA: 0x000BAD40 File Offset: 0x000B9140
	public static void UpdateFogColor(Color newFogColor)
	{
		Blocksworld.fogColor = newFogColor;
		if (!Blocksworld.renderingShadows)
		{
			foreach (KeyValuePair<string, Material> keyValuePair in Materials.materialCache)
			{
				if (keyValuePair.Value != null)
				{
					keyValuePair.Value.SetColor("_FogColor", Blocksworld.fogColor);
				}
			}
			List<BlockTerrain> list = BWSceneManager.AllTerrainBlocks();
			for (int i = 0; i < list.Count; i++)
			{
				list[i].UpdateFogColor(Blocksworld.fogColor);
			}
			Material sharedMaterial = Blocksworld.worldSky.renderer.sharedMaterial;
			sharedMaterial.SetColor("_FogColor", Blocksworld.fogColor);
			Block.prefabShadow.GetComponent<Renderer>().sharedMaterial.SetColor("_FogColor", Blocksworld.fogColor);
		}
		Blocksworld.weather.FogChanged();
	}

	// Token: 0x0600198A RID: 6538 RVA: 0x000BAE44 File Offset: 0x000B9244
	public static void UpdateSunLight(Color tint, Color emissiveColor, float lightIntensity, float sunIntensity)
	{
		if (Blocksworld.renderingSkybox && Blocksworld.overheadLight != null)
		{
			Blocksworld.overheadLight.color = 0.5f * (Color.white + tint) + emissiveColor;
			Blocksworld._lightIntensityBasic = lightIntensity;
			Blocksworld.UpdateSunIntensity(sunIntensity);
		}
	}

	// Token: 0x0600198B RID: 6539 RVA: 0x000BAE9C File Offset: 0x000B929C
	public static void UpdateSunIntensity(float sunIntensity)
	{
		if (Blocksworld.renderingSkybox && Blocksworld.overheadLight != null)
		{
			Blocksworld._lightIntensityMultiplier = sunIntensity;
			float num = Blocksworld._lightIntensityBasic * Blocksworld._lightIntensityMultiplier;
			Blocksworld.overheadLight.intensity = num;
			Blocksworld.overheadLight.bounceIntensity = num * 0.35f / 0.9f;
			Blocksworld.overheadLight.shadowStrength = Mathf.Clamp(num * num, 0.3f, 1f);
			DynamicGI.UpdateEnvironment();
		}
	}

	// Token: 0x0600198C RID: 6540 RVA: 0x000BAF18 File Offset: 0x000B9318
	public static void ResetSkyAndFogSettings()
	{
		if (Blocksworld.worldSky != null)
		{
			Blocksworld.worldSky.SetSkyBoxRotation(0f, true);
		}
		Blocksworld.UpdateSunIntensity(1f);
		Blocksworld.UpdateFogColor(Blocksworld._buildModeFogColor);
		Blocksworld.bw.SetFog(Blocksworld._buildModeFogStart, Blocksworld._buildModeFogEnd);
	}

	// Token: 0x0600198D RID: 6541 RVA: 0x000BAF67 File Offset: 0x000B9367
	public static void RenderScreenshot(Texture2D imageTexture, bool includeGuiCamera = false, FilterMode filterMode = FilterMode.Bilinear)
	{
		Blocksworld.RenderScreenshot(imageTexture, Blocksworld.mainCamera, (!includeGuiCamera) ? null : Blocksworld.guiCamera, filterMode);
	}

	// Token: 0x0600198E RID: 6542 RVA: 0x000BAF88 File Offset: 0x000B9388
	public static void RenderScreenshot(Texture2D imageTexture, Camera camera, Camera overlayCamera, FilterMode filterMode = FilterMode.Bilinear)
	{
		int width = imageTexture.width;
		int height = imageTexture.height;
		bool flag = Blocksworld.blocksworldCamera.currentReticle && Blocksworld.blocksworldCamera.currentReticle.enabled;
		if (flag)
		{
			Blocksworld.blocksworldCamera.currentReticle.enabled = false;
		}
		RenderTexture renderTexture = new RenderTexture(width, height, 24);
		renderTexture.filterMode = filterMode;
		camera.targetTexture = renderTexture;
		camera.Render();
		camera.targetTexture = null;
		if (overlayCamera != null)
		{
			overlayCamera.targetTexture = renderTexture;
			overlayCamera.Render();
			overlayCamera.targetTexture = null;
		}
		RenderTexture.active = renderTexture;
		imageTexture.ReadPixels(new Rect(0f, 0f, (float)width, (float)height), 0, 0);
		RenderTexture.active = null;
		UnityEngine.Object.Destroy(renderTexture);
		if (flag)
		{
			Blocksworld.blocksworldCamera.currentReticle.enabled = true;
		}
	}

	// Token: 0x0600198F RID: 6543 RVA: 0x000BB06C File Offset: 0x000B946C
	public static string SaveScreenshot(bool includeGuiCamera = false, string path = null, int w = 4096, int h = 3072, FilterMode filterMode = FilterMode.Bilinear)
	{
		BWLog.Info("Generating screenshot...");
		string text = Application.dataPath + "/..";
		if (BWUser.currentUser != null)
		{
			text = BWFilesystem.CurrentUserScreenshotsFolder;
		}
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		Texture2D texture2D = new Texture2D(w, h, TextureFormat.RGB24, false);
		Blocksworld.RenderScreenshot(texture2D, includeGuiCamera, filterMode);
		texture2D.Apply();
		byte[] bytes = texture2D.EncodeToPNG();
		string text2 = string.Empty;
		if (path == null)
		{
			int num = 100;
			for (int i = 1; i <= num; i++)
			{
				string str = i.ToString("D3");
				text2 = "Blocksworld " + str + ".png";
				path = Path.Combine(text, text2);
				if (!File.Exists(path))
				{
					break;
				}
				if (i == num)
				{
					OnScreenLog.AddLogItem("Too many screenshots in folder", 5f, false);
				}
			}
		}
		File.WriteAllBytes(path, bytes);
		BWLog.Info("Wrote: " + path);
		Blocksworld.UI.Overlay.ShowTimedOnScreenMessage("Wrote: " + text2, 3f);
		UnityEngine.Object.Destroy(texture2D);
		return path;
	}

	// Token: 0x06001990 RID: 6544 RVA: 0x000BB18C File Offset: 0x000B958C
	public static void SaveMeshToFile(Bunch bunch)
	{
		BWLog.Info("Saving mesh...");
		string text = Application.dataPath + "/..";
		if (Options.ScreenshotDirectory != string.Empty)
		{
			if (Options.ScreenshotDirectory.StartsWith("/"))
			{
				text = Options.ScreenshotDirectory;
			}
			else
			{
				text = text + "/" + Options.ScreenshotDirectory;
			}
		}
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		bool flag = true;
		string text2 = (!flag) ? ".stl" : ".obj";
		string text3 = "BWMesh" + text2;
		int num = 100;
		for (int i = 1; i <= num; i++)
		{
			string str = i.ToString("D3");
			text3 = text + "/BWMesh " + str + text2;
			if (!File.Exists(text3))
			{
				break;
			}
			if (i == num)
			{
				OnScreenLog.AddLogItem("Too many mesh files in folder", 5f, false);
			}
		}
		MeshUtils.Export(text3, bunch);
		BWLog.Info("Wrote: " + text3);
		Blocksworld.UI.Overlay.ShowTimedOnScreenMessage("Wrote: " + text3, 3f);
	}

	// Token: 0x06001991 RID: 6545 RVA: 0x000BB2C4 File Offset: 0x000B96C4
	public void ButtonPlayTapped()
	{
		if (Blocksworld.resettingPlay)
		{
			return;
		}
		if (Blocksworld.CurrentState == State.Paused)
		{
			Sound.PlayOneShotSound("Button Generic", 1f);
			WorldSession.current.Unpause();
			Blocksworld.SetBlocksworldState(State.Play);
			WorldSession.current.OnPlay();
			VisualEffect.ResumeVfxs();
			Blocksworld.weather.Resume();
		}
		else if (Blocksworld.CurrentState == State.FrameCapture)
		{
			Sound.PlayOneShotSound("Button Generic", 1f);
			WorldSession.current.ExitScreenCaptureSetup();
			WorldSession.current.OnPlay();
			VisualEffect.ResumeVfxs();
			Blocksworld.weather.Resume();
		}
		else
		{
			if (Blocksworld.rewardVisualizationGafs != null)
			{
				BWLog.Info("Not calling fast save since the reward vis gafs was not null");
				return;
			}
			if (Tutorial.state != TutorialState.None)
			{
				this.FastSave();
			}
			else
			{
				BWLog.Info("Not calling fast save since tutorial state was " + Tutorial.state);
			}
			Sound.PlayOneShotSound("Button Play", 1f);
			BW.Analytics.SendAnalyticsEvent("world-play");
			this.Play();
		}
	}

	// Token: 0x06001992 RID: 6546 RVA: 0x000BB3D0 File Offset: 0x000B97D0
	public void ButtonExitWorldTapped()
	{
		if (Blocksworld.rewardVisualizationGafs != null)
		{
			return;
		}
		if (Blocksworld.resettingPlay)
		{
			return;
		}
		if (BW.isIPad && IOSInterface.IsStartingRecording())
		{
			return;
		}
		if (Blocksworld.CurrentState == State.Play)
		{
			this.Stop(true, true);
		}
		if (BW.Options.saveOnWorldExit() && Blocksworld.CurrentState == State.Build && !Blocksworld.f3PressedInCurrentWorld)
		{
			Blocksworld.Deselect(true, true);
			this.Save();
		}
		OnScreenLog.Clear();
		Sound.PlayOneShotSound("Button Generic", 1f);
		Tutorial.ResetState();
		Blocksworld.leaderboardData.Reset();
		WorldSession.Quit();
	}

	// Token: 0x06001993 RID: 6547 RVA: 0x000BB474 File Offset: 0x000B9874
	public void ButtonMenuTapped()
	{
		if (Blocksworld.CurrentState == State.Play)
		{
			if (WorldSession.isCommunitySession())
			{
				Blocksworld.UI.Dialog.ShowEscapeMenuForCommunityWorld();
			}
			else
			{
				Blocksworld.UI.Dialog.ShowEscapeMenuForLocalWorldPlayMode();
			}
		}
		else if (Blocksworld.CurrentState == State.Build)
		{
			Blocksworld.UI.Dialog.ShowEscapeMenuForLocalWorldBuildMode();
		}
	}

	// Token: 0x06001994 RID: 6548 RVA: 0x000BB4D7 File Offset: 0x000B98D7
	public void ButtonPauseTapped()
	{
		if (Blocksworld.rewardVisualizationGafs != null)
		{
			return;
		}
		if (Blocksworld.resettingPlay)
		{
			return;
		}
		Sound.PlayOneShotSound("Button Generic", 1f);
		WorldSession.current.PauseButtonPressed();
	}

	// Token: 0x06001995 RID: 6549 RVA: 0x000BB508 File Offset: 0x000B9908
	public void ButtonStopTapped()
	{
		if (Blocksworld.rewardVisualizationGafs != null)
		{
			return;
		}
		if (Blocksworld.resettingPlay)
		{
			return;
		}
		if (Blocksworld.CurrentState == State.FrameCapture)
		{
			Sound.PlayOneShotSound("Button Generic", 1f);
			BWSceneManager.ResumeScene();
			Blocksworld.SetBlocksworldState(State.Play);
			WorldSession.current.OnPlay();
		}
		else
		{
			Sound.PlayOneShotSound("Button Stop", 1f);
			BW.Analytics.SendAnalyticsEvent("world-stop");
			if (WorldSession.jumpRestoreConfig != null)
			{
				WorldSession.RestoreJumpConfig();
			}
			else
			{
				this.Stop(false, true);
				Blocksworld.SetBlocksworldState(State.Build);
			}
		}
	}

	// Token: 0x06001996 RID: 6550 RVA: 0x000BB59F File Offset: 0x000B999F
	public void ButtonOptionsTapped()
	{
		Sound.PlayOneShotSound("Button Generic", 1f);
		if (Blocksworld.resettingPlay)
		{
			return;
		}
		this.ShowOptionsScreen();
	}

	// Token: 0x06001997 RID: 6551 RVA: 0x000BB5C4 File Offset: 0x000B99C4
	public void ShowOptionsScreen()
	{
		if (Blocksworld.CurrentState != State.Paused)
		{
			WorldSession.current.Pause();
			this._showingOptionsWhenPaused = false;
		}
		else
		{
			this._showingOptionsWhenPaused = true;
		}
		Blocksworld.lockInput = true;
		Blocksworld.blocksworldCamera.SetCameraStill(true);
		string sessionTitle = WorldSession.current.sessionTitle;
		string sessionUserName = WorldSession.current.sessionUserName;
		string sessionDescription = WorldSession.current.sessionDescription;
		Blocksworld.UI.ShowOptionsScreen(sessionTitle, sessionUserName, sessionDescription);
		Blocksworld.leaderboardData.PauseLeaderboard(true);
	}

	// Token: 0x06001998 RID: 6552 RVA: 0x000BB644 File Offset: 0x000B9A44
	public void HideOptionsScreen()
	{
		Blocksworld.leaderboardData.PauseLeaderboard(false);
		Blocksworld.UI.HideOptionsScreen();
		if (!this._showingOptionsWhenPaused)
		{
			WorldSession.current.Unpause();
		}
		this._showingOptionsWhenPaused = false;
		Blocksworld.lockInput = false;
		Blocksworld.blocksworldCamera.SetCameraStill(false);
	}

	// Token: 0x06001999 RID: 6553 RVA: 0x000BB694 File Offset: 0x000B9A94
	public void ButtonRestartTapped()
	{
		if (Blocksworld.resettingPlay)
		{
			return;
		}
		if (Blocksworld.rewardVisualizationGafs != null)
		{
			return;
		}
		if (Blocksworld.CurrentState == State.Paused)
		{
			WorldSession.current.Unpause();
		}
		else if (Blocksworld.CurrentState == State.FrameCapture)
		{
			WorldSession.current.ExitScreenCaptureSetup();
		}
		Sound.PlayOneShotSound("Button Rewind", 1f);
		BW.Analytics.SendAnalyticsEvent("world-rewind");
		this.Restart();
	}

	// Token: 0x0600199A RID: 6554 RVA: 0x000BB70B File Offset: 0x000B9B0B
	public void ButtonCaptureSetupTapped()
	{
		if (Blocksworld.rewardVisualizationGafs != null)
		{
			return;
		}
		Sound.PlayOneShotSound("Button Generic", 1f);
		WorldSession.current.EnterScreenCaptureSetup();
	}

	// Token: 0x0600199B RID: 6555 RVA: 0x000BB734 File Offset: 0x000B9B34
	public void ButtonCaptureTapped()
	{
		if (Blocksworld.resettingPlay)
		{
			return;
		}
		if (Blocksworld.rewardVisualizationGafs != null)
		{
			return;
		}
		if (Blocksworld.capturingScreenshot)
		{
			return;
		}
		if (WorldSession.isProfileBuildSession())
		{
			if (CharacterEditor.Instance.InEditMode())
			{
				CharacterEditor.Instance.Exit();
			}
			Sound.PlayOneShotSound("Step", 1f);
			WorldSession.Save();
			this.Play();
			WorldSession.current.OnCapture();
			Blocksworld.fixedUpdateCommands.Add(new DelayedProfilePictureCaptureCommand());
			return;
		}
		if (WorldSession.current.TakeScreenshot())
		{
			Sound.PlayOneShotSound("Step", 1f);
			if (WorldSession.isWorldScreenshotSession())
			{
				WorldSession.current.ExitScreenCaptureSetup();
				WorldSession.Quit();
			}
			else
			{
				Blocksworld.capturingScreenshot = true;
				Blocksworld.UI.Overlay.ShowTimedOnScreenMessage("Added to Photos!", 1.5f);
				CameraFlashEffect cameraFlashEffect = Blocksworld.guiCamera.gameObject.GetComponent<CameraFlashEffect>();
				if (cameraFlashEffect == null)
				{
					cameraFlashEffect = Blocksworld.guiCamera.gameObject.AddComponent<CameraFlashEffect>();
				}
				cameraFlashEffect.time = 0f;
				cameraFlashEffect.autoDestroy = true;
			}
		}
		else
		{
			Sound.PlayOneShotSound("Player Error Retro", 1f);
			Blocksworld.UI.Overlay.ShowTimedOnScreenMessage("Error saving Photo", 1.5f);
		}
	}

	// Token: 0x0600199C RID: 6556 RVA: 0x000BB87F File Offset: 0x000B9C7F
	public void ButtonUndoTapped()
	{
		if (Blocksworld.resettingPlay)
		{
			return;
		}
		if (Blocksworld.CurrentState == State.Build && History.CanUndo())
		{
			Sound.PlayOneShotSound("Button Generic", 1f);
			History.Undo();
		}
	}

	// Token: 0x0600199D RID: 6557 RVA: 0x000BB8B4 File Offset: 0x000B9CB4
	public void ButtonRedoTapped()
	{
		if (Blocksworld.resettingPlay)
		{
			return;
		}
		if (Blocksworld.CurrentState == State.Build && History.CanRedo())
		{
			Sound.PlayOneShotSound("Button Generic", 1f);
			History.Redo();
		}
	}

	// Token: 0x0600199E RID: 6558 RVA: 0x000BB8E9 File Offset: 0x000B9CE9
	public void ButtonLikeToggleTapped()
	{
		Sound.PlayOneShotSound("Button Generic", 1f);
		WorldSession.current.ToggleWorldUpvoted();
	}

	// Token: 0x0600199F RID: 6559 RVA: 0x000BB904 File Offset: 0x000B9D04
	public void ButtonVRCameraTapped()
	{
		Sound.PlayOneShotSound("Button Generic", 1f);
		Blocksworld.SetVRMode(true);
	}

	// Token: 0x060019A0 RID: 6560 RVA: 0x000BB91C File Offset: 0x000B9D1C
	public string ExtractProfileWorldAvatarString()
	{
		Block block = BWSceneManager.AllBlocks().Find((Block b) => b.IsProfileCharacter());
		if (block == null)
		{
			return string.Empty;
		}
		return this.ExtractProfileWorldAvatarString(block);
	}

	// Token: 0x060019A1 RID: 6561 RVA: 0x000BB964 File Offset: 0x000B9D64
	public string ExtractProfileWorldAvatarString(Block profileCharacter)
	{
		StringBuilder stringBuilder = new StringBuilder(32768);
		StringWriter writer = new StringWriter(stringBuilder, CultureInfo.InvariantCulture);
		JSONStreamEncoder jsonstreamEncoder = new JSONStreamEncoder(writer, 20);
		List<Block> list = ConnectednessGraph.ConnectedComponent(profileCharacter, 3, null, true);
		for (int i = list.Count - 1; i >= 0; i--)
		{
			if (list[i] != profileCharacter && Blocksworld.IsSelectionLocked(list[i]))
			{
				list.RemoveAt(i);
			}
		}
		Util.AddGroupedTilesToBlockList(list);
		jsonstreamEncoder.BeginObject();
		jsonstreamEncoder.InsertNewline();
		jsonstreamEncoder.WriteKey("avatar");
		jsonstreamEncoder.BeginArray();
		foreach (Block block in list)
		{
			List<List<Tile>> list2 = block.tiles;
			if (block == profileCharacter)
			{
				list2 = Blocksworld.CloneBlockTiles(block, false, false);
				Tile tile = list2[0].Find((Tile t) => t.gaf.Predicate == Block.predicateCreate);
				string stringArg = Util.GetStringArg(tile.gaf.Args, 0, string.Empty);
				string text = ProfileBlocksterUtils.GetNonProfileBlockType(stringArg);
				if (string.IsNullOrEmpty(text))
				{
					text = ((!(profileCharacter is BlockAnimatedCharacter)) ? "Character Male" : "Anim Character Male");
				}
				tile.gaf.Args[0] = text;
				list2 = new List<List<Tile>>
				{
					list2[0],
					new List<Tile>
					{
						new Tile(new GAF(Block.predicateThen, new object[0]))
					}
				};
			}
			jsonstreamEncoder.InsertNewline();
			jsonstreamEncoder.BeginObject();
			jsonstreamEncoder.WriteKey((!Blocksworld.useCompactGafWriteRenamings) ? "tile-rows" : "r");
			this.WriteTilesAsJSON(jsonstreamEncoder, list2, false, Blocksworld.useCompactGafWriteRenamings);
			jsonstreamEncoder.EndObject();
		}
		jsonstreamEncoder.EndArray();
		jsonstreamEncoder.InsertNewline();
		jsonstreamEncoder.WriteKey("connections");
		jsonstreamEncoder.BeginArray();
		foreach (Block block2 in list)
		{
			jsonstreamEncoder.InsertNewline();
			jsonstreamEncoder.BeginArray();
			foreach (Block item in block2.connections)
			{
				int num = list.IndexOf(item);
				if (num >= 0)
				{
					jsonstreamEncoder.WriteNumber((long)list.IndexOf(item));
				}
			}
			jsonstreamEncoder.EndArray();
		}
		jsonstreamEncoder.InsertNewline();
		jsonstreamEncoder.EndArray();
		jsonstreamEncoder.InsertNewline();
		jsonstreamEncoder.WriteKey("connectionTypes");
		jsonstreamEncoder.BeginArray();
		foreach (Block block3 in list)
		{
			jsonstreamEncoder.InsertNewline();
			jsonstreamEncoder.BeginArray();
			foreach (int num2 in block3.connectionTypes)
			{
				jsonstreamEncoder.WriteNumber((long)num2);
			}
			jsonstreamEncoder.EndArray();
		}
		jsonstreamEncoder.InsertNewline();
		jsonstreamEncoder.EndArray();
		jsonstreamEncoder.InsertNewline();
		jsonstreamEncoder.EndObject();
		jsonstreamEncoder.InsertNewline();
		return stringBuilder.ToString();
	}

	// Token: 0x060019A2 RID: 6562 RVA: 0x000BBD70 File Offset: 0x000BA170
	public void ButtonCopyTapped()
	{
		if (!this.copyModelAnimationCommand.Animating())
		{
			List<Block> list = new List<Block>();
			if (Blocksworld.selectedBlock != null)
			{
				list.Add(Blocksworld.selectedBlock);
			}
			else if (Blocksworld.selectedBunch != null)
			{
				list.AddRange(Blocksworld.selectedBunch.blocks);
			}
			this.copyModelAnimationCommand.SetBlocks(list);
			this.copyModelAnimationCommand.endCommand = this.GetScaleCommand(delegate(UIQuickSelect q, float s)
			{
				q.SetModelIconScale(s);
			});
			Blocksworld.AddFixedUpdateUniqueCommand(this.copyModelAnimationCommand, true);
			Blocksworld.clipboard.SetModelToSelection();
		}
	}

	// Token: 0x060019A3 RID: 6563 RVA: 0x000BBE18 File Offset: 0x000BA218
	public void ButtonSaveModelTapped()
	{
		if (!Blocksworld.modelCollection.CanSaveModels)
		{
			Blocksworld.UI.Dialog.ShowMaximumModelsDialog();
			return;
		}
		List<List<List<Tile>>> buffer = new List<List<List<Tile>>>();
		this.CopySelectionToBuffer(buffer);
		bool useHD = Blocksworld.hd;
		useHD |= (BWStandalone.Instance != null);
		if (ModelCollection.ModelContainsDisallowedTile(buffer))
		{
			BWLog.Error("Attempting to create model with disallowed gaf");
			return;
		}
		Dictionary<GAF, int> gafUsage = Scarcity.GetNormalizedInventoryUse(buffer, WorldType.User, false);
		Action completion = delegate()
		{
			if (!this.saveModelAnimationCommand.Animating())
			{
				List<Block> list = new List<Block>();
				Bounds bounds = new Bounds(Vector3.zero, Vector3.one);
				if (Blocksworld.selectedBlock != null)
				{
					list.Add(Blocksworld.selectedBlock);
					bounds = Blocksworld.selectedBlock.GetBounds();
				}
				else if (Blocksworld.selectedBunch != null)
				{
					list.AddRange(Blocksworld.selectedBunch.blocks);
					bounds = Blocksworld.selectedBunch.GetBounds();
				}
				this.saveModelAnimationCommand.SetBlocks(list);
				Blocksworld.AddFixedUpdateUniqueCommand(this.saveModelAnimationCommand, true);
				Blocksworld.modelCollection.SaveToModelCollection(buffer, gafUsage);
				if (bounds.size.y >= 25f)
				{
					WorldSession.platformDelegate.TrackAchievementIncrease("skyscraper", 1);
				}
			}
		};
		Action completionWithUnsellableBlocksWarning = delegate()
		{
			HashSet<GAF> hashSet = new HashSet<GAF>();
			foreach (GAF gaf in gafUsage.Keys)
			{
				BlockItem blockItem = BlockItem.FindByGafPredicateNameAndArguments(gaf.Predicate.Name, gaf.Args);
				if (blockItem == null)
				{
					BWLog.Info("no blockItem for gaf: " + gaf);
				}
				else if (blockItem.IsUnsellable())
				{
					hashSet.Add(gaf);
				}
			}
			if (hashSet.Count > 0)
			{
				Blocksworld.UI.Dialog.ShowUnsellableBlocksInModelWarning(hashSet, completion);
			}
			else
			{
				completion();
			}
		};
		Action completionWithModelExistWarning = delegate()
		{
			if (Blocksworld.modelCollection.ContainsSimilarModel(buffer))
			{
				Blocksworld.UI.Dialog.ShowGenericDialog("\nYou have a model like this...\n\nSave anyway?", "No", "Yes", null, completionWithUnsellableBlocksWarning);
			}
			else
			{
				completionWithUnsellableBlocksWarning();
			}
		};
		Action<string> setNameAction = delegate(string name)
		{
			Blocksworld.modelCollection.SetTempName(name);
		};
		Action<Texture2D> callback = delegate(Texture2D tex)
		{
			Blocksworld.modelCollection.SetTempSnapshot(tex, useHD);
			Blocksworld.modelCollection.SetTempName(string.Empty);
			Blocksworld.UI.Dialog.ShowModelSaveDialog(tex, completionWithModelExistWarning, setNameAction);
		};
		ScreenshotUtils.GenerateModelSnapshotTexture(buffer, useHD, callback);
	}

	// Token: 0x060019A4 RID: 6564 RVA: 0x000BBF31 File Offset: 0x000BA331
	public bool ModelAnimationInProgress()
	{
		return this.copyModelAnimationCommand.Animating() || this.saveModelAnimationCommand.Animating();
	}

	// Token: 0x060019A5 RID: 6565 RVA: 0x000BBF51 File Offset: 0x000BA351
	public bool IsPullingObject()
	{
		return this.pullObject != null && this.pullObject.IsActive;
	}

	// Token: 0x060019A6 RID: 6566 RVA: 0x000BBF6C File Offset: 0x000BA36C
	public bool HadObjectTapping()
	{
		return Blocksworld.worldSessionHadBlockTap;
	}

	// Token: 0x060019A7 RID: 6567 RVA: 0x000BBF74 File Offset: 0x000BA374
	private DelegateCommand GetScaleCommand(Action<UIQuickSelect, float> scaleFunc)
	{
		UIQuickSelect quickSelect = Blocksworld.UI.QuickSelect;
		if (quickSelect != null)
		{
			float scale = 2f;
			return new DelegateCommand(delegate(DelegateCommand a)
			{
				scaleFunc(quickSelect, scale);
				scale *= 0.85f;
				if (scale < 1f)
				{
					scaleFunc(quickSelect, 1f);
					a.SetDone(true);
				}
				else
				{
					a.SetDone(false);
				}
			});
		}
		return null;
	}

	// Token: 0x060019A8 RID: 6568 RVA: 0x000BBFDC File Offset: 0x000BA3DC
	private void ButtonCopyScriptTapped(TileTapGesture gesture, Tile tile)
	{
		if (!this.copyScriptAnimationCommand.Animating())
		{
			this.copyScriptAnimationCommand.SetTiles(Blocksworld.scriptPanel.tiles);
			Blocksworld.AddFixedUpdateUniqueCommand(this.copyScriptAnimationCommand, true);
			this.copyScriptAnimationCommand.endCommand = this.GetScaleCommand(delegate(UIQuickSelect q, float s)
			{
				q.SetScriptIconScale(s);
			});
			Blocksworld.clipboard.SetScriptToSelection();
		}
	}

	// Token: 0x060019A9 RID: 6569 RVA: 0x000BC052 File Offset: 0x000BA452
	private void ButtonClearScriptTapped(TileTapGesture gesture, Tile tile)
	{
		Blocksworld.RemoveScriptsFromSelection();
		History.AddStateIfNecessary();
		Blocksworld.UpdateTiles();
	}

	// Token: 0x060019AA RID: 6570 RVA: 0x000BC064 File Offset: 0x000BA464
	private void ButtonPasteScriptTapped(TileTapGesture gesture, Tile tile)
	{
		if (Blocksworld.selectedBlock != null)
		{
			this.PasteScriptFromClipboard(Blocksworld.selectedBlock);
			History.AddStateIfNecessary();
			Blocksworld.UpdateTiles();
		}
	}

	// Token: 0x060019AB RID: 6571 RVA: 0x000BC088 File Offset: 0x000BA488
	private void CharacterEditTapped(TileTapGesture gesture, Tile tile)
	{
		if (CharacterEditor.Instance.InEditMode())
		{
			return;
		}
		if (Blocksworld.selectedBlock != null && Blocksworld.selectedBlock is BlockAnimatedCharacter)
		{
			CharacterEditor.Instance.EditCharacter(Blocksworld.selectedBlock as BlockAnimatedCharacter);
			return;
		}
		if (WorldSession.isProfileBuildSession() && WorldSession.current.profileWorldAnimatedBlockster != null)
		{
			CharacterEditor.Instance.EditCharacter(WorldSession.current.profileWorldAnimatedBlockster);
		}
	}

	// Token: 0x060019AC RID: 6572 RVA: 0x000BC100 File Offset: 0x000BA500
	private void CharacterEditExitTapped(TileTapGesture gesture, Tile tile)
	{
		if (CharacterEditor.Instance.InEditMode())
		{
			CharacterEditor.Instance.Exit();
		}
	}

	// Token: 0x060019AD RID: 6573 RVA: 0x000BC11C File Offset: 0x000BA51C
	public static bool CanSelectBlock(Block block)
	{
		bool enableTerrainSelection = Options.EnableTerrainSelection;
		return (!block.isTerrain || enableTerrainSelection || (block.SelectableTerrain() && ((Blocksworld.selectedBlock == null && Blocksworld.selectedBunch == null) || !block.DoubleTapToSelect()))) && (Tutorial.state == TutorialState.None || (!block.IsLocked() && !Blocksworld.locked.Contains(block))) && (!enableTerrainSelection || !block.isTerrain || Blocksworld.selectedBlock == null || Blocksworld.selectedBlock.isTerrain);
	}

	// Token: 0x060019AE RID: 6574 RVA: 0x000BC1C4 File Offset: 0x000BA5C4
	private bool RaycastMoveBlock(Block block)
	{
		int connectionType = 3;
		bool flag = Options.RaycastMoveBlocksWithoutSelection || (Options.RaycastMoveSingletonBlocksWithoutSelection && block.ConnectionsOfType(connectionType, false).Count == 0);
		if (flag)
		{
			Bounds bounds = block.go.GetComponent<Collider>().bounds;
			float magnitude = (bounds.center - Blocksworld.cameraTransform.position).magnitude;
			if (magnitude > 0.01f)
			{
				return Util.MeanAbs(bounds.size / magnitude) < 0.35f;
			}
		}
		return flag;
	}

	// Token: 0x060019AF RID: 6575 RVA: 0x000BC25C File Offset: 0x000BA65C
	private bool BlockDuplicateBegan(BlockDuplicateGesture gesture, Block block)
	{
		bool flag = block == Blocksworld.selectedBlock;
		if (Blocksworld.selectedBunch != null)
		{
			flag = false;
			for (int i = 0; i < Blocksworld.selectedBunch.blocks.Count; i++)
			{
				Block block2 = Blocksworld.selectedBunch.blocks[i];
				flag |= (block2 == block);
			}
		}
		return flag;
	}

	// Token: 0x060019B0 RID: 6576 RVA: 0x000BC2B8 File Offset: 0x000BA6B8
	private static string GetUniquesJSON(List<GAF> uniquesInModel)
	{
		StringBuilder stringBuilder = new StringBuilder();
		StringWriter writer = new StringWriter(stringBuilder, CultureInfo.InvariantCulture);
		JSONStreamEncoder jsonstreamEncoder = new JSONStreamEncoder(writer, 20);
		jsonstreamEncoder.BeginObject();
		jsonstreamEncoder.InsertNewline();
		jsonstreamEncoder.WriteKey("uniques");
		jsonstreamEncoder.BeginArray();
		jsonstreamEncoder.InsertNewline();
		foreach (GAF gaf in uniquesInModel)
		{
			gaf.ToJSON(jsonstreamEncoder);
			jsonstreamEncoder.InsertNewline();
		}
		jsonstreamEncoder.EndArray();
		jsonstreamEncoder.InsertNewline();
		jsonstreamEncoder.EndObject();
		jsonstreamEncoder.InsertNewline();
		return stringBuilder.ToString();
	}

	// Token: 0x060019B1 RID: 6577 RVA: 0x000BC374 File Offset: 0x000BA774
	private void BlockDuplicated(BlockDuplicateGesture gesture, Block block, Gestures.Touch touch)
	{
		this.CopySelectionToBuffer(Blocksworld.clipboard.modelDuplicateBuffer);
		TBox.Show(false);
		TBoxGesture.skipOneTap = true;
		Blocksworld.blockTapGesture.Reset();
		Blocksworld.mouseBlock = null;
		if (ModelCollection.ModelContainsDisallowedTile(Blocksworld.clipboard.modelDuplicateBuffer))
		{
			BWLog.Error("Attempting to create model with disallowed gaf");
			return;
		}
		if (BW.Options.useScarcity())
		{
			Dictionary<GAF, int> missing = new Dictionary<GAF, int>();
			List<GAF> list = new List<GAF>();
			if (Blocksworld.clipboard.AvailablityCountForBlockList(Blocksworld.clipboard.modelDuplicateBuffer, missing, list) == 0)
			{
				Sound.PlayOneShotSound("Destroy", 1f);
				string str = (list.Count != 0) ? "A world can only have one of these:" : "Missing items:";
				Blocksworld.UI.Dialog.ShowPasteFailInfo(missing, list, "\nCould not duplicate selection.\n\n" + str);
				Blocksworld.Deselect(false, true);
				return;
			}
		}
		Sound.PlayOneShotSound("Paste Model", 1f);
		Scarcity.UpdateInventory(true, null);
		List<Block> blocks = this.PasteModelToFinger(Blocksworld.clipboard.modelDuplicateBuffer, touch, true, null);
		if (!this.pasteModelAnimationCommand.Animating())
		{
			this.pasteModelAnimationCommand.SetBlocks(blocks);
			Blocksworld.AddFixedUpdateUniqueCommand(this.pasteModelAnimationCommand, true);
		}
	}

	// Token: 0x060019B2 RID: 6578 RVA: 0x000BC4A8 File Offset: 0x000BA8A8
	private bool HasEnoughScarcityForSelection()
	{
		if (Scarcity.inventory == null)
		{
			return true;
		}
		List<Block> list = new List<Block>();
		if (Blocksworld.selectedBunch != null)
		{
			for (int i = 0; i < Blocksworld.selectedBunch.blocks.Count; i++)
			{
				list.Add(Blocksworld.selectedBunch.blocks[i]);
			}
		}
		else if (Blocksworld.selectedBlock != null)
		{
			list.Add(Blocksworld.selectedBlock);
		}
		Dictionary<GAF, int> dictionary = new Dictionary<GAF, int>();
		for (int j = 0; j < list.Count; j++)
		{
			Block block = list[j];
			for (int k = 0; k < block.tiles[0].Count; k++)
			{
				Tile tile = block.tiles[0][k];
				GAF normalizedGaf = Scarcity.GetNormalizedGaf(tile.gaf, false);
				if (Scarcity.IsRelevantGAF(block.BlockType(), normalizedGaf, true))
				{
					if (dictionary.ContainsKey(normalizedGaf))
					{
						Dictionary<GAF, int> dictionary2;
						GAF key;
						(dictionary2 = dictionary)[key = normalizedGaf] = dictionary2[key] + 1;
					}
					else
					{
						dictionary.Add(normalizedGaf, 1);
					}
				}
			}
			foreach (List<Tile> list2 in block.tiles)
			{
				foreach (Tile tile2 in list2)
				{
					GAF gaf = tile2.gaf;
					if (gaf.Predicate == Block.predicatePlaySoundDurational)
					{
						if (dictionary.ContainsKey(gaf))
						{
							Dictionary<GAF, int> dictionary2;
							GAF key2;
							(dictionary2 = dictionary)[key2 = gaf] = dictionary2[key2] + 1;
						}
						else
						{
							dictionary.Add(gaf, 1);
						}
					}
				}
			}
		}
		bool flag = true;
		foreach (GAF gaf2 in dictionary.Keys)
		{
			flag &= (Scarcity.GetInventoryCount(gaf2, false) >= dictionary[gaf2]);
		}
		return flag;
	}

	// Token: 0x060019B3 RID: 6579 RVA: 0x000BC71C File Offset: 0x000BAB1C
	public void CopyBlocksToBuffer(List<Block> list, List<List<List<Tile>>> modelBuffer)
	{
		HashSet<Block> hashSet = new HashSet<Block>();
		modelBuffer.Clear();
		for (int i = 0; i < list.Count; i++)
		{
			Block block = list[i];
			if (!hashSet.Contains(block))
			{
				BlockGrouped blockGrouped = block as BlockGrouped;
				if (blockGrouped != null && blockGrouped.group != null)
				{
					foreach (Block block2 in blockGrouped.group.GetBlocks())
					{
						if (!hashSet.Contains(block2))
						{
							hashSet.Add(block2);
							modelBuffer.Add(Blocksworld.CloneBlockTiles(block2, false, false));
						}
					}
				}
				else
				{
					modelBuffer.Add(Blocksworld.CloneBlockTiles(block, false, false));
					hashSet.Add(block);
				}
			}
		}
	}

	// Token: 0x060019B4 RID: 6580 RVA: 0x000BC7EC File Offset: 0x000BABEC
	public void CopySelectionToBuffer(List<List<List<Tile>>> modelBuffer)
	{
		if (Blocksworld.selectedBunch != null)
		{
			this.CopyBlocksToBuffer(Blocksworld.selectedBunch.blocks, modelBuffer);
		}
		else if (Blocksworld.selectedBlock != null)
		{
			BlockGrouped blockGrouped = Blocksworld.selectedBlock as BlockGrouped;
			if (blockGrouped != null && blockGrouped.group != null)
			{
				this.CopyBlocksToBuffer(blockGrouped.group.GetBlockList(), modelBuffer);
			}
			else
			{
				this.CopyBlocksToBuffer(new List<Block>
				{
					Blocksworld.selectedBlock
				}, modelBuffer);
			}
		}
	}

	// Token: 0x060019B5 RID: 6581 RVA: 0x000BC870 File Offset: 0x000BAC70
	public void CopySelectedScriptToBuffer(List<List<List<Tile>>> scriptBuffer)
	{
		Block selectedScriptBlock = Blocksworld.GetSelectedScriptBlock();
		if (selectedScriptBlock != null)
		{
			scriptBuffer.Clear();
			scriptBuffer.Add(Blocksworld.CloneBlockTiles(selectedScriptBlock, true, false));
		}
	}

	// Token: 0x060019B6 RID: 6582 RVA: 0x000BC8A0 File Offset: 0x000BACA0
	public void PasteScriptFromClipboard(Block block)
	{
		if (block.tiles.Count > 2 || (block.tiles.Count > 1 && block.tiles[1].Count > 1))
		{
			Blocksworld.UI.Dialog.ShowScriptExistsDialog(block);
		}
		else
		{
			Blocksworld.DoPasteScriptFromClipboard(block, true);
		}
	}

	// Token: 0x060019B7 RID: 6583 RVA: 0x000BC902 File Offset: 0x000BAD02
	public static void DoPasteScriptFromClipboard(Block block, bool replace = false)
	{
		if (Blocksworld.clipboard.scriptCopyPasteBuffer.Count > 0)
		{
			Blocksworld.PasteScript(block, Blocksworld.clipboard.scriptCopyPasteBuffer[0], replace, false);
		}
	}

	// Token: 0x060019B8 RID: 6584 RVA: 0x000BC934 File Offset: 0x000BAD34
	public void PasteBlocksFromSavedModel(int modelIndex)
	{
		if (modelIndex >= Blocksworld.modelCollection.models.Count)
		{
			return;
		}
		List<List<List<Tile>>> list = Blocksworld.modelCollection.models[modelIndex].CreateModel();
		if (ModelCollection.ModelContainsDisallowedTile(list))
		{
			BWLog.Error("Attempting to create model with disallowed gaf");
			return;
		}
		List<Block> blocksToUpdateConnection = this.PasteBlocks(list, Util.Round(Blocksworld.blocksworldCamera.GetTargetPosition()));
		this.DetermineBlockOffset(blocksToUpdateConnection);
	}

	// Token: 0x060019B9 RID: 6585 RVA: 0x000BC9A4 File Offset: 0x000BADA4
	private static List<List<Tile>> FilterCompatible(List<List<Tile>> script, Block block, HashSet<GAF> incompatible)
	{
		List<List<Tile>> list = new List<List<Tile>>();
		foreach (List<Tile> list2 in script)
		{
			List<Tile> list3 = new List<Tile>();
			foreach (Tile tile in list2)
			{
				Predicate predicate = tile.gaf.Predicate;
				if (predicate.CompatibleWith(block))
				{
					list3.Add(tile);
				}
				else
				{
					bool flag = true;
					HashSet<Predicate> equivalentPredicates = PredicateRegistry.GetEquivalentPredicates(predicate);
					if (equivalentPredicates != null)
					{
						List<Predicate> list4 = PredicateRegistry.ForBlock(block, false);
						foreach (Predicate predicate2 in list4)
						{
							if (equivalentPredicates.Contains(predicate2))
							{
								object[] args = PredicateRegistry.ConvertEquivalentPredicateArguments(predicate, predicate2, tile.gaf.Args);
								GAF gaf = new GAF(predicate2, args, true);
								Tile item = new Tile(gaf);
								list3.Add(item);
								flag = false;
								break;
							}
						}
					}
					if (flag)
					{
						incompatible.Add(tile.gaf);
					}
				}
			}
			list.Add(list3);
		}
		return list;
	}

	// Token: 0x060019BA RID: 6586 RVA: 0x000BCB54 File Offset: 0x000BAF54
	public static void RemoveScriptsFromSelection()
	{
		if (Blocksworld.scriptPanel.IsShowing())
		{
			Blocksworld.scriptPanel.SavePositionForNextLayout();
		}
		BlockGrouped blockGrouped = Blocksworld.selectedBlock as BlockGrouped;
		if (blockGrouped != null)
		{
			foreach (Block block in blockGrouped.group.GetBlocks())
			{
				Blocksworld.RemoveScriptFrom(block);
				block.AddOrRemoveEmptyScriptLine();
			}
		}
		else if (Blocksworld.selectedBunch != null)
		{
			for (int j = 0; j < Blocksworld.selectedBunch.blocks.Count; j++)
			{
				Blocksworld.RemoveScriptFrom(Blocksworld.selectedBunch.blocks[j]);
				Blocksworld.selectedBunch.blocks[j].AddOrRemoveEmptyScriptLine();
			}
		}
		else if (Blocksworld.selectedBlock != null)
		{
			Blocksworld.RemoveScriptFrom(Blocksworld.selectedBlock);
			Blocksworld.selectedBlock.AddOrRemoveEmptyScriptLine();
		}
		Blocksworld.scriptPanel.AssignUnparentedTiles();
		if (Blocksworld.scriptPanel.IsShowing())
		{
			Blocksworld.scriptPanel.Layout();
		}
	}

	// Token: 0x060019BB RID: 6587 RVA: 0x000BCC64 File Offset: 0x000BB064
	public static void RemoveScriptFrom(Block block)
	{
		for (int i = 1; i < block.tiles.Count; i++)
		{
			List<Tile> list = block.tiles[i];
			foreach (Tile tile in list)
			{
				tile.Show(false);
			}
		}
		block.tiles.RemoveRange(1, block.tiles.Count - 1);
	}

	// Token: 0x060019BC RID: 6588 RVA: 0x000BCD00 File Offset: 0x000BB100
	public static void PasteScript(Block block, List<List<Tile>> script, bool replace = false, bool force = false)
	{
		HashSet<GAF> hashSet = new HashSet<GAF>();
		List<List<Tile>> collection = Blocksworld.FilterCompatible(Blocksworld.CloneBlockTiles(script, null, false, false), block, hashSet);
		if (!force && hashSet.Count > 0)
		{
			Blocksworld.UI.Dialog.ShowPasteScriptIncompatibleDialog(block, script, new List<GAF>(hashSet), replace);
			return;
		}
		if (replace)
		{
			Blocksworld.RemoveScriptFrom(block);
		}
		bool flag = Blocksworld.scriptPanel.IsShowing();
		if (flag)
		{
			Blocksworld.scriptPanel.SavePositionForNextLayout();
			Blocksworld.Deselect(true, true);
		}
		block.tiles.AddRange(collection);
		for (int i = block.tiles.Count - 1; i >= 0; i--)
		{
			if (i < block.tiles.Count - 1)
			{
				List<Tile> list = block.tiles[i];
				if (list.Count <= 1)
				{
					block.tiles.RemoveAt(i);
				}
			}
		}
		if (flag)
		{
			Blocksworld.SelectBlock(block, true, true);
		}
		Scarcity.UpdateInventory(false, null);
		Scarcity.PaintScarcityBadges();
		Sound.PlayOneShotSound("Paste Script", 1f);
	}

	// Token: 0x060019BD RID: 6589 RVA: 0x000BCE0C File Offset: 0x000BB20C
	private List<Block> PasteBlocks(List<List<List<Tile>>> blocks, Vector3 targetPos)
	{
		List<Block> list = new List<Block>();
		foreach (List<List<Tile>> tiles in blocks)
		{
			List<List<Tile>> tiles2 = Blocksworld.CloneBlockTiles(tiles, null, false, false);
			Block block = Block.NewBlock(tiles2, false, false);
			BWSceneManager.AddBlock(block);
			list.Add(block);
		}
		Vector3 b = Util.Round(Util.ComputeCenter(list, false));
		foreach (Block block2 in list)
		{
			Vector3 b2 = block2.GetPosition() - b;
			block2.MoveTo(targetPos + b2);
		}
		return list;
	}

	// Token: 0x060019BE RID: 6590 RVA: 0x000BCEF8 File Offset: 0x000BB2F8
	private void DetermineBlockOffset(List<Block> blocksToUpdateConnection)
	{
		for (int i = 0; i < 10; i++)
		{
			Vector3 b = default(Vector3);
			bool flag = true;
			foreach (Block block in blocksToUpdateConnection)
			{
				if (block.IsColliding(0f, null))
				{
					b = Vector3.up * 2f;
					flag = false;
					break;
				}
			}
			if (flag)
			{
				b = Vector3.up;
			}
			foreach (Block block2 in blocksToUpdateConnection)
			{
				block2.MoveTo(block2.GetPosition() + b);
			}
			if (flag)
			{
				break;
			}
		}
	}

	// Token: 0x060019BF RID: 6591 RVA: 0x000BCFFC File Offset: 0x000BB3FC
	public static List<Block> InsertModelTiles(List<List<List<Tile>>> modelBuffer, float x, float y, float z)
	{
		List<Block> list = new List<Block>();
		foreach (List<List<Tile>> tiles in modelBuffer)
		{
			Block block = Block.NewBlock(tiles, false, false);
			if (block != null)
			{
				list.Add(block);
				BWSceneManager.AddBlock(block);
			}
		}
		Bounds bounds = Util.ComputeBoundsWithSize(list, true);
		float f = x - bounds.center.x;
		float f2 = y - bounds.min.y;
		float f3 = z - bounds.center.z;
		Vector3 b = new Vector3(Mathf.Round(f), Mathf.Ceil(f2), Mathf.Round(f3));
		foreach (Block block2 in list)
		{
			block2.MoveTo(block2.GetPosition() + b);
		}
		BlockGroups.GatherBlockGroups(list);
		foreach (Block block3 in list)
		{
			BlockGrouped blockGrouped = block3 as BlockGrouped;
			if (blockGrouped != null)
			{
				blockGrouped.UpdateSATVolumes();
			}
		}
		Scarcity.UpdateInventory(true, null);
		return list;
	}

	// Token: 0x060019C0 RID: 6592 RVA: 0x000BD194 File Offset: 0x000BB594
	public static List<Block> InsertModel(string modelSource, float x, float y, float z)
	{
		Block.ClearConnectedCache();
		RaycastHit raycastHit;
		if (Physics.Raycast(new Vector3(x, y, z), -Vector3.up, out raycastHit, 100f))
		{
			y = raycastHit.point.y + y;
		}
		List<List<List<Tile>>> list = ModelUtils.ParseModelString(modelSource);
		if (list == null)
		{
			return null;
		}
		List<Block> list2 = Blocksworld.InsertModelTiles(list, x, y, z);
		ConnectednessGraph.Update(list2);
		return list2;
	}

	// Token: 0x060019C1 RID: 6593 RVA: 0x000BD1FC File Offset: 0x000BB5FC
	public static string LockModelJSON(string modelStr)
	{
		JObject obj = JSONDecoder.Decode(modelStr);
		List<List<List<Tile>>> modelBuffer = ModelUtils.ParseModelJSON(obj);
		return Blocksworld.LockModelTiles(modelBuffer);
	}

	// Token: 0x060019C2 RID: 6594 RVA: 0x000BD220 File Offset: 0x000BB620
	public static string UnlockModelJSON(string modelStr)
	{
		JObject obj = JSONDecoder.Decode(modelStr);
		List<List<List<Tile>>> modelBuffer = ModelUtils.ParseModelJSON(obj);
		return Blocksworld.UnlockModelTiles(modelBuffer);
	}

	// Token: 0x060019C3 RID: 6595 RVA: 0x000BD244 File Offset: 0x000BB644
	public static string UnlockModelTiles(List<List<List<Tile>>> modelBuffer)
	{
		for (int i = 0; i < modelBuffer.Count; i++)
		{
			List<List<Tile>> list = modelBuffer[i];
			List<Tile> list2 = list[0];
			for (int j = list2.Count - 1; j >= 0; j--)
			{
				Tile tile = list2[j];
				if (tile.gaf.Predicate == Block.predicateGroup && BlockGroups.GroupType(tile) == "locked-model")
				{
					list2.RemoveAt(j);
				}
			}
		}
		return ModelUtils.GetJSONForModel(modelBuffer);
	}

	// Token: 0x060019C4 RID: 6596 RVA: 0x000BD2D4 File Offset: 0x000BB6D4
	public static string LockModelTiles(List<List<List<Tile>>> modelBuffer)
	{
		int nextGroupId = BlockGroup.GetNextGroupId();
		for (int i = 0; i < modelBuffer.Count; i++)
		{
			List<List<Tile>> tiles = modelBuffer[i];
			BlockGroups.SetGroup(tiles, nextGroupId, "locked-model", i == 0);
		}
		return ModelUtils.GetJSONForModel(modelBuffer);
	}

	// Token: 0x060019C5 RID: 6597 RVA: 0x000BD31C File Offset: 0x000BB71C
	public List<Block> PasteModelToFinger(List<List<List<Tile>>> model, Gestures.Touch touch, bool addHistoryState = true, BlockGroupTemplate template = null)
	{
		if (model == null || model.Count == 0)
		{
			return new List<Block>();
		}
		Blocksworld.Deselect(false, true);
		if (ModelCollection.ModelContainsDisallowedTile(model))
		{
			BWLog.Error("Attempting to create model with disallowed gaf");
			return new List<Block>();
		}
		List<Block> list = new List<Block>();
		Vector3 a = Util.Round(Blocksworld.blocksworldCamera.GetTargetPosition());
		foreach (List<List<Tile>> tiles in model)
		{
			List<List<Tile>> tiles2 = Blocksworld.CloneBlockTiles(tiles, null, false, false);
			Block block = Block.NewBlock(tiles2, false, false);
			BWSceneManager.AddBlock(block);
			list.Add(block);
		}
		Vector3 b = Util.Round(Util.ComputeCenter(list, false));
		Vector3 b2 = Util.Round((Blocksworld.cameraTransform.position - b).normalized);
		foreach (Block block2 in list)
		{
			Vector3 b3 = block2.GetPosition() - b;
			block2.MoveTo(a + b3 + b2);
		}
		for (int i = 0; i < 10; i++)
		{
			Vector3 vector = Vector3.zero;
			bool flag = true;
			foreach (Block block3 in list)
			{
				if (block3.IsColliding(0f, null))
				{
					flag = false;
					break;
				}
			}
			if (!flag)
			{
				vector += Vector3.up;
			}
			foreach (Block block4 in list)
			{
				block4.MoveTo(block4.GetPosition() + vector);
			}
			if (flag)
			{
				break;
			}
		}
		BlockGroups.GatherBlockGroups(list);
		if (template != null)
		{
			bool flag2 = true;
			bool flag3 = false;
			foreach (Block block5 in list)
			{
				BlockGrouped blockGrouped = block5 as BlockGrouped;
				if (blockGrouped == null || blockGrouped.group == null)
				{
					flag3 = true;
					flag2 = blockGrouped.BlockUsesDefaultPaintsAndTextures();
					break;
				}
			}
			if (flag3)
			{
				BlockGroups.AddGroup(list, template.type);
			}
			if (list.Count > 0)
			{
				BlockGrouped blockGrouped2 = list[0] as BlockGrouped;
				if (blockGrouped2 != null)
				{
					Block mainBlockInGroup = blockGrouped2.GetMainBlockInGroup();
					Block.WriteDefaultExtraTiles(Tutorial.state == TutorialState.None, mainBlockInGroup.tiles, mainBlockInGroup.BlockType());
				}
			}
			if (flag2)
			{
				Blocksworld.SetDefaultPaintsAndTextures(list);
			}
		}
		Scarcity.UpdateInventory(true, null);
		if (addHistoryState)
		{
			History.AddStateIfNecessary();
		}
		Blocksworld.selectedBlock = null;
		Blocksworld.SelectBunch(list, true, true);
		Block block6 = Blocksworld.selectedBlock;
		if (block6 == null && Blocksworld.selectedBunch != null && Blocksworld.selectedBunch.blocks.Count == 1)
		{
			block6 = Blocksworld.selectedBunch.blocks[0];
		}
		if (block6 != null)
		{
			Sound.PlayCreateSound(new GAF(Block.predicateCreate, new object[]
			{
				block6.BlockType()
			}), false, block6);
		}
		else
		{
			Sound.PlayOneShotSound("Create", 1f);
		}
		return list;
	}

	// Token: 0x060019C6 RID: 6598 RVA: 0x000BD6F0 File Offset: 0x000BBAF0
	public static void SetDefaultPaintsAndTextures(List<Block> list)
	{
		foreach (Block block in new List<Block>(list))
		{
			for (int i = 0; i < block.subMeshGameObjects.Count + 1; i++)
			{
				string paint = block.GetDefaultPaint(i).Split(new char[]
				{
					','
				})[0];
				block.PaintTo(paint, true, i);
				block.TextureTo(block.GetDefaultTexture(i), Vector3.zero, true, i, true);
			}
		}
	}

	// Token: 0x060019C7 RID: 6599 RVA: 0x000BD7A0 File Offset: 0x000BBBA0
	private bool BlockTapBegan(BlockTapGesture gesture, Block block)
	{
		if (this.RaycastMoveBlock(block) && block != Blocksworld.selectedBlock && Blocksworld.selectedBunch == null && !TBox.tileButtonRotate.HitExtended(Blocksworld.touches[0], -10f, -10f, -10f, 10f, false) && !TBox.tileButtonMove.HitExtended(Blocksworld.touches[0], -10f, -10f, -10f, 10f, false) && !TBox.tileButtonScale.HitExtended(Blocksworld.touches[0], -10f, -10f, -10f, 10f, false) && !TBox.tileCharacterEditIcon.HitExtended(Blocksworld.touches[0], -10f, -10f, -10f, 10f, false) && !TBox.tileCharacterEditExitIcon.HitExtended(Blocksworld.touches[0], -10f, -10f, -10f, 10f, false) && Blocksworld.CanSelectBlock(block))
		{
			Blocksworld.Select(block, false, true);
			TBox.Show(false);
			TBoxGesture.skipOneTap = true;
			return false;
		}
		return true;
	}

	// Token: 0x060019C8 RID: 6600 RVA: 0x000BD8F4 File Offset: 0x000BBCF4
	private void BlockTapped(BlockTapGesture gesture, Block block)
	{
		bool flag = Blocksworld.CanSelectBlock(block);
		if (this.RaycastMoveBlock(block) || gesture.GetStartMouseBlock() == Blocksworld.mouseBlock || !flag)
		{
			Blocksworld.Select((!flag) ? null : block, false, true);
		}
		Tutorial.Step();
	}

	// Token: 0x060019C9 RID: 6601 RVA: 0x000BD944 File Offset: 0x000BBD44
	public static void ZeroInventoryTileTapped(Tile zeroTile)
	{
		float magnitude = Blocksworld.buildPanel.GetSpeed().magnitude;
		if (!Tutorial.InTutorialOrPuzzle() && magnitude < 5f)
		{
			if (zeroTile.IsCreateModel())
			{
				Dictionary<GAF, int> missing = new Dictionary<GAF, int>();
				List<GAF> list = new List<GAF>();
				ModelData modelData = Blocksworld.modelCollection.models[(int)zeroTile.gaf.Args[0]];
				if (Blocksworld.clipboard.AvailableModelCount(modelData, missing, list, true) == 0)
				{
					string str = (list.Count <= 0) ? "Missing items:" : "A world can only have one of these:";
					string caption = "\nCould not create model " + modelData.name + "\n\n" + str;
					Blocksworld.UI.Dialog.ShowPasteFailInfo(missing, list, caption);
				}
			}
			else
			{
				Dictionary<string, HashSet<string>> dictionary = Blocksworld.GetUniqueBlockMap();
				string stringArgSafe = Util.GetStringArgSafe(zeroTile.gaf.Args, 0, string.Empty);
				if (dictionary.ContainsKey(stringArgSafe))
				{
					Block block = BWSceneManager.FindBlockOfType(stringArgSafe);
					if (block != null)
					{
						Vector3 position = block.GetPosition();
						Vector3 scale = block.Scale();
						Blocksworld.GoToCameraFrameFor(position, scale);
						Blocksworld.SelectBlock(block, true, false);
					}
				}
				else
				{
					Blocksworld.UI.Dialog.ShowZeroInventoryDialog(zeroTile);
				}
			}
		}
	}

	// Token: 0x060019CA RID: 6602 RVA: 0x000BDA90 File Offset: 0x000BBE90
	public static void BlockPanelTileTapped(Tile tile)
	{
		if (Blocksworld.CurrentState == State.Build)
		{
			Predicate predicate = tile.gaf.Predicate;
			if (predicate.CanEditTile(tile))
			{
				if (!predicate.EditableParameter.settings.hideOnLeftSide || !Blocksworld.scriptPanel.TileOnLeftSide(tile))
				{
					if (Blocksworld.bw.tileParameterEditor.IsEditing() && tile.subParameterCount == 1)
					{
						tile.doubleWidth = false;
						Blocksworld.scriptPanel.SavePositionForNextLayout();
						Blocksworld.scriptPanel.Layout();
						Blocksworld.bw.tileParameterEditor.StopEditing();
					}
					else
					{
						tile.doubleWidth = predicate.EditableParameter.useDoubleWidth;
						Blocksworld.scriptPanel.SavePositionForNextLayout();
						Blocksworld.scriptPanel.Layout();
						Blocksworld.bw.tileParameterEditor.StartEditing(tile, predicate.EditableParameter);
					}
				}
			}
		}
		Blocksworld.selectedTile = tile;
	}

	// Token: 0x060019CB RID: 6603 RVA: 0x000BDB79 File Offset: 0x000BBF79
	public static GUISkin LoadSkin(bool hd)
	{
		return (GUISkin)Resources.Load((!hd) ? "GUI/Skin SD" : "GUI/Skin HD");
	}

	// Token: 0x060019CC RID: 6604 RVA: 0x000BDB9A File Offset: 0x000BBF9A
	public float CalcSDLabelHeight(string text, float width)
	{
		return Blocksworld.skin.label.CalcHeight(new GUIContent(text), width * NormalizedScreen.scale) / NormalizedScreen.scale;
	}

	// Token: 0x060019CD RID: 6605 RVA: 0x000BDBC0 File Offset: 0x000BBFC0
	public static void LoadBlocksworldSceneAsync()
	{
		if (!Blocksworld.isLoadingScene && !Blocksworld.loadComplete)
		{
			Blocksworld.isLoadingScene = true;
			GameObject gameObject = new GameObject();
			gameObject.AddComponent<BlocksworldLoader>();
		}
		else if (Blocksworld.loadComplete)
		{
			IOSInterface.BlocksworldSceneLoaded();
		}
	}

	// Token: 0x060019CE RID: 6606 RVA: 0x000BDC08 File Offset: 0x000BC008
	public static void LoadBlocksworldSceneSync()
	{
		if (!Blocksworld.loadComplete)
		{
			Application.LoadLevel("Scene");
		}
	}

	// Token: 0x060019CF RID: 6607 RVA: 0x000BDC20 File Offset: 0x000BC020
	public static void SetSpeechBubbleText(string text, int index)
	{
		TileParameterEditor tileParameterEditor = Blocksworld.bw.tileParameterEditor;
		if (Blocksworld.mainCamera == null || tileParameterEditor == null || tileParameterEditor.parameter == null || Blocksworld.selectedTile == null || text == null)
		{
			BWLog.Warning(string.Concat(new object[]
			{
				"Problem in SetSpeechBubbleText(). Param editor null: ",
				tileParameterEditor == null,
				" Selected tile null: ",
				Blocksworld.selectedTile == null,
				" Text null: ",
				text == null
			}));
		}
		else
		{
			tileParameterEditor.SetEditing(true);
			tileParameterEditor.selectedTile = Blocksworld.selectedTile;
			tileParameterEditor.parameter.objectValue = text;
		}
		if (tileParameterEditor != null)
		{
			tileParameterEditor.StopEditing();
		}
		Blocksworld.selectedTile = null;
		Blocksworld.stringInput = null;
		if (Blocksworld.CurrentState != State.WaitForOption)
		{
			Blocksworld.SetBlocksworldState(State.Build);
		}
	}

	// Token: 0x060019D0 RID: 6608 RVA: 0x000BDD14 File Offset: 0x000BC114
	private static void SetTileParameterValue(object value, int index, bool last)
	{
		if (Blocksworld.mainCamera == null)
		{
			return;
		}
		Blocksworld.selectedTile.gaf.Args[index] = value;
		if (last)
		{
			Blocksworld.selectedTile = null;
			Blocksworld.SetBlocksworldState(State.Build);
		}
	}

	// Token: 0x060019D1 RID: 6609 RVA: 0x000BDD4B File Offset: 0x000BC14B
	public static void EnableWorldSave(bool enabled)
	{
		Blocksworld.worldSaveEnabled = enabled;
	}

	// Token: 0x060019D2 RID: 6610 RVA: 0x000BDD53 File Offset: 0x000BC153
	public static void DisableBuildMode()
	{
		Blocksworld.bw.forcePlayMode = true;
		Blocksworld.buildPanel.showShopButton = false;
		Blocksworld.UI.SidePanel.Hide();
	}

	// Token: 0x060019D3 RID: 6611 RVA: 0x000BDD7A File Offset: 0x000BC17A
	public static void EnableBuildMode()
	{
		Blocksworld.bw.forcePlayMode = false;
		Blocksworld.buildPanel.showShopButton = false;
		Blocksworld.UI.SidePanel.Show();
	}

	// Token: 0x060019D4 RID: 6612 RVA: 0x000BDDA4 File Offset: 0x000BC1A4
	public static void ForceSave()
	{
		if (WorldSession.current == null)
		{
			return;
		}
		if (Blocksworld.CurrentState == State.Paused)
		{
			WorldSession.UnpauseCurrentSession();
		}
		if (Blocksworld.CurrentState == State.Play)
		{
			Blocksworld.bw.Stop(false, true);
		}
		if (Blocksworld.CurrentState == State.Build)
		{
			Blocksworld.bw.Save();
		}
	}

	// Token: 0x060019D5 RID: 6613 RVA: 0x000BDDF8 File Offset: 0x000BC1F8
	private static List<GAF> GetImplicitGAFs(List<GAF> gafList)
	{
		List<GAF> list = new List<GAF>();
		HashSet<GAF> hashSet = new HashSet<GAF>(gafList);
		HashSet<Type> hashSet2 = new HashSet<Type>();
		foreach (GAF gaf in gafList)
		{
			Type item = (gaf.Predicate != Block.predicateCreate) ? PredicateRegistry.GetTypeForPredicate(gaf.Predicate) : Block.GetBlockTypeFromName((string)gaf.Args[0]);
			hashSet2.Add(item);
		}
		foreach (Type type in hashSet2)
		{
			List<GAF> implicitlyUnlockedGAFs = Scarcity.GetImplicitlyUnlockedGAFs(type);
			foreach (GAF item2 in implicitlyUnlockedGAFs)
			{
				if (!hashSet.Contains(item2))
				{
					list.Add(item2);
					hashSet.Add(item2);
				}
			}
		}
		return list;
	}

	// Token: 0x060019D6 RID: 6614 RVA: 0x000BDF44 File Offset: 0x000BC344
	public static void LoadGAFUnlockData(BlocksInventory blocksInventory)
	{
		Blocksworld.unlockedGAFs = new List<GAF>();
		Blocksworld.unlockedPaints.Clear();
		for (int i = 0; i < blocksInventory.BlockItemIds.Count; i++)
		{
			int num = blocksInventory.BlockItemIds[i];
			if (BlockItem.Exists(num))
			{
				BlockItem blockItem = BlockItem.FindByID(num);
				if (blockItem == null)
				{
					BWLog.Error("Unknown blockId: " + num);
				}
				else
				{
					GAF gaf = new GAF(blockItem);
					Blocksworld.unlockedGAFs.Add(gaf);
					Predicate predicate = gaf.Predicate;
					if (predicate == Block.predicateTextureTo)
					{
						string textureName = (string)gaf.Args[0];
						Blocksworld.AddToPublicProvidedTextures(textureName);
					}
					else if (predicate == Block.predicatePaintTo)
					{
						Blocksworld.unlockedPaints.Add((string)gaf.Args[0]);
					}
				}
			}
		}
		List<GAF> implicitGAFs = Blocksworld.GetImplicitGAFs(Blocksworld.unlockedGAFs);
		Blocksworld.unlockedGAFs.AddRange(implicitGAFs);
	}

	// Token: 0x060019D7 RID: 6615 RVA: 0x000BE042 File Offset: 0x000BC442
	public static void AddToPublicProvidedTextures(string textureName)
	{
		Blocksworld.publicProvidedGafs.Add(new GAF("Block.TextureTo", new object[]
		{
			textureName,
			Vector3.zero
		}));
	}

	// Token: 0x060019D8 RID: 6616 RVA: 0x000BE070 File Offset: 0x000BC470
	public static void SetBuildPanelRightSided()
	{
		Blocksworld.buildPanel.UpdatePosition();
		Blocksworld.UpdateTiles();
		Blocksworld.buildPanel.PositionReset(false);
		Blocksworld.UI.SidePanel.Show();
		Blocksworld.scriptPanel.PositionReset();
	}

	// Token: 0x060019D9 RID: 6617 RVA: 0x000BE0A5 File Offset: 0x000BC4A5
	public static void SetBackgroundMusic(string name)
	{
		Blocksworld.musicPlayer.SetMusic(name, 0.4f);
	}

	// Token: 0x060019DA RID: 6618 RVA: 0x000BE0B7 File Offset: 0x000BC4B7
	public static void StopBackgroundMusic()
	{
		Blocksworld.musicPlayer.Stop();
	}

	// Token: 0x060019DB RID: 6619 RVA: 0x000BE0C3 File Offset: 0x000BC4C3
	public static void SetBackgroundMusicVolumeMultiplier(float m)
	{
		Blocksworld.musicPlayer.SetVolumeMultiplier(m);
	}

	// Token: 0x060019DC RID: 6620 RVA: 0x000BE0D0 File Offset: 0x000BC4D0
	public static void SetMusicEnabled(bool enabled)
	{
		Blocksworld.musicPlayer.SetEnabled(enabled);
	}

	// Token: 0x060019DD RID: 6621 RVA: 0x000BE0E0 File Offset: 0x000BC4E0
	public static void UpdateLightColor(bool updateFog = true)
	{
		if (!Blocksworld.renderingShadows)
		{
			List<Block> list = BWSceneManager.AllBlocks();
			Color color = Color.white;
			for (int i = 0; i < list.Count; i++)
			{
				Block block = list[i];
				Color lightTint = block.GetLightTint();
				color *= lightTint;
			}
			for (int j = 0; j < list.Count; j++)
			{
				Block block2 = list[j];
				Color emissiveLightTint = block2.GetEmissiveLightTint();
				color += emissiveLightTint;
			}
			color *= Blocksworld.blocksworldCamera.GetLightTint();
			Blocksworld.lightColor = color;
			Blocksworld.directionalLight.GetComponent<Light>().color = color;
			if (updateFog && Blocksworld.worldSky != null)
			{
				Blocksworld.UpdateFogColor(BlockSky.GetFogColor());
			}
		}
	}

	// Token: 0x060019DE RID: 6622 RVA: 0x000BE1B0 File Offset: 0x000BC5B0
	public static void DefineTestRewardModel(string modelName, List<List<List<Tile>>> model)
	{
		StringBuilder stringBuilder = new StringBuilder(32768);
		StringWriter writer = new StringWriter(stringBuilder, CultureInfo.InvariantCulture);
		JSONStreamEncoder encoder = new JSONStreamEncoder(writer, 20);
		ModelUtils.WriteJSONForModel(encoder, model);
		string text = stringBuilder.ToString();
		Blocksworld.DefineRewardModel("Model 1", text);
		if (Options.ExportRewardModelFromClipboard)
		{
			string text2 = Application.dataPath + "/../Models";
			if (!Directory.Exists(text2))
			{
				Directory.CreateDirectory(text2);
				BWLog.Info("Created directory '" + text2 + "'");
			}
			for (int i = 0; i < 500; i++)
			{
				string text3 = string.Concat(new object[]
				{
					text2,
					"/Model ",
					i + 1,
					".txt"
				});
				string path = string.Concat(new object[]
				{
					text2,
					"/Model ",
					i + 1,
					" Inventory.txt"
				});
				string iconPath = string.Concat(new object[]
				{
					text2,
					"/Model ",
					i + 1,
					" Icon.png"
				});
				string screenshotPath = string.Concat(new object[]
				{
					text2,
					"/Model ",
					i + 1,
					" Screenshot.png"
				});
				if (!File.Exists(text3) && !File.Exists(path))
				{
					BWLog.Info("Exporting model to path '" + text3 + "'");
					File.WriteAllText(text3, text);
					Dictionary<GAF, int> dictionary = new Dictionary<GAF, int>();
					foreach (List<List<Tile>> info in model)
					{
						Clipboard.CalculateGafRelevance(info, dictionary);
					}
					File.WriteAllText(path, Scarcity.GetInventoryJSON(dictionary, false, false));
					ScreenshotUtils.GenerateModelIconTexture(model, true, delegate(Texture2D tex)
					{
						byte[] bytes = tex.EncodeToPNG();
						UnityEngine.Object.Destroy(tex);
						File.WriteAllBytes(iconPath, bytes);
					});
					ScreenshotUtils.GenerateModelSnapshotTexture(model, true, delegate(Texture2D tex)
					{
						byte[] bytes = tex.EncodeToPNG();
						UnityEngine.Object.Destroy(tex);
						File.WriteAllBytes(screenshotPath, bytes);
					});
					break;
				}
			}
		}
	}

	// Token: 0x060019DF RID: 6623 RVA: 0x000BE3E8 File Offset: 0x000BC7E8
	public static void DefineRewardModel(string modelName, string modelJsonStr)
	{
		RewardVisualization.definedModels[modelName] = modelJsonStr;
	}

	// Token: 0x060019E0 RID: 6624 RVA: 0x000BE3F6 File Offset: 0x000BC7F6
	public static void DefineRewardModelIcon(string modelName, string modelJsonStr)
	{
		RewardVisualization.LoadRewardModelIcon(modelName, modelJsonStr);
	}

	// Token: 0x060019E1 RID: 6625 RVA: 0x000BE400 File Offset: 0x000BC800
	public static void VisualizeBlockReward(string blockCountJson)
	{
		JObject jobject = JSONDecoder.Decode(blockCountJson);
		Blocksworld.rewardVisualizationGafs = new List<GAF>();
		foreach (JObject jobject2 in jobject.ArrayValue)
		{
			GAF item = new GAF("Block.VisualizeReward", new object[]
			{
				(string)jobject2[1],
				(int)jobject2[2]
			});
			Blocksworld.rewardVisualizationGafs.Add(item);
		}
		Blocksworld.rewardVisualizationIndex = 0;
		Blocksworld.rewardExecutionInfo.timer = 0f;
	}

	// Token: 0x060019E2 RID: 6626 RVA: 0x000BE4BC File Offset: 0x000BC8BC
	public static void VisualizeRewardModel(string modelName, string modelJsonStr)
	{
		RewardVisualization.definedModels[modelName] = modelJsonStr;
		Blocksworld.rewardVisualizationGafs = new List<GAF>();
		Blocksworld.rewardVisualizationGafs.Add(new GAF("Block.VisualizeReward", new object[]
		{
			modelName,
			1
		}));
		Blocksworld.rewardVisualizationIndex = 0;
		Blocksworld.rewardExecutionInfo.timer = 0f;
	}

	// Token: 0x060019E3 RID: 6627 RVA: 0x000BE51C File Offset: 0x000BC91C
	public static void ShowSetPurchasePrompt(string rewardsJson, string setTitle, int setId, int setPrice)
	{
		Blocksworld.waitForSetPurchase = true;
		JObject jobject = JSONDecoder.Decode(rewardsJson);
		Dictionary<GAF, int> dictionary = new Dictionary<GAF, int>();
		string dialogCaption = "You Won!";
		foreach (JObject jobject2 in jobject.ArrayValue)
		{
			GAF key = new GAF(jobject2[0].StringValue, new object[]
			{
				jobject2[1].StringValue
			});
			int intValue = jobject2[2].IntValue;
			dictionary[key] = intValue;
		}
		if (RewardVisualization.expectedRewardModelIconCount == 0)
		{
			Blocksworld.UI.Dialog.ShowSetPurchasePrompt(dictionary, dialogCaption, setTitle, setId, setPrice);
			return;
		}
		Blocksworld.bw.StartCoroutine(Blocksworld.WaitForModelIconsAndShowSetPurchasePrompt(dictionary, dialogCaption, setTitle, setId, setPrice));
	}

	// Token: 0x060019E4 RID: 6628 RVA: 0x000BE600 File Offset: 0x000BCA00
	private static IEnumerator WaitForModelIconsAndShowSetPurchasePrompt(Dictionary<GAF, int> rewards, string dialogCaption, string setTitle, int setId, int setPrice)
	{
		while (!RewardVisualization.AreRewardModelIconsLoaded())
		{
			yield return null;
		}
		Blocksworld.UI.Dialog.ShowSetPurchasePrompt(rewards, dialogCaption, setTitle, setId, setPrice);
		yield break;
	}

	// Token: 0x060019E5 RID: 6629 RVA: 0x000BE638 File Offset: 0x000BCA38
	public static bool TileEnabled(Tile tile)
	{
		bool flag;
		if (Blocksworld.enabledGAFs != null)
		{
			flag = Blocksworld.enabledGAFs.Contains(tile.gaf);
		}
		else if (BW.Options.useScarcity() && Scarcity.inventory != null)
		{
			GAF gaf = tile.gaf;
			if (gaf.Predicate == Block.predicateSendCustomSignal || gaf.Predicate == Block.predicateSendCustomSignalModel || Block.customVariablePredicates.Contains(gaf.Predicate))
			{
				gaf = Scarcity.GetNormalizedGaf(gaf, false);
			}
			flag = (Scarcity.inventory.ContainsKey(gaf) && Scarcity.inventory[gaf] != 0);
		}
		else
		{
			flag = Blocksworld.IsUnlocked(tile);
		}
		if (CharacterEditor.Instance.InEditMode())
		{
			bool flag2 = PanelSlots.GetTabIndexForGaf(tile.gaf) == 6;
			if (tile.IsCreate())
			{
				flag2 &= (tile.panelSection != 29 && tile.panelSection != 23 && tile.panelSection != 25 && tile.panelSection != 32);
				flag = (flag && flag2);
			}
		}
		return flag;
	}

	// Token: 0x060019E6 RID: 6630 RVA: 0x000BE760 File Offset: 0x000BCB60
	private static void RemoveUnusedBlockPrefabs()
	{
		HashSet<string> hashSet = new HashSet<string>();
		List<Block> list = BWSceneManager.AllBlocks();
		foreach (Block block in list)
		{
			foreach (List<Tile> list2 in block.tiles)
			{
				foreach (Tile tile in list2)
				{
					GAF gaf = tile.gaf;
					if (gaf.Predicate == Block.predicateCreate)
					{
						hashSet.Add((string)gaf.Args[0]);
					}
				}
			}
		}
		ResourceLoader.UnloadUnusedBlockPrefabs(hashSet);
	}

	// Token: 0x060019E7 RID: 6631 RVA: 0x000BE87C File Offset: 0x000BCC7C
	private static void RemoveUnusedTextures()
	{
		HashSet<string> hashSet = new HashSet<string>();
		List<Block> list = BWSceneManager.AllBlocks();
		foreach (Block block in list)
		{
			foreach (List<Tile> list2 in block.tiles)
			{
				foreach (Tile tile in list2)
				{
					GAF gaf = tile.gaf;
					if (gaf.Predicate == Block.predicateTextureTo)
					{
						hashSet.Add((string)gaf.Args[0]);
					}
				}
			}
		}
		hashSet.Add("Plain");
		ResourceLoader.UnloadUnusedTextures(hashSet);
	}

	// Token: 0x060019E8 RID: 6632 RVA: 0x000BE9A4 File Offset: 0x000BCDA4
	private static void RemoveUnusedAssets()
	{
		bool flag = false;
		if (Blocksworld.loadedTextureCountAfterRemovingAssets != ResourceLoader.loadedTextures.Count)
		{
			Blocksworld.RemoveUnusedTextures();
			flag = true;
		}
		if (Blocksworld.loadedBlockCountAfterRemovingAssets != Blocksworld.goPrefabs.Count)
		{
			Blocksworld.RemoveUnusedBlockPrefabs();
			flag = true;
		}
		if (flag)
		{
			Resources.UnloadUnusedAssets();
			GC.Collect();
			Blocksworld.loadedTextureCountAfterRemovingAssets = ResourceLoader.loadedTextures.Count;
			Blocksworld.loadedBlockCountAfterRemovingAssets = Blocksworld.goPrefabs.Count;
		}
	}

	// Token: 0x060019E9 RID: 6633 RVA: 0x000BEA18 File Offset: 0x000BCE18
	public static void DidReceiveMemoryWarning()
	{
		if (!RewardVisualization.rewardAnimationRunning && !Blocksworld.inBackground)
		{
			Blocksworld.RemoveUnusedAssets();
		}
	}

	// Token: 0x060019EA RID: 6634 RVA: 0x000BEA34 File Offset: 0x000BCE34
	public static Color[] GetColors(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			Color color = new Color(1f, 1f, 1f, 0f);
			return new Color[]
			{
				color,
				color
			};
		}
		if (!Blocksworld.colorDefinitions.ContainsKey(name))
		{
			BWLog.Info("Could not find color " + name + " using blue and red gradient");
			Blocksworld.colorDefinitions[name] = new Color[]
			{
				Color.blue,
				Color.red
			};
		}
		return Blocksworld.colorDefinitions[name];
	}

	// Token: 0x060019EB RID: 6635 RVA: 0x000BEAEB File Offset: 0x000BCEEB
	public static Color getColor(string name)
	{
		return Blocksworld.GetColors(name)[0];
	}

	// Token: 0x060019EC RID: 6636 RVA: 0x000BEB00 File Offset: 0x000BCF00
	public static void SetupColorDefinitions(Dictionary<string, Color[]> colorDefs, Dictionary<string, Color[]> gradientDefs, Dictionary<string, Color> skyBoxTintDefs)
	{
		ColorDefinitions colorDefinitions = Resources.Load<ColorDefinitions>("ColorDefinitions");
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		ColorDefinition[] definitions = colorDefinitions.definitions;
		foreach (ColorDefinition colorDefinition in definitions)
		{
			if (!colorDefs.ContainsKey(colorDefinition.name))
			{
				colorDefs[colorDefinition.name] = new Color[]
				{
					colorDefinition.first,
					colorDefinition.second
				};
				dictionary[colorDefinition.name] = colorDefinition.skyBoxColorName;
			}
		}
		foreach (KeyValuePair<string, string> keyValuePair in dictionary)
		{
			string key = keyValuePair.Key;
			string value = keyValuePair.Value;
			if (colorDefs.ContainsKey(value))
			{
				skyBoxTintDefs[key] = colorDefs[value][0];
			}
			else if (!string.IsNullOrEmpty(value))
			{
				BWLog.Error(string.Concat(new string[]
				{
					"Unable to find sky box tint '",
					value,
					"' for paint '",
					key,
					"'"
				}));
			}
		}
		ColorGradientDefinitions[] gradientDefinitions = colorDefinitions.gradientDefinitions;
		if (gradientDefinitions != null)
		{
			foreach (ColorGradientDefinitions colorGradientDefinitions in gradientDefinitions)
			{
				if (!gradientDefs.ContainsKey(colorGradientDefinitions.name))
				{
					gradientDefs[colorGradientDefinitions.name] = new Color[]
					{
						colorGradientDefinitions.skyColor,
						colorGradientDefinitions.equatorColor,
						colorGradientDefinitions.groundColor
					};
				}
			}
		}
	}

	// Token: 0x060019ED RID: 6637 RVA: 0x000BECF4 File Offset: 0x000BD0F4
	public static void SetupRarityBorders(Dictionary<RarityLevelEnum, Material> enabledMaterials, Dictionary<RarityLevelEnum, Material> disabledMaterials)
	{
		string str = (!Blocksworld.hd) ? "SD" : "HD";
		RarityBorderDefinitions rarityBorderDefinitions = Resources.Load<RarityBorderDefinitions>("RarityBorders/RarityBorderDefinitions" + str);
		foreach (RarityBorderDefinition rarityBorderDefinition in rarityBorderDefinitions.rarityBorderDefinitions)
		{
			enabledMaterials[rarityBorderDefinition.rarity] = rarityBorderDefinition.tileBorderMaterialEnabled;
			disabledMaterials[rarityBorderDefinition.rarity] = rarityBorderDefinition.tileBorderMaterialDisabled;
		}
	}

	// Token: 0x060019EE RID: 6638 RVA: 0x000BED74 File Offset: 0x000BD174
	public void SetupTextureMetaDatas()
	{
		TextureMetaDatas component = Blocksworld.blocksworldDataContainer.GetComponent<TextureMetaDatas>();
		if (component != null)
		{
			TextureMetaData[] infos = component.infos;
			foreach (TextureMetaData textureMetaData in infos)
			{
				Vector3 preferredSize = textureMetaData.preferredSize;
				if (preferredSize.x * preferredSize.y * preferredSize.z > 0.1f)
				{
					Materials.wrapTexturePrefSizes[textureMetaData.name] = textureMetaData.preferredSize;
				}
				Materials.fourSidesIgnoreRightLeft[textureMetaData.name] = textureMetaData.fourSidesIgnoreRightLeft;
				Materials.twoSidesMirror[textureMetaData.name] = textureMetaData.twoSidesMirror;
				Materials.mipMapBias[textureMetaData.name] = textureMetaData.mipMapBias;
				List<TextureApplicationChangeRule> list = new List<TextureApplicationChangeRule>();
				foreach (TextureApplicationChangeRule textureApplicationChangeRule in textureMetaData.applicationRules)
				{
					if (textureApplicationChangeRule.setScarcityEquivalent)
					{
						Scarcity.SetTextureOriginal(textureApplicationChangeRule.texture, textureMetaData.name);
					}
					list.Add(textureApplicationChangeRule);
				}
				if (list.Count > 0)
				{
					Materials.textureApplicationRules[textureMetaData.name] = list;
				}
			}
		}
		else
		{
			BWLog.Info("Could not find texture meta data component");
		}
	}

	// Token: 0x060019EF RID: 6639 RVA: 0x000BEEC4 File Offset: 0x000BD2C4
	public static bool IsLuminousPaint(string paint)
	{
		bool flag;
		if (Blocksworld.luminousPaints.TryGetValue(paint, out flag))
		{
			return flag;
		}
		flag = paint.StartsWith("Luminous ");
		Blocksworld.luminousPaints[paint] = flag;
		return flag;
	}

	// Token: 0x060019F0 RID: 6640 RVA: 0x000BEEFE File Offset: 0x000BD2FE
	public static bool IsLuminousTexture(string texture)
	{
		if (texture != null)
		{
			if (texture == "Pulsate Glow")
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x060019F1 RID: 6641 RVA: 0x000BEF20 File Offset: 0x000BD320
	public static void UpdateDrag()
	{
		float num = 0.2f;
		foreach (Chunk chunk in Blocksworld.chunks)
		{
			Rigidbody rb = chunk.rb;
			if (rb != null)
			{
				float drag = num * Blocksworld.dragMultiplier * chunk.GetDragMultiplier();
				rb.drag = drag;
			}
		}
	}

	// Token: 0x060019F2 RID: 6642 RVA: 0x000BEFA8 File Offset: 0x000BD3A8
	public static void UpdateAngularDrag()
	{
		float num = 2f;
		foreach (Chunk chunk in Blocksworld.chunks)
		{
			Rigidbody rb = chunk.rb;
			if (rb != null)
			{
				float angularDrag = num * Blocksworld.angularDragMultiplier * chunk.GetAngularDragMultiplier();
				rb.angularDrag = angularDrag;
			}
		}
	}

	// Token: 0x060019F3 RID: 6643 RVA: 0x000BF030 File Offset: 0x000BD430
	public static bool IsGlobalLockPull()
	{
		return Blocksworld.staticLockPull || Blocksworld.dynamicLockPull;
	}

	// Token: 0x060019F4 RID: 6644 RVA: 0x000BF044 File Offset: 0x000BD444
	public static void AddUpdateCommand(Command c)
	{
		Blocksworld.updateCommands.Add(c);
	}

	// Token: 0x060019F5 RID: 6645 RVA: 0x000BF051 File Offset: 0x000BD451
	public static void AddFixedUpdateCommand(Command c)
	{
		Blocksworld.fixedUpdateCommands.Add(c);
	}

	// Token: 0x060019F6 RID: 6646 RVA: 0x000BF05E File Offset: 0x000BD45E
	public static void AddFixedUpdateUniqueCommand(Command c, bool resetWhenAdded = true)
	{
		Command.AddUniqueCommand(Blocksworld.fixedUpdateCommands, c, resetWhenAdded);
	}

	// Token: 0x060019F7 RID: 6647 RVA: 0x000BF06C File Offset: 0x000BD46C
	public static void AddResetStateCommand(Command c)
	{
		Blocksworld.resetStateCommands.Add(c);
	}

	// Token: 0x060019F8 RID: 6648 RVA: 0x000BF079 File Offset: 0x000BD479
	public static void AddResetStateUniqueCommand(Command c, bool resetWhenAdded = true)
	{
		Command.AddUniqueCommand(Blocksworld.resetStateCommands, c, resetWhenAdded);
	}

	// Token: 0x060019F9 RID: 6649 RVA: 0x000BF087 File Offset: 0x000BD487
	private static void UpdateEditorMusicPlayerEnabled()
	{
		if (BW.isUnityEditor)
		{
			Blocksworld.musicPlayer.SetEnabled(Blocksworld.IsMusicEnabledForState());
		}
	}

	// Token: 0x060019FA RID: 6650 RVA: 0x000BF0A2 File Offset: 0x000BD4A2
	public static bool InModalDialogState()
	{
		return Blocksworld.CurrentState == State.WaitForOption || Blocksworld.CurrentState == State.WaitForOptionScarcityFeedback || (Tutorial.InTutorialOrPuzzle() && Tutorial.progressBlocked);
	}

	// Token: 0x060019FB RID: 6651 RVA: 0x000BF0D0 File Offset: 0x000BD4D0
	public static int RemoveAllTilesWithPredicate(HashSet<Predicate> preds)
	{
		int num = 0;
		List<Block> list = BWSceneManager.AllBlocks();
		for (int i = 0; i < list.Count; i++)
		{
			Block block = list[i];
			for (int j = block.tiles.Count - 1; j >= 1; j--)
			{
				List<Tile> list2 = block.tiles[j];
				for (int k = list2.Count - 1; k >= 0; k--)
				{
					if (preds.Contains(list2[k].gaf.Predicate))
					{
						list2.RemoveAt(k);
						num++;
					}
				}
				if (list2.Count == 1 && list2[0].gaf.Predicate == Block.predicateThen)
				{
					block.tiles.RemoveAt(j);
				}
			}
			block.tiles.Add(Block.EmptyTileRow());
		}
		return num;
	}

	// Token: 0x060019FC RID: 6652 RVA: 0x000BF1C8 File Offset: 0x000BD5C8
	public static List<Tile> GetBlockTilesMatching(Predicate<Tile> pred)
	{
		List<Tile> list = new List<Tile>();
		List<Block> list2 = BWSceneManager.AllBlocks();
		for (int i = 0; i < list2.Count; i++)
		{
			Block block = list2[i];
			foreach (List<Tile> list3 in block.tiles)
			{
				foreach (Tile tile in list3)
				{
					if (pred(tile))
					{
						list.Add(tile);
					}
				}
			}
		}
		return list;
	}

	// Token: 0x060019FD RID: 6653 RVA: 0x000BF2A4 File Offset: 0x000BD6A4
	private void LateUpdate()
	{
		Blocksworld.blocksworldCamera.LateUpdate();
		if (!Blocksworld.vrEnabled)
		{
			return;
		}
		if (MappedInput.InputDown(MappableInput.RESET_VR_SENSOR))
		{
			Blocksworld.ResetVRSensor();
		}
		bool flag = MappedInput.InputDown(MappableInput.EXIT_VR_MODE);
		flag |= MappedInput.InputDown(MappableInput.STOP);
		if (flag)
		{
			Blocksworld.SetVRMode(false);
		}
	}

	// Token: 0x060019FE RID: 6654 RVA: 0x000BF2F8 File Offset: 0x000BD6F8
	public static void RecieveIOSMessage(string messageStr)
	{
		if (messageStr != null)
		{
			if (messageStr == "ReplayKitViewControllerWillAppear")
			{
				if (WorldSession.current != null)
				{
					WorldSession.current.ReplayKitViewControllerDidAppear();
				}
				return;
			}
			if (messageStr == "ReplayKitViewControllerDidDisappear")
			{
				if (WorldSession.current != null)
				{
					WorldSession.current.ReplayKitViewControllerDidDisappear();
				}
				return;
			}
			if (messageStr == "ReplayKitRecordingInterrupted")
			{
				Blocksworld.UI.Tapedeck.RecordingWasStoppedExternally();
				return;
			}
			if (messageStr == "ReplayKitAvailablityDidChange")
			{
				Blocksworld.UI.Tapedeck.SetScreenRecordingEnabled(WorldSession.platformDelegate.ScreenRecordingAvailable());
				if (WorldUILayout.currentLayout != null)
				{
					WorldUILayout.currentLayout.Apply();
				}
				return;
			}
		}
		if (messageStr.StartsWith("CopyWorldId:"))
		{
			string worldIdClipboard = messageStr.Remove(0, 11);
			WorldSession.worldIdClipboard = worldIdClipboard;
		}
		else
		{
			BWLog.Warning("Don't understand message from iOS: " + messageStr);
		}
	}

	// Token: 0x060019FF RID: 6655 RVA: 0x000BF404 File Offset: 0x000BD804
	public static void HandleWin()
	{
		foreach (KeyValuePair<BlockAnimatedCharacter, CharacterStateHandler> keyValuePair in BlockAnimatedCharacter.stateControllers)
		{
			if (TagManager.BlockHasTag(keyValuePair.Key, "Hero"))
			{
				keyValuePair.Value.InterruptState(CharacterState.Win, true);
			}
			else if (TagManager.BlockHasTag(keyValuePair.Key, "Villain"))
			{
				keyValuePair.Value.InterruptState(CharacterState.Fail, true);
			}
		}
	}

	// Token: 0x06001A00 RID: 6656 RVA: 0x000BF4AC File Offset: 0x000BD8AC
	public static void HandleLose()
	{
		foreach (KeyValuePair<BlockAnimatedCharacter, CharacterStateHandler> keyValuePair in BlockAnimatedCharacter.stateControllers)
		{
			if (TagManager.BlockHasTag(keyValuePair.Key, "Hero"))
			{
				keyValuePair.Value.InterruptState(CharacterState.Fail, true);
			}
			else if (TagManager.BlockHasTag(keyValuePair.Key, "Villain"))
			{
				keyValuePair.Value.InterruptState(CharacterState.Win, true);
			}
		}
	}

	// Token: 0x0400144C RID: 5196
	public static float fixedDeltaTime;

	// Token: 0x0400144D RID: 5197
	public static bool useCompactGafWriteRenamings = false;

	// Token: 0x0400144E RID: 5198
	public static bool inBackground = false;

	// Token: 0x0400144F RID: 5199
	public static Bunch selectedBunch = null;

	// Token: 0x04001450 RID: 5200
	public static bool lockPull = false;

	// Token: 0x04001451 RID: 5201
	public static List<NamedPose> cameraPoses = new List<NamedPose>();

	// Token: 0x04001452 RID: 5202
	public static Dictionary<string, NamedPose> cameraPosesMap = new Dictionary<string, NamedPose>();

	// Token: 0x04001453 RID: 5203
	public static bool staticLockPull = false;

	// Token: 0x04001454 RID: 5204
	public static bool dynamicLockPull = false;

	// Token: 0x04001455 RID: 5205
	public static bool lockInput = false;

	// Token: 0x04001456 RID: 5206
	public static bool isFirstFrame = false;

	// Token: 0x04001457 RID: 5207
	public static bool hideInGameUI = false;

	// Token: 0x04001458 RID: 5208
	public static bool started = false;

	// Token: 0x04001459 RID: 5209
	public static bool renderingShadows = false;

	// Token: 0x0400145A RID: 5210
	public static bool renderingSkybox = false;

	// Token: 0x0400145B RID: 5211
	public static bool renderingWater = false;

	// Token: 0x0400145C RID: 5212
	public static float _lightIntensityBasic = 1f;

	// Token: 0x0400145D RID: 5213
	public static float _lightIntensityMultiplier = 1f;

	// Token: 0x0400145E RID: 5214
	public static Color _buildModeFogColor = Color.white;

	// Token: 0x0400145F RID: 5215
	public static float _buildModeFogStart = 0f;

	// Token: 0x04001460 RID: 5216
	public static float _buildModeFogEnd = 0f;

	// Token: 0x04001461 RID: 5217
	public static Clipboard clipboard;

	// Token: 0x04001462 RID: 5218
	public static ModelCollection modelCollection;

	// Token: 0x04001463 RID: 5219
	private static List<Command> updateCommands = new List<Command>();

	// Token: 0x04001464 RID: 5220
	private static List<Command> fixedUpdateCommands = new List<Command>();

	// Token: 0x04001465 RID: 5221
	private static List<Command> resetStateCommands = new List<Command>();

	// Token: 0x04001466 RID: 5222
	public static HashSet<string> unlockedPaints = new HashSet<string>();

	// Token: 0x04001467 RID: 5223
	public static bool resetting = false;

	// Token: 0x04001468 RID: 5224
	public static bool loadComplete = false;

	// Token: 0x04001469 RID: 5225
	public static bool isLoadingScene = false;

	// Token: 0x0400146A RID: 5226
	public static bool resettingPlay = false;

	// Token: 0x0400146B RID: 5227
	public static bool capturingScreenshot = false;

	// Token: 0x0400146C RID: 5228
	public static Vector3 constrainedManipulationAxis = Vector3.up;

	// Token: 0x0400146D RID: 5229
	public static HashSet<Block> editorSelectionLocked = new HashSet<Block>();

	// Token: 0x0400146E RID: 5230
	private static bool keyLReleased = true;

	// Token: 0x0400146F RID: 5231
	private static Block recentSelectionUnlockedBlock = null;

	// Token: 0x04001470 RID: 5232
	public static bool isUsingSmallScreen;

	// Token: 0x04001471 RID: 5233
	public static string currentBackgroundMusic = string.Empty;

	// Token: 0x04001472 RID: 5234
	private static int loadedTextureCountAfterRemovingAssets = -1;

	// Token: 0x04001473 RID: 5235
	private static int loadedBlockCountAfterRemovingAssets = -1;

	// Token: 0x04001474 RID: 5236
	public static bool interpolateRigidBodies = false;

	// Token: 0x04001475 RID: 5237
	public const int sizeTile = 80;

	// Token: 0x04001476 RID: 5238
	public const int sizeTileMesh = 75;

	// Token: 0x04001477 RID: 5239
	public static int marginTile = -16;

	// Token: 0x04001478 RID: 5240
	public static int defaultPanelPadding = 32;

	// Token: 0x04001479 RID: 5241
	public const int buildPanelPadding = 20;

	// Token: 0x0400147A RID: 5242
	public static float fogMultiplier = 1f;

	// Token: 0x0400147B RID: 5243
	public static float fogStart = 40f;

	// Token: 0x0400147C RID: 5244
	public static float fogEnd = 100f;

	// Token: 0x0400147D RID: 5245
	public static Color fogColor = new Color(0.380392164f, 0.7411765f, 1f);

	// Token: 0x0400147E RID: 5246
	public static HashSet<string> existingBlockNames = new HashSet<string>();

	// Token: 0x0400147F RID: 5247
	public static Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();

	// Token: 0x04001480 RID: 5248
	public static Dictionary<string, GameObject> goPrefabs = new Dictionary<string, GameObject>();

	// Token: 0x04001481 RID: 5249
	public static Dictionary<string, Mesh> meshes = new Dictionary<string, Mesh>();

	// Token: 0x04001482 RID: 5250
	public static Dictionary<string, Collider> colliders = new Dictionary<string, Collider>();

	// Token: 0x04001483 RID: 5251
	public static Dictionary<string, GameObject> compoundColliders = new Dictionary<string, GameObject>();

	// Token: 0x04001484 RID: 5252
	public static Dictionary<string, GameObject> shapes = new Dictionary<string, GameObject>();

	// Token: 0x04001485 RID: 5253
	public static Dictionary<string, GameObject> glues = new Dictionary<string, GameObject>();

	// Token: 0x04001486 RID: 5254
	public static Dictionary<string, GameObject> joints = new Dictionary<string, GameObject>();

	// Token: 0x04001487 RID: 5255
	public static List<GAF> globalGafs = new List<GAF>();

	// Token: 0x04001488 RID: 5256
	public static List<GAF> rewardVisualizationGafs;

	// Token: 0x04001489 RID: 5257
	private static int rewardVisualizationIndex;

	// Token: 0x0400148A RID: 5258
	private static ScriptRowExecutionInfo rewardExecutionInfo = new ScriptRowExecutionInfo();

	// Token: 0x0400148B RID: 5259
	public static bool winIsWaiting;

	// Token: 0x0400148C RID: 5260
	public static bool hasWon;

	// Token: 0x0400148D RID: 5261
	public static bool stopASAP = false;

	// Token: 0x0400148E RID: 5262
	public static bool waitForSetPurchase = false;

	// Token: 0x0400148F RID: 5263
	private static bool canSaveInMenu = true;

	// Token: 0x04001490 RID: 5264
	public static bool worldSaveEnabled = true;

	// Token: 0x04001491 RID: 5265
	private static bool f3PressedInCurrentWorld = false;

	// Token: 0x04001492 RID: 5266
	public static Blocksworld bw;

	// Token: 0x04001493 RID: 5267
	public static GameObject blocksworldDataContainer;

	// Token: 0x04001494 RID: 5268
	protected static Camera _mainCamera;

	// Token: 0x04001495 RID: 5269
	private static Camera _mainCameraOverrideBackup;

	// Token: 0x04001496 RID: 5270
	public static Transform cameraTransform;

	// Token: 0x04001497 RID: 5271
	public static Vector3 cameraPosition;

	// Token: 0x04001498 RID: 5272
	public static Vector3 cameraForward;

	// Token: 0x04001499 RID: 5273
	public static Vector3 cameraUp;

	// Token: 0x0400149A RID: 5274
	public static Vector3 cameraRight;

	// Token: 0x0400149B RID: 5275
	public static Camera guiCamera;

	// Token: 0x0400149C RID: 5276
	public static Camera rewardCamera;

	// Token: 0x0400149D RID: 5277
	public static GUISkin skin;

	// Token: 0x0400149E RID: 5278
	public static GameObject directionalLight;

	// Token: 0x0400149F RID: 5279
	public static Light overheadLight;

	// Token: 0x040014A0 RID: 5280
	public static Transform lightingRig;

	// Token: 0x040014A1 RID: 5281
	public static Texture buttonPlus;

	// Token: 0x040014A2 RID: 5282
	public static Texture buttonMinus;

	// Token: 0x040014A3 RID: 5283
	public static BlockSky worldSky = null;

	// Token: 0x040014A4 RID: 5284
	public static GameObject worldOcean = null;

	// Token: 0x040014A5 RID: 5285
	public static BlockWater worldOceanBlock = null;

	// Token: 0x040014A6 RID: 5286
	private int whiteBackground;

	// Token: 0x040014A7 RID: 5287
	public static GameObject prefabArrow;

	// Token: 0x040014A8 RID: 5288
	public ParticleSystem explosion;

	// Token: 0x040014A9 RID: 5289
	public static ParticleSystem stars;

	// Token: 0x040014AA RID: 5290
	public static ParticleSystem starsReward;

	// Token: 0x040014AB RID: 5291
	public static GameObject rewardStarburst;

	// Token: 0x040014AC RID: 5292
	public static SfxDefinitions sfxDefinitions;

	// Token: 0x040014AD RID: 5293
	public static EngineSoundDefinitions engineSoundDefinitions;

	// Token: 0x040014AE RID: 5294
	public static LeaderboardData leaderboardData;

	// Token: 0x040014AF RID: 5295
	public static BlocksworldComponentData componentData;

	// Token: 0x040014B0 RID: 5296
	public static readonly BlocksworldCamera blocksworldCamera = new BlocksworldCamera();

	// Token: 0x040014B1 RID: 5297
	public static Vector3 prevCamPos = Util.nullVector3;

	// Token: 0x040014B2 RID: 5298
	public static bool cameraMoved = true;

	// Token: 0x040014B3 RID: 5299
	public static Color lightColor = Color.white;

	// Token: 0x040014B4 RID: 5300
	public static Color dynamicLightColor = Color.white;

	// Token: 0x040014B5 RID: 5301
	public static float dynamicLightIntensityMultiplier = 1f;

	// Token: 0x040014B6 RID: 5302
	public static List<ILightChanger> dynamicalLightChangers = new List<ILightChanger>();

	// Token: 0x040014B7 RID: 5303
	public static WeatherEffect weather = WeatherEffect.clear;

	// Token: 0x040014B8 RID: 5304
	public static FpsCounter fpsCounter;

	// Token: 0x040014B9 RID: 5305
	private Vector3 mousePositionLast = Vector3.zero;

	// Token: 0x040014BA RID: 5306
	private Vector3 mousePositionDelta;

	// Token: 0x040014BB RID: 5307
	public static Vector3 mousePositionFirst;

	// Token: 0x040014BC RID: 5308
	public static BuildPanel buildPanel;

	// Token: 0x040014BD RID: 5309
	public static ScriptPanel scriptPanel;

	// Token: 0x040014BE RID: 5310
	public static Tile tileButtonClearScript;

	// Token: 0x040014BF RID: 5311
	public static Tile tileButtonCopyScript;

	// Token: 0x040014C0 RID: 5312
	public static Tile tileButtonPasteScript;

	// Token: 0x040014C1 RID: 5313
	public static HashSet<GAF> publicProvidedGafs = new HashSet<GAF>();

	// Token: 0x040014C2 RID: 5314
	private float offsetMenu;

	// Token: 0x040014C3 RID: 5315
	public static List<GAF> enabledGAFs = null;

	// Token: 0x040014C4 RID: 5316
	public static List<GAF> enabledPanelBlock = null;

	// Token: 0x040014C5 RID: 5317
	private static string displayString;

	// Token: 0x040014C6 RID: 5318
	public static List<GAF> unlockedGAFs;

	// Token: 0x040014C7 RID: 5319
	public static HashSet<GAF> sidePanelGafs;

	// Token: 0x040014C8 RID: 5320
	public static Dictionary<OldSymbol, float> joysticks = new Dictionary<OldSymbol, float>();

	// Token: 0x040014C9 RID: 5321
	public TileParameterEditor tileParameterEditor;

	// Token: 0x040014CA RID: 5322
	public static Tile selectedTile;

	// Token: 0x040014CB RID: 5323
	public bool forcePlayMode;

	// Token: 0x040014CC RID: 5324
	public static float lastRealtimeSinceStartup = 0f;

	// Token: 0x040014CD RID: 5325
	public static float deltaTime = 0f;

	// Token: 0x040014CE RID: 5326
	public static bool hd;

	// Token: 0x040014CF RID: 5327
	public static float screenScale;

	// Token: 0x040014D0 RID: 5328
	public static int screenWidth;

	// Token: 0x040014D1 RID: 5329
	public static int screenHeight;

	// Token: 0x040014D2 RID: 5330
	public static Vector3 mouse;

	// Token: 0x040014D3 RID: 5331
	public static int numTouches = 0;

	// Token: 0x040014D4 RID: 5332
	public static Vector3[] touches = new Vector3[20];

	// Token: 0x040014D5 RID: 5333
	public static Block mouseBlock = null;

	// Token: 0x040014D6 RID: 5334
	public static int mouseBlockIndex = 0;

	// Token: 0x040014D7 RID: 5335
	public static Vector3 mouseBlockNormal = Vector3.zero;

	// Token: 0x040014D8 RID: 5336
	public static Vector3 mouseBlockHitPosition = Vector3.zero;

	// Token: 0x040014D9 RID: 5337
	public static Block mouseBlockLast = null;

	// Token: 0x040014DA RID: 5338
	public bool tWidgetHit;

	// Token: 0x040014DB RID: 5339
	public static bool tWidgetHitAtStart = false;

	// Token: 0x040014DC RID: 5340
	public static bool tBoxHit = false;

	// Token: 0x040014DD RID: 5341
	public static bool tBoxHitAtStart = false;

	// Token: 0x040014DE RID: 5342
	public static Vector3 mouseBlockNormalLast = Vector3.zero;

	// Token: 0x040014DF RID: 5343
	public static Block selectedBlock = null;

	// Token: 0x040014E0 RID: 5344
	private Vector3 anglesDelta;

	// Token: 0x040014E1 RID: 5345
	private Quaternion rotationStart;

	// Token: 0x040014E2 RID: 5346
	private Vector3 textureNormalLast;

	// Token: 0x040014E3 RID: 5347
	private float orbit;

	// Token: 0x040014E4 RID: 5348
	private static string stringInput = null;

	// Token: 0x040014E5 RID: 5349
	private static bool consumeEvent = false;

	// Token: 0x040014E6 RID: 5350
	public LineRenderer pullObjectLineRenderer;

	// Token: 0x040014E7 RID: 5351
	public static GestureRecognizer recognizer = new GestureRecognizer();

	// Token: 0x040014E8 RID: 5352
	private BaseGesture[] buildOnlyGestures;

	// Token: 0x040014E9 RID: 5353
	private SecretCommandGesture autoPlayGesture;

	// Token: 0x040014EA RID: 5354
	private PullObjectGesture pullObject;

	// Token: 0x040014EB RID: 5355
	public TapControlGesture tapControl;

	// Token: 0x040014EC RID: 5356
	public ParameterEditGesture parameterEditGesture;

	// Token: 0x040014ED RID: 5357
	public static UIGesture uiGesture;

	// Token: 0x040014EE RID: 5358
	public static TBoxGesture tBoxGesture;

	// Token: 0x040014EF RID: 5359
	public static TileDragGesture tileDragGesture;

	// Token: 0x040014F0 RID: 5360
	public static CreateTileDragGesture createTileDragGesture;

	// Token: 0x040014F1 RID: 5361
	public static ReplaceBodyPartTileDragGesture replaceBodyPartGesture;

	// Token: 0x040014F2 RID: 5362
	public static CharacterEditGearGesture characterEditGearGesture;

	// Token: 0x040014F3 RID: 5363
	public static CWidgetGesture cWidgetGesture;

	// Token: 0x040014F4 RID: 5364
	public static BlockDuplicateGesture blockDupeGesture;

	// Token: 0x040014F5 RID: 5365
	public static BlockTapGesture blockTapGesture;

	// Token: 0x040014F6 RID: 5366
	public static ButtonTapGesture buttonTapGesture;

	// Token: 0x040014F7 RID: 5367
	public static OrbitDuringControlCameraGesture orbitDuringControlGesture;

	// Token: 0x040014F8 RID: 5368
	private Vector3 mouseStart;

	// Token: 0x040014F9 RID: 5369
	public static string currentWorldId = null;

	// Token: 0x040014FA RID: 5370
	public static bool launchIntoPlayMode;

	// Token: 0x040014FB RID: 5371
	private JObject lastLoadedCameraObj;

	// Token: 0x040014FC RID: 5372
	public static float angularDragMultiplier = 1f;

	// Token: 0x040014FD RID: 5373
	public static float dragMultiplier = 1f;

	// Token: 0x040014FE RID: 5374
	public static List<Block> locked = new List<Block>();

	// Token: 0x040014FF RID: 5375
	public static List<Chunk> chunks = new List<Chunk>();

	// Token: 0x04001500 RID: 5376
	public static float timerStart = -1f;

	// Token: 0x04001501 RID: 5377
	public static float timerStop = -1f;

	// Token: 0x04001502 RID: 5378
	public static bool gameStart = false;

	// Token: 0x04001503 RID: 5379
	public const string SIGNAL_NAMES = "signal-names";

	// Token: 0x04001504 RID: 5380
	public const string BLOCK_NAMES = "block-names";

	// Token: 0x04001505 RID: 5381
	public const string CAMERA_POSES = "camera-poses";

	// Token: 0x04001506 RID: 5382
	public const string PUZZLE_GAFS = "puzzle-gafs";

	// Token: 0x04001507 RID: 5383
	public const string PUZZLE_GAF_USAGE = "puzzle-gaf-usage";

	// Token: 0x04001508 RID: 5384
	public const string PUZZLE_GAFS_AFTER_STEP_BY_STEP = "puzzle-gafs-after-step-by-step";

	// Token: 0x04001509 RID: 5385
	public const string PUZZLE_GAF_USAGE_AFTER_STEP_BY_STEP = "puzzle-gaf-usage-after-step-by-step";

	// Token: 0x0400150A RID: 5386
	public const int SIGNAL_COUNT = 26;

	// Token: 0x0400150B RID: 5387
	public const int MAX_PLAYER_COUNT = 8;

	// Token: 0x0400150C RID: 5388
	public const int MAX_COUNTERS = 2;

	// Token: 0x0400150D RID: 5389
	public const int MAX_TIMERS = 2;

	// Token: 0x0400150E RID: 5390
	public const int MAX_GAUGES = 2;

	// Token: 0x0400150F RID: 5391
	public const int MAX_RADARS = 1;

	// Token: 0x04001510 RID: 5392
	public static bool[] sending = new bool[26];

	// Token: 0x04001511 RID: 5393
	public static float[] sendingValues = new float[26];

	// Token: 0x04001512 RID: 5394
	public static string[] signalNames = new string[26];

	// Token: 0x04001513 RID: 5395
	public const string GENERIC_WILDCARD_NAME = "*";

	// Token: 0x04001514 RID: 5396
	public const string DEFAULT_CUSTOM_SIGNAL_NAME = "Signal";

	// Token: 0x04001515 RID: 5397
	public const string DEFAULT_CUSTOM_INT_VARIABLE_NAME = "Int";

	// Token: 0x04001516 RID: 5398
	public static Dictionary<string, float> sendingCustom = new Dictionary<string, float>();

	// Token: 0x04001517 RID: 5399
	public static Dictionary<Block, string> blockNames = new Dictionary<Block, string>();

	// Token: 0x04001518 RID: 5400
	public static int numSendTilesInUse = 0;

	// Token: 0x04001519 RID: 5401
	public static int numTagTilesInUse = 0;

	// Token: 0x0400151A RID: 5402
	public static HashSet<string> everPresentTagsInUse = new HashSet<string>();

	// Token: 0x0400151B RID: 5403
	public static HashSet<string> arrowTags = new HashSet<string>();

	// Token: 0x0400151C RID: 5404
	public static HashSet<string> handAttachmentTags = new HashSet<string>();

	// Token: 0x0400151D RID: 5405
	public static HashSet<string> laserTags = new HashSet<string>();

	// Token: 0x0400151E RID: 5406
	public static HashSet<string> projectileTags = new HashSet<string>();

	// Token: 0x0400151F RID: 5407
	public static HashSet<string> waterTags = new HashSet<string>();

	// Token: 0x04001520 RID: 5408
	public static HashSet<string> customSignals = new HashSet<string>();

	// Token: 0x04001521 RID: 5409
	public static Dictionary<string, int> customIntVariables = new Dictionary<string, int>();

	// Token: 0x04001522 RID: 5410
	public static Dictionary<Block, Dictionary<string, int>> blockIntVariables = new Dictionary<Block, Dictionary<string, int>>();

	// Token: 0x04001523 RID: 5411
	public static float gameTime = 0f;

	// Token: 0x04001524 RID: 5412
	public static int updateCounter = 0;

	// Token: 0x04001525 RID: 5413
	public static int playFixedUpdateCounter = 0;

	// Token: 0x04001526 RID: 5414
	public static int numCounters = 10;

	// Token: 0x04001527 RID: 5415
	public static Dictionary<string, bool> countersActivated = new Dictionary<string, bool>();

	// Token: 0x04001528 RID: 5416
	public static Dictionary<string, int> counters = new Dictionary<string, int>();

	// Token: 0x04001529 RID: 5417
	public static Dictionary<string, int> counterTargets = new Dictionary<string, int>();

	// Token: 0x0400152A RID: 5418
	public static Dictionary<string, bool> counterTargetsActivated = new Dictionary<string, bool>();

	// Token: 0x0400152B RID: 5419
	public static Texture starOutlineTexture;

	// Token: 0x0400152C RID: 5420
	public static Texture starTexture;

	// Token: 0x0400152D RID: 5421
	public static Plane[] frustumPlanes = new Plane[0];

	// Token: 0x0400152E RID: 5422
	public static float maxBlockTapDistance = 1060f;

	// Token: 0x0400152F RID: 5423
	public static float maxBlockDragDistance = 1060f;

	// Token: 0x04001530 RID: 5424
	public static MusicPlayer musicPlayer;

	// Token: 0x04001531 RID: 5425
	public static Dictionary<string, string> iconColors = new Dictionary<string, string>();

	// Token: 0x04001532 RID: 5426
	public static Dictionary<string, Color[]> colorDefinitions = new Dictionary<string, Color[]>();

	// Token: 0x04001533 RID: 5427
	public static Dictionary<string, Color[]> ambientLightGradientDefinitions = new Dictionary<string, Color[]>();

	// Token: 0x04001534 RID: 5428
	public static Dictionary<string, Color> skyBoxTintDefinitions = new Dictionary<string, Color>();

	// Token: 0x04001535 RID: 5429
	public static Dictionary<RarityLevelEnum, Material> rarityBorderMaterialsEnabled = new Dictionary<RarityLevelEnum, Material>();

	// Token: 0x04001536 RID: 5430
	public static Dictionary<RarityLevelEnum, Material> rarityBorderMaterialsDisabled = new Dictionary<RarityLevelEnum, Material>();

	// Token: 0x04001537 RID: 5431
	public static TilePool tilePool;

	// Token: 0x04001538 RID: 5432
	public static TilePool modelTilePool;

	// Token: 0x04001539 RID: 5433
	public Vector2 storeFogDistances = new Vector2(150f, 400f);

	// Token: 0x0400153A RID: 5434
	private UIMain _ui;

	// Token: 0x0400153B RID: 5435
	private Color _defaultBackgroundColor;

	// Token: 0x0400153C RID: 5436
	public static readonly float defaultGravityStrength = 20f;

	// Token: 0x0400153D RID: 5437
	private static VRType VR_Default = VRType.VR_None;

	// Token: 0x0400153E RID: 5438
	public static bool vrEnabled = false;

	// Token: 0x0400153F RID: 5439
	private static VRType currentVRType = Blocksworld.VR_Default;

	// Token: 0x04001540 RID: 5440
	private static GameObject vrCameraAdjust;

	// Token: 0x04001541 RID: 5441
	public Transform cameraTiltTransform;

	// Token: 0x04001542 RID: 5442
	public Transform cameraParentTransform;

	// Token: 0x04001543 RID: 5443
	internal static bool worldSessionHadVR = false;

	// Token: 0x04001544 RID: 5444
	internal static bool worldSessionHadBlocksterMover = false;

	// Token: 0x04001545 RID: 5445
	internal static bool worldSessionHadBlocksterSpeaker = false;

	// Token: 0x04001546 RID: 5446
	internal static bool worldSessionHadBlockTap = false;

	// Token: 0x04001547 RID: 5447
	internal static int worldSessionCoinsCollected = 0;

	// Token: 0x04001548 RID: 5448
	internal static bool worldSessionHadHypderjumpUse = false;

	// Token: 0x04001549 RID: 5449
	private static State _currentState = State.Build;

	// Token: 0x0400154A RID: 5450
	private static State _lastState;

	// Token: 0x0400154B RID: 5451
	private static float _stateTime;

	// Token: 0x0400154C RID: 5452
	private bool autoLoad;

	// Token: 0x0400154D RID: 5453
	private HudMeshLabel labelTimer;

	// Token: 0x0400154E RID: 5454
	private List<HudMeshLabel> labelCounters = new List<HudMeshLabel>();

	// Token: 0x0400154F RID: 5455
	private static string[] counterNames = new string[]
	{
		"0",
		"1",
		"2"
	};

	// Token: 0x04001550 RID: 5456
	private Block lastBuildModeSelectedBlock;

	// Token: 0x04001551 RID: 5457
	private Bunch lastBuildModeSelectedBunch;

	// Token: 0x04001552 RID: 5458
	private static HashSet<Predicate> analogStickPredicates;

	// Token: 0x04001553 RID: 5459
	private static HashSet<Predicate> tiltMoverPredicates;

	// Token: 0x04001554 RID: 5460
	private bool keyboardPasteInProgress;

	// Token: 0x04001555 RID: 5461
	private Vector3 wasdeqMouseCamPosTarget;

	// Token: 0x04001556 RID: 5462
	private Vector3 wasdeqMouseCamLookAtTarget;

	// Token: 0x04001557 RID: 5463
	private bool cameraSelectionOrbitMode;

	// Token: 0x04001558 RID: 5464
	private float lastControlPressTime = -1f;

	// Token: 0x04001559 RID: 5465
	private int controlPressesInShortTime;

	// Token: 0x0400155A RID: 5466
	private float lastQuickScrollPressTime = -1f;

	// Token: 0x0400155B RID: 5467
	private List<int> quickScrollKeys = new List<int>();

	// Token: 0x0400155C RID: 5468
	private Dictionary<string, int> materialUsage = new Dictionary<string, int>();

	// Token: 0x0400155D RID: 5469
	private static HashSet<Predicate> taggedPredicates = null;

	// Token: 0x0400155E RID: 5470
	public static Dictionary<string, HashSet<string>> uniqueBlockMap = null;

	// Token: 0x0400155F RID: 5471
	private static HashSet<GAF> uniqueGafs;

	// Token: 0x04001560 RID: 5472
	private static PredicateSet magnetPredicates = null;

	// Token: 0x04001561 RID: 5473
	private static PredicateSet taggedHandAttachmentPreds = null;

	// Token: 0x04001562 RID: 5474
	private static PredicateSet taggedArrowPreds = null;

	// Token: 0x04001563 RID: 5475
	private static PredicateSet taggedLaserPreds = null;

	// Token: 0x04001564 RID: 5476
	private static PredicateSet taggedProjectilePreds = null;

	// Token: 0x04001565 RID: 5477
	private static PredicateSet taggedWaterPreds = null;

	// Token: 0x04001566 RID: 5478
	private bool _showingOptionsWhenPaused;

	// Token: 0x04001567 RID: 5479
	private CopyModelAnimationCommand copyModelAnimationCommand = new CopyModelAnimationCommand();

	// Token: 0x04001568 RID: 5480
	private SaveModelAnimationCommand saveModelAnimationCommand = new SaveModelAnimationCommand();

	// Token: 0x04001569 RID: 5481
	private CopyScriptAnimationCommand copyScriptAnimationCommand = new CopyScriptAnimationCommand();

	// Token: 0x0400156A RID: 5482
	private const float DIRECT_RAYCAST_SCREEN_THRESHOLD = 0.35f;

	// Token: 0x0400156B RID: 5483
	private PasteModelAnimationCommand pasteModelAnimationCommand = new PasteModelAnimationCommand();

	// Token: 0x0400156C RID: 5484
	public static Dictionary<string, bool> luminousPaints = new Dictionary<string, bool>();
}
