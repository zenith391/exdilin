using System;

namespace Exdilin
{
    public class BlockEntry
    {
        public string id;
        public string modelName;
        public BlockMetaData metaData;
        public Type blockType;
        public bool hasDefaultTiles;
		/// <summary>
		/// The mod that created the block entry
		/// </summary>
		public Mod originator;
    }
}
