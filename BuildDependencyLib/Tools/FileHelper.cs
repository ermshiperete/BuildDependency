// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace BuildDependency.Tools
{
	public class FileHelper
	{
		// inspired by http://stackoverflow.com/a/4375058

		private Regex _regex;
		private string _pattern;

		public FileHelper(string pattern, bool ignoreCase)
		{
			_regex = WildcardToRegex.Convert(pattern, ignoreCase);
			_pattern = pattern;
		}

		public bool IsFilenameMatch(string name)
		{
			return _regex.IsMatch(name);
		}

		public string GetPathBeforeWildcard(string filepath)
		{
			var wildcard = _pattern.IndexOfAny(new[] { '*', '?' });
			if (wildcard > -1)
			{
				var pathSep = _pattern.Substring(0, wildcard).LastIndexOf('/');
				if (pathSep > -1)
					return filepath.Substring(0, pathSep + 1);
			}
			return string.Empty;
		}

		public string GetPathStartingAtWildcard(string filepath)
		{
			var before = GetPathBeforeWildcard(filepath);
			if (string.IsNullOrEmpty(before))
				return filepath;
			return filepath.Substring(before.Length);
		}
	}
}

