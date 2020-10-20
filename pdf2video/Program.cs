using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using FFmpeg.NET;
using PdfiumLight;

namespace pdf2video
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            bool busy = true;
            var parsed = Parser.Default.ParseArguments<Options>(args);

            parsed.WithNotParsed(e => busy = false);

            parsed.WithParsed<Options>(async o =>
            {
                Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
                PdfDocument pdf = new PdfDocument(o.PdfInputFile);
                if (o.StartPage.HasValue == false)
                    o.StartPage = 0;
                if (o.EndPage.HasValue == false)
                    o.EndPage = pdf.PageCount() - 1;

                Directory.CreateDirectory("imgs");


                //render each page
                int frameCount = 0;
                for (int i = o.StartPage.Value; i <= o.EndPage.Value; i += 1)
                {
                    Console.WriteLine($"Rendering page {i} of {o.EndPage}");
                    var page = pdf.GetPage(i);
                    var scale = o.PixelResolution / page.Width;
                    var img = page.Render(o.PixelResolution, (int)(scale * page.Height),
                        (int)(o.ClipRectX * o.PixelResolution), (int)(o.ClipRectY * scale * page.Height),
                        (int)(o.ClipRectWidth * o.PixelResolution), (int)(o.ClipRectHeight * scale * page.Height),
                        PdfRotation.Rotate0, PdfRenderFlags.LcdText);

                    img.Save(Path.Combine("imgs", i.ToString("D8") + ".png"));
                    frameCount += 1;

                }

                var ffmpeg = new Engine();
                Console.WriteLine("encoding video");

                //calc framerate
                double frameRate = 1;
                if (o.FrameRate.HasValue)
                {
                    Console.WriteLine($"using framerate of {o.FrameRate} FPS");
                    frameRate = frameCount / o.Duration.Value;
                }
                else if (o.Duration.HasValue)
                {
                    Console.WriteLine($"using total duration of {o.Duration} seconds");
                    frameRate = frameCount / o.Duration.Value;
                }
                else if (o.FrameTime.HasValue)
                {
                    Console.WriteLine($"using frameTime of {o.FrameTime} seconds");
                    frameRate = 1.0 / o.FrameTime.Value;
                }

                frameRate = Math.Round(frameRate, 2);

                Console.WriteLine($"generating {1 / frameRate * frameCount} seconds of video at {o.VideoOutputFile}");

                await ffmpeg.ExecuteAsync($"-f image2 -r {frameRate} -i imgs/%08d.png -vcodec libx264 -profile:v high444 -refs 16 -crf 0 -preset slow {o.VideoOutputFile}");


                //Clear temp path
                DirectoryInfo di = new DirectoryInfo("imgs");

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }

                Console.WriteLine("all done");
                busy = false;
            });
            //open pdf
            //select page range
            //render as png sequence
            //export to mov
            while (busy)
                Thread.Sleep(50);
            
        }
    }

    public class Options
    {
        [Option("pdf", Required = true, HelpText = "PDF File to convert to a video")]
        public string PdfInputFile  { get; set; }

        [Option('o',"output", Required = true, HelpText = "video output file")]
        public string VideoOutputFile { get; set; }



        [Option("from", Required = false, HelpText = "start conversion from this page (0 based index)")]
        public int? StartPage { get; set; }

        [Option("to", Required = false, HelpText = "end conversion at (including) this page (0 based index)")]
        public int? EndPage { get; set; }

        [Option("res", Required = false, HelpText = "desired width of the rendered video (height calculated automatically)", Default = 2048)]
        public int PixelResolution { get; set; }


        [Option("frameTime", HelpText = "time each frame is displayed in the video", Group = "duration")]
        public double? FrameTime { get; set; }
        [Option("duration", HelpText = "duration of the entire converted video", Group = "duration")]
        public double? Duration { get; set; }
        [Option("frameRate", HelpText = "framerate of the converted video (in FPS)", Group = "duration")]
        public double? FrameRate { get; set; }


        [Option("clipX", Required =false, HelpText = "relative clipping. left coordinate", Default = 0)]
        public double ClipRectX { get; set; }

        [Option("clipY", Required = false, HelpText = "relative clipping. top coordinate", Default = 0)]
        public double ClipRectY { get; set; }

        [Option("clipWidth", Required = false, HelpText = "relative clipping. width", Default = 1)]
        public double ClipRectWidth { get; set; }
        [Option("clipHeight", Required = false, HelpText = "relative clipping. height", Default = 1)]
        public double ClipRectHeight { get; set; }
    }
}
