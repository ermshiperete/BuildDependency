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
		private readonly CheckBox _win32;
		private readonly CheckBox _win64;
		private readonly CheckBox _linux32;
		private readonly CheckBox _linux64;
		private readonly SelectableFilterCollection<ArtifactTemplate> _dataStore;
		private readonly Spinner _spinner;

		public ImportDialog(List<Server> servers)
		{
			_serversCombo = new ComboBox();
			_projectCombo = new ComboBox();
			_configCombo = new ComboBox();
			_win32 = new CheckBox {
				Text = "Win 32-bit",
				Checked = true
			};
			_win64 = new CheckBox
			{
				Text = "Win 64-bit",
				Checked = true
			};
			_linux32 = new CheckBox {
				Text = "Linux 32-bit",
				Checked = true
			};
			_linux64 = new CheckBox {
				Text = "Linux 64-bit",
				Checked = true
			};
			_spinner = new Spinner {
				Size = new Size(30, 30),
				Visible = false
			};
			_gridView = new GridView();
			_dataStore = new SelectableFilterCollection<ArtifactTemplate>(_gridView);
			Init(servers);
		}

		private async void Init(List<Server> servers)
		{
			_model = new ImportDialogModel();
			_serversCombo.SelectedIndexChanged += OnServerChanged;
			_serversCombo.DataStore = servers;
			_projectCombo.SelectedIndexChanged += OnProjectChanged;
			_projectCombo.DataStore = await _model.GetProjects();
			_configCombo.SelectedIndexChanged += OnConfigChanged;
			var importButton = new Button {
				Text = "Import"
			};
			importButton.Click += (sender, e) =>
			{
				Result = true;
				Close();
			};
			var cancelButton = new Button {
				Text = "Cancel"
			};
			cancelButton.Click += (sender, e) => Close();
			_gridView.DataStore = _dataStore;
			_gridView.Columns.Add(new GridColumn {
				HeaderText = "Artifacts source",
				DataCell = new TextBoxCell("Source"),
				Sortable = true,
				Resizable = true,
				AutoSize = true,
			});
			_gridView.Columns.Add(new GridColumn {
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
			var content = new TableLayout {
				ClientSize = new Size(600, 600),
				Padding = new Padding(10, 10, 10, 5),
				Spacing = new Size(5, 5),
				Rows =  {
					new TableRow(new StackLayout {
						Spacing = 5,
						Orientation = Orientation.Horizontal,
						Items =  {
							new Label {
								Text = "Server:"
							},
							_serversCombo
						}
					}),
					new TableRow(new Label {
						Text = "Condition to apply to all dependencies:"
					}),
					new TableRow(new StackLayout {
						Spacing = 5,
						Orientation = Orientation.Horizontal,
						Items =  {
							_win32,
							_win64,
							_linux32,
							_linux64
						}
					}),
					new TableRow(new Label {
						Text = "Project"
					}),
					new TableRow(_projectCombo),
					new TableRow(new Label {
						Text = "Build Configurations"
					}),
					new TableRow(_configCombo),
					new TableRow(new Label {
						Text = "Dependencies"
					}),
					new TableRow(_gridView) {
						ScaleHeight = true
					},
					new TableRow(new Label {
						Text = "Click Import to import the dependencies of the selected configuration."
					}),
					new TableRow(new StackLayout {
						Orientation = Orientation.Horizontal,
						Spacing = 5,
						Items = {
							_spinner,
							null,
							importButton,
							cancelButton
						}
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
				var projects = await _model.GetProjects();
				if (projects == null)
					return;

				_projectCombo.DataStore = projects;
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

				_configCombo.DataStore = await _model.GetConfigurationsForProjectTask(project.Id);
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

				await Task.Run(async () =>
					{
						_dataStore.Clear();
						var artifactsTask = _model.LoadArtifacts(config.Id);
						var depTask = _model.TeamCity.GetArtifactDependenciesAsync(config.Id);
						await artifactsTask;
						var dependencies = await depTask;
						if (dependencies == null)
							return;

						foreach (var dep in dependencies)
						{
							if (dep == null || dep.Properties == null || dep.BuildType == null)
								continue;
							var artifact = new ArtifactTemplate(_model.TeamCity, new ArtifactProperties(dep.Properties), dep.BuildType);
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
				return AddOrEditArtifactDependencyDialog.GetConditionFromCheckBox(_win32, _win64, _linux32, _linux64);
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

