using System.Collections.Generic;

public static class MenuInputHandler
{
	private static List<IMenuInputHandler> inputHandlerStack;

	public static IMenuInputHandler currentInputHandler
	{
		get
		{
			if (inputHandlerStack == null || inputHandlerStack.Count == 0)
			{
				return null;
			}
			return inputHandlerStack[0];
		}
	}

	public static void HandleInput()
	{
		if (currentInputHandler != null)
		{
			currentInputHandler.HandleMenuInputEvents();
		}
	}

	public static void Clear()
	{
		inputHandlerStack = new List<IMenuInputHandler>();
	}

	public static void RequestControl(IMenuInputHandler handler)
	{
		if (inputHandlerStack == null)
		{
			inputHandlerStack = new List<IMenuInputHandler>();
		}
		inputHandlerStack.Insert(0, handler);
	}

	public static void Release(IMenuInputHandler handler)
	{
		if (inputHandlerStack != null)
		{
			inputHandlerStack.Remove(handler);
		}
	}
}
