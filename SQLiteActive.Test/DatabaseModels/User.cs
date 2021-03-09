using System;

namespace SQLiteActive.Test {

	[Table ("Users")]
	public class User {

		[PrimaryKey, AutoIncrement]
		public int UserID { get; set; }

		public string Firstname { get; set; }

		[NotNull]
		public string Lastname { get; set; }

		[Ignore]
		public string Email { get; set; }

		public bool IsAdmin { get; set; }

		public DateTime DateOfBirth { get; set; }
	}
}

