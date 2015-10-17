// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using BuildDependency.Dialogs;
using Eto.Forms;

namespace BuildDependency
{
	class MainClass
	{
		[STAThread]
		public static void Main(string[] args)
		{
			new Application().Run(new BuildDependencyManagerDialog());
		}

	}
}
