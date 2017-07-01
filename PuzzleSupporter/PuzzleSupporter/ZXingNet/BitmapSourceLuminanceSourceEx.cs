/*
* Copyright 2012 ZXing.Net authors
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
* 
* Modified By Go Suzuki.
*/

using System.Windows;
using ZXing;
#if NETFX_CORE
using Windows.UI.Xaml.Media.Imaging;
#else
using System.Windows.Media;
using System.Windows.Media.Imaging;
#endif

namespace PuzzleSupporter.ZXingNet {
    public class BitmapSourceLuminanceSourceEx : BaseLuminanceSource {
        /// <summary>
        /// Initializes a new instance of the <see cref="BitmapSourceLuminanceSource"/> class.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public BitmapSourceLuminanceSourceEx(int width, int height)
           : base(width, height) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BitmapSourceLuminanceSource"/> class.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        public BitmapSourceLuminanceSourceEx(BitmapSource bitmap)
           : base(bitmap.PixelWidth, bitmap.PixelHeight) {
            switch (bitmap.Format.ToString()) {
                case "Bgr24":
                case "Bgr32":
                    CalculateLuminanceBGR(bitmap);
                    break;
                case "Bgra32":
                    CalculateLuminanceBGRA(bitmap);
                    break;
                case "Rgb24":
                    CalculateLuminanceRGB(bitmap);
                    break;
                case "Bgr565":
                    CalculateLuminanceBGR565(bitmap);
                    break;
                default:
                    // there is no special conversion routine to luminance values
                    // we have to convert the image to a supported format
                    bitmap = new FormatConvertedBitmap(bitmap, PixelFormats.Bgra32, null, 0);
                    CalculateLuminanceBGR(bitmap);
                    break;
            }
        }

        byte[] cache;
        int cache_size = -1;

        int luminances_size = -1;

        public void UpdateImage(BitmapSource bitmap) {
            this.Width = bitmap.PixelWidth;
            this.Height = bitmap.PixelHeight;
            if (luminances_size != Width * Height) {
                luminances_size = Width * Height;
                luminances = new byte[luminances_size];
            }
            switch (bitmap.Format.ToString()) {
                case "Bgr24":
                case "Bgr32":
                    CalculateLuminanceBGR(bitmap);
                    break;
                case "Bgra32":
                    CalculateLuminanceBGRA(bitmap);
                    break;
                case "Rgb24":
                    CalculateLuminanceRGB(bitmap);
                    break;
                case "Bgr565":
                    CalculateLuminanceBGR565(bitmap);
                    break;
                default:
                    // there is no special conversion routine to luminance values
                    // we have to convert the image to a supported format
                    bitmap = new FormatConvertedBitmap(bitmap, PixelFormats.Bgra32, null, 0);
                    CalculateLuminanceBGR(bitmap);
                    break;
            }
        }

        private void CalculateLuminanceRGB(BitmapSource bitmap) {
            var width = bitmap.PixelWidth;
            var height = bitmap.PixelHeight;
            var stepX = (bitmap.Format.BitsPerPixel + 7) / 8;
            var bufferSize = width * stepX;
            if (bufferSize != cache_size) {
                cache = new byte[bufferSize];
                cache_size = bufferSize;
            }
            var rect = new Int32Rect(0, 0, width, 1);
            var luminanceIndex = 0;
            byte r, g, b;

            for (var curY = 0; curY < height; curY++) {
                bitmap.CopyPixels(rect, cache, bufferSize, 0);
                for (var curX = 0; curX < bufferSize; curX += stepX) {
                    r = cache[curX];
                    g = cache[curX + 1];
                    b = cache[curX + 2];
                    luminances[luminanceIndex] = (byte)((RChannelWeight * r + GChannelWeight * g + BChannelWeight * b) >> ChannelWeight);
                    luminanceIndex++;
                }
                rect.Y++;
            }
        }

        private void CalculateLuminanceBGR(BitmapSource bitmap) {
            var width = bitmap.PixelWidth;
            var height = bitmap.PixelHeight;
            var stepX = (bitmap.Format.BitsPerPixel + 7) / 8;
            var bufferSize = width * stepX;
            if (bufferSize != cache_size) {
                cache = new byte[bufferSize];
                cache_size = bufferSize;
            }
            var rect = new Int32Rect(0, 0, width, 1);
            var luminanceIndex = 0;
            byte b, g, r;

            for (var curY = 0; curY < height; curY++) {
                bitmap.CopyPixels(rect, cache, bufferSize, 0);
                for (var curX = 0; curX < bufferSize; curX += stepX) {
                    b = cache[curX];
                    g = cache[curX + 1];
                    r = cache[curX + 2];
                    luminances[luminanceIndex] = (byte)((RChannelWeight * r + GChannelWeight * g + BChannelWeight * b) >> ChannelWeight);
                    luminanceIndex++;
                }
                rect.Y++;
            }
        }

