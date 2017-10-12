// Copyright (c) 2017 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace BuildDependency
{
	public static class FileCache
	{
		static FileCache()
		{
			Instance = new FileCacheImpl();
		}

		public static FileCacheImpl Instance { get; set; }

		public static bool Enabled
		{
			get => Instance.Enabled;
			set => Instance.Enabled = value;
		}

		public static bool CacheFile(string url, string filePath)
		{
			return Instance.CacheFile(url, filePath);
		}

		public static string GetCachedFile(string url)
		{
			return Instance.GetCachedFile(url);
		}

		public class FileCacheImpl
		{
			protected string CacheDir { get; set; }
			private bool _enabled = true;
			private bool? _canCache;

			public FileCacheImpl()
			{
				CacheDir = Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
					"BuildDependency", "Cache");
			}

			protected string GetFilePathInCache(string url)
			{
				using (var sha1 = new SHA1Managed())
				{
					var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(url));
					var bldr = new StringBuilder();
					foreach (var b in hash)
					{
						bldr.Append(b.ToString("X2"));
					}
					var hashString = bldr.ToString();
					return Path.Combine(CacheDir, hashString.Substring(0, 2), hashString);
				}
			}

			public bool Enabled
			{
				get
				{
					if (!_enabled || _canCache.HasValue)
						return _enabled && _canCache.Value;

					if (Directory.Exists(CacheDir))
					{
						var probeFile = Path.Combine(CacheDir, "ProbeFile");
						try
						{
							File.WriteAllText(probeFile, "testing if we can create this file");
							_canCache = true;
						}
						catch (UnauthorizedAccessException)
						{
							_canCache = false;
						}
						finally
						{
							if (File.Exists(probeFile))
								File.Delete(probeFile);
						}
					}
					else
					{
						try
						{
							Directory.CreateDirectory(CacheDir);
							_canCache = true;
						}
						catch (UnauthorizedAccessException)
						{
							_canCache = false;
						}
					}
					return _canCache.Value;
				}
				set => _enabled = value;
			}

			public bool CacheFile(string url, string filePath)
			{
				var dest = GetFilePathInCache(url);
				Directory.CreateDirectory(Path.GetDirectoryName(dest));
				File.Copy(filePath, dest, true);
				return true;
			}

			public string GetCachedFile(string url)
			{
				var cachedFile = GetFilePathInCache(url);
				return File.Exists(cachedFile) ? cachedFile : null;
			}
		}
	}
}
