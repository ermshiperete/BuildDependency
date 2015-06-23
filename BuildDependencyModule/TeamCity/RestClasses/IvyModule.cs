// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using BuildDependency.TeamCity.RestClasses;

namespace BuildDependency.RestClasses
{
	public class IvyModule
	{
		//public string Version { get; set; }
		public Info Info { get; set; }
		//public List<TeamcityArtifact> Publications { get; set; }
		public Publications Publications { get; set; }
	}
}

