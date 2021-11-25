using System;
using UnityEngine;

namespace TMPro.Examples
{
	// Token: 0x02000375 RID: 885
	public class CameraController : MonoBehaviour
	{
		// Token: 0x0600279D RID: 10141 RVA: 0x001224F0 File Offset: 0x001208F0
		private void Awake()
		{
			if (QualitySettings.vSyncCount > 0)
			{
				Application.targetFrameRate = 60;
			}
			else
			{
				Application.targetFrameRate = -1;
			}
			if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
			{
				Input.simulateMouseWithTouches = false;
			}
			this.cameraTransform = base.transform;
			this.previousSmoothing = this.MovementSmoothing;
		}

		// Token: 0x0600279E RID: 10142 RVA: 0x0012254F File Offset: 0x0012094F
		private void Start()
		{
			if (this.CameraTarget == null)
			{
				this.dummyTarget = new GameObject("Camera Target").transform;
				this.CameraTarget = this.dummyTarget;
			}
		}

		// Token: 0x0600279F RID: 10143 RVA: 0x00122584 File Offset: 0x00120984
		private void LateUpdate()
		{
			this.GetPlayerInput();
			if (this.CameraTarget != null)
			{
				if (this.CameraMode == CameraController.CameraModes.Isometric)
				{
					this.desiredPosition = this.CameraTarget.position + Quaternion.Euler(this.ElevationAngle, this.OrbitalAngle, 0f) * new Vector3(0f, 0f, -this.FollowDistance);
				}
				else if (this.CameraMode == CameraController.CameraModes.Follow)
				{
					this.desiredPosition = this.CameraTarget.position + this.CameraTarget.TransformDirection(Quaternion.Euler(this.ElevationAngle, this.OrbitalAngle, 0f) * new Vector3(0f, 0f, -this.FollowDistance));
				}
				if (this.MovementSmoothing)
				{
					this.cameraTransform.position = Vector3.SmoothDamp(this.cameraTransform.position, this.desiredPosition, ref this.currentVelocity, this.MovementSmoothingValue * Time.fixedDeltaTime);
				}
				else
				{
					this.cameraTransform.position = this.desiredPosition;
				}
				if (this.RotationSmoothing)
				{
					this.cameraTransform.rotation = Quaternion.Lerp(this.cameraTransform.rotation, Quaternion.LookRotation(this.CameraTarget.position - this.cameraTransform.position), this.RotationSmoothingValue * Time.deltaTime);
				}
				else
				{
					this.cameraTransform.LookAt(this.CameraTarget);
				}
			}
		}

