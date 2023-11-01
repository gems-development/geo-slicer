using System;
using System.Collections.Generic;

namespace GeoSlicer.Utils.BoundHoleDelDependency;

public class LinkedNode<T>
{
    public T Elem { get; set; }
    public LinkedNode<T> Next { get; set; }
    public LinkedNode<T> Previous { get; set; }

    public LinkedNode(T elem)
    {
        Next = this;
        Previous = this;
        Elem = elem;
    }
    public LinkedNode(T elem, LinkedNode<T> previous)
    {
        Previous = previous;
        Next = previous.Next;
        Elem = elem;
        previous.Next.Previous = this;
        previous.Next = this;
    }
    public LinkedNode(T elem, LinkedNode<T> previous, LinkedNode<T> next)
    {
        Elem = elem;
        Next = next;
        Previous = previous;
        next.Previous = this;
        previous.Next = this;
    }

    public void LeaveList()
    {
        if (ReferenceEquals(this, Next))
            throw new AggregateException("The list consists of one element");
        Previous.Next = Next;
        Next.Previous = Previous;
        Next = this;
        Previous = this;
    }

    protected bool Equals(LinkedNode<T> other)
    {
        if (EqualityComparer<T>.Default.Equals(Elem, other.Elem))
        {
            LinkedNode<T> otherNext = other.Next;
            LinkedNode<T> thisNext = this.Next;
            while (!ReferenceEquals(this, thisNext))
            {
                if (!EqualityComparer<T>.Default.Equals(thisNext.Elem, otherNext.Elem))
                    return false;
                otherNext = otherNext.Next;
                thisNext = thisNext.Next;
            }

            return ReferenceEquals(other, otherNext);
        }
        return false;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((LinkedNode<T>)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Elem, Next, Previous);
    }
}