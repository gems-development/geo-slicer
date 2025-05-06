using System.Collections.Generic;
using GeoSlicer.Utils.BoundRing;
using NetTopologySuite.Geometries;

namespace GeoSlicer.HoleDeleters.BoundHoleDelDetails.Connectors;

internal class IntersectionBoundRFrames
{
    private LinkedListNode<BoundingRing>? _intersectFrame;
    private LinkedNode<Coordinate>? _currentPointThisRing;
    private LinkedNode<Coordinate>? _currentPointIntersectFrame;
    private bool _findCurrentPointThisRing;
    private bool _findCurrentPointIntersectFrame;
    private Coordinate? _framesIntersectionPointMin;
    private Coordinate? _framesIntersectionPointMax;
    private LinkedListNode<LinkedListNode<BoundingRing>>? _intersectFrameNode;
    
    /// <summary>
    /// Метод пытается соединить <paramref name="thisRing"/> с каким-либо кольцом, рамка которого пересекается с
    /// рамкой кольца <paramref name="thisRing"/>.
    /// </summary>
    /// <returns>True, если соединение было успешно произведено.</returns>
    internal bool TryBruteforceConnect(
        LinkedListNode<BoundingRing> thisRing,
        LinkedList<BoundingRing> listOfHoles,
        Cache cache,
        IntersectsChecker intersectsChecker)
    {
        _intersectFrameNode = cache.IntersectFrames.First;
        while (_intersectFrameNode is not null)
        {
            _intersectFrame = _intersectFrameNode.Value;
            _currentPointThisRing = thisRing.Value.Ring;
            _currentPointIntersectFrame = _intersectFrame.Value.Ring;
            _findCurrentPointThisRing = false;
            _findCurrentPointIntersectFrame = false;
            
            intersectsChecker.GetIntersectionBoundRFrames(
                thisRing.Value, 
                _intersectFrame.Value,
                out _framesIntersectionPointMin,
                out _framesIntersectionPointMax);
            
            if (ConnectThisRWithIntersectFrameR(thisRing, listOfHoles, cache, intersectsChecker))
                return true;

            _intersectFrameNode = _intersectFrameNode.Next;
        }
        
        return false;
    }

    private bool ConnectThisRWithIntersectFrameR(
        LinkedListNode<BoundingRing> thisRing,
        LinkedList<BoundingRing> listOfHoles,
        Cache cache,  IntersectsChecker intersectsChecker)
    {
        do
        {
            FindLineInFramesIntersection(thisRing, intersectsChecker);
            CheckLineIntersectThisRingOrFrameRing(thisRing, intersectsChecker);
            CheckLineIntersectRingFromList(cache.IntersectFrames, intersectsChecker);
            CheckLineIntersectRingFromList(cache.FramesContainThis, intersectsChecker);

            if (_findCurrentPointThisRing && _findCurrentPointIntersectFrame)
            {
                Connector.Connect(
                    thisRing, _intersectFrame!,
                    _currentPointThisRing!, _currentPointIntersectFrame!,
                    listOfHoles);
                return true;
            } 
            
        } while (!ReferenceEquals(_currentPointThisRing, thisRing.Value.Ring) &&
                 !ReferenceEquals(_currentPointIntersectFrame, _intersectFrame!.Value.Ring));

        return false;
    }
    
