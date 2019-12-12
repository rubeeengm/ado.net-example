namespace Models {
	public class InvoiceDetail {
		public int id { get; set; }
		public int productId { get; set; }
		public Product product { get; set; }
		public int invoiceId { get; set; }
		public Invoice invoice { get; set; }
		public int quantity { get; set; }
		public decimal price { get; set; }
		public decimal iva { get; set; }
		public decimal subTotal { get; set; }
		public decimal total { get; set; }
	}
}
