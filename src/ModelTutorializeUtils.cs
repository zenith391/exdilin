using System;
using System.Collections;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x020001DE RID: 478
public class ModelTutorializeUtils
{
	// Token: 0x0600189D RID: 6301 RVA: 0x000AC76F File Offset: 0x000AAB6F
	public static void PrepareForStepByStepTutorial(List<Block> modelBlocks, ModelTutorializeUtils.StepByStepTutorializeOptions options = null, Action completionHandler = null)
	{
		Blocksworld.bw.StartCoroutine(ModelTutorializeUtils.PrepareForStepByStepTutorialTimeSliced(modelBlocks, options, completionHandler));
	}

	// Token: 0x0600189E RID: 6302 RVA: 0x000AC784 File Offset: 0x000AAB84
	private static IEnumerator PrepareForStepByStepTutorialTimeSliced(List<Block> modelBlocks, ModelTutorializeUtils.StepByStepTutorializeOptions options = null, Action completionHandler = null)
	{
		if (options == null)
		{
			options = new ModelTutorializeUtils.StepByStepTutorializeOptions();
		}
		List<Block> allBlocks = BWSceneManager.AllBlocks();
		Dictionary<Block, int[]> blockIndexMap = ModelTutorializeUtils.GetIdentityBlockIndexMap(allBlocks);
		HashSet<Block> blocksToPlace = new HashSet<Block>();
		HashSet<Block> placedBlocks = new HashSet<Block>();
		ModelTutorializeUtils.CalculateBlocksToPlace(allBlocks, modelBlocks, blocksToPlace, placedBlocks);
		float minY = float.MaxValue;
		foreach (Block block in blocksToPlace)
		{
			if (options.disableRendererForUnplacedBlocks)
			{
				ModelTutorializeUtils.EnableRenderers(block, false);
			}
			minY = Mathf.Min(block.go.transform.position.y, minY);
		}
		Dictionary<Block, Block> blockSymmetryMap = new Dictionary<Block, Block>();
		Dictionary<Block, ModelTutorializeUtils.SymmetricBlocks> blockSymmetries = new Dictionary<Block, ModelTutorializeUtils.SymmetricBlocks>();
		ModelTutorializeUtils.CalculateBlockSymmetries(blocksToPlace, blockSymmetryMap, blockSymmetries);
		Dictionary<Block, float> blockOcclusions = new Dictionary<Block, float>();
		Dictionary<Block, List<ModelTutorializeUtils.BlockOcclusion>> blockOtherOcclusions = new Dictionary<Block, List<ModelTutorializeUtils.BlockOcclusion>>();
		ModelTutorializeUtils.CalculateBlockOcclusions(blocksToPlace, blockOcclusions, blockOtherOcclusions, options);
		ModelTutorializeUtils.SymmetricBlocks currentSymmetry = null;
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
			ModelTutorializeUtils.FindNextBlocksToPlace(blocksToPlace, placedBlocks, blockSymmetryMap, blockSymmetries, blockOcclusions, blockOtherOcclusions, symmetryChain, sortedBlocks, minY, ref index, ref lastPlacedBlock, ref currentSymmetry, options);
		}
		for (int i = 0; i < sortedBlocks.Count; i++)
		{
			Block item = sortedBlocks[i];
			allBlocks.Remove(item);
			allBlocks.Add(item);
		}
		ModelTutorializeUtils.UpdateBlockIndexMapAndInformBlocks(allBlocks, blockIndexMap);
		if (completionHandler != null)
		{
			completionHandler();
		}
		yield break;
	}

	// Token: 0x0600189F RID: 6303 RVA: 0x000AC7B0 File Offset: 0x000AABB0
	private static void FindNextBlocksToPlace(HashSet<Block> blocksToPlace, HashSet<Block> placedBlocks, Dictionary<Block, Block> blockSymmetryMap, Dictionary<Block, ModelTutorializeUtils.SymmetricBlocks> blockSymmetries, Dictionary<Block, float> blockOcclusions, Dictionary<Block, List<ModelTutorializeUtils.BlockOcclusion>> blockOtherOcclusions, List<Block> symmetryChain, List<Block> sortedBlocks, float minY, ref int index, ref Block lastPlacedBlock, ref ModelTutorializeUtils.SymmetricBlocks currentSymmetry, ModelTutorializeUtils.StepByStepTutorializeOptions options)
	{
		float num = float.MinValue;
		Block block = null;
		foreach (Block block2 in blocksToPlace)
		{
			Vector3 position = block2.go.transform.position;
			float num2 = 0f;
			float y = position.y;
			float num3 = Mathf.Max(-options.heightPenaltyFactor * (y - minY), -options.maxHeightPenalty);
			num2 += num3;
			float num4 = 0f;
			bool flag = false;
			for (int i = 0; i < block2.connections.Count; i++)
			{
				Block block3 = block2.connections[i];
				if (placedBlocks.Contains(block3) && block3.GetType() != typeof(BlockPosition))
				{
					flag = true;
				}
				if (block3 == lastPlacedBlock)
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
			float num6 = blockOcclusions[block2];
			num2 += num6;
			if (blockSymmetries.ContainsKey(block2) && blockSymmetries[block2] == currentSymmetry && symmetryChain.Count > options.symmetryChainPenaltyThreshold)
			{
				num2 -= (float)(symmetryChain.Count - options.symmetryChainPenaltyThreshold) * options.symmetryChainPenalty;
			}
			float num7 = 0f;
			List<ModelTutorializeUtils.BlockOcclusion> list;
			if (blockOtherOcclusions.TryGetValue(block2, out list))
			{
				for (int j = 0; j < list.Count; j++)
				{
					ModelTutorializeUtils.BlockOcclusion blockOcclusion = list[j];
					if (blocksToPlace.Contains(blockOcclusion.block))
					{
						num7 -= blockOcclusion.severity * options.selfOcclusionPenaltyFactor;
					}
				}
			}
			num2 += num7;
			if (num2 > num && (!symmetryChain.Contains(block2) || block == null))
			{
				num = num2;
				block = block2;
			}
		}
		if (block == null)
		{
			BWLog.Error("Best block was null. Should never happen...");
		}
		else
		{
			bool flag2 = true;
			Block item;
			if (!symmetryChain.Contains(block) && blockSymmetryMap.TryGetValue(block, out item) && (blockSymmetries[block] == currentSymmetry || currentSymmetry == null))
			{
				currentSymmetry = blockSymmetries[block];
				symmetryChain.Add(item);
			}
			else if (symmetryChain.Count > 0)
			{
				for (int k = 0; k < symmetryChain.Count; k++)
				{
					Block block4 = symmetryChain[k];
					if (blocksToPlace.Contains(block4))
					{
						blocksToPlace.Remove(block4);
						sortedBlocks.Add(block4);
						placedBlocks.Add(block4);
						lastPlacedBlock = block4;
						if (options.disableRendererForUnplacedBlocks)
						{
							ModelTutorializeUtils.EnableRenderers(block4, true);
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
					ModelTutorializeUtils.EnableRenderers(block, true);
				}
				index++;
			}
		}
	}

	// Token: 0x060018A0 RID: 6304 RVA: 0x000ACB48 File Offset: 0x000AAF48
	private static void UpdateBlockIndexMapAndInformBlocks(List<Block> allBlocks, Dictionary<Block, int[]> blockIndexMap)
	{
		for (int i = 0; i < allBlocks.Count; i++)
		{
			Block key = allBlocks[i];
			blockIndexMap[key][1] = i;
		}
		foreach (KeyValuePair<Block, int[]> keyValuePair in blockIndexMap)
		{
			int[] value = keyValuePair.Value;
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

	// Token: 0x060018A1 RID: 6305 RVA: 0x000ACC0C File Offset: 0x000AB00C
	private static void EnableRenderers(Block b, bool enable)
	{
		foreach (MeshRenderer meshRenderer in b.go.GetComponentsInChildren<MeshRenderer>())
		{
			meshRenderer.enabled = enable;
		}
	}

	// Token: 0x060018A2 RID: 6306 RVA: 0x000ACC44 File Offset: 0x000AB044
	private static Dictionary<string, List<Block>> GatherBlockTypeClustersWithSameScale(List<Block> blocks)
	{
		Dictionary<string, List<Block>> dictionary = new Dictionary<string, List<Block>>();
		for (int i = 0; i < blocks.Count; i++)
		{
			Block block = blocks[i];
			Vector3 v = block.goT.rotation * block.Scale();
			string key = block.BlockType() + Util.Round(Util.Abs(v)).ToString();
			List<Block> list;
			if (!dictionary.TryGetValue(key, out list))
			{
				list = new List<Block>();
				dictionary[key] = list;
			}
			list.Add(block);
		}
		return dictionary;
	}

	// Token: 0x060018A3 RID: 6307 RVA: 0x000ACCE0 File Offset: 0x000AB0E0
	private static void UpdateSymmetry(Block b1, Block b2, Dictionary<string, ModelTutorializeUtils.SymmetricBlocks> symmetries)
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
		if (!flag || num == 1 || num == -1)
		{
			return;
		}
		float num4 = position[num];
		float num5 = position2[num];
		float centerCoord = 0.5f * (num4 + num5);
		string key = ModelTutorializeUtils.SymmetricBlocks.CalculateLookupKey(num, centerCoord);
		ModelTutorializeUtils.SymmetricBlocks symmetricBlocks;
		if (!symmetries.TryGetValue(key, out symmetricBlocks))
		{
			symmetricBlocks = new ModelTutorializeUtils.SymmetricBlocks
			{
				coordIndex = num,
				centerCoord = centerCoord
			};
			symmetries[key] = symmetricBlocks;
		}
		if (num4 < num5)
		{
			symmetricBlocks.Add(b1, b2);
		}
		else
		{
			symmetricBlocks.Add(b2, b1);
		}
	}

	// Token: 0x060018A4 RID: 6308 RVA: 0x000ACDF0 File Offset: 0x000AB1F0
	private static Dictionary<string, ModelTutorializeUtils.SymmetricBlocks> FindSymmetricBlocks(List<Block> blocks)
	{
		Dictionary<string, List<Block>> dictionary = ModelTutorializeUtils.GatherBlockTypeClustersWithSameScale(blocks);
		Dictionary<string, ModelTutorializeUtils.SymmetricBlocks> dictionary2 = new Dictionary<string, ModelTutorializeUtils.SymmetricBlocks>();
		foreach (KeyValuePair<string, List<Block>> keyValuePair in dictionary)
		{
			List<Block> value = keyValuePair.Value;
			for (int i = 0; i < value.Count; i++)
			{
				for (int j = i + 1; j < value.Count; j++)
				{
					ModelTutorializeUtils.UpdateSymmetry(value[i], value[j], dictionary2);
				}
			}
		}
		return dictionary2;
	}

	// Token: 0x060018A5 RID: 6309 RVA: 0x000ACEAC File Offset: 0x000AB2AC
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

	// Token: 0x060018A6 RID: 6310 RVA: 0x000ACF78 File Offset: 0x000AB378
	private static Dictionary<Block, int[]> GetIdentityBlockIndexMap(List<Block> allBlocks)
	{
		Dictionary<Block, int[]> dictionary = new Dictionary<Block, int[]>();
		for (int i = 0; i < allBlocks.Count; i++)
		{
			Block key = allBlocks[i];
			dictionary[key] = new int[]
			{
				i,
				i
			};
		}
		return dictionary;
	}

	// Token: 0x060018A7 RID: 6311 RVA: 0x000ACFC0 File Offset: 0x000AB3C0
	private static void CalculateBlockOcclusions(HashSet<Block> blocksToPlace, Dictionary<Block, float> blockOcclusions, Dictionary<Block, List<ModelTutorializeUtils.BlockOcclusion>> blockOtherOcclusions, ModelTutorializeUtils.StepByStepTutorializeOptions options)
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
		foreach (Block block in blocksToPlace)
		{
			string blockType = block.BlockType();
			string[] array = Scarcity.DefaultPaints(blockType);
			string[] array2 = Scarcity.DefaultTextures(blockType);
			float num = 0f;
			float num2 = 1f;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != block.GetPaint(i))
				{
					num = Mathf.Max(options.paintedBlockOcclusionSeverityBias, num);
					num2 = Mathf.Max(num2, options.paintedBlockOcclusionSeverityMultiplier);
				}
				string texture = block.GetTexture(i);
				if (array2[i] != texture)
				{
					num = Mathf.Max(options.texturedBlockOcclusionSeverityBias, num);
					num2 = Mathf.Max(num2, options.paintedBlockOcclusionSeverityMultiplier);
					TextureInfo textureInfo;
					if (Materials.textureInfos.TryGetValue(texture, out textureInfo))
					{
						switch (textureInfo.mapping)
						{
						case Mapping.OneSideTo1x1:
						case Mapping.TwoSidesTo1x1:
						case Mapping.OneSideWrapTo1x1:
						case Mapping.TwoSidesWrapTo1x1:
							num = Mathf.Max(options.texturedSideBlockOcclusionSeverityBias, num);
							num2 = Mathf.Max(num2, options.texturedSideBlockOcclusionSeverityMultiplier);
							break;
						}
					}
				}
			}
			Vector3 position = block.go.transform.position;
			float num3 = 0.5f * Util.MeanAbs(block.Scale());
			float num4 = 15f + num3;
			foreach (KeyValuePair<Vector3, float> keyValuePair in dictionary)
			{
				Vector3 key = keyValuePair.Key;
				RaycastHit[] array3 = Physics.RaycastAll(position, key, num4, -1);
				float num5 = 0f;
				foreach (RaycastHit raycastHit in array3)
				{
					Collider collider = raycastHit.collider;
					Block block2 = BWSceneManager.FindBlock(collider.gameObject, true);
					if (block2 != null && block2 != block)
					{
						float num6 = 1f - raycastHit.distance / num4;
						float num7 = num6 * keyValuePair.Value;
						List<ModelTutorializeUtils.BlockOcclusion> list;
						if (!blockOtherOcclusions.TryGetValue(block2, out list))
						{
							list = new List<ModelTutorializeUtils.BlockOcclusion>();
							blockOtherOcclusions[block2] = list;
						}
						list.Add(new ModelTutorializeUtils.BlockOcclusion
						{
							block = block,
							severity = num7
						});
						num5 = Mathf.Max(num7, num5);
					}
				}
				num += num5 * num2;
			}
			blockOcclusions[block] = num;
		}
	}

	// Token: 0x060018A8 RID: 6312 RVA: 0x000AD450 File Offset: 0x000AB850
	private static void CalculateBlockSymmetries(HashSet<Block> blocksToPlace, Dictionary<Block, Block> blockSymmetryMap, Dictionary<Block, ModelTutorializeUtils.SymmetricBlocks> blockSymmetries)
	{
		Dictionary<string, ModelTutorializeUtils.SymmetricBlocks> dictionary = ModelTutorializeUtils.FindSymmetricBlocks(new List<Block>(blocksToPlace));
		HashSet<Block> hashSet = new HashSet<Block>(blocksToPlace);
		List<ModelTutorializeUtils.SymmetricBlocks> list = new List<ModelTutorializeUtils.SymmetricBlocks>(dictionary.Values);
		list.Sort((ModelTutorializeUtils.SymmetricBlocks s1, ModelTutorializeUtils.SymmetricBlocks s2) => s2.side1.Count.CompareTo(s1.side1.Count));
		int num = (list.Count <= 0) ? 0 : list[0].side1.Count;
		for (int i = 0; i < list.Count; i++)
		{
			ModelTutorializeUtils.SymmetricBlocks symmetricBlocks = list[i];
			if (symmetricBlocks.side1.Count != 1 || num <= 2)
			{
				for (int j = 0; j < symmetricBlocks.side1.Count; j++)
				{
					Block block = symmetricBlocks.side1[j];
					Block block2 = symmetricBlocks.side2[j];
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

	// Token: 0x020001DF RID: 479
	public class StepByStepTutorializeOptions
	{
		// Token: 0x04001382 RID: 4994
		public bool disableRendererForUnplacedBlocks = true;

		// Token: 0x04001383 RID: 4995
		public float waitTimePerBlock = 0.01f;

		// Token: 0x04001384 RID: 4996
		public int symmetryChainPenaltyThreshold = 3;

		// Token: 0x04001385 RID: 4997
		public float symmetryChainPenalty = 0.2f;

		// Token: 0x04001386 RID: 4998
		public float heightPenaltyFactor = 0.1f;

		// Token: 0x04001387 RID: 4999
		public float maxHeightPenalty = 1f;

		// Token: 0x04001388 RID: 5000
		public float selfOcclusionPenaltyFactor = 0.1f;

		// Token: 0x04001389 RID: 5001
		public float upOcclusionPenaltyFactor = 0.5f;

		// Token: 0x0400138A RID: 5002
		public float sideOcclusionPenaltyFactor = 0.3f;

		// Token: 0x0400138B RID: 5003
		public float paintedBlockOcclusionSeverityBias = 0.1f;

		// Token: 0x0400138C RID: 5004
		public float paintedBlockOcclusionSeverityMultiplier = 2f;

		// Token: 0x0400138D RID: 5005
		public float texturedBlockOcclusionSeverityBias = 0.1f;

		// Token: 0x0400138E RID: 5006
		public float texturedBlockOcclusionSeverityMultiplier = 2f;

		// Token: 0x0400138F RID: 5007
		public float texturedSideBlockOcclusionSeverityBias = 0.3f;

		// Token: 0x04001390 RID: 5008
		public float texturedSideBlockOcclusionSeverityMultiplier = 2.5f;

		// Token: 0x04001391 RID: 5009
		public float connectedToLastBlockReward = 1f;

		// Token: 0x04001392 RID: 5010
		public float isolatedBlockPenalty = 3f;

		// Token: 0x04001393 RID: 5011
		public Func<float, float> distanceToLastBlockRewardFunction = (float dist) => 1f / (1f + 0.25f * dist);
	}

	// Token: 0x020001E0 RID: 480
	private class SymmetricBlocks
	{
		// Token: 0x060018AD RID: 6317 RVA: 0x000AD6E8 File Offset: 0x000ABAE8
		public void Add(Block b1, Block b2)
		{
			this.side1.Add(b1);
			this.side2.Add(b2);
		}

		// Token: 0x060018AE RID: 6318 RVA: 0x000AD702 File Offset: 0x000ABB02
		public int PairCount()
		{
			return this.side1.Count;
		}

		// Token: 0x060018AF RID: 6319 RVA: 0x000AD710 File Offset: 0x000ABB10
		public static string CalculateLookupKey(int coordIndex, float centerCoord)
		{
			return coordIndex.ToString() + Mathf.RoundToInt(centerCoord * 10f).ToString();
		}

		// Token: 0x060018B0 RID: 6320 RVA: 0x000AD749 File Offset: 0x000ABB49
		public override string ToString()
		{
			return string.Format("[SymmetricBlocks pair count: {0}, coordIndex: {1}, centerCoord: {2}]", this.PairCount(), this.coordIndex, this.centerCoord);
		}

		// Token: 0x04001395 RID: 5013
		public List<Block> side1 = new List<Block>();

		// Token: 0x04001396 RID: 5014
		public List<Block> side2 = new List<Block>();

		// Token: 0x04001397 RID: 5015
		public int coordIndex;

		// Token: 0x04001398 RID: 5016
		public float centerCoord;
	}

	// Token: 0x020001E1 RID: 481
	private class BlockOcclusion
	{
		// Token: 0x04001399 RID: 5017
		public Block block;

		// Token: 0x0400139A RID: 5018
		public float severity;
	}
}
