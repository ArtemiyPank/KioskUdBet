using KioskApp.ViewModels;
using Microsoft.Maui.Controls;
using Syncfusion.Maui.Sliders;
using System;
using System.Diagnostics;

namespace KioskApp.Views
{
    public partial class CartPage : ContentPage
    {
        public CartPage(CartViewModel viewModel)
        {
            InitializeComponent();

            if (timeRangeSlider != null)
            {
                timeRangeSlider.StepDuration = new SliderStepDuration(minutes: 15);
            }
            else
            {
                Debug.WriteLine("timeRangeSlider is null.");
            } 

            BindingContext = viewModel;
        }

        private void OnRangeSliderValueChanging(object sender, DateTimeRangeSliderValueChangingEventArgs e)
        {
            var rangeSlider = sender as SfDateTimeRangeSlider;

            Debug.WriteLine("In OnRangeSliderValueChanging");

            // Используем свойства RangeStart и RangeEnd
            var newRangeStart = e.NewRangeStart;
            var newRangeEnd = e.NewRangeEnd;

            // Проверка, что диапазон не меньше 1 часа
            if ((newRangeEnd - newRangeStart) < TimeSpan.FromHours(1))
            {
                // Если меньше, предотвращаем изменение и сбрасываем значение
                e.Cancel = true;
            }
        }
    }
}
