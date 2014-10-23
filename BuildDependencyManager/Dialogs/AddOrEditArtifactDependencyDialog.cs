// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using Xwt;
using BuildDependencyManager.RestClasses;
using BuildDependencyManager.TeamCity;
using BuildDependencyManager.TeamCity.RestClasses;
using BuildDependencyManager.Widgets;

namespace BuildDependencyManager.Dialogs
{
	public class AddOrEditArtifactDependencyDialog: Dialog
	{
		private ImportDialogModel _model;
		private readonly ComboBox _serversCombo;
		private readonly ComboBox _projectCombo;
		private readonly ComboBox _configCombo;
		private readonly ComboBox _buildTagType;
		private readonly MultiLineTextEntry _textView;
		private Label _buildTagEntryLabel;
		private readonly TextEntry _buildTagEntry;
		private Table _table;
		private string _buildNumber;
		private string _buildTag;
		private int _currentBuildSourceIndex;
		private readonly CheckBox _windows;
		private readonly CheckBox _linux32;
		private readonly CheckBox _linux64;

		public AddOrEditArtifactDependencyDialog(bool isAddDialog, List<Server> servers)
		{
			_model = new ImportDialogModel();
			Title = isAddDialog ? "Add New Artifact Dependency" : "Edit Artifact Dependency";
			Width = 600;
			Height = 400;

			Buttons.Add(new DialogButton("Save", Command.Ok));
			Buttons.Add(new DialogButton("Cancel", Command.Cancel));

			int row = 0;
			_table = new Table();
			_table.Add(new Label("Server:"), 0, row);
			_serversCombo = new ComboBox();
			_serversCombo.SelectionChanged += OnServerChanged;
			foreach (var server in servers)
			{
				_serversCombo.Items.Add(server);
			}
			_serversCombo.SelectedIndex = 0;

			_table.Add(_serversCombo, 1, row++);

			_table.Add(new Label("Depend on:"), 0, row);
			_projectCombo = new ComboBox();
			_projectCombo.SelectionChanged += OnProjectChanged;
			_table.Add(_projectCombo, 1, row++, hexpand: true);

			_configCombo = new ComboBox();
			_configCombo.SelectionChanged += OnConfigChanged;
			_table.Add(_configCombo, 1, row++, hexpand: true);

			_table.Add(new Label("Get Artifacts from:"), 0, row);
			_buildTagType = new ComboBox();
			_buildTagType.Items.Add("Last successful build");
			_buildTagType.Items.Add("Last pinned build");
			_buildTagType.Items.Add("Last finished build");
			_buildTagType.Items.Add("Build with specified build number");
			_buildTagType.Items.Add("Last finished build with specified tag");
			_buildTagType.SelectedIndex = 0;
			_buildTagType.SelectionChanged += OnArtifactSourceChanged;
			_table.Add(_buildTagType, 1, row++, hexpand: true);

			_buildTagEntryLabel = new Label("Build tag:") { TextAlignment = Alignment.End, Visible = false };
			_table.Add(_buildTagEntryLabel, 0, row);
			_buildTagEntry = new TextEntry() { Visible = false };
			_table.Add(_buildTagEntry, 1, row++);

			_table.Add(new Label("Artifact rules:"), 0, row, vpos: WidgetPlacement.Start);
			_textView = new MultiLineTextEntry();
			_textView.MinHeight = 200;
			_textView.HorizontalPlacement = WidgetPlacement.Start;
			_table.Add(_textView, 1, row++, hexpand: true, vexpand: true, vpos: WidgetPlacement.Start);

			_table.Add(new Label("Newline-delimited set or rules in the form of\n[+:|-:]SourcePath[!ArchivePath][=>DestinationPath]"), 1, row++);

			_table.Add(new Label("Condition:"), 0, row);
			var hbox = new HBox();
			_table.Add(hbox, 1, row++);
			_windows = new CheckBox("Windows") { State = CheckBoxState.On };
			hbox.PackStart(_windows);
			_linux32 = new CheckBox("Linux 32-bit") { State = CheckBoxState.On };
			hbox.PackStart(_linux32);
			_linux64 = new CheckBox("Linux 64-bit") { State = CheckBoxState.On };
			hbox.PackStart(_linux64);

			Content = _table;

			_model.GetProjects(_projectCombo.Items);
		}

