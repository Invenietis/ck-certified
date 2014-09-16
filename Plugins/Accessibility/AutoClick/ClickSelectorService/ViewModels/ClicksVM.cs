#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\AutoClick\ClickSelectorService\ViewModels\ClicksVM.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;
using AutoClick.Res;
using CK.Core;
using CK.Plugins.AutoClick.Model;
using CommonServices;
using HighlightModel;

namespace CK.Plugins.AutoClick.ViewModel
{
    public class ClicksVM : ObservableCollection<ClickEmbedderVM>, INotifyPropertyChanged, IHighlightableElement
    {
        #region Properties

        private ClickEmbedderVM _selectedClickEmbedderVM;
        public ClickVM NextClick { get { return _selectedClickEmbedderVM.NextClick; } }

        private readonly CKReadOnlyCollectionOnICollection<ClickEmbedderVM> _clicksVmReadOnlyAdapter;
        public ICKReadOnlyList<ClickEmbedderVM> ReadOnlyClicksVM { get { return _clicksVmReadOnlyAdapter.ToReadOnlyList(); } }

        internal ClickVM GetNextClick( bool doIncrement )
        {
            ClickVM nextClick = _selectedClickEmbedderVM.GetNextClick( doIncrement );
            if( doIncrement )
                OnPropertyChanged( "NextClick" );
            return nextClick;
        }