        private void CalculateLuminanceBGRA(BitmapSource bitmap) {
            var width = bitmap.PixelWidth;
            var height = bitmap.PixelHeight;
            var stepX = (bitmap.Format.BitsPerPixel + 7) / 8;
            var bufferSize = width * stepX;
            if (bufferSize != cache_size) {
                cache = new byte[bufferSize];
                cache_size = bufferSize;
            }
            var rect = new Int32Rect(0, 0, width, 1);
            var luminanceIndex = 0;

            byte b, g, r, luminance, alpha;


            for (var curY = 0; curY < height; curY++) {
                bitmap.CopyPixels(rect, cache, bufferSize, 0);
                for (var curX = 0; curX < bufferSize; curX += stepX) {
                    b = cache[curX];
                    g = cache[curX + 1];
                    r = cache[curX + 2];
                    luminance = (byte)((RChannelWeight * r + GChannelWeight * g + BChannelWeight * b) >> ChannelWeight);
                    alpha = cache[curX + 3];
                    luminance = (byte)(((luminance * alpha) >> 8) + (255 * (255 - alpha) >> 8));
                    luminances[luminanceIndex] = luminance;
                    luminanceIndex++;
                }
                rect.Y++;
            }
        }

        private void CalculateLuminanceBGR565(BitmapSource bitmap) {
            var width = bitmap.PixelWidth;
            var height = bitmap.PixelHeight;
            var stepX = (bitmap.Format.BitsPerPixel + 7) / 8;
            var bufferSize = width * stepX;
            if (bufferSize != cache_size) {
                cache = new byte[bufferSize];
                cache_size = bufferSize;
            }
            var rect = new Int32Rect(0, 0, width, 1);
            var luminanceIndex = 0;

            byte byte1, byte2;
            int b5, g5, r5, r8, g8, b8;

            for (var curY = 0; curY < height; curY++) {
                bitmap.CopyPixels(rect, cache, bufferSize, 0);
                for (var curX = 0; curX < bufferSize; curX += stepX) {
                    byte1 = cache[curX];
                    byte2 = cache[curX + 1];

                    b5 = byte1 & 0x1F;
                    g5 = (((byte1 & 0xE0) >> 5) | ((byte2 & 0x03) << 3)) & 0x1F;
                    r5 = (byte2 >> 2) & 0x1F;
                    r8 = (r5 * 527 + 23) >> 6;
                    g8 = (g5 * 527 + 23) >> 6;
                    b8 = (b5 * 527 + 23) >> 6;

                    // cheap, not fully accurate conversion
                    //var pixel = (byte2 << 8) | byte1;
                    //b8 = (((pixel) & 0x001F) << 3);
                    //g8 = (((pixel) & 0x07E0) >> 2) & 0xFF;
                    //r8 = (((pixel) & 0xF800) >> 8);

                    luminances[luminanceIndex] = (byte)((RChannelWeight * r8 + GChannelWeight * g8 + BChannelWeight * b8) >> ChannelWeight);
                    luminanceIndex++;
                }
                rect.Y++;
            }
        }

        /// <summary>
        /// Should create a new luminance source with the right class type.
        /// The method is used in methods crop and rotate.
        /// </summary>
        /// <param name="newLuminances">The new luminances.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns></returns>
        protected override LuminanceSource CreateLuminanceSource(byte[] newLuminances, int width, int height) {
            return new BitmapSourceLuminanceSourceEx(width, height) { luminances = newLuminances };
        }

        /// <summary>
        /// initializing constructor
        /// </summary>
        /// <param name="writeableBitmap"></param>
        //public BitmapSourceLuminanceSourceEx(WriteableBitmap writeableBitmap)
        // : base(writeableBitmap.PixelWidth, writeableBitmap.PixelHeight)
        //{
        //    SetWriteableBitmap(writeableBitmap);
        //}

        //public void UpdateImage(WriteableBitmap writeableBitmap) {
        //    this.Width = writeableBitmap.PixelWidth;
        //    this.Height = writeableBitmap.PixelHeight;
        //    SetWriteableBitmap(writeableBitmap);
        //}

        private unsafe void SetWriteableBitmap(WriteableBitmap writeableBitmap) {
            var height = writeableBitmap.PixelHeight;
            var width = writeableBitmap.PixelWidth;
            var stride = width * writeableBitmap.Format.BitsPerPixel / 8;

            // In order to measure pure decoding speed, we convert the entire image to a greyscale array
            // luminance array is initialized with new byte[width * height]; in base class

            if (cache_size != stride * height) {
                cache = new byte[stride * height];
                cache_size = stride * height;
            }
            writeableBitmap.CopyPixels(cache, stride, 0);
            //Color co;
            int srcPixel;
            if (luminances_size != stride * height) {
                luminances = new byte[stride * height];
                luminances_size = stride * height;
            }
            for (int sourceIndex = 0; sourceIndex < cache.Length && sourceIndex < luminances.Length; sourceIndex++) {
                srcPixel = cache[sourceIndex];
                //co = Color.FromArgb(
                //    (byte)((srcPixel >> 24) & 0xff),
                //    (byte)((srcPixel >> 16) & 0xff),
                //    (byte)((srcPixel >> 8) & 0xff),
                //    (byte)(srcPixel & 0xff));
                //luminances[sourceIndex] = (byte)((RChannelWeight * co.R + GChannelWeight * co.G + BChannelWeight * co.B) >> ChannelWeight);
                luminances[sourceIndex] = (byte)((RChannelWeight * ((srcPixel >> 16) & 0xff) + GChannelWeight * ((srcPixel >> 8) & 0xff) + BChannelWeight * (srcPixel & 0xff)) >> ChannelWeight);

            }
        }
      }
}
