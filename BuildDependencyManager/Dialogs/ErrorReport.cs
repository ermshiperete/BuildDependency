// Copyright (c) 2016 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Net;
using Eto.Forms;
using BuildDependency.Tools;
using PlatformTools = BuildDependency.Tools.Platform;

namespace BuildDependency.Manager.Dialogs
{
	public partial class ErrorReport
	{
		private readonly Exception _exception;

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
			var runtime = PlatformTools.IsMono
				? $"Mono\nmonoversion={PlatformTools.MonoVersion}" : ".NET";
			MessageBox.Show(
				$"The following details will be sent:\nhostname={Dns.GetHostName()}\n" +
				$"desktop={PlatformTools.DesktopEnvironment}\n" +
				$"shell={PlatformTools.DesktopEnvironmentInfoString}\n" +
				$"processorCount={Environment.ProcessorCount}\n" +
				$"user={ExceptionLogging.Client.Config.UserId}\n" +
				$"runtime=${runtime}\n\nStacktrace:\n{_exception?.StackTrace}",
				$"{Application.Instance.Name} Error Report Details");
		}
	}
}

