// Copyright (c) 2015 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System.Collections.Generic;
using BuildDependency.Interfaces;

namespace BuildDependency.TeamCity.RestClasses
{
	public class ArtifactDependencies
	{
		public List<ArtifactDependency> ArtifactDependency { get; set; }

		public List<IArtifactDependency> Dependencies
		{
			get
			{
				return ArtifactDependency.ConvertAll(d => d as IArtifactDependency);
			}
		}
	}
}
