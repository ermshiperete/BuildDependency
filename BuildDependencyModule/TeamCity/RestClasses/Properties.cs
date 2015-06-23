﻿// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using BuildDependency.Interfaces;
using RestSharp.Serializers;

namespace BuildDependency.TeamCity.RestClasses
{
	public class Properties
	{
		[SerializeAs(Name = "Property")]
		public List<Property> Property { get; set; }
	}
}

