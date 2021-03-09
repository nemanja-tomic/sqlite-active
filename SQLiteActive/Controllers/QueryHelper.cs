using System;
using System.Data;
using Mono.Data.Sqlite;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;

namespace SQLiteActive {
	internal class QueryHelper {

		#region Private properties

		private string isConnectionString;
		private TableMapper ioTableMapper;
		private static Object ioDatabaseLock = new object();

		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="SQLiteActive.QueryHelper"/> class and assigns provided connection string to private property.
		/// </summary>
		/// <param name="asConnectionString">SQLite database connection string.</param>
		public QueryHelper(string asConnectionString) {
			isConnectionString = asConnectionString;
		}
		#endregion

		#region Public methods

		#region generateTable(Object aoTableModel)
		/// <summary>
		/// Generates database table with structure corresponding to provided database model.
		/// </summary>
		/// <param name="aoTableModel">Database model to generate table for.</param>
		public void generateTable(Type aoTableModel) {
			ioTableMapper = new TableMapper(aoTableModel);
			String lsQuery = ioTableMapper.generateCreateTableQuery();

			using (SqliteConnection toConnection = new SqliteConnection(isConnectionString)) {
				toConnection.Open();
				SqliteCommand toCommand = new SqliteCommand(lsQuery, toConnection);
				executeNonQuery(toCommand);
			}
		}
		#endregion

		#region insert(Object aoModel)
		/// <summary>
		/// Inserts object data to appropriate table.
		/// </summary>
		/// <param name="aoModel">Database model instance for which to insert data to SQLite.</param>
		public int insert(Object aoModel) {
			int liInsertID = 0;
			ioTableMapper = new TableMapper(aoModel);

			String lsQuery = ioTableMapper.generateInsertQuery(aoModel);
			using (SqliteConnection toConnection = new SqliteConnection(isConnectionString)) {
				toConnection.Open();
				SqliteCommand toCommand = new SqliteCommand(lsQuery, toConnection);
				liInsertID = executeInsertQuery(toCommand);
			}
			return liInsertID;
		}
		#endregion

		#region update(Object aoModel)
		/// <summary>
		/// Updates the specific record based on primary key value in provided model instance.
		/// </summary>
		/// <param name="aoModel">Model containing data for update. Primary key must be specified and valid, or exception will be thrown.</param>
		public bool update(Object aoModel) {
			bool lbReturnValue = false;
			ioTableMapper = new TableMapper(aoModel);

			String lsQuery = ioTableMapper.generateUpdateQuery();
			using (SqliteConnection toConnection = new SqliteConnection(isConnectionString)) {
				toConnection.Open();
				SqliteCommand toCommand = new SqliteCommand(lsQuery, toConnection);
				var tiInsertResult = executeNonQuery(toCommand);
				if (tiInsertResult > 0) {
					lbReturnValue = true;
				}
			}

			return lbReturnValue;
		}
		#endregion

		public bool dropTable<T>(){
			bool lbReturnValue = false;
			ioTableMapper = new TableMapper(typeof(T));

			String lsQuery = ioTableMapper.generateDropTableQuery();
			using (SqliteConnection toConnection = new SqliteConnection(isConnectionString)) {
				toConnection.Open();
				SqliteCommand toCommand = new SqliteCommand(lsQuery, toConnection);
				executeNonQuery(toCommand);
				lbReturnValue = true;
			}

			return lbReturnValue;
		}

		#region delete(Object aoModel)
		/// <summary>
		/// Deletes the given object form the database using its primary key property.
		/// </summary>
		/// <param name="aoModel">The object to delete. Primary key must be specified and valid, or exception will be thrown.</param>
		public bool delete(Object aoModel) {
			bool lbReturnValue = false;
			ioTableMapper = new TableMapper(aoModel);

			String lsQuery = ioTableMapper.generateDeleteQuery();
			using (SqliteConnection toConnection = new SqliteConnection(isConnectionString)) {
				toConnection.Open();
				SqliteCommand toCommand = new SqliteCommand(lsQuery, toConnection);
				executeNonQuery(toCommand);
				lbReturnValue = true;
			}
			return lbReturnValue;
		}
		#endregion

		#region selectAll<T>()
		/// <summary>
		/// Selects all records for provided table (model type).
		/// </summary>
		/// <returns>All records from specified table.</returns>
		/// <typeparam name="T">Valid SQLiteActive model type for which to fetch data.</typeparam>
		public List<T> selectAll<T>() where T : new() {
			List<T> loReturnList = new List<T>();
			ioTableMapper = new TableMapper(typeof(T));

			String lsQuery = ioTableMapper.generateSelectAllQuery();
			using (SqliteConnection toConnection = new SqliteConnection(isConnectionString)) {
				toConnection.Open();
				SqliteCommand toCommand = new SqliteCommand(lsQuery, toConnection);
				using (SqliteDataReader toReader = toCommand.ExecuteReader()) {
					while (toReader.Read()) {
						loReturnList.Add((T)ioTableMapper.fillModel(toReader));
					}
				}
			}

			return loReturnList;
		}
		#endregion

		public List<T> Where<T>(Expression<Func<T, bool>> aoPredicate) where T : new() {
			List<T> loReturnList = new List<T>();
			ioTableMapper = new TableMapper(typeof(T));
			String lsQuery = ioTableMapper.generateSelectWithConditionQuery<T>(aoPredicate);

			using (SqliteConnection toConnection = new SqliteConnection(isConnectionString)) {
				toConnection.Open();
				SqliteCommand toCommand = new SqliteCommand(lsQuery, toConnection);
				using (SqliteDataReader toReader = toCommand.ExecuteReader()) {
					while (toReader.Read()) {
						loReturnList.Add((T)ioTableMapper.fillModel(toReader));
					}
				}
			}
			return loReturnList;
		}

		#endregion

		#region Private methods

		#region executeNonQuery(SqliteCommand aoCommand)
		/// <summary>
		/// Executes the query for provided SqliteCommand.
		/// </summary>
		/// <returns>Number of rows affected.</returns>
		/// <param name="aoCommand">Sqlite command object.</param>
		private int executeNonQuery(SqliteCommand aoCommand) {
			int liReturnValue = 0;
			lock (ioDatabaseLock) {
				liReturnValue = aoCommand.ExecuteNonQuery();
			}
			return liReturnValue;
		}

		#endregion

		#region executeScalar(SqliteCommand aoCommand)
		/// <summary>
		/// Executes query for provided SqliteCommand, and returns the first column of the first row.
		/// </summary>
		/// <returns>The first column of the first row.</returns>
		/// <param name="aoCommand">SqliteCommand to be executed.</param>
		private string executeScalar(SqliteCommand aoCommand) {
			string lsReturnValue = String.Empty;
			lock (ioDatabaseLock) {
				lsReturnValue = aoCommand.ExecuteScalar().ToString();
			}
			return lsReturnValue;
		}
		#endregion

		#region executeInsertQuery(SqliteCommand aoCommand)
		/// <summary>
		/// Executes the insert query and returns the last insert id.
		/// </summary>
		/// <returns>The last insert id.</returns>
		/// <param name="aoCommand">SQliteCommand object to be executed.</param>
		private int executeInsertQuery(SqliteCommand aoCommand) {
			int liInsertID = 0;

			lock (ioDatabaseLock) {
				aoCommand.ExecuteNonQuery();

				string lsSql = @"select last_insert_rowid()";
				aoCommand.CommandText = lsSql;
				liInsertID = Convert.ToInt32(aoCommand.ExecuteScalar());
			}

			return liInsertID;
		}
		#endregion
		
		#endregion

	}
}

