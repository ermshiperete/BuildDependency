// Copyright (c) 2014-2016 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BuildDependency.Artifacts;
using BuildDependency.Manager.Tools;
using BuildDependency.TeamCity;
using BuildDependency.TeamCity.RestClasses;
using BuildDependency.Tools;
using Eto.Drawing;
using Eto.Forms;

namespace BuildDependency.Manager.Dialogs
{
	public class BuildDependencyManagerDialog: Form
	{
		private List<ArtifactTemplate> _artifacts;
		private List<Server> _servers;
		private readonly GridView _gridView;
		private SelectableFilterCollection<ArtifactTemplate> _dataStore;
		private readonly Spinner _spinner;
		private string _fileName;
		private bool _fileWaitingToBeLoaded;

		public BuildDependencyManagerDialog(string fileName = null)
		{
			Application.Instance.UnhandledException += OnUnhandledException;
			Application.Instance.Initialized += OnApplicationInitialized;
			Application.Instance.Name = "Build Dependency Manager";
			_artifacts = new List<ArtifactTemplate>();
			Title = "Build Dependency Manager";
			ClientSize = new Size(700, 400);

			Menu = new MenuBar
			{
				Items =
				{
					new ButtonMenuItem
					{
						Text = "&File",
						Items =
						{
							new Command(OnFileNew) { MenuText = "&New" },
							new Command(OnFileOpen) { MenuText = "&Open..." },
							new Command(OnFileSave) { MenuText = "&Save" },
							new Command(OnFileSaveAs) { MenuText = "Save &As..." },
							new Command(OnFileImport) { MenuText = "&Import..." },
						}
					},
					new ButtonMenuItem
					{
						Text = "&Tools",
						Items =
						{
							new Command(OnToolsServers) { MenuText = "&Servers" },
							new Command(OnToolsSort) { MenuText = "S&ort" }
						}
					},
					new ButtonMenuItem
					{
						Text = "&Help",
						Items =
						{
							new Command(OnHelpAbout) { MenuText = "&About" }
						}
					}
				},
				QuitItem = new Command((sender, e) => Application.Instance.Quit())
				{
					MenuText = "E&xit", 
				}
			};

			_spinner = new Spinner { Size = new Size(30, 30), Visible = false };
			_gridView = new GridView();
			_gridView.GridLines = GridLines.Both;
			_gridView.ShowHeader = true;
			_gridView.Size = new Size(680, 350);
			_dataStore = new SelectableFilterCollection<ArtifactTemplate>(_gridView, _artifacts);
			_gridView.DataStore = _dataStore;
			_gridView.CellDoubleClick += OnEditArtifact;
			_gridView.Columns.Add(new GridColumn
				{
					HeaderText = "Artifacts source",
					DataCell = new TextBoxCell("Source"),
					AutoSize = true,
					Resizable = true,
					Sortable = true,
				});
			_gridView.Columns.Add(new GridColumn
				{
					HeaderText = "Artifacts path",
					DataCell = new TextBoxCell("PathRules"),
					//AutoSize = true,
					Resizable = true,
					Sortable = true
				});
			_gridView.ContextMenu = new ContextMenu(
				new ButtonMenuItem(new Command(OnEditArtifact) { MenuText = "&Edit" }),
				new ButtonMenuItem(new Command(OnDeleteArtifact) { MenuText = "&Delete" }));

			var button = new Button { Text = "Add Artifact" };
			button.Click += OnAddArtifact;

			var content = new TableLayout
			{
				Padding = new Padding(10, 10, 10, 5),
				Spacing = new Size(5, 5),
				Rows =
				{
					new TableRow(_gridView) { ScaleHeight = true },
					new StackLayout
					{
						Orientation = Orientation.Horizontal,
						Spacing = 5,
						Items = { _spinner, null, button }
					}
				}
			};

			Content = content;

			if (string.IsNullOrEmpty(fileName))
				OnFileNew(this, EventArgs.Empty);
			else
			{
				FileName = fileName;
				_fileWaitingToBeLoaded = true;
			}
		}

		private void OnApplicationInitialized(object sender, EventArgs e)
		{
			if (_fileWaitingToBeLoaded)
			{
				using (new WaitSpinner(_spinner))
				{
					OpenFile(FileName);
					_fileWaitingToBeLoaded = false;
				}
			}
		}

		private void OnUnhandledException(object sender, Eto.UnhandledExceptionEventArgs e)
		{
			var ex = e.ExceptionObject as Exception;
			var errorReport = new ErrorReport(ex);
			if (errorReport.ShowModal(this))
			{
				if (ex != null)
				{
					ExceptionLogging.Client.Config.Metadata.AddToTab("Exception Details",
						"stacktrace", ex.StackTrace);
					ExceptionLogging.Client.Notify(ex, Bugsnag.Severity.Error);
				}
			}
			Application.Instance.Quit();
		}

