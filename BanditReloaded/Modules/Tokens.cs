using R2API;
using UnityEngine;
using System.Collections.Generic;
// using Zio;
// Zio.FileSystems;
using System.IO;
using System.Reflection;

namespace BanditReloaded.Modules
{
    public static class Tokens
    {
        //public static SubFileSystem fileSystem;
        public static void RegisterLanguageTokens()
        {
            //TODO: Replace this with the proper way of loading languages once someone figures that out.
            string languageFileName = "BanditReloaded.txt";
            string pathToLanguage = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\language";
            LanguageAPI.AddPath(System.IO.Path.Combine(pathToLanguage + @"\en", languageFileName));
            //LanguageAPI.AddPath(System.IO.Path.Combine(pathToLanguage + @"\es-419", languageFileName));
            //LanguageAPI.AddPath(System.IO.Path.Combine(pathToLanguage + @"\RU", languageFileName));
        }
    }
}