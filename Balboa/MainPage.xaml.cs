/*-----------------------------------------------------------------------
 * Copyright 2017 Alexandr Zyuzin.
 *
 * This file is part of MPD client Balboa.
 *
 * Главная страница приложения
 *
 --------------------------------------------------------------------------*/

using Balboa.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


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

    public partial class MainPage : Page
    {
        private ResourceLoader _resldr = new ResourceLoader();
        private Server _server  = new Server();
        private MainMenu _mainMenu;

        public enum DataPanelState { CurrentTrack, CurrentPlaylist, FileSystem, Playlists, Statistic, Artists, Genres, Search, Settings }

        public MainPage()
        {
            this.InitializeComponent();
            Application.Current.Suspending += OnSuspending;
            Application.Current.Resuming += OnResuming;
            this.SizeChanged += MainPage_SizeChanged;

            _server.ServerError += async (object server, MpdResponse e) =>
                    {
                            await DisplayMessage(new Message(MsgBoxType.Error, e.ErrorMessage, MsgBoxButton.Close, 200));
                    };

            //_server.Error += OnServerError;
            //_server.CriticalError += OnServerCriticalError;

            _mainMenu = new MainMenu(_server);
            _mainMenu.RequestAction += OnDataPanelActionRequested;
            grid_Main.Children.Add(_mainMenu);

            PageHeaderPanel.Init(_server);
            PlayModePanel.Init(_server);
            PlayControlPanel.Init(_server);

            if (_server.Initialized)
                _server.Start();         // Запускаем сеанс взаимодействия с MPD
        }

        private async void OnDataPanelActionRequested(Object sender, ActionParams actionParams)
        {
            if (actionParams.ActionType.HasFlag(ActionType.ActivateDataPanel))
            {
                ActivatePanel(actionParams.PanelClass);
            }
            if (actionParams.ActionType.HasFlag(ActionType.DisplayMessage))
            {
                MsgBoxButton pressedButton = await DisplayMessage(actionParams.Message);
                ((IRequestAction)sender).HandleUserResponse(pressedButton);
            }
        }

        private void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //var displayinformation = DisplayInformation.GetForCurrentView();

            //if (displayinformation.CurrentOrientation == DisplayOrientations.Landscape || displayinformation.CurrentOrientation == DisplayOrientations.LandscapeFlipped)
            //{
            //    if (e.NewSize.Width >= 1100)
            //    {
            //        VisualStateManager.GoToState(this, "Default", true);
            //    }

            //    if (e.NewSize.Width < 1100)
            //    {
            //        // Прячем горизонтальный регулятор громкости
            //        // Показываем кнопку регулятора громкости
            //        // Меняем размер колонки 1 в панели управления воспроизведения grid_PlayControls.ColumnDefinitions[1];
            //        // (меняем 300 на 20*
            //        VisualStateManager.GoToState(this, "Filled", true);
            //    }

            //    if (e.NewSize.Width < 900)
            //    {
            //        VisualStateManager.GoToState(this, "Narrow", true);
            //    }

            //    if (e.NewSize.Width < 620)
            //    {
            //        VisualStateManager.GoToState(this, "SuperNarrow", true);
            //    }
            //}

            //if (displayinformation.CurrentOrientation == DisplayOrientations.Portrait || displayinformation.CurrentOrientation == DisplayOrientations.PortraitFlipped)
            //{
            //    VisualStateManager.GoToState(this, "Portrait", true); 
            //}
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

        private async void OnServerCriticalError(object sender, EventArgs eventArgs)
        {
            //_server.Halt();
            //MsgBoxButtons responce = await MessageBox(_resldr.GetString("CriticalError"), ((ServerEventArgs)eventArgs).Message, MsgBoxButtons.GoToSettings | MsgBoxButtons.CloseApplication);
            //switch (responce)
            //{
            //    case MsgBoxButtons.CloseApplication: Application.Current.Exit();break;
            //    case MsgBoxButtons.Retry: _server.Restart();break;
            //                case MsgBoxButtons.GoToSettings: SwitchDataPanelsTo(DataPanelState.Settings);break;
            //            }
        }

        #endregion
        private void ActivatePanel(Panels panelClass)
        {
            if (DataPanel.Child != null)
            {
                if (DataPanel.Child as IRequestAction != null)
                    ((IRequestAction)DataPanel.Child).RequestAction -= OnDataPanelActionRequested;
                ((IDisposable)DataPanel.Child).Dispose();
            }
            Type t = Type.GetType("Balboa." + panelClass.ToString());
            if (t == null)  throw new ArgumentNullException($"Class '{panelClass.ToString()}' does not exist.");
            var panel = Activator.CreateInstance(t, _server) as UserControl;
            if (panel as IRequestAction != null)
                ((IRequestAction)panel).RequestAction += OnDataPanelActionRequested;
            DataPanel.Child = panel;
            _mainMenu.SelectItem(panelClass);



                //if (state == DataPanelState.CurrentTrack)
                //    p_PageHeader.Visibility = Visibility.Collapsed;
                //else
                //    p_PageHeader.Visibility = Visibility.Visible;
        }


        [Flags]
        private enum MsgBoxButtons { OK = 1,Cancel = 2, Continue = 4, Retry = 8,  Exit = 16, GoToSettings = 32, CloseApplication = 64 }

        private async Task<MsgBoxButton> DisplayMessage(Message messageArgs)
        {
            MsgBox.SetButtons(messageArgs.Buttons);
            MsgBox.Message = messageArgs.Text;
            MsgBox.BoxHeight = messageArgs.BoxHeight;
            return await MsgBox.Show();
        }
    }
}

  