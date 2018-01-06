
using System.ComponentModel;

namespace Balboa.Common
{
    public class Output: INotifyPropertyChanged, IUpdatable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public int    ID { get; set; }
        public string Name { get; set; }
        public bool   Enabled { get; set; }

        ///<summary>
        ///  Забирает из ответа сервера данные по одному выходу и 
        /// распределяет их по свойствам экземпляра
        /// Обработанные данные удаляются из ответа
        ///</summary>
        public void Update(MPDResponce responce)
        {
            int i = 0;
            do
            {
                string[] items = responce[i].Split(':');
                string tagname = items[0].ToLower();
                string tagvalue = items[1].Trim();
                switch (tagname)
                    {
                        case "outputid":
                            ID = int.Parse(tagvalue);
                            break;
                        case "outputname":
                            Name = tagvalue;
                            break;
                        case "outputenabled": Enabled = (tagvalue == "1") ? true : false; break;
                    }
                    i++;
                }
                while ((i < responce.Count) && (!responce[i].StartsWith("outputid")));
            responce.RemoveRange(0, i);
        }
    }
}
