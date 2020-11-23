﻿using System;
using System.Collections.Generic;

namespace RealStereo
{
    public class VolumeInterpolation
    {
        private static int ORIGIN_SIZE = 500;
        private static int TARGET_SIZE = 100;
        private static int POWER = 2;

        public double[,,,] Values;

        public VolumeInterpolation(List<PointConfiguration> points)
        {
            Values = new double[TARGET_SIZE + 1, TARGET_SIZE + 1, points[0].Volumes.Count, 2];
            CalculateValues(points);
        }

        public int GetScale()
        {
            return ORIGIN_SIZE / TARGET_SIZE;
        }

        public int MapCoordinate(int x)
        {
            return x / (ORIGIN_SIZE / TARGET_SIZE);
        }

        private void CalculateValues(List<PointConfiguration> points)
        {
            // interpolate values using Shepard's method for scattered data
            for (int x = 0; x <= TARGET_SIZE; x++)
            {
                for (int y = 0; y <= TARGET_SIZE; y++)
                {
                    for (int speaker = 0; speaker < points[0].Volumes.Count; speaker++)
                    {
                        double totalWeight = 0;
                        double totalFullVolume = 0;
                        double totalHalfVolume = 0;
                        bool valueSet = false;

                        foreach (PointConfiguration point in points)
                        {
                            int pointX = MapCoordinate(point.Coordinates.X);
                            int pointY = MapCoordinate(point.Coordinates.Y);

                            if (pointX == x && pointY == y)
                            {
                                Values[x, y, speaker, 0] = point.Volumes[speaker][0];
                                Values[x, y, speaker, 1] = (Values[x, y, speaker, 0] - point.Volumes[speaker][1]) / ((int) (point.Volumes[speaker][2] * 100) - (int) (point.Volumes[speaker][2] * 100) / 2);
                                valueSet = true;
                                break;
                            }

                            int distanceX = pointX - x;
                            int distanceY = pointY - y;
                            double weight = Math.Pow(Math.Sqrt(distanceX * distanceX + distanceY * distanceY), POWER);
                            weight = 1 / weight;
                            totalFullVolume += weight * point.Volumes[speaker][0];
                            totalHalfVolume += weight * point.Volumes[speaker][1];
                            totalWeight += weight;
                        }

                        if (!valueSet)
                        {
                            // store volume level at index 0
                            Values[x, y, speaker, 0] = totalFullVolume / totalWeight;

                            // store the difference that 1% change of the speaker volume does
                            Values[x, y, speaker, 1] = (Values[x, y, speaker, 0] - (totalHalfVolume / totalWeight)) / ((int) (points[0].Volumes[speaker][2] * 100) - (int) (points[0].Volumes[speaker][2] * 100) / 2);
                        }
                    }
                }
            }
        }
    }
}
