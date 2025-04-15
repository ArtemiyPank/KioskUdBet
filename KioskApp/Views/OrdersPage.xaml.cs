using KioskApp.Services;
using KioskApp.ViewModels;
using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace KioskApp.Views
{
    public partial class OrdersPage : ContentPage
    {
        public OrdersPage(IOrderApiService orderApiService)
        {
            try
            {
                InitializeComponent();
                BindingContext = new OrdersViewModel(orderApiService);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OrdersPage constructor: {ex.Message}");
                // Handle error appropriately, perhaps show an alert
            }
        }
    }
}
