using System;

// https://stackoverflow.com/a/74051720
public class UnorderedPair<T> : IEquatable<UnorderedPair<T>>
{
    public T X;
    public T Y;

    public UnorderedPair()
    {

    }

    public UnorderedPair(T x, T y)
    {
        X = x;
        Y = y;
    }

    public bool Equals(UnorderedPair<T> other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        // For equality simply include the swapped check
        return X.Equals(other.X) && Y.Equals(other.Y) || X.Equals(other.Y) && Y.Equals(other.X);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((UnorderedPair<T>)obj);
    }

    public override int GetHashCode()
    {
        // and for the HashCode (used as key in HashSet and Dictionary) simply order them by size an hash them again ^^
        var hashX = X == null ? 0 : X.GetHashCode();
        var hashY = Y == null ? 0 : Y.GetHashCode();
        return HashCode.Combine(Math.Min(hashX, hashY), Math.Max(hashX, hashY));
    }

    public static bool operator ==(UnorderedPair<T> left, UnorderedPair<T> right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(UnorderedPair<T> left, UnorderedPair<T> right)
    {
        return !Equals(left, right);
    }
}
