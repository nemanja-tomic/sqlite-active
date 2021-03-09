using System;
using System.Reflection;
using System.Linq.Expressions;

namespace SQLiteActive {
	internal static class Converter {
		#region Public methods

		#region resolveType(Type aoPropertyType)
		/// <summary>
		/// Resolves property type to appropriate query keyword.
		/// </summary>
		/// <returns>Correct query keyword corresponding to property type.</returns>
		/// <param name="aoPropertyType">Database model property type.</param>
		public static String resolveType(Type aoPropertyType) {
			String lsSqlType = String.Empty;

			switch (Type.GetTypeCode(aoPropertyType)) {
				case TypeCode.Boolean:
				case TypeCode.Byte:
				case TypeCode.UInt16:
				case TypeCode.SByte:
				case TypeCode.Int16:
				case TypeCode.Int32:
					lsSqlType = DataTypes.INTEGER.ToString();
					break;
				case TypeCode.UInt32:
				case TypeCode.Int64:
					lsSqlType = DataTypes.BIGINT.ToString();
					break;
				case TypeCode.Single:
				case TypeCode.Double:
				case TypeCode.Decimal:
					lsSqlType = DataTypes.FLOAT.ToString();
					break;
				case TypeCode.String:
					lsSqlType = DataTypes.VARCHAR.ToString();
					break;
				case TypeCode.DateTime:
					lsSqlType = DataTypes.INTEGER.ToString();
					break;
				default:
					if (aoPropertyType == typeof(byte[])) {
						lsSqlType = DataTypes.BLOB.ToString();
					} else if (aoPropertyType == typeof(Guid)) {
						lsSqlType = DataTypes.VARCHAR.ToString() + "(36)";
					} else if (aoPropertyType.IsEnum) {
						lsSqlType = DataTypes.INTEGER.ToString();
					} else {
						throw new SQLiteActiveException("Unsupported data type.");
					}
					break;
			}
			return lsSqlType;
		}
		#endregion

		#region convertToTimestamp(DateTime aoValue)
		/// <summary>
		/// Converts DateTime to Unix timestamp.
		/// </summary>
		/// <returns>Unix timestamp value for provided DateTime.</returns>
		/// <param name="aoValue">DateTime object.</param>
		public static int convertToTimestamp(DateTime aoValue) {
			TimeSpan aoReturnValue = (aoValue - new DateTime(1970, 1, 1, 0, 0, 0, 0));
			return (int)aoReturnValue.TotalSeconds;
		}
		#endregion

		#region convertToDateTime(int aiUnixTimeStamp)
		/// <summary>
		/// Converts Unix timestamp to DateTime.
		/// </summary>
		/// <returns>The DateTime object for provided Unix timestamp.</returns>
		/// <param name="aiUnixTimeStamp">Unix timestamp to be converted to DateTime.</param>
		public static DateTime convertToDateTime(int aiUnixTimeStamp) {
			DateTime loDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
			loDateTime = loDateTime.AddSeconds(aiUnixTimeStamp);
			return loDateTime;
		}
		#endregion

		#region modelToDbValue(PropertyInfo aoProperty, Object aoModel)
		/// <summary>
		/// Gets property type/value and converts it to appropriate one to be passed to SQLite database.
		/// </summary>
		/// <returns>Value to be passed to SQLite database.</returns>
		/// <param name="aoProperty">Property from table model.</param>
		/// <param name="aoModel">Database table model.</param>
		public static Object modelToDbValue(PropertyInfo aoProperty, Object aoModel) {
			Object loReturnValue;
			switch (Type.GetTypeCode(aoProperty.PropertyType)) {
				case TypeCode.DateTime:
					loReturnValue = convertToTimestamp((DateTime)aoProperty.GetValue(aoModel, null));
					break;
				case TypeCode.Boolean:
					loReturnValue = ((bool)aoProperty.GetValue(aoModel, null)) ? 1 : 0;
					break;
				default:
					if (aoProperty.PropertyType.IsEnum) {
						loReturnValue = Convert.ToInt32(aoProperty.GetValue(aoModel, null));
					} else {
						loReturnValue = aoProperty.GetValue(aoModel, null);
					}
					break;
			}
			return loReturnValue;
		}
		#endregion

		#region dbToModelValue(PropertyInfo aoProperty, Object aoReaderValue)
		/// <summary>
		/// Converts type/value from database to appropriate one in database model.
		/// </summary>
		/// <returns>Correct type/value to write into database model.</returns>
		/// <param name="aoProperty">PropertyInfo of database model property into which to insert converted data.</param>
		/// <param name="aoReaderValue">SQLiteDataReader indexed with desired propertyName.</param>
		public static Object dbToModelValue(PropertyInfo aoProperty, Object aoReaderValue) {
			Object loReturnValue;
			switch (Type.GetTypeCode(aoProperty.PropertyType)) {
				case TypeCode.DateTime:
					loReturnValue = convertToDateTime(Convert.ToInt32(aoReaderValue));
					break;
				case TypeCode.Boolean:
					loReturnValue = Convert.ToBoolean(aoReaderValue);
					break;
				default:
					if (aoProperty.PropertyType.IsEnum) {
						loReturnValue = Enum.ToObject(aoProperty.PropertyType, aoReaderValue);
					} else if (aoProperty.PropertyType == typeof(Guid)) {
						loReturnValue = new Guid(aoReaderValue.ToString());
					} else {
						loReturnValue = Convert.ChangeType(aoReaderValue, aoProperty.PropertyType);
					}
					break;
			}
			return loReturnValue;
		}
		#endregion

		#region getSqlName(Expression aoExpression)
		/// <summary>
		/// Gets the correct SQL syntax for provided expression operator.
		/// </summary>
		/// <returns>SQL operator syntax.</returns>
		/// <param name="aoExpression">Expression for which to get SQL operator.</param>
		public static String getSqlName(Expression aoExpression) {
			var loNodeType = aoExpression.NodeType;
			String lsReturnValue = String.Empty;

			switch (loNodeType) {
				case ExpressionType.GreaterThan:
					lsReturnValue = ">";
					break;
				case ExpressionType.GreaterThanOrEqual:
					lsReturnValue = ">=";
					break;
				case ExpressionType.LessThan:
					lsReturnValue = "<";
					break;
				case ExpressionType.LessThanOrEqual:
					lsReturnValue = "<=";
					break;
				case ExpressionType.And:
					lsReturnValue = "&";
					break;
				case ExpressionType.AndAlso:
					lsReturnValue = "and";
					break;
				case ExpressionType.Or:
					lsReturnValue = "|";
					break;
				case ExpressionType.OrElse:
					lsReturnValue = "or";
					break;
				case ExpressionType.Equal:
					lsReturnValue = "=";
					break;
				case ExpressionType.NotEqual:
					lsReturnValue = "!=";
					break;
				default:
					throw new NotSupportedException("Cannot get SQL for: " + loNodeType);
			}
			return lsReturnValue;
		}
		#endregion

		#endregion
	}
}

