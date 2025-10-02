using System.Collections.Generic;
using System.Linq;

namespace ComprasAPI.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public string Estado { get; set; }

        public List<BookingRequest> Items { get; set; } = new List<BookingRequest>();
    }
}
