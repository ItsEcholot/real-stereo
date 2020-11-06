﻿
using Emgu.CV.Aruco;
using System.Collections.Generic;
using System.Drawing;

namespace RealStereo
{
    public class Configuration
    {
        public Point Coordinates;
        public Dictionary<int, float[]> Volumes; // Float array index 0 contains the recording volume of the 100% audio playback volume, index 1 the 50% audio playback volume
    }
}