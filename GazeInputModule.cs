using System;
using UnityEngine;
using UnityEngine.EventSystems;

// Token: 0x02000024 RID: 36
[AddComponentMenu("Cardboard/GazeInputModule")]
public class GazeInputModule : BaseInputModule
{
	// Token: 0x0600011E RID: 286 RVA: 0x00007F44 File Offset: 0x00006344
	public override bool ShouldActivateModule()
	{
		bool flag = base.ShouldActivateModule();
		flag = (flag && (Cardboard.SDK.VRModeEnabled || !this.vrModeOnly));
		if (flag != this.isActive)
		{
			this.isActive = flag;
			if (GazeInputModule.cardboardPointer != null && this.isActive)
			{
				GazeInputModule.cardboardPointer.OnGazeEnabled();
			}
		}
		return flag;
	}

	// Token: 0x0600011F RID: 287 RVA: 0x00007FB0 File Offset: 0x000063B0
	public override void DeactivateModule()
	{
		this.DisableGazePointer();
		base.DeactivateModule();
		if (this.pointerData != null)
		{
			this.HandlePendingClick();
			base.HandlePointerExitAndEnter(this.pointerData, null);
			this.pointerData = null;
		}
		base.eventSystem.SetSelectedGameObject(null, this.GetBaseEventData());
	}

	// Token: 0x06000120 RID: 288 RVA: 0x00008000 File Offset: 0x00006400
	public override bool IsPointerOverGameObject(int pointerId)
	{
		return this.pointerData != null && this.pointerData.pointerEnter != null;
	}

	// Token: 0x06000121 RID: 289 RVA: 0x00008024 File Offset: 0x00006424
	public override void Process()
	{
		GameObject currentGameObject = this.GetCurrentGameObject();
		this.CastRayFromGaze();
		this.UpdateCurrentObject();
		this.UpdateReticle(currentGameObject);
		Camera enterEventCamera = this.pointerData.enterEventCamera;
		if (!Cardboard.SDK.TapIsTrigger && !Input.GetMouseButtonDown(0) && Input.GetMouseButton(0))
		{
			this.HandleDrag();
		}
		else if (Time.unscaledTime - this.pointerData.clickTime >= this.clickTime)
		{
			if (!this.pointerData.eligibleForClick && (Cardboard.SDK.Triggered || (!Cardboard.SDK.TapIsTrigger && Input.GetMouseButtonDown(0))))
			{
				this.HandleTrigger();
				if (GazeInputModule.cardboardPointer != null)
				{
					GazeInputModule.cardboardPointer.OnGazeTriggerStart(enterEventCamera);
				}
			}
			else if (!Cardboard.SDK.Triggered && !Input.GetMouseButton(0))
			{
				this.HandlePendingClick();
			}
		}
	}

	// Token: 0x06000122 RID: 290 RVA: 0x00008124 File Offset: 0x00006524
	private void CastRayFromGaze()
	{
		Vector2 a = this.NormalizedCartesianToSpherical(Cardboard.SDK.HeadPose.Orientation * Vector3.forward);
		if (this.pointerData == null)
		{
			this.pointerData = new PointerEventData(base.eventSystem);
			this.lastHeadPose = a;
		}
		this.pointerData.Reset();
		this.pointerData.position = new Vector2(this.hotspot.x * (float)Screen.width, this.hotspot.y * (float)Screen.height);
		base.eventSystem.RaycastAll(this.pointerData, this.m_RaycastResultCache);
		this.pointerData.pointerCurrentRaycast = BaseInputModule.FindFirstRaycast(this.m_RaycastResultCache);
		this.m_RaycastResultCache.Clear();
		this.pointerData.delta = a - this.lastHeadPose;
		this.lastHeadPose = a;
	}

