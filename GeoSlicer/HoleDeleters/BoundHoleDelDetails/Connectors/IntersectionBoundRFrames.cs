﻿using System;
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
    internal bool BruteforceConnect(
        LinkedListNode<BoundingRing> thisRing,
        LinkedList<BoundingRing> listOfHoles,
        Cache cache)
    {
        _intersectFrameNode = cache.IntersectFrames.First;
        while (_intersectFrameNode is not null)
        {
            _intersectFrame = _intersectFrameNode.Value;
            _currentPointThisRing = thisRing.Value.Ring;
            _currentPointIntersectFrame = _intersectFrame.Value.Ring;
            _findCurrentPointThisRing = false;
            _findCurrentPointIntersectFrame = false;
            
            IntersectsChecker.GetIntersectionBoundRFrames(
                thisRing.Value, 
                _intersectFrame.Value,
                out _framesIntersectionPointMin,
                out _framesIntersectionPointMax);
            
            if (ConnectThisRWithIntersectFrameR(thisRing, listOfHoles, cache))
                return true;

            _intersectFrameNode = _intersectFrameNode.Next;
        }
        
        return false;
    }

    private bool ConnectThisRWithIntersectFrameR(
        LinkedListNode<BoundingRing> thisRing,
        LinkedList<BoundingRing> listOfHoles,
        Cache cache)
    {
        do
        {
            FindLineInFramesIntersection(thisRing);
            CheckLineIntersectThisRingOrFrameRing(thisRing);
            CheckLineIntersectRingFromList(cache.IntersectFrames);
            CheckLineIntersectRingFromList(cache.FramesContainThis);

            if (_findCurrentPointThisRing && _findCurrentPointIntersectFrame)
            {
                thisRing.Value.ConnectBoundRings(
                    _intersectFrame!.Value,
                    _currentPointThisRing!, _currentPointIntersectFrame!);
                listOfHoles.Remove(_intersectFrame);
                return true;
            } 
            
        } while (!ReferenceEquals(_currentPointThisRing, thisRing.Value.Ring) &&
                 !ReferenceEquals(_currentPointIntersectFrame, _intersectFrame!.Value.Ring));

        return false;
    }
    
    // Меняет ссылки _currentPointThisRing, _currentPointIntersectFrame так, что линия
    // (_currentPointThisRing, _currentPointIntersectFrame) находится внутри рамки
    // (_framesIntersectionPointMin, _framesIntersectionPointMax). Эта линия, возможно, 
    // может соединить кольцо thisRing с кольцом из рамки (_framesIntersectionPointMin, _framesIntersectionPointMax).
    // Метод устанавливает флаги _findCurrentPointThisRing, _findCurrentPointIntersectFrame в true,
    // если найдены корректные точки.
    
    private void FindLineInFramesIntersection(LinkedListNode<BoundingRing> thisRing)
    {
        if (!_findCurrentPointThisRing)
        {
            do
            {
                if (IntersectsChecker.PointInsideFrameCheck(
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
                if (IntersectsChecker.PointInsideFrameCheck(
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

    // Проверяет, пересекает ли линия (_currentPointThisRing, _currentPointIntersectFrame)
    // кольца thisRing и _intersectFrame. В случае пересечений может поменять ссылки 
    // _currentPointThisRing, _currentPointIntersectFrame и установить флаги _findCurrentPointThisRing,
    // _findCurrentPointIntersectFrame в false.
    private void CheckLineIntersectThisRingOrFrameRing
        (LinkedListNode<BoundingRing> thisRing)
    {
        if (!_findCurrentPointThisRing || !_findCurrentPointIntersectFrame)
            return;
        
        if (IntersectsChecker.IntersectRingWithSegmentNotExtPoints(
                thisRing,
                _currentPointThisRing!.Elem, _currentPointIntersectFrame!.Elem))
        {
            _findCurrentPointThisRing = false;
            _currentPointThisRing = _currentPointThisRing.Next;
        }

        if (IntersectsChecker.IntersectRingWithSegmentNotExtPoints(
                _intersectFrame!,
                _currentPointThisRing.Elem, _currentPointIntersectFrame.Elem))
        {
            _findCurrentPointIntersectFrame = false;
            _currentPointIntersectFrame = _currentPointIntersectFrame.Next;
        }
    }

    // Проверяет, пересекает ли линия (_currentPointThisRing, _currentPointIntersectFrame)
    // кольца из list. В случае пересечений может поменять ссылки 
    // _currentPointThisRing, _currentPointIntersectFrame и установить флаги _findCurrentPointThisRing,
    // _findCurrentPointIntersectFrame в false.
    private void CheckLineIntersectRingFromList(LinkedList<LinkedListNode<BoundingRing>> list)
    {
        if (!_findCurrentPointThisRing || !_findCurrentPointIntersectFrame)
            return;
        
        foreach (var frame in list)
        {
            if (!ReferenceEquals(_intersectFrame, frame) &&
                IntersectsChecker.IntersectBoundRingWithLine(
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