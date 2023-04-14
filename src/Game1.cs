using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;

namespace eXNB
{
    internal class Game1 : Game
    {
        public static GraphicsDeviceManager graphics;
        public static string[] args;
        public static string inputDir = "Content";
        public static string outputDir = "Content";

        public Game1(string[] programArgs)
        {
            args = programArgs;

            string newInputDir = GetArgument("-i");
            string newOutputDir = GetArgument("-o");
            inputDir = newInputDir.Length > 0 ? newInputDir : inputDir;
            outputDir = newOutputDir.Length > 0 ? newOutputDir : outputDir;

            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1;
            graphics.PreferredBackBufferHeight = 1;
        }

        protected override void LoadContent()
        {
            if (!Directory.Exists(inputDir))
            {
                Log("ERROR", $"Input directory \"{inputDir}\" does not exist.");
                Exit();
                return;
            }

            DirectoryInfo rootDir = new DirectoryInfo($@"{inputDir}");
            List<FileInfo> textureFiles = new List<FileInfo>();
            List<string> erroredFiles = new List<string>();
            WalkDirectoryTree(rootDir, textureFiles, "xnb");
            int completed = 0;

            foreach (FileInfo file in textureFiles)
            {
                try
                {
                    if (!file.Exists)
                    {
                        erroredFiles.Add($"File \"{file.FullName}\" does not exist. Skipping...");
                        continue;
                    }

                    long fileStart = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                    Texture2D temp = Content.Load<Texture2D>($@"{file.FullName}");

                    //Fix colors (use hardware eventually)
                    Bitmap temp_img = Texture2Image(temp);
                    System.Drawing.Color pixel;

                    for (int x = 0; x < temp_img.Width; ++x)
                    {
                        for (int y = 0; y < temp_img.Height; ++y)
                        {
                            pixel = temp_img.GetPixel(x, y);
                            pixel = System.Drawing.Color.FromArgb(pixel.A, pixel.R, pixel.G, pixel.B);
                            temp_img.SetPixel(x, y, pixel);
                        }
                    }

                    temp = GetTexture2DFromBitmap(graphics.GraphicsDevice, temp_img);

                    //Write file
                    string png = $"{file.FullName.Replace(".xnb", ".png")}";
                    Stream o = File.Create(png);
                    temp.SaveAsPng(o, temp.Width, temp.Height);
                    ++completed;

                    Console.SetCursorPosition(0, 5);
                    Console.Write($"     - Processing... {completed}/{textureFiles.Count}");
                    Console.SetCursorPosition(0, 6);
                    Console.Write($"     - {erroredFiles.Count} Error(s)");

                    temp.Dispose();
                    o.Dispose();
                }
                catch (Exception e)
                {
                    erroredFiles.Add(e.Message);
                }
            }

            foreach(string em in erroredFiles)
            {
                Log("ERROR", em);
            }

            Exit();
        }

        //https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/file-system/how-to-iterate-through-a-directory-tree
        static void WalkDirectoryTree(DirectoryInfo root, List<FileInfo> file_list, string ext = "*")
        {
            FileInfo[] files = null;
            DirectoryInfo[] subDirs = null;

            try
            {
                files = root.GetFiles($"*.{ext}");
            }
            catch (Exception e)
            {
                Log("ERROR", e.Message);
            }

            if (files != null)
            {
                foreach (FileInfo fi in files)
                {
                    file_list.Add(fi);
                }

                subDirs = root.GetDirectories();

                foreach (DirectoryInfo dirInfo in subDirs)
                {
                    WalkDirectoryTree(dirInfo, file_list, ext);
                }
            }
        }

        //https://stackoverflow.com/a/12495674
        public static Bitmap Texture2Image(Texture2D texture)
        {
            Image img;
            using (MemoryStream MS = new MemoryStream())
            {
                texture.SaveAsPng(MS, texture.Width, texture.Height);
                MS.Seek(0, SeekOrigin.Begin);
                img = Bitmap.FromStream(MS);
            }
            return (Bitmap)img;
        }

        //https://stackoverflow.com/a/2870399
        public static Texture2D GetTexture2DFromBitmap(GraphicsDevice device, Bitmap bitmap)
        {
            Texture2D tex = new Texture2D(device, bitmap.Width, bitmap.Height);
            BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);
            int bufferSize = data.Height * data.Stride;
            byte[] bytes = new byte[bufferSize];
            Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
            tex.SetData(bytes);
            bitmap.UnlockBits(data);
            return tex;
        }

        public static string GetArgument(string arg)
        {
            for(int i = 0; i < args.Length; ++i)
            {
                if (args[i] == arg && args.Length > i + 1)
                {
                    return args[i + 1];
                }
            }

            return "";
        }


        public static void Log(string prefix, string message)
        {
#if !DEBUG
            if(prefix == "DEBUG") return;
#endif
            Console.WriteLine($"[ {prefix} ]: {message}");
        }
    }
}