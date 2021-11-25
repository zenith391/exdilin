using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x020002C6 RID: 710
public class TextureNormalRestriction
{
	// Token: 0x06002071 RID: 8305 RVA: 0x000EE098 File Offset: 0x000EC498
	public static bool CanTextureBlockWithNormal(Block block, string texture, string blockType, Vector3 normal, int meshIndex, out Vector3 normalRewrite)
	{
		normalRewrite = normal;
		TextureInfo textureInfo;
		if (Materials.textureInfos.TryGetValue(texture, out textureInfo))
		{
			switch (textureInfo.mapping)
			{
			case Mapping.OneSideTo1x1:
			case Mapping.TwoSidesTo1x1:
			case Mapping.OneSideWrapTo1x1:
			case Mapping.TwoSidesWrapTo1x1:
			{
				Side side = Materials.FindSide(normal);
				string text = blockType + meshIndex + side.ToString();
				SideRule sideRule = null;
				if (!TextureNormalRestriction.sideRules.ContainsKey(text))
				{
					TextureNormalRestriction.sideRules[text] = null;
					BlockMeshMetaData[] blockMeshMetaDatas = block.GetBlockMeshMetaDatas();
					int num = 0;
					foreach (BlockMeshMetaData blockMeshMetaData in blockMeshMetaDatas)
					{
						if (meshIndex == num)
						{
							foreach (TextureSideRule textureSideRule in blockMeshMetaData.textureSideRules)
							{
								string text2 = blockType + meshIndex + TextureNormalRestriction.ParseSideString(textureSideRule.side).ToString();
								SideRule sideRule2 = new SideRule();
								sideRule2.rewrite = !string.IsNullOrEmpty(textureSideRule.rewriteSide);
								if (sideRule2.rewrite)
								{
									sideRule2.rewriteSide = TextureNormalRestriction.ParseSideString(textureSideRule.rewriteSide);
								}
								TextureNormalRestriction.sideRules[text2] = sideRule2;
								if (text2 == text)
								{
									sideRule = sideRule2;
								}
							}
							break;
						}
						num++;
					}
				}
				else
				{
					sideRule = TextureNormalRestriction.sideRules[text];
				}
				if (sideRule != null)
				{
					if (!sideRule.rewrite)
					{
						return false;
					}
					normalRewrite = Materials.SideToNormal(sideRule.rewriteSide);
				}
				break;
			}
			}
		}
		return true;
	}

	// Token: 0x06002072 RID: 8306 RVA: 0x000EE260 File Offset: 0x000EC660
	private static Side ParseSideString(string s)
	{
		Side result = Side.Front;
		try
		{
			result = (Side)Enum.Parse(typeof(Side), s);
		}
		catch (Exception ex)
		{
			BWLog.Info("Could not parse side string '" + s + "' Exception message: " + ex.Message);
		}
		return result;
	}

	// Token: 0x04001BAF RID: 7087
	private static Dictionary<string, SideRule> sideRules = new Dictionary<string, SideRule>();
}
