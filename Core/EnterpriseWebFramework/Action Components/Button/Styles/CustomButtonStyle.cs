﻿using System.Collections.Generic;

namespace EnterpriseWebLibrary.EnterpriseWebFramework {
	/// <summary>
	/// A style that displays a button in a custom way.
	/// </summary>
	public class CustomButtonStyle: ButtonStyle {
		private readonly ElementClassSet classes;
		private readonly IEnumerable<FlowComponent> children;

		/// <summary>
		/// Creates a custom style object.
		/// </summary>
		/// <param name="classes">The classes on the button.</param>
		/// <param name="children"></param>
		public CustomButtonStyle( ElementClassSet classes = null, IEnumerable<PhrasingComponent> children = null ) {
			this.classes = classes;
			this.children = children;
		}

		ElementClassSet ButtonStyle.GetClasses() {
			return ActionComponentCssElementCreator.AllStylesClass.Add( classes ?? ElementClassSet.Empty );
		}

		IEnumerable<FlowComponentOrNode> ButtonStyle.GetChildren() {
			return children;
		}

		string ButtonStyle.GetJsInitStatements( string id ) {
			return "";
		}
	}
}