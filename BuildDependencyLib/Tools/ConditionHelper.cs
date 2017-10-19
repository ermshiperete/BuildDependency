// Copyright (c) 2015 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;

namespace BuildDependency.Tools
{
	public static class ConditionHelper
	{
		public static string PlatformString { get; set; }

		public static bool AreTrue(this Conditions condition)
		{
			if (condition == Conditions.None)
				return true;

			var ret = Platform.IsLinux ?
				(condition & Conditions.Linux) != 0 :
				(condition & Conditions.Windows) != 0;

			if (string.IsNullOrEmpty(PlatformString))
			{
				ret &= Environment.Is64BitProcess
					? (condition & Conditions.Bit64) != 0
					: (condition & Conditions.Bit32) != 0;
			}
			else
			{
				ret &= PlatformString == "AnyCPU" |
					(PlatformString == "x64"
					? (condition & Conditions.Bit64) != 0
					: (condition & Conditions.Bit32) != 0);
			}

			return ret;
		}
	}
}

