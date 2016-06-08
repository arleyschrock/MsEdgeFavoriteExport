using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Diagnostics.Trace;

namespace MsEdgeFavoriteExport
{
    class Program
    {
        static void Main(string[] args)
        {
            Listeners.Add(new System.Diagnostics.TextWriterTraceListener(Console.Out));
            var packageDir = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages"));
            if (packageDir.Exists)
            {
                try
                {
                    ProcessEdge(packageDir);
                }
                catch (Exception e)
                {
                    WriteLine(e.Message);
                }
            }
            else
            {
                WriteLine("Unable to locate package directory. Are you running a modern version of Windows?");
            }

#if DEBUG
            Console.ReadKey();
#endif
        }

        private static void ProcessEdge(DirectoryInfo packageDir)
        {
            var edge = packageDir.GetDirectories("Microsoft.MicrosoftEdge*").Single();
            var roaming = edge.GetDirectories("RoamingState");
            if (roaming.Length == 0)
            {
                WriteLine("You must not be syncing favorites.");
            }
            else
            {
                var dir = roaming.Single();
                var favs = dir.GetFiles("*.json")
                    .Select(x => JsonConvert.DeserializeObject<FavoriteItem>(File.ReadAllText(x.FullName)))
                    .ToArray();

                var grouped = favs.GroupBy(x => x.ParentId);
                var root = grouped.FirstOrDefault(x => string.IsNullOrWhiteSpace(x.Key));
                foreach (var group in grouped.OrderBy(x => (x.Key ?? string.Empty).Length))
                {
                    foreach (var item in group.OrderBy(x => x.OrderNumber))
                    {
                        Save(item, favs);
                    }
                }

            }
        }

        public static string TargetRoot => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Favorites");
        private static string Q(string x) => Path.Combine(TargetRoot, x);
        private static string Q(params string[] x) => Path.Combine(x);
        private static void Save(FavoriteItem item, IEnumerable<FavoriteItem> items)
        {
            WriteLine($"Saving item '{item.Title}'");

            var dir = FolderLineage(items, item)
                .Reverse()
                .Aggregate(TargetRoot, (acc, x) => Q(acc, x.Title.Contains("Favorites_Bar") ? x.Title.Replace("_", " ").Trim():x.Title), x => x);

            WriteLine($"Deduced '{dir}' as directory...");

            if (!Directory.Exists(dir))
            {
                WriteLine($"Creating directory: '{dir}'");
                Directory.CreateDirectory(dir);
            }

            CreateShortcut(item, Path.Combine(dir, $"{Path.GetInvalidPathChars().Aggregate(item.Title, (title, x) => title.Replace(x, '_'), x => x)}.url"));
        }

        private static void CreateShortcut(FavoriteItem item, string saveLocation)
        {
            try
            {
                using (var writer = new StreamWriter(saveLocation))
                {
                    writer.WriteLine("[InternetShortcut]");
                    writer.WriteLine($"URL={item.URL}");
                    writer.Flush();
                }
            }
            catch (Exception e)
            {
                WriteLine($"Error while saving {item.Title}, '{item.URL}' to location: {saveLocation}\nThe error message was:\n{e.Message}");
            }
        }

        private static IEnumerable<FavoriteItem> FolderLineage(IEnumerable<FavoriteItem> source, FavoriteItem target)
        {
            target = source.FirstOrDefault(x => x.ParentId == target.ParentId && x.IsFolder);
            while (target != null && !string.IsNullOrWhiteSpace(target.ParentId))
            {
                yield return target;
                target = source.FirstOrDefault(x => x.ItemId == target.ParentId && x.IsFolder);
            }
        }
    }
}
