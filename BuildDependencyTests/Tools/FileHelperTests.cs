// Copyright (c) 2015 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using NUnit.Framework;
using BuildDependency.Tools;

namespace BuildDependencyTests.Tools
{
	[TestFixture]
	public class FileHelperTests
	{
		[Test]
		[TestCase("b.txt", "b.txt", "")]
		[TestCase("*.txt", "a/b.txt", "")]
		[TestCase("a*.txt", "ab.txt", "")]
		[TestCase("a/*.txt", "a/b.txt", "a/")]
		[TestCase("a/**/b*.txt", "a/b/c/bd.txt", "a/")]
		[TestCase("a/a**/b*.txt", "a/ab/c/bd.txt", "a/")]
		public void PathBeforeWildcard(string pattern, string filename, string expectedPath)
		{
			var fileHelper = new FileHelper(pattern, false);
			Assert.That(fileHelper.GetPathBeforeWildcard(filename), Is.EqualTo(expectedPath));
		}

		[Test]
		[TestCase("b.txt", "b.txt", "b.txt")]
		[TestCase("**.txt", "a/b.txt", "a/b.txt")]
		[TestCase("a*.txt", "ab.txt", "ab.txt")]
		[TestCase("a/*.txt", "a/b.txt", "b.txt")]
		[TestCase("a/**/b*.txt", "a/b/c/bd.txt", "b/c/bd.txt")]
		[TestCase("a/a**/b*.txt", "a/ab/c/bd.txt", "ab/c/bd.txt")]
		public void PathStartingAtWildcard(string pattern, string filename, string expectedPath)
		{
			var fileHelper = new FileHelper(pattern, false);
			Assert.That(fileHelper.GetPathStartingAtWildcard(filename), Is.EqualTo(expectedPath));
		}
	}
}