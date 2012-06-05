﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RedStapler.StandardLibrary;

namespace EnterpriseWebLibrary.DevelopmentUtility.Operations.CodeGeneration {
	internal static class FormItemStatics {
		private const string deferredCallWarning =
			"You almost certainly should not call this method from a deferred block of code since this could cause validations to be added to the data modification in the wrong order.";

		internal static void WriteFormItemGetters( TextWriter writer, ModificationField field ) {
			// Some of these form item getters need modification methods to be executed to work properly. They return these methods, as out parameters, instead of
			// just adding them to the data modification. This allows client code on a page to specify the order of modification methods, which is important because
			// there may be both child modifications (like file collections) and one-to-many modifications (like M+Vision references for an applicant) on the same
			// page, and the main modification needs to execute between these.
			writeStringFormItemGetters( writer, field );
			writeNumericFormItemGetters( writer, field );
			writeBoolFormItemGetters( writer, field );
			writeDateFormItemGetters( writer, field );
			writeEnumerableFormItemGetters( writer, field );

			writeGenericGetterWithoutValueParams( writer, field, null );
			writeGenericGetterWithoutValueParams( writer, field, false );
			writeGenericGetterWithoutValueParams( writer, field, true );
			writeGenericGetterWithValueParams( writer, field, null );
			writeGenericGetterWithValueParams( writer, field, false );
			writeGenericGetterWithValueParams( writer, field, true );
		}

		private static void writeStringFormItemGetters( TextWriter writer, ModificationField field ) {
			if( !field.TypeIs( typeof( string ) ) )
				return;

			writeFormItemGetters( writer,
			                      field,
			                      "EwfTextBox",
			                      "Text",
			                      "string",
			                      "\"\"",
			                      new CSharpParameter[ 0 ],
			                      getAllowEmptyParameter( false ).ToSingleElementArray(),
			                      new CSharpParameter( "int", "textBoxRows", "1" ).ToSingleElementArray(),
			                      new CSharpParameter[ 0 ],
			                      "new EwfTextBox( v ) { " + ( field.Size.HasValue ? "MaxCharacters = " + field.Size.Value + ", " : "" ) + "Rows = textBoxRows }",
			                      "validator.GetString( new ValidationErrorHandler( subject ), control.GetPostBackValue( postBackValues ), allowEmpty" +
			                      ( field.Size.HasValue ? ", " + field.Size.Value : "" ) + " )",
			                      "true" );
			writeFormItemGetters( writer,
			                      field,
			                      "EwfTextBox",
			                      "EmailAddress",
			                      "string",
			                      "\"\"",
			                      new CSharpParameter[ 0 ],
			                      getAllowEmptyParameter( false ).ToSingleElementArray(),
			                      new CSharpParameter[ 0 ],
			                      new CSharpParameter[ 0 ],
			                      "new EwfTextBox( v )" + ( field.Size.HasValue ? " { MaxCharacters = " + field.Size.Value + " }" : "" ),
			                      "validator.GetEmailAddress( new ValidationErrorHandler( subject ), control.GetPostBackValue( postBackValues ), allowEmpty" +
			                      ( field.Size.HasValue ? ", maxLength: " + field.Size.Value : "" ) + " )",
			                      "true" );
			writeFormItemGetters( writer,
			                      field,
			                      "EwfTextBox",
			                      "PhoneNumber",
			                      "string",
			                      "\"\"",
			                      new CSharpParameter[ 0 ],
			                      getAllowEmptyParameter( false ).ToSingleElementArray(),
			                      new CSharpParameter[ 0 ],
			                      new[] { new CSharpParameter( "bool", "allowExtension", "true" ), new CSharpParameter( "bool", "allowSurroundingGarbage", "false" ) },
			                      "new EwfTextBox( v )" + ( field.Size.HasValue ? " { MaxCharacters = " + field.Size.Value + " }" : "" ),
			                      "validator.GetPhoneNumber( new ValidationErrorHandler( subject ), control.GetPostBackValue( postBackValues ), allowExtension, allowEmpty, allowSurroundingGarbage )",
			                      "true" );
			writeFormItemGetters( writer,
			                      field,
			                      "EwfTextBox",
			                      "Uri",
			                      "string",
			                      "\"\"",
			                      new CSharpParameter[ 0 ],
			                      getAllowEmptyParameter( false ).ToSingleElementArray(),
			                      new CSharpParameter[ 0 ],
			                      new CSharpParameter[ 0 ],
			                      "new EwfTextBox( v )" + ( field.Size.HasValue ? " { MaxCharacters = " + field.Size.Value + " }" : "" ),
			                      "validator.GetUrl( new ValidationErrorHandler( subject ), control.GetPostBackValue( postBackValues ), allowEmpty" +
			                      ( field.Size.HasValue ? ", " + field.Size.Value : "" ) + " )",
			                      "true" );
			writeFormItemGetters( writer,
			                      field,
			                      "EwfListControl",
			                      "SelectList",
			                      "string",
			                      "\"\"",
			                      new[] { new CSharpParameter( "System.Action<EwfListControl>", "itemAdder" ) },
			                      new CSharpParameter[ 0 ],
			                      new[]
			                      	{
			                      		new CSharpParameter( "EwfListControl.ListControlType", "listControlType", "EwfListControl.ListControlType.DropDownList" ),
			                      		new CSharpParameter( "bool", "autoPostBack", "false" )
			                      	},
			                      new CSharpParameter[ 0 ],
			                      "{ var control = new EwfListControl { Type = listControlType, AutoPostBack = autoPostBack }; itemAdder( control ); control.Value = v; return control; }",
			                      "control.Value",
			                      "true" );
		}

