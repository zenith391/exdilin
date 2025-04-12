using System;
using UnityEngine;

namespace Blocks
{
	// Token: 0x0200005D RID: 93
	public class FootInfo
	{
		// Token: 0x040005E5 RID: 1509
		public GameObject go;

		// Token: 0x040005E6 RID: 1510
		public Rigidbody rb;

		// Token: 0x040005E7 RID: 1511
		public Collider collider;

		// Token: 0x040005E8 RID: 1512
		public Vector3 position;

		// Token: 0x040005E9 RID: 1513
		public Vector3 normal;

		// Token: 0x040005EA RID: 1514
		public Transform bone;

		// Token: 0x040005EB RID: 1515
		public float boneYOffset;

		// Token: 0x040005EC RID: 1516
		public SpringJoint joint;

		// Token: 0x040005ED RID: 1517
		public Mesh ankleMesh;

		// Token: 0x040005EE RID: 1518
		public string oldName;

		// Token: 0x040005EF RID: 1519
		public Vector3 pausedVelocity;

		// Token: 0x040005F0 RID: 1520
		public Vector3 pausedAngularVelocity;

		// Token: 0x040005F1 RID: 1521
		public Vector3 origLocalPosition;

		// Token: 0x040005F2 RID: 1522
		public Quaternion origLocalRotation;

		// Token: 0x040005F3 RID: 1523
		public Vector3[] ankleMeshOrigVertices;
	}
}
