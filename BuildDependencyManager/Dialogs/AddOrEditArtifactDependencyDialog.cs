// Copyright (c) 2014-2015 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BuildDependency.Artifacts;
using BuildDependency.Manager.Tools;
using BuildDependency.RestClasses;
using BuildDependency.TeamCity;
using BuildDependency.TeamCity.RestClasses;
using Eto.Drawing;
using Eto.Forms;

namespace BuildDependency.Manager.Dialogs
{
	public class AddOrEditArtifactDependencyDialog: Dialog<bool>
	{
		private ImportDialogModel _model;
		private readonly ComboBox _serversCombo;
		private readonly ComboBox _projectCombo;
		private readonly ComboBox _configCombo;
		private readonly ComboBox _buildTagType;
		private readonly TextArea _rulesTextBox;
		private readonly ListBox _preview;
		private Label _buildTagEntryLabel;
		private readonly TextBox _buildTagEntry;
		private TableLayout _table;
		private string _buildNumber;
		private string _buildTag;
		private int _currentBuildSourceIndex;
		private readonly CheckBox _win32;
		private readonly CheckBox _win64;
		private readonly CheckBox _linux32;
		private readonly CheckBox _linux64;
		private readonly Spinner _spinner;
		private List<Artifact> _artifacts;
		private ArtifactTemplate _artifactToLoad;

		public AddOrEditArtifactDependencyDialog(bool isAddDialog, List<Server> servers)
		{
			_serversCombo = new ComboBox();
			_projectCombo = new ComboBox();
			_configCombo = new ComboBox();
			_buildTagType = new ComboBox();
			_buildTagEntry = new TextBox {
				Visible = false
			};
			_rulesTextBox = new TextArea();
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
			_preview = new ListBox();
			_spinner = new Spinner {
				Size = new Size(20, 20),
				Visible = true
			};

			Init(isAddDialog, servers);
		}

		private async void Init(bool isAddDialog, List<Server> servers)
		{
			_model = new ImportDialogModel();
			Title = isAddDialog ? "Add New Artifact Dependency" : "Edit Artifact Dependency";
			Width = 600;
			Height = 600;
			Resizable = true;
			_serversCombo.SelectedIndexChanged += OnServerChanged;
			_serversCombo.DataStore = servers;
			_serversCombo.SelectedIndex = 0;
			_projectCombo.DataStore = await _model.GetProjects();
			_projectCombo.SelectedIndexChanged += OnProjectChangedInitial;
			_configCombo.SelectedIndexChanged += OnConfigChanged;
			_buildTagType.Items.Add("Last successful build");
			_buildTagType.Items.Add("Last pinned build");
			_buildTagType.Items.Add("Last finished build");
			_buildTagType.Items.Add("Build with specified build number");
			_buildTagType.Items.Add("Last finished build with specified tag");
			_buildTagType.SelectedIndex = 0;
			_buildTagType.SelectedIndexChanged += OnArtifactSourceChanged;
			_buildTagEntryLabel = new Label {
				Text = "Build tag:",
				TextAlignment = TextAlignment.Right,
				Visible = false
			};
			_rulesTextBox.Height = 150;
			//_rulesTextBox.HorizontalPlacement = HorizontalAlignment.Left;
			_rulesTextBox.TextChanged += (sender, e) => UpdatePreview();
			_preview.Height = 150;
			//_preview.HeightRequest = 200;
			//_preview.HorizontalPlacement = WidgetPlacement.Start;
			_preview.BackgroundColor = Colors.LightGrey;
			var okButton = new Button {
				Text = "Save"
			};
			okButton.Click += (sender, e) =>
			{
				Result = true;
				Close();
			};
			var cancelButton = new Button {
				Text = "Cancel"
			};
			cancelButton.Click += (sender, e) => Close();
			_table = new TableLayout {
				Padding = new Padding(10, 10, 10, 0),
				Spacing = new Size(5, 5),
				Rows =  {
					new TableRow("Server:", _serversCombo),
					new TableRow("Depend on:", new StackLayout() {
						Orientation = Orientation.Vertical,
						HorizontalContentAlignment = HorizontalAlignment.Stretch,
						Items =  {
							_projectCombo,
							_configCombo
						}
					}),
					new TableRow("Get Artifacts from:", _buildTagType),
					new TableRow(_buildTagEntryLabel, _buildTagEntry),
					new TableRow(new Label {
						Text = "Artifact rules:",
						VerticalAlignment = VerticalAlignment.Top
					}, new TableLayout {
						Rows =  {
							new TableRow(_rulesTextBox) {
								ScaleHeight = true
							},
							new TableRow(new Label {
								Text = "Newline-delimited set or rules in the form of\n[+:|-:]SourcePath[!ArchivePath][=>DestinationPath]"
							})
						}
					}) {
						ScaleHeight = true
					},
					new TableRow("Condition:", new StackLayout {
						Spacing = 5,
						Orientation = Orientation.Horizontal,
						Items =  {
							_win32,
							_win64,
							_linux32,
							_linux64
						}
					}),
					new TableRow("Preview:", _preview),
					new TableRow(new StackLayout {
						Orientation = Orientation.Horizontal,
						Spacing = 5,
						Items =  {
							_spinner,
							null
						}
					}, new StackLayout {
						Orientation = Orientation.Horizontal,
						Spacing = 5,
						Items =  {
							null,
							okButton,
							cancelButton
						}
					})
				}
			};
			DefaultButton = okButton;
			AbortButton = cancelButton;
			Content = _table;

			// This can happen because we run async
			if (_artifactToLoad != null)
				LoadArtifact(_artifactToLoad);
		}

