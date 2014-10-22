// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using Xwt;
using System.Collections.Generic;
using BuildDependencyManager.TeamCity;
using BuildDependencyManager.TeamCity.RestClasses;
using BuildDependencyManager.Widgets;
using Xwt.GtkBackend;
using Xwt.Backends;
using Xwt.Drawing;
using System.Globalization;

namespace BuildDependencyManager
{
	public class BuildDependencyManagerDialog: Dialog
	{
		private List<Artifact> _artifacts;
		private ListStore _store;
		private readonly ListView _listView;
		private readonly Table _table;
		private DataField<string> _artifactsSource;
		private DataField<string> _artifactsPath;

		private int _rowCount;

		public BuildDependencyManagerDialog()
		{
			Title = "Build Dependency Manager";
			Width = 700;
			Height = 400;

			var menu = new Menu();
			MainMenu = menu;
			var fileMenu = new MenuItem("_File");
			fileMenu.SubMenu = new Menu();

			var collection = fileMenu.SubMenu.Items;
			var menuItem = new MenuItem("_New");
			menuItem.Clicked += OnFileNew;
			collection.Add(menuItem);
			menuItem = new MenuItem("_Open");
			menuItem.Clicked += OnFileOpen;
			fileMenu.SubMenu.Items.Add(menuItem);
			menuItem = new MenuItem("_Save");
			menuItem.Clicked += OnFileSave;
			fileMenu.SubMenu.Items.Add(menuItem);
			menuItem = new MenuItem("_Import");
			menuItem.Clicked += OnImport;
			fileMenu.SubMenu.Items.Add(menuItem);
			menuItem = new MenuItem("E_xit");
			menuItem.Clicked += (sender, e) => Close();
			fileMenu.SubMenu.Items.Add(menuItem);

			menu.Items.Add(fileMenu);

			var button = new DialogButton("Add Artifact");
			button.Clicked += OnAddArtifact;
			Buttons.Add(button);

			var vbox = new VBox();

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
			_listView.ButtonPressed += HandleButtonPressed;

			_store = new ListStore(_artifactsSource, _artifactsPath);
			_artifacts = new List<Artifact>();
			_listView.DataSource = _store;

			vbox.PackStart(_listView, true);

			Content = vbox;
		}

		private void AddArtifactToStore(int row, Artifact artifact)
		{
			var source = string.Format("{0}\n({1})", artifact.ConfigName, artifact.TagLabel);
			if ((artifact.Condition & Artifact.Conditions.All) != Artifact.Conditions.All && artifact.Condition != Artifact.Conditions.None)
				source = string.Format("{0}\nCondition: {1}", source, artifact.Condition);
			_store.SetValue<string>(row, _artifactsSource, source);
			_store.SetValue<string>(row, _artifactsPath, artifact.PathRules);
		}

		private void OnAddArtifact(object sender, EventArgs e)
		{
			using (var dlg = new AddOrEditArtifactDependency(true))
			{
				if (dlg.Run() == Command.Ok)
				{
					var artifact = dlg.GetArtifact();
					_artifacts.Add(artifact);
					int row = _store.AddRow();
					AddArtifactToStore(row, artifact);
				}
			}
		}

		private void OnEditArtifact(object sender, EventArgs e)
		{
			int row = (int)((MenuItem)sender).Tag;
			Console.WriteLine("Editing row {0}", row);
			if (row >= _artifacts.Count)
				return;
			using (var dlg = new AddOrEditArtifactDependency(_artifacts[row]))
			{
				if (dlg.Run() == Command.Ok)
				{
					var artifact = dlg.GetArtifact();
					_artifacts[row] = artifact;
					_store.InsertRowAfter(row);
					_store.RemoveRow(row);
					AddArtifactToStore(row, artifact);
				}
			}
		}

		private void OnDeleteArtifact(object sender, EventArgs e)
		{
			Console.WriteLine("Deleting row {0}", ((MenuItem)sender).Tag);
			_store.RemoveRow((int)((MenuItem)sender).Tag);
		}

