using System.Collections.Generic;
using Blocks;

public class ModelFeatures
{
	public const string MAX_WHEELS_ALONG_SAME_LINE = "MaxWheelsAlongSameLine";

	public const string NON_CONDITIONAL_FREEZE = "NonConditionalFreeze";

	private static List<ModelFeatureType> featureTypes;

	private static List<List<List<Tile>>> tempList;

	static ModelFeatures()
	{
		featureTypes = new List<ModelFeatureType>();
		tempList = new List<List<List<Tile>>>();
		featureTypes = new List<ModelFeatureType>
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
				predicates = { Block.predicateFreeze },
				name = "NonConditionalFreeze"
			}
		};
	}

	public static List<ModelFeatureType> GetModelFeatures(IEnumerable<Block> model)
	{
		tempList.Clear();
		foreach (Block item in model)
		{
			tempList.Add(item.tiles);
		}
		return GetModelFeatures(tempList);
	}

	public static List<ModelFeatureType> GetModelFeatures(List<List<List<Tile>>> model)
	{
		for (int i = 0; i < featureTypes.Count; i++)
		{
			featureTypes[i].Reset();
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
					for (int m = 0; m < featureTypes.Count; m++)
					{
						featureTypes[m].Update(model, tile, j, k, l, beforeThen);
					}
				}
			}
		}
		return featureTypes;
	}

	public static string CategorizeModel(List<List<List<Tile>>> model)
	{
		List<ModelFeatureType> modelFeatures = GetModelFeatures(model);
		return ModelCategorizer.GetModelCategory(modelFeatures);
	}
}
