using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Database.Queries
{
    public sealed class SearchLimitException : Exception { }
    public static class ProductSearch
    {
        public const int MaxRules = 256, MaxVariants = 64, MaxLength = 512;
        public static string Fold(CMSDatabase db, string value) =>
            db.Database.SqlQuery<string>($"SELECT lower({value} COLLATE \"default\") AS \"Value\"").Single();

        public static IQueryable<SynonymForString> MatchingPair(CMSDatabase db, string left, string right)
        {
            string a = Fold(db, left), b = Fold(db, right);
            return db.SynonymsForStrings.Where(s =>
                (EF.Functions.Collate(s.String, "default").ToLower() == a && EF.Functions.Collate(s.Synonym, "default").ToLower() == b)
                || (EF.Functions.Collate(s.String, "default").ToLower() == b && EF.Functions.Collate(s.Synonym, "default").ToLower() == a));
        }

        public static IQueryable<ProductPage> Query(CMSDatabase db, string text)
        {
            if (text.Length > MaxLength) throw new SearchLimitException();
            string folded = Fold(db, text);
            var rules = db.SynonymsForStrings.AsNoTracking().OrderBy(s => s.ID).Take(MaxRules + 1)
                .Select(s => new { Left = EF.Functions.Collate(s.String, "default").ToLower(), Right = EF.Functions.Collate(s.Synonym, "default").ToLower() }).ToArray();
            if (rules.Length > MaxRules) throw new SearchLimitException();
            var seen = new HashSet<string>(StringComparer.Ordinal) { folded };
            var pending = new Queue<string>();
            pending.Enqueue(folded);
            while (pending.Count > 0)
            {
                string current = pending.Dequeue();
                foreach (var rule in rules)
                    foreach (var direction in new[] { (rule.Left, rule.Right), (rule.Right, rule.Left) })
                    {
                        if (string.IsNullOrWhiteSpace(direction.Item1) || string.IsNullOrWhiteSpace(direction.Item2)) continue;
                        for (int start = 0; start <= current.Length - direction.Item1.Length; start++)
                        {
                            int end = start + direction.Item1.Length;
                            if ((start > 0 && !char.IsWhiteSpace(current[start - 1])) || (end < current.Length && !char.IsWhiteSpace(current[end]))
                                || !current.AsSpan(start, direction.Item1.Length).SequenceEqual(direction.Item1.AsSpan())) continue;
                            string next = current.Substring(0, start) + direction.Item2 + current.Substring(end);
                            if (next.Length > MaxLength) throw new SearchLimitException();
                            if (seen.Add(next))
                            {
                                if (seen.Count > MaxVariants) throw new SearchLimitException();
                                pending.Enqueue(next);
                            }
                        }
                    }
            }
            // [Комментарий ИИ-агента]: Один серверный OR сохраняет уникальность товаров; слова варианта соединяются через AND.
            var product = Expression.Parameter(typeof(ProductPage), "product");
            Expression alternatives = Expression.Constant(false);
            foreach (string variant in seen.OrderBy(v => v, StringComparer.Ordinal))
            {
                string[] words = variant.Split((char[])null, StringSplitOptions.RemoveEmptyEntries).Distinct(StringComparer.Ordinal).ToArray();
                if (words.Length == 0) continue;
                Expression allWords = Expression.Constant(true);
                foreach (string word in words)
                {
                    string term = word;
                    Expression<Func<ProductPage, bool>> match = p => EF.Functions.Collate(p.PageName, "default").ToLower().Contains(term);
                    allWords = Expression.AndAlso(allWords, new ReplaceParameter(match.Parameters[0], product).Visit(match.Body));
                }
                alternatives = Expression.OrElse(alternatives, allWords);
            }
            return db.ProductPages.AsNoTracking().Where(p => p.Published).Where(Expression.Lambda<Func<ProductPage, bool>>(alternatives, product));
        }
        private sealed class ReplaceParameter : ExpressionVisitor
        {
            private readonly ParameterExpression source, target;
            public ReplaceParameter(ParameterExpression source, ParameterExpression target) { this.source = source; this.target = target; }
            protected override Expression VisitParameter(ParameterExpression node) => node == source ? target : node;
        }
    }
}