		private static void writeNumericFormItemGetters( TextWriter writer, ModificationField field ) {
			if( field.TypeIs( typeof( int ) ) ) {
				writeNumberAsTextFormItemGetters( writer,
				                                  field,
				                                  "int?",
				                                  new[] { new CSharpParameter( "int", "min", "int.MinValue" ), new CSharpParameter( "int", "max", "int.MaxValue" ) },
				                                  "validator.GetInt( new ValidationErrorHandler( subject ), control.GetPostBackValue( postBackValues ), min, max )" );
				writeNumberAsSelectListFormItemGetters( writer, field, "int?", "validator.GetInt( new ValidationErrorHandler( subject ), control.Value )" );
				writeHtmlAndFileFormItemGetters( writer, field, "int?" );
				writeFileCollectionFormItemGetters( writer, field, "int" );
			}
			if( field.TypeIs( typeof( int? ) ) ) {
				writeNumberAsTextFormItemGetters( writer,
				                                  field,
				                                  "int?",
				                                  getAllowEmptyParameter( true ).ToSingleElementArray(),
				                                  "validator.GetNullableInt( new ValidationErrorHandler( subject ), control.GetPostBackValue( postBackValues ), allowEmpty )" );
				writeNumberAsSelectListFormItemGetters( writer, field, "int?", "validator.GetNullableInt( new ValidationErrorHandler( subject ), control.Value, true )" );
				writeHtmlAndFileFormItemGetters( writer, field, "int?" );
			}

			if( field.TypeIs( typeof( short ) ) ) {
				writeNumberAsTextFormItemGetters( writer,
				                                  field,
				                                  "short?",
				                                  new[] { new CSharpParameter( "short", "min", "short.MinValue" ), new CSharpParameter( "short", "max", "short.MaxValue" ) },
				                                  "validator.GetShort( new ValidationErrorHandler( subject ), control.GetPostBackValue( postBackValues ), min, max )" );
				writeNumberAsSelectListFormItemGetters( writer, field, "short?", "validator.GetShort( new ValidationErrorHandler( subject ), control.Value )" );
			}
			if( field.TypeIs( typeof( short? ) ) ) {
				writeNumberAsTextFormItemGetters( writer,
				                                  field,
				                                  "short?",
				                                  new[]
				                                  	{
				                                  		getAllowEmptyParameter( true ), new CSharpParameter( "short", "min", "short.MinValue" ),
				                                  		new CSharpParameter( "short", "max", "short.MaxValue" )
				                                  	},
				                                  "validator.GetNullableShort( new ValidationErrorHandler( subject ), control.GetPostBackValue( postBackValues ), allowEmpty, min, max )" );
				writeNumberAsSelectListFormItemGetters( writer, field, "short?", "validator.GetNullableShort( new ValidationErrorHandler( subject ), control.Value, true )" );
			}

			if( field.TypeIs( typeof( byte ) ) ) {
				writeNumberAsTextFormItemGetters( writer,
				                                  field,
				                                  "byte?",
				                                  new[] { new CSharpParameter( "byte", "min", "byte.MinValue" ), new CSharpParameter( "byte", "max", "byte.MaxValue" ) },
				                                  "validator.GetByte( new ValidationErrorHandler( subject ), control.GetPostBackValue( postBackValues ), min, max )" );
				writeNumberAsSelectListFormItemGetters( writer, field, "byte?", "validator.GetByte( new ValidationErrorHandler( subject ), control.Value )" );
			}
			if( field.TypeIs( typeof( byte? ) ) ) {
				writeNumberAsTextFormItemGetters( writer,
				                                  field,
				                                  "byte?",
				                                  getAllowEmptyParameter( true ).ToSingleElementArray(),
				                                  "validator.GetNullableByte( new ValidationErrorHandler( subject ), control.GetPostBackValue( postBackValues ), allowEmpty )" );
				writeNumberAsSelectListFormItemGetters( writer, field, "byte?", "validator.GetNullableByte( new ValidationErrorHandler( subject ), control.Value, true )" );
			}

			if( field.TypeIs( typeof( decimal ) ) ) {
				writeNumberAsTextFormItemGetters( writer,
				                                  field,
				                                  "decimal?",
				                                  new[]
				                                  	{
				                                  		new CSharpParameter( "decimal", "min", "Validator.SqlDecimalDefaultMin" ),
				                                  		new CSharpParameter( "decimal", "max", "Validator.SqlDecimalDefaultMax" )
				                                  	},
				                                  "validator.GetDecimal( new ValidationErrorHandler( subject ), control.GetPostBackValue( postBackValues ), min, max )" );
				writeNumberAsSelectListFormItemGetters( writer, field, "decimal?", "validator.GetDecimal( new ValidationErrorHandler( subject ), control.Value )" );
				writeHtmlAndFileFormItemGetters( writer, field, "decimal?" );
				writeFileCollectionFormItemGetters( writer, field, "decimal" );
			}
			if( field.TypeIs( typeof( decimal? ) ) ) {
				writeNumberAsTextFormItemGetters( writer,
				                                  field,
				                                  "decimal?",
				                                  new[]
				                                  	{
				                                  		getAllowEmptyParameter( true ), new CSharpParameter( "decimal", "min", "Validator.SqlDecimalDefaultMin" ),
				                                  		new CSharpParameter( "decimal", "max", "Validator.SqlDecimalDefaultMax" )
				                                  	},
				                                  "validator.GetNullableDecimal( new ValidationErrorHandler( subject ), control.GetPostBackValue( postBackValues ), allowEmpty, min, max )" );
				writeNumberAsSelectListFormItemGetters( writer,
				                                        field,
				                                        "decimal?",
				                                        "validator.GetNullableDecimal( new ValidationErrorHandler( subject ), control.Value, true )" );
				writeHtmlAndFileFormItemGetters( writer, field, "decimal?" );
			}
		}

