// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;

namespace BuildDependencyManager.TeamCity.RestClasses
{
	//<artifact-dependency id="0" type="artifact_dependency"><properties><property name="cleanDestinationDirectory" value="false"/>
	// <property name="pathRules" value="*.zip!*.*"/><property name="revisionName" value="lastSuccessful"/>
	//<property name="revisionValue" value="latest.lastSuccessful"/><property name="source_buildTypeId" value="bt356"/></properties></artifact-dependency>
	public class ArtifactDependency
	{
		public string Id { get; set; }

		public string Type { get; set; }

		public Properties Properties { get; set; }
	}
}

