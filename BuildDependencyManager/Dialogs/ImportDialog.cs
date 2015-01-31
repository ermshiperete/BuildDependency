// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using Xwt;
using BuildDependency.TeamCity.RestClasses;
using BuildDependency.TeamCity;

namespace BuildDependency.Dialogs
{
	public class ImportDialog: Dialog
	{
		private ImportDialogModel _model;
		private readonly ComboBox _serversCombo;
		private readonly ComboBox _projectCombo;
		private readonly ComboBox _configCombo;
		private readonly ListView _listView;
		private DataField<string> _artifactsSource;
		private DataField<string> _artifactsPath;
		private ListStore _store;
		private CheckBox _windows;
		private CheckBox _linux32;
		private CheckBox _linux64;

		public ImportDialog(List<Server> servers)
		{
			_model = new ImportDialogModel();
			Title = "Import dependencies from TeamCity";
			Width = 600;
			Height = 400;

			Buttons.Add(new DialogButton("Import", Command.Ok));
			Buttons.Add(new DialogButton("Cancel", Command.Cancel));

			var vbox = new VBox();
			var hbox = new HBox();
			vbox.PackStart(hbox);
			hbox.PackStart(new Label("Server:"));
			_serversCombo = new ComboBox();
			_serversCombo.SelectionChanged += OnServerChanged;
			foreach (var server in servers)
			{
				_serversCombo.Items.Add(server);
			}
			_serversCombo.SelectedIndex = 0;
			hbox.PackStart(_serversCombo);

			vbox.PackStart(new Label("Condition to apply to all dependencies:"));
			hbox = new HBox();
			vbox.PackStart(hbox);
			_windows = new CheckBox("Windows") { State = CheckBoxState.On };
			hbox.PackStart(_windows);
			_linux32 = new CheckBox("Linux 32-bit") { State = CheckBoxState.On };
			hbox.PackStart(_linux32);
			_linux64 = new CheckBox("Linux 64-bit") { State = CheckBoxState.On };
			hbox.PackStart(_linux64);

			vbox.PackStart(new Label("Project"));
			_projectCombo = new ComboBox();
			_projectCombo.SelectionChanged += OnProjectChanged;
			vbox.PackStart(_projectCombo);
			vbox.PackStart(new Label("Build Configurations"));
			_configCombo = new ComboBox();
			_configCombo.SelectionChanged += OnConfigChanged;
			vbox.PackStart(_configCombo);
			vbox.PackStart(new Label("Dependencies"));
			_listView = new ListView();
			_artifactsSource = new DataField<string>();
			_artifactsPath = new DataField<string>();

			_listView.Columns.Add(new ListViewColumn("Artifacts source", new TextCellView { TextField = _artifactsSource }));
			_listView.Columns.Add(new ListViewColumn("Artifacts path", new TextCellView { TextField = _artifactsPath }));
			_listView.GridLinesVisible = GridLines.Both;
			_listView.HeadersVisible = true;
			_listView.HeightRequest = 300;
			_listView.VerticalPlacement = WidgetPlacement.Fill;
			_listView.ExpandVertical = true;
			vbox.PackStart(_listView);

			vbox.PackStart(new Label("Click Import to import the dependencies of the selected configuration."));

			Content = vbox;

			_store = new ListStore(_artifactsSource, _artifactsPath);
			_listView.DataSource = _store;

			_model.GetProjects(_projectCombo.Items);
		}

		private void OnServerChanged (object sender, EventArgs e)
		{
			_model.TeamCity = _serversCombo.SelectedItem as TeamCityApi;
			_projectCombo.Items.Clear();
			_configCombo.Items.Clear();
			_model.GetProjects(_projectCombo.Items);
			_projectCombo.SelectedIndex = 0;
			OnProjectChanged(this, EventArgs.Empty);
		}

		private void OnProjectChanged(object sender, EventArgs e)
		{
			var project = _projectCombo.SelectedItem as Project;
			_model.GetConfigurationsForProject(project.Id, _configCombo.Items);
			_configCombo.SelectedIndex = 0;
			OnConfigChanged(this, EventArgs.Empty);
		}

		private void OnConfigChanged(object sender, EventArgs e)
		{
			var config = _configCombo.SelectedItem as BuildType;
			SelectedBuildConfig = config.Id;

			var dataSource = _model.GetArtifactsDataSource(config.Id);
			_store.Clear();
			for (int i = 0; i < dataSource.RowCount; i++)
			{
				int row = _store.AddRow();
				_store.SetValue<string>(row, _artifactsSource, (string)dataSource.GetValue(row, 0));
				_store.SetValue<string>(row, _artifactsPath, (string)dataSource.GetValue(row, 1));
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
	}
}

