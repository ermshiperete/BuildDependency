// Copyright (c) 2014-2015 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using BuildDependency.TeamCity;
using BuildDependency.TeamCity.RestClasses;

namespace BuildDependency.Dialogs
{
	public class ImportDialogModel
	{
		public TeamCityApi TeamCity { get; set; }

		public List<ArtifactProperties> Artifacts { get; private set; }

		public List<Project> Projects
		{
			get
			{
				var allProjects = TeamCity.GetAllProjects();
				if (allProjects != null)
					allProjects.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));
				return allProjects;
			}
		}

		public List<BuildType> GetConfigurationsForProject(string projectId)
		{
			return TeamCity.GetBuildTypesForProject(projectId);
		}

		public void LoadArtifacts(string configId)
		{
			Artifacts = new List<ArtifactProperties>();
			var deps = TeamCity.GetArtifactDependencies(configId);
			if (deps != null)
			{
				foreach (var dep in deps)
				{
					Artifacts.Add(new ArtifactProperties(dep.Properties));
				}
			}
		}

	}
}

