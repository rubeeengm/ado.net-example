using Common;
using System;
using System.Data.SqlClient;

namespace Services {
	public class TestService {
		public static void testConnection() {
			try {
				var orderService = new InvoiceService();
				var result = orderService.getAll();
			} catch(Exception ex) {
				Console.WriteLine($"Sql Server:{ex.Message}");
			}
		}
	}
}
