using Common;
using Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Services {
	public class TestService {
		public static void testConnection() {
			try {
				var orderService = new InvoiceService();
				//var result = orderService.get(1);
				var invoice = new Invoice {
					clientId = 1
					, detail = new List<InvoiceDetail> {
						new InvoiceDetail {
							productId = 1
							, quantity = 5
							, price = 1500
						}
						, new InvoiceDetail {
							productId = 8
							, quantity = 15
							, price = 125
						}
					}
				};

				orderService.create(invoice);
			} catch(Exception ex) {
				Console.WriteLine($"Sql Server:{ex.Message}");
			}
		}
	}
}
