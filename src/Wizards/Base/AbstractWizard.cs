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
        public StepList Steps { get; } = new();


        public TOptions Options { get; } = new();


        /// <summary>
        /// Initializes the <see cref="Steps"/> by calling <see cref="InitSteps"/>.
        /// </summary>
        protected AbstractWizard() => InitSteps();


        public abstract void InitSteps();


        public async Task<TOptions> Run()
        {
            do
            {
                await Steps.Current.Execute();
            } while (Steps.Next());

            return Options;
        }
    }
}
