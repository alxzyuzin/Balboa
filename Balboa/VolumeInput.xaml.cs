using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Balboa
{
    public sealed partial class VolumeInput : UserControl
    {
        public VolumeInput()
        {
            this.InitializeComponent();
        }

        private bool _volumeChangedByStatus = true;
        private void sl_Volume_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            var sl = sender as Slider;
            //if (!_volumeChangedByStatus && _status.State == "play")
            //    _server.SetVolume((int)sl.Value);
            //_volumeChangedByStatus = false;
        }

        private void sl_Volume_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var sl = sender as Slider;
            var cp = e.GetCurrentPoint((UIElement)sender) as PointerPoint;
            int mouseweeldelta = cp.Properties.MouseWheelDelta / 12;

            int newvalue = (int)sl.Value + mouseweeldelta;
            if ((newvalue >= 0) && (newvalue <= 100) && Math.Abs(mouseweeldelta) > 1)
            {
                //   _server.SetVolume(newvalue);
            }
        }
    } //class VolumeInput
}
