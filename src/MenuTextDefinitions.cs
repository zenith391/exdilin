using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

// Token: 0x02000436 RID: 1078
public class MenuTextDefinitions : ScriptableObject
{
	// Token: 0x17000248 RID: 584
	// (get) Token: 0x06002E4E RID: 11854 RVA: 0x0014A1E3 File Offset: 0x001485E3
	private static MenuTextDefinitions menuTextDefinitions
	{
		get
		{
			if (MenuTextDefinitions._menuTextDefinitions == null)
			{
				MenuTextDefinitions._menuTextDefinitions = Resources.Load<MenuTextDefinitions>("MenuTextDefinitions");
			}
			return MenuTextDefinitions._menuTextDefinitions;
		}
	}

	// Token: 0x06002E4F RID: 11855 RVA: 0x0014A20C File Offset: 0x0014860C
	public static string GetTextString(BWMenuTextEnum messageName)
	{
		string result = string.Empty;
		MenuTextDef menuTextDef = MenuTextDefinitions.menuTextDefinitions.definitions.Find((MenuTextDef d) => d.messageName == messageName);
		if (menuTextDef != null)
		{
			result = Regex.Unescape(menuTextDef.messageString);
		}
		return result;
	}

	// Token: 0x040026DE RID: 9950
	public List<MenuTextDef> definitions;

	// Token: 0x040026DF RID: 9951
	private static MenuTextDefinitions _menuTextDefinitions;
}
