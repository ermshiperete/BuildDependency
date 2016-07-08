// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Reflection;

namespace BuildDependency.Tools
{
	public static class Utils
	{
		public static Tuple<object, string> GetVersion(string ns)
		{
			// Access fields in GitVersionInformation that GitVersion creates at compile time
			var gitVersionInformationType = Assembly.GetCallingAssembly()
				.GetType(ns + ".GitVersionInformation");
			if (gitVersionInformationType == null)
				return new Tuple<object, string>(null, string.Empty);
			var fullSemVer = gitVersionInformationType.GetField("FullSemVer");
			var sha = gitVersionInformationType.GetField("Sha");
			return new Tuple<object, string>(fullSemVer.GetValue(null),
				sha.GetValue(null).ToString().Substring(0, 7));
		}

	}
}