		private void OnServerChanged (object sender, EventArgs e)
		{
			_model.TeamCity = _serversCombo.SelectedItem as TeamCityApi;
		}

		public AddOrEditArtifactDependencyDialog(List<Server> servers, Artifact artifact)
			: this(false, servers)
		{
			LoadArtifact(artifact);
		}

		private void SetCheckBox(CheckBox checkBox, Artifact artifact, Artifact.Conditions condition)
		{
			checkBox.State = (artifact.Condition & condition) == condition ? CheckBoxState.On : CheckBoxState.Off;
		}

		private void LoadArtifact(Artifact artifact)
		{
			_serversCombo.SelectedItem = artifact.Server;
			_projectCombo.SelectedItem = artifact.Project;
			_configCombo.SelectedItem = artifact.Config;
			_textView.Text = artifact.PathRules;
			SetCheckBox(_windows, artifact, Artifact.Conditions.Windows);
			SetCheckBox(_linux32, artifact, Artifact.Conditions.Linux32);
			SetCheckBox(_linux64, artifact, Artifact.Conditions.Linux64);
			BuildTagType revisionName;
			Enum.TryParse(artifact.RevisionName, out revisionName);
			switch (revisionName)
			{
				case BuildTagType.buildNumber:
					_buildNumber = artifact.RevisionValue;
					break;
				case BuildTagType.buildTag:
					_buildTag = artifact.Tag;
					break;
			}
			_buildTagType.SelectedIndex = (int)revisionName;
		}

		private void OnArtifactSourceChanged(object sender, EventArgs e)
		{
			if (_currentBuildSourceIndex == 3)
				_buildNumber = _buildTagEntry.Text;
			else if (_currentBuildSourceIndex == 4)
				_buildTag = _buildTagEntry.Text;

			var combobox = sender as ComboBox;
			switch (combobox.SelectedIndex)
			{
				case 0:
				case 1:
				case 2:
					_buildTagEntry.Visible = false;
					_buildTagEntryLabel.Visible = false;
					break;
				case 3:
					_buildTagEntryLabel.Visible = true;
					_buildTagEntry.Visible = true;
					_buildTagEntryLabel.Text = "Build number:";
					_buildTagEntry.Text = _buildNumber;
					break;
				case 4:
					_buildTagEntryLabel.Visible = true;
					_buildTagEntry.Visible = true;
					_buildTagEntryLabel.Text = "Build tag:";
					_buildTagEntry.Text = _buildTag;
					break;
			}
			_currentBuildSourceIndex = combobox.SelectedIndex;
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

			var dataSource = _model.GetArtifactsDataSource(config.Id);
		}

		public static Artifact.Conditions GetConditionFromCheckBox(CheckBox windows, CheckBox linux32, CheckBox linux64)
		{
			return (GetCheckBoxState(windows, Artifact.Conditions.Windows) |
				GetCheckBoxState(linux32, Artifact.Conditions.Linux32) |
				GetCheckBoxState(linux64, Artifact.Conditions.Linux64));
		}

		private static Artifact.Conditions GetCheckBoxState(CheckBox checkBox, Artifact.Conditions condition)
		{
			return checkBox.State == CheckBoxState.On ? condition : Artifact.Conditions.None;
		}

		public Artifact GetArtifact()
		{
			var server = _serversCombo.SelectedItem as Server;
			var proj = _projectCombo.SelectedItem as Project;
			var config = _configCombo.SelectedItem as BuildType;
			var artifact = new Artifact(server, proj, config.Id);
			artifact.PathRules = _textView.Text;
			artifact.Condition = GetConditionFromCheckBox(_windows, _linux32, _linux64);

			artifact.RevisionName = Enum.GetName(typeof(BuildTagType), _buildTagType.SelectedIndex);
			switch (_buildTagType.SelectedIndex)
			{
				case 0:
					artifact.RevisionValue = "latest.lastSuccessful";
					break;
				case 1:
					artifact.RevisionValue = "latest.lastPinned";
					break;
				case 2:
					artifact.RevisionValue = "latest.lastFinished";
					break;
				case 3:
					artifact.RevisionValue = _buildTagEntry.Text;
					break;
				case 4:
					artifact.RevisionValue = _buildTagEntry.Text + ".tcbuildtag";
					break;
			}
			return artifact;
		}
	}
}

