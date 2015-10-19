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

			bool ret = Environment.OSVersion.Platform == PlatformID.Unix ?
				((condition & Conditions.Linux) != 0) :
				((condition & Conditions.Windows) != 0);

			ret &= Environment.Is64BitOperatingSystem ?
				((condition & Conditions.Bit64) != 0) :
				((condition & Conditions.Bit32) != 0);

			return ret;
		}
	}
}

