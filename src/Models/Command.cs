namespace Xperience.Xman.Models
{
    public abstract class Command
    {
        public IEnumerable<string> Keywords { get; private set; }


        public string Description { get; private set; }


        protected Command(IEnumerable<string> keyWords, string description)
        {
            Keywords = keyWords;
            Description = description;
        }


        public abstract void Execute();
    }
}