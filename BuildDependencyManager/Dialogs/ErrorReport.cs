// Copyright (c) 2016 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Net;
using Eto.Forms;
using BuildDependency.Tools;

namespace BuildDependency.Manager.Dialogs
{
	public partial class ErrorReport
	{
		private Exception _exception;

		public ErrorReport(Exception ex)
		{
			_exception = ex;
			InitializeComponent();
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			_label1.Wrap = WrapMode.Word;
			_label2.Wrap = WrapMode.Word;
			_label3.Wrap = WrapMode.Word;
			DefaultButton.Focus();
		}

		private void OnSendButtonClick(object sender, EventArgs e)
		{
			Result = true;
			Close();
		}

		private void OnAbortButtonClick(object sender, EventArgs e)
		{
			Result = false;
			Close();
		}

		private void OnMoreInfoButtonClick(object sender, EventArgs e)
		{
			MessageBox.Show(string.Format("The following details will be sent:\nhostname={0}\n" +
				"desktop={1}\nshell={2}\nprocessorCount={3}\nuser={4}\n\nStacktrace:\n{5}",
				Dns.GetHostName(), BuildDependency.Tools.Platform.DesktopEnvironment,
				BuildDependency.Tools.Platform.DesktopEnvironmentInfoString, Environment.ProcessorCount,
				ExceptionLogging.Client.Config.UserId, _exception?.StackTrace),
				Application.Instance.Name + " Error Report Details");
		}
	}
}

