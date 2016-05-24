// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using BuildDependency.Manager.Dialogs;
using BuildDependency.Tools;
using Eto.Forms;

namespace BuildDependency.Manager
{
	class MainClass
	{
		[STAThread]
		public static void Main(string[] args)
		{
			ExceptionLogging.Initialize("4bae82b8c647df7fea786dbaecb4b351");
			string fileName = args.Length > 0 ? args[0] : null;
			new Application().Run(new BuildDependencyManagerDialog(fileName));
		}
	}
}
