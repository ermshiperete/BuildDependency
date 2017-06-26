// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using BuildDependency.Artifacts;
using BuildDependency.RestClasses;
using BuildDependency.TeamCity.RestClasses;
using RestSharp;

namespace BuildDependency.TeamCity
{
	public class TeamCityApi: Server
	{
		private Dictionary<string, BuildType> _buildTypes;
		private Dictionary<string, Project> _projects;
		private Task<List<BuildType>> _buildTypeTask;

		public TeamCityApi()
			: base(ServerType.TeamCity)
		{
		}

		private string BaseUrl => $"{Url}/guestAuth/app/rest/latest";

		public string BaseRepoUrl => $"{Url}/guestAuth/repository";

		public override string Url
		{
			get => base.Url;
			set
			{
				base.Url = value;
				if (string.IsNullOrEmpty(value))
					return;

				var request = new RestRequest
				{
					Resource = "/buildTypes",
					RootElement = "buildType"
				};

				_buildTypeTask = ExecuteApi<List<BuildType>>(request);
			}
		}

		private static async Task<T> Execute<T>(string baseUrl, RestRequest request, bool throwException = true) where T : new()
		{
			var client = new RestClient
			{
				BaseUrl = new Uri(baseUrl)
			};
			IRestResponse<T> response;
//			try
//			{
			response = await client.ExecuteGetTaskAsync<T>(request);
//			}
//			catch (Exception e)
//			{
//				if (throwException)
//					throw;
//				return default(T);
//			}
//
//			if (response == null)
//				return default(T);

			if (response.ErrorException != null)
			{
				if (throwException && !(response.ErrorException is WebException) && !(response.ErrorException is NullReferenceException))
				{
					const string message = "Error retrieving response.  Check inner details for more info.";
					throw new ApplicationException(message, response.ErrorException);
				}
				return default(T);
			}
			return response.Data;
		}

		private async Task<T> ExecuteApi<T>(RestRequest request, bool throwException = true) where T : new()
		{
			return await Execute<T>(BaseUrl, request, throwException);
		}

		private async Task<T> ExecuteRepo<T>(RestRequest request, bool throwException = true) where T : new()
		{
			return await Execute<T>(BaseRepoUrl, request, throwException);
		}

		public List<Project> GetAllProjects()
		{
			return GetAllProjectsAsync().Result;
		}

		public async Task<List<Project>> GetAllProjectsAsync()
		{
			var request = new RestRequest
			{
				Resource = "/projects",
				RootElement = "projects"
			};

			var response = await ExecuteApi<Projects>(request);
			if (response == null)
				return null;
			var projects = response.Project;
			FillInProjectList(projects);
			return projects;
		}

		public List<BuildType> GetBuildTypes()
		{
			return GetBuildTypesTask().Result;
		}

		public Task<List<BuildType>> GetBuildTypesTask()
		{
			return _buildTypeTask;
		}

		public Task<List<BuildType>> GetBuildTypesForProjectTask(string projectId)
		{
			var request = new RestRequest
				{
					Resource = $"/projects/id:{projectId}/buildTypes",
					RootElement = "buildType"
				};

			return ExecuteApi<List<BuildType>>(request);
		}

		public List<ArtifactDependency> GetArtifactDependencies(string buildTypeId)
		{
			return GetArtifactDependenciesAsync(buildTypeId).Result;
		}

		public async Task<List<ArtifactDependency>> GetArtifactDependenciesAsync(string buildTypeId)
		{
			var request = new RestRequest
				{
					Resource = $"/buildTypes/id:{buildTypeId}/artifact-dependencies",
					RootElement = "artifact-dependency"
				};

			return await ExecuteApi<List<ArtifactDependency>>(request, false);
		}

		public List<Artifact> GetArtifacts(ArtifactTemplate template)
		{
			return GetArtifactsAsync(template).Result;
		}

		public async Task<List<Artifact>> GetArtifactsAsync(ArtifactTemplate template)
		{
			var request = new RestRequest
			{
				Resource =
					$"/download/{template.Config.IdForArtifacts}/{template.RevisionValue}/teamcity-ivy.xml",
				RootElement = "publications"
			};

			var artifacts = await ExecuteRepo<List<Artifact>>(request, false) ?? new List<Artifact>();
			return artifacts;
		}

		public Dictionary<string, BuildType> BuildTypes
		{
			get
			{
				if (_buildTypes == null)
				{
					FillBuildTypesDict();
				}
				return _buildTypes;
			}
		}

		private async void FillBuildTypesDict()
		{
			_buildTypes = new Dictionary<string, BuildType>();
			foreach (var buildType in await _buildTypeTask)
				_buildTypes.Add(buildType.Id, buildType);
		}

		private void FillInProjectList(List<Project> projectList)
		{
			if (_projects != null)
				return;

			_projects = new Dictionary<string, Project>();
			foreach (var proj in projectList)
				_projects.Add(proj.Id, proj);
		}

		public Dictionary<string, Project> Projects
		{
			get
			{
				var projectList = GetAllProjects();
				FillInProjectList(projectList);
				return _projects;
			}
		}

	}
}

