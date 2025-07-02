using UnityEngine;

public class ForwardEvents : MonoBehaviour
{
	public static void ForwardCollisionEnter(Collision collision)
	{
		bool flag = true;
		ContactPoint[] contacts = collision.contacts;
		for (int i = 0; i < contacts.Length; i++)
		{
			ContactPoint contactPoint = contacts[i];
			CollisionManager.ForwardCollisionEnter(contactPoint.thisCollider.gameObject, contactPoint.otherCollider.gameObject, (!flag) ? null : collision);
			flag = false;
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		ForwardCollisionEnter(collision);
	}

	public static void ForwardCollisionStay(Collision collision, GameObject gameObject)
	{
		CollisionManager.ForwardCollisionStay(gameObject, collision);
		ContactPoint[] contacts = collision.contacts;
		for (int i = 0; i < contacts.Length; i++)
		{
			ContactPoint contactPoint = contacts[i];
			if (!(contactPoint.thisCollider == null) && !(contactPoint.otherCollider == null))
			{
				CollisionManager.ForwardCollisionEnter(contactPoint.thisCollider.gameObject, contactPoint.otherCollider.gameObject, null);
			}
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		ForwardCollisionStay(collision, base.gameObject);
	}

	private void OnCollisionExit()
	{
	}

	private void OnTriggerEnter(Collider collider)
	{
		CollisionManager.ForwardTriggerEnter(base.gameObject, collider);
	}

	private void OnTriggerExit(Collider collider)
	{
		CollisionManager.ForwardTriggerExit(base.gameObject, collider);
	}
}
