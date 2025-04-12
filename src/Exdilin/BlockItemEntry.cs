using System;

namespace Exdilin
{
    public class BlockItemEntry
    {
        public BlockItem item;
		public int buildPaneSubTab = 0;
        public string buildPaneTab = "Blocks";
        public string[] argumentPatterns;
		public bool infinite = true;
		public int count = 1;
    }
}
