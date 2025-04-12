using System;
using UnityEngine;

// Token: 0x0200002A RID: 42
public interface ICardboardPointer
{
	// Token: 0x06000162 RID: 354
	void OnGazeEnabled();

	// Token: 0x06000163 RID: 355
	void OnGazeDisabled();

	// Token: 0x06000164 RID: 356
	void OnGazeStart(Camera camera, GameObject targetObject, Vector3 intersectionPosition);

	// Token: 0x06000165 RID: 357
	void OnGazeStay(Camera camera, GameObject targetObject, Vector3 intersectionPosition);

	// Token: 0x06000166 RID: 358
	void OnGazeExit(Camera camera, GameObject targetObject);

	// Token: 0x06000167 RID: 359
	void OnGazeTriggerStart(Camera camera);

	// Token: 0x06000168 RID: 360
	void OnGazeTriggerEnd(Camera camera);
}
