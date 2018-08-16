using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicViewer.ComicModel
{
    public class ConvolutionKernels
    {
        ///<summary>
        /// Sharpen kernel with the size 3x3
        ///</summary>
        public static int[,] Sharpen = {
                                            { 0, -2,  0},
                                            {-2, 11, -2},
                                            { 0, -2,  0}
                                        };

        ///<summary>
        /// EdgeDetect kernel with the size 3x3
        ///</summary>
        public static int[,] EdgeDetect = {
                                            {0, 1, 0},
                                            {1, -5, 1},
                                            {0, 1, 0}
                                        };
        ///<summary>
        /// Blur kernel with the size 3x3
        ///</summary>
        public static int[,] Blur = {
                                        { 1, 1,  1},
                                        {1, 1, 1},
                                        { 1, 1,  1}
                                    };
        ///<summary>
        /// Emboss kernel with the size 3x3
        ///</summary>
        public static int[,] Emboss = {
                                        { -2, -1,  0},
                                        {-1, 1, 1},
                                        { 0, 1,  2}
                                    };
        ///<summary>
        /// Gradient kernel with the size 3x3
        ///</summary>
        public static int[,] Gradient = {
                                            { -1,0,  -1},
                                            {-1, 0, -1},
                                            { -1, 0,  -1}
                                        };

    }
}
