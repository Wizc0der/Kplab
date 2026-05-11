using System.IO;
using System.IO.MemoryMappedFiles;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

namespace Task_1.Services
{
    public class CommunicationService
    {
        private const string PipeName = "HotelChatPipe";
        private const string MmfName = "HotelNotifications";
        private NamedPipeServerStream _server;
        private bool _isRunning;

        public event System.Action<string> MessageReceived;
        public event System.Action<string> NotificationReceived;

        public async Task StartServerAsync()
        {
            _isRunning = true;
            _server = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Message);

            await Task.Run(() =>
            {
                while (_isRunning)
                {
                    _server.WaitForConnection();
                    var buffer = new byte[1024];
                    int len = _server.Read(buffer, 0, buffer.Length);
                    string msg = Encoding.UTF8.GetString(buffer, 0, len);
                    MessageReceived?.Invoke(msg);
                    _server.Disconnect();
                }
            });
        }

        public void SendNotification(string text)
        {
            using var mmf = MemoryMappedFile.CreateOrOpen(MmfName, 1024);
            using var stream = mmf.CreateViewStream();
            var bytes = Encoding.UTF8.GetBytes(text);
            stream.Write(bytes, 0, bytes.Length);
            NotificationReceived?.Invoke(text);
        }

        public void Stop() => _isRunning = false;
    }
}