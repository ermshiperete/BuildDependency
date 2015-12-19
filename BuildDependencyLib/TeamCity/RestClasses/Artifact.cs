// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;

namespace BuildDependency.RestClasses
{
	public class Artifact
	{
		public string Name { get; set; }
		public string Type { get; set; }
		public string Ext { get; set; }

		public override string ToString()
		{
			return string.Format("{0}{1}{2}", Name, string.IsNullOrEmpty(Ext) ? "" : ".", Ext);
		}
	}
}

