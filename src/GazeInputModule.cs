using System;
using UnityEngine;
using UnityEngine.EventSystems;

[AddComponentMenu("Cardboard/GazeInputModule")]
public class GazeInputModule : BaseInputModule
{
	[Tooltip("Whether gaze input is active in VR Mode only (true), or all the time (false).")]
	public bool vrModeOnly;

	[HideInInspector]
	public float clickTime = 0.1f;

	[HideInInspector]
	public Vector2 hotspot = new Vector2(0.5f, 0.5f);

	private PointerEventData pointerData;

	private Vector2 lastHeadPose;

	public static ICardboardPointer cardboardPointer;

	private bool isActive;

	public override bool ShouldActivateModule()
	{
		bool flag = base.ShouldActivateModule() && (Cardboard.SDK.VRModeEnabled || !vrModeOnly);
		if (flag != isActive)
		{
			isActive = flag;
			if (cardboardPointer != null && isActive)
			{
				cardboardPointer.OnGazeEnabled();
			}
		}
		return flag;
	}

	public override void DeactivateModule()
	{
		DisableGazePointer();
		base.DeactivateModule();
		if (pointerData != null)
		{
			HandlePendingClick();
			HandlePointerExitAndEnter(pointerData, null);
			pointerData = null;
		}
		base.eventSystem.SetSelectedGameObject(null, GetBaseEventData());
	}

	public override bool IsPointerOverGameObject(int pointerId)
	{
		if (pointerData != null)
		{
			return pointerData.pointerEnter != null;
		}
		return false;
	}

	public override void Process()
	{
		GameObject currentGameObject = GetCurrentGameObject();
		CastRayFromGaze();
		UpdateCurrentObject();
		UpdateReticle(currentGameObject);
		Camera enterEventCamera = pointerData.enterEventCamera;
		if (!Cardboard.SDK.TapIsTrigger && !Input.GetMouseButtonDown(0) && Input.GetMouseButton(0))
		{
			HandleDrag();
		}
		else
		{
			if (!(Time.unscaledTime - pointerData.clickTime >= clickTime))
			{
				return;
			}
			if (!pointerData.eligibleForClick && (Cardboard.SDK.Triggered || (!Cardboard.SDK.TapIsTrigger && Input.GetMouseButtonDown(0))))
			{
				HandleTrigger();
				if (cardboardPointer != null)
				{
					cardboardPointer.OnGazeTriggerStart(enterEventCamera);
				}
			}
			else if (!Cardboard.SDK.Triggered && !Input.GetMouseButton(0))
			{
				HandlePendingClick();
			}
		}
	}

	private void CastRayFromGaze()
	{
		Vector2 vector = NormalizedCartesianToSpherical(Cardboard.SDK.HeadPose.Orientation * Vector3.forward);
		if (pointerData == null)
		{
			pointerData = new PointerEventData(base.eventSystem);
			lastHeadPose = vector;
		}
		pointerData.Reset();
		pointerData.position = new Vector2(hotspot.x * (float)Screen.width, hotspot.y * (float)Screen.height);
		base.eventSystem.RaycastAll(pointerData, m_RaycastResultCache);
		pointerData.pointerCurrentRaycast = BaseInputModule.FindFirstRaycast(m_RaycastResultCache);
		m_RaycastResultCache.Clear();
		pointerData.delta = vector - lastHeadPose;
		lastHeadPose = vector;
	}

	private void UpdateCurrentObject()
	{
		GameObject gameObject = pointerData.pointerCurrentRaycast.gameObject;
		HandlePointerExitAndEnter(pointerData, gameObject);
		GameObject eventHandler = ExecuteEvents.GetEventHandler<ISelectHandler>(gameObject);
		if (eventHandler == base.eventSystem.currentSelectedGameObject)
		{
			ExecuteEvents.Execute(base.eventSystem.currentSelectedGameObject, GetBaseEventData(), ExecuteEvents.updateSelectedHandler);
		}
		else
		{
			base.eventSystem.SetSelectedGameObject(null, pointerData);
		}
	}

