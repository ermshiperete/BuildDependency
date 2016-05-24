// Copyright (c) 2014-2015 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BuildDependency.TeamCity;
using BuildDependency.TeamCity.RestClasses;

namespace BuildDependency.Manager.Dialogs
{
	public class ImportDialogModel
	{
		public TeamCityApi TeamCity { get; set; }

		public List<ArtifactProperties> Artifacts { get; private set; }

		public async Task<List<Project>> GetProjects()
		{
			if (TeamCity == null)
				return null;

			var allProjects = await TeamCity.GetAllProjectsAsync();
			if (allProjects != null)
				allProjects.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));
			return allProjects;
		}

		public Task<List<BuildType>> GetConfigurationsForProjectTask(string projectId)
		{
			return TeamCity.GetBuildTypesForProjectTask(projectId);
		}

		public async Task LoadArtifacts(string configId)
		{
			Artifacts = new List<ArtifactProperties>();
			var deps = await TeamCity.GetArtifactDependenciesAsync(configId);
			if (deps != null)
			{
				foreach (var dep in deps)
				{
					if (dep.Properties != null)
						Artifacts.Add(new ArtifactProperties(dep.Properties));
				}
			}
		}

	}
}

