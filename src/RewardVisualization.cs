using System;
using System.Collections;
using System.Collections.Generic;
using Blocks;
using SimpleJSON;
using UnityEngine;

// Token: 0x02000276 RID: 630
public class RewardVisualization
{
	// Token: 0x06001D43 RID: 7491 RVA: 0x000CEFC0 File Offset: 0x000CD3C0
	private static Vector3 GetRotAxis()
	{
		if (RewardVisualization.rewardSpinModel != null)
		{
			Vector3 vector = new Vector3(1f, 7f, 1f);
			return vector.normalized;
		}
		Vector3 vector2 = new Vector3(1f, 3f, 1f);
		return vector2.normalized;
	}

	// Token: 0x06001D44 RID: 7492 RVA: 0x000CF018 File Offset: 0x000CD418
	private static bool SamePaints(Block b1, Block b2)
	{
		for (int i = 0; i < b1.subMeshGameObjects.Count + 1; i++)
		{
			if (b1.GetBuildModePaint(i) != b2.GetBuildModePaint(i))
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x06001D45 RID: 7493 RVA: 0x000CF060 File Offset: 0x000CD460
	private static bool SameTextures(Block b1, Block b2)
	{
		for (int i = 0; i < b1.subMeshGameObjects.Count + 1; i++)
		{
			if (b1.GetBuildModeTexture(i) != b2.GetBuildModeTexture(i))
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x06001D46 RID: 7494 RVA: 0x000CF0A8 File Offset: 0x000CD4A8
	private static List<Block> GetFirstPossibleWorldModel()
	{
		List<Block> list = BWSceneManager.AllBlocks();
		for (int i = 0; i < list.Count; i++)
		{
			Block block = list[i];
			if (!block.isTerrain)
			{
				return Block.connectedCache[block];
			}
		}
		return null;
	}

	// Token: 0x06001D47 RID: 7495 RVA: 0x000CF0F4 File Offset: 0x000CD4F4
	private static Block FindMatchingWorldModelBlock(Block firstBlock)
	{
		string b = firstBlock.BlockType();
		Vector3 v = firstBlock.Scale();
		List<Block> list = BWSceneManager.AllBlocks();
		for (int i = 0; i < list.Count; i++)
		{
			Block block = list[i];
			if (!block.isTerrain && block.BlockType() == b && Vector3.Distance(Util.Abs(block.Scale()), Util.Abs(v)) < 0.01f && RewardVisualization.SameTextures(block, firstBlock) && RewardVisualization.SamePaints(block, firstBlock))
			{
				return block;
			}
		}
		return null;
	}

	// Token: 0x06001D48 RID: 7496 RVA: 0x000CF192 File Offset: 0x000CD592
	private static void CalculateLocalPose(Transform camT, Vector3 globalPos, Quaternion globalRot, out Vector3 localPos, out Quaternion localRot)
	{
		localPos = camT.InverseTransformPoint(globalPos);
		localRot = Quaternion.Inverse(camT.rotation) * globalRot;
	}

	// Token: 0x06001D49 RID: 7497 RVA: 0x000CF1BC File Offset: 0x000CD5BC
	private static void CalculateRewardStartPose(Vector3 globalPos, Quaternion globalRot, out Vector3 rewardStartPos, out Quaternion rewardStartRot)
	{
		Vector3 position;
		Quaternion rhs;
		RewardVisualization.CalculateLocalPose(Blocksworld.cameraTransform, globalPos, globalRot, out position, out rhs);
		Transform transform = Blocksworld.rewardCamera.transform;
		rewardStartPos = transform.TransformPoint(position);
		rewardStartRot = transform.rotation * rhs;
	}

	// Token: 0x06001D4A RID: 7498 RVA: 0x000CF204 File Offset: 0x000CD604
	public static HashSet<GAF> GetScarcityHighlightGafs(HashSet<GAF> result)
	{
		if (RewardVisualization.rewardAnimationRunning && RewardVisualization.rewardBlockTile != null && Scarcity.inventory != null)
		{
			int num;
			if (!Scarcity.inventory.TryGetValue(RewardVisualization.rewardBlockTile.gaf, out num) || num < 0)
			{
				return result;
			}
			if (result == null)
			{
				result = new HashSet<GAF>();
			}
			result.Add(RewardVisualization.rewardBlockTile.gaf);
		}
		return result;
	}

	// Token: 0x06001D4B RID: 7499 RVA: 0x000CF274 File Offset: 0x000CD674
	public static void UpdateTiles()
	{
		if (RewardVisualization.rewardBlockTile == null || RewardVisualization.rewardBlockTile.tileObject != null)
		{
			return;
		}
		GAF gaf = RewardVisualization.rewardBlockTile.gaf;
		RewardVisualization.rewardBlockTile = null;
		foreach (List<Tile> list in Blocksworld.buildPanel.tiles)
		{
			foreach (Tile tile in list)
			{
				if (tile.gaf.Equals(gaf))
				{
					RewardVisualization.rewardBlockTile = tile;
				}
			}
		}
		Blocksworld.buildPanel.DisableTilesBut(RewardVisualization.rewardBlockTile);
	}

	// Token: 0x06001D4C RID: 7500 RVA: 0x000CF364 File Offset: 0x000CD764
	private static void PrepareRewardVisualization(string awardName, int count)
	{
		RewardVisualization.rewardAnimationRunning = true;
		if (RewardVisualization.rewardStateDurations == null)
		{
			if (RewardVisualization.lightGo != null)
			{
				UnityEngine.Object.Destroy(RewardVisualization.lightGo);
				RewardVisualization.lightGo = null;
			}
			RewardVisualization.lightGo = new GameObject();
			RewardVisualization.rewardLight = RewardVisualization.lightGo.AddComponent<Light>();
			RewardVisualization.rewardLight.type = LightType.Directional;
			RewardVisualization.rewardLight.intensity = 1f;
			RewardVisualization.rewardLight.cullingMask = 2048;
			RewardVisualization.lightGo.transform.rotation = Quaternion.Euler(60.41f, 334.61f, 283.74f);
			RewardVisualization.rewardBlockTile = null;
			RewardVisualization.rewardFailed = false;
			RewardVisualization.rewardBlocksLeft = count;
			RewardVisualization.rewardBlocksCount = count;
			RewardVisualization.rewardBlocksReceived = 0;
			bool flag = RewardVisualization.definedModels.ContainsKey(awardName);
			bool flag2 = awardName == "purchasedModel";
			RewardVisualization.rewardAnimationList = new List<RewardVisualization.RewardVisualizationState>();
			RewardVisualization.rewardAnimationList.Add(RewardVisualization.RewardVisualizationState.APPEARS);
			RewardVisualization.rewardAnimationList.Add(RewardVisualization.RewardVisualizationState.SPINS);
			for (int i = 0; i < count; i++)
			{
				RewardVisualization.rewardAnimationList.Add(RewardVisualization.RewardVisualizationState.MOVES_TOWARDS_INVENTORY);
			}
			RewardVisualization.rewardAnimationList.Add(RewardVisualization.RewardVisualizationState.IN_INVENTORY);
			RewardVisualization.rewardAnimationList.Add(RewardVisualization.RewardVisualizationState.END_WAIT);
			RewardVisualization.rewardStateDurations = new Dictionary<RewardVisualization.RewardVisualizationState, float>();
			RewardVisualization.rewardStateDurations[RewardVisualization.RewardVisualizationState.APPEARS] = ((!flag2) ? 0.3f : 2f);
			RewardVisualization.rewardStateDurations[RewardVisualization.RewardVisualizationState.SPINS] = ((!flag) ? 2.2f : 5f);
			RewardVisualization.rewardStateDurations[RewardVisualization.RewardVisualizationState.MOVES_TOWARDS_INVENTORY] = 0.4f / Mathf.Log((float)(1 + count));
			RewardVisualization.rewardStateDurations[RewardVisualization.RewardVisualizationState.IN_INVENTORY] = 0.6f;
			RewardVisualization.rewardStateDurations[RewardVisualization.RewardVisualizationState.END_WAIT] = 0.5f;
			RewardVisualization.worldModelBlock = null;
			RewardVisualization.modelInitRotation = Quaternion.identity;
			if (flag)
			{
				RewardVisualization.rotationSpeed = 100f;
				RewardVisualization.rewardSpinBlocks = new List<Block>();
				if (flag2)
				{
					RewardVisualization.modelInitRotation = RewardVisualization.GetFirstPossibleWorldModel()[0].go.transform.rotation;
				}
				RewardVisualization.rewardSpinModel = RewardVisualization.CreateRewardModel(awardName, RewardVisualization.rewardSpinBlocks, RewardVisualization.modelInitRotation);
				if (flag2 && RewardVisualization.rewardSpinBlocks.Count > 0)
				{
					RewardVisualization.worldModelBlock = RewardVisualization.FindMatchingWorldModelBlock(RewardVisualization.rewardSpinBlocks[0]);
					if (RewardVisualization.worldModelBlock != null)
					{
						Transform transform = RewardVisualization.worldModelBlock.go.transform;
						Transform transform2 = RewardVisualization.rewardSpinBlocks[0].go.transform;
						RewardVisualization.CalculateRewardStartPose(transform.position, transform.rotation, out RewardVisualization.rewardModelBlockStartPos, out RewardVisualization.rewardModelBlockStartRotation);
						RewardVisualization.rewardModelStartRotation = RewardVisualization.rewardModelBlockStartRotation;
						RewardVisualization.rewardSpinModel.transform.rotation = RewardVisualization.rewardModelStartRotation;
						Vector3 b = transform2.position - RewardVisualization.rewardSpinModel.transform.position;
						RewardVisualization.rewardModelBlockStartPos -= b;
					}
				}
			}
			else
			{
				RewardVisualization.rewardSpinBlock = RewardVisualization.CreateRewardBlock(awardName);
			}
			if (RewardVisualization.rewardStarburstGo == null)
			{
				RewardVisualization.rewardStarburstGo = Blocksworld.rewardStarburst;
			}
			if (Blocksworld.starsReward != null)
			{
				Blocksworld.starsReward.gameObject.SetLayer(Layer.Rewards, false);
			}
			RewardVisualization.rewardEnterCurve = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0f, 1f, 0f, 0f),
				new Keyframe(0.5f, -0.2f, 0f, 0f),
				new Keyframe(1f, 0f, 0f, 0f)
			});
			RewardVisualization.rewardExitCurve = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0f, 0f, 0f, 0f),
				new Keyframe(1f, 1f, 0f, 0f)
			});
		}
	}

	// Token: 0x06001D4D RID: 7501 RVA: 0x000CF760 File Offset: 0x000CDB60
	private static Tile CreateFakeRewardTile(string awardName)
	{
		if (!Blocksworld.prefabs.ContainsKey(awardName))
		{
			return new Tile(new GAF("Block.TextureTo", new object[]
			{
				awardName,
				Vector3.up,
				0
			}));
		}
		return new Tile(new GAF("Block.Create", new object[]
		{
			awardName
		}));
	}

	// Token: 0x06001D4E RID: 7502 RVA: 0x000CF7C8 File Offset: 0x000CDBC8
	private static GameObject CreateRewardModel(string awardName, List<Block> list, Quaternion rotation)
	{
		List<List<List<Tile>>> list2 = ModelUtils.ParseModelJSON(JSONDecoder.Decode(RewardVisualization.definedModels[awardName]));
		GameObject gameObject = new GameObject("Reward model " + awardName);
		gameObject.transform.rotation = rotation;
		gameObject.SetLayer(Layer.Rewards, true);
		if (list2.Count > 0)
		{
			Vector3 vector = Vector3.zero;
			for (int i = 0; i < list2.Count; i++)
			{
				List<List<Tile>> list3 = list2[i];
				list3.RemoveRange(2, list3.Count - 2);
				list3[1] = Block.EmptyTileRow();
				Block block = Block.NewBlock(list3, false, false);
				block.Reset(false);
				vector += block.GetPosition();
				block.IgnoreRaycasts(true);
				block.go.SetLayer(Layer.Rewards, true);
				if (block.goShadow != null)
				{
					UnityEngine.Object.Destroy(block.goShadow);
					block.goShadow = null;
				}
				list.Add(block);
			}
			BlockGroups.GatherBlockGroups(list);
			for (int j = 0; j < list.Count; j++)
			{
				Block block2 = list[j];
				BlockTankTreadsWheel blockTankTreadsWheel = block2 as BlockTankTreadsWheel;
				if (blockTankTreadsWheel != null && blockTankTreadsWheel.IsMainBlockInGroup())
				{
					blockTankTreadsWheel.CreateTreads(false, true);
				}
			}
			for (int k = 0; k < list.Count; k++)
			{
				Block block3 = list[k];
				Collider[] componentsInChildren = block3.go.GetComponentsInChildren<Collider>();
				foreach (Collider obj in componentsInChildren)
				{
					UnityEngine.Object.Destroy(obj);
				}
			}
			vector /= (float)list2.Count;
			gameObject.transform.position = vector;
			float num = Util.MaxComponent(Util.ComputeBoundsWithSize(list, true).extents);
			for (int m = 0; m < list.Count; m++)
			{
				Block block4 = list[m];
				block4.goT.parent = gameObject.transform;
			}
			float num2 = 2f;
			RewardVisualization.modelScaler = 1f;
			if (num > num2)
			{
				RewardVisualization.modelScaler = num2 / num;
				gameObject.transform.localScale = Vector3.one * RewardVisualization.modelScaler;
			}
		}
		else
		{
			BWLog.Info("Empty reward model");
		}
		return gameObject;
	}

	// Token: 0x06001D4F RID: 7503 RVA: 0x000CFA2C File Offset: 0x000CDE2C
	public static void LoadRewardModelIcon(string modelName, string modelJsonStr)
	{
		RewardVisualization.expectedModelIcons.Add(modelName);
		if (RewardVisualization.definedModelIcons.ContainsKey(modelName))
		{
			return;
		}
		List<List<List<Tile>>> model = ModelUtils.ParseModelString(modelJsonStr);
		Action<Texture2D> callback = delegate(Texture2D tex)
		{
			if (tex == null)
			{
				RewardVisualization.expectedModelIcons.Remove(modelName);
			}
			else
			{
				RewardVisualization.definedModelIcons[modelName] = tex;
			}
		};
		ScreenshotUtils.GenerateModelIconTexture(model, Blocksworld.hd, callback);
	}

	// Token: 0x06001D50 RID: 7504 RVA: 0x000CFA90 File Offset: 0x000CDE90
	public static bool AreRewardModelIconsLoaded()
	{
		foreach (string key in RewardVisualization.expectedModelIcons)
		{
			if (!RewardVisualization.definedModelIcons.ContainsKey(key))
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x06001D51 RID: 7505 RVA: 0x000CFB00 File Offset: 0x000CDF00
	public static bool GetIconForModel(string modelName, out Texture2D icon)
	{
		return RewardVisualization.definedModelIcons.TryGetValue(modelName, out icon);
	}

	// Token: 0x17000139 RID: 313
	// (get) Token: 0x06001D52 RID: 7506 RVA: 0x000CFB0E File Offset: 0x000CDF0E
	public static int expectedRewardModelIconCount
	{
		get
		{
			return RewardVisualization.expectedModelIcons.Count;
		}
	}

	// Token: 0x06001D53 RID: 7507 RVA: 0x000CFB1A File Offset: 0x000CDF1A
	public static void Cancel()
	{
		RewardVisualization.ClearModelRewardIcons();
		RewardVisualization.ClearDefinedModels();
		RewardVisualization.CleanUp();
	}

	// Token: 0x06001D54 RID: 7508 RVA: 0x000CFB2C File Offset: 0x000CDF2C
	private static void ClearModelRewardIcons()
	{
		RewardVisualization.expectedModelIcons.Clear();
		foreach (Texture2D texture2D in RewardVisualization.definedModelIcons.Values)
		{
			if (texture2D != null)
			{
				UnityEngine.Object.Destroy(texture2D);
			}
		}
		RewardVisualization.definedModelIcons.Clear();
	}

	// Token: 0x06001D55 RID: 7509 RVA: 0x000CFBAC File Offset: 0x000CDFAC
	private static void ClearDefinedModels()
	{
		RewardVisualization.definedModels.Clear();
	}

	// Token: 0x06001D56 RID: 7510 RVA: 0x000CFBB8 File Offset: 0x000CDFB8
	private static Block CreateRewardBlock(string awardName)
	{
		List<Tile> list = null;
		string text = awardName;
		bool flag = Blocksworld.existingBlockNames.Contains(awardName);
		if (!flag)
		{
			list = new List<Tile>();
			list.Add(new Tile(new GAF("Block.TextureTo", new object[]
			{
				awardName,
				Vector3.up,
				0
			})));
			string text2 = "Yellow";
			string key = awardName + " SD.png";
			if (Blocksworld.iconColors.ContainsKey(key))
			{
				text2 = Blocksworld.iconColors[key];
			}
			list.Add(new Tile(new GAF("Block.PaintTo", new object[]
			{
				text2,
				0
			})));
			text = "Cube";
		}
		Block block = Blocksworld.bw.AddNewBlock(new Tile(new GAF("Block.Create", new object[]
		{
			text
		})), false, list, flag);
		BlockTerrain blockTerrain = block as BlockTerrain;
		if (blockTerrain != null)
		{
			BWSceneManager.RemoveTerrainBlock(blockTerrain);
		}
		BlockWaterCube blockWaterCube = block as BlockWaterCube;
		if (blockWaterCube != null)
		{
			BlockAbstractWater.waterCubes.Remove(blockWaterCube);
		}
		Vector3 vector = block.Scale();
		while (Util.MaxComponent(vector) >= 3f)
		{
			vector = new Vector3(Mathf.Max(1f, Mathf.Round(vector.x / 2f)), Mathf.Max(1f, Mathf.Round(vector.y / 2f)), Mathf.Max(1f, Mathf.Round(vector.z / 2f)));
			block.ScaleTo(vector, true, true);
		}
		block.IgnoreRaycasts(true);
		block.go.SetLayer(Layer.Rewards, false);
		if (block.goShadow != null)
		{
			UnityEngine.Object.Destroy(block.goShadow);
			block.goShadow = null;
		}
		Transform parent = block.goT.parent;
		if (parent != null)
		{
			parent.gameObject.SetLayer(Layer.Rewards, false);
		}
		IEnumerator enumerator = block.goT.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Transform transform = (Transform)obj;
				GameObject gameObject = transform.gameObject;
				if (gameObject != null)
				{
					gameObject.SetLayer(Layer.Rewards, false);
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
		if (block == null)
		{
			BWLog.Info("Could not create reward block " + awardName);
		}
		return block;
	}

	// Token: 0x06001D57 RID: 7511 RVA: 0x000CFE50 File Offset: 0x000CE250
	private static void EmitRewardStars(Vector3 pos, int count)
	{
		if (Blocksworld.starsReward != null)
		{
			Blocksworld.starsReward.transform.position = pos;
			Blocksworld.starsReward.Emit(count);
		}
	}

	// Token: 0x06001D58 RID: 7512 RVA: 0x000CFE80 File Offset: 0x000CE280
	private static void MoveRewardStarburst(Vector3 pos, float size, Camera camera)
	{
		if (RewardVisualization.rewardStarburstGo != null)
		{
			RewardVisualization.rewardStarburstGo.transform.position = pos;
			RewardVisualization.rewardStarburstGo.transform.localScale = new Vector3(size, size, size);
			Quaternion rotation = Quaternion.AngleAxis(-Time.time * 30f, camera.transform.forward);
			RewardVisualization.rewardStarburstGo.transform.LookAt(RewardVisualization.rewardStarburstGo.transform.position + rotation * camera.transform.up, -camera.transform.forward);
		}
	}

	// Token: 0x06001D59 RID: 7513 RVA: 0x000CFF25 File Offset: 0x000CE325
	private static void SetBlockPosRot(Block b, Vector3 pos, Quaternion rot)
	{
		if (b != null)
		{
			b.goT.position = pos;
			b.goT.rotation = rot;
		}
	}

	// Token: 0x06001D5A RID: 7514 RVA: 0x000CFF48 File Offset: 0x000CE348
	private static void DestroyRewardBlocks(bool destroySpin = true, bool destroyMove = true)
	{
		if (destroySpin)
		{
			if (RewardVisualization.rewardSpinBlock != null)
			{
				RewardVisualization.rewardSpinBlock.Destroy();
				RewardVisualization.rewardSpinBlock = null;
			}
			if (RewardVisualization.rewardSpinModel != null)
			{
				foreach (Block block in RewardVisualization.rewardSpinBlocks)
				{
					block.Destroy();
				}
				RewardVisualization.rewardSpinBlocks = null;
				UnityEngine.Object.Destroy(RewardVisualization.rewardSpinModel);
				RewardVisualization.rewardSpinModel = null;
			}
		}
		if (RewardVisualization.rewardMoveBlock != null && destroyMove)
		{
			RewardVisualization.rewardMoveBlock.Destroy();
			RewardVisualization.rewardMoveBlock = null;
		}
	}

	// Token: 0x06001D5B RID: 7515 RVA: 0x000D0008 File Offset: 0x000CE408
	private static void IncrementInventoryReward(GAF gaf)
	{
		if (Scarcity.inventory == null)
		{
			return;
		}
		if (RewardVisualization.startInventoryValue == -1)
		{
			return;
		}
		int count = RewardVisualization.startInventoryValue + RewardVisualization.rewardBlocksReceived;
		RewardVisualization.SetRewardInventory(RewardVisualization.rewardBlockTile.gaf, count);
		Scarcity.inventoryScales[RewardVisualization.rewardBlockTile.gaf] = 1.5f;
		Sound.PlaySound("Reward Counter", Sound.GetOrCreateOneShotAudioSource(), true, 1f, 1f, false);
	}

	// Token: 0x06001D5C RID: 7516 RVA: 0x000D007C File Offset: 0x000CE47C
	private static void CleanUp()
	{
		Camera rewardCamera = Blocksworld.rewardCamera;
		RewardVisualization.DestroyRewardBlocks(true, true);
		if (RewardVisualization.rewardStarburstGo != null && RewardVisualization.rewardStarburstGo.GetComponent<Renderer>() != null)
		{
			RewardVisualization.rewardStarburstGo.GetComponent<Renderer>().enabled = false;
		}
		RewardVisualization.rewardBlockTile = null;
		RewardVisualization.rewardStateDurations = null;
		RewardVisualization.rewardSpinBlock = null;
		RewardVisualization.rewardAnimationRunning = false;
		if (RewardVisualization.rewardBlockTile != null)
		{
			Scarcity.inventoryScales[RewardVisualization.rewardBlockTile.gaf] = 1f;
		}
		if (rewardCamera != null)
		{
			rewardCamera.enabled = false;
		}
		if (Blocksworld.bw != null && Blocksworld.bw.pullObjectLineRenderer != null)
		{
			Blocksworld.bw.pullObjectLineRenderer.enabled = false;
		}
	}

	// Token: 0x06001D5D RID: 7517 RVA: 0x000D014E File Offset: 0x000CE54E
	private static void EndWait()
	{
		if (RewardVisualization.prevRewardState == RewardVisualization.RewardVisualizationState.IN_INVENTORY)
		{
			RewardVisualization.rewardStarburstGo.GetComponent<Renderer>().enabled = false;
			Blocksworld.UI.SidePanel.Hide();
		}
	}

	// Token: 0x06001D5E RID: 7518 RVA: 0x000D017C File Offset: 0x000CE57C
	private static Vector3 GetModelTargetPos()
	{
		UITabBar tabBar = Blocksworld.UI.TabBar;
		if (tabBar != null)
		{
			return tabBar.GetTabBarPosition(TabBarTabId.Models) + new Vector3(1f, 1f, 0f) * tabBar.GetTabHeight() * 0.5f;
		}
		return Vector3.zero;
	}

	// Token: 0x06001D5F RID: 7519 RVA: 0x000D01DC File Offset: 0x000CE5DC
	private static void InInventory(Camera camera)
	{
		bool flag = RewardVisualization.rewardSpinModel != null;
		Vector3 a = Vector3.zero;
		if (flag)
		{
			a = RewardVisualization.GetModelTargetPos();
		}
		else if (RewardVisualization.rewardBlockTile != null && RewardVisualization.rewardBlockTile.tileObject != null)
		{
			a = RewardVisualization.rewardBlockTile.tileObject.GetPosition() + new Vector3(40f, 40f, 0f);
		}
		if (RewardVisualization.prevRewardState == RewardVisualization.RewardVisualizationState.MOVES_TOWARDS_INVENTORY)
		{
			Vector3 position = a * NormalizedScreen.scale;
			RewardVisualization.EmitRewardStars(camera.ScreenToWorldPoint(position), RewardVisualization.tileHitParticles);
			RewardVisualization.rewardBlocksReceived++;
			if (!flag && RewardVisualization.rewardBlockTile != null)
			{
				RewardVisualization.IncrementInventoryReward(RewardVisualization.rewardBlockTile.gaf);
			}
		}
		RewardVisualization.DestroyRewardBlocks(flag, true);
	}

	// Token: 0x06001D60 RID: 7520 RVA: 0x000D02B0 File Offset: 0x000CE6B0
	private static void MoveTowardsInventory(Camera camera, float timeFraction, float rotAngle, int rewardAnimationIndex, RewardVisualization.RewardVisualizationState state, string awardName)
	{
		Vector3 position = camera.transform.position;
		Quaternion rotation = camera.transform.rotation;
		float num = 1f - timeFraction;
		if (camera != null)
		{
			RewardVisualization.EmitRewardStars(position + camera.transform.forward * RewardVisualization.centralParticlesDistance, RewardVisualization.centralParticles);
		}
		bool flag = RewardVisualization.rewardSpinModel != null;
		if (RewardVisualization.prevRewardState != state && !flag && RewardVisualization.rewardBlockTile != null)
		{
			RewardVisualization.rewardMoveBlock = RewardVisualization.CreateRewardBlock(awardName);
		}
		Vector3 vector = Vector3.zero;
		if (flag)
		{
			vector = RewardVisualization.GetModelTargetPos();
		}
		else if (RewardVisualization.rewardBlockTile != null && RewardVisualization.rewardBlockTile.tileObject != null)
		{
			vector = RewardVisualization.rewardBlockTile.tileObject.GetPosition() + new Vector3(40f, 40f, 0f);
		}
		if (rewardAnimationIndex != RewardVisualization.prevRewardAnimationIndex)
		{
			Sound.PlaySound("Reward Swhoosh", Sound.GetOrCreateOneShotAudioSource(), true, 1f, 1f, false);
			RewardVisualization.rewardBlocksLeft--;
			if (RewardVisualization.rewardBlocksLeft == 0 && !flag)
			{
				RewardVisualization.DestroyRewardBlocks(true, false);
			}
			if (RewardVisualization.prevRewardState == state)
			{
				RewardVisualization.EmitRewardStars(camera.ScreenToWorldPoint(vector), RewardVisualization.tileHitParticles);
				RewardVisualization.rewardBlocksReceived++;
				if (RewardVisualization.rewardBlockTile != null)
				{
					RewardVisualization.IncrementInventoryReward(RewardVisualization.rewardBlockTile.gaf);
				}
			}
		}
		GameObject gameObject = null;
		if (RewardVisualization.rewardMoveBlock != null && RewardVisualization.rewardMoveBlock.go != null)
		{
			gameObject = RewardVisualization.rewardMoveBlock.go;
		}
		else if (RewardVisualization.rewardSpinModel != null)
		{
			gameObject = RewardVisualization.rewardSpinModel;
		}
		if (gameObject != null && camera != null)
		{
			Vector3 a = position + camera.transform.forward * RewardVisualization.camDist;
			Vector3 position2 = vector * NormalizedScreen.scale;
			Vector3 a2 = camera.ScreenToWorldPoint(position2);
			float num2 = RewardVisualization.rewardExitCurve.Evaluate(timeFraction);
			Vector3 vector2 = a * (1f - num2) + a2 * num2;
			gameObject.transform.position = vector2;
			if (RewardVisualization.rewardBlocksLeft == 0)
			{
				RewardVisualization.MoveRewardStarburst(vector2, RewardVisualization.starburstSize * num, camera);
			}
			else
			{
				RewardVisualization.MoveRewardStarburst(position + camera.transform.forward * 6f, RewardVisualization.starburstSize, camera);
			}
		}
		Vector3 rotAxis = RewardVisualization.GetRotAxis();
		if (RewardVisualization.rewardSpinBlock != null && RewardVisualization.rewardSpinBlock.go != null)
		{
			RewardVisualization.rewardSpinBlock.goT.rotation = rotation * Quaternion.AngleAxis(rotAngle, rotAxis) * RewardVisualization.modelInitRotation;
		}
		if (gameObject != null)
		{
			gameObject.transform.rotation = rotation * Quaternion.AngleAxis(rotAngle, rotAxis) * RewardVisualization.modelInitRotation;
		}
	}

	// Token: 0x06001D61 RID: 7521 RVA: 0x000D05C4 File Offset: 0x000CE9C4
	private static void Spin(Camera camera, float timeFraction, float rotAngle, string awardName)
	{
		Vector3 position = camera.transform.position;
		Quaternion rotation = camera.transform.rotation;
		RewardVisualization.EmitRewardStars(position + camera.transform.forward * RewardVisualization.centralParticlesDistance, RewardVisualization.centralParticles);
		RewardVisualization.MoveRewardStarburst(position + camera.transform.forward * 6f, RewardVisualization.starburstSize, camera);
		GameObject gameObject = null;
		bool flag = false;
		if (RewardVisualization.rewardSpinBlock != null && RewardVisualization.rewardSpinBlock.go != null)
		{
			gameObject = RewardVisualization.rewardSpinBlock.go;
		}
		else if (RewardVisualization.rewardSpinModel != null)
		{
			gameObject = RewardVisualization.rewardSpinModel;
			flag = true;
		}
		Vector3 rotAxis = RewardVisualization.GetRotAxis();
		if (gameObject != null && camera != null)
		{
			gameObject.transform.position = position + camera.transform.forward * RewardVisualization.camDist;
			gameObject.transform.rotation = rotation * Quaternion.AngleAxis(rotAngle, rotAxis) * RewardVisualization.modelInitRotation;
		}
		float num = (!flag) ? 0.5f : 0.75f;
		if (RewardVisualization.rewardBlockTile == null && timeFraction > num && !RewardVisualization.rewardFailed)
		{
			if (!flag)
			{
				List<List<Tile>> tiles = Blocksworld.buildPanel.tiles;
				for (int i = 0; i < tiles.Count; i++)
				{
					List<Tile> list = tiles[i];
					for (int j = 0; j < list.Count; j++)
					{
						Tile tile = list[j];
						bool flag2 = tile.gaf.Predicate.Name == "Block.Create";
						bool flag3 = tile.gaf.Predicate.Name == "Block.TextureTo";
						if (flag2 || flag3)
						{
							string a = (string)tile.gaf.Args[0];
							if (a == awardName)
							{
								RewardVisualization.rewardBlockTile = tile;
								break;
							}
						}
					}
				}
			}
			if (RewardVisualization.rewardBlockTile == null)
			{
				if (flag)
				{
					RewardVisualization.rewardBlockTile = Block.ThenTile();
				}
				else
				{
					BWLog.Info("Could not find reward tile for " + awardName + " creating fake!");
					Tile tile2 = RewardVisualization.CreateFakeRewardTile(awardName);
					tile2.Show(true);
					RewardVisualization.rewardBlockTile = tile2;
					List<List<Tile>> tiles2 = Blocksworld.buildPanel.tiles;
					if (tiles2.Count > 0)
					{
						List<Tile> list2 = tiles2[tiles2.Count - 1];
						list2.Add(tile2);
						Blocksworld.buildPanel.Layout();
					}
					else
					{
						BWLog.Info("Failed to create fake tile for " + awardName);
						RewardVisualization.rewardBlockTile = null;
						RewardVisualization.rewardFailed = true;
					}
				}
			}
			if (RewardVisualization.rewardBlockTile != null)
			{
				if (!flag && Scarcity.inventory != null)
				{
					int num2;
					if (!Scarcity.globalInventory.TryGetValue(RewardVisualization.rewardBlockTile.gaf, out num2))
					{
						num2 = 0;
					}
					int num3 = 0;
					if (Scarcity.worldGAFUsage != null && Scarcity.worldGAFUsage.TryGetValue(RewardVisualization.rewardBlockTile.gaf, out num3) && num3 < 0)
					{
						num3 = 0;
					}
					RewardVisualization.startInventoryValue = num2 + num3 - RewardVisualization.rewardBlocksCount;
					RewardVisualization.SetRewardInventory(RewardVisualization.rewardBlockTile.gaf, RewardVisualization.startInventoryValue);
				}
				Blocksworld.buildPanel.Show(true);
				int tabIndex = (!flag) ? 0 : 1;
				Blocksworld.buildPanel.GetTabBar().SetSelectedTab(tabIndex, false);
				Blocksworld.buildPanel.PositionReset(true);
				if (!flag)
				{
					Blocksworld.buildPanel.DisableTilesBut(RewardVisualization.rewardBlockTile);
					Blocksworld.buildPanel.ScrollToVisible(RewardVisualization.rewardBlockTile, true, false, false);
				}
				Blocksworld.buildPanel.SnapBackInsideBounds(true);
				Blocksworld.UI.SidePanel.Show();
			}
		}
	}

	// Token: 0x06001D62 RID: 7522 RVA: 0x000D09A1 File Offset: 0x000CEDA1
	private static void SetRewardInventory(GAF gaf, int count)
	{
		if (count <= 0 || RewardVisualization.startInventoryValue < 0)
		{
			count = -1;
		}
		Scarcity.inventory[gaf] = count;
	}

	// Token: 0x06001D63 RID: 7523 RVA: 0x000D09C4 File Offset: 0x000CEDC4
	private static void Appear(Camera camera, float timeFraction, float rotAngle)
	{
		Vector3 position = camera.transform.position;
		Quaternion rotation = camera.transform.rotation;
		RewardVisualization.EmitRewardStars(position + camera.transform.forward * RewardVisualization.centralParticlesDistance, RewardVisualization.centralParticles);
		RewardVisualization.MoveRewardStarburst(position + camera.transform.forward * 6f, RewardVisualization.starburstSize * timeFraction, camera);
		GameObject gameObject = null;
		if (RewardVisualization.rewardSpinBlock != null && RewardVisualization.rewardSpinBlock.go != null)
		{
			gameObject = RewardVisualization.rewardSpinBlock.go;
		}
		else if (RewardVisualization.rewardSpinModel != null)
		{
			gameObject = RewardVisualization.rewardSpinModel;
		}
		Vector3 rotAxis = RewardVisualization.GetRotAxis();
		Quaternion quaternion = rotation * Quaternion.AngleAxis(rotAngle, rotAxis) * RewardVisualization.modelInitRotation;
		if (gameObject != null)
		{
			Vector3 position2;
			Quaternion rotation2;
			if (RewardVisualization.worldModelBlock == null)
			{
				position2 = position + camera.transform.forward * (RewardVisualization.camDist + RewardVisualization.rewardEnterCurve.Evaluate(timeFraction) * RewardVisualization.maxDist);
				rotation2 = quaternion;
			}
			else
			{
				if (RewardVisualization.worldModelBlock.go.activeSelf)
				{
					List<Block> list = Block.connectedCache[RewardVisualization.worldModelBlock];
					foreach (Block block in list)
					{
						block.chunk.go.SetActive(false);
						block.Deactivate();
					}
				}
				gameObject.transform.localScale = Vector3.one * ((1f - timeFraction) * 1f + timeFraction * RewardVisualization.modelScaler);
				position2 = (1f - timeFraction) * RewardVisualization.rewardModelBlockStartPos + timeFraction * (position + camera.transform.forward * RewardVisualization.camDist);
				rotation2 = Quaternion.Lerp(Quaternion.AngleAxis(rotAngle, rotAxis) * RewardVisualization.rewardModelStartRotation, quaternion, timeFraction);
			}
			gameObject.transform.position = position2;
			gameObject.transform.rotation = rotation2;
		}
	}

	// Token: 0x06001D64 RID: 7524 RVA: 0x000D0C08 File Offset: 0x000CF008
	private static bool SanityCheckAfterPrepare()
	{
		if (RewardVisualization.rewardStarburstGo == null)
		{
			BWLog.Info("Reward starburst was null");
			return false;
		}
		if (RewardVisualization.rewardStarburstGo.GetComponent<Renderer>() == null)
		{
			BWLog.Info("Reward starburst renderer was null");
			return false;
		}
		return true;
	}

	// Token: 0x06001D65 RID: 7525 RVA: 0x000D0C48 File Offset: 0x000CF048
	private static bool SanityCheckBeforePrepare()
	{
		if (Blocksworld.rewardCamera == null)
		{
			BWLog.Info("RewardVisualization: Could not find the reward camera");
			return false;
		}
		if (Blocksworld.buildPanel == null)
		{
			BWLog.Info("RewardVisualization: Blocksworld.buildPanel was null");
			return false;
		}
		if (Blocksworld.bw == null)
		{
			BWLog.Info("RewardVisualization: Blocksworld.bw was null");
			return false;
		}
		if (Blocksworld.bw.gameObject == null)
		{
			BWLog.Info("RewardVisualization: Blocksworld.bw.gameObject was null");
			return false;
		}
		if (Blocksworld.starsReward == null)
		{
			BWLog.Info("RewardVisualization: Blocksworld.starsReward was null");
			return false;
		}
		if (Blocksworld.rewardStarburst == null)
		{
			BWLog.Info("RewardVisualization: Blocksworld.rewardStarburst was null");
			return false;
		}
		return true;
	}

	// Token: 0x06001D66 RID: 7526 RVA: 0x000D0D00 File Offset: 0x000CF100
	public static TileResultCode VisualizeReward(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float timer = eInfo.timer;
		if (timer == 0f && RewardVisualization.rewardAnimationRunning)
		{
			return TileResultCode.True;
		}
		bool flag = !RewardVisualization.rewardAnimationRunning;
		if (!RewardVisualization.SanityCheckBeforePrepare())
		{
			RewardVisualization.CleanUp();
			return TileResultCode.True;
		}
		Camera rewardCamera = Blocksworld.rewardCamera;
		if (flag)
		{
			rewardCamera.enabled = true;
			Sound.PlaySound("Reward Appear", Sound.GetOrCreateOneShotAudioSource(), true, 1f, 1f, false);
			Sound.PlaySound("Reward Appear", Sound.GetOrCreateOneShotAudioSource(), true, 1f, 1f, false);
		}
		string awardName = "Rocket";
		if (args.Length > 0)
		{
			awardName = (string)args[0];
		}
		int num = 1;
		if (args.Length > 1)
		{
			num = (int)args[1];
		}
		if (num < 1)
		{
			num = 1;
		}
		RewardVisualization.PrepareRewardVisualization(awardName, num);
		if (!RewardVisualization.SanityCheckAfterPrepare())
		{
			RewardVisualization.CleanUp();
			return TileResultCode.True;
		}
		float num2 = 0f;
		float timeFraction = 0f;
		RewardVisualization.RewardVisualizationState state = RewardVisualization.RewardVisualizationState.APPEARS;
		int num3 = 0;
		for (int i = 0; i < RewardVisualization.rewardAnimationList.Count; i++)
		{
			RewardVisualization.RewardVisualizationState rewardVisualizationState = RewardVisualization.rewardAnimationList[i];
			float num4 = RewardVisualization.rewardStateDurations[rewardVisualizationState];
			if (num2 > timer)
			{
				break;
			}
			state = rewardVisualizationState;
			float num5 = timer - num2;
			timeFraction = Mathf.Clamp(num5 / num4, 0f, 1f);
			num2 += num4;
			num3++;
		}
		if (timer >= num2)
		{
			RewardVisualization.CleanUp();
			return TileResultCode.True;
		}
		float rotAngle = timer * RewardVisualization.rotationSpeed;
		if (flag)
		{
			RewardVisualization.rewardStarburstGo.GetComponent<Renderer>().enabled = true;
		}
		switch (state)
		{
		case RewardVisualization.RewardVisualizationState.APPEARS:
			RewardVisualization.Appear(rewardCamera, timeFraction, rotAngle);
			break;
		case RewardVisualization.RewardVisualizationState.SPINS:
			RewardVisualization.Spin(rewardCamera, timeFraction, rotAngle, awardName);
			break;
		case RewardVisualization.RewardVisualizationState.MOVES_TOWARDS_INVENTORY:
			RewardVisualization.MoveTowardsInventory(rewardCamera, timeFraction, rotAngle, num3, state, awardName);
			break;
		case RewardVisualization.RewardVisualizationState.IN_INVENTORY:
			RewardVisualization.InInventory(rewardCamera);
			break;
		case RewardVisualization.RewardVisualizationState.END_WAIT:
			RewardVisualization.EndWait();
			break;
		}
		RewardVisualization.prevRewardState = state;
		RewardVisualization.prevRewardAnimationIndex = num3;
		return TileResultCode.Delayed;
	}

	// Token: 0x040017E2 RID: 6114
	public static Dictionary<string, string> definedModels = new Dictionary<string, string>();

	// Token: 0x040017E3 RID: 6115
	private static Dictionary<string, Texture2D> definedModelIcons = new Dictionary<string, Texture2D>();

	// Token: 0x040017E4 RID: 6116
	private static HashSet<string> expectedModelIcons = new HashSet<string>();

	// Token: 0x040017E5 RID: 6117
	private static List<RewardVisualization.RewardVisualizationState> rewardAnimationList = null;

	// Token: 0x040017E6 RID: 6118
	private static Dictionary<RewardVisualization.RewardVisualizationState, float> rewardStateDurations = null;

	// Token: 0x040017E7 RID: 6119
	private static RewardVisualization.RewardVisualizationState prevRewardState = RewardVisualization.RewardVisualizationState.APPEARS;

	// Token: 0x040017E8 RID: 6120
	private static int prevRewardAnimationIndex = -1;

	// Token: 0x040017E9 RID: 6121
	private static Block rewardSpinBlock = null;

	// Token: 0x040017EA RID: 6122
	private static Block rewardMoveBlock = null;

	// Token: 0x040017EB RID: 6123
	private static GameObject rewardSpinModel = null;

	// Token: 0x040017EC RID: 6124
	private static List<Block> rewardSpinBlocks = null;

	// Token: 0x040017ED RID: 6125
	private static Block worldModelBlock;

	// Token: 0x040017EE RID: 6126
	private static Vector3 rewardModelBlockStartPos;

	// Token: 0x040017EF RID: 6127
	private static Quaternion rewardModelBlockStartRotation;

	// Token: 0x040017F0 RID: 6128
	private static Quaternion rewardModelStartRotation;

	// Token: 0x040017F1 RID: 6129
	private static Quaternion modelInitRotation = Quaternion.identity;

	// Token: 0x040017F2 RID: 6130
	private static float modelScaler = 1f;

	// Token: 0x040017F3 RID: 6131
	private static bool rewardFailed = false;

	// Token: 0x040017F4 RID: 6132
	private static GameObject rewardStarburstGo = null;

	// Token: 0x040017F5 RID: 6133
	public static Tile rewardBlockTile = null;

	// Token: 0x040017F6 RID: 6134
	private static AnimationCurve rewardEnterCurve = null;

	// Token: 0x040017F7 RID: 6135
	private static AnimationCurve rewardExitCurve = null;

	// Token: 0x040017F8 RID: 6136
	public static bool rewardAnimationRunning = false;

	// Token: 0x040017F9 RID: 6137
	private static int rewardBlocksLeft = 1;

	// Token: 0x040017FA RID: 6138
	private static int rewardBlocksReceived = 0;

	// Token: 0x040017FB RID: 6139
	private static int rewardBlocksCount = 1;

	// Token: 0x040017FC RID: 6140
	private static int startInventoryValue = 0;

	// Token: 0x040017FD RID: 6141
	private static float rotationSpeed = 200f;

	// Token: 0x040017FE RID: 6142
	private static GameObject lightGo = null;

	// Token: 0x040017FF RID: 6143
	private static Light rewardLight = null;

	// Token: 0x04001800 RID: 6144
	private static float camDist = 5f;

	// Token: 0x04001801 RID: 6145
	private static float maxDist = 10f;

	// Token: 0x04001802 RID: 6146
	private static float starburstSize = 20f;

	// Token: 0x04001803 RID: 6147
	private static int tileHitParticles = 15;

	// Token: 0x04001804 RID: 6148
	private static int centralParticles = 1;

	// Token: 0x04001805 RID: 6149
	private static float centralParticlesDistance = 14f;

	// Token: 0x02000277 RID: 631
	private enum RewardVisualizationState
	{
		// Token: 0x04001807 RID: 6151
		APPEARS,
		// Token: 0x04001808 RID: 6152
		SPINS,
		// Token: 0x04001809 RID: 6153
		MOVES_TOWARDS_INVENTORY,
		// Token: 0x0400180A RID: 6154
		IN_INVENTORY,
		// Token: 0x0400180B RID: 6155
		END_WAIT
	}
}
