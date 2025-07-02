using System;
using System.Collections;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class ModelTutorializeUtils
{
	public class StepByStepTutorializeOptions
	{
		public bool disableRendererForUnplacedBlocks = true;

		public float waitTimePerBlock = 0.01f;

		public int symmetryChainPenaltyThreshold = 3;

		public float symmetryChainPenalty = 0.2f;

		public float heightPenaltyFactor = 0.1f;

		public float maxHeightPenalty = 1f;

		public float selfOcclusionPenaltyFactor = 0.1f;

		public float upOcclusionPenaltyFactor = 0.5f;

		public float sideOcclusionPenaltyFactor = 0.3f;

		public float paintedBlockOcclusionSeverityBias = 0.1f;

		public float paintedBlockOcclusionSeverityMultiplier = 2f;

		public float texturedBlockOcclusionSeverityBias = 0.1f;

		public float texturedBlockOcclusionSeverityMultiplier = 2f;

		public float texturedSideBlockOcclusionSeverityBias = 0.3f;

		public float texturedSideBlockOcclusionSeverityMultiplier = 2.5f;

		public float connectedToLastBlockReward = 1f;

		public float isolatedBlockPenalty = 3f;

		public Func<float, float> distanceToLastBlockRewardFunction = (float dist) => 1f / (1f + 0.25f * dist);
	}

	private class SymmetricBlocks
	{
		public List<Block> side1 = new List<Block>();

		public List<Block> side2 = new List<Block>();

		public int coordIndex;

		public float centerCoord;

		public void Add(Block b1, Block b2)
		{
			side1.Add(b1);
			side2.Add(b2);
		}

		public int PairCount()
		{
			return side1.Count;
		}

		public static string CalculateLookupKey(int coordIndex, float centerCoord)
		{
			return coordIndex.ToString() + Mathf.RoundToInt(centerCoord * 10f);
		}

		public override string ToString()
		{
			return $"[SymmetricBlocks pair count: {PairCount()}, coordIndex: {coordIndex}, centerCoord: {centerCoord}]";
		}
	}

	private class BlockOcclusion
	{
		public Block block;

		public float severity;
	}

	public static void PrepareForStepByStepTutorial(List<Block> modelBlocks, StepByStepTutorializeOptions options = null, Action completionHandler = null)
	{
		Blocksworld.bw.StartCoroutine(PrepareForStepByStepTutorialTimeSliced(modelBlocks, options, completionHandler));
	}

	private static IEnumerator PrepareForStepByStepTutorialTimeSliced(List<Block> modelBlocks, StepByStepTutorializeOptions options = null, Action completionHandler = null)
	{
		if (options == null)
		{
			options = new StepByStepTutorializeOptions();
		}
		List<Block> allBlocks = BWSceneManager.AllBlocks();
		Dictionary<Block, int[]> blockIndexMap = GetIdentityBlockIndexMap(allBlocks);
		HashSet<Block> blocksToPlace = new HashSet<Block>();
		HashSet<Block> placedBlocks = new HashSet<Block>();
		CalculateBlocksToPlace(allBlocks, modelBlocks, blocksToPlace, placedBlocks);
		float minY = float.MaxValue;
		foreach (Block item2 in blocksToPlace)
		{
			if (options.disableRendererForUnplacedBlocks)
			{
				EnableRenderers(item2, enable: false);
			}
			minY = Mathf.Min(item2.go.transform.position.y, minY);
		}
		Dictionary<Block, Block> blockSymmetryMap = new Dictionary<Block, Block>();
		Dictionary<Block, SymmetricBlocks> blockSymmetries = new Dictionary<Block, SymmetricBlocks>();
		CalculateBlockSymmetries(blocksToPlace, blockSymmetryMap, blockSymmetries);
		Dictionary<Block, float> blockOcclusions = new Dictionary<Block, float>();
		Dictionary<Block, List<BlockOcclusion>> blockOtherOcclusions = new Dictionary<Block, List<BlockOcclusion>>();
		CalculateBlockOcclusions(blocksToPlace, blockOcclusions, blockOtherOcclusions, options);
		SymmetricBlocks currentSymmetry = null;
		List<Block> symmetryChain = new List<Block>();
		List<Block> sortedBlocks = new List<Block>();
		Block lastPlacedBlock = null;
		int index = 0;
		while (blocksToPlace.Count > 0)
		{
			if (options.waitTimePerBlock > 0f)
			{
				yield return new WaitForSeconds(options.waitTimePerBlock);
			}
			FindNextBlocksToPlace(blocksToPlace, placedBlocks, blockSymmetryMap, blockSymmetries, blockOcclusions, blockOtherOcclusions, symmetryChain, sortedBlocks, minY, ref index, ref lastPlacedBlock, ref currentSymmetry, options);
		}
		for (int i = 0; i < sortedBlocks.Count; i++)
		{
			Block item = sortedBlocks[i];
			allBlocks.Remove(item);
			allBlocks.Add(item);
		}
		UpdateBlockIndexMapAndInformBlocks(allBlocks, blockIndexMap);
		completionHandler?.Invoke();
	}

	private static void FindNextBlocksToPlace(HashSet<Block> blocksToPlace, HashSet<Block> placedBlocks, Dictionary<Block, Block> blockSymmetryMap, Dictionary<Block, SymmetricBlocks> blockSymmetries, Dictionary<Block, float> blockOcclusions, Dictionary<Block, List<BlockOcclusion>> blockOtherOcclusions, List<Block> symmetryChain, List<Block> sortedBlocks, float minY, ref int index, ref Block lastPlacedBlock, ref SymmetricBlocks currentSymmetry, StepByStepTutorializeOptions options)
	{
		float num = float.MinValue;
		Block block = null;
		foreach (Block item in blocksToPlace)
		{
			Vector3 position = item.go.transform.position;
			float num2 = 0f;
			float y = position.y;
			float num3 = Mathf.Max((0f - options.heightPenaltyFactor) * (y - minY), 0f - options.maxHeightPenalty);
			num2 += num3;
			float num4 = 0f;
			bool flag = false;
			for (int i = 0; i < item.connections.Count; i++)
			{
				Block block2 = item.connections[i];
				if (placedBlocks.Contains(block2) && block2.GetType() != typeof(BlockPosition))
				{
					flag = true;
				}
				if (block2 == lastPlacedBlock)
				{
					num4 += options.connectedToLastBlockReward;
				}
			}
			if (!flag)
			{
				num4 -= options.isolatedBlockPenalty;
			}
			num2 += num4;
			float num5 = 0f;
			if (lastPlacedBlock != null)
			{
				float magnitude = (lastPlacedBlock.go.transform.position - position).magnitude;
				num5 = options.distanceToLastBlockRewardFunction(magnitude);
			}
			num2 += num5;
			float num6 = blockOcclusions[item];
			num2 += num6;
			if (blockSymmetries.ContainsKey(item) && blockSymmetries[item] == currentSymmetry && symmetryChain.Count > options.symmetryChainPenaltyThreshold)
			{
				num2 -= (float)(symmetryChain.Count - options.symmetryChainPenaltyThreshold) * options.symmetryChainPenalty;
			}
			float num7 = 0f;
			if (blockOtherOcclusions.TryGetValue(item, out var value))
			{
				for (int j = 0; j < value.Count; j++)
				{
					BlockOcclusion blockOcclusion = value[j];
					if (blocksToPlace.Contains(blockOcclusion.block))
					{
						num7 -= blockOcclusion.severity * options.selfOcclusionPenaltyFactor;
					}
				}
			}
			num2 += num7;
			if (num2 > num && (!symmetryChain.Contains(item) || block == null))
			{
				num = num2;
				block = item;
			}
		}
		if (block == null)
		{
			BWLog.Error("Best block was null. Should never happen...");
			return;
		}
		bool flag2 = true;
		if (!symmetryChain.Contains(block) && blockSymmetryMap.TryGetValue(block, out var value2) && (blockSymmetries[block] == currentSymmetry || currentSymmetry == null))
		{
			currentSymmetry = blockSymmetries[block];
			symmetryChain.Add(value2);
		}
		else if (symmetryChain.Count > 0)
		{
			for (int k = 0; k < symmetryChain.Count; k++)
			{
				Block block3 = symmetryChain[k];
				if (blocksToPlace.Contains(block3))
				{
					blocksToPlace.Remove(block3);
					sortedBlocks.Add(block3);
					placedBlocks.Add(block3);
					lastPlacedBlock = block3;
					if (options.disableRendererForUnplacedBlocks)
					{
						EnableRenderers(block3, enable: true);
					}
					index++;
				}
			}
			symmetryChain.Clear();
			currentSymmetry = null;
			flag2 = false;
		}
		if (flag2)
		{
			blocksToPlace.Remove(block);
			sortedBlocks.Add(block);
			placedBlocks.Add(block);
			lastPlacedBlock = block;
			if (options.disableRendererForUnplacedBlocks)
			{
				EnableRenderers(block, enable: true);
			}
			index++;
		}
	}

	private static void UpdateBlockIndexMapAndInformBlocks(List<Block> allBlocks, Dictionary<Block, int[]> blockIndexMap)
	{
		for (int i = 0; i < allBlocks.Count; i++)
		{
			Block key = allBlocks[i];
			blockIndexMap[key][1] = i;
		}
		foreach (KeyValuePair<Block, int[]> item in blockIndexMap)
		{
			int[] value = item.Value;
			if (value[0] != value[1])
			{
				for (int j = 0; j < allBlocks.Count; j++)
				{
					Block block = allBlocks[j];
					block.IndicesSwitched(value[0], value[1]);
				}
			}
		}
	}

	private static void EnableRenderers(Block b, bool enable)
	{
		MeshRenderer[] componentsInChildren = b.go.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer meshRenderer in componentsInChildren)
		{
			meshRenderer.enabled = enable;
		}
	}

	private static Dictionary<string, List<Block>> GatherBlockTypeClustersWithSameScale(List<Block> blocks)
	{
		Dictionary<string, List<Block>> dictionary = new Dictionary<string, List<Block>>();
		for (int i = 0; i < blocks.Count; i++)
		{
			Block block = blocks[i];
			Vector3 v = block.goT.rotation * block.Scale();
			string key = block.BlockType() + Util.Round(Util.Abs(v)).ToString();
			if (!dictionary.TryGetValue(key, out var value))
			{
				value = (dictionary[key] = new List<Block>());
			}
			value.Add(block);
		}
		return dictionary;
	}

	private static void UpdateSymmetry(Block b1, Block b2, Dictionary<string, SymmetricBlocks> symmetries)
	{
		Vector3 position = b1.GetPosition();
		Vector3 position2 = b2.GetPosition();
		int num = -1;
		bool flag = true;
		for (int i = 0; i < 3; i++)
		{
			int num2 = i;
			float f = position[num2] - position2[num2];
			float num3 = Mathf.Abs(f);
			if (num3 > 0.01f)
			{
				if (num != -1)
				{
					flag = false;
					break;
				}
				num = num2;
			}
		}
		if (flag && num != 1 && num != -1)
		{
			float num4 = position[num];
			float num5 = position2[num];
			float centerCoord = 0.5f * (num4 + num5);
			string key = SymmetricBlocks.CalculateLookupKey(num, centerCoord);
			if (!symmetries.TryGetValue(key, out var value))
			{
				value = (symmetries[key] = new SymmetricBlocks
				{
					coordIndex = num,
					centerCoord = centerCoord
				});
			}
			if (num4 < num5)
			{
				value.Add(b1, b2);
			}
			else
			{
				value.Add(b2, b1);
			}
		}
	}

	private static Dictionary<string, SymmetricBlocks> FindSymmetricBlocks(List<Block> blocks)
	{
		Dictionary<string, List<Block>> dictionary = GatherBlockTypeClustersWithSameScale(blocks);
		Dictionary<string, SymmetricBlocks> dictionary2 = new Dictionary<string, SymmetricBlocks>();
		foreach (KeyValuePair<string, List<Block>> item in dictionary)
		{
			List<Block> value = item.Value;
			for (int i = 0; i < value.Count; i++)
			{
				for (int j = i + 1; j < value.Count; j++)
				{
					UpdateSymmetry(value[i], value[j], dictionary2);
				}
			}
		}
		return dictionary2;
	}

	private static void CalculateBlocksToPlace(List<Block> allBlocks, List<Block> modelBlocks, HashSet<Block> blocksToPlace, HashSet<Block> placedBlocks)
	{
		for (int i = 0; i < modelBlocks.Count; i++)
		{
			Block block = modelBlocks[i];
			if (!block.IsLocked())
			{
				blocksToPlace.Add(block);
			}
		}
		for (int j = 0; j < allBlocks.Count; j++)
		{
			Block block2 = allBlocks[j];
			BlockTankTreadsWheel blockTankTreadsWheel = block2 as BlockTankTreadsWheel;
			if (block2.IsLocked() || blockTankTreadsWheel != null)
			{
				placedBlocks.Add(block2);
				blocksToPlace.Remove(block2);
			}
			if (blockTankTreadsWheel != null)
			{
				BlockGroup groupOfType = blockTankTreadsWheel.GetGroupOfType("tank-treads");
				Block mainBlockInGroup = blockTankTreadsWheel.GetMainBlockInGroup("tank-treads");
				if (!mainBlockInGroup.IsLocked())
				{
					mainBlockInGroup.AddLockedTileRow();
				}
				placedBlocks.UnionWith(groupOfType.GetBlocks());
			}
		}
	}

	private static Dictionary<Block, int[]> GetIdentityBlockIndexMap(List<Block> allBlocks)
	{
		Dictionary<Block, int[]> dictionary = new Dictionary<Block, int[]>();
		for (int i = 0; i < allBlocks.Count; i++)
		{
			Block key = allBlocks[i];
			dictionary[key] = new int[2] { i, i };
		}
		return dictionary;
	}

	private static void CalculateBlockOcclusions(HashSet<Block> blocksToPlace, Dictionary<Block, float> blockOcclusions, Dictionary<Block, List<BlockOcclusion>> blockOtherOcclusions, StepByStepTutorializeOptions options)
	{
		Dictionary<Vector3, float> dictionary = new Dictionary<Vector3, float>
		{
			{
				Vector3.up,
				options.upOcclusionPenaltyFactor
			},
			{
				(Vector3.right + Vector3.up * 0.5f).normalized,
				options.sideOcclusionPenaltyFactor
			},
			{
				(Vector3.right + Vector3.forward + Vector3.up * 0.5f).normalized,
				options.sideOcclusionPenaltyFactor
			},
			{
				(Vector3.right + Vector3.back + Vector3.up * 0.5f).normalized,
				options.sideOcclusionPenaltyFactor
			},
			{
				(Vector3.left + Vector3.up * 0.5f).normalized,
				options.sideOcclusionPenaltyFactor
			},
			{
				(Vector3.left + Vector3.forward + Vector3.up * 0.5f).normalized,
				options.sideOcclusionPenaltyFactor
			},
			{
				(Vector3.left + Vector3.back + Vector3.up * 0.5f).normalized,
				options.sideOcclusionPenaltyFactor
			},
			{
				(Vector3.forward + Vector3.up * 0.5f).normalized,
				options.sideOcclusionPenaltyFactor
			},
			{
				(Vector3.back + Vector3.up * 0.5f).normalized,
				options.sideOcclusionPenaltyFactor
			}
		};
		foreach (Block item in blocksToPlace)
		{
			string blockType = item.BlockType();
			string[] array = Scarcity.DefaultPaints(blockType);
			string[] array2 = Scarcity.DefaultTextures(blockType);
			float num = 0f;
			float num2 = 1f;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != item.GetPaint(i))
				{
					num = Mathf.Max(options.paintedBlockOcclusionSeverityBias, num);
					num2 = Mathf.Max(num2, options.paintedBlockOcclusionSeverityMultiplier);
				}
				string texture = item.GetTexture(i);
				if (!(array2[i] != texture))
				{
					continue;
				}
				num = Mathf.Max(options.texturedBlockOcclusionSeverityBias, num);
				num2 = Mathf.Max(num2, options.paintedBlockOcclusionSeverityMultiplier);
				if (Materials.textureInfos.TryGetValue(texture, out var value))
				{
					Mapping mapping = value.mapping;
					if ((uint)(mapping - 2) <= 1u || (uint)(mapping - 6) <= 1u)
					{
						num = Mathf.Max(options.texturedSideBlockOcclusionSeverityBias, num);
						num2 = Mathf.Max(num2, options.texturedSideBlockOcclusionSeverityMultiplier);
					}
				}
			}
			Vector3 position = item.go.transform.position;
			float num3 = 0.5f * Util.MeanAbs(item.Scale());
			float num4 = 15f + num3;
			foreach (KeyValuePair<Vector3, float> item2 in dictionary)
			{
				Vector3 key = item2.Key;
				RaycastHit[] array3 = Physics.RaycastAll(position, key, num4, -1);
				float num5 = 0f;
				RaycastHit[] array4 = array3;
				for (int j = 0; j < array4.Length; j++)
				{
					RaycastHit raycastHit = array4[j];
					Collider collider = raycastHit.collider;
					Block block = BWSceneManager.FindBlock(collider.gameObject, checkChildGos: true);
					if (block != null && block != item)
					{
						float num6 = 1f - raycastHit.distance / num4;
						float num7 = num6 * item2.Value;
						if (!blockOtherOcclusions.TryGetValue(block, out var value2))
						{
							value2 = (blockOtherOcclusions[block] = new List<BlockOcclusion>());
						}
						value2.Add(new BlockOcclusion
						{
							block = item,
							severity = num7
						});
						num5 = Mathf.Max(num7, num5);
					}
				}
				num += num5 * num2;
			}
			blockOcclusions[item] = num;
		}
	}

	private static void CalculateBlockSymmetries(HashSet<Block> blocksToPlace, Dictionary<Block, Block> blockSymmetryMap, Dictionary<Block, SymmetricBlocks> blockSymmetries)
	{
		Dictionary<string, SymmetricBlocks> dictionary = FindSymmetricBlocks(new List<Block>(blocksToPlace));
		HashSet<Block> hashSet = new HashSet<Block>(blocksToPlace);
		List<SymmetricBlocks> list = new List<SymmetricBlocks>(dictionary.Values);
		list.Sort((SymmetricBlocks s1, SymmetricBlocks s2) => s2.side1.Count.CompareTo(s1.side1.Count));
		int num = ((list.Count > 0) ? list[0].side1.Count : 0);
		for (int num2 = 0; num2 < list.Count; num2++)
		{
			SymmetricBlocks symmetricBlocks = list[num2];
			if (symmetricBlocks.side1.Count == 1 && num > 2)
			{
				continue;
			}
			for (int num3 = 0; num3 < symmetricBlocks.side1.Count; num3++)
			{
				Block block = symmetricBlocks.side1[num3];
				Block block2 = symmetricBlocks.side2[num3];
				if (hashSet.Contains(block) && hashSet.Contains(block2))
				{
					blockSymmetryMap[block] = block2;
					blockSymmetryMap[block2] = block;
					blockSymmetries[block] = symmetricBlocks;
					blockSymmetries[block2] = symmetricBlocks;
					hashSet.Remove(block);
					hashSet.Remove(block2);
				}
			}
		}
	}
}
