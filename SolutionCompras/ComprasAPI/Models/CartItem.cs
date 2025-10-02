namespace ComprasAPI.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }   // FK a Producto
        public int Quantity { get; set; }

        // Propiedad para incluir el producto completo
        public Product Producto { get; set; }
    }
}
