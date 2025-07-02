using UnityEngine;

namespace Blocks;

public class FootInfo
{
	public GameObject go;

	public Rigidbody rb;

	public Collider collider;

	public Vector3 position;

	public Vector3 normal;

	public Transform bone;

	public float boneYOffset;

	public SpringJoint joint;

	public Mesh ankleMesh;

	public string oldName;

	public Vector3 pausedVelocity;

	public Vector3 pausedAngularVelocity;

	public Vector3 origLocalPosition;

	public Quaternion origLocalRotation;

	public Vector3[] ankleMeshOrigVertices;
}
