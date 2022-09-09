namespace HelperAPI.Modules.Interfaces
{
    public interface IBaseModule
    {
        /// <summary>
        /// 新增路由
        /// </summary>
        /// <param name="app"></param>
        public void AddModuleRoutes (IEndpointRouteBuilder app);

    }
}
