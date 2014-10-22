// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;

namespace BuildDependencyManager.TeamCity.RestClasses
{
	public class Property
	{
		// <property name="cleanDestinationDirectory" value="false"/>

		public string Name { get; set; }

		public string Value { get; set; }
	}
}

