using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Exdilin;
using Gestures;
using UnityEngine;

namespace Blocks;

public class Block : ITBox, ILightChanger
{
	public static bool goTouchStarted;

	public static GameObject goTouched;

	public static GameObject prefabShadow;

	public static Transform shadowParent;

	private BlockMetaData meta;

	public HashSet<BlockGroup> groups;

	public Chunk chunk;

	public List<Block> connections = new List<Block>();

	public List<int> connectionTypes = new List<int>();

	public Block modelBlock;

	private float blockMass = -1f;

	private float blockMassOverride = -1f;

	private float storedMassOverride = -1f;

	private bool overridingMass;

	private static List<Block> massAlteredBlocks;

	private static int nextBlockId;

	public GameObject go;

	public Transform goT;

	public GameObject goShadow;

	public Transform goShadowT;

	private bool hasShadow;

	public string colliderName = string.Empty;

	protected Layer goLayerAssignment;

	public List<List<Tile>> tiles = new List<List<Tile>>();

	public Mesh mesh;

	public Dictionary<string, Mesh> childMeshes;

	public Mesh meshShadow;

	protected Color[] colorsShadow;

	public Renderer renderer;

	private bool isScaled;

	private Vector3 colliderScale = Vector3.one;

	private Vector3 meshScale = Vector3.one;

	private string meshScaleTexture = string.Empty;

	public float effectPower;

	public Vector3 size = Vector3.one;

	public Vector3 shadowSize = Vector3.one;

	public bool broken;

	public bool vanished;

	public bool didFix;

	public bool isTreasure;

	public bool gluedOnContact;

	public bool allowGlueOnContact = true;

	private bool hadRigidBody;

	public bool activateForPlay = true;

	public int lastTeleportedFrameCount = -1;

	public bool tagBumpEnabled;

	public bool isTransparent;

	public bool isRuntimeInvisible;

	public bool isRuntimePhantom;

	public int frozenInTerrainStatus = -1;

	public CollisionMesh[] shapeMeshes;

	public CollisionMesh[] glueMeshes;

	public CollisionMesh[] jointMeshes;

	public const float JOINT_ANGULAR_DRIVE_FORCE_FACTOR = 100f;

	public const float JOINT_ANGULAR_SPRING_DAMPING_FACTOR = 0.0035f;

	public const float JOINT_ANGULAR_SPRING_FACTOR = 0.5f;

	public const float cycleTime = 0.25f;

	public ScriptRowExecutionInfo[] executionInfos = new ScriptRowExecutionInfo[1];

	public bool skipUpdateSATVolumes;

	public bool isTerrain;

	public bool fakeTerrain;

	public bool containsPlayModeTiles;

	public float lastShadowHitDistance = -2f;

	private Vector3 lastShadowHitPoint;

	protected Vector3 oldShadowHitNormal = Vector3.up;

	private float shadowMaxDistance = 5f;

	private float shadowStrengthMultiplier = 1f;

	private float oldShadowAlpha = -1f;

	public Vector3 oldPos = Util.nullVector3;

	private Quaternion oldRotation = Quaternion.Euler(new Vector3(12.3f, 34.8f, 34.2f));

	public float buoyancyMultiplier = 1f;

	protected const float METAL_BUOYANCY_MULTIPLIER = 0.2f;

	public static AnimationCurve vanishAnimCurve;

	private int animationStep;

	private float animationFixedTime;

	private Vector3 playPosition;

	private Vector3 parentPlayPosition;

	public Quaternion playRotation;

	private Quaternion parentPlayRotation;

	private Vector3 objectAnimationPositionOffset;

	private Quaternion objectAnimationRotationOffset;

	private Vector3 objectAnimationScaleOffset;

	internal int rbConstraintsOn;

	internal int rbConstraintsOff;

	internal bool rbUpdatedConstraints;

	protected List<CollisionMesh> subMeshes;

	protected List<string> subMeshPaints;

	protected List<string> subMeshTextures;

	protected List<Vector3> subMeshTextureNormals;

	public List<GameObject> subMeshGameObjects;

	private CollisionMesh mainMesh;

	protected bool[] canBeTextured;

	protected bool[] canBeMaterialTextured;

	protected AudioSource audioSource;

	protected AudioSource loopingAudioSource;

	protected AudioLowPassFilter lpFilter;

	public static LeaderboardData leaderboardData;

	public static Predicate predicateThen;

	public static Predicate predicateStop;

	public static Predicate predicateSectionIndex;

	public static Predicate predicateHideTileRow;

	public static Predicate predicateHideNextTile;

	public static Predicate predicateLocked;

	public static Predicate predicateUnlocked;

	public static Predicate predicateCreate;

	public static Predicate predicateCreateModel;

	public static Predicate predicateFreeze;

	public static Predicate predicateUnfreeze;

	public static Predicate predicatePaintTo;

	public static Predicate predicateTextureTo;

	public static Predicate predicateMoveTo;

	public static Predicate predicateRotateTo;

	public static Predicate predicateScaleTo;

	public static Predicate predicatePlaySoundDurational;

	public static Predicate predicateSetFog;

	public static Predicate predicateVanishBlock;

	public static Predicate predicateVanishModel;

	public static Predicate predicateGroup;

	public static Predicate predicateUI;

	public static Predicate predicateUIOpaque;

	public static Predicate predicateTutorialCreateBlockHint;

	public static Predicate predicateTutorialAutoAddBlock;

	public static Predicate predicateTutorialRemoveBlockHint;

	public static Predicate predicateTutorialRotateExistingBlock;

	public static Predicate predicateTutorialPaintExistingBlock;

	public static Predicate predicateTutorialTextureExistingBlock;

	public static Predicate predicateTutorialOperationPose;

	public static Predicate predicateTutorialMoveBlock;

	public static Predicate predicateTutorialMoveModel;

	public static Predicate predicateTutorialHideInBuildMode;

	public static Predicate predicateImpact;

	public static Predicate predicateImpactModel;

	public static Predicate predicateParticleImpact;

	public static Predicate predicateParticleImpactModel;

	public static Predicate predicateBump;

	public static Predicate predicateBumpChunk;

	public static Predicate predicateBumpModel;

	public static Predicate predicateTaggedBump;

	public static Predicate predicateTaggedBumpModel;

	public static Predicate predicateTaggedBumpChunk;

	public static Predicate predicateNegate;

	public static Predicate predicateNegateMod;

	public static Predicate predicateAnimate;

	public static Predicate predicatePulledByMagnet;

	public static Predicate predicatePulledByMagnetModel;

	public static Predicate predicatePushedByMagnet;

	public static Predicate predicatePushedByMagnetModel;

	public static Predicate predicateGameOver;

	public static Predicate predicateGameWin;

	public static Predicate predicateGameLose;

	public static Predicate predicateTag;

	public static Predicate predicateWithinTaggedBlock;

	public static Predicate predicateCustomTag;

	public static Predicate predicateObjectCounterIncrement;

	public static Predicate predicateObjectCounterDecrement;

	public static Predicate predicateObjectCounterEquals;

	public static Predicate predicateObjectCounterEqualsMax;

	public static Predicate predicateObjectCounterValueCondition;

	public static Predicate predicateCounterEquals;

	public static Predicate predicateCounterValueCondition;

	public static Predicate predicateGaugeValueCondition;

	public static Predicate predicateTimerEquals;

	public static Predicate predicateTimerValueCondition;

	public static Predicate predicateIsTreasure;

	public static Predicate predicateIsTreasureForTag;

	public static Predicate predicateIsPickup;

	public static Predicate predicateIsPickupForTag;

	public static Predicate predicateWithinWater;

	public static Predicate predicateModelWithinWater;

	public static Predicate predicateWithinTaggedWater;

	public static Predicate predicateModelWithinTaggedWater;

	public static Predicate predicateSetSpawnPoint;

	public static Predicate predicateSetActiveCheckpoint;

	public static Predicate predicateSpawn;

	public static Predicate predicateWait;

	public static Predicate predicateWaitTime;

	public static Predicate predicateFirstFrame;

	public static Predicate predicateLevitate;

	public static Predicate predicateSpeak;

	public static Predicate predicateTiltLeftRight;

	public static Predicate predicateTiltFrontBack;

	public static Predicate predicateButton;

	public static Predicate predicateDPadHorizontal;

	public static Predicate predicateDPadVertical;

	public static Predicate predicateDPadMoved;

	public static Predicate predicateSendSignal;

	public static Predicate predicateSendCustomSignal;

	public static Predicate predicateSendSignalModel;

	public static Predicate predicateSendCustomSignalModel;

	public static Predicate predicateVariableCustomInt;

	public static Predicate predicateBlockVariableInt;

	public static Predicate predicateBlockVariableIntLoadGlobal;

	public static Predicate predicateBlockVariableIntStoreGlobal;

	public static HashSet<Predicate> customVariablePredicates;

	public static HashSet<Predicate> globalVariableOperations;

	public static HashSet<Predicate> blockVariableOperations;

	public static HashSet<Predicate> blockVariableOperationsOnGlobals;

	public static HashSet<Predicate> blockVariableOperationsOnOtherBlockVars;

	public static Dictionary<Predicate, int> variablePredicateParamDefaults;

	public static Dictionary<Predicate, string> variablePredicateLabels;

	public static Predicate predicateTapBlock;

	public static Predicate predicateTapChunk;

	public static Predicate predicateTapModel;

	public static Predicate predicateTapHoldBlock;

	public static Predicate predicateTapHoldModel;

	public static Predicate predicateCamFollow;

	public static Predicate predicateTutorialHelpTextAction;

	public static HashSet<Predicate> noTilesAfterPredicates;

	public static Dictionary<string, Vector3[]> defaultTextureNormals;

	public static Dictionary<string, bool[]> blockTypeCanBeTextured;

	public static Dictionary<string, bool[]> blockTypeCanBeMaterialTextured;

	public static Dictionary<string, Vector3> defaultScales;

	public static Dictionary<string, Vector3> defaultOrientations;

	public static Dictionary<string, HashSet<string>> defaultSfxs;

	public static Dictionary<string, HashSet<string>> sameGridCellPairs;

	public static readonly HashSet<string> treasureBlocks;

	public static readonly string treasurePickupSfx;

	protected static Dictionary<string, List<List<Tile>>> defaultExtraTiles;

	protected static Dictionary<string, Action<List<List<Tile>>>> defaultExtraTilesProcessors;

	private static Dictionary<string, Vector3> blockSizes;

	private static Dictionary<string, float[]> blockMassConstants;

	private static ScriptRowExecutionInfo resetExecutionInfo;

	private static HashSet<Predicate> notPlayModePredicates;

	private int shadowUpdateInterval = 1;

	private int shadowUpdateCounter;

	private static Bounds shadowBounds;

	private const float CAMERA_MOVE_THRESHOLD_SQR = 1.0000001E-06f;

	private const float CAMERA_ROT_THRESHOLD_SQR = 0.0001f;

	private static HashSet<string> emptySet;

	public static Dictionary<string, Vector3> canScales;

	protected static HashSet<string> skinPaints;

	public static Dictionary<Block, List<Block>> connectedCache;

	public static Dictionary<Block, HashSet<Chunk>> connectedChunks;

	private static HashSet<Predicate> keepRBPreds;

	private static DelegateMultistepCommand stepInvincibilityCommand;

	protected static UnmuteAllBlocksCommand unmuteCommand;

	private static LoopSfxCommand loopSfxCommand;

	public static HashSet<Block> vanishingOrAppearingBlocks;

	private const float SPEECH_BUBBLE_DELAY = 0.4f;

	private bool wasHorizNegative;

	private bool wasHorizPositive;

	private bool wasVertNegative;

	private bool wasVertPositive;

	private const string CHUNK_GLUE_ON_CONTACT_SIZE = "CGOCS";

	private static UpdateBlockIsTriggerCommand updateBlockTriggerCommand;

	private const string RANDOM_WAIT_TIME = "RandomWaitTime";

	private const string RANDOM_WAIT_TIME_SENSOR = "RandomWaitTimeSensor";

	private static StopScriptsCommand stopScriptsCommand;

	private static UnlockInputCommand unlockInputCommand;

	public static Color transparent;

	private static Dictionary<string, Vector3[]> blockScaleConstraints;

	public static Dictionary<string, List<HashSet<string>>> animationSupports;

	private List<GameObject> fakeRigidbodyGos;

	private static HashSet<Predicate> taggedBumpPreds;

	protected AudioClip loopClip;

	protected string loopName = string.Empty;

	public static Dictionary<string, Type> blockNameTypeMap;

	protected MeshColliderInfo meshColliderInfo;

	private static HashSet<Predicate> possibleModelConflictingInputPredicates;

	private static HashSet<Predicate> inputPredicates;

	private static HashSet<Predicate> emptyPredicateSet;

	[CompilerGenerated]
	private static Dictionary<string, int> f__switch_map3;

	[CompilerGenerated]
	private static PredicateSensorDelegate f__mg_cache0;

	[CompilerGenerated]
	private static PredicateActionDelegate f__mg_cache1;

	[CompilerGenerated]
	private static PredicateSensorDelegate f__mg_cache2;

	[CompilerGenerated]
	private static PredicateActionDelegate f__mg_cache3;

	[CompilerGenerated]
	private static PredicateActionDelegate f__mg_cache4;

	[CompilerGenerated]
	private static PredicateActionDelegate f__mg_cache5;

	[CompilerGenerated]
	private static Action<Block, string, float, float> f__mg_cache6;

	[CompilerGenerated]
	private static Action<Block, string, float, float> f__mg_cache7;

	[CompilerGenerated]
	private static Action<Block> f__mg_cache8;

	public string currentPaint { get; private set; }

	public Block(List<List<Tile>> tiles)
	{
		this.tiles = tiles;
		isTerrain = this is BlockSky || this is BlockTerrain || this is BlockAbstractWater || this is BlockBillboard;
		string blockType = BlockType();
		go = Blocksworld.InstantiateBlockGo(blockType);
		goT = go.transform;
		MeshFilter component = go.GetComponent<MeshFilter>();
		SkinnedMeshRenderer component2 = go.GetComponent<SkinnedMeshRenderer>();
		if (component != null || component2 != null)
		{
			if (component2 != null)
			{
				component2.sharedMesh = UnityEngine.Object.Instantiate(component2.sharedMesh);
				mesh = component2.sharedMesh;
			}
			else if (component != null)
			{
				mesh = component.mesh;
			}
		}
		if (goT.childCount > 0)
		{
			childMeshes = new Dictionary<string, Mesh>();
			List<GameObject> list = new List<GameObject>();
			foreach (object item in goT)
			{
				Transform transform = (Transform)item;
				list.Add(transform.gameObject);
			}
			list.Sort(new GameObjectNameComparer());
			foreach (GameObject item2 in list)
			{
				MeshFilter component3 = item2.GetComponent<MeshFilter>();
				SkinnedMeshRenderer component4 = item2.GetComponent<SkinnedMeshRenderer>();
				if (childMeshes.ContainsKey(item2.name))
				{
					BWLog.Error("Block contains duplicate child object names: " + item2.name);
				}
				if (component3 != null)
				{
					childMeshes[item2.name] = component3.mesh;
				}
				else if (component4 != null)
				{
					component4.sharedMesh = UnityEngine.Object.Instantiate(component4.sharedMesh);
					childMeshes[item2.name] = component4.sharedMesh;
				}
			}
		}
		renderer = go.GetComponent<Renderer>();
		if (renderer == null)
		{
			renderer = go.GetComponent<SkinnedMeshRenderer>();
		}
		goLayerAssignment = (isTerrain ? Layer.Terrain : Layer.Default);
		go.layer = (int)goLayerAssignment;
		if (!Blocksworld.renderingShadows && HasShadow())
		{
			InstantiateShadow(prefabShadow);
		}
		if (this is BlockSky)
		{
			Blocksworld.worldSky = (BlockSky)this;
		}
		if (this is BlockWater)
		{
			Blocksworld.worldOcean = go;
			Blocksworld.worldOceanBlock = (BlockWater)this;
		}
		go.name = nextBlockId++.ToString();
		AddMissingScaleTo();
		FindSubMeshes();
		size = BlockSize();
		shadowSize = size;
		CalculateMaxExtent();
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
		for (int i = 0; i < subMeshes.Count; i++)
		{
			int meshIndex = i + 1;
			SetSubmeshInitPaintToTile(meshIndex, paint, ignoreIfExists: true);
			SetSubmeshInitTextureToTile(meshIndex, texture, Vector3.up, ignoreIfExists: true);
		}
		UpdateRuntimeInvisible();
		Reset();
	}

	public List<Block> ConnectionsOfType(int connectionType, bool directed = false)
	{
		List<Block> list = new List<Block>();
		for (int i = 0; i < connections.Count; i++)
		{
			if ((Mathf.Abs(connectionTypes[i]) & connectionType) != 0 && (!directed || connectionTypes[i] >= 0))
			{
				list.Add(connections[i]);
			}
		}
		return list;
	}

	public static void Register()
	{
		predicateMoveTo = PredicateRegistry.Add<Block>("Block.MoveTo", null, (Block b) => b.MoveToAction, new Type[1] { typeof(Vector3) });
		predicateRotateTo = PredicateRegistry.Add<Block>("Block.RotateTo", null, (Block b) => b.RotateToAction, new Type[1] { typeof(Vector3) });
		predicateScaleTo = PredicateRegistry.Add<Block>("Block.ScaleTo", null, (Block b) => b.ScaleToAction, new Type[1] { typeof(Vector3) });
		predicateTextureTo = PredicateRegistry.Add<Block>("Block.TextureTo", (Block b) => b.IsTexturedTo, (Block b) => b.TextureToAction, new Type[3]
		{
			typeof(string),
			typeof(Vector3),
			typeof(int)
		}, new string[3] { "Texture name", "Normal", "Submesh index" });
		predicatePaintTo = PredicateRegistry.Add<Block>("Block.PaintTo", (Block b) => b.IsPaintedTo, (Block b) => b.PaintToAction, new Type[2]
		{
			typeof(string),
			typeof(int)
		}, new string[2] { "Paint name", "Submesh index" });
		predicateGroup = PredicateRegistry.Add<Block>("Block.Group", null, (Block b) => b.IgnoreAction, new Type[3]
		{
			typeof(int),
			typeof(string),
			typeof(int)
		}, new string[3] { "Group ID", "Group type", "Main block" });
		predicateSetFog = PredicateRegistry.Add<Block>("Block.SetFog", null, (Block b) => b.SetFogAction, new Type[3]
		{
			typeof(string),
			typeof(string),
			typeof(string)
		});
		PredicateRegistry.Add<Block>("Block.PaintSkyTo", (Block b) => b.IsSkyPaintedTo, (Block b) => b.PaintSkyToAction, new Type[2]
		{
			typeof(string),
			typeof(int)
		});
		PredicateRegistry.Add<Block>("Error", (Block b) => b.IgnoreSensor, (Block b) => b.IgnoreAction, new Type[1] { typeof(string) }, new string[1] { "Message" });
		predicateGameOver = PredicateRegistry.Add<Block>("Meta.GameOver", null, (Block b) => b.GameOver, new Type[3]
		{
			typeof(string),
			typeof(float),
			typeof(string)
		}, new string[3] { "Message", "Duration", "SFX" });
		predicateGameWin = PredicateRegistry.Add<Block>("Meta.GameWin", null, (Block b) => b.GameWin, new Type[3]
		{
			typeof(string),
			typeof(float),
			typeof(string)
		}, new string[3] { "Message", "Duration", "SFX" });
		predicateGameLose = PredicateRegistry.Add<Block>("Meta.GameLose", null, (Block b) => b.GameLose, new Type[3]
		{
			typeof(string),
			typeof(float),
			typeof(string)
		}, new string[3] { "Message", "Duration", "SFX" });
		noTilesAfterPredicates.Add(predicateGameOver);
		noTilesAfterPredicates.Add(predicateGameWin);
		noTilesAfterPredicates.Add(predicateGameLose);
		predicateTag = PredicateRegistry.Add<Block>("Position.Position", null, (Block b) => b.RegisterTag, new Type[1] { typeof(string) });
		predicateWithinTaggedBlock = PredicateRegistry.Add<Block>("Position.IsWithin", (Block b) => b.WithinTaggedBlock, null, new Type[1] { typeof(string) });
		predicateCustomTag = PredicateRegistry.Add<Block>("Block.CustomTag", null, (Block b) => b.RegisterTag, new Type[1] { typeof(string) });
		PredicateRegistry.Add<Block>("Variables.GlobalBooleanVariableEquals", (Block b) => VariableManager.GlobalBooleanVariableValueEquals, (Block b) => VariableManager.SetGlobalBooleanVariableValue, new Type[2]
		{
			typeof(string),
			typeof(int)
		}, new string[2] { "Variable name", "Value" });
		PredicateRegistry.Add<Block>("Variables.GlobalIntegerVariableEquals", (Block b) => VariableManager.GlobalIntegerVariableValueEquals, (Block b) => VariableManager.SetGlobalIntegerVariableValue, new Type[2]
		{
			typeof(string),
			typeof(int)
		}, new string[2] { "Variable name", "Value" });
		PredicateRegistry.Add<Block>("Variables.RandomizeGlobalIntegerVariable", null, (Block b) => VariableManager.RandomizeGlobalIntegerVariable, new Type[3]
		{
			typeof(string),
			typeof(int),
			typeof(int)
		}, new string[3] { "Variable name", "Min (inclusive)", "Max (exclusive)" });
		PredicateRegistry.Add<Block>("Variables.IncrementIntegerVariable", null, (Block b) => VariableManager.IncrementIntegerVariable, new Type[3]
		{
			typeof(string),
			typeof(int),
			typeof(int)
		}, new string[2] { "Variable name", "Increment" });
		PredicateRegistry.Add<Block>("Block.InCameraFrustum", (Block b) => b.InCameraFrustum);
		PredicateRegistry.Add<Block>("GaugeUI.Equals", (Block b) => b.GaugeUIValueEquals, (Block b) => b.GaugeUISetValue, new Type[2]
		{
			typeof(int),
			typeof(int)
		});
		PredicateRegistry.Add<Block>("GaugeUI.Fraction", (Block b) => b.GaugeUIGetFraction, null, new Type[2]
		{
			typeof(float),
			typeof(int)
		});
		PredicateRegistry.Add<Block>("GaugeUI.Increment", null, (Block b) => b.GaugeUIIncrementValue, new Type[2]
		{
			typeof(int),
			typeof(int)
		});
		predicateObjectCounterIncrement = PredicateRegistry.Add<Block>("ObjectCounterUI.Increment", null, (Block b) => b.IncrementObjectCounterUI, new Type[2]
		{
			typeof(int),
			typeof(int)
		}, new string[2] { "Increment", "Counter Index" });
		predicateObjectCounterDecrement = PredicateRegistry.Add<Block>("ObjectCounterUI.Decrement", null, (Block b) => b.DecrementObjectCounterUI, new Type[2]
		{
			typeof(int),
			typeof(int)
		}, new string[2] { "Decrement", "Counter Index" });
		predicateObjectCounterEquals = PredicateRegistry.Add<Block>("ObjectCounterUI.Equals", (Block b) => b.ObjectCounterUIValueEquals, (Block b) => b.ObjectCounterUISetValue, new Type[2]
		{
			typeof(int),
			typeof(int)
		}, new string[2] { "Value", "Counter Index" });
		predicateObjectCounterEqualsMax = PredicateRegistry.Add<Block>("ObjectCounterUI.EqualsMax", (Block b) => b.ObjectCounterUIValueEqualsMaxValue, (Block b) => b.ObjectCounterUISetValueToMax, new Type[1] { typeof(int) }, new string[1] { "Counter Index" });
		PredicateRegistry.Add<Block>("GaugeUI.EqualsMax", (Block b) => b.GaugeUIValueEqualsMaxValue, (Block b) => b.GaugeUISetValueToMax, new Type[1] { typeof(int) }, new string[1] { "Gauge Index" });
		PredicateRegistry.Add<Block>("TimerUI.EqualsMax", (Block b) => b.TimerUIValueEqualsMaxValue, (Block b) => b.TimerUISetValueToMax, new Type[1] { typeof(int) }, new string[1] { "Timer Index" });
		PredicateRegistry.Add<Block>("CounterUI.EqualsMin", (Block b) => b.CounterUIValueEqualsMinValue, (Block b) => b.CounterUISetValueToMin, new Type[1] { typeof(int) }, new string[1] { "Counter Index" });
		PredicateRegistry.Add<Block>("CounterUI.EqualsMax", (Block b) => b.CounterUIValueEqualsMaxValue, (Block b) => b.CounterUISetValueToMax, new Type[1] { typeof(int) }, new string[1] { "Counter Index" });
		predicateObjectCounterValueCondition = PredicateRegistry.Add<Block>("ObjectCounterUI.ValueCondition", (Block b) => b.ObjectCounterUIValueCondition, null, new Type[3]
		{
			typeof(int),
			typeof(int),
			typeof(int)
		}, new string[3] { "Value", "0: lt, 1: gt, 2: !=", "Counter index" });
		predicateCounterEquals = PredicateRegistry.Add<Block>("CounterUI.Equals", (Block b) => b.CounterUIValueEquals, (Block b) => b.SetCounterUIValue, new Type[2]
		{
			typeof(int),
			typeof(int)
		}, new string[2] { "Value", "Counter Index" });
		PredicateRegistry.Add<Block>("CounterUI.Increment", null, (Block b) => b.IncrementCounterUIValue, new Type[2]
		{
			typeof(int),
			typeof(int)
		}, new string[2] { "Value", "Counter Index" });
		PredicateRegistry.Add<Block>("CounterUI.AnimateScore", null, (Block b) => b.CounterUIAnimateScore, new Type[2]
		{
			typeof(int),
			typeof(int)
		}, new string[2] { "Animation type", "Counter Index" });
		PredicateRegistry.Add<Block>("CounterUI.ScoreMultiplier", null, (Block b) => b.CounterUIScoreMultiplier, new Type[3]
		{
			typeof(int),
			typeof(int),
			typeof(int)
		}, new string[3] { "Multiplier", "Positive only", "Counter Index" });
		PredicateRegistry.Add<Block>("CounterUI.GlobalScoreMultiplier", null, (Block b) => b.CounterUIGlobalScoreMultiplier, new Type[3]
		{
			typeof(int),
			typeof(int),
			typeof(int)
		}, new string[3] { "Multiplier", "Positive only", "Counter Index" });
		PredicateRegistry.Add<Block>("CounterUI.Randomize", null, (Block b) => b.RandomizeCounterUI, new Type[3]
		{
			typeof(int),
			typeof(int),
			typeof(int)
		}, new string[3] { "Min", "Max", "Counter Index" });
		predicateCounterValueCondition = PredicateRegistry.Add<Block>("CounterUI.ValueCondition", (Block b) => b.CounterUIValueCondition, null, new Type[3]
		{
			typeof(int),
			typeof(int),
			typeof(int)
		}, new string[3] { "Value", "0: lt, 1: gt, 2: !=", "Counter index" });
		predicateGaugeValueCondition = PredicateRegistry.Add<Block>("GaugeUI.ValueCondition", (Block b) => b.GaugeUIValueCondition, null, new Type[3]
		{
			typeof(int),
			typeof(int),
			typeof(int)
		}, new string[3] { "Value", "0: lt, 1: gt, 2: !=", "Gauge index" });
		PredicateRegistry.Add<Block>("TimerUI.Start", null, (Block b) => b.TimerUIStartTimer, new Type[2]
		{
			typeof(int),
			typeof(int)
		});
		PredicateRegistry.Add<Block>("TimerUI.Pause", null, (Block b) => b.TimerUIPauseTimer, new Type[1] { typeof(int) });
		PredicateRegistry.Add<Block>("TimerUI.PauseUI", null, (Block b) => b.TimerUIPauseUI, new Type[2]
		{
			typeof(float),
			typeof(int)
		});
		PredicateRegistry.Add<Block>("TimerUI.Wait", null, (Block b) => b.TimerUIWaitTimer, new Type[2]
		{
			typeof(float),
			typeof(int)
		});
		predicateTimerValueCondition = PredicateRegistry.Add<Block>("TimerUI.ValueCondition", (Block b) => b.TimerUIValueCondition, null, new Type[3]
		{
			typeof(float),
			typeof(int),
			typeof(int)
		}, new string[3] { "Value", "0: lt, 1: gt, 2: !=", "Timer index" });
		predicateTimerEquals = PredicateRegistry.Add<Block>("TimerUI.Equals", (Block b) => b.TimerUITimeEquals, (Block b) => b.TimerUISetTime, new Type[2]
		{
			typeof(float),
			typeof(int)
		});
		PredicateRegistry.Add<Block>("TimerUI.Increment", null, (Block b) => b.TimerUIIncrementTime, new Type[2]
		{
			typeof(float),
			typeof(int)
		});
		predicateNegate = PredicateRegistry.Add<Block>("Meta.Negate", (Block b) => b.IgnoreSensor);
		predicateNegateMod = PredicateRegistry.Add<Block>("Meta.NegateModifier", (Block b) => b.IgnoreSensor);
		predicateCreate = PredicateRegistry.Add<Block>("Block.Create", null, null, new Type[1] { typeof(string) });
		predicateCreateModel = PredicateRegistry.Add<Block>("Model.Create", null, null, new Type[1] { typeof(int) });
		PredicateRegistry.Add<Block>("Button", null, null, new Type[1] { typeof(string) });
		PredicateRegistry.Add<Block>("Control", null, null, new Type[1] { typeof(string) });
		PredicateRegistry.Add<Block>("Radar", null, null, new Type[1] { typeof(string) });
		PredicateRegistry.Add<Block>("Key", null, null, new Type[1] { typeof(string) });
		PredicateRegistry.Add<Block>("Hand", null, null, new Type[1] { typeof(string) });
		PredicateRegistry.Add<Block>("TileOverlay", null, null, new Type[1] { typeof(string) });
		predicateUI = PredicateRegistry.Add<Block>("UI", null, null, new Type[1] { typeof(string) });
		predicateUIOpaque = PredicateRegistry.Add<Block>("UI Opaque");
		predicateThen = PredicateRegistry.Add<Block>("Meta.Then");
		predicateWait = PredicateRegistry.Add<Block>("Meta.Wait", null, (Block b) => b.Wait);
		predicateWaitTime = PredicateRegistry.Add<Block>("Meta.WaitTime", null, (Block b) => b.WaitTime, new Type[1] { typeof(float) }, new string[1] { "Seconds" });
		PredicateRegistry.Add<Block>("Meta.RandomWaitTime", (Block b) => b.RandomWaitTimeSensor, (Block b) => b.RandomWaitTime, new Type[1] { typeof(float) }, new string[1] { "Seconds" });
		PredicateRegistry.Add<Block>("Meta.StopScriptsModelForTime", null, (Block b) => b.StopScriptsModelForTime, new Type[1] { typeof(float) });
		PredicateRegistry.Add<Block>("Meta.StopScriptsModel", null, (Block b) => b.StopScriptsModel);
		PredicateRegistry.Add<Block>("Meta.StopScriptsBlock", null, (Block b) => b.StopScriptsBlock);
		predicateStop = PredicateRegistry.Add<Block>("Meta.Stop");
		PredicateRegistry.Add<Block>("Meta.LockInput", null, (Block b) => b.LockInput);
		predicateHideTileRow = PredicateRegistry.Add<Block>("Meta.HideTileRow", null, (Block b) => b.IgnoreAction);
		predicateHideNextTile = PredicateRegistry.Add<Block>("Meta.HideNextTile", (Block b) => b.IgnoreSensor, (Block b) => b.IgnoreAction);
		PredicateRegistry.Add<Block>("Meta.TileRowHint", null, (Block b) => b.IgnoreAction, new Type[1] { typeof(string) }, new string[1] { "Hint text" });
		PredicateRegistry.Add<Block>("Meta.TestSensor", (Block b) => b.TestSensor, null, new Type[2]
		{
			typeof(string),
			typeof(string)
		}, new string[2] { "Sensor Name", "Data" });
		PredicateRegistry.Add<Block>("Meta.TestAction", null, (Block b) => b.TestAction, new Type[2]
		{
			typeof(string),
			typeof(string)
		}, new string[2] { "Action Name", "Data" });
		PredicateRegistry.Add<Block>("Meta.TestObjective", (Block b) => b.IsTestObjectiveDone, (Block b) => b.SetTestObjectiveDone, new Type[1] { typeof(string) }, new string[1] { "Objective name" });
		predicateSectionIndex = PredicateRegistry.Add<Block>("Section.SectionIndex", null, (Block b) => b.IgnoreAction, new Type[1] { typeof(int) }, new string[1] { "Section Index" });
		predicateIsTreasure = PredicateRegistry.Add<Block>("Block.IsTreasure", null, (Block b) => b.SetAsTreasureModel, new Type[1] { typeof(int) }, new string[1] { "Counter Index" });
		predicateIsTreasureForTag = PredicateRegistry.Add<Block>("Block.IsTreasureForTag", null, (Block b) => b.SetAsTreasureModelTag, new Type[2]
		{
			typeof(string),
			typeof(int)
		}, new string[2] { "Tag name", "Counter Index" });
		predicateIsPickup = PredicateRegistry.Add<Block>("Block.IsPickup", null, (Block b) => b.SetAsPickupModel);
		predicateIsPickupForTag = PredicateRegistry.Add<Block>("Block.IsPickupForTag", null, (Block b) => b.SetAsPickupModelTag, new Type[1] { typeof(string) }, new string[1] { "Tag name" });
		PredicateRegistry.Add<Block>("Block.RespawnPickup", null, (Block b) => b.RespawnPickup);
		PredicateRegistry.Add<Block>("Block.OnCollect", (Block b) => b.OnCollect, (Block b) => b.ForceCollectPickup);
		PredicateRegistry.Add<Block>("Block.OnCollectByTag", (Block b) => b.OnCollectByTag, (Block b) => b.ForceCollectPickup, new Type[1] { typeof(string) }, new string[1] { "Tag name" });
		PredicateRegistry.Add<Block>("Block.SetAsTreasureBlockIcon", null, (Block b) => b.SetAsTreasureBlockIcon, new Type[1] { typeof(int) }, new string[1] { "Counter Index" });
		PredicateRegistry.Add<Block>("Block.SetAsTreasureTextureIcon", null, (Block b) => b.SetAsTreasureTextureIcon, new Type[1] { typeof(int) }, new string[1] { "Counter Index" });
		PredicateRegistry.Add<Block>("Block.SetAsCounterUIBlockIcon", null, (Block b) => b.SetAsCounterUIBlockIcon, new Type[1] { typeof(int) }, new string[1] { "Counter Index" });
		PredicateRegistry.Add<Block>("Block.SetAsCounterUITextureIcon", null, (Block b) => b.SetAsCounterUITextureIcon, new Type[1] { typeof(int) }, new string[1] { "Counter Index" });
		PredicateRegistry.Add<Block>("Block.SetAsTimerUIBlockIcon", null, (Block b) => b.SetAsTimerUIBlockIcon, new Type[1] { typeof(int) }, new string[1] { "Timer Index" });
		PredicateRegistry.Add<Block>("Block.SetAsTimerUITextureIcon", null, (Block b) => b.SetAsTimerUITextureIcon, new Type[1] { typeof(int) }, new string[1] { "Timer Index" });
		PredicateRegistry.Add<Block>("Block.SetAsGaugeUIBlockIcon", null, (Block b) => b.SetAsGaugeUIBlockIcon, new Type[1] { typeof(int) }, new string[1] { "Gauge Index" });
		PredicateRegistry.Add<Block>("Block.SetAsGaugeUITextureIcon", null, (Block b) => b.SetAsGaugeUITextureIcon, new Type[1] { typeof(int) }, new string[1] { "Gauge Index" });
		predicateWithinWater = PredicateRegistry.Add<Block>("Water.IsWithin", (Block b) => b.IsWithinWater);
		predicateModelWithinWater = PredicateRegistry.Add<Block>("Water.IsWithinModel", (Block b) => b.IsAnyBlockInModelWithinWater);
		PredicateRegistry.Add<Block>("Water.IsWithinChunk", (Block b) => b.IsAnyBlockInChunkWithinWater);
		PredicateRegistry.Add<Block>("Block.IsSpeedLess", (Block b) => b.IsSlowerThan, null, new Type[1] { typeof(float) });
		PredicateRegistry.Add<Block>("Block.IsSpeedMore", (Block b) => b.IsFasterThan, null, new Type[1] { typeof(float) });
		predicateWithinTaggedWater = PredicateRegistry.Add<Block>("Block.WithinTaggedWater", (Block b) => b.WithinTaggedWater, null, new Type[1] { typeof(string) }, new string[1] { "Tag" });
		PredicateRegistry.Add<Block>("Block.WithinTaggedWaterChunk", (Block b) => b.WithinTaggedWaterChunk, null, new Type[1] { typeof(string) }, new string[1] { "Tag" });
		predicateModelWithinTaggedWater = PredicateRegistry.Add<Block>("Block.WithinTaggedWaterModel", (Block b) => b.WithinTaggedWaterModel, null, new Type[1] { typeof(string) }, new string[1] { "Tag" });
		PredicateRegistry.Add<Block>("Block.BumpTarget");
		predicateLocked = PredicateRegistry.Add<Block>("Block.Locked", null, (Block b) => b.IgnoreAction);
		predicateUnlocked = PredicateRegistry.Add<Block>("Block.Unlocked", null, (Block b) => b.IgnoreAction);
		PredicateRegistry.Add<Block>("Block.Win", null, (Block b) => b.Win);
		PredicateRegistry.Add<Block>("Block.IncreaseCounter", null, (Block b) => b.IncreaseCounter, new Type[1] { typeof(string) }, new string[1] { "Counter name" });
		PredicateRegistry.Add<Block>("Block.DecreaseCounter", null, (Block b) => b.DecreaseCounter, new Type[1] { typeof(string) }, new string[1] { "Counter name" });
		PredicateRegistry.Add<Block>("Block.CounterEquals", (Block b) => b.CounterEquals, (Block b) => b.SetCounter, new Type[2]
		{
			typeof(string),
			typeof(string)
		}, new string[2] { "Counter name", "Value" });
		PredicateRegistry.Add<Block>("Block.Target", null, (Block b) => b.Target);
		PredicateRegistry.Add<Block>("Block.Inventory", null, (Block b) => b.IgnoreAction);
		PredicateRegistry.Add<Block>("Block.VisualizeReward", null, (Block b) => b.VisualizeReward, new Type[2]
		{
			typeof(string),
			typeof(int)
		});
		PredicateRegistry.Add<Block>("Block.PlaySound", null, (Block b) => b.PlaySound, new Type[5]
		{
			typeof(string),
			typeof(string),
			typeof(string),
			typeof(float),
			typeof(float)
		});
		predicatePlaySoundDurational = PredicateRegistry.Add<Block>("Block.PlaySoundDurational", (Block b) => b.SoundSensor, (Block b) => b.PlaySoundDurational, new Type[5]
		{
			typeof(string),
			typeof(string),
			typeof(string),
			typeof(float),
			typeof(float)
		}, new string[2] { "SFX name", "Location" });
		PredicateRegistry.Add<Block>("Block.Mute", null, (Block b) => b.Mute);
		PredicateRegistry.Add<Block>("Block.MuteModel", null, (Block b) => b.MuteModel);
		PredicateRegistry.Add<Block>("Block.PlayVfxDurational", null, (Block b) => b.PlayVfxDurational, new Type[4]
		{
			typeof(string),
			typeof(float),
			typeof(int),
			typeof(string)
		});
		PredicateRegistry.Add<Block>("Block.TeleportToTag", null, (Block b) => b.TeleportToTag, new Type[5]
		{
			typeof(string),
			typeof(int),
			typeof(int),
			typeof(int),
			typeof(int)
		}, new string[5] { "Tag name", "Reset angle", "Reset velocity", "Reset ang vel", "Find free" });
		PredicateRegistry.Add<Block>("Block.TagProximityCheck", (Block b) => b.TagProximityCheck, null, new Type[3]
		{
			typeof(string),
			typeof(float),
			typeof(int)
		}, new string[3] { "Tag name", "Distance", "Trigger below" });
		PredicateRegistry.Add<Block>("Block.TagVisibilityCheck", (Block b) => b.TagVisibilityCheck, null, new Type[2]
		{
			typeof(string),
			typeof(int)
		}, new string[2] { "Tag name", "Trigger invisible" });
		PredicateRegistry.Add<Block>("Block.TagVisibilityCheckModel", (Block b) => b.TagVisibilityCheckModel, null, new Type[2]
		{
			typeof(string),
			typeof(int)
		}, new string[2] { "Tag name", "Trigger invisible" });
		PredicateRegistry.Add<Block>("Block.TagFrustumCheck", (Block b) => b.TagFrustumCheck, null, new Type[5]
		{
			typeof(string),
			typeof(int),
			typeof(float),
			typeof(float),
			typeof(Vector3)
		});
		predicateAnimate = PredicateRegistry.Add<Block>("Block.Animate", null, (Block b) => b.Animate, new Type[2]
		{
			typeof(int),
			typeof(string)
		});
		predicateTutorialHelpTextAction = PredicateRegistry.Add<Block>("Block.TutorialHelpTextAction", (Block b) => b.IgnoreSensor, (Block b) => b.IgnoreAction, new Type[10]
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
		}, new string[10]
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
		});
		predicateTutorialCreateBlockHint = PredicateRegistry.Add<Block>("Block.TutorialCreateBlockHint", null, (Block b) => b.TutorialCreateBlockHint, new Type[11]
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
		}, new string[11]
		{
			"World target position", "World target normal", "Camera euler angles", "Camera distance", "Rotate before scale", "Paint before pose", "Texture before pose", "Auto angles filter", "Use two-finger scale", "Use two-finger move",
			"Add default tiles"
		});
		predicateTutorialRemoveBlockHint = PredicateRegistry.Add<Block>("Block.TutorialRemoveBlockHint", null, (Block b) => b.IgnoreAction, new Type[3]
		{
			typeof(int),
			typeof(Vector3),
			typeof(Vector3)
		}, new string[3] { "Block index", "Camera position", "Camera lookat position" });
		predicateTutorialAutoAddBlock = PredicateRegistry.Add<Block>("Block.TutorialAutoAddBlock", null, (Block b) => b.IgnoreAction, new Type[1] { typeof(float) }, new string[1] { "Wait time" });
		predicateTutorialPaintExistingBlock = PredicateRegistry.Add<Block>("Block.TutorialPaintExistingBlock", null, (Block b) => b.TutorialPaintExistingBlock, new Type[4]
		{
			typeof(int),
			typeof(string),
			typeof(Vector3),
			typeof(Vector3)
		}, new string[4] { "Block index", "Paint name", "Camera position", "Camera lookat position" });
		predicateTutorialTextureExistingBlock = PredicateRegistry.Add<Block>("Block.TutorialTextureExistingBlock", null, (Block b) => b.TutorialTextureExistingBlock, new Type[4]
		{
			typeof(int),
			typeof(string),
			typeof(Vector3),
			typeof(Vector3)
		}, new string[4] { "Block index", "Texture name", "Camera position", "Camera lookat position" });
		predicateTutorialRotateExistingBlock = PredicateRegistry.Add<Block>("Block.TutorialRotateExistingBlock", null, (Block b) => b.TutorialRotateExistingBlock, new Type[4]
		{
			typeof(int),
			typeof(Vector3),
			typeof(Vector3),
			typeof(Vector3)
		}, new string[4] { "Block index", "Angles", "Camera position", "Camera lookat position" });
		predicateTutorialOperationPose = PredicateRegistry.Add<Block>("Block.TutorialOperationPose", null, (Block b) => b.IgnoreAction, new Type[3]
		{
			typeof(string),
			typeof(int),
			typeof(string)
		}, new string[3] { "Tutorial State", "Mesh index", "Pose Name" });
		predicateTutorialMoveBlock = PredicateRegistry.Add<Block>("Block.TutorialMoveBlock", null, (Block b) => b.IgnoreAction, new Type[1] { typeof(Vector3) }, new string[1] { "Position" });
		predicateTutorialMoveModel = PredicateRegistry.Add<Block>("Block.TutorialMoveModel", null, (Block b) => b.IgnoreAction, new Type[1] { typeof(Vector3) }, new string[1] { "Position" });
		predicateTutorialHideInBuildMode = PredicateRegistry.Add<Block>("Block.TutorialHideInBuildMode", null, (Block b) => b.IgnoreAction);
		predicateTiltLeftRight = PredicateRegistry.Add<Block>("Block.DeviceTilt", (Block b) => b.IsTiltedLeftRight, null, new Type[2]
		{
			typeof(float),
			typeof(float)
		});
		PredicateRegistry.Add<Block>("Block.TiltCamera", null, (Block b) => b.TiltCamera, new Type[1] { typeof(float) });
		predicateTiltFrontBack = PredicateRegistry.Add<Block>("Block.DeviceTiltFrontBack", (Block b) => b.IsTiltedFrontBack, null, new Type[2]
		{
			typeof(float),
			typeof(float)
		});
		predicateButton = PredicateRegistry.Add<Block>("Block.ButtonInput", (Block b) => b.IsReceivingButtonInput, null, new Type[1] { typeof(string) });
		predicateSendSignal = PredicateRegistry.Add<Block>("Block.SendSignal", (Block b) => b.IsSendingSignal, (Block b) => b.SendSignal, new Type[2]
		{
			typeof(int),
			typeof(float)
		}, new string[2] { "Signal index", "Signal strength" });
		predicateSendSignalModel = PredicateRegistry.Add<Block>("Block.SendSignalModel", (Block b) => b.IsSendingSignalModel, (Block b) => b.SendSignalModel, new Type[2]
		{
			typeof(int),
			typeof(float)
		}, new string[2] { "Signal index", "Signal strength" });
		predicateSendCustomSignal = PredicateRegistry.Add<Block>("Block.SendCustomSignal", (Block b) => b.IsSendingCustomSignal, (Block b) => b.SendCustomSignal, new Type[2]
		{
			typeof(string),
			typeof(float)
		}, new string[2] { "Signal name", "Signal strength" });
		predicateSendSignal.canHaveOverlay = true;
		predicateSendCustomSignalModel = PredicateRegistry.Add<Block>("Block.SendCustomSignalModel", (Block b) => b.IsSendingCustomSignalModel, (Block b) => b.SendCustomSignalModel, new Type[2]
		{
			typeof(string),
			typeof(float)
		}, new string[2] { "Signal name", "Signal strength" });
		predicateSendCustomSignalModel.canHaveOverlay = true;
		PredicateRegistry.Add<Block>("Block.GameStart", (Block b) => b.IsGameStart);
		predicateDPadHorizontal = PredicateRegistry.Add<Block>("Block.DPadHorizontal", (Block b) => b.IsDPadHorizontal, null, new Type[2]
		{
			typeof(float),
			typeof(string)
		}, new string[2] { "Sign", "DPad Name" });
		predicateDPadVertical = PredicateRegistry.Add<Block>("Block.DPadVertical", (Block b) => b.IsDPadVertical, null, new Type[2]
		{
			typeof(float),
			typeof(string)
		}, new string[2] { "Sign", "DPad Name" });
		predicateDPadMoved = PredicateRegistry.Add<Block>("Block.DPadMoved", (Block b) => b.IsDPadMoved, null, new Type[1] { typeof(string) }, new string[1] { "DPad Name" });
		predicateTapBlock = PredicateRegistry.Add<Block>("Block.Tap", (Block b) => b.IsTappingBlock);
		predicateTapModel = PredicateRegistry.Add<Block>("Block.TapModel", (Block b) => b.IsTappingAnyBlockInModel);
		predicateTapChunk = PredicateRegistry.Add<Block>("Block.TapChunk", (Block b) => b.IsTappingAnyBlockInChunk);
		predicateTapHoldBlock = PredicateRegistry.Add<Block>("Block.TapHold", (Block b) => b.IsTapHoldingBlock);
		predicateTapHoldModel = PredicateRegistry.Add<Block>("Block.TapHoldModel", (Block b) => b.IsTapHoldingAnyBlockInModel);
		PredicateRegistry.Add<Block>("Block.GlueOnContact", (Block b) => b.IsGlueOnContact, (Block b) => b.GlueOnContact, new Type[1] { typeof(float) }, new string[1] { "Size Factor" });
		PredicateRegistry.Add<Block>("Block.GlueOnContactChunk", null, (Block b) => b.GlueOnContactChunk, new Type[1] { typeof(float) }, new string[1] { "Size Factor" });
		PredicateRegistry.Add<Block>("Block.AllowGlueOnContact", (Block b) => b.IsAllowGlueOnContact, (Block b) => b.AllowGlueOnContact, new Type[1] { typeof(int) });
		PredicateRegistry.Add<Block>("Block.ReleaseGlueOnContact", null, (Block b) => b.ReleaseGlueOnContact);
		predicateBump = PredicateRegistry.Add<Block>("Block.Bump", (Block b) => b.IsBumping, null, new Type[1] { typeof(string) });
		predicateBumpModel = PredicateRegistry.Add<Block>("Block.BumpModel", (Block b) => b.IsBumpingAnyBlockInModel, null, new Type[1] { typeof(string) });
		predicateImpact = PredicateRegistry.Add<Block>("Block.OnImpact", (Block b) => b.OnImpact);
		predicateImpactModel = PredicateRegistry.Add<Block>("Block.OnImpactModel", (Block b) => b.OnImpactModel);
		predicateParticleImpact = PredicateRegistry.Add<Block>("Block.OnParticleImpact", (Block b) => b.OnParticleImpact, null, new Type[1] { typeof(int) });
		predicateParticleImpactModel = PredicateRegistry.Add<Block>("Block.OnParticleImpactModel", (Block b) => b.OnParticleImpactModel, null, new Type[1] { typeof(int) });
		predicateBumpChunk = PredicateRegistry.Add<Block>("Block.BumpChunk", (Block b) => b.IsBumpingAnyBlockInChunk, null, new Type[1] { typeof(string) });
		predicateTaggedBump = PredicateRegistry.Add<Block>("Block.TaggedBump", (Block b) => b.IsBumpingTag, null, new Type[1] { typeof(string) }, new string[1] { "Tag name" });
		predicateTaggedBumpModel = PredicateRegistry.Add<Block>("Block.TaggedBumpModel", (Block b) => b.IsBumpingAnyTaggedBlockInModel, null, new Type[1] { typeof(string) }, new string[1] { "Tag name" });
		predicateTaggedBumpChunk = PredicateRegistry.Add<Block>("Block.TaggedBumpChunk", (Block b) => b.IsBumpingAnyTaggedBlockInChunk, null, new Type[1] { typeof(string) }, new string[1] { "Tag name" });
		predicatePulledByMagnet = PredicateRegistry.Add<Block>("Block.PulledByMagnet", (Block b) => b.IsPulledByMagnet);
		predicatePulledByMagnetModel = PredicateRegistry.Add<Block>("Block.PulledByMagnetModel", (Block b) => b.IsPulledByMagnetModel);
		predicatePushedByMagnet = PredicateRegistry.Add<Block>("Block.PushedByMagnet", (Block b) => b.IsPushedByMagnet);
		predicatePushedByMagnetModel = PredicateRegistry.Add<Block>("Block.PushedByMagnetModel", (Block b) => b.IsPushedByMagnetModel);
		PredicateRegistry.Add<Block>("Block.HitByBlocksterHandAttachment", (Block b) => b.IsHitByBlocksterHandAttachment);
		PredicateRegistry.Add<Block>("Block.HitByTaggedBlocksterHandAttachment", (Block b) => b.IsHitByTaggedBlocksterHandAttachment, null, new Type[1] { typeof(string) });
		PredicateRegistry.Add<Block>("Block.ModelHitByBlocksterHandAttachment", (Block b) => b.IsHitByBlocksterHandAttachmentModel);
		PredicateRegistry.Add<Block>("Block.ModelHitByTaggedBlocksterHandAttachment", (Block b) => b.IsHitByTaggedBlocksterHandAttachmentModel, null, new Type[1] { typeof(string) });
		PredicateRegistry.Add<Block>("Block.HitByProjectile", (Block b) => b.IsHitByProjectile);
		PredicateRegistry.Add<Block>("Block.TaggedHitByProjectile", (Block b) => b.IsHitByTaggedProjectile, null, new Type[1] { typeof(string) });
		PredicateRegistry.Add<Block>("Block.HitByTaggedProjectileModel", (Block b) => b.IsHitByTaggedProjectileModel, null, new Type[1] { typeof(string) });
		PredicateRegistry.Add<Block>("Block.HitByTaggedProjectileChunk", (Block b) => b.IsHitByTaggedProjectileChunk, null, new Type[1] { typeof(string) });
		PredicateRegistry.Add<Block>("Block.HitByProjectileModel", (Block b) => b.IsHitByProjectileModel);
		PredicateRegistry.Add<Block>("Block.HitByProjectileChunk", (Block b) => b.IsHitByProjectileChunk);
		PredicateRegistry.Add<Block>("Laser.HitByBeam", (Block b) => b.IsHitByPulseOrBeam);
		PredicateRegistry.Add<Block>("Laser.HitByPulse", (Block b) => b.IsHitByPulseOrBeam);
		PredicateRegistry.Add<Block>("Laser.TaggedHitByBeam", (Block b) => b.IsHitByTaggedPulseOrBeam, null, new Type[1] { typeof(string) });
		PredicateRegistry.Add<Block>("Block.HitByLaserModel", (Block b) => b.IsHitByLaserModel);
		PredicateRegistry.Add<Block>("Block.HitByLaserChunk", (Block b) => b.IsHitByLaserChunk);
		PredicateRegistry.Add<Block>("Block.HitByTaggedLaserModel", (Block b) => b.IsHitByTaggedLaserModel, null, new Type[1] { typeof(string) });
		PredicateRegistry.Add<Block>("Block.HitByTaggedLaserChunk", (Block b) => b.IsHitByTaggedLaserChunk, null, new Type[1] { typeof(string) });
		PredicateRegistry.Add<Block>("Block.HitByArrow", (Block b) => b.IsHitByArrow);
		PredicateRegistry.Add<Block>("Block.HitByTaggedArrow", (Block b) => b.IsHitByTaggedArrow, null, new Type[1] { typeof(string) });
		PredicateRegistry.Add<Block>("Block.ModelHitByArrow", (Block b) => b.IsHitByArrowModel);
		PredicateRegistry.Add<Block>("Block.ModelHitByTaggedArrow", (Block b) => b.IsHitByTaggedArrowModel, null, new Type[1] { typeof(string) });
		PredicateRegistry.Add<Block>("Block.Teleported", (Block b) => b.Teleported);
		PredicateRegistry.Add<Block>("Block.Trigger", (Block b) => b.IsTriggering);
		PredicateRegistry.Add<Block>("Block.TriggerModel", (Block b) => b.IsAnyBlockInModelTriggering);
		PredicateRegistry.Add<Block>("Block.TriggerChunk", (Block b) => b.IsAnyBlockInChunkTriggering);
		PredicateRegistry.Add<Block>("Block.SetTrigger", null, (Block b) => b.SetTrigger);
		PredicateRegistry.Add<Block>("Block.SetTriggerModel", null, (Block b) => b.SetEveryBlockInModelAsTrigger);
		PredicateRegistry.Add<Block>("Block.SetTriggerChunk", null, (Block b) => b.SetEveryBlockInChunkAsTrigger);
		predicateFirstFrame = PredicateRegistry.Add<Block>("Block.FirstFrame", (Block b) => b.IsFirstFrame);
		predicateFreeze = PredicateRegistry.Add<Block>("Block.Fixed", (Block b) => b.IsFrozen, (Block b) => b.Freeze);
		predicateUnfreeze = PredicateRegistry.Add<Block>("Block.Unfreeze", (Block b) => b.IsNotFrozen, (Block b) => b.Unfreeze);
		PredicateRegistry.Add<Block>("Block.Phantom", (Block b) => b.IsPhantom, (Block b) => b.SetPhantom);
		PredicateRegistry.Add<Block>("Block.Unphantom", (Block b) => b.IsNotPhantom, (Block b) => b.SetUnphantom);
		PredicateRegistry.Add<Block>("Block.PhantomModel", (Block b) => b.IsPhantomModel, (Block b) => b.SetPhantomModel);
		PredicateRegistry.Add<Block>("Block.UnphantomModel", (Block b) => b.IsNotPhantomModel, (Block b) => b.SetUnphantomModel);
		PredicateRegistry.Add<Block>("Block.Clone", null, (Block b) => delegate
		{
			Block block = new Block(b.tiles);
			BWSceneManager.AddTempBlock(block);
			Vector3 position = b.goT.position;
			position.x += 2f;
			block.goT.position = position;
			return TileResultCode.True;
		});
		BlockItemEntry blockItemEntry = new BlockItemEntry();
		blockItemEntry.argumentPatterns = new string[0];
		blockItemEntry.buildPaneTab = "Actions & Scripting";
		blockItemEntry.item = new BlockItem(9876, "clone", "Clone", "Clone", "Block.Clone", new object[0], "Yellow/Danger", "Yellow", RarityLevelEnum.common);
		BlockItemsRegistry.AddBlockItem(blockItemEntry);
		PredicateRegistry.Add<Block>("Block.Explode", (Block b) => b.IsExploded, (Block b) => b.Explode, new Type[1] { typeof(float) }, new string[1] { "Strength Multiplier" });
		PredicateRegistry.Add<Block>("Block.ExplodeLocal", (Block b) => b.HitByExplosion, (Block b) => b.ExplodeLocal, new Type[1] { typeof(float) }, new string[1] { "Radius" });
		PredicateRegistry.Add<Block>("Block.SmashLocal", null, (Block b) => b.SmashLocal, new Type[1] { typeof(float) }, new string[1] { "Radius" });
		PredicateRegistry.Add<Block>("Block.Detach", (Block b) => b.IsDetached, (Block b) => b.Detach);
		PredicateRegistry.Add<Block>("Block.ExplodeTag", (Block b) => b.IsExploded, (Block b) => b.ExplodeTag, new Type[2]
		{
			typeof(string),
			typeof(float)
		}, new string[2] { "Tag", "Radius" });
		PredicateRegistry.Add<Block>("Block.Invincible", (Block b) => b.IsInvincible, (Block b) => b.SetInvincible);
		PredicateRegistry.Add<Block>("Block.InvincibleModel", (Block b) => b.IsInvincibleModel, (Block b) => b.SetInvincibleModel);
		PredicateRegistry.Add<Block>("Prop.Attacking", (Block b) => b.IsWeaponAttacking);
		PredicateRegistry.Add<Block>("Prop.Blocking", (Block b) => b.IsShieldBlocking);
		PredicateRegistry.Add<Block>("Block.CameraFollow", null, (Block b) => b.CameraFollow, new Type[4]
		{
			typeof(float),
			typeof(float),
			typeof(float),
			typeof(Vector3)
		});
		PredicateRegistry.Add<Block>("Block.CameraFollow2D", null, (Block b) => b.CameraFollow2D, new Type[2]
		{
			typeof(float),
			typeof(float)
		});
		predicateCamFollow = PredicateRegistry.Add<Block>("Block.CameraFollowLookToward", null, (Block b) => b.CameraFollowLookToward, new Type[3]
		{
			typeof(float),
			typeof(float),
			typeof(float)
		}, new string[3] { "Vel. resp.", "Smoothness", "Angle" });
		PredicateRegistry.Add<Block>("Block.CameraFollowLookTowardTag", null, (Block b) => b.CameraFollowLookTowardTag, new Type[3]
		{
			typeof(string),
			typeof(float),
			typeof(float)
		}, new string[3] { "Tag Name", "Vel. resp.", "Smoothness" });
		PredicateRegistry.Add<Block>("Block.CameraFollowThirdPersonPlatform", null, (Block b) => b.CameraFollowThirdPersonPlatform);
		PredicateRegistry.Add<Block>("Block.CameraMoveTo", null, (Block b) => b.CameraMoveTo, new Type[1] { typeof(float) });
		PredicateRegistry.Add<Block>("Block.CameraLookAt", null, (Block b) => b.CameraLookAt, new Type[1] { typeof(float) });
		PredicateRegistry.Add<Block>("Block.SetTargetCameraAngle", null, (Block b) => b.SetTargetCameraAngle, new Type[1] { typeof(float) });
		PredicateRegistry.Add<Block>("Block.SetVerticalDistanceOffsetFactor", null, (Block b) => b.SetVerticalDistanceOffsetFactor, new Type[1] { typeof(float) });
		PredicateRegistry.Add<Block>("Block.SetTargetFollowDistanceMultiplier", null, (Block b) => b.SetTargetFollowDistanceMultiplier, new Type[1] { typeof(float) });
		PredicateRegistry.Add<Block>("Block.CameraToNamedPose", null, (Block b) => b.CameraToNamedPose, new Type[5]
		{
			typeof(string),
			typeof(float),
			typeof(float),
			typeof(float),
			typeof(int)
		}, new string[5] { "Pose Name", "Move smoothness", "Aim smoothness", "Direction dist", "Move only" });
		PredicateRegistry.Add<Block>("Block.CameraSpeedFoV", null, (Block b) => b.CameraSpeedFoV, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockCharacter>("Block.StartFirstPersonCamera", (Block b) => b.IsFirstPersonBlock, (Block b) => b.FirstPersonCamera, new Type[1] { typeof(int) });
		PredicateRegistry.Add<Block>("Block.HUDReticle", null, (Block b) => b.SetHudReticle, new Type[1] { typeof(int) });
		predicateSpeak = PredicateRegistry.Add<Block>("Block.Speak", null, (Block b) => b.Speak, new Type[1] { typeof(string) }, new string[1] { "Text" });
		PredicateRegistry.Add<Block>("Block.SpeakNonDurational", null, (Block b) => b.SpeakNonDurational, new Type[2]
		{
			typeof(string),
			typeof(int)
		});
		PredicateRegistry.Add<Block>("Block.ShowTextWindow", null, (Block b) => b.ShowTextWindow, new Type[4]
		{
			typeof(string),
			typeof(Vector3),
			typeof(float),
			typeof(string)
		}, new string[4] { "Text", "Position", "Width", "Buttons" });
		PredicateRegistry.Add<Block>("Block.BreakOff", (Block b) => b.IsBrokenOff, (Block b) => b.BreakOff);
		PredicateRegistry.Add<Block>("Block.Vanish", (Block b) => b.IsVanished, (Block b) => b.Vanish);
		PredicateRegistry.Add<Block>("Block.Appear", null, (Block b) => b.Appear);
		predicateVanishBlock = PredicateRegistry.Add<Block>("Block.VanishBlock", (Block b) => b.IsVanished, (Block b) => b.VanishBlock, new Type[1] { typeof(int) }, new string[1] { "Animate" });
		PredicateRegistry.Add<Block>("Block.AppearBlock", (Block b) => b.IsAppeared, (Block b) => b.AppearBlock, new Type[1] { typeof(int) }, new string[1] { "Animate" });
		predicateVanishModel = PredicateRegistry.Add<Block>("Block.VanishModel", (Block b) => b.IsVanished, (Block b) => b.VanishModel, new Type[1] { typeof(int) }, new string[1] { "Animate" });
		PredicateRegistry.Add<Block>("Block.AppearModel", (Block b) => b.IsAppeared, (Block b) => b.AppearModel, new Type[1] { typeof(int) }, new string[1] { "Animate" });
		PredicateRegistry.Add<Block>("Block.VanishModelForever", null, (Block b) => b.VanishModelForever, new Type[2]
		{
			typeof(int),
			typeof(float)
		}, new string[2] { "Animate", "Per block delay" });
		predicateLevitate = PredicateRegistry.Add<Block>("Block.LevitateAnimation", null, (Block b) => b.LevitateAnimation);
		PredicateRegistry.Add<Block>("Block.SetBackgroundMusic", null, (Block b) => b.SetBackgroundMusic, new Type[1] { typeof(string) }, new string[1] { "Music name" });
		PredicateRegistry.Add<Block>("Meta.TileBackground");
		PredicateRegistry.Add<Block>("Meta.TileBackgroundWithLabel");
		PredicateRegistry.Add<Block>("Meta.ButtonBackground");
		PredicateRegistry.Add<Block>("Block.PullLockBlock", (Block b) => b.IsPullLockedSensor, (Block b) => b.PullLockBlock);
		PredicateRegistry.Add<Block>("Block.PullLockChunk", (Block b) => b.IsPullLockedSensor, (Block b) => b.PullLockChunk);
		PredicateRegistry.Add<Block>("Block.PullLockModel", (Block b) => b.IsPullLockedSensor, (Block b) => b.PullLockModel);
		predicateSetSpawnPoint = PredicateRegistry.Add<Block>("Block.SetSpawnpoint", null, (Block b) => b.SetSpawnpoint, new Type[1] { typeof(int) }, new string[1] { "Player Index" });
		predicateSetActiveCheckpoint = PredicateRegistry.Add<Block>("Block.SetActiveCheckpoint", null, (Block b) => b.SetActiveCheckpoint, new Type[1] { typeof(int) }, new string[1] { "Player Index" });
		predicateSpawn = PredicateRegistry.Add<Block>("Block.Spawn", null, (Block b) => b.Spawn, new Type[2]
		{
			typeof(int),
			typeof(float)
		}, new string[2] { "Player Index", "Duration" });
		PredicateRegistry.Add<Block>("Block.SetBuoyancy", null, (Block b) => b.SetBuoyancy, new Type[1] { typeof(float) });
		PredicateRegistry.Add<Block>("Block.SetMass", null, (Block b) => b.SetMass, new Type[1] { typeof(float) });
		PredicateRegistry.Add<Block>("Block.SetFriction", null, (Block b) => b.SetFriction, new Type[1] { typeof(float) });
		PredicateRegistry.Add<Block>("Block.SetBounce", null, (Block b) => b.SetBounce, new Type[1] { typeof(float) });
		PredicateRegistry.Add<Block>("AnalogOp.Ceil", (Block b) => b.AnalogCeil);
		PredicateRegistry.Add<Block>("AnalogOp.Floor", (Block b) => b.AnalogFloor);
		PredicateRegistry.Add<Block>("AnalogOp.Round", (Block b) => b.AnalogRound);
		PredicateRegistry.Add<Block>("AnalogOp.Min", (Block b) => b.AnalogMin, null, new Type[1] { typeof(float) }, new string[1] { "Value" });
		PredicateRegistry.Add<Block>("AnalogOp.Max", (Block b) => b.AnalogMax, null, new Type[1] { typeof(float) }, new string[1] { "Value" });
		PredicateRegistry.Add<Block>("AnalogOp.Clamp", (Block b) => b.AnalogClamp, null, new Type[2]
		{
			typeof(float),
			typeof(float)
		}, new string[2] { "Lower Value", "Upper Value" });
		defaultExtraTiles["DNO Gas Canister"] = new List<List<Tile>>
		{
			new List<Tile>
			{
				new Tile(new GAF("Laser.HitByBeam")),
				ThenTile(),
				new Tile(new GAF("Block.ExplodeLocal", 8f))
			},
			EmptyTileRow()
		};
		foreach (Mod mod in ModLoader.mods)
		{
			mod.Register(RegisterType.PREDICATES);
		}
		foreach (PredicateEntry value in PredicateEntryRegistry.GetPredicateEntries().Values)
		{
			PredicateRegistry.AddTyped(value.blockType, value.id, value.sensorConstructor, value.actionConstructor, value.argTypes, value.argNames);
		}
		SetupVariablePredicates();
		SetupVRPredicates();
	}

	public static void SetupVariablePredicates()
	{
		predicateVariableCustomInt = PredicateRegistry.Add<Block>("Variable.CustomInt", null, (Block b) => b.VariableDeclare, new Type[2]
		{
			typeof(string),
			typeof(int)
		}, new string[2] { "Variable name", "Int value" });
		Predicate predicate = PredicateRegistry.Add<Block>("Variable.CustomIntAdd", null, (Block b) => b.VariableAdd, new Type[2]
		{
			typeof(string),
			typeof(int)
		}, new string[2] { "Variable name", "Int value" });
		Predicate predicate2 = PredicateRegistry.Add<Block>("Variable.CustomIntSubtract", null, (Block b) => b.VariableSubtract, new Type[2]
		{
			typeof(string),
			typeof(int)
		}, new string[2] { "Variable name", "Int value" });
		Predicate predicate3 = PredicateRegistry.Add<Block>("Variable.CustomIntMultiply", null, (Block b) => b.VariableMultiply, new Type[2]
		{
			typeof(string),
			typeof(int)
		}, new string[2] { "Block variable name", "Int value" });
		Predicate predicate4 = PredicateRegistry.Add<Block>("Variable.CustomIntDivide", null, (Block b) => b.VariableDivide, new Type[2]
		{
			typeof(string),
			typeof(int)
		}, new string[2] { "Block variable name", "Int value" });
		Predicate predicate5 = PredicateRegistry.Add<Block>("Variable.CustomIntModulus", null, (Block b) => b.VariableModulus, new Type[2]
		{
			typeof(string),
			typeof(int)
		}, new string[2] { "Block variable name", "Int value" });
		Predicate predicate6 = PredicateRegistry.Add<Block>("Variable.CustomIntEquals", (Block b) => b.VariableEquals, (Block b) => b.VariableAssign, new Type[2]
		{
			typeof(string),
			typeof(int)
		}, new string[2] { "Variable name", "Int value" });
		Predicate predicate7 = PredicateRegistry.Add<Block>("Variable.CustomIntNotEquals", (Block b) => b.VariableNotEquals, null, new Type[2]
		{
			typeof(string),
			typeof(int)
		}, new string[2] { "Variable name", "Int value" });
		Predicate predicate8 = PredicateRegistry.Add<Block>("Variable.CustomIntLessThan", (Block b) => b.VariableLessThan, null, new Type[2]
		{
			typeof(string),
			typeof(int)
		}, new string[2] { "Variable name", "Int value" });
		Predicate predicate9 = PredicateRegistry.Add<Block>("Variable.CustomIntMoreThan", (Block b) => b.VariableMoreThan, null, new Type[2]
		{
			typeof(string),
			typeof(int)
		}, new string[2] { "Variable name", "Int value" });
		predicateBlockVariableInt = PredicateRegistry.Add<Block>("BlockVariable.Int", null, (Block b) => b.BlockVariableDeclare, new Type[2]
		{
			typeof(string),
			typeof(int)
		}, new string[2] { "Block variable name", "Int value" });
		Predicate predicate10 = PredicateRegistry.Add<Block>("BlockVariable.IntAdd", null, (Block b) => b.BlockVariableAdd, new Type[2]
		{
			typeof(string),
			typeof(int)
		}, new string[2] { "Block variable name", "Int value" });
		Predicate predicate11 = PredicateRegistry.Add<Block>("BlockVariable.IntSubtract", null, (Block b) => b.BlockVariableSubtract, new Type[2]
		{
			typeof(string),
			typeof(int)
		}, new string[2] { "Block variable name", "Int value" });
		Predicate predicate12 = PredicateRegistry.Add<Block>("BlockVariable.IntMultiply", null, (Block b) => b.BlockVariableMultiply, new Type[2]
		{
			typeof(string),
			typeof(int)
		}, new string[2] { "Block variable name", "Int value" });
		Predicate predicate13 = PredicateRegistry.Add<Block>("BlockVariable.IntDivide", null, (Block b) => b.BlockVariableDivide, new Type[2]
		{
			typeof(string),
			typeof(int)
		}, new string[2] { "Block variable name", "Int value" });
		Predicate predicate14 = PredicateRegistry.Add<Block>("BlockVariable.IntModulus", null, (Block b) => b.BlockVariableModulus, new Type[2]
		{
			typeof(string),
			typeof(int)
		}, new string[2] { "Block variable name", "Int value" });
		Predicate predicate15 = PredicateRegistry.Add<Block>("BlockVariable.IntEquals", (Block b) => b.BlockVariableEquals, (Block b) => b.BlockVariableAssign, new Type[2]
		{
			typeof(string),
			typeof(int)
		}, new string[2] { "Block variable name", "Int value" });
		Predicate predicate16 = PredicateRegistry.Add<Block>("BlockVariable.IntNotEquals", (Block b) => b.BlockVariableNotEquals, null, new Type[2]
		{
			typeof(string),
			typeof(int)
		}, new string[2] { "Block variable name", "Int value" });
		Predicate predicate17 = PredicateRegistry.Add<Block>("BlockVariable.IntLessThan", (Block b) => b.BlockVariableLessThan, null, new Type[2]
		{
			typeof(string),
			typeof(int)
		}, new string[2] { "Block variable name", "Int value" });
		Predicate predicate18 = PredicateRegistry.Add<Block>("BlockVariable.IntMoreThan", (Block b) => b.BlockVariableMoreThan, null, new Type[2]
		{
			typeof(string),
			typeof(int)
		}, new string[2] { "Block variable name", "Int value" });
		Predicate predicate19 = PredicateRegistry.Add<Block>("BlockVariable.IntRandom", null, (Block b) => b.BlockVariableAssignRandom, new Type[2]
		{
			typeof(string),
			typeof(int)
		}, new string[2] { "Block variable name", "Int value" });
		Predicate predicate20 = PredicateRegistry.Add<Block>("BlockVariable.IntSpeed", null, (Block b) => b.BlockVariableAssignSpeed, new Type[2]
		{
			typeof(string),
			typeof(int)
		}, new string[1] { "Block variable name" });
		Predicate predicate21 = PredicateRegistry.Add<Block>("BlockVariable.IntAltitude", null, (Block b) => b.BlockVariableAssignAltitude, new Type[2]
		{
			typeof(string),
			typeof(int)
		}, new string[1] { "Block variable name" });
		predicateBlockVariableIntLoadGlobal = PredicateRegistry.Add<Block>("BlockVariable.IntLoadGlobal", null, (Block b) => b.BlockVariableLoadGlobal, new Type[2]
		{
			typeof(string),
			typeof(string)
		}, new string[2] { "Block variable name", "Global variable name" });
		predicateBlockVariableIntStoreGlobal = PredicateRegistry.Add<Block>("BlockVariable.IntStoreGlobal", null, (Block b) => b.BlockVariableStoreGlobal, new Type[2]
		{
			typeof(string),
			typeof(string)
		}, new string[2] { "Block variable name", "Global variable name" });
		Predicate predicate22 = PredicateRegistry.Add<Block>("BlockVariable.IntAddBV", null, (Block b) => b.BlockVariableAddBV, new Type[2]
		{
			typeof(string),
			typeof(string)
		}, new string[2] { "Block variable name", "Block variable name" });
		Predicate predicate23 = PredicateRegistry.Add<Block>("BlockVariable.IntSubtractBV", null, (Block b) => b.BlockVariableSubtractBV, new Type[2]
		{
			typeof(string),
			typeof(string)
		}, new string[2] { "Block variable name", "Block variable name" });
		Predicate predicate24 = PredicateRegistry.Add<Block>("BlockVariable.IntMultiplyBV", null, (Block b) => b.BlockVariableMultiplyBV, new Type[2]
		{
			typeof(string),
			typeof(string)
		}, new string[2] { "Block variable name", "Block variable name" });
		Predicate predicate25 = PredicateRegistry.Add<Block>("BlockVariable.IntDivideBV", null, (Block b) => b.BlockVariableDivideBV, new Type[2]
		{
			typeof(string),
			typeof(string)
		}, new string[2] { "Block variable name", "Block variable name" });
		Predicate predicate26 = PredicateRegistry.Add<Block>("BlockVariable.IntModulusBV", null, (Block b) => b.BlockVariableModulusBV, new Type[2]
		{
			typeof(string),
			typeof(string)
		}, new string[2] { "Block variable name", "Block variable name" });
		Predicate predicate27 = PredicateRegistry.Add<Block>("BlockVariable.IntEqualsBV", (Block b) => b.BlockVariableEqualsBV, (Block b) => b.BlockVariableAssignBV, new Type[2]
		{
			typeof(string),
			typeof(string)
		}, new string[2] { "Block variable name", "Block variable name" });
		Predicate predicate28 = PredicateRegistry.Add<Block>("BlockVariable.IntNotEqualsBV", (Block b) => b.BlockVariableNotEqualsBV, null, new Type[2]
		{
			typeof(string),
			typeof(string)
		}, new string[2] { "Block variable name", "Block variable name" });
		Predicate predicate29 = PredicateRegistry.Add<Block>("BlockVariable.IntLessThanBV", (Block b) => b.BlockVariableLessThanBV, null, new Type[2]
		{
			typeof(string),
			typeof(string)
		}, new string[2] { "Block variable name", "Block variable name" });
		Predicate predicate30 = PredicateRegistry.Add<Block>("BlockVariable.IntMoreThanBV", (Block b) => b.BlockVariableMoreThanBV, null, new Type[2]
		{
			typeof(string),
			typeof(string)
		}, new string[2] { "Block variable name", "Block variable name" });
		customVariablePredicates = new HashSet<Predicate>();
		variablePredicateParamDefaults = new Dictionary<Predicate, int>();
		customVariablePredicates.Add(predicateVariableCustomInt);
		customVariablePredicates.Add(predicateBlockVariableInt);
		globalVariableOperations = new HashSet<Predicate>();
		globalVariableOperations.Add(predicate);
		globalVariableOperations.Add(predicate2);
		globalVariableOperations.Add(predicate3);
		globalVariableOperations.Add(predicate4);
		globalVariableOperations.Add(predicate5);
		globalVariableOperations.Add(predicate6);
		globalVariableOperations.Add(predicate7);
		globalVariableOperations.Add(predicate8);
		globalVariableOperations.Add(predicate9);
		variablePredicateParamDefaults[predicate] = 1;
		variablePredicateParamDefaults[predicate2] = 1;
		variablePredicateParamDefaults[predicate3] = 2;
		variablePredicateParamDefaults[predicate4] = 2;
		variablePredicateParamDefaults[predicate5] = 2;
		variablePredicateParamDefaults[predicate6] = 0;
		variablePredicateParamDefaults[predicate7] = 0;
		variablePredicateParamDefaults[predicate8] = 0;
		variablePredicateParamDefaults[predicate9] = 0;
		blockVariableOperations = new HashSet<Predicate>();
		blockVariableOperations.Add(predicate10);
		blockVariableOperations.Add(predicate11);
		blockVariableOperations.Add(predicate12);
		blockVariableOperations.Add(predicate13);
		blockVariableOperations.Add(predicate14);
		blockVariableOperations.Add(predicate15);
		blockVariableOperations.Add(predicate16);
		blockVariableOperations.Add(predicate17);
		blockVariableOperations.Add(predicate18);
		blockVariableOperations.Add(predicate19);
		blockVariableOperations.Add(predicate20);
		blockVariableOperations.Add(predicate21);
		variablePredicateParamDefaults[predicate10] = 1;
		variablePredicateParamDefaults[predicate11] = 1;
		variablePredicateParamDefaults[predicate12] = 2;
		variablePredicateParamDefaults[predicate13] = 2;
		variablePredicateParamDefaults[predicate14] = 2;
		variablePredicateParamDefaults[predicate15] = 0;
		variablePredicateParamDefaults[predicate16] = 0;
		variablePredicateParamDefaults[predicate17] = 0;
		variablePredicateParamDefaults[predicate18] = 0;
		variablePredicateParamDefaults[predicate19] = 10;
		variablePredicateParamDefaults[predicate20] = 0;
		variablePredicateParamDefaults[predicate21] = 0;
		blockVariableOperationsOnGlobals = new HashSet<Predicate>();
		blockVariableOperationsOnGlobals.Add(predicateBlockVariableIntLoadGlobal);
		blockVariableOperationsOnGlobals.Add(predicateBlockVariableIntStoreGlobal);
		blockVariableOperationsOnOtherBlockVars = new HashSet<Predicate>();
		blockVariableOperationsOnOtherBlockVars.Add(predicate22);
		blockVariableOperationsOnOtherBlockVars.Add(predicate23);
		blockVariableOperationsOnOtherBlockVars.Add(predicate24);
		blockVariableOperationsOnOtherBlockVars.Add(predicate25);
		blockVariableOperationsOnOtherBlockVars.Add(predicate26);
		blockVariableOperationsOnOtherBlockVars.Add(predicate27);
		blockVariableOperationsOnOtherBlockVars.Add(predicate28);
		blockVariableOperationsOnOtherBlockVars.Add(predicate29);
		blockVariableOperationsOnOtherBlockVars.Add(predicate30);
		variablePredicateLabels = new Dictionary<Predicate, string>();
		variablePredicateLabels[predicate22] = " + ";
		variablePredicateLabels[predicate23] = " - ";
		variablePredicateLabels[predicate24] = " * ";
		variablePredicateLabels[predicate25] = " / ";
		variablePredicateLabels[predicate26] = " % ";
		variablePredicateLabels[predicate27] = " = ";
		variablePredicateLabels[predicate28] = " != ";
		variablePredicateLabels[predicate29] = " < ";
		variablePredicateLabels[predicate30] = " > ";
		customVariablePredicates.UnionWith(globalVariableOperations);
		customVariablePredicates.UnionWith(blockVariableOperations);
		customVariablePredicates.UnionWith(blockVariableOperationsOnGlobals);
		customVariablePredicates.UnionWith(blockVariableOperationsOnOtherBlockVars);
	}

	public static string GetLabelForPredicate(Predicate p, string lhs, string rhs)
	{
		if (p == predicateBlockVariableIntLoadGlobal)
		{
			return lhs + " <- " + rhs;
		}
		if (p == predicateBlockVariableIntStoreGlobal)
		{
			return lhs + " -> " + rhs;
		}
		if (variablePredicateLabels.ContainsKey(p))
		{
			return lhs + variablePredicateLabels[p] + rhs;
		}
		return lhs;
	}

	public static void SetupVRPredicates()
	{
		PredicateRegistry.Add<Block>("Block.VRCameraMode", (Block b) => b.IsVRCameraMode);
		PredicateRegistry.Add<Block>("Block.IsVRCameraFocus", (Block b) => b.IsVRFocus);
		PredicateRegistry.Add<Block>("Block.IsVRCameraFocusModel", (Block b) => b.IsVRFocusModel);
		PredicateRegistry.Add<Block>("Block.IsVRCameraLookAt", (Block b) => b.IsVRLookAt);
		PredicateRegistry.Add<Block>("Block.IsVRCameraLookAtModel", (Block b) => b.IsVRLookAtModel);
	}

	public TileResultCode IsVRCameraMode(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (Blocksworld.IsVRCameraMode())
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsVRFocus(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (Blocksworld.IsBlockVRCameraFocus(this))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsVRFocusModel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		bool flag = false;
		UpdateConnectedCache();
		List<Block> list = connectedCache[this];
		for (int i = 0; i < list.Count; i++)
		{
			if (flag)
			{
				break;
			}
			flag |= Blocksworld.IsBlockVRCameraFocus(list[i]);
		}
		if (flag)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsVRLookAt(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (Blocksworld.IsBlockVRCameraLookAt(this))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsVRLookAtModel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		bool flag = false;
		UpdateConnectedCache();
		List<Block> list = connectedCache[this];
		for (int i = 0; i < list.Count; i++)
		{
			if (flag)
			{
				break;
			}
			flag |= Blocksworld.IsBlockVRCameraLookAt(list[i]);
		}
		if (flag)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	internal int GetRigidbodyConstraintsMask()
	{
		int globalRigidbodyConstraints = (int)BlockMaster.GetGlobalRigidbodyConstraints();
		return (globalRigidbodyConstraints & ~rbConstraintsOff) | rbConstraintsOn;
	}

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

	protected TileResultCode boolToTileResult(bool result)
	{
		if (result)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	protected TileResultCode tileResultFromRBConstraintExclusion(int existingConstraints, int rc)
	{
		bool result = (existingConstraints & rc) == 0;
		return boolToTileResult(result);
	}

	protected TileResultCode tileResultFromRBConstraintInclusion(int existingConstraints, int rc)
	{
		bool result = (existingConstraints & rc) == rc;
		return boolToTileResult(result);
	}

	public TileResultCode IsConstrainTranslation(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int rBConstraintsArgAsInt = GetRBConstraintsArgAsInt(args, 0, translation: true);
		return tileResultFromRBConstraintInclusion(rbConstraintsOn, rBConstraintsArgAsInt);
	}

	public TileResultCode ConstrainTranslation(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int rBConstraintsArgAsInt = GetRBConstraintsArgAsInt(args, 0, translation: true);
		rBConstraintsArgAsInt = rbConstraintsOn | rBConstraintsArgAsInt;
		rbUpdatedConstraints |= rBConstraintsArgAsInt != rbConstraintsOn;
		rbConstraintsOn = rBConstraintsArgAsInt;
		rbConstraintsOff &= ~rBConstraintsArgAsInt;
		return TileResultCode.True;
	}

	public TileResultCode IsFreeTranslation(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int rBConstraintsArgAsInt = GetRBConstraintsArgAsInt(args, 0, translation: true);
		return tileResultFromRBConstraintInclusion(rbConstraintsOff, rBConstraintsArgAsInt);
	}

	public TileResultCode FreeTranslation(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int rBConstraintsArgAsInt = GetRBConstraintsArgAsInt(args, 0, translation: true);
		rBConstraintsArgAsInt = rbConstraintsOff | rBConstraintsArgAsInt;
		rbUpdatedConstraints |= rBConstraintsArgAsInt != rbConstraintsOff;
		rbConstraintsOff = rBConstraintsArgAsInt;
		rbConstraintsOn &= ~rBConstraintsArgAsInt;
		return TileResultCode.True;
	}

	public TileResultCode IsConstrainRotation(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int rBConstraintsArgAsInt = GetRBConstraintsArgAsInt(args, 0, translation: false);
		return tileResultFromRBConstraintInclusion(rbConstraintsOn, rBConstraintsArgAsInt);
	}

	public TileResultCode ConstrainRotation(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int rBConstraintsArgAsInt = GetRBConstraintsArgAsInt(args, 0, translation: false);
		rBConstraintsArgAsInt = rbConstraintsOn | rBConstraintsArgAsInt;
		rbUpdatedConstraints |= rBConstraintsArgAsInt != rbConstraintsOn;
		rbConstraintsOn = rBConstraintsArgAsInt;
		rbConstraintsOff &= ~rBConstraintsArgAsInt;
		return TileResultCode.True;
	}

	public TileResultCode IsFreeRotation(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int rBConstraintsArgAsInt = GetRBConstraintsArgAsInt(args, 0, translation: false);
		return tileResultFromRBConstraintInclusion(rbConstraintsOff, rBConstraintsArgAsInt);
	}

	public TileResultCode FreeRotation(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int rBConstraintsArgAsInt = GetRBConstraintsArgAsInt(args, 0, translation: false);
		rBConstraintsArgAsInt = rbConstraintsOff | rBConstraintsArgAsInt;
		rbUpdatedConstraints |= rBConstraintsArgAsInt != rbConstraintsOff;
		rbConstraintsOff = rBConstraintsArgAsInt;
		rbConstraintsOn &= ~rBConstraintsArgAsInt;
		return TileResultCode.True;
	}

	public static void Init()
	{
		if (leaderboardData == null)
		{
			leaderboardData = Blocksworld.leaderboardData;
		}
		if (!Blocksworld.renderingShadows)
		{
			if (prefabShadow == null)
			{
				prefabShadow = UnityEngine.Object.Instantiate(Resources.Load("Blocks/Shadow2")) as GameObject;
				prefabShadow.SetActive(value: false);
			}
			if (shadowParent == null)
			{
				GameObject gameObject = new GameObject("Block Shadows");
				shadowParent = gameObject.transform;
				prefabShadow.transform.parent = shadowParent;
			}
		}
	}

	protected void InstantiateShadow(GameObject prefab)
	{
		goShadow = UnityEngine.Object.Instantiate(prefab);
		goShadow.SetActive(value: true);
		goShadowT = goShadow.transform;
		goShadowT.parent = shadowParent;
		hasShadow = true;
		Mesh mesh = meshShadow;
		meshShadow = goShadow.GetComponent<MeshFilter>().mesh;
		if (mesh != null && mesh != meshShadow)
		{
			UnityEngine.Object.Destroy(mesh);
		}
		colorsShadow = new Color[4];
		UpdateShadowColors("Black");
	}

	private bool HasShadow()
	{
		if (!isTerrain)
		{
			return !(this is BlockAbstractMovingPlatform);
		}
		return false;
	}

	public virtual void Destroy()
	{
		UnityEngine.Object.Destroy(mesh);
		if (childMeshes != null)
		{
			foreach (Mesh value in childMeshes.Values)
			{
				UnityEngine.Object.Destroy(value);
			}
		}
		childMeshes = null;
		UnityEngine.Object.Destroy(go);
		go = null;
		DestroyShadow();
		BWSceneManager.BlockDestroyed(this);
	}

	public void DestroyShadow()
	{
		if (goShadow != null)
		{
			UnityEngine.Object.Destroy(goShadow);
		}
		if (meshShadow != null)
		{
			UnityEngine.Object.Destroy(meshShadow);
		}
		goShadow = null;
		goShadowT = null;
		hasShadow = false;
	}

	public virtual void BeforePlay()
	{
		hadRigidBody = false;
	}

	public int AddOrRemoveEmptyScriptLine()
	{
		int num = 0;
		if (tiles[tiles.Count - 1].Count > 1)
		{
			Tile item = new Tile(new GAF("Meta.Then"));
			tiles.Add(new List<Tile> { item });
			num++;
		}
		for (int num2 = tiles.Count - 2; num2 >= 0; num2--)
		{
			if (tiles[num2].Count == 1)
			{
				tiles[num2][0].Show(show: false);
				tiles.RemoveAt(num2);
				num--;
			}
		}
		return num;
	}

	public void RemoveTile(Tile tile)
	{
		foreach (List<Tile> tile2 in tiles)
		{
			if (tile2.Remove(tile))
			{
				break;
			}
		}
	}

	public void PrintTiles()
	{
		string text = string.Empty;
		foreach (List<Tile> tile in tiles)
		{
			foreach (Tile item in tile)
			{
				text = text + item?.ToString() + ", ";
			}
			text += "\n";
		}
		BWLog.Info(text);
	}

	public void AddLockedTileRow()
	{
		tiles.Insert(1, LockedTileRow());
	}

	public bool IsDefaultPaint(GAF gaf)
	{
		string paint = (string)gaf.Args[0];
		int meshIndex = ((gaf.Args.Length > 1) ? ((int)gaf.Args[1]) : 0);
		return Scarcity.FreePaint(BlockType(), meshIndex, paint);
	}

	public string GetDefaultPaint(int meshIndex = 0)
	{
		string blockType = BlockType();
		return Scarcity.DefaultPaint(blockType, meshIndex);
	}

	public bool IsDefaultTexture(GAF gaf)
	{
		string texture = (string)gaf.Args[0];
		int meshIndex = ((gaf.Args.Length > 2) ? ((int)gaf.Args[2]) : 0);
		return IsDefaultTexture(texture, meshIndex);
	}

	public bool IsDefaultTexture(string texture, int meshIndex)
	{
		return Scarcity.GetNormalizedTexture(texture) == Scarcity.GetNormalizedTexture(GetDefaultTexture(meshIndex));
	}

	public string GetDefaultTexture(int meshIndex = 0)
	{
		string blockType = BlockType();
		return Scarcity.DefaultTexture(blockType, meshIndex);
	}

	public bool[] GetCanBeTextured()
	{
		string key = BlockType();
		if (blockTypeCanBeTextured == null)
		{
			blockTypeCanBeTextured = new Dictionary<string, bool[]>();
		}
		if (!blockTypeCanBeTextured.ContainsKey(key))
		{
			List<bool> list = new List<bool>();
			BlockMeshMetaData[] blockMeshMetaDatas = GetBlockMeshMetaDatas();
			foreach (BlockMeshMetaData blockMeshMetaData in blockMeshMetaDatas)
			{
				list.Add(blockMeshMetaData.canBeTextured);
			}
			bool[] array = list.ToArray();
			blockTypeCanBeTextured[key] = array;
			return array;
		}
		return blockTypeCanBeTextured[key];
	}

	public float GetBuoyancyMultiplier()
	{
		BlockMetaData blockMetaData = GetBlockMetaData();
		if (blockMetaData != null)
		{
			return blockMetaData.buoyancyMultiplier;
		}
		return 1f;
	}

	public bool[] GetCanBeMaterialTextured()
	{
		string key = BlockType();
		if (!blockTypeCanBeMaterialTextured.ContainsKey(key))
		{
			List<bool> list = new List<bool>();
			BlockMeshMetaData[] blockMeshMetaDatas = GetBlockMeshMetaDatas();
			foreach (BlockMeshMetaData blockMeshMetaData in blockMeshMetaDatas)
			{
				list.Add(blockMeshMetaData.canBeMaterialTextured);
			}
			bool[] array = list.ToArray();
			blockTypeCanBeMaterialTextured[key] = array;
			return array;
		}
		return blockTypeCanBeMaterialTextured[key];
	}

	public static bool IsDefaultSfx(string blockType, string sfxName)
	{
		if (defaultSfxs.TryGetValue(blockType, out var value))
		{
			return value.Contains(sfxName);
		}
		return false;
	}

	private static void CreateDefaultDictionaries()
	{
		if (defaultTextureNormals != null)
		{
			return;
		}
		defaultTextureNormals = new Dictionary<string, Vector3[]>();
		defaultTextureNormals.Add("Sky UV", new Vector3[1]
		{
			new Vector3(1f, 0f, -1f)
		});
		defaultTextureNormals.Add("Terrain Cube", new Vector3[1] { Vector3.up });
		defaultTextureNormals.Add("Terrain Wedge", new Vector3[1] { Vector3.up });
		defaultTextureNormals.Add("Emitter", new Vector3[1] { Vector3.right });
		foreach (string treasureBlock in treasureBlocks)
		{
			defaultSfxs[treasureBlock] = new HashSet<string> { treasurePickupSfx };
		}
	}

	private static void AddSameGridCellPair(string s1, string s2)
	{
		HashSet<string> hashSet;
		if (sameGridCellPairs.ContainsKey(s1))
		{
			hashSet = sameGridCellPairs[s1];
		}
		else
		{
			hashSet = new HashSet<string>();
			sameGridCellPairs.Add(s1, hashSet);
		}
		hashSet.Add(s2);
	}

	private static void AddSameGridCellPairBidirectional(string s1, string s2)
	{
		AddSameGridCellPair(s1, s2);
		AddSameGridCellPair(s2, s1);
	}

	public static void UpdateOccupySameGridCell(Block b)
	{
		string text = b.BlockType();
		if (sameGridCellPairs.ContainsKey(text))
		{
			return;
		}
		BlockMetaData blockMetaData = b.GetBlockMetaData();
		if (blockMetaData != null)
		{
			string[] canOccupySameGrid = blockMetaData.canOccupySameGrid;
			string[] array = canOccupySameGrid;
			foreach (string s in array)
			{
				AddSameGridCellPairBidirectional(text, s);
			}
		}
	}

	public static bool WantsToOccupySameGridCell(Block b1, Block b2)
	{
		UpdateOccupySameGridCell(b1);
		UpdateOccupySameGridCell(b2);
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
		if (sameGridCellPairs.ContainsKey(key))
		{
			return sameGridCellPairs[key].Contains(item);
		}
		return false;
	}

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
		tiles[tiles.Count - 1].Add(new Tile(new GAF("Meta.Then")));
	}

	private static void AddAntigravityDefaultActions(bool defaultTiles, string prefix, List<List<Tile>> tiles)
	{
		AddDefaultSensorActions(defaultTiles, tiles, null, new GAF(prefix + ".IncreaseModelGravityInfluence", -1f));
		AddDefaultSensorActions(defaultTiles, tiles, null, new GAF(prefix + ".AlignInGravityFieldChunk", 1f));
	}

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
			tiles[tiles.Count - 1].Add(new Tile(new GAF("Meta.Then")));
		}
	}

	private static void AddFirstRowActions(List<List<Tile>> tiles, params GAF[] actionGafs)
	{
		AddSensorActions(tiles, null, actionGafs);
	}

	private static void AddDefaultGAF(bool defaultTiles, List<List<Tile>> tiles, string predName, params object[] args)
	{
		if (defaultTiles)
		{
			tiles[1].Add(new Tile(new GAF(predName, args)));
			tiles.Add(new List<Tile>());
			tiles[2].Add(new Tile(new GAF("Meta.Then")));
		}
	}

	public virtual bool HasDefaultScript(List<List<Tile>> tilesToUse = null)
	{
		List<List<Tile>> defaultExtraTilesCopy = GetDefaultExtraTilesCopy(BlockType());
		if (tilesToUse == null)
		{
			tilesToUse = tiles;
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
					if (IgnoreChangesToDefaultForPredicate(gaf2.Predicate))
					{
						continue;
					}
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
		return true;
	}

	protected virtual bool IgnoreChangesToDefaultForPredicate(Predicate predicate)
	{
		return false;
	}

	public bool HasDefaultTiles()
	{
		if (CanScale().magnitude > 0.01f)
		{
			Vector3 defaultScale = GetDefaultScale();
			Vector3 scale = GetScale();
			if ((defaultScale - scale).magnitude > 0.01f)
			{
				return false;
			}
		}
		Dictionary<GAF, int> dictionary = new Dictionary<GAF, int>();
		Scarcity.CalculateBlockGafUsage(this, dictionary);
		foreach (GAF key in dictionary.Keys)
		{
			Predicate predicate = key.Predicate;
			if (predicate == BlockAnimatedCharacter.predicateReplaceLimb)
			{
				return false;
			}
			if (predicate == predicateTextureTo || predicate == predicatePaintTo || predicate == predicatePlaySoundDurational)
			{
				return false;
			}
		}
		return HasDefaultScript();
	}

	protected static Tile FirstFrameTile()
	{
		return new Tile(new GAF("Block.FirstFrame"));
	}

	public static Tile ThenTile()
	{
		return new Tile(new GAF("Meta.Then"));
	}

	public static Tile StopTile()
	{
		return new Tile(new GAF(predicateStop));
	}

	public static Tile WaitTile(float time)
	{
		return new Tile(predicateWaitTime, time);
	}

	public static Tile ButtonTile(string name)
	{
		return new Tile(new GAF(predicateButton, name));
	}

	public static List<Tile> EmptyTileRow()
	{
		return new List<Tile> { ThenTile() };
	}

	public static Tile LockedTile()
	{
		return new Tile(new GAF(predicateLocked));
	}

	public static List<Tile> LockedTileRow()
	{
		return new List<Tile>
		{
			ThenTile(),
			LockedTile()
		};
	}

	public static void WriteDefaultExtraTiles(bool defaultTiles, List<List<Tile>> tiles, string blockType)
	{
		if (defaultTiles)
		{
			List<List<Tile>> defaultExtraTilesCopy = GetDefaultExtraTilesCopy(blockType, processTiles: true);
			if (defaultExtraTilesCopy != null)
			{
				tiles.RemoveRange(1, tiles.Count - 1);
				tiles.AddRange(defaultExtraTilesCopy);
			}
		}
	}

	public static List<List<Tile>> GetDefaultExtraTilesCopy(string blockType, bool processTiles = false)
	{
		if (defaultExtraTiles.TryGetValue(blockType, out var value))
		{
			List<List<Tile>> list = Util.CopyTiles(value);
			if (processTiles && defaultExtraTilesProcessors.TryGetValue(blockType, out var value2))
			{
				value2(list);
			}
			return list;
		}
		return null;
	}

	protected static void AddSimpleDefaultTiles(GAF gaf, params string[] blockTypes)
	{
		List<List<Tile>> value = new List<List<Tile>>
		{
			new List<Tile>
			{
				ThenTile(),
				new Tile(gaf)
			},
			EmptyTileRow()
		};
		foreach (string key in blockTypes)
		{
			defaultExtraTiles[key] = value;
		}
	}

	protected static void AddSimpleDefaultTiles(GAF gafFirstRow, GAF gafSecondRow, params string[] blockTypes)
	{
		List<List<Tile>> value = new List<List<Tile>>
		{
			new List<Tile>
			{
				ThenTile(),
				new Tile(gafFirstRow)
			},
			new List<Tile>
			{
				ThenTile(),
				new Tile(gafSecondRow)
			},
			EmptyTileRow()
		};
		foreach (string key in blockTypes)
		{
			defaultExtraTiles[key] = value;
		}
	}

	protected static void AddSimpleDefaultTiles(List<GAF> gafs, params string[] blockTypes)
	{
		List<Tile> list = new List<Tile> { ThenTile() };
		foreach (GAF gaf in gafs)
		{
			list.Add(new Tile(gaf));
		}
		List<List<Tile>> value = new List<List<Tile>>
		{
			list,
			EmptyTileRow()
		};
		foreach (string key in blockTypes)
		{
			defaultExtraTiles[key] = value;
		}
	}

	public static Block NewBlock(List<List<Tile>> tiles, bool defaultColors = false, bool defaultTiles = false)
	{
		CreateDefaultDictionaries();
		List<Tile> list = tiles[0];
		int index = 0;
		if (list[index].gaf.Predicate != predicateStop)
		{
			list.Insert(index, StopTile());
		}
		int index2 = 5;
		if (list[index2].gaf.Predicate != predicateTextureTo)
		{
			list.Insert(index2, new Tile(new GAF(predicateTextureTo, "Plain", Vector3.zero)));
		}
		List<Tile> list2 = tiles[tiles.Count - 1];
		if (list2.Count != 1 || list2[0].gaf.Predicate != predicateThen)
		{
			tiles.Add(EmptyTileRow());
		}
		string text = (string)tiles[0][1].gaf.Args[0];
		if (text == "Character Avatar" && (WorldSession.current.BlockIsAvailable("Block Anim Character Male") || WorldSession.current.BlockIsAvailable("Block Anim Character Female") || WorldSession.current.BlockIsAvailable("Block Anim Character Skeleton")))
		{
			text = "Anim Character Avatar";
			tiles[0][1].gaf = new GAF(predicateCreate, text);
			ProfileBlocksterUtils.ConvertToAnimated(tiles);
		}
		if (text == "Anim Character Avatar" && !WorldSession.current.BlockIsAvailable("Block Anim Character Male") && !WorldSession.current.BlockIsAvailable("Block Anim Character Female") && !WorldSession.current.BlockIsAvailable("Block Anim Character Skeleton"))
		{
			text = "Character Avatar";
			tiles[0][1].gaf = new GAF(predicateCreate, text);
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
				UnityEngine.Object obj = Resources.Load("Blocks/Prefab " + text);
				if (obj != null)
				{
					Blocksworld.existingBlockNames.Add(text);
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
						BWLog.Warning("Missing prefab '" + text + "', using '" + enumerator.Current + "' instead");
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
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			block = new BlockJukebox(tiles);
			break;
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
			break;
		case "Cloud 1":
		case "Cloud 2":
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			block = new BlockCloud(tiles);
			((BlockTerrain)block).doubleTapToSelect = true;
			BWSceneManager.AddTerrainBlock((BlockTerrain)block);
			break;
		case "Volcano":
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			block = new BlockVolcano(tiles);
			((BlockTerrain)block).doubleTapToSelect = true;
			BWSceneManager.AddTerrainBlock((BlockTerrain)block);
			break;
		case "Water Cube":
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			block = new BlockWaterCube(tiles);
			BlockAbstractWater.waterCubes.Add((BlockWaterCube)block);
			break;
		case "Master":
			block = new BlockMaster(tiles);
			break;
		case "Highscore I":
			block = new BlockHighscoreList(tiles, 0);
			break;
		case "UI Counter I":
			if (defaultTiles)
			{
				AddSensorActions(tiles, new GAF("Block.FirstFrame"), new GAF("CounterUI.Equals", 0, 0));
			}
			block = new BlockCounterUI(tiles, 0);
			break;
		case "UI Counter II":
			if (defaultTiles)
			{
				AddSensorActions(tiles, new GAF("Block.FirstFrame"), new GAF("CounterUI.Equals", 0, 1));
			}
			block = new BlockCounterUI(tiles, 1);
			break;
		case "UI Object Counter I":
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			block = new BlockObjectCounterUI(tiles, 0);
			break;
		case "UI Gauge I":
			if (defaultTiles)
			{
				AddSensorActions(tiles, new GAF("Block.FirstFrame"), new GAF("GaugeUI.Equals", 0, 0));
			}
			block = new BlockGaugeUI(tiles, 0);
			break;
		case "UI Gauge II":
			if (defaultTiles)
			{
				AddSensorActions(tiles, new GAF("Block.FirstFrame"), new GAF("GaugeUI.Equals", 0, 1));
			}
			block = new BlockGaugeUI(tiles, 1);
			break;
		case "UI Radar I":
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			block = new BlockRadarUI(tiles, 0);
			break;
		case "UI Timer I":
			if (defaultTiles)
			{
				AddSensorActions(tiles, new GAF("Block.FirstFrame"), new GAF("TimerUI.Equals", 0f));
			}
			block = new BlockTimerUI(tiles, 0);
			break;
		case "UI Timer II":
			if (defaultTiles)
			{
				AddSensorActions(tiles, new GAF("Block.FirstFrame"), new GAF("TimerUI.Equals", 0f));
			}
			block = new BlockTimerUI(tiles, 1);
			break;
		case "Orrery":
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			block = new BlockWorldJumper(tiles);
			break;
		case "Rocket":
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			block = new BlockRocket(tiles);
			break;
		case "Rocket Square":
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			block = new BlockRocketSquare(tiles);
			break;
		case "Rocket Octagonal":
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			block = new BlockRocketOctagonal(tiles);
			break;
		case "Tank Treads Wheel":
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			block = new BlockTankTreadsWheel(tiles);
			break;
		case "Missile A":
		case "Missile B":
		case "Missile C":
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			block = new BlockMissile(tiles);
			break;
		case "Missile Control":
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			block = new BlockMissileControl(tiles);
			break;
		case "Missile Control Model":
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			block = new BlockModelMissileControl(tiles);
			break;
		case "Jet Engine":
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			block = new BlockJetEngine(tiles);
			break;
		case "Raycast Wheel":
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			block = new BlockRaycastWheel(tiles);
			break;
		case "Wheel":
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			block = new BlockWheel(tiles, "Wheel Axle");
			break;
		case "RAR Moon Rover Wheel":
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			block = new BlockWheel(tiles, "RAR Moon Rover Wheel Axle");
			break;
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
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			block = new BlockTwoSidedWheel(tiles);
			break;
		case "Bulky Wheel":
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			block = new BlockBulkyWheel(tiles, "Bulky");
			break;
		case "Spoked Wheel":
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			block = new BlockSpokedWheel(tiles);
			break;
		case "Golden Wheel":
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			block = new BlockWheelBling(tiles);
			if (defaultColors)
			{
				block.RotateTo(Quaternion.Euler(0f, 270f, 0f));
			}
			break;
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
			break;
		case "Sphere":
		case "Soccer Ball":
		case "Geodesic Ball":
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			block = new BlockSphere(tiles);
			break;
		case "Motor":
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			block = new BlockMotor(tiles);
			break;
		case "Motor Cube":
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			block = new BlockMotorCube(tiles);
			break;
		case "Motor Slab":
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			block = new BlockMotorSlab(tiles);
			break;
		case "Motor Slab 2":
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			block = new BlockMotorSlab2(tiles);
			break;
		case "Piston":
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			block = new BlockPiston(tiles);
			break;
		case "Magnet":
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			block = new BlockMagnet(tiles);
			break;
		case "Torsion Spring":
			block = new BlockTorsionSpring(tiles);
			break;
		case "Torsion Spring Slab":
			block = new BlockTorsionSpringSlab(tiles);
			break;
		case "Torsion Spring Cube":
			block = new BlockTorsionSpringCube(tiles);
			break;
		case "Motor Spindle":
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			block = new BlockMotorSpindle(tiles);
			break;
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
			break;
		case "PIR Pistol":
			block = new BlockPIRPistol(tiles);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		case "RAR Cross Bow":
			block = new BlockCrossbow(tiles);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		case "Laser":
			block = new BlockLaser(tiles);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			if (defaultColors)
			{
				block.RotateTo(Quaternion.Euler(90f, 0f, 0f));
			}
			break;
		case "Laser Cannon":
			block = new BlockLaserCannon(tiles);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		case "Laser Blaster":
			block = new BlockLaserBlaster(tiles);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		case "Laser Octagonal":
			block = new BlockOctagonalLaser(tiles);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		case "Jazz Gun":
			block = new BlockJazzGun(tiles);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		case "Bumblebee Gun":
			block = new BlockBumblebeeGun(tiles);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		case "Megatron Gun":
			block = new BlockMegatronGun(tiles);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		case "Optimus Gun":
			block = new BlockOptimusGun(tiles);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		case "Soundwave Gun":
			block = new BlockSoundwaveGun(tiles);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		case "Starscream Gun":
			block = new BlockStarscreamGun(tiles);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		case "Laser Minigun":
			block = new BlockLaserMiniGun(tiles);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		case "Laser Rifle":
			block = new BlockLaserRifle(tiles);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		case "Hand Cannon":
			block = new BlockHandCannon(tiles);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		case "Laser Pistol":
			block = new BlockLaserPistol(tiles);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		case "Laser Pistol2":
		case "BBG Ray Gun":
		case "FUT Space Gun":
			block = new BlockLaserPistol2(tiles);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		case "GIJ MiniGun":
			block = new BlockMiniGun(tiles);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		case "Emitter":
			block = new BlockEmitter(tiles);
			if (defaultColors)
			{
				block.RotateTo(Quaternion.Euler(180f, 0f, 0f));
			}
			break;
		case "Stabilizer":
			block = new BlockStabilizer(tiles);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		case "Stabilizer Square":
			block = new BlockSquareStabilizer(tiles);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
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
			block.ScaleTo(new Vector3(vector.x, vector2.y, vector.z));
			BlockWater blockWater = (BlockWater)block;
			blockWater.snapper = vector.x / 10f;
			break;
		}
		case "Moving Platform Force":
			if (defaultTiles)
			{
				AddFirstRowActions(tiles, new GAF("MovingPlatformForce.MoveTo", 1, 2f), new GAF("MovingPlatformForce.MoveTo", 0, 2f));
			}
			block = new BlockMovingPlatformForce(tiles);
			break;
		case "Moving Platform":
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			block = new BlockMovingPlatform(tiles);
			break;
		case "Rotating Platform":
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			block = new BlockRotatingPlatform(tiles);
			break;
		case "Antigravity Pump":
			block = new BlockAntiGravity(tiles);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		case "Antigravity Cube":
			block = new BlockAntiGravity(tiles);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		case "Antigravity Column":
			block = new BlockAntiGravityColumn(tiles);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		case "Bat Wing":
			block = new BlockBatWing(tiles);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		case "Fairy Wings":
			block = new BlockFairyWings(tiles);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		case "Cape":
		case "BBG Cape":
			block = new BlockCape(tiles);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		case "SPY Jet Pack":
		case "RAR Jet Pack":
		case "FUT Space EVA":
			block = new BlockJetpack(tiles);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		case "Bat Wing Backpack":
			block = new BlockBatWingBackpack(tiles);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		case "Bird Wing":
			block = new BlockBirdWing(tiles);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		case "Wiser Wing":
			block = new BlockWiserWing(tiles);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		case "MLP Wings":
			block = new BlockMLPWings(tiles);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		case "Flight Yoke":
			block = new BlockFlightYoke(tiles);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		case "Steering Wheel":
			block = new BlockSteeringWheel(tiles);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		case "Drive Assist":
			if (defaultTiles)
			{
				tiles[1].Add(new Tile(new GAF("DriveAssist.Assist", 1f)));
				tiles[1].Add(new Tile(new GAF("DriveAssist.AlignInGravityFieldChunk", 1f)));
				tiles.Add(new List<Tile>());
				tiles[2].Add(new Tile(new GAF("Meta.Then")));
			}
			block = new BlockDriveAssist(tiles);
			break;
		case "Hover":
			block = new BlockHover(tiles);
			if (defaultColors)
			{
				block.PaintTo("White", permanent: true);
			}
			break;
		case "GravityGun":
			block = new BlockGravityGun(tiles);
			if (defaultColors)
			{
				block.PaintTo("Blue", permanent: true);
			}
			break;
		case "Position":
		case "Position No Glue":
		case "Position Camera Hint":
			block = new BlockPosition(tiles);
			if (defaultTiles)
			{
				tiles[1].Add(new Tile(new GAF("Block.Fixed")));
				tiles.Add(new List<Tile>());
				tiles[2].Add(new Tile(new GAF("Meta.Then")));
			}
			break;
		case "Teleport Volume Block":
			block = new BlockTeleportVolumeBlock(tiles);
			break;
		case "Volume Block":
		case "Volume Block No Glue":
		case "Volume Block Slab":
		case "Volume Block Slab No Glue":
			block = new BlockVolumeBlock(tiles);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		case "Volume Block Force":
			block = new BlockVolumeForce(tiles);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		case "Volume":
			block = new BlockVolume(tiles);
			break;
		case "Legs":
		{
			Dictionary<string, string> dictionary9 = new Dictionary<string, string>();
			dictionary9["Legs Mesh"] = string.Empty;
			block = new BlockLegs(tiles, dictionary9, 0.125f);
			break;
		}
		case "Raptor Legs":
		{
			Dictionary<string, string> dictionary8 = new Dictionary<string, string>();
			dictionary8["Legs Mesh"] = string.Empty;
			block = new BlockLegs(tiles, dictionary8, 0.125f, 0.25f);
			break;
		}
		case "Legs Small":
		{
			Dictionary<string, string> dictionary7 = new Dictionary<string, string>();
			dictionary7["Foot Right Front"] = "Legs Small Foot Right Front";
			dictionary7["Foot Left Front"] = "Legs Small Foot Left Front";
			dictionary7["Foot Right Back"] = "Legs Small Foot Right Back";
			dictionary7["Foot Left Back"] = "Legs Small Foot Left Back";
			dictionary7["Legs Mesh"] = "Legs Small Ankle Mesh";
			block = new BlockQuadped(tiles, dictionary7, 0.5f);
			break;
		}
		case "MLP Body":
		{
			Dictionary<string, string> dictionary6 = new Dictionary<string, string>();
			dictionary6["Foot Right Front"] = "MLP Legs Foot Right Front";
			dictionary6["Foot Left Front"] = "MLP Legs Foot Left Front";
			dictionary6["Foot Right Back"] = "MLP Legs Foot Right Back";
			dictionary6["Foot Left Back"] = "MLP Legs Foot Left Back";
			dictionary6["Legs Mesh Right Front"] = "MLP Legs Ankle Mesh Right Front";
			dictionary6["Legs Mesh Left Front"] = "MLP Legs Ankle Mesh Left Front";
			dictionary6["Legs Mesh Right Back"] = "MLP Legs Ankle Mesh Right Back";
			dictionary6["Legs Mesh Left Back"] = "MLP Legs Ankle Mesh Left Back";
			block = new BlockMLPLegs(tiles, dictionary6, 0.875f);
			break;
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
			Dictionary<string, string> dictionary5 = new Dictionary<string, string>();
			dictionary5["Legs Mesh"] = string.Empty;
			block = new BlockCharacter(tiles, dictionary5, hasLegMesh: false);
			break;
		}
		case "Character Female Profile":
		case "Character Female":
		{
			Dictionary<string, string> dictionary4 = new Dictionary<string, string>();
			dictionary4["Legs Mesh"] = string.Empty;
			BlockCharacter.StripNonCompatibleTiles(tiles);
			block = new BlockCharacter(tiles, dictionary4, hasLegMesh: false);
			if (defaultColors)
			{
				block.RotateTo(Quaternion.Euler(0f, 90f, 0f));
			}
			break;
		}
		case "Character Female Dress":
		{
			Dictionary<string, string> dictionary3 = new Dictionary<string, string>();
			dictionary3["Body"] = "Character Dress Body";
			dictionary3["Legs Mesh"] = "Character Skirt";
			block = new BlockCharacter(tiles, dictionary3, hasLegMesh: true);
			if (defaultColors)
			{
				block.RotateTo(Quaternion.Euler(0f, 90f, 0f));
			}
			break;
		}
		case "Character Mini":
		case "Character Mini Female":
		case "Character Mini Profile":
		case "Character Mini Female Profile":
		{
			Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
			dictionary2["Legs Mesh"] = string.Empty;
			BlockCharacter.StripNonCompatibleTiles(tiles);
			block = new BlockCharacter(tiles, dictionary2, hasLegMesh: false, -1f);
			if (text.Contains("Female") && defaultColors)
			{
				block.RotateTo(Quaternion.Euler(0f, 90f, 0f));
			}
			break;
		}
		case "Character Avatar":
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary["Legs Mesh"] = string.Empty;
			BlockCharacter.StripNonCompatibleTiles(tiles);
			block = new BlockCharacter(tiles, dictionary, hasLegMesh: false, -1f);
			((BlockCharacter)block).isAvatar = true;
			BWSceneManager.RegisterRuntimeBlockSubstitution(block, new UserAvatarSubstitution(block as BlockCharacter));
			break;
		}
		case "Anim Character Avatar":
			BlockAnimatedCharacter.StripNonCompatibleTiles(tiles);
			block = new BlockAnimatedCharacter(tiles, 2.5f, CharacterType.Avatar);
			BWSceneManager.RegisterRuntimeBlockSubstitution(block, new UserAvatarSubstitution(block as BlockAnimatedCharacter));
			break;
		case "Anim Character Male Profile":
		case "Anim Character":
		case "Anim Character Male":
			BlockAnimatedCharacter.StripNonCompatibleTiles(tiles);
			block = new BlockAnimatedCharacter(tiles, 2.5f, CharacterType.Male);
			break;
		case "Anim Character Skeleton":
		case "Anim Character Skeleton Profile":
			BlockAnimatedCharacter.StripNonCompatibleTiles(tiles);
			block = new BlockAnimatedCharacter(tiles, 2.5f, CharacterType.Skeleton);
			break;
		case "Anim Character Female Profile":
		case "Anim Character Female":
			BlockAnimatedCharacter.StripNonCompatibleTiles(tiles);
			block = new BlockAnimatedCharacter(tiles, 2.5f, CharacterType.Female);
			if (defaultColors)
			{
				block.RotateTo(Quaternion.Euler(0f, 90f, 0f));
			}
			break;
		case "Anim Character Female Dress":
			block = new BlockAnimatedCharacter(tiles, 2.5f, CharacterType.Dress);
			if (defaultColors)
			{
				block.RotateTo(Quaternion.Euler(0f, 90f, 0f));
			}
			break;
		case "Anim Character Mini":
			BlockAnimatedCharacter.StripNonCompatibleTiles(tiles);
			block = new BlockAnimatedCharacter(tiles, 1.75f, CharacterType.MiniMale);
			if (text.Contains("Female") && defaultColors)
			{
				block.RotateTo(Quaternion.Euler(0f, 90f, 0f));
			}
			break;
		case "Anim Character Mini Female":
			BlockAnimatedCharacter.StripNonCompatibleTiles(tiles);
			block = new BlockAnimatedCharacter(tiles, 1.75f, CharacterType.MiniFemale);
			if (text.Contains("Female") && defaultColors)
			{
				block.RotateTo(Quaternion.Euler(0f, 90f, 0f));
			}
			break;
		case "Tree Poplar":
			block = new Block(tiles);
			if (defaultColors)
			{
				block.ScaleTo(new Vector3(1f, 3f, 1f));
			}
			break;
		case "Slice Inverse":
			block = new BlockSliceInverse(tiles);
			break;
		case "Water Emitter Block":
			block = new BlockEmitterWater(tiles);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		case "Water Volume Block":
			block = new BlockEmitterWater(tiles, shouldHide: true);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		case "Fire Emitter Block":
			block = new BlockEmitterFire(tiles);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		case "Fire Volume Block":
			block = new BlockEmitterFire(tiles, shouldHide: true);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		case "Gas Emitter Block":
			block = new BlockEmitterGas(tiles);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		case "Gas Volume Block":
			block = new BlockEmitterGas(tiles, shouldHide: true);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		case "Campfire":
			block = new BlockEmitterCampfire(tiles);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		case "Mannequin":
		case "SAM Statue":
		case "Statue":
			block = new BlockStatue(tiles);
			WriteDefaultExtraTiles(defaultTiles, tiles, text);
			break;
		default:
			{
				foreach (string key in BlockItemsRegistry.GetBlockEntries().Keys)
				{
					if (!(key == text))
					{
						continue;
					}
					BlockEntry blockEntry = BlockItemsRegistry.GetBlockEntries()[key];
					if (blockEntry.blockType != null)
					{
						Type blockType = blockEntry.blockType;
						ConstructorInfo constructor = blockType.GetConstructor(new Type[1] { typeof(List<List<Tile>>) });
						block = constructor.Invoke(new object[1] { tiles }) as Block;
						if (blockEntry.hasDefaultTiles)
						{
							WriteDefaultExtraTiles(defaultTiles, tiles, text);
						}
						goto end_IL_031b;
					}
				}
				if (Blocksworld.compoundColliders.ContainsKey(text))
				{
					block = new BlockCompoundCollider(tiles);
					break;
				}
				if (text.StartsWith("Terrain "))
				{
					block = new BlockTerrain(tiles);
					BWSceneManager.AddTerrainBlock((BlockTerrain)block);
					break;
				}
				if (treasureBlocks.Contains(text))
				{
					if (!defaultExtraTiles.ContainsKey(text))
					{
						defaultExtraTiles[text] = new List<List<Tile>>
						{
							new List<Tile>
							{
								ThenTile(),
								new Tile(new GAF("Block.IsTreasure", 0))
							},
							new List<Tile>
							{
								new Tile(new GAF("Block.OnCollect")),
								ThenTile(),
								new Tile(new GAF("Block.PlaySoundDurational", treasurePickupSfx, "Camera"))
							},
							EmptyTileRow()
						};
					}
					WriteDefaultExtraTiles(defaultTiles, tiles, text);
					block = ((!(text == "Idol")) ? new Block(tiles) : new BlockIdol(tiles));
					break;
				}
				block = new Block(tiles);
				RegisterDefaultTilesWithMetaData(block, text);
				if (defaultTiles)
				{
					List<List<Tile>> defaultExtraTilesCopy = GetDefaultExtraTilesCopy(text);
					if (defaultExtraTilesCopy != null)
					{
						tiles.RemoveRange(1, tiles.Count - 1);
						tiles.AddRange(defaultExtraTilesCopy);
					}
				}
				if (defaultColors)
				{
					block.PaintTo("Yellow", permanent: true);
				}
				break;
			}
			end_IL_031b:
			break;
		}
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
					block.ScaleTo(defaultScale);
				}
				string[] array = Scarcity.DefaultPaints(text);
				for (int i = 0; i < array.Length; i++)
				{
					string paint = array[i].Split(',')[0];
					block.PaintTo(paint, permanent: true, i);
				}
				string[] array2 = Scarcity.DefaultTextures(text);
				Vector3[] array3 = block.GetDefaultTextureNormals();
				if (defaultTextureNormals.ContainsKey(text))
				{
					array3 = defaultTextureNormals[text];
				}
				for (int j = 0; j < array2.Length; j++)
				{
					Vector3 normal = Vector3.up;
					if (array3 != null && j < array3.Length)
					{
						normal = array3[j];
					}
					block.TextureTo(array2[j], normal, permanent: true, j, force: true);
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

	private static void RegisterDefaultTilesWithMetaData(Block b, string blockType)
	{
		if (defaultExtraTiles.ContainsKey(blockType))
		{
			return;
		}
		BlockMetaData blockMetaData = b.GetBlockMetaData();
		List<List<Tile>> list = new List<List<Tile>>();
		list.Add(new List<Tile>
		{
			new Tile(new GAF("Meta.Then"))
		});
		List<List<Tile>> list2 = list;
		List<GAF> list3 = new List<GAF>();
		if (blockMetaData != null && blockMetaData.defaultGAFs != null)
		{
			GAFInfo[] defaultGAFs = blockMetaData.defaultGAFs;
			foreach (GAFInfo gAFInfo in defaultGAFs)
			{
				list3.Add(new GAF(gAFInfo.Predicate, gAFInfo.Args));
			}
		}
		if (blockMetaData != null && blockMetaData.handUse == HandAttachmentType.Shield)
		{
			list2[0].Insert(0, new Tile(new GAF("Prop.Blocking")));
			object[] args = new object[0];
			list3.Add(new GAF("Block.Invincible", args));
		}
		foreach (GAF item in list3)
		{
			list2[0].Add(new Tile(item));
		}
		if (list3.Count > 0)
		{
			list2.Add(new List<Tile>());
			list2[1].Add(new Tile(new GAF("Meta.Then")));
		}
		defaultExtraTiles[blockType] = list2;
	}

	public virtual void OnCreate()
	{
	}

	public virtual void OnReconstructed()
	{
	}

	public BlockMeshMetaData[] GetBlockMeshMetaDatas()
	{
		BlockMetaData blockMetaData = GetBlockMetaData();
		if (blockMetaData != null)
		{
			return blockMetaData.meshDatas;
		}
		return new BlockMeshMetaData[0];
	}

	public Vector3 GetDefaultScale()
	{
		string key = BlockType();
		if (!defaultScales.ContainsKey(key))
		{
			BlockMetaData blockMetaData = GetBlockMetaData();
			if (blockMetaData != null)
			{
				Vector3 vector = blockMetaData.defaultScale;
				if (Util.MinComponent(vector) < 0.1f)
				{
					vector = Vector3.one;
				}
				defaultScales[key] = vector;
			}
			else
			{
				defaultScales[key] = Vector3.one;
			}
		}
		return defaultScales[key];
	}

	public Vector3 GetDefaultOrientation()
	{
		string key = BlockType();
		if (!defaultOrientations.ContainsKey(key))
		{
			BlockMetaData blockMetaData = GetBlockMetaData();
			if (blockMetaData != null)
			{
				defaultOrientations[key] = blockMetaData.defaultOrientation;
			}
			else
			{
				defaultOrientations[key] = Vector3.zero;
			}
		}
		return defaultOrientations[key];
	}

	public Vector3[] GetDefaultTextureNormals()
	{
		string key = BlockType();
		if (!defaultTextureNormals.ContainsKey(key))
		{
			BlockMetaData blockMetaData = GetBlockMetaData();
			if (blockMetaData != null)
			{
				List<Vector3> list = new List<Vector3>();
				BlockMeshMetaData[] meshDatas = blockMetaData.meshDatas;
				foreach (BlockMeshMetaData blockMeshMetaData in meshDatas)
				{
					list.Add(blockMeshMetaData.defaultTextureNormal);
				}
				defaultTextureNormals[key] = list.ToArray();
			}
			else
			{
				defaultTextureNormals[key] = null;
			}
		}
		return defaultTextureNormals[key];
	}

	public BlockMetaData GetBlockMetaData()
	{
		if (meta == null && go != null)
		{
			meta = go.GetComponent<BlockMetaData>();
		}
		return meta;
	}

	public bool IsLocked()
	{
		if (tiles[1].Count > 1)
		{
			return tiles[1][1].IsLocked();
		}
		return false;
	}

	public string BlockType()
	{
		return (string)tiles[0][1].gaf.Args[0];
	}

	public int BlockItemId()
	{
		return tiles[0][1].gaf.BlockItemId;
	}

	public GAF BlockCreateGAF()
	{
		return tiles[0][1].gaf;
	}

	public Tile FindTile(GAF gaf)
	{
		foreach (List<Tile> tile in tiles)
		{
			foreach (Tile item in tile)
			{
				if (item.gaf.Equals(gaf))
				{
					return item;
				}
			}
		}
		return null;
	}

	public Vector3 BlockSize()
	{
		Vector3 vector = Vector3.one;
		string key = BlockType();
		if (!blockSizes.ContainsKey(key))
		{
			BlockMetaData blockMetaData = GetBlockMetaData();
			if (blockMetaData != null)
			{
				vector = blockMetaData.blockSize;
				if (Util.MinComponent(vector) < 0.001f)
				{
					vector = Vector3.one;
				}
			}
			blockSizes[key] = vector;
		}
		return blockSizes[key];
	}

	public Vector3 Scale()
	{
		if (tiles[0].Count > 6)
		{
			return (Vector3)tiles[0][6].gaf.Args[0];
		}
		return new Vector3(1f, 1f, 1f);
	}

	public Vector3 GetScale()
	{
		return size;
	}

	public float GetMass()
	{
		if (blockMassOverride >= 0f)
		{
			return blockMassOverride;
		}
		if (IsRuntimeInvisible())
		{
			return 0f;
		}
		BlockMetaData blockMetaData = GetBlockMetaData();
		if (!broken && chunk != null && null != blockMetaData && blockMetaData.isBlocksterMassless && chunk.HasCharacter())
		{
			return 0f;
		}
		if (blockMass > 0f)
		{
			return blockMass;
		}
		Vector3 vector = Scale();
		if (blockMassConstants == null)
		{
			blockMassConstants = new Dictionary<string, float[]>();
		}
		float num;
		float num2;
		switch (BlockType())
		{
		case "Terrain Wedge":
			num = 0f;
			num2 = 0.5f;
			break;
		case "Hemisphere":
			num = 0f;
			num2 = 0.2617995f;
			break;
		default:
			num = 0f;
			num2 = 1f;
			break;
		}
		float num3 = 0.75f + 0.25f * vector.x * vector.y * vector.z;
		string key = BlockType();
		if (!blockMassConstants.ContainsKey(key))
		{
			BlockMetaData blockMetaData2 = GetBlockMetaData();
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
			blockMassConstants.Add(key, new float[2] { num2, num });
		}
		float[] array = blockMassConstants[key];
		return blockMass = array[0] * num3 + array[1];
	}

	public virtual void Reset(bool forceRescale = false)
	{
		meshScale = Vector3.one;
		if (forceRescale)
		{
			meshScale = Vector3.zero;
		}
		meshScaleTexture = string.Empty;
		skipUpdateSATVolumes = true;
		if (tiles.Count > 0)
		{
			resetExecutionInfo.timer = 0f;
			resetExecutionInfo.floatArg = 1f;
			for (int i = 2; i < tiles[0].Count; i++)
			{
				tiles[0][i].Execute(this, resetExecutionInfo);
			}
		}
		else
		{
			BWLog.Info("Block reset() without necessary tiles");
		}
		skipUpdateSATVolumes = false;
		UpdateSATVolumes();
	}

	public virtual void ResetFrame()
	{
	}

	public void CheckContainsPlayModeTiles()
	{
		containsPlayModeTiles = false;
		if (notPlayModePredicates == null)
		{
			notPlayModePredicates = new HashSet<Predicate>
			{
				predicateLocked, predicateThen, predicateUnlocked, predicateTutorialCreateBlockHint, predicateTutorialRemoveBlockHint, predicateTutorialOperationPose, predicateTutorialPaintExistingBlock, predicateTutorialTextureExistingBlock, predicateTutorialRotateExistingBlock, predicateTutorialMoveBlock,
				predicateTutorialMoveModel
			};
		}
		containsPlayModeTiles = !BWSceneManager.PlayBlockPredicates(this).IsSubsetOf(notPlayModePredicates);
	}

	public virtual void Play()
	{
		broken = false;
		didFix = false;
		vanished = false;
		isTreasure = false;
		gluedOnContact = false;
		allowGlueOnContact = true;
		lastTeleportedFrameCount = -1;
		blockMassOverride = -1f;
		storedMassOverride = -1f;
		overridingMass = false;
		rbConstraintsOn = 0;
		rbConstraintsOff = 0;
		rbUpdatedConstraints = false;
		if (!go.activeSelf && activateForPlay)
		{
			go.SetActive(value: true);
		}
		Collider component = go.GetComponent<Collider>();
		component.contactOffset = 0.01f;
		colliderName = component.name;
		Transform transform = go.transform;
		Transform parent = transform.parent;
		playPosition = transform.position;
		oldPos = Util.nullVector3;
		playRotation = transform.rotation;
		if (parent != null)
		{
			parentPlayPosition = parent.position;
			parentPlayRotation = parent.rotation;
		}
		else
		{
			parentPlayPosition = playPosition;
			parentPlayRotation = playRotation;
		}
		animationStep = 0;
		lastShadowHitDistance = -2f;
		if (audioSource != null)
		{
			audioSource.Stop();
			audioSource.volume = 1f;
		}
		UpdatePhysicMaterialsForTextureAssignment();
		UpdateBlockPropertiesForAllTextureAssignments(forceEnabled: false);
		if (subMeshGameObjects.Count > 0)
		{
			SetSubMeshVisibility(t: true);
		}
		meshScaleTexture = string.Empty;
		isRuntimePhantom = false;
		goLayerAssignment = (isTerrain ? Layer.Terrain : Layer.Default);
		UpdateRuntimeInvisible();
		shadowUpdateCounter = UnityEngine.Random.Range(0, 1000);
		if (goShadow != null && CanMergeShadow())
		{
			float sqrMagnitude = Scale().sqrMagnitude;
			float num = 8f;
			if (sqrMagnitude < num * num)
			{
				for (int i = 0; i < connections.Count; i++)
				{
					Block block = connections[i];
					if (!block.CanMergeShadow() || Mathf.Abs(connectionTypes[i]) != 1)
					{
						continue;
					}
					float sqrMagnitude2 = block.shadowSize.sqrMagnitude;
					if (sqrMagnitude2 < 16f && (block.goShadow == null || sqrMagnitude2 < sqrMagnitude))
					{
						shadowSize += Vector3.one * Util.MinComponent(block.shadowSize);
						block.DestroyShadow();
						if (shadowSize.sqrMagnitude > num * num)
						{
							break;
						}
					}
				}
			}
		}
		for (int j = 0; j < subMeshPaints.Count + 1; j++)
		{
			RegisterPaintChanged(j, GetPaint(j), null);
			RegisterTextureChanged(j, GetTexture(j), null);
		}
	}

	protected virtual void UpdateRuntimeInvisible()
	{
		bool flag = false;
		for (int i = 0; i < ((childMeshes == null) ? 1 : (childMeshes.Count + 1)); i++)
		{
			string texture = GetTexture(i);
			if (texture != "Invisible")
			{
				flag = true;
				break;
			}
		}
		bool flag2 = isRuntimeInvisible;
		isRuntimeInvisible = !flag;
		go.layer = (int)(isTransparent ? Layer.TransparentFX : ((!isRuntimePhantom) ? goLayerAssignment : Layer.Phantom));
		if (flag2 != isRuntimeInvisible)
		{
			UpdateNeighboringConnections();
		}
	}

	public virtual bool IsRuntimeInvisible()
	{
		if (!vanished)
		{
			return isRuntimeInvisible;
		}
		return true;
	}

	public bool IsProfileCharacter()
	{
		return ProfileBlocksterUtils.IsProfileBlockType(BlockType());
	}

	public virtual bool CanMergeShadow()
	{
		return true;
	}

	public void MakeFixedAndSpawnpointBeforeFirstFrame()
	{
		if (frozenInTerrainStatus == -1)
		{
			UpdateFrozenInTerrainStatus();
		}
		if (frozenInTerrainStatus == 1 || IsFixed())
		{
			Freeze(null, null);
		}
		Predicate predicate = predicateSetSpawnPoint;
		Predicate predicate2 = predicateFirstFrame;
		for (int i = 0; i < tiles.Count; i++)
		{
			List<Tile> list = tiles[i];
			Predicate predicate3 = ((list.Count <= 0) ? null : list[0].gaf.Predicate);
			Predicate predicate4 = ((list.Count <= 1) ? null : list[1].gaf.Predicate);
			if (predicate3 != predicateThen && (predicate3 != predicate2 || predicate4 != predicateThen))
			{
				continue;
			}
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

	public void HandleHideOnPlay()
	{
		Predicate predicate = predicateFirstFrame;
		for (int i = 0; i < tiles.Count; i++)
		{
			List<Tile> list = tiles[i];
			Predicate predicate2 = ((list.Count <= 0) ? null : list[0].gaf.Predicate);
			Predicate predicate3 = ((list.Count <= 1) ? null : list[1].gaf.Predicate);
			if (predicate2 != predicate || predicate3 != predicateThen)
			{
				continue;
			}
			for (int j = 0; j < list.Count; j++)
			{
				GAF gaf = list[j].gaf;
				if (gaf.Predicate == predicateVanishModel)
				{
					VanishModel(0f, animate: false);
				}
				else if (gaf.Predicate == predicateVanishBlock)
				{
					VanishBlock(animate: false, 0f);
				}
			}
		}
	}

	private void UpdatePhysicMaterialsForTextureAssignment()
	{
		Collider component = go.GetComponent<Collider>();
		if (component != null)
		{
			PhysicMaterial physicMaterialTexture = MaterialTexture.GetPhysicMaterialTexture(GetTexture());
			if (physicMaterialTexture != null)
			{
				component.sharedMaterial = physicMaterialTexture;
			}
			else
			{
				component.sharedMaterial = GetDefaultPhysicMaterialForPrefabCollider();
			}
		}
		for (int i = 0; i < subMeshGameObjects.Count; i++)
		{
			component = subMeshGameObjects[i].GetComponent<Collider>();
			if (component != null)
			{
				PhysicMaterial physicMaterialTexture2 = MaterialTexture.GetPhysicMaterialTexture(GetTexture(i + 1));
				if (physicMaterialTexture2 != null)
				{
					component.sharedMaterial = physicMaterialTexture2;
				}
				else
				{
					component.sharedMaterial = GetDefaultPhysicMaterialForPrefabCollider(i + 1);
				}
			}
		}
	}

	protected virtual int GetPrimaryMeshIndex()
	{
		return 0;
	}

	protected void UpdateBlockPropertiesForAllTextureAssignments(bool forceEnabled)
	{
		for (int i = 0; i <= subMeshGameObjects.Count; i++)
		{
			UpdateBlockPropertiesForTextureAssignment(i, forceEnabled);
		}
	}

	protected virtual void UpdateBlockPropertiesForTextureAssignment(int meshIndex, bool forceEnabled)
	{
		int primaryMeshIndex = GetPrimaryMeshIndex();
		if (meshIndex == primaryMeshIndex)
		{
			bool flag = isTransparent;
			bool flag2 = isRuntimeInvisible;
			string texture = GetTexture(primaryMeshIndex);
			isTransparent = Materials.TextureIsTransparent(texture);
			if (texture == "Metal")
			{
				buoyancyMultiplier = 0.2f;
			}
			else
			{
				buoyancyMultiplier = GetBuoyancyMultiplier();
			}
			isRuntimeInvisible = texture == "Invisible";
			bool enabled = forceEnabled || !isRuntimeInvisible;
			if (goShadow != null)
			{
				goShadow.GetComponent<Renderer>().enabled = enabled;
			}
			Renderer component = go.GetComponent<Renderer>();
			if (component != null)
			{
				component.enabled = enabled;
			}
			if (flag != isTransparent || flag2 != isRuntimeInvisible)
			{
				UpdateNeighboringConnections();
			}
		}
		int num = meshIndex - 1;
		if (num >= 0 && num < subMeshGameObjects.Count)
		{
			string texture2 = GetTexture(num + 1);
			bool enabled2 = forceEnabled || texture2 != "Invisible";
			subMeshGameObjects[num].GetComponent<Renderer>().enabled = enabled2;
		}
	}

	public virtual void Play2()
	{
	}

	public virtual void Stop(bool resetBlock = true)
	{
		if (massAlteredBlocks.Contains(this))
		{
			massAlteredBlocks.Remove(this);
		}
		RestoreMeshColliderInfo();
		if (!Blocksworld.renderingShadows && HasShadow() && goShadow == null)
		{
			InstantiateShadow(prefabShadow);
		}
		shadowSize = size;
		if (goT.root != null && goT.parent != null)
		{
			GameObject gameObject = goT.parent.gameObject;
			Util.UnparentTransformSafely(goT);
			if (goT.root == goT.parent)
			{
				UnityEngine.Object.Destroy(gameObject);
			}
		}
		meshScaleTexture = string.Empty;
		modelBlock = null;
		if (go.GetComponent<Renderer>() != null && !go.GetComponent<Renderer>().enabled)
		{
			go.GetComponent<Renderer>().enabled = true;
		}
		if (go.GetComponent<Collider>() != null)
		{
			go.GetComponent<Collider>().enabled = true;
			go.GetComponent<Collider>().isTrigger = false;
		}
		if (goShadow != null)
		{
			goShadow.GetComponent<Renderer>().enabled = true;
		}
		go.layer = (int)goLayerAssignment;
		goT.localScale = Vector3.one;
		go.SetActive(activateForPlay);
		RewardVisualization.rewardAnimationRunning = false;
		didFix = false;
		vanished = false;
		isTreasure = false;
		broken = false;
		gluedOnContact = false;
		allowGlueOnContact = true;
		lastTeleportedFrameCount = -1;
		if (resetBlock)
		{
			Reset();
		}
		lastShadowHitDistance = -2f;
		shadowUpdateInterval = 1;
		if (audioSource != null)
		{
			audioSource.Stop();
			audioSource.volume = 0f;
		}
		if (subMeshGameObjects.Count > 0)
		{
			SetSubMeshVisibility(t: false);
		}
		UpdateBlockPropertiesForAllTextureAssignments(forceEnabled: true);
		chunk = null;
	}

	public virtual void Pause()
	{
	}

	public virtual void Resume()
	{
	}

	public virtual void Break(Vector3 chunkPos, Vector3 chunkVel, Vector3 chunkAngVel)
	{
		broken = true;
	}

	public virtual void OnHudMesh()
	{
	}

	public virtual void Update()
	{
		if (!Blocksworld.renderingShadows && hasShadow && !vanished)
		{
			UpdateShadow();
		}
	}

	private float TotalBlockAlpha()
	{
		float num = ((!Materials.TextureIsTransparent(GetTexture())) ? 1f : 0.3f);
		if (subMeshTextures.Count > 0)
		{
			for (int i = 0; i < subMeshTextures.Count; i++)
			{
				num += ((!Materials.TextureIsTransparent(subMeshTextures[i])) ? 1f : 0.3f);
			}
			num /= (float)(subMeshTextures.Count + 1);
		}
		return num;
	}

	protected void SetShadowAlpha(float alpha)
	{
		alpha *= shadowStrengthMultiplier * TotalBlockAlpha();
		if (alpha != oldShadowAlpha)
		{
			colorsShadow[0].a = alpha;
			colorsShadow[1].a = alpha;
			colorsShadow[2].a = alpha;
			colorsShadow[3].a = alpha;
			meshShadow.colors = colorsShadow;
			oldShadowAlpha = alpha;
		}
	}

	private bool UpdateShadowDefined(Vector3 goPos, float maxDist, ref bool shadowDefined)
	{
		Vector3 rhs = goPos - Blocksworld.cameraPosition;
		float sqrMagnitude = rhs.sqrMagnitude;
		float num = maxDist * maxDist;
		if (sqrMagnitude > num)
		{
			SetShadowAlpha(0f);
			lastShadowHitDistance = -3f;
			shadowUpdateInterval = Mathf.Min(10, 5 + Mathf.RoundToInt(0.1f * Mathf.Sqrt(sqrMagnitude - num)));
			return false;
		}
		if (lastShadowHitDistance == -3f)
		{
			lastShadowHitDistance = -2f;
			shadowUpdateInterval = 1;
			shadowDefined = false;
		}
		shadowBounds.center = goPos;
		shadowBounds.size = shadowSize;
		if (!GeometryUtility.TestPlanesAABB(Blocksworld.frustumPlanes, shadowBounds))
		{
			SetShadowAlpha(0f);
			lastShadowHitDistance = -4f;
			shadowUpdateInterval = 5;
			if (Vector3.Dot(Blocksworld.cameraForward, rhs) < 0f)
			{
				shadowUpdateInterval = 8;
			}
			return false;
		}
		if (lastShadowHitDistance == -4f)
		{
			lastShadowHitDistance = -2f;
			shadowUpdateInterval = 1;
			shadowDefined = false;
		}
		return true;
	}

	private void PlaceShadow(bool shadowDefined, bool moved, Vector3 goPos, float maxDist, Quaternion goRot)
	{
		RaycastHit hitInfo = default(RaycastHit);
		Vector3 vector = 0.45f * Vector3.up;
		bool flag = shadowDefined && lastShadowHitDistance >= 0f && lastShadowHitDistance < shadowMaxDistance && !moved;
		bool flag2 = flag;
		Vector3 vector2 = default(Vector3);
		Vector3 vector3 = default(Vector3);
		float num = 0f;
		if (flag)
		{
			num = lastShadowHitDistance;
			vector2 = lastShadowHitPoint;
			vector3 = oldShadowHitNormal;
		}
		else if (Physics.Raycast(goPos + vector, Vector3.down, out hitInfo, shadowMaxDistance, 16))
		{
			vector2 = hitInfo.point;
			vector3 = hitInfo.normal;
			num = hitInfo.distance;
			flag2 = true;
		}
		if (flag2)
		{
			lastShadowHitDistance = num;
			lastShadowHitPoint = vector2;
			Vector3 cameraPosition = Blocksworld.cameraPosition;
			if (Physics.Raycast(cameraPosition, lastShadowHitPoint - cameraPosition, out var hitInfo2, maxDist, 16))
			{
				Vector3 point = hitInfo2.point;
				if ((vector2 - point).sqrMagnitude > 1f && (cameraPosition - vector2).sqrMagnitude > (cameraPosition - point).sqrMagnitude)
				{
					SetShadowAlpha(0f);
					shadowUpdateInterval = 5;
					return;
				}
			}
			float shadowAlpha = Mathf.Clamp(0.3f - num * (0.3f / shadowMaxDistance), 0f, 1f);
			SetShadowAlpha(shadowAlpha);
			Vector3 vector4 = vector2;
			goShadowT.position = vector4;
			float num2 = ((moved && Blocksworld.CurrentState == State.Play) ? 0.95f : 0f);
			Vector3 vector5 = (oldShadowHitNormal = num2 * oldShadowHitNormal + (1f - num2) * vector3);
			if (shadowSize != Vector3.one)
			{
				Vector3 vector6 = Util.Abs(goRot * shadowSize);
				Vector3 up = goT.up;
				Vector3 forward = goT.forward;
				Vector3 right = goT.right;
				if (Mathf.Abs(up.y) > Mathf.Abs(forward.y) && Mathf.Abs(up.y) > Mathf.Abs(right.y))
				{
					goShadowT.localScale = new Vector3(shadowSize.x + 1f, 1f, shadowSize.z + 1f);
					vector6 = Util.ProjectOntoPlane(forward, vector3);
					goShadowT.LookAt(vector4 + vector6, vector5);
				}
				else if (Mathf.Abs(forward.y) > Mathf.Abs(right.y))
				{
					goShadowT.localScale = new Vector3(shadowSize.x + 1f, 1f, shadowSize.y + 1f);
					vector6 = Util.ProjectOntoPlane(up, vector3);
					goShadowT.LookAt(vector4 + vector6, vector5);
				}
				else
				{
					goShadowT.localScale = new Vector3(shadowSize.y + 1f, 1f, shadowSize.z + 1f);
					vector6 = Util.ProjectOntoPlane(forward, vector3);
					goShadowT.LookAt(vector4 + vector6, vector5);
				}
			}
			else
			{
				goShadowT.rotation = Quaternion.FromToRotation(Vector3.up, vector5);
			}
		}
		else if (lastShadowHitDistance != -1f)
		{
			lastShadowHitDistance = -1f;
			SetShadowAlpha(0f);
			shadowUpdateInterval = 4;
		}
	}

	private bool TryPruneShadow(Vector3 goPos, Quaternion goRot, ref bool shadowDefined, ref bool moved, ref float maxDist)
	{
		shadowUpdateInterval = 3;
		moved = (goPos - oldPos).sqrMagnitude > 1.0000001E-06f;
		if (moved)
		{
			oldPos = goPos;
		}
		if (!moved && lastShadowHitDistance == -1f)
		{
			shadowUpdateInterval = 5;
			SetShadowAlpha(0f);
			return true;
		}
		bool flag = new Vector4(goRot.x - oldRotation.x, goRot.y - oldRotation.y, goRot.z - oldRotation.z, goRot.w - oldRotation.w).sqrMagnitude > 0.0001f;
		if (flag)
		{
			oldRotation = goRot;
		}
		shadowDefined = lastShadowHitDistance != -2f;
		if ((!moved && !flag && !Blocksworld.cameraMoved) & shadowDefined)
		{
			return true;
		}
		maxDist = Mathf.Clamp(150f * Util.MaxComponent(shadowSize), Mathf.Min(150f, Blocksworld.fogEnd - 1f), Blocksworld.fogEnd);
		if (shadowDefined)
		{
			return !UpdateShadowDefined(goPos, maxDist, ref shadowDefined);
		}
		return false;
	}

	protected virtual void UpdateShadow()
	{
		if (didFix && lastShadowHitDistance == -1f)
		{
			return;
		}
		shadowUpdateCounter++;
		Vector3 position = goT.position;
		if (lastShadowHitDistance > 0f)
		{
			float num = position.y - lastShadowHitDistance + 0.45f;
			float f = num - lastShadowHitPoint.y;
			if ((double)Mathf.Abs(f) > 0.02)
			{
				shadowUpdateInterval = 1;
			}
			goShadowT.position = new Vector3(position.x, num, position.z);
		}
		if (shadowUpdateCounter % shadowUpdateInterval == 0)
		{
			bool shadowDefined = true;
			bool moved = true;
			float maxDist = 0f;
			Quaternion rotation = goT.rotation;
			if (!TryPruneShadow(position, rotation, ref shadowDefined, ref moved, ref maxDist))
			{
				PlaceShadow(shadowDefined, moved, position, maxDist, rotation);
			}
		}
	}

	public virtual void FixedUpdate()
	{
		if (chunk.rb != null && rbUpdatedConstraints)
		{
			chunk.rb.constraints = (RigidbodyConstraints)GetRigidbodyConstraintsMask();
			rbUpdatedConstraints = false;
		}
	}

	public void CreateFlattenTiles()
	{
		int count = GetRuntimeTiles().Count;
		if (executionInfos.Length != count)
		{
			Array.Resize(ref executionInfos, count);
		}
		for (int i = 0; i < count; i++)
		{
			ScriptRowExecutionInfo scriptRowExecutionInfo = new ScriptRowExecutionInfo(i, this);
			executionInfos[i] = scriptRowExecutionInfo;
		}
	}

	private void RunCondition(ScriptRowExecutionInfo info)
	{
		if (info.beforeThen)
		{
			info.RunRow();
		}
	}

	private void RunAction(ScriptRowExecutionInfo info)
	{
		if (!info.beforeThen)
		{
			info.RunRow();
		}
	}

	public void RunConditions()
	{
		for (int i = 1; i < executionInfos.Length; i++)
		{
			ScriptRowExecutionInfo scriptRowExecutionInfo = executionInfos[i];
			if (scriptRowExecutionInfo.predicateTiles.Length > 1)
			{
				RunCondition(executionInfos[i]);
			}
		}
	}

	public void RunActions()
	{
		for (int i = 1; i < executionInfos.Length; i++)
		{
			ScriptRowExecutionInfo scriptRowExecutionInfo = executionInfos[i];
			if (scriptRowExecutionInfo.predicateTiles.Length > 1)
			{
				RunAction(scriptRowExecutionInfo);
			}
		}
	}

	public void RunFirstFrameActions()
	{
		for (int i = 1; i < executionInfos.Length; i++)
		{
			List<Tile> list = GetRuntimeTiles()[i];
			if (list.Count <= 1)
			{
				continue;
			}
			ScriptRowExecutionInfo scriptRowExecutionInfo = executionInfos[i];
			bool flag = scriptRowExecutionInfo.predicateTiles[0] == predicateThen;
			flag |= scriptRowExecutionInfo.predicateTiles[0] == predicateFirstFrame;
			if (!flag)
			{
				for (int j = 0; scriptRowExecutionInfo.predicateTiles[j] != predicateThen; j++)
				{
					flag |= scriptRowExecutionInfo.predicateTiles[j] == predicateFirstFrame;
				}
			}
			if (flag)
			{
				RunCondition(scriptRowExecutionInfo);
				RunAction(scriptRowExecutionInfo);
			}
		}
	}

	public virtual void IgnoreRaycasts(bool value)
	{
		go.layer = (int)((!value) ? goLayerAssignment : Layer.IgnoreRaycast);
	}

	public Vector3 GetPosition()
	{
		return (Vector3)tiles[0][2].gaf.Args[0];
	}

	public virtual Quaternion GetRotation()
	{
		return Quaternion.Euler((Vector3)tiles[0][3].gaf.Args[0]);
	}

	public virtual void EnableCollider(bool value)
	{
		if (go != null)
		{
			go.GetComponent<Collider>().enabled = value;
		}
	}

	public bool IsColliderEnabled()
	{
		if (go != null)
		{
			return go.GetComponent<Collider>().enabled;
		}
		return false;
	}

	public HashSet<string> GetNoShapeCollideClasses()
	{
		BlockMetaData blockMetaData = GetBlockMetaData();
		if (blockMetaData != null)
		{
			string[] noShapeCollideClasses = blockMetaData.noShapeCollideClasses;
			if (noShapeCollideClasses.Length != 0)
			{
				return new HashSet<string>(noShapeCollideClasses);
			}
		}
		return emptySet;
	}

	public HashSet<string> GetShapeCategories()
	{
		BlockMetaData blockMetaData = GetBlockMetaData();
		if (blockMetaData != null)
		{
			string[] shapeCategories = blockMetaData.shapeCategories;
			if (shapeCategories.Length != 0)
			{
				return new HashSet<string>(shapeCategories);
			}
		}
		return emptySet;
	}

	public bool IsAnimatedCharacterAttachment(Block characterBlock)
	{
		BlockMetaData blockMetaData = GetBlockMetaData();
		if (!(blockMetaData == null))
		{
			if (!blockMetaData.isBlocksterMassless)
			{
				if (!GetType().IsSubclassOf(typeof(Block)))
				{
					return characterBlock.goT.InverseTransformPoint(goT.position).y < 0f;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public bool IsColliderHit(Collider other)
	{
		return go.GetComponent<Collider>() == other;
	}

	public virtual bool IsColliding(float terrainOffset = 0f, HashSet<Block> exclude = null)
	{
		Bounds bounds = GetBounds();
		if (isTerrain || GetNoShapeCollideClasses().Contains("Terrain") || !Util.PointWithinTerrain(bounds.center - new Vector3(0f, bounds.size.y * terrainOffset, 0f)))
		{
			return CollisionTest.Collision(this, exclude);
		}
		return true;
	}

	public bool ContainsBlock(Block block)
	{
		return this == block;
	}

	public virtual void UpdateSATVolumes()
	{
		if (!skipUpdateSATVolumes)
		{
			string key = BlockType();
			CreateCollisionMeshes(ref glueMeshes, Blocksworld.glues[key]);
			CreateCollisionMeshes(ref shapeMeshes, Blocksworld.shapes[key]);
			CreateCollisionMeshes(ref jointMeshes, Blocksworld.joints[key]);
		}
	}

	protected virtual void TranslateSATVolumes(Vector3 offset)
	{
		if (!skipUpdateSATVolumes)
		{
			CollisionVolumes.TranslateMeshes(glueMeshes, offset);
			CollisionVolumes.TranslateMeshes(shapeMeshes, offset);
			CollisionVolumes.TranslateMeshes(jointMeshes, offset);
		}
	}

	private void CreateCollisionMeshes(ref CollisionMesh[] meshes, GameObject prefab)
	{
		if (prefab == null)
		{
			meshes = new CollisionMesh[0];
			return;
		}
		Vector3 scale = CollisionVolumesScale(meshes);
		if (prefab.name == "Joint Motor Cube")
		{
			scale = new Vector3(1f, scale.y, 1f);
		}
		CollisionVolumes.FromPrefab(prefab, goT, scale, ref meshes);
	}

	protected virtual Vector3 CollisionVolumesScale(CollisionMesh[] meshes)
	{
		return Scale();
	}

	public TileResultCode MoveToAction(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (MoveTo((Vector3)args[0]))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public virtual void TBoxStartRotate()
	{
	}

	public virtual void TBoxStopRotate()
	{
	}

	public virtual void TBoxStopScale()
	{
	}

	public virtual bool TBoxMoveTo(Vector3 pos, bool forced = false)
	{
		return MoveTo(pos);
	}

	public virtual void UpdateFrozenInTerrainStatus()
	{
		frozenInTerrainStatus = (IsFixedInTerrain() ? 1 : 0);
	}

	public virtual bool MoveTo(Vector3 pos)
	{
		if (Util.IsNullVector3(pos))
		{
			return true;
		}
		Vector3 vector = pos - goT.position;
		goT.position = pos;
		object[] args = tiles[0][2].gaf.Args;
		Vector3 vector2 = (Vector3)args[0];
		args[0] = pos;
		if (vector != Vector3.zero)
		{
			TranslateSATVolumes(vector);
		}
		if ((pos - vector2).sqrMagnitude > 0.0001f)
		{
			frozenInTerrainStatus = -1;
		}
		lastShadowHitDistance = -2f;
		shadowUpdateInterval = 1;
		return true;
	}

	public TileResultCode RotateToAction(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (RotateTo(Quaternion.Euler((Vector3)args[0])))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public virtual bool TBoxRotateTo(Quaternion rot)
	{
		return RotateTo(rot);
	}

	public virtual bool RotateTo(Quaternion rot)
	{
		bool flag = rot != goT.rotation;
		goT.rotation = rot;
		Vector3 eulerAngles = rot.eulerAngles;
		eulerAngles = Util.Round(eulerAngles * 100f) / 100f;
		object[] args = tiles[0][3].gaf.Args;
		Vector3 vector = (Vector3)args[0];
		args[0] = eulerAngles;
		if (flag)
		{
			UpdateSATVolumes();
		}
		if ((eulerAngles - vector).sqrMagnitude > 0.0001f)
		{
			frozenInTerrainStatus = -1;
		}
		lastShadowHitDistance = -2f;
		shadowUpdateInterval = 1;
		return true;
	}

	public Vector3 CanScale()
	{
		Vector3 value = Vector3.zero;
		string text = BlockType();
		if (canScales == null)
		{
			canScales = new Dictionary<string, Vector3>();
		}
		switch (text)
		{
		case "Terrain Cube":
		case "Terrain Wedge":
		case "smoothCone":
		case "Crenelation":
			return Vector3.one;
		default:
			if (!canScales.ContainsKey(text))
			{
				BlockMetaData blockMetaData = GetBlockMetaData();
				if (blockMetaData != null)
				{
					value = blockMetaData.canScale;
				}
				canScales.Add(text, value);
			}
			return canScales[text];
		}
	}

	public bool IsScaled()
	{
		return isScaled;
	}

	public TileResultCode ScaleToAction(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (ScaleTo((Vector3)args[0]))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public virtual bool TBoxScaleTo(Vector3 scale, bool recalculateCollider = true, bool forceRescale = false)
	{
		return ScaleTo(scale, recalculateCollider, forceRescale);
	}

	public Vector3 LimitScale(ref Vector3 s)
	{
		BlockMetaData blockMetaData = GetBlockMetaData();
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

	public virtual bool ScaleTo(Vector3 scale, bool recalculateCollider = true, bool forceRescale = false)
	{
		scale.x = Mathf.Abs(scale.x);
		scale.y = Mathf.Abs(scale.y);
		scale.z = Mathf.Abs(scale.z);
		LimitScale(ref scale);
		bool flag = scale != meshScale;
		isScaled = scale != Vector3.one;
		string text = BlockType();
		blockMass = -1f;
		if (scale != colliderScale)
		{
			if (!Blocksworld.colliders.ContainsKey(text))
			{
				BWLog.Info("Could not find collider for " + text);
			}
			if (CanScaleMesh(0))
			{
				Collider component = go.GetComponent<Collider>();
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
								BWLog.Info("Block " + go.name + " prefab is missing a mesh on its mesh collider");
							}
							else
							{
								meshCollider2.sharedMesh = null;
								meshCollider2.sharedMesh = UseColliderMesh(meshCollider.sharedMesh, scale);
							}
						}
					}
					CapsuleCollider capsuleCollider = Blocksworld.colliders[text] as CapsuleCollider;
					if (capsuleCollider != null)
					{
						CapsuleCollider capsuleCollider2 = component as CapsuleCollider;
						capsuleCollider2.height = scale.x;
						capsuleCollider2.radius = 0.5f * scale.z;
					}
				}
				else
				{
					BWLog.Info("Could not find a collider on " + go.name + " when scaling mesh");
				}
			}
			colliderScale = scale;
		}
		if (flag || forceRescale)
		{
			string scaleType = string.Empty;
			BlockMetaData blockMetaData = GetBlockMetaData();
			if (blockMetaData != null)
			{
				scaleType = blockMetaData.scaleType;
			}
			if (CanScaleMesh(0))
			{
				ScaleMesh(scale, GetTexture(), text, mesh, scaleType);
			}
			if (childMeshes != null)
			{
				int num = 1;
				foreach (KeyValuePair<string, Mesh> childMesh in childMeshes)
				{
					if (CanScaleMesh(num))
					{
						ScaleMesh(scale, GetTexture(num), childMesh.Key, childMesh.Value, scaleType);
					}
					num++;
				}
			}
			Vector3 vector = Vector3.zero;
			if (tiles[0].Count > 6)
			{
				object[] args = tiles[0][6].gaf.Args;
				vector = (Vector3)args[0];
				args[0] = scale;
			}
			else
			{
				tiles[0].Add(new Tile(new GAF("Block.ScaleTo", scale)));
			}
			if ((vector - scale).sqrMagnitude > 0.0001f)
			{
				frozenInTerrainStatus = -1;
			}
		}
		if (flag)
		{
			UpdateSATVolumes();
		}
		if (!Blocksworld.renderingShadows)
		{
			UpdateShadowMaxDistance(GetPaint());
		}
		meshScale = scale;
		size = BlockSize();
		size.Scale(scale);
		CalculateMaxExtent();
		shadowSize = size;
		return true;
	}

	public float CalculateMaxExtent()
	{
		return Mathf.Max(size.x, size.y, size.z) * 1.7320508f;
	}

	private void ScaleMeshUsingColors(Vector3[] vertices, Vector3[] verticesPrefab, Vector3 scale, Color[] colors)
	{
		for (int i = 0; i < vertices.Length; i++)
		{
			float x = ((colors[i].r != 0f) ? (verticesPrefab[i].x * scale.x) : (verticesPrefab[i].x + Mathf.Sign(verticesPrefab[i].x) * 0.5f * (scale.x - 1f)));
			float y = ((colors[i].g != 0f) ? (verticesPrefab[i].y * scale.y) : (verticesPrefab[i].y + Mathf.Sign(verticesPrefab[i].y) * 0.5f * (scale.y - 1f)));
			float z = ((colors[i].b != 0f) ? (verticesPrefab[i].z * scale.z) : (verticesPrefab[i].z + Mathf.Sign(verticesPrefab[i].z) * 0.5f * (scale.z - 1f)));
			vertices[i] = new Vector3(x, y, z);
		}
	}

	private void ScaleMeshMotor(Vector3[] vertices, Vector3[] verticesPrefab, Vector3 scale)
	{
		for (int i = 0; i < vertices.Length; i++)
		{
			vertices[i] = new Vector3(verticesPrefab[i].x + Mathf.Sign(verticesPrefab[i].x) * 0.5f * (scale.x - 1f), verticesPrefab[i].y + Mathf.Sign(verticesPrefab[i].y) * 0.5f * (scale.y - 1f), verticesPrefab[i].z + Mathf.Sign(verticesPrefab[i].z) * 0.5f * (scale.z - 1f));
		}
	}

	private void ScaleMeshDefault(Vector3[] vertices, Vector3[] verticesPrefab, Vector3 scale)
	{
		for (int i = 0; i < vertices.Length; i++)
		{
			vertices[i] = new Vector3(verticesPrefab[i].x + Mathf.Sign(verticesPrefab[i].x) * 0.5f * (scale.x - 1f), verticesPrefab[i].y + Mathf.Sign(verticesPrefab[i].y) * 0.5f * (scale.y - 1f), verticesPrefab[i].z + Mathf.Sign(verticesPrefab[i].z) * 0.5f * (scale.z - 1f));
		}
	}

	private void ScaleMeshDefault(Vector3[] vertices, Vector3[] verticesPrefab, Vector3 scale, Vector2[] uvs, Mapping mapping, Vector3 uvWrapScale, bool textureFourSidesIgnoreRightLeft)
	{
		ScaleMeshDefault(vertices, verticesPrefab, scale);
		if (uvs == null || vertices.Length != uvs.Length)
		{
			return;
		}
		int num = 0;
		if (mapping == Mapping.TwoSidesWrapTo1x1)
		{
			switch (Materials.FindSide(GetTextureNormal()))
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
			if (uvs[i].x < 1f / 3f)
			{
				uvs[i].x = Mathf.Repeat(uvs[i].x * 6f, 1f);
				uvs[i].x = uvs[i].x + Mathf.Sign(uvs[i].x - 0.5f) * 0.5f * (uvWrapScale.x - 1f) + ((Mathf.Round(uvWrapScale.x) % 2f != 0f) ? 0f : 0.5f);
				uvs[i].y = uvs[i].y + Mathf.Sign(uvs[i].y - 0.5f) * 0.5f * (uvWrapScale.y - 1f) + ((Mathf.Round(uvWrapScale.y) % 2f != 0f) ? 0f : 0.5f);
				if (num != 0 && num != 1)
				{
					uvs[i] = Vector2.zero;
				}
			}
			else if (uvs[i].x >= 2f / 3f)
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

	private void ScaleMeshUniform(Vector3[] vertices, Vector3[] verticesPrefab, Vector3 scale)
	{
		for (int i = 0; i < vertices.Length; i++)
		{
			vertices[i] = new Vector3(verticesPrefab[i].x * scale.x, verticesPrefab[i].y * scale.y, verticesPrefab[i].z * scale.z);
		}
	}

	private void ScaleMeshUniform(Vector3[] vertices, Vector3[] verticesPrefab, Vector3 scale, Vector2[] uvs, Mapping mapping, Vector3 uvWrapScale, bool textureFourSidesIgnoreRightLeft)
	{
		ScaleMeshUniform(vertices, verticesPrefab, scale);
		if (uvs != null)
		{
			ScaleUVsUniform(uvs, mapping, uvWrapScale, textureFourSidesIgnoreRightLeft);
		}
	}

	public virtual void ScaleMesh(Vector3 scale, string texture, string type, Mesh mesh, string scaleType = "")
	{
		if (this is BlockAnimatedCharacter || this is BlockCharacter)
		{
			return;
		}
		if (!Blocksworld.meshes.TryGetValue(type, out var value))
		{
			BWLog.Info("Count not find mesh for type " + type);
			return;
		}
		Vector3[] vertices = value.vertices;
		Vector3[] vertices2 = mesh.vertices;
		if (vertices2.Length != vertices.Length)
		{
			BWLog.Info("vertices.Length != verticesPrefab.Length in ScaleMesh() " + BlockType() + " " + vertices2.Length + " " + vertices.Length + " for " + mesh.name + " vs. " + value.name);
		}
		bool flag = this is BlockAbstractWheel || this is BlockRaycastWheel;
		Vector2[] array = null;
		Mapping mapping = Materials.GetMapping(texture);
		Vector3 vector = scale;
		if (!isTerrain && !fakeTerrain && (mapping == Mapping.AllSidesTo1x1 || mapping == Mapping.FourSidesTo1x1 || mapping == Mapping.TwoSidesWrapTo1x1) && !flag && Materials.uvWraps.ContainsKey(type))
		{
			array = Materials.CopyUVs(Materials.uvWraps[type]);
			vector = GetWrapScale(texture, scale);
		}
		Vector3 vector2 = new Vector3(vector.x / scale.x, vector.y / scale.y, vector.z / scale.z);
		bool flag2 = Materials.FourSidesIgnoreRightLeft(texture);
		float num = Mathf.Min(scale.y, scale.z);
		bool flag5;
		bool flag4;
		bool flag7;
		bool flag8;
		bool flag3;
		bool flag6;
		float num3;
		float num4;
		float num2;
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
			ScaleMeshUniform(vertices2, vertices, scale, array, mapping, vector, flag2);
			break;
		case "Slice":
		case "Slice Inverse":
			ScaleMeshUsingColors(vertices2, vertices, scale, value.colors);
			if (array != null)
			{
				ScaleUVsUniform(array, mapping, vector, flag2);
			}
			break;
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
			flag5 = type == "Sphere";
			flag4 = type == "Hemisphere";
			flag7 = type == "Wheel" || type == "Raycast Wheel";
			flag8 = type == "Bulky Wheel";
			flag3 = type == "Cylinder";
			flag6 = type == "Rocket";
			bool flag9 = flag || type == "Bulky Wheel Outer" || type == "Bulky Wheel Inner";
			num3 = ((!flag9) ? Mathf.Min(scale.x, scale.z) : Mathf.Min(scale.y, scale.z));
			num4 = num3;
			if (flag9)
			{
				num4 = Mathf.Clamp(num3, 1f, scale.x);
			}
			num2 = num3;
			if (!flag6)
			{
				switch (type)
				{
				case "smoothCone":
				case "octagonalCone":
				case "Pyramid":
					break;
				default:
					goto IL_0abd;
				}
			}
			num2 = Mathf.Clamp(num3, 1f, scale.y);
			goto IL_0abd;
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
		{
			for (int l = 0; l < vertices2.Length; l++)
			{
				vertices2[l] = new Vector3(vertices[l].x * scale.x, vertices[l].y * num, vertices[l].z * num);
			}
			break;
		}
		case "Treads X P":
		case "Treads X N":
		case "Bulky Wheel X N":
		case "Bulky Wheel X P":
		{
			for (int k = 0; k < vertices2.Length; k++)
			{
				vertices2[k] = new Vector3(vertices[k].x * num, vertices[k].y * scale.x, vertices[k].z * num);
			}
			break;
		}
		case "Motor Cube Axle":
		{
			for (int i = 0; i < vertices2.Length; i++)
			{
				vertices2[i] = new Vector3(vertices[i].x, vertices[i].y * scale.y, vertices[i].z);
			}
			break;
		}
		default:
			{
				switch (scaleType)
				{
				case "Uniform":
					ScaleMeshUniform(vertices2, vertices, scale, array, mapping, vector, flag2);
					break;
				case "UniformUV":
					fakeTerrain = true;
					ScaleMeshUniform(vertices2, vertices, scale, null, mapping, vector, flag2);
					break;
				case "Colors":
					ScaleMeshUsingColors(vertices2, vertices, scale, value.colors);
					if (array != null)
					{
						ScaleUVsUniform(array, mapping, vector, flag2);
					}
					break;
				default:
					ScaleMeshDefault(vertices2, vertices, scale, array, mapping, vector, flag2);
					break;
				}
				break;
			}
			IL_0abd:
			if (flag3 || type == "octagonalCylinder")
			{
				num2 = Mathf.Clamp(num3, 1f, 5f * scale.y);
			}
			if (flag4)
			{
				num4 = scale.x * 0.5f;
				num2 = scale.y * 0.5f;
				num3 = scale.z * 0.5f;
			}
			for (int j = 0; j < vertices2.Length; j++)
			{
				if (flag5 || flag4)
				{
					vertices2[j] = new Vector3(vertices[j].x * scale.x, vertices[j].y * scale.y, vertices[j].z * scale.z);
				}
				else
				{
					float x = vertices[j].x * num4 + Mathf.Sign(vertices[j].x) * 0.5f * (scale.x - num4);
					float y = vertices[j].y * num2 + Mathf.Sign(vertices[j].y) * 0.5f * (scale.y - num2);
					float z = vertices[j].z * num3 + Mathf.Sign(vertices[j].z) * 0.5f * (scale.z - num3);
					if (flag3 && (j == 96 || j == 165))
					{
						z = (x = 0f);
					}
					if (flag6 && (j == 225 || j == 246))
					{
						z = (x = 0f);
					}
					vertices2[j] = new Vector3(x, y, z);
				}
				if (array == null)
				{
					continue;
				}
				if (array[j].x < 1f / 3f)
				{
					array[j].x = Mathf.Repeat(array[j].x * 6f, 1f);
					if (flag4)
					{
						float num5 = Mathf.Max(Mathf.Round(0.7f * Mathf.Max(scale.x, scale.z) / scale.y), 1f);
						array[j].x = array[j].x * scale.x * vector2.x;
						array[j].y = array[j].y * scale.y * 0.5f * num5 * vector2.y;
					}
					else
					{
						array[j].x = vector2.x * (array[j].x * num4 + Mathf.Sign(array[j].x - 0.5f) * 0.5f * (scale.x - num4) + ((Mathf.Round(scale.x) % 2f != 0f) ? 0f : 0.5f));
						array[j].y = array[j].y + Mathf.Sign(array[j].y - 0.5f) * 0.5f * (vector.y - 1f) + ((Mathf.Round(vector.y) % 2f != 0f) ? 0f : 0.5f);
					}
				}
				else if (array[j].x >= 2f / 3f)
				{
					if (mapping == Mapping.FourSidesTo1x1 && !flag2)
					{
						array[j].x = 0f;
						array[j].y = 0f;
						continue;
					}
					array[j].x = Mathf.Repeat(array[j].x * 6f, 1f);
					if (flag4)
					{
						array[j].x = array[j].x * scale.x * vector2.x;
						array[j].y = array[j].y * scale.z * vector2.z;
					}
					else
					{
						array[j].x = vector2.x * (array[j].x * num4 + Mathf.Sign(array[j].x - 0.5f) * 0.5f * (scale.x - num4) + ((Mathf.Round(scale.x) % 2f != 0f) ? 0f : 0.5f));
						array[j].y = vector2.z * (array[j].y * num3 + Mathf.Sign(array[j].y - 0.5f) * 0.5f * (scale.z - num3) + ((Mathf.Round(scale.z) % 2f != 0f) ? 0f : 0.5f));
					}
					if ((flag3 && (j == 60 || j == 81)) || (flag6 && (j == 120 || j == 141)))
					{
						array[j].x = vector2.x * (0.5f * num4 + ((Mathf.Round(scale.x) % 2f != 0f) ? 0f : 0.5f));
						array[j].y = vector2.z * (0.5f * num3 + ((Mathf.Round(scale.z) % 2f != 0f) ? 0f : 0.5f));
					}
				}
				else if (mapping == Mapping.FourSidesTo1x1 && flag2)
				{
					array[j].x = 0f;
					array[j].y = 0f;
				}
				else
				{
					array[j].x = Mathf.Repeat(array[j].x * 6f, 1f);
					if (flag4)
					{
						float num6 = Mathf.Max(Mathf.Round(0.7f * Mathf.Max(scale.x, scale.z) / scale.y), 1f);
						array[j].x = array[j].x * scale.z * vector2.z;
						array[j].y = array[j].y * scale.y * 0.5f * num6 * vector2.y;
					}
					else
					{
						array[j].x = vector2.z * (array[j].x * num3 + Mathf.Sign(array[j].x - 0.5f) * 0.5f * (scale.z - num3) + ((Mathf.Round(scale.z) % 2f != 0f) ? 0f : 0.5f));
						array[j].y = array[j].y + Mathf.Sign(array[j].y - 0.5f) * 0.5f * (vector.y - 1f) + ((Mathf.Round(vector.y) % 2f != 0f) ? 0f : 0.5f);
					}
					if ((flag7 || flag8) && (j == 100 || j == 161))
					{
						array[j].x = vector2.z * 0.5f * num3 + ((Mathf.Round(scale.z) % 2f != 0f) ? 0f : 0.5f);
						array[j].y = vector2.y * 0.5f * num2 + ((Mathf.Round(scale.y) % 2f != 0f) ? 0f : 0.5f);
					}
				}
			}
			break;
		}
		mesh.vertices = vertices2;
		if (array != null)
		{
			mesh.uv = array;
		}
		mesh.RecalculateBounds();
	}

	protected void ResizeMeshForBlock(Mesh mesh)
	{
		Vector3[] vertices = mesh.vertices;
		Vector3[] vertices2 = new Vector3[mesh.vertices.Length];
		string text = string.Empty;
		BlockMetaData blockMetaData = GetBlockMetaData();
		if (blockMetaData != null)
		{
			text = blockMetaData.scaleType;
		}
		if (text == "Uniform")
		{
			ScaleMeshUniform(vertices2, vertices, Scale());
		}
		else
		{
			ScaleMeshDefault(vertices2, vertices, Scale());
		}
		mesh.vertices = vertices2;
		mesh.RecalculateBounds();
	}

	private static float GetWrapScaleComponent(float s, float p)
	{
		float result = 1f;
		if (s > 1.01f)
		{
			result = ((!(s <= p)) ? ((float)Mathf.RoundToInt(s / p)) : 1f);
		}
		return result;
	}

	public static Vector3 GetWrapScale(string texture, Vector3 scale)
	{
		Vector3 result = scale;
		if (Materials.wrapTexturePrefSizes.TryGetValue(texture, out var value))
		{
			result.x = GetWrapScaleComponent(scale.x, value.x);
			result.y = GetWrapScaleComponent(scale.y, value.y);
			result.z = GetWrapScaleComponent(scale.z, value.z);
		}
		return result;
	}

	private void ScaleUVsUniform(Vector2[] uvs, Mapping mapping, Vector3 scale, bool textureFourSidesIgnoreRightLeft)
	{
		for (int i = 0; i < uvs.Length; i++)
		{
			if (uvs[i].x < 1f / 3f)
			{
				uvs[i].x = Mathf.Repeat(uvs[i].x * 6f, 1f);
				uvs[i].x = uvs[i].x * scale.x;
				uvs[i].y = uvs[i].y * scale.y;
			}
			else if (uvs[i].x >= 2f / 3f)
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

	private Mesh UseColliderMesh(Mesh meshPrefab, Vector3 scale)
	{
		Vector3[] vertices = meshPrefab.vertices;
		int[] triangles = meshPrefab.triangles;
		Mesh mesh = new Mesh();
		string text = BlockType();
		bool flag;
		if (text != null)
		{
			if (f__switch_map3 == null)
			{
				f__switch_map3 = new Dictionary<string, int>(12)
				{
					{ "Cylinder", 0 },
					{ "octagonalCylinder", 0 },
					{ "Rocket", 0 },
					{ "smoothCone", 0 },
					{ "octagonalCone", 0 },
					{ "Wheel", 0 },
					{ "Raycast Wheel", 0 },
					{ "Bulky Wheel", 0 },
					{ "Golden Wheel", 0 },
					{ "Pyramid", 0 },
					{ "Spoked Wheel", 0 },
					{ "RAR Moon Rover Wheel", 0 }
				};
			}
			if (f__switch_map3.TryGetValue(text, out var value) && value == 0)
			{
				flag = true;
				goto IL_00e2;
			}
		}
		flag = false;
		goto IL_00e2;
		IL_00e2:
		bool flag2 = this is BlockAbstractWheel;
		if (flag || flag2)
		{
			float num = ((!flag2) ? Mathf.Min(scale.x, scale.z) : Mathf.Min(scale.y, scale.z));
			float num2 = num;
			if (flag2)
			{
				num2 = Mathf.Clamp(num, 1f, scale.x);
			}
			float num3 = num;
			if (text == "Rocket")
			{
				num3 = Mathf.Clamp(num, 1f, scale.y);
			}
			for (int i = 0; i < vertices.Length; i++)
			{
				float x = vertices[i].x * num2 + Mathf.Sign(vertices[i].x) * 0.5f * (scale.x - num2);
				float y = vertices[i].y * num3 + Mathf.Sign(vertices[i].y) * 0.5f * (scale.y - num3);
				float z = vertices[i].z * num + Mathf.Sign(vertices[i].z) * 0.5f * (scale.z - num);
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

	public virtual string GetPaint(int meshIndex = 0)
	{
		if (meshIndex > 0)
		{
			return GetSubMeshPaint(meshIndex);
		}
		if (string.IsNullOrEmpty(currentPaint))
		{
			return (string)tiles[0][4].gaf.Args[0];
		}
		return currentPaint;
	}

	public string GetTexture(int meshIndex = 0)
	{
		if (BlockType().Contains("Volume Block") && Blocksworld.CurrentState != State.Play)
		{
			return "Volume";
		}
		if (meshIndex > 0)
		{
			return GetSubMeshTexture(meshIndex);
		}
		if (renderer != null)
		{
			Material sharedMaterial = renderer.sharedMaterial;
			if (sharedMaterial != null && Materials.materialCacheTexture.TryGetValue(sharedMaterial, out var value))
			{
				return value;
			}
		}
		return (string)tiles[0][5].gaf.Args[0];
	}

	public string GetBuildModeTexture(int meshIndex)
	{
		if (meshIndex == 0)
		{
			return (string)tiles[0][5].gaf.Args[0];
		}
		return subMeshTextures[meshIndex - 1];
	}

	public string GetBuildModePaint(int meshIndex)
	{
		if (meshIndex == 0)
		{
			return (string)tiles[0][4].gaf.Args[0];
		}
		return subMeshPaints[meshIndex - 1];
	}

	public Vector3 GetTextureNormal()
	{
		return (Vector3)tiles[0][5].gaf.Args[1];
	}

	public void SetTextureNormal(Vector3 angles)
	{
		tiles[0][5].gaf.Args[1] = angles;
	}

	public virtual TileResultCode IsPaintedTo(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (GetPaint() == (string)args[0])
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public virtual TileResultCode PaintToAction(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 1, 0);
		return PaintTo((string)args[0], permanent: false, intArg);
	}

	public TileResultCode IsSkyPaintedTo(ScriptRowExecutionInfo eInfo, object[] args)
	{
		BlockSky worldSky = Blocksworld.worldSky;
		if (worldSky != null)
		{
			if (worldSky.GetPaint() == (string)args[0])
			{
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}
		return TileResultCode.False;
	}

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
		return worldSky.PaintTo(stringArg, permanent: false, intArg);
	}

	private void UpdateShadowMaxDistance(string paint)
	{
		Vector3 v = shadowSize;
		bool flag = Blocksworld.IsLuminousPaint(paint);
		BlockMetaData blockMetaData = GetBlockMetaData();
		if (flag)
		{
			shadowMaxDistance = 1f + Mathf.Abs(Util.MinComponent(v));
			if (blockMetaData != null)
			{
				shadowStrengthMultiplier = blockMetaData.lightStrengthMultiplier;
			}
		}
		else
		{
			shadowMaxDistance = 4f + Mathf.Abs(Util.MinComponent(v));
			if (blockMetaData != null)
			{
				shadowStrengthMultiplier = blockMetaData.shadowStrengthMultiplier;
			}
		}
	}

	protected void UpdateShadowColors(string paint, string texture = null, string oldTexture = null)
	{
		if (meshShadow != null && go != null && go.GetComponent<Renderer>() != null && go.GetComponent<Renderer>().sharedMaterial != null)
		{
			Color color = Color.black;
			if (Blocksworld.IsLuminousPaint(paint))
			{
				color = go.GetComponent<Renderer>().sharedMaterial.GetColor("_Emission");
			}
			if (texture != null && Blocksworld.IsLuminousTexture(texture))
			{
				color += go.GetComponent<Renderer>().sharedMaterial.GetColor("_Color");
			}
			for (int i = 0; i < colorsShadow.Length; i++)
			{
				colorsShadow[i].r = color.r;
				colorsShadow[i].g = color.g;
				colorsShadow[i].b = color.b;
			}
			meshShadow.colors = colorsShadow;
			oldShadowAlpha = -1f;
			UpdateShadowMaxDistance(paint);
			bool flag = !vanished && (VisibleInPlayMode() || Blocksworld.CurrentState != State.Play);
			if (goShadow != null)
			{
				goShadow.GetComponent<Renderer>().enabled = flag && texture != null;
			}
		}
	}

	protected virtual void SetSubMeshVisibility(bool t)
	{
		for (int i = 0; i < subMeshGameObjects.Count; i++)
		{
			string texture = GetTexture(i + 1);
			if (texture == "Invisible")
			{
				subMeshGameObjects[i].GetComponent<Renderer>().enabled = !t;
			}
		}
	}

	internal void MakeInvisibleVisible()
	{
		for (int i = 0; i < ((childMeshes == null) ? 1 : (childMeshes.Count + 1)); i++)
		{
			string texture = GetTexture(i);
			if (texture == "Invisible")
			{
				Vector3 normal = ((i != 0) ? GetSubMeshTextureNormal(i) : GetTextureNormal());
				TextureTo("Glass", normal, permanent: true, i, force: true);
			}
		}
	}

	protected virtual void RegisterPaintChanged(int meshIndex, string paint, string oldPaint)
	{
		TextureAndPaintBlockRegistry.BlockPaintChanged(this, paint, oldPaint);
	}

	protected virtual void RegisterTextureChanged(int meshIndex, string texture, string oldTexture)
	{
		TextureAndPaintBlockRegistry.BlockTextureChanged(this, texture, oldTexture);
	}

	public TileResultCode PaintToAllMeshes(string paint, bool permanent)
	{
		TileResultCode result = PaintTo(paint, permanent);
		for (int i = 0; i < subMeshPaints.Count; i++)
		{
			PaintToSubMesh(paint, permanent, i + 1);
		}
		return result;
	}

	public virtual TileResultCode PaintTo(string paint, bool permanent, int meshIndex = 0)
	{
		if (meshIndex > 0)
		{
			PaintToSubMesh(paint, permanent, meshIndex);
			return TileResultCode.True;
		}
		string paint2 = GetPaint(meshIndex);
		currentPaint = paint;
		if (!string.IsNullOrEmpty(meshScaleTexture) && paint2 == paint)
		{
			return TileResultCode.True;
		}
		if (Blocksworld.CurrentState == State.Play)
		{
			RegisterPaintChanged(0, paint, paint2);
		}
		string texture = GetTexture();
		Vector3 textureNormal = GetTextureNormal();
		Vector3 scale = Scale();
		if (mesh != null)
		{
			Materials.SetMaterial(go, mesh, BlockType(), paint, texture, textureNormal, scale, meshScaleTexture);
		}
		skipUpdateSATVolumes = true;
		meshScaleTexture = texture;
		skipUpdateSATVolumes = false;
		if (permanent)
		{
			if (paint != null)
			{
				Tile tile = tiles[0][4];
				if (tile.IsShowing())
				{
					tile.Show(show: false);
					tile.gaf.Args[0] = paint;
					tile.Show(show: true);
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
			UpdateShadowColors(paint, texture, (Blocksworld.CurrentState != State.Build) ? texture : string.Empty);
		}
		return TileResultCode.True;
	}

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

	protected bool IsCharacterFaceTexture(string texture)
	{
		if (!texture.Contains("Face") && !(texture == "Robot"))
		{
			return texture == "Texture Jack O Lantern";
		}
		return true;
	}

	protected bool IsCharacterFaceWrapAroundTexture(string texture)
	{
		bool result = false;
		if (IsCharacterFaceTexture(texture))
		{
			Mapping mapping = Materials.GetMapping(texture);
			result = mapping == Mapping.AllSidesTo4x1;
		}
		return result;
	}

	public virtual TileResultCode IsTexturedTo(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (GetTexture() == (string)args[0])
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public virtual TileResultCode TextureToAction(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string newTexture = (string)args[0];
		Vector3 newNormal = (Vector3)args[1];
		int meshIndex = 0;
		if (args.Length > 2)
		{
			meshIndex = (int)args[2];
		}
		return TextureToAction(newTexture, newNormal, meshIndex);
	}

	public virtual TileResultCode TextureToAction(string newTexture, Vector3 newNormal, int meshIndex)
	{
		string texture = GetTexture(meshIndex);
		if (newTexture != texture || this is BlockSky)
		{
			if (meshIndex == 0 && Blocksworld.CurrentState == State.Play)
			{
				RegisterTextureChanged(meshIndex, newTexture, texture);
				if (Materials.IsMaterialShaderTexture(newTexture))
				{
					for (int i = meshIndex + 1; i <= subMeshGameObjects.Count; i++)
					{
						TextureTo(newTexture, newNormal, permanent: false, i);
					}
				}
			}
			TextureTo(newTexture, newNormal, permanent: false, meshIndex);
			newTexture = GetTexture(meshIndex);
			if (Materials.HasMapping(newTexture))
			{
				if (Materials.GetMapping(texture) != Materials.GetMapping(newTexture))
				{
					skipUpdateSATVolumes = true;
					skipUpdateSATVolumes = false;
				}
			}
			else
			{
				BWLog.Info("Could not find texture: " + newTexture);
			}
		}
		ScaleTo(Scale(), recalculateCollider: false, forceRescale: true);
		return TileResultCode.True;
	}

	private string SetMaterialTexture(string texture, int meshIndex)
	{
		if (this is BlockSky)
		{
			return texture;
		}
		if (texture == "Metal")
		{
			buoyancyMultiplier = 0.2f;
		}
		else
		{
			buoyancyMultiplier = GetBuoyancyMultiplier();
		}
		GameObject gameObject = go;
		int num = meshIndex - 1;
		if (num >= 0 && num < subMeshGameObjects.Count)
		{
			gameObject = subMeshGameObjects[num];
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
				component.sharedMaterial = GetDefaultPhysicMaterialForPrefabCollider();
			}
		}
		if (physicMaterialDefinition != null)
		{
			if (this is BlockTerrain || fakeTerrain)
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

	private PhysicMaterial GetDefaultPhysicMaterialForPrefabCollider(int meshIndex = 0)
	{
		GameObject gameObject = Blocksworld.prefabs[BlockType()];
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

	private Vector3 ApplyNormalChanges(string blockType, Vector3 normal, int meshIndex)
	{
		if (meshIndex == 0)
		{
			switch (blockType)
			{
			case "Slice Corner":
				if (normal.x > 0.99f || normal.z < -0.99f || normal.y < -0.99f)
				{
					return normal;
				}
				return Vector3.forward;
			case "Wedge":
			case "Slice":
			case "Slice Inverse":
				if (normal.z > 0.01f)
				{
					return new Vector3(0f, 1f, 1f);
				}
				return normal;
			}
		}
		return normal;
	}

	public virtual TileResultCode TextureTo(string texture, Vector3 normal, bool permanent, int meshIndex = 0, bool force = false)
	{
		FindSubMeshes();
		if (!force && texture != GetDefaultTexture(meshIndex))
		{
			if (MaterialTexture.IsPhysicMaterialTexture(texture))
			{
				if (meshIndex > 0 && !isTerrain)
				{
					return TileResultCode.False;
				}
				if (!isTerrain && !MaterialTexture.CanMaterialTextureNonTerrain(texture) && !Materials.IsNormalTerrainTexture(texture) && !fakeTerrain)
				{
					return TileResultCode.False;
				}
				if (canBeMaterialTextured != null && meshIndex < canBeMaterialTextured.Length && !canBeMaterialTextured[meshIndex])
				{
					return TileResultCode.False;
				}
			}
			else if (canBeTextured != null && meshIndex < canBeTextured.Length && !canBeTextured[meshIndex] && Materials.TextureRequiresUVs(texture))
			{
				return TileResultCode.False;
			}
			BlockMetaData blockMetaData = GetBlockMetaData();
			if (blockMetaData != null && blockMetaData.meshDatas.Length > meshIndex)
			{
				if (Materials.IsMaterialShaderTexture(texture))
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
		string text = BlockType();
		bool flag = normal == Vector3.zero;
		if (permanent && !flag)
		{
			if (TextureNormalRestriction.CanTextureBlockWithNormal(this, texture, text, normal, meshIndex, out var normalRewrite))
			{
				normal = normalRewrite;
			}
			else if (Materials.TextureRequiresUVs(texture))
			{
				return TileResultCode.False;
			}
		}
		texture = SetMaterialTexture(texture, meshIndex);
		texture = ApplyTextureChangeRules(texture, meshIndex, text);
		normal = ApplyNormalChanges(text, normal, meshIndex);
		string text2 = ((!permanent) ? GetTexture(meshIndex) : string.Empty);
		bool flag2 = text2 != texture;
		if (meshIndex > 0)
		{
			TextureToSubMesh(texture, normal, permanent, meshIndex);
			if (Blocksworld.CurrentState == State.Play && flag2)
			{
				UpdateBlockPropertiesForTextureAssignment(meshIndex, forceEnabled: false);
			}
			return TileResultCode.True;
		}
		if (flag)
		{
			normal = (Vector3)tiles[0][5].gaf.Args[1];
		}
		string paint = GetPaint();
		if (mesh != null)
		{
			Materials.SetMaterial(go, mesh, text, paint, texture, normal, Vector3.one, text2);
		}
		if (!Blocksworld.renderingShadows)
		{
			UpdateShadowColors(paint, texture, text2);
		}
		bool flag3 = isTransparent;
		isTransparent = Materials.TextureIsTransparent(texture);
		if (hasShadow && !vanished && flag2)
		{
			lastShadowHitDistance = -2f;
		}
		if (permanent)
		{
			if (texture != null)
			{
				Tile tile = tiles[0][5];
				normal = new Vector3(Mathf.Round(normal.x), Mathf.Round(normal.y), Mathf.Round(normal.z));
				if (tile.IsShowing())
				{
					tile.Show(show: false);
					tile.gaf.Args[0] = texture;
					tile.gaf.Args[1] = normal;
					tile.Show(show: true);
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
		if (isTerrain && !(this is BlockSky))
		{
			PaintTo(paint, permanent);
		}
		if (Blocksworld.CurrentState == State.Play && flag2)
		{
			UpdateBlockPropertiesForTextureAssignment(meshIndex, forceEnabled: false);
		}
		UpdateRuntimeInvisible();
		if (flag3 != isTransparent)
		{
			UpdateNeighboringConnections();
		}
		return TileResultCode.True;
	}

	public virtual TileResultCode TextureToSubMesh(string texture, Vector3 normal, bool permanent, int meshIndex = 0)
	{
		if (meshIndex > 0)
		{
			FindSubMeshes();
			int num = meshIndex - 1;
			if (num < subMeshGameObjects.Count)
			{
				GameObject gameObject = subMeshGameObjects[num];
				if (gameObject != null)
				{
					Mesh mesh = null;
					if (childMeshes != null && childMeshes.ContainsKey(gameObject.name))
					{
						mesh = childMeshes[gameObject.name];
					}
					else
					{
						MeshFilter component = gameObject.GetComponent<MeshFilter>();
						if (component != null)
						{
							mesh = component.sharedMesh;
						}
					}
					if (mesh != null)
					{
						string oldTexture = ((!permanent) ? subMeshTextures[num] : string.Empty);
						Materials.SetMaterial(gameObject, mesh, gameObject.name, GetPaint(meshIndex), texture, normal, Vector3.one, oldTexture);
						if (permanent)
						{
							SetSubmeshInitTextureToTile(meshIndex, texture, normal);
						}
						subMeshTextures[num] = texture;
						subMeshTextureNormals[num] = normal;
					}
					else
					{
						BWLog.Info("TextureToSubMesh: Unable to find submesh with index " + num);
					}
				}
			}
		}
		UpdateRuntimeInvisible();
		return TileResultCode.True;
	}

	public TileResultCode IsFirstFrame(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (Blocksworld.isFirstFrame)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsPullLockedSensor(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (PullObjectGesture.IsPullLocked(this))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public virtual TileResultCode PullLockBlock(ScriptRowExecutionInfo eInfo, object[] args)
	{
		PullObjectGesture.PullLock(this);
		return TileResultCode.True;
	}

	public TileResultCode PullLockChunk(ScriptRowExecutionInfo eInfo, object[] args)
	{
		for (int i = 0; i < chunk.blocks.Count; i++)
		{
			PullObjectGesture.PullLock(chunk.blocks[i]);
		}
		return TileResultCode.True;
	}

	public static void ClearConnectedCache()
	{
		connectedCache.Clear();
		connectedChunks.Clear();
	}

	public bool UpdateConnectedCache()
	{
		if (!connectedCache.ContainsKey(this))
		{
			List<Block> list = ConnectednessGraph.ConnectedComponent(this, 3);
			HashSet<Chunk> hashSet = new HashSet<Chunk>();
			HashSet<Block> hashSet2 = new HashSet<Block>(list);
			foreach (Block item3 in new List<Block>(list))
			{
				if (item3.broken && (item3.go == null || !item3.go.activeSelf))
				{
					continue;
				}
				hashSet.Add(item3.chunk);
				BlockGroup groupOfType = item3.GetGroupOfType("tank-treads");
				if (groupOfType != null && item3.IsMainBlockInGroup("tank-treads"))
				{
					Block[] blocks = groupOfType.GetBlocks();
					foreach (Block item in blocks)
					{
						if (!hashSet2.Contains(item))
						{
							list.Add(item);
							hashSet2.Add(item);
						}
					}
				}
				BlockGroup groupOfType2 = item3.GetGroupOfType("teleport-volume");
				if (groupOfType2 == null || !item3.IsMainBlockInGroup("teleport-volume"))
				{
					continue;
				}
				Block[] blocks2 = groupOfType2.GetBlocks();
				foreach (Block item2 in blocks2)
				{
					if (!hashSet2.Contains(item2))
					{
						list.Add(item2);
						hashSet2.Add(item2);
					}
				}
			}
			for (int k = 0; k < list.Count; k++)
			{
				Block block = list[k];
				block.modelBlock = list[0];
				connectedCache[block] = list;
				connectedChunks[block] = hashSet;
			}
			return true;
		}
		return false;
	}

	public TileResultCode SetSpawnpoint(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 0, 0);
		CheckpointSystem.SetSpawnPoint(this, intArg);
		return TileResultCode.True;
	}

	public TileResultCode SetActiveCheckpoint(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 0, 0);
		CheckpointSystem.SetActiveCheckPoint(this, intArg);
		return TileResultCode.True;
	}

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

	public TileResultCode PullLockModel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		UpdateConnectedCache();
		List<Block> list = connectedCache[this];
		foreach (Block item in list)
		{
			PullObjectGesture.PullLock(item);
		}
		return TileResultCode.True;
	}

	public TileResultCode IsFrozen(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (didFix || isTerrain)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsNotFrozen(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (!didFix && !isTerrain)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsPhantom(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (isRuntimePhantom)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsNotPhantom(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (isRuntimePhantom)
		{
			return TileResultCode.False;
		}
		return TileResultCode.True;
	}

	public TileResultCode SetPhantom(ScriptRowExecutionInfo eInfo, object[] args)
	{
		isRuntimePhantom = true;
		goLayerAssignment = Layer.Phantom;
		go.layer = 10;
		foreach (object item in go.transform)
		{
			Transform transform = (Transform)item;
			transform.gameObject.layer = 10;
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

	public TileResultCode SetUnphantom(ScriptRowExecutionInfo eInfo, object[] args)
	{
		isRuntimePhantom = false;
		goLayerAssignment = (isTerrain ? Layer.Terrain : Layer.Default);
		go.layer = (int)goLayerAssignment;
		foreach (object item in go.transform)
		{
			Transform transform = (Transform)item;
			transform.gameObject.layer = (int)goLayerAssignment;
		}
		if (this is BlockAnimatedCharacter)
		{
			BlockAnimatedCharacter blockAnimatedCharacter = this as BlockAnimatedCharacter;
			blockAnimatedCharacter.SetLayer(goLayerAssignment);
		}
		else if (this is BlockCharacter)
		{
			BlockCharacter blockCharacter = this as BlockCharacter;
			blockCharacter.SetLayer(goLayerAssignment);
		}
		else if (this is BlockProceduralCollider)
		{
			BlockProceduralCollider blockProceduralCollider = this as BlockProceduralCollider;
			blockProceduralCollider.SetLayer(goLayerAssignment);
		}
		return TileResultCode.True;
	}

	public TileResultCode IsPhantomModel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		UpdateConnectedCache();
		List<Block> list = connectedCache[this];
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
		return IsPhantom(eInfo, args);
	}

	public TileResultCode IsNotPhantomModel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		UpdateConnectedCache();
		List<Block> list = connectedCache[this];
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
		return IsNotPhantom(eInfo, args);
	}

	public TileResultCode SetPhantomModel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		isRuntimePhantom = true;
		UpdateConnectedCache();
		List<Block> list = connectedCache[this];
		for (int i = 0; i < list.Count; i++)
		{
			list[i].isRuntimePhantom = true;
			list[i].goLayerAssignment = Layer.Phantom;
			list[i].go.layer = 10;
			foreach (object item in list[i].go.transform)
			{
				Transform transform = (Transform)item;
				transform.gameObject.layer = 10;
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

	public TileResultCode SetUnphantomModel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		isRuntimePhantom = false;
		UpdateConnectedCache();
		List<Block> list = connectedCache[this];
		for (int i = 0; i < list.Count; i++)
		{
			list[i].isRuntimePhantom = false;
			list[i].goLayerAssignment = (list[i].isTerrain ? Layer.Terrain : Layer.Default);
			list[i].go.layer = (int)list[i].goLayerAssignment;
			foreach (object item in list[i].go.transform)
			{
				Transform transform = (Transform)item;
				transform.gameObject.layer = (int)list[i].goLayerAssignment;
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

	public bool IsFixed()
	{
		if (didFix || isTerrain)
		{
			return true;
		}
		for (int i = 0; i < tiles.Count; i++)
		{
			List<Tile> list = tiles[i];
			if (list.Count > 1 && list[0].gaf.Predicate == predicateThen && list[1].gaf.Predicate == predicateFreeze)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsFixedInTerrain()
	{
		BlockMetaData blockMetaData = GetBlockMetaData();
		if (blockMetaData != null && !blockMetaData.freezeInTerrain)
		{
			return false;
		}
		switch (BlockType())
		{
		case "Position":
		case "Volume Block":
		case "Teleport Volume Block":
			return false;
		default:
		{
			Bounds bounds = GetBounds();
			float num = 0.1f;
			float num2 = bounds.extents.x - num;
			float num3 = bounds.extents.y - num;
			float num4 = bounds.extents.z - num;
			Vector3 point = bounds.center + new Vector3(num2, 0f - num3, num4);
			Vector3 point2 = bounds.center + new Vector3(num2, 0f - num3, 0f - num4);
			Vector3 point3 = bounds.center + new Vector3(0f - num2, 0f - num3, 0f - num4);
			Vector3 point4 = bounds.center + new Vector3(0f - num2, 0f - num3, num4);
			if (!Util.PointWithinTerrain(point) && !Util.PointWithinTerrain(point2) && !Util.PointWithinTerrain(point3))
			{
				return Util.PointWithinTerrain(point4);
			}
			return true;
		}
		}
	}

	private static HashSet<Predicate> GetKeepRBPreds()
	{
		if (keepRBPreds == null)
		{
			keepRBPreds = new HashSet<Predicate>
			{
				predicateImpact, predicateImpactModel, predicateParticleImpact, predicateParticleImpactModel, predicateBump, predicateBumpChunk, predicateBumpModel, predicateTaggedBump, predicateTaggedBumpChunk, predicateTaggedBumpModel,
				predicateWithinWater, predicateWithinTaggedWater, predicateModelWithinWater, predicateModelWithinTaggedWater, predicateUnfreeze
			};
		}
		return keepRBPreds;
	}

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
		List<Block> list = connectedCache[block];
		foreach (Block item in list)
		{
			if (item.connectionTypes.Contains(2) || item.connectionTypes.Contains(-2))
			{
				return true;
			}
		}
		foreach (Block block2 in blocks)
		{
			if (block2 is BlockWalkable || block2 is BlockCharacter || block2 is BlockAbstractLegs || block2 is BlockAbstractWheel || block2 is BlockAbstractMotor || block2 is BlockAbstractTorsionSpring || block2 is BlockTankTreadsWheel || block2 is BlockMissile || block2 is BlockAbstractPlatform || block2 is BlockPiston)
			{
				return true;
			}
		}
		HashSet<Predicate> manyPreds = GetKeepRBPreds();
		foreach (Block item2 in list)
		{
			if (item2.ContainsTileWithAnyPredicateInPlayMode2(manyPreds))
			{
				return true;
			}
		}
		return false;
	}

	public virtual void ReassignedToChunk(Chunk c)
	{
	}

	public virtual TileResultCode Freeze(bool informModelBlocks)
	{
		if (isTreasure)
		{
			TreasureHandler.BlockFrozen(this);
			return TileResultCode.True;
		}
		if (!didFix && goT.parent != null)
		{
			didFix = true;
			if (chunk.go == null)
			{
				return TileResultCode.True;
			}
			bool flag = IsKeepRBChunkBlocks(chunk.blocks);
			Rigidbody rb = chunk.rb;
			if (rb != null)
			{
				if (flag)
				{
					rb.isKinematic = true;
				}
				else
				{
					hadRigidBody = true;
					chunk.RemoveRigidbody();
				}
			}
			if (informModelBlocks)
			{
				UpdateConnectedCache();
				List<Block> list = connectedCache[this];
				list.ForEach(delegate(Block b)
				{
					b.ChunkInModelFrozen();
				});
			}
		}
		return TileResultCode.True;
	}

	public virtual TileResultCode Freeze(ScriptRowExecutionInfo eInfo, object[] args)
	{
		return Freeze(informModelBlocks: true);
	}

	public virtual TileResultCode Unfreeze(ScriptRowExecutionInfo eInfo, object[] args)
	{
		Unfreeze();
		return TileResultCode.True;
	}

	public virtual void Unfreeze()
	{
		if (isTreasure)
		{
			TreasureHandler.BlockUnfrozen(this);
		}
		else
		{
			if (!didFix)
			{
				return;
			}
			didFix = false;
			bool flag = false;
			List<Block> list = connectedCache[this];
			for (int i = 0; i < list.Count; i++)
			{
				Block block = list[i];
				flag |= block.didFix;
			}
			if (!flag)
			{
				Rigidbody rigidbody = chunk.rb;
				if (null == rigidbody && hadRigidBody)
				{
					hadRigidBody = false;
					rigidbody = chunk.AddRigidbody();
				}
				if (rigidbody != null && !vanished)
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

	public TileResultCode IsBrokenOff(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (goT.parent.gameObject.name == "Broken")
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode BreakOff(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (goT.parent.gameObject.name == "Broken")
		{
			return TileResultCode.False;
		}
		GameObject gameObject = new GameObject("Broken");
		gameObject.transform.position = goT.position;
		Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
		if (Blocksworld.interpolateRigidBodies)
		{
			rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
		}
		rigidbody.mass = GetMass();
		gameObject.GetComponent<Rigidbody>().velocity = goT.parent.GetComponent<Rigidbody>().velocity;
		gameObject.GetComponent<Rigidbody>().angularVelocity = goT.parent.GetComponent<Rigidbody>().angularVelocity;
		Transform parent = goT.parent;
		foreach (object item in parent)
		{
			Transform transform = (Transform)item;
			transform.GetComponent<Collider>().enabled = false;
		}
		goT.parent = gameObject.transform;
		foreach (object item2 in parent)
		{
			Transform transform2 = (Transform)item2;
			transform2.GetComponent<Collider>().enabled = true;
		}
		go.GetComponent<Collider>().enabled = true;
		return TileResultCode.True;
	}

	public TileResultCode IsExploded(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (broken)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public void CreatePositionedAudioSourceIfNecessary(string sfxNameWhenCreate = "Create", GameObject theGo = null)
	{
		if (audioSource == null || !audioSource.enabled)
		{
			if (theGo == null)
			{
				theGo = go;
			}
			if (audioSource == null)
			{
				audioSource = CreateAudioSource(sfxNameWhenCreate, theGo);
			}
		}
		audioSource.enabled = true;
	}

	public void CreateLoopingPositionedAudioSourceIfNecessary(string sfxNameWhenCreate = "Create", GameObject theGo = null)
	{
		if (loopingAudioSource == null)
		{
			if (theGo == null)
			{
				theGo = go;
			}
			if (loopingAudioSource == null)
			{
				loopingAudioSource = CreateAudioSource(sfxNameWhenCreate, theGo);
			}
		}
		loopingAudioSource.enabled = true;
	}

	private AudioSource CreateAudioSource(string sfxNameWhenCreate = "Create", GameObject theGo = null)
	{
		AudioSource audioSource = theGo.AddComponent<AudioSource>();
		Sound.SetWorldAudioSourceParams(audioSource);
		audioSource.playOnAwake = false;
		audioSource.clip = Sound.GetSfx(sfxNameWhenCreate);
		return audioSource;
	}

	public void PlayPositionedSound(string sfxName, float volume = 1f, float pitch = 1f)
	{
		DelegateCommand<Block, string, float, float> c = new DelegateCommand<Block, string, float, float>(this, sfxName, volume, pitch, PlayPositionedSoundNow);
		Blocksworld.AddFixedUpdateCommand(c);
	}

	public void PlayPositionedSoundAfterDelay(int delay, string sfxName, float volume = 1f, float pitch = 1f)
	{
		DelayedDelegateCommand<Block, string, float, float> c = new DelayedDelegateCommand<Block, string, float, float>(this, sfxName, volume, pitch, PlayPositionedSoundNow, delay);
		Blocksworld.AddFixedUpdateCommand(c);
	}

	private static void PlayPositionedSoundNow(Block block, string sfxName, float volume = 1f, float pitch = 1f)
	{
		if (!(block.go == null))
		{
			block.CreatePositionedAudioSourceIfNecessary(sfxName);
			if (block.audioSource != null && block.go.activeSelf && !block.vanished && !Sound.BlockIsMuted(block))
			{
				Sound.PlaySound(sfxName, block.audioSource, oneShot: true, volume, pitch);
			}
		}
	}

	public Vector3 GetChunkVelocity()
	{
		Vector3 result = Vector3.zero;
		Rigidbody rb = chunk.rb;
		if (rb != null && !rb.isKinematic)
		{
			result = rb.velocity;
		}
		return result;
	}

	public virtual TileResultCode HitByExplosion(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (AbstractDetachCommand.HitByExplosion(this))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public virtual TileResultCode ExplodeLocal(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (eInfo.timer == 0f && !vanished && !broken)
		{
			float radius = Util.GetFloatArg(args, 0, 3f);
			RadialExplosionCommand c = new RadialExplosionCommand(5f, goT.position, GetChunkVelocity(), radius * 0.5f, radius, radius * 2f, null, string.Empty);
			Blocksworld.AddFixedUpdateCommand(c);
			Sound.PlayPositionedOneShot("Local Explode", goT.position, 5f, Mathf.Max(120f, radius * 30f));
			Block b = this;
			DelegateCommand c2 = new DelegateCommand(delegate
			{
				if (!Invincibility.IsInvincible(b))
				{
					BWSceneManager.RemovePlayBlock(b);
					ExplodeOffConnectedBlocks(radius);
					b.broken = true;
				}
			});
			Blocksworld.AddFixedUpdateCommand(c2);
			if (!Invincibility.IsInvincible(this))
			{
				broken = true;
				WorldSession.platformDelegate.TrackAchievementIncrease("fireworks_display", 1);
				return TileResultCode.True;
			}
		}
		if (!(eInfo.timer >= 0.5f))
		{
			return TileResultCode.Delayed;
		}
		return TileResultCode.True;
	}

	public virtual TileResultCode ExplodeTag(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (eInfo.timer == 0f && !vanished && !broken)
		{
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			float radius = Util.GetFloatArg(args, 1, 3f);
			RadialExplosionCommand c = new RadialExplosionCommand(5f, goT.position, GetChunkVelocity(), radius * 0.5f, radius, radius * 2f, null, stringArg);
			Blocksworld.AddFixedUpdateCommand(c);
			Sound.PlayPositionedOneShot("Local Explode", goT.position, 5f, Mathf.Max(120f, radius * 30f));
			Block b = this;
			DelegateCommand c2 = new DelegateCommand(delegate
			{
				if (!Invincibility.IsInvincible(b))
				{
					BWSceneManager.RemovePlayBlock(b);
					ExplodeOffConnectedBlocks(radius);
					b.broken = true;
				}
			});
			Blocksworld.AddFixedUpdateCommand(c2);
			if (!Invincibility.IsInvincible(this))
			{
				broken = true;
				WorldSession.platformDelegate.TrackAchievementIncrease("fireworks_display", 1);
				return TileResultCode.True;
			}
		}
		if (!(eInfo.timer >= 0.5f))
		{
			return TileResultCode.Delayed;
		}
		return TileResultCode.True;
	}

	private void AddInvincibilityStep()
	{
		if (stepInvincibilityCommand == null)
		{
			stepInvincibilityCommand = new DelegateMultistepCommand(delegate
			{
				Invincibility.Step();
			}, 3);
		}
		else
		{
			stepInvincibilityCommand.SetSteps(3);
		}
		Blocksworld.AddFixedUpdateUniqueCommand(stepInvincibilityCommand);
	}

	public virtual TileResultCode IsInvincible(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (Invincibility.IsInvincible(this))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public virtual TileResultCode IsInvincibleModel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		return IsInvincible(eInfo, args);
	}

	public virtual TileResultCode IsShieldBlocking(ScriptRowExecutionInfo eInfo, object[] args)
	{
		BlockAnimatedCharacter blockAnimatedCharacter = BlockAnimatedCharacter.FindBlockingPropHolder(this);
		if (blockAnimatedCharacter != null)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public virtual TileResultCode IsWeaponAttacking(ScriptRowExecutionInfo eInfo, object[] args)
	{
		BlockAnimatedCharacter blockAnimatedCharacter = BlockAnimatedCharacter.FindAttackingPropHolder(this);
		if (blockAnimatedCharacter != null)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public virtual TileResultCode SetInvincible(ScriptRowExecutionInfo eInfo, object[] args)
	{
		AddInvincibilityStep();
		Invincibility.SetBlockInvincible(this);
		return TileResultCode.True;
	}

	public virtual TileResultCode SetInvincibleModel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		AddInvincibilityStep();
		Invincibility.SetModelInvincible(this);
		return TileResultCode.True;
	}

	public virtual TileResultCode IsDetached(ScriptRowExecutionInfo eInfo, object[] args)
	{
		return boolToTileResult(!connectedCache.ContainsKey(this) || connectedCache[this].Count == 0);
	}

	public virtual TileResultCode Detach(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (broken)
		{
			return TileResultCode.True;
		}
		if (vanishingOrAppearingBlocks.Contains(this))
		{
			return TileResultCode.Delayed;
		}
		if (isTreasure)
		{
			if (TreasureHandler.IsPickingUpOrRespawning(this))
			{
				return TileResultCode.Delayed;
			}
			bool flag = TreasureHandler.IsHiddenTreasureModel(this);
			TreasureHandler.RemoveTreasureModel(this);
		}
		RadialSmashCommand c = new RadialSmashCommand(this, GetChunkVelocity(), 0f, null, detachWithForce: false);
		Blocksworld.AddFixedUpdateCommand(c);
		broken = true;
		return TileResultCode.True;
	}

	public virtual TileResultCode SmashLocal(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (broken)
		{
			return TileResultCode.True;
		}
		float floatArg = Util.GetFloatArg(args, 0, 3f);
		RadialSmashCommand c = new RadialSmashCommand(this, GetChunkVelocity(), floatArg, null, detachWithForce: true);
		Blocksworld.AddFixedUpdateCommand(c);
		PlayPositionedSound("Explode");
		broken = true;
		return TileResultCode.True;
	}

	public virtual TileResultCode Explode(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (broken)
		{
			return TileResultCode.True;
		}
		if (vanishingOrAppearingBlocks.Contains(this))
		{
			return TileResultCode.Delayed;
		}
		bool isHiddenTreasure = false;
		if (isTreasure)
		{
			if (TreasureHandler.IsPickingUpOrRespawning(this))
			{
				return TileResultCode.Delayed;
			}
			isHiddenTreasure = TreasureHandler.IsHiddenTreasureModel(this);
			TreasureHandler.RemoveTreasureModel(this);
		}
		PlayPositionedSound("Explode");
		float floatArg = Util.GetFloatArg(args, 0, 1f);
		ExplodeOffConnectedBlocks(floatArg, isHiddenTreasure);
		return TileResultCode.True;
	}

	public void ExplodeOffConnectedBlocks(float forceMultiplier = 1f, bool isHiddenTreasure = false)
	{
		List<Block> list = ((!connectedCache.ContainsKey(this)) ? ConnectednessGraph.ConnectedComponent(this, 3) : connectedCache[this]);
		List<Block> nonVanishedBlocks = new List<Block>();
		list.ForEach(delegate(Block b)
		{
			if (b.vanished || isHiddenTreasure)
			{
				BWSceneManager.RemovePlayBlock(b);
				b.broken = true;
				b.go.SetActive(value: false);
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
		foreach (Block item in nonVanishedBlocks)
		{
			Chunk chunk = item.chunk;
			if (chunk.go == null)
			{
				list2.Add(item.goT.position);
				list3.Add(Vector3.zero);
				list4.Add(Vector3.zero);
				continue;
			}
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
			if (!(item is BlockPosition))
			{
				Collider component = item.go.GetComponent<Collider>();
				if (component != null)
				{
					component.isTrigger = false;
				}
			}
		}
		for (int num = 0; num < list5.Count; num++)
		{
			Chunk chunk2 = list5[num];
			Rigidbody rb = chunk2.rb;
			if (rb == null || rb.isKinematic)
			{
				hashSet.UnionWith(chunk2.blocks);
			}
			Blocksworld.blocksworldCamera.Unfollow(chunk2);
			Blocksworld.chunks.Remove(chunk2);
			chunk2.Destroy(delayed: true);
		}
		List<Chunk> list6 = new List<Chunk>();
		for (int num2 = 0; num2 < nonVanishedBlocks.Count; num2++)
		{
			Block block = nonVanishedBlocks[num2];
			connectedChunks.Remove(block);
			block.Break(list2[num2], list3[num2], list4[num2]);
			Chunk chunk3 = new Chunk(new List<Block> { block }, forceRigidbody: true);
			if (block is BlockPosition)
			{
				chunk3.rb.velocity = list3[num2];
				chunk3.rb.angularVelocity = list4[num2];
			}
			else if (Invincibility.IsInvincible(chunk3.blocks[0]))
			{
				chunk3.rb.isKinematic = true;
			}
			else
			{
				AddExplosiveForce(chunk3.rb, chunk3.go.transform.position, list2[num2], list3[num2], list4[num2], forceMultiplier);
			}
			Blocksworld.chunks.Add(chunk3);
			list6.Add(chunk3);
		}
		for (int num3 = 0; num3 < nonVanishedBlocks.Count; num3++)
		{
			Block key = nonVanishedBlocks[num3];
			connectedChunks[key] = new HashSet<Chunk>(list6);
		}
		if (Blocksworld.worldOceanBlock != null)
		{
			foreach (Block item2 in hashSet)
			{
				Blocksworld.worldOceanBlock.AddBlockToSimulation(item2);
			}
		}
		Blocksworld.blocksworldCamera.UpdateChunkSpeeds();
	}

	public static void AddExplosiveForce(Rigidbody rb, Vector3 localPos, Vector3 chunkPos, Vector3 chunkVel, Vector3 chunkAngVel, float forceMultiplier = 1f)
	{
		rb.velocity = chunkVel;
		rb.angularVelocity = chunkAngVel;
		if (forceMultiplier > 0f)
		{
			float num = 5f * forceMultiplier;
			rb.AddForce((localPos - chunkPos).normalized * num + UnityEngine.Random.insideUnitSphere * num, ForceMode.Impulse);
		}
	}

	public TileResultCode IncreaseCounter(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string key = (string)args[0];
		Blocksworld.counters[key]++;
		Blocksworld.countersActivated[key] = true;
		return TileResultCode.True;
	}

	public TileResultCode DecreaseCounter(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string key = (string)args[0];
		Blocksworld.counters[key]--;
		Blocksworld.countersActivated[key] = true;
		return TileResultCode.True;
	}

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

	public TileResultCode SetCounter(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string key = (string)args[0];
		Blocksworld.countersActivated[key] = true;
		int value = int.Parse((string)args[1]);
		Blocksworld.counters[key] = value;
		return TileResultCode.True;
	}

	private void StepObjectAnimation()
	{
		if (animationFixedTime < Time.fixedTime)
		{
			animationStep++;
			animationFixedTime = Time.fixedTime;
			Transform transform = goT;
			Vector3 vector = playPosition;
			Quaternion quaternion = playRotation;
			Rigidbody rb = chunk.rb;
			if (rb != null)
			{
				rb.isKinematic = true;
			}
			transform = chunk.go.transform;
			vector = parentPlayPosition;
			quaternion = parentPlayRotation;
			transform.position = vector + objectAnimationPositionOffset;
			transform.rotation = quaternion * objectAnimationRotationOffset;
			objectAnimationPositionOffset = Vector3.zero;
			objectAnimationRotationOffset = Quaternion.identity;
			objectAnimationScaleOffset = Vector3.one;
		}
	}

	public TileResultCode LevitateAnimation(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float num = Blocksworld.fixedDeltaTime * (float)animationStep;
		objectAnimationPositionOffset += new Vector3(0f, 0.5f * Mathf.Sin(2f * num + playPosition.x * 10f + playPosition.z * 25f), 0f);
		objectAnimationRotationOffset *= Quaternion.AngleAxis(axis: new Vector3(0.2f, 0.9f, 0f), angle: playPosition.x * 10.3242f + playPosition.z * 1.456f + playPosition.y + 25f * num);
		StepObjectAnimation();
		return TileResultCode.True;
	}

	public TileResultCode SineScaleAnimation(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float num = Blocksworld.fixedDeltaTime * (float)animationStep;
		Vector3 vector = (Vector3)args[0];
		Vector3 vector2 = (Vector3)args[1];
		Vector3 vector3 = (Vector3)args[1];
		float num2 = (float)Math.PI * 2f;
		objectAnimationScaleOffset = new Vector3(objectAnimationScaleOffset.x * vector.x * Mathf.Sin(num2 * (vector2.x * num + vector3.x)), objectAnimationScaleOffset.y * vector.y * Mathf.Sin(num2 * (vector2.y * num + vector3.y)), objectAnimationScaleOffset.z * vector.z * Mathf.Sin(num2 * (vector2.z * num + vector3.z)));
		objectAnimationPositionOffset += new Vector3(0f, 0.5f * Mathf.Sin(2f * num + playPosition.x * 10f + playPosition.z * 25f), 0f);
		objectAnimationRotationOffset *= Quaternion.AngleAxis(axis: new Vector3(0.2f, 0.9f, 0f), angle: playPosition.x * 10.3242f + playPosition.z * 1.456f + playPosition.y + 25f * num);
		StepObjectAnimation();
		return TileResultCode.True;
	}

	public AudioSource GetOrCreateLoopingAudioSource(string sfxName, float volume = 1f, float pitch = 1f)
	{
		return null;
	}

	public AudioSource GetOrCreateLoopingPositionedAudioSource(string sfxName = "Create")
	{
		CreateLoopingPositionedAudioSourceIfNecessary(sfxName);
		return loopingAudioSource;
	}

	public TileResultCode Animate(ScriptRowExecutionInfo eInfo, object[] args)
	{
		return TileResultCode.True;
	}

	public TileResultCode PlaySound(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string sfxName = ((args.Length == 0) ? "Create" : ((string)args[0]));
		string location = ((args.Length <= 1) ? "Camera" : ((string)args[1]));
		string soundType = ((args.Length <= 2) ? "OneShot" : ((string)args[2]));
		float volume = ((args.Length <= 3) ? 1f : ((float)args[3]));
		float pitch = ((args.Length <= 4) ? 1f : ((float)args[4]));
		return PlaySound(sfxName, location, soundType, volume, pitch, durational: false, eInfo.timer);
	}

	public TileResultCode SoundSensor(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string text = ((args.Length == 0) ? string.Empty : ((string)args[0]));
		float num = 0f;
		if (text.Length > 0)
		{
			HashSet<Block> durationalSoundSources = Sound.GetDurationalSoundSources(text);
			if (durationalSoundSources != null)
			{
				Vector3 position = goT.position;
				foreach (Block item in durationalSoundSources)
				{
					if (item.go != null)
					{
						Vector3 position2 = item.goT.position;
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

	public virtual TileResultCode Mute(ScriptRowExecutionInfo eInfo, object[] args)
	{
		Mute();
		Blocksworld.AddFixedUpdateUniqueCommand(unmuteCommand);
		return TileResultCode.True;
	}

	public TileResultCode MuteModel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		UpdateConnectedCache();
		List<Block> list = connectedCache[this];
		for (int i = 0; i < list.Count; i++)
		{
			list[i].Mute();
		}
		Blocksworld.AddFixedUpdateUniqueCommand(unmuteCommand);
		return TileResultCode.True;
	}

	public virtual void Mute()
	{
		Sound.MuteBlock(this);
	}

	public TileResultCode PlaySoundDurational(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string sfxName = ((args.Length == 0) ? "Create" : ((string)args[0]));
		string location = ((args.Length <= 1) ? "Camera" : ((string)args[1]));
		string soundType = ((args.Length <= 2) ? "OneShot" : ((string)args[2]));
		bool flag = eInfo.timer == 0f;
		ScriptRowData scriptRowData = ScriptRowData.GetScriptRowData(eInfo);
		string value;
		if (flag)
		{
			if (Sound.GetRandomSfx(ref sfxName))
			{
				scriptRowData.SetString("Rnd SFX", sfxName);
			}
		}
		else if (scriptRowData.TryGetString("Rnd SFX", out value))
		{
			sfxName = value;
		}
		float durationalSfxVolume = Sound.GetDurationalSfxVolume(sfxName);
		return PlaySound(sfxName, location, soundType, durationalSfxVolume, 1f, durational: true, eInfo.timer);
	}

	public virtual TileResultCode PlaySound(string sfxName, string location, string soundType, float volume, float pitch, bool durational = false, float timer = 0f)
	{
		bool flag = timer <= 0.001f;
		bool flag2 = soundType == "Loop";
		if ((flag || !durational) && !Sound.BlockIsMuted(this))
		{
			switch (location)
			{
			case "Block":
				UpdateWithinWaterLPFilter();
				if (soundType == "OneShot")
				{
					PlayPositionedSound(sfxName, volume, pitch);
				}
				break;
			case "Camera":
				if (soundType == "OneShot")
				{
					Sound.PlaySound(sfxName, Sound.GetOrCreateOneShotAudioSource(), oneShot: true);
				}
				else if (flag2)
				{
					Sound.PlaySound(sfxName, GetOrCreateLoopingAudioSource(sfxName), oneShot: false, volume, pitch);
				}
				break;
			}
		}
		if (Sound.sfxEnabled && flag2)
		{
			loopSfxCommand.BlockPlaysLoop(this, sfxName, volume, pitch);
			Blocksworld.AddFixedUpdateUniqueCommand(loopSfxCommand, resetWhenAdded: false);
		}
		if (durational)
		{
			return UpdateDurationalSoundSource(sfxName, timer);
		}
		return TileResultCode.True;
	}

	protected TileResultCode UpdateDurationalSoundSource(string sfxName, float timer)
	{
		float durationalSfxTime = Sound.GetDurationalSfxTime(sfxName);
		bool flag = timer >= durationalSfxTime;
		if (!Sound.BlockIsMuted(this))
		{
			Sound.AddDurationalSoundSource(sfxName, this);
		}
		if (flag)
		{
			return TileResultCode.True;
		}
		return TileResultCode.Delayed;
	}

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
		float num = lengthMult * VisualEffect.GetEffectLength(vfxName);
		if (timer < num)
		{
			return TileResultCode.Delayed;
		}
		return TileResultCode.True;
	}

	public TileResultCode PlayVfxDurational(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, "Sparkle");
		float floatArg = Util.GetFloatArg(args, 1, 1f);
		int num = ((args.Length > 2) ? ((int)args[2]) : 0);
		string colorName = ((args.Length <= 3) ? "White" : ((string)args[3]));
		return PlayVfxDurational(stringArg, floatArg, eInfo.timer, colorName);
	}

	public TileResultCode OnCollect(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (TreasureHandler.IsPickedUpThisFrame(this))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode OnCollectByTag(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (TreasureHandler.IsPickedUpThisFrame(this, (string)args[0]))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode RespawnPickup(ScriptRowExecutionInfo eInfo, object[] args)
	{
		TreasureHandler.Respawn(this);
		return TileResultCode.True;
	}

	public TileResultCode ForceCollectPickup(ScriptRowExecutionInfo eInfo, object[] args)
	{
		TreasureHandler.ForceCollect(this, Util.GetStringArg(args, 0, null));
		return TileResultCode.True;
	}

	public TileResultCode SetAsTreasureBlockIcon(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 0, 0);
		TreasureHandler.SetAsTreasureBlockIcon(this, intArg);
		return TileResultCode.True;
	}

	public TileResultCode SetAsTreasureTextureIcon(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 0, 0);
		TreasureHandler.SetAsTreasureTextureIcon(this, intArg);
		return TileResultCode.True;
	}

	public TileResultCode SetAsCounterUIBlockIcon(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 0, 0);
		if (BlockCounterUI.allCounterBlocks.TryGetValue(intArg, out var value))
		{
			value.SetCustomIconGAF(GetIconGaf());
		}
		return TileResultCode.True;
	}

	public TileResultCode SetAsCounterUITextureIcon(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 0, 0);
		if (BlockCounterUI.allCounterBlocks.TryGetValue(intArg, out var value))
		{
			value.SetCustomIconGAF(new GAF(predicateTextureTo, GetTexture(), Vector3.zero));
		}
		return TileResultCode.True;
	}

	public TileResultCode SetAsTimerUIBlockIcon(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 0, 0);
		if (BlockTimerUI.allTimerBlocks.TryGetValue(intArg, out var value))
		{
			value.SetCustomIconGAF(GetIconGaf());
		}
		return TileResultCode.True;
	}

	public TileResultCode SetAsTimerUITextureIcon(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 0, 0);
		if (BlockTimerUI.allTimerBlocks.TryGetValue(intArg, out var value))
		{
			value.SetCustomIconGAF(new GAF(predicateTextureTo, GetTexture(), Vector3.zero));
		}
		return TileResultCode.True;
	}

	public TileResultCode SetAsGaugeUIBlockIcon(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 0, 0);
		if (BlockGaugeUI.allGaugeBlocks.TryGetValue(intArg, out var value))
		{
			value.SetCustomIconGAF(GetIconGaf());
		}
		return TileResultCode.True;
	}

	public TileResultCode SetAsGaugeUITextureIcon(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 0, 0);
		if (BlockGaugeUI.allGaugeBlocks.TryGetValue(intArg, out var value))
		{
			value.SetCustomIconGAF(new GAF(predicateTextureTo, GetTexture(), Vector3.zero));
		}
		return TileResultCode.True;
	}

	public TileResultCode SetAsTreasureModel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (!broken)
		{
			int intArg = Util.GetIntArg(args, 0, 0);
			TreasureHandler.AddTreasureModel(this, null, updateTheObjectCounter: true, intArg);
		}
		return TileResultCode.True;
	}

	public TileResultCode SetAsTreasureModelTag(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (!broken)
		{
			int intArg = Util.GetIntArg(args, 1, 0);
			TreasureHandler.AddTreasureModel(this, (string)args[0], updateTheObjectCounter: true, intArg);
		}
		return TileResultCode.True;
	}

	public TileResultCode SetAsPickupModel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (!broken)
		{
			TreasureHandler.AddTreasureModel(this, null, updateTheObjectCounter: false);
		}
		return TileResultCode.True;
	}

	public TileResultCode SetAsPickupModelTag(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (!broken)
		{
			TreasureHandler.AddTreasureModel(this, (string)args[0], updateTheObjectCounter: false);
		}
		return TileResultCode.True;
	}

	public TileResultCode VisualizeReward(ScriptRowExecutionInfo eInfo, object[] args)
	{
		return RewardVisualization.VisualizeReward(eInfo, args);
	}

	public virtual TileResultCode AnalogCeil(ScriptRowExecutionInfo eInfo, object[] args)
	{
		eInfo.floatArg = Mathf.Ceil(eInfo.floatArg);
		if (!(eInfo.floatArg <= 0f))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public virtual TileResultCode AnalogFloor(ScriptRowExecutionInfo eInfo, object[] args)
	{
		eInfo.floatArg = Mathf.Floor(eInfo.floatArg);
		if (!(eInfo.floatArg <= 0f))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public virtual TileResultCode AnalogRound(ScriptRowExecutionInfo eInfo, object[] args)
	{
		eInfo.floatArg = Mathf.Round(eInfo.floatArg);
		if (!(eInfo.floatArg <= 0f))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public virtual TileResultCode AnalogMin(ScriptRowExecutionInfo eInfo, object[] args)
	{
		eInfo.floatArg = Mathf.Min(eInfo.floatArg, Util.GetFloatArg(args, 0, 1f));
		if (!(eInfo.floatArg <= 0f))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public virtual TileResultCode AnalogMax(ScriptRowExecutionInfo eInfo, object[] args)
	{
		eInfo.floatArg = Mathf.Max(eInfo.floatArg, Util.GetFloatArg(args, 0, 1f));
		if (!(eInfo.floatArg <= 0f))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public virtual TileResultCode AnalogClamp(ScriptRowExecutionInfo eInfo, object[] args)
	{
		eInfo.floatArg = Mathf.Clamp(eInfo.floatArg, Util.GetFloatArg(args, 0, 0f), Util.GetFloatArg(args, 1, 1f));
		if (!(eInfo.floatArg <= 0f))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	private AnimationCurve GetVanishAnimCurve(float animTime)
	{
		if (vanishAnimCurve == null)
		{
			float time = animTime * 0.2f;
			float value = 1.5f;
			vanishAnimCurve = new AnimationCurve(new Keyframe(0f, 1f, 0f, 0f), new Keyframe(time, value, 0f, 0f), new Keyframe(animTime, 0.02f, 0f, 0f));
		}
		return vanishAnimCurve;
	}

	public virtual TileResultCode IsVanished(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (vanished)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public virtual TileResultCode IsAppeared(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (!vanished)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public virtual TileResultCode Appear(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float num = 0.375f;
		if (eInfo.timer <= 0.25f)
		{
			if (go.GetComponent<Renderer>() != null && !go.GetComponent<Renderer>().enabled)
			{
				go.GetComponent<Renderer>().enabled = true;
			}
			if (go.GetComponent<Collider>() != null)
			{
				go.GetComponent<Collider>().enabled = true;
				go.GetComponent<Collider>().isTrigger = false;
			}
			if (goShadow != null)
			{
				goShadow.GetComponent<Renderer>().enabled = true;
			}
			Transform parent = goT.parent;
			if (parent != null && parent.GetComponent<Rigidbody>() != null)
			{
				parent.GetComponent<Rigidbody>().isKinematic = false;
				parent.GetComponent<Rigidbody>().WakeUp();
			}
			go.SetActive(value: true);
		}
		if (eInfo.timer >= num)
		{
			goT.localScale = Vector3.one;
			vanished = false;
			return TileResultCode.True;
		}
		if (CanAnimateScale())
		{
			GetVanishAnimCurve(num);
			float num2 = Mathf.Clamp(vanishAnimCurve.Evaluate(num - eInfo.timer), 0.001f, 1f);
			goT.localScale = new Vector3(num2, num2, num2);
		}
		return TileResultCode.Delayed;
	}

	public virtual TileResultCode Vanish(ScriptRowExecutionInfo eInfo, object[] args)
	{
		go.GetComponent<Collider>().enabled = false;
		if (goT.parent != null && goT.parent.GetComponent<Rigidbody>() != null)
		{
			goT.parent.GetComponent<Rigidbody>().isKinematic = true;
		}
		float num = 0.375f;
		if (!vanished)
		{
			vanished = true;
			CollisionManager.WakeUpObjectsRestingOn(this);
		}
		if (eInfo.timer >= num)
		{
			go.GetComponent<Renderer>().enabled = false;
			if (goShadow != null)
			{
				goShadow.GetComponent<Renderer>().enabled = false;
			}
			go.SetActive(value: false);
			return TileResultCode.True;
		}
		GetVanishAnimCurve(num);
		float num2 = vanishAnimCurve.Evaluate(eInfo.timer);
		goT.localScale = new Vector3(num2, num2, num2);
		return TileResultCode.Delayed;
	}

	private void SetKinematicIfSingletonOrLegs(bool k)
	{
		UpdateConnectedCache();
		List<Block> list = connectedCache[this];
		bool flag = TreasureHandler.IsPartOfTreasureModel(this);
		BlockWalkable blockWalkable = this as BlockWalkable;
		if (!didFix && (list.Count == 1 || (blockWalkable != null && !blockWalkable.unmoving)) && chunk.go != null)
		{
			Rigidbody rb = chunk.rb;
			if (rb != null && !flag && chunk.blocks.Count > 0 && !chunk.IsFrozen())
			{
				rb.isKinematic = k;
				if (!k)
				{
					rb.WakeUp();
				}
			}
		}
		if (k || flag)
		{
			return;
		}
		foreach (Block item in list)
		{
			BlockWalkable blockWalkable2 = item as BlockWalkable;
			if (item != this && blockWalkable2 != null && !blockWalkable2.unmoving && item.vanished)
			{
				return;
			}
		}
		HashSet<Chunk> hashSet = connectedChunks[this];
		foreach (Chunk item2 in hashSet)
		{
			if (item2.blocks.Count <= 0)
			{
				continue;
			}
			bool flag2 = false;
			for (int i = 0; i < item2.blocks.Count; i++)
			{
				if (flag2)
				{
					break;
				}
				flag2 |= item2.blocks[i].didFix;
			}
			if (!flag2 && item2.go != null)
			{
				Rigidbody rb2 = item2.rb;
				if (rb2 != null && rb2.isKinematic)
				{
					rb2.isKinematic = false;
					rb2.WakeUp();
				}
			}
		}
	}

	public virtual TileResultCode AppearBlock(bool animate, float timer)
	{
		float num = ((!animate) ? 0f : 0.375f);
		if (timer == 0f)
		{
			if (vanishingOrAppearingBlocks.Contains(this))
			{
				return TileResultCode.True;
			}
			if (!vanished)
			{
				vanishingOrAppearingBlocks.Remove(this);
				return TileResultCode.True;
			}
			vanishingOrAppearingBlocks.Add(this);
			if (go.GetComponent<Collider>() != null)
			{
				go.GetComponent<Collider>().enabled = true;
				if (!TreasureHandler.IsPartOfTreasureModel(this) && !ColliderIsTriggerInPlayMode())
				{
					go.GetComponent<Collider>().isTrigger = false;
				}
			}
			if (goShadow != null)
			{
				goShadow.GetComponent<Renderer>().enabled = true;
			}
			go.SetActive(value: true);
			if (this is BlockAnimatedCharacter blockAnimatedCharacter)
			{
				blockAnimatedCharacter.stateHandler.Appearing();
			}
		}
		if (timer >= num)
		{
			goT.localScale = Vector3.one;
			vanished = false;
			SetKinematicIfSingletonOrLegs(k: false);
			Appeared();
			DoWithNonVanishingOrAppearingModelBlocks(BlockModelBlockAppeared);
			vanishingOrAppearingBlocks.Remove(this);
			if (!isTreasure)
			{
				RestoreMeshColliderInfo();
			}
			return TileResultCode.True;
		}
		GetVanishAnimCurve(num);
		float num2 = Mathf.Clamp(vanishAnimCurve.Evaluate(num - timer), 0.001f, 1f);
		goT.localScale = new Vector3(num2, num2, num2);
		Appearing(num2);
		List<Block> list = connectedCache[this];
		for (int i = 0; i < list.Count; i++)
		{
			Block block = list[i];
			if (!vanishingOrAppearingBlocks.Contains(block))
			{
				block.ModelBlockAppearing(num2);
			}
		}
		return TileResultCode.Delayed;
	}

	private static void BlockModelBlockAppeared(Block b)
	{
		b.ModelBlockAppeared();
	}

	public virtual TileResultCode AppearBlock(ScriptRowExecutionInfo eInfo, object[] args)
	{
		bool animate = CanAnimateScale() && Util.GetIntBooleanArg(args, 0, defaultValue: false);
		return AppearBlock(animate, eInfo.timer);
	}

	protected virtual void RestoreMeshColliderInfo()
	{
		if (meshColliderInfo != null)
		{
			meshColliderInfo.Restore();
			meshColliderInfo = null;
		}
	}

	protected void DoWithNonVanishingOrAppearingModelBlocks(Action<Block> action)
	{
		UpdateConnectedCache();
		List<Block> list = connectedCache[this];
		for (int i = 0; i < list.Count; i++)
		{
			Block block = list[i];
			if (!vanishingOrAppearingBlocks.Contains(block))
			{
				action(block);
			}
		}
	}

	protected virtual bool CanAnimateScale()
	{
		return true;
	}

	public virtual TileResultCode VanishBlock(bool animate, float timer)
	{
		if (timer == 0f)
		{
			if (vanishingOrAppearingBlocks.Contains(this))
			{
				return TileResultCode.True;
			}
			if (vanished)
			{
				vanishingOrAppearingBlocks.Remove(this);
				return TileResultCode.True;
			}
			vanishingOrAppearingBlocks.Add(this);
			Collider component = go.GetComponent<Collider>();
			if (component != null)
			{
				component.enabled = false;
				if (component is MeshCollider)
				{
					MeshCollider meshCollider = (MeshCollider)component;
					ReplaceMeshCollider(meshCollider);
					component.isTrigger = meshCollider.convex;
				}
				else
				{
					component.isTrigger = true;
				}
			}
			SetKinematicIfSingletonOrLegs(k: true);
		}
		float num = ((!animate) ? 0f : 0.375f);
		if (timer >= num)
		{
			if (!vanished)
			{
				vanished = true;
				CollisionManager.WakeUpObjectsRestingOn(this);
			}
			if (goShadow != null)
			{
				goShadow.GetComponent<Renderer>().enabled = false;
			}
			Vanished();
			DoWithNonVanishingOrAppearingModelBlocks(delegate(Block b)
			{
				b.ModelBlockVanished();
			});
			vanishingOrAppearingBlocks.Remove(this);
			return TileResultCode.True;
		}
		if (CanAnimateScale())
		{
			GetVanishAnimCurve(num);
			float scale = vanishAnimCurve.Evaluate(timer);
			Vanishing(scale);
			DoWithNonVanishingOrAppearingModelBlocks(delegate(Block b)
			{
				b.ModelBlockVanishing(scale);
			});
		}
		return TileResultCode.Delayed;
	}

	public virtual TileResultCode VanishBlock(ScriptRowExecutionInfo eInfo, object[] args)
	{
		bool animate = CanAnimateScale() && Util.GetIntBooleanArg(args, 0, defaultValue: false);
		return VanishBlock(animate, eInfo.timer);
	}

	protected virtual void ModelBlockAppearing(float scale)
	{
	}

	protected virtual void ModelBlockVanishing(float scale)
	{
	}

	protected virtual void ModelBlockAppeared()
	{
	}

	protected virtual void ModelBlockVanished()
	{
	}

	protected virtual void Appearing(float scale)
	{
		if (go != null && CanAnimateScale())
		{
			goT.localScale = Vector3.one * scale;
			AdjustScaleForHierarchy();
		}
	}

	public virtual void Appeared()
	{
		if (go != null && CanAnimateScale())
		{
			goT.localScale = Vector3.one;
			AdjustScaleForHierarchy();
		}
		Rigidbody rb = chunk.rb;
		if (rb != null)
		{
			BlockWater worldOceanBlock = Blocksworld.worldOceanBlock;
			if (worldOceanBlock != null && !worldOceanBlock.SimulatesBlock(this))
			{
				worldOceanBlock.AddBlockToSimulation(this);
			}
		}
	}

	protected virtual void Vanishing(float scale)
	{
		if (go != null && CanAnimateScale())
		{
			goT.localScale = Vector3.one * scale;
			AdjustScaleForHierarchy();
		}
	}

	public virtual void Vanished()
	{
		go.SetActive(value: false);
	}

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
			gameObject.SetActive(value: true);
			if (!flag && block.vanished && !block.isTerrain)
			{
				CollisionManager.SetGhostBlockMode(block, CollisionManager.GhostBlockMode.Propagate);
			}
		}
	}

	private void WakeUpNonFrozenBlocksAppear(List<Block> connected, HashSet<Chunk> chunks)
	{
		foreach (Chunk chunk in chunks)
		{
			bool flag = chunk.blocks.Count > 0 && chunk.IsFrozen();
			if (chunk.go == null)
			{
				continue;
			}
			Rigidbody[] componentsInChildren = chunk.go.GetComponentsInChildren<Rigidbody>(includeInactive: true);
			if (!flag)
			{
				Rigidbody[] array = componentsInChildren;
				foreach (Rigidbody rigidbody in array)
				{
					rigidbody.isKinematic = false;
					rigidbody.WakeUp();
				}
			}
		}
		foreach (Block item in connected)
		{
			item.vanished = false;
			item.Appeared();
		}
	}

	public virtual TileResultCode AppearModel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		bool animate = Util.GetIntBooleanArg(args, 0, defaultValue: true);
		UpdateConnectedCache();
		List<Block> list = connectedCache[this];
		HashSet<Chunk> hashSet = connectedChunks[this];
		float num = ((!animate) ? 0f : 0.375f);
		if (eInfo.timer == 0f)
		{
			if (vanishingOrAppearingBlocks.Overlaps(list))
			{
				return TileResultCode.True;
			}
			vanishingOrAppearingBlocks.UnionWith(list);
			EnableAndShowBlocks(list);
		}
		if (eInfo.timer >= num)
		{
			WakeUpNonFrozenBlocksAppear(list, hashSet);
			CollisionManager.GhostBlockMode modelGhostMode = (list.Exists((Block b) => CollisionManager.GetGhostBlockInfo(b)?.didPropagate ?? false) ? CollisionManager.GhostBlockMode.NoPropagate : CollisionManager.GhostBlockMode.NotGhost);
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
			vanishingOrAppearingBlocks.ExceptWith(list);
			return TileResultCode.True;
		}
		GetVanishAnimCurve(num);
		float scale = Mathf.Clamp(vanishAnimCurve.Evaluate(num - eInfo.timer), 0.001f, 5f);
		foreach (Chunk item in hashSet)
		{
			for (int num2 = 0; num2 < item.blocks.Count; num2++)
			{
				Block block = item.blocks[num2];
				if (block.vanished)
				{
					block.Appearing(scale);
				}
			}
		}
		return TileResultCode.Delayed;
	}

	public virtual TileResultCode VanishModel(float timer, bool animate = true, bool forever = false, float delayPerBlock = 0f)
	{
		UpdateConnectedCache();
		List<Block> list = connectedCache[this];
		HashSet<Chunk> hashSet = connectedChunks[this];
		float num = 0.375f;
		GetVanishAnimCurve(num);
		if (timer == 0f)
		{
			bool flag = true;
			for (int i = 0; i < list.Count; i++)
			{
				Block block = list[i];
				if (!block.vanished || (forever && !BWSceneManager.playBlocksRemoved.Contains(block)))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return TileResultCode.True;
			}
			if (vanishingOrAppearingBlocks.Overlaps(list))
			{
				return TileResultCode.True;
			}
			vanishingOrAppearingBlocks.UnionWith(list);
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
				foreach (Chunk item in hashSet)
				{
					if (item != null && item.go != null)
					{
						Rigidbody rb = item.rb;
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
					if (!vanished)
					{
						block3.Vanishing(vanishAnimCurve.Evaluate(num));
					}
					block3.SetVanished(forever);
				}
				vanishingOrAppearingBlocks.ExceptWith(list);
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
			vanishingOrAppearingBlocks.ExceptWith(list);
			return TileResultCode.True;
		}
		float scale = vanishAnimCurve.Evaluate(timer);
		bool flag2 = delayPerBlock > 0f;
		int num2 = 0;
		foreach (Chunk item2 in hashSet)
		{
			if (item2 == null)
			{
				continue;
			}
			for (int m = 0; m < item2.blocks.Count; m++)
			{
				Block block5 = item2.blocks[m];
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
							float scale2 = vanishAnimCurve.Evaluate(num3);
							if (!vanished)
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
							flag2 = false;
						}
					}
					else
					{
						flag2 = false;
					}
				}
				else if (!vanished)
				{
					block5.Vanishing(scale);
				}
				num2++;
			}
		}
		if (flag2)
		{
			vanishingOrAppearingBlocks.ExceptWith(list);
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

	private void SetVanished(bool forever = false)
	{
		Vanished();
		if (!vanished)
		{
			vanished = true;
			CollisionManager.WakeUpObjectsRestingOn(this);
		}
		if (goShadow != null)
		{
			goShadow.GetComponent<Renderer>().enabled = false;
		}
		if (forever && !TreasureHandler.IsPartOfTreasureModel(this))
		{
			BWSceneManager.RemovePlayBlock(this);
		}
	}

	public virtual TileResultCode VanishModel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		bool intBooleanArg = Util.GetIntBooleanArg(args, 0, defaultValue: true);
		if (broken)
		{
			if (!vanished)
			{
				Blocksworld.AddFixedUpdateCommand(new VanishModelForeverCommand(this, intBooleanArg, 0f));
			}
			return TileResultCode.True;
		}
		return VanishModel(eInfo.timer, intBooleanArg);
	}

	public virtual TileResultCode VanishModelForever(ScriptRowExecutionInfo eInfo, object[] args)
	{
		bool intBooleanArg = Util.GetIntBooleanArg(args, 0, defaultValue: false);
		float floatArg = Util.GetFloatArg(args, 1, 0f);
		if (!vanished)
		{
			Blocksworld.AddFixedUpdateCommand(new VanishModelForeverCommand(this, intBooleanArg, floatArg));
		}
		return TileResultCode.True;
	}

	public Vector3 GetHierarchyScale()
	{
		Vector3 vector = Vector3.one;
		Transform parent = goT.parent;
		while (parent != null)
		{
			vector = Vector3.Scale(vector, parent.localScale);
			parent = parent.parent;
		}
		return vector;
	}

	private void AdjustScaleForHierarchy()
	{
		Vector3 hierarchyScale = GetHierarchyScale();
		if (hierarchyScale.x != 0f && hierarchyScale.y != 0f && hierarchyScale.z != 0f)
		{
			Vector3 a = new Vector3(1f / hierarchyScale.x, 1f / hierarchyScale.y, 1f / hierarchyScale.z);
			Vector3 localScale = goT.localScale;
			goT.localScale = Vector3.Scale(a, localScale);
		}
	}

	public TileResultCode Target(ScriptRowExecutionInfo eInfo, object[] args)
	{
		goT.parent.name = "Target";
		return TileResultCode.True;
	}

	public TileResultCode TutorialCreateBlockHint(ScriptRowExecutionInfo eInfo, object[] args)
	{
		return TileResultCode.True;
	}

	public TileResultCode TutorialPaintExistingBlock(ScriptRowExecutionInfo eInfo, object[] args)
	{
		return TileResultCode.True;
	}

	public TileResultCode TutorialTextureExistingBlock(ScriptRowExecutionInfo eInfo, object[] args)
	{
		return TileResultCode.True;
	}

	public TileResultCode TutorialRotateExistingBlock(ScriptRowExecutionInfo eInfo, object[] args)
	{
		return TileResultCode.True;
	}

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
		UISpeechBubble uISpeechBubble = Blocksworld.UI.SpeechBubble.OnBlockSpeak(this, text, direction);
		if (this is BlockCharacter || this is BlockAnimatedCharacter)
		{
			Blocksworld.worldSessionHadBlocksterSpeaker = true;
		}
		if (!durational || uISpeechBubble.ButtonPressed())
		{
			return TileResultCode.True;
		}
		float num = ((!uISpeechBubble.HasButtons()) ? 4 : 1000000);
		if (timer >= num)
		{
			return TileResultCode.True;
		}
		return TileResultCode.Delayed;
	}

	public TileResultCode Speak(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string text = (string)args[0];
		return Speak(text, eInfo.timer, 0);
	}

	public TileResultCode SpeakNonDurational(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string text = (string)args[0];
		int direction = ((args.Length > 1) ? ((int)args[1]) : 0);
		return Speak(text, eInfo.timer, direction, durational: false);
	}

	public TileResultCode ShowTextWindow(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string text = (string)args[0];
		Vector3 vector = (Vector3)args[1];
		float width = (float)args[2];
		string stringArg = Util.GetStringArg(args, 3, string.Empty);
		Blocksworld.UI.ShowTextWindow(this, text, vector, width, stringArg);
		return TileResultCode.True;
	}

	public TileResultCode GameOver(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (WinLoseManager.winning || WinLoseManager.losing || (WinLoseManager.playedStinger && eInfo.timer == 0f))
		{
			return TileResultCode.Delayed;
		}
		string stringArgSafe = Util.GetStringArgSafe(args, 2, "SFX win_level_stinger_01");
		UISpeechBubble uISpeechBubble = Blocksworld.UI.GetBlockTextWindow(this);
		if (uISpeechBubble == WinLoseManager.messageWindow && !Blocksworld.winIsWaiting)
		{
			string text = (string)args[0];
			uISpeechBubble = ShowCentralizedTextWindow(text, "Restart;Exit|GameDone", 400f, 100f);
			WinLoseManager.ending = true;
			WinLoseManager.messageWindow = uISpeechBubble;
		}
		ParticleSystem particleSystem = WinLoseManager.GetParticleSystem();
		Transform transform = Blocksworld.rewardCamera.transform;
		float num = 5f;
		if (eInfo.timer == 0f)
		{
			Blocksworld.musicPlayer.Stop();
			particleSystem.transform.position = transform.position + transform.forward * num;
			WinLoseManager.playedStinger = true;
			Sound.PlayOneShotSound(stringArgSafe);
		}
		if (eInfo.timer < 0.3f)
		{
			int num2 = Mathf.RoundToInt(100f / (30f * eInfo.timer + 1f));
			for (int i = 0; i < num2; i++)
			{
				Vector3 velocity = UnityEngine.Random.onUnitSphere * (UnityEngine.Random.value * 2f + 1f);
				Vector3 position = transform.up * (UnityEngine.Random.value - 0.5f) * num * 3f + transform.right * 2f * (UnityEngine.Random.value - 0.5f) * num * 2f;
				float num3 = UnityEngine.Random.value * 0.5f + 0.25f;
				float lifetime = 1.7f + UnityEngine.Random.value * 0.5f;
				Color color = ((UnityEngine.Random.value <= 0.5f) ? Color.yellow : Color.white);
				particleSystem.Emit(position, velocity, num3, lifetime, color);
			}
		}
		if (uISpeechBubble != null && uISpeechBubble.ButtonPressed())
		{
			WinLoseManager.buttonPressed = true;
		}
		return TileResultCode.Delayed;
	}

	public TileResultCode GameWin(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (WinLoseManager.losing || WinLoseManager.ending || (WinLoseManager.playedStinger && eInfo.timer == 0f))
		{
			return TileResultCode.Delayed;
		}
		Blocksworld.HandleWin();
		float floatArg = Util.GetFloatArg(args, 1, 0f);
		bool flag = floatArg == 0f && WorldSession.canShowLeaderboard();
		float num = ((floatArg >= 4f) ? floatArg : 0f);
		string stringArgSafe = Util.GetStringArgSafe(args, 2, "SFX win_level_stinger_01");
		bool flag2 = num > 0f;
		UISpeechBubble uISpeechBubble = Blocksworld.UI.GetBlockTextWindow(this);
		if (uISpeechBubble == WinLoseManager.messageWindow && !Blocksworld.winIsWaiting)
		{
			string text = ((!flag) ? "Exit" : "Next");
			string buttons = "Restart;" + text + "|GameDone";
			string text2 = (string)args[0];
			if (flag2)
			{
				string[] array = ((string)args[0]).Split(';');
				if (array.Length > 1 && !string.IsNullOrEmpty(array[1]))
				{
					buttons = array[1] + "|Exit";
					text2 = array[0];
				}
				else
				{
					buttons = "Exit";
				}
			}
			if (flag && leaderboardData != null && !leaderboardData.temporarilyDisableTimer)
			{
				leaderboardData.FinishLeaderboard(text2);
			}
			else
			{
				uISpeechBubble = ShowCentralizedTextWindow(text2, buttons, 400f, 100f);
			}
			WinLoseManager.winning = true;
			WinLoseManager.messageWindow = uISpeechBubble;
		}
		ParticleSystem particleSystem = WinLoseManager.GetParticleSystem();
		Transform transform = Blocksworld.rewardCamera.transform;
		float num2 = 5f;
		if (eInfo.timer == 0f)
		{
			Blocksworld.musicPlayer.Stop();
			particleSystem.transform.position = transform.position + transform.forward * num2;
			WinLoseManager.playedStinger = true;
			Sound.PlayOneShotSound(stringArgSafe);
		}
		if (eInfo.timer < 0.3f)
		{
			int num3 = Mathf.RoundToInt(100f / (30f * eInfo.timer + 1f));
			for (int i = 0; i < num3; i++)
			{
				Vector3 velocity = UnityEngine.Random.onUnitSphere * (UnityEngine.Random.value * 2f + 1f);
				Vector3 position = transform.up * (UnityEngine.Random.value - 0.5f) * num2 * 3f + transform.right * 2f * (UnityEngine.Random.value - 0.5f) * num2 * 2f;
				float num4 = UnityEngine.Random.value * 0.5f + 0.25f;
				float lifetime = 1.7f + UnityEngine.Random.value * 0.5f;
				Color color = ((UnityEngine.Random.value <= 0.5f) ? Color.yellow : Color.white);
				particleSystem.Emit(position, velocity, num4, lifetime, color);
			}
		}
		if (uISpeechBubble != null && uISpeechBubble.ButtonPressed())
		{
			WinLoseManager.buttonPressed = true;
		}
		if (flag2)
		{
			if (WinLoseManager.buttonPressed)
			{
				return Win();
			}
			return TileResultCode.Delayed;
		}
		return TileResultCode.Delayed;
	}

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
			Sound.PlayOneShotSound(stringArgSafe);
			WinLoseManager.playedStinger = true;
		}
		UISpeechBubble blockTextWindow = Blocksworld.UI.GetBlockTextWindow(this);
		if (blockTextWindow == WinLoseManager.messageWindow)
		{
			if (WorldSession.canShowLeaderboard() && leaderboardData != null && !leaderboardData.temporarilyDisableTimer)
			{
				leaderboardData.LoseCondition();
			}
			blockTextWindow = ShowCentralizedTextWindow((string)args[0], "Restart", 400f, 100f);
			WinLoseManager.losing = true;
			WinLoseManager.messageWindow = blockTextWindow;
		}
		return TileResultCode.Delayed;
	}

	public virtual TileResultCode RegisterTag(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string tagName = (string)args[0];
		TagManager.RegisterBlockTag(this, tagName);
		return TileResultCode.True;
	}

	private UISpeechBubble ShowCentralizedTextWindow(string text, string buttons, float width, float y)
	{
		float x = (float)NormalizedScreen.width * 0.5f - width * 0.5f;
		Vector3 vector = new Vector3(x, y, 3f);
		return Blocksworld.UI.ShowTextWindow(this, text, vector, width, buttons);
	}

	public TileResultCode IgnoreAction(ScriptRowExecutionInfo eInfo, object[] args)
	{
		return TileResultCode.True;
	}

	public TileResultCode IgnoreSensor(ScriptRowExecutionInfo eInfo, object[] args)
	{
		return TileResultCode.True;
	}

	public TileResultCode TestAction(ScriptRowExecutionInfo eInfo, object[] args)
	{
		return TileResultCode.True;
	}

	public TileResultCode TestSensor(ScriptRowExecutionInfo eInfo, object[] args)
	{
		return TileResultCode.False;
	}

	public TileResultCode IsTestObjectiveDone(ScriptRowExecutionInfo eInfo, object[] args)
	{
		return TileResultCode.False;
	}

	public TileResultCode SetTestObjectiveDone(ScriptRowExecutionInfo eInfo, object[] args)
	{
		return TileResultCode.True;
	}

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

	public TileResultCode Win(ScriptRowExecutionInfo eInfo, object[] args)
	{
		return Win();
	}

	internal static void ResetTiltOrientation()
	{
		TiltManager.Instance.ResetOrientation();
	}

	private static float GetTilt(bool xDirection, float maxAngle)
	{
		float num = ((!xDirection) ? TiltManager.Instance.GetRelativeGravityVector().y : TiltManager.Instance.GetTiltTwist());
		return Mathf.Clamp(num * 90f / Mathf.Max(10f, maxAngle), -1f, 1f);
	}

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
			HandleTiltMover(vector.x, vector.y, zTilt);
		}
		else
		{
			HandleTiltMover(vector2.x, vector2.y, zTilt);
		}
		return TileResultCode.True;
	}

	protected virtual void HandleTiltMover(float xTilt, float yTilt, float zTilt)
	{
	}

	public TileResultCode IsTiltedFrontBack(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float f = (float)args[0];
		float floatArg = Util.GetFloatArg(args, 1, 30f);
		float tilt = GetTilt(xDirection: false, floatArg);
		Blocksworld.UI.Controls.UpdateTiltPrompt();
		if (Mathf.Abs(tilt) > 0.01f && Mathf.Sign(f) == Mathf.Sign(tilt))
		{
			eInfo.floatArg = Mathf.Min(eInfo.floatArg, Mathf.Abs(tilt));
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

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

	public TileResultCode IsTiltedLeftRight(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float f = (float)args[0];
		float floatArg = Util.GetFloatArg(args, 1, 30f);
		float tilt = GetTilt(xDirection: true, floatArg);
		Blocksworld.UI.Controls.UpdateTiltPrompt();
		if (Mathf.Abs(tilt) > 0.01f && Mathf.Sign(f) == Mathf.Sign(tilt))
		{
			eInfo.floatArg = Mathf.Min(eInfo.floatArg, Mathf.Abs(tilt));
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsDPadHorizontal(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float num = (float)args[0];
		string key = ((args.Length <= 1) ? "L" : ((string)args[1]));
		Blocksworld.UI.Controls.EnableDPad(key, (num <= 0f) ? MoverDirectionMask.LEFT : MoverDirectionMask.RIGHT);
		if (Blocksworld.UI.Controls.IsDPadActive(key))
		{
			Vector2 normalizedDPadOffset = Blocksworld.UI.Controls.GetNormalizedDPadOffset(key);
			if (Mathf.Sign(normalizedDPadOffset.x) == num && Mathf.Abs(normalizedDPadOffset.x) > 0.001f)
			{
				eInfo.floatArg = Mathf.Min(num * normalizedDPadOffset.x, eInfo.floatArg);
				wasHorizPositive = num > 0f;
				wasHorizNegative = num < 0f;
				return TileResultCode.True;
			}
		}
		eInfo.floatArg = 0f;
		if (wasHorizPositive && num > 0f)
		{
			wasHorizPositive = false;
			return TileResultCode.True;
		}
		if (wasHorizNegative && num < 0f)
		{
			wasHorizNegative = false;
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsDPadVertical(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float num = (float)args[0];
		string key = ((args.Length <= 1) ? "L" : ((string)args[1]));
		Blocksworld.UI.Controls.EnableDPad(key, (num <= 0f) ? MoverDirectionMask.DOWN : MoverDirectionMask.UP);
		if (Blocksworld.UI.Controls.IsDPadActive(key))
		{
			Vector2 normalizedDPadOffset = Blocksworld.UI.Controls.GetNormalizedDPadOffset(key);
			if (Mathf.Sign(normalizedDPadOffset.y) == num && Mathf.Abs(normalizedDPadOffset.y) > 0.001f)
			{
				eInfo.floatArg = Mathf.Min(eInfo.floatArg, num * normalizedDPadOffset.y);
				wasVertPositive = num > 0f;
				wasVertNegative = num < 0f;
				return TileResultCode.True;
			}
		}
		eInfo.floatArg = 0f;
		if (wasVertPositive && num > 0f)
		{
			wasVertPositive = false;
			return TileResultCode.True;
		}
		if (wasVertNegative && num < 0f)
		{
			wasVertNegative = false;
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsDPadMoved(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string key = ((args.Length == 0) ? "L" : ((string)args[0]));
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

	public TileResultCode IsSendingSignal(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int num = (int)args[0];
		float floatArg = Util.GetFloatArg(args, 1, 1f);
		bool flag = Blocksworld.sending[num];
		eInfo.floatArg = floatArg * Mathf.Min(eInfo.floatArg, Blocksworld.sendingValues[num]);
		if (flag)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode SendSignal(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int num = (int)args[0];
		float floatArg = Util.GetFloatArg(args, 1, 1f);
		Blocksworld.sending[num] = true;
		Blocksworld.sendingValues[num] = Mathf.Max(eInfo.floatArg * floatArg, Blocksworld.sendingValues[num]);
		return TileResultCode.True;
	}

	public TileResultCode IsSendingSignalModel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int index = (int)args[0];
		float floatArg = Util.GetFloatArg(args, 1, 1f);
		float signalStrength;
		bool flag = ModelSignals.IsSendingSignal(this, index, out signalStrength);
		eInfo.floatArg = floatArg * Mathf.Min(eInfo.floatArg, signalStrength);
		if (flag)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode SendSignalModel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int index = (int)args[0];
		float floatArg = Util.GetFloatArg(args, 1, 1f);
		ModelSignals.SendSignal(this, index, floatArg);
		return TileResultCode.True;
	}

	public TileResultCode IsSendingCustomSignal(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		float floatArg = Util.GetFloatArg(args, 1, 1f);
		if (Blocksworld.sendingCustom.TryGetValue(stringArg, out var value))
		{
			eInfo.floatArg = floatArg * Mathf.Min(eInfo.floatArg, value);
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode SendCustomSignal(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		float floatArg = Util.GetFloatArg(args, 1, 1f);
		if (!Blocksworld.sendingCustom.TryGetValue(stringArg, out var value))
		{
			value = 1f;
		}
		Blocksworld.sendingCustom[stringArg] = Mathf.Max(eInfo.floatArg * floatArg, value);
		return TileResultCode.True;
	}

	public TileResultCode IsSendingCustomSignalModel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		float floatArg = Util.GetFloatArg(args, 1, 1f);
		if (ModelSignals.IsSendingCustomSignal(this, eInfo, stringArg, floatArg))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode SendCustomSignalModel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		float floatArg = Util.GetFloatArg(args, 1, 1f);
		ModelSignals.SendCustomSignal(this, eInfo, stringArg, floatArg);
		return TileResultCode.True;
	}

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

	public TileResultCode VariableAssign(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		if (Blocksworld.customIntVariables.ContainsKey(stringArg))
		{
			int intArg = Util.GetIntArg(args, 1, 0);
			float num = Mathf.Sign(intArg);
			int value = (int)(num * Mathf.Floor(eInfo.floatArg * ((float)Mathf.Abs(intArg) + 0.49f)));
			Blocksworld.customIntVariables[stringArg] = value;
		}
		return TileResultCode.True;
	}

	public TileResultCode VariableAdd(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		if (Blocksworld.customIntVariables.ContainsKey(stringArg))
		{
			int intArg = Util.GetIntArg(args, 1, 1);
			Blocksworld.customIntVariables[stringArg] += intArg;
		}
		return TileResultCode.True;
	}

	public TileResultCode VariableSubtract(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		if (Blocksworld.customIntVariables.ContainsKey(stringArg))
		{
			int intArg = Util.GetIntArg(args, 1, 1);
			Blocksworld.customIntVariables[stringArg] -= intArg;
		}
		return TileResultCode.True;
	}

	public TileResultCode VariableMultiply(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		if (Blocksworld.customIntVariables.ContainsKey(stringArg))
		{
			int intArg = Util.GetIntArg(args, 1, 1);
			Blocksworld.customIntVariables[stringArg] *= intArg;
		}
		return TileResultCode.True;
	}

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

	public TileResultCode VariableEquals(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		bool flag = false;
		if (Blocksworld.customIntVariables.ContainsKey(stringArg))
		{
			int intArg = Util.GetIntArg(args, 1, 0);
			flag = Blocksworld.customIntVariables[stringArg] == intArg;
		}
		if (flag)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode VariableNotEquals(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		bool flag = false;
		if (Blocksworld.customIntVariables.ContainsKey(stringArg))
		{
			int intArg = Util.GetIntArg(args, 1, 0);
			flag = Blocksworld.customIntVariables[stringArg] != intArg;
		}
		if (flag)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode VariableLessThan(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		bool flag = false;
		if (Blocksworld.customIntVariables.ContainsKey(stringArg))
		{
			int intArg = Util.GetIntArg(args, 1, 0);
			flag = Blocksworld.customIntVariables[stringArg] < intArg;
		}
		if (flag)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode VariableMoreThan(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		bool flag = false;
		if (Blocksworld.customIntVariables.ContainsKey(stringArg))
		{
			int intArg = Util.GetIntArg(args, 1, 0);
			flag = Blocksworld.customIntVariables[stringArg] > intArg;
		}
		if (flag)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

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

	public TileResultCode BlockVariableAssign(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (Blocksworld.blockIntVariables.ContainsKey(this))
		{
			Dictionary<string, int> dictionary = Blocksworld.blockIntVariables[this];
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			if (dictionary.ContainsKey(stringArg))
			{
				int intArg = Util.GetIntArg(args, 1, 0);
				float num = Mathf.Sign(intArg);
				int value = (int)(num * Mathf.Floor(eInfo.floatArg * ((float)Mathf.Abs(intArg) + 0.49f)));
				dictionary[stringArg] = value;
			}
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

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

	public TileResultCode BlockVariableAssignSpeed(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (Blocksworld.blockIntVariables.ContainsKey(this))
		{
			Dictionary<string, int> dictionary = Blocksworld.blockIntVariables[this];
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			if (dictionary.ContainsKey(stringArg))
			{
				int value = 0;
				if (chunk != null && chunk.rb != null)
				{
					value = (int)chunk.rb.velocity.magnitude;
				}
				dictionary[stringArg] = value;
			}
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode BlockVariableAssignAltitude(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (Blocksworld.blockIntVariables.ContainsKey(this))
		{
			Dictionary<string, int> dictionary = Blocksworld.blockIntVariables[this];
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			if (dictionary.ContainsKey(stringArg))
			{
				int value = (int)goT.position.y;
				dictionary[stringArg] = value;
			}
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode BlockVariableAdd(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (Blocksworld.blockIntVariables.ContainsKey(this))
		{
			Dictionary<string, int> dictionary = Blocksworld.blockIntVariables[this];
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			int intArg = Util.GetIntArg(args, 1, 1);
			if (dictionary.ContainsKey(stringArg))
			{
				dictionary[stringArg] += intArg;
			}
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode BlockVariableSubtract(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (Blocksworld.blockIntVariables.ContainsKey(this))
		{
			Dictionary<string, int> dictionary = Blocksworld.blockIntVariables[this];
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			int intArg = Util.GetIntArg(args, 1, 1);
			if (dictionary.ContainsKey(stringArg))
			{
				dictionary[stringArg] -= intArg;
			}
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode BlockVariableMultiply(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (Blocksworld.blockIntVariables.ContainsKey(this))
		{
			Dictionary<string, int> dictionary = Blocksworld.blockIntVariables[this];
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			int intArg = Util.GetIntArg(args, 1, 1);
			if (dictionary.ContainsKey(stringArg))
			{
				dictionary[stringArg] *= intArg;
			}
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

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
				flag = dictionary[stringArg] == intArg;
			}
			if (flag)
			{
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}
		return TileResultCode.False;
	}

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
				flag = dictionary[stringArg] != intArg;
			}
			if (flag)
			{
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}
		return TileResultCode.False;
	}

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
				flag = dictionary[stringArg] < intArg;
			}
			if (flag)
			{
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}
		return TileResultCode.False;
	}

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
				flag = dictionary[stringArg] > intArg;
			}
			if (flag)
			{
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}
		return TileResultCode.False;
	}

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
			if (flag)
			{
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}
		return TileResultCode.False;
	}

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
			if (flag)
			{
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}
		return TileResultCode.False;
	}

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
				float num2 = Mathf.Sign(num);
				int value = (int)(num2 * Mathf.Floor(eInfo.floatArg * ((float)Mathf.Abs(num) + 0.49f)));
				dictionary[stringArg] = value;
			}
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode BlockVariableAddBV(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (Blocksworld.blockIntVariables.ContainsKey(this))
		{
			Dictionary<string, int> dictionary = Blocksworld.blockIntVariables[this];
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			string stringArg2 = Util.GetStringArg(args, 1, string.Empty);
			if (dictionary.ContainsKey(stringArg) && dictionary.ContainsKey(stringArg2))
			{
				dictionary[stringArg] += dictionary[stringArg2];
			}
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode BlockVariableSubtractBV(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (Blocksworld.blockIntVariables.ContainsKey(this))
		{
			Dictionary<string, int> dictionary = Blocksworld.blockIntVariables[this];
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			string stringArg2 = Util.GetStringArg(args, 1, string.Empty);
			if (dictionary.ContainsKey(stringArg) && dictionary.ContainsKey(stringArg2))
			{
				dictionary[stringArg] -= dictionary[stringArg2];
			}
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode BlockVariableMultiplyBV(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (Blocksworld.blockIntVariables.ContainsKey(this))
		{
			Dictionary<string, int> dictionary = Blocksworld.blockIntVariables[this];
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			string stringArg2 = Util.GetStringArg(args, 1, string.Empty);
			if (dictionary.ContainsKey(stringArg) && dictionary.ContainsKey(stringArg2))
			{
				dictionary[stringArg] *= dictionary[stringArg2];
			}
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

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
				flag = dictionary[stringArg] == dictionary[stringArg2];
			}
			if (flag)
			{
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}
		return TileResultCode.False;
	}

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
				flag = dictionary[stringArg] != dictionary[stringArg2];
			}
			if (flag)
			{
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}
		return TileResultCode.False;
	}

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
				flag = dictionary[stringArg] < dictionary[stringArg2];
			}
			if (flag)
			{
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}
		return TileResultCode.False;
	}

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
				flag = dictionary[stringArg] > dictionary[stringArg2];
			}
			if (flag)
			{
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}
		return TileResultCode.False;
	}

	private bool AnyWithinWater(List<Block> blocks)
	{
		for (int i = 0; i < blocks.Count; i++)
		{
			Block block = blocks[i];
			if (block.CanTriggerBlockListSensor() && BlockWater.BlockWithinWater(block))
			{
				return true;
			}
		}
		return false;
	}

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

	public virtual TileResultCode IsWithinWater(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (BlockWater.BlockWithinWater(this))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsAnyBlockInModelWithinWater(ScriptRowExecutionInfo eInfo, object[] args)
	{
		UpdateConnectedCache();
		if (AnyWithinWater(connectedCache[this]))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsAnyBlockInChunkWithinWater(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (AnyWithinWater(chunk.blocks))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public virtual TileResultCode WithinTaggedWater(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		if (BlockWater.BlockWithinTaggedWater(this, stringArg))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode WithinTaggedWaterModel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		UpdateConnectedCache();
		if (AnyWithinTaggedWater(connectedCache[this], stringArg))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode WithinTaggedWaterChunk(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		if (AnyWithinTaggedWater(chunk.blocks, stringArg))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode WithinTaggedBlock(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		List<Block> blocksWithTag = TagManager.GetBlocksWithTag(stringArg);
		Collider component = go.GetComponent<Collider>();
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
			if (!(component2 == null) && component2.bounds.Intersects(bounds))
			{
				return TileResultCode.True;
			}
		}
		return TileResultCode.False;
	}

	public TileResultCode IsTappingBlock(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (IsTapHoldingBlock(eInfo, args) == TileResultCode.True && goTouchStarted)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public virtual TileResultCode IsTapHoldingBlock(ScriptRowExecutionInfo eInfo, object[] args)
	{
		bool flag = goTouched == go;
		Blocksworld.worldSessionHadBlockTap |= flag;
		if (flag)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	private bool IsTappingAny(List<Block> blocks)
	{
		for (int i = 0; i < blocks.Count; i++)
		{
			Block block = blocks[i];
			if (goTouched == block.go)
			{
				Blocksworld.worldSessionHadBlockTap = true;
				return true;
			}
		}
		return false;
	}

	public TileResultCode IsTappingAnyBlockInModel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (IsTapHoldingAnyBlockInModel(eInfo, args) == TileResultCode.True && goTouchStarted)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsTapHoldingAnyBlockInModel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		UpdateConnectedCache();
		if (IsTappingAny(connectedCache[this]))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsTappingAnyBlockInChunk(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (IsTappingAny(chunk.blocks))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public virtual TileResultCode IsFasterThan(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float num = Util.GetFloatArg(args, 0, 0f) + 0.5f;
		bool flag = false;
		if (chunk != null && chunk.rb != null)
		{
			flag = chunk.rb.velocity.sqrMagnitude > num * num;
		}
		if (flag)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public virtual TileResultCode IsSlowerThan(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float num = Mathf.Max(Util.GetFloatArg(args, 0, 1f) - 0.5f, 0f);
		bool flag = false;
		if (chunk != null && chunk.rb != null)
		{
			flag = chunk.rb.velocity.sqrMagnitude < num * num;
		}
		if (flag)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	private bool GlueOnContactBlock(Block b, float sizeFactor = 1.5f)
	{
		if (CollisionManager.bumpedBy.TryGetValue(b, out var value) && !value.isTerrain && value.modelBlock != modelBlock && !value.didFix)
		{
			Chunk chunk = value.chunk;
			if (chunk.rb != null && !chunk.rb.isKinematic)
			{
				Chunk chunk2 = b.chunk;
				BlockData blockData = BlockData.GetBlockData(this);
				if (!blockData.TryGetFloat("CGOCS", out var value2))
				{
					value2 = chunk2.CalculateVolumeWithSizes();
					blockData.SetFloat("CGOCS", value2);
				}
				if (chunk2 != chunk)
				{
					float num = chunk.CalculateVolumeWithSizes();
					if (num < value2 * sizeFactor)
					{
						for (int i = 0; i < chunk.blocks.Count; i++)
						{
							Block block = chunk.blocks[i];
							if (block.allowGlueOnContact)
							{
								chunk.blocks.Remove(block);
								chunk2.AddBlock(block);
								block.gluedOnContact = true;
							}
						}
						if (chunk.blocks.Count == 0)
						{
							Blocksworld.blocksworldCamera.Unfollow(chunk);
							Blocksworld.chunks.Remove(chunk);
							chunk.Destroy();
						}
						else
						{
							chunk.UpdateCenterOfMass();
						}
						chunk2.UpdateCenterOfMass();
						blockData.SetFloat("CGOCS", chunk2.CalculateVolumeWithSizes());
						return true;
					}
				}
			}
		}
		return false;
	}

	public TileResultCode IsGlueOnContact(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (gluedOnContact)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode GlueOnContact(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (broken || vanished)
		{
			return TileResultCode.True;
		}
		float floatArg = Util.GetFloatArg(args, 0, 1.5f);
		GlueOnContactBlock(this, floatArg);
		return TileResultCode.True;
	}

	public TileResultCode GlueOnContactChunk(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (broken || vanished)
		{
			return TileResultCode.True;
		}
		if (chunk.rb != null)
		{
			float floatArg = Util.GetFloatArg(args, 0, 1.5f);
			for (int i = 0; i < chunk.blocks.Count && !GlueOnContactBlock(chunk.blocks[i], floatArg); i++)
			{
			}
		}
		return TileResultCode.True;
	}

	public TileResultCode ReleaseGlueOnContact(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (broken || vanished)
		{
			return TileResultCode.True;
		}
		ReleaseGlueOnContactCommand c = new ReleaseGlueOnContactCommand(this);
		Blocksworld.AddFixedUpdateCommand(c);
		return TileResultCode.True;
	}

	public TileResultCode IsAllowGlueOnContact(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if ((!Util.GetIntBooleanArg(args, 0, defaultValue: true)) ? (!allowGlueOnContact) : allowGlueOnContact)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode AllowGlueOnContact(ScriptRowExecutionInfo eInfo, object[] args)
	{
		allowGlueOnContact = Util.GetIntBooleanArg(args, 0, defaultValue: true);
		return TileResultCode.True;
	}

	public virtual void TeleportTo(Vector3 targetPos, bool resetAngle = false, bool resetVel = false, bool resetAngVel = false)
	{
		Vector3 position = goT.position;
		if (!((double)(targetPos - position).sqrMagnitude > 0.1))
		{
			return;
		}
		UpdateConnectedCache();
		Chunk chunk = this.chunk;
		if (chunk.go == null)
		{
			return;
		}
		Vector3 position2 = chunk.go.transform.position;
		HashSet<Chunk> hashSet = connectedChunks[this];
		foreach (Chunk item in hashSet)
		{
			GameObject gameObject = item.go;
			Transform transform = gameObject.transform;
			Vector3 vector = transform.position - position2;
			if (resetAngle)
			{
				transform.rotation = parentPlayRotation;
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
			transform.position = targetPos + vector;
			BlockWater worldOceanBlock = Blocksworld.worldOceanBlock;
			for (int i = 0; i < item.blocks.Count; i++)
			{
				Block block = item.blocks[i];
				block.Teleported(resetAngle, resetVel, resetAngVel);
				if (worldOceanBlock != null && !worldOceanBlock.SimulatesBlock(block))
				{
					worldOceanBlock.AddBlockToSimulation(block);
				}
				Blocksworld.blocksworldCamera.HandleTeleport(block);
			}
		}
	}

	public Vector3 GetSafeTeleportToPosition(Vector3 targetPos)
	{
		Vector3 position = goT.position;
		Vector3 vector = targetPos - position;
		UpdateConnectedCache();
		if (connectedCache == null)
		{
			Debug.Log("connectedCache is null!");
		}
		HashSet<Block> hashSet = new HashSet<Block>(connectedCache[this]);
		Vector3 result = targetPos;
		if (hashSet.Count > 0)
		{
			Bounds bounds = default(Bounds);
			bool flag = true;
			foreach (Block item in hashSet)
			{
				Collider component = item.go.GetComponent<Collider>();
				if (flag)
				{
					flag = false;
					bounds = ((!(component != null)) ? GetBounds() : component.bounds);
				}
				else if (component != null)
				{
					bounds.Encapsulate(component.bounds);
				}
				else
				{
					bounds.Encapsulate(GetBounds());
				}
			}
			bounds.center += vector;
			result = targetPos + GetSafeNonOverlapOffset(bounds, hashSet);
		}
		return result;
	}

	private Vector3 GetSafeNonOverlapOffset(Bounds bounds, HashSet<Block> safeBlocks)
	{
		Vector3 vector = bounds.size * 1.1f;
		int[] array = new int[3] { 0, 1, -1 };
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
					int num2 = ApproximateOverlappingCollidersCount(testBounds, safeBlocks, num);
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

	private int ApproximateOverlappingCollidersCount(Bounds testBounds, HashSet<Block> safeBlocks, int bestCount)
	{
		Collider[] array = Physics.OverlapSphere(testBounds.center, Util.MaxComponent(testBounds.extents));
		int num = 0;
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			if (!collider.isTrigger && collider.bounds.Intersects(testBounds))
			{
				Block block = BWSceneManager.FindBlock(collider.gameObject, checkChildGos: true);
				if (!safeBlocks.Contains(block))
				{
					if (block.isTerrain)
					{
						if (Util.PointWithinTerrain(testBounds.center) || Util.PointWithinTerrain(testBounds.center + Vector3.up * testBounds.extents.y * 0.7f) || Util.PointWithinTerrain(testBounds.center - Vector3.up * testBounds.extents.y * 0.7f))
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

	public virtual void Teleported(bool resetAngle = false, bool resetVel = false, bool resetAngVel = false)
	{
		lastTeleportedFrameCount = Blocksworld.playFixedUpdateCounter + 1;
	}

	public TileResultCode Teleported(ScriptRowExecutionInfo eInfo, object[] args)
	{
		bool result = lastTeleportedFrameCount == Blocksworld.playFixedUpdateCounter;
		return boolToTileResult(result);
	}

	public TileResultCode TeleportToTag(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string tagName = ((args.Length == 0) ? string.Empty : ((string)args[0]));
		Vector3 position = goT.position;
		if (TagManager.TryGetClosestBlockWithTag(tagName, position, out var block))
		{
			bool intBooleanArg = Util.GetIntBooleanArg(args, 1, defaultValue: true);
			bool intBooleanArg2 = Util.GetIntBooleanArg(args, 2, defaultValue: true);
			bool intBooleanArg3 = Util.GetIntBooleanArg(args, 3, defaultValue: true);
			Vector3 targetPos = block.goT.position;
			if (Util.GetIntBooleanArg(args, 4, defaultValue: true))
			{
				targetPos = GetSafeTeleportToPosition(targetPos);
			}
			TeleportTo(targetPos, intBooleanArg, intBooleanArg2, intBooleanArg3);
			return TileResultCode.True;
		}
		return TileResultCode.True;
	}

	public TileResultCode TagProximityCheck(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string posName = ((args.Length == 0) ? string.Empty : ((string)args[0]));
		List<Block> blocksWithTag = TagManager.GetBlocksWithTag(posName);
		if (blocksWithTag.Count == 0)
		{
			return TileResultCode.False;
		}
		float num = ((args.Length <= 1) ? 15f : ((float)args[1]));
		bool flag = args.Length <= 2 || (int)args[2] != 0;
		Vector3 position = goT.position;
		if (flag)
		{
			for (int i = 0; i < blocksWithTag.Count; i++)
			{
				Block block = blocksWithTag[i];
				Vector3 position2 = block.goT.position;
				Collider component = block.go.GetComponent<Collider>();
				Vector3 vector = ((!(component != null)) ? position2 : component.ClosestPointOnBounds(position));
				float sqrMagnitude = (vector - position).sqrMagnitude;
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
			Vector3 vector2 = ((!(component2 != null)) ? position3 : component2.ClosestPointOnBounds(position));
			float sqrMagnitude2 = (vector2 - position).sqrMagnitude;
			if (sqrMagnitude2 < num * num)
			{
				return TileResultCode.False;
			}
		}
		return TileResultCode.True;
	}

	public TileResultCode TagVisibilityCheck(string tagName, bool triggerWhenInvisible, bool ignoreModel = false)
	{
		List<Block> blocksWithTag = TagManager.GetBlocksWithTag(tagName);
		Vector3 position = goT.position;
		int layermask = 539;
		for (int i = 0; i < blocksWithTag.Count; i++)
		{
			Block block = blocksWithTag[i];
			GameObject gameObject = block.go;
			if (!gameObject.activeSelf)
			{
				continue;
			}
			Vector3 position2 = gameObject.transform.position;
			Collider component = gameObject.GetComponent<Collider>();
			Vector3 direction = position2 - position;
			float num = direction.magnitude;
			RaycastHit[] array = Physics.RaycastAll(position, direction, num + 0.1f, layermask);
			bool flag = array.Length == 0;
			float num2 = Util.MinComponent(block.size) * 0.9f;
			RaycastHit[] array2 = array;
			for (int j = 0; j < array2.Length; j++)
			{
				RaycastHit raycastHit = array2[j];
				Collider collider = raycastHit.collider;
				if (collider == component)
				{
					flag = true;
					num = raycastHit.distance;
					continue;
				}
				Block block2 = BWSceneManager.FindBlock(collider.gameObject);
				if (block2 != null && block2 != this && (!ignoreModel || (block2.modelBlock != block.modelBlock && block2.modelBlock != modelBlock)) && !Materials.TextureIsTransparent(block2.GetTexture()) && raycastHit.distance + num2 < num)
				{
					flag = false;
					break;
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
		if (triggerWhenInvisible)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode TagVisibilityCheck(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string tagName = ((args.Length == 0) ? string.Empty : ((string)args[0]));
		bool triggerWhenInvisible = args.Length > 1 && (int)args[1] != 0;
		return TagVisibilityCheck(tagName, triggerWhenInvisible);
	}

	public TileResultCode TagVisibilityCheckModel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string tagName = ((args.Length == 0) ? string.Empty : ((string)args[0]));
		bool triggerWhenInvisible = args.Length > 1 && (int)args[1] != 0;
		return TagVisibilityCheck(tagName, triggerWhenInvisible, ignoreModel: true);
	}

	public TileResultCode TagFrustumCheck(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string posName = ((args.Length == 0) ? string.Empty : ((string)args[0]));
		bool flag = args.Length > 1 && (int)args[1] != 0;
		float num = ((args.Length <= 2) ? 90f : ((float)args[2]));
		float num2 = ((args.Length <= 3) ? 90f : ((float)args[3]));
		Vector3 vector = ((args.Length <= 4) ? Vector3.forward : ((Vector3)args[4]));
		List<Block> blocksWithTag = TagManager.GetBlocksWithTag(posName);
		Transform transform = goT;
		Vector3 position = transform.position;
		Vector3 forward = transform.forward;
		Vector3 up = transform.up;
		Vector3 right = transform.right;
		Vector3 normalized = (right * vector.x + up * vector.y + forward * vector.z).normalized;
		for (int i = 0; i < blocksWithTag.Count; i++)
		{
			Block block = blocksWithTag[i];
			Vector3 position2 = block.goT.position;
			Vector3 vector2 = position2 - position;
			float magnitude = vector2.magnitude;
			if (magnitude >= 0.01f)
			{
				Vector3 vec = vector2 / magnitude;
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

	public TileResultCode CounterUIValueCondition(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 2, 0);
		if (BlockCounterUI.allCounterBlocks.TryGetValue(intArg, out var value))
		{
			int intArg2 = Util.GetIntArg(args, 0, 1);
			int intArg3 = Util.GetIntArg(args, 1, 0);
			return value.ValueCondition(intArg2, intArg3);
		}
		return TileResultCode.False;
	}

	public TileResultCode GaugeUIValueCondition(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 2, 0);
		if (BlockGaugeUI.allGaugeBlocks.TryGetValue(intArg, out var value))
		{
			int intArg2 = Util.GetIntArg(args, 0, 1);
			int intArg3 = Util.GetIntArg(args, 1, 0);
			return value.ValueCondition(intArg2, intArg3);
		}
		return TileResultCode.False;
	}

	public TileResultCode TimerUIValueCondition(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 2, 0);
		if (BlockTimerUI.allTimerBlocks.TryGetValue(intArg, out var value))
		{
			float floatArg = Util.GetFloatArg(args, 0, 1f);
			int intArg2 = Util.GetIntArg(args, 1, 0);
			return value.ValueCondition(floatArg, intArg2);
		}
		return TileResultCode.False;
	}

	public TileResultCode TimerUITimeEquals(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 1, 0);
		if (BlockTimerUI.allTimerBlocks.TryGetValue(intArg, out var value))
		{
			float time = ((args.Length == 0) ? 0f : ((float)args[0]));
			return value.TimeEquals(time);
		}
		return TileResultCode.False;
	}

	public TileResultCode TimerUIStartTimer(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 1, 0);
		if (BlockTimerUI.allTimerBlocks.TryGetValue(intArg, out var value))
		{
			int dir = ((args.Length == 0) ? 1 : ((int)args[0]));
			value.StartTimer(dir);
		}
		return TileResultCode.True;
	}

	public TileResultCode TimerUISetTime(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 1, 0);
		if (BlockTimerUI.allTimerBlocks.TryGetValue(intArg, out var value))
		{
			float time = ((args.Length == 0) ? 0f : ((float)args[0]));
			value.SetTime(time);
		}
		return TileResultCode.True;
	}

	public TileResultCode TimerUIIncrementTime(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 1, 0);
		if (BlockTimerUI.allTimerBlocks.TryGetValue(intArg, out var value))
		{
			float time = ((args.Length == 0) ? 0f : ((float)args[0]));
			value.IncrementTime(time);
		}
		return TileResultCode.False;
	}

	public TileResultCode TimerUIPauseTimer(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 0, 0);
		if (BlockTimerUI.allTimerBlocks.TryGetValue(intArg, out var value) && value.IsRunning())
		{
			value.PauseOneFrame();
		}
		return TileResultCode.True;
	}

	public TileResultCode TimerUIPauseUI(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float floatArg = Util.GetFloatArg(args, 0, 0.5f);
		int intArg = Util.GetIntArg(args, 1, 0);
		bool flag = eInfo.timer >= floatArg;
		if (BlockTimerUI.allTimerBlocks.TryGetValue(intArg, out var value))
		{
			value.PauseUI(!flag);
		}
		if (flag)
		{
			return TileResultCode.True;
		}
		return TileResultCode.Delayed;
	}

	public TileResultCode TimerUIWaitTimer(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float floatArg = Util.GetFloatArg(args, 0, 1f);
		int intArg = Util.GetIntArg(args, 1, 0);
		bool flag = eInfo.timer >= floatArg;
		if (!BlockTimerUI.allTimerBlocks.TryGetValue(intArg, out var value))
		{
			return TileResultCode.True;
		}
		if (eInfo.timer == 0f && !value.CanBePaused())
		{
			return TileResultCode.True;
		}
		value.PauseOneFrame();
		if (flag)
		{
			return TileResultCode.True;
		}
		return TileResultCode.Delayed;
	}

	public TileResultCode ObjectCounterUIValueCondition(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 0, 1);
		int intArg2 = Util.GetIntArg(args, 1, 0);
		int intArg3 = Util.GetIntArg(args, 2, 0);
		return TreasureHandler.GetObjectCounter(intArg3)?.ValueCondition(intArg, intArg2) ?? TileResultCode.False;
	}

	public TileResultCode ObjectCounterUIValueEqualsMaxValue(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 0, 0);
		BlockObjectCounterUI objectCounter = TreasureHandler.GetObjectCounter(intArg);
		if (objectCounter != null && objectCounter.ValueEqualsMaxValue())
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode ObjectCounterUISetValueToMax(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 0, 0);
		TreasureHandler.GetObjectCounter(intArg)?.SetValueToMax(eInfo.floatArg);
		return TileResultCode.True;
	}

	public TileResultCode GaugeUIValueEqualsMaxValue(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 0, 0);
		if (BlockGaugeUI.allGaugeBlocks.TryGetValue(intArg, out var value))
		{
			if (value.ValueEqualsMaxValue())
			{
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}
		return TileResultCode.False;
	}

	public TileResultCode GaugeUISetValueToMax(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 0, 0);
		if (BlockGaugeUI.allGaugeBlocks.TryGetValue(intArg, out var value))
		{
			value.SetValueToMax(eInfo.floatArg);
		}
		return TileResultCode.True;
	}

	public TileResultCode TimerUIValueEqualsMaxValue(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 0, 0);
		if (BlockTimerUI.allTimerBlocks.TryGetValue(intArg, out var value))
		{
			if (value.ValueEqualsMaxValue())
			{
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}
		return TileResultCode.False;
	}

	public TileResultCode TimerUISetValueToMax(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 0, 0);
		if (BlockTimerUI.allTimerBlocks.TryGetValue(intArg, out var value))
		{
			value.SetValueToMax(eInfo.floatArg);
		}
		return TileResultCode.True;
	}

	public TileResultCode CounterUIValueEqualsMaxValue(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 0, 0);
		if (BlockCounterUI.allCounterBlocks.TryGetValue(intArg, out var value))
		{
			if (value.ValueEqualsMaxValue())
			{
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}
		return TileResultCode.False;
	}

	public TileResultCode CounterUISetValueToMax(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 0, 0);
		if (BlockCounterUI.allCounterBlocks.TryGetValue(intArg, out var value))
		{
			value.SetValueToMax(eInfo.floatArg);
		}
		return TileResultCode.True;
	}

	public TileResultCode CounterUIValueEqualsMinValue(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 0, 0);
		if (BlockCounterUI.allCounterBlocks.TryGetValue(intArg, out var value))
		{
			if (value.ValueEqualsMinValue())
			{
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}
		return TileResultCode.False;
	}

	public TileResultCode CounterUISetValueToMin(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 0, 0);
		if (BlockCounterUI.allCounterBlocks.TryGetValue(intArg, out var value))
		{
			value.SetValueToMin(eInfo.floatArg);
		}
		return TileResultCode.True;
	}

	public TileResultCode GaugeUIValueEquals(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 1, 0);
		if (BlockGaugeUI.allGaugeBlocks.TryGetValue(intArg, out var value))
		{
			int intArg2 = Util.GetIntArg(args, 0, 1);
			if (value.ValueEquals(intArg2))
			{
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}
		return TileResultCode.False;
	}

	public TileResultCode GaugeUISetValue(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 1, 0);
		if (BlockGaugeUI.allGaugeBlocks.TryGetValue(intArg, out var value))
		{
			int intArg2 = Util.GetIntArg(args, 0, 1);
			value.SetValue(intArg2, eInfo.floatArg);
		}
		return TileResultCode.True;
	}

	public TileResultCode GaugeUIGetFraction(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 1, 0);
		if (BlockGaugeUI.allGaugeBlocks.TryGetValue(intArg, out var value))
		{
			float num = Mathf.Max(Util.GetFloatArg(args, 0, 1f), 0.001f);
			float b = Mathf.Clamp(value.GetFraction() / num, 0f, 1f);
			eInfo.floatArg = Mathf.Min(eInfo.floatArg, b);
			if (!(eInfo.floatArg <= 0f))
			{
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}
		return TileResultCode.False;
	}

	public TileResultCode ObjectCounterUIValueEquals(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 0, 1);
		int intArg2 = Util.GetIntArg(args, 1, 0);
		BlockObjectCounterUI objectCounter = TreasureHandler.GetObjectCounter(intArg2);
		if (objectCounter != null && objectCounter.ValueEquals(intArg))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode ObjectCounterUISetValue(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 0, 1);
		int intArg2 = Util.GetIntArg(args, 1, 0);
		TreasureHandler.GetObjectCounter(intArg2)?.SetValue(intArg, eInfo.floatArg);
		return TileResultCode.True;
	}

	public TileResultCode CounterUIValueEquals(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 0, 1);
		int intArg2 = Util.GetIntArg(args, 1, 0);
		if (BlockCounterUI.allCounterBlocks.TryGetValue(intArg2, out var value))
		{
			if (value.ValueEquals(intArg))
			{
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}
		return TileResultCode.False;
	}

	public TileResultCode SetCounterUIValue(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 0, 1);
		int intArg2 = Util.GetIntArg(args, 1, 0);
		if (BlockCounterUI.allCounterBlocks.TryGetValue(intArg2, out var value))
		{
			value.SetValue(intArg);
		}
		return TileResultCode.True;
	}

	public TileResultCode InCameraFrustum(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (GeometryUtility.TestPlanesAABB(Blocksworld.frustumPlanes, go.GetComponent<Collider>().bounds))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode GaugeUIIncrementValue(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 0, 1);
		int intArg2 = Util.GetIntArg(args, 1, 0);
		IncrementGaugeUI(intArg, intArg2);
		return TileResultCode.True;
	}

	private void IncrementGaugeUI(int inc, int counterIndex)
	{
		if (BlockGaugeUI.allGaugeBlocks.TryGetValue(counterIndex, out var value))
		{
			value.IncrementValue(inc);
		}
	}

	public TileResultCode IncrementObjectCounterUI(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 0, 1);
		int intArg2 = Util.GetIntArg(args, 1, 0);
		return IncrementObjectCounterUI(intArg, intArg2);
	}

	public TileResultCode DecrementObjectCounterUI(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 0, 1);
		int intArg2 = Util.GetIntArg(args, 1, 0);
		return IncrementObjectCounterUI(-intArg, intArg2);
	}

	private TileResultCode IncrementObjectCounterUI(int inc, int counterIndex)
	{
		TreasureHandler.GetObjectCounter(counterIndex)?.IncrementValue(inc);
		return TileResultCode.True;
	}

	public TileResultCode RandomizeCounterUI(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 2, 0);
		if (BlockCounterUI.allCounterBlocks.TryGetValue(intArg, out var value))
		{
			int intArg2 = Util.GetIntArg(args, 0, 0);
			int intArg3 = Util.GetIntArg(args, 1, 10);
			value.Randomize(intArg2, intArg3);
		}
		return TileResultCode.True;
	}

	public TileResultCode IncrementCounterUIValue(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 0, 1);
		int intArg2 = Util.GetIntArg(args, 1, 0);
		IncrementCounterUI(intArg, intArg2);
		return TileResultCode.True;
	}

	private void IncrementCounterUI(int inc, int counterIndex)
	{
		if (BlockCounterUI.allCounterBlocks.TryGetValue(counterIndex, out var value))
		{
			value.BlockIncrementValue(this, inc);
		}
	}

	public TileResultCode CounterUIAnimateScore(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 1, 0);
		if (BlockCounterUI.allCounterBlocks.TryGetValue(intArg, out var value))
		{
			int intArg2 = Util.GetIntArg(args, 0, 0);
			value.SetScoreAnimatedBlock(this, intArg2);
		}
		return TileResultCode.True;
	}

	public TileResultCode CounterUIScoreMultiplier(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 2, 0);
		if (BlockCounterUI.allCounterBlocks.TryGetValue(intArg, out var value))
		{
			int intArg2 = Util.GetIntArg(args, 0, 0);
			bool intBooleanArg = Util.GetIntBooleanArg(args, 1, defaultValue: true);
			value.SetScoreMultiplierBlock(this, intArg2, intBooleanArg);
		}
		return TileResultCode.True;
	}

	public TileResultCode CounterUIGlobalScoreMultiplier(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 2, 0);
		if (BlockCounterUI.allCounterBlocks.TryGetValue(intArg, out var value))
		{
			int intArg2 = Util.GetIntArg(args, 0, 0);
			bool intBooleanArg = Util.GetIntBooleanArg(args, 1, defaultValue: true);
			value.SetGlobalScoreMultiplier(intArg2, intBooleanArg);
		}
		return TileResultCode.True;
	}

	public virtual TileResultCode OnImpact(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (CollisionManager.IsImpactingBlock(this))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode OnImpactModel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		UpdateConnectedCache();
		if (CollisionManager.IsImpactingAny(connectedCache[this], this))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public virtual TileResultCode OnParticleImpact(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int particleType = (int)args[0];
		if (CollisionManager.IsParticleImpactingBlock(this, particleType))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode OnParticleImpactModel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int particleType = (int)args[0];
		UpdateConnectedCache();
		if (CollisionManager.IsParticleCollidingAny(connectedCache[this], this, particleType))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public virtual TileResultCode IsBumping(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string target = (string)args[0];
		if (CollisionManager.IsBumpingBlock(this, target))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsBumpingAnyBlockInModel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string target = (string)args[0];
		UpdateConnectedCache();
		if (CollisionManager.IsBumpingAny(connectedCache[this], target, this))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsBumpingAnyBlockInChunk(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string target = (string)args[0];
		if (CollisionManager.IsBumpingAny(chunk.blocks, target, this))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public virtual TileResultCode IsBumpingTag(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string tag = (string)args[0];
		if (CollisionManager.BlockIsBumpingTag(this, tag))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

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

	public TileResultCode IsBumpingAnyTaggedBlockInModel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		UpdateConnectedCache();
		string tag = (string)args[0];
		if (IsBumpingTagAny(connectedCache[this], tag))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsBumpingAnyTaggedBlockInChunk(ScriptRowExecutionInfo eInfo, object[] args)
	{
		UpdateConnectedCache();
		string tag = (string)args[0];
		if (IsBumpingTagAny(chunk.blocks, tag))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsHitByTaggedLaserModel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (!BlockAbstractLaser.anyLaserBeamOrPulseAvailable)
		{
			return TileResultCode.False;
		}
		string tagName = (string)args[0];
		if (BlockAbstractLaser.IsHitByTaggedLaserModel(modelBlock, tagName))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsHitByTaggedProjectileModel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (!BlockAbstractLaser.anyProjectileAvailable)
		{
			return TileResultCode.False;
		}
		string tagName = (string)args[0];
		if (BlockAbstractLaser.IsHitByTaggedProjectileModel(modelBlock, tagName))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsHitByTaggedLaserChunk(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string tagName = (string)args[0];
		List<Block> blocks = chunk.blocks;
		if (BlockAbstractLaser.IsHitByTaggedLaser(blocks, tagName))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsHitByTaggedProjectileChunk(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string tagName = (string)args[0];
		List<Block> blocks = chunk.blocks;
		if (BlockAbstractLaser.IsHitByTaggedProjectile(blocks, tagName))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsHitByLaserModel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (!BlockAbstractLaser.anyLaserBeamOrPulseAvailable)
		{
			return TileResultCode.False;
		}
		if (BlockAbstractLaser.IsHitByLaserModel(modelBlock))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsHitByLaserChunk(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (!BlockAbstractLaser.anyLaserBeamOrPulseAvailable)
		{
			return TileResultCode.False;
		}
		if (BlockAbstractLaser.IsHitByLaser(chunk.blocks))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsHitByProjectileModel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (!BlockAbstractLaser.anyProjectileAvailable)
		{
			return TileResultCode.False;
		}
		if (BlockAbstractLaser.IsHitByProjectileModel(modelBlock))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsHitByProjectileChunk(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (!BlockAbstractLaser.anyProjectileAvailable)
		{
			return TileResultCode.False;
		}
		if (BlockAbstractLaser.IsHitByProjectile(chunk.blocks))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public virtual TileResultCode IsHitByProjectile(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (BlockAbstractLaser.IsHitByProjectile(this))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public virtual TileResultCode IsHitByPulseOrBeam(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (BlockAbstractLaser.IsHitByPulseOrBeam(this))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public virtual TileResultCode IsHitByTaggedProjectile(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (BlockAbstractLaser.IsHitByTaggedProjectile(this, Util.GetStringArg(args, 0, string.Empty)))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public virtual TileResultCode IsHitByTaggedPulseOrBeam(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (BlockAbstractLaser.IsHitByTaggedPulseOrBeam(this, Util.GetStringArg(args, 0, string.Empty)))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public virtual TileResultCode IsHitByArrow(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (BlockAbstractBow.IsHitByArrow(this))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public virtual TileResultCode IsHitByTaggedArrow(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string tag = (string)args[0];
		if (BlockAbstractBow.IsHitByTaggedArrow(this, tag))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsHitByArrowModel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (BlockAbstractBow.IsHitByArrowModel(modelBlock))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsHitByArrowChunk(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (BlockAbstractBow.IsHitByArrow(chunk.blocks))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsHitByTaggedArrowModel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string tag = (string)args[0];
		if (BlockAbstractBow.IsHitByTaggedArrowModel(modelBlock, tag))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsHitByTaggedArrowChunk(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string tag = (string)args[0];
		if (BlockAbstractBow.IsHitByTaggedArrow(chunk.blocks, tag))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsPulledByMagnet(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (BlockAbstractMagnet.PulledByMagnet(this))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsPulledByMagnetModel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (BlockAbstractMagnet.PulledByMagnetModel(modelBlock))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsPushedByMagnet(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (BlockAbstractMagnet.PushedByMagnet(this))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsPushedByMagnetModel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (BlockAbstractMagnet.PushedByMagnetModel(modelBlock))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

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

	public TileResultCode IsTriggering(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (CollisionManager.IsTriggeringBlock(this))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsAnyBlockInModelTriggering(ScriptRowExecutionInfo eInfo, object[] args)
	{
		UpdateConnectedCache();
		List<Block> blocks = connectedCache[this];
		if (IsTriggeringAny(blocks))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsAnyBlockInChunkTriggering(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (IsTriggeringAny(chunk.blocks))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public virtual TileResultCode SetTrigger(ScriptRowExecutionInfo eInfo, object[] args)
	{
		Blocksworld.AddFixedUpdateUniqueCommand(updateBlockTriggerCommand, resetWhenAdded: false);
		updateBlockTriggerCommand.BlockIsTrigger(this);
		return TileResultCode.True;
	}

	public TileResultCode SetEveryBlockInModelAsTrigger(ScriptRowExecutionInfo eInfo, object[] args)
	{
		UpdateConnectedCache();
		SetAllTrigger(connectedCache[this]);
		return TileResultCode.True;
	}

	public TileResultCode SetEveryBlockInChunkAsTrigger(ScriptRowExecutionInfo eInfo, object[] args)
	{
		SetAllTrigger(chunk.blocks);
		return TileResultCode.True;
	}

	private void SetAllTrigger(List<Block> blocks)
	{
		Blocksworld.AddFixedUpdateUniqueCommand(updateBlockTriggerCommand, resetWhenAdded: false);
		for (int i = 0; i < blocks.Count; i++)
		{
			Block b = blocks[i];
			updateBlockTriggerCommand.BlockIsTrigger(b);
		}
	}

	public TileResultCode CameraFollow(ScriptRowExecutionInfo eInfo, object[] args)
	{
		Blocksworld.blocksworldCamera.CameraFollow(this, args);
		return TileResultCode.True;
	}

	public TileResultCode CameraFollow2D(ScriptRowExecutionInfo eInfo, object[] args)
	{
		Blocksworld.blocksworldCamera.CameraFollow2D(this, args);
		return TileResultCode.True;
	}

	public TileResultCode CameraFollowLookToward(ScriptRowExecutionInfo eInfo, object[] args)
	{
		Blocksworld.blocksworldCamera.CameraFollowLookToward(this, args);
		return TileResultCode.True;
	}

	public TileResultCode CameraFollowLookTowardTag(ScriptRowExecutionInfo eInfo, object[] args)
	{
		Blocksworld.blocksworldCamera.CameraFollowLookTowardTag(this, args);
		return TileResultCode.True;
	}

	public TileResultCode CameraFollowThirdPersonPlatform(ScriptRowExecutionInfo eInfo, object[] args)
	{
		Blocksworld.blocksworldCamera.CameraFollowThirdPersonPlatform(this, args);
		return TileResultCode.True;
	}

	public TileResultCode CameraMoveTo(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float alpha = 0.95f;
		if (args.Length != 0)
		{
			alpha = (float)args[0];
		}
		Blocksworld.blocksworldCamera.CameraMoveTo(this, alpha);
		return TileResultCode.True;
	}

	public TileResultCode CameraToNamedPose(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		float floatArg = Util.GetFloatArg(args, 1, 0.985f);
		float floatArg2 = Util.GetFloatArg(args, 2, 0.985f);
		float floatArg3 = Util.GetFloatArg(args, 3, 15f);
		bool intBooleanArg = Util.GetIntBooleanArg(args, 4, defaultValue: false);
		Blocksworld.blocksworldCamera.CameraToNamedPose(stringArg, floatArg, floatArg2, floatArg3, intBooleanArg);
		return TileResultCode.True;
	}

	public TileResultCode SetTargetCameraAngle(ScriptRowExecutionInfo eInfo, object[] args)
	{
		Blocksworld.blocksworldCamera.SetTargetCameraAngle(Util.GetFloatArg(args, 0, 70f));
		return TileResultCode.True;
	}

	public TileResultCode SetVerticalDistanceOffsetFactor(ScriptRowExecutionInfo eInfo, object[] args)
	{
		Blocksworld.blocksworldCamera.SetVerticalDistanceOffsetFactor(Util.GetFloatArg(args, 0, 1f));
		return TileResultCode.True;
	}

	public TileResultCode SetTargetFollowDistanceMultiplier(ScriptRowExecutionInfo eInfo, object[] args)
	{
		Blocksworld.blocksworldCamera.SetTargetFollowDistanceMultiplier(Util.GetFloatArg(args, 0, 1f));
		return TileResultCode.True;
	}

	public TileResultCode CameraSpeedFoV(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float num = eInfo.floatArg * Util.GetFloatArg(args, 0, 50f);
		Blocksworld.blocksworldCamera.desiredSpeedFoV = num / 100f;
		return TileResultCode.True;
	}

	public TileResultCode CameraLookAt(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float alpha = 0.95f;
		if (args.Length != 0)
		{
			alpha = (float)args[0];
		}
		Blocksworld.blocksworldCamera.CameraLookAt(this, alpha);
		return TileResultCode.True;
	}

	public TileResultCode IsGameStart(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (Blocksworld.gameStart)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode Wait(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (eInfo.timer >= 0.25f)
		{
			return TileResultCode.True;
		}
		return TileResultCode.Delayed;
	}

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

	public TileResultCode RandomWaitTimeSensor(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float max = (float)args[0];
		ScriptRowData scriptRowData = ScriptRowData.GetScriptRowData(eInfo);
		float fixedTime = Time.fixedTime;
		if (!scriptRowData.TryGetFloat("RandomWaitTimeSensor", out var value))
		{
			value = fixedTime + UnityEngine.Random.Range(0f, max);
			scriptRowData.SetFloat("RandomWaitTimeSensor", value);
		}
		if (fixedTime >= value)
		{
			scriptRowData.RemoveFloat("RandomWaitTimeSensor");
			return TileResultCode.True;
		}
		return TileResultCode.Delayed;
	}

	public TileResultCode WaitTime(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float num = (float)args[0];
		if (eInfo.timer >= num)
		{
			return TileResultCode.True;
		}
		return TileResultCode.Delayed;
	}

	public TileResultCode StopScriptsModel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		UpdateConnectedCache();
		List<Block> list = connectedCache[this];
		for (int i = 0; i < list.Count; i++)
		{
			Block b = list[i];
			stopScriptsCommand.StopBlockScripts(b);
		}
		Blocksworld.AddFixedUpdateUniqueCommand(stopScriptsCommand);
		return TileResultCode.Delayed;
	}

	public TileResultCode StopScriptsModelForTime(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float floatArg = Util.GetFloatArg(args, 0, 1f);
		UpdateConnectedCache();
		List<Block> list = connectedCache[this];
		for (int i = 0; i < list.Count; i++)
		{
			Block b = list[i];
			stopScriptsCommand.StopBlockScripts(b);
		}
		stopScriptsCommand.StartBlockScripts(list, floatArg);
		Blocksworld.AddFixedUpdateUniqueCommand(stopScriptsCommand);
		return TileResultCode.True;
	}

	public virtual TileResultCode StopScriptsBlock(ScriptRowExecutionInfo eInfo, object[] args)
	{
		stopScriptsCommand.StopBlockScripts(this);
		Blocksworld.AddFixedUpdateUniqueCommand(stopScriptsCommand);
		return TileResultCode.Delayed;
	}

	public TileResultCode LockInput(ScriptRowExecutionInfo eInfo, object[] args)
	{
		Blocksworld.lockInput = true;
		unlockInputCommand.SetUnlockTime(Time.fixedTime + 0.1f);
		Blocksworld.AddFixedUpdateUniqueCommand(unlockInputCommand);
		return TileResultCode.True;
	}

	public TileResultCode SetBuoyancy(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float floatArg = Util.GetFloatArg(args, 0, 1f);
		buoyancyMultiplier = ((buoyancyMultiplier == floatArg) ? buoyancyMultiplier : floatArg);
		return TileResultCode.True;
	}

	public TileResultCode SetMass(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float floatArg = Util.GetFloatArg(args, 0, 1f);
		if (storedMassOverride != floatArg)
		{
			blockMassOverride = floatArg;
			storedMassOverride = floatArg;
			if (chunk != null)
			{
				chunk.UpdateCenterOfMass(immediate: false);
			}
		}
		overridingMass = true;
		if (chunk != null && !massAlteredBlocks.Contains(this))
		{
			massAlteredBlocks.Add(this);
		}
		return TileResultCode.True;
	}

	public TileResultCode SetFriction(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float floatArg = Util.GetFloatArg(args, 0, 0.5f);
		Collider component = go.GetComponent<Collider>();
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

	public TileResultCode SetBounce(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float floatArg = Util.GetFloatArg(args, 0, 0.5f);
		Collider component = go.GetComponent<Collider>();
		PhysicMaterial material = component.material;
		material.bounciness = floatArg;
		material.bounceCombine = PhysicMaterialCombine.Maximum;
		component.material = material;
		return TileResultCode.True;
	}

	public static void UpdateOverridenMasses()
	{
		foreach (Block massAlteredBlock in massAlteredBlocks)
		{
			if (massAlteredBlock.overridingMass)
			{
				massAlteredBlock.overridingMass = false;
			}
			else if (massAlteredBlock.storedMassOverride >= 0f && massAlteredBlock.storedMassOverride != massAlteredBlock.blockMassOverride)
			{
				massAlteredBlock.blockMassOverride = -1f;
				massAlteredBlock.chunk.UpdateCenterOfMass();
			}
		}
	}

	public virtual Color GetLightTint()
	{
		return Color.white;
	}

	public virtual Color GetEmissiveLightTint()
	{
		return transparent;
	}

	private void AddMissingScaleTo()
	{
		List<Tile> list = tiles[0];
		if (6 >= list.Count)
		{
			list.Add(new Tile(new GAF("Block.ScaleTo", new Vector3(1f, 1f, 1f))));
		}
	}

	public void SetSubmeshInitPaintToTile(int meshIndex, string paint, bool ignoreIfExists = false)
	{
		if (paint == null)
		{
			BWLog.Warning("SetSubmeshInitPaintToTile() trying to set paint to null");
		}
		else
		{
			if (meshIndex <= 0)
			{
				return;
			}
			AddMissingScaleTo();
			List<Tile> list = tiles[0];
			int num = 0;
			foreach (Tile item in list)
			{
				string name = item.gaf.Predicate.Name;
				if (name == "Block.PaintTo" && item.gaf.Args.Length > 1 && (int)item.gaf.Args[1] == meshIndex)
				{
					if (ignoreIfExists)
					{
						return;
					}
					break;
				}
				num++;
			}
			if (num < list.Count)
			{
				Tile tile = list[num];
				tile.gaf.Args[0] = paint;
			}
			else
			{
				list.Add(new Tile(new GAF("Block.PaintTo", paint, meshIndex)));
			}
		}
	}

	public void SetSimpleInitTile(GAF gaf)
	{
		List<Tile> list = tiles[0];
		int num = 0;
		foreach (Tile item in list)
		{
			string name = item.gaf.Predicate.Name;
			if (name == gaf.Predicate.Name)
			{
				break;
			}
			num++;
		}
		if (num < list.Count)
		{
			Tile tile = list[num];
			tile.gaf = gaf;
		}
		else
		{
			list.Add(new Tile(gaf));
		}
	}

	public GAF GetSimpleInitGAF(string predName)
	{
		List<Tile> list = tiles[0];
		foreach (Tile item in list)
		{
			if (predName == item.gaf.Predicate.Name)
			{
				return item.gaf;
			}
		}
		return null;
	}

	public Vector3 GetTextureToNormal(int meshIndex)
	{
		List<Tile> list = tiles[0];
		if (meshIndex == 0)
		{
			return (Vector3)list[5].gaf.Args[1];
		}
		foreach (Tile item in list)
		{
			string name = item.gaf.Predicate.Name;
			if (name == "Block.TextureTo" && item.gaf.Args.Length > 2 && (int)item.gaf.Args[2] == meshIndex)
			{
				return (Vector3)item.gaf.Args[1];
			}
		}
		return Vector3.zero;
	}

	public void SetSubmeshInitTextureToTile(int meshIndex, string texture, Vector3 normal, bool ignoreIfExists = false)
	{
		if (meshIndex <= 0)
		{
			return;
		}
		AddMissingScaleTo();
		List<Tile> list = tiles[0];
		int num = 0;
		foreach (Tile item in list)
		{
			string name = item.gaf.Predicate.Name;
			if (name == "Block.TextureTo" && item.gaf.Args.Length > 2 && (int)item.gaf.Args[2] == meshIndex)
			{
				if (ignoreIfExists)
				{
					return;
				}
				break;
			}
			num++;
		}
		if (num < list.Count)
		{
			Tile tile = list[num];
			tile.gaf.Args[0] = texture;
			tile.gaf.Args[1] = normal;
		}
		else
		{
			list.Add(new Tile(new GAF("Block.TextureTo", texture, normal, meshIndex)));
		}
	}

	protected virtual void FindSubMeshes()
	{
		if (subMeshes != null)
		{
			return;
		}
		subMeshes = new List<CollisionMesh>();
		subMeshGameObjects = new List<GameObject>();
		subMeshPaints = new List<string>();
		subMeshTextures = new List<string>();
		subMeshTextureNormals = new List<Vector3>();
		List<GameObject> list = new List<GameObject>();
		foreach (object item2 in goT)
		{
			Transform transform = (Transform)item2;
			GameObject gameObject = transform.gameObject;
			MeshFilter component = gameObject.GetComponent<MeshFilter>();
			SkinnedMeshRenderer component2 = gameObject.GetComponent<SkinnedMeshRenderer>();
			if (component != null || component2 != null)
			{
				list.Add(gameObject);
			}
		}
		if (canBeTextured == null)
		{
			canBeTextured = new bool[list.Count + 1];
		}
		if (canBeMaterialTextured == null)
		{
			canBeMaterialTextured = new bool[list.Count + 1];
		}
		list.Sort(new GameObjectNameComparer());
		int i;
		for (i = 0; i < list.Count; i++)
		{
			GameObject item = list[i];
			subMeshes.Add(null);
			subMeshGameObjects.Add(item);
			subMeshPaints.Add("Yellow");
			subMeshTextures.Add("Plain");
			subMeshTextureNormals.Add(Vector3.up);
			if (i < canBeTextured.Length)
			{
				canBeTextured[i] = true;
			}
			if (i < canBeMaterialTextured.Length)
			{
				canBeMaterialTextured[i] = true;
			}
		}
		canBeTextured[i] = true;
		canBeMaterialTextured[i] = true;
	}

	public virtual CollisionMesh GetSubMesh(int meshIndex)
	{
		FindSubMeshes();
		int num = meshIndex - 1;
		if (num < subMeshGameObjects.Count)
		{
			subMeshes[num] = CollisionVolumes.GenerateCollisionMesh(subMeshGameObjects[num]);
			return subMeshes[num];
		}
		return null;
	}

	public virtual GameObject GetSubMeshGameObject(int meshIndex)
	{
		FindSubMeshes();
		int num = meshIndex - 1;
		if (num < subMeshGameObjects.Count)
		{
			return subMeshGameObjects[num];
		}
		return null;
	}

	public virtual string GetSubMeshPaint(int meshIndex)
	{
		FindSubMeshes();
		int num = meshIndex - 1;
		if (num < subMeshPaints.Count)
		{
			return subMeshPaints[num];
		}
		return "Black";
	}

	public virtual string GetSubMeshTexture(int meshIndex)
	{
		FindSubMeshes();
		int num = meshIndex - 1;
		if (num < subMeshTextures.Count)
		{
			return subMeshTextures[num];
		}
		return "Plain";
	}

	public virtual Vector3 GetSubMeshTextureNormal(int meshIndex)
	{
		FindSubMeshes();
		int num = meshIndex - 1;
		if (num < subMeshTextureNormals.Count)
		{
			return subMeshTextureNormals[num];
		}
		return Vector3.up;
	}

	public int GetSubMeshIndex(GameObject obj)
	{
		int result = -1;
		if (obj == go)
		{
			result = 0;
		}
		else
		{
			for (int i = 0; i < subMeshGameObjects.Count; i++)
			{
				if (obj == subMeshGameObjects[i])
				{
					result = i + 1;
					break;
				}
			}
		}
		return result;
	}

	public virtual bool ContainsPaintableSubmeshes()
	{
		FindSubMeshes();
		return subMeshes.Count > 0;
	}

	public virtual int GetMeshIndexForRay(Ray ray, bool refresh, out Vector3 point, out Vector3 normal)
	{
		FindSubMeshes();
		point = Vector3.zero;
		normal = Vector3.up;
		if (subMeshGameObjects.Count == 0)
		{
			return 0;
		}
		float num = 1E+10f;
		if (refresh || mainMesh == null)
		{
			mainMesh = CollisionVolumes.GenerateCollisionMesh(go);
		}
		if (mainMesh != null)
		{
			Vector3 closestPoint = default(Vector3);
			if (CollisionTest.RayMeshTestClosest(ray.origin, ray.direction, mainMesh, out closestPoint, out normal))
			{
				num = (closestPoint - ray.origin).magnitude;
			}
		}
		int result = 0;
		int num2 = 0;
		foreach (GameObject subMeshGameObject in subMeshGameObjects)
		{
			if (subMeshGameObject.activeInHierarchy && subMeshGameObject.GetComponent<Renderer>().enabled)
			{
				if (refresh || subMeshes[num2] == null)
				{
					subMeshes[num2] = CollisionVolumes.GenerateCollisionMesh(subMeshGameObject);
				}
				Vector3 closestPoint2 = default(Vector3);
				Vector3 normal2 = default(Vector3);
				if (CollisionTest.RayMeshTestClosest(ray.origin, ray.direction, subMeshes[num2], out closestPoint2, out normal2))
				{
					float magnitude = (closestPoint2 - ray.origin).magnitude;
					if (magnitude < num)
					{
						num = magnitude;
						result = num2 + 1;
						point = closestPoint2;
						normal = normal2;
					}
				}
			}
			num2++;
		}
		return result;
	}

	public virtual TileResultCode PaintToSubMesh(string paint, bool permanent, int meshIndex = 0)
	{
		if (meshIndex > 0)
		{
			FindSubMeshes();
			int num = meshIndex - 1;
			if (num < subMeshes.Count)
			{
				GameObject gameObject = subMeshGameObjects[num];
				if (gameObject != null && childMeshes != null && childMeshes.ContainsKey(gameObject.name))
				{
					Materials.SetMaterial(gameObject, childMeshes[gameObject.name], gameObject.name, paint, GetTexture(meshIndex), GetSubMeshTextureNormal(meshIndex), Vector3.one, string.Empty);
					if (permanent)
					{
						SetSubmeshInitPaintToTile(meshIndex, paint);
					}
					subMeshPaints[num] = paint;
				}
			}
		}
		return TileResultCode.True;
	}

	public Bounds GetBounds()
	{
		return Util.ComputeBounds(new List<Block>(1) { this });
	}

	public Vector3[] GetScaleConstraints()
	{
		string key = BlockType();
		if (!blockScaleConstraints.ContainsKey(key))
		{
			BlockMetaData blockMetaData = GetBlockMetaData();
			Vector3[] array;
			if (blockMetaData != null)
			{
				array = blockMetaData.scaleConstraints;
				if (array.Length == 0)
				{
					array = new Vector3[3]
					{
						Vector3.right,
						Vector3.up,
						Vector3.forward
					};
				}
			}
			else
			{
				array = new Vector3[3]
				{
					Vector3.right,
					Vector3.up,
					Vector3.forward
				};
			}
			blockScaleConstraints.Add(key, array);
		}
		return blockScaleConstraints[key];
	}

	public override string ToString()
	{
		if (go == null)
		{
			return "Destroyed block";
		}
		return go.name;
	}

	public void UpdateWithinWaterLPFilter(GameObject theGo = null)
	{
		if (!Sound.sfxEnabled)
		{
			return;
		}
		AudioLowPassFilter audioLowPassFilter = lpFilter;
		if (theGo == null)
		{
			theGo = go;
		}
		else
		{
			audioLowPassFilter = theGo.GetComponent<AudioLowPassFilter>();
		}
		CreatePositionedAudioSourceIfNecessary("Create", theGo);
		if (BlockAbstractWater.CameraWithinAnyWater() || BlockWater.BlockWithinWater(this))
		{
			if (audioLowPassFilter == null)
			{
				audioLowPassFilter = theGo.GetComponent<AudioLowPassFilter>();
				if (audioLowPassFilter == null)
				{
					audioLowPassFilter = theGo.AddComponent<AudioLowPassFilter>();
				}
				if (theGo == go)
				{
					lpFilter = audioLowPassFilter;
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

	protected virtual void PlayLoopSound(bool play, AudioClip clip = null, float volume = 1f, AudioSource source = null, float pitch = 1f)
	{
		if (go == null)
		{
			return;
		}
		if (Sound.BlockIsMuted(this) || vanished)
		{
			play = false;
		}
		if (source == null)
		{
			if (loopingAudioSource == null)
			{
				loopingAudioSource = GetOrCreateLoopingPositionedAudioSource();
				Sound.SetWorldAudioSourceParams(loopingAudioSource);
			}
			source = loopingAudioSource;
		}
		if (Blocksworld.CurrentState != State.Play || !Sound.sfxEnabled)
		{
			if (source.isPlaying)
			{
				source.Stop();
			}
		}
		else if (source != null)
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
			BWLog.Info("Could not find audio source in block " + BlockType());
		}
	}

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

	public float GetMomentOfInertia(Vector3 pos, Vector3 axis, bool includeMass = true)
	{
		Vector3 vector = goT.rotation * GetScale();
		Vector3 position = goT.position;
		float num = ((!includeMass) ? 1f : GetMass());
		Vector3 vector2 = new Vector3(Mathf.Abs(axis.x), Mathf.Abs(axis.y), Mathf.Abs(axis.z));
		Vector3 lhs = Vector3.one - vector2;
		Vector3 rhs = new Vector3(vector.x * vector.x, vector.y * vector.y, vector.z * vector.z);
		float num2 = num / 12f * Vector3.Dot(lhs, rhs);
		float magnitude = Vector3.Cross(vector2, position - pos).magnitude;
		return num2 + num * magnitude * magnitude;
	}

	private void AddAnimationSupport(string blockType, int objectIndex, string animationType)
	{
		if (animationSupports == null)
		{
			animationSupports = new Dictionary<string, List<HashSet<string>>>();
		}
		List<HashSet<string>> list;
		if (animationSupports.ContainsKey(blockType))
		{
			list = animationSupports[blockType];
		}
		else
		{
			list = new List<HashSet<string>>();
			animationSupports[blockType] = list;
		}
		objectIndex = Mathf.Clamp(objectIndex, 0, 1000);
		while (objectIndex >= list.Count)
		{
			list.Add(null);
		}
		HashSet<string> hashSet = list[objectIndex];
		if (hashSet == null)
		{
			hashSet = (list[objectIndex] = new HashSet<string>());
		}
		hashSet.Add(animationType);
	}

	private bool SupportsAnimation(GAF gaf)
	{
		if (animationSupports == null)
		{
			animationSupports = new Dictionary<string, List<HashSet<string>>>();
		}
		string key = BlockType();
		if (animationSupports.ContainsKey(key))
		{
			List<HashSet<string>> list = animationSupports[key];
			int num = (int)gaf.Args[0];
			if (num < list.Count)
			{
				string item = (string)gaf.Args[1];
				return list[num]?.Contains(item) ?? false;
			}
		}
		return false;
	}

	public bool SupportsGaf(GAF gaf)
	{
		if (gaf.Predicate == predicateAnimate)
		{
			return SupportsAnimation(gaf);
		}
		return true;
	}

	private ConfigurableJoint FindJointBetween(Rigidbody rb, Rigidbody rb2)
	{
		ConfigurableJoint[] components = rb.gameObject.GetComponents<ConfigurableJoint>();
		foreach (ConfigurableJoint configurableJoint in components)
		{
			if (configurableJoint.connectedBody == rb2)
			{
				return configurableJoint;
			}
		}
		return null;
	}

	protected void DestroyFakeRigidbodies()
	{
		if (fakeRigidbodyGos == null)
		{
			return;
		}
		foreach (GameObject fakeRigidbodyGo in fakeRigidbodyGos)
		{
			UnityEngine.Object.Destroy(fakeRigidbodyGo);
		}
		fakeRigidbodyGos.Clear();
		fakeRigidbodyGos = null;
	}

	private Rigidbody CreateFakeRigidbody(Block b1)
	{
		Rigidbody rb = chunk.rb;
		Rigidbody rb2 = b1.chunk.rb;
		GameObject gameObject = new GameObject(go.name + " Fake Ridigbody");
		gameObject.transform.position = (b1.goT.position + goT.position) * 0.5f;
		Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
		if (Blocksworld.interpolateRigidBodies)
		{
			rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
		}
		rigidbody.angularDrag = 0f;
		rigidbody.drag = 0f;
		rigidbody.mass = Mathf.Max(0.5f, 0.1f * (rb2.mass + rb.mass));
		if (fakeRigidbodyGos == null)
		{
			fakeRigidbodyGos = new List<GameObject>();
		}
		fakeRigidbodyGos.Add(gameObject);
		return rigidbody;
	}

	public void CreateFakeRigidbodyBetweenJoints()
	{
		List<Block> list = ConnectionsOfType(2, directed: true);
		foreach (Block item in list)
		{
			List<Block> list2 = item.ConnectionsOfType(2, directed: true);
			foreach (Block item2 in list2)
			{
				if (item2 != this)
				{
					continue;
				}
				Rigidbody rb = chunk.rb;
				Rigidbody rb2 = item.chunk.rb;
				if (!(rb != null) || !(rb2 != null))
				{
					continue;
				}
				if (item is BlockAbstractWheel)
				{
					BlockAbstractWheel blockAbstractWheel = (BlockAbstractWheel)item;
					ConfigurableJoint configurableJoint = null;
					GameObject gameObject = goT.parent.gameObject;
					foreach (ConfigurableJoint turnJoint in blockAbstractWheel.turnJoints)
					{
						if (turnJoint.gameObject == gameObject)
						{
							configurableJoint = turnJoint;
							break;
						}
					}
					if (!(configurableJoint != null))
					{
						continue;
					}
					Rigidbody rigidbody = CreateFakeRigidbody(item);
					blockAbstractWheel.DestroyJoint(configurableJoint);
					configurableJoint = blockAbstractWheel.CreateJoints(rigidbody.gameObject);
					ConfigurableJoint configurableJoint2 = FindJointBetween(rb, rb2);
					if (this is BlockAbstractTorsionSpring)
					{
						BlockAbstractTorsionSpring blockAbstractTorsionSpring = (BlockAbstractTorsionSpring)this;
						configurableJoint2 = blockAbstractTorsionSpring.joint;
					}
					if (configurableJoint2 != null)
					{
						configurableJoint2.connectedBody = rigidbody;
						if (configurableJoint2 == configurableJoint)
						{
							BWLog.Info("same joints!!!");
						}
					}
					continue;
				}
				ConfigurableJoint configurableJoint3 = FindJointBetween(rb, rb2);
				ConfigurableJoint configurableJoint4 = FindJointBetween(rb2, rb);
				if (configurableJoint3 != null && configurableJoint4 != null)
				{
					Rigidbody jointToJointConnection = (configurableJoint4.connectedBody = (configurableJoint3.connectedBody = CreateFakeRigidbody(item)));
					if (item is BlockAbstractTorsionSpring)
					{
						BlockAbstractTorsionSpring blockAbstractTorsionSpring2 = (BlockAbstractTorsionSpring)item;
						blockAbstractTorsionSpring2.jointToJointConnection = jointToJointConnection;
					}
					if (item2 is BlockAbstractTorsionSpring)
					{
						BlockAbstractTorsionSpring blockAbstractTorsionSpring3 = (BlockAbstractTorsionSpring)item2;
						blockAbstractTorsionSpring3.jointToJointConnection = jointToJointConnection;
					}
				}
			}
		}
	}

	public int GetInstanceId()
	{
		if (go != null)
		{
			return go.GetInstanceID();
		}
		return -1;
	}

	public bool ContainsTileWithAnyPredicate(HashSet<Predicate> preds)
	{
		for (int i = 0; i < tiles.Count; i++)
		{
			List<Tile> list = tiles[i];
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

	public bool ContainsTileWithAnyPredicateInPlayMode(HashSet<Predicate> fewPreds)
	{
		return fewPreds.Overlaps(BWSceneManager.PlayBlockPredicates(this));
	}

	public bool ContainsTileWithAnyPredicateInPlayMode2(HashSet<Predicate> manyPreds)
	{
		if (!BWSceneManager.ContainsPlayBlockPredicate(this))
		{
			BWLog.Error("not in play block predicates " + ToString());
			return false;
		}
		return BWSceneManager.PlayBlockPredicates(this).Overlaps(manyPreds);
	}

	public bool ContainsTileWithPredicateInPlayMode(Predicate pred)
	{
		return BWSceneManager.PlayBlockPredicates(this).Contains(pred);
	}

	public bool ContainsTileWithPredicate(Predicate pred)
	{
		for (int i = 0; i < tiles.Count; i++)
		{
			List<Tile> list = tiles[i];
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

	public bool ContainsTagBump()
	{
		if (taggedBumpPreds == null)
		{
			taggedBumpPreds = new HashSet<Predicate>();
			taggedBumpPreds.Add(predicateTaggedBump);
			taggedBumpPreds.Add(predicateTaggedBumpModel);
			taggedBumpPreds.Add(predicateTaggedBumpChunk);
		}
		return ContainsTileWithAnyPredicateInPlayMode(taggedBumpPreds);
	}

	protected AudioClip GetLoopClip()
	{
		if (loopClip == null)
		{
			loopClip = Sound.GetSfx(loopName);
		}
		return loopClip;
	}

	public virtual bool HasDynamicalLight()
	{
		return false;
	}

	public virtual Color GetDynamicalLightTint()
	{
		return Color.white;
	}

	public float CalculateMassDistribution(Chunk chunk, Vector3 dir, Chunk ignoreChunk = null)
	{
		Vector3 position = goT.position;
		float mi = 0f;
		HashSet<Chunk> hashSet = new HashSet<Chunk>();
		if (ignoreChunk != null)
		{
			hashSet.Add(ignoreChunk);
		}
		AddMassDistributionInfo(chunk, position, dir, hashSet, ref mi);
		return mi;
	}

	private void AddMassDistributionInfo(Chunk chunk, Vector3 pos, Vector3 dir, HashSet<Chunk> visited, ref float mi)
	{
		visited.Add(chunk);
		HashSet<Chunk> hashSet = new HashSet<Chunk>();
		for (int i = 0; i < chunk.blocks.Count; i++)
		{
			Block block = chunk.blocks[i];
			if (block == null || !(block.go != null))
			{
				continue;
			}
			float momentOfInertia = block.GetMomentOfInertia(pos, dir);
			mi += momentOfInertia;
			foreach (Block item2 in block.ConnectionsOfType(2, directed: true))
			{
				Chunk item = item2.chunk;
				if (!visited.Contains(item))
				{
					AddMassDistributionInfo(item, pos, dir, visited, ref mi);
				}
			}
		}
	}

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
		ConnectedToKinematicChunkRecursive(chunk, hashSet, ref result);
		return result;
	}

	private void ConnectedToKinematicChunkRecursive(Chunk chunk, HashSet<Chunk> visited, ref bool result)
	{
		visited.Add(chunk);
		for (int i = 0; i < chunk.blocks.Count; i++)
		{
			Block block = chunk.blocks[i];
			if (block == null || !(block.go != null))
			{
				continue;
			}
			foreach (Block item in block.ConnectionsOfType(2, directed: true))
			{
				Chunk chunk2 = item.chunk;
				if (!(chunk2.rb == null) && !visited.Contains(chunk2))
				{
					if (chunk2.rb.isKinematic)
					{
						result = true;
						return;
					}
					ConnectedToKinematicChunkRecursive(chunk2, visited, ref result);
				}
			}
		}
	}

	public virtual void SetVaryingGravity(bool vg)
	{
	}

	public virtual bool CanChangeMass()
	{
		return false;
	}

	public virtual float GetCurrentMassChange()
	{
		return 0f;
	}

	public virtual bool VisibleInPlayMode()
	{
		return true;
	}

	public virtual bool ColliderIsTriggerInPlayMode()
	{
		return false;
	}

	public bool SelectableTerrain()
	{
		BlockMetaData blockMetaData = GetBlockMetaData();
		if (blockMetaData != null)
		{
			return blockMetaData.selectableTerrain;
		}
		return false;
	}

	public bool DisableBuildModeScale()
	{
		BlockMetaData blockMetaData = GetBlockMetaData();
		if (blockMetaData != null)
		{
			return blockMetaData.disableBuildModeScale;
		}
		return false;
	}

	public bool DisableBuildModeMove()
	{
		BlockMetaData blockMetaData = GetBlockMetaData();
		if (blockMetaData != null)
		{
			return blockMetaData.disableBuildModeMove;
		}
		return false;
	}

	public Vector3 AllowedBuildModeRotations()
	{
		BlockMetaData blockMetaData = GetBlockMetaData();
		if (blockMetaData != null)
		{
			return blockMetaData.allowedBuildModeRotations;
		}
		return Vector3.one;
	}

	public virtual bool DoubleTapToSelect()
	{
		return false;
	}

	private void UpdateIndexedTiles(Func<int, int> indexConverter)
	{
		HashSet<Predicate> hashSet = new HashSet<Predicate>();
		hashSet.Add(PredicateRegistry.ByName("Block.TutorialPaintExistingBlock"));
		hashSet.Add(PredicateRegistry.ByName("Block.TutorialTextureExistingBlock"));
		hashSet.Add(PredicateRegistry.ByName("Block.TutorialRotateExistingBlock"));
		hashSet.Add(predicateTutorialRemoveBlockHint);
		foreach (List<Tile> tile in tiles)
		{
			foreach (Tile item in tile)
			{
				if (hashSet.Contains(item.gaf.Predicate))
				{
					int arg = (int)item.gaf.Args[0];
					int num = indexConverter(arg);
					item.gaf.Args[0] = num;
				}
			}
		}
	}

	public void BlockRemoved(int index)
	{
		UpdateIndexedTiles((int oldIndex) => (oldIndex >= index) ? (oldIndex - 1) : oldIndex);
	}

	public void IndicesSwitched(int index1, int index2)
	{
		UpdateIndexedTiles(delegate(int num)
		{
			if (num == index1)
			{
				return index2;
			}
			return (num == index2) ? index1 : num;
		});
	}

	public virtual void RemovedPlayBlock(Block b)
	{
	}

	protected virtual bool CanScaleMesh(int meshIndex)
	{
		return true;
	}

	public virtual Vector3 GetCenter()
	{
		return goT.position;
	}

	public virtual Vector3 GetPlayModeCenter()
	{
		return goT.position;
	}

	public virtual void BecameTreasure()
	{
	}

	public virtual List<Collider> GetColliders()
	{
		List<Collider> list = new List<Collider>(1);
		Collider component = go.GetComponent<Collider>();
		if (component != null)
		{
			list.Add(component);
		}
		return list;
	}

	protected void SetChildrenLocalScale(float scale)
	{
		foreach (object item in goT)
		{
			Transform transform = (Transform)item;
			transform.localScale = Vector3.one * scale;
		}
	}

	public virtual void ChunkInModelFrozen()
	{
	}

	public virtual void ChunkInModelUnfrozen()
	{
	}

	public static Type GetBlockTypeFromName(string name)
	{
		if (blockNameTypeMap.TryGetValue(name, out var value))
		{
			return value;
		}
		return typeof(Block);
	}

	public void ReplaceMeshCollider(MeshCollider mc)
	{
		if (meshColliderInfo == null)
		{
			meshColliderInfo = new MeshColliderInfo
			{
				mesh = mc.sharedMesh,
				convex = mc.convex,
				material = mc.material,
				block = this
			};
			UnityEngine.Object.Destroy(mc);
			BoxCollider boxCollider = go.AddComponent<BoxCollider>();
			boxCollider.isTrigger = true;
			boxCollider.size = size;
		}
	}

	public virtual float GetFogMultiplier()
	{
		return 1f;
	}

	public virtual Color GetFogColorOverride()
	{
		return Color.white;
	}

	public virtual float GetLightIntensityMultiplier()
	{
		return 1f;
	}

	public bool IsUnlocked()
	{
		return ContainsTileWithPredicate(predicateUnlocked);
	}

	public virtual bool TreatAsVehicleLikeBlock()
	{
		return false;
	}

	protected bool TreatAsVehicleLikeBlockWithStatus(ref int treatAsVehicleStatus)
	{
		bool flag;
		if (treatAsVehicleStatus == -1)
		{
			flag = !didFix && ContainsTileWithAnyPredicateInPlayMode2(GetInputPredicates());
			treatAsVehicleStatus = (flag ? 1 : 0);
		}
		else
		{
			flag = treatAsVehicleStatus == 1;
		}
		return flag;
	}

	public static HashSet<Predicate> GetPossibleModelConflictingInputPredicates()
	{
		if (possibleModelConflictingInputPredicates == null)
		{
			possibleModelConflictingInputPredicates = new HashSet<Predicate>
			{
				predicateButton,
				predicateTiltLeftRight,
				predicateTiltFrontBack,
				predicateDPadMoved,
				predicateDPadVertical,
				predicateDPadHorizontal,
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
		return possibleModelConflictingInputPredicates;
	}

	public static HashSet<Predicate> GetInputPredicates()
	{
		if (inputPredicates == null)
		{
			inputPredicates = new HashSet<Predicate>
			{
				predicateButton,
				predicateTiltLeftRight,
				predicateDPadMoved,
				predicateDPadVertical,
				predicateDPadHorizontal,
				predicateTapBlock,
				predicateTapChunk,
				predicateTapModel,
				predicateTapHoldBlock,
				predicateTapHoldModel,
				predicateSendSignal,
				predicateSendCustomSignal,
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
		return inputPredicates;
	}

	public virtual float GetEffectPower()
	{
		return effectPower;
	}

	public virtual Vector3 GetEffectSize()
	{
		return size;
	}

	public virtual Vector3 GetEffectLocalOffset()
	{
		return Vector3.zero;
	}

	public virtual bool CanScaleUpwards()
	{
		return true;
	}

	public virtual void BunchMoved()
	{
	}

	public virtual void BunchRotated()
	{
	}

	public virtual void RemoveBlockMaps()
	{
		BWSceneManager.RemoveBlockInstanceIDs(go);
	}

	public virtual GAF GetIconGaf()
	{
		return new GAF(predicateCreate, BlockType());
	}

	public virtual Vector3 GetWaterForce(float fractionWithin, Vector3 relativeVelocity, BlockAbstractWater water)
	{
		return Vector3.zero;
	}

	public virtual void RestoredMeshCollider()
	{
	}

	public virtual Bounds GetShapeCollisionBounds()
	{
		return go.GetComponent<Collider>().bounds;
	}

	public virtual bool ShapeMeshCanCollideWith(Block b)
	{
		return true;
	}

	public virtual void Exploded()
	{
	}

	public virtual bool CanTriggerBlockListSensor()
	{
		return true;
	}

	public virtual bool BreakByDetachExplosion()
	{
		return true;
	}

	public virtual void TBoxSnap()
	{
	}

	private void UpdateNeighboringConnections()
	{
		for (int i = 0; i < connections.Count; i++)
		{
			Block block = connections[i];
			block.ConnectionsChanged();
		}
	}

	public virtual void ConnectionsChanged()
	{
	}

	public virtual List<GameObject> GetIgnoreRaycastGOs()
	{
		return new List<GameObject> { go };
	}

	public virtual bool IgnoreTextureToIndexInTutorial(int meshIndex)
	{
		return false;
	}

	public virtual bool IgnorePaintToIndexInTutorial(int meshIndex)
	{
		return false;
	}

	public virtual List<List<Tile>> GetRuntimeTiles()
	{
		return tiles;
	}

	public HashSet<Predicate> GetPlayPredicates()
	{
		if (tiles.Count == 2 && tiles[1].Count == 1)
		{
			return emptyPredicateSet;
		}
		HashSet<Predicate> hashSet = new HashSet<Predicate>();
		for (int i = 1; i < tiles.Count; i++)
		{
			List<Tile> list = tiles[i];
			for (int j = 0; j < list.Count; j++)
			{
				Predicate predicate = list[j].gaf.Predicate;
				if (predicate != predicateThen)
				{
					hashSet.Add(predicate);
				}
			}
		}
		return hashSet;
	}

	public BlockGroup GetGroupOfType(string groupType)
	{
		if (groups != null)
		{
			foreach (BlockGroup group in groups)
			{
				if (group.GetGroupType() == groupType)
				{
					return group;
				}
			}
		}
		return null;
	}

	public void RemoveGroup(string groupType)
	{
		BlockGroup groupOfType = GetGroupOfType(groupType);
		if (groupOfType != null)
		{
			groups.Remove(groupOfType);
		}
	}

	public bool IsMainBlockInGroup(string groupType)
	{
		BlockGroup groupOfType = GetGroupOfType(groupType);
		if (groupOfType != null)
		{
			return groupOfType.GetBlocks()[0] == this;
		}
		return false;
	}

	public Block GetMainBlockInGroup(string groupType)
	{
		BlockGroup groupOfType = GetGroupOfType(groupType);
		if (groupOfType == null)
		{
			return this;
		}
		return groupOfType.GetBlocks()[0];
	}

	public bool HasGroup(string groupType)
	{
		return GetGroupOfType(groupType) != null;
	}

	public bool HasAnyGroup()
	{
		if (groups != null)
		{
			return groups.Count > 0;
		}
		return false;
	}

	public virtual void SetBlockGroup(BlockGroup group)
	{
		if (groups == null)
		{
			groups = new HashSet<BlockGroup>();
		}
		groups.Add(group);
	}

	public virtual void OnBlockGroupReconstructed()
	{
	}

	public bool ContainsGroupTile()
	{
		List<Tile> list = tiles[0];
		for (int i = 6; i < list.Count; i++)
		{
			if (list[i].gaf.Predicate == predicateGroup)
			{
				return true;
			}
		}
		return false;
	}

	public virtual bool HasPreferredChunkRotation()
	{
		return false;
	}

	public virtual Quaternion GetPreferredChunkRotation()
	{
		return Quaternion.identity;
	}

	public virtual void ChunksAndJointsModified(Dictionary<Joint, Joint> oldToNew, Dictionary<Chunk, Chunk> oldToNewChunks, Dictionary<Chunk, Chunk> newToOldChunks)
	{
	}

	public virtual void Deactivate()
	{
		if (go != null)
		{
			go.SetActive(value: false);
		}
		if (goShadow != null)
		{
			goShadow.SetActive(value: false);
		}
	}

	public virtual void Activate()
	{
		if (go != null)
		{
			go.SetActive(value: true);
		}
		if (goShadow != null)
		{
			goShadow.SetActive(value: true);
		}
	}

	public virtual bool HasPreferredLookTowardAngleLocalVector()
	{
		return false;
	}

	public virtual Vector3 GetPreferredLookTowardAngleLocalVector()
	{
		return Vector3.back;
	}

	public TileResultCode IsFirstPersonBlock(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (Blocksworld.blocksworldCamera.firstPersonBlock == this)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode FirstPersonCamera(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (broken || vanished)
		{
			return TileResultCode.True;
		}
		Blocksworld.blocksworldCamera.FirstPersonFollow(this, Util.GetIntArg(args, 0, 0));
		return TileResultCode.True;
	}

	public TileResultCode SetHudReticle(ScriptRowExecutionInfo eInfo, object[] args)
	{
		Blocksworld.blocksworldCamera.SetHudReticle(Util.GetIntArg(args, 0, 1));
		return TileResultCode.True;
	}

	public virtual void SetFPCGearVisible(bool visible)
	{
		for (int i = 0; i < connections.Count; i++)
		{
			Block block = connections[i];
			int num = connectionTypes[i];
			if (num != 1)
			{
				continue;
			}
			BlockMetaData blockMetaData = block.GetBlockMetaData();
			if (!(null != blockMetaData) || !blockMetaData.hideInFirstPersonCamera)
			{
				continue;
			}
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

	public bool HasMover()
	{
		HashSet<Predicate> analogStickPredicates = Blocksworld.GetAnalogStickPredicates();
		HashSet<Predicate> tiltMoverPredicates = Blocksworld.GetTiltMoverPredicates();
		bool flag = false;
		for (int i = 0; i < tiles.Count; i++)
		{
			if (flag)
			{
				break;
			}
			bool flag2 = false;
			List<Tile> list = tiles[i];
			for (int j = 0; j < list.Count; j++)
			{
				if (flag)
				{
					break;
				}
				Predicate predicate = list[j].gaf.Predicate;
				if (predicate == predicateThen)
				{
					flag2 = true;
				}
				else if (flag2)
				{
					flag |= analogStickPredicates.Contains(predicate);
					flag |= tiltMoverPredicates.Contains(predicate);
				}
			}
		}
		return flag;
	}

	public bool HasAnyInputButton()
	{
		List<List<Tile>> runtimeTiles = GetRuntimeTiles();
		for (int i = 0; i < runtimeTiles.Count; i++)
		{
			List<Tile> list = runtimeTiles[i];
			for (int j = 0; j < list.Count; j++)
			{
				Predicate predicate = list[j].gaf.Predicate;
				if (predicate != predicateThen && predicate == predicateButton)
				{
					return true;
				}
			}
		}
		return false;
	}

	public virtual void StartPull()
	{
		if (chunk != null)
		{
			chunk.StartPull();
		}
	}

	public virtual void StopPull()
	{
		if (chunk != null)
		{
			chunk.StopPull();
		}
	}

	public virtual void PlaceInCharacterHand(BlockAnimatedCharacter character)
	{
	}

	public virtual void RemoveFromCharacterHand()
	{
	}

	public virtual bool CanRepelAttack(Vector3 attackPosition, Vector3 attackDirection)
	{
		return Invincibility.IsInvincible(this);
	}

	public virtual void OnAttacked(Vector3 attackPosition, Vector3 attackDirection)
	{
		if (chunk != null && chunk.rb != null)
		{
			chunk.rb.AddForceAtPosition(1000f * (attackDirection - attackDirection.y * Vector3.up), attackPosition);
		}
	}

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

	public TileResultCode IsHitByTaggedBlocksterHandAttachment(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string tag = (string)args[0];
		if (BlockAnimatedCharacter.HitByTaggedHandAttachment(this, tag))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

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

	public TileResultCode IsHitByTaggedBlocksterHandAttachmentModel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string tag = (string)args[0];
		if (BlockAnimatedCharacter.HitModelByTaggedHandAttachment(this, tag))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsFiredAsWeapon(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (BlockAnimatedCharacter.FiredAsWeapon(this))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	static Block()
	{
		goTouchStarted = false;
		goTouched = null;
		prefabShadow = null;
		shadowParent = null;
		massAlteredBlocks = new List<Block>();
		nextBlockId = 0;
		vanishAnimCurve = null;
		leaderboardData = null;
		noTilesAfterPredicates = new HashSet<Predicate>();
		defaultTextureNormals = null;
		blockTypeCanBeTextured = null;
		blockTypeCanBeMaterialTextured = new Dictionary<string, bool[]>();
		defaultScales = new Dictionary<string, Vector3>();
		defaultOrientations = new Dictionary<string, Vector3>();
		defaultSfxs = new Dictionary<string, HashSet<string>>();
		sameGridCellPairs = new Dictionary<string, HashSet<string>>();
		treasureBlocks = new HashSet<string> { "Coin", "Popsicle", "Feather", "Heart", "Key", "Energy Canister", "Amulet", "Idol", "Crystal Shard" };
		treasurePickupSfx = "SFX pick_up_sound_1";
		defaultExtraTiles = new Dictionary<string, List<List<Tile>>>();
		defaultExtraTilesProcessors = new Dictionary<string, Action<List<List<Tile>>>>();
		blockSizes = new Dictionary<string, Vector3>();
		resetExecutionInfo = new ScriptRowExecutionInfo();
		shadowBounds = default(Bounds);
		emptySet = new HashSet<string>();
		skinPaints = new HashSet<string>
		{
			"Light Magenta", "Magenta", "Light Pink", "Pink", "Dark Pink", "Light Red", "Deep Red", "Light Orange", "Earth Orange", "Light Yellow",
			"Dark Yellow", "Light Ginger", "Gingerbread Brown", "Dark Ginger", "Tan", "Beige", "Brown"
		};
		connectedCache = new Dictionary<Block, List<Block>>();
		connectedChunks = new Dictionary<Block, HashSet<Chunk>>();
		keepRBPreds = null;
		unmuteCommand = new UnmuteAllBlocksCommand();
		loopSfxCommand = new LoopSfxCommand();
		vanishingOrAppearingBlocks = new HashSet<Block>();
		updateBlockTriggerCommand = new UpdateBlockIsTriggerCommand();
		stopScriptsCommand = new StopScriptsCommand();
		unlockInputCommand = new UnlockInputCommand();
		transparent = new Color(0f, 0f, 0f, 0f);
		blockScaleConstraints = new Dictionary<string, Vector3[]>();
		blockNameTypeMap = new Dictionary<string, Type>
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
		possibleModelConflictingInputPredicates = null;
		inputPredicates = null;
		emptyPredicateSet = new HashSet<Predicate>();
	}
}