        string _leftClickVectorPath = "F1 M 90.518,256.036C 87.8446,255.265 85.158,254.533 82.498,253.716C 71.0046,250.182 61.542,243.648 54.218,234.14C 43.2447,219.892 32.3886,205.553 21.4953,191.242C 21.0126,190.608 20.6553,189.878 19.99,188.781L 111.382,119.17L 123.037,134.436C 130.207,143.837 137.291,153.305 144.557,162.633C 171.794,197.598 152.566,244.33 110.198,254.862C 108.951,255.172 107.745,255.641 106.521,256.036L 90.518,256.036 Z M 55.513,0C 56.4557,1.30399 57.281,2.71469 58.3597,3.89468C 74.5197,21.5653 63.6157,42.4214 45.4463,48.1026C 34.3784,51.564 23.8717,55.8867 15.4704,64.284C 7.2237,72.5254 4.86103,85.1654 9.80237,95.672C 12.0544,100.463 19.1717,104.241 24.8797,103.637C 26.1824,103.5 27.4677,103.213 29.0944,102.939L 31.4757,108.787C 21.2957,115.347 5.55703,105.881 2.42904,97.0453C -4.05363,78.7267 2.50769,61.452 21.2944,50.6347C 27.5757,47.0173 34.437,44.1253 41.365,41.9693C 59.341,36.3773 63.6423,23.1627 51.6823,8.57736C 49.185,5.53201 46.2463,2.84933 43.5117,0L 55.513,0 Z M 30.0809,113.185L 58.5689,150.648L 16.3769,182.769C 2.84223,161.42 9.62891,127.209 30.0809,113.185 Z";
        string _rightClickVectorPath = "F1 M 139.949,256C 137.275,255.229 134.589,254.497 131.929,253.68C 120.435,250.146 110.973,243.612 103.649,234.104C 92.6752,219.856 81.8192,205.517 70.9259,191.206C 70.4432,190.572 70.0859,189.842 69.4205,188.745L 160.813,119.134L 172.467,134.4C 179.638,143.801 186.722,153.269 193.987,162.597C 221.225,197.562 201.997,244.294 159.629,254.826C 158.382,255.136 157.175,255.605 155.951,256L 139.949,256 Z M 104.944,-0.0359802C 105.886,1.26802 106.712,2.67871 107.79,3.85872C 123.95,21.5294 113.046,42.3854 94.8769,48.0667C 83.8089,51.528 73.3022,55.8507 64.9009,64.248C 56.6542,72.4894 54.2916,85.1294 59.2329,95.636C 61.4849,100.427 68.6022,104.205 74.3102,103.601C 75.6129,103.464 76.8983,103.177 78.5249,102.903L 80.9062,108.751C 70.7249,115.311 54.9876,105.845 51.8596,97.0093C 45.3769,78.6907 51.9382,61.4161 70.7249,50.5987C 77.0063,46.9814 83.8676,44.0894 90.7956,41.9334C 108.772,36.3414 113.073,23.1267 101.113,8.54138C 98.6156,5.49605 95.6769,2.81335 92.9422,-0.0359802L 104.944,-0.0359802 Z M 84.8933,108.776C 107.965,93.1493 139.657,95.6574 155.96,114.715C 155.403,114.939 154.669,115.068 154.141,115.468L 113.579,146.413L 84.8933,108.776 Z";
        string _dbClickVectorPath = "F1 M 181.026,256C 178.353,255.229 175.666,254.497 173.006,253.68C 161.513,250.147 152.05,243.612 144.726,234.104C 133.753,219.856 122.897,205.517 112.003,191.207C 111.521,190.572 111.163,189.843 110.498,188.747L 201.89,119.135L 213.545,134.4C 220.715,143.801 227.799,153.269 235.065,162.597C 262.302,197.563 243.074,244.295 200.706,254.827C 199.459,255.136 198.253,255.605 197.029,256L 181.026,256 Z M 146.021,-0.035675C 146.964,1.26831 147.789,2.679 148.868,3.85901C 165.029,21.5297 154.124,42.3857 135.954,48.067C 124.886,51.5283 114.38,55.851 105.978,64.2483C 97.7317,72.4897 95.369,85.1297 100.31,95.6363C 102.562,100.427 109.68,104.206 115.388,103.602C 116.69,103.464 117.977,103.178 119.602,102.903L 121.984,108.751C 111.804,115.311 96.065,105.846 92.937,97.0096C 86.4544,78.691 93.0157,61.4163 111.802,50.599C 118.084,46.9816 124.945,44.0896 131.873,41.9337C 149.849,36.3417 154.15,23.1257 142.19,8.54167C 139.693,5.49634 136.754,2.81364 134.02,-0.035675L 146.021,-0.035675 Z M 120.65,113.222L 149.07,150.601L 107.139,182.558C 92.9178,164.117 100.437,125.92 120.65,113.222 Z M 25.4231,145.737L 43.8431,145.737L 43.8431,151.689L 14.9177,151.689C 16.5924,148.319 17.3897,145.389 19.1937,143.347C 23.2591,138.744 27.8351,134.595 32.1698,130.228C 33.1031,129.287 33.9764,128.261 34.7364,127.177C 37.4004,123.376 38.5457,119.269 35.8111,115.119C 33.7177,111.941 28.1817,111.531 23.3897,113.864C 21.7911,114.641 20.3004,115.644 18.6644,116.599C 17.0324,111.744 18.8804,108.731 23.3164,107.913C 27.4858,107.144 31.9644,106.476 36.0524,107.2C 43.8111,108.573 47.3511,117.387 43.0364,125.059C 40.4031,129.741 36.3004,133.615 32.7577,137.765C 30.5484,140.353 28.1537,142.784 25.4231,145.737 Z M 49.008,151.366C 51.9613,147.073 54.964,143.1 57.468,138.834C 58.2413,137.517 58.292,135.062 57.5747,133.714C 55.1853,129.221 52.2973,124.994 49.5413,120.572C 60.688,116.809 59.6987,127.68 64.592,132.185C 65.928,129.898 67.5387,127.832 68.4187,125.49C 70.2507,120.616 73.4133,118.78 79.0574,120.454C 76.64,124.197 74.58,128.148 71.7934,131.496C 68.8893,134.985 69.1947,137.656 71.9173,141C 74.5,144.173 76.508,147.816 79.2347,151.961C 75.3,151.406 71.6667,153.798 69.44,149.397C 67.9453,146.442 66.1827,143.624 64.348,140.406C 59.3627,144.429 59.684,155.188 49.008,151.366 Z";
        string _dragDropVectorPath = "F1 M 746.949,401.03L 326.356,401.03L 326.356,664.663L 746.949,664.663L 746.949,401.03 Z M 314.03,677.689L 314.03,671.441C 314.031,579.288 314.066,487.133 313.946,394.98C 313.939,390.193 315.293,388.704 320.166,388.708C 445.641,388.82 571.119,388.784 696.594,388.781C 715.423,388.78 734.254,388.897 753.083,388.752C 756.926,388.721 758.201,389.72 758.195,393.791C 758.078,486.945 758.085,580.099 758.181,673.252C 758.185,676.993 757.13,678.033 753.395,678.032C 608.249,677.942 463.103,677.956 317.957,677.948C 316.967,677.948 315.977,677.821 314.03,677.689 Z M 437.364,334.804L 427.875,334.808C 422.84,334.849 423.212,331.332 423.063,327.878C 422.908,324.258 424.213,322.728 427.96,322.846C 434.113,323.041 440.287,323.096 446.435,322.829C 450.912,322.636 451.184,325.14 451.105,328.516C 451.027,331.874 451.169,335.064 446.352,334.832C 443.363,334.688 440.36,334.805 437.364,334.804 Z M 626.759,334.787C 623.769,334.793 620.768,334.625 617.793,334.831C 612.904,335.168 613.087,331.973 613.052,328.635C 613.017,325.408 612.968,322.651 617.568,322.835C 623.869,323.087 630.191,323.016 636.496,322.853C 640.172,322.759 641.073,324.643 640.951,327.811C 640.828,330.959 641.444,334.541 636.721,334.752C 633.407,334.9 630.08,334.78 626.759,334.787 Z M 714.56,334.814C 712.46,334.345 708.089,336.24 708.021,330.352C 707.938,323.16 707.941,322.894 715.36,322.89C 728.144,322.884 728.144,322.884 728.101,335.845C 728.08,342.261 724.534,345.064 718.637,342.461C 716.821,341.66 716.22,338.105 714.56,334.814 Z M 295.243,322.887C 297.737,322.882 300.253,323.084 302.723,322.835C 306.867,322.418 308.831,323.664 308.736,328.252C 308.644,332.703 307.572,335.171 302.499,334.863C 297.196,334.543 291.84,334.516 286.541,334.87C 281.389,335.214 281.076,332.423 280.927,328.406C 280.757,323.887 282.497,322.438 286.765,322.843C 289.567,323.108 292.415,322.892 295.243,322.887 Z M 342.197,322.896L 349.652,322.896C 355.434,322.911 358.496,328.028 355.56,333.054C 355.033,333.956 353.449,334.699 352.333,334.727C 345.876,334.886 339.413,334.826 332.952,334.803C 327.896,334.786 328.467,331.09 328.296,327.758C 328.112,324.152 329.544,322.575 333.255,322.852C 336.22,323.074 339.216,322.896 342.197,322.896 Z M 531.653,334.764L 524.673,334.761C 518.736,334.714 515.765,330.202 518.383,324.837C 518.867,323.844 520.725,323.018 521.973,322.985C 528.616,322.805 535.267,322.989 541.912,322.866C 545.332,322.804 546.235,324.554 546.064,327.544C 545.891,330.59 546.912,334.362 542.119,334.678C 538.644,334.908 535.143,334.724 531.653,334.724L 531.653,334.764 Z M 389.29,334.908C 386.802,334.909 384.301,334.741 381.83,334.948C 377.991,335.269 375.572,334.791 375.898,329.819C 376.175,325.619 375.97,322.313 381.825,322.784C 387.593,323.248 393.435,323.097 399.227,322.833C 403.789,322.625 403.506,325.568 403.231,328.383C 402.967,331.091 404.791,335.256 399.233,334.941C 395.929,334.753 392.605,334.907 389.29,334.908 Z M 253.032,530.805C 253.03,534.282 252.957,537.762 253.048,541.238C 253.166,545.777 250.092,545.584 246.968,545.606C 243.945,545.628 241.864,545.093 241.976,541.265C 242.181,534.316 242.152,527.353 241.992,520.401C 241.894,516.209 244.432,516.04 247.52,516.045C 250.588,516.05 253.253,516.162 253.065,520.373C 252.909,523.844 253.033,527.328 253.032,530.805 Z M 674.29,322.887L 681.254,322.887C 687.848,322.876 690.033,325.759 687.805,332.165C 687.389,333.363 685.197,334.607 683.772,334.671C 677.651,334.945 671.509,334.729 665.375,334.804C 659.694,334.873 660.995,330.661 660.694,327.4C 660.378,323.977 661.87,322.58 665.341,322.84C 668.309,323.063 671.306,322.885 674.29,322.887 Z M 483.977,334.793L 477.508,334.794C 471.711,334.784 468.543,329.568 471.463,324.562C 471.979,323.677 473.599,323.003 474.727,322.977C 481.192,322.826 487.665,322.976 494.133,322.876C 499.032,322.801 497.98,326.328 498.14,329.156C 498.312,332.198 498.147,334.946 493.929,334.812C 490.616,334.706 487.295,334.79 483.977,334.793 Z M 579.787,322.896L 586.256,322.891C 592.822,322.917 595.019,325.832 592.878,332.203C 592.496,333.339 590.556,334.627 589.29,334.676C 582.83,334.925 576.354,334.815 569.884,334.783C 565.126,334.759 566.084,331.183 565.982,328.325C 565.88,325.561 565.791,322.797 569.834,322.888C 573.15,322.961 576.47,322.903 579.787,322.896 Z M 242.034,364.469C 253.034,362.901 253.035,362.901 253.036,372.934C 253.038,378.242 252.918,383.554 253.067,388.858C 253.201,393.611 249.959,393.013 246.962,393.098C 244.054,393.179 241.824,393.058 241.956,389.001C 242.218,380.91 242.034,372.806 242.034,364.469 Z M 242.033,581.124L 242.033,573.168C 242.037,567.524 246.449,564.667 251.563,567.205C 252.383,567.612 252.95,569.309 252.969,570.421C 253.094,577.547 252.922,584.676 253.07,591.8C 253.178,597.039 249.47,595.739 246.509,596.039C 243.066,596.387 241.698,595.107 241.974,591.561C 242.243,588.101 242.033,584.604 242.033,581.124 Z M 242.233,443.921L 242.233,414.926C 244.781,414.926 247.301,414.482 249.534,415.089C 250.924,415.465 252.816,417.389 252.876,418.691C 253.202,425.817 252.989,432.963 253.053,440.102C 253.077,442.861 251.85,444.041 249.134,443.937C 247.012,443.855 244.884,443.921 242.233,443.921 Z M 242.056,494.914C 242.056,485.59 241.945,476.985 242.107,468.384C 242.179,464.55 245.428,465.996 247.444,465.865C 249.833,465.712 252.915,465.196 252.979,469.01C 253.105,476.653 253.136,484.302 252.87,491.937C 252.836,492.964 250.993,494.524 249.764,494.782C 247.555,495.249 245.177,494.914 242.056,494.914 Z M 253.328,335.13C 252.534,338.491 254.468,342.962 248.52,342.883C 242.62,342.806 242.053,342.517 242.033,336.458C 241.986,322.842 241.988,322.905 254.842,322.85C 258.504,322.834 261.344,322.799 261.174,327.938C 261.029,332.378 260.658,335.669 255.038,334.855C 254.585,334.789 254.09,334.999 253.328,335.13 Z M 254.174,625.845C 256.739,626.415 261.367,624.362 261.136,630.673C 260.884,637.522 260.916,637.891 253.908,637.841C 240.224,637.743 242.186,639.521 242.034,626.013L 242.032,624.027C 242.039,618.439 246.356,615.459 251.106,618.37C 252.772,619.393 252.996,622.771 254.174,625.845 Z M 729.148,365.598C 728.104,373.53 726.997,374.738 719.9,373.642C 718.467,373.422 717,371.266 716.219,369.674C 715.627,368.465 716.099,366.733 716.1,365.234C 716.103,358.059 718.665,355.766 725.403,358.25C 727.296,358.947 727.935,363.055 729.148,365.598 Z M 283.223,625.928C 286.579,626.336 291.497,624.108 291.128,631.12C 290.793,637.5 290.835,637.81 284.427,637.824L 282.443,637.824C 275.651,637.806 275.431,637.291 274.964,631.65C 274.472,625.707 277.901,625.591 282.065,625.924L 283.223,625.928 Z M 534.074,620.034C 534.074,602.904 534.336,585.766 533.926,568.646C 533.79,562.958 536.336,562.716 540.72,562.569C 545.548,562.407 546.159,564.716 546.13,568.66C 546.004,585.787 546.076,602.918 546.076,621.513C 549.99,617.622 553.228,614.706 556.103,611.468C 559.628,607.494 562.004,609.927 564.738,612.43C 567.696,615.138 567.864,617.449 564.871,620.341C 557.93,627.049 551.138,633.916 544.372,640.804C 541.46,643.769 538.822,643.889 535.863,640.884C 529.019,633.93 522.203,626.942 515.132,620.224C 511.632,616.898 513.531,614.905 516.032,612.5C 518.52,610.109 520.652,607.407 524.052,611.454C 526.803,614.725 529.883,617.717 532.816,620.833L 534.074,620.034 Z M 456.382,530.299L 471.65,530.3C 483.807,530.319 495.966,530.252 508.12,530.46C 510.016,530.494 513.118,531.38 513.52,532.586C 514.363,535.112 514.066,538.167 513.571,540.883C 513.454,541.524 510.704,542.012 509.163,542.019C 494.34,542.088 479.519,542.038 464.696,542.023L 455.223,542.023C 458.986,545.768 461.68,548.723 464.663,551.351C 468.3,554.556 467.311,557.066 464.286,560.044C 461.376,562.91 459.034,564.292 455.588,560.672C 449.09,553.844 442.426,547.164 435.582,540.684C 432.315,537.591 432.674,535.4 435.73,532.475C 442.466,526.028 449.064,519.427 455.494,512.675C 458.63,509.382 461.112,508.82 464.65,512.246C 468.194,515.679 467.303,518.15 464.135,520.899C 461.246,523.406 458.45,526.022 455.612,528.588L 456.382,530.299 Z M 532.644,451.615L 523.925,461.176C 520.455,464.932 518.253,462.08 515.684,459.763C 513.047,457.384 512.589,455.483 515.36,452.801C 522.545,445.847 529.613,438.768 536.576,431.589C 539.095,428.993 541.152,428.803 543.715,431.408C 550.72,438.529 557.765,445.613 564.909,452.596C 568.203,455.816 566.447,457.915 563.919,460.397C 561.3,462.967 558.951,464.425 556.023,460.989L 547.699,451.308L 546.076,452.385L 546.076,465.16C 546.076,478.161 545.82,491.169 546.204,504.159C 546.355,509.261 544.516,510.208 539.936,510.124C 535.652,510.044 533.832,509.316 533.943,504.387C 534.296,488.727 534.073,473.053 534.073,457.385L 534.073,452.505L 532.644,451.615 Z M 624.197,542.027L 572.913,542.027C 566.531,542.027 563.882,538.511 566.349,532.646C 566.859,531.431 569.522,530.542 571.216,530.502C 580.363,530.29 589.517,530.391 598.667,530.389L 625.414,530.387C 621.443,526.335 618.511,523.059 615.274,520.117C 612.147,517.275 613.06,515.193 615.743,512.758C 618.201,510.529 620.197,508.115 623.578,511.589C 630.655,518.862 637.882,525.993 645.098,533.13C 647.383,535.39 647.858,537.395 645.307,539.889C 638.17,546.869 631.095,553.915 624.127,561.065C 620.293,564.999 618.146,561.871 615.51,559.518C 612.639,556.954 612.406,554.745 615.461,552.155C 618.751,549.366 621.857,546.357 625.042,543.443L 624.197,542.027 Z";

