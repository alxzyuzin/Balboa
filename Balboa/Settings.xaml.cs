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

    

    public sealed partial class Settings : UserControl, INotifyPropertyChanged, IDataPanel
    {
        public event PropertyChangedEventHandler   PropertyChanged;
        public event ActionRequestedEventHandler ActionRequested;

        //        public event EventHandler<DisplayMessageEventArgs>  MessageReady;

        private AppSettings _appSettings;
        private Server _server;
        private List<Output> _outputs =  new List<Output>();
        private ResourceLoader _resldr = new ResourceLoader();

        //private Message _message;
        //public Message Message
        //{
        //    get { return _message; }
        //    private set
        //    {
        //        if (_message != value)
        //        {
        //            _message = value;
        //            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
        //        }
        //    }
        //}

        //private ControlAction _action;
        //public ControlAction Action
        //{
        //    get { return _action; }
        //    private set
        //    {
        //        if (_action != value)
        //        {
        //            _action = value;
        //            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
        //        }
        //    }
        //}

       

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

        public void Update() { }

        private void _server_DataReady(object sender, EventArgs e)
        {
            var mpdData = e as MpdResponse;
            if (mpdData.Keyword == ResponseKeyword.OK && mpdData.Command.Op == "outputs")
            {
                UpdateDataCollection(mpdData.Content);
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

        private async void appbtn_SaveSettings_Tapped(object sender, TappedRoutedEventArgs e)
        {
            string notNumber = @"\D{1,}";

            if (Regex.IsMatch(_appSettings.Port, notNumber) || _appSettings.Port.Length == 0)
            {
                //Message = new Message(MsgBoxType.Error, _resldr.GetString("PortValueMustBeNumber"), MsgBoxButton.Close, 200);
                return;
            }
            else
            {
                int port = int.Parse(_appSettings.Port);
                if (port > 65535)
                {
                  //  Message = new Message(MsgBoxType.Error, _resldr.GetString("PortValueMustBeLess65536"), MsgBoxButton.Close, 200);
                    return;
                }
            }

            if (Regex.IsMatch(_appSettings.ViewUpdateInterval, notNumber) || _appSettings.ViewUpdateInterval.Length == 0)
            {
                //Message = new Message(MsgBoxType.Error, _resldr.GetString("ViewUpdateIntervalMustNumber"), MsgBoxButton.Close, 200);
                return;
            }
            else
            {
                int updateinterval = int.Parse(_appSettings.ViewUpdateInterval);
                if (updateinterval < 100)
                {
                  //  Message = new Message(MsgBoxType.Error, _resldr.GetString("ViewUpdateIntervalMustBe100"), MsgBoxButton.Close, 200);
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
                //Message = new Message(MsgBoxType.Info, $"Connection to {_appSettings.Server} failed.\n {connection.Error}.", MsgBoxButton.Close, 200);
                return;
            }
            if (_appSettings.ServerNameChanged && (!_appSettings.MusicCollectionFolderTokenChanged))
            {
                //Message = new Message(MsgBoxType.Warning, _resldr.GetString("ServerNameChanged"), MsgBoxButton.Close, 200);
                _appSettings.MusicCollectionFolder = string.Empty;            
                StorageApplicationPermissions.FutureAccessList.Clear();
                _appSettings.MusicCollectionFolderToken = String.Empty;
            }
            connection.Close();
            _appSettings.Save();
            //Action = ControlAction.RestartServer;
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));

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
            string msg;
            
            Connection connection = new Connection();

            await connection.Open(_appSettings.Server, _appSettings.Port,_appSettings.Password);
            if (connection.IsActive)
                msg = $"Succesfully connected to {_appSettings.Server}. \n{connection.InitialResponse}";
            else
                msg = connection.Error;
            //Message =  new Message(MsgBoxType.Info, msg, MsgBoxButton.Close, 200);
            connection.Close();
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

        public void HandleUserResponse(MsgBoxButton pressedButton)
        {
            throw new NotImplementedException();
        }
    }
}
