using System;
using UnityEngine;

// Token: 0x02000270 RID: 624
public class PullObject
{
	// Token: 0x06001D26 RID: 7462 RVA: 0x000CDF09 File Offset: 0x000CC309
	public static void Init()
	{
		PullObject.lineRenderer = Blocksworld.mainCamera.GetComponent<LineRenderer>();
	}

	// Token: 0x06001D27 RID: 7463 RVA: 0x000CDF1C File Offset: 0x000CC31C
	public static bool Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			PullObject.hitRigidbody = null;
			RaycastHit raycastHit;
			if (Physics.Raycast(Blocksworld.CameraScreenPointToRay(Input.mousePosition), out raycastHit))
			{
				Rigidbody rigidbody = null;
				if (raycastHit.rigidbody != null)
				{
					rigidbody = raycastHit.rigidbody;
				}
				else if (raycastHit.transform.parent.GetComponent<Rigidbody>() != null)
				{
					rigidbody = raycastHit.rigidbody;
				}
				if (rigidbody != null && !rigidbody.isKinematic)
				{
					PullObject.hitRigidbody = rigidbody;
					PullObject.hitPoint = raycastHit.point;
					PullObject.hitHandle = Quaternion.Inverse(rigidbody.transform.rotation) * (raycastHit.point - rigidbody.transform.position);
				}
			}
			return true;
		}
		if (!PullObject.statePulling && PullObject.hitRigidbody != null && Input.GetMouseButtonUp(0))
		{
			PullObject.Push(PullObject.hitRigidbody, PullObject.hitPoint);
			return true;
		}
		return false;
	}

	// Token: 0x06001D28 RID: 7464 RVA: 0x000CE028 File Offset: 0x000CC428
	public static bool FixedUpdate()
	{
		if (PullObject.statePulling)
		{
			if (Blocksworld.numTouches == 0)
			{
				PullObject.StopPull();
				PullObject.statePulling = false;
				return false;
			}
			PullObject.ContinuePull();
			return true;
		}
		else
		{
			if (PullObject.hitRigidbody != null && Blocksworld.numTouches == 1)
			{
				if ((Blocksworld.mousePositionFirst - NormalizedInput.mousePosition).sqrMagnitude > 10f)
				{
					PullObject.StartPull();
					PullObject.ContinuePull();
					PullObject.statePulling = true;
				}
				return true;
			}
			return false;
		}
	}

	// Token: 0x06001D29 RID: 7465 RVA: 0x000CE0AC File Offset: 0x000CC4AC
	private static void Push(Rigidbody rb, Vector3 point)
	{
		rb.AddForceAtPosition(50f * Blocksworld.cameraForward, point, ForceMode.Impulse);
	}

	// Token: 0x06001D2A RID: 7466 RVA: 0x000CE0C5 File Offset: 0x000CC4C5
	private static void StartPull()
	{
		PullObject.planePoint = PullObject.hitPoint;
		PullObject.planeNormal = -Blocksworld.cameraForward;
		PullObject.lineRenderer.enabled = true;
	}

	// Token: 0x06001D2B RID: 7467 RVA: 0x000CE0EC File Offset: 0x000CC4EC
	private static void ContinuePull()
	{
		Vector3 vector = PullObject.hitRigidbody.transform.position + PullObject.hitRigidbody.transform.rotation * PullObject.hitHandle;
		Vector3 vector2 = Util.ProjectScreenPointOnWorldPlane(PullObject.planePoint, PullObject.planeNormal, Blocksworld.touches[0]);
		Vector3 force = 50f * (vector2 - vector);
		PullObject.hitRigidbody.AddForceAtPosition(force, vector);
		PullObject.lineRenderer.SetPosition(0, vector);
		PullObject.lineRenderer.SetPosition(1, vector2);
	}

	// Token: 0x06001D2C RID: 7468 RVA: 0x000CE17D File Offset: 0x000CC57D
	private static void StopPull()
	{
		PullObject.lineRenderer.enabled = false;
	}

	// Token: 0x040017CC RID: 6092
	private static LineRenderer lineRenderer;

	// Token: 0x040017CD RID: 6093
	private static bool statePulling;

	// Token: 0x040017CE RID: 6094
	private static Rigidbody hitRigidbody;

	// Token: 0x040017CF RID: 6095
	private static Vector3 hitPoint;

	// Token: 0x040017D0 RID: 6096
	private static Vector3 hitHandle;

	// Token: 0x040017D1 RID: 6097
	private static Vector3 planePoint;

	// Token: 0x040017D2 RID: 6098
	private static Vector3 planeNormal;
}
