﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EnterpriseWebLibrary.Configuration;
using EnterpriseWebLibrary.WebSessionState;
using Stripe;

namespace EnterpriseWebLibrary.EnterpriseWebFramework {
	/// <summary>
	/// Methods for processing payments with Stripe.
	/// </summary>
	public static class PaymentProcessingStatics {
		/// <summary>
		/// Returns credit-card-collection hidden fields and a JavaScript function call getter that opens a Stripe Checkout modal window. If the window's submit
		/// button is clicked, the credit card is charged or otherwise used. Do not execute the getter until after the page tree has been built.
		/// </summary>
		/// <param name="testPublishableKey">Your test publishable API key. Will be used in non-live installations. Do not pass null.</param>
		/// <param name="livePublishableKey">Your live publishable API key. Will be used in live installations. Do not pass null.</param>
		/// <param name="name">See https://stripe.com/docs/checkout. Do not pass null.</param>
		/// <param name="description">See https://stripe.com/docs/checkout. Do not pass null.</param>
		/// <param name="amountInDollars">See https://stripe.com/docs/checkout, but note that this parameter is in dollars, not cents</param>
		/// <param name="testSecretKey">Your test secret API key. Will be used in non-live installations. Do not pass null.</param>
		/// <param name="liveSecretKey">Your live secret API key. Will be used in live installations. Do not pass null.</param>
		/// <param name="successHandler">A method that executes if the credit-card submission is successful. The first parameter is the charge ID and the second
		/// parameter is the amount of the charge, in dollars.</param>
		/// <param name="prefilledEmailAddressOverride">By default, the email will be prefilled with AppTools.User.Email if AppTools.User is not null. You can
		/// override this with either a specified email address (if user is paying on behalf of someone else) or the empty string (to force the user to type in the
		/// email address).</param>
		public static Tuple<IReadOnlyCollection<EtherealComponentOrElement>, Func<string>> GetCreditCardCollectionHiddenFieldsAndJsFunctionCall(
			string testPublishableKey, string livePublishableKey, string name, string description, decimal? amountInDollars, string testSecretKey, string liveSecretKey,
			Func<string, decimal, StatusMessageAndDestination> successHandler, string prefilledEmailAddressOverride = null ) {
			if( !EwfApp.Instance.RequestIsSecure( HttpContext.Current.Request ) )
				throw new ApplicationException( "Credit-card collection can only be done from secure pages." );
			EwfPage.Instance.ClientScript.RegisterClientScriptInclude(
				typeof( PaymentProcessingStatics ),
				"Stripe Checkout",
				"https://checkout.stripe.com/v2/checkout.js" );

			if( amountInDollars.HasValue && amountInDollars.Value.DollarValueHasFractionalCents() )
				throw new ApplicationException( "Amount must not include fractional cents." );

			ResourceInfo successDestination = null;
			var postBack = PostBack.CreateFull(
				id: PostBack.GetCompositeId( "ewfCreditCardCollection", description ),
				actionGetter: () => new PostBackAction( successDestination ) );
			var token = new DataValue<string>();

			var hiddenFieldId = new HiddenFieldId();
			List<EtherealComponentOrElement> hiddenFields = new List<EtherealComponentOrElement>();
			ValidationSetupState.ExecuteWithDataModifications(
				postBack.ToCollection(),
				() => hiddenFields.Add( new EwfHiddenField( "", ( postBackValue, validator ) => token.Value = postBackValue.Value, id: hiddenFieldId ).PageComponent ) );

			postBack.AddModificationMethod(
				() => {
					// We can add support later for customer creation, subscriptions, etc. as needs arise.
					if( !amountInDollars.HasValue )
						throw new ApplicationException( "Only simple charges are supported at this time." );

					var apiKey = ConfigurationStatics.IsLiveInstallation ? liveSecretKey : testSecretKey;
					dynamic response = new StripeClient( apiKey ).CreateCharge(
						amountInDollars.Value,
						"usd",
						new CreditCardToken( token.Value ),
						description: description.Any() ? description : null );
					if( response.IsError ) {
						if( response.error.type == "card_error" )
							throw new DataModificationException( response.error.message );
						throw new ApplicationException( "Stripe error: " + response );
					}

					try {
						var messageAndDestination = successHandler( (string)response.id, amountInDollars.Value );
						if( messageAndDestination.Message.Any() )
							EwfPage.AddStatusMessage( StatusMessageType.Info, messageAndDestination.Message );
						successDestination = messageAndDestination.Destination;
					}
					catch( Exception e ) {
						throw new ApplicationException( "An exception occurred after a credit card was charged.", e );
					}
				} );

			EwfPage.Instance.AddPostBack( postBack );
			return Tuple.Create<IReadOnlyCollection<EtherealComponentOrElement>, Func<string>>(
				hiddenFields,
				() => {
					if( hiddenFieldId.Id.Length == 0 )
						throw new ApplicationException( "The credit-card-collection hidden fields must be on the page." );
					var jsTokenHandler = "function( res ) { $( '#" + hiddenFieldId.Id + "' ).val( res.id ); " +
					                     EwfPage.GetPostBackScript( postBack, includeReturnFalse: false ) + "; }";
					return "StripeCheckout.open( { key: '" + ( ConfigurationStatics.IsLiveInstallation ? livePublishableKey : testPublishableKey ) + "', name: '" + name +
					       "', description: '" + description + "', " + ( amountInDollars.HasValue ? "amount: " + amountInDollars.Value * 100 + ", " : "" ) + "token: " +
					       jsTokenHandler + ", email: '" + ( prefilledEmailAddressOverride ?? ( AppTools.User == null ? "" : AppTools.User.Email ) ) + "' } )";
				} );
		}
	}
}