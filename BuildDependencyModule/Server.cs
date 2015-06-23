﻿// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using BuildDependency.Interfaces;
using BuildDependency.Jenkins;
using BuildDependency.TeamCity;

namespace BuildDependency
{
	public class Server
	{
		public static IServerApi CreateServer(ServerType type)
		{
			if (type == ServerType.TeamCity)
				return new TeamCityApi();
			if (type == ServerType.Jenkins)
				return new JenkinsApi();
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

		public override bool Equals(object obj)
		{
			var otherServer = obj as Server;
			if (otherServer == null)
				return false;

			return ServerType == otherServer.ServerType && Name == otherServer.Name &&
				Url == otherServer.Url;
		}

		public override int GetHashCode()
		{
			return ServerType.GetHashCode() ^ Name.GetHashCode() ^ Url.GetHashCode();
		}
	}
}

