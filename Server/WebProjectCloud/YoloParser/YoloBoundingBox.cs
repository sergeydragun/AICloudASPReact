﻿using System.Drawing;


namespace WebProjectCloud.YoloParser
{
    public class BoundingBoxDimensions : DimensionsBase { }

    public class YoloBoundingBox
    {
        public BoundingBoxDimensions Dimensions { get; set; }
        public string Label { get; set; }
        public float Confidence { get; set; }
        public RectangleF Rect
        {
            get { return new RectangleF(Dimensions.X, Dimensions.Y, Dimensions.Width, Dimensions.Height); }
        }
    }
}
