// Copyright (c) 2017 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using NUnit.Framework;

namespace BuildDependency
{
	[TestFixture]
	public class ServerTests
	{
		[TestCase("https://build.example.org", ExpectedResult = true)]
		[TestCase("http://build.example.org", ExpectedResult = true)]
		[TestCase("https://build.example.org:123", ExpectedResult = true)]
		[TestCase("https://test.example.org", ExpectedResult = false)]
		public bool Equals(string otherUrl)
		{
			// Setup
			var other = Server.CreateServer(ServerType.TeamCity);
			other.Url = otherUrl;
			var sut = Server.CreateServer(ServerType.TeamCity);
			sut.Url = "https://build.example.org";

			// Exercise
			return sut.Equals(other);
		}
	}
}
