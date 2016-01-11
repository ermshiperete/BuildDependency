// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using NUnit.Framework;
using BuildDependency.TeamCity.RestClasses;

namespace BuildDependencyTests
{
	[TestFixture]
	public class BuildTypeTests
	{
		[TestCase(null, ExpectedResult = null,
			TestName = "null")]
		[TestCase("http://build.example.com/viewType.html", ExpectedResult = null,
			TestName = "No buildTypeId")]
		[TestCase("http://build.example.com/viewType.html?buildTypeId=bt392", ExpectedResult = "bt392",
			TestName = "Numeric ID")]
		[TestCase("http://build.example.com/viewType.html?buildTypeId=Project_DummyBuild",
			ExpectedResult = "Project_DummyBuild", TestName = "String ID")]
		public string IdForArtifacts(string webUrl)
		{
			var sut = new BuildType();
			sut.Id = "bt123";
			sut.WebUrl = webUrl;

			return sut.IdForArtifacts;
		}

	}
}

