namespace Xperience.Xman.Steps
{
    public class StepList : List<Step>
    {
        private int _currentIndex = 0;


        public Step Current => this[_currentIndex];


        public bool HasNext()
        {
            return _currentIndex < Count - 1;
        }


        public bool HasPrevious()
        {
            return _currentIndex > 0;
        }


        public Step Next()
        {
            Step(1);
            
            return this[_currentIndex];
        }


        public Step Previous()
        {
            Step(-1);

            return this[_currentIndex];
        }

        
        private void Step(int direction)
        {
            _currentIndex += direction;
            if (_currentIndex > Count - 1)
            {
                _currentIndex = Count - 1;
            }

            if (_currentIndex < 0)
            {
                _currentIndex = 0;
            }
        }
    }
}