        #endregion

        #region Constructor

        //TODO : remove when the clickselector is transformed into a clickselectorprovider
        public ClickSelector Holder { get; set; }

        public ClicksVM(ClickSelector holder)
        {
            Holder = holder;
            InitializeDefaultClicks();
            InitializeSharedData();
            Holder.SharedData.SharedPropertyChanged += OnSharedPropertyChanged;
        }

        public ClicksVM(ClickSelector holder, IEnumerable<ClickEmbedderVM> clickEmbeddersVM )
        {
            Holder = holder;

            foreach( ClickEmbedderVM clickEmbedderVM in clickEmbeddersVM )
            {
                Add( clickEmbedderVM );
            }
            _clicksVmReadOnlyAdapter = new CKReadOnlyCollectionOnICollection<ClickEmbedderVM>( this );

            InitializeSharedData();
            Holder.SharedData.SharedPropertyChanged += OnSharedPropertyChanged;

            this.First().ChangeSelectionCommand.Execute( this );
            OnPropertyChanged( "NextClick" );
        }

        #endregion

        #region ISharedData

        double _clickSelectorOpacity;
        public double ClickSelectorOpacity
        {
            get { return _clickSelectorOpacity; }
            set
            {
                if( value != _clickSelectorOpacity )
                {
                    _clickSelectorOpacity = value;
                    OnPropertyChanged( "ClickSelectorOpacity" );
                }
            }
        }

