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
		private readonly Dictionary<string, Server> _servers;

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

		public static List<ArtifactTemplate> LoadFile(string fileName, ILog log)
		{
			return Instance.InternalLoadFile(fileName, log).WaitAndUnwrapException();
		}

		public static async Task<List<ArtifactTemplate>> LoadFileAsync(string fileName, ILog log)
		{
			return await Instance.InternalLoadFile(fileName, log);
		}

		private static void InternalSaveFile(string fileName, List<Server> servers, List<ArtifactTemplate> artifactTemplates)
		{
			using (var file = new StreamWriter(fileName))
			{
				file.WriteLine("# This file lists dependencies");
				file.WriteLine("# It can be edited with the BuildDependencyManager application.");
				file.WriteLine("# See https://github.com/ermshiperete/BuildDependency for more information.");
				file.WriteLine();
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

		private int ReadServer(StreamReader file, string line, ILog log)
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
						if (Enum.TryParse(parts[1], out type))
						{
							server = Server.CreateServer(type);
							server.Name = name;
							_servers.Add(name, server);
						}
						else
							log.LogError("Can't interpret type {0}", parts[1]);
					}
					else if (parts[0] == "Url" && server != null)
					{
						server.Url = parts[1];
					}
				}
			}
			return lineCount;
		}

		private async Task<List<ArtifactTemplate>> InternalLoadFile(string fileName, ILog log)
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
					if (line.StartsWith("[[", StringComparison.InvariantCulture))
					{
						lineNumber += ReadServer(file, line, log);
					}
					else if (line.StartsWith("[", StringComparison.InvariantCulture))
					{
						var parts = line.Trim('[', ']').Split(new[] { ':' }, 3);
						if (parts.Length > 2)
						{
							var tc = parts[0];
							var configId = parts[2];
							if (_servers.ContainsKey(tc))
							{
								var server = _servers[tc] as TeamCityApi;
								var buildTypesTask = server.GetBuildTypesTask();
								var projectsTask = server.GetAllProjectsAsync();
								var buildTypes = await buildTypesTask;
								var projects = await projectsTask;
								var config = buildTypes.FirstOrDefault(type => type.Id == configId);
								if (config != null)
								{
									var proj = projects.FirstOrDefault(project => project.Id == config.ProjectId);
									if (proj != null)
									{
										artifact = new ArtifactTemplate(server, proj, configId);
										artifacts.Add(artifact);
										continue;
									}
								}
							}
						}
						log.LogError("Can't interpret line {0}. Skipping {1}.", lineNumber, line);
					}
					else if (string.IsNullOrEmpty(line.Trim()) ||
						line.Trim().StartsWith("#", StringComparison.InvariantCulture))
					{
						// ignore empty lines and comments
					}
					else
					{
						var parts = line.Split(new []{ '=' }, 2);
						if (parts.Length < 2)
						{
							log.LogError("Can't interpret line {0}. Skipping {1}", lineNumber, line);
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
								if (Enum.TryParse(parts[1], out condition))
									artifact.Condition = condition;
								else
									log.LogError("Can't interpret condition on line {0}. Skipping {1}",
										lineNumber, line);
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

