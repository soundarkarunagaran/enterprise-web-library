using EnterpriseWebLibrary.EnterpriseWebFramework.Controls;
using EnterpriseWebLibrary.EnterpriseWebFramework.Ui;
using EnterpriseWebLibrary.EnterpriseWebFramework.UserManagement;

// Parameter: int? userId

namespace EnterpriseWebLibrary.EnterpriseWebFramework.EnterpriseWebLibrary.WebSite.Admin {
	partial class EditUser: EwfPage {
		partial class Info {
			internal User User { get; private set; }

			protected override void init() {
				if( UserId.HasValue )
					User = UserManagementStatics.GetUser( UserId.Value, true );
			}

			protected override ResourceInfo createParentResourceInfo() {
				return new SystemUsers.Info( esInfo );
			}

			public override string ResourceName => User == null ? "New User" : User.Email;
		}

		private UserFieldTable userFieldTable;

		protected override void loadData() {
			if( info.UserId.HasValue )
				EwfUiStatics.SetPageActions(
					new ActionButtonSetup(
						"Delete User",
						new PostBackButton(
							PostBack.CreateFull( id: "delete", firstModificationMethod: deleteUser, actionGetter: () => new PostBackAction( new SystemUsers.Info( es.info ) ) ) ) ) );

			var pb = PostBack.CreateFull( firstModificationMethod: modifyData, actionGetter: () => new PostBackAction( info.ParentResource ) );
			FormState.ExecuteWithDataModificationsAndDefaultAction(
				pb.ToCollection(),
				() => {
					userFieldTable = new UserFieldTable();
					userFieldTable.LoadData( info.UserId );
					ph.AddControlsReturnThis( userFieldTable );
				} );
			EwfUiStatics.SetContentFootActions( new ActionButtonSetup( "OK", new PostBackButton( pb ) ) );
		}

		private void deleteUser() {
			UserManagementStatics.SystemProvider.DeleteUser( info.User.UserId );
		}

		private void modifyData() {
			if( FormsAuthStatics.FormsAuthEnabled ) {
				if( info.UserId.HasValue )
					FormsAuthStatics.SystemProvider.InsertOrUpdateUser(
						info.User.UserId,
						userFieldTable.Email,
						userFieldTable.RoleId,
						info.User.LastRequestTime,
						userFieldTable.Salt,
						userFieldTable.SaltedPassword,
						userFieldTable.MustChangePassword );
				else
					FormsAuthStatics.SystemProvider.InsertOrUpdateUser(
						null,
						userFieldTable.Email,
						userFieldTable.RoleId,
						null,
						userFieldTable.Salt,
						userFieldTable.SaltedPassword,
						userFieldTable.MustChangePassword );
			}
			else if( UserManagementStatics.SystemProvider is ExternalAuthUserManagementProvider ) {
				var provider = UserManagementStatics.SystemProvider as ExternalAuthUserManagementProvider;
				provider.InsertOrUpdateUser( info.UserId, userFieldTable.Email, userFieldTable.RoleId, info.User?.LastRequestTime );
			}
			userFieldTable.SendEmailIfNecessary();
		}
	}
}