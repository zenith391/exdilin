using System;
using System.Collections.Generic;
using SimpleJSON;

// Token: 0x0200027B RID: 635
public class BlockMeshScarcityInfo
{
	// Token: 0x06001DA5 RID: 7589 RVA: 0x000D3DF0 File Offset: 0x000D21F0
	public static BlockMeshScarcityInfo Read(JObject jObj)
	{
		BlockMeshScarcityInfo blockMeshScarcityInfo = new BlockMeshScarcityInfo();
		List<string> list = BlockScarcityInfo.ReadStringList(jObj, "free-textures");
		blockMeshScarcityInfo.defaultTexture = ((list.Count <= 0) ? "Plain" : list[0]);
		blockMeshScarcityInfo.freeTextures = new HashSet<string>(list);
		List<string> list2 = BlockScarcityInfo.ReadStringList(jObj, "free-paints");
		blockMeshScarcityInfo.defaultPaint = ((list2.Count <= 0) ? "Yellow" : list2[0]);
		blockMeshScarcityInfo.freePaints = new HashSet<string>(list2);
		return blockMeshScarcityInfo;
	}

	// Token: 0x04001824 RID: 6180
	public string defaultTexture;

	// Token: 0x04001825 RID: 6181
	public string defaultPaint;

	// Token: 0x04001826 RID: 6182
	public HashSet<string> freeTextures = new HashSet<string>();

	// Token: 0x04001827 RID: 6183
	public HashSet<string> freePaints = new HashSet<string>();
}
