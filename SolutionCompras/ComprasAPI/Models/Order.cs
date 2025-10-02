namespace ComprasAPI.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; }
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public float Total { get; set; }

    }
}
