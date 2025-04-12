using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x020000FA RID: 250
public class BlocksworldCamera
{
	// Token: 0x1700004E RID: 78
	// (get) Token: 0x06001227 RID: 4647 RVA: 0x0007CF2B File Offset: 0x0007B32B
	// (set) Token: 0x06001228 RID: 4648 RVA: 0x0007CF33 File Offset: 0x0007B333
	public float manualCameraDistance
	{
		get
		{
			return this._manualCameraDistance;
		}
		set
		{
			this._manualCameraDistance = value;
		}
	}

	// Token: 0x1700004F RID: 79
	// (get) Token: 0x06001229 RID: 4649 RVA: 0x0007CF3C File Offset: 0x0007B33C
	// (set) Token: 0x0600122A RID: 4650 RVA: 0x0007CF44 File Offset: 0x0007B344
	public float manualCameraHeight
	{
		get
		{
			return this._manualCameraHeight;
		}
		set
		{
			this._manualCameraHeight = value;
		}
	}

	// Token: 0x17000050 RID: 80
	// (get) Token: 0x0600122B RID: 4651 RVA: 0x0007CF4D File Offset: 0x0007B34D
	// (set) Token: 0x0600122C RID: 4652 RVA: 0x0007CF55 File Offset: 0x0007B355
	public float manualCameraAngle
	{
		get
		{
			return this._manualCameraAngle;
		}
		set
		{
			this._manualCameraAngle = value;
		}
	}

	// Token: 0x0600122D RID: 4653 RVA: 0x0007CF5E File Offset: 0x0007B35E
	public void Init()
	{
		ViewportWatchdog.AddListener(new ViewportWatchdog.ViewportSizeChangedAction(this.ViewportSizeDidChange));
	}

	// Token: 0x0600122E RID: 4654 RVA: 0x0007CF74 File Offset: 0x0007B374
	public void Play()
	{
		if (null == this.reticleHolder)
		{
			this.reticleHolder = new GameObject();
			this.reticleHolder.name = "Reticle Holder";
			this.reticleHolder.layer = LayerMask.NameToLayer("UI");
			this.SetReticleParent(null);
		}
		BlocksworldComponentData componentData = Blocksworld.componentData;
		if (null != componentData)
		{
			this.maxSpeedFoV = componentData.maxSpeedFoV;
			if (this.hudDisplayObjects == null)
			{
				this.hudDisplayObjects = new List<GameObject>();
				foreach (Texture texture in componentData.hudTextures)
				{
					GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
					Material material = gameObject.GetComponent<Renderer>().material;
					if (null != material)
					{
						material.shader = Shader.Find("Blocksworld/Particles/Additive");
						material.SetTexture("_MainTex", texture);
					}
					gameObject.transform.SetParent(this.reticleHolder.transform);
					gameObject.transform.localPosition = Vector3.zero;
					gameObject.transform.localRotation = Quaternion.identity;
					gameObject.transform.localScale = 0.1f * Vector3.one;
					gameObject.layer = this.reticleHolder.layer;
					this.hudDisplayObjects.Add(gameObject);
				}
			}
			this.maxAimAdjustRange = componentData.aimAdjustMax;
			this.minAimAdjustRange = componentData.aimAdjustMin;
			this.firstPersonDeadZone = componentData.firstPersonDeadZone / 100f;
			this.firstPersonTurnScale = componentData.firstPersonTurnPower;
			this.firstPersonTorque = componentData.firstPersonTorque;
			this.firstPersonLookXFlip = componentData.FPCLookXFlip;
			this.firstPersonLookYFlip = componentData.FPCLookYFlip;
		}
		this.firstPersonMode = 0;
		foreach (GameObject gameObject2 in this.hudDisplayObjects)
		{
			gameObject2.GetComponent<Renderer>().enabled = false;
		}
		this.reticleHolder.SetActive(false);
		this.tryKeepInViewTags.Clear();
		this.moveToOffset = Vector3.zero;
		this.lookAtOffset = Vector3.zero;
		this.panOffset = Vector3.zero;
		this.zoomOffset = Vector3.zero;
		this.orbitPosDiff = Vector2.zero;
		this.positionMode = BlocksworldCamera.CameraPositionMode.DEFAULT;
		this.targetMode = BlocksworldCamera.CameraTargetMode.DEFAULT;
		this.currentCameraOffsetVec = Vector3.one;
		this.previousTargetBunch = this.targetBunch;
		this.previousTargetBlock = this.targetBlock;
		this.Unfollow();
		this.UpdateChunkSpeeds();
		this.UpdateTargetPos();
		this.filteredTargetVel = Vector3.zero;
		this.oldTargetPos = this.targetPos;
		this.screenTiltRotationSet = (this.screenTiltRotationTracking = false);
		this.currentManualCameraAngle = this.manualCameraAngle;
		this.currentManualCameraDistance = this.manualCameraDistance;
		this.camPosFiltered = Blocksworld.cameraTransform.position;
		this.targetPosFiltered = this.camPosFiltered + Blocksworld.cameraTransform.forward * this.distance;
		this.targetPos = this.targetPosFiltered;
		this.manualCameraFraction = 0f;
		this.camInitSlowdown = 1f;
		this.autoFollowDisabled = (BW.isUnityEditor && Options.DisableAutoFollow);
		this.ResetCameraFollowParameters();
		this.forceDirectionHint = Vector3.zero;
		this.filteredForceDirectionHint = Vector3.zero;
		this.mode2DRotation = Blocksworld.cameraTransform.rotation;
		this.singletonBlocks.Clear();
		this.camFollowCommands.Clear();
		this.prevCamFollowCommands.Clear();
		this.lastCameraPos = Blocksworld.cameraTransform.position;
		this.moveDist = 0f;
		this.smoothedMoveDist = 0f;
		this.firstPersonHeadgear.Clear();
		this.aimAdjustOffset = Quaternion.identity;
	}

	// Token: 0x0600122F RID: 4655 RVA: 0x0007D358 File Offset: 0x0007B758
	public void Stop()
	{
		if (this.firstPersonBlock != null)
		{
			this.firstPersonBlock.SetFPCGearVisible(true);
			foreach (BlocksworldCamera.HeadgearInfo headgearInfo in this.firstPersonHeadgear)
			{
				headgearInfo.block.EnableCollider(true);
			}
			this.firstPersonHeadgear.Clear();
			this.firstPersonBlock = null;
			this.firstPersonCharacter = null;
			this.firstPersonAnimatedCharacter = null;
			this.firstPersonHead = null;
			this.actualFpcTilt = (this.fpcTilt = 0f);
		}
		this.positionMode = BlocksworldCamera.CameraPositionMode.DEFAULT;
		this.targetMode = BlocksworldCamera.CameraTargetMode.DEFAULT;
		this.tryKeepInViewTags.Clear();
		this.screenTiltRotationSet = (this.screenTiltRotationTracking = false);
		this.modelSizes.Clear();
		this.modelBlocks.Clear();
		this.Unfollow();
		if (this.previousTargetBunch != null)
		{
			this.Follow(this.previousTargetBunch);
		}
		else if (this.previousTargetBlock != null)
		{
			this.Follow(this.previousTargetBlock);
		}
		this.ResetCameraFollowParameters();
		this.camFollowCommands.Clear();
		this.prevCamFollowCommands.Clear();
		this.buildCameraWatchdog.Reset(true);
		this.singletonBlocks.Clear();
		this.currentSpeedFoV = 0f;
		this.desiredSpeedFoV = 0f;
	}

	// Token: 0x06001230 RID: 4656 RVA: 0x0007D4D0 File Offset: 0x0007B8D0
	public void SetReticleParent(Transform parentTransform = null)
	{
		if (parentTransform == null)
		{
			parentTransform = Blocksworld.cameraTransform;
		}
		this.reticleHolder.transform.SetParent(parentTransform);
		this.reticleHolder.transform.localPosition = 0.5f * Vector3.forward;
		this.reticleHolder.transform.localRotation = Quaternion.identity;
	}

	// Token: 0x06001231 RID: 4657 RVA: 0x0007D538 File Offset: 0x0007B938
	public void SetReticleCameraEyePosition(float eyePositionX)
	{
		if (this.reticleHolder != null)
		{
			this.reticleHolder.transform.localPosition = eyePositionX * Vector3.left + 0.5f * Vector3.forward;
		}
	}

	// Token: 0x06001232 RID: 4658 RVA: 0x0007D585 File Offset: 0x0007B985
	private void ViewportSizeDidChange()
	{
		this.ResetProjectionMatrix();
	}

	// Token: 0x06001233 RID: 4659 RVA: 0x0007D590 File Offset: 0x0007B990
	private void ResetProjectionMatrix()
	{
		Camera mainCamera = Blocksworld.mainCamera;
		mainCamera.aspect = NormalizedScreen.aspectRatio;
		Matrix4x4 projectionMatrix = mainCamera.projectionMatrix;
		projectionMatrix.m11 = 1f / Mathf.Tan(3.14159274f * this.defaultFoV / 180f);
		projectionMatrix.m00 = projectionMatrix.m11 / mainCamera.aspect;
		mainCamera.projectionMatrix = projectionMatrix;
	}

	// Token: 0x06001234 RID: 4660 RVA: 0x0007D5F5 File Offset: 0x0007B9F5
	public void UpdateChunkSpeeds()
	{
		this.chunkSqrSpeeds = new float[Blocksworld.chunks.Count];
	}

	// Token: 0x06001235 RID: 4661 RVA: 0x0007D60C File Offset: 0x0007BA0C
	public void Store()
	{
		this.storedCameraPos = Blocksworld.cameraTransform.position;
		this.storedCameraRot = Blocksworld.cameraTransform.rotation;
		this.storedTargetPos = this.targetPos;
		this.storedDistance = this.distance;
		this.storedManualCameraDist = this.manualCameraDistance;
	}

	// Token: 0x06001236 RID: 4662 RVA: 0x0007D660 File Offset: 0x0007BA60
	public void Restore()
	{
		Blocksworld.cameraTransform.position = this.storedCameraPos;
		Blocksworld.cameraTransform.rotation = this.storedCameraRot;
		this.targetPos = this.storedTargetPos;
		this.distance = (this.lastDistance = this.storedDistance);
		this.manualCameraDistance = this.storedManualCameraDist;
	}

	// Token: 0x06001237 RID: 4663 RVA: 0x0007D6BA File Offset: 0x0007BABA
	public void StoreOrbitPos()
	{
		this.storedOrbitPos = this.targetPos;
	}

	// Token: 0x06001238 RID: 4664 RVA: 0x0007D6C8 File Offset: 0x0007BAC8
	public void RestoreOrbitPos()
	{
		this.targetPos = this.storedOrbitPos;
	}

	// Token: 0x06001239 RID: 4665 RVA: 0x0007D6D6 File Offset: 0x0007BAD6
	public void Reset()
	{
		this.oldPositionMode = this.positionMode;
		this.positionMode = BlocksworldCamera.CameraPositionMode.DEFAULT;
		this.targetMode = BlocksworldCamera.CameraTargetMode.DEFAULT;
	}

	// Token: 0x0600123A RID: 4666 RVA: 0x0007D6F4 File Offset: 0x0007BAF4
	public void Follow(Bunch bunch)
	{
		this.targetBunch = bunch;
		this.UpdateOrbitDistance(true, (bunch != null) ? (20f + Util.MaxComponent(Util.ComputeBounds(bunch.blocks).size)) : 20f);
		this.UpdateTargetPos();
	}

	// Token: 0x0600123B RID: 4667 RVA: 0x0007D743 File Offset: 0x0007BB43
	public void Follow(Block block)
	{
		this.Follow(block, false);
	}

