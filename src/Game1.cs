using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace eXNB
{
    internal class Game1 : Game
    {
        public GraphicsDeviceManager graphics;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1;
            graphics.PreferredBackBufferHeight = 1;
        }

        protected override void LoadContent()
        {
            DirectoryInfo rootDir = new DirectoryInfo(@"./Content/");
            List<string> texture_files = new List<string>();
            WalkDirectoryTree(rootDir, texture_files, "xnb");

            foreach (string file in texture_files)
            {
                try
                {
                    Texture2D temp = Content.Load<Texture2D>($@"{file}");
                    Console.WriteLine($"[ INFO ] : {file} loaded...");

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

                    Console.WriteLine($"[ INFO ] : {file} colors fixed...");
                    temp = GetTexture2DFromBitmap(graphics.GraphicsDevice, temp_img);

                    //Write file
                    string png = file.Replace(".xnb", ".png");
                    Stream o = File.Create(png);
                    temp.SaveAsPng(o, temp.Width, temp.Height);
                    Console.WriteLine($"[ INFO ] : {png} completed!");

                    temp.Dispose();
                    o.Dispose();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[ ERROR ] : {e.Message}");
                }
            }

            Exit();
        }

        //https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/file-system/how-to-iterate-through-a-directory-tree
        static void WalkDirectoryTree(DirectoryInfo root, List<string> file_list, string ext = "*")
        {
            FileInfo[] files = null;
            DirectoryInfo[] subDirs = null;

            try
            {
                files = root.GetFiles($"*.{ext}");
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }

            if (files != null)
            {
                foreach (FileInfo fi in files)
                {
                    file_list.Add(fi.FullName);
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
    }
}