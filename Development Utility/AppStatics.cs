﻿using System.Linq;
using RedStapler.StandardLibrary;
using RedStapler.StandardLibrary.InstallationSupportUtility;
using RedStapler.StandardLibrary.InstallationSupportUtility.RsisInterface.Messages.SystemListMessage;

namespace EnterpriseWebLibrary.DevelopmentUtility {
	internal class AppStatics {
		internal const string WebProjectFilesFolderName = "Web Project Files";
		internal const string EwfFolderName = "Ewf";
		internal const string StandardLibraryFilesFileName = "Standard Library Files.xml";
		internal static string DotNetToolsFolderPath { get { return @"C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\NETFX 4.0 Tools"; } }

		internal static string GetDevInstallationPath( SoftwareSystem system ) {
			return StandardLibraryMethods.CombinePaths( ConfigurationLogic.RevisionControlFolderPath,
			                                            system.VaultPath.Any() ? system.VaultPath.Substring( 1 ).Replace( "/", "\\" ) : "" );
		}
	}
}