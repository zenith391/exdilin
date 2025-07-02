public class TutorialOperationPose
{
	public TutorialState state;

	public int meshIndex;

	public string poseName;

	public override string ToString()
	{
		return $"[TutorialOperationPose {state} {meshIndex} {poseName}]";
	}
}
