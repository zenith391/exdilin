using UnityEngine;

public interface ICardboardPointer
{
	void OnGazeEnabled();

	void OnGazeDisabled();

	void OnGazeStart(Camera camera, GameObject targetObject, Vector3 intersectionPosition);

	void OnGazeStay(Camera camera, GameObject targetObject, Vector3 intersectionPosition);

	void OnGazeExit(Camera camera, GameObject targetObject);

	void OnGazeTriggerStart(Camera camera);

	void OnGazeTriggerEnd(Camera camera);
}
