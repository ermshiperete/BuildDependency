// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using NUnit.Framework;
using BuildDependency.Artifacts;

namespace BuildDependencyTests
{
	[TestFixture]
	public class ArtifactRuleTests
	{
		[TestCase("a/b/**=>lib", true, "a/b/**", "", "lib")]
		[TestCase("a/b/** => lib", true, "a/b/**", "", "lib")]
		[TestCase("*/.txt=>lib", true, "*/.txt", "", "lib")]
		[TestCase("release-*.zip!*.dll=>dlls", true, "release-*.zip", "*.dll", "dlls")]
		[TestCase("a.zip!**=>destination", true, "a.zip", "**", "destination")]
		[TestCase("a.zip!a/b/c/*/.dll=>dlls", true, "a.zip", "a/b/c/*/.dll", "dlls")]
		[TestCase("-:bad/exclude.txt", false, "bad/exclude.txt", "", "")]
		[TestCase("+:release-*.zip!**/*.dll=>dlls", true, "release-*.zip", "**/*.dll", "dlls")]
		[TestCase("-:release-0.0.1.zip!Bad.dll", false, "release-0.0.1.zip", "Bad.dll", "")]
		public void ParseRule(string rule, bool include, string sourcePath, string archivePath, string dest)
		{
			var artifactRule = new ArtifactRule(0, null, rule);
			Assert.That(artifactRule.Include, Is.EqualTo(include), "Include");
			Assert.That(artifactRule.SourcePath, Is.EqualTo(sourcePath), "SourcePath");
			Assert.That(artifactRule.ArchivePath, Is.EqualTo(archivePath), "ArchivePath");
			Assert.That(artifactRule.DestinationPath, Is.EqualTo(dest), "DestinationPath");
		}

		[Test]
		[TestCase("*.txt=>lib", "bla.txt", ExpectedResult = true)]
		[TestCase("?.txt=>lib", "bla.txt", ExpectedResult = false)]
		[TestCase("???.txt=>lib", "bla.txt", ExpectedResult = true)]
		[TestCase("*.txt=>lib", "bla/bla.txt", ExpectedResult = false)]
		[TestCase("**.txt=>lib", "bla/bla.txt", ExpectedResult = true)]
		[TestCase("*.txt=>lib", "blatxt", ExpectedResult = false)]
		[TestCase("*/.txt=>lib", ".txt", ExpectedResult = false)]
		[TestCase("*/.txt=>lib", "a/bla.txt", ExpectedResult = false)]
		[TestCase("*/.txt=>lib", "a/.txt", ExpectedResult = true)]
		[TestCase("**=>lib", "a/b/c/def", ExpectedResult = true)]
		[TestCase("a/**=>lib", "a/b/c/def", ExpectedResult = true)]
		[TestCase("a/**=>lib", "b/c/def", ExpectedResult = false)]
		[TestCase("a/**/*.txt=>lib", "a/b/c/def", ExpectedResult = false)]
		[TestCase("a/**/*.txt=>lib", "a/b/c/def.txt", ExpectedResult = true)]
		[TestCase("a/**/*.txt=>lib", "a/b/c/d/def.txt", ExpectedResult = true)]
		[TestCase("a/*/*.txt=>lib", "a/b/c/d/def.txt", ExpectedResult = false)]
		[TestCase("a/b/c/**/.dll=>dlls", "a/b/c/d/e/.dll", ExpectedResult = true)]
		[TestCase("a/b/c/**/.dll=>dlls", "a/b/c/d/e/f.dll", ExpectedResult = false)]
		[TestCase("a/b/c/**/*.dll=>dlls", "f.dll", ExpectedResult = false)]
		[TestCase("-:*.txt=>lib", "bla.txt", ExpectedResult = false)]
		[TestCase("+:*.txt=>lib", "bla.txt", ExpectedResult = true)]
		[TestCase("a/b/c/**d/*.dll=>dlls", "a/b/c/d/g.dll", ExpectedResult = true)]
		public bool FileIsMatch(string rule, string fileName)
		{
			var artifactRule = new ArtifactRule(0, null, rule);
			return artifactRule.IsMatch(fileName);
		}

