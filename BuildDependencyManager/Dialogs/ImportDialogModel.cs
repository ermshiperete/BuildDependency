// Copyright (c) 2014 Eberhard Beilharz
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
		private List<ArtifactProperties> _Artifacts;

		public ImportDialogModel()
		{
		}

		public TeamCityApi TeamCity { get; set; }

		public void GetProjects(ItemCollection projects)
		{
			projects.Clear();
			foreach (var proj in TeamCity.GetAllProjects())
			{
				projects.Add(proj, proj.Name);
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

		public void GetArtifactDependencies(string configId, ItemCollection dependencies)
		{
			dependencies.Clear();
			foreach (var dep in TeamCity.GetArtifactDependencies(configId))
			{
				var prop = new ArtifactProperties(dep.Properties);
				var tag = prop.RevisionName == "buildTag" ? prop.RevisionValue.Substring(0, prop.RevisionValue.Length - ".tcbuildtag".Length) : prop.RevisionName;
				dependencies.Add(string.Format("id: {0}, pathRules: {1},\n buildType: {2} ({6}),\nrevName: {3}, revValue: {4} ({5})", dep.Id,
						prop.PathRules, prop.SourceBuildTypeId, prop.RevisionName, prop.RevisionValue, tag, TeamCity.BuildTypes [prop.SourceBuildTypeId].Name));
			}
		}

		public IListDataSource GetArtifactsDataSource(string configId)
		{
			_Artifacts = new List<ArtifactProperties>();
			var deps = TeamCity.GetArtifactDependencies(configId);
			if (deps != null)
			{
				foreach (var dep in deps)
				{
					_Artifacts.Add(new ArtifactProperties(dep.Properties));
				}
			}
			return this;
		}


		#region IListDataSource implementation

		public event EventHandler<ListRowEventArgs> RowInserted;

		public event EventHandler<ListRowEventArgs> RowDeleted;

		public event EventHandler<ListRowEventArgs> RowChanged;

		public event EventHandler<ListRowOrderEventArgs> RowsReordered;

		public object GetValue(int row, int column)
		{
			if (row >= _Artifacts.Count)
				return null;
			var artifact = _Artifacts[row];
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
				return _Artifacts.Count;
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

