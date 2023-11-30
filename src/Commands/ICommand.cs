namespace Xperience.Xman.Commands
{
    public interface ICommand
    {
        public IEnumerable<string> Keywords { get; }


        public string Description { get; }


        public void Execute();
    }
}