		private void OnFileNew(object sender, EventArgs e)
		{
			_store.Clear();
			_artifacts = new List<Artifact>();
		}

		private void OnFileOpen(object sender, EventArgs e)
		{
			string fileName = null;
			Content.Cursor = CursorType.Wait;
			using (var dlg = new OpenFileDialog())
			{
				dlg.Filters.Add(new FileDialogFilter("Dependency File", "*.dep"));
				dlg.Filters.Add(new FileDialogFilter("All Files", "*"));
				dlg.ActiveFilter = dlg.Filters[0];
				if (dlg.Run())
				{
					fileName = dlg.FileName;
				}
			}
			Application.MainLoop.DispatchPendingEvents();
			try
			{
				_store.Clear();
				_artifacts = DependencyFile.LoadFile(fileName);
				foreach (var artifact in _artifacts)
				{
					int row = _store.AddRow();
					AddArtifactToStore(row, artifact);
				}
			}
			finally
			{
				Content.Cursor = CursorType.Arrow;
			}
		}

		private void OnFileSave(object sender, EventArgs e)
		{
			using (var dlg = new SaveFileDialog())
			{
				dlg.Filters.Add(new FileDialogFilter("Dependency File", "*.dep"));
				dlg.Filters.Add(new FileDialogFilter("All Files", "*"));
				dlg.ActiveFilter = dlg.Filters[0];

				if (dlg.Run())
				{
					DependencyFile.SaveFile(dlg.FileName, _artifacts);
				}
			}
		}

		private void OnImport(object sender, EventArgs e)
		{
			using (var dlg = new ImportDialog())
			{
				if (dlg.Run() == Command.Ok)
				{
					var configId = dlg.SelectedBuildConfig;
					var condition = dlg.Condition;

					foreach (var dep in TeamCityApi.Singleton.GetArtifactDependencies(configId))
					{
						var artifact = new Artifact(new ArtifactProperties(dep.Properties));
						artifact.Condition = condition;
						_artifacts.Add(artifact);
						int row = _store.AddRow();
						AddArtifactToStore(row, artifact);
					}

				}
			}
		}

		private void AddTableRow(Artifact artifact)
		{
			//_table.InsertRow(_rowCount, _rowCount + 1);
			//_table.Add(new TextEntry { ReadOnly = true, Text = string.Format("{0}\n({1})", artifact.ConfigName, artifact.Tag) }, 0, _rowCount);
			_table.Add(new TableCell<Label>(new Label { Text = string.Format("{0}\n({1})", artifact.ConfigName, artifact.TagLabel) }), 0, _rowCount);
			_table.Add(new TableCell<Label>(new Label { Text = artifact.PathRules }), 1, _rowCount);
			var linkLabel = new LinkLabel { Text = "Edit" };
			linkLabel.Tag = _rowCount;
			linkLabel.LinkClicked += (sender, e) => Console.WriteLine("Edit row {0}!", _rowCount);
			_table.Add(new TableCell<LinkLabel>(linkLabel, 50), 2, _rowCount);
			linkLabel = new LinkLabel { Text = "Delete" };
			linkLabel.Tag = _rowCount;
			linkLabel.LinkClicked += (sender, e) => Console.WriteLine("Delete row {0}!", _rowCount);
			_table.Add(new TableCell<LinkLabel>(linkLabel, 50), 3, _rowCount);
			_rowCount++;
		}

		private void HandleButtonPressed(object sender, ButtonEventArgs e)
		{
			if (e.Button != PointerButton.Right)
				return;

			var row = _listView.GetRowAtPosition(e.Position);
			var menu = new Menu();
			var menuItem = new MenuItem("_Edit");
			menuItem.Tag = row;
			menuItem.Clicked += OnEditArtifact;
			menu.Items.Add(menuItem);
			menuItem = new MenuItem("_Delete");
			menuItem.Tag = row;
			menuItem.Clicked += OnDeleteArtifact;
			menu.Items.Add(menuItem);
			menu.Popup();
		}


	}

}

