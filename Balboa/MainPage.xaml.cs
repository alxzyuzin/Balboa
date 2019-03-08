/*-----------------------------------------------------------------------
 * Copyright 2017 Alexandr Zyuzin.
 *
 * This file is part of MPD client Balboa.
 *
 * Application main page
 *
 --------------------------------------------------------------------------*/

using Balboa;
using Balboa.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;


// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237
[assembly: CLSCompliant(false)]

namespace Balboa
{

    //public static IEnumerable<T> ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
    //{
    //    foreach (T item in enumeration)
    //    {
    //        action(item);
    //        yield return item;
    //    }
    //}
    // Change the current culture to the French culture,
    // and parsing the same string yields a different value.
    //  Thread.CurrentThread.CurrentCulture = new CultureInfo("Fr-fr", true);
    //  myDateTime = DateTime.Parse(dt);
    [Flags]
    public enum DataPanelLayout
    {
        Horizontal = 1,
        Vertical = 2,
        Wide = 4,
        Narrow = 8
    }

  

    public partial class MainPage : Page, INotifyPropertyChanged
    {

        // Screen width level for switching sreen layouts

        // if MainWindowWidth < Width_1 hide BottomTrackInfoPanel and display TopTrackInfoPanel
        private const int Width_1 = 920;
        // if MainWindowWidth < Width_2 hide DataInfoPanel
        private const int Width_2 = 680;


        [Flags]
        private enum ExtendedStatusMode { BitPersample = 1, Channels = 2 }

        public event PropertyChangedEventHandler PropertyChanged;

        private ResourceLoader _resldr = new ResourceLoader();
        private Server _server  = new Server();
        private Status _status = new Status();
        private UserControl _activeDataPanel;
        private DisplayOrientations _displayOrientation;

        public  PlaylistPanel ActiveDataPanel { get { return _activeDataPanel as PlaylistPanel; } }

