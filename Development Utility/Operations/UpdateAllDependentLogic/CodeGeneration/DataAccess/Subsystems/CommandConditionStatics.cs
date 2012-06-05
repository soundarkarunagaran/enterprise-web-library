﻿using System.IO;
using RedStapler.StandardLibrary;
using RedStapler.StandardLibrary.DataAccess;
using RedStapler.StandardLibrary.InstallationSupportUtility.DatabaseAbstraction;

namespace EnterpriseWebLibrary.DevelopmentUtility.Operations.CodeGeneration.DataAccess.Subsystems {
	internal static class CommandConditionStatics {
		internal static void Generate( DBConnection cn, TextWriter writer, string baseNamespace, Database database ) {
			writer.WriteLine( "namespace " + baseNamespace + "." + database.SecondaryDatabaseName + "CommandConditions {" );
			foreach( var table in database.GetTables() ) {
				// Write the interface for all of the table's conditions.
				writer.WriteLine( "public interface " + table + "TableCondition: TableCondition {}" );

				writeEqualityConditionClasses( cn, writer, table );
				writeInequalityConditionClasses( cn, writer, table );
				writeLikeConditionClasses( cn, writer, table );
			}
			writer.WriteLine( "}" ); // namespace
		}

		private static void writeEqualityConditionClasses( DBConnection cn, TextWriter writer, string table ) {
			writer.WriteLine( "public static class " + GetTableEqualityConditionsClassName( table ) + " {" );
			foreach( var column in new TableColumns( cn, table, false ).AllColumns ) {
				CodeGenerationStatics.AddSummaryDocComment( writer, "A condition that narrows the scope of a command." );
				writer.WriteLine( "public class " + GetConditionClassName( column.Name ) + ": " + table + "TableCondition {" );
				writer.WriteLine( "private readonly " + column.DataTypeName + " value;" );

				CodeGenerationStatics.AddSummaryDocComment( writer, "Creates a condition to narrow the scope of a command." );
				writer.WriteLine( "public " + GetConditionClassName( column.Name ) + "( " + column.DataTypeName + " value ) {" );
				writer.WriteLine( "this.value = value;" );
				writer.WriteLine( "}" );

				writer.WriteLine( "internal " + column.DataTypeName + " Value { get { return value; } }" );
				writer.WriteLine( "InlineDbCommandCondition TableCondition.CommandCondition { get { return new EqualityCondition( new InlineDbCommandColumnValue( \"" +
				                  column.Name + "\", new DbParameterValue( value, \"" + column.DbTypeString + "\" ) ) ); } }" );
				writer.WriteLine( "}" );
			}
			writer.WriteLine( "}" ); // class
		}

		internal static string GetTableEqualityConditionsClassName( string table ) {
			return table + "TableEqualityConditions";
		}

		private static void writeInequalityConditionClasses( DBConnection cn, TextWriter writer, string table ) {
			// NOTE: This kind of sucks. It seems like we could use generics to not have to write N of these methods into ISU.cs.
			writer.WriteLine( "public static class " + table + "TableInequalityConditions {" );
			foreach( var column in new TableColumns( cn, table, false ).AllColumns ) {
				CodeGenerationStatics.AddSummaryDocComment( writer, "A condition that narrows the scope of a command." );
				writer.WriteLine( "public class " + GetConditionClassName( column.Name ) + ": " + table + "TableCondition {" );
				writer.WriteLine( "private readonly InequalityCondition.Operator op; " );
				writer.WriteLine( "private readonly " + column.DataTypeName + " value;" );

				CodeGenerationStatics.AddSummaryDocComment( writer,
				                                            "Creates a condition to narrow the scope of a command. Expression will read 'valueInDatabase op yourValue'. So new InequalityCondition( Operator.GreaterThan, value ) will turn into 'columnName > @value'." );
				writer.WriteLine( "public " + GetConditionClassName( column.Name ) + "( InequalityCondition.Operator op, " + column.DataTypeName + " value ) {" );
				writer.WriteLine( "this.op = op;" );
				writer.WriteLine( "this.value = value;" );
				writer.WriteLine( "}" );

				writer.WriteLine(
					"InlineDbCommandCondition TableCondition.CommandCondition { get { return new InequalityCondition( op, new InlineDbCommandColumnValue( \"" + column.Name +
					"\", new DbParameterValue( value, \"" + column.DbTypeString + "\" ) ) ); } }" );
				writer.WriteLine( "}" );
			}
			writer.WriteLine( "}" ); // class
		}

		private static void writeLikeConditionClasses( DBConnection cn, TextWriter writer, string table ) {
			writer.WriteLine( "public static class " + table + "TableLikeConditions {" );
			foreach( var column in new TableColumns( cn, table, false ).AllColumns ) {
				CodeGenerationStatics.AddSummaryDocComment( writer, "A condition that narrows the scope of a command." );
				writer.WriteLine( "public class " + GetConditionClassName( column.Name ) + ": " + table + "TableCondition {" );
				writer.WriteLine( "private readonly LikeCondition.Behavior behavior; " );
				writer.WriteLine( "private readonly string value;" );

				CodeGenerationStatics.AddSummaryDocComment( writer, "Creates a condition to narrow the scope of a command." );
				writer.WriteLine( "public " + GetConditionClassName( column.Name ) + "( LikeCondition.Behavior behavior, string value ) {" );
				writer.WriteLine( "this.behavior = behavior;" );
				writer.WriteLine( "this.value = value;" );
				writer.WriteLine( "}" );

				writer.WriteLine( "InlineDbCommandCondition TableCondition.CommandCondition { get { return new LikeCondition( behavior, \"" + column.Name +
				                  "\", value ); } }" );

				writer.WriteLine( "}" ); // class
			}
			writer.WriteLine( "}" ); // class
		}

		internal static string GetConditionClassName( string column ) {
			var capitalizedColumn = column.CapitalizeString();
			return capitalizedColumn == "Value" ? "_Value" : capitalizedColumn;
		}
	}
}