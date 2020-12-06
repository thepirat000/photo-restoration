using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;

namespace photo_api.Helpers
{
    public static class ImageHelper
    {
        public static void ResizeImage(string sourceFile, string destFile, int maxWidth, int maxHeight, long quality)
        {
            Bitmap newImage = null;
            using (var sourceImage = Image.FromFile(sourceFile))
            {
                // Determine which ratio is greater, the width or height, and use
                // this to calculate the new width and height. Effectually constrains
                // the proportions of the resized image to the proportions of the original.
                double xRatio = (double)sourceImage.Width / maxWidth;
                double yRatio = (double)sourceImage.Height / maxHeight;
                double ratioToResizeImage = Math.Max(xRatio, yRatio);
                int newWidth = (int)Math.Floor(sourceImage.Width / ratioToResizeImage);
                int newHeight = (int)Math.Floor(sourceImage.Height / ratioToResizeImage);

                // Create new image canvas -- use maxWidth and maxHeight in this function call if you wish
                // to set the exact dimensions of the output image.
                newImage = new Bitmap(newWidth, newHeight, PixelFormat.Format32bppArgb);

                // Render the new image, using a graphic object
                using (Graphics newGraphic = Graphics.FromImage(newImage))
                {
                    using (var wrapMode = new ImageAttributes())
                    {
                        wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                        newGraphic.DrawImage(sourceImage, new Rectangle(0, 0, newWidth, newHeight), 0, 0, sourceImage.Width, sourceImage.Height, GraphicsUnit.Pixel, wrapMode);
                    }

                    // Set the background color to be transparent (can change this to any color)
                    newGraphic.Clear(Color.Transparent);

                    // Set the method of scaling to use -- HighQualityBicubic is said to have the best quality
                    newGraphic.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    // Apply the transformation onto the new graphic
                    Rectangle sourceDimensions = new Rectangle(0, 0, sourceImage.Width, sourceImage.Height);
                    Rectangle destinationDimensions = new Rectangle(0, 0, newWidth, newHeight);
                    newGraphic.DrawImage(sourceImage, destinationDimensions, sourceDimensions, GraphicsUnit.Pixel);
                }
            }

            var ici = ImageCodecInfo.GetImageEncoders().FirstOrDefault(ie => ie.MimeType == "image/jpeg");
            var eps = new EncoderParameters(1);
            eps.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            newImage.Save(destFile, ici, eps);
        }
    }
}
