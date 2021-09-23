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
    public enum ImageLoadResult
        {
            Loaded,
            NotFound,
            UnauthorizedAccess,
            Error,
            ZeroFileSize
        }

    public class AlbumArt : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Properties
        private ResourceLoader _resldr = new ResourceLoader();
        private IRandomAccessStream _fileStream;

        private BitmapImage _image;
        public BitmapImage Image
        {
            get { return _image; }
            private set
            {
                if (_image != value)
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

        #endregion

        public AlbumArt()
        {
        }

        public async Task<ImageLoadResult> LoadImageData(string musicCollectionFolder, string fileName, string albumCoverFileNames)
        {

            if (fileName == "")
                return ImageLoadResult.NotFound;

            string folderName = ExtractFilePath(fileName.Replace('/', '\\'));
            // Remove all simbols "\" from end of musicCollectionFolder
            while (musicCollectionFolder.EndsWith("\\", StringComparison.Ordinal))
                musicCollectionFolder = musicCollectionFolder.Substring(0, musicCollectionFolder.Length - 1);

            StringBuilder sb = new StringBuilder(musicCollectionFolder).Append('\\').Append(folderName).Append('\\');

            string[] CoverFileNames = albumCoverFileNames.Split(';');
            StorageFile file = null;
            foreach (string albumCoverFileName in CoverFileNames)
            {
                try
                {
                    file = await StorageFile.GetFileFromPathAsync(new StringBuilder(sb.ToString()).Append(albumCoverFileName).ToString());
                    break;
                }
                catch (FileNotFoundException)
                {
                    //  Just catch exeption and do nothing hear. Try another file name from CoverFileNames
                }
                catch (UnauthorizedAccessException)
                {
                    Error = string.Format(_resldr.GetString("CheckDirectoryAvailability"), musicCollectionFolder);
                    return ImageLoadResult.UnauthorizedAccess;
                }
                catch (Exception ee)
                {
                    Error = string.Format(_resldr.GetString("Exception"), ee.GetType().ToString(), ee.Message);
                    return ImageLoadResult.Error;
                }
            }

            if (file == null)
            {
               
                return ImageLoadResult.NotFound;
            }

            try
            {
                _fileStream = await file.OpenAsync(FileAccessMode.Read);
                if (_fileStream.Size == 0)
                {
                    Error = $"File {fileName} size is 0.";
                    _fileStream.Dispose();
                    _fileStream = null;
                    return ImageLoadResult.ZeroFileSize;
                }

            }
            catch (Exception ex)
            {
                Error = string.Format(_resldr.GetString("Exception"), ex.GetType().ToString(), ex.Message);
                _fileStream = null;
                return ImageLoadResult.Error;
            }
            return ImageLoadResult.Loaded;
        }


        public async Task<bool> UpdateImage()
        {
            try
            {
                Image = new BitmapImage();
                if (_fileStream != null)
                    await Image.SetSourceAsync(_fileStream);
                else
                    Image.UriSource= new Uri("ms-appx:///Assets/DefaultAlbumArt.jpg");
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Извлекает путь к файлу из строки передаваемой во входном параметре
        /// содержащей путь и имя файла
        /// </summary>
        /// <param name="s">
        /// Строка для разбора
        /// </param>
        /// <returns></returns>
        private string ExtractFilePath(string path)
        {
            int i = path.LastIndexOf('\\');
            return (i < 0) ? path : path.Substring(0,i);
        }
    }
}