        private ExtendedStatusMode  _extendedStatusMode = ExtendedStatusMode.BitPersample | ExtendedStatusMode.Channels;
        private string _extendedStatus;
        public string ExtendedStatus
        {

            get { return _extendedStatus; }
            private set
            {
                if (_extendedStatus != value)
                {
                    _extendedStatus = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExtendedStatus)));
                }
            }
        }

        private string _connectionStatus = string.Empty;
        public string ConnectionStatus
        {
            get { return _connectionStatus; }
            private set
            {
                if (_connectionStatus != value)
                {
                    _connectionStatus = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ConnectionStatus)));
                }
            }
        }

        private double _appWindowWIdth;
        public double MainWindowWIdth
        {
            get { return _appWindowWIdth; }
            private set
            {
                if (_appWindowWIdth != value)
                {
                    _appWindowWIdth = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MainWindowWIdth)));
                }
            }
        }

        public MainPage()
        {
            this.InitializeComponent();
            _displayOrientation = DisplayInformation.GetForCurrentView().CurrentOrientation;

            Application.Current.Suspending += OnSuspending;
            Application.Current.Resuming += OnResuming;
            this.SizeChanged += MainPage_SizeChanged;
            

            _server.ConnectionStatusChanged += (object sender, string status) =>
                                            {
                                                ConnectionStatus = status;
                                                if (_server.IsConnected)
                                                    ((IDataPanel)_activeDataPanel)?.Update();
                                             };
            _server.DataReady += OnServerDataReady; 
                    
            _server.ServerError += async (object server, MpdResponse e) =>
                    {
                        MsgBoxButton pressedButton = await DisplayMessage(new Message(MsgBoxType.Error, 
                                                                          e.ErrorMessage, 
                                                                          MsgBoxButton.Reconnect | MsgBoxButton.Exit));
                        switch(pressedButton)
                        {
                            case MsgBoxButton.Reconnect: _server.Start(); break;
                            case MsgBoxButton.Exit: App.Current.Exit(); break;
                        }
                    };

            MainMenuPanel.Init(_server);
            MainMenuPanel.RequestAction += OnDataPanelActionRequested;
 
            TopTrackInfoPanel.Init(_server);
            BottomTrackInfoPanel.Init(_server);
            PlayControlPanel.Init(_server);

            if (_server.Initialized)
                _server.Start();         // Запускаем сеанс взаимодействия с MPD

            var actionParams = new ActionParams(ActionType.ActivateDataPanel).SetPanel(new PlaylistPanel(_server));
            OnDataPanelActionRequested(this, actionParams);
        
        }

        private void OnServerDataReady(object sender, MpdResponse data)
        {
            {
                if (data.Keyword == ResponseKeyword.OK)
                {
                    if (data.Command.Op == "status")
                    {
                        _status.Update(data.Content);
                        ExtendedStatus = AssembleExtendedStatus();

                        if (!(_activeDataPanel is CurrentTrackPanel))
                        {
                            if (_status.State == "stop")
                            {
                                TopTrackInfoPanel.Hide();
                                BottomTrackInfoPanel.Hide();
                            }
                            else
                            {
                                if (MainWindowWIdth >= Width_1)
                                    BottomTrackInfoPanel.Show();
                                else
                                    TopTrackInfoPanel.Show();
                            }
                        }
                    }
                }

                if (data.Keyword != ResponseKeyword.OK)
                {
                    ;
                }
            }
        }

        private string AssembleExtendedStatus()
        {
            var sb = new StringBuilder();
            switch (_status.State)
            {
                case "pause": sb.Append("Paused.   "); break; 
                case "play": sb.Append("Playing.   "); break; 
                case "stop": sb.Append("Stopped."); break;
                case "restart": sb.Append("Restarting ..."); break;
                default: return string.Empty;
            }
            if (_status.State == "stop")
                return sb.ToString();

            sb.Append($"Sample rate: {_status.SampleRate.ToString(CultureInfo.InvariantCulture)} kHz.");

            if (_extendedStatusMode.HasFlag(ExtendedStatusMode.BitPersample))
                sb.Append($" {_status.Bits.ToString(CultureInfo.InvariantCulture)} bits per sample.");

            if (_extendedStatusMode.HasFlag(ExtendedStatusMode.Channels))
                sb.Append($" channels: {_status.Channels.ToString(CultureInfo.InvariantCulture)}.");

            return sb.ToString();
        }

        /// <summary>
        ///  Функция вызывается когда одна из панелей запрашивает действие у главного окна
        ///  Возможные действия 
        ///     - Активировать указанную панель данных
        ///     - Отобразить окно с сообщением пользователю
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="actionParams"></param>
        private async void OnDataPanelActionRequested(Object sender, ActionParams actionParams)
        {
            if (actionParams.ActionType.HasFlag(ActionType.ActivateDataPanel))
            {
                _activeDataPanel = actionParams.Panel as UserControl;
                ActivatePanel(_activeDataPanel as IRequestAction);
            }

            if (actionParams.ActionType.HasFlag(ActionType.DisplayMessage))
            {
                MsgBoxButton pressedButton = await DisplayMessage(actionParams.Message);
                ((IRequestAction)sender).HandleUserResponse(pressedButton);
            }
        }

        private void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MainWindowWIdth = e.NewSize.Width;
            SetDataInfoPanelWidth();
            SetActiveDataPanelOrientation();
            RearangeTrackInfoPanels();

            DataInfoPanel.Visibility = MainWindowWIdth >= Width_2 ? Visibility.Visible: Visibility.Collapsed ;

            MainMenuPanel.DisplayButtonsOnly = (MainWindowWIdth < Width_2) ? true : false;
            
            _extendedStatusMode =  ExtendedStatusMode.BitPersample | ExtendedStatusMode.Channels;
            if (MainWindowWIdth < 730)
                _extendedStatusMode ^= ExtendedStatusMode.Channels;

            if (MainWindowWIdth < 635)
                _extendedStatusMode ^= ExtendedStatusMode.BitPersample;
            }

        private void SetActiveDataPanelOrientation()
        {
            if (_displayOrientation == DisplayOrientations.Landscape || _displayOrientation == DisplayOrientations.LandscapeFlipped)
            {
                if (_activeDataPanel != null)
                    ((IDataPanel)_activeDataPanel).Orientation = Orientation.Horizontal;
                TopTrackInfoPanel.ImageWidth = 90;
            }
            else
            {
                if (_activeDataPanel != null)
                    ((IDataPanel)_activeDataPanel).Orientation = Orientation.Vertical;
                TopTrackInfoPanel.ImageWidth = 190;
            }
        }

        private void RearangeTrackInfoPanels()
        {
            // Control TrackInfoPanel's

            if (_activeDataPanel is CurrentTrackPanel)
            {
                TopTrackInfoPanel.Collapse();
                BottomTrackInfoPanel.Hide();
            }
            else
            {
                if (MainWindowWIdth >= Width_1)
                {
                    TopTrackInfoPanel.Collapse();
                    if (_status.State =="play")
                        BottomTrackInfoPanel.Show();
                }
                else
                    TopTrackInfoPanel.Expand();
            }

            var displayOrientation = DisplayInformation.GetForCurrentView().CurrentOrientation;
            //if (displayOrientation == DisplayOrientations.Landscape || displayOrientation == DisplayOrientations.LandscapeFlipped)
            //    TopTrackInfoPanel.ImageWidth = 90;
            //else
            //    TopTrackInfoPanel.ImageWidth = 244;

            if (MainWindowWIdth >= Width_1)
                BottomTrackInfoPanel.Expand();
            else
                BottomTrackInfoPanel.Collapse();




        }

        private void SetDataInfoPanelWidth()
        {
            if (_activeDataPanel as IDataPanelInfo != null)
                DataInfoPanel.Width = Math.Max(MainWindowWIdth
                                                - ((IDataPanelInfo)_activeDataPanel).TotalButtonWidth
                                                - BottomTrackInfoPanel.ActualWidth
                                                - 20, 0);
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            _server.Halt();
        }

        private void OnResuming(Object sender, Object e)
        {
            _server.Start();
        }

        #region Обработчики событий объекта Server

        private async void OnServerError(object sender, EventArgs eventArgs)
        {
            //_server.Halt();
            //switch(await MessageBox(_resldr.GetString("Error"),((ServerEventArgs) eventArgs).Message, MsgBoxButtons.GoToSettings | MsgBoxButtons.Retry | MsgBoxButtons.Exit))
            //{
            //    case MsgBoxButtons.Exit: Application.Current.Exit(); break;
            //    case MsgBoxButtons.Retry: _server.Start(); break;
            //                case MsgBoxButtons.GoToSettings: SwitchDataPanelsTo(DataPanelState.Settings);break;
            //            }
        }

        //private async void OnServerCriticalError(object sender, EventArgs eventArgs)
        //{
        //    //_server.Halt();
        //    //MsgBoxButtons responce = await MessageBox(_resldr.GetString("CriticalError"), ((ServerEventArgs)eventArgs).Message, MsgBoxButtons.GoToSettings | MsgBoxButtons.CloseApplication);
        //    //switch (responce)
        //    //{
        //    //    case MsgBoxButtons.CloseApplication: Application.Current.Exit();break;
        //    //    case MsgBoxButtons.Retry: _server.Restart();break;
        //    //                case MsgBoxButtons.GoToSettings: SwitchDataPanelsTo(DataPanelState.Settings);break;
        //    //            }
        //}

        #endregion
 

        private void ActivatePanel(IRequestAction panel)
        {
            if (panel == null) throw new ArgumentNullException(nameof(panel),"Not defined panel to activate");

            if (DataPanel.Child != null)
            {
                ((IRequestAction)DataPanel.Child).RequestAction -= OnDataPanelActionRequested;
                ((IDisposable)DataPanel.Child)?.Dispose();
            }
            panel.RequestAction += OnDataPanelActionRequested;
            DataPanel.Child = panel as UserControl;
            DataInfoPanel.DataContext = panel as IDataPanelInfo;
            MainMenuPanel.HighLiteSelectedItem(panel.GetType().Name);
            SetActiveDataPanelOrientation();
            RearangeTrackInfoPanels();
            SetDataInfoPanelWidth();
        }

        private void SwitchMenuState(object sender, TappedRoutedEventArgs e)
        {
            MainMenuPanel.DisplayButtonsOnly = !MainMenuPanel.DisplayButtonsOnly;
        }

        private async Task<MsgBoxButton> DisplayMessage(Message message)
        {
            MsgBox.SetButtons(message.Buttons);
            MsgBox.Message = message.Text;
            MsgBox.InnerBoxHeight = message.BoxHeight;
            MsgBox.InnerBoxWidth = message.BoxWidth;
            return await MsgBox.Show();
        }
    }
}

  