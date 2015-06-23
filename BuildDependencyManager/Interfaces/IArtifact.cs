// Copyright (c) 2015 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;

namespace BuildDependency.Interfaces
{
	public interface IArtifact
	{
		string Name { get; set; }
		string Type { get; set; }
		string Ext { get; set; }
	}
}

