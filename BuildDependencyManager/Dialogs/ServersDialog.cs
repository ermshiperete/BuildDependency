// Copyright (c) 2014-2015 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using Eto.Drawing;
using Eto.Forms;

namespace BuildDependency.Manager.Dialogs
{
	public class ServersDialog: Dialog<bool>
	{
		private readonly ComboBox _serversCombo;
		private readonly ComboBox _serverType;
		private readonly TextBox _name;
		private readonly TextBox _url;
		private readonly List<Server> _servers;

		public ServersDialog(List<Server> servers)
		{
			_servers = servers;
			_servers.Insert(servers.Count, new NullServer());

			Resizable = true;

			_serverType = new ComboBox();
			_serverType.Items.Add("TeamCity");
			_serverType.SelectedIndex = 0;
			_name = new TextBox();
			_url = new TextBox();

			_serversCombo = new ComboBox();
			_serversCombo.SelectedIndexChanged += OnSelectedServerChanged;
			_serversCombo.DataStore = _servers;
			_serversCombo.SelectedIndex = 0;

			var okButton = new Button { Text = "OK" };
			okButton.Click += OnOk;
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

		private void OnOk (object sender, EventArgs e)
		{
			var server = _serversCombo.SelectedValue as Server;
			if (server == null || server is NullServer)
			{
				var selectedIndex = _serversCombo.SelectedIndex;

				server = Server.CreateServer((ServerType)Enum.Parse(typeof(ServerType), _serverType.SelectedKey));
				server.Name = _name.Text;
				var url = _url.Text;
				Uri uri;
				if (!string.IsNullOrEmpty(url) && !Uri.TryCreate(url, UriKind.Absolute, out uri))
				{
					url = "http://" + url;
				}
				server.Url = url;
				_servers.Insert(selectedIndex, server);
			}

			Result = true;
			Close();
		}

		private void OnSelectedServerChanged(object sender, EventArgs e)
		{
			var server = _serversCombo.SelectedValue as Server;
			if (server == null || server is NullServer)
			{
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

