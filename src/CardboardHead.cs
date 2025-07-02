using UnityEngine;

[AddComponentMenu("Cardboard/CardboardHead")]
public class CardboardHead : MonoBehaviour
{
	public delegate void HeadUpdatedDelegate(GameObject head);

	public bool trackRotation = true;

	public bool trackPosition = true;

	public Transform target;

	public bool updateEarly;

	private bool updated;

	public Ray Gaze
	{
		get
		{
			UpdateHead();
			return new Ray(base.transform.position, base.transform.forward);
		}
	}

	public event HeadUpdatedDelegate OnHeadUpdated;

	private void Awake()
	{
		Cardboard.Create();
	}

	private void Update()
	{
		updated = false;
		if (updateEarly)
		{
			UpdateHead();
		}
	}

	private void LateUpdate()
	{
		UpdateHead();
	}

	private void UpdateHead()
	{
		if (updated)
		{
			return;
		}
		updated = true;
		Cardboard.SDK.UpdateState();
		if (trackRotation)
		{
			Quaternion orientation = Cardboard.SDK.HeadPose.Orientation;
			if (target == null)
			{
				base.transform.localRotation = orientation;
			}
			else
			{
				base.transform.rotation = target.rotation * orientation;
			}
		}
		if (trackPosition)
		{
			Vector3 position = Cardboard.SDK.HeadPose.Position;
			if (target == null)
			{
				base.transform.localPosition = position;
			}
			else
			{
				base.transform.position = target.position + target.rotation * position;
			}
		}
		if (this.OnHeadUpdated != null)
		{
			this.OnHeadUpdated(base.gameObject);
		}
	}
}
