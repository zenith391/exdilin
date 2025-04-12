using System;
using System.Collections.Generic;

// Token: 0x02000406 RID: 1030
public static class MenuInputHandler
{
	// Token: 0x17000246 RID: 582
	// (get) Token: 0x06002D10 RID: 11536 RVA: 0x0014242F File Offset: 0x0014082F
	public static IMenuInputHandler currentInputHandler
	{
		get
		{
			if (MenuInputHandler.inputHandlerStack == null || MenuInputHandler.inputHandlerStack.Count == 0)
			{
				return null;
			}
			return MenuInputHandler.inputHandlerStack[0];
		}
	}

	// Token: 0x06002D11 RID: 11537 RVA: 0x00142457 File Offset: 0x00140857
	public static void HandleInput()
	{
		if (MenuInputHandler.currentInputHandler != null)
		{
			MenuInputHandler.currentInputHandler.HandleMenuInputEvents();
		}
	}

	// Token: 0x06002D12 RID: 11538 RVA: 0x0014246D File Offset: 0x0014086D
	public static void Clear()
	{
		MenuInputHandler.inputHandlerStack = new List<IMenuInputHandler>();
	}

	// Token: 0x06002D13 RID: 11539 RVA: 0x00142479 File Offset: 0x00140879
	public static void RequestControl(IMenuInputHandler handler)
	{
		if (MenuInputHandler.inputHandlerStack == null)
		{
			MenuInputHandler.inputHandlerStack = new List<IMenuInputHandler>();
		}
		MenuInputHandler.inputHandlerStack.Insert(0, handler);
	}

	// Token: 0x06002D14 RID: 11540 RVA: 0x0014249B File Offset: 0x0014089B
	public static void Release(IMenuInputHandler handler)
	{
		if (MenuInputHandler.inputHandlerStack == null)
		{
			return;
		}
		MenuInputHandler.inputHandlerStack.Remove(handler);
	}

	// Token: 0x04002595 RID: 9621
	private static List<IMenuInputHandler> inputHandlerStack;
}
