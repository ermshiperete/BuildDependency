// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using Xwt;
using BuildDependencyManager.Dialogs;

namespace BuildDependencyManager
{
	class MainClass
	{
		[STAThread]
		public static void Main(string[] args)
		{
			Application.Initialize();
			using (var dlg = new BuildDependencyManagerDialog())
			{
				dlg.Run();
			}
			Application.Dispose();
		}

	}
}
