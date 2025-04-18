using KioskApp.ViewModels;
using Syncfusion.Maui.Sliders;
using System.Diagnostics;

namespace KioskApp.Views
{
    public partial class CartPage : ContentPage
    {
        // Constructor receives the ViewModel via dependency injection
        public CartPage(CartViewModel viewModel)
        {
            InitializeComponent();

            // Set the slider step duration to 15 minutes if available
            if (timeRangeSlider != null)
            {
                timeRangeSlider.StepDuration = new SliderStepDuration(minutes: 15);
            }
            else
            {
                Debug.WriteLine("Error: timeRangeSlider is null.");
            }

            // Bind the ViewModel
            BindingContext = viewModel;
        }

        // Prevents the user from selecting a time range shorter than 1 hour
        private void OnRangeSliderValueChanging(object sender, DateTimeRangeSliderValueChangingEventArgs e)
        {
            var slider = (SfDateTimeRangeSlider)sender;
            var start = e.NewRangeStart;
            var end = e.NewRangeEnd;

            // If the selected interval is under 1 hour, cancel the change
            if (end - start < TimeSpan.FromHours(1))
            {
                e.Cancel = true;
            }
        }
    }
}
