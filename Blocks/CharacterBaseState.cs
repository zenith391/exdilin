using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000292 RID: 658
	public class CharacterBaseState
	{
		// Token: 0x06001EB8 RID: 7864 RVA: 0x000DC388 File Offset: 0x000DA788
		public virtual void Enter(CharacterStateHandler parent, bool interrupt)
		{
			parent.targetController.speed = 1f;
			parent.desiredAnimSpeed = 1f;
			Blocksworld.blocksworldCamera.fpcTilt = 0f;
			parent.firstFrame = true;
		}

		// Token: 0x06001EB9 RID: 7865 RVA: 0x000DC3BB File Offset: 0x000DA7BB
		public virtual void Exit(CharacterStateHandler parent)
		{
			Blocksworld.blocksworldCamera.fpcTilt = 0f;
			parent.desiredAnimSpeed = 1f;
		}

		// Token: 0x06001EBA RID: 7866 RVA: 0x000DC3D7 File Offset: 0x000DA7D7
		public virtual bool Update(CharacterStateHandler parent)
		{
			return true;
		}

		// Token: 0x06001EBB RID: 7867 RVA: 0x000DC3DA File Offset: 0x000DA7DA
		public virtual Vector3 GetOffset(CharacterStateHandler parent)
		{
			return (this.rootOffsets == null || this.rootOffsets.Count <= 0) ? Vector3.zero : this.rootOffsets[0];
		}

		// Token: 0x06001EBC RID: 7868 RVA: 0x000DC40E File Offset: 0x000DA80E
		public virtual void OnScreenDebug(CharacterStateHandler parent)
		{
		}

		// Token: 0x040018C6 RID: 6342
		public CharacterState desiredState;

		// Token: 0x040018C7 RID: 6343
		public List<Vector3> rootOffsets;
	}
}
