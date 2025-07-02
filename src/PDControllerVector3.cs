using UnityEngine;

public class PDControllerVector3
{
	private Vector3 lastError;

	private float p;

	private float d;

	public PDControllerVector3(float p, float d)
	{
		this.p = p;
		this.d = d;
	}

	public Vector3 Update(Vector3 currentError, float deltaTime)
	{
		Vector3 vector = (currentError - lastError) / deltaTime;
		lastError = currentError;
		return currentError * p + vector * d;
	}

	public void Reset()
	{
		lastError = Vector3.zero;
	}
}
