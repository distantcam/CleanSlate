using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace CleanSlate
{
    public static class ThemeManager
    {
        public readonly static IList<string> StyleFiles = new List<string>();

        public static readonly DependencyProperty ThemeToneProperty =
            DependencyProperty.RegisterAttached("ThemeTone", typeof(string), typeof(ThemeManager),
            new UIPropertyMetadata("", OnThemeChanged));

        public static string GetThemeTone(DependencyObject obj)
        {
            return (string)obj.GetValue(ThemeToneProperty);
        }

        public static void SetThemeTone(DependencyObject obj, string value)
        {
            obj.SetValue(ThemeToneProperty, value);
        }

        public static readonly DependencyProperty AccentColorProperty =
            DependencyProperty.RegisterAttached("AccentColor", typeof(Color), typeof(ThemeManager),
            new UIPropertyMetadata(Colors.Red, OnThemeChanged));

        public static Color GetAccentColor(DependencyObject obj)
        {
            return (Color)obj.GetValue(AccentColorProperty);
        }

        public static void SetAccentColor(DependencyObject obj, Color value)
        {
            obj.SetValue(AccentColorProperty, value);
        }

        public static readonly DependencyProperty ChromeColorProperty =
            DependencyProperty.RegisterAttached("ChromeColor", typeof(Color), typeof(ThemeManager),
            new UIPropertyMetadata(Colors.Green, OnThemeChanged));

        public static Color GetChromeColor(DependencyObject obj)
        {
            return (Color)obj.GetValue(ChromeColorProperty);
        }

        public static void SetChromeColor(DependencyObject obj, Color value)
        {
            obj.SetValue(ChromeColorProperty, value);
        }

        private static void OnThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = d as FrameworkElement;
            if (element == null)
                return;

            ChangeTheme(element.Resources.MergedDictionaries, GetThemeTone(d), GetAccentColor(d), GetChromeColor(d));
        }

        private static ResourceDictionary GetThemeResourceDictionary(string theme)
        {
            if (String.IsNullOrEmpty(theme))
                return new ResourceDictionary();

            string packUri = String.Format(@"/Styles/{0}.xaml", theme);
            return Application.LoadComponent(new Uri(packUri, UriKind.Relative)) as ResourceDictionary;
        }

        private static ResourceDictionary CreateColorDictionary(string prefix, Color color, double fadePercent)
        {
            var dictionary = new SharedResourceDictionary();
            dictionary.Add(prefix + "1Color", color);

            var brush = new SolidColorBrush(color);
            brush.Freeze();

            dictionary.Add(prefix + "1Brush", brush);

            double hue;
            double saturation;
            double value;
            ColorToHSV(color, out hue, out saturation, out value);
            var faded = ColorFromHSV(hue, saturation * fadePercent, value);

            dictionary.Add(prefix + "2Color", faded);

            brush = new SolidColorBrush(faded);
            brush.Freeze();

            dictionary.Add(prefix + "2Brush", brush);

            return dictionary;
        }

        private static void ChangeTheme(Collection<ResourceDictionary> mergedDictionaries, string tone, Color accentColor, Color chromeColor)
        {
            var dictionaries = mergedDictionaries.OfType<SharedResourceDictionary>().ToArray();

            foreach (var d in dictionaries)
                mergedDictionaries.Remove(d);

            if (String.IsNullOrEmpty(tone))
                return;

            var toneDictionary = GetThemeResourceDictionary("Style" + tone);

            if (toneDictionary == null)
                return;

            var accentDictionary = CreateColorDictionary("CleanAccent", accentColor, 0.8);
            var chromeDictionary = CreateColorDictionary("CleanChrome", chromeColor, 0.8);

            mergedDictionaries.Add(toneDictionary);
            mergedDictionaries.Add(accentDictionary);
            mergedDictionaries.Add(chromeDictionary);

            foreach (var s in StyleFiles)
            {
                var dict = GetThemeResourceDictionary(s);
                dict.MergedDictionaries.Add(toneDictionary);
                dict.MergedDictionaries.Add(accentDictionary);
                dict.MergedDictionaries.Add(chromeDictionary);

                mergedDictionaries.Add(dict);
            }
        }

        private static void ColorToHSV(Color color, out double hue, out double saturation, out double value)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            hue = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B).GetHue();
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }

        private static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            byte v = Convert.ToByte(value);
            byte p = Convert.ToByte(value * (1 - saturation));
            byte q = Convert.ToByte(value * (1 - f * saturation));
            byte t = Convert.ToByte(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }
    }
}