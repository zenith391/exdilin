using System;
using System.Collections.Generic;
using Blocks;

// Token: 0x020001D2 RID: 466
public class ModelFeatures
{
	// Token: 0x06001863 RID: 6243 RVA: 0x000ABA5C File Offset: 0x000A9E5C
	static ModelFeatures()
	{
		ModelFeatures.featureTypes = new List<ModelFeatureType>
		{
			new MultiCounterModelFeatureType
			{
				name = "MultiCounter"
			},
			new BlockWithTagAlongSameLineCountModelFeatureType
			{
				tagName = "Wheel",
				name = "MaxWheelsAlongSameLine"
			},
			new NonConditionalTileWithAnyPredicateExistsFeatureType
			{
				predicates = 
				{
					Block.predicateFreeze
				},
				name = "NonConditionalFreeze"
			}
		};
	}

	// Token: 0x06001865 RID: 6245 RVA: 0x000ABAF8 File Offset: 0x000A9EF8
	public static List<ModelFeatureType> GetModelFeatures(IEnumerable<Block> model)
	{
		ModelFeatures.tempList.Clear();
		foreach (Block block in model)
		{
			ModelFeatures.tempList.Add(block.tiles);
		}
		return ModelFeatures.GetModelFeatures(ModelFeatures.tempList);
	}

	// Token: 0x06001866 RID: 6246 RVA: 0x000ABB6C File Offset: 0x000A9F6C
	public static List<ModelFeatureType> GetModelFeatures(List<List<List<Tile>>> model)
	{
		for (int i = 0; i < ModelFeatures.featureTypes.Count; i++)
		{
			ModelFeatures.featureTypes[i].Reset();
		}
		for (int j = 0; j < model.Count; j++)
		{
			List<List<Tile>> list = model[j];
			for (int k = 0; k < list.Count; k++)
			{
				List<Tile> list2 = list[k];
				bool beforeThen = true;
				for (int l = 0; l < list2.Count; l++)
				{
					Tile tile = list2[l];
					if (tile.gaf.Predicate == Block.predicateThen)
					{
						beforeThen = false;
					}
					for (int m = 0; m < ModelFeatures.featureTypes.Count; m++)
					{
						ModelFeatures.featureTypes[m].Update(model, tile, j, k, l, beforeThen);
					}
				}
			}
		}
		return ModelFeatures.featureTypes;
	}

	// Token: 0x06001867 RID: 6247 RVA: 0x000ABC64 File Offset: 0x000AA064
	public static string CategorizeModel(List<List<List<Tile>>> model)
	{
		List<ModelFeatureType> modelFeatures = ModelFeatures.GetModelFeatures(model);
		return ModelCategorizer.GetModelCategory(modelFeatures, null);
	}

	// Token: 0x04001354 RID: 4948
	public const string MAX_WHEELS_ALONG_SAME_LINE = "MaxWheelsAlongSameLine";

	// Token: 0x04001355 RID: 4949
	public const string NON_CONDITIONAL_FREEZE = "NonConditionalFreeze";

	// Token: 0x04001356 RID: 4950
	private static List<ModelFeatureType> featureTypes = new List<ModelFeatureType>();

	// Token: 0x04001357 RID: 4951
	private static List<List<List<Tile>>> tempList = new List<List<List<Tile>>>();
}
