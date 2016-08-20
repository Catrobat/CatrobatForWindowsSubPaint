﻿using Catrobat.Paint.WindowsPhone.Tool;
using Windows.UI.Xaml.Shapes;

namespace Catrobat.Paint.WindowsPhone.Command
{
    class BrushCommand : CommandBase
    {
        private Path Path { get; set; }

        public BrushCommand(Path path)
        {
            ToolType = ToolType.Brush;
            Path = path;
        }

        public override bool ReDo()
        {
            if (!PocketPaintApplication.GetInstance().PaintingAreaCanvas.Children.Contains(Path))
            {
                PocketPaintApplication.GetInstance().PaintingAreaCanvas.Children.Add(Path);
                return true;
            }
            return false;
        }

        public override bool UnDo()
        {
            if (PocketPaintApplication.GetInstance().PaintingAreaCanvas.Children.Contains(Path))
            {
                PocketPaintApplication.GetInstance().PaintingAreaCanvas.Children.Remove(Path);
                return true;                
            }
            return false;
        }
    }
}
