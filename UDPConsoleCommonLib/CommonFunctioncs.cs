using System;
public static class CommonFunctioncs
{
    private static Random random = new Random();

    public static int RandomRange(int minInclusive, int maxExclusive)
    {
        if(minInclusive > maxExclusive)
            throw new ArgumentException("Min value is greater than max value");

       return random.Next(minInclusive, maxExclusive);
    }
}