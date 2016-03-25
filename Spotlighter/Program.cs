using System;
using System.IO;
using System.Threading;

namespace Spotlighter
{
    class Program
    {
        const string SPOTLIGHT_PATH = @"Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets";

        static void Main(string[] args)
        {
            var source = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                SPOTLIGHT_PATH);

            var destination = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            var horizDest = Path.Combine(destination, @"Spotlighter\Landscape");
            var vertDest = Path.Combine(destination, @"Spotlighter\Vertical");

            if (!Directory.Exists(horizDest))
                Directory.CreateDirectory(horizDest);
            if (!Directory.Exists(vertDest))
                Directory.CreateDirectory(vertDest);

            Console.WriteLine($"Copying files from '{source}'...");
            Console.WriteLine($"{Directory.GetFiles(source).Length} files found");
            int count = 0;
            foreach (var file in Directory.EnumerateFiles(source))
            {
                Console.Write(".");
                try
                {
                    var fInfo = new FileInfo(file);

                    // Handle screen based size
                    if (fInfo.Length > 400 * 1024)
                    {
                        var image = System.Drawing.Image.FromFile(file);
                        if (image.Width > image.Height)
                            File.Copy(file, Path.Combine(horizDest, Path.ChangeExtension(Path.GetFileName(file), "jpg")));
                        else
                            File.Copy(file, Path.Combine(vertDest, Path.ChangeExtension(Path.GetFileName(file), "jpg")));

                        count++;
                    }
                }
                catch (IOException)
                {
                    // File already exists
                }
            }

            Console.WriteLine();
            Console.WriteLine($"Done, copied {count} new images");

            Thread.Sleep(5000);
        }
    }
}
