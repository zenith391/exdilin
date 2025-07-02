using UnityEngine;

public class ForwardTriggerStay : MonoBehaviour
{
	private void OnTriggerStay(Collider collider)
	{
		TreasureHandler.ForwardTriggerStay(base.gameObject, collider);
	}
}
