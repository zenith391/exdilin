using System;
using UnityEngine;

// Token: 0x020002D8 RID: 728
public interface ITreasureAnimationDriver
{
	// Token: 0x06002141 RID: 8513
	Vector3 GetTreasurePositionOffset(TreasureHandler.TreasureState state);

	// Token: 0x06002142 RID: 8514
	Quaternion GetTreasureRotation(TreasureHandler.TreasureState state);

	// Token: 0x06002143 RID: 8515
	bool TreasureAnimationActivated();
}
