using Common;
using Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Services {
	public class InvoiceService {
		public List<Invoice> getAll() {
			var result = new List<Invoice>();

			using (var context = new SqlConnection(Parameters.connectionString)) {
				//abre la conexion
				context.Open();

				var command = new SqlCommand("SELECT * FROM INVOICES", context);
				
				using (var reader = command.ExecuteReader()) {
					while (reader.Read()) {
						var invoice = new Invoice {
							id = Convert.ToInt32(reader["id"])
							, iva = Convert.ToDecimal(reader["iva"])
							, subTotal = Convert.ToDecimal(reader["subtotal"])
							, total = Convert.ToDecimal(reader["total"])
							, clientId = Convert.ToInt32(reader["clientId"])
						};

						result.Add(invoice);
					}
				}

				//set aditional properties
				foreach (var invoice in result) {
					//client
					setClient(invoice, context);

					//detail
					setDetail(invoice, context);
				}
			}

			return result;
		}

		public Invoice get(int id) {
			var result = new Invoice();

			using(var context = new SqlConnection(Parameters.connectionString)) {
				context.Open();

				var command = new SqlCommand("SELECT * FROM INVOICES WHERE ID = @ID", context);
				command.Parameters.AddWithValue("@ID", id);

				using (var reader = command.ExecuteReader()) {
					reader.Read();

					result.id = Convert.ToInt32(reader["ID"]);
					result.iva = Convert.ToDecimal(reader["IVA"]);
					result.subTotal = Convert.ToDecimal(reader["SUBTOTAL"]);
					result.total = Convert.ToDecimal(reader["TOTAL"]);
					result.clientId = Convert.ToInt32(reader["CLIENTID"]);
				}

				//Client
				setClient(result, context);

				//Detail
				setDetail(result, context);
			}

			return result;
		}

		private void setClient(Invoice invoice, SqlConnection context) {
			var command = new SqlCommand(
				"SELECT * FROM CLIENTS WHERE ID = @CLIENTID", context
			);
			command.Parameters.AddWithValue("@CLIENTID", invoice.clientId);

			using (var reader = command.ExecuteReader()) {
				reader.Read();

				invoice.client = new Client {
					id = Convert.ToInt32(reader["ID"])
					, name = reader["NAME"].ToString()
				};
			}
		}

		private void setDetail(Invoice invoice, SqlConnection context) {
			var command = new SqlCommand(
				"SELECT * FROM INVOICEDETAIL WHERE INVOICEID = @INVOICEID", context
			);
			command.Parameters.AddWithValue("@INVOICEID", invoice.id);

			using (var reader = command.ExecuteReader()) {
				while (reader.Read()) {
					invoice.detail.Add(new InvoiceDetail {
						id = Convert.ToInt32(reader["ID"])
						, productId = Convert.ToInt32(reader["PRODUCTID"])
						, quantity = Convert.ToInt32(reader["QUANTITY"])
						, iva = Convert.ToDecimal(reader["IVA"])
						, subTotal = Convert.ToDecimal(reader["SUBTOTAL"])
						, total = Convert.ToDecimal(reader["TOTAL"])
						, invoice = invoice
					});
				}
			}

			foreach (var detail in invoice.detail) {
				//product
				setProduct(detail,context);
			}
		}

		private void setProduct(InvoiceDetail detail, SqlConnection context) {
			var command = new SqlCommand(
				"SELECT * FROM PRODUCTS WHERE ID = @PRODUCTID", context
			);
			command.Parameters.AddWithValue("@PRODUCTID", detail.productId);

			using (var reader = command.ExecuteReader()) {
				reader.Read();

				detail.product = new Product {
					id = Convert.ToInt32(reader["ID"])
					, price = Convert.ToDecimal(reader["PRICE"])
					, name = reader["NAME"].ToString()
				};
			}
		}
	}
}
