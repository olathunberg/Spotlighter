using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

namespace Spotlighter
{
    public static class Program
    {
        const string SPOTLIGHT_PATH = @"Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets";

        static void Main(string[] args)
        {
            Console.CursorVisible = false;
            var source = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                SPOTLIGHT_PATH);
            var destination = args != null && args.Length == 1 ? args[0] : Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            var horizDest = Path.Combine(destination, @"Spotlighter\Landscape");
            var vertDest = Path.Combine(destination, @"Spotlighter\Vertical");

            if (!Directory.Exists(horizDest))
                Directory.CreateDirectory(horizDest);
            if (!Directory.Exists(vertDest))
                Directory.CreateDirectory(vertDest);

            var destHashes = ComputeHashes(new string[] { horizDest, vertDest });

            Console.WriteLine("");
            Console.WriteLine($"Copying files from '{source}'...");
            Console.WriteLine($"{Directory.GetFiles(source).Length} files found");
            int count = 0;
            foreach (var file in Directory.EnumerateFiles(source))
            {
                Console.Write(".");
                try
                {
                    var fInfo = new FileInfo(file);

                    var sourceHash = CalculateMD5(file);
                    if (destHashes.Contains(sourceHash))
                        continue;
                    destHashes.Add(sourceHash);

                    // Handle screen based size
                    if (fInfo.Length > 400 * 1024)
                    {
                        using (var image = System.Drawing.Image.FromFile(file))
                        {
                            if (image.Width > image.Height)
                                File.Copy(file, Path.Combine(horizDest, Path.ChangeExtension(Path.GetFileName(file), "jpg")));
                            else
                                File.Copy(file, Path.Combine(vertDest, Path.ChangeExtension(Path.GetFileName(file), "jpg")));
                        }

                        count++;
                    }
                }
                catch (IOException ex) when (ex.HResult == -2147024816)
                {
                    // File already exists
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            Console.WriteLine();
            Console.WriteLine($"Done, copied {count} new images");

            Thread.Sleep(5000);

            Console.CursorVisible = true;
        }

        private static List<string> ComputeHashes(string[] destPaths)
        {
            var counter = 0;
            var percentComplete = 0;
            var destHashes = new List<string>();

            var numDestinationfiles = destPaths.Sum(x => Directory.GetFiles(x).Length);
            Console.WriteLine($"Computing destination hashes: {numDestinationfiles} files");

            foreach (var destPath in destPaths)
            {
                foreach (var file in Directory.EnumerateFiles(destPath))
                {
                    destHashes.Add(CalculateMD5(file));
                    counter++;
                    var newPercentComplete = 100 - (int)(((numDestinationfiles - counter) / (float)numDestinationfiles) * 100);

                    if (newPercentComplete != percentComplete)
                    {
                        percentComplete = newPercentComplete;
                        Console.CursorLeft = 0;
                        Console.Write($"{percentComplete}%");
                    }
                }
            }

            return destHashes;
        }

        private static string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}
