using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockAbstractWheel : Block, INoCollisionSound
{
	public static Predicate predicateWheelDrive;

	public static Predicate predicateWheelTurn;

	private GameObject chassis;

	public List<GameObject> turnObjects = new List<GameObject>();

	public List<ConfigurableJoint> turnJoints = new List<ConfigurableJoint>();

	public List<ConfigurableJoint> spinJoints = new List<ConfigurableJoint>();

	private Collider[] ignoreColliders;

	private JointDrive driveX;

	private JointDrive driveYZ;

	private JointDrive suspensionDrive;

	private Quaternion initialRBToLocalRotation = Quaternion.identity;

	private Quaternion initialRBToLocalRotationInverse = Quaternion.identity;

	internal BlockSteeringWheel externalControlBlock;

	public bool isSpareTire;

	private int treatAsVehicleStatus = -1;

	private float speed;

	private float angle;

	private float speedTarget;

	private float angleTarget;

	private float lastSpeedTarget;

	public GameObject axle;

	private float scaleSpeed;

	public float maxSpeedInc = 99999f;

	private bool braking;

	private float brakeForce;

	private bool drivenToTurn;

	private float modelMass = -1f;

	private float wheelMass = -1f;

	private float brakeMultiplier = 1f;

	private float speedMultiplier = 1f;

	private float turnMultiplier = 1f;

	private float brakeEffectiveness = 1f;

	private float driveEffectiveness = 1f;

	private float jumpEffectiveness = 1f;

	private float steerEffectiveness = 1f;

	private static PhysicMaterial wheelFriction = new PhysicMaterial();

	public float onGround = 0.2f;

	private Vector3 pausedVelocityAxle;

	private Vector3 pausedAngularVelocityAxle;

	private bool engineLoopOn;

	private float engineLoopPitch = 1f;

	private float engineLoopVolume;

	private bool engineIncreasingPitch;

	private int idleEngineCounter;

	private LoopSoundSourceInfo loopSoundSourceInfo;

	private float turnTowardsMax;

	private float driveTowardsMax;

	private float volScale = 1f;

	private float pitchScale = 1f;

	internal bool jointsAdjusted;

	internal float suspensionHeightOverride = -1f;

	internal float suspensionLengthOverride = -1f;

	internal bool suspensionDamperOverrideActive;

	internal bool suspensionSpringOverrideActive;

	internal float suspensionDamperOverride = 10f;

	internal float suspensionSpringOverride = 50000f;

	private float externalSuspensionHeightOverride = -1f;

	private float externalSuspensionLengthOverride = -1f;

	private bool externalSuspensionDamperOverrideActive;

	private bool externalSuspensionSpringOverrideActive;

	private float externalSuspensionDamperOverride = 10f;

	private float externalSuspensionSpringOverride = 50000f;

	private float lastSuspensionHeight = -1234f;

	private float lastSuspensionLength = -1234f;

	private float lastSuspensionClicks = -1234f;

	private float lastSuspensionStiffness = -1234f;

	private float lastExternalSuspensionHeight = -1234f;

	private float lastExternalSuspensionLength = -1234f;

	private float lastExternalSuspensionClicks = -1234f;

	private float lastExternalSuspensionStiffness = -1234f;

	private HashSet<Block> blocksInModelSet;

	private static List<LoopSoundSourceInfo> loopSoundSources = new List<LoopSoundSourceInfo>();

	public string defaultWheelDefinition = "Generic";

	private static WheelDefinitions wheelDefs;

	private static Dictionary<string, WheelDefinition> wheelDefsDict;

	private WheelDefinition wheelDefinition = new WheelDefinition
	{
		suspensionSpring = 10f,
		suspensionDampen = 1.25f,
		suspensionLength = 0.5f,
		suspensionHeight = 0.25f,
		speedHelperMultiplier = 0.1f,
		wheelSpinMultiplier = 1.15f,
		turnHelperMultiplier = 0.45f,
		rampTurn = 22f,
		staticFriction = 0.5f,
		dynamicFriction = 0.6f
	};

	private static Transform helper_object_parent = null;

	private Vector3 _targetAngularVelocity = Vector3.zero;

	private int engineIntervalCounter;

	public BlockAbstractWheel(List<List<Tile>> tiles, string axleName = "", string wheelDefinitionName = "")
		: base(tiles)
	{
		if (!string.IsNullOrEmpty(axleName))
		{
			Transform transform = goT.Find(axleName);
			if ((bool)transform)
			{
				axle = transform.gameObject;
			}
		}
		loopName = "Wheel Engine Loop Default";
		defaultWheelDefinition = wheelDefinitionName;
		SetWheelDefinitionData(defaultWheelDefinition);
	}

	private static GameObject CreateNewHelperObject(string goName, string helperName)
	{
		if (helper_object_parent == null)
		{
			GameObject gameObject = new GameObject("Wheel Helper Objects");
			helper_object_parent = gameObject.transform;
		}
		GameObject gameObject2 = new GameObject(goName + " " + helperName);
		gameObject2.transform.parent = helper_object_parent;
		return gameObject2;
	}

	public new static void Register()
	{
		predicateWheelDrive = PredicateRegistry.Add<BlockAbstractWheel>("Wheel.Drive", (Block b) => ((BlockAbstractWheel)b).IsDrivingSensor, (Block b) => ((BlockAbstractWheel)b).Drive, new Type[1] { typeof(float) }, new string[1] { "Force" });
		predicateWheelTurn = PredicateRegistry.Add<BlockAbstractWheel>("Wheel.Turn", (Block b) => ((BlockAbstractWheel)b).IsTurning, (Block b) => ((BlockAbstractWheel)b).Turn, new Type[1] { typeof(float) }, new string[1] { "Angle" });
		PredicateRegistry.Add<BlockAbstractWheel>("Wheel.SetAsSpareTire", null, (Block b) => ((BlockAbstractWheel)b).SetAsSpareTire);
		PredicateRegistry.Add<BlockAbstractWheel>("Wheel.Brake", (Block b) => ((BlockAbstractWheel)b).IsBraking, (Block b) => ((BlockAbstractWheel)b).Brake, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockAbstractWheel>("Wheel.BrakeEffectiveness", (Block b) => ((BlockAbstractWheel)b).IsBrakeEffectiveness, (Block b) => ((BlockAbstractWheel)b).SetBrakeEffectiveness, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockAbstractWheel>("Wheel.DriveEffectiveness", (Block b) => ((BlockAbstractWheel)b).IsDriveEffectiveness, (Block b) => ((BlockAbstractWheel)b).SetDriveEffectiveness, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockAbstractWheel>("Wheel.JumpEffectiveness", (Block b) => ((BlockAbstractWheel)b).IsJumpEffectiveness, (Block b) => ((BlockAbstractWheel)b).SetJumpEffectiveness, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockAbstractWheel>("Wheel.SteerEffectiveness", (Block b) => ((BlockAbstractWheel)b).IsSteerEffectiveness, (Block b) => ((BlockAbstractWheel)b).SetSteerEffectiveness, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockAbstractWheel>("Wheel.SuspensionDamper", (Block b) => ((BlockAbstractWheel)b).IsSuspensionDamper, (Block b) => ((BlockAbstractWheel)b).SetSuspensionDamper, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockAbstractWheel>("Wheel.SuspensionHeight", (Block b) => ((BlockAbstractWheel)b).IsSuspensionHeight, (Block b) => ((BlockAbstractWheel)b).SetSuspensionHeight, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockAbstractWheel>("Wheel.SuspensionLength", (Block b) => ((BlockAbstractWheel)b).IsSuspensionLength, (Block b) => ((BlockAbstractWheel)b).SetSuspensionLength, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockAbstractWheel>("Wheel.SuspensionSpring", (Block b) => ((BlockAbstractWheel)b).IsSuspensionSpring, (Block b) => ((BlockAbstractWheel)b).SetSuspensionSpring, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockAbstractWheel>("Wheel.TurnTowardsTag", null, (Block b) => ((BlockAbstractWheel)b).TurnTowardsTag, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockAbstractWheel>("Wheel.DriveTowardsTag", null, (Block b) => ((BlockAbstractWheel)b).DriveTowardsTag, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockAbstractWheel>("Wheel.DriveTowardsTagRaw", null, (Block b) => ((BlockAbstractWheel)b).DriveTowardsTagRaw, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockAbstractWheel>("Wheel.IsWheelTowardsTag", (Block b) => ((BlockAbstractWheel)b).IsWheelTowardsTag, null, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockAbstractWheel>("Wheel.IsDPadAlongWheel", (Block b) => ((BlockAbstractWheel)b).IsDPadAlongWheel, null, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockAbstractWheel>("Wheel.TurnAlongDPad", null, (Block b) => ((BlockAbstractWheel)b).TurnAlongDPad, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockAbstractWheel>("Wheel.DriveAlongDPad", null, (Block b) => ((BlockAbstractWheel)b).DriveAlongDPad, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockAbstractWheel>("Wheel.DriveAlongDPadRaw", null, (Block b) => ((BlockAbstractWheel)b).DriveAlongDPadRaw, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		Block.AddSimpleDefaultTiles(new GAF("Wheel.Drive", 12.5f), "Wheel", "RAR Moon Rover Wheel", "Bulky Wheel", "Spoked Wheel", "Golden Wheel");
	}

	private static void LoadWheelDefinitions()
	{
		if (!(wheelDefs != null))
		{
			wheelDefs = Resources.Load<WheelDefinitions>("WheelDefinitions");
			wheelDefsDict = new Dictionary<string, WheelDefinition>();
			WheelDefinition[] definitions = wheelDefs.definitions;
			foreach (WheelDefinition wheelDefinition in definitions)
			{
				wheelDefsDict[wheelDefinition.name] = wheelDefinition;
			}
		}
	}

	public static WheelDefinition FindWheelDefinition(string name)
	{
		LoadWheelDefinitions();
		if (wheelDefsDict.TryGetValue(name, out var value))
		{
			return value;
		}
		return wheelDefs.definitions[0];
	}

	public void SetWheelDefinitionData(WheelDefinition def)
	{
		wheelDefinition = def;
		wheelFriction.staticFriction = wheelDefinition.staticFriction;
		wheelFriction.dynamicFriction = wheelDefinition.dynamicFriction;
	}

	public void SetWheelDefinitionData(string name)
	{
		LoadWheelDefinitions();
		if (wheelDefs.definitions.Length != 0)
		{
			wheelDefinition = FindWheelDefinition(name);
			SetWheelDefinitionData(wheelDefinition);
		}
	}

	public static void ClearLoopSoundSources()
	{
		loopSoundSources.Clear();
	}

	public override void Play()
	{
		base.Play();
		SetWheelDefinitionData(defaultWheelDefinition);
		isSpareTire = false;
		jointsAdjusted = true;
		externalControlBlock = null;
		treatAsVehicleStatus = -1;
		volScale = Mathf.Clamp(Util.MaxComponent(Scale()), 1f, 3f);
		pitchScale = Mathf.Clamp(1f / (volScale * 0.6f), 0.6f, 1f);
		scaleSpeed = 1f / GetRadius();
		speedTarget = 0f;
		lastSpeedTarget = 0f;
		speed = 0f;
		angle = 0f;
		onGround = 0.2f;
		modelMass = -1f;
		wheelMass = -1f;
		brakeEffectiveness = 1f;
		driveEffectiveness = 1f;
		jumpEffectiveness = 1f;
		steerEffectiveness = 1f;
		suspensionHeightOverride = -1f;
		suspensionLengthOverride = -1f;
		suspensionDamperOverrideActive = false;
		suspensionSpringOverrideActive = false;
		suspensionDamperOverride = 10f;
		suspensionSpringOverride = 50000f;
		externalSuspensionHeightOverride = -1f;
		externalSuspensionLengthOverride = -1f;
		externalSuspensionDamperOverrideActive = false;
		externalSuspensionSpringOverrideActive = false;
		externalSuspensionDamperOverride = 10f;
		externalSuspensionSpringOverride = 50000f;
		lastSuspensionHeight = -1234f;
		lastSuspensionLength = -1234f;
		lastSuspensionClicks = -1234f;
		lastSuspensionStiffness = -1234f;
		lastExternalSuspensionHeight = -1234f;
		lastExternalSuspensionLength = -1234f;
		lastExternalSuspensionClicks = -1234f;
		lastExternalSuspensionStiffness = -1234f;
		if (goT.GetComponent<Collider>().sharedMaterial == null)
		{
			goT.GetComponent<Collider>().sharedMaterial = wheelFriction;
		}
		ClearLoopSoundSources();
		loopSoundSourceInfo = new LoopSoundSourceInfo(goT.position, this);
		loopSoundSources.Add(loopSoundSourceInfo);
	}

	private void SetupJoints()
	{
		if (chassis == null)
		{
			return;
		}
		Rigidbody component = chassis.GetComponent<Rigidbody>();
		component.mass = Mathf.Max(component.mass, 2f);
		suspensionDrive.positionSpring = GetSuspensionSpring();
		suspensionDrive.positionDamper = GetSuspensionDamper();
		SoftJointLimit linearLimit = new SoftJointLimit
		{
			limit = GetSuspensionLength()
		};
		foreach (ConfigurableJoint turnJoint in turnJoints)
		{
			turnJoint.xDrive = suspensionDrive;
			turnJoint.linearLimit = linearLimit;
		}
	}

	public override void Play2()
	{
		Rigidbody rb = chunk.rb;
		Rigidbody rigidbody = ((!(goT.parent != null)) ? null : goT.parent.GetComponent<Rigidbody>());
		if (rb == null || rigidbody == null)
		{
			BWLog.Info("Missing rigidbody when parenting wheel, parented to character joint?");
			return;
		}
		rb.maxAngularVelocity = float.PositiveInfinity;
		rb.angularDrag = 0.025f;
		List<Block> list = ConnectionsOfType(2, directed: true);
		HashSet<Transform> hashSet = new HashSet<Transform>();
		foreach (Block item in list)
		{
			hashSet.Add(item.goT.parent);
		}
		ignoreColliders = null;
		if (hashSet.Count == 0)
		{
			if (axle != null)
			{
				axle.GetComponent<Renderer>().enabled = false;
			}
		}
		else
		{
			foreach (Transform item2 in hashSet)
			{
				Rigidbody component = item2.GetComponent<Rigidbody>();
				if (component == null)
				{
					BWLog.Info("Missing rigidbody when parenting wheel, parented to character joint?");
				}
				else
				{
					CreateJoints(item2.gameObject);
				}
			}
		}
		float num = Bunch.GetModelMass(this);
		brakeMultiplier = num * wheelDefinition.brakeHelperMultiplier;
		turnMultiplier = num * wheelDefinition.turnHelperMultiplier;
		float num2 = num * 0.5f + num * num * 0.001f;
		speedMultiplier = num2 * wheelDefinition.speedHelperMultiplier;
		blocksInModelSet = null;
		initialRBToLocalRotation = goT.localRotation;
		initialRBToLocalRotationInverse = Quaternion.Inverse(initialRBToLocalRotation);
	}

	public override void Update()
	{
		base.Update();
		if (!isTreasure && !broken && !vanished && !isSpareTire && !(goT.parent == null))
		{
			bool flag = externalControlBlock != null && !externalControlBlock.broken;
			flag &= turnObjects.Count > 0;
			if (flag & (Mathf.Abs(speedTarget) > 0f || (chunk != null && chunk.rb != null && chunk.rb.velocity.sqrMagnitude > 25f)))
			{
				Vector3 right = turnObjects[0].transform.right;
				Vector3 normalized = (goT.forward - Vector3.Dot(goT.forward, right) * right).normalized;
				Vector3 normalized2 = (goT.up - Vector3.Dot(goT.up, right) * right).normalized;
				goT.parent.rotation = Quaternion.LookRotation(normalized, normalized2) * initialRBToLocalRotationInverse;
			}
		}
	}

	public float GetRadius()
	{
		Vector3 scale = GetScale();
		return 0.25f * (scale.y + scale.z);
	}

	public void DestroyJoint(ConfigurableJoint joint)
	{
		turnJoints.Remove(joint);
		UnityEngine.Object.Destroy(joint);
	}

	private float GetSuspensionDamper()
	{
		if (chassis == null)
		{
			return 0f;
		}
		float num = (suspensionDamperOverrideActive ? suspensionDamperOverride : ((!externalSuspensionDamperOverrideActive) ? wheelDefinition.suspensionDampen : externalSuspensionDamperOverride));
		return num * modelMass;
	}

	private float GetSuspensionSpring()
	{
		if (chassis == null)
		{
			return 0f;
		}
		float num = (suspensionSpringOverrideActive ? suspensionSpringOverride : ((!externalSuspensionSpringOverrideActive) ? wheelDefinition.suspensionSpring : externalSuspensionSpringOverride));
		return num * modelMass;
	}

	private float GetSuspensionHeight()
	{
		float num = Mathf.Max(1f, GetScale().y);
		float num2 = ((suspensionHeightOverride >= 0f) ? suspensionHeightOverride : ((!(externalSuspensionHeightOverride >= 0f)) ? wheelDefinition.suspensionHeight : externalSuspensionHeightOverride));
		return num2 * num;
	}

	private float GetSuspensionLength()
	{
		float num = Mathf.Max(1f, GetScale().y);
		float num2 = ((suspensionLengthOverride >= 0f) ? suspensionLengthOverride : ((!(externalSuspensionLengthOverride >= 0f)) ? wheelDefinition.suspensionLength : externalSuspensionLengthOverride));
		return num2 * num;
	}

	public ConfigurableJoint CreateJoints(GameObject chassis)
	{
		ConfigurableJoint configurableJoint = chassis.AddComponent<ConfigurableJoint>();
		configurableJoint.autoConfigureConnectedAnchor = false;
		configurableJoint.enablePreprocessing = true;
		configurableJoint.enableCollision = false;
		GameObject gameObject = CreateNewHelperObject(go.name, "Turn Helper");
		Vector3 position = goT.parent.position;
		Vector3 anchor = position - chassis.transform.position;
		gameObject.transform.position = position;
		configurableJoint.anchor = anchor;
		configurableJoint.connectedAnchor = Vector3.zero;
		gameObject.transform.rotation = goT.rotation;
		Rigidbody component = goT.parent.GetComponent<Rigidbody>();
		Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
		BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
		boxCollider.size = GetScale() * 0.75f;
		boxCollider.sharedMaterial = goT.GetComponent<Collider>().sharedMaterial;
		float mass = (rigidbody.mass = component.mass / 2f);
		component.mass = mass;
		rigidbody.detectCollisions = false;
		configurableJoint.connectedBody = rigidbody;
		configurableJoint.axis = goT.up;
		configurableJoint.xMotion = ConfigurableJointMotion.Limited;
		configurableJoint.yMotion = ConfigurableJointMotion.Locked;
		configurableJoint.zMotion = ConfigurableJointMotion.Locked;
		configurableJoint.angularXMotion = ConfigurableJointMotion.Limited;
		configurableJoint.angularYMotion = ConfigurableJointMotion.Locked;
		configurableJoint.angularZMotion = ConfigurableJointMotion.Locked;
		configurableJoint.linearLimit = new SoftJointLimit
		{
			limit = GetSuspensionLength()
		};
		driveYZ.maximumForce = float.PositiveInfinity;
		driveYZ.positionSpring = 10000000f;
		driveYZ.positionDamper = 10f;
		configurableJoint.angularXDrive = driveYZ;
		float y = -0.1f;
		configurableJoint.targetPosition = goT.TransformDirection(new Vector3(0f, y, 0f));
		suspensionDrive.positionSpring = 1000f;
		suspensionDrive.positionDamper = 10f;
		suspensionDrive.maximumForce = float.PositiveInfinity;
		configurableJoint.xDrive = suspensionDrive;
		ConfigurableJoint configurableJoint2 = gameObject.AddComponent<ConfigurableJoint>();
		configurableJoint2.enablePreprocessing = true;
		configurableJoint2.enableCollision = false;
		configurableJoint2.anchor = component.transform.position - gameObject.transform.position;
		configurableJoint2.axis = Vector3.zero;
		configurableJoint2.xMotion = ConfigurableJointMotion.Locked;
		configurableJoint2.yMotion = ConfigurableJointMotion.Locked;
		configurableJoint2.zMotion = ConfigurableJointMotion.Locked;
		configurableJoint2.angularXMotion = ConfigurableJointMotion.Free;
		configurableJoint2.angularYMotion = ConfigurableJointMotion.Locked;
		configurableJoint2.angularZMotion = ConfigurableJointMotion.Locked;
		configurableJoint2.connectedBody = component;
		configurableJoint2.projectionMode = JointProjectionMode.PositionAndRotation;
		configurableJoint2.projectionDistance = 0.005f;
		turnObjects.Add(gameObject);
		turnJoints.Add(configurableJoint);
		spinJoints.Add(configurableJoint2);
		this.chassis = chassis;
		Collider component2 = goT.GetComponent<Collider>();
		ignoreColliders = chassis.GetComponentsInChildren<Collider>();
		for (int i = 0; i < ignoreColliders.Length; i++)
		{
			if (ignoreColliders[i].enabled && ignoreColliders[i].gameObject.activeInHierarchy)
			{
				Physics.IgnoreCollision(ignoreColliders[i], component2, ignore: true);
				Physics.IgnoreCollision(ignoreColliders[i], boxCollider, ignore: true);
			}
		}
		return configurableJoint;
	}

	internal void AssignExternalControl(BlockSteeringWheel controlBlock, List<BlockAbstractWheel> controlledWheels)
	{
		if (externalControlBlock == controlBlock)
		{
			return;
		}
		externalControlBlock = controlBlock;
		Collider component = goT.GetComponent<Collider>();
		HashSet<Collider> hashSet = new HashSet<Collider>();
		for (int i = 0; i < controlledWheels.Count; i++)
		{
			BlockAbstractWheel blockAbstractWheel = controlledWheels[i];
			if (blockAbstractWheel != this)
			{
				hashSet.Add(blockAbstractWheel.goT.GetComponent<Collider>());
			}
		}
		if (hashSet.Count <= 0)
		{
			return;
		}
		foreach (Collider item in hashSet)
		{
			Physics.IgnoreCollision(item, component, ignore: true);
		}
		if (ignoreColliders != null)
		{
			HashSet<Collider> other = new HashSet<Collider>(ignoreColliders);
			hashSet.UnionWith(other);
		}
		ignoreColliders = new Collider[hashSet.Count];
		hashSet.CopyTo(ignoreColliders);
	}

	private void DestroyJointsAndHelperObjects()
	{
		foreach (ConfigurableJoint spinJoint in spinJoints)
		{
			UnityEngine.Object.Destroy(spinJoint);
		}
		spinJoints.Clear();
		foreach (ConfigurableJoint turnJoint in turnJoints)
		{
			UnityEngine.Object.Destroy(turnJoint);
		}
		turnJoints.Clear();
		for (int i = 0; i < turnObjects.Count; i++)
		{
			UnityEngine.Object.Destroy(turnObjects[i]);
		}
		turnObjects.Clear();
	}

	public override void Stop(bool resetBlock = true)
	{
		ResetIgnoreCollision();
		DestroyJointsAndHelperObjects();
		if (chassis == null && axle != null && axle.GetComponent<Collider>() != null)
		{
			UnityEngine.Object.Destroy(axle.GetComponent<Collider>());
		}
		if (axle != null)
		{
			axle.GetComponent<Renderer>().enabled = true;
			axle.transform.localScale = Vector3.one;
		}
		engineLoopOn = false;
		PlayLoopSound(play: false, GetLoopClip());
		base.Stop(resetBlock);
	}

	public override void Pause()
	{
		engineLoopOn = false;
		PlayLoopSound(play: false, GetLoopClip());
	}

	public override void ResetFrame()
	{
		if (speedTarget != 0f)
		{
			lastSpeedTarget = speedTarget;
		}
		else
		{
			lastSpeedTarget = Mathf.Lerp(lastSpeedTarget, 0f, 0.01f);
		}
		speedTarget = 0f;
		angleTarget = 0f;
		turnTowardsMax = 0f;
		driveTowardsMax = 0f;
		braking = false;
		brakeForce = 0f;
		drivenToTurn = false;
	}

	private void UpdateBlocksInModelSetIfNecessary()
	{
		if (blocksInModelSet == null)
		{
			UpdateConnectedCache();
			blocksInModelSet = new HashSet<Block>(Block.connectedCache[this]);
		}
	}

	public void GlueToChassis()
	{
		if (chassis == null)
		{
			return;
		}
		foreach (ConfigurableJoint spinJoint in spinJoints)
		{
			UnityEngine.Object.Destroy(spinJoint);
		}
		spinJoints.Clear();
		foreach (ConfigurableJoint turnJoint in turnJoints)
		{
			UnityEngine.Object.Destroy(turnJoint);
		}
		turnJoints.Clear();
		chunk.Destroy();
		Blocksworld.chunks.Remove(chunk);
	}

	private void DriveRelativePoint(Vector3 targetPos, float howMuch, bool towards = true)
	{
		if (!(chassis == null))
		{
			Vector3 position = goT.position;
			Transform transform = chassis.transform;
			Vector3 vec = targetPos - position;
			if (!towards)
			{
				vec *= -1f;
			}
			vec = Util.ProjectOntoPlane(vec, transform.up);
			if (vec.sqrMagnitude > 0.0001f)
			{
				Vector3 normalized = vec.normalized;
				float f = Util.AngleBetween(normalized, transform.forward, transform.up);
				float num = Mathf.Abs(f);
				float num2 = num / 180f;
				float num3 = 1f - num2;
				num3 = Mathf.Clamp(num3 * num3, 0.25f, 1f);
				speedTarget += num3 * howMuch;
			}
		}
	}

	private void DriveOffsetRaw(Vector3 offset, float howMuch)
	{
		Vector3 rhs = Quaternion.Euler(0f, -90f, 0f) * goT.right;
		float num = Vector3.Dot(offset, rhs);
		speedTarget += howMuch * num;
	}

	private void TurnRelativePoint(Vector3 targetPos, float turnTowardsMax, bool towards = true)
	{
		if (!broken && !(chassis == null))
		{
			Vector3 position = goT.position;
			Transform transform = chassis.transform;
			Vector3 vec = targetPos - position;
			if (!towards)
			{
				vec *= -1f;
			}
			vec = Util.ProjectOntoPlane(vec, transform.up);
			Vector3 v = Quaternion.Euler(0f, -90f, 0f) * goT.right;
			if (vec.sqrMagnitude > 0.0001f)
			{
				Vector3 normalized = vec.normalized;
				float num = Util.AngleBetween(normalized, v, transform.up);
				angleTarget = Mathf.Clamp(angleTarget - num, 0f - turnTowardsMax, turnTowardsMax);
			}
		}
	}

	public TileResultCode IsBrakeEffectiveness(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float num = Util.GetFloatArg(args, 0, 1f) * 0.01f;
		if (num == brakeEffectiveness)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsDriveEffectiveness(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float num = Util.GetFloatArg(args, 0, 1f) * 0.01f;
		if (num == driveEffectiveness)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsJumpEffectiveness(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float num = Util.GetFloatArg(args, 0, 1f) * 0.01f;
		if (num == jumpEffectiveness)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsSteerEffectiveness(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float num = Util.GetFloatArg(args, 0, 1f) * 0.01f;
		if (num == steerEffectiveness)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode SetBrakeEffectiveness(ScriptRowExecutionInfo eInfo, object[] args)
	{
		brakeEffectiveness = Util.GetFloatArg(args, 0, 1f) * eInfo.floatArg * 0.01f;
		return TileResultCode.True;
	}

	public TileResultCode SetDriveEffectiveness(ScriptRowExecutionInfo eInfo, object[] args)
	{
		driveEffectiveness = Util.GetFloatArg(args, 0, 1f) * eInfo.floatArg * 0.01f;
		return TileResultCode.True;
	}

	public TileResultCode SetJumpEffectiveness(ScriptRowExecutionInfo eInfo, object[] args)
	{
		jumpEffectiveness = Util.GetFloatArg(args, 0, 1f) * eInfo.floatArg * 0.01f;
		return TileResultCode.True;
	}

	public TileResultCode SetSteerEffectiveness(ScriptRowExecutionInfo eInfo, object[] args)
	{
		steerEffectiveness = Util.GetFloatArg(args, 0, 1f) * eInfo.floatArg * 0.01f;
		return TileResultCode.True;
	}

	public TileResultCode SetAsSpareTire(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (!isSpareTire)
		{
			isSpareTire = true;
			suspensionHeightOverride = 0f;
			suspensionLengthOverride = 0f;
			SetupJoints();
			for (int i = 0; i < turnJoints.Count; i++)
			{
				turnJoints[i].angularXMotion = ConfigurableJointMotion.Locked;
			}
			for (int j = 0; j < spinJoints.Count; j++)
			{
				spinJoints[j].angularXMotion = ConfigurableJointMotion.Locked;
			}
		}
		return TileResultCode.True;
	}

	public TileResultCode SetSuspensionHeight(ScriptRowExecutionInfo eInfo, object[] args)
	{
		suspensionHeightOverride = Util.GetFloatArg(args, 0, 0.25f) * eInfo.floatArg;
		jointsAdjusted |= lastSuspensionHeight != suspensionHeightOverride;
		lastSuspensionHeight = suspensionHeightOverride;
		return TileResultCode.True;
	}

	public TileResultCode IsSuspensionHeight(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (lastSuspensionHeight == Util.GetFloatArg(args, 0, 0.25f))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public void SetExternalSuspensionHeight(float height)
	{
		externalSuspensionHeightOverride = height;
		jointsAdjusted |= lastExternalSuspensionHeight != height && suspensionHeightOverride < 0f;
		lastExternalSuspensionHeight = externalSuspensionHeightOverride;
	}

	public TileResultCode SetSuspensionLength(ScriptRowExecutionInfo eInfo, object[] args)
	{
		suspensionLengthOverride = Util.GetFloatArg(args, 0, 0.5f);
		jointsAdjusted |= lastSuspensionLength != suspensionLengthOverride;
		lastSuspensionLength = suspensionLengthOverride;
		return TileResultCode.True;
	}

	public TileResultCode IsSuspensionLength(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (lastSuspensionLength == Util.GetFloatArg(args, 0, 0.5f))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public void SetExternalSuspensionLength(float length)
	{
		externalSuspensionLengthOverride = length;
		jointsAdjusted |= lastExternalSuspensionLength != length && suspensionLengthOverride < 0f;
		lastExternalSuspensionLength = externalSuspensionLengthOverride;
	}

	public TileResultCode SetSuspensionDamper(ScriptRowExecutionInfo eInfo, object[] args)
	{
		suspensionDamperOverrideActive = true;
		float floatArg = Util.GetFloatArg(args, 0, 2f);
		suspensionDamperOverride = floatArg * floatArg * 0.2f;
		jointsAdjusted |= lastSuspensionClicks != floatArg;
		lastSuspensionClicks = floatArg;
		return TileResultCode.True;
	}

	public TileResultCode IsSuspensionDamper(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (lastSuspensionClicks == Util.GetFloatArg(args, 0, 2f))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public void SetExternalSuspensionDamper(float clicks)
	{
		externalSuspensionDamperOverrideActive = true;
		externalSuspensionDamperOverride = clicks * clicks * 0.2f;
		jointsAdjusted |= lastExternalSuspensionClicks != clicks;
		lastExternalSuspensionClicks = clicks;
	}

	public TileResultCode SetSuspensionSpring(ScriptRowExecutionInfo eInfo, object[] args)
	{
		suspensionSpringOverrideActive = true;
		float floatArg = Util.GetFloatArg(args, 0, 5f);
		suspensionSpringOverride = floatArg * floatArg * 2f + 5f;
		jointsAdjusted |= lastSuspensionStiffness != floatArg;
		lastSuspensionStiffness = floatArg;
		return TileResultCode.True;
	}

	public TileResultCode IsSuspensionSpring(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (lastSuspensionStiffness == Util.GetFloatArg(args, 0, 5f))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public void SetExternalSuspensionSpring(float stiffness)
	{
		externalSuspensionSpringOverrideActive = true;
		externalSuspensionSpringOverride = stiffness * stiffness * 2f + 5f;
		jointsAdjusted |= lastExternalSuspensionStiffness != stiffness;
		lastExternalSuspensionStiffness = stiffness;
	}

	public TileResultCode DriveTowardsTag(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string tagName = ((args.Length == 0) ? string.Empty : ((string)args[0]));
		float num = ((args.Length <= 1) ? 20f : ((float)args[1]));
		driveTowardsMax += num * eInfo.floatArg;
		UpdateBlocksInModelSetIfNecessary();
		if (TagManager.TryGetClosestBlockWithTag(tagName, goT.position, out var block, blocksInModelSet))
		{
			Transform transform = block.goT;
			Vector3 position = transform.position;
			DriveRelativePoint(position, Mathf.Abs(num), num > 0f);
			return TileResultCode.True;
		}
		return TileResultCode.True;
	}

	public TileResultCode DriveTowardsTagRaw(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string tagName = ((args.Length == 0) ? string.Empty : ((string)args[0]));
		float num = ((args.Length <= 1) ? 20f : ((float)args[1]));
		Vector3 position = goT.position;
		UpdateBlocksInModelSetIfNecessary();
		if (TagManager.TryGetClosestBlockWithTag(tagName, position, out var block, blocksInModelSet))
		{
			Transform transform = block.goT;
			Vector3 position2 = transform.position;
			Vector3 vector = position2 - position;
			if (vector.sqrMagnitude > 0.01f)
			{
				DriveOffsetRaw(vector.normalized, num * eInfo.floatArg);
			}
			return TileResultCode.True;
		}
		return TileResultCode.True;
	}

	public TileResultCode DriveAlongDPad(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string key = ((args.Length == 0) ? string.Empty : ((string)args[0]));
		float num = ((args.Length <= 1) ? 20f : ((float)args[1]));
		Blocksworld.UI.Controls.EnableDPad(key, MoverDirectionMask.ALL);
		driveTowardsMax += num * eInfo.floatArg;
		Vector3 worldDPadOffset = Blocksworld.UI.Controls.GetWorldDPadOffset(key);
		Vector3 position = goT.position;
		Vector3 targetPos = position + worldDPadOffset * 1000f;
		DriveRelativePoint(targetPos, num, num > 0f);
		return TileResultCode.True;
	}

	public TileResultCode DriveAlongDPadRaw(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string key = ((args.Length == 0) ? string.Empty : ((string)args[0]));
		float num = ((args.Length <= 1) ? 20f : ((float)args[1]));
		Blocksworld.UI.Controls.EnableDPad(key, MoverDirectionMask.ALL);
		Vector3 worldDPadOffset = Blocksworld.UI.Controls.GetWorldDPadOffset(key);
		DriveOffsetRaw(worldDPadOffset, num * eInfo.floatArg);
		return TileResultCode.True;
	}

	public void Brake(float f)
	{
		braking = true;
		brakeForce = f * f;
		engineLoopOn = false;
		idleEngineCounter = 0;
		engineIncreasingPitch = false;
	}

	public TileResultCode Brake(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (externalControlBlock == null || isSpareTire)
		{
			Brake(eInfo.floatArg);
		}
		return TileResultCode.True;
	}

	public TileResultCode IsBraking(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (braking)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public void Drive(float f, bool driveTurning)
	{
		speedTarget += f;
		drivenToTurn = driveTurning;
		engineLoopOn = !broken;
		idleEngineCounter = 0;
		engineIncreasingPitch = true;
	}

	public TileResultCode Drive(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (externalControlBlock == null || isSpareTire)
		{
			float f = (float)args[0] * eInfo.floatArg;
			Drive(f, driveTurning: false);
		}
		return TileResultCode.True;
	}

	public void KeepSpinning()
	{
		SetMotorTorqueNoChassis(lastSpeedTarget);
	}

	private bool IsDirectionAlongWheel(Vector3 direction, float sign, ScriptRowExecutionInfo eInfo, bool analog)
	{
		if (direction.sqrMagnitude > 0.001f)
		{
			Vector3 lhs = Quaternion.Euler(0f, -90f, 0f) * goT.right;
			float num = sign * Vector3.Dot(lhs, direction.normalized);
			if (num > 0f)
			{
				if (analog)
				{
					eInfo.floatArg = Mathf.Min(eInfo.floatArg, num);
				}
				return true;
			}
		}
		return false;
	}

	public TileResultCode IsDPadAlongWheel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string key = ((args.Length == 0) ? string.Empty : ((string)args[0]));
		float sign = ((args.Length <= 1) ? 1f : ((float)args[1]));
		Blocksworld.UI.Controls.EnableDPad(key, MoverDirectionMask.ALL);
		Vector3 worldDPadOffset = Blocksworld.UI.Controls.GetWorldDPadOffset(key);
		if (IsDirectionAlongWheel(worldDPadOffset, sign, eInfo, analog: false))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsWheelTowardsTag(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string tagName = ((args.Length == 0) ? string.Empty : ((string)args[0]));
		float sign = ((args.Length <= 1) ? 1f : ((float)args[1]));
		Vector3 position = goT.position;
		if (TagManager.TryGetClosestBlockWithTag(tagName, position, out var block))
		{
			Vector3 direction = block.goT.position - position;
			if (IsDirectionAlongWheel(direction, sign, eInfo, analog: false))
			{
				return TileResultCode.True;
			}
		}
		return TileResultCode.False;
	}

	public TileResultCode TurnAlongDPad(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string key = ((args.Length == 0) ? string.Empty : ((string)args[0]));
		float num = ((args.Length <= 1) ? 30f : ((float)args[1]));
		Blocksworld.UI.Controls.EnableDPad(key, MoverDirectionMask.ALL);
		Vector3 worldDPadOffset = Blocksworld.UI.Controls.GetWorldDPadOffset(key);
		turnTowardsMax += num * eInfo.floatArg;
		Vector3 targetPos = goT.position + worldDPadOffset * 1000f;
		TurnRelativePoint(targetPos, turnTowardsMax, turnTowardsMax > 0f);
		return TileResultCode.True;
	}

	public TileResultCode TurnTowardsTag(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string tagName = ((args.Length == 0) ? string.Empty : ((string)args[0]));
		float num = ((args.Length <= 1) ? 30f : ((float)args[1]));
		turnTowardsMax += num * eInfo.floatArg;
		UpdateBlocksInModelSetIfNecessary();
		if (TagManager.TryGetClosestBlockWithTag(tagName, goT.position, out var block, blocksInModelSet))
		{
			Transform transform = block.goT;
			Vector3 position = transform.position;
			TurnRelativePoint(position, Mathf.Abs(turnTowardsMax), turnTowardsMax > 0f);
		}
		return TileResultCode.True;
	}

	public void Turn(float angleOffset)
	{
		angleTarget += angleOffset;
	}

	public TileResultCode Turn(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (externalControlBlock == null || isSpareTire)
		{
			float num = (float)args[0] * Mathf.Min(1f, 2f * eInfo.floatArg);
			angleTarget += num;
		}
		return TileResultCode.True;
	}

	public TileResultCode IsDrivingSensor(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (!((float)args[0] <= 0f))
		{
			return IsDriving();
		}
		return IsBreaking();
	}

	public TileResultCode IsDriving()
	{
		Vector3 rhs = Quaternion.Euler(0f, -90f, 0f) * goT.right;
		if (!(goT.parent.GetComponent<Rigidbody>().velocity.sqrMagnitude <= 0.25f) && !(Vector3.Dot(goT.parent.GetComponent<Rigidbody>().velocity, rhs) <= 0f))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsBreaking()
	{
		Vector3 rhs = Quaternion.Euler(0f, -90f, 0f) * goT.right;
		if (!(goT.parent.GetComponent<Rigidbody>().velocity.sqrMagnitude <= 0.25f) && !(Vector3.Dot(goT.parent.GetComponent<Rigidbody>().velocity, rhs) >= 0f))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsTurning(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (!((float)args[0] >= 0f))
		{
			return IsTurningLeft();
		}
		return IsTurningRight();
	}

	public TileResultCode IsTurningLeft()
	{
		if (!(angleTarget >= 0f))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsTurningRight()
	{
		if (!(angleTarget <= 0f))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	private void SetMotor(float targetSpeed)
	{
		float new_x = targetSpeed * scaleSpeed;
		float num = Mathf.Clamp(wheelMass / (scaleSpeed * scaleSpeed), 20f, 100000f);
		float num2 = num * wheelDefinition.wheelSpinMultiplier * modelMass;
		for (int i = 0; i < spinJoints.Count; i++)
		{
			ConfigurableJoint configurableJoint = spinJoints[i];
			if (configurableJoint != null && configurableJoint.connectedBody != null)
			{
				Rigidbody connectedBody = configurableJoint.connectedBody;
				if (connectedBody.IsSleeping())
				{
					connectedBody.WakeUp();
				}
				_targetAngularVelocity.Set(new_x, 0f, 0f);
				configurableJoint.targetAngularVelocity = _targetAngularVelocity;
				if (braking)
				{
					driveX.positionDamper = suspensionDrive.positionDamper * num2 * brakeEffectiveness;
				}
				else if (targetSpeed != 0f)
				{
					driveX.positionDamper = suspensionDrive.positionDamper * num2;
				}
				else
				{
					driveX.positionDamper = 0f;
				}
				driveX.positionSpring = 0f;
				driveX.maximumForce = num2;
				configurableJoint.angularXDrive = driveX;
			}
		}
	}

	private void SetMotorTorqueNoChassis(float targetSpeed)
	{
		if (chunk != null && !(chunk.rb == null))
		{
			float num = targetSpeed * scaleSpeed;
			Vector3 vector = Vector3.right * num;
			Vector3 vector2 = initialRBToLocalRotationInverse * chunk.rb.angularVelocity;
			if (vector.x > 0f && vector2.x < vector.x)
			{
				float num2 = Mathf.Clamp(wheelMass / (scaleSpeed * scaleSpeed), 5f, 100000f);
				float num3 = num2 * wheelDefinition.wheelSpinMultiplier * wheelMass;
				chunk.rb.AddRelativeTorque(initialRBToLocalRotation * Vector3.right * num3);
			}
			else if (vector.x < 0f && vector2.x > vector.x)
			{
				float num4 = -1f * Mathf.Clamp(wheelMass / (scaleSpeed * scaleSpeed), 5f, 100000f);
				float num5 = num4 * wheelDefinition.wheelSpinMultiplier * wheelMass;
				chunk.rb.AddRelativeTorque(initialRBToLocalRotation * Vector3.right * num5);
			}
		}
	}

	private void SetAngle(float angle)
	{
		for (int i = 0; i < turnJoints.Count; i++)
		{
			ConfigurableJoint configurableJoint = turnJoints[i];
			if (configurableJoint != null && configurableJoint.connectedBody != null)
			{
				SoftJointLimit softJointLimit = new SoftJointLimit
				{
					limit = angle
				};
				if (angle > 0f)
				{
					configurableJoint.highAngularXLimit = softJointLimit;
				}
				else if (angle == 0f)
				{
					configurableJoint.highAngularXLimit = softJointLimit;
					configurableJoint.lowAngularXLimit = softJointLimit;
				}
				else
				{
					configurableJoint.lowAngularXLimit = softJointLimit;
				}
				configurableJoint.targetRotation = Quaternion.Euler(angle, 0f, angle);
				Rigidbody connectedBody = configurableJoint.connectedBody;
				if (connectedBody.IsSleeping())
				{
					connectedBody.WakeUp();
				}
			}
		}
	}

	private void UpdateEngineSound()
	{
		if (!Sound.sfxEnabled || isTreasure || vanished || externalControlBlock != null)
		{
			PlayLoopSound(play: false, GetLoopClip());
			return;
		}
		Transform transform = goT;
		if (transform.parent == null)
		{
			return;
		}
		Rigidbody component = transform.parent.GetComponent<Rigidbody>();
		if (component == null)
		{
			return;
		}
		int num = 4;
		bool flag = engineIntervalCounter % num == 0;
		engineIntervalCounter++;
		float num2 = Mathf.Abs(Vector3.Dot(component.angularVelocity, transform.right));
		float num3 = num2 / 15f;
		float max = 3f * pitchScale;
		float num4 = 0.1f;
		float num5 = 0.6f * pitchScale;
		float num6 = num5 + num3;
		float num7 = 0.02f;
		if (num6 < engineLoopPitch)
		{
			num7 *= -1f;
		}
		float num8 = ((!engineIncreasingPitch) ? (-0.07f) : num7);
		engineLoopPitch = Mathf.Clamp(engineLoopPitch + num8, (!engineLoopOn) ? num4 : num5, max);
		Vector3 position = transform.position;
		this.loopSoundSourceInfo.Update(position, engineLoopPitch, engineLoopOn);
		float num9 = 1f;
		for (int i = 0; i < loopSoundSources.Count; i++)
		{
			LoopSoundSourceInfo loopSoundSourceInfo = loopSoundSources[i];
			if (loopSoundSourceInfo.block != this && loopSoundSourceInfo.playing)
			{
				float magnitude = (position - loopSoundSourceInfo.pos).magnitude;
				float num10 = Mathf.Abs(loopSoundSourceInfo.pitch - this.loopSoundSourceInfo.pitch);
				num9 = Mathf.Min(num9 + num10, Mathf.Clamp(magnitude / 7f - 0.75f, 0f, 1f));
			}
		}
		float num11 = ((!engineLoopOn) ? (-0.01f) : 0.1f);
		num11 *= volScale;
		engineLoopVolume = Mathf.Clamp(engineLoopVolume + num11, 0f, 0.2f * volScale);
		if (engineLoopOn && num8 < 0f)
		{
			idleEngineCounter++;
			if (idleEngineCounter > 100)
			{
				engineLoopOn = false;
			}
		}
		float num12 = num9 * engineLoopVolume;
		if (flag)
		{
			if (Sound.BlockIsMuted(this))
			{
				num12 = 0f;
			}
			float num13 = 1f;
			if (num12 > 0.01f)
			{
				float num14 = 0.1f;
				num13 = num14 * 2f * (Mathf.PerlinNoise(Time.time, 0f) - 0.5f) + 1f;
			}
			PlayLoopSound(num12 > 0.01f, GetLoopClip(), num12, null, num13 * 0.7f * engineLoopPitch);
		}
		UpdateWithinWaterLPFilter();
		engineIncreasingPitch = false;
	}

	private void ResetIgnoreCollision()
	{
		if (ignoreColliders == null)
		{
			return;
		}
		Collider component = goT.GetComponent<Collider>();
		for (int i = 0; i < ignoreColliders.Length; i++)
		{
			if (ignoreColliders[i] != null && ignoreColliders[i].enabled && ignoreColliders[i].gameObject.activeInHierarchy)
			{
				Physics.IgnoreCollision(ignoreColliders[i], component, ignore: false);
			}
		}
		ignoreColliders = null;
	}

	private void DisconnectFromStuff()
	{
		ResetIgnoreCollision();
		chassis = null;
		externalControlBlock = null;
		DestroyJointsAndHelperObjects();
		engineLoopOn = false;
		PlayLoopSound(play: false, GetLoopClip());
	}

	public override void Break(Vector3 chunkPos, Vector3 chunkVel, Vector3 chunkAngVel)
	{
		if (!broken)
		{
			DisconnectFromStuff();
			base.Break(chunkPos, chunkVel, chunkAngVel);
		}
	}

	public override void RemovedPlayBlock(Block b)
	{
		base.RemovedPlayBlock(b);
		if (chassis == b.go || externalControlBlock == b)
		{
			DisconnectFromStuff();
		}
	}

	public void FixedUpdateDriveAndTurn(float jumpYOffset)
	{
		UpdateEngineSound();
		if (isTreasure || broken || vanished || isSpareTire)
		{
			return;
		}
		if (modelMass < 0f)
		{
			modelMass = Bunch.GetModelMassPerType<BlockAbstractWheel>(this);
			wheelMass = goT.parent.GetComponent<Rigidbody>().mass;
		}
		if (jointsAdjusted)
		{
			jointsAdjusted = false;
			SetupJoints();
		}
		speedTarget *= ((!drivenToTurn) ? driveEffectiveness : steerEffectiveness);
		angleTarget *= steerEffectiveness;
		SetAngle(angle);
		if (chassis == null)
		{
			angleTarget = Mathf.Clamp(angleTarget, -20f, 20f);
			SetMotorTorqueNoChassis(speedTarget);
			return;
		}
		SetMotor(0f - speed);
		angle += (angleTarget - angle) / wheelDefinition.rampTurn;
		Vector3 forward;
		Vector3 vector;
		Vector3 right;
		if (externalControlBlock != null)
		{
			forward = externalControlBlock.goT.forward;
			vector = -externalControlBlock.goT.up;
			right = externalControlBlock.goT.right;
		}
		else
		{
			forward = turnObjects[0].transform.forward;
			vector = -turnObjects[0].transform.up;
			right = turnObjects[0].transform.right;
		}
		if (goT.parent == null)
		{
			return;
		}
		Rigidbody component = goT.parent.GetComponent<Rigidbody>();
		float num = ((!(onGround <= 0f)) ? Vector3.Dot(forward, component.velocity) : speed);
		if (braking)
		{
			if (brakeEffectiveness >= 1f && num < 25f)
			{
				speed = 0f;
			}
			else
			{
				speed = Mathf.Lerp(num, 0f, 0.1f * brakeEffectiveness);
			}
		}
		else
		{
			speed = Mathf.Lerp(num, speedTarget, 0.1f * ((!drivenToTurn) ? driveEffectiveness : steerEffectiveness));
		}
		if (onGround > 0f)
		{
			Vector3 position = goT.position;
			Vector3 zero = Vector3.zero;
			if (Mathf.Abs(angle) > 10f)
			{
				zero += modelMass * 0.00125f * right * num * Mathf.Sin(angle * ((float)Math.PI / 180f)) * turnMultiplier;
				zero += vector * 0.35f * Mathf.Abs(num) * speedMultiplier;
			}
			if (braking)
			{
				zero -= forward * num * brakeMultiplier * Mathf.Min(1f, brakeForce * brakeEffectiveness);
			}
			else
			{
				zero += forward * speedTarget * speedMultiplier;
			}
			zero += vector * 0.35f * Mathf.Abs(num) * speedMultiplier;
			component.AddForceAtPosition(zero, position);
		}
		bool flag = true;
		for (int i = 0; i < turnJoints.Count; i++)
		{
			ConfigurableJoint configurableJoint = turnJoints[i];
			if (configurableJoint != null)
			{
				float y = configurableJoint.connectedAnchor.y;
				float num2 = Mathf.Lerp(y, GetSuspensionHeight() + jumpYOffset * jumpEffectiveness, 0.85f);
				float num3 = num2 - y;
				if (num3 > 0.01f)
				{
					configurableJoint.connectedAnchor += Vector3.up * num3;
				}
				else if (num3 < -0.01f)
				{
					configurableJoint.connectedAnchor += Vector3.up * num3;
				}
				Rigidbody connectedBody = configurableJoint.connectedBody;
				Rigidbody component2 = configurableJoint.GetComponent<Rigidbody>();
				if (connectedBody == null || connectedBody.isKinematic || component2 == null || component2.isKinematic)
				{
					flag = false;
					break;
				}
			}
		}
		if (flag)
		{
			Vector3 vec = forward * 0.25f * Mathf.Clamp(speed, -60f, 60f);
			vec = Util.ProjectOntoPlane(vec, Vector3.up);
			Blocksworld.blocksworldCamera.AddForceDirectionHint(chunk, vec);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (goT.parent != null && CollisionManager.IsImpactingBlock(this))
		{
			onGround = Blocksworld.fixedDeltaTime * 10f;
		}
		else if (onGround > 0f)
		{
			onGround = Mathf.Max(0f, onGround - Blocksworld.fixedDeltaTime);
		}
		if (externalControlBlock == null)
		{
			FixedUpdateDriveAndTurn(0f);
		}
	}

	public GameObject GetRealChassis()
	{
		return chassis;
	}

	public float GetHelperObjectMass()
	{
		float num = 0f;
		for (int i = 0; i < turnObjects.Count; i++)
		{
			num += turnObjects[i].GetComponent<Rigidbody>().mass;
		}
		return num;
	}

	public override bool TreatAsVehicleLikeBlock()
	{
		return TreatAsVehicleLikeBlockWithStatus(ref treatAsVehicleStatus);
	}

	public override void ChunksAndJointsModified(Dictionary<Joint, Joint> oldToNew, Dictionary<Chunk, Chunk> oldToNewChunks, Dictionary<Chunk, Chunk> newToOldChunks)
	{
		if (broken || isTreasure)
		{
			return;
		}
		for (int i = 0; i < turnJoints.Count; i++)
		{
			ConfigurableJoint configurableJoint = turnJoints[i];
			if (configurableJoint != null && oldToNew.TryGetValue(configurableJoint, out var value))
			{
				configurableJoint = (ConfigurableJoint)value;
				turnJoints[i] = configurableJoint;
				chassis = configurableJoint.gameObject;
			}
		}
		for (int j = 0; j < spinJoints.Count; j++)
		{
			ConfigurableJoint configurableJoint2 = spinJoints[j];
			if (configurableJoint2 != null && oldToNew.TryGetValue(configurableJoint2, out var value2))
			{
				configurableJoint2 = (ConfigurableJoint)value2;
				spinJoints[j] = configurableJoint2;
			}
		}
	}
}
