using Common;
using Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;

namespace Services {
	public class InvoiceService {

		public void create(Invoice model) {
			prepareOrder(model);

			using (var transaction = new TransactionScope()) {
				using (var context = new SqlConnection(Parameters.connectionString)) {
					context.Open();

					//header
					addHeader(model, context);

					//detail
					addDetail(model, context);
				}

				transaction.Complete();
			}
		}

		public Invoice get(int id) {
			var result = new Invoice();

			using (var context = new SqlConnection(Parameters.connectionString)) {
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

		public void update(Invoice model) {
			prepareOrder(model);
			
			using (var transaction = new TransactionScope()) {
				using (var context = new SqlConnection(
					Parameters.connectionString)
				) {
					context.Open();

					//header
					updateHeader(model, context);

					//remove detail
					removeDetail(model.id, context);

					addDetail(model, context);
				}

				transaction.Complete();
			}
		}

		public void delete(int id) {
			using (var context = new SqlConnection(Parameters.connectionString)) {
				context.Open();

				var command = new SqlCommand(
					"DELETE FROM INVOICES WHERE ID = @ID", context
				);
				
				command.Parameters.AddWithValue("@ID", id);
				command.ExecuteNonQuery();

				//elimina el detalle
				removeDetail(id, context);
			}
		}

		private void addHeader(Invoice model, SqlConnection context) {
			var query = "INSERT INTO INVOICES(CLIENTID, IVA, SUBTOTAL,TOTAL) OUTPUT INSERTED.ID " +
						"VALUES(@CLIENTID, @IVA, @SUBTOTAL, @TOTAL)";
			var command = new SqlCommand(query, context);

			command.Parameters.AddWithValue("@CLIENTID", model.clientId);
			command.Parameters.AddWithValue("@IVA", model.iva);
			command.Parameters.AddWithValue("@SUBTOTAL", model.subTotal);
			command.Parameters.AddWithValue("@TOTAL", model.total);

			model.id = Convert.ToInt32(command.ExecuteScalar());
		}

		private void updateHeader(Invoice model, SqlConnection context) {
			var query = "UPDATE INVOICES SET CLIENTID = @CLIENTID, IVA = @IVA, SUBTOTAL = @SUBTOTAL, TOTAL = @TOTAL " +
				"WHERE ID = @ID";
			var command = new SqlCommand(query, context);

			command.Parameters.AddWithValue("@CLIENTID", model.clientId);
			command.Parameters.AddWithValue("@IVA", model.iva);
			command.Parameters.AddWithValue("@SUBTOTAL", model.subTotal);
			command.Parameters.AddWithValue("@TOTAL", model.total);
			command.Parameters.AddWithValue("@ID", model.id);

			command.ExecuteNonQuery();
		}

		private void addDetail(Invoice model, SqlConnection context) {
			foreach (var detail in model.detail) {
				var query = "INSERT INTO INVOICEDETAIL(INVOICEID, PRODUCTID, QUANTITY, PRICE" +
					", IVA, SUBTOTAL, TOTAL) VALUES(@INVOICEID, @PRODUCTID, @QUANTITY, @PRICE" +
					", @IVA, @SUBTOTAL, @TOTAL)";

				var command = new SqlCommand(query, context);

				command.Parameters.AddWithValue("@INVOICEID", model.id);
				command.Parameters.AddWithValue("@PRODUCTID", detail.productId);
				command.Parameters.AddWithValue("@QUANTITY", detail.quantity);
				command.Parameters.AddWithValue("@PRICE", detail.price);
				command.Parameters.AddWithValue("@IVA", detail.iva);
				command.Parameters.AddWithValue("@SUBTOTAL", detail.subTotal);
				command.Parameters.AddWithValue("@TOTAL", detail.total);

				command.ExecuteNonQuery();
			}
		}

		private void removeDetail(int invoiceId, SqlConnection context) {
			var query = "DELETE FROM INVOICEDETAIL WHERE INVOICEID = @INVOICEID";
			var command = new SqlCommand(query, context);
			
			command.Parameters.AddWithValue("@INVOICEID", invoiceId);
			command.ExecuteNonQuery();
		}

		private void prepareOrder(Invoice model) {
			foreach (var detail in model.detail) {
				detail.total = detail.quantity * detail.price;
				detail.iva = detail.total * Parameters.ivaRate;
				detail.subTotal = detail.total - detail.iva;
			}

			model.total = model.detail.Sum(x => x.total);
			model.iva = model.detail.Sum(x => x.iva);
			model.subTotal = model.detail.Sum(x => x.subTotal);
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
