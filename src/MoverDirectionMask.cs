using System;

[Flags]
public enum MoverDirectionMask
{
	NONE = 0,
	LEFT = 1,
	RIGHT = 2,
	UP = 4,
	DOWN = 8,
	ALL = 0xF
}