		private static void writeNumberAsTextFormItemGetters( TextWriter writer, ModificationField field, string valueParamTypeName,
		                                                      IEnumerable<CSharpParameter> optionalValidationParams, string validationMethodExpressionOrBlock ) {
			writeFormItemGetters( writer,
			                      field,
			                      "EwfTextBox",
			                      "Text",
			                      valueParamTypeName,
			                      "null",
			                      new CSharpParameter[ 0 ],
			                      new CSharpParameter[ 0 ],
			                      new CSharpParameter[ 0 ],
			                      optionalValidationParams,
			                      "new EwfTextBox( v.ObjectToString( true ) )",
			                      validationMethodExpressionOrBlock,
			                      "true" );
		}

		private static void writeNumberAsSelectListFormItemGetters( TextWriter writer, ModificationField field, string valueParamTypeName,
		                                                            string validationMethodExpressionOrBlock ) {
			writeFormItemGetters( writer,
			                      field,
			                      "EwfListControl",
			                      "SelectList",
			                      valueParamTypeName,
			                      "null",
			                      new[] { new CSharpParameter( "System.Action<EwfListControl>", "itemAdder" ) },
			                      new CSharpParameter[ 0 ],
			                      new[]
			                      	{
			                      		new CSharpParameter( "EwfListControl.ListControlType", "listControlType", "EwfListControl.ListControlType.DropDownList" ),
			                      		new CSharpParameter( "bool", "autoPostBack", "false" )
			                      	},
			                      new CSharpParameter[ 0 ],
			                      "{ var control = new EwfListControl { Type = listControlType, AutoPostBack = autoPostBack }; itemAdder( control ); control.Value = v.ObjectToString( true ); return control; }",
			                      validationMethodExpressionOrBlock,
			                      "true" );
		}

