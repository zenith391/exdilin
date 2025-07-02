using UnityEngine;

public interface ITreasureAnimationDriver
{
	Vector3 GetTreasurePositionOffset(TreasureHandler.TreasureState state);

	Quaternion GetTreasureRotation(TreasureHandler.TreasureState state);

	bool TreasureAnimationActivated();
}
