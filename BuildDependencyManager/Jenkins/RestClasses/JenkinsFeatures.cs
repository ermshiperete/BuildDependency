// Copyright (c) 2015 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System.Collections.Generic;
using System.Linq;
using BuildDependency.Interfaces;
using BuildDependency.TeamCity.RestClasses;

namespace BuildDependency.Jenkins.RestClasses
{
	class JenkinsFeatures
	{
		public string AssignedLabel { get; set; }
		public string Mode { get; set; }
		public string NodeDescription { get; set; }
		public string NodeName { get; set; }
		public string NumExecutors { get; set; }
		public object Description { get; set; }
		public List<JenkinsJob> Job { get; set; }
		public string OverallLoad { get; set; }
		public JenkinsView PrimaryView { get; set; }
		public string QuietingDown { get; set; }
		public string SlaveAgentPort { get; set; }
		public string UnlabeledLoad { get; set; }
		public string UseCrumbs { get; set; }
		public string UseSecurity { get; set; }
		public List<JenkinsView> View { get; set; }

		public List<IBuildJob> BuildJobs { get { return Job.ConvertAll(j => j as IBuildJob); } }

		private static IProject ExtractProject(IBuildJob buildJob)
		{
			var name = JenkinsJob.GetProjectName(buildJob);
			return new JenkinsProject() {Name = name, Id = name};
		}

		public IEnumerable<IProject> Projects
		{
			get
			{
				var projects = BuildJobs.ConvertAll(ExtractProject);
				return projects.GroupBy(proj => proj.Id, (id, projs) => projs.ToArray()[0]);
			}
		}
	}
}
