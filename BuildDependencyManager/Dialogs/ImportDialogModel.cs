// Copyright (c) 2014-2015 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using Xwt;
using BuildDependency.TeamCity;
using BuildDependency.TeamCity.RestClasses;

namespace BuildDependency.Dialogs
{
	public class ImportDialogModel: IListDataSource
	{
		public TeamCityApi TeamCity { get; set; }

		public List<ArtifactProperties> Artifacts { get; private set; }

		public void GetProjects(ItemCollection projects)
		{
			projects.Clear();
			var allProjects = TeamCity.GetAllProjects();
			allProjects.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));
			foreach (var proj in allProjects)
			{
				projects.Add(proj, proj.Name);
			}
		}

		public List<Project> Projects
		{
			get
			{
				var allProjects = TeamCity.GetAllProjects();
				if (allProjects != null)
					allProjects.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));
				return allProjects;
			}
		}

		public void GetConfigurationsForProject(string projectId, ItemCollection configs)
		{
			configs.Clear();

			foreach (var config in TeamCity.GetBuildTypesForProject(projectId))
			{
				configs.Add(config, config.Name);
			}
		}

		public List<BuildType> GetConfigurationsForProject(string projectId)
		{
			return TeamCity.GetBuildTypesForProject(projectId);
		}

		public void GetArtifactDependencies(string configId, ItemCollection dependencies)
		{
			dependencies.Clear();
			foreach (var dep in TeamCity.GetArtifactDependencies(configId))
			{
				var prop = new ArtifactProperties(dep.Properties);
				var tag = prop.RevisionName == "buildTag" ? prop.RevisionValue.Substring(0, prop.RevisionValue.Length - ".tcbuildtag".Length) : prop.RevisionName;
				dependencies.Add(string.Format("id: {0}, pathRules: {1},\n buildType: {2} ({6}),\nrevName: {3}, revValue: {4} ({5})", dep.Id,
						prop.PathRules, prop.SourceBuildTypeId, prop.RevisionName, prop.RevisionValue, tag, TeamCity.BuildTypes[prop.SourceBuildTypeId].Name));
			}
		}

		public IListDataSource GetArtifactsDataSource(string configId)
		{
			Artifacts = new List<ArtifactProperties>();
			var deps = TeamCity.GetArtifactDependencies(configId);
			if (deps != null)
			{
				foreach (var dep in deps)
				{
					Artifacts.Add(new ArtifactProperties(dep.Properties));
				}
			}
			return this;
		}

		public void LoadArtifacts(string configId)
		{
			Artifacts = new List<ArtifactProperties>();
			var deps = TeamCity.GetArtifactDependencies(configId);
			if (deps != null)
			{
				foreach (var dep in deps)
				{
					Artifacts.Add(new ArtifactProperties(dep.Properties));
				}
			}
		}

		#region IListDataSource implementation

		public event EventHandler<ListRowEventArgs> RowInserted;

		public event EventHandler<ListRowEventArgs> RowDeleted;

		public event EventHandler<ListRowEventArgs> RowChanged;

		public event EventHandler<ListRowOrderEventArgs> RowsReordered;

		public object GetValue(int row, int column)
		{
			if (row >= Artifacts.Count)
				return null;
			var artifact = Artifacts[row];
			if (column == 0)
			{
				return string.Format("{0}\n({1})", TeamCity.BuildTypes[artifact.SourceBuildTypeId].Name,
					artifact.TagLabel);
			}
			return artifact.PathRules;
		}

		public void SetValue(int row, int column, object value)
		{
			throw new NotImplementedException();
		}

		public int RowCount
		{
			get
			{
				return Artifacts.Count;
			}
		}

		public Type[] ColumnTypes
		{
			get
			{
				return new[] { typeof(string), typeof(string) };
			}
		}

		#endregion
	}
}

