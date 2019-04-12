using System;
using System.Text;
using System.Collections.Generic;

namespace Treynessen.OtherTypes
{
    public class Translit
    {
        private string availableSymbols = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";

        private List<string> ruIntoEng = new List<string>
        {
            "A", "B", "V", "G", "D", "E", "Yo", "Zh", "Z", "I",
            "J", "K", "L", "M", "N", "O", "P", "R", "S", "T",
            "U", "F", "H", "C", "Ch", "Sh", "Sch", "", "Y", "",
            "Eh", "Yu", "Ya"
        };

        public string Translate(char ruSymbol)
        {
            int index = availableSymbols.IndexOf(char.ToUpper(ruSymbol));
            if (index == -1)
                throw new ArgumentException();
            if (ruSymbol == char.ToLower(ruSymbol))
                return ruIntoEng[index].ToLower();
            return ruIntoEng[index];
        }

        public string Translate(string ruWord)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var symbol in ruWord)
                builder.Append(Translate(symbol));
            return builder.ToString();
        }
    }
}
