// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using BuildDependency.TeamCity;

namespace BuildDependency
{
	public class Server
	{
		public static Server CreateServer(ServerType type)
		{
			if (type == ServerType.TeamCity)
				return new TeamCityApi();
			throw new ArgumentException("Unknown server type");
		}


		protected Server(ServerType type)
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

