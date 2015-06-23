// Copyright (c) 2015 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System.Collections.Generic;
using BuildDependency.Interfaces;

namespace BuildDependency.TeamCity.RestClasses
{
	class BuildTypes
	{
		public List<BuildType> BuildType { get; set; }

		public List<IBuildJob> BuildJobs { get { return BuildType.ConvertAll(j => j as IBuildJob); } }
	}
}
