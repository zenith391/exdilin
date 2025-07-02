using UnityEngine;

public class ParameterValueVisualizer
{
	protected GameObject visualizerGo;

	public virtual void Update()
	{
	}

	public virtual void Destroy()
	{
		if (visualizerGo != null)
		{
			Object.Destroy(visualizerGo);
			visualizerGo = null;
		}
	}
}
