// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using RestSharp;
using BuildDependency.TeamCity.RestClasses;
using BuildDependency.RestClasses;
using System.Threading.Tasks;

namespace BuildDependency.TeamCity
{
	public class TeamCityApi: Server
	{
		private Dictionary<string, BuildType> _buildTypes;
		private Dictionary<string, Project> _projects;

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

		private T Execute<T>(string baseUrl, RestRequest request, bool throwException = true) where T : new()
		{
			var client = new RestClient();
			client.BaseUrl = new Uri(baseUrl);
			var response = client.Execute<T>(request);

			if (response.ErrorException != null)
			{
				if (throwException)
				{
					const string message = "Error retrieving response.  Check inner details for more info.";
					throw new ApplicationException(message, response.ErrorException);
				}
				return default(T);
			}
			return response.Data;
		}

		private T ExecuteApi<T>(RestRequest request, bool throwException = true) where T : new()
		{
			return Execute<T>(BaseUrl, request, throwException);
		}

		private T ExecuteRepo<T>(RestRequest request) where T : new()
		{
			return Execute<T>(BaseRepoUrl, request);
		}

		public List<Project> GetAllProjects()
		{
			return GetAllProjectsAsync().Result;
		}

		public async Task<List<Project>> GetAllProjectsAsync()
		{
			var request = new RestRequest();
			request.Resource = "/projects";
			request.RootElement = "projects";

			return ExecuteApi<Projects>(request).Project;
		}

		public List<BuildType> GetBuildTypes()
		{
			return GetBuildTypesAsync().Result;
		}

		public async Task<List<BuildType>> GetBuildTypesAsync()
		{
			var request = new RestRequest();
			request.Resource = string.Format("/buildTypes");
			request.RootElement = "buildType";

			return ExecuteApi<List<BuildType>>(request);
		}

		public List<BuildType> GetBuildTypesForProject(string projectId)
		{
			return GetBuildTypesForProjectAsync(projectId).Result;
		}

		public async Task<List<BuildType>> GetBuildTypesForProjectAsync(string projectId)
		{
			var request = new RestRequest();
			request.Resource = string.Format("/projects/id:{0}/buildTypes", projectId);
			request.RootElement = "buildType";

			return ExecuteApi<List<BuildType>>(request);
		}

		public List<ArtifactDependency> GetArtifactDependencies(string buildTypeId)
		{
			return GetArtifactDependenciesAsync(buildTypeId).Result;
		}

		public async Task<List<ArtifactDependency>> GetArtifactDependenciesAsync(string buildTypeId)
		{
			var request = new RestRequest();
			request.Resource = string.Format("/buildTypes/id:{0}/artifact-dependencies", buildTypeId);
			request.RootElement = "artifact-dependency";

			return ExecuteApi<List<ArtifactDependency>>(request, false);
		}

		public List<Artifact> GetArtifacts(ArtifactTemplate template)
		{
			return GetArtifactsAsync(template).Result;
		}

		public async Task<List<Artifact>> GetArtifactsAsync(ArtifactTemplate template)
		{
			var request = new RestRequest();
			request.Resource = string.Format("/download/{0}/{1}/teamcity-ivy.xml", template.Config.Id, template.RevisionValue);
			request.RootElement = "publications";

			var artifacts = ExecuteRepo<List<Artifact>>(request);
			return artifacts;
		}

		public Dictionary<string, BuildType> BuildTypes
		{
			get
			{
				if (_buildTypes == null)
				{
					_buildTypes = new Dictionary<string, BuildType>();
					foreach (var buildType in GetBuildTypes())
						_buildTypes.Add(buildType.Id, buildType);
				}
				return _buildTypes;
			}
		}

		public Dictionary<string, Project> Projects
		{
			get
			{
				if (_projects == null)
				{
					_projects = new Dictionary<string, Project>();
					foreach (var proj in GetAllProjects())
						_projects.Add(proj.Id, proj);
				}
				return _projects;
			}
		}

	}
}