		public AddOrEditArtifactDependencyDialog(List<Server> servers, ArtifactTemplate artifact)
			: this(false, servers)
		{
			_artifactToLoad = artifact;
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			if (_artifactToLoad != null)
				_spinner.Visible = false;
		}

		private void OnServerChanged(object sender, EventArgs e)
		{
			_model.TeamCity = _serversCombo.SelectedValue as TeamCityApi;
		}

		private static void SetCheckBox(CheckBox checkBox, ArtifactTemplate artifact, Conditions condition)
		{
			checkBox.Checked = (artifact.Condition & condition) == condition;
		}

		private void LoadArtifact(ArtifactTemplate artifact)
		{
			_serversCombo.SelectedValue = artifact.Server;
			_projectCombo.SelectedValue = artifact.Project;
			_configCombo.SelectedValue = artifact.Config;
			_rulesTextBox.Text = artifact.PathRules;
			UpdatePreview();
			SetCheckBox(_win32, artifact, Conditions.Win32);
			SetCheckBox(_win64, artifact, Conditions.Win64);
			SetCheckBox(_linux32, artifact, Conditions.Linux32);
			SetCheckBox(_linux64, artifact, Conditions.Linux64);
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
			_spinner.Visible = false;
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

		private async void OnProjectChangedInitial(object sender, EventArgs e)
		{
			var project = _projectCombo.SelectedValue as Project;
			if (project == null)
				return;

			_configCombo.DataStore = await _model.GetConfigurationsForProjectTask(project.Id);
			if (_artifactToLoad != null)
				_configCombo.SelectedValue = _artifactToLoad.Config;
			else
				_configCombo.SelectedIndex = 0;

			_projectCombo.SelectedIndexChanged -= OnProjectChangedInitial;
			_projectCombo.SelectedIndexChanged += OnProjectChanged;
		}

		private async void OnProjectChanged(object sender, EventArgs e)
		{
			var project = _projectCombo.SelectedValue as Project;
			if (project == null)
				return;

			_configCombo.DataStore = await _model.GetConfigurationsForProjectTask(project.Id);
			_configCombo.SelectedIndex = 0;
		}

		private async void OnConfigChanged(object sender, EventArgs e)
		{
			// this updates _textView.Text
			var config = _configCombo.SelectedValue as BuildType;
			if (config == null)
				return;
			await _model.LoadArtifacts(config.Id);
			var server = _serversCombo.SelectedValue as Server;
			var tcServer = server as TeamCityApi;
			if (tcServer != null)
			{
				var template = GetArtifact();
				if (template == null)
					return;
				_artifacts = await tcServer.GetArtifactsAsync(template);
			}
			UpdatePreview();
		}

		private async void UpdatePreview()
		{
			using (new WaitSpinner(_spinner))
			{
				_preview.Items.Clear();
				var template = GetArtifact();
				if (template == null)
					return;
				var jobsByFile = new Dictionary<string, List<IJob>>();
				var jobs = await template.GetJobs();
				foreach (var job in jobs.OfType<DownloadFileJob>())
				{
					if (!jobsByFile.ContainsKey(job.SourceFile))
						jobsByFile.Add(job.SourceFile, new List<IJob>());
					jobsByFile[job.SourceFile].Add(job);
				}
				foreach (var artifact in _artifacts)
				{
					List<IJob> jobsForThisFile;
					if (!jobsByFile.TryGetValue(artifact.ToString(), out jobsForThisFile))
						_preview.Items.Add(string.Format("**{0}**", artifact));
					else
					{
						foreach (var job in jobsForThisFile)
						{
							_preview.Items.Add(string.Format("{0} {1}", artifact, GetTarget(job)));
						}
					}
				}
			}
		}

		private string GetTarget(IJob job)
		{
			var downloadFile = job as DownloadFileJob;
			if (downloadFile != null)
				return "=> " + downloadFile.TargetFile;

			return string.Empty;
		}

		public static Conditions GetConditionFromCheckBox(CheckBox win32, CheckBox win64, CheckBox linux32, CheckBox linux64)
		{
			return (GetCheckBoxState(win32, Conditions.Win32) | GetCheckBoxState(win64, Conditions.Win64) |
			GetCheckBoxState(linux32, Conditions.Linux32) |
			GetCheckBoxState(linux64, Conditions.Linux64));
		}

		private static Conditions GetCheckBoxState(CheckBox checkBox, Conditions condition)
		{
			return checkBox.Checked.Value ? condition : Conditions.None;
		}

		public ArtifactTemplate GetArtifact()
		{
			var server = _serversCombo.SelectedValue as Server;
			var proj = _projectCombo.SelectedValue as Project;
			var config = _configCombo.SelectedValue as BuildType;
			if (config == null)
				return null;

			var artifact = new ArtifactTemplate(server, proj, config.Id);
			artifact.PathRules = _rulesTextBox.Text;
			artifact.Condition = GetConditionFromCheckBox(_win32, _win64, _linux32, _linux64);

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
