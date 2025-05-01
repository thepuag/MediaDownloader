using System;
using System.IO;
using System.Net.Http;
using System.Diagnostics;
using System.Threading.Tasks;
using System.ComponentModel;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YouTubeDownloader
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            // Pedir URL de YouTube al usuario
            Console.Write("Introduce la URL de YouTube: ");
            var url = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(url))
            {
                Console.WriteLine("URL no válida.");
                return 1;
            }

            // Ruta de Descargas/Media
            var downloadsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads",
                "Media");
            Directory.CreateDirectory(downloadsPath);

            var youtube = new YoutubeClient();
            try
            {
                // Extraer VideoId de la URL
                var videoId = VideoId.Parse(url);

                // Obtener metadatos y streams
                var video = await youtube.Videos.GetAsync(videoId);
                var manifest = await youtube.Videos.Streams.GetManifestAsync(videoId);

                // Seleccionar el mejor video-only (la máxima resolución)
                var videoStreamInfo = manifest.GetVideoOnlyStreams()
                    .OrderByDescending(s => s.VideoQuality.MaxHeight)
                    .FirstOrDefault();
                // Seleccionar el mejor audio-only (mayor bitrate)
                var audioStreamInfo = manifest.GetAudioOnlyStreams()
                    .OrderByDescending(s => s.Bitrate)
                    .FirstOrDefault();

                if (videoStreamInfo == null || audioStreamInfo == null)
                {
                    Console.Error.WriteLine("No se encontraron streams de vídeo o audio adecuados.");
                    return 1;
                }

                // Preparar nombres y rutas
                var safeTitle = SanitizeFileName(video.Title);
                var videoFile = Path.Combine(downloadsPath, $"{safeTitle}.{videoStreamInfo.Container}");
                var audioFile = Path.Combine(downloadsPath, $"{safeTitle}.{audioStreamInfo.Container}");

                Console.WriteLine($"Descargando vídeo ({videoStreamInfo.VideoQuality.MaxHeight}p): {Path.GetFileName(videoFile)}");
                var videoProgress = new Progress<double>(p => DisplayProgress(p, "Vídeo"));
                await youtube.Videos.Streams.DownloadAsync(videoStreamInfo, videoFile, videoProgress);
                Console.WriteLine();

                Console.WriteLine($"Descargando audio ({audioStreamInfo.Bitrate / 1000} kbps): {Path.GetFileName(audioFile)}");
                var audioProgress = new Progress<double>(p => DisplayProgress(p, "Audio"));
                await youtube.Videos.Streams.DownloadAsync(audioStreamInfo, audioFile, audioProgress);
                Console.WriteLine("\n¡Descargas completadas!");

                Console.WriteLine("Nota: para fusionar audio y vídeo en un solo archivo, puedes usar ffmpeg:");
                Console.WriteLine($"ffmpeg -i \"{videoFile}\" -i \"{audioFile}\" -c:v copy -c:a aac \"{Path.Combine(downloadsPath, safeTitle + "_merged.mp4")}\"");

                return 0;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                Console.Error.WriteLine("Acceso prohibido (403) con YoutubeExplode.");
                return 1;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error inesperado: {ex.Message}");
                return 1;
            }
        }

        // Barra de progreso de 20 slots con etiqueta
        static void DisplayProgress(double value, string label)
        {
            const int totalSlots = 20;
            int filled = (int)Math.Round(value * totalSlots);
            filled = Math.Min(filled, totalSlots);
            var bar = new string('#', filled) + new string('-', totalSlots - filled);
            var percent = (int)Math.Round(value * 100);
            Console.Write($"\r[{label}] [{bar}] {percent}%");
        }

        // Sanitiza nombre
        static string SanitizeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }
    }
}
