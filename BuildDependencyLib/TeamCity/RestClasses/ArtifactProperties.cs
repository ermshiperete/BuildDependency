// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;

namespace BuildDependency.TeamCity.RestClasses
{
	public class ArtifactProperties
	{
		public ArtifactProperties()
		{
		}

		public ArtifactProperties(Properties properties)
		{
			foreach (var prop in properties.Property)
			{
				switch (prop.Name)
				{
					case "cleanDestinationDirectory":
						CleanDestinationDirectory = Convert.ToBoolean(prop.Value);
						break;
					case "pathRules":
						PathRules = prop.Value;
						break;
					case "revisionName":
						RevisionName = prop.Value;
						break;
					case "revisionValue":
						RevisionValue = prop.Value;
						break;
					case "source_buildTypeId":
						SourceBuildTypeId = prop.Value;
						break;
				}
			}
		}

		public bool CleanDestinationDirectory { get; set; }

		public string PathRules { get; set; }

		public string RevisionName { get; set; }

		public string RevisionValue { get; set; }

		public string SourceBuildTypeId { get; set; }

		public string TagLabel
		{
			get
			{
				switch (RevisionName)
				{
					case "lastSuccessful":
						return "Last successful build";
					case "lastPinned":
						return "Last pinned build";
					case "lastFinished":
						return "Last finished build";
					case "buildNumber":
						return string.Format("Build #{0}", RevisionValue);
					case "buildTag":
						return Tag;
				}
				return string.Empty;
			}
		}

		public string Tag
		{
			get
			{
				if (RevisionName == "buildTag")
					return RevisionValue.Substring(0, RevisionValue.Length - ".tcbuildtag".Length);
				return string.Empty;
			}
		}
	}
}

