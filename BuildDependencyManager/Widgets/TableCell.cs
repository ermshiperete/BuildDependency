// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using Xwt;
using Xwt.Drawing;

namespace BuildDependencyManager.Widgets
{
	public class TableCell<T>: Canvas where T: Widget, new()
	{
	private T _object;

		public TableCell(): this(new T())
		{
		}

		public TableCell(T obj, double minWidth = 200, double minHeight = 30)
		{
			HorizontalPlacement = WidgetPlacement.Center;
			VerticalPlacement = WidgetPlacement.Center;
			Margin = new WidgetSpacing(2, 2, 2, 2);
			_object = obj;
			_object.HorizontalPlacement = WidgetPlacement.Center;
			_object.VerticalPlacement = WidgetPlacement.Center;
			//_object.WidthRequest = Math.Max(200, _object.WidthRequest);
			//_object.HeightRequest = Math.Max(30, _object.HeightRequest);

			var preferredSize = _object.Surface.GetPreferredSize(true);
			AddChild(_object, new Rectangle(new Point(0, 0), preferredSize));
			WidthRequest = Math.Max(minWidth, preferredSize.Width);
			HeightRequest = Math.Max(minHeight, preferredSize.Height);
		}

		public T Object { get { return _object; }}

		protected override void OnDraw(Xwt.Drawing.Context ctx, Rectangle dirtyRect)
		{

			base.OnDraw(ctx, dirtyRect);

			if (Bounds.IsEmpty)
				return;

			ctx.Rectangle(Bounds.X, Bounds.Y, Bounds.Width - 1, Bounds.Height - 1);
			ctx.SetColor(Colors.Gray);
			ctx.SetLineWidth(1);
			ctx.Stroke();
		}
	}
}

