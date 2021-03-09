using System;
using SQLiteActive;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ConsoleApp {
	class MainClass {
		public static void Main(string[] args) {
			SQLiteEngine loDBEngine = new SQLiteEngine("DB.sqlite", String.Format("D:{0}Projects{0}SQLiteActive", System.IO.Path.DirectorySeparatorChar));
//			loDBEngine.dropTable<DBModel>();
//			loDBEngine.createTable<DBModel>();

//			var testModel = new DBModel();
//			testModel.ModelID = Guid.NewGuid();
//			testModel.DateOfBirth = DateTime.Now;
//			testModel.Email = "my@example.com";
//			testModel.Firstname = "John";
//			testModel.Lastname = "Doe";
//			testModel.IsRetarded = true;
//			testModel.AccountStatus = Status.Terminated;

			Console.WriteLine("Start.");

//			var liInsertID = loDBEngine.insert(testModel);
//			Guid mojGuid = new Guid("0cd4f1d6-f8d6-4b20-86b3-9521e1bad2a6");
			var dbModel = loDBEngine.selectAll<DBModel>().FirstOrDefault();
//			dbModel.Firstname += " Updated Second Time!";
			dbModel.Lastname = null;
//			dbModel.SomeNumber = 22;
			var liUpdateUD = loDBEngine.update(dbModel);
//			var loSelectedList = loDBEngine.selectAll<DBModel>().Where(x => x.Firstname == "Ben");
//			foreach (DBModel toItem in loSelectedList) {
//				Console.WriteLine(toItem.ID + " " + toItem.Lastname);
//			}
//			loDBEngine.delete(dbModel);
//			for (int j = 0; j < 10; j++) {
//				Thread toThread = new Thread(() => initProcess(loDBEngine, j));
//				toThread.Start();
//			}

//			var loResult = loDBEngine.selectWhere<DBModel>(x => x.ModelID == 1).FirstOrDefault();
//			loResult.Firstname += "UPDATED";
//			loResult.AccountStatus = Status.Terminated;
//			var liUpdateUD = loDBEngine.update(loResult);
//			loDBEngine.selectSingle<DBModel>(x => x.ID == 1);
//			Console.WriteLine(String.Format("Inserted new row with id: {0}", liInsertID));
//			Console.WriteLine(String.Format("Affected rows: {0}", liUpdateUD));
			Console.WriteLine("End.");
		}

		private static void initProcess(SQLiteEngine aoEngine, int aiThreadID) {
			Console.WriteLine("Thread #"+aiThreadID+" filling model list...");
			var loList = fillList();
			Console.WriteLine("Thread #"+aiThreadID+" inserting into database...");
			aoEngine.insertMany(loList);
			Console.WriteLine("Thread #"+aiThreadID+" done.");
		}

		private static List<DBModel> fillList(){
			List<DBModel> loModelList = new List<DBModel>();
			for (int i = 1; i <= 10000000; i++) {
				DBModel toModel = new DBModel();
				toModel.DateOfBirth = DateTime.Now;
				toModel.Email = String.Format("blah{0}@example.com", i);
				toModel.Firstname = (i % 2 == 0) ? "Mike" : "Dick" ;
				toModel.Lastname = (i % 3 == 0) ? "Hunt" : "Litoris";
				toModel.ModelID = Guid.NewGuid();
				loModelList.Add(toModel);
			}
			return loModelList;
		}
	}

	[Table ("MultipleTest")]
	public class DBModel {

		[PrimaryKey]
		public Guid ModelID { get; set; }
		
		public string Firstname { get; set; }

		[NotNull]
		public string Lastname { get; set; }
		
		public string Email { get; set; }

		public int SomeNumber { get; set; }
		
		public Boolean IsRetarded { get; set; }

		public DateTime DateOfBirth { get; set; }

		public Status AccountStatus { get; set; }
	}

	public enum Status {
		Active = 1,
		Terminated = 0
	}
}
