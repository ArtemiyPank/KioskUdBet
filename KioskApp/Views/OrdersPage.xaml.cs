using KioskApp.ViewModels;
using KioskApp.Services;
using Microsoft.Maui.Controls;

namespace KioskApp.Views
{
    public partial class OrdersPage : ContentPage
    {
        public OrdersPage(IOrderApiService orderApiService)
        {
            InitializeComponent();
            BindingContext = new OrdersViewModel(orderApiService);
        }
    }
}
