using Godot;

public class TeriaSimplex : OpenSimplexNoise
{
    public TeriaSimplex(int seed, int octaves, float period, float persistence)
    {
        Seed = seed;
        Octaves = octaves;
        Period = period;
        Persistence = persistence;
    }

    public TeriaSimplex()
    {
    }
}