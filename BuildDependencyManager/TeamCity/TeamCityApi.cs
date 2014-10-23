// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using RestSharp;
using BuildDependencyManager.TeamCity.RestClasses;

namespace BuildDependencyManager.TeamCity
{
	public class TeamCityApi: Server
	{
		private Dictionary<string, BuildType> _buildTypes;
		private Dictionary<string, Project> _projects;

		public TeamCityApi() : base(ServerType.TeamCity)
		{
		}

		private string BaseUrl
		{
			get { return string.Format("{0}/guestAuth/app/rest/7.0", Url); }
		}

		private T Execute<T>(RestRequest request) where T : new()
		{
			var client = new RestClient();
			client.BaseUrl = BaseUrl;
			var response = client.Execute<T>(request);

			if (response.ErrorException != null)
			{
				const string message = "Error retrieving response.  Check inner details for more info.";
				throw new ApplicationException(message, response.ErrorException);
			}
			return response.Data;
		}

		public List<Project> GetAllProjects()
		{
			var request = new RestRequest();
			request.Resource = "/projects";
			request.RootElement = "projects";

			return Execute<Projects>(request).Project;
		}

		public List<BuildType> GetBuildTypes()
		{
			var request = new RestRequest();
			request.Resource = string.Format("/buildTypes");
			request.RootElement = "buildType";

			return Execute<List<BuildType>>(request);
		}

		public List<BuildType> GetBuildTypesForProject(string projectId)
		{
			var request = new RestRequest();
			request.Resource = string.Format("/projects/id:{0}/buildTypes", projectId);
			request.RootElement = "buildType";

			return Execute<List<BuildType>>(request);
		}

		public List<ArtifactDependency> GetArtifactDependencies(string buildTypeId)
		{
			var request = new RestRequest();
			request.Resource = string.Format("/buildTypes/id:{0}/artifact-dependencies", buildTypeId);
			request.RootElement = "artifact-dependency";

			return Execute<List<ArtifactDependency>>(request);
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

