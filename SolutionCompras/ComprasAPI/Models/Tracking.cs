namespace ComprasAPI.Models
{
    public class Tracking
    {
        public int Id { get; set; }
        public string Estado { get; set; }
        public int PedidoId { get; set; }
        public DateTime Fecha { get; set; }
    }
}
