﻿using Catrobat.Paint.WindowsPhone.Command;
using System;
using System.IO;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Catrobat.Paint.WindowsPhone.Tool
{
    class StampTool : RectangleShapeBaseTool
    {
        public StampTool()
        {
            ToolType = ToolType.Stamp;
             
        }

        public override void HandleDown(object arg)
        {
            
        }

        public override void HandleMove(object arg)
        {
            
        }

        public override void HandleUp(object arg)
        
        {
        }

        public override void Draw(object o)
        {
            
        }

        public async void StampCopy()
        {
            double heightStampControl = PocketPaintApplication.GetInstance().StampControl.GetHeightOfRectangleStampSelection();
            double widthStampControl = PocketPaintApplication.GetInstance().StampControl.GetWidthOfRectangleStampSelection();
            double croppedImageHeight = heightStampControl, croppedImageWidth = widthStampControl;

            Point leftTopPointStampSelection = PocketPaintApplication.GetInstance().StampControl.GetLeftTopPointOfStampedSelection();

            System.Diagnostics.Debug.WriteLine("leftTop(Stampcopy): " + leftTopPointStampSelection);
            if(leftTopPointStampSelection.X < 0)
            {
                // + and - = -
                croppedImageWidth = widthStampControl + leftTopPointStampSelection.X;
            }
            if(leftTopPointStampSelection.Y < 0)
            {
                croppedImageHeight = heightStampControl + leftTopPointStampSelection.Y;
            }

            double xOffsetStampControl = leftTopPointStampSelection.X;
            double yOffsetStampControl = leftTopPointStampSelection.Y;

            string filename = "stamp" + ".png";
            await PocketPaintApplication.GetInstance().StorageIo.WriteBitmapToPngMediaLibrary(filename);
            StorageFile storageFile = await KnownFolders.PicturesLibrary.GetFileAsync(filename);
            InMemoryRandomAccessStream mrAccessStream = new InMemoryRandomAccessStream();

            using (Stream stream = await storageFile.OpenStreamForReadAsync())
            {
                using (var memStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memStream);
                    memStream.Position = 0;

                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(memStream.AsRandomAccessStream());
                    BitmapEncoder encoder = await BitmapEncoder.CreateForTranscodingAsync(mrAccessStream, decoder);

                    encoder.BitmapTransform.ScaledHeight = (uint)PocketPaintApplication.GetInstance().PaintingAreaCanvas.RenderSize.Height;
                    encoder.BitmapTransform.ScaledWidth = (uint)PocketPaintApplication.GetInstance().PaintingAreaCanvas.RenderSize.Width;

                    BitmapBounds bounds = new BitmapBounds
                    {
                        Height = (uint) croppedImageHeight - 1,
                        Width = (uint) croppedImageWidth - 1,
                        X = (uint) ((xOffsetStampControl) < 0 ? 0 : xOffsetStampControl),
                        Y = (uint) ((yOffsetStampControl) < 0 ? 0 : yOffsetStampControl)
                    };

                    encoder.BitmapTransform.Bounds = bounds;

                    // write out to the stream
                    try
                    {
                        await encoder.FlushAsync();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
                //render the stream to the screen
                WriteableBitmap wbCroppedBitmap = new WriteableBitmap((int)croppedImageWidth, (int)croppedImageHeight);
                wbCroppedBitmap.SetSource(mrAccessStream);
                PocketPaintApplication.GetInstance().StampControl.SetSourceImageStamp(wbCroppedBitmap);
            }
        }

        public void StampClear()
        {
            PocketPaintApplication.GetInstance().StampControl.SetSourceImageStamp(null);
        }

        public void StampPaste()
        {
            double heightStampControl = PocketPaintApplication.GetInstance().StampControl.GetHeightOfRectangleStampSelection();
            double widthStampControl = PocketPaintApplication.GetInstance().StampControl.GetWidthOfRectangleStampSelection();

            Point leftTopPointStampSelection = PocketPaintApplication.GetInstance().StampControl.GetLeftTopPointOfStampedSelection();
            double xCoordinateOnWorkingSpace = leftTopPointStampSelection.X;
            double yCoordinateOnWorkingSpace = leftTopPointStampSelection.Y;
            System.Diagnostics.Debug.WriteLine("xCoor: " + xCoordinateOnWorkingSpace + " " + yCoordinateOnWorkingSpace);
            Image stampedImage = new Image
            {
                Source = PocketPaintApplication.GetInstance().StampControl.GetImageSourceStampedImage(),
                Height = heightStampControl,
                Width = widthStampControl,
                Stretch = Stretch.Fill
            };

            PocketPaintApplication.GetInstance().PaintingAreaView.addElementToPaintingAreCanvas(stampedImage, (int)(xCoordinateOnWorkingSpace), (int)(yCoordinateOnWorkingSpace));
            CommandManager.GetInstance().CommitCommand(new StampCommand((uint)xCoordinateOnWorkingSpace, (uint)yCoordinateOnWorkingSpace, stampedImage));
        }

        public void StampPaste(uint xCoordinateOnWorkingSpace, uint yCoordinateOnWorkingSpace, Image stampedImage)
        {
            Canvas.SetLeft(stampedImage, xCoordinateOnWorkingSpace);
            Canvas.SetTop(stampedImage, yCoordinateOnWorkingSpace);
            PocketPaintApplication.GetInstance().PaintingAreaCanvas.Children.Add(stampedImage);
        }

        public override void ResetDrawingSpace()
        {
            PocketPaintApplication.GetInstance().StampControl.SetStampSelection();
            PocketPaintApplication.GetInstance().StampControl.ResetCurrentCopiedSelection();
            PocketPaintApplication.GetInstance().PaintingAreaView.app_btnStampClear_Click(new object(), new RoutedEventArgs());
        }

        public override void ResetUsedElements()
        {
            PocketPaintApplication.GetInstance().StampControl.SetSourceImageStamp(null);
        }
    }
}
