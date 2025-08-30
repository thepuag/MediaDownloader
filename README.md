## Answer

MediaDownloader is a .NET 8.0 console application that serves as a YouTube media downloader using yt-dlp as its core download engine [1](#0-0) . 
The application implements a self-healing wrapper pattern that automatically manages dependencies and provides real-time download progress monitoring [2](#0-1) .

### Key Features

**Automatic Dependency Management**: The application automatically downloads the yt-dlp executable from GitHub if it's not present in the application directory [3](#0-2) . This ensures the tool works out-of-the-box without manual setup.

**Smart Download Organization**: Downloads are automatically organized into a dedicated Media folder within the user's Downloads directory [4](#0-3) . Files are saved with their original titles and appropriate extensions.

**Real-time Progress Monitoring**: The application captures and displays live download progress by filtering yt-dlp output for lines containing percentage or ETA information [5](#0-4) . 
Progress lines are dynamically updated in place for a clean console experience.

**High-Quality Downloads**: Configured to download the best available video and audio quality using yt-dlp's "bestvideo+bestaudio/best" format selection [6](#0-5) .

### Technical Architecture

The application follows a subprocess wrapper pattern where the main program orchestrates the external yt-dlp process [7](#0-6) . It redirects standard output and error streams to provide user feedback while maintaining process isolation [8](#0-7) .

The workflow is straightforward: prompt for URL input, ensure yt-dlp availability, create download directories, configure the download process, and monitor execution with real-time feedback [9](#0-8) .

### Requirements

- .NET 8.0 Runtime
- Internet connection for downloading yt-dlp and accessing YouTube
- Write permissions to the user's Downloads folder
- Ability to execute external processes

## Notes

The application is designed as a simple, user-friendly wrapper around yt-dlp that handles the complexity of dependency management and provides a clean console interface for YouTube downloads.
The self-healing architecture ensures reliability by automatically resolving missing dependencies at runtime.

Wiki pages you might want to explore:
- [MediaDownloader Overview (thepuag/MediaDownloader)](/wiki/thepuag/MediaDownloader#1)
