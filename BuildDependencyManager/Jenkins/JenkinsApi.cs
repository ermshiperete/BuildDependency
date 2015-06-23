// Copyright (c) 2015 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BuildDependency.Interfaces;
using BuildDependency.Jenkins.RestClasses;
using BuildDependency.Tools;
using RestSharp;

namespace BuildDependency.Jenkins
{
	public class JenkinsApi: Server, IServerApi
	{
		private JenkinsFeatures _features;

		public JenkinsApi() : base(ServerType.Jenkins)
		{
		}

		private static T Execute<T>(string baseUrl, IRestRequest request, bool throwException = true) where T : new()
		{
			var client = RestHelper.CreateRestClient(baseUrl);
			JenkinsXmlDeserializer.Register(client);
			return RestHelper.Execute<T>(client, request, throwException);
		}

		private JenkinsFeatures JenkinsFeature
		{
			get
			{
				if (_features == null)
				{
					var request = new RestRequest {Resource = string.Format("/hudson"), RootElement = ""};
					_features = Execute<JenkinsFeatures>(BaseRepoUrl, request);
				}
				return _features;
			}
		}

		#region IServerApi Members

		public string BaseRepoUrl
		{
			get { return string.Format("{0}/api/xml", Url); }
		}

		public Dictionary<string, IBuildJob> BuildJobs
		{
			get { throw new NotImplementedException(); }
		}

		public Dictionary<string, IProject> Projects
		{
			get
			{
				var allProjects = GetAllProjects();
				var projectsDict = new Dictionary<string, IProject> {{allProjects[0].Id, allProjects[0]}};
				return projectsDict;
			}
		}

		public List<IProject> GetAllProjects()
		{
			return JenkinsFeature.Projects.ToList();
		}

		public Task<List<IProject>> GetAllProjectsAsync()
		{
			throw new NotImplementedException();
		}

		public List<IBuildJob> GetBuildJobs()
		{
			return JenkinsFeature.BuildJobs;
		}

		public Task<List<IBuildJob>> GetBuildJobsAsync()
		{
			throw new NotImplementedException();
		}

		public List<IBuildJob> GetBuildJobsForProject(string projectId)
		{
			return JenkinsFeature.BuildJobs.Where(job => job.ProjectId == projectId).ToList();
		}

		public Task<List<IBuildJob>> GetBuildJobsForProjectAsync(string projectId)
		{
			throw new NotImplementedException();
		}

		public List<IArtifactDependency> GetArtifactDependencies(string buildTypeId)
		{
			throw new NotImplementedException();
		}

		public Task<List<IArtifactDependency>> GetArtifactDependenciesAsync(string buildTypeId)
		{
			throw new NotImplementedException();
		}

		public List<IArtifact> GetArtifacts(ArtifactTemplate template)
		{
			throw new NotImplementedException();
		}

		public Task<List<IArtifact>> GetArtifactsAsync(ArtifactTemplate template)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
