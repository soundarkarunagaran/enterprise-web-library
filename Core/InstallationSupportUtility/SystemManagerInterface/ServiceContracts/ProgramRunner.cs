﻿using System.IO;
using System.ServiceModel;

namespace EnterpriseWebLibrary.InstallationSupportUtility.SystemManagerInterface.ServiceContracts {
	[ ServiceContract ]
	public interface ProgramRunner {
		[ OperationContract ]
		Stream DownloadClientSideAppPackage( string authenticationKey, int buildId );

		[ OperationContract ]
		Stream DownloadInstallationConfigurationPackage( string authenticationKey, int buildId, int installationId );
	}
}