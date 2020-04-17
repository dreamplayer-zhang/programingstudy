using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Livet.Behaviors;
/*
 Livet Copyright (c) 2010-2011 Livet Project

Livet is provided with zlib/libpng license.
Is your feedback, requests, and various, contact please ugaya40@hotmail.com.

This software is provided 'as-is', without any express or implied
 warranty. In no event will the authors be held liable for any damages
 arising from the use of this software.
 
Permission is granted to anyone to use this software for any purpose,
 including commercial applications, and to alter it and redistribute it
 freely, subject to the following restrictions:
 

1. The origin of this software must not be misrepresented; you must not
 claim that you wrote the original software. If you use this software
 in a product, an acknowledgment in the product documentation would be
 appreciated but is not required.
 
2. Altered source versions must be plainly marked as such, and must not be
 misrepresented as being the original software.
 
3. This notice may not be removed or altered from any source
 distribution.
 */

namespace MetroTrilithon.UI.Controls
{
	public class CallMethodButton : Button
	{
		static CallMethodButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(CallMethodButton), new FrameworkPropertyMetadata(typeof(CallMethodButton)));
		}

		private readonly MethodBinder _binder = new MethodBinder();
		private readonly MethodBinderWithArgument _binderWithArgument = new MethodBinderWithArgument();
		private bool _hasParameter;

		#region MethodTarget 依存関係プロパティ

		public object MethodTarget
		{
			get { return this.GetValue(MethodTargetProperty); }
			set { this.SetValue(MethodTargetProperty, value); }
		}
		public static readonly DependencyProperty MethodTargetProperty =
            DependencyProperty.Register("MethodTargetProperty", typeof(object), typeof(CallMethodButton), new UIPropertyMetadata(null));
        //C# 6.0 이상에서는 nameof()가 권장됨.(vs2013은 C# 5.0) nameof(MethodTargetProperty)

		#endregion

		#region MethodName 依存関係プロパティ

		public string MethodName
		{
			get { return (string)this.GetValue(MethodNameProperty); }
			set { this.SetValue(MethodNameProperty, value); }
		}
		public static readonly DependencyProperty MethodNameProperty =
            DependencyProperty.Register("MethodNameProperty", typeof(string), typeof(CallMethodButton), new UIPropertyMetadata(null));
        //C# 6.0 이상에서는 nameof()가 권장됨.(vs2013은 C# 5.0) nameof(MethodNameProperty)

		#endregion

		#region MethodParameter 依存関係プロパティ

		public object MethodParameter
		{
			get { return this.GetValue(MethodParameterProperty); }
			set { this.SetValue(MethodParameterProperty, value); }
		}
		public static readonly DependencyProperty MethodParameterProperty =
            DependencyProperty.Register("MethodParameterProperty", typeof(object), typeof(CallMethodButton), new UIPropertyMetadata(null, MethodParameterPropertyChangedCallback));
        //C# 6.0 이상에서는 nameof()가 권장됨.(vs2013은 C# 5.0) nameof(MethodParameterProperty)

		private static void MethodParameterPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var source = (CallMethodButton)d;
			source._hasParameter = true;
		}

		#endregion

		protected override void OnClick()
		{
			base.OnClick();

			if (string.IsNullOrEmpty(this.MethodName)) return;

			var target = this.MethodTarget ?? this.DataContext;
			if (target == null) return;

			if (this._hasParameter)
			{
				this._binderWithArgument.Invoke(target, this.MethodName, this.MethodParameter);
			}
			else
			{
				this._binder.Invoke(target, this.MethodName);
			}
		}
	}
}
