using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace Balboa.Common
{
    public class AlbumArt: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ResourceLoader      _resldr = new ResourceLoader();
        private IRandomAccessStream _fileStream;

        private BitmapImage _image;
        public BitmapImage Image
        {
            get { return _image; }
            private set
            {
                if (_image!=value)
                {
                    _image = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Image)));
                }
            }
        }

        private string _error;
        public string Error
        {
            get { return _error; }
            private set
            {
                if (_error != value)
                {
                    _error = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Error)));
                }
            }
        }

        public async Task<bool> LoadImageData(string musicCollectionFolder, string fileName, string albumCoverFileNames)
        {
            if (fileName == "")
                return false;

            string ResourceStringException = _resldr.GetString("Exception");

            string folderName = ExtractFilePath(fileName);
            // Remove all simbols "\" from end of musicCollectionFolder
            while (musicCollectionFolder.EndsWith("\\", StringComparison.Ordinal))
                musicCollectionFolder = musicCollectionFolder.Substring(0, musicCollectionFolder.Length - 1);

            StringBuilder sb = new StringBuilder(musicCollectionFolder);

            sb.Append('\\').Append(folderName).Replace('/', '\\').Append('\\');

            int pathlength = sb.Length;

            string[] CoverFileNames = albumCoverFileNames.Split(';');
            StorageFile file =null;
            foreach (string albumCoverFileName in CoverFileNames)
            {
                try
                {
                    sb.Append(albumCoverFileName);
                    
                    file = await StorageFile.GetFileFromPathAsync(sb.ToString());
                    break;
                }
                catch (FileNotFoundException)
                {
                    sb.Remove(pathlength, albumCoverFileName.Length);
                }
                catch (UnauthorizedAccessException)
                {
                    Error = string.Format(_resldr.GetString("CheckDirectoryAvailability"), musicCollectionFolder);
                    return false;
                }
                catch (Exception ee)
                {
                    Error = string.Format(_resldr.GetString("Exception"), ee.GetType().ToString(), ee.Message);
                    return false;
                }
            }

            if (file == null)
                return false;

            try
            {
                _fileStream = await file.OpenAsync(FileAccessMode.Read);
                if (_fileStream.Size == 0)
                {
                    Error = $"File {fileName} size is 0.";
                    _fileStream.Dispose();
                    _fileStream = null;
                    return false;
                }

            }
            catch (Exception ex)
            {
                //Error = $"{ResourceStringException} {ex.GetType().ToString()} {}";
                Error = string.Format(_resldr.GetString("Exception"), ex.GetType().ToString(), ex.Message);
                _fileStream = null;
                return false;
            }
            return true;
        }


        public async Task UpdateImage()
        {
            if (_fileStream != null)
            {
                Image = new BitmapImage();
                await Image.SetSourceAsync(_fileStream);
            }
        }

        /// <summary>
        /// Извлекает имя файла из строки передаваемой во входном параметре
        /// Строка должна содержать корректное имя файла и путь
        /// </summary>
        /// <param name="s">
        /// Строка для разбора
        /// </param>
        /// <returns></returns>
        private string ExtractFilePath(string path)
        {
            if (path.Length == 0)
                return path;

            string[] fileparts = path.Split('/');
            StringBuilder filepath = new StringBuilder(fileparts[0]);
            for (int i = 1; i < fileparts.Length - 1; i++)
            {
                filepath.Append('\\');
                filepath.Append(fileparts[i]);
            }
            return filepath.ToString();
        }
    }
}
