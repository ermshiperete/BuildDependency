// Copyright (c) 2017 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Net;

namespace BuildDependency
{
	public static class Network
	{
		/// <summary>
		/// Returns <c>true</c> if files can be downloaded from the internet.
		/// </summary>
		/// <returns></returns>
		public static bool IsInternetAvailable()
		{
			// from https://stackoverflow.com/a/2031831
			try
			{
				using (var client = new WebClient())
				{
					using (client.OpenRead("http://www.gstatic.com/generate_204"))
					{
						return true;
					}
				}
			}
			catch
			{
				return false;
			}
		}

		public static bool WorkOffline { get; set; }

		public static bool IsOnline => !WorkOffline && IsInternetAvailable();
	}
}
