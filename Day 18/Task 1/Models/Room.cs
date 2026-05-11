using System.ComponentModel;

namespace Task_1.Models
{
    public enum RoomStatus { Free, Booked, Maintenance }

    public class Room : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public double PricePerNight { get; set; }

        private RoomStatus _status = RoomStatus.Free;
        public RoomStatus Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(nameof(Status)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}