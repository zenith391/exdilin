using UnityEngine;

public class PullObject
{
	private static LineRenderer lineRenderer;

	private static bool statePulling;

	private static Rigidbody hitRigidbody;

	private static Vector3 hitPoint;

	private static Vector3 hitHandle;

	private static Vector3 planePoint;

	private static Vector3 planeNormal;

	public static void Init()
	{
		lineRenderer = Blocksworld.mainCamera.GetComponent<LineRenderer>();
	}

	public static bool Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			hitRigidbody = null;
			if (Physics.Raycast(Blocksworld.CameraScreenPointToRay(Input.mousePosition), out var hitInfo))
			{
				Rigidbody rigidbody = null;
				if (hitInfo.rigidbody != null)
				{
					rigidbody = hitInfo.rigidbody;
				}
				else if (hitInfo.transform.parent.GetComponent<Rigidbody>() != null)
				{
					rigidbody = hitInfo.rigidbody;
				}
				if (rigidbody != null && !rigidbody.isKinematic)
				{
					hitRigidbody = rigidbody;
					hitPoint = hitInfo.point;
					hitHandle = Quaternion.Inverse(rigidbody.transform.rotation) * (hitInfo.point - rigidbody.transform.position);
				}
			}
			return true;
		}
		if (!statePulling && hitRigidbody != null && Input.GetMouseButtonUp(0))
		{
			Push(hitRigidbody, hitPoint);
			return true;
		}
		return false;
	}

	public static bool FixedUpdate()
	{
		if (statePulling)
		{
			if (Blocksworld.numTouches == 0)
			{
				StopPull();
				statePulling = false;
				return false;
			}
			ContinuePull();
			return true;
		}
		if (hitRigidbody != null && Blocksworld.numTouches == 1)
		{
			if ((Blocksworld.mousePositionFirst - NormalizedInput.mousePosition).sqrMagnitude > 10f)
			{
				StartPull();
				ContinuePull();
				statePulling = true;
			}
			return true;
		}
		return false;
	}

	private static void Push(Rigidbody rb, Vector3 point)
	{
		rb.AddForceAtPosition(50f * Blocksworld.cameraForward, point, ForceMode.Impulse);
	}

	private static void StartPull()
	{
		planePoint = hitPoint;
		planeNormal = -Blocksworld.cameraForward;
		lineRenderer.enabled = true;
	}

	private static void ContinuePull()
	{
		Vector3 vector = hitRigidbody.transform.position + hitRigidbody.transform.rotation * hitHandle;
		Vector3 vector2 = Util.ProjectScreenPointOnWorldPlane(planePoint, planeNormal, Blocksworld.touches[0]);
		Vector3 force = 50f * (vector2 - vector);
		hitRigidbody.AddForceAtPosition(force, vector);
		lineRenderer.SetPosition(0, vector);
		lineRenderer.SetPosition(1, vector2);
	}

	private static void StopPull()
	{
		lineRenderer.enabled = false;
	}
}
