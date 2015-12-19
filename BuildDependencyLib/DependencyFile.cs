// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BuildDependency.Artifacts;
using BuildDependency.TeamCity;
using BuildDependency.TeamCity.RestClasses;

namespace BuildDependency
{
	public class DependencyFile
	{
		private static DependencyFile _instance;
		private Dictionary<string, Server> _servers;

		private DependencyFile()
		{
			_servers = new Dictionary<string, Server>();
		}

		private static DependencyFile Instance
		{
			get
			{
				if (_instance == null)
					_instance = new DependencyFile();
				return _instance;
			}
		}

		public static void SaveFile(string fileName, List<Server> servers, List<ArtifactTemplate> artifacts)
		{
			InternalSaveFile(fileName, servers, artifacts);
		}

		public static List<ArtifactTemplate> LoadFile(string fileName)
		{
			return Instance.InternalLoadFile(fileName).Result;
		}

		public static async Task<List<ArtifactTemplate>> LoadFileAsync(string fileName)
		{
			return await Instance.InternalLoadFile(fileName);
		}

		private static void InternalSaveFile(string fileName, List<Server> servers, List<ArtifactTemplate> artifactTemplates)
		{
			using (var file = new StreamWriter(fileName))
			{
				foreach (var server in servers)
				{
					file.WriteLine("[[{0}]]", server.Name);
					file.WriteLine("Type={0}", server.ServerType);
					file.WriteLine("Url={0}", server.Url);
					file.WriteLine();
				}

				foreach (var template in artifactTemplates)
				{
					file.WriteLine("[{0}::{1}]", template.Server.Name, template.Config.Id);
					file.WriteLine("Name={0}", template.ConfigName);
					file.WriteLine("RevisionName={0}", template.RevisionName);
					file.WriteLine("RevisionValue={0}", template.RevisionValue);
					file.WriteLine("Condition={0}", template.Condition);
					file.WriteLine("Path={0}", template.PathRules);
					file.WriteLine();
				}
			}
		}

		private int ReadServer(StreamReader file, string line)
		{
			var name = line.Trim('[', ']');
			var lineCount = 0;
			Server server = null;

			while (!file.EndOfStream)
			{
				line = file.ReadLine();
				lineCount++;
				if (string.IsNullOrEmpty(line.Trim()))
					break;

				var parts = line.Split(new [] { '=' }, 2);
				if (parts.Length == 2)
				{
					if (parts[0] == "Type")
					{
						ServerType type;
						if (Enum.TryParse<ServerType>(parts[1], out type))
						{
							server = Server.CreateServer(type);
							server.Name = name;
							_servers.Add(name, server);
						}
						else
							Console.WriteLine("Can't interpret type {0}", parts[1]);
					}
					else if (parts[0] == "Url" && server != null)
					{
						server.Url = parts[1];
					}
				}
			}
			return lineCount;
		}

		private async Task<List<ArtifactTemplate>> InternalLoadFile(string fileName)
		{
			_servers.Clear();
			var artifacts = new List<ArtifactTemplate>();
			int lineNumber = 0;
			ArtifactTemplate artifact = null;
			using (var file = new StreamReader(fileName))
			{
				while (!file.EndOfStream)
				{
					var line = file.ReadLine();
					lineNumber++;
					if (line.StartsWith("[["))
					{
						lineNumber += ReadServer(file, line);
					}
					else if (line.StartsWith("["))
					{
						var parts = line.Trim('[', ']').Split(new[] { ':' }, 3);
						if (parts.Length > 2)
						{
							var tc = parts[0];
							var configId = parts[2];
							var server = _servers[tc] as TeamCityApi;
							var buildTypesTask = server.GetBuildTypesTask();
							var projectsTask = server.GetAllProjectsAsync();
							var buildTypes = await buildTypesTask;
							var projects = await projectsTask;
							var config = buildTypes.First(type => type.Id == configId);
							var proj = projects.First(project => project.Id == config.ProjectId);
							artifact = new ArtifactTemplate(server, proj, configId);
							artifacts.Add(artifact);
						}
						else
						{
							Console.WriteLine("Can't interpret line {0}. Skipping {1}.", lineNumber, line);
						}
					}
					else if (string.IsNullOrEmpty(line.Trim()))
					{
						// ignore
					}
					else
					{
						var parts = line.Split(new []{ '=' }, 2);
						if (parts.Length < 2)
						{
							Console.WriteLine("Can't interpret line {0}. Skipping {1}", lineNumber, line);
							continue;
						}
						switch (parts[0])
						{
							case "RevisionName":
								artifact.RevisionName = parts[1];
								break;
							case "RevisionValue":
								artifact.RevisionValue = parts[1];
								break;
							case "Condition":
								Conditions condition;
								if (Enum.TryParse<Conditions>(parts[1], out condition))
									artifact.Condition = condition;
								else
									Console.WriteLine("Can't interpret condition on line {0}. Skipping {1}", lineNumber, line);
								break;
							case "Path":
								{
									var bldr = new StringBuilder();
									line = parts[1];
									while (!string.IsNullOrEmpty(line) && !file.EndOfStream)
									{
										bldr.AppendLine(line);
										line = file.ReadLine();
									}
									artifact.PathRules = bldr.ToString();
									break;
								}
						}
					}
				}
			}
			return artifacts;
		}
	}
}