		[TestCase("*.txt=>lib", "bla.txt", ExpectedResult = "lib/bla.txt")]
		[TestCase("**.txt=>lib", "bla/bla.txt", ExpectedResult = "lib/bla/bla.txt")]
		[TestCase("a/b/**=>lib", "a/b/c/file.txt", ExpectedResult = "lib/c/file.txt")]
		[TestCase("a/b/c/*/.dll=>dlls", "a/b/c/d/.dll", ExpectedResult = "dlls/d/.dll")]
		[TestCase("a/b/c/**/.dll=>dlls", "a/b/c/d/e/f/.dll", ExpectedResult = "dlls/d/e/f/.dll")]
		[TestCase("a/b/c/**/*.dll=>dlls", "a/b/c/d/e/f/g.dll", ExpectedResult = "dlls/d/e/f/g.dll")]
		[TestCase("a/b/c/**d/*.dll=>dlls", "a/b/c/d/g.dll", ExpectedResult = "dlls/d/g.dll")]
		[TestCase("a/b/c/**d/*.dll=>dlls", "a/b/c/e/f/d/g.dll", ExpectedResult = "dlls/e/f/d/g.dll")]
		[TestCase("*.txt=>lib", "blatxt", ExpectedResult = null)]
		[TestCase("abc.dll*=>lib", "abc.dll.config", ExpectedResult = "lib/abc.dll.config")]
		[TestCase("**", "README", ExpectedResult = "README")]
		public string GetTarget(string rule, string fileName)
		{
			var artifactRule = new ArtifactRule(0, null, rule);
			return NormalizedPath(artifactRule.GetTarget(fileName));
		}

		[TestCase("*.txt=>lib", "bla.txt", ExpectedResult = "lib/bla.txt")]
		[TestCase("**", "README", ExpectedResult = "README")]
		public string GetJobs_DownloadFile(string rule, string file)
		{
			var artifactRule = new ArtifactRule(0, "http://example.com/download", rule);
			var jobs = artifactRule.GetJobs(file);

			Assert.That(jobs.Count, Is.EqualTo(1));
			Assert.That(jobs[0], Is.TypeOf<DownloadFileJob>());
			var downloadJob = jobs[0] as DownloadFileJob;
			Assert.That(downloadJob.Url, Is.EqualTo("http://example.com/download/" + file));
			return NormalizedPath(downloadJob.TargetFile);
		}

		[Test]
		public void GetJobs_ZipFile()
		{
			var artifactRule = new ArtifactRule(0, "http://example.com/download", "a.zip!a/b/c/*/.dll=>dlls");
			var jobs = artifactRule.GetJobs("a.zip");

			Assert.That(jobs.Count, Is.EqualTo(2));
			Assert.That(jobs[0], Is.TypeOf<DownloadZipJob>());
			var downloadJob = jobs[0] as DownloadZipJob;
			Assert.That(downloadJob.Url, Is.EqualTo("http://example.com/download/a.zip"));
			Assert.That(downloadJob.TargetFile, Is.EqualTo(Path("Downloads/a.zip")));
			Assert.That(jobs[1], Is.TypeOf<UnzipFilesJob>());
			var unzipJob = jobs[1] as UnzipFilesJob;
			Assert.That(unzipJob.ZipFile, Is.EqualTo(Path("Downloads/a.zip")));
			Assert.That(unzipJob.SourcePath, Is.EqualTo("a/b/c/*/.dll"));
			Assert.That(unzipJob.DestinationPath, Is.EqualTo("dlls"));
		}

		private static string Path(string inPath)
		{
			return inPath.Replace('/', System.IO.Path.DirectorySeparatorChar);
		}

		private static string NormalizedPath(string inPath)
		{
			return inPath?.Replace('\\', '/');
		}
	}
}

