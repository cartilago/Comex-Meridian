using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageConvolutionFilters
{
    public class Blur3x3Filter : ConvolutionFilterBase
    {
        public override string FilterName
        {
            get { return "Blur3x3Filter"; }
        }

        private float factor = 1.0f;
        public override float Factor
        {
            get { return factor; }
        }

        private float bias = 0.0f;
        public override float Bias
        {
            get { return bias; }
        }

        private float[,] filterMatrix =
            new float[,] { { 0.0f, 0.2f, 0.0f, }, 
                           { 0.2f, 0.2f, 0.2f, }, 
                           { 0.0f, 0.2f, 0.2f, }, };

        public override float[,] FilterMatrix
        {
            get { return filterMatrix; }
        }
    }

    public class Blur5x5Filter : ConvolutionFilterBase
    {
        public override string FilterName
        {
            get { return "Blur5x5Filter"; }
        }

        private float factor = 1.0f / 13.0f;
        public override float Factor
        {
            get { return factor; }
        }

        private float bias = 0.0f;
        public override float Bias
        {
            get { return bias; }
        }

        private float[,] filterMatrix =
            new float[,] { { 0, 0, 1, 0, 0, }, 
                           { 0, 1, 1, 1, 0, }, 
                           { 1, 1, 1, 1, 1, },
                           { 0, 1, 1, 1, 0, },
                           { 0, 0, 1, 0, 0, }, };

        public override float[,] FilterMatrix
        {
            get { return filterMatrix; }
        }
    }

    public class Gaussian3x3BlurFilter : ConvolutionFilterBase
    {
        public override string FilterName
        {
            get { return "Gaussian3x3BlurFilter"; }
        }

        private float factor = 1.0f / 16.0f;
        public override float Factor
        {
            get { return factor; }
        }

        private float bias = 0.0f;
        public override float Bias
        {
            get { return bias; }
        }

        private float[,] filterMatrix =
            new float[,] { { 1, 2, 1, }, 
                            { 2, 4, 2, }, 
                            { 1, 2, 1, }, };

        public override float[,] FilterMatrix
        {
            get { return filterMatrix; }
        }
    }

    public class Gaussian5x5BlurFilter : ConvolutionFilterBase
    {
        public override string FilterName
        {
            get { return "Gaussian5x5BlurFilter"; }
        }

        private float factor = 1.0f / 159.0f;
        public override float Factor
        {
            get { return factor; }
        }

        private float bias = 0.0f;
        public override float Bias
        {
            get { return bias; }
        }

        private float[,] filterMatrix =
            new float[,] { { 2, 04, 05, 04, 2 }, 
                           { 4, 09, 12, 09, 4 }, 
                           { 5, 12, 15, 12, 5 },
                           { 4, 09, 12, 09, 4 },
                           { 2, 04, 05, 04, 2 }, };

        public override float[,] FilterMatrix
        {
            get { return filterMatrix; }
        }
    }

    public class MotionBlurFilter : ConvolutionFilterBase
    {
        public override string FilterName
        {
            get { return "MotionBlurFilter"; }
        }

        private float factor = 1.0f / 18.0f;
        public override float Factor
        {
            get { return factor; }
        }

        private float bias = 0.0f;
        public override float Bias
        {
            get { return bias; }
        }

        private float[,] filterMatrix =
            new float[,] { {1, 0, 0, 0, 0, 0, 0, 0, 1,},
                           {0, 1, 0, 0, 0, 0, 0, 1, 0,},
                           {0, 0, 1, 0, 0, 0, 1, 0, 0,},
                           {0, 0, 0, 1, 0, 1, 0, 0, 0,},
                           {0, 0, 0, 0, 1, 0, 0, 0, 0,},
                           {0, 0, 0, 1, 0, 1, 0, 0, 0,},
                           {0, 0, 1, 0, 0, 0, 1, 0, 0,},
                           {0, 1, 0, 0, 0, 0, 0, 1, 0,},
                           {1, 0, 0, 0, 0, 0, 0, 0, 1,}, };

        public override float[,] FilterMatrix
        {
            get { return filterMatrix; }
        }
    }

    public class MotionBlurLeftToRightFilter : ConvolutionFilterBase
    {
        public override string FilterName
        {
            get { return "MotionBlurLeftToRightFilter"; }
        }

        private float factor = 1.0f / 9.0f;
        public override float Factor
        {
            get { return factor; }
        }

        private float bias = 0.0f;
        public override float Bias
        {
            get { return bias; }
        }

        private float[,] filterMatrix =
            new float[,] { {1, 0, 0, 0, 0, 0, 0, 0, 0,},
                           {0, 1, 0, 0, 0, 0, 0, 0, 0,},
                           {0, 0, 1, 0, 0, 0, 0, 0, 0,},
                           {0, 0, 0, 1, 0, 0, 0, 0, 0,},
                           {0, 0, 0, 0, 1, 0, 0, 0, 0,},
                           {0, 0, 0, 0, 0, 1, 0, 0, 0,},
                           {0, 0, 0, 0, 0, 0, 1, 0, 0,},
                           {0, 0, 0, 0, 0, 0, 0, 1, 0,},
                           {0, 0, 0, 0, 0, 0, 0, 0, 1,}, };

        public override float[,] FilterMatrix
        {
            get { return filterMatrix; }
        }
    }

    public class MotionBlurRightToLeftFilter : ConvolutionFilterBase
    {
        public override string FilterName
        {
            get { return "MotionBlurRightToLeftFilter"; }
        }

        private float factor = 1.0f / 9.0f;
        public override float Factor
        {
            get { return factor; }
        }

        private float bias = 0.0f;
        public override float Bias
        {
            get { return bias; }
        }

        private float[,] filterMatrix =
            new float[,] { {0, 0, 0, 0, 0, 0, 0, 0, 1,},
                           {0, 0, 0, 0, 0, 0, 0, 1, 0,},
                           {0, 0, 0, 0, 0, 0, 1, 0, 0,},
                           {0, 0, 0, 0, 0, 1, 0, 0, 0,},
                           {0, 0, 0, 0, 1, 0, 0, 0, 0,},
                           {0, 0, 0, 1, 0, 0, 0, 0, 0,},
                           {0, 0, 1, 0, 0, 0, 0, 0, 0,},
                           {0, 1, 0, 0, 0, 0, 0, 0, 0,},
                           {1, 0, 0, 0, 0, 0, 0, 0, 0,}, };

        public override float[,] FilterMatrix
        {
            get { return filterMatrix; }
        }
    }
}