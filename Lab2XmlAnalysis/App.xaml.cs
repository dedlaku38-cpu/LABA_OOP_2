// File: App.xaml.cs
using Lab2XmlAnalysis.ViewModels;

namespace Lab2XmlAnalysis
{
    public partial class App : Application
    {
        // Убеждаемся, что конструктор принимает MainPage
        public App(MainPage mainPage)
        {
            InitializeComponent();

            MainPage = mainPage;

            UserAppTheme = AppTheme.Light;
        }
    }
}