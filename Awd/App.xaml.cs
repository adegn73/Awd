using System.Configuration;
using System.Data;
using System.Windows;

namespace Awd
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        async void App_Startup(object sender, StartupEventArgs e)
        {

            var fileName = e.Args.FirstOrDefault();

            var viewModel = new MainViewModel(fileName);
            
            var mainWindow = new MainWindow()
            {
                DataContext = viewModel
            };
            mainWindow.Show();
        }
    }

}
