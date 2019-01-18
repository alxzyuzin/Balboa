using System.ComponentModel;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Balboa
{
    public sealed partial class PlaylistNameInput : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _tmpPlaylistName = string.Empty;
        public string TmpPlaylistName
        {
            get { return _tmpPlaylistName; }
            set
            {
                if (_tmpPlaylistName != value)
                {
                    _tmpPlaylistName = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TmpPlaylistName)));
                }
            }
        }


        private string _playlistName = string.Empty;
        public string PlaylistName
        {
            get { return _playlistName; }
            set
            {
                if (_playlistName != value)
                {
                    _playlistName = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlaylistName)));
                }
            }
        }

        private string _playlistNameUTF8 = string.Empty;
        public string PlaylistNameUTF8
        {
            get { return _playlistNameUTF8; }
            set
            {
                if (_playlistNameUTF8 != value)
                {
                    _playlistNameUTF8 = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlaylistNameUTF8)));
                }
            }
        }

        public PlaylistNameInput()
        {
            this.InitializeComponent();
        }

        public PlaylistNameInput(string playliatName):this()
        {
            TmpPlaylistName = PlaylistName = playliatName;
        }

        private void btn_PlaylistNameSave_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (TmpPlaylistName != null)
            {
                string str = TmpPlaylistName;
                Encoding encoding = Encoding.Unicode;
                byte[] encBytes = encoding.GetBytes(str);
                byte[] utf8Bytes = Encoding.Convert(encoding, Encoding.UTF8, encBytes);
                PlaylistNameUTF8 = Encoding.UTF8.GetString(utf8Bytes, 0, utf8Bytes.Length);
                PlaylistName = TmpPlaylistName;
            }
            (this.Parent as Popup).IsOpen = false;
        }

        private void btn_PlaylistNameCancel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            (this.Parent as Popup).IsOpen = false;
        }
    }  // class PlaylistNameInput
}
