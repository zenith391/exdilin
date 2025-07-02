using UnityEngine;

public class DirectionAngleXZVisualizer : ParameterValueVisualizer
{
	protected static GameObject visualizerPrefab;

	public override void Update()
	{
		if (visualizerGo == null)
		{
			if (visualizerPrefab == null)
			{
				visualizerPrefab = Resources.Load("GUI/Direction Visualizer") as GameObject;
			}
			visualizerGo = Object.Instantiate(visualizerPrefab);
		}
		float angle = (float)Blocksworld.bw.tileParameterEditor.parameter.objectValue;
		Vector3 position = Blocksworld.selectedBlock.go.transform.position;
		Quaternion rotation = GetRotation(angle);
		visualizerGo.transform.position = position + Vector3.up * Util.MaxComponent(Blocksworld.selectedBlock.size);
		visualizerGo.transform.rotation = rotation;
		visualizerGo.transform.localScale = new Vector3(3f, 3f, 3f);
	}

	protected virtual Quaternion GetRotation(float angle)
	{
		return Quaternion.Euler(0f, angle + 180f, 0f);
	}
}
