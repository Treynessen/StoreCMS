namespace Treynessen.TemplatesManagement
{
    public static partial class TemplatesManagementFunctions
    {
        private static (int, int) GetNearestConditionalExpressionIndexes(string source, out ConditionalExpressionInfo conditionalExpressionInfo)
        {
            conditionalExpressionInfo = null;
            int beginIndex = -1, endIndex = -1;
            // Находим первую конструкцию {ce.LeftSide}...значение...{ce.RightSide}
            foreach (var ce in conditionalExpressions)
            {
                int tempBeginIndex = source.IndexOf(ce.LeftSide);
                if (tempBeginIndex >= 0)
                {
                    int tempEndIndex = source.IndexOf(ce.RightSide, tempBeginIndex);
                    if (tempEndIndex >= 0)
                    {
                        // Если это первая итерация или найдена конструкция, находящаяся перед 
                        // текущей конструкцией
                        if (beginIndex < 0 || tempBeginIndex < beginIndex)
                        {
                            beginIndex = tempBeginIndex;
                            endIndex = tempEndIndex;
                            conditionalExpressionInfo = ce;
                        }
                    }
                }
            }
            if (beginIndex >= 0 && endIndex > 0)
            {
                // Ищем условные конструкции внутри нашей условной конструкции
                // Если таковые имеются и закрывающая стороная совпадает с нашей конструкцией, 
                // то присваиваем endIndex следующую закрывающую сторону
                bool wasOffset = false;
                int lastTempBeginIndex = beginIndex + conditionalExpressionInfo.LeftSide.Length;
                do
                {
                    wasOffset = false;
                    // Ищем самую первую условную конструкцию в интервале от [lasTempBeginIndex; endIndex]
                    // Если таковая имеется, тогда присваиваем endIndex индекс следующей закрывающей стороны
                    int? tempBeginIndex = null;
                    foreach (var ce in conditionalExpressions)
                    {
                        int temp = source.IndexOf(ce.LeftSide, lastTempBeginIndex);
                        if (temp > 0)
                        {
                            temp += ce.LeftSide.Length;
                            if (!tempBeginIndex.HasValue && temp <= endIndex)
                                tempBeginIndex = temp;
                            else if (temp <= endIndex && temp < tempBeginIndex)
                                tempBeginIndex = temp;
                        }
                    }
                    if (tempBeginIndex.HasValue)
                    {
                        lastTempBeginIndex = tempBeginIndex.Value;
                        int tempEndIndex = source.IndexOf(conditionalExpressionInfo.RightSide, endIndex + conditionalExpressionInfo.RightSide.Length);
                        if (tempEndIndex > 0)
                        {
                            endIndex = tempEndIndex;
                            wasOffset = true;
                        }
                    }
                } while (wasOffset);
            }
            return (beginIndex, endIndex);
        }
    }
}