    /// <summary>
    /// Меняет ссылки _currentPointThisRing, _currentPointIntersectFrame так, что линия
    /// (_currentPointThisRing, _currentPointIntersectFrame) находится внутри рамки
    /// (_framesIntersectionPointMin, _framesIntersectionPointMax). Эта линия, возможно, 
    /// может соединить кольцо <paramref name="thisRing"/> с кольцом из рамки (_framesIntersectionPointMin, _framesIntersectionPointMax).
    /// Метод устанавливает флаги _findCurrentPointThisRing, _findCurrentPointIntersectFrame в true,
    /// если найдены корректные точки.
    /// </summary>
    private void FindLineInFramesIntersection(LinkedListNode<BoundingRing> thisRing,  IntersectsChecker intersectsChecker)
    {
        if (!_findCurrentPointThisRing)
        {
            do
            {
                if (intersectsChecker.CheckPointInsideFrameCheck(
                        _currentPointThisRing!.Elem,
                        _framesIntersectionPointMin!, _framesIntersectionPointMax!))
                {
                    _findCurrentPointThisRing = true;
                    break;
                }
                _currentPointThisRing = _currentPointThisRing.Next;
            } while (!ReferenceEquals(_currentPointThisRing, thisRing.Value.Ring));
        }

        if (!_findCurrentPointIntersectFrame)
        {
            do
            {
                if (intersectsChecker.CheckPointInsideFrameCheck(
                        _currentPointIntersectFrame!.Elem,
                        _framesIntersectionPointMin!, _framesIntersectionPointMax!))
                {
                    _findCurrentPointIntersectFrame = true;
                    break;
                }
                _currentPointIntersectFrame = _currentPointIntersectFrame.Next;
            } while (!ReferenceEquals(_currentPointIntersectFrame, _intersectFrame!.Value.Ring));
        }
    }

    /// <summary>
    /// Проверяет, пересекает ли линия (_currentPointThisRing, _currentPointIntersectFrame)
    /// кольца <paramref name="thisRing"/> и _intersectFrame. В случае пересечений может поменять ссылки 
    /// _currentPointThisRing, _currentPointIntersectFrame и установить флаги _findCurrentPointThisRing,
    /// _findCurrentPointIntersectFrame в false.
    /// </summary>
    private void CheckLineIntersectThisRingOrFrameRing
        (LinkedListNode<BoundingRing> thisRing,  IntersectsChecker intersectsChecker)
    {
        if (!_findCurrentPointThisRing || !_findCurrentPointIntersectFrame)
            return;
        
        if (intersectsChecker.CheckIntersectsRingWithSegmentNotExtPoints(
                thisRing,
                _currentPointThisRing!.Elem, _currentPointIntersectFrame!.Elem))
        {
            _findCurrentPointThisRing = false;
            _currentPointThisRing = _currentPointThisRing.Next;
        }

        if (intersectsChecker.CheckIntersectsRingWithSegmentNotExtPoints(
                _intersectFrame!,
                _currentPointThisRing.Elem, _currentPointIntersectFrame.Elem))
        {
            _findCurrentPointIntersectFrame = false;
            _currentPointIntersectFrame = _currentPointIntersectFrame.Next;
        }
    }

    /// <summary>
    /// Проверяет, пересекает ли линия (_currentPointThisRing, _currentPointIntersectFrame)
    /// кольца из <paramref name="list"/>. В случае пересечений может поменять ссылки 
    /// _currentPointThisRing, _currentPointIntersectFrame и установить флаги _findCurrentPointThisRing,
    /// _findCurrentPointIntersectFrame в false.
    /// </summary>
    private void CheckLineIntersectRingFromList(LinkedList<LinkedListNode<BoundingRing>> list,  IntersectsChecker intersectsChecker)
    {
        if (!_findCurrentPointThisRing || !_findCurrentPointIntersectFrame)
            return;
        
        foreach (var frame in list)
        {
            if (!ReferenceEquals(_intersectFrame, frame) &&
                intersectsChecker.CheckIntersectsBoundRingWithLine(
                    frame, 
                    _currentPointThisRing!.Elem, _currentPointIntersectFrame!.Elem))
            { 
                _findCurrentPointThisRing = false;
                _findCurrentPointIntersectFrame = false;
                _currentPointThisRing = _currentPointThisRing.Next;
                _currentPointIntersectFrame = _currentPointIntersectFrame.Next;
                break;
            }
        }
    }
}