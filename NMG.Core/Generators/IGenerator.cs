namespace NMG.Core.Generators
{
    public interface IGenerator
    {
        void Generate(bool writeToFile = true);
    }
}