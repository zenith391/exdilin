using UnityEngine;

namespace Gestures;

public class Touch
{
	public int fingerId;

	public TouchPhase Phase;

	public Vector2 Position;

	public Vector2 LastPosition;

	public GestureCommand Command;

	public int moveFrameCount;

	public Touch(Vector2 initialPosition)
	{
		LastPosition = initialPosition;
		Position = initialPosition;
		Phase = TouchPhase.Began;
		moveFrameCount = 0;
	}

	public void Moved(Vector2 newPosition)
	{
		LastPosition = Position;
		Position = newPosition;
		Phase = ((!(Position == LastPosition)) ? TouchPhase.Moved : TouchPhase.Stationary);
		moveFrameCount++;
	}

	public void End()
	{
		LastPosition = Position;
		Phase = TouchPhase.Ended;
		moveFrameCount = 0;
	}
}
