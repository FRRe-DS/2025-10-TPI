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
}