        Color _clickSelectorBackgroundColor;
        public Color ClickSelectorBackgroundColor
        {
            get { return _clickSelectorBackgroundColor; }
            set
            {
                if( value != _clickSelectorBackgroundColor )
                {
                    _clickSelectorBackgroundColor = value;
                    OnPropertyChanged( "ClickSelectorBackgroundColor" );
                }
            }
        }

        int _clickSelectorBorderThickness;
        public int ClickSelectorBorderThickness
        {
            get { return _clickSelectorBorderThickness; }
            set
            {
                if( value != _clickSelectorBorderThickness )
                {
                    _clickSelectorBorderThickness = value;
                    OnPropertyChanged( "ClickSelectorBorderThickness" );
                }
            }
        }

        private Brush _clickSelectorBorderBrush;
        public Brush ClickSelectorBorderBrush
        {
            get { return _clickSelectorBorderBrush; }
            set
            {
                if( value != _clickSelectorBorderBrush )
                {
                    _clickSelectorBorderBrush = value;
                    OnPropertyChanged( "ClickSelectorBorderBrush" );
                }
            }
        }

        void OnSharedPropertyChanged( object sender, SharedPropertyChangedEventArgs e )
        {
            switch( e.PropertyName )
            {
                case "WindowOpacity":
                    ClickSelectorOpacity = Holder.SharedData.WindowOpacity;
                    break;
                case "WindowBorderThickness":
                    ClickSelectorBorderThickness = Holder.SharedData.WindowBorderThickness;
                    break;
                case "WindowBorderBrush":
                    ClickSelectorBorderBrush = new SolidColorBrush( Holder.SharedData.WindowBorderBrush );
                    break;
                case "WindowBackgroundColor":
                    ClickSelectorBackgroundColor = Holder.SharedData.WindowBackgroundColor;
                    break;
            }
        }

