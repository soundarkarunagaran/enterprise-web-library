﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using EnterpriseWebLibrary.Configuration;
using EnterpriseWebLibrary.Configuration.SystemGeneral;
using EnterpriseWebLibrary.Email;
using Humanizer;

namespace EnterpriseWebLibrary.InstallationSupportUtility.InstallationModel {
	public class ExistingInstallationLogic {
		public const string SystemDatabaseUpdatesFileName = "Database Updates.sql";
		private const int serviceFailureResetPeriod = 3600; // seconds

		private readonly GeneralInstallationLogic generalInstallationLogic;
		private readonly InstallationConfiguration runtimeConfiguration;

		public ExistingInstallationLogic( GeneralInstallationLogic generalInstallationLogic, InstallationConfiguration runtimeConfiguration ) {
			this.generalInstallationLogic = generalInstallationLogic;
			this.runtimeConfiguration = runtimeConfiguration;
		}

		public InstallationConfiguration RuntimeConfiguration => runtimeConfiguration;

		public string DatabaseUpdateFilePath => EwlStatics.CombinePaths( runtimeConfiguration.ConfigurationFolderPath, SystemDatabaseUpdatesFileName );

		/// <summary>
		/// Stops all web sites and services associated with this installation.
		/// </summary>
		public void Stop( bool stopServices ) {
			if( runtimeConfiguration.WebApplications.Any( i => i.IisApplication != null ) && runtimeConfiguration.InstallationType != InstallationType.Development )
				IsuStatics.StopIisAppPool( IisAppPoolName );
			if( stopServices )
				this.stopServices();
		}

		public void UninstallServices() {
			// Installutil tries to stop services during uninstallation, but doesn't report failure if a service doesn't stop. That's why we stop the services
			// ourselves first.
			stopServices();

			var allServices = ServiceController.GetServices();
			foreach( var service in runtimeConfiguration.WindowsServices.Where( s => allServices.Any( sc => sc.ServiceName == s.InstalledName ) ) )
				runInstallutil( service, true );
		}

		private void stopServices() {
			var allServices = ServiceController.GetServices();
			var serviceNames = RuntimeConfiguration.WindowsServices.Select( s => s.InstalledName );
			foreach( var service in allServices.Where( sc => serviceNames.Contains( sc.ServiceName ) ) ) {
				// Clear failure actions.
				EwlStatics.RunProgram( "sc", "failure \"{0}\" reset= {1} actions= \"\"".FormatWith( service.ServiceName, serviceFailureResetPeriod ), "", true );

				if( service.Status == ServiceControllerStatus.Stopped )
					continue;
				service.Stop();
				service.WaitForStatusWithTimeOut( ServiceControllerStatus.Stopped );
			}
		}

		/// <summary>
		/// Starts all web sites and services associated with this installation.
		/// </summary>
		public void Start() {
			var allServices = ServiceController.GetServices();
			foreach( var service in RuntimeConfiguration.WindowsServices.Select(
				s => {
					var serviceController = allServices.SingleOrDefault( sc => sc.ServiceName == s.InstalledName );
					if( serviceController == null )
						throw new UserCorrectableException(
							"The \"" + s.InstalledName + "\" service could not be found. Re-install the services for the installation to correct this error." );
					return serviceController;
				} ) ) {
				try {
					service.Start();
				}
				catch( InvalidOperationException e ) {
					const string message = "Failed to start service.";

					// We have seen this happen when an exception was thrown while initializing global logic for the system.
					if( e.InnerException is Win32Exception &&
					    e.InnerException.Message.Contains( "The service did not respond to the start or control request in a timely fashion" ) )
						throw new UserCorrectableException( message, e );

					throw new ApplicationException( message, e );
				}
				service.WaitForStatusWithTimeOut( ServiceControllerStatus.Running );

				// Set failure actions.
				const int restartDelay = 60000; // milliseconds
				EwlStatics.RunProgram(
					"sc",
					"failure \"{0}\" reset= {1} actions= restart/{2}".FormatWith( service.ServiceName, serviceFailureResetPeriod, restartDelay ),
					"",
					true );
				EwlStatics.RunProgram( "sc", "failureflag \"{0}\" 1".FormatWith( service.ServiceName ), "", true );
			}
			if( runtimeConfiguration.WebApplications.Any( i => i.IisApplication != null ) && runtimeConfiguration.InstallationType != InstallationType.Development )
				IsuStatics.StartIisAppPool( IisAppPoolName );
		}

		public void InstallServices() {
			foreach( var service in runtimeConfiguration.WindowsServices ) {
				if( ServiceController.GetServices().Any( sc => sc.ServiceName == service.InstalledName ) )
					throw new UserCorrectableException( "A service could not be installed because one with the same name already exists." );
				runInstallutil( service, false );
			}
		}

		private void runInstallutil( WindowsService service, bool uninstall ) {
			try {
				EwlStatics.RunProgram(
					EwlStatics.CombinePaths( RuntimeEnvironment.GetRuntimeDirectory(), "installutil" ),
					( uninstall ? "/u " : "" ) + "\"" +
					EwlStatics.CombinePaths( GetWindowsServiceFolderPath( service, true ), service.NamespaceAndAssemblyName + ".exe" /* file extension is required */ ) + "\"",
					"",
					true );
			}
			catch( Exception e ) {
				const string message = "Installer tool failed.";
				if( e.Message.Contains( typeof( EmailSendingException ).Name ) )
					throw new UserCorrectableException( message, e );
				throw new ApplicationException( message, e );
			}
		}

		internal string IisAppPoolName => runtimeConfiguration.FullShortName;

		public string GetWindowsServiceFolderPath( WindowsService service, bool useDebugFolderIfDevelopmentInstallation ) {
			var path = EwlStatics.CombinePaths( generalInstallationLogic.Path, service.Name );
			if( runtimeConfiguration.InstallationType == InstallationType.Development )
				path = EwlStatics.CombinePaths( path, EwlStatics.GetProjectOutputFolderPath( useDebugFolderIfDevelopmentInstallation ) );
			return path;
		}
	}
}