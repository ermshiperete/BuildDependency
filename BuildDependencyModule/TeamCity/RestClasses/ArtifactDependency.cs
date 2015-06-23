// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using BuildDependency.Interfaces;
using RestSharp.Serializers;

namespace BuildDependency.TeamCity.RestClasses
{
	//<artifact-dependency id="0" type="artifact_dependency"><properties><property name="cleanDestinationDirectory" value="false"/>
	// <property name="pathRules" value="*.zip!*.*"/><property name="revisionName" value="lastSuccessful"/>
	//<property name="revisionValue" value="latest.lastSuccessful"/><property name="source_buildTypeId" value="bt356"/></properties></artifact-dependency>
	public class ArtifactDependency : IArtifactDependency
	{
		public string Id { get; set; }

		public string Type { get; set; }

		public List<Property> Properties { get; set; }

		public List<IProperty> ArtifactProperties
		{
			get { return Properties.ConvertAll(p => p as IProperty); }
			set { }
		}
	}
}