		private static void writeFileCollectionFormItemGetters( TextWriter writer, ModificationField field, string valueParamTypeName ) {
			writeFormItemGetters( writer,
			                      field,
			                      "BlobFileCollectionManager",
			                      "FileCollection",
			                      valueParamTypeName,
			                      "-1",
			                      new CSharpParameter( "bool", "sortByName" ).ToSingleElementArray(),
			                      new CSharpParameter[ 0 ],
			                      new CSharpParameter[ 0 ],
			                      new CSharpParameter[ 0 ],
			                      "{ var control = new BlobFileCollectionManager( sortByName: sortByName ); control.LoadData( AppRequestState.PrimaryDatabaseConnection, (int)v ); return control; }",
			                      "",
			                      "true" );
		}

		private static void writeHtmlAndFileFormItemGetters( TextWriter writer, ModificationField field, string valueParamTypeName ) {
			writeFormItemGetters( writer,
			                      field,
			                      "HtmlBlockEditor",
			                      "Html",
			                      valueParamTypeName,
			                      "null",
			                      new CSharpParameter( "out DbMethod", "modificationMethod" ).ToSingleElementArray(),
			                      new CSharpParameter[ 0 ],
			                      new CSharpParameter[ 0 ],
			                      new[]
			                      	{
			                      		new CSharpParameter( "System.Func<bool>", "destroyHtmlPredicate", "null" ),
			                      		new CSharpParameter( "System.Func<Validator, string, string>", "extraValidation", "null" )
			                      	},
			                      "{ var control = new HtmlBlockEditor( (int?)v ); control.LoadData( AppRequestState.PrimaryDatabaseConnection ); mm = cn => " +
			                      field.PropertyName + " = control.ModifyData( cn ); return control; }",
			                      "control.ValidateFormValues( postBackValues, validator, destroyHtml: destroyHtmlPredicate != null ? destroyHtmlPredicate() : false, extraValidation: extraValidation )",
			                      "true",
			                      preFormItemGetterStatements: "DbMethod mm = null;",
			                      postFormItemGetterStatements: "modificationMethod = mm;" );
			writeFormItemGetters( writer,
			                      field,
			                      "BlobFileManager",
			                      "File",
			                      valueParamTypeName,
			                      "null",
			                      new CSharpParameter( "out DbMethod", "modificationMethod" ).ToSingleElementArray(),
			                      new CSharpParameter[ 0 ],
			                      new CSharpParameter[ 0 ],
			                      new CSharpParameter( "bool", "requireUploadIfNoFile", "false" ).ToSingleElementArray(),
			                      "{ var control = new BlobFileManager(); if( v.HasValue ) control.LoadData( AppRequestState.PrimaryDatabaseConnection, (int)v.Value ); mm = cn => " +
			                      field.PropertyName + " = control.ModifyData( cn ); return control; }",
			                      "control.ValidateFormValues( validator, subject, requireUploadIfNoFile )",
			                      "true",
			                      preFormItemGetterStatements: "DbMethod mm = null;",
			                      postFormItemGetterStatements: "modificationMethod = mm;" );
		}

