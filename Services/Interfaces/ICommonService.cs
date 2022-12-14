using HelperAPI.Modules.Implements;
using IronOcr;
using static HelperAPI.Modules.Implements.CommonModule;

namespace HelperAPI.Services.Interfaces
{
    public interface ICommonService
    {
        public Task<SignInReqeust> getVaildTest();
        public Task<string> ImageOcr(IFormFile formFile , bool IsGraphicalVerification);
        public Task<string> Decipher(DecipherRequest decipherRequest);
    }
}
