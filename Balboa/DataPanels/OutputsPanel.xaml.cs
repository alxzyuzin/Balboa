using Balboa.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Balboa
{
    public sealed partial class OutputsPanel : UserControl, IRequestAction, IDisposable
    {

        private Server _server;
        private List<Output> _outputs = new List<Output>();
        private ResourceLoader _resldr = new ResourceLoader();

        public event ActionRequestedEventHandler RequestAction;

        public OutputsPanel()
        {
            this.InitializeComponent();
        }

        public OutputsPanel(Server server):this()
        {
            _server = server;
            _server.DataReady += _server_DataReady;
            _server.Outputs();
        }


        public void Update()
        {
            _server.Outputs();
        }

        private void _server_DataReady(object sender, EventArgs e)
        {
            var mpdData = e as MpdResponse;
            if (mpdData.Keyword == ResponseKeyword.OK && mpdData.Command.Op == "outputs")
            {
                UpdateDataCollection(mpdData.Content);
                UpdateToggleSwitchList();
            }
        }

        public void UpdateDataCollection(List<string> serverData)
        {
            _outputs.Clear();
            while (serverData.Count > 0)
            {
                var output = new Output();
                output.Update(serverData);
                _outputs.Add(output);
            }
        }

        private void UpdateToggleSwitchList()
        {
            foreach (ToggleSwitch ts in Outputs.Children)
                ts.Toggled -= ts_Output_Switched;

            Outputs.Children.Clear();

            foreach (Output output in _outputs)
            {
                ToggleSwitch ts = new ToggleSwitch();

                ts.Style = Application.Current.Resources["ToggleSwitchStyle"] as Style;
                ts.Name = output.Id.ToString(CultureInfo.CurrentCulture);
                ts.Header = output.Name;
                ts.IsOn = output.Enabled;
                ts.Width = 300;
                ts.Toggled += ts_Output_Switched;
                Outputs.Children.Add(ts);
            }
        }

        private void ts_Output_Switched(object sender, RoutedEventArgs e)
        {
            var ts = sender as ToggleSwitch;
            int output = int.Parse(ts.Name, NumberStyles.Integer, CultureInfo.InvariantCulture);
            if (ts.IsOn)
                _server.EnableOutput(output);
            else
                _server.DisableOutput(output);
        }


        public void Dispose()
        {
            _server.DataReady -= _server_DataReady;

            foreach (ToggleSwitch ts in Outputs.Children)
            {
                ts.Toggled -= ts_Output_Switched;
            }
        }

        public void HandleUserResponse(MsgBoxButton pressedButton)
        {
            throw new NotImplementedException();
        }
    }
}
