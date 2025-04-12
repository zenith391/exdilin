using System;
using System.Collections.Generic;

// Token: 0x020001BD RID: 445
public class ModelData
{
	// Token: 0x0600180D RID: 6157 RVA: 0x000AA828 File Offset: 0x000A8C28
	public bool Setup()
	{
		List<List<List<Tile>>> list = this.CreateModel();
		if (list == null)
		{
			BWLog.Error("Failed to setup model from source:" + this.id);
			return false;
		}
		if (ModelCollection.ModelContainsDisallowedTile(list))
		{
			BWLog.Error("Model contains disallowed tile");
			return false;
		}
		this.hash = ModelUtils.GenerateHashString(list);
		this.gafUsage = Scarcity.GetNormalizedInventoryUse(list, WorldType.User, false);
		this.uniqueBlockNames = Scarcity.GetUniqueBlockNames(this.gafUsage);
		return true;
	}

	// Token: 0x0600180E RID: 6158 RVA: 0x000AA89C File Offset: 0x000A8C9C
	public List<List<List<Tile>>> CreateModel()
	{
		if (string.IsNullOrEmpty(this.source))
		{
			this.source = WorldSession.platformDelegate.GetModelSource(this.type, this.id);
			if (string.IsNullOrEmpty(this.source))
			{
				BWLog.Error("No source for model " + this.name + " id: " + this.id);
				return null;
			}
			if (this.sourceLocked)
			{
				this.source = Blocksworld.LockModelJSON(this.source);
			}
		}
		if (string.IsNullOrEmpty(this.source))
		{
			BWLog.Error("No source for model " + this.name + " id: " + this.id);
			return null;
		}
		return ModelUtils.ParseModelString(this.source);
	}

	// Token: 0x040012F8 RID: 4856
	public string id;

	// Token: 0x040012F9 RID: 4857
	public string type;

	// Token: 0x040012FA RID: 4858
	public string name;

	// Token: 0x040012FB RID: 4859
	public string source;

	// Token: 0x040012FC RID: 4860
	public bool sourceLocked;

	// Token: 0x040012FD RID: 4861
	public Tile tile;

	// Token: 0x040012FE RID: 4862
	public string pathToIconFile;

	// Token: 0x040012FF RID: 4863
	public bool hidden;

	// Token: 0x04001300 RID: 4864
	public string hash;

	// Token: 0x04001301 RID: 4865
	public Dictionary<GAF, int> gafUsage;

	// Token: 0x04001302 RID: 4866
	public HashSet<string> uniqueBlockNames;

	// Token: 0x04001303 RID: 4867
	public ModelData.LoadState iconLoadState;

	// Token: 0x020001BE RID: 446
	public enum LoadState
	{
		// Token: 0x04001305 RID: 4869
		NOT_LOADED,
		// Token: 0x04001306 RID: 4870
		LOADING,
		// Token: 0x04001307 RID: 4871
		SUCCESS
	}
}