        void InitializeSharedData()
        {
            ClickSelectorOpacity = Holder.SharedData.WindowOpacity;
            ClickSelectorBackgroundColor = Holder.SharedData.WindowBackgroundColor;
            ClickSelectorBorderBrush = new SolidColorBrush( Holder.SharedData.WindowBorderBrush );
            ClickSelectorBorderThickness = Holder.SharedData.WindowBorderThickness;
        }

        #endregion

        #region Methods

        private void InitializeDefaultClicks()
        {
            ClickEmbedderVM clickEmbedderVM;
            IList<ClickVM> _clickList = new List<ClickVM>();

            clickEmbedderVM = new ClickEmbedderVM( this, R.LeftClick, "/Res/Images/LeftClick.png", _leftClickVectorPath, _clickList );
            clickEmbedderVM.Add( new ClickVM( clickEmbedderVM, "Clic Gauche", new List<ClickInstruction>()
                { 
                    ClickInstruction.LeftButtonDown, ClickInstruction.LeftButtonUp
                } ) );
            Add( clickEmbedderVM );
            _clickList.Clear();

            clickEmbedderVM = new ClickEmbedderVM( this, R.RightClick, "/Res/Images/RightClick.png", _rightClickVectorPath, _clickList );
            clickEmbedderVM.Add( new ClickVM( clickEmbedderVM, "Clic Droit", new List<ClickInstruction>() 
                { 
                   ClickInstruction.RightButtonDown, ClickInstruction.RightButtonUp
                } ) );
            Add( clickEmbedderVM );
            _clickList.Clear();

            clickEmbedderVM = new ClickEmbedderVM( this, R.DoubleClick, "/Res/Images/DoubleLeftClick.png",_dbClickVectorPath, _clickList );
            clickEmbedderVM.Add( new ClickVM( clickEmbedderVM, "Double Clic", new List<ClickInstruction>() 
                { 
                    ClickInstruction.LeftButtonDown, ClickInstruction.LeftButtonUp, ClickInstruction.LeftButtonDown, ClickInstruction.LeftButtonUp
                } ) );
            Add( clickEmbedderVM );
            _clickList.Clear();

            clickEmbedderVM = new ClickEmbedderVM( this, R.DragDrop, "/Res/Images/DragDrop.png",_dragDropVectorPath, _clickList );
            clickEmbedderVM.Add( new ClickVM( clickEmbedderVM, R.LeftDown, new List<ClickInstruction>() 
                { 
                     ClickInstruction.LeftButtonDown
                } ) );
            clickEmbedderVM.Add( new ClickVM( clickEmbedderVM, R.LeftUp, new List<ClickInstruction>() 
                { 
                    ClickInstruction.LeftButtonUp
                } ) );
            Add( clickEmbedderVM );

            this.First().ChangeSelectionCommand.Execute( this );
            OnPropertyChanged( "NextClick" );
        }

