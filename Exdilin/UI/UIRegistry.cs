using System;
using System.Collections.Generic;

namespace Exdilin.UI
{
    public static class UIRegistry
    {
        private static Dictionary<string, MenuBarButton> menuBarButtons = new Dictionary<string, MenuBarButton>();

        public static Dictionary<string, MenuBarButton> GetMenuBarButtons()
        {
            return menuBarButtons;
        }

        public static void RegisterMenuBarButton(string id, MenuBarButton button)
        {
            menuBarButtons[Mod.ExecutionMod.Id + "/" + id] = button;
        }

    }
}
