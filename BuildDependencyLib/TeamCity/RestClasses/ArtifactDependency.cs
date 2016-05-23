// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using BuildDependency.TeamCity.RestClasses;
using RestSharp.Deserializers;

namespace BuildDependency.TeamCity.RestClasses
{
	//<artifact-dependency id="0" type="artifact_dependency"><properties><property name="cleanDestinationDirectory" value="false"/>
	// <property name="pathRules" value="*.zip!*.*"/><property name="revisionName" value="lastSuccessful"/>
	//<property name="revisionValue" value="latest.lastSuccessful"/><property name="source_buildTypeId" value="bt356"/></properties></artifact-dependency>
	// Newer versions of TC:
	// <artifact-dependency id="0" type="artifact_dependency"> <properties count="4">
	// <property name="cleanDestinationDirectory" value="false"/> <property name="pathRules" value="*=>lib/dotnet"/>
	// <property name="revisionName" value="buildTag"/> <property name="revisionValue" value="glyssen.tcbuildtag"/> </properties>
	// <source-buildType id="bt399" name="geckofx29-win32-continuous" projectName="GeckoFx" projectId="GeckoFx" href="/guestAuth/app/rest/latest/buildTypes/id:bt399" webUrl="http://build.palaso.org/viewType.html?buildTypeId=bt399"/>
	// </artifact-dependency>
	public class ArtifactDependency
	{
		public string Id { get; set; }

		public string Type { get; set; }

		public Properties Properties { get; set; }

		[DeserializeAs(Name = "source-buildType")]
		public BuildType BuildType { get; set; }
	}
}

