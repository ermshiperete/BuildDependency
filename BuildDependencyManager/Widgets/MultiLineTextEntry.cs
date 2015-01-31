// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using Xwt;
using Xwt.Formats;
#if __MonoCS__
using Xwt.GtkBackend;
#endif

namespace BuildDependency.Widgets
{
	public class MultiLineTextEntry: RichTextView
	{
		private string _text;
		private bool _readOnly;

		public MultiLineTextEntry()
		{
			Text = string.Empty;
			ReadOnly = false;
		}

		public string Text
		{
			get
			{ 
#if __MonoCS__
				var backend = BackendHost.Backend as RichTextViewBackend;
				if (backend != null)
				{
					var textView = backend.Widget as Gtk.TextView;
					return textView.Buffer.Text;
				}
#endif
				return _text; 
			}
			set
			{
				_text = value;
				LoadText(value, TextFormat.Plain);
			}
		}

		public bool ReadOnly
		{
			get { return _readOnly; }
			set
			{
				_readOnly = value;
#if __MonoCS__
				var backend = BackendHost.Backend as RichTextViewBackend;

				if (backend != null)
				{
					var textView = backend.Widget as Gtk.TextView;
					textView.Editable = !value;
				}
#endif
			}
		}

	}
}

