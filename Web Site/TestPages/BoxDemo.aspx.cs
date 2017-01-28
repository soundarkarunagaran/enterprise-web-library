using EnterpriseWebLibrary.EnterpriseWebFramework;
using EnterpriseWebLibrary.EnterpriseWebFramework.Controls;

namespace EnterpriseWebLibrary.WebSite.TestPages {
	partial class BoxDemo: EwfPage {
		partial class Info {
			public override string ResourceName { get { return "Box"; } }
		}

		protected override void loadData() {
			ph.AddControlsReturnThis(
				new Section( new Paragraph( "This is a basic box." ).ToCollection(), style: SectionStyle.Box ),
				new Section( "Heading Box", new Paragraph( "This is a box with heading." ).ToCollection(), style: SectionStyle.Box ),
				new Section( "Expandable Box", new Paragraph( "This is an expandable box." ).ToCollection(), style: SectionStyle.Box, expanded: false ) );
		}
	}
}