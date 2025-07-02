using UnityEngine;

namespace Blocks;

public interface IHelpForceGiver
{
	Vector3 GetHelpForceAt(Rigidbody thisRb, Rigidbody otherRb, Vector3 pos, Vector3 relativeVelocity, bool fresh);

	Vector3 GetForcePoint(Vector3 contactPos);

	bool IsHelpForceActive();
}
