using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public abstract class BlockWalkable : Block
{
	public WalkControllerAnimated walkController;

	public bool controllerWasActive;

	public bool isHovering;

	private int noCollisionCounter;

	private float speakSoundTime;

	private float speakOuchSoundTime;

	protected Vector3 handPos = new Vector3(0.5f, -0.5f, 0.4f);

	protected float handsOutXMod = 0.7f;

	protected float handsOutYMod = -0.1f;

	public GameObject[] hands = new GameObject[2];

	public GameObject middle;

	public GameObject head;

	protected ConfigurableJoint middleJoint;

	protected Vector3 middleLocalPosition;

	public GameObject body;

	protected GameObject groundGO;

	public float modelMass = 1f;

	private bool wasBroken;

	public bool playCallouts;

	public FootInfo[] feet;

	public float[] legPairOffsets;

	public int[][] legPairIndices;

	public float ankleYSeparation;

	protected float stepSpeedTrigger = 0.4f;

	protected float stepTime = 0.125f;

	protected float footWeight = 0.5f;

	protected float ankleOffset = 0.125f;

	public float maxStepLength = 1f;

	public float maxStepHeight = 1f;

	private Vector3 oldVel = Vector3.zero;

	private float wheeness;

	private int lpFilterUpdateCounter;

	protected Vector3 oldLocalScale = Vector3.one;

	protected bool resetFeetPositionsOnCreate;

	public int legPairCount = 1;

	public int jumpCountdown;

	public int startJumpCountdown = 50;

	public float onGroundHeight = 1f;

	public bool ignoreRotation;

	protected bool idle;

	public bool upright;

	public bool unmoving;

	protected bool onGround = true;

	protected float maxSurfaceWalkAngle = 45f;

	protected float maxSurfaceWalkAngleDot = 0.7f;

	protected int treatAsVehicleStatus = -1;

	protected bool hasOuchSound;

	protected bool hasWheeSound;

	protected bool hasOhSound;

	protected float ouchPlayProbability = 1f;

	protected float wheePlayProbability = 0.7f;

	protected float ohPlayProbability = 0.4f;

	protected LegParameters legParameters;

	protected List<GameObject> ignoreRaycastGOs;

	protected bool moveCM;

	protected float moveCMOffsetFeetCenter = 1f;

	protected float moveCMMaxDistance = 1f;

	protected Vector3 capsuleColliderOffset = Vector3.zero;

	protected float capsuleColliderHeight = 1f;

	protected float capsuleColliderRadius = 0.5f;

	private static PhysicMaterial capsuleColliderMaterial;

	public Shader InvisibleShader;

	public static int raycastMask = 539;

	protected AudioSource voxAudioSource;

	protected GameObject voxGameObject;

	public Dictionary<string, Dictionary<string, List<string>>> textureSemanticSfxs = new Dictionary<string, Dictionary<string, List<string>>>();

	public Dictionary<string, List<string>> connectedBlockTypeSemanticSfxs = new Dictionary<string, List<string>>();

	public Dictionary<string, List<string>> semanticSfxs = new Dictionary<string, List<string>>();

	public Dictionary<string, bool> canInterrupts = new Dictionary<string, bool>();

	public Dictionary<string, int> forbiddenCounters = new Dictionary<string, int>();

	public Dictionary<string, List<string>> defaultSemanticSfxs = new Dictionary<string, List<string>>();

	private BoxColliderData origBoxColliderData;

	protected static Shader invisibleShader;

	private const float IPM_G = 9.82f;

	private const float IPM_VEL_FACTOR = 385.72958f;

	public BlockWalkable(List<List<Tile>> tiles, int legPairCount = 1, int[][] legPairIndices = null, float ankleYSeparation = 0f)
		: base(tiles)
	{
		InvisibleShader = Shader.Find("Blocksworld/Invisible");
		this.legPairCount = legPairCount;
		this.ankleYSeparation = ankleYSeparation;
		if (legPairIndices == null)
		{
			legPairIndices = new int[1][] { new int[2] { 0, 1 } };
		}
		this.legPairIndices = legPairIndices;
		unmoving = true;
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

	private void PlayOhSound(float vol)
	{
		PlaySemanticSound("Oh", vol, ohPlayProbability);
	}

	private void PlayWheeSound(float vol)
	{
		PlaySemanticSound("Whee", vol, wheePlayProbability);
	}

	public void PlayOuchSound(float vol, bool always = false)
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
		if (time - speakSoundTime > 1.5f && wheeness > 25f)
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
		if (!unmoving && time - speakSoundTime > 3f)
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

	public LegParameters GetLegParameters()
	{
		LegParameters component = go.GetComponent<LegParameters>();
		if (component == null)
		{
			BWLog.Info("No parameters found for legs");
		}
		return component;
	}

	public virtual GameObject GetHandAttach(int hand)
	{
		if (hand < 0 || hand > 1)
		{
			return null;
		}
		return hands[hand];
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
		body = goT.parent.gameObject;
		onGround = true;
		upright = true;
		walkController = new WalkControllerAnimated(this);
		controllerWasActive = false;
		modelMass = Bunch.GetModelMass(this);
		ignoreRotation = ChunkContainsJoint() && modelMass > 10f;
		walkController.SetChunk();
		legParameters = GetLegParameters();
		if (legParameters != null)
		{
			maxSurfaceWalkAngle = legParameters.maxSurfaceWalkAngle;
		}
		maxSurfaceWalkAngleDot = Mathf.Cos((float)Math.PI / 180f * maxSurfaceWalkAngle);
		CreateVocalAudioSource();
		if (legParameters != null)
		{
			ohPlayProbability = legParameters.ohPlayProbability;
			wheePlayProbability = legParameters.wheePlayProbability;
			ouchPlayProbability = legParameters.ouchPlayProbability;
		}
		if (!unmoving)
		{
			ReplaceColliderWithCapsule();
		}
		GetCharacterSfxs();
		SetSfxs(GetTexture(), 0);
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

	public override void Break(Vector3 chunkPos, Vector3 chunkVel, Vector3 chunkAngVel)
	{
		base.Break(chunkPos, chunkVel, chunkAngVel);
		if (!playCallouts)
		{
			PlayOuchSound(1f, always: true);
		}
		MakeUnmoving();
	}

	public override void Stop(bool resetBlock = true)
	{
		base.Stop(resetBlock);
		if (voxAudioSource != null)
		{
			voxAudioSource.Stop();
		}
		MakeUnmoving();
		body = null;
		controllerWasActive = false;
		isHovering = false;
		groundGO = null;
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

	protected bool ChunkContainsJoint()
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

	public virtual void GatherIgnoreColliders(HashSet<Collider> ignoreColliders)
	{
		for (int i = 0; i < 2 * legPairCount; i++)
		{
			if (feet[i].collider != null)
			{
				ignoreColliders.Add(feet[i].collider);
			}
		}
		if (!(body != null))
		{
			return;
		}
		Transform[] componentsInChildren = body.GetComponentsInChildren<Transform>();
		foreach (Transform transform in componentsInChildren)
		{
			Collider component = transform.gameObject.GetComponent<Collider>();
			if (component != null)
			{
				ignoreColliders.Add(component);
			}
		}
	}

	public static void InitPlay()
	{
	}

	protected virtual Vector3 GetFeetCenter()
	{
		Vector3 zero = Vector3.zero;
		FootInfo[] array = feet;
		foreach (FootInfo footInfo in array)
		{
			if (footInfo.go != null)
			{
				zero += footInfo.go.transform.position / feet.Length;
			}
		}
		return zero;
	}

	protected virtual void UpdateBodyIfNecessary()
	{
		if (chunk != null && body == null)
		{
			body = chunk.go;
			Vector3 centerOfMass = body.GetComponent<Rigidbody>().centerOfMass;
			centerOfMass.y -= 0.5f;
			body.GetComponent<Rigidbody>().centerOfMass = centerOfMass;
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

	public bool IsOnGround()
	{
		if (walkController != null)
		{
			return walkController.IsOnGround();
		}
		Collider component = goT.GetComponent<Collider>();
		Vector3 origin = component.bounds.center + Vector3.down * component.bounds.extents.y * 0.9f;
		bool flag = go.IsLayer(Layer.IgnoreRaycast);
		IgnoreRaycasts(value: true);
		RaycastHit hitInfo;
		bool flag2 = Physics.Raycast(origin, Vector3.down, out hitInfo, component.bounds.extents.y * 0.2f, raycastMask);
		if (flag2 && hitInfo.collider != null)
		{
			flag2 = !hitInfo.collider.isTrigger;
		}
		if (!flag)
		{
			IgnoreRaycasts(value: false);
		}
		return flag2;
	}

	public float NearGround(float maxDist = 1f)
	{
		if (walkController == null)
		{
			return -1f;
		}
		if (walkController.OnGroundHeight() <= maxDist)
		{
			return walkController.OnGroundHeight();
		}
		return -1f;
	}

	public bool OnMovingObject()
	{
		return walkController.onMovingObject;
	}

	public bool GetThinksOnGround()
	{
		return onGround;
	}

	private void MakeMoving()
	{
		if (unmoving)
		{
			ReplaceColliderWithCapsule();
			unmoving = false;
			if (chunk != null)
			{
				chunk.UpdateCenterOfMass();
			}
		}
	}

	protected virtual void MakeUnmoving()
	{
		RestoreOrigBoxCollider();
		unmoving = true;
		if (chunk != null)
		{
			chunk.UpdateCenterOfMass();
		}
	}

	protected void ReplaceColliderWithCapsule()
	{
		if (capsuleColliderMaterial == null)
		{
			capsuleColliderMaterial = new PhysicMaterial();
			capsuleColliderMaterial.dynamicFriction = 0.3f;
			capsuleColliderMaterial.staticFriction = 0.3f;
			capsuleColliderMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
		}
		BoxCollider component = go.GetComponent<BoxCollider>();
		if (component != null)
		{
			if (origBoxColliderData == null)
			{
				origBoxColliderData = new BoxColliderData(component);
			}
			UnityEngine.Object.Destroy(component);
		}
		CapsuleCollider component2 = go.GetComponent<CapsuleCollider>();
		if (component2 == null)
		{
			CapsuleCollider capsuleCollider = go.AddComponent<CapsuleCollider>();
			capsuleCollider.material = capsuleColliderMaterial;
			capsuleCollider.height = capsuleColliderHeight;
			capsuleCollider.center = capsuleColliderOffset;
			capsuleCollider.radius = capsuleColliderRadius;
			capsuleCollider.enabled = false;
			capsuleCollider.enabled = true;
			walkController.SetCapsuleCollider(capsuleCollider);
			walkController.AddIgnoreCollider(capsuleCollider);
		}
		else
		{
			component2.enabled = true;
		}
	}

	protected void RestoreOrigBoxCollider()
	{
		if (origBoxColliderData != null)
		{
			CapsuleCollider component = go.GetComponent<CapsuleCollider>();
			if (component != null)
			{
				UnityEngine.Object.Destroy(component);
			}
			BoxCollider boxCollider = go.GetComponent<BoxCollider>();
			if (boxCollider == null)
			{
				boxCollider = go.AddComponent<BoxCollider>();
			}
			boxCollider.center = origBoxColliderData.center;
			boxCollider.size = origBoxColliderData.size;
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
		if (!unmoving)
		{
			UpdateBodyIfNecessary();
			upright = goT.up.y > 0.5f;
			Rigidbody component = body.GetComponent<Rigidbody>();
			bool flag = walkController != null && walkController.IsActive() && !didFix;
			if (!flag)
			{
				flag = true;
				walkController.Translate(Vector3.forward, 0f);
			}
			Vector3 zero = Vector3.zero;
			if (flag)
			{
				walkController.FixedUpdate();
				zero = walkController.GetRigidBodyBelowVelocity();
			}
			controllerWasActive = flag;
			if (lpFilterUpdateCounter % 5 == 0)
			{
				UpdateWithinWaterLPFilter();
				UpdateWithinWaterLPFilter(voxGameObject);
			}
			lpFilterUpdateCounter++;
			jumpCountdown--;
			idle = false;
		}
	}

	public Shader GetInvisibleShader()
	{
		invisibleShader = ((!(invisibleShader != null)) ? Shader.Find("Blocksworld/Invisible") : invisibleShader);
		return invisibleShader;
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
		if (IsOnGround())
		{
			return TileResultCode.False;
		}
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
			float force = (float)args[0] * eInfo.floatArg;
			walkController.Jump(force);
		}
		return TileResultCode.True;
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

	public TileResultCode Idle(ScriptRowExecutionInfo eInfo, object[] args)
	{
		idle = true;
		return TileResultCode.True;
	}

	public TileResultCode DPadControl(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (walkController != null && !vanished)
		{
			BlockAnimatedCharacter blockAnimatedCharacter = this as BlockAnimatedCharacter;
			if (!blockAnimatedCharacter.stateHandler.IsImmobile() && !blockAnimatedCharacter.didFix)
			{
				string key = ((args.Length == 0) ? "L" : ((string)args[0]));
				float maxSpeed = ((args.Length <= 1) ? walkController.defaultDPadMaxSpeed : ((float)args[1]));
				Blocksworld.UI.Controls.EnableDPad(key, MoverDirectionMask.ALL);
				Blocksworld.worldSessionHadBlocksterMover = true;
				walkController.DPadControl(key, maxSpeed);
			}
		}
		return TileResultCode.True;
	}

	protected override void HandleTiltMover(float xTilt, float yTilt, float zTilt)
	{
		if (walkController != null && !vanished)
		{
			walkController.TiltMoverControl(new Vector2(xTilt, yTilt));
		}
	}

	public TileResultCode GotoTap(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (walkController != null && !vanished)
		{
			float maxSpeed = ((args.Length == 0) ? walkController.defaultMaxSpeed : ((float)args[0]));
			walkController.GotoTap(maxSpeed);
		}
		return TileResultCode.True;
	}

	public TileResultCode GotoTag(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (walkController == null || vanished)
		{
			return TileResultCode.True;
		}
		string tagName = ((args.Length == 0) ? string.Empty : ((string)args[0]));
		float maxSpeed = ((args.Length <= 1) ? walkController.defaultMaxSpeed : ((float)args[1]));
		if (walkController.GotoTag(tagName, maxSpeed))
		{
			return TileResultCode.Delayed;
		}
		return TileResultCode.True;
	}

	public TileResultCode ChaseTag(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (walkController != null && !vanished)
		{
			string tagName = ((args.Length == 0) ? string.Empty : ((string)args[0]));
			float maxSpeed = ((args.Length <= 1) ? walkController.defaultMaxSpeed : ((float)args[1]));
			walkController.ChaseTag(tagName, maxSpeed);
		}
		return TileResultCode.True;
	}

	public float Ipm(float v, float h)
	{
		float value = v * Mathf.Sqrt(h / 9.82f + v * v / 385.72958f);
		return Mathf.Clamp(value, 0f - maxStepLength, maxStepLength);
	}
}
