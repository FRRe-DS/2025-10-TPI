using System.Collections.Generic;
using System.Linq;

namespace ComprasAPI.Models
{
    public class Cart
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        // Propiedad calculada para el total
        public decimal Total
        {
            get
            {
                return Items.Sum(item => (item.Producto?.Precio ?? 0) * item.Quantity);
            }
        }
    }
}
