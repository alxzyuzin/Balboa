/*-----------------------------------------------------------------------
 * Copyright 2017 Alexandr Zyuzin.
 *
 * This file is part of MPD client Balboa.
 *
 * Главная страница приложения
 *
 --------------------------------------------------------------------------*/

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
        private Server _server;

        public enum DataPanelState { CurrentTrack, CurrentPlaylist, FileSystem, Playlists, Statistic, Artists, Genres, Search, Settings }

        public MainPage()
        {
            this.InitializeComponent();
            Application.Current.Suspending += OnSuspending;
            Application.Current.Resuming += OnResuming;
            this.SizeChanged += MainPage_SizeChanged;

            _server = new Server(this);

            _server.Error += OnServerError;
            _server.CriticalError += OnServerCriticalError;

            var mainMenu = new MainMenu(_server);
            mainMenu.ActionRequested += OnDataPanelActionRequested;
            grid_Main.Children.Add(mainMenu); 
            grid_Main.Children.Add(new PageHeader(_server));
            

            p_PlayMode.Init(_server);
            p_ControlPanel.Init(_server);

            if (_server.Initialized)
                _server.Start();         // Запускаем сеанс взаимодействия с MPD
        }

        private void OnDataPanelActionRequested(Object sender, ActionParams actionParams)
        {
            if (actionParams.ActionType.HasFlag(ActionType.ActivateDataPanel))
            {
                ActivatePanel(actionParams.PanelClassName);
            }
            if (actionParams.ActionType.HasFlag(ActionType.DisplayMessage))
            {
                MsgBoxButton pressedButton = MsgBoxButton.Close;// = await DisplayMessage(actionParams.Message);
                ((IDataPanel)sender).HandleUserResponse(pressedButton);
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
        private void ActivatePanel(string panelClassName)
        {
            if (DataPanel.Child != null)
                ((IActionRequested)DataPanel.Child).ActionRequested -= OnDataPanelActionRequested;

            Type t = Type.GetType("Balboa." + panelClassName);
            if (t == null)  throw new ArgumentNullException($"Class '{panelClassName}' does not exist.");
            IDataPanel panel = Activator.CreateInstance(t, _server) as IDataPanel;
            DataPanel.Child = panel as UserControl;
            //((IDataPanel)panel).Init(_server);
            //((IDataPanel)panel).Update();
            //((IActionRequested)panel).ActionRequested += OnDataPanelActionRequested;

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

  