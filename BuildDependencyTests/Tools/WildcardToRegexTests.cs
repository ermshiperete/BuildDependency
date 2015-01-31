// Copyright (c) 2015 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using NUnit.Framework;
using BuildDependencyManager.Tools;

namespace BuildDependencyTests.Tools
{
	[TestFixture]
	public class WildcardToRegexTests
	{
		[Test]
		[TestCase("**", "^.*$", TestName = "All Wildcard")]
		[TestCase("**/*.txt", "^.*/[^/\\\\]*\\.txt$", TestName = "Contains Subdirectory")]
		[TestCase("**/*.txt*", "^.*/[^/\\\\]*\\.txt[^/\\\\]*$", TestName = "Extension ending in wildcard")]
		public void ConvertString(string wildcard, string regex)
		{
			Assert.That(WildcardToRegex.ConvertString(wildcard), Is.EqualTo(regex));
		}

		[Test]
		[TestCase("**", "^.*$", TestName = "All Wildcard Filename")]
		[TestCase("**/*.txt", "^[^/\\\\]*\\.txt$", TestName = "Contains Subdirectory with Filename")]
		[TestCase("A/**/*.txt", "^[^/\\\\]*\\.txt$", TestName = "Contains specific Subdirectory with Filename")]
		public void FileFilter(string wildcard, string fileFilter)
		{
			Assert.That(WildcardToRegex.FileFilter(wildcard), Is.EqualTo(fileFilter));
		}

		[Test]
		[TestCase("**", "^.*$", TestName = "All Wildcard Directory")]
		[TestCase("**/*.txt", "^.*$", TestName = "Contains Directory and Filename")]
		[TestCase("A/**/*.txt", "^A/.*$", TestName = "Contains specific Directory and Filename")]
		public void DirectoryFilter(string wildcard, string fileFilter)
		{
			Assert.That(WildcardToRegex.DirectoryFilter(wildcard), Is.EqualTo(fileFilter));
		}
	}
}

