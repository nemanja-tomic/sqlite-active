using System;

namespace SQLiteActive {
	public class SQLiteActiveException: Exception {
		public SQLiteActiveException(): base() {
		}
		public SQLiteActiveException(string asMessage): base(asMessage) {
		}
		public SQLiteActiveException(string asMessage, Exception aoInnerException): base(asMessage, aoInnerException) {
		}
	}
}

