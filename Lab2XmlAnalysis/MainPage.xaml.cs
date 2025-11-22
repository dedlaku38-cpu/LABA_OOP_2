using Lab2XmlAnalysis.ViewModels;
using Microsoft.UI.Xaml;
using Windows.Graphics;

namespace Lab2XmlAnalysis
{
    public partial class MainPage : ContentPage
    {
        private readonly MainViewModel _viewModel;

        public MainPage()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            BindingContext = _viewModel;
        }

        // --- 1. ЗМІНА РОЗМІРУ (ЗАВАНТАЖЕННЯ) ---
        private void OnContentSizeChanged(object sender, EventArgs e)
        {
            Dispatcher.Dispatch(() =>
            {
                // Перевіряємо обидва списки (обидва тепер ScrollView)
                CheckScrollButtonVisibility(ParamsScrollView, ParamsScrollUpButton, ParamsScrollDownButton);
                CheckScrollButtonVisibility(ResultsScrollView, ResultsScrollUpButton, ResultsScrollDownButton);
            });
        }

        // =========================================================
        // 2. ПАРАМЕТРИ
        // =========================================================
        private void OnParamsScrolled(object sender, ScrolledEventArgs e)
        {
            CheckScrollButtonVisibility(sender as ScrollView, ParamsScrollUpButton, ParamsScrollDownButton);
        }

        private async void OnParamsScrollUp(object sender, EventArgs e)
        {
            await ParamsScrollView.ScrollToAsync(0, 0, true);
        }

        private async void OnParamsScrollDown(object sender, EventArgs e)
        {
            await ParamsScrollView.ScrollToAsync(0, ParamsScrollView.ContentSize.Height, true);
        }

        // =========================================================
        // 3. РЕЗУЛЬТАТИ (ПОВЕРНУТО ScrollView)
        // =========================================================
        private void OnResultsScrolled(object sender, ScrolledEventArgs e)
        {
            CheckScrollButtonVisibility(sender as ScrollView, ResultsScrollUpButton, ResultsScrollDownButton);
        }

        private async void OnResultsScrollUp(object sender, EventArgs e)
        {
            await ResultsScrollView.ScrollToAsync(0, 0, true);
        }

        private async void OnResultsScrollDown(object sender, EventArgs e)
        {
            await ResultsScrollView.ScrollToAsync(0, ResultsScrollView.ContentSize.Height, true);
        }

        // =========================================================
        // УНІВЕРСАЛЬНА ЛОГІКА ВИДИМОСТІ
        // =========================================================
        private void CheckScrollButtonVisibility(ScrollView scrollView, Button upButton, Button downButton)
        {
            if (scrollView == null) return;

            double contentHeight = scrollView.ContentSize.Height;
            double viewportHeight = scrollView.Height;
            double scrollY = scrollView.ScrollY;

            // Якщо контенту мало -> ховаємо кнопки
            if (viewportHeight <= 0 || contentHeight <= viewportHeight + 10)
            {
                if (upButton.IsVisible) upButton.IsVisible = false;
                if (downButton.IsVisible) downButton.IsVisible = false;
                return;
            }

            // Вгору? (Ми зверху?)
            bool isAtTop = scrollY <= 10;
            upButton.IsVisible = !isAtTop;

            // Вниз? (Ми знизу?)
            bool isAtBottom = scrollY >= (contentHeight - viewportHeight - 10);
            downButton.IsVisible = !isAtBottom;
        }
    }
}