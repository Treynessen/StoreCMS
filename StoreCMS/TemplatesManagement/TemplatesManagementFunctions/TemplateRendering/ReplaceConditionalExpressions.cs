using System.Text;
using System.Text.RegularExpressions;

namespace Treynessen.TemplatesManagement
{
    public class ConditionalExpressionInfo
    {
        public Regex Regex { get; set; }
        public string LeftSide { get; set; }
        public string RightSide { get; set; }
        public string Checker { get; set; }
    }

    public static partial class TemplatesManagementFunctions
    {
        private static ConditionalExpressionInfo[] conditionalExpressions =
        {
            new ConditionalExpressionInfo{Regex = new Regex(@"\[Product:IfSpecial\(.*\)\]"), LeftSide = "[Product:IfSpecial(", RightSide=")]", Checker = "Model is ProductPage && (Model as ProductPage).SpecialProduct"},
            new ConditionalExpressionInfo{Regex = new Regex(@"\[Product:IfStock\(.*\)\]"), LeftSide = "[Product:IfStock(", RightSide=")]", Checker = "Model is ProductPage && (Model as ProductPage).Price > 0 && (Model as ProductPage).OldPrice > 0"}
        };

        private static void ReplaceConditionalExpressions(StringBuilder cshtmlContentBuilder)
        {
            string source = cshtmlContentBuilder.ToString();
            foreach (var ce in conditionalExpressions)
            {
                while (ce.Regex.IsMatch(source))
                {
                    string value = ce.Regex.Match(source).Value;
                    value = value.Substring(ce.LeftSide.Length, value.Length - ce.LeftSide.Length - ce.RightSide.Length);
                    string newValue = ReplaceConditionalExpressionValue(value);
                    source = source.Replace($"{ce.LeftSide}{value}{ce.RightSide}", "@{ if(" + $"{ce.Checker}" + ") { " + $"{newValue}" + " } }");
                    cshtmlContentBuilder.Replace($"{ce.LeftSide}{value}{ce.RightSide}", "@{ if(" + $"{ce.Checker}" + ") { " + $"{newValue}" + " } }");
                }
            }
        }

        private static string ReplaceConditionalExpressionValue(string value)
        {
            bool replaceConditionalExpression = false;
            foreach (var ce in conditionalExpressions)
            {
                if (ce.Regex.IsMatch(value))
                {
                    replaceConditionalExpression = true;
                    break;
                }
            }
            bool replaceReplacementCollectionValues = false;
            if (!replaceConditionalExpression)
            {
                foreach (var r in replacements)
                {
                    if (value.Contains(r.Key))
                    {
                        replaceReplacementCollectionValues = true;
                        break;
                    }
                }
            }
            // Условные конструкции и дополнительный контент
            if (replaceConditionalExpression)
            {
                foreach (var ce in conditionalExpressions)
                {
                    if (ce.Regex.IsMatch(value))
                    {
                        // Если значение конструкции содержит условное выражение, то разделяем это значение на 3 части:
                        // 1 - левая часть до условного выражения
                        // 2 - значение в скобках условного выражения
                        // 3 - правая часть после условного выражения
                        int leftSideIndex = value.IndexOf(ce.LeftSide);
                        int rightSideIndex = value.IndexOf(ce.RightSide, leftSideIndex);
                        string leftSubvalue = value.Substring(0, leftSideIndex);
                        // Центральное подзначение - это и есть сама условная конструкция
                        // Нам нужна не вся условная конструкция, а только значение в скобках
                        string middleSubvalue = value.Substring(leftSideIndex + ce.LeftSide.Length, rightSideIndex - leftSideIndex - ce.LeftSide.Length);
                        string rightSubvalue = null;
                        if (rightSideIndex + ce.RightSide.Length < value.Length)
                            rightSubvalue = value.Substring(rightSideIndex + ce.RightSide.Length);
                        if (!string.IsNullOrEmpty(leftSubvalue))
                            leftSubvalue = ReplaceConditionalExpressionValue(leftSubvalue);
                        middleSubvalue = ReplaceConditionalExpressionValue(middleSubvalue);
                        middleSubvalue = " if" + $" ({ce.Checker}) " + "{ " + middleSubvalue + " } ";
                        if (!string.IsNullOrEmpty(rightSubvalue))
                            rightSubvalue = ReplaceConditionalExpressionValue(rightSubvalue);
                        return leftSubvalue + middleSubvalue + rightSubvalue;
                    }
                }
            }
            // Обычные замены из коллекции
            else if (replaceReplacementCollectionValues)
            {
                foreach (var r in replacements)
                {
                    if (value.Contains(r.Key))
                    {
                        // Тоже самое, как и в предыдущем условии
                        int leftSideIndex = value.IndexOf(r.Key);
                        int rightSideIndex = leftSideIndex + r.Key.Length;
                        string leftSubvalue = value.Substring(0, leftSideIndex);
                        string middleSubvalue = r.Value;
                        if (middleSubvalue.StartsWith("@{"))
                            middleSubvalue = middleSubvalue.Substring(2);
                        if (middleSubvalue.EndsWith(" }"))
                            middleSubvalue = middleSubvalue.Substring(0, middleSubvalue.Length - 2);
                        string rightSubvalue = value.Substring(rightSideIndex);
                        if (!string.IsNullOrEmpty(leftSubvalue))
                            leftSubvalue = ReplaceConditionalExpressionValue(leftSubvalue);
                        if (!string.IsNullOrEmpty(rightSubvalue))
                            rightSubvalue = ReplaceConditionalExpressionValue(rightSubvalue);
                        return leftSubvalue + middleSubvalue + rightSubvalue;
                    }
                }
            }
            // Тег хелперы

            // Текст
            return $"@Html.Raw(\"{value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("@@", "@")}\")";
        }
    }
}