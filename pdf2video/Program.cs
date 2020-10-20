using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
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
            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(async o =>
                   {
                       PdfDocument pdf = new PdfDocument(o.PdfInputFile);
                       if (o.StartPage.HasValue == false)
                           o.StartPage = 0;
                       if (o.EndPage.HasValue == false)
                           o.EndPage = pdf.PageCount()-1;

                       Directory.CreateDirectory("imgs");
                       //Clear temp path
                       DirectoryInfo di = new DirectoryInfo("imgs");

                       foreach (FileInfo file in di.GetFiles())
                       {
                           file.Delete();
                       }

                       //render each page
                       var slides = new List<Image>();
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


                       }

                       var ffmpeg = new Engine();

                       await ffmpeg.ExecuteAsync("-f image2 -r 1 -i imgs/%08d.png -vcodec libx264 -profile:v high444 -refs 16 -crf 0 -preset slow a.mp4");

                       Console.WriteLine("all done");

                   });
            //open pdf
            //select page range
            //render as png sequence
            //export to mov
            Console.ReadKey();
        }
    }

    public class Options
    {
        [Option("pdf", Required = true, HelpText = "PDF File to convert to a video")]
        public string PdfInputFile  { get; set; }

        //[Option('o',"output", Required = true, HelpText = "video output file")]
        //public string VideoOutputFile { get; set; }

        [Option("format", Required = false, HelpText = "video format")]
        public string VideoFormat { get; set; }


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
        [Option("frameRate", HelpText = "framerate of the converted video (in HZ)", Group = "duration")]
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
