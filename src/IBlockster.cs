using System;
using System.Collections.Generic;
using Blocks;

// Token: 0x0200019B RID: 411
public interface IBlockster
{
	// Token: 0x060016EC RID: 5868
	Block IBlockster_BottomAttachment();

	// Token: 0x060016ED RID: 5869
	List<Block> IBlockster_HeadAttachments();

	// Token: 0x060016EE RID: 5870
	Block IBlockster_FrontAttachment();

	// Token: 0x060016EF RID: 5871
	Block IBlockster_BackAttachment();

	// Token: 0x060016F0 RID: 5872
	Block IBlockster_RightHandAttachment();

	// Token: 0x060016F1 RID: 5873
	Block IBlockster_LeftHandAttachement();

	// Token: 0x060016F2 RID: 5874
	void IBlockster_FindAttachments();
}
