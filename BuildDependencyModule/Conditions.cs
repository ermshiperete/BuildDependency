// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;

namespace BuildDependency
{
	[Flags]
	public enum Conditions
	{
		None = 0,
		Win32 = 1,
		Win64 = 2,
		Windows = Win32 | Win64,
		Linux32 = 4,
		Linux64 = 8,
		Linux = Linux32 | Linux64,
		Bit64 = Win64 | Linux64,
		Bit32 = Win32 | Linux32,
		All = Win32 | Win64 | Linux32 | Linux64
	}
}
