﻿using System;
using System.Collections.Generic;
using System.IO;
using EnterpriseWebLibrary.DataAccess;
using EnterpriseWebLibrary.DataAccess.CommandWriting;
using EnterpriseWebLibrary.DataAccess.CommandWriting.Commands;
using EnterpriseWebLibrary.DataAccess.CommandWriting.InlineConditionAbstraction.Conditions;
using EnterpriseWebLibrary.DatabaseSpecification;
using EnterpriseWebLibrary.DatabaseSpecification.Databases;
using EnterpriseWebLibrary.IO;
using Humanizer;

namespace EnterpriseWebLibrary.InstallationSupportUtility.DatabaseAbstraction.Databases {
	public class MySql: Database {
		private readonly MySqlInfo info;

		public MySql( MySqlInfo info ) {
			this.info = info;
		}

		string Database.SecondaryDatabaseName { get { return ( info as DatabaseInfo ).SecondaryDatabaseName; } }

		void Database.ExecuteSqlScriptInTransaction( string script ) {
			using( var sw = new StringWriter() ) {
				sw.WriteLine( "START TRANSACTION;" );
				sw.Write( script );
				sw.WriteLine( "COMMIT;" );
				sw.WriteLine( "quit" );

				executeMethodWithDbExceptionHandling(
					delegate {
						try {
							EwlStatics.RunProgram(
								EwlStatics.CombinePaths( binFolderPath, "mysql" ),
								getHostAndAuthenticationArguments() + " " + info.Database + " --disable-reconnect --batch --disable-auto-rehash",
								sw.ToString(),
								true );
						}
						catch( Exception e ) {
							throw DataAccessMethods.CreateDbConnectionException( info, "updating logic in", e );
						}
					} );
			}
		}

		int Database.GetLineMarker() {
			var value = 0;
			ExecuteDbMethod(
				delegate( DBConnection cn ) {
					var command = cn.DatabaseInfo.CreateCommand();
					command.CommandText = "SELECT ParameterValue FROM global_ints WHERE ParameterName = 'LineMarker'";
					value = (int)cn.ExecuteScalarCommand( command );
				} );
			return value;
		}

		void Database.UpdateLineMarker( int value ) {
			ExecuteDbMethod(
				delegate( DBConnection cn ) {
					var command = new InlineUpdate( "global_ints" );
					command.AddColumnModification( new InlineDbCommandColumnValue( "ParameterValue", new DbParameterValue( value ) ) );
					command.AddCondition( new EqualityCondition( new InlineDbCommandColumnValue( "ParameterName", new DbParameterValue( "LineMarker" ) ) ) );
					command.Execute( cn );
				} );
		}

		void Database.ExportToFile( string filePath ) {
			executeMethodWithDbExceptionHandling(
				delegate {
					try {
						// The --hex-blob option prevents certain BLOBs from causing errors during database re-creation.
						EwlStatics.RunProgram(
							EwlStatics.CombinePaths( binFolderPath, "mysqldump" ),
							getHostAndAuthenticationArguments() + " --single-transaction --hex-blob --result-file=\"{0}\" ".FormatWith( filePath ) + info.Database,
							"",
							true );
					}
					catch( Exception e ) {
						throw DataAccessMethods.CreateDbConnectionException( info, "exporting (to file)", e );
					}
				} );
		}

		void Database.DeleteAndReCreateFromFile( string filePath ) {
			using( var sw = new StringWriter() ) {
				sw.WriteLine( "DROP DATABASE IF EXISTS {0};".FormatWith( info.Database ) );
				sw.WriteLine( "CREATE DATABASE {0};".FormatWith( info.Database ) );
				sw.WriteLine( "use {0}".FormatWith( info.Database ) );
				sw.Write( File.ReadAllText( filePath ) );
				sw.WriteLine( "quit" );

				executeMethodWithDbExceptionHandling(
					delegate {
						try {
							EwlStatics.RunProgram(
								EwlStatics.CombinePaths( binFolderPath, "mysql" ),
								getHostAndAuthenticationArguments() + " --disable-reconnect --batch --disable-auto-rehash",
								sw.ToString(),
								true );
						}
						catch( Exception e ) {
							if( e.Message.Contains( "ERROR" ) && e.Message.Contains( "at line" ) )
								throw new UserCorrectableException( "Failed to create database from file. Please try the operation again after obtaining a new database file.", e );
							throw DataAccessMethods.CreateDbConnectionException( info, "re-creating (from file)", e );
						}
					} );
			}
		}

		private string binFolderPath => IoMethods.GetFirstExistingFolderPath(
			new[] { @"C:\Program Files\MySQL\MySQL Server 5.7\bin", @"C:\Program Files\MySQL\MySQL Server 5.5\bin" },
			"MySQL" );

		private string getHostAndAuthenticationArguments() {
			return "--host=localhost --user=root --password=password";
		}

		List<string> Database.GetTables() {
			var tables = new List<string>();
			ExecuteDbMethod(
				delegate( DBConnection cn ) {
					var command = cn.DatabaseInfo.CreateCommand();
					command.CommandText =
						"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{0}' AND TABLE_TYPE = 'BASE TABLE'".FormatWith( info.Database );
					cn.ExecuteReaderCommand(
						command,
						reader => {
							while( reader.Read() )
								tables.Add( reader.GetString( 0 ) );
						} );
				} );
			return tables;
		}

		List<string> Database.GetProcedures() {
			throw new NotSupportedException();
		}

		List<ProcedureParameter> Database.GetProcedureParameters( string procedure ) {
			throw new NotSupportedException();
		}

		void Database.PerformMaintenance() {}

		void Database.ShrinkAfterPostUpdateDataCommands() {}

		public void ExecuteDbMethod( Action<DBConnection> method ) {
			executeMethodWithDbExceptionHandling(
				() => {
					var connection = new DBConnection( new MySqlInfo( ( info as DatabaseInfo ).SecondaryDatabaseName, info.Database, false ) );
					connection.ExecuteWithConnectionOpen( () => method( connection ) );
				} );
		}

		private void executeMethodWithDbExceptionHandling( Action method ) {
			try {
				method();
			}
			catch( DbConnectionFailureException e ) {
				throw new UserCorrectableException( "Failed to connect to MySQL.", e );
			}
		}
	}
}