		private static void writeBoolFormItemGetters( TextWriter writer, ModificationField field ) {
			if( field.TypeIs( typeof( bool ) ) ) {
				writeFormItemGetters( writer,
				                      field,
				                      "EwfCheckBox",
				                      "CheckBox",
				                      "bool",
				                      "false",
				                      new CSharpParameter[ 0 ],
				                      new CSharpParameter[ 0 ],
				                      new[] { new CSharpParameter( "bool", "putLabelOnCheckBox", "true" ), new CSharpParameter( "bool", "autoPostBack", "false" ) },
				                      new CSharpParameter[ 0 ],
				                      "putLabelOnCheckBox ? new EwfCheckBox( v.Value, ls ) { AutoPostBack = autoPostBack } : new EwfCheckBox( v.Value ) { AutoPostBack = autoPostBack }",
				                      "control.IsCheckedInPostBack( postBackValues )",
				                      "!putLabelOnCheckBox" );
				writeFormItemGetters( writer,
				                      field,
				                      "EwfListControl",
				                      "SelectList",
				                      "bool",
				                      "false",
				                      new CSharpParameter[ 0 ],
				                      new CSharpParameter[ 0 ],
				                      new[]
				                      	{
				                      		new CSharpParameter( "EwfListControl.ListControlType", "listControlType", "EwfListControl.ListControlType.DropDownList" ),
				                      		new CSharpParameter( "bool", "autoPostBack", "false" )
				                      	},
				                      new CSharpParameter[ 0 ],
				                      "{ var control = new EwfListControl { Type = listControlType, AutoPostBack = autoPostBack }; control.FillWithYesNo(); control.Value = v.Value.ToString(); return control; }",
				                      "bool.Parse( control.GetPostBackValue( postBackValues ) )",
				                      "true" );
			}
		}

		private static void writeDateFormItemGetters( TextWriter writer, ModificationField field ) {
			if( field.TypeIs( typeof( DateTime? ) ) ) {
				writeFormItemGetters( writer,
				                      field,
				                      "DatePicker",
				                      "Date",
				                      "System.DateTime?",
				                      "null",
				                      new CSharpParameter[ 0 ],
				                      new CSharpParameter[ 0 ],
				                      new CSharpParameter( "bool", "constrainToSqlSmallDateTimeRange", "true" ).ToSingleElementArray(),
				                      getAllowEmptyParameter( true ).ToSingleElementArray(),
				                      "new DatePicker( v ) { ConstrainToSqlSmallDateTimeRange = constrainToSqlSmallDateTimeRange }",
				                      "control.ValidateAndGetNullablePostBackDate( postBackValues, validator, new ValidationErrorHandler( subject ), allowEmpty )",
				                      "true" );
			}
			if( field.TypeIs( typeof( DateTime ) ) ) {
				writeFormItemGetters( writer,
				                      field,
				                      "DatePicker",
				                      "Date",
				                      "System.DateTime?",
				                      "null",
				                      new CSharpParameter[ 0 ],
				                      new CSharpParameter[ 0 ],
				                      new CSharpParameter( "bool", "constrainToSqlSmallDateTimeRange", "true" ).ToSingleElementArray(),
				                      new CSharpParameter[ 0 ],
				                      "new DatePicker( v ) { ConstrainToSqlSmallDateTimeRange = constrainToSqlSmallDateTimeRange }",
				                      "control.ValidateAndGetPostBackDate( postBackValues, validator, new ValidationErrorHandler( subject ) )",
				                      "true" );
			}
		}

		private static void writeEnumerableFormItemGetters( TextWriter writer, ModificationField field ) {
			if( !field.EnumerableElementTypeName.Any() )
				return;
			writeFormItemGetters( writer,
			                      field,
			                      "EwfCheckBoxList<" + field.EnumerableElementTypeName + ">",
			                      "CheckBoxList",
			                      field.TypeName,
			                      "null",
			                      new CSharpParameter( "IEnumerable<EwfListItem<" + field.EnumerableElementTypeName + ">>", "items" ).ToSingleElementArray(),
			                      new CSharpParameter[ 0 ],
			                      new[]
			                      	{
			                      		new CSharpParameter( "string", "caption", "\"\"" ), new CSharpParameter( "bool", "includeSelectAndDeselectAllButtons", "false" ),
			                      		new CSharpParameter( "byte", "numberOfColumns", "1" )
			                      	},
			                      new CSharpParameter[ 0 ],
			                      "new EwfCheckBoxList<" + field.EnumerableElementTypeName + ">( items, v ?? new " + field.EnumerableElementTypeName +
			                      "[ 0 ], caption: caption, includeSelectAndDeselectAllButtons: includeSelectAndDeselectAllButtons, numberOfColumns: numberOfColumns )",
			                      "control.GetSelectedItemIdsInPostBack( postBackValues )",
			                      "true" );
		}

