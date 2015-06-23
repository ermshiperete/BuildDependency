// Copyright (c) 2014-2015 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System.Collections.Generic;
using BuildDependency.RestClasses;

namespace BuildDependency.TeamCity.RestClasses
{
	public class Publications
	{
		public List<TeamcityArtifact> Artifact { get; set; }
	}
}
