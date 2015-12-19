// Copyright (c) 2014-2015 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BuildDependency.Artifacts;
using BuildDependency.Manager.Tools;
using BuildDependency.TeamCity;
using BuildDependency.TeamCity.RestClasses;
using Eto.Drawing;
using Eto.Forms;

namespace BuildDependency.Manager.Dialogs
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
		private readonly SelectableFilterCollection<ArtifactTemplate> _dataStore;
		private readonly Spinner _spinner;

		public ImportDialog(List<Server> servers)
		{
			_model = new ImportDialogModel();
			_serversCombo = new ComboBox();
			_serversCombo.SelectedIndexChanged += OnServerChanged;
			_serversCombo.DataStore = servers;
			_projectCombo = new ComboBox();
			_projectCombo.SelectedIndexChanged += OnProjectChanged;
			_projectCombo.DataStore = _model.Projects;
			_configCombo = new ComboBox();
			_configCombo.SelectedIndexChanged += OnConfigChanged;
			_windows = new CheckBox { Text = "Windows", Checked = true };
			_linux32 = new CheckBox { Text = "Linux 32-bit", Checked = true };
			_linux64 = new CheckBox { Text = "Linux 64-bit", Checked = true };
			_spinner = new Spinner { Size = new Size(30, 30), Visible = false };
			var importButton = new Button { Text = "Import" };
			importButton.Click += (sender, e) =>
			{
				Result = true;
				Close();
			};
			var cancelButton = new Button { Text = "Cancel" };
			cancelButton.Click += (sender, e) => Close();

			_gridView = new GridView();
			_dataStore = new SelectableFilterCollection<ArtifactTemplate>(_gridView);
			_gridView.DataStore = _dataStore;

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
							Items = { _spinner, null, importButton, cancelButton }
						})
				}
			};

			Content = content;

			DefaultButton = importButton;
			AbortButton = cancelButton;

			_serversCombo.SelectedIndex = 0;
			_projectCombo.SelectedIndex = 0;
		}

		private async void OnServerChanged(object sender, EventArgs e)
		{
			using (new WaitSpinner(_spinner))
			{
				_model.TeamCity = _serversCombo.SelectedValue as TeamCityApi;
				await Task.Run(() =>
					{
						var projects = _model.Projects;
						if (projects == null)
							return;

						_projectCombo.DataStore = projects;
					});
				_projectCombo.SelectedIndex = 0;
			}
		}

		private async void OnProjectChanged(object sender, EventArgs e)
		{
			using (new WaitSpinner(_spinner))
			{
				var project = _projectCombo.SelectedValue as Project;
						if (project == null)
							return;
				await Task.Run(() =>
					{
						_configCombo.DataStore = _model.GetConfigurationsForProject(project.Id);
					});
				_configCombo.SelectedIndex = 0;
			}
		}

		private async void OnConfigChanged(object sender, EventArgs e)
		{
			using (new WaitSpinner(_spinner))
			{
				var config = _configCombo.SelectedValue as BuildType;
				if (config == null)
					return;

				SelectedBuildConfig = config.Id;

				await Task.Run(() =>
					{
						_dataStore.Clear();
						_model.LoadArtifacts(config.Id);
						var dependencies = _model.TeamCity.GetArtifactDependencies(config.Id);
						if (dependencies == null)
							return;

						foreach (var dep in dependencies)
						{
							if (dep == null)
								continue;
							var artifact = new ArtifactTemplate(_model.TeamCity, new ArtifactProperties(dep.Properties));
							_dataStore.Add(artifact);
						}
					});
			}
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

