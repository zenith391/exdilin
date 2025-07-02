using UnityEngine;

public class DistanceVisualizer : ParameterValueVisualizer
{
	protected static GameObject visualizerPrefab;

	public Color color = Color.white;

	public override void Update()
	{
		if (visualizerGo == null)
		{
			if (visualizerPrefab == null)
			{
				visualizerPrefab = Resources.Load("GUI/Distance Visualizer") as GameObject;
			}
			visualizerGo = Object.Instantiate(visualizerPrefab);
			visualizerGo.GetComponent<Renderer>().material.color = color;
		}
		float num = (float)Blocksworld.bw.tileParameterEditor.parameter.objectValue;
		Vector3 position = Blocksworld.selectedBlock.go.transform.position;
		visualizerGo.transform.position = position;
		visualizerGo.transform.localScale = Vector3.one * num * 2f;
	}
}
