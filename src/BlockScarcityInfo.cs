using System.Collections.Generic;
using SimpleJSON;

public class BlockScarcityInfo
{
	public string blockName;

	public List<BlockMeshScarcityInfo> meshInfos;

	public HashSet<string> freeSfxs;

	public HashSet<string> shapeCategories;

	public static List<string> ReadStringList(JObject jObj, string key)
	{
		List<string> list = new List<string>();
		if (jObj.ContainsKey(key))
		{
			List<JObject> arrayValue = jObj[key].ArrayValue;
			foreach (JObject item in arrayValue)
			{
				list.Add(item.StringValue);
			}
		}
		return list;
	}

	public static BlockScarcityInfo Read(JObject jObj)
	{
		BlockScarcityInfo blockScarcityInfo = new BlockScarcityInfo();
		blockScarcityInfo.blockName = jObj["name"].StringValue;
		blockScarcityInfo.meshInfos = new List<BlockMeshScarcityInfo>();
		List<JObject> arrayValue = jObj["meshes"].ArrayValue;
		foreach (JObject item2 in arrayValue)
		{
			BlockMeshScarcityInfo item = BlockMeshScarcityInfo.Read(item2);
			blockScarcityInfo.meshInfos.Add(item);
		}
		blockScarcityInfo.freeSfxs = new HashSet<string>(ReadStringList(jObj, "free-sfxs"));
		blockScarcityInfo.shapeCategories = new HashSet<string>(ReadStringList(jObj, "shape-categories"));
		return blockScarcityInfo;
	}
}
