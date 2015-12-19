// Copyright (c) 2015 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using Eto.Forms;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace BuildDependency.Manager.Tools
{
	public class WaitSpinner: IDisposable
	{
		private Spinner _Spinner;

		public WaitSpinner(Spinner spinner)
		{
			_Spinner = spinner;
			_Spinner.Visible = _Spinner.Enabled = true;
		}

		public WaitSpinner(Control control)
		{

		}

		#region IDisposable implementation

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing && _Spinner != null)
			{
				_Spinner.Visible = _Spinner.Enabled = false;
			}
			_Spinner = null;
		}

		#endregion
	}
}

