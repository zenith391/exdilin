using System.Collections.Generic;

public class ModelData
{
	public enum LoadState
	{
		NOT_LOADED,
		LOADING,
		SUCCESS
	}

	public string id;

	public string type;

	public string name;

	public string source;

	public bool sourceLocked;

	public Tile tile;

	public string pathToIconFile;

	public bool hidden;

	public string hash;

	public Dictionary<GAF, int> gafUsage;

	public HashSet<string> uniqueBlockNames;

	public LoadState iconLoadState;

	public bool Setup()
	{
		List<List<List<Tile>>> list = CreateModel();
		if (list == null)
		{
			BWLog.Error("Failed to setup model from source:" + id);
			return false;
		}
		if (ModelCollection.ModelContainsDisallowedTile(list))
		{
			BWLog.Error("Model contains disallowed tile");
			return false;
		}
		hash = ModelUtils.GenerateHashString(list);
		gafUsage = Scarcity.GetNormalizedInventoryUse(list, WorldType.User);
		uniqueBlockNames = Scarcity.GetUniqueBlockNames(gafUsage);
		return true;
	}

	public List<List<List<Tile>>> CreateModel()
	{
		if (string.IsNullOrEmpty(source))
		{
			source = WorldSession.platformDelegate.GetModelSource(type, id);
			if (string.IsNullOrEmpty(source))
			{
				BWLog.Error("No source for model " + name + " id: " + id);
				return null;
			}
			if (sourceLocked)
			{
				source = Blocksworld.LockModelJSON(source);
			}
		}
		if (string.IsNullOrEmpty(source))
		{
			BWLog.Error("No source for model " + name + " id: " + id);
			return null;
		}
		return ModelUtils.ParseModelString(source);
	}
}
