﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Linq;
using System.Xml;
using System.Net;
using BepInEx;

namespace RogueLibsCore
{
    /// <summary>
    ///   <para>Provides information about, and means to manipulate in-game localization.</para>
    /// </summary>
    public static class Localization
    {
        private static int init;
        internal static void Init()
        {
            if (Interlocked.Exchange(ref init, 1) == 1) return;
            localePath = Path.Combine(Paths.BepInExRootPath, "locale");
            Directory.CreateDirectory(localePath);

            if (PermitDownload() && DownloadIndex())
                UpdateVanillaLanguages();

            InitializeLanguages();
        }

        private static bool PermitDownload()
        {
            string lastAccessFile = Path.Combine(localePath, ".lastaccess");
            if (!File.Exists(lastAccessFile)) return true;
            try
            {
                string text = File.ReadAllText(lastAccessFile);
                DateTime lastAccess = DateTime.Parse(text, CultureInfo.InvariantCulture);
                return DateTime.Now - lastAccess > new TimeSpan(1, 0, 0);
            }
            catch
            {
                RogueFramework.LogError("Could not read the locale/.lastaccess file.");
                return true;
            }
        }
        private static LanguageVersions? Versions;
        private static bool DownloadIndex()
        {
            const string url = "https://raw.githubusercontent.com/Chasmical/RogueLibs/main/RogueLibsCore/Resources/locale.index.xml";
            string downloadPath = Path.Combine(localePath, ".vanilla.index.xml");
            try
            {
                WebClient web = new WebClient();
                web.DownloadFile(url, downloadPath);

                using (FileStream stream = new FileStream(downloadPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (XmlReader reader = XmlReader.Create(stream))
                {
                    Versions = new LanguageVersions();
                    Versions.ReadXml(reader);
                }

                string lastAccessFile = Path.Combine(localePath, ".lastaccess");
                File.WriteAllText(lastAccessFile, DateTime.Now.ToString(CultureInfo.InvariantCulture));

                return true;
            }
            catch (Exception e)
            {
                RogueFramework.LogError("Error in DownloadIndex!");
                RogueFramework.LogError(e.ToString());
                return false;
            }
            finally
            {
                File.Delete(downloadPath);
            }
        }

        private static void UpdateVanillaLanguages()
        {
            string vanillaLanguagesPath = Path.Combine(localePath, "vanilla");
            Directory.CreateDirectory(vanillaLanguagesPath);

            if (Versions is null) return;
            foreach (KeyValuePair<string, int> entry in Versions.Entries.ToList())
            {
                string id = entry.Key;
                int latest = entry.Value;

                string path = Path.Combine(vanillaLanguagesPath, $"{id}.xml");
                if (!File.Exists(path)) UpdateLanguage(id, path, latest);
                else
                {
                    try
                    {
                        int current;
                        using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                        using (XmlReader reader = XmlReader.Create(stream))
                            current = ReadPrefix(reader);

                        if (current < latest) UpdateLanguage(id, path, latest);
                    }
                    catch (Exception e)
                    {
                        RogueFramework.LogError($"Something's wrong with {id}.xml");
                        RogueFramework.LogError(e.ToString());
                    }
                }
            }
        }
        private static int ReadPrefix(XmlReader reader)
        {
            reader.MoveToContent();
            string? versionAttr = reader.GetAttribute("Version");
            return versionAttr is not null && int.TryParse(versionAttr, out int version) ? version : -1;
        }
        private static void UpdateLanguage(string language, string filePath, int newVersion)
        {
            string url = $"https://raw.githubusercontent.com/Chasmical/RogueLibs/main/RogueLibsCore/Resources/locale.{language}.xml";
            string downloadPath = filePath + ".download";
            WebClient web = new WebClient();
            try
            {
                web.DownloadFile(url, downloadPath);
                File.Delete(filePath);
                File.Move(downloadPath, filePath);

                Versions!.Entries[language] = newVersion;
            }
            finally
            {
                File.Delete(downloadPath);
            }
        }

        private static string localePath = null!; // initialized in Init()

        private static void InitializeLanguages()
        {
            ReInitializeLanguages();
            LanguageService.OnCurrentChanged += static e => Current = ReloadLanguage(CurrentWatcher = SetupWatcher(e.Value));
            LanguageService.OnFallBackChanged += static e => FallBack = ReloadLanguage(FallBackWatcher = SetupWatcher(e.Value));
        }
        private static void ReInitializeLanguages()
        {
            CurrentWatcher = SetupWatcher(LanguageService.Current);
            FallBackWatcher = SetupWatcher(LanguageService.FallBack);

            ReloadCurrent(null, null);
            ReloadFallBack(null, null);
        }
        private static FileSystemWatcher? SetupWatcher(LanguageCode code)
        {
            string? name = LanguageService.GetLanguageName(code);
            string path = localePath;

            if (!File.Exists(Path.Combine(path, name + ".xml"))
                && LanguageService.Current is >= LanguageCode.English and <= LanguageCode.Korean)
            {
                path = Path.Combine(localePath, "vanilla");
                if (!File.Exists(Path.Combine(path, name + ".xml")))
                {
                    RogueFramework.LogError("Could not find the vanilla language file.");
                    return null;
                }
            }
            return new FileSystemWatcher(path)
            {
                Filter = name + ".xml",
                EnableRaisingEvents = true,
            };
        }

        private static LocaleLanguage? ReloadLanguage(FileSystemWatcher? watcher)
            => watcher is null ? null : ReloadLanguage(watcher.Path, watcher.Filter);
        private static LocaleLanguage? ReloadLanguage(string directory, string name)
        {
            string path = Path.Combine(directory, name);
            if (!File.Exists(path)) return null;
            try
            {
                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (XmlReader reader = XmlReader.Create(stream))
                {
                    LocaleLanguage language = new LocaleLanguage();
                    language.ReadXml(reader);
                    return language;
                }
            }
            catch
            {
                RogueFramework.LogError($"Error reading from {path}.");
                return null;
            }
        }

        private static FileSystemWatcher? currentWatcher;
        private static FileSystemWatcher? CurrentWatcher
        {
            get => currentWatcher;
            set
            {
                if (currentWatcher == value) return;
                if (currentWatcher is not null)
                {
                    currentWatcher.Created -= ReloadCurrent;
                    currentWatcher.Changed -= ReloadCurrent;
                    currentWatcher.Renamed -= ReloadCurrent;
                    currentWatcher.Deleted -= ReloadCurrent;
                }
                currentWatcher = value;
                if (value is not null)
                {
                    value.Created += ReloadCurrent;
                    value.Changed += ReloadCurrent;
                    value.Renamed += ReloadCurrent;
                    value.Deleted += ReloadCurrent;
                }
            }
        }
        private static void ReloadCurrent(object? _, object? __) => Current = ReloadLanguage(CurrentWatcher);

        private static FileSystemWatcher? fallBackWatcher;
        private static FileSystemWatcher? FallBackWatcher
        {
            get => fallBackWatcher;
            set
            {
                if (fallBackWatcher == value) return;
                if (fallBackWatcher is not null)
                {
                    fallBackWatcher.Created -= ReloadFallBack;
                    fallBackWatcher.Changed -= ReloadFallBack;
                    fallBackWatcher.Renamed -= ReloadFallBack;
                    fallBackWatcher.Deleted -= ReloadFallBack;
                }
                fallBackWatcher = value;
                if (value is not null)
                {
                    value.Created += ReloadFallBack;
                    value.Changed += ReloadFallBack;
                    value.Renamed += ReloadFallBack;
                    value.Deleted += ReloadFallBack;
                }
            }
        }
        private static void ReloadFallBack(object? _, object? __) => FallBack = ReloadLanguage(FallBackWatcher);

        /// <summary>
        ///   <para>Gets the current language's information.</para>
        /// </summary>
        public static LocaleLanguage? Current { get; private set; }
        /// <summary>
        ///   <para>Gets the fall-back language's information.</para>
        /// </summary>
        public static LocaleLanguage? FallBack { get; private set; }

        /// <summary>
        ///   <para>Returns the value of an entry with the specified <paramref name="name"/> and <paramref name="type"/> in the current or fall-back language.</para>
        /// </summary>
        /// <param name="name">The name of the entry to look for.</param>
        /// <param name="type">The type of the entry to look for.</param>
        /// <returns>The value of an entry with the specified <paramref name="name"/> and <paramref name="type"/> in the current or fall-back language, if found; otherwise, <see langword="null"/>.</returns>
        public static string? GetName(string? name, string type)
        {
            if (name is null) return null;
            return Current?[type, name] ?? FallBack?[type, name];
        }
    }
}
