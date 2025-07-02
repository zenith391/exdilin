using System.Collections.Generic;
using SimpleJSON;

public class BlockMeshScarcityInfo
{
	public string defaultTexture;

	public string defaultPaint;

	public HashSet<string> freeTextures = new HashSet<string>();

	public HashSet<string> freePaints = new HashSet<string>();

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
}
