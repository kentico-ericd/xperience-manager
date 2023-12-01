using Xperience.Xman.Options;
using Xperience.Xman.Steps;

namespace Xperience.Xman.Wizards
{
    /// <summary>
    /// A configuration wizard which displays steps for user input and populates an <see cref="IWizardOptions"/>
    /// with the provided data.
    /// </summary>
    public abstract class AbstractWizard<TOptions> where TOptions : IWizardOptions, new()
    {
        private readonly TOptions options = new();
        private readonly StepList steps = new();


        /// <summary>
        /// A list of <see cref="Step{T}"/>s to display when <see cref="Run"/> is called.
        /// </summary>
        public StepList Steps => steps;


        /// <summary>
        /// The options to populate after <see cref="Run"/> is called.
        /// </summary>
        public TOptions Options => options;


        protected AbstractWizard()
        {
        }


        /// <summary>
        /// Initializes the <see cref="Steps"/> with the <see cref="Step{T}"/>s required to
        /// populate the <see cref="Options"/>.
        /// </summary>
        protected abstract void InitSteps();


        /// <summary>
        /// Requests user input to generate the <see cref="TOptions"/>.
        /// </summary>
        public TOptions Run()
        {
            do
            {
                steps.Current.Execute();
            } while (steps.Next());

            return options;
        }
    }
}
