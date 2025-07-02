using UnityEngine;

public class NamedPose
{
	public string name = string.Empty;

	public Vector3 position;

	public Vector3 direction;

	public NamedPose(string name, Vector3 position, Vector3 direction)
	{
		this.name = name;
		this.position = position;
		this.direction = direction;
	}
}
