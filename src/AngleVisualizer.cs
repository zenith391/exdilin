using UnityEngine;

public abstract class AngleVisualizer : ParameterValueVisualizer
{
	protected static GameObject visualizerPrefab;

	private GameObject movingArrow;

	protected float sign = 1f;

	public AngleVisualizer(float sign = 1f)
	{
		this.sign = sign;
	}

	public override void Update()
	{
		Transform transform = Blocksworld.selectedBlock.go.transform;
		if (visualizerGo == null)
		{
			if (visualizerPrefab == null)
			{
				visualizerPrefab = Resources.Load("GUI/Angle Visualizer") as GameObject;
			}
			visualizerGo = Object.Instantiate(visualizerPrefab);
			visualizerGo.transform.localScale = Vector3.one * 2f;
			movingArrow = visualizerGo.transform.Find("Arrow 1").gameObject;
			movingArrow.transform.Find("Head").gameObject.GetComponent<Renderer>().material.color = Color.red;
			movingArrow.transform.Find("Body").gameObject.GetComponent<Renderer>().material.color = Color.red;
			GameObject gameObject = visualizerGo.transform.Find("Arrow 2").gameObject;
			gameObject.transform.rotation = GetArrowRotation(0f, transform);
		}
		float angle = (float)Blocksworld.bw.tileParameterEditor.parameter.objectValue;
		Vector3 position = transform.position;
		visualizerGo.transform.position = position + Vector3.up * Util.MaxComponent(Blocksworld.selectedBlock.size);
		movingArrow.transform.rotation = GetArrowRotation(angle, transform);
	}

	protected abstract Quaternion GetArrowRotation(float angle, Transform blockT);
}
