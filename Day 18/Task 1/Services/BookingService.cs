using System.Threading.Tasks;
using Task_1.Models;

namespace Task_1.Services
{
    public class BookingService
    {

        public async Task<bool> ProcessBookingAsync(Booking booking, Room room)
        {

            await Task.Delay(3000);
            room.Status = RoomStatus.Booked;
            return true;
        }

        public void CancelBooking(Booking booking, Room room)
        {
            room.Status = RoomStatus.Free;
            booking.Status = "Cancelled";
        }
    }
}