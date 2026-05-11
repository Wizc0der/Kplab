using System;

namespace Task_1.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public string GuestName { get; set; }
        public int RoomId { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public string Status { get; set; } = "Active";
    }
}