		// Token: 0x060027A0 RID: 10144 RVA: 0x00122720 File Offset: 0x00120B20
		private void GetPlayerInput()
		{
			this.moveVector = Vector3.zero;
			this.mouseWheel = Input.GetAxis("Mouse ScrollWheel");
			float num = (float)Input.touchCount;
			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || num > 0f)
			{
				this.mouseWheel *= 10f;
				if (Input.GetKeyDown(KeyCode.I))
				{
					this.CameraMode = CameraController.CameraModes.Isometric;
				}
				if (Input.GetKeyDown(KeyCode.F))
				{
					this.CameraMode = CameraController.CameraModes.Follow;
				}
				if (Input.GetKeyDown(KeyCode.S))
				{
					this.MovementSmoothing = !this.MovementSmoothing;
				}
				if (Input.GetMouseButton(1))
				{
					this.mouseY = Input.GetAxis("Mouse Y");
					this.mouseX = Input.GetAxis("Mouse X");
					if (this.mouseY > 0.01f || this.mouseY < -0.01f)
					{
						this.ElevationAngle -= this.mouseY * this.MoveSensitivity;
						this.ElevationAngle = Mathf.Clamp(this.ElevationAngle, this.MinElevationAngle, this.MaxElevationAngle);
					}
					if (this.mouseX > 0.01f || this.mouseX < -0.01f)
					{
						this.OrbitalAngle += this.mouseX * this.MoveSensitivity;
						if (this.OrbitalAngle > 360f)
						{
							this.OrbitalAngle -= 360f;
						}
						if (this.OrbitalAngle < 0f)
						{
							this.OrbitalAngle += 360f;
						}
					}
				}
				if (num == 1f && Input.GetTouch(0).phase == TouchPhase.Moved)
				{
					Vector2 deltaPosition = Input.GetTouch(0).deltaPosition;
					if (deltaPosition.y > 0.01f || deltaPosition.y < -0.01f)
					{
						this.ElevationAngle -= deltaPosition.y * 0.1f;
						this.ElevationAngle = Mathf.Clamp(this.ElevationAngle, this.MinElevationAngle, this.MaxElevationAngle);
					}
					if (deltaPosition.x > 0.01f || deltaPosition.x < -0.01f)
					{
						this.OrbitalAngle += deltaPosition.x * 0.1f;
						if (this.OrbitalAngle > 360f)
						{
							this.OrbitalAngle -= 360f;
						}
						if (this.OrbitalAngle < 0f)
						{
							this.OrbitalAngle += 360f;
						}
					}
				}
				if (Input.GetMouseButton(0))
				{
					Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
					RaycastHit raycastHit;
					if (Physics.Raycast(ray, out raycastHit, 300f, 23552))
					{
						if (raycastHit.transform == this.CameraTarget)
						{
							this.OrbitalAngle = 0f;
						}
						else
						{
							this.CameraTarget = raycastHit.transform;
							this.OrbitalAngle = 0f;
							this.MovementSmoothing = this.previousSmoothing;
						}
					}
				}
				if (Input.GetMouseButton(2))
				{
					if (this.dummyTarget == null)
					{
						this.dummyTarget = new GameObject("Camera Target").transform;
						this.dummyTarget.position = this.CameraTarget.position;
						this.dummyTarget.rotation = this.CameraTarget.rotation;
						this.CameraTarget = this.dummyTarget;
						this.previousSmoothing = this.MovementSmoothing;
						this.MovementSmoothing = false;
					}
					else if (this.dummyTarget != this.CameraTarget)
					{
						this.dummyTarget.position = this.CameraTarget.position;
						this.dummyTarget.rotation = this.CameraTarget.rotation;
						this.CameraTarget = this.dummyTarget;
						this.previousSmoothing = this.MovementSmoothing;
						this.MovementSmoothing = false;
					}
					this.mouseY = Input.GetAxis("Mouse Y");
					this.mouseX = Input.GetAxis("Mouse X");
					this.moveVector = this.cameraTransform.TransformDirection(this.mouseX, this.mouseY, 0f);
					this.dummyTarget.Translate(-this.moveVector, Space.World);
				}
			}
			if (num == 2f)
			{
				Touch touch = Input.GetTouch(0);
				Touch touch2 = Input.GetTouch(1);
				Vector2 a = touch.position - touch.deltaPosition;
				Vector2 b = touch2.position - touch2.deltaPosition;
				float magnitude = (a - b).magnitude;
				float magnitude2 = (touch.position - touch2.position).magnitude;
				float num2 = magnitude - magnitude2;
				if (num2 > 0.01f || num2 < -0.01f)
				{
					this.FollowDistance += num2 * 0.25f;
					this.FollowDistance = Mathf.Clamp(this.FollowDistance, this.MinFollowDistance, this.MaxFollowDistance);
				}
			}
			if (this.mouseWheel < -0.01f || this.mouseWheel > 0.01f)
			{
				this.FollowDistance -= this.mouseWheel * 5f;
				this.FollowDistance = Mathf.Clamp(this.FollowDistance, this.MinFollowDistance, this.MaxFollowDistance);
			}
		}

		// Token: 0x04002277 RID: 8823
		private Transform cameraTransform;

		// Token: 0x04002278 RID: 8824
		private Transform dummyTarget;

		// Token: 0x04002279 RID: 8825
		public Transform CameraTarget;

		// Token: 0x0400227A RID: 8826
		public float FollowDistance = 30f;

		// Token: 0x0400227B RID: 8827
		public float MaxFollowDistance = 100f;

		// Token: 0x0400227C RID: 8828
		public float MinFollowDistance = 2f;

		// Token: 0x0400227D RID: 8829
		public float ElevationAngle = 30f;

		// Token: 0x0400227E RID: 8830
		public float MaxElevationAngle = 85f;

		// Token: 0x0400227F RID: 8831
		public float MinElevationAngle;

		// Token: 0x04002280 RID: 8832
		public float OrbitalAngle;

		// Token: 0x04002281 RID: 8833
		public CameraController.CameraModes CameraMode;

		// Token: 0x04002282 RID: 8834
		public bool MovementSmoothing = true;

		// Token: 0x04002283 RID: 8835
		public bool RotationSmoothing;

		// Token: 0x04002284 RID: 8836
		private bool previousSmoothing;

		// Token: 0x04002285 RID: 8837
		public float MovementSmoothingValue = 25f;

		// Token: 0x04002286 RID: 8838
		public float RotationSmoothingValue = 5f;

		// Token: 0x04002287 RID: 8839
		public float MoveSensitivity = 2f;

		// Token: 0x04002288 RID: 8840
		private Vector3 currentVelocity = Vector3.zero;

		// Token: 0x04002289 RID: 8841
		private Vector3 desiredPosition;

		// Token: 0x0400228A RID: 8842
		private float mouseX;

		// Token: 0x0400228B RID: 8843
		private float mouseY;

		// Token: 0x0400228C RID: 8844
		private Vector3 moveVector;

		// Token: 0x0400228D RID: 8845
		private float mouseWheel;

		// Token: 0x0400228E RID: 8846
		private const string event_SmoothingValue = "Slider - Smoothing Value";

		// Token: 0x0400228F RID: 8847
		private const string event_FollowDistance = "Slider - Camera Zoom";

		// Token: 0x02000376 RID: 886
		public enum CameraModes
		{
			// Token: 0x04002291 RID: 8849
			Follow,
			// Token: 0x04002292 RID: 8850
			Isometric,
			// Token: 0x04002293 RID: 8851
			Free
		}
	}
}
