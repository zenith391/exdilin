using UnityEngine;

public class RadialExplosionVisualizer : ParameterValueVisualizer
{
	protected static GameObject visualizerPrefab;

	private GameObject childGo;

	public override void Update()
	{
		if (visualizerGo == null)
		{
			if (visualizerPrefab == null)
			{
				visualizerPrefab = Resources.Load("GUI/Distance Visualizer") as GameObject;
			}
			visualizerGo = Object.Instantiate(visualizerPrefab);
			visualizerGo.GetComponent<Renderer>().material.color = Color.red;
			childGo = Object.Instantiate(visualizerPrefab);
			childGo.transform.parent = visualizerGo.transform;
		}
		float num = (float)Blocksworld.bw.tileParameterEditor.parameter.objectValue;
		Vector3 position = Blocksworld.selectedBlock.go.transform.position;
		visualizerGo.transform.position = position;
		visualizerGo.transform.localScale = Vector3.one * num * 2f;
		childGo.transform.localScale = Vector3.one * 2f;
	}
}
