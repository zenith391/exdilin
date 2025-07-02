using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class SupportWheelHelpForceBehaviour : MonoBehaviour
{
	public IHelpForceGiver giver;

	public ForceMode thisForceMode = ForceMode.Impulse;

	public ForceMode otherForceMode;

	public bool forwardEvents;

	public bool addToSelf = true;

	private List<Rigidbody> rigids = new List<Rigidbody>();

	private List<Vector3> forces = new List<Vector3>();

	private List<Vector3> points = new List<Vector3>();

	private List<int> counts = new List<int>();

	private HashSet<Rigidbody> intersectingRbs = new HashSet<Rigidbody>();

	private void OnCollisionEnter(Collision collisionInfo)
	{
		Rigidbody attachedRigidbody = collisionInfo.collider.attachedRigidbody;
		intersectingRbs.Remove(attachedRigidbody);
		if (forwardEvents)
		{
			ForwardEvents.ForwardCollisionEnter(collisionInfo);
		}
	}

	private void OnCollisionStay(Collision collisionInfo)
	{
		ApplyForce(collisionInfo);
		if (forwardEvents)
		{
			ForwardEvents.ForwardCollisionStay(collisionInfo, base.gameObject);
		}
	}

	private void ApplyForce(Collision collisionInfo)
	{
		if (!giver.IsHelpForceActive())
		{
			return;
		}
		rigids.Clear();
		forces.Clear();
		points.Clear();
		counts.Clear();
		Rigidbody rigidbody = null;
		ContactPoint[] contacts = collisionInfo.contacts;
		for (int i = 0; i < contacts.Length; i++)
		{
			ContactPoint contactPoint = contacts[i];
			if (rigidbody == null)
			{
				rigidbody = contactPoint.thisCollider.attachedRigidbody;
			}
			Collider otherCollider = contactPoint.otherCollider;
			if (!(otherCollider == null) && !(rigidbody == null))
			{
				Rigidbody attachedRigidbody = otherCollider.attachedRigidbody;
				Vector3 point = contactPoint.point;
				Vector3 helpForceAt = giver.GetHelpForceAt(rigidbody, attachedRigidbody, point, collisionInfo.relativeVelocity, !intersectingRbs.Contains(attachedRigidbody));
				int num = rigids.IndexOf(attachedRigidbody);
				if (num < 0)
				{
					rigids.Add(attachedRigidbody);
					forces.Add(helpForceAt);
					counts.Add(1);
					points.Add(point);
				}
				else
				{
					forces[num] += helpForceAt;
					counts[num]++;
					points[num] += contactPoint.point;
				}
			}
		}
		if (!(rigidbody != null))
		{
			return;
		}
		float mass = rigidbody.mass;
		for (int j = 0; j < rigids.Count; j++)
		{
			int num2 = counts[j];
			Vector3 vector = forces[j] / num2;
			Vector3 vector2 = points[j] / num2;
			if (addToSelf)
			{
				rigidbody.AddForceAtPosition(vector, giver.GetForcePoint(vector2), thisForceMode);
			}
			Rigidbody rigidbody2 = rigids[j];
			if (rigidbody2 != null && rigidbody2 != rigidbody)
			{
				float num3 = 10f * Mathf.Clamp(rigidbody2.mass / mass, 0.2f, 5f);
				Vector3 force = (0f - num3) * vector;
				rigidbody2.AddForceAtPosition(force, vector2, otherForceMode);
				intersectingRbs.Add(rigidbody2);
			}
		}
	}
}