	// Token: 0x06000123 RID: 291 RVA: 0x0000820C File Offset: 0x0000660C
	private void UpdateCurrentObject()
	{
		GameObject gameObject = this.pointerData.pointerCurrentRaycast.gameObject;
		base.HandlePointerExitAndEnter(this.pointerData, gameObject);
		GameObject eventHandler = ExecuteEvents.GetEventHandler<ISelectHandler>(gameObject);
		if (eventHandler == base.eventSystem.currentSelectedGameObject)
		{
			ExecuteEvents.Execute<IUpdateSelectedHandler>(base.eventSystem.currentSelectedGameObject, this.GetBaseEventData(), ExecuteEvents.updateSelectedHandler);
		}
		else
		{
			base.eventSystem.SetSelectedGameObject(null, this.pointerData);
		}
	}

	// Token: 0x06000124 RID: 292 RVA: 0x0000828C File Offset: 0x0000668C
	private void UpdateReticle(GameObject previousGazedObject)
	{
		if (GazeInputModule.cardboardPointer == null)
		{
			return;
		}
		Camera enterEventCamera = this.pointerData.enterEventCamera;
		GameObject currentGameObject = this.GetCurrentGameObject();
		Vector3 intersectionPosition = this.GetIntersectionPosition();
		if (currentGameObject == previousGazedObject)
		{
			if (currentGameObject != null)
			{
				GazeInputModule.cardboardPointer.OnGazeStay(enterEventCamera, currentGameObject, intersectionPosition);
			}
		}
		else
		{
			if (previousGazedObject != null)
			{
				GazeInputModule.cardboardPointer.OnGazeExit(enterEventCamera, previousGazedObject);
			}
			if (currentGameObject != null)
			{
				GazeInputModule.cardboardPointer.OnGazeStart(enterEventCamera, currentGameObject, intersectionPosition);
			}
		}
	}

	// Token: 0x06000125 RID: 293 RVA: 0x0000831C File Offset: 0x0000671C
	private void HandleDrag()
	{
		bool flag = this.pointerData.IsPointerMoving();
		if (flag && this.pointerData.pointerDrag != null && !this.pointerData.dragging)
		{
			ExecuteEvents.Execute<IBeginDragHandler>(this.pointerData.pointerDrag, this.pointerData, ExecuteEvents.beginDragHandler);
			this.pointerData.dragging = true;
		}
		if (this.pointerData.dragging && flag && this.pointerData.pointerDrag != null)
		{
			if (this.pointerData.pointerPress != this.pointerData.pointerDrag)
			{
				ExecuteEvents.Execute<IPointerUpHandler>(this.pointerData.pointerPress, this.pointerData, ExecuteEvents.pointerUpHandler);
				this.pointerData.eligibleForClick = false;
				this.pointerData.pointerPress = null;
				this.pointerData.rawPointerPress = null;
			}
			ExecuteEvents.Execute<IDragHandler>(this.pointerData.pointerDrag, this.pointerData, ExecuteEvents.dragHandler);
		}
	}

	// Token: 0x06000126 RID: 294 RVA: 0x00008434 File Offset: 0x00006834
	private void HandlePendingClick()
	{
		if (!this.pointerData.eligibleForClick)
		{
			return;
		}
		if (GazeInputModule.cardboardPointer != null)
		{
			Camera enterEventCamera = this.pointerData.enterEventCamera;
			GazeInputModule.cardboardPointer.OnGazeTriggerEnd(enterEventCamera);
		}
		GameObject gameObject = this.pointerData.pointerCurrentRaycast.gameObject;
		ExecuteEvents.Execute<IPointerUpHandler>(this.pointerData.pointerPress, this.pointerData, ExecuteEvents.pointerUpHandler);
		ExecuteEvents.Execute<IPointerClickHandler>(this.pointerData.pointerPress, this.pointerData, ExecuteEvents.pointerClickHandler);
		if (this.pointerData.pointerDrag != null)
		{
			ExecuteEvents.ExecuteHierarchy<IDropHandler>(gameObject, this.pointerData, ExecuteEvents.dropHandler);
		}
		if (this.pointerData.pointerDrag != null && this.pointerData.dragging)
		{
			ExecuteEvents.Execute<IEndDragHandler>(this.pointerData.pointerDrag, this.pointerData, ExecuteEvents.endDragHandler);
		}
		this.pointerData.pointerPress = null;
		this.pointerData.rawPointerPress = null;
		this.pointerData.eligibleForClick = false;
		this.pointerData.clickCount = 0;
		this.pointerData.pointerDrag = null;
		this.pointerData.dragging = false;
	}

