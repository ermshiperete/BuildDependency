// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;

namespace BuildDependency.Tools
{
	public static class FileHelper
	{
		// inspired by http://stackoverflow.com/a/4375058

		public static bool FilenameMatch(string pattern, string name, bool ignoreCase)
		{
			var regex = WildcardToRegex.Convert(pattern, ignoreCase);
			return regex.IsMatch(name);
		}

	}
}

