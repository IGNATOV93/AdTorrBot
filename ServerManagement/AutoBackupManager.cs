using System.IO.Compression;
using AdTorrBotTorrserverBot.Torrserver;

namespace AdTorrBot.ServerManagement
{
    public abstract class AutoBackupManager
    {
        private static readonly string BotRootPath = AppDomain.CurrentDomain.BaseDirectory;
        
        public static readonly string BotAppDbPath = Path.Combine(BotRootPath, "app.db");
        public static readonly string BotSettingsPath = Path.Combine(BotRootPath, "settings.json");


        public static readonly string FilePathTorrserver = "/opt/torrserver/";
        public static readonly string TorrserverAccsDbPath = Path.Combine(FilePathTorrserver, "accs.db");
        public static readonly string TorrserverConfigDbPath = Path.Combine(FilePathTorrserver, "config.db");
        public static readonly string TorrserverSettingsPath = Path.Combine(FilePathTorrserver, "settings.json");
        public static readonly string TorrserverConfigPath = Path.Combine(FilePathTorrserver, "torrserver.config");

        private static readonly string[] FilesToBackup = {
        BotAppDbPath, BotSettingsPath,
        TorrserverAccsDbPath, TorrserverConfigDbPath,
        TorrserverSettingsPath, TorrserverConfigPath  };
        private static readonly string DefaultArchivePath = Path.Combine(BotRootPath, "backup.zip"); 
        public static string GetBackupMessage()
        {
            string backupMessage =
                "📂 *Автоматически резервируемые файлы:*\n\n" +
                "🗂️ *Файлы бота:*\n" +
                "- `app.db`\n" +
                "- `settings.json`\n\n" +
                "🗂️ *Файлы Torrserver:*\n" +
                "- `accs.db`\n" +
                "- `config.db`\n" +
                "- `settings.json`\n" +
                "- `torrserver.config`\n\n" +
                "✅ Эти файлы будут автоматически сохраняться в резервные копии.";
            return backupMessage;
        }
        public static string GetBackupArchivePath()
        {
            return DefaultArchivePath;
        }

        public static async Task<bool> CreateBackupArchive()
        {
            List<string> missingFiles = new List<string>();
            bool success = true;

            if (File.Exists(DefaultArchivePath)) File.Delete(DefaultArchivePath);
            await Torrserver.ControlTorrserver(false);

            try
            {
                using (var zip = ZipFile.Open(DefaultArchivePath, ZipArchiveMode.Create))
                {
                    foreach (string file in FilesToBackup)
                    {
                        if (File.Exists(file))
                        {
                            string fileToProcess = file;

                            string folderName = file.StartsWith(BotRootPath, StringComparison.OrdinalIgnoreCase)
                                ? "AdTorrBot"
                                : file.StartsWith(FilePathTorrserver, StringComparison.OrdinalIgnoreCase)
                                ? "torrserver"
                                : "Unknown";

                            string entryName = Path.Combine(folderName, Path.GetFileName(fileToProcess));

                            try
                            {
                                if (IsFileLocked(fileToProcess))
                                {
                                    Console.WriteLine($"⚠️ Файл {fileToProcess} заблокирован, создаю копию...");

                                    try
                                    {
                                        string tempFilePath = Path.Combine("/tmp", Path.GetFileName(fileToProcess));
                                        File.Copy(fileToProcess, tempFilePath, true);
                                        fileToProcess = tempFilePath;
                                    }
                                    catch (IOException ex)
                                    {
                                        Console.WriteLine($"❌ Ошибка при копировании файла {fileToProcess}: {ex.Message}");
                                        missingFiles.Add(fileToProcess);
                                        success = false;
                                        continue; 
                                    }
                                }
                                using (FileStream fs = new FileStream(fileToProcess, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                {
                                    var zipEntry = zip.CreateEntry(entryName);
                                    using (var entryStream = zipEntry.Open())
                                    {
                                        fs.CopyTo(entryStream);
                                    }
                                }

                                Console.WriteLine($"✅ Добавлен в архив: {fileToProcess}");
                            }
                            catch (IOException ex)
                            {
                                Console.WriteLine($"⚠️ Ошибка доступа к файлу {fileToProcess}: {ex.Message}");
                                missingFiles.Add(fileToProcess);
                                success = false;
                            }
                        }
                        else
                        {
                            missingFiles.Add(file);
                            success = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка при создании архива: {ex.Message}");
                success = false;
            }

            if (missingFiles.Count > 0)
            {
                Console.WriteLine("⚠️ Следующие файлы не найдены или недоступны:");
                foreach (string missingFile in missingFiles)
                {
                    Console.WriteLine($"   - {missingFile}");
                }
            }

            Console.WriteLine(success ? $"🎉 Архив успешно создан: {DefaultArchivePath}" : "❌ Архивирование завершилось с ошибками!");
            await Torrserver.ControlTorrserver(true);
            Console.WriteLine("🚀 Torrserver успешно запустился");

            return success;
        }


        /// <summary>
        /// Проверяет, заблокирован ли файл другим процессом.
        /// </summary>
        private static bool IsFileLocked(string filePath)
        {
            try
            {
                using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return false;
                }
            }
            catch (IOException)
            {
                return true;
            }
        }
    }
}
