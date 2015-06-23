// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using BuildDependency.Interfaces;
using BuildDependency.Tools;
using RestSharp;
using BuildDependency.TeamCity.RestClasses;
using BuildDependency.RestClasses;
using System.Threading.Tasks;

namespace BuildDependency.TeamCity
{
	public class TeamCityApi: Server, IServerApi
	{
		private Dictionary<string, IBuildJob> _buildTypes;
		private Dictionary<string, IProject> _projects;

		public TeamCityApi()
			: base(ServerType.TeamCity)
		{
		}

		private string BaseUrl
		{
			get { return string.Format("{0}/guestAuth/app/rest/7.0", Url); }
		}

		public string BaseRepoUrl
		{
			get { return string.Format("{0}/guestAuth/repository", Url); }
		}

		private T ExecuteApi<T>(RestRequest request, bool throwException = true) where T : new()
		{
			return RestHelper.Execute<T>(BaseUrl, request, throwException);
		}

		private T ExecuteRepo<T>(RestRequest request) where T : new()
		{
			return RestHelper.Execute<T>(BaseRepoUrl, request);
		}

		public List<IProject> GetAllProjects()
		{
			return GetAllProjectsAsync().Result;
		}

		public async Task<List<IProject>> GetAllProjectsAsync()
		{
			var request = new RestRequest
			{
				Resource = "/projects",
				RootElement = "projects"
			};

			return ExecuteApi<Projects>(request).IProject;
		}

		public List<IBuildJob> GetBuildJobs()
		{
			return GetBuildJobsAsync().Result;
		}

		public async Task<List<IBuildJob>> GetBuildJobsAsync()
		{
			var request = new RestRequest
			{
				Resource = string.Format("/buildTypes"),
				RootElement = "buildTypes"
			};

			return ExecuteApi<List<IBuildJob>>(request);
		}

		public List<IBuildJob> GetBuildJobsForProject(string projectId)
		{
			return GetBuildJobsForProjectAsync(projectId).Result;
		}

		public async Task<List<IBuildJob>> GetBuildJobsForProjectAsync(string projectId)
		{
			var request = new RestRequest
			{
				Resource = string.Format("/projects/id:{0}/buildTypes", projectId),
				RootElement = "buildTypes"
			};

			return ExecuteApi<BuildTypes>(request).BuildJobs;
		}

		public List<IArtifactDependency> GetArtifactDependencies(string buildTypeId)
		{
			return GetArtifactDependenciesAsync(buildTypeId).Result;
		}

		public async Task<List<IArtifactDependency>> GetArtifactDependenciesAsync(string buildTypeId)
		{
			var request = new RestRequest
			{
				Resource = string.Format("/buildTypes/id:{0}/artifact-dependencies", buildTypeId),
				RootElement = "artifact-dependencies"
			};

			var deps = ExecuteApi<List<IArtifactDependency>>(request, false);
			return deps;
		}

		public List<IArtifact> GetArtifacts(ArtifactTemplate template)
		{
			return GetArtifactsAsync(template).Result;
		}

		public async Task<List<IArtifact>> GetArtifactsAsync(ArtifactTemplate template)
		{
			var request = new RestRequest
			{
				Resource = string.Format("/download/{0}/{1}/teamcity-ivy.xml", template.Config.Id, template.RevisionValue),
				RootElement = "publications"
			};

			var artifacts = ExecuteRepo<List<IArtifact>>(request);
			return artifacts;
		}

		public Dictionary<string, IBuildJob> BuildJobs
		{
			get
			{
				if (_buildTypes == null)
				{
					_buildTypes = new Dictionary<string, IBuildJob>();
					foreach (var buildType in GetBuildJobs())
						_buildTypes.Add(buildType.Id, buildType);
				}
				return _buildTypes;
			}
		}

		public Dictionary<string, IProject> Projects
		{
			get
			{
				if (_projects == null)
				{
					_projects = new Dictionary<string, IProject>();
					foreach (var proj in GetAllProjects())
						_projects.Add(proj.Id, proj);
				}
				return _projects;
			}
		}

	}
}

