using System;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.OtherTypes;

namespace Treynessen.Functions
{
    public static partial class OtherFunctions
    {
        public static string GetCorrectAliasName(string basis, HttpContext context)
        {
            if (string.IsNullOrEmpty(basis))
                return null;

            Translit translit = context.RequestServices.GetService<Translit>();

            StringBuilder builder = new StringBuilder();
            string availableSymbols = "qwertyuiopasdfghjklzxcvbnm1234567890-_";
            foreach (var symbol in basis)
            {
                if (!availableSymbols.Contains(char.ToLower(symbol)))
                {
                    if (symbol == ' ')
                        builder.Append('_');
                    else
                    {
                        try
                        {
                            builder.Append(translit.Translate(symbol));
                        }
                        catch (ArgumentException)
                        {
                            continue;
                        }
                    }
                }
                else builder.Append(symbol);
            }
            if (builder.Length == 0)
                return null;
            return builder.ToString().ToLower();
        }
    }
}