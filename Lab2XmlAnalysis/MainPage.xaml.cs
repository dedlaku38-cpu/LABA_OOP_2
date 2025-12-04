using Lab2XmlAnalysis.ViewModels;
using Microsoft.Maui.Controls; // Основное пространство имен для MAUI

namespace Lab2XmlAnalysis
{
    public partial class MainPage : ContentPage
    {
        private readonly MainViewModel _viewModel;

        public MainPage()
        {
            InitializeComponent();

            // Инициализация ViewModel и привязка контекста
            _viewModel = new MainViewModel();
            BindingContext = _viewModel;
        }

        // =========================================================
        // 1. ЗМІНА РОЗМІРУ (ЗАВАНТАЖЕННЯ / ПОВОРОТ ЕКРАНУ)
        // =========================================================
        private void OnContentSizeChanged(object sender, EventArgs e)
        {
            // Виконуємо в Dispatcher, щоб UI встиг оновитися перед перевіркою розмірів
            Dispatcher.Dispatch(() =>
            {
                // Перевіряємо обидва списки
                CheckScrollButtonVisibility(ParamsScrollView, ParamsScrollUpButton, ParamsScrollDownButton);
                CheckScrollButtonVisibility(ResultsScrollView, ResultsScrollUpButton, ResultsScrollDownButton);
            });
        }

        // =========================================================
        // 2. ПАРАМЕТРИ (ЛІВА ЧАСТИНА)
        // =========================================================
        private void OnParamsScrolled(object sender, ScrolledEventArgs e)
        {
            CheckScrollButtonVisibility(sender as ScrollView, ParamsScrollUpButton, ParamsScrollDownButton);
        }

        private async void OnParamsScrollUp(object sender, EventArgs e)
        {
            if (ParamsScrollView != null)
                await ParamsScrollView.ScrollToAsync(0, 0, true);
        }

        private async void OnParamsScrollDown(object sender, EventArgs e)
        {
            if (ParamsScrollView != null)
                await ParamsScrollView.ScrollToAsync(0, ParamsScrollView.ContentSize.Height, true);
        }

        // =========================================================
        // 3. РЕЗУЛЬТАТИ (ПРАВА ЧАСТИНА)
        // =========================================================
        private void OnResultsScrolled(object sender, ScrolledEventArgs e)
        {
            CheckScrollButtonVisibility(sender as ScrollView, ResultsScrollUpButton, ResultsScrollDownButton);
        }

        private async void OnResultsScrollUp(object sender, EventArgs e)
        {
            if (ResultsScrollView != null)
                await ResultsScrollView.ScrollToAsync(0, 0, true);
        }

        private async void OnResultsScrollDown(object sender, EventArgs e)
        {
            if (ResultsScrollView != null)
                await ResultsScrollView.ScrollToAsync(0, ResultsScrollView.ContentSize.Height, true);
        }

        // =========================================================
        // УНІВЕРСАЛЬНА ЛОГІКА ВИДИМОСТІ КНОПОК
        // =========================================================
        private void CheckScrollButtonVisibility(ScrollView scrollView, Button upButton, Button downButton)
        {
            // Захист від null (якщо елементи ще не ініціалізовані)
            if (scrollView == null || upButton == null || downButton == null) return;

            double contentHeight = scrollView.ContentSize.Height;
            double viewportHeight = scrollView.Height;
            double scrollY = scrollView.ScrollY;

            // Якщо контенту мало і він вміщується на екрані -> ховаємо обидві кнопки
            // Додаємо невеликий буфер (+10), щоб уникнути мерехтіння при граничних значеннях
            if (viewportHeight <= 0 || contentHeight <= viewportHeight + 10)
            {
                if (upButton.IsVisible) upButton.IsVisible = false;
                if (downButton.IsVisible) downButton.IsVisible = false;
                return;
            }

            // Логіка кнопки "Вгору"
            // Якщо ми майже зверху (scrollY <= 10), ховаємо кнопку
            bool isAtTop = scrollY <= 10;
            if (upButton.IsVisible != !isAtTop)
                upButton.IsVisible = !isAtTop;

            // Логіка кнопки "Вниз"
            // Якщо ми майже знизу, ховаємо кнопку
            bool isAtBottom = scrollY >= (contentHeight - viewportHeight - 10);
            if (downButton.IsVisible != !isAtBottom)
                downButton.IsVisible = !isAtBottom;
        }
    }
}