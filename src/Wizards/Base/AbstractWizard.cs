using Xperience.Xman.Options;
using Xperience.Xman.Steps;

namespace Xperience.Xman.Wizards
{
    /// <summary>
    /// A configuration wizard which displays steps for user input and populates an <see cref="IWizardOptions"/>
    /// with the provided data.
    /// </summary>
    public abstract class AbstractWizard<TOptions> : IWizard<TOptions> where TOptions : IWizardOptions, new()
    {
        private readonly TOptions options = new();
        private readonly StepList steps = new();


        public StepList Steps => steps;


        public TOptions Options => options;


        /// <summary>
        /// Initializes the <see cref="Steps"/> by calling <see cref="InitSteps"/>.
        /// </summary>
        public AbstractWizard()
        {
            InitSteps();
        }


        public abstract void InitSteps();


        public async Task<TOptions> Run()
        {
            do
            {
                await steps.Current.Execute();
            } while (steps.Next());

            return options;
        }
    }
}
