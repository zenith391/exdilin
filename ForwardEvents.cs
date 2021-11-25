using System;
using UnityEngine;

// Token: 0x02000208 RID: 520
public class ForwardEvents : MonoBehaviour
{
	// Token: 0x06001A2F RID: 6703 RVA: 0x000C16DC File Offset: 0x000BFADC
	public static void ForwardCollisionEnter(Collision collision)
	{
		bool flag = true;
		foreach (ContactPoint contactPoint in collision.contacts)
		{
			CollisionManager.ForwardCollisionEnter(contactPoint.thisCollider.gameObject, contactPoint.otherCollider.gameObject, (!flag) ? null : collision);
			flag = false;
		}
	}

	// Token: 0x06001A30 RID: 6704 RVA: 0x000C173F File Offset: 0x000BFB3F
	private void OnCollisionEnter(Collision collision)
	{
		ForwardEvents.ForwardCollisionEnter(collision);
	}

	// Token: 0x06001A31 RID: 6705 RVA: 0x000C1748 File Offset: 0x000BFB48
	public static void ForwardCollisionStay(Collision collision, GameObject gameObject)
	{
		CollisionManager.ForwardCollisionStay(gameObject, collision);
		foreach (ContactPoint contactPoint in collision.contacts)
		{
			if (!(contactPoint.thisCollider == null) && !(contactPoint.otherCollider == null))
			{
				CollisionManager.ForwardCollisionEnter(contactPoint.thisCollider.gameObject, contactPoint.otherCollider.gameObject, null);
			}
		}
	}

	// Token: 0x06001A32 RID: 6706 RVA: 0x000C17CB File Offset: 0x000BFBCB
	private void OnCollisionStay(Collision collision)
	{
		ForwardEvents.ForwardCollisionStay(collision, base.gameObject);
	}

	// Token: 0x06001A33 RID: 6707 RVA: 0x000C17D9 File Offset: 0x000BFBD9
	private void OnCollisionExit()
	{
	}

	// Token: 0x06001A34 RID: 6708 RVA: 0x000C17DB File Offset: 0x000BFBDB
	private void OnTriggerEnter(Collider collider)
	{
		CollisionManager.ForwardTriggerEnter(base.gameObject, collider);
	}

	// Token: 0x06001A35 RID: 6709 RVA: 0x000C17E9 File Offset: 0x000BFBE9
	private void OnTriggerExit(Collider collider)
	{
		CollisionManager.ForwardTriggerExit(base.gameObject, collider);
	}
}
