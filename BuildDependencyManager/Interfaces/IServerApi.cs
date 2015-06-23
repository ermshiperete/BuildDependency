// Copyright (c) 2015 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BuildDependency.Interfaces
{
	public interface IServerApi
	{
		ServerType ServerType { get; set; }
		string Name { get; set; }
		string Url { get; set; }

		string BaseRepoUrl { get; }
		Dictionary<string, IBuildJob> BuildJobs { get; }
		Dictionary<string, IProject> Projects { get; }
		List<IProject> GetAllProjects();
		Task<List<IProject>> GetAllProjectsAsync();
		List<IBuildJob> GetBuildJobs();
		Task<List<IBuildJob>> GetBuildJobsAsync();
		List<IBuildJob> GetBuildJobsForProject(string projectId);
		Task<List<IBuildJob>> GetBuildJobsForProjectAsync(string projectId);
		List<IArtifactDependency> GetArtifactDependencies(string buildTypeId);
		Task<List<IArtifactDependency>> GetArtifactDependenciesAsync(string buildTypeId);
		List<IArtifact> GetArtifacts(ArtifactTemplate template);
		Task<List<IArtifact>> GetArtifactsAsync(ArtifactTemplate template);
	}
}