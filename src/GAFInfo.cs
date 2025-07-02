using System;
using UnityEngine;

[Serializable]
public class GAFInfo
{
	public enum GAFPredicate
	{
		Block_Fixed,
		Meta_WaitTime,
		Block_PaintTo,
		Block_TextureTo
	}

	public GAFPredicate predicate = GAFPredicate.Meta_WaitTime;

	public GAFArgument[] args;

	public string Predicate => predicate switch
	{
		GAFPredicate.Block_Fixed => "Block.Fixed", 
		GAFPredicate.Meta_WaitTime => "Meta.WaitTime", 
		GAFPredicate.Block_PaintTo => "Block.PaintTo", 
		GAFPredicate.Block_TextureTo => "Block.TextureTo", 
		_ => "Block.TextureTo", 
	};

	public object[] Args
	{
		get
		{
			if (args != null)
			{
				object[] array = new object[args.Length];
				for (int i = 0; i < args.Length; i++)
				{
					GAFArgument gAFArgument = args[i];
					switch (gAFArgument.type)
					{
					case GAFArgumentType.STRING:
						array[i] = ((gAFArgument.content != null) ? gAFArgument.content : string.Empty);
						break;
					case GAFArgumentType.FLOAT:
					{
						if (float.TryParse(gAFArgument.content, out var result2))
						{
							array[i] = result2;
						}
						else
						{
							BWLog.Error("Could not parse '" + gAFArgument.content + "' as a float");
						}
						break;
					}
					case GAFArgumentType.INTEGER:
					{
						if (int.TryParse(gAFArgument.content, out var result))
						{
							array[i] = result;
						}
						else
						{
							BWLog.Error("Could not parse '" + gAFArgument.content + "' as an int");
						}
						break;
					}
					case GAFArgumentType.VECTOR3:
						BWLog.Warning("Vector3 parsing in meta data not implemented yet. Setting value to (0, 0, 0)");
						array[i] = default(Vector3);
						break;
					}
				}
				return array;
			}
			return new object[0];
		}
	}
}
