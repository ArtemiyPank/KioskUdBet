using KioskApp.Services;
using KioskApp.ViewModels;

namespace KioskApp.Views
{
    // Code-behind for the OrdersPage XAML view
    public partial class OrdersPage : ContentPage
    {

        // Constructor initializes UI components; ViewModel is bound in XAML
        public OrdersPage(IOrderApiService orderApiService)
        {
            InitializeComponent();
            BindingContext = new OrdersViewModel(orderApiService);

        }
    }
}