        public void ChangeSelection( ClickEmbedderVM selectedClickEmbedder )
        {
            //If the previous selectedClickEmbedder is different from this one
            if( !selectedClickEmbedder.IsSelected )
            {
                _selectedClickEmbedderVM = selectedClickEmbedder;
                foreach( ClickEmbedderVM clickEmbedderVM in this )
                {
                    if( clickEmbedderVM == selectedClickEmbedder )
                        clickEmbedderVM.IsSelected = true;
                    else
                        clickEmbedderVM.IsSelected = false;
                }
            }
            else
            {
                selectedClickEmbedder.Index = 0;
            }
            OnPropertyChanged( "NextClick" );
        }

        public void Click()
        {
            Holder.AskClickType();
        }

        #endregion

        #region VMBase Methods
        static bool _throwException;

        /// <summary>
        /// Sets whether an exception is thrown, or if a Debug.Fail() is used
        /// when an invalid property name is passed to the <see cref="CheckPropertyName"/> method.
        /// The default value is false, but it might be set to true in unit test contexts.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void SetThrowOnInvalidPropertyName( bool throwException )
        {
            _throwException = throwException;
        }

        /// <summary>
        /// Warns the developer if this object does not have
        /// a public property with the specified name. This 
        /// method does not exist in a Release build.
        /// </summary>
        [Conditional( "DEBUG" )]
        [DebuggerStepThrough]
        public void CheckPropertyName( string propertyName )
        {
            // Verify that the property name matches a real,  
            // public, instance property on this object.
            if( TypeDescriptor.GetProperties( this )[propertyName] == null )
            {
                string msg = "Invalid property name: " + propertyName;
                if( _throwException ) throw new Exception( msg );
                Debug.Fail( msg );
            }
        }

        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public new event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        //TODO : new ?