		private static void writeFormItemGetters( TextWriter writer, ModificationField field, string controlType, string controlTypeForName, string valueParamTypeName,
		                                          string valueParamDefaultValue, IEnumerable<CSharpParameter> requiredControlParams,
		                                          IEnumerable<CSharpParameter> requiredValidationParams, IEnumerable<CSharpParameter> optionalControlParams,
		                                          IEnumerable<CSharpParameter> optionalValidationParams, string controlGetterExpressionOrBlock,
		                                          string validationMethodExpressionOrBlock, string putLabelOnFormItemExpression,
		                                          string preFormItemGetterStatements = "", string postFormItemGetterStatements = "" ) {
			writeFormItemGetterWithoutValueParams( writer,
			                                       controlType,
			                                       field,
			                                       controlTypeForName,
			                                       requiredControlParams,
			                                       requiredValidationParams,
			                                       optionalControlParams,
			                                       optionalValidationParams,
			                                       validationMethodExpressionOrBlock.Any() );
			writeFormItemGetterWithValueParams( writer,
			                                    controlType,
			                                    field,
			                                    controlTypeForName,
			                                    valueParamTypeName,
			                                    valueParamDefaultValue,
			                                    requiredControlParams,
			                                    requiredValidationParams,
			                                    optionalControlParams,
			                                    optionalValidationParams,
			                                    preFormItemGetterStatements,
			                                    controlGetterExpressionOrBlock,
			                                    validationMethodExpressionOrBlock,
			                                    putLabelOnFormItemExpression,
			                                    postFormItemGetterStatements );
		}

		private static void writeFormItemGetterWithoutValueParams( TextWriter writer, string controlType, ModificationField field, string controlTypeForName,
		                                                           IEnumerable<CSharpParameter> requiredControlParams,
		                                                           IEnumerable<CSharpParameter> requiredValidationParams,
		                                                           IEnumerable<CSharpParameter> optionalControlParams,
		                                                           IEnumerable<CSharpParameter> optionalValidationParams, bool includeValidationListParam ) {
			// NOTE: The "out" parameter logic is a hack. We need to improve CSharpParameter.
			var body = "return Get" + field.PascalCasedName + controlTypeForName + "FormItem( false, " +
			           requiredControlParams.Concat( requiredValidationParams ).Select( i => ( i.Name == "modificationMethod" ? "out " : "" ) + i.Name ).
			           	GetCommaDelimitedStringFromCollection().AppendDelimiter( ", " ) + "labelAndSubject: labelAndSubject, " +
			           optionalControlParams.Select( i => i.Name + ": " + i.Name ).GetCommaDelimitedStringFromCollection().AppendDelimiter( ", " ) +
			           "cellSpan: cellSpan, textAlignment: textAlignment" +
			           optionalValidationParams.Select( i => i.Name + ": " + i.Name ).GetCommaDelimitedStringFromCollection().PrependDelimiter( ", " ) +
			           ( includeValidationListParam ? ", validationList: validationList" : "" ) + " );";

			writeFormItemGetter( writer,
			                     controlType,
			                     field,
			                     controlTypeForName,
			                     "",
			                     requiredControlParams,
			                     requiredValidationParams,
			                     "",
			                     optionalControlParams,
			                     optionalValidationParams,
			                     includeValidationListParam,
			                     body );
		}

