// Copyright (c) 2014-2015 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using BuildDependency.Artifacts;
using BuildDependency.TeamCity;
using BuildDependency.TeamCity.RestClasses;
using Eto.Drawing;
using Eto.Forms;

namespace BuildDependency.Dialogs
{
	public class ImportDialog: Dialog<bool>
	{
		private ImportDialogModel _model;
		private readonly ComboBox _serversCombo;
		private readonly ComboBox _projectCombo;
		private readonly ComboBox _configCombo;
		private readonly GridView _gridView;
		private readonly CheckBox _windows;
		private readonly CheckBox _linux32;
		private readonly CheckBox _linux64;

		public ImportDialog(List<Server> servers)
		{
			_model = new ImportDialogModel();
			_serversCombo = new ComboBox();
			_serversCombo.SelectedIndexChanged += OnServerChanged;
			_serversCombo.DataStore = servers;
			_serversCombo.SelectedIndex = 0;
			_gridView = new GridView();
			_gridView.DataStore = new SelectableFilterCollection<ArtifactTemplate>(_gridView);

			_gridView.Columns.Add(new GridColumn
				{
					HeaderText = "Artifacts source",
					DataCell = new TextBoxCell("Source"),
					Sortable = true,
					Resizable = true,
					AutoSize = true,
				});

			_gridView.Columns.Add(new GridColumn
				{
					HeaderText = "Artifacts path",
					DataCell = new TextBoxCell("PathRules"),
					Sortable = true,
					Resizable = true,
					AutoSize = true,
					Editable = true
				});
			_gridView.GridLines = GridLines.Both;
			_gridView.ShowHeader = true;
			_gridView.Height = 300;
			_projectCombo = new ComboBox();
			_projectCombo.SelectedIndexChanged += OnProjectChanged;
			_projectCombo.DataStore = _model.Projects;
			_projectCombo.SelectedIndex = 0;
			_configCombo = new ComboBox();
			_configCombo.SelectedIndexChanged += OnConfigChanged;
			_windows = new CheckBox { Text = "Windows", Checked = true };
			_linux32 = new CheckBox { Text = "Linux 32-bit", Checked = true };
			_linux64 = new CheckBox { Text = "Linux 64-bit", Checked = true };
			var importButton = new Button { Text = "Import" };
			importButton.Click += (sender, e) =>
			{
				Result = true;
				Close();
			};
			var cancelButton = new Button { Text = "Cancel" };
			cancelButton.Click += (sender, e) => Close();

			Title = "Import dependencies from TeamCity";
			Width = 600;
			Height = 600;
			Resizable = true;

			var content = new TableLayout
			{
				Padding = new Padding(10, 10, 10, 5),
				Spacing = new Size(5, 5),
				Rows =
				{
					new TableRow(new StackLayout
						{
							Spacing = 5,
							Orientation = Orientation.Horizontal,
							Items = { new Label { Text = "Server:" }, _serversCombo }
						}),
					new TableRow(new Label { Text = "Condition to apply to all dependencies:" }),
					new TableRow(new StackLayout
						{
							Spacing = 5,
							Orientation = Orientation.Horizontal,
							Items = { _windows, _linux32, _linux64 }
						}),
					new TableRow(new Label { Text = "Project" }),
					new TableRow(_projectCombo),
					new TableRow(new Label { Text = "Build Configurations" }),
					new TableRow(_configCombo),
					new TableRow(new Label { Text = "Dependencies" }),
					new TableRow(_gridView) { ScaleHeight = true },

					new TableRow(new Label { Text = "Click Import to import the dependencies of the selected configuration." }),
					new TableRow(new StackLayout
						{
							Orientation = Orientation.Horizontal,
							Spacing = 5,
							Items = { null, importButton, cancelButton }
						})
				}
			};

			Content = content;

			DefaultButton = importButton;
			AbortButton = cancelButton;
		}

		private void OnServerChanged(object sender, EventArgs e)
		{
			_model.TeamCity = _serversCombo.SelectedValue as TeamCityApi;
			var projects = _model.Projects;
			if (projects == null)
				return;

			_projectCombo.DataStore = projects;
			_projectCombo.SelectedIndex = 0;
			OnProjectChanged(this, EventArgs.Empty);
		}

		private void OnProjectChanged(object sender, EventArgs e)
		{
			var project = _projectCombo.SelectedValue as Project;
			if (project == null)
				return;
			_configCombo.DataStore = _model.GetConfigurationsForProject(project.Id);
			_configCombo.SelectedIndex = 0;
			OnConfigChanged(this, EventArgs.Empty);
		}

		private void OnConfigChanged(object sender, EventArgs e)
		{
			var config = _configCombo.DataStore as BuildType;
			if (config == null)
				return;

			SelectedBuildConfig = config.Id;

			_model.LoadArtifacts(config.Id);
		}

		public string SelectedBuildConfig { get; private set; }

		public Conditions Condition
		{
			get
			{
				return AddOrEditArtifactDependencyDialog.GetConditionFromCheckBox(_windows, _linux32, _linux64);
			}
		}

		public Server Server
		{
			get
			{
				return _serversCombo.SelectedValue as Server;
			}
		}
	}
}

