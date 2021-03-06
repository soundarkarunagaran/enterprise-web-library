﻿namespace EnterpriseWebLibrary {
	/// <summary>
	/// General system-specific logic.
	/// </summary>
	public interface SystemGeneralProvider {
		/// <summary>
		/// Gets the Aspose.Pdf license name. Returns the empty string if the system doesn't have an Aspose.Pdf license.
		/// </summary>
		string AsposePdfLicenseName { get; }

		/// <summary>
		/// Gets the Aspose.Words license name. Returns the empty string if the system doesn't have an Aspose.Words license.
		/// </summary>
		string AsposeWordsLicenseName { get; }

		/// <summary>
		/// Password used for intermediate log in and some other things.
		/// </summary>
		string IntermediateLogInPassword { get; }

		/// <summary>
		/// Email address used for sanitized systems that use forms authentication.
		/// </summary>
		string FormsLogInEmail { get; }

		/// <summary>
		/// Password used for sanitized systems that use forms authentication.
		/// </summary>
		string FormsLogInPassword { get; }

		/// <summary>
		/// Gets the email default from name.
		/// </summary>
		string EmailDefaultFromName { get; }

		/// <summary>
		/// Gets the email default from address.
		/// </summary>
		string EmailDefaultFromAddress { get; }
	}
}