using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x0200019F RID: 415
public interface ITBox
{
	// Token: 0x0600174A RID: 5962
	Vector3 GetPosition();

	// Token: 0x0600174B RID: 5963
	Quaternion GetRotation();

	// Token: 0x0600174C RID: 5964
	Vector3 GetScale();

	// Token: 0x0600174D RID: 5965
	Vector3 CanScale();

	// Token: 0x0600174E RID: 5966
	void EnableCollider(bool value);

	// Token: 0x0600174F RID: 5967
	bool IsColliderEnabled();

	// Token: 0x06001750 RID: 5968
	bool IsColliderHit(Collider other);

	// Token: 0x06001751 RID: 5969
	bool IsColliding(float terrainOffset = 0f, HashSet<Block> exclude = null);

	// Token: 0x06001752 RID: 5970
	bool ContainsBlock(Block block);

	// Token: 0x06001753 RID: 5971
	void IgnoreRaycasts(bool value);

	// Token: 0x06001754 RID: 5972
	bool TBoxMoveTo(Vector3 pos, bool forced = false);

	// Token: 0x06001755 RID: 5973
	bool MoveTo(Vector3 pos);

	// Token: 0x06001756 RID: 5974
	bool TBoxRotateTo(Quaternion rot);

	// Token: 0x06001757 RID: 5975
	bool RotateTo(Quaternion rot);

	// Token: 0x06001758 RID: 5976
	void TBoxSnap();
}
