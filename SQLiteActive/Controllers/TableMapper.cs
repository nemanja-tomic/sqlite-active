using System;
using System.Data.Linq;
using System.Reflection;
using System.Text;
using System.Linq;
using Mono.Data.Sqlite;
using System.Linq.Expressions;
using System.Collections.Generic;


namespace SQLiteActive {
	internal class TableMapper {

		#region Private properties

		private Object ioTableModel;
		private String isTableName;
		private Type ioTableType;

		#endregion

		#region Constructors

		#region TableMapper(Object aoTableModel)
		/// <summary>
		/// Initializes a new instance of the <see cref="SQLiteActive.TableMapper"/> class, and assigns provided table model and its type to private properties.
		/// </summary>
		/// <param name="aoTableModel">Table model for which to initialize TableMapper instance.</param>
		public TableMapper(Object aoTableModel) {
			ioTableType = aoTableModel.GetType();
			ioTableModel = aoTableModel;
			if (!checkTableModel()) {
				throw new SQLiteActiveException(String.Format("Provided table model is not valid SQLiteActive model. Check model custom attributes."));
			}
		}
		#endregion

		#region TableMapper(Type aoTableType)
		/// <summary>
		/// Initializes a new instance of the <see cref="SQLiteActive.TableMapper"/> class, and assigns provided table model type to private property.
		/// </summary>
		/// <param name="aoTableType">Table type for which to initialize TableMapper instance.</param>
		public TableMapper(Type aoTableType) {
			ioTableType = aoTableType;
			if (!checkTableModel()) {
				throw new SQLiteActiveException(String.Format("Provided table model is not valid SQLiteActive model. Check model custom attributes."));
			}
		}
		#endregion

		#endregion

		#region Public methods

		#region checkTableModel()
		/// <summary>
		/// Checks if assigned table model is valid SQLiteActive model.
		/// </summary>
		/// <returns><c>true</c>, if table model is valid, <c>false</c> otherwise.</returns>
		public bool checkTableModel() {
			bool lbReturnValue = false;

			TableAttribute loTableAttribute = (TableAttribute)ioTableType.GetCustomAttributes(true).Where(x => x.GetType() == typeof(TableAttribute)).FirstOrDefault();
			isTableName = (String.IsNullOrEmpty(loTableAttribute.Name)) ? ioTableType.Name : loTableAttribute.Name;

			if (!String.IsNullOrEmpty(isTableName)) {
				lbReturnValue = true;
			}

			return lbReturnValue;
		}
		#endregion

		#region generateTable()
		/// <summary>
		/// Generates the create table query for assigned table model type.
		/// </summary>
		/// <returns>Create table SQL query.</returns>
		public String generateCreateTableQuery() {

			if (!checkTableModel()) {
				throw new SQLiteActiveException(String.Format("Provided table model is not valid SQLiteActive model. Check model custom attributes."));
			}

			StringBuilder lsReturnValue = new StringBuilder();
			lsReturnValue.AppendLine("BEGIN TRANSACTION;");
			lsReturnValue.Append(String.Format("CREATE TABLE IF NOT EXISTS {0} (", isTableName));

			var loProperties = ioTableType.GetProperties();

			foreach (PropertyInfo toProperty in loProperties) {
				if (isIgnored(toProperty)) {
					continue;
				}

				var tsColumnName = toProperty.Name;
				var tsColumnKeywords = String.Empty;
				var tsType = Converter.resolveType(toProperty.PropertyType);

				if (isPrimaryKey(toProperty)) {
					tsColumnKeywords += String.Format(" {0}", getPrimaryKeyString(toProperty));
				}
				if (isAutoIncrement(toProperty)) {
					tsColumnKeywords += String.Format(" {0}", getAutoincrementString(toProperty));
				}
				if (!isNullable(toProperty)) {
					tsColumnKeywords += String.Format(" {0}", getNotNullString(toProperty));
				}

				lsReturnValue.Append(String.Format("{0} {2} {1},", tsColumnName, tsColumnKeywords, tsType));
			}

			lsReturnValue.Length--;
			lsReturnValue.Append(");");
			lsReturnValue.AppendLine("END TRANSACTION;");
			return lsReturnValue.ToString();
		}
		#endregion

