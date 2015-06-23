// Copyright (c) 2015 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using BuildDependency.Interfaces;

namespace BuildDependency.Jenkins.RestClasses
{
	class JenkinsJob : IBuildJob
	{
		// <job><name>Bloom-Linux-any-master--JSTests</name>
		// <url>https://jenkins.lsdev.sil.org/job/Bloom-Linux-any-master--JSTests/</url><color>blue</color></job>
		public string Name { get; set; }
		public string Url { get; set; }
		public string Color { get; set; }

		public string Id { get { return Name; }}
		public string Href { get { return Url; }}
		public string ProjectId { get { return GetProjectName(this); } }
		public string ProjectName { get { return "Default"; } }

		public override bool Equals(object obj)
		{
			var other = obj as JenkinsJob;
			if (other == null)
				return false;

			return Id == other.Id;
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}

		public override string ToString()
		{
			return string.Format("[JenkinsJob: Id={0}, Name={1}, Url={2}]", Id, Name, Url);
		}

		internal static string GetProjectName(IBuildJob buildJob)
		{
			var indexOfSep = buildJob.Name.IndexOfAny(new[] { '-', '_' });
			return indexOfSep > -1 ? buildJob.Name.Substring(0, indexOfSep) : buildJob.Name;
		}
	}
}
