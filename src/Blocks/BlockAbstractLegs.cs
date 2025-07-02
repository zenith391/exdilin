using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public abstract class BlockAbstractLegs : Block
{
	public static ObjectSfxRules globalSfxRules;

	protected AudioSource voxAudioSource;

	protected GameObject voxGameObject;

	public Dictionary<string, Dictionary<string, List<string>>> textureSemanticSfxs = new Dictionary<string, Dictionary<string, List<string>>>();

	public Dictionary<string, List<string>> connectedBlockTypeSemanticSfxs = new Dictionary<string, List<string>>();

	public Dictionary<string, List<string>> semanticSfxs = new Dictionary<string, List<string>>();

	public Dictionary<string, bool> canInterrupts = new Dictionary<string, bool>();

	public Dictionary<string, int> forbiddenCounters = new Dictionary<string, int>();

	public Dictionary<string, List<string>> defaultSemanticSfxs = new Dictionary<string, List<string>>();

	protected float maxSurfaceWalkAngle = 45f;

	protected float maxSurfaceWalkAngleDot = 0.7f;

	private int treatAsVehicleStatus = -1;

	protected bool wacky;

	protected bool replaceWithCapsuleCollider;

	protected Vector3 capsuleColliderOffset = Vector3.zero;

	protected float capsuleColliderHeight = 1f;

	protected float capsuleColliderRadius = 0.5f;

	private static PhysicMaterial capsuleColliderMaterial;

	private bool hasOuchSound;

	private bool hasWheeSound;

	private bool hasOhSound;

	private float ouchPlayProbability = 1f;

	private float wheePlayProbability = 0.7f;

	private float ohPlayProbability = 0.4f;

	public WalkController walkController;

	public bool controllerWasActive;

	private float previousWackiness;

	public float modelMass = 1f;

	public float footOffsetY;

	protected bool moveCM;

	protected float moveCMOffsetFeetCenter = 1f;

	protected bool hasMovedCM;

	protected float moveCMMaxDistance = 1f;

	protected bool hasChangedInertia;

	public int legPairCount = 1;

	public float[] legPairOffsets;

	public int[][] legPairIndices;

	public float ankleYSeparation;

	public float stepLengthMultiplier = 1f;

	public float stepTimeMultiplier = 1f;

	protected bool oneAnkleMeshPerFoot;

	private bool playCallouts = true;

	private LegParameters legParameters;

	private static Dictionary<string, int> numLegsOnChunk = new Dictionary<string, int>();

	private List<GameObject> ignoreRaycastGOs;

	protected float stepSpeedTrigger = 0.4f;

	private float stepTime = 0.125f;

	private float footWeight = 0.5f;

	private float ankleOffset = 0.125f;

	public float maxStepLength = 1f;

	public float maxStepHeight = 1f;

	public float onGroundHeight = 0.5f;

	public GameObject body;

	public FootInfo[] feet;

	protected bool resetFeetPositionsOnCreate;

	protected Dictionary<string, string> partNames;

	private Vector3 oldLocalScale = Vector3.one;

	public float stepTimer;

	private int currentLeg;

	public bool upright;

	public bool unmoving;

	public bool footHitGround;

	private GameObject groundGO;

	public int jumpCountdown;

	public int startJumpCountdown = 50;

	private bool wasBroken;

	private bool onGround = true;

	private bool idle;

	public bool ignoreRotation;

	public bool keepCollider;

	private bool multiLegged;

	private int legIndex;

	private Vector3 vpe = Vector3.zero;

	private Vector3 vi = Vector3.zero;

	private Vector3 vd = Vector3.zero;

	public static int raycastMask = 539;

	private Vector3 oldVel = Vector3.zero;

	private float wheeness;

	private int lpFilterUpdateCounter;

	private int noCollisionCounter;

	private float speakSoundTime;

	private float speakOuchSoundTime;

	private float torqueMult = 1f;

	private float xWidth = 0.25f;

	public Shader InvisibleShader;

	private const float IPM_G = 9.82f;

	private const float IPM_VEL_FACTOR = 385.72958f;

	private const float UPRIGHT_CONTROL_P = 1f;

	private const float UPRIGHT_CONTROL_I = 2f;

	private const float UPRIGHT_CONTROL_D = 0.03f;

	private static Shader invisibleShader;

	public BlockAbstractLegs(List<List<Tile>> tiles, Dictionary<string, string> partNames = null, int legPairCount = 1, float[] legPairOffsets = null, int[][] legPairIndices = null, float ankleYSeparation = 0f, bool oneAnkleMeshPerFoot = false, float torqueMultiplier = 1f, float footWidth = 0.25f)
		: base(tiles)
	{
		if (partNames == null)
		{
			partNames = new Dictionary<string, string>();
		}
		this.partNames = partNames;
		this.legPairCount = legPairCount;
		this.ankleYSeparation = ankleYSeparation;
		this.oneAnkleMeshPerFoot = oneAnkleMeshPerFoot;
		torqueMult = torqueMultiplier;
		xWidth = footWidth;
		FindFeet();
		StoreDefaultFootScale();
		if (legPairOffsets == null)
		{
			legPairOffsets = new float[1];
		}
		this.legPairOffsets = legPairOffsets;
		if (legPairIndices == null)
		{
			legPairIndices = new int[1][] { new int[2] { 0, 1 } };
		}
		this.legPairIndices = legPairIndices;
		Materials.GetMaterial("White", "Plain", Materials.shaders["Plain"]);
		InvisibleShader = Shader.Find("Blocksworld/Invisible");
		unmoving = false;
	}

	public List<string> GetPossibleSfxs(string semantic, bool warn = true)
	{
		if (semanticSfxs.TryGetValue(semantic, out var value))
		{
			return value;
		}
		if (defaultSemanticSfxs.TryGetValue(semantic, out value))
		{
			return value;
		}
		if (warn)
		{
			BWLog.Warning("Could not find any sfxs for " + semantic);
		}
		return new List<string>();
	}

	public string SampleSfx(string semantic, bool warn = true)
	{
		List<string> possibleSfxs = GetPossibleSfxs(semantic, warn);
		List<string> list = new List<string>();
		foreach (string item in possibleSfxs)
		{
			if (!forbiddenCounters.ContainsKey(item))
			{
				list.Add(item);
			}
		}
		if (list.Count > 0)
		{
			return list[UnityEngine.Random.Range(0, list.Count)];
		}
		return string.Empty;
	}

	public static void InitPlay()
	{
		numLegsOnChunk.Clear();
	}

	public void SetSfxs(string texture, int meshIndex)
	{
		if (meshIndex == 0)
		{
			if (textureSemanticSfxs.TryGetValue(texture, out var value))
			{
				semanticSfxs = value;
			}
			else
			{
				semanticSfxs = defaultSemanticSfxs;
			}
			if (connectedBlockTypeSemanticSfxs.Count > 0)
			{
				semanticSfxs = connectedBlockTypeSemanticSfxs;
			}
			hasWheeSound = GetPossibleSfxs("Whee", warn: false).Count > 0;
			hasOuchSound = GetPossibleSfxs("Ouch", warn: false).Count > 0;
			hasOhSound = GetPossibleSfxs("Oh", warn: false).Count > 0;
		}
	}

	public override void OnCreate()
	{
		base.OnCreate();
		if (resetFeetPositionsOnCreate)
		{
			ResetFeetPositions();
			return;
		}
		for (int i = 0; i < legPairCount; i++)
		{
			PositionAnkle(i * 2);
			PositionAnkle(1 + i * 2);
		}
	}

	public virtual void FindFeet()
	{
		feet = new FootInfo[legPairCount * 2];
		for (int i = 0; i < legPairCount; i++)
		{
			string text = ((i != 0) ? "Back" : "Front");
			string text2 = ((legPairCount != 1) ? ("Foot Right " + text) : "Foot Right");
			string text3 = ((legPairCount != 1) ? ("Foot Left " + text) : "Foot Left");
			string text4 = ((!partNames.ContainsKey(text2)) ? text2 : partNames[text2]);
			string text5 = ((!partNames.ContainsKey(text3)) ? text3 : partNames[text3]);
			Transform transform = goT.Find(text4);
			Transform transform2 = goT.Find(text5);
			string text6 = "Bone ";
			Transform transform3 = goT.Find(text6 + text4);
			Transform transform4 = goT.Find(text6 + text5);
			if (transform != null)
			{
				feet[i * 2] = new FootInfo
				{
					go = transform.gameObject
				};
				if (transform3 != null)
				{
					feet[i * 2].bone = transform3.transform;
					feet[i * 2].boneYOffset = transform3.transform.localPosition.y + ankleYSeparation;
				}
				else
				{
					BWLog.Info("Could not find bone with name '" + text6 + text4 + "'");
				}
			}
			else
			{
				BWLog.Info("Could not find foot with name '" + text4 + "'");
			}
			if (transform2 != null)
			{
				feet[1 + i * 2] = new FootInfo
				{
					go = transform2.gameObject
				};
				if (transform4 != null)
				{
					feet[1 + i * 2].bone = transform4.transform;
					feet[1 + i * 2].boneYOffset = transform4.transform.localPosition.y + ankleYSeparation;
				}
				else
				{
					BWLog.Info("Could not find bone with name '" + text6 + text5 + "'");
				}
			}
			else
			{
				BWLog.Info("Could not find foot with name '" + text5 + "'");
			}
		}
	}

	protected virtual void PauseAnkles()
	{
		for (int i = 0; i < legPairCount; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				PositionAnkle(j + i * 2);
			}
		}
	}

	public override void Pause()
	{
		if (voxAudioSource != null)
		{
			voxAudioSource.Stop();
		}
		for (int i = 0; i < feet.Length; i++)
		{
			feet[i].go.name = feet[i].oldName;
		}
		if (!broken && !unmoving)
		{
			OrientFeetWithGround(!controllerWasActive || walkController.IsOnGround());
			PauseAnkles();
		}
		for (int j = 0; j < 2 * legPairCount; j++)
		{
			FootInfo footInfo = feet[j];
			Rigidbody rb = footInfo.rb;
			if (rb != null)
			{
				footInfo.pausedVelocity = rb.velocity;
				footInfo.pausedAngularVelocity = rb.angularVelocity;
				rb.isKinematic = true;
			}
		}
	}

	public override void Resume()
	{
		for (int i = 0; i < 2 * legPairCount; i++)
		{
			FootInfo footInfo = feet[i];
			Rigidbody rb = footInfo.rb;
			if (rb != null)
			{
				rb.isKinematic = false;
				rb.velocity = footInfo.pausedVelocity;
				rb.angularVelocity = footInfo.pausedAngularVelocity;
			}
		}
	}

	public override void Break(Vector3 chunkPos, Vector3 chunkVel, Vector3 chunkAngVel)
	{
		base.Break(chunkPos, chunkVel, chunkAngVel);
		for (int i = 0; i < 2 * legPairCount; i++)
		{
			FootInfo footInfo = feet[i];
			UnityEngine.Object.Destroy(footInfo.joint);
			Rigidbody rb = footInfo.rb;
			if (rb != null)
			{
				Block.AddExplosiveForce(rb, footInfo.go.transform.position, chunkPos, chunkVel, chunkAngVel);
			}
		}
		go.GetComponent<Collider>().enabled = true;
	}

	private void CreateVocalAudioSource()
	{
		if (voxAudioSource == null)
		{
			voxGameObject = new GameObject(go.name + " Vox Object");
			voxGameObject.transform.parent = goT;
			voxAudioSource = voxGameObject.AddComponent<AudioSource>();
			voxAudioSource.playOnAwake = false;
			voxAudioSource.loop = false;
			voxAudioSource.dopplerLevel = 0.5f;
			voxAudioSource.enabled = true;
			voxAudioSource.spatialBlend = 0.5f;
			voxGameObject.transform.localPosition = Vector3.zero;
			Sound.SetWorldAudioSourceParams(voxAudioSource);
		}
	}

	public override void Play()
	{
		base.Play();
		treatAsVehicleStatus = -1;
		lpFilterUpdateCounter = UnityEngine.Random.Range(1, 5);
		unmoving = false;
		legParameters = GetLegParameters();
		if (legParameters != null)
		{
			maxSurfaceWalkAngle = legParameters.maxSurfaceWalkAngle;
		}
		maxSurfaceWalkAngleDot = Mathf.Cos((float)Math.PI / 180f * maxSurfaceWalkAngle);
		CreateVocalAudioSource();
		ignoreRaycastGOs = null;
		forbiddenCounters.Clear();
		body = goT.parent.gameObject;
		keepCollider |= body.transform.childCount <= 1;
		if (!keepCollider)
		{
			go.GetComponent<Collider>().enabled = false;
		}
		hasMovedCM = false;
		hasChangedInertia = false;
		onGround = true;
		upright = true;
		StoreDefaultFootScale();
		UnVanishFeet();
		for (int i = 0; i < 2 * legPairCount; i++)
		{
			FootInfo footInfo = feet[i];
			if (footInfo == null)
			{
				BWLog.Info("foot was null");
			}
			footInfo.position = footInfo.go.transform.position;
			footInfo.oldName = footInfo.go.name;
			footInfo.go.name = go.name;
			footInfo.normal = Vector3.up;
		}
		stepTimer = 0f;
		vpe = Vector3.zero;
		vi = Vector3.zero;
		vd = Vector3.zero;
		wasBroken = false;
		startJumpCountdown = (int)Mathf.Round(1f / Blocksworld.fixedDeltaTime);
		walkController = new WalkController(this);
		controllerWasActive = false;
		modelMass = Bunch.GetModelMass(this);
		ignoreRotation = ChunkContainsJoint() && modelMass > 10f;
		GetCharacterSfxs();
		SetSfxs(GetTexture(), 0);
		if (legParameters != null)
		{
			walkController.SetDefaultWackiness(legParameters.wackiness);
			ohPlayProbability = legParameters.ohPlayProbability;
			wheePlayProbability = legParameters.wheePlayProbability;
			ouchPlayProbability = legParameters.ouchPlayProbability;
			walkController.DPadControl("L", 2f, 0.25f);
		}
		walkController.SetChunk();
		playCallouts = true;
		if (unmoving)
		{
			go.GetComponent<Collider>().enabled = true;
		}
		if (torqueMult != 1f)
		{
			walkController.SetAddedTorqueMultiplier = torqueMult;
		}
	}

	protected void StoreDefaultFootScale()
	{
		oldLocalScale = feet[0].go.transform.localScale;
	}

	public LegParameters GetLegParameters()
	{
		LegParameters component = go.GetComponent<LegParameters>();
		if (component == null)
		{
			BWLog.Info("No parameters found for legs");
		}
		return component;
	}

	public void GetSemanticSfxRule(ObjectSfxItem[] items, Dictionary<string, List<string>> dict)
	{
		foreach (ObjectSfxItem objectSfxItem in items)
		{
			string semanticName = objectSfxItem.semanticName;
			string sfxName = objectSfxItem.sfxName;
			if (!dict.TryGetValue(semanticName, out var value))
			{
				value = (dict[semanticName] = new List<string>());
			}
			canInterrupts[sfxName] = objectSfxItem.canInterrupt;
			value.Add(sfxName);
		}
	}

	public void GetCharacterSfxs()
	{
		ObjectSfxRules component = go.GetComponent<ObjectSfxRules>();
		if (component == null)
		{
			component = Blocksworld.blocksworldDataContainer.GetComponent<ObjectSfxRules>();
			if (component == null)
			{
				BWLog.Info("Could not find any object sfx rules " + BlockType());
			}
		}
		if (component != null)
		{
			connectedBlockTypeSemanticSfxs.Clear();
			for (int i = 0; i < component.textureRules.Length; i++)
			{
				ObjectTextureSfxRule objectTextureSfxRule = component.textureRules[i];
				string texture = objectTextureSfxRule.texture;
				string key = texture;
				Dictionary<string, List<string>> dictionary;
				if (!textureSemanticSfxs.ContainsKey(key))
				{
					dictionary = new Dictionary<string, List<string>>();
					textureSemanticSfxs[key] = dictionary;
				}
				else
				{
					dictionary = textureSemanticSfxs[key];
				}
				GetSemanticSfxRule(objectTextureSfxRule.items, dictionary);
			}
			HashSet<string> hashSet = new HashSet<string>();
			foreach (Block connection in connections)
			{
				hashSet.Add(connection.BlockType());
			}
			ObjectBlockTypeSfxRule[] connectedBlockTypeRules = component.connectedBlockTypeRules;
			foreach (ObjectBlockTypeSfxRule objectBlockTypeSfxRule in connectedBlockTypeRules)
			{
				string blockType = objectBlockTypeSfxRule.blockType;
				if (hashSet.Contains(blockType))
				{
					GetSemanticSfxRule(objectBlockTypeSfxRule.items, connectedBlockTypeSemanticSfxs);
				}
			}
		}
		GetSemanticSfxRule(component.defaultTextureRule.items, defaultSemanticSfxs);
	}

	public virtual void PlayLegs1()
	{
		if (!unmoving)
		{
			for (int i = 0; i < 2 * legPairCount; i++)
			{
				Util.UnparentTransformSafely(feet[i].go.transform);
			}
			legIndex = ((!numLegsOnChunk.ContainsKey(body.name)) ? 1 : (numLegsOnChunk[body.name] + 1));
			numLegsOnChunk[body.name] = legIndex;
			currentLeg = legIndex % 2;
		}
	}

	public virtual void PlayLegs2()
	{
		if (unmoving)
		{
			return;
		}
		for (int i = 0; i < 2 * legPairCount; i++)
		{
			FootInfo footInfo = feet[i];
			Rigidbody rigidbody = footInfo.rb;
			if (rigidbody == null)
			{
				rigidbody = (footInfo.rb = footInfo.go.AddComponent<Rigidbody>());
				rigidbody.mass = footWeight;
				if (Blocksworld.interpolateRigidBodies)
				{
					rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
				}
				footInfo.collider = footInfo.go.GetComponent<Collider>();
				footInfo.collider.enabled = true;
				footInfo.joint = CreateJoint(body, footInfo.go);
				footInfo.go.AddComponent<ForwardEvents>();
			}
			rigidbody.isKinematic = false;
		}
		body.GetComponent<Rigidbody>().mass = Mathf.Max(1f, body.GetComponent<Rigidbody>().mass - 1f);
		multiLegged = numLegsOnChunk[body.name] > 1;
	}

	protected virtual void ResetFeetPositions()
	{
		for (int i = 0; i < legPairCount; i++)
		{
			feet[i * 2].go.transform.position = goT.position + goT.rotation * new Vector3(xWidth, footOffsetY - ankleOffset, legPairOffsets[i]);
			feet[i * 2].go.transform.rotation = goT.rotation;
			feet[i * 2 + 1].go.transform.position = goT.position + goT.rotation * new Vector3(0f - xWidth, footOffsetY - ankleOffset, legPairOffsets[i]);
			feet[i * 2 + 1].go.transform.rotation = goT.rotation;
		}
		for (int j = 0; j < feet.Length; j++)
		{
			feet[j].position = feet[j].go.transform.position;
		}
		for (int k = 0; k < legPairCount; k++)
		{
			PositionAnkle(k * 2);
			PositionAnkle(1 + k * 2);
		}
	}

	private void MakeUnmoving()
	{
		for (int i = 0; i < 2 * legPairCount; i++)
		{
			if (feet == null)
			{
				continue;
			}
			FootInfo footInfo = feet[i];
			if (!(footInfo.go == null) && !(footInfo.collider == null))
			{
				footInfo.collider.enabled = false;
				if (footInfo.joint != null)
				{
					UnityEngine.Object.Destroy(footInfo.joint);
				}
				UnityEngine.Object.Destroy(footInfo.rb);
				UnityEngine.Object.Destroy(footInfo.go.GetComponent<ForwardEvents>());
				footInfo.go.transform.parent = goT;
				footInfo.go.name = footInfo.oldName;
			}
		}
		ResetFeetPositions();
		RestoreCollider();
		unmoving = true;
		if (chunk != null)
		{
			chunk.UpdateCenterOfMass();
		}
	}

	private void MakeMoving()
	{
		if (unmoving)
		{
			unmoving = false;
			PlayLegs1();
			PlayLegs2();
			ResetFeetPositions();
			if (chunk != null)
			{
				chunk.UpdateCenterOfMass();
			}
		}
	}

	public override void Stop(bool resetBlock = true)
	{
		base.Stop(resetBlock);
		if (voxAudioSource != null)
		{
			voxAudioSource.Stop();
		}
		UnVanishFeet();
		ignoreRaycastGOs = null;
		MakeUnmoving();
		keepCollider = false;
		body = null;
		wasBroken = false;
		controllerWasActive = false;
		groundGO = null;
	}

	public SpringJoint CreateJoint(GameObject parent, GameObject child)
	{
		SpringJoint springJoint = child.AddComponent<SpringJoint>();
		springJoint.enablePreprocessing = false;
		springJoint.connectedBody = parent.GetComponent<Rigidbody>();
		springJoint.anchor = Vector3.zero;
		springJoint.axis = body.transform.up;
		SetSpring(springJoint);
		return springJoint;
	}

	private void SetSpring(SpringJoint joint)
	{
		SetSpring(joint, 10f + 4f * modelMass, 0.5f);
	}

	public void SetSpring(SpringJoint joint, float spring, float damper)
	{
		float wackiness = walkController.GetWackiness();
		float spring2 = Mathf.Max(0.01f, 5f * wackiness * spring);
		float damper2 = Mathf.Max(0.1f, 5f * wackiness * damper);
		joint.spring = spring2;
		joint.damper = damper2;
	}

	private void InspectSoftLimitSpring(string name, SoftJointLimitSpring ls)
	{
		BWLog.Info(name + ": " + ls.damper + " " + ls.spring);
	}

	public TileResultCode FreezeRotation(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (broken || unmoving)
		{
			return TileResultCode.False;
		}
		Rigidbody component = body.GetComponent<Rigidbody>();
		component.freezeRotation = true;
		return TileResultCode.True;
	}

	public TileResultCode Translate(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (broken || unmoving)
		{
			return TileResultCode.True;
		}
		Vector3 dir = ((args.Length == 0) ? Vector3.forward : ((Vector3)args[0]));
		float num = ((args.Length <= 1) ? walkController.defaultMaxSpeed : ((float)args[1]));
		walkController.Translate(dir, eInfo.floatArg * num);
		return TileResultCode.True;
	}

	public TileResultCode Walk(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (broken || unmoving)
		{
			return TileResultCode.True;
		}
		float num = (float)args[0] * eInfo.floatArg;
		if ((num > 0f && IsWalking() != TileResultCode.True) || (num < 0f && IsBacking() != TileResultCode.True))
		{
			body.GetComponent<Rigidbody>().AddForce(num * goT.forward, ForceMode.Impulse);
		}
		return TileResultCode.True;
	}

	public TileResultCode Turn(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (broken || unmoving)
		{
			return TileResultCode.True;
		}
		float speed = (float)args[0] * eInfo.floatArg;
		walkController.Turn(speed);
		return TileResultCode.True;
	}

	public TileResultCode AvoidTag(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (broken || unmoving || vanished)
		{
			return TileResultCode.True;
		}
		string tagName = (string)args[0];
		float avoidDistance = ((args.Length <= 1) ? walkController.defaultAvoidDistance : ((float)args[1]));
		float maxSpeed = ((args.Length <= 2) ? walkController.defaultMaxSpeed : ((float)args[2]));
		walkController.AvoidTag(tagName, avoidDistance, maxSpeed, eInfo.floatArg);
		return TileResultCode.True;
	}

	public TileResultCode TurnTowardsTag(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (broken || unmoving || vanished)
		{
			return TileResultCode.True;
		}
		string tagName = (string)args[0];
		walkController.TurnTowardsTag(tagName);
		return TileResultCode.True;
	}

	public TileResultCode TurnTowardsTap(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (broken || unmoving || vanished)
		{
			return TileResultCode.True;
		}
		walkController.TurnTowardsTap();
		return TileResultCode.True;
	}

	public TileResultCode TurnAlongCam(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (broken || unmoving || vanished)
		{
			return TileResultCode.True;
		}
		walkController.TurnAlongCamera();
		return TileResultCode.True;
	}

	public TileResultCode Jump(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (!broken && !unmoving && !vanished)
		{
			float num = (float)args[0] * eInfo.floatArg;
			if (controllerWasActive || walkController.IsActive())
			{
				walkController.Jump(num);
			}
			else if (IsOnGround(onGroundHeight))
			{
				if (upright)
				{
					num *= 2f;
				}
				body.GetComponent<Rigidbody>().AddForce(num * Vector3.up, ForceMode.Impulse);
				jumpCountdown = startJumpCountdown;
			}
		}
		return TileResultCode.True;
	}

	public TileResultCode Idle(ScriptRowExecutionInfo eInfo, object[] args)
	{
		idle = true;
		return TileResultCode.True;
	}

	public TileResultCode Stand(ScriptRowExecutionInfo eInfo, object[] args)
	{
		MakeMoving();
		idle = true;
		return TileResultCode.True;
	}

	public TileResultCode Sit(ScriptRowExecutionInfo eInfo, object[] args)
	{
		idle = true;
		MakeUnmoving();
		return TileResultCode.True;
	}

	public TileResultCode DPadControl(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (walkController != null && !vanished)
		{
			string key = ((args.Length == 0) ? "L" : ((string)args[0]));
			float maxSpeed = ((args.Length <= 1) ? walkController.defaultDPadMaxSpeed : ((float)args[1]));
			float wackiness = ((args.Length <= 2) ? walkController.defaultWackiness : ((float)args[2]));
			Blocksworld.UI.Controls.EnableDPad(key, MoverDirectionMask.ALL);
			Blocksworld.worldSessionHadBlocksterMover = true;
			walkController.DPadControl(key, maxSpeed, wackiness);
		}
		return TileResultCode.True;
	}

	protected override void HandleTiltMover(float xTilt, float yTilt, float zTilt)
	{
	}

	public TileResultCode GotoTap(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (walkController != null && !vanished)
		{
			float maxSpeed = ((args.Length == 0) ? walkController.defaultMaxSpeed : ((float)args[0]));
			float wackiness = ((args.Length <= 1) ? walkController.defaultWackiness : ((float)args[1]));
			walkController.GotoTap(maxSpeed, wackiness);
		}
		return TileResultCode.True;
	}

	public TileResultCode GotoTag(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (walkController != null && !vanished)
		{
			string tagName = ((args.Length == 0) ? string.Empty : ((string)args[0]));
			float maxSpeed = ((args.Length <= 1) ? walkController.defaultMaxSpeed : ((float)args[1]));
			float wackiness = ((args.Length <= 2) ? walkController.defaultWackiness : ((float)args[2]));
			walkController.GotoTag(tagName, maxSpeed, wackiness, eInfo.floatArg);
		}
		return TileResultCode.True;
	}

	public TileResultCode ChaseTag(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (walkController != null && !vanished)
		{
			string tagName = ((args.Length == 0) ? string.Empty : ((string)args[0]));
			float maxSpeed = ((args.Length <= 1) ? walkController.defaultMaxSpeed : ((float)args[1]));
			float wackiness = ((args.Length <= 2) ? walkController.defaultWackiness : ((float)args[2]));
			walkController.GotoTag(tagName, maxSpeed, wackiness, eInfo.floatArg, slowDown: false);
		}
		return TileResultCode.True;
	}

	public TileResultCode IsWalkingSensor(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (!((float)args[0] >= 0f))
		{
			return IsBacking();
		}
		return IsWalking();
	}

	public TileResultCode IsWalking()
	{
		if (!broken && !unmoving && !(Vector3.Dot(body.GetComponent<Rigidbody>().velocity, goT.forward) < 0f))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsBacking()
	{
		if (!broken && !unmoving && !(Vector3.Dot(body.GetComponent<Rigidbody>().velocity, goT.forward) >= 0f))
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
		if (!broken && !unmoving && !(body.GetComponent<Rigidbody>().angularVelocity.y >= 0f))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsTurningRight()
	{
		if (!broken && !unmoving && !(body.GetComponent<Rigidbody>().angularVelocity.y <= 0f))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsJumping(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (!IsOnGround(onGroundHeight))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public bool IsOnGround(float onGroundHeight)
	{
		IgnoreRaycasts(value: true);
		RaycastHit hitInfo;
		bool flag = Physics.Raycast(goT.position, -Vector3.up, out hitInfo, onGroundHeight, raycastMask);
		if (flag && hitInfo.collider != null)
		{
			flag = !hitInfo.collider.isTrigger;
		}
		IgnoreRaycasts(value: false);
		return flag;
	}

	public bool GetThinksOnGround()
	{
		return onGround;
	}

	public override TileResultCode PlaySound(string sfxName, string location, string soundType, float volume, float pitch, bool durational = false, float timer = 0f)
	{
		if (durational && Sound.sfxEnabled && !Sound.BlockIsMuted(this))
		{
			if (timer == 0f)
			{
				voxAudioSource.Stop();
				voxAudioSource.clip = Sound.GetSfx(sfxName);
				voxAudioSource.Play();
			}
			return UpdateDurationalSoundSource(sfxName, timer);
		}
		return base.PlaySound(sfxName, location, soundType, volume, pitch, durational, timer);
	}

	private void PlayOhSound(float vol)
	{
		PlaySemanticSound("Oh", vol, ohPlayProbability);
	}

	private void PlayWheeSound(float vol)
	{
		PlaySemanticSound("Whee", vol, wheePlayProbability);
	}

	private void PlayOuchSound(float vol, bool always = false)
	{
		if (PlaySemanticSound("Ouch", vol, (!always) ? ouchPlayProbability : 1f))
		{
			speakOuchSoundTime = Time.time;
		}
	}

	private bool PlaySemanticSound(string name, float vol, float prob)
	{
		if (UnityEngine.Random.value < prob && !Sound.durationalSoundBlockIDs.Contains(go.GetInstanceID()))
		{
			string text = SampleSfx(name, warn: false);
			if (text.Length > 0 && Sound.sfxEnabled && !Sound.BlockIsMuted(this) && (!voxAudioSource.isPlaying || (canInterrupts.ContainsKey(text) && canInterrupts[text])))
			{
				foreach (string item in new List<string>(forbiddenCounters.Keys))
				{
					int num = forbiddenCounters[item] - 1;
					forbiddenCounters[item] = num;
					if (num <= 0)
					{
						forbiddenCounters.Remove(item);
					}
				}
				forbiddenCounters[text] = 1;
				voxAudioSource.Stop();
				voxAudioSource.clip = Sound.GetSfx(text);
				voxAudioSource.Play();
			}
			speakSoundTime = Time.time;
			return true;
		}
		return false;
	}

	public override void Mute()
	{
		base.Mute();
		if (voxAudioSource.isPlaying)
		{
			voxAudioSource.Stop();
		}
	}

	public void Collided(Vector3 relativeVelocity)
	{
		if (broken)
		{
			return;
		}
		noCollisionCounter = 0;
		if (playCallouts && hasOuchSound && Time.time - speakOuchSoundTime > 1f)
		{
			Vector3 vector = relativeVelocity;
			float magnitude = vector.magnitude;
			if (magnitude > 6f || broken)
			{
				float vol = 0.2f * (magnitude - 5f);
				PlayOuchSound(vol, always: true);
			}
		}
	}

	public void UpdateSounds()
	{
		if (!playCallouts || (!hasWheeSound && !hasOhSound && !hasOuchSound) || !(body != null))
		{
			return;
		}
		Rigidbody component = body.GetComponent<Rigidbody>();
		if (!(component != null))
		{
			return;
		}
		float num = (component.velocity - oldVel).magnitude / Blocksworld.fixedDeltaTime;
		oldVel = component.velocity;
		wheeness = wheeness * 0.95f + num * 0.05f;
		float time = Time.time;
		if (num > 400f && time - speakOuchSoundTime > 1f)
		{
			PlayOuchSound(1f);
			wheeness = 0f;
		}
		if (!footHitGround && time - speakSoundTime > 1.5f && wheeness > 25f)
		{
			float num2 = 0.5f * (1f - Mathf.Max(jumpCountdown, 0f) / (float)startJumpCountdown) + 0.5f;
			if (UnityEngine.Random.value < num2)
			{
				PlayWheeSound(1f);
				wheeness = 0f;
			}
		}
		float num3 = 0f;
		if (walkController != null)
		{
			num3 = Mathf.Sqrt(walkController.GetWantedSpeedSqr());
		}
		if (!unmoving && footHitGround && time - speakSoundTime > 3f)
		{
			float num4 = Mathf.Abs(component.velocity.magnitude - num3);
			float num5 = 1f / Mathf.Max(num3, 1f);
			num4 *= num5;
			float num6 = component.angularVelocity.magnitude * num5;
			if (num4 > 5f || num6 > 5f)
			{
				PlayOhSound(1f);
				speakSoundTime = time;
			}
		}
	}

	protected virtual void ReplaceCollider()
	{
		if (replaceWithCapsuleCollider)
		{
			if (capsuleColliderMaterial == null)
			{
				capsuleColliderMaterial = new PhysicMaterial();
				capsuleColliderMaterial.dynamicFriction = 0.3f;
				capsuleColliderMaterial.staticFriction = 0.3f;
				capsuleColliderMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
			}
			BoxCollider[] componentsInChildren = go.GetComponentsInChildren<BoxCollider>();
			BoxCollider[] array = componentsInChildren;
			foreach (BoxCollider obj in array)
			{
				UnityEngine.Object.Destroy(obj);
			}
			CapsuleCollider capsuleCollider = go.AddComponent<CapsuleCollider>();
			capsuleCollider.material = capsuleColliderMaterial;
			capsuleCollider.height = capsuleColliderHeight;
			capsuleCollider.center = capsuleColliderOffset;
			capsuleCollider.radius = capsuleColliderRadius;
			capsuleCollider.enabled = false;
			capsuleCollider.enabled = true;
			walkController.AddIgnoreCollider(capsuleCollider);
		}
	}

	protected virtual void RestoreCollider()
	{
	}

	protected virtual void PositionAnkles()
	{
		for (int i = 0; i < legPairCount; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				PositionAnkle(j + i * 2);
			}
		}
	}

	public override void Update()
	{
		base.Update();
		if (!broken && !unmoving && Blocksworld.CurrentState == State.Play)
		{
			UpdateBodyIfNecessary();
			upright = goT.up.y > 0.5f;
			PositionAnkles();
		}
	}

	public float Ipm(float v, float h)
	{
		float value = v * Mathf.Sqrt(h / 9.82f + v * v / 385.72958f);
		return Mathf.Clamp(value, 0f - maxStepLength, maxStepLength);
	}

	protected virtual Vector3 GetFeetCenter()
	{
		Vector3 zero = Vector3.zero;
		FootInfo[] array = feet;
		foreach (FootInfo footInfo in array)
		{
			zero += footInfo.go.transform.position / feet.Length;
		}
		return zero;
	}

	public void ChangeInertia()
	{
		Rigidbody rigidBody = GetRigidBody();
		if (!(rigidBody != null) || rigidBody.isKinematic || ChunkContainsOtherLegs())
		{
			return;
		}
		Vector3 inertiaTensor = rigidBody.inertiaTensor;
		float num = ((!(legParameters != null)) ? 1f : legParameters.inertiaThreshold);
		float num2 = ((!(legParameters != null)) ? 2f : legParameters.inertiaScaler);
		bool flag = false;
		for (int i = 0; i < 3; i++)
		{
			float num3 = inertiaTensor[i];
			if (num3 > num)
			{
				num3 = num + (num3 - num) / num2;
				inertiaTensor[i] = num3;
				flag = true;
			}
		}
		if (flag)
		{
			try
			{
				rigidBody.inertiaTensor = inertiaTensor;
			}
			catch
			{
				BWLog.Info("Unable to set inertia tensor, possibly due to the use of rigidbody constraints in the world.");
			}
		}
	}

	protected bool ChunkContainsOtherLegs()
	{
		bool result = false;
		foreach (Block block in chunk.blocks)
		{
			if (block is BlockAbstractLegs && block != this)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	private bool ChunkContainsJoint()
	{
		List<Block> blocks = chunk.blocks;
		for (int i = 0; i < blocks.Count; i++)
		{
			Block block = blocks[i];
			if (block.connectionTypes.Contains(2) || block.connectionTypes.Contains(-2))
			{
				return true;
			}
		}
		return false;
	}

	protected void MoveCenterOfMass()
	{
		Rigidbody rigidBody = GetRigidBody();
		if (rigidBody != null && !rigidBody.isKinematic && !ChunkContainsOtherLegs())
		{
			Vector3 feetCenter = GetFeetCenter();
			Vector3 vector = feetCenter + goT.up * moveCMOffsetFeetCenter - rigidBody.worldCenterOfMass;
			if (vector.magnitude > moveCMMaxDistance)
			{
				vector = vector.normalized * moveCMMaxDistance;
			}
			if (rigidBody.mass > 3f)
			{
				float num = rigidBody.mass - 2f;
				vector /= num;
			}
			rigidBody.centerOfMass += vector;
		}
	}

	protected void UpdateColliderAndCM(bool controllerActive)
	{
		bool flag = false;
		if (controllerActive && !controllerWasActive)
		{
			ReplaceCollider();
			flag = true;
		}
		else if (!controllerActive && controllerWasActive)
		{
			RestoreCollider();
			flag = true;
		}
		if (controllerActive)
		{
			if (moveCM && (!hasMovedCM || flag))
			{
				hasMovedCM = true;
			}
			if (!hasChangedInertia)
			{
				ChangeInertia();
				hasChangedInertia = true;
			}
		}
	}

	private void UpdateBodyIfNecessary()
	{
		if (body == null)
		{
			body = chunk.go;
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		noCollisionCounter++;
		if (broken && !wasBroken && playCallouts)
		{
			PlayOuchSound(1f);
		}
		wasBroken = broken;
		if (broken || vanished)
		{
			return;
		}
		if (Blocksworld.CurrentState == State.Play)
		{
			UpdateSounds();
		}
		if (unmoving || chunk.IsFrozen())
		{
			return;
		}
		Transform transform = goT;
		Vector3 up = transform.up;
		upright = up.y > 0.5f;
		UpdateBodyIfNecessary();
		bool flag = walkController != null && walkController.IsActive() && !didFix;
		OrientFeetWithGround(!flag || walkController.IsOnGround());
		if (!flag)
		{
			if (wacky)
			{
				wacky = false;
			}
			else
			{
				flag = true;
				walkController.Translate(Vector3.forward, 0f);
			}
		}
		UpdateColliderAndCM(flag);
		stepTimer += Blocksworld.fixedDeltaTime;
		Rigidbody component = body.GetComponent<Rigidbody>();
		float sqrMagnitude = Util.ProjectOntoPlane(component.velocity, Vector3.up).sqrMagnitude;
		float num = sqrMagnitude;
		float num2 = stepTime * stepTimeMultiplier;
		bool flag2 = false;
		if (flag)
		{
			num2 = stepTimeMultiplier * stepTime * (1f - walkController.GetAndResetHighSpeedFraction());
			buoyancyMultiplier = 2.5f;
			float sqrMagnitude2 = walkController.GetRigidBodyBelowVelocity().sqrMagnitude;
			num = 2f * Mathf.Max(sqrMagnitude - sqrMagnitude2, walkController.GetWantedSpeedSqr() - sqrMagnitude2);
			if (up.y < 0.955f && onGround)
			{
				flag2 = true;
			}
		}
		else
		{
			buoyancyMultiplier = 1f;
		}
		if (stepTimer > num2 && ((num > stepSpeedTrigger && !idle) || flag2))
		{
			groundGO = null;
			footHitGround = false;
			IgnoreRaycasts(value: true);
			Transform transform2 = body.transform;
			Vector3 position = transform2.position;
			Vector3 vector = position;
			if (multiLegged)
			{
				vector += Util.ProjectOntoPlane(transform.position - vector, up);
			}
			onGround = Physics.Raycast(transform.position + up * 0.5f, -Vector3.up, out var hitInfo, 5f, raycastMask) && !hitInfo.collider.isTrigger;
			if (upright && onGround)
			{
				Vector3 velocity = component.velocity;
				velocity.y = 0f;
				Vector3 lhs = Vector3.zero;
				Vector3 vector2 = Vector3.zero;
				float num3 = 1f;
				if (flag)
				{
					num3 = walkController.GetWackiness();
					vector2 = walkController.GetTotalForce();
					lhs = walkController.GetTotalCmTorque();
				}
				if (num3 != previousWackiness)
				{
					if (feet[0].joint != null)
					{
						for (int i = 0; i < 2 * legPairCount; i++)
						{
							SetSpring(feet[i].joint);
						}
					}
					previousWackiness = num3;
				}
				vi += velocity * Blocksworld.fixedDeltaTime;
				if (vi.sqrMagnitude > 1f)
				{
					vi.Normalize();
				}
				vd = (velocity - vpe) / Blocksworld.fixedDeltaTime;
				Vector3 vector3 = 1f * velocity + 2f * vi + 0.03f * vd;
				vpe = velocity;
				float num4 = Ipm(vector3.z, position.y - hitInfo.point.y);
				float num5 = Ipm(vector3.x, position.y - hitInfo.point.y);
				Vector3 vector4 = hitInfo.point - (footOffsetY - 0.125f) * Vector3.up;
				float num6 = 1f;
				if (flag)
				{
					float magnitude = velocity.magnitude;
					num6 = Mathf.Clamp(magnitude / 8f, 0.1f, 1f);
				}
				float num7 = num2 / (stepTime * stepTimeMultiplier);
				num5 *= num6 * stepLengthMultiplier * num7;
				num4 *= num6 * stepLengthMultiplier * num7;
				Vector3 vector5 = vector4 + footOffsetY * Vector3.up + num4 * Vector3.forward + num5 * Vector3.right;
				if (flag)
				{
					Vector3 vector6 = default(Vector3) + vector2 * 0.03f;
					float magnitude2 = lhs.magnitude;
					vector6 += Vector3.Cross(lhs, -Vector3.up).normalized * magnitude2 * 0.05f;
					vector6.y = 0f;
					float sqrMagnitude3 = vector6.sqrMagnitude;
					float num8 = 0.5f;
					if (sqrMagnitude3 > num8 * num8)
					{
						vector6.Normalize();
						vector6 *= num8;
					}
					vector5 += vector6 * stepLengthMultiplier;
				}
				groundGO = hitInfo.collider.gameObject;
				for (int j = 0; j < legPairIndices.Length; j++)
				{
					int[] array = legPairIndices[j];
					int num9 = array[currentLeg];
					int num10 = array[(currentLeg + 1) % 2];
					float num11 = legPairOffsets[j];
					Vector3 forward = transform.forward;
					forward.y = 0f;
					vector5 += forward * num11;
					FootInfo footInfo = feet[num9];
					FootInfo footInfo2 = feet[num10];
					footInfo.normal = hitInfo.normal;
					Vector3 vector7 = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f) * Vector3.right;
					Vector3 position2 = footInfo2.go.transform.position;
					int num12 = num9 % 2;
					position2 += (float)((num12 == 0) ? 1 : (-1)) * (0.75f - xWidth) * vector7;
					Vector3 vector8 = ((num12 == 0) ? 1 : (-1)) * vector7;
					Plane plane = new Plane(vector8, position2);
					if (!plane.GetSide(vector5))
					{
						vector5 -= plane.GetDistanceToPoint(vector5) * vector8;
					}
					if (Mathf.Abs(vector5.y - transform.position.y) < maxStepHeight && Vector3.Dot(footInfo.normal, Vector3.up) > maxSurfaceWalkAngleDot)
					{
						footHitGround = true;
						Vector3 position3 = vector5 - walkController.GetRigidBodyBelowVelocity() * Blocksworld.fixedDeltaTime * xWidth;
						footInfo.go.transform.position = position3;
						footInfo.position = position3;
						List<string> possibleSfxs = GetPossibleSfxs("Legs Step");
						if (possibleSfxs.Count > 0)
						{
							string sfxName = possibleSfxs[currentLeg % possibleSfxs.Count];
							PlayPositionedSound(sfxName, 0.2f);
						}
						stepTimer = 0f;
					}
				}
				currentLeg = (currentLeg + 1) % 2;
			}
			IgnoreRaycasts(value: false);
		}
		Vector3 vector9 = Vector3.zero;
		if (flag)
		{
			walkController.FixedUpdate();
			vector9 = walkController.GetRigidBodyBelowVelocity();
		}
		controllerWasActive = flag;
		if (lpFilterUpdateCounter % 5 == 0)
		{
			UpdateWithinWaterLPFilter();
			UpdateWithinWaterLPFilter(voxGameObject);
		}
		lpFilterUpdateCounter++;
		for (int k = 0; k < feet.Length; k++)
		{
			FootInfo footInfo3 = feet[k];
			Rigidbody rb = footInfo3.rb;
			if (jumpCountdown <= 0 && upright && stepTimer <= num2 && !idle && Mathf.Abs(footInfo3.position.y - goT.position.y) < maxStepHeight && !Util.IsNullVector3(footInfo3.position) && Vector3.Dot(footInfo3.normal, Vector3.up) > maxSurfaceWalkAngleDot)
			{
				footInfo3.go.transform.position = footInfo3.position;
				rb.velocity = Vector3.zero;
				if (flag)
				{
					footInfo3.position += vector9 * Blocksworld.fixedDeltaTime;
				}
				if (groundGO != null)
				{
					CollisionManager.ForwardCollisionEnter(go, groundGO, null);
				}
			}
			else
			{
				footInfo3.position = Util.nullVector3;
			}
			if (rb.IsSleeping())
			{
				rb.WakeUp();
			}
		}
		jumpCountdown--;
		idle = false;
	}

	public virtual void PositionAnkle(int i)
	{
		FootInfo footInfo = feet[i];
		if (footInfo.bone != null)
		{
			footInfo.bone.position = footInfo.go.transform.position - footInfo.go.transform.TransformDirection(Vector3.up) * footInfo.boneYOffset;
			footInfo.bone.rotation = footInfo.go.transform.rotation;
		}
	}

	public virtual void OrientFeetWithGround(bool v = true)
	{
		for (int i = 0; i < 2 * legPairCount; i++)
		{
			feet[i].go.transform.rotation = ((!upright || !v) ? goT.rotation : Quaternion.Euler(0f, goT.eulerAngles.y, 0f));
		}
	}

	public override List<GameObject> GetIgnoreRaycastGOs()
	{
		List<GameObject> list = base.GetIgnoreRaycastGOs();
		for (int i = 0; i < feet.Length; i++)
		{
			list.Add(feet[i].go);
		}
		return list;
	}

	public override void IgnoreRaycasts(bool value)
	{
		if (ignoreRaycastGOs == null)
		{
			ignoreRaycastGOs = new List<GameObject>();
			ignoreRaycastGOs.Add(go);
			for (int i = 0; i < 2 * legPairCount; i++)
			{
				ignoreRaycastGOs.Add(feet[i].go);
			}
			Vector3 position = goT.position;
			if (body != null)
			{
				ignoreRaycastGOs.Add(body);
				foreach (Block block in chunk.blocks)
				{
					if (block == this)
					{
						continue;
					}
					foreach (GameObject ignoreRaycastGO in block.GetIgnoreRaycastGOs())
					{
						Vector3 position2 = ignoreRaycastGO.transform.position;
						if ((position2 - position).magnitude < 2f)
						{
							ignoreRaycastGOs.Add(ignoreRaycastGO);
						}
					}
				}
			}
		}
		int layer = (int)((!value) ? goLayerAssignment : Layer.IgnoreRaycast);
		for (int j = 0; j < ignoreRaycastGOs.Count; j++)
		{
			GameObject gameObject = ignoreRaycastGOs[j];
			if (gameObject != null)
			{
				gameObject.layer = layer;
			}
		}
	}

	public override bool ScaleTo(Vector3 scale, bool recalculateCollider = true, bool forceRescale = false)
	{
		return true;
	}

	public override TileResultCode Vanish(ScriptRowExecutionInfo eInfo, object[] args)
	{
		TileResultCode tileResultCode = base.Vanish(eInfo, args);
		switch (tileResultCode)
		{
		case TileResultCode.True:
		{
			for (int j = 0; j < feet.Length; j++)
			{
				feet[j].go.GetComponent<Renderer>().enabled = false;
				feet[j].go.SetActive(value: false);
			}
			break;
		}
		case TileResultCode.Delayed:
		{
			for (int i = 0; i < feet.Length; i++)
			{
				feet[i].go.transform.localScale = goT.localScale;
			}
			break;
		}
		}
		return tileResultCode;
	}

	public TileResultCode WackyMode(ScriptRowExecutionInfo eInfo, object[] args)
	{
		wacky = true;
		return TileResultCode.True;
	}

	protected virtual void UnVanishFeet()
	{
		for (int i = 0; i < feet.Length; i++)
		{
			GameObject gameObject = feet[i].go;
			if (gameObject.GetComponent<Renderer>().sharedMaterial.shader != InvisibleShader)
			{
				gameObject.GetComponent<Renderer>().enabled = true;
				gameObject.SetActive(value: true);
			}
			gameObject.transform.localScale = oldLocalScale;
		}
	}

	public Rigidbody GetRigidBody()
	{
		UpdateBodyIfNecessary();
		if (body != null)
		{
			return body.GetComponent<Rigidbody>();
		}
		return null;
	}

	public virtual bool FeetPartOfGo()
	{
		return false;
	}

	public override void Appeared()
	{
		base.Appeared();
		if (FeetPartOfGo())
		{
			return;
		}
		ResetFeetPositions();
		for (int i = 0; i < feet.Length; i++)
		{
			FootInfo footInfo = feet[i];
			Rigidbody rb = footInfo.rb;
			if (rb != null && !rb.isKinematic)
			{
				rb.velocity = Vector3.zero;
			}
			if (footInfo.go.GetComponent<Renderer>().sharedMaterial.shader != GetInvisibleShader())
			{
				footInfo.go.GetComponent<Renderer>().enabled = true;
			}
			footInfo.go.GetComponent<Collider>().enabled = true;
		}
	}

	public Shader GetInvisibleShader()
	{
		invisibleShader = ((!(invisibleShader != null)) ? Shader.Find("Blocksworld/Invisible") : invisibleShader);
		return invisibleShader;
	}

	public override void Vanished()
	{
		base.Vanished();
		if (FeetPartOfGo())
		{
			return;
		}
		for (int i = 0; i < feet.Length; i++)
		{
			FootInfo footInfo = feet[i];
			Renderer component = footInfo.go.GetComponent<Renderer>();
			Collider component2 = footInfo.go.GetComponent<Collider>();
			if (component != null)
			{
				footInfo.go.GetComponent<Renderer>().enabled = false;
			}
			if (component2 != null)
			{
				component2.enabled = false;
			}
		}
	}

	public override void Teleported(bool resetAngle = false, bool resetVel = false, bool resetAngVel = false)
	{
		base.Teleported(resetAngle, resetVel, resetAngVel);
		if (FeetPartOfGo())
		{
			return;
		}
		ResetFeetPositions();
		for (int i = 0; i < feet.Length; i++)
		{
			if (!(resetVel || resetAngVel))
			{
				continue;
			}
			Rigidbody rb = feet[i].rb;
			if (rb != null)
			{
				if (resetVel)
				{
					rb.velocity = Vector3.zero;
				}
				if (resetAngVel)
				{
					rb.angularVelocity = Vector3.zero;
				}
			}
		}
	}

	public override void Destroy()
	{
		for (int i = 0; i < feet.Length; i++)
		{
			Mesh ankleMesh = feet[i].ankleMesh;
			if (ankleMesh != mesh)
			{
				UnityEngine.Object.Destroy(ankleMesh);
			}
			if (feet[i].go != null)
			{
				UnityEngine.Object.Destroy(feet[i].go);
			}
		}
		base.Destroy();
	}

	public override void RemoveBlockMaps()
	{
		if (feet != null)
		{
			for (int i = 0; i < feet.Length; i++)
			{
				if (feet[i].go != null)
				{
					BWSceneManager.RemoveChildBlockInstanceID(feet[i].go);
				}
			}
		}
		base.RemoveBlockMaps();
	}

	public override TileResultCode TextureTo(string texture, Vector3 normal, bool permanent, int meshIndex = 0, bool force = false)
	{
		TileResultCode tileResultCode = base.TextureTo(texture, normal, permanent, meshIndex, force);
		if (tileResultCode == TileResultCode.True && meshIndex == 0)
		{
			SetSfxs(texture, meshIndex);
		}
		return tileResultCode;
	}

	public override float GetCurrentMassChange()
	{
		if (FeetPartOfGo())
		{
			return 0f;
		}
		float num = 0f;
		if (feet != null)
		{
			FootInfo[] array = feet;
			foreach (FootInfo footInfo in array)
			{
				if (footInfo != null)
				{
					Rigidbody rb = footInfo.rb;
					if (rb != null)
					{
						num += rb.mass;
					}
				}
			}
		}
		return num;
	}

	public override bool CanChangeMass()
	{
		return true;
	}

	public override void BecameTreasure()
	{
		base.BecameTreasure();
		MakeUnmoving();
	}

	protected override void RestoreMeshColliderInfo()
	{
		base.RestoreMeshColliderInfo();
		Blocksworld.AddFixedUpdateCommand(new DelayedDelegateCommand(delegate
		{
			if (walkController != null)
			{
				walkController.ClearIgnoreColliders();
			}
		}, 1));
	}

	public override bool TreatAsVehicleLikeBlock()
	{
		return TreatAsVehicleLikeBlockWithStatus(ref treatAsVehicleStatus);
	}

	public override void ChunksAndJointsModified(Dictionary<Joint, Joint> oldToNew, Dictionary<Chunk, Chunk> oldToNewChunks, Dictionary<Chunk, Chunk> newToOldChunks)
	{
		if (!broken && !unmoving && !isTreasure && newToOldChunks.TryGetValue(chunk, out var value))
		{
			body = chunk.go;
			Rigidbody component = body.GetComponent<Rigidbody>();
			for (int i = 0; i < feet.Length; i++)
			{
				FootInfo footInfo = feet[i];
				Vector3 position = value.go.transform.TransformPoint(footInfo.joint.connectedAnchor);
				footInfo.joint.connectedBody = component;
				footInfo.joint.connectedAnchor = body.transform.InverseTransformPoint(position);
				SetSpring(footInfo.joint);
			}
			if (walkController != null)
			{
				walkController.SetChunk();
			}
		}
	}

	public override void Deactivate()
	{
		base.Deactivate();
		for (int i = 0; i < feet.Length; i++)
		{
			feet[i].go.SetActive(value: false);
		}
	}

	public void UpdateSpring()
	{
		modelMass = Bunch.GetModelMass(this);
		if (feet[0].joint != null)
		{
			for (int i = 0; i < 2 * legPairCount; i++)
			{
				SetSpring(feet[i].joint);
			}
		}
	}
}
