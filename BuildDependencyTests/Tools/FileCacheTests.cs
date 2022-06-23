// Copyright (c) 2017 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.IO;
using System.Security.Cryptography;
using NUnit.Framework;

namespace BuildDependency
{
	[TestFixture]
	public class FileCacheTests
	{
		class DummyFileCache : FileCache.FileCacheImpl
		{
			public DummyFileCache(string baseDir = null)
			{
				CacheDir = baseDir ?? Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			}

			public void Shutdown()
			{
				if (Directory.Exists(CacheDir) && CacheDir != "/")
					Directory.Delete(CacheDir, true);
			}

			public string GetCachedFilePath(string url)
			{
				return GetFilePathInCache(url);
			}
		}

		private string _file;
		private DummyFileCache _fileCache;

		[SetUp]
		public void SetUp()
		{
		}

		[TearDown]
		public void TearDown()
		{
			if (File.Exists(_file))
				File.Delete(_file);
			_fileCache.Shutdown();
		}

		private string CreateTestFile()
		{
			_file = Path.GetTempFileName();
			File.WriteAllText(_file, "This is just a test file");
			return _file;
		}

		private void CreateFileCache(string baseDir = null)
		{
			_fileCache = new DummyFileCache(baseDir);
			FileCache.Instance = _fileCache;
		}

		[TestCase(null, ExpectedResult = true, Description = "Can write")]
		[TestCase("/", ExpectedResult = false, Description = "CacheDir exists, but we're not allowed to write")]
		[TestCase("/nonexistant", ExpectedResult = false, Description = "CacheDir doesn't exist, we can't write")]
		public bool Enabled(string baseDir)
		{
			// Setup
			CreateFileCache(baseDir);

			// Exercise
			return FileCache.Enabled;
		}

		[Test]
		public void CacheFile_CopiesFile()
		{
			// Setup
			var file = CreateTestFile();
			var url = "https://build.palaso.org/guestAuth/repository/download/Chorus/latest.lastSuccessful/MercExt/fixutf8.py";
			CreateFileCache();

			// Exercise
			Assert.That(FileCache.CacheFile(url, file), Is.True);

			// Verify
			var cachedFile = _fileCache.GetCachedFilePath(url);
			Assert.That(File.Exists(cachedFile), Is.True);
			Assert.That(File.ReadAllText(cachedFile), Is.EqualTo("This is just a test file"));
			Assert.That(File.GetLastWriteTimeUtc(cachedFile), Is.EqualTo(File.GetLastWriteTimeUtc(file)));
		}

		[Test]
		public void CacheFile_OverwritesExistingFile()
		{
			// Setup
			var file = CreateTestFile();
			var url = "https://build.palaso.org/guestAuth/repository/download/Chorus/latest.lastSuccessful/MercExt/fixutf8.py";
			CreateFileCache();
			_fileCache.CacheFile(url, file);
			File.Delete(_file);
			file = CreateTestFile();

			// Exercise
			Assert.That(FileCache.CacheFile(url, file), Is.True);

			// Verify
			var cachedFile = _fileCache.GetCachedFilePath(url);
			Assert.That(File.Exists(cachedFile), Is.True);
			Assert.That(File.ReadAllText(cachedFile), Is.EqualTo("This is just a test file"));
			Assert.That(File.GetLastWriteTimeUtc(cachedFile), Is.EqualTo(File.GetLastWriteTimeUtc(file)));
		}

		[Test]
		public void GetCachedFile_ReturnsNullIfNotCached()
		{
			// Setup
			CreateFileCache();
			var url = "https://build.palaso.org/nonexistant.txt";

			// Exercise/Verify
			Assert.That(FileCache.GetCachedFile(url), Is.Null);
		}

		[Test]
		public void GetCachedFile_ReturnsCachedFile()
		{
			// Setup
			var file = CreateTestFile();
			CreateFileCache();
			var url = "https://build.palaso.org/guestAuth/repository/download/Chorus/latest.lastSuccessful/MercExt/fixutf8.py";
			_fileCache.CacheFile(url, file);

			// Exercise/Verify
			Assert.That(FileCache.GetCachedFile(url), Is.Not.Null);
		}
	}
}
