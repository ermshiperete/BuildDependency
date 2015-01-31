// Copyright (c) 2015 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using BuildDependency;

namespace BuildDependency.Tools
{
	public static class ConditionHelper
	{
		public static bool IsTrue(Conditions condition)
		{
			if (condition == Conditions.None)
				return true;

			bool ret = true;
			if (Environment.OSVersion.Platform == PlatformID.Unix)
				ret &= ((condition & Conditions.Linux) != 0);
			else
				ret &= ((condition & Conditions.Windows) != 0);

			if (Environment.Is64BitOperatingSystem)
				ret &= ((condition & Conditions.Bit64) != 0);
			else
				ret &= ((condition & Conditions.Bit32) != 0);
			return ret;
		}
	}
}

