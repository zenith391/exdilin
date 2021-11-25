using System;
using UnityEngine;

// Token: 0x020001EE RID: 494
[Serializable]
public class GAFInfo
{
	// Token: 0x17000074 RID: 116
	// (get) Token: 0x060018D7 RID: 6359 RVA: 0x000AF77C File Offset: 0x000ADB7C
	public string Predicate
	{
		get
		{
			switch (this.predicate)
			{
			case GAFInfo.GAFPredicate.Block_Fixed:
				return "Block.Fixed";
			case GAFInfo.GAFPredicate.Meta_WaitTime:
				return "Meta.WaitTime";
			case GAFInfo.GAFPredicate.Block_PaintTo:
				return "Block.PaintTo";
			case GAFInfo.GAFPredicate.Block_TextureTo:
				return "Block.TextureTo";
			default:
				return "Block.TextureTo";
			}
		}
	}

	// Token: 0x17000075 RID: 117
	// (get) Token: 0x060018D8 RID: 6360 RVA: 0x000AF7C8 File Offset: 0x000ADBC8
	public object[] Args
	{
		get
		{
			if (this.args != null)
			{
				object[] array = new object[this.args.Length];
				for (int i = 0; i < this.args.Length; i++)
				{
					GAFArgument gafargument = this.args[i];
					switch (gafargument.type)
					{
					case GAFArgumentType.STRING:
						array[i] = ((gafargument.content != null) ? gafargument.content : string.Empty);
						break;
					case GAFArgumentType.FLOAT:
					{
						float num;
						if (float.TryParse(gafargument.content, out num))
						{
							array[i] = num;
						}
						else
						{
							BWLog.Error("Could not parse '" + gafargument.content + "' as a float");
						}
						break;
					}
					case GAFArgumentType.INTEGER:
					{
						int num2;
						if (int.TryParse(gafargument.content, out num2))
						{
							array[i] = num2;
						}
						else
						{
							BWLog.Error("Could not parse '" + gafargument.content + "' as an int");
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

	// Token: 0x0400141F RID: 5151
	public GAFInfo.GAFPredicate predicate = GAFInfo.GAFPredicate.Meta_WaitTime;

	// Token: 0x04001420 RID: 5152
	public GAFArgument[] args;

	// Token: 0x020001EF RID: 495
	public enum GAFPredicate
	{
		// Token: 0x04001422 RID: 5154
		Block_Fixed,
		// Token: 0x04001423 RID: 5155
		Meta_WaitTime,
		// Token: 0x04001424 RID: 5156
		Block_PaintTo,
		// Token: 0x04001425 RID: 5157
		Block_TextureTo
	}
}
