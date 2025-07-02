using System.Collections;
using UnityEngine;

public class BlocksworldLoader : MonoBehaviour
{
	public void Start()
	{
		Object.DontDestroyOnLoad(this);
		StartCoroutine(LoadBlocksworldScene());
	}

	public IEnumerator LoadBlocksworldScene()
	{
		yield return Application.LoadLevelAsync("Scene");
		Object.Destroy(this);
	}
}
