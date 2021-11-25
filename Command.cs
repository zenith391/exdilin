using System;
using System.Collections.Generic;

// Token: 0x02000124 RID: 292
public abstract class Command
{
	// Token: 0x060013F3 RID: 5107 RVA: 0x0008B457 File Offset: 0x00089857
	public virtual void Execute()
	{
		this.done = true;
	}

	// Token: 0x060013F4 RID: 5108 RVA: 0x0008B460 File Offset: 0x00089860
	public virtual void OnHudMesh()
	{
	}

	// Token: 0x060013F5 RID: 5109 RVA: 0x0008B462 File Offset: 0x00089862
	public virtual bool IsDone()
	{
		return this.done;
	}

	// Token: 0x060013F6 RID: 5110 RVA: 0x0008B46A File Offset: 0x0008986A
	public void SetDone(bool d)
	{
		this.done = d;
	}

	// Token: 0x060013F7 RID: 5111 RVA: 0x0008B473 File Offset: 0x00089873
	public virtual void Reset()
	{
		this.done = false;
	}

	// Token: 0x060013F8 RID: 5112 RVA: 0x0008B47C File Offset: 0x0008987C
	public virtual void Removed()
	{
	}

	// Token: 0x060013F9 RID: 5113 RVA: 0x0008B480 File Offset: 0x00089880
	public static void ExecuteCommands(List<Command> commands)
	{
		if (commands.Count > 0)
		{
			for (int i = 0; i < commands.Count; i++)
			{
				Command command = commands[i];
				command.Execute();
			}
			for (int j = commands.Count - 1; j >= 0; j--)
			{
				Command command2 = commands[j];
				if (command2.IsDone())
				{
					command2.Removed();
					commands.RemoveAt(j);
				}
			}
		}
	}

	// Token: 0x060013FA RID: 5114 RVA: 0x0008B4F8 File Offset: 0x000898F8
	public static void RemoveCommands(List<Command> commands)
	{
		foreach (Command command in commands)
		{
			command.Removed();
		}
		commands.Clear();
	}

	// Token: 0x060013FB RID: 5115 RVA: 0x0008B554 File Offset: 0x00089954
	public static void AddUniqueCommand(List<Command> commands, Command command, bool resetWhenAdded = true)
	{
		if (!commands.Contains(command))
		{
			commands.Add(command);
			if (resetWhenAdded)
			{
				command.Reset();
			}
		}
	}

	// Token: 0x04000F9F RID: 3999
	protected bool done;
}
