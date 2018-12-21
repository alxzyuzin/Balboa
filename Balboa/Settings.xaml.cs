using Balboa.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Balboa
{
    public delegate void ErrorEventHandler<ErrorEventArgs>(object sender, ErrorEventArgs eventArgs);

    public class DisplayMessageEventArgs : EventArgs
    {
        public DisplayMessageEventArgs(MsgBoxType type, string message, MsgBoxButton buttons, int boxHeight )
        {
            Type = type;
            Message = message;
            Buttons = buttons;
            BoxHeight = boxHeight;
        }
        public MsgBoxType Type { get; private set; }
        public string Message { get; private set; } = string.Empty;
        public MsgBoxButton Buttons { get; private set; } = MsgBoxButton.Close;
        public int BoxHeight { get; private set; } = 200;
    }

    public sealed partial class Settings : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler   PropertyChanged;
        public event EventHandler<DisplayMessageEventArgs>  MessageReady;

        private AppSettings _appSettings;
        private Server _server;
        private List<Output> _outputs =  new List<Output>();
        private ResourceLoader _resldr = new ResourceLoader();

        public Settings()
        {
            this.InitializeComponent();
        }

        public void Init(Server server)
        {
            _server = server;
            _appSettings = server.Settings;
            this.DataContext = _appSettings;
            _server.DataReady += _server_DataReady;
        }

        private void _server_DataReady(object sender, EventArgs e)
        {
            var mpdData = e as MpdResponse;
            if (mpdData.Command.Op == "outputs" && mpdData.Keyword == ResponseKeyword.Ok)
            {
                UpdateOutputsCollection(mpdData.Content);
                UpdateToggleSwitchPanel();
            }
        }


        private void UpdateToggleSwitchPanel()
        {
            stp_Outputs.Children.Clear();

            foreach (Output output in _outputs)
            {
                ToggleSwitch ts = new ToggleSwitch();

                ts.Style = Application.Current.Resources["ToggleSwitchStyle"] as Style;
                ts.Name = output.Id.ToString(CultureInfo.CurrentCulture);
                ts.Header = output.Name;
                ts.IsOn = output.Enabled;
                ts.Width = 300;
                ts.Toggled += ts_Output_Switched;
                stp_Outputs.Children.Add(ts);
            }
        }

        public void UpdateOutputsCollection(List<string> serverData)
        {
            _outputs.Clear();
            while (serverData.Count > 0) 
            {
                var output = new Output();
                output.Update(serverData);
                _outputs.Add(output);
            }
        }

        private async void appbtn_SaveSettings_Tapped(object sender, TappedRoutedEventArgs e)
        {
            string notNumber = @"\D{1,}";

            if (Regex.IsMatch(_appSettings.Port, notNumber) || _appSettings.Port.Length == 0)
            {
                var args = new DisplayMessageEventArgs(MsgBoxType.Error, _resldr.GetString("PortValueMustBeNumber"),
                                                    MsgBoxButton.Close, 200);
                MessageReady?.Invoke(this, args);
                return;
            }
            else
            {
                int port = int.Parse(_appSettings.Port);
                if (port > 65535)
                {
                    var args = new DisplayMessageEventArgs(MsgBoxType.Error, _resldr.GetString("PortValueMustBeLess65536"),
                                                    MsgBoxButton.Close, 200);
                    MessageReady?.Invoke(this, args);
                    return;
                }
            }

            if (Regex.IsMatch(_appSettings.ViewUpdateInterval, notNumber) || _appSettings.ViewUpdateInterval.Length == 0)
            {
                var args = new DisplayMessageEventArgs(MsgBoxType.Error, _resldr.GetString("ViewUpdateIntervalMustNumber"),
                                                    MsgBoxButton.Close, 200);
                MessageReady?.Invoke(this, args);
                return;
            }
            else
            {
                int updateinterval = int.Parse(_appSettings.ViewUpdateInterval);
                if (updateinterval < 100)
                {
                    var args = new DisplayMessageEventArgs(MsgBoxType.Error, _resldr.GetString("ViewUpdateIntervalMustBe100"),
                                                     MsgBoxButton.Close, 200);
                    MessageReady?.Invoke(this, args);
                    return;
                }
            }

            if (!_appSettings.SettingsChanged)
                  return;
            // Проверим возможность соединения с новыми параметрами перед их сохранением
            Connection connection = new Connection();
            await connection.Open(_appSettings.Server, _appSettings.Port, _appSettings.Password);
            

            if (!connection.IsActive)
            {
                string msg = $"Connection to {_appSettings.Server} failed.\n {connection.Error}.";
                MessageReady?.Invoke(this, new DisplayMessageEventArgs(MsgBoxType.Info, msg, MsgBoxButton.Close, 200));
                return;
            }
            if (_appSettings.ServerNameChanged && (!_appSettings.MusicCollectionFolderTokenChanged))
            {
                var args = new DisplayMessageEventArgs(MsgBoxType.Warning, _resldr.GetString("ServerNameChanged"),
                                                     MsgBoxButton.Close, 200);
                MessageReady?.Invoke(this, args);
                _appSettings.MusicCollectionFolder = string.Empty;            
                StorageApplicationPermissions.FutureAccessList.Clear();
                _appSettings.MusicCollectionFolderToken = String.Empty;
            }
            connection.Close();
            _appSettings.Save();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));

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

        private async void appbtn_TestConnection_Tapped(object sender, TappedRoutedEventArgs e)
        {
            string message = string.Empty;
            Connection connection = new Connection();

            await connection.Open(_appSettings.Server, _appSettings.Port,_appSettings.Password);
            if (connection.IsActive)
                message = $"Succesfully connected to {_appSettings.Server}. \n{connection.InitialResponse}";
            else
                message = connection.Error;
 
            connection.Close();
            MessageReady?.Invoke(this, new DisplayMessageEventArgs(MsgBoxType.Info, message, MsgBoxButton.Close, 200));
        }

        private async void btn_SelectMusicCollectionPath_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            folderPicker.FileTypeFilter.Add("*");
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                _appSettings.MusicCollectionFolder = folder.Path;
                if (_appSettings.MusicCollectionFolderToken != String.Empty)
                    StorageApplicationPermissions.FutureAccessList.Clear();
                _appSettings.MusicCollectionFolderToken = StorageApplicationPermissions.FutureAccessList.Add(folder);
            }
        }

        private void btn_ClearMusicCollectionPath_Tapped(object sender, TappedRoutedEventArgs e)
        {
            StorageApplicationPermissions.FutureAccessList.Clear();
            _appSettings.MusicCollectionFolderToken = String.Empty;
            _appSettings.MusicCollectionFolder = String.Empty;
        }

        private void appbtn_StartSession_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _server.Restart();
        }

        private void appbtn_StopSession_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _server.Halt();
        }

    }
}
