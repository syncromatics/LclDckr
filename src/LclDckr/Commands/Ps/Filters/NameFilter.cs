namespace LclDckr.Commands.Ps.Filters
{
    public class NameFilter : IFilter
    {
        public string Name { get; set; }
        public string Value => $"name={Name}";

        public NameFilter()
        {
            
        }

        public NameFilter(string name)
        {
            Name = name;
        }
    }
}
