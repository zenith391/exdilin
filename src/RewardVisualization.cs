using System;
using System.Collections.Generic;
using Blocks;
using SimpleJSON;
using UnityEngine;

public class RewardVisualization
{
	private enum RewardVisualizationState
	{
		APPEARS,
		SPINS,
		MOVES_TOWARDS_INVENTORY,
		IN_INVENTORY,
		END_WAIT
	}

	public static Dictionary<string, string> definedModels = new Dictionary<string, string>();

	private static Dictionary<string, Texture2D> definedModelIcons = new Dictionary<string, Texture2D>();

	private static HashSet<string> expectedModelIcons = new HashSet<string>();

	private static List<RewardVisualizationState> rewardAnimationList = null;

	private static Dictionary<RewardVisualizationState, float> rewardStateDurations = null;

	private static RewardVisualizationState prevRewardState = RewardVisualizationState.APPEARS;

	private static int prevRewardAnimationIndex = -1;

	private static Block rewardSpinBlock = null;

	private static Block rewardMoveBlock = null;

	private static GameObject rewardSpinModel = null;

	private static List<Block> rewardSpinBlocks = null;

	private static Block worldModelBlock;

	private static Vector3 rewardModelBlockStartPos;

	private static Quaternion rewardModelBlockStartRotation;

	private static Quaternion rewardModelStartRotation;

	private static Quaternion modelInitRotation = Quaternion.identity;

	private static float modelScaler = 1f;

	private static bool rewardFailed = false;

	private static GameObject rewardStarburstGo = null;

	public static Tile rewardBlockTile = null;

	private static AnimationCurve rewardEnterCurve = null;

	private static AnimationCurve rewardExitCurve = null;

	public static bool rewardAnimationRunning = false;

	private static int rewardBlocksLeft = 1;

	private static int rewardBlocksReceived = 0;

	private static int rewardBlocksCount = 1;

	private static int startInventoryValue = 0;

	private static float rotationSpeed = 200f;

	private static GameObject lightGo = null;

	private static Light rewardLight = null;

	private static float camDist = 5f;

	private static float maxDist = 10f;

	private static float starburstSize = 20f;

	private static int tileHitParticles = 15;

	private static int centralParticles = 1;

	private static float centralParticlesDistance = 14f;

	public static int expectedRewardModelIconCount => expectedModelIcons.Count;

	private static Vector3 GetRotAxis()
	{
		if (rewardSpinModel != null)
		{
			return new Vector3(1f, 7f, 1f).normalized;
		}
		return new Vector3(1f, 3f, 1f).normalized;
	}

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

	private static Block FindMatchingWorldModelBlock(Block firstBlock)
	{
		string text = firstBlock.BlockType();
		Vector3 v = firstBlock.Scale();
		List<Block> list = BWSceneManager.AllBlocks();
		for (int i = 0; i < list.Count; i++)
		{
			Block block = list[i];
			if (!block.isTerrain && block.BlockType() == text && Vector3.Distance(Util.Abs(block.Scale()), Util.Abs(v)) < 0.01f && SameTextures(block, firstBlock) && SamePaints(block, firstBlock))
			{
				return block;
			}
		}
		return null;
	}

	private static void CalculateLocalPose(Transform camT, Vector3 globalPos, Quaternion globalRot, out Vector3 localPos, out Quaternion localRot)
	{
		localPos = camT.InverseTransformPoint(globalPos);
		localRot = Quaternion.Inverse(camT.rotation) * globalRot;
	}

	private static void CalculateRewardStartPose(Vector3 globalPos, Quaternion globalRot, out Vector3 rewardStartPos, out Quaternion rewardStartRot)
	{
		CalculateLocalPose(Blocksworld.cameraTransform, globalPos, globalRot, out var localPos, out var localRot);
		Transform transform = Blocksworld.rewardCamera.transform;
		rewardStartPos = transform.TransformPoint(localPos);
		rewardStartRot = transform.rotation * localRot;
	}

	public static HashSet<GAF> GetScarcityHighlightGafs(HashSet<GAF> result)
	{
		if (rewardAnimationRunning && rewardBlockTile != null && Scarcity.inventory != null)
		{
			if (!Scarcity.inventory.TryGetValue(rewardBlockTile.gaf, out var value) || value < 0)
			{
				return result;
			}
			if (result == null)
			{
				result = new HashSet<GAF>();
			}
			result.Add(rewardBlockTile.gaf);
		}
		return result;
	}

