using System;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000073 RID: 115
	public class LoopSoundSourceInfo
	{
		// Token: 0x06000998 RID: 2456 RVA: 0x000433D1 File Offset: 0x000417D1
		public LoopSoundSourceInfo(Vector3 pos, Block block)
		{
			this.pos = pos;
			this.block = block;
		}

		// Token: 0x06000999 RID: 2457 RVA: 0x000433F2 File Offset: 0x000417F2
		public void Update(Vector3 pos, float pitch, bool playing)
		{
			this.pos = pos;
			this.pitch = pitch;
			this.playing = playing;
		}

		// Token: 0x0400079B RID: 1947
		public Vector3 pos;

		// Token: 0x0400079C RID: 1948
		public float pitch = 1f;

		// Token: 0x0400079D RID: 1949
		public Block block;

		// Token: 0x0400079E RID: 1950
		public bool playing;
	}
}