	// Token: 0x06000127 RID: 295 RVA: 0x00008574 File Offset: 0x00006974
	private void HandleTrigger()
	{
		GameObject gameObject = this.pointerData.pointerCurrentRaycast.gameObject;
		this.pointerData.pressPosition = this.pointerData.position;
		this.pointerData.pointerPressRaycast = this.pointerData.pointerCurrentRaycast;
		this.pointerData.pointerPress = (ExecuteEvents.ExecuteHierarchy<IPointerDownHandler>(gameObject, this.pointerData, ExecuteEvents.pointerDownHandler) ?? ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject));
		this.pointerData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(gameObject);
		if (this.pointerData.pointerDrag != null && !Cardboard.SDK.TapIsTrigger)
		{
			ExecuteEvents.Execute<IInitializePotentialDragHandler>(this.pointerData.pointerDrag, this.pointerData, ExecuteEvents.initializePotentialDrag);
		}
		this.pointerData.rawPointerPress = gameObject;
		this.pointerData.eligibleForClick = true;
		this.pointerData.delta = Vector2.zero;
		this.pointerData.dragging = false;
		this.pointerData.useDragThreshold = true;
		this.pointerData.clickCount = 1;
		this.pointerData.clickTime = Time.unscaledTime;
	}

	// Token: 0x06000128 RID: 296 RVA: 0x00008698 File Offset: 0x00006A98
	private Vector2 NormalizedCartesianToSpherical(Vector3 cartCoords)
	{
		cartCoords.Normalize();
		if (cartCoords.x == 0f)
		{
			cartCoords.x = Mathf.Epsilon;
		}
		float num = Mathf.Atan(cartCoords.z / cartCoords.x);
		if (cartCoords.x < 0f)
		{
			num += 3.14159274f;
		}
		float y = Mathf.Asin(cartCoords.y);
		return new Vector2(num, y);
	}

	// Token: 0x06000129 RID: 297 RVA: 0x0000870C File Offset: 0x00006B0C
	private GameObject GetCurrentGameObject()
	{
		if (this.pointerData != null && this.pointerData.enterEventCamera != null)
		{
			return this.pointerData.pointerCurrentRaycast.gameObject;
		}
		return null;
	}

	// Token: 0x0600012A RID: 298 RVA: 0x00008750 File Offset: 0x00006B50
	private Vector3 GetIntersectionPosition()
	{
		Camera enterEventCamera = this.pointerData.enterEventCamera;
		if (enterEventCamera == null)
		{
			return Vector3.zero;
		}
		float d = this.pointerData.pointerCurrentRaycast.distance + enterEventCamera.nearClipPlane;
		return enterEventCamera.transform.position + enterEventCamera.transform.forward * d;
	}

	// Token: 0x0600012B RID: 299 RVA: 0x000087BC File Offset: 0x00006BBC
	private void DisableGazePointer()
	{
		if (GazeInputModule.cardboardPointer == null)
		{
			return;
		}
		GameObject currentGameObject = this.GetCurrentGameObject();
		if (currentGameObject)
		{
			Camera enterEventCamera = this.pointerData.enterEventCamera;
			GazeInputModule.cardboardPointer.OnGazeExit(enterEventCamera, currentGameObject);
		}
		GazeInputModule.cardboardPointer.OnGazeDisabled();
	}

	// Token: 0x0400016E RID: 366
	[Tooltip("Whether gaze input is active in VR Mode only (true), or all the time (false).")]
	public bool vrModeOnly;

	// Token: 0x0400016F RID: 367
	[HideInInspector]
	public float clickTime = 0.1f;

	// Token: 0x04000170 RID: 368
	[HideInInspector]
	public Vector2 hotspot = new Vector2(0.5f, 0.5f);

	// Token: 0x04000171 RID: 369
	private PointerEventData pointerData;

	// Token: 0x04000172 RID: 370
	private Vector2 lastHeadPose;

	// Token: 0x04000173 RID: 371
	public static ICardboardPointer cardboardPointer;

	// Token: 0x04000174 RID: 372
	private bool isActive;
}
