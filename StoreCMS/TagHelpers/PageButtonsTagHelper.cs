using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Treynessen.TagHelpers
{
    // Html будет выглядеть следующим образом:
    // <ul class="Class"><li><span>НОМЕР</span></li><li><a>НОМЕР</a></li></ul>
    // где <li><span>НОМЕР</span></li> - неактивная кнопка (текущая страница)
    // <li><a>НОМЕР</a></li> - активная кнопка (следующая и/или предыдущая страница)
    // так же в блоке будут кнопки << и >>, которые перенаправляют на первую или последнюю страницу соответственно

    // Допустим:

    // что текущая кнопка - 1, а общее количество кнопок = 5, тогда
    // наш блок будет выглядеть следующим образом: (без стрелок)
    // |Кнопка 1| |Кнопка 2(актив)| |Кнопка 3(актив)| >>

    // что текущая кнопка - 3, а общее количество кнопок = 5, тогда
    // наш блок будет выглядеть следующим образом: (без стрелок)
    // |Кнопка 1(актив)| |Кнопка 2(актив)| |Кнопка 3| |Кнопка 4(актив)| |Кнопка 5(актив)|

    // что текущая кнопка - 3, а общее количество кнопок = 6, тогда
    // наш блок будет выглядеть следующим образом: (без стрелок)
    // |Кнопка 1(актив)| |Кнопка 2(актив)| |Кнопка 3| |Кнопка 4(актив)| |Кнопка 5(актив)| >>

    // что текущая кнопка - 4, а общее количество кнопок = 6, тогда
    // наш блок будет выглядеть следующим образом:
    // << |Кнопка 2(актив)| |Кнопка 3(актив)| |Кнопка 4| |Кнопка 5(актив)| |Кнопка 6(актив)|

    // что текущая кнопка - 4, а общее количество кнопок = 7, тогда
    // наш блок будет выглядеть следующим образом:
    // << |Кнопка 2(актив)| |Кнопка 3(актив)| |Кнопка 4| |Кнопка 5(актив)| |Кнопка 6(актив)| >>

    // что текущая кнопка - 5, а общее количество кнопок = 5, тогда
    // наш блок будет выглядеть следующим образом: (без стрелок)
    // << |Кнопка 3(актив)| |Кнопка 4(актив)| |Кнопка 5|

    public class PageButtonsTagHelper : TagHelper
    {
        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext Context { get; set; }

        public string CurrentPath { get; set; }
        public int? CurrentPage { get; set; }
        public int? PagesCount { get; set; }

        public string Style { get; set; }
        public string Class { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrEmpty(CurrentPath) || !CurrentPage.HasValue || !PagesCount.HasValue
                || (CurrentPage.Value == 1 && PagesCount.Value == 1))
            {
                output.SuppressOutput();
                return;
            }
            // Очищаем строку запросов от page=...
            string queryString = Context.HttpContext.Request.QueryString.ToString();
            Regex regex = new Regex("&?page=.*", RegexOptions.IgnoreCase);
            var matches = regex.Matches(queryString);
            foreach(var match in matches as IEnumerable<Match>)
            {
                int index = match.Value.IndexOf("&", 1);
                if (index >= 0)
                    queryString = queryString.Replace(match.Value.Substring(0, index + 1), string.Empty);
                else queryString = queryString.Replace(match.Value, string.Empty);
            }
            if (queryString.Length == 1 && queryString[0].Equals('?'))
                queryString = string.Empty;

            output.TagName = "ul";
            if (!string.IsNullOrEmpty(Class))
                output.Attributes.SetAttribute("class", Class);
            else if (!string.IsNullOrEmpty(Style))
                output.Attributes.SetAttribute("style", Style);
            // Будем начинать отрисовку с самой левой кнопки и идти постепенно направо
            int currentOperation = CurrentPage.Value - 2;
            // Если текущая операция == 2, то рисуем стрелки с ссылкой на первую страницу
            StringBuilder tagContentBuilder = new StringBuilder();
            if (currentOperation >= 2)
                tagContentBuilder.Append($"<li><a href=\"{CurrentPath}{queryString}\">«</a></li>");
            if (currentOperation < 1)
                currentOperation = 1;
            // Рисуем все возможные страницы
            for (; currentOperation <= (PagesCount < CurrentPage.Value + 2 ? PagesCount : CurrentPage.Value + 2); ++currentOperation)
            {
                if (currentOperation == CurrentPage.Value)
                    tagContentBuilder.Append($"<li><span>{currentOperation}</span></li>");
                else
                    tagContentBuilder.Append($"<li><a href=\"{CurrentPath}{(string.IsNullOrEmpty(queryString) ? "?" : $"{queryString}&")}page={currentOperation}\">{currentOperation}</a></li>");
            }
            // Рисуем стрелки на последнюю страницу, если необходимо
            if (CurrentPage.Value + 2 < PagesCount.Value)
                tagContentBuilder.Append($"<li><a href=\"{CurrentPath}{(string.IsNullOrEmpty(queryString) ? "?" : $"{queryString}&")}page={PagesCount.Value}\">»</a></li>");
            output.Content.AppendHtml(tagContentBuilder.ToString());
        }
    }
}