		private void OnAddArtifact(object sender, EventArgs e)
		{
			using (new WaitSpinner(_spinner))
			{
				using (var dlg = new AddOrEditArtifactDependencyDialog(true, _servers))
				{
					dlg.ShowModal(this);

					if (dlg.Result)
					{
						var artifact = dlg.GetArtifact();
						_dataStore.Add(artifact);
					}
				}
			}
		}

		private void OnEditArtifact(object sender, EventArgs e)
		{
			var item = _gridView.SelectedItem as ArtifactTemplate;
			if (item == null)
				return;
			var artifactIndex = _dataStore.IndexOf(item);
			using (var dlg = new AddOrEditArtifactDependencyDialog(_servers, item))
			{
				dlg.ShowModal();
				if (dlg.Result)
				{
					var artifact = dlg.GetArtifact();
					_dataStore[artifactIndex] = artifact;
				}
			}
		}

		private void OnDeleteArtifact(object sender, EventArgs e)
		{
			var item = _gridView.SelectedItem as ArtifactTemplate;
			if (item == null)
				return;
			_dataStore.Remove(item);
		}

		private string FileName
		{
			get { return _fileName; }
			set
			{
				_fileName = value;
				Title = string.Format("Build Dependency Manager - {0}", Path.GetFileName(_fileName));
			}
		}

		private void OnFileNew(object sender, EventArgs e)
		{
			_dataStore.Clear();
			_servers = new List<Server>();
			var server = Server.CreateServer(ServerType.TeamCity);
			server.Name = "TC";
			server.Url = "http://build.palaso.org";
			_servers.Add(server);
		}

		private static void AddFileFilters(FileDialog dlg)
		{
			dlg.Filters.Add(new FileDialogFilter("Dependency File (*.dep)", "*.dep"));
			dlg.Filters.Add(new FileDialogFilter("All Files (*.*)", "*"));
			dlg.CurrentFilterIndex = 0;
		}

		public async void OpenFile(string fileName)
		{
			FileName = fileName;
			await LoadFileAsync(FileName);
		}

		private async void OnFileOpen(object sender, EventArgs e)
		{
			using (new WaitSpinner(_spinner))
			{
				using (var dlg = new OpenFileDialog())
				{
					AddFileFilters(dlg);
					if (dlg.ShowDialog(this) != DialogResult.Ok)
						return;

					FileName = dlg.FileName;

					await LoadFileAsync(FileName);
				}
			}
		}

		private async Task LoadFileAsync(string filename)
		{
			_dataStore.Clear();
			_dataStore.AddRange(await DependencyFile.LoadFileAsync(filename));
		}

		private async void OnFileSave(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(FileName))
			{
				OnFileSaveAs(sender, e);
				return;
			}

			var fileName = FileName;
			await Task.Run(() =>
				{
					DependencyFile.SaveFile(fileName, _servers, _artifacts);
					JobsFile.WriteJobsFile(Path.ChangeExtension(fileName, ".files"), _artifacts);
				});
		}

		private async void OnFileSaveAs(object sender, EventArgs e)
		{
			using (var dlg = new SaveFileDialog())
			{
				AddFileFilters(dlg);

				if (dlg.ShowDialog(this) != DialogResult.Ok)
					return;

				var fileName = dlg.FileName;
				using (new WaitSpinner(_spinner))
				{
					await Task.Run(() =>
						{
							if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
								fileName += ".dep";
							DependencyFile.SaveFile(fileName, _servers, _artifacts);
							JobsFile.WriteJobsFile(Path.ChangeExtension(fileName, ".files"), _artifacts);
						});
					FileName = fileName;
				}
			}
		}

		private async void OnFileImport(object sender, EventArgs e)
		{
			using (new WaitSpinner(_spinner))
			{
				using (var dlg = new ImportDialog(_servers))
				{
					dlg.ShowModal();
					if (dlg.Result)
					{
						var configId = dlg.SelectedBuildConfig;
						var condition = dlg.Condition;

						var server = dlg.Server as TeamCityApi;
						if (server == null)
							return;
						await Task.Run(async () =>
							{
								foreach (var dep in await server.GetArtifactDependenciesAsync(configId))
								{
									if (dep == null)
										continue;
									var artifact = new ArtifactTemplate(server, new ArtifactProperties(dep.Properties), dep.BuildType);
									artifact.Condition = condition;
									_dataStore.Add(artifact);
								}
							});
					}
				}
			}
		}

		private void OnToolsServers(object sender, EventArgs e)
		{
			using (var dlg = new ServersDialog(_servers))
			{
				dlg.ShowModal();
				if (dlg.Result)
				{
					_servers = dlg.Servers;
				}
			}
		}

		private async void OnToolsSort(object sender, EventArgs e)
		{
			using (new WaitSpinner(_spinner))
			{
				await Task.Run(() =>
					{
						_dataStore.Sort = (x, y) => string.Compare(x.ConfigName, y.ConfigName, StringComparison.Ordinal);
					});
			}
		}

		private void OnHelpAbout(object sender, EventArgs e)
		{
			using (var dlg = new AboutDialog())
			{
				dlg.ShowModal();
			}
		}
	}
}
