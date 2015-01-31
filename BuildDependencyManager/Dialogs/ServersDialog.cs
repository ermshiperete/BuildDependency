// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using Xwt;

namespace BuildDependency.Dialogs
{
	public class ServersDialog: Dialog
	{
		private readonly ComboBox _serversCombo;
		private readonly ComboBox _serverType;
		private readonly TextEntry _name;
		private readonly TextEntry _url;
		private bool _inSelectedServerChanged;

		public ServersDialog(List<Server> servers)
		{
			Buttons.Add(new DialogButton("OK", Command.Ok));
			Buttons.Add(new DialogButton("Cancel", Command.Cancel));

			var vbox = new VBox();
			var hbox = new HBox();
			hbox.PackStart(new Label("Available Servers:"));
			_serversCombo = new ComboBox();
			_serversCombo.SelectionChanged += OnSelectedServerChanged;

			foreach (var server in servers)
			{
				_serversCombo.Items.Add(server);
			}
			_serversCombo.Items.Add("Add new server");
			hbox.PackStart(_serversCombo);
			vbox.PackStart(hbox);

			vbox.PackStart(new HSeparator());

			vbox.PackStart(new Label("Type:"));
			_serverType = new ComboBox();
			_serverType.Items.Add("TeamCity");
			_serverType.SelectedIndex = 0;
			_serverType.SelectionChanged += OnServerTypeChanged;
			vbox.PackStart(_serverType);

			vbox.PackStart(new Label("Name:"));
			_name = new TextEntry();
			_name.Changed += OnNameChanged;
			vbox.PackStart(_name);

			vbox.PackStart(new Label("URL:"));
			_url = new TextEntry();
			_url.Changed += OnUrlChanged;
			vbox.PackStart(_url);

			Content = vbox;

			if (_serversCombo.Items.Count > 0)
				_serversCombo.SelectedIndex = 0;
		}

		private void OnUrlChanged (object sender, EventArgs e)
		{
			var server = _serversCombo.SelectedItem as Server;
			if (server != null)
				server.Url = _url.Text;
		}

		private void OnNameChanged(object sender, EventArgs e)
		{
			if (_inSelectedServerChanged)
				return;
			_inSelectedServerChanged = true;
			var server = _serversCombo.SelectedItem as Server;
			if (server != null)
			{
				var selectedIndex = _serversCombo.SelectedIndex;
				_serversCombo.Items.Remove(server);
				Application.MainLoop.DispatchPendingEvents();
				server.Name = _name.Text;
				_serversCombo.Items.Insert(selectedIndex, server);
				_serversCombo.SelectedIndex = selectedIndex;
			}

			_inSelectedServerChanged = false;
		}

		private void OnServerTypeChanged(object sender, EventArgs e)
		{
			
		}

		private void OnSelectedServerChanged(object sender, EventArgs e)
		{
			if (_inSelectedServerChanged)
				return;
			_inSelectedServerChanged = true;
			var server = _serversCombo.SelectedItem as Server;
			if (server == null)
			{
				var selectedIndex = _serversCombo.SelectedIndex;
				_serversCombo.Items.Insert(selectedIndex, Server.CreateServer(ServerType.TeamCity));
				_serversCombo.SelectedIndex = selectedIndex;
				_serverType.SelectedIndex = 0;
				_name.Text = string.Empty;
				_url.Text = string.Empty;
			}
			else
			{
				_serverType.SelectedIndex = (int)server.ServerType;
				_name.Text = server.Name;
				_url.Text = server.Url;
			}
			_inSelectedServerChanged = false;
			Application.MainLoop.DispatchPendingEvents();
		}

		public List<Server> Servers
		{
			get
			{
				var servers = new List<Server>();
				foreach (var obj in _serversCombo.Items)
				{
					var server = obj as Server;
					if (server != null)
						servers.Add(server);
				}
				return servers;
			}
		}
	}
}

