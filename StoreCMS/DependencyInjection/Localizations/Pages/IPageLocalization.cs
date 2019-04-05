using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Trane.Localizations
{
    public interface IPageLocalization
    {
        string AddPage { get; }

        string Title { get; }
        string BreadcrumbName { get; }
        string ThisFieldIsRequired { get; }
    }
}
