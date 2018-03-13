using System;

internal class Deterministic
{
    static int SEED = 423456789;
    static public Random random = new Random(SEED);
}