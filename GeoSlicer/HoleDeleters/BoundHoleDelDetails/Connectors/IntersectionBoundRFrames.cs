using System.Collections.Generic;
using GeoSlicer.Utils.BoundRing;

namespace GeoSlicer.HoleDeleters.BoundHoleDelDetails.Connectors;

internal static class IntersectionBoundRFrames
{
    internal static bool BruteforceConnect(
        LinkedListNode<BoundingRing> thisRing,
        LinkedList<BoundingRing> listOfHoles,
        Cache cache)
    {
        var currentFrameNode = cache.IntersectFrames.First;
        do
        {
            while (currentFrameNode is not null)
            {
                if (IntersectsChecker.IntersectOrContainFrames(currentFrameNode.Value.Value, thisRing.Value))
                {
                    break;
                }

                currentFrameNode = currentFrameNode.Next;
            }

            if (currentFrameNode is not null)
            {
                var currentFrame = currentFrameNode.Value;
                var startThisRing = thisRing.Value.Ring;
                var startCurrentFrame = currentFrame.Value.Ring;
                bool flagFirstCycle = false;
                bool flagSecondCycle = false;
                IntersectsChecker.GetIntersectionBoundRFrames(
                    thisRing.Value, 
                    currentFrame.Value,
                    out var framesIntersectionPointMin,
                    out var framesIntersectionPointMax);
                do
                {
                    if (!flagFirstCycle)
                    {
                        do
                        {
                            if (IntersectsChecker.PointInsideFrameCheck(
                                    startThisRing.Elem, framesIntersectionPointMin, framesIntersectionPointMax))
                            {
                                flagFirstCycle = true;
                                break;
                            }

                            startThisRing = startThisRing.Next;
                        } while (!ReferenceEquals(startThisRing, thisRing.Value.Ring));
                    }

                    if (!flagSecondCycle)
                    {
                        do
                        {
                            if (IntersectsChecker.PointInsideFrameCheck(
                                    startCurrentFrame.Elem, framesIntersectionPointMin, framesIntersectionPointMax))
                            {
                                flagSecondCycle = true;
                                break;
                            }

                            startCurrentFrame = startCurrentFrame.Next;
                        } while (!ReferenceEquals(startCurrentFrame, currentFrame.Value.Ring));
                    }

                    if (flagFirstCycle && flagSecondCycle)
                    {
                        if (IntersectsChecker.IntersectRingWithSegmentNotExtPoints(thisRing, startThisRing.Elem,
                                startCurrentFrame.Elem))
                        {
                            flagFirstCycle = false;
                            startThisRing = startThisRing.Next;
                        }

                        if (IntersectsChecker.IntersectRingWithSegmentNotExtPoints(currentFrameNode.Value,
                                startThisRing.Elem,
                                startCurrentFrame.Elem))
                        {
                            flagSecondCycle = false;
                            startCurrentFrame = startCurrentFrame.Next;
                        }

                        if (flagFirstCycle && flagSecondCycle)
                        {
                            foreach (var frame in cache.IntersectFrames)
                            {
                                if (!ReferenceEquals(currentFrameNode.Value, frame))
                                {
                                    if (IntersectsChecker.HasIntersectsBoundRFrame
                                            (frame.Value, startThisRing.Elem, startCurrentFrame.Elem)

                                        || IntersectsChecker.PointInsideBoundRFrame
                                            (startThisRing.Elem, frame.Value)

                                        || IntersectsChecker.PointInsideBoundRFrame
                                            (startCurrentFrame.Elem, frame.Value))
                                    {
                                        if (IntersectsChecker.IntersectBoundRingWithSegment(frame, startThisRing.Elem,
                                                startCurrentFrame.Elem))
                                        {
                                            flagFirstCycle = false;
                                            flagSecondCycle = false;
                                            startThisRing = startThisRing.Next;
                                            startCurrentFrame = startCurrentFrame.Next;
                                        }
                                    }
                                }
                            }
                        }

                        if (flagFirstCycle && flagSecondCycle)
                        {
                            foreach (var frame in cache.FramesContainThis)
                            {
                                if (IntersectsChecker.IntersectBoundRingWithSegment(frame, startThisRing.Elem,
                                        startCurrentFrame.Elem))
                                {
                                    flagFirstCycle = false;
                                    flagSecondCycle = false;
                                    startThisRing = startThisRing.Next;
                                    startCurrentFrame = startCurrentFrame.Next;
                                }
                            }

                            if (flagFirstCycle && flagSecondCycle)
                            {
                                thisRing.Value.ConnectBoundRings(
                                    currentFrame.Value,
                                    startThisRing,
                                    startCurrentFrame);

                                listOfHoles.Remove(currentFrame);
                                return true;
                            }
                        }
                    }
                } while (!ReferenceEquals(startThisRing, thisRing.Value.Ring) &&

                         !ReferenceEquals(startCurrentFrame, currentFrame.Value.Ring));

                currentFrameNode = currentFrameNode.Next;
            }
        } while (currentFrameNode is not null);

        return false;
    }
}