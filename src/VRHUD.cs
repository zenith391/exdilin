using UnityEngine;

public class VRHUD : MonoBehaviour
{
	private void OnPreRender()
	{
		Blocksworld.blocksworldCamera.SetReticleCameraEyePosition(base.transform.localPosition.x);
	}
}
