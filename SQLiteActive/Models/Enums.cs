using System;

namespace SQLiteActive {
	[Flags]
	internal enum DataTypes {
		INTEGER,
		BIGINT,
		FLOAT,
		VARCHAR,
		BLOB
	}
}

