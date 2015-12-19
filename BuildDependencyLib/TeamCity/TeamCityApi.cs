// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using RestSharp;
using BuildDependency.TeamCity.RestClasses;
using BuildDependency.RestClasses;
using System.Threading.Tasks;
using System.Net;
using BuildDependency.Artifacts;

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

		private string BaseUrl
		{
			get { return string.Format("{0}/guestAuth/app/rest/7.0", Url); }
		}

		public string BaseRepoUrl
		{
			get { return string.Format("{0}/guestAuth/repository", Url); }
		}

		public override string Url
		{
			get { return base.Url; }
			set
			{
				base.Url = value;
				if (string.IsNullOrEmpty(value))
					return;

				var request = new RestRequest();
				request.Resource = string.Format("/buildTypes");
				request.RootElement = "buildType";

				_buildTypeTask = ExecuteApi<List<BuildType>>(request);
			}
		}

		private async Task<T> Execute<T>(string baseUrl, RestRequest request, bool throwException = true) where T : new()
		{
			var client = new RestClient();
			client.BaseUrl = new Uri(baseUrl);
			IRestResponse<T> response = null;
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
				if (throwException && !(response.ErrorException is WebException))
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
			var request = new RestRequest();
			request.Resource = "/projects";
			request.RootElement = "projects";

			var response = await ExecuteApi<Projects>(request);
			return response != null ? response.Project : null;
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

			return await ExecuteApi<List<ArtifactDependency>>(request, false);
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

