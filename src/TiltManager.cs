using System;
using UnityEngine;

// Token: 0x020001AA RID: 426
public class TiltManager : MonoBehaviour
{
	// Token: 0x1700006E RID: 110
	// (get) Token: 0x06001784 RID: 6020 RVA: 0x000A6138 File Offset: 0x000A4538
	public static TiltManager Instance
	{
		get
		{
			if (TiltManager._instance == null)
			{
				GameObject gameObject = new GameObject("TiltManager");
				TiltManager._instance = gameObject.AddComponent<TiltManager>();
			}
			return TiltManager._instance;
		}
	}

	// Token: 0x06001785 RID: 6021 RVA: 0x000A6170 File Offset: 0x000A4570
	public void Init()
	{
		if (!SystemInfo.supportsGyroscope)
		{
			if (BW.Options.useMouse())
			{
				this.useMouse = true;
			}
			else
			{
				this.tiltNotSupported = true;
			}
		}
		this.gyro = Input.gyro;
	}

	// Token: 0x06001786 RID: 6022 RVA: 0x000A61A9 File Offset: 0x000A45A9
	public void FixedUpdate()
	{
		if (!this.isMonitoring || this.tiltNotSupported)
		{
			return;
		}
		if (this.useMouse)
		{
			this.UpdateFromMouse();
		}
		else
		{
			this.UpdateFromGyro();
		}
	}

	// Token: 0x06001787 RID: 6023 RVA: 0x000A61E0 File Offset: 0x000A45E0
	private void UpdateFromMouse()
	{
		if (!this.haveBaseRotation)
		{
			if (Time.time <= this.monitorStartTime + 0.25f)
			{
				return;
			}
			this.baseMousePosition = Input.mousePosition;
			this.baseAttitude = Quaternion.identity;
			this.baseGravity = new Vector3(0f, 0f, -1f);
			this.relativeGravityCorrect = Quaternion.identity;
			this.haveBaseRotation = true;
		}
		Vector3 vector = this.mouseSensitivity * (Input.mousePosition - this.baseMousePosition) / (float)Screen.width;
		this.currentAttitude = Quaternion.AngleAxis(vector.x * 90f, -Vector3.up) * Quaternion.AngleAxis(vector.y * 90f, Vector3.right);
		this.gravity = this.currentAttitude * this.baseGravity;
		this.correctedAttitude = this.ApplyDeviceToUnityCorrection(this.currentAttitude);
		this.relativeAttitude = this.correctedAttitude;
		this.tiltTwist = 0f;
	}

	// Token: 0x06001788 RID: 6024 RVA: 0x000A62FC File Offset: 0x000A46FC
	private void UpdateFromGyro()
	{
		if (!this.haveBaseRotation)
		{
			if (Time.time <= this.monitorStartTime + 0.25f)
			{
				return;
			}
			this.baseAttitude = this.ApplyDeviceToUnityCorrection(this.gyro.attitude);
			this.baseGravity = this.gyro.gravity;
			this.relativeGravityCorrect = Quaternion.FromToRotation(this.baseGravity, -Vector3.forward);
			this.haveBaseRotation = true;
		}
		this.currentAttitude = this.gyro.attitude;
		this.gravity = this.gyro.gravity;
		this.correctedAttitude = this.ApplyDeviceToUnityCorrection(this.currentAttitude);
		this.relativeAttitude = Quaternion.Inverse(this.baseAttitude) * this.correctedAttitude;
		this.tiltTwist = (this.relativeAttitude * Vector3.up).x;
	}

	// Token: 0x06001789 RID: 6025 RVA: 0x000A63E8 File Offset: 0x000A47E8
	private Quaternion ApplyDeviceToUnityCorrection(Quaternion q)
	{
		Quaternion result = new Quaternion(q.x, q.y, -q.z, -q.w);
		return result;
	}

	// Token: 0x0600178A RID: 6026 RVA: 0x000A641B File Offset: 0x000A481B
	public void StartMonitoring()
	{
		if (this.tiltNotSupported)
		{
			return;
		}
		this.isMonitoring = true;
		this.gyro.enabled = true;
		this.haveBaseRotation = false;
		this.monitorStartTime = Time.time;
	}

	// Token: 0x0600178B RID: 6027 RVA: 0x000A644E File Offset: 0x000A484E
	public void StopMonitoring()
	{
		this.isMonitoring = false;
		this.gyro.enabled = false;
		this.haveBaseRotation = false;
	}

	// Token: 0x0600178C RID: 6028 RVA: 0x000A646A File Offset: 0x000A486A
	public bool IsMonitoring()
	{
		return this.isMonitoring && this.haveBaseRotation;
	}

	// Token: 0x0600178D RID: 6029 RVA: 0x000A6480 File Offset: 0x000A4880
	public void ResetOrientation()
	{
		this.gravity = -Vector3.forward;
		this.baseGravity = this.gravity;
		this.relativeGravityCorrect = Quaternion.identity;
		this.haveBaseRotation = false;
		this.monitorStartTime = Time.time;
	}

	// Token: 0x0600178E RID: 6030 RVA: 0x000A64BB File Offset: 0x000A48BB
	public float GetTiltTwist()
	{
		return this.tiltTwist;
	}

	// Token: 0x0600178F RID: 6031 RVA: 0x000A64C3 File Offset: 0x000A48C3
	public Quaternion GetRawAttitude()
	{
		return this.currentAttitude;
	}

	// Token: 0x06001790 RID: 6032 RVA: 0x000A64CB File Offset: 0x000A48CB
	public Quaternion GetCurrentAttitude()
	{
		return this.correctedAttitude;
	}

	// Token: 0x06001791 RID: 6033 RVA: 0x000A64D3 File Offset: 0x000A48D3
	public Vector3 GetGravityVector()
	{
		return this.gravity;
	}

	// Token: 0x06001792 RID: 6034 RVA: 0x000A64DB File Offset: 0x000A48DB
	public Vector3 GetRelativeGravityVector()
	{
		return this.relativeGravityCorrect * this.gravity;
	}

	// Token: 0x04001276 RID: 4726
	private static TiltManager _instance;

	// Token: 0x04001277 RID: 4727
	private Gyroscope gyro;

	// Token: 0x04001278 RID: 4728
	private Quaternion baseAttitude;

	// Token: 0x04001279 RID: 4729
	private Quaternion currentAttitude;

	// Token: 0x0400127A RID: 4730
	private Quaternion correctedAttitude;

	// Token: 0x0400127B RID: 4731
	private Quaternion relativeAttitude;

	// Token: 0x0400127C RID: 4732
	private float tiltTwist;

	// Token: 0x0400127D RID: 4733
	private Vector3 gravity = -Vector3.forward;

	// Token: 0x0400127E RID: 4734
	private Vector3 baseGravity = -Vector3.forward;

	// Token: 0x0400127F RID: 4735
	private Quaternion relativeGravityCorrect;

	// Token: 0x04001280 RID: 4736
	private bool useMouse;

	// Token: 0x04001281 RID: 4737
	private float mouseSensitivity = 1f;

	// Token: 0x04001282 RID: 4738
	private Vector3 baseMousePosition;

	// Token: 0x04001283 RID: 4739
	private bool tiltNotSupported;

	// Token: 0x04001284 RID: 4740
	private bool isMonitoring;

	// Token: 0x04001285 RID: 4741
	private float monitorStartTime;

	// Token: 0x04001286 RID: 4742
	private bool haveBaseRotation;
}
