namespace ComprasAPI.Models.DTOs
{
    public class ProductoStock
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int StockDisponible { get; set; }
        public decimal PesoKg { get; set; }
        public Dimensiones Dimensiones { get; set; }
        public UbicacionAlmacen Ubicacion { get; set; }
        public List<Categoria> Categorias { get; set; }
    }

    public class Dimensiones
    {
        public decimal LargoCm { get; set; }
        public decimal AnchoCm { get; set; }
        public decimal AltoCm { get; set; }
    }

    public class UbicacionAlmacen
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
    }

    public class Categoria
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
    }

    // Request para crear reserva
    public class ReservaInput
    {
        public string IdCompra { get; set; }
        public int UsuarioId { get; set; }
        public List<ProductoReserva> Productos { get; set; }
    }

    public class ProductoReserva
    {
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
    }

    // Response de reserva
    public class ReservaOutput
    {
        public int IdReserva { get; set; }
        public string IdCompra { get; set; }
        public int UsuarioId { get; set; }
        public string Estado { get; set; }
        public string ExpiresAt { get; set; }
        public string FechaCreacion { get; set; }
    }

    // Detalle completo de reserva
    public class ReservaCompleta
    {
        public int IdReserva { get; set; }
        public string IdCompra { get; set; }
        public int UsuarioId { get; set; }
        public string Estado { get; set; }
        public string ExpiresAt { get; set; }
        public string FechaCreacion { get; set; }
        public List<ProductoReservaDetalle> Productos { get; set; }
    }

    public class ProductoReservaDetalle
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }


}