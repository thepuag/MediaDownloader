using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Introduce la URL de YouTube:");
        string url = Console.ReadLine().Trim();

        string appDir = AppDomain.CurrentDomain.BaseDirectory;
        string ytdlpPath = Path.Combine(appDir, "yt-dlp.exe");

        // Descargar yt-dlp si no existe
        if (!File.Exists(ytdlpPath))
        {
            Console.WriteLine("Descargando yt-dlp...");
            await DownloadYtdlpAsync(ytdlpPath);
            Console.WriteLine("yt-dlp descargado correctamente!\n");
        }

        // Crear directorio de descargas
        string mediaDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Downloads",
            "Media"
        );
        Directory.CreateDirectory(mediaDir);

        // Configurar proceso de descarga
        var psi = new ProcessStartInfo
        {
            FileName = ytdlpPath,
            Arguments = $"-o \"{mediaDir}/%(title)s.%(ext)s\" -f \"bestvideo+bestaudio/best\" --newline \"{url}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using (var process = new Process { StartInfo = psi })
        {
            Console.WriteLine("\nIniciando descarga...");
            process.Start();

            // Leer la salida línea por línea
            string lastProgressLine = string.Empty;
            while (!process.StandardOutput.EndOfStream)
            {
                string line = process.StandardOutput.ReadLine();
                if (line != null)
                {
                    // Filtrar líneas de progreso (contienen porcentajes)
                    if (line.Contains("%") || line.Contains("ETA"))
                    {
                        // Borrar la última línea de progreso
                        if (!string.IsNullOrEmpty(lastProgressLine))
                        {
                            Console.SetCursorPosition(0, Console.CursorTop - 1);
                            Console.Write(new string(' ', Console.WindowWidth - 1));
                            Console.SetCursorPosition(0, Console.CursorTop);
                        }

                        Console.WriteLine(line);
                        lastProgressLine = line;
                    }
                    else
                    {
                        // Otras líneas (información, warnings, etc.)
                        Console.WriteLine(line);
                        lastProgressLine = string.Empty;
                    }
                }
            }

            await process.WaitForExitAsync();

            Console.WriteLine(process.ExitCode == 0
                ? "\nDescarga completada exitosamente!"
                : $"\nError durante la descarga (Código: {process.ExitCode})");
        }

        Console.WriteLine("\nPresiona cualquier tecla para salir...");
        Console.ReadKey();
    }

    private static async Task DownloadYtdlpAsync(string savePath)
    {
        using (var httpClient = new HttpClient())
        {
            var response = await httpClient.GetByteArrayAsync(
                "https://github.com/yt-dlp/yt-dlp/releases/download/2025.04.30/yt-dlp.exe"
            );
            await File.WriteAllBytesAsync(savePath, response);
        }
    }
}