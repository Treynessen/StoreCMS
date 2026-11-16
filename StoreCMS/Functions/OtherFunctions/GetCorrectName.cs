using System;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Translit;

namespace Treynessen.Functions
{
    public static partial class OtherFunctions
    {
        public static string GetCorrectName(string basis, HttpContext context)
        {
            if (string.IsNullOrEmpty(basis))
                return null;

            EnRuTranslit translit = context.RequestServices.GetService<EnRuTranslit>();

            StringBuilder builder = new StringBuilder();
            string availableSymbols = "qwertyuiopasdfghjklzxcvbnm1234567890-_";
            foreach (var symbol in basis)
            {
                if (!availableSymbols.Contains(char.ToLower(symbol)))
                {
                    if (symbol.Equals(' ') || symbol.Equals(',') || symbol.Equals('(') || symbol.Equals(')'))
                        builder.Append('_');
                    else if (symbol.Equals('/') || symbol.Equals('\\') || symbol.Equals('.') || symbol.Equals(':'))
                        builder.Append('-');
                    else
                    {
                        try
                        {
                            builder.Append(translit.Translit(symbol));
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