	private void UpdateReticle(GameObject previousGazedObject)
	{
		if (cardboardPointer == null)
		{
			return;
		}
		Camera enterEventCamera = pointerData.enterEventCamera;
		GameObject currentGameObject = GetCurrentGameObject();
		Vector3 intersectionPosition = GetIntersectionPosition();
		if (currentGameObject == previousGazedObject)
		{
			if (currentGameObject != null)
			{
				cardboardPointer.OnGazeStay(enterEventCamera, currentGameObject, intersectionPosition);
			}
			return;
		}
		if (previousGazedObject != null)
		{
			cardboardPointer.OnGazeExit(enterEventCamera, previousGazedObject);
		}
		if (currentGameObject != null)
		{
			cardboardPointer.OnGazeStart(enterEventCamera, currentGameObject, intersectionPosition);
		}
	}

	private void HandleDrag()
	{
		bool flag = pointerData.IsPointerMoving();
		if (flag && pointerData.pointerDrag != null && !pointerData.dragging)
		{
			ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.beginDragHandler);
			pointerData.dragging = true;
		}
		if (pointerData.dragging && flag && pointerData.pointerDrag != null)
		{
			if (pointerData.pointerPress != pointerData.pointerDrag)
			{
				ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerUpHandler);
				pointerData.eligibleForClick = false;
				pointerData.pointerPress = null;
				pointerData.rawPointerPress = null;
			}
			ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.dragHandler);
		}
	}

	private void HandlePendingClick()
	{
		if (pointerData.eligibleForClick)
		{
			if (cardboardPointer != null)
			{
				Camera enterEventCamera = pointerData.enterEventCamera;
				cardboardPointer.OnGazeTriggerEnd(enterEventCamera);
			}
			GameObject root = pointerData.pointerCurrentRaycast.gameObject;
			ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerUpHandler);
			ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerClickHandler);
			if (pointerData.pointerDrag != null)
			{
				ExecuteEvents.ExecuteHierarchy(root, pointerData, ExecuteEvents.dropHandler);
			}
			if (pointerData.pointerDrag != null && pointerData.dragging)
			{
				ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.endDragHandler);
			}
			pointerData.pointerPress = null;
			pointerData.rawPointerPress = null;
			pointerData.eligibleForClick = false;
			pointerData.clickCount = 0;
			pointerData.pointerDrag = null;
			pointerData.dragging = false;
		}
	}

	private void HandleTrigger()
	{
		GameObject gameObject = pointerData.pointerCurrentRaycast.gameObject;
		pointerData.pressPosition = pointerData.position;
		pointerData.pointerPressRaycast = pointerData.pointerCurrentRaycast;
		pointerData.pointerPress = ExecuteEvents.ExecuteHierarchy(gameObject, pointerData, ExecuteEvents.pointerDownHandler) ?? ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
		pointerData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(gameObject);
		if (pointerData.pointerDrag != null && !Cardboard.SDK.TapIsTrigger)
		{
			ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.initializePotentialDrag);
		}
		pointerData.rawPointerPress = gameObject;
		pointerData.eligibleForClick = true;
		pointerData.delta = Vector2.zero;
		pointerData.dragging = false;
		pointerData.useDragThreshold = true;
		pointerData.clickCount = 1;
		pointerData.clickTime = Time.unscaledTime;
	}

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
			num += (float)Math.PI;
		}
		float y = Mathf.Asin(cartCoords.y);
		return new Vector2(num, y);
	}

	private GameObject GetCurrentGameObject()
	{
		if (pointerData != null && pointerData.enterEventCamera != null)
		{
			return pointerData.pointerCurrentRaycast.gameObject;
		}
		return null;
	}

	private Vector3 GetIntersectionPosition()
	{
		Camera enterEventCamera = pointerData.enterEventCamera;
		if (enterEventCamera == null)
		{
			return Vector3.zero;
		}
		float num = pointerData.pointerCurrentRaycast.distance + enterEventCamera.nearClipPlane;
		return enterEventCamera.transform.position + enterEventCamera.transform.forward * num;
	}

	private void DisableGazePointer()
	{
		if (cardboardPointer != null)
		{
			GameObject currentGameObject = GetCurrentGameObject();
			if ((bool)currentGameObject)
			{
				Camera enterEventCamera = pointerData.enterEventCamera;
				cardboardPointer.OnGazeExit(enterEventCamera, currentGameObject);
			}
			cardboardPointer.OnGazeDisabled();
		}
	}
}
