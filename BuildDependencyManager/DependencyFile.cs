// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.IO;

using System.IO;
using BuildDependencyManager.TeamCity.RestClasses;
using BuildDependencyManager.TeamCity;
using System.Linq;
using System.Text;


namespace BuildDependencyManager
{
	public static class DependencyFile
	{
		public static void SaveFile(string fileName, List<Artifact> artifacts)
		{
			using (var file = new StreamWriter(fileName))
			{
				file.WriteLine("[[TC]]");
				file.WriteLine("url=build.palaso.org");
				file.WriteLine();

				foreach (var artifact in artifacts)
				{
					file.WriteLine("[TC::{0}]", artifact.Config.Id);
					file.WriteLine("Name={0}", artifact.ConfigName);
					file.WriteLine("RevisionName={0}", artifact.RevisionName);
					file.WriteLine("RevisionValue={0}", artifact.RevisionValue);
					file.WriteLine("Condition={0}", artifact.Condition);
					file.WriteLine("Path={0}", artifact.PathRules);
					file.WriteLine();
				}
			}
		}

		public static List<Artifact> LoadFile(string fileName)
		{
			var artifacts = new List<Artifact>();
			int lineNumber = 0;
			Artifact artifact = null;
			using (var file = new StreamReader(fileName))
			{
				while (!file.EndOfStream)
				{
					var line = file.ReadLine();
					lineNumber++;
					if (line == "[[TC]]")
					{
						line = file.ReadLine();
						lineNumber++;
						// skip this for now
						line = file.ReadLine();
					}
					if (line.StartsWith("["))
					{
						var parts = line.Trim('[', ']').Split(new[] { ':' }, 3);
						if (parts.Length > 2)
						{
							var tc = parts[0];
							var configId = parts[2];
							var config = TeamCityApi.Singleton.GetBuildTypes().First(type => type.Id == configId);
							var proj = TeamCityApi.Singleton.GetAllProjects().First(project => project.Id == config.ProjectId);
							artifact = new Artifact(proj, configId);
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
								Artifact.Conditions condition;
								if (Enum.TryParse<Artifact.Conditions>(parts[1], out condition))
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

