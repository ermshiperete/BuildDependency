// Copyright (c) 2014-2015 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;

namespace BuildDependency.Dialogs
{
	public class ServersDialog: Dialog<bool>
	{
		private readonly ComboBox _serversCombo;
		private readonly ComboBox _serverType;
		private readonly TextBox _name;
		private readonly TextBox _url;
		private bool _inSelectedServerChanged;
		private readonly List<Server> _servers;

		public ServersDialog(List<Server> servers)
		{
			_servers = servers;
			_servers.Insert(servers.Count, new NullServer());

			Resizable = true;

			_serverType = new ComboBox();
			_serverType.Items.Add("TeamCity");
			_serverType.SelectedIndex = 0;
			_serverType.SelectedIndexChanged += OnServerTypeChanged;

			_name = new TextBox();
			_name.TextChanged += OnNameChanged;

			_url = new TextBox();
			_url.TextChanged += OnUrlChanged;

			_serversCombo = new ComboBox();
			_serversCombo.SelectedIndexChanged += OnSelectedServerChanged;
			_serversCombo.DataStore = _servers;
			_serversCombo.SelectedIndex = 0;

			var okButton = new Button { Text = "OK" };
			okButton.Click += (sender, e) =>
			{
				Result = true;
				Close();
			};
			var cancelButton = new Button { Text = "Cancel" };
			cancelButton.Click += (sender, e) => Close();

			var table = new TableLayout
			{
				Padding = new Padding(10, 10, 10, 5),
				Spacing = new Size(3, 3),
				Rows =
				{
					new TableRow(new Label { Text = "Available Servers:"}, _serversCombo),
					new TableRow(),
					new TableRow(new Label { Text = "Type:"}, _serverType),
					new TableRow(new Label { Text = "Name:"}, _name),
					new TableRow(new Label { Text = "URL:" }, _url),
					new TableRow(),
					new TableRow(null, new StackLayout
						{
							Orientation = Orientation.Horizontal,
							Spacing = 5,

							Items = { null, okButton, cancelButton }
						})
				}
			};

			Content = table;

			DefaultButton = okButton;
			AbortButton = cancelButton;

//			if (_serversCombo.Items.Count > 0)
//				_serversCombo.SelectedIndex = 0;
		}

		private void OnUrlChanged (object sender, EventArgs e)
		{
			var server = _serversCombo.SelectedValue as Server;
			if (server != null)
				server.Url = _url.Text;
		}

		private void OnNameChanged(object sender, EventArgs e)
		{
			if (_inSelectedServerChanged)
				return;
			_inSelectedServerChanged = true;
			var server = _serversCombo.SelectedValue as Server;
			if (server != null)
			{
				var selectedIndex = _serversCombo.SelectedIndex;
				_servers.Remove(server);
				server.Name = _name.Text;
				_servers.Insert(selectedIndex, server);
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
			var server = _serversCombo.SelectedValue as Server;
			if (server == null || server is NullServer)
			{
				var selectedIndex = _serversCombo.SelectedIndex;
				_servers.Insert(selectedIndex, Server.CreateServer(ServerType.TeamCity));
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
		}

		public List<Server> Servers
		{
			get
			{
				_servers.Remove(new NullServer());
				return _servers;
			}
		}
	}
}

