using System;

internal class Deterministic
{
    static int SEED = 123456789;
    static public Random random = new Random(SEED);
}