﻿using RealStereo.Config;
using System;
using System.Collections.Generic;

namespace RealStereo.Balancing.Speaker
{
    public class VolumeInterpolation
    {
        public double[,,,] Values;

        private static readonly int OriginSize = 500;
        private static readonly int TargetSize = 100;
        private static readonly double Power = 1.5;

        private float targetVolume;
        private List<PointConfiguration> points;

        public VolumeInterpolation(List<PointConfiguration> points)
        {
            this.points = points;
            Values = new double[TargetSize + 1, TargetSize + 1, points[0].Volumes.Count, 2];
            targetVolume = CalculateMaxVolume(points);
            CalculateValues(points);
        }

        /// <summary>
        /// Get the scaling factor utilized for the interpolation.
        /// For performance reasons, the 500x500 coordinate system gets scaled down for the interpolation.
        /// </summary>
        /// <returns>Scaling factor.</returns>
        public int GetScale()
        {
            return OriginSize / TargetSize;
        }

        /// <summary>
        /// Maps an original coordinate to the scaled-down coordinate system of the interpolationn.
        /// </summary>
        /// <param name="x">Original coordinate.</param>
        /// <returns>Scaled coordinate.</returns>
        public int MapCoordinate(int x)
        {
            return x / (OriginSize / TargetSize);
        }

        /// <summary>
        /// Gets the maximum recorded volume during configuration.
        /// </summary>
        /// <param name="points">Configuration points that should be used for calculation.</param>
        /// <returns>Maximum recorded volume.</returns>
        private float CalculateMaxVolume(List<PointConfiguration> points)
        {
            float maxVolume = 0;
            foreach (PointConfiguration pointConfiguration in points)
            {
                for (int speaker = 0; speaker < pointConfiguration.Volumes.Count; speaker++)
                {
                    if (pointConfiguration.Volumes[speaker][0] > maxVolume)
                    {
                        maxVolume = pointConfiguration.Volumes[speaker][0];
                    }
                }
            }

            return maxVolume;
        }

        /// <summary>
        /// Calculates all possible values for the coordinate system using interpolation.
        /// For interpolation, Shepard's method for scattered data is used.
        /// </summary>
        /// <param name="points">Configuration points used for interpolation.</param>
        private void CalculateValues(List<PointConfiguration> points)
        {
            // interpolate values using Shepard's method for scattered data
            for (int x = 0; x <= TargetSize; x++)
            {
                for (int y = 0; y <= TargetSize; y++)
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

                            // calculate the distance of the configuration point to the current coordinate
                            int distanceX = pointX - x;
                            int distanceY = pointY - y;

                            // calculate the weight of the configuration point based on the distance
                            double weight = Math.Pow(Math.Sqrt(distanceX * distanceX + distanceY * distanceY), Power);
                            weight = 1 / weight;

                            // add the weighted volumes
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

        /// <summary>
        /// Calculates the volume that the given speaker should have depending on the current position.
        /// </summary>
        /// <param name="x">X-Coordinate of the person.</param>
        /// <param name="y">Y-Coordinate of the person.</param>
        /// <param name="speakerIndex">Speaker that the volume should be calculated for.</param>
        /// <returns>Volume that the speaker should have.</returns>
        public double GetVolumeForPositionAndSpeaker(int x, int y, int speakerIndex)
        {
            int mappedX = MapCoordinate(x);
            int mappedY = MapCoordinate(y);
            double volumeDifference = targetVolume - Values[mappedX, mappedY, speakerIndex, 0];
            double volume = points[0].Volumes[speakerIndex][2] + (volumeDifference / Values[mappedX, mappedY, speakerIndex, 1]) / 100;
            return Math.Max(Math.Min(volume, 1.0), 0.0);
        } 
    }
}
