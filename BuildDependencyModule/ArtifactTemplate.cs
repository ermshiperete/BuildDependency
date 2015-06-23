﻿// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using BuildDependency.Artifacts;
using BuildDependency.Interfaces;
using BuildDependency.TeamCity;
using BuildDependency.TeamCity.RestClasses;
using System.IO;

namespace BuildDependency
{
	public class ArtifactTemplate: ArtifactProperties
	{
		public ArtifactTemplate(IServerApi server, IProject project, string buildTypeId)
		{
			Server = server;
			Project = project;
			SourceBuildTypeId = buildTypeId;
			Condition = Conditions.All;
		}

		public ArtifactTemplate(IServerApi server, ArtifactProperties artifact)
		{
			Server = server;
			PathRules = artifact.PathRules;
			RevisionName = artifact.RevisionName;
			RevisionValue = artifact.RevisionValue;
			SourceBuildTypeId = artifact.SourceBuildTypeId;
			CleanDestinationDirectory = artifact.CleanDestinationDirectory;
			Project = ((TeamCityApi)server).Projects[Config.ProjectId];
			Condition = Conditions.All;
		}

		public string ConfigName
		{
			get
			{
				return Config.Name;
			}
		}

		public IBuildJob Config
		{
			get { return Server.BuildJobs[SourceBuildTypeId]; }
		}

		public Conditions Condition { get; set; }

		public IProject Project { get; set; }

		public IServerApi Server { get; set; }

		public string RepoUrl
		{
			get
			{
				return string.Format("{0}/download/{1}/{2}", ((TeamCityApi)Server).BaseRepoUrl, Config.Id, RevisionValue);
			}
		}

		public List<IJob> GetJobs()
		{
			var rules = new List<ArtifactRule>();
			foreach (var ruleString in PathRules.Split('\n'))
			{
				if (string.IsNullOrEmpty(ruleString.Trim()))
					continue;
				rules.Add(new ArtifactRule(Condition, RepoUrl, ruleString));
			}

			var jobs = new List<IJob>();
			foreach (var file in ((TeamCityApi)Server).GetArtifacts(this))
			{
				foreach (var rule in rules)
				{
					var jobsForThisRule = rule.GetJobs(file.ToString());
					if (jobsForThisRule != null)
						jobs.AddRange(jobsForThisRule);
				}
			}
			return jobs;
		}
	}
}
