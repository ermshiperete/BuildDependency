// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using BuildDependency.Interfaces;

namespace BuildDependency.TeamCity.RestClasses
{
	public class Projects
	{
		public List<Project> Project { get; set; }

		public List<IProject> IProject { get { return Project.ConvertAll(p => p as IProject); } }
	}
}

