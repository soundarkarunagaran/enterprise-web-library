﻿using System.Web.UI;

namespace EnterpriseWebLibrary.EnterpriseWebFramework {
	/// <summary>
	/// A placeholder control for modification error messages.
	/// </summary>
	// It's important that this is a naming container so that when controls are added to it after a transfer, other parts of the page such as form control IDs
	// don't end up being different from the way they were before the transfer.
	public class ModificationErrorPlaceholder: Control, INamingContainer, ControlTreeDataLoader {
		private readonly EwfValidation validation;
		private readonly ErrorDisplayStyle displayStyle;

		/// <summary>
		/// Creates a modification error placeholder for the specified validation, or for the top modification errors if no validation is passed.
		/// </summary>
		public ModificationErrorPlaceholder( EwfValidation validation, ErrorDisplayStyle displayStyle ) {
			this.validation = validation;
			this.displayStyle = displayStyle;
		}

		void ControlTreeDataLoader.LoadData() {
			this.AddControlsReturnThis(
				displayStyle.GetControls(
					validation != null
						? EwfPage.Instance.AddModificationErrorDisplayAndGetErrors( this, "", validation )
						: AppRequestState.Instance.EwfPageRequestState.TopModificationErrors ) );
		}
	}
}