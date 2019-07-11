using System;

namespace Treynessen.TemplatesManagement
{
    public static partial class TemplatesManagementFunctions
    {
        private static string[] tagHelpers = { "image", "cost-sort", "page-buttons" };

        private static string ReplaceConditionalExpressionValue(string value)
        {
            bool replaceConditionalExpression = false;
            int beginIndex = -1;
            int endIndex = -1;
            (beginIndex, endIndex) = GetNearestConditionalExpressionIndexes(value, out ConditionalExpressionInfo currentConditionalExpression);
            if (beginIndex >= 0 && endIndex > 0)
                replaceConditionalExpression = true;

            bool replaceTagHelpers = false;
            int tagHelperLeftIndex = -1, tagHelperRightIndex = -1;
            if (!replaceConditionalExpression)
            {
                foreach (var th in tagHelpers)
                {
                    tagHelperLeftIndex = value.IndexOf($"<{th}", StringComparison.InvariantCultureIgnoreCase);
                    if (tagHelperLeftIndex >= 0)
                        tagHelperRightIndex = value.IndexOf($"</{th}>", tagHelperLeftIndex, StringComparison.InvariantCultureIgnoreCase) + $"</{th}>".Length;
                    if (tagHelperLeftIndex >= 0 && tagHelperRightIndex > 0)
                    {
                        replaceTagHelpers = true;
                        break;
                    }
                }
            }

            bool replaceReplacementCollectionValues = false;
            if (!replaceConditionalExpression && !replaceTagHelpers)
            {
                foreach (var r in insertionReplacements)
                {
                    if (value.Contains(r.Insertion))
                    {
                        replaceReplacementCollectionValues = true;
                        break;
                    }
                }
            }
            // Условные конструкции и дополнительный контент
            if (replaceConditionalExpression)
            {
                // Если значение конструкции содержит условное выражение, то разделяем это значение на 3 части:
                // 1 - левая часть до условного выражения
                // 2 - значение в скобках условного выражения
                // 3 - правая часть после условного выражения
                string leftSubvalue = value.Substring(0, beginIndex);
                // Центральное подзначение - это и есть сама условная конструкция
                // Нам нужна не вся условная конструкция, а только значение в скобках
                // Получаем 3 части и делаем рендер каждой
                string middleSubvalue = value.Substring(beginIndex + currentConditionalExpression.LeftSide.Length, endIndex - beginIndex - currentConditionalExpression.LeftSide.Length);
                string rightSubvalue = null;
                if (endIndex + currentConditionalExpression.RightSide.Length < value.Length)
                    rightSubvalue = value.Substring(endIndex + currentConditionalExpression.RightSide.Length);
                if (!string.IsNullOrEmpty(leftSubvalue))
                    leftSubvalue = ReplaceConditionalExpressionValue(leftSubvalue);
                middleSubvalue = ReplaceConditionalExpressionValue(middleSubvalue);
                middleSubvalue = " if" + $" ({currentConditionalExpression.Checker}) " + "{ " + middleSubvalue + " } ";
                if (!string.IsNullOrEmpty(rightSubvalue))
                    rightSubvalue = ReplaceConditionalExpressionValue(rightSubvalue);
                return leftSubvalue + middleSubvalue + rightSubvalue;
            }
            // Тег хелперы
            else if (replaceTagHelpers)
            {
                // Точно так же делим текущую строку на 3 части: до тег-хелпера; сам тег-хелпер; после тег-хелпера
                // Сам тег-хелпер не трогаем
                string leftSubvalue = value.Substring(0, tagHelperLeftIndex);
                string middleSubvalue = value.Substring(tagHelperLeftIndex, tagHelperRightIndex - tagHelperLeftIndex);
                string rightSubvalue = null;
                if (tagHelperRightIndex < value.Length)
                    rightSubvalue = value.Substring(tagHelperRightIndex);
                if (!string.IsNullOrEmpty(leftSubvalue))
                    leftSubvalue = ReplaceConditionalExpressionValue(leftSubvalue);
                if (!string.IsNullOrEmpty(rightSubvalue))
                    rightSubvalue = ReplaceConditionalExpressionValue(rightSubvalue);
                return leftSubvalue + middleSubvalue + rightSubvalue;
            }
            // Обычные замены из коллекции
            else if (replaceReplacementCollectionValues)
            {
                foreach (var r in insertionReplacements)
                {
                    if (value.Contains(r.Insertion))
                    {
                        // Тоже самое, как и в предыдущем условии
                        int leftSideIndex = value.IndexOf(r.Insertion);
                        int rightSideIndex = leftSideIndex + r.Insertion.Length;
                        string leftSubvalue = value.Substring(0, leftSideIndex);
                        string middleSubvalue = r.Replacement;
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
            // Текст
            return $"@Html.Raw(\"{value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("@@", "@")}\")";
        }
    }
}