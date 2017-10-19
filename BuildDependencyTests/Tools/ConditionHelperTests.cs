// Copyright (c) 2015 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using NUnit.Framework;
using BuildDependency;
using BuildDependency.Tools;

namespace BuildDependencyTests.Tools
{
	[TestFixture]
	public class ConditionHelperTests
	{
		private static bool IsLinux => Environment.OSVersion.Platform == PlatformID.Unix;

		private static bool Is64Bit => Environment.Is64BitProcess;

		private static bool ExpectedResult(bool? expectLinux, bool? expect64Bit)
		{
			if (!expectLinux.HasValue && !expect64Bit.HasValue)
				return true;
			if (!expectLinux.HasValue)
				return Is64Bit ^ !expect64Bit.Value;
			if (!expect64Bit.HasValue)
				return IsLinux ^ !expectLinux.Value;
			return IsLinux ^ !expectLinux.Value && Is64Bit ^ !expect64Bit.Value;
		}

		private static bool ExpectedResultWithPlatform(string platform, bool? expectLinux, bool? expect64Bit)
		{
			if (!expectLinux.HasValue && !expect64Bit.HasValue)
				return true;
			if (!expectLinux.HasValue)
				return platform != "x86" ^ !expect64Bit.Value;
			if (!expect64Bit.HasValue)
				return IsLinux ^ !expectLinux.Value;
			return IsLinux ^ !expectLinux.Value && platform != "x86" ^ !expect64Bit.Value;
		}

		[TestCase(Conditions.All, null, null, TestName = "All is always true")]
		[TestCase(Conditions.None, null, null, TestName = "None is always true")]
		[TestCase(Conditions.Windows, false, null, TestName = "Windows")]
		[TestCase(Conditions.Win32, false, false, TestName = "Win32")]
		[TestCase(Conditions.Win64, false, true, TestName = "Win64")]
		[TestCase(Conditions.Linux, true, null, TestName = "Linux")]
		[TestCase(Conditions.Linux32, true, false, TestName = "Linux32")]
		[TestCase(Conditions.Linux64, true, true, TestName = "Linux64")]
		public void IsTrue(Conditions condition, bool? expectLinux, bool? expect64Bit)
		{
			Assert.That(condition.AreTrue(), Is.EqualTo(ExpectedResult(expectLinux, expect64Bit)));
		}

		[TestCase(Conditions.All, "x86", null, null, TestName = "All is always true (platform != null)")]
		[TestCase(Conditions.None, "x86",  null, null, TestName = "None is always true (platform != null)")]
		[TestCase(Conditions.All, "", null, null, TestName = "All is always true (platform == '')")]
		[TestCase(Conditions.None, "",  null, null, TestName = "None is always true (platform == '')")]
		[TestCase(Conditions.Windows, "x86", false, null, TestName = "Windows, x86")]
		[TestCase(Conditions.Win32, "x86", false, false, TestName = "Win32, x86")]
		[TestCase(Conditions.Win64, "x86", false, true, TestName = "Win64, x86")]
		[TestCase(Conditions.Linux, "x86", true, null, TestName = "Linux, x86")]
		[TestCase(Conditions.Linux32, "x86", true, false, TestName = "Linux32, x86")]
		[TestCase(Conditions.Linux64, "x86", true, true, TestName = "Linux64, x86")]

		[TestCase(Conditions.Windows, "x64", false, null, TestName = "Windows, x64")]
		[TestCase(Conditions.Win32, "x64", false, false, TestName = "Win32, x64")]
		[TestCase(Conditions.Win64, "x64", false, true, TestName = "Win64, x64")]
		[TestCase(Conditions.Linux, "x64", true, null, TestName = "Linux, x64")]
		[TestCase(Conditions.Linux32, "x64", true, false, TestName = "Linux32, x64")]
		[TestCase(Conditions.Linux64, "x64", true, true, TestName = "Linux64, x64")]

		[TestCase(Conditions.Windows, "AnyCPU", false, null, TestName = "Windows, AnyCPU")]
		[TestCase(Conditions.Win32, "AnyCPU", false, null, TestName = "Win32, AnyCPU")]
		[TestCase(Conditions.Win64, "AnyCPU", false, null, TestName = "Win64, AnyCPU")]
		[TestCase(Conditions.Linux, "AnyCPU", true, null, TestName = "Linux, AnyCPU")]
		[TestCase(Conditions.Linux32, "AnyCPU", true, null, TestName = "Linux32, AnyCPU")]
		[TestCase(Conditions.Linux64, "AnyCPU", true, null, TestName = "Linux64, AnyCPU")]
		public void IsTrueWithPlatformSpecified(Conditions condition, string platform,
			bool? expectLinux, bool? expect64Bit)
		{
			ConditionHelper.PlatformString = platform;
			Assert.That(condition.AreTrue(),
				Is.EqualTo(ExpectedResultWithPlatform(platform, expectLinux, expect64Bit)));
		}

	}
}