	// Token: 0x0600123C RID: 4668 RVA: 0x0007D750 File Offset: 0x0007BB50
	public void Follow(Block block, bool auto)
	{
		if (Blocksworld.CurrentState == State.Play)
		{
			if (!auto)
			{
				bool flag = false;
				foreach (KeyValuePair<int, ChunkFollowInfo> keyValuePair in this.targetChunkInfos)
				{
					ChunkFollowInfo chunkFollowInfo = this.targetChunkInfos[keyValuePair.Key];
					if (chunkFollowInfo.auto)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					this.Unfollow();
				}
			}
			Chunk chunk = block.chunk;
			if (chunk.go != null)
			{
				int instanceID = chunk.go.GetInstanceID();
				if (!this.targetChunkInfos.ContainsKey(instanceID))
				{
					this.targetChunkInfos[instanceID] = new ChunkFollowInfo(chunk, auto);
					this.targetChunks.Add(chunk);
				}
				List<Block> list;
				if (this.singletonBlocks.Contains(block))
				{
					list = new List<Block>
					{
						block
					};
				}
				else
				{
					block.UpdateConnectedCache();
					list = Block.connectedCache[block];
				}
				if (list != null)
				{
					if (this.allFollowedBlocks.Count == 0 || !this.allFollowedBlocks.Contains(block))
					{
						List<Block> list2 = new List<Block>();
						for (int i = 0; i < list.Count; i++)
						{
							Block item = list[i];
							if (list.Count <= 1 || !this.singletonBlocks.Contains(item))
							{
								list2.Add(item);
							}
						}
						if (list2.Count == 0)
						{
							list2.Add(list[0]);
						}
						this.modelSizes[this.followedModels] = Util.ComputeBoundsWithSize(list2, true);
						this.modelBlocks[this.followedModels] = block;
						this.followedModels++;
					}
					int j = 0;
					while (j < list.Count)
					{
						Block block2 = list[j];
						if (block2.broken || block2.IsRuntimeInvisible())
						{
							goto IL_2AF;
						}
						if (list.Count <= 1 || !this.singletonBlocks.Contains(block2))
						{
							Chunk chunk2 = block2.chunk;
							GameObject go = chunk2.go;
							if (go != null)
							{
								int instanceID2 = go.GetInstanceID();
								if (!this.targetChunkInfos.ContainsKey(instanceID2))
								{
									this.targetChunkInfos[instanceID2] = new ChunkFollowInfo(chunk2, auto);
									this.targetChunks.Add(chunk2);
								}
							}
							if (block2.TreatAsVehicleLikeBlock())
							{
								this.followingVehicle = true;
								goto IL_2AF;
							}
							goto IL_2AF;
						}
						IL_2BD:
						j++;
						continue;
						IL_2AF:
						this.allFollowedBlocks.Add(block2);
						goto IL_2BD;
					}
				}
			}
		}
		else
		{
			this.targetBlock = block;
			this.UpdateOrbitDistance(true, (block != null) ? (20f + Util.MaxComponent(block.size)) : 20f);
		}
		this.followedBlocks.Add(block);
	}

	// Token: 0x0600123D RID: 4669 RVA: 0x0007DA80 File Offset: 0x0007BE80
	public void ExpandWorldBounds(ITBox b)
	{
		if (Tutorial.state == TutorialState.None)
		{
			Vector3 position = b.GetPosition();
			if ((Blocksworld.cameraTransform.position - position).magnitude < 200f)
			{
				Bounds worldBounds = this.buildCameraWatchdog.GetWorldBounds();
				Vector3 size = worldBounds.size;
				Bounds bounds = new Bounds(position, b.GetScale());
				worldBounds.Encapsulate(bounds);
				if ((worldBounds.size - size).magnitude < 500f)
				{
					this.buildCameraWatchdog.EncapsulateWorldBounds(bounds);
				}
			}
		}
	}

	// Token: 0x0600123E RID: 4670 RVA: 0x0007DB18 File Offset: 0x0007BF18
	public Vector3 GetTargetPosition()
	{
		return this.targetPos;
	}

	// Token: 0x0600123F RID: 4671 RVA: 0x0007DB20 File Offset: 0x0007BF20
	public void SetCameraPosition(Vector3 pos)
	{
		Blocksworld.cameraTransform.position = pos;
	}

	// Token: 0x06001240 RID: 4672 RVA: 0x0007DB2D File Offset: 0x0007BF2D
	public void SetTargetPosition(Vector3 pos)
	{
		this.targetPos = pos;
	}

	// Token: 0x06001241 RID: 4673 RVA: 0x0007DB36 File Offset: 0x0007BF36
	public void SetTargetDistance(float d)
	{
		this.distance = d;
		this.lastDistance = d;
		this.manualCameraDistance = d;
	}

	// Token: 0x06001242 RID: 4674 RVA: 0x0007DB4D File Offset: 0x0007BF4D
	public void SetCameraStill(bool still)
	{
		this.cameraStill = still;
	}

	// Token: 0x06001243 RID: 4675 RVA: 0x0007DB56 File Offset: 0x0007BF56
	public void KnockCameraOver()
	{
		this.broken = true;
	}

	// Token: 0x06001244 RID: 4676 RVA: 0x0007DB5F File Offset: 0x0007BF5F
	private void ResetCameraFollowParameters()
	{
		this.cameraVelResponsiveness = 1f;
		this.cameraFollowAlpha = 0.985f;
		this.velDistanceMultiplier = 0.3f;
		this.blockDirectionFactor = Vector3.zero;
	}

	// Token: 0x06001245 RID: 4677 RVA: 0x0007DB8D File Offset: 0x0007BF8D
	private bool GameCameraDisabled()
	{
		return BW.isUnityEditor && Options.DisableGameCamera;
	}

	// Token: 0x06001246 RID: 4678 RVA: 0x0007DBA4 File Offset: 0x0007BFA4
	private void Execute(BlocksworldCamera.CameraFollowCommand c, bool callFollow = true)
	{
		if (this.GameCameraDisabled())
		{
			return;
		}
		object[] args = c.args;
		if (c.positionMode == BlocksworldCamera.CameraPositionMode.DEFAULT)
		{
			this.cameraVelResponsiveness = ((args.Length <= 0) ? 1f : ((float)args[0]));
			this.cameraFollowAlpha = ((args.Length <= 1) ? 0.95f : ((float)args[1]));
			this.velDistanceMultiplier = ((args.Length <= 2) ? 0.3f : ((float)args[2]));
			this.blockDirectionFactor = ((args.Length <= 3) ? Vector3.zero : ((Vector3)args[3]));
		}
		else if (c.positionMode == BlocksworldCamera.CameraPositionMode.DEFAULT_2D)
		{
			this.cameraVelResponsiveness = ((args.Length <= 0) ? 1f : ((float)args[0]));
			this.cameraFollowAlpha = ((args.Length <= 1) ? 0.985f : ((float)args[1]));
			this.positionMode = BlocksworldCamera.CameraPositionMode.DEFAULT_2D;
			if (this.oldPositionMode != BlocksworldCamera.CameraPositionMode.DEFAULT_2D)
			{
				this.mode2DRotation = Blocksworld.cameraTransform.rotation;
			}
		}
		else if (c.positionMode == BlocksworldCamera.CameraPositionMode.LOOK_TOWARD)
		{
			this.cameraVelResponsiveness = Util.GetFloatArg(args, 0, 1f);
			this.cameraFollowAlpha = Util.GetFloatArg(args, 1, 0.95f);
			this.cameraLookTowardAngles[c.block] = Util.GetFloatArg(args, 2, 0f);
			this.positionMode = BlocksworldCamera.CameraPositionMode.LOOK_TOWARD;
		}
		else if (c.positionMode == BlocksworldCamera.CameraPositionMode.LOOK_TOWARD_TAG)
		{
			this.tryKeepInViewTags.Add(Util.GetStringArg(args, 0, string.Empty));
			this.cameraVelResponsiveness = Util.GetFloatArg(args, 1, 1f);
			this.cameraFollowAlpha = Util.GetFloatArg(args, 2, 0.95f);
			this.positionMode = BlocksworldCamera.CameraPositionMode.LOOK_TOWARD_TAG;
		}
		if (callFollow)
		{
			this.Follow(c.block, false);
		}
	}

	// Token: 0x06001247 RID: 4679 RVA: 0x0007DD89 File Offset: 0x0007C189
	public void CameraFollow(Block block, object[] args)
	{
		if (this.GameCameraDisabled())
		{
			return;
		}
		this.camFollowCommands.Add(new BlocksworldCamera.CameraFollowCommand(block, BlocksworldCamera.CameraPositionMode.DEFAULT, args));
	}

	// Token: 0x06001248 RID: 4680 RVA: 0x0007DDAA File Offset: 0x0007C1AA
	public void CameraFollow2D(Block block, object[] args)
	{
		if (this.GameCameraDisabled())
		{
			return;
		}
		this.camFollowCommands.Add(new BlocksworldCamera.CameraFollowCommand(block, BlocksworldCamera.CameraPositionMode.DEFAULT_2D, args));
	}

	// Token: 0x06001249 RID: 4681 RVA: 0x0007DDCB File Offset: 0x0007C1CB
	public void CameraFollowLookToward(Block block, object[] args)
	{
		if (this.GameCameraDisabled())
		{
			return;
		}
		this.camFollowCommands.Add(new BlocksworldCamera.CameraFollowCommand(block, BlocksworldCamera.CameraPositionMode.LOOK_TOWARD, args));
	}

	// Token: 0x0600124A RID: 4682 RVA: 0x0007DDEC File Offset: 0x0007C1EC
	public void CameraFollowLookTowardTag(Block block, object[] args)
	{
		if (this.GameCameraDisabled())
		{
			return;
		}
		this.camFollowCommands.Add(new BlocksworldCamera.CameraFollowCommand(block, BlocksworldCamera.CameraPositionMode.LOOK_TOWARD_TAG, args));
	}

	// Token: 0x0600124B RID: 4683 RVA: 0x0007DE0D File Offset: 0x0007C20D
	public void CameraFollowThirdPersonPlatform(Block block, object[] args)
	{
	}

	// Token: 0x0600124C RID: 4684 RVA: 0x0007DE0F File Offset: 0x0007C20F
	public void CameraMoveTo(Block block)
	{
		this.CameraMoveTo(block, 0.95f);
	}

	// Token: 0x0600124D RID: 4685 RVA: 0x0007DE20 File Offset: 0x0007C220
	public void CameraToNamedPose(string poseName, float moveAlpha = 0.985f, float aimAlpha = 0.985f, float directionDistance = 15f, bool moveOnly = false)
	{
		if (this.GameCameraDisabled())
		{
			return;
		}
		NamedPose namedPose;
		if (Blocksworld.cameraPosesMap.TryGetValue(poseName, out namedPose))
		{
			this.moveToPos = namedPose.position;
			this.positionMode = BlocksworldCamera.CameraPositionMode.MOVE_TO;
			this.moveToPosAlpha = moveAlpha;
			if (!moveOnly)
			{
				this.lookAtPos = this.moveToPos + namedPose.direction * directionDistance;
				this.targetMode = BlocksworldCamera.CameraTargetMode.LOOK_AT;
				this.lookAtAlpha = aimAlpha;
			}
		}
	}

	// Token: 0x0600124E RID: 4686 RVA: 0x0007DE98 File Offset: 0x0007C298
	private bool MoveToPositionChanged(Vector3 newPos)
	{
		return this.oldPositionMode != BlocksworldCamera.CameraPositionMode.MOVE_TO || (this.moveToPos - newPos).sqrMagnitude > 0.01f;
	}

	// Token: 0x0600124F RID: 4687 RVA: 0x0007DECF File Offset: 0x0007C2CF
	public void CameraMoveTo(Block block, float alpha)
	{
		if (this.GameCameraDisabled())
		{
			return;
		}
		this.moveToPos = block.goT.position;
		this.positionMode = BlocksworldCamera.CameraPositionMode.MOVE_TO;
		this.moveToPosAlpha = alpha;
	}

	// Token: 0x06001250 RID: 4688 RVA: 0x0007DEFC File Offset: 0x0007C2FC
	public void CameraLookAt(Block block)
	{
		this.CameraLookAt(block, 0.95f);
	}

	// Token: 0x06001251 RID: 4689 RVA: 0x0007DF0A File Offset: 0x0007C30A
	public void CameraLookAt(Block block, float alpha)
	{
		if (this.GameCameraDisabled())
		{
			return;
		}
		this.lookAtPos = block.goT.position;
		this.targetMode = BlocksworldCamera.CameraTargetMode.LOOK_AT;
		this.lookAtAlpha = alpha;
	}

	// Token: 0x06001252 RID: 4690 RVA: 0x0007DF38 File Offset: 0x0007C338
	public void Unfollow()
	{
		this.targetBlock = null;
		this.targetBunch = null;
		this.targetChunkInfos.Clear();
		this.targetChunks.Clear();
		this.followedBlocks.Clear();
		this.allFollowedBlocks.Clear();
		this.followedModels = 0;
		this.followingVehicle = false;
		this.targetDistanceMultiplier = 1f;
		this.resetTargetDistanceMultiplierFactor = 1f;
		if (this.chunkSqrSpeeds != null && this.chunkSqrSpeeds.Length == Blocksworld.chunks.Count)
		{
			for (int i = 0; i < Blocksworld.chunks.Count; i++)
			{
				this.chunkSqrSpeeds[i] = 0f;
			}
		}
		this.camFollowCommands.Clear();
	}

