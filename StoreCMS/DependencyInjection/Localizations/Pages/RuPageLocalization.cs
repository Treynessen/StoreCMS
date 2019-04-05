using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Trane.Localizations
{
    public class RuPageLocalization : IPageLocalization
    {
        public string AddPage => "Добавить страницу";

        public string Title => "Заголовок";
        public string BreadcrumbName => "Псевдоним";
        public string ThisFieldIsRequired => "Это поле является обязательным";
    }
}
