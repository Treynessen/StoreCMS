﻿namespace Treynessen.Localization
{
    public interface IAdminPanelPageLocalization
    {
        string Title { get; }

        string MainPage { get; }
        string Pages { get; }
        string CategoriesAndProducts { get; }
        string Redirections { get; }
        string Templates { get; }
        string Chunks { get; }
        string FileManager { get; }
        string Users { get; }
        string UserTypes { get; }
        string SynonymsForStrings { get; }
        string UserProfile { get; }
        string Settings { get; }
        string Exit { get; }
    }
}