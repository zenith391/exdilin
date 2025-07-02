using System.Collections.Generic;
using Blocks;

public interface IBlockster
{
	Block IBlockster_BottomAttachment();

	List<Block> IBlockster_HeadAttachments();

	Block IBlockster_FrontAttachment();

	Block IBlockster_BackAttachment();

	Block IBlockster_RightHandAttachment();

	Block IBlockster_LeftHandAttachement();

	void IBlockster_FindAttachments();
}