		#region generateInsertQuery(Object aoModel)
		/// <summary>
		/// Generates the insert SQL query for assigned database model.
		/// </summary>
		/// <returns>The insert SQL query string.</returns>
		/// <param name="aoModel">Instance of database model for which to generate insert SQL query string.</param>
		public String generateInsertQuery(Object aoModel) {
			StringBuilder lsReturnValue = new StringBuilder();
			String lsColumns = String.Empty;
			String lsValues = " VALUES (";

			var loProperties = aoModel.GetType().GetProperties();

			lsReturnValue.AppendLine("BEGIN TRANSACTION;");
			lsReturnValue.Append(String.Format("INSERT INTO {0} (", isTableName));

			foreach (var toProperty in loProperties) {
				if (isIgnored(toProperty) || isAutoIncrement(toProperty)) {
					continue;
				}
				if ((!isNullable(toProperty) || isPrimaryKey(toProperty)) && 
					((toProperty.GetValue(aoModel, null) == null) || ((toProperty.PropertyType == typeof(Guid)) && (Guid)toProperty.GetValue(aoModel, null) == Guid.Empty))) {
					throw new SQLiteActiveException(String.Format("Null is not allowed for '{0}' column in '{1}' table.", toProperty.Name, isTableName));
				}
				lsColumns += String.Format("{0}, ", toProperty.Name);
				lsValues += String.Format("\"{0}\", ", Converter.modelToDbValue(toProperty, aoModel));
			}
			lsReturnValue.Append(lsColumns.Remove(lsColumns.Length - 2, 2) + ")");
			lsReturnValue.Append(lsValues.Remove(lsValues.Length - 2, 2) + ");");

			lsReturnValue.AppendLine("END TRANSACTION;");
			return lsReturnValue.ToString();
		}
		#endregion

		#region generateUpdateQuery(Object aoModel)
		/// <summary>
		/// Generates the update SQL query for assigned database model.
		/// </summary>
		/// <returns>The update SQL query string.</returns>
		/// <param name="aoModel">Instance of database model for which to generate update SQL query string.</param>
		public String generateUpdateQuery() {
			StringBuilder lsReturnValue = new StringBuilder();
			String lsValues = String.Empty;
			PropertyInfo loPrimaryKey = getPrimaryKeyProperty();
			if ((loPrimaryKey.GetValue(ioTableModel, null) == null) || ((loPrimaryKey.PropertyType == typeof(Guid)) && (Guid)loPrimaryKey.GetValue(ioTableModel, null) == Guid.Empty)) {
				throw new SQLiteActiveException("Primary key property cannot be passed as null!");
			}

			var loProperties = ioTableType.GetProperties();

			lsReturnValue.AppendLine("BEGIN TRANSACTION;");
			lsReturnValue.Append(String.Format("UPDATE {0} SET ", isTableName));

			foreach (var toProperty in loProperties) {
				if (isIgnored(toProperty) || isAutoIncrement(toProperty) || isPrimaryKey(toProperty)) {
					continue;
				}
				if (!isNullable(toProperty) && toProperty.GetValue(ioTableModel, null) == null) {
					throw new SQLiteActiveException(String.Format("{0} cannot be null.", toProperty.Name));
				}
				lsValues += String.Format("{0} = \'{1}\',", toProperty.Name, Converter.modelToDbValue(toProperty, ioTableModel));
			}

			lsReturnValue.Append(lsValues.Remove(lsValues.Length - 1, 1));
			lsReturnValue.Append(String.Format(" WHERE {0} = \'{1}\';", loPrimaryKey.Name, loPrimaryKey.GetValue(ioTableModel, null)));
			lsReturnValue.AppendLine("END TRANSACTION;");

			return lsReturnValue.ToString();
		}
		#endregion

		#region generateDropTableQuery()
		/// <summary>
		/// Generates the drop table query.
		/// </summary>
		/// <returns>The drop table query.</returns>
		public String generateDropTableQuery() {
			StringBuilder lsReturnValue = new StringBuilder();
			lsReturnValue.AppendLine("BEGIN TRANSACTION;");
			lsReturnValue.AppendLine(String.Format("DROP TABLE {0};", isTableName));
			lsReturnValue.AppendLine("END TRANSACTION;");
			return lsReturnValue.ToString();
		}
		#endregion

