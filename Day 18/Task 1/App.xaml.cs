using System.Windows;

namespace Task_1
{
    public partial class App : Application
    {
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            // Очистка ресурсов при необходимости
        }
    }
}