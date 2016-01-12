// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;

namespace BuildDependency.TeamCity.RestClasses
{
	public class BuildType
	{
		// <buildType id="bt392" name="Bloom 2 BETA Publish"
		// href="/guestAuth/app/rest/7.0/buildTypes/id:bt392" projectName="Bloom"
		// projectId="project16" webUrl="http://build.palaso.org/viewType.html?buildTypeId=bt392"/>
		public string Id { get; set; }
		public string Name { get; set; }
		public string Href { get; set; }
		public string ProjectName { get; set; }
		public string ProjectId { get; set; }
		public string WebUrl { get; set; }

		public string IdForArtifacts
		{
			get
			{
				const string buildTypeIdString = "?buildTypeId=";
				if (string.IsNullOrEmpty(WebUrl))
					return null;
				var idIndex = WebUrl.IndexOf(buildTypeIdString, StringComparison.Ordinal);
				if (idIndex < 0)
					return null;
				return WebUrl.Substring(idIndex + buildTypeIdString.Length);
			}
		}

		public override bool Equals(object obj)
		{
			var other = obj as BuildType;
			if (other != null)
			{
				return Id == other.Id;
			}
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode() ^ Id.GetHashCode();
		}

		public override string ToString()
		{
			// WPF doesn't use Text (see https://github.com/picoe/Eto/issues/414), so we have to
			// return the desired value from ToString as well as Text.
			return Text;
			//return string.Format("[BuildType: Id={0}, Name={1}, Href={2}, ProjectName={3}, ProjectId={4}, WebUrl={5}]", Id, Name, Href, ProjectName, ProjectId, WebUrl);
		}

		public string Text { get { return Name; }}
	}
}

