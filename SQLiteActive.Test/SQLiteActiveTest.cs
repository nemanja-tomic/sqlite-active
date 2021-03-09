using NUnit.Framework;
using System;
using System.Diagnostics;
using SQLiteActive;
using System.Linq;

namespace SQLiteActive.Test {

	[TestFixture()]
	public class SQLiteActiveTest {

		private SQLiteEngine ioDatabaseEngine;

		[TestFixtureSetUp()]
		public void init(){
			ioDatabaseEngine = new SQLiteEngine("TestDB.sqlite", String.Format("D:{0}Projects{0}SQLiteActive", System.IO.Path.DirectorySeparatorChar));
		}

		[Test()]
		public void CreateTable() {
			try {
				ioDatabaseEngine.createTable<User>();
				Assert.IsTrue(true);
			} catch (Exception aoException) {
				Debug.WriteLine(aoException.Message);
				Assert.IsTrue(false);
			}
		}

		[Test()]
		public void Insert() {
			try {
				ioDatabaseEngine.insert(ObjectGenerator.GenerateUser());
				Assert.IsTrue(true);
			} catch (Exception aoException) {
				Debug.WriteLine(aoException.Message);
				Assert.IsTrue(false);
			}
		}

		[Test()]
		public void InsertMany() {
			try {
				ioDatabaseEngine.insertMany(ObjectGenerator.GenerateUsers(20));
				Assert.IsTrue(true);
			} catch (Exception aoException) {
				Debug.WriteLine(aoException.Message);
				Assert.IsTrue(false);
			}
		}

		[Test()]
		public void Update() {
			try {
				var loUser = ioDatabaseEngine.selectWhere<User>(x => x.UserID == 1).FirstOrDefault();
				loUser.Firstname += "UPDATED!";
				loUser.Lastname += "UPDATED!";
				ioDatabaseEngine.update(loUser);
				Assert.IsTrue(true);
			} catch (Exception aoException) {
				Debug.WriteLine(aoException.Message);
				Assert.IsTrue(false);
			}
		}

		[Test()]
		public void SelectAll() {
			try {
				ioDatabaseEngine.selectAll<User>();
				Assert.IsTrue(true);
			} catch (Exception aoException) {
				Debug.WriteLine(aoException.Message);
				Assert.IsTrue(false);
			}
		}

		[Test()]
		public void SelectCondition() {
			try {
				ioDatabaseEngine.selectWhere<User>(x => x.UserID > 3 && x.UserID < 20);
				Assert.IsTrue(true);
			} catch (Exception aoException) {
				Debug.WriteLine(aoException.Message);
				Assert.IsTrue(false);
			}
		}
	}
}

