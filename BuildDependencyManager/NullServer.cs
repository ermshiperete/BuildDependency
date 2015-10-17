// Copyright (c) 2015 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;

namespace BuildDependency
{
	public class NullServer: Server
	{
		public NullServer()
			: base(ServerType.None)
		{
			Name = "-- Add new server --";
		}
	}
}

