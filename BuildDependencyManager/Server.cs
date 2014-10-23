// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;

namespace BuildDependencyManager
{
	public class Server
	{
		public Server(ServerType type)
		{
			ServerType = type;
		}

		public ServerType ServerType { get; set; }
		public string Name { get; set; }
		public string Url { get; set; }

		public override string ToString()
		{
			return Name;
		}
	}
}

