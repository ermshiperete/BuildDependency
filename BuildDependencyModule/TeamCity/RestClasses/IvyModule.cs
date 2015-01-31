// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;

namespace BuildDependency.RestClasses
{
	public class Info
	{
		public string Organisation { get; set; }
		public string Module { get; set; }
		public string Revision { get; set; }
	}

	public class Publications
	{
		public List<Artifact> Artifact { get; set; }
	}
	public class IvyModule
	{
		//public string Version { get; set; }
		public Info Info { get; set; }
		//public List<Artifact> Publications { get; set; }
		public Publications Publications { get; set; }
	}
}