        /// <summary>
        /// Raises this object's <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected virtual void OnPropertyChanged( string propertyName )
        {
            this.CheckPropertyName( propertyName );
            System.ComponentModel.PropertyChangedEventHandler handler = this.PropertyChanged;
            if( handler != null )
            {
                var e = new System.ComponentModel.PropertyChangedEventArgs( propertyName );
                handler( this, e );
            }
        }

        #endregion

        #region IHighlitableElement

        public Core.ICKReadOnlyList<IHighlightableElement> Children
        {
            get { return ReadOnlyClicksVM; }
        }

        public int X
        {
            get { return 0; }
        }

        public int Y
        {
            get { return 0; }
        }

        public int Width
        {
            get { return 0; }
        }

        public int Height
        {
            get { return 0; }
        }

        public SkippingBehavior Skip
        {
            get { return SkippingBehavior.EnterChildren; }
        }

        public ScrollingDirective BeginHighlight( BeginScrollingInfo beginScrollingInfo, ScrollingDirective scrollingDirective )
        {
            return scrollingDirective;
        }

        public ScrollingDirective EndHighlight( EndScrollingInfo endScrollingInfo, ScrollingDirective scrollingDirective )
        {
            return scrollingDirective;
        }

        public ScrollingDirective SelectElement( ScrollingDirective scrollingDirective )
        {
            return scrollingDirective;
        }

        public bool IsHighlightableTreeRoot
        {
            get { return false; }
        }

        #endregion
    }
}
