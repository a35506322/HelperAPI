using static HelperAPI.Modules.Implements.CommonModule;

namespace HelperAPI.Services.Interfaces
{
    public interface ICommonService
    {
        public Task<SignInReqeust> getVaildTest();
    }
}
