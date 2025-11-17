// Models/DTOs/LogisticaModels.cs
namespace ComprasAPI.Models.DTOs
{
    // Request para calcular costo de envío
    public class ShippingCostRequest
    {
        public Address DeliveryAddress { get; set; }
        public List<ProductRequest> Products { get; set; }
    }

    // Response de cálculo de costo
    public class ShippingCostResponse
    {
        public string Currency { get; set; }
        public decimal TotalCost { get; set; }
        public string TransportType { get; set; }
        public List<ProductCost> Products { get; set; }
    }

    // Request para crear envío
    public class CreateShippingRequest
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public Address DeliveryAddress { get; set; }
        public string TransportType { get; set; }
        public List<ProductRequest> Products { get; set; }
    }

    // Response de creación de envío
    public class CreateShippingResponse
    {
        public int ShippingId { get; set; }
        public string Status { get; set; }
        public string TransportType { get; set; }
        public string EstimatedDeliveryAt { get; set; }
    }

    // Detalle de seguimiento
    public class ShippingDetail
    {
        public int ShippingId { get; set; }
        public string Status { get; set; }
        public string EstimatedDeliveryAt { get; set; }
        public string TrackingNumber { get; set; }
        public string CarrierName { get; set; }
    }

    // Métodos de transporte
    public class TransportMethodsResponse
    {
        public List<TransportMethod> TransportMethods { get; set; }
    }

    public class TransportMethod
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string EstimatedDays { get; set; }
    }

    // Modelos comunes
    public class Address
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
    }

    public class ProductRequest
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
    }

    public class ProductCost
    {
        public int Id { get; set; }
        public decimal Cost { get; set; }
    }
}