namespace Xperience.Xman.Steps
{
    /// <summary>
    /// A collection of <see cref="IStep"/>s which can be navigated forward and back.
    /// </summary>
    public class StepList : List<IStep>
    {
        private int _currentIndex = 0;


        /// <summary>
        /// The current step.
        /// </summary>
        public IStep Current => this[_currentIndex];


        /// <summary>
        /// Navigates forward if there is a next element.
        /// </summary>
        /// <returns><c>False</c> if there is no next element.</returns>
        public bool Next()
        {
            if (_currentIndex < Count - 1)
            {
                Step(1);
                return true;
            }
            
            
            return false;
        }


        /// <summary>
        /// Navigates backward if there is a previous element.
        /// </summary>
        /// <returns><c>False</c> if there is no previous element.</returns>
        public bool Previous()
        {
            if (_currentIndex > 0)
            {
                Step(-1);
                return true;
            }

            return false;
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
