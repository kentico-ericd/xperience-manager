using Xperience.Xman.Options;

namespace Xperience.Xman.Services
{
    public interface ICDXmlManager : IService
    {
        public Task<RepositoryConfiguration?> GetConfig(string path);


        public void WriteConfig(RepositoryConfiguration config, string path);
    }
}
