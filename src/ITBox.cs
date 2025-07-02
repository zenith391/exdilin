using System.Collections.Generic;
using Blocks;
using UnityEngine;

public interface ITBox
{
	Vector3 GetPosition();

	Quaternion GetRotation();

	Vector3 GetScale();

	Vector3 CanScale();

	void EnableCollider(bool value);

	bool IsColliderEnabled();

	bool IsColliderHit(Collider other);

	bool IsColliding(float terrainOffset = 0f, HashSet<Block> exclude = null);

	bool ContainsBlock(Block block);

	void IgnoreRaycasts(bool value);

	bool TBoxMoveTo(Vector3 pos, bool forced = false);

	bool MoveTo(Vector3 pos);

	bool TBoxRotateTo(Quaternion rot);

	bool RotateTo(Quaternion rot);

	void TBoxSnap();
}
