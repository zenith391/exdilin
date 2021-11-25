using System;
using System.Collections.Generic;
using SimpleJSON;

// Token: 0x0200027A RID: 634
public class BlockScarcityInfo
{
	// Token: 0x06001DA2 RID: 7586 RVA: 0x000D3C88 File Offset: 0x000D2088
	public static List<string> ReadStringList(JObject jObj, string key)
	{
		List<string> list = new List<string>();
		if (jObj.ContainsKey(key))
		{
			List<JObject> arrayValue = jObj[key].ArrayValue;
			foreach (JObject jobject in arrayValue)
			{
				list.Add(jobject.StringValue);
			}
		}
		return list;
	}

	// Token: 0x06001DA3 RID: 7587 RVA: 0x000D3D04 File Offset: 0x000D2104
	public static BlockScarcityInfo Read(JObject jObj)
	{
		BlockScarcityInfo blockScarcityInfo = new BlockScarcityInfo();
		blockScarcityInfo.blockName = jObj["name"].StringValue;
		blockScarcityInfo.meshInfos = new List<BlockMeshScarcityInfo>();
		List<JObject> arrayValue = jObj["meshes"].ArrayValue;
		foreach (JObject jObj2 in arrayValue)
		{
			BlockMeshScarcityInfo item = BlockMeshScarcityInfo.Read(jObj2);
			blockScarcityInfo.meshInfos.Add(item);
		}
		blockScarcityInfo.freeSfxs = new HashSet<string>(BlockScarcityInfo.ReadStringList(jObj, "free-sfxs"));
		blockScarcityInfo.shapeCategories = new HashSet<string>(BlockScarcityInfo.ReadStringList(jObj, "shape-categories"));
		return blockScarcityInfo;
	}

	// Token: 0x04001820 RID: 6176
	public string blockName;

	// Token: 0x04001821 RID: 6177
	public List<BlockMeshScarcityInfo> meshInfos;

	// Token: 0x04001822 RID: 6178
	public HashSet<string> freeSfxs;

	// Token: 0x04001823 RID: 6179
	public HashSet<string> shapeCategories;
}
