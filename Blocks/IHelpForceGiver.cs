using System;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020000D8 RID: 216
	public interface IHelpForceGiver
	{
		// Token: 0x06001038 RID: 4152
		Vector3 GetHelpForceAt(Rigidbody thisRb, Rigidbody otherRb, Vector3 pos, Vector3 relativeVelocity, bool fresh);

		// Token: 0x06001039 RID: 4153
		Vector3 GetForcePoint(Vector3 contactPos);

		// Token: 0x0600103A RID: 4154
		bool IsHelpForceActive();
	}
}