	// Token: 0x06001253 RID: 4691 RVA: 0x0007DFF8 File Offset: 0x0007C3F8
	public void Unfollow(Chunk chunk)
	{
		if (chunk.go == null)
		{
			return;
		}
		this.targetChunkInfos.Remove(chunk.go.GetInstanceID());
		this.targetChunks.Remove(chunk);
		this.camFollowCommands.Clear();
		this.prevCamFollowCommands.Clear();
		this.forceCommandExecution = true;
	}

	// Token: 0x06001254 RID: 4692 RVA: 0x0007E058 File Offset: 0x0007C458
	public bool IsFollowing()
	{
		return this.targetBlock != null || this.targetBunch != null || this.targetChunks.Count > 0 || this.positionMode == BlocksworldCamera.CameraPositionMode.MOVE_TO;
	}

	// Token: 0x06001255 RID: 4693 RVA: 0x0007E090 File Offset: 0x0007C490
	private void AutoFollow()
	{
		if (Blocksworld.chunks.Count != this.chunkSqrSpeeds.Length)
		{
			this.UpdateChunkSpeeds();
		}
		Block block = null;
		float num = -1f;
		for (int i = 0; i < Blocksworld.chunks.Count; i++)
		{
			Chunk chunk = Blocksworld.chunks[i];
			Rigidbody rb = chunk.rb;
			if (!(rb == null))
			{
				List<Block> blocks = chunk.blocks;
				bool flag = true;
				bool flag2 = false;
				bool flag3 = false;
				bool flag4 = false;
				foreach (Block block2 in blocks)
				{
					if (!(block2 is BlockPosition) && !block2.IsRuntimeInvisible() && block2.GetMass() > 0f)
					{
						flag = false;
					}
					if (block2 is BlockCharacter || block2 is BlockAnimatedCharacter)
					{
						flag2 = true;
					}
					flag3 |= block2.HasMover();
					flag4 |= block2.HasAnyInputButton();
				}
				if (!flag)
				{
					GameObject go = chunk.go;
					Vector3 position = go.transform.position;
					Vector3 vector = Blocksworld.mainCamera.WorldToViewportPoint(position);
					float num2 = 0f;
					if (!flag3)
					{
						if (flag4)
						{
							num2 = 1f;
						}
						else
						{
							num2 = (this.targetPos - position).magnitude;
						}
					}
					Vector3 a = rb.velocity;
					if (flag2)
					{
						a -= 0.9f * Vector3.up * rb.velocity.y;
					}
					this.chunkSqrSpeeds[i] = this.chunkSqrSpeeds[i] * 0.95f + 0.0500000119f * a.sqrMagnitude;
					if (vector.x > -0.1f && vector.x < 1.1f && vector.y > -0.1f && vector.y < 1.1f && (flag3 || (num2 < 20f && this.chunkSqrSpeeds[i] > 2f)))
					{
						Transform transform = go.transform;
						Collider collider = null;
						if (transform.parent != null)
						{
							collider = transform.parent.gameObject.GetComponent<Collider>();
						}
						if (!(collider != null) || !GeometryUtility.TestPlanesAABB(Blocksworld.frustumPlanes, collider.bounds))
						{
							if (num < 0f || num2 < num)
							{
								block = blocks[0];
								num = num2;
							}
						}
					}
				}
			}
		}
		if (block != null)
		{
			this.Follow(block, true);
		}
	}

	// Token: 0x06001256 RID: 4694 RVA: 0x0007E37C File Offset: 0x0007C77C
	public void UpdateOrbitDistance(bool useMaxDist = false, float maxDist = 20f)
	{
		this.lastDistance = this.distance;
		this.UpdateTargetPos();
		this.distance = (Blocksworld.cameraTransform.position - (this.targetPos + this.GetScreenPlacementWorldError(this.targetDistanceMultiplier))).magnitude;
		if (useMaxDist)
		{
			this.distance = Mathf.Min(this.distance, maxDist);
		}
	}

	// Token: 0x06001257 RID: 4695 RVA: 0x0007E3E7 File Offset: 0x0007C7E7
	public void RestoreOrbitDistance()
	{
		this.distance = this.lastDistance;
	}

