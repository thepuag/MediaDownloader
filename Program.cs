using System;
using System.IO;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Uso: downloader <URL de YouTube>");
            return;
        }
        var url = args[0];
        var youtube = new YoutubeClient();

        // Obtener manifest de streams (vídeo+audio)
        var manifest = await youtube.Videos.Streams.GetManifestAsync(url);
        // Seleccionar el mejor stream combinado (muxed)
        var streamInfo = manifest.GetMuxedStreams()
                                 .GetWithHighestVideoQuality();

        var fileName = $"{streamInfo.VideoId}.{streamInfo.Container}";
        Console.WriteLine($"Descargando {fileName}...");

        // Descargar al directorio actual
        await youtube.Videos.Streams.DownloadAsync(streamInfo, fileName);
        Console.WriteLine("Descarga completada.");
    }
}
