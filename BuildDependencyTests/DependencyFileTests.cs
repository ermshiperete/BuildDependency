// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using System.IO;
using System.Linq;
using BuildDependency;
using BuildDependency.Artifacts;
using NUnit.Framework;

namespace BuildDependencyTests
{
	[TestFixture]
	public class DependencyFileTests
	{
		private string _tempFile;

		[SetUp]
		public void SetUp()
		{
			_tempFile = Path.GetTempFileName();
		}

		[TearDown]
		public void TearDown()
		{
			File.Delete(_tempFile);
		}

		[Test]
		public void LoadFile_TwoLineDownload()
		{
			// Setup
			File.WriteAllText(_tempFile, @"[[TC]]
Type=TeamCity
Url=https://build.palaso.org

[TC::Chorus_Linux64masterContinuous]
RevisionValue=latest.lastSuccessful
Path=LibChorus.dll=>lib/
Chorus.exe=>lib/
");

			// Execute
			var result = DependencyFile.LoadFile(_tempFile, new NullLog());

			// Verify
			Assert.That(result.Count, Is.EqualTo(1));
			var jobs = result[0].GetJobs().Result;
			Assert.That(jobs.Count, Is.EqualTo(2));
			Assert.That(jobs, Is.All.TypeOf<DownloadFileJob>());
			Assert.That(jobs.Select(job => ((DownloadFileJob) job).SourceFile),
				Is.EquivalentTo(new[] { "LibChorus.dll", "Chorus.exe" } ));
		}

		[Test]
		public void LoadFile_TwoLineDownload_NoNewLineAtEnd()
		{
			// Setup
			File.WriteAllText(_tempFile, @"[[TC]]
Type=TeamCity
Url=https://build.palaso.org

[TC::Chorus_Linux64masterContinuous]
RevisionValue=latest.lastSuccessful
Path=LibChorus.dll=>lib/
Chorus.exe=>lib/");

			// Execute
			var result = DependencyFile.LoadFile(_tempFile, new NullLog());

			// Verify
			Assert.That(result.Count, Is.EqualTo(1));
			var jobs = result[0].GetJobs().Result;
			Assert.That(jobs.Count, Is.EqualTo(2));
			Assert.That(jobs, Is.All.TypeOf<DownloadFileJob>());
			Assert.That(jobs.Select(job => ((DownloadFileJob) job).SourceFile),
				Is.EquivalentTo(new[] { "LibChorus.dll", "Chorus.exe" } ));
		}

		[Test]
		public void LoadFile_TwoLineDownload_CommaSeparated()
		{
			// Setup
			File.WriteAllText(_tempFile, @"[[TC]]
Type=TeamCity
Url=https://build.palaso.org

[TC::Chorus_Linux64masterContinuous]
RevisionValue=latest.lastSuccessful
Path=LibChorus.dll=>lib/,Chorus.exe=>lib/
");

			// Execute
			var result = DependencyFile.LoadFile(_tempFile, new NullLog());

			// Verify
			Assert.That(result.Count, Is.EqualTo(1));
			var jobs = result[0].GetJobs().Result;
			Assert.That(jobs.Count, Is.EqualTo(2));
			Assert.That(jobs, Is.All.TypeOf<DownloadFileJob>());
			Assert.That(jobs.Select(job => ((DownloadFileJob) job).SourceFile),
				Is.EquivalentTo(new[] { "LibChorus.dll", "Chorus.exe" } ));
		}

		[Test]
		public void LoadFile_NoRevisionValue()
		{
			// Setup
			File.WriteAllText(_tempFile, @"[[TC]]
Type=TeamCity
Url=https://build.palaso.org

[TC::Chorus_Linux64masterContinuous]
Path=LibChorus.dll=>lib/
");

			// Execute
			var result = DependencyFile.LoadFile(_tempFile, new NullLog());

			// Verify
			Assert.That(result.Count, Is.EqualTo(1));
			var jobs = result[0].GetJobs().Result;
			Assert.That(jobs.Count, Is.EqualTo(1));
			Assert.That(jobs, Is.All.TypeOf<DownloadFileJob>());
			Assert.That(jobs.Select(job => ((DownloadFileJob) job).SourceFile),
				Is.EquivalentTo(new[] { "LibChorus.dll" } ));
		}

	}
}
