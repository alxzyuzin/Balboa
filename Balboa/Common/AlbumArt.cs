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
//        private StorageFile         _file;
        private IRandomAccessStream _fileStream;

        private BitmapImage _image;//
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

        public async Task LoadImageData(string musicCollectionFolder, string fileName, string albumCoverFileNames)
        {
            

            string folderName = ExtractFilePath(fileName);

            while (musicCollectionFolder.EndsWith("\\", StringComparison.Ordinal))
                musicCollectionFolder = musicCollectionFolder.Substring(0, musicCollectionFolder.Length - 1);

            StringBuilder sb = new StringBuilder(musicCollectionFolder);

            sb.Append('\\');
            sb.Append(folderName);
            sb.Replace('/', '\\');
            sb.Append('\\');

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
                }
                catch (Exception ee)
                {
                    Error = string.Format(_resldr.GetString("Exception"), ee.GetType().ToString(), ee.Message);
                }
            }
            
                if (file == null) return;
                _fileStream = await file.OpenAsync(FileAccessMode.Read);
            
        }


        public async Task UpdateImage()
        {
            if (_fileStream == null || _fileStream.Size == 0)
                return;
            Image = new BitmapImage();
            try
            {
                await Image.SetSourceAsync(_fileStream);
                //using (_fileStream)
                //{
                //await Image.SetSourceAsync(_fileStream);
                //}
            }
            catch(Exception ex)
            {
                ;
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
        public static string ExtractFilePath(string path)
        {
            //path ?? throw new BalboaNullValueException("AlbumArt.cs", "ExtractFilePath", "62", "path");

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
