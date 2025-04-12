using System;

// Token: 0x020000FF RID: 255
public class ChunkFollowInfo
{
	// Token: 0x0600128C RID: 4748 RVA: 0x00081A93 File Offset: 0x0007FE93
	public ChunkFollowInfo(Chunk chunk, bool auto)
	{
		this.chunk = chunk;
		this.auto = auto;
	}

	// Token: 0x04000EF9 RID: 3833
	public Chunk chunk;

	// Token: 0x04000EFA RID: 3834
	public bool auto;
}