		private static void writeFormItemGetterWithValueParams( TextWriter writer, string controlType, ModificationField field, string controlTypeForName,
		                                                        string valueParamTypeName, string valueParamDefaultValue,
		                                                        IEnumerable<CSharpParameter> requiredControlParams,
		                                                        IEnumerable<CSharpParameter> requiredValidationParams,
		                                                        IEnumerable<CSharpParameter> optionalControlParams,
		                                                        IEnumerable<CSharpParameter> optionalValidationParams, string preFormItemGetterStatements,
		                                                        string controlGetterExpressionOrBlock, string validationMethodExpressionOrBlock,
		                                                        string putLabelOnFormItemExpression, string postFormItemGetterStatements ) {
			var validationMethod = "( control, postBackValues, subject, validator ) => " + validationMethodExpressionOrBlock;
			var formItemGetterStatement = "var formItem = Get" + field.PascalCasedName + "FormItem( useValueParameter, ( v, ls ) => " + controlGetterExpressionOrBlock +
			                              ( validationMethodExpressionOrBlock.Any() ? ", " + validationMethod : "" ) +
			                              ", labelAndSubject: labelAndSubject, putLabelOnFormItem: " + putLabelOnFormItemExpression +
			                              ", value: value, cellSpan: cellSpan, textAlignment: textAlignment" +
			                              ( validationMethodExpressionOrBlock.Any() ? ", validationList: validationList" : "" ) + " );";
			writeFormItemGetter( writer,
			                     controlType,
			                     field,
			                     controlTypeForName,
			                     valueParamTypeName,
			                     requiredControlParams,
			                     requiredValidationParams,
			                     valueParamDefaultValue,
			                     optionalControlParams,
			                     optionalValidationParams,
			                     validationMethodExpressionOrBlock.Any(),
			                     StringTools.ConcatenateWithDelimiter( Environment.NewLine,
			                                                           preFormItemGetterStatements,
			                                                           formItemGetterStatement,
			                                                           postFormItemGetterStatements,
			                                                           "return formItem;" ) );
		}

		private static void writeFormItemGetter( TextWriter writer, string controlType, ModificationField field, string controlTypeForName, string valueParamTypeName,
		                                         IEnumerable<CSharpParameter> requiredControlParams, IEnumerable<CSharpParameter> requiredValidationParams,
		                                         string valueParamDefaultValue, IEnumerable<CSharpParameter> optionalControlParams,
		                                         IEnumerable<CSharpParameter> optionalValidationParams, bool includeValidationListParam, string body ) {
			CodeGenerationStatics.AddSummaryDocComment( writer,
			                                            "Creates a form item for the field, which includes its label, its control, and its validation. The validation is added to the specified validation list, or the page's post back data modification if no validation list is specified."
			                                            	.ConcatenateWithSpace( deferredCallWarning ) );

			var parameters = new List<CSharpParameter>();
			if( valueParamTypeName.Length > 0 )
				parameters.Add( new CSharpParameter( "bool", "useValueParameter" ) );
			parameters.AddRange( requiredControlParams );
			parameters.AddRange( requiredValidationParams );
			parameters.Add( new CSharpParameter( "string", "labelAndSubject", "\"" + getDefaultLabelAndSubject( field ) + "\"" ) );
			if( valueParamTypeName.Length > 0 )
				parameters.Add( new CSharpParameter( valueParamTypeName, "value", valueParamDefaultValue ) );
			parameters.AddRange( optionalControlParams );
			parameters.Add( new CSharpParameter( "int?", "cellSpan", "null" ) );
			parameters.Add( new CSharpParameter( "TextAlignment", "textAlignment", "TextAlignment.NotSpecified" ) );
			parameters.AddRange( optionalValidationParams );
			if( includeValidationListParam )
				parameters.Add( new CSharpParameter( "ValidationList", "validationList", "null" ) );

			writer.WriteLine( "public FormItem<" + controlType + "> Get" + field.PascalCasedName + controlTypeForName + "FormItem( " +
			                  parameters.Select( i => i.MethodSignatureDeclaration ).GetCommaDelimitedStringFromCollection() + " ) {" );
			writer.WriteLine( body );
			writer.WriteLine( "}" );
		}

		private static CSharpParameter getAllowEmptyParameter( bool isOptional ) {
			return new CSharpParameter( "bool", "allowEmpty", isOptional ? "true" : "" );
		}

		private static void writeGenericGetterWithoutValueParams( TextWriter writer, ModificationField field, bool? includeValidationMethodReturnValue ) {
			var controlGetter = "( v, ls ) => controlGetter( v" + ( field.TypeName != field.NullableTypeName ? ".Value" : "" ) + ", ls )";
			var body = "return Get" + field.PascalCasedName + "FormItem( false, " + controlGetter +
			           ( includeValidationMethodReturnValue.HasValue ? ", validationMethod" : "" ) +
			           ", labelAndSubject: labelAndSubject, putLabelOnFormItem: putLabelOnFormItem, cellSpan: cellSpan, textAlignment: textAlignment" +
			           ( includeValidationMethodReturnValue.HasValue ? ", validationList: validationList" : "" ) + " );";
			writeGenericGetter( writer, field, false, includeValidationMethodReturnValue, body );
		}

