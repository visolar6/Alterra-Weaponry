namespace VELD.AlterraWeaponry.Utilities
{
    namespace LocalizationHandler
    {
        [XmlRoot("LocalizationPackages")]
        public class LocalizationPackages
        {
            [XmlElement("LocalizationPackage")]
            public LocalizationPackage[] Localizations;
        }

        /// <summary>
        /// LocalizationPackage is a Localization Package ref - 
        /// </summary>
        public class LocalizationPackage
        {
            [XmlAttribute("Lang")]
            public string Lang;

            [XmlElement("Text")]
            public Text[] Texts;
        }

        public class Text
        {
            [XmlAttribute]
            public string key;
            [XmlText]
            public string value;
        }
    }

    public class LanguagesHandler
    {
        private static string ModPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static string filename = "Localizations.xml";
        public static void LanguagePatch()
        {
            XmlSerializer serializer = new(typeof(LocalizationHandler.LocalizationPackages));

            FileStream fs = new(Path.Combine(ModPath, filename), FileMode.Open, FileAccess.Read);
            LocalizationHandler.LocalizationPackages lps;

            lps = (LocalizationHandler.LocalizationPackages)serializer.Deserialize(fs);

            foreach (LocalizationHandler.Text text in lps.Localizations.Single(lp => lps.Localizations.Any(lp1 => lp1.Lang == Language.main.GetCurrentLanguage()) ? lp.Lang == Language.main.GetCurrentLanguage() : lp.Lang == Language.defaultLanguage).Texts)
            {
                if (Language.main.Get(text.key) != null)
                {
                    LanguageHandler.SetLanguageLine(text.key, text.value);
                }
            }
        }

        public static void GlobalPatch()
        {
            XmlSerializer serializer = new(typeof(LocalizationHandler.LocalizationPackages));

            FileStream fs = new(Path.Combine(ModPath, filename), FileMode.Open, FileAccess.Read);
            LocalizationHandler.LocalizationPackages lps;

            lps = (LocalizationHandler.LocalizationPackages)serializer.Deserialize(fs);

            foreach (LocalizationHandler.LocalizationPackage locpack in lps.Localizations)
            {
                foreach (LocalizationHandler.Text text in locpack.Texts)
                {
                    if (string.IsNullOrEmpty(text.value))
                        continue;

                    LanguageHandler.SetLanguageLine(text.key, text.value, locpack.Lang);
                }
            }
        }
    }
}
