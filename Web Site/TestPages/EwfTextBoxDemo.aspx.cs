using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using EnterpriseWebLibrary.EnterpriseWebFramework;
using EnterpriseWebLibrary.EnterpriseWebFramework.Controls;
using EnterpriseWebLibrary.EnterpriseWebFramework.Ui;
using Humanizer;

// PageState: string test1
// PageState: string test2
// PageState: string test3
// PageState: string test4
// PageState: string test5
// PageState: string test6

namespace EnterpriseWebLibrary.WebSite.TestPages {
	partial class EwfTextBoxDemo: EwfPage {
		partial class Info {
			public override string ResourceName => "Text Box";
		}

		protected override void loadData() {
			var pb = PostBack.CreateFull();
			FormState.ExecuteWithDataModificationsAndDefaultAction(
				pb.ToCollection(),
				() => {
					addMessageIfNotNull( ph, getTest1( null ) );
					ph.AddControlsReturnThis( test1( setTest1 ) );

					addMessageIfNotNull( ph, getTest2( null ) );
					ph.AddControlsReturnThis( test2( setTest2 ) );

					addMessageIfNotNull( ph, getTest3( null ) );
					ph.AddControlsReturnThis( test3( setTest3 ) );

					addMessageIfNotNull( ph, getTest4( null ) );
					ph.AddControlsReturnThis( test4( setTest4 ) );

					addMessageIfNotNull( ph, getTest5( null ) );
					ph.AddControlsReturnThis( test5( setTest5 ) );

					addMessageIfNotNull( ph, getTest6( null ) );
					ph.AddControlsReturnThis( test6( setTest6 ) );

					var table = FormItemBlock.CreateFormItemTable();
					table.AddFormItems(
						FormItem.Create( "Textarea", new EwfTextBox( "This is a paragraph.", rows: 4 ) ),
						FormItem.Create( "Masked Input", new EwfTextBox( "This should not appear in the markup!", masksCharacters: true ) ) );
					ph.AddControlsReturnThis( table );

					EwfUiStatics.SetContentFootActions(
						new ActionButtonSetup( "OK", new PostBackButton() ),
						new ActionButtonSetup(
							"Reset Values",
							new PostBackButton(
								PostBack.CreateFull(
									id: "reset",
									firstModificationMethod: () => {
										setTest1( null );
										setTest2( null );
										setTest3( null );
										setTest4( null );
										setTest5( null );
										setTest6( null );
									} ) ) ) );
				} );
		}

		private void addMessageIfNotNull( Control control, string s ) {
			if( s != null )
				control.AddControlsReturnThis( "The value posted from this box was '{0}'".FormatWith( s ).ToComponents().GetControls() );
		}

		private Section test1( Action<string> setValue ) {
			var box =
				FormItem.Create( "", new EwfTextBox( "" ), validationGetter: control => new EwfValidation( ( pbv, v ) => setValue( control.GetPostBackValue( pbv ) ) ) )
					.Control;
			box.SetupAutoComplete( TestService.GetInfo(), AutoCompleteOption.NoPostBack );
			return
				new Section(
					"Autofill behavior. Typing more than 3 characters should bring up autofill options from a web service. " +
					"Selecting an item or changing the text will no cause a post-back. This value show appear when submitting the page's submit button.",
					box.ToCollection(),
					style: SectionStyle.Box );
		}

		private Section test2( Action<string> setValue ) {
			var pb = PostBack.CreateFull( id: "test2" );
			return FormState.ExecuteWithDataModificationsAndDefaultAction(
				pb.ToCollection(),
				() => {
					var box =
						FormItem.Create( "", new EwfTextBox( "" ), validationGetter: control => new EwfValidation( ( pbv, v ) => setValue( control.GetPostBackValue( pbv ) ) ) )
							.Control;
					box.SetupAutoComplete( TestService.GetInfo(), AutoCompleteOption.PostBackOnItemSelect );
					return
						new Section(
							"Autofill behavior. Typing more than 3 characters should bring up autofill options from a web service. " + "Selecting an item will cause a post-back.",
							box.ToCollection(),
							style: SectionStyle.Box );
				} );
		}

		private Section test3( Action<string> setValue ) {
			var pb = PostBack.CreateFull( id: "test3" );
			return FormState.ExecuteWithDataModificationsAndDefaultAction(
				pb.ToCollection(),
				() => {
					var box =
						FormItem.Create( "", new EwfTextBox( "" ), validationGetter: control => new EwfValidation( ( pbv, v ) => setValue( control.GetPostBackValue( pbv ) ) ) )
							.Control;
					box.SetupAutoComplete( TestService.GetInfo(), AutoCompleteOption.PostBackOnTextChangeAndItemSelect );
					return
						new Section(
							"Autofill behavior. Typing more than 3 characters should bring up autofill options from a web service. " +
							"Selecting an item  or changing the text will cause a post-back.",
							box.ToCollection(),
							style: SectionStyle.Box );
				} );
		}

		private Section test4( Action<string> setValue ) {
			var pb = PostBack.CreateFull( id: "test4" );
			return FormState.ExecuteWithDataModificationsAndDefaultAction(
				pb.ToCollection(),
				() => {
					var box =
						FormItem.Create(
							"",
							new EwfTextBox( "", autoPostBack: true ),
							validationGetter: control => new EwfValidation( ( pbv, v ) => setValue( control.GetPostBackValue( pbv ) ) ) ).Control;
					return new Section( "Post-back on change.", box.ToCollection(), style: SectionStyle.Box );
				} );
		}

		private Section test5( Action<string> setValue ) {
			var pb = PostBack.CreateFull( id: "test5" );
			return FormState.ExecuteWithDataModificationsAndDefaultAction(
				pb.ToCollection(),
				() => {
					var box =
						FormItem.Create( "", new EwfTextBox( "" ), validationGetter: control => new EwfValidation( ( pbv, v ) => setValue( control.GetPostBackValue( pbv ) ) ) )
							.Control;
					return new Section( "Post-back on enter.", box.ToCollection(), style: SectionStyle.Box );
				} );
		}

		private Section test6( Action<string> setValue ) {
			var pb = PostBack.CreateFull( id: "test6" );
			return FormState.ExecuteWithDataModificationsAndDefaultAction(
				pb.ToCollection(),
				() => {
					var box =
						FormItem.Create( "", new EwfTextBox( "" ), validationGetter: control => new EwfValidation( ( pbv, v ) => setValue( control.GetPostBackValue( pbv ) ) ) )
							.Control;
					var button = new PostBackButton( new ButtonActionControlStyle( "OK" ), usesSubmitBehavior: false );
					return new Section(
						"Post-back with non-default submit button. This post-back-value shouldn't show up when the page's submit button is submitted.",
						new WebControl[] { box, button },
						style: SectionStyle.Box );
				} );
		}
	}
}