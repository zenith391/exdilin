using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class MenuTextDefinitions : ScriptableObject
{
	public List<MenuTextDef> definitions;

	private static MenuTextDefinitions _menuTextDefinitions;

	private static MenuTextDefinitions menuTextDefinitions
	{
		get
		{
			if (_menuTextDefinitions == null)
			{
				_menuTextDefinitions = Resources.Load<MenuTextDefinitions>("MenuTextDefinitions");
			}
			return _menuTextDefinitions;
		}
	}

	public static string GetTextString(BWMenuTextEnum messageName)
	{
		string result = string.Empty;
		MenuTextDef menuTextDef = menuTextDefinitions.definitions.Find((MenuTextDef d) => d.messageName == messageName);
		if (menuTextDef != null)
		{
			result = Regex.Unescape(menuTextDef.messageString);
		}
		return result;
	}
}
