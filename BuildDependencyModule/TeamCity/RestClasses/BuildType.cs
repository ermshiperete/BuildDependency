// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using BuildDependency.Interfaces;
using RestSharp.Serializers;

namespace BuildDependency.TeamCity.RestClasses
{
	[SerializeAs(Name = "buildType")]
	public class BuildType: IBuildJob
	{
		// <buildType id="bt392" name="Bloom 2 BETA Publish"
		// href="/guestAuth/app/rest/7.0/buildTypes/id:bt392" projectName="Bloom"
		// projectId="project16" webUrl="http://build.palaso.org/viewType.html?buildTypeId=bt392"/>
		public string Id { get; set; }
		public string Name { get; set; }
		public string Href { get; set; }
		public string ProjectName { get; set; }
		public string ProjectId { get; set; }

		[SerializeAs(Name = "webUrl")]
		public string Url { get; set; }

		public override bool Equals(object obj)
		{
			var other = obj as BuildType;
			if (other == null)
				return false;

			return Id == other.Id;
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}

		public override string ToString()
		{
			return string.Format("[BuildType: Id={0}, Name={1}, Href={2}, ProjectName={3}, ProjectId={4}, Url={5}]", Id, Name, Href, ProjectName, ProjectId, Url);
		}

	}
}

