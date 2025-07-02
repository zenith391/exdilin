using System;
using System.Collections.Generic;
using UnityEngine;

public class GafToBlockItem
{
	private static List<GafPatternMatch> _gafPatterns;

	public static void Init()
	{
		_gafPatterns = new List<GafPatternMatch>();
	}

	public static void CreatePatternMatch(BlockItem blockItem, string[] argumentPatternStrings)
	{
		Predicate predicate = PredicateRegistry.ByName(blockItem.GafPredicateName);
		int num = Mathf.Min(predicate.ArgTypes.Length, argumentPatternStrings.Length);
		ArgumentPatternMatch[] array = new ArgumentPatternMatch[num];
		for (int i = 0; i < num; i++)
		{
			Type type = predicate.ArgTypes[i];
			string patternStr = ((i >= argumentPatternStrings.Length) ? "*" : argumentPatternStrings[i]);
			if (blockItem.Title == "GaugeUI Increment Up")
			{
				patternStr = "PositiveValue";
			}
			else if (blockItem.Title == "GaugeUI Increment Down")
			{
				patternStr = "NegativeValue";
			}
			else if (blockItem.Title.StartsWith("GaugeUI", StringComparison.CurrentCulture))
			{
				patternStr = "*";
			}
			if (type == typeof(int))
			{
				array[i] = new IntArgumentPatternMatch(patternStr);
				continue;
			}
			if (type == typeof(bool))
			{
				array[i] = new BoolArgumentPatternMatch(patternStr);
				continue;
			}
			if (type == typeof(float))
			{
				array[i] = new FloatArgumentPatternMatch(patternStr);
				continue;
			}
			if (type == typeof(string))
			{
				array[i] = new StringArgumentPattenMatch(patternStr);
				continue;
			}
			if (type == typeof(Vector3))
			{
				array[i] = new VectorArgumentPatternMatch(patternStr);
				continue;
			}
			BWLog.Error("Unknown argument type " + type);
			array[i] = new ArgumentPatternMatch();
		}
		_gafPatterns.Add(new GafPatternMatch(blockItem.Id, predicate, array));
	}

	public static int Find(GAF gaf)
	{
		if (_gafPatterns == null)
		{
			BWLog.Error("GAF pattern matching not set up");
			return 0;
		}
		foreach (GafPatternMatch gafPattern in _gafPatterns)
		{
			if (gafPattern.Matches(gaf))
			{
				return gafPattern.BlockItemId;
			}
		}
		return 0;
	}
}
