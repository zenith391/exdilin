using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class BlocksworldCamera
{
	private enum CameraPositionMode
	{
		DEFAULT,
		DEFAULT_2D,
		LOOK_TOWARD,
		LOOK_TOWARD_TAG,
		MOVE_TO
	}

	private enum CameraTargetMode
	{
		DEFAULT,
		LOOK_AT
	}

	private class CameraFollowCommand
	{
		public Block block;

		public CameraPositionMode positionMode;

		public object[] args;

		public CameraFollowCommand(Block block, CameraPositionMode positionMode, object[] args)
		{
			this.block = block;
			this.positionMode = positionMode;
			this.args = args;
		}
	}

	private struct HeadgearInfo
	{
		public Block block;

		public Vector3 localPos;

		public Quaternion localRot;

		public Transform parent;
	}

	private List<CameraFollowCommand> camFollowCommands = new List<CameraFollowCommand>();

	private List<CameraFollowCommand> prevCamFollowCommands = new List<CameraFollowCommand>();

	private BuildCameraWatchdog buildCameraWatchdog = new BuildCameraWatchdog();

	private bool forceCommandExecution;

	private HashSet<Block> singletonBlocks = new HashSet<Block>();

	private HashSet<string> tryKeepInViewTags = new HashSet<string>();

	private Dictionary<Block, float> cameraLookTowardAngles = new Dictionary<Block, float>();

	private const float MAX_FORCE_DIRECTION_HINT = 50f;

	private const float MAX_TARGET_POS_CHANGE = 2f;

	private const float MIN_CAMERA_CORRECTION_DISTANCE = 3f;

	private const float MANUAL_CAMERA_FRACTION_THRESHOLD = 0.1f;

	private const float DEFAULT_MANUAL_CAMERA_SMOOTHNESS = 0.8f;

	private const float MIN_MANUAL_CAMERA_SMOOTHNESS = 0.001f;

	private const float MAX_MANUAL_CAMERA_SMOOTHNESS = 0.999f;

	private float screenTiltAngle;

	private bool screenTiltAngleSet;

	private Quaternion screenTiltBaseRotation;

	private Quaternion screenTiltRotation;

	private Quaternion camTiltBaseRotation;

	private bool screenTiltRotationSet;

	private bool screenTiltRotationTracking;

	private static Vector3 camVelocity = Vector3.zero;

	private float multiFollowTargetDistanceFactor = 1.2f;

	private float targetDistanceMultiplier = 1f;

	private float resetTargetDistanceMultiplierFactor = 1f;

	private Vector3 currentCameraOffsetVec = Vector3.forward * 15f;

	private Quaternion mode2DRotation;

	private Vector3 lookAtOffset = Vector3.zero;

	private Vector3 moveToOffset = Vector3.zero;

	private Vector3 immediateOffset = Vector3.zero;

	private float targetCameraAngle = 70f;

	private float verticalDistanceOffsetFactor = 1f;

	private float targetFollowDistanceMultiplier = 1f;

	private float _manualCameraDistance = 15f;

	private float _manualCameraHeight = 15f;

	private float _manualCameraAngle = 70f;

	public Quaternion lightRotation = Quaternion.Euler(40f, 20f, 0f);

	private bool broken;

	private CameraPositionMode positionMode;

	private CameraTargetMode targetMode;

	private CameraPositionMode oldPositionMode;

	private Vector3 moveToPos = Vector3.zero;

	private float moveToPosAlpha = 0.99f;

	private Vector3 lookAtPos = Vector3.zero;

	private float lookAtAlpha = 0.99f;

	private Vector3 panOffset = Vector3.zero;

	private Vector3 zoomOffset = Vector3.zero;

	private Vector2 orbitPosDiff = Vector2.zero;

	private Vector3 filteredForceDirectionHint = Vector3.zero;

	private Vector3 forceDirectionHint = Vector3.zero;

	private Bunch targetBunch;

	private Block targetBlock;

	private Bunch previousTargetBunch;

	private Block previousTargetBlock;

	private int followedModels;

	private bool followingVehicle;

	private Dictionary<int, Bounds> modelSizes = new Dictionary<int, Bounds>();

	private Dictionary<int, Block> modelBlocks = new Dictionary<int, Block>();

	private Dictionary<int, ChunkFollowInfo> targetChunkInfos = new Dictionary<int, ChunkFollowInfo>();

	private HashSet<Chunk> targetChunks = new HashSet<Chunk>();

	private HashSet<Block> followedBlocks = new HashSet<Block>();

	private HashSet<Block> allFollowedBlocks = new HashSet<Block>();

	private Vector3 targetPos;

	private Vector3 oldTargetPos;

	private Vector3 filteredTargetVel;

	private Vector3 storedOrbitPos;

	private float distance = 20f;

	private float lastDistance = 20f;

	private Vector3 targetPosFiltered;

	private Vector3 camPosFiltered;

	private bool cameraStill;

	private float manualCameraFraction;

	private float camInitSlowdown = 1f;

	private float currentManualCameraDistance = 15f;

	private float currentManualCameraAngle = 70f;

	private Vector3 storedTargetPos;

	private Vector3 storedCameraPos;

	private Quaternion storedCameraRot;

	private float storedManualCameraDist = 15f;

	private float storedDistance;

	private float[] chunkSqrSpeeds;

	private bool autoFollowDisabled;

	private float cameraVelResponsiveness = 1f;

	private float cameraFollowAlpha = 0.985f;

	private float velDistanceMultiplier = 0.3f;

	private Vector3 blockDirectionFactor = Vector3.zero;

	private Color lightTint = Color.white;

	private int prevHitDiff;

	private float moveDist;

	private float smoothedMoveDist;

	private Vector3 lastCameraPos = Vector3.zero;

	private float defaultFoV = 22.5f;

	public float speedFoV = 22.5f;

	public float minSpeedFoV = 22.5f;

	public float maxSpeedFoV = 35f;

	public float currentSpeedFoV;

	public float desiredSpeedFoV;

	private Block desiredFirstPersonBlock;

	public Block firstPersonBlock;

	public BlockCharacter firstPersonCharacter;

	public BlockAnimatedCharacter firstPersonAnimatedCharacter;

	public GameObject firstPersonHead;

	private Vector3 firstPersonPos;

	private Quaternion firstPersonLookAngle;

	public Quaternion firstPersonSmoothForward;

	private Vector3 firstPersonBlockLastForward;

	public Vector2 firstPersonDpad;

	public float firstPersonRotation;

	public Vector3 firstPersonDeadZone;

	public float firstPersonTurnScale = 1f;

	public float firstPersonTorque = 0.5f;

	public float fpcTilt;

	protected float actualFpcTilt;

	protected Quaternion aimAdjustOffset = Quaternion.identity;

	private float maxAimAdjustRange = 50f;

	private float minAimAdjustRange = 10f;

	public int firstPersonMode;

	public Vector3 firstPersonLookOffset = Vector3.zero;

	private Vector3 firstPersonLook = Vector3.zero;

	public bool firstPersonLookXFlip;

	public bool firstPersonLookYFlip;

	private GameObject reticleHolder;

	private int hudReticle = -1;

	public Renderer currentReticle;

	private List<GameObject> hudDisplayObjects;

	private List<HeadgearInfo> firstPersonHeadgear = new List<HeadgearInfo>();

	private const float CHUNK_SPEED_ESTIMATION_ALPHA = 0.95f;

	private static Vector3[] possibleForward = new Vector3[2]
	{
		-Vector3.forward,
		Vector3.up
	};

	public float manualCameraDistance
	{
		get
		{
			return _manualCameraDistance;
		}
		set
		{
			_manualCameraDistance = value;
		}
	}

	public float manualCameraHeight
	{
		get
		{
			return _manualCameraHeight;
		}
		set
		{
			_manualCameraHeight = value;
		}
	}

	public float manualCameraAngle
	{
		get
		{
			return _manualCameraAngle;
		}
		set
		{
			_manualCameraAngle = value;
		}
	}

	public void Init()
	{
		ViewportWatchdog.AddListener(ViewportSizeDidChange);
	}

	public void Play()
	{
		if (null == reticleHolder)
		{
			reticleHolder = new GameObject();
			reticleHolder.name = "Reticle Holder";
			reticleHolder.layer = LayerMask.NameToLayer("UI");
			SetReticleParent();
		}
		BlocksworldComponentData componentData = Blocksworld.componentData;
		if (null != componentData)
		{
			maxSpeedFoV = componentData.maxSpeedFoV;
			if (hudDisplayObjects == null)
			{
				hudDisplayObjects = new List<GameObject>();
				Texture[] hudTextures = componentData.hudTextures;
				foreach (Texture texture in hudTextures)
				{
					GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
					Material material = gameObject.GetComponent<Renderer>().material;
					if (null != material)
					{
						material.shader = Shader.Find("Blocksworld/Particles/Additive");
						material.SetTexture("_MainTex", texture);
					}
					gameObject.transform.SetParent(reticleHolder.transform);
					gameObject.transform.localPosition = Vector3.zero;
					gameObject.transform.localRotation = Quaternion.identity;
					gameObject.transform.localScale = 0.1f * Vector3.one;
					gameObject.layer = reticleHolder.layer;
					hudDisplayObjects.Add(gameObject);
				}
			}
			maxAimAdjustRange = componentData.aimAdjustMax;
			minAimAdjustRange = componentData.aimAdjustMin;
			firstPersonDeadZone = componentData.firstPersonDeadZone / 100f;
			firstPersonTurnScale = componentData.firstPersonTurnPower;
			firstPersonTorque = componentData.firstPersonTorque;
			firstPersonLookXFlip = componentData.FPCLookXFlip;
			firstPersonLookYFlip = componentData.FPCLookYFlip;
		}
		firstPersonMode = 0;
		foreach (GameObject hudDisplayObject in hudDisplayObjects)
		{
			hudDisplayObject.GetComponent<Renderer>().enabled = false;
		}
		reticleHolder.SetActive(value: false);
		tryKeepInViewTags.Clear();
		moveToOffset = Vector3.zero;
		lookAtOffset = Vector3.zero;
		panOffset = Vector3.zero;
		zoomOffset = Vector3.zero;
		orbitPosDiff = Vector2.zero;
		positionMode = CameraPositionMode.DEFAULT;
		targetMode = CameraTargetMode.DEFAULT;
		currentCameraOffsetVec = Vector3.one;
		previousTargetBunch = targetBunch;
		previousTargetBlock = targetBlock;
		Unfollow();
		UpdateChunkSpeeds();
		UpdateTargetPos();
		filteredTargetVel = Vector3.zero;
		oldTargetPos = targetPos;
		screenTiltRotationSet = (screenTiltRotationTracking = false);
		currentManualCameraAngle = manualCameraAngle;
		currentManualCameraDistance = manualCameraDistance;
		camPosFiltered = Blocksworld.cameraTransform.position;
		targetPosFiltered = camPosFiltered + Blocksworld.cameraTransform.forward * distance;
		targetPos = targetPosFiltered;
		manualCameraFraction = 0f;
		camInitSlowdown = 1f;
		autoFollowDisabled = BW.isUnityEditor && Options.DisableAutoFollow;
		ResetCameraFollowParameters();
		forceDirectionHint = Vector3.zero;
		filteredForceDirectionHint = Vector3.zero;
		mode2DRotation = Blocksworld.cameraTransform.rotation;
		singletonBlocks.Clear();
		camFollowCommands.Clear();
		prevCamFollowCommands.Clear();
		lastCameraPos = Blocksworld.cameraTransform.position;
		moveDist = 0f;
		smoothedMoveDist = 0f;
		firstPersonHeadgear.Clear();
		aimAdjustOffset = Quaternion.identity;
	}

	public void Stop()
	{
		if (firstPersonBlock != null)
		{
			firstPersonBlock.SetFPCGearVisible(visible: true);
			foreach (HeadgearInfo item in firstPersonHeadgear)
			{
				item.block.EnableCollider(value: true);
			}
			firstPersonHeadgear.Clear();
			firstPersonBlock = null;
			firstPersonCharacter = null;
			firstPersonAnimatedCharacter = null;
			firstPersonHead = null;
			actualFpcTilt = (fpcTilt = 0f);
		}
		positionMode = CameraPositionMode.DEFAULT;
		targetMode = CameraTargetMode.DEFAULT;
		tryKeepInViewTags.Clear();
		screenTiltRotationSet = (screenTiltRotationTracking = false);
		modelSizes.Clear();
		modelBlocks.Clear();
		Unfollow();
		if (previousTargetBunch != null)
		{
			Follow(previousTargetBunch);
		}
		else if (previousTargetBlock != null)
		{
			Follow(previousTargetBlock);
		}
		ResetCameraFollowParameters();
		camFollowCommands.Clear();
		prevCamFollowCommands.Clear();
		buildCameraWatchdog.Reset();
		singletonBlocks.Clear();
		currentSpeedFoV = 0f;
		desiredSpeedFoV = 0f;
	}

	public void SetReticleParent(Transform parentTransform = null)
	{
		if (parentTransform == null)
		{
			parentTransform = Blocksworld.cameraTransform;
		}
		reticleHolder.transform.SetParent(parentTransform);
		reticleHolder.transform.localPosition = 0.5f * Vector3.forward;
		reticleHolder.transform.localRotation = Quaternion.identity;
	}

	public void SetReticleCameraEyePosition(float eyePositionX)
	{
		if (reticleHolder != null)
		{
			reticleHolder.transform.localPosition = eyePositionX * Vector3.left + 0.5f * Vector3.forward;
		}
	}

	private void ViewportSizeDidChange()
	{
		ResetProjectionMatrix();
	}

	private void ResetProjectionMatrix()
	{
		Camera mainCamera = Blocksworld.mainCamera;
		mainCamera.aspect = NormalizedScreen.aspectRatio;
		Matrix4x4 projectionMatrix = mainCamera.projectionMatrix;
		projectionMatrix.m11 = 1f / Mathf.Tan((float)Math.PI * defaultFoV / 180f);
		projectionMatrix.m00 = projectionMatrix.m11 / mainCamera.aspect;
		mainCamera.projectionMatrix = projectionMatrix;
	}

	public void UpdateChunkSpeeds()
	{
		chunkSqrSpeeds = new float[Blocksworld.chunks.Count];
	}

	public void Store()
	{
		storedCameraPos = Blocksworld.cameraTransform.position;
		storedCameraRot = Blocksworld.cameraTransform.rotation;
		storedTargetPos = targetPos;
		storedDistance = distance;
		storedManualCameraDist = manualCameraDistance;
	}

	public void Restore()
	{
		Blocksworld.cameraTransform.position = storedCameraPos;
		Blocksworld.cameraTransform.rotation = storedCameraRot;
		targetPos = storedTargetPos;
		distance = (lastDistance = storedDistance);
		manualCameraDistance = storedManualCameraDist;
	}

	public void StoreOrbitPos()
	{
		storedOrbitPos = targetPos;
	}

	public void RestoreOrbitPos()
	{
		targetPos = storedOrbitPos;
	}

	public void Reset()
	{
		oldPositionMode = positionMode;
		positionMode = CameraPositionMode.DEFAULT;
		targetMode = CameraTargetMode.DEFAULT;
	}

	public void Follow(Bunch bunch)
	{
		targetBunch = bunch;
		UpdateOrbitDistance(useMaxDist: true, (bunch != null) ? (20f + Util.MaxComponent(Util.ComputeBounds(bunch.blocks).size)) : 20f);
		UpdateTargetPos();
	}

	public void Follow(Block block)
	{
		Follow(block, auto: false);
	}

	public void Follow(Block block, bool auto)
	{
		if (Blocksworld.CurrentState == State.Play)
		{
			if (!auto)
			{
				bool flag = false;
				foreach (KeyValuePair<int, ChunkFollowInfo> targetChunkInfo in targetChunkInfos)
				{
					ChunkFollowInfo chunkFollowInfo = targetChunkInfos[targetChunkInfo.Key];
					if (chunkFollowInfo.auto)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					Unfollow();
				}
			}
			Chunk chunk = block.chunk;
			if (chunk.go != null)
			{
				int instanceID = chunk.go.GetInstanceID();
				if (!targetChunkInfos.ContainsKey(instanceID))
				{
					targetChunkInfos[instanceID] = new ChunkFollowInfo(chunk, auto);
					targetChunks.Add(chunk);
				}
				List<Block> list;
				if (singletonBlocks.Contains(block))
				{
					list = new List<Block> { block };
				}
				else
				{
					block.UpdateConnectedCache();
					list = Block.connectedCache[block];
				}
				if (list != null)
				{
					if (allFollowedBlocks.Count == 0 || !allFollowedBlocks.Contains(block))
					{
						List<Block> list2 = new List<Block>();
						for (int i = 0; i < list.Count; i++)
						{
							Block item = list[i];
							if (list.Count <= 1 || !singletonBlocks.Contains(item))
							{
								list2.Add(item);
							}
						}
						if (list2.Count == 0)
						{
							list2.Add(list[0]);
						}
						modelSizes[followedModels] = Util.ComputeBoundsWithSize(list2);
						modelBlocks[followedModels] = block;
						followedModels++;
					}
					for (int j = 0; j < list.Count; j++)
					{
						Block block2 = list[j];
						if (!block2.broken && !block2.IsRuntimeInvisible())
						{
							if (list.Count > 1 && singletonBlocks.Contains(block2))
							{
								continue;
							}
							Chunk chunk2 = block2.chunk;
							GameObject go = chunk2.go;
							if (go != null)
							{
								int instanceID2 = go.GetInstanceID();
								if (!targetChunkInfos.ContainsKey(instanceID2))
								{
									targetChunkInfos[instanceID2] = new ChunkFollowInfo(chunk2, auto);
									targetChunks.Add(chunk2);
								}
							}
							if (block2.TreatAsVehicleLikeBlock())
							{
								followingVehicle = true;
							}
						}
						allFollowedBlocks.Add(block2);
					}
				}
			}
		}
		else
		{
			targetBlock = block;
			UpdateOrbitDistance(useMaxDist: true, (block != null) ? (20f + Util.MaxComponent(block.size)) : 20f);
		}
		followedBlocks.Add(block);
	}

	public void ExpandWorldBounds(ITBox b)
	{
		if (Tutorial.state != TutorialState.None)
		{
			return;
		}
		Vector3 position = b.GetPosition();
		if ((Blocksworld.cameraTransform.position - position).magnitude < 200f)
		{
			Bounds worldBounds = buildCameraWatchdog.GetWorldBounds();
			Vector3 size = worldBounds.size;
			Bounds bounds = new Bounds(position, b.GetScale());
			worldBounds.Encapsulate(bounds);
			if ((worldBounds.size - size).magnitude < 500f)
			{
				buildCameraWatchdog.EncapsulateWorldBounds(bounds);
			}
		}
	}

	public Vector3 GetTargetPosition()
	{
		return targetPos;
	}

	public void SetCameraPosition(Vector3 pos)
	{
		Blocksworld.cameraTransform.position = pos;
	}

	public void SetTargetPosition(Vector3 pos)
	{
		targetPos = pos;
	}

	public void SetTargetDistance(float d)
	{
		distance = d;
		lastDistance = d;
		manualCameraDistance = d;
	}

	public void SetCameraStill(bool still)
	{
		cameraStill = still;
	}

	public void KnockCameraOver()
	{
		broken = true;
	}

	private void ResetCameraFollowParameters()
	{
		cameraVelResponsiveness = 1f;
		cameraFollowAlpha = 0.985f;
		velDistanceMultiplier = 0.3f;
		blockDirectionFactor = Vector3.zero;
	}

	private bool GameCameraDisabled()
	{
		if (BW.isUnityEditor)
		{
			return Options.DisableGameCamera;
		}
		return false;
	}

	private void Execute(CameraFollowCommand c, bool callFollow = true)
	{
		if (GameCameraDisabled())
		{
			return;
		}
		object[] args = c.args;
		if (c.positionMode == CameraPositionMode.DEFAULT)
		{
			cameraVelResponsiveness = ((args.Length == 0) ? 1f : ((float)args[0]));
			cameraFollowAlpha = ((args.Length <= 1) ? 0.95f : ((float)args[1]));
			velDistanceMultiplier = ((args.Length <= 2) ? 0.3f : ((float)args[2]));
			blockDirectionFactor = ((args.Length <= 3) ? Vector3.zero : ((Vector3)args[3]));
		}
		else if (c.positionMode == CameraPositionMode.DEFAULT_2D)
		{
			cameraVelResponsiveness = ((args.Length == 0) ? 1f : ((float)args[0]));
			cameraFollowAlpha = ((args.Length <= 1) ? 0.985f : ((float)args[1]));
			positionMode = CameraPositionMode.DEFAULT_2D;
			if (oldPositionMode != CameraPositionMode.DEFAULT_2D)
			{
				mode2DRotation = Blocksworld.cameraTransform.rotation;
			}
		}
		else if (c.positionMode == CameraPositionMode.LOOK_TOWARD)
		{
			cameraVelResponsiveness = Util.GetFloatArg(args, 0, 1f);
			cameraFollowAlpha = Util.GetFloatArg(args, 1, 0.95f);
			cameraLookTowardAngles[c.block] = Util.GetFloatArg(args, 2, 0f);
			positionMode = CameraPositionMode.LOOK_TOWARD;
		}
		else if (c.positionMode == CameraPositionMode.LOOK_TOWARD_TAG)
		{
			tryKeepInViewTags.Add(Util.GetStringArg(args, 0, string.Empty));
			cameraVelResponsiveness = Util.GetFloatArg(args, 1, 1f);
			cameraFollowAlpha = Util.GetFloatArg(args, 2, 0.95f);
			positionMode = CameraPositionMode.LOOK_TOWARD_TAG;
		}
		if (callFollow)
		{
			Follow(c.block, auto: false);
		}
	}

	public void CameraFollow(Block block, object[] args)
	{
		if (!GameCameraDisabled())
		{
			camFollowCommands.Add(new CameraFollowCommand(block, CameraPositionMode.DEFAULT, args));
		}
	}

	public void CameraFollow2D(Block block, object[] args)
	{
		if (!GameCameraDisabled())
		{
			camFollowCommands.Add(new CameraFollowCommand(block, CameraPositionMode.DEFAULT_2D, args));
		}
	}

	public void CameraFollowLookToward(Block block, object[] args)
	{
		if (!GameCameraDisabled())
		{
			camFollowCommands.Add(new CameraFollowCommand(block, CameraPositionMode.LOOK_TOWARD, args));
		}
	}

	public void CameraFollowLookTowardTag(Block block, object[] args)
	{
		if (!GameCameraDisabled())
		{
			camFollowCommands.Add(new CameraFollowCommand(block, CameraPositionMode.LOOK_TOWARD_TAG, args));
		}
	}

	public void CameraFollowThirdPersonPlatform(Block block, object[] args)
	{
	}

	public void CameraMoveTo(Block block)
	{
		CameraMoveTo(block, 0.95f);
	}

	public void CameraToNamedPose(string poseName, float moveAlpha = 0.985f, float aimAlpha = 0.985f, float directionDistance = 15f, bool moveOnly = false)
	{
		if (!GameCameraDisabled() && Blocksworld.cameraPosesMap.TryGetValue(poseName, out var value))
		{
			moveToPos = value.position;
			positionMode = CameraPositionMode.MOVE_TO;
			moveToPosAlpha = moveAlpha;
			if (!moveOnly)
			{
				lookAtPos = moveToPos + value.direction * directionDistance;
				targetMode = CameraTargetMode.LOOK_AT;
				lookAtAlpha = aimAlpha;
			}
		}
	}

	private bool MoveToPositionChanged(Vector3 newPos)
	{
		if (oldPositionMode == CameraPositionMode.MOVE_TO)
		{
			return (moveToPos - newPos).sqrMagnitude > 0.01f;
		}
		return true;
	}

	public void CameraMoveTo(Block block, float alpha)
	{
		if (!GameCameraDisabled())
		{
			moveToPos = block.goT.position;
			positionMode = CameraPositionMode.MOVE_TO;
			moveToPosAlpha = alpha;
		}
	}

	public void CameraLookAt(Block block)
	{
		CameraLookAt(block, 0.95f);
	}

	public void CameraLookAt(Block block, float alpha)
	{
		if (!GameCameraDisabled())
		{
			lookAtPos = block.goT.position;
			targetMode = CameraTargetMode.LOOK_AT;
			lookAtAlpha = alpha;
		}
	}

	public void Unfollow()
	{
		targetBlock = null;
		targetBunch = null;
		targetChunkInfos.Clear();
		targetChunks.Clear();
		followedBlocks.Clear();
		allFollowedBlocks.Clear();
		followedModels = 0;
		followingVehicle = false;
		targetDistanceMultiplier = 1f;
		resetTargetDistanceMultiplierFactor = 1f;
		if (chunkSqrSpeeds != null && chunkSqrSpeeds.Length == Blocksworld.chunks.Count)
		{
			for (int i = 0; i < Blocksworld.chunks.Count; i++)
			{
				chunkSqrSpeeds[i] = 0f;
			}
		}
		camFollowCommands.Clear();
	}

	public void Unfollow(Chunk chunk)
	{
		if (!(chunk.go == null))
		{
			targetChunkInfos.Remove(chunk.go.GetInstanceID());
			targetChunks.Remove(chunk);
			camFollowCommands.Clear();
			prevCamFollowCommands.Clear();
			forceCommandExecution = true;
		}
	}

	public bool IsFollowing()
	{
		if (targetBlock == null && targetBunch == null && targetChunks.Count <= 0)
		{
			return positionMode == CameraPositionMode.MOVE_TO;
		}
		return true;
	}

	private void AutoFollow()
	{
		if (Blocksworld.chunks.Count != chunkSqrSpeeds.Length)
		{
			UpdateChunkSpeeds();
		}
		Block block = null;
		float num = -1f;
		for (int i = 0; i < Blocksworld.chunks.Count; i++)
		{
			Chunk chunk = Blocksworld.chunks[i];
			Rigidbody rb = chunk.rb;
			if (rb == null)
			{
				continue;
			}
			List<Block> blocks = chunk.blocks;
			bool flag = true;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			foreach (Block item in blocks)
			{
				if (!(item is BlockPosition) && !item.IsRuntimeInvisible() && item.GetMass() > 0f)
				{
					flag = false;
				}
				if (item is BlockCharacter || item is BlockAnimatedCharacter)
				{
					flag2 = true;
				}
				flag3 |= item.HasMover();
				flag4 |= item.HasAnyInputButton();
			}
			if (flag)
			{
				continue;
			}
			GameObject go = chunk.go;
			Vector3 position = go.transform.position;
			Vector3 vector = Blocksworld.mainCamera.WorldToViewportPoint(position);
			float num2 = 0f;
			if (!flag3)
			{
				num2 = ((!flag4) ? (targetPos - position).magnitude : 1f);
			}
			Vector3 velocity = rb.velocity;
			if (flag2)
			{
				velocity -= 0.9f * Vector3.up * rb.velocity.y;
			}
			chunkSqrSpeeds[i] = chunkSqrSpeeds[i] * 0.95f + 0.050000012f * velocity.sqrMagnitude;
			if (vector.x > -0.1f && vector.x < 1.1f && vector.y > -0.1f && vector.y < 1.1f && (flag3 || (num2 < 20f && chunkSqrSpeeds[i] > 2f)))
			{
				Transform transform = go.transform;
				Collider collider = null;
				if (transform.parent != null)
				{
					collider = transform.parent.gameObject.GetComponent<Collider>();
				}
				if ((!(collider != null) || !GeometryUtility.TestPlanesAABB(Blocksworld.frustumPlanes, collider.bounds)) && (num < 0f || num2 < num))
				{
					block = blocks[0];
					num = num2;
				}
			}
		}
		if (block != null)
		{
			Follow(block, auto: true);
		}
	}

	public void UpdateOrbitDistance(bool useMaxDist = false, float maxDist = 20f)
	{
		lastDistance = distance;
		UpdateTargetPos();
		distance = (Blocksworld.cameraTransform.position - (targetPos + GetScreenPlacementWorldError(targetDistanceMultiplier))).magnitude;
		if (useMaxDist)
		{
			distance = Mathf.Min(distance, maxDist);
		}
	}

	public void RestoreOrbitDistance()
	{
		distance = lastDistance;
	}

	public void PlaceCamera(Vector3 rot, Vector3 pos)
	{
		Transform cameraTransform = Blocksworld.cameraTransform;
		cameraTransform.eulerAngles = rot;
		if (Util.IsNullVector3(pos))
		{
			cameraTransform.position = -15f * Blocksworld.cameraTransform.forward;
			if (cameraTransform.position.y < 0.5f)
			{
				cameraTransform.position += (Mathf.Abs(cameraTransform.position.y) + 1f) * Vector3.up;
			}
		}
		else
		{
			cameraTransform.position = pos;
		}
		SetFilteredPositionAndTarget();
	}

	public void SetFilteredPositionAndTarget()
	{
		camPosFiltered = Blocksworld.cameraTransform.position;
		targetPosFiltered = camPosFiltered + Blocksworld.cameraTransform.forward * 10f;
	}

	public void LateUpdate()
	{
		Transform cameraTiltTransform = Blocksworld.bw.cameraTiltTransform;
		if (!(cameraTiltTransform == null))
		{
			if (Blocksworld.vrEnabled)
			{
				cameraTiltTransform.localRotation = Quaternion.identity;
			}
			else if (screenTiltRotationTracking)
			{
				cameraTiltTransform.localRotation = Quaternion.Slerp(cameraTiltTransform.localRotation, screenTiltRotation, 10f * Time.deltaTime);
			}
			else
			{
				cameraTiltTransform.localRotation = Quaternion.Slerp(cameraTiltTransform.localRotation, Quaternion.identity, 5f * Time.deltaTime);
			}
		}
	}

	public void Update()
	{
		if (Blocksworld.inBackground)
		{
			return;
		}
		Transform cameraTransform = Blocksworld.cameraTransform;
		if (Blocksworld.CurrentState == State.Play)
		{
			Vector3 position = cameraTransform.position;
			camVelocity = position - lastCameraPos;
			moveDist = camVelocity.magnitude;
			smoothedMoveDist = 0.95f * smoothedMoveDist + 0.05f * moveDist;
			lastCameraPos = position;
		}
		Vector3 vector = targetPos;
		UpdateTargetPos();
		bool flag = IsFollowing();
		if (Blocksworld.interpolateRigidBodies && Blocksworld.CurrentState == State.Play && flag && (vector - targetPos).sqrMagnitude > 0f)
		{
			MoveTowardsTarget();
			AimTowardsTarget();
		}
		if (firstPersonBlock != null)
		{
			bool flag2 = firstPersonBlock.IsFixed();
			flag2 |= firstPersonCharacter != null && firstPersonCharacter.unmoving;
			flag2 |= firstPersonAnimatedCharacter != null && firstPersonAnimatedCharacter.unmoving;
			Quaternion rotation = cameraTransform.rotation;
			Transform transform = firstPersonBlock.go.transform;
			Vector3 vector2;
			Vector3 worldPosition;
			if (firstPersonAnimatedCharacter != null)
			{
				vector2 = transform.position + 0.85f * transform.up;
				worldPosition = vector2 + transform.forward;
			}
			else
			{
				vector2 = transform.position + 0.5f * transform.up;
				worldPosition = vector2 + 5f * transform.forward;
			}
			cameraTransform.position = vector2;
			if (flag2)
			{
				cameraTransform.rotation = Quaternion.Slerp(transform.rotation, firstPersonLookAngle, 0.01f);
			}
			else
			{
				cameraTransform.LookAt(worldPosition, Vector3.up);
				cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, rotation, 0.9f);
				cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, firstPersonLookAngle, 0.05f);
			}
			firstPersonPos = cameraTransform.position;
			firstPersonLookAngle = cameraTransform.rotation;
			float f = Math.Max(0f, Vector3.Dot(firstPersonBlock.goT.forward, firstPersonBlockLastForward));
			firstPersonSmoothForward = Quaternion.Slerp(firstPersonBlock.goT.rotation, firstPersonSmoothForward, Mathf.Pow(f, 2f));
			firstPersonBlockLastForward = firstPersonBlock.goT.forward;
			cameraTransform.position = firstPersonPos + 0.2f * immediateOffset;
			actualFpcTilt = Mathf.Lerp(actualFpcTilt, fpcTilt, 0.05f);
			if (actualFpcTilt < 0.001f && fpcTilt == 0f)
			{
				actualFpcTilt = 0f;
			}
			else
			{
				cameraTransform.rotation *= Quaternion.Euler(actualFpcTilt, 0f, 0f);
			}
			if (firstPersonMode > 0)
			{
				firstPersonLook = Vector3.Lerp(firstPersonLook, firstPersonLookOffset, 0.05f);
				if (firstPersonMode == 2)
				{
					firstPersonLook.x = 0f;
				}
				Vector3 vector3 = -0.4f * (cameraTransform.rotation * firstPersonLook);
				if (flag2)
				{
					vector3 *= 0.1f;
				}
				cameraTransform.LookAt(cameraTransform.position + 120f * cameraTransform.forward + vector3, cameraTransform.up);
			}
		}
		else
		{
			float x = -3f * MappedInput.InputAxis(MappableInput.AXIS2_X);
			float y = -3f * MappedInput.InputAxis(MappableInput.AXIS2_Y);
			if (Blocksworld.CurrentState == State.Play && !Blocksworld.UI.Controls.IsDPadActive("R"))
			{
				OrbitBy(new Vector2(x, y));
			}
		}
		UpdateSpeedFoV();
		UpdateLightColorTint();
		BlockSky worldSky = Blocksworld.worldSky;
		if (worldSky != null)
		{
			Transform goT = worldSky.goT;
			goT.position = cameraTransform.position - 30f * Vector3.up;
			if (Blocksworld.worldSky.lockY)
			{
				float y2 = Blocksworld.worldSky.yLock;
				if (Blocksworld.worldOcean != null)
				{
					y2 = Blocksworld.worldOcean.transform.position.y;
				}
				goT.position = new Vector3(goT.position.x, y2, goT.position.z);
			}
		}
		if (Blocksworld.worldOcean != null)
		{
			Vector3 position2 = new Vector3(cameraTransform.position.x, Blocksworld.worldOcean.transform.position.y, cameraTransform.position.z);
			Blocksworld.worldOcean.transform.position = position2;
			Blocksworld.worldOceanBlock.SnapPosition();
		}
		if (!Blocksworld.renderingShadows)
		{
			Blocksworld.directionalLight.transform.rotation = cameraTransform.rotation * lightRotation;
		}
		if (Blocksworld.CurrentState == State.Build && Tutorial.state == TutorialState.None)
		{
			buildCameraWatchdog.Update();
		}
	}

	public void CameraStateLoaded()
	{
		targetPos = Blocksworld.cameraTransform.position + Blocksworld.cameraTransform.forward * manualCameraDistance;
		distance = manualCameraDistance;
		currentManualCameraDistance = manualCameraDistance;
		currentManualCameraAngle = manualCameraAngle;
		currentCameraOffsetVec = Vector3.one;
		storedOrbitPos = targetPos;
		buildCameraWatchdog.Reset();
		List<Block> list = BWSceneManager.AllBlocks();
		Bounds defaultWorldBounds = buildCameraWatchdog.GetDefaultWorldBounds();
		for (int i = 0; i < list.Count; i++)
		{
			Block block = list[i];
			if (block is BlockWater || block is BlockSky)
			{
				continue;
			}
			Vector3 size = block.size;
			if (block is BlockTerrain)
			{
				Collider component = block.go.GetComponent<Collider>();
				if (component != null)
				{
					defaultWorldBounds.Encapsulate(component.bounds);
				}
			}
			else
			{
				Vector3 position = block.goT.position;
				defaultWorldBounds.Encapsulate(new Bounds(position, size));
			}
		}
		buildCameraWatchdog.SetWorldBounds(defaultWorldBounds);
	}

	public void HideLayer(int layer)
	{
		int cullingMask = Blocksworld.mainCamera.cullingMask;
		Blocksworld.mainCamera.cullingMask = cullingMask & ~(1 << layer);
	}

	private void UpdateTargetPos()
	{
		if (Blocksworld.CurrentState == State.FrameCapture)
		{
			return;
		}
		Vector3 vector = targetPos;
		if (targetChunkInfos.Count > 0)
		{
			Vector3 vector2 = default(Vector3);
			int num = 0;
			foreach (Chunk targetChunk in targetChunks)
			{
				if (targetChunk.go != null)
				{
					Vector3 position = targetChunk.GetPosition();
					vector2 += position;
					num++;
				}
			}
			if (num > 0)
			{
				targetPos = vector2 / num;
			}
		}
		else if (targetBlock != null)
		{
			targetPos = targetBlock.GetPosition();
		}
		else if (targetBunch != null)
		{
			targetPos = targetBunch.GetPosition();
		}
		if (Util.IsNullVector3(targetPos))
		{
			targetPos = vector;
		}
	}

	private bool ExecuteFollowCommands()
	{
		bool flag = true;
		if (prevCamFollowCommands.Count == camFollowCommands.Count)
		{
			flag = false;
			for (int i = 0; i < prevCamFollowCommands.Count; i++)
			{
				CameraFollowCommand cameraFollowCommand = prevCamFollowCommands[i];
				CameraFollowCommand cameraFollowCommand2 = camFollowCommands[i];
				if (cameraFollowCommand2.block != cameraFollowCommand.block || cameraFollowCommand2.positionMode != cameraFollowCommand.positionMode)
				{
					flag = true;
					break;
				}
			}
		}
		flag = flag || forceCommandExecution;
		if (flag)
		{
			targetBlock = null;
			targetBunch = null;
			targetChunkInfos.Clear();
			targetChunks.Clear();
			followedBlocks.Clear();
			allFollowedBlocks.Clear();
			followedModels = 0;
			followingVehicle = false;
			targetDistanceMultiplier = 1f;
			resetTargetDistanceMultiplierFactor = 1f;
			modelSizes.Clear();
			modelBlocks.Clear();
			cameraLookTowardAngles.Clear();
			forceCommandExecution = false;
		}
		for (int j = 0; j < camFollowCommands.Count; j++)
		{
			Execute(camFollowCommands[j], flag);
		}
		if (camFollowCommands.Count > 0 && Blocksworld.CurrentState == State.Play)
		{
			UpdateTargetPos();
		}
		prevCamFollowCommands.Clear();
		prevCamFollowCommands.AddRange(camFollowCommands);
		camFollowCommands.Clear();
		return flag;
	}

	public void SetReticleEnabled(bool enabled)
	{
		if (reticleHolder != null)
		{
			reticleHolder.SetActive(enabled && -1 != hudReticle);
		}
	}

	public void FixedUpdate()
	{
		if (hudReticle == -1 && null != currentReticle)
		{
			currentReticle.enabled = false;
			currentReticle = null;
			reticleHolder.SetActive(value: false);
		}
		hudReticle = -1;
		if (broken)
		{
			return;
		}
		ExecuteFollowCommands();
		Transform cameraTransform = Blocksworld.cameraTransform;
		Vector3 position = cameraTransform.position;
		Quaternion rotation = cameraTransform.rotation;
		if (Blocksworld.CurrentState == State.Play)
		{
			if (targetChunkInfos.Count > 0)
			{
				HashSet<Chunk> hashSet = new HashSet<Chunk>();
				foreach (ChunkFollowInfo value in targetChunkInfos.Values)
				{
					if (!value.auto)
					{
						continue;
					}
					if (value.chunk.rb != null && value.chunk.rb.IsSleeping())
					{
						hashSet.Add(value.chunk);
						continue;
					}
					bool flag = true;
					foreach (Block block in value.chunk.blocks)
					{
						if (!block.IsRuntimeInvisible())
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						hashSet.Add(value.chunk);
					}
				}
				foreach (Chunk item in hashSet)
				{
					Unfollow(item);
				}
			}
			bool flag2 = IsFollowing();
			if (flag2 && positionMode == CameraPositionMode.MOVE_TO && firstPersonBlock != null)
			{
				flag2 = false;
			}
			if (!flag2 && !autoFollowDisabled && firstPersonBlock == null && !GameCameraDisabled())
			{
				AutoFollow();
				UpdateTargetPos();
			}
			if ((flag2 || positionMode == CameraPositionMode.MOVE_TO) && !Blocksworld.interpolateRigidBodies)
			{
				MoveTowardsTarget();
			}
			if ((flag2 || targetMode == CameraTargetMode.LOOK_AT) && !Blocksworld.interpolateRigidBodies)
			{
				AimTowardsTarget();
			}
			if (positionMode != CameraPositionMode.MOVE_TO)
			{
				float num = 0.8f;
				if (BW.isUnityEditor)
				{
					num = Options.ManualCameraSmoothness;
					if (num < 0.001f)
					{
						num = 0.8f;
					}
					else if (num > 0.999f)
					{
						num = 0.999f;
					}
				}
				float num2 = 1f - num;
				Vector3 vector = num2 * zoomOffset;
				zoomOffset -= vector;
				Vector3 vector2 = num2 * panOffset;
				panOffset -= vector2;
				Vector3 vector3 = Vector3.zero;
				if (orbitPosDiff.sqrMagnitude > 0.0001f)
				{
					Vector2 vector4 = num2 * orbitPosDiff;
					orbitPosDiff -= vector4;
					Vector3 position2 = cameraTransform.transform.position;
					DoOrbit(vector4);
					vector3 = cameraTransform.position - position2;
				}
				Vector3 vector5 = vector2 + vector;
				if (flag2)
				{
					camPosFiltered += vector5;
					cameraTransform.position += vector5;
				}
				else
				{
					camPosFiltered += vector5 + vector3;
					cameraTransform.position = camPosFiltered + immediateOffset;
				}
				targetPos += vector2;
				targetPosFiltered += vector2;
				if (vector.sqrMagnitude > 0.0001f)
				{
					UpdateOrbitDistance();
					currentManualCameraDistance = distance;
					currentCameraOffsetVec = currentCameraOffsetVec.normalized * currentManualCameraDistance;
					if (followedModels == 0)
					{
						targetPos += vector;
						targetPosFiltered += vector;
					}
				}
			}
		}
		else
		{
			manualCameraFraction = 0f;
			camPosFiltered = Blocksworld.cameraTransform.position;
			targetPosFiltered = camPosFiltered + Blocksworld.cameraTransform.forward * 10f;
		}
		tryKeepInViewTags.Clear();
		cameraStill = false;
		ResetCameraFollowParameters();
		targetCameraAngle = 70f;
		verticalDistanceOffsetFactor = 1f;
		targetFollowDistanceMultiplier = 1f;
		immediateOffset = Vector3.zero;
		forceDirectionHint = Vector3.zero;
		if (firstPersonBlock != null)
		{
			cameraTransform.position = position;
			cameraTransform.rotation = rotation;
		}
		screenTiltRotationTracking = screenTiltRotationSet;
		screenTiltRotationSet = false;
	}

	public void Focus()
	{
		int num = 100;
		Transform cameraTransform = Blocksworld.cameraTransform;
		Vector3 position = cameraTransform.position;
		while (--num >= 0 && !((targetPos - (position + distance * cameraTransform.forward)).sqrMagnitude <= 0.1f))
		{
			MoveTowardsTarget();
		}
	}

	private void DoOrbit(Vector2 posDiff)
	{
		Transform cameraTransform = Blocksworld.cameraTransform;
		cameraTransform.RotateAround(targetPos, Vector3.up, -0.5f * posDiff.x);
		cameraTransform.RotateAround(targetPos, cameraTransform.right, 0.25f * posDiff.y);
		Vector3 point = targetPos + GetScreenPlacementWorldError(targetDistanceMultiplier);
		if (Blocksworld.CurrentState == State.Play)
		{
			float num = 4f;
			float num2 = Vector3.Angle(cameraTransform.forward, -Vector3.up);
			if (num2 < num)
			{
				cameraTransform.RotateAround(point, cameraTransform.right, 0f - (num - num2));
			}
			else if (num2 > 180f - num)
			{
				cameraTransform.RotateAround(point, cameraTransform.right, num2 - (180f - num));
			}
		}
		if (cameraTransform.up.y < 0f)
		{
			float num3 = Mathf.Sign(cameraTransform.forward.y);
			float num4 = Vector3.Angle(cameraTransform.forward, num3 * Vector3.up);
			cameraTransform.RotateAround(point, cameraTransform.right, num3 * num4);
		}
		float num5 = Vector3.Angle(cameraTransform.forward, Vector3.up);
		float num6 = 0f;
		if (Blocksworld.CurrentState != State.Play && Blocksworld.CurrentState != State.FrameCapture && num5 < 90f + num6)
		{
			float num7 = num5 - 90f - num6;
			cameraTransform.RotateAround(point, cameraTransform.right, 0f - num7);
		}
		Vector3 vector = -cameraTransform.forward.normalized;
		float num8 = Vector3.Angle(vector, Vector3.up);
		if (Blocksworld.CurrentState == State.Play)
		{
			currentManualCameraAngle = num8;
		}
		else
		{
			manualCameraAngle = num8;
			manualCameraDistance = distance;
		}
		manualCameraFraction = 1f;
		mode2DRotation = cameraTransform.rotation;
		if (Blocksworld.CurrentState != State.Play)
		{
			MoveTowardsTarget();
		}
	}

	public void OrbitBy(Vector2 posDiff)
	{
		if (positionMode != CameraPositionMode.MOVE_TO)
		{
			if (Blocksworld.CurrentState == State.Play)
			{
				orbitPosDiff += posDiff;
			}
			else
			{
				DoOrbit(posDiff);
			}
		}
	}

	public void HardOrbit(Vector2 posDiff)
	{
		Transform cameraTransform = Blocksworld.cameraTransform;
		cameraTransform.RotateAround(targetPos, Vector3.up, 0f - posDiff.x);
		cameraTransform.RotateAround(targetPos, cameraTransform.right, posDiff.y);
	}

	private void MoveTowardsTargetMoveToMode()
	{
		Transform cameraTransform = Blocksworld.cameraTransform;
		float num = moveToPosAlpha;
		float num2 = 1f - num;
		camPosFiltered = num * camPosFiltered + num2 * (moveToPos + moveToOffset);
		cameraTransform.position = camPosFiltered + immediateOffset;
		Vector3 vector = -cameraTransform.forward.normalized;
		float num3 = Vector3.Angle(vector, Vector3.up);
		currentManualCameraAngle = num3;
	}

	private bool OkCameraPosition(Vector3 newPos)
	{
		if (Util.CameraVisibilityCheck(newPos, targetPosFiltered, allFollowedBlocks))
		{
			return !Util.PointWithinTerrain(newPos, fixedAsTerrain: true);
		}
		return false;
	}

	public static Vector3 GetLookTowardAngleDirection(Block b, float angle)
	{
		Quaternion rotation = b.goT.rotation;
		Quaternion quaternion = ((Blocksworld.CurrentState != State.Play) ? rotation : b.playRotation);
		Vector3 vector = Vector3.forward;
		bool flag = false;
		if (b.HasPreferredLookTowardAngleLocalVector())
		{
			Vector3 preferredLookTowardAngleLocalVector = b.GetPreferredLookTowardAngleLocalVector();
			Vector3 lhs = quaternion * preferredLookTowardAngleLocalVector;
			if (Mathf.Abs(Vector3.Dot(lhs, Vector3.up)) < 0.1f)
			{
				vector = preferredLookTowardAngleLocalVector;
				flag = true;
			}
		}
		if (!flag)
		{
			for (int i = 0; i < possibleForward.Length; i++)
			{
				Vector3 vector2 = possibleForward[i];
				Vector3 lhs2 = quaternion * vector2;
				if (Mathf.Abs(Vector3.Dot(lhs2, Vector3.up)) < 0.1f)
				{
					vector = vector2;
					break;
				}
			}
		}
		Vector3 vector3 = rotation * vector;
		Vector3 vec = Quaternion.AngleAxis(angle, Vector3.up) * vector3;
		return Util.ProjectOntoPlane(vec, Vector3.up).normalized;
	}

	private void MoveTowardsTargetDefaultMode(bool mode2d = false, bool useLookTowardAngle = false)
	{
		Transform cameraTransform = Blocksworld.cameraTransform;
		Vector3 vector = cameraTransform.position - immediateOffset;
		Vector3 vector2 = targetPos - oldTargetPos;
		oldTargetPos = targetPos;
		float magnitude = vector2.magnitude;
		float num = magnitude;
		filteredTargetVel = 0.1f * vector2 + 0.9f * filteredTargetVel;
		if (magnitude > 2f)
		{
			vector2 = default(Vector3);
		}
		float num2 = Mathf.Min(filteredTargetVel.magnitude, 1f);
		Vector3 vector3 = vector - targetPos;
		Vector3 normalized = vector3.normalized;
		bool flag = Util.PointWithinTerrain(targetPos);
		bool flag2 = IsFollowing();
		float num3 = cameraFollowAlpha;
		bool flag3 = false;
		if (allFollowedBlocks.Count == 1)
		{
			Block block = modelBlocks[0];
			Chunk chunk = block.chunk;
			Rigidbody rb = chunk.rb;
			if (rb == null || rb.isKinematic)
			{
				flag3 = true;
			}
		}
		float num5;
		if (followedModels == 1 && !mode2d && !flag3)
		{
			float num4 = 50f;
			num5 = targetFollowDistanceMultiplier * targetDistanceMultiplier * Mathf.Max(2.3f * Util.MaxComponent(modelSizes[0].size), 12f);
			if (num5 > num4)
			{
				num5 = num4 + 2f * Mathf.Sqrt(num5 - num4);
			}
		}
		else if (followedModels == 0 || mode2d || flag3)
		{
			num5 = currentManualCameraDistance + num2 * currentManualCameraDistance * velDistanceMultiplier;
		}
		else
		{
			Bounds bounds = default(Bounds);
			for (int i = 0; i < followedModels; i++)
			{
				Bounds bounds2 = modelSizes[i];
				bounds2.center = modelBlocks[i].goT.position;
				if (i == 0)
				{
					bounds = bounds2;
				}
				else
				{
					bounds.Encapsulate(bounds2);
				}
			}
			Bounds bounds3 = bounds;
			bounds3.Expand(bounds.size * 0.2f);
			Vector3 min = bounds3.min;
			Vector3 max = bounds3.max;
			Vector3[] array = new Vector3[8]
			{
				min,
				max,
				new Vector3(min.x, min.y, max.z),
				new Vector3(min.x, max.y, min.z),
				new Vector3(min.x, max.y, max.z),
				new Vector3(max.x, min.y, min.z),
				new Vector3(max.x, min.y, max.z),
				new Vector3(max.x, max.y, min.z)
			};
			float num6 = 0.8f;
			for (int j = 0; j < array.Length; j++)
			{
				if (!GeometryUtility.TestPlanesAABB(Blocksworld.frustumPlanes, new Bounds(array[j], Vector3.one)))
				{
					num6 = Mathf.Min(2f, num6 + 0.4f);
				}
			}
			multiFollowTargetDistanceFactor = 0.95f * multiFollowTargetDistanceFactor + 0.05f * num6;
			num5 = targetFollowDistanceMultiplier * Mathf.Max(multiFollowTargetDistanceFactor * Util.MaxComponent(bounds.size), 12f);
		}
		float num7 = 50f;
		if (mode2d)
		{
			num7 *= 0.25f;
		}
		if (followedModels > 1)
		{
			forceDirectionHint = Vector3.zero;
		}
		else if (forceDirectionHint.sqrMagnitude > num7 * num7)
		{
			forceDirectionHint = forceDirectionHint.normalized * num7;
		}
		filteredForceDirectionHint = 0.05f * forceDirectionHint + 0.95f * filteredForceDirectionHint;
		Vector3 vector6;
		if (mode2d)
		{
			Vector3 vector4 = mode2DRotation * Vector3.forward;
			Vector3 vector5 = targetPos;
			vector6 = vector5 - num5 * vector4;
			Vector3 vector7 = vector2 * 100f + filteredForceDirectionHint * 1f;
			float num8 = ((!GeometryUtility.TestPlanesAABB(Blocksworld.frustumPlanes, new Bounds(vector5 * 0.5f, Vector3.one))) ? 1f : 0.33f);
			Vector3 center = vector5 - vector7 * num8;
			for (int k = 0; k < 6; k++)
			{
				if (GeometryUtility.TestPlanesAABB(Blocksworld.frustumPlanes, new Bounds(center, Vector3.one)))
				{
					break;
				}
				vector7 *= 0.85f;
				center = vector5 - vector7 * num8;
			}
			vector6 += vector7;
			float magnitude2 = vector7.magnitude;
			if (magnitude2 > 1f)
			{
				float num9 = Mathf.Clamp(vector7.magnitude / num5, 0f, 1f);
				float num10 = 1f - 0.01f * num9;
				num3 *= num10;
			}
		}
		else
		{
			float num11 = 5f;
			if (filteredTargetVel.magnitude < 4f)
			{
				float num12 = filteredTargetVel.magnitude / 4f;
				num12 *= num12;
				num11 *= num12;
			}
			int num13 = 0;
			Vector3 zero = Vector3.zero;
			Vector3 vector8 = normalized / cameraVelResponsiveness;
			if (useLookTowardAngle)
			{
				Vector3 zero2 = Vector3.zero;
				if (cameraLookTowardAngles.Count > 0)
				{
					foreach (KeyValuePair<Block, float> cameraLookTowardAngle in cameraLookTowardAngles)
					{
						zero2 += GetLookTowardAngleDirection(cameraLookTowardAngle.Key, cameraLookTowardAngle.Value);
					}
				}
				else
				{
					foreach (string tryKeepInViewTag in tryKeepInViewTags)
					{
						if (TagManager.TryGetClosestBlockWithTag(tryKeepInViewTag, targetPosFiltered, out var block2))
						{
							Vector3 position = block2.goT.position;
							Vector3 vec = targetPosFiltered - position;
							zero2 += Util.ProjectOntoPlane(vec, Vector3.up).normalized;
							num13++;
							zero += position;
						}
					}
					if (num13 > 0)
					{
						zero /= (float)num13;
					}
				}
				if (zero2.sqrMagnitude > 0.01f)
				{
					zero2.Normalize();
					Vector3 normalized2 = Util.ProjectOntoPlane(vector8, Vector3.up).normalized;
					zero2 = Vector3.RotateTowards(normalized2, zero2, 2f, 100f);
					vector8 += zero2 * 5f;
				}
				else
				{
					vector8 = vector8 - vector2 * num11 - filteredForceDirectionHint;
				}
			}
			else
			{
				vector8 = vector8 - vector2 * num11 - filteredForceDirectionHint;
			}
			if (blockDirectionFactor != Vector3.zero)
			{
				Vector3 zero3 = Vector3.zero;
				foreach (Block followedBlock in followedBlocks)
				{
					Vector3 vector9 = followedBlock.goT.rotation * blockDirectionFactor;
					zero3 += vector9;
				}
				if (zero3.sqrMagnitude > 0.01f)
				{
					vector8 += zero3;
				}
			}
			if (followedModels <= 1 && num13 == 0)
			{
				Vector3 rhs = Util.ProjectOntoPlane(cameraTransform.forward, Vector3.up);
				Vector3 vector10 = Util.ProjectOntoPlane(filteredForceDirectionHint, Vector3.up);
				Vector3 vector11 = Util.ProjectOntoPlane(filteredTargetVel, Vector3.up);
				Vector3 lhs = vector11 + 0.2f * vector10;
				float num14 = 0f - Vector3.Dot(lhs, rhs);
				if (num14 > 0f)
				{
					num14 = Mathf.Min(0.2f, num14);
					Vector3 rhs2 = Util.ProjectOntoPlane(cameraTransform.right, Vector3.up);
					int num15 = (int)Mathf.Sign(Vector3.Dot(lhs, rhs2));
					Vector3 vector12 = -Vector3.Cross(lhs, Vector3.up).normalized * num15 * num14 * 20f / (vector3.magnitude * 0.2f);
					vector8 += vector12;
				}
			}
			if ((double)vector8.sqrMagnitude < 0.01)
			{
				vector8 += normalized * 0.1f;
			}
			Vector3 normalized3 = vector8.normalized;
			Vector3 normalized4 = Vector3.Cross(normalized3, Vector3.up).normalized;
			Quaternion quaternion = Quaternion.AngleAxis(0f - currentManualCameraAngle, normalized4);
			Vector3 normalized5 = (quaternion * Vector3.up).normalized;
			vector6 = targetPosFiltered + normalized5 * num5;
			float num16 = Mathf.Clamp(0.45f + num * 10f, 0f, 1f);
			vector6 = vector6 * num16 + vector * (1f - num16);
			float magnitude3 = filteredTargetVel.magnitude;
			num3 = Mathf.Clamp(cameraFollowAlpha - magnitude3 * 0.02f, 0.85f, 1f);
			if (!flag3)
			{
				float num17 = targetCameraAngle;
				if (num13 > 0)
				{
					float num18 = Util.AngleBetween(zero - vector, Util.ProjectOntoPlane(cameraTransform.forward, Vector3.up), cameraTransform.right);
					num17 = ((!(num18 < 0f)) ? (targetCameraAngle + Mathf.Min(num18, 30f)) : (targetCameraAngle + Mathf.Clamp(num18 + 20f, -30f, 0f)));
				}
				currentManualCameraAngle = manualCameraFraction * currentManualCameraAngle + (1f - manualCameraFraction) * (0.1f * currentManualCameraAngle + 0.9f * num17);
			}
		}
		float num19 = 1f - num3;
		Vector3 vector13 = vector;
		if (flag2 && !mode2d && targetDistanceMultiplier < 1f)
		{
			float num20 = targetDistanceMultiplier * resetTargetDistanceMultiplierFactor;
			Vector3 vector14 = targetPos + lookAtOffset + GetScreenPlacementWorldError(num20);
			Vector3 normalized6 = (vector - vector14).normalized;
			Vector3 vector15 = vector14 + normalized6 * num5 * (num20 / targetDistanceMultiplier);
			if (Util.CameraVisibilityCheck(vector15, vector14, allFollowedBlocks, occludeByBroken: false))
			{
				resetTargetDistanceMultiplierFactor = 0.99f * resetTargetDistanceMultiplierFactor + 0.010499999f;
				targetDistanceMultiplier *= resetTargetDistanceMultiplierFactor;
			}
			else
			{
				resetTargetDistanceMultiplierFactor = 1f;
			}
		}
		targetDistanceMultiplier = Mathf.Clamp(targetDistanceMultiplier, 0.05f, 1f);
		camPosFiltered = manualCameraFraction * vector13 + (1f - manualCameraFraction) * (num3 * camPosFiltered + num19 * (vector6 + moveToOffset));
		Vector3 vector16 = camInitSlowdown * vector + (1f - camInitSlowdown) * camPosFiltered;
		if (!float.IsNaN(vector16.x) && !float.IsNaN(vector16.y) && !float.IsNaN(vector16.z))
		{
			Quaternion rotation = cameraTransform.rotation;
			cameraTransform.position = vector16 + immediateOffset;
			cameraTransform.rotation = rotation;
		}
		manualCameraFraction *= 0.98f;
		camInitSlowdown *= 0.99f;
	}

	private void MoveTowardsTarget()
	{
		if (cameraStill)
		{
			return;
		}
		if (Blocksworld.CurrentState == State.Play)
		{
			if (firstPersonBlock == null)
			{
				switch (positionMode)
				{
				case CameraPositionMode.DEFAULT:
				case CameraPositionMode.DEFAULT_2D:
				case CameraPositionMode.LOOK_TOWARD:
				case CameraPositionMode.LOOK_TOWARD_TAG:
					MoveTowardsTargetDefaultMode(positionMode == CameraPositionMode.DEFAULT_2D, positionMode == CameraPositionMode.LOOK_TOWARD || positionMode == CameraPositionMode.LOOK_TOWARD_TAG);
					break;
				case CameraPositionMode.MOVE_TO:
					MoveTowardsTargetMoveToMode();
					break;
				}
			}
		}
		else
		{
			Transform cameraTransform = Blocksworld.cameraTransform;
			Vector3 position = cameraTransform.position;
			Vector3 vector = targetPos - (position + distance * cameraTransform.forward);
			cameraTransform.position += 1.5f * vector / distance;
			camPosFiltered = cameraTransform.position;
		}
	}

	private Vector3 GetScreenPlacementWorldError(float distanceMultiplier)
	{
		if (Blocksworld.CurrentState != State.Play || followedModels == 0 || (followedModels == 1 && !followingVehicle))
		{
			return Vector3.zero;
		}
		Transform cameraTransform = Blocksworld.cameraTransform;
		Vector3 vector = Util.WorldToScreenPoint(targetPos, z: false);
		float num = Util.WorldToScreenPoint(targetPos + cameraTransform.up, z: false).y - vector.y;
		float num2 = 0.2f;
		if (followedModels > 1)
		{
			num2 = 0.3f;
		}
		Vector3 vector2 = new Vector3(NormalizedScreen.width, (float)NormalizedScreen.height * num2, 0f);
		Vector3 vector3 = (0f - (vector2 - vector).y) * cameraTransform.up / num;
		if (distanceMultiplier < 1f)
		{
			float num3 = 0f;
			for (int i = 0; i < followedModels; i++)
			{
				num3 = Mathf.Max(num3, Util.MaxComponent(modelSizes[i].size));
			}
			float num4 = 1f - distanceMultiplier;
			vector3 += Vector3.up * num3 * num4;
		}
		return verticalDistanceOffsetFactor * vector3;
	}

	private void AimTowardsTarget(float alpha = 0.92f)
	{
		if (cameraStill)
		{
			return;
		}
		Transform cameraTransform = Blocksworld.cameraTransform;
		if (targetMode == CameraTargetMode.DEFAULT)
		{
			Vector3 screenPlacementWorldError = GetScreenPlacementWorldError(targetDistanceMultiplier);
			Vector3 vector = targetPos + lookAtOffset + screenPlacementWorldError;
			float magnitude = (targetPosFiltered - vector).magnitude;
			float magnitude2 = (cameraTransform.position - vector).magnitude;
			float num = magnitude / magnitude2;
			alpha = Mathf.Clamp(0.95f - num * 0.15f, 0.8f, 1f);
			targetPosFiltered = alpha * targetPosFiltered + (1f - alpha) * vector;
		}
		else
		{
			alpha = lookAtAlpha;
			targetPosFiltered = alpha * targetPosFiltered + (1f - alpha) * (lookAtPos + lookAtOffset);
		}
		if (positionMode != CameraPositionMode.DEFAULT_2D)
		{
			Vector3 vector2 = Vector3.up;
			if (screenTiltAngle != 0f)
			{
				vector2 = Quaternion.AngleAxis(screenTiltAngle, Util.ProjectOntoPlane(cameraTransform.forward, Vector3.up).normalized) * vector2;
			}
			cameraTransform.LookAt(targetPosFiltered + immediateOffset, vector2);
		}
	}

	private bool FollowingAuto()
	{
		if (targetChunkInfos.Count > 0)
		{
			bool result = true;
			foreach (KeyValuePair<int, ChunkFollowInfo> targetChunkInfo in targetChunkInfos)
			{
				if (!targetChunkInfos[targetChunkInfo.Key].auto)
				{
					result = false;
					break;
				}
			}
			return result;
		}
		return false;
	}

	public void PanBy(Vector2 diff)
	{
		if (positionMode != CameraPositionMode.MOVE_TO)
		{
			Vector3 position = Blocksworld.cameraTransform.position;
			float num = ((Blocksworld.CurrentState != State.Play) ? (0.025f * Mathf.Clamp(0.12f * (position.y - 5f), 1f, 20f)) : 0.03f);
			Vector3 right = Blocksworld.cameraTransform.right;
			Vector3 vector = Util.ProjectOntoPlane(Blocksworld.cameraTransform.forward, Vector3.up);
			if (vector.magnitude > 0.1f)
			{
				vector.Normalize();
			}
			else
			{
				vector = Util.ProjectOntoPlane(Blocksworld.cameraTransform.up, Vector3.up);
				vector.Normalize();
			}
			Vector3 vector2 = num * (diff.y * vector + diff.x * right);
			if (Blocksworld.CurrentState != State.Play)
			{
				Blocksworld.cameraTransform.position += vector2;
				targetPos += vector2;
				targetPosFiltered += vector2;
			}
			else
			{
				panOffset += vector2;
			}
		}
	}

	public void ZoomBy(float diff)
	{
		ZoomBy(diff, 0f);
	}

	public void ZoomBy(float diff, float posDiff)
	{
		if (positionMode == CameraPositionMode.MOVE_TO)
		{
			return;
		}
		Transform cameraTransform = Blocksworld.cameraTransform;
		Vector3 vector = 0.1f * diff * cameraTransform.forward;
		if (Options.RelativeZoom && IsFollowing())
		{
			float magnitude = (cameraTransform.position - targetPos).magnitude;
			vector *= magnitude / 15f;
		}
		if (Blocksworld.CurrentState != State.Play)
		{
			Vector3 vector2 = cameraTransform.position + vector;
			float magnitude2 = (targetPos - vector2).magnitude;
			float num = 1f;
			if (diff < 0f || magnitude2 > 2f * num)
			{
				cameraTransform.position = vector2;
				camPosFiltered = vector2;
			}
			else if (TBox.selected == null || (!IsFollowing() && Blocksworld.CurrentState == State.Play))
			{
				Vector3 point = targetPos + vector;
				if (!Util.PointWithinTerrain(point))
				{
					targetPos = point;
					cameraTransform.position = vector2;
				}
			}
			UpdateOrbitDistance();
			float num2 = Mathf.Abs(diff);
			float num3 = Mathf.Abs(posDiff);
			float num4 = 1f;
			float num5 = 2f;
			if (Blocksworld.CurrentState == State.FrameCapture || !(num3 >= num4))
			{
			}
			manualCameraFraction = 1f;
			manualCameraDistance = distance;
		}
		else
		{
			zoomOffset += vector;
		}
	}

	public Color GetLightTint()
	{
		return lightTint;
	}

	private void UpdateLightColorTint()
	{
		Vector3 position = Blocksworld.cameraTransform.position;
		Util.PointWithinTerrain(position, out var hitsUp, out var hitsDown);
		int num = hitsUp - hitsDown;
		if (num != prevHitDiff)
		{
			if (num == 0 || Blocksworld.CurrentState == State.Play)
			{
				lightTint = Color.white;
			}
			else
			{
				lightTint = Util.Color(200f, 100f, 100f, 120f);
			}
			Blocksworld.UpdateLightColor();
		}
		prevHitDiff = num;
	}

	public void AddForceDirectionHint(Chunk chunk, Vector3 force)
	{
		if (targetChunks.Contains(chunk))
		{
			forceDirectionHint += force;
		}
	}

	public void ChunkDirty(Chunk chunk)
	{
		if (targetChunks.Contains(chunk))
		{
			forceCommandExecution = true;
		}
	}

	public void SetLookAtOffset(Vector3 offset)
	{
		lookAtOffset = offset;
	}

	public void SetMoveToOffset(Vector3 offset)
	{
		moveToOffset = offset;
	}

	public void AddImmediateOffset(Vector3 offset)
	{
		immediateOffset += offset;
	}

	public void UpdateSpeedFoV()
	{
		if (currentSpeedFoV != desiredSpeedFoV || desiredSpeedFoV != 0f)
		{
			currentSpeedFoV = 0.9f * currentSpeedFoV + 0.1f * desiredSpeedFoV;
			if (Math.Abs(currentSpeedFoV - desiredSpeedFoV) < 0.0001f)
			{
				currentSpeedFoV = desiredSpeedFoV;
			}
			float num = 1f;
			float num2 = 0.35f * currentSpeedFoV;
			float num3 = currentSpeedFoV;
			if (smoothedMoveDist < num2)
			{
				num = 0f;
			}
			else if (smoothedMoveDist < num3)
			{
				num = (smoothedMoveDist - num2) / (num3 - num2);
			}
			Blocksworld.cameraTransform.position += num * Blocksworld.cameraTransform.forward;
		}
	}

	public void EnableAutoFollow(bool e)
	{
		autoFollowDisabled = !e;
	}

	public void SetTargetCameraAngle(float a)
	{
		targetCameraAngle = a;
	}

	public void SetVerticalDistanceOffsetFactor(float f)
	{
		verticalDistanceOffsetFactor = f;
	}

	public void SetTargetFollowDistanceMultiplier(float m)
	{
		targetFollowDistanceMultiplier = m;
	}

	public void SetSingleton(Block block, bool s)
	{
		if (s)
		{
			singletonBlocks.Add(block);
		}
		else
		{
			singletonBlocks.Remove(block);
		}
	}

	public static Vector3 GetCamVelocity()
	{
		return camVelocity;
	}

	public void SetScreenTiltRotation(Quaternion tiltRotation)
	{
		if (!screenTiltRotationTracking)
		{
			screenTiltBaseRotation = tiltRotation;
			camTiltBaseRotation = Blocksworld.cameraTransform.rotation;
		}
		screenTiltRotation = Quaternion.Inverse(screenTiltBaseRotation) * tiltRotation;
		screenTiltRotationSet = true;
		screenTiltRotationTracking = true;
	}

	public void AddKeepInViewTag(string t)
	{
		tryKeepInViewTags.Add(t);
	}

	public void FirstPersonFollow(Block newBlock, int mode)
	{
		desiredFirstPersonBlock = newBlock;
		firstPersonMode = mode;
	}

	public void FinalUpdateFirstPersonFollow()
	{
		UpdateDesiredFollowBlock();
		desiredFirstPersonBlock = null;
		if (firstPersonCharacter == null && firstPersonAnimatedCharacter == null)
		{
			return;
		}
		Transform cameraTransform = Blocksworld.cameraTransform;
		Vector3 position = cameraTransform.position;
		if ((firstPersonCharacter != null && firstPersonCharacter.unmoving) || (firstPersonAnimatedCharacter != null && firstPersonAnimatedCharacter.unmoving))
		{
			return;
		}
		RaycastHit[] array = Physics.RaycastAll(position, cameraTransform.forward, maxAimAdjustRange);
		if (array.Length != 0)
		{
			Util.SmartSort(array, Blocksworld.cameraTransform.position);
			RaycastHit[] array2 = array;
			foreach (RaycastHit raycastHit in array2)
			{
				Vector3 forward = cameraTransform.InverseTransformPoint(raycastHit.point);
				if (forward.magnitude >= minAimAdjustRange)
				{
					aimAdjustOffset = Quaternion.Slerp(aimAdjustOffset, Quaternion.Slerp(Quaternion.identity, Quaternion.LookRotation(forward), 0.15f), 0.05f);
					break;
				}
			}
		}
		else
		{
			aimAdjustOffset = Quaternion.Slerp(aimAdjustOffset, Quaternion.identity, 0.1f);
		}
	}

	private void UpdateDesiredFollowBlock()
	{
		if (desiredFirstPersonBlock == firstPersonBlock)
		{
			return;
		}
		if (firstPersonBlock != null && desiredFirstPersonBlock == null)
		{
			StopFirstPersonFollow(firstPersonBlock);
			return;
		}
		if (desiredFirstPersonBlock != null && (desiredFirstPersonBlock.broken || desiredFirstPersonBlock.vanished))
		{
			if (firstPersonBlock != null)
			{
				StopFirstPersonFollow(firstPersonBlock);
			}
			return;
		}
		if (firstPersonBlock != null)
		{
			StopFirstPersonFollow(firstPersonBlock);
		}
		firstPersonBlock = desiredFirstPersonBlock;
		firstPersonCharacter = desiredFirstPersonBlock as BlockCharacter;
		firstPersonAnimatedCharacter = desiredFirstPersonBlock as BlockAnimatedCharacter;
		if (firstPersonAnimatedCharacter != null)
		{
			firstPersonHead = firstPersonAnimatedCharacter.GetHeadAttach();
		}
		firstPersonLookAngle = firstPersonBlock.go.transform.rotation;
		firstPersonPos = firstPersonBlock.go.transform.position;
		firstPersonSmoothForward = firstPersonBlock.goT.rotation;
		firstPersonBlock.SetFPCGearVisible(visible: false);
		if (firstPersonCharacter != null && !firstPersonCharacter.unmoving)
		{
			firstPersonPos += 0.5f * firstPersonCharacter.go.transform.up;
		}
		if (null != firstPersonHead)
		{
			firstPersonPos = firstPersonHead.transform.position;
		}
		if ((firstPersonCharacter == null || !firstPersonCharacter.unmoving) && (firstPersonAnimatedCharacter == null || !firstPersonAnimatedCharacter.unmoving))
		{
			for (int i = 0; i < firstPersonBlock.connections.Count; i++)
			{
				Block block = firstPersonBlock.connections[i];
				if (!block.isTerrain && block.go.activeSelf && (!(firstPersonHead != null) || !(block.go.transform.parent != firstPersonHead.transform)) && block.go.transform.localPosition.y >= 0.4f && block.go.transform.localPosition.y < 0.75f && !block.IsRuntimeInvisible())
				{
					HeadgearInfo item = new HeadgearInfo
					{
						block = block,
						localPos = block.go.transform.localPosition,
						localRot = block.go.transform.localRotation,
						parent = block.go.transform.parent
					};
					firstPersonHeadgear.Add(item);
					block.EnableCollider(value: false);
					block.go.transform.SetParent(Blocksworld.cameraTransform);
					block.go.transform.rotation = Blocksworld.cameraTransform.rotation;
					block.go.transform.position = Blocksworld.cameraTransform.position;
				}
			}
		}
		if (firstPersonMode == 1)
		{
			HashSet<Predicate> manyPreds = new HashSet<Predicate> { BlockCharacter.predicateCharacterMover };
			if (firstPersonBlock.ContainsTileWithAnyPredicateInPlayMode2(manyPreds))
			{
				firstPersonMode = 2;
			}
		}
		Blocksworld.cameraTransform.position = firstPersonPos;
		Blocksworld.cameraTransform.rotation = firstPersonBlock.go.transform.rotation;
		moveDist = 0f;
		smoothedMoveDist = 0f;
		firstPersonDpad = Vector2.zero;
		firstPersonRotation = 0f;
		aimAdjustOffset = Quaternion.identity;
		firstPersonLook = (firstPersonLookOffset = Vector3.zero);
	}

	public void StopFirstPersonFollow(Block oldBlock)
	{
		if (firstPersonBlock == null)
		{
			BWLog.Info("Attempting to stop First Person mode when it was never started");
			return;
		}
		if (firstPersonBlock != oldBlock)
		{
			BWLog.Info("First person wasn't old block, not ending");
			return;
		}
		firstPersonBlock.SetFPCGearVisible(visible: true);
		foreach (HeadgearInfo item in firstPersonHeadgear)
		{
			item.block.go.transform.SetParent(item.parent);
			item.block.go.transform.localPosition = item.localPos;
			item.block.go.transform.localRotation = item.localRot;
			item.block.EnableCollider(value: true);
		}
		firstPersonHeadgear.Clear();
		firstPersonBlock = null;
		firstPersonCharacter = null;
		firstPersonHead = null;
		Unfollow();
		Transform cameraTransform = Blocksworld.cameraTransform;
		cameraTransform.position = oldBlock.go.transform.position - 15f * oldBlock.go.transform.forward;
		cameraTransform.LookAt(oldBlock.go.transform.position);
		camPosFiltered = cameraTransform.position;
		targetPos = oldBlock.go.transform.position;
		targetPosFiltered = targetPos;
		distance = 3f;
		lastDistance = distance;
		moveToOffset = Vector3.zero;
		lookAtOffset = Vector3.zero;
		panOffset = Vector3.zero;
		zoomOffset = Vector3.zero;
		orbitPosDiff = Vector2.zero;
		positionMode = CameraPositionMode.DEFAULT;
		targetMode = CameraTargetMode.DEFAULT;
		currentCameraOffsetVec = Vector3.one;
		filteredTargetVel = Vector3.zero;
		oldTargetPos = targetPos;
		screenTiltRotationSet = (screenTiltRotationTracking = false);
		currentManualCameraAngle = manualCameraAngle;
		currentManualCameraDistance = manualCameraDistance;
		manualCameraFraction = 0f;
		camInitSlowdown = 1f;
		DoOrbit(Vector2.zero);
		targetPos = oldBlock.go.transform.position;
		targetPosFiltered = targetPos;
		aimAdjustOffset = Quaternion.identity;
	}

	public void SetHudReticle(int reticle)
	{
		hudReticle = reticle;
		Renderer renderer = ((hudReticle < 0 || hudReticle >= hudDisplayObjects.Count) ? null : hudDisplayObjects[hudReticle].GetComponent<Renderer>());
		if (!(currentReticle == renderer))
		{
			if (null != currentReticle)
			{
				currentReticle.enabled = false;
			}
			if (!(null == renderer))
			{
				currentReticle = renderer;
				currentReticle.enabled = true;
				reticleHolder.SetActive(value: true);
			}
		}
	}

	public Vector3 GetAimAdjustTarget(Block sender)
	{
		if (firstPersonCharacter == null || sender.chunk != firstPersonBlock.chunk || firstPersonCharacter.unmoving)
		{
			return Vector3.zero;
		}
		return Blocksworld.cameraTransform.position + 20f * (aimAdjustOffset * Blocksworld.cameraTransform.forward);
	}

	public void HandleTeleport(Block b)
	{
		if (IsFollowing())
		{
			Reset();
		}
	}

	public bool IsFirstPerson()
	{
		return firstPersonBlock != null;
	}

	public Transform GetFirstPersonHead()
	{
		if (firstPersonHead != null)
		{
			return firstPersonHead.transform;
		}
		return null;
	}
}