		#region generateDeleteQuery()
		/// <summary>
		/// Generates the delete query using assigned model's primary key.
		/// </summary>
		/// <returns>The delete query.</returns>
		public String generateDeleteQuery() {
			StringBuilder lsReturnValue = new StringBuilder();
			PropertyInfo loPrimaryKey = getPrimaryKeyProperty();
			if ((loPrimaryKey.GetValue(ioTableModel, null) == null) || ((loPrimaryKey.PropertyType == typeof(Guid)) && (Guid)loPrimaryKey.GetValue(ioTableModel, null) == Guid.Empty)) {
				throw new SQLiteActiveException("Primary key property cannot be passed as null!");
			}

			lsReturnValue.AppendLine("BEGIN TRANSACTION;");
			lsReturnValue.AppendLine(String.Format("DELETE FROM {0} WHERE {1} = \"{2}\";", isTableName, loPrimaryKey.Name, loPrimaryKey.GetValue(ioTableModel, null)));
			lsReturnValue.AppendLine("END TRANSACTION;");

			return lsReturnValue.ToString();
		}
		#endregion
		
		#region generateSelectQuery
		public String generateSelectQuery() {
			return String.Empty;
		}
		#endregion

		#region generateSelectAllQuery
		/// <summary>
		/// Generates the SQL query for selecting all records for assigned database model type.
		/// </summary>
		/// <returns>Select all SQL query string.</returns>
		public String generateSelectAllQuery() {
			StringBuilder lsReturnValue = new StringBuilder();
			lsReturnValue.AppendLine("BEGIN TRANSACTION;");
			lsReturnValue.AppendLine(String.Format("SELECT * FROM {0};", isTableName));
			lsReturnValue.AppendLine("END TRANSACTION;");
			return lsReturnValue.ToString();
		}
		#endregion

		#region generateSelectWithConditionQuery<T>(Expression<Func<T, bool>> aoExpression)
		/// <summary>
		/// Generates select query with specified conditions.
		/// </summary>
		/// <returns>Generated SQL select query.</returns>
		/// <param name="aoExpression">Select condition provided as predicate.</param>
		/// <typeparam name="T">Database table model.</typeparam>
		public String generateSelectWithConditionQuery<T>(Expression<Func<T, bool>> aoExpression) {
			StringBuilder lsReturnValue = new StringBuilder();
			lsReturnValue.AppendLine("BEGIN TRANSACTION;");

			lsReturnValue.Append(String.Format("SELECT * FROM {0} WHERE ", isTableName));
			lsReturnValue.Append(ExpressionHelper.resolveExpression(aoExpression.Body).CommandText);
			lsReturnValue.Append(";");

			lsReturnValue.AppendLine("END TRANSACTION;");
			return lsReturnValue.ToString();
		}
		#endregion

		#region fillModel(SqliteDataReader aoReader)
		/// <summary>
		/// Fills the model with fetched database record.
		/// </summary>
		/// <returns>New instance of assigned database model type, appropriately filled with provided SQLiteDataReader.</returns>
		/// <param name="aoReader">SQLiteDataReader with single database record.</param>
		public Object fillModel(SqliteDataReader aoReader) {
			PropertyInfo[] loProperties = ioTableType.GetProperties();
			var loModel = Activator.CreateInstance(ioTableType);

			foreach (PropertyInfo toProperty in loProperties) {
				if (isIgnored(toProperty)) {
					continue;
				}
				toProperty.SetValue(loModel, Converter.dbToModelValue(toProperty, aoReader[toProperty.Name]), null);
			}

			return loModel;
		}
		#endregion

		#endregion

		#region Private methods

		#region getPrimaryKeyName(Object aoModel)
		/// <summary>
		/// Gets the name of primary key property of assigned database model.
		/// </summary>
		/// <returns>Primary key property name.</returns>
		private PropertyInfo getPrimaryKeyProperty() {
			return ioTableType.GetProperties().Where(x => x.GetCustomAttributes(typeof(PrimaryKeyAttribute), true).Any()).FirstOrDefault();
		}
		#endregion

