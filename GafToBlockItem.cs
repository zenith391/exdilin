using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000166 RID: 358
public class GafToBlockItem
{
	// Token: 0x06001575 RID: 5493 RVA: 0x00096262 File Offset: 0x00094662
	public static void Init()
	{
		GafToBlockItem._gafPatterns = new List<GafPatternMatch>();
	}

	// Token: 0x06001576 RID: 5494 RVA: 0x00096270 File Offset: 0x00094670
	public static void CreatePatternMatch(BlockItem blockItem, string[] argumentPatternStrings)
	{
		Predicate predicate = PredicateRegistry.ByName(blockItem.GafPredicateName, true);
		int num = Mathf.Min(predicate.ArgTypes.Length, argumentPatternStrings.Length);
		ArgumentPatternMatch[] array = new ArgumentPatternMatch[num];
		for (int i = 0; i < num; i++)
		{
			Type type = predicate.ArgTypes[i];
			string patternStr = (i >= argumentPatternStrings.Length) ? "*" : argumentPatternStrings[i];
            if (blockItem.Title == "GaugeUI Increment Up")
            {
                patternStr = "PositiveValue";
            } else if (blockItem.Title == "GaugeUI Increment Down")
            {
                patternStr = "NegativeValue";
            } else if (blockItem.Title.StartsWith("GaugeUI", StringComparison.CurrentCulture))
            {
                patternStr = "*";
            }
            if (type == typeof(int))
			{
				array[i] = new IntArgumentPatternMatch(patternStr);
			}
			else if (type == typeof(bool))
			{
				array[i] = new BoolArgumentPatternMatch(patternStr);
			}
			else if (type == typeof(float))
			{
				array[i] = new FloatArgumentPatternMatch(patternStr);
			}
			else if (type == typeof(string))
			{
				array[i] = new StringArgumentPattenMatch(patternStr);
			}
			else if (type == typeof(Vector3))
			{
				array[i] = new VectorArgumentPatternMatch(patternStr);
			}
			else
			{
				BWLog.Error("Unknown argument type " + type);
				array[i] = new ArgumentPatternMatch();
			}

		}
		GafToBlockItem._gafPatterns.Add(new GafPatternMatch(blockItem.Id, predicate, array));
	}

	// Token: 0x06001577 RID: 5495 RVA: 0x000963A8 File Offset: 0x000947A8
	public static int Find(GAF gaf)
	{
		if (GafToBlockItem._gafPatterns == null)
		{
			BWLog.Error("GAF pattern matching not set up");
			return 0;
		}
		foreach (GafPatternMatch gafPatternMatch in GafToBlockItem._gafPatterns)
		{
			if (gafPatternMatch.Matches(gaf))
			{
				return gafPatternMatch.BlockItemId;
			}
		}
		return 0;
	}

	// Token: 0x040010B4 RID: 4276
	private static List<GafPatternMatch> _gafPatterns;
}
