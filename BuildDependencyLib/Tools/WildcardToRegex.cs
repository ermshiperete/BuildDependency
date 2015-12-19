// Copyright (c) 2015 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Text.RegularExpressions;

namespace BuildDependency.Tools
{
	public static class WildcardToRegex
	{
		private static readonly Regex HasQuestionMarkRegEx = new Regex(@"\?", RegexOptions.Compiled);
		private static readonly Regex IllegalCharactersRegex = new Regex("[" + @"\:<>|" + "\"]", RegexOptions.Compiled);
		private static readonly Regex CatchExtensionRegex = new Regex(@"^\s*.+\.([^\.]+)\s*$", RegexOptions.Compiled);
		private const string NonDotCharacters = @"[^.]*";
		private const string SingleStar = @"[^/\\]*";
		private const string DoubleStar = @".*";

		public static string FileFilter(string pattern)
		{
			if (pattern.Contains("/"))
			{
				pattern = pattern.Substring(pattern.LastIndexOf("/", StringComparison.InvariantCulture) + 1);
			}
			return ConvertString(pattern);
		}

		public static string DirectoryFilter(string pattern)
		{
			if (pattern.Contains("/"))
			{
				pattern = pattern.Substring(0, pattern.LastIndexOf("/", StringComparison.InvariantCulture));
			}
			return ConvertString(pattern);
		}

		public static string ConvertString(string pattern)
		{
			if (pattern == null)
			{
				throw new ArgumentNullException();
			}
			pattern = pattern.Trim();
			if (pattern.Length == 0)
				return pattern;
			if (IllegalCharactersRegex.IsMatch(pattern))
				throw new ArgumentException("Patterns contains ilegal characters.");

			bool hasExtension = CatchExtensionRegex.IsMatch(pattern);
			bool matchExact = false;
			if (HasQuestionMarkRegEx.IsMatch(pattern))
			{
				matchExact = true;
			}
			else if (hasExtension)
			{
				matchExact = CatchExtensionRegex.Match(pattern).Groups[1].Length != 3;
			}
			string regexString = Regex.Escape(pattern);
			regexString = "^" + Regex.Replace(Regex.Replace(regexString, @"\\\*\\\*", DoubleStar), @"\\\*", SingleStar);
			regexString = Regex.Replace(regexString, @"\\\?", ".");
//			if (!matchExact && hasExtension)
//			{
//				regexString += NonDotCharacters;
//			}
			regexString += "$";
			return regexString;
		}

		public static Regex Convert(string pattern, bool ignoreCase)
		{
			var regexString = ConvertString(pattern);
			var options = RegexOptions.Compiled;
			if (ignoreCase)
				options |= RegexOptions.IgnoreCase;
			var regex = new Regex(regexString, options);
			return regex;
		}
	}
}

