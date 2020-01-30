using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace CodeStoriesStokeDataGenerator
{
    class Program : Application
    {
        static Window myWindow;
        static InkCanvas myInkCanvas;

        [STAThread]
        static void Main(string[] args)
        {
            new Program().Run();
            GenerateStrokes();
        }
        protected override void OnStartup(StartupEventArgs args)
        {
            base.OnStartup(args);

            myWindow = new Window();
            myInkCanvas = new InkCanvas();
            myWindow.Content = myInkCanvas;
            myWindow.Show();
        }

        public static bool GenerateStrokes()
        {
            SaveFileDialog mySaveFileDialog = new SaveFileDialog();
            mySaveFileDialog.Filter = "isf files (*.isf)|*.isf";

            if (mySaveFileDialog.ShowDialog() == true)
            {
                // Save the ink to a .bmp picture
                SaveAsBmp(mySaveFileDialog.FileName);
                // Save the strokes to a json file
                SaveAsJsonFile(mySaveFileDialog.FileName); 
            }
                       
            return true;
        }

        static void SaveAsBmp(string FilePathAndName)
        {
            int width = (int)myInkCanvas.ActualWidth;
            int height = (int)myInkCanvas.ActualHeight;
            RenderTargetBitmap myRenderBmp = new RenderTargetBitmap(width, height, 96d, 96d, PixelFormats.Default);
            myRenderBmp.Render(myInkCanvas);
            BmpBitmapEncoder myEncoder = new BmpBitmapEncoder();
            myEncoder.Frames.Add(BitmapFrame.Create(myRenderBmp));
            string bmpFileName = FilePathAndName.Replace(".isf", "_Ink.bmp");
            using (FileStream bmpFileStream = new FileStream(bmpFileName, FileMode.Create))
            {
                myEncoder.Save(bmpFileStream);
            }
        }

        static void SaveAsJsonFile(string FilePathAndName)
        {
            int intCounter = 1;
            string myStrokesJson = string.Empty;
            myStrokesJson = "{" +
                                "\"version\": 1, " +
                                "\"language\": \"en-US\", " +
                                "\"unit\": \"mm\", " +
                                "\"strokes\": [";
            foreach (Stroke oneStroke in myInkCanvas.Strokes)
            {
                string myPoints = string.Empty;
                foreach (Point onePoint in oneStroke.StylusPoints)
                {
                    myPoints += onePoint.X + "," + onePoint.Y + ",";
                }
                myPoints = myPoints.Remove(myPoints.Length - 1); 

                myStrokesJson += "{" +
                                    "\"id\": " + intCounter + "," +
                                    "\"points\": \"" +
                                    myPoints +
                                    "\"},";
                intCounter++;
            }
            myStrokesJson = myStrokesJson.Remove(myStrokesJson.Length - 1); 
            myStrokesJson += "]}";

            string jsonFileName = FilePathAndName.Replace(".isf", "_Ink.json");
            using (TextWriter writer = new StreamWriter(jsonFileName, true))
            {
                writer.Write(myStrokesJson);
            }
        }
    }
}
