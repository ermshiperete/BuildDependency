// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using BuildDependencyManager.TeamCity;
using BuildDependencyManager.TeamCity.RestClasses;
using System.Text;

namespace BuildDependencyManager
{
	public class Artifact: ArtifactProperties
	{
		[Flags]
		public enum Conditions
		{
			None = 0,
			Windows = 1,
			Linux32 = 2,
			Linux64 = 4,
			All = Windows | Linux32 | Linux64
		}

		public Artifact(Project project, string buildTypeId)
		{
			Project = project;
			SourceBuildTypeId = buildTypeId;
			Condition = Conditions.All;
		}

		public Artifact(ArtifactProperties artifact)
		{
			PathRules = artifact.PathRules;
			RevisionName = artifact.RevisionName;
			RevisionValue = artifact.RevisionValue;
			SourceBuildTypeId = artifact.SourceBuildTypeId;
			CleanDestinationDirectory = artifact.CleanDestinationDirectory;
			Project = TeamCityApi.Singleton.Projects[Config.ProjectId];
			Condition = Conditions.All;
		}

		public string ConfigName
		{
			get
			{
				return Config.Name;
			}
		}

		public BuildType Config
		{
			get { return TeamCityApi.Singleton.BuildTypes[SourceBuildTypeId]; }
		}

		public Conditions Condition { get; set; }

		public Project Project { get; set; }
	}
}

