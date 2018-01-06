// Пример того как загрузить Control template из XAML
// В Balboa пока не используется

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;

namespace Balboa.Common
{
    public class AppBatton: Button
    {
        private void SetControlTemplate(int width, int height)
        {
            /*
            Button button = new Button();
            string template =
                "<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
            TargetType =\"Button\">" +
             "<Border>" +
                  "<ContentPresenter/>" +
             "</Border>" +
         "</ControlTemplate>";
            button.Template = (ControlTemplate)XamlReader.LoadWithInitialTemplateValidation(template);
            */
        }
    }
}
