using System;

public class DelayedDelegateCommand<T> : DelayedCommand
{
	private Action<T> action;

	private T data;

	public DelayedDelegateCommand(T data, Action<T> action, int delay)
		: base(delay)
	{
		this.action = action;
		this.data = data;
	}

	protected override void DelayedExecute()
	{
		base.DelayedExecute();
		action(data);
	}
}
public class DelayedDelegateCommand<S, T> : DelayedCommand
{
	private Action<S, T> action;

	private S data1;

	private T data2;

	public DelayedDelegateCommand(S data1, T data2, Action<S, T> action, int delay)
		: base(delay)
	{
		this.action = action;
		this.data1 = data1;
		this.data2 = data2;
	}

	protected override void DelayedExecute()
	{
		base.DelayedExecute();
		action(data1, data2);
	}
}
public class DelayedDelegateCommand<S, T, U, V> : DelayedCommand
{
	private Action<S, T, U, V> action;

	private S data1;

	private T data2;

	private U data3;

	private V data4;

	public DelayedDelegateCommand(S data1, T data2, U data3, V data4, Action<S, T, U, V> action, int delay)
		: base(delay)
	{
		this.action = action;
		this.data1 = data1;
		this.data2 = data2;
		this.data3 = data3;
		this.data4 = data4;
	}

	protected override void DelayedExecute()
	{
		base.DelayedExecute();
		action(data1, data2, data3, data4);
	}
}
public class DelayedDelegateCommand : DelayedCommand
{
	private Action action;

	public DelayedDelegateCommand(Action action, int delay)
		: base(delay)
	{
		this.action = action;
	}

	protected override void DelayedExecute()
	{
		base.DelayedExecute();
		action();
	}
}
