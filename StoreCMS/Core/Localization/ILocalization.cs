﻿namespace Trane.Localization
{
    public interface ILocalization
    {
        // ДЛЯ СТРАНИЦЫ 404
        string PageNotFoundText { get; }

        // ДЛЯ ПАНЕЛИ АДМИНИСТРАТОРА
        string AdminPanelTitle { get; }
        string UserNameLabel { get; }
        string PasswordLabel { get; }
        string LoginButtonText { get; }
    }
}