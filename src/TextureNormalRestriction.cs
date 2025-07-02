using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class TextureNormalRestriction
{
	private static Dictionary<string, SideRule> sideRules = new Dictionary<string, SideRule>();

	public static bool CanTextureBlockWithNormal(Block block, string texture, string blockType, Vector3 normal, int meshIndex, out Vector3 normalRewrite)
	{
		normalRewrite = normal;
		if (Materials.textureInfos.TryGetValue(texture, out var value))
		{
			Mapping mapping = value.mapping;
			if ((uint)(mapping - 2) <= 1u || (uint)(mapping - 6) <= 1u)
			{
				Side side = Materials.FindSide(normal);
				string text = blockType + meshIndex + side;
				SideRule sideRule = null;
				if (!sideRules.ContainsKey(text))
				{
					sideRules[text] = null;
					BlockMeshMetaData[] blockMeshMetaDatas = block.GetBlockMeshMetaDatas();
					int num = 0;
					BlockMeshMetaData[] array = blockMeshMetaDatas;
					foreach (BlockMeshMetaData blockMeshMetaData in array)
					{
						if (meshIndex == num)
						{
							TextureSideRule[] textureSideRules = blockMeshMetaData.textureSideRules;
							foreach (TextureSideRule textureSideRule in textureSideRules)
							{
								string text2 = blockType + meshIndex + ParseSideString(textureSideRule.side);
								SideRule sideRule2 = new SideRule();
								sideRule2.rewrite = !string.IsNullOrEmpty(textureSideRule.rewriteSide);
								if (sideRule2.rewrite)
								{
									sideRule2.rewriteSide = ParseSideString(textureSideRule.rewriteSide);
								}
								sideRules[text2] = sideRule2;
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
					sideRule = sideRules[text];
				}
				if (sideRule != null)
				{
					if (!sideRule.rewrite)
					{
						return false;
					}
					normalRewrite = Materials.SideToNormal(sideRule.rewriteSide);
				}
			}
		}
		return true;
	}

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
}
