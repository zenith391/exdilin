using UnityEngine;

public class UILoadingSpinner : MonoBehaviour
{
	private RectTransform ourSpinner;

	private float waitTime;

	private void Start()
	{
		ourSpinner = base.gameObject.GetComponent<RectTransform>();
	}

	private void Update()
	{
		if (base.gameObject.activeSelf)
		{
			waitTime += Time.deltaTime;
			if (waitTime > 0.1f)
			{
				ourSpinner.Rotate(0f, 0f, -30f);
				waitTime = 0f;
			}
		}
	}
}
