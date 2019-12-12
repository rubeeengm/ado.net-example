namespace Models {
	public class Invoice {
		public int id { get; set; }
		public int clientId { get; set; }
		public Client client { get; set; }
		public decimal iva { get; set; }
		public decimal subTotal { get; set; }
		public decimal total { get; set; }
	}
}
