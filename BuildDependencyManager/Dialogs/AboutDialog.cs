// Copyright (c) 2016 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Reflection;
using BuildDependency.Tools;
using Eto.Drawing;
using Eto.Forms;

namespace BuildDependency.Manager.Dialogs
{
	public class AboutDialog: Dialog
	{
		public AboutDialog()
		{
			Title = "About";
			Width = 300;

			AbortButton = new Button();
			AbortButton.Text = "Close";
			AbortButton.Click += (sender, e) => Close();

			var dynamic1 = new DynamicLayout();
			dynamic1.DefaultSpacing = new Size(6, 6);
			dynamic1.Padding = 6;
			dynamic1.BeginVertical();

			var labelTitle = new Label();
			labelTitle.TextAlignment = TextAlignment.Center;
			labelTitle.Font = new Font(labelTitle.Font.Family, labelTitle.Font.Size, FontStyle.Bold);
			labelTitle.Text = GetAttribute(typeof(AssemblyTitleAttribute));
			dynamic1.Add(labelTitle, true);

			var labelVersion = new Label();
			labelVersion.TextAlignment = TextAlignment.Center;

			var version = Utils.GetVersion("BuildDependency.Manager");
			labelVersion.Text = string.Format("{0} ({1}){2}", version.Item1, version.Item2,
#if DEBUG
				" - Debug"
#else
				""
#endif
			);
			dynamic1.Add(labelVersion, true);

			var labelDescription = new Label();
			labelDescription.TextAlignment = TextAlignment.Center;
			labelDescription.Text = GetAttribute(typeof(AssemblyDescriptionAttribute));
			dynamic1.Add(labelDescription, true);

			var labelCopyright = new Label();
			labelCopyright.TextAlignment = TextAlignment.Center;
			labelCopyright.Text = GetAttribute(typeof(AssemblyCopyrightAttribute));
			dynamic1.Add(labelCopyright, true);
			dynamic1.EndVertical();

			var dynamic2 = new DynamicLayout();
			dynamic2.DefaultSpacing = new Size(8, 8);
			dynamic2.Padding = new Padding(6);
			dynamic2.BeginVertical();
			dynamic2.Add(dynamic1, true, true);

			dynamic2.AddSeparateRow(null, AbortButton);

			dynamic2.EndVertical();

			Content = dynamic2;
		}

		private static string GetAttribute(Type type)
		{
			object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(type, false);

			return attributes.Length == 0 ? string.Empty :
				type.GetProperties()[0].GetValue(attributes[0], null).ToString();
		}
	}
}

