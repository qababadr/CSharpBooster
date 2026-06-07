namespace CBoosterSharp.Network.Downloader;

public static class FileDownloader
{
    public static async Task<string> DownloadWithProgressAsync(
        string url,
        string fileName,
        IProgress<double>? progress = null,
        HttpClient? httpClient = null,
        CancellationToken cancellationToken = default
    )
    {
        string extension = Path.GetExtension(new Uri(url).AbsolutePath);
        if (string.IsNullOrWhiteSpace(Path.GetExtension(fileName)) && !string.IsNullOrEmpty(extension))
        {
            fileName += extension;
        }

        string downloadsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Downloads"
        );

        string path = Path.Combine(downloadsPath, fileName);

        bool ownsClient = httpClient == null;
        HttpClient client = httpClient ?? new HttpClient();

        try
        {
            using HttpResponseMessage response = await client.GetAsync(
                url,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken
            );

            response.EnsureSuccessStatusCode();

            long total = response.Content.Headers.ContentLength ?? -1;
            long read = 0;

            await using Stream stream =
                await response.Content.ReadAsStreamAsync(cancellationToken);

            await using FileStream file = new(
                path,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None
            );

            byte[] buffer = new byte[8192];
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await file.WriteAsync(
                    buffer.AsMemory(0, bytesRead),
                    cancellationToken
                );

                read += bytesRead;

                if (total > 0)
                    progress?.Report((double)read / total * 100);
            }

            return path;
        }
        finally
        {
            if (ownsClient)
                client.Dispose();
        }
    }
}

