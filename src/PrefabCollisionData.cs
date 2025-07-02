using UnityEngine;

public struct PrefabCollisionData
{
	public readonly int PrefabID;

	public readonly Vector3 Scale;

	public PrefabCollisionData(GameObject prefab, Vector3 scale)
	{
		PrefabID = prefab.GetInstanceID();
		Scale = scale;
	}

	public override int GetHashCode()
	{
		return PrefabID ^ (Scale.GetHashCode() << 16);
	}

	public override bool Equals(object other)
	{
		if (!(other is PrefabCollisionData prefabCollisionData))
		{
			return false;
		}
		if (PrefabID == prefabCollisionData.PrefabID)
		{
			return Scale == prefabCollisionData.Scale;
		}
		return false;
	}
}
