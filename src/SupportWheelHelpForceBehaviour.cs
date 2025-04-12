using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x0200024D RID: 589
public class SupportWheelHelpForceBehaviour : MonoBehaviour
{
	// Token: 0x06001B0C RID: 6924 RVA: 0x000C62B8 File Offset: 0x000C46B8
	private void OnCollisionEnter(Collision collisionInfo)
	{
		Rigidbody attachedRigidbody = collisionInfo.collider.attachedRigidbody;
		this.intersectingRbs.Remove(attachedRigidbody);
		if (this.forwardEvents)
		{
			ForwardEvents.ForwardCollisionEnter(collisionInfo);
		}
	}

	// Token: 0x06001B0D RID: 6925 RVA: 0x000C62EF File Offset: 0x000C46EF
	private void OnCollisionStay(Collision collisionInfo)
	{
		this.ApplyForce(collisionInfo);
		if (this.forwardEvents)
		{
			ForwardEvents.ForwardCollisionStay(collisionInfo, base.gameObject);
		}
	}

	// Token: 0x06001B0E RID: 6926 RVA: 0x000C6310 File Offset: 0x000C4710
	private void ApplyForce(Collision collisionInfo)
	{
		if (this.giver.IsHelpForceActive())
		{
			this.rigids.Clear();
			this.forces.Clear();
			this.points.Clear();
			this.counts.Clear();
			Rigidbody rigidbody = null;
			foreach (ContactPoint contactPoint in collisionInfo.contacts)
			{
				if (rigidbody == null)
				{
					rigidbody = contactPoint.thisCollider.attachedRigidbody;
				}
				Collider otherCollider = contactPoint.otherCollider;
				if (!(otherCollider == null))
				{
					if (!(rigidbody == null))
					{
						Rigidbody attachedRigidbody = otherCollider.attachedRigidbody;
						Vector3 point = contactPoint.point;
						Vector3 helpForceAt = this.giver.GetHelpForceAt(rigidbody, attachedRigidbody, point, collisionInfo.relativeVelocity, !this.intersectingRbs.Contains(attachedRigidbody));
						int num = this.rigids.IndexOf(attachedRigidbody);
						if (num < 0)
						{
							this.rigids.Add(attachedRigidbody);
							this.forces.Add(helpForceAt);
							this.counts.Add(1);
							this.points.Add(point);
						}
						else
						{
							List<Vector3> list;
							int index;
							(list = this.forces)[index = num] = list[index] + helpForceAt;
							List<int> list2;
							int index2;
							(list2 = this.counts)[index2 = num] = list2[index2] + 1;
							int index3;
							(list = this.points)[index3 = num] = list[index3] + contactPoint.point;
						}
					}
				}
			}
			if (rigidbody != null)
			{
				float mass = rigidbody.mass;
				for (int j = 0; j < this.rigids.Count; j++)
				{
					int num2 = this.counts[j];
					Vector3 vector = this.forces[j] / (float)num2;
					Vector3 vector2 = this.points[j] / (float)num2;
					if (this.addToSelf)
					{
						rigidbody.AddForceAtPosition(vector, this.giver.GetForcePoint(vector2), this.thisForceMode);
					}
					Rigidbody rigidbody2 = this.rigids[j];
					if (rigidbody2 != null && rigidbody2 != rigidbody)
					{
						float num3 = 10f * Mathf.Clamp(rigidbody2.mass / mass, 0.2f, 5f);
						Vector3 force = -num3 * vector;
						rigidbody2.AddForceAtPosition(force, vector2, this.otherForceMode);
						this.intersectingRbs.Add(rigidbody2);
					}
				}
			}
		}
	}

	// Token: 0x0400171D RID: 5917
	public IHelpForceGiver giver;

	// Token: 0x0400171E RID: 5918
	public ForceMode thisForceMode = ForceMode.Impulse;

	// Token: 0x0400171F RID: 5919
	public ForceMode otherForceMode;

	// Token: 0x04001720 RID: 5920
	public bool forwardEvents;

	// Token: 0x04001721 RID: 5921
	public bool addToSelf = true;

	// Token: 0x04001722 RID: 5922
	private List<Rigidbody> rigids = new List<Rigidbody>();

	// Token: 0x04001723 RID: 5923
	private List<Vector3> forces = new List<Vector3>();

	// Token: 0x04001724 RID: 5924
	private List<Vector3> points = new List<Vector3>();

	// Token: 0x04001725 RID: 5925
	private List<int> counts = new List<int>();

	// Token: 0x04001726 RID: 5926
	private HashSet<Rigidbody> intersectingRbs = new HashSet<Rigidbody>();
}
