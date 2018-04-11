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
			MessageBox.Show(
				$"The following details will be sent:\n{ExceptionLogging.Client.DataThatWillBeSent}\n" +
				$"Exception: {_exception?.GetType().Name}\n{_exception?.Message}\n\n" +
				$"Stacktrace:\n{_exception?.StackTrace}",
				$"{Application.Instance.Name} Error Report Details");
		}
	}
}

