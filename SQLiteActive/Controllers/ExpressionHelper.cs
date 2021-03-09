using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace SQLiteActive {
	internal static class ExpressionHelper {

		#region Public methods

		#region resolveExpression(Expression aoBody)
		/// <summary>
		/// Resolves the predicate expression by turning it into sql string.
		/// </summary>
		/// <returns>SQL query string in CompileResult CommandText property.</returns>
		/// <param name="aoBody">Predicate expression.</param>
		public static CompileResult resolveExpression(Expression aoBody) {
			String lsText = String.Empty;
			CompileResult loReturnObject = new CompileResult();

			if (aoBody is BinaryExpression) {
				var toBinary = aoBody as BinaryExpression;
				var toLeft = resolveExpression(toBinary.Left);
				var toRight = resolveExpression(toBinary.Right);

				if (toLeft.CommandText == "?" && toLeft.Value == null) {
					lsText = compileNullBinaryExpression(toBinary, toRight);
				} else if (toRight.CommandText == "?" && toRight.Value == null) {
					lsText = compileNullBinaryExpression(toBinary, toLeft);
				} else {
					lsText = String.Format("({0} {1} {2})", toLeft.CommandText, Converter.getSqlName(toBinary), toRight.CommandText);
				}
				loReturnObject.CommandText = lsText;
			}
			switch (aoBody.NodeType) {
				case ExpressionType.Call:
					var toMethodExpression = aoBody as MethodCallExpression;
					var toArgs = new CompileResult[toMethodExpression.Arguments.Count];
					var toObj = toMethodExpression.Object != null ? resolveExpression(toMethodExpression.Object) : null;

					for (var i = 0; i < toArgs.Length; i++) {
						toArgs[i] = resolveExpression(toMethodExpression.Arguments[i]);
					}

					if (toMethodExpression.Method.Name == "Like" && toArgs.Length == 2) {
						lsText = String.Format("({0} LIKE %{1}%)", toArgs[0].CommandText, toArgs[1].CommandText);
					} else if (toMethodExpression.Method.Name == "Equals" && toArgs.Length == 1) {
						lsText = String.Format("({0} = ({1}))", toArgs[0].CommandText, toArgs[1].CommandText);
					} else if (toMethodExpression.Method.Name == "Contains") {
						lsText = String.Format("({0} LIKE '%{1}%')", toObj.CommandText, toArgs.Select(a => a.CommandText).FirstOrDefault());
					} else {
						lsText = String.Format("{2} {0} ({1})", toMethodExpression.Method.Name.ToLower(), String.Join(",", toArgs.Select(a => a.CommandText).ToArray()), toObj.CommandText);
					}
					loReturnObject.CommandText = lsText;
					break;
				case ExpressionType.Convert:
					var toConvert = aoBody as UnaryExpression;
					var toType = toConvert.Type;
					var toRecursiveResult = resolveExpression(toConvert.Operand);
					loReturnObject.CommandText = toRecursiveResult.CommandText;
					loReturnObject.Value = toRecursiveResult != null ? Convert.ChangeType(toRecursiveResult.Value, toType) : null;
					break;
				case ExpressionType.Constant:
					var toConstantExpression = aoBody as ConstantExpression;
					if (toConstantExpression.Type.IsPrimitive || toConstantExpression.Type == typeof(string)) {
						loReturnObject.CommandText = String.Format("\"{0}\"", toConstantExpression.Value.ToString());
					} else {
						loReturnObject.CommandText = "?";
						loReturnObject.Value = toConstantExpression.Value;
					}
					break;
				case ExpressionType.MemberAccess:
					var toMemberAccessExpression = aoBody as MemberExpression;
					if (toMemberAccessExpression != null && toMemberAccessExpression.Expression.NodeType == ExpressionType.Parameter) {
						loReturnObject.CommandText = String.Format("\"{0}\"", toMemberAccessExpression.Member.Name);
					} else {
						Object toObject = null;
						if (toMemberAccessExpression.Expression != null) {
							var toRecursive = resolveExpression(toMemberAccessExpression.Expression);
							if (toRecursive.Value == null) {
								throw new SQLiteActiveException("Member access failed to compile expression");
							}
							toObject = toRecursive.Value;
						}

						Object toValue = null;
						if (toMemberAccessExpression.Member.MemberType == MemberTypes.Property) {
							var toProp = (PropertyInfo)toMemberAccessExpression.Member;
							toValue = Converter.modelToDbValue(toProp, toObject);
							loReturnObject.CommandText = String.Format("\"{0}\"", toValue);
						} else if (toMemberAccessExpression.Member.MemberType == MemberTypes.Field) {
							var toField = (FieldInfo)toMemberAccessExpression.Member;
							toValue = toField.GetValue(toObject);
							loReturnObject.Value = toValue;
							loReturnObject.CommandText = String.Format("\"{0}\"", loReturnObject.Value);
						} else {
							throw new SQLiteActiveException("MemberExpr: " + toMemberAccessExpression.Member.MemberType);
						}
					}
					break;
				default:
					break;
			}
			return loReturnObject;
		}
		#endregion

		#region resolveExpressionTest
		public static CompileResult resolveExpressiona(Expression aoBody, List<Object> toQueryArgs) {
			String lsText = String.Empty;
			CompileResult loReturnObject = new CompileResult();

			if (aoBody is BinaryExpression) {
				var toBinary = aoBody as BinaryExpression;
				var toLeft = resolveExpressiona(toBinary.Left, toQueryArgs);
				var toRight = resolveExpressiona(toBinary.Right, toQueryArgs);

				if (toLeft.CommandText == "?" && toLeft.Value == null) {
					lsText = compileNullBinaryExpression(toBinary, toRight);
				} else if (toRight.CommandText == "?" && toRight.Value == null) {
					lsText = compileNullBinaryExpression(toBinary, toLeft);
				} else {
					lsText = String.Format("({0} {1} {2})", toLeft.CommandText, Converter.getSqlName(toBinary), toRight.CommandText);
				}
				loReturnObject.CommandText = lsText;
			}
			switch (aoBody.NodeType) {
				case ExpressionType.Call:
					var toMethodExpression = aoBody as MethodCallExpression;
					var toArgs = new CompileResult[toMethodExpression.Arguments.Count];
					var toObj = toMethodExpression.Object != null ? resolveExpressiona(toMethodExpression.Object, toQueryArgs) : null;

					for (var i = 0; i < toArgs.Length; i++) {
						toArgs[i] = resolveExpressiona(toMethodExpression.Arguments[i], toQueryArgs);
					}

					if (toMethodExpression.Method.Name == "Like" && toArgs.Length == 2) {
						lsText = String.Format("({0} LIKE {1})", toArgs[0].CommandText, toArgs[1].CommandText);
					} else if (toMethodExpression.Method.Name == "Equals" && toArgs.Length == 1) {
						lsText = String.Format("({0} = ({1}))", toArgs[0].CommandText, toArgs[1].CommandText);
					} else {
						lsText = String.Format("{0}({1})", toMethodExpression.Method.Name.ToLower(), String.Join(",", toArgs.Select(a => a.CommandText).ToArray()));
					}
					loReturnObject.CommandText = lsText;
					break;
				case ExpressionType.Convert:
					var toConvert = aoBody as UnaryExpression;
					var toType = toConvert.Type;
					var toRecursiveResult = resolveExpressiona(toConvert.Operand, toQueryArgs);
					loReturnObject.CommandText = toRecursiveResult.CommandText;
					loReturnObject.Value = toRecursiveResult != null ? Convert.ChangeType(toRecursiveResult.Value, toType) : null;
					break;
				case ExpressionType.Constant:
					var toConstantExpression = aoBody as ConstantExpression;
					toQueryArgs.Add(toConstantExpression.Value);
					loReturnObject.CommandText = "?";
					loReturnObject.Value = toConstantExpression.Value;
					break;
				case ExpressionType.MemberAccess:
					var toMemberAccessExpression = aoBody as MemberExpression;
					if (toMemberAccessExpression != null && toMemberAccessExpression.Expression.NodeType == ExpressionType.Parameter) {
						loReturnObject.CommandText = String.Format("\"{0}\"", toMemberAccessExpression.Member.Name);
					} else {
						Object toObject = null;
						if (toMemberAccessExpression.Expression != null) {
							var toRecursive = resolveExpressiona(toMemberAccessExpression.Expression, toQueryArgs);
							if (toRecursive.Value == null) {
								throw new SQLiteActiveException("Member access failed to compile expression");
							}
							if (toRecursive.CommandText == "?") {
								toQueryArgs.RemoveAt(toQueryArgs.Count - 1);
							}
							toObject = toRecursive.Value;
						}

						Object toValue = null;
						if (toMemberAccessExpression.Member.MemberType == MemberTypes.Property) {
							var toProp = (PropertyInfo)toMemberAccessExpression.Member;
							toValue = Converter.modelToDbValue(toProp, toObject);
							loReturnObject.CommandText = String.Format("\"{0}\"", toValue);
						} else if (toMemberAccessExpression.Member.MemberType == MemberTypes.Field) {
							var toField = (FieldInfo)toMemberAccessExpression.Member;
							toValue = toField.GetValue(toObject);
							loReturnObject.Value = toValue;
						} else {
							throw new SQLiteActiveException("MemberExpr: " + toMemberAccessExpression.Member.MemberType);
						}
					}
					break;
				default:
					break;
			}
			return loReturnObject;
		}
		#endregion

		#endregion
		
		#region Private methods

		#region compileNullBinaryExpression(BinaryExpression aoExpression, CompileResult aoParameter)
		/// <summary>
		/// Compiles a BinaryExpression where one of the parameters is null.
		/// </summary>
		/// <param name="aoParameter">The non-null parameter.</param>
		/// <param name="aoExpression">Binary expression to check nodeType for.</param>
		private static String compileNullBinaryExpression(BinaryExpression aoExpression, CompileResult aoParameter) {
			String lsReturnValue = String.Empty;

			switch (aoExpression.NodeType) {
				case ExpressionType.Equal:
					lsReturnValue = String.Format("({0} is ?)", aoParameter.CommandText);
					break;
					case ExpressionType.NotEqual:
					lsReturnValue = String.Format("({0} is not ?)", aoParameter.CommandText);
					break;
					default:
					throw new SQLiteActiveException("Cannot compile Null-BinaryExpression with type " + aoExpression.NodeType.ToString());
					break;
			}

			return lsReturnValue;
		}
		#endregion

		#endregion
		
	}
}

