// Copyright (c) 2015 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)

using System.Collections.Generic;

namespace BuildDependency.Interfaces
{
	public interface IArtifactDependency
	{
		string Id { get; set; }
		string Type { get; set; }
		List<IProperty> ArtifactProperties { get; set; }
	}
}