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
		private static bool IsLinux
		{
			get { return Environment.OSVersion.Platform == PlatformID.Unix; }
		}

		private static bool Is64bit
		{
			get { return Environment.Is64BitProcess; }
		}

		private static bool ExpectedResult(bool? expectLinux, bool? expect64bit)
		{
			if (!expectLinux.HasValue && !expect64bit.HasValue)
				return true;
			if (!expectLinux.HasValue)
				return Environment.Is64BitProcess ^ !expect64bit.Value;
			if (!expect64bit.HasValue)
				return IsLinux ^ !expectLinux.Value;
			return IsLinux ^ !expectLinux.Value && Environment.Is64BitProcess ^ !expect64bit.Value;
		}

		[Test]
		[TestCase(Conditions.All, null, null, TestName = "All is always true")]
		[TestCase(Conditions.None, null, null, TestName = "None is always true")]
		[TestCase(Conditions.Windows, false, null, TestName = "Windows")]
		[TestCase(Conditions.Win32, false, false, TestName = "Win32")]
		[TestCase(Conditions.Win64, false, true, TestName = "Win64")]
		[TestCase(Conditions.Linux, true, null, TestName = "Linux")]
		[TestCase(Conditions.Linux32, true, false, TestName = "Linux32")]
		[TestCase(Conditions.Linux64, true, true, TestName = "Linux64")]
		public void IsTrue(Conditions condition, bool? expectLinux, bool? expect64bit)
		{
			Assert.That(condition.AreTrue(), Is.EqualTo(ExpectedResult(expectLinux, expect64bit)));
		}


	}
}

