using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Task_1.Core;
using Task_1.Models;
using Task_1.Repositories;
using Task_1.Services;

namespace Task_1.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly RoomRepository _roomRepo;
        private readonly BookingRepository _bookingRepo;
        private readonly BookingService _bookingService = new();
        private readonly CommunicationService _commService = new();

        public ObservableCollection<Room> Rooms { get; } = new();
        public ObservableCollection<Booking> Bookings { get; } = new();

        // Форма бронирования (TwoWay binding)
        private string _guestName;
        public string GuestName { get => _guestName; set { _guestName = value; OnPropertyChanged(); } }

        private DateTime _checkIn = DateTime.Today;
        public DateTime CheckIn { get => _checkIn; set { _checkIn = value; OnPropertyChanged(); } }

        private DateTime _checkOut = DateTime.Today.AddDays(1);
        public DateTime CheckOut { get => _checkOut; set { _checkOut = value; OnPropertyChanged(); } }

        private Room _selectedRoom;
        public Room SelectedRoom
        {
            get => _selectedRoom;
            set { _selectedRoom = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanBook)); }
        }

        private Booking _selectedBooking;
        public Booking SelectedBooking
        {
            get => _selectedBooking;
            set
            {
                if (_selectedBooking != value)
                {
                    _selectedBooking = value;
                    OnPropertyChanged();
                    // 🔑 Ключевой момент: уведомляем, что доступность команд изменилась!
                    OnPropertyChanged(nameof(CanEdit));
                    OnPropertyChanged(nameof(CanCancel));
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public bool CanEdit => SelectedBooking != null && !IsBusy;
        public bool CanCancel => SelectedBooking != null && !IsBusy;
        public bool CanBook => SelectedRoom?.Status == RoomStatus.Free && !string.IsNullOrWhiteSpace(GuestName) && !IsBusy;

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }
        public string StatusMessage { get; private set; } = "Готово";

        // CRUD команды
        public AsyncRelayCommand LoadCommand { get; }
        public AsyncRelayCommand BookCommand { get; }
        public AsyncRelayCommand EditCommand { get; }
        public AsyncRelayCommand CancelCommand { get; }
        public AsyncRelayCommand DeleteRoomCommand { get; }
        public AsyncRelayCommand DeleteBookingCommand { get; }

        public MainViewModel()
        {
            var roomsPath = "rooms_data.json";
            var bookingsPath = "bookings_data.json";

            _roomRepo = new RoomRepository(roomsPath);
            _bookingRepo = new BookingRepository(bookingsPath);

            // 🔑 Создаём команды с правильными CanExecute
            LoadCommand = new AsyncRelayCommand(LoadDataAsync);
            BookCommand = new AsyncRelayCommand(BookRoomAsync, () => CanBook);
            EditCommand = new AsyncRelayCommand(EditBookingAsync, () => CanEdit);
            CancelCommand = new AsyncRelayCommand(CancelBookingAsync, () => CanCancel);
            DeleteRoomCommand = new AsyncRelayCommand(DeleteRoomAsync, () => SelectedRoom != null && !IsBusy);
            DeleteBookingCommand = new AsyncRelayCommand(DeleteBookingAsync, () => SelectedBooking != null && !IsBusy);

            // Автозагрузка
            LoadCommand.ExecuteAsync(null);

            // 🔑 Подписка на изменения, которые могут влиять на CanExecute
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(IsBusy) ||
                    e.PropertyName == nameof(SelectedBooking) ||
                    e.PropertyName == nameof(SelectedRoom) ||
                    e.PropertyName == nameof(GuestName))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            };

            _commService.StartServerAsync();
            _commService.MessageReceived += msg =>
                Application.Current.Dispatcher.Invoke(() => StatusMessage = $"💬 {msg}");
        }

        private async Task LoadDataAsync()
        {
            IsBusy = true;
            StatusMessage = "Загрузка данных...";

            var rooms = await _roomRepo.GetAllAsync();
            Rooms.Clear();
            foreach (var r in rooms) Rooms.Add(r);

            var bookings = await _bookingRepo.GetAllAsync();
            Bookings.Clear();
            foreach (var b in bookings) Bookings.Add(b);

            StatusMessage = $"Загружено: {Rooms.Count} номеров, {Bookings.Count} броней";
            IsBusy = false;
        }

        private async Task BookRoomAsync()
        {
            if (!ValidateBooking()) return;

            IsBusy = true;
            StatusMessage = "Обработка...";

            // Асинхронное подтверждение (День 4)
            var success = await _bookingService.ProcessBookingAsync(
                new Booking { GuestName = GuestName, CheckIn = CheckIn, CheckOut = CheckOut },
                SelectedRoom);

            if (success)
            {
                var booking = new Booking
                {
                    GuestName = GuestName,
                    RoomId = SelectedRoom.Id,
                    CheckIn = CheckIn,
                    CheckOut = CheckOut,
                    Status = "Active"
                };

                await _bookingRepo.AddAsync(booking);
                SelectedRoom.Status = RoomStatus.Booked;
                await _roomRepo.UpdateAsync(SelectedRoom);

                _commService.SendNotification($"🎉 Новая бронь: {GuestName} → №{SelectedRoom.Number}");
                StatusMessage = "✅ Бронирование успешно!";

                await LoadCommand.ExecuteAsync(null); // Refresh UI
            }
            IsBusy = false;
        }

        private async Task EditBookingAsync()
        {
            if (SelectedBooking == null)
            {
                MessageBox.Show("Выберите бронь для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Заполняем форму данными выбранной брони
            GuestName = SelectedBooking.GuestName;
            CheckIn = SelectedBooking.CheckIn;
            CheckOut = SelectedBooking.CheckOut;

            // Находим соответствующую комнату
            SelectedRoom = Rooms.FirstOrDefault(r => r.Id == SelectedBooking.RoomId);

            StatusMessage = $"✏️ Редактирование брони #{SelectedBooking.Id}. Измените данные и нажмите 'Забронировать'";

            // Показываем уведомление
            MessageBox.Show(
                $"Данные загружены в форму.\n\nГость: {GuestName}\nЗаезд: {CheckIn:dd.MM.yyyy}\nВыезд: {CheckOut:dd.MM.yyyy}\n\nИзмените значения и нажмите 'Забронировать' для сохранения.",
                "Редактирование",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private async Task CancelBookingAsync()
        {
            if (SelectedBooking == null)
            {
                MessageBox.Show("Выберите бронь для отмены", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Отменить бронирование #{SelectedBooking.Id}?\n\nГость: {SelectedBooking.GuestName}\nКомната: №{SelectedBooking.RoomId}",
                "Подтверждение отмены",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                IsBusy = true;
                StatusMessage = "Отмена бронирования...";

                try
                {
                    // Находим комнату и освобождаем её
                    var room = Rooms.FirstOrDefault(r => r.Id == SelectedBooking.RoomId);
                    if (room != null)
                    {
                        room.Status = RoomStatus.Free;
                        await _roomRepo.UpdateAsync(room);
                    }

                    // Удаляем бронь
                    await _bookingRepo.DeleteAsync(SelectedBooking);

                    // Обновляем список
                    await LoadCommand.ExecuteAsync(null);

                    _commService.SendNotification($"❌ Бронь #{SelectedBooking.Id} отменена");
                    StatusMessage = "✅ Бронирование отменено";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при отмене: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        private async Task DeleteRoomAsync()
        {
            if (SelectedRoom == null) return;
            if (SelectedRoom.Status == RoomStatus.Booked)
            {
                MessageBox.Show("Нельзя удалить забронированный номер!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Удалить комнату №{SelectedRoom.Number}?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                await _roomRepo.DeleteAsync(SelectedRoom);
                await LoadCommand.ExecuteAsync(null);
                StatusMessage = "🗑️ Номер удалён";
            }
        }

        private async Task DeleteBookingAsync()
        {
            if (SelectedBooking == null) return;

            var result = MessageBox.Show("Удалить запись о брони?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await _bookingRepo.DeleteAsync(SelectedBooking);
                await LoadCommand.ExecuteAsync(null);
                StatusMessage = "🗑️ Запись удалена";
            }
        }

        private bool ValidateBooking()
        {
            if (string.IsNullOrWhiteSpace(GuestName))
            {
                MessageBox.Show("Введите ФИО гостя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (CheckOut <= CheckIn)
            {
                MessageBox.Show("Дата выезда должна быть позже даты заезда", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (SelectedRoom?.Status != RoomStatus.Free)
            {
                MessageBox.Show("Номер уже забронирован", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }
    }
}