		private static void writeGenericGetterWithValueParams( TextWriter writer, ModificationField field, bool? includeValidationMethodReturnValue ) {
			var control = "useValueParameter ? controlGetter( value, labelAndSubject ) : controlGetter( " + field.PropertyName + ", labelAndSubject )";
			var body = "return FormItem.Create( putLabelOnFormItem ? labelAndSubject : \"\", " + control + ", cellSpan: cellSpan, textAlignment: textAlignment" +
			           ( includeValidationMethodReturnValue.HasValue
			             	? ", validationGetter: " + getValidationGetter( field, includeValidationMethodReturnValue.Value )
			             	: "" ) + " );";
			writeGenericGetter( writer, field, true, includeValidationMethodReturnValue, body );
		}

		private static string getValidationGetter( ModificationField field, bool includeValidationMethodReturnValue ) {
			return "control => new Validation( ( postBackValues, validator ) => " + ( includeValidationMethodReturnValue ? field.PropertyName + " = " : "" ) +
			       " validationMethod( control, postBackValues, labelAndSubject, validator ), validationList ?? EwfPage.Instance.PostBackDataModification )";
		}

		private static void writeGenericGetter( TextWriter writer, ModificationField field, bool includeValueParams, bool? includeValidationMethodReturnValue,
		                                        string body ) {
			CodeGenerationStatics.AddSummaryDocComment( writer,
			                                            "Creates a form item for the field, which includes its label, its control, and its validation. The validation is added to the specified validation list, or the page's post back data modification if no validation list is specified."
			                                            	.ConcatenateWithSpace( deferredCallWarning ) );

			var parameters = new List<CSharpParameter>();
			if( includeValueParams )
				parameters.Add( new CSharpParameter( "bool", "useValueParameter" ) );
			parameters.Add( new CSharpParameter( "System.Func<" + ( includeValueParams ? field.NullableTypeName : field.TypeName ) + ",string,ControlType>",
			                                     "controlGetter" ) );
			if( includeValidationMethodReturnValue.HasValue ) {
				parameters.Add( includeValidationMethodReturnValue.Value
				                	? new CSharpParameter( "System.Func<ControlType,PostBackValueDictionary,string,Validator," + field.TypeName + ">", "validationMethod" )
				                	: new CSharpParameter( "System.Action<ControlType,PostBackValueDictionary,string,Validator>", "validationMethod" ) );
			}
			parameters.Add( new CSharpParameter( "string", "labelAndSubject", "\"" + getDefaultLabelAndSubject( field ) + "\"" ) );
			parameters.Add( new CSharpParameter( "bool", "putLabelOnFormItem", "true" ) );
			if( includeValueParams )
				parameters.Add( new CSharpParameter( field.NullableTypeName, "value", field.TypeIs( typeof( string ) ) ? "\"\"" : "null" ) );
			parameters.Add( new CSharpParameter( "int?", "cellSpan", "null" ) );
			parameters.Add( new CSharpParameter( "TextAlignment", "textAlignment", "TextAlignment.NotSpecified" ) );
			if( includeValidationMethodReturnValue.HasValue )
				parameters.Add( new CSharpParameter( "ValidationList", "validationList", "null" ) );

			writer.WriteLine( "public FormItem<ControlType> Get" + field.PascalCasedName + "FormItem<ControlType>( " +
			                  parameters.Select( i => i.MethodSignatureDeclaration ).GetCommaDelimitedStringFromCollection() + " ) where ControlType: Control {" );
			writer.WriteLine( body );
			writer.WriteLine( "}" );
		}

		private static string getDefaultLabelAndSubject( ModificationField field ) {
			var result = field.PascalCasedName.ToEnglishFromCamel();
			if( result.ToLower().EndsWith( " id" ) )
				result = result.Substring( 0, result.Length - 3 );
			return result;
		}
	}
}