	// Token: 0x06001258 RID: 4696 RVA: 0x0007E3F8 File Offset: 0x0007C7F8
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
		this.SetFilteredPositionAndTarget();
	}

	// Token: 0x06001259 RID: 4697 RVA: 0x0007E495 File Offset: 0x0007C895
	public void SetFilteredPositionAndTarget()
	{
		this.camPosFiltered = Blocksworld.cameraTransform.position;
		this.targetPosFiltered = this.camPosFiltered + Blocksworld.cameraTransform.forward * 10f;
	}

	// Token: 0x0600125A RID: 4698 RVA: 0x0007E4CC File Offset: 0x0007C8CC
	public void LateUpdate()
	{
		Transform cameraTiltTransform = Blocksworld.bw.cameraTiltTransform;
		if (cameraTiltTransform == null)
		{
			return;
		}
		if (Blocksworld.vrEnabled)
		{
			cameraTiltTransform.localRotation = Quaternion.identity;
			return;
		}
		if (this.screenTiltRotationTracking)
		{
			cameraTiltTransform.localRotation = Quaternion.Slerp(cameraTiltTransform.localRotation, this.screenTiltRotation, 10f * Time.deltaTime);
		}
		else
		{
			cameraTiltTransform.localRotation = Quaternion.Slerp(cameraTiltTransform.localRotation, Quaternion.identity, 5f * Time.deltaTime);
		}
	}

	// Token: 0x0600125B RID: 4699 RVA: 0x0007E55C File Offset: 0x0007C95C
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
			BlocksworldCamera.camVelocity = position - this.lastCameraPos;
			this.moveDist = BlocksworldCamera.camVelocity.magnitude;
			this.smoothedMoveDist = 0.95f * this.smoothedMoveDist + 0.05f * this.moveDist;
			this.lastCameraPos = position;
		}
		Vector3 a = this.targetPos;
		this.UpdateTargetPos();
		bool flag = this.IsFollowing();
		if (Blocksworld.interpolateRigidBodies && Blocksworld.CurrentState == State.Play && flag && (a - this.targetPos).sqrMagnitude > 0f)
		{
			this.MoveTowardsTarget();
			this.AimTowardsTarget(0.92f);
		}
		if (this.firstPersonBlock != null)
		{
			bool flag2 = this.firstPersonBlock.IsFixed();
			flag2 |= (this.firstPersonCharacter != null && this.firstPersonCharacter.unmoving);
			flag2 |= (this.firstPersonAnimatedCharacter != null && this.firstPersonAnimatedCharacter.unmoving);
			Quaternion rotation = cameraTransform.rotation;
			Transform transform = this.firstPersonBlock.go.transform;
			Vector3 vector;
			Vector3 worldPosition;
			if (this.firstPersonAnimatedCharacter != null)
			{
				vector = transform.position + 0.85f * transform.up;
				worldPosition = vector + transform.forward;
			}
			else
			{
				vector = transform.position + 0.5f * transform.up;
				worldPosition = vector + 5f * transform.forward;
			}
			cameraTransform.position = vector;
			if (flag2)
			{
				cameraTransform.rotation = Quaternion.Slerp(transform.rotation, this.firstPersonLookAngle, 0.01f);
			}
			else
			{
				cameraTransform.LookAt(worldPosition, Vector3.up);
				cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, rotation, 0.9f);
				cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, this.firstPersonLookAngle, 0.05f);
			}
			this.firstPersonPos = cameraTransform.position;
			this.firstPersonLookAngle = cameraTransform.rotation;
			float f = Math.Max(0f, Vector3.Dot(this.firstPersonBlock.goT.forward, this.firstPersonBlockLastForward));
			this.firstPersonSmoothForward = Quaternion.Slerp(this.firstPersonBlock.goT.rotation, this.firstPersonSmoothForward, Mathf.Pow(f, 2f));
			this.firstPersonBlockLastForward = this.firstPersonBlock.goT.forward;
			cameraTransform.position = this.firstPersonPos + 0.2f * this.immediateOffset;
			this.actualFpcTilt = Mathf.Lerp(this.actualFpcTilt, this.fpcTilt, 0.05f);
			if (this.actualFpcTilt < 0.001f && this.fpcTilt == 0f)
			{
				this.actualFpcTilt = 0f;
			}
			else
			{
				cameraTransform.rotation *= Quaternion.Euler(this.actualFpcTilt, 0f, 0f);
			}
			if (this.firstPersonMode > 0)
			{
				this.firstPersonLook = Vector3.Lerp(this.firstPersonLook, this.firstPersonLookOffset, 0.05f);
				if (this.firstPersonMode == 2)
				{
					this.firstPersonLook.x = 0f;
				}
				Vector3 vector2 = -0.4f * (cameraTransform.rotation * this.firstPersonLook);
				if (flag2)
				{
					vector2 *= 0.1f;
				}
				cameraTransform.LookAt(cameraTransform.position + 120f * cameraTransform.forward + vector2, cameraTransform.up);
			}
		}
		else
		{
			float x = -3f * MappedInput.InputAxis(MappableInput.AXIS2_X);
			float y = -3f * MappedInput.InputAxis(MappableInput.AXIS2_Y);
			if (Blocksworld.CurrentState == State.Play && !Blocksworld.UI.Controls.IsDPadActive("R"))
			{
				this.OrbitBy(new Vector2(x, y));
			}
		}
		this.UpdateSpeedFoV();
		this.UpdateLightColorTint();
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
			Blocksworld.directionalLight.transform.rotation = cameraTransform.rotation * this.lightRotation;
		}
		if (Blocksworld.CurrentState == State.Build && Tutorial.state == TutorialState.None)
		{
			this.buildCameraWatchdog.Update();
		}
	}

	// Token: 0x0600125C RID: 4700 RVA: 0x0007EB00 File Offset: 0x0007CF00
	public void CameraStateLoaded()
	{
		this.targetPos = Blocksworld.cameraTransform.position + Blocksworld.cameraTransform.forward * this.manualCameraDistance;
		this.distance = this.manualCameraDistance;
		this.currentManualCameraDistance = this.manualCameraDistance;
		this.currentManualCameraAngle = this.manualCameraAngle;
		this.currentCameraOffsetVec = Vector3.one;
		this.storedOrbitPos = this.targetPos;
		this.buildCameraWatchdog.Reset(true);
		List<Block> list = BWSceneManager.AllBlocks();
		Bounds defaultWorldBounds = this.buildCameraWatchdog.GetDefaultWorldBounds();
		for (int i = 0; i < list.Count; i++)
		{
			Block block = list[i];
			if (!(block is BlockWater) && !(block is BlockSky))
			{
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
		}
		this.buildCameraWatchdog.SetWorldBounds(defaultWorldBounds);
	}

	// Token: 0x0600125D RID: 4701 RVA: 0x0007EC34 File Offset: 0x0007D034
	public void HideLayer(int layer)
	{
		int cullingMask = Blocksworld.mainCamera.cullingMask;
		Blocksworld.mainCamera.cullingMask = (cullingMask & ~(1 << layer));
	}

	// Token: 0x0600125E RID: 4702 RVA: 0x0007EC60 File Offset: 0x0007D060
	private void UpdateTargetPos()
	{
		if (Blocksworld.CurrentState == State.FrameCapture)
		{
			return;
		}
		Vector3 vector = this.targetPos;
		if (this.targetChunkInfos.Count > 0)
		{
			Vector3 a = default(Vector3);
			int num = 0;
			foreach (Chunk chunk in this.targetChunks)
			{
				if (chunk.go != null)
				{
					Vector3 position = chunk.GetPosition();
					a += position;
					num++;
				}
			}
			if (num > 0)
			{
				this.targetPos = a / (float)num;
			}
		}
		else if (this.targetBlock != null)
		{
			this.targetPos = this.targetBlock.GetPosition();
		}
		else if (this.targetBunch != null)
		{
			this.targetPos = this.targetBunch.GetPosition();
		}
		if (Util.IsNullVector3(this.targetPos))
		{
			this.targetPos = vector;
		}
	}

	// Token: 0x0600125F RID: 4703 RVA: 0x0007ED78 File Offset: 0x0007D178
	private bool ExecuteFollowCommands()
	{
		bool flag = true;
		if (this.prevCamFollowCommands.Count == this.camFollowCommands.Count)
		{
			flag = false;
			for (int i = 0; i < this.prevCamFollowCommands.Count; i++)
			{
				BlocksworldCamera.CameraFollowCommand cameraFollowCommand = this.prevCamFollowCommands[i];
				BlocksworldCamera.CameraFollowCommand cameraFollowCommand2 = this.camFollowCommands[i];
				if (cameraFollowCommand2.block != cameraFollowCommand.block || cameraFollowCommand2.positionMode != cameraFollowCommand.positionMode)
				{
					flag = true;
					break;
				}
			}
		}
		flag = (flag || this.forceCommandExecution);
		if (flag)
		{
			this.targetBlock = null;
			this.targetBunch = null;
			this.targetChunkInfos.Clear();
			this.targetChunks.Clear();
			this.followedBlocks.Clear();
			this.allFollowedBlocks.Clear();
			this.followedModels = 0;
			this.followingVehicle = false;
			this.targetDistanceMultiplier = 1f;
			this.resetTargetDistanceMultiplierFactor = 1f;
			this.modelSizes.Clear();
			this.modelBlocks.Clear();
			this.cameraLookTowardAngles.Clear();
			this.forceCommandExecution = false;
		}
		for (int j = 0; j < this.camFollowCommands.Count; j++)
		{
			this.Execute(this.camFollowCommands[j], flag);
		}
		if (this.camFollowCommands.Count > 0 && Blocksworld.CurrentState == State.Play)
		{
			this.UpdateTargetPos();
		}
		this.prevCamFollowCommands.Clear();
		this.prevCamFollowCommands.AddRange(this.camFollowCommands);
		this.camFollowCommands.Clear();
		return flag;
	}

	// Token: 0x06001260 RID: 4704 RVA: 0x0007EF1D File Offset: 0x0007D31D
	public void SetReticleEnabled(bool enabled)
	{
		if (this.reticleHolder != null)
		{
			this.reticleHolder.SetActive(enabled && -1 != this.hudReticle);
		}
	}

	// Token: 0x06001261 RID: 4705 RVA: 0x0007EF50 File Offset: 0x0007D350
	public void FixedUpdate()
	{
		if (this.hudReticle == -1 && null != this.currentReticle)
		{
			this.currentReticle.enabled = false;
			this.currentReticle = null;
			this.reticleHolder.SetActive(false);
		}
		this.hudReticle = -1;
		if (this.broken)
		{
			return;
		}
		this.ExecuteFollowCommands();
		Transform cameraTransform = Blocksworld.cameraTransform;
		Vector3 position = cameraTransform.position;
		Quaternion rotation = cameraTransform.rotation;
		if (Blocksworld.CurrentState == State.Play)
		{
			if (this.targetChunkInfos.Count > 0)
			{
				HashSet<Chunk> hashSet = new HashSet<Chunk>();
				foreach (ChunkFollowInfo chunkFollowInfo in this.targetChunkInfos.Values)
				{
					if (chunkFollowInfo.auto)
					{
						if (chunkFollowInfo.chunk.rb != null && chunkFollowInfo.chunk.rb.IsSleeping())
						{
							hashSet.Add(chunkFollowInfo.chunk);
						}
						else
						{
							bool flag = true;
							foreach (Block block in chunkFollowInfo.chunk.blocks)
							{
								if (!block.IsRuntimeInvisible())
								{
									flag = false;
									break;
								}
							}
							if (flag)
							{
								hashSet.Add(chunkFollowInfo.chunk);
							}
						}
					}
				}
				foreach (Chunk chunk in hashSet)
				{
					this.Unfollow(chunk);
				}
			}
			bool flag2 = this.IsFollowing();
			if (flag2 && this.positionMode == BlocksworldCamera.CameraPositionMode.MOVE_TO && this.firstPersonBlock != null)
			{
				flag2 = false;
			}
			if (!flag2 && !this.autoFollowDisabled && this.firstPersonBlock == null && !this.GameCameraDisabled())
			{
				this.AutoFollow();
				this.UpdateTargetPos();
			}
			if ((flag2 || this.positionMode == BlocksworldCamera.CameraPositionMode.MOVE_TO) && !Blocksworld.interpolateRigidBodies)
			{
				this.MoveTowardsTarget();
			}
			if ((flag2 || this.targetMode == BlocksworldCamera.CameraTargetMode.LOOK_AT) && !Blocksworld.interpolateRigidBodies)
			{
				this.AimTowardsTarget(0.92f);
			}
			bool flag3 = this.positionMode != BlocksworldCamera.CameraPositionMode.MOVE_TO;
			if (flag3)
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
				float d = 1f - num;
				Vector3 b = d * this.zoomOffset;
				this.zoomOffset -= b;
				Vector3 vector = d * this.panOffset;
				this.panOffset -= vector;
				Vector3 b2 = Vector3.zero;
				if (this.orbitPosDiff.sqrMagnitude > 0.0001f)
				{
					Vector2 vector2 = d * this.orbitPosDiff;
					this.orbitPosDiff -= vector2;
					Vector3 position2 = cameraTransform.transform.position;
					this.DoOrbit(vector2);
					b2 = cameraTransform.position - position2;
				}
				Vector3 vector3 = vector + b;
				if (flag2)
				{
					this.camPosFiltered += vector3;
					cameraTransform.position += vector3;
				}
				else
				{
					this.camPosFiltered += vector3 + b2;
					cameraTransform.position = this.camPosFiltered + this.immediateOffset;
				}
				this.targetPos += vector;
				this.targetPosFiltered += vector;
				if (b.sqrMagnitude > 0.0001f)
				{
					this.UpdateOrbitDistance(false, 20f);
					this.currentManualCameraDistance = this.distance;
					this.currentCameraOffsetVec = this.currentCameraOffsetVec.normalized * this.currentManualCameraDistance;
					if (this.followedModels == 0)
					{
						this.targetPos += b;
						this.targetPosFiltered += b;
					}
				}
			}
		}
		else
		{
			this.manualCameraFraction = 0f;
			this.camPosFiltered = Blocksworld.cameraTransform.position;
			this.targetPosFiltered = this.camPosFiltered + Blocksworld.cameraTransform.forward * 10f;
		}
		this.tryKeepInViewTags.Clear();
		this.cameraStill = false;
		this.ResetCameraFollowParameters();
		this.targetCameraAngle = 70f;
		this.verticalDistanceOffsetFactor = 1f;
		this.targetFollowDistanceMultiplier = 1f;
		this.immediateOffset = Vector3.zero;
		this.forceDirectionHint = Vector3.zero;
		if (this.firstPersonBlock != null)
		{
			cameraTransform.position = position;
			cameraTransform.rotation = rotation;
		}
		this.screenTiltRotationTracking = this.screenTiltRotationSet;
		this.screenTiltRotationSet = false;
	}

	// Token: 0x06001262 RID: 4706 RVA: 0x0007F4C8 File Offset: 0x0007D8C8
	public void Focus()
	{
		int num = 100;
		Transform cameraTransform = Blocksworld.cameraTransform;
		Vector3 position = cameraTransform.position;
		while (--num >= 0)
		{
			if ((this.targetPos - (position + this.distance * cameraTransform.forward)).sqrMagnitude <= 0.1f)
			{
				break;
			}
			this.MoveTowardsTarget();
		}
	}

	// Token: 0x06001263 RID: 4707 RVA: 0x0007F534 File Offset: 0x0007D934
	private void DoOrbit(Vector2 posDiff)
	{
		Transform cameraTransform = Blocksworld.cameraTransform;
		cameraTransform.RotateAround(this.targetPos, Vector3.up, -0.5f * posDiff.x);
		cameraTransform.RotateAround(this.targetPos, cameraTransform.right, 0.25f * posDiff.y);
		Vector3 point = this.targetPos + this.GetScreenPlacementWorldError(this.targetDistanceMultiplier);
		if (Blocksworld.CurrentState == State.Play)
		{
			float num = 4f;
			float num2 = Vector3.Angle(cameraTransform.forward, -Vector3.up);
			if (num2 < num)
			{
				cameraTransform.RotateAround(point, cameraTransform.right, -(num - num2));
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
			cameraTransform.RotateAround(point, cameraTransform.right, -num7);
		}
		Vector3 from = -cameraTransform.forward.normalized;
		float manualCameraAngle = Vector3.Angle(from, Vector3.up);
		if (Blocksworld.CurrentState == State.Play)
		{
			this.currentManualCameraAngle = manualCameraAngle;
		}
		else
		{
			this.manualCameraAngle = manualCameraAngle;
			this.manualCameraDistance = this.distance;
		}
		this.manualCameraFraction = 1f;
		this.mode2DRotation = cameraTransform.rotation;
		if (Blocksworld.CurrentState != State.Play)
		{
			this.MoveTowardsTarget();
		}
	}

	// Token: 0x06001264 RID: 4708 RVA: 0x0007F730 File Offset: 0x0007DB30
	public void OrbitBy(Vector2 posDiff)
	{
		if (this.positionMode == BlocksworldCamera.CameraPositionMode.MOVE_TO)
		{
			return;
		}
		if (Blocksworld.CurrentState == State.Play)
		{
			this.orbitPosDiff += posDiff;
		}
		else
		{
			this.DoOrbit(posDiff);
		}
	}

	// Token: 0x06001265 RID: 4709 RVA: 0x0007F768 File Offset: 0x0007DB68
	public void HardOrbit(Vector2 posDiff)
	{
		Transform cameraTransform = Blocksworld.cameraTransform;
		cameraTransform.RotateAround(this.targetPos, Vector3.up, -posDiff.x);
		cameraTransform.RotateAround(this.targetPos, cameraTransform.right, posDiff.y);
	}

	// Token: 0x06001266 RID: 4710 RVA: 0x0007F7B0 File Offset: 0x0007DBB0
	private void MoveTowardsTargetMoveToMode()
	{
		Transform cameraTransform = Blocksworld.cameraTransform;
		float num = this.moveToPosAlpha;
		float d = 1f - num;
		this.camPosFiltered = num * this.camPosFiltered + d * (this.moveToPos + this.moveToOffset);
		cameraTransform.position = this.camPosFiltered + this.immediateOffset;
		Vector3 from = -cameraTransform.forward.normalized;
		float num2 = Vector3.Angle(from, Vector3.up);
		this.currentManualCameraAngle = num2;
	}

	// Token: 0x06001267 RID: 4711 RVA: 0x0007F844 File Offset: 0x0007DC44
	private bool OkCameraPosition(Vector3 newPos)
	{
		return Util.CameraVisibilityCheck(newPos, this.targetPosFiltered, this.allFollowedBlocks, true, null) && !Util.PointWithinTerrain(newPos, true);
	}

	// Token: 0x06001268 RID: 4712 RVA: 0x0007F878 File Offset: 0x0007DC78
	public static Vector3 GetLookTowardAngleDirection(Block b, float angle)
	{
		Quaternion rotation = b.goT.rotation;
		Quaternion rotation2 = (Blocksworld.CurrentState != State.Play) ? rotation : b.playRotation;
		Vector3 point = Vector3.forward;
		bool flag = false;
		if (b.HasPreferredLookTowardAngleLocalVector())
		{
			Vector3 preferredLookTowardAngleLocalVector = b.GetPreferredLookTowardAngleLocalVector();
			Vector3 lhs = rotation2 * preferredLookTowardAngleLocalVector;
			if (Mathf.Abs(Vector3.Dot(lhs, Vector3.up)) < 0.1f)
			{
				point = preferredLookTowardAngleLocalVector;
				flag = true;
			}
		}
		if (!flag)
		{
			for (int i = 0; i < BlocksworldCamera.possibleForward.Length; i++)
			{
				Vector3 vector = BlocksworldCamera.possibleForward[i];
				Vector3 lhs2 = rotation2 * vector;
				if (Mathf.Abs(Vector3.Dot(lhs2, Vector3.up)) < 0.1f)
				{
					point = vector;
					break;
				}
			}
		}
		Vector3 point2 = rotation * point;
		Vector3 vec = Quaternion.AngleAxis(angle, Vector3.up) * point2;
		return Util.ProjectOntoPlane(vec, Vector3.up).normalized;
	}

	// Token: 0x06001269 RID: 4713 RVA: 0x0007F984 File Offset: 0x0007DD84
	private void MoveTowardsTargetDefaultMode(bool mode2d = false, bool useLookTowardAngle = false)
	{
		Transform cameraTransform = Blocksworld.cameraTransform;
		Vector3 vector = cameraTransform.position - this.immediateOffset;
		Vector3 a = this.targetPos - this.oldTargetPos;
		this.oldTargetPos = this.targetPos;
		float magnitude = a.magnitude;
		float num = magnitude;
		this.filteredTargetVel = 0.1f * a + 0.9f * this.filteredTargetVel;
		if (magnitude > 2f)
		{
			a = default(Vector3);
		}
		float num2 = Mathf.Min(this.filteredTargetVel.magnitude, 1f);
		Vector3 vector2 = vector - this.targetPos;
		Vector3 normalized = vector2.normalized;
		bool flag = Util.PointWithinTerrain(this.targetPos, false);
		bool flag2 = this.IsFollowing();
		float num3 = this.cameraFollowAlpha;
		bool flag3 = false;
		if (this.allFollowedBlocks.Count == 1)
		{
			Block block = this.modelBlocks[0];
			Chunk chunk = block.chunk;
			Rigidbody rb = chunk.rb;
			if (rb == null || rb.isKinematic)
			{
				flag3 = true;
			}
		}
		float num5;
		if (this.followedModels == 1 && !mode2d && !flag3)
		{
			float num4 = 50f;
			num5 = this.targetFollowDistanceMultiplier * this.targetDistanceMultiplier * Mathf.Max(2.3f * Util.MaxComponent(this.modelSizes[0].size), 12f);
			if (num5 > num4)
			{
				num5 = num4 + 2f * Mathf.Sqrt(num5 - num4);
			}
		}
		else if (this.followedModels == 0 || mode2d || flag3)
		{
			num5 = this.currentManualCameraDistance + num2 * this.currentManualCameraDistance * this.velDistanceMultiplier;
		}
		else
		{
			Bounds bounds = default(Bounds);
			for (int i = 0; i < this.followedModels; i++)
			{
				Bounds bounds2 = this.modelSizes[i];
				bounds2.center = this.modelBlocks[i].goT.position;
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
			Vector3[] array = new Vector3[]
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
			this.multiFollowTargetDistanceFactor = 0.95f * this.multiFollowTargetDistanceFactor + 0.05f * num6;
			num5 = this.targetFollowDistanceMultiplier * Mathf.Max(this.multiFollowTargetDistanceFactor * Util.MaxComponent(bounds.size), 12f);
		}
		float num7 = 50f;
		if (mode2d)
		{
			num7 *= 0.25f;
		}
		if (this.followedModels > 1)
		{
			this.forceDirectionHint = Vector3.zero;
		}
		else if (this.forceDirectionHint.sqrMagnitude > num7 * num7)
		{
			this.forceDirectionHint = this.forceDirectionHint.normalized * num7;
		}
		this.filteredForceDirectionHint = 0.05f * this.forceDirectionHint + 0.95f * this.filteredForceDirectionHint;
		Vector3 a4;
		if (mode2d)
		{
			Vector3 a2 = this.mode2DRotation * Vector3.forward;
			Vector3 a3 = this.targetPos;
			a4 = a3 - num5 * a2;
			Vector3 vector3 = a * 100f + this.filteredForceDirectionHint * 1f;
			bool flag4 = GeometryUtility.TestPlanesAABB(Blocksworld.frustumPlanes, new Bounds(a3 * 0.5f, Vector3.one));
			float d = (!flag4) ? 1f : 0.33f;
			Vector3 center = a3 - vector3 * d;
			int num8 = 0;
			while (num8 < 6 && !GeometryUtility.TestPlanesAABB(Blocksworld.frustumPlanes, new Bounds(center, Vector3.one)))
			{
				vector3 *= 0.85f;
				center = a3 - vector3 * d;
				num8++;
			}
			a4 += vector3;
			float magnitude2 = vector3.magnitude;
			if (magnitude2 > 1f)
			{
				float num9 = Mathf.Clamp(vector3.magnitude / num5, 0f, 1f);
				float num10 = 1f - 0.01f * num9;
				num3 *= num10;
			}
		}
		else
		{
			float num11 = 5f;
			if (this.filteredTargetVel.magnitude < 4f)
			{
				float num12 = this.filteredTargetVel.magnitude / 4f;
				num12 *= num12;
				num11 *= num12;
			}
			int num13 = 0;
			Vector3 a5 = Vector3.zero;
			Vector3 vector4 = normalized / this.cameraVelResponsiveness;
			if (useLookTowardAngle)
			{
				Vector3 vector5 = Vector3.zero;
				if (this.cameraLookTowardAngles.Count > 0)
				{
					foreach (KeyValuePair<Block, float> keyValuePair in this.cameraLookTowardAngles)
					{
						vector5 += BlocksworldCamera.GetLookTowardAngleDirection(keyValuePair.Key, keyValuePair.Value);
					}
				}
				else
				{
					foreach (string tagName in this.tryKeepInViewTags)
					{
						Block block2;
						if (TagManager.TryGetClosestBlockWithTag(tagName, this.targetPosFiltered, out block2, null))
						{
							Vector3 position = block2.goT.position;
							Vector3 vec = this.targetPosFiltered - position;
							vector5 += Util.ProjectOntoPlane(vec, Vector3.up).normalized;
							num13++;
							a5 += position;
						}
					}
					if (num13 > 0)
					{
						a5 /= (float)num13;
					}
				}
				if (vector5.sqrMagnitude > 0.01f)
				{
					vector5.Normalize();
					Vector3 normalized2 = Util.ProjectOntoPlane(vector4, Vector3.up).normalized;
					vector5 = Vector3.RotateTowards(normalized2, vector5, 2f, 100f);
					vector4 += vector5 * 5f;
				}
				else
				{
					vector4 = vector4 - a * num11 - this.filteredForceDirectionHint;
				}
			}
			else
			{
				vector4 = vector4 - a * num11 - this.filteredForceDirectionHint;
			}
			if (this.blockDirectionFactor != Vector3.zero)
			{
				Vector3 vector6 = Vector3.zero;
				foreach (Block block3 in this.followedBlocks)
				{
					Vector3 b = block3.goT.rotation * this.blockDirectionFactor;
					vector6 += b;
				}
				if (vector6.sqrMagnitude > 0.01f)
				{
					vector4 += vector6;
				}
			}
			if (this.followedModels <= 1 && num13 == 0)
			{
				Vector3 rhs = Util.ProjectOntoPlane(cameraTransform.forward, Vector3.up);
				Vector3 a6 = Util.ProjectOntoPlane(this.filteredForceDirectionHint, Vector3.up);
				Vector3 a7 = Util.ProjectOntoPlane(this.filteredTargetVel, Vector3.up);
				Vector3 lhs = a7 + 0.2f * a6;
				float num14 = -Vector3.Dot(lhs, rhs);
				if (num14 > 0f)
				{
					num14 = Mathf.Min(0.2f, num14);
					Vector3 rhs2 = Util.ProjectOntoPlane(cameraTransform.right, Vector3.up);
					int num15 = (int)Mathf.Sign(Vector3.Dot(lhs, rhs2));
					Vector3 b2 = -Vector3.Cross(lhs, Vector3.up).normalized * (float)num15 * num14 * 20f / (vector2.magnitude * 0.2f);
					vector4 += b2;
				}
			}
			if ((double)vector4.sqrMagnitude < 0.01)
			{
				vector4 += normalized * 0.1f;
			}
			Vector3 normalized3 = vector4.normalized;
			Vector3 normalized4 = Vector3.Cross(normalized3, Vector3.up).normalized;
			Quaternion rotation = Quaternion.AngleAxis(-this.currentManualCameraAngle, normalized4);
			Vector3 normalized5 = (rotation * Vector3.up).normalized;
			a4 = this.targetPosFiltered + normalized5 * num5;
			float num16 = Mathf.Clamp(0.45f + num * 10f, 0f, 1f);
			a4 = a4 * num16 + vector * (1f - num16);
			float magnitude3 = this.filteredTargetVel.magnitude;
			num3 = Mathf.Clamp(this.cameraFollowAlpha - magnitude3 * 0.02f, 0.85f, 1f);
			if (!flag3)
			{
				float num17 = this.targetCameraAngle;
				if (num13 > 0)
				{
					float num18 = Util.AngleBetween(a5 - vector, Util.ProjectOntoPlane(cameraTransform.forward, Vector3.up), cameraTransform.right);
					if (num18 < 0f)
					{
						num17 = this.targetCameraAngle + Mathf.Clamp(num18 + 20f, -30f, 0f);
					}
					else
					{
						num17 = this.targetCameraAngle + Mathf.Min(num18, 30f);
					}
				}
				this.currentManualCameraAngle = this.manualCameraFraction * this.currentManualCameraAngle + (1f - this.manualCameraFraction) * (0.1f * this.currentManualCameraAngle + 0.9f * num17);
			}
		}
		float d2 = 1f - num3;
		Vector3 a8 = vector;
		if (flag2 && !mode2d && this.targetDistanceMultiplier < 1f)
		{
			float num19 = this.targetDistanceMultiplier * this.resetTargetDistanceMultiplierFactor;
			Vector3 vector7 = this.targetPos + this.lookAtOffset + this.GetScreenPlacementWorldError(num19);
			Vector3 normalized6 = (vector - vector7).normalized;
			Vector3 from = vector7 + normalized6 * num5 * (num19 / this.targetDistanceMultiplier);
			if (Util.CameraVisibilityCheck(from, vector7, this.allFollowedBlocks, false, null))
			{
				this.resetTargetDistanceMultiplierFactor = 0.99f * this.resetTargetDistanceMultiplierFactor + 0.0104999989f;
				this.targetDistanceMultiplier *= this.resetTargetDistanceMultiplierFactor;
			}
			else
			{
				this.resetTargetDistanceMultiplierFactor = 1f;
			}
		}
		this.targetDistanceMultiplier = Mathf.Clamp(this.targetDistanceMultiplier, 0.05f, 1f);
		this.camPosFiltered = this.manualCameraFraction * a8 + (1f - this.manualCameraFraction) * (num3 * this.camPosFiltered + d2 * (a4 + this.moveToOffset));
		Vector3 a9 = this.camInitSlowdown * vector + (1f - this.camInitSlowdown) * this.camPosFiltered;
		if (!float.IsNaN(a9.x) && !float.IsNaN(a9.y) && !float.IsNaN(a9.z))
		{
			Quaternion rotation2 = cameraTransform.rotation;
			cameraTransform.position = a9 + this.immediateOffset;
			cameraTransform.rotation = rotation2;
		}
		this.manualCameraFraction *= 0.98f;
		this.camInitSlowdown *= 0.99f;
	}

	// Token: 0x0600126A RID: 4714 RVA: 0x000806D4 File Offset: 0x0007EAD4
	private void MoveTowardsTarget()
	{
		if (this.cameraStill)
		{
			return;
		}
		if (Blocksworld.CurrentState == State.Play)
		{
			if (this.firstPersonBlock != null)
			{
				return;
			}
			switch (this.positionMode)
			{
			case BlocksworldCamera.CameraPositionMode.DEFAULT:
			case BlocksworldCamera.CameraPositionMode.DEFAULT_2D:
			case BlocksworldCamera.CameraPositionMode.LOOK_TOWARD:
			case BlocksworldCamera.CameraPositionMode.LOOK_TOWARD_TAG:
				this.MoveTowardsTargetDefaultMode(this.positionMode == BlocksworldCamera.CameraPositionMode.DEFAULT_2D, this.positionMode == BlocksworldCamera.CameraPositionMode.LOOK_TOWARD || this.positionMode == BlocksworldCamera.CameraPositionMode.LOOK_TOWARD_TAG);
				break;
			case BlocksworldCamera.CameraPositionMode.MOVE_TO:
				this.MoveTowardsTargetMoveToMode();
				break;
			}
		}
		else
		{
			Transform cameraTransform = Blocksworld.cameraTransform;
			Vector3 position = cameraTransform.position;
			Vector3 a = this.targetPos - (position + this.distance * cameraTransform.forward);
			cameraTransform.position += 1.5f * a / this.distance;
			this.camPosFiltered = cameraTransform.position;
		}
	}

	// Token: 0x0600126B RID: 4715 RVA: 0x000807CC File Offset: 0x0007EBCC
	private Vector3 GetScreenPlacementWorldError(float distanceMultiplier)
	{
		if (Blocksworld.CurrentState != State.Play || this.followedModels == 0 || (this.followedModels == 1 && !this.followingVehicle))
		{
			return Vector3.zero;
		}
		Transform cameraTransform = Blocksworld.cameraTransform;
		Vector3 b = Util.WorldToScreenPoint(this.targetPos, false);
		float d = Util.WorldToScreenPoint(this.targetPos + cameraTransform.up, false).y - b.y;
		float num = 0.2f;
		if (this.followedModels > 1)
		{
			num = 0.3f;
		}
		Vector3 a = new Vector3((float)NormalizedScreen.width, (float)NormalizedScreen.height * num, 0f);
		Vector3 a2 = -(a - b).y * cameraTransform.up / d;
		if (distanceMultiplier < 1f)
		{
			float num2 = 0f;
			for (int i = 0; i < this.followedModels; i++)
			{
				num2 = Mathf.Max(num2, Util.MaxComponent(this.modelSizes[i].size));
			}
			float d2 = 1f - distanceMultiplier;
			a2 += Vector3.up * num2 * d2;
		}
		return this.verticalDistanceOffsetFactor * a2;
	}

	// Token: 0x0600126C RID: 4716 RVA: 0x00080924 File Offset: 0x0007ED24
	private void AimTowardsTarget(float alpha = 0.92f)
	{
		if (!this.cameraStill)
		{
			Transform cameraTransform = Blocksworld.cameraTransform;
			if (this.targetMode == BlocksworldCamera.CameraTargetMode.DEFAULT)
			{
				Vector3 screenPlacementWorldError = this.GetScreenPlacementWorldError(this.targetDistanceMultiplier);
				Vector3 vector = this.targetPos + this.lookAtOffset + screenPlacementWorldError;
				float magnitude = (this.targetPosFiltered - vector).magnitude;
				float magnitude2 = (cameraTransform.position - vector).magnitude;
				float num = magnitude / magnitude2;
				alpha = Mathf.Clamp(0.95f - num * 0.15f, 0.8f, 1f);
				this.targetPosFiltered = alpha * this.targetPosFiltered + (1f - alpha) * vector;
			}
			else
			{
				alpha = this.lookAtAlpha;
				this.targetPosFiltered = alpha * this.targetPosFiltered + (1f - alpha) * (this.lookAtPos + this.lookAtOffset);
			}
			if (this.positionMode != BlocksworldCamera.CameraPositionMode.DEFAULT_2D)
			{
				Vector3 vector2 = Vector3.up;
				if (this.screenTiltAngle != 0f)
				{
					vector2 = Quaternion.AngleAxis(this.screenTiltAngle, Util.ProjectOntoPlane(cameraTransform.forward, Vector3.up).normalized) * vector2;
				}
				cameraTransform.LookAt(this.targetPosFiltered + this.immediateOffset, vector2);
			}
		}
	}

	// Token: 0x0600126D RID: 4717 RVA: 0x00080A94 File Offset: 0x0007EE94
	private bool FollowingAuto()
	{
		if (this.targetChunkInfos.Count > 0)
		{
			bool result = true;
			foreach (KeyValuePair<int, ChunkFollowInfo> keyValuePair in this.targetChunkInfos)
			{
				if (!this.targetChunkInfos[keyValuePair.Key].auto)
				{
					result = false;
					break;
				}
			}
			return result;
		}
		return false;
	}

	// Token: 0x0600126E RID: 4718 RVA: 0x00080B24 File Offset: 0x0007EF24
	public void PanBy(Vector2 diff)
	{
		if (this.positionMode == BlocksworldCamera.CameraPositionMode.MOVE_TO)
		{
			return;
		}
		Vector3 position = Blocksworld.cameraTransform.position;
		bool flag = Blocksworld.CurrentState == State.Play;
		float d = (!flag) ? (0.025f * Mathf.Clamp(0.12f * (position.y - 5f), 1f, 20f)) : 0.03f;
		Vector3 right = Blocksworld.cameraTransform.right;
		Vector3 a = Util.ProjectOntoPlane(Blocksworld.cameraTransform.forward, Vector3.up);
		if (a.magnitude > 0.1f)
		{
			a.Normalize();
		}
		else
		{
			a = Util.ProjectOntoPlane(Blocksworld.cameraTransform.up, Vector3.up);
			a.Normalize();
		}
		Vector3 b = d * (diff.y * a + diff.x * right);
		if (Blocksworld.CurrentState != State.Play)
		{
			Blocksworld.cameraTransform.position += b;
			this.targetPos += b;
			this.targetPosFiltered += b;
		}
		else
		{
			this.panOffset += b;
		}
	}

	// Token: 0x0600126F RID: 4719 RVA: 0x00080C6E File Offset: 0x0007F06E
	public void ZoomBy(float diff)
	{
		this.ZoomBy(diff, 0f);
	}

	// Token: 0x06001270 RID: 4720 RVA: 0x00080C7C File Offset: 0x0007F07C
	public void ZoomBy(float diff, float posDiff)
	{
		if (this.positionMode == BlocksworldCamera.CameraPositionMode.MOVE_TO)
		{
			return;
		}
		Transform cameraTransform = Blocksworld.cameraTransform;
		Vector3 vector = 0.1f * diff * cameraTransform.forward;
		if (Options.RelativeZoom && this.IsFollowing())
		{
			float magnitude = (cameraTransform.position - this.targetPos).magnitude;
			vector *= magnitude / 15f;
		}
		if (Blocksworld.CurrentState != State.Play)
		{
			Vector3 vector2 = cameraTransform.position + vector;
			float magnitude2 = (this.targetPos - vector2).magnitude;
			float num = 1f;
			if (diff < 0f || magnitude2 > 2f * num)
			{
				cameraTransform.position = vector2;
				this.camPosFiltered = vector2;
			}
			else if (TBox.selected == null || (!this.IsFollowing() && Blocksworld.CurrentState == State.Play))
			{
				Vector3 point = this.targetPos + vector;
				if (!Util.PointWithinTerrain(point, false))
				{
					this.targetPos = point;
					cameraTransform.position = vector2;
				}
			}
			this.UpdateOrbitDistance(false, 20f);
			float num2 = Mathf.Abs(diff);
			float num3 = Mathf.Abs(posDiff);
			float num4 = 1f;
			float num5 = 2f;
			if (Blocksworld.CurrentState == State.FrameCapture || num3 >= num4 || num2 > num5)
			{
			}
			this.manualCameraFraction = 1f;
			this.manualCameraDistance = this.distance;
		}
		else
		{
			this.zoomOffset += vector;
		}
	}

	// Token: 0x06001271 RID: 4721 RVA: 0x00080E13 File Offset: 0x0007F213
	public Color GetLightTint()
	{
		return this.lightTint;
	}

	// Token: 0x06001272 RID: 4722 RVA: 0x00080E1C File Offset: 0x0007F21C
	private void UpdateLightColorTint()
	{
		Vector3 position = Blocksworld.cameraTransform.position;
		int num;
		int num2;
		Util.PointWithinTerrain(position, out num, out num2, false);
		int num3 = num - num2;
		if (num3 != this.prevHitDiff)
		{
			if (num3 == 0 || Blocksworld.CurrentState == State.Play)
			{
				this.lightTint = Color.white;
			}
			else
			{
				this.lightTint = Util.Color(200f, 100f, 100f, 120f);
			}
			Blocksworld.UpdateLightColor(true);
		}
		this.prevHitDiff = num3;
	}

	// Token: 0x06001273 RID: 4723 RVA: 0x00080EA4 File Offset: 0x0007F2A4
	public void AddForceDirectionHint(Chunk chunk, Vector3 force)
	{
		if (this.targetChunks.Contains(chunk))
		{
			this.forceDirectionHint += force;
		}
	}

	// Token: 0x06001274 RID: 4724 RVA: 0x00080EC9 File Offset: 0x0007F2C9
	public void ChunkDirty(Chunk chunk)
	{
		if (this.targetChunks.Contains(chunk))
		{
			this.forceCommandExecution = true;
		}
	}

	// Token: 0x06001275 RID: 4725 RVA: 0x00080EE3 File Offset: 0x0007F2E3
	public void SetLookAtOffset(Vector3 offset)
	{
		this.lookAtOffset = offset;
	}

	// Token: 0x06001276 RID: 4726 RVA: 0x00080EEC File Offset: 0x0007F2EC
	public void SetMoveToOffset(Vector3 offset)
	{
		this.moveToOffset = offset;
	}

	// Token: 0x06001277 RID: 4727 RVA: 0x00080EF5 File Offset: 0x0007F2F5
	public void AddImmediateOffset(Vector3 offset)
	{
		this.immediateOffset += offset;
	}

	// Token: 0x06001278 RID: 4728 RVA: 0x00080F0C File Offset: 0x0007F30C
	public void UpdateSpeedFoV()
	{
		if (this.currentSpeedFoV == this.desiredSpeedFoV && this.desiredSpeedFoV == 0f)
		{
			return;
		}
		this.currentSpeedFoV = 0.9f * this.currentSpeedFoV + 0.1f * this.desiredSpeedFoV;
		if (Math.Abs(this.currentSpeedFoV - this.desiredSpeedFoV) < 0.0001f)
		{
			this.currentSpeedFoV = this.desiredSpeedFoV;
		}
		float d = 1f;
		float num = 0.35f * this.currentSpeedFoV;
		float num2 = this.currentSpeedFoV;
		if (this.smoothedMoveDist < num)
		{
			d = 0f;
		}
		else if (this.smoothedMoveDist < num2)
		{
			d = (this.smoothedMoveDist - num) / (num2 - num);
		}
		Blocksworld.cameraTransform.position += d * Blocksworld.cameraTransform.forward;
	}

	// Token: 0x06001279 RID: 4729 RVA: 0x00080FF1 File Offset: 0x0007F3F1
	public void EnableAutoFollow(bool e)
	{
		this.autoFollowDisabled = !e;
	}

	// Token: 0x0600127A RID: 4730 RVA: 0x00080FFD File Offset: 0x0007F3FD
	public void SetTargetCameraAngle(float a)
	{
		this.targetCameraAngle = a;
	}

	// Token: 0x0600127B RID: 4731 RVA: 0x00081006 File Offset: 0x0007F406
	public void SetVerticalDistanceOffsetFactor(float f)
	{
		this.verticalDistanceOffsetFactor = f;
	}

	// Token: 0x0600127C RID: 4732 RVA: 0x0008100F File Offset: 0x0007F40F
	public void SetTargetFollowDistanceMultiplier(float m)
	{
		this.targetFollowDistanceMultiplier = m;
	}

	// Token: 0x0600127D RID: 4733 RVA: 0x00081018 File Offset: 0x0007F418
	public void SetSingleton(Block block, bool s)
	{
		if (s)
		{
			this.singletonBlocks.Add(block);
		}
		else
		{
			this.singletonBlocks.Remove(block);
		}
	}

	// Token: 0x0600127E RID: 4734 RVA: 0x0008103F File Offset: 0x0007F43F
	public static Vector3 GetCamVelocity()
	{
		return BlocksworldCamera.camVelocity;
	}

	// Token: 0x0600127F RID: 4735 RVA: 0x00081048 File Offset: 0x0007F448
	public void SetScreenTiltRotation(Quaternion tiltRotation)
	{
		if (!this.screenTiltRotationTracking)
		{
			this.screenTiltBaseRotation = tiltRotation;
			this.camTiltBaseRotation = Blocksworld.cameraTransform.rotation;
		}
		this.screenTiltRotation = Quaternion.Inverse(this.screenTiltBaseRotation) * tiltRotation;
		this.screenTiltRotationSet = true;
		this.screenTiltRotationTracking = true;
	}

	// Token: 0x06001280 RID: 4736 RVA: 0x0008109C File Offset: 0x0007F49C
	public void AddKeepInViewTag(string t)
	{
		this.tryKeepInViewTags.Add(t);
	}

	// Token: 0x06001281 RID: 4737 RVA: 0x000810AB File Offset: 0x0007F4AB
	public void FirstPersonFollow(Block newBlock, int mode)
	{
		this.desiredFirstPersonBlock = newBlock;
		this.firstPersonMode = mode;
	}

	// Token: 0x06001282 RID: 4738 RVA: 0x000810BC File Offset: 0x0007F4BC
	public void FinalUpdateFirstPersonFollow()
	{
		this.UpdateDesiredFollowBlock();
		this.desiredFirstPersonBlock = null;
		if (this.firstPersonCharacter == null && this.firstPersonAnimatedCharacter == null)
		{
			return;
		}
		Transform cameraTransform = Blocksworld.cameraTransform;
		Vector3 position = cameraTransform.position;
		if ((this.firstPersonCharacter == null || !this.firstPersonCharacter.unmoving) && (this.firstPersonAnimatedCharacter == null || !this.firstPersonAnimatedCharacter.unmoving))
		{
			RaycastHit[] array = Physics.RaycastAll(position, cameraTransform.forward, this.maxAimAdjustRange);
			if (array.Length > 0)
			{
				Util.SmartSort(array, Blocksworld.cameraTransform.position);
				foreach (RaycastHit raycastHit in array)
				{
					Vector3 forward = cameraTransform.InverseTransformPoint(raycastHit.point);
					if (forward.magnitude >= this.minAimAdjustRange)
					{
						this.aimAdjustOffset = Quaternion.Slerp(this.aimAdjustOffset, Quaternion.Slerp(Quaternion.identity, Quaternion.LookRotation(forward), 0.15f), 0.05f);
						break;
					}
				}
			}
			else
			{
				this.aimAdjustOffset = Quaternion.Slerp(this.aimAdjustOffset, Quaternion.identity, 0.1f);
			}
		}
	}

	// Token: 0x06001283 RID: 4739 RVA: 0x00081204 File Offset: 0x0007F604
	private void UpdateDesiredFollowBlock()
	{
		if (this.desiredFirstPersonBlock == this.firstPersonBlock)
		{
			return;
		}
		if (this.firstPersonBlock != null && this.desiredFirstPersonBlock == null)
		{
			this.StopFirstPersonFollow(this.firstPersonBlock);
			return;
		}
		bool flag = this.desiredFirstPersonBlock != null && (this.desiredFirstPersonBlock.broken || this.desiredFirstPersonBlock.vanished);
		if (flag)
		{
			if (this.firstPersonBlock != null)
			{
				this.StopFirstPersonFollow(this.firstPersonBlock);
			}
			return;
		}
		if (this.firstPersonBlock != null)
		{
			this.StopFirstPersonFollow(this.firstPersonBlock);
		}
		this.firstPersonBlock = this.desiredFirstPersonBlock;
		this.firstPersonCharacter = (this.desiredFirstPersonBlock as BlockCharacter);
		this.firstPersonAnimatedCharacter = (this.desiredFirstPersonBlock as BlockAnimatedCharacter);
		if (this.firstPersonAnimatedCharacter != null)
		{
			this.firstPersonHead = this.firstPersonAnimatedCharacter.GetHeadAttach();
		}
		this.firstPersonLookAngle = this.firstPersonBlock.go.transform.rotation;
		this.firstPersonPos = this.firstPersonBlock.go.transform.position;
		this.firstPersonSmoothForward = this.firstPersonBlock.goT.rotation;
		this.firstPersonBlock.SetFPCGearVisible(false);
		if (this.firstPersonCharacter != null && !this.firstPersonCharacter.unmoving)
		{
			this.firstPersonPos += 0.5f * this.firstPersonCharacter.go.transform.up;
		}
		if (null != this.firstPersonHead)
		{
			this.firstPersonPos = this.firstPersonHead.transform.position;
		}
		if ((this.firstPersonCharacter == null || !this.firstPersonCharacter.unmoving) && (this.firstPersonAnimatedCharacter == null || !this.firstPersonAnimatedCharacter.unmoving))
		{
			for (int i = 0; i < this.firstPersonBlock.connections.Count; i++)
			{
				Block block = this.firstPersonBlock.connections[i];
				if (!block.isTerrain && block.go.activeSelf)
				{
					if (!(this.firstPersonHead != null) || !(block.go.transform.parent != this.firstPersonHead.transform))
					{
						if (block.go.transform.localPosition.y >= 0.4f && block.go.transform.localPosition.y < 0.75f && !block.IsRuntimeInvisible())
						{
							BlocksworldCamera.HeadgearInfo item = default(BlocksworldCamera.HeadgearInfo);
							item.block = block;
							item.localPos = block.go.transform.localPosition;
							item.localRot = block.go.transform.localRotation;
							item.parent = block.go.transform.parent;
							this.firstPersonHeadgear.Add(item);
							block.EnableCollider(false);
							block.go.transform.SetParent(Blocksworld.cameraTransform);
							block.go.transform.rotation = Blocksworld.cameraTransform.rotation;
							block.go.transform.position = Blocksworld.cameraTransform.position;
						}
					}
				}
			}
		}
		if (this.firstPersonMode == 1)
		{
			HashSet<Predicate> manyPreds = new HashSet<Predicate>
			{
				BlockCharacter.predicateCharacterMover
			};
			if (this.firstPersonBlock.ContainsTileWithAnyPredicateInPlayMode2(manyPreds))
			{
				this.firstPersonMode = 2;
			}
		}
		Blocksworld.cameraTransform.position = this.firstPersonPos;
		Blocksworld.cameraTransform.rotation = this.firstPersonBlock.go.transform.rotation;
		this.moveDist = 0f;
		this.smoothedMoveDist = 0f;
		this.firstPersonDpad = Vector2.zero;
		this.firstPersonRotation = 0f;
		this.aimAdjustOffset = Quaternion.identity;
		this.firstPersonLook = (this.firstPersonLookOffset = Vector3.zero);
	}

	// Token: 0x06001284 RID: 4740 RVA: 0x00081644 File Offset: 0x0007FA44
	public void StopFirstPersonFollow(Block oldBlock)
	{
		if (this.firstPersonBlock == null)
		{
			BWLog.Info("Attempting to stop First Person mode when it was never started");
			return;
		}
		if (this.firstPersonBlock != oldBlock)
		{
			BWLog.Info("First person wasn't old block, not ending");
			return;
		}
		this.firstPersonBlock.SetFPCGearVisible(true);
		foreach (BlocksworldCamera.HeadgearInfo headgearInfo in this.firstPersonHeadgear)
		{
			headgearInfo.block.go.transform.SetParent(headgearInfo.parent);
			headgearInfo.block.go.transform.localPosition = headgearInfo.localPos;
			headgearInfo.block.go.transform.localRotation = headgearInfo.localRot;
			headgearInfo.block.EnableCollider(true);
		}
		this.firstPersonHeadgear.Clear();
		this.firstPersonBlock = null;
		this.firstPersonCharacter = null;
		this.firstPersonHead = null;
		this.Unfollow();
		Transform cameraTransform = Blocksworld.cameraTransform;
		cameraTransform.position = oldBlock.go.transform.position - 15f * oldBlock.go.transform.forward;
		cameraTransform.LookAt(oldBlock.go.transform.position);
		this.camPosFiltered = cameraTransform.position;
		this.targetPos = oldBlock.go.transform.position;
		this.targetPosFiltered = this.targetPos;
		this.distance = 3f;
		this.lastDistance = this.distance;
		this.moveToOffset = Vector3.zero;
		this.lookAtOffset = Vector3.zero;
		this.panOffset = Vector3.zero;
		this.zoomOffset = Vector3.zero;
		this.orbitPosDiff = Vector2.zero;
		this.positionMode = BlocksworldCamera.CameraPositionMode.DEFAULT;
		this.targetMode = BlocksworldCamera.CameraTargetMode.DEFAULT;
		this.currentCameraOffsetVec = Vector3.one;
		this.filteredTargetVel = Vector3.zero;
		this.oldTargetPos = this.targetPos;
		this.screenTiltRotationSet = (this.screenTiltRotationTracking = false);
		this.currentManualCameraAngle = this.manualCameraAngle;
		this.currentManualCameraDistance = this.manualCameraDistance;
		this.manualCameraFraction = 0f;
		this.camInitSlowdown = 1f;
		this.DoOrbit(Vector2.zero);
		this.targetPos = oldBlock.go.transform.position;
		this.targetPosFiltered = this.targetPos;
		this.aimAdjustOffset = Quaternion.identity;
	}

	// Token: 0x06001285 RID: 4741 RVA: 0x000818D0 File Offset: 0x0007FCD0
	public void SetHudReticle(int reticle)
	{
		this.hudReticle = reticle;
		Renderer y = (this.hudReticle < 0 || this.hudReticle >= this.hudDisplayObjects.Count) ? null : this.hudDisplayObjects[this.hudReticle].GetComponent<Renderer>();
		if (this.currentReticle == y)
		{
			return;
		}
		if (null != this.currentReticle)
		{
			this.currentReticle.enabled = false;
		}
		if (null == y)
		{
			return;
		}
		this.currentReticle = y;
		this.currentReticle.enabled = true;
		this.reticleHolder.SetActive(true);
	}

	// Token: 0x06001286 RID: 4742 RVA: 0x00081980 File Offset: 0x0007FD80
	public Vector3 GetAimAdjustTarget(Block sender)
	{
		if (this.firstPersonCharacter == null || sender.chunk != this.firstPersonBlock.chunk || this.firstPersonCharacter.unmoving)
		{
			return Vector3.zero;
		}
		return Blocksworld.cameraTransform.position + 20f * (this.aimAdjustOffset * Blocksworld.cameraTransform.forward);
	}

	// Token: 0x06001287 RID: 4743 RVA: 0x000819F2 File Offset: 0x0007FDF2
	public void HandleTeleport(Block b)
	{
		if (!this.IsFollowing())
		{
			return;
		}
		this.Reset();
	}

	// Token: 0x06001288 RID: 4744 RVA: 0x00081A06 File Offset: 0x0007FE06
	public bool IsFirstPerson()
	{
		return this.firstPersonBlock != null;
	}

	// Token: 0x06001289 RID: 4745 RVA: 0x00081A14 File Offset: 0x0007FE14
	public Transform GetFirstPersonHead()
	{
		return (!(this.firstPersonHead != null)) ? null : this.firstPersonHead.transform;
	}

	// Token: 0x04000E69 RID: 3689
	private List<BlocksworldCamera.CameraFollowCommand> camFollowCommands = new List<BlocksworldCamera.CameraFollowCommand>();

	// Token: 0x04000E6A RID: 3690
	private List<BlocksworldCamera.CameraFollowCommand> prevCamFollowCommands = new List<BlocksworldCamera.CameraFollowCommand>();

	// Token: 0x04000E6B RID: 3691
	private BuildCameraWatchdog buildCameraWatchdog = new BuildCameraWatchdog();

	// Token: 0x04000E6C RID: 3692
	private bool forceCommandExecution;

	// Token: 0x04000E6D RID: 3693
	private HashSet<Block> singletonBlocks = new HashSet<Block>();

	// Token: 0x04000E6E RID: 3694
	private HashSet<string> tryKeepInViewTags = new HashSet<string>();

	// Token: 0x04000E6F RID: 3695
	private Dictionary<Block, float> cameraLookTowardAngles = new Dictionary<Block, float>();

	// Token: 0x04000E70 RID: 3696
	private const float MAX_FORCE_DIRECTION_HINT = 50f;

	// Token: 0x04000E71 RID: 3697
	private const float MAX_TARGET_POS_CHANGE = 2f;

	// Token: 0x04000E72 RID: 3698
	private const float MIN_CAMERA_CORRECTION_DISTANCE = 3f;

	// Token: 0x04000E73 RID: 3699
	private const float MANUAL_CAMERA_FRACTION_THRESHOLD = 0.1f;

	// Token: 0x04000E74 RID: 3700
	private const float DEFAULT_MANUAL_CAMERA_SMOOTHNESS = 0.8f;

	// Token: 0x04000E75 RID: 3701
	private const float MIN_MANUAL_CAMERA_SMOOTHNESS = 0.001f;

	// Token: 0x04000E76 RID: 3702
	private const float MAX_MANUAL_CAMERA_SMOOTHNESS = 0.999f;

	// Token: 0x04000E77 RID: 3703
	private float screenTiltAngle;

	// Token: 0x04000E78 RID: 3704
	private bool screenTiltAngleSet;

	// Token: 0x04000E79 RID: 3705
	private Quaternion screenTiltBaseRotation;

	// Token: 0x04000E7A RID: 3706
	private Quaternion screenTiltRotation;

	// Token: 0x04000E7B RID: 3707
	private Quaternion camTiltBaseRotation;

	// Token: 0x04000E7C RID: 3708
	private bool screenTiltRotationSet;

	// Token: 0x04000E7D RID: 3709
	private bool screenTiltRotationTracking;

	// Token: 0x04000E7E RID: 3710
	private static Vector3 camVelocity = Vector3.zero;

	// Token: 0x04000E7F RID: 3711
	private float multiFollowTargetDistanceFactor = 1.2f;

	// Token: 0x04000E80 RID: 3712
	private float targetDistanceMultiplier = 1f;

	// Token: 0x04000E81 RID: 3713
	private float resetTargetDistanceMultiplierFactor = 1f;

	// Token: 0x04000E82 RID: 3714
	private Vector3 currentCameraOffsetVec = Vector3.forward * 15f;

	// Token: 0x04000E83 RID: 3715
	private Quaternion mode2DRotation = default(Quaternion);

	// Token: 0x04000E84 RID: 3716
	private Vector3 lookAtOffset = Vector3.zero;

	// Token: 0x04000E85 RID: 3717
	private Vector3 moveToOffset = Vector3.zero;

	// Token: 0x04000E86 RID: 3718
	private Vector3 immediateOffset = Vector3.zero;

	// Token: 0x04000E87 RID: 3719
	private float targetCameraAngle = 70f;

	// Token: 0x04000E88 RID: 3720
	private float verticalDistanceOffsetFactor = 1f;

	// Token: 0x04000E89 RID: 3721
	private float targetFollowDistanceMultiplier = 1f;

	// Token: 0x04000E8A RID: 3722
	private float _manualCameraDistance = 15f;

	// Token: 0x04000E8B RID: 3723
	private float _manualCameraHeight = 15f;

	// Token: 0x04000E8C RID: 3724
	private float _manualCameraAngle = 70f;

	// Token: 0x04000E8D RID: 3725
	public Quaternion lightRotation = Quaternion.Euler(40f, 20f, 0f);

	// Token: 0x04000E8E RID: 3726
	private bool broken;

	// Token: 0x04000E8F RID: 3727
	private BlocksworldCamera.CameraPositionMode positionMode;

	// Token: 0x04000E90 RID: 3728
	private BlocksworldCamera.CameraTargetMode targetMode;

	// Token: 0x04000E91 RID: 3729
	private BlocksworldCamera.CameraPositionMode oldPositionMode;

	// Token: 0x04000E92 RID: 3730
	private Vector3 moveToPos = Vector3.zero;

	// Token: 0x04000E93 RID: 3731
	private float moveToPosAlpha = 0.99f;

	// Token: 0x04000E94 RID: 3732
	private Vector3 lookAtPos = Vector3.zero;

	// Token: 0x04000E95 RID: 3733
	private float lookAtAlpha = 0.99f;

	// Token: 0x04000E96 RID: 3734
	private Vector3 panOffset = Vector3.zero;

	// Token: 0x04000E97 RID: 3735
	private Vector3 zoomOffset = Vector3.zero;

	// Token: 0x04000E98 RID: 3736
	private Vector2 orbitPosDiff = Vector2.zero;

	// Token: 0x04000E99 RID: 3737
	private Vector3 filteredForceDirectionHint = Vector3.zero;

	// Token: 0x04000E9A RID: 3738
	private Vector3 forceDirectionHint = Vector3.zero;

	// Token: 0x04000E9B RID: 3739
	private Bunch targetBunch;

	// Token: 0x04000E9C RID: 3740
	private Block targetBlock;

	// Token: 0x04000E9D RID: 3741
	private Bunch previousTargetBunch;

	// Token: 0x04000E9E RID: 3742
	private Block previousTargetBlock;

	// Token: 0x04000E9F RID: 3743
	private int followedModels;

	// Token: 0x04000EA0 RID: 3744
	private bool followingVehicle;

	// Token: 0x04000EA1 RID: 3745
	private Dictionary<int, Bounds> modelSizes = new Dictionary<int, Bounds>();

	// Token: 0x04000EA2 RID: 3746
	private Dictionary<int, Block> modelBlocks = new Dictionary<int, Block>();

	// Token: 0x04000EA3 RID: 3747
	private Dictionary<int, ChunkFollowInfo> targetChunkInfos = new Dictionary<int, ChunkFollowInfo>();

	// Token: 0x04000EA4 RID: 3748
	private HashSet<Chunk> targetChunks = new HashSet<Chunk>();

	// Token: 0x04000EA5 RID: 3749
	private HashSet<Block> followedBlocks = new HashSet<Block>();

	// Token: 0x04000EA6 RID: 3750
	private HashSet<Block> allFollowedBlocks = new HashSet<Block>();

	// Token: 0x04000EA7 RID: 3751
	private Vector3 targetPos;

	// Token: 0x04000EA8 RID: 3752
	private Vector3 oldTargetPos;

	// Token: 0x04000EA9 RID: 3753
	private Vector3 filteredTargetVel;

	// Token: 0x04000EAA RID: 3754
	private Vector3 storedOrbitPos;

	// Token: 0x04000EAB RID: 3755
	private float distance = 20f;

	// Token: 0x04000EAC RID: 3756
	private float lastDistance = 20f;

	// Token: 0x04000EAD RID: 3757
	private Vector3 targetPosFiltered;

	// Token: 0x04000EAE RID: 3758
	private Vector3 camPosFiltered;

	// Token: 0x04000EAF RID: 3759
	private bool cameraStill;

	// Token: 0x04000EB0 RID: 3760
	private float manualCameraFraction;

	// Token: 0x04000EB1 RID: 3761
	private float camInitSlowdown = 1f;

	// Token: 0x04000EB2 RID: 3762
	private float currentManualCameraDistance = 15f;

	// Token: 0x04000EB3 RID: 3763
	private float currentManualCameraAngle = 70f;

	// Token: 0x04000EB4 RID: 3764
	private Vector3 storedTargetPos;

	// Token: 0x04000EB5 RID: 3765
	private Vector3 storedCameraPos;

	// Token: 0x04000EB6 RID: 3766
	private Quaternion storedCameraRot;

	// Token: 0x04000EB7 RID: 3767
	private float storedManualCameraDist = 15f;

	// Token: 0x04000EB8 RID: 3768
	private float storedDistance;

	// Token: 0x04000EB9 RID: 3769
	private float[] chunkSqrSpeeds;

	// Token: 0x04000EBA RID: 3770
	private bool autoFollowDisabled;

	// Token: 0x04000EBB RID: 3771
	private float cameraVelResponsiveness = 1f;

	// Token: 0x04000EBC RID: 3772
	private float cameraFollowAlpha = 0.985f;

	// Token: 0x04000EBD RID: 3773
	private float velDistanceMultiplier = 0.3f;

	// Token: 0x04000EBE RID: 3774
	private Vector3 blockDirectionFactor = Vector3.zero;

	// Token: 0x04000EBF RID: 3775
	private Color lightTint = Color.white;

	// Token: 0x04000EC0 RID: 3776
	private int prevHitDiff;

	// Token: 0x04000EC1 RID: 3777
	private float moveDist;

	// Token: 0x04000EC2 RID: 3778
	private float smoothedMoveDist;

	// Token: 0x04000EC3 RID: 3779
	private Vector3 lastCameraPos = Vector3.zero;

	// Token: 0x04000EC4 RID: 3780
	private float defaultFoV = 22.5f;

	// Token: 0x04000EC5 RID: 3781
	public float speedFoV = 22.5f;

	// Token: 0x04000EC6 RID: 3782
	public float minSpeedFoV = 22.5f;

	// Token: 0x04000EC7 RID: 3783
	public float maxSpeedFoV = 35f;

	// Token: 0x04000EC8 RID: 3784
	public float currentSpeedFoV;

	// Token: 0x04000EC9 RID: 3785
	public float desiredSpeedFoV;

	// Token: 0x04000ECA RID: 3786
	private Block desiredFirstPersonBlock;

	// Token: 0x04000ECB RID: 3787
	public Block firstPersonBlock;

	// Token: 0x04000ECC RID: 3788
	public BlockCharacter firstPersonCharacter;

	// Token: 0x04000ECD RID: 3789
	public BlockAnimatedCharacter firstPersonAnimatedCharacter;

	// Token: 0x04000ECE RID: 3790
	public GameObject firstPersonHead;

	// Token: 0x04000ECF RID: 3791
	private Vector3 firstPersonPos;

	// Token: 0x04000ED0 RID: 3792
	private Quaternion firstPersonLookAngle;

	// Token: 0x04000ED1 RID: 3793
	public Quaternion firstPersonSmoothForward;

	// Token: 0x04000ED2 RID: 3794
	private Vector3 firstPersonBlockLastForward;

	// Token: 0x04000ED3 RID: 3795
	public Vector2 firstPersonDpad;

	// Token: 0x04000ED4 RID: 3796
	public float firstPersonRotation;

	// Token: 0x04000ED5 RID: 3797
	public Vector3 firstPersonDeadZone;

	// Token: 0x04000ED6 RID: 3798
	public float firstPersonTurnScale = 1f;

	// Token: 0x04000ED7 RID: 3799
	public float firstPersonTorque = 0.5f;

	// Token: 0x04000ED8 RID: 3800
	public float fpcTilt;

	// Token: 0x04000ED9 RID: 3801
	protected float actualFpcTilt;

	// Token: 0x04000EDA RID: 3802
	protected Quaternion aimAdjustOffset = Quaternion.identity;

	// Token: 0x04000EDB RID: 3803
	private float maxAimAdjustRange = 50f;

	// Token: 0x04000EDC RID: 3804
	private float minAimAdjustRange = 10f;

	// Token: 0x04000EDD RID: 3805
	public int firstPersonMode;

	// Token: 0x04000EDE RID: 3806
	public Vector3 firstPersonLookOffset = Vector3.zero;

	// Token: 0x04000EDF RID: 3807
	private Vector3 firstPersonLook = Vector3.zero;

	// Token: 0x04000EE0 RID: 3808
	public bool firstPersonLookXFlip;

	// Token: 0x04000EE1 RID: 3809
	public bool firstPersonLookYFlip;

	// Token: 0x04000EE2 RID: 3810
	private GameObject reticleHolder;

	// Token: 0x04000EE3 RID: 3811
	private int hudReticle = -1;

	// Token: 0x04000EE4 RID: 3812
	public Renderer currentReticle;

	// Token: 0x04000EE5 RID: 3813
	private List<GameObject> hudDisplayObjects;

	// Token: 0x04000EE6 RID: 3814
	private List<BlocksworldCamera.HeadgearInfo> firstPersonHeadgear = new List<BlocksworldCamera.HeadgearInfo>();

	// Token: 0x04000EE7 RID: 3815
	private const float CHUNK_SPEED_ESTIMATION_ALPHA = 0.95f;

	// Token: 0x04000EE8 RID: 3816
	private static Vector3[] possibleForward = new Vector3[]
	{
		-Vector3.forward,
		Vector3.up
	};

	// Token: 0x020000FB RID: 251
	private enum CameraPositionMode
	{
		// Token: 0x04000EEA RID: 3818
		DEFAULT,
		// Token: 0x04000EEB RID: 3819
		DEFAULT_2D,
		// Token: 0x04000EEC RID: 3820
		LOOK_TOWARD,
		// Token: 0x04000EED RID: 3821
		LOOK_TOWARD_TAG,
		// Token: 0x04000EEE RID: 3822
		MOVE_TO
	}

	// Token: 0x020000FC RID: 252
	private enum CameraTargetMode
	{
		// Token: 0x04000EF0 RID: 3824
		DEFAULT,
		// Token: 0x04000EF1 RID: 3825
		LOOK_AT
	}

	// Token: 0x020000FD RID: 253
	private class CameraFollowCommand
	{
		// Token: 0x0600128B RID: 4747 RVA: 0x00081A76 File Offset: 0x0007FE76
		public CameraFollowCommand(Block block, BlocksworldCamera.CameraPositionMode positionMode, object[] args)
		{
			this.block = block;
			this.positionMode = positionMode;
			this.args = args;
		}

		// Token: 0x04000EF2 RID: 3826
		public Block block;

		// Token: 0x04000EF3 RID: 3827
		public BlocksworldCamera.CameraPositionMode positionMode;

		// Token: 0x04000EF4 RID: 3828
		public object[] args;
	}

	// Token: 0x020000FE RID: 254
	private struct HeadgearInfo
	{
		// Token: 0x04000EF5 RID: 3829
		public Block block;

		// Token: 0x04000EF6 RID: 3830
		public Vector3 localPos;

		// Token: 0x04000EF7 RID: 3831
		public Quaternion localRot;

		// Token: 0x04000EF8 RID: 3832
		public Transform parent;
	}
}
