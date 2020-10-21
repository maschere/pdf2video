# pdf2video
Convert multi-page PDF animations to H.264 video

## Description
pdf2video is a light-weight command-line utility that uses [PDFium](https://pdfium.googlesource.com/pdfium/) and [FFmpeg](https://ffmpeg.org/) under the hood to render (almost) lossless H.264 videos from a multi-page PDF document.

## Installation
Download the [latest release](https://github.com/maschere/pdf2video/releases/latest), extract it somewhere in your user-folder and run pdf2video.exe from the command-line.

## Usage
Command-line parameters are

 ```console
  --pdf           Required. PDF File to convert to a video

  -o, --output    Required. video output file

  --ffmpeg        directory of ffmpeg executable

  --from          start conversion from this page (0 based index)

  --to            end conversion at (including) this page (0 based index)

  --res           (Default: 2048) desired width of the rendered (height calculated automatically)

  --frameTime     (Group: duration) time each frame is displayed in the video

  --duration      (Group: duration) duration of the entire converted video

  --frameRate     (Group: duration) framerate of the converted video (in FPS)

  --clipX         (Default: 0) relative clipping. left coordinate

  --clipY         (Default: 0) relative clipping. top coordinate

  --clipWidth     (Default: 1) relative clipping. width

  --clipHeight    (Default: 1) relative clipping. height

  --help          Display this help screen.

  --version       Display version information.
 ```


## Building from source
Clone this repository and compile it using Visual Studio. Only tested under VS2019, Win10 x64 and .NET Framework 4.7.2