		#region isAutoIncrement(PropertyInfo aoProperty)
		/// <summary>
		/// Checks if provided property is declared as autoincrement.
		/// </summary>
		/// <returns><c>true</c>, if auto increment, <c>false</c> otherwise.</returns>
		/// <param name="aoProperty">Database model property to check autoincrement for.</param>
		private bool isAutoIncrement(PropertyInfo aoProperty) {
			var lbReturnValue = false;
			var loAttributes = aoProperty.GetCustomAttributes(typeof(AutoIncrementAttribute), true);

			if (loAttributes.Count() > 0) {
				lbReturnValue = true;
			}
			return lbReturnValue;
		}
		#endregion

		#region isIgnored(PropertyInfo aoProperty)
		/// <summary>
		/// Checks if provided property is declared as ignored.
		/// </summary>
		/// <returns><c>true</c>, if ignored, <c>false</c> otherwise.</returns>
		/// <param name="aoProperty">Database model property to check for ignore attribute.</param>
		private bool isIgnored(PropertyInfo aoProperty) {
			var lbReturnValue = false;
			var loAttributes = aoProperty.GetCustomAttributes(typeof(IgnoreAttribute), true);

			if (loAttributes.Count() > 0) {
				lbReturnValue = true;
			}
			return lbReturnValue;
		}
		#endregion

		#region isPrimaryKey(PropertyInfo aoProperty)
		/// <summary>
		/// Checks if provided property is declared as primary key.
		/// </summary>
		/// <returns><c>true</c>, if primary key, <c>false</c> otherwise.</returns>
		/// <param name="aoProperty">Database model property to check primary key for.</param>
		private bool isPrimaryKey(PropertyInfo aoProperty) {
			var lbReturnValue = false;
			var loAttributes = aoProperty.GetCustomAttributes(typeof(PrimaryKeyAttribute), true);

			if (loAttributes.Count() > 0) {
				lbReturnValue = true;
			}
			return lbReturnValue;
		}
		#endregion

		#region isNullable(PropertyInfo aoProperty)
		private bool isNullable(PropertyInfo aoProperty) {
			var lbReturnValue = true;
			var loAttributes = aoProperty.GetCustomAttributes(typeof(NotNullAttribute), true);

			if (loAttributes.Count() > 0) {
				lbReturnValue = false;
			}
			return lbReturnValue;
		}
		#endregion

		#region getPrimaryKeyString(PropertyInfo aoProperty)
		private String getPrimaryKeyString(PropertyInfo aoProperty) {
			String lsReturnValue = String.Empty;

			PrimaryKeyAttribute loAttribute = (PrimaryKeyAttribute)aoProperty.GetCustomAttributes(typeof(PrimaryKeyAttribute), true).FirstOrDefault();
			lsReturnValue = loAttribute.SqlString;

			return lsReturnValue;
		}
		#endregion

		#region getAutoincrementString(PropertyInfo aoProperty)
		/// <summary>
		/// Gets the autoincrement SQL string from AutoincrementAttribute custom attribute.
		/// </summary>
		/// <returns>The autoincrement SQL string.</returns>
		/// <param name="aoProperty">Database model property.</param>
		private String getAutoincrementString(PropertyInfo aoProperty) {
			String lsReturnValue = String.Empty;

			AutoIncrementAttribute loAttribute = (AutoIncrementAttribute)aoProperty.GetCustomAttributes(typeof(AutoIncrementAttribute), true).FirstOrDefault();
			lsReturnValue = loAttribute.SqlString;

			return lsReturnValue;
		}
		#endregion

		#region getNotNullString(PropertyInfo aoProperty)
		/// <summary>
		/// Gets the not null SQL string from NotNullAttribute custom attribute.
		/// </summary>
		/// <returns>The not null SQL string.</returns>
		/// <param name="aoProperty">Database model property.</param>
		private String getNotNullString(PropertyInfo aoProperty) {
			String lsReturnValue = String.Empty;

			NotNullAttribute loAttribute = (NotNullAttribute)aoProperty.GetCustomAttributes(typeof(NotNullAttribute), true).FirstOrDefault();
			lsReturnValue = loAttribute.SqlString;

			return lsReturnValue;
		}
		#endregion

		#endregion

	}
}

