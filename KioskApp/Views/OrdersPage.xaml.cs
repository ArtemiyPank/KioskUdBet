using KioskApp.ViewModels;
using Microsoft.Maui.Controls;

namespace KioskApp.Views
{
    public partial class OrdersPage : ContentPage
    {
        public OrdersPage()
        {
            InitializeComponent();
            BindingContext = new OrdersViewModel();
        }
    }
}
