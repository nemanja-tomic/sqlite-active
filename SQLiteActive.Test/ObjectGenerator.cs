using System;
using System.Collections.Generic;

namespace SQLiteActive.Test {
	public static class ObjectGenerator {
		private static Random iiRandom = new Random();

		public static User GenerateUser(){
			User loUser = new User();
			loUser.DateOfBirth = DateTime.Now;
			loUser.Email = String.Format("fancyemail{0}@domain.com", iiRandom.Next(10, 99));
			loUser.Firstname = String.Format("mynameis{0}", iiRandom.Next());
			loUser.Lastname = String.Format("mylastnameis{0}", iiRandom.Next());
			loUser.IsAdmin = Convert.ToBoolean(iiRandom.Next(0, 1));
			return loUser;
		}

		public static List<User> GenerateUsers(int aiListCapacity) {
			var loReturnList = new List<User>();
			for (int i = 1; i <= aiListCapacity; i++) {
				loReturnList.Add(GenerateUser());
			}
			return loReturnList;
		}
	}
}