	public static void UpdateTiles()
	{
		if (rewardBlockTile == null || rewardBlockTile.tileObject != null)
		{
			return;
		}
		GAF gaf = rewardBlockTile.gaf;
		rewardBlockTile = null;
		foreach (List<Tile> tile in Blocksworld.buildPanel.tiles)
		{
			foreach (Tile item in tile)
			{
				if (item.gaf.Equals(gaf))
				{
					rewardBlockTile = item;
				}
			}
		}
		Blocksworld.buildPanel.DisableTilesBut(rewardBlockTile);
	}

	private static void PrepareRewardVisualization(string awardName, int count)
	{
		rewardAnimationRunning = true;
		if (rewardStateDurations != null)
		{
			return;
		}
		if (lightGo != null)
		{
			UnityEngine.Object.Destroy(lightGo);
			lightGo = null;
		}
		lightGo = new GameObject();
		rewardLight = lightGo.AddComponent<Light>();
		rewardLight.type = LightType.Directional;
		rewardLight.intensity = 1f;
		rewardLight.cullingMask = 2048;
		lightGo.transform.rotation = Quaternion.Euler(60.41f, 334.61f, 283.74f);
		rewardBlockTile = null;
		rewardFailed = false;
		rewardBlocksLeft = count;
		rewardBlocksCount = count;
		rewardBlocksReceived = 0;
		bool flag = definedModels.ContainsKey(awardName);
		bool flag2 = awardName == "purchasedModel";
		rewardAnimationList = new List<RewardVisualizationState>();
		rewardAnimationList.Add(RewardVisualizationState.APPEARS);
		rewardAnimationList.Add(RewardVisualizationState.SPINS);
		for (int i = 0; i < count; i++)
		{
			rewardAnimationList.Add(RewardVisualizationState.MOVES_TOWARDS_INVENTORY);
		}
		rewardAnimationList.Add(RewardVisualizationState.IN_INVENTORY);
		rewardAnimationList.Add(RewardVisualizationState.END_WAIT);
		rewardStateDurations = new Dictionary<RewardVisualizationState, float>();
		rewardStateDurations[RewardVisualizationState.APPEARS] = ((!flag2) ? 0.3f : 2f);
		rewardStateDurations[RewardVisualizationState.SPINS] = ((!flag) ? 2.2f : 5f);
		rewardStateDurations[RewardVisualizationState.MOVES_TOWARDS_INVENTORY] = 0.4f / Mathf.Log(1 + count);
		rewardStateDurations[RewardVisualizationState.IN_INVENTORY] = 0.6f;
		rewardStateDurations[RewardVisualizationState.END_WAIT] = 0.5f;
		worldModelBlock = null;
		modelInitRotation = Quaternion.identity;
		if (flag)
		{
			rotationSpeed = 100f;
			rewardSpinBlocks = new List<Block>();
			if (flag2)
			{
				modelInitRotation = GetFirstPossibleWorldModel()[0].go.transform.rotation;
			}
			rewardSpinModel = CreateRewardModel(awardName, rewardSpinBlocks, modelInitRotation);
			if (flag2 && rewardSpinBlocks.Count > 0)
			{
				worldModelBlock = FindMatchingWorldModelBlock(rewardSpinBlocks[0]);
				if (worldModelBlock != null)
				{
					Transform transform = worldModelBlock.go.transform;
					Transform transform2 = rewardSpinBlocks[0].go.transform;
					CalculateRewardStartPose(transform.position, transform.rotation, out rewardModelBlockStartPos, out rewardModelBlockStartRotation);
					rewardModelStartRotation = rewardModelBlockStartRotation;
					rewardSpinModel.transform.rotation = rewardModelStartRotation;
					Vector3 vector = transform2.position - rewardSpinModel.transform.position;
					rewardModelBlockStartPos -= vector;
				}
			}
		}
		else
		{
			rewardSpinBlock = CreateRewardBlock(awardName);
		}
		if (rewardStarburstGo == null)
		{
			rewardStarburstGo = Blocksworld.rewardStarburst;
		}
		if (Blocksworld.starsReward != null)
		{
			Blocksworld.starsReward.gameObject.SetLayer(Layer.Rewards);
		}
		rewardEnterCurve = new AnimationCurve(new Keyframe(0f, 1f, 0f, 0f), new Keyframe(0.5f, -0.2f, 0f, 0f), new Keyframe(1f, 0f, 0f, 0f));
		rewardExitCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(1f, 1f, 0f, 0f));
	}

	private static Tile CreateFakeRewardTile(string awardName)
	{
		if (!Blocksworld.prefabs.ContainsKey(awardName))
		{
			return new Tile(new GAF("Block.TextureTo", awardName, Vector3.up, 0));
		}
		return new Tile(new GAF("Block.Create", awardName));
	}

	private static GameObject CreateRewardModel(string awardName, List<Block> list, Quaternion rotation)
	{
		List<List<List<Tile>>> list2 = ModelUtils.ParseModelJSON(JSONDecoder.Decode(definedModels[awardName]));
		GameObject gameObject = new GameObject("Reward model " + awardName);
		gameObject.transform.rotation = rotation;
		gameObject.SetLayer(Layer.Rewards, recursive: true);
		if (list2.Count > 0)
		{
			Vector3 zero = Vector3.zero;
			for (int i = 0; i < list2.Count; i++)
			{
				List<List<Tile>> list3 = list2[i];
				list3.RemoveRange(2, list3.Count - 2);
				list3[1] = Block.EmptyTileRow();
				Block block = Block.NewBlock(list3);
				block.Reset();
				zero += block.GetPosition();
				block.IgnoreRaycasts(value: true);
				block.go.SetLayer(Layer.Rewards, recursive: true);
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
				if (block2 is BlockTankTreadsWheel blockTankTreadsWheel && blockTankTreadsWheel.IsMainBlockInGroup())
				{
					blockTankTreadsWheel.CreateTreads(shapeOnly: false, parentIsBlock: true);
				}
			}
			for (int k = 0; k < list.Count; k++)
			{
				Block block3 = list[k];
				Collider[] componentsInChildren = block3.go.GetComponentsInChildren<Collider>();
				Collider[] array = componentsInChildren;
				foreach (Collider obj in array)
				{
					UnityEngine.Object.Destroy(obj);
				}
			}
			zero /= (float)list2.Count;
			gameObject.transform.position = zero;
			float num = Util.MaxComponent(Util.ComputeBoundsWithSize(list).extents);
			for (int m = 0; m < list.Count; m++)
			{
				Block block4 = list[m];
				block4.goT.parent = gameObject.transform;
			}
			float num2 = 2f;
			modelScaler = 1f;
			if (num > num2)
			{
				modelScaler = num2 / num;
				gameObject.transform.localScale = Vector3.one * modelScaler;
			}
		}
		else
		{
			BWLog.Info("Empty reward model");
		}
		return gameObject;
	}

	public static void LoadRewardModelIcon(string modelName, string modelJsonStr)
	{
		expectedModelIcons.Add(modelName);
		if (definedModelIcons.ContainsKey(modelName))
		{
			return;
		}
		List<List<List<Tile>>> model = ModelUtils.ParseModelString(modelJsonStr);
		Action<Texture2D> callback = delegate(Texture2D tex)
		{
			if (tex == null)
			{
				expectedModelIcons.Remove(modelName);
			}
			else
			{
				definedModelIcons[modelName] = tex;
			}
		};
		ScreenshotUtils.GenerateModelIconTexture(model, Blocksworld.hd, callback);
	}

	public static bool AreRewardModelIconsLoaded()
	{
		foreach (string expectedModelIcon in expectedModelIcons)
		{
			if (!definedModelIcons.ContainsKey(expectedModelIcon))
			{
				return false;
			}
		}
		return true;
	}

	public static bool GetIconForModel(string modelName, out Texture2D icon)
	{
		return definedModelIcons.TryGetValue(modelName, out icon);
	}

	public static void Cancel()
	{
		ClearModelRewardIcons();
		ClearDefinedModels();
		CleanUp();
	}

	private static void ClearModelRewardIcons()
	{
		expectedModelIcons.Clear();
		foreach (Texture2D value in definedModelIcons.Values)
		{
			if (value != null)
			{
				UnityEngine.Object.Destroy(value);
			}
		}
		definedModelIcons.Clear();
	}

	private static void ClearDefinedModels()
	{
		definedModels.Clear();
	}

	private static Block CreateRewardBlock(string awardName)
	{
		List<Tile> list = null;
		string text = awardName;
		bool flag = Blocksworld.existingBlockNames.Contains(awardName);
		if (!flag)
		{
			list = new List<Tile>();
			list.Add(new Tile(new GAF("Block.TextureTo", awardName, Vector3.up, 0)));
			string text2 = "Yellow";
			string key = awardName + " SD.png";
			if (Blocksworld.iconColors.ContainsKey(key))
			{
				text2 = Blocksworld.iconColors[key];
			}
			list.Add(new Tile(new GAF("Block.PaintTo", text2, 0)));
			text = "Cube";
		}
		Block block = Blocksworld.bw.AddNewBlock(new Tile(new GAF("Block.Create", text)), addToBlocks: false, list, flag);
		if (block is BlockTerrain terrain)
		{
			BWSceneManager.RemoveTerrainBlock(terrain);
		}
		if (block is BlockWaterCube item)
		{
			BlockAbstractWater.waterCubes.Remove(item);
		}
		Vector3 vector = block.Scale();
		while (Util.MaxComponent(vector) >= 3f)
		{
			vector = new Vector3(Mathf.Max(1f, Mathf.Round(vector.x / 2f)), Mathf.Max(1f, Mathf.Round(vector.y / 2f)), Mathf.Max(1f, Mathf.Round(vector.z / 2f)));
			block.ScaleTo(vector, recalculateCollider: true, forceRescale: true);
		}
		block.IgnoreRaycasts(value: true);
		block.go.SetLayer(Layer.Rewards);
		if (block.goShadow != null)
		{
			UnityEngine.Object.Destroy(block.goShadow);
			block.goShadow = null;
		}
		Transform parent = block.goT.parent;
		if (parent != null)
		{
			parent.gameObject.SetLayer(Layer.Rewards);
		}
		foreach (object item2 in block.goT)
		{
			Transform transform = (Transform)item2;
			GameObject gameObject = transform.gameObject;
			if (gameObject != null)
			{
				gameObject.SetLayer(Layer.Rewards);
			}
		}
		if (block == null)
		{
			BWLog.Info("Could not create reward block " + awardName);
		}
		return block;
	}

	private static void EmitRewardStars(Vector3 pos, int count)
	{
		if (Blocksworld.starsReward != null)
		{
			Blocksworld.starsReward.transform.position = pos;
			Blocksworld.starsReward.Emit(count);
		}
	}

	private static void MoveRewardStarburst(Vector3 pos, float size, Camera camera)
	{
		if (rewardStarburstGo != null)
		{
			rewardStarburstGo.transform.position = pos;
			rewardStarburstGo.transform.localScale = new Vector3(size, size, size);
			Quaternion quaternion = Quaternion.AngleAxis((0f - Time.time) * 30f, camera.transform.forward);
			rewardStarburstGo.transform.LookAt(rewardStarburstGo.transform.position + quaternion * camera.transform.up, -camera.transform.forward);
		}
	}

	private static void SetBlockPosRot(Block b, Vector3 pos, Quaternion rot)
	{
		if (b != null)
		{
			b.goT.position = pos;
			b.goT.rotation = rot;
		}
	}

	private static void DestroyRewardBlocks(bool destroySpin = true, bool destroyMove = true)
	{
		if (destroySpin)
		{
			if (rewardSpinBlock != null)
			{
				rewardSpinBlock.Destroy();
				rewardSpinBlock = null;
			}
			if (rewardSpinModel != null)
			{
				foreach (Block rewardSpinBlock in rewardSpinBlocks)
				{
					rewardSpinBlock.Destroy();
				}
				rewardSpinBlocks = null;
				UnityEngine.Object.Destroy(rewardSpinModel);
				rewardSpinModel = null;
			}
		}
		if (rewardMoveBlock != null && destroyMove)
		{
			rewardMoveBlock.Destroy();
			rewardMoveBlock = null;
		}
	}

	private static void IncrementInventoryReward(GAF gaf)
	{
		if (Scarcity.inventory != null && startInventoryValue != -1)
		{
			int count = startInventoryValue + rewardBlocksReceived;
			SetRewardInventory(rewardBlockTile.gaf, count);
			Scarcity.inventoryScales[rewardBlockTile.gaf] = 1.5f;
			Sound.PlaySound("Reward Counter", Sound.GetOrCreateOneShotAudioSource(), oneShot: true);
		}
	}

	private static void CleanUp()
	{
		Camera rewardCamera = Blocksworld.rewardCamera;
		DestroyRewardBlocks();
		if (rewardStarburstGo != null && rewardStarburstGo.GetComponent<Renderer>() != null)
		{
			rewardStarburstGo.GetComponent<Renderer>().enabled = false;
		}
		rewardBlockTile = null;
		rewardStateDurations = null;
		rewardSpinBlock = null;
		rewardAnimationRunning = false;
		if (rewardBlockTile != null)
		{
			Scarcity.inventoryScales[rewardBlockTile.gaf] = 1f;
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

	private static void EndWait()
	{
		if (prevRewardState == RewardVisualizationState.IN_INVENTORY)
		{
			rewardStarburstGo.GetComponent<Renderer>().enabled = false;
			Blocksworld.UI.SidePanel.Hide();
		}
	}

	private static Vector3 GetModelTargetPos()
	{
		UITabBar tabBar = Blocksworld.UI.TabBar;
		if (tabBar != null)
		{
			return tabBar.GetTabBarPosition(TabBarTabId.Models) + new Vector3(1f, 1f, 0f) * tabBar.GetTabHeight() * 0.5f;
		}
		return Vector3.zero;
	}

	private static void InInventory(Camera camera)
	{
		bool flag = rewardSpinModel != null;
		Vector3 vector = Vector3.zero;
		if (flag)
		{
			vector = GetModelTargetPos();
		}
		else if (rewardBlockTile != null && rewardBlockTile.tileObject != null)
		{
			vector = rewardBlockTile.tileObject.GetPosition() + new Vector3(40f, 40f, 0f);
		}
		if (prevRewardState == RewardVisualizationState.MOVES_TOWARDS_INVENTORY)
		{
			Vector3 position = vector * NormalizedScreen.scale;
			EmitRewardStars(camera.ScreenToWorldPoint(position), tileHitParticles);
			rewardBlocksReceived++;
			if (!flag && rewardBlockTile != null)
			{
				IncrementInventoryReward(rewardBlockTile.gaf);
			}
		}
		DestroyRewardBlocks(flag);
	}

	private static void MoveTowardsInventory(Camera camera, float timeFraction, float rotAngle, int rewardAnimationIndex, RewardVisualizationState state, string awardName)
	{
		Vector3 position = camera.transform.position;
		Quaternion rotation = camera.transform.rotation;
		float num = 1f - timeFraction;
		if (camera != null)
		{
			EmitRewardStars(position + camera.transform.forward * centralParticlesDistance, centralParticles);
		}
		bool flag = rewardSpinModel != null;
		if (prevRewardState != state && !flag && rewardBlockTile != null)
		{
			rewardMoveBlock = CreateRewardBlock(awardName);
		}
		Vector3 vector = Vector3.zero;
		if (flag)
		{
			vector = GetModelTargetPos();
		}
		else if (rewardBlockTile != null && rewardBlockTile.tileObject != null)
		{
			vector = rewardBlockTile.tileObject.GetPosition() + new Vector3(40f, 40f, 0f);
		}
		if (rewardAnimationIndex != prevRewardAnimationIndex)
		{
			Sound.PlaySound("Reward Swhoosh", Sound.GetOrCreateOneShotAudioSource(), oneShot: true);
			rewardBlocksLeft--;
			if (rewardBlocksLeft == 0 && !flag)
			{
				DestroyRewardBlocks(destroySpin: true, destroyMove: false);
			}
			if (prevRewardState == state)
			{
				EmitRewardStars(camera.ScreenToWorldPoint(vector), tileHitParticles);
				rewardBlocksReceived++;
				if (rewardBlockTile != null)
				{
					IncrementInventoryReward(rewardBlockTile.gaf);
				}
			}
		}
		GameObject gameObject = null;
		if (rewardMoveBlock != null && rewardMoveBlock.go != null)
		{
			gameObject = rewardMoveBlock.go;
		}
		else if (rewardSpinModel != null)
		{
			gameObject = rewardSpinModel;
		}
		if (gameObject != null && camera != null)
		{
			Vector3 vector2 = position + camera.transform.forward * camDist;
			Vector3 position2 = vector * NormalizedScreen.scale;
			Vector3 vector3 = camera.ScreenToWorldPoint(position2);
			float num2 = rewardExitCurve.Evaluate(timeFraction);
			Vector3 vector4 = vector2 * (1f - num2) + vector3 * num2;
			gameObject.transform.position = vector4;
			if (rewardBlocksLeft == 0)
			{
				MoveRewardStarburst(vector4, starburstSize * num, camera);
			}
			else
			{
				MoveRewardStarburst(position + camera.transform.forward * 6f, starburstSize, camera);
			}
		}
		Vector3 rotAxis = GetRotAxis();
		if (rewardSpinBlock != null && rewardSpinBlock.go != null)
		{
			rewardSpinBlock.goT.rotation = rotation * Quaternion.AngleAxis(rotAngle, rotAxis) * modelInitRotation;
		}
		if (gameObject != null)
		{
			gameObject.transform.rotation = rotation * Quaternion.AngleAxis(rotAngle, rotAxis) * modelInitRotation;
		}
	}

	private static void Spin(Camera camera, float timeFraction, float rotAngle, string awardName)
	{
		Vector3 position = camera.transform.position;
		Quaternion rotation = camera.transform.rotation;
		EmitRewardStars(position + camera.transform.forward * centralParticlesDistance, centralParticles);
		MoveRewardStarburst(position + camera.transform.forward * 6f, starburstSize, camera);
		GameObject gameObject = null;
		bool flag = false;
		if (rewardSpinBlock != null && rewardSpinBlock.go != null)
		{
			gameObject = rewardSpinBlock.go;
		}
		else if (rewardSpinModel != null)
		{
			gameObject = rewardSpinModel;
			flag = true;
		}
		Vector3 rotAxis = GetRotAxis();
		if (gameObject != null && camera != null)
		{
			gameObject.transform.position = position + camera.transform.forward * camDist;
			gameObject.transform.rotation = rotation * Quaternion.AngleAxis(rotAngle, rotAxis) * modelInitRotation;
		}
		float num = ((!flag) ? 0.5f : 0.75f);
		if (rewardBlockTile != null || !(timeFraction > num) || rewardFailed)
		{
			return;
		}
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
						string text = (string)tile.gaf.Args[0];
						if (text == awardName)
						{
							rewardBlockTile = tile;
							break;
						}
					}
				}
			}
		}
		if (rewardBlockTile == null)
		{
			if (flag)
			{
				rewardBlockTile = Block.ThenTile();
			}
			else
			{
				BWLog.Info("Could not find reward tile for " + awardName + " creating fake!");
				Tile tile2 = CreateFakeRewardTile(awardName);
				tile2.Show(show: true);
				rewardBlockTile = tile2;
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
					rewardBlockTile = null;
					rewardFailed = true;
				}
			}
		}
		if (rewardBlockTile == null)
		{
			return;
		}
		if (!flag && Scarcity.inventory != null)
		{
			if (!Scarcity.globalInventory.TryGetValue(rewardBlockTile.gaf, out var value))
			{
				value = 0;
			}
			int value2 = 0;
			if (Scarcity.worldGAFUsage != null && Scarcity.worldGAFUsage.TryGetValue(rewardBlockTile.gaf, out value2) && value2 < 0)
			{
				value2 = 0;
			}
			startInventoryValue = value + value2 - rewardBlocksCount;
			SetRewardInventory(rewardBlockTile.gaf, startInventoryValue);
		}
		Blocksworld.buildPanel.Show(show: true);
		int tabIndex = (flag ? 1 : 0);
		Blocksworld.buildPanel.GetTabBar().SetSelectedTab(tabIndex, playSound: false);
		Blocksworld.buildPanel.PositionReset(hide: true);
		if (!flag)
		{
			Blocksworld.buildPanel.DisableTilesBut(rewardBlockTile);
			Blocksworld.buildPanel.ScrollToVisible(rewardBlockTile, immediately: true);
		}
		Blocksworld.buildPanel.SnapBackInsideBounds(immediately: true);
		Blocksworld.UI.SidePanel.Show();
	}

	private static void SetRewardInventory(GAF gaf, int count)
	{
		if (count <= 0 || startInventoryValue < 0)
		{
			count = -1;
		}
		Scarcity.inventory[gaf] = count;
	}

	private static void Appear(Camera camera, float timeFraction, float rotAngle)
	{
		Vector3 position = camera.transform.position;
		Quaternion rotation = camera.transform.rotation;
		EmitRewardStars(position + camera.transform.forward * centralParticlesDistance, centralParticles);
		MoveRewardStarburst(position + camera.transform.forward * 6f, starburstSize * timeFraction, camera);
		GameObject gameObject = null;
		if (rewardSpinBlock != null && rewardSpinBlock.go != null)
		{
			gameObject = rewardSpinBlock.go;
		}
		else if (rewardSpinModel != null)
		{
			gameObject = rewardSpinModel;
		}
		Vector3 rotAxis = GetRotAxis();
		Quaternion quaternion = rotation * Quaternion.AngleAxis(rotAngle, rotAxis) * modelInitRotation;
		if (!(gameObject != null))
		{
			return;
		}
		Vector3 position2;
		Quaternion rotation2;
		if (worldModelBlock == null)
		{
			position2 = position + camera.transform.forward * (camDist + rewardEnterCurve.Evaluate(timeFraction) * maxDist);
			rotation2 = quaternion;
		}
		else
		{
			if (worldModelBlock.go.activeSelf)
			{
				List<Block> list = Block.connectedCache[worldModelBlock];
				foreach (Block item in list)
				{
					item.chunk.go.SetActive(value: false);
					item.Deactivate();
				}
			}
			gameObject.transform.localScale = Vector3.one * ((1f - timeFraction) * 1f + timeFraction * modelScaler);
			position2 = (1f - timeFraction) * rewardModelBlockStartPos + timeFraction * (position + camera.transform.forward * camDist);
			rotation2 = Quaternion.Lerp(Quaternion.AngleAxis(rotAngle, rotAxis) * rewardModelStartRotation, quaternion, timeFraction);
		}
		gameObject.transform.position = position2;
		gameObject.transform.rotation = rotation2;
	}

	private static bool SanityCheckAfterPrepare()
	{
		if (rewardStarburstGo == null)
		{
			BWLog.Info("Reward starburst was null");
			return false;
		}
		if (rewardStarburstGo.GetComponent<Renderer>() == null)
		{
			BWLog.Info("Reward starburst renderer was null");
			return false;
		}
		return true;
	}

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

	public static TileResultCode VisualizeReward(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float timer = eInfo.timer;
		if (timer == 0f && rewardAnimationRunning)
		{
			return TileResultCode.True;
		}
		bool flag = !rewardAnimationRunning;
		if (!SanityCheckBeforePrepare())
		{
			CleanUp();
			return TileResultCode.True;
		}
		Camera rewardCamera = Blocksworld.rewardCamera;
		if (flag)
		{
			rewardCamera.enabled = true;
			Sound.PlaySound("Reward Appear", Sound.GetOrCreateOneShotAudioSource(), oneShot: true);
			Sound.PlaySound("Reward Appear", Sound.GetOrCreateOneShotAudioSource(), oneShot: true);
		}
		string awardName = "Rocket";
		if (args.Length != 0)
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
		PrepareRewardVisualization(awardName, num);
		if (!SanityCheckAfterPrepare())
		{
			CleanUp();
			return TileResultCode.True;
		}
		float num2 = 0f;
		float timeFraction = 0f;
		RewardVisualizationState rewardVisualizationState = RewardVisualizationState.APPEARS;
		int num3 = 0;
		for (int i = 0; i < rewardAnimationList.Count; i++)
		{
			RewardVisualizationState rewardVisualizationState2 = rewardAnimationList[i];
			float num4 = rewardStateDurations[rewardVisualizationState2];
			if (num2 > timer)
			{
				break;
			}
			rewardVisualizationState = rewardVisualizationState2;
			float num5 = timer - num2;
			timeFraction = Mathf.Clamp(num5 / num4, 0f, 1f);
			num2 += num4;
			num3++;
		}
		if (timer >= num2)
		{
			CleanUp();
			return TileResultCode.True;
		}
		float rotAngle = timer * rotationSpeed;
		if (flag)
		{
			rewardStarburstGo.GetComponent<Renderer>().enabled = true;
		}
		switch (rewardVisualizationState)
		{
		case RewardVisualizationState.APPEARS:
			Appear(rewardCamera, timeFraction, rotAngle);
			break;
		case RewardVisualizationState.SPINS:
			Spin(rewardCamera, timeFraction, rotAngle, awardName);
			break;
		case RewardVisualizationState.MOVES_TOWARDS_INVENTORY:
			MoveTowardsInventory(rewardCamera, timeFraction, rotAngle, num3, rewardVisualizationState, awardName);
			break;
		case RewardVisualizationState.IN_INVENTORY:
			InInventory(rewardCamera);
			break;
		case RewardVisualizationState.END_WAIT:
			EndWait();
			break;
		}
		prevRewardState = rewardVisualizationState;
		prevRewardAnimationIndex = num3;
		return TileResultCode.Delayed;
	}
}
