﻿using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using EnterpriseWebLibrary.JavaScriptWriting;

namespace EnterpriseWebLibrary.EnterpriseWebFramework.Controls {
	/// <summary>
	/// Provides a clickable link that displays a tool tip.
	/// </summary>
	public class ToolTipButton: WebControl, ControlTreeDataLoader, ControlWithJsInitLogic, ActionControl {
		private readonly Control toolTipControl;

		/// <summary>
		/// Sets the display style of this button. Do not set this to null.
		/// Choices are: BoxActionControlStyle, ImageActionControlStyle, CustomActionControlStyle, ButtonActionControlStyle, and TextActionControlStyle (default).
		/// </summary>
		public ActionControlStyle ActionControlStyle { private get; set; }

		/// <summary>
		/// Optional title to be displayed in the the tool tip.
		/// </summary>
		public string ToolTipTitle { get; set; }

		/// <summary>
		/// Creates a tool tip link. Do not pass null for the tool tip control.
		/// </summary>
		public ToolTipButton( Control toolTipControl ) {
			ActionControlStyle = new TextActionControlStyle( "" );
			this.toolTipControl = toolTipControl;
		}

		void ControlTreeDataLoader.LoadData() {
			if( toolTipControl == null )
				throw new ApplicationException( "ToolTipControl must be set on ToolTipLink" );

			if( TagKey == HtmlTextWriterTag.Button )
				PostBackButton.AddButtonAttributes( this );

			// NOTE: When this control is rendered as an anchor, the presence of an onclick attribute is necessary for it to be selected properly by our action
			// control CSS elements. This hack would not be necessary if Telerik used the onclick attribute to open the tool tip.
			this.AddJavaScriptEventScript( JsWritingMethods.onclick, "" );

			CssClass = CssClass.ConcatenateWithSpace( "ewfClickable" );
			ActionControlStyle.SetUpControl( this, "" );

			new ToolTip( toolTipControl, this, title: ToolTipTitle ?? "", sticky: true );
		}

		string ControlWithJsInitLogic.GetJsInitStatements() {
			return ActionControlStyle.GetJsInitStatements();
		}

		/// <summary>
		/// Returns the tag that represents this control in HTML.
		/// </summary>
		protected override HtmlTextWriterTag TagKey => PostBackButton.GetTagKey( ActionControlStyle );
	}
}