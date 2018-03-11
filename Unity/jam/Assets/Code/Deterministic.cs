using System;

internal class Deterministic
{
    static int SEED = 23456789;
    static public Random random = new Random(SEED);
}