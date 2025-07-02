using UnityEngine;

namespace Blocks;

public class BoxColliderData
{
	public Vector3 size;

	public Vector3 center;

	public BoxColliderData(BoxCollider c)
	{
		size = c.size;
		center = c.center;
	}
}
