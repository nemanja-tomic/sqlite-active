using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using Mono.Data.Sqlite;
using System.Collections;
using System.Linq.Expressions;

namespace SQLiteActive {
	public class SQLiteEngine {

		#region Private properties

		private string isDatabasePath;
		private string isDatabaseName;
		private string isConnectionString;
		private QueryHelper ioQueryHelper;

		#endregion

		#region Public properties
		#endregion

		#region Constructors

		#region SQLiteEngine(string asDatabaseName, string asDatabasePath = "")
		/// <summary>
		/// Initializes a new instance of the <see cref="SQLiteActive.SQLiteEngine"/> class with provided database name and/or path to database file.
		/// </summary>
		/// <param name="asDatabaseName">The name of SQLite database to be created/loaded.</param>
		/// <param name="asDatabasePath">Path to SQLite database file.</param>
		public SQLiteEngine(string asDatabaseName, string asDatabasePath = "") {
			if (String.IsNullOrEmpty(asDatabaseName)) {
				throw new SQLiteActiveException(String.Format("Database name must be provided!"));
			}

			isDatabaseName = asDatabaseName;
			isDatabasePath = (String.IsNullOrEmpty(asDatabasePath)) ? isDatabasePath = AppDomain.CurrentDomain.BaseDirectory : isDatabasePath = asDatabasePath;

			if (!Directory.Exists(isDatabasePath)) {
				try {
					Directory.CreateDirectory(isDatabasePath);
				} catch (Exception aoException) {
					throw new SQLiteActiveException(aoException.Message, aoException);
				}
			}

			isConnectionString = String.Format("Data Source={0}{2}{1};Version=3;DateTimeFormat=UnixEpoch", isDatabasePath, isDatabaseName, System.IO.Path.DirectorySeparatorChar);
			ioQueryHelper = new QueryHelper(isConnectionString);
		}
		#endregion

		#endregion

		#region Public methods

		#region createTable<T>()
		/// <summary>
		/// Creates the table with structure corresponding to provided database model.
		/// </summary>
		/// <typeparam name="T">Valid SQLiteActive database model.</typeparam>
		public void createTable<T>() {
			ioQueryHelper.generateTable(typeof(T));
		}
		#endregion

		#region insert(Object aoModel)
		/// <summary>
		/// Inserts data from model instance to appropriate table.
		/// </summary>
		/// <param name="aoModel">Model instance containing data to be stored into database.</param>
		public int insert(Object aoModel) {
			return ioQueryHelper.insert(aoModel);
		}
		#endregion

		#region insertMany(List<Object> aoModels)
		/// <summary>
		/// Inserts multiple records to database.
		/// </summary>
		/// <param name="aoModels">List of model instances containing data to be stored into database.</param>
		public void insertMany(IList aoModels) {
			if (aoModels != null && aoModels.Count > 0) {
				foreach (Object toModel in aoModels) {
					insert(toModel);
				}
			} else {
				throw new SQLiteActiveException("Model list is either null or empty.");
			}
		}
		#endregion

		#region update(Object aoModel)
		/// <summary>
		/// Updates the specific record in database from provided table model. Primary key must be specified/valid, or exception will be thrown.
		/// </summary>
		/// <param name="aoModel">Model instance containing data for update.</param>
		public bool update(Object aoModel) {
			return ioQueryHelper.update(aoModel);
		}
		#endregion

		#region delete(Object aoModel)
		/// <summary>
		/// Delete record with specified primary key value.
		/// </summary>
		/// <param name="aoModel">Model instance containing primary key value, and table information needed for deleting.</param>
		public bool delete(Object aoModel) {
			return ioQueryHelper.delete(aoModel);
		}
		#endregion

		#region dropTable<T>()
		/// <summary>
		/// Drops the table.
		/// </summary>
		/// <returns><c>true</c>, if table was droped, <c>false</c> otherwise.</returns>
		/// <typeparam name="T">SQLiteActive valid table model.</typeparam>
		public bool dropTable<T>() {
			return ioQueryHelper.dropTable<T>();
		}
		#endregion

		#region selectAll<T>()
		/// <summary>
		/// Selects all records from table specified as generic type.
		/// </summary>
		/// <returns>All records from table.</returns>
		/// <typeparam name="T">Valid SQLiteActive table model.</typeparam>
		public List<T> selectAll<T>() where T : new() {
			return ioQueryHelper.selectAll<T>();
		}
		#endregion

		#region selectWhere<T>(Expression<Func<T, bool>> aoPredicate)
		/// <summary>
		/// Selects all records from database which fulfil the conditions specified as predicate statement.
		/// </summary>
		/// <returns>List of database records that fulfil the conditions.</returns>
		/// <param name="aoPredicate">Condition for select query, passed as predicate statement.</param>
		/// <typeparam name="T">Valid SQLiteActive table model.</typeparam>
		public List<T> selectWhere<T>(Expression<Func<T, bool>> aoPredicate) where T : new() {
			return ioQueryHelper.Where<T>(aoPredicate);
		}
		#endregion

		#endregion

		#region Private methods
		#endregion

	}
}