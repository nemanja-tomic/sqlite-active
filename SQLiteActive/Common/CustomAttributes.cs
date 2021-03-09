using System;

namespace SQLiteActive {

	#region TableAttribute
	/// <summary>
	/// Defines model class as SQLiteActive model.
	/// SQLite table name should be passed as string in constructor, otherwise SQLiteActive uses class name as table name.
	/// </summary>
	[AttributeUsage (AttributeTargets.Class)]
	public class TableAttribute : Attribute {
		public string Name { get; set; }

		public TableAttribute() {
		}

		public TableAttribute(string asName) {
			Name = asName;
		}
	}
	#endregion

	#region ColumnAttribute
	/// <summary>
	/// Defines SQLite column name for model property.
	/// </summary>
	[AttributeUsage (AttributeTargets.Property)]
	public class ColumnAttribute : Attribute {
		public string Name { get; set; }

		public ColumnAttribute(string asName) {
			Name = asName;
		}
	}
	#endregion

	#region BaseKeyAttribute
	/// <summary>
	/// Parent attribute for column related SQL keywords.
	/// </summary>
	[AttributeUsage (AttributeTargets.Property)]
	public class BaseKeyAttribute: Attribute {
		public string SqlString { get; set; }

		public BaseKeyAttribute() {
		}

		public BaseKeyAttribute(string asSQL) {
			this.SqlString = asSQL;
		}
	}
	#endregion

	#region PrimaryKeyAttribute
	/// <summary>
	/// Primary key attribute, defines column as table primary key.
	/// </summary>
	public class PrimaryKeyAttribute : BaseKeyAttribute {
		public PrimaryKeyAttribute(): base("PRIMARY KEY") {
		}

		public PrimaryKeyAttribute(string asSql): base(asSql) {
		}
	}
	#endregion

	#region AutoIncrementAttribute
	/// <summary>
	/// Autoincrement attribute, defines column as autoincrement.
	/// </summary>
	public class AutoIncrementAttribute : BaseKeyAttribute {
		public AutoIncrementAttribute(): base("AUTOINCREMENT") {
		}

		public AutoIncrementAttribute(string asSql): base(asSql) {
		}
	}
	#endregion

	#region NotNullAttribute
	/// <summary>
	/// Not null attribute, defines column as not null.
	/// </summary>
	public class NotNullAttribute : BaseKeyAttribute {
		public NotNullAttribute(): base("NOT NULL") {
		}

		public NotNullAttribute(string asSql): base(asSql) {
		}
	}
	#endregion

	#region IndexedAttribute
	[AttributeUsage (AttributeTargets.Property)]
	public class IndexedAttribute : Attribute {
		public string Name { get; set; }

		public int Order { get; set; }

		public virtual bool Unique { get; set; }

		public IndexedAttribute() {
		}

		public IndexedAttribute(string asName, int aiOrder) {
			Name = asName;
			Order = aiOrder;
		}
	}
	#endregion

	#region IgnoreAttribute
	/// <summary>
	/// When assigned to model property, SQLiteActive engine will ignore it in database processes.
	/// </summary>
	public class IgnoreAttribute : BaseKeyAttribute {
	}
	#endregion

	#region UniqueAttribute
	[AttributeUsage (AttributeTargets.Property)]
	public class UniqueAttribute : IndexedAttribute {
		public override bool Unique {
			get { return true; }
			set { /* throw?  */ }
		}
	}
	#endregion

	#region MaxLengthAttribute
	[AttributeUsage (AttributeTargets.Property)]
	public class MaxLengthAttribute : Attribute {
		public int Value { get; private set; }

		public MaxLengthAttribute(int aiLength) {
			Value = aiLength;
		}
	}
	#endregion

	#region CollationAttribute
	[AttributeUsage (AttributeTargets.Property)]
	public class CollationAttribute: Attribute {
		public string Value { get; private set; }

		public CollationAttribute(string asCollation) {
			Value = asCollation;
		}
	}
	#endregion
}

