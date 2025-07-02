using UnityEngine;

public class TiltManager : MonoBehaviour
{
	private static TiltManager _instance;

	private Gyroscope gyro;

	private Quaternion baseAttitude;

	private Quaternion currentAttitude;

	private Quaternion correctedAttitude;

	private Quaternion relativeAttitude;

	private float tiltTwist;

	private Vector3 gravity = -Vector3.forward;

	private Vector3 baseGravity = -Vector3.forward;

	private Quaternion relativeGravityCorrect;

	private bool useMouse;

	private float mouseSensitivity = 1f;

	private Vector3 baseMousePosition;

	private bool tiltNotSupported;

	private bool isMonitoring;

	private float monitorStartTime;

	private bool haveBaseRotation;

	public static TiltManager Instance
	{
		get
		{
			if (_instance == null)
			{
				GameObject gameObject = new GameObject("TiltManager");
				_instance = gameObject.AddComponent<TiltManager>();
			}
			return _instance;
		}
	}

	public void Init()
	{
		if (!SystemInfo.supportsGyroscope)
		{
			if (BW.Options.useMouse())
			{
				useMouse = true;
			}
			else
			{
				tiltNotSupported = true;
			}
		}
		gyro = Input.gyro;
	}

	public void FixedUpdate()
	{
		if (isMonitoring && !tiltNotSupported)
		{
			if (useMouse)
			{
				UpdateFromMouse();
			}
			else
			{
				UpdateFromGyro();
			}
		}
	}

	private void UpdateFromMouse()
	{
		if (!haveBaseRotation)
		{
			if (Time.time <= monitorStartTime + 0.25f)
			{
				return;
			}
			baseMousePosition = Input.mousePosition;
			baseAttitude = Quaternion.identity;
			baseGravity = new Vector3(0f, 0f, -1f);
			relativeGravityCorrect = Quaternion.identity;
			haveBaseRotation = true;
		}
		Vector3 vector = mouseSensitivity * (Input.mousePosition - baseMousePosition) / Screen.width;
		currentAttitude = Quaternion.AngleAxis(vector.x * 90f, -Vector3.up) * Quaternion.AngleAxis(vector.y * 90f, Vector3.right);
		gravity = currentAttitude * baseGravity;
		correctedAttitude = ApplyDeviceToUnityCorrection(currentAttitude);
		relativeAttitude = correctedAttitude;
		tiltTwist = 0f;
	}

	private void UpdateFromGyro()
	{
		if (!haveBaseRotation)
		{
			if (Time.time <= monitorStartTime + 0.25f)
			{
				return;
			}
			baseAttitude = ApplyDeviceToUnityCorrection(gyro.attitude);
			baseGravity = gyro.gravity;
			relativeGravityCorrect = Quaternion.FromToRotation(baseGravity, -Vector3.forward);
			haveBaseRotation = true;
		}
		currentAttitude = gyro.attitude;
		gravity = gyro.gravity;
		correctedAttitude = ApplyDeviceToUnityCorrection(currentAttitude);
		relativeAttitude = Quaternion.Inverse(baseAttitude) * correctedAttitude;
		tiltTwist = (relativeAttitude * Vector3.up).x;
	}

	private Quaternion ApplyDeviceToUnityCorrection(Quaternion q)
	{
		return new Quaternion(q.x, q.y, 0f - q.z, 0f - q.w);
	}

	public void StartMonitoring()
	{
		if (!tiltNotSupported)
		{
			isMonitoring = true;
			gyro.enabled = true;
			haveBaseRotation = false;
			monitorStartTime = Time.time;
		}
	}

	public void StopMonitoring()
	{
		isMonitoring = false;
		gyro.enabled = false;
		haveBaseRotation = false;
	}

	public bool IsMonitoring()
	{
		if (isMonitoring)
		{
			return haveBaseRotation;
		}
		return false;
	}

	public void ResetOrientation()
	{
		gravity = -Vector3.forward;
		baseGravity = gravity;
		relativeGravityCorrect = Quaternion.identity;
		haveBaseRotation = false;
		monitorStartTime = Time.time;
	}

	public float GetTiltTwist()
	{
		return tiltTwist;
	}

	public Quaternion GetRawAttitude()
	{
		return currentAttitude;
	}

	public Quaternion GetCurrentAttitude()
	{
		return correctedAttitude;
	}

	public Vector3 GetGravityVector()
	{
		return gravity;
	}

	public Vector3 GetRelativeGravityVector()
	{
		return relativeGravityCorrect * gravity;
	}
}
