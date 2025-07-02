using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using Blocks;
using Exdilin;
using Gestures;
using SimpleJSON;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class Blocksworld : MonoBehaviour
{
	public static float fixedDeltaTime;

	public static bool useCompactGafWriteRenamings;

	public static bool inBackground;

	public static Bunch selectedBunch;

	public static bool lockPull;

	public static List<NamedPose> cameraPoses;

	public static Dictionary<string, NamedPose> cameraPosesMap;

	public static bool staticLockPull;

	public static bool dynamicLockPull;

	public static bool lockInput;

	public static bool isFirstFrame;

	public static bool hideInGameUI;

	public static bool started;

	public static bool renderingShadows;

	public static bool renderingSkybox;

	public static bool renderingWater;

	public static float _lightIntensityBasic;

	public static float _lightIntensityMultiplier;

	public static Color _buildModeFogColor;

	public static float _buildModeFogStart;

	public static float _buildModeFogEnd;

	public static Clipboard clipboard;

	public static ModelCollection modelCollection;

	private static List<Command> updateCommands;

	private static List<Command> fixedUpdateCommands;

	private static List<Command> resetStateCommands;

	public static HashSet<string> unlockedPaints;

	public static bool resetting;

	public static bool loadComplete;

	public static bool isLoadingScene;

	public static bool resettingPlay;

	public static bool capturingScreenshot;

	public static Vector3 constrainedManipulationAxis;

	public static HashSet<Block> editorSelectionLocked;

	private static bool keyLReleased;

	private static Block recentSelectionUnlockedBlock;

	public static bool isUsingSmallScreen;

	public static string currentBackgroundMusic;

	private static int loadedTextureCountAfterRemovingAssets;

	private static int loadedBlockCountAfterRemovingAssets;

	public static bool interpolateRigidBodies;

	public const int sizeTile = 80;

	public const int sizeTileMesh = 75;

	public static int marginTile;

	public static int defaultPanelPadding;

	public const int buildPanelPadding = 20;

	public static float fogMultiplier;

	public static float fogStart;

	public static float fogEnd;

	public static Color fogColor;

	public static HashSet<string> existingBlockNames;

	public static Dictionary<string, GameObject> prefabs;

	public static Dictionary<string, GameObject> goPrefabs;

	public static Dictionary<string, Mesh> meshes;

	public static Dictionary<string, Collider> colliders;

	public static Dictionary<string, GameObject> compoundColliders;

	public static Dictionary<string, GameObject> shapes;

	public static Dictionary<string, GameObject> glues;

	public static Dictionary<string, GameObject> joints;

	public static List<GAF> globalGafs;

	public static List<GAF> rewardVisualizationGafs;

	private static int rewardVisualizationIndex;

	private static ScriptRowExecutionInfo rewardExecutionInfo;

	public static bool winIsWaiting;

	public static bool hasWon;

	public static bool stopASAP;

	public static bool waitForSetPurchase;

	private static bool canSaveInMenu;

	public static bool worldSaveEnabled;

	private static bool f3PressedInCurrentWorld;

	public static Blocksworld bw;

	public static GameObject blocksworldDataContainer;

	protected static Camera _mainCamera;

	private static Camera _mainCameraOverrideBackup;

	public static Transform cameraTransform;

	public static Vector3 cameraPosition;

	public static Vector3 cameraForward;

	public static Vector3 cameraUp;

	public static Vector3 cameraRight;

	public static Camera guiCamera;

	public static Camera rewardCamera;

	public static GUISkin skin;

	public static GameObject directionalLight;

	public static Light overheadLight;

	public static Transform lightingRig;

	public static Texture buttonPlus;

	public static Texture buttonMinus;

	public static BlockSky worldSky;

	public static GameObject worldOcean;

	public static BlockWater worldOceanBlock;

	private int whiteBackground;

	public static GameObject prefabArrow;

	public ParticleSystem explosion;

	public static ParticleSystem stars;

	public static ParticleSystem starsReward;

	public static GameObject rewardStarburst;

	public static SfxDefinitions sfxDefinitions;

	public static EngineSoundDefinitions engineSoundDefinitions;

	public static LeaderboardData leaderboardData;

	public static BlocksworldComponentData componentData;

	public static readonly BlocksworldCamera blocksworldCamera;

	public static Vector3 prevCamPos;

	public static bool cameraMoved;

	public static Color lightColor;

	public static Color dynamicLightColor;

	public static float dynamicLightIntensityMultiplier;

	public static List<ILightChanger> dynamicalLightChangers;

	public static WeatherEffect weather;

	public static FpsCounter fpsCounter;

	private Vector3 mousePositionLast = Vector3.zero;

	private Vector3 mousePositionDelta;

	public static Vector3 mousePositionFirst;

	public static BuildPanel buildPanel;

	public static ScriptPanel scriptPanel;

	public static Tile tileButtonClearScript;

	public static Tile tileButtonCopyScript;

	public static Tile tileButtonPasteScript;

	public static HashSet<GAF> publicProvidedGafs;

	private float offsetMenu;

	public static List<GAF> enabledGAFs;

	public static List<GAF> enabledPanelBlock;

	private static string displayString;

	public static List<GAF> unlockedGAFs;

	public static HashSet<GAF> sidePanelGafs;

	public static Dictionary<OldSymbol, float> joysticks;

	public TileParameterEditor tileParameterEditor;

	public static Tile selectedTile;

	public bool forcePlayMode;

	public static float lastRealtimeSinceStartup;

	public static float deltaTime;

	public static bool hd;

	public static float screenScale;

	public static int screenWidth;

	public static int screenHeight;

	public static Vector3 mouse;

	public static int numTouches;

	public static Vector3[] touches;

	public static Block mouseBlock;

	public static int mouseBlockIndex;

	public static Vector3 mouseBlockNormal;

	public static Vector3 mouseBlockHitPosition;

	public static Block mouseBlockLast;

	public bool tWidgetHit;

	public static bool tWidgetHitAtStart;

	public static bool tBoxHit;

	public static bool tBoxHitAtStart;

	public static Vector3 mouseBlockNormalLast;

	public static Block selectedBlock;

	private Vector3 anglesDelta;

	private Quaternion rotationStart;

	private Vector3 textureNormalLast;

	private float orbit;

	private static string stringInput;

	private static bool consumeEvent;

	public LineRenderer pullObjectLineRenderer;

	public static GestureRecognizer recognizer;

	private BaseGesture[] buildOnlyGestures;

	private SecretCommandGesture autoPlayGesture;

	private PullObjectGesture pullObject;

	public TapControlGesture tapControl;

	public ParameterEditGesture parameterEditGesture;

	public static UIGesture uiGesture;

	public static TBoxGesture tBoxGesture;

	public static TileDragGesture tileDragGesture;

	public static CreateTileDragGesture createTileDragGesture;

	public static ReplaceBodyPartTileDragGesture replaceBodyPartGesture;

	public static CharacterEditGearGesture characterEditGearGesture;

	public static CWidgetGesture cWidgetGesture;

	public static BlockDuplicateGesture blockDupeGesture;

	public static BlockTapGesture blockTapGesture;

	public static ButtonTapGesture buttonTapGesture;

	public static OrbitDuringControlCameraGesture orbitDuringControlGesture;

	private Vector3 mouseStart;

	public static string currentWorldId;

	public static bool launchIntoPlayMode;

	private JObject lastLoadedCameraObj;

	public static float angularDragMultiplier;

	public static float dragMultiplier;

	public static List<Block> locked;

	public static List<Chunk> chunks;

	public static float timerStart;

	public static float timerStop;

	public static bool gameStart;

	public const string SIGNAL_NAMES = "signal-names";

	public const string BLOCK_NAMES = "block-names";

	public const string CAMERA_POSES = "camera-poses";

	public const string PUZZLE_GAFS = "puzzle-gafs";

	public const string PUZZLE_GAF_USAGE = "puzzle-gaf-usage";

	public const string PUZZLE_GAFS_AFTER_STEP_BY_STEP = "puzzle-gafs-after-step-by-step";

	public const string PUZZLE_GAF_USAGE_AFTER_STEP_BY_STEP = "puzzle-gaf-usage-after-step-by-step";

	public const int SIGNAL_COUNT = 26;

	public const int MAX_PLAYER_COUNT = 8;

	public const int MAX_COUNTERS = 2;

	public const int MAX_TIMERS = 2;

	public const int MAX_GAUGES = 2;

	public const int MAX_RADARS = 1;

	public static bool[] sending;

	public static float[] sendingValues;

	public static string[] signalNames;

	public const string GENERIC_WILDCARD_NAME = "*";

	public const string DEFAULT_CUSTOM_SIGNAL_NAME = "Signal";

	public const string DEFAULT_CUSTOM_INT_VARIABLE_NAME = "Int";

	public static Dictionary<string, float> sendingCustom;

	public static Dictionary<Block, string> blockNames;

	public static int numSendTilesInUse;

	public static int numTagTilesInUse;

	public static HashSet<string> everPresentTagsInUse;

	public static HashSet<string> arrowTags;

	public static HashSet<string> handAttachmentTags;

	public static HashSet<string> laserTags;

	public static HashSet<string> projectileTags;

	public static HashSet<string> waterTags;

	public static HashSet<string> customSignals;

	public static Dictionary<string, int> customIntVariables;

	public static Dictionary<Block, Dictionary<string, int>> blockIntVariables;

	public static float gameTime;

	public static int updateCounter;

	public static int playFixedUpdateCounter;

	public static int numCounters;

	public static Dictionary<string, bool> countersActivated;

	public static Dictionary<string, int> counters;

	public static Dictionary<string, int> counterTargets;

	public static Dictionary<string, bool> counterTargetsActivated;

	public static Texture starOutlineTexture;

	public static Texture starTexture;

	public static Plane[] frustumPlanes;

	public static float maxBlockTapDistance;

	public static float maxBlockDragDistance;

	public static MusicPlayer musicPlayer;

	public static Dictionary<string, string> iconColors;

	public static Dictionary<string, Color[]> colorDefinitions;

	public static Dictionary<string, Color[]> ambientLightGradientDefinitions;

	public static Dictionary<string, Color> skyBoxTintDefinitions;

	public static Dictionary<RarityLevelEnum, Material> rarityBorderMaterialsEnabled;

	public static Dictionary<RarityLevelEnum, Material> rarityBorderMaterialsDisabled;

	public static TilePool tilePool;

	public static TilePool modelTilePool;

	public Vector2 storeFogDistances = new Vector2(150f, 400f);

	private UIMain _ui;

	private Color _defaultBackgroundColor;

	public static readonly float defaultGravityStrength;

	private static VRType VR_Default;

	public static bool vrEnabled;

	private static VRType currentVRType;

	private static GameObject vrCameraAdjust;

	public Transform cameraTiltTransform;

	public Transform cameraParentTransform;

	internal static bool worldSessionHadVR;

	internal static bool worldSessionHadBlocksterMover;

	internal static bool worldSessionHadBlocksterSpeaker;

	internal static bool worldSessionHadBlockTap;

	internal static int worldSessionCoinsCollected;

	internal static bool worldSessionHadHypderjumpUse;

	private static State _currentState;

	private static State _lastState;

	private static float _stateTime;

	private bool autoLoad;

	private HudMeshLabel labelTimer;

	private List<HudMeshLabel> labelCounters = new List<HudMeshLabel>();

	private static string[] counterNames;

	private Block lastBuildModeSelectedBlock;

	private Bunch lastBuildModeSelectedBunch;

	private static HashSet<Predicate> analogStickPredicates;

	private static HashSet<Predicate> tiltMoverPredicates;

	private bool keyboardPasteInProgress;

	private Vector3 wasdeqMouseCamPosTarget;

	private Vector3 wasdeqMouseCamLookAtTarget;

	private bool cameraSelectionOrbitMode;

	private float lastControlPressTime = -1f;

	private int controlPressesInShortTime;

	private float lastQuickScrollPressTime = -1f;

	private List<int> quickScrollKeys = new List<int>();

	private Dictionary<string, int> materialUsage = new Dictionary<string, int>();

	private static HashSet<Predicate> taggedPredicates;

	public static Dictionary<string, HashSet<string>> uniqueBlockMap;

	private static HashSet<GAF> uniqueGafs;

	private static PredicateSet magnetPredicates;

	private static PredicateSet taggedHandAttachmentPreds;

	private static PredicateSet taggedArrowPreds;

	private static PredicateSet taggedLaserPreds;

	private static PredicateSet taggedProjectilePreds;

	private static PredicateSet taggedWaterPreds;

	private bool _showingOptionsWhenPaused;

	private CopyModelAnimationCommand copyModelAnimationCommand = new CopyModelAnimationCommand();

	private SaveModelAnimationCommand saveModelAnimationCommand = new SaveModelAnimationCommand();

	private CopyScriptAnimationCommand copyScriptAnimationCommand = new CopyScriptAnimationCommand();

	private const float DIRECT_RAYCAST_SCREEN_THRESHOLD = 0.35f;

	private PasteModelAnimationCommand pasteModelAnimationCommand = new PasteModelAnimationCommand();

	public static Dictionary<string, bool> luminousPaints;

	public static Camera mainCamera => _mainCamera;

	public static UIMain UI => bw._ui;

	public static State CurrentState => _currentState;

	private static State LastState => _lastState;

	public static void SetBlocksworldState(State nextState)
	{
		_lastState = _currentState;
		_currentState = nextState;
		_stateTime = 0f;
		musicPlayer.SetEnabled(IsMusicEnabledForState());
	}

	public static float TimeInCurrentState()
	{
		return _stateTime;
	}

	public static bool IsMusicEnabledForState()
	{
		return CurrentState != State.Background;
	}

	public static void Bootstrap(GameObject go)
	{
		Component component = go.GetComponent("BlocksworldComponentData");
		guiCamera = (Camera)component.GetType().GetField("guiCamera").GetValue(component);
		rewardCamera = (Camera)component.GetType().GetField("rewardCamera").GetValue(component);
		buttonPlus = (Texture)component.GetType().GetField("buttonPlus").GetValue(component);
		buttonMinus = (Texture)component.GetType().GetField("buttonMinus").GetValue(component);
		prefabArrow = (GameObject)component.GetType().GetField("prefabArrow").GetValue(component);
		stars = (ParticleSystem)component.GetType().GetField("stars").GetValue(component);
		starsReward = (ParticleSystem)component.GetType().GetField("starsReward").GetValue(component);
		Blocksworld blocksworld = go.AddComponent<Blocksworld>();
		blocksworld.autoLoad = BWStandalone.Instance == null;
	}

	public static string DefaultProfileWorldAssetPath()
	{
		return "ProfileWorlds/profile_world_source_anim_male";
	}

	private void Awake()
	{
		BWLog.Info("Awaking Blocksworld..");
		bw = this;
		cameraTransform = GameObject.Find("Camera Holder").transform;
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("Main Camera")) as GameObject;
		_mainCamera = gameObject.GetComponent<Camera>();
		_defaultBackgroundColor = _mainCamera.backgroundColor;
		mainCamera.transform.parent = cameraTransform;
		mainCamera.transform.position = cameraTransform.position;
		mainCamera.transform.rotation = cameraTransform.rotation;
		blocksworldDataContainer = GameObject.Find("BlocksworldData");
		sfxDefinitions = blocksworldDataContainer.GetComponent<SfxDefinitions>();
		engineSoundDefinitions = blocksworldDataContainer.GetComponent<EngineSoundDefinitions>();
		componentData = blocksworldDataContainer.GetComponent<BlocksworldComponentData>();
		leaderboardData = blocksworldDataContainer.GetComponent<LeaderboardData>();
		Shader.globalMaximumLOD = 1000;
		renderingShadows = QualitySettings.shadows != ShadowQuality.Disable;
		renderingWater = true;
		renderingSkybox = true;
		_mainCamera.clearFlags = CameraClearFlags.Skybox;
		AmplifyOcclusionEffect component = gameObject.GetComponent<AmplifyOcclusionEffect>();
		if (component != null)
		{
			component.enabled = renderingShadows;
		}
		BWFog component2 = gameObject.GetComponent<BWFog>();
		if (component2 != null)
		{
			component2.enabled = renderingShadows;
		}
		BWLog.Info("Done awaking Blocksworld!");
	}

	private IEnumerator Start()
	{
		yield return null;
		TiltManager.Instance.Init();
		PerformaceTest speedTest = new PerformaceTest("Load Main Scene");
		speedTest.Start();
		screenScale = NormalizedScreen.scale;
		hd = screenScale > 1f;
		BWLog.Info("Creating UI..");
		_ui = UIMain.CreateUI();
		UI.gameObject.SetActive(value: false);
		SetupCameraHierarchy();
		yield return null;
		fixedDeltaTime = Time.fixedDeltaTime;
		tileParameterEditor = base.gameObject.AddComponent<TileParameterEditor>();
		tileParameterEditor.enabled = false;
		SymbolCompat.Init();
		BWLog.Info("Initializing settings..");
		useCompactGafWriteRenamings = Options.UseCompactGafWriteRenamings;
		screenWidth = NormalizedScreen.width;
		screenHeight = NormalizedScreen.height;
		skin = LoadSkin(hd);
		guiCamera.transform.position = new Vector3(screenWidth / 2, screenHeight / 2, -1f);
		guiCamera.orthographicSize = screenHeight / 2;
		guiCamera.transparencySortMode = TransparencySortMode.Orthographic;
		speedTest.StartSubTest("ReadScarcityInfo");
		Scarcity.ReadScarcityInfo();
		speedTest.StopSubTest();
		BWLog.Info("Initializing music..");
		musicPlayer = MusicPlayer.Create();
		UpdateEditorMusicPlayerEnabled();
		ResourceLoader.UpdateTextureInfos();
		SetupColorDefinitions(colorDefinitions, ambientLightGradientDefinitions, skyBoxTintDefinitions);
		SetupRarityBorders(rarityBorderMaterialsEnabled, rarityBorderMaterialsDisabled);
		SetupTextureMetaDatas();
		speedTest.StartSubTest("RegisterBlocks");
		BWLog.Info("Registering blocks..");
		RegisterBlocks();
		foreach (Mod mod in ModLoader.mods)
		{
			(Mod.ExecutionMod = mod).Register(RegisterType.BLOCKS);
			Mod.ExecutionMod = null;
		}
		speedTest.StopSubTest();
		BlockItem.LoadBlockItemsFromResources();
		PanelSlots.LoadPanelSlotsFromResources();
		CollisionTest.ReadNoShapeCollides();
		PredicateRegistry.UpdateEquivalentPredicates();
		modelCollection = new ModelCollection();
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
		globalGafs = GAF.GetAllGlobalGAFs();
		yield return null;
		BWLog.Info("Loading tile data..");
		speedTest.StartSubTest("LoadTileData");
		foreach (Mod mod2 in ModLoader.mods)
		{
			(Mod.ExecutionMod = mod2).Register(RegisterType.BLOCK_ITEMS);
			Mod.ExecutionMod = null;
		}
		TileIconManager.Init();
		speedTest.StopSubTest();
		TileToggleChain.LoadTileToggleChains();
		speedTest.StartSubTest("UpdateTileParameterSettings");
		Tile.CreateAntigravityArgumentConverters();
		foreach (Mod mod3 in ModLoader.mods)
		{
			mod3.Register(RegisterType.TILE_PARAMETERS);
		}
		Tile.UpdateTileParameterSettings();
		speedTest.StopSubTest();
		BWLog.Info("Creating tile pools..");
		tilePool = new TilePool("Blocksworld", 256, TilePool.TileImageSource.Resources);
		modelTilePool = new TilePool("ModelTiles", 256, TilePool.TileImageSource.StandaloneImageManger);
		yield return null;
		EntityTagsRegistry.Read();
		tileButtonClearScript = new Tile(tilePool.GetTileObjectForIcon("Buttons/Clear_Script", enabled: true));
		tileButtonCopyScript = new Tile(tilePool.GetTileObjectForIcon("Buttons/Copy_Script", enabled: true));
		tileButtonPasteScript = new Tile(tilePool.GetTileObjectForIcon("Buttons/Paste_Script", enabled: true));
		speedTest.StartSubTest("create build panel");
		int columns = 3;
		if (BW.isUnityEditor)
		{
			columns = Options.PanelColumnCount;
			columns = ((columns != 0) ? Mathf.Clamp(columns, 2, 20) : 3);
		}
		BWLog.Info("Creating build panel..");
		buildPanel = new BuildPanel("Build Panel", columns);
		buildPanel.UpdatePosition();
		buildPanel.depth = 20f;
		speedTest.StopSubTest();
		BWLog.Info("Creating script panel..");
		scriptPanel = new ScriptPanel("Script Panel");
		scriptPanel.depth = 10f;
		scriptPanel.Show(show: false);
		SetupClipboard();
		CharacterStateHandler.ClearStateMap();
		BWLog.Info("Loading character data..");
		TextAsset textAsset = Resources.Load<TextAsset>("StateHandler/Character");
		if (null != textAsset)
		{
			CharacterStateHandler.LoadStateMap(textAsset.text);
		}
		TextAsset textAsset2 = Resources.Load<TextAsset>("StateHandler/UpperBody");
		if (null != textAsset2)
		{
			UpperBodyStateHandler.LoadStateMap(textAsset2.text);
		}
		for (CharacterRole characterRole = CharacterRole.Male; characterRole < CharacterRole.None; characterRole++)
		{
			textAsset = Resources.Load<TextAsset>("StateHandler/Character" + characterRole);
			if (null != textAsset)
			{
				CharacterStateHandler.LoadStateMap(textAsset.text, characterRole);
			}
			textAsset2 = Resources.Load<TextAsset>("StateHandler/UpperBody" + characterRole);
			if (null != textAsset2)
			{
				UpperBodyStateHandler.LoadStateMap(textAsset2.text, characterRole);
			}
		}
		yield return null;
		BWLog.Info("Creating GUI..");
		WorldUILayout.Init();
		blocksworldCamera.Init();
		InitializeLights();
		Tutorial.Init();
		TBox.Init();
		buildPanel.PositionReset();
		speedTest.StartSubTest("HudMesh.Init");
		HudMeshOnGUI.Init();
		speedTest.StopSubTest();
		rewardStarburst = UnityEngine.Object.Instantiate(Resources.Load("Blocks/Block Starburst System")) as GameObject;
		rewardStarburst.SetLayer(Layer.Rewards);
		uiGesture = new UIGesture();
		blockDupeGesture = new BlockDuplicateGesture(BlockDuplicateBegan, BlockDuplicated);
		blockTapGesture = new BlockTapGesture(BlockTapBegan, BlockTapped);
		buttonTapGesture = new ButtonTapGesture(buildPanel);
		tileDragGesture = new TileDragGesture(buildPanel, scriptPanel);
		createTileDragGesture = new CreateTileDragGesture(buildPanel);
		replaceBodyPartGesture = new ReplaceBodyPartTileDragGesture(buildPanel);
		characterEditGearGesture = new CharacterEditGearGesture(buildPanel);
		PanelScrollGesture panelScrollGesture = new PanelScrollGesture(buildPanel, scriptPanel);
		PanelMoveGesture panelMoveGesture = new PanelMoveGesture(scriptPanel);
		pullObjectLineRenderer = mainCamera.GetComponent<LineRenderer>();
		pullObject = new PullObjectGesture(pullObjectLineRenderer);
		autoPlayGesture = new SecretCommandGesture();
		tapControl = new TapControlGesture();
		tBoxGesture = new TBoxGesture();
		cWidgetGesture = new CWidgetGesture();
		parameterEditGesture = new ParameterEditGesture();
		TileTapGesture tileTapGesture = new TileTapGesture(tileButtonClearScript, ButtonClearScriptTapped);
		tileTapGesture.SetExtendedHit(-10f, -10f, -10f, -10f);
		TileTapGesture tileTapGesture2 = new TileTapGesture(tileButtonCopyScript, ButtonCopyScriptTapped);
		tileTapGesture2.SetExtendedHit(-10f, -10f, -10f, -10f);
		TileTapGesture tileTapGesture3 = new TileTapGesture(tileButtonPasteScript, ButtonPasteScriptTapped);
		tileTapGesture3.SetExtendedHit(-10f, -10f, -10f, -10f);
		Tile tile = new Tile(TBox.tileCharacterEditIcon);
		TileTapGesture tileTapGesture4 = new TileTapGesture(tile, CharacterEditTapped);
		Tile tile2 = new Tile(TBox.tileCharacterEditExitIcon);
		TileTapGesture tileTapGesture5 = new TileTapGesture(tile2, CharacterEditExitTapped);
		List<BaseGesture> list = new List<BaseGesture>
		{
			autoPlayGesture, parameterEditGesture, panelScrollGesture, tileTapGesture, tileTapGesture2, tileTapGesture3, panelMoveGesture, replaceBodyPartGesture, characterEditGearGesture, createTileDragGesture,
			tileDragGesture, buttonTapGesture
		};
		BaseGesture[] array = list.ToArray();
		orbitDuringControlGesture = new OrbitDuringControlCameraGesture();
		BWLog.Info("Creating mapped input..");
		MappedInput.Init();
		BWLog.Info("Loading key maps..");
		TextAsset textAsset3 = Resources.Load<TextAsset>("KeyMaps/default_keymap_build");
		if (null != textAsset3)
		{
			MappedInput.ClearInputMap(MappableInputMode.Build);
			if (!MappedInput.AddInputMap(textAsset3.text, MappableInputMode.Build))
			{
				BWLog.Error("Unable to add default build keymap!");
			}
		}
		TextAsset textAsset4 = Resources.Load<TextAsset>("KeyMaps/default_keymap_play");
		if (null != textAsset4)
		{
			MappedInput.ClearInputMap(MappableInputMode.Play);
			if (!MappedInput.AddInputMap(textAsset4.text, MappableInputMode.Play))
			{
				BWLog.Error("Unable to add default play keymap!");
			}
		}
		TextAsset textAsset5 = Resources.Load<TextAsset>("KeyMaps/default_keymap_menu");
		if (null != textAsset5)
		{
			MappedInput.ClearInputMap(MappableInputMode.Menu);
			if (!MappedInput.AddInputMap(textAsset5.text, MappableInputMode.Menu))
			{
				BWLog.Error("Unable to add default menu keymap!");
			}
		}
		MappedInput.SetMode(MappableInputMode.Menu);
		BaseGesture[] array2 = new BaseGesture[1] { orbitDuringControlGesture };
		BaseGesture[] array3 = new BaseGesture[8] { blockDupeGesture, blockTapGesture, tapControl, pullObject, tBoxGesture, cWidgetGesture, tileTapGesture4, tileTapGesture5 };
		recognizer.AddGesture(uiGesture);
		BaseGesture[][] array4 = new BaseGesture[3][] { array, array2, array3 };
		foreach (BaseGesture[] array5 in array4)
		{
			BaseGesture[] array6 = array5;
			foreach (BaseGesture gesture in array6)
			{
				recognizer.AddGesture(gesture);
			}
		}
		recognizer.CancelsAll(uiGesture, array);
		recognizer.CancelsAll(uiGesture, array3);
		recognizer.AnyCancelsAll(array, array2);
		recognizer.AnyCancelsAll(array, array3);
		recognizer.AnyCancelsAll(array2, array3);
		recognizer.CancelsAll(parameterEditGesture, new BaseGesture[6] { panelScrollGesture, panelMoveGesture, createTileDragGesture, replaceBodyPartGesture, characterEditGearGesture, tileDragGesture });
		recognizer.Cancels(tBoxGesture, tileDragGesture);
		recognizer.Cancels(replaceBodyPartGesture, createTileDragGesture);
		recognizer.Cancels(replaceBodyPartGesture, tileDragGesture);
		recognizer.Cancels(characterEditGearGesture, createTileDragGesture);
		recognizer.Cancels(characterEditGearGesture, tileDragGesture);
		recognizer.Cancels(createTileDragGesture, tileDragGesture);
		recognizer.Cancels(replaceBodyPartGesture, characterEditGearGesture);
		recognizer.Cancels(buttonTapGesture, panelScrollGesture);
		recognizer.Cancels(tileDragGesture, panelMoveGesture);
		recognizer.Cancels(tBoxGesture, blockTapGesture);
		recognizer.Cancels(tileTapGesture4, blockTapGesture);
		recognizer.Cancels(tileTapGesture5, blockTapGesture);
		buildOnlyGestures = new BaseGesture[7] { createTileDragGesture, tileDragGesture, replaceBodyPartGesture, characterEditGearGesture, blockDupeGesture, blockTapGesture, autoPlayGesture };
		for (int k = 1; k <= numCounters; k++)
		{
			string key = k + string.Empty;
			countersActivated.Add(key, value: false);
			counters.Add(key, 0);
			counterTargets.Add(key, 1);
			counterTargetsActivated.Add(key, value: false);
		}
		starOutlineTexture = Resources.Load("Particles/Counter Star Disabled") as Texture;
		starTexture = Resources.Load("Particles/Counter Star Enabled") as Texture;
		History.activated = true;
		started = true;
		BlockGroups.Init();
		if (BW.isIPad)
		{
			IOSInterface.BlocksworldSceneLoaded();
		}
		BWLog.Info("Finished loading scene.");
		isLoadingScene = false;
		loadComplete = true;
		speedTest.Stop();
		speedTest.DebugLogTestResults();
		foreach (Mod mod4 in ModLoader.mods)
		{
			(Mod.ExecutionMod = mod4).Register(RegisterType.SETTINGS);
			Mod.ExecutionMod = null;
		}
		foreach (Mod mod5 in ModLoader.mods)
		{
			(Mod.ExecutionMod = mod5).Register(RegisterType.TEXTURES);
			Mod.ExecutionMod = null;
		}
		BWLog.Info("Initing mods..");
		foreach (Mod mod6 in ModLoader.mods)
		{
			(Mod.ExecutionMod = mod6).Init();
			Mod.ExecutionMod = null;
		}
		StartCoroutine(ModLoader.ShowErrors());
		BlockItemEntry[] itemEntries = BlockItemsRegistry.GetItemEntries();
		foreach (BlockItemEntry blockItemEntry in itemEntries)
		{
			BWUser.currentUser.blocksInventory.Add(blockItemEntry.item.Id, blockItemEntry.count, blockItemEntry.infinite ? 1 : 0);
		}
		BWLog.Info("Finished loading.");
	}

	private void InitializeLights()
	{
		directionalLight = GameObject.Find("Directional light");
		GameObject gameObject = GameObject.Find("Lighting Rig");
		if (gameObject != null)
		{
			if (renderingShadows)
			{
				lightingRig = gameObject.transform;
				Light[] componentsInChildren = lightingRig.GetComponentsInChildren<Light>(includeInactive: true);
				if (componentsInChildren.Length != 0)
				{
					overheadLight = componentsInChildren[0];
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						componentsInChildren[i].gameObject.SetActive(value: true);
					}
					directionalLight.SetActive(value: false);
				}
				else
				{
					gameObject.SetActive(value: false);
				}
			}
			else
			{
				gameObject.SetActive(value: false);
			}
		}
		else if (renderingShadows)
		{
			BWLog.Warning("No lighting rig found for dynamic lights!");
		}
	}

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

	public static List<Dependency> GetRequiredMods()
	{
		List<Dependency> list = new List<Dependency>();
		List<Block> list2 = BWSceneManager.AllBlocks();
		for (int i = 0; i < list2.Count; i++)
		{
			string text = list2[i].BlockType();
			BlockEntry blockEntry = BlockItemsRegistry.GetBlockEntry(text);
			if (blockEntry != null)
			{
				if (blockEntry.originator != null)
				{
					Dependency item = new Dependency(blockEntry.originator.Id, blockEntry.originator.Version);
					if (!list.Contains(item))
					{
						list.Add(item);
					}
				}
				else
				{
					BWLog.Warning("Unknown mod originator for block " + text);
				}
			}
			List<List<Tile>> tiles = list2[i].tiles;
			for (int j = 0; j < tiles.Count; j++)
			{
				List<Tile> list3 = tiles[j];
				for (int k = 0; k < list3.Count; k++)
				{
					Predicate predicate = list3[k].gaf.Predicate;
					if (predicate != Block.predicateTextureTo)
					{
						continue;
					}
					object[] args = list3[k].gaf.Args;
					string stringArg = Util.GetStringArg(args, 0, "Plain");
					Mapping mapping = Materials.GetMapping(stringArg);
					ShaderType shaderType = Materials.shaders[stringArg];
					Mod owner = AssetsManager.GetOwner("Textures/" + shaderType.ToString() + "/" + mapping.ToString() + "/" + stringArg);
					if (owner != null)
					{
						Dependency item2 = new Dependency(owner.Id, owner.Version);
						if (!list.Contains(item2))
						{
							list.Add(item2);
						}
					}
				}
			}
		}
		return list;
	}

	public static void SetupCameraOverride(Transform newCameraPrefab, Block sourceBlock)
	{
		if (_mainCameraOverrideBackup != null)
		{
			BWLog.Error("Only one camera override can be active at a time!");
			return;
		}
		TakedownVRCamera();
		_mainCamera.gameObject.SetActive(value: false);
		_mainCameraOverrideBackup = _mainCamera;
		Transform transform = UnityEngine.Object.Instantiate(newCameraPrefab).transform;
		_mainCamera = transform.GetComponent<Camera>();
		mainCamera.transform.parent = cameraTransform;
		mainCamera.transform.position = cameraTransform.position;
		mainCamera.transform.rotation = cameraTransform.rotation;
		NightVisionColoring component = transform.GetComponent<NightVisionColoring>();
		if ((bool)component)
		{
			component.SetSourceBlock(sourceBlock);
		}
		SetupVRCamera();
	}

	public static void ResetCameraOverride()
	{
		if (_mainCameraOverrideBackup != null)
		{
			TakedownVRCamera();
			UnityEngine.Object.Destroy(_mainCamera.gameObject);
			_mainCamera = _mainCameraOverrideBackup;
			_mainCamera.gameObject.SetActive(value: true);
			_mainCameraOverrideBackup = null;
			SetupVRCamera();
		}
	}

	public static void CleanupAndQuitToMenu()
	{
		Cleanup(quitToMenu: true);
		UI.gameObject.SetActive(value: false);
	}

	public static void Cleanup(bool quitToMenu)
	{
		if (CharacterEditor.Instance.InEditMode())
		{
			CharacterEditor.Instance.Exit();
		}
		try
		{
			CleanupScene();
			if (quitToMenu)
			{
				if (modelCollection != null)
				{
					modelCollection.Clear();
				}
				buildPanel.ClearAllTiles();
			}
			bw.StopAllCoroutines();
			ScreenshotUtils.OnStopAllCoroutines();
			prefabs.Clear();
			compoundColliders.Clear();
			shapes.Clear();
			glues.Clear();
			joints.Clear();
			leaderboardData.ClearWorldSession();
		}
		catch (Exception ex)
		{
			BWLog.Error("Exception in Blocksworld.CleanupAndQuitToMenu '" + ex.Message + "'");
		}
		finally
		{
			if (quitToMenu)
			{
				worldSessionHadVR = false;
				worldSessionHadBlocksterMover = false;
				worldSessionHadBlocksterSpeaker = false;
				worldSessionHadBlockTap = false;
				worldSessionCoinsCollected = 0;
				worldSessionHadHypderjumpUse = false;
				WorldSession.jumpRestoreConfig = null;
				WorldSession.current = null;
				bw.StartCoroutine(CoroutineWorldDidQuit());
			}
		}
	}

	private static IEnumerator CoroutineWorldDidQuit()
	{
		yield return null;
		WorldSession.platformDelegate.WorldDidQuit();
	}

	public void OpenEditorMenu()
	{
		SetBlocksworldState(State.Menu);
		canSaveInMenu = Tutorial.state == TutorialState.None && !f3PressedInCurrentWorld;
	}

	public void ToggleEditorMenu()
	{
		if (CurrentState == State.Build)
		{
			OpenEditorMenu();
			return;
		}
		Sound.PlayOneShotSound("Button Generic");
		SetBlocksworldState(State.Build);
	}

	public static void CleanupScene()
	{
		BWSceneManager.Cleanup();
		if (WorldSession.isPuzzleBuildSession() || WorldSession.isPuzzlePlaySession())
		{
			Tutorial.Stop();
		}
		TileIconManager.Instance.CancelAllFileLoads();
		winIsWaiting = false;
		hasWon = false;
		Tutorial.ResetState();
		bw.Stop(force: true, resetBlocks: false);
		BW.Analytics.SendAnalyticsEvent("world-exit");
		inBackground = true;
		if (BlockAbstractWater.waterCubes != null)
		{
			BlockAbstractWater.waterCubes.Clear();
		}
		BlockGroups.Clear();
		worldSky = null;
		worldOcean = null;
		worldOceanBlock = null;
		WeatherEffect.ResetAll();
		weather = WeatherEffect.clear;
		bw.Reset();
		TBox.tileCharacterEditIcon.Hide();
		BlockProceduralCollider.ClearReuseableMeshes();
		Physics.gravity = (0f - defaultGravityStrength) * Vector3.up;
		TileIconManager.Instance.labelAtlas.Clear();
		Materials.materialCache.Clear();
		Materials.materialCachePaint.Clear();
		Materials.materialCacheTexture.Clear();
		RemoveUnusedBlockPrefabs();
		RemoveUnusedTextures();
		RewardVisualization.Cancel();
		rewardVisualizationGafs = null;
		rewardVisualizationIndex = 0;
		rewardExecutionInfo.timer = 0f;
		UI.Tapedeck.Reset();
		UI.SidePanel.Hide();
		WorldUILayout.HideAll();
		musicPlayer.SetEnabled(enabled: false);
		if (Sound.oneShotAudioSource != null)
		{
			Sound.oneShotAudioSource.Stop();
			Sound.oneShotAudioSource.clip = null;
		}
		BlockAnimatedCharacter.stateControllers.Clear();
		SetBlocksworldState(State.Background);
	}

	public static void EnableGameCameras(bool enableCams)
	{
		mainCamera.gameObject.SetActive(enableCams);
		mainCamera.transform.parent = bw.cameraParentTransform;
		mainCamera.transform.position = cameraTransform.position;
		mainCamera.transform.rotation = cameraTransform.rotation;
		guiCamera.gameObject.SetActive(enableCams);
	}

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

	public static bool IsStarted()
	{
		return started;
	}

	public static void SetupClipboard()
	{
		if (clipboard == null)
		{
			clipboard = new Clipboard();
			clipboard.Load();
		}
		else
		{
			UI.QuickSelect.UpdateIcons();
		}
	}

	public void InsertTile(Block block, int x, int y, Tile tile)
	{
		block.tiles[y].Insert(x, tile);
		if (block == selectedBlock)
		{
			scriptPanel.UpdateGestureRecognizer(recognizer);
		}
	}

	public static Block GetSelectedScriptBlock()
	{
		if (selectedBlock != null)
		{
			if (!(selectedBlock is BlockGrouped blockGrouped) || blockGrouped.GroupHasIndividualSripting())
			{
				return selectedBlock;
			}
			return blockGrouped.GetMainBlockInGroup();
		}
		if (selectedBunch != null && SelectedBunchIsGroup())
		{
			return (selectedBunch.blocks[0] as BlockGrouped).GetMainBlockInGroup();
		}
		return null;
	}

	public static bool IsUnlocked(Tile tile)
	{
		return true;
	}

	public static void UpdateCenterOfMasses()
	{
		foreach (Chunk chunk in chunks)
		{
			chunk.UpdateCenterOfMass();
		}
		foreach (Chunk chunk2 in chunks)
		{
			foreach (Block block in chunk2.blocks)
			{
				if (block is BlockAbstractMotor blockAbstractMotor)
				{
					blockAbstractMotor.CalculateMassDistributions();
				}
			}
		}
	}

	private static void UnloadIfNotNull(GameObject g)
	{
		if (g != null)
		{
			Resources.UnloadAsset(g);
		}
	}

	public static void UnloadBlock(string resource, HashSet<string> usedMeshes)
	{
		if (meshes.ContainsKey(resource) && meshes[resource] != null)
		{
			if (!usedMeshes.Contains(resource))
			{
				Mesh obj = meshes[resource];
				UnityEngine.Object.Destroy(obj);
			}
			meshes.Remove(resource);
			foreach (object item in goPrefabs[resource].transform)
			{
				Transform transform = (Transform)item;
				string text = transform.gameObject.name;
				meshes.Remove(text);
				if (!usedMeshes.Contains(text) && meshes.ContainsKey(text))
				{
					Mesh obj2 = meshes[text];
					UnityEngine.Object.Destroy(obj2);
				}
			}
		}
		colliders.Remove(resource);
		if (goPrefabs.ContainsKey(resource))
		{
			if (glues.ContainsKey(resource))
			{
				CollisionVolumes.Remove(glues[resource]);
			}
			if (shapes.ContainsKey(resource))
			{
				CollisionVolumes.Remove(shapes[resource]);
			}
			if (joints.ContainsKey(resource))
			{
				CollisionVolumes.Remove(joints[resource]);
			}
			if (compoundColliders.ContainsKey(resource))
			{
				CollisionVolumes.Remove(compoundColliders[resource]);
			}
			UnityEngine.Object.Destroy(goPrefabs[resource]);
			goPrefabs.Remove(resource);
		}
	}

	public static GameObject InstantiateBlockGo(string blockType)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(prefabs[blockType]);
		gameObject.SetActive(value: true);
		MeshRenderer[] componentsInChildren = gameObject.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer meshRenderer in componentsInChildren)
		{
			if (renderingShadows && !blockType.Contains("Volume Block"))
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

	public static void LoadBlockFromPrefab(string resource)
	{
		if (string.IsNullOrEmpty(resource))
		{
			return;
		}
		try
		{
			goPrefabs[resource] = UnityEngine.Object.Instantiate(prefabs[resource]);
			goPrefabs[resource].SetActive(value: false);
			colliders[resource] = goPrefabs[resource].GetComponent<Collider>();
			bool flag = false;
			MeshFilter component = goPrefabs[resource].GetComponent<MeshFilter>();
			if (component != null && component.mesh != null)
			{
				meshes[resource] = component.mesh;
				flag = true;
			}
			foreach (Transform item in goPrefabs[resource].transform)
			{
				MeshFilter component2 = item.GetComponent<MeshFilter>();
				if (component2 != null && component2.mesh != null)
				{
					meshes[item.gameObject.name] = component2.mesh;
					flag = true;
				}
			}
			if (!flag)
			{
				BWLog.Error("No mesh found for: " + resource);
			}
		}
		catch (Exception ex)
		{
			BWLog.Error("Error loading block from prefab '" + resource + "': " + ex.Message);
		}
	}

	public static void LoadBlock(string resource)
	{
		if (string.IsNullOrEmpty(resource))
		{
			return;
		}
		try
		{
			string text = resource;
			BlockEntry blockEntry = null;
			Dictionary<string, BlockEntry> blockEntries = BlockItemsRegistry.GetBlockEntries();
			if (blockEntries != null)
			{
				foreach (string key in blockEntries.Keys)
				{
					if (key == resource)
					{
						blockEntry = blockEntries[key];
						if (blockEntry != null && !string.IsNullOrEmpty(blockEntry.modelName))
						{
							text = blockEntry.modelName;
						}
						break;
					}
				}
			}
			prefabs[resource] = Resources.Load("Blocks/Prefab " + text) as GameObject;
			shapes[resource] = Resources.Load("Blocks/Shape " + text) as GameObject;
			glues[resource] = Resources.Load("Blocks/Glue " + text) as GameObject;
			joints[resource] = Resources.Load("Blocks/Joint " + text) as GameObject;
			GameObject gameObject = Resources.Load("Blocks/Collider " + text) as GameObject;
			if (gameObject != null)
			{
				compoundColliders[resource] = gameObject;
			}
			if (blockEntry == null || !(blockEntry.metaData != null) || !(prefabs[resource] != null))
			{
				return;
			}
			BlockMetaData component = prefabs[resource].GetComponent<BlockMetaData>();
			if (component != null)
			{
				BlockMetaData metaData = blockEntry.metaData;
				if (metaData.meshDatas != null)
				{
					component.meshDatas = metaData.meshDatas;
				}
				component.scaleType = metaData.scaleType;
			}
		}
		catch (Exception ex)
		{
			BWLog.Error("Error loading block '" + resource + "': " + ex.Message);
		}
	}

	[Conditional("DEBUG")]
	public static void Display(string s)
	{
		displayString = displayString + s + "\n";
	}

	[Conditional("DEBUG")]
	private void DisplayOnGUI()
	{
		GUI.Label(new Rect(10f * NormalizedScreen.scale, 10f * NormalizedScreen.scale, (float)Screen.width * NormalizedScreen.scale, (float)Screen.height * NormalizedScreen.scale), displayString);
	}

	private void OnHudMesh()
	{
		ExecuteOnHudMesh(fixedUpdateCommands);
		if (CurrentState == State.Play)
		{
			BWSceneManager.OnHudMesh();
		}
		Scarcity.PaintScarcityBadges();
		if (timerStart >= 0f && CurrentState != State.FrameCapture)
		{
			HudMeshOnGUI.Label(ref labelTimer, new Rect(350f * NormalizedScreen.scale, (float)defaultPanelPadding * NormalizedScreen.scale, (float)(NormalizedScreen.width - 600) * NormalizedScreen.scale, 120f * NormalizedScreen.scale), (((timerStop <= 0f) ? Time.time : timerStop) - timerStart).ToString("0.0"), HudMeshOnGUI.dataSource.GetStyle("label"));
		}
		HudMeshStyle defaultStyle = HudMeshOnGUI.dataSource.defaultStyle;
		int num = 0;
		for (int i = 1; i <= 2; i++)
		{
			string key = counterNames[i];
			if (!countersActivated[key] || CurrentState != State.Play)
			{
				continue;
			}
			bool flag = counterTargetsActivated[key];
			int num2 = counterTargets[key];
			int num3 = counters[key];
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
						HudMeshOnGUI.Label(labelCounters, num++, rect, starTexture);
					}
					else
					{
						HudMeshOnGUI.Label(labelCounters, num++, rect, starOutlineTexture);
					}
				}
			}
			else
			{
				string text = ((!flag) ? (string.Empty + num3) : (num3 + "/" + num2));
				float num6 = NormalizedScreen.scale * 100f;
				float height2 = NormalizedScreen.scale * 100f;
				HudMeshOnGUI.Label(rect: new Rect((float)(Screen.width / 2) - num6 / 2f + (float)(i - 1) * num6, NormalizedScreen.scale * 40f, num6, height2), labelList: labelCounters, index: num++, text: text, style: defaultStyle);
			}
		}
		Tutorial.OnHudMesh();
	}

	private void ExecuteOnHudMesh(List<Command> commands)
	{
		for (int i = 0; i < commands.Count; i++)
		{
			commands[i].OnHudMesh();
		}
	}

	private void ShowPlayerProfilePictureHint()
	{
		GUI.Label(new Rect(0f, Screen.height - 100, Screen.width, 100f), "Tap the camera button to take a profile picture of your Blockster");
	}

	private void OnApplicationQuit()
	{
		if (BW.Options.saveOnApplicationQuit())
		{
			FastSave();
		}
	}

	public void OnApplicationPause(bool pauseStatus)
	{
		if (IsStarted() && (!pauseStatus || CurrentState != State.Paused))
		{
			if (pauseStatus && WorldSession.current != null && (CurrentState == State.Play || CurrentState == State.Build || CurrentState == State.Paused) && !WinLoseManager.winning && !WinLoseManager.ending && !UI.Tapedeck.RecordButtonActive())
			{
				ShowOptionsScreen();
			}
			if (pauseStatus)
			{
				SetVRMode(enabled: false);
			}
			else if (bw != null && !UI.IsOptionsScreenVisible())
			{
				WorldSession.UnpauseCurrentSession();
			}
			if (musicPlayer != null)
			{
				musicPlayer.BWApplicationPause(pauseStatus);
			}
			UI.Tapedeck.BWApplicationPause(pauseStatus);
		}
	}

	public static bool GUIButton(Rect rect, string str, bool enabled = true)
	{
		GUI.enabled = enabled;
		if (GUI.RepeatButton(rect, str) && Input.GetMouseButtonUp(0))
		{
			return consumeEvent = true;
		}
		return false;
	}

	public static bool GUIButton(Rect rect, string str, GUIStyle style, bool enabled = true)
	{
		if (GUI.RepeatButton(rect, str, style) && Input.GetMouseButtonUp(0))
		{
			return consumeEvent = true;
		}
		return false;
	}

	public static bool GUIButton(Rect rect, Texture tex)
	{
		if (GUI.RepeatButton(rect, tex) && Input.GetMouseButtonUp(0))
		{
			return consumeEvent = true;
		}
		return false;
	}

	public static void MainMenuButtonsDidShow()
	{
		BWLog.Info("Sending custom 'MainMenu' signal to main menu world.");
		sendingCustom["MainMenu"] = 1f;
	}

	private static void SetupVRCamera()
	{
		_ = vrEnabled;
	}

	private static void TakedownVRCamera()
	{
		_ = vrEnabled;
	}

	public static Ray CameraScreenPointToRay(Vector3 screenPos)
	{
		_ = vrEnabled;
		return _mainCamera.ScreenPointToRay(screenPos);
	}

	public static void SetupCameraHierarchy()
	{
		GameObject gameObject = new GameObject("Camera Tilt Transform");
		bw.cameraTiltTransform = gameObject.transform;
		bw.cameraTiltTransform.parent = cameraTransform;
		bw.cameraTiltTransform.localPosition = Vector3.zero;
		bw.cameraTiltTransform.localRotation = Quaternion.identity;
		vrCameraAdjust = new GameObject("VR Camera Straightener");
		vrCameraAdjust.transform.parent = bw.cameraTiltTransform;
		vrCameraAdjust.transform.localPosition = Vector3.zero;
		vrCameraAdjust.transform.localRotation = Quaternion.identity;
		bw.cameraParentTransform = vrCameraAdjust.transform;
		mainCamera.transform.parent = bw.cameraParentTransform;
		mainCamera.gameObject.SetActive(value: false);
	}

	public static void ResetVRSensor()
	{
		vrCameraAdjust.transform.localRotation = Quaternion.identity;
		_ = vrEnabled;
	}

	public void SetVRModeFromJS(int trueFalse)
	{
		BWLog.Info("SetVRModeFromJS(" + trueFalse + ")");
		bool vRMode = 1 == trueFalse;
		SetVRMode(vRMode);
	}

	public static void SetVRMode(bool enabled)
	{
		if (enabled == vrEnabled)
		{
			return;
		}
		vrEnabled = enabled;
		worldSessionHadVR |= vrEnabled;
		BWLog.Info(((!vrEnabled) ? "Disabling" : "Enabling") + " VR Mode " + currentVRType);
		if (vrEnabled)
		{
			BWLog.Error("VR type " + currentVRType.ToString() + " is not enabled in this build.");
		}
		UI.SpeechBubble.SetWorldSpaceMode(vrEnabled);
		if (vrEnabled)
		{
			Vector3 vector = new Vector3(cameraTransform.forward.x, 0f, cameraTransform.forward.z);
			if (vector.sqrMagnitude < 0.1f)
			{
				vector = new Vector3(cameraTransform.up.x, 0f, cameraTransform.up.z);
			}
			vrCameraAdjust.transform.rotation = Quaternion.LookRotation(vector.normalized, Vector3.up);
		}
		else
		{
			vrCameraAdjust.transform.localRotation = Quaternion.identity;
		}
		blocksworldCamera.SetReticleParent((!vrEnabled) ? null : mainCamera.transform);
	}

	public static bool IsVRCameraMode()
	{
		return vrEnabled;
	}

	public static bool IsBlockVRCameraFocus(Block b)
	{
		return false;
	}

	public static bool IsBlockVRCameraLookAt(Block b)
	{
		return false;
	}

	public void CancelTileDragGestures()
	{
		if (tileDragGesture.IsActive)
		{
			tileDragGesture.Cancel();
		}
		if (createTileDragGesture.IsActive)
		{
			createTileDragGesture.Cancel();
		}
		if (replaceBodyPartGesture.IsActive)
		{
			replaceBodyPartGesture.Cancel();
		}
		if (characterEditGearGesture.IsActive)
		{
			characterEditGearGesture.Cancel();
		}
	}

	public void Play()
	{
		if (CharacterEditor.Instance.InEditMode())
		{
			CharacterEditor.Instance.Exit();
		}
		if (!resettingPlay)
		{
			lastBuildModeSelectedBlock = selectedBlock;
			lastBuildModeSelectedBunch = selectedBunch;
			_buildModeFogColor = RenderSettings.fogColor;
			_buildModeFogStart = RenderSettings.fogStartDistance;
			_buildModeFogEnd = RenderSettings.fogEndDistance;
		}
		EventSystem.current.SetSelectedGameObject(null);
		Select(null);
		scriptPanel.Show(show: false);
		CancelTileDragGestures();
		WorldSession.current.OnPlay();
		MappedInput.SetMode(MappableInputMode.Play);
		Block.ClearConnectedCache();
		Block.ResetTiltOrientation();
		playFixedUpdateCounter = 0;
		BWSceneManager.ResetPlayBlocksAndPredicates();
		Block.vanishingOrAppearingBlocks.Clear();
		TextureAndPaintBlockRegistry.Clear();
		BlockAccelerations.Play();
		CheckpointSystem.Clear();
		VariableManager.Clear();
		WinLoseManager.Reset();
		Invincibility.Clear();
		UpdateEditorMusicPlayerEnabled();
		ModelSignals.Clear();
		TiltManager.Instance.StartMonitoring();
		Physics.gravity = (0f - defaultGravityStrength) * Vector3.up;
		angularDragMultiplier = 1f;
		dragMultiplier = 1f;
		hasWon = false;
		winIsWaiting = false;
		waitForSetPurchase = false;
		worldSessionHadBlockTap = false;
		isFirstFrame = true;
		dynamicLockPull = false;
		interpolateRigidBodies = BW.Options.interpolateRigidbodies();
		_ = Tutorial.state;
		resettingPlay = false;
		UpdateCameraPosesMap();
		currentBackgroundMusic = string.Empty;
		Tutorial.StartPlay();
		BWSceneManager.InitPlay();
		List<List<Block>> list = ConnectednessGraph.FindChunkSubgraphs();
		foreach (List<Block> item in list)
		{
			chunks.Add(new Chunk(item));
		}
		for (int i = 0; i < chunks.Count; i++)
		{
			chunks[i].blocks[0].UpdateConnectedCache();
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
		ResetState();
		TagManager.ClearRegisteredBlocks(hard: true);
		BWSceneManager.ExecutePlayLegs();
		blocksworldCamera.Store();
		blocksworldCamera.Play();
		SetBlocksworldState(State.Play);
		TBox.Show(show: false);
		gameStart = true;
		gameTime = 0f;
		UI.SidePanel.Hide();
		UI.Controls.OnPlay();
		orbitDuringControlGesture.IsEnabled = true;
		if (pullObject != null)
		{
			pullObject.HideLine();
		}
		Sound.Play();
		CollisionManager.Play();
		UpdateCenterOfMasses();
		RemoveAllCommands();
		storeFogDistances = GetDefaultFog();
		leaderboardData.SetupLeaderboard();
		AddFixedUpdateCommand(new DelegateCommand(delegate
		{
			for (int j = 0; j < chunks.Count; j++)
			{
				Rigidbody rb = chunks[j].rb;
				if (rb != null && rb.IsSleeping() && rb.isKinematic)
				{
					rb.isKinematic = false;
					rb.isKinematic = true;
				}
			}
		}));
	}

	public static void UpdateCameraPosesMap()
	{
		cameraPosesMap.Clear();
		for (int i = 0; i < cameraPoses.Count; i++)
		{
			NamedPose namedPose = cameraPoses[i];
			if (!string.IsNullOrEmpty(namedPose.name))
			{
				cameraPosesMap[namedPose.name] = namedPose;
			}
		}
	}

	public static void GoToCameraFrameFor(Vector3 pos, Vector3 scale)
	{
		float num = 7f + 1.3f * Util.MaxComponent(scale);
		Vector3 position = cameraTransform.position;
		Vector3 normalized = (pos - position).normalized;
		Vector3 vector = pos - normalized * num;
		blocksworldCamera.SetCameraPosition(vector);
		blocksworldCamera.SetTargetPosition(pos);
		cameraTransform.LookAt(pos);
	}

	public static HashSet<Predicate> GetAnalogStickPredicates()
	{
		if (analogStickPredicates == null)
		{
			analogStickPredicates = new HashSet<Predicate>
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
		return analogStickPredicates;
	}

	public static HashSet<Predicate> GetTiltMoverPredicates()
	{
		if (tiltMoverPredicates == null)
		{
			tiltMoverPredicates = new HashSet<Predicate>
			{
				BlockCharacter.predicateCharacterTiltMover,
				BlockAnimatedCharacter.predicateChracterTiltMover,
				BlockAbstractAntiGravity.predicateTiltMover,
				BlockSphere.predicateSphereTiltMover,
				BlockFlightYoke.predicateTiltFlightSim
			};
		}
		return tiltMoverPredicates;
	}

	public static bool DPadTilesInWorld(string key)
	{
		HashSet<Predicate> hashSet = GetAnalogStickPredicates();
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
						string text = ((args.Length <= num) ? "L" : ((string)args[num]));
						return text == key;
					}
				}
			}
		}
		return false;
	}

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
		if (!force && CurrentState != State.Play && (LastState != State.Play || CurrentState != State.Menu))
		{
			return;
		}
		BWSceneManager.StopPlay();
		TiltManager.Instance.StopMonitoring();
		MappedInput.SetMode(MappableInputMode.Build);
		customIntVariables.Clear();
		blockIntVariables.Clear();
		BlockData.Clear();
		ScriptRowData.Clear();
		ModelSignals.Clear();
		TBox.ResetTargetSnapping();
		TextureAndPaintBlockRegistry.Clear();
		UpdateEditorMusicPlayerEnabled();
		UpdateTiles();
		currentBackgroundMusic = string.Empty;
		lockInput = false;
		SetBlocksworldState(State.Build);
		BlockAccelerations.Stop();
		BWSceneManager.playBlocksRemoved.Clear();
		orbitDuringControlGesture.IsEnabled = false;
		for (int i = 1; i <= numCounters; i++)
		{
			string key = i + string.Empty;
			countersActivated[key] = false;
			counters[key] = 0;
		}
		resetting = true;
		BWSceneManager.ExecuteStop(resetBlocks);
		resetting = false;
		dynamicLockPull = false;
		ClearChunks();
		blocksworldCamera.Restore();
		rewardCamera.enabled = false;
		if (!forcePlayMode)
		{
			UI.SidePanel.Show();
			buildPanel.Layout();
		}
		blocksworldCamera.Stop();
		blocksworldCamera.Follow(selectedBunch);
		ResetSkyAndFogSettings();
		CollisionManager.Stop();
		Tutorial.StopPlay();
		if (pullObject != null)
		{
			pullObject.HideLine();
		}
		UI.Controls.ResetAllControls();
		UI.Controls.Hide();
		_ = resettingPlay;
		TagManager.ClearRegisteredBlocks(hard: true);
		VisualEffect.StopVfxs();
		TreasureHandler.Stop();
		Invincibility.Clear();
		RemoveAllCommands();
		Block.ClearConnectedCache();
		leaderboardData.Reset();
		if (BW.isUnityEditor)
		{
			Tile.UpdateTileParameterSettings();
		}
		Resources.UnloadUnusedAssets();
		GC.Collect();
		if (!resettingPlay)
		{
			if (lastBuildModeSelectedBunch != null)
			{
				SelectBunch(lastBuildModeSelectedBunch.blocks);
			}
			else if (lastBuildModeSelectedBlock != null)
			{
				Select(lastBuildModeSelectedBlock);
			}
			lastBuildModeSelectedBunch = null;
			lastBuildModeSelectedBlock = null;
		}
	}

	private void ClearChunks()
	{
		foreach (Chunk chunk in chunks)
		{
			chunk.Destroy();
		}
		chunks.Clear();
	}

	private void RemoveAllCommands()
	{
		Command.RemoveCommands(fixedUpdateCommands);
		Command.RemoveCommands(updateCommands);
		Command.RemoveCommands(resetStateCommands);
	}

	public void Restart()
	{
		if (!resettingPlay)
		{
			buildPanel.ignoreShow = true;
			scriptPanel.ignoreShow = true;
			if (WorldSession.jumpRestoreConfig != null)
			{
				WorldSession.RestoreJumpConfig();
				return;
			}
			resettingPlay = true;
			Stop();
			StartCoroutine(DoRestart());
		}
	}

	public IEnumerator DoRestart()
	{
		WorldSession.current.OnRestart();
		yield return 2;
		Play();
	}

	public void Save()
	{
		if (worldSaveEnabled && !f3PressedInCurrentWorld)
		{
			if (CurrentState == State.Play || (CurrentState != State.Build && LastState == State.Play))
			{
				BWLog.Info("Skipping save since in play mode");
			}
			else
			{
				WorldSession.Save();
			}
		}
	}

	public void FastSave()
	{
		if (worldSaveEnabled && !f3PressedInCurrentWorld)
		{
			if (CurrentState == State.Play)
			{
				BWLog.Info("Skipping fast save since in play mode");
			}
			else
			{
				WorldSession.FastSave();
			}
		}
	}

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
				foreach (Tile item in list)
				{
					if (flag2 && i == 0)
					{
						GAF gaf = item.gaf;
						if ((gaf.Predicate == Block.predicateTextureTo && (string)gaf.Args[0] == "Plain") || gaf.Predicate == Block.predicateStop)
						{
							continue;
						}
					}
					item.ToJSON(encoder, compact);
				}
				encoder.EndArray();
			}
			if (flag)
			{
				encoder.BeginArray();
				Tile tile = new Tile(new GAF("Meta.Then"));
				Tile tile2 = new Tile(new GAF("Block.Locked"));
				tile.ToJSON(encoder, compact);
				tile2.ToJSON(encoder, compact);
				encoder.EndArray();
				flag = false;
			}
		}
		encoder.EndArray();
	}

	private void WriteBlockAsJSON(JSONStreamEncoder encoder, List<List<Tile>> blockTiles, bool lockBlocks, bool compact)
	{
		encoder.InsertNewline();
		encoder.BeginObject();
		encoder.WriteKey((!useCompactGafWriteRenamings) ? "tile-rows" : "r");
		WriteTilesAsJSON(encoder, blockTiles, lockBlocks, compact);
		encoder.EndObject();
	}

	private void WriteBlocksAsJSON(JSONStreamEncoder encoder, bool compact)
	{
		encoder.BeginArray();
		bool flag = Tutorial.state != TutorialState.None && Tutorial.state != TutorialState.Puzzle;
		if (flag && Tutorial.safeState != null)
		{
			foreach (List<List<Tile>> placedBlockTile in Tutorial.safeState.placedBlockTiles)
			{
				WriteBlockAsJSON(encoder, placedBlockTile, flag, compact);
			}
			foreach (List<List<Tile>> notPlacedBlockTile in Tutorial.safeState.notPlacedBlockTiles)
			{
				WriteBlockAsJSON(encoder, notPlacedBlockTile, lockBlocks: false, compact);
			}
		}
		else
		{
			List<Block> list = BWSceneManager.AllBlocks();
			Block block = ((list.Count <= 0) ? null : list[list.Count - 1]);
			for (int i = 0; i < list.Count; i++)
			{
				Block block2 = list[i];
				if (!flag || !Tutorial.IsLastBlockIncomplete() || Tutorial.state == TutorialState.BuildingCompleted || Tutorial.numGivenBlocks == list.Count || block2 != block)
				{
					WriteBlockAsJSON(encoder, block2.tiles, flag, compact);
				}
			}
			if (Tutorial.blocks != null && flag && Tutorial.step >= 0)
			{
				for (int j = Tutorial.step; j < Tutorial.blocks.Count; j++)
				{
					WriteBlockAsJSON(encoder, Tutorial.blocks[j].tiles, lockBlocks: false, compact);
				}
			}
		}
		encoder.InsertNewline();
		encoder.EndArray();
	}

	private void WriteCameraAsJSON(JSONStreamEncoder encoder)
	{
		encoder.BeginObject();
		encoder.InsertNewline();
		encoder.WriteKey("rotation");
		cameraTransform.rotation.ToJSON(encoder);
		encoder.InsertNewline();
		encoder.WriteKey("position");
		cameraTransform.position.ToJSON(encoder);
		encoder.InsertNewline();
		encoder.WriteKey("playDistance");
		encoder.WriteNumber(blocksworldCamera.manualCameraDistance);
		encoder.InsertNewline();
		encoder.WriteKey("playAngle");
		encoder.WriteNumber(blocksworldCamera.manualCameraAngle);
		encoder.InsertNewline();
		encoder.EndObject();
	}

	public string Serialize(bool saveCamera = true)
	{
		StringBuilder stringBuilder = new StringBuilder(32768);
		StringWriter writer = new StringWriter(stringBuilder, CultureInfo.InvariantCulture);
		JSONStreamEncoder jSONStreamEncoder = new JSONStreamEncoder(writer);
		jSONStreamEncoder.BeginObject();
		jSONStreamEncoder.InsertNewline();
		if (currentWorldId != null)
		{
			jSONStreamEncoder.WriteKey("id");
			jSONStreamEncoder.WriteString(currentWorldId);
			jSONStreamEncoder.InsertNewline();
		}
		jSONStreamEncoder.WriteKey("blocks");
		WriteBlocksAsJSON(jSONStreamEncoder, useCompactGafWriteRenamings);
		jSONStreamEncoder.InsertNewline();
		jSONStreamEncoder.WriteKey("connections");
		jSONStreamEncoder.BeginArray();
		List<Block> list = BWSceneManager.AllBlocks();
		for (int i = 0; i < list.Count; i++)
		{
			Block block = list[i];
			jSONStreamEncoder.InsertNewline();
			jSONStreamEncoder.BeginArray();
			foreach (Block connection in block.connections)
			{
				jSONStreamEncoder.WriteNumber(list.IndexOf(connection));
			}
			jSONStreamEncoder.EndArray();
		}
		jSONStreamEncoder.InsertNewline();
		jSONStreamEncoder.EndArray();
		jSONStreamEncoder.InsertNewline();
		jSONStreamEncoder.WriteKey("connectionTypes");
		jSONStreamEncoder.BeginArray();
		for (int j = 0; j < list.Count; j++)
		{
			Block block2 = list[j];
			jSONStreamEncoder.InsertNewline();
			jSONStreamEncoder.BeginArray();
			foreach (int connectionType in block2.connectionTypes)
			{
				jSONStreamEncoder.WriteNumber(connectionType);
			}
			jSONStreamEncoder.EndArray();
		}
		jSONStreamEncoder.InsertNewline();
		jSONStreamEncoder.EndArray();
		jSONStreamEncoder.InsertNewline();
		jSONStreamEncoder.WriteKey("frozen-in-terrain");
		jSONStreamEncoder.BeginArray();
		for (int k = 0; k < list.Count; k++)
		{
			Block block3 = list[k];
			if (block3.frozenInTerrainStatus == -1)
			{
				block3.UpdateFrozenInTerrainStatus();
			}
			jSONStreamEncoder.WriteNumber(block3.frozenInTerrainStatus);
		}
		jSONStreamEncoder.EndArray();
		jSONStreamEncoder.InsertNewline();
		if (saveCamera)
		{
			jSONStreamEncoder.WriteKey("camera");
			WriteCameraAsJSON(jSONStreamEncoder);
			jSONStreamEncoder.InsertNewline();
		}
		else if (lastLoadedCameraObj != null)
		{
			jSONStreamEncoder.WriteKey("camera");
			jSONStreamEncoder.WriteJObject(lastLoadedCameraObj);
			jSONStreamEncoder.InsertNewline();
		}
		WritePuzzleInventoryAndUsage(jSONStreamEncoder, Scarcity.puzzleInventory, "puzzle-gafs", "puzzle-gaf-usage");
		WritePuzzleInventoryAndUsage(jSONStreamEncoder, Scarcity.puzzleInventoryAfterStepByStep, "puzzle-gafs-after-step-by-step", "puzzle-gaf-usage-after-step-by-step");
		if (editorSelectionLocked.Count > 0)
		{
			jSONStreamEncoder.WriteKey("selection-locked");
			jSONStreamEncoder.BeginArray();
			for (int l = 0; l < list.Count; l++)
			{
				Block item = list[l];
				if (editorSelectionLocked.Contains(item))
				{
					jSONStreamEncoder.WriteNumber(l);
				}
			}
			jSONStreamEncoder.EndArray();
			jSONStreamEncoder.InsertNewline();
		}
		if (cameraPoses.Count > 0)
		{
			jSONStreamEncoder.WriteKey("camera-poses");
			jSONStreamEncoder.BeginArray();
			jSONStreamEncoder.InsertNewline();
			for (int m = 0; m < cameraPoses.Count; m++)
			{
				NamedPose namedPose = cameraPoses[m];
				jSONStreamEncoder.BeginObject();
				jSONStreamEncoder.WriteKey("name");
				jSONStreamEncoder.WriteString(namedPose.name);
				jSONStreamEncoder.WriteKey("position");
				namedPose.position.ToJSON(jSONStreamEncoder);
				jSONStreamEncoder.WriteKey("direction");
				namedPose.direction.ToJSON(jSONStreamEncoder);
				jSONStreamEncoder.EndObject();
				jSONStreamEncoder.InsertNewline();
			}
			jSONStreamEncoder.EndArray();
			jSONStreamEncoder.InsertNewline();
		}
		if (signalNames.Length != 0)
		{
			bool flag = false;
			string[] array = signalNames;
			foreach (string text in array)
			{
				if (text != null && text.Length > 0)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				jSONStreamEncoder.WriteKey("signal-names");
				jSONStreamEncoder.BeginArray();
				jSONStreamEncoder.InsertNewline();
				string[] array2 = signalNames;
				foreach (string text2 in array2)
				{
					string text3 = text2;
					if (text3 == null)
					{
						text3 = string.Empty;
					}
					jSONStreamEncoder.WriteString(text3);
				}
				jSONStreamEncoder.EndArray();
				jSONStreamEncoder.InsertNewline();
			}
		}
		if (blockNames.Count > 0)
		{
			bool flag2 = false;
			for (int num2 = 0; num2 < list.Count; num2++)
			{
				Block key = list[num2];
				if (blockNames.ContainsKey(key) && !string.IsNullOrEmpty(blockNames[key]))
				{
					flag2 = true;
					break;
				}
			}
			if (flag2)
			{
				jSONStreamEncoder.WriteKey("block-names");
				jSONStreamEncoder.BeginArray();
				jSONStreamEncoder.InsertNewline();
				for (int num3 = 0; num3 < list.Count; num3++)
				{
					Block key2 = list[num3];
					string str = string.Empty;
					if (blockNames.ContainsKey(key2))
					{
						str = blockNames[key2];
					}
					jSONStreamEncoder.WriteString(str);
				}
				jSONStreamEncoder.EndArray();
				jSONStreamEncoder.InsertNewline();
			}
		}
		if (launchIntoPlayMode)
		{
			jSONStreamEncoder.WriteKey("launch-into-play-mode");
			jSONStreamEncoder.WriteBool(launchIntoPlayMode);
			jSONStreamEncoder.InsertNewline();
		}
		if (staticLockPull)
		{
			jSONStreamEncoder.WriteKey("lock-pull");
			jSONStreamEncoder.WriteBool(staticLockPull);
			jSONStreamEncoder.InsertNewline();
		}
		if (WorldSession.isProfileBuildSession())
		{
			jSONStreamEncoder.WriteKey("player-profile-world");
			jSONStreamEncoder.WriteBool(WorldSession.isProfileBuildSession());
			jSONStreamEncoder.InsertNewline();
		}
		if (Tutorial.numGivenBlocks != list.Count && Tutorial.safeState == null && Tutorial.state != TutorialState.None && Tutorial.state != TutorialState.Puzzle && Tutorial.state != TutorialState.BuildingCompleted && Tutorial.IsLastBlockIncomplete())
		{
			jSONStreamEncoder.WriteKey("last-tutorial-block-tile-rows");
			jSONStreamEncoder.InsertNewline();
			WriteTilesAsJSON(jSONStreamEncoder, list[list.Count - 1].tiles, lockBlocks: false, useCompactGafWriteRenamings);
			jSONStreamEncoder.InsertNewline();
		}
		if (Tutorial.manualPaintOrTexture.Count > 0)
		{
			jSONStreamEncoder.WriteKey("manual-paint-or-texture");
			jSONStreamEncoder.InsertNewline();
			WriteTilesAsJSON(jSONStreamEncoder, new List<List<Tile>> { Tutorial.manualPaintOrTexture }, lockBlocks: false, useCompactGafWriteRenamings);
			jSONStreamEncoder.InsertNewline();
		}
		if (Tutorial.completedManualPaintOrTexture.Count > 0)
		{
			jSONStreamEncoder.WriteKey("completed-manual-paint-or-texture");
			jSONStreamEncoder.InsertNewline();
			WriteTilesAsJSON(jSONStreamEncoder, new List<List<Tile>> { Tutorial.completedManualPaintOrTexture }, lockBlocks: false, useCompactGafWriteRenamings);
			jSONStreamEncoder.InsertNewline();
		}
		if (Tutorial.state != TutorialState.None)
		{
			jSONStreamEncoder.WriteKey("saved-tutorial-step");
			jSONStreamEncoder.WriteNumber(Tutorial.step + Tutorial.savedStep);
			jSONStreamEncoder.InsertNewline();
		}
		jSONStreamEncoder.EndObject();
		return stringBuilder.ToString();
	}

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

	public List<List<Tile>> LoadJSONTiles(JObject tileRows)
	{
		List<List<Tile>> list = new List<List<Tile>>();
		foreach (JObject item in tileRows.ArrayValue)
		{
			List<Tile> list2 = new List<Tile>();
			list.Add(list2);
			foreach (JObject item2 in item.ArrayValue)
			{
				Tile tile = Tile.FromJSON(item2);
				if (tile == null)
				{
					return null;
				}
				list2.Add(tile);
			}
		}
		return list;
	}

	private void ReadSignalNames(JObject obj)
	{
		if (!obj.ObjectValue.ContainsKey("signal-names"))
		{
			return;
		}
		List<JObject> arrayValue = obj["signal-names"].ArrayValue;
		int num = 0;
		foreach (JObject item in arrayValue)
		{
			string stringValue = item.StringValue;
			if (string.IsNullOrEmpty(stringValue))
			{
				signalNames[num] = null;
			}
			else
			{
				signalNames[num] = stringValue;
			}
			num++;
		}
	}

	private void ReadBlockNames(JObject obj)
	{
		if (!obj.ObjectValue.ContainsKey("block-names"))
		{
			return;
		}
		List<JObject> arrayValue = obj["block-names"].ArrayValue;
		int num = 0;
		foreach (JObject item in arrayValue)
		{
			string stringValue = item.StringValue;
			if (!string.IsNullOrEmpty(stringValue))
			{
				List<Block> list = BWSceneManager.AllBlocks();
				if (num < list.Count)
				{
					blockNames[list[num]] = stringValue;
				}
			}
			num++;
		}
	}

	private void ReadCameraPoses(JObject obj)
	{
		if (!obj.ObjectValue.ContainsKey("camera-poses"))
		{
			return;
		}
		List<JObject> arrayValue = obj["camera-poses"].ArrayValue;
		foreach (JObject item in arrayValue)
		{
			Dictionary<string, JObject> objectValue = item.ObjectValue;
			if (!objectValue.ContainsKey("name") || !objectValue.ContainsKey("position") || !objectValue.ContainsKey("direction"))
			{
				BWLog.Info("Invalid camera pose. Missing a key in object: 'name', 'position' or 'direction'");
				continue;
			}
			string stringValue = objectValue["name"].StringValue;
			Vector3 position = objectValue["position"].Vector3Value();
			Vector3 direction = objectValue["direction"].Vector3Value();
			NamedPose namedPose = new NamedPose(stringValue, position, direction);
			cameraPoses.Add(namedPose);
			cameraPosesMap[namedPose.name] = namedPose;
		}
	}

	public static JObject GetTileRows(JObject obj)
	{
		Dictionary<string, JObject> objectValue = obj.ObjectValue;
		if (objectValue.TryGetValue("r", out var value))
		{
			return value;
		}
		if (objectValue.TryGetValue("tile-rows", out value))
		{
			return value;
		}
		return null;
	}

	private List<List<List<Tile>>> ParseWorldJSON(JObject obj)
	{
		List<List<List<Tile>>> list = new List<List<List<Tile>>>();
		foreach (JObject item in obj["blocks"].ArrayValue)
		{
			List<List<Tile>> list2 = LoadJSONTiles(GetTileRows(item));
			if (list2 == null)
			{
				return null;
			}
			list.Add(list2);
		}
		return list;
	}

	public static void UnlockTutorialGAFs(string source)
	{
		JObject jObject = JSONDecoder.Decode(source);
		if (jObject == null)
		{
			return;
		}
		List<List<List<Tile>>> list = new List<List<List<Tile>>>();
		foreach (JObject item in jObject["blocks"].ArrayValue)
		{
			JObject tileRows = GetTileRows(item);
			List<List<Tile>> list2 = new List<List<Tile>>();
			foreach (JObject item2 in tileRows.ArrayValue)
			{
				List<Tile> list3 = new List<Tile>();
				list2.Add(list3);
				foreach (JObject item3 in item2.ArrayValue)
				{
					Tile tile = Tile.FromJSON(item3);
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
		SetupTutorialGAFs(list);
	}

	public static string GetWorldGAFUsageAsBlocksInventory(string worldSource)
	{
		JObject jObject = JSONDecoder.Decode(worldSource);
		if (jObject == null)
		{
			return null;
		}
		List<List<List<Tile>>> list = new List<List<List<Tile>>>();
		foreach (JObject item in jObject["blocks"].ArrayValue)
		{
			JObject tileRows = GetTileRows(item);
			List<List<Tile>> list2 = new List<List<Tile>>();
			foreach (JObject item2 in tileRows.ArrayValue)
			{
				List<Tile> list3 = new List<Tile>();
				list2.Add(list3);
				foreach (JObject item3 in item2.ArrayValue)
				{
					Tile tile = Tile.FromJSON(item3);
					if (tile != null && !tile.IsLocked())
					{
						list3.Add(tile);
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
		Scarcity.GetInventoryUse(list, WorldType.ForComplexityCalculation, dictionary);
		HashSet<Predicate> infinitePredicates = new HashSet<Predicate> { Block.predicatePlaySoundDurational };
		return Scarcity.GetBlockIDInventoryString(dictionary, infinitePredicates);
	}

	public static void SetupTutorialGAFs(List<List<List<Tile>>> tilesLists)
	{
		Dictionary<GAF, int> dictionary = new Dictionary<GAF, int>();
		Scarcity.GetInventoryUse(tilesLists, WorldType.StepByStepTutorial, dictionary);
		HashSet<GAF> hashSet = new HashSet<GAF>();
		if (unlockedGAFs == null)
		{
			unlockedGAFs = new List<GAF>();
		}
		for (int i = 0; i < unlockedGAFs.Count; i++)
		{
			hashSet.Add(unlockedGAFs[i]);
		}
		foreach (GAF key in dictionary.Keys)
		{
			BlockItem blockItem = BlockItem.FindByGafPredicateNameAndArguments(key.Predicate.Name, key.Args);
			if (blockItem != null)
			{
				if (!hashSet.Contains(key))
				{
					hashSet.Add(key);
					unlockedGAFs.Add(key);
					Predicate predicate = key.Predicate;
					if (predicate == Block.predicateTextureTo)
					{
						string textureName = (string)key.Args[0];
						AddToPublicProvidedTextures(textureName);
					}
					else if (predicate == Block.predicatePaintTo)
					{
						unlockedPaints.Add((string)key.Args[0]);
					}
				}
				continue;
			}
			List<BlockItem> list = BlockItem.FindByGafPredicateName(key.Predicate.Name);
			foreach (BlockItem item2 in list)
			{
				GAF item = new GAF(item2.GafPredicateName, item2.GafDefaultArgs);
				if (!hashSet.Contains(item))
				{
					hashSet.Add(item);
					unlockedGAFs.Add(item);
				}
			}
		}
		List<GAF> implicitGAFs = GetImplicitGAFs(unlockedGAFs);
		unlockedGAFs.AddRange(implicitGAFs);
	}

	private void HideBlockWithDelayedShow(Block block)
	{
		if (!(block.go != null))
		{
			return;
		}
		block.go.GetComponent<Renderer>().enabled = false;
		if (block.goShadow != null)
		{
			block.goShadow.GetComponent<Renderer>().enabled = false;
		}
		AddUpdateCommand(new DelayedDelegateCommand(delegate
		{
			if (CurrentState == State.Build && !Tutorial.InTutorialOrPuzzle() && block.go != null)
			{
				block.go.GetComponent<Renderer>().enabled = true;
				if (block.goShadow != null)
				{
					block.goShadow.GetComponent<Renderer>().enabled = true;
				}
			}
		}, 10));
	}

	private bool LoadFromJSON(JObject obj, int version)
	{
		List<List<List<Tile>>> list = ParseWorldJSON(obj);
		if (list == null)
		{
			WorldSession.NotifyFileTooNew();
			return false;
		}
		foreach (List<List<Tile>> item in list)
		{
			Block block = Block.NewBlock(item);
			if (block != null)
			{
				BWSceneManager.AddBlock(block);
				if (!block.VisibleInPlayMode())
				{
					HideBlockWithDelayedShow(block);
				}
			}
		}
		List<Block> list2 = BWSceneManager.AllBlocks();
		if (list2.Count == 0)
		{
			return false;
		}
		int num = 0;
		foreach (JObject item2 in obj["connections"].ArrayValue)
		{
			foreach (JObject item3 in item2.ArrayValue)
			{
				list2[num].connections.Add(list2[(int)item3]);
			}
			num++;
		}
		num = 0;
		foreach (JObject item4 in obj["connectionTypes"].ArrayValue)
		{
			foreach (JObject item5 in item4.ArrayValue)
			{
				list2[num].connectionTypes.Add((int)item5);
			}
			num++;
		}
		List<Block> list3 = new List<Block>();
		for (num = 0; num < list2.Count; num++)
		{
			Block block2 = list2[num];
			int num2 = block2.connections.IndexOf(block2);
			if (num2 != -1)
			{
				block2.connections.RemoveAt(num2);
				block2.connectionTypes.RemoveAt(num2);
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
			currentWorldId = obj["id"].StringValue;
		}
		else
		{
			currentWorldId = null;
		}
		if (objectValue.ContainsKey("frozen-in-terrain"))
		{
			List<JObject> arrayValue = objectValue["frozen-in-terrain"].ArrayValue;
			for (int i = 0; i < arrayValue.Count; i++)
			{
				if (i < list2.Count)
				{
					list2[i].frozenInTerrainStatus = arrayValue[i].IntValue;
				}
			}
		}
		if (objectValue.ContainsKey("camera"))
		{
			lastLoadedCameraObj = obj["camera"];
			blocksworldCamera.PlaceCamera(lastLoadedCameraObj["rotation"].QuaternionValue().eulerAngles, lastLoadedCameraObj["position"].Vector3Value());
			if (lastLoadedCameraObj.ObjectValue.ContainsKey("playAngle"))
			{
				blocksworldCamera.manualCameraAngle = lastLoadedCameraObj["playAngle"].FloatValue;
			}
			if (lastLoadedCameraObj.ObjectValue.ContainsKey("playDistance"))
			{
				blocksworldCamera.manualCameraDistance = lastLoadedCameraObj["playDistance"].FloatValue;
			}
			blocksworldCamera.CameraStateLoaded();
		}
		if (objectValue.ContainsKey("selection-locked"))
		{
			List<JObject> arrayValue2 = obj["selection-locked"].ArrayValue;
			foreach (JObject item6 in arrayValue2)
			{
				int num3 = (int)item6;
				if (num3 < list2.Count)
				{
					editorSelectionLocked.Add(list2[num3]);
				}
			}
		}
		ReadSignalNames(obj);
		ReadBlockNames(obj);
		ReadCameraPoses(obj);
		BlockGroups.GatherBlockGroups(list2);
		bw.StatePlayUpdate();
		list2.ForEach(delegate(Block b)
		{
			b.oldPos = Util.nullVector3;
			b.lastShadowHitDistance = -2f;
		});
		Resources.UnloadUnusedAssets();
		GC.Collect();
		if (version < 11)
		{
			for (num = 0; num < list2.Count; num++)
			{
				Block block3 = list2[num];
				foreach (List<Tile> tile in block3.tiles)
				{
					foreach (Tile item7 in tile)
					{
						if (item7.gaf.Predicate.Name == "Block.ButtonInput")
						{
							object[] array = Util.CopyArray(item7.gaf.Args);
							switch ((string)item7.gaf.Args[0])
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
							item7.gaf = new GAF(item7.gaf.Predicate, array);
						}
						if (item7.gaf.Predicate.Name == "Block.DeviceTilt")
						{
							item7.gaf = new GAF("Block.ButtonInput", ((float)item7.gaf.Args[0] >= 0f) ? "R" : "L");
						}
					}
				}
			}
			for (num = 0; num < list2.Count; num++)
			{
				Block block4 = list2[num];
				switch (block4.GetPaint())
				{
				case "Grass":
					block4.PaintTo("Green", permanent: true);
					break;
				case "Celeste":
					block4.PaintTo("Blue", permanent: true);
					break;
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
			launchIntoPlayMode = (bool)obj["launch-into-play-mode"];
		}
		if (obj.ObjectValue.ContainsKey("lock-pull"))
		{
			staticLockPull = (bool)obj["lock-pull"];
		}
		Tutorial.savedProgressTiles = null;
		if (obj.ObjectValue.ContainsKey("last-tutorial-block-tile-rows"))
		{
			Tutorial.savedProgressTiles = LoadJSONTiles(obj["last-tutorial-block-tile-rows"]);
		}
		Tutorial.manualPaintOrTexture.Clear();
		if (obj.ObjectValue.ContainsKey("manual-paint-or-texture"))
		{
			Tutorial.manualPaintOrTexture = LoadJSONTiles(obj["manual-paint-or-texture"])[0];
		}
		Tutorial.completedManualPaintOrTexture.Clear();
		if (obj.ObjectValue.ContainsKey("completed-manual-paint-or-texture"))
		{
			Tutorial.completedManualPaintOrTexture = LoadJSONTiles(obj["completed-manual-paint-or-texture"])[0];
		}
		return true;
	}

	public void ClearWorldState()
	{
		if (CurrentState == State.Play)
		{
			Stop(force: true, resetBlocks: false);
		}
		worldSky = null;
		worldOcean = null;
		worldOceanBlock = null;
		rewardVisualizationGafs = null;
		fpsCounter = null;
		dynamicalLightChangers.Clear();
		if (f3PressedInCurrentWorld)
		{
			Tutorial.Stop();
		}
		f3PressedInCurrentWorld = false;
		signalNames = new string[26];
		blockNames = new Dictionary<Block, string>();
		BWSceneManager.ClearTerrainBlocks();
		BlockAbstractWater.waterCubes.Clear();
		BlockGroups.Clear();
		editorSelectionLocked.Clear();
		recentSelectionUnlockedBlock = null;
		keyLReleased = true;
		BlockMissile.WorldLoaded();
		if (Options.Cowlorded)
		{
			fpsCounter = new FpsCounter();
		}
		weather.Stop();
		weather = WeatherEffect.clear;
		SignalNameTileParameter.ResetDefaultIndex();
		Reset();
		blocksworldCamera.Reset();
		Scarcity.puzzleInventory = null;
		Scarcity.puzzleInventoryAfterStepByStep = null;
		launchIntoPlayMode = false;
		lockPull = false;
		cameraPoses.Clear();
		cameraPosesMap.Clear();
		staticLockPull = false;
		Scarcity.worldGAFUsage = new Dictionary<GAF, int>();
	}

	public bool LoadFromString(string s)
	{
		Reset();
		blocksworldCamera.Reset();
		Scarcity.puzzleInventory = null;
		Scarcity.puzzleInventoryAfterStepByStep = null;
		launchIntoPlayMode = false;
		lockPull = false;
		cameraPoses.Clear();
		cameraPosesMap.Clear();
		staticLockPull = false;
		bool flag = true;
		try
		{
			PerformaceTest performaceTest = new PerformaceTest("Blocksworld.LoadFromString");
			performaceTest.Start();
			performaceTest.StartSubTest("JSONDecoder.Decode");
			JObject obj = JSONDecoder.Decode(s);
			performaceTest.StopSubTest();
			int version = 25;
			performaceTest.StartSubTest("LoadFromJSON");
			flag = LoadFromJSON(obj, version);
			performaceTest.StopSubTest();
			performaceTest.Stop();
			performaceTest.DebugLogTestResults();
		}
		catch (Exception ex)
		{
			BWLog.Warning("Unable to decode JSON for '" + s + "'");
			BWLog.Warning(ex.GetType().Name + " exception message: " + ex.Message + " stacktrace: " + ex.StackTrace);
			flag = false;
		}
		return flag;
	}

	public void ProcessLoadedWorld()
	{
		UI.gameObject.SetActive(value: true);
		buildPanel.ResetState();
		buildPanel.UpdateGestureRecognizer(recognizer);
		UI.Controls.ResetContolVariants();
		UI.Controls.SetControlVariantsFromBlocks(BWSceneManager.AllBlocks());
		UI.Controls.ResetTiltPrompt();
		UpdateLightColor();
		Tutorial.UpdateCheatButton();
		History.Initialize();
		RemoveUnusedAssets();
		inBackground = false;
	}

	private void SetDynamicalLightBlocks()
	{
		List<Block> list = BWSceneManager.AllBlocks();
		for (int i = 0; i < list.Count; i++)
		{
			Block block = list[i];
			if (block.HasDynamicalLight() && !dynamicalLightChangers.Contains(block))
			{
				dynamicalLightChangers.Add(block);
			}
		}
	}

	private void Reset()
	{
		Deselect();
		TBox.Show(show: false);
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
		ClearChunks();
		mainCamera.backgroundColor = _defaultBackgroundColor;
		locked.Clear();
		foreach (Block block2 in Tutorial.blocks)
		{
			block2.Destroy();
		}
		Tutorial.blocks.Clear();
		buildPanel.PositionReset();
		copyModelAnimationCommand = new CopyModelAnimationCommand();
		saveModelAnimationCommand = new SaveModelAnimationCommand();
	}

	public static void GenerateTerrain(bool flat = false)
	{
		int num = 5;
		int num2 = 5;
		int num3 = 27;
		int num4 = num3;
		float num5 = Mathf.Floor(num / 2);
		float num6 = Mathf.Floor(num2 / 2);
		CreateTerrainBlock("Sky", Vector3.zero, Vector3.one, "Celeste", "Clouds");
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				float num7 = 0f - Mathf.Floor(num4 / 2);
				if (flat)
				{
					if (j == 0 || j == num - 1 || i == 0 || i == num2 - 1)
					{
						num7 += 2f + Mathf.Round(UnityEngine.Random.Range(0, 5));
					}
				}
				else if ((float)j != num5 || (float)i != num6)
				{
					num7 += Mathf.Round(UnityEngine.Random.Range(0, 10));
				}
				Vector3 vector = new Vector3(((float)j - num5) * (float)num3, num7, ((float)i - num6) * (float)num3);
				CreateTerrainBlock("Terrain Cube", vector + Mathf.Floor(num3 / 2) * Vector3.up, new Vector3(num3, 1f, num3), "Green", "Checkered");
				CreateTerrainBlock("Terrain Cube", vector - Vector3.up, new Vector3(num3, num4, num3), "Beige", "Checkered");
			}
		}
		ConnectednessGraph.Update(BWSceneManager.AllBlocks());
	}

	private static void CreateTerrainBlock(string type, Vector3 pos, Vector3 scale, string color, string texture)
	{
		List<List<Tile>> list = new List<List<Tile>>();
		list.Add(new List<Tile>());
		list[0].Add(new Tile(new GAF("Meta.Stop")));
		list[0].Add(new Tile(new GAF("Block.Create", type)));
		list[0].Add(new Tile(new GAF("Block.MoveTo", pos)));
		list[0].Add(new Tile(new GAF("Block.RotateTo", Vector3.zero)));
		list[0].Add(new Tile(new GAF("Block.PaintTo", color)));
		list[0].Add(new Tile(new GAF("Block.TextureTo", texture, Vector3.zero)));
		list[0].Add(new Tile(new GAF("Block.ScaleTo", scale)));
		list.Add(new List<Tile>());
		list[1].Add(new Tile(new GAF("Meta.Then")));
		list[1].Add(new Tile(new GAF("Block.Locked")));
		list.Add(new List<Tile>());
		list[2].Add(new Tile(new GAF("Meta.Then")));
		list[2].Add(new Tile(new GAF("Block.Fixed")));
		list.Add(new List<Tile>());
		list[3].Add(new Tile(new GAF("Meta.Then")));
		Block block = ((!(type == "Sky")) ? new Block(list) : new BlockSky(type, list));
		block.Reset();
		BWSceneManager.AddBlock(block);
	}

	private bool Close(Vector3 v1, Vector3 v2)
	{
		return (v2 - v1).sqrMagnitude < 0.1f;
	}

	public static List<List<Tile>> CloneBlockTiles(List<List<Tile>> tiles, Dictionary<Tile, Tile> origToCopy = null, bool excludeFirstRow = false, bool ignoreLockedGroupTiles = false)
	{
		List<List<Tile>> list = new List<List<Tile>>();
		int num = (excludeFirstRow ? 1 : 0);
		for (int i = num; i < tiles.Count; i++)
		{
			List<Tile> list2 = tiles[i];
			List<Tile> list3 = new List<Tile>();
			foreach (Tile item in list2)
			{
				if (!ignoreLockedGroupTiles || item.gaf.Predicate != Block.predicateGroup || !(BlockGroups.GroupType(item) == "locked-model"))
				{
					Tile tile = item.Clone();
					list3.Add(tile);
					if (origToCopy != null)
					{
						origToCopy[item] = tile;
					}
				}
			}
			list.Add(list3);
		}
		return list;
	}

	public static List<List<Tile>> CloneBlockTiles(Block block, bool excludeFirstRow = false, bool ignoreLockedGroupTiles = false)
	{
		return CloneBlockTiles(block.tiles, null, excludeFirstRow, ignoreLockedGroupTiles);
	}

	public static Block CloneBlock(Block block)
	{
		Block block2 = Block.NewBlock(CloneBlockTiles(block));
		BWSceneManager.AddBlock(block2);
		return block2;
	}

	private void UpdateCopyPaste()
	{
		if (CurrentState == State.Play)
		{
			return;
		}
		if (MappedInput.InputDown(MappableInput.COPY) && (selectedBlock != null || selectedBunch != null))
		{
			ButtonCopyTapped();
		}
		if (MappedInput.InputDown(MappableInput.CUT) && (selectedBlock != null || selectedBunch != null))
		{
			ButtonCopyTapped();
			List<Block> list = new List<Block>();
			if (selectedBlock != null)
			{
				list.Add(selectedBlock);
			}
			else if (selectedBunch != null)
			{
				list.AddRange(selectedBunch.blocks);
			}
			DeselectBlock();
			foreach (Block item in list)
			{
				if (item == worldOceanBlock)
				{
					worldOceanBlock = null;
					worldOcean = null;
				}
				DestroyBlock(item);
			}
		}
		if (keyboardPasteInProgress)
		{
			if (MappedInput.InputPressed(MappableInput.PASTE) && !Input.GetMouseButtonDown(0))
			{
				TBox.ContinueMove(NormalizedInput.mousePosition);
				return;
			}
			keyboardPasteInProgress = false;
			TBox.StopMove();
		}
		else
		{
			if (!MappedInput.InputDown(MappableInput.PASTE) || clipboard.modelCopyPasteBuffer.Count <= 0 || mouseBlock == null)
			{
				return;
			}
			Dictionary<GAF, int> missing = new Dictionary<GAF, int>();
			List<GAF> uniquesInModel = new List<GAF>();
			if (clipboard.AvailablityCountForBlockList(clipboard.modelCopyPasteBuffer, missing, uniquesInModel) == 0)
			{
				UI.Dialog.ShowPasteFailInfo(missing, uniquesInModel, "Not enough blocks!");
				return;
			}
			keyboardPasteInProgress = true;
			Vector3 vector = Util.Round(mouseBlockHitPosition);
			List<Block> list2 = PasteBlocks(clipboard.modelCopyPasteBuffer, vector);
			foreach (Block item2 in list2)
			{
				item2.Update();
			}
			Vector2 touchPos = mainCamera.WorldToScreenPoint(vector);
			selectedBlock = null;
			selectedBunch = null;
			SelectBunch(list2, silent: true);
			TBox.StartMove(touchPos, TBox.MoveMode.Raycast);
			Bounds bounds = Util.ComputeBoundsWithSize(list2, ignoreInvisible: false);
			Vector3 vector2 = mouseBlockNormal;
			float num = Mathf.Abs(vector2.x);
			float num2 = Mathf.Abs(vector2.y);
			float num3 = Mathf.Abs(vector2.z);
			float num4;
			if (num2 >= num && num2 >= num3)
			{
				vector2 = Vector3.up * Mathf.Sign(vector2.y);
				num4 = bounds.size.y;
			}
			else if (num > num3)
			{
				vector2 = Vector3.right * Mathf.Sign(vector2.x);
				num4 = bounds.size.x;
			}
			else
			{
				vector2 = Vector3.forward * Mathf.Sign(vector2.z);
				num4 = bounds.size.z;
			}
			vector = Util.Round(mouseBlockHitPosition + num4 * vector2);
			Vector2 touchPos2 = mainCamera.WorldToScreenPoint(vector);
			bool flag = TBox.ContinueMove(touchPos2, forceUpdate: true);
			TBox.PaintRed(!flag);
			TBox.Update();
		}
	}

	private void UpdateWASDEQMouseCameraMovement()
	{
		Transform transform = cameraTransform;
		Vector3 position = transform.position;
		float num = 15f;
		if (Input.GetMouseButton(1))
		{
			Vector3 vector = Vector3.zero;
			Vector3 vector2 = transform.forward * num;
			float num2 = 1f;
			if (MappedInput.InputPressed(MappableInput.SPEED_MULTIPLIER))
			{
				num2 = 4f;
			}
			else if (MappedInput.InputPressed(MappableInput.SLOW_MULTIPLIER))
			{
				num2 = 0.05f;
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
				vector = vector.normalized * num2;
				float num3 = Options.WASDMovementSpeedup;
				if (num3 < 0.0001f)
				{
					num3 = 1f;
				}
				vector *= num3;
			}
			_ = mousePositionDelta.sqrMagnitude;
			_ = 0.01f;
			Quaternion quaternion = Quaternion.AngleAxis(0.25f * mousePositionDelta.x, transform.up);
			Quaternion quaternion2 = Quaternion.AngleAxis(-0.25f * mousePositionDelta.y, transform.right);
			Vector3 normalized = (wasdeqMouseCamLookAtTarget - wasdeqMouseCamPosTarget).normalized;
			Vector3 vector3 = quaternion * normalized;
			vector3 = quaternion2 * vector3;
			vector2 = vector3 * num;
			if (true)
			{
				wasdeqMouseCamPosTarget += vector;
				wasdeqMouseCamLookAtTarget = wasdeqMouseCamPosTarget + vector2;
			}
			float wASDSmoothness = Options.WASDSmoothness;
			Vector3 vector4 = wASDSmoothness * position + (1f - wASDSmoothness) * wasdeqMouseCamPosTarget;
			Vector3 vector5 = wASDSmoothness * (position + cameraTransform.forward * num) + (1f - wASDSmoothness) * wasdeqMouseCamLookAtTarget;
			blocksworldCamera.SetCameraPosition(vector4);
			cameraTransform.LookAt(vector5);
			blocksworldCamera.SetTargetPosition(vector5);
			blocksworldCamera.SetFilteredPositionAndTarget();
		}
		else
		{
			wasdeqMouseCamPosTarget = position;
			wasdeqMouseCamLookAtTarget = position + cameraTransform.forward * num;
		}
	}

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
		Vector3 vector = ((selectedBlock == null) ? selectedBunch.GetPosition() : selectedBlock.GetPosition());
		Vector3 v = ((selectedBlock == null) ? selectedBunch.GetScale() : selectedBlock.Scale());
		Vector3 position = cameraTransform.position;
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
		blocksworldCamera.SetCameraPosition(vector2);
		blocksworldCamera.SetTargetPosition(vector);
		cameraTransform.LookAt(vector);
		if (MappedInput.InputPressed(MappableInput.CAM_LEFT))
		{
			blocksworldCamera.HardOrbit(-Vector2.right * 3f * num);
		}
		if (MappedInput.InputPressed(MappableInput.CAM_RIGHT))
		{
			blocksworldCamera.HardOrbit(Vector2.right * 3f * num);
		}
		if (MappedInput.InputPressed(MappableInput.CAM_UP))
		{
			blocksworldCamera.HardOrbit(Vector2.up * 3f * num);
		}
		if (MappedInput.InputPressed(MappableInput.CAM_DOWN))
		{
			blocksworldCamera.HardOrbit(-Vector2.up * 3f * num);
		}
	}

	private void UpdateAxisLockManipulation()
	{
		if (CurrentState == State.Build && Options.AxisLockMoveAndScaleEnabled)
		{
			if ((selectedBlock != null || selectedBunch != null) && (controlPressesInShortTime == 0 || Time.time < lastControlPressTime + 0.5f))
			{
				if (MappedInput.InputDown(MappableInput.AXIS_LOCK))
				{
					lastControlPressTime = Time.time;
					controlPressesInShortTime++;
				}
			}
			else if (!MappedInput.InputPressed(MappableInput.AXIS_LOCK))
			{
				controlPressesInShortTime = 0;
			}
			int num = (controlPressesInShortTime - 1) % 3;
			if (num == 0 || controlPressesInShortTime == 0)
			{
				constrainedManipulationAxis = Vector3.up;
			}
			else
			{
				switch (num)
				{
				case 1:
					constrainedManipulationAxis = Vector3.forward;
					break;
				case 2:
					constrainedManipulationAxis = Vector3.right;
					break;
				}
			}
			if (Time.time < lastControlPressTime + 1f)
			{
				Vector3 vector = Vector3.zero;
				bool flag = false;
				if (selectedBlock != null)
				{
					vector = selectedBlock.goT.position;
					flag = true;
				}
				else if (selectedBunch != null)
				{
					vector = Util.ComputeCenter(selectedBunch.blocks);
					flag = true;
				}
				if (flag)
				{
					UnityEngine.Debug.DrawLine(vector - constrainedManipulationAxis * 100f, vector + constrainedManipulationAxis * 100f, Color.white);
				}
			}
		}
		else
		{
			constrainedManipulationAxis = Vector3.up;
		}
	}

	private void UpdateQuickScroll()
	{
		if (CurrentState == State.Build && Options.QuickKeyScroll && buildPanel.IsShowing())
		{
			if (quickScrollKeys.Count > 0 && Time.time > lastQuickScrollPressTime + 0.2f)
			{
				int num = 0;
				int num2 = 1;
				for (int num3 = quickScrollKeys.Count - 1; num3 >= 0; num3--)
				{
					num += quickScrollKeys[num3] * num2;
					num2 *= 10;
				}
				Tile firstTileInSection = buildPanel.GetFirstTileInSection(num);
				if (firstTileInSection != null)
				{
					buildPanel.ScrollToVisible(firstTileInSection, immediately: true, showTileAtTopOfScreen: true, forceScroll: true);
				}
				quickScrollKeys.Clear();
			}
			int num4 = -1;
			if (MappedInput.InputDown(MappableInput.TILES_1))
			{
				num4 = 1;
			}
			if (MappedInput.InputDown(MappableInput.TILES_2))
			{
				num4 = 2;
			}
			if (MappedInput.InputDown(MappableInput.TILES_3))
			{
				num4 = 3;
			}
			if (MappedInput.InputDown(MappableInput.TILES_4))
			{
				num4 = 4;
			}
			if (MappedInput.InputDown(MappableInput.TILES_5))
			{
				num4 = 5;
			}
			if (MappedInput.InputDown(MappableInput.TILES_6))
			{
				num4 = 6;
			}
			if (MappedInput.InputDown(MappableInput.TILES_7))
			{
				num4 = 7;
			}
			if (MappedInput.InputDown(MappableInput.TILES_8))
			{
				num4 = 8;
			}
			if (MappedInput.InputDown(MappableInput.TILES_9))
			{
				num4 = 9;
			}
			if (MappedInput.InputDown(MappableInput.TILES_0))
			{
				num4 = 0;
			}
			if (num4 > -1)
			{
				quickScrollKeys.Add(num4);
				lastQuickScrollPressTime = Time.time;
			}
		}
		else
		{
			quickScrollKeys.Clear();
			lastQuickScrollPressTime = -1f;
		}
	}

	private void UpdateTimeChanges()
	{
		_ = CurrentState;
		_ = 1;
	}

	private void UpdateGlueJointShapeDisplays()
	{
		if (Options.GlueVolumeDisplay)
		{
			foreach (Block item in BWSceneManager.AllBlocks())
			{
				CollisionTest.DrawCollisionMeshes(item.glueMeshes, Color.red);
			}
		}
		if (Options.JointMeshDisplay)
		{
			foreach (Block item2 in BWSceneManager.AllBlocks())
			{
				CollisionTest.DrawCollisionMeshes(item2.jointMeshes, Color.black);
			}
		}
		if (!Options.ShapeVolumeDisplay)
		{
			return;
		}
		foreach (Block item3 in BWSceneManager.AllBlocks())
		{
			CollisionTest.DrawCollisionMeshes(item3.shapeMeshes, Color.blue);
		}
	}

	private void UpdateTutorialTiles()
	{
		if (selectedBlock == null || CurrentState != State.Build)
		{
			return;
		}
		Transform transform = cameraTransform;
		if (MappedInput.InputDown(MappableInput.APPLY_TUTORIAL_TILE) && Physics.Raycast(mainCamera.ScreenPointToRay(mouse * NormalizedScreen.scale), out var hitInfo))
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
			Block block = BWSceneManager.FindBlock(hitInfo.collider.gameObject, checkChildGos: true);
			int num = 0;
			if (block != null)
			{
				text2 = ((!flag) ? block.GetTexture() : block.GetPaint());
				num = BWSceneManager.IndexOfBlock(block);
				if (num == -1)
				{
					OnScreenLog.AddLogItem("Failed to find block index", 5f, log: true);
					num = 0;
				}
			}
			else
			{
				OnScreenLog.AddLogItem("Failed to find the target block. Using index 0", 5f, log: true);
			}
			List<Tile> list = new List<Tile>();
			list.Add(new Tile(new GAF("Meta.Then")));
			list.Add(new Tile(new GAF(text, num, text2, transform.position, hitInfo.point)));
			List<List<Tile>> tiles = selectedBlock.tiles;
			tiles.Insert(tiles.Count - 1, list);
			ShowSelectedBlockPanel();
			OnScreenLog.AddLogItem("Added " + text + " tile", 5f, log: true);
			History.AddStateIfNecessary();
		}
		if (MappedInput.InputDown(MappableInput.TUTORIAL_ROTATE_BLOCK) && Physics.Raycast(mainCamera.ScreenPointToRay(mouse * NormalizedScreen.scale), out var hitInfo2))
		{
			string text3 = "Block.TutorialRotateExistingBlock";
			Block block2 = BWSceneManager.FindBlock(hitInfo2.collider.gameObject, checkChildGos: true);
			int num2 = 0;
			Vector3 zero = Vector3.zero;
			if (block2 != null)
			{
				num2 = BWSceneManager.IndexOfBlock(block2);
				if (num2 == -1)
				{
					OnScreenLog.AddLogItem("Failed to find block index", 5f, log: true);
					num2 = 0;
				}
			}
			else
			{
				OnScreenLog.AddLogItem("Failed to find the target block. Using index 0 and rotation 0, 0, 0", 5f, log: true);
			}
			List<Tile> list2 = new List<Tile>();
			list2.Add(new Tile(new GAF("Meta.Then")));
			list2.Add(new Tile(new GAF(text3, num2, zero, transform.position, hitInfo2.point)));
			List<List<Tile>> tiles2 = selectedBlock.tiles;
			tiles2.Insert(tiles2.Count - 1, list2);
			ShowSelectedBlockPanel();
			OnScreenLog.AddLogItem("Added " + text3 + " tile", 5f, log: true);
			History.AddStateIfNecessary();
		}
		bool flag2 = MappedInput.InputDown(MappableInput.TUTORIAL_ADD_HINT);
		bool flag3 = MappedInput.InputDown(MappableInput.TUTORIAL_REMOVE_HINT);
		if (!(flag2 || flag3) || !Physics.Raycast(mainCamera.ScreenPointToRay(mouse * NormalizedScreen.scale), out var hitInfo3))
		{
			return;
		}
		Vector3 vector = Util.Round2(hitInfo3.point);
		Vector3 vector2 = Util.Round(hitInfo3.normal);
		Vector3 eulerAngles = transform.eulerAngles;
		float magnitude = (vector - cameraTransform.position).magnitude;
		List<Tile> list3 = new List<Tile>();
		list3.Add(new Tile(new GAF("Meta.Then")));
		if (flag2)
		{
			list3.Add(new Tile(new GAF("Block.TutorialCreateBlockHint", vector, vector2, eulerAngles, magnitude, 0, 0, 0, Vector3.zero)));
			BWLog.Info(string.Concat("Added hint pos ", vector, ", normal ", vector2, ", cam ", eulerAngles, ", dist ", magnitude, " to block ", hitInfo3.collider.gameObject.name));
		}
		else
		{
			Block block3 = BWSceneManager.FindBlock(hitInfo3.collider.gameObject, checkChildGos: true);
			if (block3 != null)
			{
				int num3 = BWSceneManager.IndexOfBlock(block3);
				if (num3 == -1)
				{
					OnScreenLog.AddLogItem("Failed to find block index", 5f, log: true);
					num3 = 0;
				}
				list3.Add(new Tile(new GAF("Block.TutorialRemoveBlockHint", num3, transform.position, hitInfo3.point)));
				BWLog.Info("Added remove block hint tile for the block " + block3.go.name + " with index " + num3);
			}
			else
			{
				OnScreenLog.AddLogItem("Failed to find the target block. Using index 0 and rotation 0, 0, 0", 5f, log: true);
			}
		}
		Block selectedScriptBlock = GetSelectedScriptBlock();
		selectedScriptBlock.tiles.Insert(1, list3);
		ShowSelectedBlockPanel();
		History.AddStateIfNecessary();
	}

	private void ToggleVolumeBlockVisibility()
	{
		foreach (Block item in BWSceneManager.AllBlocks())
		{
			if (item is BlockPosition)
			{
				BlockPosition blockPosition = (BlockPosition)item;
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

	private void ToggleSkyAndFogVisibility()
	{
		if (worldSky != null)
		{
			Renderer component = worldSky.go.GetComponent<Renderer>();
			if (component.enabled)
			{
				SetFog(2500f, 3000f);
				component.enabled = false;
			}
			else
			{
				component.enabled = true;
				storeFogDistances = GetDefaultFog();
				SetFog(storeFogDistances.x, storeFogDistances.y);
			}
		}
	}

	private Vector2 GetDefaultFog()
	{
		Vector2 result = new Vector2(40f, 100f);
		if (worldSky != null)
		{
			GAF simpleInitGAF = worldSky.GetSimpleInitGAF("Block.SetFog");
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

	private void UnityStandaloneUpdate()
	{
		if (CurrentState == State.Build)
		{
			StandaloneBuildModeUpdate();
		}
		else if (CurrentState == State.FrameCapture)
		{
			StandaloneFrameCaptureUpdate();
		}
		else if (CurrentState == State.Play)
		{
			StandalonePlayModeUpdate();
		}
	}

	private void StandaloneBuildModeUpdate()
	{
		if (MappedInput.InputDown(MappableInput.SAVE) && !Input.GetMouseButton(1))
		{
			WorldSession.Save();
		}
		UpdateCopyPaste();
		if (MappedInput.InputDown(MappableInput.CYCLE_BLOCK_PREV))
		{
			CycleSelectedScriptBlock(back: true);
		}
		if (MappedInput.InputDown(MappableInput.CYCLE_BLOCK_NEXT))
		{
			CycleSelectedScriptBlock(back: false);
		}
		if (MappedInput.InputDown(MappableInput.DELETE_BLOCK))
		{
			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
			{
				RemoveScriptsFromSelection();
			}
			else
			{
				List<Block> list = new List<Block>();
				if (selectedBlock != null)
				{
					list.Add(selectedBlock);
				}
				else if (selectedBunch != null)
				{
					list.AddRange(selectedBunch.blocks);
				}
				DeselectBlock();
				foreach (Block item in list)
				{
					if (item == worldOceanBlock)
					{
						worldOceanBlock = null;
						worldOcean = null;
					}
					DestroyBlock(item);
				}
			}
			Sound.PlayOneShotSound("Destroy");
			Scarcity.UpdateInventory();
			History.AddStateIfNecessary();
		}
		if (MappedInput.InputDown(MappableInput.UNDO))
		{
			if (Input.GetKey(KeyCode.LeftShift))
			{
				ButtonRedoTapped();
			}
			else
			{
				ButtonUndoTapped();
			}
		}
		if (MappedInput.InputDown(MappableInput.REDO))
		{
			ButtonRedoTapped();
		}
		bool flag = false;
		if (MappedInput.InputDown(MappableInput.TOGGLE_ORBIT_CAM))
		{
			cameraSelectionOrbitMode = !cameraSelectionOrbitMode;
			flag = cameraSelectionOrbitMode;
			Sound.PlayOneShotSound("Button Generic");
		}
		if ((MappedInput.InputDown(MappableInput.FOCUS_CAMERA) || flag) && (selectedBlock != null || selectedBunch != null))
		{
			Vector3 zero = Vector3.zero;
			Vector3 one = Vector3.one;
			if (selectedBunch != null)
			{
				zero = selectedBunch.GetPosition();
				one = selectedBunch.GetScale();
			}
			else
			{
				zero = selectedBlock.GetPosition();
				one = selectedBlock.Scale();
			}
			GoToCameraFrameFor(zero, one);
		}
		bool flag2 = MappedInput.InputDown(MappableInput.YANK_COLOR_AND_TEXTURE);
		bool flag3 = MappedInput.InputDown(MappableInput.APPLY_COLOR);
		bool flag4 = MappedInput.InputDown(MappableInput.APPLY_TEXTURE);
		if ((flag2 || flag3 || flag4) && mouseBlock != null)
		{
			Ray ray = mainCamera.ScreenPointToRay(mousePositionLast * NormalizedScreen.scale);
			Vector3 point = default(Vector3);
			int meshIndexForRay = mouseBlock.GetMeshIndexForRay(ray, refresh: true, out point, out point);
			if (flag2)
			{
				clipboard.SetPaintColor(mouseBlock.GetPaint(meshIndexForRay));
				clipboard.SetTexture(mouseBlock.GetTexture(meshIndexForRay));
			}
			else if (flag3)
			{
				mouseBlock.PaintTo((string)clipboard.GetLastPaintedColorGAF().Args[0], permanent: true, meshIndexForRay);
				History.AddStateIfNecessary();
			}
			else if (flag4)
			{
				mouseBlock.TextureTo((string)clipboard.GetLastTextureGAF().Args[0], mouseBlockNormal, permanent: true, meshIndexForRay);
				mouseBlock.ScaleTo(mouseBlockLast.Scale(), recalculateCollider: true, forceRescale: true);
				History.AddStateIfNecessary();
			}
		}
		if (MappedInput.InputDown(MappableInput.DESELECT_BLOCK))
		{
			DeselectBlock();
		}
		Vector3 v = Input.mousePosition / NormalizedScreen.scale;
		bool flag5 = v.x >= 0f && v.x <= (float)NormalizedScreen.width && v.y >= 0f && v.y <= (float)NormalizedScreen.height;
		bool flag6 = CurrentState == State.Build && buildPanel.Hit(v);
		float value = -50f * Input.GetAxis("Mouse ScrollWheel");
		value = Mathf.Clamp(value, -100f, 100f);
		float num = Mathf.Abs(value);
		if (flag6 && num > 0.1f)
		{
			buildPanel.Move(new Vector3(0f, value, 0f));
		}
		if (!flag6 && flag5)
		{
			float num2 = Mathf.Abs(value);
			if (num2 > 0.1f && !lockInput)
			{
				blocksworldCamera.ZoomBy((0f - value) * 10f);
			}
			if (cameraSelectionOrbitMode && Input.GetMouseButton(1) && (selectedBlock != null || selectedBunch != null))
			{
				UpdateWASDEQOrbitMouseCameraMovement();
			}
			else
			{
				UpdateWASDEQMouseCameraMovement();
			}
		}
	}

	private void StandaloneFrameCaptureUpdate()
	{
		Vector3 vector = Input.mousePosition / NormalizedScreen.scale;
		bool flag = vector.x >= 0f && vector.x <= (float)NormalizedScreen.width && vector.y >= 0f && vector.y <= (float)NormalizedScreen.height;
		float value = -50f * Input.GetAxis("Mouse ScrollWheel");
		value = Mathf.Clamp(value, -100f, 100f);
		float num = Mathf.Abs(value);
		if (flag)
		{
			float num2 = Mathf.Abs(value);
			if (num2 > 0.1f && !lockInput)
			{
				blocksworldCamera.ZoomBy((0f - value) * 10f);
			}
			UpdateWASDEQMouseCameraMovement();
		}
	}

	private void StandalonePlayModeUpdate()
	{
		if (Input.GetMouseButton(1))
		{
			blocksworldCamera.OrbitBy(-mousePositionDelta);
		}
		float value = -50f * Input.GetAxis("Mouse ScrollWheel");
		value = Mathf.Clamp(value, -100f, 100f);
		float num = Mathf.Abs(value);
		if (num > 0.1f && !lockInput)
		{
			blocksworldCamera.ZoomBy((0f - value) * 10f);
		}
	}

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
		foreach (KeyValuePair<string, int> item in dictionary)
		{
			int num = item.Value;
			if (materialUsage.TryGetValue(item.Key, out var value))
			{
				num -= value;
			}
			if (num > 0)
			{
				stringBuilder2.Append(num + " new materials with name: " + item.Key + "\n");
			}
			materialUsage[item.Key] = item.Value;
		}
		BWLog.Info(stringBuilder.ToString());
		BWLog.Info(stringBuilder2.ToString());
		BWLog.Info("Materials Cache size: " + Materials.materialCache.Count);
		BWLog.Info("Material paints cache size: " + Materials.materialCachePaint.Count);
		BWLog.Info("Material texture cache size: " + Materials.materialCacheTexture.Count);
	}

	private void Update()
	{
		if (!IsStarted())
		{
			return;
		}
		if (inBackground)
		{
			TileIconManager.Instance.Update();
			return;
		}
		TileIconManager.Instance.Update();
		if (WorldSession.current != null)
		{
			if (MappedInput.InputDown(MappableInput.PLAY))
			{
				if (CurrentState == State.Play)
				{
					ButtonPauseTapped();
				}
				else
				{
					ButtonPlayTapped();
				}
			}
			if (MappedInput.InputDown(MappableInput.STOP) && CurrentState == State.Play && WorldSession.isNormalBuildAndPlaySession())
			{
				ButtonStopTapped();
			}
			if (MappedInput.InputDown(MappableInput.RESTART_PLAY) && CurrentState == State.Play)
			{
				ButtonRestartTapped();
			}
		}
		MappedInput.Update();
		displayString = string.Empty;
		cameraForward = cameraTransform.forward;
		cameraUp = cameraTransform.up;
		cameraRight = cameraTransform.right;
		cameraPosition = cameraTransform.position;
		cameraMoved = (double)(cameraPosition - prevCamPos).sqrMagnitude > 9.999999776482583E-05;
		if (cameraMoved)
		{
			frustumPlanes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
			prevCamPos = cameraPosition;
		}
		Sound.sfxEnabled = true;
		if (MappedInput.InputDown(MappableInput.SCREENSHOT))
		{
			ButtonCaptureTapped();
		}
		OnScreenLog.Update();
		if (CurrentState != State.WaitForOption && CurrentState != State.EditTile)
		{
			UnityStandaloneUpdate();
		}
		if (consumeEvent)
		{
			consumeEvent = false;
			return;
		}
		deltaTime = Time.realtimeSinceStartup - lastRealtimeSinceStartup;
		lastRealtimeSinceStartup = Time.realtimeSinceStartup;
		updateCounter++;
		gameTime = (float)updateCounter * Time.deltaTime;
		_stateTime += Time.deltaTime;
		mouse = Input.mousePosition / screenScale;
		numTouches = 0;
		if (Input.touchCount == 0)
		{
			touches[0] = Input.mousePosition / screenScale;
			if (Input.GetMouseButton(0) && !UI.IsBlocking(touches[0]))
			{
				numTouches++;
			}
		}
		else
		{
			for (int i = 0; i < Input.touchCount; i++)
			{
				UnityEngine.Touch touch = Input.GetTouch(i);
				touches[numTouches] = touch.position / screenScale;
				if (!UI.IsBlocking(touches[numTouches]))
				{
					numTouches++;
				}
			}
		}
		if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
		{
			mousePositionLast = Input.mousePosition;
		}
		mousePositionDelta = Input.mousePosition - mousePositionLast;
		mousePositionLast = Input.mousePosition;
		if (CurrentState != State.Play)
		{
			mouseBlockLast = mouseBlock;
			mouseBlockNormalLast = mouseBlockNormal;
			mouseBlock = BlockAtMouse(NormalizedInput.mousePosition, out mouseBlockIndex);
		}
		if (CurrentState != State.Menu && !lockInput)
		{
			for (int j = 0; j < buildOnlyGestures.Length; j++)
			{
				buildOnlyGestures[j].IsEnabled = CurrentState == State.Build && !forcePlayMode;
			}
			bool isEnabled = CurrentState == State.Play;
			pullObject.IsEnabled = isEnabled;
			tapControl.IsEnabled = isEnabled;
			recognizer.Update();
		}
		if (!inBackground)
		{
			buildPanel.Update();
			scriptPanel.Update();
			blocksworldCamera.Update();
			if (CurrentState != State.Play)
			{
				TBox.Update();
			}
			StatePlayUpdate();
			if (Input.GetMouseButtonDown(0))
			{
				mousePositionFirst = NormalizedInput.mousePosition;
			}
			if (CurrentState == State.FrameCapture && timerStart >= 0f)
			{
				timerStart += Time.deltaTime;
			}
			if (CurrentState == State.Build && !f3PressedInCurrentWorld)
			{
				WorldSession.FastSaveAutoUpdate();
			}
			Tutorial.Update();
			Util.Update();
			UpdateDynamicalLights();
			if (stopASAP)
			{
				Stop();
				stopASAP = false;
			}
			Command.ExecuteCommands(updateCommands);
			OnHudMesh();
			if (WorldSession.current != null)
			{
				WorldSession.current.UpdateLoop();
			}
		}
	}

	public static void SelectionLock(Block b)
	{
		editorSelectionLocked.Add(b);
	}

	public static bool IsSelectionLocked(Block b)
	{
		return editorSelectionLocked.Contains(b);
	}

	private static void UpdateModelUnlockInput()
	{
		if (CurrentState != State.Build || !MappedInput.InputDown(MappableInput.MODEL_UNLOCK) || selectedBunch == null)
		{
			return;
		}
		if (selectedBunch.blocks.Exists((Block b) => b.HasGroup("locked-model")))
		{
			selectedBunch.blocks.ForEach(delegate(Block b)
			{
				BlockGroup groupOfType = b.GetGroupOfType("locked-model");
				if (groupOfType is LockedModelBlockGroup)
				{
					b.RemoveGroup("locked-model");
					BlockGroups.RemoveGroup(groupOfType);
				}
			});
			SelectBunch(selectedBunch.blocks);
		}
		else
		{
			BlockGroups.AddGroup(selectedBunch.blocks, "locked-model");
			Select(selectedBunch.blocks[0]);
		}
	}

	private static void UpdateSelectionLockInput()
	{
		if (CurrentState != State.Build)
		{
			return;
		}
		if (!MappedInput.InputPressed(MappableInput.SELECTION_LOCK))
		{
			keyLReleased = true;
			recentSelectionUnlockedBlock = null;
		}
		if (!MappedInput.InputDown(MappableInput.SELECTION_LOCK))
		{
			return;
		}
		keyLReleased = false;
		if (selectedBunch != null)
		{
			OnScreenLog.AddLogItem("Selection-locked an entire model");
			foreach (Block block in selectedBunch.blocks)
			{
				SelectionLock(block);
			}
			Select(null);
		}
		else if (selectedBlock != null)
		{
			OnScreenLog.AddLogItem("Selection-locked a block of type '" + selectedBlock.BlockType() + "'");
			SelectionLock(selectedBlock);
			Select(null);
		}
	}

	public static void UpdateDynamicalLights(bool updateFog = true, bool forceUpdate = false)
	{
		Color color = lightColor;
		Vector4 vector = dynamicLightColor;
		float num = fogMultiplier;
		float num2 = weather.GetFogMultiplier();
		Color white = Color.white;
		float num3 = 1f;
		for (int i = 0; i < dynamicalLightChangers.Count; i++)
		{
			ILightChanger lightChanger = dynamicalLightChangers[i];
			Color dynamicalLightTint = lightChanger.GetDynamicalLightTint();
			color *= dynamicalLightTint;
			num2 *= lightChanger.GetFogMultiplier();
			white *= lightChanger.GetFogColorOverride();
			num3 *= lightChanger.GetLightIntensityMultiplier();
		}
		Vector4 vector2 = fogColor;
		Vector4 vector3 = ((!(white == Color.white)) ? white : fogColor);
		Vector4 vector4 = color;
		if (!(Mathf.Abs(num3 - dynamicLightIntensityMultiplier) > 0.01f || (vector4 - vector).sqrMagnitude > 0.001f || (vector3 - vector2).sqrMagnitude > 0.001f || Mathf.Abs(num - num2) > 0.01f || forceUpdate))
		{
			return;
		}
		dynamicLightColor = color;
		Light component = directionalLight.GetComponent<Light>();
		component.color = color;
		component.intensity = num3;
		dynamicLightIntensityMultiplier = num3;
		if (updateFog && worldSky != null)
		{
			if (white != Color.white)
			{
				UpdateFogColor(white * color);
			}
			else
			{
				UpdateFogColor(BlockSky.GetFogColor());
			}
			if (Mathf.Abs(num2 - num) > 0.001f)
			{
				bw.SetFogMultiplier(num2);
			}
		}
	}

	private void FixedUpdate()
	{
		if (!IsStarted() || inBackground)
		{
			return;
		}
		fixedDeltaTime = Time.fixedDeltaTime;
		if (CurrentState == State.Play)
		{
			StatePlayFixedUpdate();
			bool showControls = MappedInput.InputDown(MappableInput.SHOW_CONTROLS) || TimeInCurrentState() < 1f;
			UI.Controls.UpdateAll(showControls);
		}
		foreach (KeyValuePair<BlockAnimatedCharacter, CharacterStateHandler> stateController in BlockAnimatedCharacter.stateControllers)
		{
			stateController.Value.Update();
		}
		if (pullObject != null && pullObject.IsEnabled)
		{
			pullObject.FixedUpdate();
		}
		if (rewardVisualizationGafs != null && !waitForSetPurchase)
		{
			if (rewardVisualizationIndex >= rewardVisualizationGafs.Count)
			{
				rewardVisualizationGafs = null;
				winIsWaiting = false;
				hasWon = true;
				WorldSession.current.OnRewardVisualizationComplete();
			}
			else
			{
				rewardExecutionInfo.timer += fixedDeltaTime;
				GAF gAF = rewardVisualizationGafs[rewardVisualizationIndex];
				List<Block> list = BWSceneManager.AllBlocks();
				if (gAF == null || list.Count == 0 || gAF.RunAction(list[0], rewardExecutionInfo) == TileResultCode.True)
				{
					rewardVisualizationIndex++;
					rewardExecutionInfo.timer = 0f;
				}
			}
		}
		else if (CurrentState != State.WaitForOption && winIsWaiting && !waitForSetPurchase)
		{
			hasWon = true;
			winIsWaiting = false;
		}
		blocksworldCamera.FixedUpdate();
		Scarcity.StepInventoryScales();
		UI.UpdateSpeechBubbles();
		UI.UpdateTextWindows();
		UI.Controls.HandleInputControlVisibility(CurrentState);
		Command.ExecuteCommands(fixedUpdateCommands);
	}

	public void StatePlayUpdate()
	{
		if (CurrentState == State.Play)
		{
			BWSceneManager.ExecutePlayBlocksUpdate();
		}
		else
		{
			BWSceneManager.ExecuteAllBlocksUpdate();
		}
		weather.Update();
		leaderboardData.UpdateGUI();
		VisualEffect.UpdateVfxs();
	}

	private void StatePlayFixedUpdate()
	{
		BWSceneManager.ModifyPlayAndScriptBlocks();
		if (isFirstFrame)
		{
			BWSceneManager.RunFirstFrameActions();
			isFirstFrame = false;
		}
		BWSceneManager.RunConditions();
		ResetState();
		BWSceneManager.RunActions();
		BWSceneManager.RunFixedUpdate();
		VisualEffect.FixedUpdateVfxs();
		TreasureHandler.FixedUpdate();
		CollisionManager.FixedUpdate();
		Block.UpdateOverridenMasses();
		BlockAnimatedCharacter.PlayQueuedHitReacts();
		blocksworldCamera.FinalUpdateFirstPersonFollow();
		Block.goTouchStarted = false;
		for (int i = 0; i < chunks.Count; i++)
		{
			chunks[i].ApplyCenterOfMassChanges();
		}
		playFixedUpdateCounter++;
	}

	private void ResetState()
	{
		gameStart = false;
		blocksworldCamera.Reset();
		CollisionManager.ResetState();
		if (!Input.GetMouseButton(0))
		{
			Block.goTouchStarted = false;
			Block.goTouched = null;
		}
		for (int i = 0; i < 26; i++)
		{
			sending[i] = false;
			sendingValues[i] = 0f;
		}
		sendingCustom.Clear();
		BWSceneManager.ResetFrame();
		BlockAbstractBow.ClearHits();
		BlockAbstractLaser.ClearHits();
		BlockGravityGun.ClearHits();
		BlockAnimatedCharacter.ClearAttackFlags();
		TagManager.ClearRegisteredBlocks();
		BlockAccelerations.ResetFrame();
		TreasureHandler.ResetState();
		Sound.ResetState();
		Command.ExecuteCommands(resetStateCommands);
	}

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
		if (selectedBlock == null || !list.Contains(selectedBlock))
		{
			SelectBlock(list[0]);
			blocksworldCamera.Focus();
			return;
		}
		int num = list.IndexOf(selectedBlock);
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
		SelectBlock(list[num]);
		blocksworldCamera.Focus();
	}

	public Block AddNewBlock(Tile type, bool addToBlocks = true, List<Tile> initTiles = null, bool defaultColors = true)
	{
		List<List<Tile>> list = new List<List<Tile>>();
		list.Add(new List<Tile>());
		list[0].Add(new Tile(new GAF("Meta.Stop")));
		list[0].Add(type);
		list[0].Add(new Tile(new GAF("Block.MoveTo", Util.nullVector3)));
		list[0].Add(new Tile(new GAF("Block.RotateTo", Vector3.zero)));
		list[0].Add(new Tile(new GAF("Block.PaintTo", "Grey")));
		if (((string)type.gaf.Args[0]).StartsWith("Decal "))
		{
			list[0].Add(new Tile(new GAF("Block.TextureTo", ((string)type.gaf.Args[0]).Substring("Decal ".Length), Vector3.zero)));
		}
		else
		{
			list[0].Add(new Tile(new GAF("Block.TextureTo", "Plain", Vector3.zero)));
		}
		list[0].Add(new Tile(new GAF("Block.ScaleTo", Vector3.one)));
		if (initTiles != null)
		{
			list[0].AddRange(initTiles);
		}
		list.Add(new List<Tile>());
		list[1].Add(new Tile(new GAF("Meta.Then")));
		bool defaultTiles = Tutorial.state == TutorialState.None || Tutorial.includeDefaultBlockTiles;
		Block block = Block.NewBlock(list, defaultColors, defaultTiles);
		block.RotateTo(Quaternion.Euler(0f, 180f + 90f * Mathf.Floor(cameraTransform.eulerAngles.y / 90f), 0f) * block.goT.rotation);
		if (addToBlocks)
		{
			BWSceneManager.AddBlock(block);
		}
		scriptPanel.Show(show: false);
		return block;
	}

	private Vector3 WorldPlanePos(Vector3 normal, Vector3 point, Vector3 screenPos)
	{
		Plane plane = new Plane(normal, point);
		Ray ray = mainCamera.ScreenPointToRay(screenPos);
		plane.Raycast(ray, out var enter);
		return ray.GetPoint(enter);
	}

	public static void DisableRenderer(GameObject parent)
	{
		if (parent.GetComponent<Renderer>() != null)
		{
			parent.GetComponent<Renderer>().enabled = false;
		}
		foreach (object item in parent.transform)
		{
			Transform transform = (Transform)item;
			transform.GetComponent<Renderer>().enabled = false;
		}
	}

	public static void DestroyBunch(Bunch bunch)
	{
		if (bunch == null)
		{
			return;
		}
		foreach (Block item in new List<Block>(bunch.blocks))
		{
			DestroyBlock(item);
		}
	}

	public static void DestroyBlock(Block block)
	{
		if (selectedBunch != null)
		{
			selectedBunch.Remove(block);
			if (selectedBunch.blocks.Count == 0)
			{
				Deselect();
			}
		}
		BWSceneManager.RemoveBlock(block);
		block.Destroy();
		if (block is BlockGrouped { group: not null } blockGrouped)
		{
			Block[] blocks = blockGrouped.group.GetBlocks();
			Block[] array = blocks;
			foreach (Block block2 in array)
			{
				if (block2.go != null && block2 != block)
				{
					DestroyBlock(block2);
				}
			}
		}
		Block.ClearConnectedCache();
	}

	public static Block BlockAtMouse(Vector3 mouse, out int meshIndex)
	{
		meshIndex = -1;
		if (!UI.SidePanel.Hit(mouse) && !scriptPanel.Hit(mouse))
		{
			Ray ray = mainCamera.ScreenPointToRay(mouse * NormalizedScreen.scale);
			RaycastHit[] array = Physics.RaycastAll(ray);
			if (array.Length != 0)
			{
				float num = float.MaxValue;
				Block result = null;
				for (int i = 0; i < 2; i++)
				{
					RaycastHit[] array2 = array;
					for (int j = 0; j < array2.Length; j++)
					{
						RaycastHit raycastHit = array2[j];
						float num2 = raycastHit.distance;
						if (!(num2 + (float)i <= num))
						{
							continue;
						}
						Block block = BWSceneManager.FindBlock(raycastHit.collider.gameObject, i == 1);
						if (block == null)
						{
							continue;
						}
						if (block is BlockVolumeBlock)
						{
							num2 += 0.01f;
							if (num2 + (float)i > num)
							{
								continue;
							}
						}
						if ((!CharacterEditor.Instance.InEditMode() || CharacterEditor.Instance.IsCharacterBlock(block) || CharacterEditor.Instance.IsCharacterAttachment(block)) && block != null && (!Tutorial.FilterOutMouseBlock() || Tutorial.BlockCanBeMouseBlock(block)))
						{
							num = num2;
							mouseBlockNormal = Quaternion.Inverse(raycastHit.collider.gameObject.transform.rotation) * raycastHit.normal;
							mouseBlockHitPosition = raycastHit.point;
							result = block;
						}
					}
				}
				return result;
			}
		}
		return null;
	}

	private static Block LonelyBlock(Bunch bunch)
	{
		if (selectedBunch.blocks.Count == 1)
		{
			return selectedBunch.blocks[0];
		}
		Block block = null;
		foreach (Block block2 in selectedBunch.blocks)
		{
			if (block != null)
			{
				return null;
			}
			block = block2;
		}
		return block;
	}

	public static void Select(Block block, bool silent = false, bool updateTiles = true)
	{
		if (CharacterEditor.Instance.InEditMode())
		{
			return;
		}
		bool flag = editorSelectionLocked.Contains(block);
		if (BW.isUnityEditor)
		{
			if (flag)
			{
				if (MappedInput.InputPressed(MappableInput.SELECTION_LOCK))
				{
					editorSelectionLocked.Remove(block);
					recentSelectionUnlockedBlock = block;
					OnScreenLog.AddLogItem("Selection-unlocked a block of type '" + block.BlockType() + "'");
					if (!silent)
					{
						Sound.PlayOneShotSound("Move");
					}
				}
				else
				{
					OnScreenLog.AddLogItem("Trying to select a selection-locked block. Hold the 'L' key and click on the block to unlock it.");
				}
				return;
			}
			if (!keyLReleased && recentSelectionUnlockedBlock != null && recentSelectionUnlockedBlock == block)
			{
				OnScreenLog.AddLogItem("Selection-unlocked the entire model");
				List<Block> list = ConnectednessGraph.ConnectedComponent(block, 3);
				foreach (Block item in list)
				{
					editorSelectionLocked.Remove(item);
				}
				if (!silent)
				{
					Sound.PlayOneShotSound("Move");
				}
				recentSelectionUnlockedBlock = null;
				keyLReleased = true;
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
		bool flag2 = block?.HasGroup("locked-model") ?? false;
		BlockGrouped blockGrouped = block as BlockGrouped;
		if (flag2)
		{
			list2 = block.GetGroupOfType("locked-model").GetBlockList();
		}
		else if (blockGrouped != null)
		{
			list2 = blockGrouped.group.GetBlockList();
		}
		else if (block is BlockCharacter && !block.IsProfileCharacter())
		{
			list2 = GetBlocksterPlusGearList((BlockCharacter)block);
		}
		else if (block != null && block.connections.Find((Block block2) => block2 is BlockCharacter && !block2.IsProfileCharacter()) is BlockCharacter b && IsBlocksterGear(block))
		{
			list2 = GetBlocksterPlusGearList(b);
		}
		if (block != null)
		{
			if (list2 != null && list2.Count > 1)
			{
				if (selectedBunch != null)
				{
					bool flag3 = selectedBunch.blocks.Contains(block);
					HashSet<Block> bunchSet = GetBunchSet(list2);
					if (flag3 && bunchSet.Count > selectedBunch.blocks.Count)
					{
						DeselectBunch();
						SelectBunch(new List<Block>(bunchSet), silent, updateTiles);
						return;
					}
					DeselectBunch();
					if (flag2)
					{
						SelectBunch(list2, silent, updateTiles);
					}
					else
					{
						SelectBlock(block, silent, updateTiles);
					}
				}
				else if (selectedBlock != null)
				{
					if (block == selectedBlock && (Tutorial.state == TutorialState.None || Tutorial.state == TutorialState.SelectBunch))
					{
						DeselectBlock(silent, updateTiles: false);
						SelectBunch(list2, silent, updateTiles);
						return;
					}
					DeselectBlock(silent, updateTiles: false);
					if (flag2)
					{
						SelectBunch(list2, silent, updateTiles);
					}
					else
					{
						SelectBlock(block, silent, updateTiles);
					}
				}
				else if (flag2)
				{
					SelectBunch(list2, silent, updateTiles);
				}
				else
				{
					SelectBlock(block, silent, updateTiles);
				}
			}
			else if (block != selectedBlock)
			{
				if (selectedBunch != null)
				{
					DeselectBunch();
				}
				if (selectedBlock != null)
				{
					DeselectBlock(silent, updateTiles: false);
				}
				SelectBlock(block, silent, updateTiles);
			}
			else if (block == selectedBlock && selectedBunch == null && (Tutorial.state == TutorialState.None || Tutorial.state == TutorialState.SelectBunch))
			{
				DeselectBlock(silent, updateTiles: false);
				SelectBunch(block, silent, updateTiles);
			}
			else if (block == selectedBlock && selectedBunch != null)
			{
				DeselectBunch();
				SelectBlock(block, silent, updateTiles);
			}
		}
		else
		{
			DeselectBunch();
			DeselectBlock(silent, updateTiles);
		}
	}

	private static HashSet<Block> GetBunchSet(List<Block> list)
	{
		HashSet<Block> hashSet = new HashSet<Block>();
		foreach (Block item in list)
		{
			hashSet.UnionWith(GetBunchBlocks(item));
		}
		return hashSet;
	}

	public static bool IsBlocksterGear(Block cb)
	{
		int tabIndexForGaf = PanelSlots.GetTabIndexForGaf(new GAF(Block.predicateCreate, cb.BlockType()));
		return tabIndexForGaf == 6;
	}

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

	public static List<Block> GetBlocksterPlusGearList(BlockCharacter b)
	{
		List<Block> list = new List<Block>();
		list.Add(b);
		foreach (Block connection in b.connections)
		{
			if (IsBlocksterGear(connection))
			{
				list.Add(connection);
			}
		}
		return list;
	}

	public static bool SelectedBunchIsGroup()
	{
		if (selectedBunch != null)
		{
			List<Block> list = new List<Block>();
			int num = -1;
			foreach (Block block in selectedBunch.blocks)
			{
				if (!(block is BlockGrouped blockGrouped))
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
			return list.Count == selectedBunch.blocks.Count;
		}
		return false;
	}

	public static List<Block> GetBunchBlocks(Block block)
	{
		List<Block> list = ConnectednessGraph.ConnectedComponent(block, 3);
		List<Block> list2 = new List<Block>();
		foreach (Block item in list)
		{
			if (!IsSelectionLocked(item))
			{
				list2.Add(item);
			}
		}
		HashSet<Block> hashSet = new HashSet<Block>(list2);
		foreach (Block item2 in new List<Block>(list2))
		{
			if (!(item2 is BlockGrouped blockGrouped))
			{
				continue;
			}
			Block[] blocks = blockGrouped.group.GetBlocks();
			Block[] array = blocks;
			foreach (Block block2 in array)
			{
				List<Block> list3 = ConnectednessGraph.ConnectedComponent(block2, 3);
				foreach (Block item3 in list3)
				{
					if (!hashSet.Contains(item3))
					{
						hashSet.Add(item3);
						list2.Add(item3);
					}
				}
			}
		}
		return list2;
	}

	public static void SelectBunch(Block block, bool silent = false, bool updateTiles = true)
	{
		List<Block> bunchBlocks = GetBunchBlocks(block);
		if (bunchBlocks.Count <= 1)
		{
			if (!IsSelectionLocked(block))
			{
				SelectBlock(block, silent);
			}
			return;
		}
		selectedBunch = new Bunch();
		for (int i = 0; i < bunchBlocks.Count; i++)
		{
			Block b = bunchBlocks[i];
			if (!IsSelectionLocked(b))
			{
				selectedBunch.Add(b);
			}
		}
		SortBlocksAsInWorld(selectedBunch.blocks);
		TBox.Attach(selectedBunch, silent);
		TBox.Show(show: true);
		blocksworldCamera.Follow(selectedBunch);
		if (updateTiles)
		{
			UpdateTiles();
			Scarcity.UpdateInventory();
		}
	}

	private static void UpdateMouseBlock(Block block)
	{
		mouseBlockLast = mouseBlock;
		mouseBlockNormalLast = mouseBlockNormal;
		mouseBlock = block;
	}

	public static void SelectBunch(List<Block> blocks, bool silent = false, bool updateTiles = true)
	{
		if (blocks.Count <= 0)
		{
			return;
		}
		if (blocks.Count == 1)
		{
			if (!IsSelectionLocked(blocks[0]))
			{
				SelectBlock(blocks[0], silent);
				UpdateMouseBlock(blocks[0]);
			}
			return;
		}
		selectedBunch = new Bunch();
		for (int i = 0; i < blocks.Count; i++)
		{
			Block b = blocks[i];
			if (!IsSelectionLocked(b))
			{
				selectedBunch.Add(b);
			}
		}
		SortBlocksAsInWorld(selectedBunch.blocks);
		TBox.Attach(selectedBunch, silent);
		TBox.Show(show: true);
		blocksworldCamera.Follow(selectedBunch);
		if (SelectedBunchIsGroup())
		{
			ShowSelectedBlockPanel();
		}
		if (updateTiles)
		{
			UpdateTiles();
			Scarcity.UpdateInventory();
		}
	}

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

	public static void DeselectBunch()
	{
		blocksworldCamera.Unfollow();
		if (selectedBunch != null && scriptPanel.IsShowing())
		{
			scriptPanel.Show(show: false);
		}
		selectedBunch = null;
	}

	public static void SelectBlock(Block block, bool silent = false, bool updateTiles = true)
	{
		if (block != null && block.go == null)
		{
			BWLog.Info("Tried to select an already destroyed block");
			return;
		}
		selectedBlock = block;
		TBox.Attach(selectedBlock, silent);
		TBox.Show(show: true);
		blocksworldCamera.Follow(block);
		if (updateTiles)
		{
			UpdateTiles();
			Scarcity.UpdateInventory();
		}
		if (locked.Contains(block))
		{
			scriptPanel.Show(show: false);
		}
		else
		{
			ShowSelectedBlockPanel();
		}
	}

	public static void ScrollToFirstBlockSpecificTile(Block block)
	{
		bool flag = Tutorial.state == TutorialState.None || Tutorial.mode == TutorialMode.Puzzle;
		flag &= !Options.DisableAutoScrollToScriptTile;
		bool flag2 = buildPanel.GetTabBar().SelectedTab == TabBarTabId.Actions;
		if (!(flag && flag2))
		{
			return;
		}
		BlockMetaData blockMetaData = block.GetBlockMetaData();
		if (!(blockMetaData != null) || !blockMetaData.scrollToScriptTileOnSelect)
		{
			return;
		}
		HashSet<Predicate> hashSet = new HashSet<Predicate>(PredicateRegistry.ForBlock(block, includeBaseTypes: false));
		if (hashSet.Count > 0)
		{
			Tile tile = buildPanel.FindFirstTileWithPredicate(hashSet);
			if (tile != null)
			{
				buildPanel.ScrollToVisible(tile, immediately: true);
			}
		}
	}

	public static void ShowSelectedBlockPanel()
	{
		scriptPanel.ClearTiles();
		Block selectedScriptBlock = GetSelectedScriptBlock();
		if (selectedBlock != null && selectedBlock.HasGroup("locked-model"))
		{
			scriptPanel.Show(show: false);
			return;
		}
		scriptPanel.SetTilesFromBlock(selectedScriptBlock);
		scriptPanel.Show(show: true);
		scriptPanel.UpdateGestureRecognizer(recognizer);
		scriptPanel.Position();
		scriptPanel.Layout();
	}

	public static void DeselectBlock(bool silent = false, bool updateTiles = true)
	{
		TBox.Detach(silent);
		TBox.Show(show: false);
		blocksworldCamera.Unfollow();
		if (selectedBlock != null)
		{
			scriptPanel.Show(show: false);
			selectedBlock = null;
			if (updateTiles)
			{
				UpdateTiles();
			}
		}
	}

	public static void Deselect(bool silent = false, bool updateTiles = true)
	{
		Select(null, silent, updateTiles);
		Select(null, silent, updateTiles);
	}

	public static HashSet<Predicate> GetTaggedPredicates()
	{
		if (taggedPredicates == null)
		{
			taggedPredicates = new HashSet<Predicate>(new Predicate[69]
			{
				PredicateRegistry.ByName("Block.TagVisibilityCheck"),
				PredicateRegistry.ByName("Position.IsWithin"),
				PredicateRegistry.ByName("Sphere.MoveToTag"),
				PredicateRegistry.ByName("Sphere.MoveThroughTag"),
				PredicateRegistry.ByName("Sphere.AvoidTag"),
				PredicateRegistry.ByName("Legs.GotoTag"),
				PredicateRegistry.ByName("Quadped.GotoTag"),
				PredicateRegistry.ByName("MLPLegs.GotoTag"),
				PredicateRegistry.ByName("Character.GotoTag"),
				PredicateRegistry.ByName("AnimCharacter.GotoTag"),
				PredicateRegistry.ByName("Legs.ChaseTag"),
				PredicateRegistry.ByName("Quadped.ChaseTag"),
				PredicateRegistry.ByName("MLPLegs.ChaseTag"),
				PredicateRegistry.ByName("Character.ChaseTag"),
				PredicateRegistry.ByName("AnimCharacter.ChaseTag"),
				PredicateRegistry.ByName("Legs.TurnTowardsTag"),
				PredicateRegistry.ByName("Quadped.TurnTowardsTag"),
				PredicateRegistry.ByName("MLPLegs.TurnTowardsTag"),
				PredicateRegistry.ByName("Character.TurnTowardsTag"),
				PredicateRegistry.ByName("AnimCharacter.TurnTowardsTag"),
				PredicateRegistry.ByName("Legs.AvoidTag"),
				PredicateRegistry.ByName("Quadped.AvoidTag"),
				PredicateRegistry.ByName("MLPLegs.AvoidTag"),
				PredicateRegistry.ByName("Character.AvoidTag"),
				PredicateRegistry.ByName("AnimCharacter.AvoidTag"),
				PredicateRegistry.ByName("Block.TaggedBump"),
				PredicateRegistry.ByName("Block.TaggedBumpModel"),
				PredicateRegistry.ByName("Block.TaggedBumpChunk"),
				PredicateRegistry.ByName("Block.TeleportToTag"),
				PredicateRegistry.ByName("Wheel.TurnTowardsTag"),
				PredicateRegistry.ByName("Wheel.DriveTowardsTag"),
				PredicateRegistry.ByName("Wheel.DriveTowardsTagRaw"),
				PredicateRegistry.ByName("Wheel.IsWheelTowardsTag"),
				PredicateRegistry.ByName("BulkyWheel.TurnTowardsTag"),
				PredicateRegistry.ByName("BulkyWheel.DriveTowardsTag"),
				PredicateRegistry.ByName("BulkyWheel.DriveTowardsTagRaw"),
				PredicateRegistry.ByName("BulkyWheel.IsWheelTowardsTag"),
				PredicateRegistry.ByName("GoldenWheel.TurnTowardsTag"),
				PredicateRegistry.ByName("GoldenWheel.DriveTowardsTag"),
				PredicateRegistry.ByName("GoldenWheel.DriveTowardsTagRaw"),
				PredicateRegistry.ByName("GoldenWheel.IsWheelTowardsTag"),
				PredicateRegistry.ByName("SpokedWheel.TurnTowardsTag"),
				PredicateRegistry.ByName("SpokedWheel.DriveTowardsTag"),
				PredicateRegistry.ByName("SpokedWheel.DriveTowardsTagRaw"),
				PredicateRegistry.ByName("SpokedWheel.IsWheelTowardsTag"),
				PredicateRegistry.ByName("RadarUI.TrackTag"),
				PredicateRegistry.ByName("AntiGravity.TurnTowardsTagChunk"),
				PredicateRegistry.ByName("AntiGravityColumn.TurnTowardsTagChunk"),
				PredicateRegistry.ByName("WiserWing.TurnTowardsTagChunk"),
				PredicateRegistry.ByName("BirdWing.TurnTowardsTagChunk"),
				PredicateRegistry.ByName("BatWing.TurnTowardsTagChunk"),
				PredicateRegistry.ByName("BatWingBackpack.TurnTowardsTagChunk"),
				PredicateRegistry.ByName("FairyWings.TurnTowardsTagChunk"),
				PredicateRegistry.ByName("MLPWings.TurnTowardsTagChunk"),
				PredicateRegistry.ByName("Cape.TurnTowardsTagChunk"),
				PredicateRegistry.ByName("Jetpack.TurnTowardsTagChunk"),
				PredicateRegistry.ByName("FlightYoke.TurnTowardsTagChunk"),
				PredicateRegistry.ByName("Block.IsTreasureForTag"),
				PredicateRegistry.ByName("Block.IsPickupForTag"),
				PredicateRegistry.ByName("Block.OnCollectByTag"),
				PredicateRegistry.ByName("Missile.TargetTag"),
				PredicateRegistry.ByName("MissileControl.TargetTag"),
				PredicateRegistry.ByName("Block.TagProximityCheck"),
				PredicateRegistry.ByName("SteeringWheel.DriveThroughTag"),
				PredicateRegistry.ByName("SteeringWheel.AvoidTag"),
				PredicateRegistry.ByName("Block.CameraFollowLookTowardTag"),
				PredicateRegistry.ByName("Magnet.InfluenceTag"),
				PredicateRegistry.ByName("Block.ExplodeTag"),
				PredicateRegistry.ByName("TeleportVolume.TeleportTag")
			});
		}
		return taggedPredicates;
	}

	private static void AddUniqueBlocks(string s1, string s2)
	{
		if (!uniqueBlockMap.TryGetValue(s1, out var value))
		{
			value = new HashSet<string>();
			uniqueBlockMap.Add(s1, value);
		}
		value.Add(s2);
	}

	private static void AddUniqueBlockNames(params string[] blockNames)
	{
		for (int i = 0; i < blockNames.Length; i++)
		{
			string text = blockNames[i];
			AddUniqueBlocks(text, text);
			for (int j = i + 1; j < blockNames.Length; j++)
			{
				string text2 = blockNames[j];
				AddUniqueBlocks(text, text2);
				AddUniqueBlocks(text2, text);
			}
		}
	}

	public static Dictionary<string, HashSet<string>> GetUniqueBlockMap()
	{
		if (uniqueBlockMap == null)
		{
			uniqueBlockMap = new Dictionary<string, HashSet<string>>();
			AddUniqueBlockNames("UI Counter I");
			AddUniqueBlockNames("UI Counter II");
			AddUniqueBlockNames("UI Timer I");
			AddUniqueBlockNames("UI Timer II");
			AddUniqueBlockNames("UI Gauge I");
			AddUniqueBlockNames("UI Gauge II");
			AddUniqueBlockNames("UI Object Counter I");
			AddUniqueBlockNames("UI Radar I");
			AddUniqueBlockNames("Master");
			AddUniqueBlockNames("Highscore I");
			AddUniqueBlockNames("Jukebox");
			AddUniqueBlockNames("Missile Control");
		}
		return uniqueBlockMap;
	}

	private static bool UpdateDisabledBlockTypes(string blockType, HashSet<string> disabledTypes)
	{
		if (disabledTypes.Contains(blockType))
		{
			return false;
		}
		Dictionary<string, HashSet<string>> dictionary = GetUniqueBlockMap();
		if (dictionary.TryGetValue(blockType, out var value))
		{
			foreach (string item in value)
			{
				disabledTypes.Add(item);
			}
			return false;
		}
		return true;
	}

	private static HashSet<GAF> GetUniqueGafs()
	{
		if (uniqueGafs == null)
		{
			uniqueGafs = new HashSet<GAF>();
			for (int i = 0; i < 8; i++)
			{
				uniqueGafs.Add(new GAF("Block.SetSpawnpoint", i));
			}
		}
		return uniqueGafs;
	}

	private static void UpdateUsedUniqueGafs(List<List<Tile>> tiles, HashSet<GAF> usedUniqueGafs)
	{
		HashSet<GAF> hashSet = GetUniqueGafs();
		foreach (List<Tile> tile in tiles)
		{
			foreach (Tile item in tile)
			{
				if (hashSet.Contains(item.gaf))
				{
					usedUniqueGafs.Add(item.gaf);
				}
			}
		}
	}

	private static PredicateSet GetMagnetPredicates()
	{
		if (magnetPredicates == null)
		{
			magnetPredicates = new PredicateSet(new HashSet<Predicate>
			{
				Block.predicatePulledByMagnet,
				Block.predicatePulledByMagnetModel,
				Block.predicatePushedByMagnet,
				Block.predicatePushedByMagnetModel
			});
		}
		return magnetPredicates;
	}

	private static PredicateSet GetTaggedHandAttachmentPreds()
	{
		if (taggedHandAttachmentPreds == null)
		{
			taggedHandAttachmentPreds = new PredicateSet();
			taggedHandAttachmentPreds.Add(PredicateRegistry.ByName("Block.HitByTaggedBlocksterHandAttachment"));
			taggedHandAttachmentPreds.Add(PredicateRegistry.ByName("Block.ModelHitByTaggedBlocksterHandAttachment"));
		}
		return taggedHandAttachmentPreds;
	}

	private static PredicateSet GetTaggedArrowPreds()
	{
		if (taggedArrowPreds == null)
		{
			taggedArrowPreds = new PredicateSet();
			taggedArrowPreds.Add(PredicateRegistry.ByName("Block.HitByTaggedArrow"));
			taggedArrowPreds.Add(PredicateRegistry.ByName("Block.ModelHitByTaggedArrow"));
		}
		return taggedArrowPreds;
	}

	private static PredicateSet GetTaggedLaserPreds()
	{
		if (taggedLaserPreds == null)
		{
			taggedLaserPreds = new PredicateSet();
			taggedLaserPreds.Add(PredicateRegistry.ByName("Laser.TaggedHitByBeam"));
			taggedLaserPreds.Add(PredicateRegistry.ByName("Block.HitByTaggedLaserModel"));
			taggedLaserPreds.Add(PredicateRegistry.ByName("Block.HitByTaggedLaserChunk"));
		}
		return taggedLaserPreds;
	}

	private static PredicateSet GetTaggedProjectilePreds()
	{
		if (taggedProjectilePreds == null)
		{
			taggedProjectilePreds = new PredicateSet();
			taggedProjectilePreds.Add(PredicateRegistry.ByName("Block.TaggedHitByProjectile"));
			taggedProjectilePreds.Add(PredicateRegistry.ByName("Block.HitByTaggedProjectileModel"));
			taggedProjectilePreds.Add(PredicateRegistry.ByName("Block.HitByTaggedProjectileChunk"));
		}
		return taggedProjectilePreds;
	}

	private static PredicateSet GetTaggedWaterPreds()
	{
		if (taggedWaterPreds == null)
		{
			taggedWaterPreds = new PredicateSet();
			taggedWaterPreds.Add(PredicateRegistry.ByName("Block.WithinTaggedWater"));
			taggedWaterPreds.Add(PredicateRegistry.ByName("Block.WithinTaggedWaterChunk"));
			taggedWaterPreds.Add(PredicateRegistry.ByName("Block.WithinTaggedWaterModel"));
		}
		return taggedWaterPreds;
	}

	public static void SetupBuildPanel()
	{
		if (unlockedGAFs == null)
		{
			BWLog.Error("Inventory not loaded!");
		}
		else
		{
			buildPanel.CreateInventoryTiles(new HashSet<GAF>(unlockedGAFs));
		}
	}

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
		case TabBarTabId.Actions:
			UpdateActionTiles(tilesInTab, hiddenGafs, neverHideGafs);
			break;
		case TabBarTabId.Sounds:
			UpdateSFXTiles(tilesInTab, hiddenGafs, neverHideGafs);
			break;
		default:
			foreach (Tile item in tilesInTab)
			{
				if (item.gaf.Predicate != Block.predicateThen && (item.gaf.Predicate != BlockAnimatedCharacter.predicateReplaceLimb || (CharacterEditor.Instance.InEditMode() && CharacterEditor.Instance.CharacterBlock().characterType != CharacterType.Avatar)))
				{
					item.Show(show: true);
					item.Enable(TileEnabled(item));
				}
			}
			break;
		case TabBarTabId.Blocks:
		case TabBarTabId.ActionBlocks:
			UpdateCreateBlockTiles(tilesInTab, hiddenGafs, neverHideGafs);
			break;
		}
		for (int i = 0; i < tilesInTab.Count; i++)
		{
			Tile tile = tilesInTab[i];
			if (!tile.visibleInPanel)
			{
				tile.Destroy();
			}
		}
	}

	public static void UpdateCreateBlockTiles(List<Tile> tiles, HashSet<GAF> hiddenGafs, HashSet<GAF> neverHideGafs)
	{
		HashSet<string> hashSet = new HashSet<string>();
		List<Block> list = BWSceneManager.AllBlocks();
		for (int i = 0; i < list.Count; i++)
		{
			Block block = list[i];
			string blockType = block.BlockType();
			UpdateDisabledBlockTypes(blockType, hashSet);
		}
		foreach (Tile tile in tiles)
		{
			GAF gaf = tile.gaf;
			if (neverHideGafs.Contains(gaf))
			{
				continue;
			}
			if (hiddenGafs.Contains(gaf))
			{
				tile.Show(show: false);
				continue;
			}
			if (gaf.Predicate == Block.predicateThen)
			{
				tile.Show(show: false);
				continue;
			}
			tile.Show(show: true);
			bool flag = TileEnabled(tile);
			if (gaf.Predicate == Block.predicateCreate && gaf.Args.Length != 0 && hashSet.Contains((string)gaf.Args[0]))
			{
				flag = false;
			}
			tile.Enable(flag);
		}
	}

	public static void UpdateSFXTiles(List<Tile> sfxTiles, HashSet<GAF> hiddenGafs, HashSet<GAF> neverHideGafs)
	{
		Block selectedScriptBlock = GetSelectedScriptBlock();
		bool flag = selectedScriptBlock != null;
		foreach (Tile sfxTile in sfxTiles)
		{
			if (flag)
			{
				bool show = neverHideGafs.Contains(sfxTile.gaf) || !hiddenGafs.Contains(sfxTile.gaf);
				sfxTile.Show(show);
				sfxTile.Enable(enabled: true);
			}
			else
			{
				sfxTile.Show(show: false);
			}
		}
	}

	public static void UpdateActionTiles(List<Tile> actionTiles, HashSet<GAF> hiddenGafs, HashSet<GAF> neverHideGafs)
	{
		if (actionTiles == null)
		{
			return;
		}
		Block selectedScriptBlock = GetSelectedScriptBlock();
		ComputeNumSendAndTagTilesInUse();
		PredicateSet predicateSet = new PredicateSet();
		PredicateSet predicateSet2 = GetMagnetPredicates();
		HashSet<GAF> usedUniqueGafs = new HashSet<GAF>();
		bool flag = false;
		HashSet<int> hashSet = new HashSet<int>();
		HashSet<int> hashSet2 = new HashSet<int>();
		HashSet<int> hashSet3 = new HashSet<int>();
		HashSet<int> hashSet4 = new HashSet<int>();
		HashSet<Predicate> hashSet5 = GetTaggedPredicates();
		Predicate predicateTag = Block.predicateTag;
		PredicateSet predicateSet3 = GetTaggedArrowPreds();
		PredicateSet predicateSet4 = GetTaggedHandAttachmentPreds();
		PredicateSet predicateSet5 = GetTaggedLaserPreds();
		PredicateSet predicateSet6 = GetTaggedProjectilePreds();
		PredicateSet predicateSet7 = GetTaggedWaterPreds();
		List<Block> list = BWSceneManager.AllBlocks();
		for (int i = 0; i < list.Count; i++)
		{
			Block block = list[i];
			string text = block.BlockType();
			UpdateUsedUniqueGafs(block.tiles, usedUniqueGafs);
			if (block is BlockObjectCounterUI blockObjectCounterUI)
			{
				hashSet.Add(blockObjectCounterUI.index);
			}
			if (block is BlockCounterUI blockCounterUI)
			{
				hashSet2.Add(blockCounterUI.index);
			}
			if (block is BlockTimerUI blockTimerUI)
			{
				hashSet3.Add(blockTimerUI.index);
			}
			if (block is BlockGaugeUI blockGaugeUI)
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
			if (hashSet.Contains(j))
			{
				continue;
			}
			hiddenGafs.Add(new GAF("Block.SetAsTreasureBlockIcon", j));
			hiddenGafs.Add(new GAF("Block.SetAsTreasureTextureIcon", j));
			hiddenGafs.Add(new GAF("ObjectCounterUI.Increment", 1, j));
			hiddenGafs.Add(new GAF("ObjectCounterUI.Decrement", 1, j));
			hiddenGafs.Add(new GAF("ObjectCounterUI.Equals", 0, j));
			hiddenGafs.Add(new GAF("ObjectCounterUI.EqualsMax", j));
			hiddenGafs.Add(new GAF("ObjectCounterUI.ValueCondition", 5, 0, j));
			hiddenGafs.Add(new GAF("ObjectCounterUI.ValueCondition", 5, 1, j));
			hiddenGafs.Add(new GAF("ObjectCounterUI.ValueCondition", 5, 2, j));
			if (j > 0)
			{
				hiddenGafs.Add(new GAF("Block.IsTreasure", j));
				for (int k = 0; k < 9; k++)
				{
					hiddenGafs.Add(new GAF("Block.IsTreasureForTag", k.ToString(), j));
				}
			}
		}
		for (int l = 0; l < 2; l++)
		{
			if (!hashSet2.Contains(l))
			{
				hiddenGafs.Add(new GAF("Block.SetAsCounterUIBlockIcon", l));
				hiddenGafs.Add(new GAF("Block.SetAsCounterUITextureIcon", l));
				hiddenGafs.Add(new GAF("CounterUI.Increment", 1, l));
				hiddenGafs.Add(new GAF("CounterUI.Increment", -1, l));
				hiddenGafs.Add(new GAF("CounterUI.Equals", 0, l));
				hiddenGafs.Add(new GAF("CounterUI.EqualsMin", l));
				hiddenGafs.Add(new GAF("CounterUI.EqualsMax", l));
				hiddenGafs.Add(new GAF("CounterUI.AnimateScore", 0, l));
				hiddenGafs.Add(new GAF("CounterUI.AnimateScore", 1, l));
				hiddenGafs.Add(new GAF("CounterUI.ScoreMultiplier", 2, 1, l));
				hiddenGafs.Add(new GAF("CounterUI.GlobalScoreMultiplier", 2, 1, l));
				hiddenGafs.Add(new GAF("CounterUI.ValueCondition", 5, 0, l));
				hiddenGafs.Add(new GAF("CounterUI.ValueCondition", 5, 1, l));
				hiddenGafs.Add(new GAF("CounterUI.ValueCondition", 5, 2, l));
				hiddenGafs.Add(new GAF("CounterUI.Randomize", 0, 10, l));
				if (l == 0)
				{
					hiddenGafs.Add(new GAF("CounterUI.Equals", 0));
				}
			}
		}
		for (int m = 0; m < 2; m++)
		{
			if (!hashSet4.Contains(m))
			{
				hiddenGafs.Add(new GAF("Block.SetAsGaugeUIBlockIcon", m));
				hiddenGafs.Add(new GAF("Block.SetAsGaugeUITextureIcon", m));
				hiddenGafs.Add(new GAF("GaugeUI.Increment", 1, m));
				hiddenGafs.Add(new GAF("GaugeUI.Increment", -1, m));
				hiddenGafs.Add(new GAF("GaugeUI.Equals", 0, m));
				hiddenGafs.Add(new GAF("GaugeUI.EqualsMax", m));
				hiddenGafs.Add(new GAF("GaugeUI.Fraction", 1f, m));
				hiddenGafs.Add(new GAF("GaugeUI.ValueCondition", 5, 0, m));
				hiddenGafs.Add(new GAF("GaugeUI.ValueCondition", 5, 1, m));
				hiddenGafs.Add(new GAF("GaugeUI.ValueCondition", 5, 2, m));
			}
		}
		for (int n = 0; n < 2; n++)
		{
			if (!hashSet3.Contains(n))
			{
				hiddenGafs.Add(new GAF("Block.SetAsTimerUIBlockIcon", n));
				hiddenGafs.Add(new GAF("Block.SetAsTimerUITextureIcon", n));
				hiddenGafs.Add(new GAF("TimerUI.Equals", 0f, n));
				hiddenGafs.Add(new GAF("TimerUI.Equals", 0f, n));
				hiddenGafs.Add(new GAF("TimerUI.EqualsMax", n));
				hiddenGafs.Add(new GAF("TimerUI.Increment", 1f, n));
				hiddenGafs.Add(new GAF("TimerUI.Increment", -1f, n));
				hiddenGafs.Add(new GAF("TimerUI.Start", 1, n));
				hiddenGafs.Add(new GAF("TimerUI.Start", -1, n));
				hiddenGafs.Add(new GAF("TimerUI.Pause", n));
				hiddenGafs.Add(new GAF("TimerUI.Wait", 1f, n));
				hiddenGafs.Add(new GAF("TimerUI.PauseUI", 1f, n));
				hiddenGafs.Add(new GAF("TimerUI.ValueCondition", 5f, 0, n));
				hiddenGafs.Add(new GAF("TimerUI.ValueCondition", 5f, 1, n));
				hiddenGafs.Add(new GAF("TimerUI.ValueCondition", 5f, 2, n));
			}
		}
		if (worldOcean == null)
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
		foreach (int allNonPrimitiveBlockId in TileToggleChain.GetAllNonPrimitiveBlockIds())
		{
			BlockItem blockItem = BlockItem.FindByID(allNonPrimitiveBlockId);
			GAF item = new GAF(blockItem);
			if (hiddenGafs.Contains(item))
			{
				hashSet6.Add(item);
			}
			hiddenGafs.Add(item);
		}
		foreach (Tile actionTile in actionTiles)
		{
			GAF gaf = actionTile.gaf;
			if (!neverHideGafs.Contains(gaf))
			{
				if (predicateSet.Contains(gaf.Predicate))
				{
					actionTile.Show(show: false);
					continue;
				}
				if (hiddenGafs.Contains(gaf))
				{
					actionTile.Show(show: false);
					continue;
				}
				if (!publicProvidedGafs.Contains(gaf) && (selectedScriptBlock == null || !gaf.Predicate.CompatibleWith(selectedScriptBlock)))
				{
					actionTile.Show(show: false);
					continue;
				}
				if (selectedScriptBlock != null)
				{
					if ((gaf.Predicate == Block.predicateSendSignal || gaf.Predicate == Block.predicateSendSignalModel) && (int)gaf.Args[0] > numSendTilesInUse)
					{
						actionTile.Show(show: false);
						continue;
					}
					if (gaf.Predicate == Block.predicateSendCustomSignal || gaf.Predicate == Block.predicateSendCustomSignalModel || gaf.Predicate == Block.predicateVariableCustomInt || gaf.Predicate == Block.predicateBlockVariableInt)
					{
						actionTile.Show(show: false);
						continue;
					}
					bool flag2 = gaf.Predicate == predicateTag;
					if (flag2 || hashSet5.Contains(gaf.Predicate))
					{
						int num = ((!flag2) ? (numTagTilesInUse - 1) : numTagTilesInUse);
						string text2 = (string)gaf.Args[0];
						if (int.TryParse(text2, out var result))
						{
							if (result > num)
							{
								actionTile.Show(show: false);
								continue;
							}
						}
						else if (!flag2 && !everPresentTagsInUse.Contains(text2))
						{
							actionTile.Show(show: false);
							continue;
						}
					}
					if (predicateSet3.Contains(gaf.Predicate) && gaf.Args.Length != 0 && !arrowTags.Contains((string)gaf.Args[0]))
					{
						actionTile.Show(show: false);
						continue;
					}
					if (predicateSet4.Contains(gaf.Predicate) && gaf.Args.Length != 0 && !handAttachmentTags.Contains((string)gaf.Args[0]))
					{
						actionTile.Show(show: false);
						continue;
					}
					if (predicateSet5.Contains(gaf.Predicate) && gaf.Args.Length != 0 && !laserTags.Contains((string)gaf.Args[0]))
					{
						actionTile.Show(show: false);
						continue;
					}
					if (predicateSet6.Contains(gaf.Predicate) && gaf.Args.Length != 0 && !projectileTags.Contains((string)gaf.Args[0]))
					{
						actionTile.Show(show: false);
						continue;
					}
					if (predicateSet7.Contains(gaf.Predicate) && gaf.Args.Length != 0 && !waterTags.Contains((string)gaf.Args[0]))
					{
						actionTile.Show(show: false);
						continue;
					}
					if (!flag && predicateSet2.Contains(gaf.Predicate))
					{
						actionTile.Show(show: false);
						continue;
					}
				}
				if (gaf.Predicate == Block.predicateThen)
				{
					actionTile.Show(show: false);
					continue;
				}
				if (selectedScriptBlock != null && !selectedScriptBlock.SupportsGaf(gaf))
				{
					actionTile.Show(show: false);
					continue;
				}
				if (gaf.Predicate == BlockMaster.predicatePaintSkyTo && !unlockedPaints.Contains((string)gaf.Args[0]))
				{
					actionTile.Show(show: false);
					continue;
				}
			}
			actionTile.Show(show: true);
			actionTile.Enable(TileEnabled(actionTile));
		}
	}

	public static void UpdateTiles()
	{
		buildPanel.Layout();
		SetupClipboard();
	}

	public static string NextAvailableBlockVariableName(Block b)
	{
		int num = 0;
		if (blockIntVariables.ContainsKey(b))
		{
			num = blockIntVariables[b].Count;
		}
		return "b" + num;
	}

	private static void ComputeNumSendAndTagTilesInUse()
	{
		numSendTilesInUse = 0;
		numTagTilesInUse = 0;
		everPresentTagsInUse.Clear();
		arrowTags.Clear();
		handAttachmentTags.Clear();
		laserTags.Clear();
		projectileTags.Clear();
		waterTags.Clear();
		customSignals.Clear();
		customSignals.Add("*");
		customIntVariables.Clear();
		blockIntVariables.Clear();
		Predicate predicateTag = Block.predicateTag;
		Predicate predicate = PredicateRegistry.ByName("Block.HitByArrow");
		Predicate predicate2 = PredicateRegistry.ByName("Laser.HitByBeam");
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
						numSendTilesInUse = Mathf.Max(a, numSendTilesInUse);
					}
					else if (predicate3 == predicateTag)
					{
						string text = (string)tile.gaf.Args[0];
						if (int.TryParse(text, out var result))
						{
							numTagTilesInUse = Mathf.Max(result + 1, numTagTilesInUse);
						}
						else
						{
							everPresentTagsInUse.Add(text);
						}
						if (block is BlockAbstractLaser blockAbstractLaser)
						{
							if (blockAbstractLaser.CanFireLaser())
							{
								laserTags.Add((string)tile.gaf.Args[0]);
							}
							if (blockAbstractLaser.CanFireProjectiles())
							{
								projectileTags.Add((string)tile.gaf.Args[0]);
							}
						}
						else if (block is BlockAbstractBow)
						{
							arrowTags.Add((string)tile.gaf.Args[0]);
						}
						else if (block is BlockWaterCube)
						{
							waterTags.Add((string)tile.gaf.Args[0]);
						}
						else if (BlockAnimatedCharacter.FindPropHolder(block) != null)
						{
							handAttachmentTags.Add((string)tile.gaf.Args[0]);
						}
					}
					else if (predicate3 == predicate2 && tile.gaf.Args.Length != 0)
					{
						laserTags.Add((string)tile.gaf.Args[0]);
					}
					else if (predicate3 == predicate && tile.gaf.Args.Length != 0)
					{
						arrowTags.Add((string)tile.gaf.Args[0]);
					}
					else if (predicate3 == Block.predicateSendCustomSignal || predicate3 == Block.predicateSendCustomSignalModel)
					{
						string text2 = (string)tile.gaf.Args[0];
						if (text2 != "*")
						{
							customSignals.Add(text2);
						}
					}
					else if (predicate3 == Block.predicateVariableCustomInt)
					{
						string text3 = (string)tile.gaf.Args[0];
						if (text3 != "*" && !customIntVariables.ContainsKey(text3))
						{
							customIntVariables.Add(text3, (int)tile.gaf.Args[1]);
						}
					}
					else
					{
						if (predicate3 != Block.predicateBlockVariableInt)
						{
							continue;
						}
						string text4 = (string)tile.gaf.Args[0];
						if (text4 != "*")
						{
							if (!blockIntVariables.ContainsKey(block))
							{
								blockIntVariables[block] = new Dictionary<string, int>();
								blockIntVariables[block].Add(text4, (int)tile.gaf.Args[1]);
							}
							else if (!blockIntVariables[block].ContainsKey(text4))
							{
								blockIntVariables[block].Add(text4, (int)tile.gaf.Args[1]);
							}
						}
					}
				}
			}
		}
	}

	private void WhiteBackground()
	{
		if (whiteBackground == 0)
		{
			if (WorldSession.current != null && WorldSession.current.worldTitle == "Logo")
			{
				blocksworldCamera.SetCameraPosition(new Vector3(-21.5f, 26f, 34f));
				cameraTransform.eulerAngles = new Vector3(45f, 180f, 0f);
				GameObject gameObject = GameObject.Find("Chunk 9");
				gameObject.transform.position = new Vector3(-26f, 2f, -1.25f);
				gameObject.transform.rotation = Quaternion.Euler(0f, 40f, 0f);
				GameObject gameObject2 = GameObject.Find("Chunk 10");
				gameObject2.transform.Translate(0.5f, 0f, 1f);
				gameObject2.transform.Rotate(0f, 12.5f, 0f);
				BlockLegs blockLegs = BWSceneManager.FindBlock(GameObject.Find("CreateLegs1x1x1 91")) as BlockLegs;
				blockLegs.feet[0].go.transform.Translate(0.5f, 0f, 1f);
				blockLegs.feet[0].go.transform.Rotate(0f, 22.5f, 0f);
				blockLegs.feet[1].go.transform.Translate(0.5f, 0f, 1.1f);
				blockLegs.feet[1].go.transform.Rotate(0f, 12.5f, 0f);
				directionalLight.transform.rotation = Quaternion.Euler(65f, 180f, 0f);
				StatePlayUpdate();
			}
			if (WorldSession.current != null && WorldSession.current.worldTitle == "Logo Compact")
			{
				blocksworldCamera.SetCameraPosition(new Vector3(-32f, 17f, 14f));
				cameraTransform.eulerAngles = new Vector3(45f, 180f, 0f);
				GameObject gameObject3 = GameObject.Find("Chunk 6");
				gameObject3.transform.position = new Vector3(-24f, 2f, -1.25f);
				gameObject3.transform.rotation = Quaternion.Euler(0f, 30f, 0f);
				GameObject gameObject4 = GameObject.Find("Chunk 3");
				gameObject4.transform.Translate(-1.25f, 0f, 2.5f);
				gameObject4.transform.Rotate(0f, -12.5f, 0f);
				BlockLegs blockLegs2 = BWSceneManager.FindBlock(GameObject.Find("CreateLegs1x1x1 38")) as BlockLegs;
				blockLegs2.feet[0].go.transform.Translate(-1.25f, 0f, 2.6f);
				blockLegs2.feet[0].go.transform.Rotate(0f, -12.5f, 0f);
				blockLegs2.feet[1].go.transform.Translate(-1.25f, 0f, 2.5f);
				blockLegs2.feet[1].go.transform.Rotate(0f, -22.5f, 0f);
				directionalLight.transform.rotation = Quaternion.Euler(65f, 180f, 0f);
				StatePlayUpdate();
			}
			mainCamera.backgroundColor = new Color(0.23046875f, 0.55859375f, 0.83203125f, 0f);
			foreach (KeyValuePair<string, Material> item in Materials.materialCache)
			{
				item.Value.SetFloat("_FogStart", 2000f);
				item.Value.SetFloat("_FogEnd", 2000f);
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
			whiteBackground = 1;
			return;
		}
		Color backgroundColor = new Color(0.38039216f, 63f / 85f, 1f);
		mainCamera.backgroundColor = backgroundColor;
		foreach (KeyValuePair<string, Material> item2 in Materials.materialCache)
		{
			item2.Value.SetFloat("_FogStart", fogStart * fogMultiplier);
			item2.Value.SetFloat("_FogEnd", fogEnd * fogMultiplier);
		}
		List<Block> list2 = BWSceneManager.AllBlocks();
		for (int j = 0; j < list2.Count; j++)
		{
			Block block2 = list2[j];
			bool flag = true;
			if (block2 is BlockPosition && CurrentState == State.Play)
			{
				flag = false;
			}
			block2.go.GetComponent<Renderer>().enabled = flag;
			if (block2.goShadow != null)
			{
				block2.goShadow.GetComponent<Renderer>().material.SetFloat("_FogStart", fogStart * fogMultiplier);
				block2.goShadow.GetComponent<Renderer>().material.SetFloat("_FogEnd", fogEnd * fogMultiplier);
			}
		}
		if (!renderingShadows)
		{
			GameObject gameObject5 = Resources.Load("Blocks/Shadow") as GameObject;
			gameObject5.GetComponent<Renderer>().sharedMaterial.SetFloat("_FogStart", fogStart * fogMultiplier);
			gameObject5.GetComponent<Renderer>().sharedMaterial.SetFloat("_FogEnd", fogEnd * fogMultiplier);
		}
		whiteBackground = 0;
	}

	public void SetFogMultiplier(float m)
	{
		fogMultiplier = m;
		SetFog(fogStart, fogEnd);
	}

	public void SetFog(float start, float end)
	{
		fogStart = start;
		fogEnd = end;
		if (!renderingShadows)
		{
			start *= fogMultiplier;
			end *= fogMultiplier;
			foreach (KeyValuePair<string, Material> item in Materials.materialCache)
			{
				if (item.Value != null)
				{
					item.Value.SetFloat("_FogStart", start);
					item.Value.SetFloat("_FogEnd", end);
				}
				else
				{
					BWLog.Info(item.Key + " material was destroyed");
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
		weather.FogChanged();
	}

	public static void UpdateFogColor(Color newFogColor)
	{
		fogColor = newFogColor;
		if (!renderingShadows)
		{
			foreach (KeyValuePair<string, Material> item in Materials.materialCache)
			{
				if (item.Value != null)
				{
					item.Value.SetColor("_FogColor", fogColor);
				}
			}
			List<BlockTerrain> list = BWSceneManager.AllTerrainBlocks();
			for (int i = 0; i < list.Count; i++)
			{
				list[i].UpdateFogColor(fogColor);
			}
			Material sharedMaterial = worldSky.renderer.sharedMaterial;
			sharedMaterial.SetColor("_FogColor", fogColor);
			Block.prefabShadow.GetComponent<Renderer>().sharedMaterial.SetColor("_FogColor", fogColor);
		}
		weather.FogChanged();
	}

	public static void UpdateSunLight(Color tint, Color emissiveColor, float lightIntensity, float sunIntensity)
	{
		if (renderingSkybox && overheadLight != null)
		{
			overheadLight.color = 0.5f * (Color.white + tint) + emissiveColor;
			_lightIntensityBasic = lightIntensity;
			UpdateSunIntensity(sunIntensity);
		}
	}

	public static void UpdateSunIntensity(float sunIntensity)
	{
		if (renderingSkybox && overheadLight != null)
		{
			_lightIntensityMultiplier = sunIntensity;
			float num = _lightIntensityBasic * _lightIntensityMultiplier;
			overheadLight.intensity = num;
			overheadLight.bounceIntensity = num * 0.35f / 0.9f;
			overheadLight.shadowStrength = Mathf.Clamp(num * num, 0.3f, 1f);
			DynamicGI.UpdateEnvironment();
		}
	}

	public static void ResetSkyAndFogSettings()
	{
		if (worldSky != null)
		{
			worldSky.SetSkyBoxRotation(0f, immediate: true);
		}
		UpdateSunIntensity(1f);
		UpdateFogColor(_buildModeFogColor);
		bw.SetFog(_buildModeFogStart, _buildModeFogEnd);
	}

	public static void RenderScreenshot(Texture2D imageTexture, bool includeGuiCamera = false, FilterMode filterMode = FilterMode.Bilinear)
	{
		RenderScreenshot(imageTexture, mainCamera, (!includeGuiCamera) ? null : guiCamera, filterMode);
	}

	public static void RenderScreenshot(Texture2D imageTexture, Camera camera, Camera overlayCamera, FilterMode filterMode = FilterMode.Bilinear)
	{
		int width = imageTexture.width;
		int height = imageTexture.height;
		bool flag = (bool)blocksworldCamera.currentReticle && blocksworldCamera.currentReticle.enabled;
		if (flag)
		{
			blocksworldCamera.currentReticle.enabled = false;
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
		imageTexture.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
		RenderTexture.active = null;
		UnityEngine.Object.Destroy(renderTexture);
		if (flag)
		{
			blocksworldCamera.currentReticle.enabled = true;
		}
	}

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
		Texture2D texture2D = new Texture2D(w, h, TextureFormat.RGB24, mipmap: false);
		RenderScreenshot(texture2D, includeGuiCamera, filterMode);
		texture2D.Apply();
		byte[] bytes = texture2D.EncodeToPNG();
		string text2 = string.Empty;
		if (path == null)
		{
			int num = 100;
			for (int i = 1; i <= num; i++)
			{
				string text3 = i.ToString("D3");
				text2 = "Blocksworld " + text3 + ".png";
				path = Path.Combine(text, text2);
				if (!File.Exists(path))
				{
					break;
				}
				if (i == num)
				{
					OnScreenLog.AddLogItem("Too many screenshots in folder");
				}
			}
		}
		File.WriteAllBytes(path, bytes);
		BWLog.Info("Wrote: " + path);
		UI.Overlay.ShowTimedOnScreenMessage("Wrote: " + text2, 3f);
		UnityEngine.Object.Destroy(texture2D);
		return path;
	}

	public static void SaveMeshToFile(Bunch bunch)
	{
		BWLog.Info("Saving mesh...");
		string text = Application.dataPath + "/..";
		if (Options.ScreenshotDirectory != string.Empty)
		{
			text = ((!Options.ScreenshotDirectory.StartsWith("/")) ? (text + "/" + Options.ScreenshotDirectory) : Options.ScreenshotDirectory);
		}
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		string text2 = ((1 == 0) ? ".stl" : ".obj");
		string text3 = "BWMesh" + text2;
		int num = 100;
		for (int i = 1; i <= num; i++)
		{
			string text4 = i.ToString("D3");
			text3 = text + "/BWMesh " + text4 + text2;
			if (!File.Exists(text3))
			{
				break;
			}
			if (i == num)
			{
				OnScreenLog.AddLogItem("Too many mesh files in folder");
			}
		}
		MeshUtils.Export(text3, bunch);
		BWLog.Info("Wrote: " + text3);
		UI.Overlay.ShowTimedOnScreenMessage("Wrote: " + text3, 3f);
	}

	public void ButtonPlayTapped()
	{
		if (resettingPlay)
		{
			return;
		}
		if (CurrentState == State.Paused)
		{
			Sound.PlayOneShotSound("Button Generic");
			WorldSession.current.Unpause();
			SetBlocksworldState(State.Play);
			WorldSession.current.OnPlay();
			VisualEffect.ResumeVfxs();
			weather.Resume();
		}
		else if (CurrentState == State.FrameCapture)
		{
			Sound.PlayOneShotSound("Button Generic");
			WorldSession.current.ExitScreenCaptureSetup();
			WorldSession.current.OnPlay();
			VisualEffect.ResumeVfxs();
			weather.Resume();
		}
		else if (rewardVisualizationGafs != null)
		{
			BWLog.Info("Not calling fast save since the reward vis gafs was not null");
		}
		else
		{
			if (Tutorial.state != TutorialState.None)
			{
				FastSave();
			}
			else
			{
				BWLog.Info("Not calling fast save since tutorial state was " + Tutorial.state);
			}
			Sound.PlayOneShotSound("Button Play");
			BW.Analytics.SendAnalyticsEvent("world-play");
			Play();
		}
	}

	public void ButtonExitWorldTapped()
	{
		if (rewardVisualizationGafs == null && !resettingPlay && (!BW.isIPad || !IOSInterface.IsStartingRecording()))
		{
			if (CurrentState == State.Play)
			{
				Stop(force: true);
			}
			if (BW.Options.saveOnWorldExit() && CurrentState == State.Build && !f3PressedInCurrentWorld)
			{
				Deselect(silent: true);
				Save();
			}
			OnScreenLog.Clear();
			Sound.PlayOneShotSound("Button Generic");
			Tutorial.ResetState();
			leaderboardData.Reset();
			WorldSession.Quit();
		}
	}

	public void ButtonMenuTapped()
	{
		if (CurrentState == State.Play)
		{
			if (WorldSession.isCommunitySession())
			{
				UI.Dialog.ShowEscapeMenuForCommunityWorld();
			}
			else
			{
				UI.Dialog.ShowEscapeMenuForLocalWorldPlayMode();
			}
		}
		else if (CurrentState == State.Build)
		{
			UI.Dialog.ShowEscapeMenuForLocalWorldBuildMode();
		}
	}

	public void ButtonPauseTapped()
	{
		if (rewardVisualizationGafs == null && !resettingPlay)
		{
			Sound.PlayOneShotSound("Button Generic");
			WorldSession.current.PauseButtonPressed();
		}
	}

	public void ButtonStopTapped()
	{
		if (rewardVisualizationGafs != null || resettingPlay)
		{
			return;
		}
		if (CurrentState == State.FrameCapture)
		{
			Sound.PlayOneShotSound("Button Generic");
			BWSceneManager.ResumeScene();
			SetBlocksworldState(State.Play);
			WorldSession.current.OnPlay();
			return;
		}
		Sound.PlayOneShotSound("Button Stop");
		BW.Analytics.SendAnalyticsEvent("world-stop");
		if (WorldSession.jumpRestoreConfig != null)
		{
			WorldSession.RestoreJumpConfig();
			return;
		}
		Stop();
		SetBlocksworldState(State.Build);
	}

	public void ButtonOptionsTapped()
	{
		Sound.PlayOneShotSound("Button Generic");
		if (!resettingPlay)
		{
			ShowOptionsScreen();
		}
	}

	public void ShowOptionsScreen()
	{
		if (CurrentState != State.Paused)
		{
			WorldSession.current.Pause();
			_showingOptionsWhenPaused = false;
		}
		else
		{
			_showingOptionsWhenPaused = true;
		}
		lockInput = true;
		blocksworldCamera.SetCameraStill(still: true);
		string sessionTitle = WorldSession.current.sessionTitle;
		string sessionUserName = WorldSession.current.sessionUserName;
		string sessionDescription = WorldSession.current.sessionDescription;
		UI.ShowOptionsScreen(sessionTitle, sessionUserName, sessionDescription);
		leaderboardData.PauseLeaderboard(isPaused: true);
	}

	public void HideOptionsScreen()
	{
		leaderboardData.PauseLeaderboard(isPaused: false);
		UI.HideOptionsScreen();
		if (!_showingOptionsWhenPaused)
		{
			WorldSession.current.Unpause();
		}
		_showingOptionsWhenPaused = false;
		lockInput = false;
		blocksworldCamera.SetCameraStill(still: false);
	}

	public void ButtonRestartTapped()
	{
		if (!resettingPlay && rewardVisualizationGafs == null)
		{
			if (CurrentState == State.Paused)
			{
				WorldSession.current.Unpause();
			}
			else if (CurrentState == State.FrameCapture)
			{
				WorldSession.current.ExitScreenCaptureSetup();
			}
			Sound.PlayOneShotSound("Button Rewind");
			BW.Analytics.SendAnalyticsEvent("world-rewind");
			Restart();
		}
	}

	public void ButtonCaptureSetupTapped()
	{
		if (rewardVisualizationGafs == null)
		{
			Sound.PlayOneShotSound("Button Generic");
			WorldSession.current.EnterScreenCaptureSetup();
		}
	}

	public void ButtonCaptureTapped()
	{
		if (resettingPlay || rewardVisualizationGafs != null || capturingScreenshot)
		{
			return;
		}
		if (WorldSession.isProfileBuildSession())
		{
			if (CharacterEditor.Instance.InEditMode())
			{
				CharacterEditor.Instance.Exit();
			}
			Sound.PlayOneShotSound("Step");
			WorldSession.Save();
			Play();
			WorldSession.current.OnCapture();
			fixedUpdateCommands.Add(new DelayedProfilePictureCaptureCommand());
		}
		else if (WorldSession.current.TakeScreenshot())
		{
			Sound.PlayOneShotSound("Step");
			if (WorldSession.isWorldScreenshotSession())
			{
				WorldSession.current.ExitScreenCaptureSetup();
				WorldSession.Quit();
				return;
			}
			capturingScreenshot = true;
			UI.Overlay.ShowTimedOnScreenMessage("Added to Photos!", 1.5f);
			CameraFlashEffect cameraFlashEffect = guiCamera.gameObject.GetComponent<CameraFlashEffect>();
			if (cameraFlashEffect == null)
			{
				cameraFlashEffect = guiCamera.gameObject.AddComponent<CameraFlashEffect>();
			}
			cameraFlashEffect.time = 0f;
			cameraFlashEffect.autoDestroy = true;
		}
		else
		{
			Sound.PlayOneShotSound("Player Error Retro");
			UI.Overlay.ShowTimedOnScreenMessage("Error saving Photo", 1.5f);
		}
	}

	public void ButtonUndoTapped()
	{
		if (!resettingPlay && CurrentState == State.Build && History.CanUndo())
		{
			Sound.PlayOneShotSound("Button Generic");
			History.Undo();
		}
	}

	public void ButtonRedoTapped()
	{
		if (!resettingPlay && CurrentState == State.Build && History.CanRedo())
		{
			Sound.PlayOneShotSound("Button Generic");
			History.Redo();
		}
	}

	public void ButtonLikeToggleTapped()
	{
		Sound.PlayOneShotSound("Button Generic");
		WorldSession.current.ToggleWorldUpvoted();
	}

	public void ButtonVRCameraTapped()
	{
		Sound.PlayOneShotSound("Button Generic");
		SetVRMode(enabled: true);
	}

	public string ExtractProfileWorldAvatarString()
	{
		Block block = BWSceneManager.AllBlocks().Find((Block b) => b.IsProfileCharacter());
		if (block == null)
		{
			return string.Empty;
		}
		return ExtractProfileWorldAvatarString(block);
	}

	public string ExtractProfileWorldAvatarString(Block profileCharacter)
	{
		StringBuilder stringBuilder = new StringBuilder(32768);
		StringWriter writer = new StringWriter(stringBuilder, CultureInfo.InvariantCulture);
		JSONStreamEncoder jSONStreamEncoder = new JSONStreamEncoder(writer);
		List<Block> list = ConnectednessGraph.ConnectedComponent(profileCharacter, 3);
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (list[num] != profileCharacter && IsSelectionLocked(list[num]))
			{
				list.RemoveAt(num);
			}
		}
		Util.AddGroupedTilesToBlockList(list);
		jSONStreamEncoder.BeginObject();
		jSONStreamEncoder.InsertNewline();
		jSONStreamEncoder.WriteKey("avatar");
		jSONStreamEncoder.BeginArray();
		foreach (Block item in list)
		{
			List<List<Tile>> tileRows = item.tiles;
			if (item == profileCharacter)
			{
				tileRows = CloneBlockTiles(item);
				Tile tile = tileRows[0].Find((Tile t) => t.gaf.Predicate == Block.predicateCreate);
				string stringArg = Util.GetStringArg(tile.gaf.Args, 0, string.Empty);
				string text = ProfileBlocksterUtils.GetNonProfileBlockType(stringArg);
				if (string.IsNullOrEmpty(text))
				{
					text = ((!(profileCharacter is BlockAnimatedCharacter)) ? "Character Male" : "Anim Character Male");
				}
				tile.gaf.Args[0] = text;
				List<List<Tile>> list2 = new List<List<Tile>>();
				list2.Add(tileRows[0]);
				list2.Add(new List<Tile>
				{
					new Tile(new GAF(Block.predicateThen))
				});
				tileRows = list2;
			}
			jSONStreamEncoder.InsertNewline();
			jSONStreamEncoder.BeginObject();
			jSONStreamEncoder.WriteKey((!useCompactGafWriteRenamings) ? "tile-rows" : "r");
			WriteTilesAsJSON(jSONStreamEncoder, tileRows, lockBlocks: false, useCompactGafWriteRenamings);
			jSONStreamEncoder.EndObject();
		}
		jSONStreamEncoder.EndArray();
		jSONStreamEncoder.InsertNewline();
		jSONStreamEncoder.WriteKey("connections");
		jSONStreamEncoder.BeginArray();
		foreach (Block item2 in list)
		{
			jSONStreamEncoder.InsertNewline();
			jSONStreamEncoder.BeginArray();
			foreach (Block connection in item2.connections)
			{
				int num2 = list.IndexOf(connection);
				if (num2 >= 0)
				{
					jSONStreamEncoder.WriteNumber(list.IndexOf(connection));
				}
			}
			jSONStreamEncoder.EndArray();
		}
		jSONStreamEncoder.InsertNewline();
		jSONStreamEncoder.EndArray();
		jSONStreamEncoder.InsertNewline();
		jSONStreamEncoder.WriteKey("connectionTypes");
		jSONStreamEncoder.BeginArray();
		foreach (Block item3 in list)
		{
			jSONStreamEncoder.InsertNewline();
			jSONStreamEncoder.BeginArray();
			foreach (int connectionType in item3.connectionTypes)
			{
				jSONStreamEncoder.WriteNumber(connectionType);
			}
			jSONStreamEncoder.EndArray();
		}
		jSONStreamEncoder.InsertNewline();
		jSONStreamEncoder.EndArray();
		jSONStreamEncoder.InsertNewline();
		jSONStreamEncoder.EndObject();
		jSONStreamEncoder.InsertNewline();
		return stringBuilder.ToString();
	}

	public void ButtonCopyTapped()
	{
		if (!copyModelAnimationCommand.Animating())
		{
			List<Block> list = new List<Block>();
			if (selectedBlock != null)
			{
				list.Add(selectedBlock);
			}
			else if (selectedBunch != null)
			{
				list.AddRange(selectedBunch.blocks);
			}
			copyModelAnimationCommand.SetBlocks(list);
			copyModelAnimationCommand.endCommand = GetScaleCommand(delegate(UIQuickSelect q, float s)
			{
				q.SetModelIconScale(s);
			});
			AddFixedUpdateUniqueCommand(copyModelAnimationCommand);
			clipboard.SetModelToSelection();
		}
	}

	public void ButtonSaveModelTapped()
	{
		if (!modelCollection.CanSaveModels)
		{
			UI.Dialog.ShowMaximumModelsDialog();
			return;
		}
		List<List<List<Tile>>> buffer = new List<List<List<Tile>>>();
		CopySelectionToBuffer(buffer);
		bool useHD = hd;
		useHD |= BWStandalone.Instance != null;
		if (ModelCollection.ModelContainsDisallowedTile(buffer))
		{
			BWLog.Error("Attempting to create model with disallowed gaf");
			return;
		}
		Dictionary<GAF, int> gafUsage = Scarcity.GetNormalizedInventoryUse(buffer, WorldType.User);
		Action completion = delegate
		{
			if (!saveModelAnimationCommand.Animating())
			{
				List<Block> list = new List<Block>();
				Bounds bounds = new Bounds(Vector3.zero, Vector3.one);
				if (selectedBlock != null)
				{
					list.Add(selectedBlock);
					bounds = selectedBlock.GetBounds();
				}
				else if (selectedBunch != null)
				{
					list.AddRange(selectedBunch.blocks);
					bounds = selectedBunch.GetBounds();
				}
				saveModelAnimationCommand.SetBlocks(list);
				AddFixedUpdateUniqueCommand(saveModelAnimationCommand);
				modelCollection.SaveToModelCollection(buffer, gafUsage);
				if (bounds.size.y >= 25f)
				{
					WorldSession.platformDelegate.TrackAchievementIncrease("skyscraper", 1);
				}
			}
		};
		Action completionWithUnsellableBlocksWarning = delegate
		{
			HashSet<GAF> hashSet = new HashSet<GAF>();
			foreach (GAF key in gafUsage.Keys)
			{
				BlockItem blockItem = BlockItem.FindByGafPredicateNameAndArguments(key.Predicate.Name, key.Args);
				if (blockItem == null)
				{
					BWLog.Info("no blockItem for gaf: " + key);
				}
				else if (blockItem.IsUnsellable())
				{
					hashSet.Add(key);
				}
			}
			if (hashSet.Count > 0)
			{
				UI.Dialog.ShowUnsellableBlocksInModelWarning(hashSet, completion);
			}
			else
			{
				completion();
			}
		};
		Action completionWithModelExistWarning = delegate
		{
			if (modelCollection.ContainsSimilarModel(buffer))
			{
				UI.Dialog.ShowGenericDialog("\nYou have a model like this...\n\nSave anyway?", "No", "Yes", null, completionWithUnsellableBlocksWarning);
			}
			else
			{
				completionWithUnsellableBlocksWarning();
			}
		};
		Action<string> setNameAction = delegate(string name)
		{
			modelCollection.SetTempName(name);
		};
		Action<Texture2D> callback = delegate(Texture2D tex)
		{
			modelCollection.SetTempSnapshot(tex, useHD);
			modelCollection.SetTempName(string.Empty);
			UI.Dialog.ShowModelSaveDialog(tex, completionWithModelExistWarning, setNameAction);
		};
		ScreenshotUtils.GenerateModelSnapshotTexture(buffer, useHD, callback);
	}

	public bool ModelAnimationInProgress()
	{
		if (!copyModelAnimationCommand.Animating())
		{
			return saveModelAnimationCommand.Animating();
		}
		return true;
	}

	public bool IsPullingObject()
	{
		if (pullObject != null)
		{
			return pullObject.IsActive;
		}
		return false;
	}

	public bool HadObjectTapping()
	{
		return worldSessionHadBlockTap;
	}

	private DelegateCommand GetScaleCommand(Action<UIQuickSelect, float> scaleFunc)
	{
		UIQuickSelect quickSelect = UI.QuickSelect;
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
					a.SetDone(d: true);
				}
				else
				{
					a.SetDone(d: false);
				}
			});
		}
		return null;
	}

	private void ButtonCopyScriptTapped(TileTapGesture gesture, Tile tile)
	{
		if (!copyScriptAnimationCommand.Animating())
		{
			copyScriptAnimationCommand.SetTiles(scriptPanel.tiles);
			AddFixedUpdateUniqueCommand(copyScriptAnimationCommand);
			copyScriptAnimationCommand.endCommand = GetScaleCommand(delegate(UIQuickSelect q, float s)
			{
				q.SetScriptIconScale(s);
			});
			clipboard.SetScriptToSelection();
		}
	}

	private void ButtonClearScriptTapped(TileTapGesture gesture, Tile tile)
	{
		RemoveScriptsFromSelection();
		History.AddStateIfNecessary();
		UpdateTiles();
	}

	private void ButtonPasteScriptTapped(TileTapGesture gesture, Tile tile)
	{
		if (selectedBlock != null)
		{
			PasteScriptFromClipboard(selectedBlock);
			History.AddStateIfNecessary();
			UpdateTiles();
		}
	}

	private void CharacterEditTapped(TileTapGesture gesture, Tile tile)
	{
		if (!CharacterEditor.Instance.InEditMode())
		{
			if (selectedBlock != null && selectedBlock is BlockAnimatedCharacter)
			{
				CharacterEditor.Instance.EditCharacter(selectedBlock as BlockAnimatedCharacter);
			}
			else if (WorldSession.isProfileBuildSession() && WorldSession.current.profileWorldAnimatedBlockster != null)
			{
				CharacterEditor.Instance.EditCharacter(WorldSession.current.profileWorldAnimatedBlockster);
			}
		}
	}

	private void CharacterEditExitTapped(TileTapGesture gesture, Tile tile)
	{
		if (CharacterEditor.Instance.InEditMode())
		{
			CharacterEditor.Instance.Exit();
		}
	}

	public static bool CanSelectBlock(Block block)
	{
		bool enableTerrainSelection = Options.EnableTerrainSelection;
		if ((!block.isTerrain || enableTerrainSelection || (block.SelectableTerrain() && ((selectedBlock == null && selectedBunch == null) || !block.DoubleTapToSelect()))) && (Tutorial.state == TutorialState.None || (!block.IsLocked() && !locked.Contains(block))))
		{
			if (enableTerrainSelection && block.isTerrain && selectedBlock != null)
			{
				return selectedBlock.isTerrain;
			}
			return true;
		}
		return false;
	}

	private bool RaycastMoveBlock(Block block)
	{
		int connectionType = 3;
		bool flag = Options.RaycastMoveBlocksWithoutSelection || (Options.RaycastMoveSingletonBlocksWithoutSelection && block.ConnectionsOfType(connectionType).Count == 0);
		if (flag)
		{
			Bounds bounds = block.go.GetComponent<Collider>().bounds;
			float magnitude = (bounds.center - cameraTransform.position).magnitude;
			if (magnitude > 0.01f)
			{
				return Util.MeanAbs(bounds.size / magnitude) < 0.35f;
			}
		}
		return flag;
	}

	private bool BlockDuplicateBegan(BlockDuplicateGesture gesture, Block block)
	{
		bool flag = block == selectedBlock;
		if (selectedBunch != null)
		{
			flag = false;
			for (int i = 0; i < selectedBunch.blocks.Count; i++)
			{
				Block block2 = selectedBunch.blocks[i];
				flag = flag || block2 == block;
			}
		}
		return flag;
	}

	private static string GetUniquesJSON(List<GAF> uniquesInModel)
	{
		StringBuilder stringBuilder = new StringBuilder();
		StringWriter writer = new StringWriter(stringBuilder, CultureInfo.InvariantCulture);
		JSONStreamEncoder jSONStreamEncoder = new JSONStreamEncoder(writer);
		jSONStreamEncoder.BeginObject();
		jSONStreamEncoder.InsertNewline();
		jSONStreamEncoder.WriteKey("uniques");
		jSONStreamEncoder.BeginArray();
		jSONStreamEncoder.InsertNewline();
		foreach (GAF item in uniquesInModel)
		{
			item.ToJSON(jSONStreamEncoder);
			jSONStreamEncoder.InsertNewline();
		}
		jSONStreamEncoder.EndArray();
		jSONStreamEncoder.InsertNewline();
		jSONStreamEncoder.EndObject();
		jSONStreamEncoder.InsertNewline();
		return stringBuilder.ToString();
	}

	private void BlockDuplicated(BlockDuplicateGesture gesture, Block block, Gestures.Touch touch)
	{
		CopySelectionToBuffer(clipboard.modelDuplicateBuffer);
		TBox.Show(show: false);
		TBoxGesture.skipOneTap = true;
		blockTapGesture.Reset();
		mouseBlock = null;
		if (ModelCollection.ModelContainsDisallowedTile(clipboard.modelDuplicateBuffer))
		{
			BWLog.Error("Attempting to create model with disallowed gaf");
			return;
		}
		if (BW.Options.useScarcity())
		{
			Dictionary<GAF, int> missing = new Dictionary<GAF, int>();
			List<GAF> list = new List<GAF>();
			if (clipboard.AvailablityCountForBlockList(clipboard.modelDuplicateBuffer, missing, list) == 0)
			{
				Sound.PlayOneShotSound("Destroy");
				string text = ((list.Count != 0) ? "A world can only have one of these:" : "Missing items:");
				UI.Dialog.ShowPasteFailInfo(missing, list, "\nCould not duplicate selection.\n\n" + text);
				Deselect();
				return;
			}
		}
		Sound.PlayOneShotSound("Paste Model");
		Scarcity.UpdateInventory();
		List<Block> blocks = PasteModelToFinger(clipboard.modelDuplicateBuffer, touch);
		if (!pasteModelAnimationCommand.Animating())
		{
			pasteModelAnimationCommand.SetBlocks(blocks);
			AddFixedUpdateUniqueCommand(pasteModelAnimationCommand);
		}
	}

	private bool HasEnoughScarcityForSelection()
	{
		if (Scarcity.inventory == null)
		{
			return true;
		}
		List<Block> list = new List<Block>();
		if (selectedBunch != null)
		{
			for (int i = 0; i < selectedBunch.blocks.Count; i++)
			{
				list.Add(selectedBunch.blocks[i]);
			}
		}
		else if (selectedBlock != null)
		{
			list.Add(selectedBlock);
		}
		Dictionary<GAF, int> dictionary = new Dictionary<GAF, int>();
		for (int j = 0; j < list.Count; j++)
		{
			Block block = list[j];
			for (int k = 0; k < block.tiles[0].Count; k++)
			{
				Tile tile = block.tiles[0][k];
				GAF normalizedGaf = Scarcity.GetNormalizedGaf(tile.gaf);
				if (Scarcity.IsRelevantGAF(block.BlockType(), normalizedGaf, firstRow: true))
				{
					if (dictionary.ContainsKey(normalizedGaf))
					{
						dictionary[normalizedGaf]++;
					}
					else
					{
						dictionary.Add(normalizedGaf, 1);
					}
				}
			}
			foreach (List<Tile> tile2 in block.tiles)
			{
				foreach (Tile item in tile2)
				{
					GAF gaf = item.gaf;
					if (gaf.Predicate == Block.predicatePlaySoundDurational)
					{
						if (dictionary.ContainsKey(gaf))
						{
							dictionary[gaf]++;
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
		foreach (GAF key in dictionary.Keys)
		{
			flag &= Scarcity.GetInventoryCount(key) >= dictionary[key];
		}
		return flag;
	}

	public void CopyBlocksToBuffer(List<Block> list, List<List<List<Tile>>> modelBuffer)
	{
		HashSet<Block> hashSet = new HashSet<Block>();
		modelBuffer.Clear();
		for (int i = 0; i < list.Count; i++)
		{
			Block block = list[i];
			if (hashSet.Contains(block))
			{
				continue;
			}
			if (block is BlockGrouped { group: not null } blockGrouped)
			{
				Block[] blocks = blockGrouped.group.GetBlocks();
				foreach (Block block2 in blocks)
				{
					if (!hashSet.Contains(block2))
					{
						hashSet.Add(block2);
						modelBuffer.Add(CloneBlockTiles(block2));
					}
				}
			}
			else
			{
				modelBuffer.Add(CloneBlockTiles(block));
				hashSet.Add(block);
			}
		}
	}

	public void CopySelectionToBuffer(List<List<List<Tile>>> modelBuffer)
	{
		if (selectedBunch != null)
		{
			CopyBlocksToBuffer(selectedBunch.blocks, modelBuffer);
		}
		else if (selectedBlock != null)
		{
			if (selectedBlock is BlockGrouped { group: not null } blockGrouped)
			{
				CopyBlocksToBuffer(blockGrouped.group.GetBlockList(), modelBuffer);
				return;
			}
			CopyBlocksToBuffer(new List<Block> { selectedBlock }, modelBuffer);
		}
	}

	public void CopySelectedScriptToBuffer(List<List<List<Tile>>> scriptBuffer)
	{
		Block selectedScriptBlock = GetSelectedScriptBlock();
		if (selectedScriptBlock != null)
		{
			scriptBuffer.Clear();
			scriptBuffer.Add(CloneBlockTiles(selectedScriptBlock, excludeFirstRow: true));
		}
	}

	public void PasteScriptFromClipboard(Block block)
	{
		if (block.tiles.Count > 2 || (block.tiles.Count > 1 && block.tiles[1].Count > 1))
		{
			UI.Dialog.ShowScriptExistsDialog(block);
		}
		else
		{
			DoPasteScriptFromClipboard(block, replace: true);
		}
	}

	public static void DoPasteScriptFromClipboard(Block block, bool replace = false)
	{
		if (clipboard.scriptCopyPasteBuffer.Count > 0)
		{
			PasteScript(block, clipboard.scriptCopyPasteBuffer[0], replace);
		}
	}

	public void PasteBlocksFromSavedModel(int modelIndex)
	{
		if (modelIndex < modelCollection.models.Count)
		{
			List<List<List<Tile>>> list = modelCollection.models[modelIndex].CreateModel();
			if (ModelCollection.ModelContainsDisallowedTile(list))
			{
				BWLog.Error("Attempting to create model with disallowed gaf");
				return;
			}
			List<Block> blocksToUpdateConnection = PasteBlocks(list, Util.Round(blocksworldCamera.GetTargetPosition()));
			DetermineBlockOffset(blocksToUpdateConnection);
		}
	}

	private static List<List<Tile>> FilterCompatible(List<List<Tile>> script, Block block, HashSet<GAF> incompatible)
	{
		List<List<Tile>> list = new List<List<Tile>>();
		foreach (List<Tile> item2 in script)
		{
			List<Tile> list2 = new List<Tile>();
			foreach (Tile item3 in item2)
			{
				Predicate predicate = item3.gaf.Predicate;
				if (predicate.CompatibleWith(block))
				{
					list2.Add(item3);
					continue;
				}
				bool flag = true;
				HashSet<Predicate> equivalentPredicates = PredicateRegistry.GetEquivalentPredicates(predicate);
				if (equivalentPredicates != null)
				{
					List<Predicate> list3 = PredicateRegistry.ForBlock(block, includeBaseTypes: false);
					foreach (Predicate item4 in list3)
					{
						if (equivalentPredicates.Contains(item4))
						{
							object[] args = PredicateRegistry.ConvertEquivalentPredicateArguments(predicate, item4, item3.gaf.Args);
							GAF gaf = new GAF(item4, args, dummy: true);
							Tile item = new Tile(gaf);
							list2.Add(item);
							flag = false;
							break;
						}
					}
				}
				if (flag)
				{
					incompatible.Add(item3.gaf);
				}
			}
			list.Add(list2);
		}
		return list;
	}

	public static void RemoveScriptsFromSelection()
	{
		if (scriptPanel.IsShowing())
		{
			scriptPanel.SavePositionForNextLayout();
		}
		if (selectedBlock is BlockGrouped blockGrouped)
		{
			Block[] blocks = blockGrouped.group.GetBlocks();
			foreach (Block block in blocks)
			{
				RemoveScriptFrom(block);
				block.AddOrRemoveEmptyScriptLine();
			}
		}
		else if (selectedBunch != null)
		{
			for (int j = 0; j < selectedBunch.blocks.Count; j++)
			{
				RemoveScriptFrom(selectedBunch.blocks[j]);
				selectedBunch.blocks[j].AddOrRemoveEmptyScriptLine();
			}
		}
		else if (selectedBlock != null)
		{
			RemoveScriptFrom(selectedBlock);
			selectedBlock.AddOrRemoveEmptyScriptLine();
		}
		scriptPanel.AssignUnparentedTiles();
		if (scriptPanel.IsShowing())
		{
			scriptPanel.Layout();
		}
	}

	public static void RemoveScriptFrom(Block block)
	{
		for (int i = 1; i < block.tiles.Count; i++)
		{
			List<Tile> list = block.tiles[i];
			foreach (Tile item in list)
			{
				item.Show(show: false);
			}
		}
		block.tiles.RemoveRange(1, block.tiles.Count - 1);
	}

	public static void PasteScript(Block block, List<List<Tile>> script, bool replace = false, bool force = false)
	{
		HashSet<GAF> hashSet = new HashSet<GAF>();
		List<List<Tile>> collection = FilterCompatible(CloneBlockTiles(script), block, hashSet);
		if (!force && hashSet.Count > 0)
		{
			UI.Dialog.ShowPasteScriptIncompatibleDialog(block, script, new List<GAF>(hashSet), replace);
			return;
		}
		if (replace)
		{
			RemoveScriptFrom(block);
		}
		bool flag = scriptPanel.IsShowing();
		if (flag)
		{
			scriptPanel.SavePositionForNextLayout();
			Deselect(silent: true);
		}
		block.tiles.AddRange(collection);
		for (int num = block.tiles.Count - 1; num >= 0; num--)
		{
			if (num < block.tiles.Count - 1)
			{
				List<Tile> list = block.tiles[num];
				if (list.Count <= 1)
				{
					block.tiles.RemoveAt(num);
				}
			}
		}
		if (flag)
		{
			SelectBlock(block, silent: true);
		}
		Scarcity.UpdateInventory(updateTiles: false);
		Scarcity.PaintScarcityBadges();
		Sound.PlayOneShotSound("Paste Script");
	}

	private List<Block> PasteBlocks(List<List<List<Tile>>> blocks, Vector3 targetPos)
	{
		List<Block> list = new List<Block>();
		foreach (List<List<Tile>> block2 in blocks)
		{
			List<List<Tile>> tiles = CloneBlockTiles(block2);
			Block block = Block.NewBlock(tiles);
			BWSceneManager.AddBlock(block);
			list.Add(block);
		}
		Vector3 vector = Util.Round(Util.ComputeCenter(list));
		foreach (Block item in list)
		{
			Vector3 vector2 = item.GetPosition() - vector;
			item.MoveTo(targetPos + vector2);
		}
		return list;
	}

	private void DetermineBlockOffset(List<Block> blocksToUpdateConnection)
	{
		for (int i = 0; i < 10; i++)
		{
			Vector3 vector = default(Vector3);
			bool flag = true;
			foreach (Block item in blocksToUpdateConnection)
			{
				if (item.IsColliding())
				{
					vector = Vector3.up * 2f;
					flag = false;
					break;
				}
			}
			if (flag)
			{
				vector = Vector3.up;
			}
			foreach (Block item2 in blocksToUpdateConnection)
			{
				item2.MoveTo(item2.GetPosition() + vector);
			}
			if (flag)
			{
				break;
			}
		}
	}

	public static List<Block> InsertModelTiles(List<List<List<Tile>>> modelBuffer, float x, float y, float z)
	{
		List<Block> list = new List<Block>();
		foreach (List<List<Tile>> item in modelBuffer)
		{
			Block block = Block.NewBlock(item);
			if (block != null)
			{
				list.Add(block);
				BWSceneManager.AddBlock(block);
			}
		}
		Bounds bounds = Util.ComputeBoundsWithSize(list);
		float f = x - bounds.center.x;
		float f2 = y - bounds.min.y;
		float f3 = z - bounds.center.z;
		Vector3 vector = new Vector3(Mathf.Round(f), Mathf.Ceil(f2), Mathf.Round(f3));
		foreach (Block item2 in list)
		{
			item2.MoveTo(item2.GetPosition() + vector);
		}
		BlockGroups.GatherBlockGroups(list);
		foreach (Block item3 in list)
		{
			if (item3 is BlockGrouped blockGrouped)
			{
				blockGrouped.UpdateSATVolumes();
			}
		}
		Scarcity.UpdateInventory();
		return list;
	}

	public static List<Block> InsertModel(string modelSource, float x, float y, float z)
	{
		Block.ClearConnectedCache();
		if (Physics.Raycast(new Vector3(x, y, z), -Vector3.up, out var hitInfo, 100f))
		{
			y = hitInfo.point.y + y;
		}
		List<List<List<Tile>>> list = ModelUtils.ParseModelString(modelSource);
		if (list == null)
		{
			return null;
		}
		List<Block> list2 = InsertModelTiles(list, x, y, z);
		ConnectednessGraph.Update(list2);
		return list2;
	}

	public static string LockModelJSON(string modelStr)
	{
		JObject obj = JSONDecoder.Decode(modelStr);
		List<List<List<Tile>>> modelBuffer = ModelUtils.ParseModelJSON(obj);
		return LockModelTiles(modelBuffer);
	}

	public static string UnlockModelJSON(string modelStr)
	{
		JObject obj = JSONDecoder.Decode(modelStr);
		List<List<List<Tile>>> modelBuffer = ModelUtils.ParseModelJSON(obj);
		return UnlockModelTiles(modelBuffer);
	}

	public static string UnlockModelTiles(List<List<List<Tile>>> modelBuffer)
	{
		for (int i = 0; i < modelBuffer.Count; i++)
		{
			List<List<Tile>> list = modelBuffer[i];
			List<Tile> list2 = list[0];
			for (int num = list2.Count - 1; num >= 0; num--)
			{
				Tile tile = list2[num];
				if (tile.gaf.Predicate == Block.predicateGroup && BlockGroups.GroupType(tile) == "locked-model")
				{
					list2.RemoveAt(num);
				}
			}
		}
		return ModelUtils.GetJSONForModel(modelBuffer);
	}

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

	public List<Block> PasteModelToFinger(List<List<List<Tile>>> model, Gestures.Touch touch, bool addHistoryState = true, BlockGroupTemplate template = null)
	{
		if (model == null || model.Count == 0)
		{
			return new List<Block>();
		}
		Deselect();
		if (ModelCollection.ModelContainsDisallowedTile(model))
		{
			BWLog.Error("Attempting to create model with disallowed gaf");
			return new List<Block>();
		}
		List<Block> list = new List<Block>();
		Vector3 vector = Util.Round(blocksworldCamera.GetTargetPosition());
		foreach (List<List<Tile>> item in model)
		{
			List<List<Tile>> tiles = CloneBlockTiles(item);
			Block block = Block.NewBlock(tiles);
			BWSceneManager.AddBlock(block);
			list.Add(block);
		}
		Vector3 vector2 = Util.Round(Util.ComputeCenter(list));
		Vector3 vector3 = Util.Round((cameraTransform.position - vector2).normalized);
		foreach (Block item2 in list)
		{
			Vector3 vector4 = item2.GetPosition() - vector2;
			item2.MoveTo(vector + vector4 + vector3);
		}
		for (int i = 0; i < 10; i++)
		{
			Vector3 zero = Vector3.zero;
			bool flag = true;
			foreach (Block item3 in list)
			{
				if (item3.IsColliding())
				{
					flag = false;
					break;
				}
			}
			if (!flag)
			{
				zero += Vector3.up;
			}
			foreach (Block item4 in list)
			{
				item4.MoveTo(item4.GetPosition() + zero);
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
			foreach (Block item5 in list)
			{
				BlockGrouped blockGrouped = item5 as BlockGrouped;
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
			if (list.Count > 0 && list[0] is BlockGrouped blockGrouped2)
			{
				Block mainBlockInGroup = blockGrouped2.GetMainBlockInGroup();
				Block.WriteDefaultExtraTiles(Tutorial.state == TutorialState.None, mainBlockInGroup.tiles, mainBlockInGroup.BlockType());
			}
			if (flag2)
			{
				SetDefaultPaintsAndTextures(list);
			}
		}
		Scarcity.UpdateInventory();
		if (addHistoryState)
		{
			History.AddStateIfNecessary();
		}
		selectedBlock = null;
		SelectBunch(list, silent: true);
		Block block2 = selectedBlock;
		if (block2 == null && selectedBunch != null && selectedBunch.blocks.Count == 1)
		{
			block2 = selectedBunch.blocks[0];
		}
		if (block2 != null)
		{
			Sound.PlayCreateSound(new GAF(Block.predicateCreate, block2.BlockType()), script: false, block2);
		}
		else
		{
			Sound.PlayOneShotSound("Create");
		}
		return list;
	}

	public static void SetDefaultPaintsAndTextures(List<Block> list)
	{
		foreach (Block item in new List<Block>(list))
		{
			for (int i = 0; i < item.subMeshGameObjects.Count + 1; i++)
			{
				string paint = item.GetDefaultPaint(i).Split(',')[0];
				item.PaintTo(paint, permanent: true, i);
				item.TextureTo(item.GetDefaultTexture(i), Vector3.zero, permanent: true, i, force: true);
			}
		}
	}

	private bool BlockTapBegan(BlockTapGesture gesture, Block block)
	{
		if (RaycastMoveBlock(block) && block != selectedBlock && selectedBunch == null && !TBox.tileButtonRotate.HitExtended(touches[0], -10f, -10f, -10f, 10f) && !TBox.tileButtonMove.HitExtended(touches[0], -10f, -10f, -10f, 10f) && !TBox.tileButtonScale.HitExtended(touches[0], -10f, -10f, -10f, 10f) && !TBox.tileCharacterEditIcon.HitExtended(touches[0], -10f, -10f, -10f, 10f) && !TBox.tileCharacterEditExitIcon.HitExtended(touches[0], -10f, -10f, -10f, 10f) && CanSelectBlock(block))
		{
			Select(block);
			TBox.Show(show: false);
			TBoxGesture.skipOneTap = true;
			return false;
		}
		return true;
	}

	private void BlockTapped(BlockTapGesture gesture, Block block)
	{
		bool flag = CanSelectBlock(block);
		if (RaycastMoveBlock(block) || gesture.GetStartMouseBlock() == mouseBlock || !flag)
		{
			Select((!flag) ? null : block);
		}
		Tutorial.Step();
	}

	public static void ZeroInventoryTileTapped(Tile zeroTile)
	{
		float magnitude = buildPanel.GetSpeed().magnitude;
		if (Tutorial.InTutorialOrPuzzle() || !(magnitude < 5f))
		{
			return;
		}
		if (zeroTile.IsCreateModel())
		{
			Dictionary<GAF, int> missing = new Dictionary<GAF, int>();
			List<GAF> list = new List<GAF>();
			ModelData modelData = modelCollection.models[(int)zeroTile.gaf.Args[0]];
			if (clipboard.AvailableModelCount(modelData, missing, list) == 0)
			{
				string text = ((list.Count <= 0) ? "Missing items:" : "A world can only have one of these:");
				string caption = "\nCould not create model " + modelData.name + "\n\n" + text;
				UI.Dialog.ShowPasteFailInfo(missing, list, caption);
			}
			return;
		}
		Dictionary<string, HashSet<string>> dictionary = GetUniqueBlockMap();
		string stringArgSafe = Util.GetStringArgSafe(zeroTile.gaf.Args, 0, string.Empty);
		if (dictionary.ContainsKey(stringArgSafe))
		{
			Block block = BWSceneManager.FindBlockOfType(stringArgSafe);
			if (block != null)
			{
				Vector3 position = block.GetPosition();
				Vector3 scale = block.Scale();
				GoToCameraFrameFor(position, scale);
				SelectBlock(block, silent: true, updateTiles: false);
			}
		}
		else
		{
			UI.Dialog.ShowZeroInventoryDialog(zeroTile);
		}
	}

	public static void BlockPanelTileTapped(Tile tile)
	{
		if (CurrentState == State.Build)
		{
			Predicate predicate = tile.gaf.Predicate;
			if (predicate.CanEditTile(tile) && (!predicate.EditableParameter.settings.hideOnLeftSide || !scriptPanel.TileOnLeftSide(tile)))
			{
				if (bw.tileParameterEditor.IsEditing() && tile.subParameterCount == 1)
				{
					tile.doubleWidth = false;
					scriptPanel.SavePositionForNextLayout();
					scriptPanel.Layout();
					bw.tileParameterEditor.StopEditing();
				}
				else
				{
					tile.doubleWidth = predicate.EditableParameter.useDoubleWidth;
					scriptPanel.SavePositionForNextLayout();
					scriptPanel.Layout();
					bw.tileParameterEditor.StartEditing(tile, predicate.EditableParameter);
				}
			}
		}
		selectedTile = tile;
	}

	public static GUISkin LoadSkin(bool hd)
	{
		return (GUISkin)Resources.Load((!hd) ? "GUI/Skin SD" : "GUI/Skin HD");
	}

	public float CalcSDLabelHeight(string text, float width)
	{
		return skin.label.CalcHeight(new GUIContent(text), width * NormalizedScreen.scale) / NormalizedScreen.scale;
	}

	public static void LoadBlocksworldSceneAsync()
	{
		if (!isLoadingScene && !loadComplete)
		{
			isLoadingScene = true;
			GameObject gameObject = new GameObject();
			gameObject.AddComponent<BlocksworldLoader>();
		}
		else if (loadComplete)
		{
			IOSInterface.BlocksworldSceneLoaded();
		}
	}

	public static void LoadBlocksworldSceneSync()
	{
		if (!loadComplete)
		{
			Application.LoadLevel("Scene");
		}
	}

	public static void SetSpeechBubbleText(string text, int index)
	{
		TileParameterEditor tileParameterEditor = bw.tileParameterEditor;
		if (mainCamera == null || tileParameterEditor == null || tileParameterEditor.parameter == null || selectedTile == null || text == null)
		{
			BWLog.Warning("Problem in SetSpeechBubbleText(). Param editor null: " + (tileParameterEditor == null) + " Selected tile null: " + (selectedTile == null) + " Text null: " + (text == null));
		}
		else
		{
			tileParameterEditor.SetEditing(e: true);
			tileParameterEditor.selectedTile = selectedTile;
			tileParameterEditor.parameter.objectValue = text;
		}
		if (tileParameterEditor != null)
		{
			tileParameterEditor.StopEditing();
		}
		selectedTile = null;
		stringInput = null;
		if (CurrentState != State.WaitForOption)
		{
			SetBlocksworldState(State.Build);
		}
	}

	private static void SetTileParameterValue(object value, int index, bool last)
	{
		if (!(mainCamera == null))
		{
			selectedTile.gaf.Args[index] = value;
			if (last)
			{
				selectedTile = null;
				SetBlocksworldState(State.Build);
			}
		}
	}

	public static void EnableWorldSave(bool enabled)
	{
		worldSaveEnabled = enabled;
	}

	public static void DisableBuildMode()
	{
		bw.forcePlayMode = true;
		buildPanel.showShopButton = false;
		UI.SidePanel.Hide();
	}

	public static void EnableBuildMode()
	{
		bw.forcePlayMode = false;
		buildPanel.showShopButton = false;
		UI.SidePanel.Show();
	}

	public static void ForceSave()
	{
		if (WorldSession.current != null)
		{
			if (CurrentState == State.Paused)
			{
				WorldSession.UnpauseCurrentSession();
			}
			if (CurrentState == State.Play)
			{
				bw.Stop();
			}
			if (CurrentState == State.Build)
			{
				bw.Save();
			}
		}
	}

	private static List<GAF> GetImplicitGAFs(List<GAF> gafList)
	{
		List<GAF> list = new List<GAF>();
		HashSet<GAF> hashSet = new HashSet<GAF>(gafList);
		HashSet<Type> hashSet2 = new HashSet<Type>();
		foreach (GAF gaf in gafList)
		{
			Type item = ((gaf.Predicate != Block.predicateCreate) ? PredicateRegistry.GetTypeForPredicate(gaf.Predicate) : Block.GetBlockTypeFromName((string)gaf.Args[0]));
			hashSet2.Add(item);
		}
		foreach (Type item2 in hashSet2)
		{
			List<GAF> implicitlyUnlockedGAFs = Scarcity.GetImplicitlyUnlockedGAFs(item2);
			foreach (GAF item3 in implicitlyUnlockedGAFs)
			{
				if (!hashSet.Contains(item3))
				{
					list.Add(item3);
					hashSet.Add(item3);
				}
			}
		}
		return list;
	}

	public static void LoadGAFUnlockData(BlocksInventory blocksInventory)
	{
		unlockedGAFs = new List<GAF>();
		unlockedPaints.Clear();
		for (int i = 0; i < blocksInventory.BlockItemIds.Count; i++)
		{
			int id = blocksInventory.BlockItemIds[i];
			if (!BlockItem.Exists(id))
			{
				continue;
			}
			BlockItem blockItem = BlockItem.FindByID(id);
			if (blockItem == null)
			{
				BWLog.Error("Unknown blockId: " + id);
				continue;
			}
			GAF gAF = new GAF(blockItem);
			unlockedGAFs.Add(gAF);
			Predicate predicate = gAF.Predicate;
			if (predicate == Block.predicateTextureTo)
			{
				string textureName = (string)gAF.Args[0];
				AddToPublicProvidedTextures(textureName);
			}
			else if (predicate == Block.predicatePaintTo)
			{
				unlockedPaints.Add((string)gAF.Args[0]);
			}
		}
		List<GAF> implicitGAFs = GetImplicitGAFs(unlockedGAFs);
		unlockedGAFs.AddRange(implicitGAFs);
	}

	public static void AddToPublicProvidedTextures(string textureName)
	{
		publicProvidedGafs.Add(new GAF("Block.TextureTo", textureName, Vector3.zero));
	}

	public static void SetBuildPanelRightSided()
	{
		buildPanel.UpdatePosition();
		UpdateTiles();
		buildPanel.PositionReset();
		UI.SidePanel.Show();
		scriptPanel.PositionReset();
	}

	public static void SetBackgroundMusic(string name)
	{
		musicPlayer.SetMusic(name);
	}

	public static void StopBackgroundMusic()
	{
		musicPlayer.Stop();
	}

	public static void SetBackgroundMusicVolumeMultiplier(float m)
	{
		musicPlayer.SetVolumeMultiplier(m);
	}

	public static void SetMusicEnabled(bool enabled)
	{
		musicPlayer.SetEnabled(enabled);
	}

	public static void UpdateLightColor(bool updateFog = true)
	{
		if (!renderingShadows)
		{
			List<Block> list = BWSceneManager.AllBlocks();
			Color white = Color.white;
			for (int i = 0; i < list.Count; i++)
			{
				Block block = list[i];
				Color lightTint = block.GetLightTint();
				white *= lightTint;
			}
			for (int j = 0; j < list.Count; j++)
			{
				Block block2 = list[j];
				Color emissiveLightTint = block2.GetEmissiveLightTint();
				white += emissiveLightTint;
			}
			white *= blocksworldCamera.GetLightTint();
			lightColor = white;
			directionalLight.GetComponent<Light>().color = white;
			if (updateFog && worldSky != null)
			{
				UpdateFogColor(BlockSky.GetFogColor());
			}
		}
	}

	public static void DefineTestRewardModel(string modelName, List<List<List<Tile>>> model)
	{
		StringBuilder stringBuilder = new StringBuilder(32768);
		StringWriter writer = new StringWriter(stringBuilder, CultureInfo.InvariantCulture);
		JSONStreamEncoder encoder = new JSONStreamEncoder(writer);
		ModelUtils.WriteJSONForModel(encoder, model);
		string text = stringBuilder.ToString();
		DefineRewardModel("Model 1", text);
		if (!Options.ExportRewardModelFromClipboard)
		{
			return;
		}
		string text2 = Application.dataPath + "/../Models";
		if (!Directory.Exists(text2))
		{
			Directory.CreateDirectory(text2);
			BWLog.Info("Created directory '" + text2 + "'");
		}
		for (int i = 0; i < 500; i++)
		{
			string text3 = text2 + "/Model " + (i + 1) + ".txt";
			string path = text2 + "/Model " + (i + 1) + " Inventory.txt";
			string iconPath = text2 + "/Model " + (i + 1) + " Icon.png";
			string screenshotPath = text2 + "/Model " + (i + 1) + " Screenshot.png";
			if (File.Exists(text3) || File.Exists(path))
			{
				continue;
			}
			BWLog.Info("Exporting model to path '" + text3 + "'");
			File.WriteAllText(text3, text);
			Dictionary<GAF, int> dictionary = new Dictionary<GAF, int>();
			foreach (List<List<Tile>> item in model)
			{
				Clipboard.CalculateGafRelevance(item, dictionary);
			}
			File.WriteAllText(path, Scarcity.GetInventoryJSON(dictionary));
			ScreenshotUtils.GenerateModelIconTexture(model, hd: true, delegate(Texture2D tex)
			{
				byte[] bytes = tex.EncodeToPNG();
				UnityEngine.Object.Destroy(tex);
				File.WriteAllBytes(iconPath, bytes);
			});
			ScreenshotUtils.GenerateModelSnapshotTexture(model, hd: true, delegate(Texture2D tex)
			{
				byte[] bytes = tex.EncodeToPNG();
				UnityEngine.Object.Destroy(tex);
				File.WriteAllBytes(screenshotPath, bytes);
			});
			break;
		}
	}

	public static void DefineRewardModel(string modelName, string modelJsonStr)
	{
		RewardVisualization.definedModels[modelName] = modelJsonStr;
	}

	public static void DefineRewardModelIcon(string modelName, string modelJsonStr)
	{
		RewardVisualization.LoadRewardModelIcon(modelName, modelJsonStr);
	}

	public static void VisualizeBlockReward(string blockCountJson)
	{
		JObject jObject = JSONDecoder.Decode(blockCountJson);
		rewardVisualizationGafs = new List<GAF>();
		foreach (JObject item2 in jObject.ArrayValue)
		{
			GAF item = new GAF("Block.VisualizeReward", (string)item2[1], (int)item2[2]);
			rewardVisualizationGafs.Add(item);
		}
		rewardVisualizationIndex = 0;
		rewardExecutionInfo.timer = 0f;
	}

	public static void VisualizeRewardModel(string modelName, string modelJsonStr)
	{
		RewardVisualization.definedModels[modelName] = modelJsonStr;
		rewardVisualizationGafs = new List<GAF>();
		rewardVisualizationGafs.Add(new GAF("Block.VisualizeReward", modelName, 1));
		rewardVisualizationIndex = 0;
		rewardExecutionInfo.timer = 0f;
	}

	public static void ShowSetPurchasePrompt(string rewardsJson, string setTitle, int setId, int setPrice)
	{
		waitForSetPurchase = true;
		JObject jObject = JSONDecoder.Decode(rewardsJson);
		Dictionary<GAF, int> dictionary = new Dictionary<GAF, int>();
		string dialogCaption = "You Won!";
		foreach (JObject item in jObject.ArrayValue)
		{
			GAF key = new GAF(item[0].StringValue, item[1].StringValue);
			int intValue = item[2].IntValue;
			dictionary[key] = intValue;
		}
		if (RewardVisualization.expectedRewardModelIconCount == 0)
		{
			UI.Dialog.ShowSetPurchasePrompt(dictionary, dialogCaption, setTitle, setId, setPrice);
		}
		else
		{
			bw.StartCoroutine(WaitForModelIconsAndShowSetPurchasePrompt(dictionary, dialogCaption, setTitle, setId, setPrice));
		}
	}

	private static IEnumerator WaitForModelIconsAndShowSetPurchasePrompt(Dictionary<GAF, int> rewards, string dialogCaption, string setTitle, int setId, int setPrice)
	{
		while (!RewardVisualization.AreRewardModelIconsLoaded())
		{
			yield return null;
		}
		UI.Dialog.ShowSetPurchasePrompt(rewards, dialogCaption, setTitle, setId, setPrice);
	}

	public static bool TileEnabled(Tile tile)
	{
		bool flag;
		if (enabledGAFs != null)
		{
			flag = enabledGAFs.Contains(tile.gaf);
		}
		else if (BW.Options.useScarcity() && Scarcity.inventory != null)
		{
			GAF gAF = tile.gaf;
			if (gAF.Predicate == Block.predicateSendCustomSignal || gAF.Predicate == Block.predicateSendCustomSignalModel || Block.customVariablePredicates.Contains(gAF.Predicate))
			{
				gAF = Scarcity.GetNormalizedGaf(gAF);
			}
			flag = Scarcity.inventory.ContainsKey(gAF) && Scarcity.inventory[gAF] != 0;
		}
		else
		{
			flag = IsUnlocked(tile);
		}
		if (CharacterEditor.Instance.InEditMode())
		{
			bool flag2 = PanelSlots.GetTabIndexForGaf(tile.gaf) == 6;
			if (tile.IsCreate())
			{
				flag2 &= tile.panelSection != 29 && tile.panelSection != 23 && tile.panelSection != 25 && tile.panelSection != 32;
				flag = flag && flag2;
			}
		}
		return flag;
	}

	private static void RemoveUnusedBlockPrefabs()
	{
		HashSet<string> hashSet = new HashSet<string>();
		List<Block> list = BWSceneManager.AllBlocks();
		foreach (Block item in list)
		{
			foreach (List<Tile> tile in item.tiles)
			{
				foreach (Tile item2 in tile)
				{
					GAF gaf = item2.gaf;
					if (gaf.Predicate == Block.predicateCreate)
					{
						hashSet.Add((string)gaf.Args[0]);
					}
				}
			}
		}
		ResourceLoader.UnloadUnusedBlockPrefabs(hashSet);
	}

	private static void RemoveUnusedTextures()
	{
		HashSet<string> hashSet = new HashSet<string>();
		List<Block> list = BWSceneManager.AllBlocks();
		foreach (Block item in list)
		{
			foreach (List<Tile> tile in item.tiles)
			{
				foreach (Tile item2 in tile)
				{
					GAF gaf = item2.gaf;
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

	private static void RemoveUnusedAssets()
	{
		bool flag = false;
		if (loadedTextureCountAfterRemovingAssets != ResourceLoader.loadedTextures.Count)
		{
			RemoveUnusedTextures();
			flag = true;
		}
		if (loadedBlockCountAfterRemovingAssets != goPrefabs.Count)
		{
			RemoveUnusedBlockPrefabs();
			flag = true;
		}
		if (flag)
		{
			Resources.UnloadUnusedAssets();
			GC.Collect();
			loadedTextureCountAfterRemovingAssets = ResourceLoader.loadedTextures.Count;
			loadedBlockCountAfterRemovingAssets = goPrefabs.Count;
		}
	}

	public static void DidReceiveMemoryWarning()
	{
		if (!RewardVisualization.rewardAnimationRunning && !inBackground)
		{
			RemoveUnusedAssets();
		}
	}

	public static Color[] GetColors(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			Color color = new Color(1f, 1f, 1f, 0f);
			return new Color[2] { color, color };
		}
		if (!colorDefinitions.ContainsKey(name))
		{
			BWLog.Info("Could not find color " + name + " using blue and red gradient");
			colorDefinitions[name] = new Color[2]
			{
				Color.blue,
				Color.red
			};
		}
		return colorDefinitions[name];
	}

	public static Color getColor(string name)
	{
		return GetColors(name)[0];
	}

	public static void SetupColorDefinitions(Dictionary<string, Color[]> colorDefs, Dictionary<string, Color[]> gradientDefs, Dictionary<string, Color> skyBoxTintDefs)
	{
		ColorDefinitions colorDefinitions = Resources.Load<ColorDefinitions>("ColorDefinitions");
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		ColorDefinition[] definitions = colorDefinitions.definitions;
		ColorDefinition[] array = definitions;
		foreach (ColorDefinition colorDefinition in array)
		{
			if (!colorDefs.ContainsKey(colorDefinition.name))
			{
				colorDefs[colorDefinition.name] = new Color[2] { colorDefinition.first, colorDefinition.second };
				dictionary[colorDefinition.name] = colorDefinition.skyBoxColorName;
			}
		}
		foreach (KeyValuePair<string, string> item in dictionary)
		{
			string key = item.Key;
			string value = item.Value;
			if (colorDefs.ContainsKey(value))
			{
				skyBoxTintDefs[key] = colorDefs[value][0];
			}
			else if (!string.IsNullOrEmpty(value))
			{
				BWLog.Error("Unable to find sky box tint '" + value + "' for paint '" + key + "'");
			}
		}
		ColorGradientDefinitions[] gradientDefinitions = colorDefinitions.gradientDefinitions;
		if (gradientDefinitions == null)
		{
			return;
		}
		ColorGradientDefinitions[] array2 = gradientDefinitions;
		foreach (ColorGradientDefinitions colorGradientDefinitions in array2)
		{
			if (!gradientDefs.ContainsKey(colorGradientDefinitions.name))
			{
				gradientDefs[colorGradientDefinitions.name] = new Color[3] { colorGradientDefinitions.skyColor, colorGradientDefinitions.equatorColor, colorGradientDefinitions.groundColor };
			}
		}
	}

	public static void SetupRarityBorders(Dictionary<RarityLevelEnum, Material> enabledMaterials, Dictionary<RarityLevelEnum, Material> disabledMaterials)
	{
		string text = ((!hd) ? "SD" : "HD");
		RarityBorderDefinitions rarityBorderDefinitions = Resources.Load<RarityBorderDefinitions>("RarityBorders/RarityBorderDefinitions" + text);
		RarityBorderDefinition[] rarityBorderDefinitions2 = rarityBorderDefinitions.rarityBorderDefinitions;
		foreach (RarityBorderDefinition rarityBorderDefinition in rarityBorderDefinitions2)
		{
			enabledMaterials[rarityBorderDefinition.rarity] = rarityBorderDefinition.tileBorderMaterialEnabled;
			disabledMaterials[rarityBorderDefinition.rarity] = rarityBorderDefinition.tileBorderMaterialDisabled;
		}
	}

	public void SetupTextureMetaDatas()
	{
		TextureMetaDatas component = blocksworldDataContainer.GetComponent<TextureMetaDatas>();
		if (component != null)
		{
			TextureMetaData[] infos = component.infos;
			TextureMetaData[] array = infos;
			foreach (TextureMetaData textureMetaData in array)
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
				TextureApplicationChangeRule[] applicationRules = textureMetaData.applicationRules;
				foreach (TextureApplicationChangeRule textureApplicationChangeRule in applicationRules)
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

	public static bool IsLuminousPaint(string paint)
	{
		if (luminousPaints.TryGetValue(paint, out var value))
		{
			return value;
		}
		value = paint.StartsWith("Luminous ");
		luminousPaints[paint] = value;
		return value;
	}

	public static bool IsLuminousTexture(string texture)
	{
		if (texture != null && texture == "Pulsate Glow")
		{
			return true;
		}
		return false;
	}

	public static void UpdateDrag()
	{
		float num = 0.2f;
		foreach (Chunk chunk in chunks)
		{
			Rigidbody rb = chunk.rb;
			if (rb != null)
			{
				float drag = num * dragMultiplier * chunk.GetDragMultiplier();
				rb.drag = drag;
			}
		}
	}

	public static void UpdateAngularDrag()
	{
		float num = 2f;
		foreach (Chunk chunk in chunks)
		{
			Rigidbody rb = chunk.rb;
			if (rb != null)
			{
				float angularDrag = num * angularDragMultiplier * chunk.GetAngularDragMultiplier();
				rb.angularDrag = angularDrag;
			}
		}
	}

	public static bool IsGlobalLockPull()
	{
		if (!staticLockPull)
		{
			return dynamicLockPull;
		}
		return true;
	}

	public static void AddUpdateCommand(Command c)
	{
		updateCommands.Add(c);
	}

	public static void AddFixedUpdateCommand(Command c)
	{
		fixedUpdateCommands.Add(c);
	}

	public static void AddFixedUpdateUniqueCommand(Command c, bool resetWhenAdded = true)
	{
		Command.AddUniqueCommand(fixedUpdateCommands, c, resetWhenAdded);
	}

	public static void AddResetStateCommand(Command c)
	{
		resetStateCommands.Add(c);
	}

	public static void AddResetStateUniqueCommand(Command c, bool resetWhenAdded = true)
	{
		Command.AddUniqueCommand(resetStateCommands, c, resetWhenAdded);
	}

	private static void UpdateEditorMusicPlayerEnabled()
	{
		if (BW.isUnityEditor)
		{
			musicPlayer.SetEnabled(IsMusicEnabledForState());
		}
	}

	public static bool InModalDialogState()
	{
		if (CurrentState != State.WaitForOption && CurrentState != State.WaitForOptionScarcityFeedback)
		{
			if (Tutorial.InTutorialOrPuzzle())
			{
				return Tutorial.progressBlocked;
			}
			return false;
		}
		return true;
	}

	public static int RemoveAllTilesWithPredicate(HashSet<Predicate> preds)
	{
		int num = 0;
		List<Block> list = BWSceneManager.AllBlocks();
		for (int i = 0; i < list.Count; i++)
		{
			Block block = list[i];
			for (int num2 = block.tiles.Count - 1; num2 >= 1; num2--)
			{
				List<Tile> list2 = block.tiles[num2];
				for (int num3 = list2.Count - 1; num3 >= 0; num3--)
				{
					if (preds.Contains(list2[num3].gaf.Predicate))
					{
						list2.RemoveAt(num3);
						num++;
					}
				}
				if (list2.Count == 1 && list2[0].gaf.Predicate == Block.predicateThen)
				{
					block.tiles.RemoveAt(num2);
				}
			}
			block.tiles.Add(Block.EmptyTileRow());
		}
		return num;
	}

	public static List<Tile> GetBlockTilesMatching(Predicate<Tile> pred)
	{
		List<Tile> list = new List<Tile>();
		List<Block> list2 = BWSceneManager.AllBlocks();
		for (int i = 0; i < list2.Count; i++)
		{
			Block block = list2[i];
			foreach (List<Tile> tile in block.tiles)
			{
				foreach (Tile item in tile)
				{
					if (pred(item))
					{
						list.Add(item);
					}
				}
			}
		}
		return list;
	}

	private void LateUpdate()
	{
		blocksworldCamera.LateUpdate();
		if (vrEnabled)
		{
			if (MappedInput.InputDown(MappableInput.RESET_VR_SENSOR))
			{
				ResetVRSensor();
			}
			bool flag = MappedInput.InputDown(MappableInput.EXIT_VR_MODE);
			if (flag | MappedInput.InputDown(MappableInput.STOP))
			{
				SetVRMode(enabled: false);
			}
		}
	}

	public static void RecieveIOSMessage(string messageStr)
	{
		switch (messageStr)
		{
		case "ReplayKitViewControllerWillAppear":
			if (WorldSession.current != null)
			{
				WorldSession.current.ReplayKitViewControllerDidAppear();
			}
			break;
		case "ReplayKitViewControllerDidDisappear":
			if (WorldSession.current != null)
			{
				WorldSession.current.ReplayKitViewControllerDidDisappear();
			}
			break;
		case "ReplayKitRecordingInterrupted":
			UI.Tapedeck.RecordingWasStoppedExternally();
			break;
		case "ReplayKitAvailablityDidChange":
			UI.Tapedeck.SetScreenRecordingEnabled(WorldSession.platformDelegate.ScreenRecordingAvailable());
			if (WorldUILayout.currentLayout != null)
			{
				WorldUILayout.currentLayout.Apply();
			}
			break;
		default:
			if (messageStr.StartsWith("CopyWorldId:"))
			{
				string worldIdClipboard = messageStr.Remove(0, 11);
				WorldSession.worldIdClipboard = worldIdClipboard;
			}
			else
			{
				BWLog.Warning("Don't understand message from iOS: " + messageStr);
			}
			break;
		}
	}

	public static void HandleWin()
	{
		foreach (KeyValuePair<BlockAnimatedCharacter, CharacterStateHandler> stateController in BlockAnimatedCharacter.stateControllers)
		{
			if (TagManager.BlockHasTag(stateController.Key, "Hero"))
			{
				stateController.Value.InterruptState(CharacterState.Win);
			}
			else if (TagManager.BlockHasTag(stateController.Key, "Villain"))
			{
				stateController.Value.InterruptState(CharacterState.Fail);
			}
		}
	}

	public static void HandleLose()
	{
		foreach (KeyValuePair<BlockAnimatedCharacter, CharacterStateHandler> stateController in BlockAnimatedCharacter.stateControllers)
		{
			if (TagManager.BlockHasTag(stateController.Key, "Hero"))
			{
				stateController.Value.InterruptState(CharacterState.Fail);
			}
			else if (TagManager.BlockHasTag(stateController.Key, "Villain"))
			{
				stateController.Value.InterruptState(CharacterState.Win);
			}
		}
	}

	static Blocksworld()
	{
		useCompactGafWriteRenamings = false;
		inBackground = false;
		selectedBunch = null;
		lockPull = false;
		cameraPoses = new List<NamedPose>();
		cameraPosesMap = new Dictionary<string, NamedPose>();
		staticLockPull = false;
		dynamicLockPull = false;
		lockInput = false;
		isFirstFrame = false;
		hideInGameUI = false;
		started = false;
		renderingShadows = false;
		renderingSkybox = false;
		renderingWater = false;
		_lightIntensityBasic = 1f;
		_lightIntensityMultiplier = 1f;
		_buildModeFogColor = Color.white;
		_buildModeFogStart = 0f;
		_buildModeFogEnd = 0f;
		updateCommands = new List<Command>();
		fixedUpdateCommands = new List<Command>();
		resetStateCommands = new List<Command>();
		unlockedPaints = new HashSet<string>();
		resetting = false;
		loadComplete = false;
		isLoadingScene = false;
		resettingPlay = false;
		capturingScreenshot = false;
		constrainedManipulationAxis = Vector3.up;
		editorSelectionLocked = new HashSet<Block>();
		keyLReleased = true;
		recentSelectionUnlockedBlock = null;
		currentBackgroundMusic = string.Empty;
		loadedTextureCountAfterRemovingAssets = -1;
		loadedBlockCountAfterRemovingAssets = -1;
		interpolateRigidBodies = false;
		marginTile = -16;
		defaultPanelPadding = 32;
		fogMultiplier = 1f;
		fogStart = 40f;
		fogEnd = 100f;
		fogColor = new Color(0.38039216f, 63f / 85f, 1f);
		existingBlockNames = new HashSet<string>();
		prefabs = new Dictionary<string, GameObject>();
		goPrefabs = new Dictionary<string, GameObject>();
		meshes = new Dictionary<string, Mesh>();
		colliders = new Dictionary<string, Collider>();
		compoundColliders = new Dictionary<string, GameObject>();
		shapes = new Dictionary<string, GameObject>();
		glues = new Dictionary<string, GameObject>();
		joints = new Dictionary<string, GameObject>();
		globalGafs = new List<GAF>();
		rewardExecutionInfo = new ScriptRowExecutionInfo();
		stopASAP = false;
		waitForSetPurchase = false;
		canSaveInMenu = true;
		worldSaveEnabled = true;
		f3PressedInCurrentWorld = false;
		worldSky = null;
		worldOcean = null;
		worldOceanBlock = null;
		blocksworldCamera = new BlocksworldCamera();
		prevCamPos = Util.nullVector3;
		cameraMoved = true;
		lightColor = Color.white;
		dynamicLightColor = Color.white;
		dynamicLightIntensityMultiplier = 1f;
		dynamicalLightChangers = new List<ILightChanger>();
		weather = WeatherEffect.clear;
		publicProvidedGafs = new HashSet<GAF>();
		enabledGAFs = null;
		enabledPanelBlock = null;
		joysticks = new Dictionary<OldSymbol, float>();
		lastRealtimeSinceStartup = 0f;
		deltaTime = 0f;
		numTouches = 0;
		touches = new Vector3[20];
		mouseBlock = null;
		mouseBlockIndex = 0;
		mouseBlockNormal = Vector3.zero;
		mouseBlockHitPosition = Vector3.zero;
		mouseBlockLast = null;
		tWidgetHitAtStart = false;
		tBoxHit = false;
		tBoxHitAtStart = false;
		mouseBlockNormalLast = Vector3.zero;
		selectedBlock = null;
		stringInput = null;
		consumeEvent = false;
		recognizer = new GestureRecognizer();
		currentWorldId = null;
		angularDragMultiplier = 1f;
		dragMultiplier = 1f;
		locked = new List<Block>();
		chunks = new List<Chunk>();
		timerStart = -1f;
		timerStop = -1f;
		gameStart = false;
		sending = new bool[26];
		sendingValues = new float[26];
		signalNames = new string[26];
		sendingCustom = new Dictionary<string, float>();
		blockNames = new Dictionary<Block, string>();
		numSendTilesInUse = 0;
		numTagTilesInUse = 0;
		everPresentTagsInUse = new HashSet<string>();
		arrowTags = new HashSet<string>();
		handAttachmentTags = new HashSet<string>();
		laserTags = new HashSet<string>();
		projectileTags = new HashSet<string>();
		waterTags = new HashSet<string>();
		customSignals = new HashSet<string>();
		customIntVariables = new Dictionary<string, int>();
		blockIntVariables = new Dictionary<Block, Dictionary<string, int>>();
		gameTime = 0f;
		updateCounter = 0;
		playFixedUpdateCounter = 0;
		numCounters = 10;
		countersActivated = new Dictionary<string, bool>();
		counters = new Dictionary<string, int>();
		counterTargets = new Dictionary<string, int>();
		counterTargetsActivated = new Dictionary<string, bool>();
		frustumPlanes = new Plane[0];
		maxBlockTapDistance = 1060f;
		maxBlockDragDistance = 1060f;
		iconColors = new Dictionary<string, string>();
		colorDefinitions = new Dictionary<string, Color[]>();
		ambientLightGradientDefinitions = new Dictionary<string, Color[]>();
		skyBoxTintDefinitions = new Dictionary<string, Color>();
		rarityBorderMaterialsEnabled = new Dictionary<RarityLevelEnum, Material>();
		rarityBorderMaterialsDisabled = new Dictionary<RarityLevelEnum, Material>();
		defaultGravityStrength = 20f;
		VR_Default = VRType.VR_None;
		vrEnabled = false;
		currentVRType = VR_Default;
		worldSessionHadVR = false;
		worldSessionHadBlocksterMover = false;
		worldSessionHadBlocksterSpeaker = false;
		worldSessionHadBlockTap = false;
		worldSessionCoinsCollected = 0;
		worldSessionHadHypderjumpUse = false;
		_currentState = State.Build;
		counterNames = new string[3] { "0", "1", "2" };
		taggedPredicates = null;
		uniqueBlockMap = null;
		magnetPredicates = null;
		taggedHandAttachmentPreds = null;
		taggedArrowPreds = null;
		taggedLaserPreds = null;
		taggedProjectilePreds = null;
		taggedWaterPreds = null;
		luminousPaints = new Dictionary<string, bool>();
	}
}
